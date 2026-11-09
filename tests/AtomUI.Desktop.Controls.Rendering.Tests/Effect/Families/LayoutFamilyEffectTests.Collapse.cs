using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Collapse (its items are Token-owner transparent and read the Collapse's Tokens) and Expander. Each is shown expanded so
/// the header, the content and the separators between them are visible.
/// </summary>
public partial class LayoutFamilyEffectTests
{
    // ---- Collapse -------------------------------------------------------------------------------------------------------

    internal static Collapse CreateCollapse(
        CustomizableSizeType size = CustomizableSizeType.Middle,
        CollapseExpandIconPosition position = CollapseExpandIconPosition.Start,
        bool enabled = true,
        bool ghost = false,
        bool borderless = false)
    {
        var collapse = new Collapse
        {
            Width              = 360,
            SizeType           = size,
            ExpandIconPosition = position,
            IsEnabled          = enabled,
            IsGhostStyle       = ghost,
            IsBorderless       = borderless
        };
        collapse.Items.Add(new CollapseItem { Header = LongHeader, Content = "Panel content 1", IsSelected = true });
        collapse.Items.Add(new CollapseItem { Header = "Panel header 2", Content = "Panel content 2" });
        collapse.Items.Add(new CollapseItem { Header = "Panel header 3", Content = "Panel content 3" });
        return collapse;
    }

    /// <summary>A header that reaches the expand icon, so the icon's inline margin clips it.</summary>
    private const string LongHeader = "Panel header 1 with a title long enough to reach the expand icon at the end";

    /// <summary>
    /// The inline gap between the header text and an end-positioned expand icon: perturb only the leading side, which
    /// clips the long header earlier (the other sides would just move the icon).
    /// </summary>
    private static object WiderLeadingMargin(object value) =>
        new Thickness(((Thickness)value).Left + 48, 0, 0, 0);

    public static IEnumerable<object[]> CollapseRows() => MapRows("Collapse");

    [Theory]
    [MemberData(nameof(CollapseRows))]
    public void Collapse_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var size = token switch
        {
            "CollapseHeaderPaddingLG" or "CollapseContentPaddingLG" or "LeftExpandButtonMarginLG" or "FontSizeLG" or
                "FontHeightLG" or "UniformlyPaddingLG" or "PaddingLG" or "UniformlyMarginSM" => CustomizableSizeType.Large,
            "RightExpandButtonMarginLG" => CustomizableSizeType.Large,
            "CollapseHeaderPaddingSM" or "CollapseContentPaddingSM" or "LeftExpandButtonMarginSM" or "UniformlyPaddingXS" or
                "PaddingSM" or "UniformlyMarginXXS" or "RightExpandButtonMarginSM" => CustomizableSizeType.Small,
            _ => CustomizableSizeType.Middle
        };
        var position = token.StartsWith("RightExpandButtonMargin", StringComparison.Ordinal)
            ? CollapseExpandIconPosition.End
            : CollapseExpandIconPosition.Start;
        var enabled = token != "ColorTextDisabled";
        // A ghost collapse paints its headers and contents with ColorTransparent.
        var ghost = token == "ColorTransparent";
        // The expand button margins and the rounded first header: compare the first header.
        var headerRegion = token.Contains("ExpandButtonMargin", StringComparison.Ordinal) ||
                           token is "CollapsePanelBorderRadius" or "BorderRadiusLG";
        Run(() => CreateCollapse(size, position, enabled, ghost), null, CollapseTokens.Identity, token,
            new EffectOptions
            {
                Perturb = token is "CollapsePanelBorderRadius" or "BorderRadiusLG"
                    ? static _ => new CornerRadius(24)
                    : position == CollapseExpandIconPosition.End ? WiderLeadingMargin : null,
                Region  = headerRegion ? FirstCollapseHeader : null
            });
    }

    private static Control FirstCollapseHeader(Control collapse) =>
        Descendant<PixelAlignedBorder>(Descendant<CollapseItem>(collapse), "PART_HeaderDecorator");

    [Fact]
    public void Collapse_Ghost_Panels_Paint_No_Header_Or_Content_Background()
    {
        // antd: a ghost collapse is transparent; ContentBg / HeaderBg do not reach it.
        WithProvider(CreateCollapse(ghost: true), provider =>
        {
            var collapse = (Collapse)((Control)provider.Child!);
            var item     = collapse.GetVisualDescendants().OfType<CollapseItem>().First();
            var header   = item.GetVisualDescendants().OfType<PixelAlignedBorder>().First(static b => b.Name == "PART_HeaderDecorator");
            var content  = item.GetVisualDescendants().OfType<PixelAlignedBorder>().First(static b => b.Name == "PART_ContentFrame");
            ((ISolidColorBrush)header.Background!).Color.A.ShouldBe((byte)0);
            ((ISolidColorBrush)content.Background!).Color.A.ShouldBe((byte)0);
        });
    }

    // ---- Expander -------------------------------------------------------------------------------------------------------

    internal static Expander CreateExpander(
        CustomizableSizeType size = CustomizableSizeType.Middle,
        ExpanderIconPosition position = ExpanderIconPosition.Start,
        bool enabled = true)
    {
        return new Expander
        {
            Width              = 360,
            Header             = LongHeader,
            Content            = "Expander content",
            IsExpanded         = true,
            SizeType           = size,
            ExpandIconPosition = position,
            IsEnabled          = enabled
        };
    }

    public static IEnumerable<object[]> ExpanderRows() => MapRows("Expander");

    [Theory]
    [MemberData(nameof(ExpanderRows))]
    public void Expander_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var size = token switch
        {
            "HeaderPaddingLG" or "ContentPaddingLG" or "FontSizeLG" or "FontHeightLG" or "UniformlyPaddingLG" or "PaddingLG" =>
                CustomizableSizeType.Large,
            "HeaderPaddingSM" or "ContentPaddingSM" or "UniformlyPaddingXS" or "PaddingSM" => CustomizableSizeType.Small,
            _ => CustomizableSizeType.Middle
        };
        var position = token == "RightExpandButtonHMargin" ? ExpanderIconPosition.End : ExpanderIconPosition.Start;
        var enabled  = token != "ColorTextDisabled";
        var ghost    = token == "ColorTransparent";
        Run(() =>
            {
                var expander = CreateExpander(size, position, enabled);
                expander.IsGhostStyle = ghost;
                return expander;
            }, null, ExpanderTokens.Identity, token,
            new EffectOptions
            {
                Perturb = token is "ExpanderBorderRadius" or "BorderRadiusLG"
                    ? static _ => new CornerRadius(24)
                    : position == ExpanderIconPosition.End ? WiderLeadingMargin : null,
                Region  = token.Contains("ExpandButtonHMargin", StringComparison.Ordinal)
                    ? static expander => Descendant<PixelAlignedBorder>(expander, "PART_HeaderDecorator")
                    : null
            });
    }
}
