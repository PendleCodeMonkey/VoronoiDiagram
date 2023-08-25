using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PendleCodeMonkey.VoronoiDiagram
{
	class VoronoiGenerator
	{
		public static void GenerateVoronoi(WriteableBitmap wb,
											List<(System.Drawing.Point pt, int colour)> points,
											DistanceMeasurement measure = DistanceMeasurement.Euclidian,
											bool drawPoints = true,
											bool contrastPointRendering = true)
		{
			static double Distance(DistanceMeasurement measure, System.Drawing.Point point, int x, int y)
			{
				int xd = Math.Abs(x - point.X);
				int yd = Math.Abs(y - point.Y);
				return measure switch
				{
					DistanceMeasurement.Manhattan => xd + yd,
					DistanceMeasurement.Chebyshev => Math.Max(xd, yd),
					_ => Math.Sqrt((xd * xd) + (yd * yd))
				};
			}

			static byte GetContrastingColour(byte r, byte g, byte b) => (((r * 0.299) + (g * 0.587) + (b * 0.114)) > 127) ? (byte)0 : (byte)0xff;

			var stride = wb.BackBufferStride;
			var width = wb.PixelWidth;
			var height = wb.PixelHeight;

			byte[] pixels = new byte[height * width * 4];

			// Render the Voronoi cells
			Parallel.For(0, height, y =>
			{
				Parallel.For(0, width, x =>
				{
					int ind = -1;
					double dist = double.MaxValue;
					for (int i = 0; i < points.Count; i++)
					{
						double d = Distance(measure, points[i].pt, x, y);
						if (d < dist)
						{
							dist = d;
							ind = i;
						}
					}
					if (ind > -1)
					{
						int color = points[ind].colour;
						int index = y * stride + x * 3;
						pixels[index++] = (byte)(color & 0xff);
						pixels[index++] = (byte)(color >> 8 & 0xff);
						pixels[index] = (byte)(color >> 16 & 0xff);
					}
				});
			});

			// Render the points (if enabled)
			// This can be done in one of two ways:
			// 1) Using contrasting colouring - i.e. as black or white dots over the top of the Voronoi cells that have been rendered above.
			// 2) Using an enhanced point rendering scheme which entails drawing the point in black and surrounding it with a white outer circle.
			if (drawPoints)
			{
				if (contrastPointRendering)
				{
					List<byte> contrastColourList = new();
					foreach (var (pt, colour) in points)
					{
						// Determine the byte value to be used for the contrasting colour for this point (which will be 0 for black
						// and 0xff for white)
						int index = pt.Y * stride + pt.X * 3;
						byte b = pixels[index++];
						byte g = pixels[index++];
						byte r = pixels[index];
						contrastColourList.Add(GetContrastingColour(r, g, b));
					}
					int ptIndex = 0;
					foreach (var (pt, colour) in points)
					{
						int index = pt.Y * stride + pt.X * 3;
						byte contrastColour = contrastColourList[ptIndex++];
						for (int i = -2; i < 3; i++)
						{
							for (int j = -2; j < 3; j++)
							{
								index = (pt.Y + i) * stride + (pt.X + j) * 3;
								pixels[index++] = contrastColour;
								pixels[index++] = contrastColour;
								pixels[index] = contrastColour;
							}
						}
					}
				}
				else
				{
					// Use the enhanced point rendering scheme.
					List<((int y, int x) offsets, byte colour)> pts = new()
					{
						((-3, -2), 0xff), ((-3, -1), 0xff), ((-3, 0), 0xff), ((-3, 1), 0xff), ((-3, 2), 0xff),
						((-2, -3), 0xff), ((-2, -2), 0xff), ((-2, -1), 0), ((-2, 0), 0), ((-2, 1), 0), ((-2, 2), 0xff), ((-2, 3), 0xff),
						((-1, -3), 0xff), ((-1, -2), 0), ((-1, -1), 0), ((-1, 0), 0), ((-1, 1), 0), ((-1, 2), 0), ((-1, 3), 0xff),
						((0, -3), 0xff), ((0, -2), 0), ((0, -1), 0), ((0, 0), 0), ((0, 1), 0), ((0, 2), 0), ((0, 3), 0xff),
						((1, -3), 0xff), ((1, -2), 0), ((1, -1), 0), ((1, 0), 0), ((1, 1), 0), ((1, 2), 0), ((1, 3), 0xff),
						((2, -3), 0xff), ((2, -2), 0xff), ((2, -1), 0), ((2, 0), 0), ((2, 1), 0), ((2, 2), 0xff), ((2, 3), 0xff),
						((3, -2), 0xff), ((3, -1), 0xff), ((3, 0), 0xff), ((3, 1), 0xff), ((3, 2), 0xff)
					};
					foreach (var point in points)
					{
						foreach (var (offsets, colour) in pts)
						{
							int index = (point.pt.Y + offsets.y) * stride + (point.pt.X + offsets.x) * 3;
							pixels[index++] = colour;
							pixels[index++] = colour;
							pixels[index] = colour;
						}
					}
				}
			}

			// Update writeable bitmap.
			Int32Rect rect = new(0, 0, width, height);
			wb.WritePixels(rect, pixels, stride, 0);
		}
	}
}
