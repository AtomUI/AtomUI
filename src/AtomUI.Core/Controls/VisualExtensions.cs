using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Extension methods that look up direct children in the visual tree and observe the visibility of ancestors of a
/// <see cref="Visual" />.
/// </summary>
public static class VisualExtensions
{
    /// <summary>
    /// Returns the first direct visual child of <paramref name="visual" /> that is an instance of
    /// <typeparamref name="T" />. Grandchildren are not searched.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the child to find.
    /// </typeparam>
    /// <param name="visual">
    /// The visual whose direct children are searched.
    /// </param>
    /// <returns>
    /// The first matching child, or <see langword="null" /> when <paramref name="visual" /> is
    /// <see langword="null" /> or no direct child matches.
    /// </returns>
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
    
    /// <summary>
    /// Determines whether <paramref name="searchedChild" /> is a direct visual child of
    /// <paramref name="visual" />.
    /// </summary>
    /// <param name="visual">
    /// The visual whose direct children are checked.
    /// </param>
    /// <param name="searchedChild">
    /// The child to look for.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="searchedChild" /> is a direct child of
    /// <paramref name="visual" />; otherwise <see langword="false" />.
    /// </returns>
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

    /// <summary>
    /// Subscribes <paramref name="handler" /> to the <see cref="Visual.IsVisibleProperty" /> of every ancestor of
    /// <paramref name="visual" />, from its visual parent up to and including the <see cref="TopLevel" />.
    /// </summary>
    /// <param name="visual">
    /// The visual whose ancestors are observed. Its own IsVisible is not.
    /// </param>
    /// <param name="handler">
    /// Receives the IsVisible value of one ancestor when the subscription starts and each time that value changes.
    /// </param>
    /// <param name="disposable">
    /// Receives the subscriptions; disposing it ends them.
    /// </param>
    /// <remarks>
    /// The ancestors are visited once, at the time of the call. An ancestor attached later is not observed, and
    /// an ancestor that is not a <see cref="Control" /> is passed over.
    /// </remarks>
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
