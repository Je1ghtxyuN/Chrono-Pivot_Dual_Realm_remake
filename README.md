# Chrono-Pivot: Dual Realm Remake (VR Project)

## 简介 (Introduction)

本项目是使用 Unity 引擎开发的虚拟现实 (VR) 体验项目，基于 Universal Render Pipeline (URP) 和 Unity 官方的 XR Interaction Toolkit 构建。


---

## 🛠️ 环境与要求 (Prerequisites & Requirements)

要正确运行和参与本项目开发，您需要满足以下要求：

* **Unity 版本:** Unity 2022.3.53f1c1 或更高版本 。
* **渲染管线:** Universal Render Pipeline (URP)。
* **XR 核心插件:** OpenXR Plugin Management。
* **协作工具:** Git 和 Git LFS 。

---

## 协作与设置指南 (Setup & Collaboration Guide)

**⚠️ 警告：本项目包含大量高分辨率贴图和模型，必须使用 Git LFS 才能正常工作。**

请 **严格** 按照以下步骤进行操作，以确保 Unity 资源不会损坏，且项目能够正确导入：

### 1. 克隆仓库

使用 Git LFS 客户端克隆仓库。如果您之前配置了 SSH，请使用 SSH URL：

```bash
git clone git@github.com:Je1ghtxyuN/Chrono-Pivot_Dual_Realm_remake.git
cd Chrono-Pivot_Dual_Realm_remake
```

### 2. 强制下载 LFS 资源 (关键步骤)

由于 Git LFS 机制，克隆操作可能只下载了文件的“指针”，而不是实际内容。您必须手动运行 `git lfs pull` 来下载所有的模型和贴图文件。

```bash
# 确保 LFS 已安装并初始化
git lfs install

# 强制下载所有 LFS 跟踪的大文件
git lfs pull
```

*如果网络不稳定导致下载失败，请多尝试几次，直到 LFS 文件全部下载完成。*

### 3. 打开项目

1.  打开 **Unity Hub**。
2.  点击 **Add Project from disk**，选择您克隆的文件夹。
3.  打开项目，Unity 会自动导入资源。

> **如果在 Unity 控制台中看到 `File could not be read` 错误，这意味着 LFS 文件未完全下载。请关闭 Unity，回到终端重新运行 `git lfs pull`。**

### 4. 协作流程 (Workflow) - 简化本地合并

为保证开发时的稳定性，同时避免复杂的 PR 审查，我们采用 **功能分支开发并本地合并到 `main`** 的模式。

| 步骤 | 操作目的 | Git 命令示例 |
| :--- | :--- | :--- |
| **1. 同步主分支** | 每次开始工作前，确保本地 `main` 分支是最新的。 | `git checkout main` |
| | | `git pull origin main` |
| **2. 创建功能分支** | 为您的功能或修复创建一个新的独立分支。 | `git checkout -b feature/您的功能名称` |
| **3. 开发与提交** | 在此分支上进行开发和提交。 | `git commit -m "feat: 完成了功能X"` |
| **4. 本地合并** | 功能完成后，切换回 `main` 分支，并将您的功能分支合并到 `main`。 | `git checkout main` |
| | | `git merge feature/您的功能名称` |
| **5. 推送最终结果** | 将包含新功能的 `main` 分支推送到远端仓库。 | `git push origin main` |
| **6. (可选) 清理** | 如果功能已稳定并推送到远端，您可以删除本地功能分支。 | `git branch -d feature/您的功能名称` |

-----

## 🖥️ 运行平台

  * **主要目标平台:** Meta Quest，Pico (一体机 VR)
  * **开发/测试平台:** PC VR (通过 SteamVR 串流)
