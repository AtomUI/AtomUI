using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Primitives;
using AtomUI.Animations;
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
using AtomUICheckBox = AtomUI.Desktop.Controls.CheckBox;
using AtomUIListTransfer = AtomUI.Desktop.Controls.ListTransfer;
using AtomUITreeTransfer = AtomUI.Desktop.Controls.TreeTransfer;
using AtomUITransferListItem = AtomUI.Desktop.Controls.TransferListItem;
using AtomUITransferTreeViewItem = AtomUI.Desktop.Controls.TransferTreeViewItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Transfer;

public class TransferSemanticPartTests
{
    private const string SourceClass      = "semantic-source";
    private const string TargetClass      = "semantic-target";
    private const string ActionsClass     = "semantic-actions";
    private const string HeaderClass      = "semantic-header";
    private const string TitleClass       = "semantic-title";
    private const string BodyClass        = "semantic-body";
    private const string ListClass        = "semantic-list";
    private const string FooterClass      = "semantic-footer";
    private const string SectionScopeClass = "semantic-scope-section";
    private const string ItemIconClass    = "semantic-item-icon";
    private const string ItemContentClass = "semantic-item-content";
    private const string InheritedItemClass = "semantic-item";
    private const string ItemClass        = "semantic-item";
    private const string SourceItemClass  = "semantic-source-item";
    private const string TargetItemClass  = "semantic-target-item";

    private static readonly string[] OwnerThemePaths =
    [
        "src/AtomUI.Desktop.Controls/Transfer/Themes/ListTransferTheme.axaml",
        "src/AtomUI.Desktop.Controls/Transfer/Themes/TreeTransferTheme.axaml"
    ];

    private const string DecoratorThemePath =
        "src/AtomUI.Desktop.Controls/Transfer/Themes/TransferItemDecoratorTheme.axaml";

    private const string ListItemThemePath =
        "src/AtomUI.Desktop.Controls/Transfer/Themes/TransferListItemTheme.axaml";

    static TransferSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AbstractTransfer), out _).ShouldBeFalse();

        foreach (var ownerType in new[] { typeof(AtomUIListTransfer), typeof(AtomUITreeTransfer) })
        {
            registry.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();
            descriptor.Parts.Select(static part => part.Name)
                      .OrderBy(static name => name, StringComparer.Ordinal)
                      .ShouldBe(ownerType == typeof(AtomUIListTransfer)
                       ?
                       [
                           "actions", "body", "footer", "header", "item", "itemContent", "itemIcon",
                           "list", "root",
                           "source.body", "source.footer", "source.header", "source.item",
                           "source.itemContent", "source.itemIcon", "source.list",
                           "source.section", "source.title",
                           "target.body", "target.footer", "target.header", "target.item",
                           "target.itemContent", "target.itemIcon", "target.list",
                           "target.section", "target.title", "title"
                       ]
                       :
                       [
                           "actions", "body", "footer", "header", "item", "list", "root",
                           "source.body", "source.footer", "source.header", "source.item",
                           "source.list", "source.section", "source.title",
                           "target.body", "target.footer", "target.header", "target.item",
                           "target.list", "target.section", "target.title", "title"
                       ]);

            AssertRoot(descriptor, ownerType);
            AssertPart(descriptor, "source.section", SourceClass, typeof(TemplatedControl),
                "/template/ .semantic-source");
            AssertPart(descriptor, "target.section", TargetClass, typeof(TemplatedControl),
                "/template/ .semantic-target");
            AssertPart(descriptor, "actions", ActionsClass, typeof(StackPanel),
                "/template/ .semantic-actions");
            AssertPart(descriptor, "header", HeaderClass, typeof(PixelAlignedBorder),
                $"/template/ .{SectionScopeClass} /template/ .semantic-header",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "title", TitleClass, typeof(ContentPresenter),
                $"/template/ .{SectionScopeClass} /template/ .semantic-title",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "body", BodyClass, typeof(DockPanel),
                $"/template/ .{SectionScopeClass} /template/ .semantic-body",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "list", ListClass, typeof(ContentPresenter),
                $"/template/ .{SectionScopeClass} /template/ .semantic-list",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "footer", FooterClass, typeof(PixelAlignedBorder),
                $"/template/ .{SectionScopeClass} /template/ .semantic-footer",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertDirectionPart(descriptor, "source.header", HeaderClass, typeof(PixelAlignedBorder), SourceClass);
            AssertDirectionPart(descriptor, "target.header", HeaderClass, typeof(PixelAlignedBorder), TargetClass);
            AssertDirectionPart(descriptor, "source.title", TitleClass, typeof(ContentPresenter), SourceClass);
            AssertDirectionPart(descriptor, "target.title", TitleClass, typeof(ContentPresenter), TargetClass);
            AssertDirectionPart(descriptor, "source.body", BodyClass, typeof(DockPanel), SourceClass);
            AssertDirectionPart(descriptor, "target.body", BodyClass, typeof(DockPanel), TargetClass);
            AssertDirectionPart(descriptor, "source.list", ListClass, typeof(ContentPresenter), SourceClass);
            AssertDirectionPart(descriptor, "target.list", ListClass, typeof(ContentPresenter), TargetClass);
            AssertDirectionPart(descriptor, "source.footer", FooterClass, typeof(PixelAlignedBorder), SourceClass);
            AssertDirectionPart(descriptor, "target.footer", FooterClass, typeof(PixelAlignedBorder), TargetClass);

            var itemContract = ownerType == typeof(AtomUIListTransfer)
                ? typeof(AtomUITransferListItem)
                : typeof(AtomUITransferTreeViewItem);
            // TreeTransfer 的目标侧是平面 TransferListView，容器为 TransferListItem
            var targetItemContract = typeof(AtomUITransferListItem);
            AssertPart(descriptor, "item", ItemClass, itemContract,
                ">> .semantic-item",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "source.item", SourceItemClass, itemContract,
                ">> .semantic-source-item",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            AssertPart(descriptor, "target.item", TargetItemClass, targetItemContract,
                ">> .semantic-target-item",
                SemanticPartCardinality.Multiple, runtimeCreated: true);
            if (ownerType == typeof(AtomUIListTransfer))
            {
                AssertPart(descriptor, "itemIcon", ItemIconClass, typeof(AtomUICheckBox),
                    ">> .semantic-item /template/ .semantic-item-icon",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
                AssertPart(descriptor, "source.itemIcon", ItemIconClass, typeof(AtomUICheckBox),
                    ">> .semantic-source-item /template/ .semantic-item-icon",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
                AssertPart(descriptor, "target.itemIcon", ItemIconClass, typeof(AtomUICheckBox),
                    ">> .semantic-target-item /template/ .semantic-item-icon",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
                AssertPart(descriptor, "itemContent", ItemContentClass, typeof(ContentPresenter),
                    ">> .semantic-item /template/ .semantic-item-content",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
                AssertPart(descriptor, "source.itemContent", ItemContentClass, typeof(ContentPresenter),
                    ">> .semantic-source-item /template/ .semantic-item-content",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
                AssertPart(descriptor, "target.itemContent", ItemContentClass, typeof(ContentPresenter),
                    ">> .semantic-target-item /template/ .semantic-item-content",
                    SemanticPartCardinality.Multiple, runtimeCreated: true);
            }
        }
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        foreach (var themePath in OwnerThemePaths)
        {
            var document = XDocument.Load(GetRepoFile(themePath), LoadOptions.SetLineInfo);
            var template = document.Descendants()
                                   .Single(static element => element.Name.LocalName == "ControlTemplate");

            var decorators = template.Descendants()
                                     .Where(static element => element.Name.LocalName == "TransferItemDecorator")
                                     .ToArray();
            decorators.Length.ShouldBe(2);
            decorators.Count(static element => HasMarker(element, SourceClass) &&
                                               HasMarker(element, SectionScopeClass)).ShouldBe(1);
            decorators.Count(static element => HasMarker(element, TargetClass) &&
                                               HasMarker(element, SectionScopeClass)).ShouldBe(1);

            var actions = template.Descendants()
                                  .Single(static element => element.Name.LocalName == "StackPanel" &&
                                                            element.Attribute("Name")?.Value == "ActionsLayout");
            HasMarker(actions, ActionsClass).ShouldBeTrue();

            template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
            AssertNoLiteralSemanticClasses(document);
        }

        var decoratorTheme = XDocument.Load(GetRepoFile(DecoratorThemePath), LoadOptions.SetLineInfo);
        AssertNamedMarker(decoratorTheme, "HeaderFrame", HeaderClass);
        AssertNamedMarker(decoratorTheme, "TitleContentPresenter", TitleClass);
        AssertNamedMarker(decoratorTheme, "BodyLayout", BodyClass);
        AssertNamedMarker(decoratorTheme, "ContentPresenter", ListClass);
        AssertNamedMarker(decoratorTheme, "FooterFrame", FooterClass);
        decoratorTheme.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
        AssertNoLiteralSemanticClasses(decoratorTheme);

        var listItemTheme = XDocument.Load(GetRepoFile(ListItemThemePath), LoadOptions.SetLineInfo);
        AssertNamedMarker(listItemTheme, "SelectedIndicator", ItemIconClass);
        AssertNamedMarker(listItemTheme, "ContentPresenter", ItemContentClass);
        listItemTheme.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
        AssertNoLiteralSemanticClasses(listItemTheme);
    }

    [Fact]
    public void Default_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        foreach (var themePath in OwnerThemePaths.Append(DecoratorThemePath).Append(ListItemThemePath))
        {
            var document = XDocument.Load(GetRepoFile(themePath), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();

            selectors.ShouldAllBe(static selector =>
                !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    public static IEnumerable<object[]> TransferOwners()
    {
        yield return [typeof(AtomUIListTransfer)];
        yield return [typeof(AtomUITreeTransfer)];
    }

    [Theory]
    [MemberData(nameof(TransferOwners))]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets(Type ownerType)
    {
        // MemberData 会在 xUnit 发现阶段物化数据，控件必须在测试线程内创建
        var transfer = ownerType == typeof(AtomUIListTransfer)
            ? CreatePopulatedListTransfer()
            : (AbstractTransfer)CreatePopulatedTreeTransfer();
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(transfer.GetType(), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        transfer.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector
            .OfType(transfer.GetType()).Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        // 方向限定变体（source.header 等）由专用用例覆盖，这里验证双侧部件
        foreach (var part in descriptor.Parts.Where(static part =>
                     part.Name is "actions" or "body" or "footer" or "header" or "list"
                         or "source.section" or "target.section" or "title"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        transfer.Styles.Add(ownerStyle);

        var window = Show(transfer);
        try
        {
            transfer.Tag.ShouldBe("root");

            FindSemantic<TemplatedControl>(transfer, SourceClass).Tag.ShouldBe("source.section");
            FindSemantic<TemplatedControl>(transfer, TargetClass).Tag.ShouldBe("target.section");
            FindSemantic<StackPanel>(transfer, ActionsClass).Tag.ShouldBe("actions");

            foreach (var header in FindAllSemantic<PixelAlignedBorder>(transfer, HeaderClass))
            {
                header.Tag.ShouldBe("header");
            }
            foreach (var title in FindAllSemantic<ContentPresenter>(transfer, TitleClass))
            {
                title.Tag.ShouldBe("title");
            }
            foreach (var body in FindAllSemantic<DockPanel>(transfer, BodyClass))
            {
                body.Tag.ShouldBe("body");
            }
            foreach (var listHost in FindAllSemantic<ContentPresenter>(transfer, ListClass))
            {
                listHost.Tag.ShouldBe("list");
            }
            foreach (var footer in FindAllSemantic<PixelAlignedBorder>(transfer, FooterClass))
            {
                footer.Tag.ShouldBe("footer");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Markers_Remain_Stable_When_State_Changes()
    {
        var transfer = CreatePopulatedListTransfer();
        var window = Show(transfer);
        try
        {
            var source      = FindSemantic<TemplatedControl>(transfer, SourceClass);
            var target      = FindSemantic<TemplatedControl>(transfer, TargetClass);
            var actions     = FindSemantic<StackPanel>(transfer, ActionsClass);
            var headers     = FindAllSemantic<PixelAlignedBorder>(transfer, HeaderClass);
            var titles      = FindAllSemantic<ContentPresenter>(transfer, TitleClass);
            var bodies      = FindAllSemantic<DockPanel>(transfer, BodyClass);
            var listHosts   = FindAllSemantic<ContentPresenter>(transfer, ListClass);
            var footers     = FindAllSemantic<PixelAlignedBorder>(transfer, FooterClass);

            headers.Length.ShouldBe(2);
            titles.Length.ShouldBe(2);
            bodies.Length.ShouldBe(2);
            listHosts.Length.ShouldBe(2);
            footers.Length.ShouldBe(2);
            footers.ShouldAllBe(static footer => !footer.IsEffectivelyVisible);

            transfer.IsOneWay        = true;
            transfer.IsFilterEnabled = true;
            transfer.Status          = InputControlStatus.Error;
            transfer.SourceViewFooter = "footer";
            Dispatcher.UIThread.RunJobs();

            FindSemantic<TemplatedControl>(transfer, SourceClass).ShouldBeSameAs(source);
            FindSemantic<TemplatedControl>(transfer, TargetClass).ShouldBeSameAs(target);
            FindSemantic<StackPanel>(transfer, ActionsClass).ShouldBeSameAs(actions);
            FindAllSemantic<PixelAlignedBorder>(transfer, HeaderClass).ShouldBe(headers);
            FindAllSemantic<ContentPresenter>(transfer, TitleClass).ShouldBe(titles);
            FindAllSemantic<DockPanel>(transfer, BodyClass).ShouldBe(bodies);
            FindAllSemantic<ContentPresenter>(transfer, ListClass).ShouldBe(listHosts);
            FindAllSemantic<PixelAlignedBorder>(transfer, FooterClass).ShouldBe(footers);
            footers[0].IsEffectivelyVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Items_Inherit_Motion_And_Hover_Transitions_By_Default()
    {
        var transfer = CreatePopulatedListTransfer();
        transfer.IsMotionEnabled = true;
        var window = Show(transfer);
        try
        {
            var decorator = transfer.GetVisualDescendants()
                                    .OfType<TransferItemDecorator>()
                                    .First();
            decorator.IsMotionEnabled.ShouldBeTrue();

            var view = transfer.GetVisualDescendants()
                               .OfType<TransferListView>()
                               .First();
            view.IsMotionEnabled.ShouldBeTrue();

            var container = transfer.GetVisualDescendants()
                                    .OfType<AtomUITransferListItem>()
                                    .First();
            container.IsMotionEnabled.ShouldBeTrue();
            var transitions = container.Transitions.ShouldNotBeNull();
            transitions
                     .OfType<SolidColorBrushTransition>()
                     .Any(static transition => transition.Property is { Name: "Background" })
                     .ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Qualified_Section_Parts_Apply_To_The_Direction_Side_Only()
    {
        var transfer = CreatePopulatedListTransfer();
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIListTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        transfer.Classes.Add("semantic-qualified-demo");
        var ownerStyle = new Style(selector => selector
            .OfType<AtomUIListTransfer>().Class("semantic-qualified-demo"));
        foreach (var name in new[]
                 {
                     "source.header", "target.header", "source.list", "target.list"
                 })
        {
            ownerStyle.Children.Add(CreatePartStyle(descriptor.ShouldNotBeNull(), name));
        }

        transfer.Styles.Add(ownerStyle);

        var window = Show(transfer);
        try
        {
            var sourceHeader = FindDirectionSemantic<PixelAlignedBorder>(transfer, HeaderClass, SourceClass);
            var targetHeader = FindDirectionSemantic<PixelAlignedBorder>(transfer, HeaderClass, TargetClass);
            sourceHeader.Tag.ShouldBe("source.header");
            targetHeader.Tag.ShouldBe("target.header");

            var sourceList = FindDirectionSemantic<ContentPresenter>(transfer, ListClass, SourceClass);
            var targetList = FindDirectionSemantic<ContentPresenter>(transfer, ListClass, TargetClass);
            sourceList.Tag.ShouldBe("source.list");
            targetList.Tag.ShouldBe("target.list");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Qualified_Item_Parts_And_Markers_Apply_Per_Direction()
    {
        var transfer = CreatePopulatedListTransfer();
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIListTransfer), out var listDescriptor).ShouldBeTrue();
        listDescriptor.ShouldNotBeNull();

        // 生成的条目限定样式直接挂在 Transfer 作用域下即可命中（Nesting().Descendant() 链）
        var itemOwnerStyle = new Style(selector => selector
            .OfType<AtomUIListTransfer>().Class("semantic-qualified-item-demo"));
        itemOwnerStyle.Children.Add(CreatePartStyle(listDescriptor.ShouldNotBeNull(), "source.itemIcon"));
        itemOwnerStyle.Children.Add(CreatePartStyle(listDescriptor.ShouldNotBeNull(), "target.itemContent"));
        transfer.Classes.Add("semantic-qualified-item-demo");

        var window = new AvaloniaWindow { Width = 640, Height = 360, Content = transfer };
        window.Styles.Add(itemOwnerStyle);
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var containers = transfer.GetVisualDescendants()
                                     .OfType<AtomUITransferListItem>()
                                     .ToArray();
            containers.Length.ShouldBe(3);

            var sourceContainers = containers
                                   .Where(container => container.Classes.Contains(SourceItemClass))
                                   .ToArray();
            var targetContainers = containers
                                   .Where(container => container.Classes.Contains(TargetItemClass))
                                   .ToArray();
            (sourceContainers.Length + targetContainers.Length).ShouldBe(containers.Length);
            sourceContainers.ShouldAllBe(static container =>
                !container.Classes.Contains(TargetItemClass));
            targetContainers.ShouldAllBe(static container =>
                !container.Classes.Contains(SourceItemClass));
            sourceContainers.Length.ShouldBe(2);
            targetContainers.Length.ShouldBe(1);

            foreach (var container in sourceContainers)
            {
                FindSemantic<AtomUICheckBox>(container, ItemIconClass).Tag.ShouldBe("source.itemIcon");
            }

            foreach (var container in targetContainers)
            {
                FindSemantic<ContentPresenter>(container, ItemContentClass).Tag.ShouldBe("target.itemContent");
                FindSemantic<AtomUICheckBox>(container, ItemIconClass).Tag.ShouldBeNull();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Nested_List_Containers_Carry_The_Inherited_Item_Marker()
    {
        var transfer = new AtomUIListTransfer
        {
            Width       = 520,
            Height      = 260,
            IsMotionEnabled = false,
            ItemsSource = new[]
            {
                CreateListItem("first"),
                CreateListItem("second"),
                CreateListItem("third")
            },
            TargetKeys = new List<EntityKey> { "third" }
        };

        var window = Show(transfer);
        try
        {
            var containers = transfer.GetVisualDescendants()
                                     .OfType<AtomUITransferListItem>()
                                     .ToArray();
            containers.Length.ShouldBe(3);
            containers.ShouldAllBe(static container =>
                container.Classes.Contains(InheritedItemClass));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Nested_Tree_Containers_Carry_The_Inherited_Item_Marker()
    {
        var child = new TreeItemNode { ItemKey = "child", Header = "Child" };
        var parent = new TreeItemNode { ItemKey = "parent", Header = "Parent" };
        parent.Children.Add(child);
        var transfer = new AtomUITreeTransfer
        {
            Width = 520,
            Height = 260,
            IsMotionEnabled = false,
            ItemsSource = new[] { parent }
        };

        var window = Show(transfer);
        try
        {
            var containers = transfer.GetVisualDescendants()
                                     .OfType<AtomUITransferTreeViewItem>()
                                     .ToArray();
            containers.Length.ShouldBeGreaterThanOrEqualTo(1);
            containers.ShouldAllBe(static container =>
                container.Classes.Contains(InheritedItemClass));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Generated_Styles_Apply_To_All_Items_From_The_Owner_Scope()
    {
        var transfer = CreatePopulatedListTransfer();
        transfer.Classes.Add("semantic-item-demo");

        var window = new AvaloniaWindow { Width = 640, Height = 360, Content = transfer };
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIListTransfer), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        // 未限定条目样式同样直接挂在 Transfer 作用域下，命中源、目标两侧全部条目
        var ownerStyle = new Style(selector => selector
            .OfType<AtomUIListTransfer>().Class("semantic-item-demo"));
        foreach (var part in descriptor.Parts.Where(static part =>
                     part.Name is "itemIcon" or "itemContent"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        window.Styles.Add(ownerStyle);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var containers = transfer.GetVisualDescendants()
                                     .OfType<AtomUITransferListItem>()
                                     .ToArray();
            containers.Length.ShouldBe(3);

            foreach (var container in containers)
            {
                FindSemantic<AtomUICheckBox>(container, ItemIconClass).Tag.ShouldBe("itemIcon");
                FindSemantic<ContentPresenter>(container, ItemContentClass).Tag.ShouldBe("itemContent");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Owner_SizeType_Does_Not_Drive_Any_Visual_Branch()
    {
        var transfer = CreatePopulatedListTransfer();
        var window = Show(transfer);
        try
        {
            var header = FindAllSemantic<PixelAlignedBorder>(transfer, HeaderClass)[0];
            var listHost = FindAllSemantic<ContentPresenter>(transfer, ListClass)[0];
            var source = FindSemantic<TemplatedControl>(transfer, SourceClass);

            var headerHeight = header.Bounds.Height;
            var hostHeight   = listHost.Bounds.Height;
            var sectionWidth = source.Bounds.Width;

            foreach (var sizeType in new[]
                     {
                         CustomizableSizeType.Large,
                         CustomizableSizeType.Small,
                         CustomizableSizeType.Custom
                     })
            {
                transfer.SizeType = sizeType;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                header.Bounds.Height.ShouldBe(headerHeight);
                listHost.Bounds.Height.ShouldBe(hostHeight);
                source.Bounds.Width.ShouldBe(sectionWidth);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIListTransfer CreatePopulatedListTransfer()
    {
        return new AtomUIListTransfer
        {
            Width       = 520,
            Height      = 260,
            IsMotionEnabled = false,
            SourceTitle = "Source",
            TargetTitle = "Target",
            ItemsSource = new[]
            {
                CreateListItem("first"),
                CreateListItem("second"),
                CreateListItem("third")
            },
            TargetKeys = new List<EntityKey> { "third" }
        };
    }

    private static AtomUITreeTransfer CreatePopulatedTreeTransfer()
    {
        var child = new TreeItemNode { ItemKey = "child", Header = "Child" };
        var parent = new TreeItemNode { ItemKey = "parent", Header = "Parent" };
        parent.Children.Add(child);
        return new AtomUITreeTransfer
        {
            Width = 520,
            Height = 260,
            IsMotionEnabled = false,
            SourceTitle = "Source",
            TargetTitle = "Target",
            ItemsSource = new[] { parent },
            TargetKeys = new List<EntityKey> { "child" }
        };
    }

    private static ListItemData CreateListItem(string key)
    {
        return new ListItemData
        {
            ItemKey = key,
            Content = key
        };
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type ownerType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(ownerType);
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
        bool runtimeCreated = false)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static void AssertDirectionPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string directionClass)
    {
        AssertPart(descriptor, name, selectorClass, contractType,
            $"/template/ .{directionClass} /template/ .{selectorClass}",
            SemanticPartCardinality.Single, runtimeCreated: true);
    }

    private static void AssertItemDirectionPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string directionClass)
    {
        AssertPart(descriptor, name, selectorClass, contractType,
            $".{directionClass} /template/ .{selectorClass}",
            SemanticPartCardinality.Multiple);
    }

    private static Style CreatePartStyle(ControlSemanticDescriptor descriptor, string name)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        style.Setters.Add(new Setter(Control.TagProperty, name));
        return style;
    }

    private static T FindDirectionSemantic<T>(Control scope, string marker, string directionClass)
        where T : Visual
    {
        return FindAllSemantic<T>(scope, marker)
               .Single(control => control.GetSelfAndVisualAncestors()
                                        .Any(ancestor => ancestor.Classes.Contains(directionClass)));
    }

    private static T FindSemantic<T>(Control scope, string marker)
        where T : StyledElement
    {
        return FindAllSemantic<T>(scope, marker).Single();
    }

    private static T[] FindAllSemantic<T>(Control scope, string marker)
        where T : StyledElement
    {
        return scope.GetVisualDescendants()
                    .OfType<T>()
                    .Where(control => control.Classes.Contains(marker))
                    .ToArray();
    }

    private static void AssertNamedMarker(XDocument document, string elementName, string marker)
    {
        var element = document.Descendants()
                              .Single(candidate => candidate.Attribute("Name")?.Value == elementName);
        HasMarker(element, marker).ShouldBeTrue($"element '{elementName}' should carry '{marker}'");
    }

    private static void AssertNoLiteralSemanticClasses(XDocument document)
    {
        var literalMarkers = document.Descendants()
                                     .Attributes("Classes")
                                     .Where(static attribute => attribute.Value.Split(
                                         (char[]?)null,
                                         StringSplitOptions.RemoveEmptyEntries)
                                         .Any(static value => value.StartsWith(
                                             "semantic-",
                                             StringComparison.Ordinal)))
                                     .ToArray();
        literalMarkers.ShouldBeEmpty();
    }

    private static bool HasMarker(XElement element, string marker)
    {
        if (((string?)element.Attribute("Classes"))
            ?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Contains(marker, StringComparer.Ordinal) == true)
        {
            return true;
        }

        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var enabled) &&
               enabled;
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 360,
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
