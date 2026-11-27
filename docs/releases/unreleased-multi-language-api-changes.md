# Multi-language controls migration guide

For AtomUI consumers upgrading from 6.3.0. Rebuild your app and any control or design-language packages that depend on the changed APIs. [简体中文](unreleased-multi-language-api-changes.zh-CN.md) · [Changelog](../../CHANGELOG.md#unreleased)

## Interaction feedback

| Before | Use after upgrading |
| --- | --- |
| `IWaveSpiritAwareControl` | `IInteractionEffectAwareControl` |
| `WaveSpiritAwareControlProperty` | `InteractionEffectAwareControlProperty` |
| `ConfigureWaveSpiritBindingStyle` | `ConfigureInteractionEffectBindingStyle` |
| `IsWaveSpiritEnabled` in C# or AXAML | `IsInteractionEffectEnabled` |
| `EnableWaveSpirit` in a saved theme seed or Token rule | `EnableInteractionEffect` |

Use `IsInteractionEffectEnabled="False"` to disable transient feedback. Set `InteractionEffect.Effect` to `InteractionEffect.None` to disable the selected effect, or assign another effect to change its presentation. Custom templates that used `PART_WaveSpirit` can remove the part and its selectors. Its visual is now supplied by the selected effect through the public playback contract. Recompile code that referenced the retired wave shape or brush properties or `WaveSpiritType`.

## Motion durations

Use `StateTransitionDuration`, `StateTransitionDurationFast`, or `StateTransitionDurationSlow` in theme overrides for property changes such as hover and checked state. Continue to use `MotionDuration*` for component animations such as opening a popup. Rebuild compiled theme packages against the same AtomUI version as the application.
