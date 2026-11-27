# Tab Overflow Popup Performance Progress

> **Superseded measurement — invalid interaction evidence.** The 2026-09-15 source review found that this probe caught
> `Unable to create IPopupImpl and no overlay layer is found` and continued measuring. The repeated-open numbers below
> therefore describe a failed popup-open path, not realized popup interaction. Their speedup percentages, acceptance
> conclusions and visual-retention claims must not be used. Later shadow smoke comparisons using the same host share this
> limitation. The original raw samples remain here for audit; corrected host validation and fresh before/after measurements
> are recorded in [the 2026-09-15 review](2026-09-15-tab-overflow-review.md).

## Qualification

- **Scenario frequency:** Gallery pages and production navigation surfaces can contain many tab owners; users can reopen the overflow surface repeatedly during one attached lifetime.
- **Numeric-report requirement:** explicit. The approved design requires a material reduction, defined as at least 30% lower repeated-open/close allocations and Gen0 collections, without a stable elapsed-time regression.
- **Correctness constraints:** identical overflow geometry, placement, activation, close, pinned-open, keyboard, and light-dismiss behavior; no owner/item/template retention after lifecycle teardown.

## Baseline model

- **Popup subsystem:** the current implementation constructs a new `MenuFlyout` for every open.
- **Binding subsystem:** every open constructs two relay bindings and one `CompositeDisposable`.
- **Event subsystem:** every overflow item gets one click and one close delegate, removed only during flyout cleanup.
- **Dispatcher subsystem:** activation allocates a posted callback instead of completing synchronously through the popup context.

The repeated interaction probe exercises the real more-button click path and the control lifecycle close path after a fixed warmup. It reports elapsed time, current-thread allocated bytes, Gen0 collections, and stable tree shape for all four public owners. The existing `tabcontrol` suite remains the never-open creation/layout guard.

## Falsification hypothesis

The architecture is rejected or redesigned if the post-change ten-process distribution does not show at least 30% lower allocation and Gen0 cost, if elapsed time regresses stably, if never-open scenarios regress by more than 5%, or if retained-state tests show growth beyond the one allowed empty context plus cached content root while attached.

## Environment

- Worktree: `.worktrees/tab-overflow-popup-search`
- Branch: `codex/tab-overflow-popup-search`
- Configuration: Release
- Runtime/project: `tools/performances/AtomUI.TabOverflowPerformance/AtomUI.TabOverflowPerformance.csproj`
- Interaction command: `dotnet run --project tools/performances/AtomUI.TabOverflowPerformance/AtomUI.TabOverflowPerformance.csproj -c Release -- --count 500 --warmup 25`
- Never-open command: `dotnet run --project tools/performances/AtomUI.TabOverflowPerformance/AtomUI.TabOverflowPerformance.csproj -c Release -- --creation-only --count 20`

## Measurements

### Baseline: repeated open/close

Ten independent Release processes, 25 warmup cycles and 100 measured cycles per owner. Values below are microseconds/cycle; allocation and Gen0 values were identical across the ten samples for each owner.

| Owner | 10 elapsed samples (us/cycle) | Median us/cycle | Bytes/cycle | Gen0/100 cycles | Visual warm/end |
| --- | --- | ---: | ---: | ---: | ---: |
| TabControl.Line | 3357.51, 3404.44, 3574.07, 3417.91, 3566.23, 3279.69, 3092.41, 3451.22, 3313.83, 3263.71 | 3380.98 | 432957.7 | 7 | 224/224 |
| TabControl.Card | 2086.94, 2136.14, 2294.42, 2393.47, 2021.34, 1959.47, 1808.23, 1819.74, 2152.25, 1969.96 | 2054.14 | 438239.0 | 6 | 272/272 |
| TabStrip.Line | 1222.94, 1291.67, 1201.69, 1873.52, 1134.41, 1421.17, 1477.45, 1391.95, 1451.13, 1397.14 | 1394.55 | 432527.6 | 6 | 221/221 |
| TabStrip.Card | 1236.59, 1483.63, 1254.64, 1281.36, 1248.14, 1437.16, 1413.25, 1389.01, 1095.85, 1561.22 | 1335.19 | 432527.6 | 6 | 221/221 |

### Baseline: never-open creation/layout

Ten independent Release processes, one unmeasured warmup owner and 10 measured overflowing owners per scenario. Values below are milliseconds/item; allocation was stable apart from sub-kilobyte noise.

| Owner | 10 elapsed samples (ms/item) | Median ms/item | Median KB/item | Visuals/10 owners |
| --- | --- | ---: | ---: | ---: |
| TabControl.Line | 38.201, 33.351, 31.322, 30.624, 27.656, 32.674, 31.594, 29.571, 29.059, 31.853 | 31.458 | 3514.0 | 2250 |
| TabControl.Card | 36.406, 43.128, 42.316, 33.015, 36.814, 31.553, 43.688, 34.963, 36.416, 37.956 | 36.615 | 4237.0 | 2730 |
| TabStrip.Line | 20.149, 21.667, 17.447, 19.847, 17.970, 18.270, 29.155, 19.513, 18.349, 17.733 | 18.931 | 3444.4 | 2220 |
| TabStrip.Card | 29.292, 29.754, 27.093, 26.906, 25.756, 28.644, 28.150, 27.947, 26.386, 26.573 | 27.520 | 3793.5 | 2220 |

Post-change raw samples are appended here before performance acceptance is evaluated.

### Post-change: repeated open/close

Ten independent Release processes, 25 warmup cycles and 100 measured cycles per owner. Allocation and Gen0 values were identical
across all ten samples for each owner.

| Owner | 10 elapsed samples (us/cycle) | Median us/cycle | Bytes/cycle | Gen0/100 cycles | Visual warm/end |
| --- | --- | ---: | ---: | ---: | ---: |
| TabControl.Line | 211.37, 203.00, 211.78, 204.09, 208.01, 205.72, 204.48, 201.44, 200.12, 205.16 | 204.82 | 30016.4 | 0 | 225/225 |
| TabControl.Card | 231.23, 245.49, 246.89, 249.20, 245.06, 242.76, 235.16, 238.31, 254.76, 242.57 | 243.91 | 35392.4 | 0 | 273/273 |
| TabStrip.Line | 194.42, 189.24, 198.11, 185.76, 196.86, 190.93, 188.14, 192.08, 188.76, 209.23 | 191.51 | 29680.4 | 0 | 222/222 |
| TabStrip.Card | 190.13, 191.14, 194.56, 193.69, 191.85, 195.57, 191.81, 191.51, 194.64, 187.28 | 191.83 | 29680.4 | 0 | 222/222 |

### Post-change: never-open creation/layout

Ten independent Release processes, one unmeasured warmup owner and 10 measured overflowing owners per scenario.

| Owner | 10 elapsed samples (ms/item) | Median ms/item | Median KB/item | Visuals/10 owners |
| --- | --- | ---: | ---: | ---: |
| TabControl.Line | 31.360, 29.832, 34.475, 43.492, 33.206, 29.005, 28.234, 28.943, 28.314, 29.899 | 29.866 | 3527.7 | 2260 |
| TabControl.Card | 25.332, 34.937, 25.650, 25.641, 25.375, 24.821, 34.185, 35.364, 31.346, 35.174 | 28.498 | 4250.6 | 2740 |
| TabStrip.Line | 12.881, 17.214, 16.781, 14.685, 16.984, 14.693, 22.686, 16.358, 21.202, 18.248 | 16.883 | 3458.5 | 2230 |
| TabStrip.Card | 15.775, 16.212, 17.449, 16.308, 16.008, 16.422, 17.684, 15.734, 22.312, 18.059 | 16.365 | 3794.5 | 2230 |

### Acceptance result

| Owner | Repeated time change | Allocation change | Gen0 change | Never-open time change | Never-open allocation change |
| --- | ---: | ---: | ---: | ---: | ---: |
| TabControl.Line | -93.94% | -93.07% | 7 -> 0 | -5.06% | +0.39% |
| TabControl.Card | -88.13% | -91.92% | 6 -> 0 | -22.17% | +0.32% |
| TabStrip.Line | -86.27% | -93.14% | 6 -> 0 | -10.82% | +0.41% |
| TabStrip.Card | -85.63% | -93.14% | 6 -> 0 | -40.53% | +0.03% |

The repeated-interaction allocation target is exceeded for all four owners, elapsed time improves materially, and Gen0 collections fall
to zero for the 100-cycle probe. Never-open allocations remain within +0.41%, well inside the 5% guard, while all four median elapsed
times improve. Warm/end visual counts are identical in every repeated-interaction sample, so the measured lifecycle does not accumulate
popup visuals. The one-visual-per-owner increase in the never-open tree is the intentionally static empty Popup shell described by the
approved architecture; its allocation effect remains below 0.5%.

The broad `AtomUI.Performance` project was not used as the interaction host because its current baseline contains unrelated stale Calendar, Steps, and Card APIs and does not compile. The dedicated project keeps this change scoped while using the same AtomUI runtime and headless realization path.

### Post-feedback verification

After removing automatic popup focus and making the Gallery examples responsive, a fresh 500-cycle Release smoke retained identical
warm/end visual counts for every owner: `225/225`, `273/273`, `222/222`, and `222/222`. Allocations remained at 29,680.1–35,392.1
bytes/cycle; the focus correction adds no open-time allocation path. A fresh 20-owner never-open run remained at 3,457.4–4,250.4
KB/item with the expected fixed visual counts. Separate weak-reference regressions prove that an opened owner, shared viewer, context,
and custom popup root are all collectible after detach.

After the Popup surface-radius correction, another fresh 500-cycle Release smoke measured `339.30`, `247.80`, `198.64`, and
`199.90` us/cycle with unchanged allocations of `30,016.1`, `35,392.1`, `29,680.1`, and `29,680.1` bytes/cycle. Gen0 was `2`
for each 500-cycle batch and every warm/end visual count remained identical (`225/225`, `273/273`, `222/222`, `222/222`). The
20-owner never-open probe measured `29.921`, `20.507`, `12.884`, and `13.065` ms/item with `3,531.1`, `4,250.4`, `3,457.4`,
and `3,808.1` KB/item and unchanged visual counts. The final-surface presenter-chain tracking therefore adds no never-open visual,
no repeated-cycle allocation, and no visual accumulation. A dedicated regression additionally proves that replacing a nested popup
surface releases the old corner-radius binding and allows the old surface to be garbage-collected while the host remains alive.

After replacing the edge gradients with four directional inset shadows and nesting the static indicator Borders inside the real
`ScrollContentViewport`, a fresh 500-cycle Release smoke measured `211.13`, `243.57`, `74.65`, and `74.54` us/cycle. Allocations
were `30,128.1`, `35,504.1`, `29,792.1`, and `29,792.1` bytes/cycle: a fixed 112-byte/cycle increase over the previous visual
correction smoke, while remaining 91.90%–93.11% below the original implementation. Gen0 remained `2` for each 500-cycle batch,
and every warm/end visual count was identical (`226/226`, `274/274`, `223/223`, `223/223`), proving no per-cycle visual
accumulation. The intentional one-Panel template change accounts for one additional stable visual per attached owner.

The matching 20-owner never-open smoke measured `30.082`, `23.680`, `12.617`, and `14.535` ms/item with `3,535.1`, `4,254.4`,
`3,461.4`, and `3,812.1` KB/item. Relative to the immediately preceding smoke, allocation increased only 0.09%–0.12%, well
inside the 5% guard, while the visual count increased by exactly one static viewport Panel per owner. The C# measurement override,
two per-instance `LinearGradientBrush` objects and four gradient stops have been removed; edge scrolling now changes only the
visibility of template-owned Borders whose `BoxShadows` come from family Token resources.
