using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;

namespace AtomUI.Reflection;

internal static class StyledElementReflectionExtensions
{
    #region 反射信息定义
    
    private static readonly Lazy<PropertyInfo> TemplatedParentPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(StyledElement).GetProperty("TemplatedParent", BindingFlags.Instance | BindingFlags.Public),
            typeof(StyledElement),
            "TemplatedParent"));

    private static readonly Lazy<MethodInfo> TemplatedParentThemeInvalidation = new(ResolveTemplatedParentThemeInvalidation);

    [DynamicDependency("OnTemplatedParentControlThemeChanged", typeof(StyledElement))]
    private static MethodInfo ResolveTemplatedParentThemeInvalidation()
    {
        return FixedMemberReflection.RequireMethod(
            typeof(StyledElement).GetMethod("OnTemplatedParentControlThemeChanged", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(StyledElement), "OnTemplatedParentControlThemeChanged");
    }
    
    #endregion
    
    public static void SetTemplatedParent(this StyledElement styledElement, AvaloniaObject? templateParent)
    {
        TemplatedParentPropertyInfo.Value.SetValue(styledElement, templateParent);
    }

    internal static void ReleaseTemplatedParentTheme(this StyledElement styledElement)
    {
        // Logical detach removes Style frames, but leaves TemplatedParentTheme
        // frames subscribed to the former owner. Use only at template retirement.
        TemplatedParentThemeInvalidation.Value.Invoke(styledElement, null);
    }
}
