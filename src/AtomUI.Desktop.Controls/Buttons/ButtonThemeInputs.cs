using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal sealed class ButtonThemeInputs
{
    private ButtonThemeInputs() { }

    public const double DisabledOpacity = 0.5;

    public static readonly TextDecorationCollection LinkUnderline =
    [
        new TextDecoration
        {
            Location = TextDecorationLocation.Underline,
            StrokeOffset = 4,
            StrokeOffsetUnit = TextDecorationUnit.Pixel
        }
    ];
}
