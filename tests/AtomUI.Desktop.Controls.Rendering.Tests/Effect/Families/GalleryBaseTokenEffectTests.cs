using AtomUI.Desktop.Controls.Rendering.Tests.Effect;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public class GalleryBaseTokenEffectTests
{
    public static IEnumerable<object[]> GlobalRows()
    {
        var rows = new Dictionary<string, string[]>
        {
            ["GalleryShowCaseHeader"] = ["ColorTextSecondary", "ColorTextTertiary", "ColorBgContainer", "ColorBorderSecondary", "BorderThickness"],
            ["ShowCaseItem"] = ["ColorBgContainer", "ColorFillTertiary", "BorderThickness"],
            ["ShowCaseZoomOverlay"] = ["ColorBgLayout", "ColorBgContainer"],
            ["SemanticPartPreview"] = ["ColorBgContainer", "ColorBorderSecondary", "ColorBgLayout", "ColorText", "ColorFillAlter", "BorderThickness", "FontSizeLG", "Padding"],
            ["GallerySelectableTextBlock"] = ["SelectionBackground", "SelectionForeground", "FontHeight"],
            ["IconInfoItem"] = ["ColorPrimaryBorder", "BorderThickness", "BorderRadius", "EnableMotion", "ColorBgTextHover", "ColorBgTextActive", "ColorTextDisabled", "MotionDurationMid", "PaddingXS"],
            ["ColorItemControl"] = ["FontSizeSM", "FontWeightStrong", "PaddingXS"]
        };
        return rows.SelectMany(pair => pair.Value.Select(token => new object[] { pair.Key, token }));
    }

    [Theory, MemberData(nameof(GlobalRows))]
    public void Migrated_Global_Changes_The_Actual_Gallery_Consumer(string id, string token)
    {
        TokenEffectHarness.AssertHasEffect(() => Create(id, token), new ControlTokenIdentity("AtomUI", id), token,
            c =>
            {
                if (id == "SemanticPartPreview")
                {
                    var info = c.GetVisualDescendants().OfType<Avalonia.Controls.Button>().First(b => b.Name == "PART_InfoButton");
                    info.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                }
                if (id == "IconInfoItem")
                {
                    ((IPseudoClasses)c.Classes).Add(token == "ColorBgTextActive" ? ":pressed" : ":pointerover");
                    if (token is "ColorPrimaryBorder" or "BorderThickness")
                    {
                        ((IPseudoClasses)c.Classes).Add(":focus-visible");
                    }
                }
            }, options: new EffectOptions
            {
                Target = id == "SemanticPartPreview" ? root => root.GetVisualDescendants().OfType<SemanticPartPreview>().Single()
                    : id == "ShowCaseZoomOverlay" && token == "ColorBgLayout" ? root => root.GetVisualDescendants().OfType<ShowCaseZoomOverlay>().Single() : null,
                Region = c => Region(c, id, token),
                Perturb = id == "IconInfoItem" && token == "BorderRadius" ? _ => new Avalonia.CornerRadius(24) : null
            });
    }

    private static Control Create(string id, string token) => id switch
    {
        "GalleryShowCaseHeader" => new GalleryShowCaseHeader { Width = 650, Title = "Components", Subtitle = "Current component subtitle", Description = "Themeable component description", Namespace = "AtomUI.Desktop.Controls", Package = "AtomUI" },
        "ShowCaseItem" => new ShowCaseItem
        {
            Width = 500, Title = "Example", Description = "A component example",
            IsDeferredContentEnabled = token is "ColorFillTertiary" or "BorderThickness",
            DeferredContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) => new Border { Width = 200, Height = 100 }),
            Content = token == "ColorBgContainer" ? new Border { Width = 200, Height = 100 } : null
        },
        "ShowCaseZoomOverlay" => Zoom(token),
        "SemanticPartPreview" => new GalleryShowCaseHost
        {
            Width = 900, Height = 520, SelectedTab = GalleryShowCaseTab.SemanticParts,
            SemanticPartsContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object>((_, _) =>
            {
                var button = new AtomUI.Desktop.Controls.Button { Content = "Example" };
                return new SemanticPartPreview { Title = "Semantic inspection", PreviewContent = button, SemanticOwner = button, SemanticOwnerType = typeof(AtomUI.Desktop.Controls.Button) };
            })
        },
        "GallerySelectableTextBlock" => new GallerySelectableTextBlock { Text = "Selected first line\nSecond line", SelectionStart = 0, SelectionEnd = 19, FontSize = 20 },
        "IconInfoItem" => new IconInfoItem { IconName = "Navigation home icon", Icon = new AtomUI.Icons.AntDesign.HomeOutlined(), IsEnabled = token != "ColorTextDisabled" },
        "ColorItemControl" => new ColorItemControl { Width = 280, ColorName = "Primary500", Hex = "#123456", Background = Brushes.White, Foreground = Brushes.Black },
        _ => throw new InvalidOperationException(id)
    };

    private static Control Zoom(string token)
    {
        var zoom = new ShowCaseZoomOverlay { Width = 700, Height = 500, IsOpen = true, Title = "Zoom example", StageContent = new Border { Width = 100, Height = 80 } };
        // The outer layout surface is visible when the card surface is explicitly transparent.
        return token == "ColorBgLayout" ? new AtomUI.Theme.ThemeConfigProvider
        {
            Config = new AtomUI.Theme.Configuration.ThemeConfigBuilder().WithControl(new ControlTokenIdentity("AtomUI", "ShowCaseZoomOverlay"),
                new AtomUI.Theme.Configuration.ControlThemeConfigBuilder().WithToken("ColorBgContainer", "#00000000").Build()).Build(),
            Child = zoom
        } : zoom;
    }

    private static Control Region(Control control, string id, string token)
    {
        var descendants = control.GetVisualDescendants().OfType<Control>().ToArray();
        var name = (id, token) switch
        {
            ("GalleryShowCaseHeader", "ColorTextSecondary") => "PART_SubtitleText",
            ("GalleryShowCaseHeader", "ColorTextTertiary") => "PART_DescriptionText",
            ("GalleryShowCaseHeader", _) => "PART_MetadataCard",
            ("ShowCaseItem", "ColorFillTertiary" or "BorderThickness") => "PART_DeferredPlaceholder",
            ("SemanticPartPreview", "ColorText" or "FontSizeLG") => "PART_Title",
            ("SemanticPartPreview", "ColorFillAlter") => "PART_DetailsPane",
            _ => null
        };
        return name is null ? control : descendants.Single(c => c.Name == name);
    }

    [Theory]
    [InlineData("ColorItemControl", "HexFontSize")]
    [InlineData("ColorItemControl", "HexOpacity")]
    [InlineData("IconInfoItem", "IconNameFontSize")]
    public void Label_Own_Token_Changes_The_Actual_Text(string id, string token)
    {
        TokenEffectHarness.AssertHasEffect(() => id == "ColorItemControl"
            ? new ColorItemControl { Width = 280, ColorName = "Primary500", Hex = "#123456", Background = Brushes.White, Foreground = Brushes.Black }
            : new IconInfoItem { IconName = "Navigation home icon", Foreground = Brushes.Black },
            new ControlTokenIdentity("AtomUI", id), token,
            c => ((IPseudoClasses)c.Classes).Add(":pointerover"),
            options: new EffectOptions
            {
                Region = c => c.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.TextBlock>()
                    .First(t => id == "IconInfoItem" || t.Name == "PART_HexTextBlock")
            });
    }
}
