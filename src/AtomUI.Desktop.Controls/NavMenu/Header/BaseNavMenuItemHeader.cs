using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the header of a <see cref="NavMenuItem" />, which shows the icon, the text and the sub-menu
/// indicator of the item and carries the state values that the header themes select on.
/// </summary>
/// <remarks>
/// <para>
/// The template of a <see cref="NavMenuItem" /> creates the header and forwards the values of the item to
/// it. The item owns these values; the header mirrors them for styling.
/// </para>
/// </remarks>
public class BaseNavMenuItemHeader : TemplatedControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        HeaderedContentControl.HeaderProperty.AddOwner<BaseNavMenuItemHeader>();
    
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        HeaderedItemsControl.HeaderTemplateProperty.AddOwner<BaseNavMenuItemHeader>();
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<BaseNavMenuItemHeader, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseNavMenuItemHeader>();
    
    /// <summary>
    /// Identifies the <see cref="HasSubMenu" /> property.
    /// </summary>
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> HasSubMenuProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(nameof(HasSubMenu),
            o => o.HasSubMenu,
            (o, v) => o.HasSubMenu = v);
    
    /// <summary>
    /// Identifies the <see cref="IsSubMenuOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSubMenuOpenProperty =
        AvaloniaProperty.Register<BaseNavMenuItemHeader, bool>(nameof(IsSubMenuOpen));

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDangerProperty =
        NavMenuItem.IsDangerProperty.AddOwner<BaseNavMenuItemHeader>();
    
    /// <summary>
    /// Identifies the <see cref="IsDarkStyle" /> property.
    /// </summary>
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsDarkStyleProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(nameof(IsDarkStyle),
            o => o.IsDarkStyle,
            (o, v) => o.IsDarkStyle = v);
    
    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsSelected), 
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    /// <summary>
    /// Identifies the <see cref="IsInSelectedPath" /> property.
    /// </summary>
    public static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsInSelectedPathProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsInSelectedPath), 
            o => o.IsInSelectedPath,
            (o, v) => o.IsInSelectedPath = v);
    
    /// <summary>
    /// Content shown in the text area of the header. The template of the owning <see cref="NavMenuItem" />
    /// forwards the item's header to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <c>string</c> is shown as a text block. When <see cref="HeaderTemplate" /> is set, the text area presents
    /// the content with that template instead. <see langword="null" /> leaves the text area empty.
    /// </para>
    /// </remarks>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    /// <summary>
    /// Template that the text area of the header applies to <see cref="Header" />. The template of the owning
    /// <see cref="NavMenuItem" /> forwards the item's header template to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When it is <see langword="null" />, the text area shows the content created from <see cref="Header" />
    /// directly, which is a text block for a <c>string</c>.
    /// </para>
    /// </remarks>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    /// <summary>
    /// Icon shown in front of the text of the item. The template of the owning <see cref="NavMenuItem" />
    /// forwards the item's icon to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The icon takes the item icon size and is painted in the foreground of the header. A value that is not
    /// <see langword="null" /> sets the <c>icon</c> pseudo-class, which makes the icon area of the template
    /// visible; <see langword="null" /> hides it.
    /// </para>
    /// </remarks>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Controls whether the header animates changes of its background and foreground colors. The template of
    /// the owning <see cref="NavMenuItem" /> forwards the item's value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> enables brush transitions on the header, slow for the background and medium for
    /// the foreground; <see langword="false" /> changes the colors at once.
    /// </para>
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _hasSubMenu;
    
    /// <summary>
    /// Mirrors whether the owning <see cref="NavMenuItem" /> has child items. The template of the item forwards
    /// its value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> shows the sub-menu indicator in the header, which is painted in the foreground of
    /// the header; <see langword="false" /> hides it.
    /// </para>
    /// </remarks>
    public bool HasSubMenu
    {
        get => _hasSubMenu;
        set => SetAndRaise(HasSubMenuProperty, ref _hasSubMenu, value);
    }
    
    /// <summary>
    /// Mirrors whether the sub-menu of the owning <see cref="NavMenuItem" /> is open. The template of the item
    /// forwards its value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The header themes select the styles of a header with an open sub-menu with this value.
    /// </para>
    /// </remarks>
    public bool IsSubMenuOpen
    {
        get => GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }

    /// <summary>
    /// Mirrors the danger state of the owning <see cref="NavMenuItem" />. The template of the item forwards its
    /// value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> switches the header to the danger colors: the text color, also on hover, the
    /// background while the pointer is held down, and the text and background of the selected state.
    /// </para>
    /// </remarks>
    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }
    
    private bool _isDarkStyle;

    /// <summary>
    /// Mirrors the dark style state of the owning <see cref="NavMenuItem" />. The template of the item forwards
    /// its value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> replaces the colors of the header with the dark variants of the navigation menu
    /// tokens for the normal, hover, selected, disabled and danger states.
    /// </para>
    /// </remarks>
    public bool IsDarkStyle
    {
        get => _isDarkStyle;
        set => SetAndRaise(IsDarkStyleProperty, ref _isDarkStyle, value);
    }
    
    private bool _isSelected;
    
    /// <summary>
    /// Mirrors whether the owning <see cref="NavMenuItem" /> is selected. The template of the item binds this
    /// property to the item's <c>SelectingItemsControl.IsSelected</c> value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> gives the header the selected text color and the selected background.
    /// </para>
    /// </remarks>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    private bool _isInSelectedPath;
    
    /// <summary>
    /// Mirrors whether the owning <see cref="NavMenuItem" /> is an ancestor of the selected entry without being
    /// selected itself. The template of the item forwards its value to this property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see langword="true" /> gives the header the selected text color and leaves its background unchanged.
    /// </para>
    /// </remarks>
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

    internal static readonly DirectProperty<BaseNavMenuItemHeader, bool> IsKeyboardFocusOutlineVisibleProperty =
        AvaloniaProperty.RegisterDirect<BaseNavMenuItemHeader, bool>(
            nameof(IsKeyboardFocusOutlineVisible),
            o => o.IsKeyboardFocusOutlineVisible,
            (o, v) => o.IsKeyboardFocusOutlineVisible = v);

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

    private bool _isKeyboardFocusOutlineVisible;

    internal bool IsKeyboardFocusOutlineVisible
    {
        get => _isKeyboardFocusOutlineVisible;
        set => SetAndRaise(IsKeyboardFocusOutlineVisibleProperty, ref _isKeyboardFocusOutlineVisible, value);
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

    private readonly ManagedFocusOutline _keyboardFocusOutline = new();

    /// <summary>
    /// When the template has been applied, sets the <c>icon</c> pseudo-class from the current <see cref="Icon" />,
    /// so that the icon area of the template is visible exactly when an icon is set. Overrides call the base
    /// member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    /// <summary>
    /// When a property of the header changes, keeps its visual state in step: a change of <see cref="Icon" />
    /// updates the <c>icon</c> pseudo-class, and a change of the internal <c>IsKeyboardFocusOutlineVisible</c>
    /// value shows or hides the keyboard focus outline. Overrides call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IsKeyboardFocusOutlineVisibleProperty)
        {
            UpdateKeyboardFocusOutline();
        }
    }

    /// <summary>
    /// When the header is attached to the visual tree, shows the keyboard focus outline when
    /// <c>IsKeyboardFocusOutlineVisible</c> is <see langword="true" />; the outline is shown only while the header
    /// is attached. Overrides call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateKeyboardFocusOutline();
    }

    /// <summary>
    /// When the header is detached from the visual tree, hides the keyboard focus outline. Overrides call the base
    /// member first.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _keyboardFocusOutline.Hide();
    }

    private void UpdateKeyboardFocusOutline()
    {
        if (IsKeyboardFocusOutlineVisible && this.IsAttachedToVisualTree())
        {
            _keyboardFocusOutline.Show(this);
        }
        else
        {
            _keyboardFocusOutline.Hide();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuItemPseudoClass.Icon, Icon is not null);
    }

}
