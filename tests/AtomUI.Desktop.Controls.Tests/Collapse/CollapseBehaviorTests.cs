using Avalonia.Data;
using Avalonia.Controls.Templates;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using AtomUI.Animations;
using MotionApi = AtomUI.Animations.Motion;
using Shouldly;
using Xunit;
using AtomUICollapse = AtomUI.Desktop.Controls.Collapse;
using AtomUICollapseItem = AtomUI.Desktop.Controls.CollapseItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.CollapseControl;

public class CollapseBehaviorTests
{
    static CollapseBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Item_Padding_Overrides_Update_Template_Padding_Without_Overriding_Item_Local_Values()
    {
        var normalItem = new AtomUICollapseItem
        {
            Header     = "Normal",
            Content    = "Content",
            IsSelected = true
        };
        var explicitItem = new AtomUICollapseItem
        {
            Header         = "Explicit",
            Content        = "Content",
            IsSelected     = true,
            HeaderPadding  = new Thickness(3),
            ContentPadding = new Thickness(4)
        };
        var collapse = new AtomUICollapse
        {
            Items =
            {
                normalItem,
                explicitItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.ItemHeaderPadding  = new Thickness(7);
            collapse.ItemContentPadding = new Thickness(9);
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(normalItem).Padding.ShouldBe(new Thickness(7));
            GetContentFrame(normalItem).Padding.ShouldBe(new Thickness(9));
            GetHeaderDecorator(explicitItem).Padding.ShouldBe(new Thickness(3));
            GetContentFrame(explicitItem).Padding.ShouldBe(new Thickness(4));

            collapse.ItemHeaderPadding  = new Thickness(11);
            collapse.ItemContentPadding = new Thickness(13);
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(normalItem).Padding.ShouldBe(new Thickness(11));
            GetContentFrame(normalItem).Padding.ShouldBe(new Thickness(13));
            GetHeaderDecorator(explicitItem).Padding.ShouldBe(new Thickness(3));
            GetContentFrame(explicitItem).Padding.ShouldBe(new Thickness(4));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Ghost_And_Borderless_Mode_Update_Frame_Border_At_Runtime()
    {
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(5),
            Items =
            {
                new AtomUICollapseItem
                {
                    Header  = "Header",
                    Content = "Content"
                }
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindVisualByName<PixelAlignedBorder>(collapse, "PART_Frame");
            frame.ShouldNotBeNull();
            frame!.BorderThickness.ShouldBe(new Thickness(5));

            collapse.IsGhostStyle = true;
            Dispatcher.UIThread.RunJobs();
            frame.BorderThickness.ShouldBe(new Thickness(0));

            collapse.IsGhostStyle = false;
            collapse.IsBorderless = true;
            Dispatcher.UIThread.RunJobs();
            frame.BorderThickness.ShouldBe(new Thickness(0));

            collapse.IsBorderless = false;
            Dispatcher.UIThread.RunJobs();
            frame.BorderThickness.ShouldBe(new Thickness(5));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Bordered_Items_Keep_Separators_On_Stable_Shells_And_Content_Frames()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var middleItem = new AtomUICollapseItem
        {
            Header     = "Middle",
            Content    = "Content",
            IsSelected = true
        };
        var lastItem = new AtomUICollapseItem
        {
            Header     = "Last",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                middleItem,
                lastItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(middleItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(lastItem).BorderThickness.ShouldBe(default(Thickness));

            GetContentFrame(firstItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            GetContentFrame(middleItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            GetContentFrame(lastItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));

            GetHeaderDecorator(firstItem).BorderThickness.ShouldBe(default(Thickness));
            GetHeaderDecorator(middleItem).BorderThickness.ShouldBe(default(Thickness));
            GetHeaderDecorator(lastItem).BorderThickness.ShouldBe(default(Thickness));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Selection_Does_Not_Change_Item_Separator_Ownership()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var middleItem = new AtomUICollapseItem
        {
            Header     = "Middle",
            Content    = "Content",
            IsSelected = true
        };
        var lastItem = new AtomUICollapseItem
        {
            Header     = "Last",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                middleItem,
                lastItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 320,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            firstItem.IsSelected = true;
            middleItem.IsSelected = false;
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(middleItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(lastItem).BorderThickness.ShouldBe(default(Thickness));

            firstItem.ContentBorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            middleItem.ContentBorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            lastItem.ContentBorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));

            GetHeaderDecorator(firstItem).BorderThickness.ShouldBe(default(Thickness));
            GetHeaderDecorator(middleItem).BorderThickness.ShouldBe(default(Thickness));
            GetHeaderDecorator(lastItem).BorderThickness.ShouldBe(default(Thickness));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Corner_Radius_Follows_Owner_Container_Corners()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var middleItem = new AtomUICollapseItem
        {
            Header     = "Middle",
            Content    = "Content",
            IsSelected = true
        };
        var lastItem = new AtomUICollapseItem
        {
            Header     = "Last",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            CornerRadius   = new CornerRadius(8),
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                middleItem,
                lastItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(firstItem).CornerRadius.ShouldBe(new CornerRadius(8, 8, 0, 0));
            GetHeaderDecorator(middleItem).CornerRadius.ShouldBe(default(CornerRadius));
            GetHeaderDecorator(lastItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));

            GetContentFrame(firstItem).CornerRadius.ShouldBe(default(CornerRadius));
            GetContentFrame(middleItem).CornerRadius.ShouldBe(default(CornerRadius));
            GetContentFrame(lastItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Single_Item_Header_Receives_All_Owner_Corner_Radii()
    {
        var item = new AtomUICollapseItem
        {
            Header     = "Only",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            CornerRadius    = new CornerRadius(8),
            IsMotionEnabled = false,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(item).CornerRadius.ShouldBe(new CornerRadius(8, 8, 8, 8));
            GetContentFrame(item).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Corner_Radius_Updates_When_Owner_CornerRadius_Changes()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header  = "First",
            Content = "Content"
        };
        var lastItem = new AtomUICollapseItem
        {
            Header     = "Last",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            CornerRadius    = new CornerRadius(8),
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                lastItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.CornerRadius = new CornerRadius(12);
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(firstItem).CornerRadius.ShouldBe(new CornerRadius(12, 12, 0, 0));
            GetHeaderDecorator(lastItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 12, 12));
            GetContentFrame(lastItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 12, 12));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Corner_Radius_Redistributes_When_Item_Order_Changes()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header  = "First",
            Content = "Content"
        };
        var secondItem = new AtomUICollapseItem
        {
            Header  = "Second",
            Content = "Content"
        };
        var thirdItem = new AtomUICollapseItem
        {
            Header     = "Third",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            CornerRadius    = new CornerRadius(8),
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                secondItem,
                thirdItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.Items.Insert(0, new AtomUICollapseItem { Header = "New", Content = "Content" });
            Dispatcher.UIThread.RunJobs();

            GetHeaderDecorator(firstItem).CornerRadius.ShouldBe(default(CornerRadius));
            GetHeaderDecorator(secondItem).CornerRadius.ShouldBe(default(CornerRadius));
            GetHeaderDecorator(thirdItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
            GetContentFrame(thirdItem).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Borderless_And_Ghost_Styles_Update_Structural_Separators_At_Runtime()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var lastItem = new AtomUICollapseItem
        {
            Header     = "Last",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            Items =
            {
                firstItem,
                lastItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.IsBorderless = true;
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetContentFrame(firstItem).BorderThickness.ShouldBe(default(Thickness));
            GetContentFrame(lastItem).BorderThickness.ShouldBe(default(Thickness));

            collapse.IsBorderless = false;
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetContentFrame(firstItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            GetContentFrame(lastItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));

            collapse.IsGhostStyle = true;
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(default(Thickness));
            GetItemShell(lastItem).BorderThickness.ShouldBe(default(Thickness));
            GetContentFrame(firstItem).BorderThickness.ShouldBe(default(Thickness));
            GetContentFrame(lastItem).BorderThickness.ShouldBe(default(Thickness));

            collapse.IsGhostStyle = false;
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetContentFrame(firstItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
            GetContentFrame(lastItem).BorderThickness.ShouldBe(new Thickness(0, 1, 0, 0));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Accordion_Mode_Normalizes_Initial_Selected_Items_To_First_Active_Item()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var secondItem = new AtomUICollapseItem
        {
            Header     = "Second",
            Content    = "Content",
            IsSelected = true
        };
        var thirdItem = new AtomUICollapseItem
        {
            Header  = "Third",
            Content = "Content"
        };
        var collapse = new AtomUICollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                secondItem,
                thirdItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 320,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            firstItem.IsSelected.ShouldBeTrue();
            secondItem.IsSelected.ShouldBeFalse();
            thirdItem.IsSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Accordion_Expand_Button_Click_Closes_Previous_Item_And_Opens_Target()
    {
        var firstItem = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var secondItem = new AtomUICollapseItem
        {
            Header  = "Second",
            Content = "Content"
        };
        var collapse = new AtomUICollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items =
            {
                firstItem,
                secondItem
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var expandButton = GetExpandButton(secondItem);
            expandButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, expandButton));
            Dispatcher.UIThread.RunJobs();

            firstItem.IsSelected.ShouldBeFalse();
            secondItem.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Accordion_Clicking_Active_Item_Collapses_To_No_Active_Item()
    {
        var item = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.UpdateSelectionFromEvent(item, new RoutedEventArgs());
            Dispatcher.UIThread.RunJobs();

            item.IsSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Accordion_Mode_Uses_Toggleable_Single_Selection()
    {
        var collapse = new TestCollapse { IsAccordion = true };

        collapse.ExposedSelectionMode.ShouldBe(SelectionMode.Single | SelectionMode.Toggle);
    }

    [Fact]
    public void Accordion_Selection_Model_Replaces_The_Previous_Item()
    {
        var first = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var second = new AtomUICollapseItem { Header = "Second", Content = "Content" };
        var collapse = new TestCollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items = { first, second }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.SelectIndex(0);
            Dispatcher.UIThread.RunJobs();
            collapse.ExposedSelectedIndex.ShouldBe(0);
            collapse.SelectIndex(1);
            Dispatcher.UIThread.RunJobs();
            collapse.ExposedSelectedIndex.ShouldBe(1);

            first.IsSelected.ShouldBeFalse();
            second.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Switching_To_Accordion_Retains_The_First_Selected_Index()
    {
        var first = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var second = new AtomUICollapseItem { Header = "Second", Content = "Content" };
        var collapse = new TestCollapse
        {
            IsMotionEnabled = false,
            Items = { first, second }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.SelectIndex(0);
            collapse.SelectIndex(1);
            Dispatcher.UIThread.RunJobs();
            collapse.IsAccordion = true;
            Dispatcher.UIThread.RunJobs();

            collapse.ExposedSelectedIndex.ShouldBe(0);
            first.IsSelected.ShouldBeTrue();
            second.IsSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Switching_From_Accordion_To_Normal_Allows_Additional_Selection()
    {
        var first = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var second = new AtomUICollapseItem { Header = "Second", Content = "Content" };
        var collapse = new TestCollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items = { first, second }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.SelectIndex(0);
            Dispatcher.UIThread.RunJobs();
            collapse.IsAccordion = false;
            collapse.SelectIndex(1);
            Dispatcher.UIThread.RunJobs();

            first.IsSelected.ShouldBeTrue();
            second.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Normal_Mode_Expand_Buttons_Toggle_Items_Independently()
    {
        var first = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var second = new AtomUICollapseItem { Header = "Second", Content = "Content" };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items = { first, second }
        };

        ShowInWindow(collapse, window =>
        {
            Click(GetExpandButton(first), window);
            Click(GetExpandButton(second), window);

            first.IsSelected.ShouldBeTrue();
            second.IsSelected.ShouldBeTrue();

            Click(GetExpandButton(first), window);

            first.IsSelected.ShouldBeFalse();
            second.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void ItemsSource_Replace_Clears_Selection_And_Replacement_Container_Starts_Collapsed()
    {
        var original = new AtomUICollapseItem { Header = "Original", Content = "Content" };
        var replacement = new AtomUICollapseItem { Header = "Replacement", Content = "Content" };
        var source = new ObservableCollection<AtomUICollapseItem> { original };
        var collapse = new TestCollapse
        {
            IsMotionEnabled = false,
            ItemsSource = source
        };

        ShowInWindow(collapse, window =>
        {
            Click(GetExpandButton(original), window);
            collapse.ExposedSelectedIndex.ShouldBe(0);

            source[0] = replacement;
            Dispatcher.UIThread.RunJobs();

            collapse.ExposedSelectedIndex.ShouldBe(-1);
            replacement.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void ItemsSource_Reset_Clears_Selection_And_Replacement_Containers_Start_Collapsed()
    {
        var original = new AtomUICollapseItem { Header = "Original", Content = "Content" };
        var replacement = new AtomUICollapseItem { Header = "Replacement", Content = "Content" };
        var source = new ResettableObservableCollection<AtomUICollapseItem> { original };
        var collapse = new TestCollapse
        {
            IsMotionEnabled = false,
            ItemsSource = source
        };

        ShowInWindow(collapse, window =>
        {
            Click(GetExpandButton(original), window);
            collapse.ExposedSelectedIndex.ShouldBe(0);

            source.Reset(replacement);
            Dispatcher.UIThread.RunJobs();

            collapse.ExposedSelectedIndex.ShouldBe(-1);
            replacement.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Header_And_Icon_Trigger_Modes_Use_Their_Expected_Pointer_Targets()
    {
        var item = new AtomUICollapseItem { Header = "Header", Content = "Content" };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items = { item }
        };

        ShowInWindow(collapse, window =>
        {
            Click(GetHeaderPresenter(item), window);
            item.IsSelected.ShouldBeTrue();

            Click(GetHeaderPresenter(item), window);
            item.IsSelected.ShouldBeFalse();

            collapse.TriggerType = CollapseTriggerType.Icon;
            Dispatcher.UIThread.RunJobs();

            Click(GetHeaderPresenter(item), window);
            item.IsSelected.ShouldBeFalse();

            Click(GetExpandButton(item), window);
            item.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void Header_Keyboard_Enter_And_Space_Toggle_Selection()
    {
        var item = new AtomUICollapseItem { Header = "Header", Content = "Content" };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items = { item }
        };

        ShowInWindow(collapse, window =>
        {
            item.Focus(NavigationMethod.Tab).ShouldBeTrue();

            PressKey(window, Key.Return, PhysicalKey.Enter);
            item.IsSelected.ShouldBeTrue();

            PressKey(window, Key.Space, PhysicalKey.Space);
            item.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Disabled_Item_Rejects_Pointer_And_Keyboard_Selection()
    {
        var item = new AtomUICollapseItem
        {
            Header = "Header",
            Content = "Content",
            IsEnabled = false
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items = { item }
        };

        ShowInWindow(collapse, window =>
        {
            Click(GetExpandButton(item), window);
            PressKey(window, Key.Enter, PhysicalKey.Enter);

            item.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Disabled_Collapse_Rejects_Pointer_And_Keyboard_Selection()
    {
        var item = new AtomUICollapseItem { Header = "Header", Content = "Content" };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = false,
            Items = { item }
        };

        ShowInWindow(collapse, window =>
        {
            item.Focus(NavigationMethod.Tab).ShouldBeTrue();
            collapse.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();

            Click(GetExpandButton(item), window);
            PressKey(window, Key.Space, PhysicalKey.Space);

            item.IsSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Accordion_Runtime_Selected_Item_Replaces_The_Current_Item()
    {
        var first = new AtomUICollapseItem
        {
            Header     = "First",
            Content    = "Content",
            IsSelected = true
        };
        var collapse = new AtomUICollapse
        {
            IsAccordion     = true,
            IsMotionEnabled = false,
            Items = { first }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var second = new AtomUICollapseItem
            {
                Header     = "Second",
                Content    = "Content",
                IsSelected = true
            };
            collapse.Items.Add(second);
            Dispatcher.UIThread.RunJobs();

            first.IsSelected.ShouldBeFalse();
            second.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Removing_Selected_Item_Clears_The_Selection_Model()
    {
        var item = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var collapse = new TestCollapse
        {
            IsMotionEnabled = false,
            Items = { item }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.SelectIndex(0);
            Dispatcher.UIThread.RunJobs();
            collapse.Items.Remove(item);
            Dispatcher.UIThread.RunJobs();

            collapse.ExposedSelectedIndex.ShouldBe(-1);
            item.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clearing_Items_Clears_The_Selection_Model()
    {
        var collapse = new TestCollapse
        {
            IsMotionEnabled = false,
            Items =
            {
                new AtomUICollapseItem { Header = "First", Content = "Content" },
                new AtomUICollapseItem { Header = "Second", Content = "Content" }
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            collapse.SelectIndex(0);
            Dispatcher.UIThread.RunJobs();
            collapse.Items.Clear();
            Dispatcher.UIThread.RunJobs();

            collapse.ExposedSelectedIndex.ShouldBe(-1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Motion_Reconciles_Final_Visibility_When_Selection_Changes_During_Animation()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(1)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            item.IsSelected = false;
            Dispatcher.UIThread.RunJobs();
            var motionActor = GetContentViewport(item);
            WaitForLayoutCondition(() => !(!MotionApi.GetVisibilityTask(motionActor).IsCompleted) &&
                                         motionActor.IsVisible == item.IsSelected);
            item.IsSelected.ShouldBeFalse();
            motionActor.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Motion_Latest_Selection_Wins_Without_Changing_Structural_Borders()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var motionActor = GetContentViewport(item);
            var initialItemBorder = item.ItemBorderThickness;
            var initialContentBorder = item.ContentBorderThickness;
            var itemBorderChangeCount = 0;
            var contentBorderChangeCount = 0;
            using var itemBorderSubscription = item.GetObservable(AtomUICollapseItem.ItemBorderThicknessProperty)
                                                  .Subscribe(_ => itemBorderChangeCount++);
            using var contentBorderSubscription = item.GetObservable(AtomUICollapseItem.ContentBorderThicknessProperty)
                                                     .Subscribe(_ => contentBorderChangeCount++);

            itemBorderChangeCount = 0;
            contentBorderChangeCount = 0;

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            (!MotionApi.GetVisibilityTask(motionActor).IsCompleted).ShouldBeTrue();

            item.IsSelected = false;
            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            WaitForLayoutCondition(() => motionActor.IsVisible &&
                                         !(!MotionApi.GetVisibilityTask(motionActor).IsCompleted) &&
                                         motionActor.Opacity == 1);

            item.IsSelected.ShouldBeTrue();
            motionActor.IsVisible.ShouldBeTrue();
            (!MotionApi.GetVisibilityTask(motionActor).IsCompleted).ShouldBeFalse();
            motionActor.RenderTransform.ShouldBeNull();
            motionActor.Transitions.ShouldBeNull();
            itemBorderChangeCount.ShouldBe(0);
            contentBorderChangeCount.ShouldBe(0);
            item.ItemBorderThickness.ShouldBe(initialItemBorder);
            item.ContentBorderThickness.ShouldBe(initialContentBorder);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Template_Reapply_Unsubscribes_Old_Expand_Button()
    {
        var item = new TestCollapseItem();
        var oldButton = new IconButton();
        var newButton = new IconButton();

        item.ApplyTemplateParts(oldButton, new ContentViewport());
        item.ApplyTemplateParts(newButton, new ContentViewport());

        oldButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, oldButton));
        item.IsSelected.ShouldBeFalse();

        newButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, newButton));
        item.IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void Item_Template_Reapply_Clears_Old_Content_Motion_Actor_Values()
    {
        var item = new TestCollapseItem
        {
            Header = "First",
            Content = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items = { item }
        };

        ShowInWindow(collapse, _ =>
        {
            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var oldMotionActor = GetContentViewport(item);
            (!MotionApi.GetVisibilityTask(oldMotionActor).IsCompleted).ShouldBeTrue();
            item.ApplyTemplateParts(new IconButton(), new ContentViewport());

            (!MotionApi.GetVisibilityTask(oldMotionActor).IsCompleted).ShouldBeFalse();
            oldMotionActor.IsVisible.ShouldBeTrue();
            oldMotionActor.Opacity.ShouldBe(1);
            MotionApi.GetVisibilityTask(oldMotionActor).IsCompleted.ShouldBeTrue();
            oldMotionActor.RenderTransform.ShouldBeNull();
            oldMotionActor.Transitions.ShouldBeNull();
            oldMotionActor.Height.ShouldBe(double.NaN);
        });
    }

    [Fact]
    public void Item_Detach_Clears_Active_Content_Motion_Values()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var motionActor = GetContentViewport(item);
            (!MotionApi.GetVisibilityTask(motionActor).IsCompleted).ShouldBeTrue();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            (!MotionApi.GetVisibilityTask(motionActor).IsCompleted).ShouldBeFalse();
            motionActor.IsVisible.ShouldBeTrue();
            motionActor.Opacity.ShouldBe(1);
            motionActor.RenderTransform.ShouldBeNull();
            motionActor.RenderTransform.ShouldBeNull();
            motionActor.Transitions.ShouldBeNull();
            motionActor.Height.ShouldBe(double.NaN);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Content_Motion_Animates_Viewport_Without_Overriding_Height_Or_Scaling_Content()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var motionActor = GetContentViewport(item);
            (!MotionApi.GetVisibilityTask(motionActor).IsCompleted).ShouldBeTrue();
            motionActor.RenderTransform.ShouldBeNull();
            motionActor.Height.ShouldBe(double.NaN);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Content_Motion_Completes_With_Natural_Height()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "Content",
            MotionDuration = TimeSpan.FromMilliseconds(1)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            var motionActor = GetContentViewport(item);
            WaitForLayoutCondition(() => !(!MotionApi.GetVisibilityTask(motionActor).IsCompleted) &&
                                         motionActor.IsVisible == item.IsSelected);
            motionActor.Opacity.ShouldBe(1);
            motionActor.Height.ShouldBe(double.NaN);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Content_Motion_Starts_Growing_For_Control_Content()
    {
        var wrappedContent = new TextBlock
        {
            Text         = "Long wrapped collapse content that needs multiple lines when measured with the item width.",
            TextWrapping = TextWrapping.Wrap
        };
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = wrappedContent,
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 180,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var motionActor = GetContentViewport(item);
            WaitForLayoutCondition(() => motionActor.Bounds.Height > 0.0);
            motionActor.Bounds.Height.ShouldBeGreaterThan(0.0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Content_Motion_Starts_Growing_For_String_Content()
    {
        var item = new AtomUICollapseItem
        {
            Header         = "First",
            Content        = "String content",
            MotionDuration = TimeSpan.FromMilliseconds(100)
        };
        var collapse = new AtomUICollapse
        {
            IsMotionEnabled = true,
            Items =
            {
                item
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 180,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            item.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            var motionActor = GetContentViewport(item);
            WaitForLayoutCondition(() => motionActor.Bounds.Height > 0.0);
            motionActor.Bounds.Height.ShouldBeGreaterThan(0.0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Shell_Separators_Follow_Item_Collection_Structure()
    {
        var firstItem = new AtomUICollapseItem { Header = "First", Content = "Content" };
        var lastItem = new AtomUICollapseItem { Header = "Last", Content = "Content" };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            IsMotionEnabled = false,
            Items = { firstItem, lastItem }
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(lastItem).BorderThickness.ShouldBe(default(Thickness));

            collapse.Items.Remove(lastItem);
            Dispatcher.UIThread.RunJobs();
            GetItemShell(firstItem).BorderThickness.ShouldBe(default(Thickness));

            collapse.Items.Add(lastItem);
            Dispatcher.UIThread.RunJobs();
            GetItemShell(firstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(lastItem).BorderThickness.ShouldBe(default(Thickness));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Item_Shell_Separators_Refresh_After_ItemsSource_Reset()
    {
        var initialFirstItem = new AtomUICollapseItem { Header = "Initial first", Content = "Content" };
        var initialLastItem = new AtomUICollapseItem { Header = "Initial last", Content = "Content" };
        var resetFirstItem = new AtomUICollapseItem { Header = "Reset first", Content = "Content" };
        var resetLastItem = new AtomUICollapseItem { Header = "Reset last", Content = "Content" };
        var source = new ResettableObservableCollection<AtomUICollapseItem>
        {
            initialFirstItem,
            initialLastItem
        };
        var collapse = new AtomUICollapse
        {
            BorderThickness = new Thickness(1),
            IsMotionEnabled = false,
            ItemsSource = source
        };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = collapse
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetItemShell(initialFirstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(initialLastItem).BorderThickness.ShouldBe(default(Thickness));

            source.Reset(resetFirstItem, resetLastItem);
            Dispatcher.UIThread.RunJobs();

            GetItemShell(resetFirstItem).BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            GetItemShell(resetLastItem).BorderThickness.ShouldBe(default(Thickness));
        }
        finally
        {
            window.Close();
        }
    }

    private static PixelAlignedBorder GetItemShell(AtomUICollapseItem item)
    {
        var shell = item.GetVisualChildren().OfType<PixelAlignedBorder>().SingleOrDefault();
        shell.ShouldNotBeNull();
        return shell!;
    }

    private sealed class ResettableObservableCollection<T> : ObservableCollection<T>
    {
        public void Reset(params T[] items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private static PixelAlignedBorder GetHeaderDecorator(AtomUICollapseItem item)
    {
        var header = FindVisualByName<PixelAlignedBorder>(item, "PART_HeaderDecorator");
        header.ShouldNotBeNull();
        return header!;
    }

    private static PixelAlignedBorder GetContentFrame(AtomUICollapseItem item)
    {
        var frame = FindVisualByName<PixelAlignedBorder>(item, "PART_ContentFrame");
        frame.ShouldNotBeNull();
        return frame!;
    }

    private static IconButton GetExpandButton(AtomUICollapseItem item)
    {
        var button = FindTemplatePart<IconButton>(item, "PART_ExpandButton");
        button.ShouldNotBeNull();
        return button!;
    }

    private static ContentPresenter GetHeaderPresenter(AtomUICollapseItem item)
    {
        var presenter = FindTemplatePart<ContentPresenter>(item, "PART_HeaderPresenter");
        presenter.ShouldNotBeNull();
        return presenter!;
    }

    private static ContentViewport GetContentViewport(AtomUICollapseItem item)
    {
        var motionActor = FindTemplatePart<ContentViewport>(item, "PART_ContentViewport");
        motionActor.ShouldNotBeNull();
        return motionActor!;
    }

    private static void WaitForLayoutCondition(Func<bool> condition, int timeoutMilliseconds = 500)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            Thread.Sleep(10);
        }

        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        condition().ShouldBeTrue("The expected layout or animation state did not settle before the timeout.");
    }

    private static void Click(Control control, AvaloniaWindow window)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();

        window.MouseMove(point.Value);
        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void PressKey(AvaloniaWindow window, Key key, PhysicalKey physicalKey)
    {
        window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 360,
            Height = 260,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static T? FindTemplatePart<T>(AtomUICollapseItem item, string name)
        where T : Control
    {
        return item.GetSelfAndVisualDescendants()
                   .OfType<T>()
                   .FirstOrDefault(control => control.Name == name &&
                                              ReferenceEquals(control.TemplatedParent, item));
    }

    private static T? FindVisualByName<T>(Control root, string name)
        where T : Control
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<T>()
                   .FirstOrDefault(control => control.Name == name);
    }

    private sealed class TestCollapse : AtomUICollapse
    {
        protected override Type StyleKeyOverride => typeof(AtomUICollapse);

        public SelectionMode ExposedSelectionMode => SelectionMode;

        public int ExposedSelectedIndex => Selection.SelectedIndex;

        public void SelectIndex(int index)
        {
            var container = Items[index] as Control;
            container.ShouldNotBeNull();
            UpdateSelectionFromEvent(container!, new RoutedEventArgs());
        }
    }

    private sealed class TestCollapseItem : AtomUICollapseItem
    {
        protected override Type StyleKeyOverride => typeof(AtomUICollapseItem);

        public void ApplyTemplateParts(IconButton expandButton, ContentViewport motionActor)
        {
            Template = new FuncControlTemplate((owner, scope) =>
            {
                scope.Register("PART_ExpandButton", expandButton);
                scope.Register("PART_ContentViewport", motionActor);
                MotionApi.SetEnter(motionActor, new ExpandMotion());
                MotionApi.SetLeave(motionActor, new CollapseMotion());
                motionActor.Bind(MotionApi.IsVisibleProperty, new TemplateBinding(IsSelectedProperty));
                motionActor.Bind(MotionApi.IsEnabledProperty, new TemplateBinding(IsMotionEnabledProperty));
                motionActor.Bind(MotionApi.DurationOverrideProperty, new TemplateBinding(MotionDurationProperty));
                return new StackPanel { Children = { expandButton, motionActor } };
            });
            ApplyTemplate();
        }
    }
}
