using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
static void UpdateMinigameScreen(float dt)
{
    if (activeMinigameType == MinigameType.Pokie)
    {
        activePokieMachine.Update(dt, player);
        activePokieMachine.BetOptions = activePokieMachine.GetAvailableBets(player);
        // clamp current bet to highest unlocked option
        if (activePokieMachine.BetAmount > activePokieMachine.BetOptions[^1])
            activePokieMachine.BetAmount = activePokieMachine.BetOptions[^1];

        if (!activePokieMachine.IsSpinning)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                activePokieMachine.StartSpin(player);

            if (Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                int idx = Array.IndexOf(activePokieMachine.BetOptions, activePokieMachine.BetAmount);
                idx = Math.Min(idx + 1, activePokieMachine.BetOptions.Length - 1);
                activePokieMachine.BetAmount = activePokieMachine.BetOptions[idx];
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Down))
            {
                int idx = Array.IndexOf(activePokieMachine.BetOptions, activePokieMachine.BetAmount);
                idx = Math.Max(idx - 1, 0);
                activePokieMachine.BetAmount = activePokieMachine.BetOptions[idx];
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            activePokieMachine.Close();
            activeMinigameType = MinigameType.None;
            ChangeScene(SceneState.Building);
        }
    }
    else if (activeMinigameType == MinigameType.Darts)
{
    activeDartsGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activeDartsGame.Close();
        ChangeScene(SceneState.Building); // or whatever you use to return
    }
}
    else if (activeMinigameType == MinigameType.Pool)
    {
        activePoolGame.Update(dt, player);

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            activePoolGame.Close();
            activeMinigameType = MinigameType.None;
            ChangeScene(SceneState.Building);
        }
    }
    
    else if (activeMinigameType == MinigameType.Bowling)
    {
        activeBowlingGame.Update(dt, player);
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            activeBowlingGame.Close();
            activeMinigameType = MinigameType.None;
            ChangeScene(SceneState.Building);
        }
    }
    else if (activeMinigameType == MinigameType.Claw)
    {
        activeClawMachine.Update(dt, player);
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            activeClawMachine.Close();
            activeMinigameType = MinigameType.None;
            ChangeScene(SceneState.Building);
        }
    }
    else if (activeMinigameType == MinigameType.Pinball)
{
    activePinballGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activePinballGame.Close();
        activeMinigameType = MinigameType.None;
        ChangeScene(SceneState.Building);
    }
}
else if (activeMinigameType == MinigameType.AirHockey)
{
    activeAirHockeyGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activeAirHockeyGame.Close();
        activeMinigameType = MinigameType.None;
        ChangeScene(SceneState.Building);
    }
}
else if (activeMinigameType == MinigameType.PianoTiles)
{
    activePianoTilesGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activePianoTilesGame.Close();
        activeMinigameType = MinigameType.None;
        ChangeScene(SceneState.Building);
    }
}
else if (activeMinigameType == MinigameType.Flappy)
{
    activeFlappyGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activeFlappyGame.Close();
        activeMinigameType = MinigameType.None;
        ChangeScene(SceneState.Building);
    }
}
else if (activeMinigameType == MinigameType.MiniGolf)
{
    activeMiniGolfGame.Update(dt, player);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        activeMiniGolfGame.Close();
        activeMinigameType = MinigameType.None;
        ChangeScene(SceneState.Building);
    }
}
}

static void DrawMinigameScreen()
{
    if (activeMinigameType == MinigameType.Pokie)
    {
        Raylib.ClearBackground(Color.White);

        Raylib.DrawRectangle(0, 0, ScreenWidth, 110, new Color((byte)230, (byte)40, (byte)40, (byte)255));
        int titleWidth = Program.MeasureTextUI("POKIE MACHINE", 50);
        Program.DrawTextUI("POKIE MACHINE", ScreenWidth / 2 - titleWidth / 2, 30, 50, Color.White);

        int reelWidth = 150;
        int reelHeight = 150;
        int startX = ScreenWidth / 2 - (reelWidth * 3 + 40) / 2;
        int y = 220;

        Raylib.DrawRectangle(startX - 30, y - 30, reelWidth * 3 + 40 + 60, reelHeight + 60,
            new Color((byte)40, (byte)40, (byte)45, (byte)255));
        Raylib.DrawRectangleLines(startX - 30, y - 30, reelWidth * 3 + 40 + 60, reelHeight + 60, Color.Gold);

        for (int i = 0; i < activePokieMachine.Reels.Length; i++)
        {
            int x = startX + i * (reelWidth + 20);

            Raylib.DrawRectangle(x, y, reelWidth, reelHeight, Color.White);
            Raylib.DrawRectangleLines(x, y, reelWidth, reelHeight, new Color((byte)40, (byte)40, (byte)45, (byte)255));

            DrawSlotSymbolIcon(activePokieMachine.Reels[i], x, y, reelWidth);
        }

        Program.DrawTextUI($"Bet: ${activePokieMachine.BetAmount}  (Up/Down to change)", startX, 420, 24,
            new Color((byte)30, (byte)30, (byte)35, (byte)255));
        Program.DrawTextUI($"Wallet: ${player.Money}", startX, 460, 24,
            new Color((byte)30, (byte)30, (byte)35, (byte)255));

        if (activePokieMachine.ResultTimer > 0f)
        {
            int msgWidth = Program.MeasureTextUI(activePokieMachine.ResultMessage, 26);
            Program.DrawTextUI(activePokieMachine.ResultMessage, ScreenWidth / 2 - msgWidth / 2, 500, 26,
                new Color((byte)0, (byte)140, (byte)0, (byte)255));
        }

        string prompt = activePokieMachine.IsSpinning ? "Spinning..."
            : activePokieMachine.HasSpunOnce ? "SPACE = Spin Again | ESC = Leave"
            : "SPACE = Spin | ESC = Leave";
        int promptWidth = Program.MeasureTextUI(prompt, 22);
        Program.DrawTextUI(prompt, ScreenWidth / 2 - promptWidth / 2, 560, 22,
            new Color((byte)90, (byte)90, (byte)95, (byte)255));
    }
    else if (activeMinigameType == MinigameType.Pool)
    {
        activePoolGame.Draw(player);
    }
    else if (activeMinigameType == MinigameType.Darts)
    activeDartsGame.Draw(player);
    else if (activeMinigameType == MinigameType.Bowling)
        activeBowlingGame.Draw(player);
    else if (activeMinigameType == MinigameType.Claw)
        activeClawMachine.Draw(player);
    else if (activeMinigameType == MinigameType.Pinball)
    activePinballGame.Draw(player);
    else if (activeMinigameType == MinigameType.AirHockey)
        activeAirHockeyGame.Draw(player);
    else if (activeMinigameType == MinigameType.PianoTiles)
        activePianoTilesGame.Draw(player);
    else if (activeMinigameType == MinigameType.Flappy)
        activeFlappyGame.Draw(player);
    else if (activeMinigameType == MinigameType.MiniGolf)
    activeMiniGolfGame.Draw(player);
}

static void DrawSlotSymbolIcon(SlotSymbol symbol, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2;

    switch (symbol)
    {
        case SlotSymbol.Cherry:
        {
            Color stem = new Color((byte)40, (byte)110, (byte)40, (byte)255);
            Color leaf = new Color((byte)60, (byte)160, (byte)60, (byte)255);
            Color fruit = new Color((byte)200, (byte)30, (byte)30, (byte)255);
            Color shine = new Color((byte)255, (byte)160, (byte)160, (byte)255);

            Raylib.DrawLineEx(new Vector2(cx, cy - 4), new Vector2(cx - 8, cy - 28), 3, stem);
            Raylib.DrawLineEx(new Vector2(cx, cy - 4), new Vector2(cx + 10, cy - 30), 3, stem);
            Raylib.DrawEllipse(cx + 16, cy - 32, 9, 5, leaf);

            Raylib.DrawCircle(cx - 10, cy + 12, 13, fruit);
            Raylib.DrawCircle(cx - 14, cy + 8, 4, shine);
            Raylib.DrawCircle(cx + 12, cy + 18, 13, fruit);
            Raylib.DrawCircle(cx + 8, cy + 14, 4, shine);
            break;
        }

        case SlotSymbol.Lemon:
        {
            Color lemon = new Color((byte)240, (byte)210, (byte)40, (byte)255);
            Color shade = new Color((byte)210, (byte)180, (byte)20, (byte)255);

            Raylib.DrawEllipse(cx, cy, 20, 26, lemon);
            Raylib.DrawTriangle(
                new Vector2(cx, cy - 26),
                new Vector2(cx - 5, cy - 32),
                new Vector2(cx + 5, cy - 32),
                shade);
            Raylib.DrawTriangle(
                new Vector2(cx, cy + 26),
                new Vector2(cx - 5, cy + 32),
                new Vector2(cx + 5, cy + 32),
                shade);
            Raylib.DrawEllipse(cx - 6, cy - 6, 6, 9, new Color((byte)255,(byte)235,(byte)140,(byte)180));
            break;
        }

        case SlotSymbol.Bell:
        {
            Color gold = new Color((byte)220, (byte)175, (byte)30, (byte)255);
            Color goldDark = new Color((byte)170, (byte)130, (byte)10, (byte)255);

            Raylib.DrawCircle(cx, cy - 18, 5, gold);
            Raylib.DrawCircleLines(cx, cy - 18, 5, goldDark);
            Raylib.DrawTriangle(
                new Vector2(cx - 22, cy + 14),
                new Vector2(cx + 22, cy + 14),
                new Vector2(cx, cy - 14),
                gold);
            Raylib.DrawRectangle(cx - 24, cy + 12, 48, 8, goldDark);
            Raylib.DrawCircle(cx, cy + 26, 5, goldDark);
            break;
        }

        case SlotSymbol.Star:
            DrawStarShape(cx, cy, 26, new Color((byte)80, (byte)170, (byte)255, (byte)255));
            break;

        case SlotSymbol.Seven:
        {
            string text = "7";
            int fontSize = 56;
            int tw = Program.MeasureTextUI(text, fontSize);
            Program.DrawTextUI(text, cx - tw / 2 + 2, cy - fontSize / 2 + 2, fontSize, new Color((byte)120,(byte)0,(byte)90,(byte)120));
            Program.DrawTextUI(text, cx - tw / 2, cy - fontSize / 2, fontSize, new Color((byte)220,(byte)20,(byte)160,(byte)255));
            Raylib.DrawCircle(cx - 28, cy - 22, 3, Color.Gold);
            Raylib.DrawCircle(cx + 30, cy + 20, 3, Color.Gold);
            Raylib.DrawCircle(cx + 26, cy - 26, 2, Color.Gold);
            break;
        }
    }
}

static void DrawStarShape(int cx, int cy, int radius, Color color)
{
    const int points = 5;
    float innerRadius = radius * 0.45f;
    Vector2[] outer = new Vector2[points];
    Vector2[] inner = new Vector2[points];

    for (int i = 0; i < points; i++)
    {
        float angleOuter = -MathF.PI / 2 + i * (2 * MathF.PI / points);
        float angleInner = angleOuter + MathF.PI / points;
        outer[i] = new Vector2(cx + MathF.Cos(angleOuter) * radius, cy + MathF.Sin(angleOuter) * radius);
        inner[i] = new Vector2(cx + MathF.Cos(angleInner) * innerRadius, cy + MathF.Sin(angleInner) * innerRadius);
    }

    for (int i = 0; i < points; i++)
        Raylib.DrawTriangle(new Vector2(cx, cy), inner[i], inner[(i + 1) % points], color);

    for (int i = 0; i < points; i++)
    {
        int next = (i + 1) % points;
        Raylib.DrawTriangle(outer[i], inner[i], new Vector2(cx, cy), color);
        Raylib.DrawTriangle(outer[i], new Vector2(cx, cy), inner[next], color);
    }
}
    }
}
