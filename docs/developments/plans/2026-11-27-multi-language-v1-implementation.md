# 多设计语言 v1.0 实施与验收

以[主规范 v1.0](../../engineering/development/multi-language-control-contracts.md)、[动效替换契约](../../architecture/systems/motion/design-language-overrides.md)和[动效实施方案](../specs/2026-11-27-design-language-motion-overrides-design.md)为准。在 `feature/refactor-theme-system` 实施。功能验收完成后，再独占测量机做性能验收。已有主题首次装载的验证按[专项实施记录](2026-11-27-theme-first-load-refactor.md)复用，改动影响原证据时重开对应场景。

| 任务 | 依赖 | 完成条件 | 状态 |
| --- | --- | --- | --- |
| V1 主题值与过渡 | 现有 Token、编译主题和 Motion 播放 | 同一 Spring 文本得到相同参数和曲线。微状态与编排独立，已声明组件通道可覆盖，缓存按有效值共享。 | 核心定向用例和正式主题文件消费通过 |
| V2 瞬态反馈 | V1 的曲线和共享播放 | 中性 API、零常驻节点、真实指针位置、同节点接续、撤销和释放、空效果短路。默认波浪及外部按压反馈均经公开协议。 | 真实 Button 与无友元 NuGet 的反馈用例通过 |
| V3 编排与持续呈现 | V1，现有弹层宿主 | 标准槽位、位置感知、局部显式 null 及清除、对话框和反馈默认轨迹；指示条与加载呈现按公开扩展点替换。 | 缺省槽位与宿主功能验收完成 |
| V4 公共行为与模板 | 已有首次装载接入，V2–V3 的控件入口 | 完成 SplitButton、ToggleSwitch、TreeView/TabItem 试点和受影响家族迁移。验证原生优先级、业务操作次数、部件命中、默认几何和语言贡献。 | 试点迁移与外部策略的正式包验收完成 |
| V5 事务与编译合约 | V1–V4 的实际贡献入口 | 同窗两作用域、旧回调释放、组合输入保护、跨包 Semantic PART 红绿对照；同一套公开入口可由无友元包在正式 JIT 和 NativeAOT 消费，裁剪负例仍有效。 | macOS arm64 验收通过，见证据 |
| V6 产品验收 | V1–V5 功能通过 | 记录锁定 Ant Design 体验的适用场景和实际结果；在已约定的代表场景测量动效和完整 Gallery 启动。 | Shadcn NativeAOT 加载通过，按用户决定收尾性能复测。见[验收结果](#nativeaot-加载与-t4-复测) |

实施以规范指定的行为和家族为边界。现有控件仅换 Token 即可满足目标时复用其模板和行为。根据实际共享需求提炼公共入口。跨仓与系统原生输入法证据按可用宿主记录，未测的环境不计通过。

## 验证记录（2026-11-27）

- Core 在清理后运行曲线、过渡、播放、作用域与启动定向套件，33/33 通过。弹簧测试用同一个输入分别经过动态解析和编译 worker 共用的 codec，核对四参数、五个曲线采样点与无效参数。后续用独立编译主题文件再次验证端到端读取。Semantic PART 生成器定向套件 78/78，外层锚点与内层标记的双向负例分别能阻断语言包构建。
- 模块消费用例在真实 AtomUI Window 的 Headless 后端运行，3/3 通过。Button 按下和松开淡出使用同一装饰节点，退出最多接管一次，改为 `None` 后立即移除视觉。两次指针激活共两次 Click。另通过默认 SwitchTrack 布局、Select 显式 null 和 ClearValue、TabItem 用户主题优先和清除回退、Button 加载指示器按需建树和移除，以及 TreeView 语义策略拒绝后的事件计数与回退。
- Ant Design 参考采用 `6.7.0`（`c811baa979c8`）。已对照 Button 状态曲线、横向 Menu 的背景与边框时长、普通与 quick 波浪的曲线和生命周期参数。以上证据用于已核对的通道，全面视觉与交互验收尚在 V6。
- Release 入口生成 18 个 `6.3.0-multilanguage.1` 候选 NuGet 包。独立消费仅用 `PackageReference`，`net10.0 / osx-arm64` 的 JIT、NativeAOT 各通过外部反馈、编译 Spring、组字期间祖先冻结三项。NativeAOT 单独的 Ant-only 取值保持 `14`，ILC map 保留 Ant Button 规则，QRCode 注册、主题工厂及规则和 Shadcn 规则计数均为 0。
- macOS 原生窗口从 AppKit 文本输入客户端注入 preedit、确认和取消，验证焦点、模板和已提交文本。确认与取消均通过，独立 `PopupRoot` 在切换设计语言时沿用受保护的作用域主色和原宿主，确认后祖先模板追平。此证据覆盖 AppKit 文本输入链与所测的编辑边界。
- 临时控件用例和独立消费工程已清理。本轮原始日志、包哈希、ILC 输入摘要和闭包计数位于 `.artifacts/multi-language-v1/`，权威汇总为 `acceptance.json`。持久保留的生成器用例验证实际的跨包契约红绿结果，Core 用例验证运行时与编译 worker 的同一曲线输入。

### 策略登记和缺省编排复验

追加候选 `6.3.0-multilanguage.2`。外部策略提供者和应用分别构建，应用只通过公开 NuGet 引用。JIT 与 NativeAOT 都验证了启动及套模板不构造策略、Ant 作用域首次使用构造一次、重复读取复用、Shadcn 作用域不取 Ant 策略以及实际指针激活恰好询问一次。NativeAOT 的 1 个选中策略构造符号保留，未使用 QRCode 策略、控件注册片段和规则符号均为 0。

Dialog、Drawer、Message 和 Notification 从 `MotionResource` 取得标准缺省轨迹。Drawer 在显式关闭两方向动效时仍各产生一次打开和关闭，清除覆盖后恢复有效默认。临时外部包源码与消费工程已清理。证据索引为 `.artifacts/multi-language-v1/closure/acceptance.json`。

### 完整 Gallery 启动配对

代码和功能验收后，以最终功能候选 `6.3.0-multilanguage.2` 发布两种 Gallery 产物，对照[原启动基线](2026-11-27-theme-first-load-refactor.md#t2-基线留存记录2026-11-27)。六组各交错运行 10 对，120/120 正常退出。一分钟负载为 5.82–21.50，采样器仍标记所有样本为高负载。

| 完整 Shell 首批中位配对差 | Ant-only | 三语言启用，以 Ant 启动 | Shadcn-only |
| --- | ---: | ---: | ---: |
| JIT/R2R | −7.42 ms | +11.93 ms | +18.31 ms |
| NativeAOT | +7.68 ms | +1.41 ms | +11.01 ms |

五组超过无退步线。JIT 启用 Shadcn 的两组中，Configure 分段分别增加 29.44 和 29.09 ms，均为 10/10 同方向。Commit.Theme 分别缩短 23.25 和 22.43 ms。AOT 使用正式发布配置，本轮无 AOT Commit 读数。采样和哈希见 `.artifacts/multi-language-v1/closure/t4/`。

根因修正针对显式注册阶段提前执行的生成登记。配置阶段只提交生成注册入口，包准备时建立必要的标识和类型信息，选中的控件真正推导 Token 时才构造规则对象。与已有语言包自动发现及明确覆盖继续共用原先的注册与验证入口。受影响的基准工具同步使用实际交互租约，待代码与功能复验通过后重新执行性能采样。

`net10.0` Core 定向复验 36/36 通过，新增用例断言登记前不执行回调、包准备只建立规则标识、首次实际推导才创建实例且可重复使用。基准工具编译通过，并运行 Button、CheckBox、RadioButton、OptionButtonGroup 和 Switch 的五套状态验证，均通过。

### 登记修正后的启动复测

由当前源码发布 `6.3.0-multilanguage.3` 的 JIT/R2R 和 NativeAOT Gallery，复用同一基线、场景与正式发布配置。编译后确认两侧产物和场景，并停止其他本轮构建、功能验证。开测前一分钟负载为 4.75，桌面检查约 82% CPU 空闲。六组各 10 对，120/120 次成功。记录到的采样负载为 5.04–14.62，120 个样本依然符合采样器的高负载标记。

| 完整 Shell 首批中位配对差 | Ant-only | 三语言启用，以 Ant 启动 | Shadcn-only |
| --- | ---: | ---: | ---: |
| JIT/R2R | −17.98 ms | −12.91 ms | −3.38 ms |
| NativeAOT | −3.37 ms | +8.66 ms | −4.01 ms |

启用 Shadcn 的 JIT 配置阶段中位差分别降至 −1.16 和 −1.37 ms。三组 JIT 的 Commit 中位数为 60.17、62.98 和 59.51 ms。以相同二进制单独复核 NativeAOT 三语言，额外 10 对的首批中位差为 +5.11 ms。合计 20 对为 +6.50 ms，15 对较慢。首次 UI 泵的合计差为 +10.07 ms，最长 UI 间隔为 −8.98 ms。以同一正式发布配置采样，因此仍缺 AOT Commit 分段。NativeAOT 三语言的无退步验收线未通过，T4 保留。产物哈希及六组全样本见 `.artifacts/multi-language-v1/retest/t4/samples/`。单组复验见同目录 `aot-all-focus/`。

### 取值登记的摘要校验与排序

在 Avalonia 12.2.0 上对同一个 NativeAOT Gallery 交替使用仅 Ant 和三语言均已启用的配置，两侧都以 Ant 打开。诊断版五对的主题提交差中位数为 +19.05 ms。下钻后注册表准备增加约 14.2 ms，主要来自摘要校验的 +9.26 ms 和规则排序的 +4.37 ms。初始主题应用差异接近零。分段样本见 `.artifacts/multi-language-v1/startup-root/t4/` 下的 `diagnostic-samples`、`trace-samples` 和 `registry-samples`。

生成的取值登记直接传入构建时读取的摘要，启动校验仍与实际加载的 Token 类型提供的摘要比对。旧包及手写登记沿原程序集摘要校验路径处理。规则排序先计算键，消除排序比较中重复读取类型全名。Core 定向 57 项与生成器定向 82 项通过，包含摘要不匹配、语言作用域和 TypeMap 生成检查。

正式采样前将包加载事务归回 `ApplicationScope.cs`，并将 Dialog、Drawer、MessageCard、NotificationCard 的动效属性归回各自的公共属性区。四个控件合计 327 个成员的语法指纹逐项一致。合并后的 Core 定向复验 90 项通过。

`6.3.0-multilanguage.4` 独立 NuGet 消费确认 Avalonia 12.2.0。JIT 真实 Button 通过首次切换及清除作用域。NativeAOT 的默认探针发布遇到 ILC 10.0.14 的 Portable PDB 读取越界。仅移除临时消费缓存的第三方 PDB 后，双作用域通过且 ILC map 仍有 Ant 与 Shadcn 各两个 Button 规则符号。Ant-only 在移除临时调试符号的诊断发布中读得 Button 字号 14，Shadcn 规则和 QRCode 规则、注册、工厂均为零。详细日志和产物校验见 `.artifacts/multi-language-v1/startup-root/`。

Browser Debug 重现 `ManagedToNativeGenerator` 读取同身份的两份 Avalonia.Base 12.2.0。共享输出目录的文件与桌面探针生成的适配版哈希一致。启用适配的 Desktop 改用独立输出路径，复制和发布均采用包解析阶段选出的唯一适配资产，原版继续供共享依赖解析。实际 Build 与 Publish 输出哈希已核对。完成 Desktop 隔离构建后，同一 Browser Debug 构建命令通过。证据见同目录 `browser-assembly-receipt.json` 和 `browser-final.log`。

下表是完整 Shell 首个渲染批次的配对中位差，负值表示候选更快。优化前后的 Gallery 均使用 Avalonia 12.2.0，既有 T4 基线另列。JIT 三组和 NativeAOT Shadcn-only 各 20 对，NativeAOT 其余两组各 10 对。300 个正式采样进程均成功，采样负载 9.42–17.05，全部触发高负载标记。

| 场景 | JIT 相对优化前 | JIT 相对 T4 基线 | NativeAOT 相对优化前 | NativeAOT 相对 T4 基线 |
| --- | ---: | ---: | ---: | ---: |
| Ant-only | −1.02 ms | +8.42 ms | −19.34 ms | −5.61 ms |
| 三语言启用，以 Ant 启动 | −2.27 ms | +4.79 ms | −21.85 ms | −9.89 ms |
| Shadcn-only | +2.42 ms | +12.10 ms | −1.46 ms | +2.76 ms |

JIT 的候选内核 Commit 中位数为 62.52、64.87、61.05 ms，较同版本优化前的配对差为 +1.04、−1.07、−0.18 ms。NativeAOT 三语言的首次 UI 泵也提前约 18.47 ms。NativeAOT Shadcn-only 对发布基线的两批中位差方向相反，合计仍为正。JIT 三组相对既有 T4 基线也仍为正。T4 尚未通过完整无退步门槛。正式 NativeAOT 二进制维持默认 EventSource 配置，候选内核 Commit 耗时未实测。完整配对差、进程输出与产物哈希见 `.artifacts/multi-language-v1/startup-root/t4/paired-combined.json` 和同目录各批次采样。

### 原基线复验与 Shadcn 加载指示器

复用上轮的现有候选和原 T4 基线，20 个输入文件哈希一致。每组 10 对，80 个进程全部通过。采样负载 9.36–12.31，仍全部触发高负载标记。

| 场景 | 完整 Shell 首批配对中位差 | 候选更慢 |
| --- | ---: | ---: |
| JIT Ant-only | +2.46 ms | 6/10 |
| JIT 三语言，以 Ant 启动 | +7.70 ms | 5/10 |
| JIT Shadcn-only | +8.25 ms | 6/10 |
| NativeAOT Shadcn-only | +0.73 ms | 5/10 |

JIT Shadcn-only 的首次绘制到 Shell 合成批次在三批中分别多 7.91、3.88、4.50 ms，共 23/30 对变慢。两份候选的 JIT 诊断调用栈在这段时间均捕获到合成线程初始化标量关键帧表达式，两份原基线诊断栈未捕获该调用。正式采样与诊断调用栈分开记录。

旧候选包使用真实 Shadcn Button 复现了空闲状态下加载图标可见、已加载且正在旋转。共享 Button 样式通过 `ContentPresenter#PART_LoadingIcon` 管理加载状态，Shadcn 模板沿用直接构造图标的旧形态，导致选择器漏匹配。修复让 Shadcn 对接已有的共享延迟加载模板。重新打包后的同一用例验证空闲无子元素、进入加载可创建默认或自定义指示器、退出加载释放子元素。

修复后以同一 Gallery 探针重新发布 JIT Shadcn-only，修复前、修复后及原基线交错各 10 次，30 个进程全部通过。相对修复前，完整 Shell 首批配对中位差 −10.13 ms，首次绘制至 Shell 合成批次 −4.07 ms。相对原基线两项分别为 +0.10、−0.32 ms。单次修复后诊断在 Shell 合成批次前未采到标量关键帧表达式初始化。正式采样负载 7.15–8.80，仍触发高负载标记，原 T4 全组门槛仍未关闭。修复后的 NativeAOT 未重新发布。本轮样本、启动分段和包消费记录见 `.artifacts/multi-language-v1/startup-root/t4/lower-load-retest/`。

Splash 使用三个内置设计语言各自的 `BoxShadowsSecondary`。通过 macOS 原生透明启动窗口检查 Ant Design、Shadcn、PrimeNG 各自的亮暗场景，六个样本的卡片外侧均有阴影且窗口边角保持透明。卡片为 420 × 110 DIP，外侧非透明像素数分别为 25,546、2,049、3,552。实际窗体捕获使用 `RenderTargetBitmap`，不作为原生合成器帧计时证据。

### NativeAOT 加载与 T4 复测

当前代码使用 Avalonia 12.2.0 发布 macOS arm64 NativeAOT Gallery，保留正式 EventSource 配置。在真实 Shadcn Button 和同一渲染窗口中，空闲时加载部件隐藏且没有子元素，进入加载后默认指示器已挂接并可见，退出后指示器与视觉树断开。三个状态均通过。同一产物用于计时，状态检查只在单独的验收进程中启用，并在首次合成批次记录后执行。

复用修复后的 JIT 产物和原 T4 基线，15 个文件的哈希与既有记录一致。正式交错采样四组各 10 对，80 个进程全部成功。启动前一分钟负载为 4.71，正式进程的启动前读数为 5.06–11.02，全部触发高负载标记。

| 场景 | 完整 Shell 首批配对中位差 | 候选更慢 |
| --- | ---: | ---: |
| JIT Ant-only | −14.21 ms | 4/10 |
| JIT 三语言，以 Ant 启动 | −0.95 ms | 5/10 |
| JIT Shadcn-only | +0.28 ms | 5/10 |
| NativeAOT Shadcn-only | −1.55 ms | 4/10 |

JIT Shadcn-only 的首次绘制到 Shell 合成批次快 3.49 ms，内核 Commit 快 1.77 ms。成对首批结果各有五次快和慢，未观察到修复前可重复的渲染阶段延迟。JIT Ant-only、JIT 三语言和 NativeAOT Shadcn-only 满足首批无退步数值，JIT Shadcn-only 仍超过原有的 0 ms 线。追加的逐进程低负载采样完成 25 个进程，四组各有 3 个完整配对，负载 4.80–4.92。按用户决定停止追加采样，保留部分数据，性能工作收尾。原 T4 全组硬门槛未判通过。证据见 `.artifacts/multi-language-v1/startup-root/t4/final-t4/`。

### 动效参数覆盖审计

#### 现有配置会在哪里失效

源码审计从 XML 的配置位置跟到最终消费。主题编译器在控件取值前冻结 `<Control>` 中显式配置的共享和控件 Token。Skeleton、SplitView、Spin 等取值规则中的字面量或派生公式属于默认值。根 `<Tokens>` 改变共享值，控件规则声明的专用默认仍可细化它。需要针对具体控件时，应使用 `<Controls>/<Control>/<Tokens>`。这一层的覆写契约已有实现。 [控件求值](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs) · [XML 作用域](../../architecture/systems/theming/design-languages.md#10-作用域与控件级配置)

具体反例是 PrimeNG ToolTip 的 `<Control Catalog="AtomUI" Id="ToolTip"><Tokens><Token Name="MotionDuration" Value="00:00:00.220" /></Tokens></Control>`。显式 220 ms 会传到 ToolTip，再绑定到 Popup。Popup 播放选择的 Fade 定义带有显式 250 ms，所以 220 ms 没有进入计划。Shadcn Dialog 则选择显式 150/100 ms，覆盖了 presenter 已读取的 `MotionDurationSlow/Mid`。 [ToolTip 主题](../../../src/AtomUI.Desktop.Controls/Tooltip/Themes/ToolTipTheme.axaml) · [ToolTip 转交](../../../src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs) · [Popup 播放](../../../src/AtomUI.Desktop.Controls/Popup/Popup.cs) · [Dialog 播放](../../../src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs)

临时失败用例从实际 Shadcn 主题选择 `SelectOpen`，在定义上引用已存在的时长 Token，并给播放目标提供 220 ms。垂直翻转后检查最终 `MotionPlan.Duration`，实测仍然是 150 ms。该用例单独证明了定位边界的丢值，输入通过作用域资源提供。运行时与构建期 XML 的端到端验收列在后文。临时用例已清理，原结果见 `.artifacts/multi-language-v1/xml-motion-fix/shadcn-before.log`。

#### 已确认的影响面

Shadcn 与 PrimeNG 各有 **12 个**由方案直接给出有限时长的槽位。下表列出实际使用这些槽位的主要入口。Shadcn 默认为进入 150 ms、退出 100 ms。PrimeNG 默认为 300/300 ms，Tooltip 为 250/250 ms。 [Shadcn 方案](../../../src/DesignLanguages/Shadcn/AtomUI.DesignLanguages.Shadcn/ShadcnMotionScheme.cs) · [PrimeNG 方案](../../../src/DesignLanguages/PrimeNG/AtomUI.DesignLanguages.PrimeNG/PrimeNGMotionScheme.cs)

| 槽位 | 可达的组件入口 |
| --- | --- |
| `SelectOpen/Close` | Select、ComboBox、TreeSelect、Cascader、AutoComplete 及其派生模板、InfoPickerInput 的日期和时间选择器 |
| `DropdownOpen/Close` | DropdownButton、SplitButton、ContextMenu、MenuFlyout |
| `SubMenuOpen/Close` | MenuItem 的子菜单 |
| `PopoverOpen/Close` | Flyout、FlyoutHost、PopupConfirm |
| `TooltipOpen/Close` | ToolTip |
| `DialogOpen/Close` | Overlay Dialog 的表面播放入口 |

| 同类路径 | 已核实的问题与处理范围 |
| --- | --- |
| 宿主回退 | Popup 和共用组件宿主只检查是否声明了 `DurationToken`。声明的 Token 取不到时，宿主回退时长仍会被绕开。Popup 的 `MotionDuration` 同时接收框架主题默认及 FlyoutHost、Flyout、ToolTip 的转交，因此仅凭 `IsSet` 或有效值相同无法推断使用者的覆盖意图。 [Popup](../../../src/AtomUI.Desktop.Controls/Popup/Popup.cs) · [组件播放](../../../src/AtomUI.Desktop.Controls/Popup/ComponentMotionSelection.cs) |
| 几何准备 | Shadcn 的方向感知浮层覆盖上述五类浮层槽位，共 10 个方向。定位时创建新 Motion，丢弃了原定义的 `Duration`、`DurationToken`、`Easing`、`EasingToken`、`Delay`、`Origin` 和 `FillMode`。框架默认 Popup、Dialog、Drawer、反馈卡片已显式传递这些选项，可作为保真对照。 [Shadcn 定位](../../../src/DesignLanguages/Shadcn/AtomUI.DesignLanguages.Shadcn/ShadcnMotionScheme.cs) · [默认 Popup 定位](../../../src/AtomUI.Desktop.Controls/Popup/PopupMotion.cs) |
| 样式过渡 | 非生成 AXAML 中检出 20 处有时长 Token、写固定曲线且没有曲线 Token 的过渡，涵盖选中标记、反馈布局、进度与数值、菜单颜色、Steps、Tab 和 Tour。Button 的按下位移通道则同时写死 150 ms 和曲线，同模板的颜色通道已经读取两个 Token。先核对哪些通道承诺独立可配，保留每个通道原来的设计配方。 [Button 样式](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonAppearanceResources.axaml) · [过渡消费](../../../src/AtomUI.Core/Theme/Resources/TokenTransitionsFactory.cs) |
| 瞬态反馈 | 普通 Ant 波浪的扩张读时长 Token，淡出固定 2 秒、曲线固定。Quick 波浪已读取时长和曲线 Token。内置按压缩放和墨水反馈各自带阶段默认，可通过完整效果替换。普通波浪的扩张、淡出、曲线需要独立验收，另外两种仅在公开该阶段配置时接入相应数据。 [Ant 波浪](../../../src/AtomUI.Controls.Shared/WaveSpirit/AntDesignWaveEffect.cs) · [按压缩放](../../../src/AtomUI.Core/Animations/InteractionEffects/PressScaleInteractionEffect.cs) · [墨水反馈](../../../src/AtomUI.Core/Animations/InteractionEffects/InkRippleInteractionEffect.cs) |

进一步核对显示，Motion 定义的选择缓存以语言、变体和 registry 为键。在这个缓存中存入已解析时长或曲线会使同语言的 XML 更新仍命中旧定义。配置值应在真正准备播放时读当前作用域。 [MotionResource 缓存](../../../src/AtomUI.Desktop.Controls/Popup/MotionResourceExtension.cs) · [作用域查值](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPreparation.cs)

#### 修复实施

1. **先收敛时序优先级。** 把播放请求的显式时长与宿主回退时长分开传给现有 `AbstractMotion.Prepare`。统一采用「明确的播放覆写 → 目标 `Motion.DurationOverride` → 用户所选 Motion 的显式 `Duration` → 成功读到的作用域时长 Token → 宿主共同默认 → 框架全局回退」。保留公开 Motion 定义的显式含义和禁用终态。Popup 和共用组件宿主采用同一路径，`MotionDuration` 的共同默认及强覆写入口写入公开说明，Token 的存在性以实际解析结果为准。
2. **把语言的时间配方放回值层。** 保留语言方案的轨迹、起点和方向。PrimeNG Dialog 已有控件视图的 Slow/Mid 两个 300 ms，可直接消费。PrimeNG ToolTip 的 250 ms 统一由现有 Own `MotionDuration` 默认提供，两方向沿已存在的绑定消费，原 XML 写法即可修改。PrimeNG 其余 300 ms 槽位逐个核对真实 owner 的共用默认，补到取值规则。Shadcn 的 150/100 ms 有独立覆盖的真实需求，建议复用 `DurationToken` 并只增加一对共享的进出场时长 Token，通过现有 XML `<Tokens>` 和 `<Control>` 覆盖。先验证 owner 到播放目标的传递，再批量接入。已公开的单时长 XML（例如 ToolTip）可作为两方向共用显式值，同层方向配置优先于单时长。兼容判定读取归一化配置中的显式项，由控件包负责自己的兼容入口，沿用原有共享值和控件视图优先级。
3. **让定位保留定义选项。** Shadcn 的 `AnchoredMotion` 是无运行状态的轨迹定义，定位时浅复制现有定义并只写偏移，或用现有完整选项拷贝机制达到等价结果。保留显式原点、已保留的原点和计算默认原点的顺序。沿用公开 `IPlacementAwareMotion` 和第三方定义能力，内置实现合用同一套选项保真约定。
4. **把已承诺的通道接完整。** 为承诺按主题配置的曲线使用现有 `TokenTransitions.Easing` 和可表达相同默认曲线的 Token，Button 的独立位移另验证 150 ms 默认。Wave 扩张保留现有时长输入，淡出与曲线按独立阶段提供可配置值，默认仍为 2 秒和原曲线。`Delay` 当前由显式 Motion 定义提供，在几何和播放之间完整保留；将来有明确 XML 延时通道时沿相同取值方式接入。配置沿用现有作用域、资源、XML 和 Source Generator，执行改动落在已使用的播放准备路径。

#### 验收

- PrimeNG ToolTip 用同一组默认与 XML 样本验证 250 ms 与 220 ms，检查 Popup 最终 MotionPlan，覆盖原生、overlay 浮层。PrimeNG Dialog 和 Select 分别验证原 300 ms 默认及显式控件值。
- Shadcn Dialog 和一个会翻转的 Select 浮层断言默认 150/100 ms。同窗两作用域分别设两组时长和曲线，翻转、退出与重新打开时观察实际计划。应用自定义定义的延时、填充和原点应保留。覆盖声明的 Token 缺席、显式 0、全局禁用、显式 null 和清除覆盖。
- 代表性的 Button 位移、Steps/Tab 曲线、普通 Ant 波浪分别检查实际参数或播放轨迹。默认配方逐项对照现有行为，用户值只影响声明的方向或通道。
- 同一份 XML 经运行时加载和构建期编译，各自验证实播值，并覆盖同语言只更新 Token。保留 NativeAOT 的现有可达性负例，确认新增路由只在使用的控件和动效准备时执行。这些用例通过后再批量迁移剩余入场、离场和曲线通道。

#### 本轮实现与复验

Popup 和共用组件宿主在同一个 Motion 准备入口传入回退时长。Shadcn 与 PrimeNG 的已覆盖槽位通过进出场时长 Token 或已有宿主时长取值，Shadcn 的定位副本只更新几何。曲线沿用现有 Easing Token 路径，专用过渡的可选曲线为空时保留原配方。普通 Ant 波浪独立读取淡出时长和曲线。开发者的 XML 配置入口见[动效参数契约](../../architecture/systems/motion/design-language-overrides.md#状态通道与弹簧值契约)。

独立验证程序使用 Avalonia 12.2.0。在 JIT 与 macOS arm64 NativeAOT 中分别通过 9 个场景，逐个校验两种产物内携带的 XML 与构建输入字节一致。每种语言的样本既由用户主题目录运行时加载，也由 Source Generator 编译并注册。验证读取实际准备的 MotionPlan，包含以下结果。

- Shadcn Dialog 默认 150/100 ms，XML 覆盖为 220/90 ms。Select 在同窗两作用域分别使用 235/310 ms。已解析的定义在第二作用域更新后读到 340 ms，禁用后时长归零。传入翻转几何后位移方向正确，延时和填充保持。Button 按下位移保留默认 150 ms 和原曲线，XML 覆盖后为 126 ms 和指定弹簧。
- PrimeNG ToolTip 默认 250 ms，控件级 XML 让开闭计划都使用 220 ms。Dialog 开场默认 300 ms，控件级 XML 覆盖到 310 ms。Select 默认 300 ms。
- 普通 Ant 波浪在实际装饰节点上验证淡出轨道。默认 2 秒和原曲线，XML 覆盖为 520 ms 和指定弹簧。

清理了独立程序及其产物。清理后的 Core、Shadcn、PrimeNG 定向测试分别通过 43、85、22 项。输入与产物哈希、播放断言和构建日志保存在 `.artifacts/multi-language-v1/xml-motion-fix/`。
