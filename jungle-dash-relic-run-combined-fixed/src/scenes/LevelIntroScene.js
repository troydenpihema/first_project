import Phaser from 'phaser';
import { getLevel } from '../data/levels.js';
import GameAudio from '../audio/GameAudio.js';

export default class LevelIntroScene extends Phaser.Scene {
  constructor() {
    super('LevelIntroScene');
  }

  init(data) {
    this.levelIndex = data.levelIndex ?? 0;
    this.campaign = data.campaign ?? { fruit: 0, lives: 3, totalCrates: 0, brokenCrates: 0 };
    this.level = getLevel(this.levelIndex);
  }

  create() {
    const colors = {
      coast: '#0d8bbb',
      jungle: '#0d7a3c',
      temple: '#8c693f',
      volcano: '#79271d',
      night: '#171b4f'
    };

    this.cameras.main.setBackgroundColor(colors[this.level.theme] ?? '#0d7a3c');

    this.add.text(480, 145, `LEVEL ${this.levelIndex + 1}`, {
      fontFamily: 'Arial',
      fontSize: '54px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000',
      strokeThickness: 8
    }).setOrigin(0.5);

    this.add.text(480, 220, this.level.name, {
      fontFamily: 'Arial',
      fontSize: '38px',
      fontStyle: '900',
      color: '#ffffff',
      stroke: '#000',
      strokeThickness: 5
    }).setOrigin(0.5);

    this.add.text(480, 305, 'Press ENTER', {
      fontFamily: 'Arial',
      fontSize: '26px',
      fontStyle: 'bold',
      color: '#dfffee'
    }).setOrigin(0.5);

    this.input.keyboard.once('keydown-ENTER', () => {
      GameAudio.unlock();
      GameAudio.play('start');
      GameAudio.startMusic(this.level.theme);
      this.scene.start('GameScene', {
        levelIndex: this.levelIndex,
        campaign: this.campaign
      });
    });
  }
}
