using AtomUI.Generated.AtomUICore;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme;

/// <summary>UI 收集结束后冻结的 schema 输入；不含资源 factory、Visual 或可变 Builder。</summary>
internal sealed record ThemeRegistrationInput(
    ControlTokenDescriptor[] Controls,
    ControlThemeAssetDescriptor[] Assets,
    PackageSchemaFingerprint[]? PackageFingerprints)
{
    internal bool CanBuildInBackground => Controls.All(static control =>
        IsRuntimeType(control.ControlType) && control.OwnTokens.All(static token =>
            IsRuntimeType(token.ValueType) && token.ResourceKey is Enum or string)) &&
        Assets.All(static asset => asset.AssetUri.GetType() == typeof(Uri) &&
            asset.ExportedThemes.All(static export => IsRuntimeType(export.TargetType) &&
                (export.ResourceKey is not Type keyType || IsRuntimeType(keyType))) &&
            asset.RequiredTokenOwners.All(static owner => owner.OwnerType is { } type && IsRuntimeType(type)) &&
            asset.SemanticThemeBindings.All(static binding => IsRuntimeType(binding.TargetType) &&
                binding.OwnerIdentity.OwnerType is { } type && IsRuntimeType(type)));

    private static bool IsRuntimeType(Type type) => type.GetType() == typeof(object).GetType();

    internal ThemeSchemaRegistry Build()
    {
        return PackageFingerprints is { } fingerprints
            ? new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), Controls,
                GeneratedThemeSchema.GetAlgorithms(), Assets, GeneratedThemeSchema.GlobalSchemaFingerprint, fingerprints)
            : new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), Controls,
                GeneratedThemeSchema.GetAlgorithms(), Assets);
    }
}
