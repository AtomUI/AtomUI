# CT19 C8：色谱分量配置短设计

用户已明确批准“按推荐短设计实施（SV默认）”。沿既有 `feature/custom-theme` 和当前 checkout 由主 agent 执行；不创建分支或 worktree，不自动提交，不发布包。

`AbstractColorPickerView.ColorSpectrumComponents` 的公开默认值由 `HueSaturation` 改为 `SaturationValue`。这样属性读取与当前实际默认画面一致，保持既有二维饱和度／明度面板及独立色相滑块。调用方读取默认值将观察到新枚举值，必须在控件实现文档与 changelog 中说明。参考采用本地 Ant Design 源码 `211c6ed29b0d2abd18485e293681f8a4e415240b` 的 HSV 面板：横轴饱和度、纵轴明度，色相另用滑轨；不将 AtomUI 的六模式枚举描述为 Ant Design API。

普通与渐变 View 的实际 `ColorSpectrum.Components` 都通过 `TemplateBinding` 消费该公开配置。现有 `ThirdComponent` 已按模式计算剩余分量，在各自 ControlTemplate 中用显式编译绑定连接第三滑块的 `ColorComponent`，不新增公开签名、Token、PART、转换器或代码绑定。

| 二维模式 | 第三滑块 |
| --- | --- |
| HueSaturation / SaturationHue | Value |
| HueValue / ValueHue | Saturation |
| SaturationValue / ValueSaturation | Hue |

模式切换不得改写当前 HSV、Alpha、格式、确认模式和现有输入状态。实际指针输入应修改被操作的轴／分量，保持其他分量。模板替换后新部件继续消费配置，旧部件解除绑定；两类 View 保留原几何、滑轨、输入、动效和默认像素。现有键盘可达性缺口单列后续工作，不包含在 C8 中。

验收先用所属模块的临时检查建立旧版 RED，再验证六模式、往返和实际输入；默认亮暗画面按同轨迹整图精确比较，保留 A/A 与 1 DIP 正对照。显式模式首次生效的预期视觉变化另供审查，不扩大 C1–C7 的合成差异批准。真实 Mac managed／NativeAOT 检查编译绑定和输入，其他平台由已授权 CI 验证；功能成功不等同原生最终像素或完整家族通过。

## C8 脚本化验收点

- AC8-1（批准的兼容策略）：产品 Window 1200×820、scale1、有效 Light／Dark，每个场景保留普通与渐变 View、原调色板／输入／Alpha滑轨／渐变stop及正常动效；初始默认 SV、六个显式模式、ClearValue 恢复。断言默认及恢复新旧整图相同，原实例身份不变。首次800×620准备宿主裁掉了第二个View，因此撤回其完整视觉覆盖结论；正式宿主仅扩展外层窗口，并新增两个View完整可见性断言，不改控件内部几何。
- AC8-2（公开配置合同）：相同完整历史分别执行 main、独立 A/A、独立左间距24→25 DIP角色；真实合成帧等待既有Transition与至少650ms，并确认连续帧稳定。比较全幅精确RGBA，记录全部真实差异及区域，不能以直接Skia诊断替代主比较。
- AC8-3（交互合同）：各模式下通过框架指针点击二维面板与第三滑轨，证明命中和事件到达；按模式名对应的 H／S／V 轴独立计算结果，Alpha与未操作分量保持。输入前以真实布局／帧确认就绪，不能改内部节点、吞失败或用直接设值替代指针。
- AC8-4（生命周期与 AOT）：真实 Mac managed／NativeAOT 各执行六模式、恢复及实际指针合同，记录原生句柄、缩放、完整 HSV、实际部件枚举和输入事件。模板重建与离树恢复用所属模块临时检查验证；无新增键盘支持或原生最终像素结论。
