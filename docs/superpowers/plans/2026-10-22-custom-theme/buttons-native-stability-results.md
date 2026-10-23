# CT-12 / CT-25 原生按钮显示稳定性结果

本次是实际macOS显示面证据，补充[B10离屏边界](baseline.md#按钮连续闪动补查-b10)。原用户观察来自性能脚本连续交替35次配置，不能以静止终态否定；本次只做有限切换，不再在可见窗口重放快速循环，没有产品“闪动修复”提交。

## 实际身份与方法

取证构建阶段为按钮Global/配色消费分包后、后续字体族分包前；最终CT-25仍需核对最后产物。工作树feature/custom-theme，Native自驱动项目在系统临时目录，引用当前Desktop产品。已验证PID86632、可执行路径、copy-local加载程序集与当前worktree Debug/net10产品DLL的SHA256完全相同，实际NSWindow、RenderScaling2、ClientSize720×660 DIP。ScreenCaptureKit按相同PID和固定窗口标题定位windowID32457，实际1440×1320 PNG，保留完整窗口；未裁剪、缩放、对齐或设置容差，没有桌面点击或Computer Use。截图不包含光标，不改变UI状态。

10个Button/Dropdown加1个Toggle，三档/Custom、普通/主色、原选择状态；900ms等待后按文字握手采集A、1DIP检错、A恢复、B、A返回五阶段，每阶段8帧。Native使用正常动效，同一阶段不再更改配置；源实例引用和Toggle checked=true保持。B时Middle Button字体20、Dropdown18，返回14；scope配置只切B一次、清除一次。

## 实际数值和视觉

| 阶段 | 阶段内后7帧相对首帧差异 | 相对初始画面差异 |
| --- | --- | --- |
| A | 全部0 | 0 |
| 1DIP | 全部0 | 29220像素 |
| A恢复 | 全部0 | 0 |
| B | 全部0 | 253549像素 |
| 返回A | 全部0 | 0 |

已查看B与返回A完整窗口图：字体、主色和下拉指示器实际变化/恢复，连体边框、圆角与按钮轮廓保持。本场景的采样期间未发现自发持续闪动。该结论覆盖当前Mac原生显示面、静止及有限主题更新后采样；不等于每个原生动画帧、鼠标交互、打开弹层、其他平台或全部控件均已验收。

## 驱动问题与清理

首次校验错误要求加载DLL路径包含仓库名，copy-local是正常构建行为；修为比对精确hash，未截图。第二次采样助手未初始化AppKit而在CGS_REQUIRE_INIT中止；修为MainActor初始化NSApplication、禁止激活采样应用，未改变产品。两次失败均关闭本次Native窗口，不计产品RED。第三次Native与采样均exit0。

编译/运行入口为外部Native.csproj、Capture.swift与Run.py，一条python命令完成启动、PID/hash验证、ScreenCaptureKit五阶段捕获、断言和关闭。不存在系统坐标输入。40张媒体、驱动、Swift可执行和专用bin/obj在视觉审查和文本记录后清理，结果在清理完成后追记；本次未修改产品逻辑，无新增常驻测试。

清理完成：40张媒体全部删除，audit clean；外部Native/Swift/运行驱动及专用bin/obj删除；三个本次Native PID均已退出。没有产品逻辑改动，沿用上一消费分包清理后Buttons193/NET8零警告证据，不重复扩大测试。diff检查通过，作者自审完成。
