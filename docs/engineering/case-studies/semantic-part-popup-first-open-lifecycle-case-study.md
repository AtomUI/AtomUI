# Semantic Part Popup 首次打开生命周期竞态案例

本文记录 2026-09 ColorPicker Semantic Part 改造中暴露的 Popup 首次打开问题、全局同类排查、最终修复和后续评审规则。
它是长期工程案例，不属于阶段性计划或进度记录。

核心结论：Popup 的“请求打开”“业务状态已打开”“物理宿主已打开”和“视觉内容已可见”是四个不同状态。延迟创建的
Popup host 可能先触发 `Opened`，再挂载会预置透明度的 `PopupMotionActor`。如果两条生命周期只处理一种先后顺序，首次打开
就会停在“物理已开、视觉透明”，第二次打开才正常。钉住预览还必须在首次打开前抑制 light-dismiss，否则会留下只拦截输入、
无法完成关闭的遮罩。

## 适用范围

这份案例适用于：

- 在模板中或独立 host 中使用 `Popup`、`Flyout`、overlay root 的控件。
- Popup 内容、Popup host template 或 motion actor 延迟实例化的路径。
- Gallery Semantic Part Preview 为解析 `popup.*` 而把弹层钉住常开的场景。
- 在 Popup 直接 `Child` 外新增静态语义包裹层的模板改造。
- 依赖 light-dismiss、placement、arrow、shadow mask 或跨 VisualRoot 高亮的弹层控件。

本次直接涉及的实现包括：

- `Popup` / `PopupMotionActor`
- `ColorPicker` / `GradientColorPicker`
- `Flyout` 及使用它的控件家族
- `ComboBox`
- `Mentions`
- `InfoPickerInput` 及其 DatePicker / TimePicker 家族

## 用户体验契约

本次问题的原始触发条件是：Gallery 页面初始显示 Examples，用户第一次切换到 Semantic Parts，延迟创建 ColorPicker 预览，
预览中的 Popup 应立即显示并保持钉住。修复必须同时满足：

- 第一次进入 Semantic Parts 就可看到 ColorPicker Popup，不能依赖第二次切换或第二次打开。
- Popup 位置、箭头、阴影遮罩和外沿尺寸保持原行为。
- Popup 钉住期间页面其他区域仍可交互，高亮器可解析 `popup.root`。
- 切回 Examples、页面 detach 或 lifecycle close 时物理 host 能释放。
- 取消钉住后恢复该控件原本的 Click / Hover / Focus light-dismiss 配置。
- 正常产品路径继续启用 motion；不能把关闭动画当作根因修复。

2026-09-04 的用户录屏显示：第一次进入 Semantic Parts 时 Popup 没有视觉呈现，第二次切换后正常。这个“第一次失败、第二次正常”
是模板、host 或 actor 首次物化顺序问题的强信号。

## 四层状态模型

排查 Popup 首次打开时，必须分别观察下面四层状态：

| 层级 | 代表状态 | 回答的问题 |
| --- | --- | --- |
| 请求状态 | `IsPopupPinnedOpen` 或用户操作 | 外部是否要求弹层保持打开？ |
| 业务状态 | `IsPickerOpen`、`IsDropDownOpen`、`IsOpen` | 控件状态机是否接受了打开请求？ |
| 物理状态 | `Popup.IsOpen`、`Opened` / `Closed` | Avalonia Popup host 是否已经打开？ |
| 视觉状态 | actor 附加、Opacity、布局和目标可达性 | 用户是否真的能看到并操作弹层？ |

只断言 `Popup.IsOpen == true` 不能证明修复完成。本次故障正是业务状态和物理状态都为 true，但 motion actor 仍为透明。

## 排查记录

### 1. 保留原始复现

排查始终保留以下条件：

- Semantic Parts 真延迟创建。
- ColorPicker Popup 默认 motion 开启。
- Popup 钉住常开。
- 原始 Preview 高度、布局、placement 和跨 VisualRoot 高亮路径不变。

没有通过提前创建 Preview、移除 Popup、取消钉住或永久禁用 motion 让症状消失。

### 2. 先排除业务状态丢失

针对 ColorPicker 补充并检查了：

- 模板应用前设置 `IsPopupPinnedOpen=true`。
- `IsPickerOpen` 业务状态保持 true。
- pinned relay 在物理打开前连接到 `PART_Popup`。
- detach 时允许 lifecycle close，reattach 后重新投射 pinned 请求。

这些检查能证明打开请求没有丢失，但仍不能解释“物理已开、视觉不可见”。

### 3. 修正模板打开时序

ColorPicker 模板原先通过 `IsOpen="{TemplateBinding IsPickerOpen, Mode=TwoWay}"` 驱动物理 Popup。Semantic Preview 在模板应用前
已经设置 pinned/open，导致模板膨胀过程中就打开 Popup；placement target、pinned relay 和 dismiss 配置还未准备完成。

最终保留业务状态，移除模板 `IsOpen` 双向绑定，由 `AbstractColorPicker` 在 template part、placement、pinned relay、事件和 dismiss
都就绪后再打开物理 Popup。外部 `Closed` 仍负责回写业务状态，lifecycle close 使用显式作用域绕过 pinned close 拒绝。

这一步解决了“物理打开过早”的问题，但不是录屏中透明首次打开的最终根因。

### 4. 根据动画线索检查视觉状态

用户指出可能存在动画竞态后，检查发现：

1. `PopupMotionActor.OnAttachedToLogicalTree` 在 motion 开启时先把自身 `Opacity` 设为 0，防止 Popup Show 后闪现一帧全不透明内容。
2. 延迟创建的 Popup host 可以先触发 `Popup.Opened`，再应用 host template 并挂载 actor。
3. 旧的 `Popup.HandlePopupOpened` 在 actor 尚不存在时直接返回。
4. actor 随后通知 owner 已就绪，但旧实现只保存 actor 引用，不重新进入开启动画。
5. 因此 actor 永久停在 `Opacity=0`；关闭再打开后 actor 已存在，第二次动画才正常。

这与录屏的“第一次不显示、第二次正常”完全一致。

### 5. 用乱序生命周期红测证明因果

回归测试显式构造以下顺序：

```text
Popup 打开并触发 Opened
  -> 此时没有 motion actor
  -> late actor 以 Opacity=0 就绪
  -> Popup 必须重新对账并执行同一 open-motion 路径
  -> actor 最终 Opacity=1
```

`PopupLifecycleTests.Open_Popup_Starts_Open_Motion_When_Actor_Becomes_Ready_After_Opened` 在修复前失败，actor 的透明度保持 0；
共享 `Popup` 修复后通过。

## 根因分层

本次改造过程中出现过三个彼此独立、但会叠加成类似症状的问题。后续排查必须分开验证。

### 直接 Child 契约被语义包裹层破坏

ColorPicker 的 `popup.root` 需要一个 owner 模板内的静态 marker，因为运行时创建的 Picker View 跨过新的 `TemplatedParent` 边界，
owner 的 Semantic selector route 无法进入该动态模板。

如果直接用普通 `Border` 包裹 Popup 内容，虽然 marker 可达，却会破坏共享 Popup 的直接 Child 契约：placement 和
`ShadowsAwareContainer` 只从直接 `Child` 读取 `IArrowAwareShadowMaskInfoProvider`。最终使用
`ColorPickerPopupRootFrame : Border`，由它把箭头和 shadow mask 信息转发给内部 `ArrowDecoratedBox`。

长期规则：在 Popup 直接 Child 外增加静态语义层时，必须先审计直接 Child 的接口、类型、测量、placement、shadow 和 input
契约；新宿主要么实现等价转发，要么复用原稳定宿主，不能只看视觉效果。

### 物理 Popup 打开早于宿主配置

模板 `IsOpen` binding 可能在 `OnApplyTemplate` 完成配置前打开 Popup。钉住、placement、light-dismiss、事件和动态内容都需要在
首次物理打开前准备完成。对于这类控件，应让业务状态与物理 Popup 状态分离，并由代码在模板尾部进行一次状态投射和对账。

### Opened 与 actor-ready 顺序不完整

共享 `Popup` 过去只处理“actor 先就绪，再 Opened”。真实 host 首次物化时还会出现“Opened 先发生，再 actor 就绪”。
`Popup` 现在记录当前打开周期是否已经触发 `Opened`，把开启动画抽成幂等的 `StartOpenMotion()`，并从两条入口对账：

- `HandlePopupOpened()`：actor 已存在时立即开始。
- `NotifyMotionActorReady()`：Popup 已处于打开周期且没有执行关闭 motion 时补开始。

`Closed` 清理打开周期标记、取消 motion 并释放当前 actor，避免旧 host actor 跨周期复用。异步调度捕获本次 actor 和 cancellation
token，避免字段在 dispatch 前被新 host 替换。

## 钉住 Popup 的 light-dismiss 排查

Avalonia 在 Popup 打开瞬间依据 `IsLightDismissEnabled` 创建 dismiss overlay。打开后再把属性改为 false，已经创建的 overlay
不会因此消失。钉住 Popup 又会拒绝普通 dismiss close，于是 overlay 只剩下拦截页面输入的副作用。

正确顺序固定为：

```text
收到 pinned 请求
  -> 建立 owner -> Popup pinned relay
  -> 根据 pinned 状态计算本次物理 Popup 的 light-dismiss
  -> 完成 placement / input pass-through / event 配置
  -> 最后打开物理 Popup
```

取消钉住时不能无条件恢复为 true。拥有 trigger 配置的共享宿主必须恢复配置值：

```text
effective light-dismiss = configured light-dismiss && !IsPopupPinnedOpen
```

没有独立配置、模板默认固定为 true 的控件可以恢复模板默认值。

## 全局同类排查结果

本次按三组入口进行仓库级扫描：所有 `IsPopupPinnedOpenProperty` owner、所有 `IsLightDismissEnabled` 配置、所有
`PopupMotionActor` / motion actor 生命周期。结论如下。

| 范围 | 结果 | 处理 |
| --- | --- | --- |
| `PopupRootTheme` 与 `OverlayPopupHostTheme` | 都使用 `PopupMotionActor`，存在同一首次 actor-ready 乱序风险 | 在共享 `Popup` 修复，两种 host 一次覆盖 |
| ColorPicker / GradientColorPicker | motion 开启的 pinned Preview 首次暴露竞态；直接 Child 还有箭头/阴影转发契约 | 保留代码驱动物理打开，增加 `ColorPickerPopupRootFrame`，共享 Popup 补 actor-ready 对账 |
| AutoComplete | 已在首次打开前抑制 pinned light-dismiss | 保留现有保护；不以 Gallery 的 `IsMotionEnabled=false` 掩盖共享问题 |
| Select / Cascader / TreeSelect | 共用 `AbstractSelect`，已有首次打开前的 pinned light-dismiss 保护 | 无需重复修改 |
| ComboBox | pinned relay 已存在，但首次打开前没有抑制 light-dismiss | 在 `ComboBox` 模板接入和 pin 变化时对账，并补红绿测试（**已于 2026-09-16 关闭，见下方补记**） |
| Mentions | pinned relay 已存在，Popup 本身禁用 motion，但 light-dismiss overlay 仍会拦截输入 | 在 `Mentions` 模板接入和 pin 变化时对账，并补红绿测试 |
| DatePicker / TimePicker 家族 | 共用 `InfoPickerInput`，模板固定启用 light-dismiss，但 pinned 状态未前置覆盖 | 在共享 `InfoPickerInput` 修复，并用 DatePicker 回归测试覆盖 |
| Flyout 家族 | `FlyoutHost` 和多种控件把 pin 转发给 `Flyout`，但底层 Popup 仍直接使用 configured light-dismiss | 在共享 `Flyout` 计算 effective 值；Click 可恢复 true，Hover / Focus 保持 false |
| Menu / NavMenu / Tour | 相应 Popup 模板本来就关闭 light-dismiss | 无需修改 |
| Tooltip | 代码创建 Popup，未启用 light-dismiss，pin 不会留下 dismiss overlay | 无需修改 |
| Message / Notification | actor 在自身 `OnApplyTemplate` 获取，并从同一入口预隐藏和调度 show | 未发现独立的 `Opened -> actor-ready` 丢失握手 |
| Badge | 已使用 pending state 和 `TryStartPendingShowMotion()` 处理模板/actor 延迟 | 无需修改 |
| FloatButtonGroup 及模板内 motion 控件 | actor 由 owner template 直接管理，并在 template/load 路径显式对账 | 未发现与 Popup 相同的跨 host 乱序 |

扫描当时 Gallery 有 41 个页面接入 `SemanticPartsContentTemplate`。其中使用 pinned-open Popup 的预览是 AutoComplete、Cascader
和 ColorPicker；前两者把 motion 关闭，ColorPicker 保留正常 motion，因而首次暴露共享 Popup 的真实竞态。关闭 motion 可以降低
演示噪声，但不能成为修复或全局默认；至少要有一个真实 motion 开启的首次物化回归场景。

## 最终方案

最终保留的改动按所有权分层：

- `Popup` 负责物理打开周期和 motion actor 就绪顺序，支持 `Opened -> actor-ready` 与 `actor-ready -> Opened` 两种顺序。
- 产品控件负责自己的业务 open 状态、template part 配置顺序和 pinned 生命周期。
- 直接拥有模板 Popup 的控件负责在首次物理打开前计算 effective light-dismiss。
- `Flyout` 作为共享独立宿主统一计算 `configured && !pinned`，下游控件只转发配置与 pin。
- Gallery Preview 只负责延迟创建、目标解析和高亮，不在 Popup 打开后篡改产品控件状态来补救。

被移除或否定的方案：

- 不保留 ColorPicker 的瞬态属性忽略标记；它会掩盖状态机问题并引入第二套同步规则。
- 不通过永久关闭 motion 规避第一次透明。
- 不在 Popup 打开后修改 light-dismiss 期待移除已经创建的 overlay。
- 不让 Preview 全局扫描 TopLevel 或直接操纵产品控件内部 actor。
- 不把动态 Picker View 内的 marker 当作 owner selector 可达节点。

## 回归测试范式

### Motion 乱序测试

测试必须主动构造 actor 晚于 `Opened` 的顺序，并断言视觉结果，不只断言 `Popup.IsOpen`。

当前覆盖：

- `PopupLifecycleTests.Open_Popup_Starts_Open_Motion_When_Actor_Becomes_Ready_After_Opened`

### 首次 pinned light-dismiss 测试

测试必须在控件进窗口、应用模板或调用 `ShowAt` 之前设置 pin，才能覆盖“首次打开瞬间读取配置”的真实时序。至少断言：

- 业务状态打开。
- 物理 Popup 打开并收到 pinned relay。
- 首次打开前 effective light-dismiss 为 false。
- 取消 pin 后恢复原配置，而不是固定恢复 true。
- **树上不存在 `IsVisible=true` 的 `LightDismissOverlayLayer`。** 只断言 `IsLightDismissEnabled=false` 不足以证明遮罩
  未创建：属性被抑制后遮罩仍可能留在树上（`IsVisible=true`）并接管全部命中点。

当前覆盖：

- `FlyoutPinnedOpenTests.Pinned_Flyout_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_Configuration`
- `ComboBoxDisplayMemberBindingTests.Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It`
- `ComboBoxPinnedPopupOverlayTests`（遮罩层断言 + `InputHitTest` 行为断言 + 关闭回写；2026-09-16 补）
- `MentionsBehaviorTests.Popup_Pin_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It`
- `DatePickerBehaviorTests.Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It`

**补记（2026-09-16）：** 上表 ComboBox 一行已关闭。原测试只断言属性、未断言遮罩层，因此当“模板绑定 `IsOpen` 导致
**打开早于抑制**”时测试仍然全绿——属性最终确实是 `false`，但遮罩已经在模板充气阶段建出来并一直可见，实测钉住时
`InputHitTest` 命中 `LightDismissOverlayLayer`，整页除输入框外不可交互。修法：`PART_Popup` 去掉 `IsOpen` 模板绑定，
弹层开合改由控件代码接管，`OnApplyTemplate` 先抑制遮罩再补开弹层，并由 `Popup.Closed` 显式补齐关闭回写。
这与 `AbstractSelect`（Select / Cascader / TreeSelect）的既有约定一致：它们的模板同样不声明 `IsOpen` 绑定。

### Gallery 首次选择测试

Gallery 测试必须从 Examples 初始状态开始，第一次选择 Semantic Parts 后同时验证：

- 延迟 factory 只执行一次。
- Preview、owner 和 Popup 第一次 materialize。
- Popup 已打开且视觉 actor 不停留在透明状态。
- `popup.*` 高亮目标在正确 VisualRoot 中可达。
- 切回 Examples、detach、reattach 和再次选择不会依赖旧 host 状态。

## Review 清单

### Popup 生命周期

- 是否把请求、业务、物理和视觉状态分别建模和断言？
- `Opened` 与 template/actor ready 是否允许任意先后顺序？
- ready 晚到时是否有幂等的状态对账入口？
- async motion 是否捕获本次 actor 和 cancellation token？
- `Closed`、detach 和 re-template 是否清理旧 actor、订阅和 pending 状态？
- motion 关闭时是否显式恢复可见状态，而不是依赖默认 Opacity？

### Pinned 与 light-dismiss

- pin 是否在首次物理打开前传递到底层 Popup？
- effective light-dismiss 是否在首次打开前计算？
- 取消 pin 是否恢复控件/trigger 原配置？
- Hover / Focus 等原本为 false 的配置是否不会被误改为 true？
- pinned close 拒绝与 lifecycle close 是否有明确边界？

### Semantic wrapper

- 新 marker 是否位于 owner selector 真正可达的模板节点？
- runtime-created 子控件是否引入新的 `TemplatedParent` 边界？
- 在 Popup 直接 Child 外加 wrapper 时，是否转发原 Child 的接口和 placement/shadow/input 契约？
- 高亮 Bounds 是否贴合真实语义区域？

### 验证

- 是否有修复前失败、修复后通过的乱序或首次物化测试？
- 是否保留正常 motion、原布局、placement、arrow、shadow 和 dismiss 行为？
- 是否测试 PopupRoot 与 OverlayPopupHost 的共享实现？
- 是否运行对应控件测试、Gallery 测试和 `git diff --check`？

## 常见误判

- `Popup.IsOpen == true` 不等于用户可见；还要看 actor、Opacity、Bounds 和 VisualRoot。
- 第二次正常不代表状态值错误；更常见的是首次模板物化后，第二次复用了已经存在的 host/actor。
- headless 测试能证明状态机，但不能单独证明真实 host 的模板和合成时序。
- 打开后设置 `IsLightDismissEnabled=false` 太晚，已创建的 overlay 不会被撤销。
- 把 `IsMotionEnabled=false` 写进 Semantic Preview 只能回避动画路径，不能证明产品默认配置正确。
- 为 marker 加一个普通 Border 不是“零影响”改动；Popup 直接 Child 的类型和接口可能属于隐藏但必要的宿主契约。

## 本次验证命令

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
dotnet run --project tools/AtomUI.Docs.LLMsGenerator/AtomUI.Docs.LLMsGenerator.csproj -- verify --config docs/AI/generated/llms.config.json
git diff --check
```

针对每个新发现的 light-dismiss 缺口，先运行对应单测确认修复前失败，再实现共享 owner 修复并确认转绿。全量结果应以当前分支
最终验证输出为准；局部红绿测试不能替代 Desktop Controls 与 Gallery 全量回归。

2026-09-04 当前分支的最终验证记录：

| 验证 | 结果 |
| --- | --- |
| Popup、ColorPicker、Flyout、ComboBox、Mentions、DatePicker 组合回归 | 109 passed，0 failed |
| Desktop Controls 全量 | 3397 passed，0 failed |
| Gallery 全量 | 557 passed，0 failed |
| ColorPicker / Popup LLMS 范围生成 | 官方生成器成功生成 2 个 controls / 7 个文件；本次同步 ColorPicker 的 `index-cn.md`、`semantic-cn.md` 与 Popup 的 `semantic-cn.md` |
| `git diff --check` | 通过 |

仓库级 LLMS `verify` 仍为非零：AutoComplete 缺少显式 LLMS Semantic Parts；AutoComplete、Cascader、NumericUpDown、Transfer
及全局聚合产物存在本分支原有的陈旧项，Cascader/聚合内容另有外部项目名称诊断。本次只同步 ColorPicker 与 Popup 范围产物；
再次验证时两者均未出现在诊断列表。该已知基线问题不能被记录成“LLMS 全仓通过”，也不应通过本修复顺手重写无关控件文档。

## 相关文档

- [Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)
- [Semantic Part Gallery Preview 设计](../../gallery/authoring/semantic-part-preview.md)
- [ColorPicker 实现原理](../../controls/desktop/data-entry/color-picker/implementation.md)
- [Popup Anchor 作用域](../development/popup-anchor-scope.md)
