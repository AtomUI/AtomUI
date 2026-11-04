# Popup In Dialog Global Regression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Preserve the Issue #441 reproduction as a permanent Desktop Controls TestApp and prove that every AtomUI popup creation path keeps the owning Window `TopLevel` when used inside an Overlay Dialog.

**Architecture:** The production fix is owned by the presentation boundary: Dialog stays in the owning Window `OverlayLayer`, popup-bearing content stays in the same `TopLevel`, and Avalonia continues to own popup hosting, positioning, light-dismiss, focus and input. Verification is split into a source inventory guard, shared popup primitive tests, public-control family tests, and a real desktop TestApp; no control receives a Dialog-specific workaround.

**Tech Stack:** .NET 10, Avalonia Headless/XUnit, AtomUI Desktop Controls, AXAML, Shouldly.

**Spec:** `docs/controls/desktop/feedback/modal/popup-layering-design.md`

## Global Constraints

- `WindowDrawnDecorations` may draw chrome only; it must not host Dialog, Drawer or popup-bearing business content.
- Dialog content, popup placement target and popup service must resolve to the same owning Window `TopLevel`.
- Popup host creation, positioning, focus, light-dismiss and disposal remain Avalonia primitive responsibilities.
- Do not add per-control Dialog detection, delays, retries, forced overlay mode, copied popup hosts or nested `VisualLayerManager` instances.
- Public APIs, default values, template part names, theme keys and tokens remain unchanged.
- Windows and macOS have current manual evidence; Linux X11/Wayland is explicitly untested.
- Do not create commits unless the user requests them.

## File Structure

- `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupTestHost.cs`: reusable attached Window/Dialog fixture, dispatcher pump and popup-host assertions.
- `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogContentPopupLayeringTests.cs`: shared ownership, ComboBox interaction and lifecycle regression.
- `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupPrimitiveLayeringTests.cs`: direct Popup, Flyout/MenuFlyout, ToolTip and ContextMenu behavior matrix.
- `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupControlFamilyTests.cs`: parameterized public-control family integration matrix.
- `tests/AtomUI.Desktop.Controls.Tests/Dialog/PopupEntryInventoryTests.cs`: repository-source allowlist guard for AXAML and C# popup entry points.
- `tests/AtomUI.Desktop.Controls.TestApp/*`: permanent manual regression executable and `Scenarios/PopupInDialog` UI.
- `AtomUI.slnx`: builds the TestApp with the repository.
- Modal, Drawer, Window and visual-layer design documents: final ownership model, TestApp routing and platform evidence.

---

### Task 1: Lock the Popup Source Inventory

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Dialog/PopupEntryInventoryTests.cs`
- Modify: `docs/controls/desktop/feedback/modal/popup-layering-design.md`

**Interfaces:**
- Consumes: runtime source under `src/AtomUI.Desktop.Controls*`.
- Produces: `PopupEntryInventoryTests.Popup_Entry_Inventory_Matches_The_Audited_Allowlist()` and an explicit sorted allowlist grouped by template Popup, constructed Popup, flyout primitive and direct `ShowAt` consumer.

- [ ] **Step 1: Write the repository scan test**

  Scan `.axaml` files for `<atom:Popup`, and `.cs` files for `new Popup`, public `PopupFlyoutBase` derivatives and direct `.ShowAt(` calls. Normalize paths to `/`, compare sorted results to exact allowlists, and print added/removed paths on failure.

- [ ] **Step 2: Run the inventory test and inspect the first failure**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~PopupEntryInventoryTests`

  Expected before the allowlist is reconciled: FAIL listing every currently unclassified popup entry.

- [ ] **Step 3: Reconcile the allowlist with the audited source paths**

  Classify each result into the paths defined by section 10.1 of the spec. Do not suppress a file without naming its public control or consumer in the test data.

- [ ] **Step 4: Run the inventory test again**

  Expected: PASS, with future popup entry additions producing a focused allowlist diff.

### Task 2: Extract the Shared Attached Dialog Fixture

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupTestHost.cs`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogContentPopupLayeringTests.cs`

**Interfaces:**
- Produces: `DialogPopupTestHost.Open(Control content)`, `FindPopupHost(Control logicalOwner)`, `Click(Control target)`, `LightDismiss()`, and deterministic `Dispose()` cleanup.
- Consumes: `ScopeAwareOverlayLayerPanel`, `OverlayDialogPresenter`, `DrawnDecorationsTestHost`, dispatcher and forced render ticks.

- [ ] **Step 1: Write fixture characterization assertions**

  Assert after `Open` that content is attached, `TopLevel.GetTopLevel(content)` is the fixture Window, presenter parent is `DialogOverlayLayer`, and that layer is parented by Avalonia `OverlayLayer`.

- [ ] **Step 2: Run the existing Dialog popup tests**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DialogContentPopupLayeringTests`

  Expected: PASS before refactoring.

- [ ] **Step 3: Move duplicated setup and cleanup into `DialogPopupTestHost`**

  Keep assertions on actual `OverlayPopupHost` instances; never replace them with requested open-state assertions.

- [ ] **Step 4: Re-run the Dialog popup tests**

  Expected: PASS with unchanged public behavior and deterministic presenter/layer release.

### Task 3: Cover Every Shared Popup Primitive

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupPrimitiveLayeringTests.cs`
- Reuse: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupTestHost.cs`

**Interfaces:**
- Consumes: direct AtomUI `Popup`, `Flyout`, `MenuFlyout`, `ToolTip`, and `ContextMenu` public open/close APIs.
- Produces: one direct Dialog integration test per primitive path.

- [ ] **Step 1: Write failing primitive cases one at a time**

  Each case attaches its anchor in Dialog content, opens by the public API, asserts a real host above the Dialog presentation, performs one public action or light-dismiss, then closes/detaches and asserts no stale host/open state.

- [ ] **Step 2: Run each new primitive case immediately**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DialogPopupPrimitiveLayeringTests`

  Expected: cases supported by the shared root fix PASS; any failure identifies the failing primitive owner, not a consumer control.

- [ ] **Step 3: Fix only a proven primitive defect, if present**

  Make the smallest root-owner change in the primitive host/anchor/lifecycle implementation. Do not add Dialog checks to popup consumers.

- [ ] **Step 4: Re-run primitive and existing Dialog tests**

  Expected: PASS for all primitive paths and existing ComboBox behavior.

### Task 4: Cover the Public Control Families

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupControlFamilyTests.cs`
- Reuse: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupTestHost.cs`

**Interfaces:**
- Consumes: public open state or pointer interaction for selector, autosuggest, picker, color, menu/navigation, flyout-consumer and Tour controls.
- Produces: named theory data so every control in spec section 10.3 is visible in test output.

- [ ] **Step 1: Add family data and a host-establishment assertion**

  Include ComboBox, Select, Cascader, TreeSelect, AutoComplete, AutoCompleteSearchEdit, AutoCompleteTextArea, Mentions, DatePicker, TimePicker, RangeDatePicker, RangeTimePicker, ColorPicker, GradientColorPicker, Menu/MenuItem, NavMenu, DropdownButton, SplitButton, PopupConfirm, AvatarGroup fold, Transfer dropdown, TabControl overflow, DataGrid filter and Tour.

- [ ] **Step 2: Run the matrix and record independent failures**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DialogPopupControlFamilyTests`

  Expected: shared-template families pass through the common `TopLevel`; failures must name the exact control and missing public behavior.

- [ ] **Step 3: Resolve only independently proven family defects**

  If a family has a distinct placement target or lifecycle owner, fix its shared family base/template and re-run every member. If the matrix is green, do not modify consumer controls merely to create code churn.

- [ ] **Step 4: Run the complete Dialog test slice**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog`

  Expected: PASS.

### Task 5: Migrate the Reproduction into the Permanent TestApp

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/Program.cs`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/TestApplication.axaml`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/TestApplication.axaml.cs`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/MainWindow.axaml`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/MainWindow.axaml.cs`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/Scenarios/PopupInDialog/PopupInDialogScenario.axaml`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/Scenarios/PopupInDialog/PopupInDialogScenario.axaml.cs`
- Create: `tests/AtomUI.Desktop.Controls.TestApp/README.md`
- Modify: `AtomUI.slnx`
- Preserve the local ignored reproduction during migration; the version-controlled source of truth is `tests/AtomUI.Desktop.Controls.TestApp/*`.

**Interfaces:**
- Produces: a `WinExe`, `IsPackable=false` manual regression app using source project references and a discoverable `PopupInDialogScenario`.
- Consumes: only public AtomUI/Avalonia APIs for status; no private reflection, temp logs, auto-open or swallowed exception paths.

- [ ] **Step 1: Create the TestApp shell and preserve the accepted ComboBox interaction**

  Keep the page ComboBox, user-opened Overlay Dialog, Dialog ComboBox, selection status and code-open button. Replace internal host dumps with public `TopLevel.GetTopLevel` ownership and open/selection/action status.

- [ ] **Step 2: Add manual family groups**

  Provide user-triggered Dialog scenarios for selectors, autosuggest, pickers, ColorPicker, Flyout/MenuFlyout, ToolTip/ContextMenu and Tour. Each group has visible expected-result text and no automatic popup opening.

- [ ] **Step 3: Add the project to `AtomUI.slnx` and make it the canonical regression app**

  The preserved scenario must live under `tests/AtomUI.Desktop.Controls.TestApp`; the legacy ignored copy is not deleted by this task. The project references Desktop Controls, ColorPicker and DataGrid source projects plus the AtomUI generator analyzer.

- [ ] **Step 4: Build the permanent app**

  Run: `dotnet build tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0 --no-restore`

  Expected: PASS.

- [ ] **Step 5: Run the app for Windows manual acceptance**

  Run the built executable and manually click every group. Expected: popups are visible, interactive, dismissible, Dialog remains alive, and the process does not crash.

### Task 6: Synchronize Design Documents and Platform Evidence

**Files:**
- Modify: `docs/controls/desktop/feedback/modal/{overview.md,implementation.md,changelog.md,popup-layering-design.md}`
- Modify: `docs/controls/desktop/feedback/drawer/{overview.md,implementation.md,changelog.md}`
- Modify: `docs/controls/desktop/window/window/{overview.md,implementation.md,changelog.md}`
- Modify: `docs/architecture/systems/rendering/visual-layers.md`

**Interfaces:**
- Produces: one consistent statement of presentation ownership, chrome-only decorations, reference-counted suppression leases, permanent manual validation routing and platform evidence.

- [ ] **Step 1: Remove stale business-host wording**

  Search for `Dialog/Drawer host`, `drawn host`, removed template part names and statements that imply business presentation is hosted by Window decorations.

- [ ] **Step 2: Add the final shared invariant and validation routing**

  Modal owns the global popup inventory/matrix; Drawer and Window link to that canonical design instead of duplicating the consumer list; the global visual-layer standard names the TestApp scenario.

- [ ] **Step 3: Record exact platform evidence**

  Every corresponding current design page states: Windows tested, macOS tested, Linux X11/Wayland not tested. Historical changelog wording must not imply Linux was executed.

- [ ] **Step 4: Validate documentation consistency**

  Run: `rg -n "Dialog/Drawer host|drawn host|临时 demo|临时实机验收|Windows/Linux/macOS|Windows、Linux、macOS" docs/controls/desktop/feedback/modal docs/controls/desktop/feedback/drawer docs/controls/desktop/window/window docs/architecture/systems/rendering`

  Expected: no stale current-design claim; only intentionally quoted historical identifiers may remain.

### Task 7: Full Verification

**Files:**
- Verify all files changed by Tasks 1-6.

**Interfaces:**
- Produces: fresh build/test/diff evidence suitable for review.

- [ ] **Step 1: Run the Desktop Controls suite**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore`

  Expected: all tests PASS.

- [ ] **Step 2: Run optional-package tests touched by the matrix**

  Run: `dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore`

  Expected: all tests PASS.

- [ ] **Step 3: Rebuild the TestApp and validate the patch**

  Run: `dotnet build tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0 --no-restore`

  Run: `git diff --check`

  Expected: both PASS with no whitespace errors.

- [ ] **Step 4: Review final scope**

  Inspect `git status --short` and `git diff --stat`. Confirm there are no per-control workaround changes, no private TestApp reflection/logging, no deleted reproduction without a permanent replacement, and no claim that Linux was tested.
