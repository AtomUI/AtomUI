# CT-10 共享主题消费链实施计划

继续既有feature/custom-theme，不创建新分支/worktree，不自动提交、不用subagent。以[渲染合同](../../../architecture/systems/theming/control-rendering.md)、[消费规范](../../../engineering/development/theming/consumption.md)和[CT-02前证](baseline.md)为依据；控制默认/既有合法配置的视觉保持，失效输入修复单独列入兼容表。

## 审计与归属

本包是共享消费正确性及生命周期改造，不是纯成员整理。ArrowIndicator公开4个StyledProperty，80余行；geometry与pen由实例持有，无订阅；ApplyTemplate只构建一次geometry，ArrowSize只使Measure失效而没有清除缓存，B06已证实8→16仍8×4。1.5是箭头圆弧半径，属于需核定开放边界的形态值，不伪装成全局Token已经支持。

PopupRoot/OverlayPopupHost内置主题直接设Shared FontSize/Family与Normal Weight/Style；存在自然继承与PlacementTarget字体的职责差异，须按实际逻辑/视觉树和owner证明根因，不能一开始加代码绑定覆盖原生优先级。独立host已有ThemeContext lease，但这不等于排版已接通。涉及跨根binding时必须先证明ControlTheme不可表达并定义Open/Close/owner迁移释放；不改变Host选择/尺寸/遮罩/滚动。

InputControlFrame为internal共享状态owner，312行；AddOnDecoratedBox为内部框体布局owner，922行，不能新建第二套状态机。InputControlFrame模板已有Padding/BorderThickness/CornerRadius TemplateBinding，但共享主题会给内部root设Global默认；外层真实Control输入投射是否到位须逐适用owner核定。组合公开子Control继续保留自身identity，不把Shared机械换成内部frame身份。成员审计、现有Input/Addon/Select/Cascader/TreeSelect测试与Gallery文档影响在触及文件前逐链登记。

Icon、Splash、TextSelectionHandle及VisualBrush文字还需核对owner、有效Global、完整typeface、绘制/测量缓存和动态释放。公共控制/内部对象区分沿用目录，不为无Own对象创造identity。运行/视觉结果只覆盖实际已验证对象。

## 执行包

### S1 箭头缓存与形态

- [x] 保留原B06的同实例8→16→8；临时验证前后Measure、Render输入、默认及re-apply，不新增常驻测试。
- [x] 最小根因修正geometry依赖/测量/绘制失效；不新建几何服务或并行Token状态。将写死1.5移为兼容新增的`ArrowIndicator.TipRadius` StyledProperty，默认1.5 DIP、0为尖角、非负有限值；geometry/measure/render共同依赖，允许生成专用Part Style设置。无需给primitive新增Own identity；父owner既有Token/Part分工保持。默认形状与几何算法不改。正式API及算法归属写入arrow-indicator控件文档，并同步Tooltip arrow Part合同。
- [x] 覆盖实际箭头消费路径及stroke/fill更新；默认Skia前后0差异，1DIP检错、动态形态变化/恢复和原始尺寸/方向合同。

### S2 跨根字体

- [x] 真实PopupRoot与Overlay，owner FontFamily/Size/Weight/Style同实例A→B→A前证；附逻辑/视觉/继承父链及有效值来源，定位主题Setter还是继承/owner链问题。
- [x] 优先ControlTheme声明式投射/继承；确需跨根代码绑定才显式注释原因、优先级及对称释放。保留显式popup/内容样式覆盖、嵌套/兄弟scope和独立窗口默认。
- [x] 已打开更新、重开、PlacementTarget/owner更换、detach/re-template/关闭和WeakReference；默认前后像素0、声明字体实际像素与值一致。

### S3 框体与组合投射

- [x] 建立InputControlFrame/AddOn与每个public owner的输入→默认/状态→真实目标表，先跑既有失败B03和对应状态/variant/compact/addon/size。
- [ ] 正确Effective Global来自公开owner；root Padding/BorderThickness/CornerRadius/排版及状态落到真实frame；等价关系迁至AXAML，避免本地值压制实例或专用Style。
- [x] 保持默认视觉及输入/焦点/选择/尺寸/裁剪/滚动；Custom只退出接管维度。适用owner清单完整，不用单LineEdit通过替代Select等分支。

几何与组合投射已验收，见[框体结果](shared-consumers-frame-results.md)。完整根排版/前景消费与 S4 合并核对后再勾选第二项。

### S4 Icon/文字/其他共享对象

- [ ] 审核IconPresenter/IconTemplate/VisualBrush、Splash、TextSelectionHandle、绘制缓存与字体；每条真实输入/owner/释放链记录并按失败前证修复。
- [ ] 明确共享和Control Effective Global取值；无独立Own对象保持已有identity边界；Theme/Part定制沿原生优先级。

### S5 出口

- [ ] 实际消费者清单、每条修复/未决项及兼容表；更新对应正式模块/控件文档和必要Gallery，不留“setter存在但不消费”。
- [ ] 逐包脚本化默认前后0差异、动态A→B→A、值/几何/实际像素、必要输入和释放；临时源/驱动/媒体/专用产物清理。
- [ ] 清理后仅受影响模块精确filter及必要NET8；作者自审，更新tasks/results。整体CT-10仅在全部上述包验收后完成。

## 已授权修复的兼容边界

| 输入 | 旧错误 | 目标 | 默认保持 |
| --- | --- | --- | --- |
| 同ArrowIndicator尺寸8→16→8 | 已建geometry仍8×4 | 既有公开ArrowSize使真实几何/尺寸更新再恢复 | 未改尺寸时原几何完全保持 |
| owner已打开弹层后字体变化 | B07保留14/Normal/Normal/旧Family | 实际owner完整typeface在两类host持续更新，显式定制按原生优先级 | 默认字体/默认Host合同不变 |
| 外层输入Padding11,13/边宽3/radius0 | B03实际frame仍11,4/1/6 | 对应公开输入到达适用真实frame | 默认输入下前后无差异 |

S1–S4细节按有效前证逐项收敛并记录ruling；本计划不把未调查的分支算已完成，也不授权降低原始UX来规避症状。

S1实际失败、兼容和清理证据见[箭头结果](shared-consumers-arrow-results.md)。

S2实际失败、逐阶段兼容、原生缓存边界和清理证据见[跨根字体结果](shared-consumers-font-results.md)。

S4 已验收分包见 [图标、文字与共享绘制结果](shared-consumers-typography-results.md)。S3 全 owner 字体/字段与 S5 总出口保持开放；CT-11 可在已就绪共享机制上独立推进，家族逐项验证完成后回填这两个出口，不把包级结果冒充全 owner 完成。
