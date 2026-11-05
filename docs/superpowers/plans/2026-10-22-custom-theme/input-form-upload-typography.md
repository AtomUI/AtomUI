# CT13 FU3：Form/Upload 排版接续

状态更新：最后五表已于2026-10-30获用户明确限定批准，CT13既定范围已完成；下文原始执行/失败记录保留，CT25/FW1继续独立。见[批准汇总](input-final-visual-review.md)。


> 在现有目录/分支单agent使用executing-plans/TDD，不新建分支/worktree、委派或提交。

**Goal:** 七个公开Form/Upload owner的有效字体族进入实际文字，保持原默认继承、各语义字号与公开子Control身份。

**Architecture:** 先核实真实字体/字号与父继承。字体族使用既有IThemeInheritanceSource只读父桥和ControlTypographyBindings在根ControlTheme声明式选择；没有新Own/公开标志/代码字体绑定。字号与Decorator Dark Extra前景另据实际模型核定，不机械统一Header/Label/FileName。

**Tech Stack:** 当前AXAML、NET10临时真实文字/Skia、Mac managed/AOT、NET8。

**Spec:** [消费合同](../../../engineering/development/theming/consumption.md)、[渲染合同](../../../architecture/systems/theming/control-rendering.md)、[FU2](input-form-upload-global-surfaces-results.md)。

## 顺序与验收

- [x] 父Courier New/23，各owner有效FontFamily=Times New Roman，先真实七文字节点RED；原root与实际字号分布记录，不从root推断字形。
- [x] 声明式可选字体族、未配置真实父继承，Style/Local/清除/换父、对子Control显式字体隔离及生成专用Style命中/撤回。
- [x] 触及C#先成员审计，整理必须保持原成员/初始化语义与API；新增只读接口能力，无状态字段。
- [x] 明暗默认/配置/恢复同历史、AA/1DIP复位、实际glyph/typeface与可读性，资源/模板/GC与Mac managed/AOT。
- [x] NET8/成员门禁/正式文档；临时源、副本、工程、媒体清理后精确Form/Upload/直接消费者。具体视觉另审，不能代替完整CT13。

2026-10-30已完成前证与审计清理，见[真实审计](input-form-upload-typography-audit.md)。字体实现、清理与最终模块验收已完成，见[结果](input-form-upload-typography-results.md)；限定视觉并入最后五表审查，不代表自动批准。
