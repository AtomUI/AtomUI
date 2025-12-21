using AtomUI.Controls;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class AbstractUploadPictureContent : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsImageFileProperty =
        AvaloniaProperty.Register<AbstractUploadPictureContent, bool>(nameof(IsImageFile));
    
    public static readonly StyledProperty<FileUploadStatus> StatusProperty =
        AbstractUploadListItem.StatusProperty.AddOwner<AbstractUploadPictureContent>();
    
    public static readonly StyledProperty<string?> FileNameProperty =
        AbstractUploadListItem.FileNameProperty.AddOwner<AbstractUploadPictureContent>();
    
    public static readonly StyledProperty<Uri?> FilePathProperty =
        AbstractUploadListItem.FilePathProperty.AddOwner<AbstractUploadPictureContent>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractUploadPictureContent>();
    
    public bool IsImageFile
    {
        get => GetValue(IsImageFileProperty);
        set => SetValue(IsImageFileProperty, value);
    }
    
    public string? FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }
    
    public Uri? FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }
    
    public FileUploadStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<AbstractUploadPictureContent, double>(nameof(MaskOpacity), 0.0);
    
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