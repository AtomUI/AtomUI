using AtomUI.Generator.DesignLanguages;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.DesignLanguages.DesignLanguageTestHost;

namespace AtomUI.Generator.Tests.DesignLanguages;

/// <summary>
/// The language package outputs: the schema of every <c>[LanguageDesignToken]</c> class, the
/// <c>DesignLanguageRegistration.Register</c> entry for every <c>[DesignLanguage]</c> class, and the language
/// build contracts whose identity is read from the constant the <c>Id</c> override returns.
/// </summary>
public class DesignLanguageRegistrationGeneratorTests
{
    private const string LanguageDiagnosticId = "ATOMUITOKEN002";

    [Fact]
    public void Emits_The_Language_Token_Schema_Ordered_By_Name_With_Seed_And_Language_Stages()
    {
        var output = Run(CreatePackage("Acme.Shadcn", LanguageSource, CoreReference()), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(output).ShouldBeEmpty();
        var schema = GetGeneratedSource(output, LanguageSchemaFile);
        schema.ShouldContain("namespace AtomUI.Generated.AcmeShadcn;");
        schema.ShouldContain("internal static class GeneratedLanguageTokenSchema");
        schema.ShouldContain(
            "internal static global::System.Collections.Generic.IReadOnlyList<TokenDescriptor> Tokens_Acme_Shadcn_ShadcnToken() => s_Tokens_Acme_Shadcn_ShadcnToken;");
        // Slots follow the ordinal name order, independent of declaration order; the language seed keeps the Seed stage.
        schema.ShouldContain(
            "TokenDescriptor.CreateGenerated(\"BaseScale\", 0, TokenStage.Seed, TokenValueCodec<global::System.Double>.Instance, \"ShadcnToken.BaseScale\", GeneratedLanguageTokenAccessor_Tokens_Acme_Shadcn_ShadcnToken.Instance, 1),");
        schema.ShouldContain(
            "TokenDescriptor.CreateGenerated(\"InputRadius\", 1, TokenStage.Language, TokenValueCodec<global::System.Double>.Instance, \"ShadcnToken.InputRadius\", GeneratedLanguageTokenAccessor_Tokens_Acme_Shadcn_ShadcnToken.Instance, 1),");
        schema.ShouldContain(
            "TokenDescriptor.CreateGenerated(\"Ring\", 2, TokenStage.Language, TokenValueCodec<global::System.String>.Instance, \"ShadcnToken.Ring\", GeneratedLanguageTokenAccessor_Tokens_Acme_Shadcn_ShadcnToken.Instance, 1),");
        // One accessor per language token class instead of per-token delegates.
        schema.ShouldContain("internal sealed class GeneratedLanguageTokenAccessor_Tokens_Acme_Shadcn_ShadcnToken : TokenValueAccessor");
        schema.ShouldContain("case 2: return ((global::Acme.Shadcn.ShadcnToken)token).Ring;");
        schema.ShouldContain("case 0: ((global::Acme.Shadcn.ShadcnToken)token).BaseScale = (global::System.Double)value!; return;");
        schema.ShouldContain(
            "case 1: return global::AtomUI.Theme.Schema.ThemeResourceValue.Project(((global::Acme.Shadcn.ShadcnToken)token).InputRadius);");
        schema.ShouldNotContain("static token =>");
        schema.ShouldNotContain("Activator");
    }

    [Fact]
    public void Language_Token_Schema_Is_Independent_Of_Declaration_Order()
    {
        var reordered = LanguageSource
            .Replace("\n        public partial double InputRadius { get; set; }\n", "\n", StringComparison.Ordinal)
            .Replace("        [DesignTokenKind(DesignTokenKind.Seed)]\n        public partial double BaseScale",
                "        public partial double InputRadius { get; set; }\n\n        [DesignTokenKind(DesignTokenKind.Seed)]\n        public partial double BaseScale",
                StringComparison.Ordinal);
        reordered.ShouldNotBe(LanguageSource);
        var core = CoreReference();

        var first = GetGeneratedSource(Run(CreatePackage("Acme.Shadcn", LanguageSource, core), out _), LanguageSchemaFile);
        var second = GetGeneratedSource(Run(CreatePackage("Acme.Shadcn", reordered, core), out _), LanguageSchemaFile);

        second.ShouldBe(first);
    }

    [Fact]
    public void Emits_Register_With_One_Shared_Descriptor_Per_Language_Ordered_By_Metadata_Name()
    {
        // Declared out of order on purpose: registration order follows the metadata name.
        var source = LanguageSource + """

            namespace Acme.Shadcn
            {
                [DesignLanguage]
                public sealed class AcmeLanguage : AtomUI.Theme.Languages.DesignLanguage<ShadcnToken>
                {
                    public override string Id => "Acme";
                    public override AtomUI.Theme.Languages.DesignLanguageAxes Axes => new AtomUI.Theme.Languages.DesignLanguageAxes();
                    public override void DeriveLanguage(ShadcnToken language) { }
                }
            }
            """;

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(output).ShouldBeEmpty();
        var registration = GetGeneratedSource(output, LanguageRegistrationFile);
        registration.ShouldContain("namespace AtomUI.Generated.AcmeShadcn;");
        registration.ShouldContain("public static class DesignLanguageRegistration");
        registration.ShouldContain("public static class GeneratedDesignLanguageRegistration");
        registration.ShouldContain("global::System.ComponentModel.EditorBrowsableState.Never");
        registration.ShouldContain("DesignLanguageRegistration.Register(builder);");
        registration.ShouldContain("public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        registration.ShouldContain("global::System.ArgumentNullException.ThrowIfNull(builder);");
        registration.ShouldContain("builder.AddDesignLanguage(s_language0);");
        registration.ShouldContain("builder.AddDesignLanguage(s_language1);");
        // Created once and shared: the runtime keys its compilation caches by descriptor instance.
        registration.ShouldContain(
            "private static readonly global::AtomUI.Theme.Schema.DesignLanguageDescriptor s_language0 = new global::AtomUI.Theme.Schema.DesignLanguageDescriptor(new global::Acme.Shadcn.AcmeLanguage(), GeneratedLanguageTokenSchema.Tokens_Acme_Shadcn_ShadcnToken());");
        registration.ShouldContain(
            "private static readonly global::AtomUI.Theme.Schema.DesignLanguageDescriptor s_language1 = new global::AtomUI.Theme.Schema.DesignLanguageDescriptor(new global::Acme.Shadcn.ShadcnLanguage(), GeneratedLanguageTokenSchema.Tokens_Acme_Shadcn_ShadcnToken());");
        CountOf(registration, "builder.AddDesignLanguage(").ShouldBe(2);
        registration.ShouldNotContain("typeof(");
        // Both languages share one schema.
        CountOf(GetGeneratedSource(output, LanguageSchemaFile), "private static readonly TokenDescriptor[]").ShouldBe(1);
    }

    [Fact]
    public void Derived_Language_Token_Schema_Includes_The_Inherited_Tokens_From_A_Referenced_Language()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource.Replace(
            "public sealed partial class ShadcnToken", "public partial class ShadcnToken", StringComparison.Ordinal), core), "Acme.Shadcn");
        var brandSource = """
            using AtomUI.Theme.DesignTokens;
            using AtomUI.Theme.Languages;

            namespace Acme.Brand
            {
                [LanguageDesignToken]
                public sealed partial class BrandToken : Acme.Shadcn.ShadcnToken
                {
                    public partial double BrandHue { get; set; }

                    [NotTokenDefinition]
                    public double Scratch { get; set; }
                }

                [DesignLanguage]
                public sealed class BrandLanguage : DesignLanguage<BrandToken>
                {
                    public override string Id => "Brand";
                    public override DesignLanguageAxes Axes => new DesignLanguageAxes();
                    public override void DeriveLanguage(BrandToken language) { }
                }
            }
            """;

        var output = Run(CreatePackage("Acme.Brand", brandSource, core, shadcn), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(output).ShouldBeEmpty();
        var schema = GetGeneratedSource(output, LanguageSchemaFile);
        schema.ShouldContain("Tokens_Acme_Brand_BrandToken()");
        schema.ShouldContain("CreateGenerated(\"BaseScale\", 0, TokenStage.Seed");
        schema.ShouldContain("CreateGenerated(\"BrandHue\", 1, TokenStage.Language");
        schema.ShouldContain("CreateGenerated(\"InputRadius\", 2, TokenStage.Language");
        schema.ShouldContain("CreateGenerated(\"Ring\", 3, TokenStage.Language");
        schema.ShouldContain("\"BrandToken.Ring\"");
        schema.ShouldContain("case 3: return ((global::Acme.Brand.BrandToken)token).Ring;");
        schema.ShouldNotContain("Scratch");
        GetGeneratedSource(output, LanguageRegistrationFile).ShouldContain(
            "new global::Acme.Brand.BrandLanguage(), GeneratedLanguageTokenSchema.Tokens_Acme_Brand_BrandToken()");
        // The base language token class is not repeated: it belongs to the referenced language package.
        schema.ShouldNotContain("Tokens_Acme_Shadcn_ShadcnToken");
    }

    [Theory]
    [InlineData("public abstract class BrokenLanguage : DesignLanguage<ShadcnToken>")]
    [InlineData("public sealed class BrokenLanguage<T> : DesignLanguage<ShadcnToken>")]
    [InlineData("public sealed class BrokenLanguage")]
    public void Reports_A_Language_Class_That_Cannot_Be_Registered(string declaration)
    {
        var name = declaration.Contains("<T>", StringComparison.Ordinal) ? "Acme.Shadcn.BrokenLanguage<T>" : "Acme.Shadcn.BrokenLanguage";
        var source = LanguageSource + $$"""

            namespace Acme.Shadcn
            {
                using AtomUI.Theme.Languages;

                [DesignLanguage]
                {{declaration}}
                {
                }
            }
            """;

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        // The build contract model reports the missing Id override of the abstract and generic declarations as well.
        var diagnostic = diagnostics.Where(static d => d.Id == LanguageDiagnosticId &&
            d.GetMessage(null).EndsWith("with a public parameterless constructor.", StringComparison.Ordinal)).ShouldHaveSingleItem();
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage(null).ShouldBe(
            $"'{name}' must be a non-abstract, non-generic class derived from DesignLanguage<TLanguageToken> with a public parameterless constructor.");
        diagnostic.Location.GetLineSpan().Path.ShouldBe("Acme.Shadcn.cs");
        // The valid language next to it is still registered; the broken one is not.
        var registration = GetGeneratedSource(output, LanguageRegistrationFile);
        registration.ShouldContain("new global::Acme.Shadcn.ShadcnLanguage()");
        registration.ShouldNotContain("BrokenLanguage");
    }

    // Suspected generator bug: LocalLanguageModel.Collect does not apply the registration checks (abstract, generic,
    // public parameterless constructor), so an unregistered language still gets a ThemeBuildLanguageAttribute.
    [Fact]
    public void Does_Not_Publish_A_Build_Contract_For_A_Language_That_Is_Not_Registered()
    {
        // A theme package must not be able to compile a theme for a language its language package never registers.
        var source = LanguageSource.Replace("public sealed class ShadcnLanguage", "public abstract class ShadcnLanguage",
            StringComparison.Ordinal);

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.ShouldContain(static d => d.Id == LanguageDiagnosticId &&
            d.GetMessage(null) == "'Acme.Shadcn.ShadcnLanguage' must be a non-abstract, non-generic class derived from DesignLanguage<TLanguageToken> with a public parameterless constructor.");
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeFalse();
        GetGeneratedSource(output, ContractsFile).ShouldNotContain("ThemeBuildLanguageAttribute");
    }

    [Fact]
    public void Reports_A_Language_Class_Without_A_Public_Parameterless_Constructor()
    {
        var source = LanguageSource.Replace(
            "public override string Id => \"Shadcn\";",
            "public ShadcnLanguage(int scale) { }\n\n        public override string Id => \"Shadcn\";",
            StringComparison.Ordinal);

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.Where(static d => d.Id == LanguageDiagnosticId).ShouldHaveSingleItem().GetMessage(null).ShouldBe(
            "'Acme.Shadcn.ShadcnLanguage' must be a non-abstract, non-generic class derived from DesignLanguage<TLanguageToken> with a public parameterless constructor.");
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeFalse();
        // The schema of the language token class does not depend on a valid language.
        HasGeneratedSource(output, LanguageSchemaFile).ShouldBeTrue();
    }

    [Fact]
    public void Reports_A_Language_Whose_Token_Class_Is_Not_A_Language_Token_Of_The_Same_Assembly()
    {
        var core = CoreReference();
        var shadcn = Reference(CreatePackage("Acme.Shadcn", LanguageSource.Replace(
            "public sealed partial class ShadcnToken", "public partial class ShadcnToken", StringComparison.Ordinal), core), "Acme.Shadcn");
        var source = """
            using AtomUI.Theme.DesignTokens;
            using AtomUI.Theme.Languages;

            namespace Acme.Brand
            {
                public sealed class UnmarkedToken : LanguageDesignToken
                {
                }

                [DesignLanguage]
                public sealed class UnmarkedLanguage : DesignLanguage<UnmarkedToken>
                {
                    public override string Id => "Unmarked";
                    public override DesignLanguageAxes Axes => new DesignLanguageAxes();
                    public override void DeriveLanguage(UnmarkedToken language) { }
                }

                [DesignLanguage]
                public sealed class BorrowedLanguage : DesignLanguage<Acme.Shadcn.ShadcnToken>
                {
                    public override string Id => "Borrowed";
                    public override DesignLanguageAxes Axes => new DesignLanguageAxes();
                    public override void DeriveLanguage(Acme.Shadcn.ShadcnToken language) { }
                }
            }
            """;

        var output = Run(CreatePackage("Acme.Brand", source, core, shadcn), out var diagnostics);

        diagnostics.Where(static d => d.Id == LanguageDiagnosticId).Select(static d => d.GetMessage(null)).ShouldBe([
            "The language token type 'Acme.Shadcn.ShadcnToken' of 'Acme.Brand.BorrowedLanguage' must be declared with [LanguageDesignToken] in the same assembly.",
            "The language token type 'Acme.Brand.UnmarkedToken' of 'Acme.Brand.UnmarkedLanguage' must be declared with [LanguageDesignToken] in the same assembly."
        ], ignoreOrder: true);
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeFalse();
        HasGeneratedSource(output, LanguageSchemaFile).ShouldBeFalse();
        // Neither language gets a build contract.
        GetGeneratedSource(output, ContractsFile).ShouldNotContain("ThemeBuildLanguageAttribute");
    }

    [Theory]
    [InlineData("public sealed partial class OrphanToken : AbstractDesignToken { }", "Acme.Shadcn.OrphanToken")]
    [InlineData("public sealed partial class OrphanToken<T> : LanguageDesignToken { }", "Acme.Shadcn.OrphanToken<T>")]
    [InlineData("public static class Holder { [LanguageDesignToken] public sealed partial class OrphanToken : LanguageDesignToken { } }", "Acme.Shadcn.Holder.OrphanToken")]
    public void Reports_A_Language_Token_Class_That_Is_Not_A_Top_Level_Non_Generic_Language_Token(string declaration, string name)
    {
        var attribute = declaration.StartsWith("public static class Holder", StringComparison.Ordinal) ? "" : "[LanguageDesignToken]";
        var source = LanguageSource + $$"""

            namespace Acme.Shadcn
            {
                using AtomUI.Theme.DesignTokens;

                {{attribute}}
                {{declaration}}
            }
            """;

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.Where(static d => d.Id == LanguageDiagnosticId).ShouldHaveSingleItem().GetMessage(null).ShouldBe(
            $"'{name}' must be a top-level, non-generic class derived from LanguageDesignToken.");
        var schema = GetGeneratedSource(output, LanguageSchemaFile);
        schema.ShouldContain("Tokens_Acme_Shadcn_ShadcnToken()");
        schema.ShouldNotContain("OrphanToken");
    }

    [Fact]
    public void Emits_The_Schema_But_No_Registration_When_The_Runtime_Has_No_Builder()
    {
        // A language package's tokens are useful to tooling even before the runtime offers IThemeManagerBuilder.
        var core = CSharpCompilationWithoutBuilder();

        var output = Run(CreatePackage("Acme.Shadcn", LanguageSource, core), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, LanguageSchemaFile).ShouldBeTrue();
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeFalse();
    }

    [Fact]
    public void Emits_Nothing_For_An_Assembly_Without_Languages_Or_Language_Tokens()
    {
        var output = Run(CreatePackage("Acme.Controls", ControlsSource, CoreReference()), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, LanguageSchemaFile).ShouldBeFalse();
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeFalse();
        GetGeneratedSource(output, ContractsFile).ShouldNotContain("ThemeBuildLanguageAttribute");
        GetGeneratedSource(output, ContractsFile).ShouldNotContain("\"$language\"");
    }

    [Theory]
    [InlineData("public override string Id => \"Shadcn\";")]
    [InlineData("public override string Id { get => \"Shadcn\"; }")]
    [InlineData("public override string Id { get { return \"Shadcn\"; } }")]
    public void Writes_The_Language_Build_Contract_With_The_Id_Read_From_The_Id_Override(string idDeclaration)
    {
        var source = LanguageSource.Replace("public override string Id => \"Shadcn\";", idDeclaration, StringComparison.Ordinal);

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        Errors(output).ShouldBeEmpty();
        var contracts = GetGeneratedSource(output, ContractsFile);
        contracts.ShouldContain(
            "[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildLanguageAttribute(1, \"Shadcn\", \"Acme.Shadcn.ShadcnToken\", \"Acme.Shadcn, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\")]");
        // Language token contracts: Catalog "$language", OwnerId = the language id, no resource key; seed stage 0, language stage 4.
        var coreLib = typeof(object).Assembly.FullName;
        contracts.ShouldContain(
            $"[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildTokenAttribute(1, \"$language\", \"Shadcn\", \"BaseScale\", \"global::System.Double\", \"{coreLib}\", \"\", \"\", \"BaseScale\", 0, 1)]");
        contracts.ShouldContain(
            $"[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildTokenAttribute(1, \"$language\", \"Shadcn\", \"InputRadius\", \"global::System.Double\", \"{coreLib}\", \"\", \"\", \"InputRadius\", 4, 1)]");
        contracts.ShouldContain(
            $"[assembly: global::AtomUI.Theme.Definitions.Compiled.ThemeBuildTokenAttribute(1, \"$language\", \"Shadcn\", \"Ring\", \"global::System.String\", \"{coreLib}\", \"\", \"\", \"Ring\", 4, 1)]");
        CountOf(contracts, "\"$language\"").ShouldBe(3);
        ErrorsIn(output, ContractsFile).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("public override string Id => \"Bad Id\";")]
    [InlineData("public override string Id => \"1Shadcn\";")]
    [InlineData("public override string Id => string.Concat(\"Sha\", \"dcn\");")]
    [InlineData("public override string Id => System.Environment.MachineName;")]
    public void Reports_A_Language_Whose_Id_Is_Not_A_Constant_Identifier(string idDeclaration)
    {
        var source = LanguageSource.Replace("public override string Id => \"Shadcn\";", idDeclaration, StringComparison.Ordinal);

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        var diagnostic = diagnostics.Where(static d => d.Id == LanguageDiagnosticId).ShouldHaveSingleItem();
        diagnostic.GetMessage(null).ShouldBe("'Acme.Shadcn.ShadcnLanguage' must return a constant string from its Id property.");
        diagnostic.Location.GetLineSpan().Path.ShouldBe("Acme.Shadcn.cs");
        var contracts = GetGeneratedSource(output, ContractsFile);
        contracts.ShouldNotContain("ThemeBuildLanguageAttribute");
        contracts.ShouldNotContain("\"$language\"");
    }

    // Suspected generator bug: LocalLanguageModel.TryGetConstantId accepts only a string literal syntax node, so an Id
    // that returns a const field (or nameof) is rejected although its value is a compile-time constant.
    [Theory]
    [InlineData("private const string LanguageId = \"Shadcn\";\n\n        public override string Id => LanguageId;")]
    [InlineData("public override string Id => nameof(Shadcn);")]
    public void Reads_The_Id_From_A_Constant_That_Is_Not_A_Literal(string idDeclaration)
    {
        var source = LanguageSource.Replace("public override string Id => \"Shadcn\";", idDeclaration, StringComparison.Ordinal)
            + "\nnamespace Acme.Shadcn { internal static class Shadcn { } }\n";

        var output = Run(CreatePackage("Acme.Shadcn", source, CoreReference()), out var diagnostics);

        diagnostics.ShouldBeEmpty();
        GetGeneratedSource(output, ContractsFile).ShouldContain("ThemeBuildLanguageAttribute(1, \"Shadcn\",");
    }

    [Fact]
    public void Generator_Alone_Emits_The_Schema_And_Registration()
    {
        // The language generator does not depend on the token resource generator having run.
        var output = Run(CreatePackage("Acme.Shadcn", LanguageSource, CoreReference()),
            [new DesignLanguageRegistrationGenerator()], out var diagnostics);

        diagnostics.ShouldBeEmpty();
        HasGeneratedSource(output, LanguageSchemaFile).ShouldBeTrue();
        HasGeneratedSource(output, LanguageRegistrationFile).ShouldBeTrue();
        HasGeneratedSource(output, ContractsFile).ShouldBeFalse();
    }

    private static MetadataReference CSharpCompilationWithoutBuilder()
    {
        var stubs = RuntimeStubs;
        var start = stubs.IndexOf("namespace AtomUI.Theme\n{\n    public interface IThemeManagerBuilder", StringComparison.Ordinal);
        start.ShouldBeGreaterThan(0);
        var end = stubs.IndexOf("namespace AtomUI.Theme.DesignTokens", start, StringComparison.Ordinal);
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("AtomUI.Core",
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(stubs.Remove(start, end - start),
                new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
                cancellationToken: TestContext.Current.CancellationToken)],
            PlatformReferences(), new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        compilation.GetTypeByMetadataName("AtomUI.Theme.IThemeManagerBuilder").ShouldBeNull();
        return compilation.ToMetadataReference();
    }
}
