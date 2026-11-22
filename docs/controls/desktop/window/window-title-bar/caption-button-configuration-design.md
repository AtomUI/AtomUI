# WindowTitleBar Caption Button 配置设计

本文档定义 AtomUI managed caption buttons 的公共配置、能力约束、状态推导、职责边界和模板集成契约。Window 的整体公共契约见 [Window 桌面版架构设计](../window/overview.md)，WindowTitleBar 的公共设计见 [WindowTitleBar 桌面版架构设计](overview.md)，源码职责和布局模型见 [WindowTitleBar 实现原理](implementation.md)。

## 1. 设计定位

Caption button 配置负责区分以下两个互相正交的维度：

- 窗口是否允许执行最小化、最大化、全屏、置顶或关闭操作。
- AtomUI managed title bar 是否展示对应操作入口。

窗口能力属于宿主 `Window` 和平台实现；按钮呈现属于 `WindowTitleBar` 控件家族。隐藏 managed caption button 不修改窗口能力，也不禁止任务栏、系统菜单、快捷键、标题栏双击或应用自定义入口继续执行同一窗口操作。

该设计覆盖默认 `WindowTitleBar`、`CaptionButtonGroup`、`CaptionButton`、`WindowsCaptionButton` 及复用默认 caption button 模型的派生标题宿主。业务自定义操作仍由 `LeftAddOn`、`RightAddOn` 或专用标题栏承载，不进入系统 caption button 配置模型。

## 2. 设计原则

- 能力与呈现分离：`CanMinimize`、`CanMaximize` 和平台能力决定操作是否成立；caption visibility 属性只表达 managed button 呈现意图。
- Public API 保持 XAML 原生：每个固定窗口操作使用独立 `StyledProperty<bool>`，允许 Binding、Style 和属性优先级分别组合。
- 单一状态 owner：`Window` 持有窗口能力、`WindowState`、`Topmost`、全屏恢复状态和实际窗口操作；模板节点不维护第二份领域状态。
- 宿主上下文自动接入：位于 AtomUI `Window` 逻辑树内的每个 `WindowTitleBar` 自动连接最近宿主，不要求应用或派生窗口重复配置 caption 状态和命令。
- 投影定义与生命周期分离：`Window` 定义完整宿主投影和标题栏交互订阅并返回可释放 lease；每个 `WindowTitleBar` 独立持有和释放自己的 lease。
- 派生视觉单向计算：`CaptionButtonGroup` 只根据投影输入计算 effective visibility 和 checked state，不查找或反向修改宿主窗口。
- 声明式交互：caption button 通过命令和操作参数发出请求，不在 template apply 阶段逐个注册 Click handler。
- 固定系统操作保持固定结构：caption button 不是任意排序的 item collection；平台顺序、Windows decoration role 和可访问性语义由内置模板维护。
- 平台能力显式退化：不支持的操作从 effective visibility 中移除，不显示不可执行的 managed button。

## 3. 专项模型与 Public API

### 3.1 状态术语

| 术语 | Owner | 语义 |
| --- | --- | --- |
| Requested visibility | `Window` public API | 应用是否请求显示某个 managed caption button。 |
| Operation capability | `Window` 与平台层 | 操作当前是否允许执行，与按钮是否显示无关。 |
| State eligibility | 标题栏呈现策略 | 当前 `WindowState` 是否允许该按钮出现在普通标题栏。 |
| Effective visibility | `CaptionButtonGroup` | requested visibility、operation capability、state eligibility 和平台支持共同推导的实际 managed button 可见性。 |
| Checked state | `Window` 状态投影 | 最大化、全屏和置顶等 toggle operation 的当前状态。 |

Requested visibility 不是 capability alias。设置 visibility 为 `false` 只从 managed title bar 移除入口，不写回 `CanMinimize`、`CanMaximize`、`Topmost` 或 `WindowState`。

### 3.2 Window Public API

Caption button 的公共配置统一属于宿主 `Window`：

| API | 类型 | 默认值 | 语义 |
| --- | --- | --- | --- |
| `IsMinimizeCaptionButtonVisible` | `StyledProperty<bool>` | `true` | 请求显示最小化按钮。 |
| `IsMaximizeCaptionButtonVisible` | `StyledProperty<bool>` | `true` | 请求显示最大化/还原共用按钮。 |
| `IsCloseCaptionButtonVisible` | `StyledProperty<bool>` | `true` | 请求显示关闭按钮。 |
| `IsFullScreenCaptionButtonVisible` | `StyledProperty<bool>` | `false` | 请求显示进入/退出全屏共用按钮。 |
| `IsPinCaptionButtonVisible` | `StyledProperty<bool>` | `false` | 请求显示置顶/取消置顶共用按钮。 |

`IsMaximizeCaptionButtonVisible` 控制同一个 caption slot；窗口最大化后该 slot 展示 Restore 状态，不引入第二个 Restore visibility 属性。FullScreen 和 Pin 使用相同 toggle-slot 规则。

该公共模型不使用 flags enum、caption options 对象或可变按钮集合。固定操作的独立 StyledProperty 保证每个按钮能够单独参与 XAML Binding、Style 和属性优先级，而不会要求调用方重建整个配置集合或使用 converter。

## 4. 平台与窗口状态策略

### 4.1 Effective visibility 矩阵

下表中的“显示”均要求对应 requested visibility 为 `true`，并且当前标题栏使用 AtomUI managed button。`Minimized` 不增加独立的隐藏规则：窗口本身不呈现，派生值按 Normal 规则保留，并在恢复后的 `WindowState` 投影到达时重新计算。

| 操作 | Normal | Minimized | Maximized | FullScreen | 额外能力条件 |
| --- | --- | --- | --- | --- | --- |
| Minimize | 显示 | 按 Normal 保留 | 显示 | 隐藏 | `CanMinimize=true` 且平台允许最小化。 |
| Maximize / Restore | 显示 Maximize | 按 Normal 保留 | 显示 Restore | 隐藏 | `CanMaximize=true` 且平台允许最大化/还原。 |
| FullScreen / Exit | 显示 FullScreen | 按 Normal 保留 | 隐藏 | 显示 Exit | 进入全屏要求平台能把该窗口带入全屏（macOS 有 owner 的窗口不能）；退出入口在全屏期间始终保留。 |
| Pin / Unpin | 按 checked state 显示 | 按 Normal 保留 | 按 checked state 显示 | 按 checked state 显示 | 当前 backend 支持置顶。 |
| Close | 显示 | 按 Normal 保留 | 显示 | 显示 | managed close operation 可用。 |

能力为 `false` 时，即使 requested visibility 为 `true`，对应 managed button 仍然隐藏。该规则避免出现可见但无法执行的系统按钮，也保持 visibility 属性不反向授予窗口能力。

### 4.2 平台边界

| 平台或模式 | Caption button 来源 | 配置语义 |
| --- | --- | --- |
| Windows managed chrome | `WindowsCaptionButton` | 完整应用 requested visibility、能力和状态规则；隐藏 Maximize 同时移除该 managed button 的 Windows snap hover 入口。 |
| Linux managed chrome | `CaptionButton` | 完整应用 requested visibility、能力和状态规则；窗口管理器对原生能力的最终执行结果仍由平台决定。 |
| macOS native standard buttons | AppKit native chrome | 本设计不承诺独立隐藏原生红黄绿按钮；native chrome 布局和能力由 macOS 平台路径维护。Pin 等确由 AtomUI 模板提供的辅助按钮仍遵循 managed visibility。 |
| Native decorations | 操作系统窗口装饰 | requested visibility 不伪装成跨平台原生按钮控制能力。 |
| Unsupported backend action | 无可靠操作能力 | 从 effective visibility 中移除对应 managed button。 |

平台层只发布能力和 native chrome 输入，不复制 requested visibility，也不成为 caption button 配置 owner。

### 4.3 原生窗口状态转换

Minimize、Maximize 和 Restore 使用 Avalonia `WindowState` 作为唯一跨平台状态入口。AtomUI 不直接发送 Win32 system command，不调用 AppKit、X11 或 Wayland 状态 API，也不使用控件 transition 模拟窗口收进任务栏或从任务栏恢复的动画。实际状态转换及可用动画由 Windows DWM、macOS AppKit 或 Linux 窗口管理器/合成器负责，并受平台能力和用户动画设置约束。

CSD 模式隐藏 AtomUI 默认标题栏时必须保持 `WindowDecorations.Full`。`WindowDrawnDecorationsTheme` 只隐藏 managed title-bar frame、shadow 和 presenter，并继续服从 Avalonia `WindowDrawnDecorations.HasTitleBar`。该边界既保留无重复标题栏的自定义 chrome，也保留平台执行原生窗口状态转换所需的完整装饰能力。macOS 非 CSD 隐藏原生标题栏仍属于独立的 `BorderOnly` 分支。

## 5. 架构与职责

| 类型或层 | 职责 | 不负责 |
| --- | --- | --- |
| `Window` | 注册 public visibility 属性；持有 `CanMinimize`、`CanMaximize`、`WindowState`、`Topmost` 和全屏恢复状态；执行窗口操作；发布平台能力；定义完整的 title-bar host projection 并创建可释放 lease。 | 不持有任意内容区标题栏的生命周期，不创建或遍历 caption button visual，不计算按钮布局。 |
| `WindowTitleBar` | 在逻辑树接入时发现最近的 AtomUI `Window`；独立持有 host projection lease；维护标题栏伪类、内容、布局输入和交互表面；把 caption 输入传给模板；保留生命周期受控的宿主关联供 detached popup 和 routed event 转发。 | 不定义 Window 到标题栏的投影字段集合，不直接读取 Window 状态，不调用平台窗口 API，不反向维护窗口状态。 |
| `CaptionButtonGroup` | 根据投影输入计算 effective visibility；向固定按钮投影 checked state、active state、motion state 和命令。 | 不持有 `HostWindow`，不查找逻辑祖先，不执行窗口操作。 |
| `CaptionButton` / `WindowsCaptionButton` | 呈现 icon、checked icon、active/inactive、hover、pressed 和 motion；通过命令提交一个固定窗口操作。 | 不解释窗口能力，不写入 `WindowState` 或 `Topmost`。 |
| Platform chrome manager | 发布 native chrome、backend 和操作支持能力。 | 不决定应用 requested visibility，不访问标题栏 template part。 |

Window action 的执行入口集中在 `Window` 内部窗口操作路径。public `Window.ToggleFullScreen()` 与 FullScreen caption button 进入同一入口；全屏状态下的 caption button 由迁入全屏标题栏宿主的同一标题栏实例提供，见 [Window 全屏标题栏设计](../window/full-screen-title-bar-design.md)。最小化、最大化/还原、全屏/退出、置顶/取消置顶和关闭使用同一 capability 检查，不因默认 caption button 是否可见而改变。

## 6. Template、组合与集成契约

AtomUI `Window` 为逻辑树内的 `WindowTitleBar` 提供统一宿主上下文。标题栏 attach 时由 `Window` 创建 host projection lease，并将以下输入单向投影到默认、派生或内容区 `WindowTitleBar`：

- 五个 caption button requested visibility 属性。
- `CanMinimize`、`CanMaximize` 和平台操作支持能力。
- `WindowState`、`Topmost`、active state 和 motion state。
- 由 `Window` 所有的 caption action command。

`Window.NotifyConfigureTitleBar` 只负责 Window 默认标题栏的标题、Logo、对齐和 add-on 内容配置，不承担通用宿主发现，也不成为 caption 状态投影的唯一入口。默认标题栏由 `Window` 的模板生命周期显式连接宿主；进入逻辑树后的重复连接保持幂等。应用直接放入 Window 内容区的 `WindowTitleBar` 通过同一 host projection 获得 caption 操作、空白区域拖动和双击最大化/还原；只有默认标题栏参与标题栏高度测量和 CSD geometry ownership。

`WindowTitleBarTheme` 再通过 `TemplateBinding` 把这些输入传给 `PART_CaptionButtonGroup`。`CaptionButtonGroupTheme` 使用 internal effective visibility 属性控制各按钮 `IsVisible`，并使用固定 command parameter 表达 Minimize、ToggleMaximize、ToggleFullScreen、TogglePin 和 Close。

`PART_CaptionButtonGroup` 保持 template-stable。其内部 `PART_MinimizeButton`、`PART_MaximizeButton`、`PART_FullScreenButton`、`PART_PinButton` 和 `PART_CloseButton` 仍属于 internal template contract，不是应用公共定制入口。

内置模板保持以下边界：

- Windows minimize、maximize 和 close button 保留对应 `WindowDecorationProperties.ElementRole`。
- Maximize 和 Restore、FullScreen 和 Exit、Pin 和 Unpin 分别复用同一个 button slot。
- 隐藏按钮从 Trailing 的实测宽度中消失，标题安全区通过正常 measure invalidation 重新计算。
- 业务操作不插入系统按钮序列，继续使用 `RightAddOn`。
- 完整替换 ControlTheme 或标题栏的应用负责保留同等 capability、visibility 和 action 语义。

## 7. 核心算法、数据流与生命周期

### 7.1 Effective visibility

设：

- `Vx` 为操作 `x` 的 requested visibility。
- `Cx` 为操作 `x` 的 capability。
- `F` 表示 `WindowState.FullScreen`。
- `M` 表示 `WindowState.Maximized`。

有效可见性为：

```text
MinimizeVisible   = Vminimize   && Cminimize   && !F
MaximizeVisible   = Vmaximize   && Cmaximize   && !F
FullScreenVisible = Vfullscreen && !M && (F || Cfullscreen)
PinVisible        = Vpin        && Cpin
CloseVisible      = Vclose      && Cclose
```

每个输出使用明确的 internal direct property 或等价只读模板状态。实现可以在一个集中方法中更新这些固定输出；只有实际消除重复时才使用内部 flags，不把 flags 暴露为 public configuration。

### 7.2 单向数据流

```text
Window public visibility + capability + WindowState + platform support
  -> Window creates title-bar host projection lease
  -> WindowTitleBar projected inputs
  -> CaptionButtonGroup effective visibility / checked state
  -> CaptionButton TemplateBinding
  -> measure, render and automation state
```

交互请求沿独立命令路径返回：

```text
CaptionButton command + fixed action parameter
  -> Window action executor
  -> WindowState / Topmost / close lifecycle
  -> forward projection refreshes caption visual state
```

该命令路径不把 template part 变成窗口状态 owner，也不让 visibility 参与能力写回。

### 7.3 生命周期

- `WindowTitleBar` 在连接逻辑树时发现最近的 AtomUI `Window`，并为该实例取得一个 host projection lease；离开逻辑树时释放 lease 和宿主引用。
- `AttachHost` 对相同 Window 幂等；宿主变化时先释放旧 lease，再从新 Window 创建完整投影，不能让标题栏同时消费两个宿主。
- 默认标题栏由 `Window` 模板生命周期显式连接宿主，因此在进入逻辑树前也能获得完整输入；随后逻辑树接入不得重复创建 binding。
- host projection lease 的字段集合、binding 方向和标题栏交互订阅由 `Window` 定义，lease 所有权属于接收投影的 `WindowTitleBar`。Window 不集中保存只能容纳单个标题栏的共享 binding 或事件容器。
- `CaptionButtonGroup` 不通过逻辑祖先发现 `Window`，不建立 Window 到 group 的 runtime relay binding，也不持有 host projection lease。
- `WindowTitleBar` 的宿主引用只用于 lease identity、detached popup、routed event 和同类标题栏级集成；caption capability、state 和 action 只能通过投影属性与命令进入模板。
- Caption button 的 `Command` 和 `CommandParameter` 由模板声明；template reapply 不注册或释放逐按钮 Click handler。
- Window 关闭、标题栏从逻辑树移除、移动到另一 Window 或默认标题栏被替换后，旧 lease 必须释放；template reapply 不重复建立 host projection。
- Windows pointer-over 修正由 `WindowsCaptionButton` 消费窗口状态输入并在自身生命周期内失效，不要求 group 缓存 template part 引用。

## 8. 资源、性能与 AOT 边界

- Caption visibility 使用静态注册的 Avalonia 属性、直接绑定和固定布尔计算，不使用反射、程序集扫描、运行时类型发现或动态注册。
- 宿主发现只遍历当前逻辑祖先，投影使用强类型 AvaloniaProperty binding；不使用字符串 path、运行时类型扫描或全局 Window registry。
- 每个标题栏只分配一个与宿主连接周期一致的固定大小 lease；状态变化不重建 lease，template reapply 不增加 Window binding 数量。
- 状态变化只更新固定数量的 effective properties，不创建 options 对象、descriptor collection、converter graph 或临时 Visual。
- Theme 使用 `TemplateBinding` 和既有 selector，不在状态变化时重建 caption button。
- 平台能力由显式 chrome manager 或 backend 枚举发布，不通过反射推断窗口管理器能力。
- Caption 配置不引入新的 DynamicResource、Token、timer、异步任务或非 Visual resource host。
- NativeAOT 下命令参数和操作映射使用封闭枚举或显式分支，不依赖动态成员访问。

## 9. 兼容性与定制边界

- 五个 public visibility 属性的类型、默认值和独立 Binding 语义属于稳定契约。
- `false` 始终表示隐藏对应 AtomUI managed button，不表示禁止同一窗口操作。
- capability 为 `false` 时按钮保持隐藏；visibility 为 `true` 不覆盖 capability。
- `IsMaximizeCaptionButtonVisible` 同时控制 Maximize 和 Restore 视觉 slot；FullScreen 和 Pin 使用相同 toggle-slot 语义。
- 原生 decorations 的按钮可见性由平台能力决定，不把 managed visibility 契约扩大为无法保证的 native chrome 契约。
- `PART_CaptionButtonGroup`、平台 caption button 顺序、Windows element role 和标题安全区测量语义保持稳定。
- 派生 `WindowTitleBar` 或完整自定义 ControlTheme 必须消费宿主投影或提供等价的 capability、visibility、checked state 和 action flow。
- public `WindowTitleBar` 直接放入 AtomUI `Window` 内容区时，caption 状态、操作、拖动和双击最大化必须自动关联最近宿主；该关联不授予标题栏高度提示或唯一 CSD chrome role。
- CSD 隐藏默认标题栏时保持 `WindowDecorations.Full`，只隐藏 AtomUI managed title-bar visual；窗口状态转换继续交给 Avalonia 和平台窗口管理器。
- 业务按钮和任意顺序定制不进入 caption button API；调用方使用 `LeftAddOn`、`RightAddOn` 或完整标题栏扩展点。

## 10. 验证要求

验证必须直接覆盖以下设计不变量：

- Public API：五个 visibility 属性均为 `StyledProperty<bool>`，默认值分别为 Minimize/Maximize/Close `true`、FullScreen/Pin `false`，并允许独立 Binding 和 Style 覆盖。
- 能力隔离：把 Minimize 或 Maximize visibility 设为 `false` 后，`CanMinimize`、`CanMaximize` 和非 caption 操作入口保持不变。
- Effective truth table：requested visibility、capability、Normal/Maximized/FullScreen 和 backend support 的组合符合第 4 节矩阵。
- 动态更新：模板应用前、模板应用后、标题栏替换后以及 WindowState 变化后都能重放正确 effective visibility。
- 宿主发现：内容区标题栏、默认标题栏和同一 Window 内多个标题栏分别获得同一 Window 的完整投影与交互订阅；嵌套内容始终选择最近的 AtomUI Window。
- 宿主切换：标题栏从 Window A 移除并接入 Window B 后只跟随和操作 Window B；旧 Window 不再被标题栏或 binding lease 保留。
- 双击路径：真实 pointer 双击在 release 阶段触发 Normal/Maximized 切换；detach 后同一标题栏不再操作旧 Window；默认标题栏不得因显式和 logical attach 重复订阅而切换两次。
- 原生状态能力：CSD 且 `IsTitleBarVisible=false` 时三平台保持 `WindowDecorations.Full`；drawn title-bar visual 同时服从 `HasTitleBar` 和 `IsTitleBarVisible`。
- 命令路径：隐藏按钮不改变窗口操作命令的 capability；可见按钮不会在 capability 为 `false` 时执行操作。
- Theme contract：三平台模板保持稳定 part、Windows element role、toggle icon 和按钮顺序；按钮隐藏后 Trailing 宽度与标题安全区重新测量。
- 生命周期：logical detach、Window close、template reapply 和标题栏替换不保留旧 Window 引用、Click handler 或重复 projection binding。
- 平台验证：Windows 验证最小化进入任务栏、任务栏恢复、最大化/还原和 snap role；Linux 验证窗口管理器状态转换、managed visibility 和 backend capability；macOS 验证 AppKit 状态转换及 native standard button 边界未被错误承诺。
- AOT：Desktop Controls 测试和适用的 NativeAOT 发布路径不产生新的反射或 trimming 风险。
