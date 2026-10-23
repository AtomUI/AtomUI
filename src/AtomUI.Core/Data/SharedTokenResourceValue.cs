using System.Diagnostics;
using AtomUI.Animations;
using Avalonia.Animation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace AtomUI.Data;

public class SharedTokenResourceValue
{
    public SharedTokenKind? Kind { get; set; }
    
    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        Debug.Assert(Kind != null);
        var provideTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        Debug.Assert(provideTarget != null);
        var targetObject = provideTarget.TargetObject as AvaloniaObject;
        Debug.Assert(targetObject != null);
        var targetProperty = provideTarget.TargetProperty as AvaloniaProperty;
        Debug.Assert(targetProperty != null);
        
        // Preserve the declared resource input for owner-bound Transition collections.
        // Legacy standalone literal use still returns the same application value below.
        if (targetObject is TransitionBase transition && targetProperty == TransitionBase.DurationProperty && Kind is { } kind)
        {
            transition.SetValue(TokenTransitionsExtension.DurationInputKindProperty, kind);
        }

        var application = Application.Current;
        Debug.Assert(application != null);
        var themeVariant = application.ActualThemeVariant;
        
        if (application.TryGetResource(Kind, themeVariant, out var value))
        {
            return value;
        }
        return null;
    }
}