# 动画基础设施全局审查与验证

日期：2026-10-28；工作树 `feature/motion`，基线 `6c647fef1`，改动未提交。
整体职责见 [动画基础设施架构](animations.md)，本页记录实际审查、测量和完成边界。
旧阶段的测试数量不作为本页输入版本的通过证明。

## 已完成的结构审查

| 范围 | 结果与处理 |
| --- | --- |
| 有限动效 | 保留已批准的 Motion 基类、子类扩展、组合与统一请求所有权；不恢复 Actor 或第二套时钟 |
| 曲线求解 | 保留6组控制点，删除固定28轮二分求解，使用 Avalonia SplineEasing |
| 初始化命名 | StateMotion 类族归位为 Transitions/Initialization 下的 TransitionInitialization / Session / Owner；atom:TransitionInitialization.Mode 用于内部模板 |
| 初始化重入 | 构造/释放期间以当前登记身份和 Mode 判断所有权；Reset 后重新检查 owner、target 与 parent，防止已释放对象重新入链 |
| 过程反馈 | SwitchKnob、BorderBeam 使用原生无限 Style.Animations，一次运行一个作用域；取消源和每周期异步重启已删除 |
| 自动过渡 | ProgressBar、Spin 使用同一初始化协议，移除私有 Enable/DisableTransitions 调用和投递开启 |
| 内部死代码 | 删除13个无消费者的可通知 Transition 类及基类，删除9个不可达插值函数；阴影实际使用的布尔插值保留并简化 |
| 准备成本 | 空属性呈现不再捕获 Select 委托；取消资源延迟到实际启动原生时钟时创建 |

## 复用与保留依据

| 能力 | 处理理由 |
| --- | --- |
| 原生 SplineEasing、Animation、Style.Animations、Transitions | 框架拥有求解、时钟、关键帧和循环，AtomUI只补所有权与生命周期 |
| RectTransition / PixelPointTransition | 当前框架对应 Transition 列表没有等价公开类型，保留小型类型化适配 |
| Color / SolidColorBrush / BoxShadows | 当前语义含预乘透明度的通道插值；框架颜色 Animator 使用 gamma sRGB，不能按同名类型直接替换 |
| INotifyTransitionCompleted / TransitionCompletedEventArgs | 现有公开扩展契约兼容保留；内部不再依赖已删除的 notifier 实现 |
| AnimatableReflectionExtensions | 既有公开兼容入口保留签名和命名空间，源码移入 Reflection；库内初始化、有限播放及新循环路径均不调用它 |

参考源码按本地规则定位至 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`，该 checkout tag 为12.1.3；
实际编译、样条采样与样式行为验证使用项目的12.1.4包，不把本地 tag 等同实际依赖版本。

## 有效失败与行为验证

1. 登记构造回调撤销 Mode，旧实现仍发布抑制；释放旧登记时重入创建的新登记被旧回调清掉。
   两个临时用例均先失败，修正后通过，已合并入既有 live-binding/登记契约。
2. target.Reset 回调释放 owner 后，旧实现仍设置 parent，目标无法完成初始化。
   临时用例先失败后通过，合并入既有 Reset 重入契约。
3. 100ms loading 周期跳帧至350ms时，旧循环重启得到0°，原生无限时间线得到180°。
   真实 SwitchKnob 已通过；长期保障合并到既有 recurring-work 用例。
4. 样式撤销与 styled base binding 叠加时，需要单独的动画优先级覆盖跨越原生样式批次。
   最新来源0.6的恢复、隐藏暂停、显示恢复、只作用自身 host 与停止后不再更新均通过。
5. loading 运行中将周期100ms改为200ms，旧实现忽略参数变化；新增重建入口后，在新周期中点前采样90°通过。
6. ProgressBar/Spin 的自定义过渡、首帧、重挂载、首次后续过渡及最新 Transitions binding 已通过临时双场景验证。

临时场景先用于验证，必要断言合并到既有契约；当前没有净增永久测试用例。

## 前后配对微基准

环境：Apple M1 Max、Release、net10.0、Avalonia12.1.4，前后各10个独立进程，同一临时控制台程序和输入。
曲线包含顺序/交错采样、精确端点与独立64轮二分数学 oracle；误差门槛0.0031对应框架参数精度的保守输出界。

| 工作负载 | 修改前 | 修改后 |
| --- | --- | --- |
| 6组曲线共1,200,000次 Ease，中位数 | 393.96585ms | 46.1643ms |
| 求值循环线程分配 | 0B | 0B |
| 对数学 oracle 最大归一化误差 | 5.3282e-9 | 0.00253705 |
| 空属性 MotionPlan 构造 | 96B/次 | 96B/次 |
| 空属性 MotionPresentation 构造 | 304B/次 | 216B/次 |
| 100,000次呈现准备中位数 | 16.1414ms | 7.0052ms |

框架精度意味着中间采样不与旧高精度二分逐位相同；控制点及0/1端点保持一致。
空属性 plan 的 LINQ 路径在.NET10已无额外分组分配，没有据源码表象宣称消除了不存在的分配。
呈现准备数据不包含整个 PlayAsync、绑定通知、渲染或布局；这些比例不等于应用帧率提升，未证明最低设备60/120FPS。

## 当前集成与验收结果

- Core6项通过：初始化、重入、live binding、异步组合、同步取消、类型化 AddOwner 和固定成员保留。
- 过程反馈及相关控制阶段清理后48项通过。
- 初始化消费者定向261项通过，包含按钮、选择组、Tabs、虚拟化集合、输入、导航及资源绑定路径。
- 临时 Core/桌面/Gallery audit 测试目录均已删除，必要契约已合并；微基准源、二进制及专用原生报告完成记录后已清理。
- 最终清理后的 Gallery 导航与相关 Showcase 定向27项通过。
- 独立审查问题修正后，Popup/Drawer/Dialog 生命周期定向156项通过。
- 最终源码的生产 net8 Desktop.Controls 及依赖重新构建通过，0警告0错误。
- 最终源码 Gallery Release/osx-arm64 NativeAOT 发布 exit0，输出校验通过，产物为 Mach-O arm64；无 AtomUI IL/AOT诊断。
  ReactiveUI IL2104/IL3053 和本机 Homebrew dylib 构建版本高于macOS12部署目标的警告仍存在；不构成旧版macOS运行验收。

后续集成：有限 Motion/布局/相关控制300项通过，生产 net8 Desktop.Controls 及依赖构建0警告0错误。
实际原生全场景3进程144动作完成后，专项复查发现滚出视口的 Beam 仍有更新；更严格的零更新断言先在旧报告中确认12个失败动作，
接入实际视口运行条件后，屏外3进程12动作零更新，屏内3进程12动作可观测中间值。
这些是几何、生命周期与批次观测，不承诺实际呈现帧率。

完整 WorkspaceWindow 的 Skia 脚本通过两轮实际选页与侧栏往返，并核对展开标志、非零视口、父项分配与选择身份。
独立控制场景验证预选 Switch/Segmented、60%初始进度、正常状态切换、80%进度与同实例重挂载。
已检查真实合成器截图：文字自然、选中态正确、进度与指示器可见，往返后的菜单和 Footer 排列正常。

独立全工作树只读审查提出2项重要问题，均有实际定向 RED：Popup 清理回调注册的新表面被旧调用覆盖；Drawer 动效清理异常阻断宿主释放。
修正使用旧资源身份先退役、独立清理尝试与聚合报错；临时复现与相关定向43项通过，最终清理后的定向结果为156项通过。

两个复现已合并入原有 Popup 表面就绪、Drawer 开关/释放契约，临时复现删除。修正后最终专项156项通过；
新表面重复通知不重置其准备，清理异常在报错的同时完成遮罩、宿主、binding和内容释放。
Popup 原生专项3进程24个开合动作通过。最终仅报告实际执行的定向与专项结果，不累计重复运行数量，不声称全量回归通过。

Skia4个媒体文件及管理文件共5个已清理，审计 clean，无项目内派生取证二进制；原始用户媒体保留。
改动测试的 Fact/Theory 方法数382→378，差异来自既有关闭场景参数化，没有净增永久用例。

## 审查后的保留边界

独立审查无未修复 Critical/Important，也没有延期 Minor。以下判断仍有明确边界：

- 无最低硬件、跨平台和 display-present 证明，不作无条件帧率承诺。
- 本地12.1.3只作阅读参考，行为与编译以实际12.1.4包验证；样条使用框架精度，不逐位承诺CSS求解一致。
- 自定义 easing/插值器必须纯且配置不可变；不深克隆任意第三方对象。
- 合成后端及独立应用变换自动合并不在当前支持能力内；祖先排列失效没有被证明多余，不再凭猜测裁减。
- 公开非Motion兼容入口保留；未作全面ApiCompat二进制认证，未运行用户未要求的全量回归。
- 曾怀疑的 OverlayDialog 主机清理问题在追踪 DisposeAsync 独立兜底后撤回，不当成已修复缺陷。

原生观测使用 schema2，记录 Core、Controls 和 Desktop.Controls 实际程序集 hash，源改动 hash 包括未跟踪源码；
不会因为本次新 Motion 文件尚未提交而漏记身份。它不与旧 schema1报告混为同一协议的配对结果。
