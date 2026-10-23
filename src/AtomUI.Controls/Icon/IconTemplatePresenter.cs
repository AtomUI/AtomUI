using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

public class IconTemplatePresenter : Control
{
    #region 公共属性定义
    public static readonly StyledProperty<IconTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<IconTemplatePresenter, IconTemplate?>(nameof(IconTemplate));
    
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconTemplatePresenter, IBrush?>(nameof(IconBrush));
    
    [Content]
    public IconTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }
    
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    #endregion
    
    private PathIcon? _icon;
    private CompositeDisposable? _disposables;

    static IconTemplatePresenter()
    {
        AffectsMeasure<IconTemplatePresenter>( IconTemplateProperty);
        AffectsRender<IconTemplatePresenter>(IconBrushProperty);
        IconTemplateProperty.Changed.AddClassHandler<IconTemplatePresenter>((x, e) => x.HandleIconTemplateChanged(e));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_icon != null)
        {
            ConfigureIcon(_icon);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _disposables?.Dispose();
        _disposables = null;
    }

    private void HandleIconTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        _disposables?.Dispose();
        _disposables = null;
        LogicalChildren.Clear();
        VisualChildren.Clear();
        _icon = ((IconTemplate?)change.NewValue)?.Build();
        if (_icon != null)
        {
            ConfigureIcon(_icon);
            VisualChildren.Add(_icon);
            LogicalChildren.Add(_icon);
        }
    }

    private void ConfigureIcon(PathIcon pathIcon)
    {
        // This runtime-built content is outside a ControlTemplate. An ordinary child
        // Style cannot express the same Template binding priority; own and dispose
        // these bindings so explicit child values win and discarded icons stop following.
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(7);
        _disposables.Add(BindUtils.RelayBind(this, WidthProperty, pathIcon, WidthProperty,
            BindingMode.Default, BindingPriority.Template));
        _disposables.Add(BindUtils.RelayBind(this, HeightProperty, pathIcon, HeightProperty,
            BindingMode.Default, BindingPriority.Template));
        if (pathIcon is Icon icon)
        {
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, Icon.StrokeBrushProperty,
                BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, Icon.FillBrushProperty,
                BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, Icon.SecondaryStrokeBrushProperty,
                BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, Icon.SecondaryFillBrushProperty,
                BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, Icon.FallbackBrushProperty,
                BindingMode.Default, BindingPriority.Template));
        }
        else
        {
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, pathIcon, PathIcon.ForegroundProperty,
                BindingMode.Default, BindingPriority.Template));
        }
    }
}
