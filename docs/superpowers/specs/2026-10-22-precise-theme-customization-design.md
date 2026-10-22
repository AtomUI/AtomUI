# 精确主题定制设计过程记录

日期：2026-10-22。本文保留选型及收敛记录，不再拥有正式设计合同。

正式设计已按模块归属更新；实现状态单独维护。本文不作为产品实施指令，也不表示新机制已完成运行验证。

## 正式设计位置

| 专题 | 唯一正式来源 |
| --- | --- |
| 现行配置、Token运行与提交 | [主题运行时](../../architecture/systems/theming/runtime.md) |
| 有效Own、默认依赖、别名与版本 | [Own求值](../../architecture/systems/theming/own-token-evaluation.md) |
| 完整品牌、样式资格、跨根与激活 | [品牌配方](../../architecture/systems/theming/brand-recipes.md) |
| 视觉输入、优先级、自绘和更新 | [Control渲染](../../architecture/systems/theming/control-rendering.md) |
| 默认声明、跨程序集元数据与生成 | [Generator默认值生成](../../modules/generator/own-token-defaults.md) |
| 工具状态、完整往返与受限生成 | [Gallery工作台](../../gallery/theming/workbench.md) |
| 研发与验收要求 | [全局主题规范](../../engineering/development/theming/overview.md) |
| 源码差异及机制/平台证据边界 | [主题验证边界](../../architecture/systems/theming/verification.md) |

## 选型与收敛

- 保留现有ThemeManager/Compiler/Snapshot、生成注册、惰性Control槽和Avalonia样式体系，补齐其合同与消费者；没有引入平行主题引擎的依据。
- 字面量替换不能解决owner、提前派生、悬空Own、优先级、自绘与更新问题；机械Token化会混淆默认、几何、内容及品牌语义。
- Own选择显式依赖和生成式强类型计算，保留旧整体evaluator的独立版本；不从任意C#方法体自动推导。
- 兼容别名按每个配置层先归一，再走正常覆盖；同层不同显式值冲突，跨层按优先级生效。
- 品牌需要Token、组件覆盖和语义样式的完整表达；候选规则资格及跨根装配仍须原型验证，不能以合并资源代替样式隔离。
- 早期“先提交Token、激活失败后恢复旧Snapshot”的候选已被替代。正式品牌设计把可恢复接线/预激活放在Token CommitCore之前，提交后的Publish继续不可回滚，并区分Token结果与品牌表现完整性。
- 不复制Ant首页通过重建子树清旧值的做法；已有实例、状态、自绘和弹层更新由实际消费者证明。

## 工程与证据入口

[整体方案](2026-10-22-custom-theme/overview.md)拥有目标、问题映射、工作包与进度；[审计](../audits/2026-10-22-theme-customization/overview.md)拥有参考源码和静态证据。正式文档不依赖这些过程资料才能成立。

工作位于用户授权的feature/custom-theme分支。当前完成设计资料更新，产品源码、逐Control运行验收和真实平台/AOT证据尚未完成。
