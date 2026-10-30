using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AtomUIRangeTimePicker = AtomUI.Desktop.Controls.RangeTimePicker;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class TimePickerSemanticPartHighlightTests
{
    public TimePickerSemanticPartHighlightTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData("popup.root", 1)]
    [InlineData("popup.container", 1)]
    [InlineData("popup.content", 1)]
    [InlineData("popup.column", 4)]
    [InlineData("popup.item", 21)]
    [InlineData("popup.footer", 1)]
    public void Highlight_Session_Resolves_Popup_Parts_On_A_Pinned_Open_RangeTimePicker(
        string partPath,
        int expectedMin)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIRangeTimePicker), out var descriptor).ShouldBeTrue();
        var part = descriptor.ShouldNotBeNull().Parts.Single(candidate => candidate.Path == partPath);

        var picker = new AtomUIRangeTimePicker
        {
            Width = 420,
            IsMotionEnabled = false,
            ClockIdentifier = AtomUI.Desktop.Controls.ClockIdentifierType.HourClock12,
            RangeStartSelectedTime = new TimeSpan(10, 9, 20),
            RangeEndSelectedTime = new TimeSpan(12, 12, 20),
            IsPickerOpen = true,
            IsPopupPinnedOpen = true
        };

        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            EnableOverlayLayer = true,
            Child = picker
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AtomUIWindow
        {
            Width = 640,
            Height = 640,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            picker.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var popup = picker.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            using var session = SemanticPartHighlightSession.Start(picker, part, registry);
            Dispatcher.UIThread.RunJobs();

            session.TotalMatchCount.ShouldBeGreaterThanOrEqualTo(expectedMin,
                $"'{partPath}' resolved {session.TotalMatchCount} targets, expected at least {expectedMin}.");
            session.HighlightedTargetCount.ShouldBeGreaterThanOrEqualTo(expectedMin,
                $"'{partPath}' created {session.HighlightedTargetCount} adorners, expected at least {expectedMin}.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }



    [Theory]
    [InlineData("popup.root", 1)]
    [InlineData("popup.container", 1)]
    [InlineData("popup.content", 1)]
    [InlineData("popup.column", 4)]
    [InlineData("popup.item", 21)]
    [InlineData("popup.footer", 1)]
    public void Preview_Hover_Highlights_Popup_Parts_End_To_End(string partPath, int expectedMin)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        var picker = new AtomUIRangeTimePicker
        {
            Width = 420,
            IsMotionEnabled = false,
            ClockIdentifier = AtomUI.Desktop.Controls.ClockIdentifierType.HourClock12,
            RangeStartSelectedTime = new TimeSpan(10, 9, 20),
            RangeEndSelectedTime = new TimeSpan(12, 12, 20),
            IsPickerOpen = true,
            IsPopupPinnedOpen = true
        };

        var preview = new SemanticPartPreview
        {
            Width = 900,
            Title = "TimePicker",
            SemanticOwnerType = typeof(AtomUIRangeTimePicker)
        };
        preview.PreviewContent = picker;

        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            EnableOverlayLayer = true,
            Child = preview
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AtomUIWindow
        {
            Width = 980,
            Height = 900,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            preview.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            preview.ActivatePreview();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var item = preview.Items.Single(candidate => candidate.Descriptor.Path == partPath);
            preview.SetHoveredPart(item, true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var adorners = window.GetVisualDescendants()
                                 .Count(visual => visual.GetType().Name == "SemanticPartAdorner");
            adorners.ShouldBeGreaterThanOrEqualTo(expectedMin,
                $"'{partPath}' hover produced {adorners} adorners, expected at least {expectedMin}.");

            preview.SetHoveredPart(item, false);
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants()
                  .Count(visual => visual.GetType().Name == "SemanticPartAdorner")
                  .ShouldBe(0, $"'{partPath}' unhover must release all adorners.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
