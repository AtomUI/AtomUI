using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

public static class VisualExtensions
{
    public static T? FindChildOfType<T>(this Visual? visual) where T : class
    {
        if (visual == null)
        {
            return null;
        }
           
        IEnumerable<Visual> visualChildren = visual.GetVisualChildren();
        foreach (var child in visualChildren)
        {
            if (child is T target)
            {
                return target;
            }
        }
        return null;
    }
    
    public static bool HasChild(this Visual? visual, Visual searchedChild)
    {
        if (visual == null)
        {
            return false;
        }

        IEnumerable<Visual> visualChildren = visual.GetVisualChildren();
        foreach (var child in visualChildren)
        {
            if (child == searchedChild)
            {
                return true;
            }
        }
        return false;
    }

    public static void SubscribeAncestorIsVisible(
        this Visual visual,
        Action<bool> handler,
        CompositeDisposable disposable)
    {
        var current = visual.GetVisualParent();
        while (current is not null)
        {
            if (current is Control control)
            {
                control.GetObservable(Visual.IsVisibleProperty)
                       .Subscribe(handler)
                       .DisposeWith(disposable);
            }

            if (current is TopLevel)
            {
                break;
            }

            current = current.GetVisualParent();
        }
    }

    internal static void TrackEffectiveVisibility(
        this Visual visual,
        Action<bool> handler,
        CompositeDisposable disposable)
    {
        EventHandler<AvaloniaPropertyChangedEventArgs> propertyChangedHandler = (_, args) =>
        {
            if (args.Property == Visual.IsVisibleProperty)
            {
                handler(IsVisualChainVisible(visual));
            }
        };

        Visual? current = visual;
        while (current is not null)
        {
            var subscribedVisual = current;
            subscribedVisual.PropertyChanged += propertyChangedHandler;
            Disposable.Create(() => subscribedVisual.PropertyChanged -= propertyChangedHandler)
                      .DisposeWith(disposable);

            if (current is TopLevel)
            {
                break;
            }

            current = current.GetVisualParent();
        }

        handler(IsVisualChainVisible(visual));
    }

    private static bool IsVisualChainVisible(Visual visual)
    {
        Visual? current = visual;
        while (current is not null)
        {
            if (!current.IsVisible)
            {
                return false;
            }

            if (current is TopLevel)
            {
                break;
            }

            current = current.GetVisualParent();
        }

        return true;
    }

    internal static Visual? GetVisualRoot(this Visual visual)
    {
        return visual.GetPresentationSource()?.RootVisual;
    }

    internal static bool TryGetInputPosition(this Visual visual, RawPointerEventArgs args, out Point position)
    {
        // Process observers run after routed input, which may have detached the target or closed its source.
        if (visual.IsAttachedToVisualTree() &&
            visual.GetPresentationSource() is { RootVisual: { } root } source &&
            ReferenceEquals(source, args.Root) &&
            root.TranslatePoint(args.Position, visual) is { } localPosition)
        {
            position = localPosition;
            return true;
        }

        position = default;
        return false;
    }
    
}
