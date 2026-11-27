using System.Collections;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A flyout that shows a menu in a <see cref="MenuFlyoutPresenter" />, built from <see cref="Items" /> or
/// <see cref="ItemsSource" />, for example the drop-down of a <c>DropdownButton</c>. A click on a menu item is
/// reported through <see cref="MenuItemClicked" />.
/// </summary>
public class MenuFlyout : Flyout, IScrollAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MenuFlyout, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MenuFlyout, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// The theme of the <c>MenuItem</c> containers that the presenter generates for the items. Set it on the flyout;
    /// the value is relayed one-way to the item container theme of the presenter.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<MenuFlyout>();

    /// <summary>
    /// Identifies the <see cref="FlyoutPresenterTheme" /> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> FlyoutPresenterThemeProperty =
        AvaloniaProperty.Register<MenuFlyout, ControlTheme?>(nameof(FlyoutPresenterTheme));

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuFlyout>();

    /// <summary>
    /// The source of the items shown in the menu. A change is applied to <see cref="Items" />, which then shows the
    /// items of the source and becomes read-only; <see langword="null" /> returns it to an empty collection that
    /// stores its own items. Setting a source while <see cref="Items" /> holds items added directly throws
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The template that builds the header of each <c>MenuItem</c> generated from <see cref="Items" />. It is
    /// relayed one-way to the presenter; <see langword="null" /> applies no template.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The theme applied to the <see cref="MenuFlyoutPresenter" /> when the flyout opens. With
    /// <see langword="null" />, the theme of the presenter is left untouched.
    /// </summary>
    public ControlTheme? FlyoutPresenterTheme
    {
        get => GetValue(FlyoutPresenterThemeProperty);
        set => SetValue(FlyoutPresenterThemeProperty, value);
    }

    /// <summary>
    /// Controls scrolling in a menu that is taller than one display page. With <see langword="true" /> the height of
    /// the popup is limited to <c>DisplayPageSize</c> item rows plus the padding of the presenter, and the items
    /// scroll inside it. With <see langword="false" /> the height is unlimited and no scroll viewer is used.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// The items shown in the menu. They can be added in XAML content or through the collection itself, and are
    /// passed to the presenter as its items source. While <see cref="ItemsSource" /> is set, the collection shows
    /// that source instead and is read-only.
    /// </summary>
    [Content]
    public ItemCollection Items { get; }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when a menu item of the flyout is clicked. <c>Item</c> of the event arguments is the clicked
    /// <c>MenuItem</c>; the sender and the arguments come from the routed <c>MenuItemClicked</c> event of the
    /// presenter.
    /// </summary>
    public event EventHandler<FlyoutMenuItemClickedEventArgs>? MenuItemClicked;

    #endregion

    #region 运行时字段

    private protected MenuFlyoutPresenter? Presenter;

    private CompositeDisposable? _presenterBindingDisposables;

    #endregion

    #region 构造函数

    static MenuFlyout()
    {
        ShouldUseOverlayPopupProperty.OverrideDefaultValue<MenuFlyout>(true);
    }

    public MenuFlyout()
    {
        var itemCollectionType = typeof(ItemCollection);
        Items = (ItemCollection)Activator.CreateInstance(itemCollectionType, true)!;
        BindPointerPlacementOffsets(FlyoutHostTokenKind.HorizontalOffset, FlyoutHostTokenKind.VerticalOffset);
    }

    #endregion

    #region 内部协作 API

    internal override (object PopupRoot, object OverlayHost) GetPopupShadowTokenResourceKeys() =>
        (MenuFlyoutPresenterTokenKind.MenuPopupShadows, MenuFlyoutPresenterTokenKind.MenuPopupShadows);

    /// <summary>
    /// Returns the drop-down open motion as the default <see cref="Flyout.OpenMotion" /> of the menu, where other
    /// flyouts start with the popover open motion.
    /// </summary>
    internal override AbstractMotion GetDefaultOpenMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.DropdownOpen);

    /// <summary>
    /// Returns the drop-down close motion as the default <see cref="Flyout.CloseMotion" /> of the menu, where other
    /// flyouts start with the popover close motion.
    /// </summary>
    internal override AbstractMotion GetDefaultCloseMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.DropdownClose);

    #endregion

    #region 控件 API

    /// <summary>
    /// Creates the <see cref="MenuFlyoutPresenter" /> that the popup hosts, with <see cref="Items" /> as its items
    /// source, forwards the presenter's <c>MenuItemClicked</c> event to <see cref="MenuItemClicked" />, and relays
    /// the item template, the item container theme, the effective arrow visibility, the motion switch,
    /// <see cref="IsScrollEnabled" />, the overlay popup switch, the arrow position and the pinned-open state to it
    /// one-way. The relay bindings and the event subscription of a previous presenter are disposed first.
    /// </summary>
    protected override Control CreatePresenter()
    {
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(7);

        if (Presenter != null)
        {
            Presenter.MenuItemClicked -= HandleMenuItemClicked;
        }

        Presenter = new MenuFlyoutPresenter
        {
            MenuFlyout  = this,
            ItemsSource = Items
        };

        Presenter.MenuItemClicked += HandleMenuItemClicked;
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, ItemTemplateProperty, Presenter, MenuFlyoutPresenter.ItemTemplateProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, ItemContainerThemeProperty, Presenter,
                MenuFlyoutPresenter.ItemContainerThemeProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, IsArrowVisibleEffectiveProperty, Presenter,
                MenuFlyoutPresenter.IsArrowVisibleProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, IsMotionEnabledProperty, Presenter,
                MenuFlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, IsScrollEnabledProperty, Presenter,
                MenuFlyoutPresenter.IsScrollEnabledProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, Presenter,
                MenuFlyoutPresenter.ShouldUseOverlayPopupProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, ArrowPositionProperty, Presenter,
                MenuFlyoutPresenter.ArrowPositionProperty));
        _presenterBindingDisposables.Add(
            BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, Presenter,
                MenuFlyoutPresenter.IsPopupPinnedOpenProperty));
        ConfigureArrowPosition();
        ConfigureShowArrowEffective();
        return Presenter;
    }

    /// <summary>
    /// Applies <see cref="FlyoutPresenterTheme" /> to the presenter hosted by the popup when the flyout is opening,
    /// then calls the base implementation.
    /// </summary>
    protected override void OnOpening(CancelEventArgs args)
    {
        if (Popup.Child is { } presenter)
        {
            if (FlyoutPresenterTheme is { } theme)
            {
                presenter.SetValue(StyledElement.ThemeProperty, theme);
            }
        }

        base.OnOpening(args);
    }

    /// <summary>
    /// Calls the base implementation, then forwards a changed <see cref="ItemsSource" /> to <see cref="Items" />.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            Items.SetItemsSource(change.GetNewValue<IEnumerable?>());
        }
    }

    #endregion

    #region 内部实现

    private void HandleMenuItemClicked(object? sender, FlyoutMenuItemClickedEventArgs e)
    {
        MenuItemClicked?.Invoke(sender, e);
    }

    #endregion
}
