using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Steps. Every theme of the Steps parts reads the Steps TokenResource: StepsItem is a registered item with no Own Tokens
/// and is Token-owner transparent, the indicator, the layout panels and the arrows have no identity, so all reads are map
/// rows of Steps and <c>Controls[Steps]</c> reaches them. The default arrange is a horizontal three-step Steps on the
/// second step (finish, process, wait); each row switches the type, orientation, size, status or hover state in which its
/// Token shows.
/// </summary>
public partial class StepsProgressFamilyEffectTests
{
    public static IEnumerable<object[]> StepsRows() => EffectTestData.For("AtomUI", "Steps");

    private sealed record StepsCase
    {
        public StepsType Type { get; init; } = StepsType.Default;
        public Orientation Orientation { get; init; } = Orientation.Horizontal;
        public Orientation TitlePlacement { get; init; } = Orientation.Horizontal;
        public SizeType SizeType { get; init; } = SizeType.Middle;
        public StepsPanelVariant PanelVariant { get; init; } = StepsPanelVariant.Filled;
        public StepsStatus Status { get; init; } = StepsStatus.Process;
        public double? Percent { get; init; }
        public bool Clickable { get; init; }
        public bool CustomIcon { get; init; }
        public bool LongContent { get; init; }
        public int Current { get; init; } = 1;

        /// <summary>Explicit status of the last item (the wait item by default).</summary>
        public StepsStatus? LastStatus { get; init; }

        /// <summary>Hovers the item at this index (pseudo-class and the indicator's item hover).</summary>
        public int? HoverIndex { get; init; }

        /// <summary>
        /// The part whose bounds are compared when the Token paints a small part of a wide Steps (an icon, a tail, an
        /// arrow): the part of the item in a given status, or of a given index.
        /// </summary>
        public Func<Steps, Control>? Region { get; init; }

        public Func<object, object>? Perturb { get; init; }
    }

    private static readonly StepsCase s_default = new();
    private static readonly StepsCase s_error = new() { Status = StepsStatus.Error };
    private static readonly StepsCase s_small = new() { SizeType = SizeType.Small };
    private static readonly StepsCase s_vertical = new() { Orientation = Orientation.Vertical };
    private static readonly StepsCase s_verticalTitle = new() { TitlePlacement = Orientation.Vertical };
    private static readonly StepsCase s_progress = new() { Percent = 60 };
    private static readonly StepsCase s_customIcon = new() { CustomIcon = true };
    private static readonly StepsCase s_dot = new() { Type = StepsType.Dot };
    private static readonly StepsCase s_dotError = new() { Type = StepsType.Dot, Status = StepsStatus.Error };
    private static readonly StepsCase s_outlineDot = new() { Type = StepsType.OutlineDot };
    private static readonly StepsCase s_navigation = new() { Type = StepsType.Navigation };
    private static readonly StepsCase s_verticalNavigation = new() { Type = StepsType.Navigation, Orientation = Orientation.Vertical };
    private static readonly StepsCase s_inline = new() { Type = StepsType.Inline };
    private static readonly StepsCase s_panel = new() { Type = StepsType.Panel };
    private static readonly StepsCase s_panelError = new() { Type = StepsType.Panel, Status = StepsStatus.Error };
    private static readonly StepsCase s_outlinedPanel = new() { Type = StepsType.Panel, PanelVariant = StepsPanelVariant.Outlined };

    private static StepsCase CaseFor(string token)
    {
        var @case = BaseCaseFor(token);
        if (@case.Region is null && StatusRegion(token) is { } region)
        {
            @case = @case with { Region = region };
        }

        return @case;
    }

    // Status colours of a part paint a small area of the Steps: compare that part of the item in the colour's status.
    private static Func<Steps, Control>? StatusRegion(string token)
    {
        var status = token switch
        {
            _ when token.StartsWith("Wait", StringComparison.Ordinal) => StepsStatus.Wait,
            _ when token.StartsWith("Process", StringComparison.Ordinal) => StepsStatus.Process,
            _ when token.StartsWith("Finish", StringComparison.Ordinal) => StepsStatus.Finish,
            _ when token.StartsWith("Error", StringComparison.Ordinal) => StepsStatus.Error,
            _ => (StepsStatus?)null
        };
        if (status is not { } value)
        {
            return null;
        }

        if (token.EndsWith("TailColor", StringComparison.Ordinal))
        {
            return steps => Part(ItemWhere(steps, item => item.ConnectorStatus == value && !item.IsLast), "Connector");
        }

        if (token.Contains("Icon", StringComparison.Ordinal) || token.EndsWith("DotColor", StringComparison.Ordinal))
        {
            return steps => Part(ItemWhere(steps, item => item.EffectiveStatus == value), "PART_Indicator");
        }

        return steps => ItemWhere(steps, item => item.EffectiveStatus == value);
    }

    private static StepsCase BaseCaseFor(string token) => token switch
    {
        // ---- indicator -----------------------------------------------------------------------
        // The default radius equals the container size (a circle); a larger radius looks the same, so square it.
        "IconContainerCornerRadius" => s_default with { Perturb = static _ => new CornerRadius(4) },
        "IconContainerSizeSM" or "FontSizeHeading3" or "FontSizeSM" => s_small,
        "CustomIconSize" or "CustomIconFontSize" or "ControlHeightSM" => s_customIcon,
        "ProgressGrooveColor" or "ProgressColor" or "ColorSplit" or "ProgressFramePadding" => s_progress,
        "ProgressFramePaddingSM" => s_progress with { SizeType = SizeType.Small },
        "ErrorIconColor" or "ErrorIconBgColor" or "ErrorIconBorderColor" or "ErrorTitleColor" or "ErrorDescriptionColor"
            or "ErrorTailColor" or "ColorError" => s_error,
        "WaitDotColor" or "ProcessDotColor" or "FinishDotColor" or "DotSize" or "DotCurrentSize" or "DotLineThickness"
            or "ControlHeightLG" => s_dot,
        "ErrorDotColor" => s_dotError,
        // The tail of an item follows the next item's status: two finished items show the finish tail.
        "FinishTailColor" => s_default with { Current = 2 },
        "OutlineDotBorderThickness" => s_outlineDot,
        // Colours derived into a single status part: the wait icon (ColorTextLabel, ColorFillContent), the process icon
        // mark (ColorTextLightSolid), the finish icon background (ControlItemBgActive) and the wait tail (ColorTextDisabled).
        "ColorTextLabel" or "ColorFillContent" => s_default with { Region = static s => Part(ItemAt(s, 2), "PART_Indicator") },
        "ColorTextLightSolid" => s_default with { Region = static s => Part(ItemAt(s, 1), "PART_Indicator") },
        "ControlItemBgActive" => s_default with { Region = static s => Part(ItemAt(s, 0), "PART_Indicator") },
        "ColorTextDisabled" => s_default with { Region = static s => Part(ItemAt(s, 1), "Connector") },
        // A clickable wait item turns process-coloured on hover; the process indicator turns ColorPrimaryHover.
        "ColorPrimaryHover" => s_default with { Clickable = true, LastStatus = StepsStatus.Process, HoverIndex = 2 },
        // ---- layout --------------------------------------------------------------------------
        "DescriptionMaxWidth" => s_default with { LongContent = true },
        // The horizontal rail gap only applies when titles sit below the icons.
        "UniformlyMarginXXS" => s_verticalTitle with { Region = static s => Part(ItemAt(s, 0), "Connector") },
        "VerticalItemSpacing" => s_vertical with { Region = static s => Part(ItemAt(s, 0), "Connector") },
        "VerticalDescriptionPadding" or "UniformlyPaddingSM" => s_vertical,
        "VerticalLabelContentMargin" or "UniformlyMarginSM" => s_verticalTitle,
        // ---- navigation ----------------------------------------------------------------------
        "NavArrowColor" or "FontSizeIcon" => s_navigation with { Region = static s => Part(ItemAt(s, 0), "NavigationArrow") },
        "StepsNavActiveColor" or "NavItemGutter" or "Spacing" => s_navigation,
        // The arrow is centred in its slot, so a margin on every side cancels out; a one-sided margin moves it.
        "VerticalNavArrowMargin" => s_verticalNavigation with
        {
            Region = static s => Part(ItemAt(s, 0), "NavigationArrow"), Perturb = s_oneSidedMargin
        },
        "VerticalNavItemPadding" or "UniformlyPadding" => s_verticalNavigation,
        "VerticalNavArrowMarginSM" => s_verticalNavigation with
        {
            SizeType = SizeType.Small, Region = static s => Part(ItemAt(s, 0), "NavigationArrow"), Perturb = s_oneSidedMargin
        },
        // ---- inline --------------------------------------------------------------------------
        "InlineDotSize" or "InlineHeaderMargin" or "InlineHeaderPadding" or "InlineItemPadding" or "InlineTitleColor"
            or "InlineTailColor" or "ColorTextSecondary" or "UniformlyPaddingXS" => s_inline,
        "UniformlyPaddingXXS" => s_inline with { Region = static s => ItemAt(s, 0) },
        "ControlItemBgHover" => s_inline with { HoverIndex = 0 },
        // ---- panel ---------------------------------------------------------------------------
        "PanelWaitBackground" or "PanelFinishBackground" or "PanelProcessActiveBackground" or "PanelWaitTextColor"
            or "PanelFinishTextColor" or "PanelActiveTextColor" or "PanelArrowWidth" or "RelativeLineHeightLG"
            or "ColorFillTertiary" or "ColorPrimaryBg" => s_panel,
        // A non-current process / error item uses the plain panel colours; a current one the active colours.
        "PanelProcessBackground" or "PanelProcessTextColor" => s_panel with
        {
            LastStatus = StepsStatus.Process, Region = static s => ItemAt(s, 2)
        },
        "PanelErrorBackground" or "PanelErrorTextColor" or "ColorErrorBg" => s_panel with
        {
            LastStatus = StepsStatus.Error, Region = static s => ItemAt(s, 2)
        },
        "PanelFinishActiveBackground" => s_panel with { Status = StepsStatus.Finish },
        "PanelWaitActiveBackground" => s_panel with { Status = StepsStatus.Wait },
        "PanelErrorActiveBackground" or "PanelErrorActiveTextColor" => s_panelError,
        // The panel ends are rounded with a 6 / 4 px radius; 4 px more is a few corner pixels of a 200 px item, so the
        // rows round the ends by half the item height.
        "PanelFirstCornerRadius" or "BorderRadius" => s_panel with { Region = static s => ItemAt(s, 0), Perturb = s_largeRadius },
        "PanelLastCornerRadius" => s_panel with { Region = static s => ItemAt(s, 2), Perturb = s_largeRadius },
        "PanelFirstCornerRadiusSM" or "BorderRadiusSM" => s_panel with
        {
            SizeType = SizeType.Small, Region = static s => ItemAt(s, 0), Perturb = s_largeRadius
        },
        "PanelLastCornerRadiusSM" => s_panel with
        {
            SizeType = SizeType.Small, Region = static s => ItemAt(s, 2), Perturb = s_largeRadius
        },
        "PanelArrowWidthSM" or "RelativeLineHeight" or "PaddingXS" => s_panel with { SizeType = SizeType.Small },
        "PanelFirstBorderThickness" or "PanelMiddleBorderThickness" or "PanelLastBorderThickness" or "ColorBgContainer"
            or "LineWidthBold" => s_outlinedPanel,
        "PaddingSM" => s_panel,
        _ => s_default
    };

    private static readonly Func<object, object> s_largeRadius = static _ => new CornerRadius(20);
    private static readonly Func<object, object> s_oneSidedMargin = static _ => new Thickness(8, 8, 0, 0);

    private static StepsItem ItemAt(Steps steps, int index) => steps.Items.OfType<StepsItem>().ElementAt(index);

    private static StepsItem ItemWhere(Steps steps, Func<StepsItem, bool> predicate) =>
        steps.Items.OfType<StepsItem>().First(predicate);

    private static Control Part(StepsItem item, string name) =>
        item.GetVisualDescendants().OfType<Control>().First(control => control.Name == name);

    [Theory]
    [MemberData(nameof(StepsRows))]
    public void Steps_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var @case = CaseFor(token);
        Action<Control>? arrange = @case.HoverIndex is { } index
            ? control =>
            {
                var item = ((Steps)control).Items.OfType<StepsItem>().ElementAt(index);
                ((IPseudoClasses)item.Classes).Add(":pointerover");
                item.GetVisualDescendants().OfType<StepsItemIndicator>().First().IsItemHover = true;
            }
            : null;
        var options = @case.Region is null && @case.Perturb is null
            ? null
            : new EffectOptions
            {
                Region  = @case.Region is { } region ? control => region((Steps)control) : null,
                Perturb = @case.Perturb
            };
        TokenEffectHarness.AssertHasEffect(() => CreateSteps(@case), StepsTokens.Identity, token, arrange, options: options);
    }

    private static Steps CreateSteps(StepsCase @case)
    {
        var steps = new Steps
        {
            Type            = @case.Type,
            Orientation     = @case.Orientation,
            TitlePlacement  = @case.TitlePlacement,
            SizeType        = @case.SizeType,
            PanelVariant    = @case.PanelVariant,
            Status          = @case.Status,
            Percent         = @case.Percent,
            IsItemClickable = @case.Clickable,
            Current         = @case.Current
        };
        // Inline steps size to their content (antd inline-flex); the other horizontal types fill a fixed width.
        if (@case.Orientation == Orientation.Horizontal && @case.Type != StepsType.Inline)
        {
            steps.Width = 640;
        }

        var content = @case.LongContent
            ? "A description long enough to wrap at the default maximum description width"
            : "Description";
        steps.Items.Add(new StepsItem
        {
            Header = "Finished", SubHeader = "00:01", Content = content,
            Icon   = @case.CustomIcon ? new UserOutlined() : null
        });
        steps.Items.Add(new StepsItem { Header = "In Progress", SubHeader = "00:02", Content = content });
        steps.Items.Add(new StepsItem { Header = "Waiting", SubHeader = "00:03", Content = content, Status = @case.LastStatus });
        return steps;
    }
}
