using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Binding;

namespace AtomUI.Toolkits.GalleryBase.Routing;

// A Gallery route is an explicit view factory. The upstream generated dispatcher
// runs before DefaultViewLocator.Map and can choose a secondary showcase sharing
// the same ViewModel. Keep route factories authoritative for the Gallery host.
internal sealed class GalleryRouteViewLocator : IViewLocator
{
    private readonly Dictionary<Type, GalleryRouteDescriptor> _routes = new();

    internal GalleryRouteViewLocator(GalleryRouteRegistry registry)
    {
        foreach (var route in registry.Routes)
        {
            _routes[route.ViewModelType] = route;
        }
    }

    public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class
    {
        return ResolveView((object?)viewModel, contract);
    }

    public IViewFor? ResolveView(object? viewModel, string? contract)
    {
        if (viewModel is not null && string.IsNullOrEmpty(contract) &&
            _routes.TryGetValue(viewModel.GetType(), out var route))
        {
            return route.CreateView();
        }
        // Named contracts and non-route consumers retain the configured global
        // locator. This class is assigned only to the host, never globally.
        return ViewLocator.GetCurrent().ResolveView(viewModel, contract);
    }

    [RequiresDynamicCode("IViewLocator reserves this entry point for dynamic resolution; Gallery routes use explicit factories.")]
    public IViewFor? ResolveViewUnsafe(object? viewModel, string? contract)
    {
        return ResolveView(viewModel, contract);
    }
}
