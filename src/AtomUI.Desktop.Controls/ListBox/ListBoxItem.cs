using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

public class ListBoxItem : AvaloniaListBoxItem
{
    #region 内部属性定义

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        ListBox.ItemHoverBgProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        ListBox.ItemSelectedBgProperty.AddOwner<ListBoxItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListBoxItem>();
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsShowSelectedIndicator),
            o => o.IsShowSelectedIndicator,
            (o, v) => o.IsShowSelectedIndicator = v);
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsSelectedIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsSelectedIndicatorVisible),
            o => o.IsSelectedIndicatorVisible,
            (o, v) => o.IsSelectedIndicatorVisible = v);
    
    internal static readonly DirectProperty<ListBoxItem, IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, IconTemplate?>(nameof(SelectedIndicator),
            o => o.SelectedIndicator,
            (o, v) => o.SelectedIndicator = v);
    
    internal static readonly DirectProperty<ListBoxItem, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, bool>(nameof(IsFiltering),
            o => o.IsFiltering,
            (o, v) => o.IsFiltering = v);
    
    internal static readonly DirectProperty<ListBoxItem, object?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, object?>(nameof(FilterValue),
            o => o.FilterValue,
            (o, v) => o.FilterValue = v);
    
    internal static readonly DirectProperty<ListBoxItem, ListBoxItemFilterAction> FilterActionProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, ListBoxItemFilterAction>(
            nameof(FilterAction),
            o => o.FilterAction,
            (o, v) => o.FilterAction = v);
    
    internal static readonly DirectProperty<ListBoxItem, InlineCollection?> FilterHighlightRunsProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItem, InlineCollection?>(
            nameof(FilterHighlightRuns), t => t.FilterHighlightRuns, 
            (t, v) => t.FilterHighlightRuns = v);
    
    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        ListBox.FilterHighlightForegroundProperty.AddOwner<ListBoxItem>();
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    internal IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isShowSelectedIndicator;

    internal bool IsShowSelectedIndicator
    {
        get => _isShowSelectedIndicator;
        set => SetAndRaise(IsShowSelectedIndicatorProperty, ref _isShowSelectedIndicator, value);
    }
    
    private bool _isSelectedIndicatorVisible;

    internal bool IsSelectedIndicatorVisible
    {
        get => _isSelectedIndicatorVisible;
        set => SetAndRaise(IsSelectedIndicatorVisibleProperty, ref _isSelectedIndicatorVisible, value);
    }
    
    private IconTemplate? _selectedIndicator;

    internal IconTemplate? SelectedIndicator
    {
        get => _selectedIndicator;
        set => SetAndRaise(SelectedIndicatorProperty, ref _selectedIndicator, value);
    }
    
    private bool _isFiltering;

    internal bool IsFiltering
    {
        get => _isFiltering;
        set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }
    
    private object? _filterValue;

    internal object? FilterValue
    {
        get => _filterValue;
        set => SetAndRaise(FilterValueProperty, ref _filterValue, value);
    }
    
    private ListBoxItemFilterAction _filterAction = ListBoxItemFilterAction.All;
    
    public ListBoxItemFilterAction FilterAction
    {
        get => _filterAction;
        set => SetAndRaise(FilterActionProperty, ref _filterAction, value);
    }
    
    private InlineCollection? _filterHighlightRuns;
    internal InlineCollection? FilterHighlightRuns
    {
        get => _filterHighlightRuns;
        set => SetAndRaise(FilterHighlightRunsProperty, ref _filterHighlightRuns, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    #endregion
    
    private static readonly Point s_invalidPoint = new(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

        if (change.Property == IsSelectedProperty ||
            change.Property == IsShowSelectedIndicatorProperty)
        {
            ConfigureSelectedIndicator();
        }
        else if (change.Property == FilterValueProperty ||
                 change.Property == FilterActionProperty ||
                 change.Property == IsFilteringProperty)
        {
            NotifyBuildFilterHighlightRuns();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedIndicator();
    }

    private void ConfigureSelectedIndicator()
    {
        SetCurrentValue(IsSelectedIndicatorVisibleProperty, IsShowSelectedIndicator && IsSelected);
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
        {
            return;
        }

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is ListBox owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or 
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse || (p.Pointer.Type == PointerType.Pen && p.Properties.IsRightButtonPressed))
                {
                    // If the pressed point comes from a mouse or right-click pen, perform the selection immediately.
                    // In case of pen, only right-click is accepted, as left click (a tip touch) is used for scrolling. 
                    e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    // e.Handled = true;
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Source == this && !e.Handled && e.InitialPressMouseButton == MouseButton.Right)
        {
            var args = new ContextRequestedEventArgs(e);
            RaiseEvent(args);
            e.Handled = args.Handled;
        }
        if (!e.Handled && !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point    = e.GetCurrentPoint(this);
            var settings = TopLevel.GetTopLevel(e.Source as Visual)?.PlatformSettings;
            var tapSize  = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is ListBox owner)
            {
                if (owner.UpdateSelectionFromPointerEvent(this, e))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }
                    e.Handled = true;
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }

    protected virtual void NotifyBuildFilterHighlightRuns()
    {
        var filterValue = FilterValue?.ToString();
        if (IsFiltering && !string.IsNullOrEmpty(filterValue))
        {
            string? headerText   = null;
            if (Content is IListBoxItemData listBoxItemData)
            {
                headerText = listBoxItemData.Content as string;
            }
            else if (Content is string strContent)
            {
                headerText = strContent;
            }
            if (string.IsNullOrWhiteSpace(headerText))
            {
                return;
            }
            
            Debug.Assert(headerText != null);
            var highlightRanges = CalculateHighlightRanges(filterValue, headerText);
            var runs            = new InlineCollection();
            for (var i = 0; i < headerText.Length; i++)
            {
                var c   =  headerText[i];
                var run = new Run($"{c}");
                
                if (FilterAction.HasFlag(ListBoxItemFilterAction.HighlightedMatch))
                {
                    if (IsNeedHighlight(i, highlightRanges))
                    {
                        run.Foreground = FilterHighlightForeground;
                        if (FilterAction.HasFlag(ListBoxItemFilterAction.BoldedMatch))
                        {
                            run.FontWeight = FontWeight.Bold;
                        }
                    }
                }
                else if (FilterAction.HasFlag(ListBoxItemFilterAction.HighlightedWhole))
                {
                    run.Foreground = FilterHighlightForeground;
                    if (FilterAction.HasFlag(ListBoxItemFilterAction.BoldedMatch))
                    {
                        run.FontWeight = FontWeight.Bold;
                    }
                }
            
                runs.Add(run);
            }

            FilterHighlightRuns = runs;
        }
        else
        {
            FilterHighlightRuns = null;
        }
    }

    protected List<(int, int)> CalculateHighlightRanges(string highlightWords, string text)
    {
        var ranges          = new List<(int, int)>();
        int currentIndex    = 0;
        var highlightLength = highlightWords.Length;
        
        while (true)
        {
            int foundIndex = text.IndexOf(highlightWords, currentIndex, StringComparison.Ordinal);
            if (foundIndex == -1) // 如果没有找到，退出循环
            {
                break;
            }
                
            currentIndex = foundIndex + highlightLength;
            ranges.Add((foundIndex, currentIndex));
        }
        return ranges;
    }
    
    protected bool IsNeedHighlight(int pos, in List<(int, int)> ranges)
    {
        foreach (var range in ranges)
        {
            if (pos >= range.Item1 && pos < range.Item2)
            {
                return true;
            }
        }

        return false;
    }
}