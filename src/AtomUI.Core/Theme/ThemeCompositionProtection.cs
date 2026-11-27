using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Theme;

/// <summary>
/// Keeps an existing editor's presentation and open candidate popups while a deferred native composition owns it.
/// </summary>
internal sealed class ThemeCompositionProtection
{
    private static Dictionary<Control, ThemeCompositionProtection>? s_compositions;

    private readonly Control _editor;
    private readonly Dictionary<Control, TargetHold> _popupHolds = [];
    private TargetHold? _boundaryHold;
    private ThemeContext? _underlyingContext;
    private ThemeSnapshot? _baselineSnapshot;
    private ThemeVariant? _baselineVariant;
    private ThemeContext? _frozenContext;
    private bool _ending;

    private ThemeCompositionProtection(Control editor)
    {
        _editor = editor;
        editor.DetachedFromVisualTree += OnDetached;
        editor.LostFocus += OnLostFocus;
    }

    internal static void Track(Control editor, bool composing)
    {
        if (s_compositions?.TryGetValue(editor, out var current) == true)
        {
            current._ending = !composing;
            if (current._ending)
            {
                // The platform can clear preedit immediately before it delivers the committed text.
                Dispatcher.UIThread.Post(current.ReleaseIfEnding, DispatcherPriority.Input);
            }
        }
        else if (composing && editor.IsAttachedToVisualTree())
        {
            (s_compositions ??= []).Add(editor, new ThemeCompositionProtection(editor));
        }
    }

    internal static IReadOnlyList<Prepared>? Prepare(
        ThemeManager manager,
        ThemeContext? root,
        ThemeSnapshot? nextRoot,
        IReadOnlyList<ThemeScopeStagedSnapshot> scopes)
    {
        if (s_compositions is not { Count: > 0 } active)
        {
            return null;
        }

        List<Prepared>? changes = null;
        foreach (var composition in active.Values)
        {
            if (composition._ending || !composition._editor.IsAttachedToVisualTree())
            {
                continue;
            }

            Control? candidateBoundary = null;
            ThemeContext? candidateContext = null;
            ThemeSnapshot? candidatePrevious = null;
            ThemeVariant candidateVariant = ThemeVariant.Default;

            var existingBoundary = composition._boundaryHold?.Target;
            var insideExistingBoundary = existingBoundary is not null;

            foreach (var element in composition._editor.GetSelfAndVisualAncestors().OfType<Control>())
            {
                var context = insideExistingBoundary && composition._underlyingContext is not null
                    ? composition._underlyingContext
                    : ThemeScope.ResolveContext(element);
                if (ReferenceEquals(element, existingBoundary))
                {
                    insideExistingBoundary = false;
                }

                if (context is null || context.IsCompositionSnapshot || !ReferenceEquals(context.Manager, manager))
                {
                    continue;
                }

                var shouldInspectElement = ReferenceEquals(element, existingBoundary) || manager.UsesScopedControlTheme(element);
                if (!shouldInspectElement)
                {
                    continue;
                }

                var next = ReferenceEquals(context, root)
                    ? nextRoot
                    : scopes.FirstOrDefault(scope => ReferenceEquals(scope.Context, context))?.Snapshot;
                var baseline = composition._baselineSnapshot is not null && ReferenceEquals(context, composition._underlyingContext)
                    ? composition._baselineSnapshot
                    : context.Snapshot;
                if (next is not null && manager.ChangesScopedControlTheme(element.StyleKey, baseline, next))
                {
                    candidateBoundary = element;
                    candidateContext = context;
                    candidatePrevious = baseline;
                    candidateVariant = composition._baselineVariant ?? element.ActualThemeVariant;
                }
            }

            if (candidateBoundary is not null && candidateContext is not null && candidatePrevious is not null)
            {
                if (existingBoundary is null ||
                    (!ReferenceEquals(candidateBoundary, existingBoundary) &&
                     candidateBoundary.IsVisualAncestorOf(existingBoundary)))
                {
                    (changes ??= []).Add(new Prepared(
                        composition,
                        candidateBoundary,
                        candidateContext,
                        candidatePrevious,
                        candidateVariant));
                    continue;
                }
            }

            if (existingBoundary is not null &&
                composition._underlyingContext is { } heldContext &&
                composition._baselineSnapshot is { } heldSnapshot &&
                composition._baselineVariant is { } heldVariant)
            {
                // Refresh popup holds when a second theme switch occurs while an existing boundary remains active.
                (changes ??= []).Add(new Prepared(
                    composition,
                    existingBoundary,
                    heldContext,
                    heldSnapshot,
                    heldVariant));
            }
        }
        return changes;
    }

    internal sealed record Prepared(
        ThemeCompositionProtection Composition,
        Control Boundary,
        ThemeContext Context,
        ThemeSnapshot Previous,
        ThemeVariant Variant)
    {
        internal void Apply() => Composition.Hold(Boundary, Context, Previous, Variant);
    }

    private void Hold(Control boundary, ThemeContext context, ThemeSnapshot previous, ThemeVariant variant)
    {
        if (_ending || !boundary.IsAttachedToVisualTree())
        {
            return;
        }

        _underlyingContext ??= context;
        _baselineSnapshot ??= previous;
        _baselineVariant ??= variant;
        _frozenContext ??= new ThemeContext(context.Manager, _baselineSnapshot, context.RegistrationId)
        {
            IsCompositionSnapshot = true
        };

        var effectiveVariant = _baselineVariant;
        if (_boundaryHold is null)
        {
            _boundaryHold = CreateTargetHold(boundary, _frozenContext, effectiveVariant, attachDetachHandler: true);
        }
        else if (!ReferenceEquals(_boundaryHold.Target, boundary))
        {
            var previousHold = _boundaryHold;
            _boundaryHold = CreateTargetHold(boundary, _frozenContext, effectiveVariant, attachDetachHandler: true);
            previousHold.Target.DetachedFromVisualTree -= OnDetached;
            previousHold.Dispose();
        }

        HoldOpenPopups(boundary, _frozenContext, effectiveVariant);
    }

    private void HoldOpenPopups(Control boundary, ThemeContext frozen, ThemeVariant variant)
    {
        var boundaryRoot = TopLevel.GetTopLevel(boundary);
        var popups = new HashSet<Popup>(ReferenceEqualityComparer.Instance);
        foreach (var visualDescendant in boundary.GetSelfAndVisualDescendants().OfType<Popup>())
        {
            popups.Add(visualDescendant);
        }
        foreach (var logicalDescendant in boundary.GetSelfAndLogicalDescendants().OfType<Popup>())
        {
            popups.Add(logicalDescendant);
        }

        foreach (var popup in popups)
        {
            if (!popup.IsOpen || popup.Child is not { } child)
            {
                continue;
            }

            if (!boundary.IsVisualAncestorOf(popup) && !_popupHolds.ContainsKey(popup))
            {
                _popupHolds.Add(popup, CreateTargetHold(popup, frozen, variant, attachDetachHandler: false));
            }

            if (ResolvePopupPresentationHost(child, boundaryRoot) is { } hostControl &&
                !boundary.IsVisualAncestorOf(hostControl) &&
                !_popupHolds.ContainsKey(hostControl))
            {
                _popupHolds.Add(hostControl, CreateTargetHold(hostControl, frozen, variant, attachDetachHandler: false));
            }
        }
    }

    private static Control? ResolvePopupPresentationHost(Control popupChild, TopLevel? boundaryRoot)
    {
        Control? outermost = null;
        foreach (var ancestor in popupChild.GetSelfAndVisualAncestors().OfType<Control>())
        {
            if (ancestor is TopLevel && !ReferenceEquals(ancestor, boundaryRoot))
            {
                return ancestor;
            }
            if (ancestor.GetVisualParent() is OverlayLayer)
            {
                return ancestor;
            }
            outermost = ancestor;
        }
        return outermost;
    }

    private TargetHold CreateTargetHold(Control target, ThemeContext frozen, ThemeVariant variant, bool attachDetachHandler)
    {
        var resources = new FrozenResources(target, frozen.ResourceProvider, variant);
        var contextOverride = target.SetValue(ThemeScope.ContextProperty, frozen, BindingPriority.Animation)!;
        var variantOverride = target.SetValue(ThemeVariantScope.ActualThemeVariantProperty, variant, BindingPriority.Animation)!;
        // Preserve keys defined by the target and all of its existing merged dictionaries.
        target.Resources.MergedDictionaries.Insert(0, resources);
        if (attachDetachHandler)
        {
            target.DetachedFromVisualTree += OnDetached;
        }
        return new TargetHold(target, resources, [contextOverride, variantOverride]);
    }

    private void OnLostFocus(object? sender, FocusChangedEventArgs args) => Track(_editor, false);
    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs args) => Release();

    private void ReleaseIfEnding()
    {
        if (_ending)
        {
            Release();
        }
    }

    private void Release()
    {
        if (s_compositions?.TryGetValue(_editor, out var active) != true || !ReferenceEquals(active, this))
        {
            return;
        }
        s_compositions.Remove(_editor);
        _editor.DetachedFromVisualTree -= OnDetached;
        _editor.LostFocus -= OnLostFocus;

        foreach (var popupHold in _popupHolds.Values)
        {
            popupHold.Dispose();
        }
        _popupHolds.Clear();

        if (_boundaryHold is { } boundaryHold)
        {
            _boundaryHold = null;
            boundaryHold.Target.DetachedFromVisualTree -= OnDetached;
            boundaryHold.Dispose();
        }

        _underlyingContext = null;
        _baselineSnapshot = null;
        _baselineVariant = null;
        _frozenContext = null;
    }

    private sealed record TargetHold(Control Target, FrozenResources Resources, IReadOnlyList<IDisposable> Overrides) : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in Overrides)
            {
                item.Dispose();
            }
            Target.Resources.MergedDictionaries.Remove(Resources);
        }
    }

    private sealed class FrozenResources(Control boundary, ThemeTokenResourceProvider previous, ThemeVariant variant) : ResourceProvider
    {
        public override bool HasResources => true;

        public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
        {
            value = null;
            if (!previous.ContainsToken(key))
            {
                return false;
            }
            if ((boundary as IStyleHost).StylingParent is StyledElement parent &&
                ThemeTokenResourceProvider.TryFindApplicationResource(parent, key, variant, out value))
            {
                return true;
            }
            return previous.TryGetResource(key, variant, out value);
        }
    }
}
