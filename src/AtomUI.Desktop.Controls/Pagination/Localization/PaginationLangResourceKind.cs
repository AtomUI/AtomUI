using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// The localizable texts of the pagers, each member naming one text unit of a language pack. The pager
/// themes resolve <see cref="JumpToText" />, <see cref="PageText" /> and <see cref="TotalInfoFormat" />
/// through the <c>PaginationLangResource</c> markup; <c>Pagination</c> reads the automation-name members
/// through the localizer.
/// </summary>
[LanguageCatalog]
public enum PaginationLangResourceKind
{
    /// <summary>
    /// Label shown in front of the quick jumper input.
    /// </summary>
    JumpToText,
    /// <summary>
    /// Label shown after the quick jumper input. It also precedes the page number in the automation name
    /// of a page item and follows the page size of a page-size item.
    /// </summary>
    PageText,
    /// <summary>
    /// Template of the total-info text. Its <c>${Total}</c>, <c>${RangeStart}</c> and <c>${RangeEnd}</c>
    /// placeholders are replaced by the item count and the range of the current page.
    /// </summary>
    TotalInfoFormat,
    /// <summary>
    /// Automation name of the button that goes to the previous page.
    /// </summary>
    PreviousPageText,
    /// <summary>
    /// Automation name of the button that goes to the next page.
    /// </summary>
    NextPageText,
    /// <summary>
    /// Automation name of the button that jumps back several pages; <c>{0}</c> is replaced by the number
    /// of pages it jumps back.
    /// </summary>
    PreviousPagesTextFormat,
    /// <summary>
    /// Automation name of the button that jumps forward several pages; <c>{0}</c> is replaced by the
    /// number of pages it jumps forward.
    /// </summary>
    NextPagesTextFormat
}
