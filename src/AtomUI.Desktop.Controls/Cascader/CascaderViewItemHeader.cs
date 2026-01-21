using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
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

    internal IBrush? ContentFrameBackground
    {
        get => GetValue(ContentFrameBackgroundProperty);
        set => SetValue(ContentFrameBackgroundProperty, value);
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
    
    #endregion

    static CascaderViewItemHeader()
    {
        PressedMixin.Attach<CascaderViewItemHeader>();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            IconEffectiveVisible = Icon is not null;
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
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
        IconEffectiveVisible = Icon is not null;
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
}