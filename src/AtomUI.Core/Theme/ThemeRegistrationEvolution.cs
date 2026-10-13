using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Resources;

namespace AtomUI.Theme;

/// <summary>一次启动演化的候选状态；提交前不替换任何有效查询状态。</summary>
internal sealed record ThemeRegistrationEvolution(
    ThemeTransaction Transaction,
    ThemeSchemaRegistry Registry,
    CompiledThemeCatalog Catalog,
    IReadOnlyList<ThemeDiagnostic> CatalogDiagnostics,
    long CatalogChangeGeneration,
    IReadOnlyList<ThemeDefinitionRegistration> Resolvers,
    ThemeConfigNormalizeResult RuntimeConfig,
    ThemeConfigNormalizeResult InitialConfig,
    SemanticPartRegistry SemanticParts,
    StaticThemeResourceProvider.ResourceTable StaticResources,
    ThemeManager.StaticResourceMountSnapshot PreviousResources);
