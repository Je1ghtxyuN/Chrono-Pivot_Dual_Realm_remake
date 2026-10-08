# Windows SteamVR 人物操作

新版 Unity 工程：`D:\_work\_SRTP\ChronoPivot_ReportVR`（当前保留工程）。

## 启动

1. 在 SteamVR 中完成头显及两个手柄的连接，并确认地面高度和活动区域。
2. 在 SteamVR 设置 → OpenXR 中将 SteamVR 设为当前 OpenXR 运行时。
3. 运行 `Builds/Windows/ChronoPivot_ReportVR.exe`。串流软件需要将头显和手柄提供给 SteamVR。
4. Unity 中调试时，请打开上述新版工程；选择第一关 `Assets/ReportScenesLowPoly/Scenes/Fig_2_01_Introduction.unity` 后进入 Play。

工程通过 Unity OpenXR 接入 SteamVR，目标为 Windows x64 / D3D11；无需另装 SteamVR Unity SDK。已配置 Touch、Index、Vive 和 Khronos Simple 控制器 profile。Vive 使用触控板轴，且没有 A/X 面板键；跨关卡以完成任务后走入出口为主。

## 操作

| 输入 | 行为 |
| --- | --- |
| 头显 | 控制第一人称位置、观察方向和人物头部 |
| 双手柄 | 控制人物双手；握住机关后移动或旋转手柄进行调整 |
| 左摇杆 / 触控板 | 按观察方向移动 |
| 右摇杆 / 触控板 | 分段转向，默认每次 30° |
| 左 / 右握把 | 抓取附近物品或沿手柄方向抓取；保持握住以携带 |
| 挥动手柄后松握把 | 依实际手部运动速度抛出物品 |
| 右扳机 | 对准交互对象进行使用；不会切换场景 |
| 右 B / 右菜单键 | 打开 / 关闭 UI；菜单中左右扳机操作按钮及滑条 |
| 左 B / 菜单键 | 重置当前关卡 |
| 右 A / 左 X | 通关后切换下一关 / 上一关；调试自由切换需启用 `allowDebugNavigation` |

头显暂时丢失跟踪时保持 VR 模式并暂停移动与转向；跟踪恢复后继续。手柄丢失跟踪会松开物品，并将未跟踪的手放回休息姿态；恢复后须先松开握把再重新抓取。

第一人称只显示手掌和手指，不显示手臂或腿部；完整人物仍用于身体投影。双手沿用原有手柄跟踪和抓取绑定。`Report VR / Update First Person Hands Only` 可单独更新所有关卡共用的双手网格，不重建人物或场景。

## 编辑器调参

选择 `Report VR Origin`：

- `DesktopPlayer`：操作模式 Auto / Desktop / VR；键鼠速度、鼠标灵敏度；同一 Inspector 下方可调整 VR 行走速度、分段转向角度和间隔。
- `CharacterController`：碰撞半径、台阶高度及斜坡限制；VR 胶囊由 XRI 随头显高度和位置调整。
- 左右 Controller 的 `ReportGrabInteractor`：近距离范围、远距离范围和碰撞层。
- 物件的 `ReportGrabbable`：释放速度与角速度上限、挂点和 XRI 抛掷平滑参数。
- 人物的 `LowPolyPlayerAvatar`：身高范围、肩位、手腕偏移、桌面休息手位和步态。

停止 Play 后保存场景，才能保留正式参数。`Report VR / Update VR Player Controls` 可更新全部关卡的输入绑定和跟踪设置，保留已有场景模型及数值。

## 键鼠调试

运行 `Builds/WindowsDesktop/ChronoPivot_ReportVR.exe -desktop` 可使用不启动 OpenXR 的调试版本。WASD 移动、鼠标观察；左键 / E 抓取；按住 F / 右键蓄力，松开抛出；R 重置；Esc 解锁鼠标。

启用新 UI 后，Esc 打开 / 关闭暂停菜单，Tab 查看任务；关闭菜单会恢复鼠标观察。界面说明见 `Documentation/Game_UI.md`。

## 验证范围

Unity Play Mode 使用虚拟 XR 设备检查实际人物脚本、抓取及物理抛掷。结果位于 `Documentation/Verification/grab-playtest.txt` 和 `vr-tracking-all-scenes.txt`。实际头显、SteamVR 合成器及串流延迟仍需真机确认。
