# Motion 验证状态与完成门禁

本页区分[目标架构](overview.md)与当前可用实现，定义迁移、平台与发布验收。内置消费者已迁移，
代码保持在 `feature/motion` 的未提交工作树中等待审查。定向验证、原生观测与平台性能验收分别记录，不能相互替代。

## 当前源码边界

| 能力 | 当前源码与目标差异 |
| --- | --- |
| 定义与执行 | Core/Animations/Motion 已实现 AbstractMotion/Build、Motion 附加属性接入、内置家族、组合、类型化属性和原生执行；第三方子类与 atom: AXAML 消费有定向证据；变换通道写入 CompositionVisual，播放由 MotionFrameDriver 直接驱动 |
| 目标 | 真实 Control；应用 RenderTransform 可与变换动效共存；旧 Actor、接口族和 MotionScene 已删除 |
| 内容展开 | 公开 IMotionLayout 与 Controls.ContentViewport 已接入 Expander、Collapse、NavMenu Inline 和 TreeView 系列，几何 / 输入有定向证据 |
| 状态初始化 | Animations 下已有 TransitionInitializationSession/TransitionSuppressionScope，继续服从独立初始化合同 |
| 显示隐藏 | 内置消费者已迁入 Motion，Overlay Dialog 使用框架时钟并校准为普通 Zoom；实际呈现的支持矩阵尚未验收 |
| 注册 | 基础 Motion 不依赖 Desktop animator 注册；旧 TransformOperations 注册已删除，Masonry 继续拥有自己的 ITransform 适配 |
| 性能 | 已运行 NavMenu 侧栏与子菜单原生探针；没有实际 display present 数据及最低设备证据，跨平台帧率目标尚未验收 |

源码入口：[定义基类](../../../../src/AtomUI.Core/Animations/Motion/AbstractMotion.cs)、
[Motion 接入](../../../../src/AtomUI.Core/Animations/Motion/Motion.cs)、
[单次播放](../../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPlayback.cs)、
[内容视口](../../../../src/AtomUI.Controls/Primitives/ContentViewport.cs)、
[Popup 准备](../../../../src/AtomUI.Desktop.Controls/Popup/PopupSurface.cs)、
[Desktop 注册](../../../../src/AtomUI.Desktop.Controls/ThemeManagerBuilderExtensions.cs)、
[原生观测](../../../../tools/performances/AtomUI.GalleryPerformance/NativeMotionProbe.cs)。
这些路径描述现存实现，消费者迁移后同步更新，不能把旧类删除与整个目标验收完成等同。

## 定义与接入拆分验证

2026-10-28，独立基线 `2de819b6a`。定义基类为 `AbstractMotion`，`Motion` 保留为 sealed 附加属性与目标级操作入口。
六个属性仍以 Motion 为 owner，Enter/Leave 的值类型为 AbstractMotion；产品 AXAML、属性默认值、静态回调及可见性登记不变。
Core 定义/组合/运行时、控件的 OpenMotion/CloseMotion、内部动效与现有自定义测试子类同步采用 AbstractMotion。

临时消费者直接使用生产 `https://atomui.net` 的 Motion.Enter/Leave/IsVisible，实例为另一个命名空间中的自定义 AbstractMotion 子类。
重构前消费者因缺失 AbstractMotion 出现两处 CS0246，这是新 API 不可用的编译证据，不作为运行时 Bug 复现。
重构后两种首次显隐、绑定驱动开关、最新透明度来源恢复、静态请求身份/Reset 与同实例重新挂载均通过。
临时用例与 AXAML 已删除，重新构建后 Core 定向6项通过；没有净增永久测试。

清理后相关桌面控件定向918项通过，Gallery 导航及相关 Showcase 定向24项通过。
最终对相同源码构建产物使用 `--no-build` 记录 Core6、Desktop918、Gallery24 的 TRX，汇总与逐项 Passed 结果一致，无跳过项；重复运行不累计成更多用例。
首次桌面运行中的两项 NavMenu 间距测试因跨入子项模板匹配了多个同名 Frame 而失败；查找限定当前 TemplatedParent 后，
保留原有 Single 与4/0间距断言，两个场景先定向通过，再完成上述918项确认。未为此改变产品布局或模板。

结构审查覆盖9个控件文件：检查器在 TreeViewItem 的公共 IsChecked 与显式接口 IsChecked 上发生同名键异常；
其余8个文件报告41项存量成员布局问题。对全部12个发生类型引用变更的 Desktop 源文件逐字比较，
把新增的 AbstractMotion 类型名还原为 Motion 后均与基线完全相同，说明没有新增成员布局变更。
TreeViewItem 的重复名称与显式接口映射人工核对，字段仅变更定义类型；未把检查器结果宣称为全绿，也未扩大到无关成员重排。

生产 net8 Desktop.Controls 及依赖重新构建通过，0警告0错误；实际命令包含 `-p:AtomUITargetFrameworks=net8.0` 并恢复所需资产，
此前仅使用 `--no-restore` 的尝试因本地资产缺少 net8 target 失败，不属于源码编译错误。
当前输入的 Gallery Release/osx-arm64 NativeAOT 发布 exit0，输出校验通过，无 AtomUI IL/AOT 诊断；
ReactiveUI 的 IL2104/IL3053 及本机 Homebrew dylib 的 macOS 部署版本告警仍存在，不作为旧版 macOS 运行证明。

一位全新上下文审查者只读审查本轮差异及新基类，未发现 Critical、Important 或 Minor；
核对实例实现、三个静态 getter 限定、附加属性初始化顺序/CWT/回调、所有类型接入与未变化的产品 AXAML。
本轮不重新认证前一轮整个动效库或帧率、合成后端及跨平台性能；未运行全量回归，代码保持未提交待用户审查。
临时测试、AXAML、TRX、日志及专用发布验证目录在完成记录后清理，常规开发构建产物保留。

## 能力验证

实现核心前验证合成路径的真实完成/故障、当前呈现值、反向、终态提交、客户端基值和变换后命中。
原生路径验证 Delay、FillMode、首 tick 前取消、多轨道不同时长/曲线与组合结束。
验证运行资源转交终态记录后，不再保留 CTS/任务/帧订阅，Reset/下一请求/detach 可释放剩余覆盖。
布局能力的精确签名需经独立第三方库、编译 AXAML 和 AOT 验证，不能只凭友元测试通过就称可扩展。
不满足合成合同的组合使用可靠原生路径；需要框架公开能力时单独提出依赖方案，不反射内部成员冒充支持。

## 契约矩阵

| 范围 | 必须证明 |
| --- | --- |
| 构建与参数 | 实例复用、快照隔离、非法数值/时间/通道在写目标前失败、DIP/比例和 origin 正确 |
| 曲线与轨迹 | 端点及中间值、分段曲线、变换顺序、实际组件参数与参考规格一致 |
| 请求 | 整体替代、NotReady 不抢占、重复状态幂等、显式 Play 重新播放、回调重入 |
| 组合 | 一个所有权、并行同通道拒绝、子项 FillMode 校验、局部 Delay、段间保持、父时长缩放、取消未开始子段 |
| 属性 | 绑定/主题在覆盖期间更新、恢复最新值、终态显式持有、跨家族不复活旧变换 |
| 生命周期 | 外部取消、detach、换模板、容器回收、owner 销毁、清理异常、迟到通知 |
| 布局/输入 | 自然内容、固定父约束、四方向、动态内容、嵌套滚动、滚动条、裁剪和真实命中 |
| 业务接入 | Popup 首帧和物理关闭、焦点/遮罩、BackTop 快速显隐、TreeView 反向及键盘 |
| 扩展 | 普通第三方子类、自定义曲线/属性/布局无内部权限可用，不依赖全局类型注册 |
| 发布 | 实际目标宿主编译、裁剪、NativeAOT 和所声明平台运行，未使用类型可移除 |

## 迁移与事实同步

旧 API 不保留长期兼容实现。先验证核心与扩展，再迁视觉动效、业务接入和布局，消费者迁移验证后删除 Actor 类族、
双 transform、无调用的旧几何和重复执行路径。现有状态初始化与已经采用原生时钟的场景不因迁移倒退。
公共 API、主题、Semantic Part 专用 Style、Gallery API/Token/示例和单控件文档同步；视觉规格差异单独记录。

Core 和 Controls 模块入口描述当前目录与目标归属；本系统文档拥有共享合同，单控件文档不重复维护另一份运行协议。
当前布局协作细节见[内容展开](../control-infrastructure/content-expansion.md)，其中只描述现存视口与租约机制。
更新当前实现状态时必须有源码与相应验证证据，不能仅移除预实现 notice。

## 验证范围与证据

按[定向验证](../../../engineering/workflows/affected-verification.md)运行改动模块，不自行扩展到全量回归。
新验证默认临时，遵守[测试生命周期](../../../engineering/development/test-value-and-lifecycle.md)，清理后再次验证最终模块。
纯文档变更检查相对链接、目标/现状区分、命名、导航和 git diff --check，不以文档验证冒充产品测试。

原生性能门槛、支持矩阵及统计口径由[性能验收](performance.md)统一拥有；最低设备、平台、内容规模和并发必须冻结。
平台呈现、布局/交互、资源释放与发布证据分别保存，某一项通过不能代替其他项。
目前未指定最低性能配置，不能将当前开发机探针或架构预算描述为新库已达到 60/120FPS。

## 本次审查与能力证据（2026-10-28）

核心临时验证共 50 项通过，覆盖构建、轨迹、组合、替代、取消、绑定恢复、清理异常和布局扩展。
独立审查的三项重要问题均有 RED→GREEN 证据：绑定 `IsOpen=false` 的关闭意图与反向重开、关闭期间 Popup 换模板的稳定收敛、
合法 AddOwner 属性的通道适用性。相关覆盖合并到既有用例；临时测试与独立 fixture 验证结束后删除，不新增永久用例。

独立第三方程序集通过 `atom:` 编译 AXAML、自定义 Motion、实际有限时长播放、`IMotionLayout` 和 AddOwner 属性通道验证，
并在 macOS arm64 NativeAOT 可执行文件中返回 `THIRD_PARTY_AXAML_PLAYBACK_LAYOUT_AOT_OK`（exit 0）。
类型化属性校验直接调用 `ValidateValue`，避免对象转换路径引入的 IL2026；没有通过关闭裁剪检查隐藏警告。

Motion 迁移阶段清理后的工作树按以下范围验证，全部为 `net10.0` 的精确过滤，没有运行全量回归。
下述后续 NavMenu 布局修复另列其最终验证范围，不将阶段证据冒充最新输入的全量证明：

| 范围 | 结果 |
| --- | --- |
| Core MotionExecutionTests 与 TransitionInitializationTests（最长组合、AddOwner/Reset、既有状态初始化） | 5 passed |
| Controls FixedAvaloniaMemberRetentionTests（清理后重建程序集） | 1 passed |
| Desktop 布局/输入、Expander、Collapse、NavMenu、TreeView/Transfer、Masonry | 255 passed |
| Desktop Popup/Flyout/ContextMenu、FloatButton/BackTop、Badge、Drawer | 171 passed |
| Desktop Feedback/Message/Notification、Dialog/MessageBox | 280 passed |
| Gallery Badge/Modal 语义高亮、Expander/Collapse 页面 | 8 passed |
| DataGridFilterDialogPopupTests | 4 passed |

Desktop 第一组同时包含现存 Motion 目录的真实卡片关闭用例；表格是实际命令的分组，不叠加历史临时验证数量。

Motion 迁移版本（不含后续 NavMenu 布局修复）的 Gallery 发布执行 `PublishToLocal.ps1`，使用 Release、`osx-arm64`、`publishAot=true`，
返回 exit 0 与 `NativeAOT output validation passed`；产物为 Mach-O arm64 可执行文件。
该次没有 Motion/Core 的链接器或 AOT 警告。依赖 ReactiveUI 的 IL2104/IL3053，以及本机 Homebrew dylib
构建版本高于 macOS 12 部署目标的链接警告仍存在；此结果不代表旧版 macOS 或其他平台已运行验收。

原生侧栏和子菜单各运行 3 个进程、每进程 8 次开关，共 48 个动作。属性采样间隔中位数约 8.3ms，
子菜单关闭的 Rendered 批次间隔 P99 为 21.045ms；Rendered 与属性变化都不等于屏幕实际呈现。
原生 `all` 场景的第二个进程在 BorderBeam 未观测到中间值，不能报告整组通过，也不能把它直接归因于新 Motion。
这些数据没有配对基线，不构成提升百分比或 60/120FPS 保证。

Gallery 的实际 Expander 页面通过 Skia 渲染及程序鼠标输入验证，检查闭合、中段、完整展开和快速反向，
逐帧检查自然文本、裁剪与后续内容占位。截图审计及清理已完成。原生 Gallery 精确可执行文件已启动并核对进程，
但 UI 自动化无法按该路径连接；遵守本地调试规则停止该路径的自动化，没有改连同名安装版。

旧 `AtomUI.Performance` 的目标统计与四个受影响场景已迁移到现存节点；其完整工程仍受基线中的 7 个编译错误阻塞
（入口缺失命名空间、已移除的 TabControl/TabStrip 专用 ScrollViewer 类型）。这套工具不作为本次通过证据；
原生 `AtomUI.GalleryPerformance` 是独立工程，其证据按上述限定范围报告。

### 保留的设计边界

- 框架全局时钟仍是唯一计时来源；不使用合成关键帧动画（12.1.4 无完成通知与当前值回读），变换值由 UI 线程逐帧写入合成通道。
- 应用 RenderTransform 与 Motion 变换已分属不同合成通道（2026-10-29），原“独立来源需模板隔离表面”的限制取消；既有隔离表面仍可工作，未逐个删除。
- 关闭期间 Popup 原模板被销毁时，新表面预隐藏并稳定关闭，不保留已经销毁表面的剩余离场时长。
- Popup 换目标的原生首帧无闪烁、完整 Flyout Completing 链、全部 DPI/MinMax/自身固定尺寸组合及最低设备仍需专项验收。
- 没有复现框架时钟异常停滞，不引入按成功时长猜测完成的 watchdog；取消与 detach 仍释放资源。
- 自定义曲线和插值器遵守纯函数、不可变合同；请求快照不深克隆任意第三方对象内部状态。

## NavMenu 侧栏往返的原生补充验收（2026-10-28）

后续按用户录屏复现冷启动 Gallery：展开 Layout、实际点击 FlexPanel、收起再展开侧栏。
macOS 原生窗口为 1300×900 DIP、RenderScaling=2，输入通过框架 RawPointerEventArgs 管线，
每步核对 InputHitTest，不以直接修改展开状态代替点击。

修复前 Components 的 IsSubMenuOpen/展示意图均为 true，Motion 结果为 Completed，DesiredSize 高度528，
但实际 Bounds 高度0，祖先仍缓存高度44的折叠排列；内部 Layout 的视口高度220正常。
单独重测视口无效，重新排列祖先可恢复分配。当时的修复让租约建立、进度和释放统一失效自身 Measure
与全部祖先 Arrange；该方案已于 2026-10-29 被精确修复取代，见下文“布局动效失效范围收窄”。

修复后 3 个独立原生进程、各2轮侧栏往返通过：Components 视口528、Layout 视口220，
祖先实际分配包含 Header 与子菜单，选择保持，单次点击开合及叶子行左/中/右命中均通过。
原生取证使用 RenderTargetBitmap 即时渲染，只检查菜单视觉与几何，不等同显示器呈现或 FPS；
完整 Gallery 的 Skia 合成器帧另行验证。本次程序自驱动原生验收补充了此前 UI 工具无法按路径连接的证据缺口，
不扩展为所有平台或性能门禁已通过。

本次修复仅使用已有类型化 Layoutable/Visual API，无新增反射或动态注册；临时原生输入探针读取 InputRoot 的反射不进入产品。
本次未重跑 NativeAOT 发布或其他平台验收，前述发布记录仍属于迁移阶段版本。

清理后的共享布局/输入与 NavMenu 定向验证246项通过，范围为 NavMenu Inline/InlineCollapsed、
项激活、键盘、选择、ContentExpansion Motion/Boundary/Input、Expander/Collapse 行为和 TreeView 状态回放。
既有打开路径用例合并实际可见性、动效完成和父项分配断言，不新增永久用例。
临时场景移除后的 Gallery CaseNavigationLayoutTests 另3项通过，合计249项最终定向测试；未运行全量回归。
临时原生输入驱动、Skia 场景和13个取证文件已删除，媒体审计 clean；原生性能工具按正常入口重建通过。

## 展开准备正确性验证（2026-10-28）

本轮仅保留 `MotionVisibility` 的首帧占位准备与异常清理。窗口共享帧调度、背景缓存和 NavMenu 宽度迁移已撤回，
播放执行器与 NavMenu 恢复 `e0894f90b` 实现；未保留性能提升或帧率达标声明。

原失败用例在可见切换后的首个 Loaded 回调前强制布局，曾得到120 DIP自然高度，修复后Bounds与DesiredSize均为0。
准备失败用例确认临时透明度释放，恢复最新显示意图与自然布局。两个临时用例在原播放执行器下通过后删除。
清理后Core Animations定向5项、Desktop ContentExpansion/NavMenuInline/OverlayDialogPresenterMotion/CloseMotionExecution
定向92项均通过，永久测试净增0；未运行全量回归。保留源文件的结构检查1个文件、0项问题，git diff --check通过。
这份证据仅证明上述正确性行为，不证明原生屏幕闪烁已全部消除或60/120 FPS达标。

## 布局动效失效范围收窄（2026-10-29）

原实现每个进度 tick 失效视口全部视觉祖先的排列。Avalonia 12.1.4 的 `InvalidateArrange` 同时调用 `InvalidateVisual`，
`CompositingRenderer.UpdateCore` 重录该祖先绘制指令，合成端把其整棵子树边界加入重绘区域；
窗口外框等绘制型祖先因此使子菜单、Expander、Collapse、TreeView 展开退化为逐帧整窗重绘。

收窄为只失效自身测量后，临时 Headless 用例复现了上节 macOS 侧栏往返问题：Components 视口 DesiredSize 580、实际高度 2–4。
根因是框架排列缺口：约束变化导致的重新测量不清除 IsArrangeValid，祖先测量期间不转发 ChildDesiredSizeChanged，
相同排列矩形使祖先跳过排列。现由视口在排列后比较父链 DesiredSize 快照，只重新排列尺寸变化的祖先，
详细合同见[内容展开](../control-infrastructure/content-expansion.md)。

A/B 临时验证：固定尺寸边界外的绘制祖先在 5 个 tick 中被重新排列 5 次（原实现）→ 0 次；
侧栏往返 1、2 轮的子菜单分配在仅收窄版本失败，在原实现与最终实现均通过。
最终 Desktop 定向 780 项（Motion/Expander/Collapse/NavMenu/TreeView/Transfer/TreeSelect/TabControl/DialogSurface/Masonry）
与 Rendering 72 项（TabOverflowShadow）通过。临时用例已删除，全局测试净新增 0。
本次没有原生窗口 DirtyRects 或呈现帧率数据，不能据此宣称帧率达标。

## 帧驱动与合成变换通道（2026-10-29）

按[帧流水线重构设计](../../../superpowers/specs/2026-10-29-motion-frame-pipeline-redesign.md)步骤 3 的平台无关部分：
MotionPlayback 删除 MotionProgress 与 Avalonia Animation，改由 MotionFrameDriver 直接订阅框架全局时钟；
平移/缩放/旋转写入 CompositionVisual 合成通道；播放中的空帧请求合成提交。

临时 A/B（Headless，ManualMotionClock）：

| 验证 | 原实现 | 新实现 |
| --- | --- | --- |
| 延迟段脉冲后合成端存在待提交批次 | 失败 | 通过（单独及分组运行） |
| 纯缩放帧不重录目标绘制指令（5 帧） | 失败 | 通过 |
| 应用 RotateTransform 与缩放动效共存 | 抛 NotSupportedException | 通过，应用变换保持不变 |

首版通道释放直接恢复保存值，替代交接时旧请求覆盖了新请求刚呈现的缩放/平移；
Dialog 与 Feedback 卡片“进入中关闭”的连续性用例失败后改为按通道所有权交接，相关用例通过。

读取 RenderTransform 的既有动效断言改为读取合成通道（测试辅助 MotionTransformProbe），断言值不变。
最终 Desktop.Controls 4128 项中 4119 通过；9 项失败在会话开始前的基线上同样失败
（Dialog 4 项 9 DIP 偏差、通知悬停重验证、MessageBox 最小宽度、DatePicker 范围 3 项）。
Core Motion 8 项、Controls 27 项通过；Gallery 412 项中 19 项 SemanticPartHighlight 失败，基线同样失败。
临时用例已删除，全局测试净新增 0。

未完成：透明度仍逐帧写 UI 属性并重录目标自身绘制指令；平台帧计时（预计上屏时间）、NavMenu 根宽度迁移、
mask 独立图层未实施；没有原生窗口呈现数据，Windows/macOS 未运行验证。

## Wayland 呈现时间采样（2026-10-29）

Native 绑定 `wp_presentation` 并按提交请求 feedback，Core 的 MotionFrameDriver 按预计上屏时间采样（实现与边界见
[Native Linux](../../../modules/native/linux.md)）。本机 Wayland、60Hz、Debug，真实 Gallery；同一进程内只切换 Core 时钟，
Native feedback 保持开启；每变体预热 2 轮、每方向 20 次、动作窗口 650ms。

**NavMenu 内联子菜单（Components）**：首版预测即使上屏帧数不变，布局通知由每动作 342–376 降至 14–19。

**Overlay Dialog＋mask 同时动画**（用户报告不足 60FPS 的场景）：Modal 页 BasicDialog 设为 modal，打开 300ms、关闭 200ms，
触发点改为离窗口中心最远的可见按钮（1259,72；窗口 1300×900），放大缩放位移。只数上屏帧会掩盖卡顿，
因此临时为每个上屏帧标注其内容在动画时间轴上的时刻，统计相邻上屏帧内容是否恰好前进 1 个刷新：

| 版本 | 方向 | 上屏 FPS | 漏刷新 | 内容均匀前进 | 重复 | 跳帧 | 内容−上屏 中位 |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 脉冲时间采样 | 开 / 关 | 58.6–59.8 / 58.6–59.8 | 1–19 | 46–65% / 51–63% | 38–65 | 33–69 | 落后约 40ms |
| 首版预测（绘制完成登记） | 开 / 关 | 59.1 / 60.0 | 5 / 0 | 81% / 73% | 29 / 23 | 36 / 37 | 0 |
| 最终（绘制开始登记、帧消费推进、残差修正、起点顺延） | 开 / 关 | 57.8 / 59.3 | 14 / 3 | 83% / 83% | 33 / 26 | 29 / 16 | 0 |

脉冲采样下即使上屏约 59FPS，仍有 35–54% 的帧内容重复或跳跃，与用户观感一致；这不是上屏帧率问题。
首版预测在该场景曾**劣化**上屏（未计入已占用准备槽，见上节设计修正），后续每一项修正都有同进程对照。
最终版本第 3 帧起内容基本逐刷新前进（各帧位置不良步数 0–3/20），剩余问题：

- 起始帧仍重复一次（18–19/20 动作），即动画晚一个刷新开始，不再伴随跳帧；
- 漏刷新是真实慢帧，同次运行两种采样同时升高（14/19、3/11），主要来自 mask 的整窗重绘与 Debug 构建，
  属于设计步骤 4（mask 独立图层）范围，采样调度无法消除。

因此**不能宣称 Overlay Dialog＋mask 已稳定 60FPS**。数据为合成器 presentation feedback，非高速相机；未测 120Hz、VRR、
多输出、Release/NativeAOT，不代表 Windows/macOS。临时宿主与内容标注已删除，无新增永久测试；
Desktop.Controls 4119/4128（9 项基线失败不变）、Core 8、Controls 27 通过。
