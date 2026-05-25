
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

        static Player player = new Player(new Vector2(0, 650));

        static int dayOfWeek = 0; // 0-6, Monday to Sunday
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static float dayCounter = 0f; // tracks full day cycles
        static string currentBiome = "SAFE ZONE";
        static string lastBiome = "";
        static float biomeMessageTimer = 0f;
        static bool isFishing = false;
        static float fishingTimer = 0f;
        static float fishingDuration = 3f;
        static bool skillsOpen = false;
        static bool hoverWoodcutting = false;
        static bool hoverFishing = false;
        static bool questsOpen = false;
        static List<Quest> quests = new();
        static string playerName = "";
        static bool nameEntered = false;
        static bool hoverCombat = false;
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
        static bool wardrobeOpen = false;
        static int wardrobeTab = 0; // 0 = shirt, 1 = skin, 2 = pants
        static List<Vector2> raindrops = new();

        static List<TreeObject> trees = new();
        static List<Lake> lakes = new();
        static List<NPC> npcs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();

        static List<FloatingText> floatingTexts = new();
        static List<Enemy> enemies = new();
        static List<LootDrop> lootDrops = new();

        static Building currentBuilding = null;
        static float shakeDuration = 0f;
        static float shakeMagnitude = 6f;

        static void DrawWardrobe()
{
    if (!wardrobeOpen) return;

    int wx = ScreenWidth / 2 - 300;
    int wy = 100;

    // background
    Raylib.DrawRectangle(wx, wy, 600, 480, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(wx, wy, 600, 480, Color.Gold);
    Raylib.DrawText("WARDROBE", wx + 220, wy + 15, 32, Color.Gold);

    // tabs
    string[] tabs = { "SHIRT", "SKIN", "PANTS" };
    for (int i = 0; i < 3; i++)
    {
        Color tabColor = wardrobeTab == i ? Color.Gold : Color.White;
        Raylib.DrawRectangle(wx + 20 + i * 140, wy + 60, 120, 36, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(wx + 20 + i * 140, wy + 60, 120, 36, tabColor);
        Raylib.DrawText(tabs[i], wx + 40 + i * 140, wy + 70, 20, tabColor);
    }

    // color options
    Color[][] colorOptions = {
        new Color[] { Color.Blue, Color.Red, Color.Green, Color.Black, Color.White, Color.Purple, Color.Orange, Color.Yellow },
        new Color[] { Color.Beige, new Color((byte)210,(byte)160,(byte)110,(byte)255), new Color((byte)150,(byte)100,(byte)60,(byte)255), new Color((byte)80,(byte)50,(byte)30,(byte)255) },
        new Color[] { Color.Black, new Color((byte)80,(byte)50,(byte)20,(byte)255), new Color((byte)30,(byte)50,(byte)100,(byte)255), new Color((byte)80,(byte)80,(byte)80,(byte)255) }
    };

    string[][] colorNames = {
        new string[] { "Blue", "Red", "Green", "Black", "White", "Purple", "Orange", "Yellow" },
        new string[] { "Light", "Medium", "Dark", "Deep" },
        new string[] { "Black", "Brown", "Navy", "Grey" }
    };

    Raylib.DrawText("SELECT COLOR:", wx + 20, wy + 115, 22, Color.LightGray);

    for (int i = 0; i < colorOptions[wardrobeTab].Length; i++)
    {
        int cx = wx + 20 + (i % 4) * 140;
        int cy = wy + 150 + (i / 4) * 100;

        Raylib.DrawRectangle(cx, cy, 100, 60, colorOptions[wardrobeTab][i]);
        Raylib.DrawRectangleLines(cx, cy, 100, 60, Color.White);
        Raylib.DrawText(colorNames[wardrobeTab][i], cx + 4, cy + 66, 16, Color.LightGray);

        Vector2 mouse = Raylib.GetMousePosition();
        if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(cx, cy, 100, 60)))
        {
            Raylib.DrawRectangleLines(cx, cy, 100, 60, Color.Gold);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (wardrobeTab == 0) player.ShirtColor = colorOptions[wardrobeTab][i];
                else if (wardrobeTab == 1) player.SkinColor = colorOptions[wardrobeTab][i];
                else if (wardrobeTab == 2) player.PantsColor = colorOptions[wardrobeTab][i];
            }
        }
    }

    // player preview
    Raylib.DrawText("PREVIEW", wx + 460, wy + 115, 22, Color.LightGray);
    Raylib.DrawCircle(wx + 510, wy + 200, 20, player.SkinColor);
    Raylib.DrawRectangle(wx + 493, wy + 220, 34, 50, player.ShirtColor);
    Raylib.DrawRectangle(wx + 493, wy + 270, 14, 20, player.PantsColor);
    Raylib.DrawRectangle(wx + 510, wy + 270, 14, 20, player.PantsColor);

    // close button
    Raylib.DrawRectangle(wx + 220, wy + 420, 160, 40, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLines(wx + 220, wy + 420, 160, 40, Color.White);
    Raylib.DrawText("CLOSE (Q)", wx + 240, wy + 432, 20, Color.White);
}
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

        static string GetCurrentBiome()
{
    float x = player.Position.X;
    float y = player.Position.Y;

    if (x >= -1500 && x <= 2100 && y >= -800 && y <= 1400)
        return "SAFE ZONE";
    else if (y < -400 || y > 1000)
        return "FOREST";
    else if (x > 2100)
        return "DESERT";
    else if (x < -1500)
        return "SNOW ZONE";
    else
        return "GRASSLANDS";
}

        static Color GetNightOverlay()
            {
                float night = MathF.Sin(timeOfDay * MathF.PI);
                byte alpha = (byte)(180 * (1f - night));
                return new Color((byte)0, (byte)0, (byte)30, alpha);
            }
            static void DrawInventoryIcon(string item, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2 - 10;

    switch (item)
    {
        case "Logs":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, Color.Brown);
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)100, (byte)60, (byte)20, (byte)255));
            break;

        case "Fish":
            Raylib.DrawTriangle(
                new Vector2(cx + 20, cy),
                new Vector2(cx + 8, cy - 8),
                new Vector2(cx + 8, cy + 8),
                Color.SkyBlue
            );
            Raylib.DrawEllipse(cx - 6, cy, 18, 10, Color.Blue);
            break;

        case "Bones":
            Raylib.DrawRectangle(cx - 3, cy - 18, 6, 36, Color.White);
            Raylib.DrawCircle(cx, cy - 18, 7, Color.White);
            Raylib.DrawCircle(cx, cy + 18, 7, Color.White);
            break;

        case "Fur":
            Raylib.DrawCircle(cx, cy, 18, new Color((byte)139, (byte)90, (byte)43, (byte)255));
            Raylib.DrawCircle(cx - 8, cy - 8, 8, new Color((byte)160, (byte)110, (byte)60, (byte)255));
            Raylib.DrawCircle(cx + 8, cy - 8, 8, new Color((byte)160, (byte)110, (byte)60, (byte)255));
            break;

        case "Stingers":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - 22),
                new Vector2(cx - 8, cy + 18),
                new Vector2(cx + 8, cy + 18),
                new Color((byte)180, (byte)120, (byte)0, (byte)255)
            );
            break;

        case "Pelts":
            Raylib.DrawRectangle(cx - 16, cy - 18, 32, 36, new Color((byte)100, (byte)100, (byte)120, (byte)255));
            Raylib.DrawRectangle(cx - 10, cy - 10, 20, 20, new Color((byte)130, (byte)130, (byte)150, (byte)255));
            break;

        case "Money":
            Raylib.DrawCircle(cx, cy, 18, Color.Gold);
            Raylib.DrawText("$", cx - 6, cy - 12, 22, Color.DarkGray);
            break;
    }
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
        static void UpdateQuests()
{
    foreach (Quest quest in quests)
    {
        if (quest.Completed) continue;

        if (quest.Title == "Lumberjack")
            quest.Progress = player.Logs;

        if (quest.Title == "Fisher")
            quest.Progress = player.Fish;

        if (quest.Title == "Big Money")
            quest.Progress = player.Money;

        if (quest.Progress >= quest.Target)
        {
            quest.Completed = true;
            quest.Progress = quest.Target;
            player.Money += quest.Reward;
            ShowLevelUp($"Quest Complete: {quest.Title}! +${quest.Reward}", 0);
        }
    }
}
            static void DrawQuestsUI()
{
    // QUESTS button
    Rectangle questsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 60, 140, 40);
    Raylib.DrawRectangleRec(questsBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(questsBtn, 2, questsOpen ? Color.Gold : Color.White);
    Raylib.DrawText("QUESTS", ScreenWidth - 300, ScreenHeight - 48, 22, questsOpen ? Color.Gold : Color.White);

    if (!questsOpen) return;

    // Quest panel
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 260, 300, 200, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLines(ScreenWidth - 320, ScreenHeight - 260, 300, 200, Color.White);
    Raylib.DrawText("QUESTS", ScreenWidth - 290, ScreenHeight - 248, 22, Color.Gold);

    int yOffset = 0;
    foreach (Quest quest in quests)
    {
        Color questColor = quest.Completed ? Color.Green : Color.White;
        string tick = quest.Completed ? "[DONE]" : $"{quest.Progress}/{quest.Target}";
        Raylib.DrawText(quest.Description, ScreenWidth - 310, ScreenHeight - 210 + yOffset, 18, questColor);
        Raylib.DrawText(tick, ScreenWidth - 310, ScreenHeight - 192 + yOffset, 16, questColor);
        Raylib.DrawText($"Reward: ${quest.Reward}", ScreenWidth - 180, ScreenHeight - 192 + yOffset, 16, Color.Gold);
        yOffset += 50;
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
        else if (!Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 160, ScreenHeight - 250, 140, 195)))
            skillsOpen = false;
    }

    if (skillsOpen)
    {
        Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
        Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);
        Rectangle combatBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 230, 140, 40);
        hoverCombat = Raylib.CheckCollisionPointRec(mouse, combatBtn);

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

    // Combat button
        Rectangle combatBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 230, 140, 40);
        Color combatColor = hoverCombat ? Color.Gold : Color.White;
        Raylib.DrawRectangleRec(combatBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
        Raylib.DrawRectangleLinesEx(combatBtn, 2, combatColor);
        Raylib.DrawText($"Combat Lv {player.CombatLevel}", ScreenWidth - 155, ScreenHeight - 218, 20, combatColor);

        // Combat progress bar
        if (!hoverCombat)
        {
            int combatRequired = player.CombatLevel * player.CombatLevel * 50;
            float combatProgress = (float)player.CombatXP / combatRequired;
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
            Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, (int)(140 * combatProgress), 8, Color.Red);
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

    // Combat XP tooltip on hover
    if (hoverCombat)
    {
        int required = player.CombatLevel * player.CombatLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 230, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Raylib.DrawText($"XP: {player.CombatXP}/{required}", ScreenWidth - 315, ScreenHeight - 218, 20, Color.LightGray);
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

    // enemies
    foreach (Enemy enemy in enemies)
{
    if (enemy.Dead) continue;
    int ex = cx + (int)((enemy.Position.X - player.Position.X) * minimapScale);
    int ey = cy + (int)((enemy.Position.Y - player.Position.Y) * minimapScale);
    Raylib.DrawCircle(ex, ey, 3, Color.Red);
}

    // player dot
    Raylib.DrawCircle(cx, cy, 4, Color.White);

    Raylib.DrawText(playerName, minimapX, minimapY + 208, 18, Color.LightGray);
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

                      if (!nameEntered)
    {
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if (playerName.Length < 12)
                playerName += (char)key;
            key = Raylib.GetCharPressed();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && playerName.Length > 0)
            playerName = playerName.Substring(0, playerName.Length - 1);

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) && playerName.Length > 0)
            nameEntered = true;
    }
    else
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            currentScene = SceneState.World;
    }

    break;

                case SceneState.World:

                    if (shakeDuration > 0) shakeDuration -= dt;
                    if (levelUpTimer > 0) levelUpTimer -= dt;
                    timeOfDay += daySpeed * dt;
                    UpdateWeather(dt);
                    UpdateQuests();
                    player.UpdateHealth(dt);
                    bool nearEnemy = false;

                    if (player.Health <= 0)
                    {
                    player.Health = player.MaxHealth;
                    player.Position = new Vector2(400, 400);
                    player.Money = Math.Max(0, player.Money - 50);
                    ShowLevelUp("You died! Lost $50", 0);
                    }

                    currentBiome = GetCurrentBiome();
                    if (currentBiome != lastBiome)
                    {
                        lastBiome = currentBiome;
                        biomeMessageTimer = 3f;
                    }
                    if (biomeMessageTimer > 0) biomeMessageTimer -= dt;

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

                    for (int i = lootDrops.Count - 1; i >= 0; i--)
                    {
                        LootDrop drop = lootDrops[i];

                        if (!drop.Collected && Raylib.CheckCollisionRecs(player.Bounds, drop.Bounds))
                        {
                            drop.Collected = true;

                            switch (drop.ItemType)
                            {
                                case "Bone":
                                    player.Bones++;
                                    break;
                                case "Fur":
                                    player.Fur++;
                                    break;
                                case "Stinger":
                                    player.Stingers++;
                                    break;
                                case "Bear Pelt":
                                    player.BearPelts++;
                                    break;
                            }

                            floatingTexts.Add(new FloatingText {
                                Position = player.Position - new Vector2(0, 40),
                                Text = $"+1 {drop.ItemType}",
                                Timer = 1.5f,
                                TextColor = Color.Gold
                            });

                            lootDrops.RemoveAt(i);
                        }
                    }

                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Update(dt);
                        if (!enemy.Dead && Raylib.CheckCollisionRecs(player.Bounds, enemy.Bounds))
            {
                int damage = 0;
                if (enemy.Type == "Wild Dog") damage = 5;
                else if (enemy.Type == "Wolf") damage = 10;
                else if (enemy.Type == "Scorpion") damage = 8;
                else if (enemy.Type == "Bear") damage = 15;

                player.TakeDamage(damage);
                TriggerShake(0.2f);

                floatingTexts.Add(new FloatingText {
                    Position = player.Position - new Vector2(0, 20),
                    Text = $"-{damage}",
                    Timer = 1f,
                    TextColor = Color.Red
                });
            }
                        if (!enemy.Dead && Vector2.Distance(player.Position, enemy.Position) < 80)
                {
                    nearEnemy = true;

                    if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        int attackDamage = 1 + (player.CombatLevel / 10);
                        enemy.Health -= attackDamage;
                        floatingTexts.Add(new FloatingText {
                            Position = enemy.Position - new Vector2(0, 20),
                            Text = $"-{attackDamage}",
                            Timer = 1f,
                            TextColor = Color.Red
                        });

                        TriggerShake(0.1f);
                        floatingTexts.Add(new FloatingText {
                            Position = enemy.Position - new Vector2(0, 20),
                            Text = "-1",
                            Timer = 1f,
                            TextColor = Color.Red
                        });

                        if (enemy.Health <= 0)
                        {
                            enemy.Dead = true;

                            if (enemy.Type == "Wild Dog") player.AddCombatXP(20);
                                else if (enemy.Type == "Wolf") player.AddCombatXP(35);
                                else if (enemy.Type == "Scorpion") player.AddCombatXP(30);
                                else if (enemy.Type == "Bear") player.AddCombatXP(50);

                           if (enemy.Type == "Wild Dog")
                                lootDrops.Add(new LootDrop(enemy.Position, "Bone"));

                            else if (enemy.Type == "Wolf")
                                lootDrops.Add(new LootDrop(enemy.Position, "Fur"));

                            else if (enemy.Type == "Scorpion")
                                lootDrops.Add(new LootDrop(enemy.Position, "Stinger"));

                            else if (enemy.Type == "Bear")
                                lootDrops.Add(new LootDrop(enemy.Position, "Bear Pelt"));
                        }
                    }
                }
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

    if (!tree.Chopped && !nearEnemy)
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
                    floatingTexts.Add(new FloatingText {
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
    if (currentBuilding.BuildingName == "MY HOUSE")
{
    Vector2 mouse = Raylib.GetMousePosition();

    // tab switching
    for (int i = 0; i < 3; i++)
    {
        Rectangle tabBtn = new Rectangle(ScreenWidth / 2 - 280 + i * 140, 160, 120, 36);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
            Raylib.CheckCollisionPointRec(mouse, tabBtn))
        {
            wardrobeTab = i;
        }
    }

    if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E))
            wardrobeOpen = !wardrobeOpen;
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && wardrobeOpen)
    {
        wardrobeOpen = false;
        return;
    }
}

    if (currentBuilding.BuildingName == "HOSPITAL")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        if (player.Money >= 20)
        {
            player.Money -= 20;
            player.Health = player.MaxHealth;
            shopMessage = "Full health restored for $20!";
            shopMessageTimer = 1.5f;
        }
        else
        {
            shopMessage = "Need $20 to heal!";
            shopMessageTimer = 1.5f;
        }
    }
}

if (currentBuilding.BuildingName == "WEAPONS")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        int upgradeCost = player.CombatLevel * 50;
        if (player.Money >= upgradeCost)
        {
            player.Money -= upgradeCost;
            player.MaxHealth += 10;
            shopMessage = $"Weapon upgraded! Damage increased.";
            shopMessageTimer = 1.5f;
        }
        else
        {
            shopMessage = $"Need ${upgradeCost} to upgrade!";
            shopMessageTimer = 1.5f;
        }
    }
}

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

        if (Raylib.IsKeyPressed(KeyboardKey.G))
            {
                int earned = 0;
                earned += player.Bones * 8; player.Bones = 0;
                earned += player.Fur * 15; player.Fur = 0;
                earned += player.Stingers * 12; player.Stingers = 0;
                earned += player.BearPelts * 25; player.BearPelts = 0;

                if (earned > 0)
                {
                    player.Money += earned;
                    shopMessage = $"Sold all loot for ${earned}!";
                    shopMessageTimer = 1.5f;
                }
                else
                {
                    shopMessage = "No loot to sell!";
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
            Vector2 mouse = Raylib.GetMousePosition();
            Rectangle questsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 60, 140, 40);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
    if (Raylib.CheckCollisionPointRec(mouse, questsBtn))
        questsOpen = !questsOpen;
}
            Raylib.EndDrawing();
        }

        static void DrawMenu()
{
    Raylib.ClearBackground(new Color(10, 10, 20, 255));

    Raylib.DrawText("OPEN WORLD RPG", 312, 182, 64, new Color((byte)255, (byte)200, (byte)0, (byte)80));
    Raylib.DrawText("OPEN WORLD RPG", 310, 180, 64, Color.Gold);

    if (!nameEntered)
    {
        Raylib.DrawText("ENTER YOUR NAME:", 440, 320, 28, Color.LightGray);
        Raylib.DrawRectangle(420, 360, 440, 50, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(420, 360, 440, 50, Color.White);
        Raylib.DrawText(playerName, 440, 375, 28, Color.White);

        // blinking cursor
        if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            Raylib.DrawText("|", 440 + Raylib.MeasureText(playerName, 28), 375, 28, Color.White);

        Raylib.DrawText("PRESS ENTER TO CONFIRM", 420, 430, 22, Color.LightGray);
    }
    else
    {
        Raylib.DrawText($"Welcome, {playerName}!", 420, 320, 34, Color.White);

        if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            Raylib.DrawText("PRESS ENTER TO START", 390, 390, 34, Color.White);
    }
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

           // base
            Raylib.DrawRectangle(-10000, -10000, 20000, 20000, new Color(90, 170, 90, 255));

            // forest top
            Raylib.DrawRectangle(-10000, -10000, 20000, 400, new Color(40, 100, 40, 255));

            // forest bottom
            Raylib.DrawRectangle(-10000, 1000, 20000, 9000, new Color(40, 100, 40, 255));

            // desert (right)
            Raylib.DrawRectangle(2100, -10000, 7900, 20000, new Color(210, 180, 100, 255));

            // snow (left)
            Raylib.DrawRectangle(-10000, -10000, 8500, 20000, new Color(220, 235, 255, 255));

            // safe zone
            Raylib.DrawRectangle(-1500, -800, 3600, 2200, new Color(90, 170, 90, 255));

            // main horizontal road
            Raylib.DrawRectangle(-10000, 550, 20000, 180, Color.DarkGray);

            // north highway
            Raylib.DrawRectangle(200, -10000, 120, 10000, Color.DarkGray);

            // south highway
            Raylib.DrawRectangle(200, 730, 120, 9270, Color.DarkGray);

            // desert side road
            Raylib.DrawRectangle(2100, 200, 3000, 120, Color.DarkGray);

            // snow side road
            Raylib.DrawRectangle(-10000, 200, 8500, 120, Color.DarkGray);

            // vertical road to bank
            Raylib.DrawRectangle(1020, -200, 120, 760, Color.DarkGray);

            // vertical road to store
            Raylib.DrawRectangle(-1260, -200, 120, 760, Color.DarkGray);

            // vertical road to hospital
            Raylib.DrawRectangle(200, -200, 120, 760, Color.DarkGray);

            // vertical road to weapons shop
            Raylib.DrawRectangle(500, -200, 120, 760, Color.DarkGray);

            // road markings main road
            for (int i = -10000; i < 10000; i += 200)
            {
                Raylib.DrawRectangle(i, 630, 100, 12, Color.Yellow);
            }

            // road markings north highway
            for (int i = -10000; i < 730; i += 200)
            {
                Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);
            }

            // road markings south highway
            for (int i = 730; i < 10000; i += 200)
            {
                Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);
            }

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

             foreach (Enemy enemy in enemies)
            {
                enemy.Draw();
            }

            foreach (LootDrop drop in lootDrops)
            {
                drop.Draw();
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

            if (currentBuilding.BuildingName == "HOSPITAL")
        {
            Raylib.DrawText("E = Restore Health ($20)", 20, 600, 22, Color.LightGray);
        }

            if (currentBuilding.BuildingName == "WEAPONS")
        {
            int upgradeCost = player.CombatLevel * 50;
            Raylib.DrawText($"E = Upgrade Weapon (${upgradeCost})", 20, 600, 22, Color.LightGray);
        }

    if (currentBuilding.BuildingName == "STORE")
    {
        Raylib.DrawText("E = Sell Log ($5) | F = Sell Fish ($10) | G = Sell Loot", 20, 600, 22, Color.LightGray);
    }

    if (currentBuilding.BuildingName == "BANK")
    {
        Raylib.DrawText($"E = Deposit $10 | F = Withdraw $10", 20, 600, 22, Color.LightGray);
    }

    if (currentBuilding.BuildingName == "MY HOUSE")
{
    Raylib.DrawText("E = Open Wardrobe", 20, 600, 22, Color.LightGray);
}
}
        DrawWardrobe();
        }

        static void DrawHUD()
        {
            DrawSkillsUI();
            DrawQuestsUI();

            // health bar
            int hbWidth = 300;
            int hbX = ScreenWidth / 2 - hbWidth / 2;
            Raylib.DrawRectangle(hbX, 10, hbWidth, 24, new Color((byte)40, (byte)40, (byte)40, (byte)220));
            float healthPercent = (float)player.Health / player.MaxHealth;
            Color hpColor = healthPercent > 0.5f ? Color.Green : healthPercent > 0.25f ? Color.Orange : Color.Red;
            Raylib.DrawRectangle(hbX, 10, (int)(hbWidth * healthPercent), 24, hpColor);
            Raylib.DrawRectangleLines(hbX, 10, hbWidth, 24, Color.White);
            Raylib.DrawText($"HP: {player.Health}/{player.MaxHealth}", hbX + hbWidth / 2 - 40, 13, 18, Color.White);

           Raylib.DrawRectangle(0, ScreenHeight - 34, ScreenWidth, 34, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Raylib.DrawText("SPACE = Chop Tree | R = Fish | TAB = Inventory | E = Enter Building | F = Drive Vehicle", 20, ScreenHeight - 28, 20, Color.White);

            if (biomeMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, biomeMessageTimer));
    Color biomeColor = currentBiome switch
    {
        "SNOW ZONE" => new Color((byte)150,(byte)200,(byte)255,alpha),
        "DESERT" => new Color((byte)210,(byte)150,(byte)20,alpha),
        "FOREST" => new Color((byte)30,(byte)130,(byte)30,alpha),
        "SAFE ZONE" => new Color((byte)100,(byte)200,(byte)100,alpha),
        _ => new Color((byte)255,(byte)255,(byte)255,alpha)
    };

    int textWidth = Raylib.MeasureText($"ENTERING {currentBiome}", 36);
    Raylib.DrawText($"ENTERING {currentBiome}", ScreenWidth / 2 - textWidth / 2, 280, 36, biomeColor);
}
           
 if (player.InventoryOpen)
{
    int invX = ScreenWidth - 380;
    int invY = 100;
    int slotSize = 80;
    int padding = 10;
    int cols = 4;

    // build dynamic item list
    List<(string name, int count)> items = new();
    if (player.Logs > 0) items.Add(("Logs", player.Logs));
    if (player.Fish > 0) items.Add(("Fish", player.Fish));
    if (player.Bones > 0) items.Add(("Bones", player.Bones));
    if (player.Fur > 0) items.Add(("Fur", player.Fur));
    if (player.Stingers > 0) items.Add(("Stingers", player.Stingers));
    if (player.BearPelts > 0) items.Add(("Pelts", player.BearPelts));
    if (player.Money > 0) items.Add(("Money", player.Money));

    // background panel
    Raylib.DrawRectangle(invX - 20, invY - 20, cols * (slotSize + padding) + 30, 5 * (slotSize + padding) + 60, new Color((byte)0, (byte)0, (byte)0, (byte)220));
    Raylib.DrawText("INVENTORY", invX, invY - 10, 24, Color.Gold);

    for (int i = 0; i < 20; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int x = invX + col * (slotSize + padding);
        int y = invY + 20 + row * (slotSize + padding);

        // slot background
        Raylib.DrawRectangle(x, y, slotSize, slotSize, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(x, y, slotSize, slotSize, new Color((byte)100, (byte)100, (byte)100, (byte)255));

        if (i >= items.Count) continue;

        // draw icon
        DrawInventoryIcon(items[i].name, x, y, slotSize);

        // item name and count top left
        Raylib.DrawText($"{items[i].count}", x + 6, y + 6, 16, Color.White);
        Raylib.DrawText(items[i].name, x + 4, y + slotSize - 20, 13, Color.LightGray);
    }

    Raylib.DrawText("TAB = Close", invX, invY + 5 * (slotSize + padding) + 30, 20, Color.LightGray);
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
            for (int i = -8000; i < 8000; i += 250)
            {
                trees.Add(new TreeObject(new Vector2(i, -300)));
                trees.Add(new TreeObject(new Vector2(i, 1200)));
            }
            

            lakes.Add(new Lake(new Vector2(700, 1200)));
            lakes.Add(new Lake(new Vector2(-900, -600)));

         buildings.Add(new Building(
    new Rectangle(1200, 410, 160, 120),
    new Color(180,120,90,255),
    new Color(90,70,50,255),
    new Vector2(1100,700),
    "BANK",
    new NPC(new Vector2(700,450), "Bank Manager", "Chur maori. Welcome to Waikato Bank.")
));

buildings.Add(new Building(
    new Rectangle(1700, 410, 160, 120),
    Color.DarkBlue,
    new Color(50,60,90,255),
    new Vector2(1800,650),
    "DBar",
    new NPC(new Vector2(600,420), "Dbar Owner", "Grab a woodys and relax at Dbar.")
));

buildings.Add(new Building(
    new Rectangle(-1000, 410, 160, 120),
    Color.DarkGreen,
    new Color(40,90,50,255),
    new Vector2(-1050,600),
    "STORE",
    new NPC(new Vector2(500,420), "Store Clerk", "Need supplies for fishing? Show me the moolack")
));

buildings.Add(new Building(
    new Rectangle(340, -200, 160, 120),
    new Color(220,50,50,255),
    new Color(200,220,220,255),
    new Vector2(420,650),
    "HOSPITAL",
    new NPC(new Vector2(600,420), "Doctor", "Kia ora! I can patch you up for $20.")
));

buildings.Add(new Building(
    new Rectangle(660, 150, 160, 120),
    new Color(80,80,80,255),
    new Color(50,50,60,255),
    new Vector2(740,650),
    "WEAPONS",
    new NPC(new Vector2(600,420), "Weapons Dealer", "Need a sharper blade bro? I got you.")
));

buildings.Add(new Building(
    new Rectangle(-400, 410, 160, 120),
    new Color(200,160,100,255),
    new Color(180,140,100,255),
    new Vector2(-320,650),
    "MY HOUSE",
    new NPC(new Vector2(800,420), "Mirror", "Check yourself out bro.")
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

            quests.Add(new Quest("Lumberjack", "Chop 10 trees", 10, 50));
            quests.Add(new Quest("Fisher", "Catch 10 fish", 10, 75));
            quests.Add(new Quest("Big Money", "Earn $100", 100, 200));

            // Grasslands - Wild Dogs
            enemies.Add(new Enemy(new Vector2(2300, 600), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(2600, 400), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(2400, 900), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(3000, 300), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(3500, 700), "Wild Dog", 3, Color.Brown));

            // Forest - Wolves
            enemies.Add(new Enemy(new Vector2(-300, -600), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(400, -800), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-200, 1400), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(500, 1600), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(1000, -700), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-800, 1500), "Wolf", 5, Color.DarkGray));

            // Desert - Scorpions
            enemies.Add(new Enemy(new Vector2(3500, 300), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(4000, 700), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(4500, 200), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(5000, 600), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(5500, 400), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));

            // Snow - Bears
            enemies.Add(new Enemy(new Vector2(-3000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-3500, 600), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-4000, 400), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-4500, 700), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-5000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
                    }

                }
    struct FloatingText
    {
    public Vector2 Position;
    public string Text;
    public float Timer;
    public Color TextColor;
    }

    class LootDrop
{
    public Vector2 Position;
    public string ItemType;
    public bool Collected = false;

    public LootDrop(Vector2 pos, string itemType)
    {
        Position = pos;
        ItemType = itemType;
    }

    public Rectangle Bounds =>
        new Rectangle(Position.X - 10, Position.Y - 10, 20, 20);

    public void Draw()
    {
        if (Collected) return;

        Raylib.DrawCircle((int)Position.X, (int)Position.Y, 10, Color.Gold);

        switch (ItemType)
        {
            case "Bone":
                Raylib.DrawRectangle((int)Position.X - 3, (int)Position.Y - 8, 6, 16, Color.White);
                break;
            case "Fur":
                Raylib.DrawCircle((int)Position.X, (int)Position.Y, 7, new Color((byte)139, (byte)90, (byte)43, (byte)255));
                break;
            case "Stinger":
                Raylib.DrawTriangle(
                    new Vector2(Position.X, Position.Y - 10),
                    new Vector2(Position.X - 6, Position.Y + 6),
                    new Vector2(Position.X + 6, Position.Y + 6),
                    new Color((byte)180, (byte)120, (byte)0, (byte)255)
                );
                break;
            case "Bear Pelt":
                Raylib.DrawRectangle((int)Position.X - 8, (int)Position.Y - 8, 16, 16, new Color((byte)100, (byte)100, (byte)120, (byte)255));
                break;
        }
    }
}
    class Enemy
{
    public Vector2 Position;
    public int Health;
    public int MaxHealth;
    public string Type;
    public Color EnemyColor;
    public bool Dead = false;
    float respawnTimer = 0f;
    Vector2 wanderTarget;
    float wanderTimer = 0f;
    float speed = 40f;
    public Vector2 SpawnPosition;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 40, 40);

    public Enemy(Vector2 pos, string type, int health, Color color)
    {
        Position = pos;
        SpawnPosition = pos;
        Type = type;
        Health = health;
        MaxHealth = health;
        EnemyColor = color;
        wanderTarget = pos;
    }

    public void Update(float dt)
    {
        if (Dead)
        {
            respawnTimer += dt;
            if (respawnTimer >= 10f)
            {
                Dead = false;
                Health = MaxHealth;
                Position = SpawnPosition;
                respawnTimer = 0f;
            }
            return;
        }

        wanderTimer -= dt;

        if (wanderTimer <= 0)
        {
            wanderTarget = Position + new Vector2(
                Raylib.GetRandomValue(-100, 100),
                Raylib.GetRandomValue(-100, 100)
            );
            wanderTimer = Raylib.GetRandomValue(2, 5);
        }

        Position = Vector2.Lerp(Position, wanderTarget, dt * 1.2f);
    }

    public void Draw()
    {
        if (Dead) return;

        Raylib.DrawRectangleRec(Bounds, EnemyColor);
        Raylib.DrawText(Type, (int)Position.X - 10, (int)Position.Y - 20, 16, Color.White);

        // health bar background
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, 40, 6, Color.DarkGray);

        // health bar fill
        float healthPercent = (float)Health / MaxHealth;
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, (int)(40 * healthPercent), 6, Color.Red);
    }
}
    class Quest
{
    public string Title;
    public string Description;
    public int Target;
    public int Progress;
    public bool Completed;
    public int Reward;

    public Quest(string title, string description, int target, int reward)
    {
        Title = title;
        Description = description;
        Target = target;
        Reward = reward;
        Progress = 0;
        Completed = false;
    }
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
        public int Bones = 0;
        public int Fur = 0;
        public int Stingers = 0;
        public int BearPelts = 0;
        public int Health = 100;
        public int MaxHealth = 100;
        public int CombatLevel = 1;
        public int CombatXP = 0;
        float regenTimer = 0f;
        float damageCooldown = 0f;
        public Color ShirtColor = Color.Blue;
        public Color SkinColor = Color.Beige;
        public Color PantsColor = Color.Black;

        public bool InventoryOpen = false;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 position)
        {
            Position = position;
        }
        public void TakeDamage(int damage)
{
    if (damageCooldown > 0) return;
    Health -= damage;
    damageCooldown = 1f;
    if (Health < 0) Health = 0;
}

        public void UpdateHealth(float dt)
        {
            if (damageCooldown > 0) damageCooldown -= dt;

            if (damageCooldown <= 0 && Health < MaxHealth)
            {
                regenTimer += dt;
                if (regenTimer >= 2f)
                {
                    Health++;
                    regenTimer = 0f;
                }
            }
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

        public void AddCombatXP(int xp)
        {
            if (CombatLevel >= 100) return;

            CombatXP += xp;

            int requiredXP = CombatLevel * CombatLevel * 50;

            if (CombatXP >= requiredXP)
            {
                CombatXP = 0;
                CombatLevel++;
                Program.ShowLevelUp("Combat", CombatLevel);
            }
        }


        public void Draw()
{
            if (Hidden) return;

            Raylib.DrawCircle((int)Position.X + 20,(int)Position.Y + 12,12,SkinColor);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 24,20,30,ShirtColor);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 54,8,12,PantsColor);
            Raylib.DrawRectangle((int)Position.X + 22,(int)Position.Y + 54,8,12,PantsColor);

            Raylib.DrawRectangle((int)Position.X + 2,(int)Position.Y + 26,8,18,SkinColor);
            Raylib.DrawRectangle((int)Position.X + 30,(int)Position.Y + 26,8,18,SkinColor);
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

    // door
    Raylib.DrawRectangle(
        (int)Bounds.X + 50,
        (int)Bounds.Y + 70,
        30,
        50,
        Color.Brown
    );

    // window left
    Raylib.DrawRectangle(
        (int)Bounds.X + 10,
        (int)Bounds.Y + 20,
        30,
        30,
        Color.LightGray
    );

    // window right
    Raylib.DrawRectangle(
        (int)Bounds.X + 100,
        (int)Bounds.Y + 20,
        30,
        30,
        Color.LightGray
    );
}
    }
}
