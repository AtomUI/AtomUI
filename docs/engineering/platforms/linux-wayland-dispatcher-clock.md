# Linux Wayland Dispatcher 时间源

## 当前接入

AtomUI 的 Avalonia 最低依赖为 12.1.5。Dispatcher 由框架维护统一时间轴：替换平台后端时保持当前时间连续，
向后端提交 timer 截止值时转换到后端时间轴。Wayland worker 初始化前创建 Dispatcher、随后安装 Managed 后端，
不再需要 AtomUI 将两个 Stopwatch 合并。

`UseAtomUIPlatformDetect()` 的 Wayland 入口仍按顺序安装帧节奏适配、执行框架 Wayland 初始化器，
但不再读取或改写 Dispatcher 的私有字段。初始缩放与 EGL 帧节奏是不同能力，各自按实际成员契约校验，
不再以版本或 commit 白名单决定接入；程序集版本只用于诊断。

ApplicationStartup 仍在平台服务就绪后启动后台 Prepare，保留既有配置回调、UI 线程与 Application 构造边界；
移除时钟补丁不改变这些启动契约。

## 历史问题与移除依据

旧版框架在首次创建 Dispatcher 时固定时间源，后来安装的 Managed 后端另起 Stopwatch，绝对 timer 截止值
在两个起点之间没有转换。后台初始化耗时因此会反复叠加到定时器间隔。原 AtomUI 适配通过共享同一 Stopwatch
修复此问题。

框架现在原生完成时间轴转换，旧 `_timeProvider` 和其编译器生成闭包已经不存在。继续保留反射适配既没有作用，
也会留下失效的裁剪保留声明；因此已删除内部适配及平台入口中的调用，不以修改其他 hook 代替。

## 升级验证要求

升级框架或更换 Dispatcher 后端时，验证以下独立契约：

1. 在后端初始化之前创建 Dispatcher，并留出可观测的初始化起点差。
2. 初始化前排队的 timer 保留剩余等待时间；更换后端后时间不倒退或跳变。
3. 初始化后启动的 16ms timer 不再反复额外等待后端起点差；驱动真实主循环，不能只调用 `RunJobs()`。
4. 在真实 Wayland Gallery 中复核 hover/Transition、Popup 和退出行为；定时器周期不是屏幕 FPS。
5. 对实际 trim/NativeAOT 产物运行启动与定时器检查。签名核对、Headless 与 macOS 结果不能代替 Linux 运行证据。

帧节奏和初始缩放的职责、线程与版本边界见 [Native Linux 平台 API](../../modules/native/linux.md)。
