using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class SkeletonInput : SkeletonElement
{
    public SkeletonInput()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HeightProperty || change.Property == IsBlockProperty)
        {
            ConfigureWidth();
        }
    }

    private void ConfigureWidth()
    {
        if (!double.IsNaN(Height) && !IsBlock)
        {
            SetTemplateWidth(Height * 5);
            SetTemplateMinWidth(Height * 5);
        }
    }
}
