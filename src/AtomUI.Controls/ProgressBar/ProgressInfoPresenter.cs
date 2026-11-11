using Avalonia;
using Avalonia.Controls.Presenters;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Presenter for <see cref="AbstractProgressBar.ProgressInfoContent"/>. It sits in a Canvas whose desired size never
/// changes, so a content size change (a resized control, longer templated text, a new template) would not reach the
/// progress bar's own layout. The presenter reports it to the owning progress bar instead.
/// </summary>
internal sealed class ProgressInfoPresenter : ContentPresenter
{
    private Size? _lastReportedSize;

    protected override Type StyleKeyOverride => typeof(ContentPresenter);

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (_lastReportedSize != size)
        {
            _lastReportedSize = size;
            (TemplatedParent as AbstractProgressBar)?.HandleProgressInfoContentResized();
        }

        return size;
    }
}
