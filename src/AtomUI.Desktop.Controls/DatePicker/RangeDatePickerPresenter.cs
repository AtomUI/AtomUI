using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class RangeDatePickerPresenter : DatePickerPresenter
{
    public static readonly StyledProperty<DateTime?> SecondarySelectedDateTimeProperty =
        AvaloniaProperty.Register<RangeDatePickerPresenter, DateTime?>(nameof(SecondarySelectedDateTime));
    internal static readonly StyledProperty<bool> IsRangeStartActiveProperty =
        AvaloniaProperty.Register<RangeDatePickerPresenter, bool>(nameof(IsRangeStartActive), true);

    public DateTime? SecondarySelectedDateTime
    {
        get => GetValue(SecondarySelectedDateTimeProperty);
        set => SetValue(SecondarySelectedDateTimeProperty, value);
    }
    internal bool IsRangeStartActive
    {
        get => GetValue(IsRangeStartActiveProperty);
        set => SetValue(IsRangeStartActiveProperty, value);
    }

    public event EventHandler? RangePartConfirmed;

    protected override bool IsRangeEditor => true;
    protected override DateTime? SecondarySelectedDateTimeCore => SecondarySelectedDateTime;
    protected override DateRangeActivePart ActiveRangePart =>
        IsRangeStartActive ? DateRangeActivePart.Start : DateRangeActivePart.End;

    internal void NotifySelectRangeStart(bool isStart)
    {
        SetCurrentValue(IsRangeStartActiveProperty, isStart);
        if (EditSession.IsOpen)
        {
            ApplyResult(EditSession.Apply(new DatePickerEditAction.ActivatePart(ActiveRangePart)));
        }

        SyncTimeViewTimeValue();
        SetupConfirmButtonEnableStatus();
    }

    internal void ResetRangePickState()
    {
        // Confirmation flags are reconstructed from both committed endpoints on Open.
    }

    internal void ResetRangeOpenPanelState() => ResetOpenPanelState();

    internal void RestoreCommittedRangeAfterClose(DateViewerRange committed)
    {
        if (!EditSession.IsOpen)
        {
            return;
        }

        EditSession.Apply(new DatePickerEditAction.Reconfigure(CreateInput() with { Committed = committed }));
        ApplyResult(EditSession.Close(DatePickerCloseReason.Outside));
    }

    internal void NotifyRepairReverseRange(bool isRepair)
    {
        // Raw endpoint order is always retained; final date ordering is an editor result.
    }

    protected override bool IsRangeProperty(AvaloniaProperty property) =>
        property == SecondarySelectedDateTimeProperty;

    protected override DateTime? ResolveOpenDisplayAnchor()
    {
        var active = EditSession.OpenAnchor;
        if (active is null || PanelCount == 1)
        {
            return active;
        }

        return EditSession.Draft.Start ?? active;
    }

    protected override void ApplyDraft(DateViewerRange draft)
    {
        UpdatingCandidate = true;
        SetCurrentValue(SelectedDateTimeProperty, draft.Start);
        SetCurrentValue(SecondarySelectedDateTimeProperty, draft.End);
        UpdatingCandidate = false;
    }

    protected override void OnPartialConfirmed(DatePickerEditResult result)
    {
        SetCurrentValue(IsRangeStartActiveProperty, result.ActivePart == DateRangeActivePart.Start);
        RangePartConfirmed?.Invoke(this, EventArgs.Empty);
    }

    protected override void SetupConfirmButtonEnableStatus()
    {
        if (ConfirmButton is not null)
        {
            ConfirmButton.IsEnabled = EditSession.CanConfirm;
        }
    }

    protected override void NotifyTimeViewHoverChanged(TimeSpan? value) =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.PreviewTime(value)));

    protected override void NotifyTodayButtonClicked() =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.Today(DateTime.Today)));

    protected override void NotifyNowButtonClicked() =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.Now(DateTime.Now)));

}
