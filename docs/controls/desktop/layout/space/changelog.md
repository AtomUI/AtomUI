# Space Changelog

本文档记录 Space 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-11

- Fix
  - 换行时只断开已有项的行：首个可见项单独超出可用宽度（或高度）时直接作为首行，不再产生一条空行及其 `LineSpacing`，水平与垂直布局的首行原点均为 0。

## 2026-11-10

- API
  - 新增共享基类 `AtomUI.Controls.Commons.AbstractSpace`，`Space` 改为继承它。`Children`、`IsItemsHost`、`Orientation`、`ItemSpacing`、`LineSpacing`、`ItemsAlignment`、`ItemWidth`、`ItemHeight`、子元素同步、测量与排列、`IChildIndexProvider`、`INavigableContainer` 移入基类，属性的名称、默认值与 `Space` 上的静态字段访问方式不变；`Space` 保留 `SizeType`、`SplitTemplate`、背景与边框（含虚线）、尺寸档 Token 绑定、语义标记与 `Render`。
  - 基类新增 `IsWrap`（默认 `true`，`AffectsMeasure`）：为 `false` 时所有元素在同一行，超出部分溢出，不压缩元素。
  - 基类新增 `Justify`（`SpaceJustify`：`Start`、`End`、`Center`、`Between`、`Around`、`Evenly`，默认 `Start`，`AffectsArrange`）：每行独立分配剩余主轴空间，不改变元素尺寸；`R ≤ 0` 时不移动，`n = 1` 时 `Between` 等同 `Start`。
  - `SpaceItemsAlignment` 的命名空间由 `AtomUI.Desktop.Controls` 改为 `AtomUI.Controls`，值不变；XAML 仍使用 `https://atomui.net`，只引用 `AtomUI.Desktop.Controls` 的 C# 代码需要补 `using AtomUI.Controls`。
  - `Space` 原有的 `protected virtual HandleChildrenChanged` 随子元素同步移入基类，签名不变，`Space` 改为覆盖它以维护语义标记与分隔符；基类另新增 `private protected virtual InvalidateMeasureOnChildrenChanged` 与 `GetLayoutInset`（`Space` 返回 `Padding + BorderThickness`）。
- Theme
  - 无变化：`Space` 本体没有 ControlTheme，Token 与尺寸档绑定不变。
- Fix
  - `ItemsAlignment` 为 `Center` 或 `End` 且发生换行时，第二行起的元素原先没有加上本行的纵向起点，所有行都挤在第一行；现在每行在自己的行高内对齐。
- Desktop 影响
  - 默认行为不变：`IsWrap` 默认 `true`、`Justify` 默认 `Start`；`ItemsAlignment` 为 `Start`（默认）或单行时排列结果与改动前相同。
  - 行为修复：换行后使用 `Center` / `End` 的 `Space` 现在逐行对齐，此前的重叠属于缺陷。
  - 公共类型变化：`SpaceItemsAlignment` 命名空间变化（见 API）；其余为新增成员，源码兼容，二进制上 `SpaceItemsAlignment` 的类型标识变化需要重新编译引用方。
  - 公共 API 快照：仓库外的反射工具分别读取改动前（工作区快照 `93990a8ec085`，以 `git archive` 导出到仓库外构建）与改动后的 `AtomUI.Desktop.Controls.dll`、`AtomUI.Controls.dll`，对全部公开类型输出“类型简名、基类简名链、接口、展开继承后的 public/protected 成员签名”，按类型简名合并后逐类型比对。`Space` 与 `SpaceItemsAlignment` 的差异只有：`SpaceItemsAlignment` 的命名空间；`Space` 的基类链插入 `AbstractSpace`；`Space` 新增 `IsWrap`、`IsWrapProperty`、`Justify`、`JustifyProperty`；新增枚举 `SpaceJustify`（`AtomUI.Controls`）。其余成员（含 `ItemSpacing`、`LineSpacing`、`Orientation`、`ItemsAlignment`、`ItemWidth`、`ItemHeight`、`Children` 及其静态属性字段）的签名、访问级别与静态性不变，没有成员被删除。`CompactSpace`、`CompactSpaceItem`、`CompactSpaceAddOn`、`CompactSpaceFiller` 无差异。
  - 无头渲染对照（V6 回归）：同一个无头渲染驱动（Skia，仓库外）分别引用改动前与改动后的树，浅色、深色各渲染 Space 的 22 个场景（小、中、大三档尺寸，自定义间距，纵向，换行与行间距，换行后 `Start`，单行 `Start`/`Center`/`End`（元素高度不同），纵向 `Center`/`End`，`SplitTemplate`（横向与纵向），背景与边框，虚线边框，`ItemWidth`/`ItemHeight`，嵌套），共 44 张逐像素比对，40 张完全一致。有差异的是换行后 `ItemsAlignment` 为 `Center`、`End` 的两个场景（浅色、深色各两张，每张约 12,956 像素变化）：改动前第二行的元素叠在第一行上，改动后各行在自己的行高内对齐，即上文的行为修复。同一驱动对改动前的树连续渲染两次，结果逐像素相同。
- Tests
  - 新增 `AbstractSpaceTests`（共享层）：默认值、六种 `Justify` 的位置、`n = 1` 的 `Between`、溢出时不移动不压缩、隐藏子元素不占位不产生间距、`IsWrap = false` 保持单行、换行后 `Center` / `End` 在本行内对齐、纵向主轴的分布。
  - 新增 `SpaceWrapJustifyTests`（Desktop）：默认换行、换行后的居中对齐、`Justify` 在 root frame inset 内分配。
  - 现有 `CompactSpaceBehaviorTests`、`SpaceRootFrameTests`、`SpaceSemanticPartTests` 保持不变并通过。
  - V6 回归：`dotnet test tests/AtomUI.Controls.Tests` 222 项、`tests/AtomUI.Desktop.Controls.Tests` 4,179 项、`tests/AtomUIGallery.Tests` 412 项（含 `SpaceShowCasePageTests`，Space 展示页可打开）全部通过，`controlgallery/AtomUIGallery.Desktop` 构建通过。

## 2026-08-21

- Code
  - Add root frame appearance properties (`Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding`, `BorderDashArray`, `BorderDashOffset`) to `Space`, rendered through `BorderRenderHelper`, so Ant Design style-class root decoration (dashed border, background and padding) can be expressed directly on the control.
  - Make `Padding + BorderThickness` participate in the measure/arrange frame inset so children stay inside the root border.
- Gallery
  - Migrate `SpaceShowCase` to `GalleryShowCaseHost` with a Semantic Parts preview for `root` / `item` / `separator`.
  - Add a `Custom Semantic Part styling` example aligned with the Ant Design `style-class` demo: a dashed root border with grey item background and red bold separator, plus a `size=large` root with light blue background.
  - Add localization units for the semantic part descriptions, the style demo title/description and the demo button labels (en-US / zh-CN / zh-TW / pt-BR).
- Tests
  - Add `SpaceRootFrameTests` covering layout inset, legacy default layout and the appearance API surface.
  - Extend `SpaceShowCasePageTests` with document layout, semantic preview materialization, official style values and approved localization copy checks.
- Docs
  - Document the root customization boundary in the Space Semantic Part contract and update the Space overview and implementation docs.

## 2026-08-20

- Code
  - Add `Space` semantic parts for `root` / `item` / `separator` and propagate runtime markers from direct children and split separators.
  - Generate `SpaceItemStyle` and `SpaceSeparatorStyle` contract types through the semantic-part generator.
- Tests
  - Add regression coverage for the `Space` descriptor, runtime marker counts, separator rebuild behavior and generated style routing.
- Docs
  - Add the dedicated `Space Semantic Part` contract document and link it from the Space architecture and implementation docs.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Space`.
  - Align generated output paths with `controls/space/index-cn.md` and `controls/space/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Space desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Space desktop architecture documentation under `docs/controls/desktop/layout/space/overview.md`.
  - Add Space implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Space control-level changelog.
  - Add Space Token documentation covering SpaceToken.
