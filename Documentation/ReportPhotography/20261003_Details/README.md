# 中期报告场景配图索引

依据《02_晷枢双界_省创中期检查报告_LaTeX版.pdf》的场景配图说明拍摄。覆盖报告图2.1–2.13、图2.15，共14组，并另附古代园林，共15场景、90张原图。

- 原图：`Photos`，每场景6张，2560×1440 PNG。
- 推荐主图：`Selected`，按报告图号命名，可直接插入报告。
- 六视角预览：`ContactSheets`；交互图库：打开同目录`index.html`。
- 第一关与远景主题环采用金色外环、青色细内环的镂空造型。
- 本轮新增105组自建主题小景，包括花廊、漏窗园墙、竹石花池、石灯、雪檐棚、补给木桶和室内备件柜。
- Unity层级中的`Report Details · 自建主题小景`保存新增内容，各组Transform可以直接调整位置、旋转和缩放；室内器材还可展开调整子物件。
- 新增装饰无Collider，已检查与既有路线和自然实验模型的可见几何间距。
- 照片为Unity静态展示取景。临时隐藏任务牌和玩家模型，恢复后不保存场景；实际布局与玩法保留。单摆杆的静态预览按运行时初始摆长显示，拍完恢复。

| 报告图号 | 内容 | 主图 | 多角度预览 |
|---|---|---|---|
| 2.1 | 基础力学与空间感知引导 | [推荐图 · 正面全景](Selected/Fig_2_01_Introduction.png) | [六视角总览](ContactSheets/Fig_2_01_Introduction.jpg) |
| 2.2 | 浮力与压强实验 | [推荐图 · 主要模型](Selected/Fig_2_02_Buoyancy_Canal.png) | [六视角总览](ContactSheets/Fig_2_02_Buoyancy_Canal.jpg) |
| 2.3 | 声学与共振实验 | [推荐图 · 主要模型](Selected/Fig_2_03_Acoustic_Resonance.png) | [六视角总览](ContactSheets/Fig_2_03_Acoustic_Resonance.jpg) |
| 2.4 | 地磁感应与反射光学实验 | [推荐图 · 另一侧全景](Selected/Fig_2_04_Geomagnetic_Optics.png) | [六视角总览](ContactSheets/Fig_2_04_Geomagnetic_Optics.jpg) |
| 2.5 | 电磁锁组装与电路连通 | [推荐图 · 主要模型](Selected/Fig_2_05_Electromagnetic_Lock.png) | [六视角总览](ContactSheets/Fig_2_05_Electromagnetic_Lock.jpg) |
| 2.6 | 流体管网排故与修复 | [推荐图 · 正面全景](Selected/Fig_2_06_Hydraulic_Pipework.png) | [六视角总览](ContactSheets/Fig_2_06_Hydraulic_Pipework.jpg) |
| 2.7 | 频率耦合实验 | [推荐图 · 主要模型](Selected/Fig_2_07_Frequency_Coupling.png) | [六视角总览](ContactSheets/Fig_2_07_Frequency_Coupling.jpg) |
| 2.8 | 冰雪竞技场与热学环境 | [推荐图 · 正面全景](Selected/Fig_2_08_Snow_Arena.png) | [六视角总览](ContactSheets/Fig_2_08_Snow_Arena.jpg) |
| 2.9 | 创意工坊与自由探索 | [推荐图 · 主要模型](Selected/Fig_2_09_Creative_Workshop.png) | [六视角总览](ContactSheets/Fig_2_09_Creative_Workshop.jpg) |
| 2.10 | 物理实验室设计 | [推荐图 · 主要模型](Selected/Fig_2_10_Physics_Laboratory.png) | [六视角总览](ContactSheets/Fig_2_10_Physics_Laboratory.jpg) |
| 2.11 | 园林水渠场景设计 | [推荐图 · 另一侧全景](Selected/Fig_2_11_Garden_Canal_Design.png) | [六视角总览](ContactSheets/Fig_2_11_Garden_Canal_Design.jpg) |
| 2.12 | 天体运行与日晷设计 | [推荐图 · 玩家高度](Selected/Fig_2_12_Celestial_Sundial.png) | [六视角总览](ContactSheets/Fig_2_12_Celestial_Sundial.jpg) |
| 2.13 | 冰雪实验场场景与资产设计 | [推荐图 · 正面全景](Selected/Fig_2_13_Winter_Asset_Design.png) | [六视角总览](ContactSheets/Fig_2_13_Winter_Asset_Design.jpg) |
| 2.15 | 三维场景与交互资产建模 | [推荐图 · 另一侧全景](Selected/Fig_2_15_SelfModeled_Asset_Collection.png) | [六视角总览](ContactSheets/Fig_2_15_SelfModeled_Asset_Collection.jpg) |
| 附图 | 古代园林补充场景 | [全景](Photos/AncientGarden_LowPoly/01_Hero.png) | [六视角总览](ContactSheets/AncientGarden_LowPoly.jpg) |

## 每组取景

1. 正面全景：主要环境及主题视觉。
2. 另一侧全景：空间布局与建筑/器材关系。
3. 玩家高度：出生位置与进入路线。
4. 主要模型：与报告图注相关的场景主体。
5. 实验器材一：当前关卡实际实验模型。
6. 实验器材二：另一处器材或另一侧取景。

## 其它报告图的适用范围

图2.14是收集的参考资料，应使用资料来源图片；图2.16是UV展开与烘焙，图2.17涉及引擎/性能，图2.18–2.23涉及XR、骨骼、材质、脚本结构、物理流程及联网同步。本次环境照片可作为相关内容的场景补图，不能代替这些工具或运行状态的证据截图。图2.7的照片展示机械摆/LC两区，不是双端联机实测。
