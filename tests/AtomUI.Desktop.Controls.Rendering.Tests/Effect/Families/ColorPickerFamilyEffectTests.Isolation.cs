using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using static AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families.FeedbackFamilyEffectTests;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class ColorPickerFamilyEffectTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_Pairs_Isolate_Own_And_Global_And_Follow_Root(bool views)
    {
        var source = Create(views ? "ColorPickerView" : "ColorPicker", "ColorBorder");
        var target = Create(views ? "GradientColorPickerView" : "GradientColorPicker", "ColorBorder");
        var sourceId = Identity(views ? "ColorPickerView" : "ColorPicker");
        var targetId = Identity(views ? "GradientColorPickerView" : "GradientColorPicker");
        var token = views ? "ColorPickerHandlerDarkColor" : "ColorBorder";
        var own = views ? "ColorPickerWidth" : "TriggerPadding";
        WithIsolationWindow(new StackPanel { Children = { source, target } }, (provider, capture) =>
        {
            using var before = capture();
            if (views)
            {
                Arrange(source, token);
                Arrange(target, token);
            }
            using var arranged = capture();
            provider.Config = IsolationConfig(control: (sourceId, token, "#ff00b96b"));
            using var sourceChanged = capture();
            ChangedRatioOf(arranged, sourceChanged, Region(source, token)).ShouldBeGreaterThan(0.005);
            ChangedRatioOf(arranged, sourceChanged, Region(target, token)).ShouldBe(0);
            provider.Config = IsolationConfig(control: (targetId, token, "#ff00b96b"));
            using var targetChanged = capture();
            ChangedRatioOf(arranged, targetChanged, Region(source, token)).ShouldBe(0);
            ChangedRatioOf(arranged, targetChanged, Region(target, token)).ShouldBeGreaterThan(0.005);
            provider.Config = IsolationConfig(control: (sourceId, own, views ? "330" : "16"));
            using var ownSource = capture();
            if (views)
            {
                source.GetValue(ColorPickerThemeInputs.ThemeColorPickerWidthProperty).ShouldBe(330);
                target.GetValue(ColorPickerThemeInputs.ThemeColorPickerWidthProperty).ShouldBe(234);
            }
            else
            {
                source.GetValue(ColorPickerThemeInputs.ThemeTriggerPaddingProperty).ShouldBe(new Thickness(16));
                target.GetValue(ColorPickerThemeInputs.ThemeTriggerPaddingProperty).ShouldNotBe(new Thickness(16));
            }
            provider.Config = IsolationConfig(root: (views ? "ColorFillSecondary" : "ColorBorder", "#ff00b96b"));
            using var rootChanged = capture();
            ChangedRatioOf(arranged, rootChanged, source).ShouldBeGreaterThan(0);
            ChangedRatioOf(arranged, rootChanged, target).ShouldBeGreaterThan(0);
        });
    }

    [Theory]
    [InlineData("ColorPicker")]
    [InlineData("GradientColorPicker")]
    public void Popup_View_And_Public_Editors_Keep_Their_Identities(string id)
    {
        var picker = (AbstractColorPicker)Create(id, "ColorBorder");
        WithIsolationWindow(OverlayLayerHost(picker, 700, 550), (provider, capture) =>
        {
            picker.IsPickerOpen = true;
            using var opened = capture();
            var view = (AbstractColorPickerView)picker.PickerPresenter!;
            var viewIdentity = Identity(id + "View");
            ThemeScope.GetTokenOwner(picker).ShouldBe(Identity(id));
            ThemeScope.GetTokenOwner(view).ShouldBe(viewIdentity);
            var number = view.GetVisualDescendants().OfType<NumericUpDown>().First(n => n.Name == "PART_AlphaInput");
            var hex = view.GetVisualDescendants().OfType<LineEdit>().Single();
            var editor = number.GetVisualDescendants().OfType<EmbeddedTextBox>().Single();
            ThemeScope.GetTokenOwner(number).ShouldBe(NumericUpDownTokens.Identity);
            ThemeScope.GetTokenOwner(editor).ShouldBe(NumericUpDownTokens.Identity);
            ThemeScope.GetTokenOwner(hex).ShouldBe(LineEditTokens.Identity);
            var baseline = view.GetVisualDescendants().OfType<ColorSpectrum>().Single().CornerRadius;
            var numericForeground = editor.Foreground;
            provider.Config = IsolationConfig(control: (Identity(id), "BorderRadiusSM", "20"));
            using var triggerOnly = capture();
            view.GetVisualDescendants().OfType<ColorSpectrum>().Single().CornerRadius.ShouldBe(baseline);
            provider.Config = IsolationConfig(control: (viewIdentity, "BorderRadiusSM", "20"));
            using var viewOnly = capture();
            view.GetVisualDescendants().OfType<ColorSpectrum>().Single().CornerRadius.ShouldBe(new CornerRadius(20));
            editor.Foreground.ShouldBe(numericForeground);
            provider.Config = IsolationConfig(control: (NumericUpDownTokens.Identity, "ColorText", "#ff00b96b"));
            using var editorOnly = capture();
            ((ISolidColorBrush)editor.Foreground!).Color.ShouldBe(Color.Parse("#ff00b96b"));
            ChangedRatioOf(opened, editorOnly, number).ShouldBeGreaterThan(0);
            editor.FontSize.ShouldBe(11);
            number.Width.ShouldBe(48);
            number.SpinnerHandleWidth.ShouldBe(16);
            picker.IsPickerOpen = false;
            using var closed = capture();
        });
    }

    [Theory]
    [InlineData("ColorPickerView")]
    [InlineData("GradientColorPickerView")]
    public void Explicit_Base_Values_Drive_Their_Real_Dependent_Parts(string id)
    {
        var view = Create(id, "ColorPickerWidth");
        WithIsolationWindow(view, (provider, capture) =>
        {
            provider.Config = new ThemeConfigBuilder().WithToken("EnableMotion", "false")
                .WithControl(Identity(id), new ControlThemeConfigBuilder()
                    .WithToken("ColorPickerHandlerSizeSM", "20")
                    .WithToken("ColorPickerSliderTrackSize", "12")
                    .WithToken("ColorPickerPresetColorSize", "30").Build()).Build();
            using var changed = capture();
            view.GetValue(ColorPickerThemeInputs.ThemeColorPickerSliderSizeProperty).ShouldBe(20);
            view.GetValue(ColorPickerThemeInputs.ThemeColorPickerSliderThumbSizeProperty).ShouldBe(22);
            view.GetValue(ColorPickerThemeInputs.ThemeColorPickerPreviewSizeProperty).ShouldBe(36);
            view.GetValue(ColorPickerThemeInputs.ThemeCheckedMarkSizeProperty).ShouldBe(18);
            var thumb = view.GetVisualDescendants().OfType<ColorSliderThumb>().First();
            thumb.Bounds.Width.ShouldBe(22);
            view.GetVisualDescendants().OfType<ColorBlock>().Single(c => c.Name == "ColorPreview").Bounds.Width.ShouldBe(36);
        });
    }
    [Theory]
    [InlineData("ColorPickerView")]
    [InlineData("GradientColorPickerView")]
    public void Embedded_Editor_Selection_Live_Publish_And_User_Font_Keep_Their_Contract(string id)
    {
        var view = Create(id, "ColorPickerWidth");
        WithIsolationWindow(view, (provider, capture) =>
        {
            var number = view.GetVisualDescendants().OfType<NumericUpDown>().First(n => n.Name == "PART_AlphaInput");
            var editor = number.GetVisualDescendants().OfType<EmbeddedTextBox>().Single();
            editor.Focus();
            editor.SelectionStart = 0;
            editor.SelectionEnd = editor.Text!.Length;
            using var before = capture();
            editor.FontSize.ShouldBe(11);
            provider.Config = IsolationConfig(control: (NumericUpDownTokens.Identity, "SelectionBackground", "#ff00b96b"));
            using var selected = capture();
            ((ISolidColorBrush)editor.SelectionBrush!).Color.ShouldBe(Color.Parse("#ff00b96b"));
            ChangedRatioOf(before, selected, number).ShouldBeGreaterThan(0);
            provider.Config = IsolationConfig(control: (NumericUpDownTokens.Identity, "SelectionForeground", "#ffff00ff"));
            using var foreground = capture();
            ((ISolidColorBrush)editor.SelectionForegroundBrush!).Color.ShouldBe(Color.Parse("#ffff00ff"));
            ChangedRatioOf(selected, foreground, number).ShouldBeGreaterThan(0);
            number.FontSize = 19;
            provider.Config = IsolationConfig(control: (Identity(id), "EditorFontSize", "29"));
            using var font = capture();
            number.FontSize.ShouldBe(19);
            editor.FontSize.ShouldBe(19);
            view.GetValue(ColorPickerThemeInputs.ThemeEditorFontSizeProperty).ShouldBe(29);
            var swatch = view.GetVisualDescendants().OfType<PaletteColorItem>().First();
            ThemeScope.GetTokenOwner(swatch).ShouldBe(Identity(id));
            swatch.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeTrue();
            provider.Child = null;
            using var detached = capture();
            swatch.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse();
        });
    }
}
