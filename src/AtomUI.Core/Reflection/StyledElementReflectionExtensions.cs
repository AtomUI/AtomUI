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
    
    #endregion
    
    public static void SetTemplatedParent(this StyledElement styledElement, AvaloniaObject? templateParent)
    {
        TemplatedParentPropertyInfo.Value.SetValue(styledElement, templateParent);
    }
}
