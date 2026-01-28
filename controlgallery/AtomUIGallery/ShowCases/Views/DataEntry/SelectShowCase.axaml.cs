using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SelectShowCase : ReactiveUserControl<SelectViewModel>
{
    public SelectShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is SelectViewModel viewModel)
            {
                InitializeRandomOptions(viewModel);
                InitializeMaxTagCountOptions(viewModel);
                InitializeGeneratedOptions(viewModel);
            }
        });
        InitializeComponent();
        CustomSearchSelect.FilterFn = CustomFilter;
    }

    public static bool CustomFilter(object value, object filterValue)
    {
        // 使用大小写敏感的搜索
        var valueStr = value.ToString();
        Debug.Assert(valueStr != null);
        var filterStr = filterValue.ToString();
        if (filterStr == null)
        {
            return false;
        }
        return valueStr.Contains(filterStr, StringComparison.Ordinal);
    }

    private void InitializeRandomOptions(SelectViewModel viewModel)
    {
        var options = new List<SelectOption>();
        for (var i = 10; i < 36; i++)
        {
            var base36Str = ConvertToBase36(i);
            options.Add(new SelectOption 
            {
                Header = base36Str + i,
                Value = base36Str + i
            });
        }
        viewModel.RandomOptions = options;
    }
    
    private void InitializeMaxTagCountOptions(SelectViewModel viewModel)
    {
        var options = new List<SelectOption>();
        for (var i = 10; i < 36; i++)
        {
            var base36Str = ConvertToBase36(i);
            options.Add(new SelectOption 
            {
                Header = $"Long label: {base36Str + i}",
                Value  = base36Str + i
            });
        }
        viewModel.MaxTagCountOptions = options;
    }

    private void InitializeGeneratedOptions(SelectViewModel viewModel)
    {
        viewModel.GeneratedOptions ??= new ObservableCollection<ISelectOption>
        {
            new SelectOption { Header = "Option 1", Value = "option-1" },
            new SelectOption { Header = "Option 2", Value = "option-2" }
        };

        viewModel.GeneratedSelectedOptions ??= new ObservableCollection<ISelectOption>();

        IObservable<bool> CanModifySelected()
        {
            return Observable.Merge(
                    Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => viewModel.GeneratedSelectedOptions.CollectionChanged += h,
                        h => viewModel.GeneratedSelectedOptions.CollectionChanged -= h)
                        .Select(_ => Unit.Default),
                    Observable.Return(Unit.Default))
                .Select(_ => viewModel.GeneratedSelectedOptions.Count > 0);
        }

        IObservable<bool> HasOptions()
        {
            return Observable.Merge(
                    Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => viewModel.GeneratedOptions.CollectionChanged += h,
                        h => viewModel.GeneratedOptions.CollectionChanged -= h)
                        .Select(_ => Unit.Default),
                    Observable.Return(Unit.Default))
                .Select(_ => viewModel.GeneratedOptions.Count > 0);
        }

        if (viewModel.AddOptionCommand == null)
        {
            var counter = viewModel.GeneratedOptions.Count;
            viewModel.AddOptionCommand = ReactiveCommand.Create(() =>
            {
                counter++;
                var option = new SelectOption
                {
                    Header = $"New Option {counter}",
                    Value  = $"new-{counter}"
                };
                viewModel.GeneratedOptions.Add(option);
            });
        }

        List<ISelectOption> GetSelectedTargets()
        {
            return viewModel.GeneratedSelectedOptions.Count > 0
                ? new List<ISelectOption> { viewModel.GeneratedSelectedOptions[0] }
                : [];
        }

        if (viewModel.RemoveOptionCommand == null)
        {
            viewModel.RemoveOptionCommand = ReactiveCommand.Create(() =>
            {
                if (viewModel.GeneratedOptions.Count == 0)
                {
                    return;
                }

                var targets = GetSelectedTargets();
                if (targets.Count == 0)
                {
                    return;
                }

                foreach (var target in targets)
                {
                    viewModel.GeneratedOptions.Remove(target);
                }
            }, CanModifySelected());
        }

        if (viewModel.UpdateOptionCommand == null)
        {
            viewModel.UpdateOptionCommand = ReactiveCommand.Create(() =>
            {
                if (viewModel.GeneratedOptions.Count == 0)
                {
                    return;
                }

                var targets = GetSelectedTargets();
                if (targets.Count == 0)
                {
                    return;
                }

                foreach (var target in targets)
                {
                    var index = viewModel.GeneratedOptions.IndexOf(target);
                    if (index < 0)
                    {
                        continue;
                    }

                    ISelectOption updated;
                    if (target is SelectOption selectOption)
                    {
                        updated = selectOption with { Header = $"{selectOption.Header} (updated)" };
                    }
                    else
                    {
                        updated = new SelectOption
                        {
                            Header = $"{target.Header} (updated)",
                            Value  = target.Value,
                            Group  = target.Group,
                            IsEnabled = target.IsEnabled,
                            IsDynamicAdded = target.IsDynamicAdded
                        };
                    }

                    viewModel.GeneratedOptions[index] = updated;
                }
            }, CanModifySelected());
        }

        if (viewModel.SetSelectedOptionCommand == null)
        {
            viewModel.SetSelectedOptionCommand = ReactiveCommand.Create(() =>
            {
                if (viewModel.GeneratedOptions.Count == 0)
                {
                    return;
                }

                var currentTargets = GetSelectedTargets();
                var current = currentTargets.Count > 0 ? currentTargets[0] : null;
                var currentIndex = current != null ? viewModel.GeneratedOptions.IndexOf(current) : -1;
                var targetIndex = currentIndex + 1;
                if (targetIndex < 0 || targetIndex >= viewModel.GeneratedOptions.Count)
                {
                    targetIndex = 0;
                }
                var target = viewModel.GeneratedOptions[targetIndex];
                viewModel.GeneratedSelectedOptions.Clear();
                viewModel.GeneratedSelectedOptions.Add(target);
            }, HasOptions());
        }
    }

    public static string ConvertToBase36(int num)
    {
        if (num == 0) return "0";
        const string chars  = "0123456789abcdefghijklmnopqrstuvwxyz";
        string       result = "";
        while (num > 0)
        {
            int remainder = num % 36;
            result =  chars[remainder] + result;
            num    /= 36;
        }
        return result;
    }

    private void HandleSizeTypeChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is SelectViewModel viewModel)
        {
            if (e.CheckedOption.Tag is SizeType sizeType)
            {
                viewModel.SelectSizeType = sizeType;
            }
        }
    }

}

public record CustomOption : SelectOption
{
    public string? Description { get; set; }
    public string? Emoji { get; set; }
}
