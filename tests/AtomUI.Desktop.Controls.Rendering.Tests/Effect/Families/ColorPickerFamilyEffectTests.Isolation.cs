using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;
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
                source.GetValue(ColorPickerAttachedProperties.SpectrumMinWidthProperty).ShouldBe(330);
                target.GetValue(ColorPickerAttachedProperties.SpectrumMinWidthProperty).ShouldBe(234);
            }
            else
            {
                source.GetValue(ColorPickerAttachedProperties.TriggerPaddingProperty).ShouldBe(new Thickness(16));
                target.GetValue(ColorPickerAttachedProperties.TriggerPaddingProperty).ShouldNotBe(new Thickness(16));
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
            view.GetValue(ColorPickerAttachedProperties.SliderHeightProperty).ShouldBe(20);
            view.GetValue(ColorPickerAttachedProperties.SliderThumbSizeProperty).ShouldBe(22);
            view.GetValue(ColorPickerAttachedProperties.PreviewSizeProperty).ShouldBe(36);
            view.GetValue(ColorPickerAttachedProperties.CheckedMarkSizeProperty).ShouldBe(18);
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
            view.GetValue(ColorPickerAttachedProperties.EditorFontSizeProperty).ShouldBe(29);
            var swatch = view.GetVisualDescendants().OfType<PaletteColorItem>().First();
            ThemeScope.GetTokenOwner(swatch).ShouldBe(Identity(id));
            swatch.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeTrue();
            provider.Child = null;
            using var detached = capture();
            swatch.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse();
        });
    }
    [Theory]
    [InlineData("selected")]
    [InlineData("closed")]
    [InlineData("disabled")]
    [InlineData("user-local")]
    public void Gradient_Labels_Follow_Live_Tokens_After_Stop_And_Close_History(string phase)
    {
        var picker = (GradientColorPicker)Create("GradientColorPicker", "ColorText");
        WithIsolationWindow(OverlayLayerHost(picker, 700, 550), (provider, capture) =>
        {
            picker.IsPickerOpen = true;
            using var opened = capture();
            var labels = picker.GetVisualDescendants().OfType<WrapPanel>().Single(p => p.Name == "PART_ColorTextPanel")
                .Children.OfType<Avalonia.Controls.TextBlock>().ToArray();
            if (phase == "user-local")
            {
                labels[0].Foreground = Brushes.Magenta;
            }
            var view = (GradientColorPickerView)picker.PickerPresenter!;
            var thumb = view.GetVisualDescendants().OfType<GradientColorSliderThumb>().Last();
            var window = (Avalonia.Controls.Window)TopLevel.GetTopLevel(picker)!;
            var point = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            using var selected = capture();
            picker.ActivatedStopIndex.ShouldBe(1);
            if (phase == "selected")
            {
                labels[1].Foreground.ShouldBe(picker.InactiveStopTextColor);
                provider.Config = IsolationConfig(control: (GradientColorPickerTokens.Identity, "ColorTextDisabled", "#ff00b96b"));
                using var live = capture();
                ((ISolidColorBrush)picker.InactiveStopTextColor!).Color.ShouldBe(Color.Parse("#ff00b96b"));
                ((ISolidColorBrush)labels[1].Foreground!).Color.ShouldBe(Color.Parse("#ff00b96b"));
                ChangedRatioOf(selected, live, labels[1]).ShouldBeGreaterThan(0);
                return;
            }
            picker.IsPickerOpen = false;
            using var closed = capture();
            picker.ActivatedStopIndex.ShouldBeNull();
            if (phase == "closed")
            {
                provider.Config = IsolationConfig(control: (GradientColorPickerTokens.Identity, "ColorText", "#ff00b96b"));
                using var live = capture();
                labels.ShouldAllBe(label => ((ISolidColorBrush)label.Foreground!).Color == Color.Parse("#ff00b96b"));
                ChangedRatioOf(closed, live, labels[0]).ShouldBeGreaterThan(0);
            }
            else if (phase == "disabled")
            {
                var enabledColor = ((ISolidColorBrush)labels[0].Foreground!).Color;
                picker.IsEnabled = false;
                using var disabled = capture();
                labels.ShouldAllBe(label => ((ISolidColorBrush)label.Foreground!).Color == ((ISolidColorBrush)picker.InactiveStopTextColor!).Color);
                ChangedRatioOf(closed, disabled, labels[0]).ShouldBeGreaterThan(0);
                picker.IsEnabled = true;
                using var enabled = capture();
                labels.ShouldAllBe(label => ((ISolidColorBrush)label.Foreground!).Color == enabledColor);
            }
            else
            {
                labels[0].Foreground.ShouldBe(Brushes.Magenta);
            }
        });
    }

    [Theory]
    [InlineData("ColorPicker")]
    [InlineData("GradientColorPicker")]
    public void Popup_Motion_Uses_View_Default_Host_Disable_Gate_And_User_Local_Priority(string id)
    {
        var picker = (AbstractColorPicker)Create(id, "ColorBorder");
        WithIsolationWindow(OverlayLayerHost(picker, 700, 550), (provider, capture) =>
        {
            ThemeConfig Motion(string? control = null, bool enabled = true) =>
                control is null ? new ThemeConfigBuilder().WithToken("EnableMotion", "true").Build()
                : new ThemeConfigBuilder().WithToken("EnableMotion", "true")
                    .WithControl(Identity(control), new ControlThemeConfigBuilder().WithToken("EnableMotion", enabled ? "true" : "false").Build()).Build();
            provider.Config = Motion();
            using var initial = capture();
            picker.IsPickerOpen = true;
            using var opened = capture();
            var view = (AbstractColorPickerView)picker.PickerPresenter!;
            picker.IsMotionEnabled.ShouldBeTrue();
            view.IsMotionEnabled.ShouldBeTrue();
            provider.Config = Motion(id + "View", false);
            using var viewOff = capture();
            picker.IsMotionEnabled.ShouldBeTrue();
            view.IsMotionEnabled.ShouldBeFalse();
            provider.Config = Motion();
            using var viewOn = capture();
            view.IsMotionEnabled.ShouldBeTrue();
            provider.Config = Motion(id, false);
            using var hostOff = capture();
            picker.IsMotionEnabled.ShouldBeFalse();
            view.IsMotionEnabled.ShouldBeFalse();
            view.IsMotionEnabled = true;
            using var userOn = capture();
            view.IsMotionEnabled.ShouldBeTrue();
            view.ClearValue(AbstractColorPickerView.IsMotionEnabledProperty);
            using var userCleared = capture();
            view.IsMotionEnabled.ShouldBeFalse();
            provider.Config = Motion();
            using var hostOn = capture();
            view.IsMotionEnabled.ShouldBeTrue();
            view.IsMotionEnabled = false;
            provider.Config = Motion(id + "View", true);
            using var userOff = capture();
            view.IsMotionEnabled.ShouldBeFalse();
        });
    }
}
