// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal enum DataGridSortTooltipType
{
    Cancel,
    Ascending,
    Descending,
}

internal partial class DataGridColumnHeader
{
    #region 内部属性定义

    internal static readonly DirectProperty<DataGridColumnHeader, bool> CanUserSortProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(CanUserSort),
            o => o.CanUserSort,
            (o, v) => o.CanUserSort = v);

    internal static readonly DirectProperty<DataGridColumnHeader, ListSortDirection?> CurrentSortingStateProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, ListSortDirection?>(
            nameof(CurrentSortingState),
            o => o.CurrentSortingState,
            (o, v) => o.CurrentSortingState = v);

    internal static readonly StyledProperty<DataGridSortDirections> SupportedSortDirectionsProperty =
        DataGridColumn.SupportedSortDirectionsProperty.AddOwner<DataGridColumnHeader>();

    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsSorterTooltipVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsSorterTooltipVisible),
            o => o.IsSorterTooltipVisible,
            (o, v) => o.IsSorterTooltipVisible = v);

    public static readonly StyledProperty<DataGridSortTooltipType> SortTooltipTypeProperty =
        AvaloniaProperty.Register<DataGridColumnHeader, DataGridSortTooltipType>(nameof(SortTooltipType));

    private bool _canUserSort = false;

    internal bool CanUserSort
    {
        get => _canUserSort;
        set => SetAndRaise(CanUserSortProperty, ref _canUserSort, value);
    }

    private ListSortDirection? _currentSortingState;

    internal ListSortDirection? CurrentSortingState
    {
        get => _currentSortingState;
        set => SetAndRaise(CurrentSortingStateProperty, ref _currentSortingState, value);
    }

    public DataGridSortDirections SupportedSortDirections
    {
        get => GetValue(SupportedSortDirectionsProperty);
        set => SetValue(SupportedSortDirectionsProperty, value);
    }

    private bool _isSorterTooltipVisible;

    internal bool IsSorterTooltipVisible
    {
        get => _isSorterTooltipVisible;
        set => SetAndRaise(IsSorterTooltipVisibleProperty, ref _isSorterTooltipVisible, value);
    }

    internal DataGridSortTooltipType SortTooltipType
    {
        get => GetValue(SortTooltipTypeProperty);
        set => SetValue(SortTooltipTypeProperty, value);
    }

    #endregion

    private void NotifyPropertyChangedForSorting(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsSorterTooltipVisibleProperty || change.Property == CurrentSortingStateProperty)
        {
            ConfigureIsSorterTooltipVisible();
        }
    }

    private void ConfigureIsSorterTooltipVisible()
    {
        if (!IsSorterTooltipVisible || SupportedSortDirections == DataGridSortDirections.None)
        {
            return;
        }

        ListSortDirection? nextDirection = null;
        if (CurrentSortingState != null)
        {
            if ((SupportedSortDirections & DataGridSortDirections.All) == DataGridSortDirections.All)
            {
                if (CurrentSortingState == ListSortDirection.Ascending)
                {
                    nextDirection = ListSortDirection.Descending;
                }
                else if (CurrentSortingState == ListSortDirection.Descending)
                {
                    nextDirection = null;
                }
            }
            else if ((SupportedSortDirections & DataGridSortDirections.Ascending) == DataGridSortDirections.Ascending)
            {
                nextDirection = null;
            }
            else if ((SupportedSortDirections & DataGridSortDirections.Descending) ==
                     DataGridSortDirections.Descending)
            {
                nextDirection = null;
            }
        }
        else
        {
            if ((SupportedSortDirections & DataGridSortDirections.All) == DataGridSortDirections.All)
            {
                nextDirection = ListSortDirection.Ascending;
            }
            else if ((SupportedSortDirections & DataGridSortDirections.Ascending) == DataGridSortDirections.Ascending)
            {
                nextDirection = ListSortDirection.Ascending;
            }
            else if ((SupportedSortDirections & DataGridSortDirections.Descending) ==
                     DataGridSortDirections.Descending)
            {
                nextDirection = ListSortDirection.Descending;
            }
        }

        if (nextDirection == null)
        {
            SortTooltipType = DataGridSortTooltipType.Cancel;
        }
        else if (nextDirection == ListSortDirection.Ascending)
        {
            SortTooltipType = DataGridSortTooltipType.Ascending;
        }
        else if (nextDirection == ListSortDirection.Descending)
        {
            SortTooltipType = DataGridSortTooltipType.Descending;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (CanUserSort)
        {
            ToolTip.SetIsOpen(this, true);
        }
        HandlePointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (CanUserSort)
        {
            ToolTip.SetIsOpen(this, false);
        }
        HandlePointerExited(e);
    }

    internal void InvokeProcessSort(KeyModifiers keyModifiers, ListSortDirection? forcedDirection = null)
    {
        Debug.Assert(OwningGrid != null);
        if (OwningGrid.WaitForLostFocus(() => InvokeProcessSort(keyModifiers, forcedDirection)))
        {
            return;
        }

        if (OwningGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
        {
            ProcessSort(keyModifiers, forcedDirection);
        }
    }

    // TODO GroupSorting: verify that this member handles sorting for column groups and complete it where it is
    // missing. Remove this TODO when that is done.
    internal void ProcessSort(KeyModifiers keyModifiers, ListSortDirection? forcedDirection = null)
    {
        ProcessRangeSort(keyModifiers, forcedDirection);
    }

    private void ProcessRangeSort(
        KeyModifiers keyModifiers,
        ListSortDirection? forcedDirection)
    {
        if (OwningColumn is null ||
            OwningGrid is null ||
            !OwningColumn.EffectiveCanUserSort ||
            OwningColumn.FieldId is not { IsValid: true } field)
        {
            return;
        }

        var allowedDirections = OwningColumn.EffectiveSupportedSortDirections;
        DataGridSortDirection? requestedDirection = forcedDirection switch
        {
            ListSortDirection.Ascending => DataGridSortDirection.Ascending,
            ListSortDirection.Descending => DataGridSortDirection.Descending,
            _ => null
        };
        if (requestedDirection is { } requested)
        {
            var required = requested == DataGridSortDirection.Ascending
                ? DataGridSortDirections.Ascending
                : DataGridSortDirections.Descending;
            if ((allowedDirections & required) == 0)
            {
                return;
            }
        }

        var eventArgs = new DataGridColumnEventArgs(OwningColumn);
        OwningGrid.NotifyColumnSorting(eventArgs);
        if (eventArgs.Handled)
        {
            return;
        }

        if (forcedDirection.HasValue)
        {
            OwningGrid.SetSort(
                field,
                requestedDirection,
                DataGridSortUpdateMode.Replace);
            return;
        }

        KeyboardHelper.GetMetaKeyState(this, keyModifiers, out _, out var shift);
        OwningGrid.ApplySortGesture(field, shift, allowedDirections);
    }

    internal void InvokeClearSort()
    {
        Debug.Assert(OwningGrid != null);
        if (OwningGrid.WaitForLostFocus(InvokeClearSort))
        {
            return;
        }

        if (OwningGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
        {
            ProcessClearSort();
        }
    }

    internal void ProcessClearSort()
    {
        if (OwningGrid is not null)
        {
            if (OwningColumn is null ||
                !OwningColumn.EffectiveCanUserSort ||
                OwningColumn.FieldId is not { IsValid: true } field)
            {
                return;
            }
            var eventArgs = new DataGridColumnEventArgs(OwningColumn);
            OwningGrid.NotifyColumnSorting(eventArgs);
            if (!eventArgs.Handled)
            {
                OwningGrid.SetSort(field, null, DataGridSortUpdateMode.AppendOrReplace);
            }
        }
    }
}
