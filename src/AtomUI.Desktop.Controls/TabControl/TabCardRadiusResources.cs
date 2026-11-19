using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal static class TabCardRadiusResources
{
    internal static CornerRadius Read(OwnerTokenSet? tokens, int firstIndex, SizeType size, CornerRadius previous)
    {
        var index = size switch
        {
            SizeType.Small => 0,
            SizeType.Middle => 1,
            SizeType.Large => 2,
            _ => -1
        };
        var value = index < 0 ? null : tokens?.GetValue(firstIndex + index);
        if (value is CornerRadius radius)
        {
            return radius;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        if (value is string text)
        {
            try
            {
                return CornerRadius.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }
}
