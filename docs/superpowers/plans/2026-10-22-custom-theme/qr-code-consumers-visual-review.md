# CT15 QRCode 限定视觉审查（2026-11-01）

状态：有效第二批工程、正常Skia旧新/AOT、临时清理与清理后8项精确模块/NET8零警告完成；仅以下16对已获限定视觉批准。

输入2metadata/2Own，四算法8默认/type/key/slot与原始Bitmap默认RGBA保持，不缩137/274。以下只列16对明确静态变化；第一批主背景错误480帧与未运行publish二进制已留账，全部排除本表。

| 算法 | 状态 | 非零像素 | max通道差 | bbox | 明确原因 |
| --- | --- | ---: | ---: | --- | --- | --- |
| L | global-expired | 14801 | 230 | [280, 200, 440, 360] | 过期实际文字与几何进入Global，原160槽裁剪/方角Cover/显式低对比列明 |
| L | global | 13850 | 237 | [280, 200, 440, 360] | 原字体/边框/圆角/bordered padding进入QRCode Global，保留原码内容 |
| L | native-global | 1313 | 224 | [280, 200, 440, 360] | 原生Courier17/Blue/Orange/9/2/19保持，未覆写边框画刷按Global生效 |
| L | value-empty | 11117 | 224 | [293, 213, 427, 347] | 空Value清除原来仍显示的已编码旧二维码，保留160空框 |
| D | global-expired | 13889 | 174 | [280, 200, 440, 360] | 过期实际文字与几何进入Global，原160槽裁剪/方角Cover/显式低对比列明 |
| D | global | 13848 | 66 | [280, 200, 440, 360] | 原字体/边框/圆角/bordered padding进入QRCode Global，保留原码内容 |
| D | native-global | 1263 | 41 | [280, 200, 440, 360] | 原生Courier17/Blue/Orange/9/2/19保持，未覆写边框画刷按Global生效 |
| D | value-empty | 11117 | 199 | [293, 213, 427, 347] | 空Value清除原来仍显示的已编码旧二维码，保留160空框 |
| C | global-expired | 15566 | 230 | [280, 200, 440, 360] | 过期实际文字与几何进入Global，原160槽裁剪/方角Cover/显式低对比列明 |
| C | global | 14643 | 237 | [280, 200, 440, 360] | 原字体/边框/圆角/bordered padding进入QRCode Global，保留原码内容 |
| C | native-global | 1313 | 224 | [280, 200, 440, 360] | 原生Courier17/Blue/Orange/9/2/19保持，未覆写边框画刷按Global生效 |
| C | value-empty | 12423 | 224 | [289, 209, 431, 351] | 空Value清除原来仍显示的已编码旧二维码，保留160空框 |
| DC | global-expired | 14762 | 174 | [280, 200, 440, 360] | 过期实际文字与几何进入Global，原160槽裁剪/方角Cover/显式低对比列明 |
| DC | global | 14639 | 66 | [280, 200, 440, 360] | 原字体/边框/圆角/bordered padding进入QRCode Global，保留原码内容 |
| DC | native-global | 1263 | 41 | [280, 200, 440, 360] | 原生Courier17/Blue/Orange/9/2/19保持，未覆写边框画刷按Global生效 |
| DC | value-empty | 12423 | 199 | [289, 209, 431, 351] | 空Value清除原来仍显示的已编码旧二维码，保留160空框 |

有效主背景在同一算法scope，D/DC为#141414，并有实际画刷断言；已查看暗色默认与旧新版空Value原图，不能借白码白底隐藏差异。原Size160、完整payload、quietZone0、EccLevel、图标挖空、外框与滚动/裁剪条件保持。Global样本为Menlo23/Border3/Radius17/Padding19/#123456文字与边框/#102030遮罩背景；Root native Courier17/Blue码/Orange文字、Padding9/Border2/Radius19保持，未手设BorderBrush才跟Global。

Global23过期完整原文沿原160槽裁剪，刷新仍原按钮角色；Cover原为方角，Root圆角17不自动改变其矩形拓扑，覆层上角方角可见。明确深遮罩与深蓝文字在亮暗均低对比，原生511尺寸沿原720×520窗口裁剪；不加宽/改色/改Clip掩盖。Icon原生橙色保持，不跟码色统一染色。

正常Skia有效旧新各240帧/140检查，候选全PASS，旧44FAIL保留且无缺帧；176静态160零/16明确差异，64持续态46非零不纳入。四default旧新0；两版各60主恢复52零/8非零（四算法parts-clear/icon-clear，L/C各1/max2，D/DC各12/max1），全部保留未通过。独立A/A与restore各4零，+1DIP5419/5419/5531/5531像素可检出，max224/199/224/199。完整同坐标RGBA，无裁剪/掩码/对齐/容差；contact sheet只辅助查看。

43不同验证用例为40所选与11补充中3新+8复用导出，35临时+8既有，不冒称一次整批43。原始Bitmap四默认全部RGBA通道旧新0，候选四默认+四Ecc均解码原含中文payload；仅编码探针在内存规范alpha占位/白边，保留OpenCV ECI提示，不当作UI视觉或全部扫码器支持。

原生输入focus/键盘/caret、Space刷新、旧模板/卸载事件退休、生成Bitmap/编码流释放与调用方Icon保持通过。Headless不代替OS/摄像头，Loading Spin/caret/按钮持续态保持正常motion；完整周期、全部自然极值/异常输入、GC和平台及完整CT15继续开放。CT12/13/14批准与CT25/FW1/官方依赖/容差不改，无提交授权。


修正publish-final确切二进制aot2四算法240帧/140检查PASS，176静态cross全0、16对全0；41持续态cross非零保留，三版主恢复同52零/8微差。第一批错误480帧与未运行发布不纳入结论，待全部清理及最终门槛后请求限定批准。


清理后8PASS/0Skip（706ms）与NET8零警告（19.04s）完成；35临时/1221取证/566原会话及4最终文件删除，零新增永久测试。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
