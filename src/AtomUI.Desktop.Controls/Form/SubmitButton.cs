using Avalonia;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A form button that raises a bubbling <see cref="Submit" /> event when it is clicked. The
/// <see cref="Form" /> that contains the button handles the event with <see cref="Form.Submit" />.
/// Set <see cref="IsWatchValidateResult" /> to disable the button while the form is invalid.
/// </summary>
public class SubmitButton : Button
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsWatchValidateResult" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWatchValidateResultProperty =
        AvaloniaProperty.Register<SubmitButton, bool>(nameof(IsWatchValidateResult), false);
    
    /// <summary>
    /// Gets or sets whether the button is enabled only while the <see cref="Form" /> that owns it is valid.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false" />, with which the button does not change <c>IsEnabled</c>.
    /// The owning form is the nearest <see cref="Form" /> in the logical tree, or else in the visual tree. It is
    /// looked up when the button is attached and whenever this property changes. A button that finds no form when
    /// it is attached is disabled.
    /// </remarks>
    public bool IsWatchValidateResult
    {
        get => GetValue(IsWatchValidateResultProperty);
        set => SetValue(IsWatchValidateResultProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Submit" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> SubmitEvent =
        RoutedEvent.Register<SubmitButton, RoutedEventArgs>(nameof(Submit), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the button is clicked, after the <c>Click</c> event. The event bubbles up the element tree.
    /// </summary>
    /// <remarks>
    /// A <see cref="Form" /> that contains the button handles it: the form calls <see cref="Form.Submit" />, then
    /// marks the event as handled. Handlers on the button and on the elements between the button and the form
    /// run before the submit. Handlers above the form do not receive it unless they are registered for handled
    /// events.
    /// </remarks>
    public event EventHandler<RoutedEventArgs>? Submit
    {
        add => AddHandler(SubmitEvent, value);
        remove => RemoveHandler(SubmitEvent, value);
    }
    #endregion

    private Form? _ownerForm;
    private IDisposable? _formValidSubscription;

    /// <summary>
    /// Calls the base method, then raises <see cref="Submit" />.
    /// </summary>
    /// <remarks>
    /// An override that skips the base method must raise <see cref="Submit" /> itself.
    /// </remarks>
    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(SubmitEvent));
    }

    /// <summary>
    /// Calls the base method, then starts following the owning form while <see cref="IsWatchValidateResult" /> is set.
    /// An override must call the base method to keep this step.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureOwnerForm();
    }

    /// <summary>
    /// Calls the base method, then looks up the owning form again, as <see cref="OnAttachedToLogicalTree" />
    /// does. The lookup falls back to the visual tree, so this step finds a <see cref="Form" /> that is only a
    /// visual ancestor. An override must call the base method to keep this step.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureOwnerForm();
    }

    /// <summary>
    /// Stops following the owning form and disposes the subscription to it, then calls the base method.
    /// An override must call the base method to keep this step.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ReleaseOwnerForm();
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Stops following the owning form, then calls the base method, as <see cref="OnDetachedFromLogicalTree" />
    /// does. An override must call the base method to keep this step.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseOwnerForm();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base method, then starts or stops following the owning form when
    /// <see cref="IsWatchValidateResult" /> changed. An override must call the base method to keep that reaction.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsWatchValidateResultProperty)
        {
            ConfigureOwnerForm();
        }
    }

    private void ConfigureOwnerForm()
    {
        if (!IsWatchValidateResult)
        {
            ReleaseOwnerForm();
            return;
        }

        var ownerForm = this.FindLogicalAncestorOfType<Form>() ?? this.FindAncestorOfType<Form>();
        if (ReferenceEquals(_ownerForm, ownerForm))
        {
            ApplyFormValidState(ownerForm?.IsFormValid);
            return;
        }

        ReleaseOwnerForm();
        _ownerForm = ownerForm;
        if (_ownerForm != null)
        {
            _formValidSubscription = _ownerForm.GetObservable(Form.IsFormValidProperty).Subscribe(ApplyFormValidState);
            ApplyFormValidState(_ownerForm.IsFormValid);
        }
    }

    private void ReleaseOwnerForm()
    {
        _formValidSubscription?.Dispose();
        _formValidSubscription = null;
        _ownerForm             = null;
    }

    private void ApplyFormValidState(bool? isFormValid)
    {
        if (IsWatchValidateResult)
        {
            SetCurrentValue(IsEnabledProperty, isFormValid == true);
        }
    }
}
