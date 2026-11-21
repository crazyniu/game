#!/usr/bin/env bash
# Export a Creator project and optionally compile the local artifact.
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
project="$root/PerchSort"
platform="${1:?expected ios or android}"
mode="${2:-project}"
case "$platform:$mode" in
  ios:project|ios:sim|ios:dev|android:project|android:apk) ;;
  ios:ipa)
    : "${PERCH_IOS_BUNDLE_ID:?Set the actual iOS bundle ID for an IPA}"
    : "${PERCH_IOS_TEAM:?Set the Apple developer team for an IPA}"
    ;;
  android:aab)
    : "${PERCH_ANDROID_PACKAGE:?Set the actual application ID for a release bundle}"
    : "${PERCH_ANDROID_KEYSTORE:?Set the release keystore for a release bundle}"
    : "${PERCH_ANDROID_KEYSTORE_PASSWORD:?Set the release keystore password}"
    : "${PERCH_ANDROID_KEY_ALIAS:?Set the release key alias}"
    : "${PERCH_ANDROID_KEY_PASSWORD:?Set the release key password}"
    ;;
  *) echo "Expected ios [project|sim|dev|ipa] or android [project|apk|aab]" >&2; exit 2;;
esac
if [[ "$platform" == android && -z "${2:-}" ]]; then mode=apk; fi
creator="${COCOS_CREATOR:-/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator}"
if [[ ! -x "$creator" ]]; then echo "Set COCOS_CREATOR to the Creator 3.8.8 executable." >&2; exit 1; fi
if [[ "$platform" == ios ]]; then
  [[ "$(uname -s)" == Darwin ]] || { echo 'Xcode on macOS is required.' >&2; exit 1; }
elif [[ -z "${ANDROID_HOME:-}" && -d "${HOME}/Library/Android/sdk" ]]; then
  export ANDROID_HOME="${HOME}/Library/Android/sdk"
fi
cd "$root"
[[ -d PerchSort/node_modules ]] || npm --prefix PerchSort ci --no-audit --no-fund
node tools/native/apply.mjs "$platform" "$mode"
python3 tools/cocos-meta.py
mkdir -p build
status=0
"$creator" --project "$project" --build "configPath=$root/build/native-config/$platform.json" > "build/cocos-$platform.log" 2>&1 || status=$?
if [[ "$status" != 36 && "$status" != 0 ]]; then tail -45 "build/cocos-$platform.log" >&2; exit "$status"; fi
out="$project/build/$platform"
if [[ ! -d "$out/proj" ]]; then echo "Creator did not generate $out/proj. See build/cocos-$platform.log." >&2; exit 1; fi
node tools/native/icons.mjs
if [[ "$platform" == android ]]; then
  [[ "$mode" != project ]] || { echo "$out/proj"; exit 0; }
  [[ -f "$out/proj/gradlew" ]] || { echo "No Gradle wrapper in $out/proj." >&2; exit 1; }
  if [[ "$mode" == apk ]]; then task=assembleDebug; else task=bundleRelease; fi
  # Earlier outputs are removed first, so the one found afterwards is this build's.
  find "$out/proj" -type f -path '*/outputs/*' -name "*.$mode" -delete
  (cd "$out/proj" && bash ./gradlew "$task")
  artifacts=()
  while IFS= read -r file; do artifacts+=("$file"); done < <(find "$out/proj" -type f -path '*/outputs/*' -name "*.$mode")
  [[ ${#artifacts[@]} -eq 1 ]] || { echo "Expected one new .$mode in $out/proj, found ${#artifacts[@]}." >&2; exit 1; }
  artifact="${artifacts[0]}"
  if [[ "$mode" == aab ]]; then
    release="$(keytool -list -v -keystore "$PERCH_ANDROID_KEYSTORE" -storepass "$PERCH_ANDROID_KEYSTORE_PASSWORD" -alias "$PERCH_ANDROID_KEY_ALIAS" | awk '/SHA256:/{print $2; exit}')"
    signer="$(keytool -printcert -jarfile "$artifact" | awk '/SHA256:/{print $2; exit}')"
    [[ -n "$release" && "$release" == "$signer" ]] || { echo "$artifact is not signed with the release key ${PERCH_ANDROID_KEY_ALIAS}." >&2; exit 1; }
  fi
  echo "$artifact"
  exit 0
fi
shopt -s nullglob
projects=("$out"/proj/*.xcodeproj)
[[ ${#projects[@]} -eq 1 ]] || { echo "Expected one Xcode project in $out/proj." >&2; exit 1; }
xcode="${projects[0]}"
scheme="$(basename "$xcode" .xcodeproj)-mobile"
cmake="$project/native/engine/ios/CMakeLists.txt"
hook='include("${CMAKE_CURRENT_LIST_DIR}/../../../../tools/native/ios-compat.cmake")'
if ! grep -Fqx "$hook" "$cmake"; then printf '\n%s\n' "$hook" >> "$cmake"; fi
cmake_bin="$(dirname "$(dirname "$creator")")/Resources/tools/cmake/bin/cmake"
"$cmake_bin" -S "$project/native/engine/ios" -B "$out/proj"
webp="$(dirname "$(dirname "$creator")")/Resources/resources/3d/engine/native/external/ios/libs/libwebp.a"
python3 tools/native/ios-webp.py "$webp" "$out/libwebp/device.a"
python3 tools/native/ios-webp.py "${webp%/ios/libs/libwebp.a}/ios-m1-simulator/libs/libwebp.a" "$out/libwebp/simulator.a"
python3 tools/native/ios-project.py "$xcode"
case "$mode" in
  project) echo "$xcode";;
  sim)
    xcodebuild -project "$xcode" -scheme "$scheme" -sdk iphonesimulator -destination 'generic/platform=iOS Simulator' -configuration Debug \
      -derivedDataPath "$out/simulator" ARCHS="$(uname -m)" ONLY_ACTIVE_ARCH=YES CODE_SIGNING_ALLOWED=NO build
    bundle="$out/proj/Debug-iphonesimulator/$scheme.app"
    test -x "$bundle/$scheme" || { echo "Simulator build has no executable: $bundle" >&2; exit 1; }
    echo "$bundle";;
  dev)
    xcodebuild -project "$xcode" -scheme "$scheme" -sdk iphoneos -configuration Release \
      -derivedDataPath "$out/device" CODE_SIGNING_ALLOWED=NO build
    bundle="$out/proj/Release-iphoneos/$scheme.app"
    test -x "$bundle/$scheme" || { echo "Device build has no executable: $bundle" >&2; exit 1; }
    echo "$bundle";;
  ipa)
    export_dir="$out/export"
    mkdir -p "$export_dir"
    xcodebuild -project "$xcode" -scheme "$scheme" -sdk iphoneos -configuration Release \
      -archivePath "$out/PerchSort.xcarchive" DEVELOPMENT_TEAM="$PERCH_IOS_TEAM" archive
    /usr/bin/plutil -create xml1 "$export_dir/ExportOptions.plist"
    /usr/bin/plutil -insert method -string app-store-connect "$export_dir/ExportOptions.plist"
    /usr/bin/plutil -insert destination -string export "$export_dir/ExportOptions.plist"
    /usr/bin/plutil -insert teamID -string "$PERCH_IOS_TEAM" "$export_dir/ExportOptions.plist"
    xcodebuild -exportArchive -archivePath "$out/PerchSort.xcarchive" -exportPath "$export_dir" \
      -exportOptionsPlist "$export_dir/ExportOptions.plist"
    echo "$export_dir";;
esac
