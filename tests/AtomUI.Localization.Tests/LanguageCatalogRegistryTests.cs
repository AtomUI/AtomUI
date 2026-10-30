using AtomUI.Localization.Data;
using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LanguageCatalogRegistryTests
{
    [Fact]
    public void Freeze_Assigns_Deterministic_Catalog_Slots()
    {
        var builder = new LocalizationBuilder();
        var beta = CreateCatalog<BetaResourceKind>("Acme:Beta", "Value");
        var alpha = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "Value");
        builder.AddCatalog(beta);
        builder.AddCatalog(alpha);
        builder.AddTranslationBundle(CreateBundle(beta, LanguageTags.EnUS, ["Beta"]));
        builder.AddTranslationBundle(CreateBundle(alpha, LanguageTags.EnUS, ["Alpha"]));

        var registry = builder.FreezeRegistry();

        registry.Catalogs.Select(static catalog => catalog.CatalogId)
                .ShouldBe(["Acme:Alpha", "Acme:Beta"]);
        registry.TryGetCatalogSlot(typeof(AlphaResourceKind), out var alphaSlot).ShouldBeTrue();
        registry.TryGetCatalogSlot(typeof(BetaResourceKind), out var betaSlot).ShouldBeTrue();
        alphaSlot.ShouldBe(0);
        betaSlot.ShouldBe(1);
    }

    [Fact]
    public void Freeze_Rejects_Duplicate_Catalog_Id_Or_Resource_Type()
    {
        var duplicateId = new LocalizationBuilder();
        duplicateId.AddCatalog(CreateCatalog<AlphaResourceKind>("Acme:Shared", "Value"));
        duplicateId.AddCatalog(CreateCatalog<BetaResourceKind>("Acme:Shared", "Value"));

        Should.Throw<LanguageCatalogException>(() => duplicateId.FreezeRegistry())
              .Message.ShouldContain("Acme:Shared");

        var duplicateType = new LocalizationBuilder();
        duplicateType.AddCatalog(CreateCatalog<AlphaResourceKind>("Acme:First", "Value"));
        duplicateType.AddCatalog(CreateCatalog<AlphaResourceKind>("Acme:Second", "Value"));

        Should.Throw<LanguageCatalogException>(() => duplicateType.FreezeRegistry())
              .Message.ShouldContain(typeof(AlphaResourceKind).FullName!);
    }

    [Fact]
    public void Freeze_Rejects_Bundle_For_Unknown_Catalog()
    {
        var builder = new LocalizationBuilder();
        builder.AddTranslationBundle(new TranslationBundleDescriptor(
            "Acme:Missing",
            LanguageTags.EnUS,
            TranslationSourceKind.ModuleBuiltIn,
            "Acme",
            ["Value"]));

        var exception = Should.Throw<LanguageCatalogException>(() => builder.FreezeRegistry());

        exception.Message.ShouldContain("Acme:Missing");
        exception.Message.ShouldContain("not registered");
    }

    [Fact]
    public void Freeze_Rejects_Bundle_Slot_Count_Mismatch()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "First", "Second");
        var wrongSlots = new LocalizationBuilder();
        wrongSlots.AddCatalog(catalog);
        wrongSlots.AddTranslationBundle(CreateBundle(catalog, LanguageTags.EnUS, ["First"]));

        Should.Throw<LanguageCatalogException>(() => wrongSlots.FreezeRegistry())
              .Message.ShouldContain("value slots");
    }

    [Fact]
    public void Freeze_Requires_Complete_English_Source()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "First", "Second");
        var builder = new LocalizationBuilder();
        builder.AddCatalog(catalog);
        builder.AddTranslationBundle(CreateBundle(catalog, LanguageTags.EnUS, ["First", null]));

        var exception = Should.Throw<LanguageCatalogException>(() => builder.FreezeRegistry());

        exception.Message.ShouldContain("Acme:Alpha");
        exception.Message.ShouldContain("Second");
        exception.Message.ShouldContain("en-US");
    }

    [Fact]
    public void Freeze_Rejects_Same_Priority_Unit_Conflict()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "First", "Second");
        var builder = new LocalizationBuilder();
        builder.AddCatalog(catalog);
        builder.AddTranslationBundle(CreateBundle(catalog, LanguageTags.EnUS, ["First", "Second"]));
        builder.AddTranslationBundle(CreateBundle(
            catalog,
            LanguageTags.ZhCN,
            ["第一", null],
            TranslationSourceKind.StaticLanguagePack,
            "Pack.One"));
        builder.AddTranslationBundle(CreateBundle(
            catalog,
            LanguageTags.ZhCN,
            ["冲突", "第二"],
            TranslationSourceKind.StaticLanguagePack,
            "Pack.Two"));

        var exception = Should.Throw<LanguageCatalogException>(() => builder.FreezeRegistry());

        exception.Message.ShouldContain("Acme:Alpha");
        exception.Message.ShouldContain("First");
        exception.Message.ShouldContain("zh-CN");
        exception.Message.ShouldContain("Pack.One");
        exception.Message.ShouldContain("Pack.Two");
    }

    [Fact]
    public void Freeze_Allows_NonOverlapping_Values_At_The_Same_Priority()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "First", "Second");
        var builder = new LocalizationBuilder();
        builder.AddCatalog(catalog);
        builder.AddTranslationBundle(CreateBundle(catalog, LanguageTags.EnUS, ["First", "Second"]));
        builder.AddTranslationBundle(CreateBundle(
            catalog,
            LanguageTags.ZhCN,
            ["第一", null],
            TranslationSourceKind.StaticLanguagePack,
            "Pack.One"));
        builder.AddTranslationBundle(CreateBundle(
            catalog,
            LanguageTags.ZhCN,
            [null, "第二"],
            TranslationSourceKind.StaticLanguagePack,
            "Pack.Two"));

        var registry = builder.FreezeRegistry();

        registry.GetBundles(0).Count.ShouldBe(3);
    }

    [Fact]
    public void Freeze_Allows_Higher_Priority_Overlap()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "Value");
        var builder = new LocalizationBuilder();
        builder.AddCatalog(catalog);
        builder.AddTranslationBundle(CreateBundle(catalog, LanguageTags.EnUS, ["Built in"]));
        builder.AddTranslationBundle(CreateBundle(
            catalog,
            LanguageTags.EnUS,
            ["Override"],
            TranslationSourceKind.ApplicationOverride,
            "App.Override"));

        var registry = builder.FreezeRegistry();

        registry.GetBundles(0).Count.ShouldBe(2);
    }

    [Fact]
    public void Freeze_Is_Independent_Of_Registration_Order()
    {
        var catalog = CreateCatalog<AlphaResourceKind>("Acme:Alpha", "Value");
        var english = CreateBundle(catalog, LanguageTags.EnUS, ["English"]);
        var chinese = CreateBundle(catalog, LanguageTags.ZhCN, ["中文"]);

        var first = new LocalizationBuilder();
        first.AddTranslationBundle(chinese);
        first.AddCatalog(catalog);
        first.AddTranslationBundle(english);

        var second = new LocalizationBuilder();
        second.AddCatalog(catalog);
        second.AddTranslationBundle(english);
        second.AddTranslationBundle(chinese);

        first.FreezeRegistry().GetBundles(0).Select(BundleIdentity)
             .ShouldBe(second.FreezeRegistry().GetBundles(0).Select(BundleIdentity));
    }

    [Fact]
    public void Builder_Rejects_Registration_After_Freeze()
    {
        var builder = new LocalizationBuilder();
        builder.FreezeRegistry();

        Should.Throw<InvalidOperationException>(() =>
            builder.AddCatalog(CreateCatalog<AlphaResourceKind>("Acme:Alpha", "Value")));
    }

    [Fact]
    public void Data_Catalog_Type_Resolves_By_Assembly_And_Metadata_Name()
    {
        var registry = CreateDataRegistry(typeof(DeltaResourceKind).Assembly.GetName().Name!, typeof(DeltaResourceKind).FullName!);

        registry.TryGetCatalogSlot(typeof(DeltaResourceKind), out var slot).ShouldBeTrue();
        registry.TryGetCatalogSlot(typeof(DeltaResourceKind), out var cachedSlot).ShouldBeTrue();

        slot.ShouldBe(0);
        cachedSlot.ShouldBe(0);
    }

    [Fact]
    public void Data_Catalog_Does_Not_Match_The_Same_Metadata_Name_From_Another_Assembly()
    {
        var registry = CreateDataRegistry("Other.Assembly", typeof(DeltaResourceKind).FullName!);

        registry.TryGetCatalogSlot(typeof(DeltaResourceKind), out _).ShouldBeFalse();
        registry.TryGetCatalogSlot(typeof(DeltaResourceKind), out _).ShouldBeFalse();
    }

    [Fact]
    public void Unregistered_Key_Types_Miss_Without_Allocating_After_First_Lookup()
    {
        var registry = CreateDataRegistry(typeof(DeltaResourceKind).Assembly.GetName().Name!, typeof(DeltaResourceKind).FullName!);
        // Avalonia 以控件 Type 查找 ControlTheme（键类型为 RuntimeType），以字符串或 token enum 查找其它资源。
        Type[] keyTypes = [typeof(Type).GetType(), typeof(string), typeof(GammaResourceKind)];
        foreach (var keyType in keyTypes)
        {
            registry.TryGetCatalogSlot(keyType, out _).ShouldBeFalse();
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var found = false;
        for (var index = 0; index < 100; index++)
        {
            foreach (var keyType in keyTypes)
            {
                found |= registry.TryGetCatalogSlot(keyType, out _);
            }
        }

        found.ShouldBeFalse();
        (GC.GetAllocatedBytesForCurrentThread() - allocated).ShouldBe(0);
    }

    private static LanguageCatalogRegistry CreateDataRegistry(string assemblyName, string metadataName)
    {
        var catalog = new LanguageDataCatalogDescriptor(new LanguageModuleCatalog(
            "Acme:Delta",
            assemblyName,
            metadataName,
            [new LanguageModuleUnit("Value", 0, false)]));
        return LanguageCatalogRegistry.Create(
            [catalog],
            [CreateBundle(catalog, LanguageTags.EnUS, ["Delta"])]);
    }

    private static string BundleIdentity(TranslationBundleDescriptor bundle)
    {
        return $"{bundle.Language.Value}:{bundle.SourceKind}:{bundle.SourceIdentity}";
    }

    private static LanguageCatalogDescriptor<TResourceKind> CreateCatalog<TResourceKind>(
        string id,
        params string[] unitNames)
        where TResourceKind : struct, Enum
    {
        return new LanguageCatalogDescriptor<TResourceKind>(
            id,
            unitNames.Select(name => new LanguageCatalogUnitDescriptor(name)).ToArray(),
            static key => Convert.ToInt32(key) - 1);
    }

    private static TranslationBundleDescriptor CreateBundle(
        LanguageCatalogDescriptor catalog,
        LanguageTag language,
        IReadOnlyList<string?> values,
        TranslationSourceKind sourceKind = TranslationSourceKind.ModuleBuiltIn,
        string sourceIdentity = "Acme")
    {
        return new TranslationBundleDescriptor(
            catalog.CatalogId,
            language,
            sourceKind,
            sourceIdentity,
            values);
    }

    private enum AlphaResourceKind
    {
        First,
        Second
    }

    private enum BetaResourceKind
    {
        Value
    }

    private enum GammaResourceKind
    {
        Value
    }

    private enum DeltaResourceKind
    {
        Value
    }
}
