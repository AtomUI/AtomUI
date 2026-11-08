using ControlsConsumption = AtomUI.Generated.AtomUIControls.GeneratedThemeConsumption;
using ControlsOwnState = AtomUI.Generated.AtomUIControls.OwnTokenState;
using DesktopConsumption = AtomUI.Generated.AtomUIDesktopControls.GeneratedThemeConsumption;
using DesktopOwnState = AtomUI.Generated.AtomUIDesktopControls.OwnTokenState;
using ColorPickerConsumption = AtomUI.Generated.AtomUIDesktopControlsColorPicker.GeneratedThemeConsumption;
using ColorPickerOwnState = AtomUI.Generated.AtomUIDesktopControlsColorPicker.OwnTokenState;
using DataGridConsumption = AtomUI.Generated.AtomUIDesktopControlsDataGrid.GeneratedThemeConsumption;
using DataGridOwnState = AtomUI.Generated.AtomUIDesktopControlsDataGrid.OwnTokenState;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect;

/// <summary>
/// Theory data for Token effect tests, read from the generator's per-package consumption map: the Control's
/// Active Own Tokens plus every global Token it consumes (AXAML and code, deduplicated).
/// </summary>
public static class EffectTestData
{
    /// <summary>Consumption-map Control id for primitives without a Token identity.</summary>
    internal const string SharedPrimitiveId = "shared-primitive";

    private static readonly Lazy<OwnTokenRow[]> s_ownTokens = new(LoadOwnTokens);
    private static readonly Lazy<GlobalConsumerRow[]> s_globalConsumers = new(LoadGlobalConsumers);

    /// <summary>
    /// Returns one <c>{ string token, bool isOwn }</c> row per Token in the Control's promise surface. Reserved and
    /// Unreferenced Own Tokens are excluded; <c>shared-primitive</c> entries are exercised through the owning
    /// Control's globals and are never returned.
    /// </summary>
    public static IEnumerable<object[]> For(string catalog, string controlId)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(controlId);
        if (!s_ownTokens.Value.Any(row => row.Catalog == catalog && row.ControlId == controlId) &&
            !s_globalConsumers.Value.Any(row => row.Catalog == catalog && row.ControlId == controlId))
        {
            throw new ArgumentException(
                $"Control '{catalog}:{controlId}' has no entries in any generated Token consumption map.",
                nameof(controlId));
        }

        return Select(s_ownTokens.Value, s_globalConsumers.Value, catalog, controlId)
               .Select(static token => new object[] { token.Token, token.IsOwn })
               .ToArray();
    }

    /// <summary>Selects the effect-test Tokens for one Control: Active Own Tokens first, then sorted globals.</summary>
    internal static IReadOnlyList<EffectToken> Select(
        IEnumerable<OwnTokenRow> ownTokens,
        IEnumerable<GlobalConsumerRow> globalConsumers,
        string catalog,
        string controlId)
    {
        if (controlId == SharedPrimitiveId)
        {
            throw new ArgumentException(
                "Primitive consumption is exercised through the owning Control's global Tokens.",
                nameof(controlId));
        }

        var seen   = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<EffectToken>();
        foreach (var row in ownTokens)
        {
            if (row.Catalog == catalog && row.ControlId == controlId &&
                row.Status == OwnTokenStatus.Active && seen.Add(row.Token))
            {
                result.Add(new EffectToken(row.Token, true));
            }
        }

        foreach (var token in globalConsumers
                              .Where(row => row.Catalog == catalog && row.ControlId == controlId)
                              .Select(static row => row.GlobalToken)
                              .Distinct(StringComparer.Ordinal)
                              .Order(StringComparer.Ordinal))
        {
            if (seen.Add(token))
            {
                result.Add(new EffectToken(token, false));
            }
        }

        return result;
    }

    private static OwnTokenRow[] LoadOwnTokens()
    {
        return DesktopConsumption.OwnTokens
                                 .Select(static e => new OwnTokenRow(e.Catalog, e.ControlId, e.Token, Map(e.State)))
                                 .Concat(ControlsConsumption.OwnTokens.Select(static e =>
                                     new OwnTokenRow(e.Catalog, e.ControlId, e.Token, Map(e.State))))
                                 .Concat(ColorPickerConsumption.OwnTokens.Select(static e =>
                                     new OwnTokenRow(e.Catalog, e.ControlId, e.Token, Map(e.State))))
                                 .Concat(DataGridConsumption.OwnTokens.Select(static e =>
                                     new OwnTokenRow(e.Catalog, e.ControlId, e.Token, Map(e.State))))
                                 .ToArray();
    }

    private static GlobalConsumerRow[] LoadGlobalConsumers()
    {
        return DesktopConsumption.GlobalConsumers
                                 .Select(static e => new GlobalConsumerRow(e.Catalog, e.ControlId, e.GlobalToken))
                                 .Concat(ControlsConsumption.GlobalConsumers.Select(static e =>
                                     new GlobalConsumerRow(e.Catalog, e.ControlId, e.GlobalToken)))
                                 .Concat(ColorPickerConsumption.GlobalConsumers.Select(static e =>
                                     new GlobalConsumerRow(e.Catalog, e.ControlId, e.GlobalToken)))
                                 .Concat(DataGridConsumption.GlobalConsumers.Select(static e =>
                                     new GlobalConsumerRow(e.Catalog, e.ControlId, e.GlobalToken)))
                                 .ToArray();
    }

    // Each package generates its own internal OwnTokenState; map them onto one test-side status.
    private static OwnTokenStatus Map(DesktopOwnState state) => Map(state.ToString());
    private static OwnTokenStatus Map(ControlsOwnState state) => Map(state.ToString());
    private static OwnTokenStatus Map(ColorPickerOwnState state) => Map(state.ToString());
    private static OwnTokenStatus Map(DataGridOwnState state) => Map(state.ToString());

    private static OwnTokenStatus Map(string state) => Enum.Parse<OwnTokenStatus>(state);

    internal enum OwnTokenStatus
    {
        Active,
        Reserved,
        Unreferenced
    }

    internal readonly record struct OwnTokenRow(string Catalog, string ControlId, string Token, OwnTokenStatus Status);

    internal readonly record struct GlobalConsumerRow(string Catalog, string ControlId, string GlobalToken);

    internal readonly record struct EffectToken(string Token, bool IsOwn);
}
