# DataGrid Immutable Query + Range Source Final Evidence

Captured on 2026-09-06 from branch `codex/datagrid-query-range-source` in the isolated
`datagrid-query-range-source` worktree. No commit was created.

## Implemented architecture

- `DataGridQuery` is the immutable owner of sort, filter and grouping intent.
- `ItemsSource : IDataGridSource?` is the DataGrid's only public data entry and exposes
  validated, snapshot-aware range requests. The runtime has no `Source` alias, legacy enumerable
  ItemsSource, collection-view or `DataGridDataConnection` execution path.
- A generation-scoped coordinator owns cancellation, at most two concurrent source calls,
  a 16-block LRU cache and explicit applied/edit/drag pin lifetimes.
- The existing row/cell visual virtualizer consumes only an immutable committed presentation
  index. Measure, arrange and recycle never fetch or synchronously wait for source data.
- Selection, current row, editing and mutations use stable row keys. Large selection is stored
  as immutable key sets/index intervals/all-matching expressions, not materialized items.
- Local sources use typed field delegates. Schema-driven column generation uses a compiled
  Avalonia property accessor and explicitly removes `INotifyPropertyChanged` subscriptions on
  disposal; it does not infer CLR member paths from protocol `FieldId` values.
- Paging, grouping, expansion, row-details heights, failure rollback and scroll anchors are
  generation/snapshot scoped. Stale work cannot mutate the committed presentation.

## Public data-entry contract

- DataGrid exposes `ItemsSourceProperty` as
  `DirectProperty<DataGrid, IDataGridSource?>` and `ItemsSource : IDataGridSource?` as its only
  public data-entry property.
- The transitional `Source`/`SourceProperty` names are absent. The retained `ItemsSource` name
  does not restore the legacy enumerable or CollectionView execution path.
- A reflection contract test fixes the property name and type. Existing source replacement,
  invalidation, detach/reattach, cancellation, virtualization and mutation tests were migrated
  without changing their behavioral assertions.
- This naming adjustment changed no DataGrid theme, template, row/cell geometry or visual-state
  implementation. The full DataGrid/Gallery suites and the NativeAOT Gallery runtime smoke were
  rerun after the adjustment; the frozen zero-pixel-diff frames below remain the render baseline.

## Ownership and release audit

| Acquired resource | Owner | Release path | Evidence |
| --- | --- | --- | --- |
| Source `Invalidated` subscription | attached DataGrid | source replace or visual detach | `DataGridSourceLifecycleTests` exact subscription counts and three GC cycles |
| Generation CTS | range coordinator | next generation, cancel/clear or dispose | cancellation and stale-result coordinator tests |
| Active viewport scope CTS | range coordinator | different normalized viewport, generation replacement or dispose | cross-Dispatcher thumb and caller/scope cancellation tests |
| Visible/prefetch block lease | active viewport scope | retain shared blocks first; then release on supersession or detach | overlapping-block and prefetch-priority coordinator tests |
| Block work-item CTS | range request work item | last lease release or generation cancellation; dispose after Source reaches a terminal state | cooperative/ignored-cancellation and tracked-request cleanup tests |
| Fetch concurrency lease | range coordinator | `finally` after every fetch | 10,000-cycle verifier: maximum concurrency 1, active work returns to zero |
| Cached-block pins | presentation/edit/drag owner | snapshot replacement, rollback, cancel, detach or dispose | cache pin tests and plateau verifier: 4 live applied pins |
| Local collection subscription | owned local source | `Dispose`; weak forwarder cannot retain source | `DataGridLocalSourceLifetimeTests` |
| Generated-column property subscription | binding property accessor | `Unsubscribe`/`Dispose` | exact `PropertyChanged` subscription-count test returns to zero |
| Pagination/template handlers | current template owner | template reapply or detach | pagination lifecycle and source lifecycle tests |
| Pointer capture/drag state | active reorder session | completion, cancel, invalidation, source replace, disable, column removal or detach | range mutation cancellation matrix |

## Functional verification

| Command/scope | Result |
| --- | --- |
| DataGrid Release/net10.0 suite | 270 passed, 0 failed, 0 skipped after viewport-supersession regression coverage |
| Gallery Release/net10.0 suite | 430 passed, 0 failed, 0 skipped |
| `AtomUI.slnx` Release after Release-configured restore | 672 passed, 0 failed, 0 skipped; production libraries built for net10.0 and net8.0 |
| Controls.Shared outside the solution | 157 passed |
| Generator outside the solution | 426 passed |
| Icons.Shared outside the solution | 1 passed |
| Desktop.Controls outside the solution, excluding one proven baseline failure | 2,871 passed |

The complete Desktop.Controls run has one unrelated `NavMenu` theme-string contract failure.
The same failure is present on the unmodified baseline. A separately observed Dialog popup
failure was flaky; its exact test passed on both the branch and baseline. Neither failure is in
the DataGrid package or was hidden by changing product code.

## Virtualization and bounded-state verification

The dedicated state verifier passed:

- local 1,000,000-row source: 64 returned entries, one projection, declared total 1,000,000;
- remote logical domain: 40 returned entries, 1,000-row active window, declared total
  10,000,000,000;
- 10,000 bidirectional viewport cycles: 15,464 requests, 989,696 generated entries, maximum
  request range 64, maximum concurrency 1, maximum realized rows 6, cache blocks 16, pins 4,
  sparse height entries 0 and stale commits 0.

## Render verification

A temporary Avalonia Headless/Skia harness rendered the frozen pre-refactor revision and this
branch with identical theme, window, grid, columns, source rows and layout. The harness was
removed after verification.

| Frame | Baseline/current structure | Baseline/current SHA-256 | Pixel difference |
| --- | --- | --- | ---: |
| 100-row initial viewport | 293 visual nodes, 6 realized rows, 24 cells, 3 columns, 800x420 | `d4f0ce10037bfc762760f8e247cf70ab4ec7384411181b820e1e44633bf337f5` | 0 |
| after 12 equal vertical scroll steps | 7 realized rows, 28 cells, 3 columns, 800x420 | `21bd027440dca15cb41222d303bc09e2653162867fc2c7d1e6971becd46f7419` | 0 |

The automated suite additionally covers scrollbar geometry, nested scroll chaining, row-details
extent/recycling, star and group-header layout, selected-row versus sorted-cell precedence,
loading/refresh/error geometry and popup/template lifecycles.

The user supplied a post-fix macOS Gallery screen recording and confirmed that rapid scrollbar
dragging is fixed. A broader manual desktop/browser screenshot sweep was not represented as
passed evidence; this does not affect the reproducible zero-pixel-diff headless frames above.

## Performance

The full measurements and independent samples are retained in
`docs/developments/evidence/tf-13-datagrid-performance-after.md`.

| Scenario | Baseline median ms | Final median ms | Baseline P95 ms | Final P95 ms | Median allocation delta |
| --- | ---: | ---: | ---: | ---: | ---: |
| Basic 8x4 | 24.395 | 13.875 | 38.496 | 19.743 | +2.27% |
| Virtualized 1000x8 | 16.832 | 15.880 | 29.804 | 21.684 | +2.76% |
| RowDetails 1000 | 9.512 | 9.131 | 14.383 | 14.443 | +4.25% |
| Gallery shape | 44.321 | 38.087 | 59.857 | 53.748 | +2.78% |

All timing metrics remain inside the frozen non-inferiority boundary. Allocation increases are
bounded per DataGrid instance and independent of source row count; the measured Ready phase is
38,570 bytes lower and the difference is concentrated in deterministic close-time cancellation,
pin, cache and subscription release.

Ten independent cold Gallery-shape samples improved from baseline mean/median/P95
493.379/466.240/646.966 ms to 419.971/416.216/461.405 ms. Median cold allocation increased
2.53%, consistent with the bounded lifecycle overhead above.

The rapid-thumb regression was measured separately with a cooperative fake-remote Source whose
fixed delay is 180 ms. Each of 20 samples dispatched eight distinct intermediate viewports across
separate UI dispatcher turns before timing the final target. The final target completed with mean
201.704 ms, median 195.920 ms and P95 231.633 ms; maximum Source concurrency was 2, 50 obsolete
requests were canceled and stale commits remained 0. The original screen recording showed about
2.6 seconds between the thumb settling and the final rows appearing, while the pre-fix deterministic
regression could not admit the final Source request within 250 ms when two obsolete requests held
the concurrency slots.

## AOT, trimming and final hygiene

- The full AOT/trimming/native/Browser-WASM verification matrix passed. Browser AOT compiled the
  DataGrid bitcode and produced no DataGrid IL2026 or IL3050 diagnostic.
- Final macOS arm64 NativeAOT Gallery publish passed output validation. The produced native
  executable started, remained healthy for the smoke interval with no console exception, and was
  then stopped. A process-table check confirmed that no Gallery verification process remained.
- Native publish emitted only existing ReactiveUI trimming/AOT summary warnings and local dylib
  deployment-target warnings.
- DataGrid legacy-consumer and unsafe-reflection scans contain no runtime legacy data path or CLR
  reflective field getter. Remaining scan hits are nested ordinary `ItemsControl.ItemsSource`,
  documentation text, existing transition scheduling, and Avalonia's delegate-backed
  `ClrPropertyInfo` abstraction.
- LLMS generation and verification passed for 79 controls and 161 source files.
- `git diff --check` passed and the root workspace remained clean.
