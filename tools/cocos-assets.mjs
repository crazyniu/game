import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
const require = createRequire(new URL('../PerchSort/package.json', import.meta.url));
const sharp = require('sharp');
const root = path.resolve(import.meta.dirname, '../PerchSort/assets/resources');
async function files(dir) { const out=[]; for (const e of await fs.readdir(dir,{withFileTypes:true})) { const p=path.join(dir,e.name); if(e.isDirectory())out.push(...await files(p));else out.push(p); } return out; }
let paths=await files(root);
for(const p of paths.filter(p=>p.endsWith('.svg'))) await sharp(p,{density:192}).png().toFile(p.replace(/\.svg$/,'.png'));
paths=await files(root);
const images={}, data={}, sounds={};
let pixels=0;
for(const p of paths){
 const relative=path.relative(root,p).replaceAll(path.sep,'/'), key=relative.replace(/\.[^.]+$/,'');
 if(/\.(png|webp)$/.test(p)){
  const {data:raw,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let left=info.width,top=info.height,right=-1,bottom=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(raw[(y*info.width+x)*4+3]>0){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
  images[key]={path:key,size:[info.width,info.height],ink:right<0?[0,0,info.width,info.height]:[left,top,right-left+1,bottom-top+1]};pixels+=info.width*info.height;
 }else if(p.endsWith('.json')&&!relative.startsWith('manifest'))data[key]=JSON.parse(await fs.readFile(p,'utf8'));
 else if(/\.(wav|mp3|ogg)$/.test(p))sounds[path.basename(key)]=key;
}
const aliases={};
// Match the original Art lookup priority exactly.
for(const dir of ['art','art/island','art/icons','birds','world','world/scene']){
 for(const [key,value] of Object.entries(images)){
  if(key.startsWith(dir+'/')){const name=key.slice(dir.length+1);if(!aliases[name])aliases[name]=key;}
 }
}
await fs.writeFile(path.join(root,'manifest.json'),JSON.stringify({images,aliases,data,sounds}));
console.log(`${Object.keys(images).length} images, ${Object.keys(data).length} JSON documents, ${Object.keys(sounds).length} sounds; ${Math.round(pixels*4/1024/1024)} MiB decoded (loaded on demand)`);
