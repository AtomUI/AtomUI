# Collapse Changelog

本文档记录 Collapse 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-27

- Implementation
  - 内容动效改为主题中的 `atom:Motion` 声明，使用 `ExpandMotion` / `CollapseMotion` 与公开布局能力。
  - `PART_ContentMotionActor` 替换为 `ContentViewport#PART_ContentViewport`，移除容器持有的动画执行器和完成回调。
  - 保持手风琴空间交换、内容自然排版、视口命中、结构边线与 Semantic Part route；隐藏后恢复底层透明度来源。
- Docs
  - 同步模板绑定、布局租约、请求替代和资源释放边界。

## 2026-09-10

- Design
  - 纳入[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)，定义普通与手风琴模式共用的尺寸/透明度进度、稳定内容裁剪、反转连续性与执行所有权。
  - 保持 selection model、模板分隔线、既有 Token 和 ColorPicker 派生接入边界。
- Implementation
  - 展开进度由动画执行器的私有附加属性持有，基础 actor 通过内部 `IMotionActorLayout` 协作，分离通用布局与内容开合职责。
  - `CollapseItem` 接入 Core 内部 `ContentExpansionAnimator`，以原生 `Animation` 同步驱动内部布局进度和透明度，完整内容按自然尺寸排版并裁剪。
  - 统一取消、反转、首尾帧和生命周期收敛；清理只释放机制自身资源，保留自定义尺寸与变换。
- Docs
  - 同步共享执行链、ColorPicker 直接消费者边界与帧级验证入口。

## 2026-08-17

- Design
  - Publish Semantic Parts `root`、`header`、`icon`、`title`、`body`（对齐 antd Collapse Semantic DOM 契约），通过
    `[SemanticPart]` descriptor、静态模板 marker 与 owner-scoped 生成 Style（`CollapseHeaderStyle` 等）支持语义定制。
  - 建立 items host scope marker 链：`semantic-scope-items`（`PART_ItemsPresenter`）、`semantic-scope-panel`（默认
    ItemsPanel）、`semantic-scope-item`（每个 `CollapseItem` 容器，创建与 prepare 路径幂等添加）；Part 路由从 owner
    出发经一步逻辑树 `>` 直达容器，再以 `/template/` 进入容器模板。
  - 定义 root 表面投影：`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 经主题模板绑定到
    `PART_Frame`；默认主题保持根背景透明，不改变既有外观。
- Behavior
  - 按 antd collapse 样式规则分发容器圆角：首项 header 承接上圆角、末项 header 与 content 承接下圆角，header/body
    背景沿容器圆角绘制而不是盖住圆角边框。
  - 展开/收起动效由 Core 共享的 `ContentExpansionAnimator` 拥有；收起稳定态按统一动效设计隐藏内容 actor，
    `body` Part 的身份、数量与样式契约不受影响。
- Docs
  - 新增 `semantic-part.md`，更新 `overview.md` 与 `implementation.md` 的 Semantic Part 契约、marker 放置与维护不变量。

## 2026-07-13

- Design
  - Define Avalonia selection model as the sole expansion-state owner for normal and accordion modes.
  - Define structural separator ownership so item and content borders no longer depend on selection or motion timing.
- Compatibility
  - Preserve the existing public API, stable template parts, resource keys, Token names and theme values.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Collapse`.
  - Align generated output paths with `controls/collapse/index-cn.md` and `controls/collapse/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Collapse desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Collapse desktop architecture documentation under `docs/controls/desktop/data-display/collapse/overview.md`.
  - Add Collapse implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Collapse control-level changelog.
  - Add Collapse Token documentation covering CollapseToken.
