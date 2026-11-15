using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

[TemplatePart("PART_SelectedItemIndicator", typeof(Border), IsRequired = false)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = false)]
[TemplatePart("PART_TabsContainer", typeof(Control), IsRequired = false)]
public abstract class AbstractTabStrip : Avalonia.Controls.Primitives.TabStrip, IMotionAwareControl
{
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AbstractTabControl.TabStripPlacementProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AbstractTabControl.HeaderStartExtraContentProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AbstractTabControl.HeaderStartExtraContentTemplateProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AbstractTabControl.HeaderEndExtraContentProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AbstractTabControl.HeaderEndExtraContentTemplateProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<TabIndicatorMode> IndicatorModeProperty =
        AbstractTabControl.IndicatorModeProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<double> IndicatorFixedLengthProperty =
        AbstractTabControl.IndicatorFixedLengthProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<bool> IsIndicatorLengthAnimatedProperty =
        AbstractTabControl.IsIndicatorLengthAnimatedProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<bool> IsSelectedItemCenteredProperty =
        AbstractTabControl.IsSelectedItemCenteredProperty.AddOwner<AbstractTabStrip>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTabStrip>();

    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }

    public IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }

    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }

    public IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }

    public TabIndicatorMode IndicatorMode
    {
        get => GetValue(IndicatorModeProperty);
        set => SetValue(IndicatorModeProperty, value);
    }

    public double IndicatorFixedLength
    {
        get => GetValue(IndicatorFixedLengthProperty);
        set => SetValue(IndicatorFixedLengthProperty, value);
    }

    public bool IsIndicatorLengthAnimated
    {
        get => GetValue(IsIndicatorLengthAnimatedProperty);
        set => SetValue(IsIndicatorLengthAnimatedProperty, value);
    }

    public bool IsSelectedItemCentered
    {
        get => GetValue(IsSelectedItemCenteredProperty);
        set => SetValue(IsSelectedItemCenteredProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private TabSelectionIndicator? _tabIndicator;

    // Desktop line tabs retain their reorder-aware transition until opt-in composition is requested.
    private protected virtual bool UsesCustomIndicatorTransition => false;

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

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _tabIndicator?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tabIndicator?.Attach(IsMotionEnabled);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tabIndicator?.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        _tabIndicator?.Update(TabStripPlacement, IndicatorMode, IndicatorFixedLength, IsIndicatorLengthAnimated,
            IsSelectedItemCentered, IsMotionEnabled);
        return size;
    }

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
