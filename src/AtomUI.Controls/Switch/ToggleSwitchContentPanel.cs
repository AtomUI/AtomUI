using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Content canvas used inside a toggle switch template to measure on/off content presenters and position them via
/// render transforms driven by <see cref="OnContentOffset" /> and <see cref="OffContentOffset" />.
/// </summary>
public sealed class ToggleSwitchContentPanel : Canvas
{
    /// <summary>
    /// Identifies the <see cref="OnContentOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OnContentOffsetProperty =
        AbstractToggleSwitch.OnContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    /// <summary>
    /// Identifies the <see cref="OffContentOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OffContentOffsetProperty =
        AbstractToggleSwitch.OffContentOffsetProperty.AddOwner<ToggleSwitchContentPanel>();

    /// <summary>
    /// Offset applied to <c>PART_OnContentPresenter</c> inside the switch groove.
    /// </summary>
    public Point OnContentOffset
    {
        get => GetValue(OnContentOffsetProperty);
        set => SetValue(OnContentOffsetProperty, value);
    }

    /// <summary>
    /// Offset applied to <c>PART_OffContentPresenter</c> inside the switch groove.
    /// </summary>
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

    /// <summary>
    /// Reapplies content presenter render-transform offsets whenever <see cref="OnContentOffset" /> or
    /// <see cref="OffContentOffset" /> changes.
    /// </summary>
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

    /// <summary>
    /// Measures all content presenters and notifies the owning <see cref="AbstractToggleSwitch" /> to rearrange so
    /// centering offsets reflect any content size change.
    /// </summary>
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

    /// <summary>
    /// Arranges all content presenters at their desired sizes and applies their current render-transform offsets.
    /// </summary>
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
