using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

using IconControl = Icon;

/// <summary>
/// Hosts one <see cref="PathIcon" /> as its only child and gives it the presenter's size and brush.
/// </summary>
public class IconPresenter : Control
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IconPresenter.Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<IconPresenter, PathIcon?>(nameof(Icon));
    
    /// <summary>
    /// Identifies the <see cref="IconPresenter.IconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(IconBrush));
    
    /// <summary>
    /// The icon shown as the presenter's only visual and logical child.
    /// </summary>
    /// <value>
    /// Set on the presenter; in XAML it is the content of the presenter. Assigning another icon removes the
    /// previous one. <see langword="null" /> leaves the presenter without an icon.
    /// </value>
    [Content]
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// The brush the displayed icon is painted with.
    /// </summary>
    /// <value>
    /// Set on the presenter. For an <see cref="AtomUI.Controls.Icon" /> it becomes the stroke, fill,
    /// secondary-layer and fallback brushes; for any other <see cref="PathIcon" /> it becomes the foreground.
    /// When no value is set, the control theme supplies the <c>ColorIconHover</c> token color.
    /// </value>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    #endregion
    
    private CompositeDisposable? _disposables;
    
    static IconPresenter()
    {
        AffectsMeasure<IconPresenter>(IconProperty);
        AffectsRender<IconPresenter>(IconBrushProperty);
        IconProperty.Changed.AddClassHandler<IconPresenter>((x, e) => x.HandleIconChanged(e));
    }
    
    private void HandleIconChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldChild = (Control?)change.OldValue;
        var newChild = (Control?)change.NewValue;
        if (oldChild != null)
        {
            _disposables?.Dispose();
            _disposables = null;
            ((ISetLogicalParent)oldChild).SetParent(null);
            LogicalChildren.Remove(oldChild);
            VisualChildren.Remove(oldChild);
        }

        if (newChild is PathIcon pathIcon)
        {
            AddIcon(pathIcon);
        }
    }

    /// <summary>
    /// Rebinds the current icon to the presenter's size and brush when the presenter is attached to the visual tree,
    /// replacing the bindings created when the icon was assigned. An override has to call the base member to keep
    /// this behavior.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Icon != null)
        {
            ConfigureIcon(Icon);
        }
    }

    /// <summary>
    /// Disposes the bindings that give the current icon the presenter's size and brush when the presenter is
    /// detached from the visual tree. An override has to call the base member to release them.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _disposables?.Dispose();
        _disposables = null;
    }

    private void AddIcon(PathIcon pathIcon)
    {
        ConfigureIcon(pathIcon);
        ((ISetLogicalParent)pathIcon).SetParent(null);
        pathIcon.SetVisualParent(null);
        VisualChildren.Add(pathIcon);
        LogicalChildren.Add(pathIcon);
    }

    private void ConfigureIcon(PathIcon pathIcon)
    {
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(7);
        _disposables.Add(BindUtils.RelayBind(this, WidthProperty, pathIcon, WidthProperty));
        _disposables.Add(BindUtils.RelayBind(this, HeightProperty, pathIcon, HeightProperty));
        if (pathIcon is Icon icon)
        {
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.StrokeBrushProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.FillBrushProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.SecondaryStrokeBrushProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.SecondaryFillBrushProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.FallbackBrushProperty, BindingMode.Default, BindingPriority.Template));
        }
        else
        {
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, pathIcon, PathIcon.ForegroundProperty, BindingMode.Default, BindingPriority.Template));
        }
    }
}
