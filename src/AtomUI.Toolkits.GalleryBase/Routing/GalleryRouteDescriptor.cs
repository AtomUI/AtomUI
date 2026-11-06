using AtomUI.Controls;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Routing;

public sealed class GalleryRouteDescriptor
{
    private readonly Func<IScreen, IRoutableViewModel> _viewModelFactory;
    private readonly Action<DefaultViewLocator>        _registerView;
    private readonly Func<IViewFor>                    _createView;

    public EntityKey RouteKey { get; }

    public Type ViewModelType { get; }

    public Type ViewType { get; }

    private GalleryRouteDescriptor(EntityKey routeKey,
                                   Type viewModelType,
                                   Type viewType,
                                   Func<IScreen, IRoutableViewModel> viewModelFactory,
                                   Action<DefaultViewLocator> registerView,
                                   Func<IViewFor> createView)
    {
        RouteKey          = routeKey;
        ViewModelType     = viewModelType;
        ViewType          = viewType;
        _viewModelFactory = viewModelFactory;
        _registerView     = registerView;
        _createView       = createView;
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
            locator => locator.Map<TViewModel>(() => viewFactory()),
            () => viewFactory());
    }

    public IRoutableViewModel CreateViewModel(IScreen hostScreen)
    {
        return _viewModelFactory(hostScreen);
    }

    public void RegisterView(DefaultViewLocator locator)
    {
        _registerView(locator);
    }

    internal IViewFor CreateView()
    {
        return _createView();
    }
}
