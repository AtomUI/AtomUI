using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls.Primitives;

internal static class TopLevelReflectionExtensions
{
    private static readonly Lazy<PropertyInfo> LastPointerPositionPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(TopLevel).GetProperty("LastPointerPosition", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(TopLevel),
            "LastPointerPosition"));

    public static PixelPoint? GetLastPointerPosition(this TopLevel topLevel)
    {
        return LastPointerPositionPropertyInfo.Value.GetValue(topLevel) as PixelPoint?;
    }
}
