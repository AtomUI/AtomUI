# TabControl / TabStrip source review — 2026-09-15

## Scope and environment

- Branch: `codex/tab-overflow-popup-search`; baseline: `ad3f88b7366baed79df05e052fcff09d942c9d75`.
- Review includes both base owners, Line/Card controls, items, drag reorder, scrolling/layout, shared overflow context/menu/viewer, themes, Gallery examples and their consumers.
- Host: macOS 26.6.2 (25G83), arm64; .NET SDK 10.0.300; performance configuration: Release.
- Resolved Avalonia: 12.1.2, verified against NuGet source metadata and local source commit `d3c867a9e2de379249b03dbeb3495bd7f076a81a`.
- Gallery is not launched during this review. Headless interaction/compositor tests and NativeAOT publication are separate checks; they do not claim native Windows/Linux visual verification.
- Qualification: the TabControl Gallery source declares 13 Line and 11 Card owners; TabStrip declares 12 Line and 10 Card owners. Overflow reopening is an interactive operation that can recur multiple times within one attached owner lifetime. There is no telemetry claim about a typical user's frequency.

## Findings and corrections

1. **Close callback reentrancy:** callbacks can remove/reorder items or replace `ItemsSource`. Revalidate the original source and logical item after callbacks before removing it, and preserve the intended selection if collection notifications change numeric indices. Generated containers are resolved without passing a Control to a typed `AvaloniaList<T>.IndexOf`.
2. **Reorder callback reentrancy and exceptions:** cancel a stale reorder when its callback mutates the collection/source, and release preview, pointer capture and drag state in `finally` when callbacks throw. Keep selection tied to the logical item.
3. **Generated close affordances:** owner defaults now update generated containers while preserving item-level overrides.
4. **Layout and input:** Card measurement reserves the add-button width/height on the placement axis; narrow arrange stays nonnegative. Vertical Line centering uses the vertical axis. RTL horizontal wheel movement respects physical direction and boundary chaining while preserving vertical-wheel fallback.
5. **Popup session reentrancy:** validate the opening transaction after snapshot notifications and template callbacks. Treat closing as a transaction so notifications cannot reopen a popup being destroyed. A prepared template builds application content once and retains the normal inherited DataContext contract.
6. **Default-menu allocation:** reuse empty item containers and their control templates. The cache belongs to the menu and is bounded by the latest nonempty snapshot, shrinks with that snapshot, and is discarded on context replacement/full teardown. Clear projection, header/template and presenter data before reuse; keep the same AXAML, geometry, theme resources and keyboard behavior.
7. **Withdrawn hypothesis:** the suspected CloseIcon binding replacement issue did not reproduce on the baseline. Retain the regression coverage; do not change item production code for it.
8. **Owner→item indexer-binding retention (fixed):** the indexer bindings created in `PrepareContainerForItemOverride` (`SizeType`, `IsMotionEnabled`, `ItemTemplate→ContentTemplate`, plus Card `CornerRadius`/`BorderThickness`) subscribe each `IndexerBindingExpression` to the owner's `PropertyChanged`, and Avalonia never disposes a local-value binding on its own. For generated containers `ClearContainerForItemOverride` now disposes them. For items that are their own container (a `TabItem`/`TabStripItem` added directly to `Items`) `PanelContainerGenerator` deliberately skips `ClearItemContainer` on every removal path (`src/Avalonia.Controls/Presenters/PanelContainerGenerator.cs` guards with `ItemIsOwnContainerProperty`), so the clear-side release alone left the owner holding every removed item forever — a reflection dump of the owner's `_propertyChanged` invocation list showed 12 removed items × 2 live subscriptions after `Items.Clear()`. `TabItem`/`TabStripItem` now route their direct logical detach (`e.Source == this && e.Parent is BaseTabControl/BaseTabStrip`; cascade teardown carries an ancestor's args and keeps the bindings so re-attaching restores sync) into the owner's virtual `ReleaseTabItemOwnerBindings`/`ReleaseTabStripItemOwnerBindings`, which Card owners extend with their two extra properties. Release stays owner-side so a non-card owner never touches a user-authored `CornerRadius` binding, and a card owner only releases slots its own `Prepare` binding already claimed.
9. **Indicator click reopened a dismissed popup (fixed, same day):** clicking the overflow menu indicator while its popup was open closed the popup and immediately reopened it. Root cause: the popup uses `IsLightDismissEnabled` with `OverlayDismissEventPassThrough` and no `OverlayInputPassThroughElement`, so the light-dismiss layer treated the indicator press as an outside press (closing the popup, then re-raising the press onto the button whose `Click` on release reopened it). The agreed behavior is that the indicator only ever opens: `TabScrollViewer` now sets `OverlayInputPassThroughElement` to the indicator when wiring the popup, so the dismiss layer's custom hit test lets indicator presses through to the button directly — no dismiss, no follow-up reopen, the popup stays open; `HandleMenuIndicatorClicked` keeps its unconditional open (an open popup early-returns). Dismissal elsewhere is unchanged. `TabOverflowIndicatorToggleTests` drives real headless mouse input: an indicator press/release while open keeps the popup open, a press outside the menu (point computed from the menu's actual bounds — the popup can cover most of the window in cramped layouts) still dismisses, and a normal click while closed still opens; the suite failed before the fix.

The previously confirmed edge-shadow correction is preserved. The internal edge renderer compensates the resolved Skia shadow-offset transform while keeping the directional logical Tokens and input-transparent clipping. No global Border rendering behavior is changed.

## Evidence discipline and test-host correction

The first review run captured failing close, reorder, layout and session reproductions before their fixes. A subsequent run passed all 345 pre-reuse cases while the 16 newly added reuse cases failed. Reuse tests verify identity/template reuse, current item state, resource replacement, keyboard navigation, cache shrinkage, payload collection and full teardown for all four owner types.

An apparent teardown leak was traced to a pending compositor frame, not to a production owner subscription. The strong path was:

`owner → CompositionVisual.Compositor → _objectSerializationQueue → CompositionBorderVisual.Visual → Border._templatedParent → TabOverflowMenu`

At the resolved dependency commit, `src/Avalonia.Base/Rendering/Composition/Compositor.cs:221–226` enqueues the visual strongly and `:134–136` dequeues it during commit. Layout plus dispatcher jobs did not guarantee that the headless render timer advanced. Adding one `AvaloniaHeadlessPlatform.ForceRenderTimerTick()` followed by dispatcher jobs, with no production change, changed the diagnostic from 15 live weak references to zero. The permanent GC fixtures complete this frame before collecting; the temporary reflection probe was deleted.

Source evidence for the container cost is `src/Avalonia.Controls/Presenters/PanelContainerGenerator.cs`: a nonvirtualizing reset clears and recreates containers. `TemplatedControl` retains its applied template across detach, permitting reuse of an empty container. `ItemsControl` supplies item DataContext during preparation; clearing detached `ContentPresenter` content alone does not update its DataContext. The explicit presenter update releases the former header data before caching. These are observations about this dependency version, not permanent framework contracts.

## Performance methodology

The [September 14 report](2026-09-14-tab-overflow-performance.md) is superseded. Its host swallowed popup creation failures, so its claimed interaction percentages were invalid. An intermediate overlay-host run without explicit frame completion is also excluded from acceptance: pending compositor work could cross cycle and GC boundaries.

The final host requires an actual visible overlay popup and realized menu-item bounds. Failed opens are fatal. It runs layout and bounded frame completion after open, ordinary close and disposal, following the resolved dependency's headless dispatch/render ordering. Both versions use the identical harness; the baseline is built from a clean archive of the baseline commit. New code closes through the ordinary close entry; old code uses the compatible lifecycle entry that also implements its ordinary flyout cleanup.

- Ten independent processes per version and scenario.
- Interaction: 25 unmeasured warmup cycles, 100 measured open/close cycles per owner; time, current-thread allocations, process Gen0 collections and warm/end visual counts.
- Never-open: one warmup owner, then ten created and laid-out owners per process; time, current-thread allocations and visual count.
- Headless drawing includes layout/compositor processing, not native GPU raster timing or complete Gallery page-load timing.
- Improvement formula: `(baseline median − new median) / baseline median × 100%`.
- Gates remain unchanged: at least 30% lower repeated allocation and Gen0; no stable interaction time regression; no stable never-open regression greater than 5%; bounded cache with no retained old item/template payload.

10. **ShowCase masonry hole above full-span rows (investigated, fix REVERTED):** the user reported wrong spacing between showcase cards. A full-span-placement hole hypothesis produced two rounds of panel-level balancing (two-column exact-partition DP, then an N-column local search), but the user's source screenshots identified the actual cause: stray `Margin` attributes on showcase markup inflate item boxes and punch extra gaps that no panel-level balancing can remove. All masonry balancing changes (panel, tests, docs) were reverted at the user's request. The real fix — removing the four stray `Margin` attributes (`0,0,10,0` and `0,0,30,0` on the Card Shape Position and Add/Close items) from `TabControlShowCase.axaml` and `TabStripShowCase.axaml` — was applied afterwards, with both showcase snapshot hashes updated; `AtomUIGallery.Tests` 448/448 and `AtomUI.Toolkits.GalleryBase.Tests` 93/93 pass. The unrelated `GalleryBaseCatalogTests` language-restore fix is kept (pre-existing order-dependent test isolation bug, reproducible without any masonry change).

## Final verification

Executed on 2026-09-15 after the finding-8 binding-lifetime fix, in the feature worktree (`net10.0`, headless):

- `tests/AtomUI.Desktop.Controls.Tests` full suite: **3256/3256 passed** (3 m 24 s). This includes the 16 reuse/teardown cases for all four owner kinds, the 365-case Tab filter (TabControl/TabStrip/TabReorder/TabOverflow/TabContainer/TabOwner), and `DialogPopupControlFamilyTests` 23/23 — an earlier single-run failure of `PopupConfirm_In_Dialog_Confirms_And_Closes_Its_Flyout` inside one filtered combination did not reproduce in isolation nor inside the full-suite run and is classified as cross-test interference unrelated to this branch.
- `tests/AtomUI.Desktop.Controls.Rendering.Tests`: **72/72 passed**.
- `tests/AtomUIGallery.Tests`: **448/448 passed** (localization catalog, showcase page and snapshot contracts).
- `git diff --check`: clean; no `tests/**/TestResults` artifacts (`.artifacts/` is ignored).
- NativeAOT Gallery publication (`osx-arm64`, Release, `PublishToLocal.ps1`): **passed** — "NativeAOT output validation passed for runtime 'osx-arm64'"; only the pre-existing ReactiveUI IL2104/IL3053 package warnings and Homebrew dylib version linker warnings appear.
- Indicator-reopen fix (finding 9), same day, final "indicator only opens" behavior (option B): `TabOverflowIndicatorToggleTests` **3/3 passed** (keep-open on indicator press while open, outside-press still dismisses, click while closed still opens; failed under the previous close-then-reopen code); Tab filter regression **368/368 passed**; full `AtomUI.Desktop.Controls.Tests` rerun **3259/3259 passed**; `git diff --check` clean.
- Visual acceptance for finding 9: **passed on 2026-09-15** with the user's native-machine recording (returned to the session; not stored in the repository). The recording shows: the popup opens without flicker; clicking the "···" indicator while the popup is open keeps it open (no close-reopen); clicking outside (a visible tab) dismisses it; clicking the indicator after dismissal reopens it. All four acceptance steps from the session report were exercised.

The binding-lifetime fix itself introduces no reflection or dynamic code (public `BindingOperations` API plus virtual dispatch), so it adds no new AOT-sensitive surface.
