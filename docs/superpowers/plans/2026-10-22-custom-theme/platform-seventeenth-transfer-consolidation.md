# TreeView、Transfer 与 NavMenu 测试采样整合审查

本次将尚未提交的[第十七批](platform-seventeenth-followups.md)与后续 Transfer 切片汇总，便于一次审查。当前基线为 `204c44149fdb0af5c7678b3f580a9e9948c56310`，研发仍在既有 `feature/custom-theme` 和当前 checkout。拟创建一个提交、推送既有远端分支，并用此前指定的 AtomUI 凭证运行三 OS／两 Browser 验证 Actions；不创建分支或 worktree，不发布 NuGet。

这是一份新的整合快照。此前第十七批的 14 文件请求仅覆盖其已保存补丁，不自动覆盖本表的 Transfer 或后续目录变化；整合快照也尚未获得 Git 授权。

## 变更与证据

- TreeView：12 个 Global-only 默认迁移 V2，两个目录选中色仅兼容保留；正式四文档、目录同步。明暗默认／恢复／新旧像素为零，Mac managed／NativeAOT 各 304 项资源检查通过，临时清理后 TreeView 57 项、NET8 零警告。详见[结果](container-tree-defaults-results.md)。
- NavMenu：仅修改既有动效测试，在触发前订阅 Bounds，保留严格中间值、原超时与终点断言；两个延迟观察反例和两个直接跳变负对照验证有效。产品源码、模板和动效未改；清理后 NavMenu／TreeView 精确范围 365 项通过。最初折叠后的展开产品修复仍待独立设计批准，排除本批。
- Transfer：三个 scope 共 11 个默认迁移 V2，保留字段、默认公式、identity、类型、key 与 slot。Mac managed／NativeAOT 各 684 项；原生脚本中的树分页阶段只算内部模板诊断。另以纯公开 API 补充往返，并核定 ListWidthLG 为共享公开属性标识可达的既有条件消费者；不新增树分页能力。详见[默认结果](container-transfer-defaults-results.md)与[公开补充](container-transfer-public-results.md)。
- Transfer 文档：纠正不存在的 Token／控件名称及 Semantic owner，修正专用 Style、SetterTargetType。原两项反例失败、修正后四项通过，原文编译的八个 Style／九个关键 Setter 实际命中；临时源已删除。共享属性路径在旧版与候选各明暗两项通过，16 阶段宽度一致。
- 视觉范围：Transfer 原 34 对及公开补充 14 对自然合成差异分别获得精确限定批准，原始失败和计数保留，无通用容差。两批媒体分别清理 204／141 文件，Git 媒体审计 clean。批准不扩大到其他 Host、原生最终像素或完整家族。
- 最终清理：新增常驻测试为零，临时驱动／工程／媒体已删除。最新 Transfer 精确相关范围 44 项、0 Skip、测试 3 秒通过；NET8 和三 Token 成员审计的源码身份保持，32 个 Transfer 文档链接通过。作者另步自审，不声称独立评审。

## 27 文件范围

- TreeView：`TreeViewToken.cs`，正式 overview／implementation／token／changelog 四文档，计划与结果两文档，共 7 文件。
- Transfer：`ListTransferToken.cs`、`TreeTransferToken.cs`、`TransferListViewToken.cs`，正式 overview／implementation／token／semantic-part／changelog 五文档，默认计划／默认结果／34 对限定表／公开补充四文档，共 12 文件。
- 既有 `NavMenuInlineCollapsedTests.cs`，共 1 文件。
- 容器执行计划、control-inventory.csv、token-contract-review.csv、tasks.md、第十六轮结果、第十七批原审查及本文，共 7 文件。

精确路径、文件 SHA 与可校验补丁保存在本地审查收据；批准前 index 保持空，不提交／推送／调度。完整 CT11–CT19／CT25、其他平台和原生最终显示面继续开放；本批提交或 CI 通过均不能替代家族验收。
