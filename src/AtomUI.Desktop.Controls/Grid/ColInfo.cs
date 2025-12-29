using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

[TypeConverter(typeof(GridColSizeConverter))]
public record ColInfo : GridColSize;
