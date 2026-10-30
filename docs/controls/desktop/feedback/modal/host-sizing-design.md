# Modal 宿主尺寸与 Resize 设计

本文档定义 `Dialog` 与 `MessageBox` 在 Overlay 和原生 Window 宿主中的尺寸请求、结构性最小尺寸、宿主容量、交互 resize 与 maximize/restore 契约。Modal 总体公共设计见 [Modal 桌面版架构设计](overview.md)，内部 owner 与 presenter 结构见 [Modal 桌面版实现原理](implementation.md)，尺寸 Token 语义见 [Modal Token 设计](token.md)。

## 1. 设计定位

Modal 尺寸模型负责把 public `HostWidth`、`HostHeight`、`HostMin*`、`HostMax*` 请求解析为可供具体宿主执行的有效 Surface 约束。它覆盖初次自然测量、运行时尺寸请求、用户拖拽缩放、owner 或 screen 容量变化、maximize/restore，以及 Window chrome 与 Surface 正文之间的换算。

尺寸模型不负责用户内容内部的滚动、缩放或响应式布局。Dialog 只保证默认模板中的标题、Footer 操作区和非零正文 viewport 在宿主容量允许时保持可用；复杂内容仍应自行提供 `ScrollViewer` 或其他溢出策略。

## 2. 设计原则

- `HostWidth`、`HostHeight`、`HostMin*` 和 `HostMax*` 统一使用 DIP，并统一描述 `DialogSurface` 正文尺寸，不包含 BoxShadow 或 Window chrome。
- `HostMin*` 是调用方请求的下限，不是绕过控件结构下限的开关。有效最小尺寸不得低于标题、Footer 和最小正文 viewport 形成的结构性最小尺寸。
- `HostMax*` 是调用方请求的上限；宿主容量是不可突破的最终上限。默认 `PositiveInfinity` 表示使用当前宿主容量，不表示创建无界 Surface。
- `HostWidth/Height=NaN` 在初次打开时选择自然尺寸，不把自然尺寸转换为永久 minimum。Overlay 自然轴持续参与布局，只有该轴收到 finite 尺寸请求或被用户 resize 接管时才进入定尺寸状态。
- Overlay 与 Window 使用同一套 Surface 约束解析；normal 尺寸状态、宿主容量来源、Window chrome 换算和 resize 执行机制按宿主策略分别维护。
- Overlay resize handle 在按下后捕获 pointer，并在 release 或 capture lost 时通过同一个幂等结束路径清理本次 resize；pointer/native 热路径只使用已解析约束，不重新测量标题、Footer 或内容。
- owner、screen、主题、模板或有效按钮集合变化后重新解析约束；定尺寸轴只在实际尺寸越界时 clamp，自然轴按最新约束重新测量，不无条件重置用户已经调整的尺寸。
- Overlay 的 owner 容量变化只临时约束当前 Surface position，不把边界修正写入用户偏移。默认居中的 Dialog 按新 owner bounds 重新居中；用户拖动形成的偏移在容量恢复后继续生效。
- Overlay 自动布局只产生当前受限的 actual size，不把自然轴的测量结果写成定尺寸意图。宽、高两轴的状态和接管入口相互独立。

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
| normal axis sizing | Overlay 正常态中每个轴的尺寸模式：自然轴或定尺寸轴；其生命周期独立于当前 actual size。 |
| fixed size | finite `HostWidth/Height` 请求或用户 resize 形成的 presenter-owned 定尺寸值；不反向写回 public request。 |
| actual size | presenter 当前实际排列的 Surface 正文尺寸。 |

### 3.2 Public API 语义

| API | 稳定语义 |
| --- | --- |
| `HostWidth`, `HostHeight` | finite 值请求对应轴的 Surface 尺寸；初次 `NaN` 选择自然尺寸。Overlay 未被接管的自然轴在约束变化后继续自然测量。打开后设置 `NaN` 不作为强制切回自然模式或重置用户尺寸的命令。用户 resize 不反向写回这两个属性。 |
| `HostMinWidth`, `HostMinHeight` | 提高有效最小尺寸；低于结构性最小尺寸的值不会降低控件结构下限。注册默认值继续为 `0`。 |
| `HostMaxWidth`, `HostMaxHeight` | 限制有效最大尺寸；`PositiveInfinity` 解析为宿主容量。小于有效最小尺寸时，有效最小尺寸优先。原生 Window 任一轴使用 finite `HostMax*` 时，有效 native maximize capability 关闭，避免平台最大化产生受限的非整屏窗口。 |
| `IsResizable` | 只控制用户是否可以执行交互 resize；无论其值如何，初始尺寸和运行时显式尺寸都必须落在有效约束内。 |
| `IsMaximizable` | 请求宿主提供最大化能力。Overlay 不受 finite `HostMax*` 影响；原生 Window 只有在两个 `HostMax*` 都为 `PositiveInfinity` 时才令 `CanMaximize=true`，否则标题栏最大化按钮禁用。 |

`DialogOptions` 与声明式 `Dialog` 使用相同语义。`MessageBox` 可以通过自身 Token 或 Theme 提高 requested/structural minimum，但不能削弱 Dialog 的结构性下限。

### 3.3 Overlay 正常态的按轴尺寸模式

每个轴分别拥有以下两种模式。finite public request 与用户 resize 是定尺寸值的不同来源，不构成两套相互同步的尺寸缓存。

| 模式 | 进入条件 | 布局规则 |
| --- | --- | --- |
| 自然轴 | 初次打开时对应 `HostWidth/Height=NaN`，且该轴未被显式请求或交互 resize 接管。 | Surface 对应 Width/Height 保持自然测量语义；当前 actual size 由内容、effective constraints 和 host capacity 共同确定。 |
| 定尺寸轴 | 初次或运行时收到 finite 尺寸请求，或用户 resize 实际改变该轴。 | presenter 持有该轴的定尺寸值，在有效区间内保留，越界时 clamp；放宽约束不自动切回自然模式。 |

模式不能根据 `Dialog.HostWidth/Height` 的当前值反推：用户 resize 不回写 public request，运行时设置 `NaN` 也不清除已经形成的定尺寸状态。

| 输入或操作 | 按轴状态变化 |
| --- | --- |
| owner 容量、`HostMin/Max`、结构性最小尺寸、内容或主题变化 | 保留模式；自然轴重新测量，定尺寸轴按最新约束保留或 clamp。自动 clamp 不接管自然轴。 |
| finite `HostWidth` / `HostHeight` 请求 | 只将对应轴切为定尺寸并更新尺寸值；另一轴保留模式，即使其自然测量结果因换行而变化也不能被写死。 |
| 运行时设置 `HostWidth/Height=NaN` | 自然轴继续自然测量；定尺寸轴保留当前尺寸，不以此操作重置用户几何。 |
| 用户从东/西边缘 resize | 仅实际改变的宽度轴转为定尺寸；高度轴保留模式。 |
| 用户从北/南边缘 resize | 仅实际改变的高度轴转为定尺寸；宽度轴保留模式。 |
| 用户从角点 resize | 两轴分别在实际改变时接管；零位移或约束阻止尺寸变化时不切换模式。 |
| 拖动标题栏或 placement 变化 | 只更新位置，不改变尺寸模式。 |
| maximize / restore | 最大化临时应用 capacity；恢复正常态的按轴模式、定尺寸值和位置意图，自然轴按恢复时的约束重新测量。 |

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
| normal 尺寸状态 | 按轴保留自然或定尺寸模式；自然轴随着布局和约束变化重新测量。 | 原生 Show 前解析初始自然尺寸；显示后由 native actual size、显式请求和原生 resize 驱动，不采用 Overlay 的持续自然测量规则。 |
| resize 执行 | `OverlayDialogResizer` 依据缓存约束 clamp Surface 和 offset，只接管实际改变的轴。 | `Window.CanResize` 与平台 native resize 依据 Window min/max 执行。Surface 内 Overlay resizer 保持隐藏。 |
| maximize | Surface 使用完整 Dialog body owner bounds。 | 仅当两个 `HostMax*` 都为 `PositiveInfinity` 时启用，平台 Window 使用当前 working area。 |
| restore | 恢复正常态的按轴模式、定尺寸值和用户位置偏移；自然轴重新测量，定尺寸轴按最新 normal constraints clamp。 | 恢复此前 Window placement，再换算并按最新 Surface constraints clamp。 |
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
- finite `HostWidth/Height` 变化表示对应轴新的显式尺寸请求。打开后设置 `NaN` 不强制重置当前定尺寸几何。Overlay 的按轴模式转换遵循 §3.3，原生 Window 遵循 §5 的实际尺寸策略。
- Overlay 自然轴受限时只改变当前 actual size；容量或约束放宽后按内容重新测量。不得将受限的 `Bounds` / `DesiredSize` 写成该轴固定 Width/Height，也不缓存首次自然尺寸作为恢复目标。
- Overlay 定尺寸轴以及 Window actual size 只在越界时 clamp；放宽约束不无条件恢复旧尺寸，不重新解释为自然轴。
- `IsResizable` 变化不重置 actual size。
- normal resize 只更新 presenter 拥有的交互几何；Overlay 按实际改变的轴更新定尺寸值，不把另一自然轴的 actual size 一并提交。不反向修改 `HostWidth/Height`，避免 public request 与运行时几何形成双向状态竞争。
- Overlay maximize 临时使用 capacity，不改写 normal 按轴模式、requested/effective constraints；原生 Window 的 finite `HostMax*` 与 maximize capability 互斥，两轴都恢复为 `PositiveInfinity` 后 capability 自动恢复。

Window presenter 在解析 Surface constraints 后进行一次单向 chrome 换算：

```text
windowMin = effectiveSurfaceMin + windowChrome
windowMax = effectiveSurfaceMax + windowChrome
windowRequested = requestedSurfaceSize + windowChrome
```

`windowChrome` 以当前 Window Template 的 ContentPresenter 到 `DialogSurface` 的实际 inset 语义为准：managed/drawn title bar 模板使用 `Padding`、`FrameShadowThickness` 和有效标题栏高度；CSD 模板使用 `Padding` 与平台发布的 `WindowDecorationMargin`。`VisibleFrameBorderThickness`、CSD 状态、frame metrics、DPI 或模板变化都会触发重新解析，但纯 BoxShadow 绘制范围不进入 chrome 或 Surface 尺寸。

### 6.1 Overlay 自然测量与恢复

Overlay 在每次有效布局中根据正常态的按轴模式生成 Surface 输入：自然轴提供 `NaN`，定尺寸轴提供当前受限的尺寸值，Min/Max 提供最新 effective constraints。actual size 从本次测量和排列结果读取，不能反向成为自然轴的定尺寸来源。正文高度测量必须使用当前有效宽度，使文本换行等宽高依赖参与同一次布局。

只有 finite public request 或用户交互 resize 能接管相应轴。自动 constraint clamp、另一轴的显式更新、position 调整与 restore 都不能仅因手中持有完整 `Rect`，就把两轴的 actual size 同时写成定尺寸。位置计算仍使用本次实际 Surface size，并保持既有 startup anchor 和用户偏移语义。

进入最大化时保存 normal 按轴模式、定尺寸值和位置意图。最大化中的 owner 容量变化只更新最大化几何，不以该几何覆盖 normal 状态；此时收到 finite `HostWidth/Height` 请求只更新快照中对应轴的定尺寸状态。restore 先恢复 normal 模式，再按最新约束测量自然轴或 clamp 定尺寸轴，不能只把最大化前的实际矩形作为两个固定尺寸恢复。

自然高度恢复必须保持同一 Dialog Session 和 Content 实例，不依赖关闭重开、替换内容、计时重试或调用方手动 `InvalidateMeasure()`。内容、主题或宽度已变化时，应恢复到当前自然测量结果，而不是复用初次打开时的高度。

## 7. 架构、Template 与 Owner 边界

- `Dialog` 只拥有 public requested properties 与 Session，不缓存 presenter 的实际几何。
- `DialogSurface` 拥有结构性区域的测量结果；Header、Content viewport、Footer 和 `PART_Resizer` 继续由 `DialogSurfaceTheme.axaml` 组合。
- concrete presenter 拥有 host capacity、effective constraints、actual size 与 maximize restore geometry。Overlay presenter 还独占 normal 按轴尺寸模式及定尺寸值，将其与本次 actual size 分开；当前受限的 Surface position 与 `Dialog.OffsetX/Y` 中的持久用户偏移也分别管理。
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
  -> Overlay normal axis sizing | Window native actual size
  -> natural measure / explicit request / user resize / maximize / restore
  -> actual Surface size
```

Overlay 的实际测量结果不反向覆盖 normal 自然轴模式。模式和正常态快照由当前 presenter 生命周期拥有，Session 结束时释放；重开时重新读取 public request，不跨 Session 继承用户 resize 或容量钳制结果。

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
- Overlay 未被显式请求或用户 resize 接管的自然轴，在容量与约束放宽后恢复当前自然测量；自动 clamp 不成为持久尺寸意图。该规则同样适用于 `MessageBox` 和声明式、静态 API 创建的 Overlay Session。
- runtime `NaN` 不作为重置定尺寸或用户几何的命令；原生 Window 显示后的 native actual size 策略保持独立，不能因 Overlay 的持续自然测量而改变。
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
| Overlay presenter | 八方向 resize、pointer release/capture lost、owner 往返缩放及位置偏移继续满足约束；自然轴受限与恢复不切换模式，单轴请求和交互只接管对应轴，maximize/restore 不固化自然尺寸。 |
| Window presenter | Window tree 在 native Show 前完成 Surface 自然测量、chrome 换算、owner-screen capacity、最终尺寸和 initial placement；首个原生可见帧直接使用该几何快照，不发生 opacity staging、Dispatcher 延迟或 post-show correction。native min/max、runtime Host*、screen/chrome 变化和 restore 均保持正文语义；finite HostMax 任一轴都会禁用 native maximize，两轴恢复无限后重新启用。 |
| MessageBox | MessageBox 最小宽度继续提高 Dialog structural minimum，不能降低结构性高度。 |
| Gallery | resizable Window/Overlay 示例无需手写 `HostMinHeight` 也不能缩到操作区不可用；显式 HostMin/Max 示例能展示收紧区间。 |
| 平台 | Windows、macOS、X11 和 Wayland 实机验证最小尺寸、working area、DPI 和 native resize。 |
| 文档与 AOT | 控件文档、源码 public surface、Token 类型或生成数据、Modal 文档一致；实现不增加反射或动态注册。 |

### 11.1 Overlay 按轴尺寸验收

基础自然高度场景保留非空标题，设置 `IsFooterVisible=false`，由正文中的固定页脚占据 32 DIP；关闭 motion、标题栏拖动与交互 resize。不得在 owner 往返过程中关闭重开 Dialog、替换正文或通过用户尺寸操作切换模式。

| 场景 | 必须证明的行为 |
| --- | --- |
| 原始自然高度往返 | owner 宽度固定 `1240`，高度 `820 → 640 → 820`；Dialog `HostWidth=760`、`HostMaxWidth=760`、`HostHeight=NaN`。正文保留编辑框、1200 DIP 滚动内容与固定 32 DIP 页脚；固定 `HostMaxHeight=796` 和每步更新为 owner 高度减 24 两种配置均恢复初始实测 viewport，误差不超过 1 DIP。 |
| 公开重测对照 | 在相同往返中重复设置 `HostHeight=NaN` 或调用公开 `InvalidateMeasure()` 不得是恢复前提；自然模式保持一致，不丢失内容或焦点。 |
| 单轴显式请求 | 自然高度 Dialog 在运行时更新 finite `HostWidth` 后仍能恢复高度；自然宽度与 finite `HostHeight` 的反向组合对称验证。 |
| 交互 resize | 东/西、北/南与角点分别验证接管轴、未接管自然轴、零位移和边界钳制；已形成的定尺寸在合法范围内保持，不因 runtime `NaN` 被重置。 |
| 最大化与恢复 | 纯自然和混合轴模式在 maximize/restore 后仍可随容量恢复；最大化中修改一轴显式请求不得接管另一自然轴。用户已调整的定尺寸与位置偏移继续保留。 |
| 约束与内容变化 | 分别收紧、放宽 `HostMin/Max`，以及改变内容、主题或宽度；自然轴按最新内容和约束测量，不固定首次高度；定尺寸轴遵循保留或 clamp 规则。 |
| 状态与 UX | 同一 Session、Content、草稿和有效焦点保持；viewport、extent、可见滚动条和滚动边界随实际布局更新，不通过删除高度约束、滚动内容、页脚或裁剪条件使测试通过。 |

Headless 用例证明 managed 尺寸和状态契约；真实窗口还需在相同尺寸序列下核对渲染与交互。测试与截图按[测试生命周期规范](../../../../engineering/development/test-value-and-lifecycle.md)处理；临时截图在取证完成后删除，不加入项目或 commit。
