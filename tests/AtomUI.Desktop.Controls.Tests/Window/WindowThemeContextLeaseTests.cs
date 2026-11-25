using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowThemeContextLeaseTests
{
    static WindowThemeContextLeaseTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Show_Attaches_Theme_Context_Lease_Before_The_Window_Opened_Event()
    {
        var window = new AtomUIWindow();
        ThemeContext? contextAtOpening = null;
        ThemeVariant? variantAtOpening = null;
        ThemeVariant? actualVariantAtOpening = null;
        var bridgeCountAtOpening = 0;
        window.AddHandler(
            AvaloniaWindow.WindowOpenedEvent,
            (_, _) =>
            {
                contextAtOpening = window.GetValue(ThemeScope.ContextProperty);
                variantAtOpening = window.RequestedThemeVariant;
                actualVariantAtOpening = window.ActualThemeVariant;
                bridgeCountAtOpening = window.Resources.MergedDictionaries
                                             .OfType<ThemeContextResourceBridge>()
                                             .Count();
            });

        try
        {
            window.Show();

            contextAtOpening.ShouldNotBeNull();
            variantAtOpening.ShouldBe(
                contextAtOpening.Appearance == ThemeAppearance.Dark
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light);
            actualVariantAtOpening.ShouldBe(variantAtOpening);
            bridgeCountAtOpening.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }

        window.Resources.MergedDictionaries
              .OfType<ThemeContextResourceBridge>()
              .ShouldBeEmpty();
        window.GetValue(ThemeScope.ContextProperty).ShouldBeNull();
    }

    [Fact]
    public async Task Show_Primes_The_Window_Surface_With_The_Dark_Theme_Before_Styling()
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = Application.Current!.GetThemeManager()
                                     .ShouldBeOfType<ThemeManager>();
        var darkConfig = new ThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build();
        var darkResult = await manager.ApplyThemeAsync(
            new ThemeRequest(
                AntDesignThemes.DaybreakBlue,
                darkConfig,
                ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        darkResult.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);

        application.TryFindResource(
                       WindowTokenKind.DefaultBackground,
                       ThemeVariant.Dark,
                       out var expectedBackground)
                   .ShouldBeTrue();
        var expectedColor = GetColor(expectedBackground);
        var window = new AtomUIWindow();
        Color? backgroundAtOpening = null;
        Color? fallbackAtOpening = null;
        window.AddHandler(
            AvaloniaWindow.WindowOpenedEvent,
            (_, _) =>
            {
                backgroundAtOpening = GetColor(window.Background);
                fallbackAtOpening = GetColor(window.TransparencyBackgroundFallback);
            });

        try
        {
            window.Show();

            backgroundAtOpening.ShouldBe(expectedColor);
            fallbackAtOpening.ShouldBe(expectedColor);
        }
        finally
        {
            window.Close();
            await manager.ApplyThemeAsync(
                new ThemeRequest(
                    AntDesignThemes.DaybreakBlue,
                    null,
                    ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public void Show_Preserves_Explicit_Window_Surface_Backgrounds()
    {
        var expectedBackground = Colors.Magenta;
        var expectedFallback = Colors.Cyan;
        var window = new AtomUIWindow
        {
            Background = new SolidColorBrush(expectedBackground),
            TransparencyBackgroundFallback = new SolidColorBrush(expectedFallback)
        };
        Color? backgroundAtOpening = null;
        Color? fallbackAtOpening = null;
        window.AddHandler(
            AvaloniaWindow.WindowOpenedEvent,
            (_, _) =>
            {
                backgroundAtOpening = GetColor(window.Background);
                fallbackAtOpening = GetColor(window.TransparencyBackgroundFallback);
            });

        try
        {
            window.Show();

            backgroundAtOpening.ShouldBe(expectedBackground);
            fallbackAtOpening.ShouldBe(expectedFallback);
            GetColor(window.Background).ShouldBe(expectedBackground);
            GetColor(window.TransparencyBackgroundFallback).ShouldBe(expectedFallback);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Owned_Window_Uses_The_Owner_Context_Before_The_Window_Opened_Event()
    {
        var owner = new AtomUIWindow();
        var child = new AtomUIWindow();
        ThemeContext? contextAtOpening = null;
        child.AddHandler(
            AvaloniaWindow.WindowOpenedEvent,
            (_, _) => contextAtOpening = child.GetValue(ThemeScope.ContextProperty));

        try
        {
            owner.Show();
            child.Show(owner);

            contextAtOpening.ShouldBeSameAs(owner.GetValue(ThemeScope.ContextProperty));
            child.Resources.MergedDictionaries
                 .OfType<ThemeContextResourceBridge>()
                 .ShouldHaveSingleItem()
                 .OwnerContext.ShouldBeSameAs(contextAtOpening);
        }
        finally
        {
            child.Close();
            owner.Close();
        }
    }

    [Fact]
    public void Failed_Show_Releases_The_New_Context_Lease()
    {
        var window = new AtomUIWindow();
        window.Close();

        Should.Throw<InvalidOperationException>(() => window.Show());

        window.Resources.MergedDictionaries
              .OfType<ThemeContextResourceBridge>()
              .ShouldBeEmpty();
        window.GetValue(ThemeScope.ContextProperty).ShouldBeNull();
    }

    private static Color GetColor(object? value)
    {
        return value switch
        {
            Color color => color,
            ISolidColorBrush brush => brush.Color,
            _ => throw new InvalidOperationException(
                $"Expected a color resource, but found '{value?.GetType().FullName ?? "null"}'.")
        };
    }
}
