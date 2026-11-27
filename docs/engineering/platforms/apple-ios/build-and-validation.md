# Apple iOS 构建与验证

本文维护 iOS Debug/Release、device/Simulator RID、签名参数、bundle 检查、AOT/trim 和启动性能解释。实际 Host 存在后，
项目文档可以在这些稳定规则之上补充已执行命令。

## 构建输入

构建前确定：

- `<ios-csproj>`：真实 iOS Host 项目。
- `<ios-target-framework>`：仓库集中配置并由已安装 workload 支持的 iOS TFM。
- `<profile-name>`：development provisioning profile 的显示名称。
- `<app-path>`：本次构建实际产生的 `.app` bundle。
- `CodesignKey`：可由当前 keychain 枚举的 Apple Development identity。

多个 .NET 安装并存时，使用同一已解析 `dotnet` 和匹配的 `DOTNET_ROOT`。如果 shell/IDE 注入的 `SDKROOT` 指向错误 SDK，
应在诊断确认后为该次 .NET 构建清除它，而不是硬编码另一台机器的 SDK 路径。

## Device Debug 与 Release

Device 使用 `ios-arm64` RID。命令形状：

```bash
dotnet build <ios-csproj> \
  -f <ios-target-framework> \
  -c Debug \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Development: <identity> (<team-id>)" \
  -p:CodesignProvision="<profile-name>" \
  -v:minimal
```

Release 只改变配置并执行对应优化/AOT/trim 链路：

```bash
dotnet build <ios-csproj> \
  -f <ios-target-framework> \
  -c Release \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Development: <identity> (<team-id>)" \
  -p:CodesignProvision="<profile-name>" \
  -v:minimal
```

本机开发不默认传 `CodesignKeychain`。CI 或专用 keychain 场景先完成 identity 枚举和 keychain search-list 验证，再显式传入。

## Simulator Build

Apple Silicon Simulator 通常使用 `iossimulator-arm64` RID；真实目标以 Host 项目和当前 SDK 为准：

```bash
dotnet build <ios-csproj> \
  -f <ios-target-framework> \
  -c Debug \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -v:minimal
```

Simulator build 不证明 device signing、真机安装或 Release AOT。x64 runner 或其他架构需要使用与 runner/Simulator 实际匹配的
RID，不能机械复制 arm64。

## Bundle 检查

构建成功后，不只检查退出码；至少验证：

```bash
test -d <app-path>
test -f <app-path>/Assets.car
test -d <app-path>/LaunchScreen.storyboardc
/usr/libexec/PlistBuddy \
  -c 'Print :CFBundleDisplayName' \
  -c 'Print :CFBundleIdentifier' \
  -c 'Print :UILaunchStoryboardName' \
  <app-path>/Info.plist
```

还应确认 executable、embedded provisioning profile、Theme/Localization/DataTemplate 和应用必需 Avalonia 资源在最终 bundle 中
可发现。具体检查随真实 Host 和发布方式扩展。

## AOT 与 Trimming

- 首次 Release 可能因 link、trim 和 AOT 明显慢于 Debug/增量构建；耗时本身不是失败。
- Analyzer 和普通 compile 不能替代 Release 产物验证。
- 反射、字符串成员访问和动态资源发现必须符合仓库 AOT 规范。
- Release 只有在 `.app` 真实安装并启动后才能标记通过。
- Mobile adapter、ControlTheme、Token、Localization 和 DataTemplate 必须在裁剪后仍可发现。

## 启动性能解释

不要用 Debug 首屏判断产品启动性能。Debug 可能包含诊断、调试和较少优化的运行路径。稳定结论要求：

- 使用 Release 真机产物。
- 保留设备、系统、构建、冷/热启动定义和采样方法。
- 原生 LaunchScreen 在 .NET/Avalonia runtime 初始化前可见。
- 不把 `devicectl launch` 命令耗时等同于用户点击图标后的首帧时间。
- 页面、资源或 LaunchScreen 变化后重新部署 Release 再比较。

一次 Debug/Release 观察见
[2026-08-03 iOS Gallery 预览验证](../../../developments/progress/2026-08-03-apple-ios-gallery-preview-validation.md)，它不是长期性能基线。

## 最终验证清单

1. Toolchain、workload、certificate/private key、profile 和 device membership 已确认。
2. Device Debug 与 Simulator build 按适用场景通过。
3. Release/device build 执行真实 AOT/trim 链路。
4. `.app` 中 `Assets.car`、`LaunchScreen.storyboardc`、plist identity 和应用资源完整。
5. Simulator 完成资源、布局、旋转和启动页截图验证。
6. 真机完成安装、解锁启动、触摸、IME、前后台和 VoiceOver 验证。
7. Release 真机产物完成启动与性能采样。
8. 证据记录区分 Build、Simulator、Device、Release 和 Publication。
