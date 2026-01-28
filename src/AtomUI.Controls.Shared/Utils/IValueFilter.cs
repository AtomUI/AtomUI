namespace AtomUI.Controls.Utils;

public interface IValueFilter<T>
{
    bool Filter(T? value, T? filterValue);
}

public interface IStringValueFilter : IValueFilter<string>
{
}