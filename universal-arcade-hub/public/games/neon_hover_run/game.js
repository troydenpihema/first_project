import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js';

const canvas = document.getElementById('gameCanvas');
const scoreText = document.getElementById('scoreText');
const gemText = document.getElementById('gemText');
const shieldText = document.getElementById('shieldText');
const bestText = document.getElementById('bestText');
const menu = document.getElementById('menu');
const gameOverPanel = document.getElementById('gameOver');
const finalText = document.getElementById('finalText');
const startBtn = document.getElementById('startBtn');
const restartBtn = document.getElementById('restartBtn');

let width = 1;
let height = 1;
let running = false;
let gameOver = false;
let animationId = null;
let lastTime = 0;
let score = 0;
let gems = 0;
let shield = 100;
let speed = 34;
let distance = 0;
let shake = 0;
let best = Number(localStorage.getItem('neonHoverBest') || 0);

const keys = new Set();
const laneX = [-5.2, 0, 5.2];
let targetLane = 1;
let playerLane = 1;
let obstacleTimer = 0;
let gemTimer = 0;
let boostEnergy = 1;

const obstacles = [];
const gemObjects = [];
const particles = [];
const trackPieces = [];
const starField = [];

bestText.textContent = best;

const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: true });
renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

const scene = new THREE.Scene();
scene.fog = new THREE.FogExp2(0x050510, 0.035);

const camera = new THREE.PerspectiveCamera(62, 1, 0.1, 260);
camera.position.set(0, 8.4, 17.5);
camera.lookAt(0, 1.5, -20);

const hemi = new THREE.HemisphereLight(0x83faff, 0x120018, 2.4);
scene.add(hemi);

const keyLight = new THREE.DirectionalLight(0xffffff, 2.5);
keyLight.position.set(8, 12, 8);
keyLight.castShadow = true;
keyLight.shadow.mapSize.set(1024, 1024);
scene.add(keyLight);

const cyanLight = new THREE.PointLight(0x00f5ff, 7, 38);
cyanLight.position.set(-8, 5, 8);
scene.add(cyanLight);

const pinkLight = new THREE.PointLight(0xff2eea, 5, 36);
pinkLight.position.set(8, 4, -10);
scene.add(pinkLight);

const materials = {
  player: new THREE.MeshStandardMaterial({ color: 0xffffff, metalness: 0.65, roughness: 0.22, emissive: 0x102f3a, emissiveIntensity: 0.8 }),
  glass: new THREE.MeshStandardMaterial({ color: 0x71f8ff, metalness: 0.2, roughness: 0.08, emissive: 0x00d8ff, emissiveIntensity: 0.55 }),
  track: new THREE.MeshStandardMaterial({ color: 0x10142c, metalness: 0.55, roughness: 0.35 }),
  railCyan: new THREE.MeshStandardMaterial({ color: 0x00f5ff, emissive: 0x00f5ff, emissiveIntensity: 1.6 }),
  railPink: new THREE.MeshStandardMaterial({ color: 0xff2eea, emissive: 0xff2eea, emissiveIntensity: 1.6 }),
  obstacle: new THREE.MeshStandardMaterial({ color: 0xff184d, metalness: 0.45, roughness: 0.28, emissive: 0xff003c, emissiveIntensity: 1.2 }),
  gem: new THREE.MeshStandardMaterial({ color: 0xfff56b, metalness: 0.25, roughness: 0.15, emissive: 0xffe95b, emissiveIntensity: 1.6 }),
  particle: new THREE.MeshBasicMaterial({ color: 0xffffff, transparent: true, opacity: 1 })
};

const player = new THREE.Group();
scene.add(player);

const body = new THREE.Mesh(new THREE.BoxGeometry(2.3, 0.52, 3.1), materials.player);
body.castShadow = true;
body.position.y = 0.65;
player.add(body);

const nose = new THREE.Mesh(new THREE.ConeGeometry(0.95, 1.7, 4), materials.player);
nose.rotation.x = Math.PI / 2;
nose.rotation.z = Math.PI / 4;
nose.position.set(0, 0.68, -2.05);
nose.castShadow = true;
player.add(nose);

const cockpit = new THREE.Mesh(new THREE.SphereGeometry(0.62, 24, 12), materials.glass);
cockpit.scale.set(1, 0.42, 1.25);
cockpit.position.set(0, 1.03, -0.2);
cockpit.castShadow = true;
player.add(cockpit);

const wingGeo = new THREE.BoxGeometry(1.65, 0.16, 1.3);
const leftWing = new THREE.Mesh(wingGeo, materials.glass);
leftWing.position.set(-1.75, 0.56, 0.35);
leftWing.rotation.z = 0.12;
player.add(leftWing);
const rightWing = leftWing.clone();
rightWing.position.x = 1.75;
rightWing.rotation.z = -0.12;
player.add(rightWing);

const trailGeo = new THREE.ConeGeometry(0.28, 1.7, 18);
const leftTrail = new THREE.Mesh(trailGeo, materials.railCyan);
leftTrail.rotation.x = -Math.PI / 2;
leftTrail.position.set(-0.65, 0.58, 1.9);
player.add(leftTrail);
const rightTrail = leftTrail.clone();
rightTrail.position.x = 0.65;
player.add(rightTrail);

function createTrackPiece(z) {
  const group = new THREE.Group();
  group.position.z = z;

  const floor = new THREE.Mesh(new THREE.BoxGeometry(18, 0.35, 28), materials.track);
  floor.position.y = -0.18;
  floor.receiveShadow = true;
  group.add(floor);

  const railGeo = new THREE.BoxGeometry(0.18, 0.16, 28);
  const leftRail = new THREE.Mesh(railGeo, materials.railCyan);
  leftRail.position.set(-8.9, 0.12, 0);
  group.add(leftRail);
  const rightRail = new THREE.Mesh(railGeo, materials.railPink);
  rightRail.position.set(8.9, 0.12, 0);
  group.add(rightRail);

  const lineGeo = new THREE.BoxGeometry(0.06, 0.08, 3.2);
  for (let i = 0; i < 3; i++) {
    const line = new THREE.Mesh(lineGeo, i % 2 ? materials.railPink : materials.railCyan);
    line.position.set(laneX[i], 0.05, 0);
    group.add(line);
  }

  scene.add(group);
  trackPieces.push(group);
}

for (let i = 0; i < 10; i++) createTrackPiece(-i * 28);

function createStar() {
  const geo = new THREE.SphereGeometry(Math.random() * 0.045 + 0.015, 6, 6);
  const mat = new THREE.MeshBasicMaterial({ color: Math.random() > 0.5 ? 0x72f7ff : 0xff7cf0 });
  const star = new THREE.Mesh(geo, mat);
  star.position.set((Math.random() - 0.5) * 120, Math.random() * 42 + 4, -Math.random() * 220 - 15);
  scene.add(star);
  starField.push(star);
}

for (let i = 0; i < 260; i++) createStar();

function spawnObstacle() {
  const lane = Math.floor(Math.random() * 3);
  const group = new THREE.Group();
  group.userData = { lane, hit: false };
  group.position.set(laneX[lane], 1.0, -118);

  const pillarGeo = new THREE.BoxGeometry(2.9, 2.1 + Math.random() * 1.1, 1.2);
  const mesh = new THREE.Mesh(pillarGeo, materials.obstacle);
  mesh.castShadow = true;
  mesh.receiveShadow = true;
  group.add(mesh);

  const glow = new THREE.PointLight(0xff184d, 3.6, 10);
  glow.position.set(0, 0.4, 0);
  group.add(glow);

  scene.add(group);
  obstacles.push(group);
}

function spawnGem() {
  const lane = Math.floor(Math.random() * 3);
  const gem = new THREE.Group();
  gem.userData = { lane, taken: false };
  gem.position.set(laneX[lane], 1.35, -118);

  const mesh = new THREE.Mesh(new THREE.OctahedronGeometry(0.75, 0), materials.gem);
  mesh.castShadow = true;
  gem.add(mesh);

  const light = new THREE.PointLight(0xfff56b, 3.1, 10);
  gem.add(light);

  scene.add(gem);
  gemObjects.push(gem);
}

function addParticleBurst(x, y, z, color = 0x72f7ff, amount = 18) {
  for (let i = 0; i < amount; i++) {
    const mesh = new THREE.Mesh(new THREE.SphereGeometry(0.06 + Math.random() * 0.08, 8, 8), materials.particle.clone());
    mesh.material.color.setHex(color);
    mesh.position.set(x, y, z);
    mesh.userData = {
      vx: (Math.random() - 0.5) * 8,
      vy: Math.random() * 5,
      vz: (Math.random() - 0.5) * 8,
      life: 0.45 + Math.random() * 0.35,
      maxLife: 0.8
    };
    scene.add(mesh);
    particles.push(mesh);
  }
}

function resize() {
  const rect = canvas.parentElement.getBoundingClientRect();
  width = Math.max(1, rect.width);
  height = Math.max(1, rect.height);
  renderer.setSize(width, height, false);
  camera.aspect = width / height;
  camera.updateProjectionMatrix();
}
window.addEventListener('resize', resize);
resize();

function resetGame() {
  if (animationId) cancelAnimationFrame(animationId);
  animationId = null;

  for (const obstacle of obstacles) scene.remove(obstacle);
  for (const gem of gemObjects) scene.remove(gem);
  for (const p of particles) scene.remove(p);
  obstacles.length = 0;
  gemObjects.length = 0;
  particles.length = 0;

  running = true;
  gameOver = false;
  score = 0;
  gems = 0;
  shield = 100;
  speed = 34;
  distance = 0;
  shake = 0;
  targetLane = 1;
  playerLane = 1;
  player.position.set(0, 0, 2.5);
  obstacleTimer = 0.7;
  gemTimer = 1.1;
  boostEnergy = 1;

  menu.classList.add('hidden');
  gameOverPanel.classList.add('hidden');
  lastTime = performance.now();
  animationId = requestAnimationFrame(loop);
}

function endGame() {
  if (gameOver) return;
  running = false;
  gameOver = true;
  shake = 0.55;
  const finalScore = Math.floor(score);
  best = Math.max(best, finalScore);
  localStorage.setItem('neonHoverBest', best);
  bestText.textContent = best;
  finalText.textContent = `Score: ${finalScore} · Gems: ${gems} · Best: ${best}`;
  addParticleBurst(player.position.x, 1, player.position.z, 0xff184d, 46);
  setTimeout(() => gameOverPanel.classList.remove('hidden'), 600);
}

function updateInput(dt) {
  if (keys.has('ArrowLeft') || keys.has('a') || keys.has('A')) targetLane = Math.max(0, targetLane - 1);
  if (keys.has('ArrowRight') || keys.has('d') || keys.has('D')) targetLane = Math.min(2, targetLane + 1);

  // Prevent held key from lane-skipping too fast.
  if (targetLane !== playerLane) playerLane = targetLane;

  const boostHeld = keys.has('ArrowUp') || keys.has('w') || keys.has('W') || keys.has(' ');
  if (boostHeld && boostEnergy > 0.05) {
    speed += 26 * dt;
    boostEnergy = Math.max(0, boostEnergy - dt * 0.32);
    leftTrail.scale.setScalar(1.18 + Math.sin(distance * 0.2) * 0.08);
    rightTrail.scale.copy(leftTrail.scale);
  } else {
    boostEnergy = Math.min(1, boostEnergy + dt * 0.18);
    leftTrail.scale.setScalar(1);
    rightTrail.scale.setScalar(1);
  }
}

let laneCooldown = 0;
window.addEventListener('keydown', (e) => {
  if (['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', ' ', 'a', 'd', 'w', 's', 'A', 'D', 'W', 'S'].includes(e.key)) e.preventDefault();
  if ((e.key === 'Enter' || e.key === ' ') && !running) resetGame();
  if ((e.key === 'ArrowLeft' || e.key === 'a' || e.key === 'A') && laneCooldown <= 0) {
    targetLane = Math.max(0, targetLane - 1);
    laneCooldown = 0.16;
  }
  if ((e.key === 'ArrowRight' || e.key === 'd' || e.key === 'D') && laneCooldown <= 0) {
    targetLane = Math.min(2, targetLane + 1);
    laneCooldown = 0.16;
  }
  keys.add(e.key);
});
window.addEventListener('keyup', (e) => keys.delete(e.key));

function tick(dt) {
  laneCooldown = Math.max(0, laneCooldown - dt);
  updateInput(dt);

  speed += dt * 0.55;
  speed = Math.min(speed, 76);
  distance += speed * dt;
  score += dt * speed * 3.4;

  const targetX = laneX[targetLane];
  player.position.x += (targetX - player.position.x) * Math.min(1, dt * 12);
  player.rotation.z += ((targetX - player.position.x) * -0.055 - player.rotation.z) * Math.min(1, dt * 9);
  player.rotation.x = Math.sin(distance * 0.16) * 0.025;
  player.position.y = Math.sin(distance * 0.13) * 0.12;

  camera.position.x += (player.position.x * 0.22 - camera.position.x) * Math.min(1, dt * 4);
  camera.position.y = 8.4 + Math.sin(distance * 0.06) * 0.12;
  camera.lookAt(player.position.x * 0.12, 1.15, -21);

  obstacleTimer -= dt;
  gemTimer -= dt;
  const spawnRate = Math.max(0.45, 1.18 - score / 9000);
  if (obstacleTimer <= 0) {
    spawnObstacle();
    obstacleTimer = spawnRate + Math.random() * 0.22;
  }
  if (gemTimer <= 0) {
    spawnGem();
    gemTimer = 0.72 + Math.random() * 0.56;
  }

  for (const piece of trackPieces) {
    piece.position.z += speed * dt;
    if (piece.position.z > 20) piece.position.z -= 280;
  }

  for (const star of starField) {
    star.position.z += speed * dt * 0.45;
    if (star.position.z > 22) {
      star.position.z = -220;
      star.position.x = (Math.random() - 0.5) * 120;
      star.position.y = Math.random() * 42 + 4;
    }
  }

  for (let i = obstacles.length - 1; i >= 0; i--) {
    const obstacle = obstacles[i];
    obstacle.position.z += speed * dt;
    obstacle.rotation.y += dt * 1.8;

    if (obstacle.position.z > 16) {
      scene.remove(obstacle);
      obstacles.splice(i, 1);
      continue;
    }

    if (!obstacle.userData.hit && obstacle.position.z > 0.2 && obstacle.position.z < 4.7) {
      const laneMatch = obstacle.userData.lane === targetLane;
      if (laneMatch) {
        obstacle.userData.hit = true;
        shield -= 34;
        shake = 0.22;
        addParticleBurst(obstacle.position.x, 1.2, obstacle.position.z, 0xff184d, 24);
        if (shield <= 0) endGame();
      }
    }
  }

  for (let i = gemObjects.length - 1; i >= 0; i--) {
    const gem = gemObjects[i];
    gem.position.z += speed * dt;
    gem.rotation.y += dt * 3.2;
    gem.rotation.x += dt * 1.4;

    if (gem.position.z > 16) {
      scene.remove(gem);
      gemObjects.splice(i, 1);
      continue;
    }

    if (!gem.userData.taken && gem.position.z > 0.3 && gem.position.z < 4.7 && gem.userData.lane === targetLane) {
      gem.userData.taken = true;
      gems += 1;
      score += 280;
      shield = Math.min(100, shield + 6);
      addParticleBurst(gem.position.x, 1.35, gem.position.z, 0xfff56b, 18);
      scene.remove(gem);
      gemObjects.splice(i, 1);
    }
  }

  for (let i = particles.length - 1; i >= 0; i--) {
    const p = particles[i];
    p.position.x += p.userData.vx * dt;
    p.position.y += p.userData.vy * dt;
    p.position.z += p.userData.vz * dt + speed * dt * 0.5;
    p.userData.vy -= 5.8 * dt;
    p.userData.life -= dt;
    p.material.opacity = Math.max(0, p.userData.life / p.userData.maxLife);
    if (p.userData.life <= 0) {
      scene.remove(p);
      particles.splice(i, 1);
    }
  }

  cyanLight.intensity = 6.5 + Math.sin(distance * 0.1) * 1.5;
  pinkLight.intensity = 5.0 + Math.cos(distance * 0.08) * 1.3;

  if (shake > 0) shake = Math.max(0, shake - dt);

  scoreText.textContent = Math.floor(score);
  gemText.textContent = gems;
  shieldText.textContent = `${Math.max(0, Math.floor(shield))}%`;
}

function render() {
  const ox = shake ? (Math.random() - 0.5) * shake * 0.7 : 0;
  const oy = shake ? (Math.random() - 0.5) * shake * 0.45 : 0;
  const oldX = camera.position.x;
  const oldY = camera.position.y;
  camera.position.x += ox;
  camera.position.y += oy;
  renderer.render(scene, camera);
  camera.position.x = oldX;
  camera.position.y = oldY;
}

function loop(now) {
  const dt = Math.min(0.033, (now - lastTime) / 1000 || 0.016);
  lastTime = now;

  if (running) tick(dt);
  render();

  if (running || particles.length > 0 || gameOver) {
    animationId = requestAnimationFrame(loop);
  } else {
    animationId = null;
  }
}

startBtn.addEventListener('click', resetGame);
restartBtn.addEventListener('click', resetGame);

for (const btn of document.querySelectorAll('.touch-btn')) {
  const action = btn.dataset.action;
  const down = (e) => {
    e.preventDefault();
    if (action === 'left' && laneCooldown <= 0) {
      targetLane = Math.max(0, targetLane - 1);
      laneCooldown = 0.16;
    }
    if (action === 'right' && laneCooldown <= 0) {
      targetLane = Math.min(2, targetLane + 1);
      laneCooldown = 0.16;
    }
    if (action === 'boost') keys.add(' ');
  };
  const up = (e) => {
    e.preventDefault();
    if (action === 'boost') keys.delete(' ');
  };
  btn.addEventListener('pointerdown', down);
  btn.addEventListener('pointerup', up);
  btn.addEventListener('pointercancel', up);
  btn.addEventListener('pointerleave', up);
}

resize();
render();
