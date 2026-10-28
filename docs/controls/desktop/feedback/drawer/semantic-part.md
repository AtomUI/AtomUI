# Drawer Semantic Part 契约

本文定义 Drawer 的 Semantic Part 契约：部件清单、marker 落点、跨根可达性与定制方式。控件定位与公共 API 见 [overview.md](overview.md)，实现结构见 [implementation.md](implementation.md)，系统级设计见 [Semantic Parts 系统设计](../../../../architecture/systems/theming/semantic-parts.md)，Gallery 预览模式见 [Semantic Part Preview](../../../../gallery/authoring/semantic-part-preview.md)。

## 1. Semantic Parts

Drawer 的语义 owner 是 `AtomUI.Desktop.Controls.Drawer` 本身（无模板的零尺寸标记控件）。全部非 root 部件位于运行时创建的 `DrawerContainer`（注入 `ScopeAwareAdornerLayer`，在 owner 可视子树之外、同一 TopLevel 之内），因此统一声明 `CrossVisualRoot=true` + `RuntimeCreated=true`；marker 静态声明在两个内部容器主题上（`DrawerContainerTheme.axaml`、`DrawerInfoContainerTheme.axaml`），运行时无需代码注入。

与上游 Drawer 的语义 DOM（`_semantic.tsx`，10 个槽位）的映射：

| 上游 Drawer 槽位 | AtomUI 部件 | 说明 |
| --- | --- | --- |
| `root` | `root`（隐式，owner 契约） | 上游 root 是 fixed 定位容器；AtomUI 遵循系统契约 root=owner 控件，上游 root 对应内部 `DrawerContainer` 基础设施，不作为部件暴露。内联语义预览中 owner 拉伸铺满舞台，root 高亮即整个内联容器，视觉语义对齐上游。 |
| `mask` | `mask` | 一一对应；`IsShowMask=false` 时不呈现（Optional）。 |
| `section` | `section` | 上游 v6 由 `content` 改名而来；AtomUI 由 `DrawerInfoContainer` 模板中的 `Frame` Border 承载（背景/阴影层）。 |
| `header` | `header` | 一一对应（`InfoHeader` Grid）。 |
| `title` | `title` | 一一对应（`HeaderText`）。 |
| `extra` | `extra` | 一一对应（`ExtraContentPresenter`）。 |
| `body` | `body` | 一一对应（`InfoContainer` presenter，`ContentPadding` 落点）。 |
| `footer` | `footer` | 一一对应（`InfoFooter` presenter，仅设置 Footer 时可见）。 |
| `close` | `close` | 一一对应（`PART_CloseButton` IconButton）。 |
| `dragger` | 不暴露 | 上游 v6 的 resizable 拖拽手柄；AtomUI Drawer 暂无 resizable 能力，待能力落地后按上游补齐。 |
| `wrapper` | 不暴露 | 上游动效包装容器，其语义预览清单同样不包含它；对应 AtomUI 的 `PART_Surface` 动效基础设施。 |

部件明细：

#### `mask`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `mask` |
| Selector | `.semantic-mask` |
| SelectorRoute | `>> .semantic-mask` |
| Style Type | `AtomUI.Theme.Styling.DrawerMaskStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Optional（`IsShowMask=false` 时不呈现） |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerContainerTheme.axaml` 的 `PART_Mask`（静态 marker） |
| 职责 | 遮罩层：绝对定位、层级、`ColorBgMask` 背景、指针事件命中 |
| 相关 API | `IsShowMask`、`IsCloseOnMaskClick` |
| 相关 Token | `ColorBgMask` |

#### `section`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `section` |
| Selector | `.semantic-section` |
| SelectorRoute | `>> .semantic-section` |
| Style Type | `AtomUI.Theme.Styling.DrawerSectionStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `Frame`（静态 marker） |
| 职责 | 抽屉面板容器：flex 布局、宽高、背景、按 Placement 的边缘阴影 |
| 相关 API | `DialogSize`、`Placement`、`SizeType` |
| 相关 Token | `ColorBgElevated`、`BoxShadowDrawer{Left,Right,Up,Down}` |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `>> .semantic-header` |
| Style Type | `AtomUI.Theme.Styling.DrawerHeaderStyle` |
| ContractType | `Avalonia.Controls.Grid` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `InfoHeader`（静态 marker） |
| 职责 | 头部区域：标题/关闭按钮/额外操作的排布、内边距 |
| 相关 API | `Title`、`Extra`、`IsShowCloseButton` |
| 相关 Token | `HeaderMargin` |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `>> .semantic-title` |
| Style Type | `AtomUI.Theme.Styling.DrawerTitleStyle` |
| ContractType | `AtomUI.Desktop.Controls.TextBlock` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `HeaderText`（静态 marker） |
| 职责 | 标题文字排版 |
| 相关 API | `Title` |
| 相关 Token | `FontSizeLG`、`FontHeightLG`、`FontWeightStrong` |

#### `extra`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `extra` |
| Selector | `.semantic-extra` |
| SelectorRoute | `>> .semantic-extra` |
| Style Type | `AtomUI.Theme.Styling.DrawerExtraStyle` |
| ContractType | `Avalonia.Controls.Presenters.ContentPresenter` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `ExtraContentPresenter`（静态 marker） |
| 职责 | 头部尾缘的额外操作内容呈现 |
| 相关 API | `Extra`、`ExtraTemplate` |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `>> .semantic-body` |
| Style Type | `AtomUI.Theme.Styling.DrawerBodyStyle` |
| ContractType | `Avalonia.Controls.Presenters.ContentPresenter` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `InfoContainer`（静态 marker） |
| 职责 | 主内容区：flex 占比、内边距、滚动 |
| 相关 API | `Content`、`ContentTemplate`、`ContentPadding` |
| 相关 Token | `ContentPadding` |

#### `footer`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `footer` |
| Selector | `.semantic-footer` |
| SelectorRoute | `>> .semantic-footer` |
| Style Type | `AtomUI.Theme.Styling.DrawerFooterStyle` |
| ContractType | `Avalonia.Controls.Presenters.ContentPresenter` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `InfoFooter`（静态 marker） |
| 职责 | 底部操作区：上分隔线、内边距；仅设置 Footer 时可见 |
| 相关 API | `Footer`、`FooterTemplate` |
| 相关 Token | `FooterPadding` |

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Drawer` / `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `>> .semantic-close` |
| Style Type | `AtomUI.Theme.Styling.DrawerCloseStyle` |
| ContractType | `AtomUI.Controls.IconButton` |
| Cardinality | Single |
| CrossVisualRoot / RuntimeCreated | true / true |
| AtomUI 节点 | `DrawerInfoContainerTheme.axaml` 的 `PART_CloseButton`（静态 marker） |
| 职责 | 关闭按钮：图标、hover/pressed 反馈；钉住预览时忽略其关闭请求 |
| 相关 API | `IsShowCloseButton`、`IsPinnedOpen` |
| 相关 Token | `CloseIconPadding`、`CloseIconMargin` |

隐式 `root`：由生成器无条件加入，`StyleType=null`，定制走 owner 级普通 Style（见系统文档）。

## 2. 职责与存在条件

- 部件仅在 `IsOpen=true` 且 `DrawerContainer` 挂到 `ScopeAwareAdornerLayer` 时物化；关闭后容器脱离 layer，所有部件目标消失。
- 容器挂层时通过 `ISetLogicalParent.SetParent(drawer)` 挂到 owner 逻辑树下（先于 `layer.Children.Add`，脱离时置空），owner 嵌套的生成 Semantic Style 因此可达；同时按 ImagePreviewer 先例显式中继 `ThemeVariantScope.ActualThemeVariant`。
- `Drawer : ISemanticPartCrossRootProvider` 在容器挂层/脱离/释放时触发 `CrossRootsChanged`，`GetCrossRoots()` 返回存活容器；Gallery 语义预览据此自动收集跨根，无需页面 code-behind 注册 `AdditionalRoots`（区别于 DropdownButton 的 Popup 根注册模式）。
- `IsPinnedOpen=true` 时遮罩点击与关闭按钮不再把 `IsOpen` 置 false（对齐上游语义演示的受控常开）；外部代码直接设置 `IsOpen` 仍正常关闭。

## 3. 数量语义

全部部件为 Single（每个 Drawer 至多一个实例），仅 `mask` 为 Optional（`IsShowMask=false` 时不呈现）。多抽屉场景中各 Drawer 的部件由各自的 `GetCrossRoots()` 容器承载，互不串扰；嵌套子 Drawer 复用父级同一 scope layer，但部件解析以各自容器为根。

## 4. Selector 用法

生成 Style 命名规则为 `Drawer{PascalCase(part)}Style`，位于 `AtomUI.Theme.Styling`（AXAML 前缀 `atom:`）。推荐写法（与 Gallery「语义样式」示例一致）：

```xml
<atom:Drawer.Styles>
    <Style Selector="atom|Drawer.semantic-object-style-demo">
        <atom:DrawerMaskStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#661677ff" />
        </atom:DrawerMaskStyle>
        <atom:DrawerSectionStyle x:SetterTargetType="Border">
            <Setter Property="CornerRadius" Value="8,0,0,8" />
        </atom:DrawerSectionStyle>
        <atom:DrawerTitleStyle x:SetterTargetType="atom:TextBlock">
            <Setter Property="Foreground" Value="#1677ff" />
        </atom:DrawerTitleStyle>
        <atom:DrawerCloseStyle x:SetterTargetType="atom:IconButton">
            <Setter Property="Foreground" Value="#1677ff" />
        </atom:DrawerCloseStyle>
    </Style>
</atom:Drawer.Styles>
```

规则要点：

- 生成 Style 必须作为 owner（或其业务 class）的嵌套 Style 子节点使用；`x:SetterTargetType` 必须写部件 ContractType（AtomUI 类型带 `atom:` 前缀，Avalonia 类型不带）。
- `root` 不生成 Style，owner 级 Setter 直接写在外层普通 Style 上。
- 禁止对 `DrawerContainer`/`DrawerInfoContainer` 直接写业务 Selector（internal 类型，不属于公共契约）；禁止 code-behind 找节点设属性替代生成 Style。

## 5. 定制边界

以下模板节点**不属于**语义部件，不承载稳定定制契约：`PART_RootClip`（宿主圆角裁剪）、`RootLayout`（两个容器各自的布局 Panel）、`InfoLayout`（行布局 Grid）、两条 `Separator`（分割线，随 header/footer 存在性显示）、`PART_Surface`（动效宿主）。上游的 `wrapper`/`dragger` 不暴露（见 §1 映射表）。

## 6. 兼容性与验证

契约变更定义：增删部件、修改 SelectorClass/SelectorRoute/ContractType/Cardinality 均为破坏性契约变更，需同步本文与 Gallery 演示。

与上游差异（设计决定，非缺陷）：

- `root` 指向 owner 控件（系统级契约），上游 root（fixed 容器）对应内部 `DrawerContainer`；内联语义预览通过让 owner 铺满舞台保持视觉等价。
- `dragger` 未实现（AtomUI Drawer 暂无 resizable）；`wrapper` 为动效基础设施，上游预览亦不暴露。

验证要求（均为可失败契约测试）：

- `tests/AtomUI.Desktop.Controls.Tests/Drawer/DrawerSemanticPartTests.cs`：descriptor 契约、两个容器主题的静态 marker 清单、打开后 marker 物化、容器逻辑父不变量、生成 Style 跨根命中全部 8 个目标、`IsPinnedOpen` 钉住语义、`GetCrossRoots` 上报。
- `tests/AtomUIGallery.Tests/ShowCases/DrawerSemanticPartHighlightTests.cs`：页面级端到端——语义页签内 9 张部件卡 hover 逐一高亮（各 1 个描边）、钉住忽略遮罩点击、页签切换清理高亮会话。
- `tests/AtomUIGallery.Tests/ShowCases/DrawerShowCasePageTests.cs`：AXAML 契约断言（内联钉住模式、PartDescriptions 顺序、生成 Style 唯一定制入口、无 code-behind 回退）。
