# ColorPicker 确认与取消设计

本文定义 `ColorPicker` 与 `GradientColorPicker` 的颜色候选、显式确认、取消和提交值隔离契约。控件整体设计见 [桌面版架构设计](overview.md)，源码职责与宿主接入见 [桌面版实现原理](implementation.md)，视觉变量见 [Token 设计](token.md)，设计变化记录见 [Changelog](changelog.md)。

## 1. 设计定位

颜色选择器支持即时生效和显式确认两种交互。显式确认允许用户在面板中连续试选纯色、透明度、预设色或渐变，并在提交前放弃本次修改。确认能力由选择器宿主管理，面板内的滑杆、色谱、输入区与预设色板共同编辑一份候选状态。

该能力覆盖弹层选择器及其 Click、Hover、Focus 触发方式。独立使用 `ColorPickerView` / `GradientColorPickerView` 的应用仍自行拥有值提交时机；面板协作属性与操作通知不扩大为第二套公共提交 API。

## 2. 设计原则

- 宿主 `Value` 是绑定、Form、trigger 色块和 trigger 文本的唯一提交值来源。
- 确认模式中面板只编辑候选值，候选变化不提前触发宿主值变化或 Form 通知。
- 显式确认是提交入口；普通关闭与生命周期结束只丢弃候选和释放编辑资源。
- 取消不写回打开时的值，因此不会覆盖编辑期间的新外部值。
- `IsNeedConfirm=false` 保留原有 API 默认值、事件、清除与同步行为。
- 纯色与渐变使用相同操作语义，渐变候选与提交值保持对象隔离。

## 3. 专项模型与 Public API

### 3.1 状态所有权

| 状态 | Owner | 语义 |
| --- | --- | --- |
| 提交值 | `ColorPicker` / `GradientColorPicker` | public `Value`，纯色为 `Color?`，渐变为 `LinearGradientBrush?`。 |
| 候选值 | 面板编辑状态 | 本次编辑可提交的颜色，可为 `null`；输入、预设色、滑杆与 clear 都更新此状态。 |
| 编辑游标与预览 | 面板及内部输入控件 | HSV 游标、活动渐变节点、文本输入和面板预览；由候选与有效输入派生。 |
| 输入有效性 | `ColorPickerInput`，由面板汇总 | 决定能否将当前输入同步到候选并确认。 |
| 业务打开状态 | `AbstractColorPicker` | `IsPickerOpen` 与 pinned 策略。 |
| 物理显示状态 | Popup | 宿主显示、定位、动效和物理关闭。 |

候选 `null` 表示明确的空颜色，透明纯色仍然是非空颜色。白色等编辑默认色只用于初始化调色游标，不因打开或关闭面板自动成为提交值。原有 `DefaultValue` 初始化语义保持不变；初始化完成后的会话以当前提交值为基准。

面板已有非空纯色属性或渐变编辑默认值不能代替可空候选状态。明确选择预设色或输入有效文本时，即使颜色与编辑默认值相同，也形成非空候选；候选形成不能只依赖属性差异通知。清除操作先形成候选 `null`，再同步面板空态；不得将清除过程中产生的中间颜色当作新的有效候选。

### 3.2 属性契约

`AbstractColorPicker.IsNeedConfirm` 是默认值为 `false` 的布尔 StyledProperty，两个选择器共同继承。设为 `true` 时显示“取消 / 确定”，仅显式确认写回宿主。

`ColorPicker.ValueSyncStrategy` 默认 `Immediate`，其枚举继续只有 `Immediate` 与 `OnCompleted`。`IsNeedConfirm=true` 优先于该策略，不修改属性本身的值。`GradientColorPicker` 通过共享确认属性支持相同交互，不增加 `ValueSyncStrategy`。

```xml
<atom:ColorPicker IsNeedConfirm="True" Value="{Binding SelectedColor}" />
<atom:GradientColorPicker IsNeedConfirm="True" Value="{Binding SelectedGradient}" />
```

### 3.3 宿主事件契约

- `ValueChanged` / `GradientValueChanged` 跟随实际提交值变化，包含外部写入；面板草稿变化不转发为宿主值事件。
- 确认模式中，一次有效的非空确认产生一次 `ValueSelected`，即使确认值与原提交值相同；实际值没有变化时不额外制造值变化或 Form 通知。
- 确认清空时通过值变化事件报告 `null`，不为 `ValueSelected` 增加可空参数或新事件类型。原提交值已经为空时不制造值变化通知。
- 取消、无效输入、普通关闭和生命周期结束不产生 `ValueSelected`。
- 默认模式保留原有关闭时非空 `ValueSelected` 的语义。面板内部事件仍负责编辑协作，不代表业务提交。

## 4. 模式与宿主策略

| 控件与配置 | 面板编辑 | 面板清除 | 普通关闭 |
| --- | --- | --- | --- |
| 纯色，`IsNeedConfirm=false`，`Immediate` | 立即写回 `Value`。 | 立即写回 `null`。 | 保留提交值，按原规则发出选择事件。 |
| 纯色，`IsNeedConfirm=false`，`OnCompleted` | 保存最近候选。 | 保留原有直接清空行为。 | 提交最近候选，按原规则发出选择事件。 |
| 渐变，`IsNeedConfirm=false` | 保留原有即时写回行为。 | 立即写回 `null`。 | 保留提交值，按原规则发出选择事件。 |
| 纯色或渐变，`IsNeedConfirm=true` | 只更新候选与面板预览。 | 候选为 `null`，可取消。 | 丢弃草稿，不提交、不发选择事件。 |

Click、Hover、Focus 保留各自打开、关闭延迟与 light-dismiss 行为；确认模式不会把 Hover 离开自动关闭解释为确认。Escape、外点、失焦、窗口失活及 `ClosePicker()` 等业务关闭均执行取消语义。

| 宿主状态 | 确定 | 取消 | 生命周期结束 |
| --- | --- | --- | --- |
| 普通弹层 | 提交并请求关闭。 | 丢弃候选并请求关闭。 | 丢弃候选，释放编辑订阅及宿主。 |
| `IsPopupPinnedOpen=true` | 提交，按最新提交值重置候选，保持打开。 | 按最新提交值重置候选，保持打开。 | 丢弃候选并按共享 Popup 生命周期契约处理物理宿主。 |

钉住时普通关闭请求仍受 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md) 拦截，不发布瞬时关闭业务状态。无效锚点、宿主消失等导致物理关闭时，旧候选失效；恢复显示后从最新提交值开始编辑。

## 5. 架构与职责

- `AbstractColorPicker` 负责共享确认配置、面板接入及 Popup 生命周期。Popup 的显示状态不决定颜色是否提交。
- `ColorPicker` / `GradientColorPicker` 负责各自提交值类型、确认写回、对外事件、Form 和 trigger 投影。
- `AbstractColorPickerView` 与具体面板承载内部候选编辑和确认/取消操作通知；纯色面板维护可空候选，渐变面板维护独立候选画刷。
- `ColorPickerInput` 负责格式输入及有效性。色谱、滑杆和预设色板输出编辑操作，不直接写宿主 `Value`。
- Theme 负责调色正文与操作区的静态组合，Gallery 展示契约，不持有业务草稿或补偿取消。

候选状态只有一个 owner，面板属性与内部控件属性是状态投影。同步顺序应区分宿主初始化、外部重置和用户编辑，避免把重置过程当成新用户输入。日期专用编辑会话不承担颜色或渐变语义。

## 6. Template 与集成契约

`ColorPickerViewTheme.axaml` 与 `GradientColorPickerViewTheme.axaml` 保持预设色组、分隔线和调色正文的原有组合，在正文之后设置确认操作区。操作区与正文纵向组合，内部“取消 / 确定”按钮统一使用 `SizeType="Small"` 并靠右排列；关闭确认模式时整区隐藏且不占布局间距。

按钮可见性、可用状态、间距、对齐及视觉绑定优先使用 ControlTheme Setter、选择器和 TemplateBinding。C# 只处理操作通知与值转换。按钮文案通过控件本地化 Catalog 提供，布局复用 SharedToken，按钮复用 Button 主题；详见 [Token 边界](token.md)。

操作区不增加 ColorPicker Semantic Part。既有 `root`、`body`、`content`、`description`、`popup.root` 身份、数量、route 与默认视觉保持不变，`popup.root` 包含增加操作区后的完整弹层边界，详见 [Semantic Part 契约](semantic-part.md)。

默认模板负责接入内部确认/取消通知。替换面板模板时，应用必须保留操作接线及输入有效性投影；修改 trigger 或 `popup.root` 的专用 Semantic Style 不得改变提交策略。

## 7. 数据流与生命周期

本节定义 `IsNeedConfirm=true` 的编辑流程及进入、退出该模式时的转换；默认模式的同步与关闭行为遵循第 4 节兼容矩阵。

### 7.1 打开与编辑

打开时，从宿主提交值初始化面板候选；渐变按值复制画刷和每个 stop。初始化完成后再接入编辑通知，不能因面板初始属性变化触发业务提交。提交值为空时保留候选空态，编辑游标可以使用有效默认色。

用户操作先更新输入或游标，再形成有效候选并刷新面板预览。trigger 色块、文本、业务绑定与 Form 继续从宿主提交值派生。空候选在 `IsClearEnabled` 允许的清除操作中形成，确认空候选有效。

### 7.2 确认与取消

确认依次执行：

1. 汇总输入有效性并完成当前有效文本到候选的同步。输入无效时禁止确认，不能提交上一次有效候选掩盖错误输入。
2. 读取候选快照，以 `SetCurrentValue` 写回宿主，保留绑定；渐变提交值与继续编辑的候选保持隔离。
3. 由实际属性变化发出值事件与 Form 通知；对非空确认发出一次 `ValueSelected`。
4. 普通弹层请求关闭；钉住弹层从最新提交值建立新候选，继续显示。

关闭回调不重复提交或发出确认选择事件。一个确认操作在完成写回前结束旧编辑通知路径，重入或关闭动画不会再次处理同一次确认。

取消只结束候选编辑并请求关闭；钉住时重置候选。正常重开不保存上次未确认草稿，取消不写 `Value`、不触发 Form 通知。

### 7.3 外部写入与配置切换

打开期间外部更新 `Value`，包括 Form clear，宿主立即接收新提交值，旧候选与无效输入状态一起重置。显式外部 `null` 不通过 `DefaultValue` 重新填充。渐变候选从新的提交画刷重新复制，取消不能回放旧画刷。

打开期间切换 `IsNeedConfirm`，丢弃未提交草稿，按当前提交值同步面板，再使用新模式；配置变化本身不隐式提交。`IsNeedConfirm=true` 时切换纯色 `ValueSyncStrategy` 也重置草稿，显式确认优先级保持不变；`IsNeedConfirm=false` 时该同步策略的既有配置行为保持不变。

### 7.4 生命周期与对象隔离

普通关闭结束当前编辑订阅；重开前重新初始化候选。卸载、模板重建、TopLevel 切换和宿主失效同时清理候选、旧输入/按钮订阅及旧绑定。关闭动画只是视觉表现，不延长候选的有效期。锚点恢复或新模板恢复打开时也按当前提交值重新开始。

渐变复制覆盖 StartPoint、EndPoint、Opacity、SpreadMethod 及每个 stop 的 Color、Offset；不共享可变 `GradientStops` 或 `GradientStop`。编辑节点颜色、位置、数量和次序都只能影响候选。确认时生成独立提交快照；钉住弹层继续编辑时不能修改刚提交的对象。

## 8. 资源、性能与 AOT 边界

纯色候选使用值类型，渐变复制成本随 stop 数量线性增长。复制发生在编辑初始化、外部重置和提交边界，不在纯预览绘制中反复复制完整画刷；滑杆现有候选生成算法保持自身职责。

该能力不增加异步任务、计时器或全局缓存。新增内部操作订阅归属面板会话或模板，结束编辑、模板重建和 detach 时成对释放。模式与输入有效性的模板投影采用编译期可见绑定，本地化沿用生成式 Catalog，不引入反射发现或字符串属性路径。

## 9. 兼容性与定制边界

`IsNeedConfirm=false` 是兼容模式，保留既有纯色同步策略、渐变即时写回、直接清除和关闭选择事件。显式开启确认才改变面板编辑、清除及关闭时机，不改变 `Value` 类型、绑定默认值或数据校验接入。

确认模式保持调色正文尺寸、预设色布局、trigger 尺寸、Popup 定位、动画与钉住契约；仅操作区增加所需弹层高度。既有 ControlTheme key、模板部件、Token 与五个 Semantic Part 不因确认能力被删除、重命名或改路由。

确认操作区不提供额外公共按钮文本、命令或主题 Part API。专用 Semantic Style 仍是既有部件的样式入口，Gallery 和应用不通过获取内部按钮或面板节点来补写确认逻辑。

## 10. 验证要求

| 层级 | 必须证明的不变量 |
| --- | --- |
| 默认兼容 | `IsNeedConfirm=false`；纯色 Immediate/OnCompleted、渐变即时提交、面板直接清除与原有关闭事件保持一致。 |
| 候选编辑 | 输入、预设色、透明度与渐变节点编辑更新面板预览，宿主值、trigger、绑定与 Form 不提前变化。 |
| 确认事件 | 修改后确认只写回一次；确认原值仍有一次非空选择事件；取消及无效确认不发选择事件，关闭回调不重复通知。 |
| 空值 | 打开/关闭空值不提交默认白色；透明色与 null 区分；清除后取消保留提交值，清除后确认提交 null。 |
| 渐变隔离 | 编辑颜色、位置、增删节点及取消不修改原画刷或 stops；钉住确认后继续编辑不修改刚提交对象。 |
| 外部与配置 | 打开期间外部值写入、Form clear、确认模式或同步策略变化重置旧候选，取消不覆盖新提交值。 |
| 输入校验 | 尚未提交的有效文本在确认前同步；无效文本不能确认，也不能以旧候选代替。 |
| 关闭与生命周期 | Escape、外点、失焦、窗口失活、Hover 关闭与 ClosePicker 取消；detach、模板重建、无效锚点和 TopLevel 变化清理旧草稿及订阅。 |
| 钉住弹层 | 确认提交、取消重置，业务状态保持打开；宿主失效清理，恢复不复活旧候选。 |
| Theme 与 Gallery | 原正文及 trigger 尺寸保持；默认模式无额外间距；确认按钮与文案在 Light/Dark、不同尺寸和触发方式下可用；Gallery 展示纯色、渐变、清除取消及绑定值。 |
| 语义与 AOT | 五个 Part 的专用 Style 仍命中；静态注册、模板和本地化生成产物保持 AOT 兼容。 |

验证代码按 [测试价值与生命周期规范](../../../../engineering/development/test-value-and-lifecycle.md) 临时优先并在验证后清理，最终运行所修改模块的 focused 检查。文档检查只证明结构、链接和设计一致性，不能替代行为或发布验证。
