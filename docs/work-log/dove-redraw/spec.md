# T10e — 重画 bird_05 扇尾鸽（正面四态）

负责：Codex（gpt-6-astra / high）。验收：Claude。

## 背景
T10d 已交付 10 种正面鸟（art/raw/birds_front/，报告 tasks/T10d_report.md）。其余 9 种已通过。bird_05 扇尾鸽原图尾扇过宽，等比缩小到总宽 62% 后身体只有其它鸟约 60% 大，在 contact_front_row.png 里明显偏小，不合格。只靠缩放无法同时满足“体型一致”和“不过宽”，因此重画。

## 要求（照做，不得自行发挥）
- 只改 bird_05 的 4 张：bird_05.png、bird_05_blink.png、bird_05_sleep.png、bird_05_fly.png。其它鸟、PerchSort/ 工程、已有素材一律不动；不提交 git。
- 原画用 image_gen 生成，风格与已通过的 bird_01 / bird_04 一致：正面、粗深棕描边、柔和厚涂、同一光源（左上）、同画布 1024×1024 透明底、脚底线 y=870。
- 物种特征保留：白/奶油色身体，翅膀上灰色心形斑，灰色小喙，橙色脚，背后竖起的白色扇形尾。
- **尺寸硬指标**（站立态）：
  - 身体+头（不含尾扇）高度与 bird_01 去尾含冠高度相差 ≤ 5%。
  - 尾扇改为“小而竖”的扇形：尾扇从身体两侧各外露不超过身体宽度的 25%；含尾扇总宽 ≤ 画布 55%。
  - 身体宽度（不含尾扇）在画布 40–50%。
- blink：只有眼区像素与站立态不同。sleep：趴下闭眼，脚底线 y=870。fly：展翅正面，翼展 ≤ 980px，中心与站立态对齐。
- 生成后更新 manifest_front.json 中 bird_05 条目（去掉之前的“缩小例外”备注），并重出 contact_front.png、contact_front_row.png、contact_front_gray64.png。

## 交付
- 报告 tasks/T10e_report.md：量得的身体高度、身体宽度、总宽三项数值，对比 bird_01。
- 有歧义用 orca orchestration ask 问协调者。完成后 worker_done。
