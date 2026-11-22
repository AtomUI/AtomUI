# Expander Changelog

本文档记录 Expander 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-11-20

- Token
  - `ExpanderToken.HeaderColor`（默认 `ColorTextHeading`）：头部文字颜色。
  - `ExpanderToken.HeaderFontWeight`（默认 `Normal`）：头部字重，作用于头部内容呈现器；PrimeNG Aura 600，Lara / Nora 700。

## 2026-11-11

- Behavior
  - Header 成为键盘停靠点（Ant Design Collapse `collapsible`）：`Header` 模式下 Expander 自身可聚焦，Enter / Space 切换展开，展开按钮不再是额外的 Tab 停靠点；`Icon` 模式下只有展开按钮是停靠点。
- Theme
  - `PART_HeaderDecorator` 标记 `TemplatedControl.IsTemplateFocusTarget`，焦点描边只包围 Header。
- Implementation
  - Expander 实现内部的 `IFocusOutlineShapeProvider`，按展开方向与展开状态给出 Header 贴住的外框圆角，避免方角描边戳出圆角外框。
- Tests
  - `FocusOutlineTargetTests` 覆盖两种触发模式的停靠点、Enter / Space 切换与描边圆角。

## 2026-11-08

- Changed
  - 主题改读 `ExpanderTokenResource`，`Controls[Expander]` 覆盖生效；`ContentBg` 改为内容区背景（antd `contentBg`），Ghost 模式头部与内容区透明。
  - 删除从未被消费的 `LeftExpandButtonVMargin`、`RightExpandButtonVMargin`（左右展开时头部旋转，水平外边距 Token 已覆盖）。
  - 可见变化：内容区绘制 `ColorBgContainer`（此前透明），Ghost 头部改为透明（此前 `ContentBg`）。

## 2026-10-27

- Implementation
  - 内容动效改为主题中的 `atom:Motion` 声明，使用 `ExpandMotion` / `CollapseMotion` 与公开布局能力。
  - `PART_ContentMotionActor` 替换为 `ContentViewport#PART_ContentViewport`，移除控件持有的动画执行器和完成回调。
  - 保持四方向自然排版、真实视口命中、反向连续性、分隔线及 Semantic Part route；隐藏后恢复底层透明度来源。
- Docs
  - 同步请求、布局租约、模板绑定和生命周期所有权。

## 2026-09-27

- Theme
  - 由 Expander 主题字典直接持有正文 `StringToTextBlockConverter`，消除对其他主题同名资源的隐式依赖。
  - 保持字符串正文的垂直居中、水平拉伸与自动换行，以及非字符串内容原样传递；不改变模板布局、交互或 Semantic Part route。
- Tests
  - 覆盖编译主题的局部资源所有权、字符串及内容更新、非字符串内容和同名宿主 converter 隔离。

## 2026-09-10

- Design
  - 纳入[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)，定义四方向的尺寸轴、内容锚定、稳定排版、请求反转和生命周期归一契约。
  - 保留 `IsExpanded`、方向 selector、标题/内容分隔线和有效主题时长的现有所有权，不增加手风琴状态或公开动效配置。
- Implementation
  - 展开进度由动画执行器的私有附加属性持有，基础 actor 通过内部 `IMotionActorLayout` 协作，分离通用布局与内容开合职责。
  - 接入 Core 内部 `ContentExpansionAnimator`，通过原生 `Animation` 同步驱动内部布局进度与透明度，四方向保持完整内容排版及标题侧锚定。
  - 关闭动效、方向切换、模板替换及 detach / reattach 统一收敛到当前展开状态，只释放机制自身资源，保留自定义尺寸与变换。
- Docs
  - 同步四方向执行链、生命周期和帧级验证要求。

## 2026-07-13

- Theme
  - Move the internal Header/Content separator from `PART_HeaderDecorator` to an unnamed Content border inside `PART_ContentMotionActor`, preserving public API, stable template parts, Token contracts, and Borderless/Ghost behavior across all four expand directions.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Expander`.
  - Align generated output paths with `controls/expander/index-cn.md` and `controls/expander/semantic-cn.md`.

## 2026-06-22

- Docs
  - 建立 Expander 控件文档目录，补齐 `overview.md`、`implementation.md`、`token.md` 和 `changelog.md`。
  - 记录 Expander 的公共契约、触发区域、展开方向、动效状态机、自定义 padding、主题结构、Token 分类和验证策略。
  - 在 Data Display 分类入口中登记 Expander 文档。
