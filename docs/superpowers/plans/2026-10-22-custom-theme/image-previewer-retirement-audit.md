# CT15 ImagePreviewer 本体退出核对

复用13metadata/14Own、104限定视觉、既有117个loader/lease/集合退出检查。本轮只补同provider/window持续存活且调用方保留ObservableCollection与借入图片时，single/group打开后关闭移除或直接移除的本体GC。保留原完整标题/借用Source身份、正常动效/真实公开打开；不把lease字段已空等同owner回收。借入图片由调用方最后释放，不能为了回收而清集合、dispose调用方图片或关窗口。

四临时场景在Temporary/CT15ImageRetirement；按实际Host记录，不将Headless Window冒充原生NSWindow/Browser。先证实际公开IsOpen/当前图片loaded、cross roots后按原退出条件。失败先分持有链，不改loader/来源所有权/缩放/滚动/API/官方依赖；清理后仅精确ImagePreviewer模块。


四不同场景Debug4PASS/0Skip/17.7252s，Release4PASS/0Skip/17.5788s；实际cross root均ImagePreviewerDialog（当前Headless Window，不是NSWindow/Browser）。公开IsOpen/IsCurrentLoaded/cross root打开判据均通过；关闭移除或打开直接移除后root退出且owner可回收。调用方32×16 borrowed WriteableBitmap和ObservableCollection仍保持活着，原标题/Source不变，退出后调用方继续Add成功；不释放调用方图片/不清集合制造回收。

无产品失败/源码修改。4临时case/1源3877字节已删除，无媒体；清理后真实重建精确ImagePreviewer117PASS/0Skip/15.2674s。当前产品源码未变，复用同源NET8零警告零错误16.76s，git diff --check通过。最终会话/marker读取后清理，作者另步自审非独立审查。该四条本体GC不关闭旧171旧新/29静态及64持续态cross非零、平台/极值或完整CT15。
