# CT-08B Context 样式宿主与退役结果

范围见[计划](style-hosts.md)，在既有 `feature/custom-theme` worktree、提交 `79f2fc00e` 之上实施。主 agent 独立完成和作者自审，不使用 subagent、Computer Use、全量回归或新建分支/worktree；本轮没有创建 commit。

## 实现与边界

新增三个 Core 文件，拥有四个 internal 类型：`ThemeStyleContext` / `ThemeStyleGeneration`、`ThemeStyleHostLease`、`ThemeStyleUpdate`。它们由现有 Application 初始化、Provider 和 ThemeContextLease 实际调用，替代此前只有准备组件、没有产品接线的状态。

- Context 惰性持有表现状态和独立引用资格，Snapshot/Token 资源真源不变；纯 Token 准备不创建可观察样式状态。
- 每个需要资产的宿主创建独占原生图。候选先完整准备、按原位置接线，再切换资格；表现 Commit 仅换引用，之后退役。空集合、恢复和重复释放均有明确阶段；失败的恢复/退役保留一个待完成更新，下一次准备拒绝越过它。
- 应用根只匹配无逻辑父级的 TopLevel。Popup 沿实际 owner 的 styling/inheritance parent 继承；独立 Window 使用已有 owner lease。祖先已承载同 Context 的资产时不在 Window 重复挂载，避免意外提高优先级。
- 候选紧邻旧组插入，后续应用 Style 保持原顺序。退役重挂保留组清除模板 frame；最终卸载使用仅匹配宿主的临时无资源锚点触发原生失效并立即移除。原生部分修改后抛错时，独立清理各子 owner 并汇总异常，失败可重试。
- 已完成更新句柄释放旧图/Context/宿主；已 Dispose 的 lease 释放宿主。工厂执行前登记待更新状态，初次挂载未 ready 的宿主拒绝重入更新。

这不是品牌选择 API 或完整事务协调。**有效样式集合的继承/重置计算、全部 root/scope/catalog/package 入口、内容 NoOp、Manager generation/完整拓扑校验、两类结果及外层 cleanup 边界的表现失败报告/有界重试，仍属 CT-08C。** Provider 的 Token `Inherit=false` 不能被当作样式输入合并已完成的证据。没有改产品 Token、ControlTheme、控件尺寸/颜色、XML v1 或编译 ABI v1；95 个产品 Own 终端和完整控件家族视觉验收仍为 0 项迁移/完成。

## 失败证据与验收

环境：macOS26、.NET10.0.8、实际项目 Avalonia12.1.4。临时源使用所属测试项目 `Temporary/ThemeStyleHosts`，所有 C# 文件声明完成后删除。精确命令：

```sh
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.ThemeStyleHosts --logger 'console;verbosity=normal'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.ThemeStyleHosts --logger 'console;verbosity=normal'
```

| 对象 | 有效失败与修正后证据 |
| --- | --- |
| 首帧接线 | 根 Window Context 为 null、Provider 资格为 null，2/2 FAIL；修复真实根 selector 和接线后通过。最初 Provider fixture 先设 Content 再设 Context 导致的无继承错误已修正，不计为有效 RED |
| 阶段、退役恢复与宿主释放 | 第一轮扩展 9 项 6PASS/3FAIL：无 host 可越过 Wire、保留图 Remove 抛错后丢失、已 Dispose lease 仍持有 host；同断言修正后 9/9 PASS |
| 应用优先级、工厂重入 | 12 项 10PASS/2FAIL：候选追加到尾部改变后续应用 trigger 优先级；工厂能覆盖 pending；相邻插入和提前登记后通过 |
| 初次装配重入 | 14 项 13PASS/1FAIL：Mount 回调能对未 ready 宿主启动更新；增加实际宿主状态门槛后通过 |
| 完成句柄 GC | 保留已完成 update 时旧图弱引用仍 alive，1/1 FAIL；释放完成句柄引用后相同 GC 断言通过 |
| 纯 Token 路径 | 加入既有事务测试后 65 项 62PASS/3FAIL；诊断定位到样式状态提前创建触发日志回调、空资产宿主错误检查全局 Dispatcher。改为表现状态惰性创建和实际 owner 访问检查；实际资产工厂仍要求 UI 线程。原断言与 fixture 恢复后通过 |
| 根窗口重复资产 | 应用 trigger 预期 padding11，Window 重复资产导致24，1/1 FAIL；按真实 styling 祖先去重后通过 |
| 重复准备 | 作者自审补充1/1 FAIL：Wire后再次PrepareHosts能替换候选引用；增加Created/Preparing/Prepared单次门槛后同断言1/1 PASS，保持实际已接线图，补充临时源随后删除 |
| 最终 Core 临时验收 | **19/19 PASS，0SKIP，总1.6243s**。包含父/兄弟24与重置子3/红色、同实例/局部值、候选/激活/恢复、host stamp 失效、接线/激活/恢复/退役失败及重试、部分 AddOwner/RemoveOwner、关闭、已有代的新窗口和失败 owner 替换 |
| 真实编译 Part 与模板 | 专用 `CardHeaderStyle` 在 AXAML 声明 padding17/LightCyan，代码只观测；同 Card/Header/Content 连续10代品牌↔空集合，旧 Source frame 全部消失，host 始终只有活动组 |
| PopupRoot / Overlay | **Desktop临时3/3 PASS，0SKIP，总3.7824s**。现有及关闭后重新打开的两类实际 host 均继承 owner，换空集合后恢复基准且旧 Part Source 不再存在；模板实例保持 |

默认 Headless 在 Native 请求下实际回退 Overlay，第一次独立断言确实失败，未将其记为 Native 通过。随后仅在本次验证中暂设既有 fixture `OverlayPopups=false`，断言真实物理 host 为 PopupRoot / OverlayPopupHost；结束后恢复原 fixture，产品默认未变。这是 **Headless 后端的两类宿主验证，非 macOS 原生窗口验证**。曾尝试嵌套 session 导致外层 Dispatcher 清理访问错误，停止相关进程后改用原 fixture 的临时配置；不把宿主错误算作产品行为失败。

## 脚本化视觉兼容

仓库外独立临时 .NET 驱动引用当前源码，以 `UseSkia` + `UseHeadless(UseHeadlessDrawing=false)` 自驱动真实 AtomUI Window。固定客户区480×360、默认主题、Provider、Card、主/默认 Button 和选中 CheckBox；覆盖实际 Transition 时长且连续真实帧稳定后捕获。不缩放、不裁剪、不遮罩、零 RGBA 通用容差。

执行 `dotnet run --no-restore --project <系统临时目录>/Visual.csproj`，由 `ATOMUI_STYLE_PHASE=before/after` 选择同一场景的采样阶段；`ATOMUI_STYLE_EVIDENCE` 为 evidence 工具登记的仓库外 artifacts 路径。Frame compare 精确命令形态：

```sh
python3 /Users/chinboy/.codex/skills/scripted-visual-acceptance/scripts/frame_compare.py --run <本次登记run> --before before.png --after after.png --expect identical --diff-name final-diff.png
```

- 同版本 A/A：480×360，**0 个差异像素**。
- 临时 Card 左间距1DIP检错：同尺寸，**3,692 个差异像素**；随后恢复间距。
- 最终产品改动前后：480×360，**0 个差异像素**。已查看 before、最终 after 和1DIP差异图；Card 边框/标题/间距、文字、两类按钮、CheckBox 均无可见差异。
- 驱动一度因同步 session Dispose 在自身工作线程等待而不退出；managed stack 证明后改为异步释放，最终进程 exit0。没有通过 CUA 或人工输入推进。

这只证明该默认代表场景的 Skia 兼容，不是所有主题/控件/状态、原生最终显示面、交互无闪烁或 NativeAOT 结论。

## 测试处置与最终工作树

临时 Core19项、重复准备补充1项、Desktop3项及编译 AXAML 默认全部删除；不增加常驻用例。复用既有 `ThemeContextLeaseTests` 的安装/释放用例补资格一致、卸载不残留样式的断言：现有 bridge/variant 检查不能发现漏接样式；编译器也不能保证实际绑定/释放。这是同一共享生命周期路径的扩展，没有新增 helper 或进程，执行成本随原用例，属于每次全局检查仍应保持的共享基础设施不变量。

媒体审计无发现；登记目录清理成功（6个文件，包含取证清单），独立临时项目/编译产物、诊断 stack、Core/Desktop 临时源和 AXAML 均已删除。两份重建后测试 DLL 均不含临时 namespace；原 fixture 和事务诊断断言已恢复。表现状态仅为 typed observable 源，不是非 Visual DynamicResource 绑定目标；没有新增 Application.ResourcesChanged 全局订阅。

清理后的精确验证：

```sh
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ThemeConfigProviderTests|FullyQualifiedName~ThemeScopeGraphTests|FullyQualifiedName~ThemeContextLeaseTests|FullyQualifiedName~ThemeContextLeaseLifecycleTests|FullyQualifiedName~ThemeManagerTests|FullyQualifiedName~ThemeManagerTransactionTests'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~CardSemanticPartTests|FullyQualifiedName~WindowThemeContextLeaseTests'
dotnet build src/AtomUI.Core/AtomUI.Core.csproj --configuration Release --framework net8.0
```

- Core **50/50 PASS，0SKIP，用例总527ms**；Desktop **16/16 PASS，0SKIP，用例总907ms**。
- NET8 Release **0 warning / 0 error，最终5.22s**（正常restore后的首次通过12.08s）。第一次带 `--no-restore` 因外部临时项目的 restore 图缺 NET8 target 报 NETSDK1005；按原模块多目标配置正常 restore 后通过，不改包版本。
- Provider 完整成员组织审计：1文件/1控件，0问题；本次顺便将已有事件与协作方法归入规定区域，签名/可见性/事件行为不变。
- 7份相关文档本地链接检查通过，`git diff --check`通过；仅保留文本记录，未提交媒体/临时源码/驱动，没有遗留本次UI进程。
- 作者单独自审完成：检查准备期无挂载、相邻组顺序、根窗口去重、模板frame清理、句柄释放和纯Token边界；本子项无未处置阻断项。遵循用户不使用subagent要求，没有独立评审。

**B的宿主接线和阶段原语完成，CT-08整体仍进行中。** C须接管所有Token事务入口与完整样式输入，特别是外层既有cleanup边界仍仅使用原来的诊断方式，不能将内部聚合异常/待清理状态冒充对外品牌表现结果；须在完整协调时验证统一的失败报告、有界重试和恢复核对，再宣称品牌应用成功。
