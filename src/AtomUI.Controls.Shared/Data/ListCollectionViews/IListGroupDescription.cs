using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

public interface IListGroupDescription
{
    AvaloniaList<object> GroupKeys { get; }
    object? GroupKeyFromItem(object item, int level, CultureInfo culture);
    bool KeysMatch(object groupKey, object itemKey);
}