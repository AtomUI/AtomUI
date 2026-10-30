using System.Diagnostics;
using System.Reflection;
using AtomUI.Reflection;

namespace AtomUI.Desktop.Controls.Utils;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

internal static class TextBoxReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<FieldInfo> ScrollViewerFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaTextBox).GetField(
                "_scrollViewer",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy),
            typeof(AvaloniaTextBox),
            "_scrollViewer"));
    
    internal static readonly Lazy<MethodInfo> GetVerticalSpaceBetweenScrollViewerAndPresenterMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaTextBox).GetMethod(
                "GetVerticalSpaceBetweenScrollViewerAndPresenter",
                BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaTextBox),
            "GetVerticalSpaceBetweenScrollViewerAndPresenter"));
    
    internal static readonly Lazy<MethodInfo> SnapshotUndoRedoMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaTextBox).GetMethod("SnapshotUndoRedo", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaTextBox),
            "SnapshotUndoRedo"));
    
    internal static readonly Lazy<MethodInfo> HandleTextInputMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaTextBox).GetMethod("HandleTextInput", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaTextBox),
            "HandleTextInput"));
    
    #endregion

    public static AvaloniaTextBox SetScrollViewer(this AvaloniaTextBox textBox, ScrollViewer scrollViewer)
    {
        ScrollViewerFieldInfo.Value.SetValue(textBox, scrollViewer);
        return textBox;
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
