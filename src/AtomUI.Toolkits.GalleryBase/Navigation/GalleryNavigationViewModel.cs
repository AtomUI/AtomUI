using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Threading;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Navigation;

/// <summary>
/// Navigates a Gallery workspace between its pages: decides which keys of the navigation tree can be
/// navigated to, creates the page view model for a route and shows it as the only page of the host
/// screen. Also provides a diagnostic run that opens every navigable page in turn.
/// </summary>
public class GalleryNavigationViewModel : ReactiveObject, IActivatableViewModel, IDisposable
{
    private readonly GalleryBaseConfiguration _configuration;
    private readonly HashSet<EntityKey>       _routeNodeKeys;
    private readonly Lazy<ReactiveCommand<EntityKey, Unit>> _navigateToCommand;
    private readonly Lazy<ReactiveCommand<TimeSpan, Unit>> _testNavigatePagesCommand;
    private readonly Lazy<ReactiveCommand<Unit, Unit>> _stopTestNavigatePagesCommand;
    private DispatcherTimer? _dispatcherTimer;
    private readonly IDisposable              _activationSubscription;
    private bool _isDisposed;
    private bool _isDiagnosticNavigationRunning;
    private bool _shouldResetDeferredLoadingDisabledOnDiagnosticStop;
    private int _diagnosticRouteIndex;

    private EntityKey? _currentRoute;
    private INavMenuNode? _selectedItem;

    /// <summary>
    /// The navigation menu node that is currently selected, or <see langword="null" />. This class only
    /// stores the value and raises change notifications; navigating with <see cref="NavigateToCommand" />
    /// does not update it.
    /// </summary>
    public INavMenuNode? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    /// <summary>
    /// The screen that shows the pages. Its router is reset to the new page on every navigation, and it is
    /// passed as the host screen to every page view model that is created.
    /// </summary>
    public IScreen HostScreen { get; }

    /// <summary>
    /// Each activation navigates the host screen to the default route of the configuration, unless that
    /// page is already shown. The activation subscription ends when the view model is disposed.
    /// </summary>
    public ViewModelActivator Activator { get; } = new();

    /// <summary>
    /// Navigates the host screen to the page of a route. The command parameter is the key of a page node of
    /// the navigation tree. The command does nothing when <see cref="CanNavigateTo" /> returns
    /// <see langword="false" /> for the key, when that page is already shown, or after the view model has
    /// been disposed.
    /// </summary>
    public ReactiveCommand<EntityKey, Unit> NavigateToCommand => _navigateToCommand.Value;

    /// <summary>
    /// Starts a diagnostic run that opens every navigable page in turn, in the order of the route registry.
    /// The command parameter is the time between two pages. Calling it again during a run restarts the timer
    /// with the new interval. Deferred loading is disabled while the run is active:
    /// <see cref="GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled" /> is set to <see langword="true" />.
    /// </summary>
    public ReactiveCommand<TimeSpan, Unit> TestNavigatePagesCommand => _testNavigatePagesCommand.Value;

    /// <summary>
    /// Ends the diagnostic run started by <see cref="TestNavigatePagesCommand" /> and restores deferred
    /// loading when it was enabled before the run started.
    /// </summary>
    public ReactiveCommand<Unit, Unit> StopTestNavigatePagesCommand => _stopTestNavigatePagesCommand.Value;

    /// <summary>
    /// Creates the navigation of a workspace. Each activation of the view model navigates to the default
    /// route of the configuration.
    /// </summary>
    /// <param name="hostScreen">
    /// The screen whose router shows the pages.
    /// </param>
    /// <param name="configuration">
    /// The Gallery configuration that supplies the navigation tree, the routes and the default route.
    /// </param>
    public GalleryNavigationViewModel(IScreen hostScreen, GalleryBaseConfiguration configuration)
    {
        HostScreen      = hostScreen;
        _configuration  = configuration;
        _routeNodeKeys  = Walk(configuration.NavigationNodes)
            .Where(node => node.IsRoute)
            .Select(node => node.Key)
            .ToHashSet();

        _navigateToCommand = new(() => ReactiveCommand.Create<EntityKey>(DoNavigateTo));
        _testNavigatePagesCommand = new(() => ReactiveCommand.Create<TimeSpan>(DoTestNavigatePages));
        _stopTestNavigatePagesCommand = new(() => ReactiveCommand.Create(DoStopTestNavigatePages));

        _activationSubscription = Activator.Activated.Subscribe(_ => DoNavigateTo(_configuration.DefaultRoute));
    }

    /// <summary>
    /// Determines whether a key can be navigated to.
    /// </summary>
    /// <param name="routeKey">
    /// The key to test.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a page of the navigation tree has this key and the route registry
    /// contains a route for it; otherwise <see langword="false" />.
    /// </returns>
    public bool CanNavigateTo(EntityKey routeKey)
    {
        return _routeNodeKeys.Contains(routeKey) && _configuration.Routes.ContainsRoute(routeKey);
    }

    /// <summary>
    /// Ends the diagnostic run, detaches its timer and disposes the activation subscription. After disposal
    /// the navigation and diagnostic commands do nothing, and calling this method again has no effect.
    /// Overrides must call the base implementation to release the timer and the subscription.
    /// </summary>
    public virtual void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        StopDiagnosticNavigation();
        if (_dispatcherTimer is { } timer)
        {
            timer.Tick -= HandleDiagnosticTimerTick;
            _dispatcherTimer = null;
        }
        _activationSubscription.Dispose();
    }

    private void DoNavigateTo(EntityKey routeKey)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!CanNavigateTo(routeKey))
        {
            return;
        }

        if (_currentRoute is not null && _currentRoute.Value == routeKey)
        {
            return;
        }

        _currentRoute = routeKey;
        var viewModel = _configuration.Routes.CreateViewModel(routeKey, HostScreen);
        HostScreen.Router.NavigateAndReset.Execute(viewModel)
                  .Subscribe();
    }

    private void HandleDiagnosticTimerTick(object? sender, EventArgs e)
    {
        var routes = _configuration.Routes.Routes
            .Select(route => route.RouteKey)
            .Where(CanNavigateTo)
            .ToArray();
        if (routes.Length == 0)
        {
            return;
        }

        var routeKey = routes[_diagnosticRouteIndex++ % routes.Length];
        DoNavigateTo(routeKey);
    }

    private void DoTestNavigatePages(TimeSpan interval)
    {
        if (_isDisposed)
        {
            return;
        }

        if (_dispatcherTimer is null)
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += HandleDiagnosticTimerTick;
        }
        _dispatcherTimer.Stop();
        if (!_isDiagnosticNavigationRunning)
        {
            _shouldResetDeferredLoadingDisabledOnDiagnosticStop =
                !GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        }

        _isDiagnosticNavigationRunning = true;
        GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
        if (_isDisposed)
        {
            return;
        }

        _dispatcherTimer.Interval = interval;
        _dispatcherTimer.Start();
    }

    private void DoStopTestNavigatePages()
    {
        if (_isDisposed)
        {
            return;
        }

        StopDiagnosticNavigation();
    }

    private void StopDiagnosticNavigation()
    {
        _dispatcherTimer?.Stop();
        if (_isDiagnosticNavigationRunning && _shouldResetDeferredLoadingDisabledOnDiagnosticStop)
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = false;
        }

        _isDiagnosticNavigationRunning = false;
        _shouldResetDeferredLoadingDisabledOnDiagnosticStop = false;
    }

    private static IEnumerable<GalleryNavigationNode> Walk(IEnumerable<GalleryNavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }
}
