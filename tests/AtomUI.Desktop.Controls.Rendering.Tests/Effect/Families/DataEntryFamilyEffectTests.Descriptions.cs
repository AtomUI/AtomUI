using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.Layout;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>Descriptions: header, items (default and bordered, horizontal and vertical) and the bordered frame.</summary>
public partial class DataEntryFamilyEffectTests
{
    internal static Descriptions CreateDescriptions(
        Orientation layout = Orientation.Horizontal,
        bool bordered = false,
        SizeType size = SizeType.Middle)
    {
        var descriptions = new Descriptions
        {
            Width      = 520,
            Header     = "User info",
            Extra      = "Edit",
            Layout     = layout,
            IsBordered = bordered,
            SizeType   = size,
            ColumnInfo = 2
        };
        descriptions.Items.Add(new DescriptionItem { Label = "Name", Content = "Ada Lovelace" });
        descriptions.Items.Add(new DescriptionItem { Label = "Phone", Content = "1810000000" });
        descriptions.Items.Add(new DescriptionItem { Label = "City", Content = "London" });
        descriptions.Items.Add(new DescriptionItem { Label = "Remark", Content = "Analytical engine" });
        return descriptions;
    }

    private static Control DescriptionsHeader(Control descriptions) => Descendant<DockPanel>(descriptions, "HeaderLayout");

    private static Control DescriptionsGrid(Control descriptions) => Descendant<Grid>(descriptions, "PART_GridLayout");

    public static IEnumerable<object[]> DescriptionsRows() => MapRows("Descriptions");

    [Theory]
    [MemberData(nameof(DescriptionsRows))]
    public void Descriptions_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(DescriptionsScenario(token), DescriptionsTokens.Identity, token);
    }

    private static Scenario DescriptionsScenario(string token)
    {
        switch (token)
        {
            // The header: title colour, size, weight, line height, extra colour and the gap below it.
            case "TitleColor" or "ExtraColor" or "FontSizeLG" or "FontWeightStrong" or "FontHeightLG":
                return new Scenario(static () => CreateDescriptions(), null,
                    new EffectOptions { Region = static descriptions => DescriptionsHeader(descriptions) });
            // Bordered items: the label cell fill and colour, the cell paddings per size, the frame and the cell borders.
            case "LabelBg" or "ColorFillAlter" or "ColorTextSecondary" or "ItemPadding" or "UniformlyPaddingLG" or
                "UniformlyPaddingSM" or "ColorSplit" or "BorderThickness":
                return new Scenario(static () => CreateDescriptions(bordered: true));
            // Vertical bordered items: the separator line between label and content.
            case "LineWidth":
                return new Scenario(static () => CreateDescriptions(Orientation.Vertical, bordered: true));
            case "ItemPaddingLG" or "UniformlyPadding":
                return new Scenario(static () => CreateDescriptions(bordered: true, size: SizeType.Large));
            case "ItemPaddingSM" or "UniformlyPaddingXS":
                return new Scenario(static () => CreateDescriptions(bordered: true, size: SizeType.Small));
            case "BorderRadiusLG":
                return new Scenario(static () => CreateDescriptions(bordered: true), null,
                    new EffectOptions { Perturb = static _ => new Avalonia.CornerRadius(60) });
            // Default items: the row gap per size.
            case "Spacing":
                return new Scenario(static () => CreateDescriptions(size: SizeType.Large));
            case "SpacingXS":
                return new Scenario(static () => CreateDescriptions(size: SizeType.Small));
            default:
                // LabelColor / ColorTextTertiary, ContentColor / ColorText, ColonMargin / UniformlyMarginXXS /
                // UniformlyMarginXS, HeaderMargin / FontSizeSM / RelativeLineHeightSM, SpacingSM, FontSize,
                // RelativeLineHeight.
                return new Scenario(static () => CreateDescriptions());
        }
    }
}
