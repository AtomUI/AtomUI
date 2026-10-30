using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIPreviewer = AtomUI.Desktop.Controls.ImagePreviewer;
using AtomUIGroupPreviewer = AtomUI.Desktop.Controls.ImageGroupPreviewer;
using AtomUIImageViewer = AtomUI.Desktop.Controls.ImageViewer;
using AtomUIPreviewDialog = AtomUI.Desktop.Controls.ImagePreviewerDialog;
using AtomUIOverlayHost = AtomUI.Desktop.Controls.ImagePreviewerOverlayHost;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerSemanticPartTests
{
    private const string ImageClass = "semantic-image";
    private const string CoverClass = "semantic-cover";
    private const string ScopeCoverClass = "semantic-scope-cover";
    private const string ScopeItemsClass = "semantic-scope-items";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupMaskClass = "semantic-popup-mask";
    private const string PopupBodyClass = "semantic-popup-body";
    private const string PopupFooterClass = "semantic-popup-footer";
    private const string PopupActionsClass = "semantic-popup-actions";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "cover", "image", "popup.actions", "popup.body",
        "popup.footer", "popup.mask", "popup.root"
    ];

    private static readonly Dictionary<string, string[]> ThemeMarkerMap = new(StringComparer.Ordinal)
    {
        ["ImagePreviewerTheme.axaml"] = [ScopeCoverClass],
        ["ImageGroupPreviewerTheme.axaml"] = [ScopeItemsClass],
        ["ImagePreviewerCoverTheme.axaml"] = [ImageClass, CoverClass],
        ["ImageViewerTheme.axaml"] = [PopupBodyClass, PopupFooterClass],
        ["ImagePreviewFloatToolbarTheme.axaml"] = [PopupActionsClass],
        ["ImagePreviewerOverlayHostTheme.axaml"] = [PopupRootClass, PopupMaskClass]
    };

    static ImagePreviewerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ImagePreviewer_Descriptor_Exposes_Only_Approved_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUIPreviewer));
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIPreviewer));
        AssertPart(descriptor, "image", ImageClass, typeof(Control),
            "/template/ .semantic-scope-cover /template/ .semantic-image",
            SemanticPartCardinality.Single, crossNestedOwners: true);
        AssertPart(descriptor, "cover", CoverClass, typeof(Border),
            "/template/ .semantic-scope-cover /template/ .semantic-cover",
            SemanticPartCardinality.Single, crossNestedOwners: true);
        AssertPopupPart(descriptor, "popup.root", PopupRootClass, typeof(Panel),
            ">> .semantic-popup-root", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.mask", PopupMaskClass, typeof(Panel),
            ">> .semantic-popup-mask", SemanticPartCardinality.Optional);
        AssertPopupPart(descriptor, "popup.body", PopupBodyClass, typeof(Panel),
            ">> .semantic-popup-body", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.footer", PopupFooterClass, typeof(Control),
            ">> .semantic-popup-footer", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.actions", PopupActionsClass, typeof(Border),
            ">> .semantic-popup-footer /template/ .semantic-popup-actions",
            SemanticPartCardinality.Single);
    }

    [Fact]
    public void ImageGroupPreviewer_Descriptor_Exposes_Only_Approved_Parts()
    {
        var descriptor = GetDescriptor(typeof(AtomUIGroupPreviewer));
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIGroupPreviewer));
        AssertPart(descriptor, "image", ImageClass, typeof(Control),
            "/template/ .semantic-scope-items >> .semantic-image",
            SemanticPartCardinality.Multiple, runtimeCreated: true, crossNestedOwners: true);
        AssertPart(descriptor, "cover", CoverClass, typeof(Border),
            "/template/ .semantic-scope-items >> .semantic-cover",
            SemanticPartCardinality.Multiple, runtimeCreated: true, crossNestedOwners: true);
        AssertPopupPart(descriptor, "popup.root", PopupRootClass, typeof(Panel),
            ">> .semantic-popup-root", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.mask", PopupMaskClass, typeof(Panel),
            ">> .semantic-popup-mask", SemanticPartCardinality.Optional);
        AssertPopupPart(descriptor, "popup.body", PopupBodyClass, typeof(Panel),
            ">> .semantic-popup-body", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.footer", PopupFooterClass, typeof(Control),
            ">> .semantic-popup-footer", SemanticPartCardinality.Single);
        AssertPopupPart(descriptor, "popup.actions", PopupActionsClass, typeof(Border),
            ">> .semantic-popup-footer /template/ .semantic-popup-actions",
            SemanticPartCardinality.Single);
    }

    [Fact]
    public void Built_In_Templates_Implement_Approved_Static_Markers()
    {
        foreach (var (fileName, expectedMarkers) in ThemeMarkerMap)
        {
            var document = XDocument.Load(GetThemeFile(fileName), LoadOptions.SetLineInfo);
            var markers = document.Descendants()
                                  .SelectMany(static element => element.Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal)))
                                  .Select(static attribute => attribute.Name.LocalName["Classes.".Length..])
                                  .ToArray();
            markers.ShouldBe(expectedMarkers, ignoreOrder: true);
        }

        // 所有主题合计不得出现 .semantic-root（root 隐式，无 marker）。
        foreach (var fileName in ThemeMarkerMap.Keys)
        {
            XDocument.Load(GetThemeFile(fileName), LoadOptions.SetLineInfo)
                     .Descendants()
                     .Any(static element => element.Attributes()
                         .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                     .ShouldBeFalse($"no theme may declare Classes.semantic-root ({fileName})");
        }
    }

    [Fact]
    public void Default_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        foreach (var fileName in ThemeMarkerMap.Keys)
        {
            var selectors = XDocument.Load(GetThemeFile(fileName), LoadOptions.SetLineInfo)
                                     .Descendants()
                                     .Where(static element => element.Name.LocalName == "Style")
                                     .Attributes("Selector")
                                     .Select(static attribute => attribute.Value)
                                     .ToArray();
            selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal),
                $"built-in themes must not consume semantic selectors ({fileName})");
        }
    }

    [Fact]
    public void Generated_Trigger_Styles_Apply_To_ImagePreviewer_Image_And_Cover()
    {
        var owner = new AtomUIPreviewer
        {
            Width = 200,
            Height = 120,
            IsMotionEnabled = false
        };
        AttachOwnerStyle(owner, descriptor =>
            descriptor.Parts.Where(static part => part.Name is "image" or "cover"));

        var window = Show(owner);
        try
        {
            owner.Tag.ShouldBe("root");
            FindSemanticControl<Control>(window, ImageClass).Tag.ShouldBe("image");
            FindSemanticControl<Border>(window, CoverClass).Tag.ShouldBe("cover");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Image_Style_Carries_Image_CornerRadius_To_The_Renderer()
    {
        // Gallery「自定义语义结构的样式」对齐上游 styles.image 的 borderRadius: 4。
        var owner = new AtomUIPreviewer
        {
            Width = 200,
            Height = 120,
            IsMotionEnabled = false
        };
        owner.Classes.Add("semantic-owner");
        var ownerStyle = new Style(s => s.OfType<AtomUIPreviewer>().Class("semantic-owner"));
        var imageStyle = (Style)Activator.CreateInstance(
            GetDescriptor(typeof(AtomUIPreviewer))
                .Parts.Single(static part => part.Name == "image")
                .StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        imageStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(4)));
        ownerStyle.Children.Add(imageStyle);
        owner.Styles.Add(ownerStyle);

        var window = Show(owner);
        try
        {
            var renderer = FindSemanticControl<Control>(window, ImageClass).ShouldBeOfType<ImagePreviewRenderer>();
            renderer.CornerRadius.ShouldBe(new CornerRadius(4));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Trigger_Styles_Apply_To_ImageGroupPreviewer_Items()
    {
        var owner = new AtomUIGroupPreviewer
        {
            Width = 320,
            Height = 120,
            IsMotionEnabled = false,
            ItemsSource = new List<ImagePreviewItem>
            {
                new(ImageSource.Parse("avares://AtomUI.Tests/Assets/first.png")),
                new(ImageSource.Parse("avares://AtomUI.Tests/Assets/second.png"))
            }
        };
        AttachOwnerStyle(owner, descriptor =>
            descriptor.Parts.Where(static part => part.Name is "image" or "cover"));

        var window = Show(owner);
        try
        {
            owner.Tag.ShouldBe("root");
            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(control => control.Classes.Contains(ImageClass))
                  .ShouldBe(2);
            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Where(control => control.Classes.Contains(ImageClass))
                  .ShouldAllBe(control => control.Tag as string == "image");
            window.GetVisualDescendants()
                  .OfType<Border>()
                  .Where(border => border.Classes.Contains(CoverClass))
                  .ShouldAllBe(border => border.Tag as string == "cover");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Overlay_Host_Generated_Popup_Styles_Hit_All_Popup_Parts()
    {
        var owner = new AtomUIPreviewer();
        owner.Classes.Add("semantic-owner");
        var ownerStyle = new Style(s => s.OfType<AtomUIPreviewer>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in GetDescriptor(typeof(AtomUIPreviewer)).Parts.Where(
                     static part => part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        owner.Styles.Add(ownerStyle);

        var window = new AvaloniaWindow { Width = 640, Height = 480 };
        var overlayLayer = new Panel();
        window.Content = new StackPanel { Children = { owner, overlayLayer } };
        window.Show();
        window.UpdateLayout();

        var host = new AtomUIOverlayHost(window, owner)
        {
            ItemsSource = new List<ImagePreviewEntry>
            {
                CreateEntry("avares://AtomUI.Tests/Assets/first.png")
            }
        };
        ((ISetLogicalParent)host).SetParent(owner);
        overlayLayer.Children.Add(host);
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        try
        {
            FindSemanticControl<Panel>(window, PopupRootClass).Tag.ShouldBe("popup.root");
            FindSemanticControl<Panel>(window, PopupMaskClass).Tag.ShouldBe("popup.mask");
            FindSemanticControl<Panel>(window, PopupBodyClass).Tag.ShouldBe("popup.body");
            FindSemanticControl<Control>(window, PopupFooterClass).Tag.ShouldBe("popup.footer");
            FindSemanticControl<Border>(window, PopupActionsClass).Tag.ShouldBe("popup.actions");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Native_Dialog_Carries_Popup_Markers_With_Optional_Absent()
    {
        var owner = new AtomUIPreviewer();
        var window = new AvaloniaWindow { Content = owner };
        window.Show();
        window.UpdateLayout();

        var dialog = new AtomUIPreviewDialog(window, owner)
        {
            ItemsSource = new List<ImagePreviewEntry>
            {
                CreateEntry("avares://AtomUI.Tests/Assets/first.png")
            }
        };
        ((ISetLogicalParent)dialog).SetParent(owner);
        dialog.Show();
        dialog.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            // popup.root 由代码注入；body/footer/actions 来自共享 ImageViewer/浮层模板。
            var popupRoot = FindSemanticControl<Panel>(dialog, PopupRootClass);
            popupRoot.ShouldNotBeNull();
            popupRoot.Children.Single().ShouldBeOfType<AtomUIImageViewer>();

            dialog.GetVisualDescendants().OfType<Panel>().Single(c => c.Classes.Contains(PopupBodyClass)).ShouldNotBeNull();
            dialog.GetVisualDescendants().OfType<Control>().Single(c => c.Classes.Contains(PopupFooterClass)).ShouldNotBeNull();
            dialog.GetVisualDescendants().OfType<Border>().Single(c => c.Classes.Contains(PopupActionsClass)).ShouldNotBeNull();

            // Optional：native dialog 无遮罩层（关闭由 OS 窗口原语承担，无内嵌关闭部件）。
            dialog.GetVisualDescendants().OfType<Panel>().Any(c => c.Classes.Contains(PopupMaskClass)).ShouldBeFalse();
        }
        finally
        {
            dialog.Close();
            window.Close();
        }
    }

    [Fact]
    public void Native_Dialog_Owner_Styles_Do_Not_Cross_The_TopLevel_Boundary()
    {
        var owner = new AtomUIPreviewer();
        owner.Classes.Add("semantic-owner");
        var ownerStyle = new Style(s => s.OfType<AtomUIPreviewer>().Class("semantic-owner"));
        var bodyStyle = (Style)Activator.CreateInstance(
            GetDescriptor(typeof(AtomUIPreviewer))
                .Parts.Single(static part => part.Name == "popup.body")
                .StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        bodyStyle.Setters.Add(new Setter(Control.TagProperty, "popup.body"));
        ownerStyle.Children.Add(bodyStyle);
        owner.Styles.Add(ownerStyle);

        var window = new AvaloniaWindow { Content = owner };
        window.Show();
        window.UpdateLayout();

        var dialog = new AtomUIPreviewDialog(window, owner)
        {
            ItemsSource = new List<ImagePreviewEntry>
            {
                CreateEntry("avares://AtomUI.Tests/Assets/first.png")
            }
        };
        ((ISetLogicalParent)dialog).SetParent(owner);
        dialog.Show();
        dialog.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            // 跨 TopLevel 边界时 owner 作用域 Semantic Style 不级联进 native dialog；
            // marker 仍存在（descriptor/marker 契约与 Gallery AdditionalRoots 解析不依赖样式级联）。
            var body = dialog.GetVisualDescendants().OfType<Panel>().Single(c => c.Classes.Contains(PopupBodyClass));
            body.Tag.ShouldBeNull(
                "owner-scoped Semantic Styles must not cascade into the independent native dialog TopLevel");
        }
        finally
        {
            dialog.Close();
            window.Close();
        }
    }

    [Fact]
    public void Overlay_Host_Open_Close_Reopen_Rematerializes_Popup_Markers()
    {
        var owner = new AtomUIPreviewer();
        var window = new AvaloniaWindow { Width = 640, Height = 480 };
        var overlayLayer = new Panel();
        window.Content = new StackPanel { Children = { owner, overlayLayer } };
        window.Show();
        window.UpdateLayout();

        try
        {
            for (var cycle = 0; cycle < 2; cycle++)
            {
                var host = new AtomUIOverlayHost(window, owner)
                {
                    ItemsSource = new List<ImagePreviewEntry>
                    {
                        CreateEntry("avares://AtomUI.Tests/Assets/first.png")
                    }
                };
                ((ISetLogicalParent)host).SetParent(owner);
                overlayLayer.Children.Add(host);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                FindSemanticControl<Panel>(window, PopupRootClass).ShouldNotBeNull();
                FindSemanticControl<Panel>(window, PopupBodyClass).ShouldNotBeNull();

                overlayLayer.Children.Remove(host);
                ((ISetLogicalParent)host).SetParent(null);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                window.GetVisualDescendants()
                      .OfType<Control>()
                      .Any(control => control.Classes.Contains(PopupRootClass))
                      .ShouldBeFalse($"popup markers must be destroyed after close (cycle {cycle})");
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static ControlSemanticDescriptor GetDescriptor(Type ownerType)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        return descriptor.ShouldNotBeNull();
    }

    private static void AttachOwnerStyle<T>(T owner, Func<ControlSemanticDescriptor, IEnumerable<SemanticPartDescriptor>> select)
        where T : Control
    {
        owner.Classes.Add("semantic-owner");
        var ownerStyle = new Style(s => s.OfType<T>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in select(GetDescriptor(typeof(T))))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        owner.Styles.Add(ownerStyle);
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
        root.CrossVisualRoot.ShouldBeFalse();
        root.RuntimeCreated.ShouldBeFalse();
        root.CrossNestedOwners.ShouldBeFalse();
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality,
        bool runtimeCreated = false,
        bool crossNestedOwners = false)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.CrossNestedOwners.ShouldBe(crossNestedOwners);
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static void AssertPopupPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeTrue();
        part.CrossNestedOwners.ShouldBeTrue();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(Visual root, string marker)
        where T : Control
    {
        return root.GetVisualDescendants()
                   .OfType<T>()
                   .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 240,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static ImagePreviewEntry CreateEntry(string source)
    {
        return new ImagePreviewEntry(new ImagePreviewItem(ImageSource.Parse(source)));
    }

    private static string GetThemeFile(string fileName)
    {
        return GetRepoFile($"src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/{fileName}");
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
