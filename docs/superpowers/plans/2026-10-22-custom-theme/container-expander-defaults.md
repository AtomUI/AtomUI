# CT14 N3-E：Expander 默认函数迁移执行清单

> 主agent按既有CT14 N3／Own V2设计，使用superpowers:executing-plans连续执行；不使用subagent，不将本片细化当作新增交互设计。

**Goal:** 迁移Expander的13项Global-only默认，保持全部值、资源身份、显式覆盖、模板与现有行为。

**Architecture:** ExpanderToken接入DependencyDefaultsV2／Revision1／IControlTokenDefaults，生成静态默认入口。没有Own→Own边；不修改Expander.cs或AXAML。

**Tech Stack:** C#、Avalonia、NET10 Headless/Skia与当前Mac原生managed／NativeAOT、NET8构建。

**Spec:** [容器N3执行计划](container-consumers.md)、[Own默认求值](own-defaults.md)。L0机制迁移沿用已有执行授权；发现默认／交互／画面修复时另交设计审查。

## 固定边界

- 现有feature/custom-theme和checkout；来源0b5e70c71，不创建分支／worktree，不发布NuGet。Git提交另审。
- 13字段顺序、名称、类型、identity、资源key／枚举数值／dense slot保持。当前Global／Control Global／Own优先级及明确0保持。
- HeaderPadding=Thickness(UniformlyPadding,UniformlyPaddingSM)，SM=Thickness(UniformlyPaddingSM,UniformlyPaddingXS)，LG=Thickness(UniformlyPaddingLG,UniformlyPadding)；HeaderBg=ColorFillAlter。
- ContentPadding=Thickness(16,UniformlyPadding)，SM=PaddingSM、LG=PaddingLG；ContentBg=ColorBgContainer。常量16不改为其他Global，Ghost头部消费者不偷换为body背景。
- ExpanderBorderRadius=BorderRadiusLG；H Start／End外边距分别是右／左UniformlyMarginSM；两个V字段分别是下／上UniformlyMarginSM。当前旋转Header仍消费H字段，V字段没有现行消费者，不新增垂直图标布局。
- 保留四方向、Start／End、四尺寸、Header／Content实例padding、Ghost／Borderless、disabled、折叠body可选物化、裁剪和连续动效。专用Part定制仍走生成Style，不回写部件属性。

## 审查重点

1. 全局padding极值不会改变ContentPadding水平常量16。
2. 实例HeaderPadding／ContentPadding明确0继续优先，清除回到当前主题。
3. 有效Control Global与明确Own分别优先，删除配置后恢复；两V字段只证明配置往返。
4. 左／右方向使用原有旋转与裁剪，不能接V字段来改变视觉。
5. collapsed body缺席及Ghost／Borderless背景保持；同实例A→B→A不靠重建恢复。

## 执行步骤

- [x] 原ExpanderControl精确filter40项通过；13项声明／公式与真实消费者只读核对。
- [x] 在Temporary/CT14ExpanderDefaults写V2合同RED及明暗原消费兼容探针；记录真实Compiler的26默认、Global／Control Global／Own／0／撤回及类型／key／slot。
- [x] 只改ExpanderToken.cs，13默认静态化；同合同GREEN，逐条新旧记录相同。V字段保留资源及配置往返，不标Active交付。
- [x] 实际明暗产品Window同历史自然A/A、1DIP、新旧／恢复对照；当前Mac managed／NativeAOT分别核对资源与实例保持，结论不推及其他Host或原生最终像素。
- [x] 删除临时测试／外部工程／媒体，再运行原ExpanderControl精确filter、Desktop NET8、完整Token文件成员审计；同步正式文档／CSV／总表并作者另步自审，Git另审。

新增测试默认临时；缺少独立长期价值不进入全局集。准备失败与有效RED分开记录，任何自然差异保留真实计数并按视觉规范处置，未审差异不标本机切片完成。完整Expander、其余Global／字体／输入、N2／N4及CT14／CT25继续开放。

本机切片验证与清理完成，具体数值、准备错误、自然帧和原生边界见[结果](container-expander-defaults-results.md)；不代表完整家族或新Git授权。
