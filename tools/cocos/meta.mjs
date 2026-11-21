#!/usr/bin/env node
// Write stable Cocos asset identities for every file and folder under PerchSort/assets that has no .meta yet.
// The UUID is derived from the asset's relative path, so it is the same on every machine; existing metas are never changed.
// Image metas match what Creator writes for an untrimmed sprite frame, so a build leaves them unchanged.
// Usage: node tools/cocos/meta.mjs           write the missing metas
//        node tools/cocos/meta.mjs --check   report missing, orphaned or broken metas and exit 1 (CI)
//        node tools/cocos/meta.mjs uuid <relative path>   print the UUID for a path under assets/
import { createHash } from 'node:crypto';
import { existsSync, readFileSync, readdirSync, statSync, writeFileSync } from 'node:fs';
import { basename, extname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const ASSETS = fileURLToPath(new URL('../../PerchSort/assets/', import.meta.url));
const NAMESPACE = '9a21e085-c160-43cc-b108-f67df530e04d';

/** RFC 4122 version 5 UUID (the same value as Python's uuid.uuid5). */
export function uuidFor(rel) {
  const hash = createHash('sha1').update(Buffer.from(NAMESPACE.replace(/-/g, ''), 'hex')).update(rel, 'utf8').digest();
  hash[6] = (hash[6] & 0x0f) | 0x50;
  hash[8] = (hash[8] & 0x3f) | 0x80;
  const hex = hash.subarray(0, 16).toString('hex');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

// Creator's own meta format for each asset kind (read from metas Creator wrote in this project).
const FORMATS = {
  directory: { ver: '1.2.0', importer: 'directory', files: [] },
  '.ts': { ver: '4.0.24', importer: 'typescript', files: [] },
  '.json': { ver: '2.0.1', importer: 'json', files: ['.json'] },
  '.wav': { ver: '1.0.0', importer: 'audio-clip', files: ['.json', '.wav'], userData: { downloadMode: 0 } },
  '.effect': { ver: '1.7.1', importer: 'effect', files: ['.json'] },
  '.scene': { ver: '1.1.50', importer: 'scene', files: ['.json'] },
  text: { ver: '1.0.1', importer: 'text', files: ['.json'] },
  '.ttf': { ver: '1.0.1', importer: 'ttf-font', files: ['.json'] },
};
const IMAGES = ['.png', '.webp'];
// Creator writes these on the built-in resources bundle; written up front so a build leaves the meta unchanged.
const FOLDER_DATA = { resources: { isBundle: true, bundleConfigID: 'default', bundleName: 'resources', priority: 8 } };

function vertices(w, h) {
  const x = w / 2, y = h / 2;
  return { rawPosition: [-x, -y, 0, x, -y, 0, -x, y, 0, x, y, 0], indexes: [0, 1, 2, 2, 1, 3], uv: [0, h, w, h, 0, 0, w, 0], nuv: [0, 0, 1, 0, 0, 1, 1, 1], minPos: [-x, -y, 0], maxPos: [x, y, 0] };
}

let manifest;
function imageSize(rel, ext) {
  manifest ??= JSON.parse(readFileSync(join(ASSETS, 'resources/manifest.json'), 'utf8'));
  const key = rel.replace(/^resources\//, '').slice(0, -ext.length);
  const entry = manifest.images[key];
  if (!entry) throw new Error(`${rel} is not in resources/manifest.json; run node tools/cocos-assets.mjs first`);
  return entry.size;
}

function metaFor(abs, rel, isDir) {
  const ext = extname(abs);
  const uuid = uuidFor(rel);
  if (!IMAGES.includes(ext)) {
    const format = isDir ? FORMATS.directory : FORMATS[ext] ?? FORMATS.text;
    const files = ext === '.ttf' ? [...format.files, basename(abs)] : format.files;   // a font keeps its file next to the import
    return { ver: format.ver, importer: format.importer, imported: true, uuid, files, subMetas: {}, userData: (isDir && FOLDER_DATA[rel]) || format.userData || {} };
  }
  const data = { ver: '1.0.27', importer: 'image', imported: true, uuid, files: [], subMetas: {}, userData: {} };
  const [w, h] = imageSize(rel, ext);
  const name = rel.split('/').pop().slice(0, -ext.length);
  data.files = ['.json', ext];
  data.userData = { type: 'sprite-frame', redirect: `${uuid}@6c48a`, hasAlpha: true, fixAlphaTransparencyArtifacts: true };
  data.subMetas['6c48a'] = {
    ver: '1.0.22', importer: 'texture', imported: true, uuid: `${uuid}@6c48a`, displayName: name, id: '6c48a', name: 'texture', files: ['.json'], subMetas: {},
    userData: { wrapModeS: 'clamp-to-edge', wrapModeT: 'clamp-to-edge', minfilter: 'linear', magfilter: 'linear', mipfilter: 'none', anisotropy: 0, isUuid: true, imageUuidOrDatabaseUri: uuid, visible: false },
  };
  data.subMetas.f9941 = {
    ver: '1.0.12', importer: 'sprite-frame', imported: true, uuid: `${uuid}@f9941`, displayName: name, id: 'f9941', name: 'spriteFrame', files: ['.json'], subMetas: {},
    userData: {
      trimType: 'none', trimThreshold: 1, rotated: false, offsetX: 0, offsetY: 0, trimX: 0, trimY: 0, width: w, height: h, rawWidth: w, rawHeight: h,
      borderTop: 0, borderBottom: 0, borderLeft: 0, borderRight: 0, packable: false, pixelsToUnit: 100, pivotX: 0.5, pivotY: 0.5, meshType: 0,
      isUuid: true, imageUuidOrDatabaseUri: `${uuid}@6c48a`, atlasUuid: '', vertices: vertices(w, h),
    },
  };
  return data;
}

const check = process.argv.includes('--check');
const missing = [], orphans = [], broken = [];
function walk(dir) {
  for (const name of readdirSync(dir).sort()) {
    if (name.startsWith('.')) continue;
    const abs = join(dir, name);
    const rel = relative(ASSETS, abs).split('\\').join('/');
    if (name.endsWith('.meta')) {
      if (!existsSync(abs.slice(0, -5))) orphans.push(rel);
      else if (readFileSync(abs, 'utf8').includes('"importer": "*"')) broken.push(rel);
      continue;
    }
    const isDir = statSync(abs).isDirectory();
    if (!existsSync(`${abs}.meta`)) {
      missing.push(rel);
      if (!check) writeFileSync(`${abs}.meta`, JSON.stringify(metaFor(abs, rel, isDir), null, 2) + '\n');
    }
    if (isDir) walk(abs);
  }
}

if (process.argv[2] === 'uuid') {
  console.log(uuidFor(process.argv[3]));
  process.exit(0);
}
walk(ASSETS);
if (check) {
  for (const rel of missing) console.error(`missing meta: ${rel}`);
  for (const rel of orphans) console.error(`meta without asset: ${rel}`);
  for (const rel of broken) console.error(`broken import (importer "*"): ${rel}`);
  if (missing.length + orphans.length + broken.length) process.exit(1);
} else {
  for (const rel of missing) console.log(`+ ${rel}.meta`);
  for (const rel of broken) console.error(`broken import (importer "*"), restore it from Git: ${rel}`);
}
