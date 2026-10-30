# Motion 重构能力验证

日期：2026-10-27。位置：`/Users/chinboy/Projects/dotnet/AtomUIV6-motion`；分支 `feature/motion`；基线 `6c647fef1`。
本文记录实际能力原型和源码依据，正式合同由 `docs/architecture/systems/motion/` 拥有。

## 原型结果

`dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter FullyQualifiedName~MotionCapabilityTests --no-restore --verbosity minimal`
结果：4 passed、0 failed。

- animation priority 的 opacity 覆盖为 .3 时，底层绑定从 .8 更新到 .6，覆盖仍为 .3；释放后恢复 .6，后续绑定仍有效。
- 原生 Forward 动画首 tick 前取消可把 default(double)=0 写入进度属性。不能让它直接控制应用的基础值或新请求的共享进度。
- 每次创建独立的非视觉 Animatable，原生 Animation 可使用框架全局时钟完成，不需要 Control/Actor 或反射 Clock。
- 对单个 setter 显式设置 TransformOperations animator 后，Core-only 进程可以完成该动画，不需要 Desktop 注册。

## 精确依赖源码

当前依赖 Avalonia 12.1.4，对应本地源码位于 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`。
`Animator<T>` 通过 `clock ?? control.Clock ?? Clock.GlobalClock` 选择时钟，解释了非视觉对象的原型结果。
`AnimationInstance<T>` 的 Forward final fill 使用 SetValue 写入最后插值值，未 tick 的初始字段为默认值。

`Animation.AnimatorRegistry.cs` 的 SetAnimator 将 setter 放入静态 Dictionary，公开路径没有对应移除接口。
因此局部精确适配证明了注册边界，但逐次为临时 setter 注册会形成持续持有；新核心先通过 double 进度与类型化求值避免该路径。
原型只在专用测试进程中创建有限注册项，临时验证结束后按项目规范删除。

合成端的可靠完成、当前呈现衔接和命中尚未取得完整验证，不因 RequestCommitAsync 或 batch Rendered 而标记支持。
原生路径作为当前已证明可用的执行基础；性能和最终平台呈现仍需在真实 Gallery 单独验收。

## TDD 起点

新 Motion 公开合同测试已运行失败：Core 中尚不存在 `AtomUI.Animations.Motion`，ShouldNotBeNull 断言失败。
这条临时验证用于启动新 API 的 RED→GREEN；后续需以真实播放、替代、绑定恢复与生命周期行为验证，不能止于类型存在。

## 重构后的验证进展（2026-10-28）

以上是实现前的能力原型记录；当前源码已经完成消费者迁移并删除旧 MotionScene/Actor 类型。
最终事实以[正式验证记录](../../architecture/systems/motion/verification.md)为准。

- Core 临时能力验证共 50 项通过，第三方 AXAML、有限时长播放、外部布局能力与 AddOwner 属性已在独立 NativeAOT 可执行文件中实际运行通过。
- 独立代码审查发现的三项重要问题全部先复现失败再修正：Popup 绑定关闭/反向打开、关闭期间换模板、AddOwner 通道适用性。
- 修正 AOT 对象转换警告时保留类型化属性验证，没有关闭链接器检查。
- 临时测试、fixture 与渲染截图已按生命周期清理；必要契约合并到既有用例，无净增永久测试。
- 原生 Gallery 的 NavMenu 侧栏和子菜单探针分别完成 48 个动作；观测数据没有配对基线及实际 display present，不能解释为 60/120FPS 保证。
- Skia 实际 Expander Gallery 页面通过渲染、输入和快速反向检查；原生应用按精确路径连接 UI 自动化失败，未连接其他同名应用代验。

平台支持矩阵、最低硬件和完整性能验收仍是独立门禁。合成后端暂未启用；通用原生变换使用模板隔离边界。
