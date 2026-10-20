using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using Avalonia;

namespace AtomUIGallery.ShowCases.Rate;

public partial class RateShowCase : GalleryReactiveUserControl<RateViewModel>
{
    public const string LanguageId = nameof(RateShowCase);

    public RateShowCase()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            if (DataContext is RateViewModel viewModel)
            {
                ConfigureLocalizedTooltips(viewModel);

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => ConfigureLocalizedTooltips(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                        .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.Tooltips = null;
                }).DisposeWith(disposables);
            }
        });
    }

    private void ConfigureLocalizedTooltips(RateViewModel viewModel)
    {
        viewModel.Tooltips = new List<string>
        {
            RateShowCaseLanguage.Get(RateShowCaseLangResourceKind.P2TooltipTerrible, "terrible"),
            RateShowCaseLanguage.Get(RateShowCaseLangResourceKind.P2TooltipBad, "bad"),
            RateShowCaseLanguage.Get(RateShowCaseLangResourceKind.P2TooltipNormal, "normal"),
            RateShowCaseLanguage.Get(RateShowCaseLangResourceKind.P2TooltipGood, "good"),
            RateShowCaseLanguage.Get(RateShowCaseLangResourceKind.P2TooltipWonderful, "wonderful"),
        };
        SyncActiveTooltip(viewModel, 0);
        viewModel.RefreshLocalizedState();
    }

    private void HandleValueChanged(object? sender, RateValueChangedEventArgs e)
    {
        if (DataContext is RateViewModel viewModel)
        {
            SyncActiveTooltip(viewModel, e.NewValue);
        }
    }

    private static void SyncActiveTooltip(RateViewModel viewModel, double value)
    {
        var index = (int)Math.Round(value, MidpointRounding.AwayFromZero) - 1;
        if (viewModel.Tooltips?.Count > 0 && index >= 0 && index < viewModel.Tooltips.Count)
        {
            viewModel.ActiveTooltip = viewModel.Tooltips[index];
        }
        else
        {
            viewModel.ActiveTooltip = null;
        }
    }
}

internal static class RateShowCaseLanguage
{
    public static string Get(RateShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(
        RateShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
