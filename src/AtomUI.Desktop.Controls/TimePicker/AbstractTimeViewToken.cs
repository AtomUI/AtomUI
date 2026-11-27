using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by the time picker and the date picker: the size and spacing of a time selection column
/// and its items, and the visibility of the popup arrow.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractTimeViewToken : AbstractControlDesignToken
{
    /// <summary>
    /// Height of an item in a time selection column.
    /// </summary>
    public partial double ItemHeight { get; set; }
    /// <summary>
    /// Width of the hour, minute and second selection columns.
    /// Use <see cref="PeriodHostWidth" /> for the AM or PM column.
    /// </summary>
    public partial double ItemWidth { get; set; }
    /// <summary>
    /// Width of the container for the AM or PM selection column.
    /// </summary>
    public partial double PeriodHostWidth { get; set; }
    /// <summary>
    /// Padding inside an item in a time selection column.
    /// </summary>
    public partial Thickness ItemPadding { get; set; }
    /// <summary>
    /// Margin around an item in a time selection column.
    /// </summary>
    public partial Thickness ItemMargin { get; set; }

    /// <summary>
    /// Whether the picker popup shows its pointer arrow toward the input.
    /// AntDesign and shadcn set <see langword="true" />, PrimeNG sets <see langword="false" />.
    /// </summary>
    public partial bool PopupArrowVisible { get; set; }
}
