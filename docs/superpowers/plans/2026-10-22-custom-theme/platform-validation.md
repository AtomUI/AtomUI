# CT-25 多平台验证执行方案

用户于2026-10-24授权使用 AtomUI/AtomUI Actions，明确禁止发布到 nuget.org；随后指定使用 AtomUI 凭证。已通过 Git 的该仓库 HTTPS credential helper 在内存中取得凭证并调用 GitHub API，确认目标仓库与 Actions 可访问。未打印或写入令牌，未改 CLI 的全局登录状态。

远端当前默认分支为 release/6.0；feature/custom-theme 尚无远端分支。现有 verify-affected.yml 仅运行 macOS，release-gallery.yml 构建安装包，release-nuget-packages.yml 有发布入口。本任务只使用验证路径，不触发 NuGet 发布流程。当前源码仍未提交，不把远端其他提交的成功运行算作当前候选通过。

拟在现有 verify-affected.yml 增加显式手工多平台输入，保留默认 PR 的原有 macOS 行为。手工多平台覆盖 Windows、Linux、macOS 的 CI selected tests 与真实主题资产物化；Desktop 使用已有 Complex 产品 fixture 做实际 OS 后端 NativeAOT 启动、模板／scope／暗色切换检查，Linux 经 xvfb，分别记录宿主与限制。Browser 使用现有 Browser fixture 的 trimmed interpreter／Mono AOT 发布和实际 Chromium 运行，必须出现 PRODUCT_PASS browser，并检查输出 receipt，不以页面加载或编译成功替代运行。仅保存文本日志／receipt，不发布包、不上传临时视觉媒体。各平台的颜色／几何／交互与原生显示面仍须逐项证据，不以该 smoke 代替全部家族验收。

先完成本地可审查源码与工作流、临时 RED/GREEN、清理和聚焦校验，再申请必要的提交及远端分支创建／推送授权。用户此前的 Actions 授权不被扩大为提交或创建分支授权；未经具体方案的“我同意”不创建远端分支。此处是待执行方案，尚无 Windows/Linux/Browser 通过结论。

实现补充：复用已安装的Chrome命令行，无需浏览器SDK依赖；以[官方Headless CLI](https://developer.chrome.com/docs/automation-and-testing/headless)启动独立临时profile和SwiftShader软件后端。最初dump-dom／virtual-time-budget路径在真实Mono AOT下早于异步启动完成，已用同发布物的持续运行单变量对照证明（4.491秒空DOM对5.260秒完成23断言）。现由仅本机HTTP响应临时加入的观察脚本回传真实DOM、浏览器错误与User Agent，使用独立随机回传地址和严格Origin；Python只接受实际pre#result中的精确PRODUCT_PASS browser，并再次拒绝错误、重复和非成功receipt。不制造产品标记，不以HTML源码文字或页面加载代替运行。等待采用有界真实时钟，结束／超时清理本次进程组、server与profile。此入口只证明产品fixture合同，不作为连续动效、性能或像素证据。
