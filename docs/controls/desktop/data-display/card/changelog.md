# Card Changelog

本文档记录 Card 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-11-12

- Fix
  - 修复高 DPI（150% / 200%）下 `CardGridItem` 网格线只有 1 个物理像素、比卡片外边框细一半的问题。网格线由
    `CardGridItemShadows` 中偏移 `lineWidth` 的外阴影与内阴影绘制，而 Avalonia Skia 后端把阴影偏移按物理像素施加。
    `CardGridItem` 的 `Frame` 由 Avalonia `Border` 改为 `DashedBorder`，阴影经共享的 `BoxShadowRenderHelper` 绘制，偏移随渲染缩放；
    1x 下渲染结果不变，Token 未变。
- Tests
  - 新增 `CardGridItemShadowRenderingTests`，在 1x / 1.5x / 2x 下断言列间网格线与卡片外边框等宽。

## 2026-11-11

- Fix
  - 清空模板（Avalonia 不回调 `OnApplyTemplate`）后，已离开模板的 `LoadingHost` 不再接收新的加载指示器；再次切换 `IsLoading` 时移除旧指示器并丢弃该 host。

## 2026-11-10

- API
  - 新增共享基类 `AtomUI.Controls.Commons.AbstractCard : HeaderedContentControl`，承载 `Extra`/`ExtraTemplate`、`Cover`/`CoverTemplate`、`IsLoading`（默认 `false`）与 `Actions`；Desktop `Card` 改为继承它，同名公共成员与 `ExtraProperty` 等静态属性字段仍可通过 `Card` 访问，AXAML 与 C# 用法不变。
  - `AbstractCard` 新增 `protected virtual bool HasHeaderContent()`（默认 `Header`、`HeaderTemplate`、`Extra`、`ExtraTemplate` 任一非空）驱动 `:headerless`，`protected virtual void OnActionsChanged(NotifyCollectionChangedEventArgs)`，以及加载指示器钩子 `protected abstract Control CreateLoadingIndicator()`、`protected virtual AttachLoadingIndicator/DetachLoadingIndicator`（默认加入、移出模板部件 `PART_LoadingHost`）和 `protected Panel? LoadingHost`。
  - 新增 internal `IsActionsPanelVisible`、`IsContentVisible`（等于 `!IsLoading`）、`LoadingIndicator` 与常量 `LoadingHostPart`。
  - Desktop `Card` 新增 `protected override CreateLoadingIndicator()`，返回 `IsLoading`、`IsActive` 为真、`ParagraphRows=4`、`IsShowTitle=false`、拉伸对齐、`ClipToBounds=False` 的 `Skeleton`；`OnActionsChanged` 改为把 Actions 同步到 `CardActionPanel`。
  - 控件内部的集合辅助 `ControlCollectionChangedUtils` 从 Desktop 程序集移入 `AtomUI.Controls`（internal），`AbstractSpace` 删除其私有重复实现并共用它。
  - `HeaderBorderThickness`、`EffectiveBorderThickness`、`EffectiveCornerRadius` 只服务 Desktop 独有的 Grid 内容类型，保留在 Desktop `Card`，不下沉。
- Theme
  - `CardTheme.axaml` 的 `Border#CardContent`（语义类 `semantic-body` 不变）内，原先常驻的 `atom:Skeleton` 改为 `Panel#PART_LoadingHost` 加 `ContentPresenter`：呈现器绑定 `Content`、`ContentTemplate`、水平与垂直内容对齐，`IsVisible` 绑定 `IsContentVisible`；加载期间 Skeleton 加入该 Panel。
- Fix
  - 不加载的 Card 不再持有 Skeleton 及其模板节点；骨架只在 `IsLoading` 为真时创建，加载结束或重新应用模板时移除并丢弃。
- Desktop 影响
  - 默认（非加载）Card 的视觉元素数（无头渲染树实测，对照批前的树 `93990a8ec085`）：含 Header 与单个 `TextBlock` 内容的卡片由 22 个减到 14 个，含 Header、Extra 与三个 `TextBlock` 内容的卡片由 26 个减到 18 个，每卡减少 8 个。原先常驻的 `Skeleton` 及其占位（`Border#PART_RootLayout`、占位 `Panel`、`Grid`、`DockPanel`、`SkeletonAvatar`、`StackPanel#PART_Content`、`SkeletonTitle`、`SkeletonParagraph`）共 9 个，改为一个 `Panel#PART_LoadingHost`。其中 6 个来自共享骨架的占位改为按需创建（见 Skeleton changelog），2 个来自 Card 不再常驻 `Skeleton`（含 `PART_RootLayout` 与 `PART_PlaceholderHost`，换成 `PART_LoadingHost`）。其余场景同样每卡减少 8 个（无头部 24 → 16，带封面 27 → 19，带操作区 37 → 29，Meta 内容 40 → 32，Grid 内容 50 → 42，Tabs 内容 61 → 53）。加载中的 Card 减少 2 个（39 → 37）。
  - 加载中的 Card：Skeleton 与其占位在加载开始时创建，视觉与改动前一致（四行段落、无标题、微光激活、拉伸）；切换到 `IsLoading=false` 后骨架被移除，内容呈现器恢复显示。
  - 公共 API：除新增成员与基类变为 `AbstractCard`（`HeaderedContentControl` 仍在继承链上）外没有变化。
  - 边框计算保留在 Desktop，Header/Content 的边框与圆角行为不变。
  - 公共 API 快照：仓库外的反射工具分别读取改动前（工作区快照 `93990a8ec085`，以 `git archive` 导出到仓库外构建）与改动后的 `AtomUI.Desktop.Controls.dll`、`AtomUI.Controls.dll`，对全部公开类型输出“类型简名、基类简名链、接口、展开继承后的 public/protected 成员签名”，按类型简名合并后逐类型比对。`CardMetaContent`、`CardGridContent`、`CardTabsContent`、`CardGridItem`、`CardActionButton`、`CardActionPanel`、`CardToken` 无差异；`Card` 的差异只有基类链插入 `AbstractCard`，以及来自它的 protected 成员 `HasHeaderContent`、`CreateLoadingIndicator`、`AttachLoadingIndicator`、`DetachLoadingIndicator`、`OnActionsChanged`、`LoadingHost`；`Extra`、`ExtraTemplate`、`Cover`、`CoverTemplate`、`IsLoading`、`Actions` 及其静态属性字段在展开继承后的签名不变，没有成员被删除。
  - 无头渲染对照（V6 回归）：同一个无头渲染驱动（Skia，仓库外）分别引用改动前与改动后的树，浅色、深色各渲染 Card 的 28 个场景（默认、无头部、仅头部、无 Extra、封面、封面无头部、操作区、封面加操作区、加载态（含无头部、带操作区）、加载中切到非加载与非加载切到加载、小与大两档尺寸及其加载态、Borderless、Inner 与嵌套 Inner、Hoverable、Meta 内容及与封面和操作区组合、Grid 内容（含无头部）、Tabs 内容（含无头部与 `TabBarExtraContent`）），共 56 张逐像素比对，全部一致。加载态比对的是静止帧：改动前后的加载指示器都把 `IsActive` 置为假后截取；改动后 `IsActive` 为真且 `IsMotionEnabled=false` 的加载帧同样与改动前一致。同一驱动对改动前的树连续渲染两次，结果逐像素相同。
- Tests
  - 新增 `AbstractCardTests`（`AtomUI.Controls.Tests`）：默认值、加载指示器仅在加载期间存在、模板应用前加载、内容可见性切换、快速切换无残留、重新应用模板、缺少宿主不创建、Actions 钩子与面板可见性、Headerless 判定。
  - `CardBehaviorTests` 追加：`Skeleton_exists_only_while_loading`、`Loading_toggle_restores_content`、`Actions_sync_and_visibility`、`Headerless_follows_header_and_extra`、`Not_loading_card_has_no_skeleton_visuals`（含视觉元素数上限）。
  - V6 回归：`dotnet test tests/AtomUI.Controls.Tests` 222 项、`tests/AtomUI.Desktop.Controls.Tests` 4,179 项、`tests/AtomUIGallery.Tests` 412 项（含 `CardShowCasePageTests`，Card 展示页可打开）全部通过，`controlgallery/AtomUIGallery.Desktop` 构建通过。

## 2026-11-08

- Changed
  - 主题改读控件有效 Token：Card、CardActionButton、CardMetaContent 以各自 TokenResource 读取全局 Token；`CardGridItem`、`CardTabsContent`
    标为 Token owner 透明部件，与无 identity 的 `CardActionPanel` 一样读取 Card 的 Token，`Controls[Card]` 作用于网格项、页签内容与操作区。
    Card 圆角经 `Controls[Card].BorderRadiusLG` 设置。
  - `ExtraColor` 接通头部 extra 文字色；`ActionsBg` 改由 `CardActionPanel` 在 Render 中绘制（此前模板背景的 TemplateBinding 遮蔽了样式
    值，从未生效），分隔线保持在背景之上，操作项间隙改读 `LineWidth`；删除无效的 `Padding="0,1,0,0"` 字面量。
  - `ActionsSpacing`、`TabsMarginBottom`、`CardHeadPadding`、`CardPaddingBase` 标为保留 Token（antd `actionsLiMargin`、`tabsMarginBottom`、
    `cardHeadPadding`、`cardPaddingBase`）。默认渲染不变。

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
