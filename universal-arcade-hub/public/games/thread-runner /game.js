(() => {
  if (!window.Phaser) return;

  const W = 960;
  const H = 540;
  const clamp = Phaser.Math.Clamp;
  const SAVE_KEY = "thread-runner-v5-safe-save";

  class TinyAudio {
    constructor(scene) {
      this.scene = scene;
      this.ctx = null;
      this.enabled = true;
      this.musicEnabled = true;
      this.nextBeat = 0;
      this.note = 0;
    }

    ensure() {
      if (!this.ctx) {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return false;
        this.ctx = new AudioContext();
      }
      if (this.ctx.state === "suspended") this.ctx.resume();
      return true;
    }

    beep(freq, duration, type = "sine", volume = 0.045) {
      if (!this.enabled || !this.ensure()) return;
      const now = this.ctx.currentTime;
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = type;
      osc.frequency.setValueAtTime(freq, now);
      gain.gain.setValueAtTime(0.001, now);
      gain.gain.exponentialRampToValueAtTime(volume, now + 0.01);
      gain.gain.exponentialRampToValueAtTime(0.001, now + duration);
      osc.connect(gain);
      gain.connect(this.ctx.destination);
      osc.start(now);
      osc.stop(now + duration + 0.02);
    }

    sfx(name) {
      if (!this.enabled) return;
      if (name === "jump") this.beep(420, 0.08, "triangle", 0.04);
      if (name === "spool") {
        this.beep(760, 0.05, "sine", 0.035);
        setTimeout(() => this.beep(940, 0.05, "sine", 0.03), 35);
      }
      if (name === "stitch") this.beep(300, 0.11, "sawtooth", 0.035);
      if (name === "dash") this.beep(220, 0.12, "square", 0.03);
      if (name === "power") {
        this.beep(520, 0.07, "sine", 0.035);
        setTimeout(() => this.beep(720, 0.08, "sine", 0.03), 60);
      }
      if (name === "hit") this.beep(120, 0.18, "sawtooth", 0.045);
      if (name === "unlock") {
        [523, 659, 784].forEach((f, i) => setTimeout(() => this.beep(f, 0.09, "triangle", 0.035), i * 70));
      }
      if (name === "click") this.beep(620, 0.04, "sine", 0.025);
    }

    music(timeMs, intensity) {
      if (!this.enabled || !this.musicEnabled || !this.ensure()) return;
      if (timeMs < this.nextBeat) return;
      const notes = [196, 233, 262, 294, 349, 392, 466, 523];
      const freq = notes[this.note % notes.length];
      this.beep(freq, 0.08, "triangle", 0.012);
      this.note++;
      this.nextBeat = timeMs + Math.max(150, 300 - intensity * 20);
    }
  }

  class ThreadRunner extends Phaser.Scene {
    constructor() {
      super("ThreadRunnerV5Safe");
    }

    create() {
      this.loadSave();
      this.audio = new TinyAudio(this);
      this.resetState();
      this.buildWorld();
      this.buildPlayer();
      this.buildUi();
      this.bindInput();
      this.showMenu();
    }

    loadSave() {
      try {
        const raw = localStorage.getItem(SAVE_KEY);
        this.save = raw ? JSON.parse(raw) : {};
      } catch {
        this.save = {};
      }

      this.save.bestScore = this.save.bestScore || 0;
      this.save.bestDistance = this.save.bestDistance || 0;
      this.save.bestCombo = this.save.bestCombo || 0;
      this.save.totalSpools = this.save.totalSpools || 0;
      this.save.audio = this.save.audio !== false;
      this.save.skin = this.save.skin || "Neon";
      this.save.theme = this.save.theme || "Cotton";
      this.save.unlockedSkins = this.save.unlockedSkins || ["Neon"];
      this.save.unlockedThemes = this.save.unlockedThemes || ["Cotton"];
      this.save.tutorialSeen = this.save.tutorialSeen || false;
    }

    writeSave() {
      try {
        localStorage.setItem(SAVE_KEY, JSON.stringify(this.save));
      } catch {}
    }

    resetState() {
      this.mode = "menu";
      this.started = false;
      this.pausedGame = false;
      this.dead = false;
      this.runSaved = false;

      this.score = 0;
      this.distance = 0;
      this.spools = 0;
      this.combo = 0;
      this.bestCombo = 0;
      this.energy = 100;
      this.speed = 290;
      this.level = 1;

      this.vy = 0;
      this.holding = false;
      this.shield = false;
      this.magnetTimer = 0;
      this.slowTimer = 0;
      this.dashCooldown = 0;
      this.dashTimer = 0;
      this.ghostTimer = 0;
      this.comboFlashTimer = 0;

      this.nextPlatformTimer = 0.45;
      this.nextNeedleTimer = 1.15;
      this.nextSpoolTimer = 1.0;
      this.nextPowerTimer = 4.2;
      this.nextHazardTimer = 3.2;

      this.platforms = [];
      this.needles = [];
      this.scissors = [];
      this.rips = [];
      this.spoolItems = [];
      this.bridges = [];
      this.powerups = [];
      this.bgItems = [];
      this.trail = [];
      this.textPopups = [];
      this.confetti = [];

      this.missions = [
        { id: "spools", label: "Collect 25 spools", target: 25, value: 0, done: false, reward: "Blue Stitch Skin", unlockSkin: "Blue" },
        { id: "combo", label: "Reach combo x15", target: 15, value: 0, done: false, reward: "Gold Thread Skin", unlockSkin: "Gold" },
        { id: "distance", label: "Run 500m", target: 500, value: 0, done: false, reward: "Denim Theme", unlockTheme: "Denim" }
      ];
    }

    buildWorld() {
      this.cameras.main.setBackgroundColor("#090812");
      this.zoneGradient = this.add.rectangle(W / 2, H / 2, W, H, 0x090812).setDepth(-30);

      for (let i = 0; i < 56; i++) {
        const line = this.add.rectangle(
          Phaser.Math.Between(0, W),
          Phaser.Math.Between(38, H - 38),
          Phaser.Math.Between(70, 255),
          2,
          0xffffff,
          Phaser.Math.FloatBetween(0.06, 0.2)
        );
        line.speed = Phaser.Math.Between(16, 78);
        line.setDepth(-10);
        this.bgItems.push(line);
      }

      for (let i = 0; i < 26; i++) {
        const dash = this.add.rectangle(i * 44, 292, 24, 3, 0xffffff, 0.18);
        dash.speed = 45;
        dash.setDepth(1);
        this.bgItems.push(dash);
      }

      for (let i = 0; i < 8; i++) {
        const star = this.add.star(Phaser.Math.Between(0, W), Phaser.Math.Between(40, H - 60), 5, 3, 6, 0x7df9ff, 0.16);
        star.speed = Phaser.Math.Between(12, 35);
        star.setDepth(-8);
        this.bgItems.push(star);
      }

      this.addPlatform(0, 410, 330, "#21172e");
      this.addPlatform(380, 370, 250, "#21172e");
      this.addPlatform(700, 410, 260, "#21172e");
    }

    buildPlayer() {
      this.playerGlow = this.add.circle(160, 260, 34, 0x7df9ff, 0.13).setDepth(18);
      this.player = this.add.circle(160, 260, 18, this.skinColor()).setDepth(20);
      this.playerStroke = this.add.circle(160, 260, 18).setStrokeStyle(4, 0x7df9ff, 1).setDepth(21);
      this.playerEye = this.add.circle(168, 253, 3, 0xffffff).setDepth(22);
      this.shieldRing = this.add.circle(160, 260, 32).setStrokeStyle(4, 0xffd45e, 0).setDepth(23);
      this.ghostRing = this.add.circle(160, 260, 39).setStrokeStyle(3, 0xff7df2, 0).setDepth(22);
    }

    skinColor() {
      if (this.save.skin === "Blue") return 0x2ee6ff;
      if (this.save.skin === "Gold") return 0xffdf7d;
      if (this.save.skin === "Ghost") return 0xd6c6ff;
      return 0x7b3df2;
    }

    buildUi() {
      this.scoreText = this.add.text(24, 22, "Score 0", {
        fontFamily: "Arial",
        fontSize: "25px",
        fontStyle: "bold",
        color: "#ffffff"
      }).setDepth(90);

      this.spoolText = this.add.text(24, 55, "Spools 0", {
        fontFamily: "Arial",
        fontSize: "17px",
        color: "#7df9ff"
      }).setDepth(90);

      this.comboText = this.add.text(24, 80, "Combo x0", {
        fontFamily: "Arial",
        fontSize: "16px",
        color: "#ffb3d9"
      }).setDepth(90);

      this.levelText = this.add.text(W / 2, 24, "Cotton Run", {
        fontFamily: "Arial",
        fontSize: "17px",
        fontStyle: "bold",
        color: "#c8c5ff"
      }).setOrigin(0.5).setDepth(90);

      this.add.text(720, 21, "STITCH ENERGY", {
        fontFamily: "Arial",
        fontSize: "13px",
        color: "#c8c5ff"
      }).setDepth(90);

      this.add.rectangle(720, 44, 210, 14, 0x1b1728, 1).setOrigin(0, 0.5).setDepth(90);
      this.energyBar = this.add.rectangle(720, 44, 210, 14, 0x7df9ff, 1).setOrigin(0, 0.5).setDepth(91);

      this.statusText = this.add.text(720, 66, "", {
        fontFamily: "Arial",
        fontSize: "13px",
        color: "#ffdf7d"
      }).setDepth(90);

      this.dashText = this.add.text(720, 86, "SHIFT = Dash", {
        fontFamily: "Arial",
        fontSize: "13px",
        color: "#a9a3cf"
      }).setDepth(90);

      this.missionPanel = this.add.container(24, 124).setDepth(88);
      this.missionBg = this.add.rectangle(0, 0, 292, 96, 0x090812, 0.45).setOrigin(0, 0).setStrokeStyle(1, 0x7df9ff, 0.35);
      this.missionTitle = this.add.text(12, 9, "CHALLENGES", {
        fontFamily: "Arial",
        fontSize: "12px",
        fontStyle: "bold",
        color: "#7df9ff"
      });
      this.missionLines = this.missions.map((m, idx) =>
        this.add.text(12, 31 + idx * 20, "", {
          fontFamily: "Arial",
          fontSize: "13px",
          color: "#f4efff"
        })
      );
      this.missionPanel.add([this.missionBg, this.missionTitle, ...this.missionLines]);

      this.pauseText = this.add.text(W - 24, H - 24, "P Pause · M Audio · N Skin · T Theme", {
        fontFamily: "Arial",
        fontSize: "14px",
        color: "#a9a3cf"
      }).setOrigin(1, 1).setDepth(90);
    }

    bindInput() {
      this.keys = this.input.keyboard.addKeys({
        space: Phaser.Input.Keyboard.KeyCodes.SPACE,
        up: Phaser.Input.Keyboard.KeyCodes.UP,
        shift: Phaser.Input.Keyboard.KeyCodes.SHIFT,
        r: Phaser.Input.Keyboard.KeyCodes.R,
        p: Phaser.Input.Keyboard.KeyCodes.P,
        h: Phaser.Input.Keyboard.KeyCodes.H,
        m: Phaser.Input.Keyboard.KeyCodes.M,
        n: Phaser.Input.Keyboard.KeyCodes.N,
        t: Phaser.Input.Keyboard.KeyCodes.T
      });

      this.input.on("pointerdown", () => {
        this.holding = true;
        this.press();
      });

      this.input.on("pointerup", () => {
        this.holding = false;
      });

      this.input.keyboard.on("keydown-SPACE", () => this.press());
      this.input.keyboard.on("keydown-UP", () => this.press());
      this.input.keyboard.on("keydown-SHIFT", () => this.dash());
      this.input.keyboard.on("keydown-H", () => {
        if (this.mode === "menu") this.showTutorial();
      });

      this.input.keyboard.on("keydown-P", () => {
        if (this.started && !this.dead) this.togglePause();
      });

      this.input.keyboard.on("keydown-R", () => {
        if (this.dead) this.scene.restart();
      });

      this.input.keyboard.on("keydown-M", () => this.toggleAudio());
      this.input.keyboard.on("keydown-N", () => this.cycleSkin());
      this.input.keyboard.on("keydown-T", () => this.cycleTheme());
    }

    clearOverlay() {
      if (this.menuGroup) this.menuGroup.destroy();
      if (this.tutorialGroup) this.tutorialGroup.destroy();
      if (this.pauseOverlay) this.pauseOverlay.destroy();
      this.menuGroup = null;
      this.tutorialGroup = null;
      this.pauseOverlay = null;
    }

    showMenu() {
      this.mode = "menu";
      this.clearOverlay();

      this.menuGroup = this.add.container(0, 0).setDepth(160);

      const panel = this.add.rectangle(W / 2, H / 2, 730, 420, 0x090812, 0.88)
        .setStrokeStyle(3, 0x7df9ff, 0.55);

      const title = this.add.text(W / 2, 92, "THREAD RUNNER", {
        fontFamily: "Arial",
        fontSize: "60px",
        fontStyle: "900",
        color: "#ffffff",
        stroke: "#7b3df2",
        strokeThickness: 8
      }).setOrigin(0.5);

      const subtitle = this.add.text(W / 2, 150, "STITCH RUSH V5 SAFE", {
        fontFamily: "Arial",
        fontSize: "24px",
        fontStyle: "bold",
        color: "#7df9ff"
      }).setOrigin(0.5);

      const stats = this.add.text(
        W / 2,
        220,
        "Best Score: " + this.save.bestScore +
        "  ·  Best Distance: " + this.save.bestDistance + "m" +
        "  ·  Best Combo: x" + this.save.bestCombo +
        "\nTotal Spools: " + this.save.totalSpools +
        "\nSkin: " + this.save.skin +
        "  ·  Theme: " + this.save.theme +
        "  ·  Audio: " + (this.save.audio ? "On" : "Off"),
        {
          fontFamily: "Arial",
          fontSize: "17px",
          color: "#f4efff",
          align: "center",
          lineSpacing: 8
        }
      ).setOrigin(0.5);

      const controls = this.add.text(
        W / 2,
        326,
        "SPACE / CLICK = Play    H = Tutorial    N = Skin    T = Theme    M = Audio",
        {
          fontFamily: "Arial",
          fontSize: "16px",
          color: "#d8d2ff",
          align: "center"
        }
      ).setOrigin(0.5);

      const prompt = this.add.text(W / 2, 398, "Press SPACE or click to play", {
        fontFamily: "Arial",
        fontSize: "19px",
        fontStyle: "bold",
        color: "#ffdf7d"
      }).setOrigin(0.5);

      this.tweens.add({ targets: prompt, alpha: 0.45, yoyo: true, repeat: -1, duration: 760 });

      this.menuGroup.add([panel, title, subtitle, stats, controls, prompt]);
    }

    showTutorial() {
      this.audio.enabled = this.save.audio;
      this.audio.sfx("click");
      this.clearOverlay();
      this.mode = "tutorial";

      this.tutorialGroup = this.add.container(0, 0).setDepth(170);
      const panel = this.add.rectangle(W / 2, H / 2, 720, 410, 0x090812, 0.92)
        .setStrokeStyle(3, 0x7df9ff, 0.65);

      const title = this.add.text(W / 2, 104, "HOW TO RUN", {
        fontFamily: "Arial",
        fontSize: "38px",
        fontStyle: "900",
        color: "#ffffff"
      }).setOrigin(0.5);

      const body = this.add.text(
        W / 2,
        260,
        "1. Jump between stitched fabric platforms.\n" +
        "2. Hold SPACE / UP / mouse to float using stitch energy.\n" +
        "3. Tap while falling to stitch a temporary bridge.\n" +
        "4. SHIFT dashes and briefly phases through hazards.\n" +
        "5. Collect spools and power-ups to complete challenges.\n\n" +
        "Power-ups: Shield blocks one hit · Magnet pulls spools · Time slows the world · Ghost phases through hazards.",
        {
          fontFamily: "Arial",
          fontSize: "18px",
          color: "#f4efff",
          align: "center",
          lineSpacing: 9
        }
      ).setOrigin(0.5);

      const prompt = this.add.text(W / 2, 452, "Press SPACE / click to start", {
        fontFamily: "Arial",
        fontSize: "18px",
        fontStyle: "bold",
        color: "#ffdf7d"
      }).setOrigin(0.5);

      this.tutorialGroup.add([panel, title, body, prompt]);
      this.save.tutorialSeen = true;
      this.writeSave();
    }

    startGame() {
      this.audio.enabled = this.save.audio;
      this.audio.ensure();
      this.mode = "playing";
      this.started = true;
      this.clearOverlay();
      this.audio.sfx("click");
    }

    togglePause() {
      this.pausedGame = !this.pausedGame;

      if (this.pausedGame) {
        this.pauseOverlay = this.add.container(0, 0).setDepth(200);
        this.pauseOverlay.add([
          this.add.rectangle(W / 2, H / 2, W, H, 0x090812, 0.58),
          this.add.text(W / 2, H / 2, "PAUSED\nPress P to resume", {
            fontFamily: "Arial",
            fontSize: "34px",
            fontStyle: "bold",
            color: "#ffffff",
            align: "center",
            lineSpacing: 12
          }).setOrigin(0.5)
        ]);
      } else if (this.pauseOverlay) {
        this.pauseOverlay.destroy();
      }
    }

    toggleAudio() {
      this.save.audio = !this.save.audio;
      this.audio.enabled = this.save.audio;
      this.writeSave();
      this.popText(this.save.audio ? "AUDIO ON" : "AUDIO OFF", W / 2, 78, "#ffdf7d");
      if (this.mode === "menu") this.showMenu();
    }

    cycleSkin() {
      const available = this.save.unlockedSkins.slice();
      const idx = available.indexOf(this.save.skin);
      this.save.skin = available[(idx + 1) % available.length] || "Neon";
      this.player.fillColor = this.skinColor();
      this.writeSave();
      this.popText("SKIN: " + this.save.skin, W / 2, 78, "#7df9ff");
      if (this.mode === "menu") this.showMenu();
    }

    cycleTheme() {
      const available = this.save.unlockedThemes.slice();
      const idx = available.indexOf(this.save.theme);
      this.save.theme = available[(idx + 1) % available.length] || "Cotton";
      this.writeSave();
      this.zoneFlash();
      this.popText("THEME: " + this.save.theme, W / 2, 78, "#7df9ff");
      if (this.mode === "menu") this.showMenu();
    }

    update(_time, deltaMs) {
      const dt = Math.min(deltaMs / 1000, 0.033);

      this.animateBackground(dt);

      if (this.save.audio && this.mode === "playing") {
        this.audio.music(this.time.now, this.level);
      }

      if (!this.started || this.dead || this.pausedGame) return;

      this.updateDifficulty(dt);
      this.updatePlayer(dt);
      this.moveWorld(dt);
      this.spawnWorld(dt);
      this.collisions();
      this.updateEffects(dt);
      this.updateMissions();
      this.updateHud();

      if (this.player.y > H + 100) this.endGame("Unravelled off-screen");
    }

    updateDifficulty(dt) {
      this.distance += this.speed * dt * 0.1;

      const baseSpeed = this.slowTimer > 0 ? 230 : 290;
      this.speed = clamp(baseSpeed + this.distance * 0.47, baseSpeed, this.slowTimer > 0 ? 385 : 575);

      const newLevel = Math.floor(this.distance / 170) + 1;
      if (newLevel !== this.level) {
        this.level = newLevel;
        this.zoneFlash();
      }

      this.score += this.speed * dt * 0.12 + this.combo * dt * 1.5;
    }

    updatePlayer(dt) {
      const holding = this.holding || this.keys.space.isDown || this.keys.up.isDown;

      if (this.dashTimer > 0) {
        this.dashTimer -= dt;
        this.addTrail(true, 0xffdf7d);
      }

      this.vy += 1350 * dt;

      if (holding && !this.isGrounded() && this.energy > 0) {
        this.vy = Math.max(this.vy - 1500 * dt, -430);
        this.energy -= 33 * dt;
        this.addTrail(true);
      } else {
        this.energy += 25 * dt;
        this.addTrail(false);
      }

      this.energy = clamp(this.energy, 0, 100);

      this.player.y += this.vy * dt;
      this.syncPlayerParts();
      this.handlePlatformCollision();

      this.playerGlow.alpha = 0.10 + Math.sin(this.time.now * 0.008) * 0.04;
      this.playerStroke.rotation += 0.045;
      this.shieldRing.alpha = this.shield ? 0.9 : 0;
      this.shieldRing.rotation += 0.04;
      this.ghostRing.alpha = this.ghostTimer > 0 ? 0.55 : 0;
      this.ghostRing.rotation -= 0.035;
    }

    syncPlayerParts() {
      this.player.x = 160;
      this.playerGlow.x = this.player.x;
      this.playerGlow.y = this.player.y;
      this.playerStroke.x = this.player.x;
      this.playerStroke.y = this.player.y;
      this.playerEye.x = this.player.x + 8;
      this.playerEye.y = this.player.y - 7;
      this.shieldRing.x = this.player.x;
      this.shieldRing.y = this.player.y;
      this.ghostRing.x = this.player.x;
      this.ghostRing.y = this.player.y;
    }

    moveWorld(dt) {
      this.moveArray(this.platforms, dt);
      this.moveArray(this.needles, dt);
      this.moveArray(this.scissors, dt);
      this.moveArray(this.rips, dt);
      this.moveArray(this.spoolItems, dt);
      this.moveArray(this.bridges, dt);
      this.moveArray(this.powerups, dt);
      this.moveTrail(dt);
      this.movePopups(dt);
      this.moveConfetti(dt);
    }

    spawnWorld(dt) {
      this.nextPlatformTimer -= dt;
      this.nextNeedleTimer -= dt;
      this.nextSpoolTimer -= dt;
      this.nextPowerTimer -= dt;
      this.nextHazardTimer -= dt;

      if (this.nextPlatformTimer <= 0) {
        const pattern = Phaser.Math.Between(1, 5);

        if (pattern === 1) {
          this.addPlatform(W + 80, Phaser.Math.Between(320, 435), Phaser.Math.Between(175, 300), this.zoneColor());
        } else if (pattern === 2) {
          const y = Phaser.Math.Between(325, 420);
          this.addPlatform(W + 80, y, 160, this.zoneColor());
          this.addPlatform(W + 300, y - Phaser.Math.Between(35, 70), 150, this.zoneColor());
        } else {
          this.addPlatform(W + 80, Phaser.Math.Between(320, 435), Phaser.Math.Between(190, 280), this.zoneColor());
        }

        this.nextPlatformTimer = Phaser.Math.FloatBetween(0.74, 1.15);
      }

      if (this.nextNeedleTimer <= 0) {
        this.addNeedle(W + 70, Phaser.Math.Between(245, 395));
        const min = Math.max(0.74, 1.38 - this.level * 0.045);
        const max = Math.max(1.08, 2.05 - this.level * 0.055);
        this.nextNeedleTimer = Phaser.Math.FloatBetween(min, max);
      }

      if (this.nextHazardTimer <= 0 && this.level >= 2) {
        const type = Phaser.Utils.Array.GetRandom(["scissor", "rip"]);
        if (type === "scissor") this.addScissor(W + 80, Phaser.Math.Between(190, 330));
        if (type === "rip") this.addRip(W + 80, Phaser.Math.Between(355, 448));
        this.nextHazardTimer = Phaser.Math.FloatBetween(3.2, 5.4);
      }

      if (this.nextSpoolTimer <= 0) {
        const y = Phaser.Math.Between(175, 320);
        const amount = Phaser.Math.Between(3, 7);
        const arc = Phaser.Math.Between(0, 1) === 1;

        for (let i = 0; i < amount; i++) {
          const yy = arc ? y + Math.sin(i / Math.max(1, amount - 1) * Math.PI) * -55 : y + Math.sin(i) * 22;
          this.addSpool(W + 75 + i * 38, yy);
        }

        this.nextSpoolTimer = Phaser.Math.FloatBetween(1.15, 2.0);
      }

      if (this.nextPowerTimer <= 0) {
        const type = Phaser.Utils.Array.GetRandom(["shield", "magnet", "slow", "ghost"]);
        this.addPowerup(W + 90, Phaser.Math.Between(185, 305), type);
        this.nextPowerTimer = Phaser.Math.FloatBetween(5.2, 7.6);
      }
    }

    animateBackground(dt) {
      for (const item of this.bgItems) {
        item.x -= item.speed * dt;
        if (item.x < -280) item.x = W + Phaser.Math.Between(50, 230);
      }
    }

    addPlatform(x, y, width, color = "#21172e") {
      const c = Phaser.Display.Color.HexStringToColor(color).color;
      const rect = this.add.rectangle(x, y, width, 34, c).setOrigin(0, 0.5).setDepth(5);
      rect.setStrokeStyle(3, 0x7df9ff, 0.56);

      const stitches = [];
      for (let sx = 18; sx < width - 12; sx += 34) {
        const dash = this.add.rectangle(x + sx, y, 16, 2, 0xffffff, 0.32).setDepth(6);
        stitches.push(dash);
      }

      this.platforms.push({ kind: "platform", obj: rect, stitches, x, y, width, height: 34 });
    }

    addBridge(x, y) {
      const rect = this.add.rectangle(x, y, 172, 16, 0x1bffe4).setOrigin(0, 0.5).setDepth(8);
      rect.setStrokeStyle(2, 0xffffff, 0.55);
      this.bridges.push({ kind: "bridge", obj: rect, x, y, width: 172, height: 16, life: 2.4 });
      this.popText("STITCH!", x + 70, y - 28, "#7df9ff");
      this.audio.sfx("stitch");
    }

    addNeedle(x, y) {
      const tri = this.add.triangle(x, y, 15, 0, 30, 62, 0, 62, 0xff4f8b).setDepth(12);
      tri.setStrokeStyle(2, 0xffffff, 0.45);
      tri.angle = Phaser.Math.Between(-8, 8);
      this.needles.push({ kind: "needle", obj: tri, x, y, width: 30, height: 62, pulse: Phaser.Math.FloatBetween(0, 10) });
    }

    addScissor(x, y) {
      const group = this.add.container(x, y).setDepth(13);
      const bladeA = this.add.rectangle(0, -8, 48, 8, 0xded8ff).setAngle(30);
      const bladeB = this.add.rectangle(0, 8, 48, 8, 0xded8ff).setAngle(-30);
      const handleA = this.add.circle(-28, -14, 9).setStrokeStyle(3, 0xff4f8b);
      const handleB = this.add.circle(-28, 14, 9).setStrokeStyle(3, 0xff4f8b);
      group.add([bladeA, bladeB, handleA, handleB]);
      this.scissors.push({ kind: "scissor", obj: group, x, y, width: 70, height: 46, spin: Phaser.Math.FloatBetween(-1, 1) });
    }

    addRip(x, y) {
      const group = this.add.container(x, y).setDepth(7);
      const hole = this.add.ellipse(0, 0, 112, 34, 0x020106, 0.96);
      const edge = this.add.ellipse(0, 0, 116, 38).setStrokeStyle(3, 0xff7df2, 0.55);
      group.add([hole, edge]);
      this.rips.push({ kind: "rip", obj: group, x, y, width: 112, height: 34 });
    }

    addSpool(x, y) {
      const outer = this.add.circle(x, y, 12, 0x29f2ff).setDepth(14);
      const inner = this.add.circle(x, y, 5, 0x090812).setDepth(15);
      const ring = this.add.circle(x, y, 13).setStrokeStyle(2, 0xffffff, 0.55).setDepth(16);
      this.spoolItems.push({ kind: "spool", outer, inner, ring, x, y, radius: 16, bob: Phaser.Math.FloatBetween(0, 6.28) });
    }

    addPowerup(x, y, type) {
      const colors = {
        shield: 0xffd45e,
        magnet: 0xff7df2,
        slow: 0x7df9ff,
        ghost: 0xa98cff
      };

      const labels = {
        shield: "S",
        magnet: "M",
        slow: "T",
        ghost: "G"
      };

      const outer = this.add.circle(x, y, 17, colors[type]).setDepth(17);
      const ring = this.add.circle(x, y, 22).setStrokeStyle(3, colors[type], 0.55).setDepth(16);
      const inner = this.add.text(x, y, labels[type], {
        fontFamily: "Arial",
        fontSize: "18px",
        fontStyle: "bold",
        color: "#090812"
      }).setOrigin(0.5).setDepth(18);

      this.powerups.push({ kind: "power", outer, ring, inner, x, y, radius: 24, type, bob: Phaser.Math.FloatBetween(0, 6.28) });
    }

    moveArray(arr, dt) {
      for (let i = arr.length - 1; i >= 0; i--) {
        const item = arr[i];

        item.x -= this.speed * dt;

        if (item.kind === "needle") {
          item.y += Math.sin(this.time.now * 0.004 + item.pulse) * 0.08;
          item.obj.rotation += 0.005;
        }

        if (item.kind === "scissor") {
          item.obj.rotation += (0.018 + Math.abs(item.spin) * 0.012);
        }

        if (item.kind === "spool" || item.kind === "power") {
          const yOffset = Math.sin(this.time.now * 0.006 + item.bob) * 4;
          if (item.outer) {
            item.outer.x = item.x;
            item.outer.y = item.y + yOffset;
          }
          if (item.inner) {
            item.inner.x = item.x;
            item.inner.y = item.y + yOffset;
          }
          if (item.ring) {
            item.ring.x = item.x;
            item.ring.y = item.y + yOffset;
            item.ring.rotation += 0.04;
          }
        }

        if (item.obj) {
          item.obj.x = item.x;
          item.obj.y = item.y;
        }

        if (item.stitches) {
          for (let s = 0; s < item.stitches.length; s++) {
            item.stitches[s].x = item.x + 18 + s * 34;
            item.stitches[s].y = item.y;
          }
        }

        if (typeof item.life === "number") {
          item.life -= dt;
          item.obj.alpha = Math.max(0.22, item.life / 2.4);
          if (item.life <= 0) {
            this.destroyItem(item);
            arr.splice(i, 1);
            continue;
          }
        }

        if (item.x < -380) {
          this.destroyItem(item);
          arr.splice(i, 1);
        }
      }
    }

    destroyItem(item) {
      if (item.obj) item.obj.destroy();
      if (item.outer) item.outer.destroy();
      if (item.inner) item.inner.destroy();
      if (item.ring) item.ring.destroy();
      if (item.stitches) item.stitches.forEach((s) => s.destroy());
    }

    moveTrail(dt) {
      for (let i = this.trail.length - 1; i >= 0; i--) {
        const t = this.trail[i];
        t.x -= this.speed * dt;
        t.life -= dt;
        t.obj.x = t.x;
        t.obj.y = t.y;
        t.obj.alpha = Math.max(0, t.life / 0.7) * t.baseAlpha;
        t.obj.scale = Math.max(0.2, t.life / 0.7);

        if (t.life <= 0 || t.x < -40) {
          t.obj.destroy();
          this.trail.splice(i, 1);
        }
      }
    }

    movePopups(dt) {
      for (let i = this.textPopups.length - 1; i >= 0; i--) {
        const p = this.textPopups[i];
        p.life -= dt;
        p.obj.y -= 30 * dt;
        p.obj.alpha = Math.max(0, p.life / 0.75);
        if (p.life <= 0) {
          p.obj.destroy();
          this.textPopups.splice(i, 1);
        }
      }
    }

    moveConfetti(dt) {
      for (let i = this.confetti.length - 1; i >= 0; i--) {
        const c = this.confetti[i];
        c.life -= dt;
        c.vy += 300 * dt;
        c.x += c.vx * dt;
        c.y += c.vy * dt;
        c.obj.x = c.x;
        c.obj.y = c.y;
        c.obj.rotation += c.spin * dt;
        c.obj.alpha = Math.max(0, c.life / 1.2);

        if (c.life <= 0) {
          c.obj.destroy();
          this.confetti.splice(i, 1);
        }
      }
    }

    addTrail(boosting, color = 0x7df9ff) {
      if (this.time.now % 2 > 1) return;
      const dot = this.add.circle(
        this.player.x - 18,
        this.player.y + 3,
        boosting ? 6 : 4,
        boosting ? color : this.skinColor(),
        boosting ? 0.52 : 0.33
      ).setDepth(7);
      this.trail.push({ obj: dot, x: dot.x, y: dot.y, life: 0.7, baseAlpha: boosting ? 0.52 : 0.33 });
    }

    isGrounded() {
      const items = this.platforms.concat(this.bridges);
      for (const p of items) {
        const playerBottom = this.player.y + 18;
        const withinX = this.player.x + 17 > p.x && this.player.x - 17 < p.x + p.width;
        const closeY = playerBottom >= p.y - p.height / 2 - 5 && playerBottom <= p.y + p.height / 2 + 12;
        if (withinX && closeY && this.vy >= 0) return true;
      }
      return false;
    }

    handlePlatformCollision() {
      const items = this.platforms.concat(this.bridges);
      for (const p of items) {
        const playerBottom = this.player.y + 18;
        const withinX = this.player.x + 17 > p.x && this.player.x - 17 < p.x + p.width;
        const fallingOnto = playerBottom >= p.y - p.height / 2 && playerBottom <= p.y + p.height / 2 + 14;

        if (withinX && fallingOnto && this.vy >= 0) {
          this.player.y = p.y - p.height / 2 - 18;
          this.vy = 0;
          this.syncPlayerParts();

          if (Math.abs(this.player.y - 292) < 84) {
            this.combo = clamp(this.combo + 0.016, 0, 99);
          }
          return;
        }
      }
    }

    collisions() {
      this.checkNeedles();
      this.checkScissors();
      this.checkRips();
      this.checkSpools();
      this.checkPowerups();
    }

    hitHazard(item, arr, index, label = "HIT") {
      if (this.ghostTimer > 0 || this.dashTimer > 0) {
        this.popText("PHASED", this.player.x + 34, this.player.y - 36, "#ff7df2");
        return false;
      }

      if (this.shield) {
        this.shield = false;
        this.popText("SHIELD BROKE", this.player.x + 20, this.player.y - 34, "#ffdf7d");
        this.cameras.main.shake(120, 0.004);
        this.audio.sfx("hit");
        this.destroyItem(item);
        arr.splice(index, 1);
        return false;
      }

      this.endGame(label);
      return true;
    }

    checkNeedles() {
      for (let i = this.needles.length - 1; i >= 0; i--) {
        const n = this.needles[i];
        const d = Phaser.Math.Distance.Between(this.player.x, this.player.y, n.x + 6, n.y + 24);
        if (d < 33) {
          if (this.hitHazard(n, this.needles, i, "Needle caught the thread")) return;
        }
      }
    }

    checkScissors() {
      for (let i = this.scissors.length - 1; i >= 0; i--) {
        const s = this.scissors[i];
        const d = Phaser.Math.Distance.Between(this.player.x, this.player.y, s.x, s.y);
        if (d < 45) {
          if (this.hitHazard(s, this.scissors, i, "Snipped by scissors")) return;
        }
      }
    }

    checkRips() {
      for (let i = this.rips.length - 1; i >= 0; i--) {
        const r = this.rips[i];
        const withinX = this.player.x + 16 > r.x - r.width / 2 && this.player.x - 16 < r.x + r.width / 2;
        const withinY = this.player.y + 16 > r.y - r.height / 2 && this.player.y - 16 < r.y + r.height / 2;
        if (withinX && withinY) {
          if (this.hitHazard(r, this.rips, i, "Fell into a fabric rip")) return;
        }
      }
    }

    checkSpools() {
      for (let i = this.spoolItems.length - 1; i >= 0; i--) {
        const s = this.spoolItems[i];

        if (this.magnetTimer > 0) {
          const dMag = Phaser.Math.Distance.Between(this.player.x, this.player.y, s.x, s.y);
          if (dMag < 190) {
            s.x += (this.player.x - s.x) * 0.09;
            s.y += (this.player.y - s.y) * 0.09;
          }
        }

        const d = Phaser.Math.Distance.Between(this.player.x, this.player.y, s.x, s.y);
        if (d < 33) {
          this.destroyItem(s);
          this.spoolItems.splice(i, 1);
          this.spools++;
          this.score += 60 + Math.floor(this.combo * 2);
          this.combo = clamp(this.combo + 1, 0, 99);
          this.bestCombo = Math.max(this.bestCombo, Math.floor(this.combo));
          this.energy = Math.min(100, this.energy + 16);
          this.popText("+SPOOL", s.x, s.y - 24, "#7df9ff");
          this.audio.sfx("spool");

          if (this.combo > 0 && Math.floor(this.combo) % 10 === 0) {
            this.comboFlashTimer = 0.4;
            this.popText("COMBO x" + Math.floor(this.combo), this.player.x + 40, this.player.y - 46, "#ffb3d9");
          }
        }
      }
    }

    checkPowerups() {
      for (let i = this.powerups.length - 1; i >= 0; i--) {
        const p = this.powerups[i];

        const d = Phaser.Math.Distance.Between(this.player.x, this.player.y, p.x, p.y);
        if (d < 38) {
          this.activatePowerup(p.type);
          this.destroyItem(p);
          this.powerups.splice(i, 1);
        }
      }
    }

    activatePowerup(type) {
      if (type === "shield") {
        this.shield = true;
        this.popText("SHIELD", this.player.x + 40, this.player.y - 44, "#ffdf7d");
      }

      if (type === "magnet") {
        this.magnetTimer = 6;
        this.popText("MAGNET", this.player.x + 40, this.player.y - 44, "#ff7df2");
      }

      if (type === "slow") {
        this.slowTimer = 5;
        this.popText("TIME STITCH", this.player.x + 40, this.player.y - 44, "#7df9ff");
      }

      if (type === "ghost") {
        this.ghostTimer = 4;
        this.popText("GHOST THREAD", this.player.x + 40, this.player.y - 44, "#a98cff");
      }

      this.audio.sfx("power");
    }

    dash() {
      if (!this.started || this.dead || this.pausedGame) return;
      if (this.dashCooldown > 0 || this.energy < 24) return;

      this.energy -= 24;
      this.dashCooldown = 2.1;
      this.dashTimer = 0.26;
      this.ghostTimer = Math.max(this.ghostTimer, 0.26);
      this.combo = clamp(this.combo + 1, 0, 99);
      this.popText("DASH", this.player.x + 44, this.player.y - 38, "#ffdf7d");
      this.cameras.main.flash(80, 255, 223, 125, false);
      this.audio.sfx("dash");
    }

    updateEffects(dt) {
      this.magnetTimer = Math.max(0, this.magnetTimer - dt);
      this.slowTimer = Math.max(0, this.slowTimer - dt);
      this.ghostTimer = Math.max(0, this.ghostTimer - dt);
      this.dashCooldown = Math.max(0, this.dashCooldown - dt);
      this.dashTimer = Math.max(0, this.dashTimer - dt);
      this.comboFlashTimer = Math.max(0, this.comboFlashTimer - dt);

      if (this.combo > 0) this.combo = Math.max(0, this.combo - dt * 0.18);
    }

    unlockSkin(name) {
      if (!this.save.unlockedSkins.includes(name)) {
        this.save.unlockedSkins.push(name);
        this.audio.sfx("unlock");
      }
    }

    unlockTheme(name) {
      if (!this.save.unlockedThemes.includes(name)) {
        this.save.unlockedThemes.push(name);
        this.audio.sfx("unlock");
      }
    }

    updateMissions() {
      for (const m of this.missions) {
        if (m.id === "spools") m.value = this.spools;
        if (m.id === "combo") m.value = Math.max(m.value, Math.floor(this.bestCombo));
        if (m.id === "distance") m.value = Math.floor(this.distance);

        if (!m.done && m.value >= m.target) {
          m.done = true;
          this.score += 500;
          if (m.unlockSkin) this.unlockSkin(m.unlockSkin);
          if (m.unlockTheme) this.unlockTheme(m.unlockTheme);
          this.popText("UNLOCKED: " + m.reward, W / 2, 148, "#ffdf7d");
          this.spawnConfetti(W / 2, 160);
          this.cameras.main.flash(180, 255, 223, 125, false);
          this.writeSave();
        }
      }
    }

    updateHud() {
      this.scoreText.setText("Score " + Math.floor(this.score));
      this.spoolText.setText("Spools " + this.spools);
      this.comboText.setText("Combo x" + Math.floor(this.combo));
      this.energyBar.displayWidth = 210 * (this.energy / 100);

      if (this.comboFlashTimer > 0) {
        this.comboText.setScale(1.12);
        this.comboText.setColor("#ffffff");
      } else {
        this.comboText.setScale(1);
        this.comboText.setColor("#ffb3d9");
      }

      const zones = ["Cotton Run", "Denim District", "Silk Skyline", "Patchwork Rush", "Luxury Loom"];
      this.levelText.setText(zones[(this.level - 1) % zones.length] + " · Level " + this.level);

      const active = [];
      if (this.shield) active.push("Shield");
      if (this.magnetTimer > 0) active.push("Magnet " + Math.ceil(this.magnetTimer));
      if (this.slowTimer > 0) active.push("Slow " + Math.ceil(this.slowTimer));
      if (this.ghostTimer > 0) active.push("Ghost " + Math.ceil(this.ghostTimer));
      this.statusText.setText(active.join("  "));

      this.dashText.setText(this.dashCooldown > 0 ? "Dash ready in " + this.dashCooldown.toFixed(1) : "SHIFT = Dash");

      this.missionLines.forEach((line, idx) => {
        const m = this.missions[idx];
        const mark = m.done ? "✓" : "•";
        const value = Math.min(m.value, m.target);
        line.setText(`${mark} ${m.label} (${value}/${m.target})`);
        line.setColor(m.done ? "#ffdf7d" : "#f4efff");
      });
    }

    zoneColor() {
      if (this.save.theme === "Denim") {
        const colors = ["#102743", "#163457", "#1b406b", "#10233a", "#263d59"];
        return colors[(this.level - 1) % colors.length];
      }

      if (this.save.theme === "Luxury") {
        const colors = ["#3d2f16", "#5a4219", "#2c2110", "#4a3514", "#6a4f1f"];
        return colors[(this.level - 1) % colors.length];
      }

      const colors = ["#21172e", "#13243a", "#2d1b3f", "#352536", "#3d2f16"];
      return colors[(this.level - 1) % colors.length];
    }

    zoneFlash() {
      if (this.save.theme === "Denim") {
        const colors = [0x07111d, 0x081a2a, 0x0a2238, 0x0f2d45, 0x163a55];
        this.zoneGradient.fillColor = colors[(this.level - 1) % colors.length];
      } else if (this.save.theme === "Luxury") {
        const colors = [0x120c05, 0x211808, 0x2c200c, 0x3a2a0f, 0x1a1206];
        this.zoneGradient.fillColor = colors[(this.level - 1) % colors.length];
      } else {
        const colors = [0x090812, 0x071722, 0x130a20, 0x1c1016, 0x211808];
        this.zoneGradient.fillColor = colors[(this.level - 1) % colors.length];
      }

      this.cameras.main.flash(180, 125, 249, 255, false);
      if (this.started) this.popText("NEW ZONE", W / 2, 148, "#7df9ff");
    }

    popText(text, x, y, color) {
      const obj = this.add.text(x, y, text, {
        fontFamily: "Arial",
        fontSize: "16px",
        fontStyle: "bold",
        color
      }).setOrigin(0.5).setDepth(110);

      this.textPopups.push({ obj, life: 0.75 });
    }

    spawnConfetti(x, y) {
      const colors = [0x7df9ff, 0xffdf7d, 0xff7df2, 0xffffff, 0x7b3df2];
      for (let i = 0; i < 42; i++) {
        const rect = this.add.rectangle(x, y, Phaser.Math.Between(4, 9), Phaser.Math.Between(4, 9), Phaser.Utils.Array.GetRandom(colors)).setDepth(130);
        this.confetti.push({
          obj: rect,
          x,
          y,
          vx: Phaser.Math.Between(-220, 220),
          vy: Phaser.Math.Between(-260, -80),
          spin: Phaser.Math.FloatBetween(-8, 8),
          life: 1.2
        });
      }
    }

    press() {
      if (this.mode === "menu" || this.mode === "tutorial") {
        this.startGame();
        return;
      }

      if (this.dead) {
        this.scene.restart();
        return;
      }

      if (!this.started) {
        this.startGame();
        return;
      }

      if (this.pausedGame) return;

      if (this.isGrounded()) {
        this.vy = -555;
        this.addTrail(true);
        this.audio.sfx("jump");
        return;
      }

      if (this.vy > 115 && this.energy >= 35) {
        this.energy -= 35;
        this.addBridge(this.player.x + 55, this.player.y + 72);
      }
    }

    medalForScore() {
      if (this.score >= 4500) return "Platinum Stitch";
      if (this.score >= 3000) return "Gold Stitch";
      if (this.score >= 1800) return "Silver Stitch";
      return "Bronze Stitch";
    }

    saveRunResults() {
      if (this.runSaved) return;
      this.runSaved = true;

      const score = Math.floor(this.score);
      const distance = Math.floor(this.distance);
      this.save.bestScore = Math.max(this.save.bestScore, score);
      this.save.bestDistance = Math.max(this.save.bestDistance, distance);
      this.save.bestCombo = Math.max(this.save.bestCombo, this.bestCombo);
      this.save.totalSpools += this.spools;

      if (score >= 3000) this.unlockSkin("Ghost");
      if (score >= 4500) this.unlockTheme("Luxury");

      this.writeSave();
    }

    endGame(reason = "Run Unravelled") {
      if (this.dead) return;
      this.dead = true;
      this.started = false;
      this.mode = "gameover";

      this.audio.sfx("hit");
      this.cameras.main.shake(180, 0.006);
      this.saveRunResults();

      const panel = this.add.rectangle(W / 2, H / 2, 610, 330, 0x090812, 0.96)
        .setStrokeStyle(3, 0x7df9ff)
        .setDepth(150);

      this.add.text(
        W / 2,
        H / 2 - 104,
        "Run Unravelled",
        {
          fontFamily: "Arial",
          fontSize: "38px",
          fontStyle: "900",
          color: "#ffffff",
          stroke: "#7b3df2",
          strokeThickness: 5
        }
      ).setOrigin(0.5).setDepth(151);

      this.add.text(
        W / 2,
        H / 2 - 58,
        reason,
        {
          fontFamily: "Arial",
          fontSize: "17px",
          fontStyle: "bold",
          color: "#ffb3d9"
        }
      ).setOrigin(0.5).setDepth(151);

      const completed = this.missions.filter((m) => m.done).length;

      this.add.text(
        W / 2,
        H / 2 + 24,
        "Score: " + Math.floor(this.score) +
        "\nSpools: " + this.spools +
        "\nBest Combo: x" + this.bestCombo +
        "\nDistance: " + Math.floor(this.distance) + "m" +
        "\nChallenges Completed: " + completed + "/" + this.missions.length +
        "\nMedal: " + this.medalForScore() +
        "\nSaved Best: " + this.save.bestScore,
        {
          fontFamily: "Arial",
          fontSize: "19px",
          fontStyle: "bold",
          color: "#f4efff",
          align: "center",
          lineSpacing: 7
        }
      ).setOrigin(0.5).setDepth(151);

      this.add.text(W / 2, H / 2 + 146, "Press R or tap to restart", {
        fontFamily: "Arial",
        fontSize: "19px",
        color: "#7df9ff"
      }).setOrigin(0.5).setDepth(151);
    }
  }

  new Phaser.Game({
    type: Phaser.AUTO,
    parent: "game-container",
    width: W,
    height: H,
    backgroundColor: "#090812",
    scale: {
      mode: Phaser.Scale.FIT,
      autoCenter: Phaser.Scale.CENTER_BOTH
    },
    scene: ThreadRunner
  });
})();
