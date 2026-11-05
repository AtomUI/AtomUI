# CT13 FU1：Form/Upload 默认依赖与兼容字段

> 已授权原目录/分支内单agent继续，使用executing-plans/TDD；不提交、新建分支/worktree或委派。

**Goal:** Form8与Upload13既有Own迁移DependencyDefaultsV2，保留公式、类型、资源key/ID/slot，并核定无内置消费的VerticalLabelPadding。

**Architecture:** 两个既有Token类改sealed partial并实现IControlTokenDefaults，各字段单个静态默认方法，Revision1。公式只读取有效Global或原常量，不虚构Own边。VerticalLabelPadding保留稳定资源与配置能力，按Legacy/CompatibilityRetained核对，不新增布局效果。

**Tech Stack:** 当前生成器/V2 evaluator、NET10临时专项、实际Skia、Mac managed/AOT、NET8。

**Spec:** [输入目录](inputs.md)、[Own求值](../../../architecture/systems/theming/own-token-evaluation.md)、[已核源码](input-autocomplete-mentions-remaining-audit.md)。

## Task 1：有效前证及完全等价迁移

**Files:** Form/FormToken.cs、Upload/UploadToken.cs；临时CustomThemeFormUploadDefaults。

- [x] 真实注册descriptor要求V2的2项有效RED；原明暗、控件Global覆盖、显式Own及清除的21字段值/type/key-ID/slot完整快照。
- [x] 保持原精确公式与顺序，包括ControlHeightLG×2.55、MarginXXS/2、FontSize+PaddingXS、原progress2和VerticalLabelMargin=default；无四舍五入或范围扩缩。
- [x] 21默认方法、V2Revision1，删除空构造与旧CalculateTokenValues；原快照逐一相等，算法/Own更新与清除保持。

## Task 2：消费与兼容

- [x] 表单不同布局、必填/标签/colon、help及上传四listType/默认拖拽/进度/预览尺寸的真实资产观测，发现既有断链另记消费包，不以V2代替全部消费者。
- [x] VerticalLabelPadding无内置消费者的源码/文档/实际资源读取与配置保持，分类Legacy且不接入默认布局；正式token文档不宣称可调内置padding。
- [x] 实际同历史明暗AA/1DIP复位、旧新默认/配置/恢复，正常动画与产品数据保持；变化若有先诊断，不能放宽容差。

## Task 3：出口

- [x] 当前Mac managed/AOT实跑、NET8、Token成员门禁和默认生成器精确filter；正式implementation/token/changelog同步。
- [x] 作者另步自审、临时源/副本/原生工程/媒体全部清理后精确Form/Upload及直接消费者验证；不跑全量，CT12六临时文件保留。
- [x] 回填各自结果与inventory；其他Form/Upload Global/字体/Part/状态和整个CT13继续，不能把本默认包当完整家族。

2026-10-30本默认包工程/清理完成；见[input-form-upload-defaults-results.md](input-form-upload-defaults-results.md)，完整家族仍开放。
