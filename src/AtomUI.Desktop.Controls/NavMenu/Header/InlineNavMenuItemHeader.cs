using AtomUI.Theme;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// A NavMenu template part without Own Tokens: its theme reads NavMenu Tokens and its TokenTransitions follow the NavMenu.
/// <summary>
/// The header of a menu item in Inline mode: the icon, the title content and the arrow that marks an item that
/// has a submenu, indented by the nesting depth of the item.
/// </summary>
[TokenOwnerTransparent]
public class InlineNavMenuItemHeader : BaseNavMenuItemHeader
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Level" /> property.
    /// </summary>
    public static readonly DirectProperty<InlineNavMenuItemHeader, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<InlineNavMenuItemHeader, int>(
            nameof(Level), 
            o => o.Level,
            (o, v) => o.Level = v);
    
    /// <summary>
    /// Identifies the <see cref="InlineItemIndentUnit" /> property.
    /// </summary>
    public static readonly DirectProperty<InlineNavMenuItemHeader, double> InlineItemIndentUnitProperty =
        AvaloniaProperty.RegisterDirect<InlineNavMenuItemHeader, double>(nameof(InlineItemIndentUnit),
            o => o.InlineItemIndentUnit,
            (o, v) => o.InlineItemIndentUnit = v);
    
    /// <summary>
    /// Identifies the <see cref="MenuIndicatorRenderTransform" /> property.
    /// </summary>
    public static readonly StyledProperty<ITransform?> MenuIndicatorRenderTransformProperty =
        AvaloniaProperty.Register<InlineNavMenuItemHeader, ITransform?>(nameof(MenuIndicatorRenderTransform));
    
    private int _level;
    
    /// <summary>
    /// Gets or sets the nesting depth of the item. The owning <c>NavMenuItem</c> supplies it, and the theme indents
    /// the header content from the left by <see cref="Level" /> times <see cref="InlineItemIndentUnit" />.
    /// </summary>
    public int Level
    {
        get => _level;
        set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    private double _inlineItemIndentUnit;

    /// <summary>
    /// Gets or sets the width of one nesting level in device-independent pixels, used with <see cref="Level" /> to
    /// indent the header content from the left. The theme sets it from the <c>InlineItemIndentUnit</c> Token of
    /// <see cref="NavMenu" />; a local value replaces the Token value.
    /// </summary>
    public double InlineItemIndentUnit
    {
        get => _inlineItemIndentUnit;
        set => SetAndRaise(InlineItemIndentUnitProperty, ref _inlineItemIndentUnit, value);
    }
    
    /// <summary>
    /// Gets or sets the render transform of the frame around the submenu indicator arrow. The theme rotates it 90
    /// degrees for a closed submenu and -90 degrees for an open one. The frame is shown only when the item has a
    /// submenu.
    /// </summary>
    public ITransform? MenuIndicatorRenderTransform
    {
        get => GetValue(MenuIndicatorRenderTransformProperty);
        set => SetValue(MenuIndicatorRenderTransformProperty, value);
    }
    #endregion

}