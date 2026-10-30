using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Avalonia.Automation;
using AtomUI.Desktop.Controls.Localization;
using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia.Input.Raw;
using Avalonia.Media;
using AvaloniaGrid = Avalonia.Controls.Grid;
using DateViewerControl = AtomUI.Desktop.Controls.DateViewer;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>One realized grid consuming a shared interaction session.</summary>
internal sealed class DatePanel : TemplatedControl
{
    public static readonly StyledProperty<DatePanelSession?> SessionProperty =
        AvaloniaProperty.Register<DatePanel, DatePanelSession?>(nameof(Session));
    public static readonly StyledProperty<int> PanelIndexProperty =
        AvaloniaProperty.Register<DatePanel, int>(nameof(PanelIndex));
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty = DateViewerControl.CellTemplateProperty.AddOwner<DatePanel>();
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty = DateViewerControl.FullCellTemplateProperty.AddOwner<DatePanel>();
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty = DateViewerControl.PresentationProperty.AddOwner<DatePanel>();
    public static readonly StyledProperty<IDataTemplate?> DefaultFullCellTemplateProperty = DateViewerControl.DefaultFullCellTemplateProperty.AddOwner<DatePanel>();
    public static readonly StyledProperty<Avalonia.Styling.ControlTheme?> CellThemeProperty = DateViewerControl.CellThemeProperty.AddOwner<DatePanel>();
    public static readonly StyledProperty<TextAlignment> WeekHeaderTextAlignmentProperty =
        AvaloniaProperty.Register<DatePanel, TextAlignment>(nameof(WeekHeaderTextAlignment), TextAlignment.Center);
    public static readonly StyledProperty<double> WeekHeaderMinHeightProperty =
        AvaloniaProperty.Register<DatePanel, double>(nameof(WeekHeaderMinHeight));
    public static readonly StyledProperty<Thickness> WeekHeaderPaddingProperty =
        AvaloniaProperty.Register<DatePanel, Thickness>(nameof(WeekHeaderPadding));

    public DatePanelSession? Session
    {
        get => GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    public int PanelIndex
    {
        get => GetValue(PanelIndexProperty);
        set => SetValue(PanelIndexProperty, value);
    }
    public IDataTemplate? CellTemplate { get => GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    public IDataTemplate? FullCellTemplate { get => GetValue(FullCellTemplateProperty); set => SetValue(FullCellTemplateProperty, value); }
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    public IDataTemplate? DefaultFullCellTemplate { get => GetValue(DefaultFullCellTemplateProperty); set => SetValue(DefaultFullCellTemplateProperty, value); }
    public Avalonia.Styling.ControlTheme? CellTheme { get => GetValue(CellThemeProperty); set => SetValue(CellThemeProperty, value); }
    public TextAlignment WeekHeaderTextAlignment { get => GetValue(WeekHeaderTextAlignmentProperty); set => SetValue(WeekHeaderTextAlignmentProperty, value); }
    public double WeekHeaderMinHeight { get => GetValue(WeekHeaderMinHeightProperty); set => SetValue(WeekHeaderMinHeightProperty, value); }
    public Thickness WeekHeaderPadding { get => GetValue(WeekHeaderPaddingProperty); set => SetValue(WeekHeaderPaddingProperty, value); }

    private AvaloniaGrid? _headers;
    private AvaloniaGrid? _cells;
    private DatePanelSession? _subscribedSession;
    private readonly List<DateViewerCell> _pool = new();
    private IReadOnlyList<DateCellLayout> _layouts = Array.Empty<DateCellLayout>();
    private IDisposable? _pointerSubscription;
    internal event EventHandler? GeometryChanged;

    public DatePanel() => Focusable = true;

    protected override Avalonia.Automation.Peers.AutomationPeer OnCreateAutomationPeer() => new DatePanelAutomationPeer(this);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseCells();
        base.OnApplyTemplate(e);
        _headers = e.NameScope.Find<AvaloniaGrid>("PART_WeekHeader");
        _cells = e.NameScope.Find<AvaloniaGrid>("PART_CellHost");
        SubscribeSession();
        Realize();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _pointerSubscription?.Dispose();
        _pointerSubscription = (AvaloniaLocator.Current.GetService(typeof(IInputManager)) as IInputManager)?.Process.Subscribe(HandleRawPointer);
        SubscribeSession();
        Realize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _pointerSubscription?.Dispose();
        _pointerSubscription = null;
        Session?.Apply(new DatePanelAction.Hover(null));
        UnsubscribeSession();
        ReleaseCells();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SessionProperty)
        {
            UnsubscribeSession();
            ReleaseCells();
            if (this.IsAttachedToVisualTree())
            {
                SubscribeSession();
            }

            Realize();
        }
        else if (change.Property == PanelIndexProperty || change.Property == CellTemplateProperty || change.Property == FullCellTemplateProperty || change.Property == DefaultFullCellTemplateProperty || change.Property == CellThemeProperty || change.Property == PresentationProperty || change.Property == WeekHeaderTextAlignmentProperty || change.Property == WeekHeaderMinHeightProperty || change.Property == WeekHeaderPaddingProperty)
        {
            Realize();
        }
    }

    private void SubscribeSession()
    {
        if (ReferenceEquals(_subscribedSession, Session))
        {
            return;
        }

        UnsubscribeSession();
        _subscribedSession = Session;
        if (_subscribedSession is not null)
        {
            _subscribedSession.ConnectPanel(this);
            _subscribedSession.Changed += OnSessionChanged;
            _subscribedSession.FocusRequested += OnFocusRequested;
        }
    }

    private void UnsubscribeSession()
    {
        if (_subscribedSession is null)
        {
            return;
        }

        _subscribedSession.Changed -= OnSessionChanged;
        _subscribedSession.FocusRequested -= OnFocusRequested;
        _subscribedSession.DisconnectPanel(this);
        _subscribedSession = null;
    }

    private void OnSessionChanged(object? sender, EventArgs e) => Realize();

    private void OnFocusRequested(object? sender, EventArgs e)
    {
        var target = _pool.FirstOrDefault(cell => cell.Model is { IsFocusable: true, IsFocused: true });
        target?.Focus(NavigationMethod.Directional);
    }

    private void Realize()
    {
        if (_cells is null)
        {
            return;
        }

        if (Session is not { } session || PanelIndex < 0 || PanelIndex >= session.Models.Count)
        {
            ReleaseCells();
            return;
        }
        var model = session.Models[PanelIndex];
        ConfigureGrid(_cells, model.Rows, model.Columns);
        if (_headers is not null)
        {
            _headers.IsVisible = model.ColumnHeaders.Count > 0;
            ConfigureGrid(_headers, 1, model.Columns);
            while (_headers.Children.Count > model.ColumnHeaders.Count)
            {
                _headers.Children.RemoveAt(_headers.Children.Count - 1);
            }

            while (_headers.Children.Count < model.ColumnHeaders.Count)
            {
                _headers.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center });
            }

            for (var index = 0; index < model.ColumnHeaders.Count; index++)
            {
                var header = (TextBlock)_headers.Children[index];
                // Runtime-created headers have no TemplatedParent, so ControlTheme cannot select them through /template/.
                // The theme sets these metrics on DatePanel; the panel applies them to its pooled headers.
                if (header.TextAlignment != WeekHeaderTextAlignment)
                {
                    header.TextAlignment = WeekHeaderTextAlignment;
                }

                if (!MathUtils.AreClose(header.MinHeight, WeekHeaderMinHeight))
                {
                    header.MinHeight = WeekHeaderMinHeight;
                }

                if (header.Padding != WeekHeaderPadding)
                {
                    header.Padding = WeekHeaderPadding;
                }

                header.Text = model.ColumnHeaders[index];
                var automationName = index == 0 && model.ColumnHeaders[index].Length == 0 && session.Input.ShowWeek
                    ? GetWeekAutomationName() : null;
                AutomationProperties.SetName(_headers.Children[index], automationName);
                AvaloniaGrid.SetColumn(_headers.Children[index], index);
            }
        }

        // Model positions include the optional week column and quarter topology;
        // a bounded pool keeps all runtime cells under this single grid owner.
        while (_pool.Count < model.Cells.Count)
        {
            _pool.Add(new DateViewerCell());
        }

        for (var index = _cells.Children.Count - 1; index >= model.Cells.Count; index--)
        {
            ((DateViewerCell)_cells.Children[index]).Unbind();
            _cells.Children.RemoveAt(index);
        }
        for (var index = 0; index < model.Cells.Count; index++)
        {
            var cell = _pool[index];
            var state = model.Cells[index];
            var cellTemplate = state.Kind == DateViewerCellType.Week ? null : CellTemplate;
            var fullCellTemplate = state.Kind == DateViewerCellType.Week ? null : FullCellTemplate;
            var defaultFullCellTemplate = state.Kind == DateViewerCellType.Week ? null : DefaultFullCellTemplate;
            var contentChanged = !ReferenceEquals(cell.CellTemplate, cellTemplate) ||
                                 !ReferenceEquals(cell.FullCellTemplate, fullCellTemplate) ||
                                 !ReferenceEquals(cell.DefaultFullCellTemplate, defaultFullCellTemplate) || cell.Presentation != Presentation;
            // Runtime-created, recycled targets cannot be reached by a fixed TemplateBinding.
            cell.CellTemplate = cellTemplate;
            cell.FullCellTemplate = fullCellTemplate;
            cell.DefaultFullCellTemplate = defaultFullCellTemplate;
            cell.Theme = CellTheme;
            cell.Presentation = Presentation;
            if (contentChanged || cell.ContentRevision != session.ContentRevision || !ReferenceEquals(cell.Model, state) || !ReferenceEquals(cell.Session, session))
            {
                cell.Bind(session, state);
            }

            AvaloniaGrid.SetRow(cell, state.Row);
            AvaloniaGrid.SetColumn(cell, state.Column);
            if (_cells.Children.Count <= index)
            {
                _cells.Children.Add(cell);
            }
        }
    }

    private void ReleaseCells()
    {
        _cells?.Children.Clear();
        _headers?.Children.Clear();
        foreach (var cell in _pool)
        {
            cell.Unbind();
        }

        if (_layouts.Count > 0)
        {
            _layouts = Array.Empty<DateCellLayout>();
            GeometryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static void ConfigureGrid(AvaloniaGrid grid, int rows, int columns)
    {
        if (grid.RowDefinitions.Count != rows)
        {
            grid.RowDefinitions.Clear();
            for (var index = 0; index < rows; index++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            }
        }
        if (grid.ColumnDefinitions.Count != columns)
        {
            grid.ColumnDefinitions.Clear();
            for (var index = 0; index < columns; index++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || Session is null || !IsEffectivelyEnabled)
        {
            return;
        }

        var source = e.Source as Visual;
        var sourceCell = source as DateViewerCell ?? source?.GetVisualAncestors().OfType<DateViewerCell>().FirstOrDefault();
        if (sourceCell?.IsNestedInput(e.Source) == true)
        {
            return;
        }

        var direction = e.Key switch
        {
            Key.Left => DateFocusDirection.Left,
            Key.Right => DateFocusDirection.Right,
            Key.Up => DateFocusDirection.Up,
            Key.Down => DateFocusDirection.Down,
            _ => (DateFocusDirection?)null
        };
        if (direction is { } movement)
        {
            var previous = Session.FocusedValue;
            Session.Apply(new DatePanelAction.MoveFocus(movement));
            e.Handled = previous != Session.FocusedValue;
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this))
        {
            OnFocusRequested(this, EventArgs.Empty);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (Session is not { } session || _cells is null)
        {
            return;
        }

        var point = e.GetPosition(_cells);
        var hit = _cells.Children.OfType<DateViewerCell>().FirstOrDefault(cell => cell.Bounds.Contains(point));
        if (hit?.Model is { Value: { } value, IsDisabled: false })
        {
            session.Apply(new DatePanelAction.Hover(value));
        }
        else
        {
            session.Apply(new DatePanelAction.Hover(null));
        }
    }

    private void HandleRawPointer(RawInputEventArgs args)
    {
        if (args is not RawPointerEventArgs pointer || Session is not { HoveredValue: not null } session)
        {
            return;
        }

        var isInsideSharedPanel = false;
        if (pointer.Type is not (RawPointerEventType.LeaveWindow or RawPointerEventType.TouchCancel))
        {
            var panels = session.ConnectedPanels;
            for (var index = 0; index < panels.Count; index++)
            {
                var panel = panels[index];
                if (panel.IsVisible && panel.TryGetInputPosition(pointer, out var position) &&
                    new Rect(panel.Bounds.Size).Contains(position))
                {
                    isInsideSharedPanel = true;
                    break;
                }
            }
        }
        if (!isInsideSharedPanel && session.HoveredValue.HasValue)
        {
            session.Apply(new DatePanelAction.Hover(null));
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        var layouts = _pool.Where(cell => cell.Model?.Value is not null)
            .Select(cell => new DateCellLayout(cell.Model!.Value!.Value, cell.Model.Kind, cell.Model.Row, cell.Model.Column,
                new Rect(default, cell.Bounds.Size).TransformToAABB(cell.TransformToVisual(this) ?? Matrix.Identity))).ToArray();
        if (!_layouts.SequenceEqual(layouts))
        {
            _layouts = layouts;
            GeometryChanged?.Invoke(this, EventArgs.Empty);
        }
        return result;
    }

    internal IReadOnlyList<DateCellLayout> GetCellLayouts() => _layouts;

    internal IEnumerable<DateViewerCell> GetRealizedCells() => _pool.Where(cell => cell.Model is not null);

    private static string GetWeekAutomationName() => Application.Current is { } application
        ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(DateViewerLangResourceKind.Week) ?? "Week"
        : "Week";
}

internal readonly record struct DateCellLayout(DateTime Value, DateViewerCellType Kind, int Row, int Column, Rect Bounds);
