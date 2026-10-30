# Descriptions Changelog

本文档记录 Descriptions 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-08-13

- Semantic Part
  - 为 `Descriptions` 建立 `root`、`header`、`title`、`extra`、`label` 和 `content` 公共契约。
  - 为运行时物化的 label/content 发布完整 owner-scoped `SelectorRoute`，通过 scope marker 跨越 Descriptions 根模板和内部 item 模板，同时隔离嵌套 Semantic owner。
  - 将默认尺寸、Padding 和 Background 基线投影到公开 presenter，使合法 Semantic Setter 按 Avalonia 原生样式优先级生效。
  - 通过根模板 `RootFrame` 投影 owner 的标准背景、边框、圆角和 Padding，使 root Semantic Part 能定制包含 Header 与内容区的完整表面。
- Gallery
  - 增加延迟创建的 Descriptions Semantic Parts Preview，以及 Small/默认 Large 两个同数据实例的 root/label 样式对照示例。
- Tests
  - 覆盖 descriptor、四种布局、三档尺寸、item 墠改重建、布局属性覆盖、嵌套 owner 隔离和 Gallery route 门禁。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Descriptions`.
  - Align generated output paths with `controls/descriptions/index-cn.md` and `controls/descriptions/semantic-cn.md`.

## 2026-06-22

- Docs
  - 建立 Descriptions 控件文档目录，补齐 `overview.md`、`implementation.md`、`token.md` 和 `changelog.md`。
  - 记录 Descriptions 的公共契约、响应式列数、span、边框模式、Header/Extra、主题结构、Token 分类和验证策略。
  - 在 Data Display 分类入口中登记 Descriptions 文档。
  - 将 `DescriptionItem` 文档契约更新为非视觉 `AvaloniaObject` 描述对象，明确 item 属性作为 Avalonia binding target、生成视觉仍由 `Descriptions` 管理。
  - 补充 `DescriptionItem` 资源泄露风险和防范要求，包括 item 属性订阅、binding 转接、generated control 映射、`DynamicResource` scoped host 和 WeakReference 生命周期验证。
- API
  - 将 `DescriptionItem` 从普通数据 record 调整为非视觉 `AvaloniaObject`，并暴露 `Label`、`Content`、`IsFilled`、`Span` 对应 Avalonia 属性。
- Implementation
  - `DescriptionItem` 实现 scoped `IResourceHost` / `IThemeVariantHost`，避免非视觉 item 上的 `DynamicResource` 通过 `Application.ResourcesChanged` 保留旧页面。
  - `Descriptions` 增加 item attach/detach 生命周期管理，在 item 属性变化时同步 generated control 或重新布局，并在 remove/reset/Items 替换/detach 时释放订阅和资源宿主。
