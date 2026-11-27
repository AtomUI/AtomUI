using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A pager made of a previous button, an input for the page number, the page count text and a next button.
/// </summary>
public partial class SimplePagination : AbstractPagination
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<SimplePagination, bool>(nameof(IsReadOnly), defaultValue:true);

    /// <summary>
    /// Gets or sets whether the pager only displays the page position. With <see langword="true" />, the page number
    /// input is hidden and the page count text reads "current page / page count", while the previous and next
    /// buttons keep working. The default is <see langword="true" />.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion
    
    private PaginationNavItem? _previousPageItem;
    private PaginationNavItem? _nextPageItem;
    private TextBlock? _infoIndicator;
    private QuickJumpEdit? _quickJumper;

    static SimplePagination()
    {
        AffectsMeasure<SimplePagination>(IsReadOnlyProperty);
    }

    /// <summary>
    /// Connects the template parts after the template was applied: subscribes to the previous and next buttons and
    /// to the page number input, then refreshes every part from the current page condition.
    /// </summary>
    /// <remarks>
    /// The handlers of the previous template are detached first. Overrides must call the base implementation to
    /// get this setup.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ClearTemplateParts();

        _previousPageItem = e.NameScope.Find<PaginationNavItem>("PART_PreviousNavItem");
        _nextPageItem     = e.NameScope.Find<PaginationNavItem>("PART_NextNavItem");
        _infoIndicator    = e.NameScope.Find<TextBlock>("PART_InfoIndicator");
        _quickJumper      = e.NameScope.Find<QuickJumpEdit>("PART_QuickJumper");

        Debug.Assert(_nextPageItem != null);
        Debug.Assert(_previousPageItem != null);
        Debug.Assert(_quickJumper != null);
        _previousPageItem.Click += HandleNavItemClicked;
        _nextPageItem.Click     += HandleNavItemClicked;
        _quickJumper.KeyUp      += HandleLineEditKeyUp;
        TemplateConfigured      =  true;
        HandlePageConditionChanged();
    }

    /// <summary>
    /// Updates the template parts for a new page condition: the page indicator text and the page number input show
    /// the current page and the page count, the input accepts at most the page count, and the previous and next
    /// buttons are enabled only when a page exists in their direction, and they target that page.
    /// </summary>
    /// <param name="currentPage">
    /// The current page. The first page is 1.
    /// </param>
    /// <param name="pageCount">
    /// The number of pages.
    /// </param>
    /// <param name="pageSize">
    /// The number of items on a page.
    /// </param>
    /// <param name="total">
    /// The total number of items.
    /// </param>
    /// <remarks>
    /// The parts are updated only after the template was applied. The base implementation is called at the end and
    /// raises <see cref="AbstractPagination.CurrentPageChanged" />.
    /// </remarks>
    protected override void NotifyPageConditionChanged(int currentPage, int pageCount, int pageSize, long total)
    {
        if (TemplateConfigured)
        {
            if (IsReadOnly)
            {
                if (_infoIndicator != null)
                {
                    _infoIndicator.Text = $"{currentPage} / {pageCount}";
                }
            }
            else
            {
                if (_infoIndicator != null)
                {
                    _infoIndicator.Text = $" / {pageCount}";
                }

                if (_quickJumper != null)
                {
                    _quickJumper.Text = $"{currentPage}";
                }
            }
            Debug.Assert(_previousPageItem != null);
            Debug.Assert(_nextPageItem != null);
        
            _previousPageItem.IsEnabled  = currentPage > 1;
            _previousPageItem.PageNumber = Math.Max(1, CurrentPage - 1);
            _nextPageItem.IsEnabled      = currentPage < pageCount;
            _nextPageItem.PageNumber     = Math.Min(pageCount, CurrentPage + 1);
        }
        if (_quickJumper != null)
        {
            _quickJumper.Maximum = PageCount;
        }
        base.NotifyPageConditionChanged(currentPage, pageCount, pageSize, total);
    }

    /// <summary>
    /// Keeps the upper limit of the page number input at the page count after the page count changed.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageCountProperty)
        {
            if (_quickJumper != null)
            {
                _quickJumper.Maximum = PageCount;
            }
        }
    }

    private void HandleNavItemClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is PaginationNavItem navItemSender)
        {
            SetCurrentValue(CurrentPageProperty, navItemSender.PageNumber);
            HandlePageConditionChanged();
        }
    }
    
    private void HandleLineEditKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is QuickJumpEdit lineEdit)
        {
            if (e.Key == Key.Enter)
            {
                if (lineEdit.Text is { } text && int.TryParse(text.AsSpan().Trim(), out var pageNumber))
                {
                    var pageSize    = PageSize <= 0 ? DefaultPageSize : PageSize;
                    var pageCount   = (int)Math.Ceiling(Total / (double)pageSize);
                    SetCurrentValue(CurrentPageProperty, Math.Max(1, Math.Min(pageNumber, pageCount)));
                }
            }
        }
    }

    private void ClearTemplateParts()
    {
        if (_previousPageItem is not null)
        {
            _previousPageItem.Click -= HandleNavItemClicked;
        }

        if (_nextPageItem is not null)
        {
            _nextPageItem.Click -= HandleNavItemClicked;
        }

        if (_quickJumper is not null)
        {
            _quickJumper.KeyUp -= HandleLineEditKeyUp;
        }

        _previousPageItem = null;
        _nextPageItem     = null;
        _infoIndicator    = null;
        _quickJumper      = null;
        TemplateConfigured = false;
    }
}
