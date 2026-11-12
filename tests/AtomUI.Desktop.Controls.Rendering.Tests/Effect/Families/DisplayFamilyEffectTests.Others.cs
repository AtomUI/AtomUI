using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class DisplayFamilyEffectTests
{
    public static IEnumerable<object[]> DrawerRows() => Rows("Drawer");
    public static IEnumerable<object[]> BorderBeamRows() => Rows("BorderBeam");
    public static IEnumerable<object[]> HyperLinkRows() => Rows("HyperLinkTextBlock");
    public static IEnumerable<object[]> SelectableRows() => Rows("SelectableTextBlock");
    public static IEnumerable<object[]> AdornerRows() => Rows("AdornerLayer");

    [Theory, MemberData(nameof(DrawerRows))]
    public void Drawer_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(DrawerScenario(token), DrawerTokens.Identity, token);
    }

    private static Control DrawerFrame(Control drawer) =>
        TopLevel.GetTopLevel(drawer)!.GetVisualDescendants().OfType<DrawerContainer>().First();

    private static Scenario DrawerScenario(string token)
    {
        var placement = token switch
        {
            "BoxShadowDrawerLeft" => DrawerPlacement.Left,
            "BoxShadowDrawerUp" => DrawerPlacement.Top,
            "BoxShadowDrawerDown" => DrawerPlacement.Bottom,
            _ => DrawerPlacement.Right
        };
        return new Scenario(() => new Drawer
        {
            Width = 1, Height = 1, Title = "Details", Footer = new Avalonia.Controls.TextBlock { Text = "Footer actions", HorizontalAlignment = HorizontalAlignment.Left },
            Content = token == "PushOffsetPercent"
                ? new StackPanel { Children = { new Avalonia.Controls.TextBlock { Text = "Parent content" }, new Drawer { Width = 1, Height = 1, Title = "Child", Content = "Child content", IsMotionEnabled = false } } }
                : new Avalonia.Controls.TextBlock { Text = "Drawer content", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top },
            IsPinnedOpen = true,
            Placement = placement, SizeType = token switch
            {
                "MiddleSize" => CustomizableSizeType.Middle,
                "LargeSize" => CustomizableSizeType.Large,
                _ => CustomizableSizeType.Small
            }
        }, c =>
        {
            ((Drawer)c).IsOpen = true;
            FeedbackFamilyEffectTests.WaitForVisual(() => TopLevel.GetTopLevel(c)!.GetVisualDescendants()
                .OfType<DrawerInfoContainer>().FirstOrDefault(), "Drawer surface");
            if (token == "PushOffsetPercent")
            {
                Part<Drawer>(DrawerFrame(c)).IsOpen = true;
            }
            if (token is "ColorBgTextHover" or "ColorBgTextActive" or "BorderRadiusSM")
            {
                Pseudo(Part<IconButton>(DrawerFrame(c), "PART_CloseButton"), token == "ColorBgTextActive" ? ":pressed" : ":pointerover");
            }
        }, new EffectOptions
        {
            Region = c => DrawerRegion(c, token),
            Perturb = token switch
            {
                "SmallSize" or "MiddleSize" or "LargeSize" => static _ => new Dimension(240),
                "BoxShadowDrawerUp" or "BoxShadowDrawerDown" or "BoxShadowDrawerLeft" or "BoxShadowDrawerRight" =>
                    static _ => BoxShadows.Parse("0 0 24 12 #a0000000"),
                _ => null
            }
        });
    }

    private static Control DrawerRegion(Control drawer, string token)
    {
        var frame = DrawerFrame(drawer);
        return token switch
        {
            "ColorBgMask" => Part<Control>(frame, "PART_Mask"),
            "CloseIconPadding" or "CloseIconMargin" or "PaddingXXS" or "FontSize" or
                "ColorBgTextHover" or "ColorBgTextActive" or "BorderRadiusSM" => Part<IconButton>(frame, "PART_CloseButton"),
            "HeaderMargin" or "UniformlyMarginXS" or "UniformlyMargin" or "UniformlyMarginLG" or "FontSizeLG" or "FontHeightLG" or
                "FontWeightStrong" => Part<Control>(frame, "InfoHeader"),
            "FooterPadding" or "UniformlyPadding" or "UniformlyPaddingXS" =>
                Part<ContentPresenter>(frame, "InfoFooter").Child!,
            "ContentPadding" or "PaddingLG" => Part<ContentPresenter>(frame, "InfoContainer").Child!,
            _ => Part<DrawerInfoContainer>(frame)
        };
    }

    [Theory, MemberData(nameof(BorderBeamRows))]
    public void BorderBeam_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(new Scenario(() => new BorderBeam
        {
            Width = 160, Height = 60, BorderThickness = token == "BorderThickness" ? new Thickness(1) : new Thickness(3), Duration = TimeSpan.FromDays(1), Content = "Highlighted content"
        }, c =>
        {
            if (token == "BorderThickness")
            {
                c.ClearValue(BorderBeam.BorderThicknessProperty);
            }
            Part<BorderBeamPresenter>(c).Progress = 0.15;
        },
        new EffectOptions
        {
            Perturb = token switch
            {
                "BeamOpacity" => static _ => 0.15d,
                "BorderRadiusLG" => static _ => new CornerRadius(35),
                _ => null
            }
        }), BorderBeamTokens.Identity, token);
    }

    [Theory, MemberData(nameof(HyperLinkRows))]
    public void HyperLinkTextBlock_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(new Scenario(static () => new HyperLinkTextBlock { Text = "Open details" }, token switch
        {
            "ColorLinkHover" => static c => Pseudo(c, ":pointerover"),
            "ColorLinkActive" => static c => Pseudo(c, ":pressed"),
            "ColorTextDisabled" => static c => c.IsEnabled = false,
            _ => null
        }), HyperLinkTextBlockTokens.Identity, token);
    }

    [Theory, MemberData(nameof(SelectableRows))]
    public void SelectableTextBlock_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(new Scenario(static () => new SelectableTextBlock { Text = "Select this text", SelectionStart = 0, SelectionEnd = 6 }),
            SelectableTextBlockTokens.Identity, token);
    }

    [Theory, MemberData(nameof(AdornerRows))]
    public void AdornerLayer_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(new Scenario(static () => new VisualLayerManager
        {
            Width = 200, Height = 100,
            Child = new Button { Content = "Focus", Width = 120, Height = 40 }
        }, static c =>
        {
            // The FocusOutlineAdorner draws around its adorned element, so it is attached to the button.
            var layer  = (AdornerLayer)c;
            var button = layer.GetVisualRoot()!.GetVisualDescendants().OfType<Button>().First();
            var focus  = layer.DefaultFocusAdorner!.Build();
            AdornerLayer.SetAdornedElement(focus, button);
            layer.Children.Add(focus);
        }, new EffectOptions
        {
            Target = static c => AdornerLayer.GetAdornerLayer(c)!,
            RegionInflation = 8
        }), AdornerLayerTokens.Identity, token);
    }
}
