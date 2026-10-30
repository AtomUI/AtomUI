# Gallery ShowCase Design Pattern

本文档约定 AtomUI Gallery 控件 ShowCase 页面的稳定结构。标准控件页面面向示例浏览，直接展示 `ExamplesContent`；API 和 Design Token 表不再由 Gallery 收集或保留，相关契约回到控件文档、源码 public surface、Token 类型和生成数据维护。

采用 Semantic Part 的页面通过独立 `Semantic Parts` Tab 展示公共视觉契约。该 Tab 是公共视觉契约的交互式检查工具，
不属于 API/Design Token sidecar，也不能恢复页面私有场景 controller。完整设计见
[Semantic Part Gallery Preview](semantic-part-preview.md)。

## 页面模型

标准控件 ShowCase 页面采用下面的结构：

```text
ShowCase Page
  GalleryStickyTabsHost
    Header
      GalleryShowCaseHeader
    Content
      ExamplesContent
        ShowCasePanel
          ShowCaseItem
            DeferredContentTemplate
```

推荐 AXAML 结构：

```xml
<gallery:GalleryStickyTabsHost>
    <gallery:GalleryStickyTabsHost.Header>
        <gallery:GalleryShowCaseHeader
            Title="Button"
            Category="{gallery:ButtonShowCaseLangResource ComponentCategory}"
            Status="{gallery:ButtonShowCaseLangResource ComponentStatusStable}"
            Subtitle="{gallery:ButtonShowCaseLangResource PageSubtitle}"
            Description="{gallery:ButtonShowCaseLangResource PageDescription}"
            Namespace="AtomUI.Controls"
            Package="AtomUI.Desktop.Controls"
            BaseClass="AtomUIButton" />
    </gallery:GalleryStickyTabsHost.Header>

    <gallery:ShowCasePanel Name="ExamplesContent"
                           IsScrollEnabled="False"
                           IsDeferredLoadingEnabled="True"
                           InitialDeferredLoadItemCount="4"
                           DeferredLoadBatchSize="2"
                           ContentMargin="28,10,28,28">
        <gallery:ShowCaseItem Title="..."
                              Description="..."
                              IsDeferredContentEnabled="True">
            <gallery:ShowCaseItem.DeferredContentTemplate>
                <DataTemplate x:DataType="vm:CurrentShowCaseViewModel">
                    <!-- demo content -->
                </DataTemplate>
            </gallery:ShowCaseItem.DeferredContentTemplate>
        </gallery:ShowCaseItem>
    </gallery:ShowCasePanel>
</gallery:GalleryStickyTabsHost>
```

标准页面不得声明：

- `GalleryStickyTabsHost.StickyContent`
- `ScenarioTabs`
- `ScenarioContentHost`
- `Tag="Api"` 或 `Tag="DesignToken"`
- `GalleryShowCaseScenarioController`
- `CreateScenarioContent`
- `new XxxApiDataGrid()` 或 `new XxxDesignTokenDataGrid()`
- API / Design Token DataGrid sidecar 引用或 `ApiRows` / `DesignTokenRows` metadata 绑定

上述限制适用于当前只展示 Examples 的标准页面。采用 Semantic Part Preview 后，页面必须使用 GalleryBase 统一提供的
`GalleryShowCaseHost`，由宿主创建标准 `Examples/Semantic Parts` Tab；页面仍然不得手写 `ScenarioTabs`、
`ScenarioContentHost`、`GalleryShowCaseScenarioController` 或切换 code-behind。

`IconShowCase`、`PaletteShowCase` 这类示例本身需要分组的特殊页面可以继续使用 `TabStrip + ContentControl`。这些 Tab 表示示例分组，例如 `Outlined/Filled/TwoTone` 或 `Light/Dark`，不是标准页面的 API/Design Token 导航。

## 放大布局策略

`ShowCaseItem.ZoomContentLayout` 使用 `ShowCaseZoomContentLayout`，默认值为 `Natural`，保留自然尺寸居中的展示行为。

- `Natural`：以舞台宽度和无界高度测量，按不超过舞台的期望尺寸分配居中的排列槽。按测量约束铺满宽度的内容仍重新布局铺满。
- `Stretch`：使用舞台的可用宽高测量，并提供完整舞台排列槽。内容自身的 Width/Height、Min/Max、Margin 和对齐继续生效；不修改尺寸属性或添加 RenderTransform 缩放。

需要有效展示面积的空画布、水印区域或受限滚动内容，应在示例卡片声明策略，而不是根据控件类型、对齐属性赋值来源或旧卡片宽度推断：

```xml
<gallery:ShowCaseItem ZoomContentLayout="Stretch" IsDeferredContentEnabled="True">
    <gallery:ShowCaseItem.DeferredContentTemplate>
        <DataTemplate x:DataType="x:Object">
            <Border Height="300" HorizontalAlignment="Stretch" />
        </DataTemplate>
    </gallery:ShowCaseItem.DeferredContentTemplate>
</gallery:ShowCaseItem>
```

放大期间修改策略会重新测量和排列同一内容实例。舞台通过编译型 AXAML 绑定读取源卡片策略，关闭后清除源引用并归还内容。
重新应用 Overlay 模板时，必须先清除旧舞台的视觉子级，再将同一内容实例交给新舞台；内容的逻辑挂载仍遵循源卡片的资源作用域。

## Semantic Parts Tab 范式

Semantic Part 接入页面迁移到 GalleryBase 统一提供的 `GalleryShowCaseHost`，由页面提供原 Examples 内容和
`SemanticPartsContentTemplate`：

```text
GalleryShowCaseHost
  GalleryStickyTabsHost
    Header
      GalleryShowCaseHeader
    StickyContent
      TabStrip
        Examples
        Semantic Parts
    Content
      当前 Tab 内容
```

页面编写只承担以下契约：

- Examples 保持默认 Tab，原有 `ShowCasePanel` 和 deferred item 不变。
- 只有页面提供 `SemanticPartsContentTemplate` 时才显示 Semantic Parts Tab。
- 没有 Semantic 内容的页面不创建空 TabStrip 或 sticky 行。
- 页面不得直接创建 Preview、高亮会话或 Adorner，也不得手写 Tab 切换 code-behind。

Button 是首个完整接入样例，但不拥有 Tab、Preview、descriptor 读取或 Adorner 架构。

真延迟创建、owner-scoped 目标解析、Popup、Adorner 释放、性能预算和完整验证矩阵统一由
[Semantic Part Gallery Preview](semantic-part-preview.md) 定义。

## Header 范式

Examples-only 页面在 `GalleryStickyTabsHost.Header` 中使用 `GalleryShowCaseHeader`；启用 Semantic Parts 的页面在
`GalleryShowCaseHost.Header` 中使用同一个 Header 控件。页面不再手写 `StackPanel + Tag + metadata` 页头结构。

推荐内容：

- 控件名。
- 控件类别，例如 `General`、`Data Entry`、`Data Display`。
- 稳定性或状态，例如 `Stable`、`Preview`。
- 一句话 subtitle。
- 一段 description。
- `Namespace`、`Package`、`BaseClass` 等紧凑 metadata。

Header 不应放大量营销文案、复杂交互示例、可替代 Examples 的演示内容，或会让首屏过高的长篇说明。

## Examples 范式

Examples 使用 `ShowCasePanel + ShowCaseItem`。标准页面中 `ShowCasePanel` 关闭内部滚动，把滚动权交给 `GalleryStickyTabsHost`：

```xml
<gallery:ShowCasePanel IsScrollEnabled="False"
                       IsDeferredLoadingEnabled="True"
                       InitialDeferredLoadItemCount="4"
                       DeferredLoadBatchSize="2">
```

延迟加载是 ShowCase 创建规范的一部分，不是可选优化：

- `IsDeferredLoadingEnabled` 必须为 `True`。
- `InitialDeferredLoadItemCount` 默认使用 `4`。
- `DeferredLoadBatchSize` 默认使用 `2`。
- 每个有演示内容的 `ShowCaseItem` 必须设置 `IsDeferredContentEnabled="True"`。
- 演示控件必须放入 `ShowCaseItem.DeferredContentTemplate`。
- `DeferredContentTemplate` 必须声明 `x:DataType`，通常使用当前 ShowCase 的 ViewModel 类型。
- 已 materialize 的内容不回收，避免破坏状态、焦点、Popup/Flyout 生命周期。

硬边界：

- 移入 `DeferredContentTemplate` 只能改变创建时机，不能改变演示控件树。
- 不允许把演示控件直接写成 `ShowCaseItem.Content`。
- 改造页面结构时不顺手调整示例控件数量、层级、默认值、文案或视觉布局。
- 对已整理过的页面保留 snapshot 测试，确保示例内容不会被布局改造误改。
- 普通示例不得依赖控件内部 `PART_*`、具体模板节点或自行推测模板边界。演示已发布 Semantic Part 时，必须使用
  “owner-scoped Control Selector + 生成 Semantic Style”结构，并通过 `x:SetterTargetType` 声明 Setter 类型上下文；
  复杂 `SelectorRoute` 由生成 Style 封装，只保留在 descriptor 技术元数据中，不能复制成用户主路径。

## API 与 Design Token

标准 Gallery 页面不展示、实例化或保留 API / Design Token 表格。不得新增或恢复以下旧结构：

- `*ApiDataGrid.axaml`、`*ApiDataGrid.axaml.cs`
- `*DesignTokenDataGrid.axaml`、`*DesignTokenDataGrid.axaml.cs`
- ViewModel 中的 `ApiRows`、`DesignTokenRows`、`EnsureApiRows()`、`EnsureDesignTokenRows()`
- `*ApiRow`、`*DesignTokenRow` record 或只服务旧表格的本地化资源

API 和 Token 契约的维护入口是控件文档、源码 public surface、Token 类型和生成数据；Gallery 只作为稳定示例与源码片段来源。

## Sticky Host

`GalleryStickyTabsHost` 是 Gallery 专用文档页宿主，负责页面级滚动、Header 和主体内容布局。标准页面没有 `StickyContent` 时，模板中的 sticky 行必须折叠，不保留空白导航条。

特殊页面使用 `StickyContent` 时仍遵循原 sticky 规则：

- 真实 `StickyContent` 始终保留在模板里的 inline presenter。
- 不把同一个 TabStrip 移动到 Popup、OverlayLayer 或其他 presenter。
- sticky mirror 只作为只读视觉镜像，`IsHitTestVisible=false`。
- 没有 `StickyContent` 时不创建 sticky mirror。
- `ScrollChanged` 订阅必须在 detach 时释放。

## Spacing

标准页面默认节奏：

- Header 左右：由 `GalleryShowCaseHeader` 和宿主样式控制。
- Examples：`ContentMargin="28,10,28,28"`。

标准页面不再需要让 Header、Tab、Examples、API、Token 五者对齐；只需要 Header 与 Examples 的阅读节奏稳定。

## Code-Behind

标准页面 code-behind 只保留当前控件自己的运行时逻辑，例如：

- 示例事件 handler。
- ViewModel 初始化或语言切换数据刷新。
- 示例控件 materialize 后的数据源补接。
- detach 时清理示例数据源或订阅。

标准页面不得保留标准 API/Token 场景切换逻辑：

- 不声明 `ApiScenario` / `DesignTokenScenario`。
- 不持有 `GalleryShowCaseScenarioController`。
- 不在 attach/detach/DataContextChanged 中调用 `_scenarioController`。
- 不声明 `CreateScenarioContent`。

`IconShowCase`、`PaletteShowCase` 等特殊分组页面可以继续使用 `GalleryShowCaseScenarioController` 管理自己的示例分组 lazy content。

## 测试范式

每个标准 ShowCase 至少覆盖两类测试。

以下结构测试适用于当前 Examples-only 页面。采用 `GalleryShowCaseHost` 的页面除保护原 Examples 内容外，还必须执行
[Semantic Part Gallery Preview](semantic-part-preview.md#15-验证要求) 的延迟创建与生命周期验证。

结构测试：

- Examples-only 页面使用 `GalleryStickyTabsHost`；Semantic Part 页面使用 `GalleryShowCaseHost`。
- Header 使用 `GalleryShowCaseHeader`。
- 主页面不存在 `ScenarioTabs`、`ScenarioContentHost`、`Tag="Api"`、`Tag="DesignToken"`。
- 存在直接声明的 `ExamplesContent`。
- `ExamplesContent` 设置 `IsScrollEnabled="False"`。
- `ExamplesContent` 设置 `IsDeferredLoadingEnabled="True"`。
- 每个有演示内容的 `ShowCaseItem` 设置 `IsDeferredContentEnabled="True"`。
- `ShowCaseItem` 数量与 `DeferredContentTemplate` 数量一致。
- 深模板样式门禁只接受 Gallery 自有 ControlTheme、Gallery 自有控件的实例 Styles，或符合公共 Semantic Part 结构的
  owner-scoped 单边界 selector；普通 class、`PART_*` 和多重模板边界必须被拒绝。

演示内容保护测试：

- 从第一个 `ShowCaseItem` 到 `ShowCasePanel` 结束提取示例内容。
- 与 approved snapshot 比较。
- snapshot normalize 可以剥离 deferred wrapper，但不得掩盖真实演示控件变化。

## 迁移检查清单

迁移或新增标准 ShowCase 时按以下顺序处理：

1. 保留原 `ShowCaseItem` 内容，先建立或确认 snapshot。
2. 使用 `GalleryShowCaseHeader`。
3. Examples-only 页面用 `GalleryStickyTabsHost` 包裹页面主体；需要 Semantic Parts 时改用 `GalleryShowCaseHost`。
4. 删除标准 `Examples/API/Design Token` Tab。
5. 删除 `ScenarioContentHost`，让 `ExamplesContent` 成为直接主体。
6. 保持 `ShowCasePanel IsScrollEnabled=False`。
7. 开启 panel 级延迟加载：`IsDeferredLoadingEnabled=True`、`InitialDeferredLoadItemCount=4`、`DeferredLoadBatchSize=2`。
8. 每个有演示内容的 `ShowCaseItem` 使用 `IsDeferredContentEnabled=True` 和 `DeferredContentTemplate`。
9. 删除 API/Design Token sidecar DataGrid 文件、ViewModel rows、row record 和旧表格本地化资源。
10. 从 code-behind 删除标准场景 controller 和 `CreateScenarioContent`。
11. 跑结构测试、deferred 创建测试、snapshot 测试和 Gallery 测试。

## 不做事项

- 不新增或恢复 API/Token DataGrid sidecar 文件。
- 不把 API/Token 表格改写进主 ShowCase。
- 不在 ViewModel 中维护只服务旧 API/Token 表的 metadata rows。
- 不把演示控件直接写在 `ShowCaseItem` 内容区。
- 不为标准页面保留 `Examples/API/Design Token` 一级 Tab。
- 不在每个页面私有一套标准场景 lazy cache。
- 不让 Header 和 Examples 拥有不同滚动上下文。
- 不为 sticky tabs 复制一份可交互 TabStrip。

## 推广结果

标准控件 ShowCase 已按“Header + direct ExamplesContent”范式迁移。API/Design Token sidecar 文件和 ViewModel metadata rows 已从 Gallery 清除；`GalleryShowCaseScenarioController` 只服务 Icon、Palette 这类真实需要示例分组 lazy content 的特殊页面。
