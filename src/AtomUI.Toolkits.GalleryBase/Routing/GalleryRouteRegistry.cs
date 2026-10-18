using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Routing;

public sealed class GalleryRouteRegistry
{
    private readonly Dictionary<EntityKey, GalleryRouteDescriptor> _routes = new();
    private readonly List<GalleryRouteDescriptor>                  _orderedRoutes = new();
    private readonly IReadOnlyList<GalleryRouteDescriptor>         _orderedRoutesView;
    private readonly bool _isReadOnly;

    public GalleryRouteRegistry()
    {
        _orderedRoutesView = _orderedRoutes.AsReadOnly();
    }

    private GalleryRouteRegistry(IEnumerable<GalleryRouteDescriptor> routes, bool isReadOnly)
        : this()
    {
        _isReadOnly = isReadOnly;
        foreach (var route in routes)
        {
            AddDescriptor(route);
        }
    }

    public IReadOnlyList<GalleryRouteDescriptor> Routes => _orderedRoutesView;

    public void Map(GalleryRouteDescriptor route)
    {
        ArgumentNullException.ThrowIfNull(route);
        EnsureMutable();
        AddDescriptor(route);
    }

    public void Map<TViewModel, TView>(EntityKey routeKey, Func<IScreen, TViewModel> viewModelFactory)
        where TViewModel : class, IRoutableViewModel
        where TView : class, IViewFor<TViewModel>, new()
    {
        Map<TViewModel, TView>(routeKey, viewModelFactory, static () => new TView());
    }

    public void Map<TViewModel, TView>(EntityKey routeKey,
                                       Func<IScreen, TViewModel> viewModelFactory,
                                       Func<TView> viewFactory)
        where TViewModel : class, IRoutableViewModel
        where TView : class, IViewFor<TViewModel>
    {
        EnsureMutable();
        if (string.IsNullOrWhiteSpace(routeKey.Value))
        {
            throw new GalleryConfigurationException("Gallery route key must not be empty.");
        }

        var descriptor = GalleryRouteDescriptor.Create(routeKey, viewModelFactory, viewFactory);
        AddDescriptor(descriptor);
    }

    public bool ContainsRoute(EntityKey routeKey)
    {
        return _routes.ContainsKey(routeKey);
    }

    public GalleryRouteDescriptor GetRequiredRoute(EntityKey routeKey)
    {
        if (_routes.TryGetValue(routeKey, out var descriptor))
        {
            return descriptor;
        }

        throw new GalleryConfigurationException($"Gallery route key '{routeKey}' is not registered.");
    }

    public IRoutableViewModel CreateViewModel(EntityKey routeKey, IScreen hostScreen)
    {
        return GetRequiredRoute(routeKey).CreateViewModel(hostScreen);
    }

    public void RegisterViews(DefaultViewLocator locator)
    {
        foreach (var route in _orderedRoutes)
        {
            route.RegisterView(locator);
        }
    }

    internal GalleryRouteRegistry ToReadOnlySnapshot()
    {
        return new GalleryRouteRegistry(_orderedRoutes, isReadOnly: true);
    }

    private void EnsureMutable()
    {
        if (_isReadOnly)
        {
            throw new GalleryConfigurationException(
                "Gallery route registry is read-only after configuration is built.");
        }
    }

    private void AddDescriptor(GalleryRouteDescriptor descriptor)
    {
        if (_routes.ContainsKey(descriptor.RouteKey))
        {
            throw new GalleryConfigurationException(
                $"Gallery route key '{descriptor.RouteKey}' is already registered.");
        }

        _routes.Add(descriptor.RouteKey, descriptor);
        _orderedRoutes.Add(descriptor);
    }
}
