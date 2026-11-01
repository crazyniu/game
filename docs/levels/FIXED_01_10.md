> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# 第01–10关：v4固定布局与独立纸面回放

2026-10-31 · FIXED_CANDIDATE_V4 / INDEPENDENT_REPLAY_PASS。**本页已完全替换旧R4内容；旧目标队列、睡眠、条件箱、纹样预告蛋、临时枝任意清空退役及篮/异容量配置均不适用。** 本页不是运行JSON、最优证书或Unity记录。

规则唯一归[RULES](../spec/RULES.md)，来源键见[MECHANIC_EVIDENCE](../MECHANIC_EVIDENCE.md)，课程归[LEVEL_DESIGN](../LEVEL_DESIGN.md)，证据边界见[LEVEL_SIMULATIONS](../LEVEL_SIMULATIONS.md)。

每关N为所有枝统一容量、每种总数N；02–40采用P=K+2/两空枝；01为RULES §2.1已采纳N4/K2/P3/零空教学例外。数组墙→外露端；普通字母是鸟。`?(A)`表示策划固定为A的蛋，**玩家只看见?，不能读出A**；表/回放展示的是全信息分析视图，不证明盲玩公平。所有初始数组、鸟种/形态/位置固定，无运行随机；v4正式初态禁止相邻蛋，未证实的连蛋精确孵化不属于有效玩法。普通搬空不退役；完整同种N群自动归巢并使所在枝Retired；不使用额外枝也能走下列路径。

## 关卡01

First homecoming / adopted tutorial; 世界A 阔叶林；N=4，K=2，P=3；all capacities4, A/B exactly4 each, zero empty branches, no eggs. Profile: RULES §2.1 l01-two-flock-v1, only L01. Fixed arrays are wall→exposed end.

Sources: SRC-LEAF/SRC-SIMPLE for movement; SRC-BIRD2 for completion-only retirement. N4 is source-backed; this authored tutorial profile/layout is our adopted content decision, not a competitor's claimed original level.

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | A3 | A3 |
| S2 | 4 | B3 · A | B3 · A |
| S3 | 4 | B | B |

Fixed structure: 4 runs, 0 eggs. 主路2步; first Home at step1. Unique legal first Transfer: S2→S1 A1. No empty branch distracts from matching the exposed A. These witnesses do not exercise partial transfer.

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S1 A1（run=1，Count=1）；Home A@S1；Retire S1 | A | S1=[∅][Retired]；S2=[B3]；S3=[B] |
| 2 | Move S2→S3 B3（run=3，Count=3）；Home B@S3；Retire S3 | A,B | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired] |

**Complete alternative route:** same step1; then S3→S2 B1 (run=1, Count=1), Home B@S2, Retire S2. End: S1=[] Retired, S2=[] Retired, S3=[] Available; Home={A,B}, actionCount2. Both second moves are valid, not competing correctness grades. A merely emptied source stays Available; the completed receiver retires.

Every step conserves A4/B4 including Home, uses no assistance and ends with all8 birds Home. Undo restores the entire latest move/Home/retirement snapshot; Restart returns exactly these arrays/IDs. Both document routes are independently replayed, not a new full Core/Unity/visual run or formal optimality certificate. The older K3/P5/four-transfer L01 witness is superseded and must not load as this profile.

## 关卡02

拆开交错小段；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A · C · B2` | `A · C · B2` |
| S2 | 4 | `C · A2 · B` | `C · A2 · B` |
| S3 | 4 | `C · A · C · B` | `C · A · C · B` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：10个初始连续段、0颗问号蛋。主路8步，仅存在性见证。首次归巢在第3步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 B2（run=2，Count=2） | 无 | S1=[A · C]；S2=[C · A2 · B]；S3=[C · A · C · B]；S4=[B2]；S5=[∅] |
| 2 | Move S2→S4 B1（run=1，Count=1） | 无 | S1=[A · C]；S2=[C · A2]；S3=[C · A · C · B]；S4=[B3]；S5=[∅] |
| 3 | Move S3→S4 B1（run=1，Count=1）；Home B@S4；Retire S4 | B | S1=[A · C]；S2=[C · A2]；S3=[C · A · C]；S4=[∅][Retired]；S5=[∅] |
| 4 | Move S1→S3 C1（run=1，Count=1） | B | S1=[A]；S2=[C · A2]；S3=[C · A · C2]；S4=[∅][Retired]；S5=[∅] |
| 5 | Move S2→S1 A2（run=2，Count=2） | B | S1=[A3]；S2=[C]；S3=[C · A · C2]；S4=[∅][Retired]；S5=[∅] |
| 6 | Move S3→S2 C2（run=2，Count=2） | B | S1=[A3]；S2=[C3]；S3=[C · A]；S4=[∅][Retired]；S5=[∅] |
| 7 | Move S3→S1 A1（run=1，Count=1）；Home A@S1；Retire S1 | A,B | S1=[∅][Retired]；S2=[C3]；S3=[C]；S4=[∅][Retired]；S5=[∅] |
| 8 | Move S2→S3 C3（run=3，Count=3）；Home C@S3；Retire S3 | A,B,C | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡03

先露出内层；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `B2 · A · B` | `B2 · A · B` |
| S2 | 4 | `A2 · C2` | `A2 · C2` |
| S3 | 4 | `C · A · B · C` | `C · A · B · C` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：9个初始连续段、0颗问号蛋。主路7步，仅存在性见证。首次归巢在第5步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S4 C2（run=2，Count=2） | 无 | S1=[B2 · A · B]；S2=[A2]；S3=[C · A · B · C]；S4=[C2]；S5=[∅] |
| 2 | Move S3→S4 C1（run=1，Count=1） | 无 | S1=[B2 · A · B]；S2=[A2]；S3=[C · A · B]；S4=[C3]；S5=[∅] |
| 3 | Move S1→S3 B1（run=1，Count=1） | 无 | S1=[B2 · A]；S2=[A2]；S3=[C · A · B2]；S4=[C3]；S5=[∅] |
| 4 | Move S1→S2 A1（run=1，Count=1） | 无 | S1=[B2]；S2=[A3]；S3=[C · A · B2]；S4=[C3]；S5=[∅] |
| 5 | Move S3→S1 B2（run=2，Count=2）；Home B@S1；Retire S1 | B | S1=[∅][Retired]；S2=[A3]；S3=[C · A]；S4=[C3]；S5=[∅] |
| 6 | Move S3→S2 A1（run=1，Count=1）；Home A@S2；Retire S2 | A,B | S1=[∅][Retired]；S2=[∅][Retired]；S3=[C]；S4=[C3]；S5=[∅] |
| 7 | Move S3→S4 C1（run=1，Count=1）；Home C@S4；Retire S4 | A,B,C | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡04

空枝分工；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A · C2 · A` | `A · C2 · A` |
| S2 | 4 | `B · A2 · B` | `B · A2 · B` |
| S3 | 4 | `C2 · B2` | `C2 · B2` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：8个初始连续段、0颗问号蛋。主路6步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S4 B1（run=1，Count=1） | 无 | S1=[A · C2 · A]；S2=[B · A2]；S3=[C2 · B2]；S4=[B]；S5=[∅] |
| 2 | Move S1→S2 A1（run=1，Count=1） | 无 | S1=[A · C2]；S2=[B · A3]；S3=[C2 · B2]；S4=[B]；S5=[∅] |
| 3 | Move S3→S4 B2（run=2，Count=2） | 无 | S1=[A · C2]；S2=[B · A3]；S3=[C2]；S4=[B3]；S5=[∅] |
| 4 | Move S1→S3 C2（run=2，Count=2）；Home C@S3；Retire S3 | C | S1=[A]；S2=[B · A3]；S3=[∅][Retired]；S4=[B3]；S5=[∅] |
| 5 | Move S2→S1 A3（run=3，Count=3）；Home A@S1；Retire S1 | A,C | S1=[∅][Retired]；S2=[B]；S3=[∅][Retired]；S4=[B3]；S5=[∅] |
| 6 | Move S2→S4 B1（run=1，Count=1）；Home B@S4；Retire S4 | A,B,C | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡05

先合再腾位；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `B2 · A2` | `B2 · A2` |
| S2 | 4 | `B2 · C · A` | `B2 · C · A` |
| S3 | 4 | `C2 · A · C` | `C2 · A · C` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：8个初始连续段、0颗问号蛋。主路6步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 A2（run=2，Count=2） | 无 | S1=[B2]；S2=[B2 · C · A]；S3=[C2 · A · C]；S4=[A2]；S5=[∅] |
| 2 | Move S2→S4 A1（run=1，Count=1） | 无 | S1=[B2]；S2=[B2 · C]；S3=[C2 · A · C]；S4=[A3]；S5=[∅] |
| 3 | Move S3→S2 C1（run=1，Count=1） | 无 | S1=[B2]；S2=[B2 · C2]；S3=[C2 · A]；S4=[A3]；S5=[∅] |
| 4 | Move S3→S4 A1（run=1，Count=1）；Home A@S4；Retire S4 | A | S1=[B2]；S2=[B2 · C2]；S3=[C2]；S4=[∅][Retired]；S5=[∅] |
| 5 | Move S2→S3 C2（run=2，Count=2）；Home C@S3；Retire S3 | A,C | S1=[B2]；S2=[B2]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |
| 6 | Move S1→S2 B2（run=2，Count=2）；Home B@S2；Retire S2 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡06

多段接力；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `C2 · A · B` | `C2 · A · B` |
| S2 | 4 | `A · B · C2` | `A · B · C2` |
| S3 | 4 | `A2 · B2` | `A2 · B2` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：8个初始连续段、0颗问号蛋。主路6步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 B1（run=1，Count=1） | 无 | S1=[C2 · A]；S2=[A · B · C2]；S3=[A2 · B2]；S4=[B]；S5=[∅] |
| 2 | Move S3→S4 B2（run=2，Count=2） | 无 | S1=[C2 · A]；S2=[A · B · C2]；S3=[A2]；S4=[B3]；S5=[∅] |
| 3 | Move S1→S3 A1（run=1，Count=1） | 无 | S1=[C2]；S2=[A · B · C2]；S3=[A3]；S4=[B3]；S5=[∅] |
| 4 | Move S2→S1 C2（run=2，Count=2）；Home C@S1；Retire S1 | C | S1=[∅][Retired]；S2=[A · B]；S3=[A3]；S4=[B3]；S5=[∅] |
| 5 | Move S2→S4 B1（run=1，Count=1）；Home B@S4；Retire S4 | B,C | S1=[∅][Retired]；S2=[A]；S3=[A3]；S4=[∅][Retired]；S5=[∅] |
| 6 | Move S2→S3 A1（run=1，Count=1）；Home A@S3；Retire S3 | A,B,C | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡07

阔叶林歇脚；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `C · B3` | `C · B3` |
| S2 | 4 | `B · A3` | `B · A3` |
| S3 | 4 | `C3 · A` | `C3 · A` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：6个初始连续段、0颗问号蛋。主路4步，仅存在性见证。首次归巢在第2步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S4 A3（run=3，Count=3） | 无 | S1=[C · B3]；S2=[B]；S3=[C3 · A]；S4=[A3]；S5=[∅] |
| 2 | Move S1→S2 B3（run=3，Count=3）；Home B@S2；Retire S2 | B | S1=[C]；S2=[∅][Retired]；S3=[C3 · A]；S4=[A3]；S5=[∅] |
| 3 | Move S3→S4 A1（run=1，Count=1）；Home A@S4；Retire S4 | A,B | S1=[C]；S2=[∅][Retired]；S3=[C3]；S4=[∅][Retired]；S5=[∅] |
| 4 | Move S1→S3 C1（run=1，Count=1）；Home C@S3；Retire S3 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡08

阔叶林总结；世界A 阔叶林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A3 · C` | `A3 · C` |
| S2 | 4 | `B2 · A · B` | `B2 · A · B` |
| S3 | 4 | `C · B · C2` | `C · B · C2` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：8个初始连续段、0颗问号蛋。主路6步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 C1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · A · B]；S3=[C · B · C2]；S4=[C]；S5=[∅] |
| 2 | Move S3→S4 C2（run=2，Count=2） | 无 | S1=[A3]；S2=[B2 · A · B]；S3=[C · B]；S4=[C3]；S5=[∅] |
| 3 | Move S2→S3 B1（run=1，Count=1） | 无 | S1=[A3]；S2=[B2 · A]；S3=[C · B2]；S4=[C3]；S5=[∅] |
| 4 | Move S2→S1 A1（run=1，Count=1）；Home A@S1；Retire S1 | A | S1=[∅][Retired]；S2=[B2]；S3=[C · B2]；S4=[C3]；S5=[∅] |
| 5 | Move S3→S2 B2（run=2，Count=2）；Home B@S2；Retire S2 | A,B | S1=[∅][Retired]；S2=[∅][Retired]；S3=[C]；S4=[C3]；S5=[∅] |
| 6 | Move S3→S4 C1（run=1，Count=1）；Home C@S4；Retire S4 | A,B,C | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡09

第一颗问号蛋；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `?(D) · B · A · B` | `? · B · A · B` |
| S2 | 4 | `A · B · A · B` | `A · B · A · B` |
| S3 | 4 | `C3 · A` | `C3 · A` |
| S4 | 4 | `D3 · C` | `D3 · C` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：12个初始连续段、1颗问号蛋。主路10步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第4步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 B1（run=1，Count=1） | 无 | S1=[?(D) · B · A]；S2=[A · B · A · B]；S3=[C3 · A]；S4=[D3 · C]；S5=[B]；S6=[∅] |
| 2 | Move S2→S5 B1（run=1，Count=1） | 无 | S1=[?(D) · B · A]；S2=[A · B · A]；S3=[C3 · A]；S4=[D3 · C]；S5=[B2]；S6=[∅] |
| 3 | Move S1→S2 A1（run=1，Count=1） | 无 | S1=[?(D) · B]；S2=[A · B · A2]；S3=[C3 · A]；S4=[D3 · C]；S5=[B2]；S6=[∅] |
| 4 | Move S1→S5 B1（run=1，Count=1）；Hatch S1 → D | 无 | S1=[D]；S2=[A · B · A2]；S3=[C3 · A]；S4=[D3 · C]；S5=[B3]；S6=[∅] |
| 5 | Move S2→S6 A2（run=2，Count=2） | 无 | S1=[D]；S2=[A · B]；S3=[C3 · A]；S4=[D3 · C]；S5=[B3]；S6=[A2] |
| 6 | Move S2→S5 B1（run=1，Count=1）；Home B@S5；Retire S5 | B | S1=[D]；S2=[A]；S3=[C3 · A]；S4=[D3 · C]；S5=[∅][Retired]；S6=[A2] |
| 7 | Move S2→S6 A1（run=1，Count=1） | B | S1=[D]；S2=[∅]；S3=[C3 · A]；S4=[D3 · C]；S5=[∅][Retired]；S6=[A3] |
| 8 | Move S3→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A,B | S1=[D]；S2=[∅]；S3=[C3]；S4=[D3 · C]；S5=[∅][Retired]；S6=[∅][Retired] |
| 9 | Move S4→S3 C1（run=1，Count=1）；Home C@S3；Retire S3 | A,B,C | S1=[D]；S2=[∅]；S3=[∅][Retired]；S4=[D3]；S5=[∅][Retired]；S6=[∅][Retired] |
| 10 | Move S1→S4 D1（run=1，Count=1）；Home D@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡10

看见后再选择；世界B 竹林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `C · A · C · A` | `C · A · C · A` |
| S2 | 4 | `B2 · A · B` | `B2 · A · B` |
| S3 | 4 | `A · C · ?(B) · C` | `A · C · ? · C` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：11个初始连续段、1颗问号蛋。主路9步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S3→S4 C1（run=1，Count=1）；Hatch S3 → B | 无 | S1=[C · A · C · A]；S2=[B2 · A · B]；S3=[A · C · B]；S4=[C]；S5=[∅] |
| 2 | Move S2→S3 B1（run=1，Count=1） | 无 | S1=[C · A · C · A]；S2=[B2 · A]；S3=[A · C · B2]；S4=[C]；S5=[∅] |
| 3 | Move S1→S2 A1（run=1，Count=1） | 无 | S1=[C · A · C]；S2=[B2 · A2]；S3=[A · C · B2]；S4=[C]；S5=[∅] |
| 4 | Move S1→S4 C1（run=1，Count=1） | 无 | S1=[C · A]；S2=[B2 · A2]；S3=[A · C · B2]；S4=[C2]；S5=[∅] |
| 5 | Move S2→S1 A2（run=2，Count=2） | 无 | S1=[C · A3]；S2=[B2]；S3=[A · C · B2]；S4=[C2]；S5=[∅] |
| 6 | Move S3→S2 B2（run=2，Count=2）；Home B@S2；Retire S2 | B | S1=[C · A3]；S2=[∅][Retired]；S3=[A · C]；S4=[C2]；S5=[∅] |
| 7 | Move S3→S4 C1（run=1，Count=1） | B | S1=[C · A3]；S2=[∅][Retired]；S3=[A]；S4=[C3]；S5=[∅] |
| 8 | Move S1→S3 A3（run=3，Count=3）；Home A@S3；Retire S3 | A,B | S1=[C]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[C3]；S5=[∅] |
| 9 | Move S1→S4 C1（run=1，Count=1）；Home C@S4；Retire S4 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。
