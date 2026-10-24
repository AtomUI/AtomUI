# CT-25 Tour 同目标步骤切换审查

## 复现与修复

沿用既有 Same_Target_StepChange_Preserves_Flipped_Arrow_Direction：800×600 窗口，目标位置 (320,530)、大小72×32，同一目标连续两步，相同 RequestedPlacement，保留正常 motion。Popup 内容216×142，实际 VerticalFlipped=true；同几何切换不会再发 PositionFlipped，而旧代码把箭头重置为 Top。

候选在目标和请求 placement 都未变化时，沿用 Popup 的实际横纵翻转状态配置箭头。Pinned 单向转发同时等价移入 ControlTheme TemplateBinding；私有翻转回传仍由有明确生命周期的代码绑定承担，重复模板应用前释放旧绑定。Placement 与 Semantic Part 两个既有类21项通过；完整 Tour 家族验收仍开放。

## 实际像素对比

以第三轮提交847c441c1为旧版，候选为新版；每版每主题独立创建 main、A/A、1 DIP 正对照三个实例，依次捕获第一步与同目标第二步。实际 Skia 渲染，主题由 ThemeManager 应用并核对实际明暗，650ms正常动效等待，完整800×600 RGBA比较，无遮罩、容差或自动对齐。

8组独立A/A全部0差异；8组1 DIP平移全部检出（1,648至5,515像素）。第一步明暗整图均0差异。第二步明色17,780像素变化，边界(225,384)至(487,572)；暗色9,287像素，边界(243,398)至(469,560)。已目视核对旧／新明色、候选暗色和明色差异图：箭头从错误上缘恢复到下缘，卡片正文与阴影随箭头布局上移6 DIP，目标位置保持。不能把差异只描述为三角形变化。

原始媒体已在用户批准后清理，只保留本页数值及实际目视结论。数值与只读几何记录在 `.superpowers/sdd/platform-validation/tour-visual-{evidence,comparison}.json` 及对应几何目录；清理审计无仓库媒体残留。此处是 Headless/Skia 像素证据，不替代原生最终窗口显示像素验收。

## 当前出口

用户已明确回复“允许 Tour 默认翻转修复”，批准上述默认视觉差异；此批准不包含Git提交。临时视觉驱动已删除，原测试文件恢复；正式实现／变更文档已同步。清理后最终10个精确Desktop类在Mac与本地Linux arm64各198项全部通过，包含Tour两类21项；NET8 Desktop Release零警告、零错误。独立只读审查未发现本批新增Critical／Important问题。无新增常驻测试，完整Tour及原生最终像素保持开放。
