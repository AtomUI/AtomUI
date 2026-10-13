using System.ComponentModel;
using System.Collections.Frozen;
using Avalonia.Controls;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum ControlResourcePhase
{
    Shared = 0,
    Control = 1
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlThemeResourceRegistration
{
    public ControlThemeResourceRegistration(string assetId, ControlResourcePhase phase, int order, Func<IResourceProvider> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
        ArgumentNullException.ThrowIfNull(factory);
        if (phase is not (ControlResourcePhase.Shared or ControlResourcePhase.Control))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }
        AssetId = assetId;
        Phase = phase;
        Order = order;
        Factory = factory;
    }

    public string AssetId { get; }
    public ControlResourcePhase Phase { get; }
    public int Order { get; }
    public Func<IResourceProvider> Factory { get; }

    // null 表示开放资源集合，不能据此排除任何资源 key。
    internal IReadOnlySet<object>? ResourceKeys { get; private init; }

    /// <summary>
    /// 创建具有完整生成式键证据的静态资源注册。键集合须覆盖所有 variant/include 的顶层键；
    /// 工厂可能动态增添未知键时必须使用普通构造函数。集合防御性复制，旧注册入口保持开放查找。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlThemeResourceRegistration CreateGenerated(
        string assetId, ControlResourcePhase phase, int order,
        Func<IResourceProvider> factory, IReadOnlyList<object> resourceKeys)
    {
        ArgumentNullException.ThrowIfNull(resourceKeys);
        if (resourceKeys.Any(static key => key is null))
        {
            throw new ArgumentException("Generated resource keys cannot contain null.", nameof(resourceKeys));
        }
        return new ControlThemeResourceRegistration(assetId, phase, order, factory)
        {
            ResourceKeys = resourceKeys.ToFrozenSet()
        };
    }

    internal bool HasSameMetadata(ControlThemeResourceRegistration other) =>
        AssetId == other.AssetId && Phase == other.Phase && Order == other.Order && Factory.Equals(other.Factory) &&
        (ResourceKeys is null ? other.ResourceKeys is null : other.ResourceKeys is not null && ResourceKeys.SetEquals(other.ResourceKeys));
}
