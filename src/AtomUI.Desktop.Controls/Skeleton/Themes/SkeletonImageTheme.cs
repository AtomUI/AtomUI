using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class SkeletonImageTheme : ControlTheme
{
    public static readonly IBrush DefaultGlyphForeground =
        new ImmutableSolidColorBrush(Color.FromRgb(0xbf, 0xbf, 0xbf));
}
