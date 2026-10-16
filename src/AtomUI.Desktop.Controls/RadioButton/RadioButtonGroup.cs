using AtomUI.Controls.Commons;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

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