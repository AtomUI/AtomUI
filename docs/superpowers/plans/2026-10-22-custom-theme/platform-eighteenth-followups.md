# 第十八批：Breadcrumb 默认函数与第十七轮平台结果

当前基线为 ff0d3d572，工作位置仍为现有 feature/custom-theme／checkout。本批 13 文件尚未提交，拟一个提交、推送既有远端分支，用 AtomUI 凭证运行三 OS／两 Browser 验证 Actions；不新建分支或 worktree，不发布 NuGet。此前 27 文件的授权已执行，不覆盖本批。

## 审查内容

- [第十七轮实际结果](platform-seventeenth-results.md)：三 OS 各 6,405 项选测、各 26 项 NativeAOT、两 Browser 各 23 项运行检查，8 job 均成功且源码为 ff0d3d572；370 主题资产及每 OS 27 对本地化子进程阶段完整。仅记录实际结果，没有修改 workflow、超时、断言、发布设置或重试策略。
- [Breadcrumb 本机切片](container-breadcrumb-defaults-results.md)：仅八项 Global-only 默认接入 V2，没有 Own→Own 边或行为变化；288 条字段记录和明暗 16 默认的值／类型／identity／key／slot 保持。补正正式文档中 Shared 图标尺寸与 Own、Uniformly 与 Thickness 字段的边界。
- 原版有效 RED 为 1 项 V2 合同失败、2 项原消费兼容通过；候选 3 项通过。扩充后的候选 7 项、原版 6 项同历史兼容通过；准备错误及宿主校正独立记录，不冲抵失败或计入正式帧。
- 正式实际明暗 Window 共 84 帧，28 A/A／12 恢复／42 新旧均 0，28 组 1 DIP 全检出，无需新增有限差异许可。Mac managed 136／NativeAOT 137 检查通过，各 80 条资源记录一致；AOT 链接到面向 macOS26 的本地 dylib，不声称验证 macOS12。
- 临时 C# 已删除，常驻测试净增 0；169 个媒体文件及 419 文件原生工程已清理，进程结束。清理后原 Breadcrumb 精确 18 项、0 Skip 通过，Desktop NET8 零警告／错误，1 Token 文件成员审计无问题；八条 Token／一条 Control CSV 回填。

## 精确 13 文件

- `src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbToken.cs`
- `docs/controls/desktop/navigation/breadcrumb/overview.md`
- `docs/controls/desktop/navigation/breadcrumb/implementation.md`
- `docs/controls/desktop/navigation/breadcrumb/token.md`
- `docs/controls/desktop/navigation/breadcrumb/changelog.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-breadcrumb-defaults.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-breadcrumb-defaults-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/container-consumers.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/control-inventory.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/token-contract-review.csv`
- `docs/superpowers/plans/2026-10-22-custom-theme/tasks.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-seventeenth-results.md`
- `docs/superpowers/plans/2026-10-22-custom-theme/platform-eighteenth-followups.md`

作者另步自审核对真实旧新输入、八项表达式、默认身份、清理与证据；不声称独立评审。原生最终像素、图标／键盘等剩余业务状态、N2／N4、其他 Host 及完整 CT11–CT19／CT25 仍开放。NavMenu 初始折叠首次展开产品方案仍待独立批准，排除本批；所有临时源、媒体、外部工程与构建产物不进入提交。
