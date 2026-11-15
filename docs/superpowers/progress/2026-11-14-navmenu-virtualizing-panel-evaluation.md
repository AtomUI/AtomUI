# NavMenu Virtualizing ItemsPanel Evaluation

## Question

During the AtomUI/AtomUI#541 virtualization keyboard audit, an inline NavMenu with an app-set
`VirtualizingStackPanel` could not navigate past its realized range: ↓ cycled through about five realized items
(1, 2, 3, 4, 0, 1, ...). The control documentation already records non-virtualizing ItemsPanels as a NavMenu
compatibility invariant. This evaluation decides whether NavMenu should instead support a virtualizing ItemsPanel for
very long menus.

**Decision: do not support it.** The boundary stays as documented. Default panels stay `StackPanel`. A virtualizing
ItemsPanel set by an app is unsupported and is not an extension point. The evidence follows.

## Evidence

All probes ran headless (`AtomUI.Desktop.Controls.Tests`, Avalonia 12.1.6, Debug, macOS arm64), with motion disabled
and a 240×300 menu inside the audit host's overlay-enabled window. Timings come from one serial run, so treat them as
orders of magnitude, not benchmarks.

### Container realization is already lazy by level

With 20 top-level nodes × 30 children each (620 nodes), the realized `NavMenuItem` count after first layout is
**20 in Inline, Vertical and Horizontal**. A closed submenu has no child containers. Opening one submenu realizes only
that level: 100 × 20 went from 100 to 120 containers in 33–49 ms. A hierarchical menu therefore pays only for
its top-level entries plus the levels the user has opened, which is the shape Ant Design menus are meant to have.

### Cost of a fully realized flat level (default StackPanel)

| Flat top-level entries | Show (ms) | Allocated (MB) | Containers |
| ---: | ---: | ---: | ---: |
| 100 | 122 | 29 | 100 |
| 500 | 589 | 183 | 500 |
| 1000 | 2395 | 450 | 1000 |
| 2000 | 3798 | 1226 | 2000 |
| 300 × 20 children (6300 nodes) | 545 | 104 | 300 |

The same flat menu with a `VirtualizingStackPanel` showed 1000 entries in 215 ms / 157 MB and 2000 entries in
851 ms / 608 MB, with 7 containers realized.

### Most of the remaining large-list cost is not containers

The virtualized 2000-entry case still allocates about 600 MB with only 7 containers. Adding entries one at a time to a
detached menu (`menu.Items.Add` in a loop, no layout) is quadratic: 500 → 54 ms, 1000 → 244 ms, 2000 → 792 ms.
Assigning the same 2000 nodes through `ItemsSource` takes about 1 ms. Each root `ItemsView.CollectionChanged` makes the
root `NavMenuEntryOwnershipCoordinator.Synchronize()` rebuild the whole root-scope occurrence/owner maps, so N single
adds cost O(N²). Even when a virtualizing panel is used, this path dominates a long menu populated item by item.

### What a virtualizing panel breaks besides keyboard navigation

Every subsystem below assumes that each entry container of a presented level exists:

- **Keyboard navigation.** `NavMenuInteractionHandlerBase` builds its sequence from
  `NavMenuSemanticNavigator.EnumerateDirectItems` (`ContainerFromIndex`) and stores the keyboard-active item as a
  `NavMenuItem` reference. A recycled container stays the active one while it shows another node.
- **Open-submenu state.** `IsSubMenuOpen` exists only on the container. Probe: open node 0 inline, scroll to the end
  and back. Node 0 is closed again in a fresh container, so the open state is lost on recycling. Accordion close,
  `CollectOpenInlineNodePaths` (the inline-collapsed open-path cache), `CloseOpenSubmenusPreservingSelection`,
  `FindDeepestOpenItem` and `ClosePinnedForLifecycle` all enumerate realized containers only.
- **Path location.** `DefaultOpenPaths` replay, `RestoreInlineOpenNodePath`, selected-path reveal and pinned-open
  reconciliation walk `FindDirectItem` level by level. Replay of an off-screen `DefaultOpenPaths` target opened
  nothing. Groups are themselves `ItemsControl` containers inside the virtualized panel, so locating a grouped node
  requires realizing the group first and then its child.
- **Group preparation.** `EnsureGroupContainers` iterates `ContainerFromIndex` to apply group templates before layout.
- **Mode, dark style and collapse propagation.** `NavMenuItem` pushes state into its children through
  `EnumerateDirectItems` (about ten call sites in `NavMenuItem.cs`).
- **Inline motion.** An inline submenu animates the height of its child frame. A virtualizing child panel reports an
  estimated extent, so the expand/collapse animation would target an estimate.

Selection is the only subsystem that already survives recycling: `NavMenuSelectionCoordinator.PrepareContainer`
re-projects `IsSelected` / `IsInSelectedPath` from `SelectedItem` on every prepare.

Even with support, virtualization granularity would be coarse. In inline mode an open submenu's children live inside
the parent `NavMenuItem` template, so a root virtualizing panel can recycle only whole top-level entries. An expanded
submenu with many children stays one realized block unless the app also virtualizes each `NavMenuItem` ItemsPanel,
where the motion conflict above applies.

## Cost of support

Real support means moving open state and keyboard-active identity from containers to node identity, then re-projecting
them on prepare as TreeView does after #541. It also needs index stepping through groups with on-demand realization
(`ScrollIntoView` per level), node-level enumeration replacing about 27 `EnumerateDirectItems` / `FindDirectItem` /
`ContainerFromIndex` call sites across `NavMenu.cs`, `NavMenuItem.cs`, `NavMenuInteractionHandlerBase.cs`,
`NavMenuPinnedOpenCoordinator.cs` and `NavMenuSemanticNavigator.cs`, and a motion story for virtualized child panels.
That is a control-wide state-ownership change across three modes plus inline collapsed, popup pinning and accordion.
Each of those already carries detailed invariants and tests.

## Why the benefit does not justify it

- Navigation menus are hierarchical, and NavMenu realizes containers per opened level. A 6300-node menu
  (300 × 20) shows in about 0.5 s with the default panel.
- The case that gains is a flat level with many hundreds of entries. That is a list, not navigation. Ant Design Menu
  does not virtualize either.
- At that size the dominant remaining cost (per-add O(N) entry synchronization) is not addressed by virtualization at
  all.
- TreeView supports virtualizing panels, including keyboard navigation, expansion, selection and checked state after
  #541, for apps that really need a navigation tree of thousands of rows.

## Guidance recorded in the control docs

- `overview.md` §7 keeps the invariant: root, submenu and group ItemsPanels must be non-virtualizing. It adds
  the observed failures and points very long navigation at submenu hierarchy or TreeView.
- `implementation.md` §8 records the per-level realization boundary. §9 records that open state and keyboard-active
  identity are container-owned, so a partial-virtualization assumption must not be introduced.

## Follow-ups (not part of this decision)

- Root entry synchronization on single `Items.Add` is O(root entries), so incremental population is O(N²). Batching
  root reconcile, or an incremental path for a pure Add of built-in entries, would fix it without changing the
  ownership contract.
- Per-container cost is about 1–2 ms and roughly 300 KB of allocation at show time (Debug, headless). That is worth a
  separate container-template profiling pass.

## Probe

The probes were temporary tests next to the #541 audit host
(`tests/AtomUI.Desktop.Controls.Tests/VirtualizationKeyboardAudit/AuditHost.cs` on `audit/virtualization-keyboard`,
commit `ea15cfe5b`). They are not committed. The keyboard failure is reproduced by
`TreeSelectTransferMenuAuditTests.NavMenu_Virtualized_Inline_Down_Walks_Past_Viewport` on that branch.
