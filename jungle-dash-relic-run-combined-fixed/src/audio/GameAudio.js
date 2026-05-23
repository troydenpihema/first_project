class GameAudioEngine {
  constructor() {
    this.ctx = null;
    this.master = null;
    this.musicGain = null;
    this.sfxGain = null;
    this.musicTimer = null;
    this.currentTheme = null;
    this.enabled = true;
    this.step = 0;
    this.unlocked = false;
  }

  ensure() {
    if (!this.enabled) return null;

    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) {
      console.warn('Web Audio is not supported in this browser.');
      return null;
    }

    if (!this.ctx) {
      this.ctx = new AudioContextClass();

      this.master = this.ctx.createGain();
      this.master.gain.value = 0.95;
      this.master.connect(this.ctx.destination);

      this.musicGain = this.ctx.createGain();
      this.musicGain.gain.value = 0.28;
      this.musicGain.connect(this.master);

      this.sfxGain = this.ctx.createGain();
      this.sfxGain.gain.value = 0.75;
      this.sfxGain.connect(this.master);
    }

    if (this.ctx.state === 'suspended') {
      this.ctx.resume();
    }

    return this.ctx;
  }

  unlock() {
    const ctx = this.ensure();
    if (!ctx) return false;

    // A tiny silent blip helps unlock Web Audio on stricter browsers.
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    gain.gain.value = 0.0001;
    osc.connect(gain);
    gain.connect(this.master);
    osc.start();
    osc.stop(ctx.currentTime + 0.03);

    this.unlocked = true;
    return true;
  }

  startMusic(theme = 'jungle') {
    const ctx = this.ensure();
    if (!ctx) return;

    this.unlock();

    if (this.currentTheme === theme && this.musicTimer) return;

    this.stopMusic();
    this.currentTheme = theme;
    this.step = 0;

    const bpm = theme === 'volcano' ? 130 : theme === 'night' ? 106 : theme === 'temple' ? 114 : theme === 'menu' ? 100 : 122;
    const beatMs = 60000 / bpm;

    const scales = {
      coast: [261.63, 293.66, 329.63, 392.00, 440.00],
      jungle: [220.00, 261.63, 293.66, 329.63, 392.00],
      temple: [196.00, 220.00, 261.63, 293.66, 349.23],
      volcano: [164.81, 196.00, 220.00, 246.94, 293.66],
      night: [174.61, 220.00, 261.63, 329.63, 392.00],
      menu: [196.00, 246.94, 293.66, 329.63, 392.00]
    };

    const notes = scales[theme] || scales.jungle;

    // Play an immediate chord so you know audio is alive.
    this.tone(notes[0], 0.18, 'sine', this.musicGain, 0.12);
    this.tone(notes[2] || notes[0] * 1.25, 0.18, 'triangle', this.musicGain, 0.09);

    this.musicTimer = window.setInterval(() => {
      if (!this.ctx || this.ctx.state !== 'running') return;

      const root = notes[this.step % notes.length];
      const octave = this.step % 8 === 0 ? 0.5 : 1;

      this.tone(root * octave, 0.14, 'sine', this.musicGain, 0.11);
      if (this.step % 2 === 0) this.tone(root * 2, 0.09, 'triangle', this.musicGain, 0.08);
      if (this.step % 4 === 0) this.noise(0.045, this.musicGain, 0.055, 900);

      this.step++;
    }, beatMs / 2);
  }

  stopMusic() {
    if (this.musicTimer) {
      window.clearInterval(this.musicTimer);
      this.musicTimer = null;
    }
    this.currentTheme = null;
  }

  tone(freq, duration = 0.12, type = 'sine', output = this.sfxGain, volume = 0.25, slideTo = null) {
    const ctx = this.ensure();
    if (!ctx || !output) return;

    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.type = type;
    osc.frequency.setValueAtTime(Math.max(20, freq), ctx.currentTime);
    if (slideTo) {
      osc.frequency.exponentialRampToValueAtTime(Math.max(20, slideTo), ctx.currentTime + duration);
    }

    gain.gain.setValueAtTime(0.0001, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(volume, ctx.currentTime + 0.012);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);

    osc.connect(gain);
    gain.connect(output);

    osc.start();
    osc.stop(ctx.currentTime + duration + 0.04);
  }

  noise(duration = 0.12, output = this.sfxGain, volume = 0.25, filterFreq = 1200) {
    const ctx = this.ensure();
    if (!ctx || !output) return;

    const bufferSize = Math.max(1, Math.floor(ctx.sampleRate * duration));
    const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
    const data = buffer.getChannelData(0);

    for (let i = 0; i < bufferSize; i++) {
      data[i] = Math.random() * 2 - 1;
    }

    const source = ctx.createBufferSource();
    source.buffer = buffer;

    const filter = ctx.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.value = filterFreq;

    const gain = ctx.createGain();
    gain.gain.setValueAtTime(volume, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);

    source.connect(filter);
    filter.connect(gain);
    gain.connect(output);

    source.start();
    source.stop(ctx.currentTime + duration);
  }

  play(name) {
    const ctx = this.ensure();
    if (!ctx) return;

    switch (name) {
      case 'unlock':
        this.tone(660, 0.08, 'sine', this.sfxGain, 0.12);
        break;
      case 'start':
        this.tone(392, 0.10, 'triangle', this.sfxGain, 0.26);
        setTimeout(() => this.tone(523.25, 0.14, 'triangle', this.sfxGain, 0.25), 80);
        break;
      case 'jump':
        this.tone(260, 0.11, 'square', this.sfxGain, 0.20, 560);
        break;
      case 'dash':
        this.noise(0.10, this.sfxGain, 0.28, 2100);
        this.tone(180, 0.09, 'sawtooth', this.sfxGain, 0.16, 90);
        break;
      case 'spin':
        this.tone(420, 0.09, 'triangle', this.sfxGain, 0.20, 820);
        this.noise(0.06, this.sfxGain, 0.14, 2600);
        break;
      case 'fruit':
        this.tone(660, 0.06, 'sine', this.sfxGain, 0.18);
        setTimeout(() => this.tone(880, 0.06, 'sine', this.sfxGain, 0.15), 45);
        break;
      case 'crate':
        this.noise(0.15, this.sfxGain, 0.35, 1300);
        this.tone(120, 0.10, 'sawtooth', this.sfxGain, 0.18, 70);
        break;
      case 'enemy':
        this.tone(160, 0.10, 'square', this.sfxGain, 0.22, 270);
        this.noise(0.08, this.sfxGain, 0.18, 1700);
        break;
      case 'hurt':
        this.tone(300, 0.14, 'sawtooth', this.sfxGain, 0.28, 85);
        this.noise(0.17, this.sfxGain, 0.28, 950);
        break;
      case 'checkpoint':
        this.tone(523.25, 0.09, 'triangle', this.sfxGain, 0.22);
        setTimeout(() => this.tone(659.25, 0.09, 'triangle', this.sfxGain, 0.22), 85);
        setTimeout(() => this.tone(783.99, 0.14, 'triangle', this.sfxGain, 0.22), 170);
        break;
      case 'levelComplete':
        [523.25, 659.25, 783.99, 1046.5].forEach((f, i) => {
          setTimeout(() => this.tone(f, 0.15, 'triangle', this.sfxGain, 0.24), i * 105);
        });
        break;
      case 'gameOver':
        this.tone(220, 0.20, 'sawtooth', this.sfxGain, 0.24, 110);
        setTimeout(() => this.tone(164.81, 0.28, 'sawtooth', this.sfxGain, 0.22, 82), 180);
        break;
      default:
        this.tone(440, 0.08, 'sine', this.sfxGain, 0.12);
    }
  }
}

const GameAudio = new GameAudioEngine();
export default GameAudio;
