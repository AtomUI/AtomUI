# CT14 N3-SC：TreeSelect / Cascader 默认求值迁移

在既有容器N3授权范围内，沿现有feature/custom-theme执行；只迁移TreeSelect1字段、Cascader12字段的Global-only默认求值，不改模板、交互、字段或默认值。两个真实根的13字段保持名称、类型、key、ID、slot；ControlWidth在CascaderHorizontalAlignment非Stretch分支有实际消费者，不能套用Numeric的Legacy分类。

依赖既有CT04/05的ControlTokenDefaultContext、IControlTokenDefaults与生成器。只新增静态DefaultFor函数，V2 Revision1；所有函数只读context.Global，常量300/184/111/180保持。OptionPadding保留Round((ControlHeight-FontHeight)/2)的原运算顺序，颜色按有效Global及既有Dark算法解释。

1. Temporary/CustomThemeContainerSelectDefaults内先验证真实Startup descriptor，两scope各自V1应导致2个目标失败。保存明暗13字段及type/key/slot的实际Compiler旧值，显式Own/0及Control Global读取作正对照。
2. 原场景使用Window、两个根四尺寸和固定24条选择数据，保留三行Tree弹层、Cascader原列高和内容/滚动。默认/已选/禁用恢复、Global配置、Own、0/清除/弹层、公开API的同实例轨迹采集前证；A/A及1DIP检错后恢复0。
3. 改两个Token声明与13个默认函数，目标2RED→GREEN，原值/metadata严格相等。对所选轨迹Skia自然与直接绘制旧新0；任何新差异先定位，不自行增加容差或扩大既有许可。
4. 当前Mac实际NSWindow managed与NativeAOT验证公开资源与恢复，分别标明原生像素/输入边界；NET8 Release、成员审计、正式token/changelog与CSV仅13字段/2root同步。
5. 默认删除全部临时测试/驱动/媒体；清理后只验证TreeSelectControl/Cascader精确模块及diff。不得把本切片当完整家族或平台完成；不提前commit。

Review Focus：13字段全部检查；Global→Own0→改变Global→撤回Own恢复当前Global；三条常量不受Global改变；Dark预期来自有效Global；普通/配置/清除轨迹和数据保持；Cascader宽度非Stretch条件维持；标签身份与筛选输入保持原行为。
