namespace AtomUI.Desktop.Controls;

internal readonly record struct DatePickerDateRangeConstraint(
    DateTime? Start,
    DateTime? End,
    DatePickerMode PickerMode)
{
    internal static DatePickerDateRangeConstraint Create(
        DateTime? minDate,
        DateTime? maxDate,
        DatePickerMode pickerMode)
    {
        DateTime? start = minDate.HasValue
            ? DatePickerFormattingHelper.NormalizeDateTime(minDate.Value, pickerMode)
            : null;
        DateTime? end = maxDate.HasValue
            ? DatePickerFormattingHelper.NormalizeDateTime(maxDate.Value, pickerMode)
            : null;
        if (start.HasValue && end.HasValue && start.Value > end.Value)
        {
            end = start;
        }

        return new DatePickerDateRangeConstraint(start, end, pickerMode);
    }

    internal bool Contains(DateTime? value)
    {
        if (!value.HasValue)
        {
            return false;
        }

        var normalizedValue = Normalize(value.Value);
        return (!Start.HasValue || normalizedValue >= Start.Value) &&
               (!End.HasValue || normalizedValue <= End.Value);
    }

    internal DateTime Clamp(DateTime value)
    {
        var normalizedValue = Normalize(value);
        if (Start.HasValue && normalizedValue < Start.Value)
        {
            return Start.Value;
        }

        if (End.HasValue && normalizedValue > End.Value)
        {
            return End.Value;
        }

        return normalizedValue;
    }

    internal DateTime Normalize(DateTime value)
    {
        return DatePickerFormattingHelper.NormalizeDateTime(value, PickerMode);
    }

    internal DateTime? NormalizeNullable(DateTime? value) => value is { } date ? Normalize(date) : null;
}
