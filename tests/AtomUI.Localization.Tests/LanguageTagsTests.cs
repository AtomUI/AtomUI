using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LanguageTagsTests
{
    [Fact]
    public void Common_Language_Tags_Are_Strongly_Typed_And_Canonical()
    {
        var tags = new Dictionary<string, LanguageTag>
        {
            [nameof(LanguageTags.EnUS)] = LanguageTags.EnUS,
            [nameof(LanguageTags.EnGB)] = LanguageTags.EnGB,
            [nameof(LanguageTags.ZhCN)] = LanguageTags.ZhCN,
            [nameof(LanguageTags.ZhHans)] = LanguageTags.ZhHans,
            [nameof(LanguageTags.ZhTW)] = LanguageTags.ZhTW,
            [nameof(LanguageTags.ZhHant)] = LanguageTags.ZhHant,
            [nameof(LanguageTags.JaJP)] = LanguageTags.JaJP,
            [nameof(LanguageTags.KoKR)] = LanguageTags.KoKR,
            [nameof(LanguageTags.ArSA)] = LanguageTags.ArSA,
            [nameof(LanguageTags.SrLatnRS)] = LanguageTags.SrLatnRS
        };

        tags.ShouldBe(new Dictionary<string, LanguageTag>
        {
            ["EnUS"] = LanguageTag.Parse("en-US"),
            ["EnGB"] = LanguageTag.Parse("en-GB"),
            ["ZhCN"] = LanguageTag.Parse("zh-CN"),
            ["ZhHans"] = LanguageTag.Parse("zh-Hans"),
            ["ZhTW"] = LanguageTag.Parse("zh-TW"),
            ["ZhHant"] = LanguageTag.Parse("zh-Hant"),
            ["JaJP"] = LanguageTag.Parse("ja-JP"),
            ["KoKR"] = LanguageTag.Parse("ko-KR"),
            ["ArSA"] = LanguageTag.Parse("ar-SA"),
            ["SrLatnRS"] = LanguageTag.Parse("sr-Latn-RS")
        });
    }

    [Fact]
    public void Standard_Definitions_Use_Pinned_Metadata()
    {
        StandardLanguageDefinitions.TryCreate(LanguageTags.ArSA, out var definition).ShouldBeTrue();

        definition.ShouldNotBeNull();
        definition.Tag.ShouldBe(LanguageTags.ArSA);
        definition.FormattingCulture.Name.ShouldBe("ar-SA");
        definition.NativeName.ShouldBe("العربية (المملكة العربية السعودية)");
        definition.TextDirection.ShouldBe(LanguageTextDirection.RightToLeft);
    }

    [Theory]
    [InlineData("af", "af", "Afrikaans", LanguageTextDirection.LeftToRight)]
    [InlineData("af-NA", "af-NA", "Afrikaans (Namibië)", LanguageTextDirection.LeftToRight)]
    [InlineData("zh-CN", "zh-Hans-CN", "简体中文（中国大陆）", LanguageTextDirection.LeftToRight)]
    [InlineData("zu-ZA", "zu-ZA", "isiZulu (iNingizimu Afrika)", LanguageTextDirection.LeftToRight)]
    public void Standard_Definitions_Resolve_Exact_Records_At_Table_Boundaries(
        string tag,
        string cultureName,
        string nativeName,
        LanguageTextDirection direction)
    {
        StandardLanguageDefinitions.TryCreate(LanguageTag.Parse(tag), out var definition).ShouldBeTrue();

        definition.ShouldNotBeNull();
        definition.Tag.ShouldBe(LanguageTag.Parse(tag));
        definition.FormattingCulture.Name.ShouldBe(cultureName);
        definition.NativeName.ShouldBe(nativeName);
        definition.TextDirection.ShouldBe(direction);
    }

    [Fact]
    public void Standard_Definitions_Reject_Default_And_Unknown_Private_Tags()
    {
        StandardLanguageDefinitions.TryCreate(default, out var defaultDefinition).ShouldBeFalse();
        defaultDefinition.ShouldBeNull();

        StandardLanguageDefinitions.TryCreate(
            LanguageTag.Parse("en-x-acme"),
            out var privateDefinition).ShouldBeFalse();
        privateDefinition.ShouldBeNull();
    }
}
