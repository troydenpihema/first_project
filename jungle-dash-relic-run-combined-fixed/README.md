# Jungle Dash: Relic Run — Combined Fixed Build

This is the combined fixed version.

It includes:

- The playable spacing fix
- Fairer Level 1 jumps
- 10 progressively harder levels
- Spikes placed on platforms instead of fall zones
- Music and sound effect fix
- Browser audio unlock
- Louder generated music and SFX
- Vercel-ready Phaser + Vite setup

## Run locally

Open this folder in VS Code, then run:

```bash
npm install
npm run dev
```

Open the localhost URL Vite gives you.

## Important audio note

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

Vercel settings:

```text
Framework Preset: Vite
Build Command: npm run build
Output Directory: dist
Install Command: npm install
```

## Notes

This is still a prototype, but it is now the cleanest combined version so far.
For a sellable game, the next steps are proper character art, animations, real music, sound design, controller support, save/progression, and polished level design.
