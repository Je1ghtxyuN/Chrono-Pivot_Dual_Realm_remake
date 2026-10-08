# 2026-10-06 关卡状态与第一关操作调整

## 场景变化

- 15个关卡的任务白色底板、旧状态按钮台与提示按钮已移除。
- 悬浮文字保留步骤名称、未开始/进行中/已完成状态；第一关显示0/3进度。
- 文字绕竖直轴朝向玩家，使用亮色与深色描边；菜单删除提示选项。
- 第一关木、石、铜三件材料缩至14厘米见方，独立碰撞、刚体、抓取与名称文字保留。
- 第一关按顺序完成：分别拿起三件材料、分别自由抛到金色边框落点区、原有太阳转至东方。
- 移除第一关旧归位槽和标定发射器。落点区没有发射位置限制；近放、未释放和同件重复命中无法代替三件投掷。

## 编辑器参数

- 材料物件：Transform缩放、Rigidbody质量、ReportGrabbable释放速度上限。
- 第一关任务：ReportLearningTask目标数量、稳定时长。
- 投掷区：BoxCollider大小；ReportThrowTarget最小水平飞行距离、飞行时间、速度范围、可接收样品。
- 浮空文字：TextMesh字号/颜色，ReportTextFacing朝向设置。

## 验证文件

- verification.txt：15场景状态与提示入口检查、字体shader编译。
- task-preservation.json：与本轮修改前的场景备份对比，仅第一关前两步任务条件改变，第三步太阳与光照参数保持一致。
- ../../Verification/natural-first-playtest.txt：第一关Play Mode交互结果。
- index.html：第一关与第二关状态截图。

Play Mode使用模拟头显与手柄；仍需要在真实SteamVR串流设备上检查手感。