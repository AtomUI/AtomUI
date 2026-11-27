# DataGrid Row Reorder Implementation Plan

> Execute the plan task by task. Every behavior change follows a failing-test-first cycle.

**Goal:** Replace DataGrid row reordering's shared static drag state and direct `IList` mutation with an instance-owned drag session and an optional CollectionView move capability.

**Architecture:** `DataGrid` owns the pointer-session state machine and all cleanup. `DataGridRowReorderHandle` forwards pointer input, `DataGridRowsPresenter` renders the ghost row, and `IDataGridCollectionViewMoveSupport` owns View-index validation and data movement. Built-in movement is transactional and enabled only for mutable flat views without active transforms or edit/defer state.

**Tech Stack:** .NET 10, C#, Avalonia pointer/input lifecycle, xUnit, Shouldly.

## Global Constraints

- Preserve existing public DataGrid properties, events, templates, tokens, and rendered appearance.
- The approved compatible API addition is `IDataGridCollectionViewMoveSupport` only; do not modify `IDataGridCollectionView`.
- Do not use static row-drag state, reflection-based capability discovery, delayed cleanup, forced dispatcher synchronization, or swallowed exceptions.
- Preserve existing dirty-worktree changes and do not create a commit unless requested.
- Keep the long-term design in the existing DataGrid overview, implementation, and module documents; this file is only the execution plan.

---

### Task 1: CollectionView Move Contract

**Files:**
- Create: `src/AtomUI.Desktop.Controls.DataGrid/Data/IDataGridCollectionViewMoveSupport.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Data/DataGridCollectionView.cs`
- Create: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Data/DataGridCollectionViewMoveTests.cs`

**Interfaces:**
- Produces: `bool CanMove { get; }`
- Produces: `bool TryMove(int sourceIndex, int targetIndex)` with View-relative indexes.

- [x] Add tests proving mutable flat lists are movable and that same-index/out-of-range requests do not mutate.
- [x] Run the focused tests and confirm they fail because the capability is absent.
- [x] Add the optional public interface and implement the minimal capability gate.
- [x] Add tests for arrays, read-only/fixed collections, sorting, filtering, grouping, paging, edit/add, and deferred refresh.
- [x] Implement transactional index-based movement, notification suppression, rollback, and a final View reset.
- [x] Add duplicate-equality, null-item, exception, and collection-reentrancy regression tests.
- [x] Run the focused tests until green.

### Task 2: Instance-Owned Drag Session

**Files:**
- Create: `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.RowReorder.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridRowReorderHandle.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Row/DataGridRowsPresenter.cs`
- Modify: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Interaction/DataGridRowReorderTests.cs`

**Interfaces:**
- Consumes: `IDataGridCollectionViewMoveSupport`.
- Produces: internal DataGrid methods for press, move, release, capture loss, row unload, and handle detach.

- [x] Replace reflection over static fields in tests with observable session isolation and ghost cleanup assertions.
- [x] Add failing tests for two DataGrids dragging independently and for drag-threshold event timing.
- [x] Add the DataGrid-owned session state machine and delegate handle input to it.
- [x] Make the presenter create its ghost from the session item instead of concrete `DataGridCollectionView` indexing.
- [x] Run the focused interaction tests until green.

### Task 3: Event Reentrancy and Commit Ordering

**Files:**
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.RowReorder.cs`
- Modify: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Interaction/DataGridRowReorderTests.cs`

- [x] Add failing tests for `RowReordering` cancellation and ItemsSource/CollectionView replacement inside the callback.
- [x] Revalidate pointer, handle, row, item, current View, and `CanMove` after `RowReordering`.
- [x] Add failing tests proving `RowReordered` fires only after a successful move and complete cleanup.
- [x] Commit through the session's captured View and suppress completion when the DataGrid switches View.
- [x] Run the focused interaction tests until green.

### Task 4: Unified Lifecycle Cancellation

**Files:**
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.RowReorder.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.Privates.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridRowReorderColumn.cs`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridRowReorderHandle.cs`
- Modify: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Interaction/DataGridRowReorderTests.cs`

- [x] Add failing tests for pointer capture loss, disabled/reorder-disabled state, source replacement, row recycle, reorder-column removal, template reapply, and detach.
- [x] Route every termination trigger through one idempotent DataGrid cleanup method.
- [x] Remove source-type throwing from the column; unsupported views remain visible but cannot enter Dragging.
- [x] Revalidate the session after auto-scroll so virtualized source-row recycling cannot continue the current move frame.
- [x] Run the focused interaction tests until green.

### Task 5: Boundary and Regression Verification

**Files:**
- Modify: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Interaction/DataGridRowReorderTests.cs`
- Modify: `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Data/DataGridCollectionViewMoveTests.cs`
- Review: DataGrid row-reorder design documents.

- [x] Add or complete top/bottom auto-scroll boundary and unsupported custom-view tests.
- [x] Run all DataGrid row-reorder and CollectionView move tests.
- [x] Run the full `AtomUI.Desktop.Controls.DataGrid.Tests` project.
- [x] Run `git diff --check` and review new artifacts, lifecycle pairs, API surface, and documentation consistency.
- [x] Report exact verification output and any residual manual visual checks without claiming unperformed validation.
