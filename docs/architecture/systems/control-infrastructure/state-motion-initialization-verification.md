# 状态动效初始化验证

本文记录 [状态动效初始化架构](state-motion-initialization.md) 的源码接入和验证边界。
本轮环境为 2026-10-20、.NET 10、Avalonia 12.1.4、macOS arm64；未执行 Windows / Linux 原生验收。
目录入口见 [Control 基础设施](overview.md)，模块入口见 [AtomUI.Core](../../../modules/core/overview.md)。

## 1. 当前接入

生产实现使用初始化会话和公开属性系统的可撤销覆盖。没有动态 `StateMotion.IsReady` 属性或 Ready selector；
`IsReady` 只是会话内部的普通状态。主题继续声明过渡、时长、缓动及 `IsMotionEnabled` 条件。

| 范围 | 实现及完成入口 |
| --- | --- |
| Core | `TransitionSuppressionScope` 暂时覆盖有效 `Transitions`；`StateMotionSession` 管理基线、加载、复位和明确目标；`StateMotion` 提供固定模板声明 |
| ToggleSwitch / SwitchKnob | 根会话在完整把手矩形和内容偏移提交后完成；把手通过 `StateMotion.Initialization="Owner"` 在模板构建时接入，等待 owner 与自身加载 |
| Segmented / item | 根会话在容器排列、坐标及尺寸有效后完成；item 自身会话覆盖挂载后的初始选中外观 |
| TabControl / TabStrip / item | 指示条完整 transform 提交后完成；item 自身会话覆盖初始选中颜色；空状态后的首次指示器仍直接定位 |
| RadioIndicator / AbstractOptionButton | 覆盖挂载后的圆点尺寸、父组在容器准备后写入的 checked 外观；Radio 初始波纹不创建任务，旧请求在生命周期变化时取消 |
| ListBoxItem / ListViewItem / ComboBoxItem | 构造时抑制，容器准备及自身加载后恢复；父控件在清理容器之前复位会话，默认虚拟化随后分离并重挂载 |
| CarouselPageIndicator / PaginationNavItem | 覆盖生成容器的初始选中宽度、透明度及颜色；轮播进度的独立 Animation 保留 |
| InputControlFrame / RangeInfoPickerInput / BreadcrumbItem | 在共享外观或实际动画目标层持有会话，初始同步及加载后恢复；模板变化与分离时复位 |
| 运行期直接提交 | Tab 拖动及 DataGrid 重排使用独立 `TransitionSuppressionScope`，以 `using` / `finally` 恢复；普通布局不重启初始化 |

合计 17 类根或容器显式持有会话，SwitchKnob 使用模板接入。模板行为使用共享静态回调，常见的单目标登记不创建
List 或快照数组；没有全局控件表、轮询或排队开启任务。旧模板分离时解除 owner 登记，对象自身保留供重挂载使用的接线。

原 64 个调用文件按职责复核。状态初始化与运行期直接提交已退出私有反射开关；无挂载后初始写入的目标删除冗余开关，
保留其余生命周期工作。ProgressBar / Spin 保留过程反馈和既有实现，公开反射扩展保留 API 兼容；不能称为全库零反射。
`TextBox` 当前模板没有旧 `InnerBoxDecorator`，已删除无效查找和投递；输入外观由真实 `InputControlFrame` 管理。

## 2. 根因和有效红绿证据

框架在视觉挂载时重新开启过渡。只在 `OnInitialized` 关闭、Loaded 后投递开启，不能覆盖提前逻辑初始化、重挂载和
模板目标首次写入。原 Ready selector 只控制内置主题，对应用提供的自定义过渡也没有完整的初始化覆盖。

| 验证层 | 有效失败证据 | 修正后证据 |
| --- | --- | --- |
| 公开属性覆盖 | 5 项有效值、绑定恢复及作用域顺序断言失败 | 同一断言通过，支持最新绑定、交错释放、重复释放及首次后续过渡 |
| 初始化会话 | 4 项加载前完成、初始/重挂载、重复复位及旧完成通知失败 | 原断言及目标组扩展通过 |
| 四控件自定义过渡 | Switch、Segmented、TabControl、TabStrip 初始/复用几何 4 项失败 | 自定义集合保留，初始有效呈现值等于最终目标值 |
| 模板声明 | 目标挂载前有效集合尚未抑制，1 项失败 | 构建时抑制，owner/Loaded 汇合及重挂载恢复通过 |
| 真实父子场景 | Radio/disabled Radio、ListBox、OptionButtonGroup、Carousel、ListView、Pagination、ComboBox 初始状态仍启动过渡 | 迁移实际目标后，相关模块及临时场景 296 项通过 |
| 同步重入 | Reset 的属性通知重入 Complete / Dispose，2 项永久抑制失败 | 获取期间使用 Resetting 阶段，获得句柄后再交付所有权；重入释放撤销局部句柄，原断言通过 |

单独创建内部 item 并预先设为 selected 的探针，不能证明真实父控件的准备流程正确。容器通常在挂载后才被赋予初始
选择；本轮先发现 Tab item，再通过实际 Segmented、Radio、列表、分页和轮播场景纠正了该盲区。宿主不支持弹层、
测试数据类型错误、编译错误不计入有效失败；修正宿主及断言后才记录红绿证据。

现有四控件用例继续核对提前初始化的完整首帧几何，原有选择、Form、键盘、pointer、布局、动画时长和拖动用例继续执行。
临时诊断对框架私有运行状态的读取只用于此次验证，不进入库运行时或新增常驻测试。比较最终 base target，
不以动画结束后的截图掩盖初始错误；正常重复排列也不重启已经运行的过渡。

## 3. 性能证据

对照基线是本轮重构前的 Ready selector 实现，已经修正四控件的内置初始过渡，不能把本表解释为相对最初故障版本的提速。
前后使用同一 Release 测量驱动、页面和依赖配置，各自冻结库二进制；驱动修复了过时的启动注册和三个页面路由。
计时期间不并行构建或测试，保留模板、内容、正常动效和相同的视觉节点数。

每页先各执行 10 个独立冷进程，以及预热 3 次后的 20 次重复导航；分组冷数据存在时序波动，再对前后交替顺序执行
20 对独立冷进程。下表冷数据采用交错复核，重复数据采用同策略的 20 次导航，均为 mean / median / P95。

| Gallery 页面 | 冷加载前 → 后（ms） | 重复导航前 → 后（ms） | 冷加载 UI 分配前 → 后（KB，mean） |
| --- | --- | --- | --- |
| ToggleSwitch | 244.36 / 241.87 / 264.88 → 244.21 / 244.09 / 247.16 | 52.39 / 39.38 / 124.14 → 48.73 / 44.44 / 104.89 | 8645.33 → 8646.39 |
| Segmented | 339.29 / 336.04 / 352.35 → 338.09 / 336.26 / 345.09 | 79.12 / 71.37 / 122.30 → 73.14 / 67.07 / 97.15 | 14716.67 → 14699.29 |
| TabControl | 511.83 / 507.21 / 538.24 → 508.61 / 504.40 / 536.52 | 111.27 / 101.20 / 150.01 → 106.38 / 97.16 / 145.15 | 25467.01 → 25421.94 |

冷加载配对均差分别为 −0.15、−1.20、−3.22 ms，配对均差的标准误分别为 1.82、2.73、5.51 ms；差异在本轮波动范围内。
因此冷加载基本持平，不承诺页面提速，也不把 P95 的单轮变化外推为稳定收益。

控件级分配使用每方案 3 个独立进程，每进程预热 5 批、测量 12 批，每批 120 个控件。统计 UI 线程分配中位数，
运行实例诊断在计量之后执行；不等于保留堆内存。所有记录的初始状态过渡运行实例均为零。

| 代表控件 | 初始化前 → 后（B/控件） | 节省及计算 | 分离/关闭前 → 后（B/控件） |
| --- | --- | --- | --- |
| Switch | 135842.87 → 135674.87 | 168 B，(前−后)/前 ≈ 0.12% | 6370.27 → 6290.27 |
| Segmented，含三个 item | 321065.60 → 320662.93 | 402.67 B，≈ 0.13% | 17510.27 → 18350.27 |
| Tabs，含三个 item | 535311.97 → 533146.30 | 2165.67 B，≈ 0.40% | 26290.93 → 26842.93 |
| Button | 235841.13 → 235394.93 | 446.20 B，≈ 0.19% | 11964.13 → 11964.13 |

分离时为下一次挂载重新获取抑制，部分场景增加分配；单独 SegmentedItem 初始化增加 128 B，TabStripItem 增加约 32 B。
机制有成本，不能宣称整个生命周期都更便宜。复杂度成本为三个共享内部类型、17 个持有字段及模板接入回调，删除状态
初始化的反射/投递接线和动态 Ready selector；不增加逐帧会话、就绪订阅、视觉节点或第二份选择/几何状态。

## 4. 原生与 NativeAOT

本轮原生临时宿主使用 840×720 DIP 客户区、2× 渲染，预设 Switch On、另外三类控件 Third。已打开并检查实际截图：
把手、选中块和两种指示条位于预设位置，标签和布局保持正常，独立 loading 指示可见。
这是当前源码的原生画面证据；初始运行实例与时刻零几何由前述 Headless 断言共同验证。

解锁后完成当前源码的真实鼠标验收。临时原生宿主为 Release 构建，零警告、零错误；实际运行进程 PID 50430、
窗口 ID 9038，客户区仍为 840×720 DIP、2× 渲染。先通过 accessibility 激活确切进程，再由原生工具校验前台、
窗口及命中 PID；先前明确返回 `no input was sent` 的尝试不计通过。窗口移动后重新读取外框与客户区原点，
输入使用外框相对屏幕点，未把截图像素当作 DIP。

| 验收点与来源 | 条件、操作与独立预期 | 实际证据 | 脚本 / 视觉 |
| --- | --- | --- | --- |
| AC-01 初始状态，原始反馈 | 预设 On / Third；首次加载即在目标位置，无初始飞行 | 默认过渡下首个加载样本四项均等于目标，四项尚未记录中间值；原生截图中把手、选中块、指示条及 Third content 正确。时刻零判据同时由第 2 节红绿断言支持 | PASS / PASS |
| AC-02 交互保留，既有契约 | 真实点击 Switch 和三类选择控件；选择切换并正常过渡到新目标 | 控件内 pointer 与选择事件均到达，四项均采到非目标中间值并最终到达目标；Switch On→Off，Segmented / Tabs Third→First。查看四类中间及终态截图，文字、内容和指示位置正确 | PASS / PASS |
| AC-03 拖动，运行期作用域风险 | 两类 Tab 均选中 First，将其从首位拖至末位；顺序改变且选中身份保留 | TabControl 外框相对 (45.75,263)→(217,263)，TabStrip (45.75,431)→(217,431)；均收到一次 0→2 重排事件，顺序成为 Second / Third / First，仍选中 First、索引 2，指示条随拖动并最终对齐。已查看拖动和释放画面 | PASS / PASS |
| AC-04 抑制释放，根因风险 | 两类 Tab 拖动完成后，再真实点击 Second；普通动画必须恢复 | 两项均采到非目标中间值，最终选中 Second、索引 0；内容及指示条正确。独立 loading 弧线在各阶段截图中持续变化 | PASS / PASS |

AC-03 的实时预览样本中，被拖动 First 的客户区中心 X 由 45.75 变为 62.875 DIP，释放后指示条与选中项目标一致。
没有取消拖动、改变控件尺寸或关闭正常动画来取得通过结果。原症状的修复前证据沿用第 2 节，未覆盖本轮源码
重新制造原故障；本轮鼠标结果仅声明上述 macOS 场景通过，不外推至 Windows / Linux。

默认时长专项从运行中的有效 `Transitions` 读取配置，并以真实点击、16 ms UI 观察与有界条件等待核对实际过程：

| 控件 | 默认 duration | 观测到的非目标至目标区间 |
| --- | --- | --- |
| Switch | 200 ms，MotionDurationMid | 202.69 ms |
| Segmented | 300 ms，MotionDurationSlow | 284.42 ms |
| TabControl | 300 ms，MotionDurationSlow | 304.45 ms |
| TabStrip | 300 ms，MotionDurationSlow | 304.93 ms |

该区间有帧采样和进程读取误差，只用于排除明显拉长，不作为精密动画时基基准。生产 diff 未修改 duration 或 easing；
手感验收使用主题默认值。先前 750 ms 是临时宿主的自定义过渡诊断参数，用于观察中间帧，已移除该覆盖，不能当作默认体验。

项目外宿主以 `Release / osx-arm64 / PublishAot=true` 发布成功，确认为 arm64 Mach-O 并直接运行。为四个根目标配置
750 ms 的自定义过渡：首次 opened / loaded 四项有效几何均等于目标；第一次 API 切换后四项均处于过渡，稳定后
均回到目标。重挂载直接落位，使用新模板代理实际重建模板后再次直接落位，模板应用次数由 4 增至 8。已查看
NativeAOT 终态截图：Off / First 状态、选中块、内容及 loading 指示正常。该 API 驱动证明状态及生命周期；真实鼠标证据见上述独立验收。

本机链接使用已有 Homebrew OpenSSL / Brotli，链接器提示这些动态库的最低 macOS 版本高于目标 macOS 12；
本轮证明当前 macOS 宿主可运行，不作为 macOS 12 发布兼容证明，没有修改仓库依赖或发布配置。

## 5. Switch 文本视觉中心

前轮字形专项定位了同一用户反馈：22 DIP 轨道、12 DIP 系统 Helvetica 的 “On” 行盒居中，但可见轮廓中心约高 1.05 DIP。
默认字符串在测量阶段复用 TextLayout，按轮廓、基线及 shaping offset 计算可见中心，只缓存两个偏移数值；不加固定
Margin / TranslateY，不改变字号、轨道和显式/隐式自定义模板布局。该源头修正继续保留。

原专项 15 组默认字体、Helvetica Neue、Times New Roman、中文、下伸、组合字符、阿拉伯文附加符号及 Middle/Small
达到 2× 对齐容差；隐式文本模板维持应用拥有布局。该专项记录不冒充本轮重新执行的全部字体原生矩阵。

## 6. 长期保障、清理与最终检查

四个新 Core 运行用例保护：动态绑定集合恢复与交错/重复释放、提前初始化和重挂载后的首次真实过渡、两种同步重入。
已有控件用例保护实际首帧位置。现有控件测试、编译器和 NativeAOT 不能替代绑定恢复、非栈顺序释放和同步重入的运行契约；
这些共享机制故障会影响所有接入控件，必须随 Core 进入全局运行。四例复用最低成本 Headless 宿主，无发布子进程或
框架私有字段依赖；Core 全部 18 项临时及常驻验证执行约 0.4 秒；清理后四个新例与一个既有 MotionScene 用例共 5 项，执行约 368 ms。

逐控件迁移、A/B、私有运行状态诊断和性能探针源码已删除；临时工程、进程及取证产物按专项生命周期清理。
没有晋升逐控件矩阵或私有运行状态探针。探索性 DataGrid 初始选择探针未满足所用数据源的快照/定位前提，没有执行到
有效的动画断言，不计为通过，也不据此增加会话；DataGrid 本轮的通过结论限定为既有重排与状态清理验证。

清理后的最终验证：Core 5 项（368 ms）、35 类改动控件的 Desktop 定向测试 2043 项（约 61 秒）、DataGrid 重排
19 项（约 1 秒），合计 2067 项通过、零失败、零跳过。测量工具 Release 构建零警告、零错误；不运行 affected-wide 或全量。

```bash
state_motion_filter="$(python3 -c 'names="Breadcrumb ButtonSpinner Buttons Card Carousel Cascader CheckBox Collapse ColorPicker ComboBox Dialog Drawer FloatButton ImagePreviewer Input ListBox ListView Menu NavMenu OptionButtonGroup OtpLineEdit Pagination RadioButton Rate ScrollViewer Segmented Slider SplitView Switch TabControl TextBlock TimePicker TreeView Upload WindowTitleBar".split(); print("|".join("FullyQualifiedName~."+name+"." for name in names))')"
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter "$state_motion_filter"
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~.Animations.StateMotionContractTests|FullyQualifiedName~.MotionScene.'
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DataGridRangeMutationTests
dotnet build tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj --configuration Release --no-restore
```

成员审计覆盖本轮 75 个源码文件、70 个控件声明，新增问题为零；共享 Core 类型及四个几何 owner 的新增结构符合布局规则。
同范围历史基线有 511 项存量成员组织提示，当前为 484 项，尚未清零；不把本轮审计称为全库存量规范通过。
34 份相关文档的 203 个本地链接有效，`git diff --check` 通过。前轮清理移除 103 个项目外取证/缓存文件；解锁后的
原生补验再次审计无新增二进制候选，移除 116 个取证/缓存文件，`removed=true`、`audit_error=null`。专项进程已停止，
临时宿主、二进制、日志及测量快照已清理；只保留本页文本证据。原生补验没有改动生产源码或新增常驻测试。

独立只读审查的两项实质问题已由有效复现和原断言验证关闭，复核未发现新增实质问题；性能技能的过时接入指引已同步更新。
测试处置遵循 [测试价值与生命周期](../../../engineering/development/test-value-and-lifecycle.md)，运行范围遵循
[按改动影响选择验证](../../../engineering/workflows/affected-verification.md)。
