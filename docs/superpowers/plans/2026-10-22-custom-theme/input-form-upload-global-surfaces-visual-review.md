# CT13 FU2：六公开表面限定视觉审查

2026-10-30，用户明确回复“批准 FU2 本表限定范围，保留 CT25”（call_137bdbb52074425385b7e76e4759ef93:1）。本表限定范围已批准，原始CT25调查保持；不批准完整CT13、字体/内部其他资产、其他平台、Git或通用容差。

六公开identity：Form、FormItem、FormItemDecorator、Upload、UploadDefaultDropArea、UploadTrigger。原1130×780、Account/Alice/Wrapper/两个文件、720宽DropArea与card触发器保持。现有Global按各自identity生效，不跨公共子控件借用。显式配方包含SpacingXS17/XXS13、ControlHeight47、IconSize23、BorderRadiusLG0、BorderThickness3及对应明暗品牌颜色；正常动效保留。

| 场景 | 自然变化像素 / 最大通道差 | 直接变化像素 / 最大通道差 |
| --- | --- | --- |
| Light configured | 153395 / 233 | 153438 / 233 |
| Light disabled | 161011 / 233 | 161054 / 233 |
| Dark configured | 153394 / 221 | 153437 / 221 |
| Dark disabled | 161011 / 221 | 161051 / 221 |

9对自然默认/AA/1DIP/复位/恢复等0；另Dark cleared为2点、最大通道差1，严格0原失败保留CT25。直接10非配置对均0。各版AA/检错扰动复位为0且1DIP检出，以上计数不是允许误差。

Decorator Extra的局部Dark继承低对比在原/新均存在，不在本表修复或批准。已查看具体配置/禁用图；15专项、3专用Style四上传模式命中/撤回、managed/AOT各81及NET8零通过，最终清理和模块结果见[结果](input-form-upload-global-surfaces-results.md)。

批准仅四配置场景、表列布局/表面变化及Dark cleared的2点限定边缘；不关闭CT25、其余字体/状态/内部消费者或整个CT13。
