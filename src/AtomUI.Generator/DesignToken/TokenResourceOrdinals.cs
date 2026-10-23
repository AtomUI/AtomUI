using System.Globalization;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

// Public resource IDs are a build contract, independent of schema storage slots.
internal sealed class TokenResourceOrdinals
{
    private readonly Dictionary<string, Dictionary<string, int>> _scopes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _usedScopes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Location> _locations = new(StringComparer.Ordinal);
    private Location _defaultLocation = Location.None;
    private readonly Action<Diagnostic> _report;

    private TokenResourceOrdinals(Action<Diagnostic> report)
    {
        _report = report;
    }

    internal static TokenResourceOrdinals Read(IEnumerable<(string Path, string Text)> inputs,
        Action<Diagnostic> report, CancellationToken cancellationToken)
    {
        var result = new TokenResourceOrdinals(report);
        foreach (var input in inputs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (input.Text.Length > 4 * 1024 * 1024)
            {
                result.Error(input.Path + ": input exceeds 4194304 characters.");
                continue;
            }
            var text = SourceText.From(input.Text);
            Location AtLine(int number)
            {
                var index = Math.Min(number - 1, text.Lines.Count - 1);
                var position = new LinePosition(index, 0);
                return Location.Create(input.Path, new TextSpan(text.Lines[index].Start, 0), new LinePositionSpan(position, position));
            }
            result._defaultLocation = AtLine(1);
            using var reader = new StringReader(input.Text);
            if (reader.ReadLine() != "AtomUI.TokenResourceIds\t1")
            {
                result.Error(input.Path + ": expected AtomUI.TokenResourceIds version 1 header.", AtLine(1));
                continue;
            }
            string? line;
            var lineNumber = 1;
            var rows = 0;
            while ((line = reader.ReadLine()) is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lineNumber++;
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                if (++rows > 32768)
                {
                    result.Error(input.Path + ": input exceeds 32768 rows.", AtLine(lineNumber));
                    break;
                }
                var fields = line.Split('\t');
                if (fields.Length != 4 || fields.Take(3).Any(static field => field.Length == 0 || field.Length > 512) ||
                    !int.TryParse(fields[3], NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id < 0)
                {
                    result.Error(input.Path + ":" + lineNumber.ToString(CultureInfo.InvariantCulture) + ": expected assembly, scope, field and nonnegative ID.", AtLine(lineNumber));
                    continue;
                }
                var key = fields[0] + "|" + fields[1];
                if (!result._scopes.TryGetValue(key, out var scope))
                {
                    scope = new Dictionary<string, int>(StringComparer.Ordinal);
                    result._scopes.Add(key, scope);
                    result._locations.Add(key, AtLine(lineNumber));
                }
                if (scope.ContainsKey(fields[2]))
                {
                    result.Error(input.Path + ":" + lineNumber.ToString(CultureInfo.InvariantCulture) + ": duplicate field " + key + "." + fields[2] + ".", AtLine(lineNumber));
                    continue;
                }
                scope.Add(fields[2], id);
            }
            if (rows == 0)
            {
                result.Error(input.Path + ": an explicit contract must contain resource entries.", AtLine(1));
            }
        }
        foreach (var scope in result._scopes)
        {
            if (!scope.Value.Values.OrderBy(static id => id).SequenceEqual(Enumerable.Range(0, scope.Value.Count)))
            {
                result.Error(scope.Key + ": IDs must be unique and dense from 0; append IDs without changing existing values.", result._locations[scope.Key]);
            }
        }
        return result;
    }

    internal bool HasScope(string assembly, string scope) => _scopes.ContainsKey(assembly + "|" + scope);

    internal string[] Order(string assembly, string scope, IEnumerable<string> names)
    {
        var actual = names.OrderBy(static name => name, StringComparer.Ordinal).ToArray();
        var key = assembly + "|" + scope;
        _usedScopes.Add(key);
        if (!_scopes.TryGetValue(key, out var ids))
        {
            if (_scopes.Count != 0)
            {
                Error(key + ": register the new scope's public IDs in token-resource-ids.tsv.", _defaultLocation);
            }
            return actual;
        }
        var missing = actual.Where(name => !ids.ContainsKey(name)).ToArray();
        var removed = ids.Keys.Except(actual, StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal).ToArray();
        if (missing.Length != 0 || removed.Length != 0)
        {
            Error(key + ": append new fields with new IDs (" + string.Join(", ", missing) +
                  "); preserve existing fields (" + string.Join(", ", removed) + ").", _locations[key]);
            return actual;
        }
        return actual.OrderBy(name => ids[name]).ToArray();
    }

    internal void ValidateAssembly(string assembly)
    {
        foreach (var scope in _scopes.Keys)
        {
            if (scope.StartsWith(assembly + "|", StringComparison.Ordinal) && !_usedScopes.Contains(scope))
            {
                Error(scope + ": preserve the existing scope or review an explicit API migration.", _locations[scope]);
            }
        }
    }

    private void Error(string message, Location? location = null) => _report(Diagnostic.Create(
        AtomUIDiagnosticDescriptors.TokenResourceOrdinalContract, location ?? _defaultLocation, message));
}
