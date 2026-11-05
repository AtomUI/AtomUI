# BorderBeam Token 设计

本文档定义 `AtomUI.Desktop.Controls.BorderBeamToken` 的 BorderBeam 专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。BorderBeam 整体架构见 [BorderBeam 桌面版架构设计](overview.md)，内部实现原理见 [BorderBeam 桌面版实现原理](implementation.md)，设计和契约变化记录见 [BorderBeam Changelog](changelog.md)。

## 1. 定位

BorderBeamToken 是 BorderBeam 的组件级设计变量层。它只承载流光装饰自身需要的默认动效、尺寸和渐变映射参数。颜色、线宽和圆角优先复用 SharedToken；motion 开关与光束数量保留为实例行为，不由 BorderBeamToken 或 `SharedToken.EnableMotion` 决定。

BorderBeamToken 服务以下主题和控件：

- `BorderBeamTheme.axaml`
- internal `BorderBeamPresenter`
- BorderBeam 渐变归一和动画默认值

BorderBeamToken 不承载 `Content`、`Color`、`ColorStops`、`Outset`、`Count`、`Progress`、`EffectiveBorderThickness`、`EffectiveCornerRadius` 等实例状态。这些状态由 BorderBeam 状态模型和边界感知接口处理。

## 2. Token 分类

BorderBeamToken 按流光语义分为三类。

### 2.1 高光尺寸 Token

- `BeamSize`
- `BeamOpacity`

`BeamSize` 控制流光高光段的基准尺寸，默认对齐 参考 CSS 实现中 `::before` 的 `width: 100`。`BeamOpacity` 控制高光层默认透明度，保持装饰效果有足够可见度但不压过内容。

### 2.2 动效 Token

- `MotionDuration`

`MotionDuration` 控制流光完成一周运动的默认时长，默认 `6s`。动效是否启用不由 BorderBeamToken 决定；默认主题保持动画启用，只有实例 `IsMotionEnabled=false` 才关闭流光。

### 2.3 渐变映射 Token

- `MaxVisibleStopPercent`

`MaxVisibleStopPercent` 控制用户 `0~100` 停靠点映射到可见高光段的最大百分比，默认 `70`。该值用于为透明尾迹保留空间，使高光尾部连续可见。

## 3. 控件专项模型中的 Token 使用

BorderBeam 的默认视觉由 SharedToken 与 BorderBeamToken 共同决定：

```text
SharedToken
  ColorPrimary
  ColorPrimaryHover
  BorderThickness
  BorderRadiusLG

BorderBeamToken
  BeamSize
  BeamOpacity
  MotionDuration
  MaxVisibleStopPercent
```

默认渐变使用 `ColorPrimary 0% -> ColorPrimaryHover MaxVisibleStopPercent% -> Transparent`。用户设置 `Color` 或 `ColorStops` 后，颜色来源切换为实例配置，但 `MaxVisibleStopPercent` 仍控制停靠点映射。

BorderBeamToken 不定义业务色组，不定义 preset color 组合，不定义 `Ocean`、`Aurora`、`Sunset` 等示例色板。示例色板属于 Gallery 示例数据，不属于控件 Token。

`Count` 控制单个 BorderBeam 实例沿同一边界绘制的光束数量，默认值为 `1`，非正值按 `1` 处理。它不描述
设计系统的默认外观尺度，也不需要主题资源覆盖，因此保持为普通 StyledProperty，不新增 `Count` Token。
“仅悬停时显示”同样通过 Style 设置现有 `IsMotionEnabled`，不新增 hover Token。

## 4. 控件家族影响

BorderBeamToken 只影响 BorderBeam 自身，不应被 Card、Button、GroupBox 或输入控件主题直接引用。

实现 `IBorderBeamAwareControl` 的控件只提供边界几何，不消费 BorderBeamToken。这样可以避免把装饰控件的主题变量反向耦合进被装饰控件。

如果某个控件希望默认展示 BorderBeam，应通过组合使用 BorderBeam 包装控件表达，而不是让该控件主题直接引用 BorderBeamToken。

## 5. 兼容性要求

BorderBeamToken 属于 BorderBeam 主题契约。即使 `BorderBeamToken` 是 internal 类型，生成的 `BorderBeamTokenKind` 和 AXAML resource 使用点也形成稳定依赖。

BorderBeamToken 变更要求：

- 不擅自重命名既有 Token。
- 不擅自删除既有 Token。
- 不改变既有 Token 的语义含义。
- 不把实例状态迁移到 Token。
- 不把 SharedToken 已经表达的颜色、线宽或圆角复制进 BorderBeamToken。
- 不把 Gallery 示例色板写入 Token。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 新增 BorderBeamToken | 检查生成的 `BorderBeamTokenKind`、AXAML 引用和默认值计算。 |
| 修改 `BeamSize` / `BeamOpacity` | 验证默认高光段可见度、内容遮挡和 light / dark 主题效果。 |
| 修改 `MotionDuration` | 验证动画周期、CPU 占用和 motion disabled 行为。 |
| 修改 `MaxVisibleStopPercent` | 验证单色、多 stop 渐变和透明尾迹连续性。 |
| 修改 `Count` 或 hover 组合 | 验证实例属性与 Style 行为，并确认 BorderBeamToken 和默认资源键没有变化。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步所有 AXAML 引用和生成文件。 |

## 有效默认计划

4字段使用 DependencyDefaultsV2，保留 BeamSize=100、BeamOpacity=0.95、MotionDuration=6秒、MaxVisibleStopPercent=70 及原 key/slot。它们到达同一实际 BorderBeamPresenter。

边框、圆角及默认渐变端点颜色从该 BorderBeam 实例的有效 Global 读取；Content/Color/ColorStops/Outset、Count 和边界感知模型保持原契约。IsMotionEnabled 继续是本控件独立的实例开关，不改为共享 EnableMotion 语义。
