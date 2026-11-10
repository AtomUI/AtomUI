using System.Diagnostics;
using System.Runtime.CompilerServices;
using AtomUI.Animations;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls.Primitives;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Flyouts;

public class FlyoutResourceBindingLifetimeTests
{
    [Fact]
    public void Flyout_Does_Not_Discard_Global_Token_Binding_Disposables()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs"));

        Regex.IsMatch(source, @"^\s*TokenResourceBinder\.CreateGlobalTokenBinding\(this,.*;\s*$", RegexOptions.Multiline)
             .ShouldBeFalse("global resource bindings subscribe to application styles and must have a release path");
    }

    [Fact]
    public void Reusable_Flyout_Does_Not_Keep_A_Retired_Owner_Alive()
    {
        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window { Width = 700, Height = 600 };
        window.Show();
        var retired = RetireOwner(window);
        try
        {
            for (var i = 0; i < 5; i++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            retired.Owner.IsAlive.ShouldBeFalse("a reusable Flyout must release the retired owner's positioning context");
            GC.KeepAlive(retired.Flyout);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData("Shared")]
    [InlineData("CustomEnum")]
    [InlineData("String")]
    [InlineData("Own")]
    [InlineData("TypedOwn")]
    [InlineData("TypedGlobal")]
    public void Pointer_Key_Meaning_Survives_Scoped_Publish_Restore_And_Reopen(string kind)
    {
        AvaloniaTestApp.EnsureInitialized();
        object horizontal = kind switch
        {
            "Shared" => SharedTokenKind.SizeUnit,
            "CustomEnum" => PointerKey.Horizontal,
            "String" => "pointer-horizontal",
            "Own" => FlyoutHostTokenKind.HorizontalOffset,
            "TypedOwn" => ControlTokenResourceKey.Own(FlyoutHostTokens.Identity, FlyoutHostTokenKind.HorizontalOffset),
            "TypedGlobal" => ControlTokenResourceKey.Global(FlyoutHostTokens.Identity, SharedTokenKind.SizeUnit),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        object vertical = kind switch
        {
            "CustomEnum" => PointerKey.Vertical,
            "String" => "pointer-vertical",
            "Own" => FlyoutHostTokenKind.VerticalOffset,
            "TypedOwn" => ControlTokenResourceKey.Own(FlyoutHostTokens.Identity, FlyoutHostTokenKind.VerticalOffset),
            _ => horizontal
        };
        var target = new Button { Content = "Pointer target", Width = 140, Height = 40 };
        var provider = new ThemeConfigProvider { Config = KeyConfig(kind, false), Child = target };
        var window = new AtomUI.Desktop.Controls.Window { Width = 700, Height = 500, Content = provider };
        var flyout = new KeyedPointerFlyout(horizontal, vertical);
        var baseline = kind is "Shared" ? 4d : kind is "TypedGlobal" ? 6d : 16d;
        var changed = kind is "Shared" ? 8d : kind is "TypedGlobal" ? 10d : 32d;
        void Publish(bool change)
        {
            if (kind is "CustomEnum" or "String")
            {
                provider.Resources[horizontal] = change ? changed : baseline;
                provider.Resources[vertical] = change ? changed : baseline;
            }
            else { provider.Config = KeyConfig(kind, change); }
            Dispatcher.UIThread.RunJobs();
        }
        try
        {
            Publish(false);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            flyout.ShowAt(target);
            AssertOffsets(flyout, baseline);
            Publish(true);
            AssertOffsets(flyout, changed);
            Publish(false);
            AssertOffsets(flyout, baseline);
            flyout.Hide();
            Dispatcher.UIThread.RunJobs();
            flyout.IsOpen.ShouldBeFalse();
            AssertOffsets(flyout, 0);
            Publish(true);
            AssertOffsets(flyout, 0);
            flyout.ShowAt(target);
            AssertOffsets(flyout, changed);
            flyout.CloseForLifecycle();
            AssertOffsets(flyout, 0);
        }
        finally { flyout.CloseForLifecycle(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Open_Menu_Retargets_To_The_New_Provider_Without_Caller_Hide()
    {
        AvaloniaTestApp.EnsureInitialized();
        var targetA = new Button { Content = "A", Width = 140, Height = 40 };
        var targetB = new Button { Content = "B", Width = 140, Height = 40 };
        var providerA = new ThemeConfigProvider { Config = OwnOffsets(16), Child = targetA };
        var providerB = new ThemeConfigProvider { Config = OwnOffsets(32), Child = targetB };
        var window = new AtomUI.Desktop.Controls.Window
        { Width = 700, Height = 500, Content = new StackPanel { Children = { providerA, providerB } } };
        var flyout = new MenuFlyout { RequestedPlacement = PlacementMode.Pointer, IsMotionEnabled = false };
        flyout.Items.Add(new MenuItem { Header = "Real menu item" });
        var opened = 0;
        var closed = 0;
        var openingOffsets = new List<double>();
        var closingOffsets = new List<double>();
        flyout.Opened += (_, _) => { opened++; openingOffsets.Add(flyout.HorizontalOffset); };
        flyout.Closed += (_, _) => { closed++; closingOffsets.Add(flyout.HorizontalOffset); };
        try
        {
            window.Show();
            WaitFor(() => ReadOwn(targetA) == 16 && ReadOwn(targetB) == 32);
            flyout.ShowAt(targetA);
            AssertOffsets(flyout, 16);
            flyout.ShowAt(targetB);
            flyout.IsOpen.ShouldBeTrue();
            AssertOffsets(flyout, 32);
            opened.ShouldBe(2);
            closed.ShouldBe(1);
            openingOffsets.ShouldBe(new[] { 16d, 32d });
            closingOffsets.ShouldBe(new[] { 16d });
            providerB.Config = OwnOffsets(48);
            AssertOffsets(flyout, 48);
            providerA.Config = OwnOffsets(64);
            WaitFor(() => ReadOwn(targetA) == 64);
            AssertOffsets(flyout, 48);
            providerB.Config = OwnOffsets(32);
            AssertOffsets(flyout, 32);
            flyout.Hide();
            AssertOffsets(flyout, 0);
            providerB.Config = OwnOffsets(40);
            WaitFor(() => ReadOwn(targetB) == 40);
            AssertOffsets(flyout, 0);
            flyout.ShowAt(targetB);
            AssertOffsets(flyout, 40);
            flyout.CloseForLifecycle();
            AssertOffsets(flyout, 0);
        }
        finally { flyout.CloseForLifecycle(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Same_Target_Show_Cancels_The_Existing_Close_Motion_Without_Replacing_Its_Scope()
    {
        AvaloniaTestApp.EnsureInitialized();
        var target = new Button { Content = "Animated menu", Width = 140, Height = 40 };
        var provider = new ThemeConfigProvider { Config = OwnOffsets(16), Child = target };
        var window = new AtomUI.Desktop.Controls.Window { Width = 700, Height = 500, Content = provider };
        var flyout = new MenuFlyout
        {
            RequestedPlacement = PlacementMode.Pointer, IsMotionEnabled = true, OpenMotion = null,
            CloseMotion = new FadeOutMotion(), MotionDuration = TimeSpan.FromSeconds(2)
        };
        flyout.Items.Add(new MenuItem { Header = "Real animated menu" });
        var closed = 0;
        flyout.Closed += (_, _) => closed++;
        try
        {
            window.Show();
            WaitFor(() => ReadOwn(target) == 16);
            flyout.ShowAt(target);
            AssertOffsets(flyout, 16);
            var popup = flyout.Popup.ShouldBeOfType<Popup>();
            flyout.Hide();
            popup.IsPlayingCloseMotion.ShouldBeTrue();
            flyout.ShowAt(target);
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            popup.IsHostOpen.ShouldBeTrue();
            popup.PlacementTarget.ShouldBeSameAs(target);
            closed.ShouldBe(0);
            provider.Config = OwnOffsets(32);
            AssertOffsets(flyout, 32);
        }
        finally { flyout.CloseForLifecycle(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private enum PointerKey { Horizontal, Vertical }

    private sealed class KeyedPointerFlyout : Flyout
    {
        public KeyedPointerFlyout(object horizontal, object vertical)
        {
            BindPointerPlacementOffsets(horizontal, vertical);
            RequestedPlacement = PlacementMode.Pointer;
            IsMotionEnabled = false;
            Content = new Border { Width = 120, Height = 80 };
        }
    }

    private static ThemeConfig KeyConfig(string kind, bool changed) => kind switch
    {
        "Shared" => new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.SizeUnit), changed ? "8" : "4").Build(),
        "TypedGlobal" => new ThemeConfigBuilder().WithControl(FlyoutHostTokens.Identity,
            new ControlThemeConfigBuilder().WithToken(nameof(SharedTokenKind.SizeUnit), changed ? "10" : "6").Build()).Build(),
        "Own" or "TypedOwn" => OwnOffsets(changed ? 32 : 16),
        _ => new ThemeConfigBuilder().Build()
    };

    private static ThemeConfig OwnOffsets(double value) => new ThemeConfigBuilder().WithControl(FlyoutHostTokens.Identity,
        new ControlThemeConfigBuilder().WithToken("HorizontalOffset", value.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .WithToken("VerticalOffset", value.ToString(System.Globalization.CultureInfo.InvariantCulture)).Build()).Build();

    private static double ReadOwn(Control target) => ControlTokenAccessor.Capture<FlyoutHost>(target)
        .GetOwn<double>(FlyoutHostTokenKind.HorizontalOffset);

    private static void AssertOffsets(Flyout flyout, double expected)
    {
        WaitFor(() => flyout.HorizontalOffset == expected && flyout.VerticalOffset == expected);
        flyout.HorizontalOffset.ShouldBe(expected);
        flyout.VerticalOffset.ShouldBe(expected);
    }

    private static void WaitFor(Func<bool> predicate)
    {
        var watch = Stopwatch.StartNew();
        while (!predicate() && watch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Flyout Flyout, WeakReference Owner) RetireOwner(AtomUI.Desktop.Controls.Window window)
    {
        var flyout = new Flyout { Content = new Border { Width = 120, Height = 80 } };
        var owner = new FlyoutHost { Flyout = flyout, IsMotionEnabled = false, Width = 160, Height = 40 };
        window.Content = owner;
        Dispatcher.UIThread.RunJobs();
        flyout.ShowAt(owner);
        Dispatcher.UIThread.RunJobs();
        flyout.Hide();
        Dispatcher.UIThread.RunJobs();
        window.Content = new Border();
        Dispatcher.UIThread.RunJobs();
        return (flyout, new WeakReference(owner));
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
