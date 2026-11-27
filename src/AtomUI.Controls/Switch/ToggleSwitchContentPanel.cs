using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

// Content canvas of the toggle switch. It reports the desired size of the contents, so that a change of the
// content size makes the owner measure again through the template tree. The template provides the offsets.
// The contents are arranged in fixed slots; transitions only update the render transform.
internal sealed class ToggleSwitchContentPanel : Canvas
{
    public static readonly StyledProperty<Point> OnContentOffsetProperty =
        AbstractToggleSwitch.OnContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    public static readonly StyledProperty<Point> OffContentOffsetProperty =
        AbstractToggleSwitch.OffContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    public Point OnContentOffset
    {
        get => GetValue(OnContentOffsetProperty);
        set => SetValue(OnContentOffsetProperty, value);
    }

    public Point OffContentOffset
    {
        get => GetValue(OffContentOffsetProperty);
        set => SetValue(OffContentOffsetProperty, value);
    }

    internal static void SetContentOffset(Control? child, Point offset)
    {
        if (child is null)
        {
            return;
        }

        if (child.RenderTransform is TranslateTransform transform)
        {
            transform.X = offset.X;
            transform.Y = offset.Y;
        }
        else
        {
            child.SetCurrentValue(RenderTransformProperty, new TranslateTransform(offset.X, offset.Y));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OnContentOffsetProperty || change.Property == OffContentOffsetProperty)
        {
            foreach (var child in Children)
            {
                ApplyOffset(child);
            }
        }
    }

    private void ApplyOffset(Control child) =>
        SetContentOffset(child, child.Name == "PART_OnContentPresenter" ? OnContentOffset : OffContentOffset);

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = default(Size);
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            desired = new Size(Math.Max(desired.Width, child.DesiredSize.Width),
                Math.Max(desired.Height, child.DesiredSize.Height));
        }

        // The centering offsets of the owner depend on the size of each presenter, not only on the maximum size of
        // the template root. The knob size can hide a change of the content size, so this arrange dependency must be
        // declared explicitly.
        if (TemplatedParent is AbstractToggleSwitch owner)
        {
            owner.InvalidateArrange();
        }

        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(new Rect(child.DesiredSize));
            ApplyOffset(child);
        }

        return finalSize;
    }
}
