using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunSelectStateVerification()
    {
        var failures = new List<string>();
        VerifyClosedSelectCost(failures);
        VerifySelectPopupLifecycle(failures);
        VerifySelectDropDownEventCounts(failures);
        VerifySelectDefaultValues(failures);
        VerifySelectModeSpecificContent(failures);
        VerifySelectAccessoryPaths(failures);
        VerifySelectPopupInputPassThroughTargetsSelectBox(failures);
        VerifySelectLoadingLifecycle(failures);
        VerifySelectCandidateKeyboardHighlight(failures);
        VerifyFilteredSelectClickUsesVisibleOption(failures);
        VerifyFilteredMultiSelectClickUsesVisibleOption(failures);
        VerifySelectCandidateGroupingCanInitializeSelection(failures);
        VerifyHiddenSelectedOptionReappearsAfterTagRemoval(failures);
        VerifyOpenSelectTagCloseDoesNotDismissPopup(failures);
        VerifyOpenSelectBoxPressStillDismissesPopup(failures);
        VerifyOpenSingleFilterSelectKeepsInputEditable(failures);
        VerifySelectMultiCandidateClicksAccumulate(failures);
        VerifyTreeSelectPopupLifecycle(failures);
        VerifyTreeSelectPopupInputPassThroughTargetsSelectBox(failures);
        VerifyOpenTreeSelectTagCloseDoesNotDismissPopup(failures);
        VerifyOpenTreeSelectBoxPressStillDismissesPopup(failures);
        VerifyOpenSingleFilterTreeSelectKeepsInputEditable(failures);
        VerifySingleFilterSelectUsesSingleTextBox(failures);
        VerifySingleFilterTreeSelectUsesSingleTextBox(failures);
        VerifySingleSelectWithoutFilterDoesNotFocusTextBox(failures);
        VerifySingleTreeSelectWithoutFilterDoesNotFocusTextBox(failures);
        VerifySelectFilterTextBoxCaretLockIgnoresInputEvents(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Select state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Select state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyClosedSelectCost(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        using var realized = RealizeControl(select);

        Expect(FindVisualByName<SelectCandidateList>(select, "PART_CandidateList") == null,
            "Closed default Select should not create SelectCandidateList.",
            failures);
        var filterInput = FindVisualByName<SelectFilterTextBox>(select, "PART_SingleFilterInput");
        Expect(filterInput?.IsVisible == true,
            "Closed default single Select should show the static SelectFilterTextBox as the single display surface.",
            failures);
        var selectedOptionsBox = FindVisualByName<SelectResultOptionsBox>(select, "SelectedOptionsBox");
        Expect(selectedOptionsBox?.IsVisible == false,
            "Closed default single Select should keep the static SelectResultOptionsBox hidden.",
            failures);
        Expect(FindVisualByTypeName(select, "SelectAccessoryHost") == null,
            "Closed default Select should use lightweight SelectHandle instead of SelectAccessoryHost.",
            failures);
        Expect(FindVisualByTypeName(select, "SelectHandle") != null,
            "Closed default Select should still show a lightweight SelectHandle.",
            failures);
        Expect(GetPopupFrame(select) == null,
            "Closed default Select should keep Popup child empty before first open.",
            failures);
    }

    private static void VerifySelectPopupLifecycle(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        SelectCandidateList? firstCandidateList;
        using (var realized = RealizeControl(select))
        {
            MaterializeLazyPopupContentForTest(select);
            RefreshLayout(realized.Window);
            firstCandidateList = GetPopupContent<SelectCandidateList>(select);
            Expect(firstCandidateList != null,
                "Materializing Select popup should lazily create SelectCandidateList.",
                failures);

            MaterializeLazyPopupContentForTest(select);
            RefreshLayout(realized.Window);
            Expect(ReferenceEquals(firstCandidateList, GetPopupContent<SelectCandidateList>(select)),
                "Second Select popup materialization should reuse the first SelectCandidateList.",
                failures);
        }

        Expect(firstCandidateList?.GetVisualParent() == null,
            "Detached Select should clear lazy SelectCandidateList visual parent.",
            failures);
    }

    private static void VerifySelectDropDownEventCounts(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        var openedCount = 0;
        var closedCount = 0;
        select.DropDownOpened += (_, _) => openedCount++;
        select.DropDownClosed += (_, _) => closedCount++;

        InvokeAbstractSelectMethod(select, "OpenDropDown");
        Expect(openedCount == 0,
            $"OpenDropDown should not raise DropDownOpened before the Popup.Opened callback. Actual: {openedCount}.",
            failures);

        InvokeAbstractSelectMethod(select, "NotifyPopupOpened");
        Expect(openedCount == 1,
            $"Popup.Opened should raise DropDownOpened once. Actual: {openedCount}.",
            failures);

        InvokeAbstractSelectMethod(select, "CloseDropDown");
        Expect(closedCount == 0,
            $"CloseDropDown should not raise DropDownClosed before the Popup.Closed callback. Actual: {closedCount}.",
            failures);

        InvokeAbstractSelectMethod(select, "NotifyPopupClosed");
        Expect(closedCount == 1,
            $"Popup.Closed should raise DropDownClosed once. Actual: {closedCount}.",
            failures);
    }

    private static void VerifySelectDefaultValues(ICollection<string> failures)
    {
        var singleOptions = CreateSelectOptions();
        var singleSelect = new Select
        {
            DefaultValues = new List<object> { "orange" },
            OptionsSource = singleOptions
        };
        using (RealizeControl(singleSelect))
        {
            Expect(ReferenceEquals(singleSelect.SelectedOption, singleOptions[1]),
                "Single Select should apply the first matching default value.",
                failures);
        }

        var explicitSelected = new Select
        {
            DefaultValues  = new List<object> { "orange" },
            OptionsSource  = singleOptions,
            SelectedOption = singleOptions[0]
        };
        using (RealizeControl(explicitSelected))
        {
            Expect(ReferenceEquals(explicitSelected.SelectedOption, singleOptions[0]),
                "Single Select should not overwrite an explicit selected option with DefaultValues.",
                failures);
        }

        var multipleOptions = CreateSelectOptions();
        var multipleSelect = new Select
        {
            Mode          = SelectMode.Multiple,
            DefaultValues = new List<object> { "orange", "grape" },
            OptionsSource = multipleOptions
        };
        using (RealizeControl(multipleSelect))
        {
            Expect(multipleSelect.SelectedOptions?.Count == 2 &&
                   ReferenceEquals(multipleSelect.SelectedOptions[0], multipleOptions[1]) &&
                   ReferenceEquals(multipleSelect.SelectedOptions[1], multipleOptions[4]),
                "Multiple Select should apply all matching default values in order.",
                failures);
        }

        var treeSelectDefaultStrategy = TreeFilterStrategy.HighlightedWhole |
                                        TreeFilterStrategy.BoldedMatch |
                                        TreeFilterStrategy.ExpandPath |
                                        TreeFilterStrategy.HideUnMatched;
        Expect(new TreeSelect().FilterStrategy == treeSelectDefaultStrategy,
            "TreeSelect FilterStrategy should preserve the old TreeFilterHighlightStrategy default after the rename.",
            failures);
    }

    private static void VerifySelectModeSpecificContent(ICollection<string> failures)
    {
        var filterSelect = new Select
        {
            IsFilterEnabled = true,
            OptionsSource   = CreateSelectOptions(),
            PlaceholderText = "Choose fruit"
        };
        using var filterRealized = RealizeControl(filterSelect);
        var filterInput = FindVisualByName<SelectFilterTextBox>(filterSelect, "PART_SingleFilterInput");
        Expect(filterInput?.IsVisible == true,
            "Closed single filter Select should use SelectFilterTextBox for the single display surface.",
            failures);
        Expect(filterInput?.PlaceholderText == "Choose fruit",
            $"Closed empty single filter Select should show the normal placeholder through SelectFilterTextBox. Actual: {filterInput?.PlaceholderText}.",
            failures);
        Expect(FindVisualByName<AtomUI.Desktop.Controls.TextBlock>(filterSelect, "PlaceholderText")?.IsVisible == false,
            "Closed single filter Select should not use the outer placeholder text block.",
            failures);
        Expect(GetPopupContent<SelectCandidateList>(filterSelect) == null,
            "Closed single filter Select should still defer SelectCandidateList.",
            failures);

        var multiSelect = new Select
        {
            Mode          = SelectMode.Multiple,
            OptionsSource = CreateSelectOptions()
        };
        using var multiRealized = RealizeControl(multiSelect);
        Expect(FindVisualByName<SelectResultOptionsBox>(multiSelect, "SelectedOptionsBox")?.IsVisible == true,
            "Multiple Select should show SelectResultOptionsBox.",
            failures);
        Expect(FindVisualByName<SelectFilterTextBox>(multiSelect, "PART_SingleFilterInput")?.IsVisible == false,
            "Multiple Select should keep the single-mode filter text box hidden.",
            failures);

        multiSelect.SetCurrentValue(Select.ModeProperty, SelectMode.Single);
        RefreshLayout(multiRealized.Window);
        Expect(FindVisualByName<SelectResultOptionsBox>(multiSelect, "SelectedOptionsBox")?.IsVisible == false,
            "Switching Multiple Select to Single should hide SelectResultOptionsBox.",
            failures);
    }

    private static void VerifySelectAccessoryPaths(ICollection<string> failures)
    {
        var defaultSelect = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        using var defaultRealized = RealizeControl(defaultSelect);
        Expect(FindVisualByTypeName(defaultSelect, "SelectAccessoryHost") == null,
            "Default Select should not create SelectAccessoryHost.",
            failures);
        Expect(FindVisualByName<SelectMaxCountIndicator>(defaultSelect, "PART_SelectMaxCountIndicator")?.IsVisible == false,
            "Default Select should keep the static max-count indicator hidden.",
            failures);

        var maxCountSelect = new Select
        {
            Mode                    = SelectMode.Multiple,
            IsShowMaxCountIndicator = true,
            OptionsSource           = CreateSelectOptions()
        };
        using var maxCountRealized = RealizeControl(maxCountSelect);
        Expect(FindVisualByName<SelectMaxCountIndicator>(maxCountSelect, "PART_SelectMaxCountIndicator")?.IsVisible == true,
            "Select with max count indicator should show the static max-count indicator.",
            failures);

        maxCountSelect.SetCurrentValue(AbstractSelect.IsShowMaxCountIndicatorProperty, false);
        RefreshLayout(maxCountRealized.Window);
        Expect(FindVisualByName<SelectMaxCountIndicator>(maxCountSelect, "PART_SelectMaxCountIndicator")?.IsVisible == false,
            "Turning max count indicator off should hide the static max-count indicator.",
            failures);
        Expect(FindVisualByTypeName(maxCountSelect, "SelectHandle") != null,
            "Turning max count indicator off should fall back to lightweight SelectHandle.",
            failures);
    }

    private static void VerifySelectPopupInputPassThroughTargetsSelectBox(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };

        using (RealizeControl(select))
        {
            var popup    = FindVisualByName<Avalonia.Controls.Primitives.Popup>(select, "PART_Popup");
            var addOnBox = FindVisualByName<AddOnDecoratedBox>(select, AddOnDecoratedBox.AddOnDecoratedBoxPart);
            Expect(popup?.OverlayInputPassThroughElement != null &&
                   ReferenceEquals(popup.OverlayInputPassThroughElement, addOnBox),
                "Select popup should pass overlay input through to the select box so tag close clicks are not light-dismissed first.",
                failures);
        }
    }

    private static void VerifySelectLoadingLifecycle(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        using var realized = RealizeControl(select);
        var loadingIcon = select.SuffixLoadingIcon;
        var indicator   = FindVisualByName<IconPresenter>(select, "OpenIndicator");
        Expect(indicator != null && !ReferenceEquals(indicator.Icon, loadingIcon),
            "Non-loading Select should not attach the suffix loading icon.",
            failures);

        SetSelectLoadingForTest(select, true);
        RefreshLayout(realized.Window);
        indicator = FindVisualByName<IconPresenter>(select, "OpenIndicator");
        Expect(indicator != null && ReferenceEquals(indicator.Icon, loadingIcon) && indicator.IsVisible,
            "Loading Select should show the suffix loading icon in the shared SelectHandle indicator.",
            failures);

        SetSelectLoadingForTest(select, false);
        RefreshLayout(realized.Window);
        indicator = FindVisualByName<IconPresenter>(select, "OpenIndicator");
        Expect(indicator != null && !ReferenceEquals(indicator.Icon, loadingIcon),
            "Leaving loading state should restore the non-loading SelectHandle indicator.",
            failures);
        Expect(loadingIcon?.GetVisualParent() == null,
            "Detached suffix loading icon should not keep a visual parent.",
            failures);
    }

    private static void VerifySelectCandidateKeyboardHighlight(ICollection<string> failures)
    {
        var candidateList = new SelectCandidateList
        {
            ItemsSource   = CreateSelectOptions(),
            SelectionMode = SelectionMode.Single
        };

        using var realized = RealizeControl(candidateList);
        RefreshLayout(realized.Window);

        var firstItem = candidateList.ContainerFromIndex(0) as SelectCandidateListItem;
        var secondItem = candidateList.ContainerFromIndex(1) as SelectCandidateListItem;
        if (firstItem == null || secondItem == null)
        {
            failures.Add("Select candidate keyboard highlight should realize the first two options.");
            return;
        }

        candidateList.CandidateSelectedIndex = 0;
        RefreshLayout(realized.Window);
        Expect(firstItem.IsCandidateSelected && !secondItem.IsCandidateSelected,
            "Selecting the first candidate should highlight only the first option.",
            failures);

        candidateList.CandidateSelectedIndex = 1;
        RefreshLayout(realized.Window);
        Expect(!firstItem.IsCandidateSelected && secondItem.IsCandidateSelected,
            "Moving candidate highlight should clear the old option and highlight the new option.",
            failures);

        candidateList.CandidateSelectedIndex = -1;
        RefreshLayout(realized.Window);
        Expect(!firstItem.IsCandidateSelected && !secondItem.IsCandidateSelected,
            "Clearing candidate highlight should leave no visible option highlighted.",
            failures);

        candidateList.CandidateSelectedItem = candidateList.Items[0];
        RefreshLayout(realized.Window);
        Expect(firstItem.IsCandidateSelected && !secondItem.IsCandidateSelected,
            "Selecting a candidate by item should highlight only that option.",
            failures);

        candidateList.CandidateSelectedItem = candidateList.Items[1];
        RefreshLayout(realized.Window);
        Expect(!firstItem.IsCandidateSelected && secondItem.IsCandidateSelected,
            "Changing candidate item directly should clear the old option and highlight the new option.",
            failures);
    }

    private static void VerifyFilteredSelectClickUsesVisibleOption(ICollection<string> failures)
    {
        var options = CreatePersonSelectOptions();
        var candidateList = new SelectCandidateList
        {
            Filter              = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            FilterValue         = "Tom",
            FilterValueSelector = Select.HeaderFilterPropertySelector,
            ItemsSource         = options,
            SelectionMode       = SelectionMode.Single
        };

        using var realized = RealizeControl(candidateList);
        RefreshLayout(realized.Window);

        var visibleItem = candidateList.ContainerFromIndex(0) as SelectCandidateListItem;
        if (visibleItem == null)
        {
            failures.Add("Filtered Select should realize the first visible candidate item.");
            return;
        }

        RaisePrimaryPointerPressed(visibleItem, realized.Window);
        visibleItem.RaiseEvent(new RoutedEventArgs(ListViewItem.ClickedEvent, visibleItem));

        Expect(ReferenceEquals(candidateList.SelectedItem, options[2]),
            $"Filtered Select candidate click should select visible option Tom. Actual: {(candidateList.SelectedItem as ISelectOption)?.Header}.",
            failures);
    }

    private static void VerifyFilteredMultiSelectClickUsesVisibleOption(ICollection<string> failures)
    {
        var options = CreatePersonSelectOptions();
        var candidateList = new SelectCandidateList
        {
            Filter              = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            FilterValue         = "Tom",
            FilterValueSelector = Select.HeaderFilterPropertySelector,
            ItemsSource         = options,
            SelectionMode       = SelectionMode.Multiple
        };

        using var realized = RealizeControl(candidateList);
        RefreshLayout(realized.Window);

        var visibleItem = candidateList.ContainerFromIndex(0) as SelectCandidateListItem;
        if (visibleItem == null)
        {
            failures.Add("Filtered multi Select should realize the first visible candidate item.");
            return;
        }

        RaisePrimaryPointerPressed(visibleItem, realized.Window);

        Expect(candidateList.SelectedItems?.Contains(options[2]) == true &&
               candidateList.SelectedItems?.Contains(options[0]) != true,
            "Filtered multi Select click should toggle visible option Tom, not source option Jack.",
            failures);

        candidateList.FilterValue = null;
        RefreshLayout(realized.Window);

        var secondVisibleItem = candidateList.ContainerFromIndex(1) as SelectCandidateListItem;
        if (secondVisibleItem == null)
        {
            failures.Add("Multi Select should realize the second visible candidate item.");
            return;
        }

        RaisePrimaryPointerPressed(secondVisibleItem, realized.Window);

        Expect(candidateList.SelectedItems?.Contains(options[1]) == true &&
               candidateList.SelectedItems?.Contains(options[2]) == true,
            "Multi Select should preserve existing selected option when clicking another visible option.",
            failures);
    }

    private static void VerifySelectCandidateGroupingCanInitializeSelection(ICollection<string> failures)
    {
        var candidateList = new SelectCandidateList
        {
            ItemsSource   = CreateSelectOptions(),
            SelectionMode = SelectionMode.Single
        };

        using var realized = RealizeControl(candidateList);
        RefreshLayout(realized.Window);

        try
        {
            candidateList.SetCurrentValue(ListView.IsGroupEnabledProperty, true);
            RefreshLayout(realized.Window);
        }
        catch (Exception ex)
        {
            failures.Add($"SelectCandidateList should enable grouping without initializing selection during deferred refresh. Actual: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void VerifyHiddenSelectedOptionReappearsAfterTagRemoval(ICollection<string> failures)
    {
        var options = CreateSelectOptions();
        var candidateList = new SelectCandidateList
        {
            IsHideSelectedOptions = true,
            ItemsSource           = options,
            SelectionMode         = SelectionMode.Multiple
        };

        using var realized = RealizeControl(candidateList);
        RefreshLayout(realized.Window);

        // SelectedItems 变为只读，等价操作通过 Selection 模型完成
        candidateList.Selection.SelectAll();
        RefreshLayout(realized.Window);
        Expect(candidateList.IsEffectiveEmptyVisible,
            "SelectCandidateList should show empty indicator when all options are selected and hidden.",
            failures);

        candidateList.Selection.Deselect(options.Count - 1);
        RefreshLayout(realized.Window);
        var removedContainer = candidateList.ContainerFromItem(options[^1]) as SelectCandidateListItem;
        Expect(removedContainer is { IsVisible: true },
            "Removing a selected tag should make that option visible in the dropdown again.",
            failures);
        Expect(!candidateList.IsEffectiveEmptyVisible,
            "SelectCandidateList should hide empty indicator after a removed tag becomes available again.",
            failures);
    }

    private static void VerifyOpenSelectTagCloseDoesNotDismissPopup(ICollection<string> failures)
    {
        var options = CreateSelectOptions();
        var select = new Select
        {
            Mode            = SelectMode.Multiple,
            IsFilterEnabled = true,
            OptionsSource   = options,
            SelectedOptions = options.Take(2).Cast<ISelectOption>().ToList()
        };

        using var realized = RealizeControl(select);
        RefreshLayout(realized.Window);
        SetDropDownOpenTemplateStateForTest(select, true);
        InvokePopupLifecycleCallbackForTest(select, "PopupOpened");
        RefreshLayout(realized.Window);

        var tag = select.GetSelfAndVisualDescendants()
                        .OfType<SelectTag>()
                        .FirstOrDefault(x => ReferenceEquals(x.Item, options[0]));
        var closeButton = tag == null ? null : FindVisualByName<IconButton>(tag, "PART_CloseButton");
        if (closeButton == null)
        {
            failures.Add("Open multiple Select should realize a selected tag close button.");
            return;
        }

        var pointerPressed = RaisePrimaryPointerPressed(closeButton, realized.Window);
        closeButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, closeButton));
        RefreshLayout(realized.Window);

        Expect(pointerPressed.Handled,
            "Open Select should handle pointer press on a selected tag close button.",
            failures);
        Expect(select.IsDropDownOpen,
            "Open Select should keep the popup open after a selected tag close button removes an option.",
            failures);
        Expect(select.SelectedOptions?.Contains(options[0]) != true &&
               select.SelectedOptions?.Contains(options[1]) == true,
            "Open Select tag close button should remove only the clicked selected option.",
            failures);
    }

    private static void VerifyOpenSelectBoxPressStillDismissesPopup(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };

        using var realized = RealizeControl(select);
        RefreshLayout(realized.Window);
        SetDropDownOpenTemplateStateForTest(select, true);
        InvokePopupLifecycleCallbackForTest(select, "PopupOpened");
        RefreshLayout(realized.Window);

        var addOnBox = FindVisualByName<AddOnDecoratedBox>(select, AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (addOnBox == null)
        {
            failures.Add("Open Select should realize the select box for dismiss verification.");
            return;
        }

        var pointerPressed = RaisePrimaryPointerPressed(addOnBox, realized.Window);
        RefreshLayout(realized.Window);

        Expect(pointerPressed.Handled,
            "Open Select should handle pointer press on the select box.",
            failures);
        Expect(!select.IsDropDownOpen,
            "Pressing the Select box while open should still dismiss the popup.",
            failures);
    }

    private static void VerifyOpenSingleFilterSelectKeepsInputEditable(ICollection<string> failures)
    {
        var select = new Select
        {
            Width           = 240,
            Mode            = SelectMode.Single,
            IsFilterEnabled = true,
            OptionsSource   = CreateSelectOptions()
        };

        using var realized = RealizeControl(select);
        var filterInput = FindVisualByName<SelectFilterTextBox>(select, "PART_SingleFilterInput");
        if (filterInput == null)
        {
            failures.Add("Single filter Select should realize its filter input.");
            return;
        }

        MaterializeLazyPopupContentForTest(select);
        SetDropDownOpenTemplateStateForTest(select, true);
        InvokePopupLifecycleCallbackForTest(select, "PopupOpened");
        RefreshLayout(realized.Window);

        Expect(filterInput.IsVisible,
            "Open single filter Select should show its filter input for typing.",
            failures);
        Expect(filterInput.Bounds.Width > 0,
            $"Open single filter Select should give the filter input a positive width. Actual: {filterInput.Bounds.Width:0.###}.",
            failures);
        Expect(filterInput.IsFocused ||
               ReferenceEquals(realized.Window.FocusManager?.GetFocusedElement(), filterInput),
            "Open single filter Select should focus its filter input so typing goes into search.",
            failures);
    }

    private static void VerifySelectMultiCandidateClicksAccumulate(ICollection<string> failures)
    {
        var options = CreatePersonSelectOptions();
        var select = new Select
        {
            Mode            = SelectMode.Multiple,
            IsFilterEnabled = true,
            OptionsSource   = options
        };

        using var selectRealized = RealizeControl(select);
        MaterializeLazyPopupContentForTest(select);
        var candidateList = GetPopupContent<SelectCandidateList>(select);
        if (candidateList == null)
        {
            failures.Add("Multiple Select should materialize SelectCandidateList for accumulation verification.");
            return;
        }

        var popupFrame = GetPopupFrame(select);
        if (popupFrame != null)
        {
            popupFrame.Child = null;
        }

        SetCandidateListActivatedForTest(select, true);
        using var candidateRealized = RealizeControl(candidateList);
        RefreshLayout(candidateRealized.Window);

        var firstItem = candidateList.ContainerFromIndex(0) as SelectCandidateListItem;
        var secondItem = candidateList.ContainerFromIndex(1) as SelectCandidateListItem;
        if (firstItem == null || secondItem == null)
        {
            failures.Add("Multiple Select should realize first two candidate items.");
            return;
        }

        RaisePrimaryPointerPressed(firstItem, candidateRealized.Window);
        RaisePrimaryPointerPressed(secondItem, candidateRealized.Window);

        Expect(select.SelectedOptions?.Contains(options[0]) == true &&
               select.SelectedOptions?.Contains(options[1]) == true,
            $"Multiple Select should accumulate clicked options. Mode: {candidateList.SelectionMode}, activated: {GetCandidateListActivatedForTest(select)}, select count: {select.SelectedOptions?.Count ?? 0}, candidate count: {candidateList.SelectedItems?.Count ?? 0}.",
            failures);
    }

    private static List<SelectOption> CreatePersonSelectOptions()
    {
        return
        [
            new SelectOption { Header = "Jack", Content = "jack" },
            new SelectOption { Header = "Lucy", Content = "lucy" },
            new SelectOption { Header = "Tom", Content = "tom" }
        ];
    }

    private static void VerifyTreeSelectPopupLifecycle(ICollection<string> failures)
    {
        var treeSelect = new TreeSelect
        {
            ItemsSource = CreateTreeNodes()
        };
        AtomUI.Desktop.Controls.TreeView? firstTreeView;
        using (var realized = RealizeControl(treeSelect))
        {
            Expect(GetPopupContent<AtomUI.Desktop.Controls.TreeView>(treeSelect) == null,
                "Closed TreeSelect should not create TreeSelectTreeView.",
                failures);

            MaterializeLazyPopupContentForTest(treeSelect);
            RefreshLayout(realized.Window);
            firstTreeView = GetPopupContent<AtomUI.Desktop.Controls.TreeView>(treeSelect);
            Expect(firstTreeView?.GetType().Name == "TreeSelectTreeView",
                "Materializing TreeSelect popup should lazily create TreeSelectTreeView.",
                failures);

            MaterializeLazyPopupContentForTest(treeSelect);
            RefreshLayout(realized.Window);
            Expect(ReferenceEquals(firstTreeView, GetPopupContent<AtomUI.Desktop.Controls.TreeView>(treeSelect)),
                "TreeSelect should reuse lazy TreeSelectTreeView on repeated materialization.",
                failures);
        }

        Expect(firstTreeView?.GetVisualParent() == null,
            "Detached TreeSelect should clear lazy TreeSelectTreeView visual parent.",
            failures);
    }

    private static void VerifyTreeSelectPopupInputPassThroughTargetsSelectBox(ICollection<string> failures)
    {
        var treeSelect = new TreeSelect
        {
            ItemsSource = CreateTreeNodes()
        };

        using (RealizeControl(treeSelect))
        {
            var popup    = FindVisualByName<Avalonia.Controls.Primitives.Popup>(treeSelect, "PART_Popup");
            var addOnBox = FindVisualByName<AddOnDecoratedBox>(treeSelect, AddOnDecoratedBox.AddOnDecoratedBoxPart);
            Expect(popup?.OverlayInputPassThroughElement != null &&
                   ReferenceEquals(popup.OverlayInputPassThroughElement, addOnBox),
                "TreeSelect popup should pass overlay input through to the select box so tag close clicks are not light-dismissed first.",
                failures);
        }
    }

    private static void VerifyOpenTreeSelectTagCloseDoesNotDismissPopup(ICollection<string> failures)
    {
        var nodes = CreateTreeNodes();
        var selectedItems = new List<ITreeItemNode>
        {
            nodes[0].Children.ElementAt(1),
            nodes[1].Children.ElementAt(0)
        };
        var treeSelect = new TreeSelect
        {
            IsMultiple      = true,
            IsFilterEnabled = true,
            ItemsSource     = nodes,
            SelectedItems   = selectedItems
        };

        using var realized = RealizeControl(treeSelect);
        RefreshLayout(realized.Window);
        SetDropDownOpenTemplateStateForTest(treeSelect, true);
        InvokePopupLifecycleCallbackForTest(treeSelect, "PopupOpened");
        RefreshLayout(realized.Window);

        var tag = treeSelect.GetSelfAndVisualDescendants()
                            .OfType<SelectTag>()
                            .FirstOrDefault(x => ReferenceEquals(x.Item, selectedItems[0]));
        var closeButton = tag == null ? null : FindVisualByName<IconButton>(tag, "PART_CloseButton");
        if (closeButton == null)
        {
            failures.Add("Open multiple TreeSelect should realize a selected tag close button.");
            return;
        }

        var pointerPressed = RaisePrimaryPointerPressed(closeButton, realized.Window);
        closeButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, closeButton));
        RefreshLayout(realized.Window);

        Expect(pointerPressed.Handled,
            "Open TreeSelect should handle pointer press on a selected tag close button.",
            failures);
        Expect(treeSelect.IsDropDownOpen,
            "Open TreeSelect should keep the popup open after a selected tag close button removes an item.",
            failures);
        Expect(treeSelect.SelectedItems?.Contains(selectedItems[0]) != true &&
               treeSelect.SelectedItems?.Contains(selectedItems[1]) == true,
            "Open TreeSelect tag close button should remove only the clicked selected item.",
            failures);
    }

    private static void VerifyOpenTreeSelectBoxPressStillDismissesPopup(ICollection<string> failures)
    {
        var treeSelect = new TreeSelect
        {
            ItemsSource = CreateTreeNodes()
        };

        using var realized = RealizeControl(treeSelect);
        RefreshLayout(realized.Window);
        SetDropDownOpenTemplateStateForTest(treeSelect, true);
        InvokePopupLifecycleCallbackForTest(treeSelect, "PopupOpened");
        RefreshLayout(realized.Window);

        var addOnBox = FindVisualByName<AddOnDecoratedBox>(treeSelect, AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (addOnBox == null)
        {
            failures.Add("Open TreeSelect should realize the select box for dismiss verification.");
            return;
        }

        var pointerPressed = RaisePrimaryPointerPressed(addOnBox, realized.Window);
        RefreshLayout(realized.Window);

        Expect(pointerPressed.Handled,
            "Open TreeSelect should handle pointer press on the select box.",
            failures);
        Expect(!treeSelect.IsDropDownOpen,
            "Pressing the TreeSelect box while open should still dismiss the popup.",
            failures);
    }

    private static void VerifyOpenSingleFilterTreeSelectKeepsInputEditable(ICollection<string> failures)
    {
        var treeSelect = new TreeSelect
        {
            Width           = 240,
            IsFilterEnabled = true,
            ItemsSource     = CreateTreeNodes()
        };

        using var realized = RealizeControl(treeSelect);
        var filterInput = FindVisualByName<SelectFilterTextBox>(treeSelect, "PART_SingleFilterInput");
        if (filterInput == null)
        {
            failures.Add("Single filter TreeSelect should realize its filter input.");
            return;
        }

        MaterializeLazyPopupContentForTest(treeSelect);
        SetDropDownOpenTemplateStateForTest(treeSelect, true);
        InvokePopupLifecycleCallbackForTest(treeSelect, "PopupOpened");
        RefreshLayout(realized.Window);

        Expect(filterInput.IsVisible,
            "Open single filter TreeSelect should show its filter input for typing.",
            failures);
        Expect(filterInput.Bounds.Width > 0,
            $"Open single filter TreeSelect should give the filter input a positive width. Actual: {filterInput.Bounds.Width:0.###}.",
            failures);
        Expect(filterInput.IsFocused ||
               ReferenceEquals(realized.Window.FocusManager?.GetFocusedElement(), filterInput),
            "Open single filter TreeSelect should focus its filter input so typing goes into search.",
            failures);
    }

    private static void VerifySingleFilterSelectUsesSingleTextBox(ICollection<string> failures)
    {
        var options = CreateSelectOptions();
        var select = new Select
        {
            IsFilterEnabled = true,
            OptionsSource   = options,
            SelectedOption  = options[1],
            PlaceholderText = "Choose fruit"
        };

        using var realized = RealizeControl(select);
        var filterInput = FindVisualByName<SelectFilterTextBox>(select, "PART_SingleFilterInput");
        Expect(filterInput is { IsVisible: true },
            "Closed single filter Select with a selected option should show the single filter text box.",
            failures);
        Expect(filterInput?.Text == options[1].Header?.ToString(),
            $"Closed single filter Select should display the selected option through Text. Actual: {filterInput?.Text}.",
            failures);
        Expect(filterInput?.PlaceholderText == "Choose fruit",
            $"Closed single filter Select should keep the normal placeholder, not the selected option. Actual: {filterInput?.PlaceholderText}.",
            failures);
        Expect(filterInput?.IsReadOnly == true,
            "Closed searchable single Select should keep the shared filter text box read-only until the popup opens.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "Select", failures);
        var closedTextStart = GetTextPresenterStart(select, filterInput);
        var addOnBox        = FindVisualByName<AddOnDecoratedBox>(select, AddOnDecoratedBox.AddOnDecoratedBoxPart);
        var closedWidth = addOnBox?.Bounds.Width ?? 0;

        MaterializeLazyPopupContentForTest(select);
        SetDropDownOpenTemplateStateForTest(select, true);
        InvokePopupLifecycleCallbackForTest(select, "PopupOpened");
        RefreshLayout(realized.Window);
        Expect(addOnBox == null || Math.Abs(addOnBox.Bounds.Width - closedWidth) < 0.001,
            $"Opening single filter Select should not change the select box width. Closed: {closedWidth:0.###}, open: {addOnBox?.Bounds.Width:0.###}.",
            failures);
        Expect(filterInput is { IsVisible: true },
            "Open single filter Select should keep the same filter text box visible.",
            failures);
        VerifyOpenSingleFilterTextBoxPresentation(
            select,
            filterInput,
            options[1].Header?.ToString(),
            closedTextStart,
            "Select",
            failures);

        if (filterInput != null)
        {
            filterInput.Text = "ora";
            filterInput.CaretIndex = filterInput.Text.Length;
            filterInput.SelectionStart = filterInput.Text.Length;
            filterInput.SelectionEnd = filterInput.Text.Length;
            RefreshLayout(realized.Window);
            Expect(filterInput.CaretIndex == filterInput.Text.Length &&
                   filterInput.SelectionStart == filterInput.Text.Length &&
                   filterInput.SelectionEnd == filterInput.Text.Length,
                $"Open single filter Select should allow the search caret to move while editing. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
                failures);
            Expect(select.FilterValue?.ToString() == "ora",
                $"Open single filter Select should write actual typed text to FilterValue. Actual: {select.FilterValue}.",
                failures);
            Expect(filterInput.PlaceholderText == options[1].Header?.ToString(),
                $"Typing into single filter Select should keep the selected value only as placeholder backing text. Actual: {filterInput.PlaceholderText}.",
                failures);
        }

        SetDropDownOpenTemplateStateForTest(select, false);
        InvokePopupLifecycleCallbackForTest(select, "PopupClosed");
        RefreshLayout(realized.Window);
        Expect(filterInput is { IsVisible: true },
            "Closed single filter Select should keep the same filter text box visible after filtering.",
            failures);
        Expect(filterInput?.Text == options[1].Header?.ToString(),
            $"Closing single filter Select should restore the selected option text. Actual: {filterInput?.Text}.",
            failures);
        Expect(filterInput?.PlaceholderText == "Choose fruit",
            $"Closing single filter Select should restore the normal placeholder. Actual: {filterInput?.PlaceholderText}.",
            failures);
        Expect(filterInput?.CaretIndex == 0 &&
               filterInput?.SelectionStart == 0 &&
               filterInput?.SelectionEnd == 0,
            $"Closing single filter Select should reset the caret to the beginning. Caret: {filterInput?.CaretIndex}, selection: {filterInput?.SelectionStart}-{filterInput?.SelectionEnd}.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "Select", failures);
        Expect(select.FilterValue == null,
            $"Closing single filter Select should clear FilterValue. Actual: {select.FilterValue}.",
            failures);
    }

    private static void VerifySingleFilterTreeSelectUsesSingleTextBox(ICollection<string> failures)
    {
        var nodes        = CreateTreeNodes();
        var selectedNode = nodes[1].Children.ElementAt(1);
        var treeSelect = new TreeSelect
        {
            IsFilterEnabled = true,
            ItemsSource     = nodes,
            SelectedItem    = selectedNode,
            PlaceholderText = "Choose node"
        };

        using var realized = RealizeControl(treeSelect);
        var filterInput = FindVisualByName<SelectFilterTextBox>(treeSelect, "PART_SingleFilterInput");
        Expect(filterInput is { IsVisible: true },
            "Closed single filter TreeSelect with a selected item should show the single filter text box.",
            failures);
        Expect(filterInput?.Text == selectedNode.Header?.ToString(),
            $"Closed single filter TreeSelect should display the selected item through Text. Actual: {filterInput?.Text}.",
            failures);
        Expect(filterInput?.PlaceholderText == "Choose node",
            $"Closed single filter TreeSelect should keep the normal placeholder, not the selected item. Actual: {filterInput?.PlaceholderText}.",
            failures);
        Expect(filterInput?.IsReadOnly == true,
            "Closed searchable single TreeSelect should keep the shared filter text box read-only until the popup opens.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "TreeSelect", failures);
        var closedTextStart = GetTextPresenterStart(treeSelect, filterInput);
        var addOnBox        = FindVisualByName<AddOnDecoratedBox>(treeSelect, AddOnDecoratedBox.AddOnDecoratedBoxPart);
        var closedWidth = addOnBox?.Bounds.Width ?? 0;

        MaterializeLazyPopupContentForTest(treeSelect);
        SetDropDownOpenTemplateStateForTest(treeSelect, true);
        InvokePopupLifecycleCallbackForTest(treeSelect, "PopupOpened");
        RefreshLayout(realized.Window);
        Expect(addOnBox == null || Math.Abs(addOnBox.Bounds.Width - closedWidth) < 0.001,
            $"Opening single filter TreeSelect should not change the select box width. Closed: {closedWidth:0.###}, open: {addOnBox?.Bounds.Width:0.###}.",
            failures);
        Expect(filterInput is { IsVisible: true },
            "Open single filter TreeSelect should keep the same filter text box visible.",
            failures);
        VerifyOpenSingleFilterTextBoxPresentation(
            treeSelect,
            filterInput,
            selectedNode.Header?.ToString(),
            closedTextStart,
            "TreeSelect",
            failures);

        if (filterInput != null)
        {
            filterInput.Text = "Leaf";
            filterInput.CaretIndex = filterInput.Text.Length;
            filterInput.SelectionStart = filterInput.Text.Length;
            filterInput.SelectionEnd = filterInput.Text.Length;
            RefreshLayout(realized.Window);
            Expect(filterInput.CaretIndex == filterInput.Text.Length &&
                   filterInput.SelectionStart == filterInput.Text.Length &&
                   filterInput.SelectionEnd == filterInput.Text.Length,
                $"Open single filter TreeSelect should allow the search caret to move while editing. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
                failures);
            Expect(treeSelect.FilterValue?.ToString() == "Leaf",
                $"Open single filter TreeSelect should write actual typed text to FilterValue. Actual: {treeSelect.FilterValue}.",
                failures);
            Expect(filterInput.PlaceholderText == selectedNode.Header?.ToString(),
                $"Typing into single filter TreeSelect should keep the selected value only as placeholder backing text. Actual: {filterInput.PlaceholderText}.",
                failures);
        }

        SetDropDownOpenTemplateStateForTest(treeSelect, false);
        InvokePopupLifecycleCallbackForTest(treeSelect, "PopupClosed");
        RefreshLayout(realized.Window);
        Expect(filterInput is { IsVisible: true },
            "Closed single filter TreeSelect should keep the same filter text box visible after filtering.",
            failures);
        Expect(filterInput?.Text == selectedNode.Header?.ToString(),
            $"Closing single filter TreeSelect should restore the selected item text. Actual: {filterInput?.Text}.",
            failures);
        Expect(filterInput?.PlaceholderText == "Choose node",
            $"Closing single filter TreeSelect should restore the normal placeholder. Actual: {filterInput?.PlaceholderText}.",
            failures);
        Expect(filterInput?.CaretIndex == 0 &&
               filterInput?.SelectionStart == 0 &&
               filterInput?.SelectionEnd == 0,
            $"Closing single filter TreeSelect should reset the caret to the beginning. Caret: {filterInput?.CaretIndex}, selection: {filterInput?.SelectionStart}-{filterInput?.SelectionEnd}.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "TreeSelect", failures);
        Expect(treeSelect.FilterValue == null,
            $"Closing single filter TreeSelect should clear FilterValue. Actual: {treeSelect.FilterValue}.",
            failures);
    }

    private static void VerifySingleSelectWithoutFilterDoesNotFocusTextBox(ICollection<string> failures)
    {
        var options = CreateSelectOptions();
        var select = new Select
        {
            IsFilterEnabled = false,
            OptionsSource   = options,
            SelectedOption  = options[0]
        };

        using var realized = RealizeControl(select);
        var filterInput = FindVisualByName<SelectFilterTextBox>(select, "PART_SingleFilterInput");
        Expect(filterInput is { IsVisible: true },
            "Closed single Select without filtering should still show selected text through the shared text box.",
            failures);
        Expect(filterInput?.Focusable == false,
            "Closed single Select without filtering should keep the shared text box non-focusable so it cannot show a caret.",
            failures);
        Expect(filterInput?.IsReadOnly == true,
            "Closed single Select without filtering should keep the shared text box read-only.",
            failures);
        Expect(filterInput?.Cursor?.ToString() == StandardCursorType.Hand.ToString(),
            $"Closed single Select without filtering should show a hand cursor over the shared text box. Actual: {filterInput?.Cursor}.",
            failures);
        Expect(GetSelectFilterTextHostCursor(filterInput)?.ToString() == StandardCursorType.Hand.ToString(),
            $"Closed single Select without filtering should show a hand cursor over the text host. Actual: {GetSelectFilterTextHostCursor(filterInput)}.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "Select without filtering", failures);
    }

    private static void VerifySingleTreeSelectWithoutFilterDoesNotFocusTextBox(ICollection<string> failures)
    {
        var nodes        = CreateTreeNodes();
        var selectedNode = nodes[0].Children.ElementAt(0);
        var treeSelect = new TreeSelect
        {
            IsFilterEnabled = false,
            ItemsSource     = nodes,
            SelectedItem    = selectedNode
        };

        using var realized = RealizeControl(treeSelect);
        var filterInput = FindVisualByName<SelectFilterTextBox>(treeSelect, "PART_SingleFilterInput");
        Expect(filterInput is { IsVisible: true },
            "Closed single TreeSelect without filtering should still show selected text through the shared text box.",
            failures);
        Expect(filterInput?.Focusable == false,
            "Closed single TreeSelect without filtering should keep the shared text box non-focusable so it cannot show a caret.",
            failures);
        Expect(filterInput?.IsReadOnly == true,
            "Closed single TreeSelect without filtering should keep the shared text box read-only.",
            failures);
        Expect(filterInput?.Cursor?.ToString() == StandardCursorType.Hand.ToString(),
            $"Closed single TreeSelect without filtering should show a hand cursor over the shared text box. Actual: {filterInput?.Cursor}.",
            failures);
        Expect(GetSelectFilterTextHostCursor(filterInput)?.ToString() == StandardCursorType.Hand.ToString(),
            $"Closed single TreeSelect without filtering should show a hand cursor over the text host. Actual: {GetSelectFilterTextHostCursor(filterInput)}.",
            failures);
        VerifyClosedSingleFilterCaretLocked(filterInput, "TreeSelect without filtering", failures);
    }

    private static void VerifyClosedSingleFilterCaretLocked(
        SelectFilterTextBox? filterInput,
        string controlName,
        ICollection<string> failures)
    {
        if (filterInput == null)
        {
            return;
        }

        var moveTarget = filterInput.Text?.Length ?? 0;
        if (moveTarget == 0)
        {
            return;
        }

        filterInput.SelectionStart = moveTarget;
        filterInput.SelectionEnd   = moveTarget;
        filterInput.CaretIndex     = moveTarget;

        Expect(filterInput.CaretIndex == 0 &&
               filterInput.SelectionStart == 0 &&
               filterInput.SelectionEnd == 0,
            $"Closed single filter {controlName} should lock the caret at the beginning. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
            failures);

        if (TopLevel.GetTopLevel(filterInput) is not Visual root)
        {
            failures.Add($"Closed single filter {controlName} should be attached to a visual root for caret lock event verification.");
            return;
        }

        var endPoint = new Point(
            Math.Max(1, filterInput.Bounds.Width - 2),
            Math.Max(1, filterInput.Bounds.Height / 2));
        RaisePrimaryPointerPressed(filterInput, root, endPoint);
        Expect(filterInput.CaretIndex == 0 &&
               filterInput.SelectionStart == 0 &&
               filterInput.SelectionEnd == 0,
            $"Closed single filter {controlName} should ignore pointer-press caret positioning. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
            failures);

        foreach (var key in new[] { Key.Right, Key.End, Key.Left, Key.Home })
        {
            filterInput.CaretIndex     = 0;
            filterInput.SelectionStart = 0;
            filterInput.SelectionEnd   = 0;
            RaiseKeyDown(filterInput, key);
            Expect(filterInput.CaretIndex == 0 &&
                   filterInput.SelectionStart == 0 &&
                   filterInput.SelectionEnd == 0,
                $"Closed single filter {controlName} should ignore {key} caret navigation. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
                failures);
        }
    }

    private static void VerifySelectFilterTextBoxCaretLockIgnoresInputEvents(ICollection<string> failures)
    {
        var filterInput = new SelectFilterTextBox
        {
            Text                 = "Lucy",
            Width                = 120,
            Height               = 32,
            Focusable            = true,
            IsReadOnly           = true,
            IsCaretLockedToStart = true
        };

        using var realized = RealizeControl(filterInput);
        RefreshLayout(realized.Window);
        filterInput.Focus();
        filterInput.ResetCaretToStart();

        var endPoint = new Point(
            Math.Max(1, filterInput.Bounds.Width - 2),
            Math.Max(1, filterInput.Bounds.Height / 2));
        RaisePrimaryPointerPressed(filterInput, realized.Window, endPoint);
        RaisePrimaryPointerReleased(filterInput, realized.Window, endPoint);
        Expect(filterInput.CaretIndex == 0 &&
               filterInput.SelectionStart == 0 &&
               filterInput.SelectionEnd == 0,
            $"Locked SelectFilterTextBox should ignore pointer caret positioning. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
            failures);

        foreach (var key in new[] { Key.Right, Key.End, Key.Left, Key.Home })
        {
            RaiseKeyDown(filterInput, key);
            Expect(filterInput.CaretIndex == 0 &&
                   filterInput.SelectionStart == 0 &&
                   filterInput.SelectionEnd == 0,
                $"Locked SelectFilterTextBox should ignore {key} caret navigation. Caret: {filterInput.CaretIndex}, selection: {filterInput.SelectionStart}-{filterInput.SelectionEnd}.",
                failures);
        }
    }

    private static Border? GetPopupFrame(Control control)
    {
        var popup = FindVisualByName<Avalonia.Controls.Primitives.Popup>(control, "PART_Popup");
        return popup?.Child as Border;
    }

    private static T? GetPopupContent<T>(Control control)
        where T : Control
    {
        return GetPopupFrame(control)?.Child as T;
    }

    private static void MaterializeLazyPopupContentForTest(AbstractSelect select)
    {
        var method = select.GetType().GetMethod(
            "EnsurePopupContent",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(select, null);
    }

    private static void InvokeAbstractSelectMethod(AbstractSelect select, string methodName)
    {
        typeof(AbstractSelect)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(select, null);
    }

    private static void SetDropDownOpenTemplateStateForTest(AbstractSelect select, bool isOpen)
    {
        FindVisualByName<Avalonia.Controls.Primitives.Popup>(select, "PART_Popup")
            ?.SetValue(Avalonia.Controls.Primitives.Popup.IsOpenProperty, false);
        typeof(AbstractSelect)
            .GetField("IgnorePropertyChange", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(select, true);
        select.SetCurrentValue(AbstractSelect.IsDropDownOpenProperty, isOpen);
    }

    private static void InvokePopupLifecycleCallbackForTest(AbstractSelect select, string methodName)
    {
        select.GetType()
              .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
              ?.Invoke(select, new object?[] { null, EventArgs.Empty });
    }

    private static void SetSelectLoadingForTest(AbstractSelect select, bool value)
    {
        typeof(AbstractSelect)
            .GetProperty(nameof(AbstractSelect.IsLoading), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(select, value);
    }

    private static void SetCandidateListActivatedForTest(Select select, bool value)
    {
        typeof(Select)
            .GetField("_candidateListActivated", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(select, value);
    }

    private static bool GetCandidateListActivatedForTest(Select select)
    {
        return (bool)(typeof(Select)
                   .GetField("_candidateListActivated", BindingFlags.Instance | BindingFlags.NonPublic)
                   ?.GetValue(select) ?? false);
    }

    private static Point? GetTextPresenterStart(Control owner, SelectFilterTextBox? filterInput)
    {
        if (filterInput == null)
        {
            return null;
        }

        var textPresenter = FindVisualByName<TextPresenter>(filterInput, "PART_TextPresenter");
        return textPresenter?.TranslatePoint(default, owner);
    }

    private static Cursor? GetSelectFilterTextHostCursor(SelectFilterTextBox? filterInput)
    {
        return filterInput == null
            ? null
            : FindVisualByName<AtomUI.Desktop.Controls.ScrollViewer>(filterInput, "ScrollViewer")?.Cursor;
    }

    private static void VerifyOpenSingleFilterTextBoxPresentation(
        Control owner,
        SelectFilterTextBox? filterInput,
        string? selectedText,
        Point? closedTextStart,
        string controlName,
        ICollection<string> failures)
    {
        if (filterInput == null)
        {
            return;
        }

        Expect(filterInput.PlaceholderText == selectedText,
            $"Open single filter {controlName} should show the selected value through the search input placeholder. Expected: {selectedText}, actual: {filterInput.PlaceholderText}.",
            failures);
        Expect(string.IsNullOrEmpty(filterInput.Text),
            $"Open single filter {controlName} should clear Text before user input so the selected value behaves as placeholder. Actual: {filterInput.Text}.",
            failures);
        Expect(filterInput.IsReadOnly == false,
            $"Open single filter {controlName} should keep the search text box editable.",
            failures);

        var placeholder = FindVisualByName<Avalonia.Controls.TextBlock>(filterInput, "Placeholder");
        var filterTextPresenter = FindVisualByName<TextPresenter>(filterInput, "PART_TextPresenter");
        if (placeholder == null || filterTextPresenter == null)
        {
            failures.Add($"Open single filter {controlName} should materialize placeholder and filter text presenters.");
            return;
        }

        var placeholderStart = placeholder.TranslatePoint(default, owner);
        var filterStart = filterTextPresenter.TranslatePoint(default, owner);
        if (closedTextStart.HasValue && placeholderStart.HasValue)
        {
            var openShift = placeholderStart.Value.X - closedTextStart.Value.X;
            Expect(Math.Abs(openShift) <= 2,
                $"Open single filter {controlName} should not visibly shift the selected value when Text switches to PlaceholderText. Shift: {openShift:0.###}.",
                failures);
        }
        else
        {
            failures.Add($"Open single filter {controlName} should resolve closed result and open placeholder positions.");
        }
        Expect(filterStart.HasValue && placeholderStart.HasValue && filterStart.Value.X <= placeholderStart.Value.X,
            $"Open single filter {controlName} should keep the caret at the normal input start before the placeholder text.",
            failures);
        Expect(!BrushEquals(filterInput.CaretBrush, Brushes.Transparent),
            $"Open single filter {controlName} should keep the normal caret visible.",
            failures);
        Expect(BrushEquals(placeholder.Foreground, filterInput.PlaceholderForeground),
            $"Open single filter {controlName} selected placeholder should use placeholder foreground.",
            failures);
    }

    private static PointerPressedEventArgs RaisePrimaryPointerPressed(Control target, Visual root)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.LeftMouseButton,
            PointerUpdateKind.LeftButtonPressed);

        var args = new PointerPressedEventArgs(
            target,
            pointer,
            root,
            default,
            1,
            properties,
            KeyModifiers.None);
        target.RaiseEvent(args);
        return args;
    }

    private static PointerPressedEventArgs RaisePrimaryPointerPressed(Control target, Visual root, Point localPoint)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.LeftMouseButton,
            PointerUpdateKind.LeftButtonPressed);
        var rootPoint = target.TranslatePoint(localPoint, root) ?? localPoint;

        var args = new PointerPressedEventArgs(
            target,
            pointer,
            root,
            rootPoint,
            1,
            properties,
            KeyModifiers.None);
        target.RaiseEvent(args);
        return args;
    }

    private static PointerReleasedEventArgs RaisePrimaryPointerReleased(Control target, Visual root, Point localPoint)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.None,
            PointerUpdateKind.LeftButtonReleased);
        var rootPoint = target.TranslatePoint(localPoint, root) ?? localPoint;

        var args = new PointerReleasedEventArgs(
            target,
            pointer,
            root,
            rootPoint,
            1,
            properties,
            KeyModifiers.None,
            MouseButton.Left);
        target.RaiseEvent(args);
        return args;
    }

    private static KeyEventArgs RaiseKeyDown(Control target, Key key)
    {
        var args = new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Key          = key,
            KeyModifiers = KeyModifiers.None
        };
        target.RaiseEvent(args);
        return args;
    }
}
