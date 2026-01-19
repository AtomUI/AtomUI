using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio)]
internal class CascaderViewItemHeader : ContentControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        CascaderViewItem.IsExpandedProperty.AddOwner<CascaderViewItemHeader>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        CascaderViewItem.IconProperty.AddOwner<CascaderViewItemHeader>();

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        CascaderViewItem.IsCheckedProperty.AddOwner<CascaderViewItemHeader>();

    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        CascaderViewItem.ExpandIconProperty.AddOwner<CascaderViewItemHeader>();

    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        CascaderViewItem.LoadingIconProperty.AddOwner<CascaderViewItemHeader>();

    public static readonly DirectProperty<CascaderViewItemHeader, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, bool>(nameof(IsLeaf),
            o => o.IsLeaf,
            (o, v) => o.IsLeaf = v);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        CascaderViewItem.IsLoadingProperty.AddOwner<CascaderViewItemHeader>();
    
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        CascaderViewItem.IsIndicatorEnabledProperty.AddOwner<CascaderViewItemHeader>();
    
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }
    
    public IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private bool _isLeaf;

    public bool IsLeaf
    {
        get => _isLeaf;
        internal set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    public bool IsIndicatorEnabled
    {
        get => GetValue(IsIndicatorEnabledProperty);
        set => SetValue(IsIndicatorEnabledProperty, value);
    }
    
    #region 内部属性定义
    public static readonly StyledProperty<IBrush?> ContentFrameBackgroundProperty =
        AvaloniaProperty.Register<CascaderViewItemHeader, IBrush?>(nameof (ContentFrameBackground));

    internal static readonly DirectProperty<CascaderViewItemHeader, bool> IsShowIconProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, bool>(nameof(IsShowIcon),
            o => o.IsShowIcon,
            (o, v) => o.IsShowIcon = v);

    internal static readonly DirectProperty<CascaderViewItemHeader, bool> IconEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, bool>(nameof(IconEffectiveVisible),
            o => o.IconEffectiveVisible,
            (o, v) => o.IconEffectiveVisible = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderViewItemHeader>();
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<CascaderViewItemHeader>();
    
    internal static readonly DirectProperty<CascaderViewItemHeader, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, int>(
            nameof(Level),
            o => o.Level,
            (o, v) => o.Level = v);
    
    internal static readonly DirectProperty<CascaderViewItemHeader, bool> IsFilterMatchProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, bool>(nameof(IsFilterMatch),
            o => o.IsFilterMatch,
            (o, v) => o.IsFilterMatch = v);
    
    internal static readonly DirectProperty<CascaderViewItemHeader, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, string?>(nameof(FilterHighlightWords),
            o => o.FilterHighlightWords,
            (o, v) => o.FilterHighlightWords = v);
    
    internal static readonly DirectProperty<CascaderViewItemHeader, InlineCollection?> FilterHighlightRunsProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, InlineCollection?>(
            nameof(FilterHighlightRuns), t => t.FilterHighlightRuns, 
            (t, v) => t.FilterHighlightRuns = v);
    
    internal static readonly DirectProperty<CascaderViewItemHeader, CascaderItemFilterAction> ItemFilterActionProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemHeader, CascaderItemFilterAction>(
            nameof(ItemFilterAction),
            o => o.ItemFilterAction,
            (o, v) => o.ItemFilterAction = v);
    
    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        TreeView.FilterHighlightForegroundProperty.AddOwner<CascaderViewItemHeader>();

    internal IBrush? ContentFrameBackground
    {
        get => GetValue(ContentFrameBackgroundProperty);
        set => SetValue(ContentFrameBackgroundProperty, value);
    }
    
    private bool _isShowIcon = true;

    internal bool IsShowIcon
    {
        get => _isShowIcon;
        set => SetAndRaise(IsShowIconProperty, ref _isShowIcon, value);
    }

    private bool _iconEffectiveVisible = false;

    internal bool IconEffectiveVisible
    {
        get => _iconEffectiveVisible;
        set => SetAndRaise(IconEffectiveVisibleProperty, ref _iconEffectiveVisible, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    internal ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }
    
    private int _level;

    internal int Level
    {
        get => _level;
        set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    private bool _isFilterMatch;

    internal bool IsFilterMatch
    {
        get => _isFilterMatch;
        set => SetAndRaise(IsFilterMatchProperty, ref _isFilterMatch, value);
    }
    
    private string? _filterHighlightWords;

    internal string? FilterHighlightWords
    {
        get => _filterHighlightWords;
        set => SetAndRaise(FilterHighlightWordsProperty, ref _filterHighlightWords, value);
    }
    
    private InlineCollection? _filterHighlightRuns;
    internal InlineCollection? FilterHighlightRuns
    {
        get => _filterHighlightRuns;
        set => SetAndRaise(FilterHighlightRunsProperty, ref _filterHighlightRuns, value);
    }
    
    private CascaderItemFilterAction _itemFilterAction = CascaderItemFilterAction.All;
    
    public CascaderItemFilterAction ItemFilterAction
    {
        get => _itemFilterAction;
        set => SetAndRaise(ItemFilterActionProperty, ref _itemFilterAction, value);
    }
    
    internal IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    #endregion

    static CascaderViewItemHeader()
    {
        PressedMixin.Attach<CascaderViewItemHeader>();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowIconProperty || change.Property == IconProperty)
        {
            IconEffectiveVisible = IsShowIcon && Icon is not null;
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }
        else if (change.Property == FilterHighlightWordsProperty ||
                 change.Property == IsFilterMatchProperty)
        {
            BuildFilterHighlightRuns();
        }
        
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeRadio, newValue == ItemToggleType.Radio);
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ContentFrameBackgroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        IconEffectiveVisible = IsShowIcon && Icon is not null;
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
    
    private void BuildFilterHighlightRuns()
    {
        if (!IsFilterMatch)
        {
            FilterHighlightRuns = null;
        }
        else if (FilterHighlightWords != null)
        { 
            string? headerText   = null;
            if (Content is ICascaderViewItemData treeViewItemData)
            {
                headerText = treeViewItemData.Header as string;
            }
            else if (Content is string strContent)
            {
                headerText = strContent;
            }

            if (string.IsNullOrWhiteSpace(headerText))
            {
                return;
            }
            var ranges          = new List<(int, int)>();
            int currentIndex    = 0;
            var highlightLength = FilterHighlightWords.Length;
        
            while (true)
            {
                int foundIndex = headerText.IndexOf(FilterHighlightWords, currentIndex, StringComparison.Ordinal);
                if (foundIndex == -1) // 如果没有找到，退出循环
                {
                    break;
                }
                
                currentIndex = foundIndex + highlightLength;
                ranges.Add((foundIndex, currentIndex));
            }
            Debug.Assert(headerText != null);
            var runs = new InlineCollection();
            for (var i = 0; i < headerText.Length; i++)
            {
                var c   =  headerText[i];
                var run = new Run($"{c}");
                
                if (ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedMatch))
                {
                    if (IsNeedHighlight(i, ranges))
                    {
                        run.Foreground = FilterHighlightForeground;
                    }
                }
                else if (ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedWhole))
                {
                    run.Foreground = FilterHighlightForeground;
                }
         
                if (ItemFilterAction.HasFlag(TreeItemFilterAction.BoldedMatch))
                {
                    run.FontWeight = FontWeight.Bold;
                }
                runs.Add(run);
            }

            FilterHighlightRuns = runs;
        }
    }

    private bool IsNeedHighlight(int pos, in List<(int, int)> ranges)
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