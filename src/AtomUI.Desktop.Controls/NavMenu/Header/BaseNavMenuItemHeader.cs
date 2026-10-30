using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

public class BaseNavMenuItemHeader : TemplatedControl
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> HeaderProperty =
        HeaderedContentControl.HeaderProperty.AddOwner<BaseNavMenuItemHeader>();
    
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        HeaderedItemsControl.HeaderTemplateProperty.AddOwner<BaseNavMenuItemHeader>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<BaseNavMenuItemHeader, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseNavMenuItemHeader>();
    
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> HasSubMenuProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(nameof(HasSubMenu),
            o => o.HasSubMenu,
            (o, v) => o.HasSubMenu = v);
    
    public static readonly StyledProperty<bool> IsSubMenuOpenProperty =
        AvaloniaProperty.Register<BaseNavMenuItemHeader, bool>(nameof(IsSubMenuOpen));
    
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsDarkStyleProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(nameof(IsDarkStyle),
            o => o.IsDarkStyle,
            (o, v) => o.IsDarkStyle = v);
    
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsSelected), 
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsInSelectedPathProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsInSelectedPath), 
            o => o.IsInSelectedPath,
            (o, v) => o.IsInSelectedPath = v);
    
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _hasSubMenu;
    
    public bool HasSubMenu
    {
        get => _hasSubMenu;
        set => SetAndRaise(HasSubMenuProperty, ref _hasSubMenu, value);
    }
    
    public bool IsSubMenuOpen
    {
        get => GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }
    
    private bool _isDarkStyle;

    public bool IsDarkStyle
    {
        get => _isDarkStyle;
        set => SetAndRaise(IsDarkStyleProperty, ref _isDarkStyle, value);
    }
    
    private bool _isSelected;
    
    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    private bool _isInSelectedPath;
    
    public bool IsInSelectedPath
    {
        get => _isInSelectedPath;
        set => SetAndRaise(IsInSelectedPathProperty, ref _isInSelectedPath, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty =
        AvaloniaProperty.Register<BaseNavMenuItemHeader, bool>(nameof(IsItemBackgroundEnabled), true);

    internal static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsKeyboardActiveProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsKeyboardActive),
            o => o.IsKeyboardActive,
            (o, v) => o.IsKeyboardActive = v);

    internal static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsPointerHoldProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsPointerHold),
            o => o.IsPointerHold,
            (o, v) => o.IsPointerHold = v);

    internal static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsInlineCollapsedProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsInlineCollapsed),
            o => o.IsInlineCollapsed,
            (o, v) => o.IsInlineCollapsed = v);

    internal bool IsItemBackgroundEnabled
    {
        get => GetValue(IsItemBackgroundEnabledProperty);
        set => SetValue(IsItemBackgroundEnabledProperty, value);
    }

    private bool _isKeyboardActive;

    internal bool IsKeyboardActive
    {
        get => _isKeyboardActive;
        set => SetAndRaise(IsKeyboardActiveProperty, ref _isKeyboardActive, value);
    }

    private bool _isPointerHold;

    internal bool IsPointerHold
    {
        get => _isPointerHold;
        set => SetAndRaise(IsPointerHoldProperty, ref _isPointerHold, value);
    }

    private bool _isInlineCollapsed;

    internal bool IsInlineCollapsed
    {
        get => _isInlineCollapsed;
        set => SetAndRaise(IsInlineCollapsedProperty, ref _isInlineCollapsed, value);
    }

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdatePseudoClasses();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuItemPseudoClass.Icon, Icon is not null);
    }

}
