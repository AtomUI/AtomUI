using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace AtomUI.Generator.Tests.Localization;

public class LanguageTagsGeneratorTests
{
    private const string Header = "# atomui-language-tags-schema: 1";

    [Fact]
    public void Accepts_Valid_Language_Data()
    {
        var result = RunGenerator($$"""
            {{Header}}
            EnUS	en-US	en-US	English (United States)	LeftToRight
            ArSA	ar-SA	ar-SA	العربية (المملكة العربية السعودية)	RightToLeft
            """);

        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Generates_Strongly_Typed_Language_Tags_And_Definitions()
    {
        var execution = ExecuteGenerator($$"""
            {{Header}}
            EnUS	en-US	en-US	English (United States)	LeftToRight
            ArSA	ar-SA	ar-SA	العربية (المملكة العربية السعودية)	RightToLeft
            """);

        execution.Result.Diagnostics.ShouldBeEmpty();
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();

        var languageTags = GetGeneratedSource(execution.Result, "LanguageTags.g.cs");
        languageTags.ShouldContain("public static class LanguageTags");
        languageTags.ShouldContain(
            "public static global::AtomUI.Localization.LanguageTag EnUS => " +
            "global::AtomUI.Localization.LanguageTag.FromCanonical(\"en-US\");");
        languageTags.ShouldContain(
            "public static global::AtomUI.Localization.LanguageTag ArSA => " +
            "global::AtomUI.Localization.LanguageTag.FromCanonical(\"ar-SA\");");

        var definitions = GetGeneratedSource(
            execution.Result,
            "GeneratedStandardLanguageDefinitions.g.cs");
        definitions.ShouldContain("internal static class GeneratedStandardLanguageDefinitions");
        definitions.ShouldContain("case \"ar-SA\":");
        definitions.ShouldContain("global::AtomUI.Localization.LanguageTags.ArSA");
        definitions.ShouldContain("global::System.Globalization.CultureInfo.GetCultureInfo(\"ar-SA\")");
        definitions.ShouldContain("global::AtomUI.Localization.LanguageTextDirection.RightToLeft");
        definitions.ShouldContain("العربية (المملكة العربية السعودية)");
        definitions.ShouldNotContain("CultureInfo.GetCultures");
        definitions.ShouldNotContain("Assembly.GetTypes");
        definitions.ShouldNotContain("Enum.GetNames");
        definitions.ShouldNotContain("GetFields");
    }

    [Fact]
    public void Generates_Byte_Identical_Output_For_Reordered_Records()
    {
        var first = RunGenerator($$"""
            {{Header}}
            EnUS	en-US	en-US	English (United States)	LeftToRight
            ArSA	ar-SA	ar-SA	Arabic (Saudi Arabia)	RightToLeft
            """);
        var second = RunGenerator($$"""
            {{Header}}
            ArSA	ar-SA	ar-SA	Arabic (Saudi Arabia)	RightToLeft
            EnUS	en-US	en-US	English (United States)	LeftToRight
            """);

        first.GeneratedSources.Select(static source => source.SourceText.ToString())
             .ShouldBe(second.GeneratedSources.Select(static source => source.SourceText.ToString()));
    }

    [Theory]
    [InlineData(
        "EnUS\ten-US\ten-US\tEnglish (United States)",
        "exactly five tab-separated columns")]
    [InlineData(
        "en-US\ten-US\ten-US\tEnglish (United States)\tLeftToRight",
        "valid C# identifier")]
    [InlineData(
        "EnUS\ten_US\ten-US\tEnglish (United States)\tLeftToRight",
        "valid BCP 47")]
    [InlineData(
        "EnUS\tEN-us\ten-US\tEnglish (United States)\tLeftToRight",
        "canonical BCP 47")]
    [InlineData(
        "EnUS\ten-US\ten-US\tEnglish (United States)\tDiagonal",
        "LeftToRight or RightToLeft")]
    public void Reports_Malformed_Data_Record(string record, string messageFragment)
    {
        var result = RunGenerator($$"""
            {{Header}}
            # source metadata
            {{record}}
            """);

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUILOC001");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldContain(messageFragment);
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(2);
        result.GeneratedSources.ShouldBeEmpty();
    }

    [Fact]
    public void Reports_Missing_Schema_Header()
    {
        var result = RunGenerator(
            "EnUS\ten-US\ten-US\tEnglish (United States)\tLeftToRight");

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUILOC001");
        diagnostic.GetMessage().ShouldContain("schema header");
        result.GeneratedSources.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(
        "EnUS\ten-US\ten-US\tEnglish (United States)\tLeftToRight",
        "EnUS\ten-GB\ten-GB\tEnglish (United Kingdom)\tLeftToRight",
        "identifier 'EnUS'")]
    [InlineData(
        "EnglishUS\ten-US\ten-US\tEnglish (United States)\tLeftToRight",
        "EnUS\ten-US\ten-US\tEnglish (United States)\tLeftToRight",
        "tag 'en-US'")]
    public void Reports_Duplicate_Identifier_Or_Tag(
        string firstRecord,
        string secondRecord,
        string messageFragment)
    {
        var result = RunGenerator($$"""
            {{Header}}
            {{firstRecord}}
            {{secondRecord}}
            """);

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUILOC002");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldContain(messageFragment);
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(2);
        result.GeneratedSources.ShouldBeEmpty();
    }

    [Fact]
    public void Ignores_Unmarked_Additional_Files()
    {
        var result = RunGenerator(
            "not language data",
            isLanguageTagsData: false);

        result.Diagnostics.ShouldBeEmpty();
        result.GeneratedSources.ShouldBeEmpty();
    }

    private static GeneratorRunResult RunGenerator(string data, bool isLanguageTagsData = true)
    {
        return ExecuteGenerator(data, isLanguageTagsData).Result;
    }

    private static (GeneratorRunResult Result, Compilation OutputCompilation) ExecuteGenerator(
        string data,
        bool isLanguageTagsData = true)
    {
        var compilation = CreateCompilation();
        var additionalText = new InMemoryAdditionalText("LanguageData/language-tags.tsv", data);
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            additionalText.Path,
            isLanguageTagsData);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new LanguageTagsGenerator().AsSourceGenerator()],
            [additionalText],
            (CSharpParseOptions)compilation.SyntaxTrees[0].Options,
            optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _,
            TestContext.Current.CancellationToken);
        return (driver.GetRunResult().Results.ShouldHaveSingleItem(), outputCompilation);
    }

    private static string GetGeneratedSource(GeneratorRunResult result, string hintName)
    {
        return result.GeneratedSources
                     .Single(source => source.HintName == hintName)
                     .SourceText
                     .ToString();
    }

    private static CSharpCompilation CreateCompilation()
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                         .Split(Path.PathSeparator)
                         .Select(static path => MetadataReference.CreateFromFile(path))
                         .Cast<MetadataReference>()
                         .ToImmutableArray();

        return CSharpCompilation.Create(
            "LanguageTagsGeneratorTests",
            [CSharpSyntaxTree.ParseText("""
                namespace AtomUI.Localization;

                public readonly struct LanguageTag
                {
                    public string Value => string.Empty;
                    public static LanguageTag Parse(string value) => default;
                    internal static LanguageTag FromCanonical(string value) => default;
                }

                public enum LanguageTextDirection : byte
                {
                    LeftToRight,
                    RightToLeft
                }

                public sealed class LanguageDefinition
                {
                    public LanguageDefinition(
                        LanguageTag tag,
                        System.Globalization.CultureInfo culture,
                        string nativeName,
                        LanguageTextDirection direction)
                    {
                    }
                }
                """)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = SourceText.From(text, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions s_empty = new TestAnalyzerConfigOptions(false);
        private readonly string _path;
        private readonly AnalyzerConfigOptions _fileOptions;

        public TestAnalyzerConfigOptionsProvider(string path, bool isLanguageTagsData)
        {
            _path = path;
            _fileOptions = new TestAnalyzerConfigOptions(isLanguageTagsData);
        }

        public override AnalyzerConfigOptions GlobalOptions => s_empty;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return s_empty;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return string.Equals(textFile.Path, _path, StringComparison.Ordinal)
                ? _fileOptions
                : s_empty;
        }
    }

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly bool _isLanguageTagsData;

        public TestAnalyzerConfigOptions(bool isLanguageTagsData)
        {
            _isLanguageTagsData = isLanguageTagsData;
        }

        public override bool TryGetValue(string key, out string value)
        {
            if (_isLanguageTagsData &&
                key == "build_metadata.AdditionalFiles.AtomUILanguageTagsData")
            {
                value = "true";
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
