// Resolve local build identities without changing the tracked Creator templates.
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../../', import.meta.url));
const read = file => JSON.parse(readFileSync(resolve(root, file), 'utf8'));
const dir = resolve(root, 'build/native-config');
mkdirSync(dir, {recursive: true});
const scene = read('PerchSort/assets/scenes/main.scene.meta').uuid;
const platform = process.argv[2];
for (const target of platform ? [platform] : ['ios', 'android']) {
  if (!['ios', 'android'].includes(target)) throw new Error('Expected ios or android');
  const config = read('PerchSort/build-config/' + target + '.json');
  config.startScene = scene;
  config.scenes = [{url: 'db://assets/scenes/main.scene', uuid: scene}];
  const options = config.packages[target];
  if (target === 'ios') {
    options.packageName = process.env.PERCH_IOS_BUNDLE_ID || options.packageName;
    options.developerTeam = process.env.PERCH_IOS_TEAM || '';
  } else {
    options.packageName = process.env.PERCH_ANDROID_PACKAGE || options.packageName;
    options.appBundle = process.argv[3] === 'aab';
    options.useDebugKeystore = !options.appBundle;
  }
  const path = resolve(dir, target + '.json');
  writeFileSync(path, JSON.stringify(config, null, 2) + '\n');
  console.log(path);
}
