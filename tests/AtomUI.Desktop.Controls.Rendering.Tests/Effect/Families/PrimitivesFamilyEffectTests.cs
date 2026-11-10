using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Headless;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.Primitives;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class PrimitivesFamilyEffectTests
{
    [Theory]
    [InlineData("ArrowDecoratedBox", "ColorBgElevated")]
    [InlineData("Icon", "ColorIconHover")]
    [InlineData("Icon", "FallbackColor")]
    public void Fixed_Consumption_Uses_Effective_Tokens(string member, string token)
    {
        if (member == "Icon")
        {
            TokenEffectHarness.AssertHasEffect(() => token == "FallbackColor" ? new FallbackIcon() : new CheckOutlined(), IconTokens.Identity, token);
        }
        else
        {
            TokenEffectHarness.AssertHasEffect(() => new ArrowDecoratedBox { Content = "Scoped arrow" },
                ArrowDecoratedBoxTokens.Identity, token);
        }
    }
    public static IEnumerable<object[]> ArrowRows() => EffectTestData.For("AtomUI", "ArrowDecoratedBox");
    public static IEnumerable<object[]> ScrollRows() => EffectTestData.For("AtomUI", "ScrollViewer");
    public static IEnumerable<object[]> IconRows() => EffectTestData.For("AtomUI", "Icon");
    public static IEnumerable<object[]> TitleRows() => EffectTestData.For("AtomUI", "WindowTitleBar");

    [Theory, MemberData(nameof(ArrowRows))]
    public void Arrow_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() => new ArrowDecoratedBox { Content = "Scoped arrow content", BorderThickness = token == "ColorBorderSecondary" ? new Thickness(1) : default },
            ArrowDecoratedBoxTokens.Identity, token,
            c => ((Avalonia.Controls.Window)TopLevel.GetTopLevel(c)!).Background = Brushes.Gray,
            thresholdRatio: token is "ArrowStrokeColor" or "ArrowStrokeThickness" ? TokenEffectHarness.ThinLineThresholdRatio : TokenEffectHarness.DefaultThresholdRatio,
            options: new EffectOptions
            {
                Perturb = token == "BorderRadius" ? _ => new CornerRadius(24) : null
            });
    }

    [Theory, MemberData(nameof(ScrollRows))]
    public void Scroll_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() => token == "IndicatorThumbCornerRadius" ? new IndicatorScrollViewer { Width = 260, Height = 160, Content = new Border { Width = 600, Height = 650, Background = Brushes.AliceBlue } } : new ScrollViewer
        {
            Width = 260, Height = 160, AllowAutoHide = false,
            IsLiteMode = token is "LiteModeThumbThickness" or "LineWidthBold",
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Content = new Border { Width = 600, Height = 650, Background = Brushes.AliceBlue }
        }, ScrollViewerTokens.Identity, token, c =>
        {
            var viewer = (ScrollViewer)c;
            var window = (Avalonia.Controls.Window)TopLevel.GetTopLevel(viewer)!;
            window.MouseMove(viewer.TranslatePoint(new Point(40, 40), window)!.Value);
            ((Avalonia.Controls.IPseudoClasses)viewer.Classes).Add(":pointerover");
            if (token == "ColorBgContainer")
            {
                foreach (var bar in viewer.GetVisualDescendants().OfType<ScrollBar>())
                {
                    bar.SetIsExpanded(true);
                }
            }
            if (token is "ThumbHoverBg" or "ThumbActiveBg")
            {
                foreach (var thumb in viewer.GetVisualDescendants().OfType<ScrollBarThumb>())
                {
                    ((Avalonia.Controls.IPseudoClasses)thumb.Classes).Add(token == "ThumbHoverBg" ? ":pointerover" : ":pressed");
                }
            }
        }, options: new EffectOptions
        {
            Perturb = token == "IndicatorThumbCornerRadius" ? _ => new CornerRadius(24, 0, 0, 0)
                : token == "ThumbCornerRadius" ? _ => new CornerRadius(0)
                : token is "LiteModeThumbThickness" or "LineWidthBold" ? _ => 8d : null,
            Region = c => token.Contains("Thumb") || token is "ColorBorder" or "SizeXS" or "LineWidthBold" or "UniformlyPaddingXXS"
                ? c.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.Thumb>().First()
                : token == "ColorBgContainer" ? c.GetVisualDescendants().OfType<Panel>().First(p => p.Name == "ScrollBarsSeparator") : c
        });
    }

    [Theory, MemberData(nameof(IconRows))]
    public void Icon_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() => token is "StrokeWidth" or "StrokeLineCap" or "StrokeLineJoin"
            ? new StrokeIcon() : token is "FallbackColor" ? new FallbackIcon() : new PaletteIcon(token),
            IconTokens.Identity, token, options: new EffectOptions
            { Perturb = token is "StrokeLineCap" ? _ => PenLineCap.Square : token is "StrokeLineJoin" ? _ => PenLineJoin.Miter : null });
    }

    [Theory, MemberData(nameof(TitleRows))]
    public void Title_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        if (token == "FullscreenCaptionButtonSize")
        {
            AssertWindowEndpoint(token);
            return;
        }
        TokenEffectHarness.AssertHasEffect(() => new WindowTitleBar
        {
            Width = 500, Title = "Application window", Logo = new CheckOutlined(),
            LeftAddOn = "Left", RightAddOn = new WindowTitleBarButton { Icon = new CheckOutlined() }, IsWindowActive = !token.StartsWith("Inactive") && token is not "ColorTextQuaternary" and not "ColorFillQuaternary",
            [WindowTitleBar.OsTypeProperty] = token.StartsWith("Windows") ? OsType.Windows : OsType.Linux,
            IsPinCaptionButtonVisible = true, IsPinCaptionButtonSupported = true, IsFullScreenCaptionButtonVisible = true
        }, WindowTitleBarTokens.Identity, token, c =>
        {
            c.SetValue(WindowTitleBar.OsTypeProperty, token.StartsWith("Windows") ? OsType.Windows : OsType.Linux);
            ((WindowTitleBar)c).ApplyTemplate();
            // ConfigureOsType stores the native platform locally on each group; configure the actual
            // group as well when arranging another supported platform in the headless host.
            foreach (var group in c.GetVisualDescendants().OfType<CaptionButtonGroup>())
            {
                group.SetValue(CaptionButtonGroup.OsTypeProperty, token.StartsWith("Windows") ? OsType.Windows : OsType.Linux);
                group.ApplyTemplate();
            }
            c.UpdateLayout();
            foreach (var button in c.GetVisualDescendants().OfType<CaptionButton>())
            {
                if (token.Contains("Pressed") || token == "ColorFill") { ((Avalonia.Controls.IPseudoClasses)button.Classes).Add(":pressed"); }
                else if (token.Contains("Hover") || token == "ColorFillSecondary") { ((Avalonia.Controls.IPseudoClasses)button.Classes).Add(":pointerover"); }
            }
        }, options: new EffectOptions
        {
            Perturb = token == "CaptionButtonCornerRadius" ? _ => new CornerRadius(0) : null,
            Region = token == "WindowsCloseButtonHoverColor"
                ? c => c.GetVisualDescendants().OfType<WindowsCaptionButton>().First(b => b.Name == "PART_CloseButton") : null
        });
    }

    public static IEnumerable<object[]> WindowRows() => EffectTestData.For("AtomUI", "Window");

    [Theory, MemberData(nameof(WindowRows))]
    public void Window_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        if (token is "SystemBarColor" or "FrameBorderActive" or "FrameBorderInactive" or "CornerRadius" or
            "FullscreenHeaderFramePadding" or "FullscreenPopoverShadows" or "TitleBarHeight")
        {
            AssertWindowEndpoint(token);
            return;
        }
        TokenEffectHarness.AssertHasEffect(() => new Window
        {
            Width = 500, Height = 320, Title = "Scoped window",
            Content = new Avalonia.Controls.TextBlock { Text = "Application window content", Margin = new Thickness(30) },
            [Window.OsTypeProperty] = OsType.Linux
        }, WindowTokens.Identity, token, options: new EffectOptions
        { Perturb = token == "FontFamily" ? _ => new FontFamily("monospace") : null });
    }

    private sealed class PaletteIcon : Icon
    {
        private readonly IList<DrawingInstruction> _instructions;
        public PaletteIcon(string token)
        {
            ViewBox = new Rect(0, 0, 16, 16);
            IconTheme = token is "ColorInfoBg" ? IconThemeType.TwoTone : IconThemeType.MultiColor;
            var brush = token == "SecondaryFillColor" ? IconBrushType.SecondaryFill : token == "SecondaryStrokeColor"
                ? IconBrushType.SecondaryStroke : token == "ColorInfoBg" || token == "ColorPrimary" ? IconBrushType.Fill : IconBrushType.Stroke;
            _instructions = [new RectDrawingInstruction { Rect = new Rect(0, 0, 16, 16), FillBrush = brush }];
        }
        public override Icon CreateInstance() => new PaletteIcon("ColorPrimary");
        protected override IList<DrawingInstruction> DrawingInstructions => _instructions;
    }

    private sealed class StrokeIcon : Icon
    {
        public StrokeIcon() { ViewBox = new Rect(0, 0, 24, 24); Width = 80; Height = 80; }
        public override Icon CreateInstance() => new StrokeIcon();
        protected override IList<DrawingInstruction> DrawingInstructions { get; } =
        [new PathDrawingInstruction
        {
            Data = StreamGeometry.Parse("M4,20 L12,4 L20,20"), StrokeBrush = IconBrushType.Stroke,
            IsStrokeEnabled = true, IsStrokeWidthCustomizable = true, IsStrokeLinecapCustomizable = true, IsStrokeLinejoinCustomizable = true
        }];
    }

    private sealed class FallbackIcon : Icon
    {
        public FallbackIcon() { ViewBox = new Rect(0, 0, 16, 16); }
        public override Icon CreateInstance() => new FallbackIcon();
        protected override IList<DrawingInstruction> DrawingInstructions { get; } =
        [new PathDrawingInstruction { Data = StreamGeometry.Parse("M0,0 L16,0 L16,16 L0,16 Z"), FillBrush = IconBrushType.Fallback }];
    }
}
