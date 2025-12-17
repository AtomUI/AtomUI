using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls;

using IconControl = Icon;

public class IconPresenter : Control, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<IconPresenter, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(IconBrush));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<IconPresenter>();
    
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
    
    Control IMotionAwareControl.PropertyBindTarget => this;
    private CompositeDisposable? _disposables;
    
    static IconPresenter()
    {
        AffectsMeasure<IconPresenter>(IconProperty);
        AffectsRender<IconPresenter>(IconBrushProperty);
        IconProperty.Changed.AddClassHandler<IconPresenter>((x, e) => x.HandleIconChanged(e));
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Icon, availableSize, new Thickness(0));
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Icon, finalSize, new Thickness(0));
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

        if (newChild != null)
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable(4);
            _disposables.Add(BindUtils.RelayBind(this, WidthProperty, newChild, WidthProperty, BindingMode.Default, BindingPriority.Template));
            _disposables.Add(BindUtils.RelayBind(this, HeightProperty, newChild, HeightProperty, BindingMode.Default, BindingPriority.Template));
            if (newChild is Icon icon)
            {
                _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, icon, IconControl.IsMotionEnabledProperty, BindingMode.Default, BindingPriority.Template));
                _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.StrokeBrushProperty, BindingMode.Default, BindingPriority.Template));
                _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, icon, IconControl.FillBrushProperty, BindingMode.Default, BindingPriority.Template));
            }
            else if (newChild is PathIcon pathIcon)
            {
                _disposables.Add(BindUtils.RelayBind(this, IconBrushProperty, pathIcon, PathIcon.ForegroundProperty, BindingMode.Default, BindingPriority.Template));
            }
            ((ISetLogicalParent)newChild).SetParent(null);
            newChild.SetVisualParent(null);
            VisualChildren.Add(newChild);
            LogicalChildren.Add(newChild);
        }
    }
}