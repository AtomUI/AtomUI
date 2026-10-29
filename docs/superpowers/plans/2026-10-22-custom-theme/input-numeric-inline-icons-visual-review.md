# NU2-A：NumericUpDown内嵌图标限定视觉审查（已批准）

产品仅将NumericUpDownSpinnerTheme.axaml四处IconWidth/Height从Shared.IconSize改为NumericUpDown有效Global.IconSize；默认14不变，Input上下箭头仍7。公开IconButton颜色身份、原方形按钮与命中区保留，没有新Own、identity、代码绑定或分隔线改动。

## 验证与配置

原四尺寸4FAIL/1正对照PASS878ms，实际加减IconWidth14而父IconSize22；候选6PASS6s，实际Icon和glyph均22、0与清除恢复成立。子IconButton的41/35尺寸配置不接管父组合几何，独立IconButton35及子颜色身份仍生效。

真实Skia620×510、缩放1；四尺寸两模式共8根，明暗同完整历史20主帧。真实按钮中心点击增减，值12往返及原格式行为保持。160条根状态/几何、320条动作命中宽高/颜色前后一致；变化只有Spinner图标宽高与glyph。

当前Mac实际NSWindow managed/arm64 NativeAOT各454项通过，官方Avalonia12.1.4，NET8零警告/错误23.53s。原生资源/模板/API检查不冒充原生最终像素或原生点击。

## 每对完整帧

| 阶段 | 自然像素 / 最大通道 | 直接绘制像素 / 最大通道 |
| --- | ---: | ---: |
| Light-normal | 0 / 0 | 0 / 0 |
| Light-aa | 0 / 0 | 0 / 0 |
| Light-nudge | 0 / 0 | 0 / 0 |
| Light-reset | 0 / 0 | 0 / 0 |
| Light-configured | 379 / 86 | 432 / 114 |
| Light-zero | 235 / 102 | 272 / 102 |
| Light-clear | 11 / 1 | 0 / 0 |
| Light-click-return | 7 / 1 | 0 / 0 |
| Light-disabled | 0 / 0 | 0 / 0 |
| Light-enabled-return | 0 / 0 | 0 / 0 |
| Dark-normal | 0 / 0 | 0 / 0 |
| Dark-aa | 0 / 0 | 0 / 0 |
| Dark-nudge | 0 / 0 | 0 / 0 |
| Dark-reset | 0 / 0 | 0 / 0 |
| Dark-configured | 389 / 79 | 432 / 105 |
| Dark-zero | 245 / 92 | 272 / 92 |
| Dark-clear | 21 / 2 | 0 / 0 |
| Dark-click-return | 10 / 2 | 0 / 0 |
| Dark-disabled | 0 / 0 | 0 / 0 |
| Dark-enabled-return | 0 / 0 | 0 / 0 |

自然Light-clear11/max1、Light-click-return7/max1、Dark-clear21/max2、Dark-click-return10/max2四对小差异单独申请接受；这四对直接绘制均0。原始计数保留，CT25的绘制历史问题开放，不设通用容差。

申请范围仅为四对22/0图标配置修复和上述四对自然差异；默认、AA、1DIP复位、禁用及最终启用恢复旧新均0。完整Numeric家族、分隔线/其他Global/Part/Custom/平台仍开放，无Git操作。

所有临时源/原生工程/81媒体已清理，audit clean；清理后原49项通过。仅作者另步自审，新增范围尚待本次批准，不构成家族或CT25完成。

2026-10-28用户独立批准call_7e038619071d40a2aab28d937b002a37:0，仅四对配置图标变化及四对自然差异，无Git/通用容差。
