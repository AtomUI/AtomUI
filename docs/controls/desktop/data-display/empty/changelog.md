# Empty Changelog

本文档记录 Empty 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Breaking Theme Contract
  - `description` Semantic Part 对齐 Ant Design `styles.description`：原文本节点 `TextBlock#Description` 与无 marker 的 `ContentPresenter#DescriptionContent` 合并为一个带 `semantic-description` 的 `ContentPresenter#Description`，`ContractType` 由 `TextBlock` 改为 `ContentPresenter`。`EmptyDescriptionStyle` 现在同时作用于字符串描述和自定义内容。
  - 兼容性：呈现器复用 `TextBlock` / `TextElement` 的文本属性对象，按原写法设置的 `Foreground`、字体、`TextAlignment`、`TextWrapping`、`TextTrimming`、`LineHeight`、`LetterSpacing`、`MaxLines` 继续有效并继承给文字；`TextBlock` 专有且不继承的属性（如 `TextDecorations`）不再生效，不会报错；以 `TextBlock#Description` 为选择器或在代码中按 `TextBlock` 查找该部件的写法需要迁移。迁移见 [6.2.14 迁移指南](../../../../releases/6.2.14-api-changes.zh-CN.md)。
  - 内部属性 `DescriptionText`、`DescriptionContent`、`IsDescriptionTextVisible`、`IsDescriptionContentVisible` 由 `EffectiveDescription` 取代；`null` 描述按空字符串呈现，布局不变。
- Gallery
  - 语义样式示例的 `EmptyDescriptionStyle` 改为 `x:SetterTargetType="ContentPresenter"`，`description` 部件说明更新为“描述区域，承载文字或自定义内容”。
- Tests
  - `EmptyObjectContentTests` 覆盖：描述样式同时作用于文字与自定义内容、按旧写法设置的文本属性继续生效、`null` 描述保留空文本行。
  - 无头渲染 A/B：默认文案、长文本自动换行（Small / Middle / Large）、空字符串与 `null` 六种描述，改动前后逐像素一致，控件边界相同。
- Fix
  - Desktop 模板渲染 `Image`/`ImageTemplate`（对应 Ant Design `Empty.image` 节点）：新增 `ContentPresenter#ImageContent`，高度沿用内置插画的 `EmptyImgHeight*` Token，`Image` 非空时隐藏 `PART_SvgImage`。此前设置 `Image` 会清空插画且不显示任何图片。`image` Semantic Part 仍只标记 `PART_SvgImage`。

## 2026-11-07

- Breaking API（二进制不兼容）
  - `AbstractEmpty.Description` 与 `DescriptionProperty` 由 `string?` 放宽为 `object?`。字符串的显示不变；非字符串对象通过 `ContentPresenter` 展示。把读取到的 `Description` 当作 `string` 使用的源码需要显式转换，引用旧 `string?` 签名的已编译程序集需要重新编译。迁移见 [6.2.14 迁移指南](../../../../releases/6.2.14-api-changes.zh-CN.md)。
- API
  - `AbstractEmpty` 新增 `Image`（`object?`）、`ImageTemplate`（`IDataTemplate?`）、`Title`（`object?`）、`TitleTemplate`（`IDataTemplate?`），均参与 `AffectsMeasure`。Desktop 模板不渲染 `Title`，它面向其他产品包的模板；`Image` 自 2026-11-11 起在 Desktop 渲染。
  - 图片来源互斥检查扩展到 `Image`、`PresetImage`、`ImagePath`、`ImageSource`，异常信息同步列出 `Image`。只有本地值、样式值、模板值或绑定提供的非空值才计入冲突；属性默认值和 `OverrideDefaultValue` 元数据不计入，因此子类的默认预设不会与调用方的显式来源冲突。
  - 图片提交优先级为 `Image`、`ImagePath`、`ImageSource`、`PresetImage`；模板缺少 `PART_SvgImage` 时图片同步直接返回。
- Theme
  - `Empty` 增加内部直接属性 `DescriptionText`、`DescriptionContent`、`IsDescriptionTextVisible`、`IsDescriptionContentVisible`，由 `Description` 与 `IsDescriptionVisible` 在 `OnPropertyChanged` 中派生。
  - `EmptyTheme.axaml`：`semantic-description` 的 `TextBlock` 绑定 `DescriptionText` 与 `IsDescriptionTextVisible`，`description` Semantic Part 仍是 `TextBlock`，契约不变；新增 `ContentPresenter#DescriptionContent` 展示非字符串描述，边距与前景样式与文本节点一致；Footer 的 `ContentPresenter` 命名为 `Footer`，其边距样式限定到 `ContentPresenter#Footer`，避免覆盖描述内容节点。
  - 非字符串描述所在的 `DescriptionContent` 节点没有 Semantic marker，`description` Part 的样式不作用于它。
- Performance
  - 预设 SVG 按（预设，五个画刷颜色）缓存，上限 32 项，满时清空；叠色计算和字符串拼接只在未命中时执行。每个 Empty 的预设构建分配 4.6 KB 降至 0，端到端约 154 KB 降至 149 KB（SVG 解析占主导）。
  - 缓存键取同步时控件持有的颜色，重新挂树会重新同步，因此主题切换后重新挂树的 Empty 总是使用当前颜色，不新增主题订阅。
- Desktop 影响
  - 二进制不兼容：`Description` 类型变化；字符串描述的渲染不变（无头渲染对照中默认、自定义描述、Simple 预设与隐藏描述在浅色与深色下逐像素一致）。
  - `Title`、`TitleTemplate` 在 Desktop 上被接受但不渲染（`Image` 见 2026-11-11）。
  - 已知边界：本地设置的 `PresetImage` 若等于子类默认值，不产生变更通知，与既有 `Image` 的冲突要到下一次挂树才被检出。
- Tests
  - 新增 `EmptyExtensionTests`（共享层）：`Image` 与显式预设冲突、默认预设不与 `Image` 冲突、`Title` 与对象描述渲染、预设 SVG 按颜色缓存、模板缺少 `PART_SvgImage` 时可用。
  - 新增 `EmptyObjectContentTests`（Desktop）：字符串描述走文本节点、默认描述为本地化文本、对象描述走内容节点、Footer 保持自身边距、主题切换后重新挂树的预设图片颜色。
  - `EmptySemanticPartTests` 中 description 节点断言改为绑定 `DescriptionText` 与 `IsDescriptionTextVisible`。

## 2026-11-04

- Fix
  - 在 `AbstractEmpty` 重新挂树时按当前来源与主题资源同步图片，保持同一实例及模板节点。
  - 在资源齐备并生成完整 SVG 后提交图片，资源暂缺时保留已有结果，避免摘挂过程中清空插画。
  - 统一图片来源校验与结果提交，来源全部为空时清空，相同结果不重置 renderer。
- Docs
  - 明确图片派生状态由 `AbstractEmpty` 单一维护，定义模板应用、重新挂树和挂树期间属性变化的同步职责。
  - 定义先生成完整结果再提交、资源暂缺时保留已有结果、来源明确清空时移除图片，以及重复提交幂等的维护契约。
  - 澄清 EmptyToken 的尺寸与间距职责、预设图形的 SharedToken 配色来源，以及摘挂和模板重套用时的 Semantic target 身份。
  - 补充同一实例摘挂、自定义来源、离树期间来源或主题变化的验证要求。

## 2026-08-19

- Gallery
  - Align the semantic style example copy with the Gallery semantic copy convention: the title becomes "Custom Semantic Part styling" and the description describes the owner-scoped semantic part styles. Remove the dom/classNames wording (AtomUI has no DOM concept) and add `ShouldNotContain("semantic dom"/"classNames")` guards to the showcase page test.

## 2026-08-14

- API
  - 为 `AbstractEmpty` 增加 `Footer`、`FooterTemplate` 和 `StrokeDashArray` public StyledProperty，保持
    `TemplatedControl` 基类不变。
- Semantic Part
  - 为 `Empty` 建立 `root`、`image`、`description` 和 `footer` 公共契约，并生成三个 owner-scoped Semantic Style。
  - 在既有 image/description 节点和新增 Footer Presenter 上声明静态 marker，不增加运行时树扫描或动态标记。
- Theme
  - 修复 `IsDescriptionVisible` 未投影到描述节点的问题，并在 `Footer=null` 时隐藏静态 Footer Presenter。
  - 增加 `FooterMargin` Token；通过模板表面 `DashedBorder` 投影 owner 的背景、边框、圆角、Padding 和虚线节奏。
  - 让 `PresetImage`、`ImagePath` 和 `ImageSource` 运行时切换时更新同一 renderer 并清理旧来源。
- Gallery
  - 增加延迟创建的 Empty Semantic Parts Preview 和生成 Style 对照示例。
  - 将 Customize 示例的外部操作按钮迁入 Empty Footer API。
- Tests
  - 覆盖 descriptor、静态 marker、Footer 内容、描述可见性、图片来源切换、root 表面投影、尺寸 Token 和 Gallery 生命周期。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Empty`.
  - Align generated output paths with `controls/empty/index-cn.md` and `controls/empty/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Empty desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Empty desktop architecture documentation under `docs/controls/desktop/data-display/empty/overview.md`.
  - Add Empty implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Empty control-level changelog.
  - Add Empty Token documentation covering EmptyToken.
