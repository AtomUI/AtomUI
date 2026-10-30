# Card Changelog

本文档记录 Card 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-08-13

- Semantic Part
  - 为 `Card` 增加 `root`、`header`、`title`、`extra`、`cover`、`body`、`actions` 公共契约。
  - 为 `CardMetaContent` 增加独立的 `root`、`section`、`avatar`、`title`、`description` 公共契约。
  - 在既有静态模板节点上声明 marker，不新增 Visual、Binding、订阅、运行时树扫描或布局热路径逻辑。
  - 明确 `CardGridContent`、`CardGridItem`、`CardTabsContent` 和 `CardActionButton` 不自动成为独立 Semantic Part owner。
  - 将 Borderless 零边框下沉为 Theme 默认值，使 owner-scoped root style 可以按 Avalonia 原生优先级恢复外框。
- Theme
  - 让根 `Frame` 直接承载 Card 内容，保证 Header 和 Actions 分隔线从外框内缘开始，不覆盖自定义外框交点。
- Gallery
  - 在延迟创建的 Semantic Parts Tab 中分别展示 Card 和 CardMetaContent 的完整 Part 结构。
  - 增加 `v6.1.3` Semantic Part 样例，演示固定 owner 样式和基于 `StyleVariant` 的条件样式。
  - 对齐样例的共享 Header/Body 样式、外框、Function extra 和三枚 action icon 颜色。
- Tests
  - 覆盖 descriptor、静态 marker、Headerless、Loading、Meta/Grid/Tabs、动态 Actions、同名 title owner 隔离和 Gallery 快照。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Card`.
  - Align generated output paths with `controls/card/index-cn.md` and `controls/card/semantic-cn.md`.

## 2026-06-22

- Docs
  - 建立 Card 控件文档目录，补齐 `overview.md`、`implementation.md`、`token.md` 和 `changelog.md`。
  - 记录 Card 的公共契约、内容类型模型、Actions、Grid 内容、Tabs 内容、主题结构、Token 分类和验证策略。
  - 在 Data Display 分类入口中登记 Card 文档。
