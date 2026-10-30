using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

using AvaScrollViewer = Avalonia.Controls.ScrollViewer;

public abstract class AbstractBackTopFloatButtonHost : AbstractFloatButtonHost
{
    #region 公共属性定义
    public static readonly StyledProperty<TimeSpan> ToTopDurationProperty =
        AbstractBackTopFloatButton.ToTopDurationProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    public static readonly StyledProperty<AvaScrollViewer?> TargetProperty =
        AbstractBackTopFloatButton.TargetProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        AbstractBackTopFloatButton.VisibilityHeightProperty.AddOwner<AbstractBackTopFloatButtonHost>();
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractBackTopFloatButtonHost>();

    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AbstractBackTopFloatButton.IsShowProgressProperty.AddOwner<AbstractBackTopFloatButtonHost>();

    public TimeSpan ToTopDuration
    {
        get => GetValue(ToTopDurationProperty);
        set => SetValue(ToTopDurationProperty, value);
    }
    
    public AvaScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    public bool IsShowProgress
    {
        get => GetValue(IsShowProgressProperty);
        set => SetValue(IsShowProgressProperty, value);
    }

    #endregion

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Target ??= this.FindAncestorOfType<AvaScrollViewer>();
    }
}