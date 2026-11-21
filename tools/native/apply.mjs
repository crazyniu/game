#!/usr/bin/env node
// Apply the repository's game.json (the game's identity) to every platform: the runtime platform config, Creator build
// configurations, Info.plist and per-language app names, CMakeLists, AndroidManifest and Gradle. ./package.sh runs it
// before each package; ./run.sh native-apply runs it alone. Only the listed values change; running it twice is a no-op.
//
// Real ad units are used only for store packages (./package.sh IPA and AAB, GAME_RELEASE=1). Every other build uses
// Google's test units from game.json ads.test: tapping a real ad while testing is invalid traffic. Without real units a
// store package simply has no ads (the platform reports none). WeChat uses only WeChat's own units.
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = fileURLToPath(new URL('../../', import.meta.url));
const PROJECT = join(ROOT, 'PerchSort');
const game = JSON.parse(readFileSync(join(ROOT, 'game.json'), 'utf8'));
const changed = [];

function write(path, text) {
  if (existsSync(path) && readFileSync(path, 'utf8') === text) return;
  mkdirSync(dirname(path), { recursive: true });
  writeFileSync(path, text);
  changed.push(path.slice(ROOT.length));
}
const edit = (path, fn) => write(path, fn(readFileSync(path, 'utf8')));
const json = (v) => JSON.stringify(v, null, 2) + '\n';
const xml = (s) => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
/** Xcode and Gradle project name: letters and digits only. */
const projectName = game.name.en.replace(/[^A-Za-z0-9]/g, '');
/** Game language code -> iOS .lproj and Android values qualifiers; English is iOS Base and Android's default values. */
const LOCALES = { en: { ios: 'en', android: [] }, zh_CN: { ios: 'zh-Hans', android: ['zh-rCN'] } };
const appLangs = game.languages.app;
for (const l of appLangs) if (!LOCALES[l]) throw new Error(`game.json languages.app has ${l}, which apply.mjs LOCALES lacks`);
const adsOn = game.ads.enabled === true;
const release = process.env.GAME_RELEASE === '1';
const units = (platform) => (release ? game.ads[platform] : game.ads.test[platform]);
if (adsOn) console.log(release ? 'Store package: real ad units' : 'Test build: Google test ad units');

// 1. Runtime platform config (assets/scripts/platform/Platform.ts).
write(join(PROJECT, 'assets/resources/config/platform.json'), json({
  name: game.name,
  version: game.version,
  adsEnabled: adsOn,
  privacyPolicy: game.privacyPolicy ?? '',
  ads: { ios: units('ios'), android: units('android'), wechat: game.ads.wechat },
}));

// 2. Creator build configurations.
for (const platform of ['ios', 'android', 'wechatgame']) {
  const path = join(PROJECT, 'build-config', `${platform}.json`);
  edit(path, (s) => {
    const c = JSON.parse(s);
    if (platform === 'ios') {
      c.name = projectName;
      c.packages.ios = { ...c.packages.ios, packageName: game.ios.bundleId, developerTeam: game.ios.team ?? '', targetVersion: game.ios.minVersion };
    } else if (platform === 'android') {
      c.name = projectName;
      c.packages.android = { ...c.packages.android, packageName: game.android.packageName, apiLevel: game.android.targetSdk };
    } else if (game.wechat.appid) {
      c.packages.wechatgame = { ...c.packages.wechatgame, appid: game.wechat.appid };
    }
    return json(c);
  });
}

// 3. iOS: version, the ad keys (AdMob app ID, tracking text, SKAdNetwork), per-language app names, SDK linking.
const ios = join(PROJECT, 'native/engine/ios');
const plistValue = (s, key, value) => s.replace(new RegExp(`(<key>${key}</key>\\s*<string>)[^<]*(</string>)`), `$1${xml(value)}$2`);
// Ad keys sit before ITSAppUsesNonExemptEncryption when ads are on and are removed when off
// (a tracking description without a tracking prompt draws App Review questions).
const AD_KEYS = /\t<key>GADApplicationIdentifier<\/key>[\s\S]*?<key>SKAdNetworkItems<\/key>\s*<array>[\s\S]*?<\/array>\n/;
const adKeys = [
  '\t<key>GADApplicationIdentifier</key>', `\t<string>${xml(game.ios.admobAppId)}</string>`,
  '\t<key>NSUserTrackingUsageDescription</key>', `\t<string>${xml(game.ios.trackingText.en)}</string>`,
  '\t<key>SKAdNetworkItems</key>', '\t<array>', '\t\t<dict>', '\t\t\t<key>SKAdNetworkIdentifier</key>',
  '\t\t\t<string>cstr6suwn9.skadnetwork</string>', '\t\t</dict>', '\t</array>', '',
].join('\n');
edit(join(ios, 'Info.plist'), (s) => {
  const locs = appLangs.map((l) => `\t\t<string>${LOCALES[l].ios}</string>`).join('\n');
  s = s.replace(/(<key>CFBundleLocalizations<\/key>\s*<array>)[\s\S]*?(\s*<\/array>)/, `$1\n${locs}$2`);
  s = plistValue(s, 'CFBundleDisplayName', game.name.en);
  s = plistValue(s, 'CFBundleShortVersionString', game.version);
  s = plistValue(s, 'CFBundleVersion', String(game.ios.build ?? game.build));
  s = s.replace(AD_KEYS, '');
  if (adsOn) s = s.replace('\t<key>ITSAppUsesNonExemptEncryption</key>', `${adKeys}\t<key>ITSAppUsesNonExemptEncryption</key>`);
  return s;
});
const strings = (lang) => `/* Written by tools/native/apply.mjs from game.json */\n"CFBundleDisplayName" = "${game.name[lang]}";\n`
  + (adsOn ? `"NSUserTrackingUsageDescription" = "${game.ios.trackingText[lang]}";\n` : '');
const lproj = ['Base', ...appLangs.map((l) => LOCALES[l].ios)].join(' ');
edit(join(ios, 'CMakeLists.txt'), (s) => s
  .replace(/set\(APP_NAME "[^"]*"/, `set(APP_NAME "${projectName}"`)
  .replace(/set\(GAME_ADS (ON|OFF)\)/, `set(GAME_ADS ${adsOn ? 'ON' : 'OFF'})`)
  .replace(/set\(GAME_LPROJ [^)]*\)/, `set(GAME_LPROJ ${lproj})`));
write(join(ios, 'Base.lproj/InfoPlist.strings'), strings('en'));
for (const l of appLangs) write(join(ios, `${LOCALES[l].ios}.lproj/InfoPlist.strings`), strings(l));

// 4. Android: version, AdMob app ID, per-language app names, ad SDK (GAME_ADS) and the advertising ID permission.
const android = join(PROJECT, 'native/engine/android');
edit(join(android, 'app/build.gradle'), (s) => s
  .replace(/versionCode \d+/, `versionCode ${game.build}`)
  .replace(/versionName "[^"]*"/, `versionName "${game.version}"`)
  .replace(/def GAME_ADS = (true|false)/, `def GAME_ADS = ${adsOn}`));
const AD_ID = /<uses-permission android:name="com\.google\.android\.gms\.permission\.AD_ID"[^>]*\/>/;
edit(join(android, 'app/AndroidManifest.xml'), (s) => s
  .replace(/(android:name="com\.google\.android\.gms\.ads\.APPLICATION_ID"\s*android:value=")[^"]*(")/, `$1${game.android.admobAppId}$2`)
  .replace(AD_ID, `<uses-permission android:name="com.google.android.gms.permission.AD_ID"${adsOn ? '' : ' tools:node="remove"'}/>`));
const appName = (name, note) => `<resources>\n    <!-- ${note} (tools/native/apply.mjs, game.json). Not app_name: Creator generates that from the build name. -->\n    <string name="game_app_name">${xml(name).replace(/'/g, "\\'")}</string>\n</resources>\n`;
write(join(android, 'res/values/strings.xml'), appName(game.name.en, 'App name'));
for (const l of appLangs) for (const q of LOCALES[l].android) write(join(android, `res/values-${q}/strings.xml`), appName(game.name[l], 'App name in this system language'));

console.log(changed.length ? `Applied game.json:\n  ${changed.join('\n  ')}` : 'Platform files already match game.json');
