import Phaser from 'phaser';
import GameAudio from '../audio/GameAudio.js';

export default class GameOverScene extends Phaser.Scene {
  constructor() {
    super('GameOverScene');
  }

  init(data) {
    this.stats = data;
  }

  create() {
    this.cameras.main.setBackgroundColor('#180b12');
    GameAudio.play('gameOver');

    this.add.text(480, 150, 'GAME OVER', {
      fontFamily: 'Arial',
      fontSize: '70px',
      fontStyle: '900',
      color: '#ff5d5d',
      stroke: '#000000',
      strokeThickness: 8
    }).setOrigin(0.5);

    this.add.text(480, 245, `Fruit: ${this.stats.fruit ?? 0}`, {
      fontFamily: 'Arial',
      fontSize: '28px',
      color: '#ffffff'
    }).setOrigin(0.5);

    this.add.text(480, 290, `Level crates: ${this.stats.crates ?? 0}/${this.stats.totalCrates ?? 0}`, {
      fontFamily: 'Arial',
      fontSize: '28px',
      color: '#ffffff'
    }).setOrigin(0.5);

    this.add.text(480, 375, 'Press ENTER to Restart This Level', {
      fontFamily: 'Arial',
      fontSize: '24px',
      fontStyle: 'bold',
      color: '#ffe66d'
    }).setOrigin(0.5);

    this.add.text(480, 420, 'Press M for Main Menu', {
      fontFamily: 'Arial',
      fontSize: '19px',
      color: '#dfffee'
    }).setOrigin(0.5);

    this.input.keyboard.once('keydown-ENTER', () => {
      GameAudio.play('start');
      this.scene.start('GameScene', {
        levelIndex: this.stats.levelIndex ?? 0,
        campaign: {
          ...(this.stats.campaign ?? {}),
          lives: 3
        }
      });
    });

    this.input.keyboard.once('keydown-M', () => {
      this.scene.start('MenuScene');
    });
  }
}
