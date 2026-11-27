using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// What the shimmer draws: the blocks it fills, the width its band travels over and the band's colors and timing.
/// <paramref name="Blocks" /> are the shimmer blocks in shimmer-layer coordinates, each with its own corner radii; the
/// host builds a new array for every scene and never changes it afterwards, so the render thread reads it without
/// locking.
/// </summary>
internal readonly record struct SkeletonShimmerScene(
    RoundedRect[] Blocks,
    double Width,
    Color From,
    Color To,
    TimeSpan Duration,
    Easing Easing);

/// <summary>
/// Starts the band; the time the message arrives is time zero of the cycle.
/// </summary>
internal readonly record struct SkeletonShimmerStart;

/// <summary>
/// Stops the band. The handler draws nothing afterwards and stops requesting frames.
/// </summary>
internal readonly record struct SkeletonShimmerStop;
