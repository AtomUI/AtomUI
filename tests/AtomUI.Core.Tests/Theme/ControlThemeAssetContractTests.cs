using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ControlThemeAssetContractTests
{
    [Theory]
    [InlineData("assetId")]
    [InlineData("uri")]
    [InlineData("key")]
    [InlineData("binding")]
    [InlineData("targetName")]
    [InlineData("ownerName")]
    public void Repair1_Generated_Contract_Rejects_Stale_Compiled_Evidence(string change)
    {
        var owner = ControlTokenIdentity.ForControl(typeof(Button), "Acme", "Button");
        var prototype = Asset([new ControlThemeExportDescriptor(typeof(Button), "Theme")], bindings:
            [new ControlThemeBindingDescriptor(owner, "ActionTheme", typeof(Border))]);
        var expected = ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(prototype, []);
        Should.Throw<ThemeSchemaException>(() => ControlThemeAssetDescriptor.CreateGenerated(
            change == "assetId" ? "Other" : prototype.AssetId,
            change == "uri" ? new Uri("avares://Acme/Other.axaml") : prototype.AssetUri,
            [new ControlThemeExportDescriptor(change == "targetName" ? typeof(TextBox) : typeof(Button), change == "key" ? "Other" : "Theme")], [],
            [new ControlThemeBindingDescriptor(change == "ownerName" ? ControlTokenIdentity.ForControl(typeof(TextBox), "Acme", "Button") : owner,
                change == "binding" ? "OtherTheme" : "ActionTheme", typeof(Border))], [], expected));
    }

    [Fact]
    public void Repair1_Generated_Old_Global_Snapshot_Is_Rejected_By_The_Actual_Registry()
    {
        var globals = AtomUI.Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens();
        var oldNames = globals.OrderBy(token => token.Slot).Select(token => token.Name).ToArray();
        oldNames[0] = "Old_" + oldNames[0];
        var prototype = Asset([new ControlThemeExportDescriptor(typeof(Button), typeof(string))]);
        var asset = ControlThemeAssetDescriptor.CreateGenerated(prototype.AssetId, prototype.AssetUri, prototype.ExportedThemes, [], [], oldNames,
            ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(prototype, oldNames));
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(new ThemeSchemaRegistry(globals, [], []), [asset]));
    }

    [Fact]
    public void Repair1_Compiled_Contract_Sorts_Independently_Of_Runtime_Assembly_Order()
    {
        Type Target(string name) => System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new(name), System.Reflection.Emit.AssemblyBuilderAccess.Run)
            .DefineDynamicModule("Main").DefineType("Demo.SameTarget", System.Reflection.TypeAttributes.Public, typeof(Control)).CreateType()!;
        var a = Target("AContractTarget");
        var z = Target("ZContractTarget");
        var first = Asset([new ControlThemeExportDescriptor(a, "Z"), new ControlThemeExportDescriptor(z, "A")]);
        var second = Asset([new ControlThemeExportDescriptor(a, "A"), new ControlThemeExportDescriptor(z, "Z")]);
        first.ExportedThemes.Select(export => export.ResourceKey).ShouldBe(["Z", "A"]);
        second.ExportedThemes.Select(export => export.ResourceKey).ShouldBe(["A", "Z"]);
        var compiledContract = ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(first, []);
        ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(second, []).ShouldBe(compiledContract);
        var materialized = ControlThemeAssetDescriptor.CreateGenerated(second.AssetId, second.AssetUri, second.ExportedThemes, [], [], [], compiledContract);
        materialized.ResourceKeySchemaFingerprint.ShouldBe(second.ResourceKeySchemaFingerprint);
    }

    [Fact]
    public void Repair1_Final_Fingerprint_Preserves_Full_Identity_When_Metadata_Names_Match()
    {
        Type Key(string assemblyName) => System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new(assemblyName), System.Reflection.Emit.AssemblyBuilderAccess.Run)
            .DefineDynamicModule("Main").DefineType("Demo.SameKey", System.Reflection.TypeAttributes.Public).CreateType()!;
        var first = Asset([new ControlThemeExportDescriptor(typeof(Button), Key("FirstKeyAssembly"))]);
        var second = Asset([new ControlThemeExportDescriptor(typeof(Button), Key("SecondKeyAssembly"))]);
        var firstContract = ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(first, []);
        var secondContract = ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(second, []);
        firstContract.ShouldBe(secondContract);
        var firstGenerated = ControlThemeAssetDescriptor.CreateGenerated(first.AssetId, first.AssetUri, first.ExportedThemes, [], [], [], firstContract);
        var secondGenerated = ControlThemeAssetDescriptor.CreateGenerated(second.AssetId, second.AssetUri, second.ExportedThemes, [], [], [], secondContract);
        firstGenerated.ResourceKeySchemaFingerprint.ShouldNotBe(secondGenerated.ResourceKeySchemaFingerprint);
        new ThemeSchemaRegistry([], [], [], [firstGenerated]).Revision.ShouldNotBe(new ThemeSchemaRegistry([], [], [], [secondGenerated]).Revision);
        var stale = new ControlThemeAssetDescriptor(second.AssetId, second.AssetUri, second.ExportedThemes, [], [], firstGenerated.ResourceKeySchemaFingerprint);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(new ThemeSchemaRegistry([], [], []), [stale]));
    }

    [Fact]
    public void Repair1_StyledElement_Export_Remains_Resource_Only()
    {
        var asset = Asset([new ControlThemeExportDescriptor(typeof(StyledChrome), typeof(string))]);
        var registry = new ThemeSchemaRegistry([], [], [], [asset]);
        new ControlThemeAssetManifest(registry, [asset]).Descriptors.ShouldHaveSingleItem();
        registry.Controls.ShouldBeEmpty();
        Should.Throw<ArgumentException>(() => new ControlTokenDescriptor(typeof(StyledChrome), new("Acme", "Chrome")));
        Should.Throw<ArgumentException>(() => new ControlThemeExportDescriptor(typeof(object), "Invalid"));
    }

    private sealed class StyledChrome : Avalonia.StyledElement;

    [Fact]
    public void Internal_Resource_Only_Export_Does_Not_Require_A_Token_Descriptor()
    {
        var asset = Asset([new ControlThemeExportDescriptor(typeof(InternalPresenter), typeof(InternalPresenter))]);
        var registry = new ThemeSchemaRegistry([], [], [], [asset]);
        new ControlThemeAssetManifest(registry, [asset]).Descriptors.ShouldHaveSingleItem();
        registry.Controls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Required_Owners_Canonicalize_Before_Dedup_And_Reject_Wrong_Type(bool typedFirst)
    {
        var raw = new ControlTokenIdentity("Acme", "Button");
        var typed = ControlTokenIdentity.ForControl(typeof(TextBox), "Acme", "Button");
        var asset = Asset([], typedFirst ? [typed, raw] : [raw, typed]);
        asset.RequiredTokenOwners.ShouldHaveSingleItem().OwnerType.ShouldBe(typeof(TextBox));
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry([], [new ControlTokenDescriptor(typeof(Button), raw)], [], [asset]));
        Should.Throw<ThemeSchemaException>(() => Asset([], [typed, ControlTokenIdentity.ForControl(typeof(Button), "Acme", "Button")]));
    }

    [Fact]
    public void New_Asset_Requires_Typed_Token_Owners()
    {
        Should.Throw<ArgumentException>(() => Asset([], [new ControlTokenIdentity("Acme", "Button")]));
    }

    [Fact]
    public void Fingerprint_And_Revision_Include_Exports_And_Semantic_Bindings()
    {
        var first = Asset([new ControlThemeExportDescriptor(typeof(Button), "Named")]);
        var second = Asset([new ControlThemeExportDescriptor(typeof(Button), "Different")]);
        first.ResourceKeySchemaFingerprint.ShouldNotBe(second.ResourceKeySchemaFingerprint);
        new ThemeSchemaRegistry([], [], [], [first]).Revision.ShouldNotBe(new ThemeSchemaRegistry([], [], [], [second]).Revision);
        var stale = new ControlThemeAssetDescriptor(first.AssetId, first.AssetUri, second.ExportedThemes, [], [], first.ResourceKeySchemaFingerprint);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(new ThemeSchemaRegistry([], [], []), [stale]));
    }

    [Fact]
    public void Semantic_Binding_Alone_Changes_Fingerprint_And_Revision_And_Rejects_Stale_Fingerprint()
    {
        var owner = ControlTokenIdentity.ForControl(typeof(Button), "Acme", "Button");
        var first = Asset([new ControlThemeExportDescriptor(typeof(Button), "Named")], bindings:
            [new ControlThemeBindingDescriptor(owner, "FirstTheme", typeof(InternalPresenter))]);
        var second = Asset(first.ExportedThemes, bindings:
            [new ControlThemeBindingDescriptor(owner, "SecondTheme", typeof(InternalPresenter))]);
        first.ResourceKeySchemaFingerprint.ShouldNotBe(second.ResourceKeySchemaFingerprint);
        new ThemeSchemaRegistry([], [], [], [first]).Revision.ShouldNotBe(new ThemeSchemaRegistry([], [], [], [second]).Revision);
        var stale = new ControlThemeAssetDescriptor(first.AssetId, first.AssetUri, second.ExportedThemes, [], second.SemanticThemeBindings, first.ResourceKeySchemaFingerprint);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(new ThemeSchemaRegistry([], [], []), [stale]));
    }

    internal static ControlThemeAssetDescriptor Asset(
        IEnumerable<ControlThemeExportDescriptor> exports,
        IEnumerable<ControlTokenIdentity>? owners = null,
        string id = "Acme/Theme",
        IEnumerable<ControlThemeBindingDescriptor>? bindings = null,
        IReadOnlyList<TokenDescriptor>? globals = null)
    {
        var provisional = new ControlThemeAssetDescriptor(id, new Uri($"avares://Acme/{id}.axaml"), exports, owners ?? [], bindings ?? [], 1);
        return new ControlThemeAssetDescriptor(id, provisional.AssetUri, provisional.ExportedThemes, provisional.RequiredTokenOwners,
            provisional.SemanticThemeBindings, ThemeSchemaRegistry.ComputeResourceKeySchemaFingerprint(provisional, globals ?? []));
    }

    private sealed class InternalPresenter : Control;
}
