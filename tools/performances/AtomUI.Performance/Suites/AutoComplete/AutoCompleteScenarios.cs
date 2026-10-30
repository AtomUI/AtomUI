using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateAutoCompleteScenarios()
    {
        return
        [
            new PerfScenario("AutoComplete.Default.Closed", _ => new AutoComplete
            {
                Width         = 240,
                OptionsSource = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.AllowClear.Closed", _ => new AutoComplete
            {
                Width         = 240,
                IsAllowClear  = true,
                OptionsSource = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.ContainsFilter.Closed", _ => new AutoComplete
            {
                Width         = 260,
                IsAllowClear  = true,
                Filter        = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
                OptionsSource = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.SearchEdit.Closed", _ => new AutoCompleteSearchEdit
            {
                Width             = 300,
                SearchButtonStyle = SearchButtonType.Primary,
                OptionsSource     = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.TextArea.Closed", _ => new AutoCompleteTextArea
            {
                Width         = 300,
                Lines         = 5,
                IsAllowClear  = true,
                OptionsSource = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.AsyncDebounce.Closed", _ => new AutoComplete
            {
                Width             = 240,
                AsyncLoadDebounce = TimeSpan.FromMilliseconds(120),
                OptionsSource     = CreateAutoCompleteOptions()
            }),
            new PerfScenario("AutoComplete.PopupMaterialized", _ => MaterializeAutoCompletePopupAfterLoaded(new AutoComplete
            {
                Width               = 240,
                MinimumPrefixLength = 0,
                OptionsSource       = CreateAutoCompleteOptions(),
                Value               = "a"
            })),
            new PerfScenario("AutoComplete.SearchEdit.PopupMaterialized", _ => MaterializeAutoCompletePopupAfterLoaded(new AutoCompleteSearchEdit
            {
                Width               = 300,
                MinimumPrefixLength = 0,
                OptionsSource       = CreateAutoCompleteOptions(),
                Value               = "a"
            }))
        ];
    }

    private static List<IAutoCompleteOption> CreateAutoCompleteOptions()
    {
        return
        [
            new AutoCompleteOption { Header = "Apple", Content = "Apple" },
            new AutoCompleteOption { Header = "Apricot", Content = "Apricot" },
            new AutoCompleteOption { Header = "Banana", Content = "Banana" },
            new AutoCompleteOption { Header = "Blueberry", Content = "Blueberry" },
            new AutoCompleteOption { Header = "Cherry", Content = "Cherry" },
            new AutoCompleteOption { Header = "Orange", Content = "Orange" }
        ];
    }

    private static T MaterializeAutoCompletePopupAfterLoaded<T>(T autoComplete)
        where T : AbstractAutoComplete
    {
        void HandleLoaded(object? sender, RoutedEventArgs args)
        {
            autoComplete.Loaded -= HandleLoaded;
            MaterializeLazyPopupContentForTest(autoComplete);
        }

        autoComplete.Loaded += HandleLoaded;
        return autoComplete;
    }
}
