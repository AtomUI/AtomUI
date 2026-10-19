# TreeView Changelog

本文档记录 TreeView 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-19

- Behavior
  - Preserve pre-reset TreeView selection and checked state across `ItemsSource` replay and publish only final `SelectionChanged`, `CheckedItemsChanged` and Form value differences. Consecutive explicit collection edits and assignments made before replay completes take precedence; a nested source replacement takes over the pending replay; a filter failure clears the replay transaction.

## 2026-10-03

- Implementation
  - Establish drag state synchronously and finish it before completion callbacks. Release previews through their actual visual parent; detach cancels pending drops and ends the drag without queued access to a cleared node.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record TreeView as the semantic owner, with TreeViewFlyout used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-20

- Design
  - 新增 `semantic-part.md`，定义 TreeView Semantic Part 契约：TreeView 是递归层级容器，用 `TreeView` 与
    `TreeViewItem` 两个递归 Semantic owner 表达 `root`、`item`、`itemSwitcher`、`itemIndicator`、`itemIcon`、
    `itemTitle` 六个 Part，对齐 Ant Design 6.6.0 `TreeSemanticType`（`root` / `item` / `itemIcon` / `itemTitle` /
    `itemSwitcher`），并额外公开 AtomUI 扩展 `itemIndicator`（checkbox / radio 勾选指示）。
  - `TreeView` owner 公开 `root`、`item`（顶层容器）；`TreeViewItem` owner 公开 `root`、`item`（子容器）与
    `itemSwitcher` / `itemIndicator` / `itemIcon` / `itemTitle`（节点内容区域）。二者用同一 `.semantic-item` 身份保证
    任意深度节点可达。
  - `TreeViewItemHeader`、`NodeSwitcherButton`、`CheckBoxIndicator`、`FloatableTreeView` 不持有独立 Semantic
    descriptor：`itemSwitcher` 与 `itemIndicator` 的 `ContractType` 使用公开基类 `ToggleButton`。
  - 定义 marker 路由：`.semantic-item`（容器，`> .semantic-item`，`TreeView` 与 `TreeViewItem` 各自声明）、
    `.semantic-scope-header`（internal 跳点）、`.semantic-item-switcher` / `.semantic-item-indicator` /
    `.semantic-item-icon` / `.semantic-item-title`（header 模板静态 marker，route 从 `TreeViewItem` owner 出发）。
    `itemIndicator` 是 checkbox / radio 两个备选节点共用的单一 Part。
- Docs
  - 更新 `overview.md` §3.1 Semantic Part 摘要、§9 LLMS 语义区域映射与验证策略。
  - 更新 `implementation.md` 源码文件结构、§5.1 Semantic Part marker 放置与路由、维护不变量与验证范围。
- Tests
  - 新增 `TreeViewSemanticPartTests`，覆盖两个 descriptor 的 Part 数量/顺序/字段、模板 marker 静态声明、运行时 marker
    数量、`TreeView` 顶层 route 与 `TreeViewItem` 递归 route 命中、生成 Style 的 Setter 生效。

## 2026-07-08

- Docs
  - 明确 TreeView 拖拽重排采用数据源驱动设计：UI 容器只负责命中和视觉状态，结构修改由内部数据控制器操作 root 数据源或节点 `Children`。
  - 记录轻量 `TreeNodeIndex` 的性能边界：drop 定位使用增量索引，拖拽过程中不按节点引用全树扫描。
  - 补充拖拽 move 的状态一致性要求：节点移动不按删除清理选中、勾选和展开状态，跨父级移动必须同步 parent node、权威集合和索引。

## 2026-07-05

- Implementation
  - 修复多选 TreeView 在 ItemsSource 变化或首次容器状态回放时优先恢复 `SelectedItem`，导致 `SelectedItems` 被折叠成单选的问题；多选模式现在以 `SelectedItems` 路径作为运行期选择恢复的权威来源。
  - 修复 `IFormItemAware.SetFormValue` 写入 TreeView 选择值时把节点对象转换为字符串的问题；单选保持 `SelectedItem` 节点实例，多选保持 `SelectedItems` 列表实例。
- Gallery
  - 新增 `SelectedItem` / `SelectedItems` 绑定示例，并标记 `v6.0.8`。
- Tests
  - 新增 TreeView 多选 ItemsSource 回放优先级测试，覆盖 `SelectedItem` 与 `SelectedItems` 同时存在时的恢复顺序。
  - 新增 TreeView Form 值读写回归测试，覆盖单选、多选、读取和清空语义。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `TreeView`.
  - Align generated output paths with `controls/tree-view/index-cn.md` and `controls/tree-view/semantic-cn.md`.

## 2026-06-22

- Docs
  - 补充 TreeView 节点模型分层设计，明确 `TreeItemNode` 保持轻量 POCO / record 定位，不直接改造成 `AvaloniaObject`。
  - 记录 `BindableTreeItemNode` 绑定型节点设计边界，用于 XAML binding target、`DynamicResource` 和动态资源场景。
  - 补充绑定型节点的 scoped resource host、owner attach/release、属性同步和容器生命周期约束。
- Implementation
  - 新增 `BindableTreeItemNode`，通过 Avalonia DirectProperty 承载节点属性，并使用 scoped resource host generator 管理动态资源宿主。
  - TreeView / TreeViewItem 生成容器时接入绑定型节点的资源宿主 attach、属性同步、checked / selected / expanded 回写和容器 clear 释放。

## 2026-06-19

- Docs
  - 新增 TreeView 桌面版架构设计文档，记录控件定位、公共契约、行为状态模型、视觉主题模型、兼容性不变量和专项模型。
  - 新增 TreeView 桌面版实现原理文档，记录源码职责、默认状态回放、路径遍历、勾选同步、过滤、异步加载、拖拽和维护不变量。
  - 新增 TreeView Token 设计文档，记录节点尺寸、状态色、结构间距、拖拽和过滤 Token 边界。
  - 在 Data Display 分类入口中登记 TreeView 文档。
