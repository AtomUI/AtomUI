using Avalonia.Controls;

namespace AtomUI.Localization.Performance;

internal static class LocalizationStateVerification
{
    internal static bool Run()
    {
        try
        {
            using var runtime = LocalizationScenarios.BuildRuntime(2, 8, 3);
            var manager = runtime.LanguageManager;
            var expectedSlots = manager.SupportedLanguages.Count * runtime.Registry.Catalogs.Sum(
                static catalog => catalog.Units.Count);
            var actualSlots = runtime.Snapshots.Count * runtime.Registry.Catalogs.Sum(
                static catalog => catalog.Units.Count);

            if (actualSlots != expectedSlots)
            {
                Console.Error.WriteLine($"Localization verification failed: expected {expectedSlots} snapshot slots, got {actualSlots}.");
                return false;
            }

            var host = new Border();
            host.Resources.MergedDictionaries.Add(runtime.ResourceProvider);
            var resourceNotifications = 0;
            var languageEvents = 0;
            ((IResourceHost)host).ResourcesChanged += (_, _) => resourceNotifications++;
            manager.LanguageChanged += (_, _) => languageEvents++;
            var initialState = manager.Current;
            if (initialState.CurrentLanguage != LanguageTags.EnUS || initialState.Revision != 0)
            {
                Console.Error.WriteLine(
                    $"Localization verification failed: expected initial en-US revision 0, got " +
                    $"'{initialState.CurrentLanguage.Value}' revision {initialState.Revision}.");
                return false;
            }
            if (!VerifyResolvedValue(runtime, "en-US:0:0", "initial language"))
            {
                return false;
            }

            var transitions = 0;
            foreach (var language in manager.SupportedLanguages.Skip(1).Select(static definition => definition.Tag))
            {
                var previousState = manager.Current;
                var result = manager.ChangeLanguage(language);
                transitions++;
                if (result.Status != LanguageChangeStatus.Committed ||
                    result.OldState != previousState ||
                    result.NewState.CurrentLanguage != language ||
                    result.NewState.Revision != previousState.Revision + 1)
                {
                    Console.Error.WriteLine(
                        $"Localization verification failed: switch to '{language.Value}' did not return the expected committed revision.");
                    return false;
                }
                if (manager.Current != result.NewState)
                {
                    Console.Error.WriteLine(
                        $"Localization verification failed: manager state does not match the committed '{language.Value}' revision.");
                    return false;
                }
                if (!VerifyResolvedValue(runtime, $"{language.Value}:0:0", $"language '{language.Value}'"))
                {
                    return false;
                }
            }

            if (resourceNotifications != transitions || languageEvents != transitions)
            {
                Console.Error.WriteLine(
                    $"Localization verification failed: expected {transitions} notifications/events, " +
                    $"got {resourceNotifications}/{languageEvents}.");
                return false;
            }

            Console.WriteLine($"Localization state verification passed ({transitions} committed language changes).");
            return true;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Localization verification failed: {exception.Message}");
            return false;
        }
    }

    private static bool VerifyResolvedValue(
        LocalizationRuntime runtime,
        string expected,
        string context)
    {
        var localized = runtime.Localizer.Get(LocalizationScenarios.Catalog0ResourceKind.Value);
        if (!string.Equals(localized, expected, StringComparison.Ordinal))
        {
            Console.Error.WriteLine(
                $"Localization verification failed: {context} localizer value should be '{expected}', got '{localized}'.");
            return false;
        }

        if (!runtime.ResourceProvider.TryGetResource(
                LocalizationScenarios.Catalog0ResourceKind.Value,
                theme: null,
                out var resource) ||
            !string.Equals(resource as string, expected, StringComparison.Ordinal))
        {
            Console.Error.WriteLine(
                $"Localization verification failed: {context} resource value should be '{expected}', got '{resource}'.");
            return false;
        }

        return true;
    }
}
