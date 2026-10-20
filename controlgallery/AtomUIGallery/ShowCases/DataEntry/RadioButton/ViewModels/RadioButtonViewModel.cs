using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.RadioButton;

public class RadioButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "RadioButton";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private IList<RadioButtonOption>? _radioOptions;

    public IList<RadioButtonOption>? RadioOptions
    {
        get => _radioOptions;
        set => this.RaiseAndSetIfChanged(ref _radioOptions, value);
    }

    private IList<RadioButtonOption>? _twoWayRadioOptions;

    public IList<RadioButtonOption>? TwoWayRadioOptions
    {
        get => _twoWayRadioOptions;
        set => this.RaiseAndSetIfChanged(ref _twoWayRadioOptions, value);
    }

    private object? _twoWayCheckedItem;

    public object? TwoWayCheckedItem
    {
        get => _twoWayCheckedItem;
        set
        {
            if (ReferenceEquals(_twoWayCheckedItem, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _twoWayCheckedItem, value);
            UpdateTwoWayCheckedSummary();
        }
    }

    private string? _twoWayCheckedSummary;

    public string? TwoWayCheckedSummary
    {
        get => _twoWayCheckedSummary;
        set => this.RaiseAndSetIfChanged(ref _twoWayCheckedSummary, value);
    }

    private RadioButtonOption? _twoWayChengduOption;

    private bool _toggleDisabledRadioUnCheckedEnabled;

    public bool ToggleDisabledRadioUnCheckedEnabled
    {
        get => _toggleDisabledRadioUnCheckedEnabled;
        set => this.RaiseAndSetIfChanged(ref _toggleDisabledRadioUnCheckedEnabled, value);
    }

    private bool _toggleDisabledRadioCheckedEnabled;

    public bool ToggleDisabledRadioCheckedEnabled
    {
        get => _toggleDisabledRadioCheckedEnabled;
        set => this.RaiseAndSetIfChanged(ref _toggleDisabledRadioCheckedEnabled, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleDisabledCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectChengduCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearTwoWayCheckedItemCommand { get; }

    public RadioButtonViewModel(IScreen screen)
    {
        HostScreen = screen;

        _toggleDisabledRadioUnCheckedEnabled = true;
        _toggleDisabledRadioCheckedEnabled = true;

        ToggleDisabledCommand = ReactiveCommand.Create(HandleToggleDisabled);
        SelectChengduCommand  = ReactiveCommand.Create(HandleSelectChengdu);
        ClearTwoWayCheckedItemCommand = ReactiveCommand.Create(HandleClearTwoWayCheckedItem);
    }

    private void HandleToggleDisabled()
    {
        ToggleDisabledRadioUnCheckedEnabled = !ToggleDisabledRadioUnCheckedEnabled;
        ToggleDisabledRadioCheckedEnabled = !ToggleDisabledRadioCheckedEnabled;
    }

    public void ConfigureTwoWayRadioOptions(
        RadioButtonOption hangzhou,
        RadioButtonOption shanghai,
        RadioButtonOption beijing,
        RadioButtonOption chengdu)
    {
        _twoWayChengduOption = chengdu;
        TwoWayRadioOptions =
        [
            hangzhou,
            shanghai,
            beijing,
            chengdu
        ];
        TwoWayCheckedItem = shanghai;
    }

    public void ClearTwoWayRadioOptions()
    {
        _twoWayChengduOption = null;
        TwoWayCheckedItem    = null;
        TwoWayRadioOptions   = null;
        TwoWayCheckedSummary = null;
    }

    private void HandleSelectChengdu()
    {
        if (_twoWayChengduOption != null)
        {
            TwoWayCheckedItem = _twoWayChengduOption;
        }
    }

    private void HandleClearTwoWayCheckedItem()
    {
        TwoWayCheckedItem = null;
    }

    private void UpdateTwoWayCheckedSummary()
    {
        var selectedText = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentNone, "None");
        if (TwoWayCheckedItem is RadioButtonOption { Content: not null } option)
        {
            selectedText = option.Content.ToString() ?? selectedText;
        }

        TwoWayCheckedSummary = RadioButtonShowCaseLanguage.Format(
            RadioButtonShowCaseLangResourceKind.P2CheckedItemSummaryFormat,
            "Selected: {0}",
            selectedText);
    }

}
