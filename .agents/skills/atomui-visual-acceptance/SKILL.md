---
name: atomui-visual-acceptance
description: Use when the user asks for 视觉验收, 脚本视觉验收, scripted or screenshot-based visual acceptance, a before/after visual check of an AtomUI control fix, or a temporary desktop demo that reproduces or confirms AtomUI rendering on a real window.
---

# AtomUI Visual Acceptance

## Overview

Scripted visual acceptance is a throwaway desktop demo that hosts the control in an **AtomUI window**, drives it only through public APIs and framework input routing, captures native window screenshots, asserts on pixels plus control state, writes an evidence table and exits with a code. The demo is evidence tooling, never a repository artifact.

**Violating the letter of these rules is violating the spirit: a frame from an unsupported host or an unvalidated sampling pipeline is not evidence.**

**REQUIRED BACKGROUND:** the 临时复现 Demo and 测试与验证 sections of `docs/engineering/contributing/agent-guidelines.md`.

## Host Window (hard rule)

The demo's window class MUST derive from `AtomUI.Desktop.Controls.Window` (`atom:Window`). Never host AtomUI controls in `Avalonia.Controls.Window`, whatever the control under test is.

Why: AtomUI themes only `atom:Window`. Dropdowns, flyouts, tooltips and menus default to the overlay layer that lives in that template (`PART_VisualLayerManager`). In a plain Avalonia Window a ComboBox dropdown throws `Unable to create IPopupImpl and no overlay layer is found` and the process dies, and the host carries no AtomUI theme contract at all. A plain window is an unsupported configuration, not a product bug, so nothing observed in it says anything about the product.

| Rationalization | Reality |
| --- | --- |
| "The fix is in the control theme, window chrome doesn't matter" | Popup hosting, overlay layer, theme variant and root resources come from the window template. |
| "`tests/AtomUI.Registration.Fixtures/Shared/DesktopProgram.cs` uses Avalonia Window" | That fixture is a headless registration probe, not a visual host. |
| "ThemeManager injects the root theme into every TopLevel" | Only `atom:Window` has the templated overlay layer the popups need. |
| "Swap in atom:Window only if the AtomUI title bar is wanted" | The title bar is irrelevant; the overlay layer and host contract are the point. |

Red flags in your own draft: `using Window = Avalonia.Controls.Window;` on the host class, `new Avalonia.Controls.Window { Content = ... }`, `class DemoWindow : Window` where `Window` resolves to Avalonia's. The only place `Avalonia.Controls.Window` may appear is the generic return type of `UseMainWindow`:

```csharp
// App.cs — hand-written hook, so the generator is not needed
public class App : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }

    public static void Configure(IAtomUIBuilder builder)
    {
        builder.UseDesktopControls();
        builder.UseMainWindow(static _ => Task.FromResult<Avalonia.Controls.Window>(new AcceptanceWindow()));
    }
}

// AcceptanceWindow.cs — the host derives from the AtomUI window
public class AcceptanceWindow : AtomUI.Desktop.Controls.Window { /* controls + driver */ }
```

Bootstrap chain: `AppBuilder.Configure<App>().UsePlatformDetect().WithAtomUIDefaultOptions().UseAtomUI(App.Configure).StartWithClassicDesktopLifetime(args)`.

## Workflow

1. **Demo**: temp dir named `atomui-issue<N>-acceptance-*` outside the repo; `ProjectReference` to workspace `src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj` (a NuGet reference only when reproducing a released version, and say so); Avalonia version from `Directory.Packages.props`.
2. **Keep the trigger conditions**: original sizes, text, modes, default motion. Never remove them to stabilize frames.
3. **Identity**: log binary path, PID, NSWindow number, `ClientSize`, `RenderScaling`, and SHA256 of the runtime `AtomUI.Desktop.Controls.dll` against `.artifacts/bin/Debug/net10.0/`.
4. **Driving**: public properties and methods, `RaiseEvent(new TextInputEventArgs { ... })` for typing. No desktop coordinate clicks, no Computer Use. Move the window away from the physical cursor so hover or a stray click cannot contaminate frames.
5. **Sampling**: `Activate()`, wait 800 ms, `UpdateLayout()`, then `screencapture -x -o -l <NSWindow number>`. `RenderTargetBitmap` only as an explicitly logged fallback, never presented as native.
6. **Validity gate before any AC**: same-state A/A diff = 0; a 1 DIP perturbation is detected; restoring gives 0 again. If A/A is not 0, fix the sampling condition (activation, hover, motion settle); do not widen tolerance.
7. **Assertions**: region = part bounds via `TranslatePoint` × `RenderScaling` + title-bar offset (image height − client height × scale). Compare ink count and strongest ink against the expected token color composited over the sampled background, channel tolerance ≤ 8. Pair every pixel assertion with a state assertion (`IsEffectivelyVisible`, `Text`, theme `Status == Committed`).
8. **Theme variants**: `Application.Current.GetThemeManager().ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build(), ThemeTransitionReason.UserRequest))`, then back to light.
9. **Evidence**: PNG per step plus `results.md` with `| AC | 操作与预期 | 实测 | 结果 |`; exit 0 = all pass, 2 = an AC failed, 3 = fatal. Open the key frames yourself before claiming PASS.
10. **Cleanup**: delete the demo project, keep only PNGs and `results.md` in an evidence dir, confirm no process is left, and state in the report that the demo was deleted and which capture mode was used.

Reusable driver code (native capture, pixel analysis, cursor avoidance): `references/acceptance-driver.md`.

## Common Mistakes

- Plain Avalonia Window host: crash on any popup and invalid evidence.
- `RenderTargetBitmap` frames reported as screenshots.
- `IsMotionEnabled=false` or removed constraints to make frames stable.
- Comparing frames across activation or hover changes, then calling the diff a regression.
- `screencapture -R x,y,w,h` by coordinates: captures whatever is on screen there.
- Leaving the demo running, or leaving it inside the repo.
