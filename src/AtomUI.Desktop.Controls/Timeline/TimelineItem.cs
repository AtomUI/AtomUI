using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class TimelineItem : ContentControl
{
    public const string OrderOddPC = "order-odd";
    public const string OrderEvenPC = "order-even";
    public const string OrderFirstPC = "order-first";
    public const string OrderLastPC = "order-last";
    public const string PendingItemPC = "pending-item";
        
    #region 公共属性定义

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<TimelineItem, string?>(nameof(Label));
    
    public static readonly StyledProperty<PathIcon?> IndicatorIconProperty =
        AvaloniaProperty.Register<TimelineItem, PathIcon?>(nameof(IndicatorIcon));
    
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<TimelineItem, IBrush?>(nameof(IndicatorColor));
    
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public PathIcon? IndicatorIcon
    {
        get => GetValue(IndicatorIconProperty);
        set => SetValue(IndicatorIconProperty, value);
    }
    
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }
    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<TimelineItem, TimeLineMode> ModeProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, TimeLineMode>(nameof(Mode), 
            o => o.Mode,
            (o, v) => o.Mode = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsOddProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsOdd), 
            o => o.IsOdd,
            (o, v) => o.IsOdd = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsFirstProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsFirst), 
            o => o.IsFirst,
            (o, v) => o.IsFirst = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsLastProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsLast), 
            o => o.IsLast,
            (o, v) => o.IsLast = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> NextIsPendingProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(NextIsPending), 
            o => o.NextIsPending,
            (o, v) => o.NextIsPending = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsReverseProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsReverse), 
            o => o.IsReverse,
            (o, v) => o.IsReverse = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsPendingProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsPending), 
            o => o.IsPending,
            (o, v) => o.IsPending = v);
    
    internal static readonly DirectProperty<TimelineItem, bool> IsLabelLayoutProperty
        = AvaloniaProperty.RegisterDirect<TimelineItem, bool>(nameof(IsLabelLayout), 
            o => o.IsLabelLayout,
            (o, v) => o.IsLabelLayout = v);
        
    private TimeLineMode _mode = TimeLineMode.Left;

    internal TimeLineMode Mode
    {
        get => _mode;
        set => SetAndRaise(ModeProperty, ref _mode, value);
    }
    
    private bool _isOdd;

    internal bool IsOdd
    {
        get => _isOdd;
        set => SetAndRaise(IsOddProperty, ref _isOdd, value);
    }
    
    private bool _isFirst;

    internal bool IsFirst
    {
        get => _isFirst;
        set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }
    
    private bool _isLast;

    internal bool IsLast
    {
        get => _isLast;
        set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }
    
    private bool _nextIsPending;

    internal bool NextIsPending
    {
        get => _nextIsPending;
        set => SetAndRaise(NextIsPendingProperty, ref _nextIsPending, value);
    }

    private bool _isReverse = false;

    internal bool IsReverse
    {
        get => _isReverse;
        set => SetAndRaise(IsReverseProperty, ref _isReverse, value);
    }
    
    private bool _isPending = false;

    internal bool IsPending
    {
        get => _isPending;
        set => SetAndRaise(IsPendingProperty, ref _isPending, value);
    }
    
    private bool _isLabelLayout = false;

    internal bool IsLabelLayout
    {
        get => _isLabelLayout;
        set => SetAndRaise(IsLabelLayoutProperty, ref _isLabelLayout, value);
    }

    #endregion

    static TimelineItem()
    {
        AffectsArrange<TimelineItem>(ModeProperty);
        AffectsMeasure<TimelineItem>(IsReverseProperty);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Debug.Assert(Parent is Timeline, "TimelineItem's parent must be a timeline control.");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOddProperty ||
            change.Property == IsFirstProperty ||
            change.Property == IsLastProperty ||
            change.Property == IsPendingProperty)
        {
            UpdatePseudoClasses();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(OrderOddPC, IsOdd);
        PseudoClasses.Set(OrderEvenPC, !IsOdd);
        PseudoClasses.Set(OrderFirstPC, IsFirst);
        PseudoClasses.Set(OrderLastPC, IsLast);
        PseudoClasses.Set(PendingItemPC, IsPending);
    }
}