using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Styling;

namespace AtomUI.Native;

// The internal ValueStore type cannot appear in a net8 UnsafeAccessor signature.
// These six fixed members are statically preserved for NativeAOT; there is no
// assembly/type discovery or generated code. Keep this versioned boundary here
// until upstream retirement visits template inheritance children itself.
internal sealed class StyleFrameRetirement
{
    private static readonly Lazy<Members> Access = new(() => new Members());
    private readonly StyledElement _element;
    private readonly object _store;
    private readonly (object Frame, Style Source)[] _frames;

    private StyleFrameRetirement(StyledElement element, object store, (object, Style)[] frames)
    {
        _element = element;
        _store = store;
        _frames = frames;
    }

    internal static StyleFrameRetirement[] Capture(IEnumerable<StyledElement> elements, Style[] removed)
    {
        var access = Access.Value;
        var sources = new HashSet<Style>(removed, ReferenceEqualityComparer.Instance);
        var result = new List<StyleFrameRetirement>();
        foreach (var element in elements)
        {
            var store = Invoke(access.Store, element)!;
            var frames = new List<(object, Style)>();
            foreach (var frame in (IEnumerable)Invoke(access.Frames, store)!)
            {
                if (access.StyleInstance.IsInstanceOfType(frame) &&
                    Invoke(access.Source, frame) is Style source && sources.Contains(source))
                {
                    frames.Add((frame, source));
                }
            }
            if (frames.Count != 0)
            {
                result.Add(new StyleFrameRetirement(element, store, frames.ToArray()));
            }
        }
        return result.ToArray();
    }

    internal void Retire()
    {
        var access = Access.Value;
        var errors = new List<Exception>();
        Invoke(access.Begin, _store);
        try
        {
            foreach (var (frame, source) in _frames)
            {
                try
                {
                    // Add invalidates old frames across the current inheritance
                    // scope. Shared StyleInstance objects may then be re-used, so
                    // identity alone cannot distinguish their new attachment.
                    if (!IsLive(source))
                    {
                        Invoke(access.Remove, _store, [frame]);
                    }
                }
                catch (Exception exception)
                {
                    errors.Add(exception);
                }
            }
        }
        finally
        {
            try
            {
                Invoke(access.End, _store);
            }
            catch (Exception exception)
            {
                errors.Add(exception);
            }
        }
        if (errors.Count == 1)
        {
            ExceptionDispatchInfo.Capture(errors[0]).Throw();
        }
        if (errors.Count > 1)
        {
            throw new AggregateException("Style frame retirement failed.", errors);
        }
    }

    private bool IsLive(Style source)
    {
        var visited = new HashSet<IStyle>(ReferenceEqualityComparer.Instance);
        bool Contains(IStyle style)
        {
            return visited.Add(style) &&
                   (ReferenceEquals(style, source) || style.Children.Any(Contains));
        }
        for (IStyleHost? host = _element; host is not null; host = host.StylingParent)
        {
            if (host.IsStylesInitialized && Contains(host.Styles))
            {
                return true;
            }
        }
        return false;
    }

    private static object? Invoke(MethodInfo method, object target, object?[]? arguments = null) =>
        method.Invoke(target, BindingFlags.DoNotWrapExceptions, null, arguments, null);

    private sealed class Members
    {
        internal readonly MethodInfo Store;
        internal readonly MethodInfo Frames;
        internal readonly MethodInfo Source;
        internal readonly MethodInfo Remove;
        internal readonly MethodInfo Begin;
        internal readonly MethodInfo End;
        internal readonly Type StyleInstance;

        [DynamicDependency("GetValueStore", typeof(AvaloniaObject))]
        [DynamicDependency("get_Frames", "Avalonia.PropertyStore.ValueStore", "Avalonia.Base")]
        [DynamicDependency("get_Source", "Avalonia.Styling.StyleInstance", "Avalonia.Base")]
        [DynamicDependency("RemoveFrame", "Avalonia.PropertyStore.ValueStore", "Avalonia.Base")]
        [DynamicDependency("BeginStyling", "Avalonia.PropertyStore.ValueStore", "Avalonia.Base")]
        [DynamicDependency("EndStyling", "Avalonia.PropertyStore.ValueStore", "Avalonia.Base")]
        internal Members()
        {
            Store = typeof(AvaloniaObject).GetMethod("GetValueStore", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var store = Type.GetType("Avalonia.PropertyStore.ValueStore, Avalonia.Base", true)!;
            var frame = Type.GetType("Avalonia.PropertyStore.ValueFrame, Avalonia.Base", true)!;
            StyleInstance = Type.GetType("Avalonia.Styling.StyleInstance, Avalonia.Base", true)!;
            Frames = store.GetMethod("get_Frames", Type.EmptyTypes)!;
            Source = Type.GetType("Avalonia.Styling.StyleInstance, Avalonia.Base", true)!.GetMethod("get_Source", Type.EmptyTypes)!;
            Remove = store.GetMethod("RemoveFrame", [frame])!;
            Begin = store.GetMethod("BeginStyling", Type.EmptyTypes)!;
            End = store.GetMethod("EndStyling", Type.EmptyTypes)!;
            if (Store is null || Frames is null || Source is null || Remove is null || Begin is null || End is null)
            {
                throw new NotSupportedException("The Avalonia style frame contract has changed.");
            }
        }
    }
}
