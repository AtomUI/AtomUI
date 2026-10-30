using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;

namespace AtomUIGallery.ShowCases.DatePicker;

public partial class DatePickerShowCase : GalleryReactiveUserControl<DatePickerViewModel>
{
    public const string LanguageId = nameof(DatePickerShowCase);

    public DatePickerShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is DatePickerViewModel viewModel)
            {
                viewModel.PickerPlacement = PlacementMode.BottomEdgeAlignedLeft;
                RefreshLocalizedPickerOptions(viewModel);

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager is not null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedPickerOptions(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }
            }
        });
    }

    private void HandlePickerSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            viewModel.HandlePickerSizeTypeOptionCheckedChanged(sender, args);
        }
    }

    private void HandlePickerPlacementCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            viewModel.HandlePickerPlacementCheckedChanged(sender, args);
        }
    }

    private void SetBoundSelectedDateTimeTomorrow(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            viewModel.BoundSelectedDateTime = DateTime.Today.AddDays(1);
        }
    }

    private void ClearBoundSelectedDateTime(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            viewModel.BoundSelectedDateTime = null;
        }
    }

    private void SetBoundSelectedDateRangeThisWeek(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            var today       = DateTime.Today;
            var daysToStart = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startDate   = today.AddDays(-daysToStart);

            viewModel.BoundRangeStartSelectedDate = startDate;
            viewModel.BoundRangeEndSelectedDate   = startDate.AddDays(6);
        }
    }

    private void ClearBoundSelectedDateRange(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
    {
        if (DataContext is DatePickerViewModel viewModel)
        {
            viewModel.BoundRangeStartSelectedDate = null;
            viewModel.BoundRangeEndSelectedDate   = null;
        }
    }

    private static void RefreshLocalizedPickerOptions(DatePickerViewModel viewModel)
    {
        var selectedContent = viewModel.SelectedPickerOption?.Content?.ToString() ?? DatePickerViewModel.PickerTypeTime;
        viewModel.PickerTypeOptions =
        [
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentTime, "Time", DatePickerViewModel.PickerTypeTime),
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentDate, "Date", DatePickerViewModel.PickerTypeDate),
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentWeek, "Week", DatePickerViewModel.PickerTypeWeek),
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentMonth, "Month", DatePickerViewModel.PickerTypeMonth),
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentQuarter, "Quarter", DatePickerViewModel.PickerTypeQuarter),
            PickerOption(DatePickerShowCaseLangResourceKind.P2ContentYear, "Year", DatePickerViewModel.PickerTypeYear)
        ];
        viewModel.SelectedPickerOption = FindPickerOption(viewModel, selectedContent);
    }

    private static ISelectOption FindPickerOption(DatePickerViewModel viewModel, string selectedContent)
    {
        foreach (var option in viewModel.PickerTypeOptions ?? [])
        {
            if (Equals(option.Content?.ToString(), selectedContent))
            {
                return option;
            }
        }

        return viewModel.PickerTypeOptions![0];
    }

    private static SelectOption PickerOption(DatePickerShowCaseLangResourceKind resourceKind, string fallback, string content)
    {
        return new SelectOption
        {
            Header  = DatePickerShowCaseLanguage.Get(resourceKind, fallback),
            Content = content
        };
    }
}

internal static class DatePickerShowCaseLanguage
{
    public static string Get(DatePickerShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
