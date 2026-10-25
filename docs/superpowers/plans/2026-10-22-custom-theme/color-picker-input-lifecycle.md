# CT19 C6：输入框同实例重挂同步

沿现有feature/custom-theme和checkout执行，不创建分支／worktree／子agent，不自动提交。C6不包含在第十批17文件提交中。沿用既有执行计划、系统排障、TDD、控件优化／资源生命周期和脚本化视觉验收；本项为正确性修复，无性能提升主张。

## 真实问题与合同

C5重挂60帧显示：公开ColorPickerView离树后Value改Blue，回到同一host、尺寸不变时Value、Spectrum和Alpha为Blue，但HEX仍为White；原版和C5候选均存在。输入ColorValue／Format只在attached时刷新，OnAttached仅恢复确认输入监听。

保留800×620、View宽300、原色谱、Alpha、格式选择、确认区和模板布局。外部颜色／格式在离树期间变化须回放；没有权威输入改变时保留无效／有效草稿、校验与光标。离树White→Blue→White仍是外部覆盖，不因最终值相同跳过同步。

## 执行步骤

1. 已检索PinnedOpen／SemanticPart测试，其不覆盖此合同；用所属Desktop测试项目Temporary/CT19InputLifecycle访问真实internal Input和现有Headless/Skia宿主，记录原版失败。
2. 覆盖HEX／HSV／RGB、确认两态、离树格式改变、A→B→A、无修改草稿／光标和格式绑定双向行为。成员预审files1/controls1/issues0，无纯布局修复需要。
3. 以最小生命周期状态回放修复，不新增ignore/suppress旗标。能等价表达的格式选择接线优先迁移到ControlTheme原生SelectedValueBinding／TemplateBinding；实际编译及运行证明。编辑字段不能以无条件展示绑定覆盖无效草稿。
4. 原场景红绿验证及真实Mac managed／NativeAOT、指针／键盘编辑、重模板／生命周期专项；完整明暗新旧A/A、1DIP及恢复图，不把C5的像素批准扩大到输入文字。
5. 默认删除临时测试／探针／工程／媒体；保留数值与SHA，清理后仅精确相关模块、NET8 Release、成员审计和diff检查；同步文档后交审。

## 边界

单独GradientColorPickerView的Value不等于选择首stop，本项不能以该假设制造失败。公共Gradient宿主／活动stop路径按实际合同验证。全家族及CT19仍未完成。

## 进行中的证据

原12项3通过9失败，候选12通过；扩展至18项后原5通过13失败、候选18通过（7.8236／7.6452秒）。无外部变化保留文字、数值无效草稿、校验及光标；A→B→A覆盖草稿；三格式／确认两态、格式绑定、重复挂载事件、实际Headless原始点击和文字均有断言。两产品文件SHA为ab2c9d86fb09／a4061aecd87a，候选代码没有修改输入几何。

Mac真实NSWindow、scale2，明暗三格式，managed／NativeAOT各308项通过；原始指针和文字事件走框架输入管理器。模板重挂后等待两个实际RequestAnimationFrame，原未提交场景的命中先为窗口容器、提交后为编辑器后代。公开ColorPickerView以ValueChanged报告编辑颜色，不能把外部控制的Value未自动改写当失败。原生工程420文件已删除，源码及产品SHA、结果保留；AOT先缺ssl的链接失败及添加本机openssl/brotli路径后的目标OS警告均保留，不声明旧macOS支持。

初次同步采样仅RunJobs＋render tick会让Button动画停留中间颜色；450ms采样和独立1500ms诊断均复现，不能据稳定的错误终态验收。保持200ms动效不变、仅改驱动为正常DispatcherFrame泵送后，约204–210ms到达正确终值，300–1500ms各组真实帧SHA稳定。旧采样媒体清理，数值和动画画刷轨迹保留。确认草稿的恢复基线也改为显式Black→White建立非空White，避免把构造时空草稿与显式White混比。

正常泵送的216帧中，72个1DIP正对照全部检出；仍有10组A/A的1–5像素圆角差异及恢复时2–22像素，旧新只有两组HEX恢复出现1／3像素，其余默认／草稿旧新均0；12组实际文字／确认可用状态变化已计数。完整比较和自然compositor计数保留，正在独立公开树／Bitmap SHA及直接Skia重绘诊断；尚未批准这些新增视觉变化，不把C1–C5范围扩大到本场景，也未清理当前待审媒体或声明完整视觉通过。

独立288帧诊断已完成：直接Skia的72组A/A、24组草稿／White恢复、36组旧新预期全部满足，未经对齐、裁剪、遮罩或容差，非目标阶段直接重绘差异为0。公开树按格式有492／625／654节点，72组A/A的全部几何、变换、画刷、字体及ImageBrush Bitmap SHA相同，仅各实例自动生成的Radio分组名不同，原值完整保留。恢复轨迹并非声称所有原始属性相同：ColorBlock透明背景由null变显式Transparent（透明度0），部分HSV最小值spinner启用／图标画刷状态因编辑历史不同；格式迁移也让ComboBox.SelectedValue从Item变为Tag值并有编译绑定。直接渲染仍0，不能据此删除或遮住compositor原始差异。普通无需确认的White→离树Blue→回树→White也在单独完整矩阵验证，避免只审查无效草稿覆盖场景。

补充重模板旧selector解绑及关闭整图GC后，最终临时20项全部通过（8.9373秒），旧selector不再改动当前格式，当前selector仍双向生效，view／input／selector／host四个WeakReference均释放。

## C6 分包结案

用户已明确允许输入同步修复，并接受[审查表](color-picker-input-lifecycle-results.md)内59对限定合成差异；批准不覆盖Git提交、原生最终像素或完整家族。正式324帧、108组1DIP全部检出，独立直接Skia的72组A/A和24组恢复均0；自然compositor真实计数保留，未加遮罩或通用容差。

临时测试及四个可执行驱动共六份源码、原生工程420文件已删除。三个已批准的正式／直接／普通输入SVA run均完成audit与cleanup，媒体删除，SHA、每对坐标／RGBA／上限和数字证据保留于外部SDD。清理后Desktop相关23项通过（5.1405秒），ColorPicker NET8 Release零警告／错误（32.89秒），成员审计1文件／1控件／0问题。C6切片完成，CT19和CT25完整出口仍开放；没有创建提交。
