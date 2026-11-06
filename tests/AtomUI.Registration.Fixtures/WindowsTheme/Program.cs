// Explicit Windows platform specialist; see README.md for scope and retention.
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using D = AtomUI.Desktop.Controls;
namespace Win31;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        return AppBuilder.Configure<ProbeApp>()
            .UsePlatformDetect()
            .With(new Avalonia.Rendering.Composition.CompositionOptions
            {
                UseRegionDirtyRectClipping = Environment.GetEnvironmentVariable("WIN31_REGION_CLIPPING") != "0"
            })
            .UseAtomUI(builder => { builder.UseDesktopControls(); builder.UseSplash(SplashMode.Off); })
            .StartWithClassicDesktopLifetime(args);
    }
}
public partial class ProbeApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            var scene = new Scene();
            var scope = new ThemeConfigProvider { Child = scene, Config = new ThemeConfigBuilder().Build() };
            var window = new D.Window { Title = "AtomUI Windows 31-owner acceptance", Width = 880, Height = 700, Position = new PixelPoint(120,80), Content = scope, CanResize = false };
            lifetime.MainWindow = window;
            Avalonia.Controls.Window? backdrop = null;
            if (Environment.GetEnvironmentVariable("WIN31_BACKDROP") == "1")
            {
                var screen = window.Screens.Primary!;
                backdrop = new Avalonia.Controls.Window
                {
                    WindowDecorations = Avalonia.Controls.WindowDecorations.None,
                    ShowInTaskbar = false,
                    Background = new SolidColorBrush(Color.FromRgb(48, 48, 48)),
                    Position = screen.Bounds.Position,
                    Width = screen.Bounds.Width / screen.Scaling,
                    Height = screen.Bounds.Height / screen.Scaling
                };
                backdrop.Show();
            }
            window.Opened += async (_, _) =>
            {
                try
                {
                    await DesktopPreparation.WaitAsync(window, backdrop);
                    if (Environment.GetEnvironmentVariable("WIN_INPUT_ONLY") == "1") { await InputVerification.Run(window); }
                    else if (Environment.GetEnvironmentVariable("WIN_G1_ONLY") == "1") { await GlobalStyleVerification.Run(window); }
                    else { await new Verification(window, scene, scope).Run(); }
                    lifetime.Shutdown(0);
                }
                catch (Exception e) { Console.Error.WriteLine("FAIL " + e); lifetime.Shutdown(1); }
                finally { backdrop?.Close(); }
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
internal sealed class Verification(D.Window window, Scene scene, ThemeConfigProvider scope)
{
    private readonly string _dir = Environment.GetEnvironmentVariable("WIN31_RUN_DIR") ?? throw new InvalidOperationException("WIN31_RUN_DIR required");
    private readonly List<Check> _checks = [];
    private readonly List<Stage> _stages = [];
    private readonly List<Control> _owners = [];
    private readonly List<Control> _public = [];
    private readonly List<D.OptionButtonGroup> _groups = [];
    private readonly List<D.OptionButton> _options = [];
    private readonly Dictionary<string, byte[]> _pixels = new();
    private readonly Dictionary<string, string> _fingerprints = new();
    private int _pointer, _pressed, _keys;
    private nint _hwnd;
    private Exception? _themeError;
    private D.Button MainButton => scene.FindControl<D.Button>("B01")!;
    private D.DropdownButton DropButton => scene.FindControl<D.DropdownButton>("D01")!;
    private D.ToggleIconButton Toggle => scene.FindControl<D.ToggleIconButton>("Toggle")!;
    public async Task Run()
    {
        Directory.CreateDirectory(_dir);
        _hwnd = window.TryGetPlatformHandle()!.Handle;
        Win.GetWindowThreadProcessId(_hwnd, out var processId);
        Require(processId == Environment.ProcessId && window.TryGetPlatformHandle()!.HandleDescriptor == "HWND", "actual Win32 HWND belongs to current process");
        // Park the pointer outside this fixture before establishing the no-input baseline.
        Win.SetCursorPos(1, 1);
        window.Activate();
        await Task.Delay(500);
        window.AddHandler(InputElement.PointerMovedEvent, (_, _) => _pointer++, RoutingStrategies.Tunnel);
        window.AddHandler(InputElement.PointerPressedEvent, (_, _) => _pressed++, RoutingStrategies.Tunnel);
        window.AddHandler(InputElement.KeyDownEvent, (_, _) => _keys++, RoutingStrategies.Tunnel);
        scope.ThemeChangeFailed += (_, e) => _themeError = e.Exception ?? new InvalidOperationException("Theme failed");
        for (var i=1; i<=9; i++) { _owners.Add(scene.FindControl<D.Button>($"B{i:00}")!); }
        for (var i=1; i<=9; i++) { _owners.Add(scene.FindControl<D.DropdownButton>($"D{i:00}")!); }
        for (var i=1; i<=4; i++) { _owners.Add(scene.FindControl<D.HyperLinkButton>($"H{i:00}")!); }
        for (var i=1; i<=3; i++) { _owners.Add(scene.FindControl<D.SplitButton>($"S{i:00}")!); }
        for (var i=1; i<=3; i++) { var group = scene.FindControl<D.OptionButtonGroup>($"G{i:00}")!; _groups.Add(group); _owners.Add(group); }
        _owners.Add(scene.FindControl<D.IconButton>("Icon")!);
        _owners.Add(Toggle);
        _owners.Add(scene.FindControl<SimpleButton>("Plain")!);
        Require(_owners.Count == 31 && _owners.All(x => x is not null), "31 exact named top-level owners");
        _public.AddRange(_owners);
        foreach (var group in _groups)
        {
            for (var i=0; i<3; i++) { var option = (D.OptionButton)group.ContainerFromIndex(i)!; _options.Add(option); _public.Add(option); }
        }
        Require(_public.Count == 40 && _options.Count == 9 && _public.Distinct(ReferenceEqualityComparer.Instance).Count() == 40, "40 distinct public instances including nine real Option containers");
        Require(_owners.OfType<D.Button>().All(b => b.IsMotionEnabled), "normal Button/Dropdown motion remains enabled");
        Require(_owners.OfType<D.SplitButton>().All(b => b.IsMotionEnabled) && _groups.All(g => g.IsMotionEnabled), "normal Split/Option motion remains enabled");
        var loadedDesktop = Path.Combine(AppContext.BaseDirectory, "AtomUI.Desktop.Controls.dll");
        Console.WriteLine($"IDENTITY dynamic={RuntimeFeature.IsDynamicCodeSupported} framework={RuntimeInformation.FrameworkDescription} desktop={loadedDesktop}");
        if (File.Exists(loadedDesktop)) { Console.WriteLine("DESKTOP_SHA256 " + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(loadedDesktop)))); }
        Console.WriteLine("ASSEMBLY Avalonia=" + typeof(Application).Assembly.GetName().Version);
        foreach (var theme in new[] { "Light", "Dark" })
        {
            var algorithms = theme == "Dark" ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark } : new[] { ThemeAlgorithm.Default };
            var transition = await thisManager().ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, new ThemeConfigBuilder().WithAlgorithms(algorithms).Build(), ThemeTransitionReason.UserRequest));
            Require(transition.Status is ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp, theme + " matched ambient theme committed");
            scope.Config = new ThemeConfigBuilder().Build();
            await WaitFor(() => MainButton.FontSize == 14 && DropButton.FontSize == 14, theme + " baseline fonts");
            await Phase(theme, "A", false);
            await Phase(theme, "AA", false);
            var originalMargin = MainButton.Margin;
            MainButton.Margin = new Thickness(originalMargin.Left + 1, originalMargin.Top, originalMargin.Right, originalMargin.Bottom);
            await Phase(theme, "1DIP", false);
            MainButton.ClearValue(Layoutable.MarginProperty);
            await Phase(theme, "reset", false);
            scope.Config = new ThemeConfigBuilder()
                .WithToken("FontSize", "19").WithToken("FontFamily", "Consolas").WithToken("ColorPrimary", "#d4380d").WithToken("BorderRadius", "9")
                .WithControl(ButtonTokens.Identity, new ControlThemeConfigBuilder().WithToken("ContentFontSize", "21").WithToken("ContentPadding", "19,7").Build())
                .WithControl(DropdownButtonTokens.Identity, new ControlThemeConfigBuilder().WithToken("ContentFontSize", "23").Build())
                .Build();
            await WaitFor(() => MainButton.FontSize == 21 && DropButton.FontSize == 23, theme + " B Own fonts");
            await Phase(theme, "B", true);
            scope.Config = new ThemeConfigBuilder().Build();
            await WaitFor(() => MainButton.FontSize == 14 && DropButton.FontSize == 14, theme + " return fonts");
            await Phase(theme, "returned", false);
            foreach (var name in new[] { "AA", "reset", "returned" })
            {
                var diff = Difference(_pixels[theme+"-A"], _pixels[theme+"-"+name]);
                Console.WriteLine($"COMPARE {theme} A/{name} pixels={diff.Pixels} max={diff.Max}");
                Require(diff.Pixels == 0, theme + " exact full-window A/" + name);
            }
            foreach (var name in new[] { "1DIP", "B" })
            {
                var diff = Difference(_pixels[theme+"-A"], _pixels[theme+"-"+name]);
                Console.WriteLine($"POSITIVE {theme} A/{name} pixels={diff.Pixels} max={diff.Max}");
                Require(diff.Pixels > 0, theme + " actual full-window " + name + " change detected");
            }
        }
        Require(_stages.Count == 12 && _stages.All(s => s.Frames.Length == 3), "12 stages / 36 complete native frames");
        Require(_pointer == 0 && _pressed == 0 && _keys == 0, "no native pointer/key events during complete trajectory");
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            var modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => m.ModuleName ?? "").ToArray();
            Require(!modules.Any(m => m.Equals("coreclr.dll", StringComparison.OrdinalIgnoreCase) || m.Equals("clr.dll", StringComparison.OrdinalIgnoreCase)), "NativeAOT process has no CLR module");
        }
        Save();
        Console.WriteLine($"SUMMARY checks={_checks.Count} pass={_checks.Count(c=>c.Pass)} fail={_checks.Count(c=>!c.Pass)} stages={_stages.Count} frames={_stages.Sum(s=>s.Frames.Length)} owners=31 public=40");
        if (_checks.Any(c => !c.Pass)) { throw new InvalidOperationException("Acceptance assertion failures; see result.json"); }
    }
    private IThemeManager thisManager() => Application.Current!.GetThemeManager()!;
    private async Task WaitFor(Func<bool> condition, string name)
    {
        var watch = Stopwatch.StartNew();
        while (!condition() && watch.Elapsed < TimeSpan.FromSeconds(10))
        {
            if (_themeError is not null) { throw _themeError; }
            await Task.Delay(20);
        }
        Require(condition(), name);
        if (!condition()) { throw new InvalidOperationException("Preparation failed: " + name); }
    }
    private async Task Phase(string theme, string name, bool custom)
    {
        if (_themeError is not null) { throw _themeError; }
        await Task.Delay(1100); // Original fixture's normal-motion settle interval.
        var key = theme + "-" + name;
        Require(Math.Abs(window.ClientSize.Width-880)<0.01 && Math.Abs(window.ClientSize.Height-700)<0.01, key + " 880x700 client DIP");
        Require(window.IsActive && Win.GetForegroundWindow() == _hwnd, key + " native active identity");
        Require(_pointer == 0 && _pressed == 0 && _keys == 0, key + " unchanged no-input signature");
        Require(Toggle.IsChecked == true && _groups.Select(g=>g.SelectedIndex).SequenceEqual(new[] {1,0,2}), key + " toggle/selection retained");
        Require(_public.All(c => TopLevel.GetTopLevel(c) == window && c is TemplatedControl tc && tc.Template is not null), key + " same 40 public owners attached and templated");
        foreach (var control in _public)
        {
            var origin = control.TranslatePoint(default, window);
            Require(origin is { } p && p.X>=0 && p.Y>=0 && p.X+control.Bounds.Width<=window.ClientSize.Width+0.01 && p.Y+control.Bounds.Height<=window.ClientSize.Height+0.01 && control.Bounds.Width>0 && control.Bounds.Height>0, key + " visible geometry " + control.Name + "/" + control.GetType().Name);
            var expected = custom ? 19d : 14d;
            if (control is D.DropdownButton drop) { expected = drop.SizeType switch { CustomizableSizeType.Large => custom ? 22 : 16, CustomizableSizeType.Small => custom ? 19 : 14, _ => custom ? 23 : 14 }; }
            else if (control is D.Button button) { expected = button.SizeType switch { CustomizableSizeType.Large => custom ? 22 : 16, CustomizableSizeType.Small => custom ? 19 : 14, _ => custom ? 21 : 14 }; }
            else if (control is D.HyperLinkButton hyper) { expected = hyper.SizeType switch { CustomizableSizeType.Large => custom ? 22 : 16, CustomizableSizeType.Small => custom ? 19 : 14, _ => custom ? 19 : 14 }; }
            else if (control is D.OptionButton option) { var group = _groups[_options.IndexOf(option)/3]; expected = group.SizeType switch { CustomizableSizeType.Large => custom ? 22 : 16, CustomizableSizeType.Small => custom ? 19 : 14, _ => custom ? 19 : 14 }; }
            Require(control is TemplatedControl templated && templated.FontSize == expected, key + " actual font consumer " + control.Name + "/" + control.GetType().Name + "=" + expected);
        }
        Require(MainButton.FontSize == (custom ? 21 : 14) && DropButton.FontSize == (custom ? 23 : 14), key + " separate Own font consumers");
        for (var i=1;i<=3;i++)
        {
            var split=scene.FindControl<D.SplitButton>($"S{i:00}")!;
            var parts=split.GetVisualDescendants().OfType<D.Button>().Where(b=>b.Name is "PART_PrimaryButton" or "PART_SecondaryButton").ToArray();
            var font=i==2 ? (custom?22:16) : i==3 ? (custom?19:14) : (custom?21:14);
            Require(parts.Length==2 && parts.All(b=>b.FontSize==font), key+" Split"+i+" actual primary/secondary font="+font);
        }
        var labels=scene.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>().Where(b=>!string.IsNullOrEmpty(b.Text)).ToArray();
        Require(labels.All(b=>new FormattedText(b.Text!,System.Globalization.CultureInfo.InvariantCulture,b.FlowDirection,new Typeface(b.FontFamily,b.FontStyle,b.FontWeight),b.FontSize,b.Foreground??Brushes.Black).WidthIncludingTrailingWhitespace <= b.Bounds.Width + 1/window.RenderScaling), key+" all visible text labels fit actual layout");
        if (custom) { Require(MainButton.Padding == new Thickness(19,7), key + " Own ContentPadding reaches actual Button"); }
        Require(!scene.FindControl<D.Button>("B09")!.IsEnabled && !scene.FindControl<D.DropdownButton>("D09")!.IsEnabled, key + " disabled retained");
        var ghost = scene.FindControl<D.Button>("B06")!;
        Require(ghost.Foreground is ISolidColorBrush foreground && Math.Abs(foreground.Color.R - 128) > 64 && Math.Abs(foreground.Color.G - 128) > 64 && Math.Abs(foreground.Color.B - 128) > 64, key + " Ghost foreground contrasts with neutral sample surface");
        Require(_owners.All(c => ReferenceEquals(scene.FindControl<Control>(c.Name!), c)) && _groups.SelectMany(g => Enumerable.Range(0,3).Select(i=>g.ContainerFromIndex(i))).SequenceEqual(_options), key + " all named owners and generated containers retain exact identities");
        Require(_groups.SelectMany(g=>g.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>()).Count(t=>t.Text is "One" or "Two" or "Three") >= 9, key + " all nine option labels realized");
        var geometry = string.Join(";", _public.Select(c => { var tc=(TemplatedControl)c; return c.Name + ":" + c.TranslatePoint(default, window) + ":" + c.Bounds.Size + ":" + tc.FontSize + ":" + tc.FontFamily + ":" + tc.Foreground + ":" + tc.Background + ":" + tc.Padding + ":" + tc.BorderBrush + ":" + tc.BorderThickness + ":" + tc.CornerRadius + ":" + tc.IsEnabled; }));
        _fingerprints[key] = geometry;
        if (name is "AA" or "reset" or "returned") { Require(_fingerprints[theme+"-A"] == geometry, key + " same-instance exact geometry/font restoration"); }
        var frames = new string[3];
        byte[]? first = null;
        int width=0, height=0;
        for (var i=0; i<3; i++)
        {
            await Task.Delay(200);
            var captured = Win.Capture(_hwnd);
            frames[i] = key + "-" + i + ".bmp";
            File.WriteAllBytes(Path.Combine(_dir, frames[i]), captured.Bmp);
            width=captured.Width; height=captured.Height;
            if (first is null) { first=captured.Pixels; _pixels[key]=first; }
            else { var diff=Difference(first,captured.Pixels); Require(diff.Pixels==0, key + " stable native frame " + i + " diff=" + diff.Pixels + "/max=" + diff.Max); }
        }
        _stages.Add(new Stage(key, width, height, window.RenderScaling, frames, Convert.ToHexString(SHA256.HashData(first!)), geometry));
        Console.WriteLine($"STAGE {key} {width}x{height} scaling={window.RenderScaling} frames=3 pointer={_pointer} press={_pressed} keys={_keys} buttonfont={MainButton.FontSize} dropdownfont={DropButton.FontSize}");
        Save();
    }
    private void Require(bool pass, string name)
    {
        _checks.Add(new Check(name,pass));
        if (!pass) { Console.WriteLine("CHECK_FAIL " + name); }
    }
    private void Save() => File.WriteAllText(Path.Combine(_dir, "result.json"), JsonSerializer.Serialize(new Report(RuntimeFeature.IsDynamicCodeSupported, Environment.ProcessId, _checks.ToArray(), _stages.ToArray()), ReportContext.Default.Report));
    private static (int Pixels, int Max) Difference(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) { throw new InvalidOperationException("Frame size mismatch"); }
        int count=0,max=0;
        for (var p=0;p<a.Length;p+=4) { var d=Math.Max(Math.Abs(a[p]-b[p]),Math.Max(Math.Abs(a[p+1]-b[p+1]),Math.Abs(a[p+2]-b[p+2]))); if(d>0){count++;max=Math.Max(max,d);} }
        return(count,max);
    }
}
internal sealed record Check(string Name, bool Pass);
internal sealed record Stage(string Name, int Width, int Height, double Scaling, string[] Frames, string PixelHash, string Geometry);
internal sealed record Report(bool DynamicCode, int ProcessId, Check[] Checks, Stage[] Stages);
[JsonSerializable(typeof(Report))]
internal partial class ReportContext : JsonSerializerContext { }
internal static partial class Win
{
    [StructLayout(LayoutKind.Sequential)] internal struct Rect { internal int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo { internal uint Size; internal int Width,Height; internal ushort Planes,BitCount; internal uint Compression,SizeImage; internal int XPels,YPels; internal uint Used,Important; }
    [LibraryImport("user32.dll")] internal static partial uint GetWindowThreadProcessId(nint h, out uint p);
    [LibraryImport("user32.dll")] internal static partial nint GetForegroundWindow();
    [LibraryImport("user32.dll")] private static partial int GetSystemMetrics(int index);
    [LibraryImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static partial bool SetCursorPos(int x,int y);
    [LibraryImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static partial bool GetWindowRect(nint h,out Rect r);
    [LibraryImport("user32.dll")] private static partial nint GetDC(nint h);
    [LibraryImport("user32.dll")] private static partial int ReleaseDC(nint h,nint dc);
    [LibraryImport("gdi32.dll")] private static partial nint CreateCompatibleDC(nint dc);
    [LibraryImport("gdi32.dll")] private static partial nint CreateCompatibleBitmap(nint dc,int w,int h);
    [LibraryImport("gdi32.dll")] private static partial nint SelectObject(nint dc,nint obj);
    [LibraryImport("gdi32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static partial bool BitBlt(nint dst,int x,int y,int w,int h,nint src,int sx,int sy,uint operation);
    [LibraryImport("gdi32.dll")] private static partial int GetDIBits(nint dc,nint bmp,uint start,uint lines,byte[] bits,ref BitmapInfo info,uint usage);
    [LibraryImport("gdi32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static partial bool DeleteObject(nint h);
    [LibraryImport("gdi32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static partial bool DeleteDC(nint dc);
    internal static (byte[] Pixels,byte[] Bmp,int Width,int Height) Capture(nint hwnd)
    {
        if(!GetWindowRect(hwnd,out var r)){throw new InvalidOperationException("GetWindowRect failed");}
        int w=r.Right-r.Left,h=r.Bottom-r.Top;
        int left=GetSystemMetrics(76), top=GetSystemMetrics(77), right=left+GetSystemMetrics(78), bottom=top+GetSystemMetrics(79);
        if(r.Left<left || r.Top<top || r.Right>right || r.Bottom>bottom){throw new InvalidOperationException($"Full native window is outside screen bounds: {r.Left},{r.Top},{r.Right},{r.Bottom} vs {left},{top},{right},{bottom}");}
        var screen=GetDC(0); var mem=CreateCompatibleDC(screen); var bmp=CreateCompatibleBitmap(screen,w,h); var old=SelectObject(mem,bmp);
        try
        {
            if(!BitBlt(mem,0,0,w,h,screen,r.Left,r.Top,0x40CC0020)){throw new InvalidOperationException("BitBlt failed");}
            SelectObject(mem,old);
            var data=new byte[checked(w*h*4)]; var info=new BitmapInfo{Size=40,Width=w,Height=-h,Planes=1,BitCount=32,SizeImage=(uint)data.Length};
            if(GetDIBits(screen,bmp,0,(uint)h,data,ref info,0)!=h){throw new InvalidOperationException("GetDIBits failed");}
            using var stream=new MemoryStream(); using var writer=new BinaryWriter(stream);
            writer.Write((ushort)0x4D42);writer.Write(54+data.Length);writer.Write(0);writer.Write(54);
            writer.Write(40);writer.Write(w);writer.Write(-h);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(0);writer.Write(data.Length);
            writer.Write(0);writer.Write(0);writer.Write(0);writer.Write(0);writer.Write(data);
            return(data,stream.ToArray(),w,h);
        }
        finally {SelectObject(mem,old);DeleteObject(bmp);DeleteDC(mem);ReleaseDC(0,screen);}
    }
}
