using AtomUI;
using System.Collections.ObjectModel;
using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Select";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<SelectOption>? _randomOptions;

    public List<SelectOption>? RandomOptions
    {
        get => _randomOptions;
        set => this.RaiseAndSetIfChanged(ref _randomOptions, value);
    }
    
    private List<SelectOption>? _maxTagCountOptions;

    public List<SelectOption>? MaxTagCountOptions
    {
        get => _maxTagCountOptions;
        set => this.RaiseAndSetIfChanged(ref _maxTagCountOptions, value);
    }
    
    private SizeType _selectSizeType;

    public SizeType SelectSizeType
    {
        get => _selectSizeType;
        set => this.RaiseAndSetIfChanged(ref _selectSizeType, value);
    }
        
    private ObservableCollection<ISelectOption>? _generatedOptions;

    public ObservableCollection<ISelectOption>? GeneratedOptions
    {
        get => _generatedOptions;
        set => this.RaiseAndSetIfChanged(ref _generatedOptions, value);
    }

    private ObservableCollection<ISelectOption>? _generatedSelectedOptions;

    public ObservableCollection<ISelectOption>? GeneratedSelectedOptions
    {
        get => _generatedSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _generatedSelectedOptions, value);
    }



    private ReactiveCommand<Unit, Unit>? _addOptionCommand;

    public ReactiveCommand<Unit, Unit>? AddOptionCommand
    {
        get => _addOptionCommand;
        set => this.RaiseAndSetIfChanged(ref _addOptionCommand, value);
    }

    private ReactiveCommand<Unit, Unit>? _removeOptionCommand;

    public ReactiveCommand<Unit, Unit>? RemoveOptionCommand
    {
        get => _removeOptionCommand;
        set => this.RaiseAndSetIfChanged(ref _removeOptionCommand, value);
    }

    private ReactiveCommand<Unit, Unit>? _updateOptionCommand;

    public ReactiveCommand<Unit, Unit>? UpdateOptionCommand
    {
        get => _updateOptionCommand;
        set => this.RaiseAndSetIfChanged(ref _updateOptionCommand, value);
    }

    
    private ReactiveCommand<Unit, Unit>? _setSelectedOptionCommand;

    public ReactiveCommand<Unit, Unit>? SetSelectedOptionCommand
    {
        get => _setSelectedOptionCommand;
        set => this.RaiseAndSetIfChanged(ref _setSelectedOptionCommand, value);
    }

    public SelectViewModel(IScreen screen)
    {
        HostScreen  = screen;
    }
}
