using System.Windows.Input;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

/// <summary>
/// A lightweight <see cref="ICommand" /> for the <see cref="CalendarHeaderContext" />.
/// Argument validation is left to <c>canExecute</c>: <see cref="CanExecute" /> returns <see langword="false" />
/// for an invalid argument, and <see cref="Execute" /> does nothing for it.
/// </summary>
internal sealed class CalendarRelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool> _canExecute;

    public CalendarRelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    public void Execute(object? parameter)
    {
        if (!_canExecute(parameter))
        {
            return;
        }

        _execute(parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
