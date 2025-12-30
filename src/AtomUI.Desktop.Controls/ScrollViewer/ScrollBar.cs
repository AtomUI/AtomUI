using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaScrollBar = Avalonia.Controls.Primitives.ScrollBar;

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal)]
public class ScrollBar : AvaloniaScrollBar,
                         IMotionAwareControl,
                         IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsLiteModeProperty =
        ScrollViewer.IsLiteModeProperty.AddOwner<ScrollBar>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ScrollViewer>();
    
    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsEffectiveExpandedProperty =
        AvaloniaProperty.Register<ScrollBar, bool>(nameof(IsEffectiveExpanded));
    
    internal bool IsEffectiveExpanded
    {
        get => GetValue(IsEffectiveExpandedProperty);
        set => SetValue(IsEffectiveExpandedProperty, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ScrollViewerToken.ID;

    #endregion

    public ScrollBar()
    {
        this.RegisterResources();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == AllowAutoHideProperty)
        {
            UpdateIsExpandedState();
        }
        else
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsEffectiveExpandedProperty)
            {
                this.SetIsExpanded(IsEffectiveExpanded);
            }
        }
    }
    
    private void UpdateIsExpandedState()
    {
        if (!IsLiteMode)
        {
            if (!AllowAutoHide)
            {
                var timer = this.GetTimer();
                timer?.Stop();
                IsEffectiveExpanded = false;
            }
        }
        else
        {
            if (!AllowAutoHide)
            {
                var timer = this.GetTimer();
                timer?.Stop();
            }
        }
    }
}