using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AtomUIMasonry = AtomUI.Desktop.Controls.Masonry;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartHighlightSessionTests
{
    public SemanticPartHighlightSessionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Adorner_Draws_An_Outline_Without_Covering_The_Target()
    {
        var primary = RenderAdorner(true);
        primary.Length.ShouldBe(1, "primary marker paints a single gold outline and no white halo");
        primary[0].Brush.ShouldBeNull();
        AssertPen(primary[0], Color.FromArgb(0xFF, 0xFA, 0xAD, 0x14), 2);
        primary.ShouldNotContain(
            static drawing => IsNearWhite(drawing),
            "primary marker must not paint the upstream dumi white halo ring");

        var secondary = RenderAdorner(false);
        secondary.Length.ShouldBe(1);
        secondary[0].Brush.ShouldBeNull();
        AssertPen(secondary[0], Color.FromArgb(0xD9, 0xFA, 0xAD, 0x14), 1);
        secondary.ShouldNotContain(static drawing => IsNearWhite(drawing));
    }

    private static bool IsNearWhite(GeometryDrawing drawing)
    {
        return IsNearWhiteBrush(drawing.Brush) || IsNearWhiteBrush(drawing.Pen?.Brush);
    }

    private static bool IsNearWhiteBrush(IBrush? brush)
    {
        return brush is ISolidColorBrush { Color: { A: > 200 } color } &&
               color.R > 240 && color.G > 240 && color.B > 240;
    }

    [Fact]
    public void Adorner_Disables_Ancestor_Clipping_By_Default()
    {
        var target = new Border();
        var adorner = SemanticPartAdorner.Create(target, isPrimary: true);

        AdornerLayer.GetIsClipEnabled(adorner).ShouldBeFalse();
        adorner.Clip.ShouldBeNull();
        AdornerLayer.GetAdornedElement(adorner).ShouldBe(target);
    }

    [Fact]
    public void Start_Creates_Native_Adorners_And_Dispose_Releases_Them()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "content");
        var button = new AtomUIButton
        {
            Content = "Semantic Button"
        };

        using var context = ShowInAdornerHost(button);
        var layer = AdornerLayer.GetAdornerLayer(button).ShouldNotBeNull();
        var session = SemanticPartHighlightSession.Start(button, part, registry);

        session.TotalMatchCount.ShouldBe(1);
        session.HighlightedTargetCount.ShouldBe(1);
        var adorner = layer.Children.OfType<SemanticPartAdorner>().Single();
        AdornerLayer.GetAdornedElement(adorner).ShouldNotBeNull();

        session.Dispose();

        AdornerLayer.GetAdornedElement(adorner).ShouldBeNull();
        layer.Children.ShouldNotContain(adorner);
        session.HighlightedTargetCount.ShouldBe(0);
    }

    [Fact]
    public void Start_Disables_Target_Ancestor_Clipping_For_Outward_Markers()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIMasonry), out var descriptor).ShouldBeTrue();
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");
        var masonry = new AtomUIMasonry
        {
            Width = 200,
            Height = 100,
            ColumnCount = 1
        };
        masonry.Items.Add(new Border { Width = 100, Height = 40 });
        using var context = ShowInAdornerHost(masonry);
        using var session = SemanticPartHighlightSession.Start(masonry, part, registry);
        Dispatcher.UIThread.RunJobs();
        var target = masonry.GetVisualDescendants().First(static visual => visual.Classes.Contains("semantic-item"));
        var adorner = context.Layer.Children.OfType<SemanticPartAdorner>().Single();
        AdornerLayer.GetIsClipEnabled(adorner).ShouldBeFalse();
        adorner.Clip.ShouldBeNull();
        adorner.Bounds.ShouldBe(new Rect(-2, -2, target.Bounds.Width + 4, target.Bounds.Height + 4));

        session.Dispose();
        var rootPart = descriptor.Parts.Single(static candidate => candidate.Path == "root");
        using var rootSession = SemanticPartHighlightSession.Start(masonry, rootPart, registry);
        Dispatcher.UIThread.RunJobs();
        var rootAdorner = context.Layer.Children.OfType<SemanticPartAdorner>().Single();
        AdornerLayer.GetIsClipEnabled(rootAdorner).ShouldBeFalse();
        rootAdorner.Clip.ShouldBeNull();
    }

    [Fact]
    public void Start_Gives_Outward_Markers_Their_Own_Layout_Bounds()
    {
        var primaryTarget = new Border
        {
            Width  = 20,
            Height = 4
        };
        primaryTarget.Classes.Add("semantic-item");
        var secondaryTarget = new Border
        {
            Width  = 20,
            Height = 20
        };
        secondaryTarget.Classes.Add("semantic-item");
        Canvas.SetLeft(secondaryTarget, 40);
        Canvas.SetTop(secondaryTarget, 20);

        var owner = new Canvas
        {
            Width  = 100,
            Height = 100,
            Children =
            {
                primaryTarget,
                secondaryTarget
            }
        };
        var descriptor = RuntimeDescriptor(typeof(Canvas), "MarkerBoundsCanvas");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(owner);
        using var session = SemanticPartHighlightSession.Start(owner, part, registry);
        Dispatcher.UIThread.RunJobs();

        var adorners = context.Layer.Children.OfType<SemanticPartAdorner>().ToArray();
        var primary = adorners.Single(adorner =>
            AdornerLayer.GetAdornedElement(adorner) == primaryTarget);
        var secondary = adorners.Single(adorner =>
            AdornerLayer.GetAdornedElement(adorner) == secondaryTarget);

        AssertOutwardMarkerBounds(primaryTarget, primary, context.Layer, 2);
        AssertOutwardMarkerBounds(secondaryTarget, secondary, context.Layer, 1);
    }

    [Fact]
    public void Start_Uses_The_Thirty_Two_Visible_Target_Budget()
    {
        var owner = new Grid
        {
            Width  = 400,
            Height = 400
        };
        for (var index = 0; index < 40; index++)
        {
            var target = new Border
            {
                Width  = 20,
                Height = 20
            };
            target.Classes.Add("semantic-item");
            owner.Children.Add(target);
        }

        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(owner);
        using var session = SemanticPartHighlightSession.Start(owner, part, registry);

        session.TotalMatchCount.ShouldBe(40);
        session.HighlightedTargetCount.ShouldBe(32);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(32);
    }

    [Fact]
    public void Start_Resolves_Targets_Across_Multiple_Owners()
    {
        var ownerA = new Grid
        {
            Width  = 200,
            Height = 200
        };
        var targetA = new Border
        {
            Width  = 20,
            Height = 20
        };
        targetA.Classes.Add("semantic-item");
        ownerA.Children.Add(targetA);

        var ownerB = new Grid
        {
            Width  = 200,
            Height = 200
        };
        var targetB = new Border
        {
            Width  = 20,
            Height = 20
        };
        targetB.Classes.Add("semantic-item");
        ownerB.Children.Add(targetB);

        var host = new StackPanel
        {
            Children =
            {
                ownerA,
                ownerB
            }
        };
        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(host);
        using var session = SemanticPartHighlightSession.Start([ownerA, ownerB], part, registry);

        session.TotalMatchCount.ShouldBe(2);
        session.HighlightedTargetCount.ShouldBe(2);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(2);
    }

    [Fact]
    public void Start_Enforces_A_Single_Shared_Budget_Across_Owners()
    {
        var owners = new List<Control>();
        var host = new StackPanel();
        for (var ownerIndex = 0; ownerIndex < 2; ownerIndex++)
        {
            var owner = new Grid
            {
                Width  = 400,
                Height = 400
            };
            for (var index = 0; index < 20; index++)
            {
                var target = new Border
                {
                    Width  = 20,
                    Height = 20
                };
                target.Classes.Add("semantic-item");
                owner.Children.Add(target);
            }

            owners.Add(owner);
            host.Children.Add(owner);
        }

        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(host);
        using var session = SemanticPartHighlightSession.Start(owners, part, registry);

        session.TotalMatchCount.ShouldBe(40);
        session.HighlightedTargetCount.ShouldBe(32);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(32);
    }

    [Fact]
    public void Cross_Root_Session_ReResolves_An_Owner_Template_Popup()
    {
        var popupTarget = new Border
        {
            Width  = 80,
            Height = 40
        };
        popupTarget.Classes.Add("semantic-popup-content");
        var owner = new PopupSemanticOwner(popupTarget);
        var descriptor = PopupDescriptor();
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "popup");

        using var context = ShowInNativeAdornerHost(owner);
        using var session = SemanticPartHighlightSession.Start(owner, part, registry);
        session.HighlightedTargetCount.ShouldBe(0);

        owner.Popup.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popupLayer = AdornerLayer.GetAdornerLayer(popupTarget).ShouldNotBeNull();
        session.HighlightedTargetCount.ShouldBe(1);
        popupLayer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(1);

        owner.Popup.Close();
        Dispatcher.UIThread.RunJobs();

        session.HighlightedTargetCount.ShouldBe(0);
        popupLayer.Children.OfType<SemanticPartAdorner>().ShouldBeEmpty();
    }

    private static ControlSemanticDescriptor RuntimeDescriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("GalleryTests", id),
            [
                Root(controlType),
                new SemanticPartDescriptor(
                    "item",
                    "item",
                    "semantic-item",
                    typeof(Control),
                    SemanticPartCardinality.Multiple,
                    SemanticPartCustomization.Selector,
                    null,
                    false,
                    null,
                    true,
                    "> .semantic-item")
            ]);
    }

    private static ControlSemanticDescriptor PopupDescriptor()
    {
        return new ControlSemanticDescriptor(
            typeof(PopupSemanticOwner),
            new ControlTokenIdentity("GalleryTests", "PopupSemanticOwner"),
            [
                Root(typeof(PopupSemanticOwner)),
                new SemanticPartDescriptor(
                    "popup",
                    "popup",
                    "semantic-popup-content",
                    typeof(Border),
                    SemanticPartCardinality.Single,
                    SemanticPartCustomization.Selector,
                    null,
                    true,
                    null,
                    false)
            ]);
    }

    private static SemanticPartDescriptor Root(Type controlType)
    {
        return new SemanticPartDescriptor(
            "root",
            "root",
            null,
            controlType,
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            null,
            false);
    }

    private static GeometryDrawing[] RenderAdorner(bool isPrimary)
    {
        var adorner = SemanticPartAdorner.Create(new Border(), isPrimary);
        adorner.Measure(new Size(20, 20));
        adorner.Arrange(new Rect(0, 0, 20, 20));

        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            adorner.Render(context);
        }

        var drawings = drawingGroup.Children.ToArray();
        drawings.ShouldAllBe(static child => child is GeometryDrawing);
        return drawings.Cast<GeometryDrawing>().ToArray();
    }

    private static void AssertPen(GeometryDrawing drawing, Color expectedColor, double expectedThickness)
    {
        var pen = drawing.Pen.ShouldNotBeNull();
        pen.Thickness.ShouldBe(expectedThickness);
        var brush = pen.Brush.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
        brush.Color.ShouldBe(expectedColor);
    }

    private static void AssertOutwardMarkerBounds(
        Control target,
        SemanticPartAdorner adorner,
        AdornerLayer layer,
        double expectedOutset)
    {
        adorner.Bounds.ShouldBe(new Rect(
            -expectedOutset,
            -expectedOutset,
            target.Bounds.Width + expectedOutset * 2,
            target.Bounds.Height + expectedOutset * 2));

        var targetOrigin = target.TranslatePoint(default, layer).ShouldNotBeNull();
        var adornerOrigin = adorner.TranslatePoint(default, layer).ShouldNotBeNull();
        adornerOrigin.ShouldBe(new Point(
            targetOrigin.X - expectedOutset,
            targetOrigin.Y - expectedOutset));

        var localBounds = new Rect(adorner.Bounds.Size);
        foreach (var drawing in RenderAdorner(adorner))
        {
            var pen = drawing.Pen.ShouldNotBeNull();
            var drawingBounds = drawing.Geometry.ShouldNotBeNull()
                                       .Bounds
                                       .Inflate(pen.Thickness / 2);
            drawingBounds.Left.ShouldBeGreaterThanOrEqualTo(localBounds.Left);
            drawingBounds.Top.ShouldBeGreaterThanOrEqualTo(localBounds.Top);
            drawingBounds.Right.ShouldBeLessThanOrEqualTo(localBounds.Right);
            drawingBounds.Bottom.ShouldBeLessThanOrEqualTo(localBounds.Bottom);
        }
    }

    private static GeometryDrawing[] RenderAdorner(SemanticPartAdorner adorner)
    {
        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            adorner.Render(context);
        }

        var drawings = drawingGroup.Children.ToArray();
        drawings.ShouldAllBe(static child => child is GeometryDrawing);
        return drawings.Cast<GeometryDrawing>().ToArray();
    }

    private static AdornerHostContext ShowInAdornerHost(Control control)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = control
        };
        var window = new AtomUIWindow
        {
            Width   = 500,
            Height  = 500,
            Content = visualLayerManager
        };

        window.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return new AdornerHostContext(
            window,
            AdornerLayer.GetAdornerLayer(control).ShouldNotBeNull());
    }

    private static AdornerHostContext ShowInNativeAdornerHost(Control control)
    {
        var window = new AtomUIWindow
        {
            Width   = 500,
            Height  = 500,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return new AdornerHostContext(
            window,
            AdornerLayer.GetAdornerLayer(control).ShouldNotBeNull());
    }

    private sealed class AdornerHostContext : IDisposable
    {
        private readonly Window _window;

        public AdornerHostContext(Window window, AdornerLayer layer)
        {
            _window = window;
            Layer   = layer;
        }

        public AdornerLayer Layer { get; }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class PopupSemanticOwner : TemplatedControl
    {
        public PopupSemanticOwner(Control popupChild)
        {
            Popup = new Popup
            {
                Child                 = popupChild,
                PlacementTarget       = this,
                ShouldUseOverlayLayer = true
            };
            Template = new FuncControlTemplate<PopupSemanticOwner>((owner, _) =>
                new Grid
                {
                    Children =
                    {
                        new Border
                        {
                            Width  = 120,
                            Height = 80
                        },
                        owner.Popup
                    }
                });
        }

        public Popup Popup { get; }
    }

    [Fact]
    public void Adorner_Marker_Rect_Stays_Inside_The_Expanded_Adorner_Bounds()
    {
        // 50x40 目标的主 adorner 四周各扩 2px，因此 Bounds 为 54x44。
        // 金框完全位于这个 Bounds 内，视觉上仍落在目标外侧。
        SemanticPartAdorner.GetMarkerRect(new Size(54, 44), 2, 1)
                           .ShouldBe(new Rect(1, 1, 52, 42));

        // 副 adorner 四周各扩 1px，1px 描边也保留完整的四条边。
        SemanticPartAdorner.GetMarkerRect(new Size(52, 42), 1, 0.5)
                           .ShouldBe(new Rect(0.5, 0.5, 51, 41));

        // 201x4 的细窄目标扩展后为 205x8，金框不会退化或越出 adorner。
        SemanticPartAdorner.GetMarkerRect(new Size(205, 8), 2, 1)
                           .ShouldBe(new Rect(1, 1, 203, 6));
    }

    [Fact]
    public void Marker_Rect_Is_Clamped_Inside_The_Host_Window_For_Edge_To_Edge_Targets()
    {
        // native 预览对话框的 popup.root/body 是贴边满区目标：adorner 经负 Margin 外扩 2px
        // 并平移到标题栏下方（层坐标 (-2, 38)）后，未钳制的描边矩形左、右、下三条边越出
        // 1210x691 的窗口表面被 OS 裁剪，视觉上只剩顶部一条线。
        var markerRect = SemanticPartAdorner.GetMarkerRect(new Size(1218, 659), 2, 1);

        var clamped = SemanticPartAdorner.ClampMarkerRect(
            markerRect,
            new Rect(0, 0, 1210, 691),
            new Point(-2, 38),
            2);

        // 钳制只内收半个笔宽（对齐上游 Marker：描边沿目标边缘、不内收留白）。
        // 换算回层坐标断言：被钳制的左/右/下边缘距层边缘正好 1px（2px 主笔宽的一半），
        // 描边整条完整落在窗口表面内；顶部本就在层内（标题栏下方 39px），保持原位。
        var layerRect = clamped.Translate(new Vector(-2, 38));
        layerRect.ShouldBe(new Rect(1, 39, 1208, 651));
    }

    [Fact]
    public void Marker_Rect_Clamp_Keeps_Interior_Targets_Untouched()
    {
        // 有余量的常规目标（cover 演示位）钳制不应改变描边。
        var markerRect = SemanticPartAdorner.GetMarkerRect(new Size(246, 242), 2, 1);

        var clamped = SemanticPartAdorner.ClampMarkerRect(
            markerRect,
            new Rect(0, 0, 1300, 900),
            new Point(321, 250.5),
            3);

        clamped.ShouldBe(markerRect);
    }

    [Fact]
    public void Clip_Host_Clamps_Marker_Rect_To_The_Preview_Stage()
    {
        // 语义预览表格横向溢出画布：目标右段越出画布（adorner 左缘在画布左缘外 2px，
        // 画布宽 690）。描边必须与画布求交，只保留画布内的部分，右边缘贴画布右缘
        // 内收半个笔宽，不再画到部件面板或画布之外。
        var markerRect = SemanticPartAdorner.GetMarkerRect(new Size(760, 300), 2, 1);

        var clamped = SemanticPartAdorner.ClampMarkerRectToClipHost(
            markerRect,
            clipHostRectInLayer: new Rect(40, 60, 690, 400),
            adornerPositionInLayer: new Point(38, 100),
            penThickness: 2);

        clamped.ShouldNotBeNull();
        clamped.ShouldBe(new Rect(3, 1, 688, 298));
    }

    [Fact]
    public void Clip_Host_Hides_Markers_Entirely_Outside_The_Stage()
    {
        // 目标完全落在画布之外（如溢出画布的填充列）：画布内没有任何可见部分，
        // 高亮框必须整体隐藏而不是跨过画布边界绘制。
        var markerRect = SemanticPartAdorner.GetMarkerRect(new Size(60, 300), 2, 1);

        var clamped = SemanticPartAdorner.ClampMarkerRectToClipHost(
            markerRect,
            clipHostRectInLayer: new Rect(40, 60, 690, 400),
            adornerPositionInLayer: new Point(760, 100),
            penThickness: 2);

        clamped.ShouldBeNull();
    }

    [Fact]
    public void Clip_Host_Keeps_Interior_Markers_Untouched()
    {
        // 画布内目标（如 title 部件）不受画布钳制影响。
        var markerRect = SemanticPartAdorner.GetMarkerRect(new Size(246, 42), 2, 1);

        var clamped = SemanticPartAdorner.ClampMarkerRectToClipHost(
            markerRect,
            clipHostRectInLayer: new Rect(40, 60, 690, 400),
            adornerPositionInLayer: new Point(120, 150),
            penThickness: 2);

        clamped.ShouldNotBeNull();
        clamped.ShouldBe(markerRect);
    }

    [Fact]
    public void Start_Passes_The_Clip_Host_To_The_Adorners()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "content");
        var button = new AtomUIButton
        {
            Content = "Semantic Button"
        };
        var stage = new Border();

        using var context = ShowInAdornerHost(button);
        using (var session = SemanticPartHighlightSession.Start(
            button, part, registry, additionalRoots: null, clipHost: stage))
        {
            Dispatcher.UIThread.RunJobs();
            var adorner = context.Layer.Children.OfType<SemanticPartAdorner>().Single();
            adorner.ClipHost.ShouldBeSameAs(stage);
        }

        // 未声明画布的会话（popup/独立窗口宿主）保持旧行为：只钳制到层。
        using (var plainSession = SemanticPartHighlightSession.Start(button, part, registry))
        {
            Dispatcher.UIThread.RunJobs();
            var plainAdorner = context.Layer.Children.OfType<SemanticPartAdorner>().Single();
            plainAdorner.ClipHost.ShouldBeNull();
        }
    }
}
