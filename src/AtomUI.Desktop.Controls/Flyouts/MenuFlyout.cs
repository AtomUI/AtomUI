using System.Collections;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Templates;
using Avalonia.Input.Raw;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class MenuFlyout : Flyout
{
    #region 公共属性定义

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MenuFlyout, IEnumerable?>(
            nameof(ItemsSource));
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MenuFlyout, IDataTemplate?>(nameof(ItemTemplate));
    
    public static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty =
        ItemsControl.ItemContainerThemeProperty.AddOwner<MenuFlyout>();
    
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

    [Content] 
    public ItemCollection Items { get; }
    
    public Func<IPopupHostProvider, RawPointerEventArgs, bool>? ClickHideFlyoutPredicate;
    #endregion
    
    private protected MenuFlyoutPresenter? Presenter;
    private CompositeDisposable? _presenterBindingDisposables;
    
    public MenuFlyout()
    {
        var itemCollectionType = typeof(ItemCollection);
        Items = (ItemCollection)Activator.CreateInstance(itemCollectionType, true)!;
    }
    
    protected override Control CreatePresenter()
    {
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(4);
        
        Presenter = new MenuFlyoutPresenter
        {
            MenuFlyout  = this,
            ItemsSource = Items
        };
        
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, Presenter, MenuFlyoutPresenter.ItemTemplateProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, Presenter, MenuFlyoutPresenter.ItemContainerThemeProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowEffectiveProperty, Presenter, MenuFlyoutPresenter.IsShowArrowProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, Presenter, MenuFlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsUseOverlayLayerProperty, Presenter, MenuFlyoutPresenter.IsUseOverlayLayerProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ArrowPositionProperty, Presenter, MenuFlyoutPresenter.ArrowPositionProperty));
        ConfigureArrowPosition();
        ConfigureShowArrowEffective();

        return Presenter;
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

    protected internal override void NotifyPopupCreated(Popup popup)
    {
        base.NotifyPopupCreated(popup);
        popup.IsLightDismissEnabled     = false;
        popup.IsDetectMouseClickEnabled = true;
        popup.IgnoreFirstDetected       = false;
        popup.ClickHidePredicate        = ClickHideFlyoutPredicate;
        popup.CloseAction               = PopupCloseAction;
    }

    private void PopupCloseAction(Popup popup)
    {
        Hide();
    }
    
    protected override bool HideCore(bool canCancel = true)
    {
        if (!IsOpen)
        {
            return false;
        }

        if (canCancel)
        {
            if (CancelClosing())
            {
                return false;
            }
        }

        if (Popup.PlacementTarget?.GetVisualRoot() is null)
        {
            return base.HideCore(false);
        }
        
        NotifyAboutToClose();
        
        if (Presenter != null)
        {
            if (IsMotionEnabled)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    foreach (var childItem in Presenter.Items)
                    {
                        if (childItem is MenuItem menuItem)
                        {
                            await menuItem.CloseItemAsync();
                        }
                    }
                    IsOpen                  = false;
                    Popup.MotionAwareClose(HandlePopupClosed);
                });
            }
            else
            {
                foreach (var childItem in Presenter.Items)
                {
                    if (childItem is MenuItem menuItem)
                    {
                        menuItem.Close();
                    }
                }
            
                IsOpen = false;
                Popup.MotionAwareClose(HandlePopupClosed);
            }
        }

        return true;
    }
}