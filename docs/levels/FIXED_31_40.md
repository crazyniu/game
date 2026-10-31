# 第31–40关：固定候选与纸面完整回放

2026-10-31 · DESIGNED / INDEPENDENT_REPLAY_PASS / PENDING_GRAYBOX。这是本批**固定初态唯一权威**；[设计卡](../LEVEL_DESIGN.md)只拥有教学/节奏，[模拟入口](../LEVEL_SIMULATIONS.md)拥有记号与证据边界，[RULES](../spec/RULES.md)拥有算法。仅Markdown候选，未修改游戏关卡JSON。

数组末项外露，`蛋(A)`是一只固定A种蛋，普通`E`始终是E鸟。`A3`精确等于三只A，不是随机数量；展开原序分配token ID。未列机关不存在。Complete/Sleep与Complete/Home表示本次首次成组六只、完成数+1；AutoHome不重复加数。表内接收临时枝事件包含首次Armed或已Armed再次接收，具体三态已在完整快照标出。

每条主路/对照路由独立文档模型逐步校验合法性、锁run/Count、稳定化、每种六只守恒及全空终态；不是项目Core/Unity/最优/真人证书。下面数值是路线长度，不是solverMinMoves。固定内容不得运行时生成、换色或换seed。

## 关卡31

K6/P8；Q=`A,F,C,D,E,B`。C/D两睡眠，A一操作后自动连收；非Unity Undo验收。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · C3` | 普通枝 |
| S2 | 6 | `F3 · D3` | 普通枝 |
| S3 | 6 | `C3` | 普通枝 |
| S4 | 6 | `D3` | 普通枝 |
| S5 | 6 | `A3` | 普通枝 |
| S6 | 6 | `F3 · E3` | 普通枝 |
| S7 | 6 | `E3 · B3` | 普通枝 |
| S8 | 6 | `B3` | 普通枝 |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S3 C3；Complete/Sleep C@S3；run=3,Count=3 | 1 / 无 / AF | S1=[A3]；S2=[F3 · D3]；S3=[C6][睡眠]；S4=[D3]；S5=[A3]；S6=[F3 · E3]；S7=[E3 · B3]；S8=[B3] |
| 2 | Move S2→S4 D3；Complete/Sleep D@S4；run=3,Count=3 | 2 / 无 / AF | S1=[A3]；S2=[F3]；S3=[C6][睡眠]；S4=[D6][睡眠]；S5=[A3]；S6=[F3 · E3]；S7=[E3 · B3]；S8=[B3] |
| 3 | Move S1→S5 A3；Complete/Home A@S5；AutoHome C@S3；AutoHome D@S4；run=3,Count=3 | 3 / ACD / FE | S1=[∅]；S2=[F3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[F3 · E3]；S7=[E3 · B3]；S8=[B3] |
| 4 | Move S7→S8 B3；Complete/Sleep B@S8；run=3,Count=3 | 4 / ACD / FE | S1=[∅]；S2=[F3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[F3 · E3]；S7=[E3]；S8=[B6][睡眠] |
| 5 | Move S6→S7 E3；Complete/Home E@S7；AutoHome B@S8；run=3,Count=3 | 5 / ACDEB / F | S1=[∅]；S2=[F3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[F3]；S7=[∅]；S8=[∅] |
| 6 | Move S2→S6 F3；Complete/Home F@S6；run=3,Count=3 | 6 / AFCDEB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡32

K6/P8；Q=`B,E,A,F,D,C`。E先第二步开箱；B先第四步才开箱，两条具体见证八/九步非最优。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `E5 · A` | 普通枝 |
| S2 | 6 | `B5 · F` | 普通枝 |
| S3 | 6 | `C5 · D` | 普通枝；盖箱 RevealSpecies=E |
| S4 | 6 | `A5` | 普通枝 |
| S5 | 6 | `F5 · E` | 普通枝 |
| S6 | 6 | `D5 · C` | 普通枝 |
| S7 | 6 | `B` | 普通枝 |
| S8 | 4 | `∅` | 普通枝 |

### 主路线（8步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S8 A1；run=1,Count=1 | 0 / 无 / BE | S1=[E5]；S2=[B5 · F]；S3=[C5 · D][盖箱]；S4=[A5]；S5=[F5 · E]；S6=[D5 · C]；S7=[B]；S8=[A] |
| 2 | Move S5→S1 E1；Complete/Home E@S1；Reveal S3；run=1,Count=1 | 1 / E / BA | S1=[∅]；S2=[B5 · F]；S3=[C5 · D]；S4=[A5]；S5=[F5]；S6=[D5 · C]；S7=[B]；S8=[A] |
| 3 | Move S8→S4 A1；Complete/Home A@S4；run=1,Count=1 | 2 / EA / BF | S1=[∅]；S2=[B5 · F]；S3=[C5 · D]；S4=[∅]；S5=[F5]；S6=[D5 · C]；S7=[B]；S8=[∅] |
| 4 | Move S2→S5 F1；Complete/Home F@S5；run=1,Count=1 | 3 / EAF / BD | S1=[∅]；S2=[B5]；S3=[C5 · D]；S4=[∅]；S5=[∅]；S6=[D5 · C]；S7=[B]；S8=[∅] |
| 5 | Move S7→S2 B1；Complete/Home B@S2；run=1,Count=1 | 4 / BEAF / DC | S1=[∅]；S2=[∅]；S3=[C5 · D]；S4=[∅]；S5=[∅]；S6=[D5 · C]；S7=[∅]；S8=[∅] |
| 6 | Move S6→S1 C1；run=1,Count=1 | 4 / BEAF / DC | S1=[C]；S2=[∅]；S3=[C5 · D]；S4=[∅]；S5=[∅]；S6=[D5]；S7=[∅]；S8=[∅] |
| 7 | Move S3→S6 D1；Complete/Home D@S6；run=1,Count=1 | 5 / BEAFD / C | S1=[C]；S2=[∅]；S3=[C5]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |
| 8 | Move S1→S3 C1；Complete/Home C@S3；run=1,Count=1 | 6 / BEAFDC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |


### 对照路线1（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S8 F1；run=1,Count=1 | 0 / 无 / BE | S1=[E5 · A]；S2=[B5]；S3=[C5 · D][盖箱]；S4=[A5]；S5=[F5 · E]；S6=[D5 · C]；S7=[B]；S8=[F] |
| 2 | Move S7→S2 B1；Complete/Home B@S2；run=1,Count=1 | 1 / B / EA | S1=[E5 · A]；S2=[∅]；S3=[C5 · D][盖箱]；S4=[A5]；S5=[F5 · E]；S6=[D5 · C]；S7=[∅]；S8=[F] |
| 3 | Move S1→S2 A1；run=1,Count=1 | 1 / B / EA | S1=[E5]；S2=[A]；S3=[C5 · D][盖箱]；S4=[A5]；S5=[F5 · E]；S6=[D5 · C]；S7=[∅]；S8=[F] |
| 4 | Move S5→S1 E1；Complete/Home E@S1；Reveal S3；run=1,Count=1 | 2 / BE / AF | S1=[∅]；S2=[A]；S3=[C5 · D]；S4=[A5]；S5=[F5]；S6=[D5 · C]；S7=[∅]；S8=[F] |
| 5 | Move S8→S5 F1；Complete/Home F@S5；run=1,Count=1 | 3 / BEF / AD | S1=[∅]；S2=[A]；S3=[C5 · D]；S4=[A5]；S5=[∅]；S6=[D5 · C]；S7=[∅]；S8=[∅] |
| 6 | Move S2→S4 A1；Complete/Home A@S4；run=1,Count=1 | 4 / BEAF / DC | S1=[∅]；S2=[∅]；S3=[C5 · D]；S4=[∅]；S5=[∅]；S6=[D5 · C]；S7=[∅]；S8=[∅] |
| 7 | Move S6→S1 C1；run=1,Count=1 | 4 / BEAF / DC | S1=[C]；S2=[∅]；S3=[C5 · D]；S4=[∅]；S5=[∅]；S6=[D5]；S7=[∅]；S8=[∅] |
| 8 | Move S3→S6 D1；Complete/Home D@S6；run=1,Count=1 | 5 / BEAFD / C | S1=[C]；S2=[∅]；S3=[C5]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |
| 9 | Move S1→S3 C1；Complete/Home C@S3；run=1,Count=1 | 6 / BEAFDC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡33

K6/P8；Q=`C,A,F,B,E,D`。A2只出1保Armed，再接第二蛋排A1；与26一次接收不同。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `C3 · 蛋(C) · A2` | 普通枝 |
| S2 | 6 | `F4 · 蛋(F) · A` | 普通枝 |
| S3 | 6 | `B2 · A3` | 普通枝 |
| S4 | 6 | `D4 · 蛋(D) · B` | 普通枝 |
| S5 | 6 | `C2` | 普通枝 |
| S6 | 6 | `E5 · B` | 普通枝 |
| S7 | 6 | `D · F · B2 · E` | 普通枝 |
| S8 | 5 | `∅` | 临时Unused |

### 主路线（13步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S8 A2；接收临时枝 S8；Hatch C@S1；run=2,Count=2 | 0 / 无 / CA | S1=[C4]；S2=[F4 · 蛋(F) · A]；S3=[B2 · A3]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 2 | Move S8→S3 A1；run=2,Count=1 | 0 / 无 / CA | S1=[C4]；S2=[F4 · 蛋(F) · A]；S3=[B2 · A4]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A][Armed] |
| 3 | Move S2→S8 A1；接收临时枝 S8；Hatch F@S2；run=1,Count=1 | 0 / 无 / CA | S1=[C4]；S2=[F5]；S3=[B2 · A4]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 4 | Move S1→S5 C4；Complete/Home C@S5；run=4,Count=4 | 1 / C / AF | S1=[∅]；S2=[F5]；S3=[B2 · A4]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 5 | Move S3→S1 A4；run=4,Count=4 | 1 / C / AF | S1=[A4]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 6 | Move S8→S1 A2；Complete/Home A@S1；Retire S8；run=2,Count=2 | 2 / CA / FB | S1=[∅]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[∅][Retired] |
| 7 | Move S7→S1 E1；run=1,Count=1 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2]；S8=[∅][Retired] |
| 8 | Move S7→S3 B2；run=2,Count=2 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B4]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F]；S8=[∅][Retired] |
| 9 | Move S4→S3 B1；Hatch D@S4；run=1,Count=1 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B5]；S4=[D5]；S5=[∅]；S6=[E5 · B]；S7=[D · F]；S8=[∅][Retired] |
| 10 | Move S6→S3 B1；Complete/Home B@S3；run=1,Count=1 | 3 / CAB / FE | S1=[E]；S2=[F5]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[E5]；S7=[D · F]；S8=[∅][Retired] |
| 11 | Move S1→S6 E1；Complete/Home E@S6；run=1,Count=1 | 4 / CABE / FD | S1=[∅]；S2=[F5]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[∅]；S7=[D · F]；S8=[∅][Retired] |
| 12 | Move S7→S2 F1；Complete/Home F@S2；run=1,Count=1 | 5 / CAFBE / D | S1=[∅]；S2=[∅]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[∅]；S7=[D]；S8=[∅][Retired] |
| 13 | Move S7→S4 D1；Complete/Home D@S4；run=1,Count=1 | 6 / CAFBED / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅][Retired] |


### 对照路线1（13步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S8 A1；接收临时枝 S8；Hatch F@S2；run=1,Count=1 | 0 / 无 / CA | S1=[C3 · 蛋(C) · A2]；S2=[F5]；S3=[B2 · A3]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A][Armed] |
| 2 | Move S1→S8 A2；接收临时枝 S8；Hatch C@S1；run=2,Count=2 | 0 / 无 / CA | S1=[C4]；S2=[F5]；S3=[B2 · A3]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A3][Armed] |
| 3 | Move S8→S3 A1；run=3,Count=1 | 0 / 无 / CA | S1=[C4]；S2=[F5]；S3=[B2 · A4]；S4=[D4 · 蛋(D) · B]；S5=[C2]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 4 | Move S1→S5 C4；Complete/Home C@S5；run=4,Count=4 | 1 / C / AF | S1=[∅]；S2=[F5]；S3=[B2 · A4]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 5 | Move S3→S1 A4；run=4,Count=4 | 1 / C / AF | S1=[A4]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[A2][Armed] |
| 6 | Move S8→S1 A2；Complete/Home A@S1；Retire S8；run=2,Count=2 | 2 / CA / FB | S1=[∅]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2 · E]；S8=[∅][Retired] |
| 7 | Move S7→S1 E1；run=1,Count=1 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B2]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F · B2]；S8=[∅][Retired] |
| 8 | Move S7→S3 B2；run=2,Count=2 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B4]；S4=[D4 · 蛋(D) · B]；S5=[∅]；S6=[E5 · B]；S7=[D · F]；S8=[∅][Retired] |
| 9 | Move S4→S3 B1；Hatch D@S4；run=1,Count=1 | 2 / CA / FB | S1=[E]；S2=[F5]；S3=[B5]；S4=[D5]；S5=[∅]；S6=[E5 · B]；S7=[D · F]；S8=[∅][Retired] |
| 10 | Move S6→S3 B1；Complete/Home B@S3；run=1,Count=1 | 3 / CAB / FE | S1=[E]；S2=[F5]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[E5]；S7=[D · F]；S8=[∅][Retired] |
| 11 | Move S1→S6 E1；Complete/Home E@S6；run=1,Count=1 | 4 / CABE / FD | S1=[∅]；S2=[F5]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[∅]；S7=[D · F]；S8=[∅][Retired] |
| 12 | Move S7→S2 F1；Complete/Home F@S2；run=1,Count=1 | 5 / CAFBE / D | S1=[∅]；S2=[∅]；S3=[∅]；S4=[D5]；S5=[∅]；S6=[∅]；S7=[D]；S8=[∅][Retired] |
| 13 | Move S7→S4 D1；Complete/Home D@S4；run=1,Count=1 | 6 / CAFBED / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡34

K6/P8；Q=`D,B,E,A,F,C`。双篮先占B/D，B打开种类箱而计数箱仍闭，D再开计数箱；两条件分时。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `D5 · B` | 普通枝 |
| S2 | 6 | `B5 · D` | 普通枝 |
| S3 | 6 | `E5 · A` | 普通枝；盖箱 RevealAfter=2 |
| S4 | 6 | `F5 · C` | 普通枝；盖箱 RevealSpecies=B |
| S5 | 6 | `A5 · F` | 普通枝 |
| S6 | 6 | `C5 · E` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |
| S8 | 6 | `∅` | 普通枝 |
| T1 | 1 | `∅` | 篮 |
| T2 | 1 | `∅` | 篮 |

### 主路线（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 B1；run=1,Count=1 | 0 / 无 / DB | S1=[D5]；S2=[B5 · D]；S3=[E5 · A][盖箱]；S4=[F5 · C][盖箱]；S5=[A5 · F]；S6=[C5 · E]；S7=[∅]；S8=[∅]；T1=[B]；T2=[∅] |
| 2 | Move S2→T2 D1；run=1,Count=1 | 0 / 无 / DB | S1=[D5]；S2=[B5]；S3=[E5 · A][盖箱]；S4=[F5 · C][盖箱]；S5=[A5 · F]；S6=[C5 · E]；S7=[∅]；S8=[∅]；T1=[B]；T2=[D] |
| 3 | Move T1→S2 B1；Complete/Home B@S2；Reveal S4；run=1,Count=1 | 1 / B / DE | S1=[D5]；S2=[∅]；S3=[E5 · A][盖箱]；S4=[F5 · C]；S5=[A5 · F]；S6=[C5 · E]；S7=[∅]；S8=[∅]；T1=[∅]；T2=[D] |
| 4 | Move T2→S1 D1；Complete/Home D@S1；Reveal S3；run=1,Count=1 | 2 / DB / EA | S1=[∅]；S2=[∅]；S3=[E5 · A]；S4=[F5 · C]；S5=[A5 · F]；S6=[C5 · E]；S7=[∅]；S8=[∅]；T1=[∅]；T2=[∅] |
| 5 | Move S4→S7 C1；run=1,Count=1 | 2 / DB / EA | S1=[∅]；S2=[∅]；S3=[E5 · A]；S4=[F5]；S5=[A5 · F]；S6=[C5 · E]；S7=[C]；S8=[∅]；T1=[∅]；T2=[∅] |
| 6 | Move S5→S4 F1；Complete/Sleep F@S4；run=1,Count=1 | 3 / DB / EA | S1=[∅]；S2=[∅]；S3=[E5 · A]；S4=[F6][睡眠]；S5=[A5]；S6=[C5 · E]；S7=[C]；S8=[∅]；T1=[∅]；T2=[∅] |
| 7 | Move S3→S5 A1；Complete/Home A@S5；AutoHome F@S4；run=1,Count=1 | 4 / DBAF / EC | S1=[∅]；S2=[∅]；S3=[E5]；S4=[∅]；S5=[∅]；S6=[C5 · E]；S7=[C]；S8=[∅]；T1=[∅]；T2=[∅] |
| 8 | Move S6→S3 E1；Complete/Home E@S3；run=1,Count=1 | 5 / DBEAF / C | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5]；S7=[C]；S8=[∅]；T1=[∅]；T2=[∅] |
| 9 | Move S7→S6 C1；Complete/Home C@S6；run=1,Count=1 | 6 / DBEAFC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；T1=[∅]；T2=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡35

K6/P8；Q=`E,A,D,C,B,F`。K6低混杂舒缓大段，和30同族，目标顺序/排布不同。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `E3 · A3` | 普通枝 |
| S2 | 6 | `A3 · D3` | 普通枝 |
| S3 | 6 | `D3 · C3` | 普通枝 |
| S4 | 6 | `C3 · B3` | 普通枝 |
| S5 | 6 | `B3 · F3` | 普通枝 |
| S6 | 6 | `F3` | 普通枝 |
| S7 | 6 | `∅` | 普通枝 |
| S8 | 6 | `E3` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S7 A3；run=3,Count=3 | 0 / 无 / EA | S1=[E3]；S2=[A3 · D3]；S3=[D3 · C3]；S4=[C3 · B3]；S5=[B3 · F3]；S6=[F3]；S7=[A3]；S8=[E3] |
| 2 | Move S8→S1 E3；Complete/Home E@S1；run=3,Count=3 | 1 / E / AD | S1=[∅]；S2=[A3 · D3]；S3=[D3 · C3]；S4=[C3 · B3]；S5=[B3 · F3]；S6=[F3]；S7=[A3]；S8=[∅] |
| 3 | Move S5→S6 F3；Complete/Sleep F@S6；run=3,Count=3 | 2 / E / AD | S1=[∅]；S2=[A3 · D3]；S3=[D3 · C3]；S4=[C3 · B3]；S5=[B3]；S6=[F6][睡眠]；S7=[A3]；S8=[∅] |
| 4 | Move S4→S5 B3；Complete/Sleep B@S5；run=3,Count=3 | 3 / E / AD | S1=[∅]；S2=[A3 · D3]；S3=[D3 · C3]；S4=[C3]；S5=[B6][睡眠]；S6=[F6][睡眠]；S7=[A3]；S8=[∅] |
| 5 | Move S3→S4 C3；Complete/Sleep C@S4；run=3,Count=3 | 4 / E / AD | S1=[∅]；S2=[A3 · D3]；S3=[D3]；S4=[C6][睡眠]；S5=[B6][睡眠]；S6=[F6][睡眠]；S7=[A3]；S8=[∅] |
| 6 | Move S2→S3 D3；Complete/Home D@S3；AutoHome C@S4；AutoHome B@S5；AutoHome F@S6；run=3,Count=3 | 5 / EDCBF / A | S1=[∅]；S2=[A3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[A3]；S8=[∅] |
| 7 | Move S7→S2 A3；Complete/Home A@S2；run=3,Count=3 | 6 / EADCBF / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡36

K7/P9；Q=`A,G,C,F,B,E,D`。第一排普通枝，前四步临时Unused；第二排首次用临时枝；早用也可赢多一步。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · 蛋(A)2 · G` | 普通枝 |
| S2 | 6 | `G5 · A` | 普通枝 |
| S3 | 6 | `C3 · 蛋(C)2 · F` | 普通枝 |
| S4 | 6 | `F5 · C` | 普通枝 |
| S5 | 6 | `B5 · E` | 普通枝 |
| S6 | 6 | `E5 · D` | 普通枝 |
| S7 | 6 | `D5 · B` | 普通枝 |
| S8 | 6 | `∅` | 普通枝 |
| S9 | 4 | `∅` | 临时Unused |

### 主路线（13步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S8 A1；run=1,Count=1 | 0 / 无 / AG | S1=[A3 · 蛋(A)2 · G]；S2=[G5]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[A]；S9=[∅][Unused] |
| 2 | Move S1→S2 G1；Hatch A@S1；Complete/Home G@S2；run=1,Count=1 | 1 / G / AC | S1=[A3 · 蛋(A) · A]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[A]；S9=[∅][Unused] |
| 3 | Move S1→S8 A1；Hatch A@S1；run=1,Count=1 | 1 / G / AC | S1=[A4]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[A2]；S9=[∅][Unused] |
| 4 | Move S1→S8 A4；Complete/Home A@S8；run=4,Count=4 | 2 / AG / CF | S1=[∅]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Unused] |
| 5 | Move S3→S9 F1；接收临时枝 S9；Hatch C@S3；run=1,Count=1 | 2 / AG / CF | S1=[∅]；S2=[∅]；S3=[C3 · 蛋(C) · C]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[F][Armed] |
| 6 | Move S4→S1 C1；run=1,Count=1 | 2 / AG / CF | S1=[C]；S2=[∅]；S3=[C3 · 蛋(C) · C]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[F][Armed] |
| 7 | Move S3→S1 C1；Hatch C@S3；run=1,Count=1 | 2 / AG / CF | S1=[C2]；S2=[∅]；S3=[C4]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[F][Armed] |
| 8 | Move S3→S1 C4；Complete/Home C@S1；run=4,Count=4 | 3 / AGC / FB | S1=[∅]；S2=[∅]；S3=[∅]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[F][Armed] |
| 9 | Move S9→S4 F1；Complete/Home F@S4；Retire S9；run=1,Count=1 | 4 / AGCF / BE | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 10 | Move S5→S1 E1；run=1,Count=1 | 4 / AGCF / BE | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[B5]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 11 | Move S7→S5 B1；Complete/Home B@S5；run=1,Count=1 | 5 / AGCFB / ED | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[E5 · D]；S7=[D5]；S8=[∅]；S9=[∅][Retired] |
| 12 | Move S6→S7 D1；Complete/Home D@S7；run=1,Count=1 | 6 / AGCFBD / E | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[E5]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |
| 13 | Move S1→S6 E1；Complete/Home E@S6；run=1,Count=1 | 7 / AGCFBED / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |


### 对照路线1（14步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S9 A1；接收临时枝 S9；run=1,Count=1 | 0 / 无 / AG | S1=[A3 · 蛋(A)2 · G]；S2=[G5]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[A][Armed] |
| 2 | Move S1→S2 G1；Hatch A@S1；Complete/Home G@S2；run=1,Count=1 | 1 / G / AC | S1=[A3 · 蛋(A) · A]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[A][Armed] |
| 3 | Move S1→S9 A1；接收临时枝 S9；Hatch A@S1；run=1,Count=1 | 1 / G / AC | S1=[A4]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[A2][Armed] |
| 4 | Move S1→S9 A2；接收临时枝 S9；run=4,Count=2 | 1 / G / AC | S1=[A2]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[A4][Armed] |
| 5 | Move S9→S1 A4；Complete/Home A@S1；Retire S9；run=4,Count=4 | 2 / AG / CF | S1=[∅]；S2=[∅]；S3=[C3 · 蛋(C)2 · F]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 6 | Move S3→S2 F1；Hatch C@S3；run=1,Count=1 | 2 / AG / CF | S1=[∅]；S2=[F]；S3=[C3 · 蛋(C) · C]；S4=[F5 · C]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 7 | Move S4→S1 C1；run=1,Count=1 | 2 / AG / CF | S1=[C]；S2=[F]；S3=[C3 · 蛋(C) · C]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 8 | Move S3→S1 C1；Hatch C@S3；run=1,Count=1 | 2 / AG / CF | S1=[C2]；S2=[F]；S3=[C4]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 9 | Move S3→S1 C4；Complete/Home C@S1；run=4,Count=4 | 3 / AGC / FB | S1=[∅]；S2=[F]；S3=[∅]；S4=[F5]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 10 | Move S2→S4 F1；Complete/Home F@S4；run=1,Count=1 | 4 / AGCF / BE | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[B5 · E]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 11 | Move S5→S1 E1；run=1,Count=1 | 4 / AGCF / BE | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[B5]；S6=[E5 · D]；S7=[D5 · B]；S8=[∅]；S9=[∅][Retired] |
| 12 | Move S7→S5 B1；Complete/Home B@S5；run=1,Count=1 | 5 / AGCFB / ED | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[E5 · D]；S7=[D5]；S8=[∅]；S9=[∅][Retired] |
| 13 | Move S6→S7 D1；Complete/Home D@S7；run=1,Count=1 | 6 / AGCFBD / E | S1=[E]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[E5]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |
| 14 | Move S1→S6 E1；Complete/Home E@S6；run=1,Count=1 | 7 / AGCFBED / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡37

K7/P9；Q=`B,F,G,D,A,C,E`。F种类箱先开，箱内B触发累计2箱；临时A待新长枝接回。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `F5 · A` | 普通枝 |
| S2 | 6 | `B5 · F` | 普通枝 |
| S3 | 6 | `E5 · D` | 普通枝；盖箱 RevealAfter=2 |
| S4 | 6 | `G5 · B` | 普通枝；盖箱 RevealSpecies=F |
| S5 | 6 | `A5 · G` | 普通枝 |
| S6 | 6 | `D5 · C` | 普通枝 |
| S7 | 6 | `C5 · E` | 普通枝 |
| S8 | 6 | `∅` | 普通枝 |
| S9 | 5 | `∅` | 临时Unused |

### 主路线（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S9 A1；接收临时枝 S9；run=1,Count=1 | 0 / 无 / BF | S1=[F5]；S2=[B5 · F]；S3=[E5 · D][盖箱]；S4=[G5 · B][盖箱]；S5=[A5 · G]；S6=[D5 · C]；S7=[C5 · E]；S8=[∅]；S9=[A][Armed] |
| 2 | Move S2→S1 F1；Complete/Home F@S1；Reveal S4；run=1,Count=1 | 1 / F / BG | S1=[∅]；S2=[B5]；S3=[E5 · D][盖箱]；S4=[G5 · B]；S5=[A5 · G]；S6=[D5 · C]；S7=[C5 · E]；S8=[∅]；S9=[A][Armed] |
| 3 | Move S4→S2 B1；Complete/Home B@S2；Reveal S3；run=1,Count=1 | 2 / BF / GD | S1=[∅]；S2=[∅]；S3=[E5 · D]；S4=[G5]；S5=[A5 · G]；S6=[D5 · C]；S7=[C5 · E]；S8=[∅]；S9=[A][Armed] |
| 4 | Move S5→S4 G1；Complete/Home G@S4；run=1,Count=1 | 3 / BFG / DA | S1=[∅]；S2=[∅]；S3=[E5 · D]；S4=[∅]；S5=[A5]；S6=[D5 · C]；S7=[C5 · E]；S8=[∅]；S9=[A][Armed] |
| 5 | Move S9→S5 A1；Complete/Home A@S5；Retire S9；run=1,Count=1 | 4 / BFGA / DC | S1=[∅]；S2=[∅]；S3=[E5 · D]；S4=[∅]；S5=[∅]；S6=[D5 · C]；S7=[C5 · E]；S8=[∅]；S9=[∅][Retired] |
| 6 | Move S7→S1 E1；run=1,Count=1 | 4 / BFGA / DC | S1=[E]；S2=[∅]；S3=[E5 · D]；S4=[∅]；S5=[∅]；S6=[D5 · C]；S7=[C5]；S8=[∅]；S9=[∅][Retired] |
| 7 | Move S6→S7 C1；Complete/Home C@S7；run=1,Count=1 | 5 / BFGAC / DE | S1=[E]；S2=[∅]；S3=[E5 · D]；S4=[∅]；S5=[∅]；S6=[D5]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |
| 8 | Move S3→S6 D1；Complete/Home D@S6；run=1,Count=1 | 6 / BFGDAC / E | S1=[E]；S2=[∅]；S3=[E5]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |
| 9 | Move S1→S3 E1；Complete/Home E@S3；run=1,Count=1 | 7 / BFGDACE / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡38

K7/P9；Q=`C,G,A,B,F,D,E`。篮拆G1+普通短枝接G2，再收大run；熟悉组合舒缓。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `C3 · G3` | 普通枝 |
| S2 | 6 | `G3 · A3` | 普通枝 |
| S3 | 6 | `A3 · B3` | 普通枝 |
| S4 | 6 | `B3 · F3` | 普通枝 |
| S5 | 6 | `F3 · D3` | 普通枝 |
| S6 | 6 | `D3 · E3` | 普通枝 |
| S7 | 6 | `E3 · C3` | 普通枝 |
| S8 | 6 | `∅` | 普通枝 |
| S9 | 5 | `∅` | 普通枝 |
| T1 | 1 | `∅` | 篮 |

### 主路线（10步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 G1；run=3,Count=1 | 0 / 无 / CG | S1=[C3 · G2]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3 · F3]；S5=[F3 · D3]；S6=[D3 · E3]；S7=[E3 · C3]；S8=[∅]；S9=[∅]；T1=[G] |
| 2 | Move S1→S9 G2；run=2,Count=2 | 0 / 无 / CG | S1=[C3]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3 · F3]；S5=[F3 · D3]；S6=[D3 · E3]；S7=[E3 · C3]；S8=[∅]；S9=[G2]；T1=[G] |
| 3 | Move S7→S1 C3；Complete/Home C@S1；run=3,Count=3 | 1 / C / GA | S1=[∅]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3 · F3]；S5=[F3 · D3]；S6=[D3 · E3]；S7=[E3]；S8=[∅]；S9=[G2]；T1=[G] |
| 4 | Move S6→S7 E3；Complete/Sleep E@S7；run=3,Count=3 | 2 / C / GA | S1=[∅]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3 · F3]；S5=[F3 · D3]；S6=[D3]；S7=[E6][睡眠]；S8=[∅]；S9=[G2]；T1=[G] |
| 5 | Move S5→S6 D3；Complete/Sleep D@S6；run=3,Count=3 | 3 / C / GA | S1=[∅]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3 · F3]；S5=[F3]；S6=[D6][睡眠]；S7=[E6][睡眠]；S8=[∅]；S9=[G2]；T1=[G] |
| 6 | Move S4→S5 F3；Complete/Sleep F@S5；run=3,Count=3 | 4 / C / GA | S1=[∅]；S2=[G3 · A3]；S3=[A3 · B3]；S4=[B3]；S5=[F6][睡眠]；S6=[D6][睡眠]；S7=[E6][睡眠]；S8=[∅]；S9=[G2]；T1=[G] |
| 7 | Move S3→S4 B3；Complete/Sleep B@S4；run=3,Count=3 | 5 / C / GA | S1=[∅]；S2=[G3 · A3]；S3=[A3]；S4=[B6][睡眠]；S5=[F6][睡眠]；S6=[D6][睡眠]；S7=[E6][睡眠]；S8=[∅]；S9=[G2]；T1=[G] |
| 8 | Move S2→S3 A3；Complete/Home A@S3；AutoHome B@S4；AutoHome F@S5；AutoHome D@S6；AutoHome E@S7；run=3,Count=3 | 6 / CABFDE / G | S1=[∅]；S2=[G3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[G2]；T1=[G] |
| 9 | Move S9→S2 G2；run=2,Count=2 | 6 / CABFDE / G | S1=[∅]；S2=[G5]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅]；T1=[G] |
| 10 | Move T1→S2 G1；Complete/Home G@S2；run=1,Count=1 | 7 / CGABFDE / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅]；T1=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡39

K8/P10；Q=`D,H,F,A,G,C,B,E`。四座临时枝服务首群后退，五座跨目标更新再退；有错误退役死局反例。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `D3 · 蛋(D)2 · H` | 普通枝 |
| S2 | 6 | `H3 · 蛋(H)2 · F` | 普通枝 |
| S3 | 6 | `F5 · D` | 普通枝 |
| S4 | 6 | `A3 · 蛋(A)2 · G` | 普通枝 |
| S5 | 6 | `G5 · A` | 普通枝 |
| S6 | 6 | `C5 · B` | 普通枝 |
| S7 | 6 | `B5 · E` | 普通枝 |
| S8 | 6 | `E5 · C` | 普通枝 |
| S9 | 4 | `∅` | 临时Unused |
| S10 | 5 | `∅` | 临时Unused |

### 主路线（17步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S9 D1；接收临时枝 S9；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D][Armed]；S10=[∅][Unused] |
| 2 | Move S1→S10 H1；接收临时枝 S10；Hatch D@S1；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D) · D]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D][Armed]；S10=[H][Armed] |
| 3 | Move S1→S9 D1；接收临时枝 S9；Hatch D@S1；run=1,Count=1 | 0 / 无 / DH | S1=[D4]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D2][Armed]；S10=[H][Armed] |
| 4 | Move S1→S9 D2；接收临时枝 S9；run=4,Count=2 | 0 / 无 / DH | S1=[D2]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D4][Armed]；S10=[H][Armed] |
| 5 | Move S9→S1 D4；Complete/Home D@S1；Retire S9；run=4,Count=4 | 1 / D / HF | S1=[∅]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 6 | Move S2→S3 F1；Hatch H@S2；Complete/Home F@S3；run=1,Count=1 | 2 / DF / HA | S1=[∅]；S2=[H3 · 蛋(H) · H]；S3=[∅]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 7 | Move S2→S10 H1；接收临时枝 S10；Hatch H@S2；run=1,Count=1 | 2 / DF / HA | S1=[∅]；S2=[H4]；S3=[∅]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H2][Armed] |
| 8 | Move S2→S10 H3；接收临时枝 S10；run=4,Count=3 | 2 / DF / HA | S1=[∅]；S2=[H]；S3=[∅]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H5][Armed] |
| 9 | Move S10→S2 H5；Complete/Home H@S2；Retire S10；run=5,Count=5 | 3 / DHF / AG | S1=[∅]；S2=[∅]；S3=[∅]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 10 | Move S5→S1 A1；run=1,Count=1 | 3 / DHF / AG | S1=[A]；S2=[∅]；S3=[∅]；S4=[A3 · 蛋(A)2 · G]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 11 | Move S4→S5 G1；Hatch A@S4；Complete/Home G@S5；run=1,Count=1 | 4 / DHFG / AC | S1=[A]；S2=[∅]；S3=[∅]；S4=[A3 · 蛋(A) · A]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 12 | Move S4→S1 A1；Hatch A@S4；run=1,Count=1 | 4 / DHFG / AC | S1=[A2]；S2=[∅]；S3=[∅]；S4=[A4]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 13 | Move S4→S1 A4；Complete/Home A@S1；run=4,Count=4 | 5 / DHFAG / CB | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 14 | Move S6→S1 B1；run=1,Count=1 | 5 / DHFAG / CB | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 15 | Move S8→S6 C1；Complete/Home C@S6；run=1,Count=1 | 6 / DHFAGC / BE | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[B5 · E]；S8=[E5]；S9=[∅][Retired]；S10=[∅][Retired] |
| 16 | Move S7→S8 E1；Complete/Home E@S8；run=1,Count=1 | 7 / DHFAGCE / B | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[B5]；S8=[∅]；S9=[∅][Retired]；S10=[∅][Retired] |
| 17 | Move S1→S7 B1；Complete/Home B@S7；run=1,Count=1 | 8 / DHFAGCBE / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired]；S10=[∅][Retired] |


### 对照路线1（17步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S9 D1；接收临时枝 S9；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D][Armed]；S10=[∅][Unused] |
| 2 | Move S1→S10 H1；接收临时枝 S10；Hatch D@S1；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D) · D]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D][Armed]；S10=[H][Armed] |
| 3 | Move S1→S9 D1；接收临时枝 S9；Hatch D@S1；run=1,Count=1 | 0 / 无 / DH | S1=[D4]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D2][Armed]；S10=[H][Armed] |
| 4 | Move S1→S9 D2；接收临时枝 S9；run=4,Count=2 | 0 / 无 / DH | S1=[D2]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[D4][Armed]；S10=[H][Armed] |
| 5 | Move S9→S1 D4；Complete/Home D@S1；Retire S9；run=4,Count=4 | 1 / D / HF | S1=[∅]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 6 | Move S5→S1 A1；run=1,Count=1 | 1 / D / HF | S1=[A]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A)2 · G]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 7 | Move S4→S5 G1；Hatch A@S4；Complete/Sleep G@S5；run=1,Count=1 | 2 / D / HF | S1=[A]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A3 · 蛋(A) · A]；S5=[G6][睡眠]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 8 | Move S4→S1 A1；Hatch A@S4；run=1,Count=1 | 2 / D / HF | S1=[A2]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[A4]；S5=[G6][睡眠]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 9 | Move S4→S1 A4；Complete/Sleep A@S1；run=4,Count=4 | 3 / D / HF | S1=[A6][睡眠]；S2=[H3 · 蛋(H)2 · F]；S3=[F5]；S4=[∅]；S5=[G6][睡眠]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 10 | Move S2→S3 F1；Hatch H@S2；Complete/Home F@S3；AutoHome A@S1；AutoHome G@S5；run=1,Count=1 | 4 / DFAG / HC | S1=[∅]；S2=[H3 · 蛋(H) · H]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H][Armed] |
| 11 | Move S2→S10 H1；接收临时枝 S10；Hatch H@S2；run=1,Count=1 | 4 / DFAG / HC | S1=[∅]；S2=[H4]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H2][Armed] |
| 12 | Move S2→S10 H3；接收临时枝 S10；run=4,Count=3 | 4 / DFAG / HC | S1=[∅]；S2=[H]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[H5][Armed] |
| 13 | Move S10→S2 H5；Complete/Home H@S2；Retire S10；run=5,Count=5 | 5 / DHFAG / CB | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 14 | Move S6→S1 B1；run=1,Count=1 | 5 / DHFAG / CB | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[C5]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |
| 15 | Move S8→S6 C1；Complete/Home C@S6；run=1,Count=1 | 6 / DHFAGC / BE | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[B5 · E]；S8=[E5]；S9=[∅][Retired]；S10=[∅][Retired] |
| 16 | Move S7→S8 E1；Complete/Home E@S8；run=1,Count=1 | 7 / DHFAGCE / B | S1=[B]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[B5]；S8=[∅]；S9=[∅][Retired]；S10=[∅][Retired] |
| 17 | Move S1→S7 B1；Complete/Home B@S7；run=1,Count=1 | 8 / DHFAGCBE / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅][Retired]；S10=[∅][Retired] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

### 错误退役反例（不通关）（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S4→S10 G1；接收临时枝 S10；Hatch A@S4；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A3 · 蛋(A) · A]；S5=[G5 · A]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Unused]；S10=[G][Armed] |
| 2 | Move S5→S9 A1；接收临时枝 S9；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A3 · 蛋(A) · A]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[A][Armed]；S10=[G][Armed] |
| 3 | Move S4→S9 A1；接收临时枝 S9；Hatch A@S4；run=1,Count=1 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A4]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[A2][Armed]；S10=[G][Armed] |
| 4 | Move S4→S9 A2；接收临时枝 S9；run=4,Count=2 | 0 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A2]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[A4][Armed]；S10=[G][Armed] |
| 5 | Move S9→S4 A4；Complete/Sleep A@S4；Retire S9；run=4,Count=4 | 1 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A6][睡眠]；S5=[G5]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[G][Armed] |
| 6 | Move S10→S5 G1；Complete/Sleep G@S5；Retire S10；run=1,Count=1 | 2 / 无 / DH | S1=[D3 · 蛋(D)2 · H]；S2=[H3 · 蛋(H)2 · F]；S3=[F5 · D]；S4=[A6][睡眠]；S5=[G6][睡眠]；S6=[C5 · B]；S7=[B5 · E]；S8=[E5 · C]；S9=[∅][Retired]；S10=[∅][Retired] |

第6步后A/G睡眠、两临时枝退役，枚举全部有序来源/目标对为0合法移动且未胜利。仅说明此局面无路；Undo最后一步只恢复S10，不能凭空恢复已退S9。连续撤销全部6步或Restart回固定初盘，再走主路可恢复；独立模型回放不证明Unity撤销已实现。

## 关卡40

K8/P10；Q=`A,H,C,E,B,G,D,F`。阈值1：首个未来满组即开箱，不等两群预排；终章无蛋/临时枝，主路长连收。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · H3` | 普通枝 |
| S2 | 6 | `H3 · C3` | 普通枝 |
| S3 | 6 | `C3 · E3` | 普通枝；盖箱 RevealAfter=1 |
| S4 | 6 | `E3 · B3` | 普通枝 |
| S5 | 6 | `B3 · G3` | 普通枝 |
| S6 | 6 | `G3 · D3` | 普通枝 |
| S7 | 6 | `D3 · F3` | 普通枝 |
| S8 | 6 | `F3 · A3` | 普通枝 |
| S9 | 6 | `∅` | 普通枝 |
| S10 | 6 | `∅` | 普通枝 |

### 主路线（9步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S9 H3；run=3,Count=3 | 0 / 无 / AH | S1=[A3]；S2=[H3 · C3]；S3=[C3 · E3][盖箱]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3 · D3]；S7=[D3 · F3]；S8=[F3 · A3]；S9=[H3]；S10=[∅] |
| 2 | Move S8→S1 A3；Complete/Home A@S1；Reveal S3；run=3,Count=3 | 1 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3 · D3]；S7=[D3 · F3]；S8=[F3]；S9=[H3]；S10=[∅] |
| 3 | Move S7→S8 F3；Complete/Sleep F@S8；run=3,Count=3 | 2 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3 · D3]；S7=[D3]；S8=[F6][睡眠]；S9=[H3]；S10=[∅] |
| 4 | Move S6→S7 D3；Complete/Sleep D@S7；run=3,Count=3 | 3 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[H3]；S10=[∅] |
| 5 | Move S5→S6 G3；Complete/Sleep G@S6；run=3,Count=3 | 4 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[H3]；S10=[∅] |
| 6 | Move S4→S5 B3；Complete/Sleep B@S5；run=3,Count=3 | 5 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3]；S5=[B6][睡眠]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[H3]；S10=[∅] |
| 7 | Move S3→S4 E3；Complete/Sleep E@S4；run=3,Count=3 | 6 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3]；S4=[E6][睡眠]；S5=[B6][睡眠]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[H3]；S10=[∅] |
| 8 | Move S2→S3 C3；Complete/Home C@S3；AutoHome E@S4；AutoHome B@S5；AutoHome G@S6；AutoHome D@S7；AutoHome F@S8；run=3,Count=3 | 7 / ACEBGDF / H | S1=[∅]；S2=[H3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[H3]；S10=[∅] |
| 9 | Move S9→S2 H3；Complete/Home H@S2；run=3,Count=3 | 8 / AHCEBGDF / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅]；S10=[∅] |


### 对照路线1（10步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S7→S9 F3；run=3,Count=3 | 0 / 无 / AH | S1=[A3 · H3]；S2=[H3 · C3]；S3=[C3 · E3][盖箱]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3 · D3]；S7=[D3]；S8=[F3 · A3]；S9=[F3]；S10=[∅] |
| 2 | Move S6→S7 D3；Complete/Sleep D@S7；Reveal S3；run=3,Count=3 | 1 / 无 / AH | S1=[A3 · H3]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3 · G3]；S6=[G3]；S7=[D6][睡眠]；S8=[F3 · A3]；S9=[F3]；S10=[∅] |
| 3 | Move S5→S6 G3；Complete/Sleep G@S6；run=3,Count=3 | 2 / 无 / AH | S1=[A3 · H3]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F3 · A3]；S9=[F3]；S10=[∅] |
| 4 | Move S1→S10 H3；run=3,Count=3 | 2 / 无 / AH | S1=[A3]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F3 · A3]；S9=[F3]；S10=[H3] |
| 5 | Move S8→S1 A3；Complete/Home A@S1；run=3,Count=3 | 3 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F3]；S9=[F3]；S10=[H3] |
| 6 | Move S9→S8 F3；Complete/Sleep F@S8；run=3,Count=3 | 4 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3 · B3]；S5=[B3]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[∅]；S10=[H3] |
| 7 | Move S4→S5 B3；Complete/Sleep B@S5；run=3,Count=3 | 5 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3 · E3]；S4=[E3]；S5=[B6][睡眠]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[∅]；S10=[H3] |
| 8 | Move S3→S4 E3；Complete/Sleep E@S4；run=3,Count=3 | 6 / A / HC | S1=[∅]；S2=[H3 · C3]；S3=[C3]；S4=[E6][睡眠]；S5=[B6][睡眠]；S6=[G6][睡眠]；S7=[D6][睡眠]；S8=[F6][睡眠]；S9=[∅]；S10=[H3] |
| 9 | Move S2→S3 C3；Complete/Home C@S3；AutoHome E@S4；AutoHome B@S5；AutoHome G@S6；AutoHome D@S7；AutoHome F@S8；run=3,Count=3 | 7 / ACEBGDF / H | S1=[∅]；S2=[H3]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅]；S10=[H3] |
| 10 | Move S10→S2 H3；Complete/Home H@S2；run=3,Count=3 | 8 / AHCEBGDF / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；S6=[∅]；S7=[∅]；S8=[∅]；S9=[∅]；S10=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。
