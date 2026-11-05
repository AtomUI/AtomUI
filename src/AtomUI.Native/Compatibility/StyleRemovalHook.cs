using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Native;

// Avalonia 12.1.4 adds styles through the inheritance tree, but retires frames
// through logical children only. Keep the official notification and retire the
// same sources from its pre-notification inheritance scope, including templates.
internal sealed class StyleRemovalHook : IDisposable
{
    private static readonly ConditionalWeakTable<Styles, Entry> Entries = new();
    private Entry? _entry;

    private StyleRemovalHook(Entry entry)
    {
        _entry = entry;
        entry.Users++;
    }

    internal static IDisposable Attach(IStyleHost host)
    {
        ((AvaloniaObject)host).VerifyAccess();
        return Attach(host.Styles);
    }

    private static IDisposable Attach(Styles styles)
    {
        if (!Entries.TryGetValue(styles, out var entry))
        {
            entry = new Entry(styles);
            Entries.Add(styles, entry);
        }
        return new StyleRemovalHook(entry);
    }

    public void Dispose()
    {
        if (_entry is not { } entry)
        {
            return;
        }
        entry.Styles.VerifyAccess();
        _entry = null;
        if (--entry.Users == 0)
        {
            entry.Dispose();
            Entries.Remove(entry.Styles);
        }
    }

    private sealed class Entry : IDisposable
    {
        private readonly NotifyCollectionChangedEventHandler _original;
        private readonly NotifyCollectionChangedEventHandler _wrapper;
        private readonly Dictionary<Styles, IDisposable> _children = new(ReferenceEqualityComparer.Instance);
        // A preceding Closed subscriber can throw before our release callback.
        // Ephemeron ownership must therefore allow an already closed window to
        // collect even if that notification chain never reaches this entry.
        private readonly ConditionalWeakTable<Window, IDisposable> _windows = new();
        private IDisposable? _windowObserver;
        private NotifyCollectionChangedEventArgs? _retirementArgs;
        private Action? _completeRetirement;
        private bool _disposed;

        internal Entry(Styles styles)
        {
            Styles = styles;
            ref var subscribers = ref Subscribers(Items(styles));
            // The Styles constructor installs its own notification first. Replace
            // that exact slot; event -=/+= would reorder other list subscribers.
            _original = subscribers?.GetInvocationList()
                .OfType<NotifyCollectionChangedEventHandler>()
                .FirstOrDefault(handler => ReferenceEquals(handler.Target, styles))
                ?? throw new NotSupportedException("The Avalonia Styles notification contract has changed.");
            _wrapper = HandleCollectionChanged;
            subscribers = Replace(subscribers, _original, _wrapper);
            try
            {
                styles.CollectionChanged += HandleStylesChanged;
                if (styles.Owner is Application application && ReferenceEquals(application.Styles, styles))
                {
                    // Plain Avalonia Windows do not acquire AtomUI context leases.
                    // Observe their first visibility as well, and release on Close.
                    var weakEntry = new WeakReference<Entry>(this);
                    IDisposable? observer = null;
                    observer = Visual.IsVisibleProperty.Changed.AddClassHandler<Window>((window, _) =>
                    {
                        // The property stream is process-wide. Its observer must
                        // not root an Application when an external host discards
                        // its application/locator scope without a managed shutdown.
                        if (!weakEntry.TryGetTarget(out var entry))
                        {
                            observer?.Dispose();
                        }
                        else if (ReferenceEquals(Application.Current, entry.Styles.Owner) && window.IsVisible)
                        {
                            entry.TrackWindow(window);
                        }
                    });
                    _windowObserver = observer;
                    foreach (var callback in GlobalRemovalSubscribers(application)?.GetInvocationList() ?? [])
                    {
                        if (callback.Target is Window window)
                        {
                            TrackWindow(window);
                        }
                    }
                }
                SynchronizeChildren(remove: false);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal Styles Styles { get; }
        internal int Users { get; set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Styles.CollectionChanged -= HandleStylesChanged;
            _windowObserver?.Dispose();
            _windowObserver = null;
            foreach (var window in _windows)
            {
                window.Key.Closed -= HandleWindowClosed;
                window.Value.Dispose();
            }
            _windows.Clear();
            ref var subscribers = ref Subscribers(Items(Styles));
            subscribers = Replace(subscribers, _wrapper, _original);
            foreach (var child in _children.Values)
            {
                child.Dispose();
            }
            _children.Clear();
        }

        private void TrackWindow(Window window)
        {
            if (_disposed || _windows.TryGetValue(window, out _))
            {
                return;
            }
            _windows.Add(window, Attach((IStyleHost)window));
            window.Closed += HandleWindowClosed;
        }

        private void HandleWindowClosed(object? sender, EventArgs args)
        {
            if (sender is Window window && _windows.TryGetValue(window, out var hook))
            {
                _windows.Remove(window);
                window.Closed -= HandleWindowClosed;
                hook.Dispose();
            }
        }

        private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (_disposed)
            {
                _original(sender, args);
                return;
            }

            var removed = args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace
                ? Flatten(args.OldItems!.Cast<IStyle>())
                : [];
            var scope = removed.Length != 0
                ? StyleFrameRetirement.Capture(CaptureScope(Styles.Owner), removed)
                : [];
            var errors = new List<Exception>();
            var previousArgs = _retirementArgs;
            var previousCompletion = _completeRetirement;
            var completed = false;
            void CompleteRetirement()
            {
                if (completed)
                {
                    return;
                }
                completed = true;
                var retirementErrors = new List<Exception>();
                foreach (var element in scope)
                {
                    // A notification may re-add the source. Only captured frames
                    // that no longer belong to the live scope may be retired.
                    Try(element.Retire, retirementErrors);
                }
                if (retirementErrors.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(retirementErrors[0]).Throw();
                }
                if (retirementErrors.Count > 1)
                {
                    throw new AggregateException("Style frame retirement failed.", retirementErrors);
                }
            }
            _retirementArgs = args;
            _completeRetirement = CompleteRetirement;
            try
            {
                // Install new nested hooks before framework callbacks can mutate
                // them. Retire before consumers see the public Styles event;
                // exceptions/disposal before that event still use the fallback.
                Try(() => SynchronizeChildren(remove: false), errors);
                Try(() => _original(sender, args), errors);
                Try(CompleteRetirement, errors);
                if (!_disposed)
                {
                    Try(() => SynchronizeChildren(remove: true), errors);
                }
            }
            finally
            {
                _retirementArgs = previousArgs;
                _completeRetirement = previousCompletion;
            }
            if (errors.Count == 1)
            {
                ExceptionDispatchInfo.Capture(errors[0]).Throw();
            }
            if (errors.Count > 1)
            {
                throw new AggregateException("Style notification and retirement failed.", errors);
            }
        }

        private void HandleStylesChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (ReferenceEquals(_retirementArgs, args))
            {
                _completeRetirement?.Invoke();
            }
        }

        private void SynchronizeChildren(bool remove)
        {
            var current = new HashSet<Styles>(ReferenceEqualityComparer.Instance);
            var visited = new HashSet<IStyle>(ReferenceEqualityComparer.Instance) { Styles };
            void Collect(IStyle style)
            {
                if (!visited.Add(style))
                {
                    return;
                }
                if (style is Styles collection)
                {
                    current.Add(collection);
                    return;
                }
                foreach (var child in style.Children)
                {
                    Collect(child);
                }
            }
            foreach (var child in Styles)
            {
                Collect(child);
            }
            foreach (var child in current)
            {
                if (!_children.ContainsKey(child))
                {
                    _children.Add(child, Attach(child));
                }
            }
            if (remove)
            {
                foreach (var child in _children.Keys.Where(child => !current.Contains(child)).ToArray())
                {
                    _children[child].Dispose();
                    _children.Remove(child);
                }
            }
        }
    }

    private static Style[] Flatten(IEnumerable<IStyle> styles)
    {
        var result = new List<Style>();
        var visited = new HashSet<IStyle>(ReferenceEqualityComparer.Instance);
        void Collect(IStyle style)
        {
            if (!visited.Add(style))
            {
                return;
            }
            if (style is Style source)
            {
                result.Add(source);
            }
            foreach (var child in style.Children)
            {
                Collect(child);
            }
        }
        foreach (var style in styles)
        {
            Collect(style);
        }
        return result.ToArray();
    }

    private static StyledElement[] CaptureScope(IResourceHost? owner)
    {
        var result = new List<StyledElement>();
        var visited = new HashSet<AvaloniaObject>(ReferenceEqualityComparer.Instance);
        void Collect(AvaloniaObject element)
        {
            if (!visited.Add(element))
            {
                return;
            }
            if (element is StyledElement styled)
            {
                result.Add(styled);
            }
            if (InheritanceChildren(element) is { } children)
            {
                foreach (var child in children)
                {
                    Collect(child);
                }
            }
        }
        if (owner is Application application)
        {
            // TopLevel registers its IStyleHost notification as an instance
            // delegate. Snapshot those roots before any callback can close one.
            foreach (var callback in GlobalRemovalSubscribers(application)?.GetInvocationList() ?? [])
            {
                if (callback.Target is StyledElement root)
                {
                    Collect(root);
                }
            }
        }
        else if (owner is StyledElement root)
        {
            Collect(root);
        }
        return result.ToArray();
    }

    private static NotifyCollectionChangedEventHandler? Replace(
        NotifyCollectionChangedEventHandler? subscribers,
        NotifyCollectionChangedEventHandler previous,
        NotifyCollectionChangedEventHandler replacement)
    {
        NotifyCollectionChangedEventHandler? result = null;
        foreach (var subscriber in subscribers?.GetInvocationList() ?? [])
        {
            result += subscriber.Equals(previous)
                ? replacement
                : (NotifyCollectionChangedEventHandler)subscriber;
        }
        return result;
    }

    private static void Try(Action action, List<Exception> errors)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    // These statically resolved private boundaries are intentionally isolated in
    // Native. They require net8+ and work without reflection or runtime codegen.
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_styles")]
    private static extern ref AvaloniaList<IStyle> Items(Styles styles);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_collectionChanged")]
    private static extern ref NotifyCollectionChangedEventHandler? Subscribers(AvaloniaList<IStyle> list);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetInheritanceChildren")]
    private static extern IReadOnlyList<AvaloniaObject>? InheritanceChildren(AvaloniaObject owner);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stylesRemoved")]
    private static extern ref Action<IReadOnlyList<IStyle>>? GlobalRemovalSubscribers(Application application);
}
