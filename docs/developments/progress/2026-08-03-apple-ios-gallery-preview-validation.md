# 2026-08-03 Apple iOS Gallery 预览验证

> 状态：日期化验证证据。本文记录一次本机、工具链、设备和页面布局快照，不是 AtomUI 的 iOS 支持版本矩阵或当前发布声明。

## 验证目的

该次工作用于证明一个 AtomUI/Avalonia Gallery iOS 预览可以完成开发签名、Device/Simulator 构建、安装、启动、AppIcon 与
LaunchScreen 打包，并观察 Debug/Release 启动差异。稳定规则已经整理到
[Apple iOS 工程环境](../../engineering/platforms/apple-ios/overview.md)。

## 环境快照

| 项目 | 2026-08-03 快照 |
| --- | --- |
| Host OS | macOS |
| Xcode | 26.x |
| .NET | 10，Homebrew 安装 |
| Avalonia | 12.1 |
| Physical device | iPhone 11 Pro Max |
| Simulator runtime | iOS 26.5 |
| Device/Simulator architecture | arm64 |

这些版本只描述该次执行环境。后续 Xcode、.NET、Avalonia 或设备组合需要重新验证，不能从本表推导最低/最高支持版本。

## .NET 路径快照

该机器为避免 shell shim、IDE task host 与 `DOTNET_ROOT` 不一致，显式使用：

```text
DOTNET_BIN=/opt/homebrew/Cellar/dotnet/10.0.300/bin/dotnet
DOTNET_ROOT=/opt/homebrew/Cellar/dotnet/10.0.300/libexec
DOTNET_ROOT_ARM64=/opt/homebrew/Cellar/dotnet/10.0.300/libexec
```

这些路径是该机器的 Cellar 快照，不进入正式构建规范。

## 签名标识快照

| 项目 | 该次示例 |
| --- | --- |
| Bundle ID | `com.atomui.isogallery` |
| Development profile | `AtomUI_iOS_Gallery_Development` |
| Certificate | `Apple Development: <anonymized> (<team-id>)` |
| Device ID | 已匿名化 |

验证使用 Apple Development certificate 与 iOS App Development profile。证书和 profile 与本机 private key、App ID 和目标
device membership 配对；未验证 Developer ID、App Store/TestFlight、Enterprise 或额外 entitlement 流程。

## 实际执行链路

原始验证记录确认使用或核对了以下命令族：

- `xcodebuild -version`、`xcode-select -p` 和 Xcode first-launch/component 状态。
- `security find-identity -v -p codesigning` 与 provisioning profile UUID 解析。
- .NET iOS Device Debug/Release build，RID 为 `ios-arm64`。
- .NET iOS Simulator Debug build，RID 为 `iossimulator-arm64`。
- `xcrun devicectl` 的 device discovery、install 和 process launch。
- `xcrun simctl` 的 runtime/device-type discovery、create、boot、install、launch 和 screenshot。
- 最终 `.app` 中 `Assets.car`、`LaunchScreen.storyboardc` 和 `Info.plist` key 检查。
- AppIcon PNG 尺寸与 alpha 检查。

项目路径、device ID 和私有签名身份未保留为正式文档数据。

## LaunchScreen 校准

该次 iPhone 11 Pro Max 页面布局测量后，LaunchScreen logo 使用了：

```xml
<constraint firstItem="launch-logo"
            firstAttribute="centerY"
            secondItem="root-view"
            secondAttribute="bottom"
            multiplier="0.31"
            constant="44.1" />
```

`0.31` 和 `44.1` 只适用于当时的 logo、正式页面 Grid、Safe Area、设备族和截图测量。它证明的是“需要从正式页面可见位置
推导 storyboard constraint”，不是可复用的设计常量。页面结构、素材或设备变化后必须重新测量。

## Debug/Release 观察

- 极简页面的 Debug 首屏约为数秒，包含调试/诊断和较少优化路径。
- Release 真机启动明显更快。
- 该次记录没有形成可复用的精确启动耗时、采样分布或性能门限。
- 因此结论仅为“不能使用 Debug 启动感受评价产品性能”；后续性能声明必须使用 Release 真机、明确冷/热启动和采样方法。

## 已验证排障结论

- 显式传 login `CodesignKeychain` 曾导致 .NET iOS signing detection 找不到本来可用的 key；本机默认 login keychain 更合适。
- 设备锁屏时，App 可以已经安装，但远程 process launch 会因无法解锁而失败。
- AppIcon 需要通过 asset catalog 生成 `Assets.car`，PNG alpha 会影响主屏视觉。
- LaunchScreen 需要 `UILaunchStoryboardName`、`InterfaceDefinition` 和 bundle 内 `LaunchScreen.storyboardc` 同时成立。
- Avalonia SVG/图片优先检查 `avares://` URI 与资源打包，不应先归因于 iOS 不支持。
- 设备/Simulator 可能缓存旧图标或启动资产，确认新 bundle 后卸载重装可以区分缓存与构建问题。

## 未覆盖

- Android Host、Android Emulator/真机和 TalkBack。
- App Store/TestFlight、Enterprise、Ad Hoc 发布。
- 多设备性能基线、长期 Xcode/.NET/Avalonia 兼容矩阵。
- Mobile Controls 源码、Mobile Gallery 正式项目或发布包。
