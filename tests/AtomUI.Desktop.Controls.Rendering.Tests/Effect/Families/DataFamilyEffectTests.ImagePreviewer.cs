using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Controls.Primitives;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// ImagePreviewer and ImageGroupPreviewer. The preview UI (cover, overlay host, viewer, switch buttons, float toolbar,
/// title bar toolbar and the native preview dialog) has no Token identity and reads <c>ImagePreviewerTokenResource</c>
/// explicitly: it lives in the overlay layer or in a separate window and hosts registered IconButtons, where owner reads would
/// resolve elsewhere. Rows read the Token on an ImagePreviewer placed next to the part and compare the part. The tokens of the
/// native preview dialog are asserted on an opened dialog (<see cref="Preview_Dialog_Follows_The_Previewer_Tokens"/>): the
/// harness captures its own test window only.
/// </summary>
public partial class DataFamilyEffectTests
{
    /// <summary>Tokens only the native preview dialog (its frame and title bar) consumes.</summary>
    private static readonly HashSet<string> s_previewDialogOnly = new(StringComparer.Ordinal)
    {
        "DialogMinWidth", "DialogMinHeight", "ColorBgLayout", "TitleBarBackgroundColor", "ColorBorderSecondary", "IconSize"
    };

    public static IEnumerable<object[]> ImagePreviewerRows() =>
        MapRows("ImagePreviewer").Where(static row => !s_previewDialogOnly.Contains((string)row[0]));

    [Theory]
    [MemberData(nameof(ImagePreviewerRows))]
    public void ImagePreviewer_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ImagePreviewerScenario(token);
        Run(scenario.Create, scenario.Arrange, ImagePreviewerTokens.Identity, token, scenario.Options);
    }

    /// <summary>A previewer (whose Tokens the rows override) with a preview part next to it on a dark surface.</summary>
    private static Control PreviewerWith(Control part, bool light = false, bool group = false)
    {
        AbstractImagePreviewer owner = group ? new ImageGroupPreviewer() : new ImagePreviewer();
        owner.CoverWidth = 40;
        owner.CoverHeight = 40;
        // Match the production cross-root relationship while retaining the original part scene and comparison region.
        // OverlayLayer adds a visual child without replacing its explicit logical owner.
        owner.Tag = part;
        return FeedbackFamilyEffectTests.OverlayLayerHost(new AvaloniaBorder
        {
            Background = light ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            Child = owner
        }, 760, 500);
    }

    private static EffectOptions PartOptions<T>(Func<T, Control>? region = null)
        where T : Control => new()
    {
        Target = static root => AttachPreviewPart(root),
        Region = previewer =>
        {
            var part = previewer.Tag is T ownedPart ? ownedPart : Descendant<T>(TopLevel.GetTopLevel(previewer)!);
            return region?.Invoke(part) ?? part;
        }
    };

    private static AbstractImagePreviewer AttachPreviewPart(Control root)
    {
        var owner = Descendant<AbstractImagePreviewer>(root);
        if (owner.Tag is Control part && part.GetVisualParent() is null)
        {
            var layer = OverlayLayerResolver.ResolveOverlayLayer(owner, TopLevel.GetTopLevel(owner), "preview effect");
            ((ISetLogicalParent)part).SetParent(owner);
            layer.Children.Add(part);
            Dispatcher.UIThread.RunJobs();
        }
        return owner;
    }

    private static ImagePreviewerCover FailedCover() => new() { Width = 200, Height = 150, IsFailed = true };

    /// <summary>A viewer showing the second of three failed images: error placeholder, both switch buttons, float toolbar.</summary>
    private static ImageViewer FailedViewer(int currentIndex = 1)
    {
        var viewer = new ImageViewer { Width = 600, Height = 360 };
        viewer.Count                = 3;
        viewer.CurrentIndex         = currentIndex;
        viewer.IsMultiImages        = true;
        viewer.IsFirstImage         = currentIndex == 0;
        viewer.IsCurrentImageFailed = true;
        return viewer;
    }

    private static Control NavButtonOf(ImageViewer viewer, string name) => Descendant<ImagePreviewNavButton>(viewer, name);

    private static Scenario ImagePreviewerScenario(string token, bool group = false)
    {
        Control PreviewerWith(Control value, bool light = false) => DataFamilyEffectTests.PreviewerWith(value, light, group);
        switch (token)
        {
            // The cover's error placeholder: fill, icon, text, spacing.
            case "ColorFillTertiary":
                return new Scenario(() => PreviewerWith(FailedCover()), null, PartOptions<ImagePreviewerCover>());
            // The hovered cover mask and its text.
            case "MaskBgColor" or "ColorTextLightSolid":
                return new Scenario(() => PreviewerWith(new ImagePreviewerCover
                    {
                        Width = 200, Height = 150, IsShowCoverMask = true, Content = new AvaloniaTextBlock { Text = "Preview" }
                    }),
                    static previewer => Pseudo(Descendant<ImagePreviewerCover>(TopLevel.GetTopLevel(previewer)!), ":pointerover"),
                    PartOptions<ImagePreviewerCover>());
            // The group previewer's covers are CoverImageWidth wide.
            case "CoverImageWidth":
                return new Scenario(() => PreviewerWith(new ImagePreviewerCover { Height = 150, IsFailed = true }),
                    null, PartOptions<ImagePreviewerCover>());
            // The overlay host: mask and close button (hovered for its hover background).
            case "ColorBgMask" or "NavButtonBgHoverColor":
                return OverlayHostScenario(hover: token == "NavButtonBgHoverColor", group: group);
            // The viewer's switch buttons: the first is disabled on the first image.
            case "PreviewOperationColorDisabled":
                return new Scenario(() => PreviewerWith(FailedViewer(currentIndex: 0)), null,
                    PartOptions<ImageViewer>(static viewer => NavButtonOf((ImageViewer)viewer, "PART_PreviousButton")));
            case "PaddingXS" or "MarginSM" or "NavButtonBgColor":
                return new Scenario(() => PreviewerWith(FailedViewer()), null,
                    PartOptions<ImageViewer>(static viewer => NavButtonOf((ImageViewer)viewer, "PART_PreviousButton")));
            // The float toolbar: a hovered action takes the hover colour (compared on that action).
            case "PreviewOperationHoverColor":
                return new Scenario(() => PreviewerWith(FailedViewer()),
                    static previewer => Pseudo(RotateLeftOf(previewer), ":pointerover"),
                    PartOptions<ImageViewer>(static viewer => Descendant<IconButton>(viewer, "PART_RotateLeftButton")));
            // The toolbar text and icons (PreviewOperationColor derives from ColorWhite), the indicator-to-actions spacing
            // and the indicator padding.
            case "PreviewOperationColor" or "ColorWhite" or "Spacing" or "FloatToolbarIndicatorPadding":
                return new Scenario(() => PreviewerWith(FailedViewer()), null,
                    PartOptions<ImageViewer>(static viewer => Descendant<ImagePreviewFloatToolbar>(viewer)));
            // The inline toolbar padding shows on the edges of its faint background: on a light surface.
            case "UniformlyPaddingLG":
                return new Scenario(() => PreviewerWith(FailedViewer(), light: true), null,
                    PartOptions<ImageViewer>(static viewer => Descendant<ImagePreviewFloatToolbar>(viewer)));
            case "FloatToolbarPadding" or "PaddingSM" or "MarginXL":
                return new Scenario(() => PreviewerWith(FailedViewer()), null, PartOptions<ImageViewer>());
            // The error placeholder's icon-to-text spacing.
            case "SpacingXXS":
                return new Scenario(() => PreviewerWith(FailedViewer()), null,
                    PartOptions<ImageViewer>(static viewer => Descendant<StackPanel>(viewer, "DefaultErrorLayout")));
            // The title bar toolbar of the preview dialog: action size, icon size, spacing and hover / pressed fills.
            case "IconSizeLG" or "ControlHeightSM" or "ColorBgTextHover" or "ColorBgTextActive":
                return new Scenario(() => PreviewerWith(new ImagePreviewToolbar { Count = 3, CurrentIndex = 1 }),
                    token switch
                    {
                        "ColorBgTextHover"  => static previewer => Pseudo(FirstToolbarButton(previewer), ":pointerover"),
                        "ColorBgTextActive" => static previewer => Pseudo(FirstToolbarButton(previewer), ":pressed"),
                        _                   => null
                    },
                    PartOptions<ImagePreviewToolbar>());
            default:
                // The viewer's error placeholder (ColorTextTertiary, ErrorIconSize / ControlHeightLG, SpacingXXS), the
                // switch button / float toolbar sizes (PreviewOperationSize / FontSizeIcon), EnableMotion.
                return new Scenario(() => PreviewerWith(FailedViewer()), null, PartOptions<ImageViewer>());
        }

        static Control RotateLeftOf(Control previewer) =>
            Descendant<IconButton>(Descendant<ImageViewer>(TopLevel.GetTopLevel(previewer)!), "PART_RotateLeftButton");

        static Control FirstToolbarButton(Control previewer) =>
            Descendant<IconButton>(Descendant<ImagePreviewToolbar>(TopLevel.GetTopLevel(previewer)!));
    }

    /// <summary>An overlay host inserted next to the previewer once the window is shown (it needs the top level).</summary>
    private static Scenario OverlayHostScenario(bool hover, bool group = false)
    {
        return new Scenario(
            () =>
            {
                AbstractImagePreviewer owner = group ? new ImageGroupPreviewer() : new ImagePreviewer();
                return FeedbackFamilyEffectTests.OverlayLayerHost(owner, 360, 220);
            },
            previewer =>
            {
                var host = new ImagePreviewerOverlayHost(TopLevel.GetTopLevel(previewer)!, (AbstractImagePreviewer)previewer);
                ((ISetLogicalParent)host).SetParent(previewer);
                var layer = OverlayLayerResolver.ResolveOverlayLayer(previewer, TopLevel.GetTopLevel(previewer), "preview effect");
                layer.Children.Add(host);
                Dispatcher.UIThread.RunJobs();
                if (hover)
                {
                    Pseudo(Descendant<IconButton>(host, "PART_CloseButton"), ":pointerover");
                }
            },
            new EffectOptions
            {
                Target = static root => AttachPreviewPart(root),
                Region = static previewer => Descendant<ImagePreviewerOverlayHost>(TopLevel.GetTopLevel(previewer)!)
            });
    }

    public static IEnumerable<object[]> ImageGroupPreviewerRows() => MapRows("ImageGroupPreviewer").Where(static row => !s_previewDialogOnly.Contains((string)row[0]));

    [Theory]
    [MemberData(nameof(ImageGroupPreviewerRows))]
    public void ImageGroupPreviewer_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ImagePreviewerScenario(token, group: true);
        Run(scenario.Create, scenario.Arrange, ImageGroupPreviewerTokens.Identity, token, scenario.Options);
    }

    [Fact]
    public void Preview_Motion_Follows_The_Previewer_Tokens()
    {
        var previewer = new ImagePreviewer();
        var group     = new ImageGroupPreviewer();
        var root = new StackPanel { Spacing = 8, Children = { previewer, group } };
        WithProvider(root, provider =>
        {
            previewer.IsMotionEnabled.ShouldBeTrue();
            group.IsMotionEnabled.ShouldBeTrue();
            provider.Config = ControlConfig(ImageGroupPreviewerTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !group.IsMotionEnabled);
            group.IsMotionEnabled.ShouldBeFalse();
            previewer.IsMotionEnabled.ShouldBeTrue();
            provider.Config = ControlConfig(ImagePreviewerTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !previewer.IsMotionEnabled);
            previewer.IsMotionEnabled.ShouldBeFalse();
            group.IsMotionEnabled.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preview_Dialog_Follows_The_Previewer_Tokens(bool group)
    {
        // The native preview dialog is a separate window under the previewer (its logical parent): its minimum size, background,
        // title bar background (TitleBarBackgroundColor, derived from ColorBorderSecondary) and title icon size (IconSize) follow
        // Controls[ImagePreviewer].
        AbstractImagePreviewer previewer = group ? new ImageGroupPreviewer() : new ImagePreviewer();
        previewer.PreviewTitleIcon = new AtomUI.Icons.AntDesign.PictureOutlined();
        previewer.ItemsSource = [new ImagePreviewItem(new BorrowedImageSource(new SolidImage()))];
        var identity = group ? ImageGroupPreviewerTokens.Identity : ImagePreviewerTokens.Identity;
        WithProvider(previewer, provider =>
        {
            previewer.OpenDialog();
            var dialog = WaitForDialog(previewer);
            try
            {
                provider.Config = new ThemeConfigBuilder()
                                  .WithControl(identity, new ControlThemeConfigBuilder()
                                                                               .WithToken("DialogMinWidth", "800")
                                                                               .WithToken("DialogMinHeight", "300")
                                                                               .WithToken("ColorBgLayout", "#ff102030")
                                                                               .WithToken("ColorBorderSecondary", "#ff405060")
                                                                               .WithToken("IconSize", "20").Build())
                                  .Build();
                WaitFor(() => dialog.MinWidth == 800);
                dialog.MinWidth.ShouldBe(800);
                dialog.MinHeight.ShouldBe(300);
                ((ISolidColorBrush)dialog.Background!).Color.ShouldBe(Color.Parse("#ff102030"));
                // Light mode derives the title bar from ColorBorderSecondary darkened by 1%.
                ((ISolidColorBrush)dialog.TitleBarFrameBackground!).Color.ShouldBe(Color.Parse("#ff405060").Darken(1));
                var titleIcon = dialog.GetVisualDescendants().OfType<IconPresenter>()
                                      .First(static presenter => presenter.Name == "PART_IconPresenter");
                titleIcon.Width.ShouldBe(20);

                provider.Config = ControlConfig(identity, "TitleBarBackgroundColor", "#ff00b96b");
                WaitFor(() => dialog.TitleBarFrameBackground is ISolidColorBrush { Color: var c } && c == Color.Parse("#ff00b96b"));
                ((ISolidColorBrush)dialog.TitleBarFrameBackground!).Color.ShouldBe(Color.Parse("#ff00b96b"));
            }
            finally
            {
                dialog.Close();
                Dispatcher.UIThread.RunJobs();
            }
        });
    }

    private static ImagePreviewerDialog WaitForDialog(AbstractImagePreviewer previewer)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            var state = typeof(AbstractImagePreviewer)
                        .GetField("_openState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                        .GetValue(previewer);
            if (state?.GetType().GetProperty("DialogHost")?.GetValue(state) is ImagePreviewerDialog { IsVisible: true } dialog)
            {
                dialog.UpdateLayout();
                return dialog;
            }
            Thread.Sleep(5);
        }

        throw new InvalidOperationException("The preview dialog did not open.");
    }

    /// <summary>A 64 x 48 opaque image.</summary>
    private sealed class SolidImage : IImage
    {
        public Size Size => new(64, 48);

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect) =>
            context.FillRectangle(Brushes.SteelBlue, destRect);
    }
}
