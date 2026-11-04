# Popup Default Surface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make direct `AtomUI.Desktop.Controls.Popup` render a theme-aware elevated surface from the shared popup frame while preserving every existing Popup-bearing control's current visuals.

**Architecture:** Add public `Popup.SurfaceBackground`, resolve its themed default in `PopupTheme`, and relay it to the existing `ShadowsAwareContainer` frame renderer used by both native and overlay hosts. All existing framework-owned Flyout, Tooltip, ContextMenu, AXAML Popup presenters explicitly set the value to `null`, keeping their pre-existing content-owned surfaces and the renderer's prior transparent fill.

**Tech Stack:** C# 14, .NET 10, Avalonia 12, AtomUI ControlTheme and Shared Tokens, xUnit/Shouldly/Avalonia.Headless.

**Spec:** `docs/superpowers/specs/2026-08-21-popup-default-surface-design.md`

## Global Constraints

- Do not modify `PopupRoot.Background`; it remains `null` for transparent native-window composition.
- Do not add Popup default Padding, border, arrow, wrapper, placement offset, or geometry.
- Every existing AtomUI Popup-bearing control must opt out with `SurfaceBackground=null`; only direct Popup changes visually.
- Do not infer ownership from Child type, Child Background, opacity, platform, Dialog ancestry, or runtime timing.
- Native `PopupRoot` and `OverlayPopupHost` must use the same `ShadowsAwareContainer` implementation.
- Relay bindings acquired on logical attach must be released on reattach and detach.
- No reflection or dynamic registration in production; preserve NativeAOT compatibility.
- Platform evidence remains Windows/macOS tested and Linux X11/Wayland untested until real Linux runs are recorded.

---

### Task 1: Add the shared default surface contract with TDD

**Files:**
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Popup/PopupShadowTests.cs`
- Modify: `src/AtomUI.Desktop.Controls/Popup/Popup.cs`
- Modify: `src/AtomUI.Desktop.Controls/Popup/Themes/PopupTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/ShadowsAwareContainer.cs`

**Interfaces:**
- Produces: `Popup.SurfaceBackgroundProperty : StyledProperty<IBrush?>`
- Produces: `Popup.SurfaceBackground : IBrush?`
- Produces: `ShadowsAwareContainer.SurfaceBackgroundProperty` and internal CLR wrapper.
- Produces: shared frame rendering for native and overlay hosts without layout changes.

- [ ] **Step 1: Replace the obsolete no-surface assertion with failing default/content-owned surface tests**

Add tests that express the new behavior before production edits:

```csharp
[Fact]
public void Popup_Exposes_SurfaceBackground_As_A_Public_Styled_Property()
{
    Popup.SurfaceBackgroundProperty.OwnerType.ShouldBe(typeof(Popup));
    typeof(Popup).GetProperty(nameof(Popup.SurfaceBackground)).ShouldNotBeNull();
}

[Fact]
public void ShadowsAwareContainer_Frame_Renderer_Draws_Configured_Surface()
{
    var container = new ShadowsAwareContainer
    {
        BoxShadow        = BoxShadows.Parse("0 8 24 0 #66000000"),
        SurfaceBackground = Brushes.White,
        Child             = new Border()
    };

    var frameRenderer = GetFrameRenderer(container);
    frameRenderer.ShouldNotBeNull();
    frameRenderer!.GetType().GetProperty("SurfaceBackground")
                 .ShouldNotBeNull()!.GetValue(frameRenderer)
                 .ShouldBe(Brushes.White);
}

[Fact]
public void ShadowsAwareContainer_Null_Surface_Preserves_Transparent_Frame_Fill()
{
    var container = new ShadowsAwareContainer
    {
        BoxShadow         = BoxShadows.Parse("0 8 24 0 #66000000"),
        SurfaceBackground = null,
        Child              = new Border()
    };

    var frameRenderer = GetFrameRenderer(container);
    frameRenderer.ShouldNotBeNull();
    frameRenderer!.GetType().GetProperty("SurfaceBackground")
                 .ShouldNotBeNull()!.GetValue(frameRenderer)
                 .ShouldBeNull();
}
```

Rename the private test helper to look up `_frameRenderer`. Keep the existing `ClipToBounds == false` assertion.

- [ ] **Step 2: Run RED**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~PopupShadowTests"
```

Expected: compile failure because `Popup.SurfaceBackgroundProperty` and `ShadowsAwareContainer.SurfaceBackground` do not exist.

- [ ] **Step 3: Add the public Popup property and themed default**

In `Popup.cs`, register the property in the public property region and add the CLR wrapper:

```csharp
public static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
    AvaloniaProperty.Register<Popup, IBrush?>(nameof(SurfaceBackground));

public IBrush? SurfaceBackground
{
    get => GetValue(SurfaceBackgroundProperty);
    set => SetValue(SurfaceBackgroundProperty, value);
}
```

In `PopupTheme.axaml`, add:

```xml
<Setter Property="SurfaceBackground" Value="{atom:SharedTokenResource ColorBgElevated}" />
```

- [ ] **Step 4: Extend the existing shared frame renderer**

In `ShadowsAwareContainer.cs`:

- Add internal `SurfaceBackgroundProperty` as an owner of `Popup.SurfaceBackgroundProperty` and an internal CLR wrapper.
- Replace `_shadowsRenderer` with `_frameRenderer` and `BoxShadowRenderer` with `PopupFrameRenderer`.
- Replace the single shadow relay disposable with a two-binding `CompositeDisposable` owned by logical attach/detach.
- Create the renderer when either shadow exists or `SurfaceBackground` is non-null.
- Bind renderer shadow, corner radius, and surface background from the container.
- Preserve the existing arrange bounds and arrow-aware shadow geometry.
- Render `SurfaceBackground ?? Brushes.Transparent` with the existing `BoxShadow` call; return early only when both surface is null and shadow count is zero.

The renderer body must retain the prior transparent path exactly:

```csharp
var fill = SurfaceBackground ?? Brushes.Transparent;
context.DrawRectangle(fill, null, roundedRect, BoxShadow);
```

- [ ] **Step 5: Run GREEN and adjacent popup tests**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~PopupShadowTests|FullyQualifiedName~PopupPlacementTests|FullyQualifiedName~ToolTipPopupModeTests"
```

Expected: all selected tests pass.

- [ ] **Step 6: Commit the shared primitive**

```powershell
git add src/AtomUI.Desktop.Controls/Popup/Popup.cs src/AtomUI.Desktop.Controls/Popup/Themes/PopupTheme.axaml src/AtomUI.Desktop.Controls/Primitives/ShadowsAwareContainer.cs tests/AtomUI.Desktop.Controls.Tests/Popup/PopupShadowTests.cs
git commit -m "feat(Popup): add shared default surface"
```

### Task 2: Isolate all existing control visuals and guard the family inventory

**Files:**
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorPickerTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/Themes/GradientColorPickerTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteSearchEditTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTextAreaTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/DatePicker/Themes/RangeDatePickerTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Mentions/Themes/MentionsTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/InfoPickerInputTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/Themes/RangeInfoPickerInputTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Tour/Themes/TourTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs`
- Modify: `src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs`
- Modify: `src/AtomUI.Desktop.Controls/Menu/ContextMenu.cs`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Dialog/PopupEntryInventoryTests.cs`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupPrimitiveLayeringTests.cs`

**Interfaces:**
- Consumes: `Popup.SurfaceBackground` from Task 1.
- Produces: explicit content-owned surface declarations for all existing runtime Popup entry paths.
- Produces: inventory tests that reject unclassified Popup surface ownership.

- [ ] **Step 1: Add failing inventory and runtime-family assertions**

Add an AXAML surface ownership test that iterates the existing 17-file allowlist and asserts every `<atom:Popup>` start element contains:

```xml
SurfaceBackground="{x:Null}"
```

Add C# cases for `Flyout.cs`, `ToolTip.cs`, and `ContextMenu.cs` requiring the marker:

```csharp
SurfaceBackground = null
```

In `DialogPopupPrimitiveLayeringTests`, after opening Flyout, ToolTip, ContextMenu and MenuFlyout, resolve the owning `Popup` through `PopupUtils.GetOwningPopup(...)` from the presenter descendant and assert:

```csharp
owningPopup.SurfaceBackground.ShouldBeNull();
```

Keep Direct Popup separate and assert its `SurfaceBackground.ShouldNotBeNull()` after Theme attach/open.

- [ ] **Step 2: Run RED**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~PopupEntryInventoryTests|FullyQualifiedName~DialogPopupPrimitiveLayeringTests"
```

Expected: inventory and family assertions fail because existing consumers have inherited the new default surface.

- [ ] **Step 3: Opt every existing content-owned consumer out**

For every existing AXAML `<atom:Popup>` in the 17 audited files, add exactly:

```xml
SurfaceBackground="{x:Null}"
```

For the three shared C# Popup creation paths, add exactly:

```csharp
SurfaceBackground = null,
```

Do not alter the child presenter, PopupFrame, Token setter, Padding, CornerRadius, shadow, placement, animation, or open/close binding in the same edits.

- [ ] **Step 4: Run GREEN and family regressions**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~PopupEntryInventoryTests|FullyQualifiedName~DialogPopupPrimitiveLayeringTests|FullyQualifiedName~DialogPopupControlFamilyTests"
```

Expected: all selected tests pass and popup hosts clean up after close.

- [ ] **Step 5: Run representative visual-contract suites**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Flyout|FullyQualifiedName~Tooltip|FullyQualifiedName~ContextMenu|FullyQualifiedName~ComboBox|FullyQualifiedName~Select|FullyQualifiedName~Cascader|FullyQualifiedName~TreeSelect|FullyQualifiedName~Mentions|FullyQualifiedName~Tour"
```

Run optional ColorPicker tests:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~ColorPicker"
```

Expected: all selected tests pass with no new warning or geometry assertion changes.

- [ ] **Step 6: Commit the consumer isolation**

```powershell
git add src/AtomUI.Desktop.Controls src/AtomUI.Desktop.Controls.ColorPicker tests/AtomUI.Desktop.Controls.Tests/Dialog
git commit -m "refactor(Popup): declare content-owned surfaces"
```

### Task 3: Preserve the permanent Demo as a source-level acceptance test

**Files:**
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Dialog/PopupEntryInventoryTests.cs`
- Verify unchanged: `tests/AtomUI.Desktop.Controls.TestApp/Scenarios/PopupInDialog/PopupInDialogScenario.axaml.cs`
- Modify: `tests/AtomUI.Desktop.Controls.TestApp/README.md`

**Interfaces:**
- Consumes: direct Popup themed default from Task 1.
- Produces: a permanent acceptance contract proving the Demo contains no local background patch.

- [ ] **Step 1: Add a failing TestApp source contract before changing production expectations**

Add a test that isolates the `directPopup` initializer and asserts:

```csharp
directPopupSource.ShouldNotContain("SurfaceBackground");
directPopupSource.ShouldNotContain("Background =");
directPopupSource.ShouldContain("Child = new Border");
```

Also assert `PopupTheme.axaml` contains the `ColorBgElevated` `SurfaceBackground` setter. The test must verify the fix is owned by Popup, not Demo.

- [ ] **Step 2: Run the source contract**

Run:

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~PopupEntryInventoryTests"
```

Expected: pass after Tasks 1-2; if it fails, remove the Demo-specific background or surface override rather than weakening the assertion.

- [ ] **Step 3: Update TestApp acceptance instructions**

Document Direct Popup as the default-surface proof and add Linux commands:

```bash
ATOMUI_WINDOWING_PLATFORM=wayland dotnet run --project tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0
ATOMUI_WINDOWING_PLATFORM=x11 dotnet run --project tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0
```

Keep Linux X11/Wayland marked untested until real runs are reported.

- [ ] **Step 4: Build the TestApp**

Run:

```powershell
dotnet build tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0 --no-restore
```

Expected: build succeeds with zero warnings and zero errors.

- [ ] **Step 5: Commit the acceptance surface**

```powershell
git add tests/AtomUI.Desktop.Controls.Tests/Dialog/PopupEntryInventoryTests.cs tests/AtomUI.Desktop.Controls.TestApp/README.md
git commit -m "test(Popup): preserve default surface demo"
```

### Task 4: Synchronize durable Popup documentation

**Files:**
- Create: `docs/controls/desktop/other/popup/overview.md`
- Create: `docs/controls/desktop/other/popup/implementation.md`
- Create: `docs/controls/desktop/other/popup/token.md`
- Create: `docs/controls/desktop/other/popup/changelog.md`
- Modify: `docs/architecture/systems/rendering/visual-layers.md`
- Modify: `docs/controls/desktop/feedback/modal/popup-layering-design.md`
- Modify: LLMS source configuration selected by `atomui-control-documentation` routing.

**Interfaces:**
- Consumes: final API and implementation from Tasks 1-3.
- Produces: public and maintainer documentation for surface ownership, opt-out, host transparency, validation, and platform evidence.

- [ ] **Step 1: Read and apply the AtomUI control documentation skill**

Read `.agents/skills/atomui-control-documentation/SKILL.md` completely, then read only its references routed for a new control documentation set, Public API, implementation architecture, token mapping, changelog, and LLMS coverage.

- [ ] **Step 2: Write the Popup control documentation set**

Document these exact contracts:

- `SurfaceBackground` defaults to Shared Token `ColorBgElevated`.
- `null` means content-owned/transparent host frame.
- The property does not add Padding, border, arrow, or geometry.
- `PopupRoot.Background` remains `null`.
- Native and overlay paths share `ShadowsAwareContainer`.
- Existing AtomUI controls opt out and retain their Presenter/PopupFrame surfaces.
- Windows/macOS tested; Linux X11/Wayland untested.

- [ ] **Step 3: Update architecture and modal family documentation**

Add the host-versus-surface invariant, the inventory opt-out rule, the Direct Popup acceptance behavior, and the new test coverage without changing the existing owning TopLevel/layer model.

- [ ] **Step 4: Verify documentation structure and LLMS coverage**

Run the exact documentation verification commands routed by the documentation skill. Fix source coverage configuration rather than generated output.

- [ ] **Step 5: Commit documentation**

```powershell
git add docs/controls/desktop/other/popup docs/architecture/systems/rendering/visual-layers.md docs/controls/desktop/feedback/modal/popup-layering-design.md docs/AI
git commit -m "docs(Popup): define default surface ownership"
```

### Task 5: Final verification and repaired Demo launch

**Files:**
- Verify only; no implementation changes unless a failing test identifies a scoped defect.

**Interfaces:**
- Consumes: all previous tasks.
- Produces: current verification evidence and a running repaired TestApp.

- [ ] **Step 1: Run targeted Popup and Dialog suites**

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "FullyQualifiedName~Popup|FullyQualifiedName~DialogPopup|FullyQualifiedName~Flyout|FullyQualifiedName~ToolTip|FullyQualifiedName~ContextMenu"
```

- [ ] **Step 2: Run the owning Desktop Controls test project**

```powershell
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
```

If the two previously characterized order-sensitive TabActivation tests fail, rerun them individually and report them separately; do not alter unrelated Tab behavior in this Popup change.

- [ ] **Step 3: Validate repository hygiene**

```powershell
git diff --check
git status --short
```

- [ ] **Step 4: Rebuild and launch the actual repaired Demo**

```powershell
dotnet build tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0 --no-restore
dotnet run --project tests/AtomUI.Desktop.Controls.TestApp/AtomUI.Desktop.Controls.TestApp.csproj --framework net10.0 --no-build
```

The final user-visible acceptance is: Direct Popup has an opaque elevated surface, while Flyout, MenuFlyout, ToolTip, ContextMenu and all control-family popups retain their prior visuals.
