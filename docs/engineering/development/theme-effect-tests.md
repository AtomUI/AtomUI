# 主题生效测试规范

本文定义"生效测试"：证明承诺面中的每个 Token 设置后在渲染层产生可观测变化。承诺面的定义见
[Control Token 规范](control-token-guidelines.md#own-token-承诺面)；编译与资源语义见
[主题系统架构](../../architecture/systems/theming/runtime.md)。

## 1. 目的与边界

生效测试回答一个问题："这个 Token 改了，控件看起来变了没有"。它是控件家族迁移主题读法的门禁，也是 Own Token
Active 分类的证据。

它不是像素精确对照。像素对照沿用既有的关键区域容差策略，只用于视觉回归；生效测试只断言"有变化"。

## 2. 测试矩阵

每个有 Token 身份的 Control 覆盖两组 Token：

| 组 | 来源 | 覆盖方式 |
| --- | --- | --- |
| Active Own Token | 该 Control 的 Own Token schema 去掉 `[ReservedToken]` 字段 | `ThemeConfig.Controls[identity].Tokens[字段]` |
| 消费的 Global Token | 生成器消费图里该 Control 的主题与代码消费的全局 Token | `ThemeConfig.Controls[identity].Tokens[全局名]`，另抽样一组用根 `Tokens[全局名]` 核对 |

两组的并集就是该 Control 的承诺面。消费图里没有、Own 里也没有的 Token 不测。

## 3. 单个用例的形态

```text
arrange  以基线 ThemeConfig 渲染控件到指定状态，捕获控件边界内的帧 A
act      只把目标 Token 改为明显不同的值，重新渲染，捕获帧 B
assert   A 与 B 在控件边界内的像素差异比例 > 阈值
```

扰动值按类型固定：

| Token 类型 | 扰动 |
| --- | --- |
| Color / Brush | 取反色；原值接近中灰时取纯色 |
| double（尺寸、字号、圆角） | 加倍；原值为 0 时取 8 |
| Thickness / CornerRadius | 每个分量加 4 |
| TimeSpan（动效时长） | 不做像素断言，改为断言动效系统读到的时长值变化 |
| bool（EnableMotion、EnableWaveSpirit 等） | 取反；断言对应行为开关 |
| FontWeight | 低于 SemiBold 取 Bold，否则取 Normal |
| BoxShadows | 偏移加 8、模糊加 8 |

阈值默认是控件边界内 0.5% 的像素发生通道差异大于 8；细线类 Token（`LineWidth`、边框线宽）阈值降到 0.1%。
差异为零即"设置了没效果"，用例失败。
BoxShadows 类 Token 画在控件边界之外，比较区域是控件边界向四周扩展基线与扰动后阴影集合的最大外延（逐轴取各阴影 |偏移| + 模糊 + 扩展的最大值）并裁剪到窗口后的区域，比例分母取该扩展区域的面积。

## 4. 验收场景与控件状态

控件家族迁移时，从承诺面枚举全部 Active Own Token 与消费的 Global Token，复用同一临时场景驱动逐项扰动和比较。
已迁移控件的后续改动覆盖受影响的 Token 与状态。

每个 Token 使用实际消费它的控件状态。例如 `DefaultHoverColor` 使用 `:pointerover`，
`ColorErrorBg` 使用 `Status="Error"`，Popup 内的 Token 在打开 Popup 后验证。默认状态可见的效果直接使用默认状态。

一个 Token 在任何状态下都看不见时，它不是 Active：要么改为 Reserved 并写明理由，要么删除。生效测试不接受"跳过"。

按实际绘制路径选择捕获对象、扰动值和比较区域，并在验收记录中说明使用的条件。

- 成员只存在于宿主内部（OptionButton 在 OptionButtonGroup 里、FloatButton 由 Host 放进浮层）时，从显示后的根节点
  选出被测成员。
- 响应单向的 Token 使用显式扰动值，例如行高只在变小时抬高被钳制为 0 的垂直内边距。
- Token 移动的内容画在控件边界之外（阴影偏移、浮层里的按钮或菜单）时，扩大或改用比较区域。

弹层间距类 Token（`MarginToAnchor`、`GutterToFlyout`）改为断言打开的弹层相对锚点的位移等于间距变化；整数 Token（`ZIndexPopupBase`）
断言对应属性等于覆盖值。

## 5. 位置与执行

- 通用控件状态可用 [ControlBaseline 状态截图工具](../../../tools/performances/README.md#性能改动的视觉与泄漏守卫)取证。
  精确的 Token 扰动可在临时驱动中使用 Avalonia 的 `CaptureRenderedFrame()`。需要现有测试宿主时，放在所属测试项目的
  `Temporary/<任务标识>/` 下。
- 用例按控件家族分组。家族迁移验收须通过该家族的承诺面用例。修改已迁移控件的 Token、主题或代码侧视觉计算时，
  验证受影响的 Token 和状态。
- 验收后删除专用用例、状态 setup、基线、截图和其他临时产物。保留简短结论，按
  [单元测试与验收规范](test-value-and-lifecycle.md#6-存量测试清理)分批迁移存量用例与选测映射。

## 6. 与借鉴修复的关系

从其他分支借鉴的每一项"让主题设置生效"的修复都附一个可失败的生效测试：修复前该 Token 的用例应失败，修复后通过。
不接受只附说明没有测试的借鉴。
