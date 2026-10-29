# CT13 NU3：Numeric inline 分隔边结果

状态：最小实现、本机专项及临时清理完成；[限定视觉审查](input-numeric-divider-visual-review.md)已独立批准，不关闭完整家族。

## 根因与实现

NumericUpDown公开BorderThickness已进入内部spinner和框体，但inline模板的减号右分隔和加号左分隔分别硬编码(0,0,1,0)/(1,0,0,0)。有效全局/本地3、非均匀(3,5,7,9)、0四项原反例4FAIL、739ms。候选只增加两个既有BorderThicknessFilterConverter资源与两条TemplateBinding，分别保留Right/Left。按钮本体仍按实际高度取宽，分隔线另占空间；没有C#绑定、新字段、事件或新converter类。

## 可观察证据

四专项加真实Skia场景5PASS、0Skip。初次候选5m18s的异常耗时保留；单独计时复核9.847s（专项599/18/17/14ms、视觉6s），未复现布局循环，不能推导全面性能结论。最后直接绘制5PASS、8s。ShowButtonSpinner=false实际退出inline分支，观测器原先强转PixelAlignedBorder失败，已改为只读nullable记录；这是场景观测缺陷，未改产品规避触发。另一次计时缺环境变量的失败也保留。

明暗各6根、四尺寸/自定义50与60高、固定宽260，配置3下真实中心点击增减；隐藏动作、禁用、恢复保持。156根记录的尺寸、值、ShowButtonSpinner与Bounds前后一致，按钮本体宽高一致。数值12→12.0为两侧同有的既有格式行为。全局干扰ButtonSpinner.BorderThickness=11及子IconButton=13不抢父几何，0/本地/非均匀/清除成立。

26对完整帧中16非修复阶段零差异，10显式厚度阶段改变对应分隔边与空间分配。自然与独立直接绘制精确一致；[审查表](input-numeric-divider-visual-review.md)保留每对像素/最大通道。已查看关键完整帧，用户已独立批准本包10对变化。

## 原生与构建

实际NSWindow managed候选590项通过，涵盖真实模板边框/按钮、全局/本地非均匀/0/清除/隐藏与数值保持；它不是原生最终像素或原生点击证据。

第一次原生失败曾出现根厚度3、内部3而分隔仍1；核对证明构建输入为原版主题，且候选备份被原版覆盖。重建精确两资源/两绑定并核对SHA256后，实际候选日志为PASS590；旧失败不算候选产品RED，原先错误session计数已更正。原始日志全部保留。官方Avalonia12.1.4不变。

实际arm64 NativeAOT也为590项、exit0；两后端均使用官方Avalonia12.1.4。没有IL2026/IL3050或启动图抑制；五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release构建0警告/0错误、20.56s。

作者另步自审核对了父厚度传递、两个converter筛边、TemplateBinding优先级、0/清除、Square占位与既有NU2-A四图标入口保留；无独立评审。两临时C#及目录、源码快照/媒体映射已删除；外部原生工程422文件、1,117,474,759 bytes删除；媒体142文件清理，audit clean。常驻测试净增0，没有本包进程遗留。

清理后Numeric精确49PASS、0Skip、2s；git diff --check通过。本包无Git操作，全部原有未提交工作保留。批准仅限本包，不能把本机分包作为完整CT13/CT20/CT25出口。

```bash
CT_DIVIDER_MEDIA="$PWD/.superpowers/sdd/completion-current/numeric-divider-media.json" CT_DIVIDER_TEXT="$PWD/.superpowers/sdd/completion-current" CT_DIVIDER_STAGE=after-direct CT_DIVIDER_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeNumericDivider
```
