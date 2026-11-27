# 多设计语言控件迁移指南

适用于从 AtomUI 6.3.0 升级的应用。请重新编译应用以及使用变更 API 的控件包和设计语言包。[English](unreleased-multi-language-api-changes.md) · [变更日志](../../CHANGELOG.zh-CN.md#unreleased)

## 交互反馈

| 旧用法 | 升级后 |
| --- | --- |
| `IWaveSpiritAwareControl` | `IInteractionEffectAwareControl` |
| `WaveSpiritAwareControlProperty` | `InteractionEffectAwareControlProperty` |
| `ConfigureWaveSpiritBindingStyle` | `ConfigureInteractionEffectBindingStyle` |
| C# 或 AXAML 中的 `IsWaveSpiritEnabled` | `IsInteractionEffectEnabled` |
| 已保存主题种子或 Token 规则中的 `EnableWaveSpirit` | `EnableInteractionEffect` |

用 `IsInteractionEffectEnabled="False"` 关闭瞬态反馈。将 `InteractionEffect.Effect` 设为 `InteractionEffect.None` 可关闭当前效果，也可赋予另一种效果来替换呈现。自定义模板可删除 `PART_WaveSpirit` 和对应选择器，由所选效果通过公开播放契约创建视觉。引用旧波浪画刷、形状属性或 `WaveSpiritType` 的代码需要迁移并重新编译。

## 动效时长

覆盖悬停、勾选等属性过渡时，使用 `StateTransitionDuration`、`StateTransitionDurationFast` 或 `StateTransitionDurationSlow`。弹层打开等组件动效继续使用 `MotionDuration*`。请用与应用相同的 AtomUI 版本重建编译主题包。
