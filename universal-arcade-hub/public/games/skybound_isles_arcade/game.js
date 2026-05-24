(() => {
  const canvas = document.getElementById('gameCanvas');
  const ctx = canvas.getContext('2d');
  const menu = document.getElementById('menu');
  const gameOver = document.getElementById('gameOver');
  const playBtn = document.getElementById('playBtn');
  const retryBtn = document.getElementById('retryBtn');
  const crystalEl = document.getElementById('crystalCount');
  const healthEl = document.getElementById('healthCount');
  const stageEl = document.getElementById('stageName');
  const toast = document.getElementById('toast');
  const gameOverTitle = document.getElementById('gameOverTitle');
  const gameOverText = document.getElementById('gameOverText');

  const keys = new Set();
  const pressed = new Set();

  let viewW = 1280;
  let viewH = 760;
  let dpr = 1;
  let raf = 0;
  let last = 0;
  let running = false;
  let stageIndex = 0;
  let cameraX = 0;
  let cameraY = 0;
  let shake = 0;
  let toastTimer = 0;

  const gravity = 2500;

  const player = {
    x: 120, y: 0, w: 34, h: 50,
    vx: 0, vy: 0,
    facing: 1,
    grounded: false,
    coyote: 0,
    jumpBuffer: 0,
    dashCooldown: 0,
    dashTimer: 0,
    invuln: 0,
    health: 3,
    crystals: 0,
    checkpoint: { x: 120, y: 0 },
    anim: 0
  };

  let platforms = [];
  let crystals = [];
  let enemies = [];
  let hazards = [];
  let checkpoints = [];
  let portal = null;
  let particles = [];
  let clouds = [];
  let fireflies = [];

  const stages = [
    {
      name: 'I', title: 'The Broken Coast', width: 3050, spawn: { x: 110, y: 500 },
      message: 'Reach the glowing beacon. Dash lets you cross wider gaps.',
      platforms: [
        [0,650,460,90], [540,590,260,42], [890,535,250,42], [1210,585,310,42],
        [1590,515,250,42], [1905,585,290,42], [2260,520,270,42], [2630,620,470,90],
        [720,430,150,32], [1440,380,160,32], [2180,390,160,32]
      ],
      crystals: [[615,540], [960,485], [785,380], [1300,535], [1505,330], [1685,465], [2025,535], [2250,340], [2380,470], [2780,560]],
      enemies: [[1050,495,160], [1990,545,170], [2360,480,150]],
      hazards: [[470,690,65,90], [820,690,65,90], [1510,690,70,90], [2550,690,65,90]],
      checkpoints: [[1390,525]], portal: [2870,540]
    },
    {
      name: 'II', title: 'Vineglass Ruins', width: 3550, spawn: { x: 90, y: 480 },
      message: 'Stage two adds moving lifts. Time your jumps and keep momentum.',
      platforms: [
        [0,640,370,90], [465,570,210,42], [760,505,230,42], [1110,575,250,42],
        [1460,510,250,42], [1840,600,300,42], [2240,530,245,42], [2580,455,230,42],
        [2900,565,260,42], [3260,640,430,90], [1220,395,130,32], [2360,350,135,32]
      ],
      moving: [[965,435,150,32,80,0,1.7], [1665,430,150,32,0,85,1.9], [2730,390,150,32,100,0,2.1]],
      crystals: [[530,520], [825,455], [1010,380], [1190,525], [1265,345], [1535,460], [1910,550], [2300,480], [2405,300], [2660,405], [2965,515], [3350,585]],
      enemies: [[570,530,95], [1195,535,140], [1920,560,165], [2970,525,160]],
      hazards: [[380,690,75,90], [680,690,70,90], [1370,690,80,90], [2155,690,80,90], [2820,690,75,90]],
      checkpoints: [[1800,540]], portal: [3400,560]
    },
    {
      name: 'III', title: 'The Storm Lighthouse', width: 3900, spawn: { x: 90, y: 500 },
      message: 'Final stage. Use stomp, dash, and checkpoints to reach the lighthouse.',
      platforms: [
        [0,650,390,90], [500,580,220,42], [850,515,210,42], [1165,455,210,42],
        [1500,540,245,42], [1840,455,240,42], [2180,570,245,42], [2545,500,225,42],
        [2860,420,215,42], [3195,550,260,42], [3550,635,500,90], [1350,330,130,32], [3050,300,140,32]
      ],
      moving: [[745,420,145,32,0,90,1.55], [1760,350,145,32,110,0,1.85], [2375,420,145,32,0,95,1.65], [3310,420,150,32,95,0,1.9]],
      crystals: [[560,530], [905,465], [1215,405], [1390,280], [1580,490], [1900,405], [2260,520], [2435,370], [2610,450], [2930,370], [3095,250], [3280,500], [3650,585]],
      enemies: [[585,540,130], [1010,475,125], [1570,500,135], [2250,530,130], [2930,380,120], [3290,510,120]],
      hazards: [[395,690,90,90], [735,690,90,90], [1400,690,95,90], [2070,690,95,90], [2470,690,95,90], [3110,690,95,90], [3470,690,80,90]],
      checkpoints: [[1710,395], [3150,490]], portal: [3770,555]
    }
  ];

  function resize() {
    const rect = canvas.parentElement.getBoundingClientRect();
    dpr = Math.min(devicePixelRatio || 1, 2);
    viewW = Math.max(320, rect.width);
    viewH = Math.max(240, rect.height);
    canvas.width = Math.floor(viewW * dpr);
    canvas.height = Math.floor(viewH * dpr);
    canvas.style.width = viewW + 'px';
    canvas.style.height = viewH + 'px';
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  addEventListener('resize', resize);
  resize();

  function loadStage(i) {
    stageIndex = i;
    const s = stages[stageIndex];
    platforms = s.platforms.map(p => ({ x:p[0], y:p[1], w:p[2], h:p[3], baseX:p[0], baseY:p[1], moveX:0, moveY:0, speed:0, phase:Math.random()*10 }));
    if (s.moving) {
      for (const p of s.moving) platforms.push({ x:p[0], y:p[1], w:p[2], h:p[3], baseX:p[0], baseY:p[1], moveX:p[4], moveY:p[5], speed:p[6], phase:Math.random()*10, moving:true });
    }
    crystals = s.crystals.map(c => ({ x:c[0], y:c[1], r:12, taken:false, bob:Math.random()*10 }));
    enemies = s.enemies.map(e => ({ x:e[0], y:e[1], w:38, h:36, baseX:e[0], range:e[2], vx:70, alive:true, squish:0 }));
    hazards = s.hazards.map(h => ({ x:h[0], y:h[1], w:h[2], h:h[3] }));
    checkpoints = s.checkpoints.map(c => ({ x:c[0], y:c[1], w:34, h:70, active:false, glow:0 }));
    portal = { x:s.portal[0], y:s.portal[1], w:58, h:96, open:false, pulse:0 };
    clouds = Array.from({ length: 18 }, (_, n) => ({ x: (n*260 + Math.random()*200) % s.width, y: 40 + Math.random()*260, s: 0.45 + Math.random()*1.2, speed: 8 + Math.random()*18 }));
    fireflies = Array.from({ length: 48 }, () => ({ x: Math.random()*s.width, y: 120+Math.random()*520, a: Math.random()*6.28, s: Math.random()*1.8+0.8 }));
    particles = [];
    player.x = s.spawn.x;
    player.y = s.spawn.y;
    player.vx = 0;
    player.vy = 0;
    player.health = 3;
    player.checkpoint = { x: s.spawn.x, y: s.spawn.y };
    cameraX = 0;
    cameraY = 0;
    showToast(s.title + ' — ' + s.message, 4.2);
    updateHud();
  }

  function startGame() {
    menu.classList.add('hidden');
    gameOver.classList.add('hidden');
    player.crystals = 0;
    loadStage(0);
    running = true;
    cancelAnimationFrame(raf);
    last = performance.now();
    raf = requestAnimationFrame(loop);
  }

  playBtn.addEventListener('click', startGame);
  retryBtn.addEventListener('click', startGame);

  window.addEventListener('keydown', e => {
    if (['ArrowLeft','ArrowRight','ArrowUp','ArrowDown','Space'].includes(e.code) || ['ArrowLeft','ArrowRight'].includes(e.key)) e.preventDefault();
    keys.add(e.code);
    keys.add(e.key);
    pressed.add(e.code);
    if (!running && (e.code === 'Enter' || e.code === 'Space')) startGame();
  });

  window.addEventListener('keyup', e => {
    keys.delete(e.code);
    keys.delete(e.key);
  });

  document.querySelectorAll('.touch').forEach(btn => {
    const code = btn.dataset.key;
    const down = e => { e.preventDefault(); keys.add(code); pressed.add(code); };
    const up = e => { e.preventDefault(); keys.delete(code); };
    btn.addEventListener('pointerdown', down);
    btn.addEventListener('pointerup', up);
    btn.addEventListener('pointercancel', up);
    btn.addEventListener('pointerleave', up);
  });

  function down(...codes) { return codes.some(c => keys.has(c)); }
  function wasPressed(...codes) { return codes.some(c => pressed.has(c)); }

  function showToast(text, time=2.4) {
    toast.textContent = text;
    toast.classList.remove('hidden');
    toastTimer = time;
  }

  function updateHud() {
    crystalEl.textContent = player.crystals;
    healthEl.textContent = '♥'.repeat(Math.max(0, player.health));
    stageEl.textContent = stages[stageIndex].name;
  }

  function spawnParticle(x,y,opts={}) {
    const count = opts.count || 1;
    for (let i=0;i<count;i++) {
      const a = opts.angle ?? Math.random()*Math.PI*2;
      const sp = (opts.speed || 140) * (0.35 + Math.random()*0.8);
      particles.push({
        x, y,
        vx: Math.cos(a)*sp + (opts.vx||0),
        vy: Math.sin(a)*sp + (opts.vy||0),
        life: opts.life || 0.55,
        max: opts.life || 0.55,
        size: (opts.size || 4) * (0.6 + Math.random()*0.8),
        color: opts.color || '#b9f6ff',
        gravity: opts.gravity ?? 260
      });
    }
  }

  function rects(a,b) {
    return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
  }

  function update(dt, time) {
    const s = stages[stageIndex];
    const move = (down('ArrowLeft','KeyA','a') ? -1 : 0) + (down('ArrowRight','KeyD','d') ? 1 : 0);
    const jump = wasPressed('Space','ArrowUp','KeyW','w');
    const dash = wasPressed('KeyX','ShiftLeft','ShiftRight');

    for (const p of platforms) {
      if (p.moving) {
        p.prevX = p.x; p.prevY = p.y;
        p.x = p.baseX + Math.sin(time*p.speed + p.phase) * p.moveX;
        p.y = p.baseY + Math.sin(time*p.speed + p.phase) * p.moveY;
      } else {
        p.prevX = p.x; p.prevY = p.y;
      }
    }

    player.anim += dt;
    player.jumpBuffer = jump ? 0.13 : Math.max(0, player.jumpBuffer - dt);
    player.coyote = player.grounded ? 0.12 : Math.max(0, player.coyote - dt);
    player.dashCooldown = Math.max(0, player.dashCooldown - dt);
    player.invuln = Math.max(0, player.invuln - dt);
    player.dashTimer = Math.max(0, player.dashTimer - dt);

    if (move !== 0) player.facing = move;
    const accel = player.grounded ? 3900 : 2600;
    const maxSpeed = player.grounded ? 365 : 330;
    player.vx += move * accel * dt;
    if (move === 0) player.vx *= Math.pow(player.grounded ? 0.0008 : 0.08, dt);
    player.vx = clamp(player.vx, -maxSpeed, maxSpeed);

    if (player.jumpBuffer > 0 && player.coyote > 0) {
      player.vy = -840;
      player.grounded = false;
      player.jumpBuffer = 0;
      player.coyote = 0;
      spawnParticle(player.x + player.w/2, player.y + player.h, { count: 10, color:'#dff9ff', vy: 60, speed: 90, life: 0.38, size: 3 });
    }

    if (dash && player.dashCooldown <= 0) {
      player.dashTimer = 0.16;
      player.dashCooldown = 0.72;
      player.vx = player.facing * 760;
      player.vy *= 0.28;
      shake = Math.max(shake, 0.1);
      spawnParticle(player.x + player.w/2, player.y + player.h/2, { count: 18, color:'#8af3ff', vx:-player.facing*80, speed: 220, life: 0.34, size: 3 });
    }

    if (player.dashTimer <= 0) player.vy += gravity * dt;
    else player.vy += gravity * 0.12 * dt;

    const prevX = player.x, prevY = player.y;
    player.x += player.vx * dt;
    player.y += player.vy * dt;
    player.grounded = false;

    for (const p of platforms) {
      if (!rects(player, p)) continue;
      const oldBottom = prevY + player.h;
      const oldTop = prevY;
      const oldRight = prevX + player.w;
      const oldLeft = prevX;
      if (oldBottom <= p.y + 10 && player.vy >= 0) {
        player.y = p.y - player.h;
        player.vy = 0;
        player.grounded = true;
        if (p.moving) player.x += (p.x - p.prevX);
      } else if (oldTop >= p.y + p.h - 4 && player.vy < 0) {
        player.y = p.y + p.h;
        player.vy = 40;
      } else if (oldRight <= p.x) {
        player.x = p.x - player.w;
        player.vx = Math.min(0, player.vx);
      } else if (oldLeft >= p.x + p.w) {
        player.x = p.x + p.w;
        player.vx = Math.max(0, player.vx);
      }
    }

    player.x = clamp(player.x, 0, s.width - player.w);

    for (const c of crystals) {
      if (c.taken) continue;
      c.bob += dt * 5;
      const box = { x:c.x-14, y:c.y-14, w:28, h:28 };
      if (rects(player, box)) {
        c.taken = true;
        player.crystals++;
        spawnParticle(c.x,c.y,{count:20,color:'#fff3a4',speed:180,life:0.55,size:3,gravity:60});
        updateHud();
      }
    }

    for (const e of enemies) {
      if (!e.alive) continue;
      e.x += e.vx * dt;
      if (e.x < e.baseX - e.range || e.x > e.baseX + e.range) e.vx *= -1;
      const enemyBox = { x:e.x, y:e.y, w:e.w, h:e.h };
      if (rects(player, enemyBox)) {
        if (prevY + player.h <= e.y + 12 && player.vy > 0) {
          e.alive = false;
          e.squish = 1;
          player.vy = -560;
          player.crystals += 2;
          spawnParticle(e.x+e.w/2,e.y+e.h/2,{count:22,color:'#b9ffbd',speed:170,life:0.52,size:4,gravity:160});
          updateHud();
        } else {
          hurt(player.x < e.x ? -1 : 1);
        }
      }
    }

    for (const h of hazards) if (rects(player,h)) hurt(player.x < h.x ? -1 : 1, true);
    if (player.y > 930) hurt(0, true, true);

    for (const cp of checkpoints) {
      cp.glow += dt;
      if (!cp.active && rects(player, cp)) {
        checkpoints.forEach(c => c.active = false);
        cp.active = true;
        player.checkpoint = { x: cp.x, y: cp.y - 20 };
        showToast('Checkpoint lit. Nice one.', 2.1);
        spawnParticle(cp.x+cp.w/2, cp.y+20, {count:30,color:'#8af3ff',speed:180,life:0.7,size:4,gravity:40});
      }
    }

    portal.pulse += dt;
    portal.open = crystals.filter(c => c.taken).length >= Math.ceil(crystals.length * 0.65);
    if (portal.open && rects(player, portal)) {
      if (stageIndex < stages.length - 1) {
        player.crystals += 5;
        updateHud();
        loadStage(stageIndex + 1);
      } else {
        winGame();
      }
    } else if (!portal.open && rects(player, portal)) {
      showToast('The beacon needs more crystals before it opens.', 1.6);
    }

    for (let i=particles.length-1;i>=0;i--) {
      const p = particles[i];
      p.x += p.vx * dt;
      p.y += p.vy * dt;
      p.vy += p.gravity * dt;
      p.vx *= Math.pow(0.12, dt);
      p.life -= dt;
      if (p.life <= 0) particles.splice(i,1);
    }

    for (const f of fireflies) {
      f.a += dt * (0.6 + f.s*0.2);
      f.y += Math.sin(f.a) * dt * 8;
    }

    const targetX = clamp(player.x + player.w/2 - viewW*0.42, 0, Math.max(0, s.width - viewW));
    const targetY = clamp(player.y - viewH*0.56, -80, 180);
    cameraX += (targetX - cameraX) * (1 - Math.pow(0.0008, dt));
    cameraY += (targetY - cameraY) * (1 - Math.pow(0.003, dt));
    shake = Math.max(0, shake - dt);

    if (toastTimer > 0) {
      toastTimer -= dt;
      if (toastTimer <= 0) toast.classList.add('hidden');
    }
  }

  function hurt(dir, heavy=false, fall=false) {
    if (player.invuln > 0 && !fall) return;
    player.health--;
    updateHud();
    shake = heavy ? 0.3 : 0.18;
    player.invuln = 1.1;
    player.vx = dir * -360;
    player.vy = -520;
    spawnParticle(player.x+player.w/2, player.y+player.h/2, {count:24,color:'#ff8a9a',speed:220,life:0.55,size:4,gravity:180});
    if (player.health <= 0) {
      running = false;
      gameOverTitle.textContent = 'The storm caught you';
      gameOverText.textContent = 'You reached Stage ' + stages[stageIndex].name + ' with ' + player.crystals + ' crystals.';
      setTimeout(() => gameOver.classList.remove('hidden'), 450);
      return;
    }
    if (fall) respawn();
  }

  function respawn() {
    player.x = player.checkpoint.x;
    player.y = player.checkpoint.y;
    player.vx = 0;
    player.vy = 0;
    player.invuln = 1.2;
  }

  function winGame() {
    running = false;
    gameOverTitle.textContent = 'Lighthouse Awakened';
    gameOverText.textContent = 'You cleared all stages with ' + player.crystals + ' crystals. That was vish.';
    setTimeout(() => gameOver.classList.remove('hidden'), 650);
  }

  function draw(time) {
    const s = stages[stageIndex] || stages[0];
    ctx.clearRect(0,0,viewW,viewH);
    const sx = shake > 0 ? (Math.random()-0.5) * shake * 16 : 0;
    const sy = shake > 0 ? (Math.random()-0.5) * shake * 16 : 0;
    ctx.save();
    ctx.translate(sx, sy);
    drawSky(s, time);
    drawWorld(s, time);
    drawForeground(s, time);
    ctx.restore();
  }

  function drawSky(s, time) {
    const g = ctx.createLinearGradient(0,0,0,viewH);
    g.addColorStop(0,'#172b62');
    g.addColorStop(0.45,'#7056a8');
    g.addColorStop(0.75,'#f2a76d');
    g.addColorStop(1,'#13152d');
    ctx.fillStyle = g;
    ctx.fillRect(0,0,viewW,viewH);

    ctx.save();
    ctx.globalAlpha = 0.55;
    const sunX = viewW*0.72 - cameraX*0.04;
    const sunY = viewH*0.25;
    const rg = ctx.createRadialGradient(sunX,sunY,10,sunX,sunY,220);
    rg.addColorStop(0,'rgba(255,246,177,0.85)');
    rg.addColorStop(0.25,'rgba(255,191,134,0.32)');
    rg.addColorStop(1,'rgba(255,191,134,0)');
    ctx.fillStyle = rg;
    ctx.beginPath(); ctx.arc(sunX,sunY,230,0,Math.PI*2); ctx.fill();
    ctx.restore();

    drawMountainLayer('#20376e', 0.09, 340, 0.45);
    drawMountainLayer('#172754', 0.16, 430, 0.62);
    drawMountainLayer('#0d1838', 0.25, 535, 0.82);

    for (const c of clouds) {
      const x = ((c.x - cameraX*c.speed*0.01) % (s.width + 420)) - cameraX*0.04;
      drawCloud(x, c.y, c.s);
    }
  }

  function drawMountainLayer(color, parallax, base, alpha) {
    ctx.save();
    ctx.globalAlpha = alpha;
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(-80, viewH);
    for (let x=-100; x<viewW+220; x+=160) {
      const wx = x + (cameraX*parallax % 160);
      const peak = base - 100 - Math.sin((x+cameraX*0.02)*0.01)*50;
      ctx.lineTo(wx+80, peak);
      ctx.lineTo(wx+180, viewH);
    }
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }

  function drawCloud(x,y,s) {
    ctx.save();
    ctx.globalAlpha = 0.20;
    ctx.fillStyle = '#fff';
    blob(x,y,70*s,24*s);
    blob(x+42*s,y-10*s,58*s,30*s);
    blob(x+82*s,y,68*s,25*s);
    ctx.restore();
  }

  function blob(x,y,w,h) {
    ctx.beginPath();
    ctx.ellipse(x,y,w,h,0,0,Math.PI*2);
    ctx.fill();
  }

  function drawWorld(s, time) {
    ctx.save();
    ctx.translate(-cameraX, -cameraY);

    for (const f of fireflies) {
      const pulse = 0.4 + Math.sin(f.a)*0.25;
      ctx.globalAlpha = 0.35 + pulse;
      ctx.fillStyle = '#fff3a4';
      ctx.beginPath(); ctx.arc(f.x, f.y, f.s, 0, Math.PI*2); ctx.fill();
    }
    ctx.globalAlpha = 1;

    for (const h of hazards) drawSpikes(h);
    for (const p of platforms) drawPlatform(p, time);
    for (const cp of checkpoints) drawCheckpoint(cp, time);
    for (const c of crystals) if (!c.taken) drawCrystal(c, time);
    for (const e of enemies) if (e.alive) drawEnemy(e, time);
    drawPortal(portal, time);
    drawPlayer(time);
    drawParticles();
    ctx.restore();
  }

  function drawPlatform(p, time) {
    const r = 16;
    ctx.save();
    ctx.shadowColor = 'rgba(0,0,0,0.28)';
    ctx.shadowBlur = 18;
    ctx.shadowOffsetY = 14;
    round(p.x,p.y,p.w,p.h,r);
    const g = ctx.createLinearGradient(p.x,p.y,p.x,p.y+p.h);
    g.addColorStop(0, p.moving ? '#9be8ff' : '#59d58d');
    g.addColorStop(0.22, p.moving ? '#5eb3ff' : '#2aa365');
    g.addColorStop(1, '#193b41');
    ctx.fillStyle = g;
    ctx.fill();
    ctx.shadowBlur = 0;

    ctx.fillStyle = 'rgba(255,255,255,0.18)';
    round(p.x+8,p.y+7,p.w-16,6,8);
    ctx.fill();

    ctx.fillStyle = '#162333';
    for (let x=p.x+14; x<p.x+p.w-10; x+=34) {
      ctx.beginPath();
      ctx.moveTo(x, p.y+p.h);
      ctx.quadraticCurveTo(x+8, p.y+p.h+20+Math.sin(time*2+x)*5, x+18, p.y+p.h);
      ctx.fill();
    }
    ctx.restore();
  }

  function drawSpikes(h) {
    ctx.save();
    const count = Math.max(2, Math.floor(h.w/22));
    for (let i=0;i<count;i++) {
      const x = h.x + i*(h.w/count);
      const w = h.w/count;
      const g = ctx.createLinearGradient(x,h.y,x,h.y+h.h);
      g.addColorStop(0,'#ffeff2');
      g.addColorStop(0.4,'#ff6277');
      g.addColorStop(1,'#6f1f35');
      ctx.fillStyle = g;
      ctx.beginPath();
      ctx.moveTo(x, h.y+h.h);
      ctx.lineTo(x+w/2, h.y+10);
      ctx.lineTo(x+w, h.y+h.h);
      ctx.closePath();
      ctx.fill();
    }
    ctx.restore();
  }

  function drawCrystal(c, time) {
    const y = c.y + Math.sin(time*4+c.bob)*7;
    ctx.save();
    ctx.shadowColor = '#fff3a4';
    ctx.shadowBlur = 22;
    ctx.fillStyle = '#fff3a4';
    ctx.beginPath();
    ctx.moveTo(c.x, y-18);
    ctx.lineTo(c.x+14, y-3);
    ctx.lineTo(c.x+8, y+17);
    ctx.lineTo(c.x-8, y+17);
    ctx.lineTo(c.x-14, y-3);
    ctx.closePath();
    ctx.fill();
    ctx.shadowBlur = 0;
    ctx.fillStyle = 'rgba(255,255,255,0.52)';
    ctx.beginPath(); ctx.moveTo(c.x, y-15); ctx.lineTo(c.x+6, y-2); ctx.lineTo(c.x, y+12); ctx.closePath(); ctx.fill();
    ctx.restore();
  }

  function drawEnemy(e, time) {
    ctx.save();
    const bob = Math.sin(time*7+e.x)*3;
    ctx.translate(e.x+e.w/2, e.y+e.h/2+bob);
    ctx.scale(e.vx>0 ? 1 : -1, 1);
    ctx.shadowColor = 'rgba(105,255,151,0.45)';
    ctx.shadowBlur = 16;
    ctx.fillStyle = '#6dff9d';
    round(-e.w/2,-e.h/2,e.w,e.h,14);
    ctx.fill();
    ctx.shadowBlur = 0;
    ctx.fillStyle = '#102026';
    ctx.beginPath(); ctx.arc(7,-4,3,0,Math.PI*2); ctx.fill();
    ctx.fillStyle = 'rgba(255,255,255,0.4)';
    ctx.beginPath(); ctx.arc(-8,-10,7,0,Math.PI*2); ctx.fill();
    ctx.restore();
  }

  function drawCheckpoint(cp, time) {
    ctx.save();
    ctx.translate(cp.x, cp.y);
    ctx.fillStyle = '#4d3f2d';
    round(12,14,8,cp.h,8); ctx.fill();
    ctx.shadowColor = cp.active ? '#8af3ff' : '#fff3a4';
    ctx.shadowBlur = cp.active ? 28 : 12;
    ctx.fillStyle = cp.active ? '#8af3ff' : '#fff3a4';
    ctx.beginPath();
    ctx.moveTo(19,16);
    ctx.quadraticCurveTo(54, 4 + Math.sin(time*4)*5, 32, 33);
    ctx.quadraticCurveTo(50, 52, 19, 44);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }

  function drawPortal(p, time) {
    const cx = p.x + p.w/2, cy = p.y + p.h/2;
    ctx.save();
    ctx.shadowColor = p.open ? '#8af3ff' : '#7466a8';
    ctx.shadowBlur = p.open ? 34 : 14;
    ctx.strokeStyle = p.open ? '#8af3ff' : 'rgba(255,255,255,0.34)';
    ctx.lineWidth = 7;
    ctx.beginPath();
    ctx.ellipse(cx, cy, p.w/2, p.h/2, 0, 0, Math.PI*2);
    ctx.stroke();
    if (p.open) {
      const rg = ctx.createRadialGradient(cx,cy,8,cx,cy,54+Math.sin(time*4)*8);
      rg.addColorStop(0,'rgba(255,255,255,0.72)');
      rg.addColorStop(0.45,'rgba(138,243,255,0.36)');
      rg.addColorStop(1,'rgba(138,243,255,0)');
      ctx.fillStyle = rg;
      ctx.beginPath(); ctx.ellipse(cx,cy,p.w/2-5,p.h/2-5,0,0,Math.PI*2); ctx.fill();
    }
    ctx.restore();
  }

  function drawPlayer(time) {
    ctx.save();
    const blink = player.invuln > 0 && Math.floor(time*18)%2===0;
    if (blink) ctx.globalAlpha = 0.45;
    const x = player.x, y = player.y;
    ctx.translate(x+player.w/2, y+player.h/2);
    ctx.scale(player.facing, 1);
    const lean = clamp(player.vx/800, -0.18, 0.18);
    ctx.rotate(lean);

    ctx.shadowColor = player.dashTimer>0 ? '#8af3ff' : 'rgba(0,0,0,0.3)';
    ctx.shadowBlur = player.dashTimer>0 ? 24 : 12;
    ctx.fillStyle = '#ffdf87';
    round(-14,-25,28,32,13); ctx.fill();
    ctx.shadowBlur = 0;

    ctx.fillStyle = '#28345f';
    round(-17,4,34,25,12); ctx.fill();
    ctx.fillStyle = '#8af3ff';
    round(-12,8,24,8,8); ctx.fill();

    ctx.fillStyle = '#18213f';
    round(-11,27,9,18,5); ctx.fill();
    round(4,27,9,18,5); ctx.fill();

    ctx.fillStyle = '#11182e';
    round(1,-12,4,4,3); ctx.fill();
    ctx.fillStyle = '#ff8a9a';
    round(-12,-32,24,11,10); ctx.fill();
    round(-5,-42,22,16,10); ctx.fill();

    if (player.dashTimer>0) {
      ctx.globalAlpha = 0.5;
      ctx.fillStyle = '#8af3ff';
      for (let i=0;i<3;i++) round(-55-i*18,-8+i*2,34,8,8), ctx.fill();
    }
    ctx.restore();
  }

  function drawParticles() {
    for (const p of particles) {
      const a = Math.max(0, p.life / p.max);
      ctx.globalAlpha = a;
      ctx.shadowBlur = 12;
      ctx.shadowColor = p.color;
      ctx.fillStyle = p.color;
      ctx.beginPath(); ctx.arc(p.x,p.y,p.size*a,0,Math.PI*2); ctx.fill();
    }
    ctx.globalAlpha = 1;
    ctx.shadowBlur = 0;
  }

  function drawForeground(s, time) {
    ctx.save();
    ctx.globalAlpha = 0.22;
    ctx.fillStyle = '#050614';
    const y = viewH - 70;
    for (let x = -80 - (cameraX*0.42 % 180); x < viewW + 160; x += 180) {
      ctx.beginPath();
      ctx.ellipse(x, y + Math.sin(time+x)*8, 75, 24, 0, 0, Math.PI*2);
      ctx.fill();
    }
    ctx.restore();
  }

  function round(x,y,w,h,r) {
    r = Math.min(r,w/2,h/2);
    ctx.beginPath();
    ctx.moveTo(x+r,y);
    ctx.arcTo(x+w,y,x+w,y+h,r);
    ctx.arcTo(x+w,y+h,x,y+h,r);
    ctx.arcTo(x,y+h,x,y,r);
    ctx.arcTo(x,y,x+w,y,r);
    ctx.closePath();
  }

  function loop(now) {
    const dt = Math.min(0.033, (now-last)/1000 || 0.016);
    last = now;
    pressed.clear();
    // pressed must clear after update, so copy current one first
    const currentPressed = new Set(pressed);
    // Not used: kept clean by pointer/keyboard events between frames.
    update(dt, now/1000);
    pressed.clear();
    draw(now/1000);
    if (running || particles.length) raf = requestAnimationFrame(loop);
  }

  function clamp(n,min,max) { return Math.max(min, Math.min(max,n)); }

  // Fix for one-frame input: browser events add to pressed before loop, update reads then clears at end.
  const originalLoop = loop;
  function fixedLoop(now) {
    const dt = Math.min(0.033, (now-last)/1000 || 0.016);
    last = now;
    if (running) update(dt, now/1000);
    pressed.clear();
    draw(now/1000);
    if (running || particles.length) raf = requestAnimationFrame(fixedLoop);
  }
  loop = fixedLoop;

  draw(0);
})();
