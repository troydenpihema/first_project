import Phaser from 'phaser';
import { getLevel, LEVELS } from '../data/levels.js';
import GameAudio from '../audio/GameAudio.js';

const HEIGHT = 540;

const THEME = {
  coast: {
    sky: 0x6fdcff,
    far: 0x1f7c3c,
    near: 0x22964a,
    dirt: 0x85512a,
    top: 'grass-top',
    haze: 0xffffff
  },
  jungle: {
    sky: 0x5bcf9a,
    far: 0x115c2d,
    near: 0x1c8a42,
    dirt: 0x70421f,
    top: 'grass-top',
    haze: 0xdfffd9
  },
  temple: {
    sky: 0xd7b36a,
    far: 0x7b613e,
    near: 0x96704a,
    dirt: 0x6f5230,
    top: 'stone-top',
    haze: 0xffedc4
  },
  volcano: {
    sky: 0x532025,
    far: 0x40161c,
    near: 0x7a2d22,
    dirt: 0x3c1d1b,
    top: 'volcano-top',
    haze: 0xff6930
  },
  night: {
    sky: 0x10163c,
    far: 0x141c4a,
    near: 0x273064,
    dirt: 0x1c1d37,
    top: 'night-top',
    haze: 0x8bd3ff
  }
};

export default class GameScene extends Phaser.Scene {
  constructor() {
    super('GameScene');
  }

  init(data) {
    this.levelIndex = data.levelIndex ?? 0;
    this.campaign = data.campaign ?? { fruit: 0, lives: 3, totalCrates: 0, brokenCrates: 0 };
    this.level = getLevel(this.levelIndex);

    this.fruit = this.campaign.fruit ?? 0;
    this.lives = this.campaign.lives ?? 3;
    this.levelCratesBroken = 0;
    this.levelFruit = 0;
    this.levelComplete = FalseFlag();
    this.isPaused = false;

    this.checkpoint = { ...this.level.spawn };
  }

  create() {
    this.theme = THEME[this.level.theme] ?? THEME.jungle;

    this.physics.world.setBounds(0, 0, this.level.width, HEIGHT + 240);
    this.cameras.main.setBounds(0, 0, this.level.width, HEIGHT);

    this.createBackground();
    this.createWorld();
    this.createPlayer();
    this.createInput();
    this.createCamera();
    this.createUI();
    this.createParticles();

    this.physics.add.collider(this.player, this.platforms);
    this.physics.add.collider(this.player, this.movingPlatforms, this.rideMovingPlatform, null, this);
    this.physics.add.collider(this.enemies, this.platforms);
    this.physics.add.collider(this.enemies, this.movingPlatforms);
    this.physics.add.overlap(this.player, this.fruits, this.collectFruit, null, this);
    this.physics.add.overlap(this.player, this.portal, this.finishLevel, null, this);
    this.physics.add.overlap(this.player, this.spikes, this.touchHazard, null, this);
    this.physics.add.overlap(this.player, this.checkpoints, this.activateCheckpoint, null, this);
    this.physics.add.overlap(this.player, this.powerups, this.collectPowerup, null, this);
    this.physics.add.overlap(this.playerAttackZone, this.crates, this.spinHitCrate, null, this);
    this.physics.add.overlap(this.playerAttackZone, this.enemies, this.spinHitEnemy, null, this);
    this.physics.add.collider(this.player, this.enemies, this.touchEnemy, null, this);
    this.physics.add.collider(this.player, this.crates, this.touchCrate, null, this);

    GameAudio.unlock();
    GameAudio.startMusic(this.level.theme);
    this.showToast(this.level.name);
  }

  update(time, delta) {
    if (this.levelComplete || this.isPaused) return;

    this.updatePowerups(time);
    this.updatePlayer(time);
    this.updateEnemies();
    this.updateMovingPlatforms(delta);
    this.updateAttackZone();

    if (this.player.y > HEIGHT + 160) {
      this.loseLife(true);
    }

    if (time % 250 < 20) {
      this.updateUI();
    }
  }

  createBackground() {
    this.cameras.main.setBackgroundColor(this.theme.sky);

    const sunColor = this.level.theme === 'night' ? 0xd8e8ff : 0xfff3a0;
    const sun = this.add.circle(780, 90, this.level.theme === 'night' ? 42 : 55, sunColor, this.level.theme === 'night' ? 0.35 : 0.75);
    sun.setScrollFactor(0.05);
    sun.setBlendMode(Phaser.BlendModes.ADD);

    for (let i = 0; i < 80; i++) {
      const x = i * 90 - 80;
      const y = 410 + Math.sin(i * 0.55) * 25;
      const mountain = this.add.triangle(x, y, 0, 150, 75, 0, 150, 150, this.theme.far, 0.62);
      mountain.setScrollFactor(0.16);
      mountain.setDepth(-10);
    }

    for (let i = 0; i < 55; i++) {
      const x = i * 140 + 40;
      const z = i % 2 ? 0.42 : 0.58;
      const trunk = this.add.rectangle(x, 458, 22, 150, 0x5e3418).setScrollFactor(z).setDepth(-6);
      const leaves = this.add.ellipse(x, 365 + Math.sin(i) * 14, 118, 76, this.theme.near, 0.85).setScrollFactor(z).setDepth(-5);
      if (this.level.theme === 'temple') {
        trunk.setFillStyle(0x5b462e);
        leaves.setFillStyle(0x826640, 0.9);
      }
    }

    if (this.level.theme !== 'volcano') {
      for (let i = 0; i < 13; i++) {
        const cloud = this.add.ellipse(i * 320 + 80, 70 + (i % 3) * 34, 130, 34, 0xffffff, this.level.theme === 'night' ? 0.12 : 0.42);
        cloud.setScrollFactor(0.08);
        cloud.setDepth(-12);
      }
    } else {
      for (let i = 0; i < 18; i++) {
        const ember = this.add.circle(i * 260 + 80, 80 + (i % 5) * 60, 3, 0xff9a32, 0.7);
        ember.setScrollFactor(0.15);
        ember.setDepth(-3);
      }
    }
  }

  createWorld() {
    this.platforms = this.physics.add.staticGroup();
    this.movingPlatforms = this.physics.add.group({ allowGravity: false, immovable: true });
    this.crates = this.physics.add.staticGroup();
    this.fruits = this.physics.add.group({ allowGravity: false });
    this.enemies = this.physics.add.group({ allowGravity: true });
    this.spikes = this.physics.add.staticGroup();
    this.checkpoints = this.physics.add.staticGroup();
    this.powerups = this.physics.add.group({ allowGravity: false });

    this.level.platforms.forEach(([x, y, w, h]) => this.createPlatform(x, y, w, h));
    this.level.moving.forEach(data => this.createMovingPlatform(...data));
    this.level.crates.forEach(([x, y, type]) => this.createCrate(x, y, type === 'bonus'));
    this.level.fruits.forEach(([x, y, count, gap]) => this.createFruitLine(x, y, count, gap));
    this.level.enemies.forEach(([x, y, minX, maxX, type]) => this.createEnemy(x, y, minX, maxX, type));
    this.level.spikes.forEach(([x, y, count]) => this.createSpikeRow(x, y, count));
    this.level.checkpoints.forEach(([x, y]) => this.createCheckpoint(x, y));
    this.level.powerups?.forEach(([x, y, type]) => this.createPowerup(x, y, type));

    this.totalCrates = this.crates.countActive(true);

    this.portal = this.physics.add.staticSprite(this.level.portal.x, this.level.portal.y, 'portal');
    this.portal.setDepth(10);
  }

  createPlatform(x, y, w, h) {
    const dirt = this.add.rectangle(x + w / 2, y + h / 2, w, h, this.theme.dirt);
    this.physics.add.existing(dirt, true);
    this.platforms.add(dirt);

    for (let tx = 0; tx < w; tx += 128) {
      const tileW = Math.min(128, w - tx);
      const top = this.add.tileSprite(x + tx + tileW / 2, y - 8, tileW + 6, 30, this.theme.top);
      this.physics.add.existing(top, true);
      this.platforms.add(top);
    }

    for (let i = 0; i < Math.floor(w / 50); i++) {
      const pebble = this.add.rectangle(x + 20 + i * 50, y + 35 + (i % 3) * 18, 22, 6, 0x000000, 0.16);
      pebble.setDepth(-1);
    }
  }

  createMovingPlatform(x, y, w, h, minX, maxX, speed) {
    const platform = this.physics.add.sprite(x, y, 'moving-platform');
    platform.displayWidth = w;
    platform.displayHeight = h;
    platform.refreshBody?.();

    platform.body.allowGravity = false;
    platform.body.immovable = true;
    platform.body.moves = true;
    platform.body.setSize(w, h);
    platform.body.setVelocityX(speed);

    platform.minX = minX;
    platform.maxX = maxX;
    platform.speed = speed;
    platform.setDepth(2);

    this.tweens.add({
      targets: platform,
      alpha: 0.75,
      duration: 700,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut'
    });

    this.movingPlatforms.add(platform);
  }

  createPlayer() {
    this.player = this.physics.add.sprite(this.level.spawn.x, this.level.spawn.y, 'player');
    this.player.setCollideWorldBounds(true);
    this.player.setSize(35, 50);
    this.player.setOffset(11, 10);
    this.player.body.setMaxVelocity(520, 950);
    this.player.invincibleUntil = 0;
    this.player.isSpinning = false;
    this.player.spinUntil = 0;
    this.player.dashCooldownUntil = 0;
    this.player.lastGroundedAt = 0;
    this.player.jumpBufferedUntil = 0;
    this.player.facing = 1;
    this.player.extraJumps = 0;
    this.activePowerups = {
      shieldUntil: 0,
      magnetUntil: 0,
      speedUntil: 0,
      doubleUntil: 0
    };

    this.playerAttackZone = this.add.zone(this.player.x, this.player.y, 104, 70);
    this.physics.add.existing(this.playerAttackZone);
    this.playerAttackZone.body.allowGravity = false;
    this.playerAttackZone.body.setEnable(false);
  }

  createInput() {
    this.cursors = this.input.keyboard.createCursorKeys();
    this.keys = this.input.keyboard.addKeys({
      a: Phaser.Input.Keyboard.KeyCodes.A,
      d: Phaser.Input.Keyboard.KeyCodes.D,
      w: Phaser.Input.Keyboard.KeyCodes.W,
      j: Phaser.Input.Keyboard.KeyCodes.J,
      k: Phaser.Input.Keyboard.KeyCodes.K,
      p: Phaser.Input.Keyboard.KeyCodes.P,
      r: Phaser.Input.Keyboard.KeyCodes.R,
      shift: Phaser.Input.Keyboard.KeyCodes.SHIFT
    });

    this.input.keyboard.on('keydown-P', () => {
      this.isPaused = !this.isPaused;
      this.physics.world.isPaused = this.isPaused;
      this.pauseText.setVisible(this.isPaused);
    });

    this.input.keyboard.on('keydown-R', () => {
      this.scene.restart({ levelIndex: this.levelIndex, campaign: this.campaign });
    });
  }

  createCamera() {
    this.cameras.main.startFollow(this.player, true, 0.09, 0.09);
    this.cameras.main.setFollowOffset(-130, 70);
    this.cameras.main.setDeadzone(120, 90);
  }

  createUI() {
    const style = {
      fontFamily: 'Arial',
      fontSize: '21px',
      fontStyle: '900',
      color: '#ffffff',
      stroke: '#000000',
      strokeThickness: 5
    };

    this.add.rectangle(136, 54, 250, 100, 0x000000, 0.24).setScrollFactor(0).setDepth(95);
    this.fruitText = this.add.text(18, 14, '', style).setScrollFactor(0).setDepth(100);
    this.livesText = this.add.text(18, 44, '', style).setScrollFactor(0).setDepth(100);
    this.cratesText = this.add.text(18, 74, '', style).setScrollFactor(0).setDepth(100);

    this.levelText = this.add.text(940, 18, `Level ${this.levelIndex + 1}/${LEVELS.length}`, {
      fontFamily: 'Arial',
      fontSize: '20px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000',
      strokeThickness: 4
    }).setOrigin(1, 0).setScrollFactor(0).setDepth(100);

    this.powerText = this.add.text(940, 48, '', {
      fontFamily: 'Arial',
      fontSize: '16px',
      fontStyle: '900',
      color: '#dfffee',
      stroke: '#000',
      strokeThickness: 4
    }).setOrigin(1, 0).setScrollFactor(0).setDepth(100);

    this.toastText = this.add.text(480, 92, '', {
      fontFamily: 'Arial',
      fontSize: '34px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000',
      strokeThickness: 7
    }).setOrigin(0.5).setScrollFactor(0).setDepth(200).setAlpha(0);

    this.pauseText = this.add.text(480, 230, 'PAUSED', {
      fontFamily: 'Arial',
      fontSize: '62px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000000',
      strokeThickness: 8
    }).setOrigin(0.5).setScrollFactor(0).setDepth(200).setVisible(false);

    this.updateUI();
  }

  createParticles() {
    this.collectParticles = this.add.particles(0, 0, 'spark', {
      lifespan: 330,
      speed: { min: 70, max: 180 },
      scale: { start: 0.9, end: 0 },
      quantity: 8,
      tint: [0xffe66d, 0xff9717, 0xffffff],
      emitting: false
    });

    this.breakParticles = this.add.particles(0, 0, 'spark', {
      lifespan: 450,
      speed: { min: 90, max: 240 },
      gravityY: 700,
      scale: { start: 1.1, end: 0 },
      quantity: 14,
      tint: [0xc17931, 0xffe66d, 0xffffff],
      emitting: false
    });

    this.hurtParticles = this.add.particles(0, 0, 'spark', {
      lifespan: 360,
      speed: { min: 100, max: 250 },
      gravityY: 500,
      scale: { start: 1, end: 0 },
      quantity: 14,
      tint: [0xff4b4b, 0xffffff],
      emitting: false
    });
  }

  updatePlayer(time) {
    const left = this.cursors.left.isDown || this.keys.a.isDown;
    const right = this.cursors.right.isDown || this.keys.d.isDown;
    const onGround = this.player.body.blocked.down || this.player.body.touching.down;

    if (onGround) {
      this.player.lastGroundedAt = time;
    }

    const jumpJustPressed =
      Phaser.Input.Keyboard.JustDown(this.cursors.space) ||
      Phaser.Input.Keyboard.JustDown(this.cursors.up) ||
      Phaser.Input.Keyboard.JustDown(this.keys.w);

    if (jumpJustPressed) {
      this.player.jumpBufferedUntil = time + 130;
    }

    const canUseBufferedJump = this.player.jumpBufferedUntil > time;
    const hasCoyoteTime = time - this.player.lastGroundedAt < 120;

    if (canUseBufferedJump && hasCoyoteTime) {
      this.player.setVelocityY(-535);
      GameAudio.play('jump');
      this.player.jumpBufferedUntil = 0;
      this.player.lastGroundedAt = 0;
    } else if (canUseBufferedJump && this.activePowerups.doubleUntil > time && this.player.extraJumps > 0) {
      this.player.setVelocityY(-505);
      this.player.extraJumps -= 1;
      GameAudio.play('jump');
      this.player.jumpBufferedUntil = 0;
      this.collectParticles.emitParticleAt(this.player.x, this.player.y + 20);
    }

    const targetSpeed = this.activePowerups.speedUntil > time ? 345 : 270;
    const accel = onGround ? 26 : 14;
    const decel = onGround ? 0.80 : 0.96;

    if (left) {
      this.player.setVelocityX(Phaser.Math.Linear(this.player.body.velocity.x, -targetSpeed, accel / 60));
      this.player.setFlipX(true);
      this.player.facing = -1;
    } else if (right) {
      this.player.setVelocityX(Phaser.Math.Linear(this.player.body.velocity.x, targetSpeed, accel / 60));
      this.player.setFlipX(false);
      this.player.facing = 1;
    } else {
      this.player.setVelocityX(this.player.body.velocity.x * decel);
    }

    if (Phaser.Input.Keyboard.JustDown(this.keys.shift) && time > this.player.dashCooldownUntil) {
      const dir = this.player.facing || 1;
      this.player.setVelocityX(520 * dir);
      this.player.setVelocityY(Math.min(this.player.body.velocity.y, -80));
      this.player.dashCooldownUntil = time + 900;
      this.cameras.main.shake(80, 0.004);
      GameAudio.play('dash');
      const trail = this.add.image(this.player.x - dir * 28, this.player.y + 8, 'dash-trail')
        .setFlipX(dir < 0)
        .setAlpha(0.65)
        .setDepth(3);
      this.tweens.add({ targets: trail, alpha: 0, duration: 220, onComplete: () => trail.destroy() });
    }

    if (Phaser.Input.Keyboard.JustDown(this.keys.j) || Phaser.Input.Keyboard.JustDown(this.keys.k)) {
      this.startSpin(time);
    }

    if (this.player.isSpinning) {
      this.player.angle += this.player.facing * 40;

      if (time >= this.player.spinUntil) {
        this.player.isSpinning = false;
        this.player.angle = 0;
        this.playerAttackZone.body.setEnable(false);
      }
    }

    if (time < this.player.invincibleUntil) {
      this.player.alpha = Math.sin(time / 45) > 0 ? 1 : 0.35;
    } else {
      this.player.alpha = 1;
    }
  }

  startSpin(time) {
    if (this.player.isSpinning) return;
    this.player.isSpinning = true;
    this.player.spinUntil = time + 360;
    this.playerAttackZone.body.setEnable(true);
    this.cameras.main.shake(55, 0.0025);
    GameAudio.play('spin');
  }

  updateAttackZone() {
    const dir = this.player.facing || 1;
    this.playerAttackZone.setPosition(this.player.x + dir * 24, this.player.y + 3);
  }

  rideMovingPlatform(player, platform) {
    if (player.body.blocked.down || player.body.touching.down) {
      player.x += platform.body.velocity.x * this.game.loop.delta / 1000;
    }
  }

  updateMovingPlatforms() {
    this.movingPlatforms.children.iterate(platform => {
      if (!platform || !platform.active) return;

      if (platform.x <= platform.minX) {
        platform.x = platform.minX;
        platform.body.setVelocityX(Math.abs(platform.speed));
      }

      if (platform.x >= platform.maxX) {
        platform.x = platform.maxX;
        platform.body.setVelocityX(-Math.abs(platform.speed));
      }
    });
  }

  updateEnemies() {
    this.enemies.children.iterate(enemy => {
      if (!enemy || !enemy.active) return;

      if (enemy.x <= enemy.minX) {
        enemy.setVelocityX(enemy.speed);
        enemy.setFlipX(false);
      } else if (enemy.x >= enemy.maxX) {
        enemy.setVelocityX(-enemy.speed);
        enemy.setFlipX(true);
      }
    });
  }

  createCrate(x, y, bonus = false) {
    const crate = this.crates.create(x, y, 'crate');
    crate.refreshBody();
    crate.setData('bonus', bonus);
    if (bonus) crate.setTint(0xffcf55);
    return crate;
  }

  createFruit(x, y) {
    const fruit = this.fruits.create(x, y, 'fruit');
    fruit.setDepth(4);
    fruit.body.setCircle(12, 5, 5);
    this.tweens.add({
      targets: fruit,
      y: y - 8,
      duration: 900 + Phaser.Math.Between(0, 300),
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut'
    });
    return fruit;
  }

  createFruitLine(x, y, count, gap) {
    for (let i = 0; i < count; i++) {
      this.createFruit(x + i * gap, y + Math.sin(i * 0.8) * 14);
    }
  }

  createEnemy(x, y, minX, maxX) {
    const enemy = this.enemies.create(x, y, 'enemy-crawler');
    enemy.setCollideWorldBounds(false);
    enemy.setVelocityX(105);
    enemy.speed = 105;
    enemy.minX = minX;
    enemy.maxX = maxX;
    enemy.body.setSize(44, 28);
    enemy.body.setOffset(7, 15);
    return enemy;
  }

  createSpikeRow(x, y, count) {
    for (let i = 0; i < count; i++) {
      const spike = this.spikes.create(x + i * 30, y, 'spike');
      spike.refreshBody();
    }
  }

  createCheckpoint(x, y) {
    const cp = this.checkpoints.create(x, y, 'checkpoint');
    cp.setScale(1.2);
    cp.refreshBody();
    cp.setData('active', false);
    return cp;
  }


  createPowerup(x, y, type = 'shield') {
    const key = `power-${type}`;
    const power = this.powerups.create(x, y, key);
    power.setData('type', type);
    power.setDepth(8);
    power.body.setCircle(18, 2, 2);

    this.tweens.add({
      targets: power,
      y: y - 10,
      angle: 360,
      duration: 1300,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut'
    });

    return power;
  }

  collectPowerup(player, powerup) {
    if (!powerup.active) return;

    const type = powerup.getData('type');
    const now = this.time.now;

    powerup.disableBody(true, true);
    GameAudio.play('powerup');
    this.collectParticles.emitParticleAt(powerup.x, powerup.y);

    if (type === 'shield') {
      this.activePowerups.shieldUntil = now + 15000;
      this.showToast('Shield Power!');
    } else if (type === 'magnet') {
      this.activePowerups.magnetUntil = now + 15000;
      this.showToast('Fruit Magnet!');
    } else if (type === 'speed') {
      this.activePowerups.speedUntil = now + 12000;
      this.showToast('Speed Boost!');
    } else if (type === 'double') {
      this.activePowerups.doubleUntil = now + 15000;
      this.player.extraJumps = 1;
      this.showToast('Double Jump!');
    }

    this.updateUI();
  }

  updatePowerups(time) {
    if (this.activePowerups.magnetUntil > time) {
      this.fruits.children.iterate(fruit => {
        if (!fruit || !fruit.active) return;
        const distance = Phaser.Math.Distance.Between(this.player.x, this.player.y, fruit.x, fruit.y);
        if (distance < 190) {
          const angle = Phaser.Math.Angle.Between(fruit.x, fruit.y, this.player.x, this.player.y);
          fruit.x += Math.cos(angle) * 4.6;
          fruit.y += Math.sin(angle) * 4.6;
        }
      });
    }

    if ((this.player.body.blocked.down || this.player.body.touching.down) && this.activePowerups.doubleUntil > time) {
      this.player.extraJumps = 1;
    }
  }


  collectFruit(player, fruit) {
    if (!fruit.active) return;

    const x = fruit.x;
    const y = fruit.y;
    fruit.disableBody(true, true);

    this.fruit += 1;
    this.levelFruit += 1;

    if (this.fruit > 0 && this.fruit % 40 === 0) {
      this.lives += 1;
      this.showToast('+1 Life');
    }

    this.collectParticles.emitParticleAt(x, y);
    GameAudio.play('fruit');
    this.updateUI();
  }

  spinHitCrate(zone, crate) {
    if (!this.player.isSpinning || !crate.active) return;
    this.breakCrate(crate);
  }

  spinHitEnemy(zone, enemy) {
    if (!this.player.isSpinning || !enemy.active) return;

    const x = enemy.x;
    const y = enemy.y;
    enemy.disableBody(true, true);
    this.breakParticles.emitParticleAt(x, y);
    GameAudio.play('enemy');
    this.cameras.main.shake(110, 0.006);
    this.fruit += 3;
    this.updateUI();
  }

  touchEnemy(player, enemy) {
    if (!enemy.active || this.time.now < player.invincibleUntil) return;

    if (player.body.velocity.y > 140 && player.y < enemy.y - 10) {
      const x = enemy.x;
      const y = enemy.y;
      enemy.disableBody(true, true);
      player.setVelocityY(-430);
      this.breakParticles.emitParticleAt(x, y);
      GameAudio.play('enemy');
      this.fruit += 3;
      this.updateUI();
      return;
    }

    this.hurtPlayer();
  }

  touchCrate(player, crate) {
    if (!crate.active) return;

    if (player.body.velocity.y > 150 && player.body.bottom <= crate.body.top + 25) {
      player.setVelocityY(-430);
      this.breakCrate(crate);
    }
  }

  breakCrate(crate) {
    if (!crate.active) return;

    const x = crate.x;
    const y = crate.y;
    const bonus = crate.getData('bonus');
    const amount = bonus ? 6 : 3;

    crate.disableBody(true, true);
    this.levelCratesBroken += 1;
    this.breakParticles.emitParticleAt(x, y);
    GameAudio.play('crate');
    this.cameras.main.shake(85, 0.0035);

    for (let i = 0; i < amount; i++) {
      this.createFruit(
        x + Phaser.Math.Between(-34, 34),
        y - Phaser.Math.Between(30, 85)
      );
    }

    this.updateUI();
  }

  touchHazard() {
    this.hurtPlayer(true);
  }

  activateCheckpoint(player, checkpoint) {
    if (checkpoint.getData('active')) return;

    checkpoint.setData('active', true);
    checkpoint.setTint(0xffe66d);
    checkpoint.setScale(1.45);
    this.checkpoint = { x: checkpoint.x, y: checkpoint.y - 40 };
    this.showToast('Checkpoint!');
    GameAudio.play('checkpoint');
    this.collectParticles.emitParticleAt(checkpoint.x, checkpoint.y);
  }

  hurtPlayer(forceRespawn = false) {
    if (this.time.now < this.player.invincibleUntil) return;

    if (this.activePowerups.shieldUntil > this.time.now) {
      this.activePowerups.shieldUntil = 0;
      this.player.invincibleUntil = this.time.now + 1200;
      this.hurtParticles.emitParticleAt(this.player.x, this.player.y);
      GameAudio.play('hurt');
      this.cameras.main.shake(130, 0.007);
      this.showToast('Shield Saved You!');
      this.updateUI();
      return;
    }

    this.lives -= 1;
    this.hurtParticles.emitParticleAt(this.player.x, this.player.y);
    GameAudio.play('hurt');
    this.cameras.main.shake(180, 0.012);

    if (this.lives <= 0) {
      this.scene.start('GameOverScene', {
        levelIndex: this.levelIndex,
        campaign: this.campaign,
        fruit: this.fruit,
        crates: this.levelCratesBroken,
        totalCrates: this.totalCrates
      });
      return;
    }

    if (forceRespawn) {
      this.respawnPlayer();
    } else {
      this.player.invincibleUntil = this.time.now + 1400;
      this.player.setVelocity(this.player.facing < 0 ? 330 : -330, -350);
    }

    this.updateUI();
  }

  loseLife() {
    this.lives -= 1;

    if (this.lives <= 0) {
      this.scene.start('GameOverScene', {
        levelIndex: this.levelIndex,
        campaign: this.campaign,
        fruit: this.fruit,
        crates: this.levelCratesBroken,
        totalCrates: this.totalCrates
      });
      return;
    }

    this.respawnPlayer();
    this.updateUI();
  }

  respawnPlayer() {
    this.player.setPosition(this.checkpoint.x, this.checkpoint.y);
    this.player.setVelocity(0, 0);
    this.player.invincibleUntil = this.time.now + 1500;
  }

  finishLevel() {
    if (this.levelComplete) return;
    this.levelComplete = true;

    this.physics.world.isPaused = true;

    const nextCampaign = {
      fruit: this.fruit,
      lives: this.lives,
      brokenCrates: (this.campaign.brokenCrates ?? 0) + this.levelCratesBroken,
      totalCrates: (this.campaign.totalCrates ?? 0) + this.totalCrates
    };

    GameAudio.play('levelComplete');
    this.scene.start('LevelCompleteScene', {
      levelIndex: this.levelIndex,
      campaign: nextCampaign,
      levelStats: {
        fruit: this.levelFruit,
        crates: this.levelCratesBroken,
        totalCrates: this.totalCrates,
        lives: this.lives
      }
    });
  }

  updateUI() {
    this.fruitText.setText(`Fruit: ${this.fruit}`);
    this.livesText.setText(`Lives: ${this.lives}`);
    this.cratesText.setText(`Crates: ${this.levelCratesBroken}/${this.totalCrates}`);

    if (this.powerText && this.activePowerups) {
      const now = this.time.now;
      const active = [];
      if (this.activePowerups.shieldUntil > now) active.push('Shield');
      if (this.activePowerups.magnetUntil > now) active.push('Magnet');
      if (this.activePowerups.speedUntil > now) active.push('Speed');
      if (this.activePowerups.doubleUntil > now) active.push('Double Jump');
      this.powerText.setText(active.length ? `Power: ${active.join(' + ')}` : '');
    }
  }

  showToast(message) {
    this.toastText.setText(message);
    this.toastText.setAlpha(0);
    this.toastText.setY(92);

    this.tweens.add({
      targets: this.toastText,
      alpha: 1,
      y: 78,
      duration: 260,
      ease: 'Sine.easeOut',
      yoyo: true,
      hold: 950
    });
  }
}

function FalseFlag() {
  return false;
}
