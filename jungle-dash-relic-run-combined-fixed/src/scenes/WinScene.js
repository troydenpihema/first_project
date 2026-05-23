import Phaser from 'phaser';
import GameAudio from '../audio/GameAudio.js';

export default class WinScene extends Phaser.Scene {
  constructor() {
    super('WinScene');
  }

  init(data) {
    this.campaign = data.campaign ?? {};
  }

  create() {
    this.cameras.main.setBackgroundColor('#081b14');
    GameAudio.play('levelComplete');

    this.add.text(480, 100, 'RELIC CLAIMED!', {
      fontFamily: 'Arial',
      fontSize: '62px',
      fontStyle: '900',
      color: '#ffe66d',
      stroke: '#000000',
      strokeThickness: 8
    }).setOrigin(0.5);

    this.add.text(480, 172, 'You cleared all 10 levels.', {
      fontFamily: 'Arial',
      fontSize: '28px',
      fontStyle: 'bold',
      color: '#ffffff'
    }).setOrigin(0.5);

    const fruit = this.campaign.fruit ?? 0;
    const crates = this.campaign.brokenCrates ?? 0;
    const totalCrates = this.campaign.totalCrates ?? 0;
    const lives = this.campaign.lives ?? 0;
    const completion = totalCrates ? Math.round((crates / totalCrates) * 100) : 0;

    const lines = [
      `Total Fruit: ${fruit}`,
      `Total Crates: ${crates}/${totalCrates}`,
      `Crate Completion: ${completion}%`,
      `Lives Remaining: ${lives}`
    ];

    lines.forEach((line, i) => {
      this.add.text(480, 250 + i * 43, line, {
        fontFamily: 'Arial',
        fontSize: '27px',
        color: '#ffffff'
      }).setOrigin(0.5);
    });

    this.add.text(480, 455, 'Press ENTER to Play Again', {
      fontFamily: 'Arial',
      fontSize: '24px',
      fontStyle: '900',
      color: '#dfffee'
    }).setOrigin(0.5);

    this.input.keyboard.once('keydown-ENTER', () => {
      this.scene.start('MenuScene');
    });
  }
}
