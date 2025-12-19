using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class UploadTriggerContent : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadTriggerContent>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTriggerContent>();
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

        if (change.Property == ListTypeProperty)
        {
            if (ListType == UploadListType.PictureCircle)
            {
                var radius= Math.Max(Width, Height);
                if (double.IsNaN(radius))
                {
                    radius = Math.Min(DesiredSize.Width, DesiredSize.Height);
                }
                ConfigureEffectiveCornerRadius(radius);
            }
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigureEffectiveCornerRadius(Math.Max(e.NewSize.Width, e.NewSize.Height));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    private void ConfigureEffectiveCornerRadius(double cornerRadius)
    {
        if (ListType == UploadListType.PictureCircle)
        {
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(cornerRadius));
        }
    }
}