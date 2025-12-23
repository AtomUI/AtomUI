using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class Dialog
{
    public static object? ShowDialog<TView, TViewModel>(TViewModel? dataContext, DialogOptions? options = null, TopLevel? topLevel = null)
        where TView : Control, new()
    {
        var dialogManager = FindDialogManager(topLevel);
        var dialog        = CreateDialog(new TView(), dataContext, options);
        if (options?.DialogHostType == DialogHostType.Window)
        {
            dialog.PlacementTarget = dialogManager;
        }
        dialogManager.Children.Add(dialog);
        var result = dialog.Open();
        dialogManager.Children.Remove(dialog);
        return result;
    }

    public static object? ShowDialog(Control content, object? dataContext = null, DialogOptions? options = null, TopLevel? topLevel = null)
    {
        var dialogManager = FindDialogManager(topLevel);
        var dialog        = CreateDialog(content, dataContext, options);
        if (options?.DialogHostType == DialogHostType.Window)
        {
            dialog.PlacementTarget = dialogManager;
        }
        dialogManager.Children.Add(dialog);
        var result = dialog.Open();
        dialogManager.Children.Remove(dialog);
        return result;
    }
    
    public static async Task ShowDialogAsync<TView, TViewModel>(TViewModel? dataContext, DialogOptions? options = null, TopLevel? topLevel = null)
        where TView : Control, new()
    {
        var dialogManager = FindDialogManager(topLevel);
        var dialog        = CreateDialog(new TView(), dataContext, options);
        if (options?.DialogHostType == DialogHostType.Window)
        {
            dialog.PlacementTarget = dialogManager;
        }
        dialog.Closed += (_, _) => dialogManager.Children.Remove(dialog);
        dialogManager.Children.Add(dialog);
        await Dispatcher.UIThread.InvokeAsync(async () => await dialog.OpenAsync());
    }

    public static async Task ShowDialogAsync(Control content, object? dataContext = null, DialogOptions? options = null, TopLevel? topLevel = null)
    {
        var dialogManager = FindDialogManager(topLevel);
        var dialog        = CreateDialog(content, dataContext, options);
        if (options?.DialogHostType == DialogHostType.Window)
        {
            dialog.PlacementTarget = dialogManager;
        }
        dialog.Closed += (_, _) => dialogManager.Children.Remove(dialog);
        dialogManager.Children.Add(dialog);
        await Dispatcher.UIThread.InvokeAsync(async () => await dialog.OpenAsync());
    }

    private static Dialog CreateDialog(Control content, object? dataContext, DialogOptions? options)
    {
        var dialog = new Dialog
        {
            Title                     = options?.Title,
            TitleIcon                 = options?.TitleIcon,
            IsLightDismissEnabled     = options?.IsLightDismissEnabled ?? false,
            IsModal                   = options?.IsModal ?? true,
            IsResizable               = options?.IsResizable ?? false,
            IsClosable                = options?.IsClosable ?? true,
            IsMaximizable             = options?.IsMaximizable ?? false,
            IsMinimizable             = options?.IsMinimizable ?? true,
            IsDragMovable             = options?.IsDragMovable ?? false,
            IsFooterVisible           = options?.IsFooterVisible ?? true,
            PlacementTarget           = options?.PlacementTarget,
            HorizontalOffset          = options?.HorizontalOffset,
            VerticalOffset            = options?.VerticalOffset,
            DialogHostType            = options?.DialogHostType ?? DialogHostType.Overlay,
            StandardButtons           = options?.StandardButtons ?? DialogStandardButton.NoButton,
            DefaultStandardButton     = options?.DefaultStandardButton ?? DialogStandardButton.Ok,
            HorizontalStartupLocation = options?.HorizontalStartupLocation ?? DialogHorizontalAnchor.Custom,
            VerticalStartupLocation   = options?.VerticalStartupLocation ?? DialogVerticalAnchor.Custom,
            Content                   = content,
            DataContext               = dataContext,
            Width                     = options?.Width ?? Double.NaN,
            Height                    = options?.Height ?? Double.NaN,
            MinWidth                  = options?.MinWidth ?? 0d,
            MinHeight                 = options?.MinHeight ?? 0d,
            MaxWidth                  = options?.MaxWidth ?? Double.PositiveInfinity,
            MaxHeight                 = options?.MaxHeight ?? Double.PositiveInfinity
        };
        return dialog;
    }

    private static GlobalDialogManager FindDialogManager(TopLevel? topLevel)
    {
        var toplevel      = topLevel ?? Window.GetMainWindow();
        var dialogManager = toplevel.FindDescendantOfType<GlobalDialogManager>();
        if (dialogManager == null)
        {
            throw new InvalidOperationException("The DialogManager was not found in TopLevel; you may not be using the atom:Window class.");
        }
        return dialogManager;
    }
}