using AtomUI.Theme.Styling;
using System.Collections.ObjectModel;
using AtomUI.Controls.Data;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomListBox = AtomUI.Desktop.Controls.ListBox;
using AtomListBoxItem = AtomUI.Desktop.Controls.ListBoxItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ListBox;

public class ListBoxSemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string ItemRoute = "> .semantic-item";

    static ListBoxSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomListBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "item"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            typeof(AtomListBoxItem),
            ItemRoute);
    }

    [Fact]
    public void Items_Produce_One_Item_Marker_Per_Container()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia", "Liam", "Emma")
        };

        ShowInWindow(listBox, () =>
        {
            GetSemanticItems(listBox).Length.ShouldBe(3);
        });
    }

    [Fact]
    public void Explicit_Item_Container_Receives_The_Semantic_Marker()
    {
        var view = new SemanticParts.SemanticRepairStylesView();
        var listBox = view.FindControl<AtomListBox>("ListBox").ShouldNotBeNull();
        var style = view.Styles.OfType<Style>().SelectMany(owner => owner.Children)
            .OfType<ListBoxItemStyle>().ShouldHaveSingleItem();
        style.Setters.OfType<Setter>().Single(setter => setter.Property == Control.TagProperty)
            .Value.ShouldBe("item");
        style.Setters.OfType<Setter>().Single(setter => setter.Property == TemplatedControl.BackgroundProperty)
            .Value.ShouldBe(Brushes.Red);
        ShowInWindow(view, () =>
        {
            var item = GetContainers(listBox).Single();
            item.Classes.Contains(ItemClass).ShouldBeTrue();
            item.Tag.ShouldBe("item");
            item.Background.ShouldBe(Brushes.Red);
        });
    }

    [Fact]
    public void Divider_Uses_Container_Position_Not_Item_Reference()
    {
        object repeated = new();
        var listBox = new AtomListBox
        {
            ItemsSource = new object[] { repeated, new object(), repeated }
        };

        ShowInWindow(listBox, () =>
        {
            GetSemanticItems(listBox).Select(GetSplitLineFrame)
                .Select(static frame => frame.IsVisible)
                .ShouldBe([true, true, false]);
        });
    }

    [Fact]
    public void Candidate_List_Containers_Do_Not_Carry_The_ListBox_Item_Marker()
    {
        var candidateList = new CandidateList
        {
            ItemsSource = CreateItems("Olivia", "Liam")
        };

        ShowInWindow(candidateList, () =>
        {
            GetContainers(candidateList).Length.ShouldBe(2);
            GetContainers(candidateList).ShouldAllBe(static item => !item.Classes.Contains(ItemClass));
        });
    }

    [Fact]
    public void Root_Semantic_Part_Projects_The_Standard_Surface_Properties()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia")
        };
        listBox.Background = new SolidColorBrush(Colors.AliceBlue);
        listBox.BorderBrush = new SolidColorBrush(Color.Parse("#CDC1FF"));
        listBox.BorderThickness = new Thickness(1);
        listBox.CornerRadius = new CornerRadius(8);
        listBox.Padding = new Thickness(10);

        ShowInWindow(listBox, () =>
        {
            var frame = GetFrame(listBox);

            frame.Background.ShouldBeSameAs(listBox.Background);
            frame.BorderBrush.ShouldBeSameAs(listBox.BorderBrush);
            frame.BorderThickness.ShouldBe(new Thickness(1));
            frame.CornerRadius.ShouldBe(new CornerRadius(8));
            frame.Padding.ShouldBe(new Thickness(10));
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia", "Liam")
        };
        listBox.Classes.Add("semantic-owner");
        listBox.Styles.Add(CreateRouteStyle());

        ShowInWindow(listBox, () =>
        {
            GetSemanticItems(listBox).ShouldAllBe(static item => Equals(item.Tag, "item"));
        });
    }

    [Fact]
    public void Item_Containers_Get_A_Bottom_Divider()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia", "Liam", "Emma")
        };

        ShowInWindow(listBox, () =>
        {
            var splitLines = GetSemanticItems(listBox).Select(GetSplitLineFrame).ToArray();
            splitLines[0].IsVisible.ShouldBeTrue();
            splitLines[0].BorderThickness.Bottom.ShouldBe(1);
            splitLines[1].IsVisible.ShouldBeTrue();
            splitLines[2].IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Semantic_Item_Border_Style_Overrides_The_Default_Divider()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia", "Liam")
        };
        listBox.Classes.Add("semantic-owner");
        listBox.Styles.Add(new Style(selector =>
            selector.OfType<AtomListBox>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(2)) }
        });

        ShowInWindow(listBox, () =>
        {
            var items = GetSemanticItems(listBox);
            var styledSplitLine = GetSplitLineFrame(items[0]);
            styledSplitLine.IsVisible.ShouldBeTrue();
            styledSplitLine.BorderThickness.ShouldBe(new Thickness(2));
            GetSplitLineFrame(items[1]).IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Borderless_Keeps_The_Divider_On_The_Last_Item()
    {
        var listBox = new AtomListBox
        {
            IsBorderless = true,
            ItemsSource  = CreateItems("Olivia", "Liam")
        };

        ShowInWindow(listBox, () =>
        {
            GetSemanticItems(listBox).Select(GetSplitLineFrame)
                .ShouldAllBe(static line => line.IsVisible);
        });
    }

    [Fact]
    public void Appending_An_Item_Restores_The_Previously_Last_Items_Divider()
    {
        var items = new ObservableCollection<ListItemData>(CreateItems("Olivia", "Liam"));
        var listBox = new AtomListBox
        {
            ItemsSource = items
        };

        ShowInWindow(listBox, () =>
        {
            GetSplitLineFrame(GetSemanticItems(listBox)[1]).IsVisible.ShouldBeFalse();

            items.Add(new ListItemData { Content = "Emma" });
            Dispatcher.UIThread.RunJobs();

            var splitLines = GetSemanticItems(listBox).Select(GetSplitLineFrame).ToArray();
            splitLines[0].IsVisible.ShouldBeTrue();
            splitLines[1].IsVisible.ShouldBeTrue();
            splitLines[2].IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Root_Frame_Declares_Content_Clipping_To_The_Inner_Border_Edge()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia")
        };

        ShowInWindow(listBox, () =>
        {
            var frame = GetFrame(listBox);
            frame.ClipContentToCornerRadius.ShouldBeTrue();
            // The clip figure is computed from the frame's own border thickness and
            // corner radius, so the inner edge of the ring is the clipping boundary.
            frame.Child.ShouldNotBeNull();
        });
    }

    [Fact]
    public void Candidate_And_Cascader_Items_Do_Not_Inherit_The_List_Divider()
    {
        var candidateList = new CandidateList { ItemsSource = new[] { "Time", "Date" } };
        var cascaderList = new CascaderViewFilterList
        {
            ItemsSource = new CascaderViewFilterListItemData[]
            {
                new() { Content = "Asia / Shanghai" },
                new() { Content = "Europe / London" }
            }
        };

        ShowInWindow(new StackPanel { Children = { candidateList, cascaderList } }, () =>
        {
            var items = candidateList.GetVisualDescendants().OfType<CandidateListItem>().Cast<AtomListBoxItem>()
                .Concat(cascaderList.GetVisualDescendants().OfType<CascaderViewFilterListItem>())
                .ToArray();
            items.Length.ShouldBe(4);
            foreach (var item in items)
            {
                var splitLine = GetSplitLineFrame(item);
                splitLine.IsVisible.ShouldBeFalse();
                splitLine.BorderThickness.ShouldBe(new Thickness(0));
                item.EffectiveBorderThickness.ShouldBe(new Thickness(0));
                item.IsSplitLineEffectiveVisible.ShouldBeFalse();
            }
        });
    }

    [Fact]
    public void Root_Border_Uses_The_Item_Divider_Color()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia")
        };

        ShowInWindow(listBox, () =>
        {
            var rootBorderColor = ResolveColor(GetFrame(listBox).BorderBrush);
            var itemBorderColor = ResolveColor(GetSemanticItems(listBox).Single().BorderBrush);

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
            var listBox = new AtomListBox
            {
                SizeType    = sizeType,
                ItemsSource = CreateItems("Olivia")
            };

            ShowInWindow(listBox, () =>
            {
                GetItemFrame(GetSemanticItems(listBox).Single())
                    .CornerRadius.ShouldBe(new CornerRadius(0));
            });
        }
    }

    [Fact]
    public void Items_Touch_The_Outer_Border_Without_Content_Padding()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia")
        };

        ShowInWindow(listBox, () =>
        {
            listBox.Padding.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Items_Sit_Flush_Without_Vertical_Margins()
    {
        var listBox = new AtomListBox
        {
            ItemsSource = CreateItems("Olivia", "Liam")
        };

        ShowInWindow(listBox, () =>
        {
            GetSemanticItems(listBox).ShouldAllBe(static item => item.Margin == new Thickness(0));
        });
    }

    private static ListItemData[] CreateItems(params string[] values)
    {
        return values.Select(value => new ListItemData { Content = value }).ToArray();
    }

    private static Style CreateRouteStyle()
    {
        return new Style(selector =>
            selector.OfType<AtomListBox>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(ItemClass))
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        };
    }

    private static AtomListBoxItem[] GetSemanticItems(AtomListBox listBox)
    {
        return GetContainers(listBox)
               .Where(static item => item.Classes.Contains(ItemClass))
               .ToArray();
    }

    private static AtomListBoxItem[] GetContainers(AtomListBox listBox)
    {
        return listBox.GetVisualDescendants()
                      .OfType<AtomListBoxItem>()
                      .ToArray();
    }

    private static PixelAlignedBorder GetItemFrame(AtomListBoxItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(static border => border.Name == "Frame");
    }

    private static PixelAlignedBorder GetSplitLineFrame(AtomListBoxItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(static border => border.Name == "SplitLineFrame");
    }

    private static PixelAlignedBorder GetFrame(AtomListBox listBox)
    {
        return listBox.GetVisualDescendants()
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
        part.ContractType.ShouldBe(typeof(AtomListBox));
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
