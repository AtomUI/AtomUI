using System.Runtime.CompilerServices;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using AtomUIGallery.Localization;
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

}
