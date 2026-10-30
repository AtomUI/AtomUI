using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls;

/// <summary>One editor owns raw candidates, clock cursor and confirmation progress. No visuals or popup.</summary>
internal sealed class DatePickerEditSession
{
    public DateViewerRange Draft { get; private set; } = new(null, null);
    internal bool IsOpen => _input is not null;
    public DateRangeActivePart ActivePart { get; private set; }
    public TimeSpan? TimeCursor { get; private set; }
    public TimeSpan TimeDisplayValue { get; private set; }
    public DateTime? SelectedDateForDisplay => _shortcutDate ?? Draft.Start;
    public DatePickerEditInput Input => _input ?? throw new InvalidOperationException("Open the editor first.");
    public bool CanConfirm => Constraints.Contains(ActiveValue);
    public DateTime? OpenAnchor
    {
        get
        {
            var value = Input.IsRange ? ActiveValue ?? Input.DisplayDate : Draft.Start ?? Input.DisplayDate;
            return value is { } date ? Constraints.Clamp(date) : null;
        }
    }

    private DatePickerEditInput? _input;
    private DateTime? _shortcutDate;
    private bool _confirmedStart;
    private bool _confirmedEnd;
    private DateTime? ActiveValue => ActivePart == DateRangeActivePart.Start ? Draft.Start : Draft.End;
    private DatePickerDateRangeConstraint Constraints => DatePickerDateRangeConstraint.Create(Input.MinDate, Input.MaxDate, Input.Mode);

    public void Open(DatePickerEditInput input)
    {
        var previous = _input;
        _input = input;
        ActivePart = input.ActivePart;
        // The original single presenter keeps an unconfirmed candidate across an outside close.
        // Range presenters explicitly reload both committed endpoints on every open.
        if (input.IsRange || previous is null || previous.Committed != input.Committed)
        {
            Draft = input.Committed;
            _shortcutDate = null;
        }
        if (input.IsRange && input.Committed.Start.HasValue && input.Committed.End.HasValue)
        {
            _confirmedStart = false;
            _confirmedEnd = false;
        }
        else
        {
            _confirmedStart = input.Committed.Start.HasValue;
            _confirmedEnd = input.Committed.End.HasValue;
        }
        if (previous is null || input.IsRange)
        {
            TimeDisplayValue = ActiveValue?.TimeOfDay ?? TimeSpan.Zero;
        }
    }

    public DatePickerEditResult Apply(DatePickerEditAction action)
    {
        _ = Input;
        switch (action)
        {
            case DatePickerEditAction.ChooseDate choice:
                if (!Constraints.Contains(choice.Value))
                {
                    return Snapshot();
                }

                _shortcutDate = null;
                SetActiveValue(Combine(Constraints.Normalize(choice.Value), TimeCursor ?? TimeDisplayValue));
                return Input.RequiresConfirmation ? Snapshot() : Confirm();
            case DatePickerEditAction.ChooseTime time:
                TimeCursor = time.Value;
                // Single time clicks alter the cursor/header, not the previously joined candidate.
                // Range time clicks join the active date immediately (recorded original host behavior).
                if (Input.IsRange)
                {
                    SetActiveValue(Combine(ActiveValue, time.Value));
                }

                return Snapshot();
            case DatePickerEditAction.SubmitTime time:
                TimeDisplayValue = time.Value;
                return Input.RequiresConfirmation ? Snapshot() : Confirm(allowEmptySingle: true);
            case DatePickerEditAction.PreviewDate preview:
                return Snapshot() with { HasPreview = true, PreviewValue = Combine(preview.Value, TimeCursor) };
            case DatePickerEditAction.PreviewTime preview:
                return Snapshot() with { HasPreview = true, PreviewValue = Combine(ActiveValue, preview.Value) };
            case DatePickerEditAction.PreviewCandidate:
                return Snapshot() with { HasPreview = true, PreviewValue = Combine(ActiveValue, TimeCursor ?? TimeDisplayValue) };
            case DatePickerEditAction.ActivatePart activation:
                ActivePart = activation.Part;
                TimeDisplayValue = ActiveValue?.TimeOfDay ?? TimeSpan.Zero;
                return Snapshot();
            case DatePickerEditAction.Reconfigure configuration:
                var old = Input;
                _input = configuration.Input;
                if (old.Committed.Start != Input.Committed.Start)
                {
                    Draft = Draft with { Start = Input.Committed.Start };
                }

                if (old.Committed.End != Input.Committed.End)
                {
                    Draft = Draft with { End = Input.Committed.End };
                }

                _shortcutDate = null;
                return Snapshot();
            case DatePickerEditAction.Confirm:
                return Confirm();
            case DatePickerEditAction.Today today:
                if (!Constraints.Contains(today.Value))
                {
                    return Snapshot();
                }

                _shortcutDate = null;
                SetActiveValue(today.Value.Date);
                return Input.IsRange || !Input.RequiresConfirmation ? Confirm() : Snapshot();
            case DatePickerEditAction.Now now:
                if (!Constraints.Contains(now.Value))
                {
                    return Snapshot();
                }

                TimeDisplayValue = now.Value.TimeOfDay;
                if (Input.IsRange)
                {
                    SetActiveValue(now.Value);
                }
                else
                {
                    _shortcutDate = now.Value;
                }

                return Input.RequiresConfirmation ? Snapshot() : Confirm(allowEmptySingle: true);
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    public DatePickerEditResult Close(DatePickerCloseReason reason)
    {
        if (Input.IsRange)
        {
            Draft = Input.Committed;
            _shortcutDate = null;
            TimeCursor = null;
            return Snapshot();
        }
        if (reason == DatePickerCloseReason.Dismissed)
        {
            Draft = new DateViewerRange(null, null);
            _shortcutDate = null;
        }
        return Snapshot();
    }

    private DatePickerEditResult Confirm(bool allowEmptySingle = false)
    {
        if (!CanConfirm && !(allowEmptySingle && !Input.IsRange))
        {
            return Snapshot();
        }

        if (!Input.IsRange)
        {
            _shortcutDate = null;
            return Snapshot() with { CommitKind = DatePickerCommitKind.Final, CommitValue = Draft, ShouldClose = true };
        }
        if (ActivePart == DateRangeActivePart.Start)
        {
            _confirmedStart = Draft.Start.HasValue;
        }
        else
        {
            _confirmedEnd = Draft.End.HasValue;
        }

        if (!_confirmedStart || !_confirmedEnd || Draft.Start is null || Draft.End is null)
        {
            ActivePart = ActivePart == DateRangeActivePart.Start
                ? DateRangeActivePart.End
                : DateRangeActivePart.Start;
            TimeDisplayValue = ActiveValue?.TimeOfDay ?? TimeSpan.Zero;
            return Snapshot() with { CommitKind = DatePickerCommitKind.Partial, CommitValue = Draft };
        }
        // The picker orders dates only. Equal-day endpoints retain their original clock order.
        var committed = Draft.End.Value.Date < Draft.Start.Value.Date
            ? new DateViewerRange(Draft.End, Draft.Start) : Draft;
        return Snapshot() with { CommitKind = DatePickerCommitKind.Final, CommitValue = committed, ShouldClose = true };
    }

    private DatePickerEditResult Snapshot() => new(Draft, ActivePart, CanConfirm);

    private void SetActiveValue(DateTime? value) => Draft = ActivePart == DateRangeActivePart.Start
        ? Draft with { Start = value } : Draft with { End = value };

    private DateTime? Combine(DateTime? date, TimeSpan? time)
    {
        if (date is not { } value)
        {
            return null;
        }

        return Input.HasTimePanel && time is { } clock ? value.Date.Add(clock) : value.Date;
    }
}
