using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class DataFamilyEffectTests
{
    /// <summary>
    /// A time picker presenter shown alone (the popup root in production) has no styled picker above it; it maps the
    /// TimeView inputs from the TimePicker identity itself, and a scoped <c>Controls[TimePicker]</c> override changes
    /// the visible time view.
    /// </summary>
    [Fact]
    public void Time_Picker_Presenter_Shown_Alone_Maps_The_TimePicker_Tokens_Onto_Its_TimeView()
    {
        var picker    = new HostlessTimePicker { ClockIdentifier = ClockIdentifierType.HourClock24 };
        var presenter = picker.CreatePresenter();
        WithIsolationWindow(presenter, (provider, capture) =>
        {
            var timeView = presenter.GetVisualDescendants().OfType<TimeView>().Single();
            ThemeScope.GetTokenOwner(presenter).ShouldBe(TimePickerTokens.Identity);
            var expected = ControlTokenAccessor.Capture<TimePicker>(presenter).GetOwn<double>(TimePickerTokenKind.ItemHeight);
            expected.ShouldBeGreaterThan(0);
            timeView.ItemHeight.ShouldBe(expected);

            using var before = capture();
            provider.Config = IsolationConfig(control: (TimePickerTokens.Identity, "ItemHeight", "60"));
            using var after = capture();
            timeView.ItemHeight.ShouldBe(60d);
            ChangedRatioOf(before, after, timeView).ShouldBeGreaterThan(0d, "Controls[TimePicker].ItemHeight did not reach the time view");
        });
    }

    /// <summary>
    /// A cover created alone follows the ImagePreviewer identity: its inputs are the previewer's defaults, and a scoped
    /// <c>Controls[ImagePreviewer]</c> override changes the visible cover.
    /// </summary>
    [Fact]
    public void Image_Cover_Created_Alone_Maps_The_ImagePreviewer_Tokens_Onto_Itself()
    {
        // Top-left aligned so the loading presenter's CoverImageWidth minimum, not the host, decides the cover's size.
        var cover = new ImagePreviewerCover
        {
            IsShowCoverMask     = true,
            IsLoading           = true,
            Content             = "preview",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top
        };
        var host  = new AvaloniaBorder { Width = 420, Height = 420, Child = cover };
        WithIsolationWindow(host, (provider, capture) =>
        {
            var accessor = ControlTokenAccessor.Capture<ImagePreviewer>(cover);
            ImageViewer.GetThemeCoverImageWidth(cover).ShouldBe(accessor.GetOwn<double>(ImagePreviewerTokenKind.CoverImageWidth));
            ImageViewer.GetThemeMaskBgColor(cover).ShouldNotBeNull();
            var mask = cover.GetVisualDescendants().OfType<AvaloniaBorder>().Single(static border => border.Name == "Mask");
            mask.Background.ShouldNotBeNull();

            using var before = capture();
            provider.Config = IsolationConfig(control: (ImagePreviewerTokens.Identity, "CoverImageWidth", "300"));
            using var after = capture();
            ImageViewer.GetThemeCoverImageWidth(cover).ShouldBe(300d);
            ChangedRatioOf(before, after, host).ShouldBeGreaterThan(0d, "Controls[ImagePreviewer].CoverImageWidth did not reach the standalone cover");
        });
    }

    private sealed class HostlessTimePicker : TimePicker
    {
        public TimePickerPresenter CreatePresenter()
        {
            var presenter = CreatePickerPresenter().ShouldBeAssignableTo<TimePickerPresenter>().ShouldNotBeNull();
            NotifyPickerPresenterCreated(presenter);
            NotifyPickerOpened();
            return presenter;
        }
    }
}
