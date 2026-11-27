using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme;

/// <summary>
/// A resource provider that resolves the lookups of its host through the resource provider of one
/// <see cref="ThemeContext" />, and raises a resources-changed notification when that context publishes.
/// <see cref="ThemeContextLease" /> merges it into the resources of the leased host; the drawer container of
/// AtomUI.Desktop.Controls keeps one. After <c>Dispose</c> it reports no resources and every lookup fails.
/// </summary>
internal sealed class ThemeContextResourceBridge : ResourceProvider, IDisposable
{
    private ThemeContext? _ownerContext;

    internal ThemeContextResourceBridge(ThemeContext ownerContext)
    {
        _ownerContext = ownerContext ?? throw new ArgumentNullException(nameof(ownerContext));
        ownerContext.Published += HandleContextPublished;
    }

    internal void RestartListening()
    {
        if (_ownerContext is { } context)
        {
            context.Published -= HandleContextPublished;
            context.Published += HandleContextPublished;
        }
    }

    internal ThemeContext OwnerContext => _ownerContext ??
        throw new ObjectDisposedException(nameof(ThemeContextResourceBridge));

    public override bool HasResources => _ownerContext is not null;

    public override bool TryGetResource(
        object key,
        ThemeVariant? theme,
        out object? value)
    {
        var context = _ownerContext;
        if (context is null)
        {
            value = null;
            return false;
        }
        return context.ResourceProvider.TryGetResource(key, theme, out value);
    }

    public void Dispose()
    {
        var context = Interlocked.Exchange(ref _ownerContext, null);
        if (context is not null)
        {
            context.Published -= HandleContextPublished;
        }
    }

    private void HandleContextPublished(object? sender, EventArgs args)
    {
        if (_ownerContext is not null)
        {
            RaiseResourcesChanged();
        }
    }
}
