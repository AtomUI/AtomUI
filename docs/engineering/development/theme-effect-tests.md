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

## 4. 骨架生成与作者补充

生成器从承诺面为每个 Control 产出用例骨架：一个数据驱动的 `[Theory]`，枚举该 Control 的全部 Active Own Token 与
消费的 Global Token，调用统一的 perturb-and-compare 辅助。

控件作者只补一件事：把控件摆到能看见这个 Token 的状态。例如 `DefaultHoverColor` 需要 `:pointerover`，
`ColorErrorBg` 需要 `Status="Error"`，Popup 内的 Token 需要打开 Popup。作者在骨架旁边的 partial 里按 Token
提供状态 setup；没有 setup 的 Token 使用控件默认状态。

一个 Token 在任何状态下都看不见时，它不是 Active：要么改为 Reserved 并写明理由，要么删除。生效测试不接受"跳过"。

状态 setup 之外，辅助方法的 `EffectOptions` 提供三种调整，均需在用例旁说明原因：

- `Target`：成员只存在于宿主内部（OptionButton 在 OptionButtonGroup 里、FloatButton 由 Host 放进浮层）时，从显示后的根节点
  选出被测成员。
- `Perturb`：响应单向的 Token 改用显式扰动值，例如行高只在变小时抬高被钳制为 0 的垂直内边距。
- `RegionInflation` / `Region`：Token 移动的内容画在控件边界之外（阴影偏移、浮层里的按钮或菜单）时，扩大或改用比较区域。

弹层间距类 Token（`MarginToAnchor`、`GutterToFlyout`）改为断言打开的弹层相对锚点的位移等于间距变化；整数 Token（`ZIndexPopupBase`）
断言对应属性等于覆盖值。

## 5. 位置与执行

- 测试建在 `tests/AtomUI.Desktop.Controls.Rendering.Tests` 的 compositor 帧捕获之上；DataGrid、ColorPicker、Extras
  等可选包在各自测试项目中复用同一辅助。
- 用例按控件家族分组，家族迁移完成的门禁是该家族全绿。
- 它们是常驻测试，不是临时测试；准入与清理遵守[测试价值与生命周期规范](test-value-and-lifecycle.md)。
- 全量回归包含生效测试；按改动影响选择验证时，修改某 Control 的 Token、主题或代码侧视觉计算必须运行该 Control
  的生效测试。

## 6. 与借鉴修复的关系

从其他分支借鉴的每一项"让主题设置生效"的修复都附一个可失败的生效测试：修复前该 Token 的用例应失败，修复后通过。
不接受只附说明没有测试的借鉴。
