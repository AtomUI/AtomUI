using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Shell;

public class GalleryWorkspaceViewModelLanguageTests
{
    static GalleryWorkspaceViewModelLanguageTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Language_Commands_Use_New_Language_Manager_And_Refresh_State()
    {
        var languageManager = new TestLanguageManager(LanguageTags.EnUS);
        var viewModel = new GalleryWorkspaceViewModel(
            CreateConfiguration(),
            null,
            GallerySystemAppearanceSource.Instance,
            languageManager);
        try
        {
            viewModel.IsEnUS.ShouldBeTrue();
            viewModel.IsZhCN.ShouldBeFalse();
            viewModel.IsZhTW.ShouldBeFalse();
            viewModel.IsPtBR.ShouldBeFalse();

            viewModel.SwitchToZhCNCommand.Execute().Subscribe();
            languageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.ZhCN);
            viewModel.IsZhCN.ShouldBeTrue();
            viewModel.IsZhTW.ShouldBeFalse();
            viewModel.IsEnUS.ShouldBeFalse();
            viewModel.IsPtBR.ShouldBeFalse();

            viewModel.SwitchToZhTWCommand.Execute().Subscribe();
            languageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.ZhTW);
            viewModel.IsZhTW.ShouldBeTrue();
            viewModel.IsZhCN.ShouldBeFalse();
            viewModel.IsEnUS.ShouldBeFalse();
            viewModel.IsPtBR.ShouldBeFalse();

            viewModel.SwitchToPtBRCommand.Execute().Subscribe();
            languageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.PtBR);
            viewModel.IsPtBR.ShouldBeTrue();
            viewModel.IsZhCN.ShouldBeFalse();
            viewModel.IsZhTW.ShouldBeFalse();
            viewModel.IsEnUS.ShouldBeFalse();

            viewModel.SwitchToEnUSCommand.Execute().Subscribe();
            languageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.EnUS);
            viewModel.IsEnUS.ShouldBeTrue();
            viewModel.IsZhCN.ShouldBeFalse();
            viewModel.IsZhTW.ShouldBeFalse();
            viewModel.IsPtBR.ShouldBeFalse();
        }
        finally
        {
            viewModel.Dispose();
        }
    }

    [Fact]
    public void Dispose_Unsubscribes_From_Language_Changes()
    {
        var languageManager = new TestLanguageManager(LanguageTags.ZhTW);
        var viewModel = new GalleryWorkspaceViewModel(
            CreateConfiguration(),
            null,
            GallerySystemAppearanceSource.Instance,
            languageManager);
        viewModel.IsZhTW.ShouldBeTrue();

        viewModel.Dispose();
        languageManager.ChangeLanguage(LanguageTags.EnUS);

        viewModel.IsZhTW.ShouldBeTrue();
        viewModel.IsEnUS.ShouldBeFalse();
    }

    private static GalleryBaseConfiguration CreateConfiguration()
    {
        var options = new GalleryBaseOptions();
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options.BuildConfiguration();
    }

    private sealed class TestRouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "Overview";

        public IScreen HostScreen { get; } = hostScreen;
    }

    private sealed class TestRouteView : UserControl, IViewFor<TestRouteViewModel>
    {
        public static readonly StyledProperty<TestRouteViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<TestRouteView, TestRouteViewModel?>(nameof(ViewModel));

        public TestRouteViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestRouteViewModel?)value;
        }
    }

    private sealed class TestLanguageManager : ILanguageManager
    {
        private readonly IReadOnlyDictionary<LanguageTag, LanguageDefinition> _definitions;

        public TestLanguageManager(LanguageTag initialLanguage)
        {
            LanguageDefinition[] definitions =
            [
                CreateDefinition(LanguageTags.EnUS, "English"),
                CreateDefinition(LanguageTags.ZhCN, "简体中文"),
                CreateDefinition(LanguageTags.ZhTW, "繁體中文"),
                CreateDefinition(LanguageTags.PtBR, "Português (Brasil)")
            ];
            _definitions = definitions.ToDictionary(static definition => definition.Tag);
            SupportedLanguages = definitions;
            Current = CreateState(_definitions[initialLanguage], revision: 0);
        }

        public LanguageState Current { get; private set; }

        public IReadOnlyList<LanguageDefinition> SupportedLanguages { get; }

        public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

        public LanguageChangeResult ChangeLanguage(LanguageTag language)
        {
            var definition = _definitions[language];
            if (language == Current.CurrentLanguage)
            {
                return LanguageChangeResult.NoOp(Current);
            }

            var oldState = Current;
            Current = CreateState(definition, checked(oldState.Revision + 1));
            var result = LanguageChangeResult.Committed(oldState, Current);
            LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(result));
            return result;
        }

        private static LanguageDefinition CreateDefinition(LanguageTag tag, string nativeName)
        {
            return new LanguageDefinition(
                tag,
                System.Globalization.CultureInfo.GetCultureInfo(tag.Value),
                nativeName,
                LanguageTextDirection.LeftToRight);
        }

        private static LanguageState CreateState(LanguageDefinition definition, long revision)
        {
            return new LanguageState(
                definition.Tag,
                definition.FormattingCulture,
                definition.TextDirection,
                revision);
        }
    }
}
