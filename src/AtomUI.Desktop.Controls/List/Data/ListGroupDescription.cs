using System.Globalization;

namespace AtomUI.Desktop.Controls.Data;

public class ListGroupDescription : AbstractListGroupDescription
{
    private ListGroupPropertySelector _propertySelector;

    public ListGroupDescription(ListGroupPropertySelector selector)
    {
        _propertySelector = selector;
    }

    public override object? GroupKeyFromItem(object item, int level, CultureInfo culture)
    {
        return _propertySelector(item);
    }

    public override bool KeysMatch(object groupKey, object itemKey)
    {
        if (groupKey is string k1 && itemKey is string k2)
        {
            return string.Equals(k1, k2, StringComparison.Ordinal);
        }
        return base.KeysMatch(groupKey, itemKey);
    }
    
}