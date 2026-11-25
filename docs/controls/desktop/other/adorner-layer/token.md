# AdornerLayer Token 设计

`FocusOutlineOffset` 是默认焦点描边与目标边缘的距离。`LineWidthFocus` 和 `ColorFocusBorder` 使用 AdornerLayer 的有效全局值。默认焦点装饰的绘制部件成组读取三项资源，自定义焦点模板使用自身资源。

验证应覆盖实际启用状态、局部覆盖、根覆盖和独立注册身份。通用规则见[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。
