using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows a loading state over its content. While <c>IsSpinning</c> is <c>true</c> the content is dimmed or
/// blurred behind a mask and a loading indicator with an optional tip is centered over it; otherwise the
/// content is shown as is.
/// </summary>
public partial class Spin : AbstractSpin
{
    public Spin()
    {
    }
}