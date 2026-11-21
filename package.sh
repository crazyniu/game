#!/usr/bin/env bash
# Package the game: choose a platform, then an artifact. Runs on the Mac with Cocos Creator 3.8.8.
# Identity, versions and ad units come from game.json (tools/native/apply.mjs).
set -euo pipefail
cd "$(dirname "$0")"
ROOT="$PWD"
source tools/cocos/creator.sh
if [ "$#" -gt 0 ]; then die "请运行 ./package.sh，然后输入菜单编号。"; fi
mkdir -p build

# Google Mobile Ads + UMP for iOS: downloaded into PerchSort/native/engine/ios/sdk/ (not in Git) and linked by the iOS
# CMakeLists. Not downloaded when game.json turns ads off. A store package (ipa) stops without the SDK; a test build
# compiles without ads.
ios_sdk() {
  local mode="${1:-}" dir="$PROJECT/native/engine/ios/sdk" tmp fw
  if [ "$(node -p "require('./game.json').ads.enabled === true")" != true ]; then
    echo "· game.json 关了广告，这次不带广告 SDK"
    return 0
  fi
  if [ ! -d "$dir/GoogleMobileAds.xcframework" ] || [ ! -d "$dir/UserMessagingPlatform.xcframework" ]; then
    echo "· 下载 Google Mobile Ads iOS SDK …"
    tmp="$(mktemp -d)"
    if curl -fsSL -o "$tmp/sdk.zip" https://dl.google.com/googleadmobadssdk/googlemobileadssdkios.zip && (cd "$tmp" && unzip -q sdk.zip); then
      mkdir -p "$dir"
      find "$tmp" -maxdepth 3 -name '*.xcframework' -type d -exec cp -R {} "$dir/" \;
    fi
    rm -rf "$tmp"
  fi
  for fw in GoogleMobileAds UserMessagingPlatform; do
    [ -d "$dir/$fw.xcframework" ] && continue
    [ "$mode" = ipa ] && die "广告 SDK 没下载下来（缺 ${fw}.xcframework）：上架包不能没有广告，检查网络后重试，或手动解压到 ${dir}/"
    echo "! 缺 ${fw}.xcframework（SDK 下载失败），这次按没有广告编译（测试包）"
    return 0
  done
  ls "$dir"
}

# Cocos 3.8.8's prebuilt libwebp.a fails Xcode 27's linker (unaligned members); repack an aligned copy with libtool for the iOS CMakeLists.
ios_fix_webp() {
  local src dst="$PROJECT/native/engine/ios/sdk/libwebp/libwebp.a"
  src="$(dirname "$(dirname "$COCOS")")/Resources/resources/3d/engine/native/external/ios/libs/libwebp.a"
  [ -f "$dst" ] && [ "$dst" -nt "$src" ] && return 0
  [ -f "$src" ] || { echo "! 没找到 ${src}，跳过"; return 0; }
  mkdir -p "$(dirname "$dst")"
  xcrun libtool -static -o "$dst" "$src" 2>/dev/null || { echo "! libwebp 重新打包失败"; rm -f "$dst"; }
}

# The Xcode project; Creator names the scheme <name>-mobile.
ios_project() {
  local proj
  proj="$(ls -d "$PROJECT"/build/ios/proj/*.xcodeproj 2>/dev/null | head -1)"
  [ -n "$proj" ] || die "没找到 Xcode 工程（PerchSort/build/ios/proj/*.xcodeproj），请先在打包菜单中导出 Xcode 工程"
  echo "$proj"
}

ios_build() {
  local mode="${1:-}" proj scheme team
  # Only the IPA (App Store) uses real ad units; every other artifact uses Google's test units (tools/native/apply.mjs).
  GAME_RELEASE="$([ "$mode" = ipa ] && echo 1 || echo 0)" node tools/native/apply.mjs
  ios_sdk "$mode"
  ios_fix_webp
  # Creator rewrites the launch images while exporting its templates; keep the tracked ones.
  (
    ios_dir="$PROJECT/native/engine/ios"
    saved_launch="$(mktemp -d)"
    for image in "$ios_dir"/LaunchScreenBackground*.png; do
      [ ! -f "$image" ] || cp -p "$image" "$saved_launch/"
    done
    restore_launch() {
      for image in "$saved_launch"/*.png; do
        [ ! -f "$image" ] || cp -p "$image" "$ios_dir/"
      done
      rm -rf "$saved_launch"
    }
    trap restore_launch EXIT
    cocos_build "$PROJECT/build-config/ios.json" build/cocos-ios.log
  )
  proj="$(ios_project)"
  scheme="$(basename "$proj" .xcodeproj)-mobile"
  team="$(node -e 'console.log(require("./game.json").ios.team || "")')"
  case "$mode" in
    "") echo "✓ Xcode 工程：${proj}（用 Xcode 打开可以直接装到手机上调试）" ;;
    sim)
      # arm64 only (Apple silicon): the x86_64 simulator slices of Creator's prebuilt libraries fail Xcode 27's linker.
      xcodebuild -project "$proj" -scheme "$scheme" -sdk iphonesimulator -configuration Debug ARCHS=arm64 ONLY_ACTIVE_ARCH=YES \
        -derivedDataPath "$PROJECT/build/ios/derived" CODE_SIGNING_ALLOWED=NO build > "$PROJECT/build/ios/sim.log" 2>&1 \
        || { grep -E "error:|BUILD FAILED" "$PROJECT/build/ios/sim.log" | head -30; die "模拟器版编译失败（全文 PerchSort/build/ios/sim.log）"; }
      echo "✓ 模拟器版编译通过：PerchSort/build/ios/derived/Build/Products/Debug-iphonesimulator/"
      ;;
    dev)
      xcodebuild -project "$proj" -scheme "$scheme" -sdk iphoneos -configuration Release \
        -derivedDataPath "$PROJECT/build/ios/derived-dev" CODE_SIGNING_ALLOWED=NO build > "$PROJECT/build/ios/dev.log" 2>&1 \
        || { grep -E "error:|BUILD FAILED" "$PROJECT/build/ios/dev.log" | sort -u | head -30; die "真机版编译失败（全文 PerchSort/build/ios/dev.log）"; }
      echo "✓ 真机版（Release、不签名）编译通过"
      ;;
    phone)
      # Sign, install on an iPhone paired with this Mac (USB or the same network) and launch it; test ad units.
      [ -n "$team" ] || die "game.json 的 ios.team 还没填（Apple 开发者账号的 Team ID）"
      local log="$PROJECT/build/ios/phone.log" app udid
      xcodebuild -project "$proj" -scheme "$scheme" -sdk iphoneos -configuration Release \
        -derivedDataPath "$PROJECT/build/ios/derived-phone" DEVELOPMENT_TEAM="$team" CODE_SIGN_STYLE=Automatic \
        -allowProvisioningUpdates build > "$log" 2>&1 \
        || { grep -E "error:|BUILD FAILED" "$log" | sort -u | head -30; die "签名版编译失败（全文 PerchSort/build/ios/phone.log）"; }
      # Creator's project writes the app to proj/Release-iphoneos/ rather than derived data; take the newest of both.
      app="$(ls -td "$PROJECT"/build/ios/proj/Release-iphoneos/*.app "$PROJECT"/build/ios/derived-phone/Build/Products/Release-iphoneos/*.app 2>/dev/null | head -1 || true)"
      [ -n "$app" ] || die "编译通过了，但没找到打好的 App（PerchSort/build/ios/proj/Release-iphoneos/）"
      codesign -dv "$app" 2>&1 | grep "TeamIdentifier=$team" >/dev/null || die "${app} 没有用 ${team} 签名（全文 PerchSort/build/ios/phone.log）"
      # The first paired iPhone that is reachable now.
      xcrun devicectl list devices --json-output "$PROJECT/build/ios/devices.json" >/dev/null 2>&1 || true
      udid="$(node -e '
        const d = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8")).result.devices;
        const ok = d.filter((x) => x.hardwareProperties?.deviceType === "iPhone" && x.connectionProperties?.pairingState === "paired" && x.connectionProperties?.tunnelState !== "unavailable");
        console.log(ok[0]?.hardwareProperties?.udid ?? "");
      ' "$PROJECT/build/ios/devices.json" 2>/dev/null)"
      [ -n "$udid" ] || die "没找到连着的 iPhone（手机解锁、和这台 Mac 在同一网络或插上线；第一次要在 Xcode → Devices 里配对，手机上打开「开发者模式」）。App 在 ${app}"
      xcrun devicectl device install app --device "$udid" "$app" >> "$log" 2>&1 || { tail -15 "$log"; die "装到手机失败"; }
      xcrun devicectl device process launch --device "$udid" "$(node -p 'require("./game.json").ios.bundleId')" >> "$log" 2>&1 \
        && echo "✓ 已装到手机并打开（测试广告位）" || echo "✓ 已装到手机（没能自动打开：手机锁着的话解锁后点图标）"
      ;;
    ipa)
      [ -n "$team" ] || die "game.json 的 ios.team 还没填（Apple 开发者账号的 Team ID）"
      local out="$PROJECT/build/ios/release"
      rm -rf "$out"; mkdir -p "$out"
      xcodebuild -project "$proj" -scheme "$scheme" -sdk iphoneos -configuration Release \
        -archivePath "$out/app.xcarchive" DEVELOPMENT_TEAM="$team" -allowProvisioningUpdates archive > "$out/archive.log" 2>&1 \
        || { grep -E "error:|ARCHIVE FAILED" "$out/archive.log" | head -30; die "归档失败（全文 PerchSort/build/ios/release/archive.log）"; }
      cat > "$out/ExportOptions.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>method</key><string>app-store-connect</string>
  <key>teamID</key><string>${team}</string>
  <key>signingStyle</key><string>automatic</string>
</dict></plist>
PLIST
      xcodebuild -exportArchive -archivePath "$out/app.xcarchive" -exportPath "$out" \
        -exportOptionsPlist "$out/ExportOptions.plist" -allowProvisioningUpdates > "$out/export.log" 2>&1 \
        || { tail -20 "$out/export.log"; die "导出 ipa 失败"; }
      echo "✓ ipa：$(ls "$out"/*.ipa)（菜单「6 上传 App Store Connect」直接传，或用 Transporter 上传）"
      ;;
    *) die "未知的 iOS 产物" ;;
  esac
}

# Upload the last IPA archive to App Store Connect with the account signed in to Xcode (Settings > Accounts).
# The build appears in App Store Connect 15-30 minutes later.
ios_upload() {
  local out="$PROJECT/build/ios/release" team
  [ -d "$out/app.xcarchive" ] || die "没有打好的归档（PerchSort/build/ios/release/app.xcarchive），先选「4 IPA」"
  team="$(node -e 'console.log(require("./game.json").ios.team || "")')"
  [ -n "$team" ] || die "game.json 的 ios.team 还没填"
  cat > "$out/UploadOptions.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>method</key><string>app-store-connect</string>
  <key>destination</key><string>upload</string>
  <key>teamID</key><string>${team}</string>
  <key>signingStyle</key><string>automatic</string>
</dict></plist>
PLIST
  echo "· 上传到 App Store Connect（构建号 $(/usr/libexec/PlistBuddy -c 'Print ApplicationProperties:CFBundleVersion' "$out/app.xcarchive/Info.plist")）…"
  xcodebuild -exportArchive -archivePath "$out/app.xcarchive" -exportPath "$out/upload" \
    -exportOptionsPlist "$out/UploadOptions.plist" -allowProvisioningUpdates > "$out/upload.log" 2>&1 \
    || { grep -iE "error|fail" "$out/upload.log" | sort -u | head -15; die "上传失败（全文 PerchSort/build/ios/release/upload.log）"; }
  echo "✓ 已上传到 App Store Connect，处理完（15–30 分钟）在 App 的「构建版本」里能选到"
}

# Google Play upload key: signing settings live in ~/.gradle/gradle.properties (not in Git). Created here on the first AAB:
# the user types the password, the key file stays outside the repository, and must be backed up (Google resets lost keys).
GRADLE_PROPS="$HOME/.gradle/gradle.properties"
find_keytool() {
  local k
  for k in "$(command -v keytool 2>/dev/null)" "${JAVA_HOME:-}/bin/keytool" \
           "/Applications/Android Studio.app/Contents/jbr/Contents/Home/bin/keytool" \
           "$(/usr/libexec/java_home 2>/dev/null)/bin/keytool"; do
    if [ -n "$k" ] && [ -x "$k" ] && "$k" -help >/dev/null 2>&1; then echo "$k"; return; fi
  done
}
android_upload_key() {
  local store pw pw2 kt
  store="$(grep -E '^RELEASE_STORE_FILE=' "$GRADLE_PROPS" 2>/dev/null | tail -1 | cut -d= -f2-)"
  if [ -n "$store" ]; then
    [ -f "$store" ] || die "~/.gradle/gradle.properties 里的上传密钥 ${store} 不存在（换了电脑？从备份拷回来）"
    echo "· 上传密钥：${store}"
    return 0
  fi
  kt="$(find_keytool)"
  [ -n "$kt" ] || die "没找到 keytool（装 Android Studio 或 JDK）"
  echo "还没有 Google Play 的上传密钥，现在生成一个（只做一次）。"
  echo "密码只有你知道；密钥文件和密码都要备份好，丢了就不能更新游戏，要找 Google 重置。"
  printf '密钥文件放哪（回车 = %s）：' "$HOME/keys/perchsort-upload.jks"
  IFS= read -r store || exit 1
  store="${store:-$HOME/keys/perchsort-upload.jks}"
  [ ! -e "$store" ] || die "${store} 已经存在：要用它就手动把四个 RELEASE_ 变量写进 ~/.gradle/gradle.properties（docs/10 第 4 节）"
  while :; do
    printf '设一个密码（至少 6 位，输入时不显示）：'; IFS= read -rs pw || exit 1; echo
    printf '再输一遍：'; IFS= read -rs pw2 || exit 1; echo
    [ "${#pw}" -ge 6 ] && [ "$pw" = "$pw2" ] && break
    echo "两次不一样或不到 6 位，重来。"
  done
  mkdir -p "$(dirname "$store")"
  "$kt" -genkeypair -keystore "$store" -alias upload -keyalg RSA -keysize 2048 -validity 10000 \
    -storepass "$pw" -keypass "$pw" -dname "CN=$(node -p 'require("./game.json").name.en || "Game"' 2>/dev/null)" >/dev/null 2>&1 \
    || die "生成上传密钥失败"
  mkdir -p "$(dirname "$GRADLE_PROPS")"
  {
    echo ""
    echo "# Google Play 上传密钥（./package.sh 生成于 $(date +%F)）"
    echo "RELEASE_STORE_FILE=$store"
    echo "RELEASE_STORE_PASSWORD=$pw"
    echo "RELEASE_KEY_ALIAS=upload"
    echo "RELEASE_KEY_PASSWORD=$pw"
  } >> "$GRADLE_PROPS"
  chmod 600 "$GRADLE_PROPS" "$store"
  echo "✓ 上传密钥：${store}（签名配置写进了 ~/.gradle/gradle.properties）——现在就把这个文件和密码备份到别处"
}

# 16 KB memory pages (Google Play, targetSdk 35+): every .so LOAD segment in the AAB must be 16 KB (0x4000) aligned.
# Anything that cannot be checked (no readelf, unreadable archive, no arm64 library) fails too. Play Console checks again.
android_check_16k() {
  local aab="$1" ndk="$2" tmp readelf so hdr aligns bad=0 n=0
  readelf="$(ls -d "$ndk"/toolchains/llvm/prebuilt/*/bin/llvm-readelf 2>/dev/null | head -1)"
  [ -x "$readelf" ] || { echo "✗ 没找到 llvm-readelf（${ndk}），查不了 16 KB 页"; return 1; }
  tmp="$(mktemp -d)"
  if ! unzip -q -o "$aab" 'base/lib/*' -d "$tmp"; then
    rm -rf "$tmp"; echo "✗ 解不开 ${aab} 里的原生库，查不了 16 KB 页"; return 1
  fi
  for so in "$tmp"/base/lib/arm64-*/*.so; do
    [ -f "$so" ] || continue
    n=$((n + 1))
    # Capture readelf first: piping a failed read into grep would look aligned.
    if ! hdr="$("$readelf" -lW "$so" 2>&1)"; then
      echo "✗ $(basename "$so")：readelf 读不了（${hdr%%$'\n'*}）"; bad=1; continue
    fi
    aligns="$(printf '%s\n' "$hdr" | awk '$1=="LOAD"{print $NF}')"
    if [ -z "$aligns" ]; then
      echo "✗ $(basename "$so")：没有 LOAD 段，查不了对齐"; bad=1
    elif printf '%s\n' "$aligns" | grep -qvE '^0x(4000|8000|10000)$'; then
      echo "✗ $(basename "$so") 没有按 16 KB 对齐（$(printf '%s' "$aligns" | sort -u | tr '\n' ' ')）"; bad=1
    fi
  done
  rm -rf "$tmp"
  [ "$n" -gt 0 ] || { echo "✗ AAB 里没有 arm64 的原生库，查不了 16 KB 页"; return 1; }
  [ "$bad" = 0 ] || { echo "✗ 16 KB 内存页检查没过（上面几个原生库），Google Play 会拒（docs/06）"; return 1; }
  echo "✓ 16 KB 内存页：arm64 的 ${n} 个原生库都按 16 KB 对齐"
}

android_build() {
  local mode="${1:-apk}" proj
  # Only the AAB (Google Play) uses real ad units; every other artifact uses Google's test units (tools/native/apply.mjs).
  [ "$mode" != aab ] || android_upload_key
  GAME_RELEASE="$([ "$mode" = aab ] && echo 1 || echo 0)" node tools/native/apply.mjs
  local sdk ndk cfg cand
  # SDK: ANDROID_HOME, Android Studio's default location, or an external disk /Volumes/*/Android/sdk.
  sdk="${ANDROID_HOME:-}"
  if [ -z "$sdk" ]; then
    for cand in "$HOME/Library/Android/sdk" /Volumes/*/Android/sdk; do
      if [ -d "$cand/platforms" ] && [ -d "$cand/ndk" ]; then sdk="$cand"; break; fi
    done
  fi
  [ -n "$sdk" ] && [ -d "$sdk/platforms" ] || die "没找到 Android SDK（装 Android Studio，或设 ANDROID_HOME；也会找外接盘上的 /Volumes/*/Android/sdk）"
  # NDK: ANDROID_NDK_HOME or the newest version under the SDK's ndk/.
  ndk="${ANDROID_NDK_HOME:-$(ls -d "$sdk"/ndk/*/ 2>/dev/null | sort | tail -1)}"
  ndk="${ndk%/}"
  [ -d "$ndk" ] || die "没找到 Android NDK（Android Studio → SDK Manager → SDK Tools 装 NDK，或设 ANDROID_NDK_HOME）"
  echo "· Android SDK：$sdk"
  echo "· Android NDK：$ndk"
  # Pass the SDK and NDK paths in this build's configuration (sdkPath, ndkPath) instead of Creator's preferences.
  cfg="$ROOT/build/android-build-config.json"
  SDK="$sdk" NDK="$ndk" node -e '
    const fs = require("fs");
    const [src, dst] = process.argv.slice(1);
    const c = JSON.parse(fs.readFileSync(src, "utf8"));
    c.packages.android = { ...c.packages.android, sdkPath: process.env.SDK, ndkPath: process.env.NDK };
    fs.writeFileSync(dst, JSON.stringify(c, null, 2) + "\n");
  ' "$PROJECT/build-config/android.json" "$cfg"
  NDK_ROOT="$ndk" ANDROID_SDK_ROOT="$sdk" ANDROID_HOME="$sdk" cocos_build "$cfg" build/cocos-android.log
  proj="$PROJECT/build/android/proj"
  [ -x "$proj/gradlew" ] || die "没找到 Gradle 工程：$proj"
  # Write game.json android.minSdk into the Gradle project; Creator's generated value ignores it.
  node -e '
    const fs = require("node:fs");
    const props = process.argv[1];
    const minSdk = require("./game.json").android.minSdk;
    const content = fs.readFileSync(props, "utf8");
    fs.writeFileSync(props, content.replace(/^PROP_MIN_SDK_VERSION=.*$/m, `PROP_MIN_SDK_VERSION=${minSdk}`));
  ' "$proj/gradle.properties"
  case "$mode" in
    project) echo "Gradle 工程：$proj" ;;
    apk) (cd "$proj" && ./gradlew assembleDebug) && echo "✓ apk：$(find "$proj" -name '*.apk' -path '*debug*' | head -1)" ;;
    aab)
      # Signing: the four RELEASE_ variables in ~/.gradle/gradle.properties (not in Git).
      local aab
      (cd "$proj" && ./gradlew bundleRelease) || die "AAB 编译失败"
      aab="$(find "$proj" -path '*/outputs/bundle/release/*.aab' | head -1)"
      [ -n "$aab" ] || die "没找到打好的 AAB"
      "$(find_keytool)" -printcert -rfc -jarfile "$aab" 2>/dev/null | grep -q -- '-----BEGIN CERTIFICATE-----' || die "${aab} 没有签名（检查 ~/.gradle/gradle.properties 的 RELEASE_ 变量）"
      android_check_16k "$aab" "$ndk" || die "16 KB 内存页检查没过，这个 AAB 不要传（${aab}）"
      echo "✓ aab：${aab}（已用上传密钥签名、真实广告位；传到 Google Play Console）" ;;
    *) die "未知的 Android 产物" ;;
  esac
}
read_choice() {
  printf '请输入编号：'
  IFS= read -r choice || exit 0
}

while :; do
  printf '%s\n' '打包平台' '1 微信小游戏' '2 iOS' '3 Android' '4 网页版' '0 退出'
  read_choice
  case "$choice" in
    1) node tools/native/apply.mjs; exec bash tools/wechat/build.sh ;;
    2)
      while :; do
        printf '%s\n' 'iOS 产物' '1 Xcode 工程' '2 模拟器 App' '3 未签名真机 App' '4 IPA' '5 装到手机' '6 上传 App Store Connect（上一次的 IPA）' '0 返回'
        read_choice
        case "$choice" in
          1) ios_build; exit ;;
          2) ios_build sim; exit ;;
          3) ios_build dev; exit ;;
          4) ios_build ipa; exit ;;
          5) ios_build phone; exit ;;
          6) ios_upload; exit ;;
          0) break ;;
          *) echo '请输入 0 至 6。' ;;
        esac
      done ;;
    3)
      while :; do
        printf '%s\n' 'Android 产物' '1 APK' '2 AAB' '3 Gradle 工程' '0 返回'
        read_choice
        case "$choice" in
          1) android_build apk; exit ;;
          2) android_build aab; exit ;;
          3) android_build project; exit ;;
          0) break ;;
          *) echo '请输入 0 至 3。' ;;
        esac
      done ;;
    4) exec bash scripts/build_web.sh ;;
    0) exit 0 ;;
    *) echo '请输入 0 至 4。' ;;
  esac
done
