# TextBlock 实现原理

HyperLinkTextBlock 的 PART_Text 投射根 FontSize、FontStyle、FontWeight、Text 与 Background。默认不设置容器背景 Token，保留父级表面；显式根 Background 通过现有绑定绘制。链接前景由启用、悬停、按下和禁用状态决定。SelectableTextBlock 在文字选区绘制独立背景及前景。绑定由 ControlTheme 管理，重新套模板由 Avalonia 清理，不增加订阅。

契约验证：`DisplayFamilyEffectTests` 的对应效果、作用域和资源生命周期用例。
