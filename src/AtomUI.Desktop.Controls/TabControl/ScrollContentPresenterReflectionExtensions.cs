using System.Diagnostics;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

internal static class ScrollContentPresenterReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<MethodInfo> SnapOffsetMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(ScrollContentPresenter).GetMethod("SnapOffset", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(ScrollContentPresenter),
            "SnapOffset"));

    #endregion

    public static Vector SnapOffset(this ScrollContentPresenter scrollContentPresenter, Vector offset, Vector direction = default, bool snapToNext = false)
    {
        var value = SnapOffsetMethodInfo.Value.Invoke(scrollContentPresenter, [offset, direction, snapToNext]) as Vector?;
        Debug.Assert(value.HasValue);
        return value.Value;
    }
}
