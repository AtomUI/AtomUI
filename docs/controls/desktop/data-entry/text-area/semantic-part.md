# TextArea Semantic Part

TextArea 使用独立 descriptor，root 为隐式根合同；root 不生成专用 Style，使用公开根属性/普通 owner 样式。

| Part | 专用 Style | 最低 public ContractType | Route |
| --- | --- | --- | --- |
| textarea | TextAreaTextareaStyle | TextPresenter | /template/ .semantic-textarea |
| clear | TextAreaClearStyle | Avalonia Button | /template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear |
| count | TextAreaCountStyle | AtomUI TextBlock | /template/ .semantic-count |

三个部件均为单实例、静态模板节点、不跨 visual root，stable since6.2.0。textarea 拥有输入/光标/选择展示；count 位于 owner 底部；clear 位于右侧操作区。TextArea 不提供 prefix/suffix Part，resize handle 和 placeholder 也不是公共 Part。

专用 Style 在 AXAML 的 owner Style 中嵌套使用。TextPresenter 的字体样式使用 `TextElement.FontSize`、`TextElement.FontWeight`、`TextElement.FontStyle` 等附加 Avalonia 属性；不以 CLR wrapper 名称冒充可用于 Setter 的独立 AvaloniaProperty。禁止通过找节点再写属性作为定制手段。

共享规则见[Semantic Part 系统](../../../../architecture/systems/theming/semantic-parts.md)。
