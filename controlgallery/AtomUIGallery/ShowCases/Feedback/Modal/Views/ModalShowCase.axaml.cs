using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace AtomUIGallery.ShowCases.Modal;

public partial class ModalShowCase : GalleryReactiveUserControl<ModalViewModel>
{
    public const string LanguageId = nameof(ModalShowCase);

    private IDisposable? _delayedCloseDialogDisposal;

    public ModalShowCase()
    {
        InitializeComponent();
        AddHandler(AtomUIButton.ClickEvent, HandleDemoButtonClick);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _delayedCloseDialogDisposal?.Dispose();
        _delayedCloseDialogDisposal = null;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.MessageBoxStyleCaseHostType = DialogHostType.Overlay;
            viewModel.CountdownSeconds            = 5;
        }

    }

    private void HandleDemoButtonClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not AtomUIButton button)
        {
            return;
        }

        switch (button.Name)
        {
            case "BasicOpenModalButton":
                HandleBasicModalButtonClick(button, e);
                break;
            case "BasicWindowOpenModalButton":
                HandleBasicWindowModalButtonClick(button, e);
                break;
            case "ConfirmMsgBoxBtn":
                HandleConfirmMsgBoxBtnClick(button, e);
                break;
            case "InformationMsgBoxBtn":
                HandleInformationMsgBoxBtnClick(button, e);
                break;
            case "SuccessMsgBoxBtn":
                HandleSuccessMsgBoxBtnClick(button, e);
                break;
            case "ErrorMsgBoxBtn":
                HandleErrorMsgBoxBtnClick(button, e);
                break;
            case "WarningMsgBoxBtn":
                HandleWarningMsgBoxBtnClick(button, e);
                break;
            case "MessageBoxSemanticStyleOpenButton":
                HandleMessageBoxSemanticStyleOpenButtonClick(button, e);
                break;
            case "WindowDialogSemanticOpenButton":
                HandleWindowDialogSemanticOpenButtonClick(button, e);
                break;
            case "WindowDialogStyledOpenButton":
                HandleWindowDialogStyledOpenButtonClick(button, e);
                break;
            case "SemanticStyleDialogOpenButton":
                HandleSemanticStyleDialogOpenButtonClick(button, e);
                break;
            case "LoadingDialogOpenModalButton":
                HandleLoadingDialogOpenModalButtonClick(button, e);
                break;
            case "AsyncDialogOpenModalButton":
                HandleAsyncDialogOpenModalButtonClick(button, e);
                break;
            case "CustomFooterDialogOpenButton":
                HandleCustomFooterDialogOpenButtonClick(button, e);
                break;
            case "CustomFooterMsgBoxOpenButton":
                HandleCustomFooterMsgBoxOpenButtonClick(button, e);
                break;
            case "DraggableDialogOpenButton":
                HandleDraggableMsgBoxOpenButtonClick(button, e);
                break;
            case "DelayedCloseMsgBoxOpenButton":
                HandleDelayedCloseMsgBoxOpenButtonClick(button, e);
                break;
            case "ConfigureButtonsDialogOpenButton":
                HandleConfigureButtonsDialogButtonClick(button, e);
                break;
        }
    }

    private void HandleDialogExampleLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control root)
        {
            return;
        }

        SetPlacementTarget(root, "BasicDialog", "BasicOpenModalButton");
        SetPlacementTarget(root, "BasicWindowDialog", "BasicWindowOpenModalButton");
        SetPlacementTarget(root, "AsyncDialog", "AsyncDialogOpenModalButton");
        SetPlacementTarget(root, "ConfirmMsgBox", "ConfirmMsgBoxBtn");
        SetPlacementTarget(root, "InformationMsgBox", "InformationMsgBoxBtn");
        SetPlacementTarget(root, "SuccessMsgBox", "SuccessMsgBoxBtn");
        SetPlacementTarget(root, "ErrorMsgBox", "ErrorMsgBoxBtn");
        SetPlacementTarget(root, "WarningMsgBox", "WarningMsgBoxBtn");
        SetPlacementTarget(root, "LoadingDialog", "LoadingDialogOpenModalButton");
        SetPlacementTarget(root, "CustomFooterDialog", "CustomFooterDialogOpenButton");
        SetPlacementTarget(root, "CustomFooterMsgBox", "CustomFooterMsgBoxOpenButton");
        SetPlacementTarget(root, "DraggableDialog", "DraggableDialogOpenButton");
        SetPlacementTarget(root, "DelayedCloseMsgBox", "DelayedCloseMsgBoxOpenButton");
        SetPlacementTarget(root, "ConfigureButtonPropertiesDialog", "ConfigureButtonsDialogOpenButton");
    }

    private static void SetPlacementTarget(Control root, string dialogName, string targetName)
    {
        var target = FindDescendantByName<Control>(root, targetName);
        if (target is null)
        {
            return;
        }

        if (FindDescendantByName<Dialog>(root, dialogName) is { } dialog)
        {
            dialog.PlacementTarget = target;
            return;
        }

        if (FindDescendantByName<MessageBox>(root, dialogName) is { } messageBox)
        {
            messageBox.PlacementTarget = target;
        }
    }

    private void HandleBasicModalButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsBasicModalOpened = true;
        }
    }

    private void HandleBasicWindowModalButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsBasicWindowModalOpened = true;
        }
    }

    private void HandleConfirmMsgBoxBtnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsConfirmMsgBoxOpened = true;
        }
    }

    private void HandleInformationMsgBoxBtnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsInformationMsgBoxOpened = true;
        }
    }

    private void HandleSuccessMsgBoxBtnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsSuccessMsgBoxOpened = true;
        }
    }

    private void HandleErrorMsgBoxBtnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsErrorMsgBoxOpened = true;
        }
    }

    private void HandleWarningMsgBoxBtnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsWarningMsgBoxOpened = true;
        }
    }

    private void HandleMessageBoxHostTypeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel && sender is AtomUIToggleSwitch hostTypeSwitch)
        {
            viewModel.MessageBoxStyleCaseHostType = hostTypeSwitch.IsChecked == true
                ? DialogHostType.Window
                : DialogHostType.Overlay;
        }
    }

    private void HandleMessageBoxSemanticStyleOpenButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsMessageBoxSemanticStyleOpen = true;
        }
    }

    private void HandleWindowDialogSemanticOpenButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsWindowDialogSemanticOpen = true;
        }
    }

    private void HandleWindowDialogStyledOpenButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsWindowDialogStyledOpen = true;
        }
    }

    private void HandleSemanticStyleDialogOpenButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsSemanticStyleDialogOpen = true;
        }
    }

    private void HandleLoadingDialogOpenModalButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsLoadingMsgBoxOpened = true;
        }
    }

    private void HandleLoadingDialogOpened(object? sender, EventArgs e)
    {
        if (sender is Dialog dialog)
        {
            DispatcherTimer.RunOnce(() =>
            {
                dialog.IsLoading = false;
            }, TimeSpan.FromMilliseconds(3000));
        }
    }

    private void HandleLoadingDialogButtonClicked(object? sender, DialogButtonClickedEventArgs e)
    {
        if (sender is Dialog dialog)
        {
            dialog.IsLoading = true;
            DispatcherTimer.RunOnce(() =>
            {
                dialog.IsLoading = false;
            }, TimeSpan.FromMilliseconds(3000));
        }
    }

    private void HandleAsyncDialogOpenModalButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsAsyncDialogOpened = true;
        }
    }

    private void HandleAsyncDialogButtonClicked(object? sender, DialogButtonClickedEventArgs e)
    {
        if (sender is Dialog dialog && e.SourceButton.Role == DialogButtonRole.AcceptRole)
        {
            dialog.IsConfirmLoading = true;
            e.Handled               = true;
            DispatcherTimer.RunOnce(() =>
            {
                dialog.IsConfirmLoading = false;
                dialog.Done();
            }, TimeSpan.FromMilliseconds(3000));
        }
    }

    private void HandleCustomFooterDialogOpenButtonClick(object? sender, EventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsCustomFooterDialogOpened = true;
        }
    }

    private void HandleCustomFooterMsgBoxOpenButtonClick(object? sender, EventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsCustomFooterMsgBoxOpened = true;
        }
    }

    private void HandleDraggableMsgBoxOpenButtonClick(object? sender, EventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsDraggableMsgBoxOpened = true;
        }
    }

    private void HandleDelayedCloseMsgBoxOpenButtonClick(object? sender, EventArgs e)
    {
        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsDelayedCloseMsgBoxOpened = true;
        }
    }

    private void HandleDelayedCloseMsgBoxOpened(object? sender, EventArgs e)
    {
        if (sender is MessageBox messageBox)
        {
            if (DataContext is ModalViewModel viewModel)
            {
                viewModel.CountdownSeconds = 5;
                _delayedCloseDialogDisposal?.Dispose();
                _delayedCloseDialogDisposal = DispatcherTimer.Run(() =>
                {
                    if (viewModel.CountdownSeconds == 0)
                    {
                        messageBox.Confirm();
                        return false;
                    }
                    viewModel.CountdownSeconds--;
                    return true;
                }, TimeSpan.FromMilliseconds(1000));
            }
        }
    }

    private void HandleConfigureButtonsDialogButtonClick(object? sender, EventArgs e)
    {
        if (TryFindTemplateControl(sender, "ConfigureButtonPropertiesDialog", out Dialog dialog))
        {
            dialog.ButtonsConfigure = ConfigureButtonProperties;
        }

        if (DataContext is ModalViewModel viewModel)
        {
            viewModel.IsConfigureButtonsDialogOpened = true;
        }
    }

    private void ConfigureButtonProperties(IReadOnlyList<DialogButton> buttons)
    {
        foreach (var button in buttons)
        {
            button.IsEnabled = false;
        }
    }

    private async void HandleOpenOverlayDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildDialogContent();
            var options = new DialogOptions
            {
                Title                     = "Basic Modal",
                IsResizable               = false,
                IsDragMovable             = true,
                IsMaximizable             = false,
                StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
                DefaultStandardButton     = DialogStandardButton.Ok,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
                VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
                HostMinWidth              = 400,
                PlacementTarget           = sender as Control
            };
            await Dialog.ShowDialogAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleOpenWindowDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildDialogContent();
            var options = new DialogOptions
            {
                Title                     = "Basic Modal",
                IsResizable               = false,
                IsDragMovable             = true,
                IsMaximizable             = false,
                DialogHostType            = DialogHostType.Window,
                StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
                DefaultStandardButton     = DialogStandardButton.Ok,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
                VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
                HostMinWidth              = 400
            };
            await Dialog.ShowDialogModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleOpenCustomViewDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var options = new DialogOptions
            {
                Title                     = "Basic Modal",
                IsResizable               = false,
                IsDragMovable             = true,
                IsMaximizable             = false,
                DialogHostType            = DialogHostType.Window,
                StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
                DefaultStandardButton     = DialogStandardButton.Ok,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
                VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
                HostMinWidth              = 400
            };
            var viewModel = new ModalUserControlViewModel()
            {
                Name = "AtomUI",
                Age  = 2
            };
            await Dialog.ShowDialogModalAsync<ModalUserControlView, ModalUserControlViewModel>(viewModel, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleOpenBeforeCloseDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var validationAttempts = 0;
            var statusText = new TextBlock
            {
                Text         = "The first OK click will fail async validation. Click OK again to close.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            var content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "DialogOptions.BeforeCloseAsync can decide whether a static dialog is allowed to close." },
                    statusText
                }
            };
            var options = new DialogOptions
            {
                Title                     = "Async before-close validation",
                IsResizable               = false,
                IsDragMovable             = true,
                IsMaximizable             = false,
                StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
                DefaultStandardButton     = DialogStandardButton.Ok,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
                VerticalStartupLocation   = DialogVerticalAnchor.Center,
                HostMinWidth              = 420,
                PlacementTarget           = sender as Control,
                BeforeCloseAsync = async context =>
                {
                    if (context.DialogCode != DialogCode.Accepted)
                    {
                        return true;
                    }

                    context.Dialog.IsConfirmLoading = true;
                    try
                    {
                        await Task.Delay(1000, context.CancellationToken);
                        validationAttempts++;
                        if (validationAttempts == 1)
                        {
                            statusText.Text = "Validation failed. The dialog stayed open; click OK again to pass.";
                            return false;
                        }

                        return true;
                    }
                    finally
                    {
                        context.Dialog.IsConfirmLoading = false;
                    }
                }
            };
            await Dialog.ShowDialogModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleCreateConfirmMessageBox(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildMessageBoxContent();
            var options = new MessageBoxOptions
            {
                Title             = "Do you want to delete these items?",
                IsDragMovable     = true,
                IsCenterOnStartup = true,
                Style             = MessageBoxStyle.Confirm
            };
            await MessageBox.ShowMessageBoxModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleCreateInformationMessageBox(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildMessageBoxContent();
            var options = new MessageBoxOptions
            {
                Title             = "This is a notification message",
                IsDragMovable     = true,
                IsCenterOnStartup = true,
                Style             = MessageBoxStyle.Information
            };
            await MessageBox.ShowMessageBoxModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleCreateSuccessMessageBox(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildMessageBoxContent();
            var options = new MessageBoxOptions
            {
                Title             = "Operation successful",
                IsDragMovable     = true,
                IsCenterOnStartup = true,
                Style             = MessageBoxStyle.Success
            };
            await MessageBox.ShowMessageBoxModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleCreateErrorMessageBox(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildMessageBoxContent();
            var options = new MessageBoxOptions
            {
                Title             = "This is an error message",
                IsDragMovable     = true,
                IsCenterOnStartup = true,
                Style             = MessageBoxStyle.Error
            };
            await MessageBox.ShowMessageBoxModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private async void HandleCreateWarningMessageBox(object? sender, RoutedEventArgs e)
    {
        try
        {
            var content = BuildMessageBoxContent();
            var options = new MessageBoxOptions
            {
                Title             = "This is a warning message",
                IsDragMovable     = true,
                IsCenterOnStartup = true,
                Style             = MessageBoxStyle.Warning
            };
            await MessageBox.ShowMessageBoxModalAsync(content, null, options);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
    }

    private Control BuildDialogContent()
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 5
        };
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Some contents..."
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Some contents..."
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Some contents..."
        });
        return stackPanel;
    }

    private Control BuildMessageBoxContent()
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 5
        };
        stackPanel.Children.Add(new TextBlock
        {
            Text = "some messages...some messages..."
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = "some messages...some messages..."
        });
        return stackPanel;
    }

    private static bool TryFindTemplateControl<T>(object? source, string name, out T control)
        where T : Control
    {
        var current = source as Control;
        while (current is not null)
        {
            if (current is T directControl &&
                directControl.Name == name)
            {
                control = directControl;
                return true;
            }

            var descendantControl = FindDescendantByName<T>(current, name);
            if (descendantControl is not null)
            {
                control = descendantControl;
                return true;
            }

            current = current.Parent as Control;
        }

        control = null!;
        return false;
    }

    private static T? FindDescendantByName<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && typedRoot.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => control.Name == name)
               ?? root.GetLogicalDescendants().OfType<T>().FirstOrDefault(control => control.Name == name);
    }
}
