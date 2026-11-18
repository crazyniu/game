# Perch Sort store artwork

The English and Simplified Chinese campaign shows five parts of the actual game: sorting a flock, sending it home, exploring ten forests, dressing birds, and opening star chests. Use the posters in that order. The graphics are illustrations, not unedited gameplay screenshots.

| Platform | Files per language | Size |
| --- | --- | --- |
| App Store, 6.9-inch | Five portrait JPEGs | 1320 × 2868 |
| App Store, 6.5-inch | Five portrait JPEGs | 1284 × 2778 |
| Google Play | Five portrait JPEGs and one feature graphic | 1080 × 1920 and 1024 × 500 |
| WeChat, Simplified Chinese | Five portrait JPEGs | 1080 × 1920 |
| Icons | App Store / Google Play / WeChat | 1024 / 512 / 144 px square |

The campaign uses this project's Sunny and Sky portraits, painted forest and teal-roof birdhouse as art references. Generation briefs are in `briefs/`. The images are localized independently before resizing. The twelve original PNG masters are in `masters/`. To re-export, run `./run.sh native-icons` followed by `node store/export.mjs`. Platform folders under `images/` contain the deliverables.
