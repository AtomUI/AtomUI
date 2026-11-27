using AtomUI.Theme;
using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A scroll bar of the desktop <see cref="ScrollViewer" />. It carries the lite mode of that scroll viewer and
/// forwards it to its <see cref="ScrollBarThumb" />, which draws a thinner thumb in lite mode.
/// </summary>
[TokenOwnerTransparent]
public class ScrollBar : AbstractScrollBar
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsLiteMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLiteModeProperty =
        ScrollViewer.IsLiteModeProperty.AddOwner<ScrollBar>();

    /// <summary>
    /// Selects the lite mode of this scroll bar and, through the theme, of its thumb, which is thinner in lite mode.
    /// It also decides what <see cref="UpdateIsExpandedState" /> does while auto-hide is off. The theme of the
    /// scroll viewer forwards its own <c>IsLiteMode</c> here. The default is <see langword="false" />.
    /// </summary>
    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }

    #endregion
    
    public ScrollBar()
    {
    }
    
    /// <summary>
    /// Called when <c>AllowAutoHide</c> changes. While auto-hide is off, the timer that delays expanding and
    /// collapsing is stopped in both lite and normal mode. In normal mode the scroll bar is also reported as not
    /// expanded through <c>IsEffectiveExpanded</c>, as the base method does. In lite mode that state keeps its
    /// current value.
    /// </summary>
    protected override void UpdateIsExpandedState()
    {
        if (!IsLiteMode)
        {
            if (!AllowAutoHide)
            {
                var timer = this.GetTimer();
                timer?.Stop();
                IsEffectiveExpanded = false;
            }
        }
        else
        {
            if (!AllowAutoHide)
            {
                var timer = this.GetTimer();
                timer?.Stop();
            }
        }
    }
}