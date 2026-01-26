namespace AtomUI.Desktop.Controls;

public interface ICompleteOptionFilter
{
    bool Filter(object? value, object? filterValue);
}