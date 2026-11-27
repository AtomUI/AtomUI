using System.ComponentModel;

namespace AtomUI.Controls;

/// <summary>
/// A <see cref="GridColSize" /> with no added members. The inline syntax <c>{ColInfo Order=2}</c> is resolved to
/// <see cref="ColInfoExtension" />, which returns a <see cref="GridColSize" /> and never this record.
/// </summary>
[TypeConverter(typeof(GridColSizeConverter))]
public record ColInfo : GridColSize;
