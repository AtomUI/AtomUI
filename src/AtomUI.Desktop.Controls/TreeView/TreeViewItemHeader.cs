using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio)]
internal class TreeViewItemHeader : ContentControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        TreeViewItem.IsExpandedProperty.AddOwner<TreeViewItemHeader>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        TreeViewItem.IconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        TreeViewItem.IsCheckedProperty.AddOwner<TreeViewItemHeader>();
    
    public static readonly StyledProperty<bool> IsSelectedProperty =
        TreeViewItem.IsSelectedProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<PathIcon?> SwitcherExpandIconProperty =
        TreeViewItem.SwitcherExpandIconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<PathIcon?> SwitcherCollapseIconProperty =
        TreeViewItem.SwitcherCollapseIconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<PathIcon?> SwitcherRotationIconProperty =
        TreeViewItem.SwitcherRotationIconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<PathIcon?> SwitcherLoadingIconProperty =
        TreeViewItem.SwitcherLoadingIconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly StyledProperty<PathIcon?> SwitcherLeafIconProperty =
        TreeViewItem.SwitcherLeafIconProperty.AddOwner<TreeViewItemHeader>();

    public static readonly DirectProperty<TreeViewItemHeader, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsLeaf),
            o => o.IsLeaf,
            (o, v) => o.IsLeaf = v);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        TreeViewItem.IsLoadingProperty.AddOwner<TreeViewItemHeader>();
    
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        TreeViewItem.IsIndicatorEnabledProperty.AddOwner<TreeViewItemHeader>();
    
    public static readonly StyledProperty<string?> GroupNameProperty =
        TreeViewItem.GroupNameProperty.AddOwner<TreeViewItemHeader>();
    
    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<TreeViewItemHeader, bool>(nameof(IsMasked));
    
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

    public PathIcon? SwitcherExpandIcon
    {
        get => GetValue(SwitcherExpandIconProperty);
        set => SetValue(SwitcherExpandIconProperty, value);
    }

    public PathIcon? SwitcherCollapseIcon
    {
        get => GetValue(SwitcherCollapseIconProperty);
        set => SetValue(SwitcherCollapseIconProperty, value);
    }

    public PathIcon? SwitcherRotationIcon
    {
        get => GetValue(SwitcherRotationIconProperty);
        set => SetValue(SwitcherRotationIconProperty, value);
    }

    public PathIcon? SwitcherLoadingIcon
    {
        get => GetValue(SwitcherLoadingIconProperty);
        set => SetValue(SwitcherLoadingIconProperty, value);
    }

    public PathIcon? SwitcherLeafIcon
    {
        get => GetValue(SwitcherLeafIconProperty);
        set => SetValue(SwitcherLeafIconProperty, value);
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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

    public string? GroupName
    {
        get => GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }
    
    public bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }
    
    public bool IsIndicatorEnabled
    {
        get => GetValue(IsIndicatorEnabledProperty);
        set => SetValue(IsIndicatorEnabledProperty, value);
    }
    
    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsHoverProperty =
        AvaloniaProperty.Register<TreeViewItemHeader, bool>(nameof(IsHover), false);
    
    internal static readonly StyledProperty<bool> IsPressedProperty =
        AvaloniaProperty.Register<TreeViewItemHeader, bool>(nameof(IsPressed), false);
    
    public static readonly StyledProperty<IBrush?> ContentFrameBackgroundProperty =
        AvaloniaProperty.Register<TreeViewItemHeader, IBrush?>(nameof (ContentFrameBackground));

    internal static readonly DirectProperty<TreeViewItemHeader, TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, TreeItemHoverMode>(nameof(NodeHoverMode),
            o => o.NodeHoverMode,
            (o, v) => o.NodeHoverMode = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsShowLineProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsShowLine),
            o => o.IsShowLine,
            (o, v) => o.IsShowLine = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsShowIconProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsShowIcon),
            o => o.IsShowIcon,
            (o, v) => o.IsShowIcon = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IconEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IconEffectiveVisible),
            o => o.IconEffectiveVisible,
            (o, v) => o.IconEffectiveVisible = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsDragOverProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsDragOver),
            o => o.IsDragOver,
            (o, v) => o.IsDragOver = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeViewItemHeader>();

    internal static readonly DirectProperty<TreeViewItemHeader, NodeSwitcherButtonIconMode> SwitcherModeProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, NodeSwitcherButtonIconMode>(
            nameof(SwitcherMode),
            o => o.SwitcherMode,
            (o, v) => o.SwitcherMode = v);

    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsSwitcherRotationProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(
            nameof(IsSwitcherRotation),
            o => o.IsSwitcherRotation,
            (o, v) => o.IsSwitcherRotation = v);
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<TreeViewItemHeader>();
    
    internal static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeViewItemHeader, bool>(nameof(IsShowLeafIcon));
    
    internal static readonly DirectProperty<TreeViewItemHeader, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, int>(
            nameof(Level),
            o => o.Level,
            (o, v) => o.Level = v);
    
    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsFilterMatchProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(nameof(IsFilterMatch),
            o => o.IsFilterMatch,
            (o, v) => o.IsFilterMatch = v);
    
    internal static readonly DirectProperty<TreeViewItemHeader, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, string?>(nameof(FilterHighlightWords),
            o => o.FilterHighlightWords,
            (o, v) => o.FilterHighlightWords = v);
    
    internal static readonly DirectProperty<TreeViewItemHeader, InlineCollection?> FilterHighlightRunsProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, InlineCollection?>(
            nameof(FilterHighlightRuns), t => t.FilterHighlightRuns, 
            (t, v) => t.FilterHighlightRuns = v);
    
    internal static readonly DirectProperty<TreeViewItemHeader, TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, TreeFilterStrategy>(
            nameof(FilterStrategy),
            o => o.FilterStrategy,
            (o, v) => o.FilterStrategy = v);
    
    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        TreeView.FilterHighlightForegroundProperty.AddOwner<TreeViewItemHeader>();
    
    internal static readonly DirectProperty<TreeViewItemHeader, bool> IsSelectableProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemHeader, bool>(
            nameof(IsSelectable),
            o => o.IsSelectable,
            (o, v) => o.IsSelectable = v);
    
    internal bool IsHover
    {
        get => GetValue(IsHoverProperty);
        set => SetValue(IsHoverProperty, value);
    }
    
    internal bool IsPressed
    {
        get => GetValue(IsPressedProperty);
        set => SetValue(IsPressedProperty, value);
    }
    
    internal IBrush? ContentFrameBackground
    {
        get => GetValue(ContentFrameBackgroundProperty);
        set => SetValue(ContentFrameBackgroundProperty, value);
    }
    
    private TreeItemHoverMode _nodeHoverMode;

    internal TreeItemHoverMode NodeHoverMode
    {
        get => _nodeHoverMode;
        set => SetAndRaise(NodeHoverModeProperty, ref _nodeHoverMode, value);
    }

    private bool _isShowLine;

    internal bool IsShowLine
    {
        get => _isShowLine;
        set => SetAndRaise(IsShowLineProperty, ref _isShowLine, value);
    }

    private bool _isShowIcon = true;

    internal bool IsShowIcon
    {
        get => _isShowIcon;
        set => SetAndRaise(IsShowIconProperty, ref _isShowIcon, value);
    }

    private bool _iconEffectiveVisible = true;

    internal bool IconEffectiveVisible
    {
        get => _iconEffectiveVisible;
        set => SetAndRaise(IconEffectiveVisibleProperty, ref _iconEffectiveVisible, value);
    }

    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    private bool _isDragOver;

    internal bool IsDragOver
    {
        get => _isDragOver;
        set => SetAndRaise(IsDragOverProperty, ref _isDragOver, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private NodeSwitcherButtonIconMode _switcherMode;

    internal NodeSwitcherButtonIconMode SwitcherMode
    {
        get => _switcherMode;
        set => SetAndRaise(SwitcherModeProperty, ref _switcherMode, value);
    }

    private bool _isSwitcherRotation;

    internal bool IsSwitcherRotation
    {
        get => _isSwitcherRotation;
        set => SetAndRaise(IsSwitcherRotationProperty, ref _isSwitcherRotation, value);
    }
    
    internal ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }

    internal bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
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
    
    private TreeFilterStrategy _filterStrategy = TreeFilterStrategy.All;
    
    internal TreeFilterStrategy FilterStrategy
    {
        get => _filterStrategy;
        set => SetAndRaise(FilterStrategyProperty, ref _filterStrategy, value);
    }
    
    internal IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    private bool _isSelectable = true;
    
    internal bool IsSelectable
    {
        get => _isSelectable;
        set => SetAndRaise(IsSelectableProperty, ref _isSelectable, value);
    }
    #endregion
    
    private Control? _headerContentFrame;
    private NodeSwitcherButton? _nodeSwitcherButton;
    private IconPresenter? _iconPresenter;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowIconProperty || change.Property == IconProperty)
        {
            IconEffectiveVisible = IsShowIcon && Icon is not null;
        }
        else if (change.Property == IsLoadingProperty ||
                 change.Property == IsLeafProperty ||
                 change.Property == IsSwitcherRotationProperty)
        {
            SetupSwitcherButtonIconMode();
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }
        else if (change.Property == FilterHighlightWordsProperty ||
                 change.Property == IsFilterMatchProperty ||
                 change.Property == FilterStrategyProperty ||
                 change.Property == FilterHighlightForegroundProperty)
        {
            BuildFilterHighlightRuns();
        }
    }
    
    private void SetupSwitcherButtonIconMode()
    {
        if (!IsLeaf)
        {
            if (IsLoading)
            {
                SwitcherMode = NodeSwitcherButtonIconMode.Loading;
            }
            else
            {
                if (IsSwitcherRotation)
                {
                    SwitcherMode = NodeSwitcherButtonIconMode.Rotation;
                }
                else
                {
                    SwitcherMode = NodeSwitcherButtonIconMode.Default;
                }
            }
        }
        else
        {
            SwitcherMode = NodeSwitcherButtonIconMode.Leaf;
        }
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeRadio, newValue == ItemToggleType.Radio);
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetHeaderContentFrame(e.NameScope.Find<Control>("PART_HeaderContentFrame"));
        _iconPresenter = e.NameScope.Find<IconPresenter>("PART_IconPresenter");
        _nodeSwitcherButton = e.NameScope.Find<NodeSwitcherButton>("PART_NodeSwitcherButton");
        SetupSwitcherButtonIconMode();
    }

    private void SetHeaderContentFrame(Control? headerContentFrame)
    {
        if (_headerContentFrame != null)
        {
            _headerContentFrame.PointerEntered -= HandleHeaderPresenterEntered;
            _headerContentFrame.PointerExited  -= HandleHeaderPresenterExited;
        }

        _headerContentFrame = headerContentFrame;
        if (_headerContentFrame != null)
        {
            _headerContentFrame.PointerEntered += HandleHeaderPresenterEntered;
            _headerContentFrame.PointerExited  += HandleHeaderPresenterExited;
        }
    }

    internal Rect SwitcherButtonRect(Control relativeTo)
    {
        Debug.Assert(_nodeSwitcherButton != null);
        var point = _nodeSwitcherButton.TranslatePoint(new Point(0, 0), relativeTo) ?? default;
        return new Rect(point, _nodeSwitcherButton.Bounds.Size);
    }
    
    private void HandleHeaderPresenterEntered(object? sender, PointerEventArgs? args)
    {
        if (NodeHoverMode == TreeItemHoverMode.WholeLine)
        {
            return;
        }

        SetCurrentValue(IsHoverProperty, true);
    }

    private void HandleHeaderPresenterExited(object? sender, PointerEventArgs args)
    {
        if (NodeHoverMode == TreeItemHoverMode.WholeLine)
        {
            return;
        }

        SetCurrentValue(IsHoverProperty, false);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (NodeHoverMode == TreeItemHoverMode.WholeLine)
        {
            SetCurrentValue(IsHoverProperty, true);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (NodeHoverMode == TreeItemHoverMode.WholeLine)
        {
            SetCurrentValue(IsHoverProperty, false);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SetCurrentValue(IsPressedProperty, true);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        SetCurrentValue(IsPressedProperty, false);
    }

    public Point GetDragIndicatorOffset(Control relativeTo)
    {
        var offsetX = 0d;
        var offsetY = 0d;
        if (Level != 0)
        {
            if (_iconPresenter is not null && _iconPresenter.IsVisible)
            {
                var offset = _iconPresenter.TranslatePoint(new Point(0, 0), this) ?? default;
                offsetX = offset.X;
                offsetY = offset.Y;
            }
            else if (_headerContentFrame is not null)
            {
                var offset = _headerContentFrame.TranslatePoint(new Point(0, 0), this) ?? default;
                offsetX = offset.X;
                offsetY = offset.Y;
            }
        }
        else
        {
            if (_headerContentFrame is not null)
            {
                var offset = _headerContentFrame.TranslatePoint(new Point(0, 0), this) ?? default;
                offsetX = offset.X;
                offsetY = offset.Y;
            }
        }
        
        if (_nodeSwitcherButton is not null && _nodeSwitcherButton.IsLeafIconVisible)
        {
            offsetX -= _nodeSwitcherButton.Bounds.Width;
        }

        return this.TranslatePoint(new Point(offsetX, offsetY), relativeTo) ?? default;
    }
    
    private void BuildFilterHighlightRuns()
    {
        var highlightWords = FilterHighlightWords;
        if (!IsFilterMatch || string.IsNullOrEmpty(highlightWords))
        {
            FilterHighlightRuns = null;
            return;
        }

        string? headerText = null;
        if (Content is ITreeItemNode treeViewItemData)
        {
            headerText = treeViewItemData.Header as string;
        }
        else if (Content is string strContent)
        {
            headerText = strContent;
        }

        if (string.IsNullOrWhiteSpace(headerText))
        {
            FilterHighlightRuns = null;
            return;
        }

        var strategy         = FilterStrategy;
        var highlightedMatch = (strategy & TreeFilterStrategy.HighlightedMatch) == TreeFilterStrategy.HighlightedMatch;
        var highlightedWhole = !highlightedMatch && (strategy & TreeFilterStrategy.HighlightedWhole) == TreeFilterStrategy.HighlightedWhole;
        var bold             = (strategy & TreeFilterStrategy.BoldedMatch) == TreeFilterStrategy.BoldedMatch;

        if (highlightedWhole)
        {
            FilterHighlightRuns = new InlineCollection
            {
                CreateFilterHighlightRun(headerText, highlighted: true, bold: bold)
            };
            return;
        }

        if (!highlightedMatch)
        {
            FilterHighlightRuns = new InlineCollection
            {
                CreateFilterHighlightRun(headerText, highlighted: false, bold: bold)
            };
            return;
        }

        var firstIndex = headerText.IndexOf(highlightWords, StringComparison.Ordinal);
        if (firstIndex == -1)
        {
            FilterHighlightRuns = new InlineCollection
            {
                CreateFilterHighlightRun(headerText, highlighted: false, bold: bold)
            };
            return;
        }

        var runs            = new InlineCollection();
        var highlightLength = highlightWords.Length;
        var cursor          = 0;
        var currentIndex    = firstIndex;
        while (currentIndex != -1)
        {
            if (currentIndex > cursor)
            {
                runs.Add(CreateFilterHighlightRun(headerText.Substring(cursor, currentIndex - cursor),
                    highlighted: false,
                    bold: bold));
            }

            var highlightEnd = currentIndex + highlightLength;
            runs.Add(CreateFilterHighlightRun(headerText.Substring(currentIndex, highlightLength),
                highlighted: true,
                bold: bold));
            cursor       = highlightEnd;
            currentIndex = headerText.IndexOf(highlightWords, highlightEnd, StringComparison.Ordinal);
        }

        if (cursor < headerText.Length)
        {
            runs.Add(CreateFilterHighlightRun(headerText.Substring(cursor), highlighted: false, bold: bold));
        }

        FilterHighlightRuns = runs;
    }

    private Run CreateFilterHighlightRun(string text, bool highlighted, bool bold)
    {
        var run = new Run(text);
        if (highlighted)
        {
            run.Foreground = FilterHighlightForeground;
        }
        if (bold)
        {
            run.FontWeight = FontWeight.Bold;
        }
        return run;
    }

}
