using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class PickerClearUpButton : TemplatedControl
{
    public event EventHandler? ClearRequest;

    public static readonly StyledProperty<bool> IsInClearModeProperty =
        AvaloniaProperty.Register<PickerClearUpButton, bool>(nameof(IsInClearMode));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<PickerClearUpButton, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<PickerClearUpButton, FormValidateFeedback?>(nameof(FormFeedback));

    public static readonly DirectProperty<PickerClearUpButton, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<PickerClearUpButton, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible);

    /// <summary>
    /// Size of the info icon. The icon is a registered IconPresenter, so the theme reads the picker's value on this
    /// button and template-binds it down. The theme supplies it from the <c>IconSize</c> Token; <c>NaN</c>, the
    /// registered default, does not mean that size: it reaches the icon as an unset <c>Width</c> and <c>Height</c>.
    /// </summary>
    internal static readonly StyledProperty<double> InfoIconSizeProperty =
        AvaloniaProperty.Register<PickerClearUpButton, double>(nameof(InfoIconSize), double.NaN);

    /// <summary>
    /// Brush of the info icon (read on this button for the same reason as <see cref="InfoIconSize" />). The theme
    /// supplies it from the <c>ColorTextQuaternary</c> Token.
    /// </summary>
    internal static readonly StyledProperty<IBrush?> InfoIconBrushProperty =
        AvaloniaProperty.Register<PickerClearUpButton, IBrush?>(nameof(InfoIconBrush));

    internal double InfoIconSize
    {
        get => GetValue(InfoIconSizeProperty);
        set => SetValue(InfoIconSizeProperty, value);
    }

    internal IBrush? InfoIconBrush
    {
        get => GetValue(InfoIconBrushProperty);
        set => SetValue(InfoIconBrushProperty, value);
    }

    public bool IsInClearMode
    {
        get => GetValue(IsInClearModeProperty);
        set => SetValue(IsInClearModeProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    private bool _isFormFeedbackVisible;

    public bool IsFormFeedbackVisible
    {
        get => _isFormFeedbackVisible;
        private set => SetAndRaise(IsFormFeedbackVisibleProperty, ref _isFormFeedbackVisible, value);
    }

    private IconButton? _clearButton;
    private IDisposable? _feedbackStatusSubscription;

    static PickerClearUpButton()
    {
        Button.ClickEvent.AddClassHandler<PickerClearUpButton>(
            (clearUpButton, args) => clearUpButton.HandleButtonClick(args));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _clearButton = e.NameScope.Get<IconButton>("PART_ClearButton");
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

    private void HandleButtonClick(RoutedEventArgs args)
    {
        if (IsInClearMode && ReferenceEquals(args.Source, _clearButton))
        {
            ClearRequest?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FormFeedbackProperty)
        {
            ConfigureFormFeedbackSubscription();
        }
    }

    private void ConfigureFormFeedbackSubscription()
    {
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
        if (!((ILogical)this).IsAttachedToLogicalTree)
        {
            IsFormFeedbackVisible = false;
            return;
        }

        if (FormFeedback is { } feedback)
        {
            _feedbackStatusSubscription = feedback.GetObservable(FormValidateFeedback.ValidateStatusProperty)
                                                  .Subscribe(status => IsFormFeedbackVisible = status != FormValidateStatus.Default);
        }
        else
        {
            IsFormFeedbackVisible = false;
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }
}
