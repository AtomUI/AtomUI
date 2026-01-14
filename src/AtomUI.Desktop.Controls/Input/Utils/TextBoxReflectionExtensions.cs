using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls.Utils;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

internal static class TextBoxReflectionExtensions
{
    #region 反射信息定义
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(AvaloniaTextBox))]
    private static readonly Lazy<FieldInfo> ScrollViewerFieldInfo = new Lazy<FieldInfo>(() => 
        typeof(AvaloniaTextBox).GetFieldInfoOrThrow("_scrollViewer",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(AvaloniaTextBox))]
    private static readonly Lazy<FieldInfo> TextPresenterFieldInfo = new Lazy<FieldInfo>(() => 
        typeof(AvaloniaTextBox).GetFieldInfoOrThrow("_presenter",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(AvaloniaTextBox))]
    internal static readonly Lazy<MethodInfo> ScrollViewerScrollChangedMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(AvaloniaTextBox).GetMethodInfoOrThrow("ScrollViewer_ScrollChanged",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(AvaloniaTextBox))]
    internal static readonly Lazy<MethodInfo> GetVerticalSpaceBetweenScrollViewerAndPresenterMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(AvaloniaTextBox).GetMethodInfoOrThrow("GetVerticalSpaceBetweenScrollViewerAndPresenter",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(AvaloniaTextBox))]
    internal static readonly Lazy<MethodInfo> SnapshotUndoRedoMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(AvaloniaTextBox).GetMethodInfoOrThrow("SnapshotUndoRedo",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(AvaloniaTextBox))]
    internal static readonly Lazy<MethodInfo> HandleTextInputMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(AvaloniaTextBox).GetMethodInfoOrThrow("HandleTextInput",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    #endregion

    public static ScrollViewer? GetScrollViewer(this AvaloniaTextBox textBox)
    {
        return ScrollViewerFieldInfo.Value.GetValue(textBox) as ScrollViewer;
    }
    
    public static AvaloniaTextBox SetScrollViewer(this AvaloniaTextBox textBox, ScrollViewer scrollViewer)
    {
        ScrollViewerFieldInfo.Value.SetValue(textBox, scrollViewer);
        return textBox;
    }
    
    public static TextPresenter GetTextPresenter(this AvaloniaTextBox textBox)
    {
        var textPresenter = TextPresenterFieldInfo.Value.GetValue(textBox) as TextPresenter;
        Debug.Assert(textPresenter != null);
        return textPresenter;
    }
    
    public static void HandleScrollChanged(this AvaloniaTextBox textBox, object? sender, ScrollChangedEventArgs e)
    { 
        ScrollViewerScrollChangedMethodInfo.Value.Invoke(textBox, [sender, e]);
    }

    public static double GetVerticalSpaceBetweenScrollViewerAndPresenter(this AvaloniaTextBox textBox)
    {
        var result = GetVerticalSpaceBetweenScrollViewerAndPresenterMethodInfo.Value.Invoke(textBox, []) as double?;
        Debug.Assert(result != null);
        return result.Value;
    }

    public static void SnapshotUndoRedo(this AvaloniaTextBox textBox, bool ignoreChangeCount = true)
    {
        SnapshotUndoRedoMethodInfo.Value.Invoke(textBox, [ignoreChangeCount]);
    }
    
    public static void HandleTextInput(this AvaloniaTextBox textBox, string? input)
    {
        HandleTextInputMethodInfo.Value.Invoke(textBox, [input]);
    }
}