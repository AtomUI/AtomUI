using AtomUI.Controls;
namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>
/// One interaction cursor shared by all panels; selection remains with the host.
/// </summary>
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

    /// <summary>
    /// Moves focus for a navigation key forwarded from a picker input, which keeps keyboard focus. Unlike
    /// <see cref="DatePanelAction.MoveFocus" />, the shown pages follow focus, and the focused cell is hovered too so
    /// it shows the hover look and previews the range. Returns false when focus cannot move.
    /// </summary>
    internal bool MoveKeyboardFocus(DateKeyboardMove move)
    {
        UpdateInput();
        if (FocusedValue is not { } current)
        {
            return false;
        }

        var kind = _input.PanelKind;
        if (move is DateKeyboardMove.PageStart or DateKeyboardMove.PageEnd)
        {
            var page = Models.FirstOrDefault(model => model.Cells.Any(cell =>
                cell.IsInView && cell.Kind != DateViewerCellType.Week && cell.Period?.Contains(current) == true));
            var edges = page?.Cells.Where(cell => cell.IsInView && cell.IsFocusable).ToArray();
            var edge = edges is { Length: > 0 }
                ? move == DateKeyboardMove.PageStart ? edges[0] : edges[^1]
                : null;
            return edge?.Value is { } edgeValue && TryKeyboardFocus(edgeValue, out _);
        }

        if (move is DateKeyboardMove.PreviousPage or DateKeyboardMove.NextPage)
        {
            var periods = move == DateKeyboardMove.NextPage ? 1 : -1;
            try
            {
                var target = kind switch
                {
                    DateViewerPanelKind.Date => current.AddMonths(periods),
                    DateViewerPanelKind.Year => current.AddYears(periods * 10),
                    _ => current.AddYears(periods)
                };
                return !IsOutsideBounds(target) && TryKeyboardFocus(target, out _);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        var columns = kind == DateViewerPanelKind.Date ? 7 : kind == DateViewerPanelKind.Quarter ? 4 : 3;
        var step = move switch
        {
            DateKeyboardMove.Left => -1,
            DateKeyboardMove.Right => 1,
            DateKeyboardMove.Up => -columns,
            _ => columns
        };
        // Disabled cells are skipped; the bounds and the attempt cap stop the walk over a disabled stretch.
        var candidate = current;
        for (var attempt = 0; attempt < MaxKeyboardFocusAttempts && TryAdvance(candidate, step, out candidate); attempt++)
        {
            if (IsOutsideBounds(candidate))
            {
                return false;
            }

            if (TryKeyboardFocus(candidate, out var hasCell))
            {
                return true;
            }

            if (!hasCell)
            {
                return false;
            }
        }

        return false;
    }

    private const int MaxKeyboardFocusAttempts = 400;

    private bool TryKeyboardFocus(DateTime target, out bool hasCell)
    {
        var cell = FindInViewCell(target);
        DateTime? targetDisplayDate = null;
        if (cell is null)
        {
            // The pages that would show the target are probed first, so a disabled target leaves the shown pages
            // and the focus unchanged.
            var kind = _input.PanelKind;
            var anchor = DatePanelAlgorithms.GetPanelAnchor(target, kind);
            var shown = DatePanelAlgorithms.GetPanelAnchor(_input.DisplayDate, kind);
            targetDisplayDate = anchor < shown ? anchor : DatePanelAlgorithms.Navigate(anchor, kind, 1 - _input.PanelCount);
            var probe = _input with { DisplayDate = targetDisplayDate.Value };
            cell = Enumerable.Range(0, _input.PanelCount)
                             .SelectMany(index => DatePanelAlgorithms.Build(probe, index).Cells)
                             .FirstOrDefault(candidate => IsInViewCellOf(candidate, target));
        }

        hasCell = cell is not null;
        if (cell is not { IsFocusable: true, Value: { } value })
        {
            return false;
        }

        if (targetDisplayDate is { } displayDate)
        {
            _host.NavigateTo(displayDate, _input.PanelKind);
            UpdateInput();
        }

        FocusedValue = value;
        Apply(new DatePanelAction.Hover(value));
        Rebuild();
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private bool IsOutsideBounds(DateTime value)
    {
        var kind = _input.PanelKind;
        var anchor = DatePanelAlgorithms.GetPanelAnchor(value, kind);
        return _input.MinDate is { } min && anchor < DatePanelAlgorithms.GetPanelAnchor(min, kind) ||
               _input.MaxDate is { } max && anchor > DatePanelAlgorithms.GetPanelAnchor(max, kind);
    }

    private DateViewerCellModel? FindInViewCell(DateTime value) =>
        Models.SelectMany(model => model.Cells).FirstOrDefault(cell => IsInViewCellOf(cell, value));

    private static bool IsInViewCellOf(DateViewerCellModel cell, DateTime value) =>
        cell.IsInView && cell.Kind != DateViewerCellType.Week && cell.Period?.Contains(value) == true;

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

/// <summary>
/// Navigation keys a picker input forwards to its date panel.
/// </summary>
internal enum DateKeyboardMove
{
    Left,
    Right,
    Up,
    Down,
    PreviousPage,
    NextPage,
    PageStart,
    PageEnd
}
