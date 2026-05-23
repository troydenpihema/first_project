import Phaser from 'phaser';
import { LEVELS } from '../data/levels.js';
import GameAudio from '../audio/GameAudio.js';

export default class MenuScene extends Phaser.Scene {
  constructor() {
    super('MenuScene');
  }

  create() {
    this.createPremiumBackground();

    this.add.text(480, 98, 'JUNGLE DASH', {
      fontFamily: 'Arial',
      fontSize: '66px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#0b120d',
      strokeThickness: 9
    }).setOrigin(0.5);

    this.add.text(480, 158, 'RELIC RUN', {
      fontFamily: 'Arial',
      fontSize: '32px',
      fontStyle: '900',
      color: '#ffffff',
      stroke: '#000000',
      strokeThickness: 6,
      letterSpacing: 4
    }).setOrigin(0.5);

    this.add.text(480, 235, '10-Level Expanded Phaser Prototype', {
      fontFamily: 'Arial',
      fontSize: '22px',
      fontStyle: 'bold',
      color: '#dfffee'
    }).setOrigin(0.5);

    this.add.text(480, 312, 'Press ENTER to Start', {
      fontFamily: 'Arial',
      fontSize: '28px',
      fontStyle: '900',
      color: '#ffffff',
      stroke: '#000000',
      strokeThickness: 4
    }).setOrigin(0.5);

    this.add.text(480, 370, 'Move A/D · Jump Space · Spin J/K · Dash Shift', {
      fontFamily: 'Arial',
      fontSize: '18px',
      color: '#e9fff6'
    }).setOrigin(0.5);

    this.soundPrompt = this.add.text(480, 462, 'Click or press ENTER to unlock sound', {
      fontFamily: 'Arial',
      fontSize: '16px',
      fontStyle: 'bold',
      color: '#ffe66d'
    }).setOrigin(0.5);

    this.add.text(480, 420, LEVELS.map((l, i) => `${i + 1}. ${l.name}`).join('   '), {
      fontFamily: 'Arial',
      fontSize: '16px',
      color: '#bdfbdd'
    }).setOrigin(0.5);

    this.input.once('pointerdown', () => {
      this.unlockSoundOnly();
    });

    this.input.keyboard.once('keydown-ENTER', () => {
      this.startGame();
    });
  }

  unlockSoundOnly() {
    GameAudio.unlock();
    GameAudio.play('unlock');
    GameAudio.startMusic('menu');

    if (this.soundPrompt) {
      this.soundPrompt.setText('Sound on ✓  Press ENTER to Start');
      this.soundPrompt.setColor('#dfffee');
    }
  }

  startGame() {
    GameAudio.unlock();
    GameAudio.play('start');
    GameAudio.startMusic('menu');
    this.scene.start('LevelIntroScene', { levelIndex: 0, campaign: { fruit: 0, lives: 3, totalCrates: 0, brokenCrates: 0 } });
  }

  createPremiumBackground() {
    this.cameras.main.setBackgroundColor('#73ddff');

    for (let i = 0; i < 55; i++) {
      const x = i * 82 - 40;
      const y = 420 + Math.sin(i * 0.7) * 22;
      const tri = this.add.triangle(x, y, 0, 120, 70, 0, 140, 120, 0x17572e, 0.55);
      tri.setScrollFactor(0);
    }

    for (let i = 0; i < 12; i++) {
      const x = i * 100 + 15;
      this.add.rectangle(x, 475, 18, 130, 0x6a3919);
      this.add.ellipse(x, 372 + Math.sin(i) * 12, 118, 75, i % 2 ? 0x1d7a3a : 0x269b4c);
    }

    const glow = this.add.circle(480, 165, 240, 0xffffff, 0.08);
    glow.setBlendMode(Phaser.BlendModes.ADD);
  }
}
