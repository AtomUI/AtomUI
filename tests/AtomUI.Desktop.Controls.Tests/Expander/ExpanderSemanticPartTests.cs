using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIExpander = AtomUI.Desktop.Controls.Expander;
using IconButtonControl = AtomUI.Desktop.Controls.IconButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ExpanderControl;

/// <summary>
/// Expander 的五个语义部件全部是 <c>ExpanderTheme.axaml</c> 单模板内的静态节点
/// （<c>RuntimeCreated=false</c>，无 <c>.semantic-scope-*</c> 锚点，无 C# marker 注入）。
/// 这些测试锁定 descriptor 契约、模板 marker 契约、生成 Style 的精确命中，以及
/// <c>header</c> 位于 <c>PART_HeaderLayoutTransform</c> 内时仍由单一 <c>/template/</c> 路由命中。
/// </summary>
public class ExpanderSemanticPartTests
{
    private const string HeaderClass = "semantic-header";
    private const string IconClass   = "semantic-icon";
    private const string TitleClass  = "semantic-title";
    private const string BodyClass   = "semantic-body";
    private const string OwnerClass  = "semantic-owner";

    static ExpanderSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var descriptor = GetDescriptor();

        // Parts 按 root 优先、其余按 Path ordinal 排序，因此不是声明顺序。
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "body", "header", "icon", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass,
            typeof(PixelAlignedBorder),
            "ExpanderHeaderStyle",
            SemanticPartCardinality.Single);
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass,
            typeof(IconButtonControl),
            "ExpanderIconStyle",
            SemanticPartCardinality.Single);
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            typeof(ContentPresenter),
            "ExpanderTitleStyle",
            SemanticPartCardinality.Single);
        // body 折叠态从视觉树中完全缺席，因此是 Optional（0 或 1 个静态 marker）。
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "body"),
            BodyClass,
            typeof(ContentPresenter),
            "ExpanderBodyStyle",
            SemanticPartCardinality.Optional);
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/Expander/Themes/ExpanderTheme.axaml"),
            LoadOptions.SetLineInfo);

        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        // 字面量 Classes="semantic-*" 不是合法静态 marker；必须使用 Classes.semantic-*="True"。
        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe([
                                "semantic-body:ContentPresenter",
                                "semantic-header:PixelAlignedBorder",
                                "semantic-icon:IconButton",
                                "semantic-title:ContentPresenter"
                            ]);

        // root 由生成器隐式产生，模板不得声明 .semantic-root。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName == "Classes.semantic-root");

        // Expander 没有容器生命周期，因此模板里不应出现任何 scope 锚点。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName.StartsWith("Classes.semantic-scope-", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Owner_Scoped_Markers_Stay_Exactly_One_Per_Declared_Part(bool isExpanded)
    {
        var expander = CreateExpander(isExpanded);

        ShowInWindow(expander, () =>
        {
            var markers = GetOwnerMarkers(expander);

            markers.SelectMany(static control => control.Classes)
                   .Where(static className => className.StartsWith("semantic-", StringComparison.Ordinal))
                   .OrderBy(static className => className, StringComparer.Ordinal)
                   .ShouldBe(isExpanded
                       ? ["semantic-body", "semantic-header", "semantic-icon", "semantic-title"]
                       : ["semantic-header", "semantic-icon", "semantic-title"]);

            FindOwnerMarker(expander, HeaderClass).ShouldBeOfType<PixelAlignedBorder>();
            FindOwnerMarker(expander, IconClass).ShouldBeOfType<IconButtonControl>();
            FindOwnerMarker(expander, TitleClass).ShouldBeOfType<ContentPresenter>();

            // header 位于 LayoutTransformControl 内，但 TemplatedParent 仍是 owner 自身：
            // 单一 /template/ 路由成立，不需要显式 SelectorRoute。
            ReferenceEquals(FindOwnerMarker(expander, HeaderClass).TemplatedParent, expander)
                .ShouldBeTrue();
        });
    }

    /// <summary>
    /// <c>body</c> 的承载节点是 <c>ContentViewport#PART_ContentViewport</c> 的内容。
    /// 折叠稳定态下 actor <c>IsVisible=False</c>，其内部 <c>ContentPresenter</c> 不挂接视觉子级，
    /// 节点只保留逻辑子级、从视觉树缺席；首次展开后节点物化，并携带 owner 的 <c>TemplatedParent</c>。
    /// Avalonia 的 <c>ContentPresenter</c> 只在自身可见时挂接子级、转不可见时不解除挂接，因此
    /// 再次收起后节点保持存在。这正是 descriptor 把 <c>body</c> 声明为 <c>Optional</c> 的原因。
    /// </summary>
    [Fact]
    public void Body_Part_Is_Optional_And_Materializes_On_First_Expansion_As_Owner_Scoped()
    {
        var expander = CreateExpander(false);

        ShowInWindow(expander, () =>
        {
            // 从未展开：body 节点不在视觉树中。
            GetOwnerMarkers(expander, BodyClass).ShouldBeEmpty();
            AssertMarkerCounts(expander, 1, 1, 1, 0);

            expander.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();

            var body = FindOwnerMarker(expander, BodyClass);
            body.ShouldBeOfType<ContentPresenter>();
            ReferenceEquals(body.TemplatedParent, expander).ShouldBeTrue();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            // 收起后节点保持存在（ContentPresenter 不解除挂接），但 actor 已不可见。
            expander.IsExpanded = false;
            Dispatcher.UIThread.RunJobs();
            GetOwnerMarkers(expander, BodyClass).Length.ShouldBe(1);
            GetContentViewport(expander).IsVisible.ShouldBeFalse();

            // header / icon / title 是常驻节点，不受展开状态影响。
            AssertMarkerCounts(expander, 1, 1, 1, 1);
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part()
    {
        var expander = CreateExpander(true);
        expander.Classes.Add(OwnerClass);

        var descriptor = GetDescriptor();
        var ownerStyle = new Style(selector => selector.OfType<AtomUIExpander>().Class(OwnerClass));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull())
                                        .ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        expander.Styles.Add(ownerStyle);

        ShowInWindow(expander, () =>
        {
            var tagged = expander.GetVisualDescendants()
                                 .OfType<Control>()
                                 .Where(static control => control.Tag is string)
                                 .ToArray();

            foreach (var name in new[] { "body", "header", "icon", "title" })
            {
                tagged.Count(control => Equals(control.Tag, name)).ShouldBe(
                    1,
                    $"Expander part '{name}' must hit exactly one node; hits: " +
                    string.Join(
                        " | ",
                        tagged.Where(control => Equals(control.Tag, name))
                              .Select(static control => $"{control.GetType().Name}#{control.Name}")));
            }
        });
    }

    /// <summary>
    /// <c>header</c> 位于 <c>LayoutTransformControl#PART_HeaderLayoutTransform</c> 内，
    /// 但仍属于 owner 的单一模板作用域：路由必须是 <c>/template/</c>，且不得外溢到同窗口内
    /// 未携带 owner class 的另一个 Expander（即不退化为宽泛 logical descendant）。
    /// </summary>
    [Fact]
    public void Header_Part_Is_Reached_By_The_Single_Template_Route_Inside_The_Layout_Transform()
    {
        var styled = CreateExpander(true);
        styled.Classes.Add(OwnerClass);
        styled.Styles.Add(CreateRouteStyle(HeaderClass, "header"));

        var plain = CreateExpander(true);

        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children    = { styled, plain }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var header = FindOwnerMarker(styled, HeaderClass);
            header.Tag.ShouldBe("header");
            ReferenceEquals(header.TemplatedParent, styled).ShouldBeTrue();

            header.GetVisualAncestors()
                  .OfType<Control>()
                  .Any(static control => control.Name == "PART_HeaderLayoutTransform")
                  .ShouldBeTrue("the header marker must stay inside PART_HeaderLayoutTransform");

            FindOwnerMarker(plain, HeaderClass).Tag.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var expander = CreateExpander(true);
        expander.Classes.Add(OwnerClass);
        expander.Styles.Add(CreateRouteStyle(HeaderClass, "header"));
        expander.Styles.Add(CreateRouteStyle(IconClass, "icon"));
        expander.Styles.Add(CreateRouteStyle(TitleClass, "title"));
        expander.Styles.Add(CreateRouteStyle(BodyClass, "body"));

        ShowInWindow(expander, () =>
        {
            FindOwnerMarker(expander, HeaderClass).Tag.ShouldBe("header");
            FindOwnerMarker(expander, IconClass).Tag.ShouldBe("icon");
            FindOwnerMarker(expander, TitleClass).Tag.ShouldBe("title");
            FindOwnerMarker(expander, BodyClass).Tag.ShouldBe("body");
        });
    }

    [Fact]
    public void Marker_Counts_Stay_Stable_Across_State_And_Visual_Mode_Switches()
    {
        var expander = CreateExpander(false);

        ShowInWindow(expander, () =>
        {
            // 折叠态：body 缺席。
            AssertMarkerCounts(expander, 1, 1, 1, 0);

            expander.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            expander.ExpandDirection  = ExpandDirection.Up;
            expander.SizeType         = CustomizableSizeType.Large;
            expander.ExpandIconPosition = ExpanderIconPosition.End;
            expander.TriggerType      = ExpanderTriggerType.Icon;
            expander.IsGhostStyle     = true;
            expander.IsBorderless     = true;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            expander.ExpandDirection = ExpandDirection.Left;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            expander.ExpandDirection = ExpandDirection.Right;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            expander.HeaderPadding  = new Thickness(5);
            expander.ContentPadding = new Thickness(5);
            expander.IsEnabled      = false;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            // 首次展开后 body 节点保持存在，收起不再移除；actor 本身转为不可见。
            expander.IsExpanded = false;
            Dispatcher.UIThread.RunJobs();
            GetContentViewport(expander).IsVisible.ShouldBeFalse();
            AssertMarkerCounts(expander, 1, 1, 1, 1);
        });
    }

    [Fact]
    public void Hiding_The_Expand_Icon_Keeps_The_Icon_Marker_On_The_Owner()
    {
        var expander = CreateExpander(true);
        expander.IsShowExpandIcon = false;

        ShowInWindow(expander, () =>
        {
            var icon = FindOwnerMarker(expander, IconClass).ShouldBeOfType<IconButtonControl>();
            icon.IsVisible.ShouldBeFalse();
            AssertMarkerCounts(expander, 1, 1, 1, 1);
        });
    }

    [Fact]
    public void Detach_And_Reattach_Keeps_One_Owner_Scoped_Marker_Per_Part()
    {
        var expander = CreateExpander(true);
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480
        };

        try
        {
            window.Show();
            window.Content = expander;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            window.Content = expander;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(expander, 1, 1, 1, 1);
        }
        finally
        {
            window.Close();
        }
    }

    private static ControlSemanticDescriptor GetDescriptor()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIExpander), out var descriptor).ShouldBeTrue();
        return descriptor.ShouldNotBeNull();
    }

    private static AtomUIExpander CreateExpander(bool isExpanded)
    {
        return new AtomUIExpander
        {
            Header          = "Header",
            Content         = "Content",
            IsExpanded      = isExpanded,
            IsMotionEnabled = false
        };
    }

    private static Style CreateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUIExpander>().Class(OwnerClass).Template().Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertMarkerCounts(
        AtomUIExpander expander,
        int expectedHeaders,
        int expectedIcons,
        int expectedTitles,
        int expectedBodies)
    {
        GetOwnerMarkers(expander, HeaderClass).Length.ShouldBe(expectedHeaders);
        GetOwnerMarkers(expander, IconClass).Length.ShouldBe(expectedIcons);
        GetOwnerMarkers(expander, TitleClass).Length.ShouldBe(expectedTitles);
        GetOwnerMarkers(expander, BodyClass).Length.ShouldBe(expectedBodies);
    }

    /// <summary>
    /// 只统计 <c>TemplatedParent</c> 为 owner 自身的节点，确保 marker 不会来自嵌套模板边界。
    /// </summary>
    private static Control[] GetOwnerMarkers(TemplatedControl owner, string? marker = null)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => ReferenceEquals(control.TemplatedParent, owner))
                    .Where(static control => control.Classes.Any(
                        className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                    .Where(control => marker is null || control.Classes.Contains(marker))
                    .ToArray();
    }

    private static Control FindOwnerMarker(TemplatedControl owner, string marker)
    {
        return GetOwnerMarkers(owner, marker).Single();
    }

    private static Control GetContentViewport(AtomUIExpander expander)
    {
        return expander.GetVisualDescendants()
                       .OfType<Control>()
                       .Single(static control => control.Name == "PART_ContentViewport");
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIExpander));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string expectedStyleTypeName,
        SemanticPartCardinality cardinality)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        // 静态根模板 Part 未声明 SelectorRoute，由生成器规范化为 "/template/ .<class>"。
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.CrossNestedOwners.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.Theme.ShouldBeNull();
        part.StyleType.ShouldNotBeNull();
        part.StyleType!.Name.ShouldBe(expectedStyleTypeName);
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
