# Mono TypeMap 后端接入 iOS/Android

日期：2026-10-31。状态：AtomUI 侧已实现，iOS arm64 模拟器已验证；Android 未验证；未提交。

问题来源：AtomUI.Mobile `docs/superpowers/progress/2026-10-31-atomui-mono-typemap-registration.md`。iOS 宿主（Mono）
启动即 `NotSupportedException`，来自生成 accessor 调用的 `TypeMapping.GetOrCreateExternalTypeMapping<TGroup>()`。
正式设计见 [其他 Mono 宿主](../../architecture/foundations/aot-browser-linking.md#8-其他-mono-宿主)。

## 1. 根因

1. `build/AtomUI.Registration.targets` 只给 `browser-wasm` 接入 Mono 转换后端，iOS/Android 的 accessor 原样进入产物。
2. `AtomUISetRegistrationPublishMode` 只要 ILLink 运行就写 `AtomUI.Registration.Trimmed=true`；iOS SDK 每次构建都运行
   ILLink（含 Debug 与 `TrimMode=copy`）。
3. 实测发现的第二个根因：官方 ILLink 只从 action 为 Link/CopyUsed 的 `EntryPoint` 根解析 TypeMap
   （`RootAssemblyInput` 才调用 `SetEntryPointAssembly`）。iOS 默认 `partial` 下应用程序集不可裁剪即为 Copy，
   TypeMap 不解析，转换器把所有 Group 物化为空映射：应用启动无崩溃但界面全黑。Browser 为 `full`，应用程序集是 Link，未暴露。

## 2. 修改

- `build/AtomUI.Registration.targets`：新增 `AtomUIResolveMonoTypeMapBackend` 在执行期判定 Mono 宿主与后端接入；
  `Trimmed` 开关只在非 Mono 或后端接入时写入；Prepare/Capture 改用该判定；非 Browser 的 `ATOMUIREG006` 文案；
  Apple 的 `custom-linker-options.txt` 进入链接输入，`MtouchLink|MtouchInterpreter|AndroidLinkMode` 进入签名配置；
  入口程序集原为 Copy 时改为 CopyUsed；新增 Apple 打包输入（`_ComputeStripAssemblyIL` 前）与 Mono AOT 输入
  （`_AOTCompile` 前）消费校验。Android 消费校验未实现。
- `Backends/ILLink10/RegistrationAbi.cs`：存在可达 accessor 而 ILLink 无 TypeMap 入口程序集时报 `ATOMUIREG006`。
- `Tasks/ResolveRegistrationToolsTask.cs`：缺失后端提示改为 Mono。
- `tests/AtomUI.TypeMap.Linker.Tests/LinkerFixture.cs`：手工 LinkContext 补设 EntryPoint 程序集，使夹具与真实 ILLink 状态一致。
- 文档：`aot-browser-linking.md` 第 8 节，`aot-and-trimming.md`、`startup-and-registration.md`、
  `modules/typemap-linker/overview.md`、`modules/toolchain/overview.md` 的范围描述。

## 3. 验证

环境：SDK 10.0.300，Microsoft.iOS.Sdk 26.5.10301，iPhone 17 Pro 模拟器 iOS 26.5（arm64），本地包 `6.2.11-local.3`
（打包到会话临时目录，未写入共享本地源），Mobile 仓库通过临时 `CustomAfterMicrosoftCommonTargets` 禁用其
`Trimmed=false` 绕过，未修改 Mobile 源码。

| 场景 | 结果 |
| --- | --- |
| arm64 Debug（partial），修复前 TypeMap 入口 | 构建通过但两个 map 均 0 条目，界面全黑 |
| arm64 Debug（partial），修复后 | AtomUI.Controls 7 条、AtomUI.Mobile.Controls 15 条；回执 `output-verified`；链接输出、打包输入、AOT 输入三道校验执行通过；产物 AtomUI 程序集无 `GetOrCreateExternalTypeMapping` 引用（包内原始 IL 有）；首页与 Button 页渲染与全量注册基线一致 |
| no-op 增量 | 签名 `contentChanged=False`，回执与链接产物字节不变 |
| `MtouchInterpreter=all`（`TrimMode=copy`） | 后端未接入，无 `Trimmed` feature、无回执，全量注册启动正常 |
| 负例：AtomUI.Mobile.Controls 不可裁剪 | `ATOMUIREG006 ... action 'Copy', expected Link` |
| 负例：入口程序集强制回 Copy | `ATOMUIREG006 ... ILLink has no TypeMap entry assembly` |
| Browser/桌面判定 | ZeroBrowser `PublishTrimmed=true` → 后端接入、`false` → 不接入；Minimal osx-arm64 → 非 Mono |
| 单元测试 | TypeMap.Linker 89/89、Build.Tasks 91/91、Generator BuildInfrastructure 54/54 |

切换注册模式后增量构建曾出现 `ShimSkiaSharp` AOT 模块过期导致的 Mono 启动中止；清理 iOS obj 后消失，属于 iOS SDK
dedup AOT 增量问题，与本修改无关。

## 4. 未覆盖

- Browser 真实发布：`Browser` fixture 当前 `ProductApp.cs(69)` CS0120 编译失败，`ZeroBrowser` 在构建阶段
  `GenerateWasmBootJson` 重复键失败，两者均早于任何注册目标，未能完成端到端回归。
- iOS Release/真机、tvOS、Mac Catalyst 未测；Android 未安装 SDK，消费校验挂接点未实现。

## 5. Mobile 侧跟进

- `AtomUI.Mobile.Controls` 必须声明 `IsTrimmable=true`（或宿主 `TrimmableAssembly`），否则 partial 链接报错。
- 新版本发布后删除 `build/MobileHost.targets` 及宿主导入。
