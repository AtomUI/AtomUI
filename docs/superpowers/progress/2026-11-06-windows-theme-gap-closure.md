# Windows custom-theme 缺口补齐

本记录延续[原 Windows 验收](2026-11-05-windows-theme-acceptance.md)，适用当前
`feature/custom-theme` 未提交源码。没有创建提交、推送或发布。历史脚本缺失的边界不变；
31-owner 验证采用已公开完整控件清单的当前重建协议。

最新结果：Windows x64完整平台专项已通过（608.063秒），此前.NET10的40626像素差异已在完整重验中消失。
.NET8/.NET10分别36对完整窗口图像逐字节一致；最后工具模块检查85项通过（iterate总120.782秒）。
具体输入、边界和原失败记录见本文末尾；不将其他执行快照的回执升级为同一全局成功上下文。

## 产品修正

- Native 的 StyleRemovalHook 在原生通知前冻结被移除的样式、继承树范围及实际旧 frame，在通知后逐元素退役。
  Window、Application、普通 Avalonia Window、嵌套 Styles、移除/替换/清空均纳入。Core 只持有内部宿主租约，
  Desktop 安装 Native 工厂；没有新增 Core→Native 依赖、反射生成、属性恢复或框架替换。
- 临时行为测试先观察4个真实失败，再覆盖11个行为/生命周期场景通过，包括异常、重入、通知前脱树、
  多租约、订阅顺序恢复和GC。验证后删除临时测试；保留的真实平台协议按需运行，不增加全局xUnit用例。
- 独立复核另发现同一Style在Remove回调内Add+Apply的新frame被删除、较早Closed回调抛异常时普通窗口被保活，
  两项均先真实RED。逐frame清理、当前实际样式图判定和弱窗口租约修正后，5个边界行为通过，含子作用域移动和
  绑定Dispose重入。ValueStore的internal类型无法用于net8 UnsafeAccessor；Native隔离6个固定反射成员并静态保留，
  不进行类型发现、代码生成、框架替换、节点属性恢复或全样式失效。最终双代AOT仍由下述真实专项确认。
- ILC8/runtime8.0.33、Windows ILC10/runtime10.0.14修复实际NU1903漏洞。SDK10.0.300、
  ILLink8/8.0.27及ILLink10/10.0.8保持独立版本。版本、源码清单和矩阵证明见
  [修补评估](../audits/2026-11-06-windows-servicing/assessment.md)。
- Reactive依赖迁移消除原先14条展开后的裁剪/AOT诊断；内部Unit工厂保留GalleryBase命令返回类型。
  新生成映射覆盖显式Card路由的实际失败已修复，采用每宿主显式工厂定位器。外部命名空间变化见
  [迁移说明](../../modules/toolkits-gallery-base/reactive-dependency-migration.md)。
- 冷包脚本修复Windows Git路径分隔符、UTF-8日志读取与过期payload白名单；包构建使用正式完整清单，
  独立包/HTTP缓存，桌面验证不会宣称Browser完成。
- 真实冷消费另发现旧应用宿主非partial，被当前生成器拒绝；Windows命名资源与引用宿主改为partial，
  保留手写Attach及全部资源/模板断言。随后NETSDK1112暴露包内修补版本默认值只影响build、不参与restore；
  runtime/compiler选择一并移到仓库专属import，外部应用自行显式声明。普通消费保留SDK默认值以验证此缺陷，
  Native8/桥接与机制矩阵选择已验收修补包，版本/ABI门槛不变。配置见
  [外部应用的修补包选择](../../architecture/foundations/aot-net8-conditional-backends.md#外部应用的修补包选择)。
- 项目内NuGet缓存的XLIFF被自动扫描成应用资源，实际引发ATOMUILOC006。Localization targets按规范化的
  非空NuGetPackageRoot只排除自动发现，保留包显式导入与模块身份；临时真实MSBuild先3项RED、后5种路径GREEN，
  正式新包的项目内缓存应用亦编译并运行通过。相邻目录、应用自有语言与空根均有正向对照。
- 固定SDK的全局RuntimeFrameworkVersion也会覆盖ILLink工具版本。托管trim使用FrameworkReference元数据选runtime，
  Native使用全局runtime/compiler；实际net8托管35项已证明runtime8.0.33与ILLink8.0.27可以独立工作。
  Windows托管桥接门槛核对SDK解析的唯一runtime包身份/10.0.14版本，物理pack、签名和输出链校验继续执行。
- 消费脚本改正绝对工程路径重复拼接及链接AXAML的item类型。未用托管控件改查真实TypeDef并检查裁剪前存在，
  避免把合法ConditionalRecord字符串误判为类型保留；不删除未用控件或放宽裁剪要求。
- 七路产品协议固定完整35项断言及唯一精确runtime标记，逐项顺序核对；真实日志揭示原先36的计数笔误。
  缺项、重复项、错误runtime和重复runtime的临时反例均被拒绝，没有削减实际功能断言。
- no-scanner路径使用固定SDK实际接受的`IlcArg --noscan`，并要求编译器报告Scanner=false；
  原先未被SDK读取的属性不能作为关闭证据。旧包托管桥接另以SDK真实解析的10.0.8包验证拒绝，
  不伪造ResolvedRuntimePack、不修改恢复包；坏ABI和错误runtime均不得留下任何成功阶段回执或EXE。
- 桥接阶段指针逐字节绑定对应不可变回执，published proof再绑定实际linked proof；
  不将可变指针的当前哈希误当作旧linked哈希。托管与原生桥接的实际输出链均有诊断验证。
- 临时产物退役支持Windows长路径，跳过readonly、`.git`及所有重解析点。
  独立复核发现junction会进入根外目录；仅以两个新建owned临时目录实际复现RED，修正后外部sentinel保留，
  同时普通文件及超过300字符的长路径正向清理通过。该临时验证与工程均清理，未修改机器权限。

## 已执行的原生轨迹

| 轨迹 | 实际结果 |
| --- | --- |
| 重建31-owner/40公开实例、880×700、12阶段 | net10托管1185项、NativeAOT1186项；36张完整HWND画面逐字节一致，零差异 |
| 同协议net8.0.33 NativeAOT | 1186/1186；正常motion、保留原实例、正向1DIP变化及恢复检查 |
| 全局普通类型样式，16根、Light/Dark×四尺寸 | 两代NativeAOT各135项；模板主/次Button实际font/padding/radius恢复；同步移除约2–3ms |
| 微软拼音实际输入 | 两代原生各31项；TextBox/SearchEdit/TextArea，物理SendInput，ni预编辑、主题更新、你好提交、Escape取消、Tab焦点 |
| 实际Gallery全路由 | 托管与.NET10.0.14 NativeAOT各78路由/316项/465种实际控件类型；原生无CoreCLR |
| 原始ILC8机制矩阵 | 新8.0.33编译器46/46；23scanner、23no-scanner、38优化、8未优化；实际PE/COFF/DIR64/不可变输出链 |
| 正式包构建 | 正常脚本产出20包（14产品/分析器及6语言相关包），Release双TFM合同通过 |

控件注册包消费、CLI专项与最终模块结果由当前源码执行的
`.artifacts/verification/specialist.json` 和 `.artifacts/verification/windows-custom-theme/`
保存；临时输出只在专项完成且摘要、哈希已保留后清理。不将旧日志手工标记为新回执。

## 可复验入口与范围

策略为 native-aot、platform-interaction、package-layout 和 conditional-registration-engines 注册真实Windows
命令：`pwsh -NoLogo -NoProfile -File scripts/verification/verify-windows-custom-theme.ps1 -Check <id>`。
通过 `scripts/verification/test.py check --id <id> -- <上述命令>` 绑定当前源码和环境。
失败、缺前提、未知ABI、篡改输入/输出或不完整轨迹不能写成功回执。组件缓存已删除，每次执行实际协议。
Gallery路由证据强制NativeAOT与.NET10.0.14；5项临时反例确认空清单、可变源码缓存、托管Gallery与不受覆盖的宿主
不能绕过。native-aot/platform-interaction的verifier_excludes在实际plan.changes上检查，含--base已提交差异；
Browser、Mac/Linux与发布工作流不能复用Windows桌面回执。另2项临时RED→GREEN验证这一匹配边界。
每项检查保留最新成功或失败的专属输出；替换回执时清理旧的已标记临时根，只读Git缓存不会强制删除。

包专项比较独立下载的正式6.2.8基线，执行真实worker隔离。条件引擎专项运行七条独立冷缓存产品路径、
旧包桥接、实际坏ABI拒绝及原始46行矩阵。检查耗时以脚本输出为准，通常数十分钟。
WindowsTheme、WindowsGallery和WindowsPackages保留独立价值/触发/成本说明，不进入每次全局xUnit运行。

当前机器只有x64/x86 C++工具，没有ARM64工具或运行硬件。本轮Native8交付限定Windows x64；
Gallery的Windows ARM64发布目标保留，不能把x64证据扩展为ARM64运行认证。历史macOS8.0.27
证据也不自动认证修补后的8.0.33。

共享Reactive依赖变化仍触发策略的完整依赖兼容义务。仓库明确禁止未获“全量回归”指令时自行运行全量；
聚焦检查和真实Windows轨迹不能代替该回执。此限制保留原义，未通过删改策略使状态变绿。

本机没有创建符号链接的Windows权限。原有3个目录安全测试的前提捕获未识别实际IOException/0x80070522，
已补精确错误码并把原silent return改为明确Skip；聚焦该类6通过/3前提跳过。未将这3项记作实际符号链接验证。

## 末行图标语义复核

用户指出31-owner画面的末行图标异常。第一个菱形是明确声明的测试图形；第二个ToggleIconButton
的选中路径原为开放折线，PathIcon按填充隐式闭合后形成三角形；未选中路径仅为两条零面积线段，不能形成可见叉号。
仅修正WindowsTheme/Scene.axaml的两个填充轮廓，保留31-owner、40实例、880×700、12阶段及全部原断言。
临时验证真实加载该AXAML的路径，检查对勾空心区/两臂、叉号四臂，并在同一控件中执行true/false/true及
false/true/false，检查真实模板的可见图标、尺寸和owner画刷。原路径4失败，修正路径4通过；临时测试代码随即删除。

这次修订发生在原冻结快照的5,595项聚焦测试、85项工具自测、引擎/冷包/Gallery专项之后。
这些结果保留其原始源码及环境上下文，不能迁移为新快照的全局回执；新图标以最新平台交互专项的实际结果为准。
上一轮末帧失去前台且出现遮挡，严格原生窗口断言按原标准失败，失败日志及画面保留在系统临时证据目录
foreground-failure；不通过像素容差、裁切或忽略活动窗口来消除失败。

随后一次输入专项在注入键盘前因窗口未取得前台而失败。临时控制器虽按每PID首次出现激活，
但缺少明确启动握手，因此不能仅凭次数和Avalonia输入计数证明操作发生在轨迹之前，不以其执行认证成功。
其后严格检查实际发现6582个变化像素集中在右侧和底部：主窗恢复普通层级后，自建背景窗仍在其他应用后面，
圆角及阴影露出了变化的桌面。失败画面和输入失败均保留，未修改像素标准。

最终执行改用fixture内明确的ready/go启动门：任何轨迹及其原500ms准备期开始前，发布PID、
主窗/背景窗HWND及单调时钟并等待。runner核实两窗属于直接启动的进程，在这个门内完成自建标题栏激活、
背景窗紧邻主窗之后的层级及指针停放，记录准备时间再释放。go之后不再操作窗口或注入准备输入，
完整轨迹的前台、无输入及全幅零差异断言全部保留。自然/标题栏激活的独立微软拼音探针均31项通过，
新快照仍须由完整平台专项的实际结果判定。
临时测试源码已删除；空目录的清理被自动审批以blocked by policy拒绝，未转用其他删除途径。

## 上一轮实际结果：未全部通过

按用户要求结束本轮测试，不继续循环重跑。执行快照为
`b64735b21f0d5b44c59a4a2cc038de5d4827f142b4ef9de9484d90c58145265e`，
实际产物位于系统临时目录`aw-s_42bjxb`，注册平台专项回执保持`failed`。

- .NET8/.NET10各自的全局样式135项、微软拼音31项、原生31-owner场景1186项、
  托管31-owner场景1185项全部通过，各自恢复比较仍为零差异。
- .NET8托管/原生36张完整窗口截图相等。
- .NET10托管/原生36张完整窗口截图均不相等，每张有40626个RGB像素差异；alpha无差异。
  此项未修复、未豁免，不能宣称Windows全部验收完成。差异根因仍需进一步定位。
- 最新验证工具自测85项通过，测试映射审计15项目/751文件/750类/319组通过；
  图标4项RED/GREEN、删除临时源码后的10项控件检查通过。
- ready缺失但仅打印成功marker并exit0的进程原会误通过，实际负例RED后已修为拒绝，GREEN通过；
  独立审查确认Important关闭。部分鼠标输入失败会尝试释放left-up后仍报告失败。

原快照的5595项模块测试、引擎七路径及46行矩阵、冷包、Gallery专项保留原执行记录，
不升级为最后快照的全局成功回执。完整依赖兼容回归、三个符号链接权限前提和ARM64验证仍开放。
未创建提交。

## 继续修复：完整窗口背景的启动验收

复查上一轮原始BMP，40626个RGB差异全部位于窗口圆角及外露边缘，控件内部没有RGB差异；
原生帧外缘露出桌面内容，托管帧为自建灰色背景。原启动门仅核验PID/前台及一次层级操作，
没有验证背景的实际覆盖和显示。真实负例将同进程背景缩为1×1，旧代码仍接受，形成RED。

现在仅在go之前使用可恢复的线程级per-monitor DPI上下文，核验完整窗口物理边界被背景覆盖；
等待DWM合成后，四个外侧采样点必须命中同进程背景HWND并呈现其声明的303030颜色。
保留边界、采样点/root/颜色及时间，之后恢复调用线程DPI上下文。go之后不再操作窗口，
全幅字节比较及原有轨迹断言不变，不通过裁切、容差或改写图片消除差异。
同一1×1负例现被明确拒绝（GREEN）。自然激活和受控标题栏激活均已通过新启动门及1186项场景检查；
新原生36帧与此前背景正确的托管36帧逐字节相等。此项仍需完整注册平台专项作最终判断。
独立静态审查没有Critical/Important问题。临时探针源码已删除，保留必要日志、元数据及帧哈希。

依据：[GetWindowRect的DPI虚拟化](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect)、
[线程DPI上下文](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setthreaddpiawarenesscontext)、
[DwmFlush合成边界](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmflush)。

## 最终复验：Windows x64平台专项通过

完整注册命令`check --id platform-interaction --timeout 14400 -- pwsh -NoLogo -NoProfile -File
scripts/verification/verify-windows-custom-theme.ps1 -Check platform-interaction`实际exit0，
外层和内层均为passed，耗时608.063秒；实际产物根为系统临时目录`aw-myipdzl7`。
执行源码快照为`1c5724e9cb2ab33ad5b17742663064ad106e300787e5aa188d8793cf3cfb2700`。
本段及页首结论在执行结束后补记，不伪装为执行前的源码状态。

- NET8和NET10的发布/构建/G1/IME/native31/managed31共12个实际命令全部exit0。
- 两框架各G1 135、真实微软拼音31、原生场景1186及托管场景1185项通过；同一实例完整轨迹恢复比较仍为零。
- 各36对完整窗口BMP逐字节一致，共72对。完整窗口、正常motion、原12阶段及1DIP正向检错不变。
- 独立复核186项文件哈希全部匹配；四个完整场景的启动元数据均包含物理边界及四个实际背景采样点。
- 清理后工具模块命令`run --scope iterate --path scripts/verification/windows_desktop.py`实际iteration-passed，
  85项工具自检通过，工具阶段85.234秒、含发现和校验总120.782秒。
  之前一次有84通过/1失败，原因是当前进程PATH未包含已安装Git Bash；为验证进程补上
  `D:/softwares/Git/bin`后完整模块命令通过。原失败日志保留，没有改断言或跳过。
- 临时探针及其专用帧/门目录已删除，仅保留必要日志、元数据和哈希。静态审查没有Critical/Important问题。

原快照的5595项模块检查、七路径引擎/46行矩阵、冷包和Gallery结果继续保留其各自输入身份。
完整依赖回归、三个符号链接权限前提、ARM64仍未认证；本次通过的是既定Windows x64平台专项，
不能据此声称全分支、全平台或统一最新上下文的所有门禁全部通过。未提交代码。
