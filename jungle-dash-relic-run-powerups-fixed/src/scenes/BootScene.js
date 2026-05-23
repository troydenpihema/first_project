import Phaser from 'phaser';

export default class BootScene extends Phaser.Scene {
  constructor() {
    super('BootScene');
  }

  preload() {
    this.createGeneratedTextures();
  }

  create() {
    this.scene.start('MenuScene');
  }

  createGeneratedTextures() {
    const g = this.add.graphics();

    g.clear();
    g.fillStyle(0xf26a21, 1);
    g.fillRoundedRect(6, 10, 42, 48, 15);
    g.fillStyle(0xffb347, 1);
    g.fillRoundedRect(12, 20, 29, 23, 10);
    g.fillStyle(0xffffff, 1);
    g.fillCircle(34, 25, 5);
    g.fillStyle(0x111111, 1);
    g.fillCircle(36, 25, 2);
    g.fillStyle(0x3b1d0d, 1);
    g.fillTriangle(10, 12, 20, 0, 24, 14);
    g.fillTriangle(27, 12, 38, 0, 39, 15);
    g.fillStyle(0x17262d, 1);
    g.fillRoundedRect(8, 54, 16, 9, 5);
    g.fillRoundedRect(31, 54, 16, 9, 5);
    g.generateTexture('player', 58, 66);

    g.clear();
    g.fillStyle(0xffe1b2, 1);
    g.fillRoundedRect(0, 0, 74, 18, 8);
    g.fillStyle(0x0c1b18, 0.22);
    g.fillRoundedRect(5, 5, 64, 8, 5);
    g.generateTexture('dash-trail', 74, 18);

    g.clear();
    g.fillStyle(0x8d4a20, 1);
    g.fillRect(4, 5, 44, 44);
    g.fillStyle(0xc17931, 1);
    g.fillRect(0, 0, 44, 44);
    g.lineStyle(4, 0x5a2b12, 1);
    g.strokeRect(3, 3, 38, 38);
    g.beginPath();
    g.moveTo(6, 6);
    g.lineTo(38, 38);
    g.moveTo(38, 6);
    g.lineTo(6, 38);
    g.strokePath();
    g.generateTexture('crate', 52, 52);

    g.clear();
    g.fillStyle(0xff9717, 1);
    g.fillEllipse(16, 18, 22, 26);
    g.fillStyle(0xffd07a, 1);
    g.fillEllipse(12, 12, 6, 10);
    g.fillStyle(0x23aa4a, 1);
    g.fillEllipse(23, 5, 15, 6);
    g.generateTexture('fruit', 34, 34);

    g.clear();
    g.fillStyle(0x3e2074, 1);
    g.fillRoundedRect(4, 9, 48, 33, 16);
    g.fillStyle(0x7241c9, 1);
    g.fillRoundedRect(0, 4, 48, 33, 16);
    g.fillStyle(0xffffff, 1);
    g.fillCircle(16, 18, 5);
    g.fillCircle(32, 18, 5);
    g.fillStyle(0x111111, 1);
    g.fillCircle(17, 18, 2);
    g.fillCircle(33, 18, 2);
    g.fillStyle(0x1b0d33, 1);
    g.fillRoundedRect(13, 29, 22, 4, 2);
    g.generateTexture('enemy-crawler', 58, 50);

    g.clear();
    g.fillStyle(0x0ff0ff, 0.85);
    g.fillRoundedRect(10, 0, 48, 110, 24);
    g.lineStyle(6, 0xffffff, 0.85);
    g.strokeRoundedRect(10, 0, 48, 110, 24);
    g.fillStyle(0xffffff, 0.32);
    g.fillEllipse(34, 55, 20, 76);
    g.generateTexture('portal', 70, 120);

    g.clear();
    g.fillStyle(0xffffff, 1);
    g.fillCircle(4, 4, 4);
    g.generateTexture('spark', 8, 8);

    g.clear();
    g.fillStyle(0x111111, 1);
    g.fillTriangle(0, 26, 14, 0, 28, 26);
    g.lineStyle(2, 0xfff2cb, 0.4);
    g.strokeTriangle(0, 26, 14, 0, 28, 26);
    g.generateTexture('spike', 28, 28);

    g.clear();
    g.fillStyle(0x2f9448, 1);
    g.fillRoundedRect(0, 0, 128, 25, 8);
    g.fillStyle(0x6bd160, 1);
    for (let x = 8; x < 128; x += 20) {
      g.fillTriangle(x, 2, x + 8, -10, x + 16, 2);
    }
    g.generateTexture('grass-top', 128, 30);

    g.clear();
    g.fillStyle(0x96704a, 1);
    g.fillRoundedRect(0, 0, 128, 24, 8);
    g.lineStyle(3, 0x5f4126, 1);
    for (let x = 0; x < 128; x += 28) {
      g.lineBetween(x, 2, x + 18, 22);
    }
    g.generateTexture('stone-top', 128, 30);

    g.clear();
    g.fillStyle(0x5e2622, 1);
    g.fillRoundedRect(0, 0, 128, 24, 8);
    g.fillStyle(0xff6b2d, 0.55);
    for (let x = 10; x < 128; x += 28) {
      g.fillCircle(x, 11, 4);
    }
    g.generateTexture('volcano-top', 128, 30);

    g.clear();
    g.fillStyle(0x2e315f, 1);
    g.fillRoundedRect(0, 0, 128, 24, 8);
    g.fillStyle(0x8bd3ff, 0.55);
    for (let x = 8; x < 128; x += 24) {
      g.fillCircle(x, 11, 3);
    }
    g.generateTexture('night-top', 128, 30);

    g.clear();
    g.fillStyle(0x27d7ff, 0.8);
    g.fillCircle(10, 10, 10);
    g.lineStyle(3, 0xffffff, 0.9);
    g.strokeCircle(10, 10, 9);
    g.generateTexture('checkpoint', 22, 22);


    // Moving platform texture
    g.clear();
    g.fillStyle(0x20c8ff, 1);
    g.fillRoundedRect(0, 0, 150, 24, 10);
    g.fillStyle(0xffffff, 0.45);
    g.fillRoundedRect(10, 5, 130, 6, 5);
    g.lineStyle(3, 0x0b5870, 1);
    g.strokeRoundedRect(2, 2, 146, 20, 9);
    g.generateTexture('moving-platform', 150, 24);

    // Power-up textures
    g.clear();
    g.fillStyle(0x39ff88, 0.95);
    g.fillCircle(20, 20, 18);
    g.lineStyle(4, 0xffffff, 0.9);
    g.strokeCircle(20, 20, 15);
    g.fillStyle(0xffffff, 1);
    g.fillTriangle(20, 8, 30, 20, 20, 32);
    g.generateTexture('power-shield', 40, 40);

    g.clear();
    g.fillStyle(0xffe34d, 0.95);
    g.fillCircle(20, 20, 18);
    g.lineStyle(4, 0xffffff, 0.9);
    g.strokeCircle(20, 20, 15);
    g.fillStyle(0xffffff, 1);
    g.fillRect(17, 7, 7, 26);
    g.fillRect(9, 17, 22, 7);
    g.generateTexture('power-magnet', 40, 40);

    g.clear();
    g.fillStyle(0xff4df0, 0.95);
    g.fillCircle(20, 20, 18);
    g.lineStyle(4, 0xffffff, 0.9);
    g.strokeCircle(20, 20, 15);
    g.fillStyle(0xffffff, 1);
    g.fillTriangle(14, 8, 30, 20, 14, 32);
    g.generateTexture('power-speed', 40, 40);

    g.clear();
    g.fillStyle(0x5dd7ff, 0.95);
    g.fillCircle(20, 20, 18);
    g.lineStyle(4, 0xffffff, 0.9);
    g.strokeCircle(20, 20, 15);
    g.fillStyle(0xffffff, 1);
    g.fillTriangle(20, 7, 31, 23, 24, 23);
    g.fillTriangle(20, 33, 9, 17, 16, 17);
    g.generateTexture('power-double', 40, 40);

    g.destroy();
  }
}
