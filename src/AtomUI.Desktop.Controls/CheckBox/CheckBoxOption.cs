namespace AtomUI.Desktop.Controls;

public interface ICheckBoxOption
{
    bool IsEnabled { get; }
    object? Content { get; }
    bool IsChecked { get; }
}

public class CheckBoxOption : ICheckBoxOption
{
    public bool IsEnabled { get; set; } = true;
    public object? Content { get; set; }
    public bool IsChecked { get; set; } = false;
}