namespace AtomUI.Build.Tasks.LocalizationBuild;

internal sealed class XliffDocumentModel
{
    internal XliffDocumentModel(
        string sourceLanguage,
        string? targetLanguage,
        XliffFileModel file)
    {
        SourceLanguage = sourceLanguage;
        TargetLanguage = targetLanguage;
        File = file;
    }

    internal string SourceLanguage { get; }

    internal string? TargetLanguage { get; }

    internal XliffFileModel File { get; }
}

internal sealed class XliffFileModel
{
    internal XliffFileModel(string id, IReadOnlyList<XliffUnitModel> units)
    {
        Id = id;
        Units = units;
    }

    internal string Id { get; }

    internal IReadOnlyList<XliffUnitModel> Units { get; }
}

internal sealed class XliffUnitModel
{
    internal XliffUnitModel(
        string key,
        string? name,
        string source,
        string? target,
        string? targetState,
        string? targetSubState,
        IReadOnlyList<string> notes,
        IReadOnlyList<int> placeholderIndexes,
        int line,
        int column,
        bool isObsolete = false)
    {
        Key = key;
        Name = name;
        Source = source;
        Target = target;
        TargetState = targetState;
        TargetSubState = targetSubState;
        Notes = notes;
        PlaceholderIndexes = placeholderIndexes;
        Line = line;
        Column = column;
        IsObsolete = isObsolete;
    }

    internal string Key { get; }

    internal string? Name { get; }

    internal string Source { get; }

    internal string? Target { get; }

    internal string? TargetState { get; }

    internal string? TargetSubState { get; }

    internal IReadOnlyList<string> Notes { get; }

    internal IReadOnlyList<int> PlaceholderIndexes { get; }

    internal int Line { get; }

    internal int Column { get; }

    internal bool IsObsolete { get; }
}

internal sealed class XliffParseResult
{
    internal XliffParseResult(
        XliffDocumentModel? document,
        IReadOnlyList<XliffParseError> errors)
    {
        Document = document;
        Errors = errors;
    }

    internal XliffDocumentModel? Document { get; }

    internal IReadOnlyList<XliffParseError> Errors { get; }
}

internal sealed class XliffParseError
{
    internal XliffParseError(string message, int line, int column)
    {
        Message = message;
        Line = line;
        Column = column;
    }

    internal string Message { get; }

    internal int Line { get; }

    internal int Column { get; }
}
