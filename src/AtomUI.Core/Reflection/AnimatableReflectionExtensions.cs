using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Animation;

namespace AtomUI.Animations;

/// <summary>
/// Legacy public compatibility entry points; initialization uses scoped public transition properties.
/// </summary>
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

    /// <summary>
    /// Lets property changes of <paramref name="animatable" /> start transitions by calling the internal
    /// <c>Animatable.EnableTransitions</c> method of Avalonia.
    /// </summary>
    /// <exception cref="MissingMethodException">
    /// Avalonia no longer declares the internal method. It is looked up on the first call.
    /// </exception>
    public static void EnableTransitions(this Animatable animatable)
    {
        EnableTransitionsMethodInfo.Value.Invoke(animatable, []);
    }

    /// <summary>
    /// Stops property changes of <paramref name="animatable" /> from starting transitions by calling the internal
    /// <c>Animatable.DisableTransitions</c> method of Avalonia.
    /// </summary>
    /// <exception cref="MissingMethodException">
    /// Avalonia no longer declares the internal method. It is looked up on the first call.
    /// </exception>
    public static void DisableTransitions(this Animatable animatable)
    {
        DisableTransitionsMethodInfo.Value.Invoke(animatable, []);
    }
}
