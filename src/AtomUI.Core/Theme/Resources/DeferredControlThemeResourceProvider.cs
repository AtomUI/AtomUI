using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// 静态主题资源的惰性挂载代理：挂载期仅保存注册工厂，首次资源查找时才执行工厂构造真实字典。
/// 生成 wrapper 字典的构造即编译期 populate（会急切构建合并主题），惰性代理把这部分成本从
/// 启动路径移到实际消费点；合并列表中的位置、查找结果与急切挂载逐项等价。owner 不转发给
/// 真实字典（见 Load 注释：转发广播会经 DynamicResource 重估形成嵌套加载循环）。
/// 与 Avalonia 资源体系一致，本类型只在 UI 线程访问，不做并发保护。
/// </summary>
internal sealed class DeferredControlThemeResourceProvider : IResourceProvider
{
    private readonly string _assetId;
    private readonly Func<IResourceProvider> _factory;
    private IResourceProvider? _inner;
    private IResourceHost? _owner;
    private EventHandler? _pendingOwnerChanged;
    private bool _isLoading;

    internal DeferredControlThemeResourceProvider(string assetId, Func<IResourceProvider> factory)
    {
        _assetId = assetId;
        _factory = factory;
    }

    // 测试观测口：真实字典是否已被首次查找构造。
    internal bool IsLoaded => _inner is not null;

    public IResourceHost? Owner => _inner?.Owner ?? _owner;

    // Owner 附加路径（ResourceDictionary.OnAddOwner）会逐项探测 HasResources，此处加载会使挂载
    // 退化为急切；工厂契约保证真实字典一定提供资源，未加载时恒 true，最坏代价是多一次 TryGetResource。
    public bool HasResources => _inner?.HasResources ?? true;

    public event EventHandler? OwnerChanged
    {
        add
        {
            if (_inner is { } inner)
            {
                inner.OwnerChanged += value;
            }
            else
            {
                _pendingOwnerChanged += value;
            }
        }
        remove
        {
            if (_inner is { } inner)
            {
                inner.OwnerChanged -= value;
            }
            else
            {
                _pendingOwnerChanged -= value;
            }
        }
    }

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        // 构造期的资源查找可能沿合并列表重入本提供者（主题构建中的 StaticResource 解析等），
        // 重入按 miss 处理避免无限递归，对齐 Avalonia ResourceInclude._isLoading 语义。
        if (_isLoading)
        {
            value = null;
            return false;
        }
        return Load().TryGetResource(key, theme, out value);
    }

    void IResourceProvider.AddOwner(IResourceHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (_inner is { } inner)
        {
            inner.AddOwner(owner);
            return;
        }
        if (_owner is not null)
        {
            throw new InvalidOperationException("The resource provider already has a parent.");
        }
        _owner = owner;
    }

    void IResourceProvider.RemoveOwner(IResourceHost owner)
    {
        if (_inner is { } inner)
        {
            inner.RemoveOwner(owner);
            return;
        }
        if (_owner == owner)
        {
            _owner = null;
        }
    }

    private IResourceProvider Load()
    {
        if (_inner is { } loaded)
        {
            return loaded;
        }
        IResourceProvider created;
        _isLoading = true;
        try
        {
            created = _factory() ?? throw new InvalidOperationException(
                $"Theme resource factory '{_assetId}' returned null.");
        }
        finally
        {
            _isLoading = false;
        }
        // 不向真实字典转发挂载期 owner：ResourceDictionary.OnAddOwner 的 NotifyHostedResourcesChanged
        // 广播会触发存活控件的 DynamicResource 重估，沿合并列表重入扫描并逐个嵌套加载其余代理
        // （真实套件实测栈溢出，环路：Load→AddOwner→广播→DynamicResourceExpression→扫描→Load）。
        // 已知限制（终审 F3 裁定）：真实字典因此永远无 owner——主题资产内"字典作用域"的
        // {DynamicResource}（锚定 provider.Owner）将无法解析（Avalonia 记录警告并发布 UnsetValue），
        // 且内层 RaiseResourcesChanged 恒为 no-op。仓库内资产零命中（全部 DynamicResource 用法均为
        // setter/模板作用域，锚定目标控件）；生成资产构建后不可变，不触发通知路径。
        // 若未来出现命中场景，可行修复是 dispatcher 空闲时合并转发（需先解决每资产一次全树广播的风暴问题）。
        if (_pendingOwnerChanged is { } handlers)
        {
            foreach (var handler in handlers.GetInvocationList())
            {
                created.OwnerChanged += (EventHandler)handler;
            }
            _pendingOwnerChanged = null;
        }
        _inner = created;
        return created;
    }
}
