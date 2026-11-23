using Avalonia;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>
/// The beam ring must follow the content card's own frame: under large-radius themes the beam's theme radius
/// (Shadcn Luma BorderRadiusLG 26) detaches the light from a radius-8 card at every corner. A Card feeds its
/// border thickness and corner radius to the hosting BorderBeam through IBorderBeamAwareControl, and a frame
/// change flows into the beam immediately.
/// </summary>
public class BorderBeamCornerPathRenderingTests
{
    [Fact]
    public void Aware_Card_Feeds_The_Beam_Its_Own_Frame()
    {
        var beam = new BorderBeam
        {
            Width  = 300,
            Height = 110,
            Content = new Card
            {
                Header          = "Workspace overview",
                BorderThickness = new Thickness(2),
                CornerRadius    = new CornerRadius(8)
            }
        };
        var window = new AvaWindow { Width = 360, Height = 180, Content = beam };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            beam.EffectiveBorderBeamGeometry.BorderThickness.ShouldBe(new Thickness(2));
            beam.EffectiveBorderBeamGeometry.CornerRadius.ShouldBe(new CornerRadius(8));

            var card = (Card)beam.Content!;
            card.CornerRadius = new CornerRadius(14);
            Dispatcher.UIThread.RunJobs();
            beam.EffectiveBorderBeamGeometry.CornerRadius.ShouldBe(new CornerRadius(14));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
