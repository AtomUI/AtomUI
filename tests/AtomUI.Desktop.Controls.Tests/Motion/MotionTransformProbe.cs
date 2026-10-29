using Avalonia;
using Avalonia.Rendering.Composition;

namespace AtomUI.Desktop.Controls.Tests.Motion;

/// <summary>Reads the motion transform that Motion writes into the target's composition visual.</summary>
internal static class MotionTransformProbe
{
    /// <summary>The motion delta as scale, rotation then translation; null when the visual is untransformed.</summary>
    internal static Avalonia.Matrix? Find(Visual target)
    {
        var visual = ElementComposition.GetElementVisual(target);
        if (visual is null)
        {
            return null;
        }
        var matrix = Avalonia.Matrix.CreateScale(visual.Scale.X, visual.Scale.Y) *
                     Avalonia.Matrix.CreateRotation(visual.RotationAngle) *
                     Avalonia.Matrix.CreateTranslation(visual.Translation.X, visual.Translation.Y);
        return matrix.IsIdentity ? null : matrix;
    }

    internal static Avalonia.Matrix Matrix(Visual target) =>
        Find(target) ?? Avalonia.Matrix.Identity;

    /// <summary>The scale/rotation center in target coordinates.</summary>
    internal static Point Origin(Visual target)
    {
        var center = ElementComposition.GetElementVisual(target)?.CenterPoint ?? default;
        return new Point(center.X, center.Y);
    }
}
