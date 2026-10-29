# Cascader 高度限定视觉审查

2026-10-28。用户已确定“Own为上限、页数预算可进一步缩小”的合同；本表的具体像素与自然列宽变化仍待明确审查，不把合同选择当作视觉批准。

范围：官方Avalonia12.1.4，macOS arm64，600×740 Skia Window，单选/Outlined、四尺寸、明暗、原24条叶选项及原标题/宽300。相同完整操作历史：初始→Global.ControlHeight46→Own.DropdownHeight208→Global52→DisplayPageSize3→Own0→Global54→清除→重开。自然合成器与直接绘制各78对，70对变化、8对0；表内含同状态的A/A/检错控制帧，不表示70个独立功能。

实际高度：默认/清除/重开320→180；Global46时460→180；热Global52时520→208；页数预算156保持156；Own0后再热更新时162→0。零上限保持业务popup open与蓝色边框，但内容不显示，和原来直接配置0的路径一致。Root位置、宽300、24条数据、尺寸、Own值和页数78条记录一致。

**自然列宽193→186也属于此次申请的具体差异。** 单变量取证只改上限：180时实现标签0–7、宽186；改320后实现0–13、宽193；回180后保留193。字体14、根宽300、24条同数据保持，说明这是既有虚拟化测量历史随上限变化的结果。本包没有另设列宽缓存或全源预测量；不把列宽变化隐去或当作已获批准。其他数据与列宽策略未由本表批准。

最终方案在同一个Frame的ControlTheme Setter内取预算与Own的较小值，保留专用CascaderPopupRootStyle优先级。先前把预算放到外层Popup的候选已否决：原生总额度156使内容变144。本表对应修正后的同Frame方案，managed/AOT两类真实后端均保留156。

作者已查看初始前后、暗色Own、较小页数、0及恢复关键画面：标题和输入区域保留；默认弹层变短，180不是整行高度倍数，末行可以部分可见并按滚动区域裁剪；24条数据可滚动选择。四尺寸实际左5%/中50%/右90%悬停、wheel事件与Offset100、点击原对象2并关闭均通过。普通滚动条保留原auto-hide，不声称一直可见。Native检查是公开API/几何，不是原生像素或raw输入。

| 场景 | 自然像素/最大通道差 | 直接像素/最大通道差 |
| --- | ---: | ---: |
| Light-aa | 28943/224 | 28943/224 |
| Light-nudge | 28943/224 | 28943/224 |
| Light-reset | 28943/224 | 28943/224 |
| Light-Large-initial | 28943/224 | 28943/224 |
| Light-Large-global | 37495/224 | 37495/224 |
| Light-Large-own | 7548/34 | 7548/34 |
| Light-Large-hot-own | 40113/224 | 40113/224 |
| Light-Large-page3 | 5988/34 | 5988/34 |
| Light-Large-hot-zero | 19913/224 | 19913/224 |
| Light-Large-clear | 28943/224 | 28943/224 |
| Light-Large-reopen | 28943/224 | 28943/224 |
| Light-Middle-initial | 28943/224 | 28943/224 |
| Light-Middle-global | 37495/224 | 37495/224 |
| Light-Middle-own | 7548/34 | 7548/34 |
| Light-Middle-hot-own | 40113/224 | 40113/224 |
| Light-Middle-page3 | 5988/34 | 5988/34 |
| Light-Middle-hot-zero | 19913/224 | 19913/224 |
| Light-Middle-clear | 28943/224 | 28943/224 |
| Light-Middle-reopen | 28943/224 | 28943/224 |
| Light-Small-initial | 28943/224 | 28943/224 |
| Light-Small-global | 37495/224 | 37495/224 |
| Light-Small-own | 7548/34 | 7548/34 |
| Light-Small-hot-own | 40113/224 | 40113/224 |
| Light-Small-page3 | 5988/34 | 5988/34 |
| Light-Small-hot-zero | 19913/224 | 19913/224 |
| Light-Small-clear | 28943/224 | 28943/224 |
| Light-Small-reopen | 28943/224 | 28943/224 |
| Light-Custom-initial | 28943/224 | 28943/224 |
| Light-Custom-global | 37495/224 | 37495/224 |
| Light-Custom-own | 7548/34 | 7548/34 |
| Light-Custom-hot-own | 40113/224 | 40113/224 |
| Light-Custom-page3 | 5988/34 | 5988/34 |
| Light-Custom-hot-zero | 19913/224 | 19913/224 |
| Light-Custom-clear | 28943/224 | 28943/224 |
| Light-Custom-reopen | 28943/224 | 28943/224 |
| Dark-aa | 37338/204 | 37338/204 |
| Dark-nudge | 37338/204 | 37338/204 |
| Dark-reset | 37338/204 | 37338/204 |
| Dark-Large-initial | 37338/204 | 37338/204 |
| Dark-Large-global | 67441/200 | 67438/200 |
| Dark-Large-own | 3955/15 | 3955/15 |
| Dark-Large-hot-own | 74822/200 | 74822/200 |
| Dark-Large-page3 | 3019/15 | 3019/15 |
| Dark-Large-hot-zero | 37934/200 | 37934/200 |
| Dark-Large-clear | 37338/204 | 37338/204 |
| Dark-Large-reopen | 37338/204 | 37338/204 |
| Dark-Middle-initial | 37338/204 | 37338/204 |
| Dark-Middle-global | 67438/200 | 67438/200 |
| Dark-Middle-own | 3955/15 | 3955/15 |
| Dark-Middle-hot-own | 74822/200 | 74822/200 |
| Dark-Middle-page3 | 3019/15 | 3019/15 |
| Dark-Middle-hot-zero | 37933/200 | 37933/200 |
| Dark-Middle-clear | 37338/204 | 37338/204 |
| Dark-Middle-reopen | 37338/204 | 37338/204 |
| Dark-Small-initial | 37338/204 | 37338/204 |
| Dark-Small-global | 67439/200 | 67438/200 |
| Dark-Small-own | 3955/15 | 3955/15 |
| Dark-Small-hot-own | 74822/200 | 74822/200 |
| Dark-Small-page3 | 3019/15 | 3019/15 |
| Dark-Small-hot-zero | 37933/200 | 37933/200 |
| Dark-Small-clear | 37338/204 | 37338/204 |
| Dark-Small-reopen | 37338/204 | 37338/204 |
| Dark-Custom-initial | 37338/204 | 37338/204 |
| Dark-Custom-global | 67438/200 | 67438/200 |
| Dark-Custom-own | 3955/15 | 3955/15 |
| Dark-Custom-hot-own | 74822/200 | 74822/200 |
| Dark-Custom-page3 | 3019/15 | 3019/15 |
| Dark-Custom-hot-zero | 37933/200 | 37933/200 |
| Dark-Custom-clear | 37338/204 | 37338/204 |
| Dark-Custom-reopen | 37338/204 | 37338/204 |

两个后端的Dark-Large-global计数67441/67438、Dark-Small-global67439/67438分别保留，不建立通用容差。原版整Panel移动复位另有3像素、最大差1的既有边缘失败，坐标(26,82)/(24,83)/(28,85)保留CT25；本表完整主轨迹改用标题1DIP作比较器检错，两侧一致，全部A/A与标题复位0。没有据此宣称原整Panel往返问题已修复。

未覆盖：Multiple/多级列/分组的完整视觉、其他variant/字体/尺寸配置、原生最终像素与raw输入、跨平台和持续稳定性专项。仅本表场景与本地输入/几何，不关闭完整家族。

审查状态：待用户确认上述高度与自然列宽限定范围；不含Git、发布或通用容差。媒体查看、文字记录后清理，不提供失效媒体链接。
