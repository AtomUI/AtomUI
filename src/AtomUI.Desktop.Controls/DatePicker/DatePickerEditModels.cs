using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls;

internal enum DatePickerCommitKind { None, Partial, Final }
internal enum DatePickerCloseReason { Accepted, Outside, Escape, Dismissed, OwnerDetached }

internal sealed record DatePickerEditInput
{
    public DatePickerMode Mode { get; init; }
    public bool IsRange { get; init; }
    public bool RequestedConfirmation { get; init; }
    public bool ShowTime { get; init; }
    public DateViewerRange Committed { get; init; } = new(null, null);
    public DateRangeActivePart ActivePart { get; init; }
    public DateTime? DisplayDate { get; init; }
    public DateTime? MinDate { get; init; }
    public DateTime? MaxDate { get; init; }
    public bool HasTimePanel => ShowTime && Mode == DatePickerMode.Date;
    public bool RequiresConfirmation => RequestedConfirmation || IsRange && HasTimePanel;
}

internal abstract record DatePickerEditAction
{
    public sealed record ChooseDate(DateTime Value) : DatePickerEditAction;
    public sealed record ChooseTime(TimeSpan? Value) : DatePickerEditAction;
    public sealed record PreviewDate(DateTime? Value) : DatePickerEditAction;
    public sealed record PreviewTime(TimeSpan? Value) : DatePickerEditAction;
    public sealed record PreviewCandidate : DatePickerEditAction;
    public sealed record SubmitTime(TimeSpan Value) : DatePickerEditAction;
    public sealed record ActivatePart(DateRangeActivePart Part) : DatePickerEditAction;
    public sealed record Reconfigure(DatePickerEditInput Input) : DatePickerEditAction;
    public sealed record Confirm : DatePickerEditAction;
    public sealed record Today(DateTime Value) : DatePickerEditAction;
    public sealed record Now(DateTime Value) : DatePickerEditAction;
}

internal sealed record DatePickerEditResult(
    DateViewerRange Draft,
    DateRangeActivePart ActivePart,
    bool CanConfirm,
    DatePickerCommitKind CommitKind = DatePickerCommitKind.None,
    DateViewerRange? CommitValue = null,
    bool ShouldClose = false,
    bool HasPreview = false,
    DateTime? PreviewValue = null);
