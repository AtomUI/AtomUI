# CT13 S7：add-on 颜色消费限定视觉审查

2026-10-29；用户明确回复“批准 S7 本表范围，保留 CT25 调查”（call_ac357f9defaf4cc29f851e1b3d968c5e:0），本表范围已批准。

范围：当前 Avalonia 12.1.4/macOS arm64，1120×800、缩放1的 Skia Window；Select/TreeSelect/Cascader 各四尺寸×四 variant 共48实例，原L/R外侧附加内容与标题保持。相同完整历史：initial/AA/仅标题1DIP扰动及严格复位/真实hover/error默认/配置error/配置warning/配置普通/禁用配置/清除。控件motion保持开启，没有对齐、裁剪、遮罩或通用容差。

候选仅改变三个内部add-on主题各23个资源来源。显式配置将11个通用颜色键分别设为Select `#176d93`、TreeSelect `#89346d`、Cascader `#41752a`，普通边框、错误/警告边框和Filled中心背景随公开owner变化；原版中心仍读Shared，外侧add-on已有正确owner路径。四尺寸的默认布局、内容、位置及variant保持。强烈填充色为验证输入，不是新的主题默认或推荐配色。

| Theme/stage | Natural pixels/max channel | Direct pixels/max channel |
| --- | ---: | ---: |
| light/error-configured | 76792/232 | 76790/232 |
| light/warning-configured | 76795/232 | 76742/232 |
| light/normal-configured | 76795/222 | 76790/222 |
| light/disabled-configured | 76790/222 | 76790/222 |
| light/cleared | 1/1 | 0/0 |
| dark/error-configured | 76795/197 | 76790/197 |
| dark/warning-configured | 76795/193 | 76742/193 |
| dark/normal-configured | 76795/109 | 76790/109 |
| dark/disabled-configured | 76790/109 | 76790/109 |
| dark/cleared | 6/1 | 0/0 |

自然/直接共44对：26对0、18对变化。初始、AA、标题检错与复位、真实hover、默认错误态的24对全部0；直接清除2对另为0。标题1DIP在各侧/各后端均检出（明1307/暗1282像素）且复位0；使用同一实例，检错轨迹和主轨迹两侧一致。

原始自然清除FAIL保留：候选与原版清除帧间Light1点、Dark6点，maxDelta均1。Light坐标(275,90)；Dark为(275,90)、(599,670)、(598,672/674/676/677)。原版自身initial→cleared为4/8点，候选为3/2点；所有原始计数保留，不写成0。对应直接绘制的新旧/自身清除为0。这里只支持实际资源/几何恢复与局部合成历史边缘证据，不能宣称最终合成器严格恢复已通过或根因全部解释。保留CT25继续调查，产品没有重绘补丁。

作者已查看明色原/候选error配置、暗色普通配置：中心表面正确取三个owner各自颜色，四variant和L/R内容保留；默认画面没有变化。

19项专项、当前Mac managed/NativeAOT各325、NET8零警告、临时源/工程/89媒体清理及精确三模块148项通过。当前申请：仅表内8个配置场景及2个自然清除边缘场景；不含其他字体/模式/弹层/平台、完整CT13、Git或通用像素容差。
