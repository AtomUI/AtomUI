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

public partial class Descriptions : TemplatedControl, ISizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsBordered));

    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsShowColon), true);

    public static readonly StyledProperty<ResponsiveInt?> ColumnInfoProperty =
        AvaloniaProperty.Register<Descriptions, ResponsiveInt?>(nameof(ColumnInfo));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<Descriptions, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<Descriptions, IDataTemplate?>(nameof(ExtraTemplate));

    public static readonly StyledProperty<Orientation> LayoutProperty =
        AvaloniaProperty.Register<Descriptions, Orientation>(nameof(Layout));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Descriptions>();

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<Descriptions, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<Descriptions, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<Descriptions, IEnumerable?>(nameof(ItemsSource));

    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }

    public ResponsiveInt? ColumnInfo
    {
        get => GetValue(ColumnInfoProperty);
        set => SetValue(ColumnInfoProperty, value);
    }

    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    public Orientation Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private DescriptionItems _items = new();

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachMediaOwner(MediaQueryHost.FindOwner(this));
        AttachDescriptionItems(Items);
        UpdateGridColumnsForCurrentBreakPoint(true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachAllDescriptionItems();
        DetachMediaOwner();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _gridLayout = e.NameScope.Find<Grid>("PART_GridLayout");
        RebuildDescriptionItems();
        UpdateGridColumnsForCurrentBreakPoint(true);
    }

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
