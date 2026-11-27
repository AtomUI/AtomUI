# Shadcn 严格对齐实施记录

执行 [严格对齐方案](../plans/2026-11-20-shadcn-strict-alignment.md)，官方基线固定为 `f50887ae68d0fdcbfbe86b57be08af94a4c88a68`。本轮从实际 Button Gallery 截图出发，优先落地按钮外观和状态，再验证已知输入边框与选择图标差异。

## 按钮阴影与焦点

- 现有 `DefaultShadow` 是中性 Outlined、Solid 和 Dashed 的共用公开键。Shadcn 的四个按钮 owner 使用内部外观策略，Vega 中性 Outlined 取 `shadow-xs`。Solid／Dashed 基于同一公开键查显式覆盖，默认投影分别保持平贴。显式资源和 ThemeConfigProvider 值按实际资源排序解析。显式空值、等于默认的值、移除覆盖和同实例风格切换都通过。
- 成套运行时阴影断言在原实际模板从失败 17／17 变为通过 17／17。Button、DropdownButton、SubmitButton、ResetButton 均经过实际 owner 路径。
- 原 Gallery Button Shape、Type 及中性 Solid 临时场景在 1300×900 Skia 检查 8 风格 × 浅深色，64／64 阴影和 16／16 实际 Tab 焦点通过。macOS 原生 Luma、Vega 浅深色共 16／16 阴影、4／4 Tab 通过。临时测试样例的基线与复位变化为零，1 DIP 扰动变动 32 个 Skia 或 128 个 Retina 像素。
- Luma 原常态多余阴影在四个样本中产生 252／240／246／354 个外侧像素，修复后均为零。Vega 浅色的实际描边阴影为 `0 1 2 #0D000000`，外侧变化 120 个 Skia 或 395 个 Retina 像素。深色原底 RGB 10 上的模糊投影低于像素量化；用同风格的独立浅色承载面确认实际绘制。焦点已按各风格的宽度与透明度单独绘制，移出后恢复常态。
- Button 的轮廓、次要填充、无描边、链接和危险实心样式按主题规则绘制。危险实心使用 10% 浅色或 20% 深色的危险色底。链接默认跟随主色，Sera 保留常态下划线。链接种子与信息色显式同色的复现在修复前浅深色都失败，修复后通过浅色、深色和暗色 Compact，分别覆盖局部和窗口根主题设置、移除局部配置。
- 实际 Button、DropdownButton、SplitButton 和普通 Button 的 Flyout 使用按压与展开各自的状态。44 项临时断言覆盖八风格 × 双外观的 Tab、边框与悬停、按压位移、有效禁用透明度、禁用命令拦截以及焦点覆盖。最终按钮阴影和配色专项通过 24 项。
- 回归审查补测 4 项，旧实现均失败，修复后全部通过。显式 Button 和 AdornerLayer 焦点画刷保留原透明度，透明焦点在按钮加外沿 6 DIP 的区域内变化为 0 像素。指针按住时，显式前景、背景、边框可以独立生效，移除覆盖后恢复当前常态或悬停绘制。预设色继续使用原调色板的按下色阶。

## 输入边框

- Sera 的 LineEdit、TextArea 和 Select 只画下边框，保留原测量所需的四侧边框厚度。真实焦点与错误态继续绘制有色下边线。Skia 原 LineEdit 保持 428 × 44 DIP，原生 Select 保持 120 × 43 DIP。原生下边线可见，上边线取样与画布色一致。原四行 TextArea 保持 408 × 110 DIP，Gallery 滚动视口和内容范围不变。
- Luma、Rhea 的默认描边输入和选择触发器常态使用透明边框。真实 Tab、有效状态边线及应用所属作用域内的资源增删均按原 Gallery 路径复拍。保持现有 Underlined、显式边框厚度和语义部件覆盖。
- Luma Select 箭头使用 `muted-foreground`。同一实际控件切到深色时，Skia 和 macOS 原生均从 `#737373` 更新到 `#A1A1A1`。单独调整 Select 后缀的主题绑定，显式 Select Token 和后缀语义部件可覆盖该值。原 Gallery 输入／选择主矩阵通过 Skia 16／16、macOS 原生 6／6。另有 Sera 四行 TextArea 双外观 2／2。
- Native 补测在原 Select 关闭弹层后重新进入触发器，实际悬停状态从 False 变为 True，祖先覆盖的边框从 `#01A37F` 更新为 `#0156DD`，箭头覆盖为 `#E81A75`，移除后恢复透明边框和默认箭头色。局部配置的悬停目标色在首轮通过，补测的一帧命中两种颜色之间的过渡值。Skia 同一轮通过全部覆盖检查。

## 校验记录

- Button 常态投影的前后脚本使用同一套作用点和 Gallery 路径，预期取值按官方 registry 核对。
- 清理临时测试后，Core 57 项、Shadcn 164 项、桌面控件及 CompactSpace 308 项通过。定向渲染矩阵共 688 项，修正既有 Select 测试的焦点和弹层取证场景后全部通过。最后的 Select 后缀调整另复验 78 项控件测试和 5 项定向渲染测试，核对 Select 以及共享后缀入口的颜色资源。
- 合批 Gallery 构建与最终 Desktop.Controls 增量构建均通过。各批范围和未验收项见计划。
