using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of a timeline entry: a content control with a label, a node and a rail toward the next node.
/// The owning <see cref="AbstractTimeline" /> supplies the layout settings and the position among the items.
/// </summary>
public abstract class AbstractTimelineItem : ContentControl
{
    /// <summary>
    /// Pseudo-class of an item at an odd index among the visible items, counted from 0 in display order.
    /// </summary>
    public const string OrderOddPC = "order-odd";
    /// <summary>
    /// Pseudo-class of an item at an even index among the visible items, counted from 0 in display order.
    /// </summary>
    public const string OrderEvenPC = "order-even";
    /// <summary>
    /// Pseudo-class of the first visible item in display order.
    /// </summary>
    public const string OrderFirstPC = "order-first";
    /// <summary>
    /// Pseudo-class of the last visible item in display order.
    /// </summary>
    public const string OrderLastPC = "order-last";
    /// <summary>
    /// Pseudo-class of the pending item, which a timeline creates when
    /// <see cref="AbstractTimeline.Pending" /> is set.
    /// </summary>
    public const string PendingItemPC = "pending-item";
        
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Label" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<AbstractTimelineItem, string?>(nameof(Label));
    
    /// <summary>
    /// Identifies the <see cref="IndicatorIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IndicatorIconProperty =
        AvaloniaProperty.Register<AbstractTimelineItem, PathIcon?>(nameof(IndicatorIcon));
    
    /// <summary>
    /// Identifies the <see cref="IndicatorColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<AbstractTimelineItem, IBrush?>(nameof(IndicatorColor));
    
    /// <summary>
    /// Text shown next to the node in addition to the content. It is hidden when the value is <see langword="null" />
    /// or empty. As soon as one visible item has a label that is not <see langword="null" />, an empty one included,
    /// every item of the timeline is laid out with room for one; on a vertical timeline the label and the content
    /// then lie on opposite sides of the node.
    /// </summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Icon drawn in the node instead of the default dot. <see langword="null" /> shows the dot.
    /// </summary>
    public PathIcon? IndicatorIcon
    {
        get => GetValue(IndicatorIconProperty);
        set => SetValue(IndicatorIconProperty, value);
    }
    
    /// <summary>
    /// Color of the node. <see langword="null" /> falls back to the color of the theme.
    /// </summary>
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Orientation> OrientationProperty =
        AbstractTimeline.OrientationProperty.AddOwner<AbstractTimelineItem>();
    
    internal static readonly DirectProperty<AbstractTimelineItem, TimelineMode> ModeProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, TimelineMode>(nameof(Mode), 
            o => o.Mode,
            (o, v) => o.Mode = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsOddProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsOdd), 
            o => o.IsOdd,
            (o, v) => o.IsOdd = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsFirstProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsFirst), 
            o => o.IsFirst,
            (o, v) => o.IsFirst = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsLastProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsLast), 
            o => o.IsLast,
            (o, v) => o.IsLast = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> NextIsPendingProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(NextIsPending), 
            o => o.NextIsPending,
            (o, v) => o.NextIsPending = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsReverseProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsReverse), 
            o => o.IsReverse,
            (o, v) => o.IsReverse = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsPendingProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsPending), 
            o => o.IsPending,
            (o, v) => o.IsPending = v);
    
    internal static readonly DirectProperty<AbstractTimelineItem, bool> IsLabelLayoutProperty = 
        AvaloniaProperty.RegisterDirect<AbstractTimelineItem, bool>(nameof(IsLabelLayout), 
            o => o.IsLabelLayout,
            (o, v) => o.IsLabelLayout = v);
        
    /// <summary>
    /// Gets or sets the direction in which the timeline stacks its items. The owning timeline binds the value
    /// of <see cref="AbstractTimeline.Orientation" /> to every item. The theme forwards it to
    /// <c>Orientation</c> of the section panel and of the indicator, and uses it to select the gap to the next
    /// item and the size of the indicator.
    /// </summary>
    internal Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private TimelineMode _mode = TimelineMode.Start;

    /// <summary>
    /// Gets or sets on which side of its node the item places its content. The owning timeline binds the value
    /// of <see cref="AbstractTimeline.Mode" /> to every item. The theme forwards it to <c>Mode</c> of the
    /// section panel, aligns the label and the content with it, and sets the margin of the indicator by mode:
    /// on a vertical timeline the content lies on the right for <see cref="TimelineMode.Start" />, on the left
    /// for <see cref="TimelineMode.End" />, and alternates between the two sides for
    /// <see cref="TimelineMode.Alternate" />.
    /// </summary>
    internal TimelineMode Mode
    {
        get => _mode;
        set => SetAndRaise(ModeProperty, ref _mode, value);
    }
    
    private bool _isOdd;

    /// <summary>
    /// Gets or sets whether the item is at an odd index among the visible items, counted from 0 in display
    /// order, which is the reversed item order while the timeline is reversed. The owning timeline sets the
    /// value.
    /// </summary>
    /// <remarks>
    /// A change updates the <c>order-odd</c> and <c>order-even</c> pseudo-classes. The theme forwards the value
    /// to <c>IsOdd</c> of the section panel, and for <see cref="TimelineMode.Alternate" /> on a vertical
    /// timeline aligns the label and the content of odd items on the other side than those of even items.
    /// </remarks>
    internal bool IsOdd
    {
        get => _isOdd;
        set => SetAndRaise(IsOddProperty, ref _isOdd, value);
    }
    
    private bool _isFirst;

    /// <summary>
    /// Gets or sets whether the item is the first among the visible items in display order. The owning timeline
    /// sets the value.
    /// </summary>
    /// <remarks>
    /// A change updates the <c>order-first</c> pseudo-class. The theme forwards the value to <c>IsFirst</c> of
    /// the indicator.
    /// </remarks>
    internal bool IsFirst
    {
        get => _isFirst;
        set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }
    
    private bool _isLast;

    /// <summary>
    /// Gets or sets whether the item is the last among the visible items in display order. The owning timeline
    /// sets the value.
    /// </summary>
    /// <remarks>
    /// A change updates the <c>order-last</c> pseudo-class. The theme forwards the value to <c>IsLast</c> of
    /// the indicator, and on a vertical timeline sets the <c>Padding</c> of the last item to <c>0</c>, so that
    /// no gap follows the last node.
    /// </remarks>
    internal bool IsLast
    {
        get => _isLast;
        set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }
    
    private bool _nextIsPending;

    /// <summary>
    /// Gets or sets whether the item that follows this one among the visible items in display order is the
    /// pending item. The owning timeline sets the value.
    /// </summary>
    /// <remarks>
    /// The theme forwards the value to <c>NextIsPending</c> of the indicator, and on a vertical timeline that
    /// is not reversed gives this item the large gap to the next item (<c>ItemPaddingBottomLG</c>) instead of
    /// the regular gap.
    /// </remarks>
    internal bool NextIsPending
    {
        get => _nextIsPending;
        set => SetAndRaise(NextIsPendingProperty, ref _nextIsPending, value);
    }

    private bool _isReverse = false;

    /// <summary>
    /// Gets or sets whether the timeline shows its items in reverse order, the last item first. The owning
    /// timeline binds the value of <see cref="AbstractTimeline.IsReverse" /> to every item.
    /// </summary>
    /// <remarks>
    /// The value does not reorder the items; the first, last and odd flags of the items follow the display
    /// order. On a vertical timeline the theme gives the large gap to the next item
    /// (<c>ItemPaddingBottomLG</c>) to the item before the pending one while this is
    /// <see langword="false" />, and to the pending item itself while this is <see langword="true" />. A change
    /// measures the item again.
    /// </remarks>
    internal bool IsReverse
    {
        get => _isReverse;
        set => SetAndRaise(IsReverseProperty, ref _isReverse, value);
    }
    
    private bool _isPending = false;

    /// <summary>
    /// Gets or sets whether the item is the pending item of the timeline, the item a timeline creates for
    /// <see cref="AbstractTimeline.Pending" />.
    /// </summary>
    /// <remarks>
    /// A change updates the <c>pending-item</c> pseudo-class. The owning timeline reads the value to set
    /// <see cref="NextIsPending" /> of the item before this one. On a vertical timeline that is reversed, the
    /// theme gives the pending item the large gap to the next item (<c>ItemPaddingBottomLG</c>).
    /// </remarks>
    internal bool IsPending
    {
        get => _isPending;
        set => SetAndRaise(IsPendingProperty, ref _isPending, value);
    }
    
    private bool _isLabelLayout = false;

    /// <summary>
    /// Gets or sets whether the items of the timeline are laid out with room for a label. The owning timeline
    /// sets the same value on every visible item: <see langword="true" /> as soon as one visible item has a
    /// <see cref="Label" /> that is not <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// The theme forwards the value to <c>IsLabelLayout</c> of the section panel. On a vertical timeline it
    /// uses the middle margin of the indicator; on a horizontal timeline it centers the label and the content.
    /// </remarks>
    internal bool IsLabelLayout
    {
        get => _isLabelLayout;
        set => SetAndRaise(IsLabelLayoutProperty, ref _isLabelLayout, value);
    }

    #endregion

    static AbstractTimelineItem()
    {
        // The rail in the indicator crosses the bottom edge of the item to join the top edge of the next node.
        // The ContentControl default ClipToBounds=true would cut off this overflowing rail segment.
        ClipToBoundsProperty.OverrideDefaultValue<AbstractTimelineItem>(false);
        AffectsArrange<AbstractTimelineItem>(ModeProperty, OrientationProperty);
        // The item padding is the gap to the next item (antd itemPaddingBottom); a Token change must re-measure.
        AffectsMeasure<AbstractTimelineItem>(IsReverseProperty, PaddingProperty);
    }

    /// <summary>
    /// Measures the template root inside <see cref="Avalonia.Controls.Primitives.TemplatedControl.Padding" />, which
    /// carries the gap to the next item. Falls back to the base method when the first visual child is not a
    /// <see cref="Layoutable" />.
    /// </summary>
    /// <remarks>
    /// An override that does not call the base method must inset the template root by
    /// <see cref="Avalonia.Controls.Primitives.TemplatedControl.Padding" /> itself, or the gap to the next item is
    /// lost.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Mirrors the paddingBottom of the antd li: the gap to the next item is carried by the item's Padding,
        // and the template root (wrapper) is laid out inside the content area, which keeps the wrapper and
        // section boxes tight.
        if (VisualChildren.Count > 0 && VisualChildren[0] is Layoutable root)
        {
            return LayoutHelper.MeasureChild(root, availableSize, Padding);
        }

        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Arranges the template root inside <see cref="Avalonia.Controls.Primitives.TemplatedControl.Padding" />. Falls
    /// back to the base method when the first visual child is not a <see cref="Layoutable" />.
    /// </summary>
    /// <remarks>
    /// An override that does not call the base method must arrange the template root inside
    /// <see cref="Avalonia.Controls.Primitives.TemplatedControl.Padding" /> itself, or the gap to the next item is
    /// lost.
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (VisualChildren.Count > 0 && VisualChildren[0] is Layoutable root)
        {
            return LayoutHelper.ArrangeChild(root, finalSize, Padding);
        }

        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Calls the base method and asserts in debug builds that the <c>Parent</c> is an
    /// <see cref="AbstractTimeline" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is AbstractTimeline, "AbstractTimelineItem's parent must be a timeline control.");
    }

    /// <summary>
    /// Calls the base method, then updates the pseudo-classes when <c>IsOdd</c>, <c>IsFirst</c>, <c>IsLast</c> or
    /// <c>IsPending</c> changed, and asks the owning timeline to recalculate the order flags of all items when
    /// <see cref="Visual.IsVisible" /> or <see cref="Label" /> changed.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or the pseudo-classes and the order flags stop following these changes.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOddProperty ||
            change.Property == IsFirstProperty ||
            change.Property == IsLastProperty ||
            change.Property == IsPendingProperty)
        {
            UpdatePseudoClasses();
        }

        if ((change.Property == IsVisibleProperty || change.Property == LabelProperty) &&
            Parent is AbstractTimeline timeline)
        {
            timeline.NotifyItemLayoutChanged();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(OrderOddPC, IsOdd);
        PseudoClasses.Set(OrderEvenPC, !IsOdd);
        PseudoClasses.Set(OrderFirstPC, IsFirst);
        PseudoClasses.Set(OrderLastPC, IsLast);
        PseudoClasses.Set(PendingItemPC, IsPending);
    }
}
