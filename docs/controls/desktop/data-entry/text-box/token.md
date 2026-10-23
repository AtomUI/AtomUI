# TextBox Token

`AtomUI:TextBox` 的 Own 定义是 TextBoxToken，继承 AbstractControlDesignToken。字段是内容布局默认值，不保存文本、光标或实例状态。

| Own 字段 | 职责与依赖 |
| --- | --- |
| ContentPadding | 中档内容 inset，依赖 ControlHeight、FontSize、RelativeLineHeight、UniformlyPaddingSM 和 LineWidth。 |
| ContentPaddingLG | 大档 inset，读取对应大档高度/字号/行高与 ControlPaddingHorizontal。 |
| ContentPaddingSM | 小档 inset，读取对应小档高度/字号/行高与 ControlPaddingHorizontalSM。 |

边宽与圆角是本身份 Effective Global 的几何输入，不复制为新的 Own 字段。根 Padding 可覆盖内容 inset；Token 与实例值的职责见[共享输入架构](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

## Effective Global消费

TextBoxTheme自身读取字号、行高、selection/caret、placeholder、间距与EnableMotion时使用TextBox typed Global；不得以SharedTokenResource绕过该身份配置。此迁移保留原尺寸/IsCustomFontSize条件和Setter优先级；内容padding仍是原三个Own。内部公共frame的跨owner颜色/动效路由另按共享输入架构验收，不以根主题迁移替代。

## 默认求值协议

本Own定义使用DependencyDefaultsV2、Revision1，默认工厂以有效Global为输入，各字段没有Own之间的依赖；显式Own覆盖优先。名称、类型、公共资源编号及既有单位/范围保持，不因迁移另建别名。Global变化时重新计算未显式覆盖的默认值，清除Own后恢复对应公式。

三档垂直padding保留原有数值语义：中/小档使用Math.Round(.../2*10)/10再减LineWidth，大档使用Math.Ceiling(.../2*10)/10再减LineWidth；不能为了统一公式改变默认像素。水平padding仍分别使用UniformlyPaddingSM、ControlPaddingHorizontalSM、ControlPaddingHorizontal减LineWidth。
