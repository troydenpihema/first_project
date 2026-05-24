(() => {
  if (!window.Phaser) return;

  const WIDTH = 960;
  const HEIGHT = 540;

  class ThreadRunner extends Phaser.Scene {
    constructor() {
      super("ThreadRunner");
      this.started = false;
      this.ended = false;
      this.score = 0;
      this.spools = 0;
      this.energy = 100;
      this.speed = 280;
      this.nextPlatformX = 0;
      this.nextNeedle = 760;
      this.nextSpools = 520;
      this.pointerHeld = false;
      this.bestCombo = 0;
      this.combo = 0;
    }

    preload() {
      this.makeTextures();
    }

    create() {
      this.started = false;
      this.ended = false;
      this.score = 0;
      this.spools = 0;
      this.energy = 100;
      this.speed = 280;
      this.nextPlatformX = 0;
      this.nextNeedle = 760;
      this.nextSpools = 520;
      this.bestCombo = 0;
      this.combo = 0;

      this.createBackground();

      this.platforms = this.physics.add.group({ allowGravity: false, immovable: true });
      this.needles = this.physics.add.group({ allowGravity: false, immovable: true });
      this.collectibles = this.physics.add.group({ allowGravity: false, immovable: true });
      this.bridges = this.physics.add.group({ allowGravity: false, immovable: true });

      this.player = this.physics.add.sprite(160, 250, "player");
      this.player.setDepth(10);
      this.player.setCircle(17, 3, 3);
      this.player.setCollideWorldBounds(false);

      this.createStartingPlatforms();

      this.physics.add.collider(this.player, this.platforms, () => this.onGround());
      this.physics.add.collider(this.player, this.bridges, () => this.onGround());
      this.physics.add.overlap(this.player, this.needles, () => this.gameOver());
      this.physics.add.overlap(this.player, this.collectibles, (_player, spool) => this.collectSpool(spool));

      this.keys = this.input.keyboard.addKeys({
        up: Phaser.Input.Keyboard.KeyCodes.UP,
        space: Phaser.Input.Keyboard.KeyCodes.SPACE,
        r: Phaser.Input.Keyboard.KeyCodes.R
      });

      this.input.on("pointerdown", () => {
        this.pointerHeld = true;
        this.handlePress();
      });

      this.input.on("pointerup", () => {
        this.pointerHeld = false;
      });

      this.input.keyboard.on("keydown-SPACE", () => this.handlePress());
      this.input.keyboard.on("keydown-UP", () => this.handlePress());
      this.input.keyboard.on("keydown-R", () => {
        if (this.ended) this.scene.restart();
      });

      this.createUi();
      this.createTitle();

      this.trail = [];
    }

    update(_time, deltaMs) {
      const dt = deltaMs / 1000;

      this.bgLines.children.iterate((line) => {
        if (!line) return;
        line.x -= line.speed * dt;
        if (line.x < -250) line.x = WIDTH + Phaser.Math.Between(40, 220);
      });

      if (!this.started || this.ended) return;

      this.score += dt * this.speed * 0.12;
      this.speed = Phaser.Math.Clamp(280 + this.score * 0.025, 280, 520);

      this.player.x = 160;
      this.player.setVelocityX(0);

      const holding = this.pointerHeld || this.keys.space.isDown || this.keys.up.isDown;
      const grounded = this.player.body.touching.down || this.player.body.blocked.down;

      if (holding && !grounded && this.energy > 0) {
        this.player.setVelocityY(Math.max(this.player.body.velocity.y - 36, -430));
        this.energy -= 34 * dt;
        this.spawnSpark(this.player.x - 12, this.player.y + 10);
      } else {
        this.energy += 25 * dt;
      }

      this.energy = Phaser.Math.Clamp(this.energy, 0, 100);

      this.scrollGroup(this.platforms, dt);
      this.scrollGroup(this.needles, dt);
      this.scrollGroup(this.collectibles, dt);
      this.scrollGroup(this.bridges, dt);

      this.spawnPlatforms(dt);
      this.spawnNeedles(dt);
      this.spawnSpools(dt);
      this.cleanup(this.platforms);
      this.cleanup(this.needles);
      this.cleanup(this.collectibles);
      this.cleanup(this.bridges);

      if (this.player.y > HEIGHT + 100) this.gameOver();

      this.updateTrail(dt);
      this.updateUi();
    }

    makeTextures() {
      const g = this.make.graphics({ x: 0, y: 0, add: false });

      g.clear();
      g.fillStyle(0x7b3df2, 1);
      g.fillCircle(20, 20, 18);
      g.lineStyle(4, 0x7df9ff, 1);
      g.strokeCircle(20, 20, 16);
      g.lineStyle(2, 0xffffff, 0.9);
      g.beginPath();
      g.moveTo(7, 22);
      g.quadraticCurveTo(20, 6, 34, 21);
      g.strokePath();
      g.generateTexture("player", 40, 40);

      g.clear();
      g.fillStyle(0x21172e, 1);
      g.fillRoundedRect(0, 0, 230, 34, 12);
      g.lineStyle(3, 0x7df9ff, 0.45);
      g.strokeRoundedRect(0, 0, 230, 34, 12);
      for (let x = 16; x < 210; x += 32) {
        g.lineStyle(2, 0xffffff, 0.4);
        g.beginPath();
        g.moveTo(x, 17);
        g.lineTo(x + 16, 17);
        g.strokePath();
      }
      g.generateTexture("platform", 230, 34);

      g.clear();
      g.fillStyle(0xff4f8b, 1);
      g.fillTriangle(14, 0, 28, 58, 0, 58);
      g.lineStyle(3, 0xffffff, 0.45);
      g.strokeTriangle(14, 0, 28, 58, 0, 58);
      g.generateTexture("needle", 28, 58);

      g.clear();
      g.fillStyle(0x29f2ff, 1);
      g.fillCircle(13, 13, 11);
      g.fillStyle(0x08070d, 1);
      g.fillCircle(13, 13, 5);
      g.lineStyle(2, 0xffffff, 0.65);
      g.strokeCircle(13, 13, 11);
      g.generateTexture("spool", 26, 26);

      g.clear();
      g.fillStyle(0x1bffe4, 1);
      g.fillRoundedRect(0, 0, 160, 16, 8);
      g.lineStyle(2, 0xffffff, 0.6);
      g.strokeRoundedRect(0, 0, 160, 16, 8);
      g.generateTexture("bridge", 160, 16);

      g.clear();
      g.fillStyle(0xffffff, 1);
      g.fillCircle(4, 4, 4);
      g.generateTexture("spark", 8, 8);
    }

    createBackground() {
      this.bgLines = this.add.group();

      for (let i = 0; i < 40; i++) {
        const line = this.add.rectangle(
          Phaser.Math.Between(0, WIDTH),
          Phaser.Math.Between(40, HEIGHT - 40),
          Phaser.Math.Between(70, 240),
          2,
          0xffffff,
          Phaser.Math.FloatBetween(0.08, 0.22)
        );
        line.setDepth(-10);
        line.speed = Phaser.Math.Between(20, 75);
        this.bgLines.add(line);
      }

      for (let i = 0; i < 22; i++) {
        const dash = this.add.rectangle(i * 52, 292, 28, 3, 0xffffff, 0.2);
        dash.setDepth(1);
        this.bgLines.add(dash);
        dash.speed = 35;
      }
    }

    createStartingPlatforms() {
      this.addPlatform(0, 410, 310);
      this.addPlatform(355, 370, 230);
      this.addPlatform(650, 405, 260);
      this.addPlatform(970, 345, 230);
      this.nextPlatformX = 1220;
    }

    addPlatform(x, y, width) {
      const p = this.platforms.create(x, y, "platform");
      p.setOrigin(0, 0.5);
      p.displayWidth = width;
      p.displayHeight = 34;
      p.refreshBody();
      p.body.setSize(width, 28);
      p.body.setOffset(0, 3);
      p.setDepth(3);
      return p;
    }

    addBridge() {
      if (this.energy < 34) return;
      this.energy -= 34;

      const b = this.bridges.create(this.player.x + 50, this.player.y + 72, "bridge");
      b.setOrigin(0, 0.5);
      b.displayWidth = 160;
      b.displayHeight = 16;
      b.refreshBody();
      b.body.setSize(160, 14);
      b.life = 2.6;
      b.setDepth(4);

      this.tweens.add({
        targets: b,
        alpha: 0.35,
        yoyo: true,
        repeat: 4,
        duration: 180,
        onComplete: () => {
          if (b.active) b.destroy();
        }
      });
    }

    handlePress() {
      if (this.ended) {
        this.scene.restart();
        return;
      }

      if (!this.started) {
        this.started = true;
        this.hideTitle();
        return;
      }

      const grounded = this.player.body.touching.down || this.player.body.blocked.down;

      if (grounded) {
        this.player.setVelocityY(-555);
        this.spawnSpark(this.player.x, this.player.y + 18);
        return;
      }

      if (this.player.body.velocity.y > 130) {
        this.addBridge();
      }
    }

    onGround() {
      if (!this.started || this.ended) return;
      const nearLine = Math.abs(this.player.y - 292) < 75;
      if (nearLine) {
        this.combo = Phaser.Math.Clamp(this.combo + 1, 0, 99);
        this.bestCombo = Math.max(this.bestCombo, this.combo);
      }
    }

    collectSpool(spool) {
      spool.disableBody(true, true);
      this.spools += 1;
      this.combo += 1;
      this.bestCombo = Math.max(this.bestCombo, this.combo);
      this.energy = Phaser.Math.Clamp(this.energy + 16, 0, 100);
      this.score += 50;
      this.spawnSpark(spool.x, spool.y);
    }

    spawnPlatforms(dt) {
      this.nextPlatformX -= this.speed * dt;

      while (this.nextPlatformX < WIDTH + 80) {
        const gap = Phaser.Math.Between(120, 215);
        const width = Phaser.Math.Between(170, 290);
        const y = Phaser.Math.Between(315, 435);
        this.addPlatform(this.nextPlatformX + gap, y, width);
        this.nextPlatformX += gap + width;
      }
    }

    spawnNeedles(dt) {
      this.nextNeedle -= this.speed * dt;

      if (this.nextNeedle < WIDTH) {
        const needle = this.needles.create(WIDTH + 45, Phaser.Math.Between(245, 385), "needle");
        needle.setDepth(6);
        needle.body.setSize(18, 48);
        needle.body.setOffset(5, 5);
        needle.angle = Phaser.Math.Between(-10, 10);

        this.tweens.add({
          targets: needle,
          y: needle.y + Phaser.Math.Between(-32, 32),
          yoyo: true,
          repeat: -1,
          duration: Phaser.Math.Between(650, 1000)
        });

        this.nextNeedle = WIDTH + Phaser.Math.Between(360, 620);
      }
    }

    spawnSpools(dt) {
      this.nextSpools -= this.speed * dt;

      if (this.nextSpools < WIDTH) {
        const y = Phaser.Math.Between(175, 320);
        const amount = Phaser.Math.Between(3, 6);

        for (let i = 0; i < amount; i++) {
          const s = this.collectibles.create(WIDTH + 40 + i * 38, y + Math.sin(i) * 25, "spool");
          s.setDepth(7);
          s.body.setCircle(11, 2, 2);

          this.tweens.add({
            targets: s,
            y: s.y - 10,
            yoyo: true,
            repeat: -1,
            duration: 680 + i * 70
          });
        }

        this.nextSpools = WIDTH + Phaser.Math.Between(460, 720);
      }
    }

    scrollGroup(group, dt) {
      group.children.iterate((obj) => {
        if (!obj || !obj.active) return;
        obj.x -= this.speed * dt;

        if (typeof obj.life === "number") {
          obj.life -= dt;
          if (obj.life <= 0) obj.destroy();
        }
      });
    }

    cleanup(group) {
      group.children.iterate((obj) => {
        if (!obj || !obj.active) return;
        if (obj.x < -280) obj.destroy();
      });
    }

    spawnSpark(x, y) {
      for (let i = 0; i < 4; i++) {
        const dot = this.add.image(x, y, "spark");
        dot.setDepth(8);
        dot.setTint(0x7df9ff);
        this.tweens.add({
          targets: dot,
          x: x + Phaser.Math.Between(-28, 28),
          y: y + Phaser.Math.Between(-28, 28),
          alpha: 0,
          scale: 0,
          duration: 360,
          onComplete: () => dot.destroy()
        });
      }
    }

    updateTrail(dt) {
      const dot = this.add.circle(this.player.x - 18, this.player.y + 3, 5, 0x7df9ff, 0.42);
      dot.setDepth(5);
      this.trail.push(dot);

      if (this.trail.length > 18) {
        const old = this.trail.shift();
        if (old) old.destroy();
      }

      this.trail.forEach((d, index) => {
        d.x -= this.speed * dt;
        d.alpha = (index / this.trail.length) * 0.38;
        d.scale = Math.max(index / this.trail.length, 0.2);
      });

      this.player.rotation = Phaser.Math.Clamp(this.player.body.velocity.y / 900, -0.5, 0.8);
    }

    createUi() {
      this.scoreText = this.add.text(24, 20, "Score 0", {
        fontFamily: "Arial",
        fontSize: "24px",
        fontStyle: "bold",
        color: "#ffffff"
      }).setDepth(30);

      this.spoolText = this.add.text(24, 52, "Spools 0", {
        fontFamily: "Arial",
        fontSize: "17px",
        color: "#7df9ff"
      }).setDepth(30);

      this.comboText = this.add.text(24, 76, "Perfect Stitch x0", {
        fontFamily: "Arial",
        fontSize: "15px",
        color: "#ffb3d9"
      }).setDepth(30);

      this.add.text(720, 21, "STITCH ENERGY", {
        fontFamily: "Arial",
        fontSize: "13px",
        color: "#c8c5ff"
      }).setDepth(30);

      this.add.rectangle(720, 44, 210, 14, 0x1b1728, 1).setOrigin(0, 0.5).setDepth(30);
      this.energyBar = this.add.rectangle(720, 44, 210, 14, 0x7df9ff, 1).setOrigin(0, 0.5).setDepth(31);
    }

    updateUi() {
      this.scoreText.setText("Score " + Math.floor(this.score));
      this.spoolText.setText("Spools " + this.spools);
      this.comboText.setText("Perfect Stitch x" + this.combo);
      this.energyBar.displayWidth = 210 * (this.energy / 100);
    }

    createTitle() {
      this.title = this.add.text(WIDTH / 2, 135, "THREAD RUNNER", {
        fontFamily: "Arial",
        fontSize: "58px",
        fontStyle: "900",
        color: "#ffffff",
        stroke: "#7b3df2",
        strokeThickness: 8
      }).setOrigin(0.5).setDepth(50);

      this.subtitle = this.add.text(WIDTH / 2, 198, "STITCH RUSH", {
        fontFamily: "Arial",
        fontSize: "26px",
        fontStyle: "bold",
        color: "#7df9ff"
      }).setOrigin(0.5).setDepth(50);

      this.instructions = this.add.text(
        WIDTH / 2,
        292,
        "SPACE / UP / CLICK / TAP to start and jump\nHold while airborne to float with thread energy\nTap while falling to stitch a temporary bridge\nCollect spools. Dodge needles.",
        {
          fontFamily: "Arial",
          fontSize: "20px",
          color: "#f4efff",
          align: "center",
          lineSpacing: 10
        }
      ).setOrigin(0.5).setDepth(50);

      this.tweens.add({
        targets: this.instructions,
        alpha: 0.55,
        yoyo: true,
        repeat: -1,
        duration: 850
      });
    }

    hideTitle() {
      this.tweens.add({
        targets: [this.title, this.subtitle, this.instructions],
        alpha: 0,
        duration: 180
      });
    }

    gameOver() {
      if (this.ended) return;

      this.ended = true;
      this.started = false;
      this.player.setVelocity(0, 0);
      this.player.setTint(0xff4f8b);

      const panel = this.add.rectangle(WIDTH / 2, HEIGHT / 2, 540, 270, 0x090812, 0.9)
        .setStrokeStyle(3, 0x7df9ff, 0.85)
        .setDepth(80);

      this.add.text(
        WIDTH / 2,
        HEIGHT / 2 - 42,
        "Run Unravelled\nScore: " + Math.floor(this.score) + "\nSpools: " + this.spools + "\nBest Combo: x" + this.bestCombo,
        {
          fontFamily: "Arial",
          fontSize: "28px",
          fontStyle: "bold",
          color: "#ffffff",
          align: "center",
          lineSpacing: 8
        }
      ).setOrigin(0.5).setDepth(81);

      this.add.text(WIDTH / 2, HEIGHT / 2 + 88, "Press R or tap to restart", {
        fontFamily: "Arial",
        fontSize: "20px",
        color: "#7df9ff"
      }).setOrigin(0.5).setDepth(81);
    }
  }

  const config = {
    type: Phaser.AUTO,
    parent: "game-container",
    width: WIDTH,
    height: HEIGHT,
    backgroundColor: "#0b0a12",
    physics: {
      default: "arcade",
      arcade: {
        gravity: { y: 1350 },
        debug: false
      }
    },
    scale: {
      mode: Phaser.Scale.FIT,
      autoCenter: Phaser.Scale.CENTER_BOTH
    },
    scene: ThreadRunner
  };

  new Phaser.Game(config);
})();
