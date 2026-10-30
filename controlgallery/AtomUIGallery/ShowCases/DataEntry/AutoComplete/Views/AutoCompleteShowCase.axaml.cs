using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Desktop.Controls;
using P2 = AtomUIGallery.Localization.AutoCompleteShowCaseLangResourceKind;

namespace AtomUIGallery.ShowCases.AutoComplete;

public partial class AutoCompleteShowCase : GalleryReactiveUserControl<AutoCompleteViewModel>
{
    public const string LanguageId = nameof(AutoCompleteShowCase);

    public AutoCompleteShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is AutoCompleteViewModel viewModel)
            {
                viewModel.BasicOptionsAsyncLoader            = new MockValOptionsAsyncLoader();
                viewModel.CustomOptionsAsyncLoader           = new EmailOptionsAsyncLoader();
                viewModel.CustomInputOptionsAsyncLoader      = new MockValOptionsAsyncLoader();
                viewModel.UncertainCategoryOptionsAsyncLoader = new UncertainCategoryOptionsAsyncLoader();
                InitCertainCategoryOptions(viewModel);
                InitFilterCaseOptions(viewModel);
                InitStyleClassOptions(viewModel);

                Disposable.Create(() =>
                {
                    viewModel.BasicOptionsAsyncLoader             = null;
                    viewModel.CustomOptionsAsyncLoader            = null;
                    viewModel.CustomInputOptionsAsyncLoader       = null;
                    viewModel.UncertainCategoryOptionsAsyncLoader = null;
                    viewModel.CertainCategoryOptions              = null;
                    viewModel.FilterCaseOptions                   = null;
                    viewModel.StyleClassOptions                   = null;
                    viewModel.SemanticPreviewOptions              = null;
                }).DisposeWith(disposables);
            }
        });
    }

    /// <summary>
    /// antd certain-category demo：三个类目（组件库 / 解决方案 / 文章）及带引用计数的条目。
    /// </summary>
    private void InitCertainCategoryOptions(AutoCompleteViewModel vm)
    {
        vm.CertainCategoryOptions =
        [
            new CategoryAutoCompleteOption { Header = L(P2.P2TextLibraries),  Content = "Libraries",  IsGroupHeader = true },
            new CategoryAutoCompleteOption { Header = L(P2.P2ItemAntDesign),  Content = "AntDesign",  ReferenceCount = 10000 },
            new CategoryAutoCompleteOption { Header = L(P2.P2ItemAntDesignUi), Content = "AntDesign UI", ReferenceCount = 10600 },
            new CategoryAutoCompleteOption { Header = L(P2.P2TextSolutions),  Content = "Solutions",  IsGroupHeader = true },
            new CategoryAutoCompleteOption { Header = L(P2.P2ItemAntDesignUiFaq), Content = "AntDesign UI FAQ", ReferenceCount = 60100 },
            new CategoryAutoCompleteOption { Header = L(P2.P2ItemAntDesignFaq), Content = "AntDesign FAQ", ReferenceCount = 30010 },
            new CategoryAutoCompleteOption { Header = L(P2.P2TextArticles),   Content = "Articles",   IsGroupHeader = true },
            new CategoryAutoCompleteOption { Header = L(P2.P2ItemDesignLanguage), Content = "AntDesign design language", ReferenceCount = 100000 }
        ];
    }

    private static string L(AutoCompleteShowCaseLangResourceKind kind)
    {
        return GalleryLocalization.Get(kind, kind.ToString());
    }

    /// <summary>
    /// antd non-case-sensitive demo：Burns Bay Road / Downing Street / Wall Street。
    /// </summary>
    private void InitFilterCaseOptions(AutoCompleteViewModel vm)
    {
        vm.FilterCaseOptions =
        [
            new AutoCompleteOption() { Header = "Burns Bay Road", Content = "Burns Bay Road" },
            new AutoCompleteOption() { Header = "Downing Street", Content = "Downing Street" },
            new AutoCompleteOption() { Header = "Wall Street",    Content = "Wall Street" }
        ];
    }

    /// <summary>
    /// antd style-class demo：Burnaby / Seattle / Los Angeles / San Francisco / Meet student。
    /// </summary>
    private void InitStyleClassOptions(AutoCompleteViewModel vm)
    {
        var options = new List<IAutoCompleteOption>();
        foreach (var value in new[] { "Burnaby", "Seattle", "Los Angeles", "San Francisco", "Meet student" })
        {
            options.Add(new AutoCompleteOption() { Header = value, Content = value });
        }
        vm.StyleClassOptions      = options;
        vm.SemanticPreviewOptions = options;
    }
}
