using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIPopup = AtomUI.Desktop.Controls.Popup;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Popups;

public class PopupMarginPlacementTests
{
    static PopupMarginPlacementTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, true, true, true)]
    public void Popup_Flip_Prediction_Uses_Content_Size_Without_Child_Margin(
        bool csd, bool horizontal, bool hasMargin, bool checkNativePrediction)
    {
        var margin = hasMargin ? new Thickness(6, 10, 14, 22) : default;
        var child = new Border { Width = 120, Height = 60, Margin = margin };
        var target = new Border { Width = 40, Height = 30 };
        var canvas = new Canvas();
        canvas.Children.Add(target);
        var popup = new AtomUIPopup
        {
            PlacementTarget = target,
            RequestedPlacement = horizontal ? PlacementMode.Right : PlacementMode.Bottom,
            ShouldUseOverlayLayer = true,
            Child = child
        };
        canvas.Children.Add(popup);
        var window = new AtomUIWindow { Width = 480, Height = 360, Content = canvas };
        var actualDeflate = default(Thickness);
        var nativePrediction = (flipX: false, flipY: false);
        popup.CustomPlacementCallback = (placement, _, _, _, _, _) =>
        {
            actualDeflate = placement.Deflate;
            if (checkNativePrediction)
            {
                nativePrediction = PopupUtils.CalculatePopupRootFlipInfo(placement);
            }
        };

        try
        {
            window.Show();
            // Windows only supplies a CSD Window template. Exercise the supported
            // non-CSD Linux template in this headless placement case, preserving
            // the same AtomUI host, content and placement geometry.
            if (!csd && window.OsType == OsType.Windows)
            {
                window.SetValue(AtomUIWindow.OsTypeProperty, OsType.Linux);
            }
            window.IsCsdEnabled = csd;
            if (csd)
            {
                window.FrameShadowThickness = new Thickness(20, 20, 20, 40);
            }
            window.UpdateLayout();
            if (checkNativePrediction)
            {
                // Align the client and screen edges to exercise the native prediction's
                // screen coordinates with the same real placement request.
                var workingArea = window.Screens.Primary!.WorkingArea;
                window.Position = new PixelPoint(
                    workingArea.Right - (int)window.ClientSize.Width,
                    workingArea.Bottom - (int)window.ClientSize.Height);
            }

            // Preserve AtomUI's shadow/motion hierarchy while using the ContentControl
            // presenter contract supported by application-supplied popup host themes.
            window.TryFindResource(typeof(OverlayPopupHost), out var theme).ShouldBeTrue();
            window.Resources[typeof(OverlayPopupHost)] = new ControlTheme
            {
                TargetType = typeof(OverlayPopupHost),
                BasedOn = theme.ShouldBeOfType<ControlTheme>(),
                Setters =
                {
                    new Setter(TemplatedControl.TemplateProperty,
                        new FuncControlTemplate<OverlayPopupHost>((host, scope) =>
                            new PopupMotionActor
                            {
                                ClipToBounds = false,
                                Content = new VisualLayerManager
                                {
                                    ClipToBounds = false,
                                    Child = new ShadowsAwareContainer
                                    {
                                        IsOverlayMode = true,
                                        Child = new ContentPresenter
                                        {
                                            Name = "PART_ContentPresenter",
                                            Content = host.Content,
                                            ContentTemplate = host.ContentTemplate
                                        }.RegisterInNameScope(scope)
                                    }
                                }
                            }))
                }
            };
            Canvas.SetLeft(target, 100);
            Canvas.SetTop(target, 80);
            popup.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame();
            Dispatcher.UIThread.RunJobs();

            var host = window.GetVisualDescendants().OfType<OverlayPopupHost>().Single();
            host.Presenter.ShouldNotBeNull();
            host.Presenter!.Child.ShouldBeSameAs(child);
            actualDeflate.ShouldBe(margin);
            var hostSize = host.Bounds.Size;
            var visibleFrame = new Rect(window.ClientSize);
            if (csd)
            {
                visibleFrame = visibleFrame.Deflate(window.FrameShadowThickness);
            }
            var canvasOrigin = canvas.TranslatePoint(default, window)!.Value;
            var extra = horizontal ? margin.Left + margin.Right : margin.Top + margin.Bottom;
            var hostLength = horizontal ? hostSize.Width : hostSize.Height;
            var remainingSpace = hasMargin ? hostLength - extra / 2 : hostLength + 8;
            if (horizontal)
            {
                Canvas.SetLeft(target, visibleFrame.Right - remainingSpace - target.Width - canvasOrigin.X);
            }
            else
            {
                Canvas.SetTop(target, visibleFrame.Bottom - remainingSpace - target.Height - canvasOrigin.Y);
            }
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame();
            Dispatcher.UIThread.RunJobs();

            var targetBounds = new Rect(target.Bounds.Size)
                .TransformToAABB(target.TransformToVisual(window)!.Value);
            var childBounds = new Rect(child.Bounds.Size)
                .TransformToAABB(child.TransformToVisual(window)!.Value);
            var diagnostics = $"host={host.Bounds}, child={childBounds}, target={targetBounds}, " +
                              $"frame={visibleFrame}, margin={margin}, deflate={actualDeflate}";
            child.Bounds.Size.ShouldBe(new Size(120, 60), diagnostics);
            if (checkNativePrediction)
            {
                nativePrediction.ShouldBe((false, false), diagnostics);
            }
            popup.IsHorizontalFlipped.ShouldBeFalse(diagnostics);
            popup.IsVerticalFlipped.ShouldBeFalse(diagnostics);
            if (horizontal)
            {
                childBounds.Left.ShouldBeGreaterThanOrEqualTo(targetBounds.Right, diagnostics);
                childBounds.Right.ShouldBeLessThanOrEqualTo(visibleFrame.Right, diagnostics);
            }
            else
            {
                childBounds.Top.ShouldBeGreaterThanOrEqualTo(targetBounds.Bottom, diagnostics);
                childBounds.Bottom.ShouldBeLessThanOrEqualTo(visibleFrame.Bottom, diagnostics);
            }
        }
        finally
        {
            popup.IsOpen = false;
            window.Close();
        }
    }
}
