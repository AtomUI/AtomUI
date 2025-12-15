using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using ShimSkiaSharp;
using SvgControl = Avalonia.Svg.Svg;

namespace AtomUI.Controls.Utils;

internal static class SvgControlRefectionExtensions
{
    #region 反射信息定义

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(SvgControl))]
    private static readonly Lazy<FieldInfo> PictureFieldInfo = new Lazy<FieldInfo>(
        () => typeof(SvgControl).GetFieldInfoOrThrow("_picture",
            BindingFlags.Instance | BindingFlags.NonPublic));

    #endregion
    
    public static SKPicture? GetSKPicture(this SvgControl svgControl)
    {
        return PictureFieldInfo.Value.GetValue(svgControl) as SKPicture;
    }
}