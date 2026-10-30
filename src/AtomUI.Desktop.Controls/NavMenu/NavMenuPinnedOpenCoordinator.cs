using System.Collections;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 拥有 NavMenu 钉住打开的请求状态，并把请求收敛到运行时生成的菜单项容器上。
///
/// 钉住请求以 <see cref="INavMenuNode"/> 为键，而不是以 <see cref="NavMenuItem"/> 容器为键。旧实现只把请求
/// 落在“当下已实现的容器”上：属性变更、attach、Mode 切换都发生在容器生成之前或容器重建之际，声明式写法
/// （AXAML 里 <c>IsPopupPinnedOpen="True"</c>）在模板应用和容器生成之前就写入属性，那时没有任何可钉住容器，
/// 请求当场丢失。把请求表达成节点路径后，它可以独立于容器生命周期存活，任何触发点只更新请求，
/// 实现后的容器由同一份请求按路径补齐。
///
/// 容器 prepare 通过 <see cref="PrepareContainer"/> 与选择状态走同一容器时序契约；属性变更、attach、Mode
/// 切换走 <see cref="Reconcile"/> 立即求值；条目图变化走 <see cref="QueueReconcile"/> 合并调度，在
/// ItemsPresenter 收敛后再求值（集合变更回调里容器与节点映射可能尚未收敛）。
/// </summary>
internal sealed class NavMenuPinnedOpenCoordinator
{
    // 需要保持打开的子菜单节点，root -> leaf 顺序。列表可能只包含节点而对应容器尚未生成。
    private readonly List<INavMenuNode> _pinnedPath = [];

    // 已经按 _pinnedPath 钉住的实现容器，用于在请求释放时精确回收，避免遗留钉住状态。
    private readonly List<NavMenuItem> _pinnedItems = [];

    private int _reconcileGeneration;
    private bool _isApplying;

    /// <summary>
    /// 把钉住请求收敛到当前菜单状态。请求关闭或处于 Inline 时释放；否则解析钉住路径并把钉住状态
    /// 应用到已经实现的容器。该方法是幂等的，可在属性变更、attach 与 Mode 切换时重复调用。
    /// </summary>
    public void Reconcile(NavMenu menu)
    {
        if (!menu.IsPopupPinnedOpen || menu.EffectiveMode == NavMenuMode.Inline)
        {
            Release();
            return;
        }

        PruneStalePath(menu);
        if (_pinnedPath.Count == 0)
        {
            _pinnedPath.AddRange(ResolvePath(menu));
        }

        ApplyToRealizedContainers(menu);
    }

    /// <summary>
    /// 合并调度一次收敛。用于条目集合变更：此刻容器与节点映射可能尚未收敛，立即求值会读到错误的
    /// 容器归属。请求状态是持久的，因此延后求值不会丢请求；连续变化只在 Loaded 优先级收敛一次。
    /// </summary>
    public void QueueReconcile(NavMenu menu)
    {
        var generation = ++_reconcileGeneration;
        Dispatcher.UIThread.Post(
            () =>
            {
                if (generation == _reconcileGeneration)
                {
                    Reconcile(menu);
                }
            },
            DispatcherPriority.Loaded);
    }

    /// <summary>
    /// 容器 prepare 阶段的不变量：容器对应的节点位于钉住路径上时必须立即带上钉住状态。
    /// 请求在容器存在之前就已确定，容器实现时按请求补齐，而不是等某个全局重扫恰好晚于容器生成。
    /// 请求以节点为键持久保存，因此容器在 Mode 切换、条目重排后重建时同样在这里重新补齐。
    /// <see cref="NavMenuItem"/> 会在自身 attach 时按钉住状态打开子菜单，这里不必考虑容器是否已 attach。
    /// </summary>
    public void PrepareContainer(NavMenu menu, NavMenuItem menuItem)
    {
        if (!menu.IsPopupPinnedOpen || ((INavMenuItem)menuItem).Node is not { } node)
        {
            return;
        }

        if (_pinnedPath.Count == 0)
        {
            Reconcile(menu);
        }

        if (_pinnedPath.Contains(node))
        {
            ApplyPinnedState(menuItem);
        }
    }

    /// <summary>
    /// 展开某个子菜单后，把钉住路径扩展为“该节点及其祖先”的当前链，并释放不再位于该链上的旧钉住项。
    /// 只有真实子菜单的展开才扩展路径：没有子项的 <see cref="NavMenuItem"/> 同样会派发
    /// <c>SubmenuOpened</c>，但它不代表任何可钉住的子菜单，不能覆盖已有钉住路径。
    /// </summary>
    public void ExtendToOpenedItem(NavMenu menu, NavMenuItem openedItem)
    {
        if (!openedItem.HasSubMenu ||
            ((INavMenuItem)openedItem).Node is not { } node ||
            !IsNodeInMenu(menu, node))
        {
            return;
        }

        _pinnedPath.Clear();
        _pinnedPath.AddRange(BuildPathToNode(node));

        // 钉住本身会打开子菜单，打开又同步派发 SubmenuOpened 回到这里；正在应用时只更新请求，
        // 由外层应用循环用最新路径完成下发，避免递归重入破坏应用中的容器集合。
        if (!_isApplying)
        {
            ApplyToRealizedContainers(menu, releaseObsolete: true);
        }
    }

    /// <summary>
    /// 容器被回收或移除时摘掉它的钉住记录。请求本身以节点为键保存在 <c>_pinnedPath</c> 中，
    /// 不随容器回收消失：容器属性由 <c>ClearNodeContainerBindings</c> 在回收时清空，条目图收敛后
    /// 再由 <see cref="Reconcile"/> 按路径补回。仅当容器对应的节点已经离开菜单条目图
    /// （真正被移除，而不是回收复用）时才做生命周期关闭。
    /// </summary>
    public void ForgetContainer(NavMenu menu, NavMenuItem menuItem)
    {
        var wasPinned = false;
        for (var i = _pinnedItems.Count - 1; i >= 0; i--)
        {
            if (IsSameOrDescendant(_pinnedItems[i], menuItem))
            {
                _pinnedItems.RemoveAt(i);
                wasPinned = true;
            }
        }

        if (wasPinned &&
            (((INavMenuItem)menuItem).Node is not { } node || !IsNodeInMenu(menu, node)))
        {
            menuItem.CloseForLifecycle();
        }
    }

    /// <summary>
    /// 释放所有容器上的钉住状态并清空请求路径，但不关闭已经打开的交互状态。
    /// 用于取消钉住、切到 Inline 和 detach：<c>IsPopupPinnedOpen=false</c> 只解除关闭拦截，
    /// 已打开的子菜单保持打开，交由调用方按生命周期决定是否关闭。
    /// </summary>
    public void Release()
    {
        _reconcileGeneration++;
        foreach (var item in _pinnedItems)
        {
            item.IsPopupPinnedOpen = false;
        }

        _pinnedItems.Clear();
        _pinnedPath.Clear();
    }

    private void ApplyToRealizedContainers(NavMenu menu, bool releaseObsolete = false)
    {
        if (_isApplying)
        {
            return;
        }

        _isApplying = true;
        try
        {
            var desired = new List<NavMenuItem>(_pinnedPath.Count);
            foreach (var node in _pinnedPath)
            {
                if (IsNodeInMenu(menu, node) &&
                    menu.FindRealizedMenuItem(node) is { } item &&
                    !desired.Contains(item))
                {
                    desired.Add(item);
                }
            }

            if (releaseObsolete)
            {
                for (var i = _pinnedItems.Count - 1; i >= 0; i--)
                {
                    var item = _pinnedItems[i];
                    if (!desired.Contains(item))
                    {
                        item.IsPopupPinnedOpen = false;
                        _pinnedItems.RemoveAt(i);
                    }
                }
            }

            foreach (var item in desired)
            {
                ApplyPinnedState(item);
            }
        }
        finally
        {
            _isApplying = false;
        }
    }

    private void ApplyPinnedState(NavMenuItem menuItem)
    {
        if (!menuItem.IsPopupPinnedOpen)
        {
            menuItem.IsPopupPinnedOpen = true;
        }

        if (!_pinnedItems.Contains(menuItem))
        {
            _pinnedItems.Add(menuItem);
        }
    }

    private void PruneStalePath(NavMenu menu)
    {
        for (var i = _pinnedPath.Count - 1; i >= 0; i--)
        {
            if (!IsNodeInMenu(menu, _pinnedPath[i]))
            {
                _pinnedPath.RemoveAt(i);
            }
        }
    }

    private static List<INavMenuNode> ResolvePath(NavMenu menu)
    {
        // 已展开链优先：钉住时若已有展开的子菜单，语义是钉住当前展开链，而不是强行改钉第一项。
        var openPath = ResolveOpenPath(menu);
        if (openPath.Count > 0)
        {
            return openPath;
        }

        foreach (var node in EnumerateNodes(menu.Items))
        {
            if (node.Children.Any())
            {
                return BuildPathToNode(node);
            }
        }

        return [];
    }

    private static List<INavMenuNode> ResolveOpenPath(NavMenu menu)
    {
        foreach (var topLevelItem in NavMenuSemanticNavigator.EnumerateDirectItems(menu))
        {
            if (((INavMenuItem)topLevelItem).Node is not { } node || !IsNodeInMenu(menu, node))
            {
                continue;
            }

            if (FindDeepestOpenSubmenu(topLevelItem) is { } openItem &&
                ((INavMenuItem)openItem).Node is { } openNode)
            {
                return BuildPathToNode(openNode);
            }
        }

        return [];
    }

    private static NavMenuItem? FindDeepestOpenSubmenu(NavMenuItem menuItem)
    {
        if (!menuItem.HasSubMenu || !menuItem.IsSubMenuOpen)
        {
            return null;
        }

        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(menuItem))
        {
            if (FindDeepestOpenSubmenu(child) is { } deeper)
            {
                return deeper;
            }
        }

        return menuItem;
    }

    private static List<INavMenuNode> BuildPathToNode(INavMenuNode node)
    {
        var path = new List<INavMenuNode>();
        for (var current = node; current is not null; current = current.ParentNode as INavMenuNode)
        {
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static bool IsNodeInMenu(NavMenu menu, INavMenuNode node)
    {
        var root = node;
        while (root.ParentNode is INavMenuNode parent)
        {
            root = parent;
        }

        return NavMenuEntryGraph.ContainsDirectNode(menu.Items, root);
    }

    private static IEnumerable<INavMenuNode> EnumerateNodes(IEnumerable entries)
    {
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case INavMenuNode node:
                    yield return node;
                    break;

                case NavMenuGroup group:
                    foreach (var nested in EnumerateNodes(group.Entries))
                    {
                        yield return nested;
                    }

                    break;
            }
        }
    }

    private static bool IsSameOrDescendant(NavMenuItem candidate, NavMenuItem ancestor)
    {
        for (var current = candidate; current is not null; current = current.SemanticParentItem)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}
