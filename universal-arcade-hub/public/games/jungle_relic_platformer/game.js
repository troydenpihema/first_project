const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

const menu = document.getElementById("menu");
const gameOverScreen = document.getElementById("gameOver");
const startBtn = document.getElementById("startBtn");
const restartBtn = document.getElementById("restartBtn");
const fruitCountEl = document.getElementById("fruitCount");
const fruitTotalEl = document.getElementById("fruitTotal");
const keyCountEl = document.getElementById("keyCount");
const heartCountEl = document.getElementById("heartCount");
const levelCountEl = document.getElementById("levelCount");
const dialogue = document.getElementById("dialogue");
const dialogueTitle = document.getElementById("dialogueTitle");
const dialogueText = document.getElementById("dialogueText");
const endTitle = document.getElementById("endTitle");
const endText = document.getElementById("endText");

let W = 1180;
let H = 720;
let DPR = 1;
let last = 0;
let anim = null;
let running = false;
let cameraX = 0;
let levelIndex = 0;
let keysCollected = 0;
let fruitCollected = 0;
let invincible = 0;
let messageTimer = 0;
let shake = 0;
let particles = [];
let pressed = new Set();
let previousInteract = false;

const gravity = 2000;
const friction = 0.86;
const tile = 48;

const player = {
  x: 130,
  y: 260,
  w: 36,
  h: 54,
  vx: 0,
  vy: 0,
  facing: 1,
  grounded: false,
  hearts: 3,
  jumpLock: false,
  hurtLock: 0,
};

const levels = [
  {
    name: "Misty Mango Grove",
    width: 3200,
    spawn: { x: 130, y: 300 },
    fruitGoal: 10,
    platforms: [
      { x: 0, y: 630, w: 720, h: 90 },
      { x: 800, y: 610, w: 420, h: 110 },
      { x: 1320, y: 640, w: 520, h: 80 },
      { x: 1940, y: 610, w: 500, h: 110 },
      { x: 2520, y: 630, w: 680, h: 90 },
      { x: 360, y: 505, w: 180, h: 34 },
      { x: 700, y: 475, w: 190, h: 34 },
      { x: 1040, y: 500, w: 160, h: 34 },
      { x: 1510, y: 500, w: 190, h: 34 },
      { x: 2200, y: 490, w: 210, h: 34 },
      { x: 2640, y: 500, w: 170, h: 34 },
    ],
    movers: [
      { x: 1720, y: 470, w: 150, h: 28, baseX: 1720, range: 210, speed: 1.5, phase: 0 },
    ],
    fruits: [
      [410,455],[462,455],[745,425],[800,425],[1088,452],[1560,452],[1615,452],[2250,440],[2700,450],[2770,450],[2960,580]
    ],
    keys: [{ x: 1760, y: 420, taken: false }],
    enemies: [
      { x: 930, y: 565, w: 42, h: 36, vx: 70, min: 825, max: 1170, type: "hog" },
      { x: 2050, y: 565, w: 42, h: 36, vx: 80, min: 1970, max: 2380, type: "hog" },
      { x: 2750, y: 585, w: 36, h: 32, vx: 95, min: 2560, max: 3120, type: "bug" },
    ],
    signs: [
      { x: 220, y: 574, title: "Old Sign", text: "Collect fruit, find the golden key, then reach the relic gate. Jump on enemies to bounce off them." },
      { x: 1480, y: 584, title: "Explorer Kid", text: "That moving platform leads to the key. Time your jump and keep your momentum, bro." },
    ],
    gate: { x: 3040, y: 520, w: 80, h: 110, locked: true },
  },
  {
    name: "Sunken Fern Ruins",
    width: 3600,
    spawn: { x: 120, y: 330 },
    fruitGoal: 13,
    platforms: [
      { x: 0, y: 635, w: 600, h: 85 },
      { x: 730, y: 635, w: 430, h: 85 },
      { x: 1300, y: 625, w: 390, h: 95 },
      { x: 1840, y: 645, w: 380, h: 75 },
      { x: 2380, y: 625, w: 360, h: 95 },
      { x: 2920, y: 630, w: 680, h: 90 },
      { x: 350, y: 515, w: 160, h: 32 },
      { x: 820, y: 500, w: 150, h: 32 },
      { x: 1180, y: 475, w: 140, h: 32 },
      { x: 1510, y: 500, w: 140, h: 32 },
      { x: 2050, y: 500, w: 150, h: 32 },
      { x: 2540, y: 505, w: 150, h: 32 },
      { x: 3080, y: 500, w: 200, h: 32 },
    ],
    movers: [
      { x: 560, y: 500, w: 130, h: 28, baseX: 560, range: 190, speed: 1.7, phase: 1 },
      { x: 1680, y: 470, w: 130, h: 28, baseX: 1680, range: 240, speed: 1.4, phase: 0.5 },
      { x: 2740, y: 475, w: 130, h: 28, baseX: 2740, range: 200, speed: 1.8, phase: 2 },
    ],
    fruits: [[385,465],[440,465],[855,450],[1215,425],[1560,450],[1608,450],[2090,452],[2145,452],[2590,455],[3130,450],[3190,450],[3300,580],[3400,580],[3480,580]],
    keys: [{ x: 2800, y: 425, taken: false }],
    enemies: [
      { x: 790, y: 590, w: 42, h: 36, vx: 86, min: 750, max: 1115, type: "hog" },
      { x: 1390, y: 580, w: 42, h: 36, vx: 94, min: 1320, max: 1640, type: "hog" },
      { x: 2460, y: 580, w: 36, h: 32, vx: 115, min: 2400, max: 2700, type: "bug" },
      { x: 3150, y: 585, w: 42, h: 36, vx: 110, min: 2960, max: 3560, type: "hog" },
    ],
    signs: [
      { x: 160, y: 579, title: "Stone Tablet", text: "This ruin has wider gaps. Hold jump longer for a bigger leap." },
      { x: 3000, y: 584, title: "Relic Door", text: "The gate will only open when you have a key and enough fruit." },
    ],
    gate: { x: 3450, y: 520, w: 80, h: 110, locked: true },
  },
  {
    name: "Cloud Vine Summit",
    width: 3900,
    spawn: { x: 120, y: 330 },
    fruitGoal: 16,
    platforms: [
      { x: 0, y: 635, w: 520, h: 85 },
      { x: 690, y: 620, w: 330, h: 100 },
      { x: 1180, y: 650, w: 320, h: 70 },
      { x: 1660, y: 615, w: 310, h: 105 },
      { x: 2140, y: 640, w: 320, h: 80 },
      { x: 2660, y: 620, w: 340, h: 100 },
      { x: 3180, y: 635, w: 720, h: 85 },
      { x: 310, y: 495, w: 140, h: 30 },
      { x: 760, y: 490, w: 130, h: 30 },
      { x: 1250, y: 500, w: 130, h: 30 },
      { x: 1750, y: 500, w: 130, h: 30 },
      { x: 2260, y: 510, w: 130, h: 30 },
      { x: 2800, y: 495, w: 130, h: 30 },
      { x: 3300, y: 500, w: 170, h: 30 },
    ],
    movers: [
      { x: 520, y: 500, w: 120, h: 26, baseX: 520, range: 190, speed: 2.0, phase: 0 },
      { x: 1020, y: 475, w: 120, h: 26, baseX: 1020, range: 220, speed: 1.6, phase: 1.2 },
      { x: 1980, y: 475, w: 120, h: 26, baseX: 1980, range: 230, speed: 2.0, phase: 2.3 },
      { x: 2480, y: 475, w: 120, h: 26, baseX: 2480, range: 220, speed: 1.6, phase: 0.7 },
      { x: 3000, y: 470, w: 120, h: 26, baseX: 3000, range: 180, speed: 2.1, phase: 1.5 },
    ],
    fruits: [[345,445],[390,445],[790,440],[835,440],[1280,450],[1325,450],[1790,450],[1835,450],[2300,460],[2345,460],[2828,445],[2875,445],[3340,452],[3400,452],[3500,585],[3570,585],[3680,585]],
    keys: [{ x: 3060, y: 455, taken: false }],
    enemies: [
      { x: 760, y: 575, w: 36, h: 32, vx: 130, min: 710, max: 980, type: "bug" },
      { x: 1250, y: 605, w: 42, h: 36, vx: 120, min: 1200, max: 1460, type: "hog" },
      { x: 1720, y: 570, w: 42, h: 36, vx: 122, min: 1680, max: 1930, type: "hog" },
      { x: 2700, y: 575, w: 36, h: 32, vx: 140, min: 2680, max: 2980, type: "bug" },
      { x: 3350, y: 590, w: 42, h: 36, vx: 138, min: 3220, max: 3820, type: "hog" },
    ],
    signs: [
      { x: 130, y: 579, title: "Summit Warning", text: "Final area. Moving platforms are faster here. Take your time — rushing gets you smoked." },
      { x: 3420, y: 579, title: "Ancient Mask", text: "The relic is close. Bring the fruit and the final key to finish the adventure." },
    ],
    gate: { x: 3740, y: 525, w: 88, h: 105, locked: true },
  }
];

function resize() {
  const shell = document.getElementById("gameShell");
  const rect = shell.getBoundingClientRect();
  DPR = Math.min(window.devicePixelRatio || 1, 2);
  canvas.width = Math.floor(rect.width * DPR);
  canvas.height = Math.floor(rect.height * DPR);
  canvas.style.width = `${rect.width}px`;
  canvas.style.height = `${rect.height}px`;
  ctx.setTransform(DPR, 0, 0, DPR, 0, 0);
  W = rect.width;
  H = rect.height;
}
window.addEventListener("resize", resize);
resize();

function currentLevel() {
  return levels[levelIndex];
}

function totalFruit() {
  return currentLevel().fruits.length;
}

function startGame() {
  levelIndex = 0;
  keysCollected = 0;
  fruitCollected = 0;
  player.hearts = 3;
  menu.classList.add("hidden");
  gameOverScreen.classList.add("hidden");
  loadLevel(0);
  running = true;
  last = performance.now();
  if (anim) cancelAnimationFrame(anim);
  anim = requestAnimationFrame(loop);
}

function loadLevel(index) {
  levelIndex = index;
  const level = currentLevel();
  player.x = level.spawn.x;
  player.y = level.spawn.y;
  player.vx = 0;
  player.vy = 0;
  player.grounded = false;
  invincible = 1.2;
  cameraX = 0;
  particles = [];
  keysCollected = 0;
  fruitCollected = 0;
  level.keys.forEach(k => k.taken = false);
  level.fruits = level.fruits.map(f => Array.isArray(f) ? { x: f[0], y: f[1], taken: false, bob: Math.random() * 10 } : { ...f, taken: false });
  level.gate.locked = true;
  showMessage(level.name, "Find fruit, grab the key, and reach the relic gate.", 2.4);
  updateHud();
}

function updateHud() {
  fruitCountEl.textContent = fruitCollected;
  fruitTotalEl.textContent = currentLevel().fruitGoal;
  keyCountEl.textContent = keysCollected;
  heartCountEl.textContent = player.hearts;
  levelCountEl.textContent = levelIndex + 1;
}

function showMessage(title, text, seconds = 3) {
  dialogueTitle.textContent = title;
  dialogueText.textContent = text;
  dialogue.classList.remove("hidden");
  messageTimer = seconds;
}

function hideMessage() {
  dialogue.classList.add("hidden");
  messageTimer = 0;
}

function keyDown(name) {
  return pressed.has(name);
}

window.addEventListener("keydown", (e) => {
  pressed.add(e.key);
  if (["ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", " "].includes(e.key)) e.preventDefault();
  if (!running && (e.key === "Enter" || e.key === " ")) startGame();
});
window.addEventListener("keyup", (e) => pressed.delete(e.key));

document.querySelectorAll("#touchControls button").forEach(btn => {
  const k = btn.dataset.key;
  const down = e => { e.preventDefault(); pressed.add(k); };
  const up = e => { e.preventDefault(); pressed.delete(k); };
  btn.addEventListener("pointerdown", down);
  btn.addEventListener("pointerup", up);
  btn.addEventListener("pointercancel", up);
  btn.addEventListener("pointerleave", up);
});

startBtn.addEventListener("click", startGame);
restartBtn.addEventListener("click", startGame);

function loop(now) {
  const dt = Math.min(0.033, (now - last) / 1000 || 0.016);
  last = now;
  if (running) update(dt);
  draw();
  if (running) anim = requestAnimationFrame(loop);
}

function update(dt) {
  const level = currentLevel();
  const left = keyDown("ArrowLeft") || keyDown("a") || keyDown("A");
  const right = keyDown("ArrowRight") || keyDown("d") || keyDown("D");
  const jump = keyDown(" ") || keyDown("ArrowUp") || keyDown("w") || keyDown("W");
  const interact = keyDown("e") || keyDown("E");

  if (messageTimer > 0) {
    messageTimer -= dt;
    if (messageTimer <= 0) hideMessage();
  }

  if (invincible > 0) invincible -= dt;
  if (player.hurtLock > 0) player.hurtLock -= dt;
  if (shake > 0) shake -= dt;

  updateMovers(dt, level);

  const accel = player.grounded ? 2100 : 1450;
  const maxSpeed = 360;
  if (left) {
    player.vx -= accel * dt;
    player.facing = -1;
  }
  if (right) {
    player.vx += accel * dt;
    player.facing = 1;
  }
  if (!left && !right && player.grounded) player.vx *= Math.pow(friction, dt * 60);
  player.vx = clamp(player.vx, -maxSpeed, maxSpeed);

  if (jump && player.grounded && !player.jumpLock) {
    player.vy = -920;
    player.grounded = false;
    player.jumpLock = true;
    puff(player.x + player.w / 2, player.y + player.h, 8, "#fff1bb");
  }
  if (!jump) player.jumpLock = false;
  if (!jump && player.vy < -300) player.vy = -300;

  player.vy += gravity * dt;
  player.vy = Math.min(player.vy, 1200);

  moveAndCollide(player, dt, level);
  updateEnemies(dt, level);
  collectThings(level);
  handleSigns(level, interact, previousInteract);
  handleGate(level);
  updateParticles(dt);
  previousInteract = interact;

  if (player.y > H + 260) hurtPlayer(true);

  cameraX += ((player.x + player.w / 2) - W * 0.43 - cameraX) * Math.min(1, dt * 6);
  cameraX = clamp(cameraX, 0, Math.max(0, level.width - W));
  updateHud();
}

function updateMovers(dt, level) {
  const t = performance.now() / 1000;
  for (const m of level.movers) {
    const oldX = m.x;
    m.x = m.baseX + Math.sin(t * m.speed + m.phase) * m.range;
    m.dx = m.x - oldX;
  }
}

function moveAndCollide(obj, dt, level) {
  const solids = [...level.platforms, ...level.movers];
  obj.grounded = false;

  obj.x += obj.vx * dt;
  for (const p of solids) {
    if (hit(obj, p)) {
      if (obj.vx > 0) obj.x = p.x - obj.w;
      if (obj.vx < 0) obj.x = p.x + p.w;
      obj.vx = 0;
    }
  }

  obj.y += obj.vy * dt;
  for (const p of solids) {
    if (hit(obj, p)) {
      if (obj.vy > 0) {
        obj.y = p.y - obj.h;
        obj.vy = 0;
        obj.grounded = true;
        if (p.dx) obj.x += p.dx;
      } else if (obj.vy < 0) {
        obj.y = p.y + p.h;
        obj.vy = 0;
      }
    }
  }

  obj.x = clamp(obj.x, 0, level.width - obj.w);
}

function updateEnemies(dt, level) {
  for (const enemy of level.enemies) {
    enemy.x += enemy.vx * dt;
    if (enemy.x < enemy.min) {
      enemy.x = enemy.min;
      enemy.vx = Math.abs(enemy.vx);
    }
    if (enemy.x + enemy.w > enemy.max) {
      enemy.x = enemy.max - enemy.w;
      enemy.vx = -Math.abs(enemy.vx);
    }

    if (hit(player, enemy) && invincible <= 0) {
      const stomp = player.vy > 140 && player.y + player.h - enemy.y < 24;
      if (stomp) {
        player.vy = -700;
        enemy.deadTimer = 0.22;
        enemy.x = enemy.min - 9999;
        puff(player.x + player.w / 2, player.y + player.h, 12, "#ffe26c");
      } else {
        hurtPlayer(false);
      }
    }
  }
}

function collectThings(level) {
  for (const fruit of level.fruits) {
    if (!fruit.taken && hit(player, { x: fruit.x, y: fruit.y, w: 26, h: 26 })) {
      fruit.taken = true;
      fruitCollected++;
      puff(fruit.x + 13, fruit.y + 13, 10, "#ff6b4a");
    }
  }

  for (const key of level.keys) {
    if (!key.taken && hit(player, { x: key.x, y: key.y, w: 30, h: 34 })) {
      key.taken = true;
      keysCollected++;
      puff(key.x + 15, key.y + 16, 16, "#ffe26c");
      showMessage("Golden Key", "Nice. Now collect enough fruit and head to the relic gate.", 2.4);
    }
  }
}

function handleSigns(level, interact, wasInteract) {
  if (!interact || wasInteract) return;
  for (const sign of level.signs) {
    const near = Math.abs((player.x + player.w / 2) - sign.x) < 78 && Math.abs(player.y + player.h - sign.y) < 95;
    if (near) {
      showMessage(sign.title, sign.text, 4.2);
      return;
    }
  }
}

function handleGate(level) {
  const gate = level.gate;
  if (fruitCollected >= level.fruitGoal && keysCollected > 0) gate.locked = false;

  if (hit(player, gate)) {
    if (gate.locked) {
      player.x = gate.x - player.w - 4;
      player.vx = Math.min(0, player.vx);
      const needFruit = Math.max(0, level.fruitGoal - fruitCollected);
      const needKey = keysCollected <= 0 ? " and a golden key" : "";
      showMessage("Locked Gate", `You need ${needFruit} more fruit${needKey}.`, 2.2);
    } else {
      nextLevel();
    }
  }
}

function nextLevel() {
  if (levelIndex >= levels.length - 1) {
    running = false;
    endTitle.textContent = "Relic Found!";
    endText.textContent = "Deadly bro — you cleared all 3 stages and found the jungle relic.";
    gameOverScreen.classList.remove("hidden");
    return;
  }
  loadLevel(levelIndex + 1);
}

function hurtPlayer(fell) {
  if (invincible > 0 || player.hurtLock > 0) return;
  player.hearts--;
  invincible = 1.4;
  player.hurtLock = 0.3;
  shake = 0.25;
  player.vy = -560;
  player.vx = fell ? 0 : -player.facing * 420;
  puff(player.x + player.w / 2, player.y + player.h / 2, 18, "#ff5e5e");

  if (fell) {
    const s = currentLevel().spawn;
    player.x = s.x;
    player.y = s.y;
    player.vx = 0;
    player.vy = 0;
  }

  if (player.hearts <= 0) {
    running = false;
    endTitle.textContent = "Game Over";
    endText.textContent = `You reached Level ${levelIndex + 1}. Hit restart and run it back.`;
    gameOverScreen.classList.remove("hidden");
  }
}

function puff(x, y, amount, color) {
  for (let i = 0; i < amount; i++) {
    const a = Math.random() * Math.PI * 2;
    const s = 50 + Math.random() * 180;
    particles.push({
      x, y,
      vx: Math.cos(a) * s,
      vy: Math.sin(a) * s,
      life: 0.45 + Math.random() * 0.35,
      max: 0.8,
      size: 3 + Math.random() * 5,
      color,
    });
  }
}

function updateParticles(dt) {
  for (let i = particles.length - 1; i >= 0; i--) {
    const p = particles[i];
    p.x += p.vx * dt;
    p.y += p.vy * dt;
    p.vy += 800 * dt;
    p.life -= dt;
    if (p.life <= 0) particles.splice(i, 1);
  }
}

function draw() {
  ctx.save();
  ctx.clearRect(0, 0, W, H);
  if (shake > 0) ctx.translate((Math.random() - 0.5) * 10, (Math.random() - 0.5) * 8);
  drawSky();
  ctx.translate(-cameraX, 0);
  drawWorld();
  ctx.restore();
}

function drawSky() {
  const sky = ctx.createLinearGradient(0, 0, 0, H);
  sky.addColorStop(0, "#72d8ff");
  sky.addColorStop(0.58, "#bdf2ff");
  sky.addColorStop(1, "#ffe2a8");
  ctx.fillStyle = sky;
  ctx.fillRect(0, 0, W, H);

  drawCloud(120 - cameraX * 0.15, 105, 1.1);
  drawCloud(520 - cameraX * 0.11, 74, 0.75);
  drawCloud(910 - cameraX * 0.18, 128, 0.95);

  ctx.fillStyle = "rgba(65, 136, 93, 0.32)";
  for (let i = -2; i < 9; i++) {
    const x = i * 260 - (cameraX * 0.25 % 260);
    ctx.beginPath();
    ctx.moveTo(x, H);
    ctx.lineTo(x + 160, 250);
    ctx.lineTo(x + 330, H);
    ctx.fill();
  }
}

function drawCloud(x, y, s) {
  ctx.fillStyle = "rgba(255,255,255,0.86)";
  ctx.beginPath();
  ctx.arc(x, y, 34 * s, 0, Math.PI * 2);
  ctx.arc(x + 42 * s, y - 18 * s, 42 * s, 0, Math.PI * 2);
  ctx.arc(x + 88 * s, y, 34 * s, 0, Math.PI * 2);
  ctx.arc(x + 44 * s, y + 14 * s, 44 * s, 0, Math.PI * 2);
  ctx.fill();
}

function drawWorld() {
  const level = currentLevel();
  drawParallaxJungle(level.width);
  for (const p of level.platforms) drawPlatform(p, false);
  for (const m of level.movers) drawPlatform(m, true);
  for (const sign of level.signs) drawSign(sign);
  for (const fruit of level.fruits) if (!fruit.taken) drawFruit(fruit);
  for (const key of level.keys) if (!key.taken) drawKey(key);
  for (const enemy of level.enemies) if (enemy.x > -1000) drawEnemy(enemy);
  drawGate(level.gate);
  drawPlayer();
  drawParticles();
}

function drawParallaxJungle(worldW) {
  ctx.fillStyle = "rgba(27, 119, 70, 0.32)";
  for (let x = -80; x < worldW + 160; x += 150) {
    const h = 160 + Math.sin(x * 0.01) * 38;
    ctx.fillRect(x, H - h, 24, h);
    ctx.beginPath();
    ctx.ellipse(x + 12, H - h, 70, 38, 0, 0, Math.PI * 2);
    ctx.fill();
  }
}

function drawPlatform(p, moving) {
  ctx.fillStyle = moving ? "#8f6bff" : "#6d3c1e";
  roundRect(p.x, p.y, p.w, p.h, 10);
  ctx.fill();
  ctx.fillStyle = moving ? "#c9bcff" : "#35a852";
  roundRect(p.x, p.y, p.w, Math.min(18, p.h), 10);
  ctx.fill();
  ctx.fillStyle = "rgba(255,255,255,0.12)";
  for (let x = p.x + 14; x < p.x + p.w - 10; x += 42) {
    ctx.fillRect(x, p.y + 23, 18, 4);
  }
}

function drawFruit(fruit) {
  const bob = Math.sin(performance.now() / 280 + fruit.bob) * 4;
  const x = fruit.x;
  const y = fruit.y + bob;
  ctx.fillStyle = "#ff6948";
  ctx.beginPath();
  ctx.arc(x + 13, y + 14, 13, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "#ffb33e";
  ctx.beginPath();
  ctx.arc(x + 8, y + 10, 5, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "#2faa4d";
  ctx.beginPath();
  ctx.ellipse(x + 16, y + 2, 9, 5, -0.5, 0, Math.PI * 2);
  ctx.fill();
}

function drawKey(k) {
  const y = k.y + Math.sin(performance.now() / 250) * 5;
  ctx.strokeStyle = "#ffdd4a";
  ctx.lineWidth = 6;
  ctx.lineCap = "round";
  ctx.beginPath();
  ctx.arc(k.x + 10, y + 12, 9, 0, Math.PI * 2);
  ctx.moveTo(k.x + 19, y + 12);
  ctx.lineTo(k.x + 36, y + 12);
  ctx.moveTo(k.x + 30, y + 12);
  ctx.lineTo(k.x + 30, y + 22);
  ctx.stroke();
}

function drawEnemy(e) {
  ctx.save();
  ctx.translate(e.x + e.w / 2, e.y + e.h / 2);
  if (e.vx < 0) ctx.scale(-1, 1);
  ctx.translate(-e.w / 2, -e.h / 2);
  ctx.fillStyle = e.type === "bug" ? "#8c35d1" : "#d46a28";
  roundRect(0, 5, e.w, e.h - 5, 16);
  ctx.fill();
  ctx.fillStyle = "#fff";
  ctx.beginPath(); ctx.arc(e.w - 12, 15, 5, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = "#1b120b";
  ctx.beginPath(); ctx.arc(e.w - 10, 15, 2, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = "rgba(0,0,0,0.25)";
  ctx.fillRect(8, e.h - 2, 10, 5);
  ctx.fillRect(e.w - 18, e.h - 2, 10, 5);
  ctx.restore();
}

function drawSign(sign) {
  ctx.fillStyle = "#7d4a25";
  ctx.fillRect(sign.x - 5, sign.y, 10, 55);
  ctx.fillStyle = "#c9853d";
  roundRect(sign.x - 38, sign.y - 30, 76, 36, 7);
  ctx.fill();
  ctx.fillStyle = "#442714";
  ctx.font = "bold 18px Arial";
  ctx.textAlign = "center";
  ctx.fillText("E", sign.x, sign.y - 6);
}

function drawGate(g) {
  ctx.fillStyle = g.locked ? "#6d4b2e" : "#39b86a";
  roundRect(g.x, g.y, g.w, g.h, 16);
  ctx.fill();
  ctx.fillStyle = g.locked ? "#f0c14b" : "#b6ffcf";
  ctx.beginPath();
  ctx.arc(g.x + g.w / 2, g.y + g.h / 2, 14, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "rgba(0,0,0,0.18)";
  for (let x = g.x + 15; x < g.x + g.w; x += 22) ctx.fillRect(x, g.y + 10, 5, g.h - 20);
}

function drawPlayer() {
  const flash = invincible > 0 && Math.floor(performance.now() / 80) % 2 === 0;
  if (flash) return;

  ctx.save();
  ctx.translate(player.x + player.w / 2, player.y + player.h / 2);
  ctx.scale(player.facing, 1);
  ctx.translate(-player.w / 2, -player.h / 2);

  ctx.fillStyle = "rgba(0,0,0,0.18)";
  ctx.beginPath();
  ctx.ellipse(player.w / 2, player.h + 3, 21, 5, 0, 0, Math.PI * 2);
  ctx.fill();

  ctx.fillStyle = "#ffca72";
  ctx.beginPath();
  ctx.arc(player.w / 2, 17, 17, 0, Math.PI * 2);
  ctx.fill();

  ctx.fillStyle = "#ff5a3d";
  ctx.beginPath();
  ctx.moveTo(2, 16);
  ctx.quadraticCurveTo(player.w / 2, -12, player.w - 2, 16);
  ctx.quadraticCurveTo(player.w / 2, 5, 2, 16);
  ctx.fill();

  ctx.fillStyle = "#ffffff";
  ctx.beginPath(); ctx.arc(24, 16, 5, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = "#2b170c";
  ctx.beginPath(); ctx.arc(26, 16, 2, 0, Math.PI * 2); ctx.fill();

  ctx.fillStyle = "#2d8cff";
  roundRect(5, 32, player.w - 10, 22, 9);
  ctx.fill();
  ctx.fillStyle = "#20336e";
  ctx.fillRect(8, 51, 9, 13);
  ctx.fillRect(player.w - 17, 51, 9, 13);
  ctx.fillStyle = "#fff1bb";
  ctx.fillRect(2, 36, 8, 13);
  ctx.fillRect(player.w - 10, 36, 8, 13);
  ctx.restore();
}

function drawParticles() {
  for (const p of particles) {
    ctx.globalAlpha = Math.max(0, p.life / p.max);
    ctx.fillStyle = p.color;
    ctx.beginPath();
    ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1;
}

function hit(a, b) {
  return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
}

function clamp(v, min, max) {
  return Math.max(min, Math.min(max, v));
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

draw();
