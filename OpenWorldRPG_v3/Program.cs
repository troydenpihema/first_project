
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace OpenWorldRPG
{
    enum SceneState
    {
        MainMenu,
        World,
        Building
    }

    class Program
    {
        const int ScreenWidth = 1280;
        const int ScreenHeight = 720;

        static string levelUpMessage = "";
        static float levelUpTimer = 0f;
        static float timeOfDay = 0f; // 0 to 1, full day cycle
        static float daySpeed = 0.02f; // how fast the day progresses

        static SceneState currentScene = SceneState.MainMenu;

        static Camera2D camera = new Camera2D();

        static Player player = new Player(new Vector2(400, 400));

        static int dayOfWeek = 0; // 0-6, Monday to Sunday
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static float dayCounter = 0f; // tracks full day cycles
        static bool isFishing = false;
        static float fishingTimer = 0f;
        static float fishingDuration = 3f;
        static bool skillsOpen = false;
        static bool hoverWoodcutting = false;
        static bool hoverFishing = false;
        
        static bool shopOpen = false;
        static string shopMessage = "";
        static float shopMessageTimer = 0f;
        static int minimapSize = 200;
        static int minimapX = 20;
        static int minimapY = 20;
        static float minimapScale = 0.02f;
        static bool isRaining = false;
        static float rainTimer = 0f;
        static float rainInterval = 30f;
        static List<Vector2> raindrops = new();

        static List<TreeObject> trees = new();
        static List<Lake> lakes = new();
        static List<NPC> npcs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();

        static List<FloatingText> floatingTexts = new();

        static Building currentBuilding = null;
        static float shakeDuration = 0f;
        static float shakeMagnitude = 6f;
        static void TriggerShake(float duration) => shakeDuration = duration;
                public static void ShowLevelUp(string skill, int level)
                    {
                    levelUpMessage = $"{skill} LEVEL UP! {level}";
                    levelUpTimer = 2.5f;
                    }
        static string GetTimeString()
            {
                float totalHours = timeOfDay * 24f;
                int hours = (int)totalHours;
                int minutes = (int)((totalHours - hours) * 60f);
                string period = hours >= 12 ? "PM" : "AM";
                int displayHour = hours % 12;
                if (displayHour == 0) displayHour = 12;
                return $"{displayHour}:{minutes:D2} {period}";
            }
        static Color GetNightOverlay()
            {
                float night = MathF.Sin(timeOfDay * MathF.PI);
                byte alpha = (byte)(180 * (1f - night));
                return new Color((byte)0, (byte)0, (byte)30, alpha);
            }
            static void UpdateWeather(float dt)
{
    rainTimer += dt;

    if (rainTimer >= rainInterval)
    {
        rainTimer = 0f;
        isRaining = !isRaining;
        rainInterval = Raylib.GetRandomValue(20, 60);

        if (isRaining)
        {
            raindrops.Clear();
            for (int i = 0; i < 200; i++)
            {
                raindrops.Add(new Vector2(
                    Raylib.GetRandomValue(0, ScreenWidth),
                    Raylib.GetRandomValue(0, ScreenHeight)
                ));
            }
        }
    }

    if (isRaining)
    {
        for (int i = 0; i < raindrops.Count; i++)
        {
            Vector2 drop = raindrops[i];
            drop.Y += 400f * dt;
            drop.X += 50f * dt;
            if (drop.Y > ScreenHeight) drop.Y = 0;
            if (drop.X > ScreenWidth) drop.X = 0;
            raindrops[i] = drop;
        }
    }
}
        static void DrawWeather()
{
    if (!isRaining) return;

    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)20, (byte)40, (byte)60));

    foreach (Vector2 drop in raindrops)
    {
        Raylib.DrawLine(
            (int)drop.X, (int)drop.Y,
            (int)drop.X + 4, (int)drop.Y + 12,
            new Color((byte)150, (byte)200, (byte)255, (byte)180)
        );
    }
}

            static void UpdateSkillsUI()
{
    Vector2 mouse = Raylib.GetMousePosition();

    // SKILLS button bounds
    Rectangle skillsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 60, 140, 40);

    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (Raylib.CheckCollisionPointRec(mouse, skillsBtn))
            skillsOpen = !skillsOpen;
        else if (!Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 160, ScreenHeight - 200, 140, 145)))
            skillsOpen = false;
    }

    if (skillsOpen)
    {
        Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
        Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);

        hoverWoodcutting = Raylib.CheckCollisionPointRec(mouse, wcBtn);
        hoverFishing = Raylib.CheckCollisionPointRec(mouse, fishBtn);
    }
    else
    {
        hoverWoodcutting = false;
        hoverFishing = false;
    }
}
        static void DrawSkillsUI()
{
    // SKILLS button
    Rectangle skillsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 60, 140, 40);
    Raylib.DrawRectangleRec(skillsBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(skillsBtn, 2, skillsOpen ? Color.Gold : Color.White);
    Raylib.DrawText("SKILLS", ScreenWidth - 130, ScreenHeight - 48, 22, skillsOpen ? Color.Gold : Color.White);

    if (!skillsOpen) return;

    // Woodcutting button
    Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
    Color wcColor = hoverWoodcutting ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(wcBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(wcBtn, 2, wcColor);
    Raylib.DrawText($"WC Lv {player.WoodcuttingLevel}", ScreenWidth - 155, ScreenHeight - 118, 20, wcColor);

    // Woodcutting progress bar
    if (!hoverWoodcutting)
        {
            int wcRequired = player.WoodcuttingLevel * player.WoodcuttingLevel * 50;
            float wcProgress = (float)player.WoodcuttingXP / wcRequired;
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, (int)(140 * wcProgress), 8, Color.Green);
        }

    // Fishing button
    Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);
    Color fishColor = hoverFishing ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(fishBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(fishBtn, 2, fishColor);
    Raylib.DrawText($"Fish Lv {player.FishingLevel}", ScreenWidth - 155, ScreenHeight - 168, 20, fishColor);

    // Fishing progress bar
    if (!hoverFishing)
        {
            int fishRequired = player.FishingLevel * player.FishingLevel * 50;
            float fishProgress = (float)player.FishingXP / fishRequired;
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 143, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 143, (int)(140 * fishProgress), 8, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        }

    // XP tooltip on hover
    if (hoverWoodcutting)
    {
        int required = player.WoodcuttingLevel * player.WoodcuttingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 130, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Raylib.DrawText($"XP: {player.WoodcuttingXP}/{required}", ScreenWidth - 315, ScreenHeight - 118, 20, Color.LightGray);
    }

    if (hoverFishing)
    {
        int required = player.FishingLevel * player.FishingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 180, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Raylib.DrawText($"XP: {player.FishingXP}/{required}", ScreenWidth - 315, ScreenHeight - 168, 20, Color.LightGray);
    }
}
        static void DrawMinimap()
{
    // background
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, minimapSize, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Raylib.DrawRectangleLines(minimapX, minimapY, minimapSize, minimapSize, Color.White);

    int cx = minimapX + minimapSize / 2;
    int cy = minimapY + minimapSize / 2;

    // lakes
    foreach (Lake lake in lakes)
    {
        int lx = cx + (int)((lake.Position.X - player.Position.X) * minimapScale);
        int ly = cy + (int)((lake.Position.Y - player.Position.Y) * minimapScale);
        Raylib.DrawCircle(lx, ly, 6, new Color((byte)30, (byte)100, (byte)200, (byte)255));
    }

    // buildings
    foreach (Building building in buildings)
    {
        int bx = cx + (int)((building.Bounds.X - player.Position.X) * minimapScale);
        int by = cy + (int)((building.Bounds.Y - player.Position.Y) * minimapScale);
        Raylib.DrawRectangle(bx, by, 8, 8, Color.Yellow);
    }

    // npcs
    foreach (NPC npc in npcs)
    {
        int nx = cx + (int)((npc.Position.X - player.Position.X) * minimapScale);
        int ny = cy + (int)((npc.Position.Y - player.Position.Y) * minimapScale);
        Raylib.DrawCircle(nx, ny, 3, Color.Red);
    }

    // player dot
    Raylib.DrawCircle(cx, cy, 4, Color.White);

    Raylib.DrawText("MAP", minimapX + 80, minimapY + 185, 18, Color.LightGray);
}
        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG V3");
            Raylib.SetTargetFPS(60);

            GenerateWorld();

            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Zoom = 1f;

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                Update(dt);
                Draw();
            }

            Raylib.CloseWindow();
        }

        static void Update(float dt)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Tab))
            {
            player.InventoryOpen = !player.InventoryOpen;
            }
            switch(currentScene)
            {
                case SceneState.MainMenu:

                    if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                    {
                        currentScene = SceneState.World;
                    }

                    break;

                case SceneState.World:

                    if (shakeDuration > 0) shakeDuration -= dt;
                    if (levelUpTimer > 0) levelUpTimer -= dt;
                    timeOfDay += daySpeed * dt;
                    UpdateWeather(dt);

                    if (timeOfDay > 1f)
                    {
                        timeOfDay = 0f;
                        dayOfWeek = (dayOfWeek + 1) % 7;
                    }

                    for (int i = floatingTexts.Count - 1; i >= 0; i--)
                    {
                        var ft = floatingTexts[i];
                        ft.Timer -= dt;
                        ft.Position.Y -= 40f * dt;
                        floatingTexts[i] = ft;
                        if (ft.Timer <= 0) floatingTexts.RemoveAt(i);
}

                    player.Update(dt, buildings, trees);

                    foreach (Vehicle vehicle in vehicles)
                    {
                        vehicle.Update(dt);

                        if (Raylib.CheckCollisionRecs(player.Bounds, vehicle.Bounds))
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.F))
                            {
                                vehicle.Driving = !vehicle.Driving;
                                player.Hidden = vehicle.Driving;
                            }

                            if (vehicle.Driving)
                            {
                                player.Position = vehicle.Position;
                            }
                        }
                    }

                    foreach (NPC npc in npcs)
                    {
                        npc.Update(dt);
                    }

                    foreach (TreeObject tree in trees)
                    {
                        tree.Update(dt);

                        if (!tree.Chopped)
                        {
                            if (Vector2.Distance(player.Position, tree.Position) < 80)
                            {
                                if (Raylib.IsKeyPressed(KeyboardKey.Space))
                                {
                                    tree.Health--;

                                    if (tree.Health <= 0)
                                    {
                                        tree.Chopped = true;
                                        player.AddWoodcuttingXP(25);
                                        player.Logs += Raylib.GetRandomValue(1, 3);
                                        TriggerShake(0.15f);
                                        floatingTexts.Add(new FloatingText {   // 👈 add this
                                        Position = player.Position - new Vector2(0, 20),
                                        Text = "+25 WC XP",
                                        Timer = 1.2f,
                                        TextColor = Color.Yellow
                                    });
                                    }
                                }
                            }
                        }
                    }

                    foreach (Lake lake in lakes)
                   {
                    lake.Update(dt);

                    if (Vector2.Distance(player.Position, lake.Position) < 120)
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.R) && !isFishing)
                        {
                            isFishing = true;
                            fishingTimer = 0f;
                        }
                        }
                    }

                        if (isFishing)
                    {
                         fishingTimer += dt;

                        if (fishingTimer >= fishingDuration)
                    {
                        isFishing = false;
                        fishingTimer = 0f;
                        player.AddFishingXP(20);
                        player.Fish += 1;
                        floatingTexts.Add(new FloatingText {
                        Position = player.Position - new Vector2(0, 20),
                        Text = "+20 Fishing XP",
                        Timer = 1.2f,
                        TextColor = new Color((byte)0, (byte)206, (byte)209, (byte)255)
        });
    }
}

                    foreach (Building building in buildings)
                    {
                        if (Raylib.CheckCollisionRecs(player.Bounds, building.Bounds))
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.E))
                            {
                                currentBuilding = building;
                                currentScene = SceneState.Building;
                                player.Position = new Vector2(300, 500);
                            }
                        }
                    }

                    camera.Target = player.Position;

                    break;

                case SceneState.Building:

                    player.UpdateInterior(dt, currentBuilding.InteriorObjects);
                    if (shopMessageTimer > 0) shopMessageTimer -= dt;

if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
{
    if (currentBuilding.BuildingName == "STORE")
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            if (player.Logs > 0)
            {
                player.Logs--;
                player.Money += 5;
                shopMessage = "Sold 1 Log for $5!";
                shopMessageTimer = 1.5f;
            }
            else
            {
                shopMessage = "No logs to sell!";
                shopMessageTimer = 1.5f;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            if (player.Fish > 0)
            {
                player.Fish--;
                player.Money += 10;
                shopMessage = "Sold 1 Fish for $10!";
                shopMessageTimer = 1.5f;
            }
            else
            {
                shopMessage = "No fish to sell!";
                shopMessageTimer = 1.5f;
            }
        }
    }

    if (currentBuilding.BuildingName == "BANK")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        if (player.Money >= 10)
        {
            player.Money -= 10;
            player.BankBalance += 10;
            shopMessage = "Deposited $10!";
            shopMessageTimer = 1.5f;
        }
        else
        {
            shopMessage = "Not enough money!";
            shopMessageTimer = 1.5f;
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.F))
    {
        if (player.BankBalance >= 10)
        {
            player.BankBalance -= 10;
            player.Money += 10;
            shopMessage = "Withdrew $10!";
            shopMessageTimer = 1.5f;
        }
        else
        {
            shopMessage = "Not enough in bank!";
            shopMessageTimer = 1.5f;
        }
    }
}
}
                    if (Raylib.IsKeyPressed(KeyboardKey.Q))
                    {
                        currentScene = SceneState.World;
                        player.Position = currentBuilding.ExitPosition;
                    }

                    camera.Target = player.Position;

                    break;
            }
        }
        

        static void Draw()
        {
            Raylib.BeginDrawing();

            switch(currentScene)
            {
                case SceneState.MainMenu:
                    DrawMenu();
                    break;

                case SceneState.World:
                    DrawWorld();
                    break;

                case SceneState.Building:
                    DrawInterior();
                    break;
            }
            
            UpdateSkillsUI();
            Raylib.EndDrawing();
        }

        static void DrawMenu()
        {
             Raylib.ClearBackground(new Color(20, 20, 30, 255));

    // Subtle animated background pulse
    float pulse = MathF.Sin((float)Raylib.GetTime() * 1.5f) * 10f;

            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(10, 10, 20, 255));
    Raylib.DrawText("OPEN WORLD RPG", 310, 180, 64, Color.Gold);
    Raylib.DrawText("OPEN WORLD RPG", 312, 182, 64, new Color(255,200,0,80)); // shadow

    // Blinking "press enter"
    if ((int)(Raylib.GetTime() * 2) % 2 == 0)
        Raylib.DrawText("PRESS ENTER TO START", 390, 360, 34, Color.White);
        }

        static void DrawWorld()
        {
            Raylib.ClearBackground(new Color(100,180,100,255));

            if (shakeDuration > 0)
        {
        camera.Offset = new Vector2(
        ScreenWidth / 2 + Raylib.GetRandomValue(-1, 1) * shakeMagnitude,
        ScreenHeight / 2 + Raylib.GetRandomValue(-1, 1) * shakeMagnitude
            );
        }
        else
        {
            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
        }
            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(-5000, -5000, 10000, 10000, new Color(90,170,90,255));

            Raylib.DrawRectangle(-4000, 550, 8000, 180, Color.DarkGray);

            foreach (var ft in floatingTexts)
            {
            byte alpha = (byte)(255 * (ft.Timer / 1.2f));
            Raylib.DrawText(ft.Text, (int)ft.Position.X, (int)ft.Position.Y, 22,
                new Color(ft.TextColor.R, ft.TextColor.G, ft.TextColor.B, alpha));
            }

            foreach (Building building in buildings)
            {
                building.Draw();
            }

            foreach (TreeObject tree in trees)
            {
                tree.Draw();
            }

            foreach (Lake lake in lakes)
            {
                lake.Draw();
            }

            foreach (NPC npc in npcs)
            {
                npc.Draw();
            }

            foreach (Vehicle vehicle in vehicles)
            {
                vehicle.Draw();
            }

            player.Draw();

            Raylib.EndMode2D();

            Color overlay = GetNightOverlay();
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, overlay);
            DrawWeather();

            DrawHUD();
        }

        static void DrawInterior()
        {
            Raylib.ClearBackground(new Color(40,40,40,255));

            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(0,0,1400,1000,currentBuilding.InteriorColor);

            foreach (Rectangle obj in currentBuilding.InteriorObjects)
            {
                Raylib.DrawRectangleRec(obj, Color.DarkBrown);
            }
            currentBuilding.InteriorNPC.Draw();
            player.Draw();

            Raylib.EndMode2D();

            if (shopMessageTimer > 0)
                {
                    byte alpha = (byte)(255 * Math.Min(1f, shopMessageTimer));
                    Raylib.DrawText(shopMessage, 480, 560, 30, new Color((byte)255, (byte)215, (byte)0, alpha));
                }

            Raylib.DrawText("Q = EXIT BUILDING", 20, 20, 28, Color.White);
            if (currentBuilding.BuildingName == "STORE" || currentBuilding.BuildingName == "BANK")
                {
                    Raylib.DrawRectangle(ScreenWidth - 300, 0, 300, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
                    Raylib.DrawText($"Wallet: ${player.Money}", ScreenWidth - 280, 15, 26, Color.Gold);

    if (currentBuilding.BuildingName == "BANK")
        {
            Raylib.DrawText($"Bank: ${player.BankBalance}", ScreenWidth - 280, 45, 26, Color.LightGray);
            Raylib.DrawText($"Total: ${player.Money + player.BankBalance}", ScreenWidth - 280, 75, 22, Color.White);
        }
    }

            if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Raylib.DrawText(currentBuilding.BuildingName, 20, 630, 30, Color.Gold);
    Raylib.DrawText(currentBuilding.InteriorNPC.Name + ": " + currentBuilding.InteriorNPC.Dialogue, 20, 670, 24, Color.White);

    if (currentBuilding.BuildingName == "STORE")
    {
        Raylib.DrawText("E = Sell Log ($5) | F = Sell Fish ($10)", 20, 600, 22, Color.LightGray);
    }

    if (currentBuilding.BuildingName == "BANK")
    {
        Raylib.DrawText($"E = Deposit $10 | F = Withdraw $10", 20, 600, 22, Color.LightGray);
    }
}
        }

        static void DrawHUD()
        {
            DrawSkillsUI();

           Raylib.DrawRectangle(0, ScreenHeight - 34, ScreenWidth, 34, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Raylib.DrawText("SPACE = Chop Tree | R = Fish | TAB = Inventory | E = Enter Building | F = Drive Vehicle", 20, ScreenHeight - 28, 20, Color.White);
           
            if (player.InventoryOpen)
{
    Raylib.DrawRectangle(
        420,
        120,
        440,
        420,
        new Color(0,0,0,220)
    );

    Raylib.DrawText(
        "INVENTORY",
        540,
        150,
        40,
        Color.Gold
    );

    Raylib.DrawText(
        $"Logs: {player.Logs}",
        500,
        240,
        32,
        Color.White
    );

    Raylib.DrawText(
        $"Fish: {player.Fish}",
        500,
        290,
        32,
        Color.White
    );

    Raylib.DrawText(
        $"Money: ${player.Money}",
        500,
        340,
        32,
        Color.White
    );

    Raylib.DrawText(
        "TAB = Close Inventory",
        470,
        470,
        24,
        Color.LightGray
    );
}
   if (levelUpTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, levelUpTimer));
    Raylib.DrawText(levelUpMessage, 380, 280, 40, new Color((byte)255, (byte)215, (byte)0, alpha));
}   

    if (isFishing)
{
    Raylib.DrawRectangle(400, 650, 480, 36, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Raylib.DrawRectangle(400, 650, (int)(480 * (fishingTimer / fishingDuration)), 36, new Color((byte)0, (byte)206, (byte)209, (byte)255));
    Raylib.DrawText("Fishing...", 580, 655, 24, Color.White);
}

        // Day/night HUD box top right
            Raylib.DrawRectangle(ScreenWidth - 280, 0, 280, 80, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Raylib.DrawText(dayNames[dayOfWeek], ScreenWidth - 260, 12, 28, Color.Gold);
            Raylib.DrawText(GetTimeString(), ScreenWidth - 260, 45, 26, Color.White);  
            DrawMinimap();
        }

        static void GenerateWorld()
        {
            for (int i = -2500; i < 2500; i += 250)
            {
                trees.Add(new TreeObject(new Vector2(i, -300)));
                trees.Add(new TreeObject(new Vector2(i, 1200)));
            }

            lakes.Add(new Lake(new Vector2(700, 1200)));
            lakes.Add(new Lake(new Vector2(-900, -600)));

        buildings.Add(new Building(
            new Rectangle(900, 400, 280, 240),
            new Color(180,120,90,255),
            new Color(90,70,50,255),
            new Vector2(1100,700),
            "BANK",
            new NPC(
            new Vector2(700,450),
            "Bank Manager",
            "Chur maori. Welcome to Waikato Bank."
    )
));

        buildings.Add(new Building(
            new Rectangle(1700, 300, 320, 280),
            Color.DarkBlue,
            new Color(50,60,90,255),
            new Vector2(1800,650),
            "DBar",
            new NPC(
            new Vector2(600,420),
            "Dbar Owner",
            "Grab a woodys and relax at Dbar."
    )
));

           buildings.Add(new Building(
            new Rectangle(-1200, 250, 300, 240),
            Color.DarkGreen,
            new Color(40,90,50,255),
            new Vector2(-1050,600),
            "STORE",
            new NPC(
            new Vector2(500,420),
            "Store Clerk",
            "Need supplies for fishing? Show me the moolack"
    )
));

            npcs.Add(new NPC(
                new Vector2(500,500),
                    "Local Resident",
                    "Nice weather today."
            ));

            npcs.Add(new NPC(
                new Vector2(1400,900),
                    "Fisherman",
                    "The lakes nearby have good fishing."
));

            vehicles.Add(new Vehicle(new Vector2(300,800), Color.Red, 650));
            vehicles.Add(new Vehicle(new Vector2(1200,700), Color.Yellow, 900));
            vehicles.Add(new Vehicle(new Vector2(-400,650), Color.DarkBlue, 500));
        }

    }
    struct FloatingText
    {
    public Vector2 Position;
    public string Text;
    public float Timer;
    public Color TextColor;
    }
    class Player
    {
        public Vector2 Position;

        public bool Hidden = false;

        float speed = 300;

        public int WoodcuttingLevel = 1;
        public int FishingLevel = 1;

        public int WoodcuttingXP = 0;
        public int FishingXP = 0;

        public int Logs = 0;
        public int Fish = 0;
        public int Money = 0;
        public int BankBalance = 0;

        public bool InventoryOpen = false;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 position)
        {
            Position = position;
        }

        public void Update(float dt, List<Building> buildings, List<TreeObject> trees)
        {
            Vector2 move = GetInput();

            Vector2 oldPos = Position;

            Position += move * speed * dt;

            foreach (Building building in buildings)
{
    Rectangle collisionBox = new Rectangle(
        building.Bounds.X,
        building.Bounds.Y,
        building.Bounds.Width,
        building.Bounds.Height - 90
    );

    if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
    {
        Position = oldPos;
    }
}

            foreach (TreeObject tree in trees)
            {
                if (!tree.Chopped)
                {
                    if (Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
                    {
                        Position = oldPos;
                    }
                }
            }
        }

        public void UpdateInterior(float dt, List<Rectangle> objects)
        {
            Vector2 move = GetInput();

            Vector2 oldPos = Position;

            Position += move * speed * dt;

            foreach (Rectangle rect in objects)
            {
                if (Raylib.CheckCollisionRecs(Bounds, rect))
                {
                    Position = oldPos;
                }
            }
        }

        Vector2 GetInput()
        {
            Vector2 move = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))
                move.Y -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
                move.Y += 1;

            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
                move.X -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
                move.X += 1;

            if (move != Vector2.Zero)
                move = Vector2.Normalize(move);

            return move;
        }

        public void AddWoodcuttingXP(int xp)
        {
            if (WoodcuttingLevel >= 100) return;

            WoodcuttingXP += xp;

            int requiredXP = WoodcuttingLevel * WoodcuttingLevel * 50;

            if (WoodcuttingXP >= requiredXP)
            {
                WoodcuttingXP = 0;
                WoodcuttingLevel++;
                Program.ShowLevelUp("Woodcutting", WoodcuttingLevel);
            }
        }

        public void AddFishingXP(int xp)
        {
            if (FishingLevel >= 100) return;

            FishingXP += xp;

            int requiredXP = FishingLevel * FishingLevel * 50;

            if (FishingXP >= requiredXP)
            {
                FishingXP = 0;
                FishingLevel++;
                Program.ShowLevelUp("Fishing", FishingLevel);
            }
        }

        public void Draw()
        {
            if (Hidden) return;

            Raylib.DrawCircle((int)Position.X + 20,(int)Position.Y + 12,12,Color.Beige);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 24,20,30,Color.Blue);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 54,8,12,Color.Black);
            Raylib.DrawRectangle((int)Position.X + 22,(int)Position.Y + 54,8,12,Color.Black);

            Raylib.DrawRectangle((int)Position.X + 2,(int)Position.Y + 26,8,18,Color.Beige);
            Raylib.DrawRectangle((int)Position.X + 30,(int)Position.Y + 26,8,18,Color.Beige);
        }
    }

    class TreeObject
{
    public Vector2 Position;

    public bool Chopped = false;

    public int Health = 3;

    float respawnTimer = 0f;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 80);

    public TreeObject(Vector2 pos)
    {
        Position = pos;
    }

    public void Update(float dt)
    {
        if (Chopped)
        {
            respawnTimer += dt;

            if (respawnTimer >= 5f)
            {
                Chopped = false;
                Health = 3;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        if (Chopped)
        {
            // stump
            Raylib.DrawRectangle(
                (int)Position.X + 12,
                (int)Position.Y + 55,
                36,
                18,
                Color.Brown
            );

            return;
        }

        // trunk
        Raylib.DrawRectangle(
            (int)Position.X + 20,
            (int)Position.Y + 40,
            20,
            40,
            Color.Brown
        );

        // leaves
        Raylib.DrawCircle(
            (int)Position.X + 30,
            (int)Position.Y + 25,
            35,
            Color.DarkGreen
        );
    }
}

    class Lake
    {
        public Vector2 Position;
        float rippleTimer = 0f;
        public void Update(float dt) => rippleTimer += dt;

        public Lake(Vector2 pos)
        {
            Position = pos;
        }

        public void Draw()
        {
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 120, new Color(30, 100, 200, 255));

            float ripple = MathF.Sin(rippleTimer * 2f) * 6f;
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(90 + ripple), Color.SkyBlue);
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(60 + ripple * 0.5f), Color.SkyBlue);
        }
    }

    class NPC
{
    public Vector2 Position;

    public string Name;
    public string Dialogue;
    Vector2 wanderTarget;
    float wanderTimer = 0f;
    float speed = 60f;

    public NPC(Vector2 pos, string name, string dialogue)
    {
        Position = pos;
        Name = name;
        Dialogue = dialogue;
    }
public void Update(float dt)
{
    wanderTimer -= dt;

    if (wanderTimer <= 0)
    {
        wanderTarget = Position + new Vector2(
            Raylib.GetRandomValue(-80, 80),
            Raylib.GetRandomValue(-80, 80)
        );
        wanderTimer = Raylib.GetRandomValue(2, 5);
    }

    Position = Vector2.Lerp(Position, wanderTarget, dt * 1.5f);
}
public NPC(Vector2 pos)
{
    Position = pos;
    Name = "Citizen";
    Dialogue = "Hello there.";
}
    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 40, 60);

    public void Draw()
    {
        // head
        Raylib.DrawCircle(
            (int)Position.X + 20,
            (int)Position.Y + 12,
            12,
            Color.Beige
        );

        // shirt
        Raylib.DrawRectangle(
            (int)Position.X + 10,
            (int)Position.Y + 24,
            20,
            30,
            Color.Red
        );

        // legs
        Raylib.DrawRectangle(
            (int)Position.X + 10,
            (int)Position.Y + 54,
            8,
            12,
            Color.Black
        );

        Raylib.DrawRectangle(
            (int)Position.X + 22,
            (int)Position.Y + 54,
            8,
            12,
            Color.Black
        );
    }
}

    class Vehicle
    {
        public Vector2 Position;

        public bool Driving = false;

        float speed;

        Color color;
        Vector2 velocity = Vector2.Zero;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 100, 50);

        public Vehicle(Vector2 pos, Color vehicleColor, float vehicleSpeed)
        {
            Position = pos;
            color = vehicleColor;
            speed = vehicleSpeed;
        }

        public void Update(float dt)
        {
            if (!Driving) return;

            Vector2 move = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.Up))
                move.Y -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.Down))
                move.Y += 1;

            if (Raylib.IsKeyDown(KeyboardKey.Left))
                move.X -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.Right))
                move.X += 1;

            if (move != Vector2.Zero)
                move = Vector2.Normalize(move);

            Vector2 targetVelocity = move * speed;
            velocity = Vector2.Lerp(velocity, targetVelocity, dt * 5f);
            Position += velocity * dt;
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, color);

            Raylib.DrawRectangle((int)Position.X + 18,(int)Position.Y + 8,64,20,Color.Black);

            Raylib.DrawCircle((int)Position.X + 20,(int)Position.Y + 50,10,Color.Black);
            Raylib.DrawCircle((int)Position.X + 80,(int)Position.Y + 50,10,Color.Black);
        }
    }

    class Building
    {
        public Rectangle Bounds;

        public Color ExteriorColor;
        public Color InteriorColor;

        public Vector2 ExitPosition;
        public string BuildingName;

        public NPC InteriorNPC;

        public List<Rectangle> InteriorObjects = new();

        public Building(
            Rectangle bounds,
            Color exterior,
            Color interior,
            Vector2 exitPos,
            string buildingName,
            NPC npc
        )
        {
            Bounds = bounds;
            ExteriorColor = exterior;
            InteriorColor = interior;
            ExitPosition = exitPos;
            BuildingName = buildingName;
            InteriorNPC = npc;

            InteriorObjects.Add(new Rectangle(200,200,240,80));
            InteriorObjects.Add(new Rectangle(600,400,200,120));
            InteriorObjects.Add(new Rectangle(900,250,150,200));
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, ExteriorColor);

            Raylib.DrawRectangle(
                (int)Bounds.X + 100,
                (int)Bounds.Y + 160,
                60,
                80,
                Color.Brown
            );

            Raylib.DrawRectangle(
                (int)Bounds.X + 30,
                (int)Bounds.Y + 40,
                60,
                60,
                Color.LightGray
            );

            Raylib.DrawRectangle(
                (int)Bounds.X + 180,
                (int)Bounds.Y + 40,
                60,
                60,
                Color.LightGray
            );
        }
    }
}
