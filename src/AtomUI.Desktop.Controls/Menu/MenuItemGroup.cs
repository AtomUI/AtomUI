using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 下拉菜单里的「分组标题 + 选项列表」容器，对应 antd 的
/// <c>ant-menu-item-group</c>：标题渲染为不可交互的标题行，子项为普通菜单项。
/// </summary>
public class MenuItemGroup : ItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MenuItemGroup, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MenuItemGroup, IDataTemplate?>(nameof(HeaderTemplate));

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

    private const string ItemTitleGroupClass = "semantic-item-title-group";

    // 分组所在层级由 owner 在 prepare 时下发；分组内的菜单项继承该层级。
    private MenuSemanticLevel _semanticLevel;

    internal MenuSemanticLevel SemanticLevel
    {
        get => _semanticLevel;
        set
        {
            MenuSemanticLevelScope.ApplyGroupLevel(this, value);
            if (_semanticLevel != value)
            {
                _semanticLevel = value;
                MenuSemanticLevelScope.ApplyChildrenLevel(this, value);
            }
        }
    }

    public MenuItemGroup()
    {
        // itemTitle 语义部件锚定到分组标题元素；分组容器本身承担跨视觉根路由的
        // 中间标记类（与 DropdownButton.SemanticParts.cs 中 itemTitle 的
        // SelectorRoute 保持一致），标题元素在模板应用时注入。
        Classes.Add(ItemTitleGroupClass);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        var menuItem = new MenuItem();
        menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
        ApplyChildSemanticLevel(menuItem);
        return menuItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is MenuItem or MenuSeparator)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
            ApplyChildSemanticLevel(menuItem);

            if (item != null && item is not Visual)
            {
                if (!menuItem.IsSet(MenuItem.HeaderProperty))
                {
                    menuItem.SetCurrentValue(MenuItem.HeaderProperty, item);
                }

                if (item is IMenuItemData menuItemData)
                {
                    if (!menuItem.IsSet(MenuItem.IconProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.IconProperty, menuItemData.Icon);
                    }

                    if (menuItem.ItemKey == null)
                    {
                        menuItem.ItemKey = menuItemData.ItemKey;
                    }

                    if (!menuItem.IsSet(MenuItem.IsEnabledProperty))
                    {
                        menuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }

                    if (!menuItem.IsSet(MenuItem.InputGestureProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.InputGestureProperty, menuItemData.InputGesture);
                    }
                }
            }
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is not MenuSeparator)
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        MenuPinnedOpenScope.ContainersChanged(Parent);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Find<ContentPresenter>("GroupTitlePresenter")?
         .Classes.Add(DropdownButtonSemanticParts.ItemTitleClass);
    }

    // Explicit containers do not always receive a generator clear callback.
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty && Parent is null)
        {
            SemanticLevel = MenuSemanticLevel.None;
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MenuItem item)
        {
            item.SemanticLevel = MenuSemanticLevel.None;
        }
        base.ClearContainerForItemOverride(container);
    }

    private void ApplyChildSemanticLevel(MenuItem menuItem)
    {
        menuItem.SemanticLevel = SemanticLevel;
    }
}
