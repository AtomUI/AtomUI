# Overlay Dialog background retention proposal

Status (2026-10-29): no accepted mask fix. The initial BitmapCache experiment
was rejected by native rendering-equivalence checks and removed. The subsequent
renderer-source experiment was stopped and cleaned after the user explicitly
prohibited compiling or maintaining Avalonia packages. Continue only within
AtomUI and the existing resolved packages/public APIs. The existing Wayland
adapter is retained. This file preserves historical decisions, not an active
implementation plan or shipped Window/Dialog contract.

Current evidence, limitations and remaining acceptance are recorded in the
[Overlay mask investigation](../progress/2026-10-29-overlay-dialog-mask-performance-investigation.md).

## Evidence and intent

On the current Debug Gallery, native Wayland, 60 Hz and 5/3 scaling, a modal
BasicDialog with the original Modal page behind it misses presentations while
its mask fades. The four-way adapter/mask comparison reproduces the cost with
both actual prepared-frame and original Wayland EGL targets. Keeping the Dialog
stationary and fading only its mask still incurs the cost.

An asynchronous GL elapsed-time query measured approximately 14.7 ms median and
20 ms P95 GPU elapsed time during the fade; composition rendering was about
14.4 ms median, while layout was normally below 1 ms. These overlapping times
must not be added. A changing translucent mask requires recompositing the
background; the present renderer replays its unchanged drawing operations.

An explicitly invalidated BitmapCache on the Window's ContentFrame reduced
median drawing time to about 6 ms and GPU elapsed time to about 8 ms. Five
closing repetitions had only approximately 16.67 ms presentation intervals;
opening still had a long interval. This is prototype evidence, not acceptance.

## Approaches considered before native acceptance

The initial selection below was superseded by the rejection and user boundary
recorded later in this document. It is not a recommendation to retry the cache.

1. Retain the background's rasterized content below the overlay, using Avalonia's
   existing live-invalidating BitmapCache. This directly reduced the measured
   rendering/GPU cost. It requires an explicit lifetime, resource release and
   rendering-equivalence checks. Initially selected for experiment; subsequently rejected.
2. Move the mask opacity evaluation to composition animations. This can reduce
   UI sampling work, but alone does not remove the measured drawing/GPU cost.
   It remains a separate investigation, not the selected repair for this cost.
3. Optimize individual background decorations. Their aggregate cost is material,
   but no single decoration was shown to dominate. This remains useful follow-up
   work; it does not yet provide a validated replacement for retention.

## Historical BitmapCache proposal (withdrawn)

- Keep the original live visual/logical tree, mask opacity animation, Dialog
  animation, durations, layout, clipping and hit testing.
- Window owns a reference-counted background-retention lease. An animated,
  modal, default-window Overlay Dialog holds one lease until host teardown.
  Modeless Dialogs, native Dialog windows, and explicit OverlayScope hosts do
  not participate in this initial implementation.
- WindowTheme declares BitmapCache on ContentFrame while the lease is active.
  The overlay remains outside that cached subtree. Preserve application-owned
  cache configuration and restore the original effective state after the last
  lease; nested dialogs cannot prematurely release each other's background.
  The current template part carries an internal attached retention state;
  detached templates lose that state and cannot match a later lease. A normal
  theme-priority compiled binding maps that state to the declared cache. This
  preserves application styles added during a modal lifetime; a conditional
  class selector would have higher priority and mask those styles.
- Cache content remains live: text, theme, animation, scrolling and resize
  changes invalidate and redraw it through Avalonia. This is not a frozen
  screenshot. Layout, input and automation still use the original controls.
- Keep ClearType enabled and additional device-pixel snapping disabled for the
  measured Skia path. The initial snapping prototype shifted content by one
  physical pixel; disabling ClearType changed glyph edges. With ClearType and
  no additional snapping, actual GL framebuffer readback differed from the
  uncached scene at 22 of 3,250,500 pixels, by one channel level at most.
- Native resize readback exposed another Avalonia 12.1.4 cache boundary: cache
  extent calculation truncates floating-point values (the integer test compares
  each extent with itself), then stretches the bitmap into the original bounds.
  The theme uses RenderAtScale=1.00000001, below float precision, to prevent an
  integral physical extent from falling below its integer. A temporary native
  probe reduced resize differences from 46,150 pixels (max 124) to 20 (max 1).
  This compatibility setting must be revalidated when updating Avalonia.
- Avalonia 12.1.4 does not include CacheModeProperty in Visual.AffectsRender.
  Explicitly invalidate the cache target when its effective cache mode changes;
  otherwise an already attached control may never synchronize that change.
  Use normal public APIs, with a focused compatibility comment, no new reflection.
- Release leases on close, cancellation, failed show, disposal and owner teardown.
  Resource allocation follows the cache lifetime rather than every animation
  tick. No persistent background cache after the last modal host is removed.

## Acceptance required by the original experiment

1. Temporary failing tests for acquisition/restoration, nested ownership, custom
   CacheMode preservation, failed/cancelled show, removal, template replacement,
   resize and live background updates; delete after validation.
2. Same real Gallery scene and presentation feedback, opening/closing separately,
   20 repetitions each, retaining first-frame latency and long intervals. Check
   the fade-active range separately from post-close restoration frames.
3. Framebuffer comparison at default size and fractional-scale resize. Preserve
   text, backgrounds and clipping; this does not replace real-window visual and
   pointer/keyboard acceptance.
4. Focused Window/Dialog tests after cleaning temporary artifacts. Existing four
   positioning failures remain explicitly identified as baseline failures.
5. NativeAOT compatibility and Windows behavior remain acceptance obligations;
   do not claim completion while applicable evidence is missing.

No commit or push is authorized. Existing uncommitted changes must be preserved.

## Native acceptance result and architecture decision

The default Gallery image was insufficient coverage. Although live text updates
and the corrected fractional resize differed by at most one channel level,
a sparse transformed child caused 456 changed pixels (maximum channel delta 121).
A full-size transparent extent removed the sparse allocation geometry problem but
still changed transparent text: 1,968 pixels, maximum delta 96. Enabling ClearType
on the offscreen target did not preserve the original glyph edges.

The current renderer sizes a cache from subtree drawing bounds and maps its pixel
surface back to those original bounds. A tiny scale offset only handles near-integer
floating-point underflow; it cannot handle genuinely fractional bounds. An opaque
fill is also unsafe: copying Window.Background can differ from the actual drawn
frame backdrop, and the native comparison found 40 changed bottom-corner pixels
(maximum delta 133) even before enabling the parent cache. Custom frame layers,
transparency and live theme changes make a copied brush an invalid general repair.

The retention candidate and its temporary lifecycle tests have been removed.
Do not adopt it conditionally for the measured Gallery page, disable ClearType,
change clipping/layout, or freeze the page to make these checks pass.

A future retention design must cache the original composed background pixels in
physical target coordinates, including their real backdrop, rather than rerender
transparent text into a new surface. It must refresh on any background visual,
resource, size, scale, frame-layer or device change, preserve original alpha and
edges, and release with the final modal owner. The current public BitmapCache API
does not establish those properties. The subsequent renderer-source route was
withdrawn by the user; this observation does not authorize framework builds, a
maintained fork, a dependency upgrade or a DLL replacement.

Separately, a temporary host experiment compared UI-driven mask sampling with a
composition animation while retaining the original Dialog body, background and
nominal durations. It showed no stable benefit and did not align composition
completion with UI teardown. That diagnostic host was removed; no composition
playback change remains in the product. The investigation records its data and
limitations alongside the unresolved GPU/background and post-draw waits.

## Withdrawn renderer-source experiment: retain the composed backdrop

The user explicitly rejected compiling or maintaining Avalonia packages on
2026-10-29. This supersedes the earlier feasibility approval. The temporary
source build was stopped before launching the host or implementing renderer
retention; its exported source and binaries were removed. Workspace dependencies,
installed assemblies and the NuGet cache were not replaced. The following is a
withdrawn proposal, not an authorized next step. It proposed an
independent temporary host built from the exact resolved renderer source; do not
replace workspace dependencies, installed assemblies or the NuGet cache.

1. Establish a render boundary immediately before the first modal overlay. Cache
   the pixels already composed from the real underlay and content, including
   their original alpha, text rendering and corner coverage. Do not synthesize a
   replacement background brush or use RenderTargetBitmap.
2. Retain that GPU image in physical target coordinates. On subsequent overlay
   frames, restore those pixels without resampling, then draw the original mask
   and Dialog. Visual/logical layout and input retain their original owners.
3. Track background invalidation separately from overlay-only invalidation. Any
   background/resource/ancestor change refreshes the retained image; generation
   changes for size, scale, configure, device, target or decoration retire it.
   Without reliable invalidation, use the original renderer immediately.
4. Confirm this separation can be represented by the renderer without private
   field mutation, freezing content, or broad reflection. The current platform
   surface bridge sees only a finished frame; it does not provide this boundary
   or background invalidation ownership. It cannot establish correctness alone.
5. First gate: native default, sparse transparent text, fractional transforms and
   resize, custom opaque/translucent frame layers, live theme/text/scroll updates,
   nested modals and teardown must preserve pixels and resource lifetimes.
   Only then repeat twenty opens/closes and investigate residual presentation
   delay. No source integration, dependency upgrade or public API change follows
   automatically from a successful temporary prototype.

If the renderer cannot expose this boundary simply and safely, reject this route
rather than accumulating conditions around the failed BitmapCache implementation.


## Current boundary after user correction

Continue only within AtomUI and the existing resolved Avalonia packages/public
APIs. Do not build a custom Avalonia renderer, maintain a fork/package, or replace
framework assemblies as an implementation or verification route. Source may be
read to understand behavior. Neither rejected cache strategy is a product fix.
