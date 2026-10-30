using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 菜单项容器在 Semantic Part 契约中的语义层级。plain <see cref="Menu"/> 的顶层项与子菜单项使用互斥的
/// marker 类，因此这个层级必须在容器 prepare 时由 owner 明确下发，而不是靠容器自身推断：同一个
/// <see cref="MenuItem"/> 既可能出现在菜单栏第一层，也可能出现在任意深度的子菜单里，还可能被
/// ContextMenu、MenuFlyout、DropdownButton 的弹层复用。
/// </summary>
internal enum MenuSemanticLevel
{
    /// <summary>
    /// 未纳入 plain Menu 语义作用域。MenuFlyout / ContextMenu / DropdownButton 弹层等复用方保持既有
    /// <c>.semantic-item</c> 行为，不参与 plain Menu 的层级隔离。
    /// </summary>
    None = 0,

    /// <summary>plain Menu 菜单栏第一层。</summary>
    TopLevel = 1,

    /// <summary>plain Menu 子菜单内部（含任意深度嵌套与子菜单内分组）。</summary>
    SubMenu = 2
}

/// <summary>
/// 把 <see cref="MenuSemanticLevel"/> 落到容器 marker 类上。层级互斥：写入当前层级前先移除另一层级的类，
/// 保证容器在层级之间转移或被回收复用时既不残留旧层级，也不重复添加。
/// </summary>
internal static class MenuSemanticLevelScope
{
    // 分组是 route 的中间跳点，不是公开 Part，因此没有生成的常量（与 NavMenu 分组跳点同法）。
    private const string TopLevelGroupClass = "semantic-scope-group";
    private const string SubMenuGroupClass  = "semantic-sub-menu-group";

    /// <summary>
    /// 应用菜单项容器的互斥 marker；None 恢复复用方的默认 semantic-item 语义。
    /// </summary>
    public static void ApplyItemLevel(Control container, MenuSemanticLevel level)
    {
        switch (level)
        {
            case MenuSemanticLevel.None:
            case MenuSemanticLevel.TopLevel:
                Apply(container, MenuSemanticParts.ItemClass, MenuSemanticParts.SubMenuItemClass);
                break;

            case MenuSemanticLevel.SubMenu:
                Apply(container, MenuSemanticParts.SubMenuItemClass, MenuSemanticParts.ItemClass);
                break;
        }
    }

    /// <summary>应用分组层级；None 释放 plain Menu 的两种分组 marker。</summary>
    public static void ApplyGroupLevel(Control container, MenuSemanticLevel level)
    {
        switch (level)
        {
            case MenuSemanticLevel.None:
                container.Classes.Remove(TopLevelGroupClass);
                container.Classes.Remove(SubMenuGroupClass);
                break;
            case MenuSemanticLevel.TopLevel:
                Apply(container, TopLevelGroupClass, SubMenuGroupClass);
                break;

            case MenuSemanticLevel.SubMenu:
                Apply(container, SubMenuGroupClass, TopLevelGroupClass);
                break;
        }
    }

    public static void ApplyChildrenLevel(ItemsControl owner, MenuSemanticLevel level)
    {
        foreach (var container in owner.GetRealizedContainers())
        {
            switch (container)
            {
                case MenuItem item:
                    item.SemanticLevel = level;
                    break;
                case MenuItemGroup group:
                    group.SemanticLevel = level;
                    break;
            }
        }
    }

    private static void Apply(Control container, string keep, string drop)
    {
        if (container.Classes.Contains(drop))
        {
            container.Classes.Remove(drop);
        }

        if (!container.Classes.Contains(keep))
        {
            container.Classes.Add(keep);
        }
    }
}
