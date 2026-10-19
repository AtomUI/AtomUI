using AtomUI.Controls.Localization;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDialog = AtomUI.Desktop.Controls.Dialog;
using AtomUIFlyout = AtomUI.Desktop.Controls.Flyout;
using AtomUIPopup = AtomUI.Desktop.Controls.Popup;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Localization;

public class CommonCatalogTests
{
    static CommonCatalogTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void UseDesktopControls_Registers_All_BuiltIn_Common_Translations()
    {
        var application = Application.Current.ShouldNotBeNull();
        var languageManager = application.GetLanguageManager().ShouldNotBeNull();
        var localizer = application.GetLocalizer().ShouldNotBeNull();
        var text = new TextBlock();
        var extension = new CommonLangResourceExtension(CommonLangResourceKind.Ok);
        var dynamicResource = extension.ProvideValue(
                new ProvideValueServiceProvider(text, TextBlock.TextProperty))
            .ShouldBeOfType<DynamicResourceExtension>();
        dynamicResource.ResourceKey.ShouldBe(CommonLangResourceKind.Ok);
        text.Bind(TextBlock.TextProperty, dynamicResource);
        var window = new AvaloniaWindow
        {
            Content = text
        };
        window.Show();

        try
        {
            text.Text.ShouldBe("Ok");
            localizer.Get(CommonLangResourceKind.Ok).ShouldBe("Ok");
            languageManager.ChangeLanguage(LanguageTags.ZhCN);
            text.Text.ShouldBe("确定");
            localizer.Get(CommonLangResourceKind.Loading).ShouldBe("正在加载数据");
            languageManager.ChangeLanguage(LanguageTags.ZhTW);
            text.Text.ShouldBe("確定");
            localizer.Get(CommonLangResourceKind.Optional).ShouldBe("(可選)");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Application_Language_State_Reaches_Window_Dialog_Popup_Flyout_And_NonVisual_Consumers()
    {
        var application = Application.Current.ShouldNotBeNull();
        var languageManager = application.GetLanguageManager().ShouldNotBeNull();
        var localizer = application.GetLocalizer().ShouldNotBeNull();
        languageManager.ChangeLanguage(LanguageTags.EnUS);

        var windowText = CreateLocalizedText();
        var dialogText = CreateLocalizedText();
        var popupText = CreateLocalizedText();
        var flyoutText = CreateLocalizedText();
        var placementTarget = new Border
        {
            Width = 80,
            Height = 24
        };
        var popup = new AtomUIPopup
        {
            PlacementTarget = placementTarget,
            ShouldUseOverlayLayer = true,
            Child = popupText
        };
        var canvas = new Canvas();
        canvas.Children.Add(windowText);
        canvas.Children.Add(placementTarget);
        canvas.Children.Add(popup);
        var themeScope = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child = canvas
        };
        var window = new AtomUIWindow
        {
            Width = 320,
            Height = 240,
            Content = themeScope
        };
        var dialog = new AtomUIDialog
        {
            PlacementTarget = placementTarget,
            Content = dialogText,
            StandardButtons = DialogStandardButton.NoButton,
            IsMotionEnabled = false
        };
        var flyout = new AtomUIFlyout
        {
            Content = flyoutText
        };
        Task? dialogTask = null;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen = true;
            flyout.ShowAt(placementTarget);
            dialogTask = dialog.OpenAsync(TestContext.Current.CancellationToken);
            PumpUntil(() =>
                TopLevel.GetTopLevel(dialogText) is not null &&
                TopLevel.GetTopLevel(popupText) is not null &&
                TopLevel.GetTopLevel(flyoutText) is not null);

            AssertLocalizedText("Ok", windowText, dialogText, popupText, flyoutText);
            ResolveForPlainObject().ShouldBe("Ok");
            localizer.Get(CommonLangResourceKind.Optional).ShouldBe("(optional)");

            languageManager.ChangeLanguage(LanguageTags.ZhTW);
            Dispatcher.UIThread.RunJobs();

            AssertLocalizedText("確定", windowText, dialogText, popupText, flyoutText);
            ResolveForPlainObject().ShouldBe("確定");
            localizer.Get(CommonLangResourceKind.Optional).ShouldBe("(可選)");
        }
        finally
        {
            if (dialogTask is not null && !dialogTask.IsCompleted)
            {
                dialog.Reject();
                PumpUntil(() => dialogTask.IsCompleted);
            }
            flyout.Hide();
            popup.IsOpen = false;
            window.Close();
            languageManager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

    private static TextBlock CreateLocalizedText()
    {
        var text = new TextBlock();
        var extension = new CommonLangResourceExtension(CommonLangResourceKind.Ok);
        var dynamicResource = extension.ProvideValue(
                new ProvideValueServiceProvider(text, TextBlock.TextProperty))
            .ShouldBeOfType<DynamicResourceExtension>();
        text.Bind(TextBlock.TextProperty, dynamicResource);
        return text;
    }

    private static string ResolveForPlainObject()
    {
        var extension = new CommonLangResourceExtension(CommonLangResourceKind.Ok);
        return extension.ProvideValue(new ProvideValueServiceProvider(new object(), new object()))
                        .ShouldBeOfType<string>();
    }

    private static void AssertLocalizedText(string expected, params TextBlock[] consumers)
    {
        consumers.ShouldAllBe(consumer => consumer.Text == expected);
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue();
    }

    private sealed class ProvideValueServiceProvider(
        object targetObject,
        object targetProperty) : IServiceProvider, IProvideValueTarget
    {
        public object TargetObject { get; } = targetObject;

        public object TargetProperty { get; } = targetProperty;

        public object? GetService(Type serviceType)
        {
            return serviceType == typeof(IProvideValueTarget) ? this : null;
        }
    }
}
