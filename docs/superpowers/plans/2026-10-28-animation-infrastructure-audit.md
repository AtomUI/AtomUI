# Animation infrastructure audit and refinement

> For agentic workers: use superpowers:executing-plans inline. Each implementation change is verified; no automatic commits. Follow project temporary-test lifecycle and focused verification constraints.

**Goal:** Audit the whole animation infrastructure, eliminate unjustified duplication, align names with ownership, and deliver measured, reviewable improvements without changing the approved public Motion model or existing UX.

**Architecture:** Avalonia owns clocks, native animations, transitions and spline solving. AtomUI Motion owns finite effect definitions, request ownership, composition, recoverable presentation and layout capability. Transition initialization owns initial-state suppression/readiness only; continuous feedback uses native repeating animation facilities where publicly supported and verified.

**Tech stack:** Avalonia 12.1.4, .NET 10, current feature/motion worktree. Local reference source ReferenceProjects/Avalonia (12.1.3); verify APIs against installed12.1.4 packages rather than assuming source tag equivalence.

**Spec:** docs/architecture/systems/motion/overview.md and control-infrastructure/transition-initialization.md. The user has approved the Motion base/subclass/extensibility model, waived legacy Motion compatibility, and now requests global audit/refinement. Internal naming cleanup follows those established responsibilities; do not invent a second unified engine.

## Constraints

- Work was done in the dedicated `feature/motion` worktree (reusing the authorized branch, no new branch/worktree/clone). The branch was linearly merged into `release/6.0` on 2026-10-30; the branch and worktree have since been deleted.
- No commit, amend, merge, push or new public API. Preserve bindings, effects, interactions, selection, geometry, clipping, scrolling and resource lifetimes.
- Existing public non-Motion Transition contracts are not automatically covered by the legacy-Motion compatibility waiver. Keep them unless replacement is equivalent and does not remove their contract.
- No private-framework reflection in new primary execution/initialization paths. Existing public compatibility entry points may remain isolated and documented.
- Do not force everything through Motion. Native Transitions, finite explicit Motion and continuous feedback have distinct responsibilities.
- Temporary tests/benchmarks first; delete after validation, zero net-new permanent cases unless independently justified. No full/change regression; exact filters or explicit iterate paths only.
- Performance claims require before/after measurements under the same harness. Native property/Rendered data is not display present; no unconditional60/120FPS claim.

## Review focus

1. Reentrant property notifications, cancellation, detach and cleanup failures must not let an old request write a new target or retain scopes.
2. Initial transitions, remount and virtualized containers must commit a complete baseline and restore the latest live binding before real subsequent changes.
3. Shared spline instances, interleaved samples, endpoints and custom easing must retain the approved curve semantics at framework numerical precision.
4. Infinite feedback must preserve elapsed-time phase across skipped frames and release on cancel/hide/detach/duration change without frame-by-frame tasks or a zero-duration busy loop.
5. Layout ownership must preserve the recently fixed native NavMenu roundtrip, full-row input and constraints; optimize only proven redundant invalidation/work.

## Audit facts

- StateMotion/StateMotionSession are initialization/registration and Transition suppression, not Motion subclasses or a playback clock.
- MotionCurves duplicates the framework SplineEasing solver with28 fixed bisection steps.
- Twelve internal Notifiable*Transition implementations and their internal base have no source/AXAML consumers after migration; public completion contracts need separate compatibility treatment.
- ProgressBar/Spin use private DisableTransitions/EnableTransitions and posted readiness, although an existing scoped initialization protocol is available.
- RunInfiniteAsync restarts finite native runs; native RunAsync rejects Infinite and native Apply is internal. Investigate public Style.Animations for the two leaf renderers rather than reflecting Apply or introducing a second clock.
- Brush/shadow/geometry adapters must be compared with native interpolation before removing them. Public API and visual equivalence are separate checks.

## Tasks

### Task1: Formal boundaries and inventory
- [x] Record findings/owners/consumer evidence and update the formal architecture classification before changing code.
- [x] Freeze the internal naming: TransitionInitialization marker, TransitionInitializationSession and ITransitionInitializationOwner, under Transitions/Initialization; same AtomUI.Animations namespace and atom: prefix.
- [x] Preserve the approved Motion base/subclass API, native automatic transitions and independent continuous feedback responsibilities.

### Task2: Framework spline and initialization naming
- [x] Characterize six curve presets, endpoints and interleaved sampling against an independent high-precision oracle; record framework tolerance and benchmark the old/new solver without changing parameters.
- [x] Replace private Bezier math with SplineEasing and remove redundant solver code.
- [x] Rename/move the internal initialization protocol and update all consumers, AXAML, tests, current docs and skill references. Do not rewrite historical research claims silently.
- [x] Run existing Core initialization/playback and representative control contracts before/after; remove temporary characterization code when validated.

### Task3: Remove dead paths and correct lifecycle/native repeating ownership
- [x] Remove unused internal Notifiable implementations and prune only proven unreachable interpolation helpers; retain public compatibility contracts explicitly.
- [x] Replace ProgressBar/Spin private-toggle/post readiness with the scoped initialization protocol; verify initial values, live transitions/bindings and remount.
- [x] Prototype native repeating Style.Animations for leaf SwitchKnob/BorderBeamPresenter; prove real phase, visibility, cancellation and duration behavior before adopting. Keep runtime style exceptions explicit when ControlTheme cannot express runtime snapshot/lifecycle equivalently.
- [x] Compare remaining native adapters and runtime frame path; only make changes with equivalent contracts or a reproduced correctness/performance defect.

### Task4: Integration and measured acceptance
- [x] Verify changed state-control families, finite Motion, shared geometry/input, progress/spin/switch/beam and Gallery with exact filters.
- [x] Use native/Skia scripted scenarios for initialization, looping and the native sidebar roundtrip; record actual identity, geometry, input and independent visual review.
- [x] Run matching before/after benchmarks; report allocations/timing at their actual scope, not as FPS guarantees.
- [x] Run Gallery NativeAOT after final runtime changes; distinguish third-party warnings and platform gaps.

### Task5: Independent final review and clean delivery
- [x] Dispatch one fresh whole-diff reviewer under executing-plans/requesting-code-review; no implementation delegation.
- [x] Re-grade findings, perform one important-fix pass with valid RED→GREEN, and ledger every deferred item or unsupported judgment.
- [x] Delete temporary tests, probes, media and dedicated artifacts; rebuild normal tools and verify the final cleaned modules and documentation links.
- [x] Deliver code, formal docs, measurements and limitations for user review; preserve worktree, do not commit.

## Execution ledger

2026-10-28: Global audit started after user review of duplicated spline solving and ambiguous StateMotion naming. Prior Motion refactor/native NavMenu fix remain uncommitted and are preserved. No product edits for this audit yet.

Task1: Formal global boundaries recorded in docs/modules/core/animations.md. Scope is internal cleanup and implementation of the approved ownership model; no public API expansion or new branch required.

Task2 audit: Registration callback can synchronously reenter while old suppression is disposed or a new suppression is constructed. The old callback can then overwrite/clear a newer registration. RegisterTarget can continue assigning a released owner after target.Reset reenters owner.Dispose. Reproduce these before repairing; use current property/registration identity and phase, not extra patch flags.

Task2 RED: Core baseline5 passed. Three temporary reentrancy cases all failed for the target defect: constructor rollback to None left Transitions null, disposal reentry cleared the newer registration, and owner disposal during target.Reset left a released parent blocking readiness. No production changes preceded this evidence.

Task2 GREEN: registration identity retired before disposal, new registrations checked after scope construction/publication, missing registrations ignored on late attach, and RegisterTarget rechecked phases/parent after Reset. Existing5 + temporary3 =8 passed. No new patch flags.

Task2 baseline benchmark:10 isolated console processes on Apple M1 Max, Release net10.0, installed Avalonia12.1.4, six presets and1,200,000 Ease calls per process. Old fixed28-step solver median396.1132ms, measured-loop allocation0, max error against independent64-step oracle5.328224070288989e-9. Interleaved sampling and endpoints passed. Pending matching native-spline measurement; do not translate this microbenchmark into FPS.

Ruling: Retain existing public non-Motion Transition compatibility contracts; remove only proven unused internal implementations. Framework BrushTransition interpolates gamma-corrected color whereas current brush helper uses premultiplied channel interpolation, so name similarity alone does not establish visual equivalence. Cost: a small public compatibility surface and semantically distinct brush adapter remain.

Ruling: Native public Style.Animations is the loop candidate; Style with no selector matches only target==host in local framework source. Prove installed12.1.4 behavior before adoption. The finite RunAsync API rejects Infinite and Apply is internal, so do not reflect it. Cost: one explicitly owned native style per active loop; native fallback must be documented if the spike disproves this path.

Task3 RED: SwitchKnob100ms loop sampled at350ms should be180 degrees, old finite-restart helper produced0. Native Style.Animations prototype fixes the real consumer (temporary test passes); generalized visibility/host-only/binding-restoration contract is being validated before broad adoption. Style installation is deferred by normal styling, so tests must pump that boundary before seeding the manual clock.

Task2/3: internal protocol renamed/moved, atom:TransitionInitialization.Mode and all owner interfaces/consumers synchronized; core8 passed before merging3 reentry cases into existing tests. Deleted13 unused internal notifier files; retained Bool interpolation used by shadows, pruned9 unreachable helpers, grouped semantic adapters under Transitions, retained public reflection methods in Reflection compatibility boundary. ProgressBar/Spin now use scoped initialization instead of private toggles/posted readiness. Representative control stage29 passed.

Task3 GREEN: native infinite loop phase, effective invisibility, host-only style, live styled binding restoration and stopping passed2 cases (one temporary real SwitchKnob + one improved existing recurring-work contract). A raw style removal lost a styled base source; a separately owned animation-priority cover across removal preserved it. The typed scalar loop adapter accepts styled feedback values with this release protocol and private direct feedback properties with native direct-binding semantics. No generic application DirectProperty preservation claim.

Ruling: perform initialization correctness repair and native-loop spike before the mechanical naming/curve rollout, keeping the observed failures attributable. All product fixes had relevant RED evidence; mechanical rename/curve reuse uses before/after characterization, not artificial failing source/type tests. Cost: the ledger is not a strict chronological task-number sequence.

Benchmark finding: .NET10 empty-property MotionPlan creation is96 bytes/call (the plan object), so do not claim LINQ allocations solely from reading GroupBy. Production net8 support also needs verification; evaluate actual presenter allocation before changing its empty-property path.

Task2/3 measured refinement: moved semantic color/brush/shadow interpolation to Transitions/TransitionInterpolation and public ColorTransition source to Transitions (AtomUI.Media namespace preserved). Removed nine unused interpolation functions, retained the bool operation actually used by shadows. Zero-duration playback/binding/cancellation characterization passed before lazy cancellation allocation; existing Core6 (including retention) passed after merged reentry cases.

Task4 paired benchmark: same Release net10.0 protocol, Apple M1 Max,10 processes each. Six preset Ease calls1,200,000/process: median393.96585ms before vs46.1643ms after; measured-loop allocation0. Dense/interleaved max normalized error vs64-step oracle changed from5.328224070288989e-9 to0.002537049161133398; endpoints exact. Empty-property plan96 bytes before/after unchanged. Empty-property presentation creation100,000/process:304→216 bytes/call and median16.1414→7.0052ms. Replaced captured Select with typed array loop; deferred CTS creation until an actual native clock run. These are preparation/solver microbenchmarks, not application FPS or total playback-allocation claims.

Ruling: use framework SplineEasing precision rather than maintain a private high-precision solver. Cost: intermediate samples are not bit-identical to the old implementation; observed normalized error0.0025371 is within the framework parameter tolerance bound0.0031. Preset control points and exact endpoints are unchanged.

Task3 additional RED→GREEN: live SwitchKnob duration changed100→200ms at50ms; old active loop ignored it (0 degrees at100ms vs expected90). Rebuild on duration change using the existing loading intent; temporary duration/phase cases and progress/spin first-load/remount/live-transition-source characterization all passed4 cases. Folded real knob duration wiring into the existing recurring-work contract and removed desktop temporary audit cases.

Task2/3 cleanup: the3 initialization reentry reproductions are merged into existing Core source-restoration/reset tests; no new permanent test case. Core final-stage6 passed including zero-duration reentrant cancellation and binding restoration after lazy CTS creation. All Task3 temporary cases removed before final module checks.

Task3 clean integration:48 selected control/recurring-work tests passed after removing temporary desktop cases. Core merged contracts retain case count, presenter304→216B measured and lazy CTS zero-duration cancellation/source-restoration characterization passed6 selected Core cases. Scope loops now use native infinite host-only style, typed styled release cover and direct-property-owned feedback semantics.

Task4 current: native schema2 all7 scenarios passed3 independent Debug Gallery processes (4 measured iterations,1 warmup,450ms each); current summary /tmp/atomui-animation-audit-native-all/summary.json. Added Core/Controls hashes and untracked source content to report identity. Clean selected initialization consumer tests261 passed; feedback/loop selected48 passed; Core6 passed. Native/Skia visual scripted checks, net8 production compilation, Gallery NativeAOT, final one fresh review and remaining benchmark/log cleanup still required. No active process/tool session at checkpoint.

Task4 finding: the existing offscreen native scene only allowed zero updates, it did not require them. All3 processes confirmed the target was geometrically outside its scroll viewport but still updated Progress about every16.67ms. This is valid native failure evidence for waste, not a reason to label all native performance passed. BorderBeam now tracks the actual EffectiveViewport rectangle and owns the attach/detach subscription; loops start only when paint bounds (including Outset) intersect it. Strengthened the existing specialized native assertion to forbid offscreen progress updates; no new permanent test case. Viewport field is actual geometry, not a patch flag.

Checkpoint: latest production edit is BorderBeam effective-viewport gating and stricter native offscreen assertion. Current active tool session27254 builds/runs3-process viewport validation. All prior audit temporary tests removed; benchmark fixture remains /tmp/atomui-animation-audit-bench. Source final review not dispatched yet. Remaining: viewport native green + visible resume; current finite Motion/geometry focused tests; Gallery tests; net8 compilation; scripted native/Skia visual verification (new evidence run required, prior evidence deleted); full Gallery NativeAOT for final source; one fresh readonly whole-diff reviewer; important fixes once; final cleanup/docs/tests, no commit.

Final review: /root/global_animation_final_review read-only fresh context, With fixes,2 Important,0 Critical; no minor findings. Popup late-surface registration identity and Drawer independent teardown both reproduced RED (latest opacity0.7 instead of protected0; drawer overlay still parented after reported restoration failure). Fixed both in one pass; temporary2 + existing Popup/Drawer focused43 passed. No second reviewer.

Final: Ruling: reviewer declined unconditional FPS/minimum hardware/display-present/cross-platform claims — retain explicit support boundary; cost no unconditional60/120FPS guarantee.
Final: Ruling: author-owned net8/Gallery/AOT/native/Skia/cleaned verification is completed by actual scoped commands, not review inference — cost any remaining pending gate must be reported unfinished.
Final: Ruling: local reference12.1.3 does not prove pinned12.1.4 behavior — execute fixtures and native tests against actual packages; cost source-only reasoning is not platform certification.
Final: Ruling: bit-identical spline samples intentionally superseded by framework-precision contract — preserve points/endpoints and measured0.0025371 bound; cost tiny intermediate differences.
Final: Ruling: stateful/side-effecting custom algorithms remain outside pure/immutable contract — no arbitrary deep cloning; cost extension authors must uphold this requirement.
Final: Ruling: composition and arbitrary independent application transforms remain unsupported capabilities — use reliable native path and static isolation; cost no GPU backend claim or automatic transform merging.
Final: Ruling: retain public non-Motion compatibility APIs as planned — cost small compatibility surface remains.
Final: Ruling: no evidence supports reducing ancestor Arrange invalidation — preserve native NavMenu fix; cost ancestor path traversal/arrange work remains.
Final: Ruling: preliminary OverlayDialog retention concern withdrawn after following DisposeAsync fallback — code stands, cost no new dedicated test for a disproven finding.
Final: Ruling: author checks links/compilation/current modules; full regression remains prohibited and binary compatibility not certified by exhaustive ApiCompat — cost those broader certifications are not claimed.

Visual AC recorded before cleanup: actual full WorkspaceWindow Skia scene1300x900, routed mouse input with hit checks, Layout/FlexPanel select and sidebar same-instance roundtrip twice; natural text/menu/footer geometry and selected path visible. Separate650x500 product Window: checked switch, third segmented item,60%progress and active spinner; input toggles switch, updates80%, reparent/remount retains state and first subsequent toggle works. Four actual compositor frames inspected PASS. Media run scripted-visual-acceptance-2nfd_fos, cleanup pending.

Final complete for user review: final review fixes merged into existing tests; current Popup/Drawer/Dialog156 passed, Gallery27 passed, production net8 rebuild0warnings/0errors, final Gallery Release/osx-arm64 NativeAOT output validation passed (Mach-O arm64). No AtomUI AOT diagnostics; ReactiveUI IL2104/IL3053 and Homebrew deployment-version linker warnings remain. Earlier stage Core6/state-consumer261/finite300/feedback48 results retained at their actual scopes, no duplicate test total claim. All audit tests/probes/media/benchmark/native JSON outputs removed; Skia cleanup removed5 files audit clean.86 changed Markdown files,0 missing relative targets. Fact/Theory methods382→378 reflects preexisting parameterization, zero new permanent cases. Source/AXAML/skills have no StateMotion or old finite-restart helper. Original release/6.0 checkout clean; feature/motion HEAD6c647fef1 unchanged, no commit/push/merge.

Final handoff check: formal documentation reviewed,86 changed Markdown files have0 missing local targets; geometry descriptions using the word arc are unrelated to the removed reference-project material. Source old-name checks clean. Dedicated recorded audit logs removed; published NativeAOT review artifact preserved. finishing-a-development-branch applied subject to explicit user/AGENTS requirements: scoped verification only, preserve current uncommitted feature/motion worktree for review, no merge/push/commit/menu needed.
