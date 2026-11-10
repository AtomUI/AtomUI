using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes;

internal static class EmbeddingThemeConstants
{
    // TopLevel.SystemBarColor requires a mutable SolidColorBrush; the standard Color-to-IBrush
    // conversion returns an immutable brush and does not target this stricter property type.
    public static readonly IMultiValueConverter SystemBarBrushConverter =
        new FuncMultiValueConverter<ISolidColorBrush, SolidColorBrush?>(values =>
            values.FirstOrDefault() is { } brush ? new SolidColorBrush(brush.Color, brush.Opacity) : null);

    public const string TransparencyFallbackPart = "PART_TransparencyFallback";
    public const string ContentPresenterPart = "PART_ContentPresenter";
}