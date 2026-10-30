using System.Xml;
using System.Xml.Linq;

namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class Xliff21Parser
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:2.0";

    internal static XliffParseResult Parse(string content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        XDocument xml;
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            using var textReader = new StringReader(content);
            using var xmlReader = XmlReader.Create(textReader, settings);
            xml = XDocument.Load(xmlReader, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        }
        catch (XmlException exception)
        {
            var message = exception.Message.IndexOf("DTD", StringComparison.OrdinalIgnoreCase) >= 0
                ? "DTD declarations and external entities are not allowed"
                : $"malformed XML: {exception.Message}";
            return Invalid(message, exception.LineNumber, exception.LinePosition);
        }

        var errors = new List<XliffParseError>();
        var root = xml.Root;
        if (root is null)
        {
            return Invalid("the document has no xliff root element", 1, 1);
        }

        if (root.Name != XName.Get("xliff", XliffNamespace))
        {
            errors.Add(Error(root, $"the root must use the '{XliffNamespace}' XLIFF namespace"));
        }
        if (!string.Equals((string?)root.Attribute("version"), "2.1", StringComparison.Ordinal))
        {
            errors.Add(Error(root, "version must be exactly '2.1'"));
        }

        var sourceLanguage = ParseLanguage(root, "srcLang", required: true, errors);
        if (sourceLanguage is not null && sourceLanguage != "en-US")
        {
            errors.Add(Error((XObject?)root.Attribute("srcLang") ?? root, "srcLang must be exactly 'en-US'"));
        }
        var targetLanguage = ParseLanguage(root, "trgLang", required: false, errors);

        XNamespace ns = XliffNamespace;
        var files = root.Elements(ns + "file").ToArray();
        if (files.Length != 1)
        {
            errors.Add(Error(root, "the document must contain exactly one file element"));
            return new XliffParseResult(null, errors);
        }

        var fileElement = files[0];
        var fileId = ((string?)fileElement.Attribute("id"))?.Trim();
        if (string.IsNullOrEmpty(fileId))
        {
            errors.Add(Error(fileElement, "the file id is required"));
        }

        var unitElements = fileElement.Elements(ns + "unit").ToArray();
        if (unitElements.Length == 0)
        {
            errors.Add(Error(fileElement, "the file must contain at least one unit"));
        }

        var units = new List<XliffUnitModel>(unitElements.Length);
        var unitKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var unitElement in unitElements)
        {
            ParseUnit(unitElement, ns, unitKeys, units, errors);
        }

        if (errors.Count > 0 || sourceLanguage is null || string.IsNullOrEmpty(fileId))
        {
            return new XliffParseResult(null, errors);
        }

        return new XliffParseResult(
            new XliffDocumentModel(
                sourceLanguage,
                targetLanguage,
                new XliffFileModel(fileId!, units.OrderBy(static unit => unit.Key, StringComparer.Ordinal).ToArray())),
            Array.Empty<XliffParseError>());
    }

    private static void ParseUnit(
        XElement unitElement,
        XNamespace ns,
        HashSet<string> unitKeys,
        List<XliffUnitModel> units,
        List<XliffParseError> errors)
    {
        var keyText = ((string?)unitElement.Attribute("id"))?.Trim();
        if (string.IsNullOrEmpty(keyText))
        {
            errors.Add(Error((XObject?)unitElement.Attribute("id") ?? unitElement, "unit id must be a non-empty Key"));
            return;
        }
        var key = keyText!;
        if (!unitKeys.Add(key))
        {
            errors.Add(Error((XObject?)unitElement.Attribute("id") ?? unitElement, $"unit id '{key}' is duplicated"));
            return;
        }

        var name = ((string?)unitElement.Attribute("name"))?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = null;
        }

        var translate = ((string?)unitElement.Attribute("translate"))?.Trim();
        if (translate is not null and not ("yes" or "no"))
        {
            errors.Add(Error(
                (XObject?)unitElement.Attribute("translate") ?? unitElement,
                "unit translate must be yes or no"));
        }
        var isObsolete = translate == "no";

        var segments = unitElement.Elements(ns + "segment").ToArray();
        if (segments.Length != 1)
        {
            errors.Add(Error(unitElement, "each unit must contain exactly one segment"));
            return;
        }

        var segment = segments[0];
        var sourceElements = segment.Elements(ns + "source").ToArray();
        var targetElements = segment.Elements(ns + "target").ToArray();
        if (sourceElements.Length != 1 || targetElements.Length > 1)
        {
            errors.Add(Error(segment, "a segment requires exactly one source and at most one target"));
            return;
        }

        var source = ReadPlainText(sourceElements[0], "source", errors);
        var target = targetElements.Length == 0
            ? null
            : ReadPlainText(targetElements[0], "target", errors);
        string? targetState = null;
        string? targetSubState = null;
        if (targetElements.Length == 1)
        {
            targetState = ((string?)targetElements[0].Attribute("state"))?.Trim();
            targetSubState = ((string?)targetElements[0].Attribute("subState"))?.Trim();
            if (targetSubState?.Length == 0)
            {
                targetSubState = null;
            }
            if (!IsKnownTargetState(targetState))
            {
                errors.Add(Error(
                    (XObject?)targetElements[0].Attribute("state") ?? targetElements[0],
                    "target state must be initial, translated, reviewed, or final"));
            }
        }

        var sourceIndexes = Array.Empty<int>();
        var hasPublishableTarget = target is not null && !(target.Length == 0 && targetState == "initial");
        var usesCompositeFormat = CompositeFormatContractParser.ContainsPlaceholderCandidate(source) ||
                                  hasPublishableTarget && CompositeFormatContractParser.ContainsPlaceholderCandidate(target!);
        if (usesCompositeFormat &&
            !CompositeFormatContractParser.TryParse(source, out sourceIndexes, out var sourceError))
        {
            errors.Add(Error(sourceElements[0], $"source CompositeFormat is invalid: {sourceError}"));
        }
        if (usesCompositeFormat && hasPublishableTarget)
        {
            if (!CompositeFormatContractParser.TryParse(target!, out var targetIndexes, out var targetError))
            {
                errors.Add(Error(targetElements[0], $"target CompositeFormat is invalid: {targetError}"));
            }
            else if (!sourceIndexes.SequenceEqual(targetIndexes))
            {
                errors.Add(Error(targetElements[0], "source and target placeholder indexes must match"));
            }
        }

        if (errors.Count > 0)
        {
            return;
        }

        var notes = unitElement.Elements(ns + "notes")
                               .SelectMany(notesElement => notesElement.Elements(ns + "note"))
                               .Select(static note => note.Value)
                               .ToArray();
        var (line, column) = GetLineInfo(unitElement);
        units.Add(new XliffUnitModel(
            key,
            name,
            source,
            target,
            targetState,
            targetSubState,
            notes,
            sourceIndexes,
            line,
            column,
            isObsolete));
    }

    private static string ReadPlainText(
        XElement element,
        string elementName,
        List<XliffParseError> errors)
    {
        if (element.Elements().Any())
        {
            errors.Add(Error(element, $"{elementName} must contain plain text without inline XML elements"));
        }
        return element.Value;
    }

    private static string? ParseLanguage(
        XElement root,
        string attributeName,
        bool required,
        List<XliffParseError> errors)
    {
        var attribute = root.Attribute(attributeName);
        var value = attribute?.Value.Trim();
        if (string.IsNullOrEmpty(value))
        {
            if (required)
            {
                errors.Add(Error((XObject?)attribute ?? root, $"{attributeName} is required"));
            }
            return null;
        }

        if (!Bcp47LanguageTagParser.TryParse(value!, out var canonical))
        {
            errors.Add(Error(attribute!, $"{attributeName} must be a valid BCP 47 language tag"));
            return null;
        }
        if (!string.Equals(value, canonical, StringComparison.Ordinal))
        {
            errors.Add(Error(attribute!, $"{attributeName} must use canonical BCP 47 form '{canonical}'"));
            return null;
        }
        return canonical;
    }

    private static bool IsKnownTargetState(string? state)
    {
        return state is "initial" or "translated" or "reviewed" or "final";
    }

    private static XliffParseResult Invalid(string message, int line, int column)
    {
        return new XliffParseResult(null, [new XliffParseError(message, line, column)]);
    }

    private static XliffParseError Error(XObject source, string message)
    {
        var (line, column) = GetLineInfo(source);
        return new XliffParseError(message, line, column);
    }

    private static (int Line, int Column) GetLineInfo(XObject source)
    {
        if (source is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
        {
            return (lineInfo.LineNumber, lineInfo.LinePosition);
        }

        return (1, 1);
    }
}
