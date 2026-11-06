# Windows theme specialist

This executable is an explicitly invoked platform specialist, not an xUnit suite.
It preserves the current reconstructed31-owner protocol; it does not claim to be
the missing2026-10-29 historical script.

Independent value: headless tests and a successful NativeAOT publish cannot detect
wrong HWND capture, template values retained after native style retirement,
real Microsoft Pinyin composition loss, or managed/native final-display drift.
The31-owner scene keeps880×700, twelve stages, normal motion, whole-HWND capture
and exact restoration/positive1DIP comparisons. A16-root profile covers global
ordinary type styles, both appearances and four sizes. IME uses physical SendInput
only after verifying the foreground HWND belongs to this process.

Retention: these are the minimum reusable native protocols for the Windows
specialist gate. They run only when explicitly requested or required by that
gate, never on every global xUnit run. They replace the deleted one-time driver;
diagnostic reflection probes and unit reproduction tests remain temporary.
Expected execution is approximately1–2minutes per managed/native pair after
compilation; NativeAOT compilation adds several minutes. The runner owns and
cleans images, executables and intermediate outputs, retaining compact hashes.

Requirements: interactive Windows x64 desktop, exact admitted SDK/compiler packs,
the adopted Avalonia.Skia compatibility host, and Microsoft Pinyin installed for
the IME scenario. Missing prerequisites fail explicitly.

- Default scenario: set WIN31_RUN_DIR to a fresh directory.
- WIN_G1_ONLY=1: global style retirement.
- WIN_INPUT_ONLY=1: real Microsoft Pinyin composition/theme/input.
- WIN31_BACKDROP=1: use an owned uniform backdrop for complete paired HWND frames.
- WIN31_REGION_CLIPPING=0: explicitly probe the alternate composition profile.
- WIN31_STARTUP_GATE_ROOT: the specialist runner's private ready/go directory.
  The fixture writes PID, main/backdrop HWNDs and a monotonic tick, then waits
  before entering any trajectory or its original 500ms preparation period.
  The runner verifies both HWNDs belong to its directly launched process,
  activates only that window if necessary, and places its uniform backdrop
  immediately behind it. Win32 preparation uses a scoped per-monitor DPI context,
  verifies that the backdrop covers the full native bounds, and waits for four
  surrounding points to show the owned backdrop's actual uniform composed color.
  Physical bounds, sample HWNDs/colors and preparation timestamps are retained.
  The caller's DPI context is restored. After go, the
  runner performs no window or input operations; every original foreground,
  no-input and whole-frame assertion remains active.
- WindowsThemeOutputRoot MSBuild property: isolate all fixture outputs in an owned
  short path without moving product outputs or weakening path proofs.

The promoted sources retain only public behavior/state/pixel assertions; no
test-only property reset repairs template descendants or modifies Avalonia.
