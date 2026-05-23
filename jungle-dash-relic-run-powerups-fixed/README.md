# Jungle Dash: Relic Run — Powerups + Moving Platform Fix

This build keeps the same Phaser + Vite structure and adds gameplay improvements on top.

## New in this version

- Moving platforms are now clearly visible and actually move
- Moving platforms are placed more fairly in the jump path
- Raised/static platforms have better clearance so you can walk under them
- Added power-ups:
  - Shield: blocks one hit
  - Magnet: pulls nearby fruit toward you
  - Speed: faster movement for a short time
  - Double Jump: gives an extra jump for a short time
- Power-up UI display
- Power-up sound effect
- Keeps the playable spacing fix
- Keeps the audio unlock fix
- Keeps spikes on platforms

## Run locally

```bash
npm install
npm run dev
```

## Audio note

Browser audio only starts after a user action.

On the menu:

```text
Click once on the game screen
or press Enter
```

Also make sure the browser tab is not muted.

## Controls

```text
A / D or Arrow Keys = Move
Space / W / Up Arrow = Jump
J / K = Spin attack
Left Shift = Dash
P = Pause
R = Restart level
Enter = Start / Continue
M = Main Menu on game over
```

## Deploy to Vercel

Push this folder to GitHub, then import it into Vercel.

```text
Framework Preset: Vite
Build Command: npm run build
Output Directory: dist
Install Command: npm install
```
