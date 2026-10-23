# CT-13 OTP 光标状态与字号同步结果

G3-B2A将caret显隐和插入点偏移交给AXAML，删除Cell中的caret字段、代码显隐及RenderTransform维护。不新增公开API、Own、资源ID、部件或伪类；原1s线性往返动画树SHA保持一致，完整blink时间线与motion职责仍属B2B。

## 实际失败与声明式迁移

已填字符保持焦点时，主题FontSize从14改为25，cell字号和caret高度已为25，偏移仍为4.2而非7.5；实际RED一项，启用/禁用/焦点正对照一项通过。脚本Large样本同样从16改25后停在4.8。该修复使已配置字号被正确消费，默认字符位置继续原FontSize×0.30近似，不宣称精确字形度量。

显隐以主题`:cell-active:not(:disabled)`表达。首轮误用不存在的`:enabled`，被68项聚焦检查中的6项失败检出，修正后68通过。继续检查发现，仅以“不能直接TemplateBinding计算”不足以保留代码偏移：编译AXAML原型证明，模板内同一TranslateTransform.X可用DisplayText与FontSize双输入绑定更新、清除并保持对象身份。原型最初错误指定ContentControl，编译无法解析DisplayText；改为实际Cell TargetType后通过559ms，不能把原编译错误算框架限制。

最终通过有限OffsetConverter计算double，Transform每模板创建一次，更新X而不替换对象；Transform上没有DynamicResource或Token资源订阅。Cell只维护输入状态与运行时Content Cursor。后者的TextBox.TemplatedParent为空，Cell的模板选择器不能覆盖Content替换，代码保留原因已写明。未夹带额外的启用状态Cursor行为修改。

5项临时验证与66项既有OTP检查合计71项PASS，3s。覆盖字号动态更新/恢复、空内容0偏移、根焦点、禁用/重新启用、聚焦移除后root/caret GC、重新套用模板后旧caret GC及六cell/单活动caret恢复。最终成员审计1文件/1控件、问题0，类型公开头未改变。

## 渲染、AOT与清理

同一异步Dispatcher脚本、正常motion、16个单格OTP、四variant/四尺寸；明暗idle/hover/disabled/ready/configured-unfocused/returned六阶段新旧完整RGBA均0。独立A/A0、Spacing增加1DIP各1385像素、恢复0。真实指针聚焦、键盘输入、禁用/启用和转移焦点均通过。预定可见Opacity 0.65–0.85阶段记录默认Large偏移4.8、配置25后7.5、恢复4.8、空内容0；该动态观测不等同于不同相位的默认像素比较。

最终AXAML版本当前Mac arm64 NativeAOT实际运行通过，明暗12张非闪烁画面与JIT完整RGBA均0，配置/清除偏移及输入状态均正确。已查看最终暗色配置聚焦图，caret在字形右侧、所有行及焦点目标完整显示。先前C#中间候选的JIT/AOT证据另存后清理，不冒充最终AXAML版本。没有IL2026/IL3050；已知本机依赖最低OS链接提示不能证明旧macOS。NativeAOT证据限Headless/Skia，不代表原生最终显示面。

5项临时用例、3个临时源码文件、外部工程429文件及媒体运行109文件已删除，媒体Git审计clean，常驻测试净新增0。清理后OTP精确filter66项PASS，2s；NET8 Release零警告/错误17.44s，23个正式文档本地链接通过。正式implementation/changelog及执行计划同步，作者自审，无subagent、无提交。

B2B时间线、C根字体与separator排版、其他输入选择家族及CT25显示/性能出口继续开放，不能以本分包关闭完整OTP或输入家族。
