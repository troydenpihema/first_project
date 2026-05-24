const canvas = document.getElementById('game');
const ctx = canvas.getContext('2d');

const oreEl = document.getElementById('ore');
const hullEl = document.getElementById('hull');
const depthEl = document.getElementById('depth');
const bestEl = document.getElementById('best');
const startOverlay = document.getElementById('startOverlay');
const gameOverOverlay = document.getElementById('gameOverOverlay');
const resultText = document.getElementById('resultText');
const startBtn = document.getElementById('startBtn');
const restartBtn = document.getElementById('restartBtn');

let W = 1100;
let H = 720;
let dpr = 1;
let scale = 1;
let running = false;
let animationId = null;
let lastTime = 0;
let shake = 0;

let ore = 0;
let depth = 0;
let hull = 100;
let scrollSpeed = 210;
let rockTimer = 0;
let oreTimer = 0;
let sparkTimer = 0;
let best = Number(localStorage.getItem('meteorMinerBest') || 0);

const keys = new Set();
const rocks = [];
const ores = [];
const sparks = [];
const stars = [];

const pod = {
  x: 0,
  y: 0,
  w: 54,
  h: 72,
  vx: 0,
  drillHeat: 0,
};

function resize() {
  const rect = canvas.parentElement.getBoundingClientRect();
  dpr = Math.min(window.devicePixelRatio || 1, 2);
  canvas.width = Math.floor(rect.width * dpr);
  canvas.height = Math.floor(rect.height * dpr);
  canvas.style.width = `${rect.width}px`;
  canvas.style.height = `${rect.height}px`;
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  W = rect.width;
  H = rect.height;
  scale = Math.min(W / 1100, H / 720);
  createStars();
}

function createStars() {
  stars.length = 0;
  for (let i = 0; i < 90; i++) {
    stars.push({
      x: Math.random() * W,
      y: Math.random() * H,
      r: Math.random() * 1.8 + 0.4,
      s: Math.random() * 65 + 18,
      a: Math.random() * 0.55 + 0.2,
    });
  }
}

function resetGame() {
  if (animationId) cancelAnimationFrame(animationId);
  animationId = null;

  running = true;
  ore = 0;
  depth = 0;
  hull = 100;
  scrollSpeed = 210;
  rockTimer = 0.4;
  oreTimer = 0.7;
  sparkTimer = 0;
  shake = 0;
  rocks.length = 0;
  ores.length = 0;
  sparks.length = 0;

  pod.w = 54 * scale;
  pod.h = 72 * scale;
  pod.x = W / 2 - pod.w / 2;
  pod.y = H - pod.h - 66 * scale;
  pod.vx = 0;
  pod.drillHeat = 0;

  startOverlay.classList.add('hidden');
  gameOverOverlay.classList.add('hidden');
  lastTime = performance.now();
  animationId = requestAnimationFrame(loop);
}

function endGame() {
  if (!running) return;
  running = false;
  shake = 0.65;
  burst(pod.x + pod.w / 2, pod.y + pod.h / 2, 44, '#ffd76d');

  const finalScore = Math.floor(ore + depth / 10);
  if (finalScore > best) {
    best = finalScore;
    localStorage.setItem('meteorMinerBest', best);
  }

  resultText.textContent = `You mined ${ore} ore and reached ${Math.floor(depth)}m. Score: ${finalScore}. Best: ${best}.`;
  setTimeout(() => gameOverOverlay.classList.remove('hidden'), 450);
}

function rand(min, max) {
  return Math.random() * (max - min) + min;
}

function spawnRock() {
  const size = rand(34, 82) * scale;
  rocks.push({
    x: rand(20 * scale, W - size - 20 * scale),
    y: -size - 30,
    w: size,
    h: size,
    speed: scrollSpeed * rand(0.78, 1.18),
    rot: Math.random() * Math.PI * 2,
    spin: rand(-1.8, 1.8),
    hit: false,
  });
}

function spawnOre() {
  const size = rand(26, 36) * scale;
  ores.push({
    x: rand(28 * scale, W - size - 28 * scale),
    y: -size - 20,
    w: size,
    h: size,
    speed: scrollSpeed * rand(0.8, 1.05),
    pulse: Math.random() * Math.PI * 2,
  });
}

function burst(x, y, amount, color) {
  for (let i = 0; i < amount; i++) {
    const a = Math.random() * Math.PI * 2;
    const s = rand(40, 240) * scale;
    sparks.push({
      x,
      y,
      vx: Math.cos(a) * s,
      vy: Math.sin(a) * s,
      life: rand(0.28, 0.72),
      max: 0.72,
      size: rand(2, 5) * scale,
      color,
    });
  }
}

function rectsHit(a, b) {
  return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
}

function update(dt) {
  const left = keys.has('ArrowLeft') || keys.has('a') || keys.has('A');
  const right = keys.has('ArrowRight') || keys.has('d') || keys.has('D');
  const drilling = keys.has(' ') || keys.has('Space') || keys.has('Spacebar');

  scrollSpeed += dt * 7.5;
  depth += dt * scrollSpeed * 0.16;

  const accel = 1450 * scale;
  const maxV = 500 * scale;
  if (left) pod.vx -= accel * dt;
  if (right) pod.vx += accel * dt;
  if (!left && !right) pod.vx *= Math.pow(0.001, dt);
  pod.vx = Math.max(-maxV, Math.min(maxV, pod.vx));
  pod.x += pod.vx * dt;
  pod.x = Math.max(18 * scale, Math.min(W - pod.w - 18 * scale, pod.x));

  pod.drillHeat = drilling ? Math.min(1, pod.drillHeat + dt * 4) : Math.max(0, pod.drillHeat - dt * 2.8);

  rockTimer -= dt;
  oreTimer -= dt;
  sparkTimer -= dt;

  const rockEvery = Math.max(0.34, 0.95 - depth / 1800);
  if (rockTimer <= 0) {
    spawnRock();
    rockTimer = rockEvery;
  }

  if (oreTimer <= 0) {
    spawnOre();
    oreTimer = rand(0.55, 1.05);
  }

  if (drilling && sparkTimer <= 0) {
    burst(pod.x + pod.w / 2, pod.y + pod.h + 7 * scale, 3, '#ffd76d');
    sparkTimer = 0.045;
  }

  for (let i = rocks.length - 1; i >= 0; i--) {
    const rock = rocks[i];
    rock.y += rock.speed * dt;
    rock.rot += rock.spin * dt;
    if (rock.y > H + 120) rocks.splice(i, 1);

    if (rectsHit(pod, rock)) {
      if (drilling && pod.drillHeat > 0.35 && pod.y + pod.h * 0.45 < rock.y + rock.h) {
        ore += 4;
        burst(rock.x + rock.w / 2, rock.y + rock.h / 2, 20, '#ff8a4d');
        rocks.splice(i, 1);
      } else if (!rock.hit) {
        rock.hit = true;
        hull -= Math.floor(rand(18, 28));
        shake = 0.25;
        burst(pod.x + pod.w / 2, pod.y + pod.h / 2, 18, '#ff4f4f');
        rocks.splice(i, 1);
        if (hull <= 0) endGame();
      }
    }
  }

  for (let i = ores.length - 1; i >= 0; i--) {
    const item = ores[i];
    item.y += item.speed * dt;
    item.pulse += dt * 5;
    if (item.y > H + 80) ores.splice(i, 1);

    if (rectsHit(pod, item)) {
      ore += 10;
      burst(item.x + item.w / 2, item.y + item.h / 2, 16, '#70f6ff');
      ores.splice(i, 1);
    }
  }

  for (let i = sparks.length - 1; i >= 0; i--) {
    const p = sparks[i];
    p.x += p.vx * dt;
    p.y += p.vy * dt;
    p.vx *= Math.pow(0.08, dt);
    p.vy *= Math.pow(0.08, dt);
    p.life -= dt;
    if (p.life <= 0) sparks.splice(i, 1);
  }

  if (shake > 0) shake -= dt;

  oreEl.textContent = ore;
  hullEl.textContent = Math.max(0, hull);
  depthEl.textContent = Math.floor(depth);
  bestEl.textContent = best;
}

function drawBackground(dt) {
  const gradient = ctx.createLinearGradient(0, 0, 0, H);
  gradient.addColorStop(0, '#141330');
  gradient.addColorStop(0.55, '#0b0920');
  gradient.addColorStop(1, '#05040d');
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, W, H);

  for (const star of stars) {
    star.y += star.s * dt;
    if (star.y > H + 4) {
      star.y = -4;
      star.x = Math.random() * W;
    }
    ctx.globalAlpha = star.a;
    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.arc(star.x, star.y, star.r * scale, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1;

  ctx.strokeStyle = 'rgba(255, 215, 109, 0.08)';
  ctx.lineWidth = 2 * scale;
  const gap = 86 * scale;
  const offset = (depth * 1.2) % gap;
  for (let y = -gap + offset; y < H + gap; y += gap) {
    ctx.beginPath();
    ctx.moveTo(0, y);
    ctx.lineTo(W, y + 40 * scale);
    ctx.stroke();
  }
}

function drawPod() {
  const cx = pod.x + pod.w / 2;
  const drillOn = pod.drillHeat > 0.05;

  ctx.save();
  ctx.shadowColor = drillOn ? '#ffd76d' : '#70f6ff';
  ctx.shadowBlur = drillOn ? 24 : 16;

  roundRect(pod.x, pod.y, pod.w, pod.h, 16 * scale);
  ctx.fillStyle = '#e9f7ff';
  ctx.fill();

  roundRect(pod.x + pod.w * 0.19, pod.y + pod.h * 0.13, pod.w * 0.62, pod.h * 0.27, 12 * scale);
  ctx.fillStyle = '#11172d';
  ctx.fill();

  ctx.fillStyle = '#70f6ff';
  ctx.globalAlpha = 0.9;
  ctx.beginPath();
  ctx.arc(cx, pod.y + pod.h * 0.27, 8 * scale, 0, Math.PI * 2);
  ctx.fill();
  ctx.globalAlpha = 1;

  ctx.fillStyle = drillOn ? '#ffd76d' : '#7d7a86';
  ctx.beginPath();
  ctx.moveTo(cx, pod.y + pod.h + 23 * scale);
  ctx.lineTo(cx - 15 * scale, pod.y + pod.h - 2 * scale);
  ctx.lineTo(cx + 15 * scale, pod.y + pod.h - 2 * scale);
  ctx.closePath();
  ctx.fill();

  if (drillOn) {
    ctx.shadowColor = '#ffd76d';
    ctx.shadowBlur = 26;
    ctx.strokeStyle = 'rgba(255, 215, 109, 0.75)';
    ctx.lineWidth = 4 * scale;
    ctx.beginPath();
    ctx.moveTo(cx, pod.y + pod.h + 18 * scale);
    ctx.lineTo(cx + Math.sin(performance.now() / 45) * 8 * scale, pod.y + pod.h + 54 * scale);
    ctx.stroke();
  }

  ctx.restore();
}

function drawRock(rock) {
  ctx.save();
  ctx.translate(rock.x + rock.w / 2, rock.y + rock.h / 2);
  ctx.rotate(rock.rot);
  ctx.shadowColor = '#ff7a4d';
  ctx.shadowBlur = 12;
  ctx.fillStyle = '#6c5360';
  ctx.beginPath();
  const points = 9;
  for (let i = 0; i < points; i++) {
    const a = (Math.PI * 2 / points) * i;
    const r = rock.w * (0.38 + (i % 2) * 0.12);
    const x = Math.cos(a) * r;
    const y = Math.sin(a) * r;
    if (i === 0) ctx.moveTo(x, y);
    else ctx.lineTo(x, y);
  }
  ctx.closePath();
  ctx.fill();
  ctx.fillStyle = 'rgba(255,255,255,0.13)';
  ctx.beginPath();
  ctx.arc(-rock.w * 0.09, -rock.h * 0.11, rock.w * 0.08, 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();
}

function drawOre(item) {
  const glow = 0.6 + Math.sin(item.pulse) * 0.25;
  ctx.save();
  ctx.shadowColor = '#70f6ff';
  ctx.shadowBlur = 26 * glow;
  ctx.fillStyle = '#70f6ff';
  ctx.beginPath();
  ctx.moveTo(item.x + item.w / 2, item.y);
  ctx.lineTo(item.x + item.w, item.y + item.h * 0.38);
  ctx.lineTo(item.x + item.w * 0.74, item.y + item.h);
  ctx.lineTo(item.x + item.w * 0.26, item.y + item.h);
  ctx.lineTo(item.x, item.y + item.h * 0.38);
  ctx.closePath();
  ctx.fill();
  ctx.restore();
}

function drawSparks() {
  for (const p of sparks) {
    const alpha = Math.max(0, p.life / p.max);
    ctx.globalAlpha = alpha;
    ctx.shadowColor = p.color;
    ctx.shadowBlur = 14;
    ctx.fillStyle = p.color;
    ctx.beginPath();
    ctx.arc(p.x, p.y, p.size * alpha, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1;
  ctx.shadowBlur = 0;
}

function draw(dt) {
  ctx.save();
  if (shake > 0) ctx.translate(rand(-10, 10) * shake, rand(-10, 10) * shake);
  drawBackground(dt);
  ores.forEach(drawOre);
  rocks.forEach(drawRock);
  drawPod();
  drawSparks();
  ctx.restore();
}

function loop(now) {
  const dt = Math.min(0.033, (now - lastTime) / 1000 || 0.016);
  lastTime = now;
  if (running) update(dt);
  draw(dt);
  if (running || shake > 0 || sparks.length) {
    animationId = requestAnimationFrame(loop);
  } else {
    animationId = null;
  }
}

function roundRect(x, y, w, h, r) {
  r = Math.min(r, w / 2, h / 2);
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

window.addEventListener('resize', resize);
window.addEventListener('keydown', (e) => {
  keys.add(e.key);
  if ((e.key === 'Enter' || e.key === ' ') && !running && !startOverlay.classList.contains('hidden')) {
    resetGame();
  }
});
window.addEventListener('keyup', (e) => keys.delete(e.key));

for (const btn of document.querySelectorAll('[data-key]')) {
  const key = btn.dataset.key;
  const press = (e) => {
    e.preventDefault();
    keys.add(key);
  };
  const release = (e) => {
    e.preventDefault();
    keys.delete(key);
  };
  btn.addEventListener('pointerdown', press);
  btn.addEventListener('pointerup', release);
  btn.addEventListener('pointercancel', release);
  btn.addEventListener('pointerleave', release);
}

startBtn.addEventListener('click', resetGame);
restartBtn.addEventListener('click', resetGame);

resize();
bestEl.textContent = best;
draw(0.016);
