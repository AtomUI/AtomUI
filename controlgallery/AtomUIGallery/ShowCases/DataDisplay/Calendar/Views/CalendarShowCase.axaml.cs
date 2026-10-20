using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using AtomUICalendarMode = AtomUI.Desktop.Controls.CalendarMode;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;

namespace AtomUIGallery.ShowCases.Calendar;

public partial class CalendarShowCase : GalleryReactiveUserControl<CalendarViewModel>
{
    public const string LanguageId = nameof(CalendarShowCase);

    public CalendarShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            RefreshSelectableCalendarText();

            var languageManager = GalleryLocalization.GetLanguageManager();
            if (languageManager is not null)
            {
                EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshSelectableCalendarText();
                languageManager.LanguageChanged += handler;
                Disposable.Create(() => languageManager.LanguageChanged -= handler)
                          .DisposeWith(disposables);
            }
        });
    }

    private void OnSelectableCalendarSelected(object? sender, CalendarSelectedEventArgs e)
    {
        if (DataContext is CalendarViewModel vm)
        {
            vm.SelectSelectableCalendarDate(e.Value);
        }
    }

    private void RefreshSelectableCalendarText()
    {
        if (DataContext is CalendarViewModel vm)
        {
            vm.RefreshSelectableCalendarText();
        }
    }

    private void OnCustomHeaderModeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not OptionButtonGroup { DataContext: CalendarHeaderContext context } group)
        {
            return;
        }

        var mode = group.SelectedIndex switch
        {
            0 => AtomUICalendarMode.Month,
            1 => AtomUICalendarMode.Year,
            _ => context.Mode
        };
        if (mode != context.Mode && context.ChangeModeCommand.CanExecute(mode))
        {
            context.ChangeModeCommand.Execute(mode);
        }
    }

    private void OnCustomHeaderYearSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not AtomUIComboBox
            {
                DataContext: CalendarHeaderContext context,
                SelectedItem: int year
            } || year == context.Value.Year)
        {
            return;
        }

        var target = CreateDate(year, context.Value.Month, context.Value.Day);
        if (context.ChangeValueCommand.CanExecute(target))
        {
            context.ChangeValueCommand.Execute(target);
        }
    }

    private void OnCustomHeaderMonthSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not AtomUIComboBox { DataContext: CalendarHeaderContext context } comboBox ||
            comboBox.SelectedIndex is < 0 or > 11)
        {
            return;
        }

        var month = comboBox.SelectedIndex + 1;
        if (month == context.Value.Month)
        {
            return;
        }

        var target = CreateDate(context.Value.Year, month, context.Value.Day);
        if (context.ChangeValueCommand.CanExecute(target))
        {
            context.ChangeValueCommand.Execute(target);
        }
    }

    private static DateTime CreateDate(int year, int month, int preferredDay)
    {
        var day = Math.Min(preferredDay, DateTime.DaysInMonth(year, month));
        return new DateTime(year, month, day);
    }
}
