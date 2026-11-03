# CT15 Flyouts bounded visual review (2026-11-01)

状态：表内48对已获限定批准；其余边界保留。只申请下表48对，完整Flyouts/CT15仍开放。最终候选/AOT各196帧、144检查PASS，180静态跨运行全零；48申请帧跨运行全零。40聚焦用例PASS，临时材料已清理，清理后既有17项和NET8零警告验收通过（详见文末）。

具体变化：Host Global背景进入真实Popup，Native字体及clear仍保留该配置；Host重模板重新协调pinned状态，保留原scroll对象和Offset500（正文原在上方，视口空白并非删除内容）。Menu/Tree Pointer偏移读取当前scope，热Own及Native53进入实际Popup、非Pointer真实归零、重开及跨scope41/43保持。

Global/Native字体有效值已由实际presenter检查；该主轨迹正文在Offset500视口外，表内Host非零主要来自背景，不能据此声称文字glyph视觉已充分覆盖。

明确包含两项有效默认纠正：Tree背景null→原Own PopupBgColor；Compact Pointer默认offset从原应用级8进入原局部配方4，资源默认记录本身未改。Tree增加滚动条后保留全部40原行、原宽高/233限制及裁剪；默认大列表仍有原窗口上缘裁剪，不批准“所有行在窗口内同时可见”。暗色Own浅绿背景仍搭配原白字，未偷偷改文字角色，此低对比配色已列明。

不申请：global-clear各5或6/max1恢复微差；所有定时非零；旧版缺52帧后的业务历史；OS/Browser真实输入、完整焦点/平台/极值/GC出口及完整家族。两项关闭后资源弱引用PASS只覆盖该探针。既有CT12/13/14批准、CT25/FW1原始问题及容差继续保持，不授权提交。

800x600 full-frame RGBA at identical coordinates, no crop/alignment/mask/tolerance. old2/candidate2 follow the same actions. Old main replay stops after tree-own with the original missing ScrollViewer: 144 captured, 52 missing; old2cal is an independent 16-frame supplement and does not fill that history.

The author has inspected all 13 old/new state sheets across four algorithms, plus full-size critical images. Sheets are inspection aids only; the numbers below use original full-resolution frames.

| Algorithm | State | Changed pixels | Max channel delta | BBox |
| --- | --- | ---: | ---: | --- |
| light | host-global | 50056 | 65 | (272, 99, 528, 301) |
| light | host-native | 50056 | 65 | (272, 99, 528, 301) |
| light | host-native-clear | 50056 | 65 | (272, 99, 528, 301) |
| light | host-retemplate | 22254 | 51 | (249, 85, 551, 327) |
| light | pointer-menu-own | 24474 | 235 | (254, 211, 434, 576) |
| light | pointer-menu-native | 24020 | 235 | (254, 211, 464, 576) |
| light | pointer-menu-normal | 23351 | 235 | (145, 218, 318, 568) |
| light | pointer-menu-reopen | 24474 | 235 | (254, 211, 434, 576) |
| light | pointer-menu-scope | 18705 | 235 | (510, 33, 712, 401) |
| light | pointer-tree-default | 3957 | 165 | (258, 0, 408, 600) |
| light | pointer-tree-own | 60120 | 235 | (254, 211, 506, 481) |
| dark | host-global | 50056 | 216 | (272, 99, 528, 301) |
| dark | host-native | 50056 | 216 | (272, 99, 528, 301) |
| dark | host-native-clear | 50056 | 216 | (272, 99, 528, 301) |
| dark | host-retemplate | 61326 | 46 | (261, 97, 539, 327) |
| dark | pointer-menu-own | 33803 | 202 | (254, 211, 434, 576) |
| dark | pointer-menu-native | 50504 | 202 | (254, 211, 464, 576) |
| dark | pointer-menu-normal | 25664 | 189 | (145, 218, 318, 568) |
| dark | pointer-menu-reopen | 33803 | 202 | (254, 211, 434, 576) |
| dark | pointer-menu-scope | 50398 | 225 | (510, 18, 712, 401) |
| dark | pointer-tree-default | 89966 | 224 | (258, 0, 408, 600) |
| dark | pointer-tree-own | 60017 | 234 | (254, 211, 506, 481) |
| compact | host-global | 46576 | 65 | (276, 107, 524, 301) |
| compact | host-native | 46576 | 65 | (276, 107, 524, 301) |
| compact | host-native-clear | 46576 | 65 | (276, 107, 524, 301) |
| compact | host-retemplate | 21434 | 51 | (253, 93, 547, 327) |
| compact | pointer-menu-default | 33252 | 224 | (231, 195, 416, 533) |
| compact | pointer-menu-own | 21338 | 235 | (254, 211, 418, 536) |
| compact | pointer-menu-native | 21752 | 235 | (254, 211, 448, 536) |
| compact | pointer-menu-normal | 21927 | 235 | (161, 218, 318, 528) |
| compact | pointer-menu-reopen | 21338 | 235 | (254, 211, 418, 536) |
| compact | pointer-menu-scope | 17898 | 235 | (510, 71, 696, 401) |
| compact | pointer-menu-restore | 33252 | 224 | (231, 195, 416, 533) |
| compact | pointer-tree-default | 53819 | 244 | (231, 0, 420, 600) |
| compact | pointer-tree-own | 60202 | 235 | (254, 211, 506, 481) |
| darkcompact | host-global | 46576 | 216 | (276, 107, 524, 301) |
| darkcompact | host-native | 46576 | 216 | (276, 107, 524, 301) |
| darkcompact | host-native-clear | 46576 | 216 | (276, 107, 524, 301) |
| darkcompact | host-retemplate | 57422 | 46 | (265, 105, 535, 327) |
| darkcompact | pointer-menu-default | 20117 | 195 | (231, 195, 404, 521) |
| darkcompact | pointer-menu-own | 29359 | 202 | (254, 211, 418, 536) |
| darkcompact | pointer-menu-native | 43601 | 202 | (254, 211, 448, 536) |
| darkcompact | pointer-menu-normal | 23964 | 189 | (161, 218, 318, 528) |
| darkcompact | pointer-menu-reopen | 29359 | 202 | (254, 211, 418, 536) |
| darkcompact | pointer-menu-scope | 44208 | 208 | (510, 58, 696, 401) |
| darkcompact | pointer-menu-restore | 20117 | 195 | (231, 195, 404, 521) |
| darkcompact | pointer-tree-default | 96755 | 224 | (231, 0, 420, 600) |
| darkcompact | pointer-tree-own | 60099 | 234 | (254, 211, 506, 481) |


清理后最终验收（2026-11-01）：共享锁空闲后独占执行，Flyouts既有17PASS/0FAIL/0Skip（用例760ms，命令31.37s）；NET8 Desktop Release 0警告/0错误（构建19.83s，命令20.01s）。23临时/3源25325字节、1191取证、320driver/两publish各15、488原会话文件已清理，最后6文件及final marker也删除。git diff --check通过。48对及列明有效默认纠正待限定批准，原40失败/52缺帧、4恢复micro/12跨运行定时、平台/完整出口及CT15保留；不改CT12/13/14批准及CT25/FW1，无提交。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
