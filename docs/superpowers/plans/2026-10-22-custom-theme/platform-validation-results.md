# CT-25 多平台验证入口与当前 Mac 产品夹具结果

本分包完成多平台工作流、真实 Browser 运行验证器及既有产品夹具接线，241文件快照经用户批准以cfb1d6668提交推送，已执行首轮Actions。**五个运行时任务通过，三个选测任务失败，CT-25整体保持进行中**。执行范围见[方案](platform-validation.md)，修复记录见[后续包](platform-followups.md)。用户允许使用AtomUI/AtomUI Actions，指定AtomUI凭证，禁止发布到nuget.org。

## 工作流与产品夹具

`verify-affected.yml` 新增默认关闭的 `all_platforms`／`browser` 手工输入。默认 PR 保留 macos-14、原检查名称和原 artifact 名称。多平台手工入口运行三 OS 的选中测试与真实主题资产物化、三个实际 OS 后端 NativeAOT 夹具，以及 Linux Chromium 下解释／Mono AOT 两个 Browser 夹具。仍需显式 base；本轮不选择 full。权限仅 contents:read，checkout 不保留凭证，只上传文本日志／receipt，不调用发布工作流或 NuGet 发布命令。

实际发布暴露并修复了夹具的存量接线：第三方五个单字段 Token 各自声明既有 ID 0；Application 使用 partial 生成启动入口；Browser 显式 UseAtomUI；语义专用 Style 通过编译 AXAML 声明，并在 Application 实例 Initialize 加载；Desktop 显式 STAThread Main。未改产品注册引擎、公共 ID 表或增加保留描述符。运行断言改为检查真实启用包的提交顺序和五个实际选中注册描述符，避免把旧 eager 列表或完整 manifest 声明冒充裁剪后注册。

Browser SDK 的四个重复端点经追踪确认：包路径和共享 ProjectReference 输出字节与语义元数据相同，无冲突。去重仅限 fixture 的最终 WasmStaticWebAsset 项，位于 ResolveWasmOutputs 后、BootJson 前，不更改注册输入或输出验证门禁。

验证器使用 Python 标准库、本机 HTTP 和独立 Chrome profile，临时观察脚本回传真实 `pre#result`、浏览器异常和 User Agent；再次严格校验唯一成功行、产品错误和对应 receipt 的 ABI／包身份。实际发布后端负责完整哈希门禁，Python 入口不声称独立重验全部哈希。随机回传地址和严格 Origin 限制本次回传。结束／超时清理本次进程组、server、profile。

## 失败前证与实际运行

临时工作流验证最初 2 FAIL／2 正对照 PASS；补齐两类显式入口后通过。真实 Browser 发布先暴露五项缺失 ID 诊断及静态 Configure 错用实例 Styles；这些夹具缺陷均在实际编译路径修复。期间两次缺少 using 的编辑编译失败不充当产品验证。解释模式首次运行在读取旧空列表时失败，改查实际启用包与运行时注册后完成全部断言。

最初 AOT 的 dump-dom／virtual-time-budget 在 4.491 秒以空 DOM 退出；同一发布物改为持续运行，在 5.260 秒完成 23 项断言，证明抓取早于异步启动完成。最终入口等待真实断言与有界墙钟，不使用虚拟时间判断完成。曾误归因为 WASM 的 allocator 文本实际来自 Chrome stderr，已纠正；未为此修改产品或放宽成功判据。

当前宿主为 macOS 26.6.2／arm64、SDK 10.0.300、Browser workload 10.0.10、Chrome 156.0.8078.48。User Agent 的平台字符串经过浏览器缩减，宿主架构单独记录。

| 路径 | 实际结果 | 限制 |
| --- | --- | --- |
| trimmed Browser 解释执行 | 23 项产品断言及 PRODUCT_PASS browser，5.942 秒 | 当前 Mac 的 Chromium／SwiftShader |
| Browser Mono AOT | 46 个程序集编译；23 项产品断言及 PRODUCT_PASS browser，5.348 秒 | 同上；不是 Linux CI 结果 |
| 真实 OS NativeAOT Complex | 25 项产品断言及 PRODUCT_PASS selected，2.110 秒 | 当前 Mac 原生窗口／标题栏／Popup／OTP；不是逐控件视觉验收 |

两种 Browser 发布无编译或 IL 警告；Native 发布无 IL 警告，仍有五项既有 Homebrew OpenSSL／Brotli 部署目标链接警告，不承诺旧版 macOS 支持。夹具覆盖模板、局部 Own 更新与清除、作用域、暗色切换、实际资源和独立注册触发；不替代持续动效、性能、全部家族、像素或最终原生显示面义务。

## 清理与最终校验

10 项临时单元验证和 6 项真实运行验收均通过。后者分别拒绝空页、源码伪标记、产品失败与浏览器异常，接受延迟产生的实际成功，并验证 1 秒超时后自有父子进程消失。三个临时文件、追踪 helper、两份 Browser／一份 Native 发布和全部临时 runtime 目录共 424 文件／257,835,480 字节已删除，无残留自有 Chrome profile 进程；净新增常驻测试 0。

清理后 Build.Tasks 三个精确注册／门禁类 23 项通过，GalleryBase 注册类 1 项通过，Gallery 两个源读取类 10 项通过。四个 C# 文件结构检查无问题（无控件声明），所有控制流保留大括号。作者自审发现检查名称可能影响既有 required check，已保持默认原名；最终 YAML／七个 Bash 块解析通过，tooling iterate 49.321 秒通过。共享 Minimal 宿主四项断言及 PRODUCT_PASS full 通过，确认非 Complex 的共享入口仍可运行。没有运行本地 change／全量回归。

上述本地审查时源码尚未提交、远端没有feature/custom-theme。随后用户对具体241文件快照明确回复“我同意”，该快照以cfb1d6668提交并推送，调度[首轮验证](https://github.com/AtomUI/AtomUI/actions/runs/40053351877)。三个真实OS NativeAOT和两种Browser模式通过；Windows选测在BOM项目解析前置失败，Linux选测DataGrid18项失败，Mac选测Desktop两项失败，整体CI未通过。实际运行与[后续包](platform-followups.md)持续记录；不能把五个运行门禁等同于全部平台验收。CT-25既有重套模板保留失败、原生边缘像素以及各家族出口继续保留，未由本分包关闭。
