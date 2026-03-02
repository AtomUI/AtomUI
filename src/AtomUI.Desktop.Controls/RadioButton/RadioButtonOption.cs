namespace AtomUI.Desktop.Controls;

public interface IRadioButtonOption
{
    bool IsEnabled { get; }
    object? Content { get; }
    bool IsChecked { get; }
}

public class RadioButtonOption : IRadioButtonOption
{
    public bool IsEnabled { get; set; } = true;
    public object? Content { get; set; }
    public bool IsChecked { get; set; } = false;
}