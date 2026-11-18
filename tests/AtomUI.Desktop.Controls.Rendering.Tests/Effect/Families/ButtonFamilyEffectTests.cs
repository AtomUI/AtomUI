using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the Button family (docs/engineering/development/theme-effect-tests.md): every Active Own Token and
/// every consumed global Token of each member visibly changes the member when overridden through
/// <c>Controls[member]</c>. The rows come from the generated consumption map; the <c>*Setup</c> tables put the member
/// into a state where the Token shows. A Token without an entry is checked in the member's default state.
/// </summary>
public partial class ButtonFamilyEffectTests
{
    // ---- Button ---------------------------------------------------------------------------

    public static IEnumerable<object[]> ButtonRows() => EffectTestData.For("AtomUI", "Button");

    [Theory]
    [MemberData(nameof(ButtonRows))]
    public void Button_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ButtonSetup(token, static () => new AtomButton { Content = "Button" }, isDropdown: false),
            ButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> SubmitButtonRows() => EffectTestData.For("AtomUI", "SubmitButton");
    public static IEnumerable<object[]> ResetButtonRows() => EffectTestData.For("AtomUI", "ResetButton");

    [Theory]
    [MemberData(nameof(SubmitButtonRows))]
    public void SubmitButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ButtonSetup(token, static () => new SubmitButton { Content = "Submit", ButtonType = ButtonType.Default }, isDropdown: false),
            SubmitButtonTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(ResetButtonRows))]
    public void ResetButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ButtonSetup(token, static () => new ResetButton { Content = "Reset" }, isDropdown: false),
            ResetButtonTokens.Identity, token);
    }

    // ---- HyperLinkButton --------------------------------------------------------------------

    public static IEnumerable<object[]> HyperLinkButtonRows() => EffectTestData.For("AtomUI", "HyperLinkButton");

    [Theory]
    [MemberData(nameof(HyperLinkButtonRows))]
    public void HyperLinkButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(HyperLinkButtonSetup(token), HyperLinkButtonTokens.Identity, token);
    }

    // ---- DropdownButton ---------------------------------------------------------------------

    public static IEnumerable<object[]> DropdownButtonRows() => EffectTestData.For("AtomUI", "DropdownButton");

    [Theory]
    [MemberData(nameof(DropdownButtonRows))]
    public void DropdownButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "MarginToAnchor" or "UniformlyMarginXXS")
        {
            AssertDropdownFlyoutGap(token);
            return;
        }

        Run(ButtonSetup(token, static () => new DropdownButton { Content = "Dropdown" }, isDropdown: true),
            DropdownButtonTokens.Identity, token);
    }

    // ---- SplitButton ------------------------------------------------------------------------

    public static IEnumerable<object[]> SplitButtonRows() => EffectTestData.For("AtomUI", "SplitButton");

    [Theory]
    [MemberData(nameof(SplitButtonRows))]
    public void SplitButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "GutterToFlyout" or "UniformlyMarginXXS")
        {
            AssertSplitButtonFlyoutGap(token);
            return;
        }

        Run(SplitButtonSetup(token), SplitButtonTokens.Identity, token);
    }

    // ---- IconButton / ToggleIconButton ------------------------------------------------------

    public static IEnumerable<object[]> IconButtonRows() => EffectTestData.For("AtomUI", "IconButton");

    [Theory]
    [MemberData(nameof(IconButtonRows))]
    public void IconButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(IconButtonSetup(token, static () => new IconButton { Icon = new SearchOutlined() }),
            IconButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> ToggleIconButtonRows() => EffectTestData.For("AtomUI", "ToggleIconButton");

    [Theory]
    [MemberData(nameof(ToggleIconButtonRows))]
    public void ToggleIconButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(IconButtonSetup(token, static () => new ToggleIconButton
            {
                CheckedIcon   = new SearchOutlined(),
                UnCheckedIcon = new SearchOutlined()
            }),
            ToggleIconButtonTokens.Identity, token);
    }

    // ---- OptionButton / OptionButtonGroup ---------------------------------------------------

    public static IEnumerable<object[]> OptionButtonRows() => EffectTestData.For("AtomUI", "OptionButton");

    [Theory]
    [MemberData(nameof(OptionButtonRows))]
    public void OptionButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(OptionButtonSetup(token, targetsButton: true), OptionButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> OptionButtonGroupRows() => EffectTestData.For("AtomUI", "OptionButtonGroup");

    [Theory]
    [MemberData(nameof(OptionButtonGroupRows))]
    public void OptionButtonGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(OptionButtonSetup(token, targetsButton: false), OptionButtonGroupTokens.Identity, token);
    }

    // ---- shared helpers -----------------------------------------------------------------------

    internal sealed record EffectSetup(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static void Run(EffectSetup setup, ControlTokenIdentity identity, string token)
    {
        TokenEffectHarness.AssertHasEffect(
            setup.Create,
            identity,
            token,
            setup.Arrange,
            options: setup.Options);
    }

    private static Action<Control> Pseudo(params string[] pseudoClasses) => control =>
    {
        foreach (var pseudoClass in pseudoClasses)
        {
            ((IPseudoClasses)control.Classes).Add(pseudoClass);
        }
    };

    private static CustomizableSizeType SizeOf(string token) =>
        token.EndsWith("LG", StringComparison.Ordinal) ? CustomizableSizeType.Large :
        token.EndsWith("SM", StringComparison.Ordinal) ? CustomizableSizeType.Small :
        CustomizableSizeType.Middle;

    /// <summary>Line heights only lower a vertical padding that is clamped at zero, so their effect shows when they shrink.</summary>
    private static readonly EffectOptions s_halve = new() { Perturb = static value => (double)value / 2 };

    /// <summary>The default/primary/danger shadows paint below the bounds; their offset and colour Tokens need the margin.</summary>
    private static readonly EffectOptions s_shadowMargin = new() { RegionInflation = 8 };

    // ---- Button / DropdownButton states ------------------------------------------------------

    /// <summary>
    /// States for Button and Button-derived members. DropdownButton sets <c>Height</c> per size instead of
    /// <c>MinHeight</c>; Tokens that only move the vertical padding release it with a local value.
    /// </summary>
    private static EffectSetup ButtonSetup(string token, Func<AtomButton> factory, bool isDropdown)
    {
        Func<Control> With(Action<AtomButton> init) => () =>
        {
            var button = factory();
            init(button);
            return button;
        };

        EffectSetup StatusSetup(string statusToken, ButtonVariant variant, string? pseudoClass)
        {
            var color = statusToken.StartsWith("ColorSuccess", StringComparison.Ordinal) ? ButtonColor.Success : ButtonColor.Warning;
            return new(With(b => Variant(b, color, variant)), pseudoClass is null ? null : Pseudo(pseudoClass));
        }

        void Release(AtomButton button)
        {
            if (isDropdown)
            {
                button.Height = double.NaN;
            }
            else
            {
                button.MinHeight = 0;
            }
        }

        static void IconOnly(AtomButton button)
        {
            button.Content = null;
            button.Icon    = new SearchOutlined();
        }

        var size = SizeOf(token);
        switch (token)
        {
            // Sizes
            case "ContentFontSizeLG" or "ContentFontSizeSM" or "ContentPaddingLG" or "ContentPaddingSM"
                or "ControlHeightLG" or "ControlHeightSM" or "FontSizeLG" or "BorderRadiusLG" or "BorderRadiusSM"
                or "ExtraContentMarginLG" or "ExtraContentMarginSM":
                return new(With(b => b.SizeType = size));
            case "ContentLineHeight" or "ContentLineHeightLG" or "ContentLineHeightSM":
                return new(With(b =>
                {
                    b.SizeType = size;
                    Release(b);
                }), Options: s_halve);
            case "IconOnyPadding" or "IconOnyPaddingLG" or "IconOnyPaddingSM":
                return new(With(b =>
                {
                    b.SizeType = size;
                    IconOnly(b);
                    Release(b);
                }));
            case "OnlyIconSize" or "OnlyIconSizeLG" or "OnlyIconSizeSM":
                return new(With(b =>
                {
                    b.SizeType = size;
                    IconOnly(b);
                    b.IsLoading = true;
                }));
            case "IconSize" or "IconSizeLG" or "IconSizeSM" or "IconMargin" or "UniformlyPaddingXXS":
                return new(With(b =>
                {
                    b.SizeType = size;
                    b.Icon     = new SearchOutlined();
                }));
            case "IconEndMargin":
                return new(With(static b =>
                {
                    b.Icon          = new SearchOutlined();
                    b.IconPlacement = ButtonIconPlacement.End;
                }));
            case "CirclePadding":
                return new(With(b =>
                {
                    b.Content = "A";
                    b.Shape   = ButtonShape.Circle;
                    Release(b);
                }));
            case "ColorBorder" when isDropdown:
                // DropdownButton reads ColorBorder only for its disabled border; its variant colours are Button's Own.
                return new(With(static b => b.IsEnabled = false));

            // Shadows painted outside the bounds
            case "DefaultShadow" or "ControlOutlineWidth" or "ColorControlOutline":
                return new(With(static _ => { }), Options: s_shadowMargin);

            // Default variant states
            case "DefaultHoverBg" or "DefaultHoverColor" or "DefaultHoverBorderColor":
                return new(With(static _ => { }), Pseudo(":pointerover"));
            case "DefaultActiveBg" or "DefaultActiveColor" or "DefaultActiveBorderColor":
                return new(With(static _ => { }), Pseudo(":pressed"));
            case "DefaultGhostColor" or "DefaultGhostBorderColor" or "GhostBg":
                return new(With(static b => b.IsGhost = true));
            case "DefaultBorderColorDisabled" or "ColorTextDisabled" or "ColorBgContainerDisabled":
                return new(With(static b => b.IsEnabled = false));
            case "BorderColorDisabled":
                // Button: the default colour has its own disabled border; DropdownButton uses one for every colour.
                return new(With(b =>
                {
                    b.IsEnabled = false;
                    if (!isDropdown)
                    {
                        b.ButtonType = ButtonType.Primary;
                    }
                }));
            case "OpacityLoading":
                return new(With(static b => b.IsLoading = true));
            case "FocusVisualBorderThickness" or "ColorFocusBorder":
                return new(With(static _ => { }), Pseudo(":focus-visible"));

            // Primary
            case "PrimaryColor" or "ColorPrimary" or "ColorTextLightSolid":
                return new(With(static b => b.ButtonType = ButtonType.Primary));
            case "PrimaryShadow":
                return new(With(static b => b.ButtonType = ButtonType.Primary), Options: s_shadowMargin);
            case "ColorPrimaryHover":
                return new(With(static b => b.ButtonType = ButtonType.Primary), Pseudo(":pointerover"));
            case "ColorPrimaryActive":
                return new(With(static b => b.ButtonType = ButtonType.Primary), Pseudo(":pressed"));
            case "ColorPrimaryBg":
                return new(With(static b => Variant(b, ButtonColor.Primary, ButtonVariant.Filled)));
            case "ColorPrimaryBgHover":
                return new(With(static b => Variant(b, ButtonColor.Primary, ButtonVariant.Filled)), Pseudo(":pointerover"));
            case "ColorPrimaryBorder":
                return new(With(static b => Variant(b, ButtonColor.Primary, ButtonVariant.Filled)), Pseudo(":pressed"));

            // Success / Warning: the semantic variant layout of Primary with the Global status Tokens.
            case "ColorSuccess" or "ColorWarning":
                return StatusSetup(token, ButtonVariant.Solid, null);
            case "ColorSuccessHover" or "ColorWarningHover":
                return StatusSetup(token, ButtonVariant.Solid, ":pointerover");
            case "ColorSuccessActive" or "ColorWarningActive":
                return StatusSetup(token, ButtonVariant.Solid, ":pressed");
            case "ColorSuccessBg" or "ColorWarningBg":
                return StatusSetup(token, ButtonVariant.Filled, null);
            case "ColorSuccessBgHover" or "ColorWarningBgHover":
                return StatusSetup(token, ButtonVariant.Filled, ":pointerover");
            case "ColorSuccessBorder" or "ColorWarningBorder":
                return StatusSetup(token, ButtonVariant.Filled, ":pressed");

            // Danger
            case "DangerColor":
                return new(With(static b =>
                {
                    b.ButtonType = ButtonType.Primary;
                    b.IsDanger   = true;
                }));
            case "DangerShadow" or "ColorErrorOutline":
                return new(With(static b =>
                {
                    b.ButtonType = ButtonType.Primary;
                    b.IsDanger   = true;
                }), Options: s_shadowMargin);
            case "ColorError":
                return new(With(static b => b.IsDanger = true));
            case "ColorErrorHover":
                return new(With(static b =>
                {
                    b.ButtonType = ButtonType.Primary;
                    b.IsDanger   = true;
                }), Pseudo(":pointerover"));
            case "ColorErrorActive":
                return new(With(static b => b.IsDanger = true), Pseudo(":pressed"));
            case "ColorErrorBorderHover":
                return new(With(static b => b.IsDanger = true), Pseudo(":pointerover"));
            case "ColorErrorBg":
                return new(With(static b => Variant(b, ButtonColor.Danger, ButtonVariant.Filled)));
            case "ColorErrorBgHover":
                return new(With(static b => Variant(b, ButtonColor.Danger, ButtonVariant.Filled)), Pseudo(":pointerover"));
            case "ColorErrorBgActive":
                return new(With(static b => Variant(b, ButtonColor.Danger, ButtonVariant.Filled)), Pseudo(":pressed"));

            // Solid (default colour)
            case "SolidTextColor" or "ColorBgSolid":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Solid)));
            case "ColorBgSolidHover":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Solid)), Pseudo(":pointerover"));
            case "ColorBgSolidActive":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Solid)), Pseudo(":pressed"));

            // Filled (default colour)
            case "ColorFillTertiary":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Filled)));
            case "ColorFillSecondary":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Filled)), Pseudo(":pointerover"));
            case "ColorFill":
                return new(With(static b => Variant(b, ButtonColor.Default, ButtonVariant.Filled)), Pseudo(":pressed"));

            // Text
            case "TextTextColor":
                return new(With(static b => b.ButtonType = ButtonType.Text));
            case "TextTextHoverColor" or "TextHoverBg":
                return new(With(static b => b.ButtonType = ButtonType.Text), Pseudo(":pointerover"));
            case "TextTextActiveColor":
                return new(With(static b => b.ButtonType = ButtonType.Text), Pseudo(":pressed"));

            // Link
            case "ColorLink":
                return new(With(static b => b.ButtonType = ButtonType.Link));
            case "ColorLinkHover" or "LinkHoverBg":
                return new(With(static b => b.ButtonType = ButtonType.Link), Pseudo(":pointerover"));
            case "ColorLinkActive":
                return new(With(static b => b.ButtonType = ButtonType.Link), Pseudo(":pressed"));

            default:
                return new(With(static _ => { }));
        }
    }

    private static void Variant(AtomButton button, ButtonColor color, ButtonVariant variant)
    {
        button.Color   = color;
        button.Variant = variant;
    }

    private static MenuFlyout CreateMenuFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = "First item" });
        flyout.Items.Add(new MenuItem { Header = "Second item" });
        return flyout;
    }

    // ---- HyperLinkButton states --------------------------------------------------------------

    private static EffectSetup HyperLinkButtonSetup(string token)
    {
        Func<Control> With(Action<HyperLinkButton> init) => () =>
        {
            var button = new HyperLinkButton { Content = "Link" };
            init(button);
            return button;
        };

        var size = SizeOf(token);
        switch (token)
        {
            case "ContentFontSizeLG" or "ContentFontSizeSM" or "ContentPaddingLG" or "ContentPaddingSM"
                or "ControlHeightLG" or "ControlHeightSM" or "FontSizeLG":
                return new(With(b => b.SizeType = size));
            case "ContentLineHeight" or "ContentLineHeightLG" or "ContentLineHeightSM":
                // The size Setter fixes Height; a local NaN releases it so the vertical padding shows.
                return new(With(b =>
                {
                    b.SizeType = size;
                    b.Height   = double.NaN;
                }), Options: s_halve);
            case "IconOnyPadding" or "IconOnyPaddingLG" or "IconOnyPaddingSM":
                return new(With(b =>
                {
                    b.SizeType = size;
                    b.Content  = null;
                    b.Icon     = new SearchOutlined();
                    b.Height   = double.NaN;
                }));
            case "OnlyIconSize" or "OnlyIconSizeLG" or "OnlyIconSizeSM":
                return new(With(b =>
                {
                    b.SizeType = size;
                    b.Content  = null;
                    b.Icon     = new SearchOutlined();
                }));
            case "IconSize" or "IconSizeLG" or "IconSizeSM" or "IconMargin" or "UniformlyPaddingXXS":
                return new(With(b =>
                {
                    b.SizeType = size;
                    b.Icon     = new SearchOutlined();
                }));
            case "ColorLinkHover":
                return new(With(static _ => { }), Pseudo(":pointerover"));
            case "ColorLinkActive":
                return new(With(static _ => { }), Pseudo(":pressed"));
            case "ColorError":
                return new(With(static b => b.IsDanger = true));
            case "ColorErrorHover":
                return new(With(static b => b.IsDanger = true), Pseudo(":pointerover"));
            case "ColorErrorActive":
                return new(With(static b => b.IsDanger = true), Pseudo(":pressed"));
            case "ColorTextDisabled":
                return new(With(static b => b.IsEnabled = false));
            case "OpacityLoading":
                return new(With(static b => b.IsLoading = true));
            default:
                return new(With(static _ => { }));
        }
    }

    // ---- SplitButton states ------------------------------------------------------------------

    private static EffectSetup SplitButtonSetup(string token)
    {
        Func<Control> With(Action<SplitButton> init) => () =>
        {
            var button = new SplitButton { Content = "Split" };
            init(button);
            return button;
        };

        switch (token)
        {
            case "BorderRadiusLG" or "BorderRadiusSM":
                return new(With(b => b.SizeType = SizeOf(token)));
            // The Ant baseline derives both separator slots in SplitButtonToken from these globals, so they still
            // have a code read and need the filled form to show.
            case "SolidSeparatorColor" or "ColorPrimaryHover":
                return new(With(static b => b.IsPrimaryButtonType = true));
            case "DangerSeparatorColor" or "ColorErrorHover":
                return new(With(static b =>
                {
                    b.IsPrimaryButtonType = true;
                    b.IsDanger            = true;
                }));
            case "BorderThickness":
                // The primary form spaces its two Buttons by the border width; the outline form overlaps them.
                return new(With(static b => b.IsPrimaryButtonType = true));
            default:
                return new(With(static _ => { }));
        }
    }

    // ---- IconButton / ToggleIconButton states ------------------------------------------------

    private static EffectSetup IconButtonSetup(string token, Func<Control> factory)
    {
        switch (token)
        {
            case "ColorIconHover":
                return new(factory, Pseudo(":pointerover"));
            case "ColorText":
                return new(factory, Pseudo(":pressed"));
            case "ColorTextDisabled":
                return new(() =>
                {
                    var control = factory();
                    control.IsEnabled = false;
                    return control;
                });
            case "BorderRadiusSM":
                // The theme background is transparent; the radius shows on a user background.
                return new(() =>
                {
                    var control = factory();
                    ((TemplatedControl)control).Background = Avalonia.Media.Brushes.SteelBlue;
                    return control;
                });
            default:
                return new(factory);
        }
    }

    // ---- OptionButton / OptionButtonGroup states ---------------------------------------------

    private static EffectSetup OptionButtonSetup(string token, bool targetsButton)
    {
        Func<Control> With(Action<OptionButtonGroup> init) => () =>
        {
            var group = new OptionButtonGroup();
            group.Items.Add(new OptionButton { Content = "First" });
            group.Items.Add(new OptionButton { Content = "Second" });
            init(group);
            return group;
        };

        // The group always keeps one option checked (the first); unchecked states are read on the second option.
        var uncheckedToken = token is "ButtonColor" or "ColorText" or "ColorBorder" or "ColorBgContainerDisabled";
        var options = targetsButton
            ? new EffectOptions
            {
                Target = uncheckedToken ? static root => OptionAt(root, 1) : static root => OptionAt(root, 0)
            }
            : null;
        EffectOptions? Options(EffectOptions? extra = null) => extra is null
            ? options
            : options is null
                ? extra
                : extra with { Target = options.Target };

        Action<Control> OnOption(Action<OptionButton> action) => control =>
            action(control as OptionButton ?? OptionAt(control, 0));

        var size = SizeOf(token);
        switch (token)
        {
            case "BorderRadius" or "BorderRadiusLG" or "BorderRadiusSM" when !targetsButton:
                // The group frame is a line; a user background makes its rounded corners visible.
                return new(With(g =>
                {
                    g.SizeType   = size;
                    g.Background = Avalonia.Media.Brushes.SteelBlue;
                }));
            case "BorderRadius" or "BorderRadiusLG" or "BorderRadiusSM":
                // The checked solid option fills its rounded outer corners.
                return new(With(g =>
                {
                    g.SizeType    = size;
                    g.ButtonStyle = OptionButtonStyle.Solid;
                }), Options: Options());
            case "ColorText" when targetsButton:
                return new(With(static g => g.ButtonStyle = OptionButtonStyle.Solid), Options: Options());
            case "ColorBorder" when targetsButton:
                // Options draw their own border only when given a border width; the group frame is drawn otherwise.
                return new(With(static g => ((OptionButton)g.Items[1]!).BorderThickness = new Avalonia.Thickness(1)),
                    Options: Options());
            case "ContentFontSizeLG" or "ContentFontSizeSM" or "ContentPaddingLG" or "ContentPaddingSM"
                or "ControlHeightLG" or "ControlHeightSM" or "FontSizeLG" or "BorderRadiusLG" or "BorderRadiusSM"
                or "IconSizeLG" or "IconSizeSM":
                return new(With(g =>
                {
                    g.SizeType = size;
                    if (token.StartsWith("IconSize", StringComparison.Ordinal))
                    {
                        ((OptionButton)g.Items[0]!).Icon = new SearchOutlined();
                    }
                }), Options: Options());
            case "ContentLineHeight" or "ContentLineHeightLG" or "ContentLineHeightSM":
                return new(With(g =>
                {
                    g.SizeType = size;
                    g.Height   = double.NaN;
                }), Options: Options(s_halve));
            case "SpacingXXS":
                return new(With(static g => ((OptionButton)g.Items[0]!).Icon = new SearchOutlined()), Options: Options());
            case "ButtonSolidCheckedColor" or "ButtonSolidCheckedBackground" or "ColorTextLightSolid":
                return new(With(static g => g.ButtonStyle = OptionButtonStyle.Solid),
                    OnOption(static b => b.IsChecked = true), Options());
            case "ButtonSolidCheckedHoverBackground":
                return new(With(static g => g.ButtonStyle = OptionButtonStyle.Solid),
                    OnOption(static b =>
                    {
                        b.IsChecked = true;
                        ((IPseudoClasses)b.Classes).Add(":pointerover");
                    }), Options());
            case "ButtonSolidCheckedActiveBackground":
                return new(With(static g => g.ButtonStyle = OptionButtonStyle.Solid),
                    OnOption(static b =>
                    {
                        b.IsChecked = true;
                        ((IPseudoClasses)b.Classes).Add(":pressed");
                    }), Options());
            case "ButtonCheckedBgDisabled" or "ButtonCheckedColorDisabled" or "ControlItemBgActiveDisabled":
                return new(With(static g => g.IsEnabled = false), OnOption(static b => b.IsChecked = true), Options());
            case "ColorTextDisabled" or "ColorBgContainerDisabled":
                return new(With(static g => g.IsEnabled = false), Options: Options());
            case "ColorPrimary":
                return new(With(static _ => { }), OnOption(static b => b.IsChecked = true), Options());
            case "ColorPrimaryHover":
                return new(With(static _ => { }), OnOption(static b => ((IPseudoClasses)b.Classes).Add(":pointerover")), Options());
            case "ColorPrimaryActive":
                return new(With(static _ => { }), OnOption(static b => ((IPseudoClasses)b.Classes).Add(":pressed")), Options());
            case "IconSize":
                return new(With(static g => ((OptionButton)g.Items[0]!).Icon = new SearchOutlined()), Options: Options());
            default:
                return new(With(static _ => { }), Options: Options());
        }
    }

    private static OptionButton OptionAt(Control root, int index) =>
        root.GetVisualDescendants().OfType<OptionButton>().ElementAt(index);
}
