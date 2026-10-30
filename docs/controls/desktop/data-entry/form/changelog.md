# Form Changelog

本文档记录 Form 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-09-01

- Theme
  - 对齐上游 FormItem `Extra` 布局：`ExtraPresenter`（`extra`）从 `ContentFrame` 内与控件同行右对齐迁移到 `PART_ContentLayout` 中 help 区域下方（对应上游 `additional { explain, extra }` 顺序），与内容列对齐，不再挤压输入控件水平空间；默认主题补充说明文字色（`ColorTextDescription`）与 `ControlHeightSM` 最小高度，`Extra=null` 时 presenter 隐藏。
- Tests
  - `FormSemanticPartTests` 新增 Extra 几何位置契约测试（位于控件与 help 区域下方、与内容列对齐），模板静态 marker 文档顺序断言更新为 label → content → help → helpItem → extra（对齐上游 DOM 顺序）。

## 2026-08-31

- Semantic Part
  - Add Semantic Part descriptors for `FormItem`: `label`（`TextBlock#PART_Label`，`.semantic-label`，`ContractType` 为 `Avalonia.Controls.TextBlock`）、`content`（`ContentPresenter`，`.semantic-content`）、`extra`（`ExtraPresenter`，`.semantic-extra`）、`help`（`StackPanel#ExtraInfoLayout`，`.semantic-help`）与 `helpItem`（`.semantic-help-item`，`ContractType` 为 `Avalonia.Controls.TextBlock`，`Cardinality` 为 `Multiple` 且 `RuntimeCreated = true`，路由 `/template/ .semantic-help > .semantic-help-item`）。`root` 由生成器隐式提供。
  - `Form` 本身不注册任何 Part（无 `SemanticPart` 声明，也无隐式 `root`）；`FormItemDecorator`、`FormActionsItem`、`SubmitButton`、`ResetButton` 保持不变，避免与 `FormItem` 的 Part 集合产生歧义。
- Breaking API
  - 移除模板部件 `PART_ErrorMsg` 与 `ErrorMessageInlines` 样式属性。校验消息不再用 `Inline` 集合（`Run`/`LineBreak`）拼接，而是逐条消息创建带 `semantic-help-item` marker 的 `TextBlock` 节点，插入在 `Help` 文案之前，前景色跟随 error/warning 消息画刷；校验结果、消息画刷变化时统一重建，Reset 后清空。消息节点不进入逻辑树。
- Gallery
  - 将 Form ShowCase 从 `GalleryStickyTabsHost` 迁移到 `GalleryShowCaseHost`，新增语义部件预览页签：Username 项仅携带 Help 文案，Password 项进入两条 error 消息加 Extra 提示的失败状态（`LabelColInfo="8*"` / `WrapperColInfo="16*"`），与上游 Semantic DOM 示例逐项一致。
  - Examples 列表末尾新增与上游 style-class 示例对应的样式定制 ShowCaseItem（`SourceKey="form-semantic-part"`，文案不出现 “dom”）：两张 `LabelColInfo="4*"` / `WrapperColInfo="20*"` 卡片表单垂直堆叠、每张 `MaxWidth="800"` 水平铺满（等宽保证 label 列宽一致，Username / Email / Submit+reset），第二张为 `Filled` 变体并定制 `root` 描边（`#1677FF`）与 `label` 颜色；root 样式经 `/template/ Border#Frame` 落到 Form 模板根 Border，`label` / `content` 经 `.semantic-label` / `.semantic-content` 后代 selector 定制。
- Docs
  - 新增 [semantic-part.md](semantic-part.md) 作为 Form 家族 Semantic Part 契约的唯一完整来源；overview 增补 §8.5 语义模型与 LLMS 语义表；implementation 更新文件结构、模板 marker 映射表与逐条消息构建说明。

## 2026-08-23

- Architecture
  - Define Form as the bridge that writes Form-owned native errors to `DataValidationErrors` and relays warning/success/validating through `FormStatus`.
  - Align size, variant, feedback and validation propagation with the shared input-control architecture.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Form`.
  - Align generated output paths with `controls/form/index-cn.md` and `controls/form/semantic-cn.md`.

## 2026-06-23

- Docs
  - Add Form control documentation set with architecture overview, implementation notes, Token design and changelog.
  - Document Form layout, FormItem content contract, validation lifecycle, feedback model, submit/reset flow, SizeType forwarding and compatibility invariants.
  - Add Form to the Data Entry control documentation index.
- Token
  - Document FormToken categories for labels, required mark, colon margin, item spacing and vertical label layout.
