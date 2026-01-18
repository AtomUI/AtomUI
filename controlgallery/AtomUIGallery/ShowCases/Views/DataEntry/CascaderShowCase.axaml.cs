using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class CascaderShowCase : ReactiveUserControl<CascaderViewModel>
{
    public CascaderShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is CascaderViewModel vm)
            {
                InitBasicCascaderViewData(vm);
            }
        });
        InitializeComponent();
    }
    
    private void InitBasicCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderNodes = [
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                Value = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header = "Zhong Hua Men",
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }
}