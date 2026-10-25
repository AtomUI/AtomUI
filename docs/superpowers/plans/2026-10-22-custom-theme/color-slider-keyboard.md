# CT19 C10：单分量滑轨键盘短设计（已获批准）

目的：普通／渐变公开View内的第三分量和Alpha滑轨已经能自然获得焦点，但RangeBase继承链没有键盘数值操作；让真实焦点下的既有数值滑轨可操作，保留完整面板与指针行为。

## 已核对的当前合同

`ColorSlider`与渐变多stop的`GradientColorSlider`都继承`AbstractColorSlider`。基类已Focusable=true，并继承RangeBase的SmallChange=1／LargeChange=10。ColorSlider把HSV的Hue映射为0–359，S／V／Alpha映射为0–100，RGBA映射为0–255；Value变化已有颜色更新与事件路径。实际thumb是可聚焦的逻辑／视觉子节点，键盘须能沿既有冒泡到达slider，但不能执行两次。

本地比较来源：当前AtomUI普通Slider的OnKeyDown，以及`/Users/chinboy/Projects/ReferenceProjects/Avalonia`、commit8eeda4f6f546165b3f72e63c9f42247abb306905的Slider／RangeBase／ColorSlider。外部参考不替代本项目默认和交互审查。

## 推荐方案与范围

1. 只在具体ColorSlider接通键盘，保持继承链、焦点顺序、模板、指针、尺寸、剪裁、位图生命周期及默认焦点视觉；不在共享AbstractColorSlider处理，避免隐式改变多stop渐变轨道。
2. 未修饰方向键按SmallChange移动：LTR Left／Down减，Right／Up增；RTL仅反转水平键以匹配实际可见轴。以真实thumb绝对坐标和原始输入证明方向，不仅比较Value。
3. PageDown／PageUp按LargeChange减／增，Home／End到Minimum／Maximum；边界钳制、不wrap。HSV步长单位分别是度／百分点，RGBA是字节；不复制色谱的本地化Ctrl命名颜色算法。
4. Ctrl／Alt／Shift修饰键、已经Handled的事件及非数值键保持原路由，不截获输入框快捷键。禁用时不更改数值；识别出的数值键在边界也只处理一次。通过SetCurrentValue保留现有绑定与颜色通知路径，不新建同步状态或重复发事件。
5. 实际slider和其thumb自然获得焦点时均使用同一冒泡处理入口；不强设内部焦点、移除Tab节点或更换模板规避。渐变stop本身的移动／增删键盘不在本切片。

另两种实现形态不适合本范围：改继承Avalonia Slider会改变继承API与主题／轨道合同；在AbstractColorSlider处理会同步改变多stop滑轨。具体ColorSlider局部入口能保持现有结构，不新增公开签名、Token、Part、字段或订阅。

## 验证与交付

- 在所属Temporary目录取得原始输入已送达但数值不变的有效反例；两View、第三分量六模式剩余H／S／V及Alpha、LTR／RTL的方向与几何、普通／大步长、Home／End／边界、事件次数和未编辑分量。
- 保留原焦点树、实际指针拖拽、邻接输入框快捷键、禁用恢复、重模板与同实例离树重挂；RGBA额外用既有内部控制测试入口验证数值单位，不新增产品公开API。
- 正常动效下明暗新旧默认／无操作焦点／实际键盘／恢复，独立A/A及1DIP；新增可见差异按主题规范另审，不继承C9的有限噪声。
- 当前Mac managed／NativeAOT实际输入，异常或缺失事件保留；临时测试／驱动／媒体按规范清理后精确ColorPicker模块、NET8 Release、完整成员审计和作者另步自审。

2026-10-25用户明确“批准 C10 推荐短设计”，授权本文L2新增交互的实施；没有Git授权，不代表完整CT19／CT25完成。沿当前checkout和分支，不创建隔离副本或worktree，不委派，不发布NuGet。
