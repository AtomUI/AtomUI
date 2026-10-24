# CT-14 N1 / N2-A 结果

本分包完成历史能力分类、ListBox/ListView的30字段V2默认，以及实际禁用文字消费；完整CT14仍在进行中。

## 保留字段

原T37四项归Reserved：Menu.DangerItemColor/DangerItemHoverColor/MenuTearOffHeight、NavMenu.KeyGestureColor。补查发现NavMenu十个DangerItem/DarkDangerItem色字段也没有公开数据、状态或模板入口，另归Reserved，纠正此前文档中未核实的“已公开危险项”描述。共14项只改XML摘要、正式Token/changelog和审计分类，保留名称、类型、ID、默认值与配置往返，没有新增功能或运行语句。Menu.KeyGestureColor有实际InputGestureText，继续有效。CT23的可编辑字段目录仍须实现保留项规则，此处不声称工作台已交付。

## 默认和实际消费

ListBox14、ListView16字段改为DependencyDefaultsV2/Revision1，各静态函数仅依赖Global，无虚构Own依赖。原空构造器和整体Calculate移除，保留字段顺序、类型及全部默认公式，包括ListBox.FilterHighlightColor仍取ColorError。真实Compiler明暗60个默认值、公开ID及slot与旧版本逐行相同，NativeAOT同值。

原disabled ContentPresenter分别直接读取Owner/Shared ColorTextDisabled，绕过ItemDisabledColor。仅把原两个style Setter的资源输入改为各自Own，节点、选择器、属性优先级、模板和几何不变。Root有效Global进入Own默认，明确Own优先；透明色、零padding、清除、重新启用与ListBox/ListView兄弟隔离通过。ListView其他Own原本已独立消费，未按错误的跨identity假设重写。

原真实四项失败723ms；扩展兄弟及五个继承主题后，精确恢复旧两个Setter、保持新默认函数时10/10失败860ms；恢复候选逐字节一致。10临时加62相关既有用例72/72通过2s，补零padding后10/10通过873ms。夹具曾误用WithOwnToken和将Control交给要求IListItemData的ListView，后续Content断言也按真实数据容器修正；这些编译/夹具错误不算产品前证。

共享消费者逐项验证CandidateListItem、CascaderViewFilterListItem、SelectCandidateListItem、TransferListItem、GroupHeaderItem的实际编译主题、Own覆盖及清除；通过已有友元程序集名称构建外部验证，不给产品增加访问入口。这里只验证继承主题，不替代这些控件各自完整业务Host、弹层和owner接线验收。

## 画面、输入和原生绘制对照

真实Skia、HeadlessDrawing=false、正常动效，明暗八根四尺寸、1420×630/scale1、每列表8项/高度230；保留原裁剪与滚动。真实指针选择、滚轮92 DIP通过，颜色配置前后viewport/extent相同，保留条目容器。查看明亮配置、暗色恢复与五派生项暗色配置。

主场景初次20张非配置旧新对照16张RGBA0；悬停有4/3px、明亮恢复和滚轮有2px，不使用容差或遮罩，也不将其写成全部0。A/A静止0，1DIP检错24621/24590px，移回0。

独立重放取得了旧版公开Global颜色API的同轨迹：其明亮恢复也在相同文字边缘产生2px，与新Own输入的配置及恢复全画面RGBA均0，暗色同为0。旧新相同公开输入的22帧21对0，另4px位于悬停滚动条的分数裁剪边缘。原版独立重放本身也能产生这4px差异，新版同源重放有3px；完整RGBA哈希证明两种边缘图案均可出现于旧、新版本。所有thumb的Bounds、宽高、圆角、透明度、画刷、变换和裁剪逐值完全一致。证据指向原生栅格状态，未为此改变布局、滚动条、动效或输入；原生绘制现象继续由CT25跟踪，不算新增颜色/几何行为。五派生项明暗默认及恢复四帧旧新RGBA0，显式颜色各改变3793px。

当前Mac arm64 NativeAOT实际运行八根场景、60默认/ID/slot、Own压过Control Global、viewport/extent、真实选择及滚轮通过；主场景JIT/AOT22帧RGBA0。五派生主题另有独立NativeAOT实际覆盖/清除，JIT/AOT六帧RGBA0。共28帧一致；无IL2026/IL3050，仅本机OpenSSL/Brotli的OS26与目标12链接提示。此处是Headless/Skia原生可执行，不是OS最终显示面或其他平台证据。

## 清理和最终验证

10项临时测试、两套外部驱动各423文件和四文件源备份已删除；171个取证文件清理，内置Git媒体audit clean，常驻测试净增0。两个Token文件成员audit无问题；正式ListBox/ListView token/implementation/changelog及CSV已同步，明确配置兼容迁移。作者自审完成，没有独立评审、subagent、CUA或提交。

清理后显式filter覆盖ListBox/ListView、五类继承主题及AutoComplete/Select/Cascader/Transfer宿主、Menu及两类NavMenu契约，共262项通过8s；NET8 Desktop Release含依赖零警告/错误18.08s。没有运行full、change或affected-wide。OTP默认分隔符颜色的独立审查原型8图/两文件仍待用户答复，不属于本列表取证运行；未修改产品默认颜色。
