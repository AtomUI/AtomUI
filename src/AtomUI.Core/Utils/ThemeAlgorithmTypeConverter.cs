using System.ComponentModel;
using System.Globalization;
using AtomUI.Theme.Algorithms;

namespace AtomUI.Utils;

/// <summary>
/// Converts <c>Default</c>, <c>Dark</c>, <c>Compact</c> and <c>Catalog:Id</c> strings to <see cref="ThemeAlgorithm"/>.
/// </summary>
public class ThemeAlgorithmTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof (string);
    }

    public override object ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string str)
        {
            switch (str)
            {
                case "Default":
                    return ThemeAlgorithm.Default;
                case "Dark":
                    return ThemeAlgorithm.Dark;
                case "Compact":
                    return ThemeAlgorithm.Compact;
            }

            var separator = str.IndexOf(':');
            if (separator > 0 && separator == str.LastIndexOf(':'))
            {
                return new ThemeAlgorithm(str.Substring(0, separator), str.Substring(separator + 1));
            }
        }
        throw new NotSupportedException(
            "ThemeAlgorithm type converter supports 'Default', 'Dark', 'Compact' and 'Catalog:Id'.");
    }
}
