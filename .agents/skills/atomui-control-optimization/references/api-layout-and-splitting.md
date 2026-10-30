# AtomUI Control API, Layout, and Splitting

Use this reference when changing or reviewing a control's public API shape, member order, method layout, or partial-file structure.

## API Contract Optimization Levels

Treat "API optimization" as a compatibility decision, not a cleanup label.

Without explicit user approval, API, behavior, and rendered result must not change. Treat any required change in those categories as a stop-and-ask condition before implementation. Layout and API cleanup also must not introduce logic bugs, performance regressions, or resource leaks.

| Level | Meaning | Rule |
| --- | --- | --- |
| L0 internal organization | No public/protected API, Avalonia property/event, template, token, pseudo-class, Gallery, or behavior change | Allowed with normal verification |
| L1 compatible addition | Adds a property, event, interface capability, token, template part, or style entry | Explain name, default, nullability, binding semantics, docs, tests |
| L2 semantic change | Changes default value, binding priority, event timing, visual behavior, state flow, or observable interaction | Requires user review before implementation |
| L3 breaking change | Renames, deletes, changes type, changes enum values, changes resource key, changes template part, or removes supported usage | Default forbidden unless user explicitly authorizes and migration is documented |

Before editing, fill this out:

```text
Public/protected members:
StyledProperty / DirectProperty:
RoutedEvent / .NET events:
Implemented public interfaces:
Template parts:
Pseudo-classes:
Token / resource keys:
Gallery/docs exposed usage:
API change level: L0 / L1 / L2 / L3
Alternative without API change:
User approval required: Yes / No
```

## API Design Rules

- Use `StyledProperty` for user-facing state that must participate in style, theme, binding, animation, or local value priority.
- Use `DirectProperty` for runtime state that should not be style/theme overridden.
- Keep CLR wrappers in the same order as property registrations.
- Keep `DirectProperty` backing fields near the corresponding wrapper inside the contract section, not in ordinary runtime fields.
- A new property must document default value, nullability, binding meaning, and whether it affects measure, arrange, or render.
- A new event must define timing, cancellation semantics, repeatability, and whether it fires for internal sync.
- A new token must explain its relation to global tokens, control tokens, theme variables, and fallback behavior.
- Do not use "more reasonable" as a reason to change API without user approval.

## Member Layout Template

Prefer this layout for normal AtomUI controls, adjusted only to match strong local precedent.

```text
enum / small public state types

public class ControlName ...
{
    #region 公共属性定义
        public static readonly StyledProperty...
        public static readonly DirectProperty...
        CLR wrappers in the same order as registrations
    #endregion

    #region 公共事件定义
        RoutedEvent registration + event wrapper
        .NET events
    #endregion

    #region 内部属性定义
        internal template/theme contract properties
        internal DirectProperty + backing field + wrapper
    #endregion

    #region 内部协作 API
        internal constants, properties, and methods used by sibling controls
        internal static helpers that form a stable same-module collaboration contract
    #endregion

    static readonly helpers / constants
    runtime fields

    static ControlName()
    {
    }

    public ControlName()
    {
    }

    control API members, organized by control flow
        public operation APIs
        protected / protected virtual extensibility hooks
        protected override lifecycle, template, and property hooks
        protected layout / measurement / sizing hooks
        protected visual state / pseudo-class / theme sync hooks
        protected domain feature hooks
        protected input / pointer / keyboard hooks
        protected event notification hooks

    #region 实现 Xxx 接口
        explicit interface implementation
        directly paired protected virtual Notify/Hook methods, if they exist only for the interface adapter
    #endregion

    private implementation methods, organized by control flow
        template part wiring helpers
        property changed handlers
        private layout / measurement / sizing helpers
        private visual state / pseudo-class / theme sync helpers
        private domain feature methods
        private input / pointer / keyboard handlers
        async / data loading helpers
        collection / view refresh helpers
        private event notification helpers
}
```

## Contract-First Class Prelude

Inside a control class, the public contract region is the first reading entry. Do not put implementation or internal collaboration members before `#region 公共属性定义`.

Allowed before the control class:

- `enum` and small public state types that help explain the control model

Not allowed before `#region 公共属性定义` inside the control class:

- `internal const` or `internal static readonly` values used by sibling controls
- `internal static` helper methods, even if they are pure calculations
- internal collaboration APIs between related controls
- private helpers, runtime fields, template parts, disposables, state flags, or patch artifacts

Internal collaboration members are not public user API, but they are still contracts for sibling controls in the same module. Place them after the public and internal property/event contract regions, preferably in `#region 内部协作 API`, before ordinary private helpers and runtime fields.

If an `internal static` helper is used only by the current class, treat it as implementation detail and keep it with private implementation flow instead of promoting it above the public contract.

## Control API Before Interface Regions

After the static constructor and instance constructor, place the control's own API members before any `#region 实现 Xxx 接口`.

Control API members include:

- public or protected operations that are primary user-facing capabilities of the control
- protected methods and protected virtual hooks that are part of the control's extensibility contract
- protected overrides that are the natural entry points for lifecycle, template, property, layout, visual state, input, or domain behavior
- examples: `Open()`, `Close()`, `ClearSelection()`, `ScrollToNode(...)`, `FocusInput()`

The region between constructors and explicit interface implementations is not only for `public` methods. A protected API is still part of the control API and must not be pushed below explicit interface implementations merely because it is not public.

Not allowed before interface regions:

- private helpers
- private event handlers
- private layout calculations
- private visual state sync
- private collection refresh
- private async implementation details
- `NotifyXxx` methods unless they are protected/protected virtual control API hooks or directly paired with the interface region below

If a public or protected API method is mainly part of a feature flow, keep it near that flow inside the control API section instead of forcing every API method into a flat block immediately after constructors. For example, a public `PopulateComplete()` can live in the async/data loading part of the control API section when it is part of the populate pipeline.

The ordering priority is:

```text
constructors
control API members: public/protected/protected virtual/protected override
interface contract regions
private implementation flow
```

## Interface Regions

- Explicit implementations of public interfaces belong in `#region 实现 Xxx 接口`.
- Interface regions go after the control's own public/protected API members and before private implementation methods.
- Keep directly paired protected virtual hooks in the same interface region when their main purpose is adapting the explicit interface, such as `IFormItemAware.SetFormValue(...)` with `NotifySetFormValue(...)`.
- If the interface appears on the public control type declaration, treat it as an external capability even when the implementation is explicit.
- Internal-only collaboration interfaces may be placed with internal implementation details, but only when they are not part of the public type contract.

## Method Reordering Rules

- Reorder only when it clearly improves the reading path.
- Do not mechanically sort by `public` / `protected` / `private`.
- Do not hide lifecycle entry points in partial files.
- Do not separate `NotifyXxx` hooks from the interface or event flow they belong to.
- Do not mix member reordering with bug fixes, semantic changes, or formatting churn.
- If reordering exposes a correctness issue, stop and classify it as a separate behavior fix.
- Member reordering is not a substitute for broad control optimization. If the request says "重新优化", "整体优化", "代码乱", or "按照控件优化 skill 优化", perform the deep audit first and report structural findings before applying layout-only edits.
- If layout cleanup reveals state suppression, duplicated algorithms, no-op interface implementations, unreferenced helper classes, generated container state, C# runtime bindings, or collection/selection edge cases, escalate to `deep-control-optimization` and record the finding even if it is deferred.

## File Splitting Rules

Default: do not split.

- Under about `2000` lines, prefer member ordering, regions, and private helper extraction.
- Above about `2000` lines, consider partial files only when there is a stable responsibility boundary. The threshold makes splitting eligible for discussion; it does not require splitting.
- Keep the public API contract in the main control file: public/protected members, Avalonia property/event registrations, CLR wrappers, public events, constructors, and externally visible interface entry points.
- The main file also keeps ordinary fields, constructors, primary lifecycle overrides, and interface entry points unless a strong existing local pattern says otherwise.
- Split by the control's real logic domains, not by access modifier, file length, or a desire for visual symmetry.
- Do not split into many tiny partial files. Each partial must have a durable ownership theme that a maintainer can name without reading its internals.
- If two candidate partial files would frequently need to read each other's private state or be changed together, keep them together.

Good partial names:

```text
Control.Filter.cs
Control.AsyncLoad.cs
Control.Selection.cs
Control.DragAndDrop.cs
Control.StateReplay.cs
Control.Layout.cs
```

Avoid:

```text
Control.Private.cs
Control.Protected.cs
Control.Helpers.cs
Control.Part1.cs
Control.Methods.cs
```

## Reorganization Verification

For layout-only work, verify:

- contract inventory did not change
- no public/protected member moved into a different type or partial with changed accessibility
- no property registration, default value, binding priority, or event timing changed
- no template part, pseudo-class, token, or resource key changed
- tests still pass for the owning control project
- `git diff --check` is clean


## Executable Member Layout Gate

Functional tests and compilation do not prove member organization. Before declaring a touched control complete, run this read-only audit against every touched control file or the complete affected control family:

```bash
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/ImagePreviewer
```

The command uses the installed .NET 10 SDK's Roslyn parser, builds its helper in system temp, and removes build artifacts when it exits. It requires explicit `--path` arguments and does not modify source, stage files, or run regression tests. Repeat `--path` for disjoint scopes. Non-control utility types without Avalonia registrations are exempt from control-region checks; their control-flow braces are still inspected.

Review and resolve its findings for:

- public/protected versus internal property registration and wrapper regions;
- registration blocks, wrapper order and adjacent DirectProperty backing fields;
- runtime fields, public events, internal collaboration methods/events and constructors;
- public-contract prelude, explicit interface regions and API/private-method placement;
- control-flow braces and C# syntax errors.

Registration fields and CLR wrappers are classified by their own declared accessibility. An internal registration with a public wrapper can be intentional; preserve both access levels and place each in its own region. Do not change accessibility merely to make the audit pass.

For layout-only work, `--snapshot <external-text-path>` records per-member syntax-token fingerprints, and `--inventory <external-text-path>` provides a review inventory. Compare snapshots before and after; classify every non-layout difference separately. The tool is a syntax audit, not cross-project semantic compilation or an automatic formatter. Partial types are inspected per file; inherited APIs and conditional-compilation alternatives still require review.

Before moving static fields or auto-property initializers, inspect their dependencies and allocation timing. Moving a default-value helper after its registration can silently register null/default. Prefer an equivalent inline initializer when a private helper has only that registration as a consumer, and verify the original default behavior. Do not use region cleanup as permission for behavioral changes.

Report the explicit scope, inspected file/control counts and remaining findings. Passing module tests without this structural result is not completion evidence for a member-layout change.
