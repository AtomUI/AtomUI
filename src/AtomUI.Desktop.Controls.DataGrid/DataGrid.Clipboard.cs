// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Text;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

// 剪贴板：选中内容的复制格式化与写入
public partial class DataGrid
{
    /// <summary>
    /// This method formats a row (specified by a DataGridRowClipboardEventArgs) into
    /// the clipboard text builder when the DataGrid is copying its contents.
    /// </summary>
    /// <param name="text">The destination clipboard text builder.</param>
    /// <param name="e">DataGridRowClipboardEventArgs.</param>
    private static void AppendClipboardContent(StringBuilder text, DataGridRowClipboardEventArgs e)
    {
        var clipboardRowContent = e.ClipboardRowContent;
        var numberOfItem        = clipboardRowContent.Count;
        for (int cellIndex = 0; cellIndex < numberOfItem; cellIndex++)
        {
            AppendEscapedClipboardCellContent(text, clipboardRowContent[cellIndex].Content);
            if (cellIndex < numberOfItem - 1)
            {
                text.Append('\t');
            }
            else
            {
                text.Append('\r');
                text.Append('\n');
            }
        }
    }

    private static void AppendEscapedClipboardCellContent(StringBuilder text, object? content)
    {
        text.Append('"');
        var cellContent = content?.ToString();
        if (!string.IsNullOrEmpty(cellContent))
        {
            var segmentStart = 0;
            for (var charIndex = 0; charIndex < cellContent.Length; charIndex++)
            {
                if (cellContent[charIndex] != '"')
                {
                    continue;
                }

                text.Append(cellContent, segmentStart, charIndex - segmentStart);
                text.Append("\"\"");
                segmentStart = charIndex + 1;
            }

            if (segmentStart < cellContent.Length)
            {
                text.Append(cellContent, segmentStart, cellContent.Length - segmentStart);
            }
        }
        text.Append('"');
    }

    /// <summary>
    /// Handles the case where a 'Copy' key ('C' or 'Insert') has been pressed.  If pressed in combination with
    /// the control key, and the necessary prerequisites are met, the DataGrid will copy its contents
    /// to the Clipboard as text.
    /// </summary>
    /// <returns>Whether or not the DataGrid handled the key press.</returns>
    private bool ProcessCopyKey(KeyModifiers modifiers)
    {
        KeyboardHelper.GetMetaKeyState(this, modifiers, out bool ctrl, out bool shift, out bool alt);

        var selectedItems = GetLoadedSelectionObjects();
        if (ctrl && !shift && !alt && ClipboardCopyMode != DataGridClipboardCopyMode.None && selectedItems.Count > 0)
        {
            var textBuilder = new StringBuilder();

            if (ClipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader)
            {
                DataGridRowClipboardEventArgs headerArgs = new DataGridRowClipboardEventArgs(
                    null,
                    true,
                    ColumnsInternal.VisibleColumnCount);
                int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                    if (!column.IsVisible)
                    {
                        continue;
                    }

                    headerArgs.ClipboardRowContent.Add(new DataGridClipboardCellContent(null, column, column.Header));
                }

                OnCopyingRowClipboardContent(headerArgs);
                AppendClipboardContent(textBuilder, headerArgs);
            }

            for (int index = 0; index < selectedItems.Count; index++)
            {
                var item = selectedItems[index];
                Debug.Assert(item != null);
                DataGridRowClipboardEventArgs itemArgs = new DataGridRowClipboardEventArgs(
                    item,
                    false,
                    ColumnsInternal.VisibleColumnCount);
                int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                    if (!column.IsVisible)
                    {
                        continue;
                    }

                    object? content = column.GetCellValue(item, column.ClipboardContentBinding);
                    itemArgs.ClipboardRowContent.Add(new DataGridClipboardCellContent(item, column, content));
                }

                OnCopyingRowClipboardContent(itemArgs);
                AppendClipboardContent(textBuilder, itemArgs);
            }

            string text = textBuilder.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                _ = CopyToClipboardAsync(text);
                return true;
            }
        }

        return false;
    }

    private async Task CopyToClipboardAsync(string text)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            if (clipboard != null && this.IsAttachedToVisualTree())
            {
                await clipboard.SetTextAsync(text);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CopyToClipboardAsync: {ex.Message}");
        }
    }
}
