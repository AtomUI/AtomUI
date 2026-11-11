# Acceptance driver building blocks

Reusable pieces for the temporary acceptance demo. Copy into the demo project; they are not repository code.
Project: `OutputType=Exe`, `net10.0`, `TreatWarningsAsErrors=true`, `PackageReference` Avalonia + Avalonia.Desktop at the
version in `Directory.Packages.props`, `ProjectReference` to `src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj`
with `GlobalPropertiesToRemove="OutputPath"`. `SkiaSharp` arrives transitively for pixel work.

## Native window capture (macOS)

```csharp
internal static class Capture
{
    [DllImport("/usr/lib/libobjc.A.dylib")] private static extern IntPtr sel_registerName(string name);
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")] private static extern nint objc_msgSend_nint(IntPtr receiver, IntPtr selector);

    public static string Mode { get; private set; } = "unknown";
    public static nint WindowNumber { get; private set; }

    public static nint ResolveWindowNumber(Window window)
    {
        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.HandleDescriptor != "NSWindow") return 0;
        return WindowNumber = objc_msgSend_nint(handle.Handle, sel_registerName("windowNumber"));
    }

    public static SKBitmap Shoot(Window window, string path)
    {
        if (WindowNumber > 0)
        {
            var psi = new ProcessStartInfo("/usr/sbin/screencapture")
            {
                ArgumentList = { "-x", "-o", "-l", WindowNumber.ToString(), path },
                UseShellExecute = false
            };
            using var process = Process.Start(psi)!;
            process.WaitForExit();
            if (process.ExitCode == 0 && File.Exists(path))
            {
                Mode = "native-screencapture";
                return SKBitmap.Decode(path);
            }
        }
        // Fallback must be visible in the log and the report; never present it as a native frame.
        Mode = "rtb-fallback";
        var scale = window.RenderScaling;
        var size = new PixelSize((int)(window.ClientSize.Width * scale), (int)(window.ClientSize.Height * scale));
        using var rtb = new RenderTargetBitmap(size, new Vector(96 * scale, 96 * scale));
        rtb.Render(window);
        rtb.Save(path, new PngBitmapEncoderOptions());
        return SKBitmap.Decode(path);
    }
}
```

The native PNG includes the title bar: client-area pixel `y` = `round(dipY * scale) + (image.Height - round(ClientSize.Height * scale))`.

## Keep the window away from the physical cursor

```csharp
internal static class PointerLocation
{
    [StructLayout(LayoutKind.Sequential)] private struct CGPoint { public double X; public double Y; }
    private const string ApplicationServices = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    [DllImport(ApplicationServices)] private static extern IntPtr CGEventCreate(IntPtr source);
    [DllImport(ApplicationServices)] private static extern CGPoint CGEventGetLocation(IntPtr cgEvent);
    [DllImport(CoreFoundation)] private static extern void CFRelease(IntPtr cf);

    public static (double X, double Y) Location()
    {
        var e = CGEventCreate(IntPtr.Zero);
        try { var p = CGEventGetLocation(e); return (p.X, p.Y); }
        finally { CFRelease(e); }
    }
}

// In the window, before the first sample (WindowStartupLocation = Manual):
var cursor = PointerLocation.Location();
var area = Screens.Primary?.WorkingArea ?? new PixelRect(0, 0, 1440, 900);
var x = cursor.X * RenderScaling < area.X + area.Width / 2d ? area.Right - (int)(Width * RenderScaling) - 40 : area.X + 40;
var y = cursor.Y * RenderScaling < area.Y + area.Height / 2d ? area.Bottom - (int)(Height * RenderScaling) - 40 : area.Y + 40;
Position = new PixelPoint(x, y);
```

## Pixel analysis

```csharp
internal readonly record struct Region(int X, int Y, int Width, int Height);
internal readonly record struct Ink(int Count, SKColor Strongest, SKColor Background);

internal static class Pixels
{
    /// <summary>Pixels whose any channel differs by more than tolerance; int.MaxValue when sizes differ.</summary>
    public static int Diff(SKBitmap a, SKBitmap b, int tolerance) { /* loop over all pixels */ }

    /// <summary>Background = most common color in the region; ink = pixels outside tolerance of it;
    /// Strongest = ink pixel farthest from the background by RGB sum.</summary>
    public static Ink Analyze(SKBitmap bitmap, Region region, int tolerance) { /* histogram + scan */ }

    public static int ChannelDistance(SKColor a, SKColor b) =>
        Math.Max(Math.Abs(a.Red - b.Red), Math.Max(Math.Abs(a.Green - b.Green), Math.Abs(a.Blue - b.Blue)));
}

// Expected color of a token brush over the sampled background:
app.TryGetResource(SharedTokenKind.ColorTextPlaceholder, app.ActualThemeVariant, out var value);
var c = ((ISolidColorBrush)value!).Color; var a = c.A / 255d;
var expected = new SKColor((byte)Math.Round(c.R * a + bg.Red * (1 - a)), (byte)Math.Round(c.G * a + bg.Green * (1 - a)), (byte)Math.Round(c.B * a + bg.Blue * (1 - a)));
```

## Sampling rhythm and gate

```csharp
private async Task SettleAsync() { await Task.Delay(800); UpdateLayout(); Dispatcher.UIThread.RunJobs(); }
private async Task<SKBitmap> ShootAsync(string name) { Activate(); await SettleAsync(); return Capture.Shoot(this, Path.Combine(outputDir, $"{shot++:00}-{name}.png")); }

// Gate (run once before the ACs): A/A == 0; Margin shifted 1 DIP => diff > 0; restored => diff == 0 against the baseline.
```

## Typing through the framework

```csharp
var innerTextBox = combo.GetVisualDescendants().OfType<Avalonia.Controls.TextBox>().Single();
innerTextBox.Focus();
await SettleAsync();
innerTextBox.RaiseEvent(new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = "abc", Source = innerTextBox });
```

## Exit

```csharp
var code = results.All(r => r.Pass) ? 0 : 2;   // 3 = fatal exception
(Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown(code);
```
