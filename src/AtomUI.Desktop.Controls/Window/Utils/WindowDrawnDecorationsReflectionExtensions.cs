using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;

namespace AtomUI.Desktop.Controls;

internal static class WindowDrawnDecorationsReflectionExtensions
{
    private static readonly Lazy<FieldInfo?> TopLevelHostFieldInfo = new(() =>
        typeof(TopLevel).GetField("_topLevelHost", BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency("_decorations", "Avalonia.Controls.TopLevelHost", "Avalonia.Controls")]
    private static readonly Lazy<FieldInfo?> DecorationsFieldInfo = new(() =>
        Type.GetType("Avalonia.Controls.TopLevelHost, Avalonia.Controls")?.GetField(
            "_decorations",
            BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicFields,
        "Avalonia.Controls.TopLevelHost",
        "Avalonia.Controls")]
    private static readonly Lazy<FieldInfo?> ResizeGripsFieldInfo = new(() =>
        Type.GetType("Avalonia.Controls.TopLevelHost, Avalonia.Controls")?.GetField(
            "_resizeGrips",
            BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicProperties,
        "Avalonia.Controls.Chrome.ResizeGripLayer",
        "Avalonia.Controls")]
    private static readonly Lazy<PropertyInfo?> GripThicknessPropertyInfo = new(() =>
        Type.GetType("Avalonia.Controls.Chrome.ResizeGripLayer, Avalonia.Controls")?.GetProperty(
            "GripThickness",
            BindingFlags.Instance | BindingFlags.NonPublic));

    public static bool TryTakeOverManagedResizeGrip(
        this TopLevel topLevel,
        double scale,
        out Thickness gripThickness)
    {
        if (!double.IsFinite(scale) || scale < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        gripThickness = default;
        var topLevelHost = TopLevelHostFieldInfo.Value?.GetValue(topLevel);
        if (topLevelHost is null ||
            DecorationsFieldInfo.Value?.GetValue(topLevelHost) is not WindowDrawnDecorations decorations ||
            ResizeGripsFieldInfo.Value?.GetValue(topLevelHost) is not { } resizeGrips ||
            GripThicknessPropertyInfo.Value is not { } gripThicknessProperty)
        {
            return false;
        }

        var frame  = decorations.FrameThickness;
        var shadow = decorations.ShadowThickness;
        gripThickness = new Thickness(
            (frame.Left + shadow.Left) * scale,
            (frame.Top + shadow.Top) * scale,
            (frame.Right + shadow.Right) * scale,
            (frame.Bottom + shadow.Bottom) * scale);

        gripThicknessProperty.SetValue(resizeGrips, default(Thickness));
        return true;
    }

    internal static Thickness GetDrawnDecorationsFrameThickness(this TopLevel topLevel)
    {
        return GetDrawnDecorations(topLevel)?.FrameThickness ?? default;
    }

    internal static double GetDrawnDecorationsTitleBarHeight(this TopLevel topLevel)
    {
        return GetDrawnDecorations(topLevel)?.TitleBarHeight ?? 0;
    }

    private static WindowDrawnDecorations? GetDrawnDecorations(TopLevel topLevel)
    {
        var topLevelHost = TopLevelHostFieldInfo.Value?.GetValue(topLevel);
        return topLevelHost is null
            ? null
            : DecorationsFieldInfo.Value?.GetValue(topLevelHost) as WindowDrawnDecorations;
    }
}
