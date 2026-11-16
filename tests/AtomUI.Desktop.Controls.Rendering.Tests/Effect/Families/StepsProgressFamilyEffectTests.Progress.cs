using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the progress controls (docs/engineering/development/theme-effect-tests.md): ProgressBar,
/// StepsProgressBar, CircleProgress and DashboardProgress. Each control has its own terminal Token (sharing the
/// AbstractProgressToken / AbstractLineProgressToken / AbstractCircleProgressToken layers), so every Active Own Token and
/// every consumed global Token visibly changes the control when overridden through <c>Controls[control]</c>. The shared
/// base themes (AbstractProgressBar, AbstractLineProgress, AbstractCircleProgress) have no Token identity and read globals
/// through <c>OwnerTokenResource</c>; the consumption map attributes those reads to <c>shared-primitive</c>, so each
/// control runs them as rows of its own and the drift guard keeps the lists equal to the theme AXAML.
/// </summary>
public partial class StepsProgressFamilyEffectTests
{
    private const string ProgressThemes = "src/AtomUI.Desktop.Controls/ProgressBar/Themes/";

    internal static readonly InputFamilyEffectTests.OwnerReadList s_progressBase = new(
        ProgressThemes + "AbstractProgressBarTheme.axaml",
        ["EnableMotion", "ColorError", "ColorSuccess", "ColorTextLabel", "ColorBgContainerDisabled",
         "ControlItemBgActiveDisabled", "ColorTextDisabled"]);

    internal static readonly InputFamilyEffectTests.OwnerReadList s_lineBase = new(
        ProgressThemes + "AbstractLineProgressTheme.axaml", ["FontSize", "FontSizeSM"]);

    internal static readonly InputFamilyEffectTests.OwnerReadList s_circleBase = new(
        ProgressThemes + "AbstractCircleProgressTheme.axaml", []);

    private static readonly InputFamilyEffectTests.OwnerReadList[] s_familyLists = [s_progressBase, s_lineBase, s_circleBase];

    private const string CircleText = "the circle percentage uses the control's CircleTextColor";

    /// <summary>The base themes each control carries, and the owner reads it does not render (with the reason).</summary>
    private static readonly Dictionary<string, (InputFamilyEffectTests.OwnerReadList[] Lists, Dictionary<string, string> NotRendered)>
        s_hosts = new(StringComparer.Ordinal)
        {
            ["ProgressBar"]       = ([s_progressBase, s_lineBase], new(StringComparer.Ordinal)),
            ["StepsProgressBar"]  = ([s_progressBase, s_lineBase], new(StringComparer.Ordinal)),
            ["CircleProgress"]    = ([s_progressBase, s_circleBase], new(StringComparer.Ordinal) { ["ColorTextLabel"] = CircleText }),
            ["DashboardProgress"] = ([s_progressBase, s_circleBase], new(StringComparer.Ordinal) { ["ColorTextLabel"] = CircleText }),
        };

    public static IEnumerable<object[]> ProgressBarRows() => RowsOf("ProgressBar");
    public static IEnumerable<object[]> StepsProgressBarRows() => RowsOf("StepsProgressBar");
    public static IEnumerable<object[]> CircleProgressRows() => RowsOf("CircleProgress");
    public static IEnumerable<object[]> DashboardProgressRows() => RowsOf("DashboardProgress");

    // ---- ProgressBar --------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(ProgressBarRows))]
    public void ProgressBar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        EffectOptions? options = null;
        Action<ProgressBar> setup = token switch
        {
            // The status icon of a completed (Large) bar.
            "LineInfoIconSize" or "IconSize" or "ColorSuccess" => static p => p.Value = 100,
            // Small bars use the small icon and the small font.
            "LineInfoIconSizeSM" or "IconSizeSM" => static p =>
            {
                p.SizeType = SizeType.Small;
                p.Value    = 100;
            },
            "FontSizeSM" => static p => p.SizeType = SizeType.Small,
            "ColorError" => static p => p.Status = ProgressStatus.Exception,
            "ColorBgContainerDisabled" or "ControlItemBgActiveDisabled" or "ColorTextDisabled" => static p => p.IsEnabled = false,
            // The padding of the bar around an inner percentage label, and the label colour on the filled track.
            "LineProgressPadding" or "UniformlyPaddingXXS" or "ColorTextLightSolid" => static p =>
                p.PercentPosition = new PercentPosition { IsInner = true, Alignment = LinePercentAlignment.Start },
            // The inner label picks InnerLabelColor where it reads better than ColorTextLightSolid: on a bright bar.
            "InnerLabelColor" => static p =>
            {
                p.PercentPosition = new PercentPosition { IsInner = true, Alignment = LinePercentAlignment.Start };
                p.StrokeBrush     = Avalonia.Media.Brushes.Yellow;
            },
            _ => static _ => { }
        };
        if (token == "LineBorderRadius")
        {
            // The default radius (100) already makes a capsule of every thickness; a larger radius looks the same, so the
            // row squares the ends instead.
            options = new EffectOptions { Perturb = static _ => new Avalonia.CornerRadius(0) };
        }

        Run(() =>
        {
            var progress = new ProgressBar { Width = 300, Value = 50 };
            setup(progress);
            return progress;
        }, ProgressBarTokens.Identity, token, options);
    }

    // ---- StepsProgressBar ---------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(StepsProgressBarRows))]
    public void StepsProgressBar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<StepsProgressBar> setup = token switch
        {
            "LineInfoIconSize" or "IconSize" or "ColorSuccess" => static p => p.Value = 100,
            "LineInfoIconSizeSM" or "IconSizeSM" => static p =>
            {
                p.SizeType = SizeType.Small;
                p.Value    = 100;
            },
            // Small steps are 2 wide, the minimum step width: a larger minimum widens them.
            "ProgressStepMinWidth" or "FontSizeSM" => static p => p.SizeType = SizeType.Small,
            "ColorError" => static p => p.Status = ProgressStatus.Exception,
            "ColorBgContainerDisabled" or "ControlItemBgActiveDisabled" or "ColorTextDisabled" => static p => p.IsEnabled = false,
            _ => static _ => { }
        };

        Run(() =>
        {
            var progress = new StepsProgressBar { Steps = 5, Value = 60 };
            setup(progress);
            return progress;
        }, StepsProgressBarTokens.Identity, token);
    }

    // ---- CircleProgress / DashboardProgress -----------------------------------------------------

    [Theory]
    [MemberData(nameof(CircleProgressRows))]
    public void CircleProgress_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        RunCircle(static () => new CircleProgress(), CircleProgressTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(DashboardProgressRows))]
    public void DashboardProgress_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        RunCircle(static () => new DashboardProgress(), DashboardProgressTokens.Identity, token);
    }

    private static void RunCircle(Func<AbstractCircleProgress> create, ControlTokenIdentity identity, string token)
    {
        Action<AbstractCircleProgress> setup = token switch
        {
            // A 60 px circle computes a 15 px label and a 13.3 px icon: doubled minimums (20, 16) raise both.
            "CircleMinimumTextFontSize" or "FontSizeSM" => static p => p.Width = 60,
            "CircleMinimumIconSize" or "SizeXS" => static p =>
            {
                p.Width = 60;
                p.Value = 100;
            },
            "ColorSuccess" => static p => p.Value = 100,
            "ColorError" => static p => p.Status = ProgressStatus.Exception,
            "ColorBgContainerDisabled" or "ControlItemBgActiveDisabled" or "ColorTextDisabled" => static p => p.IsEnabled = false,
            _ => static _ => { }
        };

        Run(() =>
        {
            var progress = create();
            progress.Value = 50;
            setup(progress);
            return progress;
        }, identity, token);
    }

    // ---- owner-read drift guard -----------------------------------------------------------------

    [Fact]
    public void Owner_Read_Lists_Match_The_Base_Themes()
    {
        foreach (var list in s_familyLists)
        {
            ThemeOwnerReads.Read(list.Theme).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }

    [Fact]
    public void Every_Owner_Read_Is_Rendered_By_At_Least_One_Control()
    {
        foreach (var list in s_familyLists)
        {
            foreach (var token in list.Tokens)
            {
                s_hosts.Values.Any(host => host.Lists.Contains(list) && !host.NotRendered.ContainsKey(token))
                       .ShouldBeTrue($"{list.Theme} reads '{token}' but no progress control renders it.");
            }
        }
    }

    // ---- shared helpers -----------------------------------------------------------------------

    private static void Run(Func<Control> create, ControlTokenIdentity identity, string token, EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, options: options);
    }

    /// <summary>Consumption-map rows of the control plus the rendered owner reads of the base themes it carries.</summary>
    private static IEnumerable<object[]> RowsOf(string controlId)
    {
        var (lists, notRendered) = s_hosts[controlId];
        var rows = EffectTestData.For("AtomUI", controlId).ToList();
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
}
