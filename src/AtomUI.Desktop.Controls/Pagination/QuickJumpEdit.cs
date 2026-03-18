using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class QuickJumpEdit : LineEdit
{
    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<QuickJumpEdit, int>(nameof(Minimum), 1);
    
    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<QuickJumpEdit, int>(nameof(Maximum),  int.MaxValue);
    
    public int Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(LineEdit);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
        {
            if (int.TryParse(Text, out var pageNumber))
            {
                if (pageNumber < Minimum)
                {
                    SetCurrentValue(TextProperty, Minimum.ToString());
                }

                if (pageNumber > Maximum)
                {
                    SetCurrentValue(TextProperty, Maximum.ToString());
                }
            }
        }
    }
    
    protected override void OnTextInput(TextInputEventArgs e)
    {
        var inputText = e.Text?.Trim();
        if (!string.IsNullOrEmpty(inputText) && inputText.All(char.IsDigit))
        {
            base.OnTextInput(e);
        }
        else
        {
            e.Handled = true;
        }
    }
}