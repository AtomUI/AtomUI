using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// DateViewer and RangeDateViewer effect rows. Their identity-less header, panel and cells read the owner's globals
/// (hand lists below); the Own Tokens are DateViewer's, and RangeDateViewer reads them from the DateViewer identity (a
/// borrowed Own Token: <see cref="RangeDateViewer_Uses_Independent_Own_Tokens"/>).
/// </summary>
public partial class DataFamilyEffectTests
{
    private const string DateViewerThemes = "src/AtomUI.Desktop.Controls/DateViewer/Themes/";

    internal static readonly OwnerReadList s_datePanel = new(DateViewerThemes + "DatePanelTheme.axaml", ["ColorText"]);

    internal static readonly OwnerReadList s_dateViewerHeader = new(DateViewerThemes + "DateViewerHeaderTheme.axaml",
        ["ColorBorderSecondary", "ControlHeightLG"]);

    internal static readonly OwnerReadList s_dateViewerCell = new(DateViewerThemes + "DateViewerCellTheme.axaml",
    [
        "BorderRadiusSM", "BorderThickness", "ColorPrimary", "ColorSplit", "ColorText", "ColorTextDisabled",
        "ColorTextLightSolid", "ControlItemBgActive", "ControlItemBgHover", "EnableMotion"
    ]);

    /// <summary>
    /// DateViewer Own Tokens (and their bases) that only the RangeDateViewer renders: the gap between its two panels.
    /// They are covered by <see cref="RangeDateViewer_Uses_Independent_Own_Tokens"/>.
    /// </summary>
    private static readonly HashSet<string> s_rangeOnlyViewerTokens = new(StringComparer.Ordinal) { "PanelSpacing", "UniformlyMargin" };

    internal static DateViewer CreateDateViewer(
        DateViewerPresentation presentation = DateViewerPresentation.Compact,
        DateViewerSelectionUnit unit = DateViewerSelectionUnit.Date,
        DateTime? value = null,
        bool cellTemplate = false)
    {
        var viewer = new DateViewer
        {
            Presentation  = presentation,
            SelectionUnit = unit,
            Value         = value ?? s_calendarDate,
            DisplayDate   = value ?? s_calendarDate
        };
        if (presentation == DateViewerPresentation.Content)
        {
            viewer.Width = 620;
        }

        if (cellTemplate)
        {
            viewer.CellTemplate = new FuncDataTemplate<object?>((_, _) => new Avalonia.Controls.Border
            {
                Height = 12, Background = Avalonia.Media.Brushes.SteelBlue
            });
        }

        return viewer;
    }

    internal static RangeDateViewer CreateRangeDateViewer(
        DateViewerPresentation presentation = DateViewerPresentation.Compact,
        DateTime? start = null,
        DateTime? end = null,
        DateViewerSelectionUnit unit = DateViewerSelectionUnit.Date,
        bool cellTemplate = false) =>
        new()
        {
            Presentation = presentation,
            SelectionUnit = unit,
            CellTemplate = cellTemplate ? new FuncDataTemplate<object?>((_, _) => new Avalonia.Controls.Border
            {
                Height = 12, Background = Avalonia.Media.Brushes.SteelBlue
            }) : null,
            Value        = new DateViewerRange(start ?? s_calendarDate, end ?? s_calendarDate.AddDays(5)),
            DisplayDate  = start ?? s_calendarDate
        };

    private static DateViewerCell ViewerCell(Control viewer, DateTime date) =>
        viewer.GetVisualDescendants().OfType<DateViewerCell>()
              .First(cell => cell.Model is { } model && model.Value?.Date == date.Date && model.IsInView &&
                             model.Kind != DateViewerCellType.Week);

    private static DateViewerCell OutsideViewerCell(Control viewer) =>
        viewer.GetVisualDescendants().OfType<DateViewerCell>()
              .First(static cell => cell.Model is { Kind: DateViewerCellType.Date, IsInView: false });

    private static DateViewerCell WeekNumberCell(Control viewer) =>
        viewer.GetVisualDescendants().OfType<DateViewerCell>()
              .First(static cell => cell.Model is { Kind: DateViewerCellType.Week, IsInView: true });

    private static IEnumerable<object[]> ViewerRows(string controlId, IEnumerable<string> excluded)
    {
        var skip = excluded.ToHashSet(StringComparer.Ordinal);
        return HostRows(controlId, [], [s_datePanel, s_dateViewerHeader, s_dateViewerCell])
               .Where(row => !skip.Contains((string)row[0]));
    }

    public static IEnumerable<object[]> DateViewerRows() => ViewerRows("DateViewer", s_rangeOnlyViewerTokens);

    [Theory]
    [MemberData(nameof(DateViewerRows))]
    public void DateViewer_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ViewerScenario(token, range: false);
        Run(scenario.Create, scenario.Arrange, DateViewerTokens.Identity, token, scenario.Options);
    }

    public static IEnumerable<object[]> RangeDateViewerRows() => ViewerRows("RangeDateViewer", []);

    [Theory]
    [MemberData(nameof(RangeDateViewerRows))]
    public void RangeDateViewer_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ViewerScenario(token, range: true);
        Run(scenario.Create, scenario.Arrange, RangeDateViewerTokens.Identity, token, scenario.Options);
    }

    private static Scenario ViewerScenario(string token, bool range)
    {
        Scenario Compact(Action<Control>? arrange = null, EffectOptions? options = null, DateTime? value = null) =>
            new(() => range
                    ? CreateRangeDateViewer(start: value)
                    : CreateDateViewer(value: value), arrange, options);

        Scenario Content(bool cellTemplate = false, EffectOptions? options = null) =>
            new(() => range
                    ? CreateRangeDateViewer(DateViewerPresentation.Content, cellTemplate: cellTemplate)
                    : CreateDateViewer(DateViewerPresentation.Content, cellTemplate: cellTemplate), null, options);

        switch (token)
        {
            // ---- full-content cells ----
            case "ContentCellMinHeight" or "ContentCellSurfaceMargin" or "ContentCellTopBorderThickness" or
                "ContentCellValueMargin" or "ColorSplit" or "LineWidthBold" or "UniformlyMarginXXS":
                return Content();
            case "ContentCellContentMargin":
                // The cell template content is inset from the cell edges.
                return Content(cellTemplate: true);
            case "ControlItemBgActive":
                // The selected full-content cell's surface (and the range background derived from it); the range viewer
                // shows it on the today cell outside its range.
                return range
                    ? new Scenario(static () =>
                    {
                        var viewer = CreateRangeDateViewer(DateViewerPresentation.Content, TodayMonthValue(),
                            TodayMonthValue().AddDays(1));
                        viewer.DisplayDate = DateTime.Today;
                        return viewer;
                    })
                    : Content();
            case "CellWidth":
                // The value frame of the selected cell.
                return Compact(options: new EffectOptions { Region = static c => ViewerCell(c, s_calendarDate) });
            case "ColorBorderSecondary" when range:
                return Compact(options: new EffectOptions { Region = static c => Descendant<DateViewerHeader>(c) });

            // ---- period cells ----
            case "PeriodCellWidth":
                return new Scenario(() => range ? CreateRangeDateViewer(unit: DateViewerSelectionUnit.Month) : CreateDateViewer(unit: DateViewerSelectionUnit.Month));
            case "PeriodCellPadding":
                // The period cells have a fixed width and centred text: a leading inset moves the text.
                return new Scenario(() => range ? CreateRangeDateViewer(unit: DateViewerSelectionUnit.Month) : CreateDateViewer(unit: DateViewerSelectionUnit.Month), null,
                    new EffectOptions { Perturb = static _ => new Thickness(30, 0, 0, 0) });

            // ---- week mode ----
            case "WeekNumberOpacity":
                return new Scenario(() => range ? CreateRangeDateViewer(unit: DateViewerSelectionUnit.Week) : CreateDateViewer(unit: DateViewerSelectionUnit.Week), null,
                    new EffectOptions { Region = static c => WeekNumberCell(c) });

            // ---- cell states ----
            case "CellHoverBg":
                return Compact(static c => Pseudo(ViewerCell(c, s_calendarDate.AddDays(-3)), ":pointerover"),
                    new EffectOptions { Region = static c => ViewerCell(c, s_calendarDate.AddDays(-3)) });
            case "ControlItemBgHover":
                // The hovered full-content cell (the compact hover is the DateViewer's CellHoverBg).
                return new Scenario(() => range
                        ? CreateRangeDateViewer(DateViewerPresentation.Content)
                        : CreateDateViewer(DateViewerPresentation.Content),
                    static c => Pseudo(ViewerCell(c, s_calendarDate.AddDays(-3)), ":pointerover"));
            case "CellRangeBg":
                // A cell in the middle of a range shows the range background behind its value.
                return Compact(static c => Pseudo(ViewerCell(c, s_calendarDate.AddDays(-3)), ":range-middle"),
                    new EffectOptions { Region = static c => ViewerCell(c, s_calendarDate.AddDays(-3)) });
            case "BorderThickness":
                // The today outline (the selection is elsewhere in the month).
                return Compact(value: TodayMonthValue());
            case "ColorTextLightSolid":
                return Compact(options: new EffectOptions { Region = static c => ViewerCell(c, s_calendarDate) });
            case "BorderRadiusSM":
                return Compact(options: new EffectOptions
                {
                    Region = static c => ViewerCell(c, s_calendarDate), Perturb = static _ => new CornerRadius(12)
                });
            case "ColorTextDisabled":
                return Compact(options: new EffectOptions { Region = static c => OutsideViewerCell(c) });
            default:
                // Cell metrics (CellHeight, CellWidth, CellRowHeight, ControlHeightSM, UniformlyPaddingXXS), the panel
                // (PanelBg / ColorBgContainer, PanelPadding, PanelWidth, DateBodyPadding / UniformlyPadding /
                // UniformlyPaddingXS), the header and footer (HeaderBorderThickness, FooterBorderThickness / LineWidth,
                // HeaderHorizontalPadding, ColorBorderSecondary, ControlHeightLG), the selected cell (ColorPrimary) and the
                // panel text (ColorText); the range viewer also its panel gap (PanelSpacing).
                return Compact();
        }
    }

    [Fact]
    public void RangeDateViewer_Uses_Independent_Own_Tokens()
    {
        // Range geometry and cell colours belong to the range viewer terminal.
        foreach (var (token, value) in new[] { ("PanelSpacing", "48"), ("UniformlyMargin", "40"), ("CellRangeBg", "#ffff0000") })
        {
            var viewer = CreateRangeDateViewer();
            WithIsolationWindow(viewer, (provider, capture) =>
            {
                using var before = capture();
                provider.Config = IsolationConfig(control: (RangeDateViewerTokens.Identity, token, value));
                using var after = capture();
                ChangedRatioOf(before, after, viewer).ShouldBeGreaterThan(0.005, $"Controls[RangeDateViewer].{token}");
            });
        }
    }
}
