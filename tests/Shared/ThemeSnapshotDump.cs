using System.Globalization;
using System.Text;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.Testing;

/// <summary>
/// Dumps the complete compiled result of a theme snapshot (every global slot and every registered control's Own
/// slots) as sorted, comparable text lines for the design-language verification tool.
/// </summary>
/// <remarks>
/// Line formats: <c>G &lt;TokenName&gt; = &lt;value&gt;</c> for global slots and
/// <c>C &lt;Catalog&gt;:&lt;ControlId&gt; &lt;TokenName&gt; = &lt;value&gt;</c> for control Own slots. Values are
/// formatted with the token descriptor's <c>Format</c> (culture-invariant). All global lines come first, then all
/// control lines; within each group lines are ordered by their key (the text before <c> = </c>) ordinally.
/// Lazily compiled controls are forced while dumping. Gradient brushes are the one exception to <c>Format</c>: it
/// yields only their type name, so they are written with their geometry and stops instead.
/// </remarks>
internal static class ThemeSnapshotDump
{
    private const string Separator = " = ";

    internal static string Dump(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var registry = snapshot.Registry;

        var globals = new List<string>(registry.GlobalTokens.Count);
        foreach (var token in registry.GlobalTokens)
        {
            globals.Add($"G {token.Name}{Separator}{FormatValue(token, snapshot.GlobalTokenValues.GetValue(token.Slot))}");
        }

        var controls = new List<string>();
        foreach (var control in registry.Controls)
        {
            if (control.OwnTokens.Count == 0)
            {
                continue;
            }
            // Indexing the lazy control table compiles the control if it has not been compiled yet.
            var values = snapshot.Controls[control.Slot].ControlTokenValues;
            var prefix = $"C {control.Identity.Catalog}:{control.Identity.Id} ";
            foreach (var token in control.OwnTokens)
            {
                controls.Add($"{prefix}{token.Name}{Separator}{FormatValue(token, values.GetValue(token.Slot))}");
            }
        }

        globals.Sort(CompareByKey);
        controls.Sort(CompareByKey);
        var builder = new StringBuilder();
        foreach (var line in globals.Concat(controls))
        {
            builder.Append(line).Append('\n');
        }
        return builder.ToString();
    }

    private static string FormatValue(TokenDescriptor token, object? value)
    {
        if (value is not IGradientBrush gradient)
        {
            return token.Format(value);
        }
        var builder = new StringBuilder(gradient.GetType().Name);
        builder.Append(CultureInfo.InvariantCulture, $"(opacity={gradient.Opacity:R}; spread={gradient.SpreadMethod}");
        if (gradient is ILinearGradientBrush linear)
        {
            builder.Append("; start=").Append(FormatPoint(linear.StartPoint))
                   .Append("; end=").Append(FormatPoint(linear.EndPoint));
        }
        if (gradient.Transform is { } transform)
        {
            var m = transform.Value;
            builder.Append(CultureInfo.InvariantCulture,
                $"; transform={m.M11:R},{m.M12:R},{m.M13:R},{m.M21:R},{m.M22:R},{m.M23:R},{m.M31:R},{m.M32:R},{m.M33:R}");
        }
        else
        {
            builder.Append("; transform=none");
        }
        builder.Append("; stops=");
        for (var i = 0; i < gradient.GradientStops.Count; i++)
        {
            var stop = gradient.GradientStops[i];
            builder.Append(i == 0 ? "" : " ")
                   .Append(CultureInfo.InvariantCulture, $"{stop.Color}@{stop.Offset:R}");
        }
        return builder.Append(')').ToString();
    }

    private static string FormatPoint(RelativePoint point) => string.Create(CultureInfo.InvariantCulture,
        $"{point.Point.X:R},{point.Point.Y:R}{(point.Unit == RelativeUnit.Relative ? "%" : "px")}");

    private static int CompareByKey(string left, string right) =>
        string.CompareOrdinal(KeyOf(left), KeyOf(right));

    private static string KeyOf(string line)
    {
        var index = line.IndexOf(Separator, StringComparison.Ordinal);
        return index < 0 ? line : line[..index];
    }
}
