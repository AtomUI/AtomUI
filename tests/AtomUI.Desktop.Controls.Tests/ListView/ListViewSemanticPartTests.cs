using System.Collections.ObjectModel;
using System.Reflection;
using AtomUI.Controls.Data;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using Shouldly;
using Xunit;
using AtomListView = AtomUI.Desktop.Controls.ListView;
using AtomListViewItem = AtomUI.Desktop.Controls.ListViewItem;
using AtomPagination = AtomUI.Desktop.Controls.Pagination;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListView;

public class ListViewSemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string GroupHeaderClass = "semantic-group-header";
    private const string ItemRoute = "> .semantic-item";
    private const string GroupHeaderRoute = "> .semantic-group-header";

    static ListViewSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomListView), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "groupHeader", "item"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomListViewItem),
            ItemRoute);
        AssertPart(descriptor.Parts.Single(static part => part.Name == "groupHeader"),
            GroupHeaderClass,
            typeof(AtomListViewItem),
            GroupHeaderRoute);
    }

    [Fact]
    public void Grouped_Items_Produce_Item_And_Group_Header_Containers()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia", "Design"),
                CreateItem("Liam", "Design"),
                CreateItem("Emma", "Engineering")
            }
        };

        ShowInWindow(listView, () =>
        {
            GetSemanticItems(listView).Length.ShouldBe(3);
            GetSemanticGroupHeaders(listView).Length.ShouldBe(2);
        });
    }

    [Fact]
    public void Group_And_Item_Containers_Use_Distinct_Recycle_Keys()
    {
        var listView = new AtomListView();

        var groupKey = ResolveNeedsContainer(listView, CreateGroupHeader("Design"));
        var itemKey = ResolveNeedsContainer(listView, CreateItem("Olivia"));

        groupKey.NeedsContainer.ShouldBeTrue();
        itemKey.NeedsContainer.ShouldBeTrue();
        groupKey.RecycleKey.ShouldNotBeNull();
        itemKey.RecycleKey.ShouldNotBeNull();
        groupKey.RecycleKey.ShouldNotBe(itemKey.RecycleKey,
            "group header containers must never be recycled as item containers.");
    }

    [Fact]
    public void Prepare_And_Clear_Cycles_Do_Not_Toggle_Markers()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia", "Design"),
                CreateItem("Liam", "Engineering")
            }
        };

        ShowInWindow(listView, () =>
        {
            var item = GetSemanticItems(listView)[0];
            var groupHeader = GetSemanticGroupHeaders(listView)[0];

            var itemIndex = listView.IndexFromContainer(item);
            var groupIndex = listView.IndexFromContainer(groupHeader);
            var itemData = listView.Items[itemIndex];
            var groupData = listView.Items[groupIndex];

            ClearContainerForItem(listView, item);
            PrepareContainerForItem(listView, item, itemData!, itemIndex);
            ClearContainerForItem(listView, groupHeader);
            PrepareContainerForItem(listView, groupHeader, groupData!, groupIndex);

            item.Classes.ShouldContain(ItemClass);
            item.Classes.ShouldNotContain(GroupHeaderClass);
            groupHeader.Classes.ShouldContain(GroupHeaderClass);
            groupHeader.Classes.ShouldNotContain(ItemClass);
        });
    }

    [Fact]
    public void Divider_Uses_Current_View_Position_For_Repeated_Items()
    {
        var repeated = new ListItemData { Content = "same" };
        var listView = new AtomListView
        {
            ItemsSource = new[] { repeated, new ListItemData { Content = "other" }, repeated }
        };

        ShowInWindow(listView, () =>
        {
            Enumerable.Range(0, 3)
                .Select(index => GetSplitLineFrame(GetSemanticItems(listView)[index]).IsVisible)
                .ShouldBe([true, true, false]);
        });
    }

    [Fact]
    public void Root_Semantic_Part_Projects_The_Standard_Surface_Properties()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[] { CreateItem("Olivia") }
        };
        listView.Background = new SolidColorBrush(Colors.AliceBlue);
        listView.BorderBrush = new SolidColorBrush(Color.Parse("#CDC1FF"));
        listView.BorderThickness = new Thickness(1);
        listView.CornerRadius = new CornerRadius(8);
        listView.Padding = new Thickness(10);

        ShowInWindow(listView, () =>
        {
            var frame = GetFrame(listView);

            frame.Background.ShouldBeSameAs(listView.Background);
            frame.BorderBrush.ShouldBeSameAs(listView.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(1));
            frame.CornerRadius.ShouldBe(new CornerRadius(8));
            frame.Padding.ShouldBe(new Thickness(10));
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia", "Design")
            }
        };
        listView.Classes.Add("semantic-owner");
        listView.Styles.Add(CreateRouteStyle(ItemClass, "item"));
        listView.Styles.Add(CreateRouteStyle(GroupHeaderClass, "group"));

        ShowInWindow(listView, () =>
        {
            GetSemanticItems(listView).ShouldAllBe(static item => Equals(item.Tag, "item"));
            GetSemanticGroupHeaders(listView).ShouldAllBe(static header => Equals(header.Tag, "group"));
        });
    }

    [Fact]
    public void Item_Containers_Get_A_Bottom_Divider_And_Group_Headers_Do_Not()
    {
        var listView = new AtomListView
        {
            IsGroupEnabled = true,
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia", "Design"),
                CreateItem("Liam", "Engineering")
            }
        };

        ShowInWindow(listView, () =>
        {
            var items = GetSemanticItems(listView);
            var itemSplitLines = items.Select(GetSplitLineFrame).ToArray();
            var groupSplitLines = GetSemanticGroupHeaders(listView).Select(GetSplitLineFrame).ToArray();

            itemSplitLines.Length.ShouldBe(2);
            groupSplitLines.Length.ShouldBe(2);
            // Every item keeps its divider except the last one, which is closed by the root border.
            itemSplitLines[0].IsVisible.ShouldBeTrue();
            itemSplitLines[0].BorderThickness.Bottom.ShouldBe(1);
            itemSplitLines[1].IsVisible.ShouldBeFalse();
            groupSplitLines.ShouldAllBe(static line => !line.IsVisible);
            var firstItemBackground = GetItemFrame(items[0]).Background;
            GetSemanticGroupHeaders(listView).Select(GetItemFrame)
                .ShouldAllBe(frame => !Equals(frame.Background, firstItemBackground));
        });
    }

    [Fact]
    public void Semantic_Item_Border_Style_Overrides_The_Default_Divider()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[] { CreateItem("Olivia"), CreateItem("Liam") }
        };
        listView.Classes.Add("semantic-owner");
        listView.Styles.Add(new Style(selector =>
            selector.OfType<AtomListView>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(2)) }
        });

        ShowInWindow(listView, () =>
        {
            var items = GetSemanticItems(listView);
            var styledSplitLine = GetSplitLineFrame(items[0]);
            styledSplitLine.IsVisible.ShouldBeTrue();
            styledSplitLine.BorderThickness.ShouldBe(new Thickness(2));
            GetSplitLineFrame(items[1]).IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Only_The_Last_Item_Suppresses_The_Divider_Without_Pagination()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia"),
                CreateItem("Liam"),
                CreateItem("Emma")
            }
        };

        ShowInWindow(listView, () =>
        {
            var splitLines = GetSemanticItems(listView).Select(GetSplitLineFrame).ToArray();
            splitLines[0].IsVisible.ShouldBeTrue();
            splitLines[1].IsVisible.ShouldBeTrue();
            splitLines[2].IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Last_Item_Keeps_The_Divider_When_Bottom_Pagination_Is_Present()
    {
        var listView = new AtomListView
        {
            ItemsSource      = new ListItemData[] { CreateItem("Olivia"), CreateItem("Liam") },
            BottomPagination = new AtomPagination()
        };

        ShowInWindow(listView, () =>
        {
            GetSemanticItems(listView).Select(GetSplitLineFrame)
                .ShouldAllBe(static line => line.IsVisible);
        });
    }

    [Fact]
    public void Borderless_Keeps_The_Divider_On_The_Last_Item()
    {
        var listView = new AtomListView
        {
            IsBorderless = true,
            ItemsSource  = new ListItemData[] { CreateItem("Olivia"), CreateItem("Liam") }
        };

        ShowInWindow(listView, () =>
        {
            GetSemanticItems(listView).Select(GetSplitLineFrame)
                .ShouldAllBe(static line => line.IsVisible);
        });
    }

    [Fact]
    public void Appending_An_Item_Restores_The_Previously_Last_Items_Divider()
    {
        var items = new ObservableCollection<ListItemData>
        {
            CreateItem("Olivia"),
            CreateItem("Liam")
        };
        var listView = new AtomListView
        {
            ItemsSource = items
        };

        ShowInWindow(listView, () =>
        {
            GetSplitLineFrame(GetSemanticItems(listView)[1]).IsVisible.ShouldBeFalse();

            items.Add(CreateItem("Emma"));
            Dispatcher.UIThread.RunJobs();

            var splitLines = GetSemanticItems(listView).Select(GetSplitLineFrame).ToArray();
            splitLines[0].IsVisible.ShouldBeTrue();
            splitLines[1].IsVisible.ShouldBeTrue();
            splitLines[2].IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Root_Frame_Declares_Content_Clipping_To_The_Inner_Border_Edge()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[] { CreateItem("Olivia") }
        };

        ShowInWindow(listView, () =>
        {
            var frame = GetFrame(listView);
            frame.ClipContentToCornerRadius.ShouldBeTrue();
            // The clip figure is computed from the frame's own border thickness and
            // corner radius, so the inner edge of the ring is the clipping boundary.
            frame.Child.ShouldNotBeNull();
        });
    }

    [Fact]
    public void Select_And_Transfer_Items_Do_Not_Inherit_The_List_Divider()
    {
        var selectList = new SelectCandidateList
        {
            ItemsSource = new ISelectOption[]
            {
                new SelectOption { Header = "Time", Content = "Time" },
                new SelectOption { Header = "Date", Content = "Date" }
            }
        };
        var transferItem = new TransferListItem();

        ShowInWindow(new StackPanel { Children = { selectList, transferItem } }, () =>
        {
            var selectItems = selectList.GetVisualDescendants().OfType<SelectCandidateListItem>().ToArray();
            selectItems.Length.ShouldBe(2);
            foreach (var selectItem in selectItems)
            {
                var selectSplitLine = GetSplitLineFrame(selectItem);
                selectSplitLine.IsVisible.ShouldBeFalse();
                selectSplitLine.BorderThickness.ShouldBe(new Thickness(0));
                selectItem.EffectiveBorderThickness.ShouldBe(new Thickness(0));
                selectItem.IsSplitLineEffectiveVisible.ShouldBeFalse();
            }

            transferItem.GetVisualDescendants()
                        .OfType<PixelAlignedBorder>()
                        .ShouldNotContain(static border => border.Name == "SplitLineFrame");

            transferItem.EffectiveBorderThickness.ShouldBe(new Thickness(0));
            transferItem.IsSplitLineEffectiveVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Root_Border_Uses_The_Item_Divider_Color()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[] { CreateItem("Olivia") }
        };

        ShowInWindow(listView, () =>
        {
            var rootBorderColor = ResolveColor(GetFrame(listView).BorderBrush);
            var itemBorderColor = ResolveColor(GetSemanticItems(listView).Single().BorderBrush);

            rootBorderColor.ShouldBe(itemBorderColor);
        });
    }

    [Fact]
    public void Item_Surfaces_Are_Square_Across_Size_Types()
    {
        foreach (var sizeType in new[]
                 {
                     CustomizableSizeType.Large,
                     CustomizableSizeType.Middle,
                     CustomizableSizeType.Small
                 })
        {
            var listView = new AtomListView
            {
                SizeType    = sizeType,
                ItemsSource = new ListItemData[] { CreateItem("Olivia") }
            };

            ShowInWindow(listView, () =>
            {
                GetItemFrame(GetSemanticItems(listView).Single())
                    .CornerRadius.ShouldBe(new CornerRadius(0));
            });
        }
    }

    [Fact]
    public void Items_Touch_The_Outer_Border_Without_Content_Padding()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[] { CreateItem("Olivia") }
        };

        ShowInWindow(listView, () =>
        {
            listView.Padding.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Items_Sit_Flush_Without_Vertical_Margins()
    {
        var listView = new AtomListView
        {
            ItemsSource = new ListItemData[]
            {
                CreateItem("Olivia"),
                CreateItem("Liam")
            }
        };

        ShowInWindow(listView, () =>
        {
            GetSemanticItems(listView).ShouldAllBe(static item => item.Margin == new Thickness(0));
        });
    }

    private static ListItemData CreateItem(string content, string? group = null)
    {
        return new ListItemData { Content = content, Group = group };
    }

    private static GroupListItemData CreateGroupHeader(string content)
    {
        return new GroupListItemData { Content = content, IsGroupItem = true };
    }

    private static Style CreateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector.OfType<AtomListView>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static AtomListViewItem[] GetSemanticItems(AtomListView listView)
    {
        return listView.GetVisualDescendants()
                       .OfType<AtomListViewItem>()
                       .Where(static item => item.Classes.Contains(ItemClass))
                       .ToArray();
    }

    private static AtomListViewItem[] GetSemanticGroupHeaders(AtomListView listView)
    {
        return listView.GetVisualDescendants()
                       .OfType<AtomListViewItem>()
                       .Where(static item => item.Classes.Contains(GroupHeaderClass))
                       .ToArray();
    }

    private static PixelAlignedBorder GetItemFrame(AtomListViewItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(static border => border.Name == "Frame");
    }

    private static PixelAlignedBorder GetSplitLineFrame(AtomListViewItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(static border => border.Name == "SplitLineFrame");
    }

    private static PixelAlignedBorder GetFrame(AtomListView listView)
    {
        return listView.GetVisualDescendants()
                       .OfType<PixelAlignedBorder>()
                       .First(static border => border.Name == "Frame");
    }

    private static Color ResolveColor(IBrush? brush)
    {
        var immutable = brush.ShouldNotBeNull().ToImmutable();
        return immutable.ShouldBeAssignableTo<IImmutableSolidColorBrush>().Color;
    }

    private static void AssertRectClose(Rect actual, Rect expected)
    {
        const double tolerance = 0.01;
        Math.Abs(actual.X - expected.X).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Y - expected.Y).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Width - expected.Width).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Height - expected.Height).ShouldBeLessThan(tolerance);
    }

    private static (bool NeedsContainer, object? RecycleKey) ResolveNeedsContainer(
        AtomListView listView,
        object? item)
    {
        var method = typeof(AtomListView).GetMethod(
            "NeedsContainerOverride",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        var args = new object?[] { item, 0, null };
        var needsContainer = (bool)method.Invoke(listView, args)!;
        return (needsContainer, args[2]);
    }

    private static void PrepareContainerForItem(
        AtomListView listView,
        AtomListViewItem container,
        object item,
        int index)
    {
        var method = typeof(AtomListView).GetMethod(
            "PrepareContainerForItemOverride",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(listView, [container, item, index]);
    }

    private static void ClearContainerForItem(
        AtomListView listView,
        AtomListViewItem container)
    {
        var method = typeof(AtomListView).GetMethod(
            "ClearContainerForItemOverride",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(listView, [container]);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
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
        part.ContractType.ShouldBe(typeof(AtomListView));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    private static void AssertPart(
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
}
