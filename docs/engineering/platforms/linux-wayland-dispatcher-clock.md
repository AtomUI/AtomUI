# Linux Wayland Dispatcher 时间源修复

## 接入与原理

`UseAtomUIPlatformDetect()` 选择原生 Wayland 时，会在原 Wayland 初始化器执行完成后、所有平台就绪回调和
Application 创建之前，执行一次共享时钟适配。默认构建使用官方 Avalonia 12.1.3 包。

Wayland 的输入队列会在 worker 就绪之前创建 Dispatcher；随后安装的 Managed 后端另起 Stopwatch，
而 Dispatcher 仍保留首次创建时的时间源。两个时间基准不一致时，Dispatcher 计算的绝对定时截止值会在后端延迟执行。

私有成员访问集中在 `AtomUI.Core/Reflection/DispatcherReflectionExtensions.cs`；Desktop 平台入口只负责
调用时机。适配器取出 Dispatcher 已有时间源捕获的 Stopwatch，将默认 Managed 后端的 `_clock` 指向同一个
对象。Dispatcher 的时间源、已有绝对定时截止值保持不变；后端读取时间、判断到期和等待全部使用该对象。
不修改 Startup，不重启时钟，不添加偏移补偿、定时唤醒或持续动画。

## 兼容边界

兼容边界限定为 Avalonia 程序集版本 `12.1.3.0`，并检查精确后端类型、闭包类型和 Stopwatch 字段。
反射信息按项目约定集中声明为 `Lazy<FieldInfo>`，使用 `FixedMemberReflection.RequireField` 校验，
并通过精确成员级 `DynamicDependency` 声明裁剪依赖；执行路径复用缓存。重复调用保留同一时钟引用。
不支持的版本或结构跳过写入并记录 Warning；升级框架时必须重新验证这条边界。

仅经 AtomUI 平台选择入口配置的默认 Wayland Managed 主循环适用。直接调用 Avalonia `UseWayland()`、
可选 GLib 主循环、X11、macOS 和 Windows 不由该适配器修复。非 Wayland 平台不会调用它。

## 构建与启动

```bash
dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj \
  -c Debug --framework net10.0

ATOMUI_WINDOWING_PLATFORM=wayland \
  .agents/skills/atomui-gallery-local-debug/scripts/launch-gallery-debug.sh --no-build
```

## 验证与维护

- 官方 12.1.3 Gallery Debug/net10.0 构建通过，实际加载的 Base/Wayland 与官方包内 DLL 一致。
- 链接实际反射适配和平台入口的 linux-x64 NativeAOT 宿主发布无裁剪警告；重复接入返回成功，
  16ms DispatcherTimer 实测约15–16ms。宿主没有额外私有成员反射或裁剪保留配置。
- 精确筛选的 Core 固定成员反射测试2项、Desktop平台选择与Linux AOT边界测试34项通过。
- 临时宿主、探针、旧源码补丁构建入口和专用产物已清理，没有新增永久测试。

NativeAOT 宿主验证不等于完整 Gallery NativeAOT 发布，也不代表其他平台验收。
计时器测量不是屏幕 FPS。更新 Avalonia 时应重新核对固定字段与编译器生成的闭包结构，
验证裁剪后可共享同一时钟，并复核原始 Gallery hover 场景。
