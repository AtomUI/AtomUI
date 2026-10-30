using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUICascader = AtomUI.Desktop.Controls.Cascader;

namespace AtomUI.Desktop.Controls.Tests.Cascader;

public class CascaderSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string ContentClass = "semantic-content";
    private const string PlaceholderClass = "semantic-placeholder";
    private const string InputClass = "semantic-input";
    private const string SuffixClass = "semantic-suffix";
    private const string ClearClass = "semantic-clear";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupListClass = "semantic-popup-list";
    private const string ItemClass = "semantic-item";
    private const string ItemContentClass = "semantic-item-content";
    private const string ItemRemoveClass = "semantic-item-remove";
    private const string PopupListItemClass = "semantic-popup-list-item";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "clear", "content", "input", "item", "itemContent", "itemRemove",
        "placeholder", "popup.list", "popup.listItem", "popup.root", "prefix", "suffix"
    ];

    static CascaderSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Cascader_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUICascader), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUICascader));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "content", ContentClass, typeof(Panel),
            "/template/ .semantic-content");
        AssertPart(descriptor, "item", ItemClass, typeof(AtomUI.Desktop.Controls.Tag),
            ">> .semantic-scope-tags >> .semantic-item", SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "itemContent", ItemContentClass, typeof(TextBlock),
            ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content");
        AssertPart(descriptor, "itemRemove", ItemRemoveClass, typeof(IconButton),
            ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove");
        AssertPart(descriptor, "placeholder", PlaceholderClass, typeof(TextBlock),
            "/template/ .semantic-content > .semantic-placeholder");
        AssertPart(descriptor, "input", InputClass, typeof(TextBox),
            "/template/ .semantic-content > .semantic-input");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix");
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            ">> .semantic-scope-handle /template/ .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(Border),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.list", PopupListClass, typeof(Control),
            "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "popup.listItem", PopupListItemClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list-item",
            SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
        templates.Length.ShouldBe(1);

        foreach (var template in templates)
        {
            var markers = template.Descendants()
                                  .SelectMany(static element => element.Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal)))
                                  .Select(static attribute => $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}")
                                  .ToArray();
            markers.ShouldBe([
                "Classes.semantic-scope-input:CascaderAddOnDecoratedBox",
                "Classes.semantic-prefix:AddOnContentPresenter",
                "Classes.semantic-suffix:StackPanel",
                "Classes.semantic-scope-handle:SelectHandle",
                "Classes.semantic-content:Panel",
                "Classes.semantic-placeholder:TextBlock",
                "Classes.semantic-input:SelectFilterTextBox",
                "Classes.semantic-scope-tags:SelectTagAwareTextBox",
                "Classes.semantic-popup-root:Border",
                "Classes.semantic-scope-view:CascaderView"
            ]);
        }

        document.Descendants()
                .Any(static element => element.Attributes()
                    .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                .ShouldBeFalse();
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();

        selectors.ShouldAllBe(static selector =>
            !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUICascader), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var cascader = new AtomUICascader
        {
            Width = 320,
            IsAllowClear = true,
            IsMotionEnabled = false
        };
        cascader.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUICascader>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        cascader.Styles.Add(ownerStyle);

        var window = Show(cascader);
        try
        {
            cascader.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(cascader, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<Panel>(cascader, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<TextBlock>(cascader, PlaceholderClass).Tag.ShouldBe("placeholder");
            FindSemanticControl<TextBox>(cascader, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(cascader, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<AvaloniaButton>(cascader, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Tag_Part_Styles_Apply_To_Multiple_Selection_Tags()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUICascader), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var cascader = new AtomUICascader
        {
            Width = 420,
            IsMultiple = true,
            IsMotionEnabled = false,
            OptionsSource = new List<CascaderOption>
            {
                new()
                {
                    Header = "Zhejiang",
                    Children = { new CascaderOption { Header = "Hangzhou" } }
                }
            }
        };
        cascader.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUICascader>().Class("semantic-owner"));
        foreach (var partName in new[] { "item", "itemContent", "itemRemove" })
        {
            var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            var capturedName = partName;
            partStyle.Setters.Add(new Setter(Control.TagProperty, capturedName));
            ownerStyle.Children.Add(partStyle);
        }
        cascader.Styles.Add(ownerStyle);

        var window = Show(cascader);
        try
        {
            // 直接赋值选中项触发 tag 创建，无需打开弹层
            var leaf = ((List<CascaderOption>)cascader.OptionsSource)[0].Children[0];
            cascader.SelectedOptions = new List<ICascaderOption> { leaf };
            Dispatcher.UIThread.RunJobs();

            var tag = cascader.GetVisualDescendants()
                              .OfType<Control>()
                              .FirstOrDefault(control => control.Classes.Contains(ItemClass));
            tag.ShouldNotBeNull();
            cascader.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .First(control => control.Classes.Contains(ItemContentClass))
                    .Tag.ShouldBe("itemContent");
            cascader.GetVisualDescendants()
                    .OfType<AvaloniaButton>()
                    .First(control => control.Classes.Contains(ItemRemoveClass))
                    .Tag.ShouldBe("itemRemove");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Prefix_Part_Presents_The_ContentLeftAddOn_Inline_In_The_Frame()
    {
        // 回归：语义 prefix presenter 位于模板属性值子树，TemplatedParent 为 null，
        // 曾误用 TemplateBinding 绑定外层 LeftAddOn API，导致 presenter 恒为空、
        // 示例回退为带分隔线的外层附件渲染。必须经 $parent 编译绑定呈现公共
        // ContentLeftAddOn API，并在内容框内联展示。
        var cascader = new AtomUICascader
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };

        var window = Show(cascader);
        try
        {
            var prefix = cascader.GetVisualDescendants()
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

    private static T FindSemanticControl<T>(AtomUICascader owner, string marker)
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
