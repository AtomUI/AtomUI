# Native dialog bottom black strip

Date: 2026-09-17

## Scope
Fix the Windows native Dialog/MessageBox bottom black strip without changing public API, layout intent, colors, shadows, or overlay behavior. Preserve existing uncommitted Gallery and rendering-probe changes. No commit without review.

## Plan
1. Reproduce a native MessageBox and compare native client size, managed window size, compositor/layout bounds, and decoration geometry. Use the local Avalonia reference at D:/workspace/projects/AtomUI/.referenceprojects/Avalonia. Keep diagnostic harnesses under .artifacts.
2. Add a failing regression for the verified geometry/lifecycle defect in the existing Desktop Controls test project. Fix only the owning native-dialog/window sizing path. Do not mask the defect with a background fill or disable decorations/shadows.
3. Run the new regression red/green, affected tests and Gallery build, then visually recheck the native MessageBox including repeated opens. Run git diff --check. Report any pre-existing diagnostic failures separately.

## Candidate files
- src/AtomUI.Desktop.Controls/Dialog/WindowHost/DialogWindow.cs: requested native client size lifecycle.
- src/AtomUI.Desktop.Controls/Dialog/WindowHost/WindowDialogPresenter.cs: surface/chrome sizing orchestration (only if evidence requires).
- tests/AtomUI.Desktop.Controls.Tests/Dialog/WindowDialogPresenterTests.cs: geometry regression coverage.

## Completion
The same native dialog renders to its bottom edge, automated regression passes, and existing API/overlay behavior remains intact. Platform visual evidence is distinct from headless evidence.
