# CT13 / CT15 NU2-B：SpinnerBorderThickness 的实际 owner

既有两处SpinnerBorderThickness默认读取Shared.BorderThickness：ButtonSpinnerDecoratedBoxTheme与ButtonSpinnerHandleTheme。目标均为内部资产；先用真实独立ButtonSpinner和NumericUpDown Input证明当前1未跟随各自有效Global.BorderThickness，再按既有OwnerTokenResource解析公开owner。保留默认1、方向、内容占位、浮动/固定、描边绘制与public根BorderThickness职责，不把此包扩成LineWidth/BorderThickness重新定义。

- [x] R：两公开owner不同厚度3/5，实际DecoratedBox及Handle属性；0/清除、独立IconButton厚度干扰、Actual owner记录。必须先失败，不根据名称直接替换。
- [x] B：原明暗/三档Custom/左右/浮动，原内容与尺寸；默认及已支持配置/禁用/恢复，同完整历史AA/1DIP。保持实际悬停及输入，不隐藏手柄规避。
- [x] I：仅两Shared入口换Owner，复用现有绑定机制，无新字段、identity、converter或C#绑定。若StyleKey命中错误owner，按真实树修正方案，不把错误值当已支持。
- [x] V：动态值、0/清除/继承与实际绘制、原生managed/AOT/NET8；新有效厚度变化单独精确视觉审查。默认、矩阵及原始差异保留，无通用容差。
- [x] C：临时源/工程/媒体清理后精确模块、文档/记录、自审，无Git，完整家族/CT25继续。

NumericUpDown inline两条硬编码分隔边另包定义，不在这里借机改变。当前BorderThickness是既有别名输入；ControlAlgorithm.Disabled只给LineWidth不会自动重推BorderThickness，测试直接提供目标别名或显式Global算法，不重犯CR3的无效假设。
