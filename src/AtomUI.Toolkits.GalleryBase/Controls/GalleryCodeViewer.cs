using System.Diagnostics;
using System.Collections.ObjectModel;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TextMateSharp.Model;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using AtomUIContextMenu = AtomUI.Desktop.Controls.ContextMenu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public sealed partial class GalleryCodeViewer : UserControl, IDisposable
{
    public static readonly StyledProperty<string?> CodeTextProperty =
        AvaloniaProperty.Register<GalleryCodeViewer, string?>(nameof(CodeText));

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<GalleryCodeViewer, string>(nameof(Language), "text");

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<GalleryCodeViewer, bool>(nameof(ShowLineNumbers), true);

    public static readonly StyledProperty<ThemeName> LightSyntaxThemeProperty =
        AvaloniaProperty.Register<GalleryCodeViewer, ThemeName>(nameof(LightSyntaxTheme), ThemeName.LightPlus);

    public static readonly StyledProperty<ThemeName> DarkSyntaxThemeProperty =
        AvaloniaProperty.Register<GalleryCodeViewer, ThemeName>(nameof(DarkSyntaxTheme), ThemeName.DarkPlus);

    private readonly TextEditor _editor;
    private readonly AtomUIContextMenu _editorContextMenu = new();
    private readonly AtomUIMenuItem _copyMenuItem = new()
    {
        Header = "Copy",
        IsEnabled = false
    };

    private GalleryTextMateInstallation? _textMateInstallation;
    private RegistryOptions? _registryOptions;
    private ScrollBar? _horizontalScrollBar;
    private ScrollBar? _verticalScrollBar;
    private Application? _subscribedApplication;
    private IThemeManager? _subscribedThemeManager;
    private IDisposable? _editorBoundsSubscription;
    private ThemeName? _currentSyntaxTheme;
    private bool _isScrollBarInsetUpdatePending;
    private bool _isUpdatingScrollBarInset;
    private bool _isDisposed;

    public GalleryCodeViewer() : this(null, null)
    {
    }

    internal GalleryCodeViewer(string? codeText, string? language)
    {
        InitializeComponent();
        _editor = this.FindControl<TextEditor>("PART_Editor")!;
        _editor.TextArea.SelectionChanged += HandleEditorSelectionChanged;
        _editorBoundsSubscription = _editor.GetObservable(BoundsProperty)
                                           .Subscribe(_ => RequestScrollBarInsetUpdate());
        ActualThemeVariantChanged += HandleActualThemeVariantChanged;
        ConfigureEditorContextMenu();
        InitializeSource(codeText, language);
    }

    public string? CodeText
    {
        get => GetValue(CodeTextProperty);
        set => SetValue(CodeTextProperty, value);
    }

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    public ThemeName LightSyntaxTheme
    {
        get => GetValue(LightSyntaxThemeProperty);
        set => SetValue(LightSyntaxThemeProperty, value);
    }

    public ThemeName DarkSyntaxTheme
    {
        get => GetValue(DarkSyntaxThemeProperty);
        set => SetValue(DarkSyntaxThemeProperty, value);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
        _registryOptions      = null;
        ReleaseThemeVariantSubscriptions();
        CancelScrollBarInsetUpdate();
        _editorBoundsSubscription?.Dispose();
        _editorBoundsSubscription = null;
        _editor.TextArea.SelectionChanged -= HandleEditorSelectionChanged;
        _editor.Document      = null;
        _copyMenuItem.Click -= HandleCopyMenuItemClick;
        _editorContextMenu.Opened -= HandleEditorContextMenuOpened;
        _editorContextMenu.Close();
        if (ReferenceEquals(_editor.ContextMenu, _editorContextMenu))
        {
            _editor.ContextMenu = null;
        }
        ActualThemeVariantChanged -= HandleActualThemeVariantChanged;
        _horizontalScrollBar  = null;
        _verticalScrollBar    = null;
        _currentSyntaxTheme   = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CodeTextProperty)
        {
            UpdateText();
        }
        else if (change.Property == LanguageProperty)
        {
            ApplyGrammar();
        }
        else if (change.Property == ShowLineNumbersProperty)
        {
            _editor.ShowLineNumbers = ShowLineNumbers;
            RequestScrollBarInsetUpdate();
        }
        else if (change.Property == LightSyntaxThemeProperty ||
                 change.Property == DarkSyntaxThemeProperty)
        {
            ApplySyntaxTheme();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeThemeVariantChanges();
        ApplySyntaxTheme();
        RequestScrollBarInsetUpdate();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelScrollBarInsetUpdate();
        ReleaseThemeVariantSubscriptions();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleEditorLayoutUpdated(object? sender, EventArgs e)
    {
        CancelScrollBarInsetUpdate();
        UpdateScrollBarInsets();
    }

    private void HandleEditorSelectionChanged(object? sender, EventArgs e)
    {
        UpdateCopyMenuItemState();
    }

    private void HandleEditorContextMenuOpened(object? sender, EventArgs e)
    {
        UpdateCopyMenuItemState();
    }

    private async void HandleCopyMenuItemClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        try
        {
            await CopySelectedCodeToClipboardAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error copying Gallery source code: {ex.Message}");
        }
    }

    private void HandleActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplySyntaxTheme();
    }

    private void HandleApplicationActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplySyntaxTheme();
    }

    private void HandleThemeManagerThemeChanged(object? sender, ThemeChangedEventArgs args)
    {
        ApplySyntaxTheme();
    }

    private void RequestScrollBarInsetUpdate()
    {
        if (_isDisposed || _isScrollBarInsetUpdatePending)
        {
            return;
        }

        _isScrollBarInsetUpdatePending = true;
        _editor.LayoutUpdated += HandleEditorLayoutUpdated;
    }

    private void CancelScrollBarInsetUpdate()
    {
        if (!_isScrollBarInsetUpdatePending)
        {
            return;
        }

        _isScrollBarInsetUpdatePending = false;
        _editor.LayoutUpdated -= HandleEditorLayoutUpdated;
    }

    private void UpdateScrollBarInsets()
    {
        // Mutating scrollbar/TextView margins triggers another layout pass, so this method must
        // only run from the one-shot request path above. Drag selection auto-scroll produces
        // repeated LayoutUpdated ticks, and a permanent handler can spin the UI thread.
        if (_isUpdatingScrollBarInset)
        {
            return;
        }

        _isUpdatingScrollBarInset = true;
        try
        {
            UpdateHorizontalScrollBarGutterInset();
            UpdateTextViewVerticalScrollBarInset();
        }
        finally
        {
            _isUpdatingScrollBarInset = false;
        }
    }

    private void UpdateHorizontalScrollBarGutterInset()
    {
        var horizontalScrollBar = GetHorizontalScrollBar();
        if (horizontalScrollBar is null)
        {
            return;
        }

        var gutterWidth = GetTextAreaLeftMarginsRightEdge();
        var currentMargin = horizontalScrollBar.Margin;
        if (Math.Abs(currentMargin.Left - gutterWidth) <= 0.5)
        {
            return;
        }

        horizontalScrollBar.Margin = new Thickness(
            gutterWidth,
            currentMargin.Top,
            currentMargin.Right,
            currentMargin.Bottom);
    }

    private void UpdateTextViewVerticalScrollBarInset()
    {
        var verticalScrollBar = GetVerticalScrollBar();
        var rightInset = verticalScrollBar is { IsVisible: true }
            ? verticalScrollBar.Bounds.Width + 1
            : 0;
        var textView = _editor.TextArea.TextView;
        var currentMargin = textView.Margin;
        if (Math.Abs(currentMargin.Right - rightInset) <= 0.5)
        {
            return;
        }

        textView.Margin = new Thickness(
            currentMargin.Left,
            currentMargin.Top,
            rightInset,
            currentMargin.Bottom);
    }

    private ScrollBar? GetHorizontalScrollBar()
    {
        if (_horizontalScrollBar is { } horizontalScrollBar &&
            TopLevel.GetTopLevel(horizontalScrollBar) == TopLevel.GetTopLevel(_editor))
        {
            return horizontalScrollBar;
        }

        _horizontalScrollBar = _editor.GetVisualDescendants()
                                      .OfType<ScrollBar>()
                                      .FirstOrDefault(scrollBar => scrollBar.Orientation == Orientation.Horizontal);
        return _horizontalScrollBar;
    }

    private ScrollBar? GetVerticalScrollBar()
    {
        if (_verticalScrollBar is { } verticalScrollBar &&
            TopLevel.GetTopLevel(verticalScrollBar) == TopLevel.GetTopLevel(_editor))
        {
            return verticalScrollBar;
        }

        _verticalScrollBar = _editor.GetVisualDescendants()
                                    .OfType<ScrollBar>()
                                    .FirstOrDefault(scrollBar => scrollBar.Orientation == Orientation.Vertical);
        return _verticalScrollBar;
    }

    private double GetTextAreaLeftMarginsRightEdge()
    {
        if (!ShowLineNumbers || !_editor.ShowLineNumbers)
        {
            return 0;
        }

        // Sum the left margins' own widths instead of projecting a point through TranslatePoint.
        // The left gutter is fixed and does not scroll, but TranslatePoint's result shifts with
        // the horizontal scroll offset and with mid-layout bounds, so the computed value jitters
        // during a drag-scroll and never converges against the 0.5 tolerance above.
        var width = 0d;
        foreach (var margin in _editor.TextArea.LeftMargins)
        {
            width += margin.Bounds.Width;
        }

        return width;
    }

    private void UpdateText()
    {
        _editor.Text = CodeText ?? string.Empty;
        RequestScrollBarInsetUpdate();
    }

    private void InitializeSource(string? codeText, string? language)
    {
        EnsureTextMateInstalled();

        if (codeText is not null)
        {
            SetCurrentValue(CodeTextProperty, codeText);
        }
        else
        {
            UpdateText();
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            SetCurrentValue(LanguageProperty, language);
        }
        else
        {
            ApplyGrammar();
        }
    }

    private void ConfigureEditorContextMenu()
    {
        _copyMenuItem.Click += HandleCopyMenuItemClick;
        _editorContextMenu.Opened += HandleEditorContextMenuOpened;
        _editorContextMenu.Items.Add(_copyMenuItem);
        _editor.ContextMenu = _editorContextMenu;
        UpdateCopyMenuItemState();
    }

    private void UpdateCopyMenuItemState()
    {
        _copyMenuItem.IsEnabled = !_editor.TextArea.Selection.IsEmpty;
    }

    private string GetSelectedCodeText()
    {
        return _editor.TextArea.Selection.IsEmpty
            ? string.Empty
            : _editor.TextArea.Selection.GetText();
    }

    private async Task CopySelectedCodeToClipboardAsync()
    {
        var selectedText = GetSelectedCodeText();
        if (string.IsNullOrEmpty(selectedText))
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(_editor)?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        var item = new DataTransferItem();
        item.SetText(selectedText);

        var dataTransfer = new DataTransfer();
        dataTransfer.Add(item);
        await clipboard.SetDataAsync(dataTransfer);
    }

    private void EnsureTextMateInstalled()
    {
        if (_textMateInstallation is not null)
        {
            return;
        }

        var syntaxTheme = ResolveSyntaxTheme();
        _registryOptions = new RegistryOptions(syntaxTheme);
        _textMateInstallation = new GalleryTextMateInstallation(_editor, _registryOptions);
        _currentSyntaxTheme = syntaxTheme;
    }

    private void ApplySyntaxTheme()
    {
        if (_isDisposed)
        {
            return;
        }

        EnsureTextMateInstalled();
        if (_registryOptions is null || _textMateInstallation is null)
        {
            return;
        }

        var syntaxTheme = ResolveSyntaxTheme();
        if (_currentSyntaxTheme == syntaxTheme)
        {
            return;
        }

        _textMateInstallation.SetTheme(_registryOptions.LoadTheme(syntaxTheme));
        _currentSyntaxTheme = syntaxTheme;
    }

    private ThemeName ResolveSyntaxTheme()
    {
        return ResolveIsDarkSyntaxTheme()
            ? DarkSyntaxTheme
            : LightSyntaxTheme;
    }

    private bool ResolveIsDarkSyntaxTheme()
    {
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            return true;
        }

        if (ActualThemeVariant == ThemeVariant.Light)
        {
            return false;
        }

        return Application.Current?.GetThemeManager()?.CurrentTheme?.Appearance == ThemeAppearance.Dark;
    }

    private void SubscribeThemeVariantChanges()
    {
        var application = Application.Current;
        var themeManager = application?.GetThemeManager();
        if (ReferenceEquals(_subscribedApplication, application))
        {
            if (ReferenceEquals(_subscribedThemeManager, themeManager))
            {
                return;
            }
        }

        ReleaseThemeVariantSubscriptions();
        _subscribedApplication = application;
        if (_subscribedApplication is not null)
        {
            _subscribedApplication.ActualThemeVariantChanged += HandleApplicationActualThemeVariantChanged;
        }

        _subscribedThemeManager = themeManager;
        if (_subscribedThemeManager is not null)
        {
            _subscribedThemeManager.ThemeChanged += HandleThemeManagerThemeChanged;
        }
    }

    private void ReleaseThemeVariantSubscriptions()
    {
        if (_subscribedApplication is not null)
        {
            _subscribedApplication.ActualThemeVariantChanged -= HandleApplicationActualThemeVariantChanged;
            _subscribedApplication = null;
        }

        if (_subscribedThemeManager is not null)
        {
            _subscribedThemeManager.ThemeChanged -= HandleThemeManagerThemeChanged;
        }
        _subscribedThemeManager = null;
    }

    private void ApplyGrammar()
    {
        EnsureTextMateInstalled();

        var scope = ResolveTextMateScope(Language);
        if (scope is not null)
        {
            _textMateInstallation?.SetGrammar(scope);
        }
    }

    private string? ResolveTextMateScope(string language)
    {
        var normalized = language.Trim().ToLowerInvariant();
        var extension = normalized switch
        {
            "axaml"  => ".xml",
            "xml"    => ".xml",
            "csharp" => ".cs",
            "cs"     => ".cs",
            _        => normalized.StartsWith(".") ? normalized : "." + normalized
        };

        return _registryOptions?.GetScopeByExtension(extension);
    }

    private sealed class GalleryTextMateInstallation : IDisposable
    {
        private static readonly TimeSpan s_grammarCompilationRetryInterval = TimeSpan.FromMilliseconds(16);

        private readonly object _lock = new();
        private readonly Registry _textMateRegistry;
        private readonly TextEditor _editor;
        private readonly TextMateColoringTransformer _transformer;
        private readonly bool _ownsTransformer;
        private Action<Exception>? _exceptionHandler;
        private GalleryTextEditorModel? _editorModel;
        private IGrammar? _grammar;
        private TMModel? _tmModel;
        private ReadOnlyDictionary<string, string>? _themeColorsDictionary;
        private int _pendingVisibleStartLine = -1;
        private int _pendingVisibleEndLine = -1;
        private bool _isVisibleLineTokenizationQueued;
        private CancellationTokenSource? _visibleLineTokenizationRetryCancellation;
        private bool _isDisposed;

        public GalleryTextMateInstallation(TextEditor editor,
                                           IRegistryOptions registryOptions,
                                           Action<Exception>? exceptionHandler = null)
        {
            RegistryOptions = registryOptions ?? throw new ArgumentNullException(nameof(registryOptions));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _exceptionHandler = exceptionHandler;
            _textMateRegistry = new Registry(registryOptions);
            _transformer = _editor.TextArea.TextView.LineTransformers
                                  .OfType<TextMateColoringTransformer>()
                                  .FirstOrDefault() ??
                           new TextMateColoringTransformer(_editor.TextArea.TextView, _exceptionHandler);

            if (!_editor.TextArea.TextView.LineTransformers.Contains(_transformer))
            {
                _editor.TextArea.TextView.LineTransformers.Add(_transformer);
                _ownsTransformer = true;
            }

            SetTheme(registryOptions.GetDefaultTheme());
            _editor.TextArea.TextView.VisualLinesChanged += HandleTextViewVisualLinesChanged;
            _editor.DocumentChanged += HandleEditorDocumentChanged;
            HandleEditorDocumentChanged(_editor, EventArgs.Empty);
        }

        public IRegistryOptions RegistryOptions { get; }

        public void SetGrammar(string scopeName)
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                ThrowIfDisposed();
                var grammar = _textMateRegistry.LoadGrammar(scopeName);
                SetGrammarInternal(grammar);
                ForceTokenizeDocument();
                RequestVisibleLineTokenization();
            }
            _editor.TextArea.TextView.Redraw();
        }

        public bool TryGetThemeColor(string colorKey, out string? colorString)
        {
            ThrowIfDisposed();
            return (_themeColorsDictionary ?? throw new ObjectDisposedException(nameof(GalleryTextMateInstallation)))
                .TryGetValue(colorKey, out colorString);
        }

        public void SetTheme(IRawTheme theme)
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                ThrowIfDisposed();
                _textMateRegistry.SetTheme(theme);
                var textMateTheme = _textMateRegistry.GetTheme();
                _transformer.SetTheme(textMateTheme);
                _themeColorsDictionary = textMateTheme.GetGuiColorDictionary();
            }
            _editor.TextArea.TextView.Redraw();
            RequestVisibleLineTokenization();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            GalleryTextEditorModel? editorModel;
            TMModel? tmModel;
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                editorModel = _editorModel;
                _editorModel = null;
                tmModel = _tmModel;
                _tmModel = null;
                _grammar = null;
                _themeColorsDictionary = null;
                _exceptionHandler = null;
            }

            CancelVisibleLineTokenizationRetry();
            _editor.DocumentChanged -= HandleEditorDocumentChanged;
            _editor.TextArea.TextView.VisualLinesChanged -= HandleTextViewVisualLinesChanged;
            editorModel?.Dispose();
            DisposeTMModel(tmModel);

            if (_ownsTransformer)
            {
                _editor.TextArea.TextView.LineTransformers.Remove(_transformer);
                _transformer.Dispose();
            }
            else
            {
                _transformer.SetModel(null, null);
            }
        }

        private void HandleEditorDocumentChanged(object? sender, EventArgs e)
        {
            if (_isDisposed || _editor.Document is null)
            {
                return;
            }

            lock (_lock)
            {
                if (_isDisposed || _editor.Document is null)
                {
                    return;
                }

                try
                {
                    _editorModel?.Dispose();
                    DisposeTMModel(_tmModel);
                    _editorModel = new GalleryTextEditorModel(
                        _editor.TextArea.TextView,
                        _editor.Document,
                        _exceptionHandler);
                    _tmModel = new TMModel(_editorModel);
                    // The transformer only reads this model's tokens. Setting its grammar or
                    // adding a listener starts TMModel's worker, which would race the UI-owned
                    // tokenizer and corrupt TextMateSharp's mutable decoding dictionaries.
                    _editorModel.SetGrammar(_grammar);
                    _transformer.SetModel(_editor.Document, _tmModel);
                    ForceTokenizeDocument();
                    RequestVisibleLineTokenization();
                }
                catch (Exception ex)
                {
                    _exceptionHandler?.Invoke(ex);
                }
            }
        }

        private void HandleTextViewVisualLinesChanged(object? sender, EventArgs e)
        {
            RequestVisibleLineTokenizationSafely();
        }

        private void RequestVisibleLineTokenizationSafely()
        {
            try
            {
                RequestVisibleLineTokenization();
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void SetGrammarInternal(IGrammar grammar)
        {
            _grammar = grammar;
            _editorModel?.SetGrammar(grammar);
        }

        private void ForceTokenizeDocument()
        {
            _editorModel?.ForceTokenizeAllLines();
        }

        private void RequestVisibleLineTokenization()
        {
            if (_isDisposed ||
                !TryGetVisibleLineRange(out var startLine, out var endLine) ||
                !TryGetTokenizationContext(out var editorModel, out var tmModel, out _) ||
                !NeedsLineTokenization(editorModel, tmModel, startLine, endLine))
            {
                return;
            }

            IncludePendingVisibleLineRange(startLine, endLine);
            QueueVisibleLineTokenization();
        }

        private void QueueVisibleLineTokenization()
        {
            if (_isDisposed || _isVisibleLineTokenizationQueued)
            {
                return;
            }

            _isVisibleLineTokenizationQueued = true;
            Dispatcher.UIThread.Post(ProcessVisibleLineTokenization, DispatcherPriority.Background);
        }

        private void ProcessVisibleLineTokenization()
        {
            if (_isDisposed)
            {
                return;
            }

            int startLine;
            int endLine;
            lock (_lock)
            {
                _isVisibleLineTokenizationQueued = false;
                startLine = _pendingVisibleStartLine;
                endLine = _pendingVisibleEndLine;
                _pendingVisibleStartLine = -1;
                _pendingVisibleEndLine = -1;
            }

            if (startLine < 0 ||
                endLine < startLine ||
                !TryGetTokenizationContext(out var editorModel, out var tmModel, out var grammar))
            {
                return;
            }

            NormalizeLineRange(editorModel, ref startLine, ref endLine);
            if (startLine < 0)
            {
                return;
            }

            if (!NeedsLineTokenization(editorModel, tmModel, startLine, endLine))
            {
                return;
            }

            if (grammar.IsCompiling)
            {
                IncludePendingVisibleLineRange(startLine, endLine);
                QueueVisibleLineTokenizationRetry();
                return;
            }

            editorModel.ForceTokenizeLineRange(startLine, endLine);
            if (NeedsLineTokenization(editorModel, tmModel, startLine, endLine))
            {
                IncludePendingVisibleLineRange(startLine, endLine);
                QueueVisibleLineTokenizationRetry();
                return;
            }
        }

        private void QueueVisibleLineTokenizationRetry()
        {
            CancellationTokenSource retryCancellation;
            lock (_lock)
            {
                if (_isDisposed || _visibleLineTokenizationRetryCancellation is not null)
                {
                    return;
                }

                retryCancellation = new CancellationTokenSource();
                _visibleLineTokenizationRetryCancellation = retryCancellation;
            }

            _ = RetryVisibleLineTokenizationAfterGrammarCompilationAsync(retryCancellation);
        }

        private async Task RetryVisibleLineTokenizationAfterGrammarCompilationAsync(CancellationTokenSource retryCancellation)
        {
            try
            {
                await Task.Delay(s_grammarCompilationRetryInterval, retryCancellation.Token)
                          .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (retryCancellation.IsCancellationRequested)
            {
                return;
            }

            Dispatcher.UIThread.Post(
                () => ProcessVisibleLineTokenizationRetry(retryCancellation),
                DispatcherPriority.Background);
        }

        private void ProcessVisibleLineTokenizationRetry(CancellationTokenSource retryCancellation)
        {
            lock (_lock)
            {
                if (!ReferenceEquals(_visibleLineTokenizationRetryCancellation, retryCancellation))
                {
                    retryCancellation.Dispose();
                    return;
                }

                _visibleLineTokenizationRetryCancellation = null;
            }

            retryCancellation.Dispose();
            if (_isDisposed)
            {
                return;
            }

            QueueVisibleLineTokenization();
        }

        private void CancelVisibleLineTokenizationRetry()
        {
            CancellationTokenSource? retryCancellation;
            lock (_lock)
            {
                retryCancellation = _visibleLineTokenizationRetryCancellation;
                _visibleLineTokenizationRetryCancellation = null;
            }

            if (retryCancellation is null)
            {
                return;
            }

            retryCancellation.Cancel();
            retryCancellation.Dispose();
        }

        private bool TryGetVisibleLineRange(out int startLine, out int endLine)
        {
            startLine = -1;
            endLine = -1;

            var textView = _editor.TextArea.TextView;
            if (!textView.VisualLinesValid || textView.VisualLines.Count == 0)
            {
                return false;
            }

            startLine = textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
            endLine = textView.VisualLines[^1].LastDocumentLine.LineNumber - 1;
            return endLine >= startLine;
        }

        private bool TryGetTokenizationContext(out GalleryTextEditorModel editorModel,
                                               out TMModel tmModel,
                                               out IGrammar grammar)
        {
            lock (_lock)
            {
                if (_isDisposed || _editorModel is null || _tmModel is null || _grammar is null)
                {
                    editorModel = null!;
                    tmModel = null!;
                    grammar = null!;
                    return false;
                }

                editorModel = _editorModel;
                tmModel = _tmModel;
                grammar = _grammar;
                return true;
            }
        }

        private void IncludePendingVisibleLineRange(int startLine, int endLine)
        {
            lock (_lock)
            {
                if (_pendingVisibleStartLine < 0)
                {
                    _pendingVisibleStartLine = startLine;
                    _pendingVisibleEndLine = endLine;
                }
                else
                {
                    _pendingVisibleStartLine = Math.Min(_pendingVisibleStartLine, startLine);
                    _pendingVisibleEndLine = Math.Max(_pendingVisibleEndLine, endLine);
                }
            }
        }

        private static bool NeedsLineTokenization(GalleryTextEditorModel editorModel,
                                                  TMModel tmModel,
                                                  int startLine,
                                                  int endLine)
        {
            NormalizeLineRange(editorModel, ref startLine, ref endLine);
            if (startLine < 0)
            {
                return false;
            }

            for (var line = startLine; line <= endLine; line++)
            {
                if (tmModel.IsLineInvalid(line))
                {
                    return true;
                }

                var tokens = tmModel.GetLineTokens(line);
                if (editorModel.GetLineLength(line) > 0 &&
                    (tokens is null || tokens.Count == 0))
                {
                    return true;
                }
            }

            return false;
        }

        private static void NormalizeLineRange(GalleryTextEditorModel editorModel,
                                               ref int startLine,
                                               ref int endLine)
        {
            var lineCount = editorModel.GetNumberOfLines();
            if (lineCount <= 0)
            {
                startLine = -1;
                endLine = -1;
                return;
            }

            startLine = Math.Clamp(startLine, 0, lineCount - 1);
            endLine = Math.Clamp(endLine, startLine, lineCount - 1);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GalleryTextMateInstallation));
            }
        }

        private void DisposeTMModel(TMModel? tmModel)
        {
            if (tmModel is null)
            {
                return;
            }

            tmModel.Dispose();
        }
    }

    private sealed class GalleryTextEditorModel : AbstractLineList, IDisposable
    {
        private readonly TextDocument _document;
        private readonly AvaloniaEdit.Rendering.TextView _textView;
        private readonly Action<Exception>? _exceptionHandler;
        private IGrammar? _grammar;
        private Tokenizer? _tokenizer;
        private DocumentSnapshot _documentSnapshot;
        private int _invalidStartLine = -1;
        private bool _isViewportTokenizationPending;
        private int _pendingViewportStartLine = -1;
        private int _pendingViewportEndLine = -1;
        private bool _isDisposed;

        public GalleryTextEditorModel(AvaloniaEdit.Rendering.TextView textView,
                                      TextDocument document,
                                      Action<Exception>? exceptionHandler)
        {
            _textView = textView;
            _document = document;
            _exceptionHandler = exceptionHandler;
            _documentSnapshot = new DocumentSnapshot(_document);
            for (var i = 0; i < _document.LineCount; i++)
            {
                AddLine(i);
            }

            _document.Changing += HandleDocumentChanging;
            _document.Changed += HandleDocumentChanged;
            _document.UpdateFinished += HandleDocumentUpdateFinished;
            _textView.ScrollOffsetChanged += HandleTextViewScrollOffsetChanged;
        }

        public override void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _grammar = null;
            _tokenizer = null;
            _document.Changing -= HandleDocumentChanging;
            _document.Changed -= HandleDocumentChanged;
            _document.UpdateFinished -= HandleDocumentUpdateFinished;
            _textView.ScrollOffsetChanged -= HandleTextViewScrollOffsetChanged;
        }

        public override void UpdateLine(int lineIndex)
        {
        }

        public override int GetNumberOfLines()
        {
            return _documentSnapshot.LineCount;
        }

        public override LineText GetLineTextIncludingTerminators(int lineIndex)
        {
            return _documentSnapshot.GetLineTextIncludingTerminatorAsMemory(lineIndex);
        }

        public override int GetLineLength(int lineIndex)
        {
            return _documentSnapshot.GetLineLength(lineIndex);
        }

        public void SetGrammar(IGrammar? grammar)
        {
            _textView.Dispatcher.VerifyAccess();
            _grammar = grammar;
            _tokenizer = grammar is null ? null : new Tokenizer(grammar);
            ForEach(line =>
            {
                line.ResetTokenizationState();
                line.IsInvalid = true;
            });
        }

        public void ForceTokenizeAllLines()
        {
            var lineCount = _documentSnapshot.LineCount;
            if (lineCount > 0)
            {
                ForceTokenizeLineRange(0, lineCount - 1);
            }
        }

        public void ForceTokenizeLineRange(int startLine, int endLine)
        {
            _textView.Dispatcher.VerifyAccess();
            var lineCount = _documentSnapshot.LineCount;
            if (_isDisposed || lineCount <= 0 || _tokenizer is null || _grammar?.IsCompiling != false)
            {
                return;
            }

            var normalizedStartLine = Math.Clamp(startLine, 0, lineCount - 1);
            var normalizedEndLine = Math.Clamp(endLine, normalizedStartLine, lineCount - 1);
            var firstChangedLine = -1;
            var lastChangedLine = -1;
            for (var lineIndex = 0; lineIndex <= normalizedEndLine; lineIndex++)
            {
                var line = Get(lineIndex);
                if (!line.IsInvalid && line.Tokens is not null)
                {
                    continue;
                }

                var text = GetLineTextIncludingTerminators(lineIndex);
                line.State ??= _tokenizer.GetInitialState();
                var result = _tokenizer.Tokenize(text, line.State, 0, 10000, TimeSpan.FromSeconds(3));
                if (result.ActualStopOffset < text.Length)
                {
                    result.Tokens.Add(new TMToken(result.ActualStopOffset, []));
                    result.EndState = line.State;
                }

                line.Tokens = result.Tokens;
                line.IsInvalid = false;
                if (lineIndex + 1 < lineCount)
                {
                    var nextLine = Get(lineIndex + 1);
                    if (!Equals(nextLine.State, result.EndState))
                    {
                        nextLine.State = result.EndState;
                        nextLine.IsInvalid = true;
                    }
                }

                if (firstChangedLine < 0)
                {
                    firstChangedLine = lineIndex;
                }

                lastChangedLine = lineIndex;
            }

            if (firstChangedLine >= 0)
            {
                var firstLine = _document.GetLineByNumber(firstChangedLine + 1);
                var lastLine = _document.GetLineByNumber(lastChangedLine + 1);
                _textView.Redraw(firstLine.Offset, lastLine.Offset + lastLine.TotalLength - firstLine.Offset);
            }
        }

        private void HandleTextViewScrollOffsetChanged(object? sender, EventArgs e)
        {
            try
            {
                RequestViewportTokenization();
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void RequestViewportTokenization()
        {
            if (_isDisposed || !_textView.VisualLinesValid || _textView.VisualLines.Count == 0)
            {
                return;
            }

            var startLine = GetFirstVisibleLineIndex();
            var endLine = GetLastVisibleLineIndex();
            if (_pendingViewportStartLine < 0)
            {
                _pendingViewportStartLine = startLine;
                _pendingViewportEndLine = endLine;
            }
            else
            {
                _pendingViewportStartLine = Math.Min(_pendingViewportStartLine, startLine);
                _pendingViewportEndLine = Math.Max(_pendingViewportEndLine, endLine);
            }

            if (_isViewportTokenizationPending)
            {
                return;
            }

            _isViewportTokenizationPending = true;
            Avalonia.Threading.Dispatcher.UIThread.Post(
                ProcessPendingViewportTokenization,
                DispatcherPriority.Background);
        }

        private void ProcessPendingViewportTokenization()
        {
            if (_isDisposed)
            {
                return;
            }

            var startLine = _pendingViewportStartLine;
            var endLine = _pendingViewportEndLine;
            _pendingViewportStartLine = -1;
            _pendingViewportEndLine = -1;
            _isViewportTokenizationPending = false;
            if (startLine >= 0 && endLine >= startLine)
            {
                ForceTokenizeLineRange(startLine, endLine);
            }
        }

        private void HandleDocumentChanging(object? sender, DocumentChangeEventArgs e)
        {
            try
            {
                if (e.RemovalLength <= 0)
                {
                    return;
                }

                var startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                var endLine = _document.GetLineByOffset(e.Offset + e.RemovalLength).LineNumber - 1;
                for (var line = endLine; line > startLine; line--)
                {
                    RemoveLine(line);
                }
                _documentSnapshot.RemoveLines(startLine, endLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void HandleDocumentChanged(object? sender, DocumentChangeEventArgs e)
        {
            try
            {
                var startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                var endLine = startLine;
                if (e.InsertionLength > 0)
                {
                    endLine = _document.GetLineByOffset(e.Offset + e.InsertionLength).LineNumber - 1;
                    for (var line = startLine; line < endLine; line++)
                    {
                        AddLine(line);
                    }
                }

                _documentSnapshot.Update(e);
                SetInvalidRange(startLine == 0 ? startLine : startLine - 1);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void SetInvalidRange(int startLine)
        {
            if (!_document.IsInUpdate)
            {
                InvalidateDocumentLines(startLine);
            }
            else
            {
                _invalidStartLine = _invalidStartLine < 0 ? startLine : Math.Min(_invalidStartLine, startLine);
            }
        }

        private void HandleDocumentUpdateFinished(object? sender, EventArgs e)
        {
            if (_invalidStartLine < 0)
            {
                return;
            }

            var startLine = Math.Clamp(_invalidStartLine, 0, _documentSnapshot.LineCount - 1);
            _invalidStartLine = -1;
            InvalidateDocumentLines(startLine);
        }

        private void InvalidateDocumentLines(int startLine)
        {
            for (var lineIndex = startLine; lineIndex < _documentSnapshot.LineCount; lineIndex++)
            {
                Get(lineIndex).IsInvalid = true;
            }

            RequestViewportTokenization();
        }

        private int GetFirstVisibleLineIndex()
        {
            return _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
        }

        private int GetLastVisibleLineIndex()
        {
            return _textView.VisualLines[^1].LastDocumentLine.LineNumber - 1;
        }
    }
}
