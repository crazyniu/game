> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# 第11–20关：v4固定布局与独立纸面回放

2026-10-31 · FIXED_CANDIDATE_V4 / INDEPENDENT_REPLAY_PASS。**本页已完全替换旧R4内容；旧目标队列、睡眠、条件箱、纹样预告蛋、临时枝任意清空退役及篮/异容量配置均不适用。** 本页不是运行JSON、最优证书或Unity记录。

规则唯一归[RULES](../spec/RULES.md)，来源键见[MECHANIC_EVIDENCE](../MECHANIC_EVIDENCE.md)，课程归[LEVEL_DESIGN](../LEVEL_DESIGN.md)，证据边界见[LEVEL_SIMULATIONS](../LEVEL_SIMULATIONS.md)。

每关N为所有枝统一容量、每种总数N；P=K+2，开局恰两空枝。数组墙→外露端；普通字母是鸟。`?(A)`表示策划固定为A的蛋，**玩家只看见?，不能读出A**；表/回放展示的是全信息分析视图，不证明盲玩公平。所有初始数组、鸟种/形态/位置固定，无运行随机；v4正式初态禁止相邻蛋，未证实的连蛋精确孵化不属于有效玩法。普通搬空不退役；完整同种N群自动归巢并使所在枝Retired；不使用额外枝也能走下列路径。

## 关卡11

两处揭示；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `D · A3` | `D · A3` |
| S2 | 4 | `A · B · D · B` | `A · B · D · B` |
| S3 | 4 | `C · ?(C) · B · C` | `C · ? · B · C` |
| S4 | 4 | `C · ?(D) · D · B` | `C · ? · D · B` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：14个初始连续段、2颗问号蛋。主路10步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第3,9步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S5 B1（run=1，Count=1） | 无 | S1=[D · A3]；S2=[A · B · D]；S3=[C · ?(C) · B · C]；S4=[C · ?(D) · D · B]；S5=[B]；S6=[∅] |
| 2 | Move S4→S5 B1（run=1，Count=1） | 无 | S1=[D · A3]；S2=[A · B · D]；S3=[C · ?(C) · B · C]；S4=[C · ?(D) · D]；S5=[B2]；S6=[∅] |
| 3 | Move S4→S2 D1（run=1，Count=1）；Hatch S4 → D | 无 | S1=[D · A3]；S2=[A · B · D2]；S3=[C · ?(C) · B · C]；S4=[C · D]；S5=[B2]；S6=[∅] |
| 4 | Move S2→S4 D2（run=2，Count=2） | 无 | S1=[D · A3]；S2=[A · B]；S3=[C · ?(C) · B · C]；S4=[C · D3]；S5=[B2]；S6=[∅] |
| 5 | Move S2→S5 B1（run=1，Count=1） | 无 | S1=[D · A3]；S2=[A]；S3=[C · ?(C) · B · C]；S4=[C · D3]；S5=[B3]；S6=[∅] |
| 6 | Move S1→S2 A3（run=3，Count=3）；Home A@S2；Retire S2 | A | S1=[D]；S2=[∅][Retired]；S3=[C · ?(C) · B · C]；S4=[C · D3]；S5=[B3]；S6=[∅] |
| 7 | Move S4→S1 D3（run=3，Count=3）；Home D@S1；Retire S1 | A,D | S1=[∅][Retired]；S2=[∅][Retired]；S3=[C · ?(C) · B · C]；S4=[C]；S5=[B3]；S6=[∅] |
| 8 | Move S3→S4 C1（run=1，Count=1） | A,D | S1=[∅][Retired]；S2=[∅][Retired]；S3=[C · ?(C) · B]；S4=[C2]；S5=[B3]；S6=[∅] |
| 9 | Move S3→S5 B1（run=1，Count=1）；Hatch S3 → C；Home B@S5；Retire S5 | A,B,D | S1=[∅][Retired]；S2=[∅][Retired]；S3=[C2]；S4=[C2]；S5=[∅][Retired]；S6=[∅] |
| 10 | Move S3→S4 C2（run=2，Count=2）；Home C@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡12

先空一条路；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `B2 · A2` | `B2 · A2` |
| S2 | 4 | `B2 · ?(C) · D` | `B2 · ? · D` |
| S3 | 4 | `C3 · D` | `C3 · D` |
| S4 | 4 | `A · ?(D) · D · A` | `A · ? · D · A` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：11个初始连续段、2颗问号蛋。主路9步，仅存在性见证。首次归巢在第3步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1,7步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S5 D1（run=1，Count=1）；Hatch S2 → C | 无 | S1=[B2 · A2]；S2=[B2 · C]；S3=[C3 · D]；S4=[A · ?(D) · D · A]；S5=[D]；S6=[∅] |
| 2 | Move S3→S5 D1（run=1，Count=1） | 无 | S1=[B2 · A2]；S2=[B2 · C]；S3=[C3]；S4=[A · ?(D) · D · A]；S5=[D2]；S6=[∅] |
| 3 | Move S2→S3 C1（run=1，Count=1）；Home C@S3；Retire S3 | C | S1=[B2 · A2]；S2=[B2]；S3=[∅][Retired]；S4=[A · ?(D) · D · A]；S5=[D2]；S6=[∅] |
| 4 | Move S1→S6 A2（run=2，Count=2） | C | S1=[B2]；S2=[B2]；S3=[∅][Retired]；S4=[A · ?(D) · D · A]；S5=[D2]；S6=[A2] |
| 5 | Move S1→S2 B2（run=2，Count=2）；Home B@S2；Retire S2 | B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A · ?(D) · D · A]；S5=[D2]；S6=[A2] |
| 6 | Move S4→S6 A1（run=1，Count=1） | B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A · ?(D) · D]；S5=[D2]；S6=[A3] |
| 7 | Move S4→S5 D1（run=1，Count=1）；Hatch S4 → D | B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A · D]；S5=[D3]；S6=[A3] |
| 8 | Move S4→S5 D1（run=1，Count=1）；Home D@S5；Retire S5 | B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A]；S5=[∅][Retired]；S6=[A3] |
| 9 | Move S4→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡13

揭示与接收；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `D · ?(D) · A · D` | `D · ? · A · D` |
| S2 | 4 | `B · A2 · C` | `B · A2 · C` |
| S3 | 4 | `C2 · B · C` | `C2 · B · C` |
| S4 | 4 | `D · B2 · A` | `D · B2 · A` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：13个初始连续段、1颗问号蛋。主路12步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第2步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 D1（run=1，Count=1） | 无 | S1=[D · ?(D) · A]；S2=[B · A2 · C]；S3=[C2 · B · C]；S4=[D · B2 · A]；S5=[D]；S6=[∅] |
| 2 | Move S1→S6 A1（run=1，Count=1）；Hatch S1 → D | 无 | S1=[D2]；S2=[B · A2 · C]；S3=[C2 · B · C]；S4=[D · B2 · A]；S5=[D]；S6=[A] |
| 3 | Move S1→S5 D2（run=2，Count=2） | 无 | S1=[∅]；S2=[B · A2 · C]；S3=[C2 · B · C]；S4=[D · B2 · A]；S5=[D3]；S6=[A] |
| 4 | Move S4→S6 A1（run=1，Count=1） | 无 | S1=[∅]；S2=[B · A2 · C]；S3=[C2 · B · C]；S4=[D · B2]；S5=[D3]；S6=[A2] |
| 5 | Move S2→S1 C1（run=1，Count=1） | 无 | S1=[C]；S2=[B · A2]；S3=[C2 · B · C]；S4=[D · B2]；S5=[D3]；S6=[A2] |
| 6 | Move S2→S6 A2（run=2，Count=2）；Home A@S6；Retire S6 | A | S1=[C]；S2=[B]；S3=[C2 · B · C]；S4=[D · B2]；S5=[D3]；S6=[∅][Retired] |
| 7 | Move S2→S4 B1（run=1，Count=1） | A | S1=[C]；S2=[∅]；S3=[C2 · B · C]；S4=[D · B3]；S5=[D3]；S6=[∅][Retired] |
| 8 | Move S3→S1 C1（run=1，Count=1） | A | S1=[C2]；S2=[∅]；S3=[C2 · B]；S4=[D · B3]；S5=[D3]；S6=[∅][Retired] |
| 9 | Move S3→S2 B1（run=1，Count=1） | A | S1=[C2]；S2=[B]；S3=[C2]；S4=[D · B3]；S5=[D3]；S6=[∅][Retired] |
| 10 | Move S1→S3 C2（run=2，Count=2）；Home C@S3；Retire S3 | A,C | S1=[∅]；S2=[B]；S3=[∅][Retired]；S4=[D · B3]；S5=[D3]；S6=[∅][Retired] |
| 11 | Move S4→S2 B3（run=3，Count=3）；Home B@S2；Retire S2 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[D]；S5=[D3]；S6=[∅][Retired] |
| 12 | Move S4→S5 D1（run=1，Count=1）；Home D@S5；Retire S5 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡14

多层拆分；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `B · D · ?(D) · A` | `B · D · ? · A` |
| S2 | 4 | `B · A · ?(C) · B` | `B · A · ? · B` |
| S3 | 4 | `C · A2 · D` | `C · A2 · D` |
| S4 | 4 | `C · D · B · C` | `C · D · B · C` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：15个初始连续段、2颗问号蛋。主路12步，仅存在性见证。首次归巢在第7步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1,5步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A1（run=1，Count=1）；Hatch S1 → D | 无 | S1=[B · D2]；S2=[B · A · ?(C) · B]；S3=[C · A2 · D]；S4=[C · D · B · C]；S5=[A]；S6=[∅] |
| 2 | Move S3→S1 D1（run=1，Count=1） | 无 | S1=[B · D3]；S2=[B · A · ?(C) · B]；S3=[C · A2]；S4=[C · D · B · C]；S5=[A]；S6=[∅] |
| 3 | Move S3→S5 A2（run=2，Count=2） | 无 | S1=[B · D3]；S2=[B · A · ?(C) · B]；S3=[C]；S4=[C · D · B · C]；S5=[A3]；S6=[∅] |
| 4 | Move S4→S3 C1（run=1，Count=1） | 无 | S1=[B · D3]；S2=[B · A · ?(C) · B]；S3=[C2]；S4=[C · D · B]；S5=[A3]；S6=[∅] |
| 5 | Move S2→S4 B1（run=1，Count=1）；Hatch S2 → C | 无 | S1=[B · D3]；S2=[B · A · C]；S3=[C2]；S4=[C · D · B2]；S5=[A3]；S6=[∅] |
| 6 | Move S2→S3 C1（run=1，Count=1） | 无 | S1=[B · D3]；S2=[B · A]；S3=[C3]；S4=[C · D · B2]；S5=[A3]；S6=[∅] |
| 7 | Move S2→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A | S1=[B · D3]；S2=[B]；S3=[C3]；S4=[C · D · B2]；S5=[∅][Retired]；S6=[∅] |
| 8 | Move S4→S2 B2（run=2，Count=2） | A | S1=[B · D3]；S2=[B3]；S3=[C3]；S4=[C · D]；S5=[∅][Retired]；S6=[∅] |
| 9 | Move S1→S6 D3（run=3，Count=3） | A | S1=[B]；S2=[B3]；S3=[C3]；S4=[C · D]；S5=[∅][Retired]；S6=[D3] |
| 10 | Move S1→S2 B1（run=1，Count=1）；Home B@S2；Retire S2 | A,B | S1=[∅]；S2=[∅][Retired]；S3=[C3]；S4=[C · D]；S5=[∅][Retired]；S6=[D3] |
| 11 | Move S4→S6 D1（run=1，Count=1）；Home D@S6；Retire S6 | A,B,D | S1=[∅]；S2=[∅][Retired]；S3=[C3]；S4=[C]；S5=[∅][Retired]；S6=[∅][Retired] |
| 12 | Move S3→S4 C3（run=3，Count=3）；Home C@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡15

竹林歇脚；世界B 竹林；N=4，K=3，P=5；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A2 · C · A` | `A2 · C · A` |
| S2 | 4 | `C2 · B2` | `C2 · B2` |
| S3 | 4 | `B · ?(B) · A · C` | `B · ? · A · C` |
| S4 | 4 | `∅` | `∅` |
| S5 | 4 | `∅` | `∅` |

本关固定结构：9个初始连续段、1颗问号蛋。主路6步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第3步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 A1（run=1，Count=1） | 无 | S1=[A2 · C]；S2=[C2 · B2]；S3=[B · ?(B) · A · C]；S4=[A]；S5=[∅] |
| 2 | Move S3→S1 C1（run=1，Count=1） | 无 | S1=[A2 · C2]；S2=[C2 · B2]；S3=[B · ?(B) · A]；S4=[A]；S5=[∅] |
| 3 | Move S3→S4 A1（run=1，Count=1）；Hatch S3 → B | 无 | S1=[A2 · C2]；S2=[C2 · B2]；S3=[B2]；S4=[A2]；S5=[∅] |
| 4 | Move S2→S3 B2（run=2，Count=2）；Home B@S3；Retire S3 | B | S1=[A2 · C2]；S2=[C2]；S3=[∅][Retired]；S4=[A2]；S5=[∅] |
| 5 | Move S1→S2 C2（run=2，Count=2）；Home C@S2；Retire S2 | B,C | S1=[A2]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A2]；S5=[∅] |
| 6 | Move S1→S4 A2（run=2，Count=2）；Home A@S4；Retire S4 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡16

回流再归巢；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `B · A3` | `B · A3` |
| S2 | 4 | `B · ?(D) · C · B` | `B · ? · C · B` |
| S3 | 4 | `?(A) · B · C2` | `? · B · C2` |
| S4 | 4 | `D3 · C` | `D3 · C` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：11个初始连续段、2颗问号蛋。主路9步，仅存在性见证。首次归巢在第5步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第4,8步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A3（run=3，Count=3） | 无 | S1=[B]；S2=[B · ?(D) · C · B]；S3=[?(A) · B · C2]；S4=[D3 · C]；S5=[A3]；S6=[∅] |
| 2 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[B2]；S2=[B · ?(D) · C]；S3=[?(A) · B · C2]；S4=[D3 · C]；S5=[A3]；S6=[∅] |
| 3 | Move S4→S2 C1（run=1，Count=1） | 无 | S1=[B2]；S2=[B · ?(D) · C2]；S3=[?(A) · B · C2]；S4=[D3]；S5=[A3]；S6=[∅] |
| 4 | Move S2→S6 C2（run=2，Count=2）；Hatch S2 → D | 无 | S1=[B2]；S2=[B · D]；S3=[?(A) · B · C2]；S4=[D3]；S5=[A3]；S6=[C2] |
| 5 | Move S2→S4 D1（run=1，Count=1）；Home D@S4；Retire S4 | D | S1=[B2]；S2=[B]；S3=[?(A) · B · C2]；S4=[∅][Retired]；S5=[A3]；S6=[C2] |
| 6 | Move S1→S2 B2（run=2，Count=2） | D | S1=[∅]；S2=[B3]；S3=[?(A) · B · C2]；S4=[∅][Retired]；S5=[A3]；S6=[C2] |
| 7 | Move S3→S6 C2（run=2，Count=2）；Home C@S6；Retire S6 | C,D | S1=[∅]；S2=[B3]；S3=[?(A) · B]；S4=[∅][Retired]；S5=[A3]；S6=[∅][Retired] |
| 8 | Move S3→S2 B1（run=1，Count=1）；Hatch S3 → A；Home B@S2；Retire S2 | B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[A]；S4=[∅][Retired]；S5=[A3]；S6=[∅][Retired] |
| 9 | Move S3→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡17

竹林挑战；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A · C · B · A` | `A · C · B · A` |
| S2 | 4 | `A2 · D · B` | `A2 · D · B` |
| S3 | 4 | `?(C) · B · D2` | `? · B · D2` |
| S4 | 4 | `C2 · D · B` | `C2 · D · B` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：13个初始连续段、1颗问号蛋。主路13步，仅存在性见证。首次归巢在第8步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第10步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A1（run=1，Count=1） | 无 | S1=[A · C · B]；S2=[A2 · D · B]；S3=[?(C) · B · D2]；S4=[C2 · D · B]；S5=[A]；S6=[∅] |
| 2 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[A · C · B2]；S2=[A2 · D]；S3=[?(C) · B · D2]；S4=[C2 · D · B]；S5=[A]；S6=[∅] |
| 3 | Move S1→S6 B2（run=2，Count=2） | 无 | S1=[A · C]；S2=[A2 · D]；S3=[?(C) · B · D2]；S4=[C2 · D · B]；S5=[A]；S6=[B2] |
| 4 | Move S4→S6 B1（run=1，Count=1） | 无 | S1=[A · C]；S2=[A2 · D]；S3=[?(C) · B · D2]；S4=[C2 · D]；S5=[A]；S6=[B3] |
| 5 | Move S2→S4 D1（run=1，Count=1） | 无 | S1=[A · C]；S2=[A2]；S3=[?(C) · B · D2]；S4=[C2 · D2]；S5=[A]；S6=[B3] |
| 6 | Move S2→S5 A2（run=2，Count=2） | 无 | S1=[A · C]；S2=[∅]；S3=[?(C) · B · D2]；S4=[C2 · D2]；S5=[A3]；S6=[B3] |
| 7 | Move S1→S2 C1（run=1，Count=1） | 无 | S1=[A]；S2=[C]；S3=[?(C) · B · D2]；S4=[C2 · D2]；S5=[A3]；S6=[B3] |
| 8 | Move S1→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A | S1=[∅]；S2=[C]；S3=[?(C) · B · D2]；S4=[C2 · D2]；S5=[∅][Retired]；S6=[B3] |
| 9 | Move S3→S1 D2（run=2，Count=2） | A | S1=[D2]；S2=[C]；S3=[?(C) · B]；S4=[C2 · D2]；S5=[∅][Retired]；S6=[B3] |
| 10 | Move S3→S6 B1（run=1，Count=1）；Hatch S3 → C；Home B@S6；Retire S6 | A,B | S1=[D2]；S2=[C]；S3=[C]；S4=[C2 · D2]；S5=[∅][Retired]；S6=[∅][Retired] |
| 11 | Move S2→S3 C1（run=1，Count=1） | A,B | S1=[D2]；S2=[∅]；S3=[C2]；S4=[C2 · D2]；S5=[∅][Retired]；S6=[∅][Retired] |
| 12 | Move S4→S1 D2（run=2，Count=2）；Home D@S1；Retire S1 | A,B,D | S1=[∅][Retired]；S2=[∅]；S3=[C2]；S4=[C2]；S5=[∅][Retired]；S6=[∅][Retired] |
| 13 | Move S3→S4 C2（run=2，Count=2）；Home C@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡18

逐层收尾；世界B 竹林；N=4，K=4，P=6；所有枝相同容量；每种含蛋共4token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-SIMPLE；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 4 | `A · ?(B) · C · D` | `A · ? · C · D` |
| S2 | 4 | `A2 · B · C` | `A2 · B · C` |
| S3 | 4 | `B2 · D2` | `B2 · D2` |
| S4 | 4 | `A · ?(C) · C · D` | `A · ? · C · D` |
| S5 | 4 | `∅` | `∅` |
| S6 | 4 | `∅` | `∅` |

本关固定结构：13个初始连续段、2颗问号蛋。主路11步，仅存在性见证。首次归巢在第5步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第6,7步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 D1（run=1，Count=1） | 无 | S1=[A · ?(B) · C]；S2=[A2 · B · C]；S3=[B2 · D2]；S4=[A · ?(C) · C · D]；S5=[D]；S6=[∅] |
| 2 | Move S2→S1 C1（run=1，Count=1） | 无 | S1=[A · ?(B) · C2]；S2=[A2 · B]；S3=[B2 · D2]；S4=[A · ?(C) · C · D]；S5=[D]；S6=[∅] |
| 3 | Move S3→S5 D2（run=2，Count=2） | 无 | S1=[A · ?(B) · C2]；S2=[A2 · B]；S3=[B2]；S4=[A · ?(C) · C · D]；S5=[D3]；S6=[∅] |
| 4 | Move S2→S3 B1（run=1，Count=1） | 无 | S1=[A · ?(B) · C2]；S2=[A2]；S3=[B3]；S4=[A · ?(C) · C · D]；S5=[D3]；S6=[∅] |
| 5 | Move S4→S5 D1（run=1，Count=1）；Home D@S5；Retire S5 | D | S1=[A · ?(B) · C2]；S2=[A2]；S3=[B3]；S4=[A · ?(C) · C]；S5=[∅][Retired]；S6=[∅] |
| 6 | Move S1→S6 C2（run=2，Count=2）；Hatch S1 → B | D | S1=[A · B]；S2=[A2]；S3=[B3]；S4=[A · ?(C) · C]；S5=[∅][Retired]；S6=[C2] |
| 7 | Move S4→S6 C1（run=1，Count=1）；Hatch S4 → C | D | S1=[A · B]；S2=[A2]；S3=[B3]；S4=[A · C]；S5=[∅][Retired]；S6=[C3] |
| 8 | Move S1→S3 B1（run=1，Count=1）；Home B@S3；Retire S3 | B,D | S1=[A]；S2=[A2]；S3=[∅][Retired]；S4=[A · C]；S5=[∅][Retired]；S6=[C3] |
| 9 | Move S1→S2 A1（run=1，Count=1） | B,D | S1=[∅]；S2=[A3]；S3=[∅][Retired]；S4=[A · C]；S5=[∅][Retired]；S6=[C3] |
| 10 | Move S4→S6 C1（run=1，Count=1）；Home C@S6；Retire S6 | B,C,D | S1=[∅]；S2=[A3]；S3=[∅][Retired]；S4=[A]；S5=[∅][Retired]；S6=[∅][Retired] |
| 11 | Move S2→S4 A3（run=3，Count=3）；Home A@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡19

六座长群；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · B3` | `A3 · B3` |
| S2 | 6 | `B3 · C3` | `B3 · C3` |
| S3 | 6 | `C3 · D3` | `C3 · D3` |
| S4 | 6 | `D3 · A3` | `D3 · A3` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：8个初始连续段、0颗问号蛋。主路5步，仅存在性见证。首次归巢在第2步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 B3（run=3，Count=3） | 无 | S1=[A3]；S2=[B3 · C3]；S3=[C3 · D3]；S4=[D3 · A3]；S5=[B3]；S6=[∅] |
| 2 | Move S4→S1 A3（run=3，Count=3）；Home A@S1；Retire S1 | A | S1=[∅][Retired]；S2=[B3 · C3]；S3=[C3 · D3]；S4=[D3]；S5=[B3]；S6=[∅] |
| 3 | Move S3→S4 D3（run=3，Count=3）；Home D@S4；Retire S4 | A,D | S1=[∅][Retired]；S2=[B3 · C3]；S3=[C3]；S4=[∅][Retired]；S5=[B3]；S6=[∅] |
| 4 | Move S2→S3 C3（run=3，Count=3）；Home C@S3；Retire S3 | A,C,D | S1=[∅][Retired]；S2=[B3]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[B3]；S6=[∅] |
| 5 | Move S2→S5 B3（run=3，Count=3）；Home B@S5；Retire S5 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡20

长群分成几段；世界C 秋日枫林；N=6，K=3，P=5；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A · C2 · A3` | `A · C2 · A3` |
| S2 | 6 | `B2 · A · C2 · A` | `B2 · A · C2 · A` |
| S3 | 6 | `C · B4 · C` | `C · B4 · C` |
| S4 | 6 | `∅` | `∅` |
| S5 | 6 | `∅` | `∅` |

本关固定结构：10个初始连续段、0颗问号蛋。主路8步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S4 A3（run=3，Count=3） | 无 | S1=[A · C2]；S2=[B2 · A · C2 · A]；S3=[C · B4 · C]；S4=[A3]；S5=[∅] |
| 2 | Move S2→S4 A1（run=1，Count=1） | 无 | S1=[A · C2]；S2=[B2 · A · C2]；S3=[C · B4 · C]；S4=[A4]；S5=[∅] |
| 3 | Move S2→S1 C2（run=2，Count=2） | 无 | S1=[A · C4]；S2=[B2 · A]；S3=[C · B4 · C]；S4=[A4]；S5=[∅] |
| 4 | Move S2→S4 A1（run=1，Count=1） | 无 | S1=[A · C4]；S2=[B2]；S3=[C · B4 · C]；S4=[A5]；S5=[∅] |
| 5 | Move S3→S1 C1（run=1，Count=1） | 无 | S1=[A · C5]；S2=[B2]；S3=[C · B4]；S4=[A5]；S5=[∅] |
| 6 | Move S3→S2 B4（run=4，Count=4）；Home B@S2；Retire S2 | B | S1=[A · C5]；S2=[∅][Retired]；S3=[C]；S4=[A5]；S5=[∅] |
| 7 | Move S1→S3 C5（run=5，Count=5）；Home C@S3；Retire S3 | B,C | S1=[A]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[A5]；S5=[∅] |
| 8 | Move S1→S4 A1（run=1，Count=1）；Home A@S4；Retire S4 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。
