using AtomUI.Controls.Commons;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public partial class Separator : AbstractSeparator
{
}

public class VerticalSeparator : Separator
{
    static VerticalSeparator()
    {
        OrientationProperty.OverrideDefaultValue<VerticalSeparator>(Orientation.Vertical);
    }

    protected override Type StyleKeyOverride => typeof(Separator);
}
