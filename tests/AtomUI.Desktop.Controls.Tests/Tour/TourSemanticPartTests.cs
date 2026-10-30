using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using IOPath = System.IO.Path;
using AtomUITour = AtomUI.Desktop.Controls.Tour;
using AtomUITourLayer = AtomUI.Desktop.Controls.TourLayer;
using AtomUITourStep = AtomUI.Desktop.Controls.TourStep;
using AtomUIDefaultTourIndicator = AtomUI.Desktop.Controls.DefaultTourIndicator;

namespace AtomUI.Desktop.Controls.Tests.Tour;

public class TourSemanticPartTests
{
    private const string TourThemePath =
        "src/AtomUI.Desktop.Controls/Tour/Themes/TourTheme.axaml";
    private const string TourStepThemePath =
        "src/AtomUI.Desktop.Controls/Tour/Themes/TourStepTheme.axaml";
    private const string TourStepsViewThemePath =
        "src/AtomUI.Desktop.Controls/Tour/Themes/TourStepsViewTheme.axaml";
    private const string DefaultTourIndicatorThemePath =
        "src/AtomUI.Desktop.Controls/Tour/Themes/DefaultTourIndicatorTheme.axaml";

    private const string PopupRootClass        = "semantic-popup-root";
    private const string PopupMaskClass        = "semantic-popup-mask";
    private const string PopupSectionClass     = "semantic-container";
    private const string PopupCoverClass       = "semantic-popup-cover";
    private const string PopupCloseClass       = "semantic-popup-close";
    private const string PopupHeaderClass      = "semantic-popup-header";
    private const string PopupTitleClass       = "semantic-popup-title";
    private const string PopupDescriptionClass = "semantic-popup-description";
    private const string PopupFooterClass      = "semantic-popup-footer";
    private const string PopupActionsClass     = "semantic-popup-actions";
    private const string PopupIndicatorsClass  = "semantic-popup-indicators";
    private const string PopupIndicatorClass   = "semantic-popup-indicator";

    // 注册表以 root 在前、其余按字母序返回部件名单。
    private static readonly string[] ApprovedPartNames =
    [
        "root",
        "popup.actions",
        "popup.close",
        "popup.cover",
        "popup.description",
        "popup.footer",
        "popup.header",
        "popup.indicator",
        "popup.indicators",
        "popup.mask",
        "popup.root",
        "popup.section",
        "popup.title",
    ];

    static TourSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Tour_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITour), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUITour));

        // 弹层根：Tour 模板内的 ArrowDecoratedBox（与 DatePicker popup.root 同构）。
        AssertPart(descriptor, "popup.root", PopupRootClass,
            typeof(ArrowDecoratedBox), "/template/ .semantic-popup-root",
            SemanticPartCardinality.Single, runtimeCreated: false);

        // 遮罩：TopLevel VisualLayerManager 中的运行时共享层，经逻辑父挂载命中。
        AssertPart(descriptor, "popup.mask", PopupMaskClass,
            typeof(Control), ">> .semantic-popup-mask",
            SemanticPartCardinality.Optional, runtimeCreated: true);

        // 卡片内容区：共享 ArrowDecoratedBox 模板的 PART_ContentDecorator。
        AssertPart(descriptor, "popup.section", PopupSectionClass,
            typeof(Border), "/template/ .semantic-popup-root /template/ .semantic-container");

        // TourStep 模板部件：ItemsControl 容器运行时物化。
        AssertPart(descriptor, "popup.cover", PopupCoverClass,
            typeof(ContentPresenter), "/template/ .semantic-popup-root >> .semantic-popup-cover",
            SemanticPartCardinality.Optional);
        AssertPart(descriptor, "popup.close", PopupCloseClass,
            typeof(AvaloniaButton), "/template/ .semantic-popup-root >> .semantic-popup-close");
        AssertPart(descriptor, "popup.header", PopupHeaderClass,
            typeof(Border), "/template/ .semantic-popup-root >> .semantic-popup-header");
        AssertPart(descriptor, "popup.title", PopupTitleClass,
            typeof(ContentPresenter), "/template/ .semantic-popup-root >> .semantic-popup-title",
            SemanticPartCardinality.Optional);
        AssertPart(descriptor, "popup.description", PopupDescriptionClass,
            typeof(ContentPresenter), "/template/ .semantic-popup-root >> .semantic-popup-description",
            SemanticPartCardinality.Optional);

        // TourStepsView 模板部件。
        AssertPart(descriptor, "popup.footer", PopupFooterClass,
            typeof(Border), "/template/ .semantic-popup-root >> .semantic-popup-footer");
        AssertPart(descriptor, "popup.actions", PopupActionsClass,
            typeof(StackPanel), "/template/ .semantic-popup-root >> .semantic-popup-actions");
        AssertPart(descriptor, "popup.indicators", PopupIndicatorsClass,
            typeof(ContentPresenter), "/template/ .semantic-popup-root >> .semantic-popup-indicators",
            SemanticPartCardinality.Optional);
        AssertPart(descriptor, "popup.indicator", PopupIndicatorClass,
            typeof(Ellipse), "/template/ .semantic-popup-root >> .semantic-popup-indicator",
            SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void IsPopupPinnedOpen_Is_Public_Api_For_Semantic_Preview()
    {
        var property = typeof(AtomUITour).GetProperty(
            nameof(AtomUITour.IsPopupPinnedOpen),
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        property.ShouldNotBeNull(
            "Tour must publicly expose IsPopupPinnedOpen so Gallery semantic previews can pin the popup open");
        property.SetMethod.ShouldNotBeNull().IsPublic.ShouldBeTrue();
        property.GetMethod.ShouldNotBeNull().IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void Built_In_Theme_Templates_Carry_The_Expected_Semantic_Markers()
    {
        AssertThemeMarkers(TourThemePath,
            (PopupRootClass, "ArrowDecoratedBox"));
        AssertThemeMarkers(TourStepThemePath,
            (PopupCoverClass, "ContentPresenter"),
            (PopupCloseClass, "DialogCaptionButton"),
            (PopupHeaderClass, "Border"),
            (PopupTitleClass, "ContentPresenter"),
            (PopupDescriptionClass, "ContentPresenter"));
        AssertThemeMarkers(TourStepsViewThemePath,
            (PopupFooterClass, "Border"),
            (PopupActionsClass, "StackPanel"),
            (PopupIndicatorsClass, "ContentPresenter"));

        // 圆点由 DefaultTourIndicator 代码物化并挂 marker 类；主题负责视觉（尺寸/颜色），
        // 此处锁定主题中存在圆点选择器，防止代码回退为直接设置视觉属性。
        var indicatorTheme = ReadThemeDocument(DefaultTourIndicatorThemePath);
        indicatorTheme.ToString().ShouldContain("Ellipse.semantic-popup-indicator");
        indicatorTheme.ToString().ShouldContain("Ellipse.semantic-popup-indicator.active");
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Pinned_Open()
    {
        var (root, tour) = CreatePinnedTourRoot(isShowMask: true);

        ShowInWindow(root, window =>
        {
            // popup.root + popup.section（共享 ArrowDecoratedBox 模板）
            window.GetVisualDescendants()
                  .OfType<ArrowDecoratedBox>()
                  .Single(control => control.Classes.Contains(PopupRootClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<Border>()
                  .Single(control => control.Classes.Contains(PopupSectionClass))
                  .ShouldNotBeNull();

            // TourStepsView 模板部件
            window.GetVisualDescendants()
                  .OfType<Border>()
                  .Single(control => control.Classes.Contains(PopupFooterClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<StackPanel>()
                  .Single(control => control.Classes.Contains(PopupActionsClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains(PopupIndicatorsClass))
                  .ShouldNotBeNull();

            // 当前步骤（CurrentIndex=0）的 TourStep 模板部件
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains(PopupCoverClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains(PopupTitleClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains(PopupDescriptionClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<Border>()
                  .Single(control => control.Classes.Contains(PopupHeaderClass))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<AvaloniaButton>()
                  .Count(control => control.Classes.Contains(PopupCloseClass))
                  .ShouldBeGreaterThanOrEqualTo(1);

            // 指示器圆点：两个步骤 → 两个圆点，第一个为激活态
            var dots = window.GetVisualDescendants()
                             .OfType<Ellipse>()
                             .Where(control => control.Classes.Contains(PopupIndicatorClass))
                             .ToArray();
            dots.Length.ShouldBe(2);
            dots.Count(control => control.Classes.Contains("active")).ShouldBe(1);
        });
    }

    [Fact]
    public void Generated_Popup_Semantic_Styles_Apply_To_The_Popup_Targets()
    {
        var (root, tour) = CreatePinnedTourRoot(isShowMask: false);
        AttachOwnerPartStyles(tour);

        ShowInWindow(root, window =>
        {
            FindSemantic<ArrowDecoratedBox>(window, PopupRootClass).Tag.ShouldBe("popup.root");
            FindSemantic<Border>(window, PopupSectionClass).Tag.ShouldBe("popup.section");
            FindSemantic<Border>(window, PopupHeaderClass).Tag.ShouldBe("popup.header");
            FindSemantic<Border>(window, PopupFooterClass).Tag.ShouldBe("popup.footer");
            FindSemantic<StackPanel>(window, PopupActionsClass).Tag.ShouldBe("popup.actions");
            FindSemantic<ContentPresenter>(window, PopupIndicatorsClass).Tag.ShouldBe("popup.indicators");
            AllSemantic<Ellipse>(window, PopupIndicatorClass)
                .ShouldAllBe(static control => Equals(control.Tag, "popup.indicator"));
            FindSemantic<AvaloniaButton>(window, PopupCloseClass).Tag.ShouldBe("popup.close");

            // 每个步骤的模板部件都会物化，专用 Style 必须全部命中（无遗漏、无未标记实例）。
            AllSemantic<ContentPresenter>(window, PopupCoverClass)
                .ShouldAllBe(static control => Equals(control.Tag, "popup.cover"));
            AllSemantic<ContentPresenter>(window, PopupTitleClass)
                .ShouldAllBe(static control => Equals(control.Tag, "popup.title"));
            AllSemantic<ContentPresenter>(window, PopupDescriptionClass)
                .ShouldAllBe(static control => Equals(control.Tag, "popup.description"));
        });
    }

    [Fact]
    public void Generated_Mask_Style_Hits_The_TourLayer_Through_Logical_Parent()
    {
        var (root, tour) = CreatePinnedTourRoot(isShowMask: true);
        AttachOwnerPartStyles(tour, "popup.mask");

        ShowInWindow(root, window =>
        {
            var layer = window.GetVisualDescendants().OfType<AtomUITourLayer>().Single();
            layer.Classes.Contains(PopupMaskClass).ShouldBeTrue();
            layer.Parent.ShouldBeSameAs(tour,
                "the shared TourLayer must be logically parented to the active tour owner so the owner-scoped semantic style cascades");
            layer.IsVisible.ShouldBeTrue();
            layer.Tag.ShouldBe("popup.mask");
        });
    }

    [Fact]
    public void MaskLayer_Logical_Parent_Releases_On_Close_And_Reattaches_For_The_Next_Tour()
    {
        var target = CreateTarget();
        var tourA = CreateTour(target, isShowMask: true);
        var tourB = CreateTour(target, isShowMask: true);
        var root = CreateRoot(target, tourA, tourB);

        ShowInWindow(root, window =>
        {
            tourA.ShowTour();
            RunLayout(window);

            var layer = window.GetVisualDescendants().OfType<AtomUITourLayer>().Single();
            layer.Parent.ShouldBeSameAs(tourA);

            tourA.HideTour();
            RunLayout(window);
            layer.Parent.ShouldBeNull(
                "the shared layer must release its logical parent when the owning tour closes");

            tourB.ShowTour();
            RunLayout(window);
            layer.Parent.ShouldBeSameAs(tourB);
        });
    }

    [Fact]
    public void Tour_Reports_The_Mask_Layer_As_A_Cross_Root_Only_While_Open()
    {
        var target = CreateTarget();
        var tour = CreateTour(target, isShowMask: true);
        var root = CreateRoot(target, tour);

        ShowInWindow(root, window =>
        {
            tour.GetCrossRoots().ShouldBeEmpty();

            tour.ShowTour();
            RunLayout(window);

            tour.GetCrossRoots().ShouldHaveSingleItem().ShouldBeOfType<AtomUITourLayer>();

            tour.HideTour();
            RunLayout(window);
            tour.GetCrossRoots().ShouldBeEmpty();
        });
    }

    [Fact]
    public void DefaultTourIndicator_Materializes_Dots_Per_Step_Count_With_Active_State()
    {
        var indicator = new AtomUIDefaultTourIndicator
        {
            IndicatorSize = 6,
            ItemSpacing = 4,
            StepCount = 3,
            ActiveIndex = 1
        };

        ShowInWindow(indicator, window =>
        {
            var dots = GetDots(window);
            dots.Length.ShouldBe(3);
            dots[1].Classes.Contains("active").ShouldBeTrue();
            dots[0].Classes.Contains("active").ShouldBeFalse();

            indicator.StepCount = 5;
            indicator.ActiveIndex = 3;
            RunLayout(window);

            dots = GetDots(window);
            dots.Length.ShouldBe(5);
            dots[3].Classes.Contains("active").ShouldBeTrue();
            dots[1].Classes.Contains("active").ShouldBeFalse();

            // 主题样式必须真正落到圆点上（尺寸 > 0），防止回退为无视觉的死节点。
            foreach (var dot in dots)
            {
                dot.Bounds.Width.ShouldBe(6, 0.01, "dot width comes from the theme style binding");
                dot.Bounds.Height.ShouldBe(6, 0.01);
            }

            // 布局契约：N*size + (N+1)*spacing（左右各一份间距），
            // measure 阶段的期望尺寸即公式值。
            indicator.DesiredSize.Width.ShouldBe(5 * 6 + 6 * 4, 0.01);
        });
    }

    private static Ellipse[] GetDots(Visual root)
    {
        return root.GetVisualDescendants()
                   .OfType<Ellipse>()
                   .Where(control => control.Classes.Contains(PopupIndicatorClass))
                   .ToArray();
    }

    private static (Canvas Root, AtomUITour Tour) CreatePinnedTourRoot(bool isShowMask)
    {
        var target = CreateTarget();
        var tour = CreateTour(target, isShowMask, stepCount: 2, pinned: true);
        if (tour.Steps[0] is AtomUITourStep firstStep)
        {
            firstStep.Cover = new Border
            {
                Width = 120,
                Height = 48
            };
        }
        return (CreateRoot(target, tour), tour);
    }

    private static Border CreateTarget()
    {
        return new Border
        {
            Width = 72,
            Height = 32
        };
    }

    private static AtomUITour CreateTour(Control target, bool isShowMask, int stepCount = 1, bool pinned = false)
    {
        var tour = new AtomUITour
        {
            IsMotionEnabled = false,
            IsShowMask = isShowMask,
            IsPopupPinnedOpen = pinned
        };
        for (var i = 0; i < stepCount; i++)
        {
            tour.Steps.Add(new AtomUITourStep
            {
                Target = target,
                Title = $"Step {i}",
                Description = $"Description {i}"
            });
        }

        return tour;
    }

    private static Canvas CreateRoot(params Control[] children)
    {
        var root = new Canvas
        {
            Width = 800,
            Height = 600
        };
        foreach (var child in children)
        {
            root.Children.Add(child);
        }

        return root;
    }

    private static void AttachOwnerPartStyles(AtomUITour tour, params string[] onlyParts)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUITour), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        tour.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUITour>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            if (onlyParts.Length > 0 && !onlyParts.Contains(part.Name))
            {
                continue;
            }

            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        tour.Styles.Add(ownerStyle);
    }

    private static T FindSemantic<T>(Visual root, string markerClass)
        where T : Control
    {
        return root.GetVisualDescendants()
                   .OfType<T>()
                   .Single(control => control.Classes.Contains(markerClass));
    }

    private static IReadOnlyList<T> AllSemantic<T>(Visual root, string markerClass)
        where T : Control
    {
        return root.GetVisualDescendants()
                   .OfType<T>()
                   .Where(control => control.Classes.Contains(markerClass))
                   .ToArray();
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type controlType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(controlType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single,
        bool runtimeCreated = true)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeTrue();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static void AssertThemeMarkers(string themePath, params (string MarkerClass, string ElementName)[] expected)
    {
        var document = ReadThemeDocument(themePath);
        var markers = document.Descendants()
                              .SelectMany(static element => element.Attributes()
                                  .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                      "Classes.semantic-", StringComparison.Ordinal)))
                              .Select(static attribute =>
                                  $"{attribute.Name.LocalName["Classes.".Length..]}:{attribute.Parent!.Name.LocalName}")
                              .ToArray();

        foreach (var (markerClass, elementName) in expected)
        {
            markers.ShouldContain($"{markerClass}:{elementName}");
        }
    }

    private static XDocument ReadThemeDocument(string relativePath)
    {
        return XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = IOPath.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }

    private static void RunLayout(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        // headless 下 Popup 需经 VisualLayerManager 的 overlay 层承载，
        // 与 TreeSelect 语义测试同构。
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 800,
            Height = 600
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 800,
            Height = 600,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
