# CT-13 OTP 光标时间线主题结果

G3-B2B追加CaretBlinkDuration/CaretBlinkEasing两个Own，V2修订2，默认仍为完整1s、线性1→0→1。原CellGap/CellWidth/LG/SM的名称、类型、公共ID0–3保持，两个新ID追加为4/5。当前资源编号1480／96范围，字段目录1221；此前1478编号冻结及1195初始字段是历史基线，不改写已有包的证据。

## 根因与最小实现

两个真实配置均被旧schema拒绝，临时验证2项失败／旧四ID实际消费正对照1项通过，703ms。旧异步Dispatcher明暗基线证明，默认、装饰motion=false、只读均保持约1s完整周期；隐藏暂停并保留相位。Ant Design本地OTP沿原生Input，只主题化caretColor等；Avalonia TextBox的500ms输入光标间隔同样独立于装饰Transition。本包保留AtomUI原1s线性往返，不重新解释motion开关。

每实例Own不能绑定共享非Visual Animation。最初RunAsync原型实际失败：框架拒绝Infinite，不能改用重复RunAsync循环来掩盖。第二个原型使用叶节点独占的原生Style，实测隐藏前Opacity0.45、隐藏后0.45、恢复首观测0.376，移除Style后1，通过后才进入产品实现。

内部OtpLineEditCaret继承Rectangle，不重写Render、布局或命中。三个Cue/Opacity关键帧仍由Cell ControlTheme词法Resources声明；typed Own Setter绑定视觉对象，活动状态TemplateBinding来自cell。仅原生Style/Animation副本由每个caret持有，复用框架clock，没有自建timer、反射或第二套优先级。私有蓝图仅含Cue与Opacity常量，不是通用Animation克隆器。共享非Visual对象上无Token订阅。

周期或缓动有效值变化时替换本实例Style；等价Spline/Spring参数、字体/颜色变化不重启。可变曲线沿Core已有有限复制表隔离。原生OnlyIfVisible保留隐藏暂停相位；失活、detach和模板退役移除Style。非正周期不夹成最小值，保留静态可见输入反馈；禁用/失焦显隐仍由既有ControlTheme合同决定。

7项临时验证与66项既有OTP检查合计73项PASS，3s：实际Own消费、旧ID、可变Spline/Spring隔离、等价更新、零/负周期、移除时保留其他Style、重挂载及root/caret/Style/Animation四对象GC。临时fixture初次namespace/IStyle编译错误已修正，不算产品行为失败。新增控件成员审计1文件/1控件，问题0。

## 脚本渲染与时间观测

实际异步Dispatcher、正常motion、16个OTP／96格、四variant／四尺寸。明暗idle/hover/disabled/error/warning/ready/hidden/returned共16张完整RGBA新旧差异均0。独立A/A0；间距移动1DIP明暗各1385像素；恢复0。未裁剪、遮罩、对齐或设置像素容差。

| 观测 | 旧明／暗 | 最终JIT明／暗 |
| --- | --- | --- |
| 默认完整周期 | 0.994／0.987s | 1.010／0.988s |
| motion=false | 0.999／1.002s | 1.001／1.007s |
| 只读 | 0.996／0.992s | 0.999／0.997s |
| 显式2s | 配置拒绝，约1s | 2.015／1.996s |
| 零／负周期 | 没有旧入口 | 全样本Opacity=1 |

时间采样使用真实渲染后的时间戳，交叉点插值估算周期；预先定义的周期误差门槛80ms用于离散时钟测量，不是像素容差。所有opacity样本参与分析，按首个向上0.5交叉点的数学相位重建完整原生曲线，不挑帧；76条轨迹的样本数、范围、周期及模型误差已汇总，原始轨迹随临时验证清理。最终默认线性模型中位绝对误差0.017/0.018，2s为0.010/0.009；SineEaseIn配置实际曲线中位误差0.014/0.012，早期线性错误模型约0.14，证明曲线进入原生动画。不同执行的焦点截图相位不相同，不声称动态逐帧像素恒等或原生最终显示面已验收。

最终JIT还通过：隐藏全样本Opacity固定、恢复首观测相差0.092/0.092且未重置；普通颜色配置保持同一个动画Style；零和原始负TimeSpan均静态1；真实键盘输入654321、禁用/重新启用、转移焦点、清除恢复及96格实例不重建；GC4/4。

## NativeAOT与最终出口

当前Mac arm64 NativeAOT实际运行通过，默认周期明暗1.000/1.000s，显式2s为2.001/2.000s；Sine、零/负值、普通颜色更新不重启、隐藏暂停/恢复、真实输入、96格实例保持及四对象GC均通过。与最终JIT的18张非闪烁画面完整RGBA差异均0，含两张静态可见caret。已查看最终AOT暗色静态光标画面，字形、活动边框及16行完整。未见IL2026/IL3050；本机macOS26依赖的最低OS链接提示保留，不能推断旧系统。此证据限Headless/Skia。

已删除7项临时用例／2个源码文件、外部工程436文件、媒体运行89文件及76条原始时间轨迹；保留数值摘要和验证日志，常驻测试净新增0。清理后OTP精确filter66项PASS、3s；NET8 Release零警告/错误、23.51s；24个正式文档本地链接、成员审计及diff检查通过。作者自审完成，无subagent/CUA/提交。

OTP根字体/separator排版、其余输入选择家族、CT25持续原生显示面及完整性能出口继续开放。完整家族验收仍为0，不以此分包代替全OTP验收。
