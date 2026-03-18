namespace AtomUI.Controls.Utils;

public interface IValueFilter
{
    ValueFilterMode Mode => ValueFilterMode.Custom;
    bool Filter(object? value, object? filterValue);
}

public delegate object? DefaultFilterValueSelector(object record);