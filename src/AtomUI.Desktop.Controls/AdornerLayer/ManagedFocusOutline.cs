using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows the themed keyboard focus outline (<see cref="AdornerLayer.DefaultFocusAdorner" />) on an element
/// that does not own keyboard focus itself, such as the keyboard active item of a host that keeps focus.
/// The owner must call <see cref="Hide" /> when the target leaves the visual tree.
/// </summary>
internal sealed class ManagedFocusOutline
{
    private Control? _adorner;

    public Visual? Target { get; private set; }

    public bool IsShown => _adorner?.Parent is not null;

    public void Show(Visual target)
    {
        if (ReferenceEquals(Target, target) && IsShown)
        {
            return;
        }

        Hide();
        // The adorner layer of the target's own root, so items inside popups are outlined in the popup.
        if (AdornerLayer.GetAdornerLayer(target) is not { DefaultFocusAdorner: { } template } adornerLayer)
        {
            return;
        }

        _adorner = template.Build();
        AdornerLayer.SetAdornedElement(_adorner, target);
        adornerLayer.Children.Add(_adorner);
        Target = target;
    }

    public void Hide()
    {
        if (_adorner?.Parent is Panel adornerLayer)
        {
            // AdornerLayer tracks the adorned element and all its ancestors, and releases that tracking only
            // when the adorned element changes while the adorner is still in the layer; removal alone leaks it.
            AdornerLayer.SetAdornedElement(_adorner, null);
            adornerLayer.Children.Remove(_adorner);
        }

        _adorner = null;
        Target   = null;
    }
}
