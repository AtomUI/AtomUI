namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class XliffTranslationTarget
{
    internal static bool IsPublishable(XliffUnitModel unit)
    {
        if (unit is null)
        {
            throw new ArgumentNullException(nameof(unit));
        }
        var hasTargetContent = !string.IsNullOrWhiteSpace(unit.Target);
        var isExplicitEmptyTranslation = unit.Source.Length == 0 && unit.Target?.Length == 0;
        return (hasTargetContent || isExplicitEmptyTranslation) &&
               unit.TargetState is "translated" or "reviewed" or "final" &&
               !RequiresAction(unit.TargetSubState);
    }

    internal static bool MeetsMinimumState(XliffUnitModel unit, string minimumState)
    {
        if (!IsPublishable(unit) ||
            !TryGetStateRank(unit.TargetState!, out var targetStateRank) ||
            !TryGetStateRank(minimumState, out var minimumStateRank))
        {
            return false;
        }

        return targetStateRank >= minimumStateRank;
    }

    internal static bool TryGetStateRank(string state, out int rank)
    {
        rank = state switch
        {
            "translated" => 1,
            "reviewed" => 2,
            "final" => 3,
            _ => 0
        };
        return rank != 0;
    }

    private static bool RequiresAction(string? subState)
    {
        if (subState is null || string.IsNullOrWhiteSpace(subState))
        {
            return false;
        }

        var normalized = subState.Trim();
        var namespaceSeparator = normalized.LastIndexOf(':');
        if (namespaceSeparator >= 0)
        {
            normalized = normalized.Substring(namespaceSeparator + 1);
        }

        return normalized.StartsWith("needs-", StringComparison.OrdinalIgnoreCase);
    }
}
