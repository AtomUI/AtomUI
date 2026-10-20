using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.CheckBox;

public class CheckBoxViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "CheckBox";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

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
    
    private IList? _defaultCheckBoxOptions;

    public IList? DefaultCheckBoxOptions
    {
        get => _defaultCheckBoxOptions;
        set => this.RaiseAndSetIfChanged(ref _defaultCheckBoxOptions, value);
    }

    private IList<CheckBoxOption>? _twoWayCheckBoxOptions;

    public IList<CheckBoxOption>? TwoWayCheckBoxOptions
    {
        get => _twoWayCheckBoxOptions;
        set => this.RaiseAndSetIfChanged(ref _twoWayCheckBoxOptions, value);
    }

    private IList? _twoWayCheckedOptions;

    public IList? TwoWayCheckedOptions
    {
        get => _twoWayCheckedOptions;
        set
        {
            if (ReferenceEquals(_twoWayCheckedOptions, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _twoWayCheckedOptions, value);
            ConfigureTwoWayCheckedOptionsCollectionChangedSource(value);
        }
    }

    private string? _twoWayCheckedSummary;

    public string? TwoWayCheckedSummary
    {
        get => _twoWayCheckedSummary;
        set => this.RaiseAndSetIfChanged(ref _twoWayCheckedSummary, value);
    }

    private CheckBoxOption? _twoWayPearOption;
    private INotifyCollectionChanged? _twoWayCheckedOptionsCollectionChangedSource;

    public ReactiveCommand<Unit, Unit> CheckStatusCommand { get; }
    public ReactiveCommand<Unit, Unit> EnableStatusCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckBoxCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckedAllStatusCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckedItemStatusCommand1 { get; }
    public ReactiveCommand<Unit, Unit> CheckedItemStatusCommand2 { get; }
    public ReactiveCommand<Unit, Unit> CheckedItemStatusCommand3 { get; }
    public ReactiveCommand<Unit, Unit> AddPearToTwoWayCheckedItemsCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearTwoWayCheckedItemsCommand { get; }

    public CheckBoxViewModel(IScreen screen)
    {
        HostScreen = screen;

        ControlledCheckBoxCheckedStatus = true;
        ControlledCheckBoxEnabledStatus = true;
        RefreshLocalizedTexts();

        AppleCheckedStatus  = false;
        PearCheckedStatus   = true;
        OrangeCheckedStatus = true;
        CheckedAllStatus    = null;

        EnableStatusCommand = ReactiveCommand.Create(HandleStatus);
        CheckStatusCommand  = ReactiveCommand.Create(HandleCheckStatus);
        CheckBoxCommand     = ReactiveCommand.Create(HandleCheckBox);

        CheckedAllStatusCommand   = ReactiveCommand.Create(HandleCheckedAllStatus);
        CheckedItemStatusCommand1 = ReactiveCommand.Create(HandleCheckedItemStatus);
        CheckedItemStatusCommand2 = ReactiveCommand.Create(HandleCheckedItemStatus);
        CheckedItemStatusCommand3 = ReactiveCommand.Create(HandleCheckedItemStatus);
        AddPearToTwoWayCheckedItemsCommand = ReactiveCommand.Create(HandleAddPearToTwoWayCheckedItems);
        ClearTwoWayCheckedItemsCommand     = ReactiveCommand.Create(HandleClearTwoWayCheckedItems);
    }

    private void HandleCheckStatus()
    {
        ControlledCheckBoxCheckedStatus = !ControlledCheckBoxCheckedStatus;
        SetupCheckBtnText();
        SetupControlledCheckBoxText();
    }

    private void HandleStatus()
    {
        ControlledCheckBoxEnabledStatus = !ControlledCheckBoxEnabledStatus;
        SetupEnabledBtnText();
        SetupControlledCheckBoxText();
    }

    private void HandleCheckBox()
    {
        SetupCheckBtnText();
        SetupControlledCheckBoxText();
    }

    public void RefreshLocalizedTexts()
    {
        SetupCheckBtnText();
        SetupEnabledBtnText();
        SetupControlledCheckBoxText();
        UpdateTwoWayCheckedSummary();
    }

    public void ConfigureTwoWayCheckBoxOptions(CheckBoxOption apple, CheckBoxOption pear, CheckBoxOption orange)
    {
        _twoWayPearOption = pear;
        TwoWayCheckBoxOptions =
        [
            apple,
            pear,
            orange
        ];
        TwoWayCheckedOptions = new ObservableCollection<CheckBoxOption>
        {
            apple
        };
    }

    private void SetupCheckBtnText()
    {
        if (ControlledCheckBoxCheckedStatus.HasValue)
        {
            if (ControlledCheckBoxCheckedStatus.Value)
            {
                CheckStatusBtnText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentUncheck, "UnCheck");
            }
            else
            {
                CheckStatusBtnText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentCheck, "Check");
            }
        }
        else
        {
            CheckStatusBtnText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentCheck, "Check");
        }
    }

    private void SetupEnabledBtnText()
    {
        if (ControlledCheckBoxEnabledStatus)
        {
            EnableStatusBtnText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentDisable, "Disable");
        }
        else
        {
            EnableStatusBtnText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentEnable, "Enable");
        }
    }

    private void SetupControlledCheckBoxText()
    {
        var checkedText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentUnchecked, "UnChecked");
        if (ControlledCheckBoxCheckedStatus.HasValue && ControlledCheckBoxCheckedStatus.Value)
        {
            checkedText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentChecked, "Checked");
        }

        var enabledText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentDisabled, "Disabled");
        if (ControlledCheckBoxEnabledStatus)
        {
            enabledText = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentEnabled, "Enabled");
        }

        ControlledCheckBoxText = CheckBoxShowCaseLanguage.Format(
            CheckBoxShowCaseLangResourceKind.P2ControlledStatusFormat,
            "{0}-{1}",
            checkedText,
            enabledText);
    }

    private void HandleCheckedAllStatus()
    {
        var isChecked = CheckedAllStatus == true;
        AppleCheckedStatus  = isChecked;
        PearCheckedStatus   = isChecked;
        OrangeCheckedStatus = isChecked;
    }

    private void HandleCheckedItemStatus()
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

    private void HandleAddPearToTwoWayCheckedItems()
    {
        if (_twoWayPearOption == null)
        {
            return;
        }

        if (TwoWayCheckedOptions == null)
        {
            TwoWayCheckedOptions = new ObservableCollection<CheckBoxOption>
            {
                _twoWayPearOption
            };
            return;
        }

        if (!TwoWayCheckedOptions.Contains(_twoWayPearOption))
        {
            TwoWayCheckedOptions.Add(_twoWayPearOption);
        }
    }

    private void HandleClearTwoWayCheckedItems()
    {
        TwoWayCheckedOptions?.Clear();
    }

    private void ConfigureTwoWayCheckedOptionsCollectionChangedSource(IList? checkedOptions)
    {
        if (_twoWayCheckedOptionsCollectionChangedSource != null)
        {
            _twoWayCheckedOptionsCollectionChangedSource.CollectionChanged -= HandleTwoWayCheckedOptionsCollectionChanged;
            _twoWayCheckedOptionsCollectionChangedSource = null;
        }

        _twoWayCheckedOptionsCollectionChangedSource = checkedOptions as INotifyCollectionChanged;
        if (_twoWayCheckedOptionsCollectionChangedSource != null)
        {
            _twoWayCheckedOptionsCollectionChangedSource.CollectionChanged += HandleTwoWayCheckedOptionsCollectionChanged;
        }

        UpdateTwoWayCheckedSummary();
    }

    private void HandleTwoWayCheckedOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _twoWayCheckedOptionsCollectionChangedSource))
        {
            UpdateTwoWayCheckedSummary();
        }
    }

    private void UpdateTwoWayCheckedSummary()
    {
        var selectedTexts = new List<string>();
        if (TwoWayCheckedOptions != null)
        {
            foreach (var option in TwoWayCheckedOptions)
            {
                if (option is CheckBoxOption checkBoxOption && checkBoxOption.Content is not null)
                {
                    selectedTexts.Add(checkBoxOption.Content.ToString() ?? string.Empty);
                }
            }
        }

        var selectedText = selectedTexts.Count > 0
            ? string.Join(", ", selectedTexts)
            : CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentNone, "None");
        TwoWayCheckedSummary = CheckBoxShowCaseLanguage.Format(
            CheckBoxShowCaseLangResourceKind.P2TwoWayCheckedSummaryFormat,
            "Selected: {0}",
            selectedText);
    }

}
