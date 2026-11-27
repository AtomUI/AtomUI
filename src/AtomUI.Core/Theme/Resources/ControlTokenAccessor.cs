using AtomUI.Theme.Derivation;
using AtomUI.Theme.Compilation;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Provides an atomic, snapshot-backed view of the tokens available to one registered Control instance. All
/// reads of one accessor use the snapshot that was captured with it, so a theme change is seen only after
/// capturing again.
/// </summary>
/// <remarks>
/// An accessor that was not returned by <see cref="Capture(Control)" /> or
/// <see cref="Capture{TControl}(Control)" /> throws an <see cref="InvalidOperationException" /> on every read.
/// </remarks>
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

    /// <summary>
    /// Captures the theme snapshot and the token slot of the runtime type of <paramref name="owner" />.
    /// </summary>
    /// <param name="owner">
    /// The control whose tokens are read later.
    /// </param>
    /// <returns>
    /// An accessor whose reads come from the snapshot taken at the call.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The runtime type of <paramref name="owner" /> is not registered in the captured theme schema.
    /// </exception>
    public static ControlTokenAccessor Capture(Control owner)
    {
        var captured = s_resolver.CaptureControl(owner);
        return new ControlTokenAccessor(captured.Snapshot, captured.ControlSlot);
    }

    /// <summary>
    /// Captures the theme snapshot and the token slot of <typeparamref name="TControl" />, which lets the tokens
    /// of a base control be read through an instance of a derived one.
    /// </summary>
    /// <typeparam name="TControl">
    /// The registered control type whose token slot is captured.
    /// </typeparam>
    /// <param name="owner">
    /// The control that supplies the theme snapshot; its runtime type does not select the token slot.
    /// </param>
    /// <returns>
    /// An accessor whose reads come from the snapshot taken at the call.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <typeparamref name="TControl" /> is not registered in the captured theme schema.
    /// </exception>
    public static ControlTokenAccessor Capture<TControl>(Control owner)
        where TControl : Control
    {
        var captured = s_resolver.CaptureControl(owner, typeof(TControl));
        return new ControlTokenAccessor(captured.Snapshot, captured.ControlSlot);
    }

    /// <summary>
    /// Reads the language-wide value of a global Token from the captured snapshot.
    /// </summary>
    /// <typeparam name="T">
    /// The type the Token value is returned as.
    /// </typeparam>
    /// <param name="token">
    /// The global Token to read.
    /// </param>
    /// <returns>
    /// The value of the Token in the captured snapshot.
    /// </returns>
    public T GetGlobal<T>(SharedTokenKind token)
    {
        return s_resolver.GetGlobal<T>(GetSnapshot(), (int)token);
    }

    /// <summary>
    /// Reads the value a global Token has for the captured control: the value the control provides for that
    /// Token when it has one, otherwise the language-wide value of <see cref="GetGlobal{T}(SharedTokenKind)" />.
    /// </summary>
    /// <typeparam name="T">
    /// The type the Token value is returned as.
    /// </typeparam>
    /// <param name="token">
    /// The global Token to read.
    /// </param>
    /// <returns>
    /// The value of the Token in the captured snapshot.
    /// </returns>
    public T GetEffectiveGlobal<T>(SharedTokenKind token)
    {
        return s_resolver.GetEffectiveGlobal<T>(GetSnapshot(), _controlSlot, (int)token);
    }

    /// <summary>
    /// Reads an Own Token of the captured control by the enum member that names it.
    /// </summary>
    /// <typeparam name="T">
    /// The type the Token value is returned as.
    /// </typeparam>
    /// <typeparam name="TTokenKind">
    /// The enum type that the Own Tokens of the control are keyed by.
    /// </typeparam>
    /// <param name="token">
    /// A member of the Token kind enum of the captured control.
    /// </param>
    /// <returns>
    /// The value of the Token in the captured snapshot.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="token" /> does not name an Own Token of the captured control.
    /// </exception>
    public T GetOwn<T, TTokenKind>(TTokenKind token)
        where TTokenKind : Enum
    {
        return s_resolver.GetOwn<T>(GetSnapshot(), _controlSlot, token);
    }

    /// <summary>
    /// Reads an Own Token of the captured control by a resource key that is not known at compile time.
    /// </summary>
    /// <typeparam name="T">
    /// The type the Token value is returned as.
    /// </typeparam>
    /// <param name="token">
    /// The resource key of the Own Token, for example one chosen at run time.
    /// </param>
    /// <returns>
    /// The value of the Token in the captured snapshot.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="token" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="token" /> does not name an Own Token of the captured control.
    /// </exception>
    public T GetOwn<T>(Enum token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return s_resolver.GetOwn<T>(GetSnapshot(), _controlSlot, token);
    }

    /// <summary>
    /// Reads the preset color palette of a primary color from the captured snapshot.
    /// </summary>
    /// <param name="primaryColor">
    /// The preset primary color whose palette is read.
    /// </param>
    /// <returns>
    /// The palette of <paramref name="primaryColor" /> as the snapshot holds it.
    /// </returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// <paramref name="primaryColor" /> has no palette in the captured snapshot.
    /// </exception>
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
