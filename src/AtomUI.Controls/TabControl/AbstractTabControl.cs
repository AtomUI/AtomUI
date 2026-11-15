using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

[TemplatePart("PART_SelectedItemIndicator", typeof(Border), IsRequired = false)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = false)]
[TemplatePart("PART_TabsContainer", typeof(Control), IsRequired = false)]
[TemplatePart("PART_SelectedContentHost", typeof(ContentPresenter), IsRequired = false)]
public abstract class AbstractTabControl : SelectingItemsControl, IMotionAwareControl
{
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<AbstractTabControl, Dock>(nameof(TabStripPlacement), Dock.Top);

    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AvaloniaProperty.Register<AbstractTabControl, object?>(nameof(HeaderStartExtraContent));

    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<AbstractTabControl, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));

    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AvaloniaProperty.Register<AbstractTabControl, object?>(nameof(HeaderEndExtraContent));

    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<AbstractTabControl, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));

    public static readonly StyledProperty<TabIndicatorMode> IndicatorModeProperty =
        AvaloniaProperty.Register<AbstractTabControl, TabIndicatorMode>(nameof(IndicatorMode));

    public static readonly StyledProperty<double> IndicatorFixedLengthProperty =
        AvaloniaProperty.Register<AbstractTabControl, double>(nameof(IndicatorFixedLength), 30, validate: static value => double.IsFinite(value) && value >= 0);

    public static readonly StyledProperty<bool> IsIndicatorLengthAnimatedProperty =
        AvaloniaProperty.Register<AbstractTabControl, bool>(nameof(IsIndicatorLengthAnimated));

    public static readonly StyledProperty<bool> IsSelectedItemCenteredProperty =
        AvaloniaProperty.Register<AbstractTabControl, bool>(nameof(IsSelectedItemCentered));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTabControl>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<AbstractTabControl>();

    public static readonly DirectProperty<AbstractTabControl, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<AbstractTabControl, object?>(nameof(SelectedContent), owner => owner.SelectedContent);

    public static readonly DirectProperty<AbstractTabControl, IDataTemplate?> SelectedContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<AbstractTabControl, IDataTemplate?>(nameof(SelectedContentTemplate),
            owner => owner.SelectedContentTemplate);

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

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public object? SelectedContent
    {
        get => _selectedContent;
        internal set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }

    public IDataTemplate? SelectedContentTemplate
    {
        get => _selectedContentTemplate;
        internal set => SetAndRaise(SelectedContentTemplateProperty, ref _selectedContentTemplate, value);
    }

    private TabSelectionIndicator? _tabIndicator;

    private object? _selectedContent;
    private IDataTemplate? _selectedContentTemplate;
    private CompositeDisposable? _selectedItemSubscriptions;

    internal ContentPresenter? ContentPart { get; private set; }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!e.Handled && !UsesCustomIndicatorTransition && e.Source is Visual source &&
            source.FindAncestorOfType<AbstractTabItem>(includeSelf: true) is { IsEffectivelyEnabled: true } item &&
            ReferenceEquals(ItemsControlFromItemContainer(item), this) &&
            e.InitialPressMouseButton == MouseButton.Left)
        {
            SetCurrentValue(SelectedIndexProperty, IndexFromContainer(item));
            e.Handled = true;
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateTabItem();

    protected virtual AbstractTabItem CreateTabItem() => new DefaultTabItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractTabItem>(item, out recycleKey);

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is AbstractTabItem tabItem)
        {
            tabItem.TabStripPlacement = TabStripPlacement;
            if (!ReferenceEquals(tabItem, item))
            {
                tabItem.SetCurrentValue(ContentControl.ContentProperty, item);
                tabItem.SetCurrentValue(HeaderedContentControl.HeaderProperty, item);
                tabItem.SetCurrentValue(HeaderedContentControl.HeaderTemplateProperty, ItemTemplate);
            }
        }

        if (index == SelectedIndex)
        {
            UpdateSelectedContent(container);
        }
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        if (SelectedIndex == oldIndex || SelectedIndex == newIndex)
        {
            UpdateSelectedContent();
        }
    }

    protected virtual bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name != "PART_SelectedContentHost")
        {
            return false;
        }

        ContentPart = presenter;
        return true;
    }

    protected void UpdateSelectedContent(Control? container = null)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;

        container ??= SelectedIndex < 0 ? null : ContainerFromIndex(SelectedIndex);
        if (container is null)
        {
            SelectedContent = SelectedContentTemplate = null;
            return;
        }

        IDataTemplate? ResolveTemplate(IDataTemplate? itemTemplate) => itemTemplate ?? ContentTemplate;
        if (SelectedContentTemplate != ResolveTemplate(container.GetValue(ContentControl.ContentTemplateProperty)))
        {
            // Release controls from the previous template before changing their data context.
            SelectedContentTemplate = null;
        }

        var subscriptions = new CompositeDisposable();
        _selectedItemSubscriptions = subscriptions;
        subscriptions.Add(container.GetObservable(ContentControl.ContentProperty).Subscribe(value => SelectedContent = value));
        subscriptions.Add(container.GetObservable(ContentControl.ContentTemplateProperty).Subscribe(value => SelectedContentTemplate = ResolveTemplate(value)));
    }

    private sealed class DefaultTabItem : AbstractTabItem;

    // Desktop line tabs retain their reorder-aware transition until opt-in composition is requested.
    private protected virtual bool UsesCustomIndicatorTransition => false;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ContentPart = null;
        if (e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost") is { } content)
        {
            RegisterContentPresenter(content);
        }

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
        UpdateSelectedContent();
        _tabIndicator?.Attach(IsMotionEnabled);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;
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
        if (change.Property == SelectedItemProperty)
        {
            UpdateSelectedContent();
        }
        else if (change.Property == ContentTemplateProperty && ContainerFromIndex(SelectedIndex) is { } container &&
                 container.GetValue(ContentControl.ContentTemplateProperty) is null)
        {
            SelectedContentTemplate = ContentTemplate;
        }
        else if (change.Property == TabStripPlacementProperty)
        {
            for (var index = 0; index < ItemCount; index++)
            {
                if (ContainerFromIndex(index) is AbstractTabItem item)
                {
                    item.TabStripPlacement = TabStripPlacement;
                }
            }
        }
        else if (change.Property == FlowDirectionProperty && _tabIndicator is not null)
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
