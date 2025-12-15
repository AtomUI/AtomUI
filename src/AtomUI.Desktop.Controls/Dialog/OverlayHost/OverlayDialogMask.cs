using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class OverlayDialogMask : TemplatedControl
{
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OverlayDialogMask>();
    
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<OverlayDialogMask, TimeSpan>(nameof(AnimationDuration));
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
    
    private readonly DialogLayer _dialogLayer;

    public OverlayDialogMask(DialogLayer dialogLayer, Dialog dialog)
    {
        _dialogLayer = dialogLayer;
        BindUtils.RelayBind(dialog, IsMotionEnabledProperty, this, IsMotionEnabledProperty);
    }

    public void Show(OverlayDialogHost maskTarget)
    {
        var dialogHostIndex = _dialogLayer.Children.IndexOf(maskTarget);
        if (dialogHostIndex == -1)
        {
            return;
        }
        _dialogLayer.SizeChanged += HandleDialogLayerSizeChanged;
        _dialogLayer.Children.Insert(dialogHostIndex, this);
        ConfigureMaskSize(_dialogLayer.Bounds.Size);
        Dispatcher.UIThread.Post(() =>
        {
            Opacity = 1.0;
        });
    }

    public void Hide()
    {
        Opacity = 0.0;
        DispatcherTimer.RunOnce(() =>
        {
            _dialogLayer.Children.Remove(this);
            _dialogLayer.SizeChanged -= HandleDialogLayerSizeChanged;
        }, AnimationDuration);
    }
    
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
                    TransitionUtils.CreateTransition<DoubleTransition>(OpacityProperty, AnimationDuration)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    private void HandleDialogLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ConfigureMaskSize(e.NewSize);
    }

    private void ConfigureMaskSize(Size size)
    {
        Width  = size.Width;
        Height = size.Height;
    }
}