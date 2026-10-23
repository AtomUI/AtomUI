# CT-13 四个文本输入owner的Global消费结果

本分包默认状态文字颜色已获用户批准，尚有渲染往返差异，不能标记整个视觉验收完成。58个引用迁移（TextBox15/LineEdit13/TextArea18/SearchEdit12），不新增Own/API，保留原Setter条件和优先级。根主题不再绕过owner读取Shared字段；共享frame和其他输入控件仍独立推进。

## 消费与状态验证

四个实际owner配置光标色后仍Shared #e0000000，4项RED1s。迁移后颜色、selection、placeholder、motion、其他owner隔离、本地覆盖和清除均通过。追加Filled/Borderless状态检查时，TextArea最初测ScrollViewer造成两项假失败；其真正文字经ContentLayout继承，已正确消费。实际PART_TextPresenter断言确认LineEdit/SearchEdit4项RED，修正两个主题的12个ScrollViewer精确类型选择器为AtomUI类型后，10项专项PASS1s，包含error/warning/Form warning/native validation优先及恢复。输入模块连临时146项PASS5s。旧TextBox资源架构断言更新为同owner typed入口，保持原契约，不新增常驻用例。

当前Mac arm64 NativeAOT实际16个输入、4个owner的caret/selection/motion配置及清除PASS；未验证平台不填通过。

## 视觉证据与未决项

16实例、四owner、四尺寸、clear/count/placeholder、多行固定高度及原内容；正常motion与真实键盘输入。A/A0，1DIP检错18328/18367。初始idle/disabled/Outlined-error/ready旧新0；hover为2/22个RGB1差异，配置变化10709/10716，返回15/22（maxDelta1/3），没有使用容差、遮罩或裁剪。重复同版本hover/返回A/A均0，返回差异可复现。

严格在原checkout内反转58个引用并恢复原选择器，与新候选逐阶段A/B：idle/hover/disabled/error/ready全部0，返回同阶段22/23；旧实现自身往返9/15，新实现15/22。观察的实际Control Bounds、至Window矩阵、字体大小和Foreground在各对应阶段均相同。四文件SHA256确认精确恢复。以上只定位到绘制/往返链，尚不构成已解释或已修复，继续CT25；不因数据属性相同就宣布像素通过。

候选修复让LineEdit/SearchEdit Filled/Borderless错误/警告文字恢复主题色，每图2946个文字像素；Borderless-error另有搜索图标边缘7/22（maxDelta7/6），仍独立调查。用户已明确回复“允许修复默认错误/警告文字颜色（推荐）”，批准仅恢复LineEdit/SearchEdit默认状态文字颜色；其他边缘与往返差异未获豁免。TextArea原默认状态文字没有这项缺陷。查看了暗色Filled-error及四色配置的完整截图。

## 清理与最终模块证据

10项临时测试、外部工程及455专用文件、三个中间源备份均删除；91个媒体运行文件清理，Git媒体审计clean。清理后Input精确filter136项PASS3s，NET8 Release零警告/错误26.90s（进程31.51s），git diff检查随后记录。此为作者自审，未使用subagent，未提交；CT13/G1完整出口保留开放。
