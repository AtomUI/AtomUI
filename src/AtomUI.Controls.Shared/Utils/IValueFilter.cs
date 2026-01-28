namespace AtomUI.Controls.Utils;

public interface IValueFilter
{
    bool Filter(object? value, object? filterValue);
}