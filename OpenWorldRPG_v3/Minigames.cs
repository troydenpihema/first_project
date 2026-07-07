using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
   class PoolGame
{
    public bool IsOpen = false;
    public int TableIndex = -1;

    public const int TableLeft = 140;
    public const int TableTop = 160;
    public const int TableWidth = 1000;
    public const int TableHeight = 460;
    public const float BallRadius = 14f;
    public const float Friction = 0.985f;
    public const float MinSpeed = 8f;

    public Vector2[] BallPos = new Vector2[16];
    public Vector2[] BallVel = new Vector2[16];
    public bool[] Potted = new bool[16];
    public Vector2[] Pockets;

    public bool Aiming = true;
    public bool BallsMoving = false;
    public float AimAngle = 0f;
    public float Power = 0f;
    public bool ChargingPower = false;
    public bool Scratched = false;
    public bool inputLocked = true;

    public int BallsPotted = 0;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;

    // ---- AI / turn system ----
    public bool VsAI = false;             // set in Open()
    public static bool SelectingMode = false;
    public static int modeChoice = 0; // 0 = Practice, 1 = Vs AI
    public int CurrentPlayer = 0;         // 0 = human, 1 = AI
    public int Player0Group = -1;         // -1 unassigned, 0 = solids(1-7), 1 = stripes(9-15)
    public int Player1Group = -1;
    public bool TableOpen = true;         // groups not yet assigned
    public bool PottedThisTurn = false;   // did current shooter pot one of theirs
    public bool FoulThisTurn = false;
    public int Winner = -1;

    float aiThinkTimer = 0f;
    bool aiShotQueued = false;

    public void Open(int tableIndex)
{
    IsOpen = true;
    TableIndex = tableIndex;
    SelectingMode = true;   // show the menu first
    modeChoice = 0;
    GameOver = false;
    Message = "";
    MessageTimer = 0;
    inputLocked = true;
}

// Called once the player confirms their choice on the menu
void StartGame(bool vsAI)
{
    SelectingMode = false;
    VsAI = vsAI;
    BallsPotted = 0;
    CurrentPlayer = 0;
    Player0Group = -1;
    Player1Group = -1;
    TableOpen = true;
    Winner = -1;
    GameOver = false;
    aiThinkTimer = 0f;
    aiShotQueued = false;
    inputLocked = true;
    SetupPockets();
    RackBalls();
}

    public void Close()
    {
        IsOpen = false;
        TableIndex = -1;
    }

    void SetupPockets()
    {
        int l = TableLeft, t = TableTop, w = TableWidth, h = TableHeight;
        Pockets = new Vector2[]
        {
            new Vector2(l, t),
            new Vector2(l + w / 2, t - 6),
            new Vector2(l + w, t),
            new Vector2(l, t + h),
            new Vector2(l + w / 2, t + h + 6),
            new Vector2(l + w, t + h),
        };
    }

    void RackBalls()
    {
        for (int i = 0; i < 16; i++) { Potted[i] = false; BallVel[i] = Vector2.Zero; }

        BallPos[0] = new Vector2(TableLeft + TableWidth * 0.25f, TableTop + TableHeight / 2f);

        float startX = TableLeft + TableWidth * 0.65f;
        float startY = TableTop + TableHeight / 2f;
        float spacing = BallRadius * 2.1f;

        int[] order = { 1, 9, 2, 10, 8, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15 };
        int idx = 0;
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col <= row; col++)
            {
                float x = startX + row * spacing * 0.87f;
                float y = startY - row * spacing / 2f + col * spacing;
                BallPos[order[idx]] = new Vector2(x, y);
                idx++;
            }
        }

        AimAngle = 0f;
        Power = 0f;
        Aiming = true;
        BallsMoving = false;
        Scratched = false;
    }

    // ---- helpers for groups ----
    bool IsSolid(int i)  => i >= 1 && i <= 7;
    bool IsStripe(int i) => i >= 9 && i <= 15;

    int GroupOf(int i)
    {
        if (IsSolid(i)) return 0;
        if (IsStripe(i)) return 1;
        return -1; // cue or 8-ball
    }

    int GroupForPlayer(int p) => p == 0 ? Player0Group : Player1Group;

    bool AllGroupPotted(int group)
    {
        for (int i = 1; i <= 15; i++)
        {
            if (i == 8) continue;
            if (GroupOf(i) == group && !Potted[i]) return false;
        }
        return true;
    }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        // ---- mode selection screen ----
    if (SelectingMode)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.Up))   modeChoice = 0;
        if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.Down)) modeChoice = 1;
        if (Raylib.IsKeyPressed(KeyboardKey.One))  modeChoice = 0;
        if (Raylib.IsKeyPressed(KeyboardKey.Two))  modeChoice = 1;

        // mouse hover + click
        Vector2 mouse = Raylib.GetMousePosition();
        Rectangle practiceBtn = new Rectangle(1280 / 2 - 320, 340, 280, 120);
        Rectangle aiBtn       = new Rectangle(1280 / 2 + 40, 340, 280, 120);
        if (Raylib.CheckCollisionPointRec(mouse, practiceBtn)) modeChoice = 0;
        if (Raylib.CheckCollisionPointRec(mouse, aiBtn))       modeChoice = 1;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(mouse, practiceBtn)) StartGame(false);
            if (Raylib.CheckCollisionPointRec(mouse, aiBtn))       StartGame(true);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
            StartGame(modeChoice == 1);

        return; // nothing else runs while the menu is up
    }

        if (BallsMoving)
        {
            UpdatePhysics(dt, player);
            return;
        }

        if (GameOver) return;

        if (inputLocked)
        {
            if (Raylib.IsKeyUp(KeyboardKey.Space))
                inputLocked = false;
            return;
        }

        // ---- AI turn ----
        if (VsAI && CurrentPlayer == 1)
        {
            aiThinkTimer += dt;
            if (!aiShotQueued)
            {
                PlanAIShot();
                aiShotQueued = true;
            }
            // small delay so the AI doesn't snap-shoot
            if (aiThinkTimer > 0.9f)
            {
                aiThinkTimer = 0f;
                aiShotQueued = false;
                Shoot();
            }
            return;
        }

        // ---- Human turn ----
        if (Raylib.IsKeyDown(KeyboardKey.Left))  AimAngle -= 1.6f * dt;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) AimAngle += 1.6f * dt;

        if (Raylib.IsKeyDown(KeyboardKey.Space))
        {
            ChargingPower = true;
            Power = Math.Min(1f, Power + dt * 0.8f);
        }
        else if (ChargingPower)
        {
            ChargingPower = false;
            Shoot();
        }
    }

    // ---- AI picks a target ball + pocket and aims the cue at the contact point ----
    void PlanAIShot()
    {
        int myGroup = GroupForPlayer(1);

        // Build list of legal target balls
        List<int> targets = new List<int>();
        if (TableOpen || myGroup == -1)
        {
            for (int i = 1; i <= 15; i++)
                if (i != 8 && !Potted[i]) targets.Add(i);
        }
        else
        {
            for (int i = 1; i <= 15; i++)
                if (i != 8 && GroupOf(i) == myGroup && !Potted[i]) targets.Add(i);
            // if my group is cleared, go for the 8
            if (targets.Count == 0 && AllGroupPotted(myGroup) && !Potted[8])
                targets.Add(8);
        }
        if (targets.Count == 0)
        {
            for (int i = 1; i <= 15; i++) if (!Potted[i]) targets.Add(i);
        }

        Vector2 cue = BallPos[0];
        float bestScore = float.MaxValue;
        float bestAngle = AimAngle;
        float bestPower = 0.6f;

        // Evaluate every (target ball, pocket) pair, pick the easiest
        foreach (int ball in targets)
        {
            Vector2 bp = BallPos[ball];
            foreach (var pocket in Pockets)
            {
                // direction the ball must travel to reach the pocket
                Vector2 ballToPocket = pocket - bp;
                float ballToPocketDist = ballToPocket.Length();
                if (ballToPocketDist < 1f) continue;
                Vector2 ballToPocketDir = ballToPocket / ballToPocketDist;

                // the "ghost ball" position: where the cue must strike from
                Vector2 ghost = bp - ballToPocketDir * (BallRadius * 2f);

                Vector2 cueToGhost = ghost - cue;
                float cueToGhostDist = cueToGhost.Length();
                if (cueToGhostDist < 1f) continue;
                Vector2 cueToGhostDir = cueToGhost / cueToGhostDist;

                // cut angle: how square the hit is (1 = straight, 0 = impossible thin cut)
                float cut = Vector2.Dot(cueToGhostDir, ballToPocketDir);
                if (cut <= 0.1f) continue; // behind the ball, skip

                // lower score = better shot: prefer square cuts and shorter distances
                float score = (1f - cut) * 1000f + ballToPocketDist * 0.5f + cueToGhostDist * 0.3f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestAngle = MathF.Atan2(cueToGhostDir.Y, cueToGhostDir.X);
                    // power scales with total distance the cue+object ball travel
                    float totalDist = cueToGhostDist + ballToPocketDist;
                    bestPower = Math.Clamp(0.35f + totalDist / 1600f, 0.35f, 0.95f);
                }
            }
        }

        // add a little inaccuracy so the AI is beatable
        float miss = (Raylib.GetRandomValue(-100, 100) / 100f) * 0.05f;
        AimAngle = bestAngle + miss;
        Power = bestPower;
    }

    void Shoot()
    {
        float speed = 200f + Power * 1400f;
        BallVel[0] = new Vector2(MathF.Cos(AimAngle), MathF.Sin(AimAngle)) * speed;
        Power = 0f;
        BallsMoving = true;
        Aiming = false;
        ChargingPower = false;

        // reset per-turn tracking
        PottedThisTurn = false;
        FoulThisTurn = false;
        firstHitGroup = -2; // -2 = nothing hit yet
        firstHitWasCue = false;
    }

    int firstHitGroup = -2;
    bool firstHitWasCue = false;

    void UpdatePhysics(float dt, Player player)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Potted[i]) continue;
            BallPos[i] += BallVel[i] * dt;
            BallVel[i] *= Friction;
            if (BallVel[i].Length() < MinSpeed) BallVel[i] = Vector2.Zero;

            if (BallPos[i].X - BallRadius < TableLeft) { BallPos[i].X = TableLeft + BallRadius; BallVel[i].X *= -0.85f; }
            if (BallPos[i].X + BallRadius > TableLeft + TableWidth) { BallPos[i].X = TableLeft + TableWidth - BallRadius; BallVel[i].X *= -0.85f; }
            if (BallPos[i].Y - BallRadius < TableTop) { BallPos[i].Y = TableTop + BallRadius; BallVel[i].Y *= -0.85f; }
            if (BallPos[i].Y + BallRadius > TableTop + TableHeight) { BallPos[i].Y = TableTop + TableHeight - BallRadius; BallVel[i].Y *= -0.85f; }
        }

        for (int i = 0; i < 16; i++)
        {
            if (Potted[i]) continue;
            for (int j = i + 1; j < 16; j++)
            {
                if (Potted[j]) continue;
                Vector2 diff = BallPos[j] - BallPos[i];
                float dist = diff.Length();
                if (dist < BallRadius * 2 && dist > 0.001f)
                {
                    // record first ball the cue contacts (for foul detection)
                    if (firstHitGroup == -2 && (i == 0 || j == 0))
                    {
                        int other = (i == 0) ? j : i;
                        if (other == 8) firstHitGroup = 2;       // 8-ball
                        else firstHitGroup = GroupOf(other);     // 0 or 1
                    }

                    Vector2 normal = diff / dist;
                    float overlap = BallRadius * 2 - dist;
                    BallPos[i] -= normal * (overlap / 2f);
                    BallPos[j] += normal * (overlap / 2f);

                    Vector2 relVel = BallVel[i] - BallVel[j];
                    float speedAlongNormal = Vector2.Dot(relVel, normal);
                    if (speedAlongNormal > 0)
                    {
                        Vector2 impulse = normal * speedAlongNormal;
                        BallVel[i] -= impulse;
                        BallVel[j] += impulse;
                    }
                }
            }
        }

        for (int i = 0; i < 16; i++)
        {
            if (Potted[i]) continue;
            foreach (var pocket in Pockets)
            {
                if (Vector2.Distance(BallPos[i], pocket) < 22f)
                {
                    Potted[i] = true;
                    BallVel[i] = Vector2.Zero;

                    if (i == 0)
                    {
                        Scratched = true;
                        FoulThisTurn = true;
                        Message = "Scratch! Cue ball potted.";
                        MessageTimer = 2f;
                    }
                    else if (i == 8)
                    {
                        HandleEightBall(player);
                    }
                    else
                    {
                        BallsPotted++;
                        player.Money += 3;
                        HandleObjectBallPotted(i);
                    }
                    break;
                }
            }
        }

        bool anyMoving = false;
        for (int i = 0; i < 16; i++)
            if (!Potted[i] && BallVel[i].Length() > 0.1f) anyMoving = true;

        if (!anyMoving)
        {
            BallsMoving = false;
            Aiming = true;
            ResolveTurn(player);
        }
    }

    void HandleObjectBallPotted(int i)
    {
        // First legal pot on an open table assigns groups
        if (TableOpen)
        {
            int g = GroupOf(i);
            if (CurrentPlayer == 0) { Player0Group = g; Player1Group = 1 - g; }
            else                    { Player1Group = g; Player0Group = 1 - g; }
            TableOpen = false;
            Message = (g == 0 ? "Solids" : "Stripes") + $" go to {(CurrentPlayer == 0 ? "You" : "AI")}!";
            MessageTimer = 1.8f;
        }

        if (GroupOf(i) == GroupForPlayer(CurrentPlayer))
            PottedThisTurn = true;
        else
            Message = "Potted opponent's ball.";
    }

    void HandleEightBall(Player player)
    {
        // Potting the 8 is only a win if your group is already cleared
        bool cleared = !TableOpen && AllGroupPotted(GroupForPlayer(CurrentPlayer));
        if (cleared && !Scratched)
        {
            Winner = CurrentPlayer;
            Message = CurrentPlayer == 0 ? "8-Ball potted! YOU WIN!" : "AI pots the 8. You lose.";
        }
        else
        {
            Winner = 1 - CurrentPlayer; // potting 8 early = loss
            Message = CurrentPlayer == 0 ? "8-Ball too early! You lose." : "AI potted 8 early. YOU WIN!";
        }
        MessageTimer = 3f;
        GameOver = true;
    }

    void ResolveTurn(Player player)
    {
        if (GameOver)
        {
            // respot cue if needed not relevant; just stop
            return;
        }

        // foul: no ball hit, or first contact was not the shooter's group
        int myGroup = GroupForPlayer(CurrentPlayer);
        if (firstHitGroup == -2) FoulThisTurn = true;
        else if (!TableOpen && myGroup != -1 && firstHitGroup != myGroup && firstHitGroup != 2)
        {
            // hitting the 8 first is only legal once your group is cleared
            FoulThisTurn = true;
        }
        else if (!TableOpen && firstHitGroup == 2 && !AllGroupPotted(myGroup))
        {
            FoulThisTurn = true; // hit 8 first illegally
        }

        if (Scratched)
        {
            BallPos[0] = new Vector2(TableLeft + TableWidth * 0.25f, TableTop + TableHeight / 2f);
            BallVel[0] = Vector2.Zero;
            Potted[0] = false;
            Scratched = false;
            FoulThisTurn = true;
        }

        // keep shooting if you legally potted one of yours and didn't foul
        bool continueTurn = PottedThisTurn && !FoulThisTurn;

        if (!continueTurn)
        {
            CurrentPlayer = 1 - CurrentPlayer;
            aiThinkTimer = 0f;
            aiShotQueued = false;
            if (MessageTimer <= 0)
            {
                Message = CurrentPlayer == 0 ? "Your turn." : "AI's turn.";
                MessageTimer = 1.5f;
            }
        }

        AimAngle = 0f;
        Power = 0f;
    }

    public void Draw(Player player)
    {
         // ---- mode selection screen ----
    if (SelectingMode)
    {
        Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)15, (byte)45, (byte)20, (byte)255));
        string title = "8-BALL POOL";
        int tw = Program.MeasureTextUI(title, 60);
        Program.DrawTextUI(title, 1280 / 2 - tw / 2, 150, 60, Color.Gold);

        string sub = "Choose a mode";
        int sw = Program.MeasureTextUI(sub, 26);
        Program.DrawTextUI(sub, 1280 / 2 - sw / 2, 250, 26, Color.LightGray);

        Rectangle practiceBtn = new Rectangle(1280 / 2 - 320, 340, 280, 120);
        Rectangle aiBtn       = new Rectangle(1280 / 2 + 40, 340, 280, 120);

        DrawModeButton(practiceBtn, "PRACTICE", "Pot freely, solo", modeChoice == 0);
        DrawModeButton(aiBtn,       "VS AI",     "Play a full match", modeChoice == 1);

        string hint = "LEFT/RIGHT or mouse to choose  -  ENTER to start  -  ESC to leave";
        int hw = Program.MeasureTextUI(hint, 18);
        Program.DrawTextUI(hint, 1280 / 2 - hw / 2, 520, 18, Color.Gray);
        return;
    }
        Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)20, (byte)60, (byte)20, (byte)255));

        Raylib.DrawRectangle(TableLeft, TableTop, TableWidth, TableHeight, new Color((byte)10, (byte)110, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(TableLeft - 12, TableTop - 12, TableWidth + 24, TableHeight + 24), 12,
            new Color((byte)90, (byte)55, (byte)25, (byte)255));

        foreach (var p in Pockets)
            Raylib.DrawCircle((int)p.X, (int)p.Y, 18, Color.Black);

        for (int i = 0; i < 16; i++)
            if (!Potted[i]) DrawBall(i);

        // only draw the aim guide on the human's turn
        bool humanAiming = Aiming && !BallsMoving && !Potted[0] && !(VsAI && CurrentPlayer == 1);
        if (humanAiming)
        {
            Vector2 dir = new Vector2(MathF.Cos(AimAngle), MathF.Sin(AimAngle));
            Vector2 cueBallPos = BallPos[0];

            for (float t = BallRadius + 6; t < 300; t += 14)
            {
                Vector2 pt = cueBallPos + dir * t;
                Raylib.DrawCircle((int)pt.X, (int)pt.Y, 2, new Color((byte)255, (byte)255, (byte)255, (byte)150));
            }

            float pullback = 20 + Power * 60;
            Vector2 stickStart = cueBallPos - dir * (BallRadius + pullback);
            Vector2 stickEnd = cueBallPos - dir * (BallRadius + pullback + 140);
            Raylib.DrawLineEx(stickStart, stickEnd, 5, new Color((byte)180, (byte)140, (byte)80, (byte)255));
        }

        if (ChargingPower)
        {
            Raylib.DrawRectangle(TableLeft, TableTop + TableHeight + 30, 200, 20, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(TableLeft, TableTop + TableHeight + 30, (int)(200 * Power), 20, Color.Red);
            Raylib.DrawRectangleLines(TableLeft, TableTop + TableHeight + 30, 200, 20, Color.White);
            Program.DrawTextUI("POWER", TableLeft, TableTop + TableHeight + 54, 16, Color.White);
        }

        Program.DrawTextUI("8-BALL POOL", 1280 / 2 - 110, 30, 40, Color.Gold);

        // turn + group HUD
        string turnText = VsAI
            ? (CurrentPlayer == 0 ? "YOUR TURN" : "AI THINKING...")
            : "8-Ball Pool";
        Program.DrawTextUI(turnText, TableLeft, TableTop - 70, 24,
            CurrentPlayer == 0 ? Color.SkyBlue : Color.Orange);

        string groupText = TableOpen ? "Table open"
            : $"You: {(Player0Group == 0 ? "Solids" : "Stripes")}  |  AI: {(Player1Group == 0 ? "Solids" : "Stripes")}";
        Program.DrawTextUI(groupText, TableLeft, TableTop - 40, 22, Color.White);
        Program.DrawTextUI($"Wallet: ${player.Money}", TableLeft + TableWidth - 200, TableTop - 40, 22, Color.White);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 26);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, TableTop + TableHeight + 80, 26, Color.Yellow);
        }

        if (GameOver)
        {
            string winText = Winner == 0 ? "YOU WIN!" : "AI WINS!";
            int ww = Program.MeasureTextUI(winText, 34);
            Program.DrawTextUI(winText, 1280 / 2 - ww / 2, TableTop + TableHeight / 2 - 20, 34,
                Winner == 0 ? Color.Gold : Color.Red);
            Program.DrawTextUI("ESC = Leave Table", 1280 / 2 - 90, TableTop + TableHeight / 2 + 30, 20, Color.LightGray);
        }
        else
        {
            string prompt = BallsMoving ? "Balls rolling..."
                : (VsAI && CurrentPlayer == 1) ? "AI is taking its shot..."
                : "LEFT/RIGHT = Aim | Hold SPACE = Power | Release = Shoot | ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 40, 18, Color.LightGray);
        }
    }

    void DrawBall(int i)
    {
        Vector2 pos = BallPos[i];

        if (i == 0)
        {
            Raylib.DrawCircle((int)pos.X, (int)pos.Y, BallRadius, Color.White);
            Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, BallRadius, Color.LightGray);
            return;
        }

        Color color = GetBallColor(i);
        bool stripe = i >= 9;

        Raylib.DrawCircle((int)pos.X, (int)pos.Y, BallRadius, stripe ? Color.White : color);
        if (stripe)
            Raylib.DrawRectangle((int)(pos.X - BallRadius), (int)(pos.Y - 5), (int)(BallRadius * 2), 10, color);

        Raylib.DrawCircle((int)(pos.X - 4), (int)(pos.Y - 4), 4, new Color((byte)255, (byte)255, (byte)255, (byte)160));

        Raylib.DrawCircle((int)pos.X, (int)pos.Y, 7, Color.White);
        string numText = i.ToString();
        int tw = Program.MeasureTextUI(numText, 12);
        Program.DrawTextUI(numText, (int)pos.X - tw / 2, (int)pos.Y - 6, 12, Color.Black);
    }

    Color GetBallColor(int i)
    {
        int n = i > 8 ? i - 8 : i;
        return n switch
        {
            1 => Color.Yellow,
            2 => Color.Blue,
            3 => Color.Red,
            4 => Color.Purple,
            5 => Color.Orange,
            6 => Color.DarkGreen,
            7 => new Color((byte)120, (byte)40, (byte)20, (byte)255),
            8 => Color.Black,
            _ => Color.Gray
        };
    }
void DrawModeButton(Rectangle r, string label, string desc, bool selected)
{
    Color fill   = selected ? new Color((byte)40, (byte)110, (byte)55, (byte)255)
                            : new Color((byte)25, (byte)60, (byte)35, (byte)255);
    Color border = selected ? Color.Gold : new Color((byte)80, (byte)80, (byte)80, (byte)255);

    Raylib.DrawRectangleRec(r, fill);
    Raylib.DrawRectangleLinesEx(r, selected ? 4 : 2, border);

    int lw = Program.MeasureTextUI(label, 32);
    Program.DrawTextUI(label, (int)(r.X + r.Width / 2 - lw / 2), (int)(r.Y + 28), 32,
        selected ? Color.White : Color.LightGray);

    int dw = Program.MeasureTextUI(desc, 16);
    Program.DrawTextUI(desc, (int)(r.X + r.Width / 2 - dw / 2), (int)(r.Y + 72), 16, Color.LightGray);
}
}
class DartsGame
{
    public bool IsOpen = false;

    // ---- board geometry ----
    const int BoardCX = 640;          // centre of the play board (screen space)
    const int BoardCY = 360;
    const float RBull = 12f;          // bullseye (50)
    const float ROuterBull = 26f;     // outer bull (25)
    const float RTripleInner = 95f;   // triple ring band
    const float RTripleOuter = 110f;
    const float RDoubleInner = 165f;
    const float RDoubleOuter = 180f;  // outside this = miss

    // standard dartboard number order, clockwise from top (12 o'clock = 20)
    static readonly int[] Sectors =
        { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };

    // ---- game state ----
    public bool VsAI = false;
    public int[] Scores = new int[2];       // remaining score per player
    public int CurrentPlayer = 0;           // 0 = human, 1 = AI
    public int DartsLeft = 3;
    public int TurnStartScore;              // to restore on a bust
    public bool GameOver = false;
    public int Winner = -1;

    public string Message = "";
    public float MessageTimer = 0f;
    public bool inputLocked = true;

    // aiming reticle
    Vector2 aim;
    Vector2 aimVel;
    float driftTimer = 0f;

    // last throws (for showing dart markers on the board)
    System.Collections.Generic.List<Vector2> marks = new();

    // AI pacing
    float aiTimer = 0f;
    bool aiThrowQueued = false;
    Vector2 aiAim;

    Random rng = new Random();

    public void Open(bool vsAI)
    {
        IsOpen = true;
        VsAI = vsAI;
        Scores[0] = 501;
        Scores[1] = 501;
        CurrentPlayer = 0;
        DartsLeft = 3;
        TurnStartScore = 501;
        GameOver = false;
        Winner = -1;
        marks.Clear();
        inputLocked = true;
        aim = new Vector2(BoardCX, BoardCY);
        aimVel = new Vector2(120, 90);
        Message = vsAI ? "501 vs AI. SPACE to throw." : "501 Practice. SPACE to throw.";
        MessageTimer = 2.5f;
    }

    public void Close() => IsOpen = false;

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;
        if (GameOver) return;

        if (inputLocked)
        {
            if (Raylib.IsKeyUp(KeyboardKey.Space)) inputLocked = false;
            return;
        }

        // ---- AI turn ----
        if (VsAI && CurrentPlayer == 1)
        {
            aiTimer += dt;
            if (!aiThrowQueued)
            {
                aiAim = PlanAIThrow();
                aiThrowQueued = true;
            }
            if (aiTimer > 1.0f)
            {
                aiTimer = 0f;
                aiThrowQueued = false;
                ThrowAt(aiAim, player);
            }
            return;
        }

        // ---- human turn: reticle drifts, SPACE to throw ----
        // wander the aim velocity a little so it isn't a straight line
        driftTimer += dt;
        if (driftTimer > 0.4f)
        {
            driftTimer = 0f;
            aimVel.X += (float)(rng.NextDouble() - 0.5) * 120;
            aimVel.Y += (float)(rng.NextDouble() - 0.5) * 120;
            float max = 220f;
            aimVel = Vector2.Clamp(aimVel, new Vector2(-max, -max), new Vector2(max, max));
        }

        aim += aimVel * dt;
        // bounce the reticle around the board area so it stays on the board
        if (aim.X < BoardCX - RDoubleOuter) { aim.X = BoardCX - RDoubleOuter; aimVel.X *= -1; }
        if (aim.X > BoardCX + RDoubleOuter) { aim.X = BoardCX + RDoubleOuter; aimVel.X *= -1; }
        if (aim.Y < BoardCY - RDoubleOuter) { aim.Y = BoardCY - RDoubleOuter; aimVel.Y *= -1; }
        if (aim.Y > BoardCY + RDoubleOuter) { aim.Y = BoardCY + RDoubleOuter; aimVel.Y *= -1; }

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            ThrowAt(aim, player);
    }

    // AI aims near a sensible target with some scatter (beatable)
    Vector2 PlanAIThrow()
    {
        int remaining = Scores[1];
        Vector2 target;

        if (remaining <= 40 && remaining % 2 == 0)
            target = SectorCentre(remaining / 2, RDoubleInner + 6); // go for the double to check out
        else if (remaining > 60)
            target = SectorCentre(20, RTripleInner + 4);            // treble 20
        else
            target = SectorCentre(Math.Min(remaining, 20), RTripleInner + 4);

        // scatter: tighten this to make the AI harder
        float scatter = 26f;
        target.X += (float)(rng.NextDouble() - 0.5) * scatter * 2;
        target.Y += (float)(rng.NextDouble() - 0.5) * scatter * 2;
        return target;
    }

    // centre point of a sector's number at a given radius from the bull
    Vector2 SectorCentre(int number, float radius)
    {
        int idx = Array.IndexOf(Sectors, number);
        if (idx < 0) idx = 0;
        // each sector spans 18 degrees; index 0 (=20) is centred at the top
        float angDeg = idx * 18f - 90f; // -90 so sector 20 sits at the top
        float ang = angDeg * MathF.PI / 180f;
        return new Vector2(BoardCX + MathF.Cos(ang) * radius, BoardCY + MathF.Sin(ang) * radius);
    }

    void ThrowAt(Vector2 pt, Player player)
    {
        marks.Add(pt);
        int score = ScoreAt(pt, out string label);

        int p = CurrentPlayer;
        int after = Scores[p] - score;

        if (after == 0)
        {
            // must finish on a double (or bull) — standard 501 rule
            bool legalFinish = label.StartsWith("D") || label == "BULL";
            if (legalFinish)
            {
                Scores[p] = 0;
                GameOver = true;
                Winner = p;
                Message = p == 0 ? $"{label}! YOU CHECK OUT — WIN!" : $"AI checks out with {label}. You lose.";
                MessageTimer = 4f;
                if (p == 0) player.Money += 25;
                return;
            }
            else
            {
                Bust(player, $"{label} — must finish on a double. Bust!");
                return;
            }
        }
        else if (after < 0 || after == 1)
        {
            Bust(player, $"{label} — bust!");
            return;
        }
        else
        {
            Scores[p] = after;
            Message = $"{label}  (−{score}).  {Scores[p]} left.";
            MessageTimer = 1.6f;
        }

        DartsLeft--;
        if (DartsLeft <= 0)
            EndTurn(player);
    }

    void Bust(Player player, string msg)
    {
        Scores[CurrentPlayer] = TurnStartScore; // restore
        Message = msg;
        MessageTimer = 2f;
        DartsLeft = 0;
        EndTurn(player);
    }

    void EndTurn(Player player)
    {
        CurrentPlayer = 1 - CurrentPlayer;
        DartsLeft = 3;
        TurnStartScore = Scores[CurrentPlayer];
        marks.Clear();
        aiTimer = 0f;
        aiThrowQueued = false;
        if (MessageTimer <= 0)
        {
            Message = CurrentPlayer == 0 ? "Your turn." : "AI's turn.";
            MessageTimer = 1.2f;
        }
    }

    // ---- scoring: map a point to a dart score ----
    int ScoreAt(Vector2 pt, out string label)
    {
        Vector2 d = pt - new Vector2(BoardCX, BoardCY);
        float dist = d.Length();

        if (dist <= RBull)       { label = "BULL"; return 50; }
        if (dist <= ROuterBull)  { label = "25";   return 25; }
        if (dist > RDoubleOuter) { label = "MISS"; return 0; }

        // which sector? angle measured from top, clockwise
        float ang = MathF.Atan2(d.Y, d.X) * 180f / MathF.PI; // -180..180, 0 = +X (right)
        ang += 90f;                  // rotate so top = 0
        if (ang < 0) ang += 360f;
        int idx = (int)MathF.Floor((ang + 9f) / 18f) % 20; // +9 so sectors centre on numbers
        int number = Sectors[idx];

        if (dist >= RTripleInner && dist <= RTripleOuter) { label = "T" + number; return number * 3; }
        if (dist >= RDoubleInner && dist <= RDoubleOuter) { label = "D" + number; return number * 2; }
        label = number.ToString();
        return number;
    }

    public void Draw(Player player)
    {
        Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)25, (byte)35, (byte)30, (byte)255));

        // board rings
        Raylib.DrawCircle(BoardCX, BoardCY, RDoubleOuter + 14, new Color((byte)15, (byte)15, (byte)15, (byte)255));
        Raylib.DrawCircle(BoardCX, BoardCY, RDoubleOuter, new Color((byte)235, (byte)225, (byte)200, (byte)255));

        // sector wedges (alternating shade) + numbers
        for (int i = 0; i < 20; i++)
        {
            float a0 = (i * 18f - 9f - 90f) * MathF.PI / 180f;
            float a1 = (i * 18f + 9f - 90f) * MathF.PI / 180f;
            Color wedge = (i % 2 == 0)
                ? new Color((byte)210, (byte)200, (byte)170, (byte)255)
                : new Color((byte)60, (byte)55, (byte)45, (byte)255);
            // crude wedge via triangle fan
            Vector2 c = new Vector2(BoardCX, BoardCY);
            Vector2 p0 = c + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * RDoubleOuter;
            Vector2 p1 = c + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * RDoubleOuter;
            Raylib.DrawTriangle(c, p1, p0, wedge); // winding order matters in raylib

            // number label outside the ring
            Vector2 np = SectorCentre(Sectors[i], RDoubleOuter + 24);
            string ns = Sectors[i].ToString();
            int nw = Program.MeasureTextUI(ns, 16);
            Program.DrawTextUI(ns, (int)np.X - nw / 2, (int)np.Y - 8, 16, Color.White);
        }

        // double & triple rings (drawn as thin outlines)
        Raylib.DrawRing(new Vector2(BoardCX, BoardCY), RTripleInner, RTripleOuter, 0, 360, 64, new Color((byte)180,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRing(new Vector2(BoardCX, BoardCY), RDoubleInner, RDoubleOuter, 0, 360, 64, new Color((byte)0,(byte)120,(byte)50,(byte)255));

        // bull
        Raylib.DrawCircle(BoardCX, BoardCY, ROuterBull, new Color((byte)0, (byte)110, (byte)40, (byte)255));
        Raylib.DrawCircle(BoardCX, BoardCY, RBull, new Color((byte)180, (byte)40, (byte)40, (byte)255));

        // existing dart marks this turn
        foreach (var m in marks)
        {
            Raylib.DrawCircle((int)m.X, (int)m.Y, 3, Color.Yellow);
            Raylib.DrawCircleLines((int)m.X, (int)m.Y, 5, Color.Black);
        }

        // aiming reticle (human turn only)
        if (!GameOver && !(VsAI && CurrentPlayer == 1) && !inputLocked)
        {
            Raylib.DrawCircleLines((int)aim.X, (int)aim.Y, 10, Color.White);
            Raylib.DrawLineEx(new Vector2(aim.X - 14, aim.Y), new Vector2(aim.X + 14, aim.Y), 2, Color.White);
            Raylib.DrawLineEx(new Vector2(aim.X, aim.Y - 14), new Vector2(aim.X, aim.Y + 14), 2, Color.White);
        }

        // HUD
        Program.DrawTextUI("501 DARTS", 1280 / 2 - 90, 30, 36, Color.Gold);

        Color p0col = CurrentPlayer == 0 ? Color.SkyBlue : Color.Gray;
        Color p1col = CurrentPlayer == 1 ? Color.Orange  : Color.Gray;
        Program.DrawTextUI($"YOU: {Scores[0]}", 120, 100, 30, p0col);
        Program.DrawTextUI(VsAI ? $"AI: {Scores[1]}" : $"P2: {Scores[1]}", 1280 - 220, 100, 30, p1col);

        if (!GameOver)
        {
            string dots = new string('|', DartsLeft);
            Program.DrawTextUI($"Darts left: {dots}", 120, 140, 22, Color.White);
        }

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 26);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, 600, 26, Color.Yellow);
        }

        if (GameOver)
        {
            string w = Winner == 0 ? "YOU WIN!" : (VsAI ? "AI WINS!" : "PLAYER 2 WINS!");
            int ww = Program.MeasureTextUI(w, 40);
            Program.DrawTextUI(w, 1280 / 2 - ww / 2, 650, 40, Winner == 0 ? Color.Gold : Color.Red);
        }

        string prompt = GameOver ? "ESC = Leave"
            : (VsAI && CurrentPlayer == 1) ? "AI is throwing..."
            : "SPACE = Throw  |  ESC = Leave";
        int pw = Program.MeasureTextUI(prompt, 18);
        Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 40, 18, Color.LightGray);
    }
}
 class PokieMachine
{
    public bool IsOpen = false;          // are we currently in the minigame screen for this machine?
    public int MachineIndex = -1;        // which machine in the array (for returning to the right spot)

    public int BetAmount = 5;
    public int[] BetOptions = { 5, 10, 25, 50 };

    public SlotSymbol[] Reels = new SlotSymbol[3];

    public bool IsSpinning = false;
    public float[] ReelStopTimers = new float[3]; // staggered stop times per reel
    public float SpinElapsed = 0f;
    public float SymbolCycleTimer = 0f;
    public const float SymbolCycleSpeed = 0.07f; // how fast symbols flick by while spinning

    public string ResultMessage = "";
    public float ResultTimer = 0f;
    public bool HasSpunOnce = false;
    public int[] GetAvailableBets(Player player)
{
    var bets = new System.Collections.Generic.List<int>();
    bets.Add(5);
    if (player.GamblingLevel >= 5)  bets.Add(10);
    if (player.GamblingLevel >= 10) bets.Add(25);
    if (player.GamblingLevel >= 20) bets.Add(50);
    return bets.ToArray();
}

    public void Open(int machineIndex)
    {
        IsOpen = true;
        MachineIndex = machineIndex;
        HasSpunOnce = false;
        ResultMessage = "";
        ResultTimer = 0f;
        // give it some default symbols to display before first spin
        for (int i = 0; i < Reels.Length; i++)
            Reels[i] = SlotData.GetRandomSymbol();
    }

    public void Close()
    {
        IsOpen = false;
        IsSpinning = false;
        MachineIndex = -1;
    }

    public void StartSpin(Player player)
    {
        if (IsSpinning) return;
        if (player.Money < BetAmount)
        {
            ResultMessage = "Not enough cash for that bet!";
            ResultTimer = 1.5f;
            return;
        }

        player.Money -= BetAmount;
        IsSpinning = true;
        HasSpunOnce = true;
        SpinElapsed = 0f;
        SymbolCycleTimer = 0f;
        ResultMessage = "";

        // each reel stops a little after the previous one
        ReelStopTimers[0] = 1.0f;
        ReelStopTimers[1] = 1.5f;
        ReelStopTimers[2] = 2.0f;
    }

    public void Update(float dt, Player player)
    {
        if (ResultTimer > 0f) ResultTimer -= dt;

        if (!IsSpinning) return;

        SpinElapsed += dt;
        SymbolCycleTimer += dt;

        // flick the symbols rapidly while any reel is still "spinning"
        if (SymbolCycleTimer >= SymbolCycleSpeed)
        {
            SymbolCycleTimer = 0f;
            for (int i = 0; i < Reels.Length; i++)
            {
                if (SpinElapsed < ReelStopTimers[i])
                    Reels[i] = SlotData.GetRandomSymbol();
            }
        }

        // once the last reel passes its stop time, spin is done -> evaluate
        if (SpinElapsed >= ReelStopTimers[^1])
        {
            IsSpinning = false;
            EvaluateResult(player);
        }
    }

    private void EvaluateResult(Player player)
{
    bool allMatch = Reels[0] == Reels[1] && Reels[1] == Reels[2];

    // find which symbol appears twice (if any)
    SlotSymbol twoMatchSymbol = SlotSymbol.Cherry;
    bool twoMatch = false;
    if (!allMatch)
    {
        if (Reels[0] == Reels[1]) { twoMatch = true; twoMatchSymbol = Reels[0]; }
        else if (Reels[1] == Reels[2]) { twoMatch = true; twoMatchSymbol = Reels[1]; }
        else if (Reels[0] == Reels[2]) { twoMatch = true; twoMatchSymbol = Reels[0]; }
    }

    if (allMatch)
    {
        int multiplier = SlotData.ThreeMatchMultiplier[Reels[0]];
        int winnings = BetAmount * multiplier;
        player.Money += winnings;

        ResultMessage = Reels[0] switch
        {
            SlotSymbol.Seven   => $"JACKPOT! Triple 7s — ${winnings}!",
            SlotSymbol.Star    => $"Triple Stars! You won ${winnings}!",
            SlotSymbol.Bell    => $"Triple Bells! You won ${winnings}!",
            SlotSymbol.Lemon   => $"Triple Lemons! You won ${winnings}!",
            SlotSymbol.Cherry  => $"Triple Cherries! You won ${winnings}!",
            _ => $"Triple match! You won ${winnings}!"
        };
    }
    else if (twoMatch)
    {
        float mult = SlotData.TwoMatchMultiplier[twoMatchSymbol];
        int winnings = (int)(BetAmount * mult);

        if (winnings <= 0)
        {
            ResultMessage = $"Two {twoMatchSymbol}s - no payout. Lost ${BetAmount}.";
        }
        else
        {
            player.Money += winnings;
            ResultMessage = twoMatchSymbol == SlotSymbol.Seven
                ? $"Two 7s! Nice — you got ${winnings} back."
                : $"Two {twoMatchSymbol}s — ${winnings} back.";
        }
    }
    else
    {
        ResultMessage = $"No match. Lost ${BetAmount}.";
    }
    // after allMatch block — three of a kind pays more XP
if (allMatch)
{
    // ... existing winnings code ...
    player.AddGamblingXP(30);  // big win = more XP
}
else if (twoMatch)
{
    // ... existing winnings code ...
    player.AddGamblingXP(15);
}
else
{
    ResultMessage = $"No match. Lost ${BetAmount}.";
    player.AddGamblingXP(10);  // XP even on loss
}

ResultTimer = 2.5f;
}
}
class BowlingGame
{
    public bool IsOpen = false;
    public int LaneIndex = -1;

    public const int LaneLeft = 480;
    public const int LaneTop = 120;
    public const int LaneWidth = 320;
    public const int LaneLength = 460;

    public Vector2 BallPos;
    public float BallVelY = 0f;
    public float BallVelX = 0f;
    public bool BallRolling = false;
    public float AimX = 0f;        // -1 to 1, horizontal launch angle
    public float Power = 0f;
    public bool ChargingPower = false;
    public float SettleTimer = 0f;
    public bool BallAtEnd = false;

    public bool[] PinDown = new bool[10];
    public Vector2[] PinPos = new Vector2[10];
    public Vector2[] PinVel = new Vector2[10];     
    public Vector2[] PinHome = new Vector2[10];  

    public int Frame = 1;
    public int Roll = 1;           // 1 or 2 within a frame
    public int TotalScore = 0;
    public int PinsThisFrame = 0;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;
    public bool InputLocked = false;

    public void Open(int laneIndex)
    {
        IsOpen = true;
        LaneIndex = laneIndex;
        Frame = 1; Roll = 1; TotalScore = 0; PinsThisFrame = 0;
        GameOver = false;
        InputLocked = true;
        Message = "Frame 1 - Hold SPACE to bowl!";
        MessageTimer = 2.5f;
        SetupPins();
        ResetBall();
    }

    public void Close() { IsOpen = false; LaneIndex = -1; }

    void SetupPins()
{
    float cx = LaneLeft + LaneWidth / 2f;
    float topY = LaneTop + 50;
    float rowGap = 34f;
    float pinGap = 30f;
    int idx = 0;
    for (int row = 0; row < 4; row++)
    {
        float rowY = topY + row * rowGap;
        float startX = cx - row * (pinGap / 2f);
        for (int col = 0; col <= row; col++)
        {
            Vector2 pos = new Vector2(startX + col * pinGap, rowY);
            PinPos[idx] = pos;
            PinHome[idx] = pos;
            PinVel[idx] = Vector2.Zero;
            PinDown[idx] = false;
            idx++;
        }
    }
}

    void ResetBall()
    {
        BallPos = new Vector2(LaneLeft + LaneWidth / 2f, LaneTop + LaneLength - 30);
        BallVelX = 0f; BallVelY = 0f;
        AimX = 0f; Power = 0f;
        BallRolling = false;
        ChargingPower = false;
        BallAtEnd = false;       
        SettleTimer = 0f; 
    }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.TryPayDropzonePlay()) Restart();
                else { Message = "Need credit to bowl again!"; MessageTimer = 2f; }
            }
            return;
        }

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space))
                InputLocked = false;
            return;
        }

        if (BallRolling)
        {
            UpdateRoll(dt, player);
            return;
        }

        // aim
        if (Raylib.IsKeyDown(KeyboardKey.Left))  AimX = Math.Max(-1f, AimX - 1.2f * dt);
        if (Raylib.IsKeyDown(KeyboardKey.Right)) AimX = Math.Min( 1f, AimX + 1.2f * dt);

        // power charge
        if (Raylib.IsKeyDown(KeyboardKey.Space))
        {
            ChargingPower = true;
            Power = Math.Min(1f, Power + dt * 0.7f);
        }
        else if (ChargingPower)
        {
            ChargingPower = false;
            BallVelY = -(300f + Power * 700f);     // travel up the lane
            BallVelX = AimX * 160f;
            BallRolling = true;
        }
    }

  void UpdateRoll(float dt, Player player)
{
    const float pinRadius = 9f;
    const float ballRadius = 14f;
    const float pinFriction = 0.90f;

    // move the ball
    BallPos.X += BallVelX * dt;
    BallPos.Y += BallVelY * dt;

    // gutter clamp
    if (BallPos.X < LaneLeft + 14) { BallPos.X = LaneLeft + 14; BallVelX = 0; }
    if (BallPos.X > LaneLeft + LaneWidth - 14) { BallPos.X = LaneLeft + LaneWidth - 14; BallVelX = 0; }

    // ball hits pins — transfer momentum
    for (int i = 0; i < 10; i++)
    {
        if (PinDown[i]) continue;
        Vector2 diff = PinPos[i] - BallPos;
        float dist = diff.Length();
        if (dist < ballRadius + pinRadius && dist > 0.001f)
        {
            Vector2 normal = diff / dist;
            float ballSpeed = new Vector2(BallVelX, BallVelY).Length();
            PinVel[i] += normal * (ballSpeed * 0.6f + 120f);
            BallVelX += normal.X * 30f;
        }
    }

    // update pin movement + pin-to-pin collisions
    for (int i = 0; i < 10; i++)
    {
        if (PinDown[i]) continue;
        if (PinVel[i].Length() < 1f) continue;

        PinPos[i] += PinVel[i] * dt;
        PinVel[i] *= pinFriction;

        if (Vector2.Distance(PinPos[i], PinHome[i]) > 12f)
        {
            for (int j = 0; j < 10; j++)
            {
                if (j == i || PinDown[j]) continue;
                Vector2 d = PinPos[j] - PinPos[i];
                float dd = d.Length();
                if (dd < pinRadius * 2.4f && dd > 0.001f)
                {
                    Vector2 n = d / dd;
                    PinVel[j] += n * (PinVel[i].Length() * 0.7f + 40f);
                }
            }
        }
    }

    // mark pins as down once they've travelled far enough and slowed
    for (int i = 0; i < 10; i++)
    {
        if (PinDown[i]) continue;
        if (Vector2.Distance(PinPos[i], PinHome[i]) > 18f && PinVel[i].Length() < 30f)
            PinDown[i] = true;
    }

    // ball reached the end of the lane
if (BallPos.Y <= LaneTop + 20)
{
    BallPos.Y = LaneTop + 20;
    BallVelX = 0; BallVelY = 0;
    BallAtEnd = true;
}

// once the ball is parked at the end, wait for pins to settle then resolve
if (BallAtEnd)
{
    bool pinsMoving = false;
    for (int i = 0; i < 10; i++)
        if (!PinDown[i] && PinVel[i].Length() > 25f) pinsMoving = true;

    if (pinsMoving)
        SettleTimer = 0f;        // reset timer while things still move
    else
        SettleTimer += dt;       // count up once everything is still

    // resolve after half a second of stillness, or force after 3s max
    if (SettleTimer > 0.5f)
    {
        BallAtEnd = false;
        SettleTimer = 0f;
        ResolveRoll(player);
    }
}

    void ResolveRoll(Player player)
    {
        int downNow = PinDown.Count(p => p);
        int knockedThisRoll = downNow - PinsThisFrame;

        if (Roll == 1)
        {
            PinsThisFrame = downNow;
            if (downNow == 10)
            {
                TotalScore += 10;
                player.Money += 5;
                Message = "STRIKE!  +$5";
                MessageTimer = 2.5f;
                NextFrame();
            }
            else
            {
                Message = $"Knocked {knockedThisRoll}. Roll 2!";
                MessageTimer = 2f;
                Roll = 2;
                BallRolling = false;
                ResetBall();
            }
        }
        else
        {
            TotalScore += downNow;
            if (downNow == 10)
            {
                player.Money += 3;
                Message = "SPARE!  +$3";
            }
            else
                Message = $"Frame done: {downNow} pins.";
            MessageTimer = 2.5f;
            NextFrame();
        }
    }

    void NextFrame()
    {
        Frame++;
        Roll = 1;
        PinsThisFrame = 0;
        if (Frame > 10)
        {
            GameOver = true;
            Message = $"Game over! Final score: {TotalScore}";
            MessageTimer = 6f;
        }
        else
        {
            SetupPins();
            ResetBall();
            BallRolling = false;
        }
    }
}

void Restart()
{
    Frame = 1; Roll = 1; TotalScore = 0; PinsThisFrame = 0;
    GameOver = false;
    InputLocked = true;
    Message = "Frame 1 - Hold SPACE to bowl!";
    MessageTimer = 2.5f;
    SetupPins();
    ResetBall();
}
    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)25, (byte)18, (byte)10, (byte)255));

        // lane
        Raylib.DrawRectangle(LaneLeft, LaneTop, LaneWidth, LaneLength, new Color((byte)200, (byte)160, (byte)90, (byte)255));
        // gutters
        Raylib.DrawRectangle(LaneLeft - 18, LaneTop, 18, LaneLength, new Color((byte)40, (byte)40, (byte)50, (byte)255));
        Raylib.DrawRectangle(LaneLeft + LaneWidth, LaneTop, 18, LaneLength, new Color((byte)40, (byte)40, (byte)50, (byte)255));
        // lane boards
        for (int b = 1; b < 8; b++)
            Raylib.DrawLine(LaneLeft + b * (LaneWidth / 8), LaneTop, LaneLeft + b * (LaneWidth / 8), LaneTop + LaneLength, new Color((byte)170, (byte)130, (byte)70, (byte)120));

        // pins
        for (int i = 0; i < 10; i++)
        {
            if (PinDown[i]) continue;
            float leanX = Math.Clamp(PinVel[i].X * 0.05f, -6f, 6f);
            int px = (int)(PinPos[i].X + leanX);
            int py = (int)PinPos[i].Y;
            Raylib.DrawCircle(px, py, 9, Color.White);
            Raylib.DrawCircleLines(px, py, 9, Color.Red);
            Raylib.DrawRectangle(px - 2, py - 4, 4, 3, Color.Red);
        }

        // ball
        Raylib.DrawCircle((int)BallPos.X, (int)BallPos.Y, 14, new Color((byte)30, (byte)30, (byte)160, (byte)255));
        Raylib.DrawCircle((int)BallPos.X - 4, (int)BallPos.Y - 4, 4, new Color((byte)120, (byte)120, (byte)220, (byte)255));

        // aim guide while setting up
        if (!BallRolling && !GameOver)
        {
            Vector2 aimDir = new Vector2(AimX * 0.5f, -1f);
            aimDir = Vector2.Normalize(aimDir);
            for (float t = 20; t < 200; t += 16)
            {
                Vector2 pt = BallPos + aimDir * t;
                Raylib.DrawCircle((int)pt.X, (int)pt.Y, 2, new Color((byte)255, (byte)255, (byte)255, (byte)150));
            }
        }

        // power meter
        if (ChargingPower)
        {
            Raylib.DrawRectangle(LaneLeft, LaneTop + LaneLength + 20, 200, 18, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(LaneLeft, LaneTop + LaneLength + 20, (int)(200 * Power), 18, Color.Red);
            Raylib.DrawRectangleLines(LaneLeft, LaneTop + LaneLength + 20, 200, 18, Color.White);
            Program.DrawTextUI("POWER", LaneLeft, LaneTop + LaneLength + 42, 14, Color.White);
        }

        // HUD
        Program.DrawTextUI("BOWLING", 1280 / 2 - 90, 30, 36, Color.Gold);
        Program.DrawTextUI($"Frame: {Math.Min(Frame, 10)}/10   Roll: {Roll}", 60, 100, 22, Color.White);
        Program.DrawTextUI($"Score: {TotalScore}", 60, 130, 22, Color.White);
        Program.DrawTextUI($"Wallet: ${player.Money}", 60, 160, 22, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 26);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, LaneTop + LaneLength + 70, 26, Color.Yellow);
        }

       if (GameOver)
{
    // dim the screen
    Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));

    string over = "GAME OVER";
    int ow = Program.MeasureTextUI(over, 50);
    Program.DrawTextUI(over, 1280 / 2 - ow / 2, 240, 50, Color.Gold);

    string score = $"Final Score: {TotalScore}";
    int sw = Program.MeasureTextUI(score, 32);
    Program.DrawTextUI(score, 1280 / 2 - sw / 2, 320, 32, Color.White);

    string playAgain = "SPACE = Play Again";
    int paw = Program.MeasureTextUI(playAgain, 26);
    Program.DrawTextUI(playAgain, 1280 / 2 - paw / 2, 400, 26, Color.Green);

    string quit = "ESC = Quit to Arcade";
    int qw = Program.MeasureTextUI(quit, 26);
    Program.DrawTextUI(quit, 1280 / 2 - qw / 2, 440, 26, Color.Red);
}
else
{
    string prompt = BallRolling ? "Ball rolling..."
        : "LEFT/RIGHT = Aim  |  Hold SPACE = Power  |  Release = Bowl  |  ESC = Leave";
    int pw = Program.MeasureTextUI(prompt, 18);
    Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 36, 18, Color.LightGray);
}
    }
}
class ClawMachine
{
    public bool IsOpen = false;
    public int MachineIndex = -1;

    public const int CabLeft = 440;
    public const int CabTop = 130;
    public const int CabWidth = 400;
    public const int CabHeight = 380;

    public float ClawX;            // claw horizontal position
    public float ClawY;            // claw vertical position
    public bool MovingRight = true;
    public bool Dropping = false;
    public bool Rising = false;
    public bool Grabbed = false;
    public int GrabbedPrize = -1;
    public float ClawSpeed = 160f;
    public Vector2[] PrizePos = new Vector2[10];
    public Color[] PrizeColor = new Color[10];
    public string[] PrizeName = new string[10];
    public bool[] PrizeTaken = new bool[10];

    public int Plays = 0;
    public string Message = "";
    public float MessageTimer = 0f;
    public bool ReturningHome = false;

    static readonly (string name, Color col)[] PrizePool =
    {
        ("Teddy",   Color.Brown),
        ("Bunny",   new Color((byte)240,(byte)180,(byte)200,(byte)255)),
        ("Robot",   Color.SkyBlue),
        ("Star",    Color.Gold),
        ("Frog",    Color.Green),
        ("Kitty",   new Color((byte)255,(byte)160,(byte)90,(byte)255)),
        ("Panda",   Color.White),
        ("Dino",    new Color((byte)120,(byte)200,(byte)120,(byte)255)),
    };

    public void Open(int idx)
    {
        IsOpen = true;
        MachineIndex = idx;
        Plays = 0;
        Message = "Insert $2 - SPACE to start the claw";
        MessageTimer = 3f;
        ResetClaw();
        LoadDailyPrizes(); 
    }

    public void Close() { IsOpen = false; MachineIndex = -1; }

    void ResetClaw()
    {
        ClawX = CabLeft + CabWidth / 2f;
        ClawY = CabTop + 30;
        MovingRight = true;
        Dropping = false; Rising = false; Grabbed = false;
        GrabbedPrize = -1; ReturningHome = false;
    }

    void LoadDailyPrizes()
    {
        for (int i = 0; i < 10; i++)
        {
            PrizeTaken[i] = Program.dailyPlushTaken[i];
            PrizeName[i]  = Program.dailyPlushStock[i];
            PrizeColor[i] = Program.PlushColor(PrizeName[i]);
            float px = CabLeft + 40 + i * ((CabWidth - 80) / 9f);
            float py = CabTop + CabHeight - 50 + (i % 2 == 0 ? -10 : 8);
            PrizePos[i] = new Vector2(px, py);
        }
    }

       public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        // idle, waiting to start a play
        if (!Dropping && !Rising && !ReturningHome && GrabbedPrize == -1 && !Grabbed)
        {
            // move claw left/right with arrows, drop with space
            if (Raylib.IsKeyDown(KeyboardKey.Left))  ClawX = Math.Max(CabLeft + 30, ClawX - ClawSpeed * dt);
            if (Raylib.IsKeyDown(KeyboardKey.Right)) ClawX = Math.Min(CabLeft + CabWidth - 30, ClawX + ClawSpeed * dt);

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.dailyPlushTaken.All(t => t)) 
                { 
                    Message = "Sold out! New plushies tomorrow."; 
                    MessageTimer = 2.5f; 
                }
                else if (player.Money >= 2)
                {
                    player.Money -= 2;
                    Plays++;
                    Dropping = true;
                }
                else
                {
                    Message = "Need $2 to play!";
                    MessageTimer = 1.5f;
                }
            }
            return;
        }

        // dropping down to grab
        if (Dropping)
        {
            ClawY += 200f * dt;
            float floorY = CabTop + CabHeight - 50;
            if (ClawY >= floorY)
            {
                ClawY = floorY;
                Dropping = false;
                Rising = true;

                // 1. Initialize logic trackers to find the closest item
                int closest = -1; 
                float best = 9999f;

                // 2. Loop through all 10 prize tracking fields
                for (int i = 0; i < 10; i++)
                {
                    if (PrizeTaken[i]) continue;

                    float dx = Math.Abs(PrizePos[i].X - ClawX);
                    if (dx < best) 
                    { 
                        best = dx; 
                        closest = i; 
                    }
                }

                // 3. Evaluate your success criteria safely based on the closest plush
                if (closest != -1 && best < 28f)
                {
                    // Calculate weight values cleanly using the target index
                    int chance = Program.IsSuperRarePlush(PrizeName[closest]) ? 18
                               : Program.IsRarePlush(PrizeName[closest]) ? 30 : 45;

                    // Roll to verify catch
                    if (Raylib.GetRandomValue(0, 100) < chance)
                    {
                        Grabbed = true;
                        GrabbedPrize = closest;
                    }
                }
            }
            return;
        }

        // rising back up
        if (Rising)
        {
            ClawY -= 200f * dt;
            if (Grabbed && GrabbedPrize != -1)
                PrizePos[GrabbedPrize] = new Vector2(ClawX, ClawY + 28);

            if (ClawY <= CabTop + 30)
            {
                ClawY = CabTop + 30;
                Rising = false;
                ReturningHome = true;
            }
            return;
        }

        // carrying prize back to the chute (top-left)
        if (ReturningHome)
        {
            float homeX = CabLeft + 40;
            ClawX -= ClawSpeed * 1.4f * dt;
            if (Grabbed && GrabbedPrize != -1)
                PrizePos[GrabbedPrize] = new Vector2(ClawX, ClawY + 28);

            if (ClawX <= homeX)
            {
                ClawX = homeX;
                if (Grabbed && GrabbedPrize != -1)
                {
                    PrizeTaken[GrabbedPrize] = true;
                    Message = Program.RegisterPlushWin(GrabbedPrize, PrizeName[GrabbedPrize]);   
                    MessageTimer = 3.5f;
                }
                else
                {
                    Message = "Aww, the claw slipped! Try again.";
                    MessageTimer = 2.5f;
                }
                ResetClaw();
            }
        }
    }


    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)30, (byte)10, (byte)40, (byte)255));

        // cabinet glass
        Raylib.DrawRectangle(CabLeft, CabTop, CabWidth, CabHeight, new Color((byte)60, (byte)80, (byte)120, (byte)90));
        Raylib.DrawRectangleLinesEx(new Rectangle(CabLeft, CabTop, CabWidth, CabHeight), 6, new Color((byte)255, (byte)80, (byte)160, (byte)255));

        // prize chute (top-left)
        Raylib.DrawRectangle(CabLeft + 16, CabTop + 16, 50, 50, new Color((byte)20, (byte)20, (byte)30, (byte)255));
        Raylib.DrawRectangleLines(CabLeft + 16, CabTop + 16, 50, 50, Color.Gold);
        Program.DrawTextUI("CHUTE", CabLeft + 18, CabTop + 68, 10, Color.Gold);

        // prizes
        for (int i = 0; i < 10; i++)   
        {
            if (PrizeTaken[i]) continue;
            if (Program.IsSuperRarePlush(PrizeName[i]))
                Raylib.DrawCircleLines((int)PrizePos[i].X, (int)PrizePos[i].Y, 16, Color.Gold);
            else if (Program.IsRarePlush(PrizeName[i]))
                Raylib.DrawCircleLines((int)PrizePos[i].X, (int)PrizePos[i].Y, 16, Color.LightGray);
            DrawPlush(PrizePos[i], PrizeColor[i], PrizeName[i]);
        }
        // claw rail
        Raylib.DrawLine(CabLeft, CabTop + 30, CabLeft + CabWidth, CabTop + 30, Color.LightGray);
        // claw cable
        Raylib.DrawLine((int)ClawX, CabTop + 30, (int)ClawX, (int)ClawY, Color.LightGray);
        // claw head
        Raylib.DrawCircle((int)ClawX, (int)ClawY, 10, Color.DarkGray);
        bool closed = Grabbed || Dropping;
        // claw prongs
        Raylib.DrawLineEx(new Vector2(ClawX, ClawY), new Vector2(ClawX - (closed ? 6 : 14), ClawY + 22), 3, Color.Gray);
        Raylib.DrawLineEx(new Vector2(ClawX, ClawY), new Vector2(ClawX + (closed ? 6 : 14), ClawY + 22), 3, Color.Gray);
        Raylib.DrawLineEx(new Vector2(ClawX, ClawY), new Vector2(ClawX, ClawY + 24), 3, Color.Gray);

        // HUD
        Program.DrawTextUI("CLAW MACHINE", 1280 / 2 - 130, 30, 36, Color.Gold);
        Program.DrawTextUI($"Wallet: ${player.Money}", 60, 100, 22, Color.Gold);
        Program.DrawTextUI($"Plays: {Plays}", 60, 130, 22, Color.White);
        Program.DrawTextUI($"Prizes won: {player.PlushPrizes}", 60, 160, 20, Color.Pink);
        Program.DrawTextUI($"Set: {Program.plushiesOwned.Count}/50   Left today: {Program.dailyPlushTaken.Count(t => !t)}/10", 60, 190, 18, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 24);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, CabTop + CabHeight + 30, 24, Color.Yellow);
        }

        bool idle = !Dropping && !Rising && !ReturningHome && !Grabbed;
        string prompt = idle
            ? "LEFT/RIGHT = Move Claw  |  SPACE = Drop ($2)  |  ESC = Leave"
            : "Claw in motion...";
        int pw = Program.MeasureTextUI(prompt, 18);
        Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 36, 18, Color.LightGray);
    }

    void DrawPlush(Vector2 pos, Color col, string name)
    {
        int x = (int)pos.X, y = (int)pos.Y;
        // body
        Raylib.DrawCircle(x, y, 16, col);
        // head
        Raylib.DrawCircle(x, y - 16, 11, col);
        // ears
        Raylib.DrawCircle(x - 8, y - 24, 5, col);
        Raylib.DrawCircle(x + 8, y - 24, 5, col);
        // eyes
        Raylib.DrawCircle(x - 4, y - 17, 2, Color.Black);
        Raylib.DrawCircle(x + 4, y - 17, 2, Color.Black);
        // label
        int tw = Program.MeasureTextUI(name, 10);
        Program.DrawTextUI(name, x - tw / 2, y + 18, 10, Color.White);
    }
}
class PinballGame
{
    public bool IsOpen = false;
    public bool InputLocked = false;

    public const int TableLeft = 490;
    public const int TableTop = 80;
    public const int TableWidth = 300;
    public const int TableHeight = 560;

    public Vector2 BallPos;
    public Vector2 BallVel;
    public const float BallRadius = 10f;
    public const float Gravity = 520f;

    public bool BallInPlay = false;
    public float LaunchPower = 0f;
    public bool Charging = false;

    public float LeftFlipperAngle = 0f;   // 0 rest, negative = flipped up
    public float RightFlipperAngle = 0f;

    public int Score = 0;
    public int Balls = 3;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;
    public bool LaunchedFromLane = false;

    // bumpers: position + radius
    public (Vector2 pos, float r, int pts)[] Bumpers;

    public void Open(int idx)
    {
        IsOpen = true;
        InputLocked = true;
        Score = 0; Balls = 3; GameOver = false;
        Message = "Hold SPACE to launch!";
        MessageTimer = 2.5f;
        Bumpers = new (Vector2, float, int)[]
        {
            (new Vector2(TableLeft + 90,  TableTop + 150), 24f, 100),
            (new Vector2(TableLeft + 200, TableTop + 150), 24f, 100),
            (new Vector2(TableLeft + 145, TableTop + 250), 28f, 200),
            (new Vector2(TableLeft + 70,  TableTop + 340), 20f, 150),
            (new Vector2(TableLeft + 220, TableTop + 340), 20f, 150),
        };
        ResetBall();
    }

    public void Close() { IsOpen = false; }

    void ResetBall()
    {
        // ball sits in the launch lane on the right
        BallPos = new Vector2(TableLeft + TableWidth - 18, TableTop + TableHeight - 30);
        BallVel = Vector2.Zero;
        BallInPlay = false;
        LaunchPower = 0f;
        Charging = false;
        LaunchedFromLane = false;
    }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space)) InputLocked = false;
            return;
        }

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.TryPayDropzonePlay())
                    Open(0);
                else
                    IsOpen = false; 
            }
            return;
        }

        // flippers
        LeftFlipperAngle  = Raylib.IsKeyDown(KeyboardKey.Left)  ? -0.6f : Math.Min(0f, LeftFlipperAngle + 6f * dt);
        RightFlipperAngle = Raylib.IsKeyDown(KeyboardKey.Right) ?  0.6f : Math.Max(0f, RightFlipperAngle - 6f * dt);

        if (!BallInPlay)
        {
            if (Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Charging = true;
                LaunchPower = Math.Min(1f, LaunchPower + dt * 0.8f);
            }
            else if (Charging)
        {
            Charging = false;
            BallVel = new Vector2(0, -(400f + LaunchPower * 600f));
            BallInPlay = true;
            LaunchedFromLane = true;   // track that it's still in the launch lane
        }
            return;
        }

        // physics
        BallVel.Y += Gravity * dt;
        BallPos += BallVel * dt;

        // top deflector — when the ball reaches the top of the launch lane, push it left into play
if (LaunchedFromLane && BallPos.Y - BallRadius < TableTop + 60 && BallPos.X > TableLeft + TableWidth - 60)
{
    BallVel.X = -280f;          // kick it left into the main area
    BallVel.Y = Math.Abs(BallVel.Y) * 0.3f;  // convert some upward into a gentle downward arc
    LaunchedFromLane = false;
}

        // walls
        if (BallPos.X - BallRadius < TableLeft) { BallPos.X = TableLeft + BallRadius; BallVel.X *= -0.7f; }
        if (BallPos.X + BallRadius > TableLeft + TableWidth) { BallPos.X = TableLeft + TableWidth - BallRadius; BallVel.X *= -0.7f; }
        if (BallPos.Y - BallRadius < TableTop) { BallPos.Y = TableTop + BallRadius; BallVel.Y *= -0.7f; }

        // bumpers — bounce ball away and score
        foreach (var b in Bumpers)
        {
            Vector2 diff = BallPos - b.pos;
            float dist = diff.Length();
            if (dist < BallRadius + b.r && dist > 0.001f)
            {
                Vector2 n = diff / dist;
                BallPos = b.pos + n * (BallRadius + b.r);
                BallVel = n * Math.Max(380f, BallVel.Length()) * 1.05f;
                Score += b.pts;
                Message = $"+{b.pts}!";
                MessageTimer = 0.6f;
            }
        }

        // flippers — simple paddle bounce near the bottom
        float flipperY = TableTop + TableHeight - 70;
        // left flipper zone
        if (BallPos.Y > flipperY && BallPos.X < TableLeft + TableWidth / 2f && BallVel.Y > 0)
        {
            if (LeftFlipperAngle < -0.2f && BallPos.Y < flipperY + 40)
            {
                BallVel.Y = -620f;
                BallVel.X = 220f;
                Score += 10;
            }
        }
        // right flipper zone
        if (BallPos.Y > flipperY && BallPos.X >= TableLeft + TableWidth / 2f && BallVel.Y > 0)
        {
            if (RightFlipperAngle > 0.2f && BallPos.Y < flipperY + 40)
            {
                BallVel.Y = -620f;
                BallVel.X = -220f;
                Score += 10;
            }
        }

        // drained — ball fell past the bottom
        if (BallPos.Y - BallRadius > TableTop + TableHeight)
        {
            Balls--;
            if (Balls <= 0)
            {
                GameOver = true;
                Message = $"Game Over! Score: {Score}";
                MessageTimer = 6f;
                if (Score > 1000) player.Money += 10;
                else if (Score > 500) player.Money += 5;
            }
            else
            {
                Message = $"Ball lost! {Balls} left";
                MessageTimer = 2f;
                ResetBall();
            }
        }
    }

    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)15, (byte)10, (byte)30, (byte)255));

        // table
        Raylib.DrawRectangle(TableLeft, TableTop, TableWidth, TableHeight, new Color((byte)25, (byte)20, (byte)50, (byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(TableLeft - 10, TableTop - 10, TableWidth + 20, TableHeight + 20), 10, new Color((byte)120, (byte)80, (byte)200, (byte)255));

        // launch lane divider
        Raylib.DrawRectangle(TableLeft + TableWidth - 30, TableTop + 100, 6, TableHeight - 100, new Color((byte)80, (byte)60, (byte)130, (byte)255));

        // bumpers
        foreach (var b in Bumpers)
        {
            Raylib.DrawCircle((int)b.pos.X, (int)b.pos.Y, b.r, new Color((byte)255, (byte)120, (byte)40, (byte)255));
            Raylib.DrawCircle((int)b.pos.X, (int)b.pos.Y, b.r - 6, new Color((byte)255, (byte)200, (byte)80, (byte)255));
            int tw = Program.MeasureTextUI(b.pts.ToString(), 12);
            Program.DrawTextUI(b.pts.ToString(), (int)b.pos.X - tw / 2, (int)b.pos.Y - 6, 12, Color.Black);
        }

        // flippers
        float flipperY = TableTop + TableHeight - 70;
        DrawFlipper(TableLeft + 60, (int)flipperY, LeftFlipperAngle, true);
        DrawFlipper(TableLeft + TableWidth - 60, (int)flipperY, RightFlipperAngle, false);

        // ball
        Raylib.DrawCircle((int)BallPos.X, (int)BallPos.Y, BallRadius, Color.White);
        Raylib.DrawCircle((int)BallPos.X - 3, (int)BallPos.Y - 3, 3, Color.LightGray);

        // top deflector ramp (angled wall sending the ball left into play)
        Raylib.DrawTriangle(
            new Vector2(TableLeft + TableWidth, TableTop + 10),
            new Vector2(TableLeft + TableWidth, TableTop + 70),
            new Vector2(TableLeft + TableWidth - 80, TableTop + 10),
            new Color((byte)120, (byte)80, (byte)200, (byte)255));

        // launch power meter
        if (Charging)
        {
            Raylib.DrawRectangle(TableLeft + TableWidth + 20, TableTop + 200, 18, 200, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(TableLeft + TableWidth + 20, TableTop + 400 - (int)(200 * LaunchPower), 18, (int)(200 * LaunchPower), Color.Red);
        }

        // HUD
        Program.DrawTextUI("PINBALL", 1280 / 2 - 90, 25, 36, Color.Gold);
        Program.DrawTextUI($"Score: {Score}", 60, 90, 24, Color.White);
        Program.DrawTextUI($"Balls: {Balls}", 60, 125, 24, Color.White);
        Program.DrawTextUI($"Wallet: ${player.Money}", 60, 160, 22, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 24);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, TableTop + TableHeight + 30, 24, Color.Yellow);
        }

        if (GameOver)
        {
            Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            string over = "GAME OVER";
            int ow = Program.MeasureTextUI(over, 50);
            Program.DrawTextUI(over, 1280 / 2 - ow / 2, 250, 50, Color.Gold);
            string sc = $"Score: {Score}";
            int sw = Program.MeasureTextUI(sc, 30);
            Program.DrawTextUI(sc, 1280 / 2 - sw / 2, 320, 30, Color.White);
            Program.DrawTextUI("SPACE = Play Again", 1280 / 2 - 110, 380, 24, Color.Green);
            Program.DrawTextUI("ESC = Quit", 1280 / 2 - 60, 420, 24, Color.Red);
        }
        else
        {
            string prompt = BallInPlay ? "LEFT/RIGHT = Flippers  |  ESC = Leave"
                : "Hold SPACE = Launch Power  |  LEFT/RIGHT = Flippers  |  ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 34, 18, Color.LightGray);
        }
    }

    void DrawFlipper(int pivotX, int pivotY, float angle, bool left)
    {
        float len = 56f;
        float dir = left ? 1f : -1f;
        Vector2 pivot = new Vector2(pivotX, pivotY);
        Vector2 tip = pivot + new Vector2(MathF.Cos(angle * dir) * len * dir, MathF.Sin(angle) * len + 4);
        Raylib.DrawLineEx(pivot, tip, 10, new Color((byte)255, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(pivotX, pivotY, 6, Color.Gray);
    }
}
class AirHockeyGame
{
    public bool IsOpen = false;
    public bool InputLocked = false;

    public const int TableLeft = 360;
    public const int TableTop = 120;
    public const int TableWidth = 560;
    public const int TableHeight = 440;

    public Vector2 Puck, PuckVel;
    public Vector2 PlayerMallet;   // bottom half, mouse-controlled
    public Vector2 AiMallet;       // top half, AI-controlled
    public const float MalletR = 28f;
    public const float PuckR = 16f;

    public int PlayerScore = 0;
    public int AiScore = 0;
    public const int WinScore = 7;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;

    int GoalWidth => 160;

    public void Open(int idx)
    {
        IsOpen = true;
        InputLocked = true;
        PlayerScore = 0; AiScore = 0; GameOver = false;
        Message = "First to 7 wins!";
        MessageTimer = 2.5f;
        AiMallet = new Vector2(TableLeft + TableWidth / 2f, TableTop + 70);
        PlayerMallet = new Vector2(TableLeft + TableWidth / 2f, TableTop + TableHeight - 70);
        ResetPuck(true);
    }

    public void Close() { IsOpen = false; }

    void ResetPuck(bool towardPlayer)
    {
        Puck = new Vector2(TableLeft + TableWidth / 2f, TableTop + TableHeight / 2f);
        PuckVel = new Vector2(Raylib.GetRandomValue(-80, 80), towardPlayer ? 220 : -220);
    }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space)) InputLocked = false;
            return;
        }

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.TryPayDropzonePlay())
                    Open(0);
                else
                    IsOpen = false; 
            }
            return;
        }

        // player mallet follows mouse, clamped to bottom half
        Vector2 mouse = Raylib.GetMousePosition();
        Vector2 prevMallet = PlayerMallet;
        PlayerMallet.X = Math.Clamp(mouse.X, TableLeft + MalletR, TableLeft + TableWidth - MalletR);
        PlayerMallet.Y = Math.Clamp(mouse.Y, TableTop + TableHeight / 2f + MalletR, TableTop + TableHeight - MalletR);
        Vector2 malletVel = (PlayerMallet - prevMallet) / dt;

        // AI mallet chases the puck when it's in the top half, else recenters
        Vector2 aiTarget = Puck.Y < TableTop + TableHeight / 2f
            ? new Vector2(Puck.X, Puck.Y)
            : new Vector2(TableLeft + TableWidth / 2f, TableTop + 70);
        Vector2 aiDir = aiTarget - AiMallet;
        if (aiDir.Length() > 1f) AiMallet += Vector2.Normalize(aiDir) * Math.Min(aiDir.Length(), 260f * dt);
        AiMallet.X = Math.Clamp(AiMallet.X, TableLeft + MalletR, TableLeft + TableWidth - MalletR);
        AiMallet.Y = Math.Clamp(AiMallet.Y, TableTop + MalletR, TableTop + TableHeight / 2f - MalletR);

        // puck physics
        Puck += PuckVel * dt;
        PuckVel *= 0.998f;

        // side walls
        if (Puck.X - PuckR < TableLeft) { Puck.X = TableLeft + PuckR; PuckVel.X *= -1; }
        if (Puck.X + PuckR > TableLeft + TableWidth) { Puck.X = TableLeft + TableWidth - PuckR; PuckVel.X *= -1; }

        float goalLeft = TableLeft + TableWidth / 2f - GoalWidth / 2f;
        float goalRight = TableLeft + TableWidth / 2f + GoalWidth / 2f;

        // top wall / AI goal
        if (Puck.Y - PuckR < TableTop)
        {
            if (Puck.X > goalLeft && Puck.X < goalRight)
            {
                PlayerScore++;
                CheckWin(player);
                if (!GameOver) { Message = "You scored!"; MessageTimer = 1.5f; ResetPuck(false); }
                return;
            }
            Puck.Y = TableTop + PuckR; PuckVel.Y *= -1;
        }
        // bottom wall / player goal
        if (Puck.Y + PuckR > TableTop + TableHeight)
        {
            if (Puck.X > goalLeft && Puck.X < goalRight)
            {
                AiScore++;
                CheckWin(player);
                if (!GameOver) { Message = "AI scored!"; MessageTimer = 1.5f; ResetPuck(true); }
                return;
            }
            Puck.Y = TableTop + TableHeight - PuckR; PuckVel.Y *= -1;
        }

        // mallet collisions
        HitPuck(PlayerMallet, malletVel);
        HitPuck(AiMallet, Vector2.Zero);
    }

    void HitPuck(Vector2 mallet, Vector2 malletVel)
    {
        Vector2 diff = Puck - mallet;
        float dist = diff.Length();
        if (dist < MalletR + PuckR && dist > 0.001f)
        {
            Vector2 n = diff / dist;
            Puck = mallet + n * (MalletR + PuckR);
            float speed = Math.Max(280f, PuckVel.Length());
            PuckVel = n * speed + malletVel * 0.4f;
        }
    }

    void CheckWin(Player player)
    {
        if (PlayerScore >= WinScore)
        {
            GameOver = true;
            Message = "YOU WIN! +$8";
            MessageTimer = 5f;
            player.Money += 8;
        }
        else if (AiScore >= WinScore)
        {
            GameOver = true;
            Message = "You lost!";
            MessageTimer = 5f;
        }
    }

    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)10, (byte)20, (byte)35, (byte)255));

        // table
        Raylib.DrawRectangle(TableLeft, TableTop, TableWidth, TableHeight, new Color((byte)220, (byte)235, (byte)250, (byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(TableLeft - 8, TableTop - 8, TableWidth + 16, TableHeight + 16), 8, new Color((byte)60, (byte)90, (byte)140, (byte)255));

        // centre line + circle
        Raylib.DrawLine(TableLeft, TableTop + TableHeight / 2, TableLeft + TableWidth, TableTop + TableHeight / 2, new Color((byte)150, (byte)170, (byte)200, (byte)255));
        Raylib.DrawCircleLines(TableLeft + TableWidth / 2, TableTop + TableHeight / 2, 50, new Color((byte)150, (byte)170, (byte)200, (byte)255));

        // goals
        float goalLeft = TableLeft + TableWidth / 2f - GoalWidth / 2f;
        Raylib.DrawRectangle((int)goalLeft, TableTop - 4, GoalWidth, 6, Color.Red);
        Raylib.DrawRectangle((int)goalLeft, TableTop + TableHeight - 2, GoalWidth, 6, Color.Blue);

        // puck + mallets
        Raylib.DrawCircle((int)Puck.X, (int)Puck.Y, PuckR, Color.Black);
        Raylib.DrawCircle((int)AiMallet.X, (int)AiMallet.Y, MalletR, Color.Red);
        Raylib.DrawCircle((int)AiMallet.X, (int)AiMallet.Y, MalletR - 8, new Color((byte)255, (byte)120, (byte)120, (byte)255));
        Raylib.DrawCircle((int)PlayerMallet.X, (int)PlayerMallet.Y, MalletR, Color.Blue);
        Raylib.DrawCircle((int)PlayerMallet.X, (int)PlayerMallet.Y, MalletR - 8, new Color((byte)120, (byte)160, (byte)255, (byte)255));

        // HUD
        Program.DrawTextUI("AIR HOCKEY", 1280 / 2 - 110, 25, 36, Color.Gold);
        Program.DrawTextUI($"AI: {AiScore}", TableLeft + TableWidth + 30, TableTop + 40, 28, Color.Red);
        Program.DrawTextUI($"You: {PlayerScore}", TableLeft + TableWidth + 30, TableTop + TableHeight - 60, 28, Color.SkyBlue);
        Program.DrawTextUI($"Wallet: ${player.Money}", 60, 90, 22, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 26);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, TableTop + TableHeight + 25, 26, Color.Yellow);
        }

        if (GameOver)
        {
            Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            string res = PlayerScore >= WinScore ? "YOU WIN!" : "YOU LOSE";
            int rw = Program.MeasureTextUI(res, 50);
            Program.DrawTextUI(res, 1280 / 2 - rw / 2, 250, 50, Color.Gold);
            Program.DrawTextUI("SPACE = Play Again", 1280 / 2 - 110, 330, 24, Color.Green);
            Program.DrawTextUI("ESC = Quit", 1280 / 2 - 60, 370, 24, Color.Red);
        }
        else
        {
            string prompt = "MOUSE = Move Mallet  |  ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 34, 18, Color.LightGray);
        }
    }
}
class PianoTilesGame
{
    public bool IsOpen = false;
    public bool InputLocked = false;

    public const int BoardLeft = 490;
    public const int BoardTop = 60;
    public const int Cols = 4;
    public const int ColWidth = 75;
    public const int BoardHeight = 560;
    public const float TileHeight = 110f;

    // each tile: column + Y position (top edge)
    public List<(int col, float y, bool hit)> Tiles = new();
    public float Speed = 220f;
    public float SpawnTimer = 0f;
    public float SpawnInterval = 0.7f;

    public int Score = 0;
    public int Lives = 3;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;
    public int LastTickets = 0;

    readonly KeyboardKey[] keys = { KeyboardKey.D, KeyboardKey.F, KeyboardKey.J, KeyboardKey.K };

    public void Open(int idx)
    {
        IsOpen = true;
        InputLocked = true;
        Score = 0; Lives = 3; GameOver = false;
        Speed = 220f; SpawnInterval = 0.7f; SpawnTimer = 0f;
        Tiles.Clear();
        Message = "Hit D F J K as tiles reach the line!";
        MessageTimer = 3f;
    }

    public void Close() { IsOpen = false; }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space)) InputLocked = false;
            return;
        }

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.TryPayDropzonePlay())
                    Open(0);
                else
                    IsOpen = false; // kick back to world if they can't/won't pay again
            }
            return;
        }

        // spawn tiles
        SpawnTimer += dt;
        if (SpawnTimer >= SpawnInterval)
        {
            SpawnTimer = 0f;
            int col = Raylib.GetRandomValue(0, Cols - 1);
            Tiles.Add((col, BoardTop - TileHeight, false));
        }

        // move tiles down
        float hitLineY = BoardTop + BoardHeight - TileHeight;
        for (int i = 0; i < Tiles.Count; i++)
        {
            var t = Tiles[i];
            t.y += Speed * dt;
            Tiles[i] = t;
        }

        // key presses — check the lowest unhit tile in that column near the hit line
        for (int k = 0; k < Cols; k++)
        {
            if (Raylib.IsKeyPressed(keys[k]))
            {
                int target = -1;
                float lowest = -9999f;
                for (int i = 0; i < Tiles.Count; i++)
                {
                    if (Tiles[i].col == k && !Tiles[i].hit && Tiles[i].y > lowest)
                    {
                        // must be within the hit zone
                        if (Tiles[i].y + TileHeight > hitLineY - 40 && Tiles[i].y < hitLineY + TileHeight)
                        {
                            lowest = Tiles[i].y;
                            target = i;
                        }
                    }
                }
                if (target != -1)
                {
                    var t = Tiles[target];
                    t.hit = true;
                    Tiles[target] = t;
                    Score += 10;
                    // speed up gradually
                    Speed += 3f;
                    if (SpawnInterval > 0.32f) SpawnInterval -= 0.005f;
                }
                else
                {
                    // pressed with no tile — penalty
                    Lives--;
                    Message = "Miss!";
                    MessageTimer = 0.8f;
                    if (Lives <= 0) EndGame(player);
                }
            }
        }

        // tiles that fell past the bottom unhit cost a life
        for (int i = Tiles.Count - 1; i >= 0; i--)
        {
            if (Tiles[i].y > BoardTop + BoardHeight)
            {
                if (!Tiles[i].hit)
                {
                    Lives--;
                    Message = "Missed a tile!";
                    MessageTimer = 0.8f;
                    if (Lives <= 0) { EndGame(player); return; }
                }
                Tiles.RemoveAt(i);
            }
        }
    }

    void EndGame(Player player)
{
    GameOver = true;
    Message = $"Game Over! Score: {Score}";
    MessageTimer = 6f;
    int tickets = Score / 10;          // 1 ticket per 10 points
    player.Tickets += tickets;
    LastTickets = tickets;
}

    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)12, (byte)12, (byte)20, (byte)255));

        // board
        Raylib.DrawRectangle(BoardLeft, BoardTop, Cols * ColWidth, BoardHeight, new Color((byte)30, (byte)30, (byte)45, (byte)255));

        // column dividers
        for (int c = 1; c < Cols; c++)
            Raylib.DrawLine(BoardLeft + c * ColWidth, BoardTop, BoardLeft + c * ColWidth, BoardTop + BoardHeight, new Color((byte)60, (byte)60, (byte)80, (byte)255));

        // hit line
        float hitLineY = BoardTop + BoardHeight - TileHeight;
        Raylib.DrawRectangle(BoardLeft, (int)hitLineY, Cols * ColWidth, 4, new Color((byte)255, (byte)220, (byte)80, (byte)255));

        // tiles
        foreach (var t in Tiles)
        {
            int x = BoardLeft + t.col * ColWidth;
            Color col = t.hit ? new Color((byte)80, (byte)200, (byte)120, (byte)255) : new Color((byte)40, (byte)120, (byte)255, (byte)255);
            Raylib.DrawRectangle(x + 4, (int)t.y, ColWidth - 8, (int)TileHeight - 6, col);
        }

        // key labels at the bottom
        string[] labels = { "D", "F", "J", "K" };
        for (int c = 0; c < Cols; c++)
        {
            int x = BoardLeft + c * ColWidth + ColWidth / 2 - 8;
            Program.DrawTextUI(labels[c], x, BoardTop + BoardHeight + 8, 28, Color.White);
        }

        // HUD
        Program.DrawTextUI("PIANO TILES", 1280 / 2 - 120, 15, 32, Color.Gold);
        Program.DrawTextUI($"Score: {Score}", 60, 90, 24, Color.White);
        Program.DrawTextUI($"Lives: {Lives}", 60, 125, 24, Color.Red);
        Program.DrawTextUI($"Wallet: ${player.Money}", 60, 160, 22, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 24);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, BoardTop + BoardHeight + 50, 24, Color.Yellow);
        }

        if (GameOver)
        {
            Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            string over = "GAME OVER";
            int ow = Program.MeasureTextUI(over, 50);
            Program.DrawTextUI(over, 1280 / 2 - ow / 2, 250, 50, Color.Gold);
            string sc = $"Score: {Score} Tickets: + {LastTickets}";;
            int sw = Program.MeasureTextUI(sc, 30);
            Program.DrawTextUI(sc, 1280 / 2 - sw / 2, 320, 30, Color.White);
            Program.DrawTextUI("SPACE = Play Again", 1280 / 2 - 110, 380, 24, Color.Green);
            Program.DrawTextUI("ESC = Quit", 1280 / 2 - 60, 420, 24, Color.Red);
        }
        else
        {
            string prompt = "D F J K = Hit Tiles  |  ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 34, 18, Color.LightGray);
        }
    }
}
class FlappyBirdGame
{
    public bool IsOpen = false;
    public bool InputLocked = false;

    public const int AreaLeft = 440;
    public const int AreaTop = 80;
    public const int AreaWidth = 400;
    public const int AreaHeight = 540;

    public float BirdY;
    public float BirdVel;
    public const float BirdX = 560;
    public const float Gravity = 1300f;
    public const float FlapStrength = -430f;
    public const float BirdRadius = 14f;

    // pipes: x position, gap centre Y
    public List<(float x, float gapY, bool scored)> Pipes = new();
    public const float PipeWidth = 60f;
    public const float PipeGap = 150f;
    public float PipeSpeed = 170f;
    public float SpawnTimer = 0f;
    public float SpawnInterval = 1.6f;

    public int Score = 0;
    public bool Started = false;
    public bool GameOver = false;
    public int TicketsEarned = 0;
    public string Message = "";
    public float MessageTimer = 0f;

    public void Open(int idx)
    {
        IsOpen = true;
        InputLocked = true;
        Score = 0; TicketsEarned = 0;
        Started = false; GameOver = false;
        BirdY = AreaTop + AreaHeight / 2f;
        BirdVel = 0f;
        Pipes.Clear();
        SpawnTimer = 0f;
        PipeSpeed = 170f;
        SpawnInterval = 1.6f;
        Message = "Press SPACE to flap!";
        MessageTimer = 3f;
    }

    public void Close() { IsOpen = false; }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space)) InputLocked = false;
            return;
        }

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (Program.TryPayDropzonePlay())
                    Open(0);
                else
                    IsOpen = false; // kick back to world if they can't/won't pay again
            }
            return;
        }

        // wait for first flap to start
        if (!Started)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                Started = true;
                BirdVel = FlapStrength;
            }
            return;
        }

        // flap
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            BirdVel = FlapStrength;

        // gravity
        BirdVel += Gravity * dt;
        BirdY += BirdVel * dt;

        // spawn pipes
        SpawnTimer += dt;
        if (SpawnTimer >= SpawnInterval)
        {
            SpawnTimer = 0f;
            float gapY = Raylib.GetRandomValue(AreaTop + 90, AreaTop + AreaHeight - 90);
            Pipes.Add((AreaLeft + AreaWidth, gapY, false));
        }

        // move pipes + score + collision
        for (int i = 0; i < Pipes.Count; i++)
        {
            var p = Pipes[i];
            p.x -= PipeSpeed * dt;

            // score when passing the bird
            if (!p.scored && p.x + PipeWidth < BirdX)
            {
                p.scored = true;
                Score++;
                // every point = 1 ticket, bonus every 5
                TicketsEarned++;
                if (Score % 5 == 0) { TicketsEarned += 2; PipeSpeed += 12f; }
            }
            Pipes[i] = p;

            // collision with this pipe
            if (BirdX + BirdRadius > p.x && BirdX - BirdRadius < p.x + PipeWidth)
            {
                if (BirdY - BirdRadius < p.gapY - PipeGap / 2f || BirdY + BirdRadius > p.gapY + PipeGap / 2f)
                    EndGame(player);
            }
        }

        // remove off-screen pipes
        Pipes.RemoveAll(p => p.x + PipeWidth < AreaLeft);

        // floor / ceiling
        if (BirdY + BirdRadius > AreaTop + AreaHeight || BirdY - BirdRadius < AreaTop)
            EndGame(player);
    }

    void EndGame(Player player)
    {
        if (GameOver) return;
        GameOver = true;
        player.Tickets += TicketsEarned;
        Message = $"Game Over! +{TicketsEarned} tickets";
        MessageTimer = 6f;
    }

    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)110, (byte)200, (byte)235, (byte)255)); // sky

        // play area
        Raylib.DrawRectangle(AreaLeft, AreaTop, AreaWidth, AreaHeight, new Color((byte)135, (byte)206, (byte)235, (byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(AreaLeft, AreaTop, AreaWidth, AreaHeight), 4, new Color((byte)60, (byte)120, (byte)160, (byte)255));

        // pipes
        foreach (var p in Pipes)
        {
            int px = (int)p.x;
            int gapTop = (int)(p.gapY - PipeGap / 2f);
            int gapBot = (int)(p.gapY + PipeGap / 2f);
            // top pipe
            Raylib.DrawRectangle(px, AreaTop, (int)PipeWidth, gapTop - AreaTop, new Color((byte)70, (byte)180, (byte)70, (byte)255));
            Raylib.DrawRectangle(px - 4, gapTop - 20, (int)PipeWidth + 8, 20, new Color((byte)60, (byte)160, (byte)60, (byte)255));
            // bottom pipe
            Raylib.DrawRectangle(px, gapBot, (int)PipeWidth, AreaTop + AreaHeight - gapBot, new Color((byte)70, (byte)180, (byte)70, (byte)255));
            Raylib.DrawRectangle(px - 4, gapBot, (int)PipeWidth + 8, 20, new Color((byte)60, (byte)160, (byte)60, (byte)255));
        }

        // bird
        Raylib.DrawCircle((int)BirdX, (int)BirdY, BirdRadius, Color.Gold);
        Raylib.DrawCircle((int)BirdX + 4, (int)BirdY - 4, 4, Color.White);     // eye white
        Raylib.DrawCircle((int)BirdX + 5, (int)BirdY - 4, 2, Color.Black);     // pupil
        Raylib.DrawTriangle(                                                    // beak
            new Vector2(BirdX + BirdRadius, BirdY - 3),
            new Vector2(BirdX + BirdRadius, BirdY + 3),
            new Vector2(BirdX + BirdRadius + 8, BirdY),
            Color.Orange);

        // HUD
        Program.DrawTextUI("FLAPPY BIRD", 1280 / 2 - 120, 25, 34, Color.DarkBlue);
        Program.DrawTextUI($"Score: {Score}", 60, 90, 24, Color.White);
        Program.DrawTextUI($"Tickets this run: {TicketsEarned}", 60, 125, 22, new Color((byte)255,(byte)180,(byte)40,(byte)255));
        Program.DrawTextUI($"Total tickets: {player.Tickets}", 60, 158, 22, new Color((byte)255,(byte)180,(byte)40,(byte)255));

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 24);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, AreaTop + AreaHeight + 25, 24, Color.Yellow);
        }

        if (GameOver)
        {
            Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            string over = "GAME OVER";
            int ow = Program.MeasureTextUI(over, 50);
            Program.DrawTextUI(over, 1280 / 2 - ow / 2, 250, 50, Color.Gold);
            string sc = $"Score: {Score}   Tickets: +{TicketsEarned}";
            int sw = Program.MeasureTextUI(sc, 28);
            Program.DrawTextUI(sc, 1280 / 2 - sw / 2, 320, 28, Color.White);
            Program.DrawTextUI("SPACE = Play Again", 1280 / 2 - 110, 380, 24, Color.Green);
            Program.DrawTextUI("ESC = Quit", 1280 / 2 - 60, 420, 24, Color.Red);
        }
        else
        {
            string prompt = Started ? "SPACE = Flap  |  ESC = Leave" : "Press SPACE to start  |  ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 34, 18, Color.LightGray);
        }
    }
}

class MiniGolfGame
{
    public bool IsOpen = false;
    public bool InputLocked = false;

    public const int CourseLeft = 240;
    public const int CourseTop = 120;
    public const int CourseWidth = 800;
    public const int CourseHeight = 480;
    public const float BallRadius = 9f;
    public const float Friction = 0.978f;
    public const float MinSpeed = 6f;

    public Vector2 BallPos;
    public Vector2 BallVel;
    public bool BallMoving = false;

    public float AimAngle = 0f;
    public float Power = 0f;
    public bool Charging = false;

    public int CurrentHole = 0;     // 0-8
    public int[] Strokes = new int[9];
    public int TotalStrokes = 0;
    public bool GameOver = false;
    public string Message = "";
    public float MessageTimer = 0f;

    // per-hole layout
    public Vector2 TeePos;
    public Vector2 HolePos;
    public List<Rectangle> Walls = new();
    public int[] Par = { 2, 3, 2, 3, 4, 3, 2, 4, 3 };

    public void Open(int idx)
    {
        IsOpen = true;
        InputLocked = true;
        CurrentHole = 0;
        TotalStrokes = 0;
        for (int i = 0; i < 9; i++) Strokes[i] = 0;
        GameOver = false;
        LoadHole(0);
        Message = "Hole 1 - aim and putt!";
        MessageTimer = 3f;
    }

    public void Close() { IsOpen = false; }

    void LoadHole(int h)
    {
        Walls.Clear();
        int l = CourseLeft, t = CourseTop, w = CourseWidth, hgt = CourseHeight;

        // outer boundary walls (always present)
        Walls.Add(new Rectangle(l, t, w, 16));                       // top
        Walls.Add(new Rectangle(l, t + hgt - 16, w, 16));            // bottom
        Walls.Add(new Rectangle(l, t, 16, hgt));                     // left
        Walls.Add(new Rectangle(l + w - 16, t, 16, hgt));            // right

        // each hole has its own internal obstacles + tee/hole positions
        switch (h)
        {
            case 0: // straight shot
                TeePos = new Vector2(l + 100, t + hgt / 2);
                HolePos = new Vector2(l + w - 120, t + hgt / 2);
                break;
            case 1: // single centre block
                TeePos = new Vector2(l + 100, t + hgt / 2);
                HolePos = new Vector2(l + w - 120, t + hgt / 2);
                Walls.Add(new Rectangle(l + w / 2 - 20, t + hgt / 2 - 80, 40, 160));
                break;
            case 2: // diagonal funnel
                TeePos = new Vector2(l + 100, t + 100);
                HolePos = new Vector2(l + w - 120, t + hgt - 120);
                Walls.Add(new Rectangle(l + w / 2, t, 16, hgt - 140));
                break;
            case 3: // L-shape
                TeePos = new Vector2(l + 100, t + hgt - 100);
                HolePos = new Vector2(l + w - 120, t + 100);
                Walls.Add(new Rectangle(l + 200, t, 16, hgt - 160));
                Walls.Add(new Rectangle(l + 400, t + 160, 16, hgt - 160));
                break;
            case 4: // double gate
                TeePos = new Vector2(l + 90, t + hgt / 2);
                HolePos = new Vector2(l + w - 110, t + hgt / 2);
                Walls.Add(new Rectangle(l + 280, t, 16, 150));
                Walls.Add(new Rectangle(l + 280, t + hgt - 150, 16, 150));
                Walls.Add(new Rectangle(l + 520, t, 16, 150));
                Walls.Add(new Rectangle(l + 520, t + hgt - 150, 16, 150));
                break;
            case 5: // zigzag
                TeePos = new Vector2(l + 90, t + 90);
                HolePos = new Vector2(l + w - 110, t + hgt - 110);
                Walls.Add(new Rectangle(l + 200, t, 16, hgt - 140));
                Walls.Add(new Rectangle(l + 420, t + 140, 16, hgt - 140));
                Walls.Add(new Rectangle(l + 620, t, 16, hgt - 140));
                break;
            case 6: // open with corner pocket
                TeePos = new Vector2(l + 100, t + hgt - 100);
                HolePos = new Vector2(l + w - 120, t + 100);
                Walls.Add(new Rectangle(l + w / 2 - 100, t + hgt / 2 - 8, 200, 16));
                break;
            case 7: // box maze
                TeePos = new Vector2(l + 90, t + hgt / 2);
                HolePos = new Vector2(l + w - 110, t + hgt / 2);
                Walls.Add(new Rectangle(l + 250, t + 90, 16, hgt - 180));
                Walls.Add(new Rectangle(l + 250, t + 90, 200, 16));
                Walls.Add(new Rectangle(l + 450, t + 90, 16, 140));
                Walls.Add(new Rectangle(l + 450, t + hgt - 110, 16, 20));
                break;
            case 8: // final - centre island
                TeePos = new Vector2(l + 100, t + hgt / 2);
                HolePos = new Vector2(l + w - 120, t + hgt / 2);
                Walls.Add(new Rectangle(l + w / 2 - 60, t + hgt / 2 - 60, 120, 120));
                break;
        }

        BallPos = TeePos;
        BallVel = Vector2.Zero;
        BallMoving = false;
        AimAngle = 0f;
        Power = 0f;
        Charging = false;
    }

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

        if (InputLocked)
        {
            if (!Raylib.IsKeyDown(KeyboardKey.Space)) InputLocked = false;
            return;
        }

        if (GameOver)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space)) Open(0);
            return;
        }

        if (BallMoving)
        {
            UpdatePhysics(dt, player);
            return;
        }

        // aim
        if (Raylib.IsKeyDown(KeyboardKey.Left))  AimAngle -= 1.8f * dt;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) AimAngle += 1.8f * dt;

        // power charge
        if (Raylib.IsKeyDown(KeyboardKey.Space))
        {
            Charging = true;
            Power = Math.Min(1f, Power + dt * 0.7f);
        }
        else if (Charging)
        {
            Charging = false;
            BallVel = new Vector2(MathF.Cos(AimAngle), MathF.Sin(AimAngle)) * (250f + Power * 750f);
            BallMoving = true;
            Strokes[CurrentHole]++;
            TotalStrokes++;
            Power = 0f;
        }
    }

    void UpdatePhysics(float dt, Player player)
    {
        BallPos += BallVel * dt;
        BallVel *= Friction;
        if (BallVel.Length() < MinSpeed) BallVel = Vector2.Zero;

        // wall collisions
        foreach (var wall in Walls)
        {
            if (BallPos.X + BallRadius > wall.X && BallPos.X - BallRadius < wall.X + wall.Width &&
                BallPos.Y + BallRadius > wall.Y && BallPos.Y - BallRadius < wall.Y + wall.Height)
            {
                // figure out which side was hit by smallest overlap
                float overlapLeft   = (BallPos.X + BallRadius) - wall.X;
                float overlapRight  = (wall.X + wall.Width) - (BallPos.X - BallRadius);
                float overlapTop    = (BallPos.Y + BallRadius) - wall.Y;
                float overlapBottom = (wall.Y + wall.Height) - (BallPos.Y - BallRadius);
                float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                if (minOverlap == overlapLeft)        { BallPos.X = wall.X - BallRadius; BallVel.X *= -0.7f; }
                else if (minOverlap == overlapRight)  { BallPos.X = wall.X + wall.Width + BallRadius; BallVel.X *= -0.7f; }
                else if (minOverlap == overlapTop)    { BallPos.Y = wall.Y - BallRadius; BallVel.Y *= -0.7f; }
                else                                  { BallPos.Y = wall.Y + wall.Height + BallRadius; BallVel.Y *= -0.7f; }
            }
        }

        // sink in hole — must be slow enough
        if (Vector2.Distance(BallPos, HolePos) < 14f && BallVel.Length() < 220f)
        {
            BallVel = Vector2.Zero;
            BallMoving = false;
            SinkHole(player);
            return;
        }

        // stopped
        if (BallVel == Vector2.Zero)
            BallMoving = false;
    }

    void SinkHole(Player player)
    {
        int strokes = Strokes[CurrentHole];
        int par = Par[CurrentHole];
        string result =
            strokes == 1 ? "HOLE IN ONE!" :
            strokes < par ? "Under par!" :
            strokes == par ? "Par!" :
            "Over par.";

        if (CurrentHole == 8)
        {
            GameOver = true;
            // payout based on total vs total par
            int totalPar = Par.Sum();
            int reward = Math.Max(5, (totalPar - TotalStrokes) * 3 + 15);
            player.Money += reward;
            Message = $"Course complete! {TotalStrokes} strokes. +${reward}";
            MessageTimer = 6f;
        }
        else
        {
            Message = $"{result}  Hole {CurrentHole + 1}: {strokes} strokes";
            MessageTimer = 2.5f;
            CurrentHole++;
            LoadHole(CurrentHole);
        }
    }

    public void Draw(Player player)
    {
        Raylib.ClearBackground(new Color((byte)20, (byte)50, (byte)25, (byte)255));

        // course green
        Raylib.DrawRectangle(CourseLeft, CourseTop, CourseWidth, CourseHeight, new Color((byte)60, (byte)150, (byte)70, (byte)255));
        // subtle mow stripes
        for (int s = 0; s < CourseWidth; s += 40)
            Raylib.DrawRectangle(CourseLeft + s, CourseTop, 20, CourseHeight, new Color((byte)55, (byte)140, (byte)65, (byte)90));

        // walls
        foreach (var wall in Walls)
        {
            Raylib.DrawRectangleRec(wall, new Color((byte)120, (byte)75, (byte)35, (byte)255));
            Raylib.DrawRectangleLinesEx(wall, 2, new Color((byte)90, (byte)55, (byte)25, (byte)255));
        }

        // hole
        Raylib.DrawCircle((int)HolePos.X, (int)HolePos.Y, 13, Color.Black);
        // flag
        Raylib.DrawLine((int)HolePos.X, (int)HolePos.Y, (int)HolePos.X, (int)HolePos.Y - 40, Color.White);
        Raylib.DrawTriangle(
            new Vector2(HolePos.X, HolePos.Y - 40),
            new Vector2(HolePos.X, HolePos.Y - 26),
            new Vector2(HolePos.X + 22, HolePos.Y - 33),
            Color.Red);

        // tee marker
        Raylib.DrawCircleLines((int)TeePos.X, (int)TeePos.Y, 12, new Color((byte)255, (byte)255, (byte)255, (byte)120));

        // ball
        Raylib.DrawCircle((int)BallPos.X, (int)BallPos.Y, BallRadius, Color.White);
        Raylib.DrawCircle((int)BallPos.X - 2, (int)BallPos.Y - 2, 3, new Color((byte)220, (byte)220, (byte)220, (byte)255));

        // aim guide + power
        if (!BallMoving && !GameOver)
        {
            Vector2 dir = new Vector2(MathF.Cos(AimAngle), MathF.Sin(AimAngle));
            for (float d = BallRadius + 6; d < 70 + Power * 120; d += 12)
            {
                Vector2 pt = BallPos + dir * d;
                Raylib.DrawCircle((int)pt.X, (int)pt.Y, 2, new Color((byte)255, (byte)255, (byte)255, (byte)170));
            }
            // arrowhead
            Vector2 tip = BallPos + dir * (70 + Power * 120);
            Raylib.DrawCircle((int)tip.X, (int)tip.Y, 4, Color.Yellow);
        }

        if (Charging)
        {
            Raylib.DrawRectangle(CourseLeft, CourseTop + CourseHeight + 16, 200, 16, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(CourseLeft, CourseTop + CourseHeight + 16, (int)(200 * Power), 16, Color.Red);
            Raylib.DrawRectangleLines(CourseLeft, CourseTop + CourseHeight + 16, 200, 16, Color.White);
            Program.DrawTextUI("POWER", CourseLeft, CourseTop + CourseHeight + 36, 14, Color.White);
        }

        // HUD
        Program.DrawTextUI("MINI GOLF", 1280 / 2 - 100, 20, 36, Color.Gold);
        Program.DrawTextUI($"Hole: {CurrentHole + 1}/9   Par: {Par[CurrentHole]}", CourseLeft + 240, CourseTop - 36, 22, Color.White);
        Program.DrawTextUI($"Strokes this hole: {Strokes[CurrentHole]}", CourseLeft + 520, CourseTop - 36, 20, Color.White);
        Program.DrawTextUI($"Total: {TotalStrokes}", CourseLeft, CourseTop - 36, 22, Color.Gold);

        if (MessageTimer > 0)
        {
            int mw = Program.MeasureTextUI(Message, 24);
            Program.DrawTextUI(Message, 1280 / 2 - mw / 2, CourseTop + CourseHeight + 60, 24, Color.Yellow);
        }

        if (GameOver)
        {
            Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            string done = "COURSE COMPLETE!";
            int dw = Program.MeasureTextUI(done, 44);
            Program.DrawTextUI(done, 1280 / 2 - dw / 2, 250, 44, Color.Gold);
            string sc = $"Total Strokes: {TotalStrokes}  (Par {Par.Sum()})";
            int sw = Program.MeasureTextUI(sc, 28);
            Program.DrawTextUI(sc, 1280 / 2 - sw / 2, 320, 28, Color.White);
            Program.DrawTextUI("SPACE = Play Again", 1280 / 2 - 110, 380, 24, Color.Green);
            Program.DrawTextUI("ESC = Quit", 1280 / 2 - 60, 420, 24, Color.Red);
        }
        else
        {
            string prompt = BallMoving ? "Ball rolling..."
                : "LEFT/RIGHT = Aim  |  Hold SPACE = Power  |  Release = Putt  |  ESC = Leave";
            int pw = Program.MeasureTextUI(prompt, 18);
            Program.DrawTextUI(prompt, 1280 / 2 - pw / 2, 720 - 34, 18, Color.LightGray);
        }
    }
}
}
