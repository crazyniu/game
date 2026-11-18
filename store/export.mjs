// Resize completed, localized poster paintings into the same delivery formats as Fishsort.
import {createRequire} from 'node:module';
import {mkdir,copyFile} from 'node:fs/promises';
import {join} from 'node:path';
import {fileURLToPath} from 'node:url';
const root=fileURLToPath(new URL('../',import.meta.url));
const sharp=createRequire(new URL('../PerchSort/package.json',import.meta.url))('sharp');
const masters=process.argv[2] || join(root,'store/masters');
const output=join(root,'store/images');
const themes=['flock','homecoming','forests','outfits','chest'];
const platforms=[['app_store_6.9',1320,2868],['app_store_6.5',1284,2778],['google_play',1080,1920],['wechat',1080,1920]];
for (const [platform,width,height] of platforms) {
  for (const locale of platform==='wechat'?['zh_CN']:['en','zh_CN']) {
    const folder=join(output,platform,locale);
    await mkdir(folder,{recursive:true});
    for (const theme of themes) {
      await sharp(join(masters,theme+'_'+locale+'.png')).resize(width,height,{fit:'cover',position:'centre'}).removeAlpha().jpeg({quality:93,mozjpeg:true}).toFile(join(folder,theme+'.jpg'));
    }
    if (platform==='google_play') {
      await sharp(join(masters,'feature_'+locale+'.png')).resize(1024,500,{fit:'cover'}).removeAlpha().jpeg({quality:93,mozjpeg:true}).toFile(join(folder,'feature.jpg'));
    }
  }
}
const iconFolder=join(output,'icon');
await mkdir(iconFolder,{recursive:true});
for (const name of ['app_store','google_play']) {
  await copyFile(join(root,'build/native-art/store',name+'.png'),join(iconFolder,name+'.png'));
}

await sharp(join(masters,'wechat_avatar.png')).resize(144,144).png().toFile(join(iconFolder,'wechat.png'));

const tiles=[];
for (const [row,locale] of ['en','zh_CN'].entries()) {
  for (const [column,theme] of themes.entries()) {
    const input=await sharp(join(output,'google_play',locale,theme+'.jpg')).resize(162,288).toBuffer();
    tiles.push({input,left:12+column*170,top:12+row*296});
  }
  const input=await sharp(join(output,'google_play',locale,'feature.jpg')).resize(402,196).toBuffer();
  tiles.push({input,left:12+row*432,top:608});
}
await sharp({create:{width:866,height:816,channels:3,background:'#163729'}}).composite(tiles).jpeg({quality:90}).toFile(join(root,'store/preview.jpg'));
