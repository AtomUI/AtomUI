using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.ContractTests;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.VisualTree;
using Sample.Controls;
using Sample.Controls.Themes.AntDesign;
using Sample.Controls.Themes.Sample;
using Sample.DesignLanguage;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.Parity.Tests;

/// <summary>
/// A design language, a control package and value packages written against the public API only. The sample language
/// derives from Ant Design; the sample controls carry no values of their own.
/// </summary>
public class SampleParityTests
{
    [Fact]
    public void The_Sample_Language_Gives_The_Sample_Controls_Its_Values()
    {
        Run(typeof(SampleAppBuilder), () =>
        {
            var (badge, tag) = Realize();
            var globalPrimary = SampleLanguage.BrandPrimary;

            // The sample derivation: the brand accent (the primary darkened by 20% in light).
            Surface(badge).ShouldBe(Color.FromRgb(
                (byte)Math.Round(globalPrimary.R * 0.8), (byte)Math.Round(globalPrimary.G * 0.8), (byte)Math.Round(globalPrimary.B * 0.8)));
            badge.GetVisualDescendants().OfType<Border>().Single().Width.ShouldBe(32);

            // The tag has no sample derivation: it inherits the Ant Design one along the language token chain.
            Surface(tag).ShouldBe(AntDesignFillSecondary());
        });
    }

    [Fact]
    public void Ant_Design_Alone_Gives_The_Same_Controls_The_Ant_Design_Values()
    {
        Run(typeof(AntDesignAppBuilder), () =>
        {
            var (badge, tag) = Realize();

            Surface(badge).ShouldBe(Color.Parse("#1677FF"));
            badge.GetVisualDescendants().OfType<Border>().Single().Width.ShouldBe(24);
            Surface(tag).ShouldBe(AntDesignFillSecondary());
        });
    }

    [Fact]
    public void The_Sample_Language_Passes_The_Design_Language_Contract()
    {
        var input = DesignLanguageContractInput.Capture(static builder =>
            builder.UseSampleControls().UseSample(static sample => sample.AddSampleControls()));

        DesignLanguageContract.Verify(input, "Sample").ShouldBeEmpty();
    }

    // antd colorFillSecondary in the default light theme: rgba(0, 0, 0, 0.06).
    private static Color AntDesignFillSecondary() => Color.FromArgb(0x0F, 0, 0, 0);

    private static Color Surface(Control control) =>
        control.GetVisualDescendants().OfType<Border>().Single().Background.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

    private static (SampleBadge Badge, SampleTag Tag) Realize()
    {
        var badge = new SampleBadge();
        var tag = new SampleTag();
        var window = new Window { Content = new StackPanel { Children = { badge, tag } } };
        window.Show();
        window.UpdateLayout();
        return (badge, tag);
    }

    private static void Run(Type appBuilder, Action assertions)
    {
        using var session = HeadlessUnitTestSession.StartNew(appBuilder, AvaloniaTestIsolationLevel.PerTest);
        session.Dispatch(assertions, CancellationToken.None).GetAwaiter().GetResult();
    }
}

/// <summary>
/// An application that enables the sample controls under the sample language.
/// </summary>
public static class SampleAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(static builder => builder.UseSampleControls().UseSample(static sample => sample.AddSampleControls()));
}

/// <summary>
/// An application that enables the sample controls under Ant Design only.
/// </summary>
public static class AntDesignAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(static builder => builder.UseSampleControls().UseAntDesign(static ant => ant.AddSampleControls()));
}

internal sealed partial class TestApplication : Application;
