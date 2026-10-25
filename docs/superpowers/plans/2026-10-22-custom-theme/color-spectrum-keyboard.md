# CT19 C9：色谱键盘短设计（已获批准）

2026-10-25用户明确回复“批准 C9 推荐短设计”。批准覆盖本文交互合同，不包含Git提交。

目的：接通两类公开 View 内既有 ColorSpectrum 的自然键盘路径，按当前实际二维轴移动选择器；沿当前 checkout 和分支，不创建分支／worktree，不提交或发布包。

## 来源与范围

当前 ColorSpectrum 的默认 Focusable=false，指针处理中的 Focus() 不能成功；已有四方向键、Ctrl、边界 wrap 逻辑因此无法自然使用。六模式接线完成后，还须核对 ValueSaturation 的 Y 轴 Saturation 等方向与实际选择器几何；静态怀疑不能代替临时 RED。其 ControlTheme 中两个 FocusEllipse selector 无实际目标节点。上述事实与滑轨已获焦点却没有键盘处理是两个独立合同，本切片只处理色谱。

公开签名、Token、资源 key、Semantic PART 与 C8 的 SV 默认不变。交互属于 L2：色谱将成为实际 Tab 停靠点，指针点击会获取焦点；须单独获得用户审查，C8 的默认值批准不覆盖本项。

## 推荐方案

1. 在现有 ColorSpectrum ControlTheme 声明 Focusable=true，保留应用的本地／样式覆盖能力。自然 Tab／Shift+Tab 按现有视觉树顺序进入和离开色谱；禁用或隐藏的色谱不参与，不改其他节点 TabIndex 或默认焦点。
2. 复用现有 AdornerLayer 默认焦点框及其 FocusVisualBorderThickness／ColorFocusBorder／FocusVisualMargin；不新增模板环、Token或公开Part。移除不存在的 FocusEllipse 节点选择器。键盘聚焦的可见结果须实际采样验证，不能因有默认 adorner 资源就直接报告焦点可见。
3. 保留既有四方向键与 Ctrl 大步长、Hue／Saturation／Value 范围及 wrap 规则。按六模式当前实际像素／选择器坐标确定方向；RTL 仅反转水平轴。仅在有效临时反例证明现有映射错误时修正；颜色 Alpha 和未操作分量保持，事件不重复。非方向键继续原路由，不新增 Home／End 或 Page 键功能。
4. 复用现有指针 Focus()，不向驱动强设内部 Focusable，不使用直接 RaiseEvent 绕过真实焦点／输入。保留原控件尺寸、裁剪、滑轨、调色板、输入、渐变stop及动效；配置和键盘操作停止后同实例保持稳定。

选择复用默认焦点框，是因为项目已有完整 AdornerLayer 主题与 Token。补建专用 FocusEllipse 需要额外尺寸／颜色合同，对恢复原生键盘路径没有必要。保持 Focusable=false 会继续让公开 View 内既有键盘路径不可达，不满足本切片目标。

## 验证与交付门槛

- 在所属 Desktop 项目 Temporary 目录建立 before／after：自然 Tab／Shift+Tab、真实指针获取焦点、两类 View × 六模式 × 四方向／RTL，事件到达与 HSV／Alpha；明确区分焦点失败和方向错误。
- 验证 Ctrl／范围边界／退化范围、禁用与恢复、模板替换／离树重挂，以及输入框和邻接控件继续按原路由使用；不扩大到滑轨键盘或渐变stop导航。
- 实际 Light／Dark、正常动效的默认未聚焦、键盘聚焦、操作后与移出焦点同轨迹比较，保留 A/A 和 1 DIP 正对照；任何新增有限差异单列真实证据审查，不继承 C8 容差。
- 真实 Mac managed／NativeAOT 输入检查；临时源、驱动和媒体按规范清理后，只运行 ColorPicker 精确模块、NET8 Release、成员审计及 diff check。

这份短设计只申请上述 L2 交互合同，不代表实施或验收已通过，不包含 Git 授权或完整 CT19／CT25 验收。
