using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Controls;

/// <summary>
/// Base class of one step in an <see cref="AbstractSteps" />. It carries the header, an optional sub-header and an
/// optional custom icon of the step, and holds the state that the owning steps control assigns to it: the step
/// number, whether it is the current step and the status it displays.
/// A derived class that adds state depending on it resets that state in <see cref="OnOwnerDetached" />.
/// </summary>
public abstract class AbstractStepsItem : HeaderedContentControl
{
    /// <summary>
    /// Identifies the <see cref="SubHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<AbstractStepsItem, object?>(nameof(SubHeader));

    /// <summary>
    /// Identifies the <see cref="SubHeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<AbstractStepsItem, IDataTemplate?>(nameof(SubHeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractStepsItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<StepsStatus?> StatusProperty =
        AvaloniaProperty.Register<AbstractStepsItem, StepsStatus?>(nameof(Status));

    /// <summary>
    /// Secondary text of the step, shown in addition to the header. The sub-header is only present while this value
    /// is not <see langword="null" />, the default.
    /// A string is shown as a wrapping text block. Any other object is presented with <see cref="SubHeaderTemplate" />
    /// or, when that is <see langword="null" />, with the data template the content presenter finds for the value.
    /// </summary>
    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="SubHeader" /> when it is not a string; a string is shown as a text block
    /// without it.
    /// When this is <see langword="null" />, the default, the content presenter uses the data template it finds for
    /// the value.
    /// </summary>
    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Custom icon shown in the step indicator in place of the step number, the finish mark and the error mark.
    /// The default is <see langword="null" />, which keeps those built-in marks.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Status displayed by this step, when it must differ from the status that follows from its position.
    /// When it is <see langword="null" />, the default, the owning steps control derives the status: the current step
    /// shows the status of the control, the steps before it are finished and the steps after it are waiting.
    /// Changing it makes the owner apply the state of this step and of the step before it again.
    /// </summary>
    public StepsStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="StepNumber" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractStepsItem, int> StepNumberProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, int>(nameof(StepNumber), item => item.StepNumber);

    /// <summary>
    /// Identifies the <see cref="IsCurrent" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractStepsItem, bool> IsCurrentProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsCurrent), item => item.IsCurrent);

    internal static readonly DirectProperty<AbstractStepsItem, StepsStatus> AutomaticStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, StepsStatus>(nameof(AutomaticStatus), item => item.AutomaticStatus);

    /// <summary>
    /// Identifies the <see cref="EffectiveStatus" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractStepsItem, StepsStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, StepsStatus>(nameof(EffectiveStatus), item => item.EffectiveStatus);

    /// <summary>
    /// Identifies the <see cref="IsFirst" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractStepsItem, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsFirst), item => item.IsFirst);

    /// <summary>
    /// Identifies the <see cref="IsLast" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractStepsItem, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<AbstractStepsItem, bool>(nameof(IsLast), item => item.IsLast);

    private int _stepNumber;

    /// <summary>
    /// Number of the step within the owning steps control: <see cref="AbstractSteps.Initial" /> plus the zero-based
    /// index of the item. The indicator displays this number plus one, and the step is current when the number equals
    /// <see cref="AbstractSteps.Current" />.
    /// The owner sets the value. It is 0 while the item is not attached to an owner.
    /// </summary>
    public int StepNumber
    {
        get => _stepNumber;
        private set => SetAndRaise(StepNumberProperty, ref _stepNumber, value);
    }

    private bool _isCurrent;

    /// <summary>
    /// Whether the owning steps control marks this step as the one in progress, that is, whether its
    /// <see cref="StepNumber" /> equals <see cref="AbstractSteps.Current" />.
    /// The owner sets the value. It is <c>false</c> while the item is not attached to an owner.
    /// </summary>
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

    /// <summary>
    /// Status displayed by the step: its own <see cref="Status" /> when that is set, and otherwise the status that the
    /// owner derives from the position of the step. The indicator, the title and the connector colors follow it.
    /// The owner sets the value. It is <see cref="StepsStatus.Wait" /> while the item is not attached to an owner.
    /// </summary>
    public StepsStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    private bool _isFirst;

    /// <summary>
    /// Whether this step is the first item of the owning steps control. The panel layout uses it to give the first
    /// step its own border thickness and corner radius, and to omit the notch in front of it.
    /// The owner sets the value. It is <c>false</c> while the item is not attached to an owner.
    /// </summary>
    public bool IsFirst
    {
        get => _isFirst;
        private set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }

    private bool _isLast;

    /// <summary>
    /// Whether this step is the last item of the owning steps control. The panel layout uses it to give the last step
    /// its own border thickness and corner radius, and to hide the arrow after it.
    /// The owner sets the value. It is <c>false</c> while the item is not attached to an owner.
    /// </summary>
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

    /// <summary>
    /// Assigns the state that the owner derives for the step: its number, whether it is the current step, the status
    /// that follows from its position, and whether it is the first or the last step. The displayed
    /// <see cref="EffectiveStatus" /> is the item's own <see cref="Status" /> when that is set, and otherwise the
    /// automatic status.
    /// The owner calls this method whenever it applies state. A derived class can call it to assign the same state
    /// from an entry point of its own.
    /// </summary>
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

    /// <summary>
    /// Called while the step is detached from its owner, after the state received from the owner has been reset.
    /// A derived class resets here any state it derived from that state. The default does nothing.
    /// </summary>
    private protected virtual void OnOwnerDetached()
    {
    }

    /// <summary>
    /// Notifies the owning steps control when <see cref="Status" /> changes, so that the state of this step and of the
    /// step before it is applied again.
    /// An override must call the base implementation; otherwise the owner is not notified.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StatusProperty)
        {
            Owner?.HandleItemStatusChanged(this);
        }
    }
}
