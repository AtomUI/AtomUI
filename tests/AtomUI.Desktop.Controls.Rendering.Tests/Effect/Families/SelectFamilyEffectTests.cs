using System.Diagnostics;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the select family (docs/engineering/development/theme-effect-tests.md): every Active Own Token and
/// every consumed global Token of each member visibly changes the member when overridden through <c>Controls[member]</c>.
/// The rows come from the generated consumption map plus the owner reads of the primitives a member hosts
/// (<see cref="RowsOf"/>). Members are hosted in a layer manager with the overlay popup layer enabled, so a drop-down opens
/// inside the test window; drop-down Tokens compare the popup frame.
/// </summary>
public partial class SelectFamilyEffectTests
{
    public static IEnumerable<object[]> SelectRows() => RowsOf("Select");

    [Theory]
    [MemberData(nameof(SelectRows))]
    public void Select_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(SelectSetup(token), SelectTokens.Identity, token);
    }

    // ---- shared helpers -----------------------------------------------------------------------

    internal sealed record EffectSetup(Func<Control> Create, Action<Control>? Arrange, EffectOptions Options);

    private static void Run(EffectSetup setup, ControlTokenIdentity identity, string token)
    {
        TokenEffectHarness.AssertHasEffect(setup.Create, identity, token, setup.Arrange, options: setup.Options);
    }

    private static readonly SelectOption[] s_selectOptions =
    [
        new() { Header = "Jack", Content = "jack" },
        new() { Header = "Lucy", Content = "lucy" },
        new() { Header = "Tom", Content = "tom" },
        new() { Header = "Disabled", Content = "disabled", IsEnabled = false }
    ];

    private static CustomizableSizeType SizeOf(string token) =>
        // These are bases of middle-size values, not small-size Tokens.
        token is "UniformlyPaddingSM" ? CustomizableSizeType.Middle :
        token.EndsWith("LG", StringComparison.Ordinal) ? CustomizableSizeType.Large :
        token.EndsWith("SM", StringComparison.Ordinal) ? CustomizableSizeType.Small :
        CustomizableSizeType.Middle;

    /// <summary>
    /// Builds the setup of a select-family member. <c>init</c> steps run on the created control before it is shown,
    /// <c>arrange</c> steps once its template exists; <c>popup</c> pins the drop-down open and compares its frame.
    /// </summary>
    private sealed class SetupBuilder
    {
        public readonly List<Action<Control>> Init = [];
        public readonly List<Action<Control>> Arrange = [];
        public bool Popup;
        public Func<Control, Control>? Region;
        public double RegionInflation = 0;
        public Func<object, object>? Perturb;

        public void Hover() => Arrange.Add(static c => FrameOf(c).IsInnerBoxHover = true);
        public void Focus() => Arrange.Add(static c => ((IPseudoClasses)FrameOf(c).Classes).Add(":focus-within"));
        public void Disabled() => Init.Add(static c => c.IsEnabled = false);

        public void OnPart<T>(string name, string pseudoClass) where T : Control => Arrange.Add(c =>
            ((IPseudoClasses)c.GetVisualDescendants().OfType<T>().First(part => part.Name == name).Classes).Add(pseudoClass));

        public EffectSetup Build(Func<Control> factory, Func<Control, Control> target)
        {
            var init      = Init.ToArray();
            var arrange   = Arrange.ToArray();
            var popup     = Popup;
            var region    = Region;
            var perturb   = Perturb;
            return new EffectSetup(
                () =>
                {
                    var control = factory();
                    foreach (var step in init)
                    {
                        step(control);
                    }
                    if (popup)
                    {
                        SetPinnedOpen(control);
                    }

                    // Only an open drop-down needs the overlay layer; closed members render as plain children.
                    return popup ? OverlayLayerHost(control) : control;
                },
                control =>
                {
                    if (popup)
                    {
                        WaitForPopup(control);
                    }
                    foreach (var step in arrange)
                    {
                        step(control);
                    }
                },
                new EffectOptions
                {
                    Target          = target,
                    Region          = region ?? (popup ? PopupFrameOf : null),
                    RegionInflation = RegionInflation,
                    Perturb         = perturb
                });
        }
    }

    private static void SetPinnedOpen(Control control)
    {
        switch (control)
        {
            case AbstractSelect select:
                select.IsPopupPinnedOpen = true;
                break;
            case ComboBox comboBox:
                comboBox.IsPopupPinnedOpen = true;
                break;
            default:
                throw new NotSupportedException(control.GetType().Name);
        }
    }

    /// <summary>A layer manager with the popup overlay layer enabled, so the drop-down renders in the window.</summary>
    private static Control OverlayLayerHost(Control content)
    {
        content.HorizontalAlignment = HorizontalAlignment.Left;
        content.VerticalAlignment   = VerticalAlignment.Top;
        var layers = new VisualLayerManager
        {
            Child = new Panel { Width = 420, Height = 420, Children = { content } }
        };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(layers, true);
        return layers;
    }

    private static Control PopupFrameOf(Control control) =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<Border>()
                .First(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible);

    private static void WaitForPopup(Control control)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            var frame = TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<Border>()
                                .FirstOrDefault(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible);
            if (frame is { Bounds.Height: > 0 })
            {
                return;
            }
            Thread.Sleep(5);
        }

        throw new InvalidOperationException($"The drop-down of {control.GetType().Name} did not open.");
    }

    /// <summary>
    /// The states of the shared input surface (InputControlFrame / AddOnDecoratedBox owner reads), common to every member.
    /// Returns false for Tokens that are not surface Tokens.
    /// </summary>
    private static bool FrameState(
        SetupBuilder b,
        string token,
        Action<InputControlStatus> status,
        Action<InputControlStyleVariant> variant,
        Action leftAddOn,
        Action selected)
    {
        switch (token)
        {
            case "ColorPrimaryHover":
                b.Hover();
                return true;
            case "ColorPrimary" or "InputActiveShadow":
                b.Focus();
                return true;
            case "ColorError":
                status(InputControlStatus.Error);
                return true;
            case "ColorErrorBorderHover":
                status(InputControlStatus.Error);
                b.Hover();
                return true;
            case "InputErrorActiveShadow":
                status(InputControlStatus.Error);
                b.Focus();
                return true;
            case "ColorWarning":
                status(InputControlStatus.Warning);
                return true;
            case "ColorWarningBorderHover":
                status(InputControlStatus.Warning);
                b.Hover();
                return true;
            case "InputWarningActiveShadow":
                status(InputControlStatus.Warning);
                b.Focus();
                return true;
            case "ColorFillTertiary":
                variant(InputControlStyleVariant.Filled);
                return true;
            case "ColorErrorBg":
                variant(InputControlStyleVariant.Filled);
                status(InputControlStatus.Error);
                return true;
            case "ColorErrorBgHover":
                variant(InputControlStyleVariant.Filled);
                status(InputControlStatus.Error);
                b.Hover();
                return true;
            case "ColorWarningBg":
                variant(InputControlStyleVariant.Filled);
                status(InputControlStatus.Warning);
                return true;
            case "ColorWarningBgHover":
                variant(InputControlStyleVariant.Filled);
                status(InputControlStatus.Warning);
                b.Hover();
                return true;
            case "ColorTextDisabled" or "ColorBgContainerDisabled":
                selected();
                b.Disabled();
                return true;
            case "ColorErrorText":
                status(InputControlStatus.Error);
                leftAddOn();
                return true;
            case "ColorWarningText":
                status(InputControlStatus.Warning);
                leftAddOn();
                return true;
            case "ColorFillAlter" or "InputPadding" or "InputPaddingLG" or "InputPaddingSM":
                leftAddOn();
                return true;
            default:
                return false;
        }
    }

    /// <summary>Types into the single-mode filter input and selects the text, so the selection brushes show.</summary>
    private static void SelectFilterText(SetupBuilder b) => b.Arrange.Add(static c =>
    {
        var filter = c.GetVisualDescendants().OfType<SelectFilterTextBox>().First(static box => box.IsEffectivelyVisible);
        filter.Text = "Filter text";
        filter.SelectAll();
    });

    /// <summary>The input surface of the member: the outermost frame of its template.</summary>
    private static InputControlFrame FrameOf(Control control) =>
        control.GetVisualDescendants().OfType<InputControlFrame>().First();

    private static T Popup<T>(Control control) where T : Control =>
        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<T>().First(static item => item.IsEffectivelyVisible);
}
