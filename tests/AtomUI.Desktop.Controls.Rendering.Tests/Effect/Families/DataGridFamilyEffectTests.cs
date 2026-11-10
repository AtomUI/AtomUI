using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Rendering.Tests.Effect;
using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using static AtomUI.Desktop.Controls.Tests.DataGrid.Theme.DataGridThemeTestFixture;
using GridControl = AtomUI.Desktop.Controls.DataGrid;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

// Permanent promise-surface contract: generated Active fields must change actual pixels in a real sourced grid.
public class DataGridFamilyEffectTests
{
    public static IEnumerable<object[]> Tokens() => GeneratedThemeConsumption.OwnTokens
        .Where(entry => entry.ControlId == "DataGrid" && entry.State == OwnTokenState.Active)
        .Select(entry => (entry.Token, Own: true))
        .Concat(GeneratedThemeConsumption.GlobalConsumers.Where(entry => entry.ControlId == "DataGrid")
            .Select(entry => (entry.GlobalToken, Own: false)))
        .Concat(s_ownerGlobals.Append("MotionDurationMid").Select(token => (token, Own: false)))
        .Distinct().Select(entry => new object[] { entry.Item1 });

    private static readonly string[] s_ownerGlobals =
    [
        "BorderRadius", "BorderRadiusSM", "BorderThickness", "ColorBgTextActive", "ColorBorder", "ColorPrimary",
        "ColorPrimaryActive", "ColorPrimaryHover", "ColorText", "ColorTextSecondary", "FontHeight", "FontWeightStrong",
        "IconSizeSM", "LineWidth", "SizeMS", "SpacingXS", "UniformlyMarginXS", "UniformlyMarginXXS"
    ];

    [Fact]
    public void Primitive_Owner_Reads_Match_The_Real_Theme_Assets()
    {
        ThemeOwnerReads.Read(
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridRowExpanderTheme.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnHeaderTheme.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnGroupHeaderTheme.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridOperationButtons.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/Filters/DataGridFilterIndicatorTheme.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/Filters/DataGridTreeFilterFlyoutPresenterTheme.axaml",
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/Filters/DataGridMenuFilterFlyoutPresenterTheme.axaml")
            .Order().ShouldBe(s_ownerGlobals.Order());
    }

    [Theory]
    [MemberData(nameof(Tokens))]
    public void Active_DataGrid_Tokens_Change_Their_Visible_State(string token)
    {
        TokenEffectHarness.AssertHasEffect(() => Host(CreateGrid(token)), DataGridTokens.Identity, token,
            control => Arrange((GridControl)control, token), options: new EffectOptions
            {
                Target = root => root.GetVisualDescendants().OfType<GridControl>().Single(),
                Region = control => Region((GridControl)control, token),
                // A wide table has only two clipped corners; use a visible radius change without weakening pixel threshold.
                Perturb = token.Contains("Radius", StringComparison.Ordinal) ? _ => new CornerRadius(80)
                    : token is "LeftFrozenShadows" or "RightFrozenShadows" ? value =>
                        new BoxShadows(new BoxShadow { IsInset = true, OffsetX = ((BoxShadows)value)[0].OffsetX,
                            Blur = 8, Spread = -8, Color = Colors.Red }) : null
            });
    }

    private static Control Region(GridControl grid, string token)
    {
        if (token is "MarginSM" or "MarginXXS")
        {
            return grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().First();
        }
        if (token == "TableTopLeftColumnCornerRadius")
        {
            return grid.GetVisualDescendants().OfType<DataGridTopLeftColumnHeader>().Single();
        }
        if (token is "HeaderBorderRadius" or "TableRadius" or "BorderRadiusLG")
        {
            return grid;
        }
        if (token is "LeftFrozenShadows" or "RightFrozenShadows" or "ColorSplit")
        {
            return grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(h =>
                h.IsShowFrozenShadow && h.FrozenShadowPosition ==
                    (token == "RightFrozenShadows" ? FrozenColumnShadowPosition.Right : FrozenColumnShadowPosition.Left));
        }
        if (token == "LineWidthBold")
        {
            return Rows(grid)[0];
        }
        if (token == "UniformlyMarginXXS")
        {
            return grid.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "IndicatorsLayout");
        }
        if (PopupToken(token))
        {
            return TopLevel.GetTopLevel(grid)!.GetVisualDescendants().OfType<Control>()
                .Single(p => p is DataGridTreeFilterFlyoutPresenter or DataGridMenuFilterFlyoutPresenter);
        }
        if (ExpanderToken(token) && !token.Contains("Expanded", StringComparison.Ordinal))
        {
            return grid.GetVisualDescendants().OfType<DataGridCell>().First(c => c.OwningColumn is DataGridDetailExpanderColumn);
        }
        if (token is "SpacingXS")
        {
            return grid.GetVisualDescendants().OfType<DataGridOperationButtons>().First();
        }
        if (token is "RowReorderIndicatorSize" or "SizeMD" or "IconSize" or "ColorBgTextHover" or "ColorBgTextActive")
        {
            return grid.GetVisualDescendants().OfType<DataGridRowReorderHandle>().First();
        }
        if (token is "FilterIndicatorPadding" or "UniformlyPaddingXS" or "UniformlyPaddingXXS" or "IconSizeSM" or "ColorBgTextHover" or "ColorBgTextActive" or "HeaderFilterHoverBg" or "TableHeaderFilterActiveBg" or "ColorIconHover")
        {
            return grid.GetVisualDescendants().OfType<DataGridFilterIndicator>().First();
        }
        if (token.Contains("Icon", StringComparison.Ordinal) || token is "OpacityLoading" or "FontHeight" or "ColorPrimary" or "SortIndicatorLayoutMargin")
        {
            return grid.GetVisualDescendants().OfType<DataGridSortIndicator>().First();
        }
        if (token.Contains("Header", StringComparison.Ordinal) || token is "ColorFillSecondary" or "FontWeightStrong" or "LineWidth")
        {
            return grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(h => h.OwningColumn == grid.Columns.OfType<DataGridTextColumn>().First());
        }
        if (token.Contains("Footer", StringComparison.Ordinal))
        {
            return grid.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Single(p => p.Name == "Footer");
        }
        return grid;
    }

}
