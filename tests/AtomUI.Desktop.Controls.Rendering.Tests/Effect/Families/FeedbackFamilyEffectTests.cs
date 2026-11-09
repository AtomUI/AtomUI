using System.Diagnostics;
using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the feedback family (docs/engineering/development/theme-effect-tests.md): Alert, Popup, ToolTip,
/// PopupConfirm, Dialog (with MessageBox and DialogButtonBox), MessageCard, NotificationCard and their window managers.
/// Every Active Own Token and every consumed global Token of each member visibly changes the member when overridden through
/// <c>Controls[member]</c>. The rows come from the generated consumption map. Members that render in a popup or an overlay
/// (ToolTip, Popup drop-downs, PopupConfirm, overlay dialogs) are hosted in a layer manager with the overlay popup layer
/// enabled, so they render inside the test window.
/// </summary>
public partial class FeedbackFamilyEffectTests
{
    // ---- Alert --------------------------------------------------------------------------------

    public static IEnumerable<object[]> AlertRows() => MapRows("Alert");

    [Theory]
    [MemberData(nameof(AlertRows))]
    public void Alert_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        EffectOptions? options = null;
        Action<Alert> setup = token switch
        {
            // The described layout: larger padding, the heading-size icon with its margin, the larger message and the
            // description below it.
            "WithDescriptionPadding" or "PaddingContentHorizontalLG" or "UniformlyPaddingMD" or
                "WithDescriptionIconSize" or "FontSizeHeading3" or "IconWithDescriptionMargin" or "UniformlyMarginSM" or
                "DescriptionLabelMargin" or "FontSizeLG" or "FontHeight" => static a =>
                {
                    a.IsShowIcon  = true;
                    a.Description = "Detailed description and advice about the alert copywriting.";
                },
            // The compact layout shows the small icon, its margin and the icon-to-text spacing.
            "DefaultIconSize" or "IconDefaultMargin" or "SpacingXXS" or "UniformlyMarginXS" => static a =>
                a.IsShowIcon = true,
            // Each type fills its icon and frame with its status colours.
            "ColorSuccess" => Typed(AlertType.Success),
            "ColorPrimary" => Typed(AlertType.Info),
            "ColorInfoBg" or "ColorInfoBorder" => Typed(AlertType.Info),
            "ColorWarning" or "ColorWarningBg" or "ColorWarningBorder" => Typed(AlertType.Warning),
            "ColorError" or "ColorErrorBg" or "ColorErrorBorder" => Typed(AlertType.Error),
            // The close button and its icon.
            "CloseIconSize" or "FontSizeIcon" => static a => a.IsClosable = true,
            // The close button's leading margin only shows when it pushes the wrapping message: perturb it widely.
            "ExtraElementMargin" => static a =>
            {
                a.IsClosable = true;
                a.Width      = 320;
                a.Message    = string.Join(' ', Enumerable.Repeat("Alert message text", 6));
            },
            _ => static _ => { }
        };
        if (token == "ExtraElementMargin")
        {
            options = new EffectOptions { Perturb = static _ => new Thickness(96, 0, 0, 0) };
        }

        TokenEffectHarness.AssertHasEffect(() =>
        {
            var alert = new Alert { Message = "Success Tips", Type = AlertType.Success };
            setup(alert);
            return alert;
        }, AlertTokens.Identity, token, options: options);

        static Action<Alert> Typed(AlertType type) => a =>
        {
            a.Type       = type;
            a.IsShowIcon = true;
        };
    }

    // ---- owner reads --------------------------------------------------------------------------

    /// <summary>
    /// Global Tokens an identity-less part theme reads through <c>OwnerTokenResource</c>. The consumption map attributes
    /// them to <c>shared-primitive</c>, so the members carrying the part run them as rows of their own.
    /// </summary>
    internal sealed record OwnerReadList(string Theme, string[] Tokens);

    /// <summary>Every owner-read list of the family; the drift guard keeps each equal to its theme AXAML.</summary>
    private static OwnerReadList[] FamilyOwnerReadLists() => [s_overlayPopupHost, s_popupRoot, ..DialogOwnerReadLists()];

    [Fact]
    public void Owner_Read_Lists_Match_The_Part_Themes()
    {
        foreach (var list in FamilyOwnerReadLists())
        {
            ThemeOwnerReads.Read(list.Theme).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }

    // ---- shared helpers -----------------------------------------------------------------------

    private static IEnumerable<object[]> MapRows(string controlId) => EffectTestData.For("AtomUI", controlId).ToArray();

    /// <summary>Consumption-map rows of the member plus the owner reads of the identity-less parts it carries.</summary>
    private static IEnumerable<object[]> OwnerRows(string controlId, OwnerReadList[] parts)
    {
        var rows = MapRows(controlId).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in parts.SelectMany(static list => list.Tokens).Distinct(StringComparer.Ordinal)
                                   .Order(StringComparer.Ordinal))
        {
            if (seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    /// <summary>A layer manager with the popup overlay layer enabled, so popups render inside the test window.</summary>
    internal static Control OverlayLayerHost(Control content, double width = 480, double height = 360)
    {
        content.HorizontalAlignment = HorizontalAlignment.Left;
        content.VerticalAlignment   = VerticalAlignment.Top;
        var layers = new VisualLayerManager
        {
            Child = new Panel { Width = width, Height = height, Children = { content } }
        };
        EnablePopupOverlayLayer(layers);
        return layers;
    }

    internal static void EnablePopupOverlayLayer(VisualLayerManager layers) =>
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(layers, true);

    /// <summary>Runs queued UI work until <paramref name="find"/> returns a laid-out visual, or fails.</summary>
    internal static T WaitForVisual<T>(Func<T?> find, string description)
        where T : Control
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            if (find() is { Bounds.Height: > 0 } visual)
            {
                TopLevel.GetTopLevel(visual)?.UpdateLayout();
                return visual;
            }
            Thread.Sleep(5);
        }

        throw new InvalidOperationException($"{description} did not appear.");
    }

    internal static void SettleFor(TimeSpan duration)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < duration)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(5);
        }
    }

    internal static void WaitFor(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(5);
        }
    }

    /// <summary>Shows <paramref name="content"/> under a local provider (motion on) and runs <paramref name="body"/>.</summary>
    internal static void WithProvider(Control content, Action<ThemeConfigProvider> body)
    {
        ThemeChangeFailedEventArgs? failure = null;
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = content };
        provider.ThemeChangeFailed += (_, args) => failure = args;
        var window = new AvaloniaWindow { Width = 800, Height = 600, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            body(provider);
            failure.ShouldBeNull(failure?.Exception?.Message);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    internal static ThemeConfig WithControlToken(ControlTokenIdentity identity, string token, string value) =>
        new ThemeConfigBuilder()
            .WithControl(identity, new ControlThemeConfigBuilder().WithToken(token, value).Build())
            .Build();

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }
}
