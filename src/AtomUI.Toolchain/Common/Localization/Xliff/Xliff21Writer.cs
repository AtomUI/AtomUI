using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class Xliff21Writer
{
    private const string XliffNamespace = "urn:oasis:names:tc:xliff:document:2.0";

    internal static string Write(XliffDocumentModel document)
    {
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        XNamespace ns = XliffNamespace;
        var root = new XElement(
            ns + "xliff",
            new XAttribute("version", "2.1"),
            new XAttribute("srcLang", document.SourceLanguage));
        if (document.TargetLanguage is not null)
        {
            root.Add(new XAttribute("trgLang", document.TargetLanguage));
        }

        var file = new XElement(ns + "file", new XAttribute("id", document.File.Id));
        foreach (var unit in document.File.Units.OrderBy(static unit => unit.Key, StringComparer.Ordinal))
        {
            file.Add(CreateUnit(ns, unit));
        }
        root.Add(file);

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = false
        };
        using var textWriter = new Utf8StringWriter();
        using (var xmlWriter = XmlWriter.Create(textWriter, settings))
        {
            xml.Save(xmlWriter);
        }
        return string.Concat(textWriter.ToString(), "\n");
    }

    private static XElement CreateUnit(XNamespace ns, XliffUnitModel unit)
    {
        var element = new XElement(
            ns + "unit",
            new XAttribute("id", unit.Key));
        if (unit.IsObsolete)
        {
            element.Add(new XAttribute("translate", "no"));
        }

        if (unit.Notes.Count > 0)
        {
            element.Add(new XElement(
                ns + "notes",
                unit.Notes.Select(note => new XElement(ns + "note", note))));
        }

        var segment = new XElement(ns + "segment", new XElement(ns + "source", unit.Source));
        if (unit.Target is not null || unit.TargetState is not null || unit.TargetSubState is not null)
        {
            var target = new XElement(ns + "target", unit.Target ?? string.Empty);
            if (unit.TargetState is not null)
            {
                target.Add(new XAttribute("state", unit.TargetState));
            }
            if (unit.TargetSubState is not null)
            {
                target.Add(new XAttribute("subState", unit.TargetSubState));
            }
            segment.Add(target);
        }
        element.Add(segment);
        return element;
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        private static readonly Encoding s_utf8 = new UTF8Encoding(false);

        public override Encoding Encoding => s_utf8;
    }
}
