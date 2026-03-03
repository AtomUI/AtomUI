using Avalonia;

namespace AtomUI.Controls;

public interface IInputControlStatusAware
{
    InputControlStatus Status { get; set; }
}

public abstract class InputControlStatusProperty
{
    public const string StatusPropertyName = "Status";
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        AvaloniaProperty.Register<StyledElement, InputControlStatus>(StatusPropertyName, InputControlStatus.Default);
}
