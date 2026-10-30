using System.Globalization;
using System.Runtime.CompilerServices;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using AtomUIGallery.Localization;
using AtomUIGallery.ShowCases.Rate;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Localization;

public class GalleryLocalizationTests
{
    static GalleryLocalizationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void GalleryLocalization_Uses_Current_Localizer_And_Formatting_Culture()
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        try
        {
            manager.ChangeLanguage(LanguageTags.ZhCN);
            GalleryLocalization.Get(CaseNavigationLangResourceKind.CollapseNavigation, "fallback")
                              .ShouldBe("收起导航");
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    [Theory]
    [InlineData("en-US", "new tab 7")]
    [InlineData("zh-CN", "新增标签 7")]
    [InlineData("zh-TW", "新增標籤 7")]
    public void Generated_Catalog_Format_Uses_The_Current_Official_Language(
        string language,
        string expected)
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        var localizer = application.GetLocalizer().ShouldNotBeNull();
        try
        {
            manager.ChangeLanguage(LanguageTag.Parse(language));

            localizer.Format(TabControlShowCaseLangResourceKind.P2HeaderNewTabFormat, 7)
                     .ShouldBe(expected);
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    [Fact]
    public void GalleryLocalization_Binding_Tracks_Language_Resource_Changes()
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        var target = new TextBlock();
        try
        {
            using var binding = GalleryLocalization.CreateBinding(
                target,
                TextBlock.TextProperty,
                CaseNavigationLangResourceKind.CollapseNavigation,
                BindingPriority.LocalValue);

            manager.ChangeLanguage(LanguageTags.EnUS);
            target.Text.ShouldBe("Collapse navigation");
            manager.ChangeLanguage(LanguageTags.ZhCN);
            target.Text.ShouldBe("收起导航");
        }
        finally
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    [Fact]
    public void GalleryLocalization_Disposed_Binding_Does_Not_Retain_Target()
    {
        var reference = CreateDisposedBindingTargetReference();

        CollectGarbage();

        reference.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Gallery_ViewModel_Formatting_Uses_Language_State_Culture()
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetLanguageManager().ShouldNotBeNull();
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            manager.ChangeLanguage(LanguageTags.EnUS);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var viewModel = new RateViewModel(new TestScreen())
            {
                TwoWayValue = 2.5
            };

            viewModel.TwoWayValueSummary.ShouldBe("Selected value: 2.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            manager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDisposedBindingTargetReference()
    {
        var target = new TextBlock();
        var binding = GalleryLocalization.CreateBinding(
            target,
            TextBlock.TextProperty,
            CaseNavigationLangResourceKind.CollapseNavigation,
            BindingPriority.LocalValue);
        binding.Dispose();
        return new WeakReference(target);
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
