using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

using IconControl = Icon;

public class IconPresenter : Control, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<IconTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<IconPresenter, IconTemplate?>(nameof(IconTemplate));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<IconPresenter, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(IconBrush));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<IconPresenter>();
    
    public IconTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }
    
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    private CompositeDisposable? _disposables;
    
    static IconPresenter()
    {
        AffectsMeasure<IconPresenter>(IconProperty, IconTemplateProperty);
        AffectsRender<IconPresenter>(IconBrushProperty);
        IconProperty.Changed.AddClassHandler<IconPresenter>((x, e) => x.HandleIconChanged(e));
        IconTemplateProperty.Changed.AddClassHandler<IconPresenter>((x, e) => x.HandleIconTemplateChanged(e));
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
            ConfigureIcon(pathIcon);
        }
    }

    private void HandleIconTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldIconTemplate = (IconTemplate?)change.OldValue;
        var newIconTemplate = (IconTemplate?)change.NewValue;
        if (oldIconTemplate != null)
        {
            _disposables?.Dispose();
            _disposables = null;
            if (Icon != null)
            {
                ((ISetLogicalParent)Icon).SetParent(null);
            }
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
        _disposables?.Dispose();
        _disposables = new CompositeDisposable(4);
        _disposables.Add(BindUtils.RelayBind(this, WidthProperty, pathIcon, WidthProperty));
        _disposables.Add(BindUtils.RelayBind(this, HeightProperty, pathIcon, HeightProperty));
        if (pathIcon is Icon icon)
        {
            _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, icon, IconControl.IsMotionEnabledProperty));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.StrokeBrushProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.FillBrushProperty, BindingMode.Default, BindingPriority.Template));
        }
        else
        {
            _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, pathIcon, PathIcon.ForegroundProperty, BindingMode.Default, BindingPriority.Template));
        }
        ((ISetLogicalParent)pathIcon).SetParent(null);
        pathIcon.SetVisualParent(null);
        VisualChildren.Add(pathIcon);
        LogicalChildren.Add(pathIcon);
    }
}