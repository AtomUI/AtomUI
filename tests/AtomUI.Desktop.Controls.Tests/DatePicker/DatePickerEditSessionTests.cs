// Pure editor regression coverage for partial/final confirmation and time cursor semantics.
using System.Reflection;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DatePickerEditSessionTests
{
    [Fact]
    public void Preview_Does_Not_Replace_The_Confirmed_Candidate()
    {
        var session = Open(new DatePickerEditInput { RequestedConfirmation = true });
        var chosen = Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 10, 2)));
        chosen.CommitKind.ShouldBe(DatePickerCommitKind.None);
        var preview = Apply(session, new DatePickerEditAction.PreviewDate(new DateTime(2026, 11, 7)));
        preview.PreviewValue.ShouldBe(new DateTime(2026, 11, 7));
        preview.Draft.Start.ShouldBe(new DateTime(2026, 10, 2));
        var commit = Apply(session, new DatePickerEditAction.Confirm());
        commit.CommitValue!.Start.ShouldBe(new DateTime(2026, 10, 2));
        commit.ShouldClose.ShouldBeTrue();
    }

    [Fact]
    public void End_First_Confirmation_Is_Partial_And_Reverse_Draft_Stays_Raw()
    {
        var session = Open(new DatePickerEditInput { IsRange = true, RequestedConfirmation = true, ActivePart = DateRangeActivePart.End });
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 10, 8)));
        var partial = Apply(session, new DatePickerEditAction.Confirm());
        partial.CommitKind.ShouldBe(DatePickerCommitKind.Partial);
        partial.ActivePart.ShouldBe(DateRangeActivePart.Start);
        partial.ShouldClose.ShouldBeFalse();
        var draft = Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 11, 19)));
        draft.Draft.ShouldBe(new DateViewerRange(new DateTime(2026, 11, 19), new DateTime(2026, 10, 8)));
        var complete = Apply(session, new DatePickerEditAction.Confirm());
        complete.CommitValue.ShouldBe(new DateViewerRange(new DateTime(2026, 10, 8), new DateTime(2026, 11, 19)));
        complete.Draft.ShouldBe(draft.Draft);
    }

    [Fact]
    public void Complete_Range_Reopen_Reconfirms_From_The_Active_End_Before_Final_Close()
    {
        var session = Open(new DatePickerEditInput
        {
            IsRange = true,
            ActivePart = DateRangeActivePart.End,
            Committed = new DateViewerRange(new DateTime(2026, 10, 12), new DateTime(2026, 11, 23))
        });

        var partial = Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 11, 16)));

        partial.CommitKind.ShouldBe(DatePickerCommitKind.Partial);
        partial.ShouldClose.ShouldBeFalse();
        partial.ActivePart.ShouldBe(DateRangeActivePart.Start);
        partial.CommitValue.ShouldBe(new DateViewerRange(new DateTime(2026, 10, 12), new DateTime(2026, 11, 16)));

        var complete = Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 10, 13)));

        complete.CommitKind.ShouldBe(DatePickerCommitKind.Final);
        complete.ShouldClose.ShouldBeTrue();
        complete.CommitValue.ShouldBe(new DateViewerRange(new DateTime(2026, 10, 13), new DateTime(2026, 11, 16)));
    }

    [Fact]
    public void Partial_Time_Confirmation_Carries_The_Cursor_Into_The_Empty_End()
    {
        var session = Open(new DatePickerEditInput { IsRange = true, ShowTime = true });
        Apply(session, new DatePickerEditAction.ChooseTime(TimeSpan.FromSeconds(2)));
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 10, 12)));
        var partial = Apply(session, new DatePickerEditAction.Confirm());
        partial.CommitKind.ShouldBe(DatePickerCommitKind.Partial);
        partial.ActivePart.ShouldBe(DateRangeActivePart.End);
        var end = Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 10, 15)));
        end.Draft.End.ShouldBe(new DateTime(2026, 10, 15, 0, 0, 2));
        Apply(session, new DatePickerEditAction.Confirm()).CommitValue.ShouldBe(new DateViewerRange(
            new DateTime(2026, 10, 12, 0, 0, 2), new DateTime(2026, 10, 15, 0, 0, 2)));
    }

    [Fact]
    public void Same_Day_Endpoints_Keep_Their_Distinct_Times()
    {
        var session = Open(new DatePickerEditInput { IsRange = true, ShowTime = true });
        Apply(session, new DatePickerEditAction.ChooseTime(new TimeSpan(18, 45, 0)));
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 15)));
        Apply(session, new DatePickerEditAction.Confirm());
        Apply(session, new DatePickerEditAction.ActivatePart(DateRangeActivePart.End));
        Apply(session, new DatePickerEditAction.ChooseTime(new TimeSpan(9, 30, 0)));
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 15)));
        var complete = Apply(session, new DatePickerEditAction.Confirm());
        complete.CommitValue.ShouldBe(new DateViewerRange(new DateTime(2026, 7, 15, 18, 45, 0), new DateTime(2026, 7, 15, 9, 30, 0)));
    }

    [Fact]
    public void External_Boundary_Changes_Disable_Confirmation_Without_Rewriting_The_Draft()
    {
        var input = new DatePickerEditInput { RequestedConfirmation = true, Committed = new DateViewerRange(new DateTime(2026, 7, 20), null) };
        var session = Open(input);
        var changed = Apply(session, new DatePickerEditAction.Reconfigure(input with { MinDate = new DateTime(2026, 8, 1) }));
        changed.Draft.ShouldBe(input.Committed);
        changed.CanConfirm.ShouldBeFalse();
        Apply(session, new DatePickerEditAction.Confirm()).CommitKind.ShouldBe(DatePickerCommitKind.None);
    }

    [Fact]
    public void Time_Policy_Does_Not_Mutate_The_Raw_Confirmation_Request()
    {
        var input = new DatePickerEditInput { IsRange = true, ShowTime = true };
        var session = Open(input);
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 20))).CommitKind.ShouldBe(DatePickerCommitKind.None);
        input.RequestedConfirmation.ShouldBeFalse();
        Apply(session, new DatePickerEditAction.Reconfigure(input with { Mode = DatePickerMode.Month }));
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 20))).CommitKind.ShouldBe(DatePickerCommitKind.Partial);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Single_Time_Cursor_Uses_The_Recorded_Order_Of_Selection(bool timeFirst)
    {
        var session = Open(new DatePickerEditInput { RequestedConfirmation = true, ShowTime = true });
        if (timeFirst) Apply(session, new DatePickerEditAction.ChooseTime(TimeSpan.FromSeconds(2)));
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 20)));
        if (!timeFirst) Apply(session, new DatePickerEditAction.ChooseTime(TimeSpan.FromSeconds(2)));
        Apply(session, new DatePickerEditAction.Confirm()).CommitValue!.Start.ShouldBe(new DateTime(2026, 7, 20).AddSeconds(timeFirst ? 2 : 0));
    }

    [Fact]
    public void Single_Now_Updates_The_Scene_Without_Creating_A_Candidate()
    {
        var session = Open(new DatePickerEditInput { RequestedConfirmation = true, ShowTime = true });
        var result = Apply(session, new DatePickerEditAction.Now(new DateTime(2026, 10, 2, 12, 34, 56)));
        result.Draft.Start.ShouldBeNull();
        result.CanConfirm.ShouldBeFalse();
        Read<DateTime?>(session, "SelectedDateForDisplay").ShouldBe(new DateTime(2026, 10, 2, 12, 34, 56));
        Read<TimeSpan>(session, "TimeDisplayValue").ShouldBe(new TimeSpan(12, 34, 56));
    }

    [Fact]
    public void Single_Candidate_Survives_Outside_Close_And_Reopen()
    {
        var input = new DatePickerEditInput { RequestedConfirmation = true };
        var session = Open(input);
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 20)));
        Close(session, DatePickerCloseReason.Outside).CommitKind.ShouldBe(DatePickerCommitKind.None);
        session.GetType().GetMethod("Open")!.Invoke(session, [input]);
        Read<DateViewerRange>(session, "Draft").Start.ShouldBe(new DateTime(2026, 7, 20));
    }

    [Fact]
    public void Incomplete_Range_Close_Discards_Candidate_And_Preserves_Confirmed_Endpoint()
    {
        var confirmedEnd = new DateTime(2026, 7, 25);
        var input = new DatePickerEditInput
        {
            IsRange = true,
            RequestedConfirmation = true,
            Committed = new DateViewerRange(null, confirmedEnd),
            ActivePart = DateRangeActivePart.Start
        };
        var session = Open(input);
        Apply(session, new DatePickerEditAction.ChooseDate(new DateTime(2026, 7, 20)));
        var result = Close(session, DatePickerCloseReason.Outside);
        result.CommitKind.ShouldBe(DatePickerCommitKind.None);
        result.Draft.ShouldBe(input.Committed);
    }

    private static object Open(DatePickerEditInput input)
    {
        var type = typeof(DatePicker).Assembly.GetType("AtomUI.Desktop.Controls.DatePickerEditSession");
        type.ShouldNotBeNull("the editor owns candidates and confirmation progress independently of visuals");
        var session = Activator.CreateInstance(type!, nonPublic: true)!;
        type!.GetMethod("Open", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(session, [input]);
        return session;
    }

    private static DatePickerEditResult Apply(object session, DatePickerEditAction action) =>
        (DatePickerEditResult)session.GetType().GetMethod("Apply", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(session, [action])!;

    private static DatePickerEditResult Close(object session, DatePickerCloseReason reason) =>
        (DatePickerEditResult)session.GetType().GetMethod("Close", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(session, [reason])!;

    private static T Read<T>(object session, string property) => (T)session.GetType().GetProperty(property)!.GetValue(session)!;
}
