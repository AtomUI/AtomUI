// Explicit real Win32/TSF/SendInput specialist; see README.md.
using System.Runtime.InteropServices;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using D = AtomUI.Desktop.Controls;
namespace Win31;

internal static partial class InputVerification
{
    internal static async Task Run(D.Window window)
    {
        Avalonia.Controls.TextBox[] editors =
        [
            new D.TextBox { Name = "IMETextBox", Width = 360 },
            new D.SearchEdit { Name = "IMESearch", Width = 360 },
            new D.TextArea { Name = "IMETextArea", Width = 360, Height = 100 }
        ];
        var panel = new StackPanel { Spacing = 16, Margin = new Thickness(24) };
        foreach (var editor in editors) { panel.Children.Add(editor); }
        var scope = new ThemeConfigProvider { Child = panel, Config = new ThemeConfigBuilder().Build() };
        window.Content = scope;
        window.Activate();
        var hwnd = window.TryGetPlatformHandle()!.Handle;
        await Task.Delay(500);
        var checks = 0;
        void Require(bool condition, string message)
        {
            checks++;
            if (!condition) { throw new InvalidOperationException("INPUT " + message); }
        }
        Require(Win.GetForegroundWindow() == hwnd, "owned fixture is foreground");
        ActivatePinyin();
        var ime = ImmGetContext(hwnd);
        try
        {
            if (ime != 0)
            {
                ImmSetOpenStatus(ime, true);
                ImmSetConversionStatus(ime, 1, 0);
            }
            foreach (var editor in editors)
            {
                editor.Text = "";
                Require(editor.Focus(), editor.Name + " focus accepted");
                await Task.Delay(200);
                var focusedContext = ImmGetContext(hwnd);
                for (var retry = 0; focusedContext == 0 && retry < 20; retry++)
                {
                    await Task.Delay(100);
                    focusedContext = ImmGetContext(hwnd);
                }
                if (focusedContext == 0) { throw new InvalidOperationException("No focused IMM context"); }
                try
                {
                    var opened = ImmSetOpenStatus(focusedContext, true);
                    var converted = ImmSetConversionStatus(focusedContext, 1, 0);
                    Console.WriteLine("IME_CONTEXT open=" + opened + " native=" + converted);
                }
                finally { ImmReleaseContext(hwnd, focusedContext); }
                await Task.Delay(250);
                var presenter = editor.GetVisualDescendants().OfType<TextPresenter>().Single();
                var compositionEvents = 0;
                presenter.PropertyChanged += (_, args) =>
                {
                    if (args.Property == TextPresenter.PreeditTextProperty) { compositionEvents++; }
                };
                await Keys(hwnd, "ni");
                if (string.IsNullOrEmpty(presenter.PreeditText))
                {
                    Console.WriteLine("INPUT_PREPARATION English mode observed; switching fixture IME with Shift");
                    Tap(hwnd, 0x10);
                    await Task.Delay(150);
                    editor.Text = "";
                    await Keys(hwnd, "ni");
                }
                Console.WriteLine("IME_PREEDIT " + editor.Name + " text=" + editor.Text + " preedit=" + presenter.PreeditText);
                Require(!string.IsNullOrEmpty(presenter.PreeditText) && editor.Text == "", editor.Name + " real noncommitted TSF composition");
                var preedit = presenter.PreeditText;
                var caret = editor.CaretIndex;
                scope.Config = new ThemeConfigBuilder().WithToken("FontSize", "19").Build();
                await Task.Delay(350);
                Require(editor.FontSize == 19, editor.Name + " hot theme reaches focused editor");
                Require(presenter.PreeditText == preedit && editor.Text == "" && editor.CaretIndex == caret,
                    editor.Name + " theme keeps exact composition/caret");
                scope.Config = new ThemeConfigBuilder().Build();
                await Task.Delay(350);
                Require(editor.FontSize == 14 && presenter.PreeditText == preedit, editor.Name + " restore keeps composition");
                await Keys(hwnd, "hao");
                Tap(hwnd, 0x20);
                await Task.Delay(300);
                Console.WriteLine("IME_COMMIT " + editor.Name + " text=" + editor.Text + " preedit=" + presenter.PreeditText + " events=" + compositionEvents);
                Require(editor.Text == "你好" && string.IsNullOrEmpty(presenter.PreeditText), editor.Name + " physical pinyin commits Chinese");
                Require(compositionEvents > 0 && editor.CaretIndex == editor.Text.Length, editor.Name + " composition events and committed caret");
                await Keys(hwnd, "ni");
                Require(!string.IsNullOrEmpty(presenter.PreeditText), editor.Name + " second composition");
                Tap(hwnd, 0x1B);
                await Task.Delay(150);
                Require(editor.Text == "你好" && string.IsNullOrEmpty(presenter.PreeditText), editor.Name + " Escape cancels only composition");
                Tap(hwnd, 0x09);
                await Task.Delay(100);
                Require(!editor.IsFocused, editor.Name + " native Tab moves focus");
            }
        }
        finally
        {
            if (ime != 0) { ImmReleaseContext(hwnd, ime); }
        }
        Console.WriteLine("INPUT_PASS checks=" + checks + " editors=3 method=Microsoft-Pinyin/physical-SendInput");
    }

    private static async Task Keys(nint hwnd, string text)
    {
        foreach (var character in text)
        {
            Tap(hwnd, (ushort)char.ToUpperInvariant(character));
            await Task.Delay(100);
        }
    }

    private static void Tap(nint hwnd, ushort key)
    {
        if (Win.GetForegroundWindow() != hwnd) { throw new InvalidOperationException("Foreground changed; refusing keyboard input"); }
        var inputs = new[] { new Input { Type = 1, VirtualKey = key }, new Input { Type = 1, VirtualKey = key, Flags = 2 } };
        if (SendInput(2, inputs, Marshal.SizeOf<Input>()) != 2) { throw new InvalidOperationException("SendInput failed " + Marshal.GetLastPInvokeError()); }
    }

    private static unsafe void ActivatePinyin()
    {
        var clsid = new Guid("33c53a50-f456-4884-b049-85fd643ecfed");
        var iid = new Guid("71c6e74c-0f28-11d8-a82a-00065b84435c");
        Marshal.ThrowExceptionForHR(CoCreateInstance(in clsid, 0, 1, in iid, out var manager));
        try
        {
            var service = new Guid("81d4e9c9-1d3b-41bc-9e6c-4b40bf79e35e");
            var profile = new Guid("fa550b04-5ad7-411f-a5ac-ca038ec515d7");
            var table = *(nint**)manager;
            var activate = (delegate* unmanaged[Stdcall]<nint, uint, ushort, Guid*, Guid*, nint, uint, int>)table[3];
            var status = activate(manager, 1, 0x0804, &service, &profile, 0, 0x10000000);
            if (status != 0) { throw new InvalidOperationException("ActivateProfile HRESULT=0x" + status.ToString("X8")); }
        }
        finally { Marshal.Release(manager); }
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private struct Input
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(8)] internal ushort VirtualKey;
        [FieldOffset(10)] internal ushort Scan;
        [FieldOffset(12)] internal uint Flags;
        [FieldOffset(16)] internal uint Time;
        [FieldOffset(24)] internal nuint Extra;
    }
    [LibraryImport("ole32.dll")] private static partial int CoCreateInstance(in Guid clsid, nint outer, uint context, in Guid iid, out nint result);
    [LibraryImport("user32.dll", SetLastError = true)] private static partial uint SendInput(uint count, Input[] inputs, int size);
    [LibraryImport("imm32.dll")] private static partial nint ImmGetContext(nint hwnd);
    [LibraryImport("imm32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool ImmReleaseContext(nint hwnd, nint context);
    [LibraryImport("imm32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool ImmSetOpenStatus(nint context, [MarshalAs(UnmanagedType.Bool)] bool status);
    [LibraryImport("imm32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool ImmSetConversionStatus(nint context, uint conversion, uint sentence);
}
