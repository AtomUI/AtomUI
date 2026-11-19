using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Dialog, MessageBox and DialogButtonBox. A Dialog holds the session; its surface (DialogSurface with the overlay header,
/// caption buttons, mask and presenter) opens in the overlay layer and has no Token identity: the surface parts inherit the
/// Dialog (or MessageBox) as Token owner and read its globals through <c>OwnerTokenResource</c>, and read the Dialog Own
/// Tokens through <c>DialogTokenResource</c>. The consumption map attributes the owner reads to <c>shared-primitive</c>, so
/// each member runs the owner reads of the parts it carries as rows of its own (drift-guarded against the theme AXAML).
/// Rows open an overlay dialog in a layer manager inside the test window and compare the surface.
/// </summary>
public partial class FeedbackFamilyEffectTests
{
    private const string DialogThemes = "src/AtomUI.Desktop.Controls/Dialog/Themes/";

    internal static readonly OwnerReadList s_dialogSurface =
        new(DialogThemes + "DialogSurfaceTheme.axaml", ["BorderRadiusLG", "BoxShadows", "FontWeightStrong"]);

    internal static readonly OwnerReadList s_overlayDialogHeader =
        new(DialogThemes + "OverlayDialogHeaderTheme.axaml", ["IconSize", "SpacingSM"]);

    internal static readonly OwnerReadList s_dialogCaptionButton = new(DialogThemes + "DialogCaptionButtonTheme.axaml",
        ["BorderRadiusSM", "ColorBgTextActive", "ColorBgTextHover", "ColorIcon", "ColorIconHover"]);

    internal static readonly OwnerReadList s_overlayDialogMask =
        new(DialogThemes + "OverlayDialogMaskTheme.axaml", ["ColorBgMask"]);

    internal static readonly OwnerReadList s_overlayDialogPresenter =
        new(DialogThemes + "OverlayDialogPresenterTheme.axaml", ["MotionDurationMid", "MotionDurationSlow"],
            "src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs");

    private static readonly OwnerReadList[] s_dialogParts =
        [s_dialogSurface, s_overlayDialogHeader, s_dialogCaptionButton, s_overlayDialogMask, s_overlayDialogPresenter];

    /// <summary>
    /// Tokens of the window-hosted surface only (the native dialog window), which the headless test window cannot capture;
    /// <see cref="Window_Hosted_Dialog_Content_Padding_Follows_The_Dialog_Tokens"/> asserts them instead.
    /// </summary>
    private static readonly HashSet<string> s_windowHostedOnly = new(StringComparer.Ordinal)
    {
        "WindowContentPadding", "PaddingContentVerticalLG"
    };

    public static IEnumerable<object[]> DialogRows() => OwnerRows("Dialog", s_dialogParts)
        .Where(static row => !s_windowHostedOnly.Contains((string)row[0]));

    /// <summary>
    /// MessageBox carries the dialog parts; its message content part (single host) reads the MessageBox TokenResource and
    /// is a map row of the MessageBox.
    /// </summary>
    public static IEnumerable<object[]> MessageBoxRows() => OwnerRows("MessageBox", s_dialogParts).Where(static row => !s_windowHostedOnly.Contains((string)row[0]));

    public static IEnumerable<object[]> DialogButtonBoxRows() => MapRows("DialogButtonBox");

    [Theory]
    [MemberData(nameof(DialogRows))]
    public void Dialog_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<Dialog> init = token switch
        {
            "LogoSize" or "SizeLG" or "SpacingSM" => static d => d.TitleIcon = new AtomUI.Icons.AntDesign.InfoCircleFilled(),
            "LoadingIndicatorMargin" or "UniformlyMargin" => static d => d.IsLoading = true,
            "ColorBgMask" => static d => d.IsModal = true,
            _ => static _ => { }
        };
        var arrange = CaptionArrange(token);
        var region  = CaptionRegion(token);
        Func<object, object>? perturb = token is "BorderRadiusLG" ? static _ => new CornerRadius(32) : null;
        var create = CreateDialogRoot(init, static () => new Dialog());
        Run(create.Root, arrange, DialogTokens.Identity, token,
            new EffectOptions { Target = create.Target, Region = region, Perturb = perturb });
    }

    [Theory]
    [MemberData(nameof(MessageBoxRows))]
    public void MessageBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var style = token switch
        {
            "ColorInfo" => MessageBoxStyle.Information,
            "ColorSuccess" => MessageBoxStyle.Success,
            "ColorError" => MessageBoxStyle.Error,
            _ => MessageBoxStyle.Warning
        };
        var arrange = CaptionArrange(token);
        var region  = CaptionRegion(token);
        Func<object, object>? perturb = token is "BorderRadiusLG" ? static _ => new CornerRadius(32) : null;
        var create = CreateDialogRoot(d =>
        {
            d.IsModal = token == "ColorBgMask";
            if (token is "LogoSize" or "SizeLG" or "SpacingSM")
            {
                d.TitleIcon = new AtomUI.Icons.AntDesign.InfoCircleFilled();
            }
            if (token is "LoadingIndicatorMargin" or "UniformlyMargin")
            {
                d.IsLoading = true;
            }
        }, () => new MessageBox { Style = style, Icon = new AtomUI.Icons.AntDesign.ExclamationCircleFilled() });
        Run(create.Root, arrange, MessageBoxTokens.Identity, token,
            new EffectOptions { Target = create.Target, Region = region, Perturb = perturb });
    }

    [Theory]
    [MemberData(nameof(DialogButtonBoxRows))]
    public void DialogButtonBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The footer button box of an open dialog.
        var create = CreateDialogRoot(static _ => { }, static () => new Dialog());
        Run(create.Root, null, DialogButtonBoxTokens.Identity, token,
            new EffectOptions
            {
                Target = root => SurfaceOf(create.Target(root)).GetVisualDescendants().OfType<DialogButtonBox>().First()
            });
    }

    [Fact]
    public void Dialog_Motion_Follows_The_Dialog_Motion_Tokens()
    {
        // The presenter reads the Dialog's durations for both the surface zoom and mask fade.
        var create = CreateDialogRoot(static _ => { }, static () => new Dialog());
        var root   = create.Root();
        WithProvider(root, provider =>
        {
            var dialog    = (Dialog)create.Target(root);
            var surface   = SurfaceOf(dialog);
            var presenter = surface.GetVisualAncestors().OfType<OverlayDialogPresenter>().First();
            var mid       = TimeSpan.FromMilliseconds(444);
            var slow      = TimeSpan.FromMilliseconds(666);
            provider.Config = new AtomUI.Theme.Configuration.ThemeConfigBuilder()
                              .WithControl(DialogTokens.Identity, new AtomUI.Theme.Configuration.ControlThemeConfigBuilder()
                                                                  .WithToken("MotionDurationMid", mid.ToString("c"))
                                                                  .WithToken("MotionDurationSlow", slow.ToString("c"))
                                                                  .Build())
                              .Build();
            WaitFor(() => presenter.OpeningMotionDuration == slow && presenter.MotionDuration == mid);
            presenter.MotionDuration.ShouldBe(mid);
            presenter.OpeningMotionDuration.ShouldBe(slow);

            provider.Config = WithControlToken(DialogTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !dialog.IsMotionEnabled);
            dialog.IsMotionEnabled.ShouldBeFalse();
            presenter.IsMotionEnabled.ShouldBeFalse();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Window_Hosted_Dialog_Content_Padding_Follows_The_Dialog_Tokens(bool messageBox)
    {
        // The native dialog window renders outside the test window; assert the padding its content frame takes.
        Dialog dialog = messageBox ? new MessageBox() : new Dialog();
        dialog.Title = "Window dialog";
        dialog.Content = new AvaloniaTextBlock { Text = "Some contents..." };
        dialog.DialogHostType = DialogHostType.Window;
        dialog.IsModal = false;
        var identity = messageBox ? MessageBoxTokens.Identity : DialogTokens.Identity;
        WithProvider(new Panel { Children = { dialog } }, provider =>
        {
            var presenter = new WindowDialogPresenter(dialog, TopLevel.GetTopLevel(dialog).ShouldNotBeNull());
            try
            {
                var show = presenter.ShowAsync(CancellationToken.None).AsTask();
                WaitFor(() => show.IsCompleted);
                var surface = presenter.FocusScope.ShouldBeOfType<DialogSurface>();
                var frame = WaitForVisual(() => surface.GetVisualDescendants().OfType<AvaloniaBorder>()
                                                       .FirstOrDefault(static b => b.Name == "ContentFrame"),
                    "The window-hosted dialog content");
                frame.Padding.ShouldBe(messageBox
                    ? ControlTokenAccessor.Capture<MessageBox>(dialog).GetOwn<Thickness>(MessageBoxTokenKind.WindowContentPadding)
                    : ControlTokenAccessor.Capture<Dialog>(dialog).GetOwn<Thickness>(DialogTokenKind.WindowContentPadding));

                var padding = new Thickness(40, 30, 40, 30);
                provider.Config = WithControlToken(identity, "WindowContentPadding", "40,30,40,30");
                WaitFor(() => frame.Padding == padding);
                frame.Padding.ShouldBe(padding);

                provider.Config = WithControlToken(identity, "PaddingContentVerticalLG", "28");
                WaitFor(() => frame.Padding.Top == 28);
                frame.Padding.Top.ShouldBe(28);
                frame.Padding.Bottom.ShouldBe(28);
            }
            finally
            {
                var close = presenter.CloseAsync().AsTask();
                WaitFor(() => close.IsCompleted);
                var dispose = presenter.DisposeAsync().AsTask();
                WaitFor(() => dispose.IsCompleted);
            }
        });
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static OwnerReadList[] DialogOwnerReadLists() => s_dialogParts;

    /// <summary>
    /// Creates a root hosting <paramref name="factory"/>'s dialog next to its anchor in a layer manager; the target opens the
    /// overlay dialog and returns it once its surface is laid out.
    /// </summary>
    private static (Func<Control> Root, Func<Control, Control> Target) CreateDialogRoot(
        Action<Dialog> init,
        Func<Dialog> factory)
    {
        Dialog? created = null;
        return (() =>
            {
                var anchor = new AvaloniaBorder { Width = 40, Height = 20 };
                var dialog = factory();
                dialog.Title           = "Basic Modal";
                dialog.Content         = new AvaloniaTextBlock { Text = "Some contents..." };
                dialog.StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel;
                dialog.IsModal         = false;
                dialog.PlacementTarget = anchor;
                init(dialog);
                created = dialog;
                return OverlayLayerHost(new Canvas { Width = 720, Height = 520, Children = { anchor, dialog } }, 720, 520);
            },
            _ =>
            {
                var dialog = created ?? throw new InvalidOperationException("The dialog root was not created.");
                dialog.IsOpen = true;
                WaitForVisual(() => SurfaceOrNull(dialog), "The dialog surface");
                return dialog;
            });
    }

    /// <summary>
    /// The caption buttons are transparent at rest: their radius, and the header padding that moves them, show on the
    /// hover background of the close button.
    /// </summary>
    private static Action<Control>? CaptionArrange(string token) => token switch
    {
        "ColorBgTextHover" or "ColorIconHover" or "BorderRadiusSM" or "PaddingContentHorizontalSM" =>
            static d => Pseudo(CaptionClose(d), ":pointerover"),
        "ColorBgTextActive" => static d => Pseudo(CaptionClose(d), ":pressed"),
        _ => null
    };

    private static Func<Control, Control> CaptionRegion(string token) => token switch
    {
        "ColorBgMask" => static d => TopLevel.GetTopLevel(d)!.GetVisualDescendants().OfType<OverlayDialogMask>().First(),
        "ColorBgTextHover" or "ColorIconHover" or "ColorBgTextActive" or "ColorIcon" or "BorderRadiusSM" or "IconSize" =>
            static d => CaptionClose(d),
        "PaddingContentHorizontalSM" => static d => SurfaceOf(d).GetVisualDescendants().OfType<OverlayDialogHeader>().First(),
        _ => static d => SurfaceOf(d)
    };

    private static DialogSurface? SurfaceOrNull(Control dialog) =>
        TopLevel.GetTopLevel(dialog)?.GetVisualDescendants().OfType<DialogSurface>()
              .FirstOrDefault(static surface => surface.IsEffectivelyVisible && surface.Bounds.Height > 0);

    private static DialogSurface SurfaceOf(Control dialog) =>
        SurfaceOrNull(dialog) ?? throw new InvalidOperationException("The dialog surface is not open.");

    private static Control CaptionClose(Control dialog) =>
        SurfaceOf(dialog).GetVisualDescendants().OfType<DialogCaptionButton>().First(static b => b.Name == "PART_CloseButton");
}
