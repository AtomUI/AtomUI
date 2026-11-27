using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Controls;

/// <summary>
/// Base class of the tab strip of a tab control and of a standalone tab strip. Next to the tab items it
/// places a selection indicator that follows the selected tab and can keep that tab centered in the
/// viewport of a scrolling strip.
/// </summary>
/// <remarks>
/// The indicator is the <c>PART_SelectedItemIndicator</c> element; keeping the selected tab centered also needs
/// <c>PART_ItemsPresenter</c> and a <c>PART_TabsContainer</c> that is a <c>ScrollViewer</c> or contains one.
/// All three parts are optional. A derived class that binds the parts itself overrides
/// <c>UsesCustomIndicatorTransition</c> to <see langword="true" /> and calls <c>SetCustomIndicatorParts</c>;
/// the base class then skips the indicator when the template is applied.
/// </remarks>
[TemplatePart("PART_SelectedItemIndicator", typeof(Border), IsRequired = false)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = false)]
[TemplatePart("PART_TabsContainer", typeof(Control), IsRequired = false)]
public abstract class AbstractTabStrip : Avalonia.Controls.Primitives.TabStrip, IMotionAwareControl
{
    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AbstractTabControl.TabStripPlacementProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AbstractTabControl.HeaderStartExtraContentProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AbstractTabControl.HeaderStartExtraContentTemplateProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AbstractTabControl.HeaderEndExtraContentProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AbstractTabControl.HeaderEndExtraContentTemplateProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="IndicatorMode" /> property.
    /// </summary>
    public static readonly StyledProperty<TabIndicatorMode> IndicatorModeProperty =
        AbstractTabControl.IndicatorModeProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="IndicatorFixedLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorFixedLengthProperty =
        AbstractTabControl.IndicatorFixedLengthProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="IsIndicatorLengthAnimated" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndicatorLengthAnimatedProperty =
        AbstractTabControl.IsIndicatorLengthAnimatedProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="IsSelectedItemCentered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedItemCenteredProperty =
        AbstractTabControl.IsSelectedItemCenteredProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTabStrip>();

    /// <summary>
    /// Placement of the strip along the edge of its tab control. The selection indicator runs horizontally for
    /// <see cref="Dock.Top" /> and <see cref="Dock.Bottom" /> and vertically for <see cref="Dock.Left" /> and
    /// <see cref="Dock.Right" />. The default is <see cref="Dock.Top" />.
    /// </summary>
    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    /// <summary>
    /// Extra content placed at the start of the strip, in front of the tab items. The default is
    /// <see langword="null" />, which shows nothing.
    /// </summary>
    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }

    /// <summary>
    /// Template used to build <see cref="HeaderStartExtraContent" />. The default is <see langword="null" />,
    /// which leaves the choice of a template to the content presenter.
    /// </summary>
    public IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Extra content placed at the end of the strip, after the tab items. The default is
    /// <see langword="null" />, which shows nothing.
    /// </summary>
    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }

    /// <summary>
    /// Template used to build <see cref="HeaderEndExtraContent" />. The default is <see langword="null" />,
    /// which leaves the choice of a template to the content presenter.
    /// </summary>
    public IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Extent of the selection indicator relative to the selected tab. The default is
    /// <see cref="TabIndicatorMode.Full" />.
    /// </summary>
    /// <remarks>
    /// <see cref="TabIndicatorMode.Full" /> spans the whole tab item, <see cref="TabIndicatorMode.Content" />
    /// spans its header and <see cref="TabIndicatorMode.Fixed" /> spans a length of
    /// <see cref="IndicatorFixedLength" /> centered on the header.
    /// </remarks>
    public TabIndicatorMode IndicatorMode
    {
        get => GetValue(IndicatorModeProperty);
        set => SetValue(IndicatorModeProperty, value);
    }

    /// <summary>
    /// Length of the selection indicator along the tab strip, in device-independent pixels. It applies only when
    /// <see cref="IndicatorMode" /> is <see cref="TabIndicatorMode.Fixed" />. The default is <c>30</c>.
    /// </summary>
    /// <remarks>
    /// The registered validator rejects NaN, infinite and negative values.
    /// </remarks>
    public double IndicatorFixedLength
    {
        get => GetValue(IndicatorFixedLengthProperty);
        set => SetValue(IndicatorFixedLengthProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the selection indicator also grows or shrinks while it moves to a tab of
    /// another size; when <see langword="false" />, only its position is animated. The default is
    /// <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The length animates only when the selection changes while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" /> and the number of items stays the same.
    /// </remarks>
    public bool IsIndicatorLengthAnimated
    {
        get => GetValue(IsIndicatorLengthAnimatedProperty);
        set => SetValue(IsIndicatorLengthAnimatedProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the strip scrolls to keep the selected tab centered in its viewport.
    /// The default is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The strip is scrolled when the selection, the item count or the viewport changes, or when the selected
    /// item moves, within the scrollable range. The scroll animates only for a selection change while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" />. Centering requires the template part
    /// <c>PART_TabsContainer</c> to be a <c>ScrollViewer</c> or to contain one, and <c>PART_ItemsPresenter</c>
    /// to exist.
    /// </remarks>
    public bool IsSelectedItemCentered
    {
        get => GetValue(IsSelectedItemCenteredProperty);
        set => SetValue(IsSelectedItemCenteredProperty, value);
    }

    /// <summary>
    /// Controls whether the selection indicator and the scroll of <see cref="IsSelectedItemCentered" /> animate
    /// when the selection changes. When <see langword="false" />, both take their new position at once.
    /// </summary>
    /// <remarks>
    /// The desktop tab strip theme binds the value to the <c>EnableMotion</c> token, and the tab control themes
    /// bind it to the value of the owning tab control; clearing a local value falls back to that binding.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private TabSelectionIndicator? _tabIndicator;

    // Desktop line tabs retain their reorder-aware transition until opt-in composition is requested.
    /// <summary>
    /// When <see langword="true" />, the derived control binds the selection indicator to its own template parts
    /// through <c>SetCustomIndicatorParts</c>, instead of the base class; <c>BaseTabStrip</c> in
    /// AtomUI.Desktop.Controls overrides it to <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false" />: the base class then binds its built-in indicator to the
    /// parts of the template when the template is applied. Both modes drive the same indicator, so the
    /// transition does not change.
    /// </remarks>
    private protected virtual bool UsesCustomIndicatorTransition => false;

    /// <summary>
    /// The base member is called first. Unless <c>UsesCustomIndicatorTransition</c> is <see langword="true" />,
    /// binds the selection indicator to the parts of the applied template; otherwise the derived class binds
    /// them itself through <c>SetCustomIndicatorParts</c>.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (!UsesCustomIndicatorTransition)
        {
            _tabIndicator ??= new TabSelectionIndicator(this);
            _tabIndicator.Apply(e.NameScope, IsMotionEnabled);
        }
    }

    private protected void SetCustomIndicatorParts(Border? indicator, ScrollViewer? scroll, ItemsPresenter? itemsPresenter)
    {
        _tabIndicator ??= new TabSelectionIndicator(this);
        _tabIndicator.SetParts(indicator, scroll, itemsPresenter, IsMotionEnabled);
    }

    private protected void ResetCustomIndicatorParts() => _tabIndicator?.Reset();

    /// <summary>
    /// The base member is called after the selection indicator has released the parts of the old template; it is
    /// bound to the new template by the next <see cref="OnApplyTemplate" />. Overrides that skip the base member
    /// leave the indicator bound to elements of the old template.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _tabIndicator?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// The base member is called first; then the selection indicator, if its parts are bound, starts tracking
    /// whether motion is allowed. Overrides call the base member.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tabIndicator?.Attach(IsMotionEnabled);
    }

    /// <summary>
    /// The base member is called after the selection indicator has released its motion tracking and stopped its
    /// running motion. The parts stay bound, so attaching the strip again only restarts the tracking. Overrides
    /// call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tabIndicator?.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Arranges the tab items and then positions the selection indicator from the arranged bounds of the
    /// selected tab. Overrides call the base member; without it the indicator is not repositioned.
    /// </summary>
    /// <returns>
    /// The size reported by the base member.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        _tabIndicator?.Update(TabStripPlacement, IndicatorMode, IndicatorFixedLength, IsIndicatorLengthAnimated,
            IsSelectedItemCentered, IsMotionEnabled);
        return size;
    }

    /// <summary>
    /// The base member is called first. While the selection indicator exists, a change of <c>FlowDirection</c>,
    /// of the selected item or of <see cref="TabStripPlacement" />, <see cref="IndicatorMode" />,
    /// <see cref="IndicatorFixedLength" />, <see cref="IsIndicatorLengthAnimated" />,
    /// <see cref="IsSelectedItemCentered" /> or <see cref="IsMotionEnabled" /> updates the indicator and
    /// requests a new arrange pass. Overrides call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlowDirectionProperty && _tabIndicator is not null)
        {
            InvalidateArrange();
        }
        if (change.Property == SelectedItemProperty || change.Property == TabStripPlacementProperty ||
            change.Property == IndicatorModeProperty || change.Property == IndicatorFixedLengthProperty ||
            change.Property == IsIndicatorLengthAnimatedProperty || change.Property == IsSelectedItemCenteredProperty ||
            change.Property == IsMotionEnabledProperty)
        {
            if (_tabIndicator is { } indicator)
            {
                indicator.Update(TabStripPlacement, IndicatorMode, IndicatorFixedLength, IsIndicatorLengthAnimated,
                    IsSelectedItemCentered, IsMotionEnabled);
                InvalidateArrange();
            }
        }
    }
}
