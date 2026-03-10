using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CheckBoxViewModel : ReactiveObject, IRoutableViewModel,
                                 IActivatableViewModel
{
    public static EntityKey ID = "CheckBox";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public bool? _controlledCheckBoxCheckedStatus;

    public bool? ControlledCheckBoxCheckedStatus
    {
        get => _controlledCheckBoxCheckedStatus;
        set => this.RaiseAndSetIfChanged(ref _controlledCheckBoxCheckedStatus, value);
    }

    public bool _controlledCheckBoxEnabledStatus;

    public bool ControlledCheckBoxEnabledStatus
    {
        get => _controlledCheckBoxEnabledStatus;
        set => this.RaiseAndSetIfChanged(ref _controlledCheckBoxEnabledStatus, value);
    }

    private string? _checkStatusBtnText;

    public string? CheckStatusBtnText
    {
        get => _checkStatusBtnText;
        set => this.RaiseAndSetIfChanged(ref _checkStatusBtnText, value);
    }

    private string? _enableStatusBtnText;

    public string? EnableStatusBtnText
    {
        get => _enableStatusBtnText;
        set => this.RaiseAndSetIfChanged(ref _enableStatusBtnText, value);
    }

    private string? _controlledCheckBoxText;

    public string? ControlledCheckBoxText
    {
        get => _controlledCheckBoxText;
        set => this.RaiseAndSetIfChanged(ref _controlledCheckBoxText, value);
    }

    // CheckAll 例子
    private bool? _checkedAllStatus;

    public bool? CheckedAllStatus
    {
        get => _checkedAllStatus;
        set => this.RaiseAndSetIfChanged(ref _checkedAllStatus, value);
    }

    private bool _appleCheckedStatus;

    public bool AppleCheckedStatus
    {
        get => _appleCheckedStatus;
        set => this.RaiseAndSetIfChanged(ref _appleCheckedStatus, value);
    }

    private bool _pearCheckedStatus;

    public bool PearCheckedStatus
    {
        get => _pearCheckedStatus;
        set => this.RaiseAndSetIfChanged(ref _pearCheckedStatus, value);
    }

    private bool _orangeCheckedStatus;

    public bool OrangeCheckedStatus
    {
        get => _orangeCheckedStatus;
        set => this.RaiseAndSetIfChanged(ref _orangeCheckedStatus, value);
    }
    
    private IList<CheckBoxOption>? _checkBoxOptions;

    public IList<CheckBoxOption>? CheckBoxOptions
    {
        get => _checkBoxOptions;
        set => this.RaiseAndSetIfChanged(ref _checkBoxOptions, value);
    }
    
    private IList<CheckBoxOption>? _defaultCheckBoxOptions;

    public IList<CheckBoxOption>? DefaultCheckBoxOptions
    {
        get => _defaultCheckBoxOptions;
        set => this.RaiseAndSetIfChanged(ref _defaultCheckBoxOptions, value);
    }

    public ReactiveCommand<Button, Unit> CheckStatusCommand { get; }
    public ReactiveCommand<Button, Unit> EnableStatusCommand { get; }
    public ReactiveCommand<Button, Unit> CheckBoxCommand { get; }
    public ReactiveCommand<CheckBox, Unit> CheckedAllStatusCommand { get; }
    public ReactiveCommand<CheckBox, Unit> CheckedItemStatusCommand1 { get; }
    public ReactiveCommand<CheckBox, Unit> CheckedItemStatusCommand2 { get; }
    public ReactiveCommand<CheckBox, Unit> CheckedItemStatusCommand3 { get; }

    public CheckBoxViewModel(IScreen screen)
    {
        HostScreen = screen;
        Activator  = new ViewModelActivator();

        CheckStatusBtnText              = "UnCheck";
        EnableStatusBtnText             = "Disable";
        ControlledCheckBoxCheckedStatus = true;
        ControlledCheckBoxEnabledStatus = true;
        SetupControlledCheckBoxText();

        AppleCheckedStatus  = false;
        PearCheckedStatus   = true;
        OrangeCheckedStatus = true;
        CheckedAllStatus    = null;
        
        EnableStatusCommand = ReactiveCommand.Create<Button>(HandleStatus);
        CheckStatusCommand  = ReactiveCommand.Create<Button>(HandleCheckStatus);
        CheckBoxCommand     = ReactiveCommand.Create<Button>(HandleCheckBox);

        CheckedAllStatusCommand   = ReactiveCommand.Create<CheckBox>(HandleCheckedAllStatus);
        CheckedItemStatusCommand1 = ReactiveCommand.Create<CheckBox>(HandleCheckedItemStatus);
        CheckedItemStatusCommand2 = ReactiveCommand.Create<CheckBox>(HandleCheckedItemStatus);
        CheckedItemStatusCommand3 = ReactiveCommand.Create<CheckBox>(HandleCheckedItemStatus);
    }

    private void HandleCheckStatus(Button sender)
    {
        ControlledCheckBoxCheckedStatus = !ControlledCheckBoxCheckedStatus;
        SetupCheckBtnText();
        SetupControlledCheckBoxText();
    }

    private void HandleStatus(Button sender)
    {
        ControlledCheckBoxEnabledStatus = !ControlledCheckBoxEnabledStatus;
        SetupEnabledBtnText();
        SetupControlledCheckBoxText();
    }

    private void HandleCheckBox(Button sender)
    {
        SetupCheckBtnText();
        SetupControlledCheckBoxText();
    }

    private void SetupCheckBtnText()
    {
        if (ControlledCheckBoxCheckedStatus.HasValue)
        {
            if (ControlledCheckBoxCheckedStatus.Value)
            {
                CheckStatusBtnText = "UnCheck";
            }
            else
            {
                CheckStatusBtnText = "Check";
            }
        }
        else
        {
            CheckStatusBtnText = "Check";
        }
    }

    private void SetupEnabledBtnText()
    {
        if (ControlledCheckBoxEnabledStatus)
        {
            EnableStatusBtnText = "Disable";
        }
        else
        {
            EnableStatusBtnText = "Enable";
        }
    }

    private void SetupControlledCheckBoxText()
    {
        var checkedText = "UnChecked";
        if (ControlledCheckBoxCheckedStatus.HasValue && ControlledCheckBoxCheckedStatus.Value)
        {
            checkedText = "Checked";
        }

        var enabledText = "Disabled";
        if (ControlledCheckBoxEnabledStatus)
        {
            enabledText = "Enabled";
        }

        ControlledCheckBoxText = $"{checkedText}-{enabledText}";
    }

    private void HandleCheckedAllStatus(CheckBox sender)
    {
        if (!CheckedAllStatus.HasValue || !CheckedAllStatus.Value)
        {
            AppleCheckedStatus  = false;
            PearCheckedStatus   = false;
            OrangeCheckedStatus = false;
        }
        else
        {
            AppleCheckedStatus  = true;
            PearCheckedStatus   = true;
            OrangeCheckedStatus = true;
        }
    }
    
    private void HandleCheckedItemStatus(CheckBox sender)
    {
        if (OrangeCheckedStatus && PearCheckedStatus && AppleCheckedStatus)
        {
            CheckedAllStatus = true;
        }
        else if (!OrangeCheckedStatus && !PearCheckedStatus && !AppleCheckedStatus)
        {
            CheckedAllStatus = false;
        }
        else
        {
            CheckedAllStatus = null;
        }
    }
}