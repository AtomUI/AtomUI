using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Controls;

public abstract class AbstractStepsItem : HeaderedContentControl
{
    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<AbstractStepsItem, object?>(nameof(SubHeader));

    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractStepsItem, IDataTemplate?>(nameof(SubHeaderTemplate));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractStepsItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<StepsStatus?> StatusProperty =
        AvaloniaProperty.Register<AbstractStepsItem, StepsStatus?>(nameof(Status));

    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public StepsStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DirectProperty<AbstractStepsItem, int> StepNumberProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, int>(nameof(StepNumber), item => item.StepNumber);

    public static readonly DirectProperty<AbstractStepsItem, bool> IsCurrentProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsCurrent), item => item.IsCurrent);

    internal static readonly DirectProperty<AbstractStepsItem, StepsStatus> AutomaticStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, StepsStatus>(nameof(AutomaticStatus), item => item.AutomaticStatus);

    public static readonly DirectProperty<AbstractStepsItem, StepsStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, StepsStatus>(nameof(EffectiveStatus), item => item.EffectiveStatus);

    public static readonly DirectProperty<AbstractStepsItem, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsFirst), item => item.IsFirst);

    public static readonly DirectProperty<AbstractStepsItem, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsLast), item => item.IsLast);

    private int _stepNumber;

    public int StepNumber
    {
        get => _stepNumber;
        private set => SetAndRaise(StepNumberProperty, ref _stepNumber, value);
    }

    private bool _isCurrent;

    public bool IsCurrent
    {
        get => _isCurrent;
        private set => SetAndRaise(IsCurrentProperty, ref _isCurrent, value);
    }

    private StepsStatus _automaticStatus = StepsStatus.Wait;

    internal StepsStatus AutomaticStatus
    {
        get => _automaticStatus;
        private set => SetAndRaise(AutomaticStatusProperty, ref _automaticStatus, value);
    }

    private StepsStatus _effectiveStatus = StepsStatus.Wait;

    public StepsStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    private bool _isFirst;

    public bool IsFirst
    {
        get => _isFirst;
        private set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }

    private bool _isLast;

    public bool IsLast
    {
        get => _isLast;
        private set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }

    internal AbstractSteps? Owner { get; private set; }
    internal int ItemIndex { get; private set; } = -1;

    internal void ApplyOwnerState(AbstractSteps owner, int index, int number, StepsStatus automaticStatus)
    {
        Owner = owner;
        ItemIndex = index;
        SetStepState(number, number == owner.Current, automaticStatus, index == 0, index == owner.ItemCount - 1);
    }

    private protected void SetStepState(int number, bool isCurrent, StepsStatus automaticStatus, bool isFirst, bool isLast)
    {
        StepNumber = number;
        IsCurrent = isCurrent;
        AutomaticStatus = automaticStatus;
        EffectiveStatus = Status ?? automaticStatus;
        IsFirst = isFirst;
        IsLast = isLast;
    }

    internal void DetachFromOwner()
    {
        Owner = null;
        ItemIndex = -1;
        StepNumber = 0;
        IsCurrent = false;
        AutomaticStatus = StepsStatus.Wait;
        EffectiveStatus = StepsStatus.Wait;
        IsFirst = false;
        IsLast = false;
        OnOwnerDetached();
    }

    private protected virtual void OnOwnerDetached()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StatusProperty)
        {
            Owner?.HandleItemStatusChanged(this);
        }
    }
}
