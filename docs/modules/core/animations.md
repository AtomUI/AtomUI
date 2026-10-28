# 动画基础设施架构

本页定义 AtomUI.Core 动画基础设施的整体职责、目录和框架复用边界。
当前实现、全局审查、测量与门禁见 [验证记录](animations-verification.md)。
有限动效的公开合同由 [Motion 系统](../../architecture/systems/motion/overview.md)拥有，
状态过渡的初始化合同由 [Transition 初始化](../../architecture/systems/control-infrastructure/transition-initialization.md)拥有。
源码迁移和平台证据见各系统的 verification 页面，不把目标目录视为已经验证完成。

## 职责分层

| 能力 | 所有权 | AtomUI 增补 |
| --- | --- | --- |
| 时钟、关键帧、循环、属性过渡、样条求值 | Avalonia Animation、Style.Animations、Transitions、SplineEasing | 使用公开入口，不实现第二套帧时钟或贝塞尔求解器 |
| 有限显式动效 | AbstractMotion 定义与 Motion 接入 | 定义与组合、每目标请求身份、连续替代、绑定恢复、终态和布局能力 |
| 初始状态提交 | Transition 初始化协议 | 暂时抑制自动过渡，等待 owner 提交完整基线，再恢复最新过渡来源 |
| 持续过程反馈 | 对应控件与原生循环动画 | 控件决定运行条件；原生动画推进循环；停止时释放本次样式/运行作用域 |
| 缺失的插值及兼容契约 | 最小适配器 | 仅保留框架缺少或语义不等价的能力，明确其使用者与差异 |

AbstractMotion 是有限动效的定义基类，Motion 拥有附加属性和目标级操作，二者不互相继承。
初始化会话不生成轨迹、不运行时钟、不继承 AbstractMotion，命名不得暗示其属于另一套播放系统。
重复反馈没有自然成功终点，其生命周期不伪装为不断完成并重建的有限 Motion 请求。

## 目录与命名

```text
Animations/
├── Motion/                         # AbstractMotion 定义、Motion 接入、构建、内置家族与有限播放
├── Transitions/
│   ├── TransitionInterpolation.cs   # 保留的颜色/阴影等语义适配
│   └── Initialization/             # TransitionInitialization、会话、可撤销抑制
├── AnimationExtensions.cs          # 必要的原生动画生命周期适配
└── BaseTransitionUtils.cs          # 既有过渡创建契约
```

`TransitionInitialization` 是模板目标登记入口，`TransitionInitializationSession` 拥有初始化阶段与作用域，
`ITransitionInitializationOwner` 把基线完成责任留给具体控件。三者沿用 `AtomUI.Animations` 命名空间，
内部模板仍使用 `atom:`；迁移须同步 marker、接口、字段、AXAML 与当前文档。

## 框架复用与曲线

Ant Design 的曲线控制点属于 AtomUI 的效果规格；求解使用 SplineEasing。
同名 Circular/Quintic 公式不代表相同控制点。保留参数，采用框架数值精度，验证端点、密集采样和交错调用，
不以另写固定次数二分换取未经证明的收益。已发布的 easing 配置按不可变输入使用，不在播放中修改共享配置。

框架插值的颜色空间、透明度、阴影数量和不兼容值切换策略必须逐项比较。不能因为类型名相似就替换有不同视觉合同的实现。
公开非 Motion 兼容 API 与已无消费者的内部代码分开处置；不借内部清理擅自移除公开契约。

## 初始化与生命周期

owner 拥有业务状态和几何，session 只抑制过渡并管理就绪。显示、选择、焦点和资源绑定的所有权不转移到 session。
初始基线、重新挂载和回收后的基线不应补播默认值过渡，后续真实变化照常使用原生 Transitions。

属性通知可能同步重入。登记清理后必须核对当前请求与登记身份；构造抑制作用域的回调不能让已经撤销的登记重新生效。
owner 或 target 在 Reset 回调中释放时，不得继续建立父子登记，也不得留下阻止目标就绪的已释放 parent。
过渡抑制按作用域恢复最新绑定与主题值，不 ClearValue 或写死原始集合，不用 Dispatcher.Post 猜测就绪。

## 持续反馈

优先在 ControlTheme 表达原生循环。运行条件依赖实际视口、运行期时长快照，或声明式绑定不能等价到达目标时，
允许控件拥有一次原生 Style.Animations 作用域，并在注释中说明原因。无 selector 的实例 Style 只作用于自己的 host，
不匹配后代；该行为必须通过实际依赖版本验证。

开始只安装一次循环，停止只释放这次作用域；不每周期重建 Task、CTS 或动画实例。
适配器明确声明单个反馈属性。StyledProperty 在原生样式移除期间使用独立的动画优先级覆盖，
退出样式批次后再恢复最新底层来源；DirectProperty 只用于控件自己拥有的反馈值，沿用框架直接绑定语义，
不承诺应用源覆盖。该限制不能扩展到公开 Motion 的属性通道，后者只接受有可恢复覆盖的 StyledProperty。
非正时长不能形成同步忙循环。隐藏、卸载、参数变化与重复启动必须有确定的停止和重建边界。
显式反馈动画独立于 Transition 初始化抑制，不能被初始化机制意外关闭。

## 性能与验证

构建与准备允许与请求复杂度相关的分配；运行帧使用冻结轨迹和类型化操作，不进行类型扫描、全局注册、反射或重复 LINQ 建模。
没有属性轨迹的普通 Motion 不应建立属性分组对象；同步稳定态不应持有没有使用的取消资源。
布局视口、期望占位和祖先排列遵循 [共享内容展开](../../architecture/systems/control-infrastructure/content-expansion.md)，
不得通过只更新展开标志或隐藏交互问题来降低测量成本。

每项性能结论记录相同宿主、输入和采样方法的前后数据。算法采样、属性变化、Rendered 批次和屏幕呈现分别报告。
支持矩阵及实际帧预算由 [Motion 性能验收](../../architecture/systems/motion/performance.md)管理；没有最低设备与呈现证据时不作无条件帧率承诺。
