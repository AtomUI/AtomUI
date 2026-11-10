using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Segmented: the track, the selected thumb and the Token-owner transparent items (hovered, pressed, selected, disabled, with
/// icons) in every size. SegmentedItem rows run against the Segmented.
/// </summary>
public partial class DataEntryFamilyEffectTests
{
    internal static Segmented CreateSegmented(CustomizableSizeType size = CustomizableSizeType.Middle, bool icons = false)
    {
        var segmented = new Segmented { SizeType = size };
        segmented.Items.Add(new SegmentedItem { Content = "Daily", Icon = icons ? new BarsOutlined() : null });
        segmented.Items.Add(new SegmentedItem { Content = "Weekly", Icon = icons ? new AppstoreOutlined() : null });
        segmented.Items.Add(new SegmentedItem { Content = "Monthly" });
        segmented.Items.Add(new SegmentedItem { Content = "Yearly", IsEnabled = false });
        segmented.SelectedIndex = 1;
        return segmented;
    }

    private static SegmentedItem SegmentedItemAt(Control segmented, int index) =>
        segmented.GetVisualDescendants().OfType<SegmentedItem>().ElementAt(index);

    public static IEnumerable<object[]> SegmentedRows() => HostRows("Segmented", "SegmentedItem");

    [Theory]
    [MemberData(nameof(SegmentedRows))]
    public void Segmented_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(SegmentedScenario(token), SegmentedTokens.Identity, token);
    }

    private static Scenario SegmentedScenario(string token)
    {
        switch (token)
        {
            // A hovered and a pressed unselected item.
            case "ItemHoverColor" or "ItemHoverBg" or "ColorFillSecondary":
                return new Scenario(static () => CreateSegmented(), static segmented => Pseudo(SegmentedItemAt(segmented, 0), ":pointerover"),
                    new EffectOptions { Region = static segmented => SegmentedItemAt(segmented, 0) });
            case "ItemActiveBg" or "ColorFill":
                return new Scenario(static () => CreateSegmented(), static segmented =>
                {
                    Pseudo(SegmentedItemAt(segmented, 0), ":pointerover");
                    Pseudo(SegmentedItemAt(segmented, 0), ":pressed");
                }, new EffectOptions { Region = static segmented => SegmentedItemAt(segmented, 0) });
            // The disabled item.
            case "ColorTextDisabled":
                return new Scenario(static () => CreateSegmented(), null,
                    new EffectOptions { Region = static segmented => SegmentedItemAt(segmented, 3) });
            // Items with icons: icon sizes per size, the gap after the icon.
            case "IconSize" or "SegmentedItemContentMargin" or "UniformlyPaddingXXS":
                return new Scenario(static () => CreateSegmented(icons: true));
            // Track, thumb and item radii: a large radius shows on the grey track and the white thumb.
            case "BorderRadius" or "BorderRadiusSM":
                return new Scenario(static () => CreateSegmented(), null,
                    new EffectOptions { Perturb = static _ => new Avalonia.CornerRadius(16) });
            case "BorderRadiusLG":
                return new Scenario(static () => CreateSegmented(CustomizableSizeType.Large), null,
                    new EffectOptions { Perturb = static _ => new Avalonia.CornerRadius(20) });
            case "BorderRadiusXS":
                return new Scenario(static () => CreateSegmented(CustomizableSizeType.Small), null,
                    new EffectOptions { Perturb = static _ => new Avalonia.CornerRadius(12) });
            case "IconSizeLG" or "ItemMinHeightLG" or "ControlHeightLG" or "FontSizeLG":
                return new Scenario(static () => CreateSegmented(CustomizableSizeType.Large, icons: true));
            case "IconSizeSM" or "ItemMinHeightSM" or "ControlHeightSM" or "SegmentedItemPaddingSM" or
                "ControlPaddingHorizontalSM":
                return new Scenario(static () => CreateSegmented(CustomizableSizeType.Small, icons: true));
            default:
                // Track (TrackBg / ColorBgLayout, TrackPadding / LineWidthBold, radius BorderRadius / BorderRadiusSM), the
                // selected thumb (ItemSelectedBg / ColorBgElevated, BoxShadowsTertiary), item text (ItemColor /
                // ColorTextLabel, ItemSelectedColor / ColorText), ItemMinHeight / ControlHeight, SegmentedItemPadding /
                // ControlPaddingHorizontal / LineWidth, FontSize, EnableMotion and MotionDuration* (read back).
                return new Scenario(static () => CreateSegmented());
        }
    }
}
