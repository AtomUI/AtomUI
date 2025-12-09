using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class ImagePreviewer : TemplatedControl, 
                              IControlSharedTokenResourcesHost,
                              IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<IList<IImage>?> SourcesProperty =
        AvaloniaProperty.Register<ImagePreviewer, IList<IImage>?>(nameof(Sources));

    public static readonly StyledProperty<IImage?> CoverImageSrcProperty =
        AvaloniaProperty.Register<ImagePreviewer, IImage?>(nameof(CoverImageSrc));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewer>();
    
    public IList<IImage>? Sources
    {
        get => GetValue(SourcesProperty);
        set => SetValue(SourcesProperty, value);
    }

    public IImage? CoverImageSrc
    {
        get => GetValue(CoverImageSrcProperty);
        set => SetValue(CoverImageSrcProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<ImagePreviewer, double>(nameof(MaskOpacity), 0.0);
    
    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
    }

    Control IMotionAwareControl.PropertyBindTarget => this;
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ImagePreviewerToken.ID;
    
    #endregion
    
    static ImagePreviewer()
    {
        FocusableProperty.OverrideDefaultValue<ImagePreviewer>(true);
        AffectsRender<ImagePreviewer>(CoverImageSrcProperty);
    }
    
    public ImagePreviewer()
    {
        this.RegisterResources();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (CoverImageSrc == null && Sources?.Count > 0)
        {
            SetCurrentValue(CoverImageSrcProperty, Sources.First());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourcesProperty)
        {
            if (Sources?.Count > 0)
            {
                if (CoverImageSrc == null)
                {
                    SetCurrentValue(CoverImageSrcProperty, Sources.First());
                }
            }
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
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