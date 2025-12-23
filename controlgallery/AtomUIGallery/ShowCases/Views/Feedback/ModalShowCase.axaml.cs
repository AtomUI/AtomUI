using System.Reactive.Disposables;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using TextBlock = AtomUI.Desktop.Controls.TextBlock;
using ToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUIGallery.ShowCases.Views;

public partial class ModalShowCase : ReactiveUserControl<ModalViewModel>
{
    public ModalShowCase()
    {
        this.WhenActivated(disposables =>
        {
            BasicOpenModalButton.Click       += HandleBasicModalButtonClick;
            BasicWindowOpenModalButton.Click += HandleBasicWindowModalButtonClick;

            ConfirmMsgBoxBtn.Click                   += HandleConfirmMsgBoxBtnClick;
            InformationMsgBoxBtn.Click               += HandleInformationMsgBoxBtnClick;
            SuccessMsgBoxBtn.Click                   += HandleSuccessMsgBoxBtnClick;
            ErrorMsgBoxBtn.Click                     += HandleErrorMsgBoxBtnClick;
            WarningMsgBoxBtn.Click                   += HandleWarningMsgBoxBtnClick;
            StyleCaseHostTypeSwitch.IsCheckedChanged += HandleStyleCaseHostTypeSwitchChanged;
            LoadingDialogOpenModalButton.Click       += HandleLoadingDialogOpenModalButtonClick;
            AsyncDialogOpenModalButton.Click         += HandleAsyncDialogOpenModalButtonClick;
            CustomFooterDialogOpenButton.Click       += HandleCustomFooterDialogOpenButtonClick;
            CustomFooterMsgBoxOpenButton.Click       += HandleCustomFooterMsgBoxOpenButtonClick;
            DraggableDialogOpenButton.Click          += HandleDraggableMsgBoxOpenButtonClick;
            DelayedCloseMsgBoxOpenButton.Click       += HandleDelayedCloseMsgBoxOpenButtonClick;
            ConfigureButtonsDialogOpenButton.Click   += HandleConfigureButtonsDialogButtonClick;
            
            disposables.Add(Disposable.Create(() => BasicOpenModalButton.Click       -= HandleBasicModalButtonClick));
            disposables.Add(Disposable.Create(() => BasicWindowOpenModalButton.Click -= HandleBasicWindowModalButtonClick));
            disposables.Add(Disposable.Create(() => ConfirmMsgBoxBtn.Click -= HandleConfirmMsgBoxBtnClick));
            disposables.Add(Disposable.Create(() => InformationMsgBoxBtn.Click -= HandleInformationMsgBoxBtnClick));
            disposables.Add(Disposable.Create(() => SuccessMsgBoxBtn.Click -= HandleSuccessMsgBoxBtnClick));
            disposables.Add(Disposable.Create(() => ErrorMsgBoxBtn.Click -= HandleErrorMsgBoxBtnClick));
            disposables.Add(Disposable.Create(() => WarningMsgBoxBtn.Click -= HandleWarningMsgBoxBtnClick));
            disposables.Add(Disposable.Create(() => StyleCaseHostTypeSwitch.IsCheckedChanged -= HandleStyleCaseHostTypeSwitchChanged));
            disposables.Add(Disposable.Create(() => LoadingDialogOpenModalButton.Click -= HandleLoadingDialogOpenModalButtonClick));
            disposables.Add(Disposable.Create(() => CustomFooterDialogOpenButton.Click -= HandleCustomFooterDialogOpenButtonClick));
            disposables.Add(Disposable.Create(() => CustomFooterMsgBoxOpenButton.Click -= HandleCustomFooterMsgBoxOpenButtonClick));
            disposables.Add(Disposable.Create(() => DraggableDialogOpenButton.Click -= HandleDraggableMsgBoxOpenButtonClick));
            disposables.Add(Disposable.Create(() => DelayedCloseMsgBoxOpenButton.Click -= HandleDelayedCloseMsgBoxOpenButtonClick));
            disposables.Add(Disposable.Create(() => ConfigureButtonsDialogOpenButton.Click -= HandleConfigureButtonsDialogButtonClick));

            ConfigureButtonPropertiesDialog.ButtonsConfigure = ConfigureButtonProperties;
            
            if (DataContext is ModalViewModel viewModel)
            {
                viewModel.MessageBoxStyleCaseHostType = DialogHostType.Overlay;
                viewModel.CountdownSeconds            = 5;
            }
        });
        InitializeComponent();
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

    private void HandleStyleCaseHostTypeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            if (DataContext is ModalViewModel viewModel)
            {
                viewModel.MessageBoxStyleCaseHostType = toggleSwitch.IsChecked == true ? DialogHostType.Window : DialogHostType.Overlay;
            }
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

    private IDisposable? _delayedCloseDialogDisposal;
    
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
        // 鼠标会被卡死，强制刷新一次事件循环
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildDialogContent();
        var options = new DialogOptions
        {
            IsModal                   = true,
            Title                     = "Basic Modal",
            IsResizable               = false,
            IsDragMovable             = true,
            IsMaximizable             = false,
            StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
            DefaultStandardButton     = DialogStandardButton.Ok,
            HorizontalStartupLocation = DialogHorizontalAnchor.Center,
            VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
            MinWidth                  = 400
        };
        await Dialog.ShowDialogAsync(content, null, options);
    }

    private async void HandleOpenWindowDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        // 鼠标会被卡死，强制刷新一次事件循环
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildDialogContent();
        var options = new DialogOptions
        {
            IsModal                   = true,
            Title                     = "Basic Modal",
            IsResizable               = false,
            IsDragMovable             = true,
            IsMaximizable             = false,
            DialogHostType            = DialogHostType.Window,
            StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
            DefaultStandardButton     = DialogStandardButton.Ok,
            HorizontalStartupLocation = DialogHorizontalAnchor.Center,
            VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
            MinWidth                  = 400
        };
        await Dialog.ShowDialogAsync(content, null, options);
    }

    private async void HandleOpenCustomViewDialogButtonClick(object? sender, RoutedEventArgs e)
    {
        // 鼠标会被卡死，强制刷新一次事件循环
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var options = new DialogOptions
        {
            IsModal                   = true,
            Title                     = "Basic Modal",
            IsResizable               = false,
            IsDragMovable             = true,
            IsMaximizable             = false,
            DialogHostType            = DialogHostType.Window,
            StandardButtons           = DialogStandardButtons.Parse("Cancel,Ok"),
            DefaultStandardButton     = DialogStandardButton.Ok,
            HorizontalStartupLocation = DialogHorizontalAnchor.Center,
            VerticalOffset            = new Dimension(30, DimensionUnitType.Percentage),
            MinWidth                  = 400
        };
        var viewModel = new ModalUserControlViewModel()
        {
            Name = "AtomUI",
            Age  = 2
        };
        Dialog.ShowDialog<ModalUserControlView, ModalUserControlViewModel>(viewModel, options);
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

    private async void HandleCreateConfirmMessageBox(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildMessageBoxContent();
        var options = new MessageBoxOptions()
        {
            IsModal           = true,
            Title             = "Do you want to delete these items?",
            IsDragMovable     = true,
            IsCenterOnStartup = true,
            Style             = MessageBoxStyle.Confirm
        };
        await MessageBox.ShowMessageAsync(content, null, options);
    }
    
    private async void HandleCreateInformationMessageBox(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildMessageBoxContent();
        var options = new MessageBoxOptions()
        {
            IsModal           = true,
            Title             = "This is a notification message",
            IsDragMovable     = true,
            IsCenterOnStartup = true,
            Style             = MessageBoxStyle.Information
        };
        await MessageBox.ShowMessageAsync(content, null, options);
    }
    
    private async void HandleCreateSuccessMessageBox(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildMessageBoxContent();
        var options = new MessageBoxOptions()
        {
            IsModal           = true,
            Title             = "Operation successful",
            IsDragMovable     = true,
            IsCenterOnStartup = true,
            Style             = MessageBoxStyle.Success
        };
        await MessageBox.ShowMessageAsync(content, null, options);
    }
    
    private async void HandleCreateErrorMessageBox(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildMessageBoxContent();
        var options = new MessageBoxOptions()
        {
            IsModal           = true,
            Title             = "This is an error message",
            IsDragMovable     = true,
            IsCenterOnStartup = true,
            Style             = MessageBoxStyle.Error
        };
        await MessageBox.ShowMessageAsync(content, null, options);
    }
    
    private async void HandleCreateWarningMessageBox(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var content = BuildMessageBoxContent();
        var options = new MessageBoxOptions()
        {
            IsModal           = true,
            Title             = "This is a warning message",
            IsDragMovable     = true,
            IsCenterOnStartup = true,
            Style             = MessageBoxStyle.Warning
        };
        await MessageBox.ShowMessageAsync(content, null, options);
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

}