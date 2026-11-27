using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Routing;

/// <summary>
/// The table of Gallery routes: one descriptor per route key, kept in registration order. Host modules fill it
/// with <c>Map</c> while the options are built. The registry that <see cref="GalleryBaseConfiguration" /> exposes
/// is a read-only snapshot in which every <c>Map</c> call throws <see cref="GalleryConfigurationException" />.
/// </summary>
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

    /// <summary>
    /// Gets the registered routes in registration order, as a read-only view.
    /// </summary>
    public IReadOnlyList<GalleryRouteDescriptor> Routes => _orderedRoutesView;

    /// <summary>
    /// Adds a prepared route descriptor to the registry.
    /// </summary>
    /// <param name="route">
    /// The descriptor to register.
    /// </param>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the registry is read-only, or when a route with the same key is already registered.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="route" /> is <see langword="null" />.
    /// </exception>
    public void Map(GalleryRouteDescriptor route)
    {
        ArgumentNullException.ThrowIfNull(route);
        EnsureMutable();
        AddDescriptor(route);
    }

    /// <summary>
    /// Maps a route key to a view model factory and to a view of type <typeparamref name="TView" /> that is
    /// created with its parameterless constructor.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the view model created for the route.
    /// </typeparam>
    /// <typeparam name="TView">
    /// The type of the view created for the view model.
    /// </typeparam>
    /// <param name="routeKey">
    /// The key of the route; it must not be blank and must not be registered yet.
    /// </param>
    /// <param name="viewModelFactory">
    /// Creates the view model, receiving the screen that hosts the route.
    /// </param>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the registry is read-only, when <paramref name="routeKey" /> is blank, or when a route with the
    /// same key is already registered.
    /// </exception>
    public void Map<TViewModel, TView>(EntityKey routeKey, Func<IScreen, TViewModel> viewModelFactory)
        where TViewModel : class, IRoutableViewModel
        where TView : class, IViewFor<TViewModel>, new()
    {
        Map<TViewModel, TView>(routeKey, viewModelFactory, static () => new TView());
    }

    /// <summary>
    /// Maps a route key to a view model factory and to a view factory.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the view model created for the route.
    /// </typeparam>
    /// <typeparam name="TView">
    /// The type of the view created for the view model.
    /// </typeparam>
    /// <param name="routeKey">
    /// The key of the route; it must not be blank and must not be registered yet.
    /// </param>
    /// <param name="viewModelFactory">
    /// Creates the view model, receiving the screen that hosts the route.
    /// </param>
    /// <param name="viewFactory">
    /// Creates the view when the view locator resolves the view model of the route.
    /// </param>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the registry is read-only, when <paramref name="routeKey" /> is blank, or when a route with the
    /// same key is already registered.
    /// </exception>
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

    /// <summary>
    /// Determines whether a route is registered under the given key.
    /// </summary>
    /// <param name="routeKey">
    /// The key of the route to look up.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a route is registered under that key; otherwise <see langword="false" />.
    /// </returns>
    public bool ContainsRoute(EntityKey routeKey)
    {
        return _routes.ContainsKey(routeKey);
    }

    /// <summary>
    /// Returns the route registered under the given key.
    /// </summary>
    /// <param name="routeKey">
    /// The key of the route to look up.
    /// </param>
    /// <returns>
    /// The descriptor registered under that key.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when no route is registered under <paramref name="routeKey" />.
    /// </exception>
    public GalleryRouteDescriptor GetRequiredRoute(EntityKey routeKey)
    {
        if (_routes.TryGetValue(routeKey, out var descriptor))
        {
            return descriptor;
        }

        throw new GalleryConfigurationException($"Gallery route key '{routeKey}' is not registered.");
    }

    /// <summary>
    /// Runs the view model factory of the route registered under the given key.
    /// </summary>
    /// <param name="routeKey">
    /// The key of the route to create the view model for.
    /// </param>
    /// <param name="hostScreen">
    /// The screen that hosts the route; it is passed to the factory.
    /// </param>
    /// <returns>
    /// The view model that the factory returned.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when no route is registered under <paramref name="routeKey" />.
    /// </exception>
    public IRoutableViewModel CreateViewModel(EntityKey routeKey, IScreen hostScreen)
    {
        return GetRequiredRoute(routeKey).CreateViewModel(hostScreen);
    }

    /// <summary>
    /// Registers the view of every route with a view locator, in registration order.
    /// </summary>
    /// <param name="locator">
    /// The view locator that resolves the view models of the routes to their views.
    /// </param>
    public void RegisterViews(DefaultViewLocator locator)
    {
        foreach (var route in _orderedRoutes)
        {
            route.RegisterView(locator);
        }
    }

    /// <summary>
    /// Creates a read-only copy of this registry that holds the same routes in the same order. Routes added to
    /// this registry afterwards do not reach the copy, and every <c>Map</c> call on the copy throws
    /// <see cref="GalleryConfigurationException" />.
    /// </summary>
    /// <returns>
    /// The read-only copy.
    /// </returns>
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
