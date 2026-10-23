# CT-13 OTP 几何与部件命中结果

G3-B1只接通三类几何：caret宽度读取有效Global LineWidth；Underlined保留有效BorderThickness.Bottom；separator保留有效MarginXXS.Left。默认1/1/4不变，不新增Own或资源编号，不修改Cell C#、光标时间线和根排版。

## 失败与修复

实际模板中的固定值阻止0、粗线和间距配置生效。编译后的专用Cell/Separator Style也未命中：ItemsControl与scope StackPanel之间存在生成的ContentPresenter，原直接子节点路由不符真实逻辑树。修正为后代步骤，末段仍保留直接子节点约束。

separator默认Margin原为模板LocalValue，会压过专用Style；改用OtpLineEdit主题词法Resources中的共享Border ControlTheme Setter。根ControlTheme的后代Setter不能覆盖DataTemplate节点；顶层Border主题又会扩大资源平台域，两种原型均被实际验证排除。最终没有代码属性回退或新增导出。

最初两项underline用例错误假定control算法禁用时，覆盖LineWidth会自动重算BorderThickness alias；改为显式uniform alias，保留独立caret LineWidth检查。新增实际动态、相邻owner和GC检查后，旧源码反向复核10项失败，其中旧GC用例未清理渲染队列，不能作为泄漏证据。补齐帧清理后候选10项临时检查与5项既有Semantic检查全部通过，1s；既有测试中的可失败Style命中证明同时覆盖全部六cell和六separator。

常驻测试净新增0：扩展既有Realized_Cells用例，保留最小编译AXAML fixture。独立价值是检出descriptor字符串、marker及生成器均无法识别的公开Style失效；复用原六cell实例化，成本未增加一套独立用例。临时几何/动态/GC测试仍按默认删除。

## 脚本化渲染与输入

临时驱动使用真实Dispatcher.MainLoop与异步等待，正常motion、相同完整操作轨迹、16个OTP/96格、四variant和四尺寸。旧驱动同步阻塞调度，造成光标透明度与标题栏颜色未正常推进；仅Window.ThemeVariant也未切换AtomUI算法。以上无效数据不作为暗色或稳定证据，修正后完整重放旧源码，产品文件在finally逐字节恢复。

有效独立检查：明暗A/A均0，Spacing增加1DIP均1460像素，恢复均0；主基线不参与扰动。明暗idle/hover/disabled/error/warning/ready六默认阶段及清除后完整RGBA均0差异。配置画面相对旧实现分别改变37519/37564像素，编译专用Style画面各改变2124像素；它们是配置恢复有效，不是默认视觉例外。

真实指针聚焦root，保持原有blink，以预定Opacity 0.65–0.85的首个实际帧观察可见caret。旧requested1/3/0均宽1、14个magenta像素；候选分别宽1/3/0、14/42/0像素。不把不同闪烁相位做默认静态像素等同或声称完整时间线通过。明暗各48次真实键盘输入654321及clear左/中/右点击均通过。已查看完整暗色配置图、专用Style图及AOT可见粗光标，内容无裁剪或重叠。

## AOT、清理及范围

当前Mac arm64 NativeAOT实际运行同一驱动，96格几何/恢复、编译专用Style、三种可见caret及明暗各48次输入清除均通过；与JIT的明暗18张非闪烁画面完整RGBA均0，caret可见像素仍14/42/0。没有IL2026/IL3050；本机macOS26依赖链接至最低12的警告不证明旧OS。该证据是NativeAOT Headless/Skia路径，不代表原生窗口最终显示面。

10项临时测试及3个源码文件、外部工程438个文件、初始错误输出位置的3个专用文件、媒体运行113个文件已删除；媒体Git审计clean。正式文档已同步OTP token/implementation/overview/changelog，并补齐semantic-part.md。清理后`FullyQualifiedName~OtpLineEdit`实际66项PASS，2s（包含原有Input.TextInputLifecycleTests.OtpLineEdit重挂载用例）；测试方法净新增0。NET8 Release零警告/错误，26.65s；23个本地文档链接及git diff --check通过。

作者自审，无subagent、无提交。B2光标声明式状态/偏移与时间线、C根排版、CT25最终显示/性能和其他输入选择家族继续开放。
