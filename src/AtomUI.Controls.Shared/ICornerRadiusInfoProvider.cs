using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// Exposes the corner radius of an element, so that another component can match its own shape to that element.
/// </summary>
public interface ICornerRadiusInfoProvider
{
    /// <summary>
    /// The radius of the corners of the element, in device-independent pixels.
    /// </summary>
    public CornerRadius CornerRadius { get; }
}