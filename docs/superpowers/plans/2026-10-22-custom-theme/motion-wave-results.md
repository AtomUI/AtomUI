# CT-11 M1：波纹输入与几何缓存

2026-10-23，未提交，作者自审。M1 不代替 Transition 动态资源、时序默认设计、Splash 合成循环和原生按钮闪动验收。

实际失败 7 项：传入 Linear/Sine easing 仍使用 CubicEaseOut；range=12 被截成8；range=0仍播放；WaveType 切换后旧值类型动画继续；Paint 后改变尺寸、原点、圆角仍使用旧挖空 geometry（100→200、0→9/11、6→0）。临时测试在真实 Window 内创建 primitive，或调用实际 painter.Paint，只读观测实际 painter/geometry，没有写私有状态制造成功。

修复按原生输入配置 painter，未提供 easing 时仍用既有 CubicEaseOut；有效正 range 不截8，零/非法 range 不启动；切换形状先取消旧动画；源 geometry 缓存键包含原点/尺寸构成的 Rect 与完整 CornerRadius。无每帧源 geometry 重建，无新绑定/订阅/时钟。新增代码注释说明 painter 为非 Avalonia 绘制对象，ControlTheme 无法直接绑定其字段。

9 项临时验证通过（1.99秒）：上述失败、同 painter easing 更新/清除、geometry 输入恢复、默认 Cubic/尺寸时长/透明度尾部不变、关闭停止播放。隐藏 +50ms 仍明确留给 M3：当前默认尺寸300ms、透明度350ms，没有顺手缩短默认动画。

脚本以真实公共 Button 的 MouseMove/Down/Up 构建实际 painter，再从其生成的 Animation 模型渲染 0/75/150/300ms 固定时间图像，避免真实时钟抖动伪造像素差异。源 Button 与 wave 保持同实例，主题 WaveAnimationRange 6→12→6 进入实际 painter；这些图像是实际绘制算法验收，**不是 compositor-clock 验收**。

Skia/Headless Drawing=false，缩放1，Light/Dark：默认旧→新 **0/0像素**，范围修改 **4158/4167像素**，恢复 **0/0**。初始 A/A0，1DIP 检错666/680；稳定 hover 夹具的旧新源码 A/B 两文件已逐字节恢复。原最小夹具未在第一次 click 前等待 hover 转场，导致恢复画刷不同，已排除并记录；修正后实际首帧/修改/恢复画刷分别保持 Light #0958d9、Dark #1554ad，未关闭动画或复位私有状态。已查看默认、变化及暗色恢复的关键图像。

临时路径为 `tests/AtomUI.Controls.Tests/Temporary/CustomThemeWave`；外部 Visual.csproj 同时引用当前产品和该临时只读 helper。精确命令为 Controls.Tests `--framework net10.0 --filter 'FullyQualifiedName~Temporary.CustomThemeWave'`，视觉阶段 stable-before/stable-after，暗色 `ATOMUI_FRAME_DARK=1`。临时验证和媒体清理后补写 focused/NET8 记录。

剩余：外部可达的 easing/精确时长 Token 入口、opacity尾部默认、Non-Visual Transition 动态值和归属、实际合成/平台时间线及全部 owner 消费；不能用本包渲染模型覆盖这些任务。

清理完成：9项临时测试及helper、外部驱动和25个媒体文件删除，audit clean。现有ButtonBehavior/SplitSeparator55/55通过；一项已有缓存测试改为先用有效range预热painter，再验证0→6更新，不新增永久测试。M1/M2清理后的NET8 Desktop含Controls/Shared/Core构建0警告0错误22.49秒。
