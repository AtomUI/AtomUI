using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// A NavMenu template part without Own Tokens: its theme reads NavMenu Tokens and its TokenTransitions follow the NavMenu.
/// <summary>
/// Header of a <see cref="NavMenuItem" /> in horizontal mode, showing the icon and the text of the entry.
/// A top-level entry draws the active bar along its bottom edge, in the light style only; an entry of a drop-down
/// shows a sub-menu arrow instead, see <see cref="IsTopLevel" />. The item template hosts the header as
/// <c>PART_Header</c>.
/// </summary>
[TokenOwnerTransparent]
public class HorizontalNavMenuItemHeader : BaseNavMenuItemHeader
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="ActiveBarScaleX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ActiveBarScaleXProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, double>(nameof(ActiveBarScaleX), 0.5d,
            coerce: (o, v) => Math.Max(Math.Min(v, 1.0), 0.0));

    /// <summary>
    /// Identifies the <see cref="ActiveBarHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ActiveBarHeightProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, double>(nameof(ActiveBarHeight));
    
    /// <summary>
    /// Identifies the <see cref="ActiveBarColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ActiveBarColorProperty =
        AvaloniaProperty.Register<HorizontalNavMenuItemHeader, IBrush?>(nameof(ActiveBarColor));
    
    /// <summary>
    /// Identifies the <see cref="IsTopLevel" /> property.
    /// </summary>
    public static readonly DirectProperty<HorizontalNavMenuItemHeader, bool> IsTopLevelProperty =
        AvaloniaProperty.RegisterDirect<HorizontalNavMenuItemHeader, bool>(
            nameof(IsTopLevel), o => o.IsTopLevel,
            (o, v) => o.IsTopLevel = v);
    
    /// <summary>
    /// Width of the active bar under a top-level entry, as a fraction of the header width.
    /// The bar is scaled about its center and the value is coerced to the range 0 to 1: 1 spans the full width,
    /// 0.5 centers a bar of half the width and 0 hides the bar. The theme takes the value from the NavMenu Token
    /// <c>ActiveBarScaleX</c>; a local value replaces it until it is cleared.
    /// </summary>
    public double ActiveBarScaleX
    {
        get => GetValue(ActiveBarScaleXProperty);
        set => SetValue(ActiveBarScaleXProperty, value);
    }

    /// <summary>
    /// Height of the active bar under a top-level entry, in device-independent pixels. 0 hides the bar.
    /// The theme takes the value from the NavMenu Token <c>ActiveBarHeight</c>; a local value replaces it until it
    /// is cleared.
    /// </summary>
    public double ActiveBarHeight
    {
        get => GetValue(ActiveBarHeightProperty);
        set => SetValue(ActiveBarHeightProperty, value);
    }
    
    private bool _isTopLevel;
    /// <summary>
    /// Gets or sets whether the header presents an entry of the first menu level.
    /// When <see langword="true" />, the sub-menu arrow is hidden and the theme applies the layout of a horizontal
    /// top-level entry, including the active bar; when <see langword="false" />, as on an entry of a drop-down, the
    /// sub-menu arrow is shown and the header takes the NavMenu Tokens <c>ItemHeight</c> and
    /// <c>ItemContentMargin</c>. The hosting <see cref="NavMenuItem" /> forwards its own <c>IsTopLevel</c> to the
    /// header.
    /// </summary>
    public bool IsTopLevel
    {
        get => _isTopLevel;
        set => SetAndRaise(IsTopLevelProperty, ref _isTopLevel, value);
    }
    
    /// <summary>
    /// Gets or sets the brush of the active bar under a top-level entry; <see langword="null" /> paints nothing.
    /// For a top-level entry the theme sets <c>Transparent</c> and, while the entry is hovered, its sub-menu is open,
    /// it is selected or it is on the selected path, the NavMenu Token <c>HorizontalItemSelectedColor</c>. A local
    /// value replaces both until it is cleared.
    /// </summary>
    public IBrush? ActiveBarColor
    {
        get => GetValue(ActiveBarColorProperty);
        set => SetValue(ActiveBarColorProperty, value);
    }

    #endregion

    private Rectangle? _activeIndicator;

    /// <summary>
    /// Updates the active bar scale when <see cref="ActiveBarScaleX" /> changes. Calls the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActiveBarScaleXProperty)
        {
            ConfigureActiveIndicator();
        }
    }

    /// <summary>
    /// Locates <c>PART_ActiveIndicator</c> in the template and applies the current <see cref="ActiveBarScaleX" />
    /// to it. Calls the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _activeIndicator =  e.NameScope.Find<Rectangle>("PART_ActiveIndicator");
        ConfigureActiveIndicator();
    }

    private void ConfigureActiveIndicator()
    {
        if (_activeIndicator != null)
        {
            if (_activeIndicator.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = ActiveBarScaleX;
                scaleTransform.ScaleY = 1.0;
            }
            else
            {
                _activeIndicator.RenderTransform = new ScaleTransform(ActiveBarScaleX, 1.0);
            }

            if (_activeIndicator.RenderTransformOrigin != RelativePoint.Center)
            {
                _activeIndicator.RenderTransformOrigin = RelativePoint.Center;
            }
        }
    }

}
