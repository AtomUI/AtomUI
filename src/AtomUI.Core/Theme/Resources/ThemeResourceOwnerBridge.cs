using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// 字典的局部资源宿主。装配期间仅刷新自身绑定，资源根由主题提交统一通知。
/// owner 订阅在解绑时对称释放，普通运行时字典变更仍转发到资源根。
/// </summary>
internal sealed class ThemeResourceOwnerBridge : IThemeVariantHost
{
    private IResourceHost? _owner;
    private int _updateDepth;
    private bool _resourcesChanged;

    public bool HasResources => _owner?.HasResources == true;
    public ThemeVariant ActualThemeVariant => (_owner as IThemeVariantHost)?.ActualThemeVariant ?? ThemeVariant.Default;
    public event EventHandler<ResourcesChangedEventArgs>? ResourcesChanged;
    public event EventHandler? ActualThemeVariantChanged;

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        // 对齐真实 owner 的查找入口：先回到宿主，保持应用覆盖优先于静态字典。
        // 此处不能先查询内层/兄弟字典，否则 DynamicResource 会绕过应用覆盖。
        if (_owner is not null)
        {
            return _owner.TryFindResource(key, theme, out value);
        }
        value = null;
        return false;
    }

    internal void SetOwner(IResourceHost? owner)
    {
        if (ReferenceEquals(owner, _owner))
        {
            return;
        }
        var previous = _owner;
        _owner = null;
        var failures = new List<Exception>();
        Unsubscribe(previous, failures);
        ThrowFailures(failures);
        _owner = owner;
        try
        {
            if (owner is not null)
            {
                owner.ResourcesChanged += HandleResourcesChanged;
                if (owner is IThemeVariantHost nextVariant)
                {
                    nextVariant.ActualThemeVariantChanged += HandleThemeVariantChanged;
                }
            }
            HandleThemeVariantChanged(this, EventArgs.Empty);
            NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
        }
        catch (Exception exception)
        {
            failures.Add(exception);
            _owner = null;
            Unsubscribe(owner, failures);
            ThrowFailures(failures);
        }
    }

    private void Unsubscribe(IResourceHost? owner, ICollection<Exception> failures)
    {
        if (owner is not null)
        {
            TryCleanup(() => owner.ResourcesChanged -= HandleResourcesChanged, failures);
            if (owner is IThemeVariantHost variant)
            {
                TryCleanup(() => variant.ActualThemeVariantChanged -= HandleThemeVariantChanged, failures);
            }
        }
    }

    internal void RunUpdate(Action update)
    {
        var failures = new List<Exception>();
        BeginUpdate();
        try
        {
            TryCleanup(update, failures);
        }
        finally
        {
            // EndUpdate 也会调用资源绑定，不能用其异常覆盖原始挂载/释放失败。
            TryCleanup(EndUpdate, failures);
        }
        ThrowFailures(failures);
    }

    internal static void TryCleanup(Action cleanup, ICollection<Exception> failures)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }

    internal static void ThrowFailures(IReadOnlyList<Exception> failures)
    {
        if (failures.Count == 1)
        {
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }
        if (failures.Count > 1)
        {
            throw new AggregateException("Theme resource ownership operation failed.", failures);
        }
    }

    private void BeginUpdate() => _updateDepth++;

    private void EndUpdate()
    {
        if (--_updateDepth == 0 && _resourcesChanged)
        {
            _resourcesChanged = false;
            ResourcesChanged?.Invoke(this, ResourcesChangedEventArgs.Create());
        }
    }

    public void NotifyHostedResourcesChanged(ResourcesChangedEventArgs e)
    {
        if (_updateDepth > 0)
        {
            _resourcesChanged = true;
        }
        else if (_owner is not null)
        {
            _owner.NotifyHostedResourcesChanged(e);
        }
        else
        {
            ResourcesChanged?.Invoke(this, e);
        }
    }

    private void HandleResourcesChanged(object? sender, ResourcesChangedEventArgs e) => ResourcesChanged?.Invoke(this, e);
    private void HandleThemeVariantChanged(object? sender, EventArgs e) => ActualThemeVariantChanged?.Invoke(this, e);
}
