using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal sealed class ControlThemeSnapshot
{
    internal ControlThemeSnapshot(
        int controlSlot,
        ThemeAppearance appearance,
        IReadOnlyDictionary<int, object?> effectiveGlobalTokenDelta,
        IReadOnlyDictionary<object, object?> effectiveGlobalResourceDelta,
        TokenValueTable controlTokenValues,
        IReadOnlyDictionary<object, object?> controlResources,
        object? appearanceSettings = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        ArgumentNullException.ThrowIfNull(effectiveGlobalTokenDelta);
        ArgumentNullException.ThrowIfNull(effectiveGlobalResourceDelta);
        ArgumentNullException.ThrowIfNull(controlTokenValues);
        ArgumentNullException.ThrowIfNull(controlResources);

        ControlSlot                  = controlSlot;
        Appearance                   = appearance;
        EffectiveGlobalTokenDelta    = effectiveGlobalTokenDelta;
        EffectiveGlobalResourceDelta = effectiveGlobalResourceDelta;
        ControlTokenValues           = controlTokenValues;
        ControlResources             = controlResources;
        AppearanceSettings           = appearanceSettings;
        EstimatedRetainedBytes        = ThemeRetainedBytesEstimator.EstimateControl(this);
    }

    internal int ControlSlot { get; }
    internal ThemeAppearance Appearance { get; }
    internal IReadOnlyDictionary<int, object?> EffectiveGlobalTokenDelta { get; }
    internal IReadOnlyDictionary<object, object?> EffectiveGlobalResourceDelta { get; }
    internal TokenValueTable ControlTokenValues { get; }
    internal IReadOnlyDictionary<object, object?> ControlResources { get; }
    internal object? AppearanceSettings { get; }
    internal long EstimatedRetainedBytes { get; }

    internal T GetEffectiveGlobalValue<T>(TokenValueTable globalValues, int slot)
    {
        if (EffectiveGlobalTokenDelta.TryGetValue(slot, out var value))
        {
            if (value is T typed)
            {
                return (T)ThemeResourceValue.CloneForConsumer(typed)!;
            }

            if (value is null && default(T) is null)
            {
                return default!;
            }

            throw new InvalidCastException(
                $"Control global Token slot {slot} contains '{value?.GetType().FullName ?? "null"}', not '{typeof(T).FullName}'.");
        }

        return globalValues.Get<T>(slot);
    }

    internal bool TryGetSharedResource(
        object resourceKey,
        IReadOnlyDictionary<object, object?> globalResources,
        out object? value)
    {
        return EffectiveGlobalResourceDelta.TryGetValue(resourceKey, out value) ||
               globalResources.TryGetValue(resourceKey, out value);
    }
}
