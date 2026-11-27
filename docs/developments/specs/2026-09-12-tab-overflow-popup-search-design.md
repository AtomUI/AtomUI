# TabControl / TabStrip Overflow Popup Search 设计

**状态：** 已实现；验证证据以 [2026-09-15 源码审查记录](../progress/2026-09-15-tab-overflow-review.md) 为准。早期性能 probe 的异常吞掉路径已撤销，不能作为验收依据。

**分支：** `codex/tab-overflow-popup-search`

**日期：** 2026-09-12

## 1. 目标

为 `TabControl`、`CardTabControl`、`TabStrip` 与 `CardTabStrip` 建立统一的溢出弹层自定义模型，并在 Gallery 中提供
与 Ant Design Tabs `popupRender` 搜索示例一致的视觉和交互体验。实现同时重构现有溢出路径，使重复打开、关闭和
导航显著减少对象创建、事件订阅与调度成本，且任何关闭、重套模板、detach 与页面导航都不保留旧 owner 或数据。

本设计不承担旧 overflow internal 类型、主题 key 或测试反射入口的兼容。Tab 选择、关闭、拖动排序、add button、
TabItem Theme 和非 overflow 滚动语义保持在现有 owner 中。

## 2. 研究依据

### 2.1 Ant Design

按仓库规定的本地优先顺序，参考源码采用：

- 本地路径：`/Users/chinboy/Projects/ReferenceProjects/ant-design`
- 检查 revision：`3bc029ad8f28bc03be737eeb0633b7bccfeb3038`
- 描述：`6.6.1-104-g3bc029ad8f`
- 主要证据：`components/tabs/demo/popupRender-Search.tsx`、Tabs `MoreProps.popupRender` API 文档

经验证的交互与视觉输入：

- 自定义入口接收 overflow/rest tab 列表与关闭 action，而不是接管 Tabs owner。
- demo 使用 click trigger 与 `bottomLeft` placement；搜索容器宽 `200px`，列表最大高度 `300px`。
- 输入包含 Search icon、clear 与 `Search tabs...`；上下方向键把焦点交给菜单。
- 过滤忽略大小写；选择后清 query、提交 active item 并关闭弹层；空结果显示 `No matching tabs`。

这些证据只用于本次方案取证。AtomUI 长期控件文档只保存重新验证后的自身契约，不把外部 revision、路径或实现细节
作为稳定架构依赖。

### 2.2 AtomUI 当前事实

当前运行时由 `BaseTabScrollViewer` 派生出 `TabControlScrollViewer` 与 `TabStripScrollViewer`，两者各自：

- 每次打开创建一个 `MenuFlyout`。
- 每次打开创建两条 `RelayBind` 和一个 `CompositeDisposable`。
- 为每个 overflow item 创建独立 `MenuItem`，并注册 click 与 close 两个实例 handler。
- 激活时创建一个 `Dispatcher.Post` closure。
- 在关闭时逐项解绑 handler、清空 items、释放 binding 和销毁 Flyout。

`BaseTabControl` / `BaseTabStrip` 还各自维护 pinned-open scroll viewer 与一条 relay。现有测试验证 header template、
closable、统一 owner close 与 pinned detach/reopen，但没有覆盖自定义 popup、重复打开分配、旧 context、模板缓存和
弱引用回收。

2026-09-12 在干净 worktree 的基线命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj \
  --framework net10.0 --filter FullyQualifiedName~Tab --nologo -v:minimal /m:1 /nr:false
```

结果：159 passed，0 failed，0 skipped。

## 3. 最终 Public Contract

两个 base owner 各自注册 `OverflowPopupTemplate`：

```csharp
public static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty;
public IDataTemplate? OverflowPopupTemplate { get; set; }
```

`TabControl`、`CardTabControl`、`TabStrip` 与 `CardTabStrip` 继承该属性。默认 `null` 使用内置菜单；自定义
`IDataTemplate` 的 data item 是：

```csharp
public sealed class TabOverflowPopupContext : INotifyPropertyChanged
{
    public IReadOnlyList<TabOverflowItem> Items { get; }
    public TabOverflowItem? SelectedItem { get; }

    public bool TryActivate(TabOverflowItem item);
    public bool TryClose(TabOverflowItem item);
    public void Dismiss();
}
```

单项为不可变 public projection：

```csharp
public sealed class TabOverflowItem
{
    public object? Item { get; }
    public object? Header { get; }
    public IDataTemplate? HeaderTemplate { get; }
    public bool IsEnabled { get; }
    public bool IsSelected { get; }
    public bool IsClosable { get; }
}
```

Context action 必须验证 item 属于当前打开会话。`TryActivate` 与 `TryClose` 不自动假设 UI 的关闭策略；默认菜单和
Gallery 搜索模板在成功 action 后调用 `Dismiss`。Context 通过 weak action target 到 owner；关闭后清空 Items、
SelectedItem 与 action target，因此只保存旧 context 不能保留控件。

Context 每次发布都替换整个只读列表，不发送增量集合通知；它先更新 `Items` 与 `SelectedItem`，再以 `Items`、
`SelectedItem` 顺序为实际变化的属性触发 `PropertyChanged`。调用方若单独保存 `TabOverflowItem`，只会按普通 CLR
引用语义保留该快照公开的数据值；旧 item 不能操作或通过内部映射保留 owner。

## 4. 统一运行时架构

### 4.1 类型收敛

最终结构只有一套：

```text
BaseTabControl / BaseTabStrip
  implements ITabOverflowOwner
        |
        v
internal sealed TabScrollViewer
  -> Popup#PART_OverflowPopup
     -> cached default TabOverflowMenu or custom template root
```

删除：

- `BaseTabScrollViewer`
- `TabControlScrollViewer`
- `TabStripScrollViewer`
- `BaseOverflowMenuItem`
- `TabControlOverflowMenuItem`
- `TabStripOverflowMenuItem`
- `BaseOverflowMenuItemTheme.axaml`
- 两套动态 `MenuFlyout` 创建、binding disposable、item instance handler 和 dispatcher activation path

`ITabOverflowOwner` 是 internal bridge，只负责 owner-specific container projection、逻辑 item 解析、选择、关闭以及
打开态 collection/selection notification。它不暴露给应用，也不拥有 Popup 视觉。

### 4.2 Template

`BaseTabScrollViewerTheme.axaml` 收敛为 `TabScrollViewerTheme.axaml`。模板保留三个既有 indicator part，并加入静态
`PART_OverflowPopup`。Popup 使用 overlay host、light-dismiss、无箭头；placement 按 Top/Bottom/Left/Right 映射到
既有四个 edge-aligned 模式。

`TabScrollViewer` 通过 internal DirectProperty 向模板输出：

- `IsOverflowPopupOpen`
- `OverflowPopupContext`
- `OverflowPopupTemplate`
- `OverflowPopupPlacement`

Owner theme 用 `TemplateBinding` 把 public template 传入 scroll viewer。Popup child 第一次打开时创建；模板不变且
owner 挂载期间复用。未打开实例只有一个无 Child Popup shell，不建立打开态订阅。

默认内容由一个 internal `TabOverflowMenu` 与一个 internal `TabOverflowMenuItem` 表达。事件使用类级 handler；单项
不持有 owner/container，也不创建 command 或 delegate subscription。

## 5. 快照、Action 与失效

打开时根据容器 bounds、Offset、Viewport 和 placement 扫描部分或完全不可见页签，按逻辑 index 生成不可变
`TabOverflowItem` 列表。TabControl 映射 `Header/HeaderTemplate`；TabStrip 映射 `Content/ContentTemplate`。

内部会话维护 public item 到源容器/action identity 的映射；映射只在 Popup 打开期间存在。选择变化发布新的不可变
projection；外部集合 add/remove/move/replace/reset 直接使会话失效并关闭 Popup，避免在旧几何快照上同步。

Action 顺序：

```text
view -> context session validation -> weak action target -> ITabOverflowOwner
     -> unified selection or CloseTab -> collection/selection state
```

`TryClose` 必须经过 effective `IsClosable`、`Closing`、cancel、集合变更、选择回放和 `Closed`。调用方不能从 context
或视觉容器直接改 `Items` / `ItemsSource`。

## 6. 生命周期与资源释放

### 6.1 普通关闭

light-dismiss、Escape、选择后的 `Dismiss` 或 collection invalidation 都进入同一幂等关闭路径：

1. 关闭 Popup。
2. 解除 collection 与 selection subscription。
3. 清空内部 item-to-source map。
4. 把 context Items 置为空、SelectedItem 置空、weak action target 置空。
5. 保留已创建但无数据的自定义/default visual subtree，以优化重复打开。

Gallery `SearchableTabOverflowPopup` 自己拥有 query。普通 light-dismiss 时 query 保留；选择成功前主动清 query。

### 6.2 完整 teardown

`OnApplyTemplate` 开始、detach、`OverflowPopupTemplate` 变化或 Popup part 失效时：

1. 先执行普通关闭的全部清理。
2. 从旧 Popup 移除 child。
3. 清除 cached visual、context、template 与 owner 引用。
4. 解除 template part handler；不留下 dispatcher callback、timer、global handler 或 resource host。

Pinned-open 只阻止普通关闭。生命周期 teardown 无条件优先，并且只在 attach、模板应用、布局/可见性或属性变化时重新
收敛；不使用无限 post、重试或 polling。

## 7. Gallery 搜索实现

新增一个 Gallery-only `SearchableTabOverflowPopup`，由两个 ShowCase 页面和四个控件复用：

- 200 宽容器、elevated background、Popup shadow、LG radius、hidden overflow。
- 搜索区使用 XS/SM padding 与 bottom border。
- LineEdit/Input 含 Search icon、clear 和本地化 `Search tabs...`。
- 列表 max-height 300，自身纵向滚动。
- 空状态使用 disabled text color，居中显示本地化 `No matching tabs`。
- 过滤使用 `Contains(query, StringComparison.OrdinalIgnoreCase)`，不使用 `ToLower()`。
- ArrowUp/Down 从 input 转移焦点；menu 支持 skip-disabled、Home/End、Enter、Escape。
- pointer 打开只展开 Popup，不自动聚焦 selected/first item、Popup 根节点或 input；焦点保留在更多按钮，后续显式键盘或 pointer 操作才进入内容。
- 选择顺序为 clear query -> `TryActivate` -> `Dismiss`。

TabControl 与 TabStrip 页面分别新增稳定 SourceKey：

- `tab-control-overflow-popup-search`
- `tab-strip-overflow-popup-search`

每个 ShowCase 同时展示 Line/Card，并只使用 public API。示例以 `MaxWidth=720` 和 stretch 对齐适配窄 Gallery 列，禁止用固定宽度把更多按钮排到裁剪边界外。

## 8. 性能交付门禁

实现前在同一机器、同一 runtime、相同 30 tabs、相同 viewport 和主题下记录现有基线；实现后使用同一 benchmark
参数比较 cold create、first open、reopen、close、scroll 与 Gallery navigation。

阻断标准：

- repeated open/close allocated bytes/op 与 Gen0 压力至少降低 30%。
- repeated open/close median time 不得稳定回退；5% 内差异只有在多轮区间重叠时视为噪声。
- never-open control 的创建、首次 layout、steady measure/arrange 与 scroll 不得稳定回退超过 5%。
- 第一次打开的额外成本必须只来自一次模板视觉树创建；后续打开不能重建该视觉树。
- 关闭态打开会话级 collection/selection、per-item button/command、timer、dispatcher callback 与 owner action target
  数量必须为 0；模板生命周期的 more-button `Click` 与 Popup `Closed` handler 可以保留到 re-template / detach，但必须
  成对解除。默认 item container 不得保留旧 item/header/template，会话间是否由框架回收复用不作为 public contract。
- 100 次 open/close 后必须回到第一次 close 后的固定 warm baseline，只允许当前空 context 与一棵缓存视觉树；旧
  projection、数据、携带旧会话状态的 item container、订阅不得存活，也不得形成批次间正斜率。三轮 Gallery 进入/离开触发完整 teardown
  后，owner/context/data/template visual 必须全部可回收。

任一稳定回退或保留对象都必须先修复；不以功能完成、平均值改善或 GC 最终可能回收为豁免。

## 9. 测试矩阵

### 9.1 API 与行为

- 两个 base owner 的 property 类型、默认 null 和四个派生控件继承。
- context/item public shape、immutability、PropertyChanged 与 stale-session action rejection。
- 四个控件、四向 placement、partial overflow、disabled、selected、closable、custom header template。
- default/custom template selection、close cancellation、可写/只读 ItemsSource、Items 路径。
- external collection mutation 关闭；selection change 更新 projection。
- 搜索大小写、clear、empty、keyboard、disabled skip、query persist/reset。

### 9.2 生命周期与 retention

- never opened、first open、100 次 reopen、ordinary close、pinned normal close、pinned lifecycle close。
- template replace、re-template、detach/reattach、owner collection reset、Gallery navigation。
- ordinary close 断言固定 warm baseline；完整 teardown 后再用 weak references 验证 owner、scroll viewer、保存的旧
  context、snapshot list/item、header、header template、custom visual root 全部可回收。
- DynamicResource 作为 `OverflowPopupTemplate` 来源时，旧 template/resource anchor 在 owner 释放后可回收。

### 9.3 Theme、Gallery 与 AOT

- `PART_OverflowPopup`、三个 indicator part、单一 `TabScrollViewerTheme` 与 `TabOverflowMenuTheme` contract。
- Light/Dark、Desktop/Browser、Line/Card、Top/Right/Bottom/Left。
- 两个 Gallery ShowCase 与 localization/catalog/source snapshot tests。
- targeted Desktop tests、Gallery tests、LLMS generate/verify、AOT/trim analyzer、linked registration、真实 NativeAOT
  publish 和启动 smoke。

## 10. 文档同步

稳定设计由以下控件文档维护：

- `docs/controls/desktop/navigation/tab-control/overflow-popup-design.md`
- `docs/controls/desktop/navigation/tab-control/overview.md`
- `docs/controls/desktop/navigation/tab-control/implementation.md`
- `docs/controls/desktop/navigation/tab-strip/overview.md`
- `docs/controls/desktop/navigation/tab-strip/implementation.md`
- 两个 control-level changelog

本设计评审阶段不手工更新 `docs/AI/generated/llms`。运行时代码、Theme、Gallery 和测试落地后，再由 LLMS Generator
统一生成并验证最终 public surface 与语义结构。
