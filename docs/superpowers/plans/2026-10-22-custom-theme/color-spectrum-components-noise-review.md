# CT19 C8：限定自然合成差异

## C8 自然合成限定差异审查表（待用户审查）

只适用于 Mac arm64、Avalonia12.1.4 Headless/Skia、scale1、1200×820、本文同一完整历史与所列图对。表中坐标为闭区间内的**完整离散坐标集合**，不是整个包围矩形；每行同时限制像素数和单通道差值。真实RGBA前后值保留在本次文字诊断记录，比较器未设遮罩或容差。每对完整新旧差异继续保留；本表不覆盖显式模式的大幅预期面板变化、其他Host或原生最终像素。

| 比较 | 图A → 图B（省略.png） | 像素数上限 | 单通道上限 | 精确坐标集合 |
| --- | --- | ---: | ---: | --- |
| AA | old-final-light-main-default → old-final-light-aa-default | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-HueSaturation → old-final-light-aa-HueSaturation | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-SaturationHue → old-final-light-aa-SaturationHue | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-HueValue → old-final-light-aa-HueValue | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-ValueHue → old-final-light-aa-ValueHue | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-SaturationValue → old-final-light-aa-SaturationValue | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-ValueSaturation → old-final-light-aa-ValueSaturation | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | old-final-light-main-reset → old-final-light-aa-reset | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | new-final-light-main-default → new-final-light-aa-default | 8 | 2 | (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) |
| AA | new-final-light-main-HueSaturation → new-final-light-aa-HueSaturation | 8 | 1 | (433,208) (434,208) (435,208) (436,208) (437,208) (433,209) (435,209) (436,209) |
| AA | new-final-light-main-SaturationHue → new-final-light-aa-SaturationHue | 8 | 1 | (433,208) (434,208) (435,208) (436,208) (437,208) (433,209) (435,209) (436,209) |
| restore | new-final-light-main-default → new-final-light-main-reset | 2 | 1 | (387,199) (918,219) |
| restore | new-final-light-aa-default → new-final-light-aa-reset | 10 | 2 | (387,199) (383,203) (396,203) (383,204) (396,204) (383,207) (396,207) (383,208) (396,208) (918,219) |
| restore | new-final-light-dip-default → new-final-light-dip-reset | 10 | 2 | (388,199) (384,203) (397,203) (384,204) (397,204) (384,207) (397,207) (384,208) (397,208) (919,219) |
| AA | new-final-dark-main-default → new-final-dark-aa-default | 15 | 1 | (384,203) (395,203) (383,205) (396,205) (383,206) (396,206) (383,208) (384,208) (395,208) (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-HueSaturation → new-final-dark-aa-HueSaturation | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-SaturationHue → new-final-dark-aa-SaturationHue | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-HueValue → new-final-dark-aa-HueValue | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-ValueHue → new-final-dark-aa-ValueHue | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-SaturationValue → new-final-dark-aa-SaturationValue | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-ValueSaturation → new-final-dark-aa-ValueSaturation | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| AA | new-final-dark-main-reset → new-final-dark-aa-reset | 6 | 1 | (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| restore | new-final-dark-main-default → new-final-dark-main-reset | 8 | 1 | (387,199) (392,199) (392,200) (387,212) (918,219) (923,219) (923,220) (918,232) |
| restore | new-final-dark-aa-default → new-final-dark-aa-reset | 17 | 1 | (387,199) (392,199) (392,200) (384,203) (395,203) (383,205) (396,205) (383,206) (396,206) (383,208) (384,208) (395,208) (387,212) (918,219) (923,219) (923,220) (918,232) |
| restore | new-final-dark-dip-default → new-final-dark-dip-reset | 17 | 1 | (388,199) (393,199) (393,200) (385,203) (396,203) (384,205) (397,205) (384,206) (397,206) (384,208) (385,208) (396,208) (388,212) (919,219) (924,219) (924,220) (919,232) |
| old-new | old-final-light-main-SaturationValue → new-final-light-main-SaturationValue | 2 | 1 | (387,199) (918,219) |
| old-new | old-final-light-main-reset → new-final-light-main-reset | 2 | 1 | (387,199) (918,219) |
| old-new | old-final-dark-main-default → new-final-dark-main-default | 15 | 1 | (384,203) (395,203) (383,205) (396,205) (383,206) (396,206) (383,208) (384,208) (395,208) (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) |
| old-new | old-final-dark-main-SaturationValue → new-final-dark-main-SaturationValue | 23 | 1 | (387,199) (392,199) (392,200) (384,203) (395,203) (383,205) (396,205) (383,206) (396,206) (383,208) (384,208) (395,208) (387,212) (918,219) (923,219) (923,220) (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) (918,232) |
| old-new | old-final-dark-main-reset → new-final-dark-main-reset | 23 | 1 | (387,199) (392,199) (392,200) (384,203) (395,203) (383,205) (396,205) (383,206) (396,206) (383,208) (384,208) (395,208) (387,212) (918,219) (923,219) (923,220) (420,225) (433,225) (420,226) (433,226) (433,227) (421,228) (918,232) |
