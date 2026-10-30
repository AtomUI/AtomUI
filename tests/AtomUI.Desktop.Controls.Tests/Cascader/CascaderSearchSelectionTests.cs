using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Cascader;

public class CascaderSearchSelectionTests
{
    static CascaderSearchSelectionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Selecting_Filter_Result_Expands_And_Selects_Matched_Path()
    {
        var (options, lingyin) = CreateProvinceOptions();
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "lin";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the filter result list should be visible after setting a filter value.");
            filterList.ItemCount.ShouldBe(1);

            filterList.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should remain visible after selecting a filter result.");
            var hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "selecting a filter result should expand the matched parent path.");
            var lingyinItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Lingyin shi"),
                "selecting a filter result should realize the matched leaf item.");

            cascaderView.SelectedOption.ShouldBeSameAs(lingyin);
            zhejiangItem.IsExpanded.ShouldBeTrue();
            hangzhouItem.IsExpanded.ShouldBeTrue();
            lingyinItem.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Selecting_Filter_Result_In_Cascader_Popup_Updates_Selection_Without_Keeping_Filter_Mode()
    {
        var (options, lingyin) = CreateProvinceOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            cascader.FilterValue = "lin";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the popup filter result list should be visible after setting a filter value.");
            filterList.ItemCount.ShouldBe(1);

            filterList.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(lingyin);
            cascaderView.IsFiltering.ShouldBeFalse();
            cascaderView.FilterValue.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Disabled_Leaf_Filter_Result_Does_Not_Commit_Selection()
    {
        var disabledLeaf = new CascaderOption
        {
            Header     = "Xisha",
            Value      = "xisha",
            IsEnabled  = false
        };
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource = new[]
            {
                new CascaderOption
                {
                    Header = "Zhejiang",
                    Children =
                    [
                        new CascaderOption
                        {
                            Header   = "Hangzhou",
                            Children = [disabledLeaf]
                        }
                    ]
                }
            }
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "xisha";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the disabled leaf should remain visible in filtered results.");
            filterList.ItemCount.ShouldBe(1);

            filterList.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            cascaderView.SelectedOption.ShouldBeNull();
            cascaderView.IsFiltering.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Closing_Cascader_Removes_Filter_Candidate_State_From_The_Old_Container()
    {
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource = new[]
            {
                new CascaderOption
                {
                    Header = "Zhejiang",
                    Children =
                    [
                        new CascaderOption
                        {
                            Header = "West Lake",
                            Value  = "west-lake"
                        }
                    ]
                }
            }
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            cascader.FilterValue = "west";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the leaf should be visible in filtered results.");
            var resultItem = WaitFor(
                () => filterList.ContainerFromIndex(0) as CascaderViewFilterListItem,
                "the filter result container should be realized.");

            cascaderView.TryMoveFilterCandidate(1).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            resultItem.IsCandidateSelected.ShouldBeTrue();

            cascader.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            resultItem.IsCandidateSelected.ShouldBeFalse();
            filterList.CandidateSelectedIndex.ShouldBe(-1);
            filterList.CandidateSelectedItem.ShouldBeNull();
            cascaderView.IsFiltering.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Closing_Incomplete_Single_Select_Path_Discards_Draft_Before_Reopen()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var zhejiang = options[0];
        var hangzhou = zhejiang.Children.Single();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should be visible when the popup opens.");
            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();

            var hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "expanding Zhejiang should reveal Hangzhou.");
            RaisePointerPressed(hangzhouItem);
            Dispatcher.UIThread.RunJobs();

            WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "expanding Hangzhou should reveal its leaves.");
            cascader.SelectedOption.ShouldBeNull();
            zhejiang.IsExpanded.ShouldBeTrue();
            hangzhou.IsExpanded.ShouldBeTrue();

            cascader.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeNull();
            zhejiang.IsExpanded.ShouldBeFalse();
            hangzhou.IsExpanded.ShouldBeFalse();

            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should be visible after reopening.");
            zhejiangItem.IsExpanded.ShouldBeFalse();
            FindCascaderViewItem(cascaderView, "Hangzhou").ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Closing_Draft_Branch_Preserves_Commit_And_Reopens_Its_Path()
    {
        var (options, _, westLake, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            RaisePointerPressed(WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the Zhejiang root option should be visible."));
            Dispatcher.UIThread.RunJobs();
            RaisePointerPressed(WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "expanding Zhejiang should reveal Hangzhou."));
            Dispatcher.UIThread.RunJobs();
            RaisePointerPressed(WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "expanding Hangzhou should reveal West Lake."));

            cascader.SelectedOption.ShouldBeSameAs(westLake);
            cascader.IsDropDownOpen.ShouldBeFalse();
            options[0].IsExpanded.ShouldBeFalse("popup close should synchronously discard the expanded path.");
            options[0].Children.Single().IsExpanded.ShouldBeFalse("popup close should synchronously discard the expanded path.");
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(westLake);
            cascader.IsDropDownOpen.ShouldBeFalse();
            options[0].IsExpanded.ShouldBeFalse();
            options[0].Children.Single().IsExpanded.ShouldBeFalse();

            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "reopening should restore the committed West Lake path before draft navigation.");

            RaisePointerPressed(WaitFor(
                () => FindCascaderViewItem(cascaderView, "Jiangsu"),
                "the Jiangsu root option should be visible."));
            Dispatcher.UIThread.RunJobs();
            RaisePointerPressed(WaitFor(
                () => FindCascaderViewItem(cascaderView, "Nanjing"),
                "expanding Jiangsu should reveal Nanjing."));
            Dispatcher.UIThread.RunJobs();
            WaitFor(
                () => FindCascaderViewItem(cascaderView, "Xuanwu Lake"),
                "expanding Nanjing should reveal its leaf without committing it.");

            cascader.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(westLake);
            options[0].IsExpanded.ShouldBeFalse();
            options[0].Children.Single().IsExpanded.ShouldBeFalse();
            options[1].IsExpanded.ShouldBeFalse();
            options[1].Children.Single().IsExpanded.ShouldBeFalse();

            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var westLakeItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "reopening should restore the previously committed West Lake path.");
            westLakeItem.IsSelected.ShouldBeTrue();
            FindCascaderViewItem(cascaderView, "Nanjing").ShouldBeNull();
            options[1].IsExpanded.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Closing_Single_Select_Popup_Clears_Tree_Keyboard_Candidate()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should be visible when the popup opens.");

            RaiseKeyDown(cascader, Key.Down);
            Dispatcher.UIThread.RunJobs();
            zhejiangItem.IsCandidateSelected.ShouldBeTrue();

            cascader.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            zhejiangItem.IsCandidateSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Tree_Mode_Pointer_And_Keyboard_Navigation_Share_One_Active_Candidate()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the first root candidate should be visible.");
            var jiangsuItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Jiangsu"),
                "the second root candidate should be visible.");

            RaiseKeyDown(cascader, Key.Down);
            Dispatcher.UIThread.RunJobs();
            zhejiangItem.IsCandidateSelected.ShouldBeTrue();

            MovePointerTo(jiangsuItem, window);

            zhejiangItem.IsCandidateSelected.ShouldBeFalse();
            jiangsuItem.IsCandidateSelected.ShouldBeTrue();

            RaiseKeyDown(cascader, Key.Up);
            Dispatcher.UIThread.RunJobs();

            jiangsuItem.IsPointerOver.ShouldBeTrue(
                "the pointer remains over Jiangsu while keyboard navigation moves the active candidate.");
            zhejiangItem.IsCandidateSelected.ShouldBeTrue();
            jiangsuItem.IsCandidateSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_Filter_Value_Leaves_Filter_Mode(string filterValue)
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "lake";
            Dispatcher.UIThread.RunJobs();
            cascaderView.IsFiltering.ShouldBeTrue();

            cascaderView.FilterValue = filterValue;
            Dispatcher.UIThread.RunJobs();

            cascaderView.IsFiltering.ShouldBeFalse();
            cascaderView.FilterValue.ShouldBeNull();
            cascaderView.FilteredPathInfos.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Moving_Pointer_Over_Filter_Result_Migrates_Active_Candidate()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascaderView = new CascaderView
        {
            Width                 = 360,
            IsMotionEnabled       = false,
            IsShowEmptyIndicator  = false,
            OptionsSource         = options
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "lake";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "both lake filter results should be visible.");
            var firstItem = WaitFor(
                () => filterList.ContainerFromIndex(0) as CascaderViewFilterListItem,
                "the first filter result container should be realized.");
            var secondItem = WaitFor(
                () => filterList.ContainerFromIndex(1) as CascaderViewFilterListItem,
                "the second filter result container should be realized.");

            cascaderView.TryMoveFilterCandidate(1).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            firstItem.IsCandidateSelected.ShouldBeTrue();

            MovePointerTo(secondItem, window);

            firstItem.IsCandidateSelected.ShouldBeFalse();
            secondItem.IsCandidateSelected.ShouldBeTrue();
            filterList.CandidateSelectedIndex.ShouldBe(1);
            filterList.CandidateSelectedItem.ShouldBeSameAs(filterList.Items[1]);
            secondItem.IsPointerOver.ShouldBeTrue();

            cascaderView.TryMoveFilterCandidate(-1).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            firstItem.IsCandidateSelected.ShouldBeTrue();
            secondItem.IsCandidateSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Disabled_Filter_Result_Does_Not_Use_Hover_Background()
    {
        var disabledLeaf = new CascaderOption
        {
            Header    = "Xisha",
            Value     = "xisha",
            IsEnabled = false
        };
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = new[] { disabledLeaf }
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "xisha";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the disabled filter result should remain visible.");
            var resultItem = WaitFor(
                () => filterList.ContainerFromIndex(0) as CascaderViewFilterListItem,
                "the disabled filter result container should be realized.");
            filterList.ItemHoverBg = Brushes.Red;
            Dispatcher.UIThread.RunJobs();
            var defaultBackground = resultItem.Background;

            ((IPseudoClasses)resultItem.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();

            resultItem.Background.ShouldBe(defaultBackground);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Filter_Result_Is_Disabled_When_Any_Ancestor_Is_Disabled()
    {
        var enabledLeaf = new CascaderOption
        {
            Header    = "West Lake",
            Value     = "west-lake",
            IsEnabled = true
        };
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource = new[]
            {
                new CascaderOption
                {
                    Header = "Zhejiang",
                    Children =
                    [
                        new CascaderOption
                        {
                            Header    = "Hangzhou",
                            IsEnabled = false,
                            Children  = [enabledLeaf]
                        }
                    ]
                }
            }
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "west";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the path under a disabled ancestor should remain visible in filtered results.");
            var itemData = filterList.Items.Single() as CascaderViewFilterListItemData;

            itemData.ShouldNotBeNull();
            itemData.IsEnabled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clearing_Selected_Leaf_Allows_The_Same_Path_To_Expand_Again()
    {
        var (options, _, westLake, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsAllowClear    = true,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should be visible when the popup opens.");
            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();

            var hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "expanding Zhejiang should reveal Hangzhou.");
            RaisePointerPressed(hangzhouItem);
            Dispatcher.UIThread.RunJobs();

            var westLakeItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "expanding Hangzhou should reveal West Lake.");
            RaisePointerPressed(westLakeItem);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(westLake);
            cascader.Clear();
            Dispatcher.UIThread.RunJobs();

            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should remain available after clearing the selection.");
            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();

            hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "expanding Zhejiang again should reveal Hangzhou.");
            RaisePointerPressed(hangzhouItem);
            Dispatcher.UIThread.RunJobs();

            WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "the previously selected path should expand again after the selection is cleared.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Switching_Branches_Allows_A_Previously_Expanded_Descendant_To_Expand_Again()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var window = CreateWindow(cascaderView);

        try
        {
            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the Zhejiang root option should be visible.");
            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();

            var hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "expanding Zhejiang should reveal Hangzhou.");
            RaisePointerPressed(hangzhouItem);
            Dispatcher.UIThread.RunJobs();
            WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "expanding Hangzhou should reveal West Lake.");

            var jiangsuItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Jiangsu"),
                "the Jiangsu root option should be visible.");
            RaisePointerPressed(jiangsuItem);
            Dispatcher.UIThread.RunJobs();
            WaitFor(
                () => FindCascaderViewItem(cascaderView, "Nanjing"),
                "switching to Jiangsu should reveal Nanjing.");

            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();
            hangzhouItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "switching back to Zhejiang should reveal Hangzhou.");
            RaisePointerPressed(hangzhouItem);
            Dispatcher.UIThread.RunJobs();

            WaitFor(
                () => FindCascaderViewItem(cascaderView, "West Lake"),
                "a descendant should expand again after its previous level list was removed.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Search_Result_Down_Key_Moves_Candidate_Without_Selecting()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            cascader.FilterValue = "lake";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the popup filter result list should be visible after setting a filter value.");
            filterList.ItemCount.ShouldBe(2);

            RaiseKeyDown(cascader, Key.Down);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeNull();
            cascaderView.SelectedOption.ShouldBeNull();
            cascaderView.IsFiltering.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Search_Result_Enter_Key_Commits_Current_Candidate()
    {
        var (options, _, westLake, xuanwuLake) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            cascader.FilterValue = "lake";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the popup filter result list should be visible after setting a filter value.");
            filterList.ItemCount.ShouldBe(2);

            var input = cascader.GetVisualDescendants().OfType<SelectFilterTextBox>()
                                .Single(control => control.Name == "PART_SingleFilterInput");
            input.IsFocused.ShouldBeTrue();
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyRelease(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(xuanwuLake);
            cascader.SelectedOption.ShouldNotBeSameAs(westLake);
            cascaderView.IsFiltering.ShouldBeFalse();
            cascaderView.FilterValue.ShouldBeNull();
            cascader.IsDropDownOpen.ShouldBeFalse();
            options[1].IsExpanded.ShouldBeFalse();
            options[1].Children.Single().IsExpanded.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Search_Result_Pointer_Move_Then_Enter_Commits_Pointer_Candidate()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            cascader.FilterValue = "lake";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the popup filter result list should be visible after setting a filter value.");
            var pointerTarget = WaitFor(
                () => filterList.ContainerFromIndex(1) as CascaderViewFilterListItem,
                "the second filter result container should be realized.");
            var itemData       = filterList.Items[1].ShouldBeOfType<CascaderViewFilterListItemData>();
            var expectedOption = itemData.ExpandItems!.Last();

            RaiseKeyDown(cascader, Key.Down);
            MovePointerTo(pointerTarget, window);
            Dispatcher.UIThread.RunJobs();

            filterList.CandidateSelectedIndex.ShouldBe(1);
            pointerTarget.IsCandidateSelected.ShouldBeTrue();

            RaiseKeyDown(cascader, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(expectedOption);
            cascader.IsDropDownOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Tree_Mode_Right_And_Enter_Navigate_To_Leaf_And_Select()
    {
        var (options, _, westLake, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            FindCascaderViewItem(cascaderView, "Zhejiang").ShouldNotBeNull();

            RaiseKeyDown(cascader, Key.Down);
            Dispatcher.UIThread.RunJobs();
            RaiseKeyDown(cascader, Key.Right);
            Dispatcher.UIThread.RunJobs();
            WaitFor(() => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "Right should expand the root candidate and reveal its first child.");

            RaiseKeyDown(cascader, Key.Right);
            Dispatcher.UIThread.RunJobs();
            WaitFor(() => FindCascaderViewItem(cascaderView, "West Lake"),
                "Right should expand the child candidate and reveal its first leaf.");

            RaiseKeyDown(cascader, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldBeSameAs(westLake);
            cascader.IsDropDownOpen.ShouldBeFalse();
            options[0].IsExpanded.ShouldBeFalse();
            options[0].Children.Single().IsExpanded.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Tree_Mode_Left_Key_Returns_Candidate_To_Parent()
    {
        var (options, _, westLake, _) = CreateLakeOptions();
        var cascader = new Desktop.Controls.Cascader
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = options
        };
        var window = CreateWindow(cascader);

        try
        {
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var cascaderView = GetCascaderView(cascader);
            RaiseKeyDown(cascader, Key.Down);
            Dispatcher.UIThread.RunJobs();
            RaiseKeyDown(cascader, Key.Right);
            Dispatcher.UIThread.RunJobs();
            RaiseKeyDown(cascader, Key.Right);
            Dispatcher.UIThread.RunJobs();
            WaitFor(() => FindCascaderViewItem(cascaderView, "West Lake"),
                "Right should reveal the first leaf candidate before Left navigation.");

            RaiseKeyDown(cascader, Key.Left);
            Dispatcher.UIThread.RunJobs();
            RaiseKeyDown(cascader, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            cascader.SelectedOption.ShouldNotBeSameAs(westLake);
            cascader.SelectedOption.ShouldBeNull();
            FindCascaderViewItem(cascaderView, "West Lake").ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Selecting_Filter_Result_From_Popup_Hosted_CascaderView_Clears_Filter_Before_Selecting()
    {
        var (options, lingyin) = CreateProvinceOptions();
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var popup = new Popup
        {
            Child           = cascaderView,
            IsMotionEnabled = false,
            IsOpen          = true
        };
        var root = new Panel();
        root.Children.Add(popup);
        var window = CreateWindow(root);

        try
        {
            cascaderView.FilterValue = "lin";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the popup-hosted filter result list should be visible after setting a filter value.");
            filterList.ItemCount.ShouldBe(1);

            filterList.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            cascaderView.SelectedOption.ShouldBeSameAs(lingyin);
            cascaderView.IsFiltering.ShouldBeFalse();
            cascaderView.FilterValue.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pointer_Press_In_Standalone_View_Moves_Focus_From_External_Control_To_CascaderView()
    {
        var (options, _, _, _) = CreateLakeOptions();
        var externalFocusTarget = new Button
        {
            Content = "External"
        };
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var root = new StackPanel
        {
            Children =
            {
                externalFocusTarget,
                cascaderView
            }
        };
        var window = CreateWindow(root);

        try
        {
            externalFocusTarget.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            TopLevel.GetTopLevel(cascaderView)?.FocusManager?.GetFocusedElement()
                    .ShouldBe(externalFocusTarget);

            var zhejiangItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Zhejiang"),
                "the root option should be realized before pointer focus transfer is tested.");

            RaisePointerPressed(zhejiangItem);
            Dispatcher.UIThread.RunJobs();

            TopLevel.GetTopLevel(cascaderView)?.FocusManager?.GetFocusedElement()
                    .ShouldBe(cascaderView,
                        "standalone CascaderView owns arrow-key navigation after pointer interaction, so focus must leave the previously focused control such as the Gallery NavMenu.");

            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();

            WaitFor(() => FindCascaderViewItem(cascaderView, "Hangzhou"),
                "Right should be routed to the focused CascaderView and expand the current tree candidate.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Selecting_Filter_Result_Does_Not_Require_Already_Realized_Path_Containers()
    {
        var (options, lingyin) = CreateProvinceOptions();
        var cascaderView = new CascaderView
        {
            IsMotionEnabled      = false,
            IsShowEmptyIndicator = false,
            OptionsSource        = options
        };
        var window = CreateWindow(cascaderView);

        try
        {
            cascaderView.FilterValue = "lin";
            Dispatcher.UIThread.RunJobs();

            var filterList = WaitFor(
                () => cascaderView.GetVisualDescendants()
                                  .OfType<CascaderViewFilterList>()
                                  .FirstOrDefault(list => list.IsVisible),
                "the filter result list should be visible after setting a filter value.");

            cascaderView.IsVisible = false;
            Dispatcher.UIThread.RunJobs();

            filterList.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            cascaderView.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            cascaderView.SelectedOption.ShouldBeSameAs(lingyin);
            var lingyinItem = WaitFor(
                () => FindCascaderViewItem(cascaderView, "Lingyin shi"),
                "the matched leaf item should be realized after the CascaderView becomes visible again.");
            lingyinItem.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    private static CascaderViewItem? FindCascaderViewItem(Visual root, string header)
    {
        return root.GetVisualDescendants()
                   .OfType<CascaderViewItem>()
                   .FirstOrDefault(x => x.DataContext is ICascaderOption option &&
                                        string.Equals(option.Header?.ToString(), header, StringComparison.Ordinal));
    }

    private static T WaitFor<T>(Func<T?> probe, string because)
        where T : class
    {
        for (var i = 0; i < 80; i++)
        {
            Dispatcher.UIThread.RunJobs();
            var value = probe();
            if (value != null)
            {
                return value;
            }
            Thread.Sleep(10);
        }

        throw new TimeoutException(because);
    }

    private static (IReadOnlyList<ICascaderOption> Options, ICascaderOption Lingyin) CreateProvinceOptions()
    {
        var lingyin = new CascaderOption
        {
            Header = "Lingyin shi",
            Value  = "lingyin"
        };
        var zhejiang = new CascaderOption
        {
            Header = "Zhejiang",
            Value  = "zhejiang",
            Children =
            [
                new CascaderOption
                {
                    Header = "Hangzhou",
                    Value  = "hangzhou",
                    Children =
                    [
                        new CascaderOption
                        {
                            Header = "West Lake",
                            Value  = "west-lake"
                        },
                        lingyin
                    ]
                }
            ]
        };
        var jiangsu = new CascaderOption
        {
            Header = "Jiangsu",
            Value  = "jiangsu"
        };

        return ([zhejiang, jiangsu], lingyin);
    }

    private static (
        IReadOnlyList<ICascaderOption> Options,
        ICascaderOption Lingyin,
        ICascaderOption WestLake,
        ICascaderOption XuanwuLake) CreateLakeOptions()
    {
        var westLake = new CascaderOption
        {
            Header = "West Lake",
            Value  = "west-lake"
        };
        var lingyin = new CascaderOption
        {
            Header = "Lingyin shi",
            Value  = "lingyin"
        };
        var zhejiang = new CascaderOption
        {
            Header = "Zhejiang",
            Value  = "zhejiang",
            Children =
            [
                new CascaderOption
                {
                    Header = "Hangzhou",
                    Value  = "hangzhou",
                    Children =
                    [
                        westLake,
                        lingyin
                    ]
                }
            ]
        };
        var xuanwuLake = new CascaderOption
        {
            Header = "Xuanwu Lake",
            Value  = "xuanwu-lake"
        };
        var jiangsu = new CascaderOption
        {
            Header = "Jiangsu",
            Value  = "jiangsu",
            Children =
            [
                new CascaderOption
                {
                    Header = "Nanjing",
                    Value  = "nanjing",
                    Children =
                    [
                        xuanwuLake
                    ]
                }
            ]
        };

        return ([zhejiang, jiangsu], lingyin, westLake, xuanwuLake);
    }

    private static void RaisePointerPressed(Control source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }

    private static void MovePointerTo(Control target, AvaloniaWindow window)
    {
        var point = target.TranslatePoint(
            new Point(target.Bounds.Width / 2, target.Bounds.Height / 2),
            window);

        point.ShouldNotBeNull();
        window.MouseMove(point.Value);
        Dispatcher.UIThread.RunJobs();
    }

    private static void RaiseKeyDown(InputElement target, Key key)
    {
        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Source       = target,
            Key          = key,
            PhysicalKey  = key switch
            {
                Key.Enter  => PhysicalKey.Enter,
                Key.Escape => PhysicalKey.Escape,
                Key.Up     => PhysicalKey.ArrowUp,
                Key.Down   => PhysicalKey.ArrowDown,
                Key.Left   => PhysicalKey.ArrowLeft,
                Key.Right  => PhysicalKey.ArrowRight,
                _          => PhysicalKey.None
            },
            KeyModifiers = KeyModifiers.None
        });
    }

    private static CascaderView GetCascaderView(Desktop.Controls.Cascader cascader)
    {
        var field = typeof(Desktop.Controls.Cascader).GetField(
            "_cascaderView",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        field.ShouldNotBeNull();
        var cascaderView = field.GetValue(cascader) as CascaderView;
        cascaderView.ShouldNotBeNull();
        return cascaderView!;
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 420,
            Height = 320
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
