
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

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
        static float autoSaveTimer = 0f;
        static float autoSaveInterval = 300f;
        static int selectedSlot = 0;
        static string[] savePaths = { "savegame1.txt", "savegame2.txt", "savegame3.txt" };
        static string savePath => savePaths[selectedSlot];
        static float totalPlayTime = 0f;
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
        static bool shopUIOpen = false;
        static int shopSelectedItem = -1;
        static string shopSelectedItemName = "";
        static bool pauseMenuOpen = false;
        static bool mapOpen = false;
        static int overwriteSlot = -1;
        static bool overwriteConfirmOpen = false;
        static bool optionsMenuOpen = false;
        static bool cheatsMenuOpen = false;
        static bool loadMenuOpen = false;
        static int cheatGoldAmount = 100;
        static string shopMessage = "";
        static float shopMessageTimer = 0f;
        static int minimapSize = 200;
        static int minimapX = 20;
        static int minimapY = 20;
        static float minimapScale = 0.02f;
        static bool isRaining = false;
        static float rainTimer = 0f;
        static float rainInterval = 30f;
        static bool mainMenuChoice = false; // false = showing menu, true = showing name entry
        static bool wardrobeOpen = false;
        static int wardrobeTab = 0; // 0 = shirt, 1 = skin, 2 = pants
        static bool chestOpen = false;
        static int chestLogs = 0;
        static int chestFish = 0;
        static int chestBones = 0;
        static int chestFur = 0;
        static int chestStingers = 0;
        static int chestBearPelts = 0;
        static int chestSelectedSlot = -1;
        static bool isLoadingGame = false;
        static bool slotSelected = false;
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

        static (bool exists, string name, string info) GetSlotInfo(int slot)
        {
            string path = savePaths[slot];
            if (!System.IO.File.Exists(path)) return (false, "", "");

            string[] lines = System.IO.File.ReadAllLines(path);
            if (lines.Length < 24) return (false, "", "");

            string name = lines[23];
            int wcLv = int.Parse(lines[15]);
            int fishLv = int.Parse(lines[17]);
            int combatLv = int.Parse(lines[19]);
            float playTime = lines.Length > 45 ? float.Parse(lines[45]) : 0f;
            int hours = (int)(playTime / 3600);
            int minutes = (int)((playTime % 3600) / 60);

            return (true, name, $"WC:{wcLv} Fish:{fishLv} Combat:{combatLv} | {hours}h {minutes}m");
        }
        static void ShowNotification(string message)
        {
            levelUpMessage = message;
            levelUpTimer = 2.5f;
        }

        static void SaveGame()
{
    List<string> lines = new List<string>
    {
        player.Position.X.ToString(),
        player.Position.Y.ToString(),
        player.Money.ToString(),
        player.BankBalance.ToString(),
        player.Logs.ToString(),
        player.BirchLogs.ToString(),
        player.OakLogs.ToString(),
        player.PineLogs.ToString(),
        player.ArcticLogs.ToString(),
        player.DeadWood.ToString(),
        player.Fish.ToString(),
        player.Bones.ToString(),
        player.Fur.ToString(),
        player.Stingers.ToString(),
        player.BearPelts.ToString(),
        player.WoodcuttingLevel.ToString(),
        player.WoodcuttingXP.ToString(),
        player.FishingLevel.ToString(),
        player.FishingXP.ToString(),
        player.CombatLevel.ToString(),
        player.CombatXP.ToString(),
        player.Health.ToString(),
        player.MaxHealth.ToString(),
        playerName,
        player.ShirtColor.R.ToString(),
        player.ShirtColor.G.ToString(),
        player.ShirtColor.B.ToString(),
        player.SkinColor.R.ToString(),
        player.SkinColor.G.ToString(),
        player.SkinColor.B.ToString(),
        player.PantsColor.R.ToString(),
        player.PantsColor.G.ToString(),
        player.PantsColor.B.ToString(),
        chestLogs.ToString(),
        chestFish.ToString(),
        chestBones.ToString(),
        chestFur.ToString(),
        chestStingers.ToString(),
        chestBearPelts.ToString(),
        quests[0].Progress.ToString(),
        quests[0].Completed ? "1" : "0",
        quests[1].Progress.ToString(),
        quests[1].Completed ? "1" : "0",
        quests[2].Progress.ToString(),
        quests[2].Completed ? "1" : "0",
        totalPlayTime.ToString(),
    };

    System.IO.File.WriteAllLines(savePath, lines);
    ShowNotification("Game Saved!");
}

       static void LoadGame()
{
    if (!System.IO.File.Exists(savePath)) return;

    string[] lines = System.IO.File.ReadAllLines(savePath);

    if (lines.Length < 45) return;

    player.Position = new Vector2(float.Parse(lines[0]), float.Parse(lines[1]));
    player.Money = int.Parse(lines[2]);
    player.BankBalance = int.Parse(lines[3]);
    player.Logs = int.Parse(lines[4]);
    player.BirchLogs = int.Parse(lines[5]);
    player.OakLogs = int.Parse(lines[6]);
    player.PineLogs = int.Parse(lines[7]);
    player.ArcticLogs = int.Parse(lines[8]);
    player.DeadWood = int.Parse(lines[9]);
    player.Fish = int.Parse(lines[10]);
    player.Bones = int.Parse(lines[11]);
    player.Fur = int.Parse(lines[12]);
    player.Stingers = int.Parse(lines[13]);
    player.BearPelts = int.Parse(lines[14]);
    player.WoodcuttingLevel = int.Parse(lines[15]);
    player.WoodcuttingXP = int.Parse(lines[16]);
    player.FishingLevel = int.Parse(lines[17]);
    player.FishingXP = int.Parse(lines[18]);
    player.CombatLevel = int.Parse(lines[19]);
    player.CombatXP = int.Parse(lines[20]);
    player.Health = int.Parse(lines[21]);
    player.MaxHealth = int.Parse(lines[22]);
    playerName = lines[23];
    nameEntered = true;
    player.ShirtColor = new Color((byte)int.Parse(lines[24]), (byte)int.Parse(lines[25]), (byte)int.Parse(lines[26]), (byte)255);
    player.SkinColor = new Color((byte)int.Parse(lines[27]), (byte)int.Parse(lines[28]), (byte)int.Parse(lines[29]), (byte)255);
    player.PantsColor = new Color((byte)int.Parse(lines[30]), (byte)int.Parse(lines[31]), (byte)int.Parse(lines[32]), (byte)255);
    chestLogs = int.Parse(lines[33]);
    chestFish = int.Parse(lines[34]);
    chestBones = int.Parse(lines[35]);
    chestFur = int.Parse(lines[36]);
    chestStingers = int.Parse(lines[37]);
    chestBearPelts = int.Parse(lines[38]);
    quests[0].Progress = int.Parse(lines[39]);
    quests[0].Completed = lines[40] == "1";
    quests[1].Progress = int.Parse(lines[41]);
    quests[1].Completed = lines[42] == "1";
    quests[2].Progress = int.Parse(lines[43]);
    quests[2].Completed = lines[44] == "1";

    if (lines.Length > 45) totalPlayTime = float.Parse(lines[45]);
}
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
        static void DrawShopUI()
{
    if (!shopUIOpen) return;

    int panelX = ScreenWidth / 2 - 420;
    int panelY = 60;
    int slotSize = 70;
    int padding = 8;
    int cols = 5;

    // background
    Raylib.DrawRectangle(panelX, panelY, 840, 560, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 840, 560, Color.Gold);
    Raylib.DrawText("SHOP", panelX + 20, panelY + 15, 28, Color.Gold);
    Raylib.DrawText("YOUR INVENTORY", panelX + 450, panelY + 15, 28, Color.Gold);

    // divider
    Raylib.DrawRectangle(panelX + 415, panelY + 10, 4, 540, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // shop stock left side - prices
    string[] shopItems = { "Logs", "Birch Logs", "Oak Logs", "Pine Logs", "Arctic Logs", "Dead Wood", "Fish", "Bones", "Fur", "Stingers", "Pelts" };
    int[] shopPrices = { 5, 8, 12, 18, 25, 3, 10, 8, 15, 12, 25 };

    Raylib.DrawText("Click item to sell", panelX + 20, panelY + 48, 16, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // inventory slots right side
    int[] invCounts = {
        player.Logs, player.BirchLogs, player.OakLogs, player.PineLogs,
        player.ArcticLogs, player.DeadWood, player.Fish, player.Bones,
        player.Fur, player.Stingers, player.BearPelts
    };

    // draw shop price list on left
    for (int i = 0; i < shopItems.Length; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = panelX + 20 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);

        bool selected = shopSelectedItemName == shopItems[i];
        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, selected ? Color.Gold : new Color((byte)100,(byte)100,(byte)100,(byte)255));

        DrawInventoryIcon(shopItems[i] == "Pelts" ? "Pelts" : shopItems[i], sx, sy, slotSize);
        Raylib.DrawText($"${shopPrices[i]}", sx + 6, sy + 6, 16, Color.Gold);
        Raylib.DrawText(shopItems[i].Length > 7 ? shopItems[i].Substring(0, 7) : shopItems[i], sx + 4, sy + slotSize - 18, 13, Color.LightGray);
    }

    // draw player inventory on right
    Raylib.DrawText("Click to select", panelX + 450, panelY + 48, 16, Color.LightGray);
    for (int i = 0; i < shopItems.Length; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = panelX + 435 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);

        bool selected = shopSelectedItemName == shopItems[i];
        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, selected ? Color.Gold : new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (invCounts[i] > 0)
        {
            DrawInventoryIcon(shopItems[i] == "Pelts" ? "Pelts" : shopItems[i], sx, sy, slotSize);
            Raylib.DrawText($"{invCounts[i]}", sx + 6, sy + 6, 16, Color.White);
            Raylib.DrawText(shopItems[i].Length > 7 ? shopItems[i].Substring(0, 7) : shopItems[i], sx + 4, sy + slotSize - 18, 13, Color.LightGray);

            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
            {
                Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    shopSelectedItem = i;
                    shopSelectedItemName = shopItems[i];
                }
            }
        }
    }

    // sell options panel at bottom if item selected
    if (shopSelectedItem >= 0)
    {
        int sellPanelY = panelY + 430;
        Raylib.DrawRectangle(panelX + 20, sellPanelY, 800, 100, new Color((byte)30,(byte)30,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(panelX + 20, sellPanelY, 800, 100, Color.Gold);

        int price = shopPrices[shopSelectedItem];
        int count = invCounts[shopSelectedItem];
        Raylib.DrawText($"Selling: {shopSelectedItemName} @ ${price} each", panelX + 30, sellPanelY + 10, 20, Color.White);

        // sell 1 button
        Rectangle sell1Btn = new Rectangle(panelX + 30, sellPanelY + 45, 160, 44);
        bool hover1 = Raylib.CheckCollisionPointRec(mouse, sell1Btn);
        Raylib.DrawRectangleRec(sell1Btn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(sell1Btn, 2, hover1 ? Color.Gold : Color.White);
        Raylib.DrawText($"Sell 1 (${price})", panelX + 40, sellPanelY + 57, 18, hover1 ? Color.Gold : Color.White);

        // sell 5 button
        Rectangle sell5Btn = new Rectangle(panelX + 210, sellPanelY + 45, 160, 44);
        bool hover5 = Raylib.CheckCollisionPointRec(mouse, sell5Btn);
        Raylib.DrawRectangleRec(sell5Btn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(sell5Btn, 2, hover5 ? Color.Gold : Color.White);
        Raylib.DrawText($"Sell 5 (${price * Math.Min(5, count)})", panelX + 220, sellPanelY + 57, 18, hover5 ? Color.Gold : Color.White);

        // sell all button
        Rectangle sellAllBtn = new Rectangle(panelX + 390, sellPanelY + 45, 160, 44);
        bool hoverAll = Raylib.CheckCollisionPointRec(mouse, sellAllBtn);
        Raylib.DrawRectangleRec(sellAllBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(sellAllBtn, 2, hoverAll ? Color.Gold : Color.White);
        Raylib.DrawText($"Sell All (${price * count})", panelX + 400, sellPanelY + 57, 18, hoverAll ? Color.Gold : Color.White);

        if (hover1 && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
        {
            SellItem(shopSelectedItemName, 1, price);
            if (GetItemCount(shopSelectedItemName) <= 0) { shopSelectedItem = -1; shopSelectedItemName = ""; }
        }
        if (hover5 && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
        {
            SellItem(shopSelectedItemName, Math.Min(5, count), price);
            if (GetItemCount(shopSelectedItemName) <= 0) { shopSelectedItem = -1; shopSelectedItemName = ""; }
        }
        if (hoverAll && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
        {
            SellItem(shopSelectedItemName, count, price);
            shopSelectedItem = -1;
            shopSelectedItemName = "";
        }
    }

    Raylib.DrawText("Q = Close Shop", panelX + 350, panelY + 520, 20, Color.LightGray);
}

static void SellItem(string itemName, int amount, int price)
{
    int earned = amount * price;
    player.Money += earned;
    shopMessage = $"Sold {amount} {itemName} for ${earned}!";
    shopMessageTimer = 1.5f;

    switch (itemName)
    {
        case "Logs": player.Logs -= amount; break;
        case "Birch Logs": player.BirchLogs -= amount; break;
        case "Oak Logs": player.OakLogs -= amount; break;
        case "Pine Logs": player.PineLogs -= amount; break;
        case "Arctic Logs": player.ArcticLogs -= amount; break;
        case "Dead Wood": player.DeadWood -= amount; break;
        case "Fish": player.Fish -= amount; break;
        case "Bones": player.Bones -= amount; break;
        case "Fur": player.Fur -= amount; break;
        case "Stingers": player.Stingers -= amount; break;
        case "Pelts": player.BearPelts -= amount; break;
    }
}

static int GetItemCount(string itemName)
{
    return itemName switch
    {
        "Logs" => player.Logs,
        "Birch Logs" => player.BirchLogs,
        "Oak Logs" => player.OakLogs,
        "Pine Logs" => player.PineLogs,
        "Arctic Logs" => player.ArcticLogs,
        "Dead Wood" => player.DeadWood,
        "Fish" => player.Fish,
        "Bones" => player.Bones,
        "Fur" => player.Fur,
        "Stingers" => player.Stingers,
        "Pelts" => player.BearPelts,
        _ => 0
    };
}
        static void DrawChestUI()
{
    if (!chestOpen) return;

    int panelX = ScreenWidth / 2 - 420;
    int panelY = 60;
    int slotSize = 70;
    int padding = 8;
    int cols = 5;

    // background
    Raylib.DrawRectangle(panelX, panelY, 840, 560, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 840, 560, Color.Gold);
    Raylib.DrawText("CHEST", panelX + 20, panelY + 15, 28, Color.Gold);
    Raylib.DrawText("INVENTORY", panelX + 450, panelY + 15, 28, Color.Gold);

    // divider
    Raylib.DrawRectangle(panelX + 415, panelY + 10, 4, 540, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    string[] itemNames = { "Logs", "Fish", "Bones", "Fur", "Stingers", "Pelts" };
    int[] chestCounts = { chestLogs, chestFish, chestBones, chestFur, chestStingers, chestBearPelts };
    int[] invCounts = { player.Logs, player.Fish, player.Bones, player.Fur, player.Stingers, player.BearPelts };

    Vector2 mouse = Raylib.GetMousePosition();

    // chest slots - 20 slots left side
    Raylib.DrawText("Click to withdraw", panelX + 20, panelY + 48, 16, Color.LightGray);
    for (int i = 0; i < 20; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = panelX + 20 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (i < itemNames.Length && chestCounts[i] > 0)
        {
            DrawInventoryIcon(itemNames[i], sx, sy, slotSize);
            Raylib.DrawText($"{chestCounts[i]}", sx + 6, sy + 6, 16, Color.White);
            Raylib.DrawText(itemNames[i], sx + 4, sy + slotSize - 18, 13, Color.LightGray);

            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
            {
                Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    switch (i)
                    {
                        case 0: player.Logs++; chestLogs--; break;
                        case 1: player.Fish++; chestFish--; break;
                        case 2: player.Bones++; chestBones--; break;
                        case 3: player.Fur++; chestFur--; break;
                        case 4: player.Stingers++; chestStingers--; break;
                        case 5: player.BearPelts++; chestBearPelts--; break;
                    }
                }
            }
        }
    }

    // inventory slots - 20 slots right side
    Raylib.DrawText("Click to deposit", panelX + 450, panelY + 48, 16, Color.LightGray);
    for (int i = 0; i < 20; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = panelX + 435 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (i < itemNames.Length && invCounts[i] > 0)
        {
            DrawInventoryIcon(itemNames[i], sx, sy, slotSize);
            Raylib.DrawText($"{invCounts[i]}", sx + 6, sy + 6, 16, Color.White);
            Raylib.DrawText(itemNames[i], sx + 4, sy + slotSize - 18, 13, Color.LightGray);

            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
            {
                Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    switch (i)
                    {
                        case 0: chestLogs++; player.Logs--; break;
                        case 1: chestFish++; player.Fish--; break;
                        case 2: chestBones++; player.Bones--; break;
                        case 3: chestFur++; player.Fur--; break;
                        case 4: chestStingers++; player.Stingers--; break;
                        case 5: chestBearPelts++; player.BearPelts--; break;
                    }
                }
            }
        }
    }

    Raylib.DrawText("Q = Close Chest", panelX + 350, panelY + 520, 20, Color.LightGray);
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

                if (x >= -3000 && x <= 4000 && y >= -1500 && y <= 2500)
                    return "SAFE ZONE";
                else if (y < -400 || y > 1000)
                    return "FOREST";
                else if (x > 4000)
                    return "DESERT";
                else if (x < -3000)
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

        case "Birch Logs":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, Color.White);
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)200,(byte)200,(byte)200,(byte)255));
            Raylib.DrawRectangle(cx - 10, cy - 6, 4, 4, Color.DarkGray);
            Raylib.DrawRectangle(cx + 6, cy - 2, 4, 4, Color.DarkGray);
            break;

        case "Oak Logs":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, new Color((byte)101,(byte)67,(byte)33,(byte)255));
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)80,(byte)50,(byte)20,(byte)255));
            Raylib.DrawCircle(cx - 18, cy, 6, new Color((byte)101,(byte)67,(byte)33,(byte)255));
            Raylib.DrawCircle(cx + 18, cy, 6, new Color((byte)101,(byte)67,(byte)33,(byte)255));
            break;

        case "Pine Logs":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, new Color((byte)120,(byte)80,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)0,(byte)80,(byte)0,(byte)255));
            break;

        case "Arctic Logs":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, new Color((byte)180,(byte)210,(byte)230,(byte)255));
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)220,(byte)235,(byte)255,(byte)255));
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 5, new Color((byte)220,(byte)235,(byte)255,(byte)180));
            break;

        case "Dead Wood":
            Raylib.DrawRectangle(cx - 20, cy - 8, 40, 16, Color.DarkGray);
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangle(cx - 8, cy - 6, 4, 12, new Color((byte)60,(byte)60,(byte)60,(byte)255));
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
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, minimapSize, new Color((byte)0,(byte)0,(byte)0,(byte)180));

    Raylib.BeginScissorMode(minimapX, minimapY, minimapSize, minimapSize);

    int cx = minimapX + minimapSize / 2;
    int cy = minimapY + minimapSize / 2;

    // -- BIOMES --
    // base grasslands colour
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, minimapSize, new Color((byte)90,(byte)170,(byte)90,(byte)200));

    // forest top
    int forestTopY = cy + (int)((-300 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, Math.Clamp(forestTopY - minimapY, 0, minimapSize), new Color((byte)40,(byte)100,(byte)40,(byte)220));

    // forest bottom
    int forestBotY = cy + (int)((1000 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(minimapX, forestBotY, minimapSize, Math.Clamp(minimapY + minimapSize - forestBotY, 0, minimapSize), new Color((byte)40,(byte)100,(byte)40,(byte)220));

    // desert right
    int desertX = cx + (int)((4000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(desertX, minimapY, Math.Clamp(minimapX + minimapSize - desertX, 0, minimapSize), minimapSize, new Color((byte)210,(byte)180,(byte)100,(byte)220));

    // snow left
    int snowLeftX = cx + (int)((-3000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(minimapX, minimapY, Math.Clamp(snowLeftX - minimapX, 0, minimapSize), minimapSize, new Color((byte)220,(byte)235,(byte)255,(byte)220));

    // safe zone overlay
    int szX = cx + (int)((-3000 - player.Position.X) * minimapScale);
    int szY = cy + (int)((-1500 - player.Position.Y) * minimapScale);
    int szX2 = cx + (int)((4000 - player.Position.X) * minimapScale);
    int szY2 = cy + (int)((2500 - player.Position.Y) * minimapScale);

    // clamp to minimap bounds
    int clampedSzX = Math.Clamp(szX, minimapX, minimapX + minimapSize);
    int clampedSzY = Math.Clamp(szY, minimapY, minimapY + minimapSize);
    int clampedSzX2 = Math.Clamp(szX2, minimapX, minimapX + minimapSize);
    int clampedSzY2 = Math.Clamp(szY2, minimapY, minimapY + minimapSize);

    Raylib.DrawRectangle(
        clampedSzX,
        clampedSzY,
        clampedSzX2 - clampedSzX,
        clampedSzY2 - clampedSzY,
        new Color((byte)90,(byte)170,(byte)90,(byte)180)
    );

    // -- ROADS --
    // main horizontal road
    int roadY = cy + (int)((550 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(minimapX, roadY, minimapSize, Math.Max(1, (int)(180 * minimapScale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // north/south highway
    int highwayX = cx + (int)((200 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(highwayX, minimapY, Math.Max(1, (int)(120 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road top
    int ringTopY = cy + (int)((-38000 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(minimapX, ringTopY, minimapSize, Math.Max(1, (int)(180 * minimapScale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road bottom
    int ringBotY = cy + (int)((38000 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(minimapX, ringBotY, minimapSize, Math.Max(1, (int)(180 * minimapScale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road left
    int ringLeftX = cx + (int)((-40000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(ringLeftX, minimapY, Math.Max(1, (int)(180 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road right
    int ringRightX = cx + (int)((39820 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(ringRightX, minimapY, Math.Max(1, (int)(180 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // snow vertical connectors
    int snow1X = cx + (int)((-20000 - player.Position.X) * minimapScale);
    int snow2X = cx + (int)((-10000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(snow1X, minimapY, Math.Max(1, (int)(120 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(snow2X, minimapY, Math.Max(1, (int)(120 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // desert vertical connectors
    int des1X = cx + (int)((15000 - player.Position.X) * minimapScale);
    int des2X = cx + (int)((25000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(des1X, minimapY, Math.Max(1, (int)(120 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(des2X, minimapY, Math.Max(1, (int)(120 * minimapScale)), minimapSize, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // desert side road
    int desertRoadY = cy + (int)((200 - player.Position.Y) * minimapScale);
    int desertRoadX = cx + (int)((4000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(desertRoadX, desertRoadY, Math.Clamp(minimapX + minimapSize - desertRoadX, 0, minimapSize), Math.Max(1, (int)(120 * minimapScale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // snow side road
    int snowRoadY = cy + (int)((200 - player.Position.Y) * minimapScale);
    int snowRoadLeftX = cx + (int)((-3000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(minimapX, snowRoadY, Math.Clamp(snowRoadLeftX - minimapX, 0, minimapSize), Math.Max(1, (int)(120 * minimapScale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));



    // -- BUILDINGS --
    foreach (Building building in buildings)
    {
        int bx = cx + (int)((building.Bounds.X - player.Position.X) * minimapScale);
        int by = cy + (int)((building.Bounds.Y - player.Position.Y) * minimapScale);

        if (bx >= minimapX && bx <= minimapX + minimapSize &&
            by >= minimapY && by <= minimapY + minimapSize)
        {
            Raylib.DrawRectangle(bx - 3, by - 3, 10, 10, Color.Yellow);
            // building name label
            Raylib.DrawText(building.BuildingName, bx + 8, by - 4, 10, Color.Yellow);
        }
    }

    // -- BORDER --
    Raylib.DrawRectangleLines(minimapX, minimapY, minimapSize, minimapSize, Color.White);

   // -- PLAYER DOT --
    Raylib.DrawCircle(cx, cy, 4, Color.White);

    Raylib.EndScissorMode();

    // -- BORDER -- drawn after scissor so it's always clean
    Raylib.DrawRectangleLines(minimapX, minimapY, minimapSize, minimapSize, Color.White);

    // -- PLAYER NAME and LEGEND -- drawn outside scissor so they don't get clipped
    Raylib.DrawText(playerName, minimapX, minimapY + minimapSize + 6, 18, Color.LightGray);

    int lx = minimapX;
    int ly = minimapY + minimapSize + 28;
    Raylib.DrawRectangle(lx, ly, 10, 10, new Color((byte)90,(byte)170,(byte)90,(byte)255));
    Raylib.DrawText("Safe", lx + 13, ly, 12, Color.LightGray);
    Raylib.DrawRectangle(lx + 50, ly, 10, 10, new Color((byte)40,(byte)100,(byte)40,(byte)255));
    Raylib.DrawText("Forest", lx + 63, ly, 12, Color.LightGray);
    Raylib.DrawRectangle(lx, ly + 16, 10, 10, new Color((byte)210,(byte)180,(byte)100,(byte)255));
    Raylib.DrawText("Desert", lx + 13, ly + 16, 12, Color.LightGray);
    Raylib.DrawRectangle(lx + 50, ly + 16, 10, 10, new Color((byte)220,(byte)235,(byte)255,(byte)255));
    Raylib.DrawText("Snow", lx + 63, ly + 16, 12, Color.LightGray);
}

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG V3");
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.Null);

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

    if (!mainMenuChoice)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        Rectangle newGameBtn = new Rectangle(ScreenWidth / 2 - 150, 360, 300, 60);
        Rectangle loadGameBtn = new Rectangle(ScreenWidth / 2 - 150, 440, 300, 60);

        bool anySaveExists = savePaths.Any(p => System.IO.File.Exists(p));

       if (Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (Raylib.CheckCollisionPointRec(mouse, newGameBtn))
    {
        mainMenuChoice = true;
        isLoadingGame = false;
    }

    if (anySaveExists && Raylib.CheckCollisionPointRec(mouse, loadGameBtn))
    {
        mainMenuChoice = true;
        isLoadingGame = true;
    }
}
    }
    else
    {
        Vector2 mouse = Raylib.GetMousePosition();

        for (int i = 0; i < 3; i++)
        {
            Rectangle slotBtn = new Rectangle(ScreenWidth / 2 - 250, 300 + i * 100, 500, 80);
            var (exists, name, info) = GetSlotInfo(i);

      if (Raylib.CheckCollisionPointRec(mouse, slotBtn))
{
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (isLoadingGame && exists)
        {
            selectedSlot = i;
            slotSelected = true;
            LoadGame();
            currentScene = SceneState.World;
        }
        else if (!isLoadingGame && !exists)
        {
            selectedSlot = i;
            slotSelected = true;
            playerName = "typing";
            nameEntered = false;
            totalPlayTime = 0f;
        }
        else if (!isLoadingGame && exists)
        {
            overwriteConfirmOpen = true;
            overwriteSlot = i;
        }
    }
}
        }

        if (slotSelected && !nameEntered)
{
    if (playerName == "typing") playerName = "";

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
        else if (nameEntered)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    player = new Player(new Vector2(0, 650));
                    chestLogs = 0; chestFish = 0; chestBones = 0;
                    chestFur = 0; chestStingers = 0; chestBearPelts = 0;
                    timeOfDay = 0f; dayOfWeek = 0;
                    quests[0].Progress = 0; quests[0].Completed = false;
                    quests[1].Progress = 0; quests[1].Completed = false;
                    quests[2].Progress = 0; quests[2].Completed = false;
                    totalPlayTime = 0f;
                    currentScene = SceneState.World;
                }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape) && playerName.Length == 0)
            {
                mainMenuChoice = false;
                slotSelected = false;
                playerName = "";
                nameEntered = false;
            }
        }

    break;

                case SceneState.World:

                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                        {
                            pauseMenuOpen = !pauseMenuOpen;
                            optionsMenuOpen = false;
                            cheatsMenuOpen = false;
                            loadMenuOpen = false;
                            mapOpen = false;
                        }

                        if (!pauseMenuOpen)
                        {
                    if (shakeDuration > 0) shakeDuration -= dt;
                    if (levelUpTimer > 0) levelUpTimer -= dt;
                    timeOfDay += daySpeed * dt;
                    UpdateWeather(dt);
                    UpdateQuests();
                    player.UpdateHealth(dt);
                    bool nearEnemy = false;
                    totalPlayTime += dt;

                    if (player.Health <= 0)
                    {
                    player.Health = player.MaxHealth;
                    player.Position = new Vector2(400, 400);
                    player.Money = Math.Max(0, player.Money - 50);
                    ShowLevelUp("You died! Lost $50", 0);
                    }

                   autoSaveTimer += dt;
                        if (autoSaveTimer >= autoSaveInterval && nameEntered)
                        {
                            autoSaveTimer = 0f;
                            SaveGame();
                        }

                    if (Raylib.IsKeyPressed(KeyboardKey.F5))
                        SaveGame();

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
                if (player.WoodcuttingLevel < tree.LevelRequired)
                {
                    floatingTexts.Add(new FloatingText {
                        Position = player.Position - new Vector2(0, 40),
                        Text = $"Need WC level {tree.LevelRequired}!",
                        Timer = 1.5f,
                        TextColor = Color.Red
                    });
                }
                else
                {
                    tree.Health--;

                    if (tree.Health <= 0)
                    {
                        tree.Chopped = true;
                        player.AddWoodcuttingXP(tree.XPReward);
                        int logsGained = Raylib.GetRandomValue(1, 3);

                        switch (tree.LogType)
                        {
                            case "Logs": player.Logs += logsGained; break;
                            case "Birch Logs": player.BirchLogs += logsGained; break;
                            case "Oak Logs": player.OakLogs += logsGained; break;
                            case "Pine Logs": player.PineLogs += logsGained; break;
                            case "Arctic Logs": player.ArcticLogs += logsGained; break;
                            case "Dead Wood": player.DeadWood += logsGained; break;
                        }

                        TriggerShake(0.15f);
                        floatingTexts.Add(new FloatingText {
                            Position = player.Position - new Vector2(0, 20),
                            Text = $"+{tree.XPReward} WC XP",
                            Timer = 1.2f,
                            TextColor = Color.Yellow
                        });
                        floatingTexts.Add(new FloatingText {
                            Position = player.Position - new Vector2(0, 44),
                            Text = $"+{logsGained} {tree.LogType}",
                            Timer = 1.2f,
                            TextColor = Color.Orange
                        });
                    }
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
}

                    break;

         case SceneState.Building:

    player.UpdateInterior(dt, currentBuilding.InteriorObjects);
    if (shopMessageTimer > 0) shopMessageTimer -= dt;

    // My House - independent of NPC distance
    if (currentBuilding.BuildingName == "MY HOUSE")
    {
        Vector2 mouse = Raylib.GetMousePosition();
        Vector2 wardrobePos = new Vector2(530, 240);
        Vector2 chestPos = new Vector2(330, 375);

        for (int i = 0; i < 3; i++)
        {
            Rectangle tabBtn = new Rectangle(ScreenWidth / 2 - 280 + i * 140, 160, 120, 36);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                Raylib.CheckCollisionPointRec(mouse, tabBtn))
                wardrobeTab = i;
        }

        if (!chestOpen && Vector2.Distance(player.Position, wardrobePos) < 200)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                wardrobeOpen = !wardrobeOpen;
        }

        if (!wardrobeOpen && Vector2.Distance(player.Position, chestPos) < 200)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                chestOpen = !chestOpen;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && wardrobeOpen)
        {
            wardrobeOpen = false;
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && chestOpen)
        {
            chestOpen = false;
            return;
        }
    }

              if (currentBuilding.BuildingName == "STORE")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
        shopUIOpen = !shopUIOpen;

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && shopUIOpen)
    {
        shopUIOpen = false;
        shopSelectedItem = -1;
        shopSelectedItemName = "";
        return;
    }
}

    // All other buildings - NPC distance check
    if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
    {
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
                    shopMessage = "Weapon upgraded! Damage increased.";
                    shopMessageTimer = 1.5f;
                }
                else
                {
                    shopMessage = $"Need ${upgradeCost} to upgrade!";
                    shopMessageTimer = 1.5f;
                }
            }
        }

        if (currentBuilding.BuildingName == "BANK")
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Z))
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

            if (Raylib.IsKeyPressed(KeyboardKey.X))
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

   if (Raylib.IsKeyPressed(KeyboardKey.Q) && !wardrobeOpen && !chestOpen && !shopUIOpen)
{
    currentScene = SceneState.World;
    player.Position = currentBuilding.ExitPosition;
    shopUIOpen = false;
    shopSelectedItem = -1;
    shopSelectedItemName = "";
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
                    DrawOverwriteConfirm();
                    break;

                case SceneState.World:
                    DrawWorld();
                    DrawPauseMenu();
                    DrawWorldMap();
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

        static void DrawOverwriteConfirm()
{
    if (!overwriteConfirmOpen) return;

    // dim background
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)150));

    // panel
    int px = ScreenWidth / 2 - 250;
    int py = ScreenHeight / 2 - 100;
    Raylib.DrawRectangle(px, py, 500, 200, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(px, py, 500, 200, Color.Gold);

    var (exists, name, info) = GetSlotInfo(overwriteSlot);
    Raylib.DrawText("OVERWRITE SAVE?", px + 120, py + 20, 28, Color.Gold);
    Raylib.DrawText($"Slot {overwriteSlot + 1}: {name}", px + 30, py + 65, 22, Color.White);
    Raylib.DrawText(info, px + 30, py + 95, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // confirm button
    Rectangle confirmBtn = new Rectangle(px + 40, py + 140, 180, 44);
    bool hoverConfirm = Raylib.CheckCollisionPointRec(mouse, confirmBtn);
    Raylib.DrawRectangleRec(confirmBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(confirmBtn, 2, hoverConfirm ? Color.Gold : Color.White);
    Raylib.DrawText("OVERWRITE", px + 65, py + 153, 24, hoverConfirm ? Color.Gold : Color.White);

    // cancel button
    Rectangle cancelBtn = new Rectangle(px + 280, py + 140, 180, 44);
    bool hoverCancel = Raylib.CheckCollisionPointRec(mouse, cancelBtn);
    Raylib.DrawRectangleRec(cancelBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(cancelBtn, 2, hoverCancel ? Color.Red : Color.White);
    Raylib.DrawText("CANCEL", px + 320, py + 153, 24, hoverCancel ? Color.Red : Color.White);

   if (hoverConfirm && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            selectedSlot = overwriteSlot;
            if (System.IO.File.Exists(savePaths[overwriteSlot]))
                System.IO.File.Delete(savePaths[overwriteSlot]);
            slotSelected = true;
            playerName = "typing";
            nameEntered = false;
            totalPlayTime = 0f;
            overwriteConfirmOpen = false;
            overwriteSlot = -1;
        }

    if (hoverCancel && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        overwriteConfirmOpen = false;
        overwriteSlot = -1;
    }
}
       static void DrawMenu()
{
    Raylib.ClearBackground(new Color(10, 10, 20, 255));
    Raylib.DrawText("OPEN WORLD RPG", 312, 182, 64, new Color((byte)255,(byte)200,(byte)0,(byte)80));
    Raylib.DrawText("OPEN WORLD RPG", 310, 180, 64, Color.Gold);

    if (!mainMenuChoice)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        bool anySaveExists = savePaths.Any(p => System.IO.File.Exists(p));

        Rectangle newGameBtn = new Rectangle(ScreenWidth / 2 - 150, 360, 300, 60);
        bool hoverNew = Raylib.CheckCollisionPointRec(mouse, newGameBtn);
        Raylib.DrawRectangleRec(newGameBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(newGameBtn, 2, hoverNew ? Color.Gold : Color.White);
        Raylib.DrawText("NEW GAME", ScreenWidth / 2 - 80, 378, 28, hoverNew ? Color.Gold : Color.White);

        Rectangle loadGameBtn = new Rectangle(ScreenWidth / 2 - 150, 440, 300, 60);
        bool hoverLoad = anySaveExists && Raylib.CheckCollisionPointRec(mouse, loadGameBtn);
        Color loadColor = anySaveExists ? (hoverLoad ? Color.Gold : Color.White) : Color.DarkGray;
        Raylib.DrawRectangleRec(loadGameBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(loadGameBtn, 2, loadColor);
        Raylib.DrawText("LOAD GAME", ScreenWidth / 2 - 85, 458, 28, loadColor);

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            mainMenuChoice = false;
            playerName = "";
            nameEntered = false;
        }

        if (!anySaveExists)
            Raylib.DrawText("No save files found", ScreenWidth / 2 - 100, 510, 20, Color.DarkGray);
    }
        else if (!slotSelected)
    {
        // show save slots
        Raylib.DrawText("SELECT SAVE SLOT", ScreenWidth / 2 - 130, 250, 28, Color.LightGray);
        Vector2 mouse = Raylib.GetMousePosition();

        for (int i = 0; i < 3; i++)
        {
            Rectangle slotBtn = new Rectangle(ScreenWidth / 2 - 250, 300 + i * 100, 500, 80);
            bool hover = Raylib.CheckCollisionPointRec(mouse, slotBtn);
            var (exists, name, info) = GetSlotInfo(i);

            Raylib.DrawRectangleRec(slotBtn, new Color((byte)30,(byte)30,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(slotBtn, 2, hover ? Color.Gold : Color.White);
            Raylib.DrawText($"SLOT {i + 1}", (int)slotBtn.X + 20, (int)slotBtn.Y + 12, 22, hover ? Color.Gold : Color.White);

            if (exists)
                {
                    Raylib.DrawText(name, (int)slotBtn.X + 120, (int)slotBtn.Y + 12, 22, Color.White);
                    Raylib.DrawText(info, (int)slotBtn.X + 20, (int)slotBtn.Y + 46, 18, Color.LightGray);
                }
            else
            {
                Raylib.DrawText("Empty Slot", (int)slotBtn.X + 120, (int)slotBtn.Y + 26, 22, Color.DarkGray);
            }
        }

        Raylib.DrawText("ESC = Back", ScreenWidth / 2 - 50, 620, 20, Color.LightGray);
    }
    else if (slotSelected && !nameEntered)
    {
        Raylib.DrawText("ENTER YOUR NAME:", 440, 320, 28, Color.LightGray);
        Raylib.DrawRectangle(420, 360, 440, 50, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(420, 360, 440, 50, Color.White);
        Raylib.DrawText(playerName, 440, 375, 28, Color.White);

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
        static void DrawWorldMap()
{
    if (!mapOpen) return;

    int mapW = 900;
    int mapH = 600;
    int mapX = ScreenWidth / 2 - mapW / 2;
    int mapY = ScreenHeight / 2 - mapH / 2;
    float scale = 0.001f;

    // background panel
    Raylib.DrawRectangle(mapX, mapY, mapW, mapH, new Color((byte)10,(byte)10,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(mapX, mapY, mapW, mapH, Color.Gold);
    Raylib.DrawText("WORLD MAP", mapX + mapW / 2 - 70, mapY + 10, 28, Color.Gold);

    int cx = mapX + mapW / 2;
    int cy = mapY + mapH / 2;

    // clip drawing to map panel
    // -- BIOMES --

    // base grasslands
    Raylib.DrawRectangle(mapX, mapY, mapW, mapH, new Color((byte)90,(byte)170,(byte)90,(byte)255));

    // forest top
    int forestTopH = cy + (int)(-300 * scale) - mapY;
    Raylib.DrawRectangle(mapX, mapY, mapW, Math.Max(0, forestTopH), new Color((byte)40,(byte)100,(byte)40,(byte)255));

    // forest bottom
    int forestBotY = cy + (int)(1000 * scale);
    Raylib.DrawRectangle(mapX, forestBotY, mapW, Math.Max(0, mapY + mapH - forestBotY), new Color((byte)40,(byte)100,(byte)40,(byte)255));

    // desert right
    int desertX = cx + (int)(4000 * scale);
    Raylib.DrawRectangle(desertX, mapY, Math.Max(0, mapX + mapW - desertX), mapH, new Color((byte)210,(byte)180,(byte)100,(byte)255));

    // snow left
    int snowX = cx + (int)(-3000 * scale);
    Raylib.DrawRectangle(mapX, mapY, Math.Max(0, snowX - mapX), mapH, new Color((byte)220,(byte)235,(byte)255,(byte)255));

    // safe zone
    int szX = cx + (int)(-3000 * scale);
    int szY = cy + (int)(-1500 * scale);
    int szW = (int)(7000 * scale);
    int szH = (int)(4000 * scale);
    Raylib.DrawRectangle(szX, szY, szW, szH, new Color((byte)90,(byte)170,(byte)90,(byte)255));
    Raylib.DrawRectangleLines(szX, szY, szW, szH, new Color((byte)120,(byte)200,(byte)120,(byte)255));

    // -- ROADS --
    // main horizontal road
    int roadY = cy + (int)(550 * scale);
    Raylib.DrawRectangle(mapX, roadY, mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // north/south highway
    int highwayX = cx + (int)(200 * scale);
    Raylib.DrawRectangle(highwayX, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // desert side road
    int desertRoadY = cy + (int)(200 * scale);
    Raylib.DrawRectangle(desertX, desertRoadY, mapX + mapW - desertX, Math.Max(2, (int)(120 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // snow side road
    Raylib.DrawRectangle(mapX, desertRoadY, snowX - mapX, Math.Max(2, (int)(120 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road top
    int ringTopY = cy + (int)(-38000 * scale);
    Raylib.DrawRectangle(mapX, ringTopY, mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road bottom
    int ringBotY = cy + (int)(38000 * scale);
    Raylib.DrawRectangle(mapX, ringBotY, mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road left
    int ringLeftX = cx + (int)(-40000 * scale);
    Raylib.DrawRectangle(ringLeftX, mapY, Math.Max(2, (int)(180 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // outer ring road right
    int ringRightX = cx + (int)(39820 * scale);
    Raylib.DrawRectangle(ringRightX, mapY, Math.Max(2, (int)(180 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // snow vertical connectors
    int snow1X = cx + (int)(-20000 * scale);
    int snow2X = cx + (int)(-10000 * scale);
    Raylib.DrawRectangle(snow1X, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(snow2X, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // desert vertical connectors
    int des1X = cx + (int)(15000 * scale);
    int des2X = cx + (int)(25000 * scale);
    Raylib.DrawRectangle(des1X, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(des2X, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // vertical building roads
    int bankRoadX = cx + (int)(1020 * scale);
    Raylib.DrawRectangle(bankRoadX, roadY - (int)(600 * scale), Math.Max(2, (int)(120 * scale)), (int)(600 * scale), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    int storeRoadX = cx + (int)(-1260 * scale);
    Raylib.DrawRectangle(storeRoadX, roadY - (int)(600 * scale), Math.Max(2, (int)(120 * scale)), (int)(600 * scale), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // -- LAKES --
    foreach (Lake lake in lakes)
    {
        int lakeX = cx + (int)(lake.Position.X * scale);
        int lakeY = cy + (int)(lake.Position.Y * scale);
        Raylib.DrawCircle(lakeX, lakeY, (int)(120 * scale), new Color((byte)30,(byte)100,(byte)200,(byte)255));
        Raylib.DrawCircleLines(lakeX, lakeY, (int)(120 * scale), Color.SkyBlue);
        Raylib.DrawText("Lake", lakeX - 14, lakeY - 8, 12, Color.White);
    }

    // -- BUILDINGS --
    foreach (Building building in buildings)
    {
        int bx = cx + (int)(building.Bounds.X * scale);
        int by = cy + (int)(building.Bounds.Y * scale);

        if (bx >= mapX && bx <= mapX + mapW && by >= mapY && by <= mapY + mapH)
        {
            Raylib.DrawRectangle(bx - 5, by - 5, 14, 14, Color.Yellow);
            Raylib.DrawRectangleLines(bx - 5, by - 5, 14, 14, Color.Gold);
            Raylib.DrawText(building.BuildingName, bx + 12, by - 6, 12, Color.Yellow);
        }
    }

    // -- PLAYER POSITION --
    int px = cx + (int)(player.Position.X * scale);
    int py = cy + (int)(player.Position.Y * scale);
    px = Math.Clamp(px, mapX + 5, mapX + mapW - 5);
    py = Math.Clamp(py, mapY + 5, mapY + mapH - 5);
    Raylib.DrawCircle(px, py, 6, Color.White);
    Raylib.DrawCircleLines(px, py, 6, Color.Gold);
    Raylib.DrawText(playerName, px + 10, py - 8, 14, Color.White);

    // -- BIOME LABELS --
    Raylib.DrawText("FOREST", mapX + mapW / 2 - 30, mapY + 8, 16, new Color((byte)150,(byte)255,(byte)150,(byte)255));
    Raylib.DrawText("FOREST", mapX + mapW / 2 - 30, forestBotY + 4, 16, new Color((byte)150,(byte)255,(byte)150,(byte)255));
    Raylib.DrawText("DESERT", mapX + mapW - 80, mapY + mapH / 2, 16, new Color((byte)255,(byte)220,(byte)100,(byte)255));
    Raylib.DrawText("SNOW ZONE", mapX + 8, mapY + mapH / 2, 16, new Color((byte)200,(byte)220,(byte)255,(byte)255));
    Raylib.DrawText("SAFE ZONE", szX + 10, szY + 10, 16, new Color((byte)100,(byte)255,(byte)100,(byte)255));
    Raylib.DrawText("GRASSLANDS", cx + (int)(2200 * scale), cy - 20, 16, new Color((byte)150,(byte)220,(byte)150,(byte)255));

    // -- LEGEND --
    int lx = mapX + 10;
    int ly = mapY + mapH - 80;
    Raylib.DrawRectangle(lx, ly, mapW - 20, 70, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Raylib.DrawCircle(lx + 20, ly + 18, 6, Color.White);
    Raylib.DrawText("= You", lx + 30, ly + 10, 14, Color.White);
    Raylib.DrawRectangle(lx + 100, ly + 12, 12, 12, Color.Yellow);
    Raylib.DrawText("= Building", lx + 116, ly + 10, 14, Color.Yellow);
    Raylib.DrawCircle(lx + 240, ly + 18, 8, new Color((byte)30,(byte)100,(byte)200,(byte)255));
    Raylib.DrawText("= Lake", lx + 252, ly + 10, 14, Color.SkyBlue);

    // -- CLOSE HINT --
    Raylib.DrawText("ESC or click MAP to close", mapX + mapW - 220, mapY + 10, 16, Color.LightGray);
}
        static void DrawPauseMenu()
{
    if (!pauseMenuOpen) return;

    // dim background
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)150));

    // panel
    Raylib.DrawRectangle(ScreenWidth / 2 - 200, ScreenHeight / 2 - 250, 400, 500, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 - 200, ScreenHeight / 2 - 250, 400, 500, Color.Gold);
    Raylib.DrawText("PAUSED", ScreenWidth / 2 - 70, ScreenHeight / 2 - 230, 40, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    string[] buttons = { "RESUME", "LOAD GAME", "OPTIONS", "CHEATS", "MAP", "QUIT TO MENU" };

    for (int i = 0; i < buttons.Length; i++)
    {
        Rectangle btn = new Rectangle(ScreenWidth / 2 - 150, ScreenHeight / 2 - 140 + i * 80, 300, 55);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Raylib.DrawText(buttons[i], (int)btn.X + 20, (int)btn.Y + 16, 26, hover ? Color.Gold : Color.White);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            switch (buttons[i])
            {
                case "RESUME":
                    pauseMenuOpen = false;
                    break;
                case "LOAD GAME":
                    loadMenuOpen = !loadMenuOpen;
                    optionsMenuOpen = false;
                    cheatsMenuOpen = false;
                    break;
                case "OPTIONS":
                    optionsMenuOpen = !optionsMenuOpen;
                    loadMenuOpen = false;
                    cheatsMenuOpen = false;
                    break;
                case "CHEATS":
                    cheatsMenuOpen = !cheatsMenuOpen;
                    optionsMenuOpen = false;
                    loadMenuOpen = false;
                    break;
                case "MAP":
                    mapOpen = !mapOpen;
                    optionsMenuOpen = false;
                    loadMenuOpen = false;
                    cheatsMenuOpen = false;
                    break;
                case "QUIT TO MENU":
                    pauseMenuOpen = false;
                    currentScene = SceneState.MainMenu;
                    mainMenuChoice = false;
                    slotSelected = false;
                    playerName = "";
                    nameEntered = false;
                    break;
            }
        }
    }

    if (loadMenuOpen) DrawPauseLoadMenu();
    if (optionsMenuOpen) DrawOptionsMenu();
    if (cheatsMenuOpen) DrawCheatsMenu();
}

static void DrawPauseLoadMenu()
{
    Raylib.DrawRectangle(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 300, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 300, Color.Gold);
    Raylib.DrawText("LOAD GAME", ScreenWidth / 2 + 280, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < 3; i++)
    {
        Rectangle slotBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 190 + i * 85, 360, 70);
        bool hover = Raylib.CheckCollisionPointRec(mouse, slotBtn);
        var (exists, name, info) = GetSlotInfo(i);

        Raylib.DrawRectangleRec(slotBtn, new Color((byte)30, (byte)30, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(slotBtn, 2, hover ? Color.Gold : Color.White);
        Raylib.DrawText($"SLOT {i + 1}", (int)slotBtn.X + 15, (int)slotBtn.Y + 10, 20, hover ? Color.Gold : Color.White);

        if (exists)
        {
            Raylib.DrawText(name, (int)slotBtn.X + 110, (int)slotBtn.Y + 10, 20, Color.White);
            Raylib.DrawText(info, (int)slotBtn.X + 15, (int)slotBtn.Y + 40, 16, Color.LightGray);
        }
        else
        {
            Raylib.DrawText("Empty Slot", (int)slotBtn.X + 110, (int)slotBtn.Y + 25, 20, Color.DarkGray);
        }

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left) && exists)
        {
            selectedSlot = i;
            LoadGame();
            pauseMenuOpen = false;
            loadMenuOpen = false;
        }
    }
}

static void DrawOptionsMenu()
{
    Raylib.DrawRectangle(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 300, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 300, Color.Gold);
    Raylib.DrawText("OPTIONS", ScreenWidth / 2 + 300, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    // day speed slider
    Raylib.DrawText("Day Speed", ScreenWidth / 2 + 240, ScreenHeight / 2 - 180, 22, Color.White);
    Rectangle sliderBg = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, 300, 16);
    Raylib.DrawRectangleRec(sliderBg, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    float daySpeedNorm = (daySpeed - 0.005f) / (0.1f - 0.005f);
    Raylib.DrawRectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, (int)(300 * daySpeedNorm), 16, Color.Gold);
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, 300, 16, Color.White);
    if (Raylib.IsMouseButtonDown(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, sliderBg))
    {
        daySpeedNorm = (mouse.X - (ScreenWidth / 2 + 240)) / 300f;
        daySpeedNorm = Math.Clamp(daySpeedNorm, 0f, 1f);
        daySpeed = 0.005f + daySpeedNorm * (0.1f - 0.005f);
    }
    Raylib.DrawText($"{daySpeed:F3}", ScreenWidth / 2 + 555, ScreenHeight / 2 - 153, 18, Color.LightGray);

    // minimap size toggle
    Raylib.DrawText("Minimap Size", ScreenWidth / 2 + 240, ScreenHeight / 2 - 100, 22, Color.White);
    Rectangle minimapBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 70, 160, 40);
    bool hoverMinimap = Raylib.CheckCollisionPointRec(mouse, minimapBtn);
    Raylib.DrawRectangleRec(minimapBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(minimapBtn, 2, hoverMinimap ? Color.Gold : Color.White);
    Raylib.DrawText(minimapSize == 200 ? "Normal" : "Large", ScreenWidth / 2 + 270, ScreenHeight / 2 - 58, 22, hoverMinimap ? Color.Gold : Color.White);
    if (hoverMinimap && Raylib.IsMouseButtonPressed(MouseButton.Left))
        minimapSize = minimapSize == 200 ? 300 : 200;

    // rain toggle
    Raylib.DrawText("Rain", ScreenWidth / 2 + 240, ScreenHeight / 2 - 10, 22, Color.White);
    Rectangle rainBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 + 20, 160, 40);
    bool hoverRain = Raylib.CheckCollisionPointRec(mouse, rainBtn);
    Raylib.DrawRectangleRec(rainBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(rainBtn, 2, hoverRain ? Color.Gold : Color.White);
    Raylib.DrawText(isRaining ? "ON" : "OFF", ScreenWidth / 2 + 270, ScreenHeight / 2 + 32, 22, isRaining ? Color.SkyBlue : Color.DarkGray);
    if (hoverRain && Raylib.IsMouseButtonPressed(MouseButton.Left))
        isRaining = !isRaining;
}

static void DrawCheatsMenu()
{
    Raylib.DrawRectangle(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 400, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 400, Color.Gold);
    Raylib.DrawText("CHEATS", ScreenWidth / 2 + 310, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    string[] cheats = { $"Add ${cheatGoldAmount} Gold", "Max Health", "Fill Inventory", "Max All Skills", "Clear Enemies" };

    for (int i = 0; i < cheats.Length; i++)
    {
        Rectangle btn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 180 + i * 70, 320, 50);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Raylib.DrawText(cheats[i], (int)btn.X + 15, (int)btn.Y + 14, 22, hover ? Color.Gold : Color.White);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            switch (i)
            {
                case 0:
                    player.Money += cheatGoldAmount;
                    ShowNotification($"+${cheatGoldAmount} Gold added!");
                    break;
                case 1:
                    player.Health = player.MaxHealth;
                    ShowNotification("Health maxed out!");
                    break;
                case 2:
                    player.Logs += 50;
                    player.Fish += 50;
                    player.Bones += 50;
                    player.Fur += 50;
                    player.Stingers += 50;
                    player.BearPelts += 50;
                    ShowNotification("Inventory filled!");
                    break;
                case 3:
                    player.WoodcuttingLevel = 99;
                    player.FishingLevel = 99;
                    player.CombatLevel = 99;
                    ShowNotification("All skills maxed!");
                    break;
                case 4:
                    foreach (Enemy e in enemies) e.Dead = true;
                    ShowNotification("All enemies cleared!");
                    break;
            }
        }
    }

    // gold amount adjuster
    Raylib.DrawText("Gold Amount:", ScreenWidth / 2 + 240, ScreenHeight / 2 + 175, 20, Color.LightGray);
    Rectangle minusBtn = new Rectangle(ScreenWidth / 2 + 370, ScreenHeight / 2 + 170, 36, 36);
    Rectangle plusBtn = new Rectangle(ScreenWidth / 2 + 460, ScreenHeight / 2 + 170, 36, 36);
    bool hoverMinus = Raylib.CheckCollisionPointRec(mouse, minusBtn);
    bool hoverPlus = Raylib.CheckCollisionPointRec(mouse, plusBtn);

    Raylib.DrawRectangleRec(minusBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(minusBtn, 2, hoverMinus ? Color.Gold : Color.White);
    Raylib.DrawText("-", (int)minusBtn.X + 12, (int)minusBtn.Y + 8, 22, Color.White);

    Raylib.DrawText($"{cheatGoldAmount}", ScreenWidth / 2 + 412, ScreenHeight / 2 + 178, 20, Color.White);

    Raylib.DrawRectangleRec(plusBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(plusBtn, 2, hoverPlus ? Color.Gold : Color.White);
    Raylib.DrawText("+", (int)plusBtn.X + 10, (int)plusBtn.Y + 8, 22, Color.White);

    if (hoverMinus && Raylib.IsMouseButtonPressed(MouseButton.Left))
        cheatGoldAmount = Math.Max(50, cheatGoldAmount - 50);
    if (hoverPlus && Raylib.IsMouseButtonPressed(MouseButton.Left))
        cheatGoldAmount = Math.Min(10000, cheatGoldAmount + 50);
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
            Raylib.DrawRectangle(-40000, -40000, 80000, 80000, new Color(90, 170, 90, 255));

            // forest top
            Raylib.DrawRectangle(-40000, -40000, 80000, 400, new Color(40, 100, 40, 255));

            // forest bottom
            Raylib.DrawRectangle(-40000, 1000, 80000, 39000, new Color(40, 100, 40, 255));

            // desert (right)
            Raylib.DrawRectangle(4000, -40000, 36000, 80000, new Color(210, 180, 100, 255));

            // snow (left)
            Raylib.DrawRectangle(-40000, -40000, 37000, 80000, new Color(220, 235, 255, 255));

            // safe zone
            Raylib.DrawRectangle(-3000, -1500, 7000, 4000, new Color(90, 170, 90, 255));

            // main horizontal road
            Raylib.DrawRectangle(-40000, 550, 80000, 180, Color.DarkGray);

            // north highway
            Raylib.DrawRectangle(200, -40000, 120, 40000, Color.DarkGray);

            // south highway
            Raylib.DrawRectangle(200, 730, 120, 39270, Color.DarkGray);

            // desert side road
            Raylib.DrawRectangle(4000, 200, 36000, 120, Color.DarkGray);

            // snow side road
            Raylib.DrawRectangle(-40000, 200, 37000, 120, Color.DarkGray);

            // vertical road to bank
            Raylib.DrawRectangle(1020, -200, 120, 760, Color.DarkGray);

            // vertical road to store
            Raylib.DrawRectangle(-1260, -200, 120, 760, Color.DarkGray);

            // vertical road to hospital
            Raylib.DrawRectangle(200, -200, 120, 760, Color.DarkGray);

            // vertical road to weapons shop
            Raylib.DrawRectangle(500, -200, 120, 760, Color.DarkGray);

            // road markings main road
            for (int i = -40000; i < 40000; i += 200)
            {
                Raylib.DrawRectangle(i, 630, 100, 12, Color.Yellow);
            }

            // road markings north highway
            for (int i = -40000; i < 730; i += 200)
            {
                Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);
            }

            // road markings south highway
            for (int i = 730; i < 40000; i += 200)
            {
                Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);
            }

            // outer ring road - top
            Raylib.DrawRectangle(-40000, -38000, 80000, 180, Color.DarkGray);

            // outer ring road - bottom
            Raylib.DrawRectangle(-40000, 38000, 80000, 180, Color.DarkGray);

            // outer ring road - left
            Raylib.DrawRectangle(-40000, -38000, 180, 76180, Color.DarkGray);

            // outer ring road - right
            Raylib.DrawRectangle(39820, -38000, 180, 76180, Color.DarkGray);

            // snow zone vertical connectors
            Raylib.DrawRectangle(-20000, -38000, 120, 76180, Color.DarkGray);
            Raylib.DrawRectangle(-10000, -38000, 120, 76180, Color.DarkGray);

            // desert zone vertical connectors
            Raylib.DrawRectangle(15000, -38000, 120, 76180, Color.DarkGray);
            Raylib.DrawRectangle(25000, -38000, 120, 76180, Color.DarkGray);

            // ring road markings top
            for (int i = -40000; i < 40000; i += 200)
                Raylib.DrawRectangle(i, -37920, 100, 12, Color.Yellow);

            // ring road markings bottom
            for (int i = -40000; i < 40000; i += 200)
                Raylib.DrawRectangle(i, 38080, 100, 12, Color.Yellow);

            // ring road markings left
            for (int i = -38000; i < 38000; i += 200)
                Raylib.DrawRectangle(-37920, i, 12, 100, Color.Yellow);

            // ring road markings right
            for (int i = -38000; i < 38000; i += 200)
                Raylib.DrawRectangle(39900, i, 12, 100, Color.Yellow);

            // snow connector markings
            for (int i = -38000; i < 38000; i += 200)
            {
                Raylib.DrawRectangle(-19920, i, 12, 100, Color.Yellow);
                Raylib.DrawRectangle(-9920, i, 12, 100, Color.Yellow);
            }

            // desert connector markings
            for (int i = -38000; i < 38000; i += 200)
            {
                Raylib.DrawRectangle(15080, i, 12, 100, Color.Yellow);
                Raylib.DrawRectangle(25080, i, 12, 100, Color.Yellow);
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

    // wardrobe and chest always visible in My House
    if (currentBuilding.BuildingName == "MY HOUSE")
    {
        // wardrobe
        Raylib.DrawRectangle(500, 200, 60, 80, new Color((byte)80,(byte)50,(byte)20,(byte)255));
        Raylib.DrawRectangle(500, 200, 60, 10, new Color((byte)100,(byte)70,(byte)30,(byte)255));
        Raylib.DrawRectangle(527, 230, 6, 20, new Color((byte)200,(byte)160,(byte)40,(byte)255));
        Raylib.DrawText("WARDROBE", 492, 285, 16, Color.White);

        // chest
        Raylib.DrawRectangle(300, 350, 60, 50, new Color((byte)120,(byte)80,(byte)20,(byte)255));
        Raylib.DrawRectangle(300, 350, 60, 20, new Color((byte)150,(byte)100,(byte)30,(byte)255));
        Raylib.DrawRectangle(322, 362, 16, 16, new Color((byte)200,(byte)160,(byte)40,(byte)255));
        Raylib.DrawText("CHEST", 298, 406, 16, Color.White);
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
            Raylib.DrawText("E = Restore Health ($20)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "WEAPONS")
        {
            int upgradeCost = player.CombatLevel * 50;
            Raylib.DrawText($"E = Upgrade Weapon (${upgradeCost})", 20, 600, 22, Color.LightGray);
        }

        if (currentBuilding.BuildingName == "STORE")
              Raylib.DrawText("E = Open Shop", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "BANK")
            Raylib.DrawText("Z = Deposit $10 | X = Withdraw $10", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "MY HOUSE")
            Raylib.DrawText("E = Open Wardrobe | Walk to CHEST and press E", 20, 600, 22, Color.LightGray);
    }

    DrawChestUI();
    DrawWardrobe();
    DrawShopUI();
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
            Raylib.DrawText("F5 = Save", ScreenWidth - 280, 90, 18, Color.LightGray);

           Raylib.DrawRectangle(0, ScreenHeight - 34, ScreenWidth, 34, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Raylib.DrawText("SPACE = Action/Chop | R = Fish | TAB = Inventory | E = Enter Building | F = Drive Vehicle", 20, ScreenHeight - 28, 20, Color.White);

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
    if (player.BirchLogs > 0) items.Add(("Birch Logs", player.BirchLogs));
    if (player.OakLogs > 0) items.Add(("Oak Logs", player.OakLogs));
    if (player.PineLogs > 0) items.Add(("Pine Logs", player.PineLogs));
    if (player.ArcticLogs > 0) items.Add(("Arctic Logs", player.ArcticLogs));
    if (player.DeadWood > 0) items.Add(("Dead Wood", player.DeadWood));
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
          // Forest top and bottom - Oak trees
        for (int i = -30000; i < 30000; i += 250)
        {
            trees.Add(TreeObject.Oak(new Vector2(i, -300)));
            trees.Add(TreeObject.Oak(new Vector2(i, 1200)));
        }

        // Safe zone - Normal trees scattered
        for (int i = -2800; i < 3800; i += 400)
        {
            trees.Add(TreeObject.Normal(new Vector2(i, 50)));
            trees.Add(TreeObject.Normal(new Vector2(i, 900)));
        }

        // Grasslands - Birch trees
        for (int i = 4200; i < 10000; i += 300)
        {
            trees.Add(TreeObject.Birch(new Vector2(i, 200)));
            trees.Add(TreeObject.Birch(new Vector2(i, 500)));
            trees.Add(TreeObject.Birch(new Vector2(i, 800)));
        }

        // Snow zone - Pine and Arctic trees
        for (int i = -30000; i < -3100; i += 300)
        {
            trees.Add(TreeObject.Pine(new Vector2(i, 200)));
            trees.Add(TreeObject.Pine(new Vector2(i, 500)));
            trees.Add(TreeObject.Arctic(new Vector2(i + 150, 350)));
        }

        // Desert - Dead trees scattered
        for (int i = 4200; i < 30000; i += 500)
        {
            trees.Add(TreeObject.Dead(new Vector2(i, 100)));
            trees.Add(TreeObject.Dead(new Vector2(i + 200, 700)));
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
            enemies.Add(new Enemy(new Vector2(4500, 600), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(6000, 400), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(8000, 900), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(10000, 300), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(12000, 700), "Wild Dog", 3, Color.Brown));

            // Forest - Wolves
            enemies.Add(new Enemy(new Vector2(-300, -600), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(400, -800), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-200, 1400), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(500, 1600), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(5000, -700), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-5000, 1500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(10000, -500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-10000, 1200), "Wolf", 5, Color.DarkGray));

            // Desert - Scorpions
            enemies.Add(new Enemy(new Vector2(6000, 300), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(10000, 700), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(15000, 200), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(20000, 600), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(25000, 400), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));

            // Snow - Bears
            enemies.Add(new Enemy(new Vector2(-6000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-10000, 600), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-15000, 400), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-20000, 700), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-25000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
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
        public int BirchLogs = 0;
        public int OakLogs = 0;
        public int PineLogs = 0;
        public int ArcticLogs = 0;
        public int DeadWood = 0;
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
    public int Health;
    public int MaxHealth;
    public string TreeType;
    public int LevelRequired;
    public int XPReward;
    public string LogType;
    float respawnTimer = 0f;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 80);

    public static TreeObject Normal(Vector2 pos) => new TreeObject(pos, "Normal", 3, 1, 25, "Logs");
    public static TreeObject Birch(Vector2 pos) => new TreeObject(pos, "Birch", 4, 3, 40, "Birch Logs");
    public static TreeObject Oak(Vector2 pos) => new TreeObject(pos, "Oak", 5, 5, 60, "Oak Logs");
    public static TreeObject Pine(Vector2 pos) => new TreeObject(pos, "Pine", 6, 10, 80, "Pine Logs");
    public static TreeObject Arctic(Vector2 pos) => new TreeObject(pos, "Arctic", 7, 15, 100, "Arctic Logs");
    public static TreeObject Dead(Vector2 pos) => new TreeObject(pos, "Dead", 2, 1, 15, "Dead Wood");

    public TreeObject(Vector2 pos, string treeType, int health, int levelRequired, int xpReward, string logType)
    {
        Position = pos;
        TreeType = treeType;
        Health = health;
        MaxHealth = health;
        LevelRequired = levelRequired;
        XPReward = xpReward;
        LogType = logType;
    }

    public void Update(float dt)
    {
        if (Chopped)
        {
            respawnTimer += dt;
            if (respawnTimer >= 5f)
            {
                Chopped = false;
                Health = MaxHealth;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        if (Chopped)
        {
            Raylib.DrawRectangle((int)Position.X + 12, (int)Position.Y + 55, 36, 18, Color.Brown);
            return;
        }

        switch (TreeType)
        {
            case "Normal":
                // brown trunk, dark green leaves
                Raylib.DrawRectangle((int)Position.X + 20, (int)Position.Y + 40, 20, 40, Color.Brown);
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 25, 35, Color.DarkGreen);
                break;

            case "Birch":
                // white trunk, light green leaves
                Raylib.DrawRectangle((int)Position.X + 20, (int)Position.Y + 40, 20, 40, Color.White);
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 45, 4, 6, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 58, 4, 6, Color.DarkGray);
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 25, 35, new Color((byte)144,(byte)238,(byte)144,(byte)255));
                break;

            case "Oak":
                // wide dark trunk, big deep green canopy
                Raylib.DrawRectangle((int)Position.X + 18, (int)Position.Y + 40, 24, 40, new Color((byte)101,(byte)67,(byte)33,(byte)255));
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 20, 42, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                Raylib.DrawCircle((int)Position.X + 10, (int)Position.Y + 30, 28, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                Raylib.DrawCircle((int)Position.X + 50, (int)Position.Y + 30, 28, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                break;

            case "Pine":
                // thin trunk, triangular layers
                Raylib.DrawRectangle((int)Position.X + 24, (int)Position.Y + 55, 12, 25, Color.Brown);
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X, Position.Y + 45),
                    new Vector2(Position.X + 60, Position.Y + 45),
                    new Color((byte)0,(byte)80,(byte)0,(byte)255)
                );
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y + 15),
                    new Vector2(Position.X + 5, Position.Y + 55),
                    new Vector2(Position.X + 55, Position.Y + 55),
                    new Color((byte)0,(byte)100,(byte)0,(byte)255)
                );
                break;

            case "Arctic":
                // snow covered pine
                Raylib.DrawRectangle((int)Position.X + 24, (int)Position.Y + 55, 12, 25, new Color((byte)100,(byte)70,(byte)40,(byte)255));
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X, Position.Y + 45),
                    new Vector2(Position.X + 60, Position.Y + 45),
                    new Color((byte)0,(byte)60,(byte)0,(byte)255)
                );
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y + 15),
                    new Vector2(Position.X + 5, Position.Y + 55),
                    new Vector2(Position.X + 55, Position.Y + 55),
                    new Color((byte)20,(byte)80,(byte)20,(byte)255)
                );
                // snow on top
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X + 8, Position.Y + 28),
                    new Vector2(Position.X + 52, Position.Y + 28),
                    new Color((byte)220,(byte)235,(byte)255,(byte)200)
                );
                break;

            case "Dead":
                // bare grey trunk with branches
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 20, 16, 60, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 10, (int)Position.Y + 28, 22, 6, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 28, (int)Position.Y + 38, 20, 5, Color.DarkGray);
                break;
        }

        // level requirement label
        if (LevelRequired > 1)
        {
            Raylib.DrawText($"WC {LevelRequired}", (int)Position.X + 5, (int)Position.Y - 18, 16, Color.Yellow);
        }
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
