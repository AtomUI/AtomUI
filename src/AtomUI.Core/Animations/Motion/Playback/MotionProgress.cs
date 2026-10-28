using Avalonia;
using Avalonia.Animation;

namespace AtomUI.Animations;

internal sealed class MotionProgress : Animatable
{
    internal static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<MotionProgress, double>("Value");

    internal Action<double>? Tick { get; set; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            Tick?.Invoke(change.GetNewValue<double>());
        }
    }
}
