using Avalonia.Controls;
using AvaloniaContentControl = Avalonia.Controls.ContentControl;
using AtomUITabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Shows the content of the scenario that is selected in a tab strip inside a content control.
/// The <c>Tag</c> of the selected tab strip item names the scenario. The content of a scenario is created on
/// first selection and cached until the controller is detached or disposed; the examples content is supplied by
/// the caller and is kept.
/// </summary>
public sealed class GalleryShowCaseScenarioController : IDisposable
{
    private const string DefaultExamplesScenario = "Examples";

    private readonly AtomUITabStrip _scenarioTabs;
    private readonly AvaloniaContentControl _contentHost;
    private readonly Control? _examplesContent;
    private readonly string _examplesScenario;
    private readonly Func<string, Control> _createScenarioContent;
    private readonly Action<Control, object?> _synchronizeScenarioContent;
    private readonly Dictionary<string, Control> _lazyScenarioContentCache = new(StringComparer.Ordinal);
    private object? _dataContext;
    private bool _disposed;

    /// <summary>
    /// Creates the controller for a tab strip and the content control that shows the scenario content, and starts
    /// following the selection of the tab strip.
    /// </summary>
    /// <param name="scenarioTabs">
    /// The tab strip whose selection selects the scenario. The <c>Tag</c> of the selected item names the scenario.
    /// </param>
    /// <param name="contentHost">
    /// The control that shows the content of the selected scenario.
    /// </param>
    /// <param name="createScenarioContent">
    /// Creates the content of a scenario that is not the examples scenario. It is called the first time that
    /// scenario is selected; the result is kept until the controller is detached or disposed.
    /// </param>
    /// <param name="examplesContent">
    /// The content of the examples scenario. It is not created by <paramref name="createScenarioContent" />
    /// and is kept when the controller is detached. When it is <see langword="null" />, that scenario is treated
    /// like any other scenario.
    /// </param>
    /// <param name="examplesScenario">
    /// The scenario name of the tab that shows <paramref name="examplesContent" />. It must not be blank. The
    /// default is <c>Examples</c>.
    /// </param>
    /// <param name="synchronizeScenarioContent">
    /// Passes the data context to scenario content each time it is shown and when the data context is updated.
    /// When it is <see langword="null" />, the data context is assigned to the <c>DataContext</c> of the content.
    /// </param>
    public GalleryShowCaseScenarioController(
        AtomUITabStrip scenarioTabs,
        AvaloniaContentControl contentHost,
        Func<string, Control> createScenarioContent,
        Control? examplesContent = null,
        string examplesScenario = DefaultExamplesScenario,
        Action<Control, object?>? synchronizeScenarioContent = null)
    {
        ArgumentNullException.ThrowIfNull(scenarioTabs);
        ArgumentNullException.ThrowIfNull(contentHost);
        ArgumentNullException.ThrowIfNull(createScenarioContent);
        ArgumentException.ThrowIfNullOrWhiteSpace(examplesScenario);

        _scenarioTabs               = scenarioTabs;
        _contentHost                = contentHost;
        _examplesContent            = examplesContent;
        _examplesScenario           = examplesScenario;
        _createScenarioContent      = createScenarioContent;
        _synchronizeScenarioContent = synchronizeScenarioContent ?? SynchronizeDataContext;

        _scenarioTabs.SelectionChanged += HandleScenarioSelectionChanged;
    }

    /// <summary>
    /// Stores the data context and shows the content of the selected scenario, creating it when needed.
    /// </summary>
    /// <param name="dataContext">
    /// The data context passed to the scenario content.
    /// </param>
    public void Attach(object? dataContext)
    {
        ThrowIfDisposed();
        _dataContext = dataContext;
        EnsureSelectedScenarioContent();
    }

    /// <summary>
    /// Drops the created scenario content: the cache is cleared and the content host shows nothing unless it shows
    /// the examples content. Dropped content is created again the next time its scenario is selected. Does nothing
    /// after the controller is disposed.
    /// </summary>
    public void Detach()
    {
        if (_disposed)
        {
            return;
        }

        ClearLazyScenarioContent();
    }

    /// <summary>
    /// Stores the data context, passes it to the examples content and to every created scenario content, and then
    /// shows the content of the selected scenario.
    /// </summary>
    /// <param name="dataContext">
    /// The data context passed to the scenario content.
    /// </param>
    public void UpdateDataContext(object? dataContext)
    {
        ThrowIfDisposed();

        _dataContext = dataContext;
        if (_examplesContent is not null)
        {
            _synchronizeScenarioContent(_examplesContent, _dataContext);
        }

        foreach (var content in _lazyScenarioContentCache.Values)
        {
            _synchronizeScenarioContent(content, _dataContext);
        }

        EnsureSelectedScenarioContent();
    }

    /// <summary>
    /// Stops following the tab strip selection and drops the created scenario content as <see cref="Detach" /> does.
    /// The controller cannot be used afterwards.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scenarioTabs.SelectionChanged -= HandleScenarioSelectionChanged;
        ClearLazyScenarioContent();
        _disposed = true;
    }

    private void HandleScenarioSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        EnsureSelectedScenarioContent();
    }

    private void EnsureSelectedScenarioContent()
    {
        if (_scenarioTabs.SelectedItem is not AtomUITabStripItem tabStripItem ||
            tabStripItem.Tag is not string scenario)
        {
            return;
        }

        var content = ResolveScenarioContent(scenario);
        if (!ReferenceEquals(_contentHost.Content, content))
        {
            _contentHost.Content = content;
        }
    }

    private void ClearLazyScenarioContent()
    {
        if (_examplesContent is null)
        {
            _contentHost.Content = null;
        }
        else if (_contentHost.Content is not null &&
                 !ReferenceEquals(_contentHost.Content, _examplesContent))
        {
            _contentHost.Content = null;
        }

        _lazyScenarioContentCache.Clear();
    }

    private Control ResolveScenarioContent(string scenario)
    {
        if (_examplesContent is not null &&
            scenario == _examplesScenario)
        {
            _synchronizeScenarioContent(_examplesContent, _dataContext);
            return _examplesContent;
        }

        if (!_lazyScenarioContentCache.TryGetValue(scenario, out var content))
        {
            content = _createScenarioContent(scenario);
            _lazyScenarioContentCache.Add(scenario, content);
        }

        _synchronizeScenarioContent(content, _dataContext);
        return content;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static void SynchronizeDataContext(Control content, object? dataContext)
    {
        content.DataContext = dataContext;
    }
}
