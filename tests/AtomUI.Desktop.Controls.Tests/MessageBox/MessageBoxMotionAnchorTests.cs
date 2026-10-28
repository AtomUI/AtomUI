using System.Reflection;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.MessageBox;

public class MessageBoxMotionAnchorTests
{
    static MessageBoxMotionAnchorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Static_MessageBox_Only_Uses_An_Explicit_Target_As_Motion_Anchor(
        bool hasExplicitTarget,
        bool shouldUseAnchor)
    {
        var fallbackTarget = new Border();
        var explicitTarget = new Border();
        var messageBox = CreateStaticMessageBox(
            fallbackTarget,
            hasExplicitTarget
                ? new MessageBoxOptions { PlacementTarget = explicitTarget }
                : null);
        var presenter = new OverlayDialogPresenter(
            messageBox,
            hasExplicitTarget ? explicitTarget : fallbackTarget);

        var openingMotion = CreateSurfaceMotion(presenter);

        if (shouldUseAnchor)
        {
            openingMotion.ShouldBeOfType<DialogZoomInMotion>();
        }
        else
        {
            openingMotion.ShouldBeOfType<DialogZoomInMotion>();
        }
    }

    private static object CreateSurfaceMotion(OverlayDialogPresenter presenter)
    {
        var method = typeof(OverlayDialogPresenter).GetMethod(
            "CreateSurfaceMotion",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return method.Invoke(presenter, [true]).ShouldNotBeNull();
    }

    private static AtomUI.Desktop.Controls.MessageBox CreateStaticMessageBox(
        Control placementTarget,
        MessageBoxOptions? options)
    {
        var createMessageBox = typeof(AtomUI.Desktop.Controls.MessageBox).GetMethod(
            "CreateMessageBox",
            BindingFlags.Static | BindingFlags.NonPublic);
        createMessageBox.ShouldNotBeNull();
        return (AtomUI.Desktop.Controls.MessageBox)createMessageBox.Invoke(
            null,
            [new AtomUI.Desktop.Controls.TextBlock { Text = "Message" }, null, options, placementTarget])!;
    }
}
