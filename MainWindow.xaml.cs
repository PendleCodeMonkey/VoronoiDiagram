using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PendleCodeMonkey.VoronoiDiagram
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private int _cachedWidth = 0;
		private int _cachedHeight = 0;

		private readonly double _initialWindowHeight;
		private readonly double _initialWindowWidth;

		private bool switchThemeAndUpdatePalette = false;
		private int _cachedPaletteStep = 0;

		private readonly int _pointSelectionThreshold = 3;

		private readonly Random _rnd = new();

		private WriteableBitmap _bmp;
		private List<(System.Drawing.Point pt, int colour)> _points;
		private readonly DispatcherTimer _dispatcherTimer;
		private readonly DataModel _model;

		private int[]? _palette = null;

		private readonly int PaletteSize = 256;

		public static readonly RoutedCommand SaveButtonCmd = new();

		public MainWindow()
		{
			_bmp = new WriteableBitmap(
				(int)SystemParameters.PrimaryScreenWidth,
				(int)SystemParameters.PrimaryScreenHeight,
				96,
				96,
				PixelFormats.Bgr24,
				null);

			_points = new List<(System.Drawing.Point, int)>();

			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

			_model = new DataModel
			{
				DistanceMeasure = DistanceMeasurement.Euclidian,
				ShowPoints = true,
				NumberOfPoints = 100,
				ThemeNames = GradientColourTheme.ThemeNames(),
				SelectedThemeName = "pcm",
				ReverseColours = false,
				RandomizeColours = false,
				Anchor = PointAnchor.Centre,
				PaletteStep = 1,
				PointRendering = PointRenderingStyle.Enhanced,
				UseColourTheme = true
			};

			_cachedPaletteStep = _model.PaletteStep;

			InitializeComponent();

			_initialWindowHeight = Height;
			_initialWindowWidth = Width;

			DataContext = _model;
		}


		internal void Render()
		{
			if (_points == null)
			{
				return;
			}

			bool regenerated = false;
			int w = (int)ImageBorder.ActualWidth;
			int h = (int)ImageBorder.ActualHeight;

			// If width or height has changed then regenerate the image
			if (w != _cachedWidth || h != _cachedHeight)
			{
				// Size has changed so if we have a valid list of points then adjust them
				// so that the new diagram will exactly fit the new dimensions.
				if (_points.Count > 0 && _cachedWidth > 0 && _cachedHeight > 0)
				{
					double xRatio = (double)w / (double)_cachedWidth;
					double yRatio = (double)h / (double)_cachedHeight;

					for (int i = 0; i < _points.Count; i++)
					{
						var pt = new System.Drawing.Point((int)(_points[i].pt.X * xRatio), (int)(_points[i].pt.Y * yRatio));
						_points[i] = (pt, _points[i].colour);
					}
				}

				_cachedWidth = w;
				_cachedHeight = h;
				_bmp = new WriteableBitmap(
						w,
						h,
						96,
						96,
						PixelFormats.Bgr24,
						null);
				regenerated = true;
			}

			VoronoiGenerator.GenerateVoronoi(_bmp,
											_points,
											_model.DistanceMeasure,
											_model.ShowPoints ?? false,
											_model.PointRendering == PointRenderingStyle.Contrast);

			if (regenerated)
			{
				img.Source = _bmp;
			}
		}

		internal List<(System.Drawing.Point, int)> CreateRandomPointsWithThemedColours(int count)
		{
			if (_model.Anchor == PointAnchor.None)
			{
				_palette = GeneratePalette(!_model.UseColourTheme);

				int w = (int)ImageBorder.ActualWidth - 20;
				int h = (int)ImageBorder.ActualHeight - 20;
				List<(System.Drawing.Point, int)> points = new();
				for (int i = 0; i < count; i++)
				{
					System.Drawing.Point pt = new(_rnd.Next(w) + 10, _rnd.Next(h) + 10);
					int colour = SelectColourFromPalette(_model.RandomizeColours ? null : i);
					points.Add((pt, colour));
				}

				return points;
			}

			_points = CreateOrderedPointsWithThemedColours(count);
			return SwitchThemeAndUpdateColours();
		}

		internal List<(System.Drawing.Point, int)> CreateOrderedPointsWithThemedColours(int count)
		{
			_palette = GeneratePalette(!_model.UseColourTheme);

			int w = (int)ImageBorder.ActualWidth - 20;
			int h = (int)ImageBorder.ActualHeight - 20;
			(int xAnchor, int yAnchor) = _model.Anchor switch
			{
				PointAnchor.TopLeft => (10, 10),
				PointAnchor.TopRight => (w + 10, 10),
				PointAnchor.BottomLeft => (10, h + 10),
				PointAnchor.BottomRight => (w + 10, h + 10),
				_ => ((w / 2) + 10, (h / 2) + 10),
			};

			List<(System.Drawing.Point pt, double dist, int col)> points = new();
			for (int i = 0; i < count; i++)
			{
				// Randomly generate x and y coordinates
				int x = _rnd.Next(w) + 10;
				int y = _rnd.Next(h) + 10;
				// Calculate the distance of the point x,y from the centre of the image.
				int xd = Math.Abs(x - xAnchor);
				int yd = Math.Abs(y - yAnchor);
				var distance = Math.Sqrt((xd * xd) + (yd * yd));

				System.Drawing.Point pt = new(x, y);
				int colour = SelectColourFromPalette(_model.RandomizeColours ? null : (i * _model.PaletteStep));
				points.Add((pt, distance, colour));
			}

			// Order the points by the distance they are from the centre of the image.
			// Then select just the point and colour info for returning to the caller.
			var ordered = points.OrderBy(x => x.dist).Select(y => (y.pt, y.col)).ToList();
			return ordered;
		}

		internal List<(System.Drawing.Point, int)> SwitchThemeAndUpdateColours()
		{
			_palette = GeneratePalette(!_model.UseColourTheme);
			List<(System.Drawing.Point, int)> points = new();
			int index = 0;
			foreach (var point in _points)
			{
				int colour = SelectColourFromPalette(_model.RandomizeColours ? null : (index * _model.PaletteStep));
				points.Add((point.pt, colour));
				index++;
			}

			return points;
		}

		private int SelectColourFromPalette(int? index)
		{
			if (_palette != null)
			{
				return index.HasValue ? _palette[index.Value % PaletteSize] : _palette[_rnd.Next(PaletteSize)];
			}
			return 0;
		}

		private int[] GeneratePalette(bool random)
		{
			return random ? GenerateRandomPalette() :
							GradientColourTheme.GeneratePalette(_model.SelectedThemeName, PaletteSize, _model.ReverseColours);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Don't want to allow the window to be resized any smaller than the initial (default) size.
			SetValue(MinWidthProperty, _initialWindowWidth);
			SetValue(MinHeightProperty, _initialWindowHeight);

			_points = CreateRandomPointsWithThemedColours(_model.NumberOfPoints);
			Render();
		}

		private void GenerateButton_Click(object sender, RoutedEventArgs e)
		{
			_points = CreateRandomPointsWithThemedColours(_model.NumberOfPoints);
			Render();
		}

		private void ParentGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(ParentGrid);

			int colour = SelectColourFromPalette(_points.Count);
			_points.Add((new System.Drawing.Point((int)pos.X, (int)pos.Y), colour));

			Render();
		}

		private int[] GenerateRandomPalette()
		{
			int[] palette = new int[PaletteSize];
			for (int i = 0; i < PaletteSize; i++)
			{
				int colourR = _rnd.Next(256);
				int colourG = _rnd.Next(256);
				int colourB = _rnd.Next(256);
				palette[i] = colourR << 16 | colourG << 8 | colourB;
			}

			return palette;
		}

		private void ParentGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			// It only makes sense to allow the user to remove points by right-clicking on them when points are actually being shown.
			if (!(_model.ShowPoints ?? false))
			{
				return;
			}

			var pos = e.GetPosition(ParentGrid);

			// Check if right-click occurred in vicinity of one of the points, removing the point if it does.
			foreach (var point in _points)
			{
				if (Math.Abs(pos.X - point.pt.X) < _pointSelectionThreshold &&
					Math.Abs(pos.Y - point.pt.Y) < _pointSelectionThreshold)
				{
					_points.Remove(point);
					Render();
					break;
				}
			}
		}

		internal void DelayedRender()
		{
			_dispatcherTimer.IsEnabled = true;
			_dispatcherTimer.Stop();
			_dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
			_dispatcherTimer.Start();
		}

		private void DispatcherTimer_Tick(object? sender, EventArgs e)
		{
			_dispatcherTimer.IsEnabled = false;
			if (switchThemeAndUpdatePalette)
			{
				_points = SwitchThemeAndUpdateColours();
				switchThemeAndUpdatePalette = false;
			}
			Render();
		}

		private void ShowPointsCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
		{
			Render();
		}

		private void DistanceMeasureRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			Render();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			DelayedRender();
		}

		private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				switchThemeAndUpdatePalette = true;
			}
			DelayedRender();
		}

		private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				switchThemeAndUpdatePalette = true;
			}
			DelayedRender();
		}

		// CanExecuteRoutedEventHandler for the Save Fractal Image button command.
		private void SaveButtonCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		// ExecutedRoutedEventHandler for the Save Fractal Image button command.
		private void SaveButtonCmdExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			string defaultNamePrefix = (string)Application.Current.FindResource("SaveImageDefaultNamePrefix");
			Microsoft.Win32.SaveFileDialog dlg = new()
			{
				FileName = $"{defaultNamePrefix}{DateTime.Now:yyyyMMddHHmmss}", // Default file name, "Voronoi_" followed by current date & time.
				DefaultExt = ".bmp",    // Default to Windows BMP format.
				Filter = (string)Application.Current.FindResource("SaveImageFilter")
			};
			// Show the save file dialog box
			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				// Save fractal image in the format specified by the file extension.
				string filename = dlg.FileName;
				string ext = Path.GetExtension(filename).Replace(".", "").ToLower();
				var type = ext switch
				{
					"gif" => ImageFileType.Gif,
					"jpeg" => ImageFileType.Jpeg,
					"png" => ImageFileType.Png,
					"tiff" => ImageFileType.Tiff,
					_ => ImageFileType.Bmp,
				};
				_bmp.Save(filename, type);
			}
		}

		private void PaletteStepTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			// Only refresh display when the Step value has actually changed.
			if (_cachedPaletteStep != _model.PaletteStep)
			{
				_cachedPaletteStep = _model.PaletteStep;
				_points = SwitchThemeAndUpdateColours();
				DelayedRender();
			}
		}
	}
}
