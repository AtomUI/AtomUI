# 选择器视觉验收及 OTP 样式核对

使用 `bugfix/shadcn` 的实际 Gallery 路由和控件。TimePicker 的矩阵在 1300 × 900 DIP、2× 的 Avalonia Headless + Skia 与 macOS 原生窗口运行。输入事件走指针、滚轮和键盘路由，原生帧按窗口编号捕获。

| 验收项 | 观察和结果 |
| --- | --- |
| TimePicker 与 RangeTimePicker 三列间距 | Luma、Nova、Vega、Maia、Lyra、Mira、Sera、Rhea × 浅深色 × 单值、范围开始、范围结束 × 小时、分钟、秒，共 144 个 Skia 列样本。Luma 浅深色在 macOS 原生另测 18 个列样本。范围选择器修复前记录六个列样本 0 像素间距，修复后矩阵全为 4 物理像素（2 DIP）。可点击行宽高保持原值，行边缘命中实际候选项。 |
| 居中 | Luma 修复前单值与范围结束端的三列，在选中及悬停时均左偏 3.75 DIP。修复后矩阵的文字布局中心距选项中心不超过 0.5 DIP，字形墨迹中心最大偏差 2.5 物理像素。浅深色代表帧均已阅。 |
| 滚轮与键盘 | 每次真实滚轮移动 50 DIP，反向准确还原原偏移和选中项位置，旁列和页面保持定位。键盘左右切列不移动视口，上下单步按各风格的 24–36 DIP 行高循环，跨循环归一后仍是单行位移。Lyra 浅色单值首次脚本预定位命中仍在移动的 Gallery 页头，独立重放用真实焦点导航定位后走原指针、滚轮与按键路径，三列通过。 |
| 取证检错 | Luma 浅色单值的原生与 Skia 弹层相同坐标下复拍差异为零，1 DIP 背景扰动均被捕获，复位后均为零差异。 |
| `Now` 与 `Today` | 实际 Gallery Basic 的 Skia 验证覆盖 TimePicker Luma 浅、深色、自定义主色和 Ant Design，以及 DatePicker Luma 浅、深色。旧 Luma 正常色为蓝色，修复后两种快捷按钮在 Luma 浅、深色分别为 `#171717`、`#E5E5E5`，悬停、按下、禁用遵循 Button 的同主题颜色。TimePicker 真实点击提交当前时间，DatePicker 点击选择系统当天并收起。 |
| DatePicker `Popup motion` | 右侧两格原本的文字带像素为 0，在原示例加入相同本地化选日期提示后，两处分别测得 1,015 个文字像素。修复前后图像差异集中于两格文字范围。两种示例都能由实际指针打开并以 Escape 关闭，第一格仍为淡入淡出，第二格继续使用显式空过渡。 |
| RangeTimePicker 弹层与输入框间距 | 在 Luma 浅色原 Gallery 场景滚到末尾（偏移 1370/1370 DIP），实际点击结束端 `12:12:20` 复现向上翻转。开始和结束端分别验证向上及向下（偏移 543/1370 DIP）。旧间距从内部文本框计算，箭头距完整框体仅 1.5 DIP。按框体上下边缘计算后，四组均约 8.5 DIP，原生可见焦点环与箭头之间为 5.5 DIP。箭头横向与当前端点对齐，原焦点、可见纵向滚动条及页面布局通过。原生与 Skia 皆是修复前 4 项失败、修复后 4 项通过。复拍与 1 DIP 扰动复位均归零。 |
| ColorPicker 第一轮边角可见性 | 录像对应 Gallery `Trigger size` 的小号带文字选色器。Luma 的色板 261 × 159 DIP、圆角 26 DIP、弹层 292 × 284 DIP。真实捕获拖拽到四个端点时，旧选圈可完全消失。单独放开裁剪后的白圈仍融入浅色弹层，用临时反差画笔在同实例上验证完整选圈可见。第一轮独立绘制层与 1 DIP 描边让选圈可见，但补充截图显示选圈会与色板分离。 |
| ColorPicker 选圈约束在色板内 | 原截图的 Basic 右上角黑色选圈约 32 × 32 物理像素，最近描边距蓝色区域约 6 物理像素。使用圆角色板向内偏移后的可用轮廓约束显示坐标，保留 1 DIP 对比描边和原裁剪。Basic 右上角选圈中心从 `(261,0)` 移到 `(247.5,13.5)` DIP，选中值保持 `#006AFF`，四角 S/V 仍可准确达到 0 或 1。实际 Gallery 的 Luma 浅色 Basic（LTR、RTL）与 Luma 深色 Trigger size 的 2× Skia 复拍中，四角和五个近边位置的每组 72 个周缘采样都在渐变内。复核了鼠标释放、未捕获移动、运行中调整圆角和触摸按住时的大选圈。触摸抬手时的 Gallery 滚动在 Headless 中干扰状态，触屏释放不计入验收。 |

## OTP 样式审查

官方 Luma 受控示例将六个等宽高槽放进同一组，组内相连且只在首尾的外侧做圆角。单槽单独成组会呈圆形。Gallery 的独立圆格来自 `OtpLineEditCellTheme` 为每格应用全局圆角并使用 `CellGap` 排列。Nova、Vega、Maia、Lyra、Mira、Rhea 同样使用相连槽位，Sera 使用有间距的下划线槽位。 [官方 Luma 样式](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/styles/style-luma.css#L351-L369) · [官方六位受控示例](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/examples/base/input-otp-controlled.tsx#L16-L29) · [官方 Sera 定义](https://ui.shadcn.com/r/styles/base-sera/input-otp.json)

用户确认对齐 Luma 后，默认单组槽位相连，首尾只保留外侧圆角，中间共用单条边线。原 LineEdit `Custom Semantic Part styling` 的 32 × 36 DIP 格子间各有一个星号，实测间隔 13 DIP。移除示例的固定宽度与逐格星号后，常规 Luma 复拍为 36 × 36 DIP，DarkCompact 为 32 × 32 DIP，间距为 0，五条内部蓝色边线各为 2 物理像素（1 DIP）。原蓝色语义边框仍生效，输入焦点环绘制在相邻格之上。键盘输入、禁用、填充、下划线变体、显式分隔符恢复与应用自定义宽度设定及清空均已复核。

清理后定向测试通过 Popup 与 TimePicker 76 项、DatePicker 153 项、Gallery 3 项、选择器 Skia 主题效果 17 项。ColorPicker 选圈约束后的定向测试通过桌面控件 28 项、渲染 185 项，原 Gallery 另有 3 项脚本场景通过。OTP 连排修复后的定向测试通过桌面控件 67 项、Gallery 1 项、Skia 主题效果与身份 11 项；原 Gallery 浅色、深色紧凑及连排 Token 效果另有 3 项脚本通过。临时媒体、驱动和测试已清理，Git 取证审计无发现。
