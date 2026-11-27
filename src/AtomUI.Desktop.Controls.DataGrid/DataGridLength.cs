// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Globalization;
using AtomUI.Utils;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Names the sizing modes a <see cref="DataGridLength" /> can take.
/// </summary>
public enum DataGridLengthUnitType
{
    /// <summary>
    /// The grid sizes the column to fit both its header and its cells.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// The column has the fixed width given by the value, in pixels.
    /// </summary>
    Pixel = 1,
    /// <summary>
    /// The grid sizes the column to fit its cells.
    /// </summary>
    SizeToCells = 2,
    /// <summary>
    /// The grid sizes the column to fit its header.
    /// </summary>
    SizeToHeader = 3,
    /// <summary>
    /// The column takes a share of the width remaining for the star columns, weighted by the value.
    /// </summary>
    Star = 4
}

/// <summary>
/// Describes the width of a DataGrid column: a fixed number of pixels, a weighted share of the remaining
/// width, or an automatic sizing mode.
/// </summary>
[TypeConverter(typeof(DataGridLengthConverter))]
public struct DataGridLength : IEquatable<DataGridLength>
{
    // WPF uses 1.0 as the default value as well
    private const double DEFAULT_VALUE = 1.0;
    
    private readonly double _desiredValue;   //  desired value storage
    private readonly double _displayValue;   //  display value storage
    private readonly double _unitValue;      //  unit value storage
    private readonly DataGridLengthUnitType _unitType; //  unit type storage

    #region 公共属性定义

    /// <summary>
    /// Gets a <see cref="T:AtomUI.Desktop.Controls.DataGridLength" /> structure that represents the standard automatic sizing mode.
    /// </summary>
    public static DataGridLength Auto { get; } = new DataGridLength(DEFAULT_VALUE, DataGridLengthUnitType.Auto);

    /// <summary>
    /// Gets the width in pixels the column wants, or <c>double.NaN</c> until the grid has computed it.
    /// </summary>
    public double DesiredValue => _desiredValue;

    /// <summary>
    /// Gets the width in pixels the column is displayed with, or <c>double.NaN</c> until the grid has computed it.
    /// </summary>
    public double DisplayValue => _displayValue;

    /// <summary>
    ///     Returns <c>true</c> if this DataGridLength instance holds 
    ///     an absolute (pixel) value.
    /// </summary>
    public bool IsAbsolute => _unitType == DataGridLengthUnitType.Pixel;

    /// <summary>
    ///     Returns <c>true</c> if this DataGridLength instance is 
    ///     automatic (not specified).
    /// </summary>
    public bool IsAuto => _unitType == DataGridLengthUnitType.Auto;

    /// <summary>
    ///     Returns <c>true</c> if this instance is to size to the cells of a column or row.
    /// </summary>
    public bool IsSizeToCells => _unitType == DataGridLengthUnitType.SizeToCells;

    /// <summary>
    ///     Returns <c>true</c> if this instance is to size to the header of a column or row.
    /// </summary>
    public bool IsSizeToHeader => _unitType == DataGridLengthUnitType.SizeToHeader;

    /// <summary>
    ///     Returns <c>true</c> if this DataGridLength instance holds a weighted proportion
    ///     of available space.
    /// </summary>
    public bool IsStar => _unitType == DataGridLengthUnitType.Star;

    /// <summary>
    /// Gets a <see cref="T:AtomUI.Desktop.Controls.DataGridLength" /> structure that represents the cell-based automatic sizing mode.
    /// </summary>
    public static DataGridLength SizeToCells { get; } =
        new DataGridLength(DEFAULT_VALUE, DataGridLengthUnitType.SizeToCells);

    /// <summary>
    /// Gets a <see cref="T:AtomUI.Desktop.Controls.DataGridLength" /> structure that represents the header-based automatic sizing mode.
    /// </summary>
    public static DataGridLength SizeToHeader { get; } =
        new DataGridLength(DEFAULT_VALUE, DataGridLengthUnitType.SizeToHeader);

    /// <summary>
    /// Gets the <see cref="T:AtomUI.Desktop.Controls.DataGridLengthUnitType" /> that represents the current sizing mode.
    /// </summary>
    public DataGridLengthUnitType UnitType => _unitType;

    /// <summary>
    /// Gets the width in pixels for <see cref="DataGridLengthUnitType.Pixel" />, the weight for
    /// <see cref="DataGridLengthUnitType.Star" /> and 1.0 for <see cref="DataGridLengthUnitType.Auto" />.
    /// </summary>
    public double Value => _unitValue;

    #endregion
    
    /// <summary>
    /// Initializes an instance that holds a fixed width of <paramref name="value" /> pixels.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is <c>double.NaN</c> or infinite.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value" /> is negative.
    /// </exception>
    public DataGridLength(double value)
        : this(value, DataGridLengthUnitType.Pixel)
    {
    }
    
    /// <summary>
    /// Initializes an instance with the given sizing mode and value.
    /// </summary>
    /// <param name="value">
    /// The width in pixels for <see cref="DataGridLengthUnitType.Pixel" /> and the weight for
    /// <see cref="DataGridLengthUnitType.Star" />. The other modes size the column from its content.
    /// </param>
    /// <param name="type">
    /// The sizing mode of the column.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is <c>double.NaN</c> or infinite, or <paramref name="type" /> is not a valid
    /// <see cref="DataGridLengthUnitType" /> value.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value" /> is negative.
    /// </exception>
    public DataGridLength(double value, DataGridLengthUnitType type)
        : this(value, type, (type == DataGridLengthUnitType.Pixel ? value : Double.NaN), (type == DataGridLengthUnitType.Pixel ? value : Double.NaN))
    {
    }
    
    /// <summary>
    /// Initializes an instance with the given sizing mode, value, desired width and display width.
    /// </summary>
    /// <param name="value">
    /// The width in pixels for <see cref="DataGridLengthUnitType.Pixel" /> and the weight for
    /// <see cref="DataGridLengthUnitType.Star" />. The other modes size the column from its content.
    /// </param>
    /// <param name="type">
    /// The sizing mode of the column.
    /// </param>
    /// <param name="desiredValue">
    /// The width in pixels the column should have, exposed as <see cref="DesiredValue" />.
    /// <c>double.NaN</c> leaves it to the grid to compute.
    /// </param>
    /// <param name="displayValue">
    /// The width in pixels the column is displayed with, exposed as <see cref="DisplayValue" />.
    /// <c>double.NaN</c> makes the grid derive it from the desired width.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is <c>double.NaN</c>, one of the three values is infinite, or
    /// <paramref name="type" /> is not a valid <see cref="DataGridLengthUnitType" /> value.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value" />, <paramref name="desiredValue" /> or <paramref name="displayValue" /> is negative.
    /// </exception>
    public DataGridLength(double value, DataGridLengthUnitType type, double desiredValue, double displayValue)
    {
        if (double.IsNaN(value))
        {
            throw DataGridError.DataGrid.ValueCannotBeSetToNAN("value");
        }
        if (double.IsInfinity(value))
        {
            throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("value");
        }
        if (double.IsInfinity(desiredValue))
        {
            throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("desiredValue");
        }
        if (double.IsInfinity(displayValue))
        {
            throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("displayValue");
        }
        if (value < 0)
        {
            throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "value", 0);
        }
        if (desiredValue < 0)
        {
            throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("desiredValue", "desiredValue", 0);
        }
        if (displayValue < 0)
        {
            throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("displayValue", "displayValue", 0);
        }

        if (type != DataGridLengthUnitType.Auto &&
            type != DataGridLengthUnitType.SizeToCells &&
            type != DataGridLengthUnitType.SizeToHeader &&
            type != DataGridLengthUnitType.Star &&
            type != DataGridLengthUnitType.Pixel)
        {
            throw DataGridError.DataGridLength.InvalidUnitType("type");
        }

        _desiredValue = desiredValue;
        _displayValue = displayValue;
        _unitValue    = (type == DataGridLengthUnitType.Auto) ? DEFAULT_VALUE : value;
        _unitType     = type;
    }

    /// <summary>
    /// Overloaded operator, compares 2 DataGridLength's.
    /// </summary>
    /// <param name="gl1">
    /// first DataGridLength to compare.
    /// </param>
    /// <param name="gl2">
    /// second DataGridLength to compare.
    /// </param>
    /// <returns>
    /// true if specified DataGridLength have same value,
    /// unit type, desired value, and display value.
    /// </returns>
    public static bool operator ==(DataGridLength gl1, DataGridLength gl2)
    {
        return (gl1.UnitType == gl2.UnitType
                && MathUtils.AreClose(gl1.Value, gl2.Value)
                && MathUtils.AreClose(gl1.DesiredValue, gl2.DesiredValue)
                && MathUtils.AreClose(gl1.DisplayValue, gl2.DisplayValue));
    }

    /// <summary>
    /// Overloaded operator, compares 2 DataGridLength's.
    /// </summary>
    /// <param name="gl1">
    /// first DataGridLength to compare.
    /// </param>
    /// <param name="gl2">
    /// second DataGridLength to compare.
    /// </param>
    /// <returns>
    /// true if specified DataGridLength have either different value,
    /// unit type, desired value, or display value.
    /// </returns>
    public static bool operator !=(DataGridLength gl1, DataGridLength gl2)
    {
        return (gl1.UnitType != gl2.UnitType
                || !MathUtils.AreClose(gl1.Value, gl2.Value)
                || !MathUtils.AreClose(gl1.DesiredValue, gl2.DesiredValue)
                || !MathUtils.AreClose(gl1.DisplayValue, gl2.DisplayValue));
    }

    /// <summary>
    /// Compares this instance of DataGridLength with another instance.
    /// </summary>
    /// <param name="other">
    /// DataGridLength length instance to compare.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="other" /> has the same unit type, value, desired value and
    /// display value as this instance.
    /// </returns>
    public bool Equals(DataGridLength other)
    {
        return (this == other);
    }

    /// <summary>
    /// Compares this instance of DataGridLength with another object.
    /// </summary>
    /// <param name="obj">
    /// Reference to an object for comparison.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> is a <see cref="DataGridLength" /> that is equal to
    /// this instance.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is DataGridLength dataGridLength)
        {
            return this == dataGridLength;
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code derived from the unit type, value, desired value and display value.
    /// </summary>
    public override int GetHashCode()
    {
        return ((int)_unitValue + (int)_unitType) + (int)_desiredValue + (int)_displayValue;
    }

}

/// <summary>
/// DataGridLengthConverter - Converter class for converting instances of other types to and from DataGridLength instances.
/// </summary> 
public class DataGridLengthConverter : TypeConverter
{
    private static readonly string StarSuffix = "*";
    private static readonly string[] ValueInvariantUnitStrings = ["auto", "sizetocells", "sizetoheader"];
    private static readonly DataGridLength[] ValueInvariantDataGridLengths = [DataGridLength.Auto, DataGridLength.SizeToCells, DataGridLength.SizeToHeader];

    /// <summary>
    /// Checks whether or not this class can convert from a given type.
    /// </summary>
    /// <param name="context">
    /// An ITypeDescriptorContext that provides a format context. 
    /// </param>
    /// <param name="sourceType">
    /// The Type being queried for support.
    /// </param>
    /// <returns>
    /// <c>true</c> if this converter can convert from the provided type, 
    /// <c>false</c> otherwise.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        // We can only handle strings, integral and floating types
        TypeCode tc = Type.GetTypeCode(sourceType);
        switch (tc)
        {
            case TypeCode.String:
            case TypeCode.Decimal:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks whether or not this class can convert to a given type.
    /// </summary>
    /// <param name="context">
    /// An ITypeDescriptorContext that provides a format context. 
    /// </param>
    /// <param name="destinationType">
    /// The Type being queried for support.
    /// </param>
    /// <returns>
    /// <c>true</c> if this converter can convert to the provided type, 
    /// <c>false</c> otherwise.
    /// </returns>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Attempts to convert to a DataGridLength from the given object.
    /// </summary>
    /// <param name="context">
    /// An ITypeDescriptorContext that provides a format context. 
    /// </param>
    /// <param name="culture">
    /// The CultureInfo to use for the conversion. 
    /// </param>
    /// <param name="value">
    /// The object to convert to a DataGridLength.
    /// </param>
    /// <returns>
    /// The DataGridLength that was constructed.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// A string is read case-insensitively as <c>Auto</c>, <c>SizeToCells</c> or <c>SizeToHeader</c>, or as a
    /// star weight followed by <c>*</c>, where a bare <c>*</c> means a weight of 1. Any other value is read as
    /// a number of pixels, and <c>double.NaN</c> gives <see cref="DataGridLength.Auto" />.
    /// </remarks>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        // GridLengthConverter in WPF throws a NotSupportedException on a null value as well.
        if (value == null)
        {
            throw DataGridError.DataGridLengthConverter.CannotConvertFrom("(null)");
        }

        if (value is string stringValue)
        {
            var trimmedValue = stringValue.AsSpan().Trim();

            if (trimmedValue.EndsWith(StarSuffix.AsSpan(), StringComparison.Ordinal))
            {
                var stringValueWithoutSuffix = trimmedValue[..^StarSuffix.Length];

                double starWeight;
                if (stringValueWithoutSuffix.IsEmpty)
                {
                    starWeight = 1;
                }
                else
                {
                    starWeight = double.Parse(stringValueWithoutSuffix,
                        NumberStyles.Float | NumberStyles.AllowThousands,
                        culture ?? CultureInfo.CurrentCulture);
                }

                return new DataGridLength(starWeight, DataGridLengthUnitType.Star);
            }

            for (int index = 0; index < ValueInvariantUnitStrings.Length; index++)
            {
                if (trimmedValue.Equals(ValueInvariantUnitStrings[index].AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return ValueInvariantDataGridLengths[index];
                }
            }
        }

        // Conversion from numeric type, WPF lets Convert exceptions bubble out here as well
        double doubleValue = Convert.ToDouble(value, culture ?? CultureInfo.CurrentCulture);
        if (double.IsNaN(doubleValue))
        {
            // WPF returns Auto in this case as well
            return DataGridLength.Auto;
        }
        return new DataGridLength(doubleValue);
    }

    /// <summary>
    /// Attempts to convert a DataGridLength instance to the given type.
    /// </summary>
    /// <param name="context">
    /// An ITypeDescriptorContext that provides a format context. 
    /// </param>
    /// <param name="culture">
    /// The CultureInfo to use for the conversion. 
    /// </param>
    /// <param name="value">
    /// The DataGridLength to convert.
    /// </param>
    /// <param name="destinationType">
    /// The type to which to convert the DataGridLength instance.
    /// </param>
    /// <returns>
    /// The object which was constructed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// An ArgumentNullException is thrown if the example object is null.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A NotSupportedException is thrown if the object is not null and is not a DataGridLength,
    /// or if the destinationType isn't one of the valid destination types.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == null)
        {
            throw new ArgumentNullException(nameof(destinationType));
        }
        if (destinationType != typeof(string))
        {
            throw DataGridError.DataGridLengthConverter.CannotConvertTo(destinationType.ToString());
        }
        DataGridLength? dataGridLength = value as DataGridLength?;
        if (!dataGridLength.HasValue)
        {
            throw DataGridError.DataGridLengthConverter.InvalidDataGridLength("value");
        }
        else
        {
            // Convert dataGridLength to a string
            switch (dataGridLength.Value.UnitType)
            {
                //  for Auto print out "Auto". value is always "1.0"
                case DataGridLengthUnitType.Auto:
                    return "Auto";

                case DataGridLengthUnitType.SizeToHeader:
                    return "SizeToHeader";

                case DataGridLengthUnitType.SizeToCells:
                    return "SizeToCells";

                //  Star has one special case when the value is 1.0.
                //  In this case the value part is dropped and only "*" is printed.
                case DataGridLengthUnitType.Star:
                    return (
                        MathUtils.AreClose(1.0, dataGridLength.Value.Value)
                            ? StarSuffix
                            : Convert.ToString(dataGridLength.Value.Value, culture ?? CultureInfo.CurrentCulture) + DataGridLengthConverter.StarSuffix);

                default:
                    return (Convert.ToString(dataGridLength.Value.Value, culture ?? CultureInfo.CurrentCulture));
            }
        }
    }
}
