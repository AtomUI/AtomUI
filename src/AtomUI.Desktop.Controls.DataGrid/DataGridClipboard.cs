// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines modes that indicates how DataGrid content is copied to the Clipboard. 
/// </summary>
public enum DataGridClipboardCopyMode
{
    /// <summary>
    /// Disable the DataGrid's ability to copy selected items as text.
    /// </summary>
    None,

    /// <summary>
    /// Enable the DataGrid's ability to copy selected items as text, but do not include
    /// the column header content as the first line in the text that gets copied to the Clipboard.
    /// </summary>
    ExcludeHeader,

    /// <summary>
    /// Enable the DataGrid's ability to copy selected items as text, and include
    /// the column header content as the first line in the text that gets copied to the Clipboard.
    /// </summary>
    IncludeHeader
}

/// <summary>
/// One cell of a DataGrid row as it is handed to a clipboard copy: its content, its column and the item
/// of its row.
/// </summary>
public struct DataGridClipboardCellContent
{
    private DataGridColumn _column;
    private object? _content;
    private object? _item;

    /// <summary>
    /// Creates a DataGridClipboardCellContent with the content of a DataGrid cell, its column and its row item.
    /// </summary>
    /// <param name="item">
    /// DataGrid row item containing the cell.
    /// </param>
    /// <param name="column">
    /// DataGridColumn containing the cell.
    /// </param>
    /// <param name="content">
    /// DataGrid cell value.
    /// </param>
    public DataGridClipboardCellContent(object? item, DataGridColumn column, object? content)
    {
        _item    = item;
        _column  = column;
        _content = content;
    }

    /// <summary>
    /// DataGridColumn containing the cell.
    /// </summary>
    public DataGridColumn Column => _column;

    /// <summary>
    /// Cell content.
    /// </summary>
    public object? Content => _content;

    /// <summary>
    /// DataGrid row item containing the cell.
    /// </summary>
    public object? Item => _item;

    /// <summary>
    /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
    /// </summary>
    /// <param name="obj">
    /// DataGridClipboardCellContent to compare.
    /// </param>
    /// <returns>
    /// True iff this and obj are equal.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if(obj is DataGridClipboardCellContent content)
        {
            return (((_column == content._column) && (_content == content._content)) && (_item == content._item));
        }
        return false;
    }

    /// <summary>
    /// Returns a deterministic hash code.
    /// </summary>
    /// <returns>
    /// Hash value.
    /// </returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_column);
        hash.Add(_content);
        hash.Add(_item);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
    /// </summary>
    /// <param name="clipboardCellContent1">
    /// The first DataGridClipboardCellContent.
    /// </param>
    /// <param name="clipboardCellContent2">
    /// The second DataGridClipboardCellContent.
    /// </param>
    /// <returns>
    /// True iff clipboardCellContent1 and clipboardCellContent2 are equal.
    /// </returns>
    public static bool operator ==(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
    {
        return (((clipboardCellContent1._column == clipboardCellContent2._column) && (clipboardCellContent1._content == clipboardCellContent2._content)) && (clipboardCellContent1._item == clipboardCellContent2._item));
    }

    /// <summary>
    /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
    /// </summary>
    /// <param name="clipboardCellContent1">
    /// The first DataGridClipboardCellContent.
    /// </param>
    /// <param name="clipboardCellContent2">
    /// The second DataGridClipboardCellContent.
    /// </param>
    /// <returns>
    /// True iff clipboardCellContent1 and clipboardCellContent2 are NOT equal.
    /// </returns>
    public static bool operator !=(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
    {
        if ((clipboardCellContent1._column == clipboardCellContent2._column) && (clipboardCellContent1._content == clipboardCellContent2._content))
        {
            return (clipboardCellContent1._item != clipboardCellContent2._item);
        }
        return true;
    }

}

/// <summary>
/// This class encapsulates a selected row's information necessary for the CopyingRowClipboardContent event.
/// </summary>
public class DataGridRowClipboardEventArgs : EventArgs
{

    private List<DataGridClipboardCellContent>? _clipboardRowContent;
    private readonly int _clipboardRowContentCapacity;
    private bool _isColumnHeadersRow;
    private object? _item;

    /// <summary>
    /// Creates a DataGridRowClipboardEventArgs object and initializes the properties.
    /// </summary>
    /// <param name="item">
    /// The row's associated data item.
    /// </param>
    /// <param name="isColumnHeadersRow">
    /// Whether or not this EventArgs is for the column headers.
    /// </param>
    /// <param name="clipboardRowContentCapacity">
    /// Initial capacity reserved for the clipboard content of the row; a value below 1 is taken as 0.
    /// </param>
    internal DataGridRowClipboardEventArgs(object? item, bool isColumnHeadersRow, int clipboardRowContentCapacity = 0)
    {
        _isColumnHeadersRow          = isColumnHeadersRow;
        _item                        = item;
        _clipboardRowContentCapacity = clipboardRowContentCapacity > 0
            ? clipboardRowContentCapacity
            : 0;
    }

    /// <summary>
    /// This list should be used to modify, add or remove a cell content before it gets stored into the clipboard.
    /// </summary>
    public List<DataGridClipboardCellContent> ClipboardRowContent
    {
        get
        {
            if (_clipboardRowContent == null)
            {
                _clipboardRowContent = _clipboardRowContentCapacity > 0
                    ? new List<DataGridClipboardCellContent>(_clipboardRowContentCapacity)
                    : new List<DataGridClipboardCellContent>();
            }
            return _clipboardRowContent;
        }
    }

    /// <summary>
    /// This property is true when the ClipboardRowContent represents column headers, in which case the Item is null.
    /// </summary>
    public bool IsColumnHeadersRow => _isColumnHeadersRow;

    /// <summary>
    /// DataGrid row item used for preparing the ClipboardRowContent.
    /// </summary>
    public object? Item => _item;

}
