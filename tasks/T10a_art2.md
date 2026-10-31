# T10a — v2 新增美术（只改 art/，不碰 PerchSort/）

风格必须与现有 `art/raw/birds`、`art/raw/env` 一致（柔和明亮卡通、暖棕外轮廓、左上光源、右下阴影）。**全部原创**，用内置 image_gen；每张独立生成、真实透明背景（RGBA）。先读 `art/ASSETS.md`、`art/manifest.json`。

## 需要的资产（路径 art/raw/…）
1. `env/crate.png` 1024×320：木板条箱，盖住一整根栖木（含上方鸟的位置），正面带透气缝与一把小挂锁/绳结；中间有可叠加文字区不要画字。
2. `env/basket.png` 512×384：编织小篮子（单只鸟的临时位），口沿朝上，内部留出鸟站位。
3. `env/nest.png` 512×512：顶部目标鸟巢（树枝编织的巢，浅色空巢，中央有圆形留白处用来叠放对应鸟的图标）；另 `env/nest_done.png`：同一个巢里有 3 枚柔和小蛋/暖光（归巢完成态）。
4. `fx/zzz.png` 256×256：卡通 “Z” 气泡（睡眠）。`fx/sparkle.png` 256：四角星闪光。`fx/check.png` 256：绿色对勾徽章。`fx/feather.png` 256：一片羽毛（归巢飘落用）。`fx/ring.png` 256：柔和光环。`fx/confetti_*.png`（3 张小纸片，不同形状，256）。
5. `ui/trophy.png` 1024：金奖杯（结算页）；`ui/star.png` 256：金星。`ui/btn_*`不需要（UI 用代码画）。
6. 睡眠鸟：不单独画 10 张。改为 `fx/closed_eye.png` 128：一条弯弧闭眼线（代码按鸟定位叠加），睡眠时配合压扁+变暗。如你判断叠加效果会明显穿帮，可改为为每种鸟生成睡眠姿态（bird_01s..bird_10s），并在报告说明。
7. 更新 `art/manifest.json` 与 `art/ASSETS.md`（尺寸、锚点、用途）；再出一张 `art/contact_v2.png` 合成预览（深浅底各一）。
8. `docs/LICENSES.md` 追加这些资产的来源说明。

## 验收（Claude）
全部文件存在、尺寸/RGBA/透明正确（我用脚本查）；我看预览图：风格一致、64px 缩小仍可辨、无白边/棋盘格伪影、无文字。
报告 `tasks/T10a_report.md`。不提交 git。
