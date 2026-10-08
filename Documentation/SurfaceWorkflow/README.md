# 低多边形表面处理

工程：`ChronoPivot_NewVersion`，分支 `NewVersion`。

## 资产与工具

- `Assets/ReportVR/BakedSurfaces/Textures`：1126 组、3378 张 PNG。
- `Assets/ReportVR/BakedSurfaces/Materials`：2657 个已接入场景与预制体的材质。
- `Assets/ReportVR/LowPolyBaked.shader`：原调色板 × 中性细节图，保持运行时颜色反馈。
- Unity 菜单 `Chrono Pivot/表面工艺`：完整清理展开烘焙、保留 UV 重新烘焙、检查和拍摄预览。
- `index.html`：场景截图、三个代表资产 UV 展开与真实贴图预览。

## 风格和范围

保留平面色块、硬边法线和原始青绿/米白/木色/金色配色。轻微程序木纹、石纹直接烘焙进纹理；不是照片纹理。没有添加写实划痕、强噪点或高模法线。AO 为本模型的局部遮蔽，不是全场景光照图。金属度和粗糙度按原材质参数写入图集。

清理 1352 个自建静态网格；只焊接位置、法线、顶点色一致的顶点，移除零面积和同材质内重复面。UV0 与 UV1（mesh.uv2）均唯一展开。硬边、轮廓、资产 GUID、材质槽、碰撞与交互组件保持原有设置。人物 Ch46 的骨骼、权重、UV 与纹理未改；透明水、Unlit 和已有主贴图材质保留。

原色块跨 UV 接缝在远处 mip 采样时会串色，因此底色图采用中性调制，颜色保存在材质 _Color。完整延展图集边缘，避免白底漏色。_BakeTint 留存初始配色供查看；_Color 仍支持现有脚本互动反馈。

## 验证

- `geometry-verification.json`：1352 网格，有效几何与硬边法线保持；408704 → 407419 三角面，共清理 1285 无效/重复面。float32 允许位置差 1e-5 米、法线差 2e-5。
- `uv-verification.json`：两个 UV 通道有限、在 [0,1]，零面积 UV 面为 0；256 × 256 内部采样重叠为 0（采样检查，非任意分辨率数学证明）。
- `review.json`：2657 材质三贴图和原颜色引用检查；5 张场景预览，15 场景结构检查。没有替代头显里的实际帧率和交互测试。

## 调整

选中 Renderer 对应的 Baked 材质，在 Inspector 修改 Interactive tint、Baked texture strength（默认 0.35）、AO strength、Smoothness scale。调整共享材质会影响所有引用对象。重新改模型或调色板后可重新运行烘焙；只改纹理时选择“仅重新烘焙材质（保留UV）”。自建模型生成器若覆盖 mesh asset，需再次运行展开烘焙。

贴图按三角面数分 256/512/1024；工程总贴图数量较多，VR 显存与帧率仍需按目标头显实测。当前工作主要保持视觉与资产完整性，没有进行跨模型图集合并和运行时减面。

## Windows Demo

本次材质已更新至 `Builds/Windows`（SteamVR/OpenXR）与 `Builds/WindowsDesktop`（键鼠调试，使用现有 -desktop 启动方式）。Windows 构建成功，0 个错误；键鼠版从该构建导出。见 `unity-verification.json`。未进行目标头显实测。
