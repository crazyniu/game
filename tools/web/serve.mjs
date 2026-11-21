// Static server for the web build (./run.sh play, serve, lan). Node built-ins only.
// Python's http.server queues five connections; another device fetching dozens of files at once gets resets and missing art.
// /__profile returns the prior Godot save read-only, and only to requests from this machine (Boot.ts imports it once).
// Usage: node tools/web/serve.mjs <directory> <port> [host, default 127.0.0.1]
import { createReadStream, existsSync, readFileSync, statSync } from 'node:fs';
import { createServer } from 'node:http';
import { homedir, platform } from 'node:os';
import { extname, join, normalize, resolve, sep } from 'node:path';

const [dir, port, host = '127.0.0.1'] = process.argv.slice(2);
if (!dir || !port) {
  console.error('Usage: node tools/web/serve.mjs <directory> <port> [host]');
  process.exit(2);
}
const root = resolve(dir);
const legacy = {
  darwin: join(homedir(), 'Library/Application Support/Godot/app_userdata/Perch Sort/profile.json'),
  win32: join(process.env.APPDATA ?? '', 'Godot/app_userdata/Perch Sort/profile.json'),
}[platform()] ?? join(homedir(), '.local/share/godot/app_userdata/Perch Sort/profile.json');
const TYPES = {
  '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.mjs': 'text/javascript; charset=utf-8',
  '.json': 'application/json', '.css': 'text/css; charset=utf-8', '.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg',
  '.webp': 'image/webp', '.svg': 'image/svg+xml', '.ico': 'image/x-icon', '.wav': 'audio/wav', '.mp3': 'audio/mpeg', '.ogg': 'audio/ogg',
  '.ttf': 'font/ttf', '.woff': 'font/woff', '.woff2': 'font/woff2', '.wasm': 'application/wasm', '.bin': 'application/octet-stream',
};
const LOOPBACK = new Set(['127.0.0.1', '::1', '::ffff:127.0.0.1']);

const server = createServer((req, res) => {
  let path;
  try {
    path = decodeURIComponent((req.url ?? '/').split('?')[0]);
  } catch {
    res.writeHead(400).end();
    return;
  }
  if (path === '/__profile') {
    const body = LOOPBACK.has(req.socket.remoteAddress ?? '') && existsSync(legacy) ? readFileSync(legacy) : Buffer.from('null');
    res.writeHead(200, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store', 'Content-Length': body.length }).end(body);
    return;
  }
  let file = normalize(join(root, path));
  if (file !== root && !file.startsWith(root + sep)) {
    res.writeHead(403).end();
    return;
  }
  let st;
  try {
    st = statSync(file);
    if (st.isDirectory()) {
      file = join(file, 'index.html');
      st = statSync(file);
    }
  } catch {
    res.writeHead(404).end();
    return;
  }
  // Every reload gets the latest build.
  res.writeHead(200, { 'Content-Type': TYPES[extname(file).toLowerCase()] ?? 'application/octet-stream', 'Content-Length': st.size, 'Cache-Control': 'no-store' });
  if (req.method === 'HEAD') {
    res.end();
    return;
  }
  createReadStream(file).on('error', () => res.destroy()).pipe(res);
});
server.listen(Number(port), host, 511, () => console.log(`Serving ${root} on ${host}:${port}`));
