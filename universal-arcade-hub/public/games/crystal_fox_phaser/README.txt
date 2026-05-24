CRYSTAL FOX - Phaser Arcade Game

A polished browser platform adventure built for your arcade hub.

HOW TO RUN
1. Open the folder in VS Code.
2. Right click index.html and choose "Open with Live Server".
   OR upload the whole folder to Replit / Vite / your arcade hub server.

IMPORTANT
Do not double-click index.html directly if your browser blocks scripts or CDN loading.
Use Live Server, Replit, Vite, or your arcade hub.

ARCADE HUB SETUP
Put the full folder inside your games folder:

games/crystal-fox-phaser/index.html

Then link to it from your hub:
<a href="./games/crystal-fox-phaser/index.html">Play Crystal Fox</a>

Or iframe it:
<iframe src="./games/crystal-fox-phaser/index.html"></iframe>

CONTROLS
Keyboard:
- Move: A/D or Left/Right
- Jump: W, Up, or Space
- Dash: Shift
- Attack: J
- Pause: P

Mobile:
- Touch controls appear on screen automatically.

NOTES
This game uses Phaser from CDN:
https://cdn.jsdelivr.net/npm/phaser@3.90.0/dist/phaser.min.js

If you want this converted to a Vite build later, move game.js into src/main.js and install Phaser with npm.
