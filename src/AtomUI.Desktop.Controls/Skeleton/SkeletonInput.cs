using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A placeholder that stands in for an input: a single block five times as wide as its height, stretched to the
/// width of its parent while <see cref="SkeletonElement.IsBlock" /> is true.
/// </summary>
public partial class SkeletonInput : SkeletonElement
{
    public SkeletonInput()
    {
    }

    /// <summary>
    /// Derives the width from the height again when <c>Height</c> or <see cref="SkeletonElement.IsBlock" /> changes.
    /// Overrides must call the base member.
    /// </summary>
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
