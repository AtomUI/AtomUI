using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Mentions: its input is a nested, registered TextArea (covered by the TextArea rows, as the AutoComplete inputs are), so
/// its own Tokens are the candidate popup. The popup opens in an overlay layer inside the test window; popup Tokens compare
/// the popup frame. The candidate list and its items are Token-owner transparent (see the isolation facts).
/// </summary>
public partial class DataEntryFamilyEffectTests
{
    /// <summary>A Mentions in an overlay-enabled layer manager; <paramref name="many"/> options overflow the popup.</summary>
    internal static Control CreateMentionsHost(bool many = false) =>
        FeedbackFamilyEffectTests.OverlayLayerHost(CreateMentions(many), 420, 480);

    internal static Mentions CreateMentions(bool many = false) =>
        new()
        {
            Width = 240,
            OptionsSource = (many ? Enumerable.Range(0, 24).Select(static index => $"user{index}") : ["ada", "bob"])
                            .Select(static name => (IMentionOption)new MentionOption { Header = name, Value = name })
                            .ToArray()
        };

    private static Mentions MentionsOf(Control root) => root.GetVisualDescendants().OfType<Mentions>().First();

    /// <summary>Opens the candidate popup and waits until its options are laid out.</summary>
    internal static void OpenMentions(Control mentions)
    {
        ((Mentions)mentions).IsDropDownOpen = true;
        FeedbackFamilyEffectTests.WaitForVisual(
            () => TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<CandidateListItem>().FirstOrDefault(),
            "The Mentions candidate popup");
    }

    internal static Control MentionsPopupFrame(Control mentions) =>
        TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<Border>()
                .First(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible);

    /// <summary>Shows the loading indicator in the popup (the async loader sets IsLoading; its setter is private).</summary>
    private static void ShowMentionsLoading(Control mentions)
    {
        // Opening populates the options and clears IsLoading, so the popup opens first.
        OpenMentions(mentions);
        typeof(Mentions).GetProperty(nameof(Mentions.IsLoading), BindingFlags.Instance | BindingFlags.Public)
                        .ShouldNotBeNull()
                        .SetValue(mentions, true);
        FeedbackFamilyEffectTests.WaitForVisual(
            () => TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<Spin>()
                          .FirstOrDefault(static spin => spin.IsEffectivelyVisible),
            "The Mentions loading indicator");
    }

    public static IEnumerable<object[]> MentionsRows() => MapRows("Mentions");

    [Theory]
    [MemberData(nameof(MentionsRows))]
    public void Mentions_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(MentionsScenario(token), MentionsTokens.Identity, token);
    }

    // CandidateList and CandidateListItem reuse these two themes. These are their actual owner reads, independent of the
    // Mentions consumption-map rows; LineWidth is suppressed by CandidateListItemTheme's zero BorderThickness in all states.
    // The selected check reads ItemSelectedIndicatorColor, which CandidateListTheme feeds from the owner's ColorPrimary.
    private static readonly InputFamilyEffectTests.OwnerReadList s_candidateList = new(
        "src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxTheme.axaml",
        ["EnableMotion", "ColorSplit", "BorderThickness", "BorderRadiusLG", "PaddingXL", "BorderRadius", "Padding", "BorderRadiusSM"]);

    private static readonly InputFamilyEffectTests.OwnerReadList s_candidateItem = new(
        "src/AtomUI.Desktop.Controls/ListBox/Themes/ListBoxItemTheme.axaml",
        ["LineWidth", "ColorSplit", "IconSize", "ControlHeightLG", "ControlHeight", "ControlHeightSM"]);

    public static IEnumerable<object[]> CandidateOwnerRows() =>
        s_candidateList.Tokens.Concat(s_candidateItem.Tokens).Append("ColorPrimary").Distinct(StringComparer.Ordinal)
                       .Where(static token => token != "LineWidth").Order(StringComparer.Ordinal)
                       .Select(static token => new object[] { token });

    [Fact]
    public void Candidate_Owner_Reads_Match_The_Actual_Base_Themes()
    {
        foreach (var list in new[] { s_candidateList, s_candidateItem })
        {
            ThemeOwnerReads.Read(list.Theme).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal));
        }
    }

    [Theory]
    [MemberData(nameof(CandidateOwnerRows))]
    public void Mentions_Candidate_Owner_Token_Has_Effect(string token)
    {
        var popup = new EffectOptions
        {
            Target = static root => MentionsOf(root),
            Region = token == "ColorPrimary"
                ? static mentions => Descendant<IconTemplatePresenter>(
                    TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<CandidateListItem>().First(), "SelectedIndicator")
                : static mentions => MentionsPopupFrame(mentions),
            Perturb = token.StartsWith("BorderRadius", StringComparison.Ordinal)
                ? static _ => new CornerRadius(40)
                : null
        };
        Run(new Scenario(static () => CreateMentionsHost(), mentions =>
        {
            OpenMentions(mentions);
            var list = TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<CandidateList>().First();
            // Exercise the list's supported public visual options in the real popup. No additional Token bindings are
            // introduced: the candidate base themes already own these reads.
            list.IsBorderless = false;
            list.SizeType = token is "BorderRadiusLG" or "ControlHeightLG" or "PaddingXL"
                ? CustomizableSizeType.Large
                : token is "BorderRadiusSM" or "ControlHeightSM" ? CustomizableSizeType.Small : CustomizableSizeType.Middle;
            if (token is "ColorPrimary" or "IconSize")
            {
                list.IsShowSelectedIndicator = true;
                list.SelectedIndex = 0;
            }
            if (token is "Padding" or "PaddingXL")
            {
                list.EmptyIndicator = "No matching people";
                list.ItemsSource = Array.Empty<IMentionOption>();
            }
        }, popup), MentionsTokens.Identity, token);
    }

    [Fact]
    public void Candidate_Item_Line_Width_Is_Suppressed_By_The_Candidate_Theme()
    {
        var host = CreateMentionsHost();
        WithProvider(host, _ =>
        {
            var mentions = MentionsOf(host);
            OpenMentions(mentions);
            var item = TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<CandidateListItem>().First();
            item.BorderThickness.ShouldBe(default);
            item.IsSplitLineEffectiveVisible.ShouldBeFalse();
        });
    }

    private static Scenario MentionsScenario(string token)
    {
        var popup = new EffectOptions
        {
            Target = static root => MentionsOf(root),
            Region = static mentions => MentionsPopupFrame(mentions)
        };
        return token switch
        {
            // The option height bounds the popup height (option height x displayed count): it shows with more options.
            "OptionHeight" or "ControlHeight" => new Scenario(static () => CreateMentionsHost(many: true), OpenMentions, popup),
            // The loading indicator's distance from the frame.
            "MarginLG" => new Scenario(static () => CreateMentionsHost(), ShowMentionsLoading, popup),
            // MinPopupWidth, PopupContentPadding / UniformlyPaddingXXS, ColorBgElevated, EnableMotion (read back).
            _ => new Scenario(static () => CreateMentionsHost(), OpenMentions, popup)
        };
    }
}
