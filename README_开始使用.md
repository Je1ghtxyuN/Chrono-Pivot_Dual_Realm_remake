# 晷枢双界 · 中期报告 Windows VR 工程

工程目录：`D:/_work/_SRTP/ChronoPivot_ReportVR`

使用 **Unity 2022.3.53f1c1** 打开。运行平台为 **Windows x86_64**，由电脑渲染，通过 **SteamVR 的 OpenXR 运行时**连接或串流到头显。

## 已包含

- 全部 14 个中期报告场景，以及上一轮独立制作的古代园林，共 15 个场景。
- 两套自建低多边形模型、材质和预制体，保留原始 GUID；没有引入 SEU 原有模型或参考仓库的模型。
- 每个场景独立的 XR Origin、地面高度跟踪、头显与左右手柄姿态跟踪。
- XR Interaction Toolkit 的连续移动、30° 分段转向、重力和 Character Controller；场景网格已添加静态碰撞。
- 按报告顺序循环浏览全部场景，电脑窗口也有场景选择列表。
- 原报告摄影机保留但关闭，运行时仅 VR 相机渲染。
- Ch46 主操控角色，青绿/米白/墨色低光泽材质，头手骨骼跟踪与行走摆腿；人物预制体和展示场景在 `Assets/ReportVR/Characters`。
- 桌面调试：WASD 行走、鼠标转向、左键／E 交互，与 VR 共用关卡碰撞和任务。
- `Documentation/ReportPhotos` 的 19 张报告图片、`Documentation/GardenPhotos` 的 4 张园林图片。

场景已整理成可行走的关卡路线，每关有入口、三步演示任务、观察点和出口。已经接入浮力配重、闸门、共振调频、铜镜角度、电磁铁芯与门锁、管道密封、频率耦合、雪球抛射、多米诺和日晷等基础演示；展览关卡使用观察或旋转任务。详细范围见 `Documentation/Demo_关卡与脚本.md`。

## SteamVR 串流启动顺序

1. 在电脑上启动 SteamVR，让头显通过 Steam Link、Virtual Desktop、PICO 的串流软件或对应设备软件进入 SteamVR，并确认头显和手柄在 SteamVR 中正常跟踪。
2. 在 SteamVR 的设置中找到 OpenXR 页面，将 **SteamVR 设为当前 OpenXR 运行时**。不同版本的设置布局可能不同。
3. 运行 `Launch_VR.cmd`，或直接打开 `Builds/Windows/ChronoPivot_ReportVR.exe`。
4. 头显内进入图 2.1 引导场景。电脑窗口显示对应的镜像和场景选择工具。

新工程使用 OpenXR 连接 SteamVR，不依赖 Valve 的旧 SteamVR Unity Plugin。串流由设备软件和 SteamVR 完成；工程本身不内置串流服务。

如果在 Unity 中调试，同样先连接 SteamVR，再打开 `Assets/ReportScenesLowPoly/Scenes/Fig_2_01_Introduction.unity`，点击 Play。

## 操作

| 输入 | 操作 |
|---|---|
| 左手摇杆 / 触控板 | 沿视线方向行走，速度 1.6 米/秒 |
| 右手摇杆 / 触控板左右 | 每次转向 30° |
| 右手扳机，或右手主按钮（通常 A） | 下一场景 |
| 左手扳机，或右手次按钮（通常 B） | 上一场景 |
| 电脑 Page Down / Page Up | 下一 / 上一场景 |
| 右手握把键 | 指向控制台按钮并交互；有效目标的射线变为金色 |
| 左手菜单键，或左手次按钮 | 重置当前关卡、模型与出生位置 |
| 电脑 E / R | 沿视线交互 / 重置（没有手柄时使用 VR 相机视线） |
| 电脑窗口 Scenes 按钮 | 选择任意场景 |
| 桌面模式 WASD / 鼠标 | 行走 / 转向；斜向移动保持相同速度 |
| 桌面模式左键 / E | 沿屏幕中心视线交互；有效距离 4 米 |
| 桌面模式 Esc | 释放鼠标；随后可点击场景菜单 |
| VR 模式电脑 Esc | 退出程序 |

完成三个节点后，进入出口会切换下一关。手动浏览按钮仍可切换任意场景。切换后需松开按钮才能再次切换，避免长按连续跳过场景。左手腕旁显示操作提示。

## 无头显调试与人物

运行 `Launch_Desktop.cmd` 强制使用键鼠，跳过 SteamVR 初始化。点击画面中菜单以外的区域锁定鼠标，WASD 移动、鼠标转向、左键／E 操作、R 恢复机关与出生视角。Esc 解锁后停止行走和交互，便于操作左上角菜单；再次点击画面继续。切关后重新点击画面。关闭程序可用窗口关闭按钮或 Alt+F4。

普通启动由 `ReportXRStartup` 手动初始化 OpenXR，未检测到头显跟踪时使用桌面模式；检测到头显后恢复 VR 驱动。Unity 中直接 Play 也可调试。不要开启 XR 设置的自动初始化。

`Launch_Desktop.cmd` 使用 `Builds/WindowsDesktop` 中的同内容副本，并移除该副本 boot.config 的原生 OpenXR 预初始化项，避免 Unity 在 C# 运行前探测 SteamVR。VR 程序在 `Builds/Windows`。两者使用相同关卡和脚本，构建菜单会同时更新。桌面副本应通过启动脚本启动，以传入 `-desktop`。

主操控角色采用用户提供的 `Assets/character/Ch46_nonPBR.fbx`，保留原造型及脸部细节，配色统一到青绿、米白、墨色与棕色。`Ch46Player.prefab` 已绑定 15 个关卡，使用现有头手目标和双段手臂求解；没有联网、表情、抓取手指或完整脚部 IK。第一人称头身仅投影，四肢单独显示。预览场景为 `Assets/ReportVR/Characters/Ch46Preview.unity`，图片为 `Documentation/CharacterPreviews/Ch46Player.png`。旧自建 `JadeScholar.prefab` 保留为备用资产。

## 场景与构建顺序

| 构建序号 | 图号 / 内容 | 场景 |
|---|---|---|
| 0 | 2.1 基础引导 | [Fig_2_01_Introduction](Assets/ReportScenesLowPoly/Scenes/Fig_2_01_Introduction.unity) |
| 1 | 2.2 浮力压强与水渠 | [Fig_2_02_Buoyancy_Canal](Assets/ReportScenesLowPoly/Scenes/Fig_2_02_Buoyancy_Canal.unity) |
| 2 | 2.3 编钟与声学 | [Fig_2_03_Acoustic_Resonance](Assets/ReportScenesLowPoly/Scenes/Fig_2_03_Acoustic_Resonance.unity) |
| 3 | 2.4 地磁与光学观星台 | [Fig_2_04_Geomagnetic_Optics](Assets/ReportScenesLowPoly/Scenes/Fig_2_04_Geomagnetic_Optics.unity) |
| 4 | 2.5 电磁锁 | [Fig_2_05_Electromagnetic_Lock](Assets/ReportScenesLowPoly/Scenes/Fig_2_05_Electromagnetic_Lock.unity) |
| 5 | 2.6 液压管网 | [Fig_2_06_Hydraulic_Pipework](Assets/ReportScenesLowPoly/Scenes/Fig_2_06_Hydraulic_Pipework.unity) |
| 6 | 2.7 双人频率耦合布景 | [Fig_2_07_Frequency_Coupling](Assets/ReportScenesLowPoly/Scenes/Fig_2_07_Frequency_Coupling.unity) |
| 7 | 2.8 冰雪竞技场 | [Fig_2_08_Snow_Arena](Assets/ReportScenesLowPoly/Scenes/Fig_2_08_Snow_Arena.unity) |
| 8 | 2.9 创意工坊 | [Fig_2_09_Creative_Workshop](Assets/ReportScenesLowPoly/Scenes/Fig_2_09_Creative_Workshop.unity) |
| 9 | 2.10 综合实验室 | [Fig_2_10_Physics_Laboratory](Assets/ReportScenesLowPoly/Scenes/Fig_2_10_Physics_Laboratory.unity) |
| 10 | 2.11 园林水渠总览 | [Fig_2_11_Garden_Canal_Design](Assets/ReportScenesLowPoly/Scenes/Fig_2_11_Garden_Canal_Design.unity) |
| 11 | 2.12 天体与日晷 | [Fig_2_12_Celestial_Sundial](Assets/ReportScenesLowPoly/Scenes/Fig_2_12_Celestial_Sundial.unity) |
| 12 | 2.13 冰雪资产展示 | [Fig_2_13_Winter_Asset_Design](Assets/ReportScenesLowPoly/Scenes/Fig_2_13_Winter_Asset_Design.unity) |
| 13 | 2.15 自建资产展厅 | [Fig_2_15_SelfModeled_Asset_Collection](Assets/ReportScenesLowPoly/Scenes/Fig_2_15_SelfModeled_Asset_Collection.unity) |
| 14 | 独立古代园林 | [AncientGarden_LowPoly](Assets/ReportGardenLowPoly/AncientGarden_LowPoly.unity) |

## 配置依据

参考：[Je1ghtxyuN/Snowscape-Quest](https://github.com/Je1ghtxyuN/Snowscape-Quest)，读取提交 `a8986a1e3140a53d3bdc82a205cbc432873e124e`。

沿用其 Unity 版本、XR Origin、Input System 和 OpenXR 配置方式，采用相同的 XR 包版本：

- Input System 1.13.1
- XR Interaction Toolkit 3.1.2
- XR Core Utils 2.4.0
- XR Plug-in Management 4.5.1
- OpenXR Plugin 1.14.1

本工程单独创建 Windows 的 OpenXR 设置，不直接复制参考仓库中混有多个平台和重复设置的 XR 资产。启用 Oculus Touch、Valve Index、HTC Vive、Khronos Simple Controller 配置。OpenXR 使用 Single Pass Instanced，图形 API 固定为 Direct3D 11，Linear 色彩空间，4× MSAA。

保留现有自建场景的内置渲染管线以保持原配色和材质；参考仓库的 URP 不是 SteamVR 串流的必要条件。

启用 OpenXR 推荐的 `USE_STICK_CONTROL_THUMBSTICKS` 和 `USE_INPUT_SYSTEM_POSE_CONTROL` 编译定义，使用 Input System 的摇杆与姿态控件。

## 修改和重新构建

- `Assets/ReportVR/Editor/SetupReportVR.cs`：项目设置、XR 输入和场景接入。现有工程已经接入；无需重新运行 Configure 来试玩。
- `Assets/ReportVR/Editor/PolishReportLevels.cs`：关卡布局、独立运动部件、路线验证及拍摄。`Apply` 会从 `Documentation/LevelReview/SceneBackups` 的初始快照重新生成布局；手动编辑场景后不要直接重跑它。
- `Assets/ReportVR/DemoLevel.cs`、`DemoStation.cs`、`DemoInteractor.cs`、`DemoExit.cs`：任务顺序、演示动作、握把射线与完成出口。
- `Assets/ReportVR/ReportSceneBrowser.cs`：场景浏览与电脑窗口选择工具。
- `Assets/ReportVR/Editor/VerifyReportVR.cs`：15 场景接入、出生点、模型、相机和 Windows XR 配置验证。
- `Assets/ReportVR/Editor/OfflineVRCheck.cs`：通过虚拟设备在 Play Mode 中验证真实跟踪组件、行走、转向、扳机切换、循环顺序与长按保护。测试暂时关闭 XR 自动启动，不使用真实头显，结束后恢复项目设置。
- 菜单 `Report VR/Build Windows x64`：验证后输出 Windows 程序。
- `Documentation/Verification/`：验证结果；`VR_scene_index.txt` 记录出生位置。

原 SEU 工程和独立制作工程没有被修改。本工程中的场景副本已经添加 VR 接入与碰撞组件。

`Documentation/LevelReview/After` 包含每关俯视图与入口视角；`SceneBackups` 保留改造前场景。`Documentation/Verification/level-routes.txt` 记录地面、坡度、净空和实际 CharacterController 路线检查。

## 验证范围

已通过的验证结果见 `Documentation/Verification/`。Windows 构建、场景引用、输入绑定和渲染可在本机检查；头显实际跟踪、SteamVR 双眼显示与串流延迟需要连接设备实测，不能由编辑器检查或离线输入测试替代。

离线 Play Mode 已验证 15 个场景的实际加载、头显与双手姿态、摇杆行走和分段转向、握把射线交互、45 个节点的顺序完成、门锁开启、模型姿态和门锁重置、扳机切换、首尾循环以及长按保护，运行时错误为 0。`Documentation/VRPreviews/` 的三张图片是离线模拟头显位置的单目预览，不代表真实头显的双眼输出。

