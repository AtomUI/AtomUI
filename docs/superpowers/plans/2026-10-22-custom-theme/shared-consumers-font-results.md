# CT-10 S2 跨根字体结果

本项修改仅包含两个 host ControlTheme 的四条 compiled binding，不新增字体状态机、代码 relay 或 Own identity。实际 logical ancestor 为 Popup，PlacementTarget 是内容继承链之外的字体 owner；原主题直接给 host 设置 Shared 14/Normal/Normal，阻断 owner 字体。

## 失败、修复与真实消费

固定640×360、scale1的 AtomUI Window，Popup 为 Canvas 中 anchor Button 的兄弟节点，Child 是带白色背景和10 DIP padding 的原生 Border/TextBlock。两种路径分别断言实际 PopupRoot 与 OverlayPopupHost；没有拿普通 Border 模拟跨根。native 是 Headless 实际 PopupRoot，不冒充 OS 最终显示面。

旧实现两条有效失败：owner 改20/monospace/Bold/Italic，已打开内容仍14/旧Family/Normal/Normal。新实现读取实际 PlacementTarget 的完整有效字体；A→B→A、显式内容 FontSize11、清除后恢复、替换22/Bold/Italic/monospace anchor、旧 anchor 后续变化隔离、同内容重开全部通过。两类 host 中打开时 retarget 后废弃 anchor 可收集，释放整个独立临时 Popup 图后 host/anchor 可收集。

公开内容样式继续按 Avalonia 原生优先级覆盖继承；前景色仍消费有效主题 ColorText，避免把 primary Button 的白色前景继承到白色弹层。

## 严格像素对照与独立问题

同 checkout 单变量 A/B 只恢复/重施两个 owned AXAML 文件，finally 逐字节核对恢复成功；没有另开分支或复制 checkout。新旧实现逐阶段比较，不对图像裁剪、对齐、遮罩或设容差。

| 比较 | PopupRoot | OverlayPopupHost |
| --- | ---: | ---: |
| 旧默认 A/A | 0 | 0 |
| 旧默认→新默认 | 0 | 0 |
| 1 DIP 正对照 | 731 | 736 |
| 新默认→owner 字体变化 | 1640 | 2263 |
| 旧实现恢复阶段→新实现恢复阶段 | 0 | 0 |
| 单实现初始默认→恢复默认 | 0 | 4（新旧完全相同） |

Overlay 的4个差异像素位于 owner Button 下方边缘，bbox `[40,85,139,87]`，每个RGB通道仅1级。原实现亦产生相同差异，新旧恢复阶段逐像素完全相同。各阶段重复采样 A/A 稳定。**没有把4像素视为可接受容差或宣布按钮已修复**：作为 CT-12 按钮专项的有效前证保留，CT-10 S2 的兼容结论仅是本次改动在默认和恢复阶段均未引入差异。全部关键帧和完整差分人工复核。

保留 Child 的关闭图 GC 反例也做了同源码 A/B：新旧都有 Popup 继承子项→Child→closed host 路径，native host 的 `_popupPositionRequest.Target` 保留 anchor。该现有原生缓存边界归 CT-25 后续审计；本次没有私有字段修补、清空用户 Child 或降低关闭 UX。

## 清理与最终验证

临时外部驱动、bin/obj、截图/差分按 scripted-visual-acceptance 清理后，再执行 Popup/主题继承精确测试及 Desktop NET8 构建；实际数值与作者自审见本节后续记录。S2 不代表 CT-10 S3–S5 或所有家族已完成。

最终清理：两轮 managed evidence 分别删除20/18个文件，audit均clean；外部驱动目录及其bin/obj已删除，无净新增常驻测试。清理后 Desktop 精确 `PopupLifecycleTests|ThemePopupInheritanceTests|PopupShadowTests|PopupPinnedOpenContractTests` **35/35通过，2s**；Desktop NET8 Release **0警告/0错误，33.76s**。作者自审检查源码、绑定优先级、retarget/生命周期及全部像素结果，未发现本项未解决的Critical/Important；不是独立评审。按钮原有4px与持Child原生缓存仍分别由CT-12/CT-25跟踪。未执行全量回归，未提交。
