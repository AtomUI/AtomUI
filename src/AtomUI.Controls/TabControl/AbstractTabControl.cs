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

/// <summary>
/// Base class of tab controls: shows the items as a strip of tabs, follows the selected tab and presents its
/// content through <see cref="SelectedContent" />.
/// </summary>
/// <remarks>
/// A derived control supplies the template and may use its own tab item type by overriding
/// <see cref="CreateTabItem" />.
/// </remarks>
[TemplatePart("PART_SelectedItemIndicator", typeof(Border), IsRequired = false)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = false)]
[TemplatePart("PART_TabsContainer", typeof(Control), IsRequired = false)]
[TemplatePart("PART_SelectedContentHost", typeof(ContentPresenter), IsRequired = false)]
public abstract class AbstractTabControl : SelectingItemsControl, IMotionAwareControl
{
    /// <summary>
    /// Identifies the <see cref="TabStripPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<AbstractTabControl, Dock>(nameof(TabStripPlacement), Dock.Top);

    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty =
        AvaloniaProperty.Register<AbstractTabControl, object?>(nameof(HeaderStartExtraContent));

    /// <summary>
    /// Identifies the <see cref="HeaderStartExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<AbstractTabControl, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));

    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty =
        AvaloniaProperty.Register<AbstractTabControl, object?>(nameof(HeaderEndExtraContent));

    /// <summary>
    /// Identifies the <see cref="HeaderEndExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<AbstractTabControl, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IndicatorMode" /> property.
    /// </summary>
    public static readonly StyledProperty<TabIndicatorMode> IndicatorModeProperty =
        AvaloniaProperty.Register<AbstractTabControl, TabIndicatorMode>(nameof(IndicatorMode));

    /// <summary>
    /// Identifies the <see cref="IndicatorFixedLength" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorFixedLengthProperty =
        AvaloniaProperty.Register<AbstractTabControl, double>(nameof(IndicatorFixedLength), 30, validate: static value => double.IsFinite(value) && value >= 0);

    /// <summary>
    /// Identifies the <see cref="IsIndicatorLengthAnimated" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndicatorLengthAnimatedProperty =
        AvaloniaProperty.Register<AbstractTabControl, bool>(nameof(IsIndicatorLengthAnimated));

    /// <summary>
    /// Identifies the <see cref="IsSelectedItemCentered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedItemCenteredProperty =
        AvaloniaProperty.Register<AbstractTabControl, bool>(nameof(IsSelectedItemCentered));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTabControl>();

    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<AbstractTabControl>();

    /// <summary>
    /// Identifies the <see cref="SelectedContent" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractTabControl, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<AbstractTabControl, object?>(nameof(SelectedContent), owner => owner.SelectedContent);

    /// <summary>
    /// Identifies the <see cref="SelectedContentTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractTabControl, IDataTemplate?> SelectedContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<AbstractTabControl, IDataTemplate?>(nameof(SelectedContentTemplate),
            owner => owner.SelectedContentTemplate);

    /// <summary>
    /// Edge of the control where the tab strip is placed. The selection indicator runs horizontally for
    /// <see cref="Dock.Top" /> and <see cref="Dock.Bottom" /> and vertically for <see cref="Dock.Left" /> and
    /// <see cref="Dock.Right" />. The default is <see cref="Dock.Top" />.
    /// </summary>
    /// <remarks>
    /// The value is copied to the <see cref="AbstractTabItem.TabStripPlacement" /> of every tab item.
    /// </remarks>
    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    /// <summary>
    /// Extra content placed at the start of the tab strip header, in front of the tab items. The default is
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
    /// Extra content placed at the end of the tab strip header, after the tab items. The default is
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
    /// <see langword="true" />.
    /// </remarks>
    public bool IsIndicatorLengthAnimated
    {
        get => GetValue(IsIndicatorLengthAnimatedProperty);
        set => SetValue(IsIndicatorLengthAnimatedProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the tab strip scrolls to keep the selected tab centered in its viewport.
    /// The default is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The strip is scrolled when the selection, the item count or the viewport changes, within the
    /// scrollable range. The scroll animates only for a selection change while <see cref="IsMotionEnabled" />
    /// is <see langword="true" />. Centering requires the template part <c>PART_TabsContainer</c> to be a
    /// <c>ScrollViewer</c> or to contain one, and <c>PART_ItemsPresenter</c> to exist.
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
    /// The desktop tab control themes bind the value to the <c>EnableMotion</c> token; clearing a local
    /// value falls back to that token.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Template used to present the content of the selected tab when the tab item has no
    /// <c>ContentTemplate</c> of its own. The default is <see langword="null" />, which leaves the choice
    /// to the content host.
    /// </summary>
    /// <remarks>
    /// In this class, <c>ItemTemplate</c> is copied to the header template of a generated tab item, so it applies
    /// to the tab headers only. The desktop <c>TabControl</c> forwards it to the content template of its tab items
    /// instead, where it takes the place of this property.
    /// </remarks>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Content of the selected tab item, shown by the content host. The control updates it when the selection
    /// or the content of the selected tab item changes. It is <see langword="null" /> when nothing is selected.
    /// </summary>
    public object? SelectedContent
    {
        get => _selectedContent;
        internal set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }

    /// <summary>
    /// Template used to present <see cref="SelectedContent" />: the <c>ContentTemplate</c> of the selected
    /// tab item, or <see cref="ContentTemplate" /> when the tab item has none. It is <see langword="null" />
    /// when nothing is selected.
    /// </summary>
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

    /// <summary>
    /// Selects the enabled tab item that received an unhandled left button release and marks the event as
    /// handled.
    /// </summary>
    /// <remarks>
    /// The base member is called first. Nothing is selected while <see cref="UsesCustomIndicatorTransition" />
    /// is <see langword="true" />; the derived control handles the release itself.
    /// </remarks>
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

    /// <summary>
    /// Creates a tab item through <see cref="CreateTabItem" /> as the container of an item.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateTabItem();

    /// <summary>
    /// Creates the tab item used as the container of an item. Override it to return a derived
    /// <see cref="AbstractTabItem" />; the default is an <see cref="AbstractTabItem" /> without additions.
    /// </summary>
    protected virtual AbstractTabItem CreateTabItem() => new DefaultTabItem();

    /// <summary>
    /// Returns <see langword="true" /> unless the item is itself an <see cref="AbstractTabItem" />, which
    /// then serves as its own container.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractTabItem>(item, out recycleKey);

    /// <summary>
    /// Prepares a tab item for its item: copies <see cref="TabStripPlacement" /> to it and, unless the item
    /// is its own container, sets its <c>Content</c> and <c>Header</c> to the item and its
    /// <c>HeaderTemplate</c> to <c>ItemTemplate</c>. The selected content is refreshed when the container
    /// is the selected one.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then refreshes the selected content when the selected index is the old or the
    /// new index of the moved container.
    /// </summary>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        if (SelectedIndex == oldIndex || SelectedIndex == newIndex)
        {
            UpdateSelectedContent();
        }
    }

    /// <summary>
    /// Registers <paramref name="presenter" /> as the content host when its name is
    /// <c>PART_SelectedContentHost</c>.
    /// </summary>
    /// <param name="presenter">
    /// The presenter found in the template by <see cref="OnApplyTemplate" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the presenter was registered, otherwise <see langword="false" />.
    /// </returns>
    protected virtual bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name != "PART_SelectedContentHost")
        {
            return false;
        }

        ContentPart = presenter;
        return true;
    }

    /// <summary>
    /// Makes <see cref="SelectedContent" /> and <see cref="SelectedContentTemplate" /> follow the content of
    /// the selected tab item, and clears both when no container is selected.
    /// </summary>
    /// <param name="container">
    /// The container whose content is presented. When <see langword="null" />, the container at the
    /// selected index is used.
    /// </param>
    /// <remarks>
    /// The subscriptions to the previously selected container are disposed first. When the resolved
    /// template changes, <see cref="SelectedContentTemplate" /> is cleared before the new value is set,
    /// which releases the controls built by the previous template.
    /// </remarks>
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
    /// <summary>
    /// When <see langword="true" />, the derived control binds the selection indicator to its own template
    /// parts and handles the release of a tab item itself, instead of the base class; <c>BaseTabControl</c>
    /// in AtomUI.Desktop.Controls overrides it to <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false" />: the base class then binds its built-in indicator to the
    /// template when the template is applied, and selects the enabled tab item that receives an unhandled
    /// left button release.
    /// </remarks>
    private protected virtual bool UsesCustomIndicatorTransition => false;

    /// <summary>
    /// Calls the base member, registers the <c>PART_SelectedContentHost</c> presenter and, unless
    /// <see cref="UsesCustomIndicatorTransition" /> is <see langword="true" />, binds the selection
    /// indicator to the parts of the template.
    /// </summary>
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

    /// <summary>
    /// Sets the parts that a derived control supplies to the selection indicator itself. Pass
    /// <see langword="null" /> for a part it does not use; the indicator is attached at once when the
    /// control is in the visual tree.
    /// </summary>
    /// <param name="indicator">
    /// The element that shows the selection indicator.
    /// </param>
    /// <param name="scroll">
    /// The scroll viewer that <see cref="IsSelectedItemCentered" /> uses to center the selected tab.
    /// </param>
    /// <param name="itemsPresenter">
    /// The presenter of the tab items, used as the coordinate reference for the position of the selected
    /// tab.
    /// </param>
    private protected void SetCustomIndicatorParts(Border? indicator, ScrollViewer? scroll, ItemsPresenter? itemsPresenter)
    {
        _tabIndicator ??= new TabSelectionIndicator(this);
        _tabIndicator.SetParts(indicator, scroll, itemsPresenter, IsMotionEnabled);
    }

    /// <summary>
    /// Releases the parts of the selection indicator and stops its animations and scrolling. The indicator
    /// stays inactive until parts are set again through <see cref="SetCustomIndicatorParts" />.
    /// </summary>
    private protected void ResetCustomIndicatorParts() => _tabIndicator?.Reset();

    /// <summary>
    /// Releases the parts of the selection indicator before the base member applies the new template.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _tabIndicator?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Calls the base member, then refreshes the selected content and attaches the selection indicator.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateSelectedContent();
        _tabIndicator?.Attach(IsMotionEnabled);
    }

    /// <summary>
    /// Disposes the subscriptions to the selected tab item, detaches the selection indicator and then
    /// calls the base member. The subscriptions are recreated when the control is attached again.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;
        _tabIndicator?.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Arranges the children through the base member, then updates the selection indicator and the
    /// centering scroll from the arranged bounds of the selected tab. An override must call the base
    /// member to keep both in sync.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        _tabIndicator?.Update(TabStripPlacement, IndicatorMode, IndicatorFixedLength, IsIndicatorLengthAnimated,
            IsSelectedItemCentered, IsMotionEnabled);
        return size;
    }

    /// <summary>
    /// Calls the base member, then follows changed values: a changed <c>SelectedItem</c> refreshes the
    /// selected content, a changed <see cref="ContentTemplate" /> is applied to the selected content when
    /// the tab item has no template of its own, and a changed <see cref="TabStripPlacement" /> is copied
    /// to every tab item.
    /// </summary>
    /// <remarks>
    /// A change of <c>SelectedItem</c>, <see cref="TabStripPlacement" />, <see cref="IndicatorMode" />,
    /// <see cref="IndicatorFixedLength" />, <see cref="IsIndicatorLengthAnimated" />,
    /// <see cref="IsSelectedItemCentered" />, <see cref="IsMotionEnabled" /> or the layout direction also
    /// updates the selection indicator and requests a new arrangement.
    /// </remarks>
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
