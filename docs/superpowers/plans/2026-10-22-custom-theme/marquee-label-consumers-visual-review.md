# CT15 MarqueeLabel 限定视觉审查（2026-11-01）

状态：工程、有效第二批正常Skia旧新/AOT、临时清理及清理后精确模块/NET8完成；仅表内8对明确静态变化已获限定批准。

输入2metadata/2Own，四算法8默认/type/key/slot保持，无公开Semantic manifest。首批空宿主遮挡的Light43帧排除，不以空白图通过；下列仅8对明确静态变化。

| 算法 | 状态 | 非零像素 | max通道差 | bbox | 有意变化 |
| --- | --- | ---: | ---: | --- | --- | --- |
| L | global | 3306 | 237 | [60, 269, 484, 292] | 明确Global Menlo23与#123456进入实际文字 |
| L | scope-switch | 2362 | 255 | [60, 267, 409, 284] | 同一owner移到第二scope，Courier19/Purple进入实际文字 |
| D | global | 3306 | 202 | [60, 269, 484, 292] | 明确Global Menlo23与#123456进入实际文字 |
| D | scope-switch | 2362 | 219 | [60, 267, 409, 284] | 同一owner移到第二scope，Courier19/Purple进入实际文字 |
| C | global | 3306 | 237 | [60, 269, 484, 292] | 明确Global Menlo23与#123456进入实际文字 |
| C | scope-switch | 2362 | 255 | [60, 267, 409, 284] | 同一owner移到第二scope，Courier19/Purple进入实际文字 |
| DC | global | 3306 | 202 | [60, 269, 484, 292] | 明确Global Menlo23与#123456进入实际文字 |
| DC | scope-switch | 2362 | 219 | [60, 267, 409, 284] | 同一owner移到第二scope，Courier19/Purple进入实际文字 |

短文完整Original short text with units.保留，Root600×28与原位置、裁剪不变；原生Courier19/Orange、本地17/61与普通Style覆盖均有真实检查。暗色显式深蓝/Purple低对比已列明，不改成其他颜色掩盖覆盖。正常长文沿原220×28、ClipToBounds=True绘制，完整Text不删改，字体23下仍按原高度裁剪；不扩宽或取消限制。

可见旧新各172帧/100检查，候选全部PASS，旧版24FAIL完整保留、无缺帧。112静态104零/8明确变化；60持续态48非零，不纳入本8对。四default旧新0；两版各44主恢复全0，独立A/A与restore各4零，1DIP1284/1282/1284/1282像素可检出，max224/199/224/199。完整同坐标RGBA，无裁剪、掩码、对齐或容差；contact sheet仅辅助查看。

完整22所选PASS（21临时+既有1），含Global/Own/native/Style、真实宽度重建/旧完成不清当前、三处InputHitTest hover原位暂停/悬停字体重排/离开恢复、CTS退出和参数边界。原4倍时长、OnlyIfVisible与绘制保持；0速度沿原OverflowException并可clear，原源码独立同例PASS，不合成停止功能。

正常滚动全部按真实clock采样，hover只是单独交互状态，不冒称默认持续动效静态通过。实际frame/hit与数学状态验证不替代OS、完整周期/全部极值、GC和平台出口。完整CT15与其他未通过保持，CT12/13/14既定批准、CT25/FW1/官方依赖/容差不改，无提交授权。


AOT确切二进制四算法172帧/100检查PASS，112静态cross0、8对全0、44主恢复与独立A/A/restore0；55持续态跨运行非零保留，不纳入8对。首遮挡43帧仍排除，等待完整清理和最终模块/NET8后请求限定批准。


清理后精确模块：Debug/net10筛选Marquee有效显隐1项与Alert9项，10PASS/0FAIL/0Skip，637ms（命令36.86s）；NET8 Release桌面构建0警告0错误，21.98s（命令22.33s）。两次均先获得共享产物非阻塞独占锁。最终会话run.py/cleanup.json/module.log/net8.log共4文件随后清理；无新增永久测试、不运行全量、不提交。


限定批准登记：2026-11-01，用户明确回复“批准表内8对，其余边界保留”（request_user_input_async / call_492fb90497c742dfbc2801dac1500c71 / item 0）。仅MarqueeLabel表内8对Global/跨scope字体颜色变化获批准；48旧新/55跨运行持续态非零、首批43帧排除、原4倍时长/0速度异常、裁剪、平台/GC/完整CT15继续保留，不授权提交。
