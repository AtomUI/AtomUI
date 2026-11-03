# MarqueeLabel Token 设计

AtomUI:MarqueeLabel 持有 CycleSpace、DefaultSpeed 两 Own，使用 DependencyDefaultsV2 revision 1。四算法默认分别200与150，保留名称、Double 类型、key 和 slot；默认不依赖运行状态。

CycleSpace 进入 Root.CycleSpace，决定文字副本间距；DefaultSpeed 进入 Root.MoveSpeed，参与原时长。参数改变更新实际动画，清除回到当前主题，原生 Local 与普通 Root Style 可覆盖。速度参数与原4倍公式关系见[实现](implementation.md)，不能仅按字段注释改写实际速度。

零间距与正速度极值按实际范围验证。0速度沿原时长计算溢出边界，不静默夹值或合成停止态；负值、非有限值及全部距离组合不凭默认迁移推断支持。字体/前景使用 Root 明确 Global，未配置保持真实继承，不新增字体 Own；文本、可见性、悬停与 PivotOffset 不进入 Token。

无公开 Semantic Part Style，应用使用 Root 属性与 Style 定制。保持测量、裁剪、方向、原生内容和可见性合同，不把字段成功当完整周期或全部平台通过。[CT15 结果](../../../../superpowers/plans/2026-10-22-custom-theme/marquee-label-consumers-results.md)记录当前边界，[通用规范](../../../../engineering/development/control-token-guidelines.md)拥有跨控件规则。
