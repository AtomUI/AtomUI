using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Animation;

namespace AtomUI.Animations;

/// <summary>Legacy public compatibility entry points; initialization uses scoped public transition properties.</summary>
public static class AnimatableReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<MethodInfo> EnableTransitionsMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(Animatable).GetMethod("EnableTransitions", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Animatable),
            "EnableTransitions"));

    private static readonly Lazy<MethodInfo> DisableTransitionsMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(Animatable).GetMethod("DisableTransitions", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Animatable),
            "DisableTransitions"));
    #endregion

    public static void EnableTransitions(this Animatable animatable)
    {
        EnableTransitionsMethodInfo.Value.Invoke(animatable, []);
    }

    public static void DisableTransitions(this Animatable animatable)
    {
        DisableTransitionsMethodInfo.Value.Invoke(animatable, []);
    }
}
