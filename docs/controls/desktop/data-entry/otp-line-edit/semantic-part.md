# OtpLineEdit Semantic Part 契约

OtpLineEdit 的部件定制使用生成的专用 Style，在父 `Style Selector="atom|OtpLineEdit"` 内声明。
整体模型见[架构设计](overview.md)和[实现原理](implementation.md)，通用路由规则见[Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 部件表

### `root`

| 字段 | 值 |
| --- | --- |
| Owner | OtpLineEdit |
| Part | root |
| Selector | 控件自身 |
| SelectorRoute | 不适用 |
| Style Type | 不生成 |
| ContractType | OtpLineEdit |
| Cardinality | Single |
| Customization | Root |
| CrossVisualRoot | false |
| CrossNestedOwners | false |
| RuntimeCreated | false |
| AtomUI 节点 | 公开 owner |
| 职责 | 值、输入、校验与整体布局 |
| 相关 API | Text、Length、Status、IsEnabled |
| 相关 Token | OtpLineEdit 有效 Global / Own |
| 稳定性 | stable since 6.2.0 |

### `cellList`

| 字段 | 值 |
| --- | --- |
| Owner | OtpLineEdit |
| Part | cellList |
| Selector | `.semantic-cell-list` |
| SelectorRoute | `/template/ .semantic-cell-list` |
| Style Type | OtpLineEditCellListStyle |
| ContractType | ItemsControl |
| Cardinality | Single |
| Customization | Selector |
| CrossVisualRoot | false |
| CrossNestedOwners | false |
| RuntimeCreated | false |
| AtomUI 节点 | cell 集合宿主 |
| 职责 | 承载分格集合 |
| 相关 API | Length、CellWidth |
| 相关 Token | CellGap |
| 稳定性 | stable since 6.2.0 |

### `cell`

| 字段 | 值 |
| --- | --- |
| Owner | OtpLineEdit |
| Part | cell |
| Selector | `.semantic-cell` |
| SelectorRoute | `/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-cell` |
| Style Type | OtpLineEditCellStyle |
| ContractType | ContentControl |
| Cardinality | Multiple |
| Customization | Selector |
| CrossVisualRoot | false |
| CrossNestedOwners | false |
| RuntimeCreated | true |
| AtomUI 节点 | 内部 cell 的 ContentControl 契约 |
| 职责 | 分格外观与内容对齐 |
| 相关 API | CellBorderBrush、CellWidth、SizeType、StyleVariant |
| 相关 Token | CellWidth 三档、有效 Global 状态与几何 |
| 稳定性 | stable since 6.2.0 |

### `separator`

| 字段 | 值 |
| --- | --- |
| Owner | OtpLineEdit |
| Part | separator |
| Selector | `.semantic-separator` |
| SelectorRoute | `/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-separator` |
| Style Type | OtpLineEditSeparatorStyle |
| ContractType | Border |
| Cardinality | Multiple |
| Customization | Selector |
| CrossVisualRoot | false |
| CrossNestedOwners | false |
| RuntimeCreated | true |
| AtomUI 节点 | 分隔符外框 |
| 职责 | 分隔符布局与外框样式 |
| 相关 API | Separator、SeparatorTemplate |
| 相关 Token | 有效 Global MarginXXS |
| 稳定性 | stable since 6.2.0 |


## 各部件职责

`root` 不声明 `.semantic-root`。普通 owner Style 可设置整体尺寸、透明背景及公共属性。
`CellBorderBrush` 优先于根 `BorderBrush`，两者均以 LocalValue 投射到 cell；存在覆盖时，对应边框属性不再跟随状态机或较低优先级 Setter，清除后恢复。

`cellList` 定制集合宿主的布局属性。`Length` 和 `Text` 仍由 owner 管理，不通过 Style 替换 ItemsSource 或创造第二套输入状态。

`cell` 用专用 Style 设置 `Padding`、`BorderBrush`、`CornerRadius`、内容对齐等 ContentControl 支持的属性。
它不是公开的输入框实例，键盘焦点仍由 OtpLineEdit 持有。部件选择器经过集合生成的容器，使用逻辑后代步骤越过容器，再以直接子节点定位 cell；不依赖内部控件类型或 Name。

`separator` 定制分隔符外框的 `Margin`、`Padding`、`Background` 和 `CornerRadius`。
默认 Margin 来自有效 Global MarginXXS 的 Left，其他边为零。该默认值位于共享 ControlTheme Setter，专用 Style 可以覆盖。
部件随 cell 生成；最后一个分隔符宿主可存在但不可见，因此 Multiple 表达实现数量，不能等同于可见分隔符数量。内容及模板由 Separator / SeparatorTemplate 提供，文字墨迹居中由 presenter 负责。

## 声明式示例

```xml
<UserControl.Styles>
    <Style Selector="atom|OtpLineEdit">
        <atom:OtpLineEditCellStyle>
            <Setter Property="BorderBrush" Value="#FF00FF" />
            <Setter Property="Padding" Value="3" />
        </atom:OtpLineEditCellStyle>
        <atom:OtpLineEditSeparatorStyle>
            <Setter Property="Margin" Value="9,0,0,0" />
        </atom:OtpLineEditSeparatorStyle>
    </Style>
</UserControl.Styles>
```

Setter 遵守 Avalonia 原生优先级，不承诺普通 Style 能压过实例 LocalValue 或所有状态触发器。
专用 Style 必须命中实际 DataTemplate 节点；不能以 code-behind 获取内部节点后赋值替代。

## 验证边界

验证编译后的专用 Style 命中全部实际 cell 和 separator，检查关键 Setter 和清除后的主题恢复。
同时验证 Length 改变、四种 variant、四种 SizeType、正常输入及只读/禁用状态。
路由元数据相等或 marker 存在不能替代真实 Setter 命中证据。
