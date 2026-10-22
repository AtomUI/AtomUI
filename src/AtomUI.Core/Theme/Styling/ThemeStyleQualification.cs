using Avalonia;

namespace AtomUI.Theme.Styling;

internal static class ThemeStyleQualification
{
    internal static readonly AttachedProperty<ThemeStyleActivation?> ActiveProperty =
        AvaloniaProperty.RegisterAttached<ThemeConfigProvider, StyledElement, ThemeStyleActivation?>(
            "ActiveStyleGeneration",
            inherits: true);
}
