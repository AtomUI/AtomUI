using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the tag / badge family (docs/engineering/development/theme-effect-tests.md): Tag, CheckableTag,
/// CheckableTagGroup, CountBadge, DotBadge and RibbonBadge. Every Active Own Token and every consumed global Token of each
/// member visibly changes the member when overridden through <c>Controls[member]</c>. The rows come from the generated
/// consumption map; Tag additionally computes its status and solid preset colours in code shared with other packages
/// (<c>AbstractTag</c> in AtomUI.Controls), which the map does not attribute to Tag, so those reads are listed by hand
/// and a drift guard keeps the list equal to the code. The badge indicators (count, dot, ribbon adorners) have no Token
/// identity and a single host each; they read that host's TokenResource explicitly, so their reads are map rows of the host.
/// </summary>
public partial class TagBadgeFamilyEffectTests
{
    private const string AbstractTagSource = "src/AtomUI.Controls/Tag/AbstractTag.cs";

    /// <summary>Effective global Tokens AbstractTag reads in code for the Tag's status and solid colours.</summary>
    internal static readonly string[] s_tagCodeReads =
    [
        "ColorError", "ColorErrorBg", "ColorErrorBorder",
        "ColorInfo", "ColorInfoBg", "ColorInfoBorder",
        "ColorSuccess", "ColorSuccessBg", "ColorSuccessBorder",
        "ColorTextLightSolid",
        "ColorWarning", "ColorWarningBg", "ColorWarningBorder"
    ];

    // ---- Tag ----------------------------------------------------------------------------------

    public static IEnumerable<object[]> TagRows()
    {
        var rows = MapRows("Tag").ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in s_tagCodeReads)
        {
            if (seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    [Theory]
    [MemberData(nameof(TagRows))]
    public void Tag_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<Tag> setup = token switch
        {
            // The default solid tag: ColorBgSolid fill and its contrasting text.
            "SolidTextColor" or "ColorBgSolid" => static t => t.Variant = TagVariant.Solid,
            // The outlined default tag draws its ColorBorder frame.
            "ColorBorder" or "BorderThickness" or "BorderRadiusSM" => static t => t.Variant = TagVariant.Outlined,
            // The uncoloured outline tag draws its border and text in ColorPrimary.
            "ColorPrimary" => static t => t.Variant = TagVariant.Outline,
            "TagIconSize" or "FontSizeIcon" => static t => t.Icon = new AtomUI.Icons.AntDesign.UserOutlined(),
            "TagCloseIconSize" or "IconSizeXS" => static t => t.IsClosable = true,
            // Status colours: the solid fill, the light background, the outlined border.
            "ColorSuccess" => Status("success", TagVariant.Solid),
            "ColorSuccessBg" => Status("success", TagVariant.Filled),
            "ColorSuccessBorder" => Status("success", TagVariant.Outlined),
            "ColorInfo" => Status("processing", TagVariant.Solid),
            "ColorInfoBg" => Status("processing", TagVariant.Filled),
            "ColorInfoBorder" => Status("processing", TagVariant.Outlined),
            "ColorWarning" => Status("warning", TagVariant.Solid),
            "ColorWarningBg" => Status("warning", TagVariant.Filled),
            "ColorWarningBorder" => Status("warning", TagVariant.Outlined),
            "ColorError" => Status("error", TagVariant.Solid),
            "ColorErrorBg" => Status("error", TagVariant.Filled),
            "ColorErrorBorder" => Status("error", TagVariant.Outlined),
            // The text of a solid preset-coloured tag.
            "ColorTextLightSolid" => Status("red", TagVariant.Solid),
            _ => static _ => { }
        };

        Run(() =>
        {
            var tag = new Tag { Text = "Tag Text" };
            setup(tag);
            return tag;
        }, null, TagTokens.Identity, token);

        static Action<Tag> Status(string color, TagVariant variant) => t =>
        {
            t.TagColor = color;
            t.Variant  = variant;
        };
    }

    [Fact]
    public void Tag_Code_Read_List_Matches_AbstractTag()
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot, AbstractTagSource));
        Regex.Matches(source, @"SharedTokenKind\.(?<name>\w+)")
             .Select(static match => match.Groups["name"].Value)
             .Distinct(StringComparer.Ordinal)
             .Order(StringComparer.Ordinal)
             .ShouldBe(s_tagCodeReads.Order(StringComparer.Ordinal), AbstractTagSource);
    }

    // ---- CheckableTag -------------------------------------------------------------------------

    public static IEnumerable<object[]> CheckableTagRows() => MapRows("CheckableTag");

    [Theory]
    [MemberData(nameof(CheckableTagRows))]
    public void CheckableTag_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<CheckableTag> setup = token switch
        {
            // The checked tag: primary fill, white text and the rounded fill corners.
            "ColorPrimary" or "ColorTextLightSolid" or "BorderRadiusSM" => static t => t.IsChecked = true,
            "ColorPrimaryHover" => static t => t.IsChecked = true,
            "ColorTextDisabled" => static t => t.IsEnabled = false,
            "ColorBgContainerDisabled" => static t =>
            {
                t.IsChecked = true;
                t.IsEnabled = false;
            },
            // The icon and its gap to the text.
            "TagIconSize" or "FontSizeIcon" or "TagTextPaddingInline" or "UniformlyPaddingXXS" => static t =>
                t.Icon = new AtomUI.Icons.AntDesign.UserOutlined(),
            _ => static _ => { }
        };
        Action<Control>? arrange = token switch
        {
            "ColorFillSecondary" or "ColorPrimaryHover" => static c => Pseudo(c, ":pointerover"),
            "ColorPrimaryActive" => static c => Pseudo(c, ":pressed"),
            _ => null
        };

        Run(() =>
        {
            var tag = new CheckableTag { Content = "Checkable" };
            setup(tag);
            return tag;
        }, arrange, CheckableTagTokens.Identity, token);
    }

    // ---- CheckableTagGroup --------------------------------------------------------------------

    public static IEnumerable<object[]> CheckableTagGroupRows() => MapRows("CheckableTagGroup");

    [Theory]
    [MemberData(nameof(CheckableTagGroupRows))]
    public void CheckableTagGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // SpacingXS spaces the group's tags: the second and third tags move.
        Run(static () => new CheckableTagGroup { Options = new[] { "Movies", "Books", "Music" } },
            null, CheckableTagGroupTokens.Identity, token);
    }

    // ---- shared helpers -----------------------------------------------------------------------

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    private static IEnumerable<object[]> MapRows(string controlId) => EffectTestData.For("AtomUI", controlId).ToArray();

    private static readonly Lazy<string> s_repositoryRoot = new(static () =>
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    });

    private static string RepositoryRoot => s_repositoryRoot.Value;
}
