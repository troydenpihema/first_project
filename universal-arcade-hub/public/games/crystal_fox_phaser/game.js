(() => {
  if (!window.Phaser) {
    document.body.innerHTML = '<div style="color:white;font-family:Arial;padding:24px">Phaser failed to load. Run this through Live Server/Replit with internet access, or install Phaser locally.</div>';
    return;
  }

  const WIDTH = 1280;
  const HEIGHT = 720;
  const SAVE_KEY = 'crystalFoxBestScore';

  const LEVELS = [
    {
      name: 'Mosslight Valley',
      tintTop: 0x172b53,
      tintBottom: 0x0a1027,
      width: 3200,
      player: { x: 110, y: 510 },
      goal: { x: 3020, y: 458 },
      platforms: [
        [0, 650, 540, 80], [620, 585, 260, 34], [980, 530, 240, 34], [1340, 585, 330, 34],
        [1790, 520, 260, 34], [2150, 580, 260, 34], [2500, 520, 240, 34], [2840, 650, 480, 80],
        [420, 455, 190, 28], [1600, 420, 210, 28], [2320, 400, 180, 28]
      ],
      crystals: [[470, 415], [700, 540], [1080, 488], [1440, 542], [1660, 380], [1870, 475], [2250, 535], [2400, 360], [2580, 475], [2960, 608]],
      enemies: [[820, 540, 700, 875], [1500, 540, 1360, 1660], [2200, 535, 2150, 2390], [2700, 475, 2510, 2730]],
      spikes: [[1235, 612, 150], [1990, 612, 140]],
      moving: [[1160, 405, 170, 26, 1160, 1500, 1.2], [1980, 405, 170, 26, 1980, 2260, 1.1]],
      sign: { x: 260, y: 585, text: 'Find the crystals. Dash through the valley. Reach the moon gate.' }
    },
    {
      name: 'Sunken Gearworks',
      tintTop: 0x38234b,
      tintBottom: 0x110b1f,
      width: 3700,
      player: { x: 90, y: 520 },
      goal: { x: 3490, y: 438 },
      platforms: [
        [0, 650, 430, 80], [520, 590, 240, 34], [865, 520, 220, 34], [1190, 455, 220, 34],
        [1510, 560, 280, 34], [1940, 610, 260, 34], [2320, 540, 220, 34], [2640, 470, 260, 34],
        [3020, 550, 250, 34], [3360, 650, 500, 80], [430, 440, 180, 28], [1770, 380, 190, 28], [2880, 345, 190, 28]
      ],
      crystals: [[580, 548], [955, 480], [1290, 415], [1585, 518], [1840, 338], [2030, 570], [2410, 500], [2740, 430], [2965, 305], [3125, 508], [3510, 608]],
      enemies: [[680, 545, 535, 755], [1555, 515, 1510, 1780], [2400, 495, 2325, 2535], [3130, 505, 3030, 3270]],
      spikes: [[770, 612, 170], [1810, 612, 160], [2530, 612, 180], [3290, 612, 120]],
      moving: [[1320, 330, 170, 26, 1320, 1660, 1.4], [2135, 430, 170, 26, 2135, 2420, 1.35], [2860, 440, 170, 26, 2860, 3180, 1.25]],
      sign: { x: 150, y: 585, text: 'The gears are alive. Watch the gaps and use dash to recover.' }
    },
    {
      name: 'Aurora Citadel',
      tintTop: 0x163c4f,
      tintBottom: 0x050817,
      width: 4200,
      player: { x: 90, y: 520 },
      goal: { x: 3970, y: 395 },
      platforms: [
        [0, 650, 420, 80], [520, 590, 230, 34], [880, 525, 230, 34], [1230, 465, 220, 34],
        [1580, 405, 210, 34], [1940, 520, 240, 34], [2300, 590, 230, 34], [2640, 510, 220, 34],
        [2970, 430, 230, 34], [3310, 360, 220, 34], [3640, 500, 230, 34], [3900, 650, 470, 80],
        [600, 390, 170, 28], [2220, 360, 180, 28], [3460, 545, 160, 28]
      ],
      crystals: [[595, 548], [710, 350], [960, 485], [1330, 425], [1660, 365], [2040, 478], [2360, 548], [2730, 470], [3080, 390], [3390, 320], [3520, 505], [3730, 460], [4040, 608]],
      enemies: [[610, 545, 525, 745], [1010, 480, 885, 1105], [2010, 475, 1940, 2175], [2720, 465, 2640, 2860], [3740, 455, 3640, 3860]],
      spikes: [[430, 612, 160], [1460, 612, 210], [2180, 612, 130], [2890, 612, 170], [3560, 612, 170]],
      moving: [[1450, 300, 160, 26, 1450, 1800, 1.55], [2460, 400, 160, 26, 2460, 2760, 1.4], [3260, 465, 170, 26, 3260, 3600, 1.45]],
      sign: { x: 170, y: 585, text: 'Final climb. Master jump, dash, and timing to open the citadel gate.' }
    }
  ];

  const clamp = Phaser.Math.Clamp;

  class BootScene extends Phaser.Scene {
    constructor() { super('Boot'); }
    preload() {}
    create() {
      this.createTextures();
      this.scene.start('Menu');
    }
    createTextures() {
      const g = this.add.graphics();

      g.clear();
      g.fillStyle(0xffffff, 1);
      g.fillRoundedRect(0, 8, 48, 50, 15);
      g.fillStyle(0xffd25c, 1);
      g.fillTriangle(7, 10, 18, 0, 24, 14);
      g.fillTriangle(31, 14, 38, 0, 43, 14);
      g.fillStyle(0x111527, 1);
      g.fillCircle(17, 28, 4);
      g.fillCircle(32, 28, 4);
      g.fillStyle(0xff6bcf, 1);
      g.fillCircle(14, 37, 4);
      g.fillCircle(35, 37, 4);
      g.fillStyle(0xffffff, 1);
      g.fillRoundedRect(13, 50, 22, 20, 8);
      g.generateTexture('fox', 48, 74);

      g.clear();
      g.fillStyle(0x2de2ff, 1);
      g.fillTriangle(18, 0, 36, 22, 25, 48);
      g.fillTriangle(18, 0, 0, 22, 11, 48);
      g.fillStyle(0xffffff, 0.55);
      g.fillTriangle(18, 5, 30, 22, 18, 42);
      g.generateTexture('crystal', 36, 50);

      g.clear();
      g.fillStyle(0x0c1225, 1);
      g.fillRoundedRect(0, 0, 56, 46, 12);
      g.fillStyle(0xff406b, 1);
      g.fillCircle(18, 19, 6);
      g.fillCircle(38, 19, 6);
      g.fillStyle(0x0c1225, 1);
      g.fillCircle(18, 19, 2);
      g.fillCircle(38, 19, 2);
      g.fillStyle(0x9b4dff, 1);
      g.fillTriangle(8, 0, 18, 0, 13, -13);
      g.fillTriangle(38, 0, 48, 0, 43, -13);
      g.fillStyle(0x151b31, 1);
      g.fillRoundedRect(8, 35, 40, 18, 8);
      g.generateTexture('enemy', 56, 58);

      g.clear();
      g.fillStyle(0xff4f6d, 1);
      for (let i = 0; i < 6; i++) g.fillTriangle(i * 30, 32, i * 30 + 15, 0, i * 30 + 30, 32);
      g.generateTexture('spikes', 180, 34);

      g.clear();
      g.fillStyle(0x44fff3, 1);
      g.fillCircle(24, 24, 24);
      g.fillStyle(0xffffff, 0.85);
      g.fillCircle(18, 16, 7);
      g.generateTexture('orb', 48, 48);

      g.clear();
      g.fillStyle(0xffd76a, 1);
      g.fillRoundedRect(0, 0, 96, 80, 16);
      g.fillStyle(0x2b1d18, 1);
      g.fillRoundedRect(12, 12, 72, 46, 8);
      g.fillStyle(0xffffff, 0.9);
      g.fillRect(18, 20, 60, 6);
      g.fillRect(18, 32, 44, 6);
      g.generateTexture('sign', 96, 80);

      g.clear();
      g.fillStyle(0x12e8ff, 0.16);
      g.fillCircle(64, 64, 62);
      g.lineStyle(6, 0x47f7ff, 0.9);
      g.strokeCircle(64, 64, 46);
      g.lineStyle(3, 0xffffff, 0.55);
      g.strokeCircle(64, 64, 26);
      g.generateTexture('portal', 128, 128);

      g.clear();
      g.fillStyle(0x18213d, 1);
      g.fillRoundedRect(0, 0, 240, 48, 12);
      g.lineStyle(3, 0x33eaff, 0.45);
      g.strokeRoundedRect(2, 2, 236, 44, 12);
      g.fillStyle(0x233057, 1);
      for (let i = 0; i < 8; i++) g.fillRect(i * 32 + 8, 8, 18, 8);
      g.generateTexture('platform', 240, 48);

      g.clear();
      g.fillStyle(0xffffff, 1);
      g.fillCircle(8, 8, 8);
      g.generateTexture('spark', 16, 16);

      g.destroy();
    }
  }

  class MenuScene extends Phaser.Scene {
    constructor() { super('Menu'); }
    create() {
      const { width, height } = this.scale;
      makeGradient(this, 0x071126, 0x132a4c);
      createStars(this, width, height, 75, 0.5);

      this.add.text(width / 2, 100, 'CRYSTAL FOX', {
        fontFamily: 'Arial Black, Arial', fontSize: '76px', color: '#ffffff', stroke: '#29e8ff', strokeThickness: 3
      }).setOrigin(0.5).setShadow(0, 0, '#2de2ff', 18);

      this.add.text(width / 2, 172, 'A polished Phaser platform adventure for your arcade hub', {
        fontFamily: 'Arial', fontSize: '22px', color: '#cfefff'
      }).setOrigin(0.5);

      const fox = this.add.image(width / 2, 315, 'fox').setScale(3.1);
      this.tweens.add({ targets: fox, y: 300, duration: 1100, yoyo: true, repeat: -1, ease: 'Sine.inOut' });

      const panel = this.add.rectangle(width / 2, 535, 760, 180, 0x06101f, 0.72).setStrokeStyle(2, 0x2de2ff, 0.4);
      this.add.text(width / 2, 488, 'Run, jump, dash, collect crystals, defeat shadow creatures, and open each moon gate.', {
        fontFamily: 'Arial', fontSize: '21px', color: '#ffffff', align: 'center', wordWrap: { width: 680 }
      }).setOrigin(0.5);

      const start = this.add.text(width / 2, 570, 'PRESS ENTER / SPACE TO START', {
        fontFamily: 'Arial Black, Arial', fontSize: '28px', color: '#ffe66d'
      }).setOrigin(0.5).setShadow(0, 0, '#ffe66d', 12);
      this.tweens.add({ targets: start, alpha: 0.35, duration: 600, yoyo: true, repeat: -1 });

      this.add.text(width / 2, 628, 'Move: A/D or Arrows   Jump: Space/W/Up   Dash: Shift   Attack: J', {
        fontFamily: 'Arial', fontSize: '18px', color: '#9fb7da'
      }).setOrigin(0.5);

      this.input.keyboard.once('keydown-ENTER', () => this.scene.start('Play', { level: 0 }));
      this.input.keyboard.once('keydown-SPACE', () => this.scene.start('Play', { level: 0 }));
      this.input.once('pointerdown', () => this.scene.start('Play', { level: 0 }));
    }
  }

  class PlayScene extends Phaser.Scene {
    constructor() { super('Play'); }
    init(data) {
      this.levelIndex = data.level || 0;
      this.level = LEVELS[this.levelIndex];
      this.score = data.score || 0;
      this.hp = data.hp || 3;
      this.dashReady = true;
      this.attackReady = true;
      this.invulnerable = 0;
      this.levelComplete = false;
    }
    create() {
      this.physics.world.setBounds(0, 0, this.level.width, HEIGHT);
      this.cameras.main.setBounds(0, 0, this.level.width, HEIGHT);
      this.cameras.main.setBackgroundColor('#050711');

      this.createWorld();
      this.createPlayer();
      this.createObjects();
      this.createInput();
      this.createHUD();
      this.createMobileControls();
      this.showLevelTitle();

      this.cameras.main.startFollow(this.player, true, 0.08, 0.08, -90, 65);
      this.cameras.main.fadeIn(450, 5, 7, 17);
    }
    createWorld() {
      makeGradient(this, this.level.tintTop, this.level.tintBottom, this.level.width, HEIGHT).setScrollFactor(0);
      createStars(this, this.level.width, HEIGHT, 180, 0.25);

      for (let i = 0; i < 16; i++) {
        const x = i * 320;
        const mountain = this.add.triangle(x + 140, 650, 0, 0, 180, -300 - (i % 3) * 50, 360, 0, 0x0b1830, 0.65);
        mountain.setScrollFactor(0.22);
      }
      for (let i = 0; i < 20; i++) {
        const x = i * 240;
        const hill = this.add.ellipse(x + 100, 660, 380, 145, 0x132642, 0.85);
        hill.setScrollFactor(0.48);
      }

      this.platforms = this.physics.add.staticGroup();
      this.level.platforms.forEach(([x, y, w, h]) => {
        const p = this.add.tileSprite(x + w / 2, y + h / 2, w, h, 'platform');
        p.setTint(0x88f8ff);
        this.physics.add.existing(p, true);
        p.body.setSize(w, h).setOffset(0, 0);
        this.platforms.add(p);
      });

      this.movers = this.physics.add.group({ allowGravity: false, immovable: true });
      this.level.moving.forEach(([x, y, w, h, minX, maxX, speed]) => {
        const m = this.add.tileSprite(x + w / 2, y + h / 2, w, h, 'platform').setTint(0xffb7f5);
        this.physics.add.existing(m);
        m.body.allowGravity = false;
        m.body.immovable = true;
        m.moveData = { minX, maxX, speed: speed * 90, dir: 1 };
        this.movers.add(m);
      });

      this.add.rectangle(this.level.width / 2, 704, this.level.width, 36, 0x050711, 0.82).setDepth(4);
    }
    createPlayer() {
      this.player = this.physics.add.sprite(this.level.player.x, this.level.player.y, 'fox').setDepth(20);
      this.player.setCollideWorldBounds(false);
      this.player.body.setSize(34, 58).setOffset(7, 13);
      this.player.setDragX(1300);
      this.player.setMaxVelocity(420, 780);
      this.physics.add.collider(this.player, this.platforms);
      this.physics.add.collider(this.player, this.movers, (player, mover) => {
        if (player.body.touching.down && mover.body.touching.up) player.x += mover.body.velocity.x * this.game.loop.delta / 1000;
      });
    }
    createObjects() {
      this.crystals = this.physics.add.group({ allowGravity: false, immovable: true });
      this.level.crystals.forEach(([x, y]) => {
        const c = this.crystals.create(x, y, 'crystal');
        c.setDepth(12).setScale(0.9);
        c.body.setCircle(18, 0, 6);
        this.tweens.add({ targets: c, y: y - 8, duration: 950 + Math.random() * 450, yoyo: true, repeat: -1, ease: 'Sine.inOut' });
      });
      this.physics.add.overlap(this.player, this.crystals, (_, crystal) => this.collectCrystal(crystal));

      this.enemies = this.physics.add.group();
      this.level.enemies.forEach(([x, y, minX, maxX]) => {
        const e = this.enemies.create(x, y, 'enemy').setDepth(16);
        e.body.setSize(42, 38).setOffset(7, 18);
        e.setCollideWorldBounds(false);
        e.setVelocityX(80);
        e.patrol = { minX, maxX, dir: 1 };
      });
      this.physics.add.collider(this.enemies, this.platforms);
      this.physics.add.collider(this.enemies, this.movers);
      this.physics.add.overlap(this.player, this.enemies, (_, enemy) => this.hitEnemy(enemy));

      this.spikes = this.physics.add.staticGroup();
      this.level.spikes.forEach(([x, y, w]) => {
        const s = this.add.tileSprite(x + w / 2, y + 16, w, 34, 'spikes').setDepth(14);
        this.physics.add.existing(s, true);
        s.body.setSize(w, 24).setOffset(0, 8);
        this.spikes.add(s);
      });
      this.physics.add.overlap(this.player, this.spikes, () => this.damagePlayer(1, true));

      this.portal = this.physics.add.staticSprite(this.level.goal.x, this.level.goal.y, 'portal').setDepth(10).setScale(1.05);
      this.portal.body.setCircle(42, 22, 22);
      this.tweens.add({ targets: this.portal, angle: 360, duration: 6000, repeat: -1 });
      this.physics.add.overlap(this.player, this.portal, () => this.completeLevel());

      this.sign = this.physics.add.staticSprite(this.level.sign.x, this.level.sign.y, 'sign').setDepth(13);
      this.sign.body.setSize(96, 80);
      this.physics.add.overlap(this.player, this.sign, () => this.showToast(this.level.sign.text));

      this.fx = this.add.particles(0, 0, 'spark', {
        speed: { min: 40, max: 220 }, lifespan: 450, scale: { start: 0.7, end: 0 }, emitting: false, blendMode: 'ADD'
      }).setDepth(50);
    }
    createInput() {
      this.keys = this.input.keyboard.addKeys({
        left: Phaser.Input.Keyboard.KeyCodes.LEFT,
        right: Phaser.Input.Keyboard.KeyCodes.RIGHT,
        up: Phaser.Input.Keyboard.KeyCodes.UP,
        a: Phaser.Input.Keyboard.KeyCodes.A,
        d: Phaser.Input.Keyboard.KeyCodes.D,
        w: Phaser.Input.Keyboard.KeyCodes.W,
        space: Phaser.Input.Keyboard.KeyCodes.SPACE,
        shift: Phaser.Input.Keyboard.KeyCodes.SHIFT,
        j: Phaser.Input.Keyboard.KeyCodes.J,
        p: Phaser.Input.Keyboard.KeyCodes.P
      });
      this.input.keyboard.on('keydown-P', () => {
        this.scene.pause();
        this.scene.launch('Pause', { from: 'Play' });
      });
      this.touch = { left: false, right: false, jump: false, dash: false, attack: false };
    }
    createHUD() {
      this.hud = this.add.container(0, 0).setScrollFactor(0).setDepth(100);
      const bg = this.add.rectangle(20, 18, 414, 54, 0x050711, 0.62).setOrigin(0).setStrokeStyle(1, 0x2de2ff, 0.25);
      this.scoreText = this.add.text(38, 34, 'CRYSTALS 0', { fontFamily: 'Arial Black, Arial', fontSize: '19px', color: '#ffffff' });
      this.hpText = this.add.text(220, 34, '♥ ♥ ♥', { fontFamily: 'Arial Black, Arial', fontSize: '20px', color: '#ff6384' });
      this.levelText = this.add.text(WIDTH - 34, 34, this.level.name, { fontFamily: 'Arial Black, Arial', fontSize: '19px', color: '#cfefff' }).setOrigin(1, 0);
      this.dashText = this.add.text(38, 76, 'DASH READY', { fontFamily: 'Arial', fontSize: '15px', color: '#ffe66d' });
      this.hud.add([bg, this.scoreText, this.hpText, this.levelText, this.dashText]);
      this.toast = this.add.text(WIDTH / 2, 620, '', { fontFamily: 'Arial', fontSize: '20px', color: '#ffffff', backgroundColor: 'rgba(5,7,17,0.78)', padding: { x: 18, y: 12 }, align: 'center', wordWrap: { width: 720 } }).setOrigin(0.5).setScrollFactor(0).setDepth(120).setAlpha(0);
    }
    createMobileControls() {
      const makeBtn = (x, y, label, key) => {
        const c = this.add.container(x, y).setScrollFactor(0).setDepth(200).setAlpha(0.82);
        const circle = this.add.circle(0, 0, 38, 0xffffff, 0.1).setStrokeStyle(2, 0xffffff, 0.24);
        const t = this.add.text(0, 0, label, { fontFamily: 'Arial Black', fontSize: '22px', color: '#ffffff' }).setOrigin(0.5);
        c.add([circle, t]);
        c.setSize(76, 76).setInteractive();
        c.on('pointerdown', () => this.touch[key] = true);
        c.on('pointerup', () => this.touch[key] = false);
        c.on('pointerout', () => this.touch[key] = false);
        c.on('pointercancel', () => this.touch[key] = false);
        return c;
      };
      makeBtn(72, HEIGHT - 78, '←', 'left');
      makeBtn(162, HEIGHT - 78, '→', 'right');
      makeBtn(WIDTH - 252, HEIGHT - 78, 'J', 'attack');
      makeBtn(WIDTH - 162, HEIGHT - 78, '⚡', 'dash');
      makeBtn(WIDTH - 72, HEIGHT - 78, '↑', 'jump');
    }
    showLevelTitle() {
      const title = this.add.text(WIDTH / 2, 126, this.level.name, { fontFamily: 'Arial Black, Arial', fontSize: '44px', color: '#ffffff', stroke: '#2de2ff', strokeThickness: 2 }).setOrigin(0.5).setScrollFactor(0).setDepth(130);
      const sub = this.add.text(WIDTH / 2, 174, `Stage ${this.levelIndex + 1} / ${LEVELS.length}`, { fontFamily: 'Arial', fontSize: '20px', color: '#cfefff' }).setOrigin(0.5).setScrollFactor(0).setDepth(130);
      this.tweens.add({ targets: [title, sub], alpha: 0, y: '-=30', delay: 1300, duration: 650, onComplete: () => { title.destroy(); sub.destroy(); } });
    }
    update(time, deltaMs) {
      const dt = deltaMs / 1000;
      if (!this.player || this.levelComplete) return;
      this.updateMovers(dt);
      this.updateEnemies();
      this.updatePlayer(dt);
      this.updateHUD();
      if (this.player.y > HEIGHT + 220) this.damagePlayer(3, true);
      if (this.invulnerable > 0) this.invulnerable -= dt;
    }
    updateMovers(dt) {
      this.movers.children.iterate(m => {
        if (!m) return;
        const d = m.moveData;
        m.x += d.dir * d.speed * dt;
        if (m.x < d.minX + m.width / 2) { m.x = d.minX + m.width / 2; d.dir = 1; }
        if (m.x > d.maxX + m.width / 2) { m.x = d.maxX + m.width / 2; d.dir = -1; }
        m.body.updateFromGameObject();
        m.body.setVelocityX(d.dir * d.speed);
      });
    }
    updateEnemies() {
      this.enemies.children.iterate(e => {
        if (!e || !e.active) return;
        const p = e.patrol;
        if (e.x <= p.minX) { p.dir = 1; e.setFlipX(false); }
        if (e.x >= p.maxX) { p.dir = -1; e.setFlipX(true); }
        e.setVelocityX(p.dir * 88);
      });
    }
    updatePlayer(dt) {
      const left = this.keys.left.isDown || this.keys.a.isDown || this.touch.left;
      const right = this.keys.right.isDown || this.keys.d.isDown || this.touch.right;
      const jumpPressed = Phaser.Input.Keyboard.JustDown(this.keys.space) || Phaser.Input.Keyboard.JustDown(this.keys.up) || Phaser.Input.Keyboard.JustDown(this.keys.w) || this.consumeTouch('jump');
      const dashPressed = Phaser.Input.Keyboard.JustDown(this.keys.shift) || this.consumeTouch('dash');
      const attackPressed = Phaser.Input.Keyboard.JustDown(this.keys.j) || this.consumeTouch('attack');
      const onGround = this.player.body.blocked.down || this.player.body.touching.down;

      if (left) { this.player.setAccelerationX(-2200); this.player.setFlipX(true); }
      else if (right) { this.player.setAccelerationX(2200); this.player.setFlipX(false); }
      else this.player.setAccelerationX(0);

      if (jumpPressed && onGround) {
        this.player.setVelocityY(-620);
        this.burst(this.player.x, this.player.y + 35, 12, 0x88f8ff);
      }

      if (dashPressed && this.dashReady) {
        const dir = this.player.flipX ? -1 : 1;
        this.player.setVelocityX(dir * 760);
        this.player.setVelocityY(Math.min(this.player.body.velocity.y, -80));
        this.dashReady = false;
        this.time.delayedCall(850, () => this.dashReady = true);
        this.cameras.main.shake(80, 0.004);
        this.burst(this.player.x - dir * 22, this.player.y + 20, 24, 0x2de2ff);
      }

      if (attackPressed && this.attackReady) {
        this.attackReady = false;
        this.time.delayedCall(300, () => this.attackReady = true);
        const dir = this.player.flipX ? -1 : 1;
        const hit = this.add.ellipse(this.player.x + dir * 48, this.player.y + 8, 92, 58, 0xffffff, 0.22).setDepth(25);
        this.tweens.add({ targets: hit, alpha: 0, scale: 1.5, duration: 150, onComplete: () => hit.destroy() });
        this.enemies.children.iterate(enemy => {
          if (!enemy || !enemy.active) return;
          if (Phaser.Math.Distance.Between(enemy.x, enemy.y, this.player.x + dir * 45, this.player.y) < 75) this.defeatEnemy(enemy);
        });
      }

      if (onGround && Math.abs(this.player.body.velocity.x) > 60) {
        if (!this.lastDust || this.time.now - this.lastDust > 90) {
          this.lastDust = this.time.now;
          this.burst(this.player.x, this.player.y + 35, 3, 0x9fb7da);
        }
      }

      this.player.setScale(1 + Math.abs(this.player.body.velocity.x) / 9000, 1);
      if (this.invulnerable > 0) this.player.setAlpha(Math.sin(this.time.now * 0.04) > 0 ? 0.35 : 1); else this.player.setAlpha(1);
    }
    consumeTouch(key) {
      if (this.touch[key]) { this.touch[key] = false; return true; }
      return false;
    }
    collectCrystal(crystal) {
      if (!crystal.active) return;
      crystal.disableBody(true, true);
      this.score += 100;
      this.burst(crystal.x, crystal.y, 24, 0x2de2ff);
      this.cameras.main.flash(80, 45, 226, 255, false);
    }
    hitEnemy(enemy) {
      if (!enemy.active || this.invulnerable > 0) return;
      const playerAbove = this.player.body.velocity.y > 100 && this.player.y < enemy.y - 18;
      if (playerAbove) {
        this.defeatEnemy(enemy);
        this.player.setVelocityY(-460);
      } else {
        this.damagePlayer(1, false);
      }
    }
    defeatEnemy(enemy) {
      if (!enemy.active) return;
      enemy.disableBody(true, true);
      this.score += 150;
      this.burst(enemy.x, enemy.y, 30, 0xff6bcf);
      this.cameras.main.shake(90, 0.004);
    }
    damagePlayer(amount, resetPosition) {
      if (this.invulnerable > 0 && amount < 3) return;
      this.hp -= amount;
      this.invulnerable = 1.25;
      this.cameras.main.shake(180, 0.009);
      this.burst(this.player.x, this.player.y, 32, 0xff4f6d);
      if (this.hp <= 0) {
        this.scene.start('GameOver', { score: this.score });
        return;
      }
      if (resetPosition) {
        this.player.setPosition(Math.max(90, this.cameras.main.scrollX + 120), 460);
        this.player.setVelocity(0, 0);
      } else {
        this.player.setVelocity(this.player.flipX ? 360 : -360, -360);
      }
    }
    completeLevel() {
      if (this.levelComplete) return;
      this.levelComplete = true;
      this.score += 500;
      this.burst(this.portal.x, this.portal.y, 60, 0x44fff3);
      this.cameras.main.flash(450, 68, 255, 243, false);
      this.cameras.main.fadeOut(800, 5, 7, 17);
      this.time.delayedCall(900, () => {
        if (this.levelIndex + 1 >= LEVELS.length) this.scene.start('Win', { score: this.score, hp: this.hp });
        else this.scene.start('Play', { level: this.levelIndex + 1, score: this.score, hp: this.hp });
      });
    }
    showToast(text) {
      if (this.toastCooldown && this.time.now - this.toastCooldown < 500) return;
      this.toastCooldown = this.time.now;
      this.toast.setText(text);
      this.tweens.killTweensOf(this.toast);
      this.toast.setAlpha(0).setY(620);
      this.tweens.add({ targets: this.toast, alpha: 1, y: 600, duration: 180, yoyo: true, hold: 1800 });
    }
    burst(x, y, amount, tint) {
      if (!this.fx) return;
      this.fx.setParticleTint(tint);
      this.fx.emitParticleAt(x, y, amount);
    }
    updateHUD() {
      this.scoreText.setText(`SCORE ${this.score}`);
      this.hpText.setText('♥ '.repeat(Math.max(0, this.hp)).trim());
      this.dashText.setText(this.dashReady ? 'DASH READY' : 'DASH CHARGING');
      this.dashText.setColor(this.dashReady ? '#ffe66d' : '#9fb7da');
    }
  }

  class PauseScene extends Phaser.Scene {
    constructor() { super('Pause'); }
    create(data) {
      const overlay = this.add.rectangle(WIDTH / 2, HEIGHT / 2, WIDTH, HEIGHT, 0x050711, 0.72);
      this.add.text(WIDTH / 2, HEIGHT / 2 - 34, 'PAUSED', { fontFamily: 'Arial Black', fontSize: '58px', color: '#ffffff' }).setOrigin(0.5);
      this.add.text(WIDTH / 2, HEIGHT / 2 + 34, 'Press P or tap to resume', { fontFamily: 'Arial', fontSize: '23px', color: '#cfefff' }).setOrigin(0.5);
      const resume = () => { this.scene.stop(); this.scene.resume(data.from); };
      this.input.keyboard.once('keydown-P', resume);
      this.input.once('pointerdown', resume);
    }
  }

  class GameOverScene extends Phaser.Scene {
    constructor() { super('GameOver'); }
    create(data) { endScreen(this, 'GAME OVER', data.score || 0, false); }
  }

  class WinScene extends Phaser.Scene {
    constructor() { super('Win'); }
    create(data) { endScreen(this, 'CRYSTAL GATE OPENED', data.score || 0, true); }
  }

  function endScreen(scene, title, score, won) {
    const best = Math.max(Number(localStorage.getItem(SAVE_KEY) || 0), score);
    localStorage.setItem(SAVE_KEY, String(best));
    makeGradient(scene, won ? 0x143d4f : 0x25102d, 0x050711);
    createStars(scene, WIDTH, HEIGHT, 90, 0.5);
    scene.add.text(WIDTH / 2, 165, title, { fontFamily: 'Arial Black', fontSize: won ? '58px' : '72px', color: '#ffffff', stroke: won ? '#2de2ff' : '#ff4f6d', strokeThickness: 3 }).setOrigin(0.5).setShadow(0, 0, won ? '#2de2ff' : '#ff4f6d', 18);
    scene.add.image(WIDTH / 2, 305, won ? 'portal' : 'enemy').setScale(won ? 1.8 : 2.7).setAlpha(0.95);
    scene.add.text(WIDTH / 2, 440, `Score: ${score}\nBest: ${best}`, { fontFamily: 'Arial Black', fontSize: '30px', color: '#ffe66d', align: 'center' }).setOrigin(0.5);
    const prompt = scene.add.text(WIDTH / 2, 560, 'PRESS ENTER / SPACE TO PLAY AGAIN', { fontFamily: 'Arial Black', fontSize: '26px', color: '#ffffff' }).setOrigin(0.5);
    scene.tweens.add({ targets: prompt, alpha: 0.35, duration: 600, yoyo: true, repeat: -1 });
    scene.input.keyboard.once('keydown-ENTER', () => scene.scene.start('Play', { level: 0 }));
    scene.input.keyboard.once('keydown-SPACE', () => scene.scene.start('Play', { level: 0 }));
    scene.input.once('pointerdown', () => scene.scene.start('Play', { level: 0 }));
  }

  function makeGradient(scene, top, bottom, w = WIDTH, h = HEIGHT) {
    const rt = scene.add.renderTexture(0, 0, w, h).setOrigin(0);
    const steps = 32;
    for (let i = 0; i < steps; i++) {
      const t = i / (steps - 1);
      const c = Phaser.Display.Color.Interpolate.ColorWithColor(
        Phaser.Display.Color.IntegerToColor(top), Phaser.Display.Color.IntegerToColor(bottom), steps - 1, i
      );
      const color = Phaser.Display.Color.GetColor(c.r, c.g, c.b);
      const rect = scene.add.rectangle(w / 2, (h / steps) * i + h / steps / 2, w, h / steps + 2, color, 1);
      rt.draw(rect);
      rect.destroy();
    }
    return rt;
  }

  function createStars(scene, w, h, count, scrollFactor) {
    for (let i = 0; i < count; i++) {
      const star = scene.add.circle(Math.random() * w, Math.random() * h * 0.82, Math.random() * 1.8 + 0.5, 0xffffff, Math.random() * 0.45 + 0.18);
      star.setScrollFactor(scrollFactor);
      scene.tweens.add({ targets: star, alpha: 0.1, duration: 900 + Math.random() * 1600, yoyo: true, repeat: -1, delay: Math.random() * 1000 });
    }
  }

  const config = {
    type: Phaser.AUTO,
    parent: 'game-root',
    width: WIDTH,
    height: HEIGHT,
    backgroundColor: '#050711',
    scale: {
      mode: Phaser.Scale.FIT,
      autoCenter: Phaser.Scale.CENTER_BOTH,
      width: WIDTH,
      height: HEIGHT
    },
    physics: {
      default: 'arcade',
      arcade: { gravity: { y: 1400 }, debug: false }
    },
    scene: [BootScene, MenuScene, PlayScene, PauseScene, GameOverScene, WinScene]
  };

  new Phaser.Game(config);
})();
