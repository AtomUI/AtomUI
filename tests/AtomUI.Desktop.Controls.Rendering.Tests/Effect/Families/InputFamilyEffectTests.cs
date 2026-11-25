using Avalonia;
using Avalonia.Headless;
using Shouldly;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the input family (docs/engineering/development/theme-effect-tests.md): every Active Own Token and
/// every consumed global Token of each member visibly changes the member when overridden through <c>Controls[member]</c>.
/// The rows come from the generated consumption map plus the owner reads of the primitives a member hosts
/// (<see cref="RowsOf"/>); <see cref="Setup"/> puts the member into a state where the Token shows. A Token without an entry
/// is checked in the member's default state.
/// </summary>
public partial class InputFamilyEffectTests
{
    private const string SampleText = "Input text";

    public static IEnumerable<object[]> TextBoxRows() => RowsOf("TextBox");
    public static IEnumerable<object[]> LineEditRows() => RowsOf("LineEdit");
    public static IEnumerable<object[]> SearchEditRows() => RowsOf("SearchEdit");
    public static IEnumerable<object[]> TextAreaRows() => RowsOf("TextArea");
    public static IEnumerable<object[]> ButtonSpinnerRows() => RowsOf("ButtonSpinner");
    public static IEnumerable<object[]> NumericUpDownRows() => RowsOf("NumericUpDown");

    [Theory]
    [MemberData(nameof(TextBoxRows))]
    public void TextBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("TextBox", token), TextBoxTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(LineEditRows))]
    public void LineEdit_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("LineEdit", token), LineEditTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(SearchEditRows))]
    public void SearchEdit_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("SearchEdit", token), SearchEditTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(TextAreaRows))]
    public void TextArea_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("TextArea", token), TextAreaTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(ButtonSpinnerRows))]
    public void ButtonSpinner_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("ButtonSpinner", token), ButtonSpinnerTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(NumericUpDownRows))]
    public void NumericUpDown_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(Setup("NumericUpDown", token), NumericUpDownTokens.Identity, token);
    }

    // ---- shared helpers -----------------------------------------------------------------------

    internal sealed record EffectSetup(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static void Run(EffectSetup setup, ControlTokenIdentity identity, string token)
    {
        TokenEffectHarness.AssertHasEffect(setup.Create, identity, token, setup.Arrange, options: setup.Options);
    }

    private static Control Create(string host) => host switch
    {
        "TextBox"       => new TextBox { Width = 260, Text = SampleText },
        "LineEdit"      => new LineEdit { Width = 260, Text = SampleText },
        "SearchEdit"    => new SearchEdit { Width = 300, Text = SampleText },
        "TextArea"      => new TextArea { Width = 260, Text = SampleText + "\nSecond line" },
        "ButtonSpinner" => new ButtonSpinner { Width = 200, Content = "12" },
        "NumericUpDown" => new NumericUpDown { Width = 200, Value = 12 },
        _               => throw new ArgumentOutOfRangeException(nameof(host), host, null)
    };

    private static CustomizableSizeType SizeOf(string token) =>
        // UniformlyPaddingSM is a base of the middle content padding, not a small-size Token.
        token == "UniformlyPaddingSM" ? CustomizableSizeType.Middle :
        token.EndsWith("LG", StringComparison.Ordinal) ? CustomizableSizeType.Large :
        token.EndsWith("SM", StringComparison.Ordinal) ? CustomizableSizeType.Small :
        CustomizableSizeType.Middle;

    /// <summary>
    /// The state in which <paramref name="token"/> shows on <paramref name="host"/>. <c>init</c> steps run on the created
    /// control before it is shown, <c>arrange</c> steps once its template exists.
    /// </summary>
    private static EffectSetup Setup(string host, string token)
    {
        var init    = new List<Action<Control>>();
        var arrange = new List<Action<Control>>();
        EffectOptions? options = null;
        var size    = SizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            init.Add(c => SetSize(c, size));
        }

        void Variant(InputControlStyleVariant variant) => init.Add(c => SetVariant(c, variant));
        void Status(InputControlStatus status) => init.Add(c => SetStatus(c, status));
        void Hover() => arrange.Add(static c => FrameOf(c).IsInnerBoxHover = true);
        void Focus() => arrange.Add(static c => FrameOf(c).IsInputFocusWithin = true);
        void Disabled() => init.Add(static c => c.IsEnabled = false);
        void LeftAddOn(object content) => init.Add(c => SetLeftAddOn(c, content));
        // TextArea has no outer add-ons; its prefix presenter receives the same status colours.
        void StatusAddOn()
        {
            if (host == "TextArea")
            {
                InnerLeft();
            }
            else
            {
                LeftAddOn("http://");
            }
        }
        void InnerLeft() => init.Add(static c => SetInner(c, left: "$"));
        void InnerRight() => init.Add(static c => SetInner(c, right: "kg"));
        void Clear() => init.Add(static c => SetAllowClear(c));
        void Reveal() => init.Add(static c =>
        {
            var input = (AbstractTextInput)c;
            input.PasswordChar         = '*';
            input.IsEnableRevealButton = true;
        });
        void OnPart<T>(string name, string pseudoClass) where T : Control => arrange.Add(c =>
            ((IPseudoClasses)c.GetVisualDescendants().OfType<T>().First(part => part.Name == name).Classes).Add(pseudoClass));
        void SearchButton(string pseudoClass) => OnPart<AtomButton>("PART_RightAddOn", pseudoClass);
        void SpinnerHandle(string pseudoClass)
        {
            if (host == "NumericUpDown")
            {
                // The floatable handle of the input mode only shows while the pointer is over the content.
                arrange.Add(static c =>
                {
                    var frame = c.GetVisualDescendants().OfType<ButtonSpinnerDecoratedBox>().First();
                    var window = (Avalonia.Controls.Window)TopLevel.GetTopLevel(c)!;
                    window.MouseMove(frame.TranslatePoint(new Point(8, 8), window)!.Value);
                    // The handle stays in view in every state, disabled included (Ant greys the handlers).
                    frame.HandleOpacity.ShouldBe(1d);
                });
            }
            if (pseudoClass.Length > 0)
            {
                OnPart<IconButton>("PART_IncreaseButton", pseudoClass);
                // The handle icons are a few pixels inside the whole input; compare the handle itself.
                options = new EffectOptions
                {
                    Region = static c => c.GetVisualDescendants().OfType<ButtonSpinnerHandle>().First()
                };
            }
        }

        switch (host, token)
        {
            // ---- ButtonSpinner / NumericUpDown handle ----
            case ("ButtonSpinner" or "NumericUpDown", "HandleWidth" or "HandleBorderColor" or "HandleBg" or "HandleIconSize"
                or "ControlHeightSM" or "FontSize" or "BorderThickness" or "ColorBgContainer"):
                SpinnerHandle(string.Empty);
                break;
            case ("ButtonSpinner" or "NumericUpDown", "FilledHandleBg"):
                Variant(InputControlStyleVariant.Filled);
                SpinnerHandle(string.Empty);
                break;
            case ("ButtonSpinner" or "NumericUpDown", "HandleHoverColor"):
                SpinnerHandle(":pointerover");
                break;
            case ("ButtonSpinner" or "NumericUpDown", "HandleHoverBg"):
                SpinnerHandle(":pointerover");
                break;
            case ("ButtonSpinner" or "NumericUpDown", "HandlePressedColor"):
                SpinnerHandle(":pressed");
                break;
            case ("ButtonSpinner" or "NumericUpDown", "HandleIconColor" or "ColorIcon"):
                // The resting icon colour shows on the two arrow glyphs only: compare the handle, not the whole input.
                SpinnerHandle(string.Empty);
                options = new EffectOptions
                {
                    Region = static c => c.GetVisualDescendants().OfType<ButtonSpinnerHandle>().First()
                };
                break;
            case ("ButtonSpinner" or "NumericUpDown", "HandleActiveBg" or "ColorPrimaryActive"):
                SpinnerHandle(":pressed");
                break;
            case ("ButtonSpinner", "IconSize" or "IconSizeLG" or "IconSizeSM"):
                LeftAddOn(new SearchOutlined());
                break;

            // ---- TextArea ----
            case ("TextArea", "ResizeIndicatorLineColor" or "ResizeHandleSize" or "ColorTextDescription" or "LineWidth"
                or "SizeXS"):
                init.Add(static c => ((TextArea)c).IsResizable = true);
                // The indicator is a few strokes in the corner of a large surface; compare the handle that draws it.
                options = new EffectOptions
                {
                    Region           = static c => c.GetVisualDescendants().OfType<ResizeHandle>().First(),
                    RegionInflation  = 2
                };
                break;
            case ("TextArea", "RightAddOnPadding" or "RightAddOnPaddingLG" or "RightAddOnPaddingSM" or "UniformlyPaddingSM"
                or "ControlPaddingHorizontalSM"):
                InnerRight();
                break;
            case ("TextArea", "ControlPaddingHorizontal"):
                init.Add(static c => SetSize(c, CustomizableSizeType.Large));
                InnerRight();
                break;
            case ("TextArea", "LeftInnerAddOnMargin" or "UniformlyMarginXXS"):
                InnerLeft();
                break;

            // ---- TextBox content padding derivation ----
            case ("TextBox", "ControlPaddingHorizontal"):
                init.Add(static c => SetSize(c, CustomizableSizeType.Large));
                break;

            // ---- SearchEdit button ----
            case ("SearchEdit", "ColorErrorBgActive"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Error);
                SearchButton(":pressed");
                break;
            case ("SearchEdit", "ColorWarningHover"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Warning);
                SearchButton(":pointerover");
                break;
            case ("SearchEdit", "ColorWarningActive"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Warning);
                SearchButton(":pressed");
                break;
            case ("SearchEdit", "ColorBgTextHover"):
                Variant(InputControlStyleVariant.Borderless);
                SearchButton(":pointerover");
                break;
            case ("SearchEdit", "ColorBgTextActive"):
                Variant(InputControlStyleVariant.Borderless);
                SearchButton(":pressed");
                break;
            case ("SearchEdit", "ColorTextDescription"):
                Variant(InputControlStyleVariant.Borderless);
                break;

            // ---- frame states ----
            case (_, "ColorPrimaryHover"):
                Hover();
                break;
            case (_, "ColorPrimary" or "InputActiveBorderColor" or "InputActiveShadow"):
                Focus();
                break;
            case (_, "ColorError"):
                Status(InputControlStatus.Error);
                break;
            case (_, "ColorErrorBorderHover"):
                Status(InputControlStatus.Error);
                Hover();
                break;
            case (_, "InputErrorActiveShadow"):
                Status(InputControlStatus.Error);
                Focus();
                break;
            case (_, "ColorWarning"):
                Status(InputControlStatus.Warning);
                break;
            case (_, "ColorWarningBorderHover"):
                Status(InputControlStatus.Warning);
                Hover();
                break;
            case (_, "InputWarningActiveShadow"):
                Status(InputControlStatus.Warning);
                Focus();
                break;
            case (_, "ColorFillTertiary"):
                Variant(InputControlStyleVariant.Filled);
                break;
            case (_, "ColorFillSecondary"):
                Variant(InputControlStyleVariant.Filled);
                Hover();
                break;
            case (_, "ColorErrorBg"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Error);
                break;
            case (_, "ColorErrorBgHover"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Error);
                Hover();
                break;
            case (_, "ColorWarningBg"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Warning);
                break;
            case (_, "ColorWarningBgHover"):
                Variant(InputControlStyleVariant.Filled);
                Status(InputControlStatus.Warning);
                Hover();
                break;
            case (_, "ColorTextDisabled" or "ColorBgContainerDisabled"):
                Disabled();
                if (host is "ButtonSpinner" or "NumericUpDown")
                {
                    SpinnerHandle(string.Empty);
                }
                break;
            case (_, "ColorErrorText"):
                Status(InputControlStatus.Error);
                StatusAddOn();
                break;
            case (_, "ColorWarningText"):
                Status(InputControlStatus.Warning);
                StatusAddOn();
                break;

            // ---- add-ons and inner parts ----
            case (_, "ColorFillAlter"):
                LeftAddOn("http://");
                break;
            case (_, "SpacingXXS"):
                InnerLeft();
                break;
            case (_, "ColorTextQuaternary" or "UniformlyPaddingXXS"):
                InnerRight();
                Clear();
                break;
            case (_, "IconSize"):
                Clear();
                break;
            case (_, "ColorTextTertiary"):
                Clear();
                OnPart<InputClearIconButton>("PART_ClearButton", ":pointerover");
                break;
            case (_, "ColorIcon"):
                Reveal();
                break;
            case (_, "ColorIconHover"):
                Reveal();
                OnPart<RevealButton>("PART_RevealButton", ":pointerover");
                break;
            case (_, "InputContentMargin"):
                init.Add(static c => SetSize(c, CustomizableSizeType.Small));
                break;

            // ---- text ----
            case (_, "SelectionBackground" or "SelectionForeground"):
                arrange.Add(static c => SelectAll(c));
                break;
            case (_, "ColorTextPlaceholder"):
                init.Add(static c => SetPlaceholder(c));
                break;
        }

        return new EffectSetup(
            () =>
            {
                var control = Create(host);
                foreach (var step in init)
                {
                    step(control);
                }

                return control;
            },
            arrange.Count == 0
                ? null
                : control =>
                {
                    foreach (var step in arrange)
                    {
                        step(control);
                    }
                },
            options);
    }

    /// <summary>The input surface of the host: the outermost frame of its template.</summary>
    private static InputControlFrame FrameOf(Control control) =>
        control.GetVisualDescendants().OfType<InputControlFrame>().First();

    private static void SetSize(Control control, CustomizableSizeType size)
    {
        switch (control)
        {
            case AbstractTextInput input:
                input.SizeType = size;
                break;
            case ButtonSpinner spinner:
                spinner.SizeType = size;
                break;
            case NumericUpDown numeric:
                numeric.SizeType = size;
                break;
        }
    }

    private static void SetVariant(Control control, InputControlStyleVariant variant)
    {
        switch (control)
        {
            case AbstractTextInput input:
                input.StyleVariant = variant;
                break;
            case ButtonSpinner spinner:
                spinner.StyleVariant = variant;
                break;
            case NumericUpDown numeric:
                numeric.StyleVariant = variant;
                break;
        }
    }

    private static void SetStatus(Control control, InputControlStatus status)
    {
        switch (control)
        {
            case AbstractTextInput input:
                input.Status = status;
                break;
            case ButtonSpinner spinner:
                spinner.Status = status;
                break;
            case NumericUpDown numeric:
                numeric.Status = status;
                break;
        }
    }

    private static void SetLeftAddOn(Control control, object content)
    {
        switch (control)
        {
            case LineEdit lineEdit:
                lineEdit.LeftAddOn = content;
                break;
            case ButtonSpinner spinner:
                spinner.LeftAddOn = content;
                break;
            case NumericUpDown numeric:
                numeric.LeftAddOn = content;
                break;
            default:
                throw new NotSupportedException($"{control.GetType().Name} has no left add-on.");
        }
    }

    private static void SetInner(Control control, object? left = null, object? right = null)
    {
        switch (control)
        {
            case Avalonia.Controls.TextBox input:
                input.InnerLeftContent  ??= left;
                input.InnerRightContent ??= right;
                break;
            case ButtonSpinner spinner:
                spinner.InnerLeftContent  ??= left;
                spinner.InnerRightContent ??= right;
                break;
            case Avalonia.Controls.NumericUpDown numeric:
                numeric.InnerLeftContent  ??= left;
                numeric.InnerRightContent ??= right;
                break;
        }
    }

    private static void SetAllowClear(Control control)
    {
        switch (control)
        {
            case AbstractTextInput input:
                input.IsAllowClear = true;
                break;
            case NumericUpDown numeric:
                numeric.IsAllowClear = true;
                break;
        }
    }

    private static void SetPlaceholder(Control control)
    {
        switch (control)
        {
            case AbstractTextInput input:
                input.Text            = null;
                input.PlaceholderText = "Placeholder";
                break;
            case NumericUpDown numeric:
                numeric.Value           = null;
                numeric.PlaceholderText = "Placeholder";
                break;
        }
    }

    private static void SelectAll(Control control)
    {
        var input = control as Avalonia.Controls.TextBox ??
                    control.GetVisualDescendants().OfType<Avalonia.Controls.TextBox>().First();
        input.SelectAll();
    }
}
