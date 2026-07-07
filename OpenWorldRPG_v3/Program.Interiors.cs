using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
    static void DrawInterior()
{
    Raylib.ClearBackground(new Color(40,40,40,255));
    
    Raylib.BeginMode2D(camera);

    static void DrawSupermarketInterior()
{
    // --- floor tiles ---
    for (int tx = 0; tx < 1400; tx += 70)
        for (int ty = 0; ty < 1000; ty += 70)
        {
            Color tileColor = ((tx / 70 + ty / 70) % 2 == 0)
                ? new Color((byte)230, (byte)235, (byte)230, (byte)255)
                : new Color((byte)215, (byte)222, (byte)215, (byte)255);
            Raylib.DrawRectangle(tx, ty, 70, 70, tileColor);
        }

    // --- entrance barrier with gap for trolleys/baskets ---
    Raylib.DrawRectangle(0,   870, 380, 20, new Color((byte)160,(byte)160,(byte)160,(byte)255));
    Raylib.DrawRectangle(650, 870, 750, 20, new Color((byte)160,(byte)160,(byte)160,(byte)255));
    

    // --- checkout counters ---
    int[] checkoutX = { 50, 250, 450, 650, 850, 1050 };
    for (int i = 0; i < checkoutX.Length; i++)
    {
        int cx = checkoutX[i];
        Raylib.DrawRectangle(cx, 30, 140, 45, new Color((byte)60,(byte)100,(byte)60,(byte)255));
        Raylib.DrawRectangle(cx, 30, 140, 8, new Color((byte)80,(byte)130,(byte)80,(byte)255));
        // conveyor belt
        Raylib.DrawRectangle(cx + 8, 42, 80, 22, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        for (int line = cx + 12; line < cx + 88; line += 12)
            Raylib.DrawRectangle(line, 42, 2, 22, new Color((byte)60,(byte)60,(byte)60,(byte)255));
        // register screen
        Raylib.DrawRectangle(cx + 98, 20, 32, 28, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        Raylib.DrawRectangle(cx + 100, 22, 28, 20, new Color((byte)0,(byte)180,(byte)220,(byte)255));
        Program.DrawTextUI($"#{i+1}", cx + 56, 50, 14, Color.White);
    }
    Program.DrawTextUI("CHECKOUTS", 540, 10, 22, Color.DarkGreen);

    // --- aisle shelves with colored products ---
    Color[][] aisleColors = {
        new Color[] { new Color((byte)220,(byte)50,(byte)50,(byte)255),   new Color((byte)255,(byte)160,(byte)0,(byte)255) },   // red/orange - canned goods
        new Color[] { new Color((byte)200,(byte)200,(byte)50,(byte)255),  new Color((byte)80,(byte)150,(byte)230,(byte)255) },  // yellow/blue - cereal/drinks
        new Color[] { new Color((byte)180,(byte)100,(byte)50,(byte)255),  new Color((byte)230,(byte)230,(byte)230,(byte)255) }, // brown/white - bread/dairy
        new Color[] { new Color((byte)50,(byte)180,(byte)50,(byte)255),   new Color((byte)220,(byte)180,(byte)220,(byte)255) }, // green/purple - snacks
    };
    string[] aisleLabels = { "CANNED GOODS", "CEREAL & DRINKS", "BREAD & DAIRY", "SNACKS" };

    int[] aisleStartX = { 50, 340, 630 };
    int[] shelfY = { 150, 205, 290, 345, 430, 485, 570, 625 };

    for (int col = 0; col < 3; col++)
    {
        int ax = aisleStartX[col];
        for (int row = 0; row < 8; row++)
        {
            int sy = shelfY[row];
            Color shelfProduct = aisleColors[(row / 2) % aisleColors.Length][row % 2];
            // shelf backing
            Raylib.DrawRectangle(ax, sy, 220, 35, new Color((byte)150,(byte)120,(byte)80,(byte)255));
            // products
            for (int p = 0; p < 10; p++)
            {
                int px = ax + 4 + p * 21;
                byte shade = (byte)(150 + (p * 7) % 60);
                Raylib.DrawRectangle(px, sy + 4, 16, 26, new Color(
                    (byte)Math.Min(255, shelfProduct.R + (p % 3) * 20),
                    (byte)Math.Min(255, shelfProduct.G + (p % 2) * 15),
                    (byte)Math.Min(255, shelfProduct.B + (p % 4) * 10),
                    (byte)255));
                Raylib.DrawRectangle(px, sy + 4, 16, 5, new Color((byte)255,(byte)255,(byte)255,(byte)80));
            }
            // shelf label strip
            Raylib.DrawRectangle(ax, sy + 30, 220, 5, new Color((byte)255,(byte)255,(byte)255,(byte)120));
        }
        // aisle number sign
        Raylib.DrawRectangle(ax + 70, 118, 80, 25, new Color((byte)0,(byte)80,(byte)0,(byte)255));
        Program.DrawTextUI($"AISLE {col+1}", ax + 74, 122, 16, Color.White);
    }

    // --- meat fridges (right wall) ---
    int[] fridgeY = { 150, 280, 410, 540, 670 };
    string[] meatTypes = { "BEEF", "CHICKEN", "LAMB", "PORK", "FISH" };
    Color[] meatColors = {
        new Color((byte)180,(byte)60,(byte)60,(byte)255),
        new Color((byte)240,(byte)180,(byte)120,(byte)255),
        new Color((byte)200,(byte)100,(byte)80,(byte)255),
        new Color((byte)220,(byte)150,(byte)100,(byte)255),
        new Color((byte)180,(byte)200,(byte)220,(byte)255)
    };
    for (int i = 0; i < fridgeY.Length; i++)
    {
        int fy = fridgeY[i];
        // fridge body
        Raylib.DrawRectangle(1310, fy, 55, 110, new Color((byte)180,(byte)210,(byte)230,(byte)255));
        Raylib.DrawRectangle(1310, fy, 55, 10, new Color((byte)140,(byte)170,(byte)200,(byte)255));
        // glass door
        Raylib.DrawRectangle(1316, fy + 14, 43, 85, new Color((byte)200,(byte)220,(byte)240,(byte)180));
        // meat packages
        for (int p = 0; p < 3; p++)
            Raylib.DrawRectangle(1318, fy + 18 + p * 24, 39, 18, meatColors[i]);
        // frost effect
        Raylib.DrawRectangle(1316, fy + 14, 43, 6, new Color((byte)220,(byte)235,(byte)255,(byte)120));
        // handle
        Raylib.DrawRectangle(1357, fy + 50, 4, 20, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        Program.DrawTextUI(meatTypes[i], 1310, fy + 100, 11, Color.DarkBlue);
    }
    Program.DrawTextUI("MEAT & SEAFOOD", 1295, 120, 14, Color.DarkBlue);

    // --- fruit & veg bins ---
    int[] binX = { 50, 160, 270, 380, 490 };
    Color[] binColors = {
        new Color((byte)255,(byte)80,(byte)80,(byte)255),   // red - apples
        new Color((byte)255,(byte)200,(byte)0,(byte)255),   // yellow - bananas
        new Color((byte)80,(byte)180,(byte)80,(byte)255),   // green - vegs
        new Color((byte)255,(byte)140,(byte)0,(byte)255),   // orange - oranges
        new Color((byte)100,(byte)180,(byte)100,(byte)255)  // green - leafy veg
    };
    string[] binLabels = { "APPLES", "BANANAS", "BROCCOLI", "ORANGES", "LETTUCE" };
    for (int i = 0; i < binX.Length; i++)
    {
        int bx = binX[i];
        // wooden bin
        Raylib.DrawRectangle(bx, 750, 90, 90, new Color((byte)140,(byte)90,(byte)40,(byte)255));
        Raylib.DrawRectangle(bx + 4, 754, 82, 82, new Color((byte)160,(byte)110,(byte)50,(byte)255));
        // produce pile (circles to simulate produce)
        for (int p = 0; p < 8; p++)
        {
            int px = bx + 8 + (p % 4) * 18;
            int py = 758 + (p / 4) * 18;
            Raylib.DrawCircle(px + 8, py + 8, 9, binColors[i]);
            Raylib.DrawCircle(px + 4, py + 4, 4,
                new Color((byte)Math.Min(255, binColors[i].R + 40),
                          (byte)Math.Min(255, binColors[i].G + 40),
                          (byte)Math.Min(255, binColors[i].B + 40), (byte)200));
        }
        Program.DrawTextUI(binLabels[i], bx + 2, 844, 12, Color.DarkGreen);
    }
    Program.DrawTextUI("FRUIT & VEG", 160, 726, 20, Color.DarkGreen);

    // --- deli station ---
    // counter
    Raylib.DrawRectangle(650, 750, 300, 45, new Color((byte)220,(byte)200,(byte)170,(byte)255));
    Raylib.DrawRectangle(650, 750, 300, 8, new Color((byte)240,(byte)220,(byte)190,(byte)255));
    Raylib.DrawRectangle(650, 750, 8, 45, new Color((byte)200,(byte)180,(byte)150,(byte)255));
    Raylib.DrawRectangle(942, 750, 8, 45, new Color((byte)200,(byte)180,(byte)150,(byte)255));
    // glass display case
    Raylib.DrawRectangle(650, 808, 120, 70, new Color((byte)200,(byte)220,(byte)240,(byte)180));
    Raylib.DrawRectangle(650, 808, 120, 8, new Color((byte)160,(byte)180,(byte)200,(byte)255));
    // deli items in case
    Raylib.DrawRectangle(658, 820, 30, 20, new Color((byte)200,(byte)100,(byte)80,(byte)255));  // salami
    Raylib.DrawRectangle(694, 820, 30, 20, new Color((byte)240,(byte)180,(byte)120,(byte)255)); // chicken
    Raylib.DrawRectangle(730, 820, 30, 20, new Color((byte)180,(byte)60,(byte)60,(byte)255));   // ham
    Raylib.DrawRectangle(658, 848, 100, 22, new Color((byte)200,(byte)220,(byte)240,(byte)100));// glass reflection
    // meat slicer
    Raylib.DrawRectangle(790, 808, 70, 70, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    Raylib.DrawCircle(810, 828, 16, new Color((byte)120,(byte)120,(byte)130,(byte)255));
    Raylib.DrawCircle(810, 828, 10, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    Raylib.DrawRectangle(820, 840, 30, 8, new Color((byte)140,(byte)140,(byte)150,(byte)255));
    // heated display
    Raylib.DrawRectangle(878, 808, 70, 70, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    Raylib.DrawRectangle(882, 814, 62, 58, new Color((byte)40,(byte)20,(byte)10,(byte)255));
    for (int r = 0; r < 3; r++)
        Raylib.DrawRectangle(886, 818 + r * 14, 54, 10,
            new Color((byte)200,(byte)80,(byte)20,(byte)(180 - r * 30)));
    Program.DrawTextUI("DELI", 762, 726, 20, Color.DarkGray);
    Program.DrawTextUI("E = Order", 688, 758, 14, Color.DarkGray);

    // --- trolleys near entrance ---
    DrawTrolley(50,  930, trolleyPickedUp && !player.HasTrolley);
    DrawTrolley(130, 930, false);
    DrawTrolley(210, 930, false);

    // --- baskets near entrance ---
    DrawBasket(960, 930, basketPickedUp && !player.HasBasket);
    DrawBasket(985, 930, false);
    DrawBasket(1010, 930, false);
    DrawBasket(1035, 930, false);
    
    // labels
    Program.DrawTextUI("TROLLEYS", 150, 900, 14, Color.DarkGray);
    Program.DrawTextUI("BASKETS", 970, 900, 14, Color.DarkGray);

    // Entrance mat
Raylib.DrawRectangle(450, 920, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(460, 930, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));

}

if (currentBuilding.BuildingName == "DropZone")
{
    Program.DrawTextUI("DROPZONE ARCADE", ScreenWidth / 2 - 200, 30, 40, Color.Gold);

    // ── BOWLING LANES (left side) ──────────────────────────────────────
    // bowling area divider
    Raylib.DrawRectangle(330, 150, 8, 400, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    Program.DrawTextUI("BOWLING", 140, 158, 22, Color.Gold);

    
    if (Vector2.Distance(player.Center, new Vector2(600, 600)) < 100)
    {
        if (!hasDropzoneCard)
            Program.DrawTextUI("[B] Buy DropZone Card ($5)", 480, 700, 20, Color.Gold);
        else
        {
            Program.DrawTextUI($"{dropzoneTierNames[dropzoneTier]} Card  Credit ${dropzoneCredit:0.00}", 460, 690, 18, Color.Gold);
            Program.DrawTextUI($"[L] Load $20   Play ${DropzonePlayCost():0.00}", 460, 714, 16, Color.White);
        }
    }

    // bowling lane 1
    Raylib.DrawRectangle(210, 240, 80, 130, new Color((byte)200, (byte)160, (byte)90, (byte)255));
    Raylib.DrawRectangleLines(210, 240, 80, 130, new Color((byte)120, (byte)70, (byte)20, (byte)255));
    Raylib.DrawRectangle(210, 240, 80, 14, new Color((byte)90, (byte)55, (byte)25, (byte)255)); // foul line
    for (int p = 0; p < 3; p++)                                                                  // pins
        Raylib.DrawCircle(236 + p * 14, 268, 4, Color.White);
    Raylib.DrawCircle(250, 350, 7, new Color((byte)30, (byte)30, (byte)160, (byte)255));         // ball
    Program.DrawTextUI("LANE 1", 222, 376, 14, Color.White);

    // bowling lane 2
    Raylib.DrawRectangle(210, 390, 80, 130, new Color((byte)200, (byte)160, (byte)90, (byte)255));
    Raylib.DrawRectangleLines(210, 390, 80, 130, new Color((byte)120, (byte)70, (byte)20, (byte)255));
    Raylib.DrawRectangle(210, 390, 80, 14, new Color((byte)90, (byte)55, (byte)25, (byte)255));
    for (int p = 0; p < 3; p++)
        Raylib.DrawCircle(236 + p * 14, 418, 4, Color.White);
    Raylib.DrawCircle(250, 500, 7, new Color((byte)30, (byte)30, (byte)160, (byte)255));
    Program.DrawTextUI("LANE 2", 222, 526, 14, Color.White);

    // ── CLAW MACHINES (middle) ─────────────────────────────────────────
    Raylib.DrawRectangle(440, 150, 8, 400, new Color((byte)60, (byte)60, (byte)60, (byte)255)); // divider
    Program.DrawTextUI("CLAW MACHINES", 540, 158, 20, Color.Gold);

    // claw machine 1
    Raylib.DrawRectangle(570, 200, 60, 100, new Color((byte)255, (byte)80, (byte)160, (byte)255));
    Raylib.DrawRectangle(576, 210, 48, 55, new Color((byte)60, (byte)80, (byte)120, (byte)160)); // glass
    Raylib.DrawCircle(588, 250, 5, Color.Brown);                                                 // plush
    Raylib.DrawCircle(602, 252, 5, Color.SkyBlue);
    Raylib.DrawCircle(615, 250, 5, Color.Gold);
    Raylib.DrawLine(600, 210, 600, 225, Color.LightGray);                                        // claw cable
    Raylib.DrawCircle(600, 225, 4, Color.DarkGray);                                              // claw
    Program.DrawTextUI("CLAW", 583, 304, 12, Color.White);

    // claw machine 2
    Raylib.DrawRectangle(670, 200, 60, 100, new Color((byte)255, (byte)80, (byte)160, (byte)255));
    Raylib.DrawRectangle(676, 210, 48, 55, new Color((byte)60, (byte)80, (byte)120, (byte)160));
    Raylib.DrawCircle(688, 250, 5, Color.Green);
    Raylib.DrawCircle(702, 252, 5, new Color((byte)240, (byte)180, (byte)200, (byte)255));
    Raylib.DrawCircle(715, 250, 5, Color.White);
    Raylib.DrawLine(700, 210, 700, 225, Color.LightGray);
    Raylib.DrawCircle(700, 225, 4, Color.DarkGray);
    Program.DrawTextUI("CLAW", 683, 304, 12, Color.White);

    // pool table 1
    Raylib.DrawRectangle(150, 802, 180, 100, new Color((byte)0, (byte)100, (byte)40, (byte)255));
    Raylib.DrawRectangleLines(150, 802, 180, 100, new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(148, 800, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(148, 900, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(148, 800, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(324, 800, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawCircle(240, 852, 6, Color.White);
    Program.DrawTextUI("POOL TABLE 1", 162, 858, 14, Color.White);

    // ── FLAPPY ──
    { int mx=(int)MPos("flappy").X, my=(int)MPos("flappy").Y;
      Raylib.DrawRectangle(mx-35, my-55, 70, 110, new Color((byte)90,(byte)170,(byte)210,(byte)255));  // was 900,520
      Raylib.DrawRectangle(mx-29, my-45, 58, 70, new Color((byte)135,(byte)206,(byte)235,(byte)255));
      Raylib.DrawCircle(mx, my-15, 6, Color.Gold);
      Raylib.DrawRectangle(mx-15, my-45, 8, 20, Color.Green);
      Raylib.DrawRectangle(mx+15, my, 8, 25, Color.Green);
      Program.DrawTextUI("FLAPPY", mx-29, my+59, 11, Color.White);
      if (Vector2.Distance(player.Center, MPos("flappy")) < 75)
          Program.DrawTextUI($"SPACE = Play (${DropzonePlayCost():0.00})", mx-60, my+77, 14, Color.Gold); }

    // ── PRIZE COUNTER ──────────────────────────────────────────────────
    Raylib.DrawRectangle(760, 510, 130, 70, new Color((byte)180, (byte)60, (byte)140, (byte)255));
    Raylib.DrawRectangle(760, 510, 130, 10, new Color((byte)220, (byte)100, (byte)180, (byte)255));
    Program.DrawTextUI("PRIZES", 795, 535, 18, Color.White);
    Raylib.DrawCircle(780, 560, 6, Color.Gold);          // trophy
    Raylib.DrawCircle(800, 560, 5, Color.Brown);         // plush
    Program.DrawTextUI($"Tickets: {player.Tickets}", 760, 584, 14, new Color((byte)255,(byte)200,(byte)60,(byte)255));
    if (Vector2.Distance(player.Center, new Vector2(825, 545)) < 80)
        Program.DrawTextUI("E = Redeem Prizes", 760, 602, 14, Color.Gold);

    // ── PRIZE REDEMPTION POPUP ─────────────────────────────────────────
    if (prizeCounterOpen)
    {
        Raylib.DrawRectangle(1280 / 2 - 200, 130, 400, 280, new Color((byte)0, (byte)0, (byte)0, (byte)235));
        Raylib.DrawRectangleLines(1280 / 2 - 200, 130, 400, 280, Color.Gold);
        Program.DrawTextUI("PRIZE COUNTER", 1280 / 2 - 110, 145, 26, Color.Gold);
        Program.DrawTextUI($"Your tickets: {player.Tickets}", 1280 / 2 - 180, 185, 20, new Color((byte)255,(byte)200,(byte)60,(byte)255));
        Program.DrawTextUI("1. Small Plush     - 10 tickets", 1280 / 2 - 180, 225, 18, Color.White);
        Program.DrawTextUI("2. Big Plush       - 25 tickets", 1280 / 2 - 180, 255, 18, Color.White);
        Program.DrawTextUI("3. Giant Plush     - 50 tickets", 1280 / 2 - 180, 285, 18, Color.White);
        Program.DrawTextUI("4. Jackpot Trophy  - 100 tickets", 1280 / 2 - 180, 315, 18, Color.White);
        Program.DrawTextUI("E = Close", 1280 / 2 - 50, 360, 18, Color.Gray);
    }

    // ── ARCADE CABINETS (right) ────────────────────────────────────────
    Raylib.DrawRectangle(840, 150, 8, 400, new Color((byte)60, (byte)60, (byte)60, (byte)255)); // divider
    Program.DrawTextUI("ARCADE", 920, 158, 22, Color.Gold);

    // arcade cabinet 1
    Raylib.DrawRectangle(874, 200, 52, 100, new Color((byte)40, (byte)40, (byte)90, (byte)255));
    Raylib.DrawRectangle(880, 210, 40, 30, Color.SkyBlue);                                       // screen
    Raylib.DrawRectangle(884, 250, 10, 10, Color.Red);                                           // buttons
    Raylib.DrawRectangle(902, 250, 10, 10, Color.Yellow);
    Program.DrawTextUI("ARCADE", 876, 304, 11, Color.White);

    // arcade cabinet 2
    Raylib.DrawRectangle(974, 200, 52, 100, new Color((byte)40, (byte)40, (byte)90, (byte)255));
    Raylib.DrawRectangle(980, 210, 40, 30, Color.SkyBlue);
    Raylib.DrawRectangle(984, 250, 10, 10, Color.Red);
    Raylib.DrawRectangle(1002, 250, 10, 10, Color.Yellow);
    Program.DrawTextUI("ARCADE", 976, 304, 11, Color.White);

    // ── FOOD & DRINK COUNTER ───────────────────────────────────────────
    Raylib.DrawRectangle(520, 510, 160, 70, new Color((byte)150, (byte)90, (byte)40, (byte)255));
    Raylib.DrawRectangle(520, 510, 160, 10, new Color((byte)180, (byte)120, (byte)60, (byte)255));
    Program.DrawTextUI("FOOD & DRINKS", 535, 535, 18, Color.White);
    Raylib.DrawCircle(540, 560, 5, Color.Red);    // soda cup
    Raylib.DrawCircle(560, 560, 5, Color.Yellow); // nachos
    Raylib.DrawCircle(640, 560, 5, Color.Orange); // hotdog

    // ── PINBALL ──
    { int mx=(int)MPos("pinball").X, my=(int)MPos("pinball").Y;
      Raylib.DrawRectangle(mx-35, my-45, 70, 110, new Color((byte)120,(byte)80,(byte)200,(byte)255));  // was 1060,200
      Raylib.DrawRectangle(mx-29, my-35, 58, 70, new Color((byte)25,(byte)20,(byte)50,(byte)255));
      Raylib.DrawCircle(mx-15, my-10, 4, Color.Orange);
      Raylib.DrawCircle(mx+5, my+5, 4, Color.Yellow);
      Raylib.DrawCircle(mx-5, my+20, 4, Color.Red);
      Program.DrawTextUI("PINBALL", mx-29, my+69, 11, Color.White);
       if (Vector2.Distance(player.Center, MPosSouth("pinball")) < 75)
          Program.DrawTextUI($"SPACE = Play (${DropzonePlayCost():0.00})", mx-60, my+87, 14, Color.Gold); }

    // ── AIR HOCKEY ──
    { int mx=(int)MPos("airhock").X, my=(int)MPos("airhock").Y;
      Raylib.DrawRectangle(mx-35, my-55, 70, 110, new Color((byte)60,(byte)90,(byte)140,(byte)255));   // was 1060,360
      Raylib.DrawRectangle(mx-29, my-45, 58, 70, new Color((byte)220,(byte)235,(byte)250,(byte)255));
      Raylib.DrawLine(mx-29, my-10, mx+29, my-10, new Color((byte)150,(byte)170,(byte)200,(byte)255));
      Raylib.DrawCircle(mx, my-10, 5, Color.Black);
      Program.DrawTextUI("AIR HOCKEY", mx-33, my+59, 10, Color.White);
      if (Vector2.Distance(player.Center, MPos("airhock")) < 75)
          Program.DrawTextUI($"SPACE = Play (${DropzonePlayCost():0.00})", mx-60, my+77, 14, Color.Gold); }

    // ── PIANO TILES ──
    { int mx=(int)MPos("piano").X, my=(int)MPos("piano").Y;
      Raylib.DrawRectangle(mx-35, my-55, 70, 110, new Color((byte)30,(byte)30,(byte)45,(byte)255));    // was 1060,520
      Raylib.DrawRectangle(mx-29, my-45, 58, 70, new Color((byte)12,(byte)12,(byte)20,(byte)255));
      for (int c = 0; c < 4; c++)
          Raylib.DrawRectangle(mx-27 + c*14, my-35 + (c%2)*20, 12, 18, new Color((byte)40,(byte)120,(byte)255,(byte)255));
      Program.DrawTextUI("PIANO", mx-23, my+59, 11, Color.White);
      if (Vector2.Distance(player.Center, MPos("piano")) < 75)
          Program.DrawTextUI($"SPACE = Play (${DropzonePlayCost():0.00})", mx-60, my+77, 14, Color.Gold); }

    // ── INTERACTION PROMPTS ────────────────────────────────────────────
    Vector2[] bowlingLanes = { new Vector2(250, 300)};
    foreach (var lane in bowlingLanes)
        if (Vector2.Distance(player.Center, lane) < 80)
            Program.DrawTextUI("SPACE = Bowl ($3)", (int)lane.X - 60, (int)lane.Y + 95, 16, Color.Gold);
    
    Vector2[] poolTables = { new Vector2(240, 852)};
foreach (var table in poolTables)
    if (Vector2.Distance(player.Center, table) < 100)
        Program.DrawTextUI("SPACE = Pool ($2)", (int)table.X - 60, (int)table.Y + 56, 16, Color.Gold);

    Vector2[] clawMachines = { new Vector2(600, 250), new Vector2(700, 250) };
    foreach (var claw in clawMachines)
        if (Vector2.Distance(player.Center, claw) < 70)
            Program.DrawTextUI("SPACE = Play ($2)", (int)claw.X - 60, (int)claw.Y + 72, 14, Color.Gold);

    Vector2[] arcadeCabinets = { new Vector2(900, 250), new Vector2(1000, 250) };
    foreach (var cab in arcadeCabinets)
        if (Vector2.Distance(player.Center, cab) < 70)
            Program.DrawTextUI("SPACE = Play", (int)cab.X - 44, (int)cab.Y + 72, 14, Color.Gold);

    if (Vector2.Distance(player.Center, new Vector2(600, 550)) < 80)
        Program.DrawTextUI("E = Order Food", 545, 588, 16, Color.Gold);


    // ── C RIDE — claw machine pro ──
        cRideAnimTimer += Raylib.GetFrameTime();
        float cRideBob = MathF.Sin(cRideAnimTimer * 4f) * 2f;      // subtle bob
        float armPump = MathF.Sin(cRideAnimTimer * 8f) * 4f;       // fast arm motion (working the joystick)

        int crx = (int)cRidePos.X;
        int cry = (int)(cRidePos.Y + cRideBob);

        // body
        Raylib.DrawRectangle(crx - 12, cry - 4, 24, 30, new Color((byte)180,(byte)60,(byte)160,(byte)255)); // purple shirt
        // head
        Raylib.DrawCircle(crx, cry - 16, 11, new Color((byte)240,(byte)200,(byte)170,(byte)255));
        // hair
        Raylib.DrawRectangle(crx - 11, cry - 26, 22, 8, new Color((byte)60,(byte)40,(byte)20,(byte)255));
        // arm working the joystick (pumping)
        Raylib.DrawLineEx(new Vector2(crx + 10, cry), new Vector2(crx + 20, cry + 8 + armPump), 4, new Color((byte)240,(byte)200,(byte)170,(byte)255));
        // name tag
        int tagW = Program.MeasureTextUI("C RIDE", 14);
        Raylib.DrawRectangle(crx - tagW/2 - 4, cry - 44, tagW + 8, 18, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("C RIDE", crx - tagW/2, cry - 42, 14, Color.Gold);

        if (cRideMessageActive)
{
    string[] brags = {
        "C RIDE: I've won 200 plushies this month alone!",
        "C RIDE: The claw never beats me. Never.",
        "C RIDE: Watch and learn, rookie.",
        "C RIDE: I could do this with my eyes closed.",
    };
    // pick a line that changes slowly so it's readable
    int bragIndex = ((int)(cRideAnimTimer / 4f)) % brags.Length;
    string msg = brags[bragIndex];

    int mw = Program.MeasureTextUI(msg, 20);
    int bx = 640 - mw/2;
    Raylib.DrawRectangle(bx - 12, 120, mw + 24, 40, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLines(bx - 12, 120, mw + 24, 40, Color.Gold);
    Program.DrawTextUI(msg, bx, 130, 20, Color.White);
}

    // entrance mat
    Raylib.DrawRectangle(540, 825, 200, 120, new Color((byte)60, (byte)60, (byte)70, (byte)255));
    Raylib.DrawRectangle(550, 835, 180, 100, new Color((byte)80, (byte)40, (byte)120, (byte)255));
}

if (currentBuilding.BuildingName == "MiniGolf")
{
    Program.DrawTextUI("MINI GOLF CLUBHOUSE", ScreenWidth / 2 - 230, 40, 40, Color.Gold);

    // the start kiosk / first tee
    Raylib.DrawRectangle(540, 300, 200, 90, new Color((byte)60, (byte)150, (byte)70, (byte)255));
    Raylib.DrawRectangleLines(540, 300, 200, 90, new Color((byte)40, (byte)100, (byte)50, (byte)255));
    // hole + flag on the kiosk
    Raylib.DrawCircle(640, 345, 9, Color.Black);
    Raylib.DrawLine(640, 345, 640, 315, Color.White);
    Raylib.DrawTriangle(new Vector2(640, 315), new Vector2(640, 327), new Vector2(660, 321), Color.Red);
    Program.DrawTextUI("START COURSE", 565, 396, 16, Color.White);

    // decorative greenery
    for (int i = 0; i < 6; i++)
    {
        Raylib.DrawCircle(200 + i * 180, 200, 14, new Color((byte)40, (byte)120, (byte)50, (byte)255));
        Raylib.DrawCircle(200 + i * 180, 540, 14, new Color((byte)40, (byte)120, (byte)50, (byte)255));
    }

    if (Vector2.Distance(player.Center, new Vector2(640, 345)) < 90)
        Program.DrawTextUI("SPACE = Play 9 Holes ($5)", 510, 410, 18, Color.Gold);

    // Entrance mat
        Raylib.DrawRectangle(590, 640, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(600, 650, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}

if (currentBuilding.BuildingName == "BANK")
{
    // =============================================
    // FLOOR — marble tiles
    // =============================================
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)235, (byte)225, (byte)195, (byte)255)
                : new Color((byte)215, (byte)205, (byte)175, (byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
            Raylib.DrawRectangle(tx, ty, 60, 1, new Color((byte)180,(byte)165,(byte)130,(byte)120));
            Raylib.DrawRectangle(tx, ty, 1, 60, new Color((byte)180,(byte)165,(byte)130,(byte)120));
        }

    // =============================================
    // LOBBY AREA
    // =============================================
    // entry mat
    Raylib.DrawRectangle(580, 870, 240, 50,
        new Color((byte)80, (byte)60, (byte)20, (byte)255));
    Raylib.DrawRectangleLines(580, 870, 240, 50,
        new Color((byte)120, (byte)100, (byte)40, (byte)255));
    Program.DrawTextUI("WAIKATO BANK", 598, 886, 16,
        new Color((byte)200, (byte)170, (byte)60, (byte)255));

    // waiting chairs in lobby (left side)
    int[] chairLobbyX = { 40, 110, 180 };
    foreach (int cx in chairLobbyX)
    {
        Raylib.DrawRectangle(cx, 700, 55, 45,
            new Color((byte)100, (byte)75, (byte)30, (byte)255));
        Raylib.DrawRectangle(cx, 700, 55, 8,
            new Color((byte)130, (byte)100, (byte)45, (byte)255));
        Raylib.DrawRectangle(cx, 700, 8, 45,
            new Color((byte)80, (byte)55, (byte)20, (byte)255));
    }
    Program.DrawTextUI("WAITING", 55, 756, 16,
        new Color((byte)120, (byte)95, (byte)40, (byte)255));

    // info stand / brochure rack
    Raylib.DrawRectangle(280, 690, 30, 80,
        new Color((byte)150, (byte)120, (byte)60, (byte)255));
    Raylib.DrawRectangle(270, 668, 50, 24,
        new Color((byte)170, (byte)140, (byte)70, (byte)255));
    for (int i = 275; i < 315; i += 12)
        Raylib.DrawRectangle(i, 672, 8, 18,
            new Color((byte)80, (byte)120, (byte)180, (byte)255));

    // ATM machine (right lobby wall)
    Raylib.DrawRectangle(1310, 600, 70, 120,
        new Color((byte)60, (byte)60, (byte)70, (byte)255));
    Raylib.DrawRectangle(1316, 615, 58, 50,
        new Color((byte)0, (byte)140, (byte)200, (byte)255));
    Raylib.DrawRectangle(1318, 617, 54, 46,
        new Color((byte)0, (byte)160, (byte)220, (byte)255));
    Raylib.DrawRectangle(1326, 672, 42, 10,
        new Color((byte)40, (byte)40, (byte)50, (byte)255));
    Raylib.DrawRectangle(1330, 684, 34, 6,
        new Color((byte)200, (byte)160, (byte)40, (byte)255));
    Program.DrawTextUI("ATM", 1326, 628, 18, Color.White);
    Program.DrawTextUI("24/7", 1326, 648, 14, Color.LightGray);

    // rope barriers guiding queue
    int[] postX = { 420, 540, 660, 780 };
    foreach (int px in postX)
    {
        Raylib.DrawRectangle(px, 480, 8, 60,
            new Color((byte)180, (byte)150, (byte)50, (byte)255));
        Raylib.DrawCircle(px + 4, 480, 6,
            new Color((byte)200, (byte)170, (byte)60, (byte)255));
    }
    // rope connecting posts
    for (int i = 0; i < postX.Length - 1; i++)
        Raylib.DrawLine(postX[i] + 4, 483, postX[i + 1] + 4, 483,
            new Color((byte)160, (byte)80, (byte)20, (byte)255));

    // =============================================
    // TELLER COUNTER (divider wall + booths)
    // =============================================
    // counter wall
    Raylib.DrawRectangle(0, 350, 1100, 20,
        new Color((byte)160, (byte)130, (byte)70, (byte)255));
    Raylib.DrawRectangle(1200, 350, 200, 20,
        new Color((byte)160, (byte)130, (byte)70, (byte)255));
    // counter top highlight
    Raylib.DrawRectangle(0, 350, 1100, 5,
        new Color((byte)200, (byte)170, (byte)100, (byte)255));

    // 3 teller booths
    string[] tellerNames = { "TELLER 1", "TELLER 2", "TELLER 3" };
    int[] boothX = { 150, 500, 850 };
    for (int i = 0; i < 3; i++)
    {
        int bx = boothX[i];

        // booth counter
        Raylib.DrawRectangle(bx, 260, 200, 90,
            new Color((byte)180, (byte)150, (byte)80, (byte)255));
        Raylib.DrawRectangle(bx, 260, 200, 8,
            new Color((byte)210, (byte)180, (byte)110, (byte)255));
        // glass screen
        Raylib.DrawRectangle(bx + 10, 220, 180, 44,
            new Color((byte)180, (byte)210, (byte)230, (byte)160));
        Raylib.DrawRectangleLines(bx + 10, 220, 180, 44,
            new Color((byte)140, (byte)165, (byte)180, (byte)255));
        // small gap at bottom of glass (transaction slot)
        Raylib.DrawRectangle(bx + 70, 258, 60, 4,
            new Color((byte)100, (byte)80, (byte)30, (byte)255));
        // computer on teller side
        Raylib.DrawRectangle(bx + 20, 195, 36, 28,
            new Color((byte)30, (byte)30, (byte)50, (byte)255));
        Raylib.DrawRectangle(bx + 22, 197, 32, 22,
            new Color((byte)0, (byte)140, (byte)200, (byte)255));
        // cash drawer
        Raylib.DrawRectangle(bx + 140, 280, 40, 20,
            new Color((byte)120, (byte)95, (byte)40, (byte)255));
        // booth name sign
        Raylib.DrawRectangle(bx + 50, 370, 100, 22,
            new Color((byte)100, (byte)75, (byte)25, (byte)255));
        Program.DrawTextUI(tellerNames[i], bx + 54, 374, 14,
            new Color((byte)220, (byte)185, (byte)70, (byte)255));
        // approach prompt label
        Program.DrawTextUI("Z=Deposit X=Withdraw", bx + 10, 400, 12,
            new Color((byte)120, (byte)95, (byte)40, (byte)200));
    }

    // teller staff chairs (behind counter)
    foreach (int bx in boothX)
    {
        Raylib.DrawRectangle(bx + 80, 150, 40, 40,
            new Color((byte)80, (byte)60, (byte)20, (byte)255));
        Raylib.DrawRectangle(bx + 80, 150, 40, 7,
            new Color((byte)110, (byte)85, (byte)35, (byte)255));
    }

    // =============================================
    // BACK OFFICE AREA (behind counter)
    // =============================================
    // filing cabinets back wall
    for (int fx = 40; fx < 1040; fx += 60)
    {
        Raylib.DrawRectangle(fx, 25, 50, 90,
            new Color((byte)120, (byte)110, (byte)90, (byte)255));
        Raylib.DrawRectangle(fx, 25, 50, 4,
            new Color((byte)150, (byte)140, (byte)115, (byte)255));
        // drawer lines
        Raylib.DrawRectangle(fx + 2, 40, 46, 2,
            new Color((byte)90, (byte)80, (byte)65, (byte)255));
        Raylib.DrawRectangle(fx + 2, 65, 46, 2,
            new Color((byte)90, (byte)80, (byte)65, (byte)255));
        Raylib.DrawRectangle(fx + 2, 90, 46, 2,
            new Color((byte)90, (byte)80, (byte)65, (byte)255));
        // handles
        Raylib.DrawRectangle(fx + 16, 46, 18, 4,
            new Color((byte)160, (byte)130, (byte)60, (byte)255));
        Raylib.DrawRectangle(fx + 16, 71, 18, 4,
            new Color((byte)160, (byte)130, (byte)60, (byte)255));
        Raylib.DrawRectangle(fx + 16, 96, 18, 4,
            new Color((byte)160, (byte)130, (byte)60, (byte)255));
    }

    // =============================================
    // VAULT DOOR (back wall centre)
    // =============================================
    Raylib.DrawRectangle(575, 18, 250, 50,
        new Color((byte)80, (byte)80, (byte)90, (byte)255));
    Raylib.DrawRectangle(578, 20, 244, 46,
        new Color((byte)100, (byte)100, (byte)115, (byte)255));
    // vault door bolts
    int[] boltY = { 28, 50 };
    int[] boltX = { 585, 625, 665, 705, 745, 785 };
    foreach (int vby in boltY)
        foreach (int vbx in boltX)
            Raylib.DrawCircle(vbx, vby, 4,
                new Color((byte)160, (byte)155, (byte)140, (byte)255));
    // vault wheel
    Raylib.DrawCircle(700, 43, 18,
        new Color((byte)180, (byte)175, (byte)155, (byte)255));
    Raylib.DrawCircle(700, 43, 12,
        new Color((byte)140, (byte)135, (byte)120, (byte)255));
    for (int spoke = 0; spoke < 6; spoke++)
    {
        float angle = spoke * MathF.PI / 3f;
        Raylib.DrawLine(700, 43,
            (int)(700 + MathF.Cos(angle) * 16),
            (int)(43  + MathF.Sin(angle) * 16),
            new Color((byte)160, (byte)155, (byte)140, (byte)255));
    }
    Program.DrawTextUI("VAULT", 672, 60, 12,
        new Color((byte)180, (byte)175, (byte)155, (byte)255));

    // =============================================
    // OFFICE (top left)
    // =============================================
    // walls
    Raylib.DrawRectangle(20,  20, 310,  20,
        new Color((byte)180, (byte)160, (byte)100, (byte)255)); // top
    Raylib.DrawRectangle(330, 20,  20, 310,
        new Color((byte)180, (byte)160, (byte)100, (byte)255)); // right
    Raylib.DrawRectangle(20, 310, 130,  20,
        new Color((byte)180, (byte)160, (byte)100, (byte)255)); // bottom left
    Raylib.DrawRectangle(250,310,  80,  20,
        new Color((byte)180, (byte)160, (byte)100, (byte)255)); // bottom right
    // door frame
    Raylib.DrawRectangle(150, 308, 100, 4,
        new Color((byte)120, (byte)90, (byte)30, (byte)255));

    // carpet inside office
    Raylib.DrawRectangle(22, 22, 306, 286,
        new Color((byte)100, (byte)80, (byte)40, (byte)255));
    // carpet border
    Raylib.DrawRectangleLines(30, 30, 290, 270,
        new Color((byte)130, (byte)100, (byte)50, (byte)255));

    // manager desk
    Raylib.DrawRectangle(50, 60, 240, 70,
        new Color((byte)140, (byte)100, (byte)40, (byte)255));
    Raylib.DrawRectangle(50, 60, 240, 8,
        new Color((byte)170, (byte)130, (byte)60, (byte)255));
    // computer
    Raylib.DrawRectangle(100, 38, 36, 26,
        new Color((byte)25, (byte)25, (byte)45, (byte)255));
    Raylib.DrawRectangle(102, 40, 32, 22,
        new Color((byte)0, (byte)120, (byte)180, (byte)255));
    // papers
    Raylib.DrawRectangle(55, 70, 50, 40,
        new Color((byte)240, (byte)235, (byte)210, (byte)255));
    Raylib.DrawRectangle(60, 76, 40, 4,
        new Color((byte)80, (byte)80, (byte)160, (byte)255));
    Raylib.DrawRectangle(60, 84, 30, 4,
        new Color((byte)80, (byte)80, (byte)160, (byte)255));
    // manager chair
    Raylib.DrawRectangle(130, 148, 48, 48,
        new Color((byte)60, (byte)45, (byte)15, (byte)255));
    Raylib.DrawRectangle(130, 148, 48, 8,
        new Color((byte)80, (byte)60, (byte)20, (byte)255));
    // side chairs (for visitors)
    Raylib.DrawRectangle(55,  200, 38, 38,
        new Color((byte)100, (byte)75, (byte)30, (byte)255));
    Raylib.DrawRectangle(215, 200, 38, 38,
        new Color((byte)100, (byte)75, (byte)30, (byte)255));
    // office sign
    Raylib.DrawRectangle(90, 24, 160, 20,
        new Color((byte)120, (byte)90, (byte)30, (byte)255));
    Program.DrawTextUI("MANAGER", 100, 27, 16,
        new Color((byte)220, (byte)185, (byte)70, (byte)255));



    // =============================================
    // LABELS
    // =============================================
    Program.DrawTextUI("WAIKATO BANK", 550, 430, 20,
        new Color((byte)140, (byte)110, (byte)40, (byte)255));
}

if (currentBuilding.BuildingName == "MARAE")
{
    // =============================================
    // ATEA FLOOR — packed earth / grass
    // =============================================
    // base grass
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)65, (byte)120, (byte)45, (byte)255));
    // grass texture patches
    for (int tx = 0; tx < 1400; tx += 40)
        for (int ty = 500; ty < 1000; ty += 40)
        {
            byte shade = (byte)(55 + (tx + ty) % 30);
            Raylib.DrawRectangle(tx, ty, 40, 40,
                new Color(shade, (byte)(shade + 60), (byte)40, (byte)255));
        }
    // central path — packed earth
    Raylib.DrawRectangle(530, 480, 340, 520,
        new Color((byte)160, (byte)120, (byte)70, (byte)255));
    // path texture
    for (int py = 490; py < 1000; py += 30)
        Raylib.DrawRectangle(530, py, 340, 2,
            new Color((byte)140, (byte)100, (byte)55, (byte)120));

    // =============================================
    // WHARENUI FLOOR — wooden planks
    // =============================================
    for (int tx = 200; tx < 1200; tx += 35)
    {
        byte shade = (byte)(tx / 35 % 2 == 0 ? 150 : 135);
        Raylib.DrawRectangle(tx, 100, 35, 380,
            new Color(shade, (byte)(shade - 30), (byte)40, (byte)255));
        Raylib.DrawRectangle(tx, 100, 1, 380,
            new Color((byte)100, (byte)60, (byte)20, (byte)150));
    }
    // horizontal plank lines
    for (int py = 100; py < 480; py += 60)
        Raylib.DrawRectangle(200, py, 1000, 2,
            new Color((byte)100, (byte)60, (byte)20, (byte)100));

    // =============================================
    // WHARENUI WALLS
    // =============================================
    // front wall with door gaps
    Raylib.DrawRectangle(200, 80, 310, 20,
        new Color((byte)120, (byte)55, (byte)25, (byte)255));
    Raylib.DrawRectangle(640, 80, 120, 20,
        new Color((byte)120, (byte)55, (byte)25, (byte)255));
    Raylib.DrawRectangle(870, 80, 310, 20,
        new Color((byte)120, (byte)55, (byte)25, (byte)255));
    // left wall
    Raylib.DrawRectangle(200, 80, 20, 400,
        new Color((byte)120, (byte)55, (byte)25, (byte)255));
    // right wall
    Raylib.DrawRectangle(1180, 80, 20, 400,
        new Color((byte)120, (byte)55, (byte)25, (byte)255));
    // back wall
    Raylib.DrawRectangle(200, 0, 1000, 82,
        new Color((byte)140, (byte)65, (byte)30, (byte)255));

    // kowhaiwhai (scroll pattern) on back wall
    for (int kx = 220; kx < 1180; kx += 50)
    {
        Raylib.DrawCircle(kx + 12, 40, 14,
            new Color((byte)200, (byte)60, (byte)20, (byte)255));
        Raylib.DrawCircle(kx + 12, 40, 8,
            new Color((byte)240, (byte)160, (byte)40, (byte)255));
        Raylib.DrawCircle(kx + 12, 40, 4,
            new Color((byte)20, (byte)20, (byte)20, (byte)255));
        // connecting scroll line
        Raylib.DrawRectangle(kx + 22, 37, 28, 6,
            new Color((byte)200, (byte)60, (byte)20, (byte)255));
    }

    // tukutuku panels (woven wall panels) on side walls
    // left wall panels
    for (int py = 100; py < 460; py += 60)
    {
        Raylib.DrawRectangle(220, py, 20, 50,
            new Color((byte)180, (byte)140, (byte)80, (byte)255));
        for (int ty = py + 5; ty < py + 50; ty += 8)
            Raylib.DrawRectangle(222, ty, 16, 4,
                new Color((byte)200, (byte)60, (byte)20, (byte)255));
    }
    // right wall panels
    for (int py = 100; py < 460; py += 60)
    {
        Raylib.DrawRectangle(1160, py, 20, 50,
            new Color((byte)180, (byte)140, (byte)80, (byte)255));
        for (int ty = py + 5; ty < py + 50; ty += 8)
            Raylib.DrawRectangle(1162, ty, 16, 4,
                new Color((byte)200, (byte)60, (byte)20, (byte)255));
    }

    // =============================================
    // WHARENUI INTERIOR FEATURES
    // =============================================
    // poutokomanawa (centre post)
    Raylib.DrawRectangle(688, 120, 24, 340,
        new Color((byte)100, (byte)55, (byte)20, (byte)255));
    Raylib.DrawRectangle(690, 122, 20, 336,
        new Color((byte)130, (byte)75, (byte)30, (byte)255));
    // carving on centre post
    for (int cy = 140; cy < 440; cy += 50)
    {
        Raylib.DrawRectangle(694, cy, 12, 8,
            new Color((byte)180, (byte)80, (byte)20, (byte)255));
        Raylib.DrawRectangle(697, cy + 10, 6, 20,
            new Color((byte)160, (byte)60, (byte)15, (byte)255));
    }

    // heke (rafters) across ceiling
    for (int rx = 220; rx < 1180; rx += 120)
    {
        Raylib.DrawRectangle(rx, 82, 100, 12,
            new Color((byte)100, (byte)55, (byte)20, (byte)255));
        // kowhaiwhai on rafter
        for (int kx = rx + 5; kx < rx + 95; kx += 20)
            Raylib.DrawRectangle(kx, 84, 10, 8,
                new Color((byte)200, (byte)60, (byte)20, (byte)180));
    }

    // benches along side walls
    // left bench
    Raylib.DrawRectangle(220, 100, 20, 340,
        new Color((byte)120, (byte)75, (byte)30, (byte)255));
    Raylib.DrawRectangle(220, 100, 20, 5,
        new Color((byte)160, (byte)110, (byte)50, (byte)255));
    // right bench
    Raylib.DrawRectangle(1160, 100, 20, 340,
        new Color((byte)120, (byte)75, (byte)30, (byte)255));
    Raylib.DrawRectangle(1160, 100, 20, 5,
        new Color((byte)160, (byte)110, (byte)50, (byte)255));

    // woven mats on floor
    int[] matX = { 280, 500, 750, 950 };
    int[] matY = { 150, 280, 350 };
    Color[] matColors =
    {
        new Color((byte)180,(byte)140,(byte)60,(byte)220),
        new Color((byte)160,(byte)80, (byte)30,(byte)220),
        new Color((byte)40, (byte)100,(byte)60,(byte)220)
    };
    foreach (int mx in matX)
        for (int m = 0; m < matColors.Length; m++)
        {
            Raylib.DrawRectangle(mx, matY[m], 120, 80, matColors[m]);
            // weave lines
            for (int wx = mx + 8; wx < mx + 120; wx += 16)
                Raylib.DrawRectangle(wx, matY[m], 4, 80,
                    new Color((byte)0,(byte)0,(byte)0,(byte)40));
            for (int wy = matY[m] + 8; wy < matY[m] + 80; wy += 16)
                Raylib.DrawRectangle(mx, wy, 120, 4,
                    new Color((byte)0,(byte)0,(byte)0,(byte)40));
        }

    // =============================================
    // ATEA (COURTYARD) FEATURES
    // =============================================
    // pou (carved posts) lining path
    int[] pouSideX = { 280, 1100 };
    int[] pouSideY = { 530, 640, 750 };
    foreach (int px in pouSideX)
        foreach (int py in pouSideY)
        {
            // post body
            Raylib.DrawRectangle(px, py, 20, 80,
                new Color((byte)100, (byte)55, (byte)20, (byte)255));
            Raylib.DrawRectangle(px + 2, py + 2, 16, 76,
                new Color((byte)130, (byte)75, (byte)30, (byte)255));
            // carved face
            Raylib.DrawRectangle(px + 3, py + 8, 14, 10,
                new Color((byte)160, (byte)80, (byte)25, (byte)255)); // forehead
            Raylib.DrawRectangle(px + 4, py + 20, 5, 5,
                new Color((byte)40, (byte)20, (byte)5, (byte)255));   // left eye
            Raylib.DrawRectangle(px + 11, py + 20, 5, 5,
                new Color((byte)40, (byte)20, (byte)5, (byte)255));   // right eye
            Raylib.DrawRectangle(px + 5, py + 30, 10, 3,
                new Color((byte)40, (byte)20, (byte)5, (byte)255));   // mouth
            // body carving
            for (int cy = py + 40; cy < py + 75; cy += 10)
                Raylib.DrawRectangle(px + 4, cy, 12, 6,
                    new Color((byte)160, (byte)80, (byte)25, (byte)200));
        }

    // waharoa (gateway) at entrance
    Raylib.DrawRectangle(500, 870, 20, 130,
        new Color((byte)100, (byte)55, (byte)20, (byte)255));  // left post
    Raylib.DrawRectangle(880, 870, 20, 130,
        new Color((byte)100, (byte)55, (byte)20, (byte)255));  // right post
    Raylib.DrawRectangle(500, 870, 400, 18,
        new Color((byte)120, (byte)65, (byte)25, (byte)255));  // top beam
    // kowhaiwhai on gateway beam
    for (int gx = 510; gx < 890; gx += 30)
    {
        Raylib.DrawCircle(gx + 8, 879, 7,
            new Color((byte)200, (byte)60, (byte)20, (byte)255));
        Raylib.DrawCircle(gx + 8, 879, 4,
            new Color((byte)240, (byte)160, (byte)40, (byte)255));
    }
    // gateway post carvings
    for (int gy = 890; gy < 990; gy += 20)
    {
        Raylib.DrawRectangle(504, gy, 12, 8,
            new Color((byte)160, (byte)80, (byte)25, (byte)255));
        Raylib.DrawRectangle(884, gy, 12, 8,
            new Color((byte)160, (byte)80, (byte)25, (byte)255));
    }

    // Te Ao Marama (sacred stone) in centre of atea
    Raylib.DrawCircle(700, 680, 28,
        new Color((byte)80, (byte)70, (byte)60, (byte)255));
    Raylib.DrawCircle(700, 680, 22,
        new Color((byte)100, (byte)90, (byte)75, (byte)255));
    Raylib.DrawCircle(700, 680, 10,
        new Color((byte)60, (byte)50, (byte)40, (byte)255));
    Program.DrawTextUI("*", 693, 668, 18,
        new Color((byte)220, (byte)180, (byte)60, (byte)200));

    // flag poles either side of wharenui entrance
    Raylib.DrawRectangle(516, 470, 6, 120,
        new Color((byte)80, (byte)55, (byte)30, (byte)255));
    Raylib.DrawRectangle(878, 470, 6, 120,
        new Color((byte)80, (byte)55, (byte)30, (byte)255));

    // left flag - tino rangatiratanga
    Raylib.DrawRectangle(522, 472, 40, 10,
        new Color((byte)0, (byte)0, (byte)0, (byte)255));       // black top
    Raylib.DrawRectangle(522, 482, 40, 5,
        new Color((byte)255, (byte)255, (byte)255, (byte)255));  // white middle
    Raylib.DrawRectangle(522, 487, 40, 13,
        new Color((byte)200, (byte)0, (byte)0, (byte)255));      // red bottom

    // right flag - tino rangatiratanga
    Raylib.DrawRectangle(884, 472, 40, 10,
        new Color((byte)0, (byte)0, (byte)0, (byte)255));        // black top
    Raylib.DrawRectangle(884, 482, 40, 5,
        new Color((byte)255, (byte)255, (byte)255, (byte)255));  // white middle
    Raylib.DrawRectangle(884, 487, 40, 13,
        new Color((byte)200, (byte)0, (byte)0, (byte)255));      // red bottom

    // =============================================
    // DOOR FRAMES on wharenui
    // =============================================
    // left door
    Raylib.DrawRectangle(530, 60, 110, 40,
        new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(534, 64, 102, 34,
        new Color((byte)110, (byte)65, (byte)25, (byte)255));
    Raylib.DrawRectangle(575, 68, 20, 28,
        new Color((byte)160, (byte)90, (byte)35, (byte)200));
    // right door
    Raylib.DrawRectangle(760, 60, 110, 40,
        new Color((byte)80, (byte)40, (byte)10, (byte)255));
    Raylib.DrawRectangle(764, 64, 102, 34,
        new Color((byte)110, (byte)65, (byte)25, (byte)255));
    Raylib.DrawRectangle(805, 68, 20, 28,
        new Color((byte)160, (byte)90, (byte)35, (byte)200));

    // =============================================
    // LABELS
    // =============================================
    Program.DrawTextUI("WHARENUI", 580, 20, 22,
        new Color((byte)220, (byte)160, (byte)60, (byte)255));
    Program.DrawTextUI("ATEA", 660, 600,  20,
        new Color((byte)180, (byte)140, (byte)60, (byte)200));
}

if (currentBuilding.BuildingName == "LIBRARY")
{
    DrawLibraryInterior();
}
if (currentBuilding.BuildingName == "BARN"){
     DrawBarnInterior();
}
if (currentBuilding.BuildingName == "ZOO") {
DrawZooInterior();
}
if (currentBuilding.BuildingName == "FARMING SHOP")
{
    DrawFarmingShopInterior();
}
if (currentBuilding.BuildingName == "CASTLE") {
    DrawCastleInterior();
}
if (currentBuilding.BuildingName == "SUPERMARKET")
{
    DrawSupermarketInterior();
}

if (currentBuilding.BuildingName != "DBar" && 
    currentBuilding.BuildingName != "GAS STATION" && 
    currentBuilding.BuildingName != "WEAPONS" && 
    currentBuilding.BuildingName != "SUPERMARKET" &&
    currentBuilding.BuildingName != "HOSPITAL" &&
    currentBuilding.BuildingName != "GYM" &&
    currentBuilding.BuildingName != "MY HOUSE" &&
    currentBuilding.BuildingName != "POLICE STATION" &&
    currentBuilding.BuildingName != "MARAE" &&
    currentBuilding.BuildingName != "BANK" &&
    currentBuilding.BuildingName != "STORE" &&
    currentBuilding.BuildingName != "DropZone" &&
    currentBuilding.BuildingName != "MiniGolf" &&
    currentBuilding.BuildingName != "KiwiCuts" &&
    currentBuilding.BuildingName != "HALLENSTEINS" &&
    currentBuilding.BuildingName != "Casino" && 
    currentBuilding.BuildingName != "Airport" &&
    currentBuilding.BuildingName != "AA" &&
    currentBuilding.BuildingName != "MAGIC SHOP" &&
    currentBuilding.BuildingName != "RANGING SHOP" &&
    currentBuilding.BuildingName != "PLAYER HOUSE" &&
    currentBuilding.BuildingName != "LIBRARY" &&
    currentBuilding.BuildingName != "HOBBIES STORE" &&
    currentBuilding.BuildingName != "JOB CENTRE" &&
    currentBuilding.BuildingName != "FARMING SHOP" &&
    currentBuilding.BuildingName != "SCHOOL" &&
    currentBuilding.BuildingName != "CASTLE" &&
    currentBuilding.BuildingName != "MALL" &&
    currentBuilding.BuildingName != "BARN" &&
    currentBuilding.BuildingName != "ZOO" &&
    currentBuilding.BuildingName != "BOAT LICENCE OFFICE" &&
    currentBuilding.BuildingName != "PRISON" )
{
    foreach (Rectangle obj in currentBuilding.InteriorObjects)
        Raylib.DrawRectangleRec(obj, Color.DarkBrown);
}

if (currentBuilding.BuildingName == "KiwiCuts")
{
    // ── Floor tile background ──────────────────────────────────────────────
    for (int tx = 0; tx < 1280; tx += 60)
        for (int ty = 0; ty < 720; ty += 60)
        {
            Color tile = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)235, (byte)230, (byte)220, (byte)255)
                : new Color((byte)220, (byte)215, (byte)205, (byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tile);
        }

    // ── Teal skirting board ────────────────────────────────────────────────
    Raylib.DrawRectangle(0, 0, 1280, 6, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(0, 714, 1280, 6, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(0, 0, 6, 720, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(1274, 0, 6, 720, new Color((byte)20,(byte)140,(byte)120,(byte)255));

    // ── Reception counter (top-left area) ─────────────────────────────────
    Raylib.DrawRectangle(60, 60, 280, 40, new Color((byte)200,(byte)190,(byte)170,(byte)255));
    Raylib.DrawRectangle(60, 60, 280, 6,  new Color((byte)220,(byte)210,(byte)190,(byte)255));
    Program.DrawTextUI("KIWI CUTS", 140, 72, 18, new Color((byte)20,(byte)140,(byte)120,(byte)255));

    // ── Full mirror wall behind chairs ─────────────────────────────────────
    Raylib.DrawRectangle(58, 100, 8, 220, new Color((byte)180,(byte)220,(byte)215,(byte)255));
    Raylib.DrawRectangle(58, 100, 8, 220, new Color((byte)200,(byte)235,(byte)230,(byte)160));

    // ── Barber chair 1 ─────────────────────────────────────────────────────
    Raylib.DrawRectangle(70,  160, 80, 65, new Color((byte)180,(byte)30,(byte)30,(byte)255));   // seat
    Raylib.DrawRectangle(130, 160, 14, 65, new Color((byte)140,(byte)20,(byte)20,(byte)255));   // backrest
    Raylib.DrawRectangle(64,  185, 10, 24, new Color((byte)80,(byte)80,(byte)80,(byte)255));    // left armrest
    Raylib.DrawRectangle(142, 185, 10, 24, new Color((byte)80,(byte)80,(byte)80,(byte)255));    // right armrest
    Raylib.DrawRectangle(78,  225, 50, 10, new Color((byte)60,(byte)60,(byte)60,(byte)255));    // footrest
    // chrome pole
    Raylib.DrawRectangle(100, 235, 8, 30, new Color((byte)180,(byte)180,(byte)180,(byte)255));
    Raylib.DrawRectangle(85,  265, 38, 6, new Color((byte)140,(byte)140,(byte)140,(byte)255));  // base

    // ── Mirror above chair 1 ───────────────────────────────────────────────
    Raylib.DrawRectangle(66,  108, 82, 48, new Color((byte)200,(byte)235,(byte)235,(byte)255));
    Raylib.DrawRectangleLines(66, 108, 82, 48, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(68,  110, 78, 4,  new Color((byte)230,(byte)250,(byte)248,(byte)200)); // highlight

    // ── Barber chair 2 ─────────────────────────────────────────────────────
    Raylib.DrawRectangle(210, 160, 80, 65, new Color((byte)180,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(270, 160, 14, 65, new Color((byte)140,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle(204, 185, 10, 24, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(282, 185, 10, 24, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(218, 225, 50, 10, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(240, 235, 8, 30, new Color((byte)180,(byte)180,(byte)180,(byte)255));
    Raylib.DrawRectangle(225, 265, 38, 6, new Color((byte)140,(byte)140,(byte)140,(byte)255));

    // ── Mirror above chair 2 ───────────────────────────────────────────────
    Raylib.DrawRectangle(206, 108, 82, 48, new Color((byte)200,(byte)235,(byte)235,(byte)255));
    Raylib.DrawRectangleLines(206, 108, 82, 48, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(208, 110, 78, 4,  new Color((byte)230,(byte)250,(byte)248,(byte)200));

    // ── Waiting area (right side) ──────────────────────────────────────────
    // bench
    Raylib.DrawRectangle(700, 120, 200, 50, new Color((byte)160,(byte)130,(byte)90,(byte)255));
    Raylib.DrawRectangle(700, 120, 200, 6,  new Color((byte)190,(byte)160,(byte)115,(byte)255));
    // seat cushions
    Raylib.DrawRectangle(706, 130, 56, 32, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(772, 130, 56, 32, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle(838, 130, 56, 32, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    // magazine table
    Raylib.DrawRectangle(700, 200, 200, 10, new Color((byte)180,(byte)150,(byte)100,(byte)255));
    Program.DrawTextUI("NZ Life", 720, 183, 14, new Color((byte)40,(byte)120,(byte)100,(byte)255));
    Program.DrawTextUI("Rugby Weekly", 800, 183, 14, new Color((byte)40,(byte)120,(byte)100,(byte)255));

    // ── Barber pole (left wall, decorative) ───────────────────────────────
    Raylib.DrawRectangle(28, 120, 10, 80, Color.White);
    Raylib.DrawRectangle(28, 120, 10, 14, Color.Red);
    Raylib.DrawRectangle(28, 148, 10, 14, Color.Red);
    Raylib.DrawRectangle(28, 176, 10, 14, Color.Red);
    Raylib.DrawRectangle(26, 116, 14, 5,  new Color((byte)180,(byte)180,(byte)180,(byte)255));
    Raylib.DrawRectangle(26, 200, 14, 5,  new Color((byte)180,(byte)180,(byte)180,(byte)255));

    // ── Exit door (bottom-centre, at the entry/exit point) ────────────────
    Raylib.DrawRectangle(600, 680, 80, 40, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangleLines(600, 680, 80, 40, new Color((byte)40,(byte)200,(byte)170,(byte)255));
    Program.DrawTextUI("EXIT", 626, 692, 16, Color.White);
}

// HUD text (already screen-space)
Program.DrawTextUI($"Player: {(int)player.Position.X}, {(int)player.Position.Y}", 20, 50, 24, Color.White);
// ... rest of HUD draws ...

if (currentBuilding.BuildingName == "HALLENSTEINS")
{
    // floor
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)18,(byte)18,(byte)18,(byte)255));
    Raylib.DrawRectangle(0, 0, 1400, 6, new Color((byte)180,(byte)140,(byte)20,(byte)255)); // gold top trim

    // front counter
    Raylib.DrawRectangle(60, 120, 500, 30, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(60, 120, 500, 6, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Program.DrawTextUI("HALLENSTEINS", 200, 130, 22, new Color((byte)180,(byte)140,(byte)20,(byte)255));

    // clothing racks
    Color rackCol = new Color((byte)60,(byte)60,(byte)60,(byte)255);
    int[] rackX = { 60, 260, 460 };
    foreach (int rx in rackX)
    {
        Raylib.DrawRectangle(rx, 190, 120, 8, rackCol);   // rail
        Raylib.DrawRectangle(rx + 4, 198, 8, 200, rackCol); // left post
        Raylib.DrawRectangle(rx + 108, 198, 8, 200, rackCol); // right post
        // hanging items
        for (int h = rx + 14; h < rx + 110; h += 18)
        {
            Color itemCol = new Color((byte)Raylib.GetRandomValue(20,200),(byte)Raylib.GetRandomValue(20,200),(byte)Raylib.GetRandomValue(20,200),(byte)255);
            Raylib.DrawRectangle(h, 198, 12, 40, itemCol);
        }
    }

    // fitting rooms
    Raylib.DrawRectangle(700, 150, 8, 400, new Color((byte)40,(byte)40,(byte)40,(byte)255)); // divider
    Raylib.DrawRectangle(720, 180, 80, 100, new Color((byte)25,(byte)25,(byte)25,(byte)255));
    Raylib.DrawRectangleLines(720, 180, 80, 100, rackCol);
    Program.DrawTextUI("FITTING", 732, 220, 14, Color.DarkGray);
    Raylib.DrawRectangle(820, 180, 80, 100, new Color((byte)25,(byte)25,(byte)25,(byte)255));
    Raylib.DrawRectangleLines(820, 180, 80, 100, rackCol);
    Program.DrawTextUI("FITTING", 832, 220, 14, Color.DarkGray);

    // Entrance mat
    Raylib.DrawRectangle(268, 851, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(278, 861, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}

// AA Interior
if (currentBuilding.BuildingName == "AA")
{
    Color aaBlue = new Color((byte)0,(byte)80,(byte)160,(byte)255);
    // floor
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)230,(byte)235,(byte)240,(byte)255));
    for (int gx = 0; gx < 1400; gx += 100) Raylib.DrawRectangle(gx, 0, 2, 1000, new Color((byte)210,(byte)215,(byte)220,(byte)255));
    for (int gy = 0; gy < 1000; gy += 100) Raylib.DrawRectangle(0, gy, 1400, 2, new Color((byte)210,(byte)215,(byte)220,(byte)255));

    // counter
    Raylib.DrawRectangle(400, 100, 400, 40, aaBlue);
    Raylib.DrawRectangle(400, 100, 400, 8, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    Program.DrawTextUI("AA DRIVING SERVICES", 460, 113, 20, Color.White);

    // licence display board
    string[] licClasses = { "D  Lv 1-19", "C  Lv 20-39", "B  Lv 40-59", "A  Lv 60-79", "S  Lv 80-100" };
    bool[] theoryDone   = { hasTheoryD, hasTheoryC, hasTheoryB, hasTheoryA, hasTheoryS };
    bool[] practDone    = { hasPracticalD, hasPracticalC, hasPracticalB, hasPracticalA, hasPracticalS };
    for (int i = 0; i < 5; i++)
    {
        int bx2 = 100 + i * 240;
        Raylib.DrawRectangle(bx2, 200, 200, 90, new Color((byte)10,(byte)40,(byte)80,(byte)255));
        Raylib.DrawRectangleLines(bx2, 200, 200, 90, aaBlue);
        Program.DrawTextUI(licClasses[i], bx2 + 10, 210, 18, Color.White);
        Program.DrawTextUI($"Theory: {(theoryDone[i] ? "Yes" : "No")}", bx2 + 10, 234, 15, theoryDone[i] ? Color.Green : Color.Red);
        Program.DrawTextUI($"Practical: {(practDone[i] ? "Yes" : "No")}", bx2 + 10, 254, 15, practDone[i] ? Color.Green : Color.Red);
        if (i == (int)CurrentLicenceClass - 1)
            Raylib.DrawRectangleLines(bx2, 200, 200, 90, Color.Gold);
    }

    Program.DrawTextUI($"Your Level: {player.DrivingLevel}  |  Class: {CurrentLicenceClass}  |  Wallet: ${player.Money}", 100, 310, 18, Color.DarkGray);

    // Entrance mat
Raylib.DrawRectangle(700, 871, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(710, 881, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));

}

// Airport Interior
if (currentBuilding.BuildingName == "Airport")
{
    // floor — grey tiles
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)200,(byte)205,(byte)210,(byte)255));
    for (int tx = 0; tx < 1400; tx += 80)
        Raylib.DrawLine(tx, 0, tx, 1000, new Color((byte)185,(byte)190,(byte)195,(byte)255));
    for (int ty = 0; ty < 1000; ty += 80)
        Raylib.DrawLine(0, ty, 1400, ty, new Color((byte)185,(byte)190,(byte)195,(byte)255));

    // check-in counter
    Raylib.DrawRectangle(400, 100, 400, 40, new Color((byte)60,(byte)80,(byte)100,(byte)255));
    Raylib.DrawRectangle(400, 100, 400, 8, new Color((byte)80,(byte)110,(byte)140,(byte)255));
    Program.DrawTextUI("CHECK-IN", 555, 112, 20, Color.White);

    // security
    Raylib.DrawRectangle(100, 200, 200, 40, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    Program.DrawTextUI("SECURITY", 130, 212, 18, Color.White);
    // x-ray belt
    Raylib.DrawRectangle(100, 240, 200, 16, new Color((byte)50,(byte)50,(byte)60,(byte)255));
    for (int b = 0; b < 5; b++)
        Raylib.DrawRectangle(106 + b * 38, 244, 30, 8, new Color((byte)30,(byte)30,(byte)40,(byte)255));

    // departure gates
    Raylib.DrawRectangle(750, 180, 180, 50, new Color((byte)40,(byte)60,(byte)80,(byte)255));
    Program.DrawTextUI("GATE A", 800, 196, 20, Color.White);
    Raylib.DrawRectangle(970, 180, 180, 50, new Color((byte)40,(byte)60,(byte)80,(byte)255));
    Program.DrawTextUI("GATE B", 1020, 196, 20, Color.White);

    // seating
    Color seatCol = new Color((byte)60,(byte)80,(byte)140,(byte)255);
    int[] seatX = { 100, 300, 700, 900 };
    foreach (int sx in seatX)
    {
        Raylib.DrawRectangle(sx, 480, 120, 80, seatCol);
        Raylib.DrawRectangle(sx, 476, 120, 8, new Color((byte)40,(byte)55,(byte)110,(byte)255));
        for (int si = 0; si < 3; si++)
            Raylib.DrawRectangle(sx + 8 + si * 38, 488, 28, 60, new Color((byte)70,(byte)95,(byte)160,(byte)255));
    }

    // departures board
    Raylib.DrawRectangle(450, 60, 500, 36, new Color((byte)10,(byte)10,(byte)15,(byte)255));
    Raylib.DrawRectangleLines(450, 60, 500, 36, new Color((byte)40,(byte)100,(byte)160,(byte)255));
    Program.DrawTextUI("DEPARTURES", 490, 66, 14, new Color((byte)40,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI("SAFE ZONE  DESERT  SNOW  BEACH  MOUNTAINS", 455, 82, 11, new Color((byte)180,(byte)240,(byte)255,(byte)255));

    // check-in interaction
    if (Vector2.Distance(player.Center, new Vector2(600, 120)) < 160)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("CHECK-IN DESK", 20, 630, 28, new Color((byte)40,(byte)180,(byte)255,(byte)255));
        Program.DrawTextUI("E = Choose destination", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) airportMenuOpen = !airportMenuOpen;
    }

// Entrance mat
Raylib.DrawRectangle(640, 854, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(650, 864, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}
if (currentBuilding.BuildingName == "Casino")
{
    // floor — dark carpet with pattern
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)40,(byte)5,(byte)40,(byte)255));
    for (int gx = 0; gx < 1400; gx += 120)
        Raylib.DrawRectangle(gx, 0, 2, 1000, new Color((byte)55,(byte)10,(byte)55,(byte)255));
    for (int gy = 0; gy < 1000; gy += 120)
        Raylib.DrawRectangle(0, gy, 1400, 2, new Color((byte)55,(byte)10,(byte)55,(byte)255));

    // chip desk
    Raylib.DrawRectangle(500, 80, 400, 40, new Color((byte)140,(byte)100,(byte)20,(byte)255));
    Raylib.DrawRectangle(500, 80, 400, 8, new Color((byte)180,(byte)140,(byte)30,(byte)255));
    Program.DrawTextUI("CHIP COUNTER", 605, 93, 20, Color.Black);

    // blackjack tables
    foreach (int tx in new[]{ 100, 400 })
    {
        Raylib.DrawEllipse(tx + 100, 360, 110, 65, new Color((byte)0,(byte)100,(byte)20,(byte)255));
        Raylib.DrawEllipseLines(tx + 100, 360, 110, 65, new Color((byte)200,(byte)160,(byte)20,(byte)255));
        Program.DrawTextUI("BLACKJACK", tx + 42, 350, 18, new Color((byte)200,(byte)160,(byte)20,(byte)255));
        Program.DrawTextUI("$10 min", tx + 60, 370, 14, Color.LightGray);
    }

    // roulette table
    Raylib.DrawRectangle(780, 260, 260, 180, new Color((byte)0,(byte)80,(byte)20,(byte)255));
    Raylib.DrawRectangleLines(780, 260, 260, 180, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Raylib.DrawCircle(910, 350, 55, new Color((byte)10,(byte)10,(byte)10,(byte)255));
    Raylib.DrawCircleLines(910, 350, 55, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    // roulette number segments (simplified)
    for (int seg = 0; seg < 12; seg++)
    {
        float angle = seg * (MathF.PI * 2f / 12f);
        Color segCol = seg % 2 == 0 ? new Color((byte)180,(byte)10,(byte)10,(byte)255) : Color.Black;
        Raylib.DrawTriangle(
            new Vector2(910, 350),
            new Vector2(910 + MathF.Cos(angle) * 52, 350 + MathF.Sin(angle) * 52),
            new Vector2(910 + MathF.Cos(angle + MathF.PI/6f) * 52, 350 + MathF.Sin(angle + MathF.PI/6f) * 52),
            segCol);
    }
    Raylib.DrawCircle(910, 350, 12, new Color((byte)220,(byte)200,(byte)160,(byte)255));
    Program.DrawTextUI("ROULETTE", 848, 262, 16, new Color((byte)200,(byte)160,(byte)20,(byte)255));

    // casino pokies (reuse pokie positions)
    foreach (int px2 in new[]{ 100, 220, 340 })
    {
        Raylib.DrawRectangle(px2, 580, 70, 90, new Color((byte)60,(byte)10,(byte)60,(byte)255));
        Raylib.DrawRectangleLines(px2, 580, 70, 90, new Color((byte)180,(byte)20,(byte)180,(byte)255));
        Program.DrawTextUI("POKIE", px2 + 8, 618, 13, new Color((byte)255,(byte)50,(byte)255,(byte)255));
    }

    // card tables (Euchre & 500)
    Raylib.DrawEllipse(1180, 360, 110, 70, new Color((byte)20,(byte)80,(byte)45,(byte)255));
    Raylib.DrawEllipseLines(1180, 360, 110, 70, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("EUCHRE", 1138, 350, 18, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("4-player", 1145, 372, 13, Color.LightGray);

    Raylib.DrawEllipse(1180, 560, 110, 70, new Color((byte)20,(byte)80,(byte)45,(byte)255));
    Raylib.DrawEllipseLines(1180, 560, 110, 70, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("500", 1162, 550, 18, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("4-player", 1145, 572, 13, Color.LightGray);

    // pokie interaction
    Vector2[] casinoPokie = { new Vector2(135,625), new Vector2(255,625), new Vector2(375,625) };
    for (int i = 0; i < casinoPokie.Length; i++)
    {
        if (Vector2.Distance(player.Center, casinoPokie[i]) < 70 && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            activePokieMachine.Open(i);
            activeMinigameType = MinigameType.Pokie;
            ChangeScene(SceneState.Minigame);
        }
    }

     // Entrance mat
        Raylib.DrawRectangle(650, 888, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(660, 898, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}

if (currentBuilding.BuildingName == "MAGIC SHOP")
{
    // floor — dark stone hex tiles
    for (int tx = 0; tx < 1280; tx += 48)
        for (int ty = 0; ty < 1000; ty += 48)
        {
            Color tile = ((tx / 48 + ty / 48) % 2 == 0)
                ? new Color((byte)18,(byte)8,(byte)36,(byte)255)
                : new Color((byte)25,(byte)12,(byte)48,(byte)255);
            Raylib.DrawRectangle(tx, ty, 48, 48, tile);
            Raylib.DrawRectangleLines(tx, ty, 48, 48, new Color((byte)50,(byte)20,(byte)80,(byte)100));
        }

    // back wall — deep purple stone
    Raylib.DrawRectangle(0, 0, 1280, 90, new Color((byte)15,(byte)5,(byte)35,(byte)255));
    for (int p = 0; p < 1280; p += 160)
    {
        Raylib.DrawRectangle(p, 0, 3, 90, new Color((byte)8,(byte)2,(byte)20,(byte)255));
        Raylib.DrawRectangle(p + 80, 0, 3, 90, new Color((byte)8,(byte)2,(byte)20,(byte)255));
    }

    // counter
    Raylib.DrawRectangle(400, 80, 400, 44, new Color((byte)30,(byte)10,(byte)60,(byte)255));
    Raylib.DrawRectangle(400, 80, 400, 6,  new Color((byte)120,(byte)40,(byte)200,(byte)255));
    Raylib.DrawRectangle(400, 120, 400, 4, new Color((byte)60,(byte)20,(byte)100,(byte)255));
    // crystal orb on counter
    Raylib.DrawCircle(600, 95, 10, new Color((byte)80,(byte)20,(byte)140,(byte)180));
    Raylib.DrawCircle(600, 95, 6,  new Color((byte)160,(byte)60,(byte)255,(byte)220));
    Raylib.DrawCircle(600, 95, 3,  Color.White);

    // staff display racks
    Color rackBg  = new Color((byte)20,(byte)6,(byte)44,(byte)255);
    Color rackBdr = new Color((byte)80,(byte)20,(byte)140,(byte)255);
    int[] rackX2  = { 100, 350, 600, 850 };
    Color[] staffCols2 = {
        new Color((byte)120,(byte)90,(byte)60,(byte)255),
        new Color((byte)40,(byte)120,(byte)220,(byte)255),
        new Color((byte)240,(byte)80,(byte)20,(byte)255),
        new Color((byte)80,(byte)0,(byte)120,(byte)255)
    };
    foreach (int rx in rackX2)
    {
        int ri = Array.IndexOf(rackX2, rx);
        Raylib.DrawRectangle(rx, 200, 180, 120, rackBg);
        Raylib.DrawRectangleLines(rx, 200, 180, 120, rackBdr);
        Raylib.DrawRectangle(rx + 10, 218, 160, 4, rackBdr);
        // staff icon
        Raylib.DrawRectangle(rx + 85, 210, 8, 100, staffCols2[ri % 4]);
        Raylib.DrawCircle(rx + 89, 212, 10, staffCols2[ri % 4]);
        Raylib.DrawCircle(rx + 89, 212, 6,  Color.White);
    }

    // essence shelves with glowing orbs
    int[] esX = { 100, 280, 460, 640 };
    for (int e = 0; e < 4; e++)
    {
        Raylib.DrawRectangle(esX[e], 480, 160, 70, rackBg);
        Raylib.DrawRectangleLines(esX[e], 480, 160, 70, rackBdr);
        Raylib.DrawRectangle(esX[e], 480, 160, 4, new Color((byte)120,(byte)40,(byte)200,(byte)255));
        float pulse = 1f + MathF.Sin((float)Raylib.GetTime() * 2f + e) * 0.3f;
        int pr = (int)(8 * pulse);
        Raylib.DrawCircle(esX[e] + 40, 516, pr + 2, new Color((byte)80,(byte)20,(byte)140,(byte)100));
        Raylib.DrawCircle(esX[e] + 40, 516, pr,     new Color((byte)160,(byte)60,(byte)255,(byte)200));
        Raylib.DrawCircle(esX[e] + 40, 516, 4,      Color.White);
        Raylib.DrawCircle(esX[e] + 100, 516, pr + 2, new Color((byte)80,(byte)20,(byte)140,(byte)100));
        Raylib.DrawCircle(esX[e] + 100, 516, pr,     new Color((byte)160,(byte)60,(byte)255,(byte)200));
        Raylib.DrawCircle(esX[e] + 100, 516, 4,      Color.White);
    }

    // candles in corners
    int[] candleX = { 30, 1220, 30, 1220 };
    int[] candleY = { 300, 300, 700, 700 };
    for (int c = 0; c < 4; c++)
    {
        Raylib.DrawRectangle(candleX[c] - 4, candleY[c], 8, 28, new Color((byte)220,(byte)210,(byte)180,(byte)255));
        Raylib.DrawCircle(candleX[c], candleY[c], 6, new Color((byte)255,(byte)180,(byte)40,(byte)80));
        Raylib.DrawCircle(candleX[c], candleY[c], 3, new Color((byte)255,(byte)220,(byte)80,(byte)255));
    }

    // runic circle on floor centre
    Raylib.DrawCircleLines(640, 600, 80, new Color((byte)120,(byte)40,(byte)200,(byte)120));
    Raylib.DrawCircleLines(640, 600, 60, new Color((byte)120,(byte)40,(byte)200,(byte)80));
    for (int r = 0; r < 6; r++)
    {
        float angle = r * MathF.PI / 3f;
        int rx2 = 640 + (int)(70 * MathF.Cos(angle));
        int ry2 = 600 + (int)(70 * MathF.Sin(angle));
        Raylib.DrawCircle(rx2, ry2, 5, new Color((byte)160,(byte)60,(byte)255,(byte)180));
    }

    // entrance mat
    Raylib.DrawRectangle(540, 870, 200, 28, new Color((byte)15,(byte)5,(byte)35,(byte)255));
    Raylib.DrawRectangleLines(540, 870, 200, 28, new Color((byte)120,(byte)40,(byte)200,(byte)255));
    Program.DrawTextUI("MAGIC SHOP", 566, 878, 16, new Color((byte)200,(byte)120,(byte)255,(byte)255));
}

if (currentBuilding.BuildingName == "MALL" && currentMiniShop != "")
    DrawMiniShopInterior(currentMiniShop);
if (currentBuilding.BuildingName == "MALL" && currentMiniShop == "")
    DrawMallConcourse();

if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom != "")
{
    DrawClassroomInterior(currentClassroom);
}
else
{
    // ... existing code (normal interior draw, including school hall desks etc.) ...
}

// DrawInterior() — ADD a dispatch call inside the appropriate branch
if (currentBuilding.BuildingName == "PRISON" && currentPrisonRoom != "")
    DrawPrisonRoomInterior(currentPrisonRoom);
if (currentBuilding.BuildingName == "PRISON" && currentPrisonRoom == "")
    DrawPrisonInterior();

if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom == "") DrawSchoolHallInterior();

if (currentBuilding.BuildingName == "HOBBIES STORE")
{
    DrawHobbiesStoreInterior();
}

if (currentBuilding.BuildingName == "BOAT LICENCE OFFICE")
{
    DrawBoatLicenceInterior();
}

if (currentBuilding.BuildingName == "FamilyHub") 
{
     DrawFamilyHubInterior();
}

if (currentBuilding.BuildingName == "BEST START") 
{
    DrawDaycareInterior();
}

    if (currentBuilding.BuildingName == "DBar")
    {
        // bar counter
        Raylib.DrawRectangle(100, 150, 300, 40, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(100, 150, 300, 8, new Color((byte)120, (byte)70, (byte)20, (byte)255));
        Program.DrawTextUI("BAR", 220, 162, 20, Color.Gold);

        // dartboard (mounted on the wall, left side)
        int dbX = 1250, dbY = 80; // centre of the board
        Raylib.DrawCircle(dbX, dbY, 56, new Color((byte)20, (byte)20, (byte)20, (byte)255));   // backboard/surround
        Raylib.DrawCircle(dbX, dbY, 50, new Color((byte)235, (byte)225, (byte)200, (byte)255)); // board face
        // alternating wedge feel via two rings of colour
        Raylib.DrawCircle(dbX, dbY, 40, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawCircle(dbX, dbY, 32, new Color((byte)200, (byte)190, (byte)160, (byte)255));
        Raylib.DrawCircle(dbX, dbY, 20, new Color((byte)180, (byte)40, (byte)40, (byte)255));   // outer bull ring
        Raylib.DrawCircle(dbX, dbY, 9,  new Color((byte)0, (byte)110, (byte)40, (byte)255));    // bullseye
        Raylib.DrawCircle(dbX, dbY, 4,  Color.Red);
        

        // pool table 1
        Raylib.DrawRectangle(150, 280, 180, 100, new Color((byte)0, (byte)100, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(150, 280, 180, 100, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 278, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 372, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(324, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawCircle(240, 330, 6, Color.White);
        Program.DrawTextUI("POOL TABLE 1", 162, 336, 14, Color.White);

        // pool table 2
        Raylib.DrawRectangle(450, 280, 180, 100, new Color((byte)0, (byte)100, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(450, 280, 180, 100, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 278, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 372, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(624, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawCircle(540, 330, 6, Color.White);
        Program.DrawTextUI("POOL TABLE 2", 462, 336, 14, Color.White);

        // pokies area divider
        Raylib.DrawRectangle(750, 150, 8, 400, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Program.DrawTextUI("POKIES", 820, 158, 22, Color.Gold);

        // pokie machine 1
        Raylib.DrawRectangle(780, 200, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(786, 210, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(796, 220, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(811, 220, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(826, 220, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(800, 272, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Program.DrawTextUI("$", 816, 274, 14, Color.Black);

        // pokie machine 2
        Raylib.DrawRectangle(780, 320, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(786, 330, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(796, 340, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(811, 340, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(826, 340, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(800, 392, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Program.DrawTextUI("$", 816, 394, 14, Color.Black);

        // pokie machine 3
        Raylib.DrawRectangle(880, 200, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(886, 210, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(896, 220, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(911, 220, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(926, 220, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(900, 272, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Program.DrawTextUI("$", 916, 274, 14, Color.Black);

        // pokie machine 4
        Raylib.DrawRectangle(880, 320, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(886, 330, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(896, 340, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(911, 340, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(926, 340, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(900, 392, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Program.DrawTextUI("$", 916, 394, 14, Color.Black);

        // table 1
        Raylib.DrawRectangle(100, 430, 90, 65, new Color((byte)80,(byte)40,(byte)10,(byte)255));
        Raylib.DrawRectangleLines(100, 430, 90, 65, new Color((byte)120,(byte)70,(byte)20,(byte)255));
        Raylib.DrawRectangle(78,  435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));  // left chair
        Raylib.DrawRectangle(192, 435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));  // right chair
        Raylib.DrawRectangle(120, 410, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));  // top chair
        Raylib.DrawRectangle(148, 497, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));  // bottom chair

        // table 2
        Raylib.DrawRectangle(320, 430, 90, 65, new Color((byte)80,(byte)40,(byte)10,(byte)255));
        Raylib.DrawRectangleLines(320, 430, 90, 65, new Color((byte)120,(byte)70,(byte)20,(byte)255));
        Raylib.DrawRectangle(298, 435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(412, 435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(340, 410, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(368, 497, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));

        // table 3
        Raylib.DrawRectangle(540, 430, 90, 65, new Color((byte)80,(byte)40,(byte)10,(byte)255));
        Raylib.DrawRectangleLines(540, 430, 90, 65, new Color((byte)120,(byte)70,(byte)20,(byte)255));
        Raylib.DrawRectangle(518, 435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(632, 435, 20, 22, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(558, 410, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));
        Raylib.DrawRectangle(578, 497, 22, 18, new Color((byte)60,(byte)30,(byte)10,(byte)255));

        // Entrance mat
        Raylib.DrawRectangle(215, 825, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(225, 835, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
            }

    if (currentBuilding.BuildingName == "POLICE STATION")
{
    // --- floor tiles (dark grey/blue tint) ---
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)50,  (byte)55,  (byte)70,  (byte)255)
                : new Color((byte)45,  (byte)48,  (byte)62,  (byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }

    // --- ambient overlay ---
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)20, (byte)20, (byte)60, (byte)80));

    // =============================================
    // RECEPTION DESK (right panel: x 1050–1400)
    // =============================================
    Raylib.DrawRectangle(1050, 0, 350, 1000, new Color((byte)35, (byte)35, (byte)60, (byte)255)); // bg panel
    Raylib.DrawRectangle(1095, 80, 280, 50,  new Color((byte)50, (byte)50, (byte)100, (byte)255)); // desk
    Raylib.DrawRectangle(1095, 80, 280, 8,   new Color((byte)80, (byte)80, (byte)140, (byte)255)); // desk top edge
    Raylib.DrawRectangle(1095, 80, 8,   50,  new Color((byte)30, (byte)30, (byte)80,  (byte)255)); // left side
    Raylib.DrawRectangle(1367, 80, 8,   50,  new Color((byte)30, (byte)30, (byte)80,  (byte)255)); // right side

    // computer on desk
    Raylib.DrawRectangle(1215, 58, 40, 28, new Color((byte)15,  (byte)15,  (byte)40,  (byte)255)); // monitor body
    Raylib.DrawRectangle(1217, 60, 36, 22, new Color((byte)0,   (byte)80,  (byte)180, (byte)255)); // screen
    Raylib.DrawRectangle(1228, 82, 14, 6,  new Color((byte)10,  (byte)10,  (byte)30,  (byte)255)); // monitor stand

    // police badge / shield sign
    Raylib.DrawRectangle(1224, 15, 22, 28, new Color((byte)30,  (byte)30,  (byte)120, (byte)255)); // shield
    Raylib.DrawRectangle(1228, 19, 14, 20, new Color((byte)220, (byte)180, (byte)0,   (byte)255)); // gold star body
    Program.DrawTextUI("★", 1226, 18, 20, new Color((byte)220, (byte)180, (byte)0, (byte)255));

    Program.DrawTextUI("RECEPTION", 1130, 92, 20, new Color((byte)160, (byte)180, (byte)220, (byte)255));
    Program.DrawTextUI("POLICE STATION", 1080, 500, 18, new Color((byte)100, (byte)120, (byte)180, (byte)255));

    // =============================================
    // ROOM DIVIDERS
    // =============================================
    int[] divX = { 350, 700 };
    foreach (int dx in divX)
    {
        Raylib.DrawRectangle(dx, 0,   20, 420, new Color((byte)60, (byte)60, (byte)90, (byte)255));
        Raylib.DrawRectangle(dx, 600, 20, 420, new Color((byte)60, (byte)60, (byte)90, (byte)255));
    }
    // reception wall
    Raylib.DrawRectangle(1050, 0, 20, 400, new Color((byte)60, (byte)60, (byte)90, (byte)255));
    Raylib.DrawRectangle(1050, 600, 20, 420, new Color((byte)60, (byte)60, (byte)90, (byte)255));

    // hallway walls
    Raylib.DrawRectangle(0,   420, 810, 20, new Color((byte)60, (byte)60, (byte)90, (byte)255));
    Raylib.DrawRectangle(0,   600, 810, 20, new Color((byte)60, (byte)60, (byte)90, (byte)255));

    // =============================================
    // OFFICES — top row
    // =============================================
    int[] offX = { 0, 350, 700 };
    string[] offLabels = { "DETECTIVE", "SERGEANT", "OFFICER" };

    for (int r = 0; r < 3; r++)
    {
        int rx = offX[r];

        // desk
        Raylib.DrawRectangle(rx + 40, 40, 260, 60,  new Color((byte)70,  (byte)60,  (byte)40,  (byte)255)); // desk surface
        Raylib.DrawRectangle(rx + 40, 40, 260, 8,   new Color((byte)100, (byte)85,  (byte)55,  (byte)255)); // top edge
        Raylib.DrawRectangle(rx + 40, 40, 8,   60,  new Color((byte)55,  (byte)45,  (byte)30,  (byte)255)); // left leg
        Raylib.DrawRectangle(rx + 292, 40, 8,  60,  new Color((byte)55,  (byte)45,  (byte)30,  (byte)255)); // right leg

        // computer
        Raylib.DrawRectangle(rx + 160, 20, 36, 24,  new Color((byte)15,  (byte)15,  (byte)40,  (byte)255)); // monitor
        Raylib.DrawRectangle(rx + 162, 22, 32, 20,  new Color((byte)0,   (byte)60,  (byte)140, (byte)255)); // screen
        Raylib.DrawRectangle(rx + 172, 42, 14, 5,   new Color((byte)10,  (byte)10,  (byte)30,  (byte)255)); // stand

        // papers on desk
        Raylib.DrawRectangle(rx + 50,  50, 50, 35,  new Color((byte)240, (byte)235, (byte)210, (byte)255));
        Raylib.DrawRectangle(rx + 56,  56, 38, 4,   new Color((byte)100, (byte)100, (byte)180, (byte)255));
        Raylib.DrawRectangle(rx + 56,  64, 30, 4,   new Color((byte)100, (byte)100, (byte)180, (byte)255));

        // chair
        Raylib.DrawRectangle(rx + 40,  130, 40, 40, new Color((byte)30, (byte)30, (byte)80,  (byte)255));
        Raylib.DrawRectangle(rx + 40,  130, 40, 7,  new Color((byte)20, (byte)20, (byte)60,  (byte)255)); // seat top
        Raylib.DrawRectangle(rx + 40,  130, 7,  40, new Color((byte)15, (byte)15, (byte)50,  (byte)255)); // left arm
        Raylib.DrawRectangle(rx + 73,  130, 7,  40, new Color((byte)15, (byte)15, (byte)50,  (byte)255)); // right arm

        // room label
        Raylib.DrawRectangle(rx + 80, 4, 180, 24,   new Color((byte)30, (byte)30, (byte)120, (byte)255));
        Program.DrawTextUI(offLabels[r], rx + 88, 8, 14, new Color((byte)220, (byte)220, (byte)255, (byte)255));
    }

    // =============================================
    // OFFICES — bottom row (interview / holding)
    // =============================================
    string[] botLabels = { "HOLDING A", "INTERVIEW", "HOLDING B" };

    for (int r = 0; r < 3; r++)
    {
        int rx = offX[r];

        // desk
        Raylib.DrawRectangle(rx + 40, 830, 260, 60, new Color((byte)70,  (byte)60,  (byte)40,  (byte)255));
        Raylib.DrawRectangle(rx + 40, 830, 260, 8,  new Color((byte)100, (byte)85,  (byte)55,  (byte)255));
        Raylib.DrawRectangle(rx + 40, 830, 8,   60, new Color((byte)55,  (byte)45,  (byte)30,  (byte)255));
        Raylib.DrawRectangle(rx + 292, 830, 8,  60, new Color((byte)55,  (byte)45,  (byte)30,  (byte)255));

        // chair
        Raylib.DrawRectangle(rx + 40, 760, 40, 40,  new Color((byte)30, (byte)30, (byte)80,  (byte)255));
        Raylib.DrawRectangle(rx + 40, 760, 40, 7,   new Color((byte)20, (byte)20, (byte)60,  (byte)255));
        Raylib.DrawRectangle(rx + 40, 760, 7,  40,  new Color((byte)15, (byte)15, (byte)50,  (byte)255));
        Raylib.DrawRectangle(rx + 73, 760, 7,  40,  new Color((byte)15, (byte)15, (byte)50,  (byte)255));

        // papers
        Raylib.DrawRectangle(rx + 50,  838, 50, 35, new Color((byte)240, (byte)235, (byte)210, (byte)255));
        Raylib.DrawRectangle(rx + 56,  844, 38, 4,  new Color((byte)180, (byte)60,  (byte)60,  (byte)255));
        Raylib.DrawRectangle(rx + 56,  852, 30, 4,  new Color((byte)180, (byte)60,  (byte)60,  (byte)255));

        // room label
        Raylib.DrawRectangle(rx + 80, 972, 180, 24, new Color((byte)30, (byte)30, (byte)120, (byte)255));
        Program.DrawTextUI(botLabels[r], rx + 88, 976, 14, new Color((byte)220, (byte)220, (byte)255, (byte)255));
    }

    // =============================================
    // HALLWAY — centre strip
    // =============================================
    Raylib.DrawRectangle(0, 440, 1050, 160, new Color((byte)40, (byte)40, (byte)65, (byte)255)); // hallway floor
    Raylib.DrawRectangle(0, 505, 1050, 10,  new Color((byte)80, (byte)80, (byte)130, (byte)150)); // centre line

    // =============================================
    // NOTICE BOARD (on hallway wall)
    // =============================================
    Raylib.DrawRectangle(420, 380, 180, 100, new Color((byte)120, (byte)80,  (byte)30,  (byte)255)); // board frame
    Raylib.DrawRectangle(425, 385, 170, 90,  new Color((byte)200, (byte)160, (byte)80,  (byte)255)); // board surface
    Raylib.DrawRectangle(435, 392, 70,  30,  new Color((byte)240, (byte)235, (byte)210, (byte)255)); // paper 1
    Raylib.DrawRectangle(515, 392, 70,  30,  new Color((byte)240, (byte)210, (byte)210, (byte)255)); // paper 2 (red tint = wanted)
    Raylib.DrawRectangle(435, 430, 70,  30,  new Color((byte)210, (byte)235, (byte)240, (byte)255)); // paper 3
    Program.DrawTextUI("NOTICES", 448, 465, 12, new Color((byte)80, (byte)50, (byte)10, (byte)255));

    // Entrance mat
    Raylib.DrawRectangle(1175, 897, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(1185, 907, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}



    if (currentBuilding.BuildingName == "HOSPITAL")
{
    // --- floor tiles ---
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)240, (byte)245, (byte)245, (byte)255)
                : new Color((byte)220, (byte)235, (byte)235, (byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }

    // --- ceiling/wall color ---
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)200, (byte)220, (byte)220, (byte)120));

    
    // --- front counter (centred in extra room: 1070–1400, counter width 280) ---
    Raylib.DrawRectangle(1095, 80, 280, 50, new Color((byte)180, (byte)200, (byte)200, (byte)255));
    Raylib.DrawRectangle(1095, 80, 280, 8,  new Color((byte)220, (byte)235, (byte)235, (byte)255));
    Raylib.DrawRectangle(1095, 80, 8,   50, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(1367, 80, 8,   50, new Color((byte)160, (byte)180, (byte)180, (byte)255));

    // computer on counter (centred at x=1235, above counter)
    Raylib.DrawRectangle(1215, 60, 40, 28, new Color((byte)20,  (byte)20,  (byte)80,  (byte)255));
    Raylib.DrawRectangle(1217, 62, 36, 22, new Color((byte)0,   (byte)160, (byte)200, (byte)255));

    // medical cross sign (centred at x=1235)
    Raylib.DrawRectangle(1228, 20, 14, 42, new Color((byte)220, (byte)50,  (byte)50,  (byte)255));
    Raylib.DrawRectangle(1214, 34, 42, 14, new Color((byte)220, (byte)50,  (byte)50,  (byte)255));

    Program.DrawTextUI("RECEPTION", 1178, 92, 22, new Color((byte)60, (byte)80, (byte)80, (byte)255));

    // replace hallway wall draw
    Raylib.DrawRectangle(120, 400, 370, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(120, 600, 370, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(610, 400, 200, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(610, 600, 200, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(950, 400, 120, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(950, 600, 100, 20, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    

    // --- room dividers ---
    int[] divX = { 350, 700, 1050 };
    foreach (int dx in divX)
    {
        Raylib.DrawRectangle(dx, 0,   20, 400, new Color((byte)160, (byte)180, (byte)180, (byte)255));
        Raylib.DrawRectangle(dx, 600, 20, 420, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    }

    // --- rooms ---
    int[] roomX = { 0, 350, 700, 1050 };
    string[] roomNums = { "01", "02", "03", "04", "05", "06" };

    // top rooms
    for (int r = 0; r < 3; r++)
    {
        int rx = roomX[r];

        // bed frame
        Raylib.DrawRectangle(rx + 30, 30, 180, 80, new Color((byte)200, (byte)210, (byte)220, (byte)255));
        Raylib.DrawRectangle(rx + 30, 30, 180, 15, new Color((byte)180, (byte)190, (byte)210, (byte)255));
        // pillow
        Raylib.DrawRectangle(rx + 40, 34, 60, 26, new Color((byte)245, (byte)245, (byte)255, (byte)255));
        // blanket
        Raylib.DrawRectangle(rx + 40, 62, 160, 40, new Color((byte)100, (byte)160, (byte)200, (byte)255));
        Raylib.DrawRectangle(rx + 40, 62, 160, 6,  new Color((byte)120, (byte)180, (byte)220, (byte)255));
        // bed rails
        Raylib.DrawRectangle(rx + 30, 45, 8, 55,   new Color((byte)160, (byte)170, (byte)180, (byte)255));
        Raylib.DrawRectangle(rx + 202, 45, 8, 55,  new Color((byte)160, (byte)170, (byte)180, (byte)255));

        // side table
        Raylib.DrawRectangle(rx + 215, 30, 15, 30, new Color((byte)180, (byte)150, (byte)100, (byte)255));
        Raylib.DrawRectangle(rx + 215, 30, 15, 4,  new Color((byte)200, (byte)170, (byte)120, (byte)255));
        // cup on table
        Raylib.DrawRectangle(rx + 219, 22, 8, 10,  new Color((byte)180, (byte)200, (byte)220, (byte)255));

        // chair
        Raylib.DrawRectangle(rx + 30,  120, 35, 35, new Color((byte)80, (byte)120, (byte)160, (byte)255));
        Raylib.DrawRectangle(rx + 30,  120, 35, 6,  new Color((byte)60, (byte)100, (byte)140, (byte)255));
        Raylib.DrawRectangle(rx + 30,  120, 6,  35, new Color((byte)60, (byte)80,  (byte)100, (byte)255));
        Raylib.DrawRectangle(rx + 59,  120, 6,  35, new Color((byte)60, (byte)80,  (byte)100, (byte)255));

        // room number sign
        Raylib.DrawRectangle(rx + 95, 4, 40, 22, new Color((byte)220, (byte)50, (byte)50, (byte)255));
        Program.DrawTextUI($"R{roomNums[r]}", rx + 100, 7, 16, Color.White);

        // IV stand
        Raylib.DrawRectangle(rx + 200, 25, 4, 80, new Color((byte)160, (byte)160, (byte)170, (byte)255));
        Raylib.DrawCircle(rx + 202, 25, 8, new Color((byte)200, (byte)220, (byte)240, (byte)220));
    }

    // bottom rooms
    for (int r = 0; r < 3; r++)
    {
        int rx = roomX[r];

        // bed frame
        Raylib.DrawRectangle(rx + 30, 850, 180, 80, new Color((byte)200, (byte)210, (byte)220, (byte)255));
        Raylib.DrawRectangle(rx + 30, 850, 180, 15, new Color((byte)180, (byte)190, (byte)210, (byte)255));
        // pillow
        Raylib.DrawRectangle(rx + 40, 854, 60, 26, new Color((byte)245, (byte)245, (byte)255, (byte)255));
        // blanket
        Raylib.DrawRectangle(rx + 40, 882, 160, 40, new Color((byte)100, (byte)160, (byte)200, (byte)255));
        Raylib.DrawRectangle(rx + 40, 882, 160, 6,  new Color((byte)120, (byte)180, (byte)220, (byte)255));
        // bed rails
        Raylib.DrawRectangle(rx + 30, 865, 8, 55,   new Color((byte)160, (byte)170, (byte)180, (byte)255));
        Raylib.DrawRectangle(rx + 202, 865, 8, 55,  new Color((byte)160, (byte)170, (byte)180, (byte)255));

        // side table
        Raylib.DrawRectangle(rx + 215, 850, 15, 30, new Color((byte)180, (byte)150, (byte)100, (byte)255));
        Raylib.DrawRectangle(rx + 215, 850, 15, 4,  new Color((byte)200, (byte)170, (byte)120, (byte)255));
        // cup on table
        Raylib.DrawRectangle(rx + 219, 842, 8, 10,  new Color((byte)180, (byte)200, (byte)220, (byte)255));

        // chair
        Raylib.DrawRectangle(rx + 30,  800, 35, 35, new Color((byte)80, (byte)120, (byte)160, (byte)255));
        Raylib.DrawRectangle(rx + 30,  800, 35, 6,  new Color((byte)60, (byte)100, (byte)140, (byte)255));
        Raylib.DrawRectangle(rx + 30,  800, 6,  35, new Color((byte)60, (byte)80,  (byte)100, (byte)255));
        Raylib.DrawRectangle(rx + 59,  800, 6,  35, new Color((byte)60, (byte)80,  (byte)100, (byte)255));

        // room number sign
        Raylib.DrawRectangle(rx + 95, 974, 40, 22, new Color((byte)220, (byte)50, (byte)50, (byte)255));
        Program.DrawTextUI($"R{roomNums[r + 3]}", rx + 100, 977, 16, Color.White);

        // IV stand
        Raylib.DrawRectangle(rx + 200, 840, 4, 80, new Color((byte)160, (byte)160, (byte)170, (byte)255));
        Raylib.DrawCircle(rx + 202, 840, 8, new Color((byte)200, (byte)220, (byte)240, (byte)220));

        // Entrance mat
        Raylib.DrawRectangle(1175, 897, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(1185, 907, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));
    }
}
if (currentBuilding.BuildingName == "GAS STATION")
{
    // --- floor — grey/beige concrete tiles ---
    for (int tx = 0; tx < 1400; tx += 80)
        for (int ty = 0; ty < 1000; ty += 80)
        {
            Color tileColor = ((tx / 80 + ty / 80) % 2 == 0)
                ? new Color((byte)195,(byte)190,(byte)165,(byte)255)
                : new Color((byte)180,(byte)175,(byte)152,(byte)255);
            Raylib.DrawRectangle(tx, ty, 80, 80, tileColor);
            Raylib.DrawRectangle(tx, ty, 80, 1, new Color((byte)150,(byte)145,(byte)120,(byte)100));
            Raylib.DrawRectangle(tx, ty, 1, 80, new Color((byte)150,(byte)145,(byte)120,(byte)100));
        }
 
    // --- SERVICE COUNTER (right side) ---
    Raylib.DrawRectangle(800, 80, 380, 50, new Color((byte)60,(byte)60,(byte)55,(byte)255));
    Raylib.DrawRectangle(800, 80, 380, 8, new Color((byte)80,(byte)80,(byte)74,(byte)255));
    Raylib.DrawRectangle(800, 80, 8, 50, new Color((byte)45,(byte)45,(byte)40,(byte)255));
    Raylib.DrawRectangle(1172, 80, 8, 50, new Color((byte)45,(byte)45,(byte)40,(byte)255));
    // register
    Raylib.DrawRectangle(1060, 56, 42, 28, new Color((byte)20,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle(1062, 58, 38, 22, new Color((byte)0,(byte)160,(byte)100,(byte)255));
    Program.DrawTextUI("$", 1076, 60, 20, Color.White);
    // lottery scratch card display
    Raylib.DrawRectangle(820, 60, 70, 22, new Color((byte)220,(byte)180,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(820, 60, 70, 22, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Program.DrawTextUI("LOTTO", 828, 64, 12, new Color((byte)100,(byte)60,(byte)0,(byte)255));
    // card reader terminal
    Raylib.DrawRectangle(900, 58, 30, 26, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(902, 60, 26, 16, new Color((byte)0,(byte)80,(byte)180,(byte)255));
    Program.DrawTextUI("COUNTER", 940, 92, 20, new Color((byte)200,(byte)195,(byte)160,(byte)255));
    
 
    // --- SNACK & DRINK AISLES ---
    Color[][] gsColors = {
        new Color[] {
            new Color((byte)220,(byte)80,(byte)80,(byte)255),
            new Color((byte)255,(byte)160,(byte)0,(byte)255),
            new Color((byte)80,(byte)180,(byte)80,(byte)255),
            new Color((byte)80,(byte)120,(byte)220,(byte)255),
            new Color((byte)200,(byte)200,(byte)50,(byte)255),
            new Color((byte)180,(byte)80,(byte)180,(byte)255),
        },
        new Color[] {
            new Color((byte)220,(byte)80,(byte)80,(byte)255),
            new Color((byte)100,(byte)160,(byte)220,(byte)255),
            new Color((byte)220,(byte)180,(byte)40,(byte)255),
            new Color((byte)80,(byte)200,(byte)100,(byte)255),
            new Color((byte)180,(byte)100,(byte)220,(byte)255),
            new Color((byte)220,(byte)130,(byte)50,(byte)255),
        }
    };
    string[] aisleSignNames = { "SNACKS", "DRINKS" };
 
    int[] gsAisleX = { 80, 340 };
    int[] gsShelfY = { 200, 260, 320, 380, 440, 500 };
 
    for (int col = 0; col < 2; col++)
    {
        int ax = gsAisleX[col];
 
        // aisle sign
        Raylib.DrawRectangle(ax + 30, 168, 120, 24, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Program.DrawTextUI(aisleSignNames[col], ax + 36, 172, 16, Color.Gold);
 
        for (int row = 0; row < 6; row++)
        {
            int sy = gsShelfY[row];
            Color pc = gsColors[col][row];
            // shelf backing
            Raylib.DrawRectangle(ax, sy, 180, 30, new Color((byte)80,(byte)80,(byte)75,(byte)255));
            Raylib.DrawRectangle(ax, sy, 180, 5, new Color((byte)100,(byte)100,(byte)95,(byte)255));
            // products
            for (int p = 0; p < 8; p++)
            {
                int px = ax + 4 + p * 22;
                Raylib.DrawRectangle(px, sy + 5, 18, 23,
                    new Color((byte)Math.Min(255, pc.R + (p%3)*15),
                              (byte)Math.Min(255, pc.G + (p%2)*10),
                              (byte)Math.Min(255, pc.B + (p%4)*8), (byte)255));
                Raylib.DrawRectangle(px, sy + 5, 18, 5, new Color((byte)255,(byte)255,(byte)255,(byte)100));
            }
            // shelf front edge
            Raylib.DrawRectangle(ax, sy + 25, 180, 5, new Color((byte)60,(byte)60,(byte)55,(byte)255));
        }
        // end cap
        Raylib.DrawRectangle(ax + 180, 196, 16, 310, new Color((byte)60,(byte)60,(byte)55,(byte)255));
    }
 
    // --- DRINK FRIDGES (back wall) ---
    int[] fridgeX = { 600, 680, 760 };
    string[] fridgeDrinks = { "COLA", "JUICE", "WATER" };
    Color[] fridgeDrinkColors = {
        new Color((byte)200,(byte)40,(byte)40,(byte)255),
        new Color((byte)220,(byte)180,(byte)40,(byte)255),
        new Color((byte)80,(byte)160,(byte)220,(byte)255)
    };
    for (int f = 0; f < 3; f++)
    {
        int fx = fridgeX[f];
        // fridge body
        Raylib.DrawRectangle(fx, 80, 60, 120, new Color((byte)50,(byte)50,(byte)60,(byte)255));
        Raylib.DrawRectangle(fx, 80, 60, 8, new Color((byte)70,(byte)70,(byte)80,(byte)255));
        // glass door
        Raylib.DrawRectangle(fx + 4, 92, 52, 96, new Color((byte)80,(byte)120,(byte)160,(byte)160));
        // cans/bottles
        for (int row = 0; row < 4; row++)
            for (int col2 = 0; col2 < 3; col2++)
                Raylib.DrawRectangle(fx + 8 + col2 * 14, 96 + row * 22, 12, 18, fridgeDrinkColors[f]);
        // frost effect
        Raylib.DrawRectangle(fx + 4, 92, 52, 6, new Color((byte)180,(byte)210,(byte)240,(byte)100));
        // handle
        Raylib.DrawRectangle(fx + 54, 128, 4, 24, new Color((byte)120,(byte)120,(byte)130,(byte)255));
        Program.DrawTextUI(fridgeDrinks[f], fx + 6, 200, 11, Color.DarkBlue);
    }
    Program.DrawTextUI("COOL DRINKS", 600, 64, 14, Color.DarkGray);
 
    // --- OIL & AUTO PRODUCTS (left wall) ---
    Raylib.DrawRectangle(20, 180, 40, 420, new Color((byte)65,(byte)60,(byte)55,(byte)255));
    Raylib.DrawRectangle(20, 180, 40, 5, new Color((byte)85,(byte)80,(byte)72,(byte)255));
    string[] oilLabels = { "OIL", "WASH", "TYRES", "TOOLS" };
    Color[] oilColors = {
        new Color((byte)40,(byte)40,(byte)20,(byte)255),
        new Color((byte)60,(byte)120,(byte)60,(byte)255),
        new Color((byte)30,(byte)30,(byte)30,(byte)255),
        new Color((byte)120,(byte)80,(byte)30,(byte)255)
    };
    for (int o = 0; o < 4; o++)
    {
        int oy = 192 + o * 96;
        for (int i = 0; i < 3; i++)
            Raylib.DrawRectangle(24, oy + i * 26, 32, 22, oilColors[o]);
        Program.DrawTextUI(oilLabels[o], 18, oy + 72, 10, Color.DarkGray);
    }
    Program.DrawTextUI("AUTO", 20, 164, 12, Color.DarkGray);
 
    // --- TOILET ROOM (back corner) ---
    Raylib.DrawRectangle(1080, 200, 120, 120, new Color((byte)180,(byte)185,(byte)185,(byte)255));
    Raylib.DrawRectangle(1080, 200, 120, 6, new Color((byte)160,(byte)165,(byte)165,(byte)255));
    // toilet
    Raylib.DrawRectangle(1100, 218, 50, 20, new Color((byte)225,(byte)240,(byte)235,(byte)255));
    Raylib.DrawRectangle(1100, 236, 50, 55, new Color((byte)232,(byte)248,(byte)240,(byte)255));
    Raylib.DrawEllipse(1125, 285, 24, 14, new Color((byte)210,(byte)235,(byte)228,(byte)255));
    // sink
    Raylib.DrawRectangle(1088, 290, 50, 25, new Color((byte)232,(byte)248,(byte)240,(byte)255));
    Raylib.DrawRectangle(1090, 295, 46, 18, new Color((byte)215,(byte)235,(byte)228,(byte)255));
    Raylib.DrawCircle(1113, 308, 4, new Color((byte)160,(byte)190,(byte)185,(byte)255));
    Program.DrawTextUI("WC", 1120, 204, 14, Color.DarkGray);
    // WC door
    Raylib.DrawRectangle(1160, 220, 38, 80, new Color((byte)160,(byte)165,(byte)165,(byte)180));
    Raylib.DrawCircle(1163, 260, 3, new Color((byte)140,(byte)140,(byte)150,(byte)255));
 
    // --- MAGAZINE RACK (near counter) ---
    Raylib.DrawRectangle(1196, 80, 55, 90, new Color((byte)100,(byte)80,(byte)50,(byte)255));
    Raylib.DrawRectangle(1196, 80, 55, 6, new Color((byte)130,(byte)105,(byte)70,(byte)255));
    // magazines
    Color[] magColors = {
        new Color((byte)220,(byte)50,(byte)50,(byte)255),
        new Color((byte)50,(byte)100,(byte)200,(byte)255),
        new Color((byte)50,(byte)160,(byte)50,(byte)255),
        new Color((byte)200,(byte)160,(byte)20,(byte)255)
    };
    for (int m = 0; m < 4; m++)
        Raylib.DrawRectangle(1200, 84 + m * 20, 47, 18, magColors[m]);
    
 
    // --- WAITING BENCH ---
    Raylib.DrawRectangle(80, 620, 240, 30, new Color((byte)80,(byte)75,(byte)65,(byte)255));
    Raylib.DrawRectangle(80, 620, 240, 6, new Color((byte)100,(byte)95,(byte)82,(byte)255));
    Raylib.DrawRectangle(80, 620, 6, 30, new Color((byte)65,(byte)60,(byte)50,(byte)255));
    Raylib.DrawRectangle(314, 620, 6, 30, new Color((byte)65,(byte)60,(byte)50,(byte)255));
    // arm dividers
    Raylib.DrawRectangle(160, 620, 6, 30, new Color((byte)65,(byte)60,(byte)50,(byte)255));
    Raylib.DrawRectangle(240, 620, 6, 30, new Color((byte)65,(byte)60,(byte)50,(byte)255));
 
    
 
    // --- ENTRANCE MAT ---
    Raylib.DrawRectangle(480, 870, 200, 30, new Color((byte)80,(byte)75,(byte)60,(byte)255));
    Raylib.DrawRectangleLines(480, 870, 200, 30, new Color((byte)120,(byte)115,(byte)95,(byte)255));
    Program.DrawTextUI("WELCOME", 518, 878, 16, new Color((byte)200,(byte)195,(byte)165,(byte)255));
}


if (currentBuilding.BuildingName == "RANGING SHOP")
{
    // floor — worn oak planks
    for (int tx = 0; tx < 1280; tx += 50)
        for (int ty = 0; ty < 1000; ty += 12)
        {
            byte shade = (byte)(ty % 24 == 0 ? 110 : 95);
            Raylib.DrawRectangle(tx, ty, 50, 12, new Color(shade, (byte)(shade - 20), (byte)10, (byte)255));
        }
    for (int ty = 0; ty < 1000; ty += 12)
        Raylib.DrawRectangle(0, ty, 1280, 1, new Color((byte)60,(byte)30,(byte)5,(byte)60));

    // back wall — dark panelled wood
    Raylib.DrawRectangle(0, 0, 1280, 80, new Color((byte)45,(byte)25,(byte)8,(byte)255));
    for (int p = 0; p < 1280; p += 120)
        Raylib.DrawRectangle(p, 0, 4, 80, new Color((byte)30,(byte)15,(byte)4,(byte)255));

    // counter
    Raylib.DrawRectangle(400, 80, 400, 40, new Color((byte)80,(byte)50,(byte)20,(byte)255));
    Raylib.DrawRectangle(400, 80, 400, 6,  new Color((byte)110,(byte)75,(byte)30,(byte)255));
    Raylib.DrawRectangle(400, 116, 400, 4, new Color((byte)55,(byte)32,(byte)10,(byte)255));

    // bow racks (3 racks with bows drawn on them)
    int[] rackX = { 80, 300, 520 };
    Color rackCol = new Color((byte)70,(byte)42,(byte)14,(byte)255);
    Color bowCol  = new Color((byte)160,(byte)100,(byte)30,(byte)255);
    Color strCol  = new Color((byte)200,(byte)185,(byte)140,(byte)255);
    foreach (int rx in rackX)
    {
        // rack backing
        Raylib.DrawRectangle(rx, 200, 180, 120, new Color((byte)40,(byte)22,(byte)6,(byte)255));
        Raylib.DrawRectangleLines(rx, 200, 180, 120, rackCol);
        // horizontal pegs
        Raylib.DrawRectangle(rx + 10, 224, 160, 6, rackCol);
        Raylib.DrawRectangle(rx + 10, 290, 160, 6, rackCol);
        // two bows hanging
        for (int b = 0; b < 2; b++)
        {
            int bx2 = rx + 30 + b * 80;
            Raylib.DrawCircleLines(bx2, 256, 24, bowCol);
            Raylib.DrawRectangle(bx2 - 2, 232, 4, 48, new Color((byte)40,(byte)22,(byte)6,(byte)255)); // mask right half
            Raylib.DrawLine(bx2, 232, bx2, 280, strCol); // string
        }
    }

    // arrow shelves
    int[] shelfX = { 80, 260, 440 };
    string[] shelfLabel = { "Arrows", "Arrows", "Bolts" };
    Color[] shelfAccent = {
        new Color((byte)160,(byte)120,(byte)40,(byte)255),
        new Color((byte)160,(byte)120,(byte)40,(byte)255),
        new Color((byte)80,(byte)120,(byte)180,(byte)255)
    };
    for (int s = 0; s < 3; s++)
    {
        Raylib.DrawRectangle(shelfX[s], 480, 160, 60, new Color((byte)50,(byte)28,(byte)8,(byte)255));
        Raylib.DrawRectangleLines(shelfX[s], 480, 160, 60, rackCol);
        Raylib.DrawRectangle(shelfX[s], 480, 160, 5, new Color((byte)90,(byte)55,(byte)18,(byte)255));
        // bundle of arrows/bolts
        for (int ar = 0; ar < 5; ar++)
            Raylib.DrawLine(shelfX[s] + 30 + ar * 12, 488, shelfX[s] + 30 + ar * 12, 530, shelfAccent[s]);
        Program.DrawTextUI(shelfLabel[s], shelfX[s] + 10, 534, 14, shelfAccent[s]);
    }

    // wall-mounted target on left wall
    Raylib.DrawCircle(60, 350, 38, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    Raylib.DrawCircle(60, 350, 28, new Color((byte)220,(byte)60,(byte)60,(byte)255));
    Raylib.DrawCircle(60, 350, 18, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    Raylib.DrawCircle(60, 350, 8,  new Color((byte)220,(byte)60,(byte)60,(byte)255));
    // arrow stuck in it
    Raylib.DrawLine(60, 310, 60, 348, new Color((byte)160,(byte)100,(byte)30,(byte)255));
    Raylib.DrawTriangle(new Vector2(56, 310), new Vector2(64, 310), new Vector2(60, 298), new Color((byte)160,(byte)100,(byte)30,(byte)255));

    // entrance mat
    Raylib.DrawRectangle(540, 870, 200, 28, new Color((byte)60,(byte)35,(byte)10,(byte)255));
    Raylib.DrawRectangleLines(540, 870, 200, 28, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Program.DrawTextUI("FLETCHER'S", 566, 878, 16, new Color((byte)200,(byte)160,(byte)60,(byte)255));

}

if (currentBuilding.BuildingName == "WEAPONS")
{
    // --- floor — dark stone tiles ---
    for (int tx = 0; tx < 1400; tx += 70)
        for (int ty = 0; ty < 1000; ty += 70)
        {
            Color tileColor = ((tx / 70 + ty / 70) % 2 == 0)
                ? new Color((byte)40, (byte)40, (byte)45, (byte)255)
                : new Color((byte)35, (byte)35, (byte)40, (byte)255);
            Raylib.DrawRectangle(tx, ty, 70, 70, tileColor);
            Raylib.DrawRectangle(tx, ty, 70, 1, new Color((byte)25,(byte)25,(byte)30,(byte)150));
            Raylib.DrawRectangle(tx, ty, 1, 70, new Color((byte)25,(byte)25,(byte)30,(byte)150));
        }
 
    // --- ambient dark overlay ---
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)10,(byte)10,(byte)20,(byte)100));
 
    // --- front service counter ---
    Raylib.DrawRectangle(400, 80, 500, 50, new Color((byte)50,(byte)50,(byte)55,(byte)255));
    Raylib.DrawRectangle(400, 80, 500, 8, new Color((byte)70,(byte)70,(byte)80,(byte)255));
    Raylib.DrawRectangle(400, 80, 8, 50, new Color((byte)35,(byte)35,(byte)40,(byte)255));
    Raylib.DrawRectangle(892, 80, 8, 50, new Color((byte)35,(byte)35,(byte)40,(byte)255));
    // glass display top on counter
    Raylib.DrawRectangle(420, 55, 460, 28, new Color((byte)60,(byte)80,(byte)100,(byte)140));
    Raylib.DrawRectangleLines(420, 55, 460, 28, new Color((byte)80,(byte)100,(byte)120,(byte)255));
    // weapons in counter display
    // sword
    Raylib.DrawRectangle(440, 60, 50, 6, new Color((byte)180,(byte)190,(byte)200,(byte)255));
    Raylib.DrawRectangle(437, 62, 8, 10, new Color((byte)160,(byte)130,(byte)60,(byte)255));
    // dagger
    Raylib.DrawTriangle(new Vector2(560, 58), new Vector2(556, 72), new Vector2(564, 72), new Color((byte)180,(byte)190,(byte)200,(byte)255));
    Raylib.DrawRectangle(554, 72, 14, 10, new Color((byte)140,(byte)100,(byte)40,(byte)255));
    // axe silhouette
    Raylib.DrawRectangle(640, 58, 8, 22, new Color((byte)150,(byte)160,(byte)170,(byte)255));
    Raylib.DrawTriangle(new Vector2(640, 58), new Vector2(640, 72), new Vector2(625, 65), new Color((byte)160,(byte)170,(byte)180,(byte)255));
    // mace
    Raylib.DrawRectangle(720, 68, 8, 18, new Color((byte)140,(byte)150,(byte)160,(byte)255));
    Raylib.DrawCircle(724, 66, 8, new Color((byte)120,(byte)130,(byte)140,(byte)255));
    // price tags
    Program.DrawTextUI("E = Upgrade Weapon", 480, 92, 16, new Color((byte)200,(byte)200,(byte)100,(byte)255));
 
    // --- BACK WALL WEAPON RACKS ---
    // rack support bars
    Raylib.DrawRectangle(20, 70, 300, 16, new Color((byte)60,(byte)60,(byte)65,(byte)255));
    Raylib.DrawRectangle(20, 170, 300, 16, new Color((byte)60,(byte)60,(byte)65,(byte)255));
    Raylib.DrawRectangle(20, 270, 300, 16, new Color((byte)60,(byte)60,(byte)65,(byte)255));
    // rack side supports
    for (int rx = 25; rx < 310; rx += 60)
    {
        Raylib.DrawRectangle(rx, 70, 8, 220, new Color((byte)45,(byte)45,(byte)50,(byte)255));
    }
 
    // RACK 1 — swords & blades
    string[] swordLabels = { "IRON", "STEEL", "SILVER", "SHADOW" };
    Color[] swordColors = {
        new Color((byte)140,(byte)140,(byte)150,(byte)255),
        new Color((byte)180,(byte)185,(byte)195,(byte)255),
        new Color((byte)200,(byte)210,(byte)225,(byte)255),
        new Color((byte)60,(byte)60,(byte)80,(byte)255)
    };
    for (int i = 0; i < 4; i++)
    {
        int rx = 35 + i * 65;
        // blade
        Raylib.DrawRectangle(rx, 38, 6, 36, swordColors[i]);
        Raylib.DrawRectangle(rx + 1, 36, 4, 4, swordColors[i]); // tip
        // crossguard
        Raylib.DrawRectangle(rx - 6, 70, 18, 5, new Color((byte)160,(byte)130,(byte)50,(byte)255));
        // handle
        Raylib.DrawRectangle(rx + 1, 74, 4, 16, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Program.DrawTextUI(swordLabels[i], rx - 10, 92, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
 
    // RACK 2 — bows & ranged
    string[] bowLabels = { "SHORT", "LONG", "WAR", "SIEGE" };
    Color[] bowColors = {
        new Color((byte)140,(byte)100,(byte)50,(byte)255),
        new Color((byte)120,(byte)80,(byte)30,(byte)255),
        new Color((byte)80,(byte)55,(byte)20,(byte)255),
        new Color((byte)60,(byte)40,(byte)15,(byte)255)
    };
    for (int i = 0; i < 4; i++)
    {
        int rx = 38 + i * 65;
        // bow arc
        Raylib.DrawCircleLines(rx + 3, 155, 18, bowColors[i]);
        // string
        Raylib.DrawLine(rx + 3, 137, rx + 3, 173, new Color((byte)200,(byte)200,(byte)200,(byte)120));
        // arrow on bow
        Raylib.DrawRectangle(rx - 12, 153, 28, 3, new Color((byte)180,(byte)150,(byte)80,(byte)255));
        Raylib.DrawTriangle(new Vector2(rx + 16, 152), new Vector2(rx + 16, 158), new Vector2(rx + 22, 155), new Color((byte)150,(byte)160,(byte)170,(byte)255));
        Program.DrawTextUI(bowLabels[i], rx - 12, 180, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
 
    // RACK 3 — shields
    Color[] shieldColors = {
        new Color((byte)100,(byte)80,(byte)40,(byte)255),
        new Color((byte)60,(byte)60,(byte)70,(byte)255),
        new Color((byte)120,(byte)50,(byte)20,(byte)255),
        new Color((byte)40,(byte)60,(byte)100,(byte)255)
    };
    string[] shieldLabels = { "WOOD", "IRON", "TOWER", "KITE" };
    for (int i = 0; i < 4; i++)
    {
        int rx = 32 + i * 65;
        // shield shape
        Raylib.DrawRectangle(rx, 238, 30, 28, shieldColors[i]);
        Raylib.DrawTriangle(
            new Vector2(rx, 266), new Vector2(rx + 15, 280), new Vector2(rx + 30, 266),
            shieldColors[i]);
        Raylib.DrawRectangleLines(rx, 238, 30, 28, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        // emblem
        Raylib.DrawRectangle(rx + 12, 242, 6, 18, new Color((byte)160,(byte)140,(byte)60,(byte)200));
        Raylib.DrawRectangle(rx + 6,  248, 18, 6, new Color((byte)160,(byte)140,(byte)60,(byte)200));
        Program.DrawTextUI(shieldLabels[i], rx - 5, 284, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    Program.DrawTextUI("BLADES",  50,  22, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    Program.DrawTextUI("RANGED",  50, 122, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    Program.DrawTextUI("SHIELDS", 50, 222, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
 
    // --- DISPLAY CASES (centre floor) ---
    Color[] caseColors = {
        new Color((byte)180,(byte)60,(byte)60,(byte)255),   // red - special weapons
        new Color((byte)60,(byte)100,(byte)180,(byte)255),  // blue - magic items
        new Color((byte)60,(byte)160,(byte)60,(byte)255),   // green - potions/aids
        new Color((byte)160,(byte)120,(byte)40,(byte)255),  // gold - legendary
    };
    string[] caseTitles = { "SPECIAL", "ENCHANTED", "SUPPLIES", "LEGENDARY" };
    int[] caseX = { 420, 650, 420, 650 };
    int[] caseY = { 280, 280, 400, 400 };
 
    for (int c = 0; c < 4; c++)
    {
        int cx = caseX[c]; int cy = caseY[c];
        // glass case
        Raylib.DrawRectangle(cx, cy, 180, 60, new Color((byte)40,(byte)50,(byte)60,(byte)255));
        Raylib.DrawRectangle(cx + 2, cy + 2, 176, 56, new Color((byte)50,(byte)60,(byte)75,(byte)255));
        Raylib.DrawRectangle(cx + 2, cy + 2, 176, 20, new Color((byte)60,(byte)80,(byte)110,(byte)180)); // glass top
        // case item placeholder (glowing)
        Raylib.DrawCircle(cx + 90, cy + 38, 12, new Color(caseColors[c].R, caseColors[c].G, caseColors[c].B, (byte)100));
        Raylib.DrawCircle(cx + 90, cy + 38, 8,  caseColors[c]);
        Raylib.DrawCircle(cx + 90, cy + 38, 4,  new Color((byte)255,(byte)255,(byte)255,(byte)180));
        // case label
        Raylib.DrawRectangle(cx, cy + 60, 180, 20, new Color((byte)30,(byte)30,(byte)35,(byte)255));
        Program.DrawTextUI(caseTitles[c], cx + 30, cy + 64, 14, caseColors[c]);
    }
 
    // --- ARMOUR STANDS (right wall) ---
    int[] armourX = { 1100, 1230 };
    string[] armourNames = { "PLATE", "CHAIN" };
    Color[] armourColors = {
        new Color((byte)160,(byte)165,(byte)175,(byte)255),
        new Color((byte)120,(byte)125,(byte)135,(byte)255)
    };
    for (int a = 0; a < 2; a++)
    {
        int ax = armourX[a];
        // stand pole
        Raylib.DrawRectangle(ax + 32, 80, 6, 140, new Color((byte)60,(byte)60,(byte)65,(byte)255));
        Raylib.DrawRectangle(ax + 20, 215, 30, 6, new Color((byte)60,(byte)60,(byte)65,(byte)255)); // base
        // helmet
        Raylib.DrawCircle(ax + 35, 92, 18, armourColors[a]);
        Raylib.DrawRectangle(ax + 22, 92, 26, 14, armourColors[a]);
        Raylib.DrawRectangle(ax + 26, 102, 18, 5, new Color((byte)20,(byte)20,(byte)30,(byte)255)); // visor gap
        // chest
        Raylib.DrawRectangle(ax + 12, 110, 46, 55, armourColors[a]);
        Raylib.DrawRectangle(ax + 14, 112, 42, 8, new Color((byte)Math.Min(255,armourColors[a].R+20),(byte)Math.Min(255,armourColors[a].G+20),(byte)Math.Min(255,armourColors[a].B+20),(byte)255));
        Raylib.DrawRectangle(ax + 32, 112, 2, 53, new Color((byte)Math.Max(0,armourColors[a].R-20),(byte)Math.Max(0,armourColors[a].G-20),(byte)Math.Max(0,armourColors[a].B-20),(byte)255)); // centre line
        // shoulder pauldrons
        Raylib.DrawCircle(ax + 12, 118, 10, armourColors[a]);
        Raylib.DrawCircle(ax + 58, 118, 10, armourColors[a]);
        Program.DrawTextUI(armourNames[a], ax + 6, 228, 14, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
 
    // --- AMMO/SUPPLIES SHELVES (right wall lower) ---
    int[] suppY = { 280, 350, 420, 490 };
    string[] suppLabels = { "ARROWS", "BOLTS", "BOMBS", "POTIONS" };
    Color[] suppColors = {
        new Color((byte)140,(byte)100,(byte)40,(byte)255),
        new Color((byte)80,(byte)80,(byte)90,(byte)255),
        new Color((byte)180,(byte)80,(byte)20,(byte)255),
        new Color((byte)80,(byte)30,(byte)120,(byte)255)
    };
    for (int s = 0; s < 4; s++)
    {
        int sy = suppY[s];
        // shelf
        Raylib.DrawRectangle(1050, sy, 320, 30, new Color((byte)50,(byte)50,(byte)55,(byte)255));
        Raylib.DrawRectangle(1050, sy, 320, 5, new Color((byte)65,(byte)65,(byte)70,(byte)255));
        // items on shelf
        for (int i = 0; i < 12; i++)
        {
            int ix = 1058 + i * 24;
            Raylib.DrawRectangle(ix, sy + 6, 18, 22, suppColors[s]);
            Raylib.DrawRectangle(ix, sy + 6, 18, 4, new Color((byte)255,(byte)255,(byte)255,(byte)60));
        }
        Program.DrawTextUI(suppLabels[s], 1052, sy + 32, 12, new Color((byte)140,(byte)140,(byte)150,(byte)255));
    }
 
    // --- ENTRANCE MAT ---
    Raylib.DrawRectangle(500, 870, 200, 30, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(500, 870, 200, 30, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    Program.DrawTextUI("WELCOME", 550, 878, 16, new Color((byte)120,(byte)120,(byte)140,(byte)255));
}

// ── BIKE DEALER INTERIOR ──────────────────────────────────────────────────
if (currentBuilding.BuildingName == "BIKE DEALER")
{
    // --- polished concrete floor tiles ---
    for (int tx = 0; tx < 1400; tx += 100)
    {
        Color tileColor = (tx / 100 % 2 == 0)
            ? new Color((byte)55,  (byte)65,  (byte)85,  (byte)255)
            : new Color((byte)48,  (byte)58,  (byte)78,  (byte)255);
        Raylib.DrawRectangle(tx, 0, 100, 1000, tileColor);
        Raylib.DrawRectangle(tx, 0, 2,   1000, new Color((byte)35, (byte)42, (byte)60, (byte)255));
    }
    for (int ty = 0; ty < 1000; ty += 100)
        Raylib.DrawRectangle(0, ty, 1400, 2, new Color((byte)35, (byte)42, (byte)60, (byte)180));
 
    // --- front service counter ---
    Raylib.DrawRectangle(400, 80, 600, 55, new Color((byte)30,  (byte)40,  (byte)65,  (byte)255));
    Raylib.DrawRectangle(400, 80, 600, 8,  new Color((byte)60,  (byte)100, (byte)180, (byte)255)); // blue accent strip
    Raylib.DrawRectangle(400, 80, 8,   55, new Color((byte)20,  (byte)28,  (byte)50,  (byte)255));
    Raylib.DrawRectangle(992, 80, 8,   55, new Color((byte)20,  (byte)28,  (byte)50,  (byte)255));
    // register / POS terminal
    Raylib.DrawRectangle(880, 55, 45,  32, new Color((byte)25,  (byte)25,  (byte)30,  (byte)255));
    Raylib.DrawRectangle(882, 57, 41,  24, new Color((byte)0,   (byte)140, (byte)255, (byte)255));
    Program.DrawTextUI("$", 896, 59, 20, Color.White);
    Program.DrawTextUI("SERVICE COUNTER", 480, 95, 20, new Color((byte)100, (byte)170, (byte)255, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 190, 1400, 28, new Color((byte)20, (byte)28, (byte)50, (byte)230));
    string[] bikeZoneNames = { "MOUNTAIN BIKES", "ROAD BIKES", "ACCESSORIES" };
    int[]    bikeZoneX     = { 60, 430, 820 };
    for (int a = 0; a < 3; a++)
        Program.DrawTextUI(bikeZoneNames[a], bikeZoneX[a], 196, 16, new Color((byte)100, (byte)180, (byte)255, (byte)255));
 
    // --- display islands (3 columns, 2 rows) ---
    // Each island: a raised platform with a bike silhouette on it
    int[] islandX = { 100, 520, 940 };
    int[] islandY = { 240, 400 };
 
    Color[] islandColors = {
        new Color((byte)45,  (byte)60,  (byte)90,  (byte)255),
        new Color((byte)40,  (byte)55,  (byte)85,  (byte)255),
        new Color((byte)50,  (byte)65,  (byte)95,  (byte)255),
    };
 
    // Bike frame colours per column
    Color[] bikeColors = {
        new Color((byte)220, (byte)60,  (byte)60,  (byte)255), // red MTB
        new Color((byte)60,  (byte)180, (byte)80,  (byte)255), // green road
        new Color((byte)60,  (byte)140, (byte)220, (byte)255), // blue accessory display
    };
 
    for (int col = 0; col < 3; col++)
    {
        for (int row = 0; row < 2; row++)
        {
            int ix = islandX[col];
            int iy = islandY[row];
 
            // platform
            Raylib.DrawRectangle(ix,      iy,      260, 120, islandColors[col]);
            Raylib.DrawRectangle(ix,      iy,      260, 6,   new Color((byte)80, (byte)120, (byte)200, (byte)255)); // top accent
            Raylib.DrawRectangle(ix,      iy + 114, 260, 6,  new Color((byte)20, (byte)28,  (byte)50,  (byte)255)); // shadow
 
            // simple bike silhouette (wheels + frame)
            int bkx = ix + 30;
            int bky = iy + 30;
            Color bc = bikeColors[col];
            // rear wheel
            Raylib.DrawCircleLines(bkx + 30,  bky + 50, 28, bc);
            Raylib.DrawCircleLines(bkx + 30,  bky + 50, 20, new Color(bc.R, bc.G, bc.B, (byte)80));
            // front wheel
            Raylib.DrawCircleLines(bkx + 130, bky + 50, 28, bc);
            Raylib.DrawCircleLines(bkx + 130, bky + 50, 20, new Color(bc.R, bc.G, bc.B, (byte)80));
            // frame triangle
            Raylib.DrawLine(bkx + 30,  bky + 50, bkx + 80,  bky + 10, bc);
            Raylib.DrawLine(bkx + 80,  bky + 10, bkx + 130, bky + 50, bc);
            Raylib.DrawLine(bkx + 80,  bky + 10, bkx + 50,  bky + 50, bc);
            Raylib.DrawLine(bkx + 50,  bky + 50, bkx + 130, bky + 50, bc);
            // handlebar
            Raylib.DrawLine(bkx + 115, bky + 10, bkx + 145, bky + 10, bc);
            Raylib.DrawLine(bkx + 130, bky + 10, bkx + 130, bky + 22, bc);
            // seat
            Raylib.DrawLine(bkx + 65,  bky + 10, bkx + 85,  bky + 10, bc);
            Raylib.DrawLine(bkx + 75,  bky + 10, bkx + 75,  bky + 22, bc);
 
            // price tag
            Raylib.DrawRectangle(ix + 180, iy + 8, 70, 22, new Color((byte)20, (byte)20, (byte)25, (byte)220));
            Program.DrawTextUI(row == 0 ? "$1,299" : "$899", ix + 184, iy + 12, 14, Color.Gold);
        }
    }
 
    // --- tool / parts racks (left wall) ---
    int[] rackY = { 230, 340, 450, 560, 670 };
    foreach (int ry in rackY)
    {
        Raylib.DrawRectangle(20, ry, 40, 80, new Color((byte)25, (byte)32, (byte)50, (byte)255));
        Raylib.DrawRectangle(20, ry, 40, 6,  new Color((byte)60, (byte)100, (byte)180, (byte)255));
        // hanging tools
        for (int t = 0; t < 4; t++)
        {
            int tx2 = 24 + t * 9;
            Raylib.DrawRectangle(tx2, ry + 10, 6, 40, new Color((byte)140, (byte)150, (byte)160, (byte)255));
            Raylib.DrawRectangle(tx2, ry + 10, 6, 6,  new Color((byte)200, (byte)200, (byte)210, (byte)255));
        }
    }
 
    // --- parts display (right wall) ---
    int[] partsY = { 230, 330, 430, 530, 630 };
    Color[] partColors = {
        new Color((byte)220,(byte)80, (byte)80, (byte)255),
        new Color((byte)80, (byte)180,(byte)220,(byte)255),
        new Color((byte)220,(byte)180,(byte)60, (byte)255),
        new Color((byte)80, (byte)220,(byte)120,(byte)255),
        new Color((byte)180,(byte)80, (byte)220,(byte)255),
    };
    for (int ps = 0; ps < 5; ps++)
    {
        int py = partsY[ps];
        Raylib.DrawRectangle(1340, py, 40, 80, new Color((byte)25, (byte)32, (byte)50, (byte)255));
        Raylib.DrawRectangle(1340, py, 40, 6,  new Color((byte)60, (byte)100, (byte)180, (byte)255));
        // part boxes
        for (int p = 0; p < 3; p++)
        {
            byte r2 = (byte)Math.Min(255, partColors[ps].R + p * 15);
            byte g2 = (byte)Math.Min(255, partColors[ps].G + p * 10);
            byte b2 = (byte)Math.Min(255, partColors[ps].B + p * 8);
            Raylib.DrawRectangle(1342 + p * 12, py + 10, 10, 40, new Color(r2, g2, b2, (byte)255));
        }
    }
 
    // --- entrance mat ---
    Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)30, (byte)50, (byte)90, (byte)255));
    Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)100, (byte)180, (byte)255));
    Program.DrawTextUI("WELCOME", 612, 940, 22, new Color((byte)100, (byte)170, (byte)255, (byte)255));
}
 
// ── CAR DEALER INTERIOR ───────────────────────────────────────────────────
else if (currentBuilding.BuildingName == "CAR DEALER")
{
    // --- glossy showroom floor (light grey, reflective look) ---
    for (int tx = 0; tx < 1400; tx += 140)
    {
        Color tileColor = (tx / 140 % 2 == 0)
            ? new Color((byte)210, (byte)212, (byte)218, (byte)255)
            : new Color((byte)200, (byte)202, (byte)208, (byte)255);
        Raylib.DrawRectangle(tx, 0, 140, 1000, tileColor);
        Raylib.DrawRectangle(tx, 0, 2,   1000, new Color((byte)180, (byte)182, (byte)190, (byte)255));
    }
    for (int ty = 0; ty < 1000; ty += 140)
        Raylib.DrawRectangle(0, ty, 1400, 2, new Color((byte)180, (byte)182, (byte)190, (byte)180));
 
    // --- service / sales counter (right side) ---
    Raylib.DrawRectangle(800, 50, 560, 70, new Color((byte)18, (byte)18, (byte)22, (byte)255));
    Raylib.DrawRectangle(800, 50, 560, 8,  new Color((byte)60, (byte)120, (byte)220, (byte)255)); // blue accent
    Raylib.DrawRectangle(800, 50, 8,   70, new Color((byte)12, (byte)12, (byte)16,  (byte)255));
    // monitor
    Raylib.DrawRectangle(1260, 28, 60, 30, new Color((byte)15, (byte)15, (byte)18, (byte)255));
    Raylib.DrawRectangle(1262, 30, 56, 24, new Color((byte)0,  (byte)120, (byte)255, (byte)255));
    Program.DrawTextUI("SALES", 1266, 35, 14, Color.White);
    Program.DrawTextUI("FINANCE & SALES COUNTER", 820, 68, 18, new Color((byte)80, (byte)150, (byte)255, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 190, 800, 28, new Color((byte)15, (byte)15, (byte)18, (byte)230));
    Program.DrawTextUI("SHOWROOM FLOOR", 80, 196, 18, new Color((byte)80, (byte)150, (byte)255, (byte)255));
 
    // --- divider pillars ---
    int[] pillarX = { 440, 860 };
    foreach (int px in pillarX)
    {
        Raylib.DrawRectangle(px, 40,  20, 780, new Color((byte)22, (byte)22, (byte)28,  (byte)255));
        Raylib.DrawRectangle(px, 40,  20, 8,   new Color((byte)60, (byte)120, (byte)220, (byte)255)); // cap accent
        Raylib.DrawRectangle(px, 812, 20, 8,   new Color((byte)60, (byte)120, (byte)220, (byte)255)); // base accent
    }
 
    // --- car display bays (2 rows × 3 cols) ---
    int[] bayX2 = { 60, 480, 900 };
    int[] bayY2 = { 130, 380 };
 
    // Car colours per bay
    Color[,] carColors = {
        {
            new Color((byte)200,(byte)30, (byte)30, (byte)255),  // red sports
            new Color((byte)20, (byte)20, (byte)20, (byte)255),  // black sedan
            new Color((byte)220,(byte)220,(byte)230,(byte)255),  // silver SUV
        },
        {
            new Color((byte)30, (byte)80, (byte)200,(byte)255),  // blue coupe
            new Color((byte)30, (byte)140,(byte)60, (byte)255),  // green hatch
            new Color((byte)200,(byte)160,(byte)20, (byte)255),  // gold luxury
        }
    };
 
    string[,] carLabels = {
        { "$28,990", "$34,500", "$42,000" },
        { "$22,990", "$19,990", "$89,990" },
    };
 
    for (int row = 0; row < 2; row++)
    {
        for (int col = 0; col < 3; col++)
        {
            int bx2 = bayX2[col];
            int by2 = bayY2[row];
            Color cc = carColors[row, col];
 
            // bay platform
            Raylib.DrawRectangle(bx2,      by2,      340, 180, new Color((byte)195, (byte)198, (byte)205, (byte)255));
            Raylib.DrawRectangle(bx2,      by2,      340, 4,   new Color((byte)60,  (byte)120, (byte)220, (byte)255));
            Raylib.DrawRectangle(bx2,      by2 + 176, 340, 4,  new Color((byte)170, (byte)172, (byte)180, (byte)255));
 
            // car body (simplified top-down silhouette)
            int cx = bx2 + 30;
            int cy = by2 + 40;
            // car base
            Raylib.DrawRectangle(cx,       cy + 20,  280, 90,  cc);
            // car roof
            Raylib.DrawRectangle(cx + 50,  cy,       180, 70,  new Color((byte)Math.Max(0,cc.R-30),(byte)Math.Max(0,cc.G-30),(byte)Math.Max(0,cc.B-30),(byte)255));
            // windscreen tint
            Raylib.DrawRectangle(cx + 55,  cy + 5,   80,  55,  new Color((byte)140,(byte)170,(byte)210,(byte)120));
            // rear screen
            Raylib.DrawRectangle(cx + 145, cy + 5,   80,  55,  new Color((byte)140,(byte)170,(byte)210,(byte)120));
            // wheels (4 circles)
            Raylib.DrawCircle(cx + 40,  cy + 110, 18, Color.DarkGray);
            Raylib.DrawCircle(cx + 40,  cy + 110, 10, Color.LightGray);
            Raylib.DrawCircle(cx + 240, cy + 110, 18, Color.DarkGray);
            Raylib.DrawCircle(cx + 240, cy + 110, 10, Color.LightGray);
            // headlights
            Raylib.DrawRectangle(cx,       cy + 25, 10, 15, new Color((byte)255,(byte)240,(byte)180,(byte)200));
            Raylib.DrawRectangle(cx + 270, cy + 25, 10, 15, new Color((byte)255,(byte)60, (byte)60, (byte)180));
 
            // price tag
            Raylib.DrawRectangle(bx2 + 220, by2 + 8, 110, 24, new Color((byte)15, (byte)15, (byte)18, (byte)220));
            Program.DrawTextUI(carLabels[row, col], bx2 + 225, by2 + 12, 14, Color.Gold);
        }
    }
 
    // --- entrance mat ---
    Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)18, (byte)18, (byte)22, (byte)255));
    Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)120, (byte)220, (byte)255));
    Program.DrawTextUI("WELCOME", 610, 940, 22, new Color((byte)80, (byte)150, (byte)255, (byte)255));
}
 
// ── BARN DEALER INTERIOR ──────────────────────────────────────────────────
else if (currentBuilding.BuildingName == "BARN DEALER")
{
    // --- dirt / straw floor ---
    for (int tx = 0; tx < 1400; tx += 90)
    {
        Color plankColor = (tx / 90 % 2 == 0)
            ? new Color((byte)130, (byte)95,  (byte)50, (byte)255)
            : new Color((byte)118, (byte)85,  (byte)42, (byte)255);
        Raylib.DrawRectangle(tx, 0, 90, 1000, plankColor);
        Raylib.DrawRectangle(tx, 0, 2,  1000, new Color((byte)100, (byte)70, (byte)30, (byte)255));
    }
    for (int ty = 0; ty < 1000; ty += 110)
        Raylib.DrawRectangle(0, ty, 1400, 2, new Color((byte)100, (byte)70, (byte)30, (byte)160));
 
    // hay scattering (decorative patches)
    int[] hayX = { 80, 300, 600, 900, 1150, 1300 };
    int[] hayY2 = { 140, 500, 750, 200, 600, 400 };
    for (int h = 0; h < 6; h++)
    {
        Raylib.DrawRectangle(hayX[h], hayY2[h], 40, 12, new Color((byte)200, (byte)170, (byte)60, (byte)120));
        Raylib.DrawRectangle(hayX[h] + 5, hayY2[h] + 4, 30, 6, new Color((byte)220, (byte)190, (byte)80, (byte)100));
    }
 
    // --- reception / hay counter ---
    Raylib.DrawRectangle(350, 60, 700, 55, new Color((byte)110, (byte)70, (byte)30, (byte)255));
    Raylib.DrawRectangle(350, 60, 700, 8,  new Color((byte)150, (byte)100, (byte)40, (byte)255)); // top highlight
    Raylib.DrawRectangle(350, 60, 8,   55, new Color((byte)90,  (byte)55, (byte)20, (byte)255));
    Raylib.DrawRectangle(1042, 60, 8,  55, new Color((byte)90,  (byte)55, (byte)20, (byte)255));
    // hay bales on counter
    for (int hb = 0; hb < 6; hb++)
    {
        int hbx = 360 + hb * 110;
        Raylib.DrawRectangle(hbx, 28, 80, 38, new Color((byte)200,(byte)165,(byte)55,(byte)255));
        Raylib.DrawRectangleLines(hbx, 28, 80, 38, new Color((byte)160,(byte)125,(byte)35,(byte)255));
        // bale straps
        Raylib.DrawRectangle(hbx + 25, 28, 4, 38, new Color((byte)140,(byte)100,(byte)30,(byte)255));
        Raylib.DrawRectangle(hbx + 51, 28, 4, 38, new Color((byte)140,(byte)100,(byte)30,(byte)255));
    }
    Program.DrawTextUI("RECEPTION", 620, 78, 22, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 165, 1400, 28, new Color((byte)80, (byte)45, (byte)18, (byte)220));
    string[] barnZones = { "HORSES", "MOUNTS", "FEED & TACK" };
    int[]    barnZoneX = { 140, 570, 1000 };
    for (int a = 0; a < 3; a++)
        Program.DrawTextUI(barnZones[a], barnZoneX[a], 172, 16, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
    // --- left stalls (3 stalls) ---
    int[] leftStallY2 = { 200, 390, 580 };
    Color[] horseColors = {
        new Color((byte)80,  (byte)50,  (byte)25,  (byte)255), // bay
        new Color((byte)200, (byte)195, (byte)180, (byte)255), // grey
        new Color((byte)40,  (byte)28,  (byte)15,  (byte)255), // black
    };
 
    for (int s = 0; s < 3; s++)
    {
        int sy2 = leftStallY2[s];
 
        // stall walls
        Raylib.DrawRectangle(20,  sy2,      280, 150, new Color((byte)105, (byte)72, (byte)38, (byte)255));
        Raylib.DrawRectangle(20,  sy2,      280, 6,   new Color((byte)140, (byte)95, (byte)45, (byte)255));
        Raylib.DrawRectangle(20,  sy2,      6,   150, new Color((byte)90,  (byte)60, (byte)28, (byte)255));
        Raylib.DrawRectangle(294, sy2,      6,   150, new Color((byte)90,  (byte)60, (byte)28, (byte)255));
        // stall gate bars
        for (int bar = 0; bar < 4; bar++)
            Raylib.DrawRectangle(26 + bar * 60, sy2 + 80, 8, 70, new Color((byte)90, (byte)60, (byte)28, (byte)255));
        // horizontal gate rail
        Raylib.DrawRectangle(20, sy2 + 100, 280, 6, new Color((byte)90, (byte)60, (byte)28, (byte)255));
 
        // horse silhouette (side view)
        Color hc = horseColors[s];
        int hx = 50;
        int hy = sy2 + 30;
        // body
        Raylib.DrawRectangle(hx + 20, hy + 20, 140, 60, hc);
        // neck
        Raylib.DrawRectangle(hx + 140, hy,     30,  50, hc);
        // head
        Raylib.DrawRectangle(hx + 155, hy - 10, 35, 30, hc);
        // ear
        Raylib.DrawRectangle(hx + 165, hy - 18, 8,  10, hc);
        // eye
        Raylib.DrawCircle(hx + 180, hy - 2, 3, new Color((byte)20, (byte)12, (byte)5, (byte)255));
        // legs
        Raylib.DrawRectangle(hx + 30,  hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 55,  hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 110, hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 135, hy + 78, 14, 38, hc);
        // tail
        Raylib.DrawRectangle(hx + 16, hy + 22, 8, 45, new Color((byte)Math.Min(255, hc.R + 20),(byte)Math.Min(255, hc.G + 15),(byte)Math.Min(255, hc.B + 10),(byte)255));
 
        // name plate
        string[] horseNames = { "Thunder - $4,500", "Snowflake - $5,200", "Midnight - $6,800" };
        Raylib.DrawRectangle(20, sy2 + 2, 160, 18, new Color((byte)70, (byte)42, (byte)16, (byte)220));
        Program.DrawTextUI(horseNames[s], 24, sy2 + 4, 12, Color.Gold);
    }
 
    // --- right stalls (3 stalls) ---
    int[] rightStallY2 = { 200, 390, 580 };
    Color[] horseColors2 = {
        new Color((byte)160, (byte)100, (byte)45,  (byte)255), // chestnut
        new Color((byte)110, (byte)75,  (byte)35,  (byte)255), // dun
        new Color((byte)190, (byte)170, (byte)120, (byte)255), // palomino
    };
 
    for (int s = 0; s < 3; s++)
    {
        int sy2 = rightStallY2[s];
 
        Raylib.DrawRectangle(1100, sy2,      280, 150, new Color((byte)105, (byte)72, (byte)38, (byte)255));
        Raylib.DrawRectangle(1100, sy2,      280, 6,   new Color((byte)140, (byte)95, (byte)45, (byte)255));
        Raylib.DrawRectangle(1100, sy2,      6,   150, new Color((byte)90,  (byte)60, (byte)28, (byte)255));
        Raylib.DrawRectangle(1374, sy2,      6,   150, new Color((byte)90,  (byte)60, (byte)28, (byte)255));
        for (int bar = 0; bar < 4; bar++)
            Raylib.DrawRectangle(1106 + bar * 60, sy2 + 80, 8, 70, new Color((byte)90, (byte)60, (byte)28, (byte)255));
        Raylib.DrawRectangle(1100, sy2 + 100, 280, 6, new Color((byte)90, (byte)60, (byte)28, (byte)255));
 
        Color hc = horseColors2[s];
        int hx = 1130;
        int hy = sy2 + 30;
        Raylib.DrawRectangle(hx + 20, hy + 20, 140, 60, hc);
        Raylib.DrawRectangle(hx + 140, hy,     30,  50, hc);
        Raylib.DrawRectangle(hx + 155, hy - 10, 35, 30, hc);
        Raylib.DrawRectangle(hx + 165, hy - 18, 8,  10, hc);
        Raylib.DrawCircle(hx + 180, hy - 2, 3, new Color((byte)20, (byte)12, (byte)5, (byte)255));
        Raylib.DrawRectangle(hx + 30,  hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 55,  hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 110, hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 135, hy + 78, 14, 38, hc);
        Raylib.DrawRectangle(hx + 16, hy + 22, 8, 45, new Color((byte)Math.Min(255, hc.R + 20),(byte)Math.Min(255, hc.G + 15),(byte)Math.Min(255, hc.B + 10),(byte)255));
 
        string[] horseNames2 = { "Blaze - $3,900", "Dusty - $4,100", "Goldie - $7,200" };
        Raylib.DrawRectangle(1100, sy2 + 2, 160, 18, new Color((byte)70, (byte)42, (byte)16, (byte)220));
        Program.DrawTextUI(horseNames2[s], 1104, sy2 + 4, 12, Color.Gold);
    }
 
    // --- central hay bale display ---
    int[] centralBaleX = { 430, 530, 630, 730, 830, 930 };
    foreach (int cbx in centralBaleX)
    {
        // bottom bale
        Raylib.DrawRectangle(cbx, 320, 80, 45, new Color((byte)200,(byte)165,(byte)55,(byte)255));
        Raylib.DrawRectangleLines(cbx, 320, 80, 45, new Color((byte)160,(byte)125,(byte)35,(byte)255));
        Raylib.DrawRectangle(cbx + 25, 320, 4, 45, new Color((byte)140,(byte)100,(byte)30,(byte)255));
        Raylib.DrawRectangle(cbx + 51, 320, 4, 45, new Color((byte)140,(byte)100,(byte)30,(byte)255));
        // top bale (stacked, offset)
        Raylib.DrawRectangle(cbx + 8, 280, 72, 42, new Color((byte)210,(byte)175,(byte)65,(byte)255));
        Raylib.DrawRectangleLines(cbx + 8, 280, 72, 42, new Color((byte)165,(byte)130,(byte)40,(byte)255));
        Raylib.DrawRectangle(cbx + 30, 280, 4, 42, new Color((byte)145,(byte)105,(byte)32,(byte)255));
    }
    // sign above bales
    Raylib.DrawRectangle(400, 260, 600, 22, new Color((byte)75, (byte)42, (byte)16, (byte)220));
    Program.DrawTextUI("PREMIUM FEED & HAY", 468, 264, 18, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
    // --- entrance mat ---
    Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)80, (byte)55, (byte)25, (byte)255));
    Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)140, (byte)95, (byte)40, (byte)255));
    Program.DrawTextUI("WELCOME", 610, 940, 22, new Color((byte)220, (byte)185, (byte)100, (byte)255));
}

if (currentBuilding.BuildingName == "STORE")
{
    // --- wooden floor tiles ---
    for (int tx = 0; tx < 1400; tx += 80)
    {
        Color plankColor = (tx / 80 % 2 == 0)
            ? new Color((byte)160, (byte)110, (byte)60, (byte)255)
            : new Color((byte)145, (byte)95,  (byte)50, (byte)255);
        Raylib.DrawRectangle(tx, 0, 80, 1000, plankColor);
        Raylib.DrawRectangle(tx, 0, 2,  1000, new Color((byte)120, (byte)80, (byte)40, (byte)255));
    }
    // horizontal plank lines
    for (int ty = 0; ty < 1000; ty += 120)
        Raylib.DrawRectangle(0, ty, 1400, 2, new Color((byte)120, (byte)80, (byte)40, (byte)180));

    // --- front counter ---
    Raylib.DrawRectangle(400, 80, 500, 50, new Color((byte)100, (byte)60, (byte)20, (byte)255));
    Raylib.DrawRectangle(400, 80, 500, 8,  new Color((byte)140, (byte)90, (byte)30, (byte)255));
    Raylib.DrawRectangle(400, 80, 8,   50, new Color((byte)80,  (byte)50, (byte)15, (byte)255));
    Raylib.DrawRectangle(892, 80, 8,   50, new Color((byte)80,  (byte)50, (byte)15, (byte)255));
    // register
    Raylib.DrawRectangle(820, 58, 40, 28, new Color((byte)30,  (byte)30,  (byte)30,  (byte)255));
    Raylib.DrawRectangle(822, 60, 36, 22, new Color((byte)0,   (byte)180, (byte)100, (byte)255));
    Program.DrawTextUI("$", 834, 62, 20, Color.White);
    Program.DrawTextUI("COUNTER", 540, 92, 22, new Color((byte)200, (byte)160, (byte)80, (byte)255));

    // --- aisle sign bar ---
    Raylib.DrawRectangle(0, 190, 1400, 25, new Color((byte)60, (byte)40, (byte)20, (byte)220));
    string[] aisleNames = { "FOOD & DRINK", "TOOLS & GEAR", "SUPPLIES" };
    int[] aisleSignX = { 80, 420, 760 };
    for (int a = 0; a < 3; a++)
        Program.DrawTextUI(aisleNames[a], aisleSignX[a], 195, 16, Color.Gold);

   // --- aisle shelves ---
Color[][] productColors = {
    new Color[] {
        new Color((byte)220,(byte)80, (byte)80, (byte)255),
        new Color((byte)255,(byte)160,(byte)0,  (byte)255),
        new Color((byte)80, (byte)180,(byte)80, (byte)255),
        new Color((byte)80, (byte)120,(byte)220,(byte)255),
        new Color((byte)200,(byte)200,(byte)50, (byte)255),
        new Color((byte)180,(byte)80, (byte)180,(byte)255),
        new Color((byte)240,(byte)120,(byte)60, (byte)255),
        new Color((byte)60, (byte)180,(byte)180,(byte)255),
    },
    new Color[] {
        new Color((byte)80, (byte)80, (byte)80, (byte)255),
        new Color((byte)160,(byte)120,(byte)60, (byte)255),
        new Color((byte)100,(byte)100,(byte)120,(byte)255),
        new Color((byte)180,(byte)140,(byte)80, (byte)255),
        new Color((byte)120,(byte)80, (byte)60, (byte)255),
        new Color((byte)80, (byte)100,(byte)80, (byte)255),
        new Color((byte)140,(byte)100,(byte)80, (byte)255),
        new Color((byte)100,(byte)80, (byte)100,(byte)255),
    },
    new Color[] {
        new Color((byte)200,(byte)200,(byte)200,(byte)255),
        new Color((byte)160,(byte)200,(byte)160,(byte)255),
        new Color((byte)200,(byte)180,(byte)140,(byte)255),
        new Color((byte)160,(byte)160,(byte)200,(byte)255),
        new Color((byte)220,(byte)160,(byte)120,(byte)255),
        new Color((byte)140,(byte)180,(byte)200,(byte)255),
        new Color((byte)180,(byte)200,(byte)160,(byte)255),
        new Color((byte)200,(byte)160,(byte)180,(byte)255),
    }
};

// Evenly spaced: room x=50..1350 (1300px), 3 aisles × 300px wide
// 4 gaps of 100px each: before aisle1, between 1&2, between 2&3, after aisle3
int[] aisleStartX = { 150, 550, 950 };
int[] shelfYPositions = { 230, 290, 380, 440, 530, 590, 670, 730 };

for (int col = 0; col < 3; col++)
{
    int ax = aisleStartX[col];
    for (int row = 0; row < 8; row++)
    {
        int sy = shelfYPositions[row];
        Color pc = productColors[col][row];

        // shelf backing wood
        Raylib.DrawRectangle(ax, sy, 280, 35, new Color((byte)100, (byte)65, (byte)25, (byte)255));
        Raylib.DrawRectangle(ax, sy, 280, 5,  new Color((byte)130, (byte)85, (byte)35, (byte)255));

        // products on shelf
        for (int p = 0; p < 12; p++)
        {
            int px = ax + 4 + p * 22;
            byte rShade = (byte)Math.Min(255, pc.R + (p % 3) * 15);
            byte gShade = (byte)Math.Min(255, pc.G + (p % 2) * 10);
            byte bShade = (byte)Math.Min(255, pc.B + (p % 4) * 8);
            Raylib.DrawRectangle(px, sy + 6, 18, 26, new Color(rShade, gShade, bShade, (byte)255));
            Raylib.DrawRectangle(px, sy + 10, 18, 6, new Color((byte)255,(byte)255,(byte)255,(byte)120));
        }
        // shelf front edge
        Raylib.DrawRectangle(ax, sy + 30, 280, 5, new Color((byte)80, (byte)50, (byte)20, (byte)255));
    }

    // end cap shelf
    Raylib.DrawRectangle(ax + 280, 220, 20, 510, new Color((byte)80, (byte)50, (byte)20, (byte)255));

    // aisle number post
    Raylib.DrawRectangle(ax + 100, 166, 80, 22, new Color((byte)40, (byte)25, (byte)10, (byte)255));
    Program.DrawTextUI($"AISLE {col + 1}", ax + 104, 169, 15, Color.Gold);
}


// --- back wall display shelves ---
// Mat occupies x=550..850, so two shelf banks either side
// All shifted down 80px from original (820 → 900)

int[] backShelfY = { 860, 907, 944 };

// Left bank: x=50, width=500 (stops before mat at x=550)
// Right bank: x=850, width=500 (starts after mat, ends at x=1350)
int[] bankX     = { 50,  850 };
int[] bankWidth = { 500, 500 };

for (int bank = 0; bank < 2; bank++)
{
    int bx = bankX[bank];
    int bw = bankWidth[bank];

    for (int bs = 0; bs < 3; bs++)
    {
        int bsy = backShelfY[bs];

        // shelf backing wood
        Raylib.DrawRectangle(bx, bsy, bw, 30, new Color((byte)100, (byte)65, (byte)25, (byte)255));
        Raylib.DrawRectangle(bx, bsy, bw, 5,  new Color((byte)130, (byte)85, (byte)35, (byte)255));

        // products — one per 24px slot
        int slots = bw / 24;
        for (int p = 0; p < slots; p++)
        {
            int px = bx + 4 + p * 24;
            byte r = (byte)(120 + (p * 13) % 100);
            byte g = (byte)(80  + (p * 7)  % 80);
            byte b = (byte)(40  + (p * 19) % 120);
            Raylib.DrawRectangle(px, bsy + 6, 20, 22, new Color(r, g, b, (byte)255));
            Raylib.DrawRectangle(px, bsy + 6, 20, 5,  new Color((byte)255, (byte)255, (byte)255, (byte)80));
        }

        // shelf front edge
        Raylib.DrawRectangle(bx, bsy + 25, bw, 5, new Color((byte)80, (byte)50, (byte)20, (byte)255));
    }
}

// --- entrance mat ---
Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)40, (byte)80, (byte)40, (byte)255));
Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)100, (byte)60, (byte)255));
Program.DrawTextUI("WELCOME", 610, 940, 22, new Color((byte)180, (byte)220, (byte)180, (byte)255));

// --- entrance mat ---
Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)40, (byte)80, (byte)40, (byte)255));
Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)100, (byte)60, (byte)255));
Program.DrawTextUI("WELCOME", 610, 940, 22, new Color((byte)180, (byte)220, (byte)180, (byte)255));
}

    if (currentBuilding.BuildingName == "GYM")
{
    if (gymCounterNPC != null)
    {
        gymCounterNPC.Draw();
        if (Vector2.Distance(player.Center, gymCounterNPC.Position) < 120)
            DrawSpeechBubble(gymCounterNPC.Position, gymCounterNPC.Dialogue,
                new Color((byte)50,(byte)100,(byte)180,(byte)255));
    }
}

  if (currentBuilding.BuildingName == "MY HOUSE")
{
    // ── BASE FLOOR — warm hardwood planks ──
    for (int tx = 0; tx < 800; tx += 40)
        for (int ty = 0; ty < 1000; ty += 10)
        {
            byte shade = (byte)(ty % 20 == 0 ? 168 : 158);
            Raylib.DrawRectangle(tx, ty, 40, 10, new Color(shade, (byte)(shade - 20), (byte)80, (byte)255));
        }
    // plank lines
    for (int ty = 0; ty < 1000; ty += 10)
        Raylib.DrawRectangle(0, ty, 800, 1, new Color((byte)120, (byte)80, (byte)40, (byte)60));
    for (int tx = 0; tx < 800; tx += 40)
        Raylib.DrawRectangle(tx, 0, 1, 1000, new Color((byte)100, (byte)60, (byte)30, (byte)40));

    // ── BATHROOM FLOOR — white tiles ──
    for (int tx = 800; tx < 1400; tx += 40)
        for (int ty = 0; ty < 405; ty += 40)
        {
            Color tc = ((tx / 40 + ty / 40) % 2 == 0)
                ? new Color((byte)240, (byte)248, (byte)244, (byte)255)
                : new Color((byte)220, (byte)236, (byte)230, (byte)255);
            Raylib.DrawRectangle(tx, ty, 40, 40, tc);
            Raylib.DrawRectangle(tx, ty, 40, 1, new Color((byte)180, (byte)210, (byte)200, (byte)120));
            Raylib.DrawRectangle(tx, ty, 1, 40, new Color((byte)180, (byte)210, (byte)200, (byte)120));
        }

    // ── BEDROOM FLOOR — carpet ──
    for (int tx = 800; tx < 1400; tx += 20)
        for (int ty = 405; ty < 1000; ty += 20)
            Raylib.DrawRectangle(tx, ty, 20, 20, new Color((byte)88, (byte)110, (byte)148, (byte)255));
    // carpet texture lines
    for (int tx = 800; tx < 1400; tx += 20)
        Raylib.DrawRectangle(tx, 405, 1, 595, new Color((byte)70, (byte)90, (byte)130, (byte)100));
    for (int ty = 405; ty < 1000; ty += 20)
        Raylib.DrawRectangle(800, ty, 600, 1, new Color((byte)70, (byte)90, (byte)130, (byte)100));

   
    // ═══════════════════════════════════════
    // KITCHEN
    // ═══════════════════════════════════════
    // bench shadow
    Raylib.DrawRectangle(64, 59, 530, 55, new Color((byte)80, (byte)50, (byte)20, (byte)80));
    // bench surface
    Raylib.DrawRectangle(60, 55, 530, 55, new Color((byte)208, (byte)184, (byte)152, (byte)255));
    // bench highlight
    Raylib.DrawRectangle(60, 55, 530, 5, new Color((byte)230, (byte)210, (byte)180, (byte)255));
    // bench front edge shadow
    Raylib.DrawRectangle(60, 105, 530, 5, new Color((byte)160, (byte)130, (byte)100, (byte)200));
    // bench left wall
    Raylib.DrawRectangle(60, 110, 55, 200, new Color((byte)208, (byte)184, (byte)152, (byte)255));
    Raylib.DrawRectangle(60, 110, 5, 200, new Color((byte)230, (byte)210, (byte)180, (byte)255));

    // stove — shadow then body
    Raylib.DrawRectangle(127, 122, 100, 100, new Color((byte)60, (byte)60, (byte)60, (byte)100));
    Raylib.DrawRectangle(125, 120, 100, 100, new Color((byte)100, (byte)100, (byte)100, (byte)255));
    Raylib.DrawRectangle(125, 120, 100, 4, new Color((byte)150, (byte)150, (byte)150, (byte)255));
    // burners with glow ring
    int[] bx = {148, 202}; int[] by = {143, 195};
    foreach (int bbx in bx) foreach (int bby in by)
    {
        Raylib.DrawCircle(bbx, bby, 13, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawCircle(bbx, bby, 10, new Color((byte)85, (byte)85, (byte)85, (byte)255));
        Raylib.DrawCircle(bbx, bby, 5,  new Color((byte)50, (byte)50, (byte)50, (byte)255));
    }

    // fridge — shadow, body, highlight, handle
    Raylib.DrawRectangle(242, 122, 90, 120, new Color((byte)60, (byte)60, (byte)60, (byte)80));
    Raylib.DrawRectangle(240, 120, 90, 120, new Color((byte)210, (byte)215, (byte)215, (byte)255));
    Raylib.DrawRectangle(240, 120, 90, 4,   new Color((byte)230, (byte)235, (byte)235, (byte)255));
    Raylib.DrawRectangle(240, 120, 4, 120,  new Color((byte)230, (byte)235, (byte)235, (byte)255));
    Raylib.DrawRectangle(240, 178, 90, 2,   new Color((byte)160, (byte)165, (byte)165, (byte)255)); // shelf line
    Raylib.DrawRectangle(322, 130, 6, 45,   new Color((byte)160, (byte)165, (byte)165, (byte)255)); // handle
    Raylib.DrawRectangle(322, 185, 6, 45,   new Color((byte)160, (byte)165, (byte)165, (byte)255)); // lower handle

    // sink in bench
    Raylib.DrawRectangle(380, 62, 80, 42, new Color((byte)170, (byte)185, (byte)185, (byte)255)); // sink basin
    Raylib.DrawRectangle(383, 65, 74, 36, new Color((byte)140, (byte)160, (byte)160, (byte)255)); // inner basin
    Raylib.DrawRectangle(383, 65, 74, 3,  new Color((byte)180, (byte)200, (byte)200, (byte)255)); // rim highlight
    Raylib.DrawRectangle(415, 58, 8, 10,  new Color((byte)160, (byte)160, (byte)160, (byte)255)); // tap base
    Raylib.DrawRectangle(412, 52, 14, 6,  new Color((byte)180, (byte)180, (byte)180, (byte)255)); // tap spout
    Raylib.DrawCircle(416, 80, 4, new Color((byte)100, (byte)140, (byte)160, (byte)180));         // drain

    // microwave on bench
    Raylib.DrawRectangle(490, 58, 90, 45, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    Raylib.DrawRectangle(492, 60, 65, 41, new Color((byte)20, (byte)20, (byte)50, (byte)255)); // window
    Raylib.DrawRectangle(492, 60, 65, 3,  new Color((byte)40, (byte)40, (byte)80, (byte)180));
    Raylib.DrawRectangle(559, 60, 20, 41, new Color((byte)80, (byte)80, (byte)80, (byte)255)); // control panel
    Raylib.DrawRectangle(562, 65, 14, 6,  new Color((byte)0, (byte)200, (byte)80, (byte)255));  // green display

    // ═══════════════════════════════════════
    // DINING
    // ═══════════════════════════════════════
    // rug under table
    Raylib.DrawRectangle(100, 400, 340, 175, new Color((byte)160, (byte)100, (byte)60, (byte)120));
    Raylib.DrawRectangle(104, 404, 332, 167, new Color((byte)0, (byte)0, (byte)0, (byte)0));
    // table shadow
    Raylib.DrawRectangle(134, 420, 260, 130, new Color((byte)80, (byte)50, (byte)20, (byte)100));
    // table surface
    Raylib.DrawRectangle(130, 415, 260, 130, new Color((byte)184, (byte)152, (byte)104, (byte)255));
    Raylib.DrawRectangle(130, 415, 260, 6,   new Color((byte)210, (byte)178, (byte)130, (byte)255)); // top highlight
    Raylib.DrawRectangle(130, 415, 5, 130,   new Color((byte)210, (byte)178, (byte)130, (byte)255)); // left highlight
    Raylib.DrawRectangle(130, 540, 260, 5,   new Color((byte)140, (byte)110, (byte)70, (byte)255));  // bottom shadow
    // wood grain lines
    for (int gx = 140; gx < 385; gx += 22)
        Raylib.DrawRectangle(gx, 415, 1, 130, new Color((byte)160, (byte)130, (byte)85, (byte)80));

    // chairs — with back rest detail
    int[][] chairsTop = { new[]{148,395}, new[]{228,395} };
    int[][] chairsSide = { new[]{100,428}, new[]{308,428} };
    int[][] chairsBot = { new[]{148,543}, new[]{228,543} };
    foreach (var c in chairsTop)
    {
        Raylib.DrawRectangle(c[0], c[1], 50, 22, new Color((byte)200, (byte)170, (byte)120, (byte)255));
        Raylib.DrawRectangle(c[0], c[1], 50, 4,  new Color((byte)220, (byte)195, (byte)145, (byte)255));
        Raylib.DrawRectangle(c[0]+4, c[1]+6, 6, 14, new Color((byte)160, (byte)130, (byte)80, (byte)255));
        Raylib.DrawRectangle(c[0]+40, c[1]+6, 6, 14, new Color((byte)160, (byte)130, (byte)80, (byte)255));
    }
    foreach (var c in chairsBot)
    {
        Raylib.DrawRectangle(c[0], c[1], 50, 22, new Color((byte)200, (byte)170, (byte)120, (byte)255));
        Raylib.DrawRectangle(c[0], c[1]+18, 50, 4,  new Color((byte)160, (byte)130, (byte)80, (byte)255));
        Raylib.DrawRectangle(c[0]+4, c[1]+4, 6, 14, new Color((byte)160, (byte)130, (byte)80, (byte)255));
        Raylib.DrawRectangle(c[0]+40, c[1]+4, 6, 14, new Color((byte)160, (byte)130, (byte)80, (byte)255));
    }
    foreach (var c in chairsSide)
    {
        Raylib.DrawRectangle(c[0], c[1], 22, 50, new Color((byte)200, (byte)170, (byte)120, (byte)255));
        Raylib.DrawRectangle(c[0], c[1], 4, 50,  new Color((byte)220, (byte)195, (byte)145, (byte)255));
        Raylib.DrawRectangle(c[0]+6, c[1]+4, 14, 6, new Color((byte)160, (byte)130, (byte)80, (byte)255));
        Raylib.DrawRectangle(c[0]+6, c[1]+40, 14, 6, new Color((byte)160, (byte)130, (byte)80, (byte)255));
    }

    // ═══════════════════════════════════════
    // LOUNGE
    // ═══════════════════════════════════════
    // rug
    Raylib.DrawRectangle(130, 680, 420, 180, new Color((byte)120, (byte)80, (byte)50, (byte)100));
    // TV unit / stand
    Raylib.DrawRectangle(568, 660, 36, 175, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangle(568, 660, 36, 6,   new Color((byte)70, (byte)70, (byte)70, (byte)255));
    // TV screen
    Raylib.DrawRectangle(572, 664, 28, 163, new Color((byte)10, (byte)10, (byte)30, (byte)255));
    Raylib.DrawRectangle(574, 666, 24, 159, new Color((byte)15, (byte)20, (byte)60, (byte)255));
    // screen glow lines
    Raylib.DrawRectangle(574, 680, 24, 2, new Color((byte)30, (byte)60, (byte)120, (byte)180));
    Raylib.DrawRectangle(574, 710, 24, 2, new Color((byte)30, (byte)60, (byte)120, (byte)180));
    Raylib.DrawRectangle(574, 740, 24, 2, new Color((byte)30, (byte)60, (byte)120, (byte)180));

    // couch bottom wall — cushion detail
    Raylib.DrawRectangle(60, 880, 340, 75, new Color((byte)180, (byte)148, (byte)96, (byte)255));  // base
    Raylib.DrawRectangle(60, 880, 340, 10, new Color((byte)210, (byte)178, (byte)120, (byte)255)); // top highlight
    Raylib.DrawRectangle(60, 880, 6, 75,   new Color((byte)210, (byte)178, (byte)120, (byte)255)); // left highlight
    Raylib.DrawRectangle(60, 946, 340, 9,  new Color((byte)140, (byte)110, (byte)65, (byte)255));  // bottom shadow
    // cushions
    Raylib.DrawRectangle(68,  890, 100, 55, new Color((byte)200, (byte)168, (byte)112, (byte)255));
    Raylib.DrawRectangle(178, 890, 100, 55, new Color((byte)200, (byte)168, (byte)112, (byte)255));
    Raylib.DrawRectangle(288, 890, 100, 55, new Color((byte)200, (byte)168, (byte)112, (byte)255));
    // cushion lines
    Raylib.DrawRectangle(168, 890, 4, 55, new Color((byte)155, (byte)125, (byte)75, (byte)200));
    Raylib.DrawRectangle(278, 890, 4, 55, new Color((byte)155, (byte)125, (byte)75, (byte)200));

    // couch left wall — cushion detail
    Raylib.DrawRectangle(60, 660, 75, 216, new Color((byte)180, (byte)148, (byte)96, (byte)255));
    Raylib.DrawRectangle(60, 660, 10, 216, new Color((byte)210, (byte)178, (byte)120, (byte)255));
    Raylib.DrawRectangle(60, 656, 75, 6,   new Color((byte)210, (byte)178, (byte)120, (byte)255));
    Raylib.DrawRectangle(127, 660, 8, 216, new Color((byte)140, (byte)110, (byte)65, (byte)255));
    // cushions
    Raylib.DrawRectangle(70, 668, 55, 95,  new Color((byte)200, (byte)168, (byte)112, (byte)255));
    Raylib.DrawRectangle(70, 773, 55, 95,  new Color((byte)200, (byte)168, (byte)112, (byte)255));
    Raylib.DrawRectangle(70, 763, 55, 4,   new Color((byte)155, (byte)125, (byte)75, (byte)200));

    // coffee table
    Raylib.DrawRectangle(190, 810, 160, 60, new Color((byte)160, (byte)120, (byte)70, (byte)255));
    Raylib.DrawRectangle(190, 810, 160, 5,  new Color((byte)190, (byte)155, (byte)100, (byte)255));
    Raylib.DrawRectangle(190, 810, 5, 60,   new Color((byte)190, (byte)155, (byte)100, (byte)255));
    Raylib.DrawRectangle(190, 866, 160, 4,  new Color((byte)120, (byte)85, (byte)40, (byte)255));
    // mug on table
    Raylib.DrawRectangle(232, 820, 18, 22, new Color((byte)220, (byte)215, (byte)205, (byte)255));
    Raylib.DrawRectangle(232, 820, 18, 3,  new Color((byte)200, (byte)195, (byte)185, (byte)255));
    Raylib.DrawRectangle(250, 826, 6, 10,  new Color((byte)200, (byte)195, (byte)185, (byte)255)); // handle
    Raylib.DrawRectangle(234, 824, 14, 2,  new Color((byte)140, (byte)80, (byte)40, (byte)200));   // coffee

    // ═══════════════════════════════════════
    // HALLWAY
    // ═══════════════════════════════════════
    // entry mat — woven texture
    Raylib.DrawRectangle(620, 930, 160, 30, new Color((byte)80, (byte)130, (byte)80, (byte)255));
    for (int mx = 622; mx < 778; mx += 8)
        Raylib.DrawRectangle(mx, 930, 4, 30, new Color((byte)100, (byte)155, (byte)100, (byte)180));
    Raylib.DrawRectangleLines(620, 930, 160, 30, new Color((byte)50, (byte)100, (byte)50, (byte)255));
    Program.DrawTextUI("ENTRY", 648, 938, 16, Color.White);

    // ═══════════════════════════════════════
    // BATHROOM
    // ═══════════════════════════════════════

    // shower — tiled walls inside, glass door
    Raylib.DrawRectangle(1050, 55, 290, 170, new Color((byte)140, (byte)190, (byte)175, (byte)255)); // shower tray
    Raylib.DrawRectangle(1050, 55, 290, 6,   new Color((byte)100, (byte)160, (byte)140, (byte)255));
    // tile grid inside shower
    for (int stx = 1052; stx < 1338; stx += 30)
        for (int sty = 58; sty < 222; sty += 30)
        {
            Raylib.DrawRectangle(stx, sty, 28, 28, new Color((byte)155, (byte)205, (byte)190, (byte)255));
            Raylib.DrawRectangle(stx, sty, 28, 1,  new Color((byte)120, (byte)170, (byte)155, (byte)180));
            Raylib.DrawRectangle(stx, sty, 1, 28,  new Color((byte)120, (byte)170, (byte)155, (byte)180));
        }
    // shower head
    Raylib.DrawRectangle(1310, 58, 6, 40,  new Color((byte)180, (byte)190, (byte)190, (byte)255));
    Raylib.DrawRectangle(1298, 92, 24, 6,  new Color((byte)180, (byte)190, (byte)190, (byte)255));
    Raylib.DrawCircle(1310, 98, 8, new Color((byte)160, (byte)170, (byte)170, (byte)255));
    // water drops
    Raylib.DrawRectangle(1296, 108, 2, 6,  new Color((byte)100, (byte)180, (byte)200, (byte)160));
    Raylib.DrawRectangle(1304, 112, 2, 8,  new Color((byte)100, (byte)180, (byte)200, (byte)160));
    Raylib.DrawRectangle(1312, 106, 2, 7,  new Color((byte)100, (byte)180, (byte)200, (byte)160));

    // toilet — cistern + bowl + seat
    Raylib.DrawRectangle(820, 55, 80, 35,  new Color((byte)225, (byte)240, (byte)235, (byte)255)); // cistern
    Raylib.DrawRectangle(820, 55, 80, 4,   new Color((byte)245, (byte)255, (byte)250, (byte)255)); // cistern top
    Raylib.DrawRectangle(820, 88, 4, 4,    new Color((byte)180, (byte)210, (byte)200, (byte)255)); // flush btn
    Raylib.DrawRectangle(830, 90, 60, 70,  new Color((byte)232, (byte)248, (byte)240, (byte)255)); // bowl outer
    Raylib.DrawRectangle(835, 96, 50, 58,  new Color((byte)210, (byte)235, (byte)228, (byte)255)); // bowl inner
    Raylib.DrawEllipse(860, 148, 36, 18, new Color((byte)200, (byte)230, (byte)222, (byte)255));   // seat

    // sink — pedestal style
    Raylib.DrawRectangle(1270, 295, 8, 30,  new Color((byte)200, (byte)215, (byte)210, (byte)255)); // pedestal
    Raylib.DrawRectangle(1260, 320, 80, 75, new Color((byte)232, (byte)248, (byte)240, (byte)255)); // basin
    Raylib.DrawRectangle(1260, 320, 80, 5,  new Color((byte)245, (byte)255, (byte)250, (byte)255)); // rim highlight
    Raylib.DrawRectangle(1268, 328, 64, 55, new Color((byte)210, (byte)235, (byte)228, (byte)255)); // inner basin
    Raylib.DrawCircle(1300, 370, 5, new Color((byte)160, (byte)190, (byte)185, (byte)255));          // drain
    // tap
    Raylib.DrawRectangle(1296, 314, 8, 10, new Color((byte)190, (byte)195, (byte)195, (byte)255));
    Raylib.DrawRectangle(1290, 310, 20, 5, new Color((byte)200, (byte)205, (byte)205, (byte)255));

    // ═══════════════════════════════════════
    // BEDROOM
    // ═══════════════════════════════════════

    // bed — frame, mattress, pillow, blanket
    Raylib.DrawRectangle(1028, 448, 304, 274, new Color((byte)100, (byte)75, (byte)50, (byte)255));  // frame shadow
    Raylib.DrawRectangle(1025, 445, 305, 275, new Color((byte)120, (byte)90, (byte)60, (byte)255));  // wooden frame
    Raylib.DrawRectangle(1025, 445, 305, 12,  new Color((byte)150, (byte)115, (byte)80, (byte)255)); // headboard highlight
    Raylib.DrawRectangle(1030, 458, 295, 260, new Color((byte)215, (byte)225, (byte)235, (byte)255)); // mattress
    // blanket
    Raylib.DrawRectangle(1030, 500, 295, 218, new Color((byte)80, (byte)110, (byte)160, (byte)255));
    Raylib.DrawRectangle(1030, 500, 295, 8,   new Color((byte)100, (byte)135, (byte)185, (byte)255)); // fold
    // blanket texture lines
    for (int bly = 510; bly < 718; bly += 25)
        Raylib.DrawRectangle(1030, bly, 295, 1, new Color((byte)70, (byte)95, (byte)145, (byte)120));
    // pillows
    Raylib.DrawRectangle(1038, 460, 100, 38, new Color((byte)240, (byte)242, (byte)248, (byte)255));
    Raylib.DrawRectangle(1038, 460, 100, 4,  new Color((byte)255, (byte)255, (byte)255, (byte)255));
    Raylib.DrawRectangle(1210, 460, 100, 38, new Color((byte)240, (byte)242, (byte)248, (byte)255));
    Raylib.DrawRectangle(1210, 460, 100, 4,  new Color((byte)255, (byte)255, (byte)255, (byte)255));

    // side table — with lamp
    Raylib.DrawRectangle(948, 460, 74, 74, new Color((byte)160, (byte)128, (byte)88, (byte)255));
    Raylib.DrawRectangle(948, 460, 74, 5,  new Color((byte)190, (byte)158, (byte)115, (byte)255));
    Raylib.DrawRectangle(948, 460, 5, 74,  new Color((byte)190, (byte)158, (byte)115, (byte)255));
    // lamp on table
    Raylib.DrawRectangle(972, 440, 20, 22, new Color((byte)220, (byte)200, (byte)160, (byte)255)); // shade
    Raylib.DrawRectangle(978, 440, 8, 4,   new Color((byte)240, (byte)225, (byte)185, (byte)255)); // shade top
    Raylib.DrawRectangle(980, 462, 4, 8,   new Color((byte)180, (byte)160, (byte)120, (byte)255)); // stand
    // lamp glow
    Raylib.DrawRectangle(964, 444, 36, 18, new Color((byte)255, (byte)240, (byte)180, (byte)40));

    // wardrobe — double door with handles and grain
    Raylib.DrawRectangle(1028, 758, 304, 194, new Color((byte)90, (byte)55, (byte)30, (byte)255));  // shadow
    Raylib.DrawRectangle(1025, 755, 305, 195, new Color((byte)110, (byte)68, (byte)38, (byte)255)); // body
    Raylib.DrawRectangle(1025, 755, 305, 18,  new Color((byte)145, (byte)100, (byte)65, (byte)255)); // top highlight
    // door split line
    Raylib.DrawRectangle(1177, 755, 6, 195, new Color((byte)80, (byte)45, (byte)20, (byte)255));
    // wood grain
    for (int wgx = 1035; wgx < 1175; wgx += 18)
        Raylib.DrawRectangle(wgx, 755, 1, 195, new Color((byte)90, (byte)55, (byte)28, (byte)100));
    for (int wgx = 1188; wgx < 1328; wgx += 18)
        Raylib.DrawRectangle(wgx, 755, 1, 195, new Color((byte)90, (byte)55, (byte)28, (byte)100));
    // handles
    Raylib.DrawRectangle(1158, 840, 12, 30, new Color((byte)200, (byte)160, (byte)64, (byte)255));
    Raylib.DrawRectangle(1190, 840, 12, 30, new Color((byte)200, (byte)160, (byte)64, (byte)255));

    // chest — with lock detail
    Raylib.DrawRectangle(818, 868, 109, 94, new Color((byte)100, (byte)70, (byte)20, (byte)255));  // shadow
    Raylib.DrawRectangle(815, 865, 108, 92, new Color((byte)148, (byte)108, (byte)34, (byte)255)); // body
    Raylib.DrawRectangle(815, 865, 108, 28, new Color((byte)180, (byte)140, (byte)50, (byte)255)); // lid
    Raylib.DrawRectangle(815, 865, 108, 4,  new Color((byte)210, (byte)175, (byte)80, (byte)255)); // lid highlight
    Raylib.DrawRectangle(815, 891, 108, 2,  new Color((byte)100, (byte)70, (byte)20, (byte)200)); // lid shadow line
    // lock
    Raylib.DrawRectangle(853, 874, 28, 18, new Color((byte)190, (byte)150, (byte)58, (byte)255));
    Raylib.DrawRectangle(860, 872, 14, 6,  new Color((byte)160, (byte)120, (byte)40, (byte)255)); // shackle

    // ── ROOM DIVIDER WALLS ──
    Raylib.DrawRectangle(800, 0,   12, 150, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(800, 300, 12, 150, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    Raylib.DrawRectangle(800, 600, 12, 400, new Color((byte)160, (byte)180, (byte)180, (byte)255));
    // wall highlight
    Raylib.DrawRectangle(800, 0,   3, 150, new Color((byte)200, (byte)215, (byte)215, (byte)255));
    Raylib.DrawRectangle(800, 300, 3, 150, new Color((byte)200, (byte)215, (byte)215, (byte)255));
    Raylib.DrawRectangle(800, 600, 3, 400, new Color((byte)200, (byte)215, (byte)215, (byte)255));
}

else if (currentBuilding.BuildingName.StartsWith("PLAYER HOUSE") && ActiveHouseData != null)
{
    var furniture = ActiveHouseData.Furniture;

    // floor
    Color floorCol = ActiveHouseData.FloorColor switch {
        "Oak"    => new Color((byte)160,(byte)110,(byte)60,(byte)255),
        "Pine"   => new Color((byte)190,(byte)150,(byte)90,(byte)255),
        "Stone"  => new Color((byte)120,(byte)120,(byte)115,(byte)255),
        "Carpet" => new Color((byte)100,(byte)80,(byte)140,(byte)255),
        "Tile"   => new Color((byte)200,(byte)200,(byte)195,(byte)255),
        _        => new Color((byte)160,(byte)110,(byte)60,(byte)255)
    };
    for (int tx = 0; tx < 1280; tx += 48)
        for (int ty = 0; ty < 1000; ty += 48)
        {
            Raylib.DrawRectangle(tx, ty, 48, 48, floorCol);
            Raylib.DrawRectangleLines(tx, ty, 48, 48,
                new Color((byte)0,(byte)0,(byte)0,(byte)30));
        }

    // wall colour
    Color wallC = ActiveHouseData.WallColor switch {
        "White" => new Color((byte)240,(byte)240,(byte)235,(byte)255),
        "Blue"  => new Color((byte)80,(byte)120,(byte)180,(byte)255),
        "Green" => new Color((byte)80,(byte)150,(byte)80,(byte)255),
        "Red"   => new Color((byte)180,(byte)70,(byte)70,(byte)255),
        _       => new Color((byte)210,(byte)190,(byte)150,(byte)255)
    };
    Raylib.DrawRectangle(0, 0, 1280, 60, wallC);
    Raylib.DrawRectangle(0, 0, 10, 1000, wallC);
    Raylib.DrawRectangle(1270, 0, 10, 1000, wallC);

    // bed
    Raylib.DrawRectangle(1030, 500, 180, 160, new Color((byte)180,(byte)160,(byte)200,(byte)255));
    Raylib.DrawRectangle(1030, 500, 180, 40,  new Color((byte)80,(byte)60,(byte)40,(byte)255));
    // pillow
    Raylib.DrawRectangle(1038, 508, 70, 30, new Color((byte)220,(byte)210,(byte)230,(byte)255));
    Raylib.DrawRectangle(1118, 508, 70, 30, new Color((byte)220,(byte)210,(byte)230,(byte)255));
    Program.DrawTextUI("BED", 1090, 516, 16, Color.White);

    // chest
    Raylib.DrawRectangle(820, 870, 105, 90, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(820, 870, 105, 90, new Color((byte)80,(byte)50,(byte)10,(byte)255));
    Program.DrawTextUI("CHEST", 830, 900, 16, Color.Gold);

    // kitchen bench
    Raylib.DrawRectangle(60, 60, 500, 50, new Color((byte)180,(byte)170,(byte)140,(byte)255));
    Raylib.DrawRectangle(60, 60, 500, 6,  new Color((byte)140,(byte)130,(byte)100,(byte)255));

    // owned furniture
    foreach (var f in furniture)
    DrawFurniturePiece(f.Type, f.RoomX, f.RoomY);

    // ── FURNITURE PLACE GHOST ─────────────────────────────────────────────────
     if (furniturePlaceMode)
    {
        DrawFurniturePiece(heldFurnitureType, furnitureCursorX, furnitureCursorY);

        int ghostW = (heldFurnitureType == "Wall"     || heldFurnitureType == "HalfWall")  ? 120
                   : (heldFurnitureType == "WallV"    || heldFurnitureType == "HalfWallV") ? 16
                   : 80;
        int ghostH = (heldFurnitureType == "Wall"     || heldFurnitureType == "HalfWall")  ? 16
                   : (heldFurnitureType == "WallV"    || heldFurnitureType == "HalfWallV") ? 90
                   : 60;
        Raylib.DrawRectangleLines(furnitureCursorX, furnitureCursorY, ghostW, ghostH,
            new Color((byte)255,(byte)255,(byte)0,(byte)220));
        Raylib.DrawRectangle(0, 0, 1280, 36, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("Left Click / Space = Place   |   ESC = Cancel",
            20, 8, 20, Color.Yellow);
    }

    // hover highlight on furniture
    Vector2 mpos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
    foreach (var f in furniture)
    {
    if (Raylib.CheckCollisionPointRec(mpos, new Rectangle(f.RoomX, f.RoomY, 80, 60)))
        {
            Raylib.DrawRectangleLines(f.RoomX, f.RoomY, 80, 60, Color.Yellow);
            Program.DrawTextUI(Raylib.IsKeyDown(KeyboardKey.Delete)
                ? $"Right Click = Sell {f.Type} (${f.Cost/2})"
                : $"Right Click = Move {f.Type}",
                f.RoomX, f.RoomY - 20, 14, Color.Yellow);
        }
    }

    // entrance mat
    Raylib.DrawRectangle(540, 870, 200, 28, wallC);
    Raylib.DrawRectangleLines(540, 870, 200, 28, Color.Gold);
    Program.DrawTextUI("MY HOME", 572, 878, 16, Color.Gold);
}

// ── DOMINO'S INTERIOR ────────────────────────────────────────────────────
else if (currentBuilding.BuildingName == "DOMINO'S")
{
    // Floor — dark navy/red checkered
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)0,  (byte)60, (byte)130,(byte)255)
                : new Color((byte)20, (byte)20, (byte)20, (byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)0,(byte)40,(byte)100,(byte)30));
 
    // Counter
    Raylib.DrawRectangle(200, 80, 900, 50, new Color((byte)0,(byte)60,(byte)130,(byte)255));
    Raylib.DrawRectangle(200, 80, 900, 8,  new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle(200, 80, 8,   50, new Color((byte)0,(byte)45,(byte)105,(byte)255));
    Raylib.DrawRectangle(1092, 80, 8,  50, new Color((byte)0,(byte)45,(byte)105,(byte)255));
 
    // Menu board
    Raylib.DrawRectangle(150, 0, 1000, 75, new Color((byte)15,(byte)15,(byte)15,(byte)255));
    string[] domMenuItems = { "Pepperoni $12", "BBQ Chicken $13", "Veggie $11", "Garlic Bread $5", "Cheesy Bread $6", "Cola $3" };
    for (int m = 0; m < domMenuItems.Length; m++)
        Program.DrawTextUI(domMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)190,(byte)20,(byte)20,(byte)255));
 
    // Pizza oven (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)100,(byte)30,(byte)10,(byte)255));
    Program.DrawTextUI("OVEN", 1128, 52, 12, Color.Orange);
    for (int d = 0; d < 3; d++)
        Raylib.DrawCircle(1130 + d * 16, 78, 5, new Color((byte)200,(byte)80,(byte)10,(byte)255));
 
    // Tables with seats
    Color tableColor = new Color((byte)0,(byte)55,(byte)120,(byte)255);
    Color chairColor = new Color((byte)190,(byte)20,(byte)20,(byte)255);
    int[] tableX = { 150, 450, 750 };
    int[] tableY = { 400, 600 };
    foreach (int ty2 in tableY)
        foreach (int tx2 in tableX)
        {
            Raylib.DrawRectangle(tx2, ty2, 120, 70, tableColor);
            Raylib.DrawRectangleLines(tx2, ty2, 120, 70, new Color((byte)0,(byte)40,(byte)100,(byte)255));
            Raylib.DrawRectangle(tx2 + 10, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 10, ty2 + 72, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 + 72, 30, 18, chairColor);
            // pizza box on table
            Raylib.DrawRectangle(tx2 + 20, ty2 + 15, 35, 25, new Color((byte)210,(byte)160,(byte)50,(byte)200));
            Raylib.DrawRectangle(tx2 + 65, ty2 + 15, 35, 25, new Color((byte)210,(byte)160,(byte)50,(byte)200));
        }
 
    // Delivery bag rack (right side)
    Raylib.DrawRectangle(1120, 300, 260, 600, new Color((byte)0,(byte)50,(byte)120,(byte)80));
    Raylib.DrawRectangleLines(1120, 300, 260, 600, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Program.DrawTextUI("DELIVERY AREA", 1132, 320, 18, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    // delivery bags
    for (int db = 0; db < 3; db++)
    {
        Raylib.DrawRectangle(1140, 360 + db * 100, 60, 70, new Color((byte)0,(byte)60,(byte)140,(byte)255));
        Raylib.DrawRectangleLines(1140, 360 + db * 100, 60, 70, new Color((byte)190,(byte)20,(byte)20,(byte)255));
        Program.DrawTextUI("BAG", 1152, 385 + db * 100, 14, Color.White);
    }
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)0,(byte)60,(byte)130,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Program.DrawTextUI("DOMINO'S", 618, 882, 18, Color.White);
}
 
// ── KFC INTERIOR ──────────────────────────────────────────────────────────
else if (currentBuilding.BuildingName == "KFC")
{
    // Floor — red/cream checkered
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)220,(byte)210,(byte)185,(byte)255)
                : new Color((byte)175,(byte)20,(byte)20,(byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)180,(byte)20,(byte)20,(byte)25));
 
    // Counter
    Raylib.DrawRectangle(200, 80, 900, 50, new Color((byte)160,(byte)15,(byte)15,(byte)255));
    Raylib.DrawRectangle(200, 80, 900, 8,  new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Raylib.DrawRectangle(200, 80, 8,   50, new Color((byte)130,(byte)10,(byte)10,(byte)255));
    Raylib.DrawRectangle(1092, 80, 8,  50, new Color((byte)130,(byte)10,(byte)10,(byte)255));
 
    // Menu board
    Raylib.DrawRectangle(150, 0, 1000, 75, new Color((byte)20,(byte)10,(byte)5,(byte)255));
    string[] kfcMenuItems = { "Orig. Chicken $10", "Zinger Burger $9", "3pc Meal $14", "Coleslaw $4", "Popcorn Chkn $7", "Pepsi $3" };
    for (int m = 0; m < kfcMenuItems.Length; m++)
        Program.DrawTextUI(kfcMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)240,(byte)225,(byte)195,(byte)255));
 
    // Fry station (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)40,(byte)25,(byte)10,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)160,(byte)100,(byte)20,(byte)255));
    Program.DrawTextUI("FRYER", 1124, 52, 12, Color.Gold);
    for (int d = 0; d < 3; d++)
        Raylib.DrawCircle(1130 + d * 16, 78, 5, new Color((byte)200,(byte)150,(byte)20,(byte)255));
 
    // Tables with seats
    Color tableColor = new Color((byte)160,(byte)15,(byte)15,(byte)255);
    Color chairColor = new Color((byte)240,(byte)225,(byte)195,(byte)255);
    int[] tableX = { 150, 450, 750 };
    int[] tableY = { 400, 600 };
    foreach (int ty2 in tableY)
        foreach (int tx2 in tableX)
        {
            Raylib.DrawRectangle(tx2, ty2, 120, 70, tableColor);
            Raylib.DrawRectangleLines(tx2, ty2, 120, 70, new Color((byte)130,(byte)10,(byte)10,(byte)255));
            Raylib.DrawRectangle(tx2 + 10, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 10, ty2 + 72, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 + 72, 30, 18, chairColor);
            // chicken bucket on table
            Raylib.DrawRectangle(tx2 + 25, ty2 + 10, 28, 35, new Color((byte)200,(byte)60,(byte)10,(byte)220));
            Raylib.DrawRectangle(tx2 + 22, ty2 + 8,  34, 8,  new Color((byte)230,(byte)80,(byte)20,(byte)255));
            Raylib.DrawRectangle(tx2 + 68, ty2 + 10, 28, 35, new Color((byte)200,(byte)60,(byte)10,(byte)220));
            Raylib.DrawRectangle(tx2 + 65, ty2 + 8,  34, 8,  new Color((byte)230,(byte)80,(byte)20,(byte)255));
        }
 
    // Kitchen area
    Raylib.DrawRectangle(1120, 300, 260, 600, new Color((byte)160,(byte)15,(byte)15,(byte)60));
    Raylib.DrawRectangleLines(1120, 300, 260, 600, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Program.DrawTextUI("KITCHEN", 1158, 320, 20, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    // warming lamps
    Raylib.DrawRectangle(1140, 360, 200, 10, new Color((byte)60,(byte)30,(byte)10,(byte)255));
    for (int wl = 0; wl < 4; wl++)
        Raylib.DrawCircle(1155 + wl * 48, 365, 8, new Color((byte)255,(byte)150,(byte)30,(byte)200));
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)160,(byte)15,(byte)15,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Program.DrawTextUI("KFC", 672, 882, 18, new Color((byte)240,(byte)225,(byte)195,(byte)255));
}
 
// ── BURGER KING INTERIOR ──────────────────────────────────────────────────
else if (currentBuilding.BuildingName == "BURGER KING")
{
    // Floor — orange/red checkered
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? Color.White
                : Color.Black;
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)200,(byte)80,(byte)0,(byte)30));
 
    // Counter
    Raylib.DrawRectangle(200, 80, 900, 50, new Color((byte)185,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle(200, 80, 900, 8,  new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawRectangle(200, 80, 8,   50, new Color((byte)150,(byte)15,(byte)15,(byte)255));
    Raylib.DrawRectangle(1092, 80, 8,  50, new Color((byte)150,(byte)15,(byte)15,(byte)255));
 
    // Menu board
    Raylib.DrawRectangle(150, 0, 1000, 75, new Color((byte)18,(byte)10,(byte)0,(byte)255));
    string[] bkMenuItems = { "Whopper $11", "Chkn Royale $10", "Onion Rings $5", "Cheese Sticks $6", "BK Meal $15", "Milkshake $6" };
    for (int m = 0; m < bkMenuItems.Length; m++)
        Program.DrawTextUI(bkMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)255,(byte)180,(byte)0,(byte)255));
 
    // Flame grill (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)35,(byte)20,(byte)5,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)180,(byte)60,(byte)0,(byte)255));
    Program.DrawTextUI("GRILL", 1126, 52, 12, Color.Orange);
    // flame flickers
    for (int d = 0; d < 3; d++)
        Raylib.DrawCircle(1130 + d * 16, 78, 5, new Color((byte)255,(byte)120,(byte)0,(byte)255));
 
    // Tables with seats
    Color tableColor = new Color((byte)185,(byte)20,(byte)20,(byte)255);
    Color chairColor = new Color((byte)255,(byte)180,(byte)0,(byte)255);
    int[] tableX = { 150, 450, 750 };
    int[] tableY = { 400, 600 };
    foreach (int ty2 in tableY)
        foreach (int tx2 in tableX)
        {
            Raylib.DrawRectangle(tx2, ty2, 120, 70, tableColor);
            Raylib.DrawRectangleLines(tx2, ty2, 120, 70, new Color((byte)150,(byte)15,(byte)15,(byte)255));
            Raylib.DrawRectangle(tx2 + 10, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 10, ty2 + 72, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 + 72, 30, 18, chairColor);
            // burger wrapper on table
            Raylib.DrawRectangle(tx2 + 20, ty2 + 15, 35, 25, new Color((byte)230,(byte)170,(byte)30,(byte)200));
            Raylib.DrawRectangle(tx2 + 65, ty2 + 15, 35, 25, new Color((byte)230,(byte)170,(byte)30,(byte)200));
        }
 
    // Crown lounge area
    Raylib.DrawRectangle(1120, 300, 260, 600, new Color((byte)185,(byte)20,(byte)20,(byte)60));
    Raylib.DrawRectangleLines(1120, 300, 260, 600, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Program.DrawTextUI("CROWN LOUNGE", 1128, 320, 18, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    // lounge seats
    for (int ls = 0; ls < 2; ls++)
    {
        Raylib.DrawRectangle(1140, 380 + ls * 160, 200, 80, new Color((byte)160,(byte)15,(byte)15,(byte)255));
        Raylib.DrawRectangleLines(1140, 380 + ls * 160, 200, 80, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    }
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)185,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Program.DrawTextUI("BURGER KING", 590, 882, 18, new Color((byte)255,(byte)180,(byte)0,(byte)255));
}
 

if (currentBuilding.BuildingName == "McDONALD'S")
{
    // Floor — checkered yellow/red tiles
    for (int tx = 0; tx < 1400; tx += 60)
        for (int ty = 0; ty < 1000; ty += 60)
        {
            Color tileColor = ((tx / 60 + ty / 60) % 2 == 0)
                ? new Color((byte)255,(byte)220,(byte)0,(byte)255)
                : new Color((byte)220,(byte)30,(byte)30,(byte)255);
            Raylib.DrawRectangle(tx, ty, 60, 60, tileColor);
        }

    // Ambient overlay to soften tiles
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)255,(byte)200,(byte)0,(byte)40));

    // Counter
    Raylib.DrawRectangle(200, 80, 900, 50, new Color((byte)200,(byte)160,(byte)0,(byte)255));
    Raylib.DrawRectangle(200, 80, 900, 8, new Color((byte)230,(byte)190,(byte)20,(byte)255));
    Raylib.DrawRectangle(200, 80, 8, 50, new Color((byte)170,(byte)130,(byte)0,(byte)255));
    Raylib.DrawRectangle(1092, 80, 8, 50, new Color((byte)170,(byte)130,(byte)0,(byte)255));

    // Menu board behind counter
    Raylib.DrawRectangle(150, 0, 1000, 75, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    string[] menuItems = { "Big Mac $8", "McChicken $7", "Fries $4", "McFlurry $5", "Happy Meal $9", "Nuggets $6" };
    for (int m = 0; m < menuItems.Length; m++)
        Program.DrawTextUI(menuItems[m], 165 + m * 155, 22, 16, new Color((byte)255,(byte)220,(byte)0,(byte)255));

    // Drink machine
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)50,(byte)50,(byte)50,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Program.DrawTextUI("DRINKS", 1118, 50, 12, Color.Gold);
    for (int d = 0; d < 3; d++)
        Raylib.DrawCircle(1130 + d * 16, 78, 5, new Color((byte)30,(byte)30,(byte)30,(byte)255));

    // Tables with seats
    Color tableColor = new Color((byte)200,(byte)160,(byte)20,(byte)255);
    Color chairColor = new Color((byte)180,(byte)30,(byte)30,(byte)255);
    int[] tableX = { 150, 450, 750 };
    int[] tableY = { 400, 600 };
    foreach (int ty2 in tableY)
        foreach (int tx2 in tableX)
        {
            Raylib.DrawRectangle(tx2, ty2, 120, 70, tableColor);
            Raylib.DrawRectangleLines(tx2, ty2, 120, 70, new Color((byte)160,(byte)120,(byte)0,(byte)255));
            // chairs
            Raylib.DrawRectangle(tx2 + 10, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 - 22, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 10, ty2 + 72, 30, 18, chairColor);
            Raylib.DrawRectangle(tx2 + 80, ty2 + 72, 30, 18, chairColor);
            // tray on table
            Raylib.DrawRectangle(tx2 + 20, ty2 + 15, 35, 25, new Color((byte)180,(byte)140,(byte)10,(byte)200));
            Raylib.DrawRectangle(tx2 + 65, ty2 + 15, 35, 25, new Color((byte)180,(byte)140,(byte)10,(byte)200));
        }

    // Play area
    Raylib.DrawRectangle(1120, 300, 260, 600, new Color((byte)255,(byte)180,(byte)0,(byte)80));
    Raylib.DrawRectangleLines(1120, 300, 260, 600, new Color((byte)220,(byte)30,(byte)30,(byte)255));
    Program.DrawTextUI("PLAY AREA", 1140, 320, 20, new Color((byte)220,(byte)30,(byte)30,(byte)255));
    // slide
    Raylib.DrawRectangle(1150, 360, 30, 120, new Color((byte)255,(byte)100,(byte)0,(byte)255));
    Raylib.DrawRectangle(1200, 400, 120, 20, new Color((byte)0,(byte)180,(byte)0,(byte)255)); // platform

    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)255,(byte)200,(byte)0,(byte)255));
    Program.DrawTextUI("McDONALD'S", 602, 882, 18, new Color((byte)255,(byte)220,(byte)0,(byte)255));
}

if (currentBuilding.BuildingName == "SWIMMING COMPLEX")
{
    // Base floor — wet tiles
    for (int tx = 0; tx < 1400; tx += 50)
        for (int ty = 0; ty < 1000; ty += 50)
        {
            Color tc = ((tx / 50 + ty / 50) % 2 == 0)
                ? new Color((byte)180,(byte)210,(byte)230,(byte)255)
                : new Color((byte)160,(byte)195,(byte)220,(byte)255);
            Raylib.DrawRectangle(tx, ty, 50, 50, tc);
        }

    // ── LANE POOL ──
    Raylib.DrawRectangle(50, 100, 500, 340, new Color((byte)20,(byte)120,(byte)200,(byte)255));
    Raylib.DrawRectangleLines(50, 100, 500, 340, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    // lane ropes
    Color[] laneColors = { Color.Red, Color.White, Color.Red, Color.White };
    for (int ln = 0; ln < 4; ln++)
        Raylib.DrawRectangle(52, 152 + ln * 60, 496, 8,
            new Color(laneColors[ln].R, laneColors[ln].G, laneColors[ln].B, (byte)180));
    // lane numbers
    for (int ln = 1; ln <= 5; ln++)
        Program.DrawTextUI($"{ln}", 24, 118 + (ln - 1) * 60, 18, Color.DarkGray);
    // start blocks
    for (int ln = 0; ln < 5; ln++)
    {
        Raylib.DrawRectangle(50, 118 + ln * 60, 28, 26, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(53, 121 + ln * 60, 22, 8, new Color((byte)100,(byte)100,(byte)120,(byte)255));
    }
    // water shimmer
    for (int wx = 80; wx < 540; wx += 60)
        Raylib.DrawRectangle(wx, 108, 40, 4, new Color((byte)100,(byte)180,(byte)220,(byte)80));
    Program.DrawTextUI("LANE POOL", 220, 70, 24, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Program.DrawTextUI("Space = Start Swimming", 170, 450, 18, Color.DarkGray);

    // ── DIVING POOL ──
    Raylib.DrawRectangle(700, 100, 450, 420, new Color((byte)10,(byte)100,(byte)190,(byte)255));
    Raylib.DrawRectangleLines(700, 100, 450, 420, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    // depth markings
    Program.DrawTextUI("5m", 1110, 200, 16, new Color((byte)180,(byte)230,(byte)255,(byte)200));
    Program.DrawTextUI("10m", 1108, 400, 16, new Color((byte)180,(byte)230,(byte)255,(byte)200));
    // water shimmer deep pool
    for (int wx = 720; wx < 1140; wx += 80)
        Raylib.DrawRectangle(wx, 108, 50, 4, new Color((byte)80,(byte)160,(byte)220,(byte)80));
    // diving platform ladder
    Raylib.DrawRectangle(700, 120, 16, 200, new Color((byte)160,(byte)165,(byte)170,(byte)255));
    for (int rung = 0; rung < 8; rung++)
        Raylib.DrawRectangle(700, 130 + rung * 22, 16, 5, new Color((byte)130,(byte)135,(byte)140,(byte)255));
    // 5m board
    Raylib.DrawRectangle(716, 170, 100, 12, new Color((byte)200,(byte)205,(byte)210,(byte)255));
    Raylib.DrawRectangle(716, 170, 100, 4, new Color((byte)230,(byte)235,(byte)240,(byte)255));
    Program.DrawTextUI("5m", 760, 155, 14, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    // 10m board
    Raylib.DrawRectangle(716, 280, 110, 12, new Color((byte)200,(byte)205,(byte)210,(byte)255));
    Raylib.DrawRectangle(716, 280, 110, 4, new Color((byte)230,(byte)235,(byte)240,(byte)255));
    Program.DrawTextUI("10m", 760, 265, 14, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    Program.DrawTextUI("DIVING POOL", 850, 70, 24, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Program.DrawTextUI("E = Dive (time your jump!)", 760, 540, 18, Color.DarkGray);

    // lifeguard chairs
    Raylib.DrawRectangle(580, 130, 30, 50, new Color((byte)220,(byte)50,(byte)50,(byte)255));
    Raylib.DrawRectangle(575, 110, 40, 22, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Program.DrawTextUI("LIFEGUARD", 555, 90, 12, Color.Red);

    // spectator bench
    Raylib.DrawRectangle(100, 700, 1000, 30, new Color((byte)180,(byte)150,(byte)100,(byte)255));
    Raylib.DrawRectangle(100, 700, 1000, 5, new Color((byte)210,(byte)180,(byte)130,(byte)255));
    Program.DrawTextUI("SPECTATOR AREA", 480, 740, 18, Color.DarkGray);

    // towel hooks
    for (int h = 0; h < 6; h++)
    {
        Raylib.DrawRectangle(80 + h * 180, 800, 12, 30, new Color((byte)80,(byte)80,(byte)90,(byte)255));
        Raylib.DrawRectangle(78 + h * 180, 826, 16, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    }

    // entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Program.DrawTextUI("SWIMMING COMPLEX", 575, 882, 16, Color.White);

    // active swimming HUD
    if (swimmingActive && swimmingPoolType == "lane")
    {
        // swimmer animation in lane pool
        int swimX = (int)(80 + (swimLapTimer / swimLapDuration) * 440);
        Raylib.DrawCircle(swimX, 200, 10, Program.player.SkinColor);
        Raylib.DrawRectangle(swimX - 12, 208, 24, 16, Program.player.ShirtColor);
    }

    if (swimmingActive && swimmingPoolType == "diving")
    {
        if (!divingJumped)
        {
            // draw diver on board
            Raylib.DrawCircle(770, 160, 10, Program.player.SkinColor);
            Raylib.DrawRectangle(758, 168, 24, 16, Program.player.ShirtColor);
        }
        else
        {
            // diver falling
            int diveY = 180 + (int)(divingFallTimer * 200f);
            diveY = Math.Min(diveY, 490);
            Raylib.DrawCircle(800, diveY, 10, Program.player.SkinColor);
        }
    }
}

if (currentBuilding.BuildingName == "TENNIS COURT")
{
    // court surface
    Raylib.DrawRectangle((int)CourtLeft - 30, (int)CourtTop - 30,
        (int)(CourtRight - CourtLeft) + 60, (int)(CourtBottom - CourtTop) + 60,
        new Color((byte)30, (byte)110, (byte)40, (byte)255));
    Raylib.DrawRectangle((int)CourtLeft, (int)CourtTop,
        (int)(CourtRight - CourtLeft), (int)(CourtBottom - CourtTop),
        new Color((byte)40, (byte)140, (byte)55, (byte)255));

    // court lines
    Raylib.DrawRectangleLinesEx(new Rectangle(CourtLeft, CourtTop, CourtRight - CourtLeft, CourtBottom - CourtTop), 3, Color.White);
    // service boxes
    Raylib.DrawLine((int)CourtLeft, (int)((CourtTop + CourtBottom) / 2), (int)CourtRight, (int)((CourtTop + CourtBottom) / 2), Color.White);
    Raylib.DrawLine((int)(CourtLeft + 180), (int)CourtTop, (int)(CourtLeft + 180), (int)CourtBottom, Color.White);
    Raylib.DrawLine((int)(CourtRight - 180), (int)CourtTop, (int)(CourtRight - 180), (int)CourtBottom, Color.White);
    // net
    Raylib.DrawRectangle((int)CourtMidX - 3, (int)CourtTop - 10, 6, (int)(CourtBottom - CourtTop) + 20, new Color((byte)230, (byte)230, (byte)230, (byte)255));
    for (int ny = (int)CourtTop; ny < CourtBottom; ny += 12)
        Raylib.DrawRectangle((int)CourtMidX - 3, ny, 6, 6, new Color((byte)180,(byte)180,(byte)180,(byte)200));

    if (tennisActive && !tennisDifficultySelect)
    {
        // player figure (their colours)
        Raylib.DrawCircle((int)tennisPlayerPos.X, (int)tennisPlayerPos.Y - 12, 11, Program.player.SkinColor);
        Raylib.DrawRectangle((int)tennisPlayerPos.X - 12, (int)tennisPlayerPos.Y - 2, 24, 26, Program.player.ShirtColor);
        // racket — swings out when active
        if (tennisSwinging)
            Raylib.DrawCircle((int)tennisPlayerPos.X + 24, (int)tennisPlayerPos.Y, 10, new Color((byte)220,(byte)220,(byte)120,(byte)255));
        else
            Raylib.DrawCircle((int)tennisPlayerPos.X + 16, (int)tennisPlayerPos.Y - 6, 8, new Color((byte)200,(byte)200,(byte)100,(byte)255));

        // AI figure
        Raylib.DrawCircle((int)tennisAIPos.X, (int)tennisAIPos.Y - 12, 11, new Color((byte)230,(byte)200,(byte)170,(byte)255));
        Raylib.DrawRectangle((int)tennisAIPos.X - 12, (int)tennisAIPos.Y - 2, 24, 26, new Color((byte)200,(byte)60,(byte)60,(byte)255));

        // ball
        Raylib.DrawCircle((int)tennisBallPos.X, (int)tennisBallPos.Y, 8, new Color((byte)220,(byte)240,(byte)60,(byte)255));
        // landing target marker while in flight
        if (tennisBallInFlight)
            Raylib.DrawCircleLines((int)tennisBallTarget.X, (int)tennisBallTarget.Y, 12, new Color((byte)255,(byte)255,(byte)255,(byte)120));

        // scoreboard
        Raylib.DrawRectangle(440, 20, 400, 50, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI($"YOU {tennisPlayerScore}   -   {tennisAIScore} AI   (first to {tennisPointsToWin})", 460, 32, 24, Color.White);

        // serve power bar
        if (tennisServePhase == 2 && tennisPlayerServing)
        {
            Raylib.DrawRectangle(440, 640, 400, 20, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangle(440, 640, (int)(400 * tennisServeToss), 20, Color.Orange);
            Program.DrawTextUI("Release SPACE to serve!", 440, 666, 18, Color.LightGray);
        }
    }

    if (tennisDifficultySelect)
    {
        Raylib.DrawRectangle(390, 280, 500, 160, new Color((byte)0,(byte)0,(byte)0,(byte)220));
        Program.DrawTextUI("SELECT DIFFICULTY", 470, 300, 28, Color.Gold);
        Program.DrawTextUI("1 - Easy", 470, 345, 22, Color.White);
        Program.DrawTextUI("2 - Normal", 470, 375, 22, Color.White);
        Program.DrawTextUI("3 - Hard", 470, 405, 22, Color.White);
    }

    if (tennisMessageTimer > 0 || tennisMessage.Contains("GAME") || tennisMessage.Contains("Match"))
    {
        int mw = Program.MeasureTextUI(tennisMessage, 24);
        Program.DrawTextUI(tennisMessage, 640 - mw / 2, 90, 24, Color.Yellow);
    }

    Program.DrawTextUI("WASD / Arrows = Move  |  SPACE = Serve / Swing  |  Q = Quit", 400, 700, 18, Color.LightGray);
}

if (currentBuilding.BuildingName == "BASKETBALL COURT")
{
    // Court surface — hardwood
    for (int bx2 = 0; bx2 < 1400; bx2 += 60)
    {
        Color plank = (bx2 / 60 % 2 == 0)
            ? new Color((byte)200,(byte)140,(byte)60,(byte)255)
            : new Color((byte)185,(byte)125,(byte)50,(byte)255);
        Raylib.DrawRectangle(bx2, 0, 60, 1000, plank);
        Raylib.DrawRectangle(bx2, 0, 1, 1000, new Color((byte)160,(byte)100,(byte)30,(byte)100));
    }

    // Court lines
    Raylib.DrawRectangleLines(50, 80, 1300, 760, Color.White);
    // Centre circle
    Raylib.DrawCircleLines(700, 460, 80, Color.White);
    Raylib.DrawLine(700, 80, 700, 840, Color.White);
    // Key / paint areas
    Raylib.DrawRectangleLines(50, 260, 220, 400, Color.White);
    Raylib.DrawRectangleLines(1130, 260, 220, 400, Color.White);
    // Free throw circles
    Raylib.DrawCircleLines(160, 460, 80, Color.White);
    Raylib.DrawCircleLines(1240, 460, 80, Color.White);
    // 3-point arcs (approximate)
    for (int angle = 0; angle <= 180; angle += 5)
    {
        float rad = angle * MathF.PI / 180f;
        int ax = 160 + (int)(MathF.Cos(rad) * 260);
        int ay = 460 + (int)(MathF.Sin(rad) * 260);
        Raylib.DrawCircle(ax, ay, 3, Color.White);
    }
    for (int angle = 0; angle <= 180; angle += 5)
    {
        float rad = (180 + angle) * MathF.PI / 180f;
        int ax = 1240 + (int)(MathF.Cos(rad) * 260);
        int ay = 460 + (int)(MathF.Sin(rad) * 260);
        Raylib.DrawCircle(ax, ay, 3, Color.White);
    }

    // Backboard + hoop left
    Raylib.DrawRectangle(50, 340, 20, 240, new Color((byte)200,(byte)200,(byte)210,(byte)255));
    Raylib.DrawRectangle(62, 400, 80, 50, new Color((byte)220,(byte)220,(byte)230,(byte)255));
    Raylib.DrawRectangleLines(62, 400, 80, 50, Color.DarkGray);
    Raylib.DrawCircle(142, 425, 22, new Color((byte)220,(byte)80,(byte)20,(byte)255));
    Raylib.DrawCircleLines(142, 425, 22, new Color((byte)180,(byte)50,(byte)10,(byte)255));
    // Net left
    for (int ny = 0; ny < 5; ny++)
        Raylib.DrawLine(130 + ny * 6, 447, 128 + ny * 6, 480, new Color((byte)200,(byte)200,(byte)200,(byte)180));

    // Backboard + hoop right
    Raylib.DrawRectangle(1330, 340, 20, 240, new Color((byte)200,(byte)200,(byte)210,(byte)255));
    Raylib.DrawRectangle(1258, 400, 80, 50, new Color((byte)220,(byte)220,(byte)230,(byte)255));
    Raylib.DrawRectangleLines(1258, 400, 80, 50, Color.DarkGray);
    Raylib.DrawCircle(1258, 425, 22, new Color((byte)220,(byte)80,(byte)20,(byte)255));
    Raylib.DrawCircleLines(1258, 425, 22, new Color((byte)180,(byte)50,(byte)10,(byte)255));
    for (int ny = 0; ny < 5; ny++)
        Raylib.DrawLine(1246 + ny * 6, 447, 1244 + ny * 6, 480, new Color((byte)200,(byte)200,(byte)200,(byte)180));

    // Free throw line label
    Program.DrawTextUI("FREE THROW LINE", 280, 490, 16, new Color((byte)255,(byte)255,(byte)255,(byte)180));
    Raylib.DrawLine(270, 460, 510, 460, new Color((byte)255,(byte)200,(byte)0,(byte)200));

    // Score display
    Raylib.DrawRectangle(540, 0, 320, 70, new Color((byte)20,(byte)20,(byte)20,(byte)200));
    Program.DrawTextUI($"SCORE: {bbScore}", 600, 18, 30, Color.Gold);

    // Ball + shot animation
    if (basketballActive && bbShooting)
    {
        float t = bbShotTimer / 1.5f;
        int ballX = (int)(400 + t * (142 - 400));
        int arcY = (int)(500 - MathF.Sin(t * MathF.PI) * 300 * bbPower);
        Raylib.DrawCircle(ballX, arcY, 14, new Color((byte)220,(byte)80,(byte)20,(byte)255));
        Raylib.DrawCircleLines(ballX, arcY, 14, new Color((byte)180,(byte)50,(byte)10,(byte)255));
    }
    else
    {
        // Ball sitting at free throw
        Raylib.DrawCircle(400, 500, 14, new Color((byte)220,(byte)80,(byte)20,(byte)255));
        Raylib.DrawCircleLines(400, 500, 14, new Color((byte)180,(byte)50,(byte)10,(byte)255));
    }

    if (bbMessageTimer > 0)
        Program.DrawTextUI(bbMessage, 200, 700, 24, Color.Gold);

    Program.DrawTextUI(!basketballActive ? "Space = Shoot (near free throw)" : "SPACE = Lock Power, then SPACE = Lock Aim | Q = Quit",
        130, 870, 20, Color.LightGray);

    // entrance mat
    Raylib.DrawRectangle(560, 920, 280, 40, new Color((byte)160,(byte)80,(byte)10,(byte)255));
    Program.DrawTextUI("BASKETBALL COURT", 578, 932, 16, Color.White);
}

    if (currentBuilding.BuildingName == "GYM")
{
    // dumbbell rack - matches collision rect (180,180,140,60)
    Raylib.DrawRectangle(180, 180, 140, 60, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(185, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(215, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(245, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Program.DrawTextUI("DUMBBELLS", 180, 248, 16, Color.White);

    // bench press - matches collision rect (480,285,220,55)
    Raylib.DrawRectangle(480, 310, 220, 40, new Color((byte)80,(byte)40,(byte)10,(byte)255));  // bench seat
    Raylib.DrawRectangle(540, 270, 100, 20, new Color((byte)60,(byte)60,(byte)60,(byte)255)); // barbell bar
    Raylib.DrawRectangle(528, 258, 24, 38, new Color((byte)40,(byte)40,(byte)40,(byte)255)); // left plate
    Raylib.DrawRectangle(628, 258, 24, 38, new Color((byte)40,(byte)40,(byte)40,(byte)255)); // right plate
    Raylib.DrawRectangle(552, 248, 16, 20, new Color((byte)30,(byte)30,(byte)30,(byte)255)); // left upright
    Raylib.DrawRectangle(612, 248, 16, 20, new Color((byte)30,(byte)30,(byte)30,(byte)255)); // right upright
    Program.DrawTextUI("BENCH PRESS", 490, 358, 16, Color.White);

    // --- TREADMILLS ---
for (int t = 0; t < 3; t++)
{
    int tx = 180 + t * 200;
    int ty = 920;

    // base
    Raylib.DrawRectangle(tx, ty, 100, 60, new Color((byte)50,(byte)50,(byte)60,(byte)255));
    // belt
    Raylib.DrawRectangle(tx + 10, ty + 20, 80, 25, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(tx + 10, ty + 20, 80, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    // handles
    Raylib.DrawRectangle(tx + 8,  ty - 30, 8, 35, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    Raylib.DrawRectangle(tx + 84, ty - 30, 8, 35, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    Raylib.DrawRectangle(tx + 8,  ty - 30, 84, 8, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    // screen
    Raylib.DrawRectangle(tx + 30, ty - 28, 40, 22, new Color((byte)0,(byte)180,(byte)220,(byte)255));
    Program.DrawTextUI("RUN", tx + 38, ty - 22, 12, Color.White);
    Program.DrawTextUI($"TREADMILL {t + 1}", tx + 5, ty + 50, 11, Color.White);
}

// --- CYCLING MACHINES ---
for (int c = 0; c < 3; c++)
{
    int cx = 10;
    int cy = 200 + c * 200;

    // base frame
    Raylib.DrawRectangle(cx, cy + 30, 80, 20, new Color((byte)50,(byte)50,(byte)60,(byte)255));
    // seat post
    Raylib.DrawRectangle(cx + 50, cy + 10, 8, 25, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    // seat
    Raylib.DrawRectangle(cx + 40, cy + 8, 28, 8, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    // handlebar post
    Raylib.DrawRectangle(cx + 18, cy + 10, 8, 25, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    // handlebars
    Raylib.DrawRectangle(cx + 8, cy + 8, 28, 6, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    // wheel
    Raylib.DrawCircle(cx + 20, cy + 42, 16, new Color((byte)40,(byte)40,(byte)50,(byte)255));
    Raylib.DrawCircleLines(cx + 20, cy + 42, 16, new Color((byte)100,(byte)100,(byte)120,(byte)255));
    Raylib.DrawCircle(cx + 20, cy + 42, 4, new Color((byte)80,(byte)80,(byte)100,(byte)255));
    Program.DrawTextUI($"BIKE {c + 1}", cx + 18, cy + 52, 11, Color.White);
}

// --- YOGA MATS ---
string[] matColors = { "Purple", "Blue", "Green" };
Color[] matColorsActual = {
    new Color((byte)120,(byte)40,(byte)160,(byte)255),
    new Color((byte)30,(byte)80,(byte)180,(byte)255),
    new Color((byte)30,(byte)140,(byte)60,(byte)255)
};
for (int m = 0; m < 3; m++)
{
    int mx = 550;
    int my = 480 + m * 50;
    Raylib.DrawRectangle(mx, my, 70, 38, matColorsActual[m]);
    Raylib.DrawRectangleLines(mx, my, 70, 38, new Color((byte)255,(byte)255,(byte)255,(byte)60));
    // rolled end detail
    Raylib.DrawRectangle(mx, my, 10, 38, new Color(
        (byte)Math.Max(0, matColorsActual[m].R - 30),
        (byte)Math.Max(0, matColorsActual[m].G - 30),
        (byte)Math.Max(0, matColorsActual[m].B - 30),
        (byte)255));
    Program.DrawTextUI("YOGA", mx + 18, my + 12, 12, Color.White);
}

// Entrance mat
Raylib.DrawRectangle(936, 875, 200, 120, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(946, 885, 180, 100, new Color((byte)30,(byte)140,(byte)60,(byte)255));

// --- COUNTER ---
Raylib.DrawRectangle(700, 150, 200, 40, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(700, 150, 200, 8, new Color((byte)80,(byte)80,(byte)100,(byte)255));
Raylib.DrawRectangle(700, 150, 8, 40, new Color((byte)80,(byte)80,(byte)100,(byte)255));
Raylib.DrawRectangle(892, 150, 8, 40, new Color((byte)80,(byte)80,(byte)100,(byte)255));
// items on counter
Raylib.DrawRectangle(730, 136, 20, 14, new Color((byte)200,(byte)50,(byte)50,(byte)255));  // water bottle
Raylib.DrawRectangle(770, 138, 30, 12, new Color((byte)200,(byte)160,(byte)40,(byte)255)); // protein bar
Raylib.DrawRectangle(820, 136, 24, 14, new Color((byte)50,(byte)150,(byte)200,(byte)255)); // shaker
Program.DrawTextUI("COUNTER", 730, 162, 14, Color.White);

// --- TOILETS AND SHOWER AREA ---
// toilet 1
Raylib.DrawRectangle(1310, 208, 60, 75, new Color((byte)220,(byte)220,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 208, 60, 16, new Color((byte)180,(byte)180,(byte)200,(byte)255));
Raylib.DrawEllipse(1340, 260, 22, 16, new Color((byte)200,(byte)200,(byte)215,(byte)255));
Raylib.DrawEllipseLines(1340, 260, 22, 16, new Color((byte)150,(byte)150,(byte)170,(byte)255));
Program.DrawTextUI("WC", 1332, 212, 12, Color.DarkGray);

// divider
Raylib.DrawRectangle(1302, 198, 6, 90, new Color((byte)160,(byte)160,(byte)170,(byte)255));

// toilet 2
Raylib.DrawRectangle(1310, 348, 60, 75, new Color((byte)220,(byte)220,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 348, 60, 16, new Color((byte)180,(byte)180,(byte)200,(byte)255));
Raylib.DrawEllipse(1340, 400, 22, 16, new Color((byte)200,(byte)200,(byte)215,(byte)255));
Raylib.DrawEllipseLines(1340, 400, 22, 16, new Color((byte)150,(byte)150,(byte)170,(byte)255));
Program.DrawTextUI("WC", 1332, 352, 12, Color.DarkGray);

// shower
Raylib.DrawRectangle(1310, 488, 80, 80, new Color((byte)180,(byte)210,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 488, 80, 8, new Color((byte)140,(byte)170,(byte)200,(byte)255));
Raylib.DrawRectangle(1310, 488, 8, 80, new Color((byte)140,(byte)170,(byte)200,(byte)255));
Raylib.DrawRectangle(1360, 493, 8, 20, new Color((byte)150,(byte)150,(byte)160,(byte)255));
Raylib.DrawRectangle(1348, 513, 32, 6, new Color((byte)150,(byte)150,(byte)160,(byte)255));
Raylib.DrawCircle(1354, 530, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Raylib.DrawCircle(1364, 534, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Raylib.DrawCircle(1374, 530, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Program.DrawTextUI("SHOWER", 1314, 558, 12, Color.DarkGray);

Program.DrawTextUI("FACILITIES", 1312, 172, 14, Color.LightGray);
    
}
DrawCollectables($"Building:{currentBuilding.BuildingName}");
    currentBuilding.InteriorNPC.Draw();

  if (currentBuilding.BuildingName == "DBar")
{
    foreach (NPC npc in dbarTableNPCs)
    {
        npc.Draw();
        if (Vector2.Distance(player.Center, npc.Position) < 120)
            DrawSpeechBubble(npc.Position, npc.Dialogue, 
                new Color((byte)80,(byte)40,(byte)10,(byte)255));
    }

    if (dbarPokieNPC != null)
    {
        dbarPokieNPC.Draw();
        if (Vector2.Distance(player.Center, dbarPokieNPC.Position) < 120)
            DrawSpeechBubble(dbarPokieNPC.Position, dbarPokieNPC.Dialogue,
                new Color((byte)180,(byte)50,(byte)50,(byte)255));
    }
}
if (currentBuilding.BuildingName == "KiwiCuts")
{
    kiwiCutsBarber.Draw();
    if (Vector2.Distance(player.Center, kiwiCutsBarber.Position) < 120)
        DrawSpeechBubble(kiwiCutsBarber.Position, kiwiCutsBarber.Dialogue,
            new Color((byte)20,(byte)140,(byte)120,(byte)255));

    foreach (NPC npc in kiwiCutsWaitingNPCs)
    {
        npc.Draw();
        if (Vector2.Distance(player.Center, npc.Position) < 100)
            DrawSpeechBubble(npc.Position, npc.Dialogue,
                new Color((byte)80,(byte)100,(byte)80,(byte)255));
    }
}

if (!player.Hidden)
{
    if (MathF.Abs(playerElevation) > 1f)
        Raylib.DrawEllipse(
            (int)player.Center.X,
            (int)(player.Center.Y + 28 - playerElevation * 0.3f),
            22, 7, new Color((byte)0,(byte)0,(byte)0,(byte)60));
    var savedPosB = player.Position;
    player.Position = new Vector2(player.Position.X, player.Position.Y + playerElevation);
    player.Draw();
    player.Position = savedPosB;
}

// ── REMOTE PLAYERS INSIDE THIS BUILDING ──────────────────────────────────
if (currentBuilding != null)
{
    string mpSceneTag2 = $"Building:{currentBuilding.BuildingName}";
    foreach (var rp in remotePlayers) rp.DrawAt(mpSceneTag2);
}

    Raylib.EndMode2D();
     // HUD text outside BeginMode2D
    Program.DrawTextUI($"Player: {(int)player.Position.X}, {(int)player.Position.Y}", 20, 50, 24, Color.White);
    if (currentBuilding != null)
    {
        Program.DrawTextUI($"MyBuilding: {currentBuilding.BuildingName}", 20, 80, 18, Color.Yellow);
        foreach (var rp in remotePlayers)
            Program.DrawTextUI($"Remote {rp.Id}: scene='{rp.Scene}' active={rp.Active}", 20, 100 + rp.Id * 20, 18, Color.Yellow);
    }

 if (currentBuilding.BuildingName == "AA")
    {
    if (Vector2.Distance(player.Center, new Vector2(600, 120)) < 160)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("AA COUNTER", 20, 630, 28, new Color((byte)0,(byte)160,(byte)255,(byte)255));
        Program.DrawTextUI("E = Open Licence Menu", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) aaMenuOpen = !aaMenuOpen;
    }
    }

    if (currentBuilding.BuildingName == "PRISON"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 150)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    if (inPrison)
    {
        Program.DrawTextUI($"Time left: {(int)prisonSentenceTimer}s  |  Bail: ${playerDebt:0}", 20, 630, 24, Color.White);
        Program.DrawTextUI("E = Pay bail and go free", 20, 668, 22, Color.Gold);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && player.Money >= playerDebt)
        {
            player.Money -= (int)playerDebt;
            playerDebt = 0f;
            inPrison = false;
            player.Position = prisonReturnPos;
            ShowNotification("Bail paid. You're free to go.");
        }
    }
}

   if (currentBuilding.BuildingName == "HALLENSTEINS")
    {
    if (Vector2.Distance(player.Center, new Vector2(310, 135)) < 150)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("HALLENSTEINS", 20, 630, 30, new Color((byte)180,(byte)140,(byte)20,(byte)255));
        Program.DrawTextUI("E = Browse clothes", 20, 670, 24, Color.White);

        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            hallensteinShopOpen = !hallensteinShopOpen;
    }
    }

    if (currentBuilding.BuildingName == "HOBBIES STORE"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("HOBBIES STORE", 20, 630, 28, new Color((byte)220,(byte)60,(byte)60,(byte)255));
    Program.DrawTextUI("E = Browse trading cards", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        hobbiesShopOpen = !hobbiesShopOpen;
}

    if (currentBuilding.BuildingName == "JOB CENTRE"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("JOB CENTRE", 20, 630, 28, new Color((byte)120,(byte)170,(byte)255,(byte)255));
    Program.DrawTextUI("E = View job board", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) jobBoardOpen = !jobBoardOpen;
}

    if (currentBuilding.BuildingName == "RANGING SHOP"){
    if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("RANGING SHOP", 20, 630, 28, new Color((byte)200,(byte)150,(byte)40,(byte)255));
        Program.DrawTextUI("E = Browse bows, crossbows & ammo", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            rangingShopOpen = !rangingShopOpen;
    }
    }

    if (currentBuilding.BuildingName == "BOAT LICENCE OFFICE"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("BOAT LICENCE OFFICE", 20, 630, 28, new Color((byte)100,(byte)200,(byte)255,(byte)255));
        Program.DrawTextUI("E = View boat licence options", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            boatMenuOpen = !boatMenuOpen;
    }

if (currentBuilding.BuildingName == "MAGIC SHOP"){
    if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("MAGIC SHOP", 20, 630, 28, new Color((byte)160,(byte)80,(byte)255,(byte)255));
        Program.DrawTextUI("E = Browse staffs & Arcane Essence", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            magicShopOpen = !magicShopOpen;
    }
}

if (currentBuilding.BuildingName == "FARMING SHOP"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("FARMING SHOP", 20, 630, 28, new Color((byte)180,(byte)140,(byte)60,(byte)255));
    Program.DrawTextUI("E = Browse tools & seeds", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        farmingShopOpen = !farmingShopOpen;
}

if (currentBuilding.BuildingName == "ZOO"
    && Vector2.Distance(player.Center, new Vector2(1000, 90)) < 160)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("ZOO ENTRY — $20", 20, 636, 26, new Color((byte)210,(byte)130,(byte)50,(byte)255));
    Program.DrawTextUI("E = Pay entry (once per visit)", 20, 672, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && player.Money >= 20 && !zooPaid)
    { player.Money -= 20; zooPaid = true; player.AddFarmingXP(5); ShowNotification("Enjoy the zoo!"); }
}

if (currentBuilding.BuildingName == "BARN"
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 180)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        barnShopOpen = !barnShopOpen;
}

    if (currentBuilding.BuildingName == "LIBRARY")
    {
        Vector2 boothPos = new Vector2(640, 400);   // booth center, world space
        bool near = Vector2.Distance(player.Center, boothPos) < 120;
        if (near && !libraryPhotoMenuOpen)
        {
            Program.DrawTextUI("[E] Take ID Photo", ScreenWidth/2 - 100, ScreenHeight - 120, 20, Color.White);
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                libraryPhotoMenuOpen = true;
        }
        if (libraryPhotoMenuOpen)
            DrawLibraryPhotoMenu();
    }

     // prompts
     if (currentBuilding.BuildingName.StartsWith("PLAYER HOUSE"))
{
    Vector2 bedPos   = new Vector2(1120, 580);  
    Vector2 chestPos2 = new Vector2(872, 915);
    if (Vector2.Distance(player.Center, bedPos) < 180)
        Program.DrawTextUI("Space = Bed menu", 20, 600, 22, Color.LightGray);
    if (Vector2.Distance(player.Center, chestPos2) < 100)
        Program.DrawTextUI("Space = Open Chest", 20, 600, 22, Color.LightGray);

    Raylib.DrawRectangle(0, 670, 1280, 40, new Color((byte)0,(byte)0,(byte)0,(byte)140));
    Program.DrawTextUI("H = Customise House | Right click = Move furniture | Delete + right click = sell", 20, 678, 20, Color.Gold);

    if (bedMenuOpen)
        {
            int mx = ScreenWidth  / 2 - 200;
            int my = ScreenHeight / 2 - 100;
            Raylib.DrawRectangle(mx, my, 400, 210, new Color((byte)10,(byte)10,(byte)25,(byte)240));
            Raylib.DrawRectangleLines(mx, my, 400, 210, Color.Gold);
            Program.DrawTextUI("BED MENU", mx + 140, my + 16, 24, Color.Gold);

            bool saveHl  = bedMenuSelected == 0;
            bool sleepHl = bedMenuSelected == 1;

            Raylib.DrawRectangle(mx + 20, my + 60, 360, 44,
                saveHl ? new Color((byte)40,(byte)80,(byte)40,(byte)255)
                       : new Color((byte)20,(byte)40,(byte)20,(byte)255));
            Raylib.DrawRectangleLines(mx + 20, my + 60, 360, 44,
                saveHl ? Color.Gold : Color.Green);
            Program.DrawTextUI("Save Game", mx + 30, my + 72, 20,
                saveHl ? Color.Gold : Color.White);

            Raylib.DrawRectangle(mx + 20, my + 116, 360, 44,
                sleepHl ? new Color((byte)20,(byte)20,(byte)60,(byte)255)
                        : new Color((byte)10,(byte)10,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(mx + 20, my + 116, 360, 44,
                sleepHl ? Color.Gold : Color.SkyBlue);
            Program.DrawTextUI("Sleep", mx + 30, my + 128, 20,
                sleepHl ? Color.Gold : Color.White);

            Program.DrawTextUI("ESC = Close", mx + 140, my + 180, 16, Color.DarkGray);
        }
}

    if (shopMessageTimer > 0)
    {
        byte alpha = (byte)(255 * Math.Min(1f, shopMessageTimer));
        Program.DrawTextUI(shopMessage, 480, 560, 30, new Color((byte)255, (byte)215, (byte)0, alpha));
    }

    // Swimming minigame HUD
if (currentBuilding.BuildingName == "SWIMMING COMPLEX" && swimmingActive)
{
    if (swimmingPoolType == "lane")
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI($"SWIMMING LAPS — Lap {swimLapsCompleted + 1}", 20, 630, 28, Color.SkyBlue);
        int barW = 900;
        float prog = swimLapTimer / swimLapDuration;
        Raylib.DrawRectangle(190, 658, barW, 32, new Color((byte)20,(byte)60,(byte)120,(byte)255));
        Color fill = swimStrokeWindow > 0 ? new Color((byte)60,(byte)220,(byte)90,(byte)255)
                : swimStrokeWindow < 0 ? new Color((byte)220,(byte)70,(byte)60,(byte)255)
                : new Color((byte)30,(byte)160,(byte)220,(byte)255);
        Raylib.DrawRectangle(190, 658, (int)(barW * prog), 32, fill);
        Raylib.DrawRectangleLines(190, 658, barW, 32, Color.White);
        Program.DrawTextUI($"Speed: {(int)(swimSpeed * 100)}%  |  Alternate LEFT/RIGHT to swim!  |  Q = Stop", 190, 700, 18, Color.LightGray);
    }
    else if (swimmingPoolType == "diving")
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));

    int barW = 900, barX = 190, barY = 658, barH = 32;

    if (divingStage == 0)
    {
        Program.DrawTextUI("DIVING — Stage 1: SPACE to set JUMP POWER!", 20, 628, 22, Color.SkyBlue);
        Raylib.DrawRectangle(barX, barY, barW, barH, new Color((byte)20,(byte)40,(byte)80,(byte)255));
        Raylib.DrawRectangle(barX, barY, (int)(barW * divingPower), barH, new Color((byte)30,(byte)200,(byte)90,(byte)255));
        Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);
    }
    else if (divingStage == 1)
    {
        Program.DrawTextUI("Stage 2: SPACE to stop the SPIN in the green!", 20, 628, 22, Color.SkyBlue);
        Raylib.DrawRectangle(barX, barY, barW, barH, new Color((byte)20,(byte)40,(byte)80,(byte)255));
        int gw = (int)(barW * 0.22f);
        Raylib.DrawRectangle(barX + barW / 2 - gw / 2, barY, gw, barH, new Color((byte)0,(byte)200,(byte)0,(byte)200));
        int cx = barX + (int)(divingRotation * barW) - 5;
        Raylib.DrawRectangle(cx, barY - 6, 10, barH + 12, Color.White);
        Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);
    }
    else if (divingStage == 2)
    {
        Program.DrawTextUI("Stage 3: SPACE for a clean ENTRY at the centre!", 20, 628, 22, Color.SkyBlue);
        Raylib.DrawRectangle(barX, barY, barW, barH, new Color((byte)20,(byte)40,(byte)80,(byte)255));
        int sw = (int)(barW * 0.08f);
        Raylib.DrawRectangle(barX + barW / 2 - sw / 2, barY, sw, barH, new Color((byte)0,(byte)220,(byte)0,(byte)255));
        int cx = barX + (int)(divingEntry * barW) - 5;
        Raylib.DrawRectangle(cx, barY - 6, 10, barH + 12, Color.White);
        Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);
    }
    else
    {
        Program.DrawTextUI(divingResult, 20, 648, 26, divingScore >= 7 ? Color.Gold : Color.White);
    }
}
}

// Basketball power/aim HUD
if (currentBuilding.BuildingName == "BASKETBALL COURT" && basketballActive && !bbShooting)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    int barW = 900; int barX2 = 190;

    if (!bbPowerLocked)
    {
        Program.DrawTextUI("SHOT POWER — Hit SPACE when bar is in the sweet spot!", 20, 628, 22, Color.Gold);
        Raylib.DrawRectangle(barX2, 658, barW, 32, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // sweet spot (0.5–0.8)
        int sweetX = barX2 + (int)(0.5f * barW);
        int sweetW = (int)(0.3f * barW);
        Raylib.DrawRectangle(sweetX, 658, sweetW, 32, new Color((byte)0,(byte)200,(byte)0,(byte)255));
        // cursor
        int cursorX2 = barX2 + (int)(bbPower * barW) - 6;
        Raylib.DrawRectangle(cursorX2, 652, 12, 44, Color.White);
        Raylib.DrawRectangleLines(barX2, 658, barW, 32, Color.White);
        Program.DrawTextUI("SPACE = Lock Power", barX2, 698, 18, Color.LightGray);
    }
    else
    {
        Program.DrawTextUI("AIM — Hit SPACE when cursor is in the centre!", 20, 628, 22, Color.Gold);
        Raylib.DrawRectangle(barX2, 658, barW, 32, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // aim sweet spot (0.35–0.65)
        int aimSweetX = barX2 + (int)(0.35f * barW);
        int aimSweetW = (int)(0.3f * barW);
        Raylib.DrawRectangle(aimSweetX, 658, aimSweetW, 32, new Color((byte)0,(byte)200,(byte)0,(byte)255));
        int aimCursorX = barX2 + (int)(bbAimX * barW) - 6;
        Raylib.DrawRectangle(aimCursorX, 652, 12, 44, Color.White);
        Raylib.DrawRectangleLines(barX2, 658, barW, 32, Color.White);
        Program.DrawTextUI("SPACE = Shoot!", barX2, 698, 18, Color.LightGray);
    }
}

// McDonald's message
if (currentBuilding.BuildingName == "McDONALD'S" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Program.DrawTextUI(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "Dominos" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Program.DrawTextUI(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "KFC" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Program.DrawTextUI(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "BurgerKing" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Program.DrawTextUI(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}


    if (currentBuilding.BuildingName == "BANK")
{
    Vector2[] boothPositions = {
        new Vector2(250, 370),
        new Vector2(600, 370),
        new Vector2(950, 370)
    };

    string[] tellerNames = { "Teller 1", "Teller 2", "Teller 3" };

    for (int i = 0; i < boothPositions.Length; i++)
    {
        if (Vector2.Distance(player.Center, boothPositions[i]) < 130)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Program.DrawTextUI(tellerNames[i], 20, 630, 30, Color.Gold);
            Program.DrawTextUI("Z = Deposit $10 | X = Withdraw $10", 20, 670, 24, Color.White);
            break; // only show one at a time if somehow near two
        }
    }
}

    Program.DrawTextUI("Q = EXIT BUILDING", 20, 20, 28, Color.White);

    if (currentBuilding.BuildingName == "STORE" || currentBuilding.BuildingName == "BANK")
    {
        Raylib.DrawRectangle(ScreenWidth - 300, 0, 300, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
        Program.DrawTextUI($"Wallet: ${player.Money}", ScreenWidth - 280, 15, 26, Color.Gold);

        if (currentBuilding.BuildingName == "BANK")
        {
            Program.DrawTextUI($"Bank: ${player.BankBalance}", ScreenWidth - 280, 45, 26, Color.LightGray);
            Program.DrawTextUI($"Total: ${player.Money + player.BankBalance}", ScreenWidth - 280, 75, 22, Color.White);
        }
    }

        if (currentBuilding.BuildingName == "GYM")
{
    Vector2 dumbbellPos = new Vector2(250, 210);
    Vector2 benchPos    = new Vector2(590, 330);
    bool nearDumbbells  = Vector2.Distance(player.Center, dumbbellPos) < 120;
    bool nearBench      = Vector2.Distance(player.Center, benchPos) < 120;

    if (nearDumbbells || nearBench || strengthMinigameActive)
    {
        Raylib.DrawRectangle(0, 600, 1280, 120, new Color((byte)0,(byte)0,(byte)0,(byte)200));

        int barX = 190;
        int barY = 648;
        int barW = 900;
        int barH = 38;

        if (strengthMinigameActive && strengthMinigameType == "dumbbell")
        {
            float greenSize = 0.15f + (player.StrengthLevel / 10) * 0.02f;
            int greenW = (int)(barW * greenSize);

            Program.DrawTextUI($"DUMBBELLS — Rep {dbConsecutiveHits} | Str Lv {player.StrengthLevel}", 
                20, 608, 24, Color.Gold);

            // bar background
            Raylib.DrawRectangle(barX, barY, barW, barH, 
                new Color((byte)40,(byte)40,(byte)40,(byte)255));

            // green zones at each end
            Raylib.DrawRectangle(barX, barY, greenW, barH, 
                new Color((byte)0,(byte)200,(byte)0,(byte)255));
            Raylib.DrawRectangle(barX + barW - greenW, barY, greenW, barH, 
                new Color((byte)0,(byte)200,(byte)0,(byte)255));

            // moving cursor
            int blockW = 28;
            int blockX = barX + (int)(dbBarPos * (barW - blockW));
            Raylib.DrawRectangle(blockX, barY - 5, blockW, barH + 10, Color.White);
            Raylib.DrawRectangleLines(blockX, barY - 5, blockW, barH + 10, Color.Gold);

            // bar outline
            Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);

            Program.DrawTextUI("SPACE = Hit when cursor is in GREEN | Miss = end set", 
                190, 694, 18, Color.LightGray);
        }
        else if (strengthMinigameActive && strengthMinigameType == "barbell")
        {
            float greenSize = 0.12f + (player.StrengthLevel / 10) * 0.02f;
            int greenW = (int)(barW * greenSize);
            int greenX = barX + (int)((bbGreenPos - greenSize / 2f) * barW);
            greenX = Math.Clamp(greenX, barX, barX + barW - greenW);

            Program.DrawTextUI($"BENCH PRESS — Press {bbConsecutiveHits} | Str Lv {player.StrengthLevel}", 
                20, 608, 24, Color.Gold);

            // bar background
            Raylib.DrawRectangle(barX, barY, barW, barH, 
                new Color((byte)40,(byte)40,(byte)40,(byte)255));

            // single static green zone
            Raylib.DrawRectangle(greenX, barY, greenW, barH, 
                new Color((byte)0,(byte)200,(byte)0,(byte)255));

            // moving cursor
            int blockW = 28;
            int blockX = barX + (int)(bbBarPos * (barW - blockW));
            Raylib.DrawRectangle(blockX, barY - 5, blockW, barH + 10, Color.White);
            Raylib.DrawRectangleLines(blockX, barY - 5, blockW, barH + 10, Color.Gold);

            // bar outline
            Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);

            Program.DrawTextUI("SPACE = Hit when cursor is in GREEN | Miss = end set", 
                190, 694, 18, Color.LightGray);
        }
        else
        {
            // idle prompt
            if (nearDumbbells)
            {
                Program.DrawTextUI("DUMBBELLS", 20, 610, 28, Color.Gold);
                Program.DrawTextUI($"Space = Start set | Hit greens to keep going, miss to end | Str Lv {player.StrengthLevel}", 
                    20, 650, 22, Color.White);
            }
            else if (nearBench)
            {
                Program.DrawTextUI("BENCH PRESS", 20, 610, 28, Color.Gold);
                Program.DrawTextUI($"Space = Start set | Hit moving green target, miss to end | Str Lv {player.StrengthLevel}", 
                    20, 650, 22, Color.White);
            }
        }
    }
}

if (currentBuilding.BuildingName == "MY HOUSE")
        {
            Vector2 wardrobePos = new Vector2(1080, 810);
            Vector2 chestPos    = new Vector2(872, 915);
            Vector2 bedPos      = new Vector2(1180, 585);
            Vector2 kitchenSinkPos = new Vector2(1300, 355);   // matches the pedestal sink draw coords
            if (Vector2.Distance(player.Center, kitchenSinkPos) < 120
                && toolbarSlots[toolbarSelectedSlot] == "Watering Can"
                && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
            {
                toolbarWaterCharge[toolbarSelectedSlot] = WateringCanMaxUses;
                ShowNotification("Refilled Watering Can!");
            }
            if (Vector2.Distance(player.Center, kitchenSinkPos) < 120
            && toolbarSlots[toolbarSelectedSlot] == "Canteen (Dirty)"
            && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
            CookRawItemFromToolbar("Canteen (Dirty)");

            if (Vector2.Distance(player.Center, wardrobePos) < 130)
                Program.DrawTextUI("Space = Open Wardrobe", 20, 600, 22, Color.LightGray);
            else if (Vector2.Distance(player.Center, chestPos) < 100)
                Program.DrawTextUI("Space = Open Chest", 20, 600, 22, Color.LightGray);
           else if (Vector2.Distance(player.Center, bedPos) < 200)
            {
                Program.DrawTextUI("Space = Bed menu", 20, 600, 22, Color.LightGray);
                if (bedMenuOpen)
                {
                    int mx = ScreenWidth / 2 - 200;
                    int my = ScreenHeight / 2 - 100;
                    Raylib.DrawRectangle(mx, my, 400, 210, new Color((byte)10,(byte)10,(byte)25,(byte)240));
                    Raylib.DrawRectangleLines(mx, my, 400, 210, Color.Gold);
                    Program.DrawTextUI("BED MENU", mx + 140, my + 16, 24, Color.Gold);

                    // Save Game option
                    bool saveHl = bedMenuSelected == 0;
                    Raylib.DrawRectangle(mx + 20, my + 60, 360, 44,
                        saveHl ? new Color((byte)40,(byte)80,(byte)40,(byte)255)
                               : new Color((byte)20,(byte)40,(byte)20,(byte)255));
                    Raylib.DrawRectangleLines(mx + 20, my + 60, 360, 44,
                        saveHl ? Color.Gold : Color.Green);
                    Program.DrawTextUI("Save Game", mx + 30, my + 72, 20,
                        saveHl ? Color.Gold : Color.White);
                    if (saveHl)
                        Program.DrawTextUI("", mx + 8, my + 72, 20, Color.Gold);

                    // Sleep option
                    bool sleepHl = bedMenuSelected == 1;
                    Raylib.DrawRectangle(mx + 20, my + 116, 360, 44,
                        sleepHl ? new Color((byte)20,(byte)20,(byte)60,(byte)255)
                                : new Color((byte)10,(byte)10,(byte)40,(byte)255));
                    Raylib.DrawRectangleLines(mx + 20, my + 116, 360, 44,
                        sleepHl ? Color.Gold : Color.SkyBlue);
                    Program.DrawTextUI("Sleep  (recover + skip 6 hrs)", mx + 30, my + 128, 20,
                        sleepHl ? Color.Gold : Color.White);
                    if (sleepHl)
                        Program.DrawTextUI("", mx + 8, my + 128, 20, Color.Gold);

                }
            }

          if (Vector2.Distance(player.Center, new Vector2(260, 480)) < 100
            && !wardrobeOpen && !chestOpen && !cookingMenuOpen)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Program.DrawTextUI("SEQUENCE TABLE", 20, 630, 28, new Color((byte)80,(byte)180,(byte)255,(byte)255));
            Program.DrawTextUI("E = Play Sequence (2v2)", 20, 668, 22, Color.White);
        }  
    
        }

        if (currentBuilding.BuildingName == "DBar")
{
    Vector2[] pokiePositions = {
        new Vector2(810, 245),
        new Vector2(810, 365),
        new Vector2(910, 245)
    };

    Vector2[] poolTablePositions = {
        new Vector2(240, 330),
        new Vector2(540, 330)
    };

    foreach (Vector2 pokiePos in pokiePositions)
    {
        if (Vector2.Distance(player.Center, pokiePos) < 80)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Program.DrawTextUI("POKIE MACHINE", 20, 630, 30, Color.Gold);
            Program.DrawTextUI($"Space = Spin ($5) | Wallet: ${player.Money}", 20, 670, 24, Color.White);
        }
    }

    // occupied pokie machine 4
if (Vector2.Distance(player.Center, new Vector2(910, 365)) < 80)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("POKIE MACHINE", 20, 630, 30, Color.Gold);
    Program.DrawTextUI("This machine is taken! That bloke won't budge.", 20, 670, 24, Color.Orange);
}
// Darts board
if (Vector2.Distance(player.Center, new Vector2(1250, 80)) < 80)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("Dart board", 20, 630, 30, Color.Gold);
    Program.DrawTextUI("Press space to play darts", 20, 670, 24, Color.White);
}

    foreach (Vector2 tablePos in poolTablePositions)
    {
        if (Vector2.Distance(player.Center, tablePos) < 100)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Program.DrawTextUI("POOL TABLE", 20, 630, 30, Color.Gold);
            Program.DrawTextUI($"Space = Play a round ($2) | Wallet: ${player.Money}", 20, 670, 24, Color.White);
        }
    }
}

        if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
        {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
        Program.DrawTextUI(currentBuilding.BuildingName, 20, 630, 30, Color.Gold);
        Program.DrawTextUI(currentBuilding.InteriorNPC.Name + ": " + currentBuilding.InteriorNPC.Dialogue, 20, 670, 24, Color.White);

        if (currentBuilding.BuildingName == "HOSPITAL")
            Program.DrawTextUI("E = Restore Health ($20)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "WEAPONS")
        {
            int upgradeCost = player.CombatLevel * 50;
            Program.DrawTextUI($"E = Upgrade Weapon (${upgradeCost})", 20, 600, 22, Color.LightGray);
        }
        
        if (currentBuilding.BuildingName == "GAS STATION")
        Program.DrawTextUI("E = Pay for fuel", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "GYM")
        {
            int cost = player.CombatLevel * 25;
            Program.DrawTextUI($"E = Train (+5 Max HP) (${cost})", 20, 600, 22, Color.LightGray);
        }

        if (currentBuilding.BuildingName == "MARAE")
            Program.DrawTextUI("E = Rest and Restore Health (Free)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "POLICE STATION")
            Program.DrawTextUI("E = Call Sweep - Clear All Enemies ($50)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "STORE")
              Program.DrawTextUI("E = Open Shop", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "BANK")
        {
            Vector2[] boothPositions = {
                new Vector2(250, 370),
                new Vector2(600, 370),
                new Vector2(950, 370)
            };
            bool nearBooth = boothPositions.Any(bp =>
                Vector2.Distance(player.Center, bp) < 130);

            if (nearBooth)
                Program.DrawTextUI($"Z = Deposit $10 | X = Withdraw $10 | Balance: ${player.BankBalance}", 20, 600, 22, Color.LightGray);
        }

    }
    if (currentBuilding.BuildingName == "Airport" && airportMenuOpen)
{
    (string name, Vector2 dest, int price)[] flights = {
        ("Safe Zone",  new Vector2(400, 600),    50),
        ("Desert",     new Vector2(14000, 600),  80),
        ("Snow Zone",  new Vector2(-32000, 600), 80),
        ("Beach",      new Vector2(24000, 600),  70),
        ("Mountains",  new Vector2(-28000,-4000),90),
        ("Volcano",    new Vector2(30000,-4000), 100),
        ("Farm",       new Vector2(-1900,-9800), 60),
    };

    int px = ScreenWidth/2 - 280, py = 80;
    Raylib.DrawRectangle(px, py, 560, 520, new Color((byte)10,(byte)20,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 560, 520, new Color((byte)40,(byte)160,(byte)220,(byte)255));
    Program.DrawTextUI("BOOK A FLIGHT", px + 145, py + 12, 26, new Color((byte)40,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", px + 190, py + 46, 18, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < flights.Length; i++)
    {
        Rectangle btn = new Rectangle(px + 20, py + 76 + i * 56, 520, 48);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
        Raylib.DrawRectangleRec(btn, new Color((byte)20,(byte)40,(byte)60,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? new Color((byte)40,(byte)200,(byte)255,(byte)255) : new Color((byte)40,(byte)60,(byte)80,(byte)255));
        // plane icon
        Program.DrawTextUI("✈", px + 32, py + 89 + i * 56, 20, new Color((byte)40,(byte)200,(byte)255,(byte)255));
        Program.DrawTextUI(flights[i].name, px + 60, py + 90 + i * 56, 20, hover ? new Color((byte)40,(byte)200,(byte)255,(byte)255) : Color.White);
        Program.DrawTextUI($"${flights[i].price}", px + 460, py + 90 + i * 56, 20, Color.Gold);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (player.Money >= flights[i].price)
            {
                player.Money -= flights[i].price;
                airportMenuOpen = false;
                ChangeScene(SceneState.World);
                player.Position = flights[i].dest;
                shopMessage = $"Safe travels bro! Arrived at {flights[i].name}.";
                shopMessageTimer = 3f;
                lastZoneMusic = default;
                CheckZoneMusic();
            }
            else { shopMessage = "Not enough cash bro!"; shopMessageTimer = 1.5f; }
        }
    }
    Program.DrawTextUI("Q = Close", px + 228, py + 492, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) airportMenuOpen = false;
}

if (currentBuilding.BuildingName == "Casino")
{
// chip desk interaction
    if (Vector2.Distance(player.Center, new Vector2(700, 100)) < 150)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("CHIP COUNTER", 20, 630, 28, new Color((byte)255,(byte)180,(byte)20,(byte)255));
        Program.DrawTextUI($"Chips: {playerChips}  |  E = Buy/Cash Out", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) casinoChipMenuOpen = !casinoChipMenuOpen;
    }

    // blackjack interaction
    Vector2[] bjPos = { new Vector2(200, 360), new Vector2(500, 360) };
    foreach (var bp in bjPos)
    {
        if (Vector2.Distance(player.Center, bp) < 100)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Program.DrawTextUI("BLACKJACK TABLE", 20, 630, 28, new Color((byte)0,(byte)200,(byte)60,(byte)255));
            Program.DrawTextUI($"Chips: {playerChips}  |  E = Play ($10 min)", 20, 668, 22, Color.White);
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && !blackjackOpen) { blackjackOpen = true; BjStartRound(); }
        }
    }

    // roulette interaction
    if (Vector2.Distance(player.Center, new Vector2(910, 350)) < 120)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("ROULETTE TABLE", 20, 630, 28, new Color((byte)200,(byte)60,(byte)0,(byte)255));
        Program.DrawTextUI($"Chips: {playerChips}  |  E = Play", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && !rouletteOpen) rouletteOpen = true;
    }

    // euchre table interaction
    if (Vector2.Distance(player.Center, new Vector2(1180, 360)) < 120)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("EUCHRE TABLE", 20, 630, 28, new Color((byte)200,(byte)160,(byte)20,(byte)255));
        Program.DrawTextUI($"Playing Cards Lv {player.PlayingCardsLevel}  |  E = Play Euchre", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) OpenCardsHub(CardGameType.Euchre);
    }

    // 500 table interaction
    if (Vector2.Distance(player.Center, new Vector2(1180, 560)) < 120)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("500 TABLE", 20, 630, 28, new Color((byte)200,(byte)160,(byte)20,(byte)255));
        Program.DrawTextUI($"Playing Cards Lv {player.PlayingCardsLevel}  |  E = Play 500", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) OpenCardsHub(CardGameType.FiveHundred);
    }
}

if (currentBuilding.BuildingName == "Casino" && casinoChipMenuOpen)
{
    int px = ScreenWidth/2 - 220, py = 120;
    Raylib.DrawRectangle(px, py, 440, 320, new Color((byte)20,(byte)5,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(px, py, 440, 320, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("CHIP COUNTER", px + 120, py + 12, 26, new Color((byte)255,(byte)180,(byte)20,(byte)255));
    Program.DrawTextUI($"Cash: ${player.Money}   Chips: {playerChips}", px + 60, py + 48, 18, Color.LightGray);

    (string label, int chips, int cost)[] buyOptions = {
        ("10 chips",  10,  10),
        ("50 chips",  50,  50),
        ("100 chips", 100, 100),
        ("500 chips", 500, 500),
    };
    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < buyOptions.Length; i++)
    {
        Rectangle btn = new Rectangle(px + 20, py + 80 + i * 44, 180, 36);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
        Raylib.DrawRectangleRec(btn, new Color((byte)40,(byte)20,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 1, hover ? Color.Gold : new Color((byte)120,(byte)80,(byte)10,(byte)255));
        Program.DrawTextUI($"Buy {buyOptions[i].label}", px + 28, py + 91 + i * 44, 16, hover ? Color.Gold : Color.White);
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left) && player.Money >= buyOptions[i].cost)
        { player.Money -= buyOptions[i].cost; playerChips += buyOptions[i].chips; }
    }
    // cash out
    Rectangle cashBtn = new Rectangle(px + 220, py + 80, 200, 36);
    bool hCash = Raylib.CheckCollisionPointRec(mouse, cashBtn);
    Raylib.DrawRectangleRec(cashBtn, new Color((byte)40,(byte)60,(byte)10,(byte)255));
    Raylib.DrawRectangleLinesEx(cashBtn, 1, hCash ? Color.Gold : Color.Green);
    Program.DrawTextUI($"Cash Out ({playerChips} chips)", px + 228, py + 91, 14, hCash ? Color.Gold : Color.Green);
    if (hCash && Raylib.IsMouseButtonPressed(MouseButton.Left) && playerChips > 0)
    { player.Money += playerChips; playerChips = 0; }

    Program.DrawTextUI("Q = Close", px + 180, py + 290, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) casinoChipMenuOpen = false;
}

if (currentBuilding.BuildingName == "Casino" && blackjackOpen)
{
    int px = ScreenWidth/2 - 340, py = 50;
    Raylib.DrawRectangle(px, py, 680, 580, new Color((byte)0,(byte)60,(byte)15,(byte)245));
    Raylib.DrawRectangleLines(px, py, 680, 580, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("BLACKJACK", px + 255, py + 10, 30, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI($"Chips: {playerChips}  |  Bet: {bjBet}", px + 200, py + 50, 18, Color.LightGray);

    // dealer hand
    Program.DrawTextUI("DEALER:", px + 20, py + 90, 20, Color.LightGray);
    for (int i = 0; i < bjDealerHand.Count; i++)
    {
        bool hide = i == 1 && !bjDealerRevealed;
        int cx = px + 20 + i * 70, cy = py + 115;
        Color cardBg = hide ? new Color((byte)60,(byte)20,(byte)100,(byte)255) : Color.White;
        Raylib.DrawRectangle(cx, cy, 60, 90, cardBg);
        Raylib.DrawRectangleLines(cx, cy, 60, 90, Color.Black);
        if (!hide)
        {
            string cn = BjCardName(bjDealerHand[i]);
            bool red = bjDealerHand[i] / 13 == 1 || bjDealerHand[i] / 13 == 2;
            Program.DrawTextUI(cn, cx + 6, cy + 8, 20, red ? Color.Red : Color.Black);
        }
        else Program.DrawTextUI("?", cx + 20, cy + 30, 28, Color.LightGray);
    }
    if (bjDealerRevealed)
        Program.DrawTextUI($"= {BjHandValue(bjDealerHand)}", px + 20 + bjDealerHand.Count * 70 + 8, py + 148, 20, Color.White);

    // player hand
    Program.DrawTextUI("YOU:", px + 20, py + 230, 20, Color.LightGray);
    for (int i = 0; i < bjPlayerHand.Count; i++)
    {
        int cx = px + 20 + i * 70, cy = py + 255;
        Raylib.DrawRectangle(cx, cy, 60, 90, Color.White);
        Raylib.DrawRectangleLines(cx, cy, 60, 90, Color.Black);
        string cn = BjCardName(bjPlayerHand[i]);
        bool red = bjPlayerHand[i] / 13 == 1 || bjPlayerHand[i] / 13 == 2;
        Program.DrawTextUI(cn, cx + 6, cy + 8, 20, red ? Color.Red : Color.Black);
    }
    Program.DrawTextUI($"= {BjHandValue(bjPlayerHand)}", px + 20 + bjPlayerHand.Count * 70 + 8, py + 288, 20, Color.White);

    // message
    if (bjMessageTimer > 0)
    {
        bjMessageTimer -= Raylib.GetFrameTime();
        int mw = Program.MeasureTextUI(bjMessage, 24);
        Program.DrawTextUI(bjMessage, px + 340 - mw/2, py + 390, 24, Color.Gold);
    }

    // bet adjustment
    Program.DrawTextUI($"Bet: {bjBet}", px + 270, py + 430, 22, Color.White);
    Rectangle betDown = new Rectangle(px + 230, py + 428, 34, 26);
    Rectangle betUp   = new Rectangle(px + 416, py + 428, 34, 26);
    bool hBD = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), betDown);
    bool hBU = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), betUp);
    Raylib.DrawRectangleRec(betDown, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleRec(betUp,   new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Program.DrawTextUI("-", px + 242, py + 430, 22, hBD ? Color.Gold : Color.White);
    Program.DrawTextUI("+", px + 424, py + 430, 22, hBU ? Color.Gold : Color.White);
    if (hBD && Raylib.IsMouseButtonPressed(MouseButton.Left)) bjBet = Math.Max(10, bjBet - 10);
    if (hBU && Raylib.IsMouseButtonPressed(MouseButton.Left)) bjBet = Math.Min(playerChips, bjBet + 10);

    // action buttons
    Vector2 mouse = Raylib.GetMousePosition();
    string[] actions = bjRoundOver ? new[]{ "Deal Again", "Leave" } : new[]{ "Hit", "Stand", "Leave" };
    for (int i = 0; i < actions.Length; i++)
    {
        Rectangle ab = new Rectangle(px + 80 + i * 180, py + 470, 160, 44);
        bool hover = Raylib.CheckCollisionPointRec(mouse, ab);
        Raylib.DrawRectangleRec(ab, new Color((byte)0,(byte)80,(byte)20,(byte)255));
        Raylib.DrawRectangleLinesEx(ab, 2, hover ? Color.Gold : new Color((byte)200,(byte)160,(byte)20,(byte)255));
        int tw = Program.MeasureTextUI(actions[i], 20);
        Program.DrawTextUI(actions[i], (int)ab.X + 80 - tw/2, (int)ab.Y + 12, 20, hover ? Color.Gold : Color.White);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if      (actions[i] == "Hit")        BjHit();
            else if (actions[i] == "Stand")      BjStand();
            else if (actions[i] == "Deal Again") BjStartRound();
            else if (actions[i] == "Leave")      blackjackOpen = false;
        }
    }
}

if (currentBuilding.BuildingName == "Casino" && rouletteOpen)
{
    string[] betTypes  = { "Red","Black","Even","Odd","1-12","13-24","25-36" };
    int[] betPayouts   = { 2,    2,      2,     2,    3,      3,      3 };

    int px = ScreenWidth/2 - 360, py = 50;
    Raylib.DrawRectangle(px, py, 720, 580, new Color((byte)0,(byte)50,(byte)10,(byte)245));
    Raylib.DrawRectangleLines(px, py, 720, 580, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI("ROULETTE", px + 280, py + 10, 30, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI($"Chips: {playerChips}  |  Bet: {rouletteBet}", px + 210, py + 50, 18, Color.LightGray);

    // spinning wheel visual
    float spinAngle = rouletteSpinning ? rouletteSpinTimer * 300f : (rouletteResult >= 0 ? rouletteResult * 9.73f : 0f);
    int wx = px + 360, wy = py + 220;
    for (int seg = 0; seg < 37; seg++)
    {
        float a = seg * (MathF.PI * 2f / 37f) + spinAngle * MathF.PI / 180f;
        Color sc = seg == 0 ? Color.Green : (seg % 2 == 0 ? new Color((byte)180,(byte)10,(byte)10,(byte)255) : Color.Black);
        Raylib.DrawTriangle(
            new Vector2(wx, wy),
            new Vector2(wx + MathF.Cos(a) * 100, wy + MathF.Sin(a) * 100),
            new Vector2(wx + MathF.Cos(a + MathF.PI*2f/37f) * 100, wy + MathF.Sin(a + MathF.PI*2f/37f) * 100),
            sc);
    }
    Raylib.DrawCircleLines(wx, wy, 100, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Raylib.DrawCircle(wx, wy, 12, new Color((byte)220,(byte)200,(byte)160,(byte)255));
    if (rouletteResult >= 0 && !rouletteSpinning)
        Program.DrawTextUI($"{rouletteResult}", wx - 8, wy - 10, 20, Color.Black);

    // spin update
    if (rouletteSpinning)
    {
        rouletteSpinTimer += Raylib.GetFrameTime();
        if (rouletteSpinTimer >= 2.5f)
        {
            rouletteSpinning = false;
            rouletteResult = Raylib.GetRandomValue(0, 36);
            // evaluate bet
            bool win = rouletteBetType switch {
                "Red"   => rouletteResult != 0 && rouletteResult % 2 == 0,
                "Black" => rouletteResult != 0 && rouletteResult % 2 == 1,
                "Even"  => rouletteResult != 0 && rouletteResult % 2 == 0,
                "Odd"   => rouletteResult % 2 == 1,
                "1-12"  => rouletteResult >= 1  && rouletteResult <= 12,
                "13-24" => rouletteResult >= 13 && rouletteResult <= 24,
                "25-36" => rouletteResult >= 25 && rouletteResult <= 36,
                _ => false
            };
            int payout = betPayouts[Array.IndexOf(betTypes, rouletteBetType)];
            if (win) { playerChips += rouletteBet * payout; rouletteMessage = $"WIN! {rouletteResult} — +{rouletteBet * payout} chips!"; }
            else     {                                        rouletteMessage = $"Lose. {rouletteResult}. Better luck next time bro."; }
            rouletteMessageTimer = 3f;
            player.AddGamblingXP(5);
        }
    }

    // bet type selector
    Program.DrawTextUI("BET TYPE:", px + 20, py + 370, 18, Color.LightGray);
    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < betTypes.Length; i++)
    {
        Rectangle bt = new Rectangle(px + 20 + i * 100, py + 396, 92, 32);
        bool sel = rouletteBetType == betTypes[i];
        bool hover = Raylib.CheckCollisionPointRec(mouse, bt);
        Raylib.DrawRectangleRec(bt, sel ? new Color((byte)120,(byte)80,(byte)10,(byte)255) : new Color((byte)30,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangleLinesEx(bt, 1, sel ? Color.Gold : (hover ? Color.Gold : new Color((byte)80,(byte)80,(byte)80,(byte)255)));
        int tw = Program.MeasureTextUI(betTypes[i], 14);
        Program.DrawTextUI(betTypes[i], (int)bt.X + 46 - tw/2, (int)bt.Y + 9, 14, sel ? Color.Gold : Color.White);
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left)) rouletteBetType = betTypes[i];
    }

    // bet amount
    Program.DrawTextUI($"Bet: {rouletteBet}", px + 290, py + 446, 20, Color.White);
    Rectangle rBD = new Rectangle(px + 250, py + 444, 34, 26);
    Rectangle rBU = new Rectangle(px + 430, py + 444, 34, 26);
    bool rhBD = Raylib.CheckCollisionPointRec(mouse, rBD);
    bool rhBU = Raylib.CheckCollisionPointRec(mouse, rBU);
    Raylib.DrawRectangleRec(rBD, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleRec(rBU, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Program.DrawTextUI("-", px + 262, py + 446, 20, rhBD ? Color.Gold : Color.White);
    Program.DrawTextUI("+", px + 440, py + 446, 20, rhBU ? Color.Gold : Color.White);
    if (rhBD && Raylib.IsMouseButtonPressed(MouseButton.Left)) rouletteBet = Math.Max(5, rouletteBet - 5);
    if (rhBU && Raylib.IsMouseButtonPressed(MouseButton.Left)) rouletteBet = Math.Min(playerChips, rouletteBet + 5);

    // message
    if (rouletteMessageTimer > 0)
    {
        rouletteMessageTimer -= Raylib.GetFrameTime();
        int mw = Program.MeasureTextUI(rouletteMessage, 20);
        Program.DrawTextUI(rouletteMessage, px + 360 - mw/2, py + 340, 20, Color.Gold);
    }

    // spin + leave buttons
    Rectangle spinBtn  = new Rectangle(px + 160, py + 500, 200, 44);
    Rectangle leaveBtn = new Rectangle(px + 380, py + 500, 160, 44);
    bool hSpin  = Raylib.CheckCollisionPointRec(mouse, spinBtn);
    bool hLeave = Raylib.CheckCollisionPointRec(mouse, leaveBtn);

    Raylib.DrawRectangleRec(spinBtn,  new Color((byte)0,(byte)80,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(spinBtn, 2, hSpin ? Color.Gold : new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Program.DrawTextUI(rouletteSpinning ? "Spinning..." : "SPIN", px + 218, py + 514, 20, hSpin ? Color.Gold : Color.White);

    Raylib.DrawRectangleRec(leaveBtn, new Color((byte)60,(byte)10,(byte)10,(byte)255));
    Raylib.DrawRectangleLinesEx(leaveBtn, 2, hLeave ? Color.Red : Color.DarkGray);
    Program.DrawTextUI("Leave", px + 418, py + 514, 20, hLeave ? Color.Red : Color.White);

    if (hSpin && !rouletteSpinning && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (playerChips >= rouletteBet)
        {
            playerChips -= rouletteBet;
            rouletteSpinning = true;
            rouletteSpinTimer = 0f;
            rouletteResult = -1;
            rouletteMessage = "";
        }
        else { rouletteMessage = "Not enough chips bro!"; rouletteMessageTimer = 2f; }
    }
    if (hLeave && Raylib.IsMouseButtonPressed(MouseButton.Left)) rouletteOpen = false;
}

    if (currentBuilding.BuildingName == "DBar" && barMenuOpen)
{
    string[] drinks = { "Tui", "Waikato", "Speights", "Lion Red", "Woodstock" };

    int panelX = ScreenWidth / 2 - 250;
    int panelY = 100;

    Raylib.DrawRectangle(panelX, panelY, 500, 420, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 500, 420, Color.Gold);
    Program.DrawTextUI("DBAR", panelX + 200, panelY + 15, 32, Color.Gold);
    Program.DrawTextUI("What are you having bro?", panelX + 80, panelY + 55, 20, Color.LightGray);
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 170, panelY + 80, 20, Color.Gold);

    if (FriendDiscount(currentBuilding.BuildingName) > 0)
    Program.DrawTextUI($"Friend discount: -{(int)(FriendDiscount(currentBuilding.BuildingName)*100)}%", 20, 700, 18, Color.Green);

    if (player.DrunkLevel > 0)
    {
        string drunkText = player.DrunkLevel switch
        {
            1 => "Feeling good...",
            2 => "Getting loose...",
            3 => "Pretty munted...",
            _ => "Absolutely gone bro"
        };
        Program.DrawTextUI(drunkText, panelX + 150, panelY + 108, 18, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    }

    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < drinks.Length; i++)
    {
        Rectangle btn = new Rectangle(panelX + 40, panelY + 140 + i * 48, 420, 40);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
        int price = 5;

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Program.DrawTextUI(drinks[i], panelX + 60, panelY + 152 + i * 48, 22, hover ? Color.Gold : Color.White);
        Program.DrawTextUI("${price}", panelX + 380, panelY + 152 + i * 48, 22, Color.Gold);

       if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (Vector2.Distance(player.Center, barCounterPos) < 120)
    {
        int finalPrice = DiscountedPrice(price);
        if (player.Money >= finalPrice) { 
            player.Money -= finalPrice;
            player.DrunkLevel++;
            player.DrunkTimer = 1440f;
            shopMessage = $"Cheers! You cracked a {drinks[i]}.";
            shopMessageTimer = 2f;
        }
        else
        {
            shopMessage = "Not enough cash bro!";
            shopMessageTimer = 1.5f;
        }
    }
    else
    {
        shopMessage = "Get to the bar to order bro!";
        shopMessageTimer = 1.5f;
    }
}
    }

    Program.DrawTextUI("Q = Close", panelX + 190, panelY + 375, 20, Color.LightGray);
}

// ── FOOD MENU POPUP ────────────────────────────────────────────────
    if (dropZoneFoodMenuOpen)
    {
        Raylib.DrawRectangle(ScreenWidth / 2 - 180, 150, 360, 240, new Color((byte)0, (byte)0, (byte)0, (byte)230));
        Raylib.DrawRectangleLines(ScreenWidth / 2 - 180, 150, 360, 240, Color.Gold);
        Program.DrawTextUI("SNACK BAR", ScreenWidth / 2 - 70, 165, 26, Color.Gold);
        Program.DrawTextUI("1. Hot Dog  - $6  (+18 HP)", ScreenWidth / 2 - 160, 210, 20, Color.White);
        Program.DrawTextUI("2. Nachos   - $7  (+20 HP)", ScreenWidth / 2 - 160, 245, 20, Color.White);
        Program.DrawTextUI("3. Soda     - $4  (+12 HP)", ScreenWidth / 2 - 160, 280, 20, Color.White);
        Program.DrawTextUI("4. Slushie  - $5  (+14 HP)", ScreenWidth / 2 - 160, 315, 20, Color.White);
        Program.DrawTextUI("E = Close", ScreenWidth / 2 - 50, 355, 18, Color.Gray);
    }

if (currentBuilding.BuildingName == "KiwiCuts" && hairMenuOpen)
{
    int panelX = ScreenWidth / 2 - 300;
    int panelY = 60;
    int panelW = 600; int panelH = 520;

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)20,(byte)30,(byte)25,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color((byte)40,(byte)220,(byte)180,(byte)255));
    Program.DrawTextUI("KIWI CUTS", panelX + 200, panelY + 12, 30, new Color((byte)40,(byte)220,(byte)180,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 220, panelY + 46, 18, Color.Gold);

    // ── Tabs ──
    string[] tabs = { "Hairstyle", "Hair Colour", "Facial Hair", "Beard Colour" };
    for (int t = 0; t < tabs.Length; t++)
    {
        bool active = barberMenuTab == t;
        Rectangle tab = new Rectangle(panelX + 10 + t * 147, panelY + 70, 142, 34);
        Raylib.DrawRectangleRec(tab, active ? new Color((byte)40,(byte)220,(byte)180,(byte)255) : new Color((byte)30,(byte)45,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 1, Color.White);
        Program.DrawTextUI(tabs[t], (int)tab.X + 8, (int)tab.Y + 9, 15, active ? Color.Black : Color.White);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), tab) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            barberMenuTab = t;
    }

    Vector2 mouse = Raylib.GetMousePosition();

    // ── Tab 0: Hairstyle ──
    if (barberMenuTab == 0)
    {
        string[] haircuts = { "Mini fro", "High top", "Flat top", "Mohawk", "Bald" };
        int[]    prices   = { 8, 12, 15, 20, 5 };
        Program.DrawTextUI($"Current: {playerHairStyle}", panelX + 20, panelY + 112, 17, Color.LightGray);
        for (int i = 0; i < haircuts.Length; i++)
        {
            Rectangle btn = new Rectangle(panelX + 20, panelY + 136 + i * 50, 560, 42);
            bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
            Raylib.DrawRectangleRec(btn, new Color((byte)30,(byte)45,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(btn, 2, hover ? new Color((byte)40,(byte)220,(byte)180,(byte)255) : Color.White);
            Program.DrawTextUI(haircuts[i], panelX + 40, panelY + 149 + i * 50, 20, hover ? new Color((byte)40,(byte)220,(byte)180,(byte)255) : Color.White);
            Program.DrawTextUI($"${prices[i]}", panelX + 520, panelY + 149 + i * 50, 20, Color.Gold);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (player.Money >= prices[i]) { player.Money -= prices[i]; playerHairStyle = haircuts[i]; shopMessage = $"Sweet! Rocking the {haircuts[i]} now bro."; shopMessageTimer = 2f; }
                else { shopMessage = "Not enough cash bro!"; shopMessageTimer = 1.5f; }
            }
        }
    }

    // ── Tab 1: Hair Colour ──
    else if (barberMenuTab == 1)
    {
        (string name, Color col)[] hairColors = {
            ("Black",    new Color((byte)20, (byte)15, (byte)10, (byte)255)),
            ("Dark Brown",new Color((byte)60,(byte)35,(byte)15,(byte)255)),
            ("Brown",    new Color((byte)80, (byte)50, (byte)20, (byte)255)),
            ("Auburn",   new Color((byte)110,(byte)45,(byte)20,(byte)255)),
            ("Blonde",   new Color((byte)200,(byte)165,(byte)80,(byte)255)),
            ("Platinum", new Color((byte)230,(byte)225,(byte)210,(byte)255)),
            ("Ginger",   new Color((byte)185,(byte)80, (byte)25,(byte)255)),
            ("Grey",     new Color((byte)140,(byte)135,(byte)130,(byte)255)),
            ("Red",      new Color((byte)190,(byte)20, (byte)20,(byte)255)),
            ("Blue",     new Color((byte)30, (byte)60, (byte)180,(byte)255)),
            ("Purple",   new Color((byte)120,(byte)30,(byte)160,(byte)255)),
            ("Green",    new Color((byte)20, (byte)130,(byte)50, (byte)255)),
        };
        Program.DrawTextUI("Pick a hair colour — $3 each", panelX + 20, panelY + 112, 17, Color.LightGray);
        for (int i = 0; i < hairColors.Length; i++)
        {
            int cx = panelX + 20 + (i % 4) * 145;
            int cy = panelY + 140 + (i / 4) * 110;
            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(cx, cy, 130, 70));
            Raylib.DrawRectangle(cx, cy, 130, 70, hairColors[i].col);
            Raylib.DrawRectangleLinesEx(new Rectangle(cx, cy, 130, 70), hover ? 3 : 1, hover ? Color.Gold : Color.White);
            Program.DrawTextUI(hairColors[i].name, cx + 4, cy + 75, 14, Color.LightGray);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (player.Money >= 3) { player.Money -= 3; playerHairColor = hairColors[i].col; shopMessage = $"{hairColors[i].name} hair, choice bro!"; shopMessageTimer = 2f; }
                else { shopMessage = "Not enough cash bro!"; shopMessageTimer = 1.5f; }
            }
        }
    }

    // ── Tab 2: Facial Hair ──
    else if (barberMenuTab == 2)
    {
        string[] facialStyles = { "None", "Stubble", "Moustache", "Goatee", "Full Beard" };
        int[]    fPrices      = { 0, 5, 8, 10, 15 };
        Program.DrawTextUI($"Current: {playerFacialHair}", panelX + 20, panelY + 112, 17, Color.LightGray);
        for (int i = 0; i < facialStyles.Length; i++)
        {
            Rectangle btn = new Rectangle(panelX + 20, panelY + 136 + i * 50, 560, 42);
            bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
            Raylib.DrawRectangleRec(btn, new Color((byte)30,(byte)45,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(btn, 2, hover ? new Color((byte)40,(byte)220,(byte)180,(byte)255) : Color.White);
            Program.DrawTextUI(facialStyles[i], panelX + 40, panelY + 149 + i * 50, 20, hover ? new Color((byte)40,(byte)220,(byte)180,(byte)255) : Color.White);
            string priceLabel = fPrices[i] == 0 ? "Free" : $"${fPrices[i]}";
            Program.DrawTextUI(priceLabel, panelX + 520, panelY + 149 + i * 50, 20, Color.Gold);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (player.Money >= fPrices[i]) { player.Money -= fPrices[i]; playerFacialHair = facialStyles[i]; shopMessage = $"Lookin sharp with {facialStyles[i]}, bro!"; shopMessageTimer = 2f; }
                else { shopMessage = "Not enough cash bro!"; shopMessageTimer = 1.5f; }
            }
        }
    }

    // ── Tab 3: Beard Colour ──
    else if (barberMenuTab == 3)
    {
        (string name, Color col)[] beardColors = {
            ("Black",     new Color((byte)20, (byte)15, (byte)10, (byte)255)),
            ("Dark Brown",new Color((byte)60, (byte)35, (byte)15, (byte)255)),
            ("Brown",     new Color((byte)80, (byte)50, (byte)20, (byte)255)),
            ("Auburn",    new Color((byte)110,(byte)45, (byte)20, (byte)255)),
            ("Blonde",    new Color((byte)200,(byte)165,(byte)80, (byte)255)),
            ("Grey",      new Color((byte)140,(byte)135,(byte)130,(byte)255)),
            ("Ginger",    new Color((byte)185,(byte)80, (byte)25, (byte)255)),
            ("White",     new Color((byte)230,(byte)228,(byte)225,(byte)255)),
        };
        Program.DrawTextUI("Pick a beard colour — $3 each", panelX + 20, panelY + 112, 17, Color.LightGray);
        for (int i = 0; i < beardColors.Length; i++)
        {
            int cx = panelX + 20 + (i % 4) * 145;
            int cy = panelY + 140 + (i / 4) * 110;
            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(cx, cy, 130, 70));
            Raylib.DrawRectangle(cx, cy, 130, 70, beardColors[i].col);
            Raylib.DrawRectangleLinesEx(new Rectangle(cx, cy, 130, 70), hover ? 3 : 1, hover ? Color.Gold : Color.White);
            Program.DrawTextUI(beardColors[i].name, cx + 4, cy + 75, 14, Color.LightGray);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (player.Money >= 3) { player.Money -= 3; playerFacialHairColor = beardColors[i].col; shopMessage = $"{beardColors[i].name} beard, mint bro!"; shopMessageTimer = 2f; }
                else { shopMessage = "Not enough cash bro!"; shopMessageTimer = 1.5f; }
            }
        }
    }

    Program.DrawTextUI("Q = Close", panelX + 255, panelY + 488, 18, Color.LightGray);
}

// classroom entry block — CHANGED, add a proximity prompt alongside the existing E-key check
if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom == "")
{
    foreach (var (subject, doorPos) in classroomDoors)
    {
        if (Vector2.Distance(player.Center, doorPos) < 90)   // slightly wider than the 70 trigger radius, so the prompt appears just before it's usable
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Program.DrawTextUI($"E = Enter {subject} class", 20, 660, 26, Color.Gold);
        }

        if (Vector2.Distance(player.Center, doorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            StartTestTransition(() => {
                currentClassroom = subject;
                player.Position = new Vector2(700, 850);
            }, $"Starting {subject} class");
        }
    }
}

Vector2 gymDoorPos = new Vector2(970, 1650);
if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom == "")
{
    if (Vector2.Distance(player.Center, gymDoorPos) < 90)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("E = Enter Gym", 20, 660, 26, Color.Gold);
    }

    if (Vector2.Distance(player.Center, gymDoorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        var court = buildings.FirstOrDefault(b => b.BuildingName == "BASKETBALL COURT");
        if (court != null)
        {
            schoolReturnBuilding = currentBuilding;
            enteredCourtFromSchool = true;
            StartTestTransition(() => {
                currentBuilding = court;
                player.Position = court.EntryPosition;
            }, "Entering gym");
        }
    }
}


  if (currentBuilding.BuildingName == "SUPERMARKET")
{
   bool nearCashier = Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120;

if (nearCashier && (player.HasTrolley || player.HasBasket))
{
    var cartInv = player.HasTrolley ? trolleyInventory : basketInventory;
    int itemCount = cartInv.Count(s => s != null);
    int totalCost = GetTotalCartCost();
    int finalCost = DiscountedPrice(totalCost);

    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("CHECKOUT", 20, 630, 28, new Color((byte)80,(byte)200,(byte)80,(byte)255));

    if (itemCount == 0)
    {
        Program.DrawTextUI("Your cart is empty bro!", 20, 668, 22, Color.LightGray);
    }
    else
    {
        Program.DrawTextUI($"{itemCount} items — Total: ${totalCost}  |  E = Pay  |  Wallet: ${player.Money}", 20, 668, 22, Color.White);

        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            // inventory space check — 1 slot for the shopping bag
            // REMOVE inventory space check entirely or check if all 8 slots are full
            if (toolbarSlots.All(s => s != null))
            {
                shopMessage = "Your toolbar is full! Can't hold any items.";
                shopMessageTimer = 3f;
            }
            else if (player.Money < totalCost)
            {
                shopMessage = $"Not enough cash bro! Need ${totalCost}.";
                shopMessageTimer = 2f;
            }
            else
            {
                // record what was in the bag
                groceryBagContents.Clear();
                foreach (string s in cartInv)
                    if (s != null) groceryBagContents.Add(s);

                player.Money -= totalCost;
                playerGroceryBags++;

                // clear the cart
                for (int s = 0; s < cartInv.Count; s++) cartInv[s] = null;

                // Adds shopping bag
                int bagsFailed = 0;
                foreach (string gi in groceryBagContents)
                    if (!AddOneItemToToolbar(gi)) bagsFailed++;
                groceryBagContents.Clear();
                playerGroceryBags = 0; // no longer needed as bag

                string msg = bagsFailed > 0
                    ? $"Paid ${finalCost}. {bagsFailed} items couldn't fit in toolbar!"
                    : $"Paid ${finalCost}. Items added to toolbar!";
                shopMessage = msg;
                shopMessageTimer = 2.5f;

                shopMessage = $"Paid ${finalCost}. Shopping bag added to toolbar!";
                shopMessageTimer = 2.5f;
                groceryShopOpen = false;
            }
        }
    }
}
}

if (currentBuilding.BuildingName == "McDONALD'S" &&
    Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("McDONALD'S", 20, 630, 30, new Color((byte)255,(byte)220,(byte)0,(byte)255));
    Program.DrawTextUI("E = Open Menu | Food restores HP", 20, 670, 24, Color.White);
}

    DrawChestUI();
    DrawWardrobe();
    DrawShopUI();
    DrawAAMenu();
    DrawAATheoryTest();
    DrawSupermarketInventoryUI();
    DrawPlushieLog();
    DrawJobBoard();
    DrawHouseMenuUI();
    DrawMagicShopUI();
    DrawFarmingShopUI();
    DrawBarnShopUI();
    DrawBoatMenu();
    DrawBoatTheoryTest();       
    DrawHobbiesStoreUI();
    DrawRangingShopUI();
    DrawGroceryShopPanel();
    DrawMcDonaldsMenu();
    DrawKFCMenu();
    DrawDominosMenu();
    DrawBurgerKingMenu();
    DrawDealerUI();
    DrawHallensteinShop();
    DrawFridgeUI();
    DrawCupboardUI();
    DrawPlayerMenu();
    DrawCookingMenu();
    DrawCalendarHUD();
    DrawCashHUD();
    DrawInventoryUI();
    
}
    }
}
