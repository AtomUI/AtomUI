# CT19 C9 ColorSpectrum Keyboard Implementation Plan

> 使用 superpowers:executing-plans，由主 agent 在当前会话执行，不委派，不创建分支／worktree，不自动提交。

**Goal:** 让普通／渐变 View 内的既有色谱自然接收键盘，并保持二维方向与实际选择器移动一致。

**Architecture:** ControlTheme 声明 Focusable，复用默认 AdornerLayer；现有 OnKeyDown 只修正被有效反例证明错误的方向映射，不增加状态、订阅或键盘功能。

**Tech Stack:** Avalonia12.1.4 raw input、Headless/Skia、.NET10／NET8、当前Mac managed／NativeAOT。

**Spec:** [已批准短设计](color-spectrum-keyboard.md)

## Global Constraints

- 保持 C8 的 SV 默认与六模式接线、原尺寸／裁剪／输入／调色板／渐变stop和正常动效。
- 不新增公开签名、Token、Part、Home／End或Page键功能；不包含滑轨键盘。
- 所属测试项目 Temporary/CT19SpectrumKeyboard 写临时检查，完成后删除；驱动和媒体放仓库外系统临时目录。
- 本机只执行明确filter与NET8 Release；Git仍需独立具体快照批准，CT24仍暂缓，不发布NuGet。

## Review Focus

1. Tab只是访问内部输入而未命中色谱：记录自然完整焦点序列、IsFocused及实际KeyDown到达。
2. 默认焦点框资源存在但未实际创建：检查真实AdornerLayer对象、有效显示与最终像素。
3. 六模式特别是VS纵轴／RTL水平轴方向错误：按手工表格及选择器绝对坐标变化独立验证。
4. 范围／Ctrl／disabled使Alpha或其他分量被改写：检查边界、wrap、实际事件数及完整HSV。
5. 重模板／离树重挂和邻接输入失效：保留实例、解绑旧部件、正常Tab移出和输入框输入。

## Task 1：自然焦点与原始反例

- [x] 两类公开View置于两个外部输入之间，使用原始Tab／Shift+Tab和指针，记录原版无法自然进入色谱；不强设内部Focusable。
- [x] 同时写六模式方向独立字面表：HS左H减／上S增；SH左S增／上H减；HV左H减／上V增；VH左V增／上H减；SV左S减／上V增；VS左V增／上S减。右／下反向，RTL仅水平反向；通过既有输入公开Value／Text准备活动色#7BAA62、Alpha61；保留原RGB／Alpha量化，按实际初值验证字面步长。

## Task 2：最小接通与方向修复

- [x] 只先在 ColorSpectrumTheme.axaml 增加 Focusable=true，删除不存在的FocusEllipse选择器；重跑并分开记录自然焦点通过与残余方向RED。
- [x] 对实际失败方向在 ColorView/ColorSpectrum.cs 的现有 OnKeyDown 修复，保留组件选取、步长、Ctrl、范围、wrap和非方向键路由；同临时检查GREEN。
- [x] 扩展Ctrl／边界／退化、禁用恢复、真实指针、输入邻接、模板／重挂及GC；不改AbstractColorSlider键盘。

## Task 3：视觉／真实宿主与清理

- [ ] 按短设计固定Light／Dark完整历史，比较未聚焦默认／键盘聚焦／移动／焦点移出；独立A/A和1DIP，实际查看关键画面，保留全部真实差异并按规范审查。
- [ ] 当前Mac managed／NativeAOT实际键盘和焦点合同；默认焦点可见性不能只由属性推出。
- [ ] 删除临时源、外部驱动及媒体后，执行ColorPicker精确filter、NET8 Release、所触及控制文件成员审计和diff check；正式文档同步，作者另步自审并如实保留完整家族边界。

计划自审：每项已批准合同有验证落点；只进入两个原产品文件。与正在验证的透明棋盘修复分别保存源身份和前后结果，两者不得相互代替验收。

当前证据：原始8项均无法自然聚焦；主题接通后4项焦点／指针通过，4组有效模式反例均在VS Up失败。修正后8项通过；范围、Ctrl（zh-CN数值分支）、禁用恢复、模板／重挂、邻接实际文本及Window／View／旧新Spectrum／旧新adorner释放，与棋盘场景合计20项通过。英文Ctrl另补，正式视觉、NativeAOT及清理后模块仍待完成。真实Mac managed的明暗／两View／LTR和RTL原始键鼠与棋盘共388项通过，8个实际NSWindow、scale2、ClientSize1200×820；不声称原生最终像素验收。

最新结果见[完整记录](color-spectrum-keyboard-results.md)及[31对限定审查表](color-spectrum-keyboard-noise-review.md)。24项临时检查通过并已删除，清理后原有27项、NET8 Release零警告／错误和3文件／3Control成员审计零问题；媒体在仓库外等待明确视觉审查。真实Mac managed388项；NativeAOT首轮383／388输入事件缺失保留，trace与原二进制复跑各388项通过不解释首轮原因，不据此关闭原生平台验收。

2026-10-25棋盘格修复及31对有限差异获用户明确批准，媒体458文件清理／auditclean。实现、功能、有限视觉与清理出口已记录；NativeAOT首轮缺失事件仍单列CT25，不从本批准推出平台最终验收。C10短设计另获批准并开始执行，本批Git未授权。
