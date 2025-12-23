using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class Descriptions : TemplatedControl,
                            IControlSharedTokenResourcesHost,
                            ISizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsBordered));

    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Descriptions, bool>(nameof(IsShowColon), true);

    public static readonly StyledProperty<DescriptionsMediaBreakInfo> ColumnInfoProperty =
        AvaloniaProperty.Register<Descriptions, DescriptionsMediaBreakInfo>(nameof(ColumnInfo),
            new DescriptionsMediaBreakInfo(3));

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

    public DescriptionsMediaBreakInfo ColumnInfo
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

    [Content] public DescriptionItems Items { get; set; } = new();

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

    string IControlSharedTokenResourcesHost.TokenId => DescriptionsToken.ID;
    Control IControlSharedTokenResourcesHost.HostControl => this;

    #endregion

    private Grid? _gridLayout;
    private MediaBreakPoint? _breakPoint;
    private int _effectiveColumns;
    private readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();

    static Descriptions()
    {
        SizeTypeProperty.OverrideDefaultValue<Descriptions>(SizeType.Large);
    }

    public Descriptions()
    {
        this.RegisterResources();
        Items.CollectionChanged += HandleCollectionChanged;
    }

    protected virtual void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_gridLayout != null && this.IsAttachedToVisualTree())
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        AddDescriptionItems(e.NewItems.OfType<DescriptionItem>().ToList());
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        RemoveDescriptionItems(e.OldItems.OfType<DescriptionItem>().ToList());
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }

            DoLayoutChildren();
            InvalidateMeasure();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.MediaBreakPointChanged -= HandleMediaBreakChanged;
        }
    }

    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        if (_breakPoint != null)
        {
            var columns = GetColumnsForMediaBreak(_breakPoint.Value);
            UpdateGridColumns(columns);
        }
    }

    private int GetColumnsForMediaBreak(MediaBreakPoint breakPoint)
    {
        var columns = 1;
        if (breakPoint == MediaBreakPoint.ExtraSmall)
        {
            columns = ColumnInfo.ExtraSmall;
        }
        else if (breakPoint == MediaBreakPoint.Small)
        {
            columns = ColumnInfo.Small;
        }
        else if (breakPoint == MediaBreakPoint.Medium)
        {
            columns = ColumnInfo.Medium;
        }
        else if (breakPoint == MediaBreakPoint.Large)
        {
            columns = ColumnInfo.Large;
        }
        else if (breakPoint == MediaBreakPoint.ExtraLarge)
        {
            columns = ColumnInfo.ExtraLarge;
        }
        else if (breakPoint == MediaBreakPoint.ExtraExtraLarge)
        {
            columns = ColumnInfo.ExtraExtraLarge;
        }

        return columns;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _gridLayout = e.NameScope.Find<Grid>(DescriptionsThemeConstants.GridLayoutPart);
        AddDescriptionItems(Items.ToList());
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            _breakPoint = window.MediaBreakPoint;
            var columns = GetColumnsForMediaBreak(_breakPoint.Value);
            UpdateGridColumns(columns);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            if (ItemsSource != null)
            {
                Items.Clear();
                Items.AddRange(ItemsSource.OfType<DescriptionItem>().ToList());
            }
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

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == LayoutProperty)
            {
                HandleLayoutChanged();
            }
        }
    }

    private void AddDescriptionItems(List<DescriptionItem> items)
    {
        if (_gridLayout != null)
        {
            foreach (DescriptionItem item in items)
            {
                if (Layout == Orientation.Horizontal)
                {
                    if (IsBordered)
                    {
                        var itemLabel = new DescriptionBorderedItemLabel();
                        {
                            var disposables = new CompositeDisposable(2);
                            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, itemLabel, SizeTypeProperty));
                            if (_itemsBindingDisposables.TryGetValue(itemLabel, out var oldDisposables))
                            {
                                oldDisposables.Dispose();
                                _itemsBindingDisposables.Remove(itemLabel);
                            }

                            _itemsBindingDisposables.Add(itemLabel, disposables);
                        }

                        var itemContent = new DescriptionBorderedItemContent();
                        {
                            var disposables = new CompositeDisposable(2);
                            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, itemContent, SizeTypeProperty));
                            if (_itemsBindingDisposables.TryGetValue(itemContent, out var oldDisposables))
                            {
                                oldDisposables.Dispose();
                                _itemsBindingDisposables.Remove(itemContent);
                            }

                            _itemsBindingDisposables.Add(itemContent, disposables);
                        }

                        itemLabel.Content   = item.Label;
                        itemContent.Content = item.Content;
                        _gridLayout.Children.Add(itemLabel);
                        _gridLayout.Children.Add(itemContent);
                    }
                    else
                    {
                        var disposables           = new CompositeDisposable(2);
                        var descriptionDefaultItem = new DescriptionDefaultItem();
                        descriptionDefaultItem.Layout = Orientation.Horizontal;
                        disposables.Add(BindUtils.RelayBind(this, IsShowColonProperty, descriptionDefaultItem,
                            DescriptionDefaultItem.IsColonVisibleProperty));
                        descriptionDefaultItem.Header  = item.Label;
                        descriptionDefaultItem.Content = item.Content;
                        _gridLayout.Children.Add(descriptionDefaultItem);
                        if (_itemsBindingDisposables.TryGetValue(descriptionDefaultItem, out var oldDisposables))
                        {
                            oldDisposables.Dispose();
                            _itemsBindingDisposables.Remove(descriptionDefaultItem);
                        }

                        _itemsBindingDisposables.Add(descriptionDefaultItem, disposables);
                    }
                }
                else
                {
                    var disposables           = new CompositeDisposable(2);
                    var descriptionDefaultItem = new DescriptionDefaultItem();
                    descriptionDefaultItem.Layout = Orientation.Vertical;
                    disposables.Add(BindUtils.RelayBind(this, IsShowColonProperty, descriptionDefaultItem,
                        DescriptionDefaultItem.IsColonVisibleProperty));
                    disposables.Add(BindUtils.RelayBind(this, IsBorderedProperty, descriptionDefaultItem,
                        DescriptionDefaultItem.IsBorderedProperty));
                    descriptionDefaultItem.Header  = item.Label;
                    descriptionDefaultItem.Content = item.Content;
                    _gridLayout.Children.Add(descriptionDefaultItem);
                    if (_itemsBindingDisposables.TryGetValue(descriptionDefaultItem, out var oldDisposables))
                    {
                        oldDisposables.Dispose();
                        _itemsBindingDisposables.Remove(descriptionDefaultItem);
                    }

                    _itemsBindingDisposables.Add(descriptionDefaultItem, disposables);
                }
            }
        }
    }

    private void RemoveDescriptionItems(List<DescriptionItem> items)
    {
        if (_gridLayout != null)
        {
            foreach (DescriptionItem item in items)
            {
                var index = Items.IndexOf(item);
                if (index != -1)
                {
                    if (Layout == Orientation.Horizontal)
                    {
                        if (IsBordered)
                        {
                            if (_gridLayout.Children[index] is DescriptionBorderedItemLabel itemLabel)
                            {
                                _gridLayout.Children.Remove(itemLabel);
                                if (_itemsBindingDisposables.TryGetValue(itemLabel, out var disposable))
                                {
                                    disposable.Dispose();
                                    _itemsBindingDisposables.Remove(itemLabel);
                                }
                            }

                            if (_gridLayout.Children[index + 1] is DescriptionBorderedItemContent itemContent)
                            {
                                _gridLayout.Children.Remove(itemContent);
                                if (_itemsBindingDisposables.TryGetValue(itemContent, out var disposable))
                                {
                                    disposable.Dispose();
                                    _itemsBindingDisposables.Remove(itemContent);
                                }
                            }
                        }
                        else
                        {
                            if (_gridLayout.Children[index] is DescriptionDefaultItem defaultItem)
                            {
                                _gridLayout.Children.Remove(defaultItem);
                                if (_itemsBindingDisposables.TryGetValue(defaultItem, out var disposable))
                                {
                                    disposable.Dispose();
                                    _itemsBindingDisposables.Remove(defaultItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_gridLayout.Children[index] is DescriptionDefaultItem defaultItem)
                        {
                            _gridLayout.Children.Remove(defaultItem);
                            if (_itemsBindingDisposables.TryGetValue(defaultItem, out var disposable))
                            {
                                disposable.Dispose();
                                _itemsBindingDisposables.Remove(defaultItem);
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateGridColumns(int columnCount)
    {
    
        if (columnCount != _effectiveColumns)
        {
            if (Layout == Orientation.Horizontal)
            {
                if (IsBordered)
                {
                    _effectiveColumns = columnCount * 2;
                }
                else
                {
                    _effectiveColumns = columnCount;
                }
            }
            else
            {
                _effectiveColumns = columnCount;
            }
            DoLayoutChildren();
        }
    }

    private void HandleBorderedChanged()
    {
        if (Layout == Orientation.Horizontal)
        {
            if (_breakPoint == null)
            {
                if (TopLevel.GetTopLevel(this) is Window window)
                {
                    _breakPoint = window.MediaBreakPoint;
                }
            }

            if (_breakPoint == null)
            {
                return;
            }

            var columns = GetColumnsForMediaBreak(_breakPoint.Value);
            UpdateGridColumns(columns);
        }
    }

    private void DoLayoutChildren()
    {
        if (_gridLayout != null)
        {
            var row    = 0;
            var column = 0;
            for (var i = 0; i < Items.Count; i++)
            {
                var item  = Items[i];
                var index = Items.IndexOf(item);
                if (index != -1)
                {
                    if (Layout == Orientation.Horizontal)
                    {
                        if (IsBordered)
                        {
                            index *= 2;
                            if (_gridLayout.Children[index] is DescriptionBorderedItemLabel itemLabel)
                            {
                                Grid.SetRow(itemLabel, row);
                                Grid.SetColumn(itemLabel, column);
                                column += 1;
                            }

                            if (_gridLayout.Children[index + 1] is DescriptionBorderedItemContent itemContent)
                            {
                                var itemSpan = Math.Max(1,
                                    Math.Min(_effectiveColumns - column, GetItemSpan(item.Span) * 2 - 1));
                                if (i == Items.Count - 1 || item.IsFilled)
                                {
                                    itemSpan = _effectiveColumns - column;
                                }
                                
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
                        else
                        {
                            if (_gridLayout.Children[index] is DescriptionDefaultItem defaultItem)
                            {
                                var itemSpan = Math.Max(1,
                                    Math.Min(_effectiveColumns - column, GetItemSpan(item.Span)));
                                if (i == Items.Count - 1 || item.IsFilled)
                                {
                                    itemSpan = _effectiveColumns - column;
                                }

                                Grid.SetRow(defaultItem, row);
                                Grid.SetColumn(defaultItem, column);
                                Grid.SetColumnSpan(defaultItem, itemSpan);
                                column += itemSpan;
                                if (column >= _effectiveColumns)
                                {
                                    column = 0;
                                    ++row;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_gridLayout.Children[index] is DescriptionDefaultItem defaultItem)
                        {
                            var itemSpan = Math.Max(1,
                                Math.Min(_effectiveColumns - column, GetItemSpan(item.Span)));
                            if (i == Items.Count - 1 || item.IsFilled)
                            {
                                itemSpan = _effectiveColumns - column;
                            }

                            Grid.SetRow(defaultItem, row);
                            Grid.SetColumn(defaultItem, column);
                            Grid.SetColumnSpan(defaultItem, itemSpan);
                            column += itemSpan;
                            if (column >= _effectiveColumns)
                            {
                                column                   = 0;
                                defaultItem.IsLastColumn = true;
                                ++row;
                            }
                            else
                            {
                                defaultItem.IsLastColumn = false;
                            }
                        }
                    }
                }
            }
            
            // 寻找最后一行
            for (var i = 0; i < Items.Count; i++)
            {
                var item  = Items[i];
                var index = Items.IndexOf(item);
                if (index != -1)
                {
                    if (Layout == Orientation.Horizontal)
                    {
                        if (IsBordered)
                        {
                            index *= 2;
                            if (_gridLayout.Children[index] is DescriptionBorderedItemLabel itemLabel)
                            {
                                itemLabel.IsLastRow = Grid.GetRow(itemLabel) == row - 1;
                            }

                            if (_gridLayout.Children[index + 1] is DescriptionBorderedItemContent itemContent)
                            {
                                itemContent.IsLastRow = Grid.GetRow(itemContent) == row - 1;
                            }
                        }
                    }
                    else
                    {
                        if (_gridLayout.Children[index] is DescriptionDefaultItem defaultItem)
                        {
                            defaultItem.IsLastRow = Grid.GetRow(defaultItem) == row - 1;
                        }
                    }
                }
            }

            _gridLayout.ColumnDefinitions.Clear();
            var columnDefinitions = new ColumnDefinitions();
            for (var i = 0; i < _effectiveColumns; i++)
            {
                columnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }

            _gridLayout.ColumnDefinitions = columnDefinitions;
            _gridLayout.RowDefinitions.Clear();
            var rowDefinitions = new RowDefinitions();
            for (var i = 0; i < row; i++)
            {
                rowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            _gridLayout.RowDefinitions = rowDefinitions;
        }
    }

    private int GetItemSpan(DescriptionsMediaBreakInfo breakInfo)
    {
        Debug.Assert(_breakPoint != null);
        return _breakPoint switch
        {
            MediaBreakPoint.ExtraSmall => breakInfo.ExtraSmall,
            MediaBreakPoint.Small => breakInfo.Small,
            MediaBreakPoint.Medium => breakInfo.Medium,
            MediaBreakPoint.Large => breakInfo.Large,
            MediaBreakPoint.ExtraLarge => breakInfo.ExtraLarge,
            _ => breakInfo.ExtraExtraLarge
        };
    }

    private void HandleLayoutChanged()
    {
        var items = Items.ToList();
        RemoveDescriptionItems(items);
        AddDescriptionItems(items);
    }
}