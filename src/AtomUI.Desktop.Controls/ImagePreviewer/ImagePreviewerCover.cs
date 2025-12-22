using AtomUI.Controls;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewerCover : ContentControl, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<PreviewImageSource?> ImageSourceProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, PreviewImageSource?>(nameof(ImageSource));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewerCover>();
    
    public static readonly StyledProperty<bool> IsShowCoverMaskProperty =
        ImagePreviewer.IsShowCoverMaskProperty.AddOwner<ImagePreviewerCover>();
    
    public PreviewImageSource? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsShowCoverMask
    {
        get => GetValue(IsShowCoverMaskProperty);
        set => SetValue(IsShowCoverMaskProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, double>(nameof(MaskOpacity), 0.0);
    
    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
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
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    protected override void OnLoaded(RoutedEventArgs args)
    {
        base.OnLoaded(args);
        ConfigureTransitions(false);
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<DoubleTransition>(MaskOpacityProperty, SharedTokenKey.MotionDurationSlow)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
}