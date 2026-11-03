# CT15 ToolTip/PopupConfirm 原生交互出口

复用既有3metadata/13Own、84+8用例、68对批准和字体/Part/资源桥/GC证据，仅补真实macOS native popup与Overlay的hover/点击/关闭/键盘和已打开scope更新。旧颜色恢复/Compact微差及FW1/FW2界限不改。

960×740 owner；Tooltip保留默认show delay400ms、原长文、120高ScrollViewer/900extent；PopupConfirm保留原Title/ConfirmContent/两按钮和140高长文。应用内Cocoa事件按真实AvnView坐标映射目标，不操作外部物理鼠标/系统UI，不用CUA。分别原生popup/overlay，读真实TopLevel/native handle及当前有效字体；hover打开/离开关闭、实际Cancel/Confirm各一次、Escape light dismiss与原内容/视口、打开时Global23→25同实例更新。只读查询运行节点，所有配置走公开owner API，不取Part设外观。

先有效反例后根因修复；正常motion/箭头/原尺寸/内容保持。取证仅系统临时目录，清理后精确本模块；同源AOT与原生截图结果按实际记录，不借既有Headless结果填通过。


## 实际结果与限制

首native hover未打开，未获得有效popup，按FAIL/证据不足保留；只加实际命中、pointer entered/exited、opening/IsOpen与Raw input观测后，默认showDelay400/serviceEnabled true/custom false、Raw根等于owner presentation source，hover正常打开。该首失败未被证明原因，不把日志/等待当产品修复，没有修改产品代码。

有效同源managed/AOT最终各75检查0FAIL、exit0。两种native popup/Overlay实际Root均符合请求，真实字体Menlo23→打开时25同实例更新；Tooltip120viewport900extent与Confirm140viewport900extent保持，实际Cancel/Confirm各一次，Escape light dismiss不重复产生Confirm/Cancel，离开关闭Tooltip。输入为应用内AppKit事件，经Avalonia.Native，不冒充外部物理鼠标/IME/其他OS。

首次发布保留了JIT服务注册诊断Assembly.GetType导致IL2026，该产物未执行、不作为AOT通过；删除该诊断反射后重新发布零IL/CS warning，保留五条原Homebrew最低OS linker warning，无suppression。AOT23984856字节，SHA256 a8cf996d0de9c74cb5eeb2dc39ee3da9250841794b236cdbe81df5fd1ef48d5e。

已查看native tooltip及Overlay confirm图，原长文、自然宽度、箭头、两按钮和可见竖滚动条均在；大字/固定220或260内容宽导致原数据裁剪/换行保持，不宣称全极值无裁剪。无产品外观变化，不请求扩大原68对批准；旧四颜色恢复各2/max1与Compact取消1/max1未自动归入FW2。进入清理与精确模块门禁。


## 清理后验收与作者自审

2026-11-01 ToolTip/PopupConfirm原生出口：默认400ms hover、两种native popup/Overlay真实Root，Tooltip120/900与Confirm140/900原长文/箭头/两按钮保持；应用内Cocoa hover/leave、实际取消确认各一次、Escape light dismiss、已打开Global23→25均验证。最终同源managed/AOT各75PASS/exit0；首hover未打开原因未证实保留，初诊断反射IL2026产物未执行，移除后零IL/CS warning重发，无suppression。无产品源改动；17取证及439会话1220947773字节已清理audit clean，最终精确67PASS/0Skip/1s，复用当前相同源码NET8零警告19.98s。旧四颜色/Compact恢复、FW1/FW2原限与完整CT15保留，见tooltip-popup-confirm-native-input.md，不提交。

精确filter=Tooltip namespace|PopupConfirm namespace|Popups.PopupShadowTests；清理后真实重建，未加新永久测试或全量。所有本次进程退出，两个发布/媒体/原始失败日志均清理，最后独立run.py/marker在门禁后删除。作者另步自审、非独立审查；只确认上述原生合成输入和限定状态，不冒充其他OS/外部物理输入/全部自然极值或把旧恢复失败并入FW2。
