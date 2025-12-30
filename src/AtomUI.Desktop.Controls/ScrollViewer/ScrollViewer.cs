using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;

public class ScrollViewer : AvaloniaScrollViewer,
                            IMotionAwareControl,
                            IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ScrollViewer>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ScrollBarsSeparatorOpacityProperty =
        AvaloniaProperty.Register<ScrollViewer, double>(nameof(ScrollBarsSeparatorOpacity));
    
    internal double ScrollBarsSeparatorOpacity
    {
        get => GetValue(ScrollBarsSeparatorOpacityProperty);
        set => SetValue(ScrollBarsSeparatorOpacityProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ScrollViewerToken.ID;

    #endregion
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<DoubleTransition>(ScrollBarsSeparatorOpacityProperty),
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(false);
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
}