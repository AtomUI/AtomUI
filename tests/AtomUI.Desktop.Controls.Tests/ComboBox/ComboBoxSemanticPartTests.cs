using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AtomUIComboBoxHandle = AtomUI.Desktop.Controls.ComboBoxHandle;
using AtomUIComboBoxItem = AtomUI.Desktop.Controls.ComboBoxItem;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

public class ComboBoxSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string FrameClass = "semantic-frame";
    private const string ContentClass = "semantic-content";
    private const string PlaceholderClass = "semantic-placeholder";
    private const string InputClass = "semantic-input";
    private const string SuffixClass = "semantic-suffix";
    private const string IndicatorClass = "semantic-indicator";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupListClass = "semantic-popup-list";
    private const string PopupListItemClass = "semantic-popup-list-item";
    private const string PopupEmptyClass = "semantic-popup-empty";

    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml";
    private const string HandleThemePath =
        "src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxHandleTheme.axaml";
    private const string ItemThemePath =
        "src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxItemTheme.axaml";
    private const string SharedAddOnThemePath =
        "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml";

    /// <summary>
    /// descriptor 按「root 优先，其后 Path 序数排序」生成。
    /// </summary>
    private static readonly string[] ApprovedPartNames =
    [
        "root", "content", "frame", "indicator", "input", "placeholder",
        "popup.empty", "popup.list", "popup.listItem", "popup.root", "prefix", "suffix"
    ];

    static ComboBoxSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_ComboBox_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIComboBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIComboBox));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix");
        // frame 的物理节点在共享 AddOnDecoratedBox 模板内，因此声明 CrossNestedOwners。
        AssertPart(descriptor, "frame", FrameClass, typeof(PixelAlignedBorder),
            "/template/ .semantic-scope-input /template/ .semantic-frame",
            crossNestedOwners: true);
        AssertPart(descriptor, "content", ContentClass, typeof(Panel),
            "/template/ .semantic-content");
        AssertPart(descriptor, "placeholder", PlaceholderClass, typeof(AvaloniaTextBlock),
            "/template/ .semantic-content > .semantic-placeholder");
        AssertPart(descriptor, "input", InputClass, typeof(AvaloniaTextBox),
            "/template/ .semantic-content > .semantic-input");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix");
        AssertPart(descriptor, "indicator", IndicatorClass, typeof(IconButton),
            ">> .semantic-scope-handle /template/ .semantic-indicator",
            crossNestedOwners: true);
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(Border),
            "/template/ .semantic-popup-root", crossVisualRoot: true);
        AssertPart(descriptor, "popup.list", PopupListClass, typeof(AvaloniaScrollViewer),
            "/template/ .semantic-popup-root >> .semantic-popup-list",
            crossVisualRoot: true);
        AssertPart(descriptor, "popup.empty", PopupEmptyClass, typeof(Border),
            "/template/ .semantic-popup-root >> .semantic-popup-empty",
            crossVisualRoot: true);
        AssertPart(descriptor, "popup.listItem", PopupListItemClass, typeof(AtomUIComboBoxItem),
            "/template/ .semantic-popup-root >> .semantic-popup-list-item",
            SemanticPartCardinality.Multiple, crossVisualRoot: true, runtimeCreated: true);

        // ComboBoxItem 模板内只有一个纯 ContentPresenter，容器级样式已由 popup.listItem 完整覆盖，
        // 因此不注册为独立 owner（见契约 §5）。
        registry.TryGetControl(typeof(AtomUIComboBoxItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AtomUIComboBoxHandle), out _).ShouldBeFalse();
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        AssertStaticMarkers(ThemePath, 1, [
            "Classes.semantic-scope-input:AddOnDecoratedBox",
            "Classes.semantic-scope-handle:ComboBoxHandle",
            "Classes.semantic-prefix:AddOnContentPresenter",
            "Classes.semantic-content:Panel",
            "Classes.semantic-placeholder:TextBlock",
            "Classes.semantic-suffix:StackPanel",
            "Classes.semantic-input:ComboBoxTextBox",
            "Classes.semantic-popup-root:Border",
            "Classes.semantic-popup-list:ScrollViewer",
            "Classes.semantic-popup-empty:Border"
        ]);

        // indicator 的物理节点在 ComboBoxHandle 自有模板内，跨嵌套 owner 校验。
        AssertStaticMarkers(HandleThemePath, 1, [
            "Classes.semantic-indicator:IconButton"
        ]);

        // frame 的物理节点在共享 AddOnDecoratedBox 模板内（与 ToolTip 的 container/arrow 同一形态），
        // marker 必须由该共享主题承载；同一模板另含 prefix / suffix 两个 scope 锚点。
        AssertStaticMarkers(SharedAddOnThemePath, 1, [
            "Classes.semantic-frame:AddOnDecoratedBoxContentFrame",
            "Classes.semantic-scope-prefix:AddOnContentPresenter",
            "Classes.semantic-scope-suffix:AddOnContentPresenter"
        ]);
    }

    [Fact]
    public void Item_Container_Template_Does_Not_Host_The_List_Item_Marker()
    {
        // popup.listItem 的 marker 必须打在容器实例上，不能写进 item 模板内部节点，
        // 否则解析器按「marker 元素本身必须是 ContractType」匹配将永不命中（系统设计 §8.3）。
        var document = XDocument.Load(GetRepoFile(ItemThemePath), LoadOptions.SetLineInfo);
        document.Descendants()
                .SelectMany(static element => element.Attributes())
                .Any(static attribute => attribute.Name.LocalName.StartsWith(
                    "Classes.semantic-", StringComparison.Ordinal))
                .ShouldBeFalse();
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in new[] { ThemePath, HandleThemePath, ItemThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();

            selectors.ShouldAllBe(selector =>
                !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIComboBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };
        comboBox.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIComboBox>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        comboBox.Styles.Add(ownerStyle);

        var window = Show(comboBox);
        try
        {
            comboBox.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(comboBox, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<PixelAlignedBorder>(comboBox, FrameClass).Tag.ShouldBe("frame");
            FindSemanticControl<Panel>(comboBox, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<AvaloniaTextBlock>(comboBox, PlaceholderClass).Tag.ShouldBe("placeholder");
            FindSemanticControl<AvaloniaTextBox>(comboBox, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(comboBox, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<IconButton>(comboBox, IndicatorClass).Tag.ShouldBe("indicator");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Prefix_Part_Presents_The_ContentLeftAddOn_Inline_In_The_Frame()
    {
        // 守护 Gate B 的模板投影改造：ContentLeftAddOn 必须是元素形式的 AddOnContentPresenter，
        // 才能承载 semantic-prefix 并保证前缀内容仍然内联可见（行为语义不变）。
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };

        var window = Show(comboBox);
        try
        {
            var prefix = comboBox.GetVisualDescendants()
                                 .OfType<ContentPresenter>()
                                 .Single(control => control.Classes.Contains(PrefixClass));
            prefix.IsVisible.ShouldBeTrue();
            prefix.Content.ShouldBe("prefix");
            prefix.Bounds.Width.ShouldBeGreaterThan(0);
            prefix.Bounds.Height.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Custom_Frame_Border_Brush_Wins_Over_The_Theme_State_Border()
    {
        // frame 部件的核心价值：把输入框边框改成自定义颜色。
        // 主题在 AddOnDecoratedBox 上按 StyleVariant / Status 设置 BorderBrush，再经 TemplateBinding 转发给
        // frame 节点；Semantic Style 直接作用于 frame 节点，必须赢得这次覆盖（系统设计 §7：Semantic Style
        // 可以有意覆盖 TemplateBinding 提供的默认视觉值）。这里用 Warning 态验证确实压过主题状态色。
        var customBrush = new SolidColorBrush(Color.Parse("#1890FF"));
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            StyleVariant = InputControlStyleVariant.Outlined,
            Status = InputControlStatus.Warning
        };
        comboBox.Classes.Add("combo-frame-demo");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIComboBox>().Class("combo-frame-demo"));
        var frameStyle = new ComboBoxFrameStyle();
        frameStyle.Setters.Add(new Setter(PixelAlignedBorder.BorderBrushProperty, customBrush));
        ownerStyle.Children.Add(frameStyle);
        comboBox.Styles.Add(ownerStyle);

        var window = Show(comboBox);
        try
        {
            var frame = comboBox.GetVisualDescendants()
                                .OfType<PixelAlignedBorder>()
                                .Single(control => control.Classes.Contains(FrameClass));

            frame.BorderBrush.ShouldBeSameAs(customBrush);
            // 边框宽度仍来自主题，未被语义定制波及。
            frame.BorderThickness.ShouldBe(new Thickness(1));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Opened()
    {
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            IsDropDownOpen = true,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.GetVisualDescendants().OfType<Popup>().Single().IsOpen.ShouldBeTrue();

            window.GetVisualDescendants()
                  .OfType<Border>()
                  .Count(control => control.Classes.Contains(PopupRootClass))
                  .ShouldBe(1);
            window.GetVisualDescendants()
                  .OfType<AvaloniaScrollViewer>()
                  .Count(control => control.Classes.Contains(PopupListClass))
                  .ShouldBe(1);

            var listItems = window.GetVisualDescendants()
                                  .OfType<AtomUIComboBoxItem>()
                                  .Where(control => control.Classes.Contains(PopupListItemClass))
                                  .ToArray();
            listItems.Length.ShouldBe(3);
        });
    }

    [Fact]
    public void Popup_Empty_Marker_Replaces_The_List_When_The_Filter_Matches_Nothing()
    {
        // 空态只在生效的过滤模式下出现（IsEditable + IsFilterEnabled + 过滤值 + 无匹配项）。
        // 未开启过滤时 IsEffectiveEmptyVisible 被强制为 false，弹层里是空的列表区而非空态区。
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            IsEditable = true,
            IsFilterEnabled = true,
            IsPopupPinnedOpen = true,
            IsDropDownOpen = true,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" }
        };

        ShowInWindow(comboBox, window =>
        {
            var empties = window.GetVisualDescendants()
                                .OfType<Border>()
                                .Where(control => control.Classes.Contains(PopupEmptyClass))
                                .ToArray();
            empties.Length.ShouldBe(1);
            empties[0].IsEffectivelyVisible.ShouldBeFalse();

            comboBox.Text = "zzz-no-match";
            Dispatcher.UIThread.RunJobs();

            empties[0].IsEffectivelyVisible.ShouldBeTrue();
            window.GetVisualDescendants()
                  .OfType<AvaloniaScrollViewer>()
                  .Single(control => control.Classes.Contains(PopupListClass))
                  .IsEffectivelyVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void List_Item_Marker_Stays_Stable_Across_Container_Recycle()
    {
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            IsDropDownOpen = true,
            ItemsSource = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(comboBox, window =>
        {
            var containers = window.GetVisualDescendants()
                                   .OfType<AtomUIComboBoxItem>()
                                   .ToArray();
            containers.Length.ShouldBe(2);
            containers.ShouldAllBe(container => container.Classes.Contains(PopupListItemClass));

            // 集合替换后复用/重建容器，marker 不得丢失，也不得重复。
            comboBox.ItemsSource = new[] { "Gamma", "Delta", "Epsilon" };
            Dispatcher.UIThread.RunJobs();

            var refreshed = window.GetVisualDescendants()
                                  .OfType<AtomUIComboBoxItem>()
                                  .ToArray();
            refreshed.Length.ShouldBe(3);
            refreshed.ShouldAllBe(container => container.Classes.Contains(PopupListItemClass));
            refreshed.ShouldAllBe(container =>
                container.Classes.Count(candidate => candidate == PopupListItemClass) == 1);
        });
    }

    [Fact]
    public void Consumer_Supplied_Item_Container_Also_Carries_The_List_Item_Marker()
    {
        // NeedsContainerOverride 对显式 ComboBoxItem 返回 false，这类容器不经过
        // CreateContainerForItemOverride，必须在 PrepareContainerForItemOverride 兜底注入。
        var supplied = new AtomUIComboBoxItem { Content = "Supplied" };
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            IsDropDownOpen = true,
            Items = { supplied }
        };

        ShowInWindow(comboBox, _ =>
        {
            supplied.Classes.ShouldContain(PopupListItemClass);
            supplied.Classes.Count(candidate => candidate == PopupListItemClass).ShouldBe(1);
        });
    }

    [Fact]
    public void Editable_State_Switches_Visibility_Without_Moving_Markers()
    {
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false
        };

        var window = Show(comboBox);
        try
        {
            var placeholder = FindSemanticControl<AvaloniaTextBlock>(comboBox, PlaceholderClass);
            var input       = FindSemanticControl<AvaloniaTextBox>(comboBox, InputClass);

            placeholder.Classes.ShouldContain(PlaceholderClass);
            input.Classes.ShouldContain(InputClass);

            comboBox.IsEditable = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            // 状态切换只改变可见性，节点与 marker 常驻（数量语义保持 Single）。
            FindSemanticControl<AvaloniaTextBlock>(comboBox, PlaceholderClass)
                .Classes.ShouldContain(PlaceholderClass);
            FindSemanticControl<AvaloniaTextBox>(comboBox, InputClass)
                .Classes.ShouldContain(InputClass);
            input.IsEffectivelyVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Semantic_Markers_Survive_Template_Reapplication()
    {
        var comboBox = new AtomUIComboBox
        {
            Width = 320,
            IsMotionEnabled = false
        };

        var window = Show(comboBox);
        try
        {
            comboBox.ApplyTemplate();

            var prefix      = FindSemanticControl<ContentPresenter>(comboBox, PrefixClass);
            var content     = FindSemanticControl<Panel>(comboBox, ContentClass);
            var placeholder = FindSemanticControl<AvaloniaTextBlock>(comboBox, PlaceholderClass);
            var input       = FindSemanticControl<AvaloniaTextBox>(comboBox, InputClass);
            var suffix      = FindSemanticControl<StackPanel>(comboBox, SuffixClass);
            var indicator   = FindSemanticControl<IconButton>(comboBox, IndicatorClass);

            foreach (var control in new Control[] { prefix, content, placeholder, input, suffix, indicator })
            {
                control.Classes.Count(candidate => candidate.StartsWith("semantic-", StringComparison.Ordinal))
                       .ShouldBe(1);
            }

            comboBox.GetVisualDescendants()
                    .OfType<Control>()
                    .Any(control => control.Classes.Contains("semantic-root"))
                    .ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertStaticMarkers(string themePath, int expectedTemplateCount, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(themePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
        templates.Length.ShouldBe(expectedTemplateCount);

        var markers = templates
                      .SelectMany(static template => template.Descendants()
                          .SelectMany(static element => element.Attributes()
                              .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                  "Classes.semantic-",
                                  StringComparison.Ordinal)))
                          .Select(static attribute => $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}"))
                      .ToArray();

        // 断言 marker 集合完全一致（不允许多、不允许少）；顺序无关，避免元素移动造成假失败。
        markers.OrderBy(static marker => marker, StringComparer.Ordinal)
               .ShouldBe(expectedMarkers.OrderBy(static marker => marker, StringComparer.Ordinal));

        document.Descendants()
                .Any(static element => element.Attributes()
                    .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                .ShouldBeFalse();
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
        root.Theme.ShouldBeNull();
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single,
        bool crossVisualRoot = false,
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
        part.CrossVisualRoot.ShouldBe(crossVisualRoot);
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.CrossNestedOwners.ShouldBe(crossNestedOwners);
        part.Since.ShouldBe("6.2.0");
        part.Theme.ShouldBeNull();
        part.StyleType.ShouldNotBeNull();
        part.StyleType!.Name.ShouldBe($"ComboBox{ToPascalCase(name)}Style");
    }

    private static string ToPascalCase(string partName)
    {
        return string.Concat(partName.Split('.')
                                     .Select(static segment =>
                                         segment.Length == 0
                                             ? segment
                                             : char.ToUpperInvariant(segment[0]) + segment[1..]));
    }

    private static T FindSemanticControl<T>(AtomUIComboBox owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 640,
            Height = 480
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 480,
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
