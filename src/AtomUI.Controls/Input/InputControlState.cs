using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Shared Avalonia property owner for input validation state projected by
/// <see cref="AbstractTextInput" /> and product input frames.
/// </summary>
internal abstract class InputControlState : AvaloniaObject
{
    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        AvaloniaProperty.Register<InputControlState, FormValidateStatus>(
            "FormStatus",
            FormValidateStatus.Default);

    internal static InputControlStatus ResolveEffectiveStatus(
        Control owner,
        InputControlStatus explicitStatus,
        FormValidateStatus formStatus)
    {
        if (DataValidationErrors.GetHasErrors(owner) ||
            formStatus == FormValidateStatus.Error)
        {
            return InputControlStatus.Error;
        }

        if (formStatus == FormValidateStatus.Warning ||
            explicitStatus == InputControlStatus.Warning)
        {
            return InputControlStatus.Warning;
        }

        return explicitStatus == InputControlStatus.Error
            ? InputControlStatus.Error
            : InputControlStatus.Default;
    }
}
