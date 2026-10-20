using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using Avalonia;

namespace AtomUIGallery.ShowCases.CheckBox;

public partial class CheckBoxShowCase : GalleryReactiveUserControl<CheckBoxViewModel>
{
    public const string LanguageId = nameof(CheckBoxShowCase);

    public CheckBoxShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is CheckBoxViewModel viewModel)
            {
                RefreshLocalizedContent(viewModel);
                
                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedContent(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                        .DisposeWith(disposables);
                }
                
                Disposable.Create(() =>
                {
                    viewModel.CheckBoxOptions       = null;
                    viewModel.DefaultCheckBoxOptions = null;
                    viewModel.TwoWayCheckBoxOptions = null;
                    viewModel.TwoWayCheckedOptions  = null;
                }).DisposeWith(disposables);
            }
        });
    }

    private void RefreshLocalizedContent(CheckBoxViewModel viewModel)
    {
        viewModel.RefreshLocalizedTexts();
        ConfigureCheckBoxOptions(viewModel);
    }
    
    private void ConfigureCheckBoxOptions(CheckBoxViewModel viewModel)
    {
        var apple = new CheckBoxOption()
        {
            Content = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentApple, "Apple")
        };
        var pear = new CheckBoxOption()
        {
            Content = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentPear, "Pear")
        };
        viewModel.CheckBoxOptions = new List<CheckBoxOption>
        {
            apple,
            pear,
            new ()
            {
                Content   = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentOrange, "Orange"),
                IsEnabled = false
            },
        };
        viewModel.DefaultCheckBoxOptions = new List<CheckBoxOption>
        {
            pear,
        };

        var twoWayApple = new CheckBoxOption()
        {
            Content = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentApple, "Apple")
        };
        var twoWayPear = new CheckBoxOption()
        {
            Content = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentPear, "Pear")
        };
        var twoWayOrange = new CheckBoxOption()
        {
            Content = CheckBoxShowCaseLanguage.Get(CheckBoxShowCaseLangResourceKind.P2ContentOrange, "Orange")
        };
        viewModel.ConfigureTwoWayCheckBoxOptions(twoWayApple, twoWayPear, twoWayOrange);
    }
}

internal static class CheckBoxShowCaseLanguage
{
    public static string Get(CheckBoxShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(
        CheckBoxShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
