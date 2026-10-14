using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using static AtomUI.Generator.Tests.Registration.TypeMapRegistrationGeneratorTests;

namespace AtomUI.Generator.Tests.Registration;

public partial class TypeMapIntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Cross_Asset_Type_Key_Requires_The_Exact_Default_Target_Condition(bool defaultTarget)
    {
        var result = Run("namespace Demo { public class Host : Avalonia.Controls.Control { } public class Other : Avalonia.Controls.Control { } public class Unrelated : Avalonia.Controls.Control { } }",
            Asset("Host", "<ControlTheme x:Key=\"Host\" TargetType=\"local:Host\"><Setter Property=\"Tag\" Value=\"{StaticResource {x:Type local:Other}}\" /></ControlTheme>"),
            Asset("Provider", "<ControlTheme x:Key=\"{x:Type local:Other}\" TargetType=\"local:" + (defaultTarget ? "Other" : "Unrelated") + "\" />"));
        if (defaultTarget) result.Diagnostics.ShouldBeEmpty();
        else result.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIREG004");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Cross_Asset_Type_Key_Provider_Must_Cover_The_Consumer_Domain(bool sameDomain)
    {
        var source = "namespace Demo { " + (sameDomain ? "[System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")]" : "") +
            "public class Host : Avalonia.Controls.Control { } [System.Runtime.Versioning.UnsupportedOSPlatform(\"browser\")] public class Other : Avalonia.Controls.Control { } }";
        var result = Run(source,
            Asset("Host", "<ControlTheme x:Key=\"Host\" TargetType=\"local:Host\"><Setter Property=\"Tag\" Value=\"{StaticResource {x:Type local:Other}}\" /></ControlTheme>"),
            Asset("Provider", "<ControlTheme x:Key=\"{x:Type local:Other}\" TargetType=\"local:Other\" />"));
        if (sameDomain) result.Diagnostics.ShouldBeEmpty();
        else result.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe("ATOMUIREG004");
    }

    [Theory]
    [InlineData("Arbitrary")]
    [InlineData("Action")]
    public void Explicit_Include_Property_Assignment_Resolves_Actual_Semantic_Target_Independent_Of_FileName(string name)
    {
        var result = Run("""
            namespace Demo;
            public class ActionControl : Avalonia.Controls.Control { }
            [AtomUI.Theme.SemanticPart("action", SelectorClass = "semantic-action", ContractType = typeof(Avalonia.Controls.Control), Since = "6.0.0",
                Customization = AtomUI.Theme.SemanticPartCustomization.SelectorAndTheme, ThemePropertyName = "ActionTheme", RuntimeCreated = true, SelectorRoute = ">> .semantic-action")]
            public class Button : Avalonia.Controls.Control { public Avalonia.Styling.ControlTheme? ActionTheme { get; set; } }
            """, Asset(name, "<ControlTheme x:Key=\"ActualValue\" TargetType=\"local:ActionControl\" />"),
            Asset("Owner", "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"/" + name + "/Themes/" + name + "Theme.axaml\"/></ResourceDictionary.MergedDictionaries>" +
                "<ControlTheme x:Key=\"Owner\" TargetType=\"local:Button\"><Setter Property=\"ActionTheme\" Value=\"{StaticResource ActualValue}\"/></ControlTheme>"));
        result.Diagnostics.ShouldBeEmpty();
        Source(result, "GeneratedSemanticPartManifest.g.cs").ShouldContain("ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Demo.ActionControl))");
        Source(result, "GeneratedTypeMapRegistration.g.cs").ShouldContain("\"ActionTheme\", typeof(global::Demo.ActionControl)");
    }

    [Fact]
    public void Resource_Intersection_Makes_Shared_Targets_Legal_And_Guards_Every_Inbound_Asset()
    {
        var result = Run("""
            namespace Demo;
            [System.Runtime.Versioning.SupportedOSPlatform("windows6.2")]
            [System.Runtime.Versioning.UnsupportedOSPlatform("windows10.0")]
            public class Any : Avalonia.Controls.Control { }
            [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
            [System.Runtime.Versioning.UnsupportedOSPlatform("windows10.0")]
            public class Later : Avalonia.Controls.Control { }
            [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
            internal class MixedTheme : Avalonia.Controls.ResourceDictionary { }
            """, Asset("Mixed", "<ControlTheme x:Key=\"Any\" TargetType=\"local:Any\"/><ControlTheme x:Key=\"Later\" TargetType=\"local:Later\"/>", "Demo.MixedTheme"));
        result.Diagnostics.ShouldBeEmpty();
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        var add = source.Substring(source.IndexOf("internal static void AddAsset_", StringComparison.Ordinal));
        add = add.Substring(0, add.IndexOf("builder.AddThemeAsset", StringComparison.Ordinal));
        add.ShouldContain("IsWindowsVersionAtLeast(7, 0, 0, 0)");
        add.ShouldContain("!global::System.OperatingSystem.IsWindowsVersionAtLeast(10, 0, 0, 0)");
        source.Split('\n').Count(line => line.Contains("GeneratedRegistrationFactories.AddAsset_")).ShouldBe(2);
    }

    [Fact]
    public void Required_Owner_Availability_Must_Cover_A_Theme_That_Does_Not_Export_It()
    {
        var result = Run("""
            namespace Demo;
            public class Any : Avalonia.Controls.Control { }
            [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
            public class DesktopOnly : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class DesktopOnlyToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            { public double Height { get; set; } public override void CalculateTokenValues(bool dark) { } }
            """, Asset("Any", """
            <ControlTheme x:Key="Any" TargetType="local:Any" xmlns:tokens="using:Demo.DesignTokens">
                <Setter Property="Tag" Value="{tokens:DesktopOnlyTokenResource Height}" />
            </ControlTheme>
            """));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG006" && d.GetMessage().Contains("required Token owner"));
    }

    [Fact]
    public void Unincluded_Exported_StaticResource_Is_Not_A_Proved_Dependency()
    {
        var result = Run("namespace Demo { public class Host : Avalonia.Controls.Control { } public class Other : Avalonia.Controls.Control { } }",
            Asset("Host", "<ControlTheme x:Key=\"Host\" TargetType=\"local:Host\"><Setter Property=\"Tag\" Value=\"{StaticResource SharedTheme}\" /></ControlTheme>"),
            Asset("Other", "<ControlTheme x:Key=\"SharedTheme\" TargetType=\"local:Other\" />"));
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIREG004");
        diagnostic.Location.GetLineSpan().Path.ShouldEndWith("HostTheme.axaml");
        diagnostic.GetMessage().ShouldContain("SharedTheme");
    }

    [Theory]
    [InlineData("ActionTheme")]
    [InlineData("RenamedTheme")]
    public void Unassigned_Same_Name_Theme_Does_Not_Change_Semantic_Metadata_Or_Owner_Assets(string fileName)
    {
        var result = Run("""
            namespace Demo;
            public class Unrelated : Avalonia.Controls.Control { }
            [AtomUI.Theme.SemanticPart("action", SelectorClass = "semantic-action", ContractType = typeof(Avalonia.Controls.Control), Since = "6.0.0",
                Customization = AtomUI.Theme.SemanticPartCustomization.SelectorAndTheme, ThemePropertyName = "ActionTheme", RuntimeCreated = true, SelectorRoute = ">> .semantic-action")]
            public class Button : Avalonia.Controls.Control { public Avalonia.Styling.ControlTheme? ActionTheme { get; set; } }
            """, new TextFile("Any/Themes/" + fileName + ".axaml", """
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Demo">
                <ControlTheme x:Key="Unrelated" TargetType="local:Unrelated" />
            </ResourceDictionary>
            """));
        result.Diagnostics.ShouldBeEmpty();
        Source(result, "GeneratedSemanticPartManifest.g.cs").ShouldContain("ControlThemeSemanticPartDescriptor(\"ActionTheme\", typeof(global::Avalonia.Controls.Control))");
        var generated = Source(result, "GeneratedTypeMapRegistration.g.cs");
        var start = generated.IndexOf("FragmentId => \"Demo.Controls:Demo.Button,", StringComparison.Ordinal);
        // Fragment behavior is a public generated registration boundary: the owner has no asset contribution.
        var block = generated.Substring(start, generated.IndexOf("\n}", start, StringComparison.Ordinal) - start);
        block.ShouldNotContain("GeneratedRegistrationFactories.AddAsset_");
        generated.ShouldNotContain("\"ActionTheme\", typeof(global::Demo.Unrelated)");
    }

    [Theory]
    [InlineData("macos")]
    [InlineData("browser")]
    public void Indivisible_Asset_With_Incompatible_Target_Availability_Is_Diagnosed(string platform)
    {
        var result = Run($$"""
            namespace Demo;
            public class AnyPlatform : Avalonia.Controls.Control { }
            [System.Runtime.Versioning.UnsupportedOSPlatform("{{platform}}")]
            public class DesktopOnly : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class DesktopOnlyToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double Height { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """, Asset("Mixed", """
            <ControlTheme x:Key="Any" TargetType="local:AnyPlatform" />
            <ControlTheme x:Key="Desktop" TargetType="local:DesktopOnly" xmlns:tokens="using:Demo.DesignTokens">
                <Setter Property="Tag" Value="{tokens:DesktopOnlyTokenResource Height}" />
            </ControlTheme>
            """));
        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Id.ShouldBe("ATOMUIREG006");
        diagnostic.Location.GetLineSpan().Path.ShouldEndWith("MixedTheme.axaml");
    }

    [Fact]
    public void Generated_Own_Token_Extension_Resolves_Existing_Boxed_Enum_Overrides()
    {
        var (_, assembly) = Emit("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """);
        var extensionType = assembly.GetType("Demo.DesignTokens.ButtonTokenResourceExtension")!;
        var tokenKeyType = assembly.GetType("Demo.DesignTokens.ButtonTokenKey")!;
        var ownKindType = assembly.GetType("Demo.DesignTokens.ButtonTokenKind")!;
        var extension = Activator.CreateInstance(extensionType)!;
        var key = extensionType.GetMethod("GetResourceKey", BindingFlags.Instance | BindingFlags.NonPublic)!
                               .Invoke(extension, [Enum.Parse(tokenKeyType, "LocalHeight")])!;
        var overrides = new Dictionary<object, object>
        {
            [Enum.Parse(ownKindType, "LocalHeight")] = 37d
        };

        overrides.TryGetValue(key, out var value).ShouldBeTrue();
        value.ShouldBe(37d);
        key.GetType().ShouldBe(ownKindType);
    }

    [Theory]
    [InlineData("String", typeof(string))]
    [InlineData("Uri", typeof(Uri))]
    public void Repair1_RefPack_Framework_Type_Keys_Match_Real_Core(string keyName, Type expectedKey)
    {
        var asset = Asset("Button", "<ControlTheme xmlns:sys=\"using:System\" x:Key=\"{x:Type sys:" + keyName + "}\" TargetType=\"local:Button\" />");
        var compilation = ReferencePackCompilation("namespace Demo { public class Button : Avalonia.Controls.Control { } }", [asset]);
        compilation.GetTypeByMetadataName("System." + keyName)!.ContainingAssembly.Name.ShouldBe("System.Runtime");
        var (_, assembly) = EmitCompilation(compilation, [asset]);
        var descriptor = GeneratedAsset(assembly);
        var export = ((System.Collections.IEnumerable)descriptor.GetType().GetProperty("ExportedThemes")!.GetValue(descriptor)!).Cast<object>().Single();
        export.GetType().GetProperty("ResourceKey")!.GetValue(export).ShouldBeSameAs(expectedKey);
        AssertRealFingerprintAndManifest(descriptor);
    }

    [Fact]
    public void Repair1_Generated_StyledElement_Descriptor_Registers_In_Real_Core()
    {
        var (_, assembly) = Emit("""
            namespace Demo;
            public class Chrome : Avalonia.StyledElement { }
            public sealed class Provider : AtomUI.Theme.Resources.ControlThemesProvider { public Provider() { Id = "Demo.Controls"; } }
            public static class Probe
            {
                public static void Register(AtomUI.IAtomUIBuilder builder) => AtomUI.Generated.TypeMapFixture.GeneratedControlPackageRegistration.Register(builder, static () => new Provider());
            }
            """, Asset("Chrome", "<ControlTheme x:Key=\"Chrome\" TargetType=\"local:Chrome\" />"));
        var descriptor = GeneratedAsset(assembly);
        AssertRealFingerprintAndManifest(descriptor);
        var core = LoadCore();
        var application = Activator.CreateInstance(Assembly.Load("Avalonia.Controls").GetType("Avalonia.Application")!);
        var builder = Activator.CreateInstance(core.GetType("AtomUI.AtomUIBuilder")!, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { application }, null)!;
        assembly.GetType("Demo.Probe")!.GetMethod("Register")!.Invoke(null, new[] { builder });
        var themeBuilder = builder.GetType().GetProperty("ThemeManagerBuilder", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(builder)!;
        var package = ((System.Collections.IEnumerable)themeBuilder.GetType().GetProperty("ControlPackages", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(themeBuilder)!).Cast<object>().Single();
        ((System.Collections.IEnumerable)package.GetType().GetProperty("Controls")!.GetValue(package)!).Cast<object>().ShouldBeEmpty();
        ((System.Collections.IEnumerable)package.GetType().GetProperty("ThemeAssets")!.GetValue(package)!).Cast<object>().ShouldHaveSingleItem();
        using var manager = (IDisposable)themeBuilder.GetType().GetMethod("Build", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)!.Invoke(themeBuilder, null)!;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Repair1_Inline_Merged_Dictionaries_Are_Lexically_Visible_Only(bool unrelatedConsumer)
    {
        var assets = new List<TextFile> { Asset("Button", """
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary><SolidColorBrush x:Key="Brush" Color="Red" /></ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
            <ControlTheme x:Key="Button" TargetType="local:Button"><Setter Property="Background" Value="{StaticResource Brush}" /></ControlTheme>
            """) };
        if (unrelatedConsumer) assets.Add(Asset("Other", "<ControlTheme x:Key=\"Other\" TargetType=\"local:Other\"><Setter Property=\"Background\" Value=\"{StaticResource Brush}\" /></ControlTheme>"));
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } public class Other : Avalonia.Controls.Control { } }", assets.ToArray());
        if (!unrelatedConsumer) result.Diagnostics.ShouldBeEmpty();
        else result.Diagnostics.ShouldHaveSingleItem().Location.GetLineSpan().Path.ShouldEndWith("OtherTheme.axaml");
    }

    private static object GeneratedAsset(Assembly assembly) => assembly.GetType("AtomUI.Generated.TypeMapFixture.GeneratedRegistrationFactories")!
        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(method => method.ReturnType.FullName == "AtomUI.Theme.Schema.ControlThemeAssetDescriptor").Invoke(null, null)!;

    private static void AssertRealFingerprintAndManifest(object descriptor)
    {
        var core = LoadCore();
        var globals = core.GetType("AtomUI.Generated.AtomUICore.GeneratedThemeSchema")!.GetMethod("GetGlobalTokens", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null);
        var registryType = core.GetType("AtomUI.Theme.Schema.ThemeSchemaRegistry")!;
        var expected = registryType.GetMethod("ComputeResourceKeySchemaFingerprint", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new[] { descriptor, globals });
        descriptor.GetType().GetProperty("ResourceKeySchemaFingerprint")!.GetValue(descriptor).ShouldBe(expected);
        var registry = Activator.CreateInstance(registryType, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { globals!, Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ControlTokenDescriptor")!, 0), Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ThemeAlgorithmDescriptor")!, 0), TypedArray(descriptor) }, null);
        Activator.CreateInstance(core.GetType("AtomUI.Theme.Schema.ControlThemeAssetManifest")!, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { registry, TypedArray(descriptor) }, null).ShouldNotBeNull();
    }

    [Fact]
    public void A_Resolved_Foreign_Include_Is_Only_An_Opaque_Lexical_Source()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } public class Other : Avalonia.Controls.Control { } }",
            Asset("Button", """
                <ResourceDictionary.MergedDictionaries><ResourceInclude Source="avares://AtomUI.Core/ExplicitOpaque.axaml" /></ResourceDictionary.MergedDictionaries>
                <ControlTheme x:Key="Button" TargetType="local:Button"><Setter Property="Tag" Value="{StaticResource ForeignKey}" /></ControlTheme>
                """),
            Asset("Other", "<ControlTheme x:Key=\"Other\" TargetType=\"local:Other\"><Setter Property=\"Tag\" Value=\"{StaticResource MissingLocal}\" /></ControlTheme>"));
        var diagnostics = result.Diagnostics.Where(d => d.Id == "ATOMUIREG004").ToArray();
        diagnostics.ShouldHaveSingleItem().GetMessage().ShouldContain("MissingLocal");
        diagnostics[0].Location.GetLineSpan().Path.ShouldEndWith("OtherTheme.axaml");
    }

    [Fact]
    public void Foreign_Include_Requires_A_Resolved_Assembly()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", "<ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://Missing.Binary/Unknown.axaml\" /></ResourceDictionary.MergedDictionaries><ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG004" && d.GetMessage().Contains("Missing.Binary"));
    }

    [Fact]
    public void Static_Type_Keys_Use_Type_Identity_Across_Xml_Prefixes()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", """
                <ControlTheme x:Key="{x:Type local:Button}" TargetType="local:Button" />
                <ControlTheme xmlns:other="using:Demo" x:Key="Alternate" TargetType="other:Button" BasedOn="{StaticResource {x:Type other:Button}}" />
                """));
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void Internal_Nested_Control_Can_Own_A_Top_Level_Token_Contract()
    {
        var result = Run("""
            namespace Demo;
            public class Host { internal class Presenter : Avalonia.Controls.Control { } }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class PresenterToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """);
        result.Diagnostics.ShouldBeEmpty();
        Source(result, "GeneratedTypeMapRegistration.g.cs").ShouldContain("typeof(global::Demo.Host.Presenter)");
    }

    [Fact]
    public void Duplicate_Export_Keys_Diagnose_Before_Metadata_Deduplication()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" /><ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG001");
    }

    [Fact]
    public void Non_Platform_Array_Attributes_Do_Not_Interfere_With_Registration()
    {
        var result = Run("namespace Demo { [Avalonia.Controls.Metadata.PseudoClasses(\":foo\")] public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        result.Diagnostics.ShouldBeEmpty();
        Source(result, "GeneratedTypeMapRegistration.g.cs").ShouldContain("typeof(global::Demo.Button)");
    }

    [Fact]
    public void Third_Party_Default_Catalog_Coexists_With_An_Explicit_BuiltIn_Button_In_Real_Core()
    {
        const string body = """
            public class Button : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """;
        var builtin = EmitCompilation(Compilation("namespace BuiltIn { " + body + " }", [], "BuiltInControls"), [],
            new Options(globalOptions: new() { ["build_property.AtomUIThemeControlCatalog"] = "AtomUI" })).Assembly;
        var acme = EmitCompilation(Compilation("namespace Acme { " + body + " }", [], "Acme.Controls"), []).Assembly;
        var first = builtin.GetType("AtomUI.Generated.BuiltInControls.GeneratedThemeSchemaDescriptorFactory")!.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(m => m.Name.StartsWith("CreateControlDescriptor_", StringComparison.Ordinal)).Invoke(null, null)!;
        var second = acme.GetType("AtomUI.Generated.AcmeControls.GeneratedThemeSchemaDescriptorFactory")!.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(m => m.Name.StartsWith("CreateControlDescriptor_", StringComparison.Ordinal)).Invoke(null, null)!;
        var identity = second.GetType().GetProperty("Identity")!.GetValue(second)!;
        identity.GetType().GetProperty("Catalog")!.GetValue(identity).ShouldBe("Acme.Controls");
        var descriptors = Array.CreateInstance(first.GetType(), 2); descriptors.SetValue(first, 0); descriptors.SetValue(second, 1);
        var core = LoadCore();
        var globals = core.GetType("AtomUI.Generated.AtomUICore.GeneratedThemeSchema")!.GetMethod("GetGlobalTokens", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null);
        var registry = core.GetType("AtomUI.Theme.Schema.ThemeSchemaRegistry")!;
        var instance = Activator.CreateInstance(registry, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { globals!, descriptors, Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ThemeAlgorithmDescriptor")!, 0), Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ControlThemeAssetDescriptor")!, 0) }, null);
        instance.ShouldNotBeNull();
    }

    [Fact]
    public void Token_Resource_Prefix_Must_Resolve_The_Actual_Owner_Contract()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ButtonToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """, Asset("Button", """
                <ControlTheme x:Key="Button" TargetType="local:Button" xmlns:wrong="using:Missing.DesignTokens">
                    <Setter Property="Tag" Value="{wrong:ButtonTokenResource LocalHeight}" />
                </ControlTheme>
                """));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG002");
    }

    [Fact]
    public void Exported_Semantic_Theme_Bindings_Use_Typed_Owner_And_Actual_Target()
    {
        var (_, assembly) = Emit("""
            namespace Demo;
            public class ActionControl : Avalonia.Controls.Control { }
            [AtomUI.Theme.SemanticPart("action", SelectorClass = "semantic-action", ContractType = typeof(ActionControl), Since = "6.0.0",
                Customization = AtomUI.Theme.SemanticPartCustomization.SelectorAndTheme, ThemePropertyName = "ActionTheme", RuntimeCreated = true, SelectorRoute = ">> .semantic-action")]
            public class Button : Avalonia.Controls.Control
            {
                public Avalonia.Styling.ControlTheme? ActionTheme { get; set; }
            }
            """, Asset("Arbitrary", "<ControlTheme x:Key=\"Action\" TargetType=\"local:ActionControl\" /><ControlTheme x:Key=\"Owner\" TargetType=\"local:Button\"><Setter Property=\"ActionTheme\" Value=\"{StaticResource Action}\" /></ControlTheme>"));
        var descriptor = GeneratedAsset(assembly);
        var bindings = ((System.Collections.IEnumerable)descriptor.GetType().GetProperty("SemanticThemeBindings")!.GetValue(descriptor)!).Cast<object>().ToArray();
        var binding = bindings.ShouldHaveSingleItem();
        binding.GetType().GetProperty("PropertyName")!.GetValue(binding).ShouldBe("ActionTheme");
        binding.GetType().GetProperty("TargetType")!.GetValue(binding).ShouldBe(assembly.GetType("Demo.ActionControl"));
        var identity = binding.GetType().GetProperty("OwnerIdentity")!.GetValue(binding)!;
        identity.GetType().GetProperty("OwnerType", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(identity).ShouldBe(assembly.GetType("Demo.Button"));
        AssertRealFingerprintAndManifest(descriptor);
    }

    [Fact]
    public void Forwarded_External_Control_Owner_Uses_Its_Defining_Assembly_In_Compiled_Fingerprint()
    {
        var definition = Compilation("[assembly: System.Reflection.AssemblyVersion(\"4.3.2.1\")] namespace Foreign { public class Widget : Avalonia.Controls.Control { } }", [], "ForwardedWidgetDefinition");
        using var definingStream = new MemoryStream();
        definition.Emit(definingStream, cancellationToken: TestContext.Current.CancellationToken).Success.ShouldBeTrue();
        var definingBytes = definingStream.ToArray();
        var definingReference = MetadataReference.CreateFromImage(definingBytes);
        var facade = EmitReference(Compilation("[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Foreign.Widget))]", [], "ForwardedWidgetFacade").AddReferences(definingReference));
        var asset = new TextFile("Widget/Themes/WidgetTheme.axaml", """
            <ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Foreign" xmlns:tokens="using:Foreign.DesignTokens">
                <ControlTheme x:Key="Widget" TargetType="local:Widget"><Setter Property="Tag" Value="{tokens:WidgetTokenResource LocalHeight}" /></ControlTheme>
            </ResourceDictionary>
            """);
        var compilation = Compilation("""
            namespace Foreign;
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class WidgetToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """, [asset]).AddReferences(definingReference, facade);
        LoadCore();
        System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(definingBytes));
        var (_, assembly) = EmitCompilation(compilation, [asset]);
        var descriptor = assembly.GetType("AtomUI.Generated.TypeMapFixture.GeneratedRegistrationFactories")!.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.ReturnType.FullName == "AtomUI.Theme.Schema.ControlThemeAssetDescriptor").Invoke(null, null)!;
        var core = LoadCore();
        var globals = core.GetType("AtomUI.Generated.AtomUICore.GeneratedThemeSchema")!.GetMethod("GetGlobalTokens", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null);
        var actual = core.GetType("AtomUI.Theme.Schema.ThemeSchemaRegistry")!.GetMethod("ComputeResourceKeySchemaFingerprint", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new[] { descriptor, globals });
        descriptor.GetType().GetProperty("ResourceKeySchemaFingerprint")!.GetValue(descriptor).ShouldBe(actual);
        var owners = ((System.Collections.IEnumerable)descriptor.GetType().GetProperty("RequiredTokenOwners")!.GetValue(descriptor)!).Cast<object>().ToArray();
        owners.ShouldHaveSingleItem();
        var ownerType = (Type)owners[0].GetType().GetProperty("OwnerType", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owners[0])!;
        ownerType.Assembly.FullName.ShouldStartWith("ForwardedWidgetDefinition, Version=4.3.2.1");
    }

    [Fact]
    public void Generated_Register_Stages_Real_Core_Package_Without_Executing_Resources_And_Rejects_Duplicates()
    {
        var (_, assembly) = Emit("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            public sealed class Provider : AtomUI.Theme.Resources.ControlThemesProvider
            {
                public Provider() { Id = "Demo.Controls"; }
            }
            public static class Probe
            {
                public static int Providers;
                public static Provider? Last;
                public static void Register(AtomUI.IAtomUIBuilder builder) => AtomUI.Generated.TypeMapFixture.GeneratedControlPackageRegistration.Register(builder,
                    static () => { Providers++; return Last = new Provider(); });
            }
            """, Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        var core = LoadCore();
        var application = Activator.CreateInstance(Assembly.Load("Avalonia.Controls").GetType("Avalonia.Application")!);
        var builder = Activator.CreateInstance(core.GetType("AtomUI.AtomUIBuilder")!, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { application }, null)!;
        var probe = assembly.GetType("Demo.Probe")!;
        var register = probe.GetMethod("Register")!;
        register.Invoke(null, new[] { builder });
        var provider = probe.GetField("Last")!.GetValue(null)!;
        ((System.Collections.ICollection)provider.GetType().GetProperty("ControlThemes")!.GetValue(provider)!).Count.ShouldBe(0);
        var themeBuilder = builder.GetType().GetProperty("ThemeManagerBuilder", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(builder)!;
        using var manager = (IDisposable)themeBuilder.GetType().GetMethod("Build", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)!.Invoke(themeBuilder, null)!;
        ((System.Collections.ICollection)provider.GetType().GetProperty("ControlThemes")!.GetValue(provider)!).Count.ShouldBe(0);
        Should.Throw<TargetInvocationException>(() => register.Invoke(null, new[] { builder })).InnerException.ShouldBeOfType<InvalidOperationException>();
        probe.GetField("Providers")!.GetValue(null).ShouldBe(1);
    }

    [Fact]
    public void Invalid_Generated_Asset_Reports_Schema_Error_Without_Static_Initializer_Wrapper()
    {
        var source = """
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            public sealed class Provider : AtomUI.Theme.Resources.ControlThemesProvider
            {
                public Provider() { Id = "Demo.Controls"; }
            }
            public static class Probe
            {
                public static void Register(AtomUI.IAtomUIBuilder builder) =>
                    AtomUI.Generated.TypeMapFixture.GeneratedControlPackageRegistration.Register(builder, static () => new Provider());
            }
            """;
        var asset = Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />");
        var generated = Run(source, asset).GeneratedTrees;
        var registration = generated.Single(tree => tree.FilePath.EndsWith("GeneratedTypeMapRegistration.g.cs", StringComparison.Ordinal));
        var invalidRegistration = System.Text.RegularExpressions.Regex.Replace(
            registration.ToString(),
            @"(?<=CompiledResourceKeyFingerprintPrefix\), )0x[0-9A-F]+UL",
            "0x0UL");
        invalidRegistration.ShouldNotBe(registration.ToString());
        var trees = generated.Select(tree => tree == registration
            ? CSharpSyntaxTree.ParseText(invalidRegistration, (CSharpParseOptions)tree.Options, tree.FilePath)
            : tree);
        using var stream = new MemoryStream();
        var emit = Compilation(source, [asset]).AddSyntaxTrees(trees).Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        emit.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        LoadCore();
        var assembly = Assembly.Load(stream.ToArray());
        var application = Activator.CreateInstance(Assembly.Load("Avalonia.Controls").GetType("Avalonia.Application")!);
        var builder = Activator.CreateInstance(LoadCore().GetType("AtomUI.AtomUIBuilder")!,
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { application }, null)!;
        var register = assembly.GetType("Demo.Probe")!.GetMethod("Register")!;

        var failure = Should.Throw<TargetInvocationException>(() => register.Invoke(null, new[] { builder }));
        failure.InnerException!.GetType().FullName.ShouldBe("AtomUI.Theme.Schema.ThemeSchemaException");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Bootstrap_Rejects_Unsupported_ABIs(int abi)
    {
        var source = "[assembly: AtomUI.Registration.ControlPackageMarker(\"Foreign.Package\", typeof(Foreign.Group), " + abi + ")] namespace Foreign { public sealed class Group { } }";
        var reference = EmitReference(Compilation(source, [], "ForeignPackage"));
        var compilation = Compilation("public class App { }", [], "Consumer").AddReferences(reference);
        var result = CSharpGeneratorDriver.Create(new ControlPackageBootstrapGenerator()).RunGenerators(compilation, TestContext.Current.CancellationToken).GetRunResult();
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG006");
    }

    [Fact]
    public void Bootstrap_Rejects_Conflicting_Package_And_Group_Identities()
    {
        var first = EmitReference(Compilation("[assembly: AtomUI.Registration.ControlPackageMarker(\"Conflict\", typeof(First.Group), 1)] namespace First { public sealed class Group { } }", [], "FirstPackage"));
        var second = EmitReference(Compilation("[assembly: AtomUI.Registration.ControlPackageMarker(\"Conflict\", typeof(Second.Group), 1)] namespace Second { public sealed class Group { } }", [], "SecondPackage"));
        var compilation = Compilation("public class App { }", [], "Consumer").AddReferences(first, second);
        var result = CSharpGeneratorDriver.Create(new ControlPackageBootstrapGenerator()).RunGenerators(compilation, TestContext.Current.CancellationToken).GetRunResult();
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG005");
        var reversed = CSharpGeneratorDriver.Create(new ControlPackageBootstrapGenerator()).RunGenerators(
            Compilation("public class App { }", [], "Consumer").AddReferences(second, first), TestContext.Current.CancellationToken).GetRunResult();
        reversed.Diagnostics.Select(d => d.ToString()).ShouldBe(result.Diagnostics.Select(d => d.ToString()));
        Source(reversed, "GeneratedControlPackageBootstrap.g.cs").ShouldBe(Source(result, "GeneratedControlPackageBootstrap.g.cs"));
    }

    [Fact]
    public void Reordered_Declarations_And_Assets_Produce_Identical_Registration()
    {
        var one = Asset("One", "<ControlTheme x:Key=\"One\" TargetType=\"local:One\" />");
        var two = Asset("Two", "<ControlTheme x:Key=\"Two\" TargetType=\"local:Two\" />");
        var first = Run("namespace Demo { public class One : Avalonia.Controls.Control { } public class Two : Avalonia.Controls.Control { } }", one, two);
        var second = Run("namespace Demo { public class Two : Avalonia.Controls.Control { } public class One : Avalonia.Controls.Control { } }", two, one);
        Source(first, "GeneratedTypeMapRegistration.g.cs").ShouldBe(Source(second, "GeneratedTypeMapRegistration.g.cs"));
    }

    [Fact]
    public void Explicit_Include_Exposes_Only_Its_Resource_Scope()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("Shared", "<SolidColorBrush x:Key=\"Brush\" Color=\"Red\" />"),
            Asset("Button", """
                <ResourceDictionary.MergedDictionaries><ResourceInclude Source="/Shared/Themes/SharedTheme.axaml" /></ResourceDictionary.MergedDictionaries>
                <ControlTheme x:Key="Button" TargetType="local:Button"><Setter Property="Background" Value="{StaticResource Brush}" /></ControlTheme>
                """));
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    }

    [Fact]
    public void Nested_StaticResource_Is_Checked_In_Nondefault_Standalone_Themes()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            new TextFile("Input/Themes/SearchButtonTheme.axaml", """
                <ControlTheme xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:Demo"
                    x:Class="Demo.SearchButtonTheme" TargetType="local:Button">
                    <Setter Property="Tag" Value="{Binding Tag, Converter={StaticResource MissingConverter}}" />
                </ControlTheme>
                """));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG004" && d.GetMessage().Contains("MissingConverter"));
    }

    private static PortableExecutableReference EmitReference(CSharpCompilation compilation)
    {
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    [Fact]
    public void Resource_Class_Platform_Guards_Both_Descriptor_And_Factory()
    {
        var result = Run("""
            namespace Demo;
            public class Chrome : Avalonia.StyledElement { }
            [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
            internal class ChromeTheme : Avalonia.Controls.ResourceDictionary { }
            """, Asset("Chrome", "<ControlTheme x:Key=\"Chrome\" TargetType=\"local:Chrome\" />", "Demo.ChromeTheme"));
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("if (global::System.OperatingSystem.IsBrowser()) { return; }");
        source.ShouldContain("internal static void AddAsset_");
        source.IndexOf("if (global::System.OperatingSystem.IsBrowser()) { return; }", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("builder.AddThemeAsset(", StringComparison.Ordinal));
    }

    [Fact]
    public void Platform_Guards_Precede_Factories_And_Combine_Supported_Platforms_As_Alternatives()
    {
        var result = Run("""
            namespace Demo;
            [System.Runtime.Versioning.SupportedOSPlatform("windows10.0")]
            [System.Runtime.Versioning.SupportedOSPlatform("macos")]
            public class Button : Avalonia.Controls.Control { }
            """, Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("IsWindowsVersionAtLeast(10, 0, 0, 0)");
        source.ShouldContain("global::System.OperatingSystem.IsMacOS()");
        source.ShouldContain(" || ");
        source.IndexOf("if (!(global::System.OperatingSystem", StringComparison.Ordinal).ShouldBeLessThan(source.IndexOf("builder.AddControl(", StringComparison.Ordinal));
    }

    [Fact]
    public void Local_Token_Owner_Is_Not_Confused_With_A_Same_Name_External_Theme_Target()
    {
        var result = Run("""
            namespace Demo;
            public class ScrollViewer : Avalonia.Controls.Control { }
            [AtomUI.Theme.DesignTokens.ControlDesignToken]
            internal sealed class ScrollViewerToken : AtomUI.Theme.DesignTokens.AbstractControlDesignToken
            {
                public double LocalHeight { get; set; }
                public override void CalculateTokenValues(bool dark) { }
            }
            """, Asset("ScrollViewer", """
                <ControlTheme x:Key="Mine" TargetType="local:ScrollViewer" />
                <ControlTheme x:Key="External" TargetType="ScrollViewer" xmlns:tokens="using:Demo.DesignTokens">
                    <Setter Property="Tag" Value="{tokens:ScrollViewerTokenResource LocalHeight}" />
                </ControlTheme>
                """));
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::Demo.ScrollViewer)");
        source.ShouldNotContain("ControlTokenIdentity.ForControl(typeof(global::Avalonia.Controls.ScrollViewer)");
    }

    [Fact]
    public void StyledElement_Theme_Target_Is_A_Resource_Only_Contract()
    {
        var result = Run("namespace Demo { public class Chrome : Avalonia.StyledElement { } }",
            Asset("Chrome", "<ControlTheme x:Key=\"{x:Type local:Chrome}\" TargetType=\"local:Chrome\" />"));
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("typeof(global::Demo.Chrome)");
        source.ShouldNotContain("CreateControlDescriptor_Chrome");
        source.ShouldNotContain("ChromeTokenKey");
    }

    [Fact]
    public void Compiled_Nested_Assets_Match_Real_Core_Fingerprints()
    {
        var (_, assembly) = Emit("namespace Demo { public class Host { public class Presenter : Avalonia.Controls.Control { } } }",
            Asset("Presenter", "<ControlTheme x:Key=\"Nested\" TargetType=\"local:Host.Presenter\" />"));
        var factories = assembly.GetType("AtomUI.Generated.TypeMapFixture.GeneratedRegistrationFactories")!;
        var descriptor = factories.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(m => m.ReturnType.FullName == "AtomUI.Theme.Schema.ControlThemeAssetDescriptor").Invoke(null, null)!;
        var core = LoadCore();
        var globals = core.GetType("AtomUI.Generated.AtomUICore.GeneratedThemeSchema")!.GetMethod("GetGlobalTokens", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null);
        var registry = core.GetType("AtomUI.Theme.Schema.ThemeSchemaRegistry")!;
        var actual = (ulong)registry.GetMethod("ComputeResourceKeySchemaFingerprint", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new[] { descriptor, globals })!;
        ((ulong)descriptor.GetType().GetProperty("ResourceKeySchemaFingerprint")!.GetValue(descriptor)!).ShouldBe(actual, "Runtime URI: " + descriptor.GetType().GetProperty("AssetUri")!.GetValue(descriptor));
        var exports = (System.Collections.IEnumerable)descriptor.GetType().GetProperty("ExportedThemes")!.GetValue(descriptor)!;
        var export = exports.Cast<object>().Single();
        ((Type)export.GetType().GetProperty("TargetType")!.GetValue(export)!).FullName.ShouldBe("Demo.Host+Presenter");
        var instance = Activator.CreateInstance(registry, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { globals!, Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ControlTokenDescriptor")!, 0), Array.CreateInstance(core.GetType("AtomUI.Theme.Schema.ThemeAlgorithmDescriptor")!, 0), TypedArray(descriptor) }, null);
        instance.ShouldNotBeNull();
    }

    [Fact]
    public void Bootstrap_Uses_Only_Resolved_Metadata_And_Full_Assembly_Identities()
    {
        var (bytes, _) = Emit("namespace Demo { public class Button : Avalonia.Controls.Control { } }", Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />"));
        var consumer = Compilation("public class Unrelated { public string Name => \"unused\"; }", [], "Consumer").AddReferences(MetadataReference.CreateFromImage(bytes));
        var result = CSharpGeneratorDriver.Create(new ControlPackageBootstrapGenerator()).RunGenerators(consumer, TestContext.Current.CancellationToken).GetRunResult();
        result.Diagnostics.ShouldBeEmpty();
        var source = Source(result, "GeneratedControlPackageBootstrap.g.cs");
        source.ShouldContain("TypeMapAssemblyTarget<global::AtomUI.Generated.TypeMapFixture.ControlPackageGroup_");
        source.ShouldContain("TypeMapFixture, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        source.ShouldNotContain("Register(");
        source.ShouldNotContain("Provider");
    }

    [Fact]
    public void Irrelevant_Source_Edit_Does_Not_Reemit_Registration_Or_Token_Output()
    {
        var assets = new[] { Asset("Button", "<ControlTheme x:Key=\"Button\" TargetType=\"local:Button\" />") };
        var compilation = Compilation("namespace Demo { public class Button : Avalonia.Controls.Control { } }", assets);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new TokenResourceKeyGenerator().AsSourceGenerator() },
            assets.ToImmutableArray<AdditionalText>(), new CSharpParseOptions(LanguageVersion.Preview), new Options(),
            new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var first = driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()).ToArray();
        driver = driver.RunGenerators(compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("internal class Unrelated { int Value => 17; }", new CSharpParseOptions(LanguageVersion.Preview), cancellationToken: TestContext.Current.CancellationToken)), TestContext.Current.CancellationToken);
        driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()).ShouldBe(first);
        driver.GetRunResult().Results.Single(r => r.TrackedSteps.ContainsKey("ControlRegistrationOutput")).TrackedSteps["ControlRegistrationOutput"].SelectMany(s => s.Outputs)
            .ShouldAllBe(output => output.Reason == IncrementalStepRunReason.Unchanged || output.Reason == IncrementalStepRunReason.Cached);
    }

    [Fact]
    public void Diagnoses_Unresolved_Static_Resources_But_Leaves_Dynamic_Host_Keys_Open()
    {
        var result = Run("namespace Demo { public class Button : Avalonia.Controls.Control { } }",
            Asset("Button", """
                <ControlTheme x:Key="Button" TargetType="local:Button">
                    <Setter Property="Tag" Value="{StaticResource MissingPrivate}" />
                    <Setter Property="Background" Value="{DynamicResource HostBackground}" />
                </ControlTheme>
                """));
        result.Diagnostics.ShouldContain(d => d.Id == "ATOMUIREG004" && d.GetMessage().Contains("MissingPrivate"));
        result.Diagnostics.ShouldNotContain(d => d.GetMessage().Contains("HostBackground"));
    }

    [Fact]
    public void Derived_Theme_Elements_And_Semantic_Style_Only_Triggers_Are_Preserved()
    {
        var result = Run("""
            namespace Demo;
            public class Button : Avalonia.Controls.Control { }
            public class FancyTheme : Avalonia.Styling.ControlTheme { }
            [AtomUI.Theme.SemanticPart("content", SelectorClass = "semantic-content", ContractType = typeof(Avalonia.Controls.Control), Since = "6.0.0", RuntimeCreated = true, SelectorRoute = ">> .semantic-content")]
            public class Panel : Avalonia.Controls.Control { }
            """, Asset("Button", "<local:FancyTheme x:Key=\"Fancy\" TargetType=\"local:Button\" />"));
        var source = Source(result, "GeneratedTypeMapRegistration.g.cs");
        source.ShouldContain("\"Fancy\"");
        source.ShouldContain("typeof(global::AtomUI.Theme.Styling.PanelContentStyle)");
        source.ShouldContain("CreateSemanticDescriptor_");
    }

    private static Array TypedArray(object value)
    {
        var result = Array.CreateInstance(value.GetType(), 1); result.SetValue(value, 0); return result;
    }

    internal static (byte[] Bytes, Assembly Assembly) Emit(string source, params TextFile[] assets)
    {
        return EmitCompilation(Compilation(source, assets), assets);
    }

    private static (byte[] Bytes, Assembly Assembly) EmitCompilation(CSharpCompilation compilation, TextFile[] assets, Options? options = null)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new TokenResourceKeyGenerator().AsSourceGenerator() },
            assets.ToImmutableArray<AdditionalText>(), new CSharpParseOptions(LanguageVersion.Preview), options ?? new Options());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        using var stream = new MemoryStream();
        var emit = output.Emit(stream);
        emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var bytes = stream.ToArray();
        LoadCore();
        return (bytes, Assembly.Load(bytes));
    }
}
