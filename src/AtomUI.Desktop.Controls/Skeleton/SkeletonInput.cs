using Avalonia;
using Avalonia.Data;

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
            SetValue(WidthProperty, Height * 5, BindingPriority.Template);
            SetValue(MinWidthProperty, Height * 5, BindingPriority.Template);
        }
    }
}
