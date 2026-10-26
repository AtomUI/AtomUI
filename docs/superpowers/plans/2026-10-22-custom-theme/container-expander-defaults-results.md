# CT14 N3-E：Expander 默认函数本机结果

基线0b5e70c71，沿[既定N3执行清单](container-expander-defaults.md)只修改ExpanderToken.cs及文档。13项默认接入DependencyDefaultsV2／Revision1，字段声明顺序／名称／类型及原公式保持，没有Own→Own依赖。Expander.cs与ExpanderTheme.axaml的SHA保持，未新增默认／交互／模板行为修复。

## 有效前后证据

- 原ExpanderControl精确40项通过。有效临时RED为V2声明合同1项失败、原明暗消费2项通过；候选三项通过。完整13Own输入扩充后，原版明暗两项仍通过，不把正常原消费称为Bug。
- 真实Compiler明暗26默认及182条全历史记录新旧相同，逐项含原始值／类型、投射资源／类型、identity、资源key类型／枚举数值与dense slot。完整历史default→Global→Control Global→13Own→明确0／透明→reset→同实例重挂；Header／Content实例padding明确0、清除及Ghost背景分别核对。
- ContentPadding水平常量16保留，SM／LG保留完整Global.PaddingSM／PaddingLG；Control Global中的UniformlyPadding进入当前owner，显式下游保持。
- 两个垂直margin仅发现声明和默认赋值，实际四方向旋转Header仍消费HMargin；两项VMargin资源、原公式及31／37明确值往返保持，归兼容保留，不补造消费者。容器Reserved当前18+2=20。

准备错误独立保留：首次观测误将两个同名Presenter都当作body，改为校验TemplatedParent；临时Grid与现有测试namespace重名已限定类型。声明Revision=1不是descriptor的复合EvaluatorRevision，Generator会对层／声明revision／默认计划计算非零指纹；探针错误的字面1断言已按正式合同改正，产品未为探针调整。原生外部宿主补齐命名空间、通过公开AddInitializer取得manager，并改用Utf8JsonWriter消除反射序列化／动态代码依赖；准备失败不计入产品RED或验收通过。

## 自然画面与保留状态

Mac arm64 Headless／Skia、scale1、1100×720，使用产品Window与实际应用主题事务；四控件保持440×240明确尺寸，分别Large／Middle／Small／Custom、Down／Up／Left／Right，保留两类图标位置及默认motion。三个独立Window分别为main／A/A／外部左距增加1DIP，主实例无检错扰动。

旧新同一完整历史：default→Global→Control Global→13Own→Ghost／Borderless→collapsed→disabled并重新展开→reset。每阶段覆盖现有Transition及至少700ms，连续三同帧，超时4秒；不关闭动效、对齐、缩放、裁剪、遮罩或设置容差。96帧中32组A/A、12组恢复、48组新旧全0，32组1DIP全部检出。明暗各96条状态／几何观察新旧相同，四方向和尺寸保持。

已查看明色默认、暗色Ghost／Borderless和明色collapsed完整画面：标题与内容位置、旋转、背景及圆角保持；明确固定Height使收起后Frame继续占有240DIP，只隐藏内容。本Host收起后仍观测到一个模板body Presenter，不宣称已验证节点完全缺席或其他Host的可选物化。所有帧原始结果保留；本次零差异无需扩展其他控件的限定噪声批准。

## 原生与清理出口

当前Mac实际NSWindow、scale2：managed245项／NativeAOT246项通过，后者动态代码禁用，两者182条资源记录完全相同。覆盖明暗、Global／Control Global／13Own／明确0／撤回、同实例与重挂及实际模板padding。这些资源／状态检查不证明原生最终像素或原生输入；Homebrew dylib面向macOS26而链接最低12的五项警告保留，只证明当前Host运行。

两份探针只用于本次迁移快照和前后取证；通用V2合同已有Core／Generator机制保障，控件与Part行为复用原40项，没有新增全局常驻测试的独立价值，已删除临时C#、常驻净增0。97个媒体文件与419文件／1,376,858,422字节外部原生工程已删除，两个原生进程结束，媒体审计clean。清理后重新构建并运行`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~ExpanderControl`，原40项通过、0Skip、测试2秒；Desktop NET8 Release46.02秒、零警告／错误，完整Token文件成员审计1文件／0Control／0问题。

正式四文档及13Token／1Control目录同步。作者另步自审核对13表达式、资源身份、准备错误、原始比较和清理，不声称独立评审。仅本机默认机制切片出口完成，完整Expander、其余Global／字体／输入／连续动效、N2／N4、其他Host及原生最终像素仍开放；新Git范围另审。
