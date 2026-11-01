> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# Current Godot export/release boundary

Migration completion and store release are different outcomes. The current primary gate is a native cloud first-level build/test. Export real Android/iOS platform artifacts using Godot export presets and matching export templates when prerequisites are available; record missing templates/SDK/toolchains/signing identities explicitly. A project folder, screenshot, Linux binary or HTML page is not an Android/iOS package. iOS tooling/signing may need a supported host; local fallback is used only after the cloud limitation is established and within existing permissions.

No store upload/deployment is authorized by the migration merge request. Do not substitute placeholder company/package identities for actual approved release identity. Keep export credentials out of source; export presets and per-user credentials have separate handling. The Unity release settings and store checklist below are historical reference and must not be executed as current Godot steps. Current export status is maintained in GODOT_MIGRATION; do not mark unchecked historical tasks complete.

## Retained legacy/reference details (not current engine workflow)

# Perch Sort — pre-release checklist

Checked 2026-10-31; Unity 6000.6.7f1. Everything below remains pre-store work, without implying accounts, signing, uploads, or release already exist. The current game runs offline, without accounts, ads, in-app purchases, analytics, or data collection; progress/settings stay on the device.

## Identity, accounts, and signing

- [ ] Register and verify Google Play Console / Apple Developer Program entities, contacts, and two-factor authentication; confirm team roles, regions, agreements, tax, EU trader status, and other applicable requirements. Follow console requirements for new personal accounts' testing/production-access gates.
- [ ] **Company name `TODO_COMPANY` and Android applicationId / iOS bundle id `com.todo.perchsort` are all placeholders; replace before submission, and they cannot change after release.** Once the real entity and unique reverse-domain identifier are decided, update both `Assets/Editor/ReleaseSettings.cs` and store records. Do not upload placeholder identities.
- [ ] Current `productName=Perch Sort`, version `0.1.0`, Android versionCode `1`, iOS buildNumber `1`; increment build numbers for every upload. Company/product names affect Unity's local save directory; verify upgrade saves after production identity freezes.
- [ ] Create Android upload keys, enable Play App Signing, and record certificate fingerprints/recovery ownership. Store keys/alias passwords in a password vault/CI secret with encrypted offline backup, never repository/logs.
- [ ] Create iOS App ID, signing certificates, and provisioning profile under the correct Team; verify capabilities match actual use. Keep certificate private keys, recovery material, and App Store Connect API keys in a restricted vault.

## Toolchain and build configuration

- [ ] Install Android Build Support (SDK/NDK Tools and OpenJDK) and iOS Build Support for **6000.6.7f1** through Unity Hub. Local T7 verification had only MacStandaloneSupport / WebGLSupport, without Android/iOS builds.
- [ ] Android minimum API **26 / Android 8.0**, target API **36 / Android 16**, ARM 64, IL2CPP; Unity 6000.6 official toolchain uses SDK 36, NDK r27c, OpenJDK 17. From 2026-08-31 Google Play requires target API36 for ordinary new apps/updates. Recheck console/official requirements at release. [Unity system requirements](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements), [Google Play target API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en)
- [ ] iOS minimum deployment **15.0**; build submissions with **Xcode 26+ / iOS 26 SDK+**. Minimum OS is not build SDK version. Local modules are absent; actual archiving/signing of this combination still needs rechecking. [Apple submission requirements](https://developer.apple.com/news/upcoming-requirements/)
- [ ] In Unity run **Perch Sort → Apply Release Settings** (CLI `bash scripts/apply_release_settings.sh`), idempotently configuring portrait orientation, identity, version, icons, permissions, and telemetry. Scene rebuilding in `run_tests.sh` also applies this development-placeholder configuration; update script identities/versions before production release.
- [ ] After installing Android/iOS modules, rerun configuration and confirm every platform icon slot is filled. Default icon is a byte-identical copy of original `art/raw/ui/icon_1024.png`; Android adaptive uses the complete flat illustration as background and a transparent foreground, without a fake cutout. Check crest/tail/feet cropping under circular, rounded-rectangle, and manufacturer masks. Any adjustment needs a derived asset, without overwriting originals.
- [ ] Confirm Portrait lock and disable landscape/upside-down rotation. iPad currently has requiresFullScreen=true; recheck system window behavior/safe areas on device. Unity default Logo / Splash is disabled (also allowed in Unity 6 Personal); verify no prolonged blank cold-start period before the first frame. [Unity official statement](https://unity.com/blog/unity-is-canceling-the-runtime-fee)
- [ ] Generate Release AAB / Xcode Archive; check ARM 64,16 KB page-size compatibility, and final merged AndroidManifest. No dangerous camera/microphone/location/storage permissions; `VIBRATE` is a normal permission used only for Collect / Win. No INTERNET, AD_ID, advertising/payment/analytics SDKs should exist; Editor acceptance does not prove final package permissions.
- [ ] Review final iOS Info.plist, entitlements, privacy manifests, and required-reason API declarations (including Unity-generated content), declaring only actual use. Disable Development Build, Script Debugging, and network diagnostics.

## Privacy, ratings, and review notes

- [ ] Publish a publicly accessible privacy-policy page with text, effective date, real entity, and contact email. Explain no data collection/sharing/tracking, local progress/settings, reset/uninstall effects, and system-backup boundaries if applicable. No accounts means no fabricated account-deletion flow.
- [ ] Complete Google Play Data safety from the final build: no collection/sharing, ads, purchases, or login. Even without collection, submit the form and privacy policy; re-review future engine/SDK changes. [Google Data safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en)
- [ ] Complete App Store Connect App Privacy accurately as “No, we do not collect data from this app,” with no tracking/ATT request. Verify final package/dependencies match; product intent alone is insufficient. [Apple App Privacy](https://developer.apple.com/help/app-store-connect/manage-app-information/manage-app-privacy/)
- [ ] Google IARC / Apple's new age questionnaire: accurately describe cartoon-bird puzzles, no violence/gore, sexual content, gambling/simulated gambling, drugs, horror, chat/UGC, unrestricted web access, or paid random items. Do not guarantee the rating outcome yourself.
- [ ] Decide target ages and whether actively targeting children. Children's/family categories require separate policy and asset/external-link checks; cute art is not automatic grounds to select them. Review notes explain offline play,40 levels, Undo/restart, no accounts, and no review credentials needed.

## Store assets

| Platform/asset | Planned dimensions and requirements | To do |
| --- | --- | --- |
| Google Play icon | 512×512 PNG,≤1024 KB; distinct from device launcher icon | [ ] Export separately from 1024 source and check small-size recognition |
| Google Play feature graphic | 1024×500, JPEG or24-bit PNG, no alpha | [ ] Create promotional image showing real gameplay |
| Google Play phone screenshots | At least 2, suggested 4–8 at1080×1920, PNG/JPEG; each side 320–3840, long side at most twice short side | [ ] Retake main menu, board, collection, victory, settings in real Release phone runtime |
| Google Play tablet screenshots | Actual 7/10-inch layouts if supporting tablets | [ ] Do not stretch phone views |
| Apple App Icon | 1024×1024, no transparency or prebaked rounded corners | [ ] Generate asset catalog through module and verify Archive |
| Apple large-screen iPhone screenshots | Portrait 1260×2736 /1290×2796 /1320×2868 as current Connect slots permit;1–10 per set | [ ] Capture actual matching device sizes |
| Apple 13-inch iPad screenshots | Portrait 2064×2752 or2048×2732; required with iPad support | [ ] Capture after device/simulator layout verification |
| Both platforms' text/links | Title, short description/subtitle, full description, keywords/category, copyright, support email/URL, privacy URL, release notes; current limits per field | [ ] Review English and planned release languages individually |
| Optional preview video | Follow current store video specifications separately; outside this delivery | [ ] Do not include nonexistent features |

Dimension sources: [Google Play assets](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en-GB&rd=1), [Apple screenshot specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/). `build/screens/*.png` are 1080×1920 Editor Play Mode acceptance images, **not verified store screenshots for all devices**; image 11 explicitly shows Reduce motion enabled.

## Device and release gates

| Test group | Minimum coverage | Pass criteria |
| --- | --- | --- |
| Minimum Android | API26, ARM 64, low-memory device | Cold start/continuous play/saves work; no dangerous-permission dialogs |
| Current Android | API35/36, at least two manufacturers, including 16 KB page device | Correct edge-to-edge/notch/gesture back, icon masks, rotation lock |
| iPhone | Available minimum iOS 15 device + current iOS notch/Dynamic Island device | Correct launch, background/foreground/call interruption, mute switch, settings persistence after restart, touch |
| iPad / Android tablet | 4:3 and widescreen, safe areas/system windows | Portrait; unclipped board/text; reachable buttons |
| Audio/haptics | Phone speakers + headphones; with/without vibration hardware | 7 sound events match; mute stops immediately; no overlapping managers on menu/scene changes; no error without BGM; vibrate only Collect/Win, never Error |
| Motion/input | Default/Reduce motion, rapid repeated Next/menu-back taps | Designed fade-out/in total 0.20 s, no lingering overlay/input lock; reduced motion has no shake/particles and permits completion/Undo |
| Saves/offline | Airplane mode, process kill/restart, previous-version upgrade, reset cancel/confirm | No network dependency; progress/toggles saved; reset preserves settings; upgrade retains saves under production identity directory |
| Performance/stability | Continuous 20 minutes, levels 1/20/40, repeated background/foreground/victory | No exceptions, persistent object/audio-source growth, noticeable stutter, or audio fatigue |

- [ ] Run `bash scripts/run_tests.sh` fully green and archive XML/logs.
- [ ] Run `bash scripts/screenshots.sh`; individually accept 11 actual Play Mode images. Human listening/device haptics require separate sign-off.
- [ ] Complete the matrix with signed device packages; check offline launch/runtime network behavior, permissions, signing, identity, version. Source configuration/Editor tests do not replace package acceptance.
- [ ] Account owner confirms all placeholders removed, assets, policy forms, and signing; then enter internal testing / TestFlight and gradually roll out under actual console requirements. This task performed no upload/release.
