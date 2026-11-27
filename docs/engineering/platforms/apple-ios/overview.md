# Apple iOS 工程环境

本文是 AtomUI/Avalonia iOS Host 的工程入口，维护稳定的工具链、签名、资源、构建、Simulator/真机和排障边界。具体机器、
Xcode/SDK 版本、设备、路径、性能和视觉校准结果进入日期化 evidence，不作为长期支持矩阵。

## 适用范围

- 创建或维护 AtomUI/Avalonia iOS Host。
- 配置 Apple Development 签名并部署开发设备。
- 使用 Simulator 验证布局、资源、启动页和基础生命周期。
- 验证 Debug/Release、AOT/trim、bundle 资产、安装和启动。
- 排查证书、profile、AppIcon、LaunchScreen、资源 URI 和设备状态问题。

本文不定义 Mobile Control API、Runtime 状态机或 Gallery 内容；这些契约分别由
[Mobile 系统架构](../../../architecture/systems/mobile/overview.md)、
[Mobile Controls 模块](../../../modules/mobile-controls/overview.md)和
[Mobile Gallery](../../../gallery/platforms/mobile-gallery.md)维护。

## 前置条件摘要

- Xcode 和 Command Line Tools 已安装、完成首次启动，并指向同一有效 Developer Directory。
- 与仓库目标框架兼容的 .NET SDK 和 iOS workload 已安装。
- Apple Developer 账号属于目标 Team，开发证书和 private key 可用。
- App ID、development provisioning profile 和目标设备 membership 一致。
- 真机已配对、启用开发者模式，并在需要远程启动时处于解锁状态。

具体检查、签名身份和 keychain 规则见 [工具链与签名](toolchain-and-signing.md)。

## 验证层级

| 层级 | 证明内容 | 不能证明 |
| --- | --- | --- |
| Toolchain | Xcode、.NET iOS workload 和签名材料可被发现 | App bundle 可安装或启动 |
| Build | 指定 target/RID 的 Debug 或 Release 构建成功 | 真机触摸、生命周期和无障碍 |
| Simulator | 资源、LaunchScreen、布局、旋转和基础启动 | 真机性能、VoiceOver、签名部署体验 |
| Physical device | 安装、启动、触摸、IME、前后台和 VoiceOver | Release/AOT/trim 未执行时的发布状态 |
| Release | Release/AOT/trim 产物构建、安装并启动 | App Store/TestFlight 发布 |

Simulator、真机、Release 和 Publication 必须分别记录。某一层通过不能替代更高层证据。

## 专题导航

| 文档 | 职责 |
| --- | --- |
| [工具链与签名](toolchain-and-signing.md) | Xcode/.NET iOS、证书、App ID、profile、device membership 和 keychain |
| [项目资源与启动](project-resources-and-launch.md) | 最小 Host、AppDelegate、项目属性、AppIcon、LaunchScreen 和 Avalonia 资源 |
| [Simulator 与真机](simulator-and-device.md) | 发现、安装、启动、锁屏行为、Simulator 和截图证据 |
| [构建与验证](build-and-validation.md) | Debug/Release、RID、签名参数、bundle、AOT/trim、性能解释和收尾门禁 |
| [排障](troubleshooting.md) | 以可观察症状为入口的有限诊断序列 |
| [2026-08-03 iOS Gallery 预览验证](../../../developments/progress/2026-08-03-apple-ios-gallery-preview-validation.md) | 一次机器、设备、版本和视觉校准快照 |

## 不覆盖

- App Store/TestFlight、企业签名和发布证书流程。
- Push、Associated Domains 等额外 entitlement 的产品配置。
- 未经实际 Host 验证的“推荐版本”或兼容组合。
- Android 工具链和验证流程。
