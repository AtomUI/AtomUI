using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Controls;
using Avalonia.Headless;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class ColorPickerFamilyEffectTests
{
    public static IEnumerable<object[]> Rows() => new[] { "ColorPicker", "GradientColorPicker", "ColorPickerView", "GradientColorPickerView" }
        .SelectMany(id => EffectTestData.For("AtomUI", id).Select(row => (string)row[0]).Concat(OwnerGlobals(id)).Distinct().Select(token => new object[] { id, token }));

    [Theory]
    [MemberData(nameof(Rows))]
    public void Terminal_Token_Changes_Actual_Visuals(string id, string token)
    {
        IDisposable? contact = null;
        try
        {
            TokenEffectHarness.AssertHasEffect(
                () => token == "UniformlyMarginXXS" ? FeedbackFamilyEffectTests.OverlayLayerHost(Create(id, token), 700, 550) : Create(id, token),
                Identity(id), token, control => Arrange(control, token, value => contact = value),
                // The default 1px inset has sub-8-channel antialiasing; compare its real 8px paint rectangle.
                channelTolerance: token == "ColorTextQuaternary" ? 1 : TokenEffectHarness.DefaultChannelTolerance,
                options: new EffectOptions
                {
                    Target = token == "UniformlyMarginXXS" ? root => root.GetVisualDescendants().OfType<AbstractColorPicker>().Single() : null,
                    Region = control => Region(control, token),
                    Perturb = token.Contains("Radius", StringComparison.Ordinal) ? _ => new CornerRadius(20) : null
                });
        }
        finally
        {
            contact?.Dispose();
        }
    }

    private static ControlTokenIdentity Identity(string id) => id switch
    {
        "ColorPicker" => ColorPickerTokens.Identity,
        "GradientColorPicker" => GradientColorPickerTokens.Identity,
        "ColorPickerView" => ColorPickerViewTokens.Identity,
        _ => GradientColorPickerViewTokens.Identity
    };

    private static Control Create(string id, string token)
    {
        Control control = id switch
        {
            "ColorPicker" => new ColorPicker { Value = Color.FromArgb(100, 120, 60, 180), IsTextVisible = true },
            "GradientColorPicker" => new GradientColorPicker { IsTextVisible = true, Value = Gradient() },
            "ColorPickerView" => new ColorPickerView { Value = Color.FromArgb(100, 120, 60, 180) },
            _ => new GradientColorPickerView { Value = Gradient() }
        };
        if (control is AbstractColorPicker picker)
        {
            picker.SizeType = token.EndsWith("LG") || token is "ControlHeight" ? CustomizableSizeType.Large : token is "FontSizeSM" or "BorderRadiusXS" or "ControlHeightXS" ? CustomizableSizeType.Small : CustomizableSizeType.Middle;
            if (token is "ColorError") { picker.Status = InputControlStatus.Error; }
            if (token is "ColorWarning") { picker.Status = InputControlStatus.Warning; }
        }
        if (control is AbstractColorPickerView view)
        {
            view.IsClearEnabled = true;
            view.IsPaletteGroupEnabled = true;
            view.PaletteGroup = [new ColorPickerPalette("Palette", true, [Colors.Red, Colors.Blue])];
        }
        return control;
    }

    private static void Arrange(Control control, string token, Action<IDisposable>? trackContact = null)
    {
        if (control is AbstractColorPicker picker)
        {
            if (token is "UniformlyMarginXXS")
            {
                picker.IsPopupPinnedOpen = true;
                picker.IsPickerOpen = true;
            }
            if (token is "LineWidthBold")
            {
                if (picker is ColorPicker colour) { colour.Value = null; colour.DefaultValue = null; }
                if (picker is GradientColorPicker gradient) { gradient.Value = null; gradient.DefaultValue = null; }
            }
        }
        if (control is AbstractColorPickerView)
        {
            var clear = control.GetVisualDescendants().OfType<ColorBlock>().Single(c => c.Name == "PART_ClearColor");
            if (token is "ControlHeight" or "BorderRadius") { clear.SizeType = CustomizableSizeType.Large; }
            if (token is "ControlHeightXS" or "BorderRadiusXS") { clear.SizeType = CustomizableSizeType.Small; }
            if (token is "ColorBorder") { ((IPseudoClasses)clear.Classes).Add(":pointerover"); }
        }
        if (token is "ColorBlockDisabledOpacity" or "ColorTextDisabled" or "ColorBgContainerDisabled")
        {
            control.IsEnabled = false;
        }
        if (token is "ColorPrimaryHover")
        {
            ((IPseudoClasses)control.Classes).Add(":pointerover");
        }
        if (token is "ColorPickerInputNumberHandleWidth")
        {
            var number = control.GetVisualDescendants().OfType<NumericUpDown>().First(n => n.IsVisible);
            var frame = number.GetVisualDescendants().OfType<ButtonSpinnerDecoratedBox>().Single();
            var window = (Avalonia.Controls.Window)TopLevel.GetTopLevel(control)!;
            window.MouseMove(frame.TranslatePoint(new Point(8, 8), window)!.Value);
            frame.HandleOpacity.ShouldBe(1);
        }
        if (token is "CheckedMarkSize" or "ColorWhite" or "ColorTextSecondary")
        {
            var item = control.GetVisualDescendants().OfType<PaletteColorItem>().First();
            item.SetCurrentValue(PaletteColorItem.IsCheckedProperty, true);
            item.SetCurrentValue(PaletteColorItem.ColorProperty, token == "ColorTextSecondary" ? Colors.White : Colors.DarkBlue);
        }
        if (token is "ColorFill")
        {
            ((IPseudoClasses)control.GetVisualDescendants().OfType<PaletteColorItem>().First().Classes).Add(":pointerover");
        }
        if (token is "ColorPrimaryActive")
        {
            ((IPseudoClasses)control.GetVisualDescendants().OfType<ColorSliderThumb>().First().Classes).Add(":pressed");
        }
        if (token is "ColorPickerHandlerLightColor")
        {
            ((IPseudoClasses)control.GetVisualDescendants().OfType<ColorSpectrum>().Single().Classes).Add(":light-selector");
        }
        if (token is "ColorPickerHandlerDarkColor")
        {
            ((IPseudoClasses)control.GetVisualDescendants().OfType<ColorSpectrum>().Single().Classes).Remove(":light-selector");
        }
        if (token is "ColorPickerHandlerSizeLG")
        {
            var spectrum = control.GetVisualDescendants().OfType<ColorSpectrum>().Single();
            var window = (Avalonia.Controls.Window)TopLevel.GetTopLevel(control)!;
            trackContact?.Invoke(window.TouchBegin(spectrum.TranslatePoint(new Point(spectrum.Bounds.Width / 2, spectrum.Bounds.Height / 2), window)!.Value));
            spectrum.Classes.ShouldContain(":large-selector");
        }
    }

    private static LinearGradientBrush Gradient() => new() { GradientStops = new GradientStops { new(Color.FromArgb(100, 255, 0, 0), 0), new(Color.FromArgb(100, 0, 0, 255), 1) } };

    private static Control Region(Control control, string token)
    {
        if (control is AbstractColorPickerView)
        {
            if (token is "SpacingXXS")
            {
                return control.GetVisualDescendants().OfType<ColorPickerPaletteGroup>().Single().GetVisualDescendants().OfType<WrapPanel>().Single();
            }
            if (token is "CheckedMarkSize" or "ColorWhite" or "ColorTextSecondary" or "ColorFill" or "ColorPickerPresetColorSize" or "ColorPickerPresetColorGroupPadding" or "ColorPickerPresetPanelWidth" or "SpacingXXS")
            {
                return token is "ColorPickerPresetPanelWidth" or "ColorPickerPresetColorGroupPadding" ? control.GetVisualDescendants().OfType<ColorPickerPaletteGroup>().Single() : control.GetVisualDescendants().OfType<PaletteColorItem>().First();
            }
            if (token is "ColorPickerAlphaInputWidth" or "ColorPickerInputNumberHandleWidth" or "EditorFontSize")
            {
                return control.GetVisualDescendants().OfType<ColorPickerInput>().Single();
            }
            if (token is "ColorPrimaryActive" or "ColorBgElevated" or "ColorPickerHandlerSizeSM")
            {
                return control.GetVisualDescendants().OfType<ColorSliderThumb>().First();
            }
            if (token.StartsWith("ColorPickerHandler", StringComparison.Ordinal) || token is "LineWidthBold")
            {
                return control.GetVisualDescendants().OfType<Panel>().Single(c => c.Name == "PART_SelectionEllipsePanel");
            }
            if (token is "ColorPickerInsetShadow" or "ColorTextQuaternary")
            {
                return control.GetVisualDescendants().OfType<ColorSlider>().First().GetVisualDescendants().OfType<Avalonia.Controls.Border>().Single(border => border.BoxShadow.Count > 0);
            }
            if (token is "ColorError" or "ColorBorder" or "ControlHeight" or "ControlHeightXS" or "BorderRadius" or "BorderRadiusXS")
            {
                return control.GetVisualDescendants().OfType<ColorBlock>().Single(c => c.Name == "PART_ClearColor");
            }
            if (token is "TransparentBgSize" or "SizeXS")
            {
                return control.GetVisualDescendants().OfType<ColorSlider>().Single(c => c.Name == "ColorSpectrumAlphaSlider");
            }
            if (token is "UniformlyPaddingXXS")
            {
                return control.GetVisualDescendants().OfType<ColorPickerPaletteGroup>().Single();
            }
            if (token is "ColorBorderSecondary")
            {
                return control.GetVisualDescendants().OfType<Control>().Single(c => c.Name == "Separator");
            }
        }
        if (token == "UniformlyMarginXXS" && control is AbstractColorPicker picker)
        {
            return picker.PickerPresenter!;
        }
        return control;
    }
}
