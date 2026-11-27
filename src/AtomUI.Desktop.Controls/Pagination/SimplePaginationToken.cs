using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="SimplePagination" />. It adds the background color of its page number input to
/// the tokens that the simple pager shares with <see cref="Pagination" />.
/// </summary>
[ControlDesignToken]
public sealed partial class SimplePaginationToken : AbstractPaginationToken
{
    /// <summary>
    /// Background color of the page number input.
    /// </summary>
    public partial Color ItemInputBg { get; set; }
}
