using AtomUI.Theme.Resources;
using Avalonia.Controls;

namespace AtomUI.Theme.Schema;

/// <summary>On-demand metadata and formatted snapshot values for editors; never instantiates controls.</summary>
public static class ThemeTokenInspection
{
    public static IReadOnlyList<TokenDescriptor> GlobalTokens(IThemeManager runtime) =>
        Array.AsReadOnly(Manager(runtime).CurrentSnapshot!.Registry.GlobalTokens.ToArray());

    public static IReadOnlyList<ControlTokenDescriptor> Controls(IThemeManager runtime) =>
        Array.AsReadOnly(Manager(runtime).CurrentSnapshot!.Registry.Controls.ToArray());

    public static IReadOnlyList<ThemeTokenInspectionValue> Read(Control scope, ControlTokenIdentity? identity = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        scope.VerifyAccess();
        var snapshot = new ThemeTokenResolver().Capture(scope);
        ControlTokenDescriptor? control = null;
        if (identity is { } owner)
        {
            snapshot.Registry.TryGetControl(owner, out control);
            if (control is null)
            {
                throw new ArgumentException("The selected control is not registered in this scope.", nameof(identity));
            }
        }
        var values = new List<ThemeTokenInspectionValue>();
        var selected = control is null ? null : snapshot.Controls[control.Slot];
        foreach (var token in snapshot.Registry.GlobalTokens)
        {
            var value = selected is not null && selected.EffectiveGlobalTokenDelta.TryGetValue(token.Slot, out var local)
                ? local : snapshot.GlobalTokenValues.GetValue(token.Slot);
            values.Add(new ThemeTokenInspectionValue(token.Name, token.ValueType, false, token.Format(value)));
        }
        if (control is not null)
        {
            foreach (var token in control.OwnTokens)
            {
                values.Add(new ThemeTokenInspectionValue(token.Name, token.ValueType, true,
                    token.Format(selected!.ControlTokenValues.GetValue(token.Slot))));
            }
        }
        return values.AsReadOnly();
    }

    private static ThemeManager Manager(IThemeManager runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        var manager = runtime as ThemeManager ??
            throw new NotSupportedException("This runtime does not expose registered token metadata.");
        manager.VerifyBrandReadAccess();
        return manager;
    }
}

public sealed record ThemeTokenInspectionValue(string Name, Type ValueType, bool IsOwn, string EffectiveValue);
