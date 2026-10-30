namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>One interaction cursor shared by all panels; selection remains with the host.</summary>
internal sealed class DatePanelSession
{
    public DateTime? FocusedValue { get; private set; }
    public DateTime? HoveredValue { get; private set; }
    public IReadOnlyList<DatePanelModel> Models { get; private set; } = Array.Empty<DatePanelModel>();
    public DatePanelInput Input => _input;
    internal Func<DatePanelSession, DateViewerCellModel, DateViewerCellContext?>? ContentFactory { get; private set; }
    internal Func<DatePanelSession, DateViewerCellModel, string?>? AutomationNameFactory { get; private set; }
    internal long ContentRevision { get; private set; }
    public event EventHandler? Changed;
    public event EventHandler? FocusRequested;
    internal IReadOnlyList<DatePanel> ConnectedPanels => _connectedPanels;

    private readonly IDatePanelHost _host;
    private readonly List<DatePanel> _connectedPanels = new(2);
    private DatePanelInput _input;
    private DatePanelInput? _topologyInput;

    public DatePanelSession(IDatePanelHost host)
    {
        _host = host;
        _input = host.ReadInput();
        FocusedValue = _input.SelectedDate ?? _input.Range?.Start ?? _input.DisplayDate;
        UpdateInput();
    }

    internal void ConnectPanel(DatePanel panel)
    {
        if (!_connectedPanels.Contains(panel))
        {
            _connectedPanels.Add(panel);
        }
    }

    internal void DisconnectPanel(DatePanel panel) => _connectedPanels.Remove(panel);

    public void UpdateInput(bool refreshAvailability = false)
    {
        _input = _host.ReadInput();
        if (_input.PanelCount is < 1 or > 2)
        {
            throw new InvalidOperationException("A date viewer has one or two panels.");
        }

        Rebuild(refreshAvailability);
        if (!FindFocusCell(FocusedValue)?.IsFocusable ?? true)
        {
            var preferred = new[] { _input.SelectedDate, _input.Range?.Start, _input.Range?.End, _input.DisplayDate }
                .FirstOrDefault(value => FindFocusCell(value)?.IsFocusable == true);
            FocusedValue = preferred ?? Models.SelectMany(model => model.Cells).FirstOrDefault(cell => cell.IsFocusable)?.Value;
            Rebuild();
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void ResetInteraction()
    {
        var input = _host.ReadInput();
        FocusedValue = input.SelectedDate ?? input.Range?.Start ?? input.Range?.End ?? input.DisplayDate;
        HoveredValue = null;
        UpdateInput();
    }

    internal void SetContentFactory(Func<DatePanelSession, DateViewerCellModel, DateViewerCellContext?>? factory)
    {
        if (ReferenceEquals(ContentFactory, factory))
        {
            return;
        }

        ContentFactory = factory;
        RefreshContent();
    }

    internal void SetAutomationNameFactory(Func<DatePanelSession, DateViewerCellModel, string?>? factory) =>
        AutomationNameFactory = factory;

    internal void RefreshContent()
    {
        ContentRevision++;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal DateViewerCellContext? CreateContext(DateViewerCellModel model)
    {
        if (ContentFactory is { } factory)
        {
            return factory(this, model);
        }

        if (model.Value is not { } value)
        {
            return null;
        }

        return new DateViewerCellContext(value, Input.Today, model.Kind, model.DisplayText, model.IsToday,
            model.IsInView, model.IsSelected, model.IsDisabled, model.IsFocused, model.IsRangeStart, model.IsRangeEnd,
            model.IsRangeMiddle, model.IsRangePreviewStart, model.IsRangePreviewEnd, model.IsRangePreviewMiddle);
    }

    public void Apply(DatePanelAction action)
    {
        switch (action)
        {
            case DatePanelAction.Navigate navigation:
                _host.NavigateTo(DatePanelAlgorithms.Navigate(_input.DisplayDate, _input.PanelKind, navigation.Periods), _input.PanelKind);
                UpdateInput();
                break;
            case DatePanelAction.ChangePanel panel:
                _host.NavigateTo(panel.Anchor ?? _input.DisplayDate, panel.Kind);
                UpdateInput();
                break;
            case DatePanelAction.Focus focus:
                UpdateInput();
                if (FocusedValue != focus.Value.Date && FindFocusCell(focus.Value) is { IsFocusable: true })
                {
                    FocusedValue = focus.Value.Date;
                    Rebuild();
                    Changed?.Invoke(this, EventArgs.Empty);
                }
                break;
            case DatePanelAction.MoveFocus movement:
                MoveFocus(movement.Direction);
                break;
            case DatePanelAction.Hover hover:
                var target = hover.Value?.Date;
                if (target.HasValue && FindFocusCell(target) is not { IsDisabled: false })
                {
                    target = null;
                }

                if (HoveredValue != target)
                {
                    HoveredValue = target;
                    Rebuild();
                    Changed?.Invoke(this, EventArgs.Empty);
                    _host.Preview(target);
                }
                break;
            case DatePanelAction.Activate activation:
                Activate(activation.Value, activation.Kind);
                break;
            case DatePanelAction.ActivateFocused:
                UpdateInput();
                if (FindFocusCell(FocusedValue) is { Value: { } value } cell)
                {
                    Activate(value, cell.Kind);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void Activate(DateTime value, DateViewerCellType kind)
    {
        // A delegate may close over mutable business state; revalidate on activation.
        UpdateInput(refreshAvailability: true);
        if (kind == DateViewerCellType.Week && _input.SelectionUnit != DateViewerSelectionUnit.Week && !_input.AllowWeekActivation)
        {
            return;
        }

        var cell = Models.SelectMany(model => model.Cells)
            .FirstOrDefault(candidate => candidate.Value == value.Date && candidate.Kind == kind && !candidate.IsDisabled);
        if (cell?.Period is not { } period)
        {
            return;
        }

        FocusedValue = value.Date;
        HoveredValue = null;
        _host.Activate(new DateCellSelection(value.Date, kind, period));
        UpdateInput();
    }

    private void MoveFocus(DateFocusDirection direction)
    {
        UpdateInput();
        if (FocusedValue is not { } current)
        {
            return;
        }

        var columns = _input.PanelKind == DateViewerPanelKind.Date ? 7 : _input.PanelKind == DateViewerPanelKind.Quarter ? 4 : 3;
        var step = direction switch
        {
            DateFocusDirection.Left => -1,
            DateFocusDirection.Right => 1,
            DateFocusDirection.Up => -columns,
            DateFocusDirection.Down => columns,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
        var candidate = current;
        while (TryAdvance(candidate, step, out candidate))
        {
            var cell = FindFocusCell(candidate);
            if (cell is null)
            {
                return;
            }

            if (!cell.IsFocusable)
            {
                continue;
            }

            FocusedValue = candidate;
            Rebuild();
            Changed?.Invoke(this, EventArgs.Empty);
            FocusRequested?.Invoke(this, EventArgs.Empty);
            return;
        }
    }

    private bool TryAdvance(DateTime value, int step, out DateTime result)
    {
        try
        {
            result = _input.PanelKind switch
            {
                DateViewerPanelKind.Date => value.AddDays(step),
                DateViewerPanelKind.Month => value.AddMonths(step),
                DateViewerPanelKind.Quarter => value.AddMonths(step * 3),
                DateViewerPanelKind.Year => value.AddYears(step),
                _ => throw new ArgumentOutOfRangeException()
            };
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = value;
            return false;
        }
    }

    private DateViewerCellModel? FindFocusCell(DateTime? value) => value is { } target
        ? Models.SelectMany(model => model.Cells).FirstOrDefault(cell => cell.Kind != DateViewerCellType.Week && cell.Period?.Contains(target) == true)
        : null;

    private void Rebuild(bool refreshAvailability = false)
    {
        var effective = _input with { FocusedValue = FocusedValue, HoveredValue = HoveredValue };
        var topology = _input with
        {
            DisplayDate = DatePanelAlgorithms.GetPanelAnchor(_input.DisplayDate, _input.PanelKind),
            SelectedDate = null, Range = null, FocusedValue = null, HoveredValue = null,
            Today = default, ActiveRangePart = default
        };
        if (refreshAvailability || _topologyInput != topology)
        {
            Models = Enumerable.Range(0, _input.PanelCount).Select(index => DatePanelAlgorithms.Build(effective, index)).ToArray();
            _topologyInput = topology;
            return;
        }
        Models = Models.Select(model => DatePanelAlgorithms.ProjectState(effective, model)).ToArray();
    }
}
