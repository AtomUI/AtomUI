using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;

namespace AtomUI.Desktop.Controls.Primitives;

internal static class SelectingItemsControlReflectionExtensions
{
    #region 反射信息定义
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SelectingItemsControl))]
    private static readonly Lazy<MethodInfo> MarkContainerSelectedMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(SelectingItemsControl).GetMethodInfoOrThrow("MarkContainerSelected",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SelectingItemsControl))]
    private static readonly Lazy<MethodInfo> InitializeSelectionModelMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(SelectingItemsControl).GetMethodInfoOrThrow("InitializeSelectionModel",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(SelectingItemsControl))]
    private static readonly Lazy<FieldInfo> SelectionFieldInfo = new Lazy<FieldInfo>(
        () => typeof(SelectingItemsControl).GetFieldInfoOrThrow("_selection",
            BindingFlags.Instance | BindingFlags.NonPublic));
    #endregion

    public static void MarkContainerSelected(this SelectingItemsControl control, Control container, bool selected)
    {
        MarkContainerSelectedMethodInfo.Value.Invoke(control, [container, selected ]);
    }

    public static void InitializeSelectionModel(this SelectingItemsControl control, ISelectionModel model)
    {
        InitializeSelectionModelMethodInfo.Value.Invoke(control, [model]);
    }
    
    public static void SetSelection(this SelectingItemsControl selectingItemsControl, ISelectionModel? selectionModel)
    {
        SelectionFieldInfo.Value.SetValue(selectingItemsControl, selectionModel);
    }

    public static ISelectionModel? GetSelection(this SelectingItemsControl selectingItemsControl)
    {
        return SelectionFieldInfo.Value.GetValue(selectingItemsControl) as ISelectionModel;
    }
}