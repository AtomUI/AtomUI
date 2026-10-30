using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Localization.LocalizationGeneratorTestHost;

namespace AtomUI.Generator.Tests.Localization;

public class LanguageCatalogSourceGeneratorTests
{
    [Fact]
    public void Emits_The_Existing_Shape_Markup_Extension()
    {
        var execution = RunGenerator();

        execution.Result.Diagnostics.ShouldBeEmpty();
        var source = GetGeneratedSource(
            execution.Result,
            "TestApp.Localization.LoginLangResourceKind.LanguageCatalog.g.cs");
        source.ShouldBe(ExpectedCatalogSource + "\n");
    }

    [Fact]
    public void Emits_One_Module_Data_Constant_With_Compiled_BuiltIn_Bundles()
    {
        var execution = RunGenerator();

        execution.Result.Diagnostics.ShouldBeEmpty();
        var source = GetGeneratedSource(execution.Result, "GeneratedLanguageModuleRegistration.g.cs");
        source.ShouldContain("builder.AddModuleData(\"Test.Package\", Data);");
        var data = ReadModule(execution);
        var catalog = data.Catalogs.ShouldHaveSingleItem();
        catalog.Units.Select(static unit => unit.Key).ShouldBe(["ItemCount", "Title"]);
        catalog.Units.Select(static unit => unit.IsFormatted).ShouldBe([true, false]);
        data.Bundles.Select(static bundle => bundle.Language).ShouldBe(["en-US", "zh-CN", "zh-TW"]);
        data.Bundles[1].Values.ShouldBe(["项目 {0}", "打开 \"文件\" C:\\临时"]);
        source.ShouldNotContain("Assembly.GetTypes");
        source.ShouldNotContain("Type.GetFields");
        source.ShouldNotContain("Enum.GetNames");
        source.ShouldNotContain("GetCustomAttributes");
        source.ShouldNotContain("Activator.CreateInstance");
        source.ShouldNotContain("System.Xml");
        source.ShouldNotContain("System.IO.File");
    }

    [Fact]
    public void Generated_Catalog_Sources_Compile_Without_Errors()
    {
        var execution = RunGenerator();

        execution.Result.GeneratedSources.Length.ShouldBe(2);
        execution.Result.GeneratedSources.ShouldNotContain(source =>
            source.HintName.Contains("LinkedLanguageCatalogRegistration", StringComparison.Ordinal));
        execution.DriverDiagnostics.ShouldBeEmpty();
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    [Fact]
    public void Appends_Extension_When_The_Catalog_Name_Does_Not_End_In_Kind()
    {
        var execution = RunWithOutputCompilation(
            CatalogAndRuntimeSource.Replace("LoginLangResourceKind", "LoginMessages"),
            LanguageFile(
                "Localization/en-US.xlf",
                SourceXliff.Replace("LoginLangResourceKind", "LoginMessages")),
            LanguageFile(
                "Localization/zh-CN.xlf",
                TargetXliff("zh-CN", "标题", "项目 {0}")
                    .Replace("LoginLangResourceKind", "LoginMessages")));

        execution.Result.Diagnostics.ShouldBeEmpty();
        GetGeneratedSource(
                execution.Result,
                "TestApp.Localization.LoginMessages.LanguageCatalog.g.cs")
            .ShouldContain("public sealed class LoginMessagesExtension");
    }

    [Fact]
    public void Encodes_Keyword_Unit_Identifiers_Without_Runtime_Discovery()
    {
        var source = CatalogAndRuntimeSource.Replace("Title", "@class");
        var sourceXliff = SourceXliff.Replace("id=\"Title\"", "id=\"class\"");
        var targetXliff = TargetXliff("zh-CN", "类型", "项目 {0}")
            .Replace("id=\"Title\"", "id=\"class\"");
        var execution = RunWithOutputCompilation(
            source,
            LanguageFile("Localization/en-US.xlf", sourceXliff),
            LanguageFile("Localization/zh-CN.xlf", targetXliff));

        ReadModule(execution).Catalogs.ShouldHaveSingleItem().Units
            .Select(static unit => unit.Key).ShouldContain("class");
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    [Fact]
    public void Generates_Unique_Extensions_For_Same_Named_Nested_Catalogs()
    {
        var catalogNamespaceIndex = CatalogAndRuntimeSource.IndexOf(
            "namespace TestApp.Localization",
            StringComparison.Ordinal);
        var source = CatalogAndRuntimeSource.Substring(0, catalogNamespaceIndex) +
            """
            namespace TestApp.Localization
            {
                public static class OuterA
                {
                    [AtomUI.Localization.LanguageCatalog]
                    public enum CommonLangResourceKind
                    {
                        Title
                    }
                }

                public static class OuterB
                {
                    [AtomUI.Localization.LanguageCatalog]
                    public enum CommonLangResourceKind
                    {
                        Title
                    }
                }
            }
            """;
        var execution = RunWithOutputCompilation(
            source,
            LanguageFile(
                "Localization/OuterA/en-US.xlf",
                NestedSourceXliff("OuterA")),
            LanguageFile(
                "Localization/OuterB/en-US.xlf",
                NestedSourceXliff("OuterB")));

        execution.Result.Diagnostics.ShouldBeEmpty();
        GetGeneratedSource(
                execution.Result,
                "TestApp.Localization.OuterA+CommonLangResourceKind.LanguageCatalog.g.cs")
            .ShouldContain("public sealed class OuterA_CommonLangResourceExtension");
        GetGeneratedSource(
                execution.Result,
                "TestApp.Localization.OuterB+CommonLangResourceKind.LanguageCatalog.g.cs")
            .ShouldContain("public sealed class OuterB_CommonLangResourceExtension");
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    [Fact]
    public void Generates_Unique_Extensions_When_Nested_Type_Paths_Contain_Underscores()
    {
        var catalogNamespaceIndex = CatalogAndRuntimeSource.IndexOf(
            "namespace TestApp.Localization",
            StringComparison.Ordinal);
        var source = CatalogAndRuntimeSource.Substring(0, catalogNamespaceIndex) +
            """
            namespace TestApp.Localization
            {
                public static class A
                {
                    public static class B
                    {
                        [AtomUI.Localization.LanguageCatalog]
                        public enum CommonLangResourceKind
                        {
                            Title
                        }
                    }
                }

                public static class A_B
                {
                    [AtomUI.Localization.LanguageCatalog]
                    public enum CommonLangResourceKind
                    {
                            Title
                    }
                }
            }
            """;
        var execution = RunWithOutputCompilation(
            source,
            LanguageFile(
                "Localization/A-B/en-US.xlf",
                NestedSourceXliff("A+B")),
            LanguageFile(
                "Localization/A_B/en-US.xlf",
                NestedSourceXliff("A_B")));

        execution.Result.Diagnostics.ShouldBeEmpty();
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    [Fact]
    public void Generates_Unique_Extensions_For_Top_Level_Underscore_And_Nested_Catalogs()
    {
        var catalogNamespaceIndex = CatalogAndRuntimeSource.IndexOf(
            "namespace TestApp.Localization",
            StringComparison.Ordinal);
        var source = CatalogAndRuntimeSource.Substring(0, catalogNamespaceIndex) +
            """
            namespace TestApp.Localization
            {
                [AtomUI.Localization.LanguageCatalog]
                public enum Outer_CommonLangResourceKind
                {
                    Title
                }

                public static class Outer
                {
                    [AtomUI.Localization.LanguageCatalog]
                    public enum CommonLangResourceKind
                    {
                    Title
                    }
                }
            }
            """;
        var execution = RunWithOutputCompilation(
            source,
            LanguageFile(
                "Localization/Outer_Common/en-US.xlf",
                """
                <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
                  <file id="TestApp.Localization.Outer_CommonLangResourceKind">
                    <unit id="Title"><segment><source>Title</source></segment></unit>
                  </file>
                </xliff>
                """),
            LanguageFile(
                "Localization/Outer/en-US.xlf",
                NestedSourceXliff("Outer")));

        execution.Result.Diagnostics.ShouldBeEmpty();
        GetGeneratedSource(
                execution.Result,
                "TestApp.Localization.Outer_CommonLangResourceKind.LanguageCatalog.g.cs")
            .ShouldContain("public sealed class Outer_005FCommonLangResourceExtension");
        GetGeneratedSource(
                execution.Result,
                "TestApp.Localization.Outer+CommonLangResourceKind.LanguageCatalog.g.cs")
            .ShouldContain("public sealed class Outer_CommonLangResourceExtension");
        execution.OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                 .ShouldBeEmpty();
    }

    private static TestGeneratorExecution RunGenerator()
    {
        return RunWithOutputCompilation(
            CatalogAndRuntimeSource,
            LanguageFile("Localization/en-US.xlf", SourceXliff),
            LanguageFile("Localization/zh-CN.xlf", TargetXliff("zh-CN", "打开 \"文件\" C:\\临时", "项目 {0}")),
            LanguageFile("Localization/zh-TW.xlf", TargetXliff("zh-TW", "開啟 \"檔案\" C:\\暫存", "項目 {0}")));
    }

    private static string GetGeneratedSource(GeneratorRunResult result, string hintName)
    {
        return result.GeneratedSources
                     .Single(source => source.HintName == hintName)
                     .SourceText
                     .ToString();
    }

    private static TestAdditionalText LanguageFile(string path, string content)
    {
        return new TestAdditionalText(
            path,
            content,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_metadata.AdditionalFiles.AtomUILanguage"] = "true",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceKind"] = "ModuleBuiltIn",
                ["build_metadata.AdditionalFiles.AtomUILanguageSourceIdentity"] = "Test.Package"
            });
    }

    private static string TargetXliff(string language, string title, string itemCount)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US" trgLang="{{language}}">
              <file id="TestApp.Localization.LoginLangResourceKind">
                <unit id="ItemCount"><segment><source>Items {0}</source><target state="translated">{{itemCount}}</target></segment></unit>
                <unit id="Title"><segment><source>Open "file" C:\Temp</source><target state="translated">{{title}}</target></segment></unit>
              </file>
            </xliff>
            """;
    }

    private static string NestedSourceXliff(string containingType)
    {
        return $$"""
            <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
              <file id="TestApp.Localization.{{containingType}}+CommonLangResourceKind">
                <unit id="Title"><segment><source>Title</source></segment></unit>
              </file>
            </xliff>
            """;
    }

    private const string CatalogAndRuntimeSource = """
        #nullable enable
        namespace AtomUI.Localization
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            public sealed class LanguageCatalogAttribute : System.Attribute;

            public abstract class LanguageResourceExtension<TResourceKind>
                where TResourceKind : struct, System.Enum
            {
                protected LanguageResourceExtension() { }
                protected LanguageResourceExtension(TResourceKind kind) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class LanguageModuleManifestAttribute : System.Attribute
            {
                public LanguageModuleManifestAttribute(int version, string moduleId, string data) { }
            }

            public interface ILocalizationBuilder
            {
                void AddModuleData(string moduleId, string data);
                void AddCatalog(LanguageCatalogDescriptor descriptor);
                void AddTranslationBundle(TranslationBundleDescriptor descriptor);
            }

            public abstract class LanguageCatalogDescriptor { }

            public sealed class LanguageCatalogDescriptor<TResourceKind> : LanguageCatalogDescriptor
                where TResourceKind : struct, System.Enum
            {
                public LanguageCatalogDescriptor(
                    string catalogId,
                    System.Collections.Generic.IReadOnlyList<LanguageCatalogUnitDescriptor> units,
                    System.Func<TResourceKind, int> unitSlotResolver) { }
            }

            public sealed class LanguageCatalogUnitDescriptor
            {
                public LanguageCatalogUnitDescriptor(string key, bool isFormatted = false) { }
            }

            public readonly struct LanguageTag
            {
                public static LanguageTag Parse(string value) => default;
            }

            public enum TranslationSourceKind : byte
            {
                ModuleBuiltIn,
                StaticLanguagePack,
                ApplicationOverride
            }

            public sealed class TranslationBundleDescriptor
            {
                public TranslationBundleDescriptor(
                    string catalogId,
                    LanguageTag language,
                    TranslationSourceKind sourceKind,
                    string sourceIdentity,
                    System.Collections.Generic.IReadOnlyList<string?> values) { }
            }
        }

        namespace TestApp.Localization
        {
            [AtomUI.Localization.LanguageCatalog]
            public enum LoginLangResourceKind
            {
                ItemCount,
                Title
            }
        }
        """;

    private const string SourceXliff = """
        <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.1" srcLang="en-US">
          <file id="TestApp.Localization.LoginLangResourceKind">
            <unit id="ItemCount"><segment><source>Items {0}</source></segment></unit>
            <unit id="Title"><segment><source>Open "file" C:\Temp</source></segment></unit>
          </file>
        </xliff>
        """;

    private const string ExpectedCatalogSource = """
        // <auto-generated />
        #nullable enable

        namespace TestApp.Localization
        {
            public sealed class LoginLangResourceExtension
                : global::AtomUI.Localization.LanguageResourceExtension<global::TestApp.Localization.LoginLangResourceKind>
            {
                public LoginLangResourceExtension()
                {
                }

                public LoginLangResourceExtension(
                    global::TestApp.Localization.LoginLangResourceKind kind)
                    : base(kind)
                {
                }
            }
        }
        """;

    private static AtomUI.Localization.Data.LanguageModuleData ReadModule(TestGeneratorExecution execution)
    {
        var tree = execution.Result.GeneratedSources.Single(static source =>
            source.HintName == "GeneratedLanguageModuleRegistration.g.cs").SyntaxTree;
        var value = tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax>()
            .Single(static variable => variable.Identifier.ValueText == "Data").Initializer!.Value;
        var data = ((Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)value).Token.ValueText;
        return AtomUI.Localization.Data.LanguageModuleDataCodec.Read("Test.Package", data);
    }
}
