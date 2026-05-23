const LEVEL_NAMES = [
  ['Emerald Beach', 'coast'],
  ['Canopy Climb', 'jungle'],
  ['Temple Rush', 'temple'],
  ['Volcano Vault', 'volcano'],
  ['Moonlit Mangroves', 'night'],
  ['Tiki Falls', 'jungle'],
  ['Lava Switchbacks', 'volcano'],
  ['Ancient Causeway', 'temple'],
  ['Storm Canopy', 'night'],
  ['Relic Run Finale', 'volcano']
];

function makeLevel(id, name, theme, difficulty) {
  const width = 3300 + difficulty * 300;
  const baseY = 480;

  const platforms = [];
  const crates = [];
  const fruits = [];
  const enemies = [];
  const spikes = [];
  const moving = [];
  const checkpoints = [];

  // Tuning:
  // Level 1 is intentionally forgiving.
  // Later levels get wider gaps and more hazards, but the gaps stay within the jump+dash capability.
  const minGap = 70 + difficulty * 8;
  const maxGap = 115 + difficulty * 9;
  const minWidth = Math.max(230, 390 - difficulty * 10);
  const maxWidth = Math.max(300, 470 - difficulty * 8);
  const segmentCount = 8 + difficulty;

  let currentEnd = 0;

  platforms.push([0, baseY, 560, 110]);
  fruits.push([95, baseY - 85, 6, 42]);
  crates.push([330, baseY - 42]);

  currentEnd = 560;

  for (let i = 1; i <= segmentCount; i++) {
    const gap = minGap + ((i * 31 + difficulty * 17) % Math.max(1, maxGap - minGap));
    const w = minWidth + ((i * 53 + difficulty * 11) % Math.max(1, maxWidth - minWidth));

    // First level has gentle height changes. Later levels climb/drop more.
    const heightStep = difficulty <= 2 ? 24 : 34;
    const y = baseY - ((i % 4) * heightStep) - (difficulty >= 6 && i % 5 === 0 ? 36 : 0);

    const x = currentEnd + gap;
    platforms.push([x, y, w, 110 + (i % 3) * 8]);

    // Fruit line above each platform.
    const fruitCount = Math.min(8, 4 + Math.floor(difficulty / 2) + (i % 2));
    fruits.push([x + 32, y - 72, fruitCount, 40]);

    // Crates on the platform.
    if (i % 2 === 0 || difficulty > 3) {
      crates.push([x + Math.min(w - 70, 82), y - 42]);
    }

    if (i % 3 === 0) {
      crates.push([x + Math.min(w - 82, 155), y - 42, 'bonus']);
    }

    // Enemies patrol on safe-width platforms.
    if (w > 270 && i % 2 === 1) {
      enemies.push([x + 76, y - 48, x + 42, x + w - 45, 'crawler']);
    }

    // Spikes are ON the platforms, not at the bottom of pits.
    // Keep Level 1 light.
    if (difficulty > 1 && i > 1 && (i + difficulty) % 3 === 0) {
      const spikeCount = Math.min(5, 1 + Math.floor(difficulty / 3));
      const spikeX = x + Math.min(w - 120, 130);
      spikes.push([spikeX, y - 18, spikeCount]);
    }

    // Raised optional mini-platforms.
    if (i % 4 === 1) {
      const miniX = x + Math.min(w - 170, 115);
      const miniY = y - (difficulty <= 2 ? 100 : 112);
      platforms.push([miniX, miniY, 155, 30]);
      fruits.push([miniX + 15, miniY - 55, 3 + Math.floor(difficulty / 4), 38]);
      if (difficulty > 3) crates.push([miniX + 62, miniY - 42, 'bonus']);
    }

    // Moving platforms appear more as difficulty rises.
    if (difficulty >= 2 && i > 1 && i % 3 === 2) {
      const mx = x - Math.floor(gap * 0.55);
      const my = y - 58;
      const travel = 120 + difficulty * 10;
      moving.push([mx, my, 145, 25, mx - 40, mx + travel, 75 + difficulty * 7]);
    }

    // Checkpoints.
    if (i === Math.floor(segmentCount / 3) || i === Math.floor(segmentCount * 2 / 3)) {
      checkpoints.push([x + 35, y - 56]);
    }

    currentEnd = x + w;
  }

  // Final platform is close enough to the previous platform.
  const finalGap = Math.min(150 + difficulty * 8, 220);
  const finalX = currentEnd + finalGap;
  const finalY = baseY - 24;
  const finalW = 560;

  platforms.push([finalX, finalY, finalW, 110]);
  fruits.push([finalX + 45, finalY - 84, 8, 40]);
  crates.push([finalX + 185, finalY - 42]);
  crates.push([finalX + 245, finalY - 42, 'bonus']);

  if (difficulty >= 5) {
    spikes.push([finalX + 350, finalY - 18, 3]);
  }

  const levelWidth = finalX + finalW + 260;

  return {
    id,
    name,
    theme,
    width: levelWidth,
    spawn: { x: 105, y: 360 },
    portal: { x: finalX + finalW - 105, y: finalY - 120 },
    platforms,
    crates,
    fruits,
    enemies,
    spikes,
    moving,
    checkpoints
  };
}

export const LEVELS = LEVEL_NAMES.map(([name, theme], index) => {
  return makeLevel(index, name, theme, index + 1);
});

export function getLevel(index) {
  return LEVELS[index] ?? LEVELS[0];
}
