using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data context of <see cref="OtpLineEdit.SeparatorTemplate" />. The input creates one for each separator it
/// shows, so the template can tell the separators apart by position and still receives the separator content.
/// </summary>
public sealed class OtpLineEditSeparatorContext
{
    /// <summary>
    /// Creates the context of one separator.
    /// </summary>
    /// <param name="cellIndex">
    /// Zero-based index of the character cell after which the separator is drawn.
    /// </param>
    /// <param name="separatorIndex">
    /// Number of this separator, counted from 1.
    /// </param>
    /// <param name="content">
    /// Content the template presents; see <see cref="Content" />.
    /// </param>
    /// <param name="contentTemplate">
    /// Template that presents this context; see <see cref="ContentTemplate" />.
    /// </param>
    public OtpLineEditSeparatorContext(
        int cellIndex,
        int separatorIndex,
        object? content,
        IDataTemplate? contentTemplate)
    {
        CellIndex       = cellIndex;
        SeparatorIndex = separatorIndex;
        Content        = content;
        ContentTemplate = contentTemplate;
    }

    /// <summary>
    /// Zero-based index of the character cell after which this separator is drawn.
    /// </summary>
    public int CellIndex { get; }

    /// <summary>
    /// Number of this separator among the separators of the input, counted from 1.
    /// </summary>
    public int SeparatorIndex { get; }

    /// <summary>
    /// Content the template presents. The input passes its <see cref="OtpLineEdit.Separator" />.
    /// </summary>
    public object? Content { get; }

    /// <summary>
    /// Template that presents this context. The input passes its <see cref="OtpLineEdit.SeparatorTemplate" />.
    /// </summary>
    public IDataTemplate? ContentTemplate { get; }
}
