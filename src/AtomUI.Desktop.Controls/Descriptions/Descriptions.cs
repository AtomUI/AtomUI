using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Displays label and content pairs in a grid whose column count follows the media breakpoint, below an optional
/// bar with a title and an extra area.
/// The entries are <see cref="DescriptionItem" /> objects in <see cref="Items" /> or imported from
/// <see cref="ItemsSource" />; this control creates the visual of each entry.
/// </summary>
public partial class Descriptions : TemplatedControl, ISizeTypeAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsBordered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsBordered));

    /// <summary>
    /// Identifies the <see cref="IsShowColon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsShowColon), true);

    /// <summary>
    /// Identifies the <see cref="ColumnInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<ResponsiveInt?> ColumnInfoProperty =
        AvaloniaProperty.Register<Descriptions, ResponsiveInt?>(nameof(ColumnInfo));

    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<Descriptions, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<Descriptions, IDataTemplate?>(nameof(ExtraTemplate));

    /// <summary>
    /// Identifies the <see cref="Layout" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> LayoutProperty =
        AvaloniaProperty.Register<Descriptions, Orientation>(nameof(Layout));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Descriptions>();

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<Descriptions, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<Descriptions, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<Descriptions, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Gets or sets whether the items are drawn as bordered cells inside a bordered item area.
    /// </summary>
    /// <remarks>
    /// In the horizontal layout each item becomes a label cell followed by a content cell, as in a table, and
    /// <see cref="ColumnInfo" /> still counts items per row. In the vertical layout each item becomes a box with a
    /// divider between its label and its content. Without borders, a gap that depends on <see cref="SizeType" />
    /// separates the item rows.
    /// </remarks>
    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a colon is shown after the label of each item. The default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// It has no effect in the horizontal layout with <see cref="IsBordered" /> set, whose label cells show no colon.
    /// </remarks>
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }

    /// <summary>
    /// Gets or sets how many items a row holds, as one number or as a number per media breakpoint.
    /// </summary>
    /// <remarks>
    /// The breakpoint is taken from the closest surrounding element that provides one; without any, <c>xxl</c> is used.
    /// A breakpoint takes the value of the nearest configured breakpoint at or below it, and otherwise the default:
    /// 1 on <c>xs</c>, 2 on <c>sm</c>, 4 on <c>xxxl</c> and 3 on the others. <see langword="null" />, the default,
    /// uses these values for every breakpoint.
    /// </remarks>
    public ResponsiveInt? ColumnInfo
    {
        get => GetValue(ColumnInfoProperty);
        set => SetValue(ColumnInfoProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown at the right end of the bar above the items, next to the <see cref="Header" />.
    /// </summary>
    /// <remarks>
    /// That bar is shown only while this property or <see cref="Header" /> is not <see langword="null" />.
    /// </remarks>
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that presents <see cref="Extra" />.
    /// </summary>
    /// <remarks>
    /// When it is <see langword="null" />, the data templates of the control are used, and content without a matching
    /// template is shown as text.
    /// </remarks>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether each item places its label to the left of its content or above it.
    /// </summary>
    /// <remarks>
    /// With <see cref="IsBordered" /> set, the horizontal layout turns the items into table cells instead. A change
    /// rebuilds the visuals of the items once the control is attached to the visual tree.
    /// </remarks>
    public Orientation Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the size that this control passes to the visual of every item.
    /// The default is <c>SizeType.Large</c>.
    /// </summary>
    /// <remarks>
    /// The size selects the padding of bordered items and cells, and the gap between the rows of items without a
    /// border.
    /// </remarks>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the title shown at the left of the bar above the items.
    /// </summary>
    /// <remarks>
    /// That bar is shown only while this property or <see cref="Extra" /> is not <see langword="null" />.
    /// </remarks>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that presents <see cref="Header" />.
    /// </summary>
    /// <remarks>
    /// When it is <see langword="null" />, the data templates of the control are used, and content without a matching
    /// template is shown as text.
    /// </remarks>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a source whose <see cref="DescriptionItem" /> objects are copied into <see cref="Items" />.
    /// </summary>
    /// <remarks>
    /// The source is read once, when this property is set: <see cref="Items" /> is cleared and every
    /// <see cref="DescriptionItem" /> in the source is added to it. Objects of other types are skipped, later changes
    /// of the source are not observed, and <see langword="null" /> leaves <see cref="Items" /> unchanged.
    /// </remarks>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private DescriptionItems _items = new();

    /// <summary>
    /// Gets or sets the entries that this control displays. This is the XAML content property.
    /// </summary>
    /// <remarks>
    /// Assigning another collection replaces the entries; while the control is attached, the previous ones are detached
    /// and the new ones are attached to it as their resource host. <see langword="null" /> is replaced by an empty
    /// collection. Adding, removing and resetting entries keeps the visuals of the items in sync; once the control is
    /// attached and its template is applied, moving or replacing an entry throws a
    /// <see cref="NotSupportedException" />. The last entry, and every entry whose <c>IsFilled</c> is set, fills the
    /// rest of its row.
    /// </remarks>
    [Content]
    public DescriptionItems Items
    {
        get => _items;
        set
        {
            if (ReferenceEquals(_items, value))
            {
                return;
            }

            _items.CollectionChanged -= HandleCollectionChanged;
            DetachAllDescriptionItems();
            _items = value ?? new DescriptionItems();
            _items.CollectionChanged += HandleCollectionChanged;
            if (this.IsAttachedToVisualTree())
            {
                AttachDescriptionItems(_items);
            }

            if (_gridLayout != null && this.IsAttachedToVisualTree())
            {
                _gridLayout.Children.Clear();
                AddDescriptionItems((IEnumerable<DescriptionItem>)_items);
                DoLayoutChildren();
                InvalidateMeasure();
            }
        }
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Descriptions, bool> IsHeaderLayoutVisibleProperty =
        AvaloniaProperty.RegisterDirect<Descriptions, bool>(nameof(IsHeaderLayoutVisible),
            o => o.IsHeaderLayoutVisible,
            (o, v) => o.IsHeaderLayoutVisible = v);

    private bool _isHeaderLayoutVisible;

    internal bool IsHeaderLayoutVisible
    {
        get => _isHeaderLayoutVisible;
        set => SetAndRaise(IsHeaderLayoutVisibleProperty, ref _isHeaderLayoutVisible, value);
    }

    #endregion

    private Grid? _gridLayout;
    private MediaBreakPoint? _breakPoint;
    private int _effectiveColumns;
    private IMediaBreakAwareControl? _mediaOwner;
    private readonly Dictionary<DescriptionItem, DescriptionItemAttachment> _itemAttachments = new(ReferenceEqualityComparer.Instance);

    static Descriptions()
    {
        SizeTypeProperty.OverrideDefaultValue<Descriptions>(SizeType.Large);
    }

    public Descriptions()
    {
        _items.CollectionChanged += HandleCollectionChanged;
    }

    /// <summary>
    /// Called when the control is attached to the visual tree. After the base member has run, it starts following the
    /// media breakpoint of the closest surrounding element that provides one, attaches the entries of
    /// <see cref="Items" /> to this control as their resource host, and applies the column count of that breakpoint.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachMediaOwner(MediaQueryHost.FindOwner(this));
        AttachDescriptionItems(Items);
        UpdateGridColumnsForCurrentBreakPoint(true);
    }

    /// <summary>
    /// Called when the control is detached from the visual tree. After the base member has run, it detaches every entry
    /// of <see cref="Items" /> and stops following the media breakpoint.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachAllDescriptionItems();
        DetachMediaOwner();
    }

    /// <summary>
    /// Called when the control template is applied. After the base member has run, it takes the grid of the template,
    /// creates the visual of every entry in <see cref="Items" /> for the current <see cref="Layout" /> and
    /// <see cref="IsBordered" />, and applies the column count of the current breakpoint.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _gridLayout = e.NameScope.Find<Grid>("PART_GridLayout");
        RebuildDescriptionItems();
        UpdateGridColumnsForCurrentBreakPoint(true);
    }

    /// <summary>
    /// Called when a property of this control changes. After the base member has run, it reacts to changes of
    /// <see cref="ItemsSource" />, <see cref="IsBordered" />, <see cref="Header" />, <see cref="Extra" /> and
    /// <see cref="ColumnInfo" />, and to <see cref="Layout" /> while the control is attached to the visual tree.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            HandleItemsSourceChanged();
        }
        else if (change.Property == IsBorderedProperty)
        {
            HandleBorderedChanged();
        }
        else if (change.Property == HeaderProperty ||
                 change.Property == ExtraProperty)
        {
            SetCurrentValue(IsHeaderLayoutVisibleProperty, Header != null || Extra != null);
        }
        else if (change.Property == ColumnInfoProperty)
        {
            HandleColumnInfoChanged();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == LayoutProperty)
            {
                HandleLayoutChanged();
            }
        }
    }

    /// <summary>
    /// Called when <see cref="Items" /> reports a change. It updates the attachments of the entries and, once the
    /// control is attached and its template is applied, adds, removes or rebuilds the visuals of the items to match
    /// the collection, then lays the items out again.
    /// </summary>
    /// <param name="sender">
    /// The collection that changed.
    /// </param>
    /// <param name="e">
    /// The change that was reported.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The change moves or replaces an entry while the control is attached and its template is applied.
    /// </exception>
    /// <remarks>
    /// An override must call the base member to keep the attachments and the item visuals in sync with the collection.
    /// </remarks>
    protected virtual void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var isAttached       = this.IsAttachedToVisualTree();
        var canUpdateVisuals = _gridLayout != null && isAttached;
        if (canUpdateVisuals &&
            (e.Action == NotifyCollectionChangedAction.Move ||
             e.Action == NotifyCollectionChangedAction.Replace))
        {
            throw new NotSupportedException();
        }

        if (isAttached)
        {
            UpdateDescriptionItemAttachments(e);
        }

        if (!canUpdateVisuals)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    AddDescriptionItems(e.NewItems, e.NewStartingIndex);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    RemoveDescriptionItems(e.OldItems, e.OldStartingIndex);
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                _gridLayout!.Children.Clear();
                AddDescriptionItems(Items);
                break;
        }

        DoLayoutChildren();
        InvalidateMeasure();
    }

    private void AttachDescriptionItems(IEnumerable<DescriptionItem> items)
    {
        foreach (var item in items)
        {
            AttachDescriptionItem(item);
        }
    }

    private void AttachDescriptionItemsFromList(IList items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is DescriptionItem item)
            {
                AttachDescriptionItem(item);
            }
        }
    }

    private void DetachDescriptionItemsFromList(IList items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is DescriptionItem item)
            {
                DetachDescriptionItem(item);
            }
        }
    }

    private void AttachDescriptionItem(DescriptionItem item)
    {
        if (_itemAttachments.TryGetValue(item, out var attachment))
        {
            attachment.ReferenceCount++;
            return;
        }

        item.PropertyChanged += HandleDescriptionItemPropertyChanged;
        _itemAttachments.Add(item, new DescriptionItemAttachment(item.AttachResourceHost(this)));
    }

    private void DetachDescriptionItem(DescriptionItem item)
    {
        if (!_itemAttachments.TryGetValue(item, out var attachment))
        {
            return;
        }

        attachment.ReferenceCount--;
        if (attachment.ReferenceCount > 0)
        {
            return;
        }

        item.PropertyChanged -= HandleDescriptionItemPropertyChanged;
        attachment.Dispose();
        _itemAttachments.Remove(item);
    }

    private void DetachAllDescriptionItems()
    {
        foreach (var (item, attachment) in _itemAttachments)
        {
            item.PropertyChanged -= HandleDescriptionItemPropertyChanged;
            attachment.Dispose();
        }

        _itemAttachments.Clear();
    }

    private void ResetDescriptionItemAttachments()
    {
        DetachAllDescriptionItems();
        AttachDescriptionItems(Items);
    }

    private void UpdateDescriptionItemAttachments(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    AttachDescriptionItemsFromList(e.NewItems);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    DetachDescriptionItemsFromList(e.OldItems);
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                ResetDescriptionItemAttachments();
                break;
        }
    }

    private void HandleDescriptionItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not DescriptionItem item)
        {
            return;
        }

        if (e.Property == DescriptionItem.LabelProperty ||
            e.Property == DescriptionItem.ContentProperty)
        {
            UpdateGeneratedDescriptionItem(item);
        }
        else if (e.Property == DescriptionItem.SpanProperty ||
                 e.Property == DescriptionItem.IsFilledProperty)
        {
            DoLayoutChildren();
            InvalidateMeasure();
        }
    }

    private void UpdateGeneratedDescriptionItem(DescriptionItem item)
    {
        if (_gridLayout == null)
        {
            return;
        }

        for (var i = 0; i < Items.Count; i++)
        {
            if (!ReferenceEquals(Items[i], item))
            {
                continue;
            }

            UpdateGeneratedDescriptionItem(i, item);
        }
    }

    private void UpdateGeneratedDescriptionItem(int itemIndex, DescriptionItem item)
    {
        Debug.Assert(_gridLayout != null);
        if (UsesHorizontalBorderedLayout())
        {
            var gridChildIndex = itemIndex * 2;
            if (gridChildIndex < _gridLayout.Children.Count &&
                _gridLayout.Children[gridChildIndex] is DescriptionBorderedItemLabel itemLabel)
            {
                itemLabel.Content = item.Label;
            }

            var contentChildIndex = gridChildIndex + 1;
            if (contentChildIndex < _gridLayout.Children.Count &&
                _gridLayout.Children[contentChildIndex] is DescriptionBorderedItemContent itemContent)
            {
                itemContent.Content = item.Content;
            }
        }
        else if (itemIndex < _gridLayout.Children.Count &&
                 _gridLayout.Children[itemIndex] is DescriptionDefaultItem defaultItem)
        {
            defaultItem.Header  = item.Label;
            defaultItem.Content = item.Content;
        }
    }

    private void AttachMediaOwner(IMediaBreakAwareControl? mediaOwner)
    {
        if (ReferenceEquals(_mediaOwner, mediaOwner))
        {
            if (_mediaOwner != null)
            {
                _breakPoint = _mediaOwner.MediaBreakPoint;
            }

            return;
        }

        DetachMediaOwner();
        if (mediaOwner == null)
        {
            return;
        }

        _mediaOwner = mediaOwner;
        _breakPoint = mediaOwner.MediaBreakPoint;
        mediaOwner.MediaBreakPointChanged += HandleMediaBreakChanged;
    }

    private void DetachMediaOwner()
    {
        if (_mediaOwner != null)
        {
            _mediaOwner.MediaBreakPointChanged -= HandleMediaBreakChanged;
        }

        _mediaOwner = null;
    }

    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        UpdateGridColumnsForCurrentBreakPoint(true);
        InvalidateMeasure();
    }

    private void HandleItemsSourceChanged()
    {
        if (ItemsSource == null)
        {
            return;
        }

        Items.Clear();
        AddItemsSourceItems(ItemsSource);
    }

    private void HandleBorderedChanged()
    {
        if (_gridLayout == null)
        {
            return;
        }

        if (Layout == Orientation.Horizontal)
        {
            RebuildDescriptionItems();
        }

        UpdateGridColumnsForCurrentBreakPoint(true);
        InvalidateMeasure();
    }

    private void HandleColumnInfoChanged()
    {
        if (_breakPoint.HasValue)
        {
            UpdateGridColumns(GetColumnsForMediaBreak(_breakPoint.Value), true);
            InvalidateMeasure();
        }
    }

    private void HandleLayoutChanged()
    {
        RebuildDescriptionItems();
        UpdateGridColumnsForCurrentBreakPoint(true);
        InvalidateMeasure();
    }

    private void AddItemsSourceItems(IEnumerable source)
    {
        var items = CollectItemsSourceItems(source);
        if (items is not null)
        {
            Items.AddRange(items);
        }
    }

    private static List<DescriptionItem>? CollectItemsSourceItems(IEnumerable source)
    {
        List<DescriptionItem>? items = null;
        if (source is IList list)
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i] is DescriptionItem item)
                {
                    items ??= new List<DescriptionItem>(list.Count - i);
                    items.Add(item);
                }
            }

            return items;
        }

        if (source is IReadOnlyList<DescriptionItem> readOnlyList)
        {
            for (var i = 0; i < readOnlyList.Count; ++i)
            {
                if (readOnlyList[i] is DescriptionItem item)
                {
                    items ??= new List<DescriptionItem>(readOnlyList.Count - i);
                    items.Add(item);
                }
            }

            return items;
        }

        foreach (var value in source)
        {
            if (value is DescriptionItem item)
            {
                items ??= new List<DescriptionItem>();
                items.Add(item);
            }
        }

        return items;
    }

    private void RebuildDescriptionItems()
    {
        if (_gridLayout == null)
        {
            return;
        }

        _gridLayout.Children.Clear();
        AddDescriptionItems(Items);
    }

    private void AddDescriptionItems(IEnumerable<DescriptionItem> items)
    {
        if (_gridLayout == null)
        {
            return;
        }

        foreach (var item in items)
        {
            AddDescriptionItem(item);
        }
    }

    private void AddDescriptionItems(IList items, int startingIndex)
    {
        if (_gridLayout == null)
        {
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            AddDescriptionItem((DescriptionItem)items[i]!, GetGridChildIndex(startingIndex + i));
        }
    }

    private void RemoveDescriptionItems(IList items, int startingIndex)
    {
        if (_gridLayout != null)
        {
            var removeIndex = GetGridChildIndex(startingIndex);
            var removeCount = GetGridChildCount(items.Count);
            for (var i = 0; i < removeCount && removeIndex < _gridLayout.Children.Count; i++)
            {
                _gridLayout.Children.RemoveAt(removeIndex);
            }
        }
    }

    private void AddDescriptionItem(DescriptionItem item, int? gridChildIndex = null)
    {
        if (_gridLayout == null)
        {
            return;
        }

        if (UsesHorizontalBorderedLayout())
        {
            AddHorizontalBorderedDescriptionItem(item, gridChildIndex);
        }
        else
        {
            AddDefaultDescriptionItem(item, gridChildIndex);
        }
    }

    private void AddHorizontalBorderedDescriptionItem(DescriptionItem item, int? gridChildIndex)
    {
        var itemLabel   = new DescriptionBorderedItemLabel();
        var itemContent = new DescriptionBorderedItemContent();
        itemLabel[!SizeTypeProperty]   = this[!SizeTypeProperty];
        itemContent[!SizeTypeProperty] = this[!SizeTypeProperty];
        itemLabel.Content              = item.Label;
        itemContent.Content            = item.Content;
        AddGridChild(itemLabel, gridChildIndex);
        AddGridChild(itemContent, gridChildIndex.HasValue ? gridChildIndex.Value + 1 : null);
    }

    private void AddDefaultDescriptionItem(DescriptionItem item, int? gridChildIndex)
    {
        var descriptionDefaultItem = new DescriptionDefaultItem
        {
            Layout  = Layout,
            Header  = item.Label,
            Content = item.Content
        };
        descriptionDefaultItem[!DescriptionDefaultItem.SizeTypeProperty] = this[!SizeTypeProperty];
        descriptionDefaultItem[!DescriptionDefaultItem.IsColonVisibleProperty] = this[!IsShowColonProperty];
        if (Layout == Orientation.Vertical)
        {
            descriptionDefaultItem[!DescriptionDefaultItem.IsBorderedProperty] = this[!IsBorderedProperty];
        }

        AddGridChild(descriptionDefaultItem, gridChildIndex);
    }

    private void AddGridChild(Control child, int? gridChildIndex)
    {
        if (_gridLayout == null)
        {
            return;
        }

        child.Classes.Add("semantic-scope-item");
        if (gridChildIndex.HasValue && gridChildIndex.Value <= _gridLayout.Children.Count)
        {
            _gridLayout.Children.Insert(gridChildIndex.Value, child);
        }
        else
        {
            _gridLayout.Children.Add(child);
        }
    }

    private int GetGridChildIndex(int itemIndex)
    {
        return UsesHorizontalBorderedLayout() ? itemIndex * 2 : itemIndex;
    }

    private int GetGridChildCount(int itemCount)
    {
        return UsesHorizontalBorderedLayout() ? itemCount * 2 : itemCount;
    }

    private bool UsesHorizontalBorderedLayout()
    {
        return Layout == Orientation.Horizontal && IsBordered;
    }

    private void UpdateGridColumns(int columnCount, bool forceLayout = false)
    {
        var effectiveColumns = GetEffectiveColumnCount(columnCount);

        if (effectiveColumns != _effectiveColumns)
        {
            _effectiveColumns = effectiveColumns;
            forceLayout       = true;
        }

        if (forceLayout)
        {
            DoLayoutChildren();
        }
    }

    private void UpdateGridColumnsForCurrentBreakPoint(bool forceLayout)
    {
        var breakPoint = ResolveCurrentBreakPoint();
        var columns    = GetColumnsForMediaBreak(breakPoint);
        UpdateGridColumns(columns, forceLayout);
    }

    private int GetEffectiveColumnCount(int columnCount)
    {
        return UsesHorizontalBorderedLayout() ? columnCount * 2 : columnCount;
    }

    private void DoLayoutChildren()
    {
        if (_gridLayout == null)
        {
            return;
        }

        EnsureEffectiveColumns();
        var row    = 0;
        var column = 0;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (UsesHorizontalBorderedLayout())
            {
                LayoutHorizontalBorderedDescriptionItem(i, item, ref row, ref column);
            }
            else
            {
                LayoutDefaultDescriptionItem(i, item, ref row, ref column);
            }
        }

        ConfigureLastRowState(row);
        ConfigureGridDefinitions(row);
    }

    private void LayoutHorizontalBorderedDescriptionItem(int itemIndex,
                                                         DescriptionItem item,
                                                         ref int row,
                                                         ref int column)
    {
        Debug.Assert(_gridLayout != null);
        var gridChildIndex = itemIndex * 2;
        if (_gridLayout.Children[gridChildIndex] is DescriptionBorderedItemLabel itemLabel)
        {
            Grid.SetRow(itemLabel, row);
            Grid.SetColumn(itemLabel, column);
            column += 1;
        }

        if (_gridLayout.Children[gridChildIndex + 1] is DescriptionBorderedItemContent itemContent)
        {
            var itemSpan = GetFillAwareItemSpan(item, itemIndex, column, GetItemSpan(item.Span) * 2 - 1);
            Grid.SetRow(itemContent, row);
            Grid.SetColumn(itemContent, column);
            Grid.SetColumnSpan(itemContent, itemSpan);
            column += itemSpan;
            if (column >= _effectiveColumns)
            {
                column                   = 0;
                itemContent.IsLastColumn = true;
                ++row;
            }
            else
            {
                itemContent.IsLastColumn = false;
            }
        }
    }

    private void LayoutDefaultDescriptionItem(int itemIndex, DescriptionItem item, ref int row, ref int column)
    {
        Debug.Assert(_gridLayout != null);
        if (_gridLayout.Children[itemIndex] is not DescriptionDefaultItem defaultItem)
        {
            return;
        }

        var itemSpan = GetFillAwareItemSpan(item, itemIndex, column, GetItemSpan(item.Span));
        Grid.SetRow(defaultItem, row);
        Grid.SetColumn(defaultItem, column);
        Grid.SetColumnSpan(defaultItem, itemSpan);
        column += itemSpan;
        if (column >= _effectiveColumns)
        {
            column = 0;
            if (Layout == Orientation.Vertical)
            {
                defaultItem.IsLastColumn = true;
            }
            ++row;
        }
        else if (Layout == Orientation.Vertical)
        {
            defaultItem.IsLastColumn = false;
        }
    }

    private int GetFillAwareItemSpan(DescriptionItem item, int itemIndex, int column, int requestedSpan)
    {
        if (itemIndex == Items.Count - 1 || item.IsFilled)
        {
            return _effectiveColumns - column;
        }

        return Math.Max(1, Math.Min(_effectiveColumns - column, requestedSpan));
    }

    private void ConfigureLastRowState(int rowCount)
    {
        Debug.Assert(_gridLayout != null);
        var lastRow = rowCount - 1;
        for (var i = 0; i < Items.Count; i++)
        {
            if (UsesHorizontalBorderedLayout())
            {
                var gridChildIndex = i * 2;
                if (_gridLayout.Children[gridChildIndex] is DescriptionBorderedItemLabel itemLabel)
                {
                    itemLabel.IsLastRow = Grid.GetRow(itemLabel) == lastRow;
                }

                if (_gridLayout.Children[gridChildIndex + 1] is DescriptionBorderedItemContent itemContent)
                {
                    itemContent.IsLastRow = Grid.GetRow(itemContent) == lastRow;
                }
            }
            else if (Layout == Orientation.Vertical &&
                     _gridLayout.Children[i] is DescriptionDefaultItem defaultItem)
            {
                defaultItem.IsLastRow = Grid.GetRow(defaultItem) == lastRow;
            }
        }
    }

    private void ConfigureGridDefinitions(int rowCount)
    {
        Debug.Assert(_gridLayout != null);
        _gridLayout.ColumnDefinitions.Clear();
        var columnDefinitions = new ColumnDefinitions();
        for (var i = 0; i < _effectiveColumns; i++)
        {
            columnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        _gridLayout.ColumnDefinitions = columnDefinitions;
        _gridLayout.RowDefinitions.Clear();
        var rowDefinitions = new RowDefinitions();
        for (var i = 0; i < rowCount; i++)
        {
            rowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        _gridLayout.RowDefinitions = rowDefinitions;
    }

    private void EnsureEffectiveColumns()
    {
        if (_effectiveColumns > 0)
        {
            return;
        }

        var breakPoint   = ResolveCurrentBreakPoint();
        var columnCount  = GetColumnsForMediaBreak(breakPoint);
        _effectiveColumns = GetEffectiveColumnCount(columnCount);
    }

    private MediaBreakPoint ResolveCurrentBreakPoint()
    {
        var breakPoint = _breakPoint;
        if (breakPoint == null && _mediaOwner != null)
        {
            breakPoint = _mediaOwner.MediaBreakPoint;
        }
        if (breakPoint == null && MediaQueryHost.FindOwner(this) is { } mediaOwner)
        {
            breakPoint = mediaOwner.MediaBreakPoint;
        }

        breakPoint ??= MediaBreakPoint.ExtraExtraLarge;
        _breakPoint = breakPoint;
        return breakPoint.Value;
    }

    private int GetColumnsForMediaBreak(MediaBreakPoint breakPoint)
    {
        var fallback = GetDefaultColumnsForMediaBreak(breakPoint);
        return ColumnInfo?.Resolve(breakPoint, fallback) ?? fallback;
    }

    private static int GetDefaultColumnsForMediaBreak(MediaBreakPoint breakPoint)
    {
        return breakPoint switch
        {
            MediaBreakPoint.ExtraSmall => 1,
            MediaBreakPoint.Small => 2,
            MediaBreakPoint.ExtraExtraExtraLarge => 4,
            _ => 3
        };
    }

    private int GetItemSpan(ResponsiveInt breakInfo)
    {
        Debug.Assert(_breakPoint != null);
        return breakInfo.Resolve(_breakPoint.Value, 1);
    }

    private sealed class DescriptionItemAttachment : IDisposable
    {
        private readonly IDisposable _resourceHostAttachment;

        public int ReferenceCount { get; set; } = 1;

        public DescriptionItemAttachment(IDisposable resourceHostAttachment)
        {
            _resourceHostAttachment = resourceHostAttachment;
        }

        public void Dispose()
        {
            _resourceHostAttachment.Dispose();
        }
    }
}
