using System.Reflection;
using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AtomUIColorPicker = AtomUI.Desktop.Controls.ColorPicker;

namespace AtomUI.Desktop.Controls.Tests.ColorPicker;

public class ColorPickerSemanticPartTests
{
    private const string BodyClass = "semantic-body";
    private const string ContentClass = "semantic-content";
    private const string DescriptionClass = "semantic-description";
    private const string PopupRootClass = "semantic-popup-root";

    private static readonly string[] ApprovedPartNames =
        ["root", "body", "content", "description", "popup.root"];

    private static readonly (string Path, string Element, string Marker)[] ApprovedMarkers =
    [
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml",
            "ColorBlock", "Classes.semantic-body"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml",
            "TextBlock", "Classes.semantic-description"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml",
            "ColorPickerPopupRootFrame", "Classes.semantic-popup-root"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml",
            "ColorBlock", "Classes.semantic-body"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml",
            "WrapPanel", "Classes.semantic-description"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml",
            "ColorPickerPopupRootFrame", "Classes.semantic-popup-root"),
        ("src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml",
            "Border", "Classes.semantic-content"),
    ];

    static ColorPickerSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_ColorPicker_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIColorPicker), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIColorPicker));
        AssertPart(descriptor, "body", BodyClass, typeof(Control),
            "/template/ .semantic-body");
        AssertPart(descriptor, "content", ContentClass, typeof(Border),
            "/template/ .semantic-body /template/ .semantic-content");
        AssertPart(descriptor, "description", DescriptionClass, typeof(Avalonia.Controls.TextBlock),
            "/template/ .semantic-description");

        var popupRoot = descriptor.Parts.Single(static part => part.Name == "popup.root");
        popupRoot.SelectorClass.ShouldBe(PopupRootClass);
        popupRoot.SelectorRoute.ShouldBe("/template/ .semantic-popup-root");
        popupRoot.ContractType.ShouldBe(typeof(Border));
        popupRoot.CrossVisualRoot.ShouldBeTrue();
        popupRoot.Since.ShouldBe("6.2.0");
        popupRoot.StyleType.ShouldNotBeNull();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_GradientColorPicker_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.GradientColorPicker), out var descriptor)
                .ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUI.Desktop.Controls.GradientColorPicker));
        AssertPart(descriptor, "body", BodyClass, typeof(Control),
            "/template/ .semantic-body");
        AssertPart(descriptor, "content", ContentClass, typeof(Border),
            "/template/ .semantic-body /template/ .semantic-content");
        // 渐变控件触发文本区是 WrapPanel（PART_ColorTextPanel），ContractType 放宽到 Panel
        AssertPart(descriptor, "description", DescriptionClass, typeof(Panel),
            "/template/ .semantic-description");

        var popupRoot = descriptor.Parts.Single(static part => part.Name == "popup.root");
        popupRoot.SelectorClass.ShouldBe(PopupRootClass);
        popupRoot.SelectorRoute.ShouldBe("/template/ .semantic-popup-root");
        popupRoot.ContractType.ShouldBe(typeof(Border));
        popupRoot.CrossVisualRoot.ShouldBeTrue();
        popupRoot.Since.ShouldBe("6.2.0");
        popupRoot.StyleType.ShouldNotBeNull();
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        foreach (var (path, element, marker) in ApprovedMarkers)
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            document.Descendants()
                    .Any(static e => e.Name.LocalName == "ControlTemplate")
                    .ShouldBeTrue($"'{path}' 应包含 ControlTemplate");
            document.Descendants()
                    .Any(e => e.Name.LocalName == element &&
                              e.Attributes().Any(a => a.Name.LocalName == marker))
                    .ShouldBeTrue($"'{path}' 的 {element} 缺少 {marker} marker");
        }

        // root 是隐式部件，任何模板都不得标注 semantic-root
        foreach (var path in ApprovedMarkers.Select(static m => m.Path).Distinct())
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            document.Descendants()
                    .Any(static e => e.Attributes()
                         .Any(static a => a.Name.LocalName == "Classes.semantic-root"))
                    .ShouldBeFalse();
        }
    }

    [Theory]
    [MemberData(nameof(PickerTypes))]
    public void Pinned_Open_Suppresses_Light_Dismiss_And_Unpin_Restores_It(Type pickerType)
    {
        // 对齐 Select/AutoComplete 钉住先例：钉住期间抑制 light-dismiss 遮罩（Avalonia 只在
        // 弹层打开瞬间读取该值建遮罩，遮罩不再拦截全窗交互）；取消钉住恢复模板默认值。
        var picker = (AtomUI.Desktop.Controls.AbstractColorPicker)Activator.CreateInstance(pickerType)!;
        var window = Show(picker, pinnedOpen: true);
        try
        {
            var popup = picker.GetVisualDescendants()
                              .OfType<AtomUI.Desktop.Controls.Popup>()
                              .First(control => control.Name == "PART_Popup");
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            picker.SetCurrentValue(
                AtomUI.Desktop.Controls.AbstractColorPicker.IsPopupPinnedOpenProperty, false);
            Dispatcher.UIThread.RunJobs();

            popup.IsLightDismissEnabled.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Picker_Popup_Templates_Must_Not_Bind_IsOpen()
    {
        // 模板充气阶段 IsOpen 绑定求值会在抑制/布局就绪前打开弹层（Cascader 同陷阱），
        // 弹层必须由代码在模板应用后驱动（对齐 AutoComplete OpeningDropDown 思路）
        foreach (var path in new[]
                 {
                     "src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml",
                     "src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml"
                 })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            var popup = document.Descendants().First(static e => (string?)e.Attribute("Name") == "PART_Popup");
            popup.Attribute("IsOpen").ShouldBeNull();
        }
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in ApprovedMarkers.Select(static m => m.Path).Distinct())
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            document.Descendants()
                    .Where(static e => e.Name.LocalName == "Style")
                    .Attributes("Selector")
                    .Select(static a => a.Value)
                    .ShouldAllBe(static selector =>
                        !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void IsPopupPinnedOpen_Is_Public_Api()
    {
        var property = typeof(AtomUI.Desktop.Controls.AbstractColorPicker)
            .GetProperty(nameof(AtomUI.Desktop.Controls.AbstractColorPicker.IsPopupPinnedOpen));
        property.ShouldNotBeNull();
        property.GetGetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
        property.GetSetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void Generated_Part_Styles_Apply_To_Trigger_And_Popup_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIColorPicker), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var picker = new AtomUIColorPicker
        {
            DefaultValue = Avalonia.Media.Color.Parse("#1677ff"),
            IsMotionEnabled = false
        };
        picker.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIColorPicker>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static p => p.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        picker.Styles.Add(ownerStyle);

        var window = Show(picker, pinnedOpen: true);
        try
        {
            picker.Tag.ShouldBe("root");
            FindSemanticControl<AtomUI.Desktop.Controls.ColorBlock>(picker, BodyClass)
                .Tag.ShouldBe("body");
            FindSemanticControl<Border>(picker, ContentClass).Tag.ShouldBe("content");
            // 命名空间 AtomUI.Desktop.Controls.Tests.ColorPicker 内简单名 TextBlock 会被
            // AtomUI.Desktop.Controls.TextBlock 遮蔽，必须用别名指向 Avalonia.Controls.TextBlock
            FindSemanticControl<AvaloniaTextBlock>(picker, DescriptionClass).Tag.ShouldBe("description");
            // popup.root 在弹层 overlay 宿主内，从 window 根查找
            FindSemanticControl<Border>(window, PopupRootClass).Tag.ShouldBe("popup.root");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(PickerTypes))]
    public void Popup_Direct_Child_Remains_The_Arrow_Decorated_Box(Type pickerType)
    {
        // Popup.cs 的箭头/阴影遮罩机制只识别直接 Child 为 IArrowAwareShadowMaskInfoProvider
        // （Popup.cs:621），popup.root 语义宿主 ColorPickerPopupRootFrame（Border 子类）直接作为
        // Popup 的 Child 承载弹层外沿（语义边框贴合弹层边缘），并向内容层 ArrowDecoratedBox
        // 转发 IArrowAwareShadowMaskInfoProvider 契约，保持箭头/阴影机制完整。
        var picker = (Control)Activator.CreateInstance(pickerType)!;
        picker.SetValue(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty, Avalonia.Media.Brushes.Transparent); // no-op 占位，防止未用告警
        var window = new AvaloniaWindow { Width = 640, Height = 480, Content = picker };
        window.Show();
        picker.ApplyTemplate();
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        try
        {
            var popup = picker.GetVisualDescendants()
                              .OfType<AtomUI.Desktop.Controls.Popup>()
                              .First(control => control.Name == "PART_Popup");
            var child = popup.Child.ShouldNotBeNull();
            child.ShouldBeOfType<AtomUI.Desktop.Controls.ColorPickerPopupRootFrame>();
            var provider = child.ShouldBeAssignableTo<AtomUI.Controls.IArrowAwareShadowMaskInfoProvider>();
            var box = provider!.GetArrowDecoratedBox().ShouldNotBeNull();
            box.ShouldBeOfType<AtomUI.Desktop.Controls.ArrowDecoratedBox>();
        }
        finally
        {
            window.Close();
        }
    }

    public static TheoryData<Type> PickerTypes => new()
    {
        typeof(AtomUI.Desktop.Controls.ColorPicker),
        typeof(AtomUI.Desktop.Controls.GradientColorPicker)
    };

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

    private static T FindSemanticControl<T>(Visual root, string marker)
        where T : Control
    {
        return root.GetVisualDescendants()
                   .OfType<T>()
                   .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(AtomUI.Desktop.Controls.AbstractColorPicker picker, bool pinnedOpen)
    {
        // ColorPicker 的弹层声明 ShouldUseOverlayPopup=true（宿主在窗口 overlay 层），
        // 而普通 Window 的 VisualLayerManager 默认不启用 PopupOverlayLayer，弹层会因
        // "Unable to create IPopupImpl and no overlay layer is found" 无法打开；
        // 与 GalleryBrowserShellView.ConfigureOverlayLayers 相同的方式显式启用 overlay 层。
        var visualLayerManager = new VisualLayerManager { Child = picker };
        visualLayerManager.EnableOverlayLayer = true;
        SetVisualLayerManagerProperty(visualLayerManager, "EnablePopupOverlayLayer", true);
        _ = GetVisualLayerManagerProperty(visualLayerManager, "OverlayLayer");
        _ = GetVisualLayerManagerProperty(visualLayerManager, "PopupOverlayLayer");
        _ = GetVisualLayerManagerProperty(visualLayerManager, "LightDismissOverlayLayer");

        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 480,
            Content = visualLayerManager
        };
        window.Show();
        picker.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        if (pinnedOpen)
        {
            // pinned-open 在窗口完成首次布局后再置位（对齐 SelectBehaviorTests 的模式），
            // 避免在模板 attach 过程中同步触发 Popup.Open。
            picker.SetCurrentValue(
                AtomUI.Desktop.Controls.AbstractColorPicker.IsPopupPinnedOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
        }

        return window;
    }

    private static void SetVisualLayerManagerProperty(
        VisualLayerManager visualLayerManager,
        string propertyName,
        object? value)
    {
        var propertyInfo = typeof(VisualLayerManager)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo.ShouldNotBeNull();
        propertyInfo.SetValue(visualLayerManager, value);
    }

    private static object? GetVisualLayerManagerProperty(
        VisualLayerManager visualLayerManager,
        string propertyName)
    {
        var propertyInfo = typeof(VisualLayerManager)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo.ShouldNotBeNull();
        return propertyInfo.GetValue(visualLayerManager);
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
    }
}
