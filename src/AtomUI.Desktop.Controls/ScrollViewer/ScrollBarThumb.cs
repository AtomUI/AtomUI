using AtomUI.Theme;
using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The draggable thumb of a <see cref="ScrollBar" />. Its theme draws it as a rounded border whose thickness and
/// colors come from the tokens of the scroll viewer.
/// </summary>
[TokenOwnerTransparent]
public class ScrollBarThumb : AbstractScrollBarThumb
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsLiteMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLiteModeProperty =
        ScrollViewer.IsLiteModeProperty.AddOwner<ScrollBarThumb>();

    /// <summary>
    /// Selects the lite look of this thumb. While <see langword="true" /> the thumb is as thick as
    /// <c>LiteModeThumbThickness</c> and grows to <c>NormalModeThumbThickness</c> while <c>IsExpanded</c> is
    /// <see langword="true" />, for example under the pointer or while pressed. While <see langword="false" /> it
    /// always has <c>NormalModeThumbThickness</c>. The theme of the owning scroll bar forwards its own
    /// <c>IsLiteMode</c> here. The default is <see langword="false" />.
    /// </summary>
    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    #endregion
}