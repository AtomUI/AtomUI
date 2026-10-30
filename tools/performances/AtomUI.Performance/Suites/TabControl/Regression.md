# TabControl Performance Regression Record

The tab performance suite covers never-open creation/layout scenarios and warm repeated overflow-popup interaction for `TabControl`, `CardTabControl`, `TabStrip`, and `CardTabStrip`.

Use Release builds and compare distributions from at least ten separate processes on the same machine:

```bash
dotnet run --project tools/performances/AtomUI.TabOverflowPerformance/AtomUI.TabOverflowPerformance.csproj -c Release -- --count 500 --warmup 25
dotnet run --project tools/performances/AtomUI.TabOverflowPerformance/AtomUI.TabOverflowPerformance.csproj -c Release -- --creation-only --count 20
```

Acceptance gates:

- repeated open/close allocated bytes and Gen0 collections are at least 30% lower;
- repeated open/close elapsed time has no stable regression;
- never-open creation/layout/scroll scenarios have no stable regression greater than 5%;
- after first close, retained popup state is bounded to one empty context, one cached content root, and empty item containers capped by the latest nonempty snapshot while attached;
- retemplate, template replacement, and detach release the cached root, context session, owner, item snapshots, and subscriptions.

The focused project avoids unrelated stale suites in the broad performance runner. Dated measurements and environment details are recorded in `docs/superpowers/progress/2026-09-15-tab-overflow-review.md`.

## Measurement validity

The September 14 result is superseded: that host swallowed popup creation failures and did not prove that the menu opened. Its percentage claims must not be used as acceptance evidence.

The replacement host requires an actual overlay popup with realized, nonzero menu-item bounds. Failed opens terminate the probe. Both versions use the same host, including completed layout and explicit compositor frames at operation boundaries, so pending rendering does not cross the collection or measurement boundary. The dated review record contains the resulting raw samples and calculations.
