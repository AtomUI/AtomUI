namespace AtomUI.Build.Tasks.LocalizationBuild;

internal enum LanguageFileValidationDiagnosticKind
{
    CatalogMismatch,
    InvalidTranslation,
    InvalidConfiguration
}

internal readonly struct LanguageFileValidationDiagnostic
{
    internal LanguageFileValidationDiagnostic(
        LanguageFileValidationDiagnosticKind kind,
        string message,
        int line,
        int column,
        string? unitKey = null)
    {
        Kind = kind;
        Message = message;
        Line = line;
        Column = column;
        UnitKey = unitKey;
    }

    internal LanguageFileValidationDiagnosticKind Kind { get; }

    internal string Message { get; }

    internal int Line { get; }

    internal int Column { get; }

    internal string? UnitKey { get; }
}

internal sealed class LanguageFileValidationOptions
{
    internal LanguageFileValidationOptions(bool requireCompleteBundle, string minimumTargetState)
    {
        RequireCompleteBundle = requireCompleteBundle;
        MinimumTargetState = minimumTargetState;
    }

    internal bool RequireCompleteBundle { get; }

    internal string MinimumTargetState { get; }
}

internal static class LanguageFileValidation
{
    internal static IReadOnlyList<LanguageFileValidationDiagnostic> ValidateTargetContent(
        XliffDocumentModel target,
        LanguageFileValidationOptions options)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var diagnostics = new List<LanguageFileValidationDiagnostic>();
        if (!ValidateOptions(options, diagnostics))
        {
            return diagnostics;
        }

        foreach (var targetUnit in target.File.Units
                                         .Where(static unit => !unit.IsObsolete)
                                         .OrderBy(static unit => unit.Key, StringComparer.Ordinal))
        {
            AddTargetContentDiagnostics(targetUnit, options, diagnostics);
        }

        return Sort(diagnostics);
    }

    internal static IReadOnlyList<LanguageFileValidationDiagnostic> ValidateTarget(
        XliffDocumentModel source,
        XliffDocumentModel target,
        LanguageFileValidationOptions options)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var diagnostics = new List<LanguageFileValidationDiagnostic>();
        if (!ValidateOptions(options, diagnostics))
        {
            return diagnostics;
        }

        var sourceUnits = source.File.Units
                                .Where(static unit => !unit.IsObsolete)
                                .ToDictionary(static unit => unit.Key, StringComparer.Ordinal);
        var targetUnits = target.File.Units
                                .Where(static unit => !unit.IsObsolete)
                                .ToDictionary(static unit => unit.Key, StringComparer.Ordinal);

        if (!string.Equals(source.File.Id, target.File.Id, StringComparison.Ordinal))
        {
            diagnostics.Add(new LanguageFileValidationDiagnostic(
                LanguageFileValidationDiagnosticKind.CatalogMismatch,
                $"Target file id '{target.File.Id}' does not match source file id '{source.File.Id}'.",
                1,
                1));
        }

        foreach (var targetUnit in targetUnits.Values.OrderBy(static unit => unit.Key, StringComparer.Ordinal))
        {
            if (!sourceUnits.TryGetValue(targetUnit.Key, out var sourceUnit))
            {
                diagnostics.Add(new LanguageFileValidationDiagnostic(
                    LanguageFileValidationDiagnosticKind.CatalogMismatch,
                    $"Target unit '{targetUnit.Key}' does not exist in the source catalog.",
                    targetUnit.Line,
                    targetUnit.Column,
                    targetUnit.Key));
                continue;
            }

            if (!string.Equals(sourceUnit.Source, targetUnit.Source, StringComparison.Ordinal))
            {
                diagnostics.Add(new LanguageFileValidationDiagnostic(
                    LanguageFileValidationDiagnosticKind.CatalogMismatch,
                    $"Target unit '{targetUnit.Key}' contains stale source text.",
                    targetUnit.Line,
                    targetUnit.Column,
                    targetUnit.Key));
            }

            if (!sourceUnit.PlaceholderIndexes.SequenceEqual(targetUnit.PlaceholderIndexes))
            {
                diagnostics.Add(new LanguageFileValidationDiagnostic(
                    LanguageFileValidationDiagnosticKind.InvalidTranslation,
                    $"Target unit '{targetUnit.Key}' changes the placeholder contract.",
                    targetUnit.Line,
                    targetUnit.Column,
                    targetUnit.Key));
            }

            AddTargetContentDiagnostics(targetUnit, options, diagnostics);
        }

        if (options.RequireCompleteBundle)
        {
            foreach (var sourceUnit in sourceUnits.Values.OrderBy(static unit => unit.Key, StringComparer.Ordinal))
            {
                if (!targetUnits.ContainsKey(sourceUnit.Key))
                {
                    diagnostics.Add(new LanguageFileValidationDiagnostic(
                        LanguageFileValidationDiagnosticKind.InvalidTranslation,
                        $"Target catalog is missing unit '{sourceUnit.Key}'.",
                        sourceUnit.Line,
                        sourceUnit.Column,
                        sourceUnit.Key));
                }
            }
        }

        return Sort(diagnostics);
    }

    private static bool ValidateOptions(
        LanguageFileValidationOptions options,
        List<LanguageFileValidationDiagnostic> diagnostics)
    {
        if (XliffTranslationTarget.TryGetStateRank(options.MinimumTargetState, out _))
        {
            return true;
        }

        diagnostics.Add(new LanguageFileValidationDiagnostic(
            LanguageFileValidationDiagnosticKind.InvalidConfiguration,
            "MinimumTargetState must be translated, reviewed, or final.",
            1,
            1));
        return false;
    }

    private static void AddTargetContentDiagnostics(
        XliffUnitModel targetUnit,
        LanguageFileValidationOptions options,
        List<LanguageFileValidationDiagnostic> diagnostics)
    {
        if (!XliffTranslationTarget.IsPublishable(targetUnit))
        {
            diagnostics.Add(new LanguageFileValidationDiagnostic(
                LanguageFileValidationDiagnosticKind.InvalidTranslation,
                $"Target unit '{targetUnit.Key}' must contain a target that is publishable: translated, " +
                "reviewed, or final state without needs-review; an empty target is valid only when the source is empty.",
                targetUnit.Line,
                targetUnit.Column,
                targetUnit.Key));
        }
        else if (!XliffTranslationTarget.MeetsMinimumState(targetUnit, options.MinimumTargetState))
        {
            diagnostics.Add(new LanguageFileValidationDiagnostic(
                LanguageFileValidationDiagnosticKind.InvalidTranslation,
                $"Target unit '{targetUnit.Key}' target state '{targetUnit.TargetState}' does not meet the " +
                $"required minimum state '{options.MinimumTargetState}'.",
                targetUnit.Line,
                targetUnit.Column,
                targetUnit.Key));
        }
    }

    private static IReadOnlyList<LanguageFileValidationDiagnostic> Sort(
        IEnumerable<LanguageFileValidationDiagnostic> diagnostics)
    {
        return diagnostics
            .OrderBy(static item => item.Line)
            .ThenBy(static item => item.Column)
            .ThenBy(static item => item.Kind)
            .ThenBy(static item => item.UnitKey, StringComparer.Ordinal)
            .ThenBy(static item => item.Message, StringComparer.Ordinal)
            .ToArray();
    }
}
