# ReactiveUI Windows NativeAOT warning closure

Previous: ReactiveUI.Avalonia12.0.3 (b1fed26cb7fe84fb2799391040660fad7927486f),
ReactiveUI23.2.28 (7c1ab24d9c84ed225c738bdb9f1a2f2586c6a5bc).
Adopted: ReactiveUI.Avalonia12.1.6 (26f23b50cfce6f5095b87bbe7f2391de6d360934),
ReactiveUI26.0.1 (88312688d0281fd260977dfd78bb12e1c85ffaaa), Binding9.1.0
(05c45cec845835d670960fac733bb302bc1c1071), Primitives9.0.0
(56523df08d686aba128e0abef660a855fd7951f3), Splat21.0.0.
These are package repository identities, not inferred release-note versions.

## Evidence and decision

The original detailed Windows Gallery publish emitted14 linker/AOT warnings. Two come
from AvaloniaObjectObservableForProperty's annotation mismatch. RoutedViewHost
uses WhenActivated/WhenAnyValue expression reflection and ResolveView's dynamic
fallback. Retaining ExpressionRewriter exposes its incompatible override
annotations. NoWarn, warning suppression and keeping vulnerable compiler packs
are excluded.

Official Avalonia integration source12.1.6 removes the old property observer and
binds routed navigation through typed Avalonia property observables, Switch and
explicit visual-tree attachment/disposal. ViewHostNavigation uses the registered
view locator. This preserves the RoutedViewHost type and public properties.
Reflection-based entry-assembly registration remains a separately annotated,
opt-in path; AtomUI supplies explicit route factories.

This is a high-risk dependency-family migration, not a patch-only version bump.
The disposable candidate build exposes27 compatibility diagnostics in GalleryBase,
principally RxVoid replacing System.Reactive.Unit and the factory Map overload
moving to Map<TViewModel>(Func<IViewFor>). DynamicData is no longer transitively
available; Cascader uses it only for IList.AddRange.

Decision under the user's explicit delegated authority: preserve the existing
System.Reactive.Unit command API with internal factories supplying explicit
generic input/output types; preserve custom view factories with the supported
Map<TViewModel> overload; replace Cascader's dependency on a transitive list
extension with the equivalent ordered IList.Add operations. Keep all existing
Gallery route, activation, cancellation and command error contracts.

The complete rename-aware reference inventories are adjacent: reactive-changed-files.csv
(2245 paths) and reactive-avalonia-changed-files.csv (233 paths including producer
and validation files). Each path has an impact classification. Their production shared/runtime areas remain potentially affected.
Other platform adapters, examples, producer CI and upstream tests do not enter
the net8/net10 Windows package compilation; this does not certify those
platforms. This evaluation does not claim that every production diff has been
manually reviewed. Impact-specific review covered explicit/generative view dispatch,
Map factory signatures, ViewHostNavigation view-model wiring/disposal, typed
RoutedViewHost property observations and attach/detach subscriptions, command Unit
versus RxVoid factories, scheduler namespaces and dependency assembly identities.
Windows source/build/runtime evidence is below; global compatibility remains distinct.

## Ordered migration and acceptance

- [x] Finish source review of registered factories, commands, activation, routing,
      scheduler/observer and dependency assembly boundaries.
- [x] Update the centrally owned package and add internal Unit adapters.
- [x] Build net8/net10 and run existing command, route, shell, activation and
      Cascader asynchronous-load contracts.
- [x] Publish with uncollapsed warnings, require no original diagnostics and
      normal NuGet vulnerability audit.
- [x] Launch the exact current-source native Gallery binary and exercise routing,
      theme commands and return navigation.
- [x] Update Gallery maintenance/version guidance and final specialist status.

An ordinary focused run must not be relabeled as the policy's global dependency
compatibility receipt. Global full regression is not authorized by the current
task instructions; that distinction must remain explicit until a valid gate is
available.

## Actual selection and behavior evidence

The smaller12.1.0/ReactiveUI24 candidate was published and rejected: ResolveView
still emitted IL2026/IL3050.12.1.6/26.0.1 removes the original diagnostic paths.
No project NoWarn, linker rooting override or source-generator disable was added.

Migration exposed a real routing regression: generated CardViewModel dispatch
selected CardAdvancedShowCase before the registered CardShowCase factory, even
after bounded readiness wait. The per-host GalleryRouteViewLocator fixes that
ordering and retains global named/non-route fallback.

Both net8 and net10 runtime packages build through the official20-package release
owner. Existing focused checks pass: GalleryBase23 and Desktop40 (including the
11 temporary retirement cases, subsequently deleted). Actual managed and serviced
NativeAOT Gallery each traverse78 routes/316 checks/465 actual control types,
including appearance changes retaining page identity, return navigation and
language commands. Native run reports.NET10.0.14 and noCoreCLR.

Public dependency contracts moved IViewFor/IViewLocator/DefaultViewLocator into
ReactiveUI.Binding; fully qualified source and assembly consumers must migrate.
The internal Unit adapter preserves AtomUI command signatures but does not erase
that dependency migration. See the GalleryBase migration document.
