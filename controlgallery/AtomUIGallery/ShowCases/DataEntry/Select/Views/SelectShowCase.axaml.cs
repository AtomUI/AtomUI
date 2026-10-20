using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls;
using Avalonia;

namespace AtomUIGallery.ShowCases.Select;

public partial class SelectShowCase : GalleryReactiveUserControl<SelectViewModel>
{
    public const string LanguageId = nameof(SelectShowCase);

    public SelectShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is SelectViewModel viewModel)
            {
                InitializeRandomOptions(viewModel);
                RefreshLocalizedOptions(viewModel);

                var languageManager = Application.Current is { } application
                    ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
                    : null;
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedOptions(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                        .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.SelectOptionsAsyncLoader = null;
                    viewModel.BasicSelectedOptions     = null;
                    viewModel.SingleLucyOptions        = null;
                    viewModel.SearchOptions            = null;
                    viewModel.CustomCountryOptions     = null;
                    viewModel.GroupedPersonOptions     = null;
                    viewModel.VariantOptions           = null;
                    viewModel.HideSelectedOptions      = null;
                    viewModel.MaxCountLimitedOptions   = null;
                    viewModel.PrefixSuffixOptions      = null;
                    viewModel.RandomOptions            = null;
                    viewModel.MaxTagCountOptions       = null;
                    viewModel.DefaultSelectedOption    = null;
                    viewModel.DefaultSelectedOptions   = null;
                    viewModel.BoundSelectedOption      = null;
                    viewModel.BoundSelectedOptions     = null;
                    viewModel.SemanticPreviewOptions   = null;
                    viewModel.SemanticPreviewSelectedOptions = null;
                    viewModel.StyleClassOptions        = null;
                }).DisposeWith(disposables);
            }
        });

        InitializeComponent();
    }

    private void HandleCustomSearchSelectAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomUISelect customSearchSelect)
        {
            customSearchSelect.Filter = new CustomFilter();
        }
    }

    private void HandleSizeTypeChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is SelectViewModel viewModel &&
            e.CheckedOption.Tag is CustomizableSizeType sizeType)
        {
            viewModel.SelectSizeType = sizeType;
        }
    }

    private void RefreshLocalizedOptions(SelectViewModel viewModel)
    {
        InitializeBasicOptions(viewModel);
        InitializeSearchOptions(viewModel);
        InitializeCustomCountryOptions(viewModel);
        InitializeGroupedPersonOptions(viewModel);
        InitializeVariantOptions(viewModel);
        InitializeHideSelectedOptions(viewModel);
        InitializeMaxCountLimitedOptions(viewModel);
        InitializeMaxTagCountOptions(viewModel);
        InitializePrefixSuffixOptions(viewModel);
        InitializeSemanticPreviewOptions(viewModel);
        InitializeStyleClassOptions(viewModel);
        viewModel.SelectOptionsAsyncLoader = new SelectOptionsAsyncLoader();
    }

    private void InitializeBasicOptions(SelectViewModel viewModel)
    {
        viewModel.BasicSelectedOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy"),
            Option(SelectShowCaseLangResourceKind.P2HeaderYiminghe, "Yiminghe", "yiminghe"),
            Option(SelectShowCaseLangResourceKind.P2HeaderDisabled, "Disabled", "disabled", isEnabled: false)
        ];
        viewModel.SingleLucyOptions = [Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy")];
        viewModel.DefaultSelectedOption  = viewModel.BasicSelectedOptions[2];
        viewModel.DefaultSelectedOptions = [viewModel.BasicSelectedOptions[2]];
        viewModel.BoundSelectedOption    = viewModel.BasicSelectedOptions[1];
        viewModel.BoundSelectedOptions =
            new System.Collections.ObjectModel.ObservableCollection<ISelectOption>
            {
                viewModel.BasicSelectedOptions[0],
                viewModel.BasicSelectedOptions[2]
            };
    }

    private static void InitializeSearchOptions(SelectViewModel viewModel)
    {
        viewModel.SearchOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy"),
            Option(SelectShowCaseLangResourceKind.P2HeaderTom, "Tom", "tom")
        ];
    }

    private static void InitializeCustomCountryOptions(SelectViewModel viewModel)
    {
        viewModel.CustomCountryOptions =
        [
            CustomOption(
                SelectShowCaseLangResourceKind.P2HeaderChina,
                "China",
                "china",
                SelectShowCaseLangResourceKind.P2DescriptionChina,
                "China",
                "\ud83c\udde8\ud83c\uddf3"),
            CustomOption(
                SelectShowCaseLangResourceKind.P2HeaderUsa,
                "USA",
                "usa",
                SelectShowCaseLangResourceKind.P2DescriptionUsa,
                "United States",
                "\ud83c\uddfa\ud83c\uddf8"),
            CustomOption(
                SelectShowCaseLangResourceKind.P2HeaderJapan,
                "Japan",
                "japan",
                SelectShowCaseLangResourceKind.P2DescriptionJapan,
                "Japan",
                "\ud83c\uddef\ud83c\uddf5"),
            CustomOption(
                SelectShowCaseLangResourceKind.P2HeaderKorea,
                "Korea",
                "korea",
                SelectShowCaseLangResourceKind.P2DescriptionKorea,
                "Korea",
                "\ud83c\uddf0\ud83c\uddf7")
        ];
    }

    private static void InitializeGroupedPersonOptions(SelectViewModel viewModel)
    {
        var manager  = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2GroupManager, "Manager");
        var engineer = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2GroupEngineer, "Engineer");
        viewModel.GroupedPersonOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack", group: manager),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy", group: manager),
            Option(SelectShowCaseLangResourceKind.P2HeaderChloe, "Chloe", "chloe", group: engineer),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucas, "Lucas", "lucas", group: engineer)
        ];
    }

    private static void InitializeVariantOptions(SelectViewModel viewModel)
    {
        viewModel.VariantOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy"),
            Option(SelectShowCaseLangResourceKind.P2HeaderYiminghe2, "yiminghe", "Yiminghe")
        ];
    }

    private static void InitializeHideSelectedOptions(SelectViewModel viewModel)
    {
        viewModel.HideSelectedOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderApples, "Apples", "Apples"),
            Option(SelectShowCaseLangResourceKind.P2HeaderNails, "Nails", "Nails"),
            Option(SelectShowCaseLangResourceKind.P2HeaderBananas, "Bananas", "Bananas"),
            Option(SelectShowCaseLangResourceKind.P2HeaderHelicopters, "Helicopters", "Helicopters")
        ];
    }

    private static void InitializeMaxCountLimitedOptions(SelectViewModel viewModel)
    {
        viewModel.MaxCountLimitedOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderAvaSwift, "Ava Swift", "Ava Swift"),
            Option(SelectShowCaseLangResourceKind.P2HeaderColeReed, "Cole Reed", "Cole Reed"),
            Option(SelectShowCaseLangResourceKind.P2HeaderMiaBlake, "Mia Blake", "Mia Blake"),
            Option(SelectShowCaseLangResourceKind.P2HeaderJakeStone, "Jake Stone", "Jake Stone"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLilyLane, "Lily Lane", "Lily Lane"),
            Option(SelectShowCaseLangResourceKind.P2HeaderRyanChase, "Ryan Chase", "Ryan Chase"),
            Option(SelectShowCaseLangResourceKind.P2HeaderZoeFox, "Zoe Fox", "Zoe Fox"),
            Option(SelectShowCaseLangResourceKind.P2HeaderAlexGrey, "Alex Grey", "Alex Grey"),
            Option(SelectShowCaseLangResourceKind.P2HeaderElleBlair, "Elle Blair", "Elle Blair")
        ];
    }

    private static void InitializePrefixSuffixOptions(SelectViewModel viewModel)
    {
        viewModel.PrefixSuffixOptions =
        [
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy"),
            Option(SelectShowCaseLangResourceKind.P2HeaderYiminghe2, "yiminghe", "Yiminghe")
        ];
    }

    private void InitializeSemanticPreviewOptions(SelectViewModel viewModel)
    {
        var options = new List<ISelectOption>
        {
            Option(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack", "jack"),
            Option(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy", "lucy"),
            Option(SelectShowCaseLangResourceKind.P2HeaderYiminghe, "Yiminghe", "yiminghe")
        };
        viewModel.SemanticPreviewOptions         = options;
        viewModel.SemanticPreviewSelectedOptions = [options[0], options[1]];
    }

    private static void InitializeStyleClassOptions(SelectViewModel viewModel)
    {
        viewModel.StyleClassOptions =
        [
            new SelectOption { Header = "GuangZhou", Content = "GuangZhou" },
            new SelectOption { Header = "ShenZhen", Content = "ShenZhen" }
        ];
    }

    private void InitializeRandomOptions(SelectViewModel viewModel)
    {
        var options = new List<SelectOption>();
        for (var i = 10; i < 36; i++)
        {
            var base36Str = ConvertToBase36(i);
            options.Add(new SelectOption
            {
                Header  = base36Str + i,
                Content = base36Str + i
            });
        }
        viewModel.RandomOptions = options;
    }

    private void InitializeMaxTagCountOptions(SelectViewModel viewModel)
    {
        var options = new List<SelectOption>();
        var labelPrefix = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2TextLongLabelPrefix, "Long label: ");
        for (var i = 10; i < 36; i++)
        {
            var base36Str = ConvertToBase36(i);
            options.Add(new SelectOption
            {
                Header  = $"{labelPrefix}{base36Str + i}",
                Content = base36Str + i
            });
        }
        viewModel.MaxTagCountOptions = options;
    }

    public static string ConvertToBase36(int num)
    {
        if (num == 0)
        {
            return "0";
        }

        const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        string result = "";
        while (num > 0)
        {
            int remainder = num % 36;
            result = chars[remainder] + result;
            num /= 36;
        }

        return result;
    }

    private static SelectOption Option(
        SelectShowCaseLangResourceKind header,
        string fallback,
        string content,
        string? group = null,
        bool isEnabled = true)
    {
        return new SelectOption
        {
            Header    = SelectShowCaseLanguage.Get(header, fallback),
            Content   = content,
            Group     = group,
            IsEnabled = isEnabled
        };
    }

    private static CustomOption CustomOption(
        SelectShowCaseLangResourceKind header,
        string headerFallback,
        string content,
        SelectShowCaseLangResourceKind description,
        string descriptionFallback,
        string emoji)
    {
        return new CustomOption
        {
            Header      = SelectShowCaseLanguage.Get(header, headerFallback),
            Content     = content,
            Description = SelectShowCaseLanguage.Get(description, descriptionFallback),
            Emoji       = emoji
        };
    }
}

public class CustomOption : SelectOption
{
    public string? Description { get; init; }
    public string? Emoji { get; init; }
}

public class CustomFilter : IValueFilter
{
    public bool Filter(object? value, object? filterValue)
    {
        if (value is string valueStr && filterValue is string filterValueStr)
        {
            return valueStr.Contains(filterValueStr, StringComparison.Ordinal);
        }

        return false;
    }
}

internal static class SelectShowCaseLanguage
{
    public static string Get(SelectShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
