using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shared geometry shell of a managed caption button: an inset rounded background layer plus the content layer
/// that determines the layout box. The background layer only carries visuals and does not change the hit area.
/// The caption band and the <c>WindowTitleBar</c> add-on buttons share the same geometry source through it.
/// </summary>
internal class CaptionButtonFrame : ContentControl
{
    public static readonly StyledProperty<Thickness> BackgroundInsetProperty =
        AvaloniaProperty.Register<CaptionButtonFrame, Thickness>(nameof(BackgroundInset));

    /// <summary>
    /// Inset of the background layer from the layout box. It only affects the painted background; the layout box is
    /// always determined by the content layer and <c>Padding</c>.
    /// </summary>
    public Thickness BackgroundInset
    {
        get => GetValue(BackgroundInsetProperty);
        set => SetValue(BackgroundInsetProperty, value);
    }
}
