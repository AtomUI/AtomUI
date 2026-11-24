using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class FeedbackFamilyEffectTests
{
    /// <summary>
    /// A dialog opened through a placement target is never styled in a tree, so its ControlTheme cannot pass the
    /// DialogSurface inputs down. The hosted surface maps them from its forwarded owner itself: the terminal's defaults
    /// arrive, and a scoped <c>Controls[owner]</c> override still changes the visible surface.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dialog_Opened_Through_A_Placement_Target_Maps_Its_Owner_Tokens_Onto_The_Hosted_Surface(bool messageBox)
    {
        var anchor   = new AvaloniaBorder { Width = 40, Height = 20 };
        var root     = OverlayLayerHost(new Canvas { Width = 720, Height = 520, Children = { anchor } }, 720, 520);
        var identity = messageBox ? MessageBoxTokens.Identity : DialogTokens.Identity;
        Dialog dialog = messageBox
            ? new MessageBox { Style = MessageBoxStyle.Information, Icon = new AtomUI.Icons.AntDesign.InfoCircleFilled() }
            : new Dialog();
        dialog.Title           = "Hosted";
        dialog.Content         = new AvaloniaTextBlock { Text = "Some contents..." };
        dialog.StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel;
        dialog.IsModal         = false;
        dialog.IsMotionEnabled = false;
        dialog.PlacementTarget = anchor;

        WithIsolationWindow(root, (provider, capture) =>
        {
            ((ILogical)dialog).IsAttachedToLogicalTree.ShouldBeFalse("the public path never attaches the Dialog itself");
            dialog.IsOpen = true;
            var surface = WaitForSurface(anchor);
            try
            {
                ThemeScope.GetTokenOwner(surface).ShouldBe(identity);
                var defaults = ControlTokenAccessorFor(messageBox, surface);
                DialogSurface.GetDefaultViewportMinWidth(surface).ShouldBe(defaults.MinWidth);
                DialogSurface.GetHeaderFontSize(surface).ShouldBe(defaults.HeaderFontSize);
                DialogSurface.GetContentPadding(surface).ShouldBe(defaults.ContentPadding);

                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, "HeaderBg", IsolationColor));
                using var after = capture();
                ChangedRatioOf(before, after, surface).ShouldBeGreaterThan(0d,
                    $"Controls[{identity}].HeaderBg did not reach the hosted surface");
            }
            finally
            {
                dialog.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
        });
    }

    private static (double MinWidth, double HeaderFontSize, Avalonia.Thickness ContentPadding) ControlTokenAccessorFor(
        bool messageBox, Control context)
    {
        if (messageBox)
        {
            var accessor = AtomUI.Theme.Resources.ControlTokenAccessor.Capture<MessageBox>(context);
            return (accessor.GetOwn<double>(MessageBoxTokenKind.MinWidth),
                accessor.GetOwn<double>(MessageBoxTokenKind.HeaderFontSize),
                accessor.GetOwn<Avalonia.Thickness>(MessageBoxTokenKind.ContentPadding));
        }
        var dialogAccessor = AtomUI.Theme.Resources.ControlTokenAccessor.Capture<Dialog>(context);
        return (dialogAccessor.GetOwn<double>(DialogTokenKind.MinWidth),
            dialogAccessor.GetOwn<double>(DialogTokenKind.HeaderFontSize),
            dialogAccessor.GetOwn<Avalonia.Thickness>(DialogTokenKind.ContentPadding));
    }

    private static DialogSurface WaitForSurface(Control anchor)
    {
        var topLevel = TopLevel.GetTopLevel(anchor).ShouldNotBeNull();
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            topLevel.UpdateLayout();
            var surface = topLevel.GetVisualDescendants().OfType<DialogSurface>().FirstOrDefault();
            if (surface is { } found && found.Bounds.Width > 0)
            {
                return found;
            }
            Thread.Sleep(5);
        }
        throw new InvalidOperationException("The hosted dialog surface did not appear.");
    }
}
