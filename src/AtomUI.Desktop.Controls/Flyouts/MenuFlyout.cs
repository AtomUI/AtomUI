using System.Collections;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public class MenuFlyout : Flyout, IScrollAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MenuFlyout, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MenuFlyout, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<MenuFlyout>();

    public static readonly StyledProperty<ControlTheme?> FlyoutPresenterThemeProperty =
        AvaloniaProperty.Register<MenuFlyout, ControlTheme?>(nameof(FlyoutPresenterTheme));

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuFlyout>();

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public ControlTheme? FlyoutPresenterTheme
    {
        get => GetValue(FlyoutPresenterThemeProperty);
        set => SetValue(FlyoutPresenterThemeProperty, value);
    }

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    [Content]
    public ItemCollection Items { get; }

    #endregion

    #region 公共事件定义

    public event EventHandler<FlyoutMenuItemClickedEventArgs>? MenuItemClicked;

    #endregion

    private protected MenuFlyoutPresenter? Presenter;
    private CompositeDisposable? _presenterBindingDisposables;

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

    private void HandleMenuItemClicked(object? sender, FlyoutMenuItemClickedEventArgs e)
    {
        MenuItemClicked?.Invoke(sender, e);
    }

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            Items.SetItemsSource(change.GetNewValue<IEnumerable?>());
        }
    }
}
