using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDrawer = AtomUI.Desktop.Controls.Drawer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

public class DrawerSemanticPartTests
{
    private const string DrawerContainerThemePath =
        "src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerContainerTheme.axaml";

    private const string DrawerInfoContainerThemePath =
        "src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerInfoContainerTheme.axaml";

    // manifest 顺序：隐式 root 在前，其余按 path 字典序。
    private static readonly string[] ApprovedPartNames =
    [
        "root", "body", "close", "extra", "footer", "header", "mask", "section", "title"
    ];

    static DrawerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Drawer_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIDrawer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIDrawer));
        AssertPart(descriptor, "mask", "semantic-mask", typeof(Border), ">> .semantic-mask",
            SemanticPartCardinality.Optional);
        AssertPart(descriptor, "section", "semantic-section", typeof(Border), ">> .semantic-section");
        AssertPart(descriptor, "header", "semantic-header", typeof(Avalonia.Controls.Grid), ">> .semantic-header");
        AssertPart(descriptor, "title", "semantic-title", typeof(AtomUI.Desktop.Controls.TextBlock), ">> .semantic-title");
        AssertPart(descriptor, "extra", "semantic-extra", typeof(ContentPresenter), ">> .semantic-extra");
        AssertPart(descriptor, "body", "semantic-body", typeof(ContentPresenter), ">> .semantic-body");
        AssertPart(descriptor, "footer", "semantic-footer", typeof(ContentPresenter), ">> .semantic-footer");
        AssertPart(descriptor, "close", "semantic-close", typeof(IconButton), ">> .semantic-close");
    }

    [Fact]
    public void Container_Themes_Carry_The_Approved_Static_Markers()
    {
        // Drawer 的 owner 模板为空，全部 marker 静态声明在两个内部容器主题上
        //（RuntimeCreated=true 豁免 owner 模板校验，因此用显式 XDocument 断言锁定）。
        var containerDoc = XDocument.Load(GetRepoFile(DrawerContainerThemePath), LoadOptions.SetLineInfo);
        CollectMarkers(containerDoc).ShouldBe(["semantic-mask:Border"]);

        var infoDoc = XDocument.Load(GetRepoFile(DrawerInfoContainerThemePath), LoadOptions.SetLineInfo);
        CollectMarkers(infoDoc).ShouldBe(
        [
            "semantic-body:ContentPresenter",
            "semantic-close:IconButton",
            "semantic-extra:ContentPresenter",
            "semantic-footer:ContentPresenter",
            "semantic-header:Grid",
            "semantic-section:Border",
            "semantic-title:TextBlock"
        ]);
    }

    [Fact]
    public void Drawer_Parts_Expose_Markers_When_Opened()
    {
        var drawer = new AtomUIDrawer
        {
            Title = "Title",
            Content = new TextBlock { Text = "Body" },
            Extra = new TextBlock { Text = "Extra" },
            Footer = new TextBlock { Text = "Footer" },
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };

        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = drawer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var descendants = window.GetVisualDescendants().ToArray();
            descendants.OfType<Border>().Single(b => b.Name == "PART_Mask")
                       .Classes.Contains("semantic-mask").ShouldBeTrue();
            descendants.OfType<Border>().Single(b => b.Name == "Frame")
                       .Classes.Contains("semantic-section").ShouldBeTrue();
            descendants.OfType<Avalonia.Controls.Grid>().Single(g => g.Name == "InfoHeader")
                        .Classes.Contains("semantic-header").ShouldBeTrue();
            descendants.OfType<AtomUI.Desktop.Controls.TextBlock>().Single(t => t.Name == "HeaderText")
                       .Classes.Contains("semantic-title").ShouldBeTrue();
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "ExtraContentPresenter")
                       .Classes.Contains("semantic-extra").ShouldBeTrue();
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "InfoContainer")
                       .Classes.Contains("semantic-body").ShouldBeTrue();
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "InfoFooter")
                       .Classes.Contains("semantic-footer").ShouldBeTrue();
            descendants.OfType<IconButton>().Single(b => b.Name == "PART_CloseButton")
                       .Classes.Contains("semantic-close").ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void DrawerContainer_Is_Logically_Parented_To_The_Owner_While_Open()
    {
        var drawer = new AtomUIDrawer
        {
            Content = new TextBlock { Text = "Body" },
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };

        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = drawer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var container = window.GetVisualDescendants().OfType<DrawerContainer>().Single();
            container.Parent.ShouldBeSameAs(drawer,
                "打开期间容器必须逻辑挂到 Drawer owner 下，owner 嵌套语义 Style 才可达（系统文档 §9.2）");

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            container.GetVisualParent().ShouldBeNull();
            container.Parent.ShouldBeNull("脱离 layer 后必须解除逻辑父，避免样式宿主残留");

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants().OfType<DrawerContainer>().Single()
                  .Parent.ShouldBeSameAs(drawer, "复用容器重开时必须重新挂回逻辑父");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Container_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIDrawer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var drawer = new AtomUIDrawer
        {
            Title = "Title",
            Content = new TextBlock { Text = "Body" },
            Extra = new TextBlock { Text = "Extra" },
            Footer = new TextBlock { Text = "Footer" },
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        drawer.Classes.Add("semantic-owner");
        var ownerStyle = new Avalonia.Styling.Style(
            selector => selector.OfType<AtomUIDrawer>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        drawer.Styles.Add(ownerStyle);

        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = drawer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var descendants = window.GetVisualDescendants().ToArray();
            descendants.OfType<Border>().Single(b => b.Name == "PART_Mask").Tag.ShouldBe("mask");
            descendants.OfType<Border>().Single(b => b.Name == "Frame").Tag.ShouldBe("section");
            descendants.OfType<Avalonia.Controls.Grid>().Single(g => g.Name == "InfoHeader").Tag.ShouldBe("header");
            descendants.OfType<AtomUI.Desktop.Controls.TextBlock>().Single(t => t.Name == "HeaderText").Tag.ShouldBe("title");
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "ExtraContentPresenter").Tag.ShouldBe("extra");
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "InfoContainer").Tag.ShouldBe("body");
            descendants.OfType<ContentPresenter>().Single(p => p.Name == "InfoFooter").Tag.ShouldBe("footer");
            descendants.OfType<IconButton>().Single(b => b.Name == "PART_CloseButton").Tag.ShouldBe("close");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pinned_Drawer_Ignores_Mask_Click_And_Close_Button()
    {
        var drawer = new AtomUIDrawer
        {
            Content = new TextBlock { Text = "Body" },
            IsCloseOnMaskClick = true,
            IsPinnedOpen = true,
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };

        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = drawer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var mask = window.GetVisualDescendants().OfType<Border>()
                             .Single(b => b.Name == "PART_Mask");
            RaisePointerReleased(mask, MouseButton.Left, PointerUpdateKind.LeftButtonReleased);
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen.ShouldBeTrue("钉住时遮罩点击不得关闭（语义预览常开）");

            var closeButton = window.GetVisualDescendants().OfType<IconButton>()
                                    .Single(b => b.Name == "PART_CloseButton");
            closeButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(
                AtomUI.Desktop.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen.ShouldBeTrue("钉住时关闭按钮不得关闭（语义预览常开）");

            // 外部代码直接置 IsOpen=false 仍可关闭（受控常开只拦交互路径）。
            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen.ShouldBeFalse();

            // 取消钉住后恢复常规交互关闭。
            drawer.IsPinnedOpen = false;
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            RaisePointerReleased(mask, MouseButton.Left, PointerUpdateKind.LeftButtonReleased);
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen.ShouldBeFalse("未钉住时遮罩点击应保持关闭行为");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Reports_The_Container_As_The_Live_Cross_Root()
    {
        var drawer = new AtomUIDrawer
        {
            Content = new TextBlock { Text = "Body" },
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        drawer.GetCrossRoots().ShouldBeEmpty();

        var raised = 0;
        drawer.CrossRootsChanged += (_, _) => raised++;

        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = drawer };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var container = window.GetVisualDescendants().OfType<DrawerContainer>().Single();
            drawer.GetCrossRoots().ShouldHaveSingleItem().ShouldBeSameAs(container);

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            drawer.GetCrossRoots().ShouldBeEmpty();
            raised.ShouldBeGreaterThanOrEqualTo(2);
        }
        finally
        {
            window.Close();
        }
    }

    private static void RaisePointerReleased(Control source, MouseButton button, PointerUpdateKind updateKind)
    {
        source.RaiseEvent(new PointerReleasedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.None, updateKind),
            KeyModifiers.None,
            button));
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
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
        part.StyleType.Name.ShouldBe($"Drawer{Pascal(name)}Style");
    }

    private static string Pascal(string partName)
    {
        return char.ToUpperInvariant(partName[0]) + partName[1..];
    }

    private static string[] CollectMarkers(XDocument document)
    {
        return document.Descendants()
                       .SelectMany(static element => element.Attributes()
                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                               "Classes.semantic-", StringComparison.Ordinal)))
                       .Select(static attribute =>
                           $"{attribute.Name.LocalName["Classes.".Length..]}:{attribute.Parent!.Name.LocalName}")
                       .OrderBy(static marker => marker, StringComparer.Ordinal)
                       .ToArray();
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
