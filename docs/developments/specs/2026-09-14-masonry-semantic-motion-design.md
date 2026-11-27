# Masonry Semantic 视觉对齐（动效 + RTL）设计

- 日期：2026-09-14
- 分支：`feature/semantic-Masonry`
- 状态：设计已获用户批准（方案 A 全部对齐；默认值偏离维持现状）
- 参考源码：本地 `/Users/chinboy/Projects/ReferenceProjects/ant-design`（antd **6.6.3**，`6.6.1-104-g3bc029ad8f`），来源类型：本地（按《参考项目源码查找规范》逐级查找命中，未使用 GitHub）

## 1. 背景与范围

Masonry 的 Semantic PART 改造已存在于基线（`05104d19f feat(Semantic): add Masonry parts and harden preview highlighting`）：
`root` + `item` 两个 Part 与 antd 6.6.3 公开的 Semantic DOM 一致（上游仅 `root`/`item`），`MasonryItemStyle`、
三层测试、Gallery Semantic Tab、契约文档齐备。**语义部件契约本身不改。**

本设计覆盖剩余的视觉差距（已逐项比对 antd 6.6.3 源码）：

1. item 入场淡入（appear fade-in）
2. item 位置滑动（position glide）
3. item 离场淡出（leave fade-out，延迟卸载）
4. RTL 镜像（`FlowDirection=RightToLeft`）

**明确不改**（有意偏离、已文档化，经用户确认维持现状）：

- `ColumnCount` 默认 0（自适应 MinColumnWidth=320 / MaxColumnCount=4），不改为 antd 的固定 3。
- `ColumnGap`/`RowGap` 默认 16，不改为 antd 的 0。
- 布局算法（StableColumns/Reflow）、`LayoutChanged` 语义、`Masonry.Column`/`Masonry.Span`、无专属 Token 等全部维持。

## 2. 上游动效规格（antd 6.6.3 `components/masonry/style/index.ts`）

```ts
// item appear（CSSMotion motionAppear）
transition: opacity ${motionDurationSlow} ${motionEaseOut}; // opacity 0 -> 1
// item leave（CSSMotion motionLeave）
transition: opacity ${motionDurationFast} ${motionEaseOut}; // opacity 1 -> 0，动画期间节点保留在 DOM
// item 位置变化（&:not(${itemCls}-fade)）
transition: left ${...}, right ${...}, top ${motionDurationSlow} ${motionEaseOut};
```

数值（antd `motionUnit=0.1, motionBase=0` ⇔ AtomUI `MotionUnit=100ms, MotionBase=0`，完全一致）：

| 效果 | 时长 | 缓动 |
| --- | --- | --- |
| appear 淡入 | `MotionDurationSlow` = 300ms | `motionEaseOut` = cubic-bezier(0.215, 0.61, 0.355, 1) |
| leave 淡出 | `MotionDurationFast` = 100ms | 同上 |
| 位置滑动 | `MotionDurationSlow` = 300ms | 同上 |

新入场 item 只淡入（初始即在最终位置，不滑动）；既有 item 位置变化只滑动不淡入；被移除 item 淡出期间不滑动。

退出口：AtomUI 全局 `EnableMotion=false` 时主题编译将全部 Motion duration 置 0——动效必须据此退化为瞬时，不新增公共 API。

## 3. 设计

### 3.1 位置滑动（MasonryPanel 内，代码驱动）

Avalonia 的 Arrange 矩形不能做 CSS 式过渡。等价机制：

- `MasonryPanel.ArrangeOverride` 将子项排到**最终矩形**（布局数学保持纯净）。
- 对存在旧矩形且位置发生变化的子项：设置 `TranslateTransform` 为（旧位置 − 新位置），用 `Animation` 在
  `MotionDurationSlow` + `CubicBezierEasing(0.215, 0.61, 0.355, 1)` 下归零到 (0,0)。
- 首次排列的子项不滑动（由入场淡入接管）。
- 连续布局变化时复用同一 transform/animation（避免 resize 风暴下动画堆积）。

代码绑定理由（主题绑定优先约束的兜底条款）：偏移量为运行时布局状态（旧/新 Arrange 矩形），ControlTheme 无法表达；
时长值从主题 token 解析，不以魔法数硬编码。

### 3.2 入场淡入（MasonryPanel 内）

- 子项首次排列时：Opacity 0→1，`MotionDurationSlow` + `motionEaseOut`。
- 动画以 Animation 优先级运行，结束后释放，不占用用户对 item Opacity 的样式定制（运行期间动画优先级高于样式，
  与 antd motion class 行为一致）。

### 3.3 离场淡出（主题模板 ghost 层）

antd 靠 CSSMotion 延迟卸载节点；Avalonia `ItemsControl` 移除容器是同步的。等价机制：

- `MasonryTheme.axaml`：`PART_RootBorder` 内改为 `Grid` 包含 `PART_ItemsPresenter` 与 `PART_MotionGhostLayer`
  （`Canvas`，`IsHitTestVisible=false`，两个子元素叠放、原点一致，ghost 坐标系与 MasonryPanel 对齐）。
- `MasonryPanel` 订阅自身 `Children.CollectionChanged`；`Remove`/`Reset` 时把被移除容器连同其最后 Arrange 矩形
  上报 Masonry（内部事件）。
- Masonry 将被移除容器重父级到 `PART_MotionGhostLayer`，按最后矩形定位，Opacity 1→0
  （`MotionDurationFast`）后从 ghost 层移除并释放。
- ghost 期间容器仍携带 `.semantic-item`（对齐 antd leave 期间节点仍在 DOM 且带类）。
- 清理时机：淡出完成；Masonry 从视觉树 detach；主题模板重挂。Reset 批量清除时逐个淡出。
- ghost 不参与 `MasonryPanel` 布局、不计入 `LayoutChanged`、不参与命中测试。

### 3.4 RTL 镜像（MasonryPanel 内）

`FlowDirection=RightToLeft` 时 ArrangeOverride 对子项 X 坐标镜像（`x' = finalWidth − rect.Right`），
对齐仓库内 `StepsPanelItemLayoutPanel` 等 FlowDirection 处理先例。等价于 antd `-rtl` + `insetInlineStart` 逻辑定位。

### 3.5 token 与禁用退化

- 时长经主题 token 解析（`MotionDurationSlow`/`MotionDurationFast` 资源，主题禁用动效时值为 0）。
- duration ≤ 0 时不启动任何动画：入场即最终透明度、位置直接生效、离场直接移除。测试需覆盖该退化。

## 4. 语义契约影响

- `item` 的 ContractType/Cardinality/SelectorRoute/StyleType 全部不变。
- 状态表新增一行说明：容器移除后至淡出完成前，该容器作为 ghost 存在于 `PART_MotionGhostLayer`，仍携带
  `.semantic-item`；`semantic-item` 计数在该窗口内比"已实例化 item container 数"多出 ghost 数
  （与 antd CSSMotion leave 窗口一致）。
- `semantic-part.md`、`implementation.md`、`changelog.md` 同步更新。

## 5. 测试计划（先写失败测试）

新增（`tests/AtomUI.Desktop.Controls.Tests/Masonry/`）：

1. 入场淡入：新子项首次排列后 Opacity 从 0 动画到 1；推进时钟后到达 1。
2. 位置滑动：既右子项矩形变化后 TranslateTransform 从偏移归零；最终 (0,0)。
3. 离场 ghost：移除 item 后容器出现在 ghost 层原位置、淡出后从树中移除；期间保留 `.semantic-item`。
4. EnableMotion 禁用：三项动效全部瞬时（无动画、无 ghost 停留）。
5. RTL：RightToLeft 下矩形 X 镜像。
6. 生命周期：detach 时 ghost 立即清理，无泄漏；Reset 批量移除全部淡出。

更新既有：受动画时序影响的断言（如 Opacity 命中测试需等待动画完成或禁用动效主题）；Gallery 页面测试如涉及计数窗口同步更新。

## 6. 验收（含视觉验收，按全局约束执行）

- 构建 + 选定测试全绿；`git diff --check` 干净。
- 视觉验收步骤（操作路径/预期现象/判定标准/录屏范围，含 Gallery 分类与入口路径）在实现完成后产出，
  由用户回传截图或录屏作为唯一视觉证据；未经回传不宣称视觉通过。图像不入库。

## 7. 风险与缓解

| 风险 | 缓解 |
| --- | --- |
| 离场重父级容器与 ItemsPresenter 生成器冲突（容器回收） | Masonry 当前无回收路径（无虚拟化）；实现时验证 Reset/Remove/Replace 全分支；如生成器持有引用则以 detach 即清理兜底 |
| ArrangeOverride 内启动动画的时序 | Avalonia 惯例：动画在 arrange 后启动（Clock 驱动，不阻塞布局）；必要时 Dispatcher.Post |
| 既有 Opacity 命中断言与运行中动画竞争 | 测试等待动画完成或用禁用动效主题断言静态值 |
| resize 风暴动画堆积 | 复用 transform/animation 实例；同一子项未完成动画被新布局取代时重启自当前值 |
| ghost 层引入破坏模板结构断言 | 先跑既有模板/语义测试，按需同步更新断言并保持"无静态 semantic marker"不变 |
