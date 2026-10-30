---
name: atomui-control-performance
description: Use when optimizing AtomUI controls, investigating control performance regressions, changing Avalonia styled-property bindings, lazy creation, templates, selectors, or Gallery performance scenarios. Applies to controls such as Space, Button, Icon, AddOnDecoratedBox, LineEdit, and shared primitives.
---

# AtomUI Control Performance Skill

> **Correctness outranks performance.** Every rule below exists to defend that priority. If any rule conflicts with correctness, correctness wins.

---

## Tier 1 — Hard Boundaries

Violating any of these blocks merge. No exception, no tradeoff, no "it's only one case".

1. **Correctness regression is unacceptable.** Performance optimization must preserve control functionality, UI appearance, animation behavior, interaction semantics, theme behavior, and public API. Any visible or behavioral change caused by an optimization is a regression, not a tradeoff. If the user explicitly approves a behavior change, that becomes a separate non-perf change, not a perf optimization.

2. **No logic bug introduction.** This includes invalid visual states, broken state transitions, incorrect property precedence, stale layout/render state, lost interactions, incorrect lifecycle cleanup, race conditions in event chains, or Gallery-visible behavior mismatches. Discovering one mid-optimization means stop, fix or revert immediately, do not continue stacking perf changes on top.

3. **No resource leak.** Anything created/subscribed/bound/cached/lazily materialized/reparented must have a defined and verified release path before the change is considered complete. Leak scanning is mandatory during analysis, not optional cleanup afterward.

4. **No measurable speed regression.** If any primary metric for the targeted control or its real Gallery ShowCase gets worse under the same measurement policy, the change must be fixed, split, reverted, or explicitly reported as a blocker.

5. **No materially more complex implementation logic.** Fragile state machines, duplicated template/style logic in code, broad lifecycle orchestration, unclear synchronization, or code harder to reason about than the original behavior warrants — all indicate the optimization is too expensive in complexity. Stop and propose a simpler option instead. See Process Gate 3 for the numeric thresholds that flag this.

6. **No theme element → C# dynamic creation.** See [Theme Static Rule](#theme-static-rule). This is the highest-leverage rule in this document; nearly all rollback incidents stem from violating it.

7. **No same-priority binding collisions for the same property.** If a property has both an internal default binding and an external override binding, they must be at different `BindingPriority` levels. See [Avalonia Binding Priority Guardrails](#avalonia-binding-priority-guardrails).

8. **No `BindUtils.RelayBind` + disposable plumbing for lifetime-consistent children.** When source and target have the same owner and lifetime, use Avalonia `[!]` binding syntax. Disposable infrastructure is reserved for mismatched lifetimes, replacement paths, detach/re-template, global/window subscriptions, timers, or lazily materialized objects.

9. **3-rounds optimization budget.** For the same scoped target, three implementation-and-measurement rounds without primary speed metric improvement = stop. At end-of-optimization, if no worthwhile gain: restore every perf-only change, delete intermediate scratch code/docs, keep only correctness fixes / leak fixes / useful measurement tooling. Report exactly what was reverted, what correctness fixes were kept, what cleanup was done.

10. **5-control pattern rollout circuit breaker.** Same refactor pattern (e.g., dynamic creation, lazy popup materialization, binding restructure) applied to ≥ 5 controls = stop and audit. Two questions: (a) did at least 3 of 5 show measurable speed gain? (b) did any one introduce ≥ 2 Gallery-visible bugs? Either "no" / "yes" stops further rollout and triggers reflection before the 6th control.

11. **No unsupported claims about framework behavior.** Any assertion such as "X triggers Y", "Z is O(N)", "this binding takes the fast path", or "IsVisible=False is free" MUST be backed by the currently resolved dependency source or by a measurement with explicit methodology. Source inspection is evidence about the current implementation, not an AtomUI architecture contract. Long-term documents keep AtomUI invariants and revalidation conditions; exact external versions, paths and line numbers belong only in dated upgrade or investigation records. See [Framework Behavior Evidence](#framework-behavior-evidence).

12. **No `_ignoreXxx` flags in property-changed handlers.** Adding a private `bool` (`_ignoreSelectedPropertyChanged`, `IgnorePropertyChange`, `_isUpdating`, `_suppressNotification`, etc.) to short-circuit property-changed dispatch hides bugs rather than fixing them. The Cascader / AbstractSelect rollbacks all follow this pattern. See [Re-entrancy & Ignore-Flag Guardrails](#re-entrancy--ignore-flag-guardrails). The few legitimate cases are listed there; the default answer is "fix the event flow, do not add the flag".

13. **Optimization qualification documented.** Every perf commit must state — in the commit description — the realistic instance count for the targeted control in its busiest Gallery ShowCase, and the per-session frequency of the operation being optimized. If instance count ≤ 5 AND operation frequency < 1/session, the commit must explain why optimization is still warranted, or be rejected before measurement begins. The investigation playbook's qualification gate (see [First-60-Minutes Investigation Playbook](#first-60-minutes-investigation-playbook)) is the operational form of this rule.

14. **Benefit report is mandatory at completion.** Every optimization turn MUST proactively report the benefit to the user before stopping, even if the user did not ask. Start with a one-sentence plain-language takeaway that says what became cheaper and where the user benefits. Then include a table with `metric / baseline / optimized / formula / improvement / conclusion`. Metric names must be user-readable and include units/scopes such as `per close`, `per row`, `per DataGrid`, or `per Gallery page`; do not report only internal method/class names or raw callsite counts. If the optimization is structural-only, report the structural count reduction (for example `handlers per instance`, `bindings per item`, `visuals per root`, `objects allocated per operation`) and its percentage, and explicitly say it does not claim page-load speedup unless timing proves it. If timing data is single-run smoke, label it smoke-only and do not present it as proof of speedup. If no valid before/after timing exists, explicitly say that no timing percentage is claimed and use the structural/correctness result as the benefit. This is part of the definition of "done".

---

## Framework Behavior Evidence

Use the source matching the currently resolved Avalonia dependency to verify framework-behavior hypotheses. **Every framework-behavior claim in a review or commit description must cite current implementation evidence or reproducible measurement.** Revalidate source-derived claims after dependency upgrades, and never promote a particular external version, commit, local path, or line number into AtomUI's long-term architecture contract.

The verified cost model and counter-intuitive points are maintained in this skill. Use the Cost Model below for both the evidence summary and the operational rule.

### Required reading paths by subsystem

- **Property system**: `Avalonia.Base/AvaloniaProperty.cs`, `AvaloniaObject.cs`, `PropertyStore/ValueStore.cs`, `Data/BindingPriority.cs`, `PropertyStore/FramePriority.cs`
- **Binding**: `Avalonia.Base/Data/TemplateBinding.cs`, `Data/TemplateBindingExpression.cs`, `Data/Core/BindingExpression.cs`, `Data/Core/MultiBindingExpression.cs`, `AvaloniaObjectExtensions.cs`, `PropertyStore/DirectBindingObserver.cs`, `Markup.Xaml.Loader/CompilerExtensions/XamlIlBindingPathHelper.cs`
- **Layout**: `Avalonia.Base/Layout/LayoutManager.cs`, `Layout/Layoutable.cs`, `Layout/LayoutQueue.cs`, `Visual.cs`
- **Render / Composition**: `Avalonia.Base/Visual.cs` (`AffectsRender`, `InvalidateVisual`), `Rendering/IRenderer.cs`, `Rendering/ImmediateRenderer.cs`, `Rendering/Composition/Server/ServerCompositionVisual/ServerCompositionVisual.DirtyInputs.cs`, `Media/SolidColorBrush.cs`, `Media/Pen.cs`, `Media/StreamGeometry.cs`
- **Routed events**: `Avalonia.Base/Interactivity/RoutedEvent.cs`, `Interactivity/Interactive.cs`, `Interactivity/EventRoute.cs`, `Input/InputElement.cs`
- **Style / Selector**: `Avalonia.Base/Styling/Selector.cs`, `Styling/TemplateSelector.cs`, `Styling/Activators/PropertyEqualsActivator.cs`, `Styling/Activators/StyleClassActivator.cs`, `Styling/Activators/NthChildActivator.cs`, `Styling/ControlTheme.cs`, `Styling/Setter.cs`, `Styling/PropertySetterTemplateInstance.cs`, `Styling/Styles.cs`, `StyledElement.cs`
- **Templates / ItemsControl**: `Avalonia.Controls/Primitives/TemplatedControl.cs`, `Controls/Presenters/ItemsPresenter.cs`, `Controls/Presenters/ContentPresenter.cs`, `Controls/Templates/DataTemplateExtensions.cs`, `Controls/ItemsControl.cs`, `Controls/VirtualizingPanel.cs`
- **Popup**: `Avalonia.Controls/Primitives/Popup.cs`, `Primitives/PopupRoot.cs`, `Primitives/OverlayPopupHost.cs`
- **Dispatcher / Threading**: `Avalonia.Base/Threading/DispatcherPriority.cs`, `Threading/Dispatcher.cs`, `Threading/Dispatcher.Invoke.cs`, `Threading/Dispatcher.Timers.cs`
- **Animation / Transition**: `Avalonia.Base/Animation/Animatable.cs`, `Animation/Transition.cs`

### Required practice

- Before claiming X is cheaper than Y in a commit description, point at the source path proving it. The Cost Model below is the first lookup; the pitfalls doc gives the explanation.
- If you can't find the source backing your claim in 5 minutes, your claim is a guess. Either measure it explicitly or remove it.

### Verification mark

Items still pending source verification are marked **`[VERIFY]`**. Decisions must not depend on `[VERIFY]` items. The current open list lives in §13 of the pitfalls doc.

---

## Theme Static Rule

> Functional visuals defined in `ControlTemplate` / `ControlTheme` stay in axaml. Moving them to C# `EnsureXxx()` / `ClearXxx()` dynamic creation as a performance optimization is **prohibited by default**.

### Why this is a hard boundary

The cost asymmetry is overwhelming:

| Item | axaml static + `IsVisible` toggle | C# dynamic Ensure/Clear |
| --- | --- | --- |
| Instantiation | Template inflation once | Each Ensure rebuilds |
| Hidden cost | `IsVisible=False` short-circuits `MeasureCore`, `ArrangeCore`, and `ImmediateRenderer.Render` (`Avalonia.Base/Layout/Layoutable.cs:546`, `:671`; `Avalonia.Base/Rendering/ImmediateRenderer.cs:34`) | Active until ClearXxx releases |
| TemplateBinding | Reactive via XAML, direct `PropertyChanged` subscription (`Avalonia.Base/Data/TemplateBindingExpression.cs:37-43`) | Manual `SetCurrentValue` + property change handlers |
| Style/Selector | Cascade automatically — but only if child has `TemplatedParent`, see below | Each rule re-implemented in C# |
| Lifecycle | Avalonia auto-managed | Manual disposables + ordered teardown |
| Race / ordering bugs | ~zero | High frequency (the rollback evidence) |

The "saved" cost of a hidden axaml node is one `Visual` instance plus its initial template inflation. Every rollback in §[Incident Callouts] confirmed that this saving was outweighed by the ongoing churn of manual `Ensure*/Clear*` plus the lost `/template/` cascade. The complexity cost of dynamic creation is permanent and compounds across controls.

### `/template/` selector requires `TemplatedParent` — non-negotiable

If you must create template children in C# (one of the three exceptions below), the framework will not match `/template/` selectors against them unless `TemplatedParent` is set. `TemplateSelector.Evaluate` returns `NeverThisInstance` when `control.TemplatedParent == null` (`Avalonia.Base/Styling/TemplateSelector.cs:39-49`). That means every theme rule like `^[X=true] /template/ Foo#bar { ... }` silently stops applying to the new C# child.

Required handling whenever a template child is created in C#:

1. Call `child.SetTemplatedParent(this)` immediately after creation.
2. Call `child.SetTemplatedParent(null)` in the symmetric `Clear*` path or `OnDetachedFromVisualTree`.
3. The Resource Leak Detection grep already includes both calls — confirm the pair stays balanced.

### Allowed exceptions (must be argued in commit description)

1. **Heavy popup content.** Subtree node count ≥ 20, or contains `ItemsControl` / `ScrollViewer` / `TreeView` / `CascaderView` / calendar/time panels / large layout trees. The lightweight `Popup` shell still stays in axaml; only the popup's CONTENT may be deferred. See [Popup Lazy Content Rule](#popup-lazy-content-rule).
2. **Adorner / overlay-layer content.** Adorners cannot live in `ControlTemplate` (architectural constraint, see `Avalonia.Controls/Primitives/AdornerLayer.cs`). Notification cards, overlay dialogs, drawer overlays, tooltips fall here.
3. **Variable-N data items.** When N is unknown at compile time. Prefer `ItemsControl` + `ItemTemplate`; only when ItemsControl is structurally insufficient may `Children.Add` be used directly.

Any "fourth case" requires opening a discussion with the user and updating this skill before merge.

### Reasons that are NOT acceptable for an exception

- ❌ "Save X bytes of allocation"
- ❌ "Save unused PropertyChanged broadcasts"
- ❌ "Element only used in mode Y" — use `IsVisible="False"` + selector instead
- ❌ "First-time inflation is slow" — one-time cost, dominated by runtime Ensure churn
- ❌ "Keep parity with TreeSelect / Select / etc." — consistency does not override hard boundary

### Right way to make a sometimes-shown element cheap

```xml
<!-- ✅ Correct: stay in axaml, hide by default, reveal via selector -->
<atom:CheckBox Name="ToggleCheckbox"
               IsChecked="{TemplateBinding IsChecked, Mode=TwoWay}"
               IsVisible="False" />

<Style Selector="^[ToggleType=CheckBox] /template/ atom|CheckBox#ToggleCheckbox">
    <Setter Property="IsVisible" Value="True" />
</Style>
```

```csharp
// ❌ Wrong: do not move axaml elements into C# dynamic creation
private CheckBox? _toggleCheckbox;
private CompositeDisposable? _toggleCheckboxDisposables;
private void EnsureToggleCheckbox() { /* ... */ }
private void ClearToggleCheckbox() { /* ... */ }
```

### Reviewer must check

Before approving any perf commit, grep new `EnsureXxx()` / `ClearXxx()` introductions and confirm each falls into one of the three allowed exceptions. If not, the commit fails this hard boundary.

---

## Popup Lazy Content Rule

This is the implementation of Theme Static Rule exception 1.

- A **lightweight `Popup` shell** stays in `ControlTheme`. It preserves placement, light-dismiss, overlay, theme styling, and required template contracts.
- **Heavy popup content** must not be created in the default closed state. Examples: candidate lists, `TreeView`, `CascaderView`, calendar/time panels, complex item presenters, filter lists, empty indicators, large popup layout trees.
- **First-open materialization.** Create popup content immediately before the first open, wire events and bindings there, sync pending selection/filter/items state after creation.
- **Keep materialized after close** by default — open/close churn is worse than the saved allocation. Release on re-template, detach, or explicit disposal paths.
- **Closed controls pay nothing.** No popup-only event subscriptions, no items-source copies, no filter setup, no selection sync, no heavy visual tree creation while popup is closed.
- **Lifecycle verification.** Cover first open, close, second open, re-template, detach, visual parent cleanup, event unsubscribe, binding disposal, and state toggles (loading/filter/selection).

---

## Framework Cost Model

The following model records implementation evidence used to form and falsify performance hypotheses. Source coordinates are review aids for the currently resolved dependency and must be revalidated after upgrades; the AtomUI implication and its measurement gate are the durable parts. Use each entry's evidence, AtomUI implication, and measurement gate as the complete review record.

### Property System

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `StyledProperty` read | `ValueStore.GetValue` does a dict lookup on `_effectiveValues`, falls back to inherited/default; no per-frame priority resolution on read. | `Avalonia.Base/PropertyStore/ValueStore.cs:286-294`, `Avalonia.Base/AvaloniaObject.cs:251-258` | Reads are O(1) but allocate-free only when the entry exists. |
| `DirectProperty` read | Direct getter delegate; bypasses `ValueStore` entirely. | `Avalonia.Base/DirectPropertyBase.cs` | Cheapest read path. Use ONLY for runtime-state properties never controlled by Style / Animation / Template. |
| `BindingPriority` order | `Animation = -1, LocalValue = 0, StyleTrigger = 1, Template = 2, Style = 3, Inherited = 4, Unset = int.MaxValue`. Lower numeric = higher priority (wins). | `Avalonia.Base/Data/BindingPriority.cs:9-50`, `Avalonia.Base/PropertyStore/FramePriority.cs:6-35` | Internal token defaults vs user override must live on different priority frames. Same-priority writes Dispose each other. |
| `IsSet(property)` | True for any `EffectiveValue` entry, regardless of source (Style / Trigger / Template / LocalValue / Animation). | `Avalonia.Base/PropertyStore/ValueStore.cs:352`, `Avalonia.Base/AvaloniaObject.cs:298-304` | Cannot be used as "user explicitly set this" check. Past Space incident roots here. |
| `SetCurrentValue` vs `SetValue` vs `ClearValue` | `SetValue` writes at LocalValue (overrides lower priorities); `SetCurrentValue` writes at the current effective frame without disturbing bindings; `ClearValue` removes the LocalValue entry to let lower-priority sources show through. | `Avalonia.Base/AvaloniaObject.cs:333-355`, `:407-422` | Theme/template default initialization → `SetCurrentValue`. "Reset to default" → `ClearValue`, never `SetValue(default)`. |
| `AffectsMeasure / AffectsArrange / AffectsRender` | Static registration subscribes once to `property.Changed`; cost is paid only when the registered property actually changes. | `Avalonia.Base/Layout/Layoutable.cs:502-512`, `Avalonia.Base/Visual.cs:446-500` | Don't flag every property "for safety". Hot-frequency properties carrying `AffectsRender` cost one `InvalidateVisual` per change. |
| `RaisePropertyChanged` with no subscribers | Null-checks `_propertyChanged`; the bare property change is cheap. | `Avalonia.Base/AvaloniaObject.cs:761-806` | Cost is dominated by Affects* + selector activators + binding observers, not the raise itself. |

### Binding System

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `TemplateBinding` | Allocates a single `TemplateBindingExpression` and subscribes directly to `templatedParent.PropertyChanged`. No expression-node walk. | `Avalonia.Base/Data/TemplateBinding.cs:57-68`, `Data/TemplateBindingExpression.cs:37-43` | All same-owner same-lifetime template bindings must be `{TemplateBinding X}`, never `{Binding RelativeSource={RelativeSource TemplatedParent}, Path=X}`. |
| `[!]` direct binding | `AvaloniaObjectExtensions.Bind(IObservable<T>)` → `DirectBindingObserver<T>`; one observer object, one disposable. | `Avalonia.Base/AvaloniaObjectExtensions.cs:188-204`, `PropertyStore/DirectBindingObserver.cs:7-84` | Lifetime-matched parent ↔ template-child is `[!]`. Tier 1 §8 binds this. |
| `BindUtils.RelayBind` | AtomUI wrapper that constructs a full `Binding` + `Path` and calls `target.Bind(...)`. Strictly heavier than `[!]`. | AtomUI source under `src/AtomUI.Base/` | Only for mismatched lifetimes, replacement paths, detach/re-template, conditional bindings. |
| `MultiBinding` + Converter | Each child observable change fires `OnChanged`; once all children initialized, `Converter.Convert` runs once per change. | `Avalonia.Base/Data/Core/MultiBindingExpression.cs:23-49`, `:86-98`, `:116` | Converter must be stateless and allocation-free. High-frequency child binding pulls converter with it. |
| `$parent[T]` / `LogicalAncestorElementNode` | Subscribes to `Attached/DetachedFromLogicalTree`; recomputes `GetLogicalAncestors().ElementAtOrDefault(level)` on every tree change, not cached. | `Avalonia.Base/Data/Core/ExpressionNodes/LogicalAncestorElementNode.cs:59-68`, `LogicalTree/ControlLocator.cs:63-68` | Avoid in templates that re-attach often. Push state to a control-level `StyledProperty` and `TemplateBinding` from the popup content instead. |
| Compiled bindings (`x:DataType`) | XAML compiler emits direct IL property access nodes; no reflection at runtime. | `Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/XamlIlBindingPathHelper.cs:140-200`, `Avalonia.Base/Data/CompiledBinding.cs:195-235` | Hot-path Gallery / VM bindings should be compiled. ControlTheme bindings are already covered by `TemplateBinding`. |
| Forgotten `Bind(...)` `IDisposable` | `BindingExpression._source` is a `WeakReference<object?>`; target holds the binding. No source-leak, but the subscription stays live. | `Avalonia.Base/Data/Core/BindingExpression.cs:26-90`, `Data/Core/UntypedBindingExpressionBase.cs:100-113` | Bindings created in detach/re-attach paths must go in `CompositeDisposable`. axaml `[!]` is auto-managed by templated child lifetime. |

### Layout & Visual Tree

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `IsVisible=False` short-circuit | `MeasureCore` returns immediately outside `if (IsVisible)`; same for `ArrangeCore`; `ImmediateRenderer` returns at the top when visual is not visible. | `Avalonia.Base/Layout/Layoutable.cs:546`, `:671`; `Avalonia.Base/Rendering/ImmediateRenderer.cs:34` | Confirmed free. Backbone of Theme Static Rule. |
| `IsVisible` setter | Triggers `UpdateIsEffectivelyVisible` recursively + parent `ChildDesiredSizeChanged` + own `InvalidateMeasure`; resets `DesiredSize` to default. | `Avalonia.Base/Layout/Layoutable.cs:842-868`, `Avalonia.Base/Visual.cs:507-509` | Don't flip `IsVisible` per frame. Don't use it as a "force re-measure" hack. |
| `InvalidateMeasure` | Sets local `IsMeasureValid=false`, enqueues with `LayoutManager`. Does NOT walk to root unless child `DesiredSize` actually changes. | `Avalonia.Base/Layout/Layoutable.cs:443-459`, `:480-486`; `Avalonia.Base/Layout/LayoutManager.cs:304` | Cheap by itself. Cost is the subsequent `MeasureOverride`. |
| `LayoutManager` queue | Per-pass deduplication via `LayoutQueue._loopQueueInfo`; max 10 passes per tick; scheduled via `MediaContext.BeginInvokeOnRender`. | `Avalonia.Base/Layout/LayoutManager.cs:23`, `:116-179`, `:348-355`; `Layout/LayoutQueue.cs:48-68` | N attribute changes in one frame ⇒ one `MeasureOverride`. Don't bother batching property setters for that reason alone. |
| `Grid` star-sizing | 4 measurement groups + cyclic-dependency loop up to `c_layoutLoopMaxCount`. Not "always 2 passes". | `Avalonia.Controls/Grid.cs:234-527` | Avoid star-grid as default panel. AtomUI conventions already favor `DockPanel` / `StackPanel` / `FlexPanel`. |
| Reparenting | Triggers `OnDetachedFromVisualTreeCore` → `OnAttachedToVisualTreeCore` → `LayoutHelper.InvalidateSelfAndChildrenMeasure` (whole subtree) + `OnTemplatedParentControlThemeChanged`. | `Avalonia.Base/Visual.cs:715-738`, `:551`, `:776-791`; `Layout/Layoutable.cs:872-877` | Almost never worth doing for perf. Use container recycling, not manual reparent. |
| `EffectiveViewportChanged` | First subscriber registers with `LayoutManager._effectiveViewportChangedListeners`; raised every layout pass for every listener. | `Avalonia.Base/Layout/Layoutable.cs:169-190`; `Layout/LayoutManager.cs:219-220`, `:357-396` | Don't subscribe per item. One subscriber per scrolling host is fine. |

### Render / Composition

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `InvalidateVisual` | Calls `Renderer.AddDirty(this)`; queues for next render tick. Not synchronous. | `Avalonia.Base/Visual.cs:418-421`; `Rendering/IRenderer.cs:30-33` | Multiple invalidations per frame coalesce. Don't double-invalidate via both `AffectsRender` and explicit calls. |
| Compositor-only animations | `Opacity / Offset / Scale / RotationAngle / Translation / AnchorPoint / CenterPoint` route to compositor without UI-thread layout. `Size` / `ClipToBounds` still pulls UI thread. | `Avalonia.Base/Rendering/Composition/Server/ServerCompositionVisual/ServerCompositionVisual.DirtyInputs.cs:89-118` | Show/hide animations: `Opacity` + `RenderTransform`. Never animate `Width/Height`. |
| `SolidColorBrush` / `Pen` | Mutable `StyledProperty` carriers with full change notification. `ImmutableSolidColorBrush` / `ImmutablePen` exist for cached use. | `Avalonia.Base/Media/SolidColorBrush.cs:13-95`; `Media/Pen.cs:17` | Custom `Render(DrawingContext)` must cache brushes/pens, not `new` them per frame. |
| `StreamGeometry.Parse(string)` | Re-parses the path string each call via `PathMarkupParser`. | `Avalonia.Base/Media/StreamGeometry.cs:34-44` | Path data lives in axaml resources or generated metadata, not in repeated C# `Parse` calls. |
| `IsHitTestVisible=false` vs `IsVisible=false` | `IsHitTestVisible` only skips hit-test. `IsVisible=false` skips measure / arrange / render. | `Avalonia.Base/Input/InputElement.cs:64-65`; `Visual.cs:58-59`; `Rendering/ImmediateRenderer.cs:34` | Want "no layout/render cost" → `IsVisible=false`. Want "still visible but inert" → `IsHitTestVisible=false` or `IsEnabled=false`. |

### Routed Events

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| Route construction | Built eagerly per `RaiseEvent`, walks `InteractiveParent` chain into a pooled `EventRoute`. | `Avalonia.Base/Interactivity/Interactive.cs:143-177`; `Interactivity/EventRoute.cs:13` | Cost is O(depth × handler-count). High-frequency events should be filtered at source before raise. |
| `Handled = true` | Does NOT abort the route. Subsequent handlers see `e.Handled == true`; only those registered with `handledEventsToo` still run. | `Avalonia.Base/Interactivity/EventRoute.cs:158-170`; `Interactivity/RoutedEventArgs.cs:43` | Class-handler cleanup that must always run requires `handledEventsToo: true`. |
| Class vs instance handler | Class handlers subscribe once to `RoutedEvent.Raised`; instance handlers go in per-control `_eventHandlers` dict. | `Avalonia.Base/Interactivity/RoutedEvent.cs:84-94`; `Interactivity/Interactive.cs:16`, `:39-41`, `:181` | Default control-library `OnXxx` overrides should register via `AddClassHandler<T>`, not per-instance. |
| `PointerEntered` / `PointerExited` routing | Both registered as `RoutingStrategies.Direct`. No tunnel/bubble. | `Avalonia.Base/Input/InputElement.cs:144-147`, `:152-155` | Nested hover regions each receive their own enter/exit. Prefer `:pointerover` selector over hand-managed handlers. |

### Style / Selector / ControlTheme

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `/template/` selector | Returns `NeverThisInstance` if `control.TemplatedParent == null`. C# `Children.Add` without `SetTemplatedParent(this)` ⇒ no match. | `Avalonia.Base/Styling/TemplateSelector.cs:39-49` | If you go to C# child creation, you MUST set `TemplatedParent` and clear it on teardown. See Theme Static Rule. |
| Selector activator | Each selector subscribes to its dependency observable; `ReevaluateIsActive` runs on every dependency change. | `Avalonia.Base/Styling/Selector.cs:44-68`; `Styling/Activators/PropertyEqualsActivator.cs:25-40` | Pseudo-class toggles are O(1). Compound selectors (`^[X=true]:pointerover`) re-evaluate on EVERY input — keep them shallow. |
| `ControlTheme.BasedOn` | Linear recursive walk in `ApplyControlTheme`. No caching. | `Avalonia.Base/StyledElement.cs:776-793` | BasedOn chain ≤ 3 levels. AtomUI internal token themes already meet this. |
| `Setter` application | Plain `Setter` applies at `StyleBase.Attach`. `PropertySetterTemplateInstance` lazy-builds with `_value ??= _template.Build()`. | `Avalonia.Base/Styling/Setter.cs:69-92`; `Styling/PropertySetterInstance.cs:45-71`; `Styling/PropertySetterTemplateInstance.cs:7-31` | Heavy template-valued setters are effectively lazy and OK to leave in. Plain setters are not. |
| Runtime `Styles.Add/Remove` | Triggers full re-attach over the scope; no fast path. | `Avalonia.Base/Styling/Styles.cs:286-310`, `:266`, `:282` | Don't mutate `Application.Styles` or scoped `Styles` in hot path. Theme variant switching goes through `ThemeVariant`. |
| `:nth-child` / `:nth-last-child` | `NthChildActivator` re-evaluates on `ChildIndexChanged`; needs an `IChildIndexProvider`. | `Avalonia.Base/Styling/Activators/NthChildActivator.cs:46-76` | Custom virtualizing panels must provide O(1) `GetChildIndex` for `:nth-child` to be free. |

### Templates / ItemsControl

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `ApplyTemplate` | Runs during `MeasureCore`, after styling pass. Cached in `_appliedTemplate`; not rebuilt unless template actually changed. | `Avalonia.Controls/Primitives/TemplatedControl.cs:306-348`, `:316`, `:123` | `OnApplyTemplate` is the cheap place to wire PART_ refs. Don't pretend it's a one-shot — re-templating happens. |
| `ItemsPresenter` realization | `if (Panel is VirtualizingPanel v) v.Attach(ItemsControl); else _generator = new ItemContainerGenerator(...);` | `Avalonia.Controls/Presenters/ItemsPresenter.cs:86-119`, `:107-110`, `:150-154`; `Controls/VirtualizingPanel.cs:133` | List-shaped controls default to `VirtualizingStackPanel`. Don't expose `ItemsPanel` as a runtime swap. |
| `DataTemplate` lookup | Walks logical tree on every Content/ContentTemplate change; matches each candidate template. Not cached across instances. | `Avalonia.Controls/Templates/DataTemplateExtensions.cs:20-62`; `Controls/Presenters/ContentPresenter.cs:633`, `:640-650` | Use `IRecyclingDataTemplate` for list scenarios. Avoid N mutually-exclusive DataTemplates. |
| `ItemsControl.Items` change | Forwards `NotifyCollectionChangedAction` to panel; no diffing. Reset = full rebuild. | `Avalonia.Controls/ItemsControl.cs:639-656` | Filter / refresh APIs should emit `Add`/`Remove`/`Replace`, not `Reset`. |

### Popup

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| Window-host vs overlay-host | `Popup.Open` calls `OverlayPopupHost.CreatePopupHost`; window host calls `topLevel.PlatformImpl?.CreatePopup()` (native window, expensive). Overlay host reuses `PopupOverlayLayer` (cheap). | `Avalonia.Controls/Primitives/Popup.cs:449`, `:791-804`; `Primitives/OverlayPopupHost.cs:74`, `:158-160`, `:164-166` | Frequent open/close popups → overlay. AtomUI defaults are correct here. |
| Routed events through popup boundary | `PopupRoot.InteractiveParent => (Interactive?)Parent`, where `Parent` is the logical `Popup`. Events bubble through. | `Avalonia.Controls/Primitives/PopupRoot.cs:96` | Routed events DO traverse the popup boundary via logical ancestry. Use that for state propagation. |
| Visual ancestry through popup boundary | `PopupRoot` is a separate visual root; `GetVisualAncestors` does NOT cross it. | `Avalonia.Controls/Primitives/PopupRoot.cs:96` | Code that needs visual ancestor (`TransformToVisual`, adorner positioning) must explicitly account for the host boundary. |
| Light dismiss | Per-`TopLevel` `LightDismissOverlayLayer`, not global capture. | `Avalonia.Controls/Primitives/Popup.cs:542`, `:557` | Nested popups must filter their own outside-click logic; the framework only gives you per-host dismiss. |

### Dispatcher / Threading

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `DispatcherPriority` | `int Value`. Order: `SystemIdle < ApplicationIdle < ContextIdle < Background < Input < Default(0) < Loaded < UiThreadRender < AfterRender < Render < BeforeRender < AsyncRenderTargetResize < DataBind < Normal < Send`. Higher value = runs first. | `Avalonia.Base/Threading/DispatcherPriority.cs:25-127` | "Wait until just after attach/load" → `DispatcherPriority.Loaded`. Default `Post` runs at `Default`, can be preempted by Render / Input / DataBind. |
| `Post` / `InvokeAsync` / `Invoke` | `Post` and `InvokeAsync` are O(1) enqueue. `Invoke` is synchronous; on UI thread + `Send` it runs immediately, otherwise blocks. | `Avalonia.Base/Threading/Dispatcher.Invoke.cs:107-111`, `:257-260`, `:616-620` | `Post` is NOT "in a moment". Don't use `Post` to fix timing — fix the event flow. |
| `DispatcherTimer` | Each tick scans the timer list to compute next due time. | `Avalonia.Base/Threading/Dispatcher.Timers.cs:52-65`, `:90` | Avoid one timer per item. Centralize short-lived timers. |

### Animation / Transition

| Topic | Verified behavior | Source | Implication |
| --- | --- | --- | --- |
| `Animatable.Transitions` activation | `EnableTransitions()` is called on `OnAttachedToVisualTree`. Until then, `CollectionChanged` is not subscribed and value changes don't run transitions. | `Avalonia.Base/Animation/Animatable.cs:62-103`, `:87-103` | Pre-attach writes normally do not animate. Attached-tree initialization uses owner-held `TransitionInitializationSession` suppression acquired before attachment; template targets opt in during construction. Follow the [state motion contract](../../../docs/architecture/systems/control-infrastructure/transition-initialization.md), not private reflection toggles or queued readiness. |

---

## Counter-Intuitive Framework Behaviors (verified)

These are the rules that catch new contributors most often. Each was confirmed (or refined) against source.

- **`IsVisible=False` is genuinely free.** Three-way short-circuit at `Layoutable.cs:546`, `:671`, `ImmediateRenderer.cs:34`. This is the foundation of [Theme Static Rule](#theme-static-rule).
- **`TemplateBinding` is its own fast path**, not a flavor of `Binding`. `TemplateBindingExpression` subscribes directly to `templatedParent.PropertyChanged`. `{Binding RelativeSource=...}` is materially heavier — never substitute it (`Avalonia.Base/Data/TemplateBindingExpression.cs:37-43` vs `Data/Core/BindingExpression.cs:60-134`).
- **`DirectProperty` reads bypass the value store**, so they cannot be overridden via Style / Animation / LocalValue priorities (`Avalonia.Base/DirectPropertyBase.cs`). Use them only for runtime-state that's never theme- or style-driven.
- **`/template/` selectors require `TemplatedParent`.** A C# child added with `Children.Add(...)` and no `SetTemplatedParent(this)` silently fails to match `^... /template/ ...` rules (`Avalonia.Base/Styling/TemplateSelector.cs:39-49`). This is the silent-cascade-loss footgun that most "C# dynamic" optimizations actually hit.
- **`$parent[T]` is recomputed on every (de)attachment**, not cached. It uses `LogicalAncestorElementNode` + `ControlLocator.Track` (`Avalonia.Base/Data/Core/ExpressionNodes/LogicalAncestorElementNode.cs:59-68`). For state that flows Popup ↔ content, store on the templated control and `TemplateBinding` from the popup content.
- **`[!]` direct binding holds a strong ref source → target.** Same-lifetime parent-child pairs are auto-managed (template lifetime). Cross-lifetime usage MUST go through `BindUtils.RelayBind` + `CompositeDisposable` (`Avalonia.Base/PropertyStore/DirectBindingObserver.cs:7-84`).
- **`Dispatcher.UIThread.Post(action)` is enqueue, not "in a moment."** `Render`, `DataBind`, `Input`, `Loaded` all preempt default priority (`Avalonia.Base/Threading/DispatcherPriority.cs:25-127`). If you find yourself reaching for `Post` to fix timing, the event flow is wrong.
- **`AffectsRender` registration is lazy.** Subscription is wired once at type registration; cost is only paid when the registered property actually changes (`Avalonia.Base/Visual.cs:446-500`). A property with no `AffectsX` registration is genuinely free of layout/render side-effects.
- **Selector activators re-evaluate on every dependency change.** Combined selectors like `^[Foo=true]:pointerover` re-evaluate on Foo change AND on hover state change. Keep selectors shallow (`Avalonia.Base/Styling/Selector.cs:44-68`).
- **`ControlTheme.BasedOn` is a linear recursive walk**, no caching (`Avalonia.Base/StyledElement.cs:776-793`). Keep BasedOn chains ≤ 3 deep.
- **Routed events traverse the popup boundary; visual ancestry does not.** `PopupRoot.InteractiveParent` returns the logical `Popup` (`Avalonia.Controls/Primitives/PopupRoot.cs:96`), so `RoutedEvent` bubbling works. But `PopupRoot` is a separate visual root, so `GetVisualAncestors` and `TransformToVisual(window)` don't cross it. Pick the right ancestry mode for the situation.
- **`IsSet(property)` is "any effective value entry exists"**, not "user explicitly set this" (`Avalonia.Base/PropertyStore/ValueStore.cs:352`). The Space `ItemSpacing/LineSpacing` incident roots here.
- **Internal token defaults and external user bindings on the same property must NOT share priority.** Same-priority entries Dispose each other. `BindingPriority` numeric ordering is verified at `Avalonia.Base/Data/BindingPriority.cs:9-50`.

---

## Performance Decision Tree

When a "X is slow" report comes in, walk this tree before writing any code. Each step has a cut-off — stop early instead of forcing a fix.

```
报告 / 假设："X 慢"
  │
  ▼
Step 1. 测量复现
  │ - DevTools 触发 X，记录: layout pass 数 / property change 数 / allocation rate
  │ - AtomUI.Performance 跑 cold + repeated samples (≥10 cold)
  │ - 没有数字 → 拒绝优化（不许 fix vibes）
  │
  ▼
Step 2. 识别 Avalonia 子系统
  │ - 落在 Cost Model 哪个章节？
  │ - 该子系统的单位成本量级是多少？(必须有 source 引用，不许猜)
  │ - 实际触发频率是多少？(测量得来，不许估)
  │ - 单位成本 × 频率 ≥ 1ms / 帧预算占比 ≥ 5% ?
  │
  ├── 否 → 不是真瓶颈，没有优化空间 → 拒绝
  │
  └── 是 → Step 3
  │
  ▼
Step 3. Avalonia 是否已提供更便宜的等价路径？
  │ - axaml 静态 + IsVisible/Selector 能否实现？
  │ - DirectProperty 是否合适（无须 styled value frame）？
  │ - 编译绑定 (CompiledBinding) 能否替换反射绑定？
  │ - Static cached Pen/Brush 能否替换每次创建？
  │ - Method group dispatcher callback 能否替换 lambda？
  │ - 已有的 utility (MathUtils 等) 能否替换手写代码？
  │
  ├── 是 → 走该路径，无需自定义代码
  │
  └── 否 → Step 4
  │
  ▼
Step 4. 自定义代码的复杂度成本（参见 Tier 1 §5 + Process Gate 3）
  │ - 触发 Theme Static Rule？(默认禁止，例外 1/2/3 之外的拒绝)
  │ - 新增 Ensure/Clear 链？
  │ - 新增 disposable / 标志位？
  │ - 复杂度增长是否超过 Gate 3 阈值？
  │
  ├── 是 → 重新评估收益是否值得复杂度，不值得就放弃
  │
  └── 否 → Step 5
  │
  ▼
Step 5. 实现 + 测量 + Gallery 矩阵
  │ - 不通过 Tier 1 任一边界 = 回退
  │ - 3 轮 (Tier 1 §9) 没改善 = 回退
  │ - 5 控件批量 (Tier 1 §10) 触发 audit
  │ - 通过 Process Gates → 准备 commit
```

---

## First-60-Minutes Investigation Playbook

This is the operational form of the Decision Tree. Walk it in order whenever a "X is slow" report comes in. Stop early at any step that disqualifies the optimization — most reports stop at Step 1 or Step 2.

### Step 1 (0–5 min) — Qualification gate

Three yes/no questions. Any "no" closes the request without further investigation.

- Does the control instantiate > 5 times in any realistic Gallery ShowCase that ships? (Open the corresponding `<Control>ShowCase` in `controlgallery/AtomUIGallery.Desktop` and count.)
- Does the user trigger the operation > 1 time per realistic session? "Page first navigation" counts as 1.
- Is there a numeric report? "Feels slow" without numbers means we don't know what we're optimizing.

If any answer is no, the answer is: "this control does not qualify for optimization — Tier 1 §13". Record the numbers, close the request.

### Step 2 (5–15 min) — Reproduce in Gallery

```bash
dotnet run --project controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj
```

- Navigate to the relevant ShowCase. Confirm visually that the slow operation reproduces.
- Open DevTools (F12). Use the Visual Tree pane to count realized visuals and confirm which presenters/popups/items are alive.
- Reproduce the slow action with DevTools focused; record the layout-pass and render-tick visible behavior. If the visual tree count is much higher than expected for the visible content, the bottleneck is structural — note this and continue.

### Step 3 (15–30 min) — Capture baseline numbers

Control-level baseline (low noise, single-control or small composition):

```bash
dotnet build tools/performances/AtomUI.Performance/AtomUI.Performance.csproj -c Debug --framework net10.0

dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --suite <control-key> --count 60 \
  --markdown /tmp/<control>-control-baseline.md
```

Find `<control-key>` in `tools/performances/README.md` ("当前命令" 节), or look under `tools/performances/AtomUI.Performance/Suites/<Control>/` for an existing suite. If none exists, create the smallest representative suite first; building it is part of the work, not a precondition that blocks measurement.

State / lifecycle verification, when the control has it:

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --verify-<control>-states
```

Gallery-level baseline (real ShowCase, includes page-level fixed costs):

```bash
dotnet build tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj -c Debug --framework net10.0

dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase <showcase-key> --label baseline \
  --cold-iterations 10 --iterations 30 --warmup 5 --timeout-ms 30000 \
  --markdown /tmp/<control>-showcase-baseline.md
```

Sample policy: `--cold-iterations 10` is the minimum for cold-first-navigation conclusions. Single-process samples are smoke-only and may NOT be reported as proof of improvement. Same `--warmup`/`--iterations`/`--cold-iterations` before and after, no exceptions.

Allocation / GC observation while interacting (optional, runs against the live Gallery process):

```bash
dotnet-counters monitor -p <gallery-pid> --counters System.Runtime
```

Watch `gen-0/1/2-gc-count`, `alloc-rate`, `gc-heap-size`. An allocation drop without a time drop is still a win, but only if you can name which allocations were removed.

### Step 4 (30–45 min) — Localize the cost

Match findings to the Cost Model. Which subsystem dominates the measured time / allocation?

- **Layout dominates** → which `MeasureOverride` runs, how many times per pass? Is `IsVisible=False` short-circuit eligible (`Avalonia.Base/Layout/Layoutable.cs:546`, `:671`)? Is a `Grid` doing 4-pass star resolution? Reparenting in a hot path?
- **Binding dominates** → are template bindings going through `{Binding RelativeSource=...}` instead of `{TemplateBinding}` (`Avalonia.Base/Data/TemplateBindingExpression.cs:37-43`)? Are there `BindUtils.RelayBind` calls where `[!]` would do? `$parent[T]` recomputation in a re-attach loop?
- **Style / Selector dominates** → any selector with a high re-evaluation frequency dependency (`^[X=true]:pointerover` chained)? `BasedOn` chain ≥ 4 levels (`Avalonia.Base/StyledElement.cs:776-793`)?
- **Event dominates** → any high-frequency `RaiseEvent` whose route depth or handler count is large (`Avalonia.Base/Interactivity/Interactive.cs:143-177`)? Class handler vs per-instance handler usage?
- **Render dominates** → `new SolidColorBrush(...)` per render (`Avalonia.Base/Media/SolidColorBrush.cs:13-95`)? `StreamGeometry.Parse(string)` repeated (`Avalonia.Base/Media/StreamGeometry.cs:34-44`)? `Width`/`Height` animation pulling layout pass?
- **Popup dominates** → window-host vs overlay-host (`Avalonia.Controls/Primitives/Popup.cs:449`)? Heavy popup content created at default closed state (Popup Lazy Content Rule)?

Use `BindingDiagnostics.IsLoggingEnabled = true` in dev build to surface binding failures masquerading as perf problems.

If none of the above lights up, the bottleneck may be Gallery-level page setup or another control on the same page. Re-run the Gallery baseline with the suspect control replaced by an empty placeholder; if the time stays the same, the original control is innocent.

### Step 5 (45–60 min) — Decide and write up

- `cost × frequency ≥ 1 ms / frame budget` OR `≥ 5 % of measured ShowCase time` → optimization candidate, proceed to Decision Tree Step 3.
- Otherwise: decline, with the numbers in the report.
- Either way, the conclusion + numbers go into `docs/superpowers/progress/<date>-<control>-performance.md` so the next investigation does not repeat the work.

### Outputs of this playbook (mandatory before writing any optimization code)

1. The control's qualification numbers (instance count, per-session frequency).
2. Cold first navigation + repeated mean / median / P95 from the Gallery baseline.
3. The dominant cost subsystem identified, with `path:line` reference.
4. The hypothesis to be falsified (Decision Tree Step 2 line "假设证伪点").

Without all four, you have not earned the right to write code. Tier 1 §11 (no unsupported claims) and §13 (qualification documented) both gate here.

---

## Process Gates

Mandatory commit-time gates. A perf commit description without all of them filled in fails review.

### Gate 0 — 框架子系统 + 成本量级（必填）

```
[ ] 被优化的成本属于哪个框架子系统？
    (Property / Binding / Layout / Render / Event / Popup / Dispatcher / Style / Animation)
[ ] 在该子系统的 Cost Model 中，当前实现证据或可复现测量：
    ___________________
[ ] 该子系统的单位成本量级（来自 Cost Model）：______
[ ] 实际触发频率（来自测量，不许估）：______
[ ] 总成本占比（≥ 5% 才进入下一步）：______
[ ] 假设证伪点（如果实测低于此量级，优化应放弃）：______
```

如果某条引用还在 `[VERIFY]` 阶段，说明该子系统认知不足以支持优化决策。先在本 Skill 的 Cost Model 中补全实现证据、AtomUI 影响和测量门槛，再继续优化。

### Gate 1 — 正确性自证

```
[ ] 行为/外观/动画/交互/主题/API 全部不变？
    验证方式: ___________________
[ ] 通过 per-control regression matrix (Suites/<Control>/Regression.md)。
    跑过的 case: ___________________
[ ] Gallery ShowCase 走查完成，覆盖控件: ___________________
```

### Gate 2 — 资源生命周期

```
[ ] 列出本次新增的: subscription / binding / timer / lazy materialized object /
    cached value / reparented element
[ ] 每一项的释放触发器已识别且验证。
[ ] 粘贴 "Resource Leak Detection" 章节 grep 命令的输出。
```

### Gate 3 — 复杂度自评

```
[ ] 新增 Ensure*/Clear*/Sync* 方法 N 个 (阈值 ≥ 4 = 需要论证)
[ ] 新增 try/finally 标志位 M 个 (阈值 ≥ 2 = 需要论证)
[ ] 同一文件新增 disposable 字段 K 个 (阈值 ≥ 3 = 需要论证)
[ ] axaml 删除行数 vs C# 增加行数: ___ vs ___
[ ] Theme Static Rule 检查: 是否把 axaml 节点搬进 C#？Y/N
    若 Y，对应三类例外的哪一类？___________________
```

任一阈值超出必须显式论证（不接受"为了性能"作为理由）。

### Gate 4 — 测量

```
[ ] cold-first-navigation: 至少 --cold-iterations 10，前后数字均有
[ ] repeated mean / median / P95: 同 warmup+iterations 跑前后
[ ] 单进程/单 sample 数据明确标记为 smoke-only，不作为最终结论
[ ] 测量场景对应的 Gallery ShowCase: ___________________
```

### Gate 5 — 回退路径

```
[ ] 一行 git command 能从 merged tip 回退:
    git revert <sha>   或   git checkout <sha>~1 -- <files>
[ ] 改动文件范围 + 文件数: ___ 个文件 / ___ 个目录
[ ] 范围足够小，回退是机械操作。
```

### Gate 6 — 构建 / lint

```
[ ] 拥有项目构建通过（无新增 warning）
[ ] git diff --check 干净
[ ] 没有遗留未使用的 using 指令
```

### Gate 7 — 用户收益汇报

Every completed optimization must include a user-facing benefit table in the final response. Do not wait for the user to ask "汇报收益".

```
[ ] 先给一句话结论，用直观语言说明「减少了什么 / 少了多少 / 哪个用户场景受益」
[ ] 最终回复包含收益表，列为: metric / baseline / optimized / formula / improvement / conclusion
[ ] metric 使用用户能看懂的名称和单位，不只写内部类名、方法名、callsite count
[ ] conclusion 说明影响场景，例如每次关闭、每行生成、每个 DataGrid、Gallery 页面加载
[ ] baseline 与 optimized 的口径一致；若是结构收益，单位写清楚（per control / per item / per root / Gallery estimated total）
[ ] 百分比使用 `(baseline - optimized) / baseline`；对 "removed" 类指标也给出百分比
[ ] 结构收益必须翻译成直观说法，例如「每次操作少创建 X 个对象」「每个实例少 Y 个订阅」「每次 arrange 少 Z 个 Geometry」
[ ] timing 数据若只是单次 smoke，明确标记 smoke-only，不当作确定性能提升
[ ] smoke-only timing 单独标明，不混入确定收益；结构优化不能用单次 timing 包装成确定速度提升
[ ] 若没有可靠 timing before/after，明确说明不声明 timing 百分比，只声明结构/分配/正确性收益
[ ] 正确性修复、验证命令、未能验证的残余风险一并汇报
```

---

## Per-Control Regression Matrix

Each control with non-trivial behavior must maintain a matrix at `tools/performances/AtomUI.Performance/Suites/<Control>/Regression.md`. Every perf commit on that control must declare which matrix entries it ran.

Minimal matrix shape:

```
# <Control> Regression Matrix

## Functional matrix (must run before any perf commit on this control)
- [ ] <feature 1 + interaction>
- [ ] <feature 2 + interaction>
- [ ] ...

## Multi-step user flows (Gallery ShowCase scripts)
- [ ] <ShowCase A>: open → step 1 → step 2 → expected outcome
- [ ] <ShowCase B>: ...

## Lifecycle matrix
- [ ] Mount → unmount → re-mount with state preserved
- [ ] Re-template
- [ ] Property toggle on/off/on
```

`<Control>StateVerification.cs` cases must map 1:1 with matrix items where automatable. Items that can only be verified by visual inspection (animation, hover transition, popup arrow position) must be listed and the reviewer must walk them in Gallery, with timestamps in the commit message.

---

## Pattern Rollout Budget

When a single optimization pattern is being applied across multiple controls, this is a **series planning concern**, not just a per-commit one.

- Apply the pattern to at most **4 controls** before a mandatory review.
- After 4 commits with the same pattern, halt and answer:
  1. Did at least 3 of 4 show measurable speed gain?
  2. Did any one introduce ≥ 2 Gallery-visible bugs that took > 30 minutes to discover?
  3. Did any one expand complexity beyond the Gate 3 thresholds?
- If 1 fails or 2/3 succeeds, do not roll out to the 5th control. Audit existing 4, decide which to keep / refine / revert.
- Decisions must be recorded in `docs/superpowers/progress/<date>-<pattern>-rollout-audit.md`.

---

## Avalonia Binding Priority Guardrails

### Rules

- **Do not use `IsSet(property)` as "user explicitly set this".** In Avalonia, `IsSet` returns true for any effective styled value or binding (Template/Style/Animation/LocalValue alike). Using it as a "user wrote this" check causes silent default restoration failures.
- **Internal default/token bindings must not share priority with expected external bindings on the same property.** Avalonia's immediate value frames are priority-based and property-keyed; disposing an entry at the same priority can remove or disturb another binding on the same property.
- **For internal defaults that override template-optional values in normal modes but yield in a special mode**, use a stronger separate priority and dispose it at the mode boundary.
- **State matrix must be exhaustive.** Whenever a property has both internal and external bindings, verify: default mode, every named mode, special/Custom mode, switching out of Custom, and source updates after switching.
- **`LocalValue` and `Animation` must always be stronger than internal defaults.** If user sets a local value, internal token binding must be disposed or not observed as the effective value.

### Incident: Space `ItemSpacing` / `LineSpacing`

Space optimization changed internal token spacing bindings without fully modeling the Gallery example:

```xml
<atom:Space SizeType="{Binding SizeType}"
            LineSpacing="{Binding #CustomSizeSlider.Value, Priority=Template}"
            ItemSpacing="{Binding #CustomSizeSlider.Value, Priority=Template}" />
```

First fix moved internal bindings to `BindingPriority.Style`, avoiding same-priority disposal collisions but creating a new bug: the always-present slider `Template` binding won in `Small`/`Middle`/`Large`, so those SizeType options stopped changing spacing. Only `Custom` still worked, masking the bug unless the full interaction was tested.

**Lesson:** Whenever you touch a property that has both internal default and external override, write down the priority of every binding on that property before changing anything.

---

## Re-entrancy & Ignore-Flag Guardrails

A private `bool` that short-circuits a property-changed handler is the single most reliable indicator that the event flow is wrong. The pattern surfaced repeatedly in the rolled-back Pattern A commits (`_ignoreSelectedPropertyChanged`, `IgnorePropertyChange = true`, `IsPlayingCloseMotion`-driven cancels). Each occurrence masked a real ordering bug; removing the flag during rollback exposed the bug, and fixing the flow — not the flag — solved it.

### Rule

When a property-changed handler is about to set another property whose own handler will set the first property again, do NOT add an ignore flag. Walk the loop and break it at the right link.

### Acceptable cases (the only ones)

- **Framework-provided guards.** Avalonia / WPF-style two-way binding already guards itself; you don't need a second flag on top.
- **Explicit batch-update markers around a known block.** `BeginUpdate()` / `EndUpdate()` style, where the suppression is bounded by code structure (a `using` scope, a `try/finally`), not by a `bool` field that lives between events.
- **State machines with documented exit conditions.** A `bool` is acceptable only when both entry and exit are deterministic (set in path A, cleared by exactly one event in path B that is guaranteed to run). The Cascader incident proved that "cleared in `OnSomethingFinished`" is not deterministic enough — async paths skip it.

### How to remove an ignore flag without breaking the chain

| Loop shape | Fix |
| --- | --- |
| User sets A → handler-of-A sets B → handler-of-B sets A again | The second `SetValue` should be `SetCurrentValue` (preserves binding source, no priority bump). Or the second handler should `if (!equals) Set...`. Both break the loop without a flag. |
| Internal token binding writes default → user binding overrides → internal writes again | Move internal binding to a different `BindingPriority` frame. See [Avalonia Binding Priority Guardrails](#avalonia-binding-priority-guardrails). |
| Pre-state must be synchronized before the visible change | Split into a dedicated `IObservable` / pre-event so order is explicit. The original change becomes "publish pre, then change", not "change with a flag-guard around the recursion". |
| Animation cancel recursing into open/close | Animation cancel should `complete` (or `Stop` without raising completion), not write the IsOpen property again. If the animation API forces a write-back, factor open/close into a small state machine with named transitions, then drive transitions from one place. |

### Reviewer must check

Grep the diff for new `_ignore`, `_suppress`, `IgnorePropertyChange`, `_isUpdating`, `_isHandling`, `IsPlayingCloseMotion`-style fields. Each one is presumed to violate this rule until its commit description explains which acceptable case it falls under. "We've always had similar flags" is not an explanation.

### Past incidents

- **Cascader `_ignoreSelectedPropertyChanged` deadlock.** A flag added to "stop the recursion" was set in path A but never cleared on async path B's exit. The control ended up permanently muted; selection didn't sync, popup couldn't reopen, leaf clicks didn't close.
- **AbstractSelect `IsPlayingCloseMotion` + `IgnorePropertyChange = true` cancel.** Together they caused all popup-bearing controls to fail to reopen reliably. The rollback restored the older event flow that did not need either flag.

---

## Required Verification Pattern

For any control performance optimization:

1. Build the changed project or the smallest owning project.
2. Update `tools/performances/AtomUI.Performance/Suites/<Control>/Regression.md` if the matrix needs new entries.
3. Add or update `<Control>StateVerification.cs` with cleanup assertions for any visuals/presenters/hosts/subscriptions/bindings created.
4. If Gallery-visible: build `controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj`, walk the matrix's "Multi-step user flows" entries by hand, record results.
5. For controls with a Gallery ShowCase, run the real ShowCase loading-time measurement and report before/after.
6. `git diff --check`; ensure no unused `using`.

---

## Performance Report Format

Performance summaries must be readable to a human reviewer.

- Compact comparison table with `Scenario`, `Before`, `After`, `Improvement`, units in every value.
- Both control-level and Gallery-level results when both were measured.
- Always include corresponding ShowCase loading-time comparison: cold first navigation (≥10 independent process samples), repeated mean / median / P95, sample policy (warmup/iterations).
- Same sample policy before and after. If first pass shows regression, rerun with sufficient warmup/iterations to distinguish real regression from measurement noise before reporting final.
- Plain-language user-facing impact, e.g., "ComboBoxShowCase repeated open went from 109ms to 91ms, ~18ms faster."
- Structural wins separately when they explain the result, e.g., "Button/IconButton count: 23 → 0", "visual nodes: 562 → 497."
- **Mandatory complexity-burden line.** e.g., "Cascader refactor: cold first nav 87 → 73 ms (-16%); +12 methods / +4 disposable fields / +2 try/finally flags; axaml -38 lines / C# +422 lines."
- State whether the result matches the optimization goal. Call out fixed costs that remain when page-level percentage is lower than control-level percentage.

---

## Lifecycle Pairing Checklist

Before writing any C# code that creates / subscribes / captures, list the pair upfront. Every left-hand call must have a defined right-hand call AND a definite event that fires it. The Resource Leak Detection grep below is the post-hoc audit; this is the up-front design step.

| Create / Acquire | Release | Where the release fires |
| --- | --- | --- |
| `_handler = Handle...; foo.Bar += _handler;` | `foo.Bar -= _handler;` | `OnDetachedFromVisualTree`. For lazy materialization paths, the symmetric `Clear*`. |
| `child.SetTemplatedParent(this); Children.Add(child);` | `Children.Remove(child); child.SetTemplatedParent(null);` | Symmetric `Clear*` when lazy; otherwise `OnDetachedFromVisualTree`. |
| `_disposables ??= new(); _disposables.Add(target.Bind(...));` | `_disposables.Dispose(); _disposables = null;` | `OnDetachedFromVisualTree`. Re-templating: also at the top of the next `OnApplyTemplate`. |
| `pointer.Capture(this);` | `pointer.Capture(null);` | The terminating gesture event (`PointerReleased`, `PointerCaptureLost`). Never on a timer. |
| `_timer = new DispatcherTimer(...); _timer.Tick += ...; _timer.Start();` | `_timer.Stop(); _timer.Tick -= ...; _timer = null;` | `OnDetachedFromVisualTree` AND any explicit "stop" path. |
| `_topLevel.Deactivated += ...;` (any TopLevel / Window-scoped subscription) | matching `-=` | `OnDetachedFromVisualTree`. The TopLevel can outlive the control by far. |
| `TransitionInitializationSession` / `TransitionSuppressionScope` | Commit a complete loaded baseline / dispose the owned override | Acquire before initial attached-tree writes; reset before detach, template replacement or container cleanup. Runtime snapping uses a separate `using` scope; restore the latest underlying collection and bindings. |
| `_popupHost.Open();` | `_popupHost.Close();` | Detach, re-template, or explicit close. The popup host is a separate visual root and will not auto-close on owner detach unless told. |

### Required practice

- **Decide the release event before writing the create call.** "I'll add cleanup later" is the leak.
- **Symmetric `Clear*` is mandatory for lazy materialization.** Anything an `EnsureXxx()` creates, `ClearXxx()` must release in reverse order. If the creation order is `bind A → wire event B → add child C`, the clear order is `remove C → unwire B → dispose A`.
- **`OnDetachedFromVisualTree` is the catch-all release point.** Any subscription that is global / TopLevel-scoped / cross-control MUST release here regardless of any other `Clear*` path. `Clear*` covers the lazy-materialization case; detach covers the "control was attached, control is gone" case. Both must be wired.
- **Re-template safety.** A control may be re-templated mid-life. PART_-discovered references and per-template subscriptions belong in a per-template `CompositeDisposable`, disposed at the START of the next `OnApplyTemplate` and at `OnDetachedFromVisualTree`. Storing them as plain fields and forgetting on re-template is a known leak shape.

### How this connects to Tier 1 §3

Tier 1 §3 says "anything created/subscribed/bound/cached/lazily materialized/reparented must have a defined and verified release path before the change is considered complete". This table is the operational form. The Resource Leak Detection grep is the audit form.

---

## Resource Leak Detection

Run before every perf commit and paste results in Gate 2. Any non-empty output requires explanation.

```bash
# Newly added Ensure*/Clear* must be paired
rg "Ensure[A-Z]\w+\(\)" --type cs -A 3 src/

# Each += event subscription needs matching -=
rg "\\+= Handle" --type cs src/

# CompositeDisposable usage — confirm each one is required by lifetime mismatch
rg "CompositeDisposable\\b" --type cs src/

# Children.Add / Insert without matching Remove
rg "Children\\.(Add|Insert)" --type cs src/

# Disposable fields — each must have a release path
rg "_[a-z]\\w+Disposables\\b" --type cs src/

# SetTemplatedParent(this) calls — each should have a matching SetTemplatedParent(null)
rg "SetTemplatedParent\\(this\\)" --type cs src/

# Legacy private toggles: classify process exceptions; do not copy into new state targets
rg "DisableTransitions\\(\\)|EnableTransitions" --type cs src/
```

---

## Measurement Toolkit

Use these to ground claims in numbers, not guesses.

### Layout / Render

- `LayoutManager.LayoutUpdated` event count: how many layout passes a user action triggered.
- `Renderer.SceneInvalidated` count: invalidate calls reaching the render thread.
- `Avalonia.Diagnostics.DevTools` (F12 in dev): visual tree, property values, binding state, layout overlay.

### Property / Binding

- Add temporary counters in `OnPropertyChanged` to measure real change frequency.
- `BindingDiagnostics.IsLoggingEnabled = true` (in dev builds) for binding failure warnings.

### Allocation / GC

- `dotnet-counters monitor -p <pid>`: real-time GC / allocation rate.
- BenchmarkDotNet for micro-benchmarks; AtomUI.Performance has the harness.

### Visual Tree Size

- `var count = 0; visual.VisitDescendants(_ => count++);` for node count.
- AtomUI.Performance has ShowCase walking utility — use it instead of writing your own.

### Decision principle

Do not report "assumed" cost savings. Every cost claim must include:

1. Measurement tool used.
2. Scenario (which ShowCase / which interaction).
3. Before / after numbers + units.
4. ≥ 10 cold samples + repeated mean/median/P95.

---

## Binding / Subscription Checklist

- Treat leak scanning as mandatory during analysis, not optional cleanup.
- Before using `BindUtils.RelayBind(...)`, storing an `IDisposable` binding, or adding a `CompositeDisposable`: first confirm Avalonia `[!]` binding is insufficient (i.e., source/target lifetimes differ).
- Disposable plumbing is reserved for: mismatched lifetimes, replacement paths, detach/re-template cleanup, global/window subscriptions, timers, event handlers, lazily materialized objects that can outlive their creator.
- Store every disposable subscription/binding created outside XAML.
- Dispose old bindings before replacing.
- Avoid duplicate bindings on repeated property changes.
- If a mode disables a feature, detach visuals and dispose subscriptions in that mode.
- For created visuals/presenters/popups/timers/event handlers/property observables/bindings/caches/global subscriptions: identify owner and release trigger before editing.
- Add a regression test toggling the feature on/off/on.

---

## Style & Best Practice

- Reuse `AtomUI.Utils.MathUtils` (`AreClose`, `IsZero`, `GreaterThan`, `LessThanOrClose`, etc.) for floating-point comparison, epsilon checks, zero/one checks, ordering with tolerance, angle conversion, fixed-point rounding. Do not introduce hand-written `Math.Abs(...) < eps`, new epsilon constants, or duplicate helpers.
- State initialization follows [the shared architecture](../../../docs/architecture/systems/control-infrastructure/transition-initialization.md): identify actual targets and complete baselines; do not use `Dispatcher.Post` to guess readiness. Keep `Transitions` in ControlTheme style setters. Test real parent/container preparation, remount and the first subsequent change; standalone preselected containers cannot prove initialization correctness. Loading, progress and explicit entrance animations retain separate contracts.
- Never `Debug.Assert(value != null)` immediately followed by a nullable guard for the same value. Express the invariant in the type, helper return value, or use a real runtime guard with explicit recovery.
- Remove unused `using` directives introduced by the change.
- Prefer no API change. AtomUI has no formal release yet; API changes need explicit justification.
- Do not commit during perf optimization unless the user explicitly asks. Leave changes in working tree for review by default.

---

## Incident Callouts

Real bugs that cost the project significant rollback effort. New incidents must be appended here with a one-paragraph summary and a "what to check next time" line.

### Space ItemSpacing / LineSpacing (binding priority)

See [Avalonia Binding Priority Guardrails — Incident](#incident-space-itemspacing--linespacing) above. Lesson: when changing token-default bindings, write down all bindings on that property and their priorities before editing.

### Cascader / Select / AddOnDecoratedBox 50+ commit rollback

A series of perf commits applied "theme element → C# dynamic creation" pattern (Pattern A) across ~50 controls. Side effects:

- **Cascader 5-bug cascade.** Single-mode first selection didn't sync to input box; popup couldn't reopen; async load was prematurely closed; clicking already-selected leaf didn't close popup; `_ignoreSelectedPropertyChanged` flag deadlocked.
- **OptionsSource churn.** `_cascaderView.OptionsSource = Options.Cast<...>().ToList()` on every property change triggered Remove notifications on the underlying `_options` collection, silently wiping `SelectedOptions` in multi mode. User saw filter input keystrokes erase their tag selections.
- **AbstractSelect popup strong-sync deadlock.** Adding `IsPlayingCloseMotion` cancel + `IgnorePropertyChange = true; SetCurrentValue(IsDropDownOpenProperty, false)` made all popup controls fail to reopen reliably.
- **InfoPickerInput popup arrow flip lost.** Replacing `IsHorizontalFlipped="{Binding $parent[atom:Popup].IsHorizontalFlipped}"` with a C# `SetCurrentValue` chain broke the arrow-tracks-active-input behavior in `IsShowTime` mode.
- **SelectCandidateList filter selection.** Using `GlobalIndexFromContainer` instead of `IndexFromContainer + Items[index]` caused clicks under filter to select the wrong item.
- **ListBox OnLoaded filter re-apply lost.** Buried in a perf commit that also added a static default filter; revert dropped the OnLoaded re-apply, breaking AutoComplete/Mentions popup re-open with stale filter state.

The rollback ultimately squashed 50+ commits into a single revert and restored the OLD axaml-based architecture. **Lesson:** Pattern A's complexity cost (manual SetCurrentValue chains, disposable bookkeeping, race-prone state machines) consistently outweighed its allocation savings. This is the original justification for the [Theme Static Rule](#theme-static-rule).

### What to check next time

When evaluating a "looks like Pattern A" perf proposal, immediately ask:

1. Can `IsVisible="False"` + style selector achieve the same conditional-cost outcome? (`Avalonia.Base/Layout/Layoutable.cs:546`, `:671` confirm it's free.)
2. Is the per-instance saving meaningful (e.g. ≥ 1 KB / instance, full popup subtree, real `ItemsControl` realization), or only a few hundred bytes / one Visual instance? Past rollback evidence: a few hundred bytes per item never paid for the complexity churn.
3. Does the saved cost recur per open/close (worth optimizing) or only once at startup (not worth)?
4. Will any TemplateBinding chain need to be replaced with a manual C# property change handler? Each handler is a future regression source.
5. If the new C# child needs to receive `/template/` style rules, are you ready to call `SetTemplatedParent(this)` on creation and `SetTemplatedParent(null)` on teardown? `TemplateSelector.Evaluate` returns `NeverThisInstance` without it (`Avalonia.Base/Styling/TemplateSelector.cs:39-49`).

If (1) is yes, or (2) is "only a few hundred bytes", or (3) is "only once", or (4) is "yes, multiple handlers", or (5) is "we forgot about that" — the optimization should not be Pattern A.
