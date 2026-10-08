# 晷枢双界：Unity 物理实验游戏与 AI 物理导师

- 仓库根目录：Unity 2022.3.53f1c1 游戏工程、场景、资源、说明与验证记录。
- `AI物理导师/ai-physics-tutor/`：本机 Qwen2.5-3B-Instruct + 物理 LoRA 导师，使用 NF4 四位量化，通过 HTTP 接入游戏。

## 获取项目

仓库使用 Git LFS 管理游戏资源与物理 LoRA。克隆后执行 `git lfs pull`，确认 `outputs/run_001/best_adapter/adapter_model.safetensors` 是真实权重而不是 LFS 指针。

基础 Qwen 权重、Python 环境与 Unity 缓存由本机管理。历史游戏构建保留原已跟踪版本，本次只更新导师插件包。当前导师插件包位于 `Builds/PhysicsTutor/ChronoPivot_PhysicsTutor.unitypackage`。

## 本机运行

1. 用 Unity 2022.3.53f1c1 打开本仓库根目录，操作说明见该目录的 `README_开始使用.md`。
2. 按 `AI物理导师/ai-physics-tutor/队友部署说明.md` 创建独立 Python 环境。
3. 下载官方 `Qwen/Qwen2.5-3B-Instruct` 到导师目录的 `models/Qwen2.5-3B-Instruct`。可在导师目录使用环境中的 Python 执行：

```python
from huggingface_hub import snapshot_download
snapshot_download('Qwen/Qwen2.5-3B-Instruct', local_dir='models/Qwen2.5-3B-Instruct')
```

4. 本地配置记录的是原开发机器的模型路径；克隆到其他目录后，将 `prototype_delivery/config.json` 的 `model_path` 和 Unity `PhysicsTutorConnection.asset` 的 Base Model Path 改为下载后的实际路径。
5. 双击 `启动导师.cmd`，看到 READY 后，在 Unity 实验手册的「AI 物理导师」页面文字提问。默认地址为 `http://127.0.0.1:18080/api/tutor`。

## 当前验证与限制

NF4 真实模型文字请求和 Unity 输入、发送、回答显示已验证，29 项轻量测试通过。样本出现浮力与重力方向错误、提示等级不足及无关检索卡片，物理教学质量尚未通过检查。详情见 `Documentation/Verification/physics-tutor-nf4-text-review.md`。

原 Windows 游戏构建未包含本轮导师接入；游戏构建仍受既有关卡路线检查失败影响，详情见游戏说明。可以使用 Unity Play 调试当前源码。
