using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The ring a <see cref="BorderBeam" /> paints on, given as border thickness and corner radius. A content that
/// implements <see cref="IBorderBeamAwareControl" /> returns it from
/// <see cref="IBorderBeamAwareControl.GetBorderBeamGeometry" />.
/// </summary>
public readonly record struct BorderBeamGeometry(
    Thickness BorderThickness,
    CornerRadius CornerRadius);
