using System.Diagnostics;
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
                InitializeBasicOptions(viewModel);
                InitializeRandomOptions(viewModel);
                InitializeMaxTagCountOptions(viewModel);
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

    private void InitializeBasicOptions(SelectViewModel viewModel)
    {
        viewModel.BasicSelectedOptions = [
            new SelectOption()
            {
                Header = "Jack",
                Value  = "jack",
            },
            new SelectOption()
            {
                Header = "Lucy",
                Value  = "lucy",
            },
            new SelectOption()
            {
                Header = "Yiminghe",
                Value  = "yiminghe",
            },
            new SelectOption()
            {
                Header    = "Disabled",
                Value     = "disabled",
                IsEnabled = false
            }
        ];
        viewModel.DefaultSelectedOptions = [viewModel.BasicSelectedOptions[2]];
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