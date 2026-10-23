using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Provides an atomic, snapshot-backed view of the tokens available to one
/// registered Control instance.
/// </summary>
public readonly struct ControlTokenAccessor
{
    private static readonly ThemeTokenResolver s_resolver = new();
    private readonly ThemeSnapshot? _snapshot;
    private readonly int _controlSlot;

    private ControlTokenAccessor(ThemeSnapshot snapshot, int controlSlot)
    {
        _snapshot = snapshot;
        _controlSlot = controlSlot;
    }

    public static ControlTokenAccessor Capture(Control owner)
    {
        var captured = s_resolver.CaptureControl(owner);
        return new ControlTokenAccessor(captured.Snapshot, captured.ControlSlot);
    }

    public static ControlTokenAccessor Capture<TControl>(Control owner)
        where TControl : Control
    {
        var captured = s_resolver.CaptureControl(owner, typeof(TControl));
        return new ControlTokenAccessor(captured.Snapshot, captured.ControlSlot);
    }

    public T GetGlobal<T>(SharedTokenKind token)
    {
        var snapshot = GetSnapshot();
        return s_resolver.GetGlobal<T>(snapshot, snapshot.Registry.GetSharedTokenSlot(token));
    }

    public T GetEffectiveGlobal<T>(SharedTokenKind token)
    {
        var snapshot = GetSnapshot();
        return s_resolver.GetEffectiveGlobal<T>(snapshot, _controlSlot, snapshot.Registry.GetSharedTokenSlot(token));
    }

    public T GetOwn<T, TTokenKind>(TTokenKind token)
        where TTokenKind : Enum
    {
        return s_resolver.GetOwn<T>(GetSnapshot(), _controlSlot, token);
    }

    public T GetOwn<T>(Enum token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return s_resolver.GetOwn<T>(GetSnapshot(), _controlSlot, token);
    }

    public PaletteInfo GetPresetPalette(PresetPrimaryColor primaryColor)
    {
        return s_resolver.GetPresetPalette(GetSnapshot(), primaryColor);
    }

    private ThemeSnapshot GetSnapshot()
    {
        return _snapshot ?? throw new InvalidOperationException(
            "The ControlTokenAccessor was not captured from a Control.");
    }
}
