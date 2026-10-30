using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LanguageCatalogDescriptorTests
{
    [Fact]
    public void LanguageCatalogAttribute_Targets_Enums()
    {
        var usage = typeof(LanguageCatalogAttribute)
                    .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
                    .Cast<AttributeUsageAttribute>()
                    .ShouldHaveSingleItem();
        usage.ValidOn.ShouldBe(AttributeTargets.Enum);
        usage.AllowMultiple.ShouldBeFalse();
        usage.Inherited.ShouldBeFalse();
    }

    [Fact]
    public void Catalog_Descriptor_Exposes_Strongly_Typed_Unit_Slots()
    {
        var descriptor = CreateCatalog();

        descriptor.CatalogId.ShouldBe("Acme.App:Acme.LoginLangResourceKind");
        descriptor.ResourceKindType.ShouldBe(typeof(LoginLangResourceKind));
        descriptor.Units.Select(static unit => unit.Key).ShouldBe(["Title", "ItemCount"]);
        descriptor.Units[1].IsFormatted.ShouldBeTrue();
        descriptor.TryGetUnitSlot(LoginLangResourceKind.Title, out var titleSlot).ShouldBeTrue();
        titleSlot.ShouldBe(0);
        descriptor.TryGetUnitSlot((LoginLangResourceKind)999, out _).ShouldBeFalse();
    }

    [Fact]
    public void Catalog_Descriptor_Defensively_Copies_Units()
    {
        var units = new[]
        {
            new LanguageCatalogUnitDescriptor("Title")
        };
        var descriptor = new LanguageCatalogDescriptor<LoginLangResourceKind>(
            "Acme.App:Acme.LoginLangResourceKind",
            units,
            static key => key == LoginLangResourceKind.Title ? 0 : -1);

        units[0] = new LanguageCatalogUnitDescriptor("Changed");

        descriptor.Units[0].Key.ShouldBe("Title");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("missing-module-separator")]
    public void Catalog_Descriptor_Rejects_Invalid_Identity(string catalogId)
    {
        Should.Throw<ArgumentException>(() => new LanguageCatalogDescriptor<LoginLangResourceKind>(
            catalogId,
            [new LanguageCatalogUnitDescriptor("Title")],
            static _ => 0));
    }

    [Fact]
    public void Catalog_Descriptor_Rejects_Duplicate_Unit_Keys()
    {
        Should.Throw<ArgumentException>(() => new LanguageCatalogDescriptor<LoginLangResourceKind>(
            "Acme.App:Acme.LoginLangResourceKind",
            [
                new LanguageCatalogUnitDescriptor("Title"),
                new LanguageCatalogUnitDescriptor("Title")
            ],
            static _ => 0));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Unit_Descriptor_Rejects_Invalid_Key(string key)
    {
        Should.Throw<ArgumentException>(() => new LanguageCatalogUnitDescriptor(key));
    }

    [Fact]
    public void Translation_Bundle_Exposes_Compiled_Partial_Values()
    {
        string?[] values = ["Sign in", null];
        var bundle = new TranslationBundleDescriptor(
            "Acme.App:Acme.LoginLangResourceKind",
            LanguageTags.EnUS,
            TranslationSourceKind.ModuleBuiltIn,
            "Acme.App",
            values);

        values[0] = "Changed";

        bundle.CatalogId.ShouldBe("Acme.App:Acme.LoginLangResourceKind");
        bundle.Language.ShouldBe(LanguageTags.EnUS);
        bundle.SourceKind.ShouldBe(TranslationSourceKind.ModuleBuiltIn);
        bundle.SourceIdentity.ShouldBe("Acme.App");
        bundle.Values.ShouldBe(["Sign in", null]);
    }

    [Fact]
    public void Translation_Bundle_Rejects_Invalid_Metadata()
    {
        Should.Throw<ArgumentException>(() => new TranslationBundleDescriptor(
            "invalid",
            LanguageTags.EnUS,
            TranslationSourceKind.ModuleBuiltIn,
            "Acme.App",
            ["Title"]));
        Should.Throw<ArgumentException>(() => new TranslationBundleDescriptor(
            "Acme.App:Acme.LoginLangResourceKind",
            default,
            TranslationSourceKind.ModuleBuiltIn,
            "Acme.App",
            ["Title"]));
        Should.Throw<ArgumentOutOfRangeException>(() => new TranslationBundleDescriptor(
            "Acme.App:Acme.LoginLangResourceKind",
            LanguageTags.EnUS,
            (TranslationSourceKind)byte.MaxValue,
            "Acme.App",
            ["Title"]));
        Should.Throw<ArgumentException>(() => new TranslationBundleDescriptor(
            "Acme.App:Acme.LoginLangResourceKind",
            LanguageTags.EnUS,
            TranslationSourceKind.ModuleBuiltIn,
            " ",
            ["Title"]));
    }

    private static LanguageCatalogDescriptor<LoginLangResourceKind> CreateCatalog()
    {
        return new LanguageCatalogDescriptor<LoginLangResourceKind>(
            "Acme.App:Acme.LoginLangResourceKind",
            [
                new LanguageCatalogUnitDescriptor("Title"),
                new LanguageCatalogUnitDescriptor("ItemCount", isFormatted: true)
            ],
            static key => key switch
            {
                LoginLangResourceKind.Title => 0,
                LoginLangResourceKind.ItemCount => 1,
                _ => -1
            });
    }

    [LanguageCatalog]
    private enum LoginLangResourceKind
    {
        Title,
        ItemCount
    }
}
