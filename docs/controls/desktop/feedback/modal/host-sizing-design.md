# Modal 宿主尺寸与 Resize 设计

本文档定义 `Dialog` 与 `MessageBox` 在 Overlay 和原生 Window 宿主中的尺寸请求、结构性最小尺寸、宿主容量、交互 resize 与 maximize/restore 契约。Modal 总体公共设计见 [Modal 桌面版架构设计](overview.md)，内部 owner 与 presenter 结构见 [Modal 桌面版实现原理](implementation.md)，尺寸 Token 语义见 [Modal Token 设计](token.md)。

## 1. 设计定位

Modal 尺寸模型负责把 public `HostWidth`、`HostHeight`、`HostMin*`、`HostMax*` 请求解析为可供具体宿主执行的有效 Surface 约束。它覆盖初次自然测量、运行时尺寸请求、用户拖拽缩放、owner 或 screen 容量变化、maximize/restore，以及 Window chrome 与 Surface 正文之间的换算。

尺寸模型不负责用户内容内部的滚动、缩放或响应式布局。Dialog 只保证默认模板中的标题、Footer 操作区和非零正文 viewport 在宿主容量允许时保持可用；复杂内容仍应自行提供 `ScrollViewer` 或其他溢出策略。

## 2. 设计原则

- `HostWidth`、`HostHeight`、`HostMin*` 和 `HostMax*` 统一使用 DIP，并统一描述 `DialogSurface` 正文尺寸，不包含 BoxShadow 或 Window chrome。
- `HostMin*` 是调用方请求的下限，不是绕过控件结构下限的开关。有效最小尺寸不得低于标题、Footer 和最小正文 viewport 形成的结构性最小尺寸。
- `HostMax*` 是调用方请求的上限；宿主容量是不可突破的最终上限。默认 `PositiveInfinity` 表示使用当前宿主容量，不表示创建无界 Surface。
- `HostWidth/Height=NaN` 表示以自然尺寸作为初始请求，不表示把自然尺寸永久锁定为最小尺寸。
- Overlay 与 Window 使用同一套 Surface 约束解析；差异只存在于宿主容量来源、Window chrome 换算和 resize 执行机制。
- Overlay resize handle 在按下后捕获 pointer，并在 release 或 capture lost 时通过同一个幂等结束路径清理本次 resize；pointer/native 热路径只使用已解析约束，不重新测量标题、Footer 或内容。
- owner、screen、主题、模板或有效按钮集合变化后重新解析约束；只有当前实际尺寸越界时才纠正，不无条件重置用户已经调整的尺寸。
- Overlay 的 owner 容量变化只临时约束当前 Surface position，不把边界修正写入用户偏移。默认居中的 Dialog 按新 owner bounds 重新居中；用户拖动形成的偏移在容量恢复后继续生效。

## 3. 尺寸模型与 Public API

### 3.1 术语

| 术语 | 含义 |
| --- | --- |
| requested size | `HostWidth` / `HostHeight` 提供的初始或运行时显式 Surface 尺寸；初次打开时的 `NaN` 使用自然测量。 |
| requested minimum | `HostMinWidth` / `HostMinHeight` 提供的调用方下限。 |
| requested maximum | `HostMaxWidth` / `HostMaxHeight` 提供的调用方上限。 |
| structural minimum | 默认模板保持标题、Footer 和最小正文 viewport 可用所需的 Surface 下限。 |
| host capacity | 当前宿主能够提供的最大 Surface 正文范围。 |
| effective constraints | 结构性下限、调用方请求和宿主容量共同解析出的最终 min/max。 |
| actual size | presenter 当前实际排列的 Surface 正文尺寸。 |

### 3.2 Public API 语义

| API | 稳定语义 |
| --- | --- |
| `HostWidth`, `HostHeight` | finite 值请求当前 Surface 尺寸；`NaN` 只在初次打开时选择自然尺寸。打开后改为 `NaN` 不重新测量或重置 actual size。用户 resize 不反向写回这两个属性。 |
| `HostMinWidth`, `HostMinHeight` | 提高有效最小尺寸；低于结构性最小尺寸的值不会降低控件结构下限。注册默认值继续为 `0`。 |
| `HostMaxWidth`, `HostMaxHeight` | 限制有效最大尺寸；`PositiveInfinity` 解析为宿主容量。小于有效最小尺寸时，有效最小尺寸优先。原生 Window 任一轴使用 finite `HostMax*` 时，有效 native maximize capability 关闭，避免平台最大化产生受限的非整屏窗口。 |
| `IsResizable` | 只控制用户是否可以执行交互 resize；无论其值如何，初始尺寸和运行时显式尺寸都必须落在有效约束内。 |
| `IsMaximizable` | 请求宿主提供最大化能力。Overlay 不受 finite `HostMax*` 影响；原生 Window 只有在两个 `HostMax*` 都为 `PositiveInfinity` 时才令 `CanMaximize=true`，否则标题栏最大化按钮禁用。 |

`DialogOptions` 与声明式 `Dialog` 使用相同语义。`MessageBox` 可以通过自身 Token 或 Theme 提高 requested/structural minimum，但不能削弱 Dialog 的结构性下限。

## 4. 结构性最小尺寸

结构性最小尺寸由默认 `DialogSurface` 的稳定语义区域组成：

| 区域 | 宽度职责 | 高度职责 |
| --- | --- | --- |
| Header | Overlay 模式保留 logo、标题区域和可见 caption 操作；Window 模式由 Window chrome 另行约束。 | Overlay 模式保留完整标题栏及其 margin。 |
| Content viewport | 使用 `DialogToken.MinWidth` 作为最小正文宽度基线。 | 使用 `DialogToken.MinHeight` 作为非零正文 viewport 高度基线。 |
| Footer | 当 `IsEffectiveFooterVisible=true` 时，保留当前有效按钮的一行排列及 Footer padding/margin。 | 保留完整按钮行及 Footer padding/margin。 |

结构性测量按 `DialogSurface` 的 DockPanel 组合语义执行：宽度取 Header、Content viewport 和 Footer 所需宽度的最大值，高度取三个可见区域所需高度之和；各区域自身的 padding 与 margin 计入对应 extent。`IsEffectiveFooterVisible=false` 时 Footer 不参与结构下限；Window presenter 隐藏 Surface Header，因此 Window 的结构性 Surface 下限不重复计算标题栏。

自定义内容的完整 `DesiredSize` 不参与结构性最小尺寸，否则自然尺寸会被错误地永久转换成 minimum。内容只获得 Token 定义的最小 viewport；超出 viewport 的内容由用户模板自行滚动、换行或裁剪。

当自定义主题缺少可选内部节点时，结构性测量退化到 `DialogToken.MinWidth/MinHeight` 基线。稳定 Template Part `PART_Header` 和 `PART_ButtonBox` 存在时，其实际测量必须参与结构下限。

## 5. 宿主策略

| 维度 | Overlay Host | Window Host |
| --- | --- | --- |
| capacity 来源 | Dialog body owner bounds；始终受 owner visible frame 限制。 | Dialog 所在 screen 的 working area，扣除当前 Window chrome 后换算为 Surface capacity。 |
| normal 约束载体 | 直接设置 `DialogSurface.Min/MaxWidth/Height`。 | 将 Surface effective constraints 加上 Window chrome，设置 native Window 的 `Min/MaxWidth/Height`。 |
| resize 执行 | `OverlayDialogResizer` 依据缓存约束 clamp Surface 和 offset。 | `Window.CanResize` 与平台 native resize 依据 Window min/max 执行。Surface 内 Overlay resizer 保持隐藏。 |
| maximize | Surface 使用完整 Dialog body owner bounds。 | 仅当两个 `HostMax*` 都为 `PositiveInfinity` 时启用，平台 Window 使用当前 working area。 |
| restore | 恢复此前 actual Surface size/offset，再按最新 normal constraints clamp。 | 恢复此前 Window placement，再换算并按最新 Surface constraints clamp。 |
| capacity 变化 | owner resize、visible frame 或 drawn frame 改变时重新解析。 | screen、render scaling 或 Window chrome 改变时重新解析。 |

`DialogHostType.Window` 因平台能力回退 Overlay 时，直接采用 Overlay 策略，不保留一套模拟 Window 的尺寸分支。

### 5.1 Window 初始几何快照

Window presenter 在调用原生 `Show()` 或 `ShowDialog(...)` 前完成一次原子初始几何解析。它先应用 managed Window 和 `DialogSurface` 的 styling/template，并测量 Window tree，使 `ContentPresenter` 附加 Surface；随后以 Surface 的自然测量、结构性约束和 Window chrome 换算为输入，选择 owner 所在 screen 的 working area（不可用时按既有 fallback 解析），得到最终 Surface 与 Window 尺寸。Presenter 使用同一快照和 owner bounds、render scaling 计算初始 placement，再交给原生 Window 显示。

因此首个原生可见帧已经使用最终尺寸和位置。初始流程不使用 opacity staging、Dispatcher 延迟或显示后的 reposition；`Opened` 之后的处理只记录 native actual size，并在真实运行时 resize、窗口状态、screen/scaling 或 chrome 变化时按当前约束重新解析。

## 6. 约束解析算法

每个轴独立解析。输入值全部使用 Surface 正文 DIP：

```text
structural = structural minimum
requestedMin = max(0, HostMin)
requestedMax = finite HostMax ? max(0, HostMax) : capacity

effectiveMin = min(max(structural, requestedMin), capacity)
effectiveMax = max(effectiveMin, min(requestedMax, capacity))

preferred = finite HostSize ? HostSize : naturalSize
initialActual = clamp(preferred, effectiveMin, effectiveMax)
```

解析遵循以下边界规则：

- `HostMax < structural minimum` 时，在 capacity 允许的范围内以结构性最小尺寸优先；有效 max 提升到 effective min。
- capacity 小于结构性最小尺寸时，capacity 优先，不能为保持 Dialog 尺寸而越出 owner 或 screen。默认模板优先压缩 Content viewport，必要时压缩至 `0`，继续保留 Header 和 Footer；capacity 连 Header 与 Footer 的 mandatory extent 都无法容纳时才允许裁剪 mandatory regions。这是结构性下限唯一的退化条件，且不改变下一次 capacity 恢复后的 normal constraints。
- finite `HostWidth/Height` 变化表示新的显式尺寸请求并更新 actual size；打开后切换为 `NaN` 只撤销显式请求，不重置 actual size。`HostMin/Max`、主题、按钮或 capacity 变化只在 actual size 越界时 clamp。
- 初始为自然尺寸的轴一旦因新约束越界并被 clamp，clamp 结果成为该轴新的 actual size；后续放宽约束不会让它自动回落到原自然尺寸。
- `IsResizable` 变化不重置 actual size。
- normal resize 完成后只更新 presenter 的 actual size/restore state，不反向修改 `HostWidth/Height`，避免 public request 与运行时几何形成双向状态竞争。
- Overlay maximize 临时使用 capacity，不改写 requested/effective constraints；原生 Window 的 finite `HostMax*` 与 maximize capability 互斥，两轴都恢复为 `PositiveInfinity` 后 capability 自动恢复。

Window presenter 在解析 Surface constraints 后进行一次单向 chrome 换算：

```text
windowMin = effectiveSurfaceMin + windowChrome
windowMax = effectiveSurfaceMax + windowChrome
windowRequested = requestedSurfaceSize + windowChrome
```

`windowChrome` 以当前 Window Template 的 ContentPresenter 到 `DialogSurface` 的实际 inset 语义为准：managed/drawn title bar 模板使用 `Padding`、`FrameShadowThickness` 和有效标题栏高度；CSD 模板使用 `Padding` 与平台发布的 `WindowDecorationMargin`。`VisibleFrameBorderThickness`、CSD 状态、frame metrics、DPI 或模板变化都会触发重新解析，但纯 BoxShadow 绘制范围不进入 chrome 或 Surface 尺寸。

## 7. 架构、Template 与 Owner 边界

- `Dialog` 只拥有 public requested properties 与 Session，不缓存 presenter 的实际几何。
- `DialogSurface` 拥有结构性区域的测量结果；Header、Content viewport、Footer 和 `PART_Resizer` 继续由 `DialogSurfaceTheme.axaml` 组合。
- concrete presenter 拥有 host capacity、effective constraints、actual size 与 maximize restore geometry。Overlay presenter 将当前受限的 Surface position 与 `Dialog.OffsetX/Y` 中的持久用户偏移分开管理。
- 共享尺寸解析是 Dialog 模块中的纯值职责，不读取平台对象、不创建 Visual，也不持有订阅。
- Window 模块只发布 chrome、working area、render scaling 和 native resize 能力；Dialog presenter 不复制 Win32、AppKit、X11 或 Wayland 几何算法。

该设计不新增 public API、Template Part、pseudo class、ControlTheme key 或 Token 名称。`PART_Header`、`PART_ButtonBox` 和 `PART_Resizer` 的既有稳定性保持不变；`ContentFrame` 仍是内部主题节点，不成为外部模板契约。

## 8. 状态流与失效

```text
Dialog Host* / IsResizable / IsMaximizable
  + DialogSurface structural metrics
  + presenter host capacity
  -> resolve effective Surface constraints
  -> Overlay direct constraints | Window chrome translation
  -> initial request / user resize / maximize / restore
  -> actual Surface size
```

以下输入使结构性测量或有效约束失效：

- Dialog template reapply、主题或 Token scope 变化。Surface 的 measure invalidation 会重新比较结构性最小尺寸；只有数值改变时才通知 presenter。
- Header/Footer visibility、标题图标、caption capability 或有效按钮集合变化。
- owner body bounds、Window chrome、screen working area 或 render scaling 变化。
- `HostWidth/Height/Min/Max`、`IsResizable` 或 `IsMaximizable` 变化。

结构性测量结果由当前 presenter/Surface 生命周期拥有，并在 re-template 或 presenter dispose 时丢弃。失效处理不增加全局 cache、静态 Window 字典或跨 Session 状态。

## 9. 资源、性能与 AOT 边界

- 结构性测量只发生在打开、相关属性/主题/模板失效或宿主容量变化时，不进入 pointer move 或 native live-resize 热路径。
- live resize 只读取缓存的 numeric constraints，并复用现有 Window native resize 或 Overlay offset/transform 路径；Overlay pointer capture lost 与 release 共享结束清理，不把旧 origin 带入下一次拖拽。
- 尺寸解析使用静态类型和值计算，不使用反射、动态发现或运行时注册。
- 新增的属性观察或 template part 事件必须由 concrete presenter 或 `DialogSurface` 释放，不引入全局订阅。
- 设计不改变现有 NativeAOT 反射兼容边界。

## 10. 兼容性与定制边界

- public 属性、注册默认值、绑定模式和 `DialogOptions` 成员保持不变。
- 可观察行为改变为：调用方不能再通过小于结构下限的 `HostMin*` 或 `HostWidth/Height` 把默认 Dialog 缩到标题、Footer 或正文 viewport 不可用。
- 调用方仍可通过更大的 `HostMin*`、更小的 `HostMax*` 或 finite `HostWidth/Height` 收紧 normal 尺寸区间。
- `HostMax*=PositiveInfinity` 保持“没有调用方额外上限”的含义，但 actual size 仍受 host capacity 限制。
- 原生 Window 的有效 `CanMaximize` 等于 `IsMaximizable && HostMaxWidth==PositiveInfinity && HostMaxHeight==PositiveInfinity`；该规则不改变 public `IsMaximizable` 的值，也不影响 Overlay maximize。
- 自定义 `DialogSurface` Theme 负责提供等价的 mandatory region 测量语义；缺失 part 时只能获得 Token 基线保护。
- 用户内容负责 viewport 内部的 overflow；Dialog 不根据任意 Content 的完整 DesiredSize 自动扩大结构下限。

## 11. 验证要求

| 层级 | 必须证明的行为 |
| --- | --- |
| 纯约束计算 | requested min/max、structural、capacity、NaN natural size、冲突 max、capacity 小于结构下限均得到确定结果。 |
| DialogSurface | Header/Footer visibility、标准/自定义按钮、模板 spacing/font 和 Dialog Token 变化会更新结构性最小尺寸；Content 的完整 DesiredSize 不会变成 minimum。 |
| Overlay presenter | 八方向 resize 不能越过 effective constraints；pointer release/capture lost 均结束当前 resize；owner 往返缩放时默认居中与用户偏移均可恢复，临时边界修正不改变 offsets；自然轴 clamp、maximize/restore 和 runtime Host* 更新保持 actual geometry 一致。 |
| Window presenter | Window tree 在 native Show 前完成 Surface 自然测量、chrome 换算、owner-screen capacity、最终尺寸和 initial placement；首个原生可见帧直接使用该几何快照，不发生 opacity staging、Dispatcher 延迟或 post-show correction。native min/max、runtime Host*、screen/chrome 变化和 restore 均保持正文语义；finite HostMax 任一轴都会禁用 native maximize，两轴恢复无限后重新启用。 |
| MessageBox | MessageBox 最小宽度继续提高 Dialog structural minimum，不能降低结构性高度。 |
| Gallery | resizable Window/Overlay 示例无需手写 `HostMinHeight` 也不能缩到操作区不可用；显式 HostMin/Max 示例能展示收紧区间。 |
| 平台 | Windows、macOS、X11 和 Wayland 实机验证最小尺寸、working area、DPI 和 native resize。 |
| 文档与 AOT | 控件文档、源码 public surface、Token 类型或生成数据、Modal 文档一致；实现不增加反射或动态注册。 |
