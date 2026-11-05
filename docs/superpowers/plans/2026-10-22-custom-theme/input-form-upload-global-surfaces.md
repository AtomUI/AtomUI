# CT13 FU2：Form/Upload Global 表面消费

> 沿当前目录/分支单agent执行executing-plans/TDD；无分支/worktree、委派或提交。

**Goal:** 已有Global到达Form、FormItem、Decorator、Upload、DropArea、Trigger各自公开identity的真实目标，内部上传资产使用Upload组合语义。

**Architecture:** 只修正真实反例证实的Shared来源；公开Control使用自身typed资源，内部资产或明确组合属性使用既有OwnerTokenResource与SourceControlType。公共子Control保持自身基础identity，共享Own不改，无实例Token字典/新scope/缓存/全局订阅。

**Tech Stack:** 当前AXAML、NET10临时场景、Skia正常动画、Mac managed/AOT、NET8。

**Spec:** [消费规则](../../../engineering/development/theming/consumption.md)、[渲染合同](../../../architecture/systems/theming/control-rendering.md)、[FU1结果](input-form-upload-defaults-results.md)。

## 顺序与验收

- [x] root错误/警告画刷、FormItem间距/高度/图标、Decorator间距、Upload布局、DropArea/Trigger表面实际RED；各自identity隔离，真实目标先于改引用。
- [x] 保持原selector、状态、模板、Own/资源ID；公开实例画刷、专用Style及撤回，透明0/清除、重模板/重挂/GC验证。
- [x] 原同历史明暗/多模式默认、状态、配置/恢复，AA/1DIP复位和直接必要A/B；自然失败不掩盖或放宽。
- [x] 当前Mac managed/AOT与NET8、代码/文档自审、临时源/副本/原生工程/媒体清理；精确Form/Upload验收。字体与完整Part/状态家族出口随后，不能以本表面包关闭CT13。

Engineering/cleanup evidence: [FU2 results](input-form-upload-global-surfaces-results.md). 用户2026-10-30明确批准FU2本表限定视觉并保留CT25；完整CT13仍开放。
