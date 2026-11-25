using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Theme.Definitions;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Reads every complete theme XML example of the theming documents with the real <see cref="ThemeDocumentReader"/>,
/// so that a documented example cannot drift away from the v2 XSD and the reader.
/// </summary>
public class ThemeDocumentationExampleTests
{
    private const string Namespace = "https://atomui.net/schemas/theme/v2";

    private static readonly string[] Documents =
    [
        "docs/reference/theming/theme-definition-xml-v2.md",
        "docs/guides/theming/writing-design-languages.md",
        "docs/guides/theming/customization.md",
        "docs/guides/theming/design-language-packages.md",
        "docs/architecture/systems/theming/runtime.md"
    ];

    private static readonly Regex XmlBlock = new(
        "```xml\\r?\\n(?<body>.*?)```",
        RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex ThemeRoot = new(
        "<Theme\\s",
        RegexOptions.CultureInvariant);

    public static TheoryData<string> DocumentPaths()
    {
        var data = new TheoryData<string>();
        foreach (var document in Documents)
        {
            data.Add(document);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(DocumentPaths))]
    public void Theme_Examples_In_The_Document_Are_Read_Without_Diagnostics(string document)
    {
        var path = Path.Combine(RepositoryRoot(), document);
        File.Exists(path).ShouldBeTrue(path);
        var examples = new List<(int Index, string Xml)>();
        var index = 0;
        foreach (Match match in XmlBlock.Matches(File.ReadAllText(path)))
        {
            var body = match.Groups["body"].Value;
            // Only complete theme documents are readable; fragments such as a bare <When> element are skipped.
            if (ThemeRoot.IsMatch(body) && body.Contains(Namespace, StringComparison.Ordinal))
            {
                examples.Add((index, body));
            }
            index++;
        }

        examples.ShouldNotBeEmpty($"{document} has no complete theme example");
        foreach (var (exampleIndex, xml) in examples)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var result = ThemeDocumentReader.Read(stream, $"{document}#example{exampleIndex}");

            result.Success.ShouldBeTrue($"{document} example {exampleIndex}: {result.DiagnosticsText()}");
            result.Diagnostics.ShouldBeEmpty($"{document} example {exampleIndex}");
        }
    }

    private static string RepositoryRoot([CallerFilePath] string sourceFile = "")
    {
        // tests/AtomUI.Core.Tests/Theme/<this file>
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, "..", "..", ".."));
    }
}
