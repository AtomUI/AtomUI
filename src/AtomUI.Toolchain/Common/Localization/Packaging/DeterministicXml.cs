using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class DeterministicXml
{
    internal static string Write(XDocument document)
    {
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
            document.Save(xmlWriter);
        }
        return textWriter.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        private static readonly Encoding s_utf8 = new UTF8Encoding(false);

        public override Encoding Encoding => s_utf8;
    }
}
