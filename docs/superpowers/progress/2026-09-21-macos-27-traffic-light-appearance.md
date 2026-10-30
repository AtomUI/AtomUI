# macOS 27 Traffic Light Appearance Follow-up

## Status

- Date recorded: 2026-09-21 (Asia/Shanghai).
- Related issue: [AtomUI/AtomUI#481](https://github.com/AtomUI/AtomUI/issues/481).
- State: pending macOS 27 and Xcode 27 native validation.
- Scope: the visual style of the Close, Minimize, and Zoom circles only.
- Out of scope: button spacing, horizontal or vertical position, title-bar padding, and content insets.

## Confirmed evidence

1. AtomUI does not draw the traffic-light circles. `MacStandardWindowButtons` resolves the native button origin and delegates to the
   macOS window helper; the helper obtains AppKit standard buttons and changes only their frame origin. It does not set button size,
   color, gradient, bezel, layer, image, or custom drawing. See
   [`MacStandardWindowButtons.cs`](../../../src/AtomUI.Desktop.Controls/Window/MacStandardWindowButtons.cs) and
   [`WindowUtils.MacOS.cs`](../../../src/AtomUI.Native/MacOS/WindowUtils.MacOS.cs).
2. Avalonia 12.1.2 obtains the three controls from `NSWindow.standardWindowButton`. On modern macOS it projects the requested frame
   theme through `NSAppearanceNameAqua` or `NSAppearanceNameDarkAqua`; the investigation found no AtomUI path that forces a legacy
   traffic-light appearance.
3. The locally built `AtomUIGallery.Desktop` apphost reports macOS SDK 26.4 in `LC_BUILD_VERSION`. The resolved Avalonia 12.1.2
   `libAvaloniaNative.dylib` reports macOS SDK 26.0. The investigation host is macOS 26.6.2 with Xcode 26.6 and macOS SDK 26.5, so it
   cannot provide final macOS 27 linked-on-or-after evidence.
4. The issue screenshots show a dimensional/highlight difference between the reference circles and the Gallery circles, but they also
   compare a dark reference window with a light Gallery window. Screenshot comparison alone therefore cannot separate SDK compatibility,
   effective appearance, active-window state, and application-specific customization.

## Working hypothesis

The leading hypothesis is that macOS 27 keeps the compatibility appearance for AppKit controls when the process and native backend are
still linked against macOS 26 SDKs. A host or native backend rebuilt with the macOS 27 SDK may adopt the updated circle highlights,
gradients, borders, and other system-controlled details without an AtomUI rendering change.

This is not yet a confirmed root cause. The main executable and `libAvaloniaNative.dylib` must be isolated independently because AppKit
may gate appearance using the process SDK version, the caller image SDK version, or both.

## Todo

- [ ] Reproduce on a macOS 27 machine with Xcode 27 installed.
- [ ] Create a minimal AppKit `NSWindow` baseline built with the macOS 27 SDK and use its `standardWindowButton` controls as the native
      reference. Do not use a third-party application as the only reference.
- [ ] Compare the native baseline and AtomUI Gallery under identical conditions: Light/Dark appearance, active/inactive window state,
      hover state, accessibility contrast/transparency settings, display scale, and screenshot scale.
- [ ] Record `LC_BUILD_VERSION` for the tested Gallery apphost and `libAvaloniaNative.dylib` in every comparison.
- [ ] Run the current SDK-26-linked Gallery binary on macOS 27 and preserve the circle-appearance baseline.
- [ ] Rebuild or replace only the apphost with a macOS-27-SDK-linked host while keeping the managed assemblies and Avalonia Native
      library unchanged. If the appearance updates, classify the root cause as apphost linked-SDK compatibility.
- [ ] If the apphost-only experiment does not update the appearance, rebuild only `libAvaloniaNative.dylib` with the macOS 27 SDK and
      repeat the comparison. If this updates the appearance, route the fix through Avalonia Native packaging or an upstream change.
- [ ] If neither SDK-27 rebuild updates the appearance, inspect the effective `NSAppearance`, standard-button class/control metrics,
      window style mask, title-bar transparency, and toolbar/title-bar configuration before proposing an AtomUI change.
- [ ] Verify that the selected remediation preserves native Close, Minimize, Zoom/full-screen behavior, hover glyphs, active/inactive
      transitions, Retina scaling, screen-sharing button restoration, and accessibility settings.
- [ ] Record the accepted remediation and its platform evidence. Update the Window implementation documentation and control changelog
      only if the implemented solution changes AtomUI's stable ownership, compatibility boundary, or native integration behavior.

## Guardrails

- Keep AppKit as the owner of the circle rendering and interaction contract.
- Do not manually paint or replace the traffic-light buttons merely to match one screenshot.
- Do not hard-code macOS 27 colors, gradients, borders, highlights, or pixel dimensions in AtomUI AXAML or managed code.
- Do not treat a Light-versus-Dark or active-versus-inactive comparison as proof of an SDK compatibility defect.
- Prefer an updated .NET apphost or Avalonia Native package over a private AppKit hook or AtomUI-only visual emulation.

## Completion criteria

This follow-up is complete only when:

1. the appearance difference is reproduced against a minimal AppKit baseline under matched conditions on macOS 27;
2. the apphost and native-backend SDK variables have been tested independently;
3. the exact compatibility boundary is identified with before/after visual evidence;
4. the accepted solution keeps AppKit-owned rendering and all standard window-button behavior; and
5. affected Window tests, Gallery validation, macOS 27 Light/Dark visual checks, and distribution-binary metadata checks pass.

Until those conditions are met, Issue #481 remains a platform-validation task rather than a confirmed AtomUI theme-rendering defect.
