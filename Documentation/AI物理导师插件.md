# AI 物理导师插件

插件已接入现有实验手册。基础模型路径已设为下载的 Qwen2.5-3B-Instruct，导师服务地址为 `http://127.0.0.1:18080/api/tutor`。本机 Python 后端以 NF4 四位量化加载基础模型和物理 LoRA；Unity 通过 HTTP 调用，不直接加载权重。

导师来源已切换为 `D:/_work/_SRTP/AI物理导师/ai-physics-tutor`。已下载并校验的 Qwen2.5-3B-Instruct 位于该目录的 `models/Qwen2.5-3B-Instruct`，用于配合 `outputs/run_001/best_adapter`。本机 3B NF4 后端及启动脚本已实现。旧目录保留，当前插件引用和测试使用新目录。

## 游戏与 Unity 调试

1. 用 Unity 2022.3.53f1c1 打开本工程，打开任一游戏关卡并点击 Play。
2. 桌面按 Esc，或 VR 按右手 B / 菜单键打开实验手册。
3. 点击侧栏「AI 物理导师」，也可从「当前任务」点击「向导师提问」。
4. 在问题框直接输入文字。先运行导师目录的 `启动导师.cmd`，看到 READY 后发送。清空服务地址时会提示尚未配置；服务未启动时会提示无法连接，不影响关卡进度。
5. 点击「语音输入」开始 Windows 听写，点击「停止听写」结束。最终识别文字进入同一输入框，确认或修改后再发送。语音不会自动发送。

导师页沿用桌面 uGUI 与 VR 世界空间菜单。打开时暂停实验、释放鼠标和抓取，关闭后恢复游戏。切到其他页面、关闭菜单、窗口失焦或销毁组件会停止听写。回答区域支持鼠标滚轮和上/下按钮，VR 扳机可点击按钮。

## 语音配置

当前工程目标为 Windows PC VR，使用 Unity `DictationRecognizer`；语音识别与基础物理模型无关。它依赖 Windows 语音服务，不是离线中文 ASR。

- 在 Windows 设置中允许桌面应用使用麦克风，确认默认录音设备可用。
- VR 串流时，将头显麦克风设为电脑的默认输入设备；也可用电脑麦克风调试。
- 安装所需语言的语音组件，并将 Windows 语音识别语言设为中文。
- 启用 Windows 在线语音识别，保持其所需网络可用。Unity 听写失败时，界面会显示错误原因，文字输入仍可使用。

插件不会自动修改系统权限、默认录音设备或语音语言。Windows 服务在不同系统和地区的可用性，以及真实头显的麦克风转写效果，需要设备实测。

## 基础模型配置与以后接入

共用配置位于：

`Assets/ReportVR/PhysicsTutor/Resources/PhysicsTutorConnection.asset`

可在 Unity Inspector 中编辑：

菜单 `Report VR/AI Tutor/Select Connection Settings` 可以直接选中该配置。

| 字段 | 当前值 | 用途 |
|---|---|---|
| Base Model Path | `D:/_work/_SRTP/AI物理导师/ai-physics-tutor/models/Qwen2.5-3B-Instruct` | 基础模型路径预留；填入路径本身不会启动或加载模型 |
| Service Endpoint | `http://127.0.0.1:18080/api/tutor` | 可运行导师服务的完整 HTTP `/api/tutor` 地址 |

当前默认使用本机 NF4 服务，在导师目录双击 `启动导师.cmd`，看到 READY 后即可发送。Windows VR 由电脑访问该地址。也可在导师页临时填写接口地址进行 Play Mode 联调；临时地址在本次运行的切关之间保留，不保存到配置资产。

客户端兼容新来源目录中 `prototype_delivery` 的协议：发送问题、teach 模式、提示等级 1–3、当前任务与观察状态、最近对话；接收回答和检索证据。对话最多 20 项、总计 10000 字符。新对话会清空历史；切关会创建独立对话。

观察状态包含当前实验目标、关卡进度、现有反馈、当前任务相关读数、操作次数、物件归位情况和已选选项。插件只读取这些状态，不代做实验或改变过关判定。

实际 HTTP 调用（包含失败）保存到 `Application.persistentDataPath/physics_tutor_calls.jsonl`。未配置服务时不会发送请求。连接失败、服务忙、无效回答会明确显示，不作为导师回答加入历史。

## 代码位置

- `Assets/ReportVR/PhysicsTutor/`：共用配置、HTTP 协议与客户端、导师控制器、Windows 听写和导师页。
- `Assets/ReportVR/ReportGameUI.cs`：菜单入口与输入互斥。
- `Assets/ReportVR/Editor/PhysicsTutorPlayCheck.cs`：Unity Play Mode 联调验证。
- `Documentation/Verification/tutor_http_fixture.py`：仅绑定本机 18081 的测试服务，用新来源目录中的导师契约检查请求；不提供真实模型回答。

测试服务只用于验证协议和客户端，不包含在游戏构建中。模型教学效果、真实语音识别与头显体验不由测试响应证明。

## 导出与运行

菜单 `Report VR/AI Tutor/Export Game Plugin` 导出适用于当前游戏工程的 `Builds/PhysicsTutor/ChronoPivot_PhysicsTutor.unitypackage`，包含模块、本机模型和服务路径配置及游戏 UI 的接入修改，不包含模型权重。包内 README 记录所需 Unity 内置网络模块。

Windows 与 WindowsDesktop 构建继续分别由 `Launch_VR.cmd` 和 `Launch_Desktop.cmd` 启动。模型文件保存在 Unity Assets 以外，不进入游戏构建或插件包。

本次导师输入、HTTP 协议、虚拟 VR 操作及原有 UI 回归通过。尝试更新 Windows 构建时，既有全关卡路线检查报告 4 项失败：浮力关的装载托盘阻挡设计路线，声学关的双声源拍频台架阻挡两段路线并造成一次 CharacterController 遍历失败。详见 `Documentation/Verification/level-routes.txt`。本轮没有修改这些场景或跳过构建检查，因此原有 Windows/WindowsDesktop 可执行文件保留原版本；当前导师功能使用 Unity Play 或将插件包导入本游戏工程后运行。

## NF4 真实文字测试

29 项轻量测试通过；3 次真实中文 HTTP 问答和一次 Unity 中文输入、发送与回答显示通过，未使用测试服务响应。另验证输入超过 4096 token 时明确返回 400。模型有 252 个四位量化层，基础权重与适配器文件未改写。

这些是技术联调结果。样本中出现浮力和重力同方向的错误陈述、三级提示不够具体，以及无关检索卡片。物理教学质量未通过本轮样本检查，不能把 JSON 核验当作物理正确性证明；也未进行未量化对照，不能据此把错误归因于量化。原始回答和审阅见 `Documentation/Verification/physics-tutor-nf4-text-review.md`，截图见 `Documentation/UIPreviews/15_AI_Tutor_Real_NF4.png`。

单次输入最多 4096 token、输出最多 512 token；模型逐个处理请求。测试后关闭后台服务释放资源，后续先运行启动脚本再打开 Unity Play。
