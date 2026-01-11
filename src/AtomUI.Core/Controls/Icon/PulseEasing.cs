using Avalonia.Animation.Easings;

namespace AtomUI.Controls;

public class PulseEasing : Easing
{
    private const int STEPS_COUNT = 8;

    private static readonly IEnumerable<double> Steps = Enumerable
                                                         .Range(0, STEPS_COUNT + 1)
                                                         .Select(index => 1.0 / STEPS_COUNT * index)
                                                         .ToArray();

    public override double Ease(double progress)
    {
        return Steps.Last(step => step <= progress);
    }
}