# CT19 C10 ColorSlider Keyboard Implementation Plan

> 主agent使用superpowers:executing-plans在当前checkout执行；不委派、不创建分支或worktree、不自动提交。

**Goal:** 让两类公开View里的单分量及Alpha滑轨响应已批准的标准数值键盘。

**Architecture:** 具体ColorSlider的一个OnKeyDown入口读取既有RangeBase步长和边界，以SetCurrentValue进入原Value→颜色通知链；不改共享基类或多stop轨道。主题和焦点顺序不变。

**Tech Stack:** Avalonia12.1.4 raw input、Headless/Skia、.NET10/8、当前Mac managed／NativeAOT。

**Spec:** [2026-10-25已批准短设计](color-slider-keyboard.md)

## Global Constraints

- 默认SmallChange1／LargeChange10；未修饰Left／Down减、Right／Up增，RTL仅水平反向；Page按大步长，Home／End边界，钳制不wrap。
- Ctrl／Alt／Shift、已处理事件及其他键保留原路由；禁用不更新；slider／thumb冒泡只处理一次。
- 保留完整面板、原尺寸／裁剪／位图、指针和Tab树；不新增公开签名／Token／Part／字段／订阅。
- 使用所属测试Temporary目录；媒体／外部驱动在系统临时目录，完成后删除；仅精确filter与NET8 Release，不运行本地全量。

## Review Focus

1. 自然thumb焦点冒泡重复更新：真实KeyDown到达数与ColorChanged次数分别验证。
2. RTL数值方向与物理thumb移动相反：读完整矩阵及绝对坐标，不只断言Value。
3. Home／End边界误丢绑定或发生额外事件：边界Handled、同值无ColorChanged及下一次绑定输入仍生效。
4. 修改了键盘却破坏邻接输入／指针／多stop：保留原输入树与操作，不改基类；非目标快捷键和原拖拽独立检查。
5. 静态图通过但原生丢事件：真实NSWindow每键记录active、focused及实际送达，不用重试修饰失败；保留C9首轮历史缺口。

## Task 1：有效原始反例

- [x] Temporary/CT19ColorSliderKeyboard建立自然Tab进入slider／thumb、两View×LTR／RTL的四方向矩阵，逐模式独立字面预期；先至少原Right到达1而Value不变的RED。
- [x] 保存完整原ColorSlider字节／SHA及成员审计；原文件1Control零成员问题，不混入纯布局改动。

## Task 2：最小实现与扩展

- [x] 仅增加具体ColorSlider.OnKeyDown，覆盖已批准数值键和现有步长，保留其他键路由；原反例GREEN。
- [x] 扩展Page／Home／End／钳制、修饰与Handled、显式步长／绑定、RGBA、禁用恢复、原实例重挂／模板、邻接真实输入及指针；所有临时功能检查通过。

## Task 3：显示、真实宿主与清理

- [x] 明暗相同完整历史旧新默认／焦点／键盘／恢复，独立A/A与1DIP，查看实际截图并逐项处理真实差异；无通用容差。
- [x] 当前Mac managed／NativeAOT真实输入、实际焦点与事件送达；记录源码／产物身份与所有失败。
- [x] 删除临时验证、外部工程与媒体后，原ColorPicker精确模块、NET8 Release、完整成员审计、正式文档、作者另步自审与diff check。

计划自审：覆盖批准的每项交互及单位，不改变RangeBase默认或其他Slider。保留既有ColorSlider同步flag和位图生命周期，不在本片重构它们。当前无Git授权；完整家族与各平台最终显示面另行验收。

2026-10-25出口：空值／双stop各26项、原生managed／AOT各5688项、640帧／128组1DIP和独立直接128AA／32恢复／40兼容0；72对自然有限差异获用户接受。临时C#／外部驱动／645 PNG及manifest清理，原27项／NET8 Release／4控件成员审计／文档链接／媒体审计与作者另步自审通过。只完成本片本机出口，未取得Git授权，不关闭完整家族／原生最终像素，C9首轮AOT丢事件原因仍未证明。
