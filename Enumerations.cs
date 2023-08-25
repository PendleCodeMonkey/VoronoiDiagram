using System.ComponentModel;

namespace PendleCodeMonkey.VoronoiDiagram
{
	// Enumeration of the supported methods for calculating the distance between two points.
	public enum DistanceMeasurement
	{
		Manhattan,
		Chebyshev,
		Euclidian
	}

	public enum ImageFileType
	{
		Bmp,
		Gif,
		Jpeg,
		Png,
		Tiff
	}

	public enum PointRenderingStyle
	{
		Contrast,
		Enhanced
	}

	[TypeConverter(typeof(EnumToDescriptionConverter))]
	public enum PointAnchor
	{
		[Description("None (Random)")]
		None,
		[Description("Centre")]
		Centre,
		[Description("Top-Left")]
		TopLeft,
		[Description("Top-Right")]
		TopRight,
		[Description("Bottom-Left")]
		BottomLeft,
		[Description("Bottom-Right")]
		BottomRight
	}
}
