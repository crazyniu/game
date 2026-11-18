# Packaging results

- WeChat Mini Game: Creator exported a local Developer Tools project. The packaged main files total 2,131,344 bytes. The 476,666,493 bytes of remote resources are excluded from the local main-package measurement. A production configuration requires the owner's AppID and HTTPS asset host. No upload or on-device acceptance was performed.
- iOS: Creator exported PerchSort.xcodeproj and Xcode built an arm64 simulator application containing a Mach-O executable. The launcher fixes the generated simulator target architecture and rearchives the appropriate Creator WebP objects for the Apple linker. No installed simulator runtime, signed IPA or device acceptance was used.
- Android: APK/AAB and project export commands are wired. Creator stopped at the Android plugin's missing SDK/NDK path check. No Android project, APK or AAB was produced on this machine.
- Local simulator builds, native configuration, package reports and WeChat remote assets are excluded from the PR. No signing credentials or production app IDs from fishsort were copied.
