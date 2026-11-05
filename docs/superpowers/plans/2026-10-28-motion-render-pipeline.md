# Motion render pipeline implementation

Approved scope: refactor the animation module at its shared execution/render/layout boundaries; validate Debug Modal mask, NavMenu sidebar, nested three-level menus and reversal. Leave all changes uncommitted.

Evidence: `.artifacts/performance/motion-investigation-20261028/root-cause/report.md`. Full-window opacity repeatedly replays stable background drawing; compositor-only does not fix it. UI animation ticks outrun actual drawing. Opening layout briefly publishes natural occupancy before its lease exists.

## Constraints

Preserve duration, easing, shadows, text size, layout flow, input, bindings and cancellation. No reflection in product code. Temporary tests are deleted after RED/GREEN; final checks remain module-focused. Native draw callbacks are diagnostic evidence, not presentation timestamps or proof of 120 Hz. No commits.

## Tasks

1. Add shared, bounded render-cache leases with ref counting, latest-source restoration, detach cleanup and explicit one-time cache synchronization. Integrate stable sibling layers at Motion boundaries. Reproduce missing reuse before implementation; verify ownership and release.
2. Prepare expansion occupancy before making hidden content visible, transfer preparation into playback without a natural-size flash, and verify nested layout/width restoration with existing tests.
3. Introduce window-scoped drawing-paced execution for supported native rendering, retain framework clock fallback, and route NavMenu width through Motion. Coalesce samples and preserve interruption, final frame and disposal. Enable compositor tracks only where completion and presentation-value ownership can be met with supported APIs; record capability limits.
4. Rebuild current Debug Gallery and run the same native workloads, including first/warm runs, nested menus and reversal. Compare draw intervals, render time and property/layout activity. Inspect diagnostic render frames separately from timing. Update motion architecture evidence.
5. Remove temporary tests/probes, run focused final verification and declaration checks, obtain one independent final review, address material findings and report acceptance gaps explicitly. Do not commit.

## Review focus

Reentrancy, overlapping leases, bound CacheMode updates, hidden/minimized/detached hosts, nested animation and scrolling, rapid reversals, driver idle lifetime, cache memory limits and compositor capability fallback. No claimed display frame rate from callback counts.

## User review disposition

The user withdrew the performance changes after failed acceptance. Tasks concerning render caches, shared frame driving and NavMenu width migration are reverted. Only the independent visibility-preparation correctness fix remains. Do not resume the withdrawn performance direction without a new user instruction.
