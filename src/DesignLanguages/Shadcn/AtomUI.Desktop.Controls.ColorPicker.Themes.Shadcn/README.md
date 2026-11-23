# AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn

`AtomUI.Desktop.Controls.ColorPicker` 在 shadcn 设计语言下的取值。对标 shadcn/ui 现行版本（registry 2026-11-15
实测）、Tailwind CSS v4.3.5；语言见 `../AtomUI.DesignLanguages.Shadcn/README.md`。

```csharp
builder.UseDesktopColorPicker();
builder.UseShadcn(language => language.AddDesktopColorPicker());
```

## 取值的由来

起点是 Ant Design 取值包的同名推导（公式复制而来，此后归本语言所有）。shadcn/ui 没有取色器组件，迁移前的 shadcn
语言包也没有控件阶段：取值全部来自 Ant 公式，颜色与几何随 shadcn 的全局 Token 变化。

## 不支持 / 近似

| 项 | 说明 |
| --- | --- |
| 取色器 | 官方无对应组件，整体为 B 协调 |

## 控件目标等级（方案 11.1）

A 对标：官方有对应组件，按视觉验收口径对官方实测；B 协调：官方无对应组件，用语言 Token 与全局 Token 组合取值，
关键状态人工评审。「取值来源」一列标出推导是否在 Ant 公式之上并入了 shadcn 的取值。

| 控件 Token | 目标等级 | 对标的官方组件 | 取值来源 |
| --- | --- | --- | --- |
| `ColorPickerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `ColorPickerViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `GradientColorPickerToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
| `GradientColorPickerViewToken` | B 协调 | — | Ant 公式（经 shadcn 全局 Token 着色） |
