import Phaser from 'phaser';
import { LEVELS, getLevel } from '../data/levels.js';
import GameAudio from '../audio/GameAudio.js';

export default class LevelCompleteScene extends Phaser.Scene {
  constructor() {
    super('LevelCompleteScene');
  }

  init(data) {
    this.levelIndex = data.levelIndex ?? 0;
    this.campaign = data.campaign;
    this.levelStats = data.levelStats;
    this.level = getLevel(this.levelIndex);
  }

  create() {
    this.cameras.main.setBackgroundColor('#0f3b2a');

    this.add.text(480, 105, 'LEVEL COMPLETE!', {
      fontFamily: 'Arial',
      fontSize: '58px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000000',
      strokeThickness: 8
    }).setOrigin(0.5);

    this.add.text(480, 172, this.level.name, {
      fontFamily: 'Arial',
      fontSize: '30px',
      fontStyle: '900',
      color: '#ffffff',
      stroke: '#000000',
      strokeThickness: 5
    }).setOrigin(0.5);

    const crates = `${this.levelStats.crates}/${this.levelStats.totalCrates}`;
    const grade = this.getGrade();

    const lines = [
      `Fruit this level: ${this.levelStats.fruit}`,
      `Crates smashed: ${crates}`,
      `Lives left: ${this.levelStats.lives}`,
      `Run grade: ${grade}`
    ];

    lines.forEach((line, i) => {
      this.add.text(480, 245 + i * 42, line, {
        fontFamily: 'Arial',
        fontSize: '26px',
        fontStyle: 'bold',
        color: '#ffffff'
      }).setOrigin(0.5);
    });

    const finalLevel = this.levelIndex + 1 >= LEVELS.length;
    this.add.text(480, 445, finalLevel ? 'Press ENTER to Finish' : 'Press ENTER for Next Level', {
      fontFamily: 'Arial',
      fontSize: '24px',
      fontStyle: '900',
      color: '#dfffee'
    }).setOrigin(0.5);

    this.input.keyboard.once('keydown-ENTER', () => {
      if (finalLevel) {
        GameAudio.play('levelComplete');
        this.scene.start('WinScene', { campaign: this.campaign });
      } else {
        GameAudio.play('start');
        this.scene.start('LevelIntroScene', {
          levelIndex: this.levelIndex + 1,
          campaign: this.campaign
        });
      }
    });
  }

  getGrade() {
    const crateRatio = this.levelStats.totalCrates === 0 ? 1 : this.levelStats.crates / this.levelStats.totalCrates;

    if (crateRatio >= 1 && this.levelStats.lives >= 3) return 'S';
    if (crateRatio >= 0.85) return 'A';
    if (crateRatio >= 0.65) return 'B';
    return 'C';
  }
}
