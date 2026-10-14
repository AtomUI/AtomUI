using AtomUI.Controls.Commons;
using AtomUI.Registration;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[BootstrapRegistration]
public class RadioButtonGroup : AbstractRadioButtonGroup
{
    public RadioButtonGroup()
    {
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new RadioButton();
    }
}