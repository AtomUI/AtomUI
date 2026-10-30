using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ControlThemeAssetManifestTests
{
    [Fact]
    public void Registry_Tracks_Theme_Asset_References_With_Resource_Key_Schema_Fingerprint()
    {
        var control = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([control]);
        var identity = ControlTokenIdentity.ForControl(control.ControlType, control.Identity.Catalog, control.Identity.Id);
        var binding = new ControlThemeBindingDescriptor(identity, "SearchButtonTheme", typeof(Button));
        var asset = ControlThemeAssetContractTests.Asset([new ControlThemeExportDescriptor(control.ControlType, "SearchButton")],
            [identity], bindings: [binding], globals: registry.GlobalTokens);
        var withAsset = TypedThemeSnapshotCacheTests.CreateRegistry([control], themeAssets: [asset]);
        new ControlThemeAssetManifest(withAsset, [asset]).Descriptors.ShouldBe([asset]);
        withAsset.Revision.ShouldNotBe(registry.Revision);
        asset.RequiredTokenOwners.ShouldBe([identity]);
        asset.SemanticThemeBindings.ShouldBe([binding]);
    }

    [Fact]
    public void Manifest_Rejects_Unknown_Owner_Duplicate_Uri_And_Empty_Fingerprint()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([ThemeCompilerTests.CreateCompilerButtonDescriptor()]);
        var asset = ControlThemeAssetContractTests.Asset([], [TypedIdentity(registry.Controls[0])], globals: registry.GlobalTokens);
        var missing = ControlThemeAssetContractTests.Asset([], [ControlTokenIdentity.ForControl(typeof(Button), "Tests", "Missing")], globals: registry.GlobalTokens);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(registry, [missing]));
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(registry, [asset, asset]));
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(registry,
            [new(asset.AssetId, asset.AssetUri, asset.ExportedThemes, asset.RequiredTokenOwners, asset.SemanticThemeBindings, 0)]));
    }

    [Fact]
    public void Manifest_Rejects_Resource_Key_Schema_Fingerprint_Mismatch()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([ThemeCompilerTests.CreateCompilerButtonDescriptor()]);
        var asset = ControlThemeAssetContractTests.Asset([], [TypedIdentity(registry.Controls[0])], globals: registry.GlobalTokens);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(registry,
            [new(asset.AssetId, asset.AssetUri, asset.ExportedThemes, asset.RequiredTokenOwners, asset.SemanticThemeBindings, asset.ResourceKeySchemaFingerprint ^ 1)]));
    }

    private static ControlTokenIdentity TypedIdentity(ControlTokenDescriptor control) => ControlTokenIdentity.ForControl(control.ControlType, control.Identity.Catalog, control.Identity.Id);

    [Fact]
    public void Manifest_Rejects_Unregistered_Required_Control_Identity()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([ThemeCompilerTests.CreateCompilerButtonDescriptor()]);
        var asset = ControlThemeAssetContractTests.Asset([], [TypedIdentity(registry.Controls[0]),
            ControlTokenIdentity.ForControl(typeof(TextBox), "Tests", "Missing")], globals: registry.GlobalTokens);
        Should.Throw<ThemeSchemaException>(() => new ControlThemeAssetManifest(registry, [asset]));
    }
}
