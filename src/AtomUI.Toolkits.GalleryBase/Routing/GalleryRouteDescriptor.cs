using AtomUI.Controls;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Routing;

/// <summary>
/// Describes one Gallery route: the key it is registered under, its view model and view types, and the factories
/// that create its view model and map it to its view.
/// </summary>
/// <remarks>
/// Create a descriptor with <c>Create</c> and register it in <c>GalleryRouteRegistry</c>, which calls
/// <see cref="RegisterView" /> and <see cref="CreateViewModel" /> for the Gallery.
/// </remarks>
public sealed class GalleryRouteDescriptor
{
    private readonly Func<IScreen, IRoutableViewModel> _viewModelFactory;
    private readonly Action<DefaultViewLocator>        _registerView;

    /// <summary>
    /// The key that identifies the route in the route registry. It must be unique among the registered routes.
    /// </summary>
    public EntityKey RouteKey { get; }

    /// <summary>
    /// The type of the view model that <see cref="CreateViewModel" /> returns.
    /// </summary>
    public Type ViewModelType { get; }

    /// <summary>
    /// The type of the view that <see cref="RegisterView" /> maps the view model to.
    /// </summary>
    public Type ViewType { get; }

    private GalleryRouteDescriptor(EntityKey routeKey,
                                   Type viewModelType,
                                   Type viewType,
                                   Func<IScreen, IRoutableViewModel> viewModelFactory,
                                   Action<DefaultViewLocator> registerView)
    {
        RouteKey          = routeKey;
        ViewModelType     = viewModelType;
        ViewType          = viewType;
        _viewModelFactory = viewModelFactory;
        _registerView     = registerView;
    }

    internal static GalleryRouteDescriptor Create<TViewModel, TView>(EntityKey routeKey,
                                                                     Func<IScreen, TViewModel> viewModelFactory,
                                                                     Func<TView> viewFactory)
        where TViewModel : class, IRoutableViewModel
        where TView : class, IViewFor<TViewModel>
    {
        return new GalleryRouteDescriptor(
            routeKey,
            typeof(TViewModel),
            typeof(TView),
            screen => viewModelFactory(screen),
            locator => locator.Map<TViewModel, TView>(viewFactory));
    }

    /// <summary>
    /// Runs the view model factory of the route.
    /// </summary>
    /// <param name="hostScreen">
    /// The screen that hosts the view model. It is passed to the factory.
    /// </param>
    /// <returns>
    /// The view model that the factory returned.
    /// </returns>
    public IRoutableViewModel CreateViewModel(IScreen hostScreen)
    {
        return _viewModelFactory(hostScreen);
    }

    /// <summary>
    /// Runs the registration action of the route, which maps the view model type to the view type on the locator.
    /// </summary>
    /// <param name="locator">
    /// The view locator that resolves the view model of the route to its view from then on.
    /// </param>
    public void RegisterView(DefaultViewLocator locator)
    {
        _registerView(locator);
    }
}
