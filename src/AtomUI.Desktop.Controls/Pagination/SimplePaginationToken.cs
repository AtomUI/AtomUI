using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SimplePaginationToken : AbstractPaginationToken
{
    /// <summary>Background color of the page number input.</summary>
    public partial Color ItemInputBg { get; set; }
}
