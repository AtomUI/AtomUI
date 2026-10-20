using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using Avalonia;

namespace AtomUIGallery.ShowCases.RadioButton;

public partial class RadioButtonShowCase : GalleryReactiveUserControl<RadioButtonViewModel>
{
    public const string LanguageId = nameof(RadioButtonShowCase);

    public RadioButtonShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is RadioButtonViewModel viewModel)
            {
                ConfigureRadioOptions(viewModel);

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => ConfigureRadioOptions(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.RadioOptions = null;
                    viewModel.ClearTwoWayRadioOptions();
                }).DisposeWith(disposables);
            }
        });
    }

    private static void ConfigureRadioOptions(RadioButtonViewModel viewModel)
    {
        viewModel.RadioOptions = new List<RadioButtonOption>
        {
            new () { Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentOptionA, "Option A") },
            new () { Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentOptionB, "Option B"), IsChecked = true },
            new () { Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentOptionC, "Option C") },
            new () { Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentOptionD, "Option D"), IsEnabled = false },
        };
        viewModel.ConfigureTwoWayRadioOptions(
            new RadioButtonOption
            {
                Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentHangzhou, "Hangzhou")
            },
            new RadioButtonOption
            {
                Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentShanghai, "Shanghai")
            },
            new RadioButtonOption
            {
                Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentBeijing, "Beijing")
            },
            new RadioButtonOption
            {
                Content = RadioButtonShowCaseLanguage.Get(RadioButtonShowCaseLangResourceKind.P2ContentChengdu, "Chengdu")
            });
    }
}

internal static class RadioButtonShowCaseLanguage
{
    public static string Get(RadioButtonShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(
        RadioButtonShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
