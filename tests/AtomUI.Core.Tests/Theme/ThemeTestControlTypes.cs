using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Core.Tests.Theme;

internal static class ThemeTestControlTypes
{
    internal static Type For(string catalog, string id)
    {
        if (catalog == "First")
        {
            return typeof(FirstThemeTestControl);
        }

        if (catalog == "Second")
        {
            return typeof(SecondThemeTestControl);
        }

        return id switch
        {
            "Button" => typeof(ButtonThemeTestControl),
            "Input" => typeof(InputThemeTestControl),
            "Rating" => typeof(RatingThemeTestControl),
            "Badge" => typeof(BadgeThemeTestControl),
            "SplitView" => typeof(SplitViewThemeTestControl),
            "Window" => typeof(WindowThemeTestControl),
            "Icon" => typeof(IconThemeTestControl),
            "LineEdit" => typeof(LineEditThemeTestControl),
            "AddOnDecoratedBox" => typeof(AddOnDecoratedBoxThemeTestControl),
            "Select" => typeof(SelectThemeTestControl),
            "DatePicker" => typeof(DatePickerThemeTestControl),
            "Dialog" => typeof(DialogThemeTestControl),
            "DataGrid" => typeof(DataGridThemeTestControl),
            _ => typeof(DefaultThemeTestControl)
        };
    }
}

internal sealed class DefaultThemeTestControl : Control
{
}

internal class ButtonThemeTestControl : TemplatedControl
{
}

internal sealed class DerivedButtonThemeTestControl : ButtonThemeTestControl
{
}

internal sealed class InputThemeTestControl : Control
{
}

internal sealed class RatingThemeTestControl : Control
{
}

internal sealed class BadgeThemeTestControl : Control
{
}

internal sealed class SplitViewThemeTestControl : Control
{
}

internal sealed class WindowThemeTestControl : Control
{
}

internal sealed class IconThemeTestControl : Control
{
}

internal sealed class LineEditThemeTestControl : TemplatedControl
{
}

internal sealed class AddOnDecoratedBoxThemeTestControl : Control
{
}

internal sealed class SelectThemeTestControl : Control
{
}

internal sealed class DatePickerThemeTestControl : Control
{
}

internal sealed class DialogThemeTestControl : Control
{
}

internal sealed class DataGridThemeTestControl : Control
{
}

internal sealed class FirstThemeTestControl : Control
{
}

internal sealed class SecondThemeTestControl : Control
{
}
