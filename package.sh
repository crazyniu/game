#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

choose() {
  printf '请输入编号：'
  IFS= read -r choice || exit 0
}

while :; do
  printf '\n选择打包平台\n1. 微信小游戏\n2. iOS\n3. Android\n4. 网页版\n0. 退出\n'
  choose
  case "$choice" in
    1) exec bash tools/wechat/build.sh ;;
    2)
      while :; do
        printf '\niOS 产物\n1. Xcode 工程\n2. 模拟器 App\n3. 未签名真机 App\n4. 签名 IPA\n0. 返回\n'
        choose
        case "$choice" in
          1) exec bash tools/native/build.sh ios project ;;
          2) exec bash tools/native/build.sh ios sim ;;
          3) exec bash tools/native/build.sh ios dev ;;
          4) exec bash tools/native/build.sh ios ipa ;;
          0) break ;;
          *) echo '请输入菜单中的编号。' ;;
        esac
      done ;;
    3)
      while :; do
        printf '\nAndroid 产物\n1. 调试 APK\n2. 发布 AAB\n3. Gradle 工程\n0. 返回\n'
        choose
        case "$choice" in
          1) exec bash tools/native/build.sh android apk ;;
          2) exec bash tools/native/build.sh android aab ;;
          3) exec bash tools/native/build.sh android project ;;
          0) break ;;
          *) echo '请输入菜单中的编号。' ;;
        esac
      done ;;
    4) exec bash scripts/build_web.sh ;;
    0) exit 0 ;;
    *) echo '请输入菜单中的编号。' ;;
  esac
done
