using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

internal static class NavMenuItemContainerBinder
{
    public static void BindNode(
        NavMenuItem menuItem,
        object? item,
        CompositeDisposable disposables)
    {
        if (item is not INavMenuNode menuNode)
        {
            return;
        }

        if (menuNode is NavMenuNode navMenuNode)
        {
            BindProperty(navMenuNode, NavMenuNode.CommandProperty, NavMenuItem.CommandProperty);
            BindProperty(navMenuNode, NavMenuNode.CommandParameterProperty, NavMenuItem.CommandParameterProperty);
            BindProperty(navMenuNode, NavMenuNode.HeaderProperty, NavMenuItem.NodeHeaderProperty);
            BindProperty(navMenuNode, NavMenuNode.TooltipProperty, NavMenuItem.TooltipProperty);
            BindProperty(navMenuNode, NavMenuNode.IsTooltipEnabledProperty, NavMenuItem.IsTooltipEnabledProperty);
        }
        else
        {
            BindValue(nameof(INavMenuNode.Command), node => node.Command, NavMenuItem.CommandProperty);
            BindValue(nameof(INavMenuNode.CommandParameter), node => node.CommandParameter, NavMenuItem.CommandParameterProperty);
            BindValue(nameof(INavMenuNode.Header), node => node.Header, NavMenuItem.NodeHeaderProperty);
            BindValue(nameof(INavMenuNode.Tooltip), node => node.Tooltip, NavMenuItem.TooltipProperty);
            BindValue(nameof(INavMenuNode.IsTooltipEnabled), node => node.IsTooltipEnabled, NavMenuItem.IsTooltipEnabledProperty);
        }

        BindValue(nameof(INavMenuNode.Icon), node => node.Icon, NavMenuItem.IconProperty);
        BindValue(nameof(INavMenuNode.IsEnabled), node => node.IsEnabled, NavMenuItem.IsEnabledProperty);
        BindValue(nameof(INavMenuNode.ItemKey), node => node.ItemKey, NavMenuItem.ItemKeyProperty);

        // Initial publication (including CanExecute) can synchronously recycle the container.
        // Never start another binding after that scope has been released.
        void BindProperty(AvaloniaObject source, AvaloniaProperty sourceProperty, AvaloniaProperty targetProperty)
        {
            if (!disposables.IsDisposed)
            {
                disposables.Add(BindUtils.RelayBind(source, sourceProperty, menuItem, targetProperty));
            }
        }

        void BindValue<T>(string propertyName, Func<INavMenuNode, T> getter, AvaloniaProperty<T> targetProperty)
        {
            if (!disposables.IsDisposed)
            {
                disposables.Add(BindUtils.RelayBind(menuNode, propertyName, getter, menuItem, targetProperty));
            }
        }
    }

    public static void BindNodeHeaderTemplate(
        NavMenuItem menuItem,
        INavMenuNode menuNode,
        ItemsControl owner,
        CompositeDisposable disposables)
    {
        if (disposables.IsDisposed)
        {
            return;
        }

        IObservable<IDataTemplate?> templates;
        if (menuNode is NavMenuNode node)
        {
            templates = node.GetObservable(NavMenuNode.HeaderTemplateProperty);
        }
        else if (menuNode is INotifyPropertyChanged observableNode)
        {
            templates = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                      handler => observableNode.PropertyChanged += handler,
                                      handler => observableNode.PropertyChanged -= handler)
                                  .Where(change => string.IsNullOrEmpty(change.EventArgs.PropertyName) ||
                                                   change.EventArgs.PropertyName == nameof(INavMenuNode.HeaderTemplate))
                                  .Select(_ => menuNode.HeaderTemplate)
                                  .StartWith(menuNode.HeaderTemplate);
        }
        else
        {
            templates = Observable.Return(menuNode.HeaderTemplate);
        }

        // Both sources are chosen during container preparation. Keeping their projection in
        // one binding preserves local-value priority and restores the current owner fallback.
        disposables.Add(menuItem.Bind(NavMenuItem.HeaderTemplateProperty,
            templates.CombineLatest(owner.GetObservable(ItemsControl.ItemTemplateProperty),
                (template, fallback) => template ?? fallback)));
    }
}
