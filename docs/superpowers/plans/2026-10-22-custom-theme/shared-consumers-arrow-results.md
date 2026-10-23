# CT-10 S1 箭头输入闭环

这是CT-10的S1子项结果，不代表共享消费包及全部控件完成。既有feature/custom-theme，主agent独立实施，未提交。

## 实现与边界

ArrowIndicator已构建几何按ArrowSize/TipRadius变化重建，Measure/Render共同失效；保留首次ApplyTemplate前的惰性构建和原ShapeBuilder/平移/描边约定。原写死的尖端半径改为公开TipRadius StyledProperty，默认1.5 DIP、0尖角、非负有限值；无新订阅、无独立Own identity、无Token字典复制。保持默认形状，父owner提供Token/组合职责，生成专用Part Style提供形态定制。正式文档位于对应ArrowIndicator控件目录，Tooltip Part同步；Gallery现有样例默认外观不改。

## 有效验证

临时Controls.Tests验证3/3 PASS、0SKIP、40ms。原B06尺寸8→16仍宽8有效RED1FAIL/paint正对照1PASS，修复后2PASS；TipRadius属性0未进入Geometry的RED1FAIL/其余2PASS，接入后3PASS。读取真实GeometryDrawing/bounds/brush/pen及DesiredSize，尺寸/形态更新和恢复、重ApplyTemplate、默认值及非法半径均执行。DrawingContext会捕获platform geometry快照，错误的几何对象引用相等断言已纠正，不作为产品问题。

临时外部Visual.csproj引用当前源码，Skia真实合成器、320x240/scale1、实际AtomUI Window，保持动效，覆盖实际Transition时长+连续3帧稳定。改前先捕获8尺寸默认及A/A/1DIP；不使用另一checkout或重定义基线。

| 场景 | 精确RGBA差异 |
| --- | --- |
| 旧默认A/A | 0 |
| 1DIP平移检错 | 27像素 |
| 旧默认vs新默认 | 0 |
| 同一箭头8→16 | 126像素，实际宽16 |
| 返回8 | 0，原bounds40,50,8,4 |
| 打开的Tooltip，注册编译ToolTipArrowStyle将TipRadius1.5→0 | 51像素，仅箭头区域 |
| 同Tooltip/Arrow实例恢复1.5 | 0 |

专用Style完全由AXAML声明；观察探针只读取值，不定位后设置语义部件。已查看默认/增大/Tooltip圆角/尖角/恢复及差异图，实际变化限定在箭头；Tooltip主体、锚点与打开状态保持。单独Arrow直接设置尺寸是普通公开实例API验证。比较全尺寸同坐标零容差，无裁剪/缩放/对齐/遮罩。

不是原生OS最终显示面、其他DPI/OS、全部弹层或所有平台通过。缓存修正后的动态尺寸16与新显式半径0产生预期视觉变化；没有改变原默认或其他既有静态输入。箭头参数化不代表CT-10 S2字体/S3框体/S4其他消费者已经完成。

## 清理与最终门槛

临时3项Controls验证源、外部驱动/AXAML/bin/obj全部删除；媒体audit无发现，cleanup成功删除13文件（含清单），无本次进程。清理后Controls.Tests的既有SplashDefaultThemeTests **3/3 PASS、212ms**；Desktop真实共享消费ToolTipArrowDecoratedBoxContract/ToolTipSemanticPart/PopupConfirmSemanticPart **15/15 PASS、837ms**。测试程序集无临时namespace UTF8/UTF16标记。

Controls Release/net8构建**0warning/0error、8.97s**，使用Release restore恢复net8/net10目标；没有运行全量回归。ArrowIndicator成员审计1文件/1控件/0问题；6份控件/模块文档29本地链接无缺失，目录CSV仅更新ArrowIndicator一行，元数据身份不变。净新增常驻测试0。

作者另行自审：旧缓存依赖/初次构建/测量与绘制一致、值验证与原生优先级、无额外状态/绑定/事件、原默认及专用Style真实命中、清理、文档及identity边界均符合本子项合同，未处置Critical/Important为0；没有独立评审。S1出口完成，继续S2；CT-10整体仍进行中。

精确入口：

```sh
dotnet test tests/AtomUI.Controls.Tests/AtomUI.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~SplashDefaultThemeTests
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ToolTipArrowDecoratedBoxContractTests|FullyQualifiedName~ToolTipSemanticPartTests|FullyQualifiedName~PopupConfirmSemanticPartTests'
dotnet restore src/AtomUI.Controls/AtomUI.Controls.csproj -p:Configuration=Release
dotnet build src/AtomUI.Controls/AtomUI.Controls.csproj --configuration Release --framework net8.0 --no-restore
```
