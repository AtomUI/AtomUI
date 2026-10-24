# CT-11 剩余 Transition 消费接入

本分包将[目录](motion-transition-catalog.md)中的83组、74个AXAML文件接入既有TokenTransitions；覆盖Desktop、DataGrid、ColorPicker及GalleryBase。每个集合保留Property、Delay、Easing及原选择条件，不新增反射工厂或动效引擎。全部prototype已与修改前保存的源码结构逐项比较；两个明确的时长映射如下。

IndicatorScrollBarThumb的原实际Duration为0，独立原HEAD回放也相同。集合移至style Setter，使用UnconfiguredDuration=0；只有登记owner的规范配置明确提供Mid才启用，清除恢复0。独立未登记primitive仍0。原生Avalonia ScrollViewer分隔区的模板内100ms集合移至style Setter，用默认100ms的Fast接入，保留viewport、extent及可见滚动条。

GalleryBase编译资产首次实际物化报内部扩展构造器MethodAccessException，已补既有Core友元访问边界。最终370个资产全部物化：Common9、Desktop315、ColorPicker15、DataGrid18、GalleryBase13；不能把物化计作370个控件状态的视觉验收。

## 实际验证

临时18项在精确回放的77个修改前文件下全部失败、2s；恢复候选源码逐字节一致后18/18通过、2s。12个实际根控件的原生集合在1234ms→456ms→清除时保持实例和Transition身份；混合Fast/VerySlow保持独立槽及原Easing/Delay。另验证Control级配置、兄弟隔离、LocalValue覆盖/清除、移除后root/provider/私有集合GC，以及Indicator和原生滚动分隔区。

真实指针选择Segmented第三项后SelectedIndex=2，实际原生飞行动画timer由修改前332ms变为配置800ms+原32ms余量=832ms；未改AbstractSegmented代码或增加定时器。全局EnableMotion=false的Indicator明确时长归0。原生Thumb没有IsMotionEnabled API，未为测试新增或假设此属性。

真实Skia、HeadlessDrawing=false，正常动效，Light/Dark分别采12根控件及原生ScrollViewer/GalleryBase IconInfoItem。1160×750与620×260、scale1，同输入的旧新15阶段×2主题共30帧完整RGBA均0。包括idle、悬停、按下、释放、配置、恢复及滚动条悬停；实际103个Transition断言配置时长，额外场景断言456/800ms。A/A均0，1DIP检错10247/4489像素，移回及配置清除均0。查看明暗idle/pressed/config及暗色额外悬停；暗色Card普通字符串仍黑的现象新旧相同，属CT15实际前景消费待核，不在此改色。

当前Mac arm64 NativeAOT实际运行Light/Dark均通过370资产、103个时长及额外场景断言，与最终JIT的30帧完整RGBA均0。初次并行publish在ILC前发生MSB4166节点退出；串行-m:1、nodeReuse=false、UseSharedCompilation=false重新发布成功。无IL2026/IL3050；本机OpenSSL/Brotli的OS26与目标12链接提示保留，不推断旧OS或其他平台。

## 清理和出口

18项临时测试、472个外部驱动/发布文件、四个源码回放helper及专用诊断目录已删除；111个媒体运行文件清理，内置Git媒体audit clean，常驻测试净增0。两个Core辅助文件成员audit无问题，git diff --check通过。

清理后显式filter：Desktop受影响消费家族2184项通过、64s；Core资源/身份/StateMotion27项通过、575ms；DataGrid header/row回收/expander/selection39项通过、36s；GalleryBase三类契约22项通过、478ms。GalleryBase、ColorPicker、DataGrid的NET8 Release分别19.56/6.62/8.75s，均零警告/错误，依赖Core/Shared/Common/Desktop同时构建。没有运行full、change或affected-wide范围。

此分包完成动态集合接入和代表机制验证。83条路径的完整业务状态、连续动画、弹层和每个公开owner视觉仍随CT12–CT19回填；CT11总出口保留进行中，不能用12个根实例或370资产物化代替全家族验收。作者自行审查，没有独立评审、subagent、CUA或提交。
