using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PendleCodeMonkey.VoronoiDiagram
{
	internal class DataModel : INotifyPropertyChanged
	{
		private DistanceMeasurement _distanceMeasure;
		private bool? _showPoints;
		private int _numberOfPoints;
		private List<string> _themeNames;
		private string _selectedThemeName;
		private bool _reverseColours;
		private bool _randomizeColours;
		private bool _useColourTheme;
		private PointAnchor _anchor;
		private int _paletteStep;
		private PointRenderingStyle _pointRendering;

		public DistanceMeasurement DistanceMeasure
		{
			get
			{
				return _distanceMeasure;
			}

			set
			{
				if (value != _distanceMeasure)
				{
					_distanceMeasure = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool? ShowPoints
		{
			get
			{
				return _showPoints ?? false;
			}

			set
			{
				if (value != _showPoints)
				{
					_showPoints = value;
					NotifyPropertyChanged();
				}
			}
		}

		public int NumberOfPoints
		{
			get
			{
				return _numberOfPoints;
			}

			set
			{
				if (value != _numberOfPoints)
				{
					_numberOfPoints = value;
					NotifyPropertyChanged();
				}
			}
		}

		public List<string> ThemeNames
		{
			get
			{
				return _themeNames;
			}

			set
			{
				if (value != _themeNames)
				{
					_themeNames = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string SelectedThemeName
		{
			get
			{
				return _selectedThemeName;
			}

			set
			{
				if (value != _selectedThemeName)
				{
					_selectedThemeName = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool ReverseColours
		{
			get
			{
				return _reverseColours;
			}

			set
			{
				if (value != _reverseColours)
				{
					_reverseColours = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool RandomizeColours
		{
			get
			{
				return _randomizeColours;
			}

			set
			{
				if (value != _randomizeColours)
				{
					_randomizeColours = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool UseColourTheme
		{
			get
			{
				return _useColourTheme;
			}

			set
			{
				if (value != _useColourTheme)
				{
					_useColourTheme = value;
					NotifyPropertyChanged();
				}
			}
		}

		public PointAnchor Anchor
		{
			get
			{
				return _anchor;
			}

			set
			{
				if (value != _anchor)
				{
					_anchor = value;
					NotifyPropertyChanged();
				}
			}
		}

		public static IEnumerable<PointAnchor> AnchorDescriptions
		{
			get { return Enum.GetValues(typeof(PointAnchor)).Cast<PointAnchor>(); }
		}

		public int PaletteStep
		{
			get
			{
				return _paletteStep;
			}

			set
			{
				if (value != _paletteStep)
				{
					_paletteStep = value;
					NotifyPropertyChanged();
				}
			}
		}

		public PointRenderingStyle PointRendering
		{
			get
			{
				return _pointRendering;
			}

			set
			{
				if (value != _pointRendering)
				{
					_pointRendering = value;
					NotifyPropertyChanged();
				}
			}
		}


		public DataModel()
		{
			_themeNames = new List<string>();
			_selectedThemeName = string.Empty;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		// This method is called by the Set accessor of each property.  
		// The CallerMemberName attribute that is applied to the optional propertyName  
		// parameter causes the property name of the caller to be substituted as an argument.  
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
