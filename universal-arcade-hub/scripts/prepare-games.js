import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

const root = process.cwd();
const gamesDir = path.join(root, 'public', 'games');
const manifestPath = path.join(root, 'public', 'games-manifest.json');
const watch = process.argv.includes('--watch');

function titleFromId(id) {
  return id.replace(/[-_]+/g, ' ').replace(/\b\w/g, (m) => m.toUpperCase());
}

function readJson(file) {
  try { return JSON.parse(fs.readFileSync(file, 'utf8')); } catch { return null; }
}

function detectGame(folderPath) {
  const id = path.basename(folderPath);
  const arcadeJson = readJson(path.join(folderPath, 'arcade.json')) || {};
  const packageJson = readJson(path.join(folderPath, 'package.json')) || {};

  const hasIndex = fs.existsSync(path.join(folderPath, 'index.html'));
  const hasDistIndex = fs.existsSync(path.join(folderPath, 'dist', 'index.html'));
  const hasUnityLoader = fs.existsSync(path.join(folderPath, 'Build')) && fs.existsSync(path.join(folderPath, 'index.html'));
  const hasGodot = fs.readdirSync(folderPath).some((name) => name.endsWith('.wasm') || name.endsWith('.pck')) && hasIndex;
  const isViteProject = Boolean(packageJson.scripts?.build && packageJson.dependencies?.vite || packageJson.devDependencies?.vite || fs.existsSync(path.join(folderPath, 'vite.config.js')));

  let type = arcadeJson.type || 'browser';
  let playUrl = `/games/${id}/index.html`;
  let description = arcadeJson.description || 'Playable arcade game.';

  if (hasUnityLoader) type = arcadeJson.type || 'unity-webgl';
  if (hasGodot) type = arcadeJson.type || 'godot-html5';

  if (isViteProject) {
    type = arcadeJson.type || 'vite';
    if (!hasDistIndex) {
      console.log(`Building Vite game: ${id}`);
      const npmInstallNeeded = !fs.existsSync(path.join(folderPath, 'node_modules'));
      if (npmInstallNeeded) spawnSync('npm', ['install'], { cwd: folderPath, stdio: 'inherit' });
      spawnSync('npm', ['run', 'build'], { cwd: folderPath, stdio: 'inherit' });
    }
    if (fs.existsSync(path.join(folderPath, 'dist', 'index.html'))) {
      playUrl = `/games/${id}/dist/index.html`;
    }
  }

  if (!hasIndex && !hasDistIndex) return null;

  const thumbnailCandidates = ['thumbnail.png', 'thumbnail.jpg', 'cover.png', 'cover.jpg'];
  const thumbnail = thumbnailCandidates.find((name) => fs.existsSync(path.join(folderPath, name)));

  return {
    id,
    title: arcadeJson.title || packageJson.name || titleFromId(id),
    description,
    type,
    playUrl,
    thumbnail: thumbnail ? `/games/${id}/${thumbnail}` : null
  };
}

function prepare() {
  fs.mkdirSync(gamesDir, { recursive: true });
  const folders = fs.readdirSync(gamesDir, { withFileTypes: true }).filter((item) => item.isDirectory());
  const games = folders.map((item) => detectGame(path.join(gamesDir, item.name))).filter(Boolean);
  fs.writeFileSync(manifestPath, JSON.stringify({ generatedAt: new Date().toISOString(), games }, null, 2));
  console.log(`Detected ${games.length} game(s). Manifest updated.`);
}

prepare();

if (watch) {
  console.log('Watching public/games for changes...');
  fs.watch(gamesDir, { recursive: true }, () => {
    clearTimeout(globalThis.__arcadeTimer);
    globalThis.__arcadeTimer = setTimeout(prepare, 400);
  });
}
