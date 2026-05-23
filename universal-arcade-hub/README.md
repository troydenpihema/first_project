# Universal Arcade Hub

This arcade is built for dropping different web-playable game types into one project.

## Run

```bash
npm install
npm run dev
```

## Where games go

Put game folders here:

```txt
public/games/
```

Each game folder should contain an `index.html` somewhere playable.

## Supported drop-in types

### 1. Plain browser games

```txt
public/games/my-game/index.html
public/games/my-game/game.js
public/games/my-game/style.css
```

These work immediately.

### 2. Built Vite games

```txt
public/games/my-vite-game/dist/index.html
```

These work immediately if already built.

### 3. Vite source projects

```txt
public/games/my-vite-game/package.json
public/games/my-vite-game/src/
public/games/my-vite-game/index.html
```

The arcade script will try to run `npm install` and `npm run build`, then play the `dist/index.html` file.

### 4. Unity WebGL games

Export your Unity game as WebGL, then drop the exported folder in:

```txt
public/games/my-unity-game/index.html
public/games/my-unity-game/Build/
public/games/my-unity-game/TemplateData/
```

### 5. Godot HTML5 games

Export your Godot game as HTML5/Web, then drop the exported folder in:

```txt
public/games/my-godot-game/index.html
public/games/my-godot-game/*.wasm
public/games/my-godot-game/*.pck
```

### 6. C# games

C# games cannot run in the browser directly unless they are exported/compiled to a browser format.

Best options:

- Unity WebGL export
- Godot C# web export, if your Godot/.NET setup supports your target
- Blazor WebAssembly for simple C# browser games

Once exported to browser files with `index.html`, drop the export folder into `public/games/`.

## Optional metadata

Add `arcade.json` inside a game folder:

```json
{
  "title": "Jungle Dash",
  "description": "A premium jungle platformer.",
  "type": "vite"
}
```

Add a thumbnail:

```txt
public/games/my-game/thumbnail.png
```

## Deploy to Vercel

```bash
npm run build
```

Then push to GitHub and import into Vercel.

Vercel settings:

```txt
Framework Preset: Vite
Build Command: npm run build
Output Directory: dist
Install Command: npm install
```
