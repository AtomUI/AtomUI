using System.Diagnostics;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using InputOwnerReadList = AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families.InputFamilyEffectTests.OwnerReadList;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// DatePicker / RangeDatePicker / TimePicker / RangeTimePicker effect rows. A picker reads its globals through the shared
/// InfoPickerInput themes (owner: marker) and the identity-less input surface, embedded editor, suffix and popup presenter;
/// the picker forwards its Token owner to its presenter, which sits in the popup's registered ArrowDecoratedBox, so the time
/// view, its Token-owner transparent cells and the buttons frame read the picker's globals; the date presenter forwards the
/// same owner to its DateViewer, so the popup date panel, header and cells read the picker's globals too (the DateViewer Own
/// Tokens stay borrowed, see the isolation tests). Rows are the member's consumption-map rows plus the owner reads of the
/// parts it carries, minus the reads a member cannot show (with the reason).
/// </summary>
public partial class DataFamilyEffectTests
{
    private const string PickerPrimitives = "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/";
    private const string DatePickerThemes = "src/AtomUI.Desktop.Controls/DatePicker/Themes/";
    private const string TimePickerThemes = "src/AtomUI.Desktop.Controls/TimePicker/Themes/";

    /// <summary>InfoPickerInputTheme reads its globals from the owner: it is the base of DatePicker and TimePicker.</summary>
    internal static readonly OwnerReadList s_infoPickerInput = new(PickerPrimitives + "InfoPickerInputTheme.axaml",
    [
        "ColorTextDisabled", "ColorTextPlaceholder", "EnableMotion", "FontHeight", "FontHeightLG", "FontHeightSM", "FontSize",
        "FontSizeLG", "FontSizeSM", "SpacingXS"
    ]);

    /// <summary>RangeInfoPickerInputTheme reads its globals from the owner: it is the base of the range pickers.</summary>
    internal static readonly OwnerReadList s_rangeInfoPickerInput = new(PickerPrimitives + "RangeInfoPickerInputTheme.axaml",
    [
        "ColorError", "ColorIcon", "ColorPrimary", "ColorText", "ColorTextDisabled", "ColorTextPlaceholder", "ColorWarning",
        "IconSize", "SpacingXS"
    ]);

    internal static readonly OwnerReadList s_infoPickerTextBox = new(PickerPrimitives + "InfoPickerTextBoxTheme.axaml",
        ["ColorText"]);

    internal static readonly OwnerReadList s_pickerClearUpButton = new(PickerPrimitives + "PickerClearUpButtonTheme.axaml",
        ["ColorTextQuaternary", "IconSize", "SpacingXS"]);

    internal static readonly OwnerReadList s_datePickerPresenter = new(DatePickerThemes + "DatePickerPresenterTheme.axaml",
        ["BorderThickness", "ColorBorderSecondary"]);

    /// <summary>The popup date panel headers (shown by the forwarded DateViewer) read the picker's globals.</summary>
    internal static readonly OwnerReadList s_datePickerHeaders = new(DatePickerThemes + "DatePickerPresenterResources.axaml",
        ["ColorBorderSecondary", "ControlHeight"]);

    internal static readonly OwnerReadList s_timePickerPresenter = new(TimePickerThemes + "TimePickerPresenterTheme.axaml",
        ["BorderThickness", "ColorBorderSecondary"]);

    internal static readonly OwnerReadList s_timeView = new(TimePickerThemes + "TimeViewTheme.axaml",
        ["ColorBgElevated", "ColorBorderSecondary", "LineWidth"]);

    /// <summary>TimeViewCell is Token-owner transparent: its theme (owner: marker) reads the hosting picker's globals.</summary>
    internal static readonly OwnerReadList s_timeViewCell = new(TimePickerThemes + "TimeViewCellTheme.axaml",
    [
        "BorderRadiusSM", "ColorBgTextHover", "ColorTextDisabled", "ColorTextSecondary", "ColorTransparent",
        "ControlItemBgActive", "ControlItemBgActiveHover", "ControlItemColorActive"
    ]);

    private static OwnerReadList Input(InputOwnerReadList list) => new(list.Theme, list.Tokens);

    /// <summary>The embedded editor (InfoPickerTextBox) inherits TextBoxTheme's owner reads.</summary>
    private static readonly OwnerReadList s_pickerTextBox = Input(InputFamilyEffectTests.s_textBox);

    private static readonly OwnerReadList[] s_pickerSurface =
    [
        Input(InputFamilyEffectTests.s_inputControlFrame), Input(InputFamilyEffectTests.s_addOnDecoratedBox),
        Input(InputFamilyEffectTests.s_clearButton), s_pickerTextBox, s_infoPickerTextBox, s_pickerClearUpButton,
        s_infoPickerInput
    ];

    private static readonly OwnerReadList[] s_pickerOwnLists =
    [
        s_infoPickerInput, s_rangeInfoPickerInput, s_infoPickerTextBox, s_pickerClearUpButton, s_datePickerPresenter,
        s_datePickerHeaders, s_timePickerPresenter, s_timeView, s_timeViewCell
    ];

    /// <summary>
    /// The popup date panel of a date picker: the picker's headers and the forwarded DateViewer's panel and cells. The
    /// DateViewer's default header (s_dateViewerHeader) is hidden, because the picker supplies its own header template.
    /// </summary>
    private static OwnerReadList[] PopupDatePanel() => [s_datePickerHeaders, s_datePanel, s_dateViewerCell];

    /// <summary>Owner reads of the popup DateViewer cells that the compact popup panel cannot show.</summary>
    private static (string Token, string Reason)[] PopupDatePanelNotRendered() =>
    [
        ("ColorSplit", "the top border of full-content cells; the popup panel is compact (Calendar and DateViewer render it)"),
        ("ControlItemBgHover", "the hovered full-content cell; the compact popup hover is the DateViewer's CellHoverBg (Calendar and DateViewer render it)")
    ];

    /// <summary>Reads of the embedded editor and the input surface that a picker cannot show (it has no such part).</summary>
    private static Dictionary<string, string> PickerNotRendered() => new(StringComparer.Ordinal)
    {
        ["BorderRadius"]        = "the embedded editor has no frame of its own; the input surface draws the radius (TextBox renders it)",
        ["BorderRadiusLG"]      = "the embedded editor has no frame of its own; the input surface draws the radius (TextBox renders it)",
        ["UniformlyPaddingXXS"] = "spaces the TextBox inner right content; the embedded editor has none (TextBox renders it)",
        ["SpacingXXS"]          = "spaces the TextBox / frame inner add-ons; the picker suffix uses SpacingXS (LineEdit renders it)",
        ["InputContentMargin"]  = "the picker lays out its own suffix instead of the frame's content add-ons (LineEdit renders it)"
    };

    private static readonly Dictionary<string, (OwnerReadList[] Lists, string[] Transparent, Dictionary<string, string> NotRendered)>
        s_pickerHosts = new(StringComparer.Ordinal)
        {
            ["DatePicker"] = ([.. s_pickerSurface, s_datePickerPresenter, .. PopupDatePanel(), s_timeView, s_timeViewCell], [],
                WithNotRendered([
                    ("RangePanelSpacing", "the gap between the two panels of the range picker (RangeDatePicker renders it)"),
                    .. PopupDatePanelNotRendered()
                ])),
            ["RangeDatePicker"] = ([.. s_pickerSurface, s_rangeInfoPickerInput, s_datePickerPresenter, .. PopupDatePanel(), s_timeView,
                s_timeViewCell], [], WithNotRendered(PopupDatePanelNotRendered())),
            ["TimePicker"] = ([.. s_pickerSurface, s_timePickerPresenter, s_timeView, s_timeViewCell], [], WithNotRendered(
                ("HeaderMargin", "the time view header is shown by the date-time picker only (the DatePicker fact renders it)"))),
            ["RangeTimePicker"] = ([.. s_pickerSurface, s_rangeInfoPickerInput, s_timePickerPresenter, s_timeView, s_timeViewCell],
                [], WithNotRendered())
        };

    private static Dictionary<string, string> WithNotRendered(params (string Token, string Reason)[] extra)
    {
        var result = PickerNotRendered();
        foreach (var (token, reason) in extra)
        {
            result[token] = reason;
        }

        return result;
    }

    private static IEnumerable<object[]> PickerRows(string controlId)
    {
        var (lists, _, notRendered) = s_pickerHosts[controlId];
        var rows = MapRows(controlId).Where(row => !notRendered.ContainsKey((string)row[0])).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in lists.SelectMany(static list => list.Tokens).Distinct(StringComparer.Ordinal)
                                   .Order(StringComparer.Ordinal))
        {
            if (!notRendered.ContainsKey(token) && seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    public static IEnumerable<object[]> DatePickerRows() => PickerRows("DatePicker");
    public static IEnumerable<object[]> RangeDatePickerRows() => PickerRows("RangeDatePicker");
    public static IEnumerable<object[]> TimePickerRows() => PickerRows("TimePicker");
    public static IEnumerable<object[]> RangeTimePickerRows() => PickerRows("RangeTimePicker");

    [Theory]
    [InlineData("DatePicker")]
    [InlineData("RangeDatePicker")]
    [InlineData("TimePicker")]
    [InlineData("RangeTimePicker")]
    public void Picker_Rows_Include_The_Rendered_Time_Cell_Radius(string controlId)
    {
        // The editor has no frame, but the popup time cells consume this global for every picker identity.
        PickerRows(controlId).Any(static row => (string)row[0] == "BorderRadiusSM" && !(bool)row[1]).ShouldBeTrue(
            $"{controlId} must exercise its time cell's owner-global BorderRadiusSM.");
    }

    [Theory]
    [MemberData(nameof(DatePickerRows))]
    public void DatePicker_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        RunPicker(PickerSetup(PickerKind.Date, token), DatePickerTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(RangeDatePickerRows))]
    public void RangeDatePicker_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "RangePickerArrowMargin" or "RangePickerIndicatorThickness" or "RangeMarginToAnchor")
        {
            AssertRangeInputToken(PickerKind.RangeDate, token);
            return;
        }
        RunPicker(PickerSetup(PickerKind.RangeDate, token), RangeDatePickerTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(TimePickerRows))]
    public void TimePicker_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        RunPicker(PickerSetup(PickerKind.Time, token), TimePickerTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(RangeTimePickerRows))]
    public void RangeTimePicker_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "RangePickerArrowMargin" or "RangePickerIndicatorThickness" or "RangeMarginToAnchor")
        {
            AssertRangeInputToken(PickerKind.RangeTime, token);
            return;
        }
        RunPicker(PickerSetup(PickerKind.RangeTime, token), RangeTimePickerTokens.Identity, token);
    }

    /// <summary>
    /// The InfoPickerInput Own Tokens (and their bases). InfoPickerInput is an abstract identity: its Own Tokens are read by
    /// the shared range theme and run on a RangeDatePicker; its global reads resolve to the concrete picker (rows above).
    /// </summary>
    private static void AssertRangeInputToken(PickerKind requestedKind, string token)
    {
        var (value, kind, popup, active) = token switch
        {
            // The popup's distance from the input (the range time picker reads it).
            "RangeMarginToAnchor" => ("24", PickerKind.RangeTime, true, false),
            // The active bar under the active input.
            "RangePickerIndicatorThickness" or "LineWidthBold" => ("6", PickerKind.RangeDate, false, true),
            // The gap around the range arrow.
            "RangePickerArrowMargin" => ("24,0,24,0", PickerKind.RangeDate, false, false),
            "UniformlyMarginXS" => ("24", PickerKind.RangeDate, false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };
        kind = requestedKind;
        var identity = kind == PickerKind.RangeDate ? RangeDatePickerTokens.Identity : RangeTimePickerTokens.Identity;
        var picker = CreatePicker(kind);
        picker.IsPopupPinnedOpen = popup;
        var host = PickerOverlayHost(picker);
        WithIsolationWindow(host, (provider, capture) =>
        {
            if (popup)
            {
                WaitForPickerPopup(picker);
            }

            if (active)
            {
                ((RangeInfoPickerInput)picker).RangeActivatedPart = RangeActivatedPart.Start;
            }

            using var before = capture();
            provider.Config = IsolationConfig(control: (identity, token, value));
            using var after = capture();
            // The popup moves within the host; the other Tokens change the input.
            ChangedRatioOf(before, after, popup ? host : picker).ShouldBeGreaterThan(0.001, $"Controls[{identity}].{token}");
            if (active)
            {
                // The thicker active bar stays on the bottom edge of the input frame.
                var indicator = picker.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Rectangle>()
                                      .First(static part => part.Name == "PART_RangePickerIndicator");
                indicator.Height.ShouldBe(6d);
                (Canvas.GetTop(indicator) + indicator.Height).ShouldBe(PickerFrameOf(picker).Bounds.Height, 0.5);
            }
        });
    }

    // ---- harness ------------------------------------------------------------------------------------------------------

    public enum PickerKind
    {
        Date,
        RangeDate,
        Time,
        RangeTime
    }

    internal sealed record PickerEffectSetup(Func<Control> Create, Action<Control>? Arrange, EffectOptions Options);

    private static void RunPicker(PickerEffectSetup setup, ControlTokenIdentity identity, string token) =>
        TokenEffectHarness.AssertHasEffect(setup.Create, identity, token, setup.Arrange, options: setup.Options);

    private static readonly DateTime s_pickerDate = new(2026, 7, 15, 10, 30, 45);

    internal static InfoPickerInput CreatePicker(PickerKind kind) => kind switch
    {
        PickerKind.Date => new DatePicker { Width = 260, SelectedDateTime = s_pickerDate, IsShowNow = true },
        PickerKind.RangeDate => new RangeDatePicker
        {
            Width = 380, RangeStartSelectedDate = s_pickerDate, RangeEndSelectedDate = s_pickerDate.AddDays(5), IsShowNow = true
        },
        PickerKind.Time => new TimePicker { Width = 200, SelectedTime = s_pickerDate.TimeOfDay },
        PickerKind.RangeTime => new RangeTimePicker
        {
            Width = 300, RangeStartSelectedTime = s_pickerDate.TimeOfDay, RangeEndSelectedTime = s_pickerDate.TimeOfDay.Add(TimeSpan.FromHours(2))
        },
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    /// <summary>
    /// Builds the setup of a picker. <c>init</c> steps run on the created picker before it is shown, <c>arrange</c> steps
    /// once its template exists; <c>popup</c> pins the popup open (in the overlay layer) and compares the presenter.
    /// </summary>
    private sealed class PickerBuilder(PickerKind kind)
    {
        public readonly PickerKind Kind = kind;
        public readonly List<Action<InfoPickerInput>> Init = [];
        public readonly List<Action<Control>> Arrange = [];
        public bool Popup;
        public Func<Control, Control>? Region;
        public Func<object, object>? Perturb;

        public bool IsRange => Kind is PickerKind.RangeDate or PickerKind.RangeTime;
        public bool IsDate => Kind is PickerKind.Date or PickerKind.RangeDate;

        public void Hover() => Arrange.Add(static c => PickerFrameOf(c).IsInnerBoxHover = true);
        public void Focus() => Arrange.Add(static c => PickerFrameOf(c).IsInputFocusWithin = true);
        public void Disabled() => Init.Add(static c => c.IsEnabled = false);

        /// <summary>Opens the popup with the time view (date pickers show it with IsShowTime).</summary>
        public void TimeView()
        {
            Popup = true;
            if (IsDate)
            {
                Init.Add(static c =>
                {
                    switch (c)
                    {
                        case DatePicker date:
                            date.IsShowTime = true;
                            break;
                        case RangeDatePicker range:
                            range.IsShowTime = true;
                            break;
                    }
                });
            }
        }

        public void Clock12() => Init.Add(static c =>
        {
            switch (c)
            {
                case TimePicker time:
                    time.ClockIdentifier = ClockIdentifierType.HourClock12;
                    break;
                case RangeTimePicker range:
                    range.ClockIdentifier = ClockIdentifierType.HourClock12;
                    break;
                case DatePicker date:
                    date.ClockIdentifier = ClockIdentifierType.HourClock12;
                    break;
                case RangeDatePicker rangeDate:
                    rangeDate.ClockIdentifier = ClockIdentifierType.HourClock12;
                    break;
            }
        });

        public PickerEffectSetup Build()
        {
            var init    = Init.ToArray();
            var arrange = Arrange.ToArray();
            var popup   = Popup;
            var kindOf  = Kind;
            return new PickerEffectSetup(
                () =>
                {
                    var picker = CreatePicker(kindOf);
                    foreach (var step in init)
                    {
                        step(picker);
                    }

                    if (popup)
                    {
                        picker.IsPopupPinnedOpen = true;
                    }

                    return popup ? PickerOverlayHost(picker) : picker;
                },
                control =>
                {
                    if (popup)
                    {
                        WaitForPickerPopup(control);
                    }

                    foreach (var step in arrange)
                    {
                        step(control);
                    }
                },
                new EffectOptions
                {
                    Target  = static host => host as InfoPickerInput ?? host.GetVisualDescendants().OfType<InfoPickerInput>().First(),
                    Region  = Region ?? (popup ? PresenterOf : null),
                    Perturb = Perturb
                });
        }
    }

    private static Control PickerOverlayHost(Control content)
    {
        content.HorizontalAlignment = HorizontalAlignment.Left;
        content.VerticalAlignment   = VerticalAlignment.Top;
        var layers = new VisualLayerManager
        {
            Child = new Panel { Width = 740, Height = 540, Children = { content } }
        };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(layers, true);
        return layers;
    }

    private static PickerPresenterBase? FindPresenter(Control control) =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<PickerPresenterBase>()
                .FirstOrDefault(static presenter => presenter.IsEffectivelyVisible && presenter.Bounds.Height > 0);

    internal static Control PresenterOf(Control control) =>
        FindPresenter(control).ShouldNotBeNull($"The popup of {control.GetType().Name} is not open.");

    internal static Control PopupRootOf(Control control) =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<ArrowDecoratedBox>()
                .First(static box => box.IsEffectivelyVisible && box.Bounds.Height > 0);

    internal static void WaitForPickerPopup(Control control)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            if (FindPresenter(control) is { } presenter &&
                presenter.GetVisualDescendants().OfType<Control>().Any(static part => part.Bounds.Height > 0))
            {
                return;
            }

            Thread.Sleep(5);
        }

        throw new InvalidOperationException($"The popup of {control.GetType().Name} did not open.");
    }

    private static InputControlFrame PickerFrameOf(Control control) =>
        control.GetVisualDescendants().OfType<InputControlFrame>().First();

    internal static Internal.DateViewer.DateViewerCell SelectedPopupDateCell(Control control) =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<Internal.DateViewer.DateViewerCell>()
                .First(static cell => cell.Model?.IsSelected == true && cell.IsEffectivelyVisible);

    private static TimeViewCell SelectedTimeCell(Control control) =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<TimeViewCell>()
                .First(static cell => cell.IsSelected && cell.IsEffectivelyVisible);

    /// <summary>The cell after the selected hour, which is in the column's viewport.</summary>
    private static TimeViewCell UnselectedTimeCell(Control control)
    {
        var selected = SelectedTimeCell(control);
        var cells    = ((Panel)selected.GetVisualParent()!).Children.OfType<TimeViewCell>().ToList();
        return cells[cells.IndexOf(selected) + 1];
    }

    private static Control ButtonsFrameOf(Control control) =>
        Descendant<AtomUI.Controls.Primitives.PixelAlignedBorder>(PresenterOf(control), control is TimePicker or RangeTimePicker ? "PART_ButtonsFrame" : "ButtonsFrame");

    private static CustomizableSizeType PickerSizeOf(string token) =>
        token is "UniformlyPaddingSM" or "ControlHeightSM" ? CustomizableSizeType.Middle :
        token.EndsWith("LG", StringComparison.Ordinal) ? CustomizableSizeType.Large :
        token.EndsWith("SM", StringComparison.Ordinal) ? CustomizableSizeType.Small :
        CustomizableSizeType.Middle;

    private static PickerEffectSetup PickerSetup(PickerKind kind, string token)
    {
        var b    = new PickerBuilder(kind);
        var size = PickerSizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            b.Init.Add(c => c.SizeType = size);
        }

        switch (token)
        {
            // ---- input surface (InputControlFrame / AddOnDecoratedBox) ----
            case "ColorPrimaryHover":
                b.Hover();
                break;
            case "ColorPrimary" or "InputActiveBorderColor" or "InputActiveShadow":
                // The focus border; the focus ring around the frame (the picker does not clip it).
                b.Focus();
                break;
            case "ColorError":
                b.Init.Add(static c => c.Status = InputControlStatus.Error);
                break;
            case "ColorErrorBorderHover":
                b.Init.Add(static c => c.Status = InputControlStatus.Error);
                b.Hover();
                break;
            case "InputErrorActiveShadow":
                b.Init.Add(static c => c.Status = InputControlStatus.Error);
                b.Focus();
                break;
            case "ColorWarning":
                b.Init.Add(static c => c.Status = InputControlStatus.Warning);
                break;
            case "ColorWarningBorderHover":
                b.Init.Add(static c => c.Status = InputControlStatus.Warning);
                b.Hover();
                break;
            case "InputWarningActiveShadow":
                b.Init.Add(static c => c.Status = InputControlStatus.Warning);
                b.Focus();
                break;
            case "ColorFillTertiary":
                b.Init.Add(static c => c.StyleVariant = InputControlStyleVariant.Filled);
                break;
            case "ColorFillSecondary":
                b.Init.Add(static c => c.StyleVariant = InputControlStyleVariant.Filled);
                b.Hover();
                break;
            case "ColorErrorBg":
                b.Init.Add(static c =>
                {
                    c.StyleVariant = InputControlStyleVariant.Filled;
                    c.Status       = InputControlStatus.Error;
                });
                break;
            case "ColorErrorBgHover":
                b.Init.Add(static c =>
                {
                    c.StyleVariant = InputControlStyleVariant.Filled;
                    c.Status       = InputControlStatus.Error;
                });
                b.Hover();
                break;
            case "ColorWarningBg":
                b.Init.Add(static c =>
                {
                    c.StyleVariant = InputControlStyleVariant.Filled;
                    c.Status       = InputControlStatus.Warning;
                });
                break;
            case "ColorWarningBgHover":
                b.Init.Add(static c =>
                {
                    c.StyleVariant = InputControlStyleVariant.Filled;
                    c.Status       = InputControlStatus.Warning;
                });
                b.Hover();
                break;
            case "ColorTextDisabled" or "ColorBgContainerDisabled":
                b.Disabled();
                break;
            case "ColorErrorText":
                b.Init.Add(static c =>
                {
                    c.Status    = InputControlStatus.Error;
                    c.LeftAddOn = "https://";
                });
                break;
            case "ColorWarningText":
                b.Init.Add(static c =>
                {
                    c.Status    = InputControlStatus.Warning;
                    c.LeftAddOn = "https://";
                });
                break;
            case "ColorFillAlter" or "InputPadding" or "InputPaddingLG" or "InputPaddingSM":
                b.Init.Add(static c => c.LeftAddOn = "https://");
                break;

            // ---- embedded editor and suffix ----
            case "ColorTextPlaceholder":
                b.Init.Add(static c =>
                {
                    ClearPicker(c);
                    c.PlaceholderText = "Placeholder";
                });
                break;
            case "SelectionBackground" or "SelectionForeground":
                b.Arrange.Add(static c => c.GetVisualDescendants().OfType<InfoPickerTextBox>().First().SelectAll());
                break;
            case "ColorTextTertiary":
                // The hovered clear icon (antd clear hover colorTextTertiary): the picker shows it while hovered with a value.
                b.Hover();
                b.Arrange.Add(static c => Pseudo(Descendant<InputClearIconButton>(c, "PART_ClearButton"), ":pointerover"));
                b.Region = static c => Descendant<InputClearIconButton>(c, "PART_ClearButton");
                break;
            case "ColorIcon" when b.IsRange:
                // The range arrow between the two inputs.
                b.Region = RangeArrowOf;
                break;
            case "SpacingXS":
                // The gap between the picker icon and a suffix.
                b.Init.Add(static c => c.ContentRightAddOn = "UTC");
                break;

            // ---- popup: date panel header, cells, buttons and time view ----
            case "RangePanelSpacing":
                b.Popup = true;
                break;
            case "HeaderMargin":
                b.TimeView();
                break;
            case "HeaderBorderThickness" or "ControlHeight" when b.IsDate:
                b.Popup = true;
                break;
            case "ColorTextLightSolid" when b.IsDate:
                // The selected popup date cell's light text.
                b.Popup  = true;
                b.Region = static c => SelectedPopupDateCell(c);
                break;
            case "ButtonsPanelMargin" or "ButtonsMargin":
                b.Popup  = true;
                b.Region = ButtonsFrameOf;
                break;
            case "ColorBorderSecondary":
                b.TimeView();
                break;
            case "PanelContentPadding" or "PaddingSM" or "ColorBgElevated" or "LineWidth":
                b.TimeView();
                break;
            case "UniformlyMarginXS" when !b.IsRange:
                // The buttons frame margin (ButtonsPanelMargin / ButtonsMargin).
                b.TimeView();
                b.Region = ButtonsFrameOf;
                break;
            case "ItemHeight" or "ItemWidth" or "ItemPadding" or "ItemMargin" or "ControlHeight" or "ControlHeightLG" or
                "UniformlyMarginXXS" or "ColorTextSecondary" or "ColorTransparent" or "ControlItemBgActive":
                b.TimeView();
                break;
            case "PeriodHostWidth":
                b.TimeView();
                b.Clock12();
                break;
            case "ItemBackgroundInset" or "CenterItemContent" or "ControlItemColorActive":
                b.TimeView();
                b.Region = static c => SelectedTimeCell(c);
                break;
            case "ControlItemBgActiveHover":
                // The selected cell of the column the picker input steps with navigation keys.
                b.TimeView();
                b.Arrange.Add(static c => SelectedTimeCell(c).IsKeyboardActive = true);
                b.Region = static c => SelectedTimeCell(c);
                break;
            case "ColorBgTextHover":
                b.TimeView();
                b.Arrange.Add(static c => Pseudo(UnselectedTimeCell(c), ":pointerover"));
                b.Region = static c => UnselectedTimeCell(c);
                break;
            case "BorderRadiusSM":
                // The selected time cell's rounded corners (radius 12, as the cell is short).
                b.TimeView();
                b.Region  = static c => SelectedTimeCell(c);
                b.Perturb = static _ => new CornerRadius(12);
                break;
        }

        return b.Build();
    }

    private static Control RangeArrowOf(Control control) =>
        control.GetVisualDescendants().OfType<Control>().First(static part => part.Name == "PART_RangePickerArrow");

    private static void ClearPicker(InfoPickerInput picker)
    {
        switch (picker)
        {
            case DatePicker date:
                date.SelectedDateTime = null;
                break;
            case RangeDatePicker range:
                range.RangeStartSelectedDate = null;
                range.RangeEndSelectedDate   = null;
                break;
            case TimePicker time:
                time.SelectedTime = null;
                break;
            case RangeTimePicker rangeTime:
                rangeTime.RangeStartSelectedTime = null;
                rangeTime.RangeEndSelectedTime   = null;
                break;
        }
    }

    [Fact]
    public void Picker_Owner_Read_Lists_Match_The_Part_Themes()
    {
        foreach (var list in s_pickerOwnLists.Concat([s_calendarHeader, s_calendarTheme, s_calendarDateCell, s_calendarDatePanel, s_datePanel,
                     s_dateViewerHeader, s_dateViewerCell]))
        {
            ThemeOwnerReads.Read(list.Theme).Union(ThemeOwnerReads.ReadCode(list.Code)).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }
}
