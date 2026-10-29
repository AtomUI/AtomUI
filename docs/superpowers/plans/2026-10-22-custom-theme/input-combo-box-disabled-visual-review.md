# CT13 CB2：ComboBox 禁用文字色精确视觉审查

唯一产品改动为ComboBoxItemTheme的disabled Foreground从Shared ColorTextDisabled接到已有Own ItemDisabledColor。默认公式、条件、优先级、身份、输入和布局保持；HandleHoverColor已有正确模板绑定，实际pointer/按钮/Icon画刷正对照通过，没有修改手柄。

当前Mac、Skia scale1、400×220、实际明暗scope，三条目、两行高度限制、Overlay popup；默认closed→真实handle hover→已有hover Own改色/清除→open→AA→1DIP→复位→禁用Own改色/清除→Down/Enter跳过disabled项→关闭。正常动效、原尺寸、焦点和完整图像坐标不变，无裁剪、遮罩、对齐或容差。

| 对照 | Light | Dark |
| --- | ---: | ---: |
| 原→候选：所有阶段，除disabled-configured | 各0 | 各0 |
| 原→候选：ItemDisabledColor=#d02088 | 402，最大通道160 | 402，最大通道122 |
| 原/候选AA、扰动复位、禁用色及hover色清除恢复 | 全部0 | 全部0 |
| 1DIP检错，原/候选 | 各4870 | 各4900 |

差异严格限定为禁用条目“Disabled”的实际文字，x41–93、y105–116；启用条目、指示器、边框、Popup布局和背景均0。明暗完整图已查看；这是原先忽略的已导出Own颜色开始绘制，测试颜色不是默认主题更改。

原版读取资源为#d02088，实际项Foreground却为#3f000000，反例失败；修复后项与真实TextBlock前景一致，三项临时检查通过。当前Mac产品Window managed42项通过，覆盖透明Own、动态换色/清除、有效Global默认输入、邻项/选择、重挂和正常关闭；AOT及最终清理验证继续。

用户已明确接受上述显式禁用文字色生效（明暗各402点，call_d39024a52b3945d9a80de9809e27cb9d:0），不含新自然合成噪声例外、Git、完整家族或CT25完成。颜色本包两临时源、49媒体文件及原生工程已清理，原ComboBox69项通过；其后专用Style优先级验收发现独立路由缺陷，已按[CB3计划](input-combo-box-item-route.md)继续，不用代码设置前景绕过。原生最终像素及其他Host仍归完整任务。
