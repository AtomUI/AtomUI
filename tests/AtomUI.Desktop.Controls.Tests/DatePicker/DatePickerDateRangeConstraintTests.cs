using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DatePickerDateRangeConstraintTests
{
    [Theory]
    [InlineData(DatePickerMode.Date, 2026, 7, 15, 2026, 10, 31)]
    [InlineData(DatePickerMode.Week, 2026, 7, 12, 2026, 10, 25)]
    [InlineData(DatePickerMode.Month, 2026, 7, 1, 2026, 10, 1)]
    [InlineData(DatePickerMode.Quarter, 2026, 7, 1, 2026, 10, 1)]
    [InlineData(DatePickerMode.Year, 2026, 1, 1, 2026, 1, 1)]
    public void Create_Normalizes_Inclusive_Bounds_By_PickerMode(
        DatePickerMode pickerMode,
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        var constraint = DatePickerDateRangeConstraint.Create(
            new DateTime(2026, 7, 15, 13, 30, 0),
            new DateTime(2026, 10, 31, 22, 10, 0),
            pickerMode);

        constraint.Start.ShouldBe(new DateTime(startYear, startMonth, startDay));
        constraint.End.ShouldBe(new DateTime(endYear, endMonth, endDay));
        constraint.Contains(constraint.Start).ShouldBeTrue();
        constraint.Contains(constraint.End).ShouldBeTrue();
    }

    [Fact]
    public void Create_With_Reversed_Bounds_Collapses_To_Minimum_Picker_Unit()
    {
        var constraint = DatePickerDateRangeConstraint.Create(
            new DateTime(2026, 9, 20),
            new DateTime(2026, 8, 1),
            DatePickerMode.Month);

        constraint.Start.ShouldBe(new DateTime(2026, 9, 1));
        constraint.End.ShouldBe(new DateTime(2026, 9, 1));
    }

    [Fact]
    public void Contains_Ignores_Time_And_Rejects_Values_Outside_Bounds()
    {
        var constraint = DatePickerDateRangeConstraint.Create(
            new DateTime(2026, 7, 15, 18, 0, 0),
            new DateTime(2026, 7, 20, 1, 0, 0),
            DatePickerMode.Date);

        constraint.Contains(new DateTime(2026, 7, 15, 0, 0, 1)).ShouldBeTrue();
        constraint.Contains(new DateTime(2026, 7, 20, 23, 59, 59)).ShouldBeTrue();
        constraint.Contains(new DateTime(2026, 7, 14)).ShouldBeFalse();
        constraint.Contains(new DateTime(2026, 7, 21)).ShouldBeFalse();
        constraint.Contains(null).ShouldBeFalse();
    }

    [Fact]
    public void Clamp_Normalizes_And_Clamps_To_One_Sided_Bounds()
    {
        var minimum = DatePickerDateRangeConstraint.Create(
            new DateTime(2026, 7, 15),
            null,
            DatePickerMode.Month);
        var maximum = DatePickerDateRangeConstraint.Create(
            null,
            new DateTime(2026, 10, 31),
            DatePickerMode.Month);

        minimum.Clamp(new DateTime(2026, 6, 20)).ShouldBe(new DateTime(2026, 7, 1));
        minimum.Clamp(new DateTime(2026, 8, 20)).ShouldBe(new DateTime(2026, 8, 1));
        maximum.Clamp(new DateTime(2026, 11, 20)).ShouldBe(new DateTime(2026, 10, 1));
        maximum.Clamp(new DateTime(2026, 9, 20)).ShouldBe(new DateTime(2026, 9, 1));
    }

    [Fact]
    public void Create_With_No_Bounds_Only_Normalizes_Values()
    {
        var constraint = DatePickerDateRangeConstraint.Create(null, null, DatePickerMode.Quarter);

        constraint.Start.ShouldBeNull();
        constraint.End.ShouldBeNull();
        constraint.Contains(new DateTime(2026, 8, 20)).ShouldBeTrue();
        constraint.Clamp(new DateTime(2026, 8, 20)).ShouldBe(new DateTime(2026, 7, 1));
    }
}
