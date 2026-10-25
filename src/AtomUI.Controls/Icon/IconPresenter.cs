using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

using IconControl = Icon;

public class IconPresenter : Control
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<IconPresenter, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(IconBrush));
    
    [Content]
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
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
            TemplatePartThemeCompatibility.SetTemplatedParent(oldChild, null);
        }

        if (newChild is PathIcon pathIcon)
        {
            AddIcon(pathIcon);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Icon != null)
        {
            TemplatePartThemeCompatibility.SetTemplatedParent(Icon, TemplatedParent);
            ConfigureIcon(Icon);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _disposables?.Dispose();
        _disposables = null;
        if (Icon is not null)
        {
            TemplatePartThemeCompatibility.SetTemplatedParent(Icon, null);
        }
    }

    private void AddIcon(PathIcon pathIcon)
    {
        ConfigureIcon(pathIcon);
        ((ISetLogicalParent)pathIcon).SetParent(null);
        pathIcon.SetVisualParent(null);
        TemplatePartThemeCompatibility.SetTemplatedParent(pathIcon, TemplatedParent);
        VisualChildren.Add(pathIcon);
        LogicalChildren.Add(pathIcon);
    }

    private void ConfigureIcon(PathIcon pathIcon)
    {
        // Runtime content is outside a ControlTemplate; ordinary child Style priority
        // cannot reproduce these Template-priority bindings. Release them on replacement
        // and detach, and rebuild on attach so explicit child values retain precedence.
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(7);
        _disposables.Add(BindUtils.RelayBind(this, WidthProperty, pathIcon, WidthProperty, BindingMode.Default, BindingPriority.Template));
        _disposables.Add(BindUtils.RelayBind(this, HeightProperty, pathIcon, HeightProperty, BindingMode.Default, BindingPriority.Template));
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
