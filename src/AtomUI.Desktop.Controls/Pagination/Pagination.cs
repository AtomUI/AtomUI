using System.Diagnostics;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum PaginationAlign
{
    Start,
    Center,
    End
}

public class Pagination : AbstractPagination, IControlSharedTokenResourcesHost
{
    internal const int MaxNavItemCount = 11;

    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsShowSizeChangerProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowSizeChanger));
    
    public static readonly StyledProperty<bool> IsShowQuickJumperProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowQuickJumper));
    
    public static readonly StyledProperty<bool> IsShowTotalInfoProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowTotalInfo));

    public static readonly StyledProperty<string?> TotalInfoTemplateProperty =
        AvaloniaProperty.Register<Pagination, string?>(nameof(TotalInfoTemplate));
    
    public bool IsShowSizeChanger
    {
        get => GetValue(IsShowSizeChangerProperty);
        set => SetValue(IsShowSizeChangerProperty, value);
    }
    
    public bool IsShowQuickJumper
    {
        get => GetValue(IsShowQuickJumperProperty);
        set => SetValue(IsShowQuickJumperProperty, value);
    }
    
    public bool IsShowTotalInfo
    {
        get => GetValue(IsShowTotalInfoProperty);
        set => SetValue(IsShowTotalInfoProperty, value);
    }
    
    public string? TotalInfoTemplate
    {
        get => GetValue(TotalInfoTemplateProperty);
        set => SetValue(TotalInfoTemplateProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Pagination, ComboBox?> SizeChangerProperty =
        AvaloniaProperty.RegisterDirect<Pagination, ComboBox?>(nameof(SizeChanger),
            o => o.SizeChanger,
            (o, v) => o.SizeChanger = v);

    internal static readonly DirectProperty<Pagination, QuickJumperBar?> QuickJumperBarProperty =
        AvaloniaProperty.RegisterDirect<Pagination, QuickJumperBar?>(nameof(QuickJumperBar),
            o => o.QuickJumperBar,
            (o, v) => o.QuickJumperBar = v);

    internal static readonly DirectProperty<Pagination, string?> PageTextProperty =
        AvaloniaProperty.RegisterDirect<Pagination, string?>(nameof(PageText),
            o => o.PageText,
            (o, v) => o.PageText = v);

    internal static readonly DirectProperty<Pagination, string?> TotalInfoTextProperty =
        AvaloniaProperty.RegisterDirect<Pagination, string?>(nameof(TotalInfoText),
            o => o.TotalInfoText,
            (o, v) => o.TotalInfoText = v);

    private ComboBox? _sizeChanger;

    internal ComboBox? SizeChanger
    {
        get => _sizeChanger;
        set => SetAndRaise(SizeChangerProperty, ref _sizeChanger, value);
    }

    private QuickJumperBar? _quickJumperBar;

    internal QuickJumperBar? QuickJumperBar
    {
        get => _quickJumperBar;
        set => SetAndRaise(QuickJumperBarProperty, ref _quickJumperBar, value);
    }

    private string? _pageText;

    public string? PageText
    {
        get => _pageText;
        set => SetAndRaise(PageTextProperty, ref _pageText, value);
    }

    private string? _totalInfoText;

    public string? TotalInfoText
    {
        get => _totalInfoText;
        set => SetAndRaise(TotalInfoTextProperty, ref _totalInfoText, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => PaginationToken.ID;

    #endregion

    private PaginationNav? _paginationNav;
    private PaginationNavItem? _previousPageItem;
    private PaginationNavItem? _nextPageItem;
    private int _nextPushItemIndex = 1;
    private int _selectedNavItemIndex = -1;
    private IDisposable? _sizeChangerDisposable;
    private IDisposable? _quickJumperDisposable;

    public Pagination()
    {
        this.RegisterResources();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _paginationNav = e.NameScope.Find<PaginationNav>(PaginationThemeConstants.NavPart);
        Debug.Assert(_paginationNav is not null);
        _paginationNav.ContainerPrepared   += HandleContainerPrepared;
        _paginationNav.PageNavigateRequest += HandlePageNavRequest;
        if (IsShowQuickJumper)
        {
            SetupQuickJumper();
        }

        if (IsShowSizeChanger)
        {
            SetupSizeChanger();
        }
    }

    private void HandleContainerPrepared(object? sender, ContainerPreparedEventArgs args)
    {
        Debug.Assert(_paginationNav is not null);
        var count = _paginationNav.ItemCount;
        if (args.Container is PaginationNavItem navItem)
        {
            if (0 == args.Index)
            {
                navItem.PaginationItemType = PaginationItemType.Previous;
                _previousPageItem          = navItem;
                _previousPageItem.Icon  = new LeftOutlined();
            }
            else if (count - 1 == args.Index)
            {
                navItem.PaginationItemType = PaginationItemType.Next;
                _nextPageItem              = navItem;
                _nextPageItem.Icon         = new RightOutlined();
            }
            else
            {
                navItem.PaginationItemType = PaginationItemType.PageIndicator;
            }
        }
        TemplateConfigured = true;
        if (_paginationNav.GetRealizedContainers().Count() == count)
        {
            HandlePageConditionChanged();
        }
    }

    protected override void NotifyPageConditionChanged(int currentPage, int pageCount, int pageSize, long total)
    {
        if (TemplateConfigured)
        {
            Debug.Assert(_paginationNav != null);
            Debug.Assert(_previousPageItem != null);
            Debug.Assert(_nextPageItem != null);
            var count = _paginationNav.ItemCount;
            // 清空状态 clear state
            _paginationNav.SelectedIndex = -1;
            _selectedNavItemIndex        = -1;
            for (int i = 1; i < count - 1; i++)
            {
                var container = _paginationNav.ContainerFromIndex(i);
                if (container is PaginationNavItem navItem)
                {
                    navItem.PaginationItemType = PaginationItemType.PageIndicator;
                    navItem.IsVisible          = false;
                    navItem.Content            = null;
                }
            }

            _previousPageItem.IsEnabled  = currentPage > 1;
            _previousPageItem.PageNumber = Math.Max(1, CurrentPage - 1);
            _nextPageItem.IsEnabled      = currentPage < pageCount;
            _nextPageItem.PageNumber     = Math.Min(pageCount, CurrentPage + 1);
            _nextPushItemIndex           = 1;

            SetupLeftButtonRange(currentPage, pageCount);
            SetupNextIndicatorNavItem(currentPage, true);
            SetupRightButtonRange(currentPage, pageCount);
            _paginationNav.SelectedIndex = _selectedNavItemIndex;
            SetupTotalInfoText();
        }
        base.NotifyPageConditionChanged(currentPage, pageCount, pageSize, total);
    }

    private void HandlePageNavRequest(object? sender, PageNavRequestArgs args)
    {
        if (args.PageNumber != CurrentPage)
        {
            CurrentPage = args.PageNumber;
        }
    }

    private void SetupLeftButtonRange(int currentPage, int pageCount)
    {
        if (currentPage < 5)
        {
            for (var i = 1; i < currentPage; i++)
            {
                SetupNextIndicatorNavItem(i, false);
            }
        }
        else
        {
            var leftDelta = Math.Max(2, 4 - (pageCount - currentPage));
            var i         = currentPage - leftDelta;
            if (i > 1)
            {
                SetupNextIndicatorNavItem(1, false);
                SetupEllipsisNavItem();
            }

            for (; i < currentPage; i++)
            {
                SetupNextIndicatorNavItem(i, false);
            }
        }
    }

    private void SetupRightButtonRange(int currentPage, int pageCount)
    {
        if (pageCount - currentPage < 4)
        {
            for (var i = currentPage + 1; i <= pageCount; i++)
            {
                SetupNextIndicatorNavItem(i, false);
            }
        }
        else
        {
            var rightDelta = Math.Max(2, 5 - currentPage);
            var i          = currentPage + 1;
            for (; i <= currentPage + rightDelta; i++)
            {
                SetupNextIndicatorNavItem(i, false);
            }

            if (i < pageCount)
            {
                SetupEllipsisNavItem();
                SetupNextIndicatorNavItem(pageCount, false);
            }
        }
    }

    private void SetupNextIndicatorNavItem(int pageIndex, bool isActive)
    {
        if (_nextPushItemIndex == 0 || _nextPushItemIndex == MaxNavItemCount)
        {
            throw new ArgumentException("Invalid next push item index");
        }

        Debug.Assert(_paginationNav != null);
        var navItem = _paginationNav.ContainerFromIndex(_nextPushItemIndex++) as PaginationNavItem;

        if (isActive)
        {
            _selectedNavItemIndex = _nextPushItemIndex - 1;
        }

        Debug.Assert(navItem != null);
        navItem.PageNumber = pageIndex;
        navItem.Content    = $"{pageIndex}";
        navItem.IsVisible  = true;
    }

    private void SetupEllipsisNavItem()
    {
        if (_nextPushItemIndex == 0 || _nextPushItemIndex == MaxNavItemCount)
        {
            throw new ArgumentException("Invalid next push item index");
        }

        Debug.Assert(_paginationNav != null);
        var navItem = _paginationNav.ContainerFromIndex(_nextPushItemIndex++) as PaginationNavItem;
        Debug.Assert(navItem != null);
        navItem.Icon               = new EllipsisOutlined();
        navItem.PaginationItemType = PaginationItemType.Ellipses;
        navItem.IsVisible          = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsShowSizeChangerProperty)
            {
                SetupSizeChanger();
            }
            else if (change.Property == IsShowQuickJumperProperty)
            {
                SetupQuickJumper();
            }
        }

        if (change.Property == PageSizeProperty)
        {
            SetupSizeChangerSelected();
        }
    }

    private void SetupSizeChangerSelected()
    {
        if (SizeChanger != null)
        {
            for (int i = 0; i < SizeChanger.Items.Count; i++)
            {
                if (SizeChanger.Items.GetAt(i) is PageSizeComboBoxItem pageSizeItem)
                {
                    if (pageSizeItem.PageSize == PageSize)
                    {
                        SizeChanger.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
    }

    private void SetupTotalInfoText()
    {
        if (IsShowTotalInfo && TotalInfoTemplate != null)
        {
            TotalInfoText = TotalInfoTemplate.Replace("${Total}", $"{Total}")
                                             .Replace("${RangeStart}", $"{(CurrentPage - 1) * PageSize}")
                                             .Replace("${RangeEnd}", $"{Math.Min(CurrentPage * PageSize, Total)}");
        }
    }

    private void SetupSizeChanger()
    {
        if (SizeChanger == null)
        {
            var sizeChanger = new ComboBox();
            sizeChanger.VerticalAlignment = VerticalAlignment.Center;
            _sizeChangerDisposable?.Dispose();
            _sizeChangerDisposable = BindUtils.RelayBind(this, SizeTypeProperty, sizeChanger, ComboBox.SizeTypeProperty);
            sizeChanger.Items.Add(new PageSizeComboBoxItem { Content = $"10 / {PageText}", PageSize  = 10 });
            sizeChanger.Items.Add(new PageSizeComboBoxItem { Content = $"20 / {PageText}", PageSize  = 20 });
            sizeChanger.Items.Add(new PageSizeComboBoxItem { Content = $"50 / {PageText}", PageSize  = 50 });
            sizeChanger.Items.Add(new PageSizeComboBoxItem { Content = $"100 / {PageText}", PageSize = 100 });
            sizeChanger.SelectedIndex    =  0;
            SizeChanger                  =  sizeChanger;
            SizeChanger.SelectionChanged += HandlePageSizeChanged;
            SetupSizeChangerSelected();
        }
    }

    private void SetupQuickJumper()
    {
        if (QuickJumperBar == null)
        {
            QuickJumperBar = new QuickJumperBar();
            QuickJumperBar.JumpRequest += (sender, args) =>
            {
                var total     = Math.Max(0, Total);
                var pageSize  = PageSize <= 0 ? DefaultPageSize : PageSize;
                var pageCount = (int)Math.Ceiling(total / (double)pageSize);
                CurrentPage = Math.Max(1, Math.Min(pageCount, args.PageNumber));
            };
            _quickJumperDisposable?.Dispose();
            _quickJumperDisposable = BindUtils.RelayBind(this, SizeTypeProperty, QuickJumperBar, QuickJumperBar.SizeTypeProperty);
        }
    }

    private void HandlePageSizeChanged(object? sender, SelectionChangedEventArgs? args)
    {
        if (args?.AddedItems.Count >= 1 && args.AddedItems[0] is PageSizeComboBoxItem comboBoxItem)
        {
            PageSize = Math.Max(comboBoxItem.PageSize, 1);
        }
    }
}