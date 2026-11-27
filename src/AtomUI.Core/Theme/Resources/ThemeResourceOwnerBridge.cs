using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Local resource host of the theme resource dictionaries. During assembly an update refreshes only the
/// bindings of the dictionaries, and the resource root is notified once by the theme commit.
/// The subscriptions taken on the owner are released again on detach; resource changes raised at run time
/// are still forwarded to the resource root.
/// </summary>
internal sealed class ThemeResourceOwnerBridge : IThemeVariantHost
{
    private IResourceHost? _owner;
    private int _updateDepth;
    private bool _resourcesChanged;

    public bool HasResources => _owner?.HasResources == true;
    public ThemeVariant ActualThemeVariant => (_owner as IThemeVariantHost)?.ActualThemeVariant ?? ThemeVariant.Default;
    public event EventHandler<ResourcesChangedEventArgs>? ResourcesChanged;
    public event EventHandler? ActualThemeVariantChanged;

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        // Mirror the lookup entry of the real owner: go back to the host first, so that application overrides
        // win over the static dictionaries. Do not query an inner or sibling dictionary here first, or
        // DynamicResource would bypass the application overrides.
        if (_owner is not null)
        {
            return _owner.TryFindResource(key, theme, out value);
        }
        value = null;
        return false;
    }

    internal void SetOwner(IResourceHost? owner)
    {
        if (ReferenceEquals(owner, _owner))
        {
            return;
        }
        var previous = _owner;
        _owner = null;
        var failures = new List<Exception>();
        Unsubscribe(previous, failures);
        ThrowFailures(failures);
        _owner = owner;
        try
        {
            if (owner is not null)
            {
                owner.ResourcesChanged += HandleResourcesChanged;
                if (owner is IThemeVariantHost nextVariant)
                {
                    nextVariant.ActualThemeVariantChanged += HandleThemeVariantChanged;
                }
            }
            HandleThemeVariantChanged(this, EventArgs.Empty);
            NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
        }
        catch (Exception exception)
        {
            failures.Add(exception);
            _owner = null;
            Unsubscribe(owner, failures);
            ThrowFailures(failures);
        }
    }

    private void Unsubscribe(IResourceHost? owner, ICollection<Exception> failures)
    {
        if (owner is not null)
        {
            TryCleanup(() => owner.ResourcesChanged -= HandleResourcesChanged, failures);
            if (owner is IThemeVariantHost variant)
            {
                TryCleanup(() => variant.ActualThemeVariantChanged -= HandleThemeVariantChanged, failures);
            }
        }
    }

    internal void RunUpdate(Action update)
    {
        var failures = new List<Exception>();
        BeginUpdate();
        try
        {
            TryCleanup(update, failures);
        }
        finally
        {
            // EndUpdate notifies resource bindings too; an exception from it must not hide the original
            // attach or release failure.
            TryCleanup(EndUpdate, failures);
        }
        ThrowFailures(failures);
    }

    internal static void TryCleanup(Action cleanup, ICollection<Exception> failures)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }

    internal static void ThrowFailures(IReadOnlyList<Exception> failures)
    {
        if (failures.Count == 1)
        {
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }
        if (failures.Count > 1)
        {
            throw new AggregateException("Theme resource ownership operation failed.", failures);
        }
    }

    private void BeginUpdate() => _updateDepth++;

    private void EndUpdate()
    {
        if (--_updateDepth == 0 && _resourcesChanged)
        {
            _resourcesChanged = false;
            ResourcesChanged?.Invoke(this, ResourcesChangedEventArgs.Create());
        }
    }

    public void NotifyHostedResourcesChanged(ResourcesChangedEventArgs e)
    {
        if (_updateDepth > 0)
        {
            _resourcesChanged = true;
        }
        else if (_owner is not null)
        {
            _owner.NotifyHostedResourcesChanged(e);
        }
        else
        {
            ResourcesChanged?.Invoke(this, e);
        }
    }

    private void HandleResourcesChanged(object? sender, ResourcesChangedEventArgs e) => ResourcesChanged?.Invoke(this, e);
    private void HandleThemeVariantChanged(object? sender, EventArgs e) => ActualThemeVariantChanged?.Invoke(this, e);
}
