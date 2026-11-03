# CT13 CB5：箭头与公开候选项圆角限定视觉审查

2026-10-29，用户明确回复“批准 CB5 本表限定范围”（call_88b527f2f3684a8291fac5b5e63c4014:0）。本表24个配置场景及所述自动布局变化已获批准，不扩大范围。

范围：当前macOS arm64/Avalonia12.1.4，600×520产品Window，明/暗局部Provider（Window外部仍为亮色背景）及四尺寸，原8项/三行popup、选中0→真实点击1→恢复0。相同完整历史含标题AA/1DIP/严格复位、真实箭头hover/pressed/open、配置、点击候选项、配置pressed、清除。正常motion保持开启，两个后端各72对、总288张；没有裁剪、重对齐或通用容差。

配置：ComboBox有效Global.IconSizeSM=23、ColorPrimaryActive=#176d93；独立ComboBoxItem有效Global.BorderRadiusSM=0。显式配置时箭头12→23，选中项绘制圆角4→0；未配置仍为12与4，按下色正确消费指定颜色。Root宽300、8条数据与三行内容保持；清除全部恢复默认。

**本表包含自动高度的具体变化。** Middle/Custom原生观测自动高度32→33；其中Middle单变量实验仅切换IconSizeSM 12→23→12，其余值及item圆角0保持：输入高度32→33→32；popup锚点Y76→77→76，popup尺寸300×104、viewport292×96、extent292×256保持。显式根Height=32仍保持32。不能把打开画面较多变化像素隐藏成仅改箭头；不通过额外Height限制消除预期glyph自然测量。

| Theme/size/stage | Natural pixels/max channel | Direct pixels/max channel |
| --- | ---: | ---: |
| light/Large-configured-open | 125/233 | 143/233 |
| light/Large-selected-configured | 97/114 | 115/114 |
| light/Large-pressed-configured | 97/232 | 117/232 |
| light/Middle-configured-open | 13223/233 | 13242/233 |
| light/Middle-selected-configured | 717/233 | 738/233 |
| light/Middle-pressed-configured | 718/233 | 738/233 |
| light/Small-configured-open | 125/233 | 143/233 |
| light/Small-selected-configured | 97/114 | 115/114 |
| light/Small-pressed-configured | 97/232 | 117/232 |
| light/Custom-configured-open | 13223/233 | 13242/233 |
| light/Custom-selected-configured | 717/233 | 738/233 |
| light/Custom-pressed-configured | 718/233 | 738/233 |
| dark/Large-configured-open | 149/200 | 167/200 |
| dark/Large-selected-configured | 97/105 | 115/105 |
| dark/Large-pressed-configured | 97/143 | 117/143 |
| dark/Middle-configured-open | 12107/228 | 12128/228 |
| dark/Middle-selected-configured | 717/233 | 738/233 |
| dark/Middle-pressed-configured | 718/233 | 738/233 |
| dark/Small-configured-open | 149/200 | 167/200 |
| dark/Small-selected-configured | 97/105 | 115/105 |
| dark/Small-pressed-configured | 97/143 | 117/143 |
| dark/Custom-configured-open | 12107/228 | 12128/228 |
| dark/Custom-selected-configured | 717/233 | 738/233 |
| dark/Custom-pressed-configured | 718/233 | 738/233 |

每后端72对中48对默认、hover、pressed、open、清除及检错控制画面均0；24对仅显式配置阶段变化。各侧标题1DIP均检出1857像素，AA与复位严格0。两后端配置的边缘计数不同，分别保留，不用其中一份替代另一份。

作者已查看明色Middle打开前后：箭头明显增大，选中项圆角变直，输入与弹层位置按上述自然高度变化；原候选文字、三行viewport和宽度保持。作者也已查看暗色局部Provider的Middle打开前后，变化与明色一致；该场景不是整窗应用算法切为Dark的验证。当前Mac managed/NativeAOT各355、8项专项、NET8零警告19.32s通过；4临时源/427工程文件/289媒体均清理，最终精确ComboBox69项通过2s。

批准范围仅本表24个配置场景及所列自动布局变化，不包括其他字体/数据/尺寸参数、其他平台、完整CT13或Git。
