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
