// PNG compression for PerchSort/assets (./run.sh art-compress, ./run.sh check).
// Approved art must look identical, so nothing is quantised: a file is rewritten only when a maximum-compression lossless encoding (never palette)
// decodes to exactly the same pixels, keeps its colour profile and is smaller.
// A PNG counts as uncompressed when its zlib stream was written at the fastest level (header FLEVEL 0).
// Usage: node tools/art/compress.mjs           recompress uncompressed PNGs
//        node tools/art/compress.mjs <files>   recompress exactly these files
//        node tools/art/compress.mjs --check   exit 1 when a PNG is uncompressed (fast: reads headers only)
import { readFileSync, writeFileSync } from 'node:fs';
import { execFileSync } from 'node:child_process';
import { createRequire } from 'node:module';

const check = process.argv.includes('--check');
const named = process.argv.slice(2).filter((a) => !a.startsWith('--'));

/** zlib FLEVEL (0 fastest .. 3 maximum) of the first IDAT chunk. */
function level(buf) {
  for (let i = 8; i + 10 < buf.length; i += 12 + buf.readUInt32BE(i)) {
    if (buf.toString('ascii', i + 4, i + 8) === 'IDAT') return buf[i + 9] >> 6;
  }
  throw new Error('PNG without image data');
}

const all = execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard', 'PerchSort/assets/*.png'], { encoding: 'utf8' }).split('\n').filter(Boolean);
const loose = all.filter((f) => level(readFileSync(f)) === 0);
if (check) {
  for (const f of loose) console.error(`uncompressed PNG: ${f}`);
  if (loose.length) {
    console.error('Run ./run.sh art-compress (lossless) and commit the result');
    process.exit(1);
  }
  process.exit(0);
}

const sharp = createRequire(new URL('../../PerchSort/package.json', import.meta.url))('sharp');
async function samePixels(a, b) {
  const [x, y] = await Promise.all([a, b].map((buf) => sharp(buf).raw().toBuffer({ resolveWithObject: true })));
  return x.info.width === y.info.width && x.info.height === y.info.height && x.info.channels === y.info.channels && x.data.equals(y.data);
}
let before = 0, after = 0;
const targets = named.length ? named : loose;
for (const file of targets) {
  const input = readFileSync(file);
  const output = await sharp(input).keepIccProfile().png({ compressionLevel: 9, adaptiveFiltering: true, palette: false }).toBuffer();
  before += input.length;
  if (output.length < input.length && (await samePixels(input, output))) {
    writeFileSync(file, output);
    after += output.length;
    console.log(`${file}: ${input.length} -> ${output.length}`);
  } else after += input.length;
}
console.log(`${targets.length} PNG files: ${(before / 1048576).toFixed(1)} MB -> ${(after / 1048576).toFixed(1)} MB, pixels unchanged`);
