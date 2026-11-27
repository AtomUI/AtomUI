using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

/// <summary>
/// Hosts the icon built from <see cref="IconTemplatePresenter.IconTemplate" /> as its only child and gives it the
/// presenter's size and brush. Each presenter builds its own icon, so one template can serve many presenters.
/// </summary>
public class IconTemplatePresenter : Control
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IconTemplatePresenter.IconTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<IconTemplatePresenter, IconTemplate?>(nameof(IconTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IconTemplatePresenter.IconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconTemplatePresenter, IBrush?>(nameof(IconBrush));
    
    /// <summary>
    /// The template of the icon shown as the presenter's only child.
    /// </summary>
    /// <value>
    /// Set on the presenter; in XAML it is the content of the presenter. A change removes the previous icon and
    /// builds a new one. <see langword="null" /> removes the icon without building a new one.
    /// </value>
    [Content]
    public IconTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }
    
    /// <summary>
    /// The brush the icon built from the template is painted with.
    /// </summary>
    /// <value>
    /// Set on the presenter, usually by the control whose theme contains it. For an
    /// <see cref="AtomUI.Controls.Icon" /> it becomes the stroke, fill, secondary-layer and fallback brushes; for
    /// any other <see cref="PathIcon" /> it becomes the foreground. No default is registered, so an unset or
    /// <see langword="null" /> value relays <see langword="null" /> to those brushes.
    /// </value>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    #endregion
    
    static IconTemplatePresenter()
    {
        AffectsMeasure<IconTemplatePresenter>( IconTemplateProperty);
        AffectsRender<IconTemplatePresenter>(IconBrushProperty);
        IconTemplateProperty.Changed.AddClassHandler<IconTemplatePresenter>((x, e) => x.HandleIconTemplateChanged(e));
    }

    private void HandleIconTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldIconTemplate = (IconTemplate?)change.OldValue;
        var newIconTemplate = (IconTemplate?)change.NewValue;
        if (oldIconTemplate != null)
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();
        }

        if (newIconTemplate != null)
        {
            var pathIcon = newIconTemplate.Build();
            if (pathIcon != null)
            {
                ConfigureIcon(pathIcon);
            }
        }
    }

    private void ConfigureIcon(PathIcon pathIcon)
    {
        pathIcon[!WidthProperty]  = this[!WidthProperty];
        pathIcon[!HeightProperty] = this[!HeightProperty];
        if (pathIcon is Icon icon)
        {
            icon[!Icon.StrokeBrushProperty]          = this[!IconBrushProperty];
            icon[!Icon.FillBrushProperty]            = this[!IconBrushProperty];
            icon[!Icon.SecondaryStrokeBrushProperty] = this[!IconBrushProperty];
            icon[!Icon.SecondaryFillBrushProperty]   = this[!IconBrushProperty];
            icon[!Icon.FallbackBrushProperty]        = this[!IconBrushProperty];
        }
        else
        {
            pathIcon[!PathIcon.ForegroundProperty] = this[!IconBrushProperty];
        }
        VisualChildren.Add(pathIcon);
        LogicalChildren.Add(pathIcon);
    }
}
