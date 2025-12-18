using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class UploadTextListItemHeader : TemplatedControl, IMotionAwareControl
{
    public static readonly StyledProperty<string?> FileNameProperty =
        AbstractUploadListItem.FileNameProperty.AddOwner<UploadTextListItemHeader>();

    public static readonly StyledProperty<FileUploadStatus> StatusProperty =
        AbstractUploadListItem.StatusProperty.AddOwner<UploadTextListItemHeader>();
    
    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AbstractUploadListItem.ErrorMessageProperty.AddOwner<UploadTextListItemHeader>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTextListItemHeader>();
    
    public string? FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
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
    
    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }
    #region 内部属性定义

    internal static readonly StyledProperty<double> RelativeLineHeightProperty =
        AvaloniaProperty.Register<UploadTextListItemHeader, double>(nameof(RelativeLineHeight));
    
    internal static readonly DirectProperty<UploadTextListItemHeader, double> EffectiveLineHeightProperty =
        AvaloniaProperty.RegisterDirect<UploadTextListItemHeader, double>(nameof(EffectiveLineHeight),
            o => o.EffectiveLineHeight,
            (o, v) => o.EffectiveLineHeight = v);

    internal double RelativeLineHeight
    {
        get => GetValue(RelativeLineHeightProperty);
        set => SetValue(RelativeLineHeightProperty, value);
    }
    
    private double _effectiveLineHeight;

    internal double EffectiveLineHeight
    {
        get => _effectiveLineHeight;
        set => SetAndRaise(EffectiveLineHeightProperty, ref _effectiveLineHeight, value);
    }
    #endregion
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RelativeLineHeightProperty ||
            change.Property == FontSizeProperty)
        {
            SetCurrentValue(EffectiveLineHeightProperty, RelativeLineHeight * FontSize);
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
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
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
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
}