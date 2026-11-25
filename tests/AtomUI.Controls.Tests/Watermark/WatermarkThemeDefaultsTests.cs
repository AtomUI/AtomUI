using System.Runtime.CompilerServices;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Watermark;

public class WatermarkThemeDefaultsTests
{
    static WatermarkThemeDefaultsTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Defaults_Follow_Actual_Owner_Theme_Publish_And_Reattachment()
    {
        var glyph = new TextGlyph { Text = "Confidential" };
        var target = new Border();
        var provider = new ThemeConfigProvider { Config = Config("#FF123456", "23"), Child = target };
        var window = new Window { Width = 320, Height = 200, Content = provider };
        window.Show();
        try
        {
            using (glyph.AttachToResourceOwner(target))
            {
                ColorOf(glyph).ShouldBe(Color.Parse("#FF123456"));
                glyph.FontSize.ShouldBe(23);
                var oldWidth = glyph.GetDesiredSize().Width;
                provider.Config = Config("#FF654321", "31");
                Pump();
                ColorOf(glyph).ShouldBe(Color.Parse("#FF654321"));
                glyph.FontSize.ShouldBe(31);
                glyph.GetDesiredSize().Width.ShouldBeGreaterThan(oldWidth);
                glyph.Opacity.ShouldBe(0.3);
            }
            provider.Config = Config("#FFABCDEF", "27");
            Pump();
            using (glyph.AttachToResourceOwner(target))
            {
                ColorOf(glyph).ShouldBe(Color.Parse("#FFABCDEF"));
                glyph.FontSize.ShouldBe(27);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Markup_Extension_Only_Writes_Explicit_Values_Including_Former_Defaults(bool explicitValues)
    {
        var extension = new TextGlyphExtension("Confidential");
        if (explicitValues)
        {
            extension.Foreground = Brushes.Black;
            extension.FontSize = 16;
        }
        var glyph = (TextGlyph)extension.ProvideValue(null!);
        var owner = new Border();
        owner.Resources[SharedTokenKind.ColorTextBase] = Brushes.Lime;
        owner.Resources[SharedTokenKind.FontSizeLG] = 29d;
        using (glyph.AttachToResourceOwner(owner))
        {
            ColorOf(glyph).ShouldBe(explicitValues ? Colors.Black : Colors.Lime);
            glyph.FontSize.ShouldBe(explicitValues ? 16 : 29);
            owner.Resources[SharedTokenKind.FontSizeLG] = 32d;
            Pump();
            glyph.FontSize.ShouldBe(explicitValues ? 16 : 32);
        }
    }

    [Fact]
    public void Last_Detach_Releases_Default_Bindings_And_The_Owner()
    {
        var glyph = new TextGlyph { Text = "Retained glyph" };
        var ownerReference = AttachAndRelease(glyph);
        for (var attempt = 0; attempt < 4 && ownerReference.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        ownerReference.IsAlive.ShouldBeFalse();
        GC.KeepAlive(glyph);
    }

    [Fact]
    public void Shared_Glyph_Returns_To_Previous_Active_Owner_And_Last_Detach_Releases_The_Glyph()
    {
        var first = new Border();
        first.Resources[SharedTokenKind.ColorTextBase] = Brushes.Red;
        first.Resources[SharedTokenKind.FontSizeLG] = 21d;
        var second = new Border();
        second.Resources[SharedTokenKind.ColorTextBase] = Brushes.Blue;
        second.Resources[SharedTokenKind.FontSizeLG] = 25d;
        var weak = UseSharedGlyph(first, second);
        for (var attempt = 0; attempt < 4 && weak.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        weak.IsAlive.ShouldBeFalse();
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference UseSharedGlyph(Border first, Border second)
    {
        var glyph = new TextGlyph { Text = "Shared" };
        using (glyph.AttachToResourceOwner(first))
        {
            ColorOf(glyph).ShouldBe(Colors.Red);
            using (glyph.AttachToResourceOwner(second))
            {
                ColorOf(glyph).ShouldBe(Colors.Blue);
                glyph.FontSize.ShouldBe(25);
            }
            ColorOf(glyph).ShouldBe(Colors.Red);
            glyph.FontSize.ShouldBe(21);
            glyph.Foreground = Brushes.Black;
            glyph.FontSize = 16;
            first.Resources[SharedTokenKind.FontSizeLG] = 31d;
            ColorOf(glyph).ShouldBe(Colors.Black);
            glyph.FontSize.ShouldBe(16);
        }
        return new WeakReference(glyph);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachAndRelease(TextGlyph glyph)
    {
        var owner = new Border();
        owner.Resources[SharedTokenKind.ColorTextBase] = Brushes.Lime;
        owner.Resources[SharedTokenKind.FontSizeLG] = 29d;
        using (glyph.AttachToResourceOwner(owner))
        {
            ColorOf(glyph).ShouldBe(Colors.Lime);
        }
        return new WeakReference(owner);
    }

    private static ThemeConfig Config(string color, string size) => new ThemeConfigBuilder()
        .WithToken("ColorTextBase", color).WithToken("FontSizeLG", size).Build();
    private static Color ColorOf(TextGlyph glyph) => ((ISolidColorBrush)glyph.Foreground).Color;
    private static void Pump() => Dispatcher.UIThread.RunJobs();
}
