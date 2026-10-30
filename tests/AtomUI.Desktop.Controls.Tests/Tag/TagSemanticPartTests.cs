using System.Collections.ObjectModel;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICheckableTag = AtomUI.Desktop.Controls.CheckableTag;
using AtomUICheckableTagGroup = AtomUI.Desktop.Controls.CheckableTagGroup;
using AtomUIIconButton = AtomUI.Desktop.Controls.IconButton;
using AtomUITag = AtomUI.Desktop.Controls.Tag;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tag;

public class TagSemanticPartTests
{
    private const string IconClass = "semantic-icon";
    private const string ContentClass = "semantic-content";
    private const string CloseClass = "semantic-close";
    private const string ItemClass = "semantic-item";
    private const string ScopeItemsClass = "semantic-scope-items";
    private const string OwnerClass = "semantic-owner";

    static TagSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUITag), out var tagDescriptor).ShouldBeTrue();
        tagDescriptor.ShouldNotBeNull();
        tagDescriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "close", "content", "icon"]);

        AssertRoot(tagDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUITag));
        AssertStaticPart(tagDescriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass,
            typeof(IconPresenter),
            "/template/ .semantic-icon");
        AssertStaticPart(tagDescriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass,
            typeof(AtomUITextBlock),
            "/template/ .semantic-content");
        AssertStaticPart(tagDescriptor.Parts.Single(static part => part.Name == "close"),
            CloseClass,
            typeof(AtomUIIconButton),
            "/template/ .semantic-close");

        registry.TryGetControl(typeof(AtomUICheckableTagGroup), out var groupDescriptor).ShouldBeTrue();
        groupDescriptor.ShouldNotBeNull();
        groupDescriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "item"]);

        AssertRoot(groupDescriptor.Parts.Single(static part => part.Name == "root"),
            typeof(AtomUICheckableTagGroup));
        AssertRuntimeItemPart(groupDescriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomUICheckableTag),
            "/template/ .semantic-scope-items > .semantic-item");

        registry.TryGetControl(typeof(AtomUICheckableTag), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AbstractTag), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AbstractCheckableTag), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(AbstractCheckableTagGroup), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Tag/Themes/TagTheme.axaml",
            [
                "semantic-close:IconButton",
                "semantic-content:TextBlock",
                "semantic-icon:IconPresenter",
                // 以下两个 marker 由 Cascader 的 itemContent / itemRemove 部件经
                // 跨嵌套校验消费（SelectTag : Tag 复用本模板），对 Tag 契约是 inert class。
                "semantic-item-content:TextBlock",
                "semantic-item-remove:IconButton"
            ]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Tag/Themes/CheckableTagGroupTheme.axaml",
            [
                "semantic-scope-items:CheckableTagItemsControl"
            ]);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Tag/Themes/CheckableTagTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Tag/Themes/CheckableTagItemsControlTheme.axaml",
            Array.Empty<string>());
    }

    [Fact]
    public void Default_Tag_Keeps_All_Three_Static_Markers_And_Collapses_Unused_Nodes()
    {
        var tag = new AtomUITag
        {
            Text = "Ant Design"
        };

        ShowInWindow(tag, () =>
        {
            AssertTagMarkerCounts(tag);
            GetSemanticIcon(tag).IsVisible.ShouldBeFalse();
            GetSemanticClose(tag).IsVisible.ShouldBeFalse();
            GetSemanticContent(tag).IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Icon_And_Closable_Tags_Show_Nodes_And_Keep_Markers()
    {
        var tag = new AtomUITag
        {
            Icon       = CreateIcon(),
            Text       = "Ant Design",
            IsClosable = true
        };

        ShowInWindow(tag, () =>
        {
            AssertTagMarkerCounts(tag);
            GetSemanticIcon(tag).IsVisible.ShouldBeTrue();
            GetSemanticClose(tag).IsVisible.ShouldBeTrue();
            GetSemanticContent(tag).IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void IsClosable_Toggle_Changes_Only_Visibility_And_Keeps_Markers()
    {
        var tag = new AtomUITag
        {
            Text = "Closable"
        };

        ShowInWindow(tag, () =>
        {
            AssertTagMarkerCounts(tag);
            GetSemanticClose(tag).IsVisible.ShouldBeFalse();

            tag.IsClosable = true;
            Dispatcher.UIThread.RunJobs();
            GetSemanticClose(tag).IsVisible.ShouldBeTrue();
            AssertTagMarkerCounts(tag);

            tag.IsClosable = false;
            Dispatcher.UIThread.RunJobs();
            GetSemanticClose(tag).IsVisible.ShouldBeFalse();
            AssertTagMarkerCounts(tag);
        });
    }

    [Fact]
    public void Close_Button_Click_Raises_Closed_Event_And_Keeps_Markers()
    {
        var closedCount = 0;
        var tag = new AtomUITag
        {
            Text       = "Close",
            IsClosable = true
        };
        tag.Closed += (_, _) => closedCount++;

        ShowInWindow(tag, () =>
        {
            var closeButton = tag.GetVisualDescendants()
                                 .OfType<AbstractIconButton>()
                                 .Single(static button => button.Name == "PART_CloseButton");
            closeButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, closeButton));
            Dispatcher.UIThread.RunJobs();

            closedCount.ShouldBe(1);
            AssertTagMarkerCounts(tag);
        });
    }

    [Fact]
    public void Group_Options_Produce_One_Item_Marker_Per_Container()
    {
        var group = new AtomUICheckableTagGroup
        {
            Options = new[] { "Movies", "Books", "Music" }
        };

        ShowInWindow(group, () =>
        {
            AssertGroupMarkerCounts(group, 3);

            var selectionHost = group.GetVisualDescendants()
                                     .OfType<SelectingItemsControl>()
                                     .Single();
            selectionHost.Classes.Contains(ScopeItemsClass).ShouldBeTrue();
        });
    }

    [Fact]
    public void Empty_Options_Produce_No_Item_Markers()
    {
        var group = new AtomUICheckableTagGroup
        {
            Options = Array.Empty<string>()
        };

        ShowInWindow(group, () =>
        {
            AssertGroupMarkerCounts(group, 0);

            var selectionHost = group.GetVisualDescendants()
                                     .OfType<SelectingItemsControl>()
                                     .Single();
            selectionHost.Classes.Contains(ScopeItemsClass).ShouldBeTrue();
        });
    }

    [Fact]
    public void Collection_Reset_Regenerates_The_Marker_Set()
    {
        var options = new ObservableCollection<string> { "Movies", "Books" };
        var group = new AtomUICheckableTagGroup
        {
            Options = options
        };

        ShowInWindow(group, () =>
        {
            AssertGroupMarkerCounts(group, 2);

            options.Clear();
            Dispatcher.UIThread.RunJobs();
            AssertGroupMarkerCounts(group, 0);

            options.Add("Music");
            options.Add("Sports");
            Dispatcher.UIThread.RunJobs();
            AssertGroupMarkerCounts(group, 2);
        });
    }

    [Fact]
    public void Checked_Toggle_And_Mode_Conversion_Do_Not_Change_Markers()
    {
        var group = new AtomUICheckableTagGroup
        {
            Options            = new[] { "Movies", "Books", "Music" },
            DefaultCheckedItem = "Books"
        };

        ShowInWindow(group, () =>
        {
            AssertGroupMarkerCounts(group, 3);

            group.CheckedItem = "Music";
            Dispatcher.UIThread.RunJobs();
            AssertGroupMarkerCounts(group, 3);

            group.IsMultiple = true;
            Dispatcher.UIThread.RunJobs();
            AssertGroupMarkerCounts(group, 3);

            group.CheckedItems = new AvaloniaList<object?> { "Movies", "Music" };
            Dispatcher.UIThread.RunJobs();
            AssertGroupMarkerCounts(group, 3);
        });
    }

    [Fact]
    public void Color_Variants_Do_Not_Change_Markers()
    {
        var tag = new AtomUITag
        {
            Text = "Color"
        };

        ShowInWindow(tag, () =>
        {
            foreach (var variant in new[] { TagVariant.Filled, TagVariant.Solid, TagVariant.Outlined })
            {
                tag.Variant = variant;
                foreach (var color in new[] { "red", "green", "success", "warning", "error", "processing", "#f50" })
                {
                    tag.TagColor = color;
                    Dispatcher.UIThread.RunJobs();
                    AssertTagMarkerCounts(tag);
                }
            }

            tag.TagColor = null;
            Dispatcher.UIThread.RunJobs();
            AssertTagMarkerCounts(tag);
        });
    }

    [Fact]
    public void Root_Style_Setter_Overrides_Preset_Color_And_Restores_On_Removal()
    {
        var baseline = new AtomUITag
        {
            TagColor = "red",
            Variant  = TagVariant.Solid,
            Text     = "Color"
        };
        var styled = new AtomUITag
        {
            TagColor = "red",
            Variant  = TagVariant.Solid,
            Text     = "Color"
        };
        styled.Classes.Add(OwnerClass);
        styled.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITag>().Class(OwnerClass))
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Yellow) }
        });

        var window = new AvaloniaWindow
        {
            Width  = 800,
            Height = 600,
            Content = new StackPanel
            {
                Children = { baseline, styled }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var presetBackground = GetBrushColor(baseline.Background);
            presetBackground.ShouldNotBe(Colors.Yellow);
            // Selector-style setters sit above the color state machine's Template priority writes.
            GetBrushColor(styled.Background).ShouldBe(Colors.Yellow);

            styled.Classes.Remove(OwnerClass);
            Dispatcher.UIThread.RunJobs();
            GetBrushColor(styled.Background).ShouldBe(presetBackground);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Semantic_Part_Projects_CornerRadius_To_Frame()
    {
        var tag = new AtomUITag
        {
            Text          = "Root",
            CornerRadius = new CornerRadius(8)
        };

        ShowInWindow(tag, () =>
        {
            GetFrame(tag).CornerRadius.ShouldBe(new CornerRadius(8));
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var tag = new AtomUITag
        {
            Text       = "Text",
            IsClosable = true
        };
        tag.Classes.Add(OwnerClass);
        tag.Styles.Add(CreateTagTemplateRouteStyle(IconClass, "icon"));
        tag.Styles.Add(CreateTagTemplateRouteStyle(ContentClass, "content"));
        tag.Styles.Add(CreateTagTemplateRouteStyle(CloseClass, "close"));

        var group = new AtomUICheckableTagGroup
        {
            Options = new[] { "Movies", "Books" }
        };
        group.Classes.Add(OwnerClass);
        group.Styles.Add(CreateGroupItemRouteStyle(ItemClass, "item"));

        var window = new AvaloniaWindow
        {
            Width  = 800,
            Height = 600,
            Content = new StackPanel
            {
                Children = { tag, group }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetSemanticIcon(tag).ShouldNotBeNull().Tag.ShouldBe((object)"icon");
            GetSemanticContent(tag).ShouldNotBeNull().Tag.ShouldBe((object)"content");
            GetSemanticClose(tag).ShouldNotBeNull().Tag.ShouldBe((object)"close");
            GetSemanticItems(group).ShouldAllBe(static item => Equals(item.Tag, "item"));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Route_Styles_Apply_Through_Template_And_Scope_Routes()
    {
        var tag = new AtomUITag
        {
            Icon       = CreateIcon(),
            Text       = "Styled",
            IsClosable = true
        };
        tag.Classes.Add(OwnerClass);
        tag.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITag>().Class(OwnerClass).Template().Class(IconClass))
        {
            Setters = { new Setter(IconPresenter.WidthProperty, 24d) }
        });
        tag.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITag>().Class(OwnerClass).Template().Class(ContentClass))
        {
            Setters = { new Setter(AtomUITextBlock.LineHeightProperty, 20d) }
        });
        tag.Styles.Add(new Style(selector =>
            selector.OfType<AtomUITag>().Class(OwnerClass).Template().Class(CloseClass))
        {
            Setters = { new Setter(AtomUIIconButton.IconWidthProperty, 18d) }
        });

        var group = new AtomUICheckableTagGroup
        {
            Options = new[] { "Movies", "Books" }
        };
        group.Classes.Add(OwnerClass);
        group.Styles.Add(new Style(selector =>
            selector.OfType<AtomUICheckableTagGroup>()
                    .Class(OwnerClass)
                    .Template()
                    .Class(ScopeItemsClass)
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)) }
        });

        var window = new AvaloniaWindow
        {
            Width  = 800,
            Height = 600,
            Content = new StackPanel
            {
                Children = { tag, group }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetSemanticIcon(tag).Width.ShouldBe(24d);
            GetSemanticContent(tag).LineHeight.ShouldBe(20d);
            GetSemanticClose(tag).IconWidth.ShouldBe(18d);
            GetSemanticItems(group).ShouldAllBe(static item => Equals(item.CornerRadius, new CornerRadius(6)));
        }
        finally
        {
            window.Close();
        }
    }

    private static PathIcon CreateIcon()
    {
        return new PathIcon { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") };
    }

    private static Color GetBrushColor(IBrush? brush)
    {
        return ((ISolidColorBrush)brush!).Color;
    }

    private static Style CreateTagTemplateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUITag>()
                    .Class(OwnerClass)
                    .Template()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static Style CreateGroupItemRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomUICheckableTagGroup>()
                    .Class(OwnerClass)
                    .Template()
                    .Class(ScopeItemsClass)
                    .Child()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertTagMarkerCounts(AtomUITag tag)
    {
        GetSemanticIcons(tag).Length.ShouldBe(1);
        GetSemanticContents(tag).Length.ShouldBe(1);
        GetSemanticCloses(tag).Length.ShouldBe(1);
    }

    private static void AssertGroupMarkerCounts(AtomUICheckableTagGroup group, int expectedItems)
    {
        GetSemanticItems(group).Length.ShouldBe(expectedItems);
    }

    private static IconPresenter GetSemanticIcon(AtomUITag tag)
    {
        return GetSemanticIcons(tag).Single();
    }

    private static IconPresenter[] GetSemanticIcons(AtomUITag tag)
    {
        return tag.GetVisualDescendants()
                  .OfType<IconPresenter>()
                  .Where(static icon => icon.Classes.Contains(IconClass))
                  .ToArray();
    }

    private static AtomUITextBlock GetSemanticContent(AtomUITag tag)
    {
        return GetSemanticContents(tag).Single();
    }

    private static AtomUITextBlock[] GetSemanticContents(AtomUITag tag)
    {
        return tag.GetVisualDescendants()
                  .OfType<AtomUITextBlock>()
                  .Where(static content => content.Classes.Contains(ContentClass))
                  .ToArray();
    }

    private static AbstractIconButton GetSemanticClose(AtomUITag tag)
    {
        return GetSemanticCloses(tag).Single();
    }

    private static AbstractIconButton[] GetSemanticCloses(AtomUITag tag)
    {
        return tag.GetVisualDescendants()
                  .OfType<AbstractIconButton>()
                  .Where(static close => close.Classes.Contains(CloseClass))
                  .ToArray();
    }

    private static AtomUICheckableTag[] GetSemanticItems(AtomUICheckableTagGroup group)
    {
        return group.GetVisualDescendants()
                    .OfType<AtomUICheckableTag>()
                    .Where(static item => item.Classes.Contains(ItemClass))
                    .ToArray();
    }

    private static PixelAlignedBorder GetFrame(AtomUITag tag)
    {
        // The root Frame comes before any template-child Frame in depth-first visual order.
        return tag.GetVisualDescendants()
                  .OfType<PixelAlignedBorder>()
                  .First(static frame => frame.Name == "Frame");
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width  = 800,
            Height = 600,
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

    private static void AssertRoot(SemanticPartDescriptor part, Type contractType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertStaticPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string selectorRoute)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertRuntimeItemPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string selectorRoute)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
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

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
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
