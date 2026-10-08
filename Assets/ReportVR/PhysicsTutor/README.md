# 晷枢双界 · AI 物理导师插件

此插件适用于当前 ChronoPivot_ReportVR 工程，接入已有实验手册、任务状态和 VR 菜单。

- Esc / 右手 B 打开菜单 → AI 物理导师；任务页也有提问入口。
- Windows 听写转文字；Unity Play 中可直接文字输入。听写依赖 Windows 语音服务与默认麦克风。
- 基础模型路径已指向下载的 Qwen2.5-3B-Instruct；服务地址为 `http://127.0.0.1:18080/api/tutor`；先运行导师目录的 `启动导师.cmd`，再在 Unity Play 中提问。
- 菜单 Report VR → AI Tutor → Select Connection Settings 打开共用配置。Base Model Path 是预留字段，填写路径本身不会加载模型；可运行服务准备好后填写 Service Endpoint。
- 导师来源为 `D:/_work/_SRTP/AI物理导师/ai-physics-tutor`，兼容其中的 `/api/tutor` HTTP 协议；问题、提示等级、当前任务与测量状态、受限对话历史通过 JSON 发送。

已下载的 Qwen2.5-3B-Instruct 保存在导师来源目录的 `models/Qwen2.5-3B-Instruct`，用于配合 `outputs/run_001/best_adapter`。本机后端采用 NF4 四位量化基础模型并加载物理 LoRA，单独运行于 Python 进程。共用配置的 Base Model Path 为 `D:/_work/_SRTP/AI物理导师/ai-physics-tutor/models/Qwen2.5-3B-Instruct`，Service Endpoint 为 `http://127.0.0.1:18080/api/tutor`。

导出的 `ChronoPivot_PhysicsTutor.unitypackage` 包含本模块、现有游戏 UI 的接入修改和配置工具。导入到当前游戏工程时，需要在 Packages/manifest.json 的 dependencies 中包含内置模块 `com.unity.modules.unitywebrequest: 1.0.0`。包不含基础模型、LoRA 权重、模型推理环境或整个游戏的其他依赖。

完整操作、语音系统设置和验证说明见工程 `Documentation/AI物理导师插件.md`。

真实文字测试与 Unity 联调已完成，但模型在浮力追问中错误地称浮力和重力同方向，三级提示也不够具体；检索资料存在无关卡片。接口可用不代表物理教学正确。详见工程 Documentation/Verification/physics-tutor-nf4-text-review.md。
