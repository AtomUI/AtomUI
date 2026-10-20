namespace AtomUI.Build.Tasks.LocalizationBuild;

// String-only fallback contract shared by application linking and the runtime matcher.
internal static class LanguageFallbackChain
{
    internal static IReadOnlyList<string> Resolve(string requested)
    {
        var values = new List<string> { requested };
        var seen = new HashSet<string>(StringComparer.Ordinal) { requested };
        if (requested == "en-US")
        {
            return values;
        }
        var structural = requested;
        if (structural.StartsWith("x-", StringComparison.Ordinal))
        {
            structural = "";
        }
        else
        {
            var subtags = structural.Split('-');
            for (var index = 1; index < subtags.Length; index++)
            {
                if (subtags[index].Length == 1)
                {
                    structural = string.Join("-", subtags, 0, index);
                    break;
                }
            }
        }
        while (structural.Length != 0)
        {
            Add(structural);
            if (structural is "zh-CN" or "zh-SG")
            {
                Add("zh-Hans");
            }
            else if (structural is "zh-HK" or "zh-MO" or "zh-TW")
            {
                Add("zh-Hant");
            }
            var separator = structural.LastIndexOf('-');
            if (separator < 0)
            {
                break;
            }
            structural = structural.Substring(0, separator);
        }
        Add("en-US");
        return values;

        void Add(string candidate)
        {
            if (seen.Add(candidate))
            {
                values.Add(candidate);
            }
        }
    }

    internal static bool IsEnglish(string value) => value == "en" || value.StartsWith("en-", StringComparison.Ordinal);
}
