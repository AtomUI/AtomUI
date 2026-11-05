# Cascader DropdownHeight 与行数预算

用户已选定合同（call_a7943c5ed4b2434eb0dbc848460a14c5:0）：Own.DropdownHeight 为上限，DisplayPageSize 预算可进一步缩小。此为下一消费者分包；N3-SC13默认协议已完成，不重做。新的具体视觉变化仍另审。

AC1：24条原选项、标题、宽度、原Overlay弹层和高度约束保持，实际最大高度不超过min(Own上限,有效行高×页数+popup padding)，内容不足时自然收缩。默认/Own208/96/0、Global46→52、页数3→10、清除/关闭重开均走同实例。初始化实际值先取证，320只代表基础公式，不能据此报告默认像素会改变。

AC2：使用真实Frame/Popup/ScrollViewer/viewport/extent/裁剪链确认两个约束的空间归属，先记录完整原版轨迹，再改主题。优先让现有Popup承载总预算、既有Frame承载Own上限，消除根DirectProperty同槽双写；若该方式不能等价实现约束，再据运行证据调整，不增加标志/延迟/缓存或无必要wrapper。

AC3：所有可滚动正高度场景保留原24条数据及选中对象、可见viewport与可用滚动条，框架输入验证行左/中/右hover、inner wheel与点击选择。没有原Nested外层滚动时不虚构已验链式滚动；Native与Overlay分开记录。

AC4：生成CascaderPopupRootStyle在AXAML覆盖/撤回高度并命中真实Frame，保留原生样式优先级，不通过code-behind定制节点。测试同样断言专用Style类与关键Setter，无Loaded/Name回退。

AC5：明暗四尺寸、原状态/数据/操作历史的自然Skia与直接绘制前后逐对比较，AA/1DIP恢复，查看后保留数值再清理。默认是否变化以实测为准；已失效配置修复的精确变化另交审查。当前Mac实际NSWindow managed/NativeAOT验证高度与恢复，原生像素/输入只在真实驱动证据支持时报告；NET8与清理后Cascader精确验证。

临时位置Temporary/CustomThemeCascaderHeight；无新公开API/Token/Part，默认删除全部验证代码/外部工程/媒体。先有效RED，再完整原版视觉，再最小主题修复；单agent、无分支/worktree/提交。
