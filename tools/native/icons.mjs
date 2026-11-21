// Derive all native sizes from the game's painted forest and Sunny portrait.
import { createRequire } from 'node:module';
import { copyFile, mkdir, readFile, writeFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../../', import.meta.url));
const require = createRequire(new URL('../../PerchSort/package.json', import.meta.url));
const sharp = require('sharp');
const resources = resolve(root, 'PerchSort/assets/resources');
const forest = resolve(resources, 'art/island/background_home_jungle.png');
// Sunny's front pose, cut from its frame sheet (resources/manifest.json: piece = x, y, offset x, offset y, width, height).
const manifest = JSON.parse(await readFile(resolve(resources, 'manifest.json'), 'utf8'));
const front = manifest.images['birds/sunny_front'];
const [px, py, , , pw, ph] = front.piece;
const sunny = await sharp(resolve(resources, front.path + '.webp')).extract({ left: px, top: py, width: pw, height: ph }).png().toBuffer();
const out = resolve(root, 'build/native-art');
const engine = resolve(root, 'PerchSort/native/engine');
async function put(file, data) {
  await mkdir(dirname(file), {recursive:true});
  await writeFile(file, data);
}
const portrait = await sharp(sunny).trim().resize(860,860,{fit:'inside'}).png().toBuffer();
const icon = await sharp(forest).resize(1024,1024,{fit:'cover'}).composite([{input:portrait,gravity:'center'}]).flatten().png().toBuffer();
for(const [name,size] of [['app_store',1024],['google_play',512],['wechat',144]]) {
  await put(resolve(out,'store',name+'.png'),await sharp(icon).resize(size,size).png().toBuffer());
}
const sizes = [29,40,57,58,60,80,87,114,120,180,1024];
const iosPath=resolve(engine,'ios/Images.xcassets/AppIcon.appiconset');
const iosIcons=existsSync(resolve(iosPath,'Contents.json'))
  ? [...new Map(JSON.parse(await readFile(resolve(iosPath,'Contents.json'),'utf8')).images.filter(x=>x.filename).map(x=>[x.filename,Number(x['expected-size']) || Math.round(parseFloat(x.size)*parseFloat(x.scale))])).entries()]
  : sizes.map(n=>[n+'.png',n]);
for(const [name,size] of iosIcons) {
  const file=resolve(out,'ios',name);
  await put(file,await sharp(icon).resize(size,size).removeAlpha().png().toBuffer());
  if(existsSync(iosPath)) await copyFile(file,resolve(iosPath,name));
}
for(const [name,w,h] of [['LaunchScreenBackgroundPortrait',1290,2796],['LaunchScreenBackground',1290,2796],['LaunchScreenBackgroundLandscape',2796,1290]]) {
  const file=resolve(out,'ios',name+'.png');
  const chick=await sharp(sunny).trim().resize(480,480,{fit:'inside'}).png().toBuffer();
  await put(file,await sharp(forest).resize(w,h,{fit:'cover'}).composite([{input:chick,gravity:'center'}]).removeAlpha().png().toBuffer());
  if(existsSync(iosPath))await copyFile(file,resolve(engine,'ios',name+'.png'));
}
for(const [density,size] of [['mdpi',48],['hdpi',72],['xhdpi',96],['xxhdpi',144],['xxxhdpi',192]]) {
  const relative='res/mipmap-'+density+'/ic_launcher.png';
  const file=resolve(out,'android',relative);
  await put(file,await sharp(icon).resize(size,size).png().toBuffer());
  if(existsSync(resolve(engine,'android/app'))) {
    const dest=resolve(engine,'android',relative);
    await mkdir(dirname(dest),{recursive:true});await copyFile(file,dest);
  }
}
console.log(out);
