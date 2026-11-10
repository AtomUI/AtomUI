# AdornerLayer Token 设计

`FocusVisualMargin` 是唯一 Active Own Token；FocusVisualBorderThickness / ColorFocusBorder 使用 AdornerLayer 的有效全局值。只影响采用默认焦点装饰模板的控件；具有自定义焦点模板的控件维持自身契约。

验证应覆盖实际启用状态、局部覆盖、根覆盖和独立注册身份。通用规则见[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。
