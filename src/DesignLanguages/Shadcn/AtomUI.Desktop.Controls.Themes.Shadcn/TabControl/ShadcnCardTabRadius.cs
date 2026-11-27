using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The radius cap of the card tabs: the pill and rounded styles would otherwise turn a tab into a capsule.
/// </summary>
internal static class ShadcnCardTabRadius
{
    private const double Maximum = 8;

    public static CornerRadius Limit(CornerRadius radius) =>
        new(Math.Min(radius.TopLeft, Maximum),
            Math.Min(radius.TopRight, Maximum),
            Math.Min(radius.BottomRight, Maximum),
            Math.Min(radius.BottomLeft, Maximum));
}
