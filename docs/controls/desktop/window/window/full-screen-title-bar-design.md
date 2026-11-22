# Window 全屏标题栏设计

本文档定义 `Window` 在 `WindowState.FullScreen` 下的标题栏呈现模型、公共契约、平台策略、内部职责、显隐算法和验证要求。Window 的整体公共契约见 [Window 桌面版架构设计](overview.md)，源码职责与生命周期见 [Window 桌面版实现原理](implementation.md)，标题栏自身的布局与 caption 模型见 [WindowTitleBar 桌面版架构设计](../window-title-bar/overview.md) 和 [WindowTitleBar Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md)，相关 Token 见 [Window Token 设计](token.md)。

## 1. 设计定位

全屏标题栏是 Window 在全屏状态下承载标题、Logo、`LeftAddOn`、`RightAddOn` 与 caption buttons 的唯一 managed 表面。它回答三个问题：

- 全屏时标题栏是否出现、以何种方式出现。
- 全屏时标题栏展示什么内容。
- 标题栏的显隐由哪些输入驱动，由谁拥有。

该设计覆盖 Windows、Linux X11、Linux Wayland 和 macOS 四个桌面后端，以及它们对应的 CSD、非 CSD 和原生 chrome 窗口路径，并让它们共用同一套模式语义、内容来源和显隐状态机。

本文中的“默认标题栏”指 Window 模板生命周期拥有的那个 `WindowTitleBar` 实例，包括派生窗口通过 `NotifyCreateTitleBar` 提供的派生标题栏；ImagePreviewer 等内置派生窗口遵循同一模型。

全屏标题栏不负责进入或退出全屏的窗口状态转换，不负责普通窗口状态下的标题栏布局，也不承载 Dialog、Drawer、Notification 等业务 presentation。

## 2. 设计原则

- 单一 owner：全屏标题栏的模式、内容来源、打开状态和内容区占位全部由 `Window` 拥有。Avalonia `WindowDrawnDecorations` 的全屏 popover 机制不参与 AtomUI 全屏标题栏，AtomUI 不向该机制提供内容，也不读取其内部状态。
- 同一实例：默认内容来源下，全屏标题栏展示的就是 Window 的默认 `WindowTitleBar` 实例本身，而不是另一份近似模板。标题、Logo、add-on、caption 配置、命令和主题在普通状态与全屏状态之间没有第二份实现。
- 模式语义与呈现者跨平台一致：`Hidden`、`AutoHide`、`Pinned`、`Docked` 在所有平台表达相同的应用意图，并且都由 AtomUI 全屏标题栏宿主呈现。平台差异只以几何输入进入模型：平台 chrome 当前占据的客户区顶部范围，以及平台原生控件在表面上的占位。
- 与平台 chrome 融合而不与之竞争：平台在全屏下自行呈现顶部 chrome 时，全屏标题栏随其位置移动，并与平台的原生标题条合为一条——原生窗口按钮直接呈现在全屏标题栏的前导区域内，与普通窗口状态下的标题栏形态一致。AtomUI 不抑制、不替换平台 chrome，也不让两者互相遮挡。
- 显隐是可测试的纯状态机：pointer、focus、popup、显式请求和计时器全部作为状态机输入，状态机不持有 Visual，不直接读写平台 API。
- 持有以 lease 表达：任何需要让全屏标题栏保持打开的协作方通过可释放 lease 声明意图，不通过轮询、延时猜测或吞掉 pointer 事件实现。
- 以 `WindowState` 为唯一全屏事实：进入和退出全屏既可能由 AtomUI 发起，也可能由平台发起（窗口管理器快捷键、合成器、macOS 原生全屏按钮与系统手势）。全屏标题栏只响应平台确认后的 `WindowState`，不假设状态转换同步完成，也不假设转换一定成功。
- 全屏表面不声明窗口移动与缩放语义：chrome role 命中测试覆盖整个窗口视觉树，且不区分窗口状态，因此全屏表面内不出现标题栏拖拽 role 或 resize role。
- 占位只有一个几何出口：全屏标题栏对内容区的占位通过 Window 发布的单一 inset 进入模板，模板和上层控件不复制该计算。

## 3. 专项模型与 Public API

### 3.1 术语

| 术语 | Owner | 语义 |
| --- | --- | --- |
| Requested mode | `Window` public API | 应用请求的全屏标题栏呈现模式。 |
| Effective mode | `Window` | requested mode 经 `WindowState`、内容来源和平台能力解析后的实际模式。 |
| Content source | `Window` | 全屏标题栏展示的内容：默认标题栏或应用提供的自定义内容。 |
| Title-bar slot | `Window` | 能够承载默认 `WindowTitleBar` 的模板位置。存在 Normal slot 与 FullScreen slot 两个，同一时刻至多一个持有该实例。 |
| Open state | `Window` | 全屏标题栏表面当前是否呈现。 |
| Reveal source | 显隐状态机 | 使表面从关闭变为打开的输入。 |
| Hold | 显隐状态机 | 使已打开的表面保持打开的输入，以 lease 或瞬时条件表达。 |
| Trigger band | 显隐状态机 | 窗口客户区顶部用于触发 reveal 的水平带。 |
| Keep zone | 显隐状态机 | pointer 位于其中时表面保持打开的区域。 |
| Dock inset | `Window` | `Docked` 模式下全屏标题栏对内容区顶部的占位高度。 |
| Native chrome extent | 平台 chrome manager | 全屏状态下平台 chrome 中不能与全屏标题栏重叠的部分当前占据的客户区顶部高度，随平台 chrome 的呈现与收起实时变化。平台不呈现顶部 chrome 时为零。 |
| Native control inset | 平台 chrome manager | 平台原生控件（原生窗口按钮）当前叠放在全屏标题栏表面之上所占据的前导或尾随宽度。原生控件未呈现时为零。 |
| Title-strip fusion | 平台 chrome manager | 平台的原生标题条被处理为透明并与全屏标题栏表面重叠的状态。成立时原生标题条不计入 native chrome extent。 |
| Persistent native inset | 平台 chrome manager | 全屏状态下平台常驻 chrome 占据的客户区顶部高度，是 native chrome extent 中不随 pointer 变化的部分。 |

### 3.2 呈现模式

`FullScreenTitleBarMode` 是封闭枚举：

| 值 | 是否呈现 | 是否自动收起 | 是否占用内容布局 | 语义 |
| --- | --- | --- | --- | --- |
| `Hidden` | 否 | 不适用 | 否 | Window 不提供任何 managed 全屏标题栏。应用自行负责全屏下的全部 chrome 与退出入口。 |
| `AutoHide` | 按需 | 是 | 否 | 表面默认关闭，由 reveal source 打开并覆盖在内容之上，hold 结束后自动收起。 |
| `Pinned` | 是 | 否 | 否 | 表面常驻并覆盖在内容之上，内容区尺寸与 `AutoHide` 相同。 |
| `Docked` | 是 | 否 | 是 | 表面常驻并位于内容区上方，内容区从表面下沿开始布局。 |

`AutoHide` 与 `Pinned` 之间切换不改变内容区尺寸，适合“图钉”类交互。`Docked` 用于全屏下仍需要常规标题栏布局的应用。

### 3.3 Window Public API

| API | 类型 | 默认值 | 语义 |
| --- | --- | --- | --- |
| `FullScreenTitleBarMode` | `StyledProperty<FullScreenTitleBarMode>` | `AutoHide` | 全屏标题栏的 requested mode。 |
| `IsFullScreenTitleBarOpen` | `StyledProperty<bool>`，默认 `TwoWay` | `false` | 全屏标题栏的 open state。读取得到实际呈现状态；写入表达应用的显式打开或关闭请求。值受 effective mode 强制约束，见 3.4。 |
| `FullScreenTitleBar` | `StyledProperty<object?>` | `null` | 自定义全屏标题栏内容。非 `null` 时替代默认标题栏成为 content source。 |
| `FullScreenTitleBarTemplate` | `StyledProperty<IDataTemplate?>` | `null` | 自定义内容的模板。非 `null` 时同样使 content source 变为自定义。 |
| `FullScreenTitleBarMouseEnterDelay` | `StyledProperty<int>` | `150` | pointer 进入 trigger band 到表面打开之间的停留时间，单位毫秒。 |
| `FullScreenTitleBarMouseLeaveDelay` | `StyledProperty<int>` | `300` | 最后一个 hold 结束到表面收起之间的延迟，单位毫秒。 |
| `ToggleFullScreen()` | 方法 | 不适用 | 进入或退出全屏。退出时恢复进入全屏前的 `WindowState`，与 FullScreen caption button 共用同一个窗口操作入口和 capability 检查。 |

延迟属性沿用 AtomUI 既有的 `MouseEnterDelay` / `MouseLeaveDelay` 命名与毫秒整数约定。负值按 `0` 处理。

自定义内容通过属性元素配置：

```xml
<atom:Window FullScreenTitleBarMode="AutoHide">
  <atom:Window.FullScreenTitleBar>
    <local:SessionToolBar />
  </atom:Window.FullScreenTitleBar>
</atom:Window>
```

自定义内容位于 Window 逻辑树内，其中放置的 `WindowTitleBar` 按内容区标题栏语义自动获得宿主投影，可直接复用 caption 状态与窗口操作命令。

### 3.4 Open state 的强制约束

`IsFullScreenTitleBarOpen` 的最终值由 effective mode 强制：

| Effective mode | 强制结果 | 应用写入的效果 |
| --- | --- | --- |
| 非全屏 | 恒为 `false` | 写入被忽略。 |
| `Hidden` | 恒为 `false` | 写入被忽略。 |
| `AutoHide` | 由显隐状态机决定 | 写入 `true` 立即打开并建立显式 hold；写入 `false` 立即关闭并释放显式 hold。 |
| `Pinned` / `Docked` | 恒为 `true` | 写入被忽略。 |

强制通过属性 coercion 实现，绑定源始终观察到强制后的值。

### 3.5 Content source 解析

| 条件 | Content source | 说明 |
| --- | --- | --- |
| `FullScreenTitleBar` 或 `FullScreenTitleBarTemplate` 非 `null` | 自定义内容 | 默认标题栏留在 Normal slot，不进入全屏表面。 |
| 两者均为 `null` 且 `IsTitleBarVisible=true` | 默认标题栏 | 默认 `WindowTitleBar` 实例迁入 FullScreen slot。 |
| 两者均为 `null` 且 `IsTitleBarVisible=false` | 无 | 没有可呈现内容，effective mode 解析为 `Hidden`。 |

`IsTitleBarVisible` 表达应用是否使用 Window 的默认标题栏，因此在没有自定义内容时同时约束全屏表面。

## 4. 平台与状态策略

### 4.1 Effective mode 解析

解析按以下顺序进行，命中即停止：

1. `WindowState != FullScreen`：全屏标题栏不活动，默认标题栏位于 Normal slot。
2. Requested mode 为 `Hidden`：`Hidden`。
3. Content source 为“无”：`Hidden`。
4. 其他情况：等于 requested mode。

Effective mode 的解析不包含平台分支。

### 4.2 平台矩阵

四个后端进入全屏的机制不同，但对 Window 暴露的事实一致：`WindowState` 变为 `FullScreen`，客户区覆盖整个显示器，普通状态下的标题栏不再呈现。

| 后端 | 窗口路径 | 全屏机制 | 状态确认 | 普通状态下默认标题栏位置 |
| --- | --- | --- | --- | --- |
| Windows | CSD（始终） | 无边框窗口覆盖所在显示器；扩展客户区请求保持，drawn decorations 基础设施保持存活但不启用任何部件 | 同步 | drawn decorations overlay |
| Linux X11 | CSD（AtomUI 始终请求扩展客户区） | 向窗口管理器请求 EWMH 全屏状态 | 异步，由窗口管理器确认，可被拒绝 | drawn decorations overlay |
| Linux Wayland | CSD 或非 CSD，取决于合成器协商的装饰模式，运行期可切换 | 向合成器请求 `xdg_toplevel` 全屏 | 异步，由合成器 configure 确认，可被拒绝 | CSD：drawn decorations overlay；非 CSD：Window 模板标题栏区域 |
| macOS | 原生 chrome（始终非 CSD） | AppKit 原生全屏，窗口进入独立 Space，带系统转场动画 | 异步，转场结束后确认 | Window 模板标题栏区域 |

四种模式在四个后端上都由 AtomUI 全屏标题栏宿主呈现，effective mode 的解析与平台无关。平台差异只通过平台 chrome manager 发布的三个几何输入进入模型：

| 后端 | 全屏下平台是否自行呈现顶部 chrome | Native chrome extent | Native control inset | Persistent native inset |
| --- | --- | --- | --- | --- |
| Windows | 否 | 恒为零 | 恒为零 | 恒为零 |
| Linux X11 | 否 | 恒为零 | 恒为零 | 恒为零 |
| Linux Wayland | 否 | 恒为零 | 恒为零 | 恒为零 |
| macOS | 是：pointer 到达屏幕顶边时系统菜单栏与原生标题条以覆盖方式滑入 | 随系统 chrome 滑动连续变化；融合成立时上限为菜单栏高度，否则为菜单栏与原生标题条之和 | 融合成立且系统 chrome 呈现时为原生窗口按钮的前导占位，否则为零 | 系统 chrome 临时呈现时为零；常驻时为其高度 |

全屏标题栏宿主、显隐状态机、slot 迁移和 Dock inset 不按 `OsType` 分支。

### 4.3 平台语义

**Windows**

- 全屏窗口的客户区从显示器顶边开始，pointer 到达顶边时被屏幕边界截停，trigger band 的事件按普通客户区输入送达。
- drawn decorations 在全屏下不启用任何部件，underlay 与 overlay 被隐藏；平台不自行呈现顶部 chrome，三个几何输入恒为零。
- 标题栏中的 Windows caption button 保留各自的窗口操作 role，与内容区标题栏的行为一致；全屏下最小化与最大化按钮按 caption 矩阵隐藏。

**Linux X11**

- 全屏是否成立由窗口管理器决定。不支持 EWMH 全屏的窗口管理器下 `WindowState` 不会变为 `FullScreen`，全屏标题栏保持不活动。
- 用户通过窗口管理器快捷键进入或退出全屏时，Window 只观察到 `WindowState` 变化，全屏标题栏按相同流程启停。
- 标题栏拖拽 role 在按下时直接向窗口管理器发起移动，不检查窗口状态；全屏表面内不得出现该 role。

**Linux Wayland**

- 全屏由合成器 configure 确认；合成器可以拒绝请求，也可以主动让窗口进入或退出全屏。
- 装饰模式由合成器协商并可在运行期改变。模式变化使 Window 的 CSD 状态和模板随之切换；切换发生在全屏期间时，按模板重套用流程回放全屏标题栏状态，Normal slot 的位置随新模板确定。
- pointer 离开窗口所在输出时合成器发送 leave，对应 pointer hold 结束。触摸按下同样参与 chrome role 命中，全屏表面内不得出现拖拽 role。

**macOS**

平台事实：

- 全屏状态下窗口客户区覆盖整个屏幕，AtomUI 普通标题栏不呈现，平台不保留标题栏占位。
- pointer 到达屏幕顶边时，AppKit 以覆盖方式滑入系统菜单栏和带原生窗口按钮的原生标题条，内容区不重排。原生标题条由独立的系统窗口承载，不属于 Avalonia 客户区；pointer 停留在系统 chrome 及其下方邻近区域时系统保持呈现，继续下移后系统滑出收起。
- 承载原生标题条的系统窗口在滑入与滑出的每一步都发出窗口移动通知，其位置可通过原生窗口按钮所属窗口这一公开关系取得。
- 原生标题条默认带有不透明的背景材质和下沿阴影，窗口的标题栏透明属性在全屏下不能去除它们。隐藏原生标题条的背景视图和阴影图层后，标题条区域完全透明，只留下原生窗口按钮。
- 系统窗口非不透明：其透明区域的 pointer 输入穿透到下方的客户区，原生窗口按钮所在位置的输入仍由系统接收。
- AppKit 拥有原生窗口按钮的 frame：原生标题条滑动期间它在每一帧重新布局按钮（其中全屏按钮每帧都会被复位），直接修改按钮 frame 会与 AppKit 逐帧冲突。偏移按钮所在标题条视图的 bounds 原点可以整体平移三个按钮，AppKit 不复位该原点。
- 全屏期间平台忽略窗口装饰类型的变更，退出全屏后才应用。
- 用户通过原生全屏按钮、菜单或系统手势进入和退出全屏时，Window 只观察到 `WindowState` 变化。
- 有 owner 的窗口被平台标记为全屏辅助窗口，AppKit 不会把它带入原生全屏，平台也不报告这一限制。macOS chrome manager 因此对有 owner 的窗口报告不支持全屏：Window 拒绝该窗口的全屏请求（`WindowState` 保持原值），全屏 caption button 不显示进入入口，全屏标题栏保持不活动。需要全屏的窗口必须不设置 owner。

呈现模型（title-strip fusion 成立时）：

- 系统 chrome 滑入时，全屏标题栏表面的顶边位于系统菜单栏下沿，并随菜单栏同步下移；原生标题条透明地叠放在表面顶部，原生窗口按钮呈现在表面的前导区域内。自上而下只有两层：系统菜单栏、全屏标题栏。
- 系统 chrome 滑出时，表面随之上移到屏幕顶边，原生窗口按钮随系统窗口一同离开，native control inset 归零，表面前导区域恢复给标题栏内容。
- 原生窗口按钮作为一组在表面高度内垂直居中，前导 x 与普通窗口状态一致（`MacStandardWindowButtons.OffsetX`，未设置时取普通状态下的实际位置）；按钮之间的间距保持 AppKit 的原生值，`MacStandardWindowButtons.Spacing` 只作用于普通窗口状态。
- 表面内被透明标题条覆盖的区域照常接收 pointer 输入；原生窗口按钮的点击由系统处理，其关闭、最小化与退出全屏语义不经过 AtomUI。
- 默认内容下，`WindowTitleBar` 的 native chrome inset 取 native control inset，前导内容为原生窗口按钮让出安全区，规则与普通窗口状态相同。自定义内容下，宿主在内容 presenter 上以同一 inset 作为内边距。
- `AutoHide` 下，系统 chrome 呈现本身是 reveal source 与 hold。pointer 进入表面后由 pointer hold 接续，从表面打开的弹层由 popup hold 接续；系统 chrome 收起后表面停留在屏幕顶边，直到所有 hold 结束。
- `Pinned` 与 `Docked` 的表面平时位于屏幕顶边且不带原生窗口按钮；系统 chrome 呈现期间表面下移一个菜单栏高度并出现原生窗口按钮。`Docked` 的内容区占位在此期间不变。

融合不成立时的退化（堆叠呈现）：

- 无法按预期结构定位原生标题条的背景视图与阴影图层时，不修改系统窗口的任何视图。
- 原生标题条保持平台默认外观并计入 native chrome extent，全屏标题栏表面位于原生标题条正下方；native control inset 为零。
- 两种呈现共用同一套模式语义、状态机与 hold，只有三个几何输入的取值不同。

其他配置：

- 系统设置为全屏时常驻显示菜单栏的配置下，系统 chrome 不随 pointer 滑入滑出，其高度由 persistent native inset 表达，见 7.2。
- 窗口没有原生窗口按钮时无法定位承载原生标题条的系统窗口，native chrome extent 退化为零，见 7.2。

### 4.4 与 Avalonia drawn decorations 的边界

CSD 路径中，Avalonia 在全屏时移除 drawn title-bar 部件并隐藏 decorations 的 underlay 与 overlay。AtomUI 对该机制的契约为：

- `WindowDrawnDecorationsTheme` 不提供 `WindowDrawnDecorationsContent.FullscreenPopover` 内容。
- AtomUI 不命名任何会被 Avalonia 按约定名称接线的全屏按钮 part，全屏下的窗口操作只经过 Window 的 caption command。
- AtomUI 不通过反射读取或修改 decorations 的全屏显隐状态。
- chrome role 命中测试覆盖包含窗口内容在内的整个视觉树，全屏标题栏宿主虽然不在 decorations 层内，其子树中的 role 仍然生效。宿主子树只允许出现 caption button 的窗口操作 role，不允许出现 `TitleBar` role 与 resize role。

## 5. 架构、文件结构与职责

```text
src/AtomUI.Desktop.Controls/Window/
├── FullScreenTitleBarMode.cs
├── Window.FullScreenTitleBar.cs
├── Chrome/
│   └── *WindowChromeManager.cs
├── Utils/
│   ├── FullScreenTitleBarHost.cs
│   └── FullScreenTitleBarRevealController.cs
└── Themes/
    └── FullScreenTitleBarHostTheme.axaml

src/AtomUI.Native/
├── NativeFullScreenChrome.cs
└── MacOS/
    └── WindowUtils.FullScreenChrome.MacOS.cs
```

`NativeFullScreenChrome.cs` 定义平台无关的契约：`FullScreenNativeChromeMetrics`（三个几何输入）、`FullScreenTitleStripFusion`（融合开关、表面高度与按钮前导 x）与 `IFullScreenNativeChrome`。平台 chrome manager 通过 `AttachFullScreenNativeChrome` 返回该接口的实例；Windows 与 Linux 返回 `null`，表示平台在全屏下不自行呈现顶部 chrome。

| 类型 | 可见性 | 职责 | 不负责 |
| --- | --- | --- | --- |
| `FullScreenTitleBarMode` | public | 封闭的呈现模式枚举。 | 不表达平台差异。 |
| `Window`（`Window.FullScreenTitleBar.cs`） | public | 注册 3.3 的属性；解析 content source 与 effective mode；强制 open state；仲裁两个 title-bar slot；发布 Dock inset；签发 hold lease；向状态机提供窗口级输入。 | 不计算 pointer 命中，不持有计时器，不绘制表面。 |
| `FullScreenTitleBarRevealController` | internal | 显隐状态机。接收 pointer、focus、hold、显式请求、抑制和时钟输入，输出 open state 请求。不依赖 Visual 与平台 API，可用注入时钟做纯逻辑测试。 | 不决定 effective mode，不迁移标题栏实例，不执行动效。 |
| `FullScreenTitleBarHost` | internal | 模板中的全屏标题栏表面。承载 FullScreen slot 与自定义内容；把自身几何提供给状态机作为 keep zone；执行打开与收起动效；报告实测高度。 | 不持有 open state，不解析模式，不直接读取 `WindowState`。 |
| 平台 chrome manager（`Window/Chrome`）与 `AtomUI.Native` | internal | 全屏期间发布 native chrome extent、native control inset 与 persistent native inset。macOS manager 通过 `AtomUI.Native` 的原生通知观察能力跟踪系统 chrome，并由 `AtomUI.Native` 执行 title-strip fusion 的识别、隐藏、按钮布局与恢复；Windows 与 Linux manager 发布零。 | 不决定模式与显隐，不访问全屏标题栏宿主。 |
| `WindowTitleBar` | public | 作为默认内容在两个 slot 之间迁移，保持自身布局、caption 与主题语义。 | 不感知自己位于哪个 slot，不参与显隐决策。 |

Window 是唯一同时了解模式、内容来源和窗口状态的对象；宿主与状态机各自只消费 Window 投影的输入，彼此不直接引用。

## 6. Template、组合与集成契约

### 6.1 模板结构

三个 Window 模板（Linux 非 CSD、macOS 非 CSD、CSD）都包含同名的全屏标题栏宿主 `PART_FullScreenTitleBarHost`，在模板中声明于 `PART_VisualLayerManager` 的内容根之内、窗口内容之上；模板声明位置不变，运行时位置是 `WindowChromeLayer`（全局层级阶梯的 T6，见 [视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)）内的表面宿主：Window 在 `OnApplyTemplate` 中经统一注入入口向该模板的 `PART_VisualLayerManager` 注入一个新的 `WindowChromeLayer`，再把宿主从模板父级迁入它的表面宿主。模板重套用与 Window 关闭时，chrome 层被释放（表面宿主清空，宿主离开该层，不回到模板父级）；Avalonia 的 `VisualLayerManager` 没有移除层的 API，清空后的层随旧模板的 `VisualLayerManager` 一起丢弃，本设计不为此新增反射。模板中的声明（模板声明位置）：

```text
Window
  -> VisualLayerManager#PART_VisualLayerManager (template-stable)
     -> 内容根
        -> 窗口内容（CSD 模板中为 Panel#PART_ContentCacheHost）
        -> FullScreenTitleBarHost#PART_FullScreenTitleBarHost (template-stable，运行时被 Window 迁出)
           -> Border#PART_Surface (template-stable)
              -> ContentPresenter#PART_ContentPresenter (template-stable)
```

运行时的有效位置（迁移之后）：

```text
Window
  -> VisualLayerManager#PART_VisualLayerManager
     -> 内容根 / 窗口内容 / 共享 AdornerLayer（T2）/ T3–T5 层 …
     -> WindowChromeLayer（T6，internal，透明，自身不参与命中）
        -> VisualLayerManager（表面宿主，IsSurfaceHost=true，只启用 AdornerLayer）
           -> FullScreenTitleBarHost#PART_FullScreenTitleBarHost
     -> TopLevelPresentationLayer（T7）/ TourLayer / Avalonia OverlayLayer … / WindowFeedbackLayer（T8）
```

该位置同时满足以下层级要求：

- 宿主位于窗口内容、共享 `AdornerLayer`（Badge、焦点描边）、Watermark、吸顶 Tab、FloatButton 和作用域 Drawer / Dialog 之上，`AutoHide` 与 `Pinned` 的表面可以覆盖它们。
- 宿主位于 TopLevel Drawer（含 Window Drawer）、Tour、Dialog、overlay popup 和 Message / Notification 之下，从全屏标题栏打开的 overlay popup、Dialog mask 和 Drawer 始终位于表面之上；标题栏打开的 Dialog 仍解析到窗口的 `OverlayLayer`。
- 宿主所在的表面宿主自带 `AdornerLayer`：标题栏按钮的键盘焦点描边与标题栏内的 Badge 画在 chrome 层自己的装饰层中，不会被其他层盖住。
- 宿主位于 `PART_ContentCacheHost` 之外，覆盖层内容缓存不包含全屏标题栏表面。
- `WindowChromeLayer` 铺满 `VisualLayerManager`；迁移前后宿主在窗口坐标中的位置保持一致（全屏状态下表面顶端的窗口坐标 Y 值不变）。宿主原先作为 `VisualLayerManager` 的 Child 被 `Padding`（`OffScreenMargin`）内缩，而注入层按整个 `VisualLayerManager` 排列，所以 `WindowChromeLayerTheme.axaml` 把父 `VisualLayerManager` 的 `Padding` 投影为 chrome 层的 `Margin`，由 ControlTheme 还原同一个内缩矩形。`WindowVisualLayerClip` 是 `PART_VisualLayerManager` 的祖先，chrome 层仍在它的裁剪之内；`FrameShadowThickness` 只作用于内容的 `WindowContentClip`，宿主在模板中本就不在其内，迁移不需要还原它。

### 6.2 Title-bar slot

| Slot | 位置 | 持有默认标题栏的条件 |
| --- | --- | --- |
| Normal slot | CSD：`WindowDrawnDecorationsTheme` 的 `PART_TitleBarPresenter`；非 CSD：Window 模板标题栏区域的标题栏 presenter | 除 FullScreen slot 条件以外的所有情况。 |
| FullScreen slot | `FullScreenTitleBarHost` 的 `PART_ContentPresenter` | effective mode 为 `AutoHide`、`Pinned` 或 `Docked`，且 content source 为默认标题栏。 |

Window 分别向两个 slot 发布内容，同一 `WindowTitleBar` 实例不会同时出现在两个 presenter 中。Content source 为自定义内容时，FullScreen slot 展示 `FullScreenTitleBar` 与 `FullScreenTitleBarTemplate`，默认标题栏保持在 Normal slot。

### 6.3 默认标题栏在 FullScreen slot 中的语义

- 标题栏保持 `:fullscreen` 伪类，caption buttons 按 [Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md) 的 FullScreen 列计算 effective visibility。
- Logo、标题、`LeftAddOn` 和 `RightAddOn` 按标题栏自身规则呈现，全屏状态不额外抑制 Logo。
- 标题栏的 native chrome inset 在全屏下取 Window 投影的 native control inset：平台原生控件叠放在表面上时为其占位，否则为零。
- 标题栏空白区域不发起窗口拖动，双击不触发最大化或还原。该约束由 Window 的标题栏交互入口按 `WindowState` 保证，并要求标题栏及其 add-on 不携带 `TitleBar` role。
- 标题栏的宿主投影 lease 保持有效；标题栏高度提示与 CSD chrome 几何协作在位于 FullScreen slot 期间暂停，回到 Normal slot 后恢复。

### 6.4 稳定契约与定制责任

- `PART_FullScreenTitleBarHost`、`PART_Surface` 和宿主内的 `PART_ContentPresenter` 是 template-stable part。
- 完整替换 Window ControlTheme 的应用仍必须在模板中声明 `PART_FullScreenTitleBarHost`（位置见 6.1 的模板声明位置），宿主随后由 Window 在运行时迁入 `WindowChromeLayer`，应用不需要也不应自行放置 chrome 层；缺少该 part 时 effective mode 解析为 `Hidden`。宿主迁移后 `Mode`、`IsOpen`、`SurfaceMargin`、`Padding`、`Content`、`ContentTemplate` 的 `TemplateBinding` 契约保持不变。
- 需要改变全屏标题栏组成的应用使用 `FullScreenTitleBar`；需要完全自管全屏 chrome 的应用使用 `Hidden`。两者都不要求替换 Window 模板。
- 表面的背景由内容自身绘制。默认标题栏沿用 `TitleBarFrameBackground` 投影与标题栏主题背景；宿主只提供覆盖模式下的阴影。

## 7. 核心算法、数据流与生命周期

### 7.1 数据流

```text
FullScreenTitleBarMode + WindowState + content source + platform path
  -> Window: effective mode
  -> slot 仲裁（默认标题栏迁移）+ RevealController 启停 + Dock inset

平台 chrome manager: native chrome extent + native control inset + persistent native inset
  -> Window: 表面顶边位置 + 表面前导安全区 + Dock inset + native chrome hold

pointer / focus / hold lease / native chrome hold / 显式请求 / 抑制 / 时钟
  -> RevealController: open state 请求
  -> Window: IsFullScreenTitleBarOpen（coercion）
  -> FullScreenTitleBarHost: 表面可见性与动效
```

状态只沿该方向流动。宿主不写回模式或 open state，状态机不读取模板节点。

### 7.2 坐标与几何

所有几何使用 Window 客户区的设备无关像素，原点位于客户区左上角。

| 区域 | 定义 |
| --- | --- |
| Trigger band | `y ∈ [0, 2]` 的全宽水平带；触摸与笔输入为 `y ∈ [0, 12]`。 |
| Keep zone | 宿主表面的实际边界，下沿向外扩展 `8`。 |
| 表面高度 | 宿主的实测高度。默认内容下等于默认标题栏高度。 |

表面高度为零或非有限值时，trigger band 仍然有效，keep zone 退化为 trigger band。

平台 chrome manager 发布三个几何输入，单位与坐标系同上：

| 输入 | 含义 | 取值 |
| --- | --- | --- |
| Native chrome extent `E` | 平台 chrome 中不能与表面重叠的部分当前占据的客户区顶部高度 | Windows 与 Linux 恒为零。macOS 在融合成立时取原生标题条上沿在客户区坐标系中的 `y` 值（即系统菜单栏下沿），融合不成立时取原生标题条下沿的 `y` 值；随系统窗口的移动通知更新。 |
| Native control inset `L` | 平台原生控件叠放在表面上所占据的前导与尾随宽度 | Windows 与 Linux 恒为零。macOS 在融合成立且 `E > P` 时取原生窗口按钮组的外沿到表面前导边的距离，否则为零。 |
| Persistent native inset `P` | 平台常驻 chrome 占据的客户区顶部高度 | Windows 与 Linux 恒为零。macOS 取窗口内容布局矩形相对窗口顶边的偏移。 |

全屏标题栏表面的顶边位于 `y = max(E, P)`，trigger band 与 keep zone 随表面整体平移。`E` 的变化直接作用于表面位置，不叠加额外动效，使表面与平台 chrome 同步移动。`L` 作为标题栏的 native chrome inset 或自定义内容的内边距参与表面内部布局，不改变表面的外部几何。

Title-strip fusion 的成立条件与操作：

- 承载原生标题条的系统窗口通过原生窗口按钮所属窗口取得，且不是 Window 自身的原生窗口。
- 原生窗口按钮的父视图是标题条视图；其同级子视图中恰有一个承载背景材质的背景视图。
- 条件满足时隐藏：该背景视图；标题条容器中除标题条视图以外的装饰视图；系统窗口内容视图中除标题条容器以外仍可见的图层（标题条下沿的阴影）。随后偏移标题条视图的 bounds 原点，使按钮组在表面高度内垂直居中并对齐普通状态的前导 x。任一条件不满足时不做任何修改，按堆叠呈现发布输入。
- 上述识别只依据视图与图层的父子关系和公开类型（是否为视觉效果视图），不按私有类名匹配。
- 融合期间每次系统窗口移动都会复核：被隐藏的元素若被 AppKit 重新显示则再次隐藏，bounds 原点偏差超过半个点时重新下发。
- 退出全屏、融合被关闭或 Window 关闭时，恢复被隐藏的视图与图层的可见性，并把标题条视图的 bounds 原点复位。

退化规则：

- 取不到承载原生标题条的系统窗口（例如窗口没有原生窗口按钮）时，`E` 与 `L` 均为零：表面停在屏幕顶边，系统菜单栏临时覆盖其上缘。
- 任一输入无法取得、为负或非有限时按零处理。
- 非全屏状态下三个输入都不被消费，平台不为其保留观察者。

### 7.3 显隐状态机

状态机只在 effective mode 为 `AutoHide` 时运行，包含四个状态：

| 状态 | 表面 | 含义 |
| --- | --- | --- |
| `Closed` | 关闭 | 等待 reveal source。 |
| `Opening` | 关闭 | pointer 位于 trigger band，enter delay 计时中。 |
| `Open` | 打开 | 至少存在一个 hold，或尚未开始 leave delay。 |
| `Closing` | 打开 | 所有 hold 已结束，leave delay 计时中。 |

Reveal source：

| 输入 | 效果 |
| --- | --- |
| 鼠标 pointer 进入 trigger band | `Closed -> Opening`；停留满 `FullScreenTitleBarMouseEnterDelay` 后 `-> Open`；计时结束前离开则回到 `Closed`。 |
| 触摸或笔在 trigger band 内按下并向下移动不少于 `16` | 立即 `-> Open`。 |
| 应用写入 `IsFullScreenTitleBarOpen=true` | 立即 `-> Open`，并建立显式 hold。 |
| Native chrome extent 由零变为正值 | 立即 `-> Open`，并建立 native chrome hold。不经过 enter delay：平台已经完成了触发判定。 |

Hold：

| Hold | 建立 | 结束 |
| --- | --- | --- |
| Pointer hold | pointer 位于 keep zone 内。 | pointer 离开 keep zone，或 pointer 离开窗口。 |
| Capture hold | 宿主后代持有 pointer capture。 | capture 释放或丢失。 |
| Focus hold | 键盘焦点所有者的逻辑祖先链包含宿主。 | 焦点移出。 |
| Popup hold | 逻辑祖先链包含宿主的 AtomUI `Popup` 打开时从 Window 取得 hold lease。 | Popup 关闭时释放 lease。 |
| Native chrome hold | Native chrome extent 大于 persistent native inset。 | 两者相等，即平台 chrome 已完全收起。 |
| 显式 hold | 应用写入 `IsFullScreenTitleBarOpen=true`。 | 应用写入 `false`，或发生立即关闭。 |

逻辑祖先链跨越 popup root，因此从全屏标题栏打开的菜单、下拉、Flyout 及其嵌套弹层都归属于宿主。

Native chrome hold 只在平台自行呈现顶部 chrome 的后端上出现。它与 trigger band 由同一个 pointer 动作引出：pointer 到达顶边前的移动事件仍送达客户区，trigger band 的 enter delay 计时与平台的呈现并行进行，先成立者使状态机进入 `Open`，另一方随后只作为 hold 生效。

状态迁移：

- 存在任一 hold 时保持 `Open`。
- 最后一个 hold 结束时 `Open -> Closing`，开始 `FullScreenTitleBarMouseLeaveDelay` 计时。
- `Closing` 期间出现新的 hold 则回到 `Open`；计时结束则 `-> Closed`。

立即关闭（跳过 leave delay，并释放显式 hold）：

- pointer 在 keep zone 之外、且不属于 popup hold 归属范围的位置按下。
- 焦点位于宿主内时按下 `Escape`；关闭后焦点还给打开前的焦点所有者。
- 应用写入 `IsFullScreenTitleBarOpen=false`。

抑制：存在以下任一条件时不接受新的 reveal，已打开的表面按立即关闭处理。

- Window 持有至少一个 chrome suppression lease，即模态 Dialog 或 Window Drawer 处于活动状态。
- Window 不处于活动状态，且不存在 popup hold。

### 7.4 Slot 迁移

默认标题栏在两个 slot 之间迁移时遵循固定顺序：

1. 清空源 slot 的内容。
2. 等待该实例脱离源 presenter 的视觉与逻辑父级。
3. 写入目标 slot 的内容。

迁移由 effective mode 或 content source 的变化触发，在同一次属性变更处理中完成，不跨越异步边界。迁移不创建新的标题栏实例，不重建 add-on 内容。实例在两个 presenter 之间移动时会短暂离开逻辑树，按标题栏既有规则释放宿主投影 lease；Window 在同一次处理中立即重新接入同一实例，因此迁移前后标题栏始终连接到同一个 Window。

### 7.5 Dock inset

```text
DockInset = effective mode == Docked ? P + 宿主实测高度 : 0
```

Dock inset 只使用 persistent native inset，不使用实时的 native chrome extent：平台 chrome 临时滑入时表面被下推并短暂覆盖内容区顶部，内容区不随平台动效重排。

Window 把 Dock inset 发布为内容区顶部占位：CSD 模板中并入 `EffectiveContentFrameMargin` 的 Top 分量，非 CSD 模板的内容根直接消费同一投影。宿主高度变化、effective mode 变化和模板重套用都会触发重算。`AutoHide` 与 `Pinned` 下 Dock inset 恒为零，内容区尺寸不随表面打开或收起而变化。

### 7.6 动效

表面打开与收起使用沿 Y 轴的位移与不透明度动效，时长取自主题动效 Token，主题关闭动效时直接切换可见性。收起动效进行期间表面不参与命中测试；`Docked` 模式不使用进出动效。动效只影响宿主表面的呈现，不改变 open state 的取值时机。进出动效相对于表面的当前顶边 `max(E, P)` 计算，与平台 chrome 引起的位移相互独立、可以叠加。

### 7.7 生命周期

| 时机 | 行为 |
| --- | --- |
| 进入全屏 | 平台 chrome manager 开始发布三个几何输入，macOS 尝试建立 title-strip fusion；解析 effective mode；按 7.4 迁移默认标题栏；`AutoHide` 启动状态机并处于 `Closed`，`Pinned` / `Docked` 直接打开。 |
| 平台不支持的全屏请求 | 平台 chrome manager 报告当前窗口不能进入全屏时（macOS 有 owner 的窗口），Window 在属性变更中立即把 `WindowState` 复位到请求前的值，全屏标题栏不观察到被拒绝的值；已处于全屏请求状态的窗口随后获得 owner 时复位为 `Normal`。 |
| 平台发起或确认的状态转换 | 只以 `WindowState` 属性变化为准：平台确认、拒绝或主动发起的转换都表现为该属性的变化，全屏标题栏随之启停。macOS 的系统 chrome 跟踪在系统完成进入全屏的转场后才找到原生标题条，转场期间 native chrome extent 为零。 |
| 退出全屏 | 平台 chrome manager 停止发布、恢复 title-strip fusion 隐藏的系统视图并释放其平台观察者；停止状态机并取消计时器；释放显式 hold；默认标题栏迁回 Normal slot；Dock inset 归零；open state 强制为 `false`。 |
| 模式或内容来源变化 | 重新解析 effective mode，按需迁移 slot 并重置状态机。 |
| CSD 状态变化 | 模板随之切换，按模板重套用处理；全屏期间发生时 Normal slot 改由新模板确定，默认标题栏仍留在 FullScreen slot。 |
| 模板重套用 | 释放旧宿主的订阅；取得新宿主后回放 effective mode、slot 内容、open state 与 Dock inset。 |
| Popup 关闭 | 释放其 hold lease；lease 释放幂等。 |
| Window 关闭 | 停止状态机，释放计时器、全部 hold lease 与宿主引用。 |

## 8. 资源、性能与 AOT 边界

- 状态机使用一个可复用的计时器，只在 `Opening` 与 `Closing` 状态下运行；`Closed` 状态不持有活动计时器。
- pointer move 处理只做固定数量的数值比较，不分配对象，不遍历视觉树；焦点归属判断只在焦点变化时沿逻辑祖先链执行一次。
- 非全屏状态下状态机不订阅 pointer 输入，宿主不可见且不参与布局测量之外的工作。
- 平台 chrome 的观察只在全屏期间存在，使用平台的窗口移动通知驱动，不使用计时器轮询；Windows 与 Linux 不建立任何观察。macOS 通过原生窗口按钮所属窗口这一公开关系定位系统窗口。
- Title-strip fusion 修改的是 AppKit 内部视图的可见性。它依赖平台未公开保证的视图结构，因此以结构识别为前提、以堆叠呈现为退化，且修改范围限于隐藏与恢复，不替换、不移除、不重排系统视图。该能力集中在 `AtomUI.Native` 的 macOS 平台层，Desktop Controls 只消费三个几何输入。
- Hold lease 是固定大小的可释放对象，Window 以计数管理；状态变化不重建 lease。
- 默认内容不创建第二个 `WindowTitleBar`、第二组 caption button 或第二套 Logo 与标题绑定。
- 不引入新的 DynamicResource owner、反射、运行时类型发现或字符串 binding path；模式解析使用封闭枚举和显式分支，对 NativeAOT 无额外裁剪风险。

## 9. 兼容性与定制边界

- `FullScreenTitleBarMode` 的四个值及其“是否呈现、是否自动收起、是否占用布局”语义属于稳定契约，默认值为 `AutoHide`。默认值在所有平台相同，不随 `OsType` 变化；需要常驻标题栏的应用显式设置 `Pinned` 或 `Docked`。
- `IsFullScreenTitleBarOpen` 始终反映实际呈现状态；`Hidden`、`Pinned`、`Docked` 与非全屏下不接受应用写入。
- 默认内容来源下，全屏标题栏与普通标题栏是同一个 `WindowTitleBar` 实例；`LeftAddOn` 与 `RightAddOn` 在全屏下保持可用。
- `IsTitleBarVisible=false` 且未提供自定义内容时，全屏下不出现 managed 表面。
- `Hidden` 不改变任何窗口能力；退出全屏的入口由应用通过 `ToggleFullScreen()` 或 `WindowState` 提供。
- 全屏下的关闭与退出全屏只经过 Window 的 caption command，与普通状态共用关闭请求流程和全屏前状态恢复。
- `AutoHide` 与 `Pinned` 不改变内容区尺寸；只有 `Docked` 产生 Dock inset。
- 四种模式与自定义全屏内容在所有平台都由 AtomUI 宿主呈现；平台差异仅限 native chrome extent、native control inset 与 persistent native inset 三个几何输入。
- 平台自行呈现顶部 chrome 时，全屏标题栏随其移动并与原生标题条融合为一条，原生窗口按钮呈现在全屏标题栏的前导区域；AtomUI 不抑制、不替换平台 chrome。融合不成立时退化为位于原生标题条正下方的堆叠呈现，两种呈现的模式语义相同。
- 原生窗口按钮的点击语义始终由平台处理，不经过 AtomUI 的 caption command；AtomUI 的 caption button 配置不控制原生窗口按钮。
- `ToggleFullScreen()` 退出全屏时恢复 AtomUI 记录的进入前状态；由平台发起的退出恢复到平台决定的状态。
- 派生窗口通过 `NotifyCreateTitleBar` 提供的标题栏与默认标题栏走同一迁移路径，派生标题栏不需要为全屏提供单独模板。
- 上层控件不得按 `OsType`、CSD 状态或全屏模式自行推算全屏标题栏高度，只能消费 Window 发布的内容区几何。

## 10. 验证要求

| 层级 | 验证内容 |
| --- | --- |
| 纯逻辑 | effective mode 解析表（`WindowState` × requested mode × content source）；表面顶边、前导安全区与 Dock inset 对三个几何输入及其退化值的计算；native chrome hold 的建立与结束；open state 强制约束表；状态机的全部迁移，包括 enter delay 中途离开、leave delay 中途恢复、多个 hold 叠加与逐个释放、立即关闭、抑制，以及零延迟配置。使用注入时钟，不依赖真实计时。 |
| 控件行为（headless） | 进入与退出全屏时默认标题栏在两个 slot 之间迁移且实例不变；任意时刻只有一个 presenter 持有该实例；add-on 内容在迁移后保持同一对象；自定义内容优先于默认标题栏；`IsTitleBarVisible=false` 的解析结果；`Docked` 的 Dock inset 等于宿主高度且其他模式为零；`IsFullScreenTitleBarOpen` 的双向绑定与强制结果；`ToggleFullScreen()` 恢复进入前的 `WindowState`。 |
| 弹层协作 | 从全屏标题栏打开菜单、下拉和嵌套弹层期间表面保持打开，全部关闭后经 leave delay 收起；模态 Dialog 或 Window Drawer 活动期间不接受 reveal。 |
| 层级与遮挡 | Window 把宿主迁入 `WindowChromeLayer`（T6）内的表面宿主，模板重套用后新模板中只有一个承载宿主的 chrome 层，旧层与 Window 关闭时的层被清空；断言父层归属与号码，而不是只断言 child `ZIndex`；全屏标题栏打开时，标题栏区域的 `InputHitTest` 命中标题栏而不是吸顶 Tab、Badge、Watermark 或 FloatButton；标题栏位于 Window Drawer、Tour、Dialog、弹层和 Message 之下；标题栏按钮的键盘焦点描边落在 chrome 层自己的装饰宿主中；迁移前后全屏表面顶端的窗口坐标 Y 值一致，`TemplateBinding` 在迁入后仍然生效。 |
| 主题契约 | 三个 Window 模板都包含 `PART_FullScreenTitleBarHost` 且声明位置符合 6.1；`WindowDrawnDecorationsTheme` 不包含全屏 popover 内容；宿主模板包含 `PART_Surface` 与 `PART_ContentPresenter`。 |
| 生命周期 | 模板重套用、退出全屏和 Window 关闭后不残留计时器、pointer 订阅、hold lease 或旧宿主引用；标题栏迁移不产生重复的宿主投影 lease。 |
| 平台实机：Windows | 四种模式；顶边触发与收起；上下排列多显示器时的触发；触摸下拉；全屏下关闭与退出全屏；任务栏位于顶边且自动隐藏时的交互；全屏表面不可拖动窗口。 |
| 平台实机：Linux X11 | 四种模式；窗口管理器快捷键进入与退出全屏；不支持 EWMH 全屏的窗口管理器下保持不活动；全屏表面不可拖动窗口。 |
| 平台实机：Linux Wayland | CSD 与非 CSD 两种装饰模式下的四种模式；合成器发起的全屏进入与退出；全屏期间装饰模式切换后的状态回放；触摸按下不发起窗口移动。 |
| 平台实机：macOS | 融合呈现：系统 chrome 滑入时表面顶边贴合菜单栏下沿并同步移动，原生窗口按钮出现在表面前导区域并垂直居中，表面内无残留的背景材质与阴影线；标题栏前导内容为原生窗口按钮让位，系统 chrome 收起后复位；被透明标题条覆盖的 add-on 可点击，原生窗口按钮的关闭、最小化与退出全屏可用。`AutoHide`、`Pinned`、`Docked` 与自定义全屏内容分别验证；`Docked` 的内容区不随系统动效重排。退出全屏后原生标题条外观恢复。堆叠退化、没有原生窗口按钮时的阶跃退化、pointer 从上方显示器进入、系统 chrome 常驻配置、带刘海显示器，以及受支持的各 macOS 大版本分别验证。 |
| Gallery | Window ShowCase 的“全屏标题栏”示例：窗口不设置 owner（见 4.3 macOS），通过 `IsFullScreenCaptionButtonVisible` 与 `ToggleFullScreen()` 进入和退出全屏，`RightAddOn` 中的图钉在全屏期间切换 `AutoHide` 与 `Pinned`；示例使用的 API 名称和默认值与本文一致。 |
| AOT | Desktop Controls 测试与适用的 NativeAOT 发布路径不产生新的反射或裁剪告警。 |

平台证据按平台独立记录，未执行的平台不得标记为通过：

| 平台 | 状态 | 已验证范围 |
| --- | --- | --- |
| macOS | 已实测（单一系统版本与单一显示器） | AppKit 平台语义：客户区覆盖整屏，系统菜单栏与原生标题条以覆盖方式滑入、内容不重排，系统窗口逐帧发出移动通知，原生标题条背景无法经公开透明属性去除，透明区域的输入穿透到客户区，有 owner 的窗口不能进入原生全屏。AtomUI 实现（基于 Gallery 的脚本视觉验收，单窗口原生截图采样门禁 A/A=0）：Gallery 主窗口的派生标题栏在 AutoHide 下迁入全屏宿主，指针到顶后呈现于菜单栏下方并与原生标题条融合，红黄绿在 40pt 表面内垂直居中，指针离开后关闭，退出全屏后同一实例与原生按钮安全区恢复；Gallery 示例窗口经 `ToggleFullScreen()` 进出全屏，Pinned 贴顶常驻且在系统 chrome 出现时下移并呈现原生按钮，Dark 主题下表面跟随标题栏背景。未测试：Docked 与自定义内容的 Gallery 路径、系统 chrome 常驻配置、带刘海显示器、没有原生窗口按钮的窗口、其他 macOS 版本、键盘 Escape 与弹层保持的实机路径。 |
| Windows | 部分实测（Windows 10，单显示器，150% 缩放，Gallery 主窗口） | 层级与遮挡（全局层级阶梯 `WindowChromeLayer`）：Space 页滚动使吸顶 Tab 出现后，鼠标从内容区滑到顶边，AutoHide 表面在全宽呈现并盖住吸顶 Tab，连续截帧中没有吸顶 Tab 画在表面之上的帧；标题栏图钉切到 Pinned 后，鼠标离开时表面仍常驻于吸顶 Tab 之上；图钉切回与经 caption button 退出全屏正常。全屏下打开 Window Drawer，Drawer 表面延伸到窗口顶边，其内按钮的键盘焦点描边可见（该次未把鼠标移到顶边，Drawer 活跃期间的 reveal 抑制未实测）。未测试：Docked 与自定义内容、上下排列多显示器、触摸下拉、任务栏位于顶边且自动隐藏、全屏下关闭、全屏表面拖动检查、Dark 主题、标题栏内按钮的焦点描边。 |
| Linux X11 | 未测试 | 平台语义来自后端实现分析，尚无实机证据。 |
| Linux Wayland | 未测试 | 平台语义来自后端实现分析，尚无实机证据。 |
