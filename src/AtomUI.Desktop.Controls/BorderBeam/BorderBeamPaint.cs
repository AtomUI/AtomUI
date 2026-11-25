using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal readonly record struct BorderBeamPaint(
    IBrush? StartColor,
    IBrush? EndColor,
    double Opacity,
    double MaxVisibleStopPercent)
{
    internal static readonly BorderBeamPaint Default = new(null, null, 0.95d, 70d);
}
