# GroupBox Changelog

本文档记录 GroupBox 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-09-16

- Feature
  - 新增 Semantic Part 契约：公开 `root`、`header`、`icon`、`title`、`content` 五个语义区域，声明位于 `GroupBox.SemanticParts.cs`，完整契约见 `semantic-part.md`。
  - 四个非 root Part 均为 `Single`、`Selector` 定制、`RuntimeCreated=false`、`CrossVisualRoot=false`，在 `GroupBoxTheme.axaml` 单一模板内以静态 `Classes.semantic-*` marker 标注。
  - 新增 `icon` 与 `title` 定制能力：Header 图标的尺寸、颜色、间距此前只能通过替换整个 `ControlTheme` 或改 Token 调整，现可通过生成的 `GroupBoxIconStyle` 定制；标题的 `TextDecorations`、`Margin` 等 owner 未暴露的属性同样开放。
  - 生成 `GroupBoxHeaderStyle` / `GroupBoxIconStyle` / `GroupBoxTitleStyle` / `GroupBoxContentStyle` 四个 Semantic Style 类型。
- Theme
  - `PART_HeaderContent` 由 `Decorator` 提升为 `Border`，使 `header` Part 具备 `Background` 能力（Avalonia `Decorator` 只有 `Child` 与 `Padding`，没有 `Background`）。`Border` 是 `Decorator` 子类，`OnApplyTemplate` 的 `Find<Decorator>` 查找路径与基于 `Bounds` 的缺口几何语义不变。
  - 主题中以节点类型开头的 selector 由 `Decorator#PART_HeaderContent` 同步为 `Border#PART_HeaderContent`，否则 `HeaderContentPadding` 与标题对齐 Setter 会因类型不匹配静默失效。
- Docs
  - 新增 `semantic-part.md`，记录准入依据、逐 Part 存在条件与可定制属性、Selector 用法、状态数量矩阵、尺寸基线、定制边界与验证清单。
  - `overview.md` 新增 §3.5 Semantic Part 契约摘要表；修正 LLMS 语义区域表中生成器回退路径产出的 `item` / `motion` 占位行（GroupBox 没有 item 集合或动效区域）。
  - `implementation.md` 新增 Semantic Part marker 所有权、节点映射与自绘几何边界说明。
- Tests
  - 新增 `GroupBoxSemanticPartTests`，覆盖 descriptor 四部件字段、模板静态 marker、生成 Style 精确命中、Part Setter 对 Token 与属性投影的优先级、`PART_HeaderContent` 类型提升后的 Token 内边距与三档对齐、缺口几何排除与 Header 背景无关、图标隐藏不保留占位宽度、状态切换与模板重应用下的 marker 稳定性，以及 root 边框/背景定制经 owner 作用域 Style 覆盖后真正进入自绘几何。
  - Gallery 页面迁移为 `GalleryShowCaseHost` 并新增 Semantic Parts Tab（延迟创建）与 Semantic Part 样式示例，补齐四个语言的本地化资源。
  - Semantic 样式示例新增 `semantic-border` 与 `semantic-border-plain` 两个 GroupBox，演示 `root` 区域的边框定制入口（`BorderBrush` / `BorderThickness` / `CornerRadius` / `Background`）；这一入口使用 owner 作用域普通 Setter，而不是 Part Style，因为分组边框与背景由控件自绘、模板中没有承载节点。
  - 修复边框演示说明文字横向溢出卡片：Avalonia `TextWrapping` 默认为 `NoWrap`，该说明漏写 `TextWrapping="Wrap"`；已补齐并由用例锁定。

## 2026-07-04

- Fix
  - 恢复基于 `PART_Frame` 的模板根测量，使未设置显式高度时 GroupBox 自动高度包含 Header 通道、内容内边距和内容期望高度，避免内容过多时被挤压。
- Tests
  - 新增 GroupBox 自动高度回归测试，覆盖内容高度大于默认示例高度时的内容 Presenter 分配。
- Docs
  - 记录自动高度测量契约、适用边界和维护不变量。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `GroupBox`.
  - Align generated output paths with `controls/group-box/index-cn.md` and `controls/group-box/semantic-cn.md`.

## 2026-06-19

- Docs
  - 新增 `implementation.md`，记录 GroupBox template part 接入、Header 缺口几何、自绘边框和维护不变量。
  - 将 `overview.md` 收敛为控件定位、公共契约、行为状态、视觉主题模型和验证入口。
  - 在 Data Display 分类入口中登记 GroupBox 实现原理文档。

## 2026-06-18

- Docs
  - 建立 GroupBox 控件文档目录，补齐 `overview.md`、`token.md` 和 `changelog.md`。
  - 记录 GroupBox Header、Content、Theme、Token、模板节点和兼容性不变量。
  - 明确 Header 缺口渲染模型：透明背景下标题区域不应依赖背景遮挡边框线。
- Theme
  - 明确 `PART_HeaderContent` 是 Header 缺口计算的稳定模板节点。
- Token
  - 按内容区域、Header 结构和 fieldset 语义分类记录 GroupBox Token 边界。
- Fix
  - 将 Header 缺口从背景遮挡改为边框几何排除，修复 `Background="Transparent"` 时标题下方露出边框短线的问题。
