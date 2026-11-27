# DataGrid control performance

Iterations: 60; warmup: 5; operation: create + first layout + close.

| Scenario | Mean ms | Median ms | P95 ms | Mean UI-thread bytes | Median UI-thread bytes | P95 UI-thread bytes | Mean process bytes | Median process bytes | P95 process bytes | Mean create bytes | Mean realize + close bytes | Mean ready bytes | Mean close bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DataGrid.Basic.8x4 | 14.457 | 13.875 | 19.743 | 3165275 | 3232704 | 3234144 | 3165330 | 3232704 | 3234144 | 43727 | 3121603 | 2958599 | 163004 |
| DataGrid.Virtualized.1000x8 | 16.308 | 15.880 | 21.684 | 4638364 | 4703056 | 4704648 | 4638530 | 4703056 | 4706408 | 135993 | 4502537 | 4273447 | 229090 |
| DataGrid.RowDetails.1000 | 10.038 | 9.131 | 14.443 | 2990866 | 3062272 | 3065264 | 2990922 | 3062272 | 3065264 | 90404 | 2900517 | 2734088 | 166429 |
| DataGrid.GalleryShape | 38.703 | 38.087 | 53.748 | 10988145 | 10993440 | 10999232 | 10989139 | 10994160 | 11002544 | 214125 | 10775014 | 10033682 | 741332 |

## Frozen-baseline comparison

The frozen pre-refactor revision is `e8f97e38c0a33c48d8e525c4dea34c3bab456d8c` and its complete environment,
commands and raw numbers are retained in `2026-09-05-datagrid-query-range-source/baseline.md`. The comparable allocation
column below is the UI-thread median because that is what the original harness recorded.

| Scenario | Baseline median ms | Current median ms | Baseline P95 ms | Current P95 ms | Baseline median bytes | Current median UI-thread bytes | Allocation delta |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DataGrid.Basic.8x4 | 24.395 | 13.875 | 38.496 | 19.743 | 3,160,816 | 3,232,704 | +2.27% |
| DataGrid.Virtualized.1000x8 | 16.832 | 15.880 | 29.804 | 21.684 | 4,576,808 | 4,703,056 | +2.76% |
| DataGrid.RowDetails.1000 | 9.512 | 9.131 | 14.383 | 14.443 | 2,937,560 | 3,062,272 | +4.25% |
| DataGrid.GalleryShape | 44.321 | 38.087 | 59.857 | 53.748 | 10,695,768 | 10,993,440 | +2.78% |

All repeated median and P95 timings remain inside the frozen non-inferiority boundary. The small total-allocation increase is
bounded per DataGrid instance and independent of row count. A paired Basic diagnostic split measured the baseline Ready phase
at 2,997,169 bytes and close at 105,786 bytes; the final implementation measures Ready at 2,958,599 bytes and close at
163,004 bytes. Thus the steady Ready path is 38,570 bytes lower, while the net increase is concentrated in deterministic
coordinator cancellation, cache/pin release and subscription/template detach after the window closes.

## Independent cold process samples

Each sample starts a fresh process with `--count 1 --warmup 0 --scenario DataGrid.GalleryShape`. Process launch itself is
outside the measurement; Avalonia and AtomUI initialization within the process is cold.

| Sample | Time ms | Process allocated bytes |
| ---: | ---: | ---: |
| 1 | 461.405 | 12,418,784 |
| 2 | 418.469 | 12,418,784 |
| 3 | 409.751 | 12,418,784 |
| 4 | 414.493 | 12,418,784 |
| 5 | 415.987 | 12,417,152 |
| 6 | 416.444 | 12,418,784 |
| 7 | 428.938 | 12,418,784 |
| 8 | 406.555 | 12,431,144 |
| 9 | 417.118 | 12,418,784 |
| 10 | 410.549 | 12,431,144 |

| Statistic | Time ms | Process allocated bytes |
| --- | ---: | ---: |
| Mean | 419.971 | 12,421,093 |
| Median | 416.216 | 12,418,784 |
| P95 (nearest-rank) | 461.405 | 12,431,144 |

Against the frozen cold baseline, mean time improves from 493.379 ms to 419.971 ms, median from 466.240 ms to 416.216 ms,
and P95 from 646.966 ms to 461.405 ms. Median allocation increases by 2.53%, matching the bounded per-instance lifecycle
overhead seen in repeated runs rather than growth with source size.

## Million-row and plateau verifier

The dedicated `--verify-states` run passed with:

- local million-row fetch: 64 returned entries, one projection, total 1,000,000;
- long remote domain: 40 returned entries, 1,000-row active window, total 10,000,000,000;
- 10,000 bidirectional cycles: 15,464 requests, maximum range 64, maximum concurrency 1, maximum realized rows 6,
  cache 16, pins 4, measured-height entries 0 and stale commits 0.

## Rapid viewport supersession

The state verifier now includes 20 asynchronous rapid-thumb samples. Every sample uses a cooperative Source with a fixed
180 ms delay, dispatches eight distinct intermediate viewports across separate UI dispatcher turns, and then times the final
viewport until its rows are committed.

| Evidence | Before fix | After fix |
| --- | ---: | ---: |
| Final Source admission with two obsolete active requests | Not admitted within 250 ms | Admitted without manually completing obsolete requests |
| Recorded thumb-settle to final rows | About 2,600 ms | — |
| 20-sample final viewport mean | — | 201.704 ms |
| 20-sample final viewport median | — | 195.920 ms |
| 20-sample final viewport P95 | — | 231.633 ms |
| Maximum Source concurrency | 2 | 2 |
| Obsolete requests canceled | 0 in the failing coordinator reproduction | 50 |
| Stale commits | 0 | 0 |

The post-fix result is bounded by one current 180 ms visible fetch plus cancellation and UI commit overhead; it no longer
grows with the number of intermediate thumb intents. The same run repeated the 10,000-cycle plateau verifier with unchanged
bounded-state results.
