
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
        public static Player player = new Player(new Vector2(0, 650));
        static int dayOfWeek = 0; // 0-6, Monday to Sunday
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static float dayCounter = 0f; // tracks full day cycles
        static string currentBiome = "SAFE ZONE";
        static string lastBiome = "";
        static float biomeMessageTimer = 0f;
        static List<NPC> dbarTableNPCs = new();
        static NPC dbarPokieNPC = null;
        // shared
        static bool strengthMinigameActive = false;
        static string strengthMinigameType = "";
        static float strengthMinigameCooldown = 0f;

        // dumbbell game
        static float dbBarPos = 0f;
        static float dbBarDir = 1f;
        static float dbBarSpeed = 0.24f;
        static int dbConsecutiveHits = 0;

        // barbell game
        static float bbBarPos = 0f;
        static float bbBarDir = 1f;
        static float bbBarSpeed = 0.20f;
        static float bbGreenPos = 0f;
        static int bbConsecutiveHits = 0;
        static Vector2 barCounterPos = new Vector2(250, 170);
        static Vector2 pump1Pos = new Vector2(580, -700);
        static Vector2 pump2Pos = new Vector2(780, -700);
        static float pump1FuelRate = 20f;
        static float pump2FuelRate = 20f;
        static bool pump1Active = false;
        static bool pump2Active = false;
        static bool isFishing = false;
        static float fishingTimer = 0f;
        static float fishingDuration = 3f;
        static bool skillsOpen = false;
        static bool hoverWoodcutting = false;
        static bool hoverFishing = false;
        static bool hoverStrength = false;
        static bool hoverAthletics = false;
        static bool hoverDriving = false;
        static bool questsOpen = false;
        static List<Quest> quests = new();
        static string playerName = "";
        static bool nameEntered = false;
        static bool hoverCombat = false;
        static bool shopOpen = false;
        static bool shopUIOpen = false;
        static bool barMenuOpen = false;
        static int barSelectedDrink = -1;
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
        static NPC gymCounterNPC = null;
        static List<(Vector2 pos, float radius, Color color)> desertPatches = new();
        static List<(Vector2 pos, Color color)> desertRocks = new();
        static List<(Vector2 pos, float radius)> snowPatches = new();
        static List<(Vector2 pos, Color color)> snowRocks = new();
        static List<(Vector2 pos, float radius, Color color)> forestPatches = new();
        static List<(Vector2 pos, Color color)> forestMushrooms = new();
        static List<(Vector2 pos, Color color)> grasslandFlowers = new();
        static List<(Vector2 pos, float radius, Color color)> grasslandPatches = new();
        static List<Vector2> raindrops = new();
        static List<TreeObject> trees = new();
        static List<Lake> lakes = new();
        static List<NPC> npcs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();

        static List<FloatingText> floatingTexts = new();
        static List<Enemy> enemies = new();
        static List<LootDrop> lootDrops = new();
        static List<(Vector2 pos, float radius, Color color)> grassPatches = new();
        static List<(Vector2 pos, Color color)> flowers = new();
        static bool worldTextureGenerated = false;
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

        static void DrawSpeechBubble(Vector2 npcPos, string text, Color bubbleColor)
{
    int padding = 8;
    int fontSize = 14;
    int textWidth = Raylib.MeasureText(text, fontSize);
    int bubbleW = textWidth + padding * 2;
    int bubbleH = fontSize + padding * 2;

    // position bubble above the NPC head
    int bx = (int)npcPos.X + 20 - bubbleW / 2;
    int by = (int)npcPos.Y - bubbleH - 18;

    // background
    Raylib.DrawRectangle(bx, by, bubbleW, bubbleH,
        new Color((byte)255,(byte)255,(byte)255,(byte)230));
    Raylib.DrawRectangleLines(bx, by, bubbleW, bubbleH, bubbleColor);

    // tail pointing down to NPC
    Raylib.DrawTriangle(
        new Vector2(bx + bubbleW / 2 - 6, by + bubbleH),
        new Vector2(bx + bubbleW / 2 + 6, by + bubbleH),
        new Vector2(bx + bubbleW / 2,     by + bubbleH + 10),
        new Color((byte)255,(byte)255,(byte)255,(byte)230)
    );
    Raylib.DrawLine(
        bx + bubbleW / 2 - 6, by + bubbleH,
        bx + bubbleW / 2,     by + bubbleH + 10,
        bubbleColor
    );
    Raylib.DrawLine(
        bx + bubbleW / 2 + 6, by + bubbleH,
        bx + bubbleW / 2,     by + bubbleH + 10,
        bubbleColor
    );

    // text
    Raylib.DrawText(text, bx + padding, by + padding, fontSize,
        new Color((byte)20,(byte)20,(byte)20,(byte)255));
}

        static bool IsNearRoad(Vector2 pos, int buffer = 80)
        {
            // main horizontal road
            if (pos.Y >= 540 - buffer && pos.Y <= 743 + buffer) return true;

            // north/south highway
            if (pos.X >= 188 - buffer && pos.X <= 333 + buffer) return true;

            // desert side road
            if (pos.Y >= 188 - buffer && pos.Y <= 333 + buffer && pos.X >= 4000 - buffer) return true;

            // snow side road
            if (pos.Y >= 188 - buffer && pos.Y <= 333 + buffer && pos.X <= -3000 + buffer) return true;

            // ring road top
            if (pos.Y >= -38020 - buffer && pos.Y <= -37820 + buffer) return true;

            // ring road bottom
            if (pos.Y >= 38000 - buffer && pos.Y <= 38200 + buffer) return true;

            // ring road left
            if (pos.X >= -40000 - buffer && pos.X <= -39820 + buffer) return true;

            // ring road right
            if (pos.X >= 39820 - buffer && pos.X <= 40000 + buffer) return true;

            // snow vertical connectors
            if (pos.X >= -20015 - buffer && pos.X <= -19865 + buffer) return true;
            if (pos.X >= -10015 - buffer && pos.X <= -9865 + buffer) return true;

            // desert vertical connectors
            if (pos.X >= 14985 - buffer && pos.X <= 15135 + buffer) return true;
            if (pos.X >= 24985 - buffer && pos.X <= 25135 + buffer) return true;

            // vertical roads to buildings
            if (pos.X >= 1020 - buffer && pos.X <= 1140 + buffer) return true;
            if (pos.X >= -1260 - buffer && pos.X <= -1140 + buffer) return true;
            if (pos.X >= 200 - buffer && pos.X <= 320 + buffer && pos.Y <= 550 + buffer) return true;
            if (pos.X >= 500 - buffer && pos.X <= 620 + buffer && pos.Y <= 550 + buffer) return true;

            return false;
        }

        static bool IsOnRoadOrSafeZone(Vector2 pos)
        {
            // gas station forecourt
            if (pos.X >= 300 && pos.X <= 1000 && pos.Y >= -1000 && pos.Y <= -420) return true;

            // main horizontal road
            if (pos.Y >= 540 && pos.Y <= 743) return true;

            // north/south highway
            if (pos.X >= 188 && pos.X <= 333) return true;

            // desert side road
            if (pos.Y >= 188 && pos.Y <= 333 && pos.X >= 4000) return true;

            // snow side road
            if (pos.Y >= 188 && pos.Y <= 333 && pos.X <= -3000) return true;

            // ring road top
            if (pos.Y >= -38020 && pos.Y <= -37820) return true;

            // ring road bottom
            if (pos.Y >= 38000 && pos.Y <= 38200) return true;

            // ring road left
            if (pos.X >= -40000 && pos.X <= -39820) return true;

            // ring road right
            if (pos.X >= 39820 && pos.X <= 40000) return true;

            // snow vertical connectors - scoped to Y range between side road and ring road
            if (pos.X >= -20015 && pos.X <= -19865 && (pos.Y <= 188 || pos.Y >= 333)) return true;
            if (pos.X >= -10015 && pos.X <= -9865 && (pos.Y <= 188 || pos.Y >= 333)) return true;

            // desert vertical connectors - scoped to Y range
            if (pos.X >= 14985 && pos.X <= 15135 && (pos.Y <= 188 || pos.Y >= 333)) return true;
            if (pos.X >= 24985 && pos.X <= 25135 && (pos.Y <= 188 || pos.Y >= 333)) return true;

            return false;
        }

        static void GenerateSafeZoneTexture()
        {
            for (int x = -2900; x < 3900; x += 320)
            {
                for (int y = -1400; y < 2400; y += 320)
                {
                    Vector2 pos = new Vector2(x + 60, y + 60);
                    if (IsOnRoadOrSafeZone(pos)) continue;
                    float radius = 18 + (Math.Abs(x * y) % 28);
                    byte green = (byte)(110 + (Math.Abs(x + y) % 30));
                    grassPatches.Add((pos, radius, new Color((byte)55, green, (byte)55, (byte)70)));
                }
            }

            Color[] flowerColors = { Color.Red, Color.Yellow, Color.White, Color.Pink, Color.Orange, Color.Purple };
            for (int x = -2900; x < 3900; x += 260)
            {
                for (int y = -1400; y < 2400; y += 260)
                {
                    Vector2 pos = new Vector2(x + 30, y + 30);
                    if (IsOnRoadOrSafeZone(pos)) continue;
                    Color fc = flowerColors[Math.Abs(x + y) % flowerColors.Length];
                    flowers.Add((pos, fc));
                }
            }
        }

        static void GenerateBiomeTextures()
{
    // DESERT patches
    for (int x = 4000; x < 39000; x += 500)
    {
        for (int y = -39000; y < 39000; y += 500)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            if (IsNearRoad(pos)) continue;
            float radius = 14 + (Math.Abs(x * y) % 24);
            byte r = (byte)(175 + Math.Abs(x + y) % 35);
            byte g = (byte)(135 + Math.Abs(x - y) % 25);
            byte b = (byte)(55  + Math.Abs(x)     % 25);
            desertPatches.Add((pos, radius, new Color(r, g, b, (byte)90)));
        }
    }

    // DESERT rocks
    for (int x = 4000; x < 39000; x += 900)
    {
        for (int y = -39000; y < 39000; y += 900)
        {
            Vector2 pos = new Vector2(x + 150, y + 150);
            if (IsNearRoad(pos)) continue;
            byte shade = (byte)(155 + Math.Abs(x + y) % 35);
            desertRocks.Add((pos, new Color(shade, shade, (byte)Math.Max(0, shade - 25), (byte)255)));
        }
    }

    // SNOW patches
    for (int x = -39000; x < -3000; x += 500)
    {
        for (int y = -39000; y < 39000; y += 500)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            if (IsNearRoad(pos)) continue;
            float radius = 18 + (Math.Abs(x * y) % 38);
            snowPatches.Add((pos, radius));
        }
    }

    // SNOW rocks
    for (int x = -39000; x < -3000; x += 1000)
    {
        for (int y = -39000; y < 39000; y += 1000)
        {
            Vector2 pos = new Vector2(x + 200, y + 200);
            if (IsNearRoad(pos)) continue;
            byte shade = (byte)(135 + Math.Abs(x + y) % 35);
            snowRocks.Add((pos, new Color(shade, shade, (byte)Math.Min(255, shade + 12), (byte)255)));
        }
    }

    // FOREST TOP patches
    for (int x = -39000; x < 39000; x += 550)
    {
        for (int y = -39000; y < -400; y += 550)
        {
            Vector2 pos = new Vector2(x + 100, y + 100);
            if (IsNearRoad(pos)) continue;
            float radius = 22 + (Math.Abs(x * y) % 40);
            byte green = (byte)(55 + Math.Abs(x + y) % 50);
            forestPatches.Add((pos, radius, new Color((byte)18, green, (byte)18, (byte)110)));
        }
    }

    // FOREST BOTTOM patches
    for (int x = -39000; x < 39000; x += 550)
    {
        for (int y = 1000; y < 39000; y += 550)
        {
            Vector2 pos = new Vector2(x + 100, y + 100);
            if (IsNearRoad(pos)) continue;
            float radius = 22 + (Math.Abs(x * y) % 40);
            byte green = (byte)(55 + Math.Abs(x + y) % 50);
            forestPatches.Add((pos, radius, new Color((byte)18, green, (byte)18, (byte)110)));
        }
    }

    // FOREST mushrooms
    Color[] mushroomColors = { Color.Red, Color.Orange, Color.Purple, Color.White };
    for (int x = -39000; x < 39000; x += 1200)
    {
        for (int y = -39000; y < -400; y += 1200)
        {
            Vector2 pos = new Vector2(x + 300, y + 300);
            if (IsNearRoad(pos)) continue;
            forestMushrooms.Add((pos, mushroomColors[Math.Abs(x + y) % mushroomColors.Length]));
        }
        for (int y = 1000; y < 39000; y += 1200)
        {
            Vector2 pos = new Vector2(x + 300, y + 300);
            if (IsNearRoad(pos)) continue;
            forestMushrooms.Add((pos, mushroomColors[Math.Abs(x + y) % mushroomColors.Length]));
        }
    }

    // GRASSLANDS patches
    for (int x = 4000; x < 14000; x += 400)
    {
        for (int y = -400; y < 1000; y += 400)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            if (IsNearRoad(pos)) continue;
            float radius = 12 + (Math.Abs(x * y) % 24);
            byte green = (byte)(125 + Math.Abs(x + y) % 40);
            grasslandPatches.Add((pos, radius, new Color((byte)75, green, (byte)55, (byte)95)));
        }
    }

    // GRASSLANDS flowers
    Color[] flowerColors = { Color.Red, Color.Yellow, Color.White, Color.Pink, Color.Orange, Color.Purple };
    for (int x = 4000; x < 14000; x += 340)
    {
        for (int y = -400; y < 1000; y += 340)
        {
            Vector2 pos = new Vector2(x + 50, y + 50);
            if (IsNearRoad(pos)) continue;
            grasslandFlowers.Add((pos, flowerColors[Math.Abs(x + y) % flowerColors.Length]));
        }
    }
}
        static void ShowNotification(string message)
        {
            levelUpMessage = message;
            levelUpTimer = 2.5f;
        }

        static bool IsOnRoad(Vector2 pos)
{

    // gas station forecourt
    if (pos.X >= 300 && pos.X <= 1000 && pos.Y >= -1000 && pos.Y <= -420) return true;

    // main horizontal road + sidewalks
    if (pos.Y >= 540 && pos.Y <= 743) return true;

    // north/south highway + sidewalks
    if (pos.X >= 188 && pos.X <= 333) return true;

    // desert side road + sidewalks
    if (pos.Y >= 188 && pos.Y <= 333 && pos.X >= 4000) return true;

    // snow side road + sidewalks
    if (pos.Y >= 188 && pos.Y <= 333 && pos.X <= -3000) return true;

    // ring road top
    if (pos.Y >= -38020 && pos.Y <= -37820) return true;

    // ring road bottom
    if (pos.Y >= 38000 && pos.Y <= 38200) return true;

    // ring road left
    if (pos.X >= -40000 && pos.X <= -39800) return true;

    // ring road right
    if (pos.X >= 39800 && pos.X <= 40000) return true;

    // snow vertical connectors
    if (pos.X >= -20015 && pos.X <= -19865) return true;
    if (pos.X >= -10015 && pos.X <= -9865) return true;

    // desert vertical connectors
    if (pos.X >= 14985 && pos.X <= 15135) return true;
    if (pos.X >= 24985 && pos.X <= 25135) return true;

    return false;
}

        static void DrawSafeZoneTexture()
{
    float viewLeft = camera.Target.X - ScreenWidth;
    float viewRight = camera.Target.X + ScreenWidth;
    float viewTop = camera.Target.Y - ScreenHeight;
    float viewBottom = camera.Target.Y + ScreenHeight;

    foreach (var patch in grassPatches)
   {
       if (patch.pos.X < viewLeft || patch.pos.X > viewRight ||
            patch.pos.Y < viewTop || patch.pos.Y > viewBottom) continue;
        Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);
    }

    foreach (var flower in flowers)
    {
        if (flower.pos.X < viewLeft || flower.pos.X > viewRight ||
            flower.pos.Y < viewTop || flower.pos.Y > viewBottom) continue;
        Raylib.DrawCircle((int)flower.pos.X, (int)flower.pos.Y, 5, flower.color);
        Raylib.DrawCircle((int)flower.pos.X + 10, (int)flower.pos.Y + 7, 4, flower.color);
        Raylib.DrawCircle((int)flower.pos.X - 9, (int)flower.pos.Y + 5, 4, flower.color);
        Raylib.DrawCircle((int)flower.pos.X + 5, (int)flower.pos.Y - 10, 4, flower.color);
    }

    // footpaths and fence stay the same - they are always near origin so no culling needed
    Raylib.DrawRectangle(1255, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));
    Raylib.DrawRectangle(-945, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));
    Raylib.DrawRectangle(395, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));
    Raylib.DrawRectangle(715, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));
    Raylib.DrawRectangle(-345, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));
    Raylib.DrawRectangle(1755, 530, 50, 80, new Color((byte)160,(byte)160,(byte)140,(byte)200));

    Raylib.DrawRectangle(-3000, -1500, 7000, 20, new Color((byte)120,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangle(-3000, 2480, 7000, 20, new Color((byte)120,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangle(-3000, -1500, 20, 4000, new Color((byte)120,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangle(3980, -1500, 20, 4000, new Color((byte)120,(byte)80,(byte)40,(byte)255));

    for (int i = -3000; i < 4000; i += 120)
    {
        Raylib.DrawRectangle(i, -1500, 12, 30, new Color((byte)100,(byte)60,(byte)20,(byte)255));
        Raylib.DrawRectangle(i, 2470, 12, 30, new Color((byte)100,(byte)60,(byte)20,(byte)255));
    }
    for (int i = -1500; i < 2500; i += 120)
    {
        Raylib.DrawRectangle(-3000, i, 30, 12, new Color((byte)100,(byte)60,(byte)20,(byte)255));
        Raylib.DrawRectangle(3970, i, 30, 12, new Color((byte)100,(byte)60,(byte)20,(byte)255));
    }
}

static void DrawBiomeTextures()
{
    float viewLeft = camera.Target.X - ScreenWidth;
    float viewRight = camera.Target.X + ScreenWidth;
    float viewTop = camera.Target.Y - ScreenHeight;
    float viewBottom = camera.Target.Y + ScreenHeight;

    Color[] mushroomColors = { Color.Red, Color.Orange, Color.Purple, Color.White };
    Color[] flowerColors = { Color.Red, Color.Yellow, Color.White, Color.Pink, Color.Orange, Color.Purple };

    // draw manually placed items from static lists with camera culling
    foreach (var patch in grassPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    foreach (var flower in flowers)
        if (flower.pos.X > viewLeft && flower.pos.X < viewRight &&
            flower.pos.Y > viewTop && flower.pos.Y < viewBottom)
        {
            Raylib.DrawCircle((int)flower.pos.X, (int)flower.pos.Y, 5, flower.color);
            Raylib.DrawCircle((int)flower.pos.X + 10, (int)flower.pos.Y + 7, 4, flower.color);
            Raylib.DrawCircle((int)flower.pos.X - 9, (int)flower.pos.Y + 5, 4, flower.color);
            Raylib.DrawCircle((int)flower.pos.X + 5, (int)flower.pos.Y - 10, 4, flower.color);
        }

    foreach (var patch in desertPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    foreach (var rock in desertRocks)
        if (rock.pos.X > viewLeft && rock.pos.X < viewRight &&
            rock.pos.Y > viewTop && rock.pos.Y < viewBottom)
        {
            Raylib.DrawCircle((int)rock.pos.X, (int)rock.pos.Y, 8, rock.color);
            Raylib.DrawCircle((int)rock.pos.X + 10, (int)rock.pos.Y + 4, 6, rock.color);
            Raylib.DrawCircle((int)rock.pos.X - 6, (int)rock.pos.Y + 6, 5, rock.color);
        }

    foreach (var patch in snowPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius,
                new Color((byte)240, (byte)248, (byte)255, (byte)180));

    foreach (var rock in snowRocks)
        if (rock.pos.X > viewLeft && rock.pos.X < viewRight &&
            rock.pos.Y > viewTop && rock.pos.Y < viewBottom)
        {
            Raylib.DrawCircle((int)rock.pos.X, (int)rock.pos.Y, 9, rock.color);
            Raylib.DrawCircle((int)rock.pos.X + 12, (int)rock.pos.Y + 5, 7, rock.color);
            Raylib.DrawCircle((int)rock.pos.X - 7, (int)rock.pos.Y + 7, 6, rock.color);
            Raylib.DrawCircle((int)rock.pos.X, (int)rock.pos.Y - 4, 5,
                new Color((byte)240, (byte)248, (byte)255, (byte)200));
        }

    foreach (var patch in forestPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    foreach (var mushroom in forestMushrooms)
        if (mushroom.pos.X > viewLeft && mushroom.pos.X < viewRight &&
            mushroom.pos.Y > viewTop && mushroom.pos.Y < viewBottom)
        {
            Raylib.DrawRectangle((int)mushroom.pos.X - 3, (int)mushroom.pos.Y, 6, 10, Color.White);
            Raylib.DrawCircle((int)mushroom.pos.X, (int)mushroom.pos.Y, 8, mushroom.color);
            Raylib.DrawCircle((int)mushroom.pos.X - 3, (int)mushroom.pos.Y - 2, 2, Color.White);
            Raylib.DrawCircle((int)mushroom.pos.X + 3, (int)mushroom.pos.Y - 3, 2, Color.White);
        }

    foreach (var patch in grasslandPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
           patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
        Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    foreach (var flower in grasslandFlowers)
        if (flower.pos.X > viewLeft && flower.pos.X < viewRight &&
            flower.pos.Y > viewTop && flower.pos.Y < viewBottom)
        {
            Raylib.DrawCircle((int)flower.pos.X, (int)flower.pos.Y, 5, flower.color);
            Raylib.DrawCircle((int)flower.pos.X + 10, (int)flower.pos.Y + 7, 4, flower.color);
            Raylib.DrawCircle((int)flower.pos.X - 9, (int)flower.pos.Y + 5, 4, flower.color);
            Raylib.DrawCircle((int)flower.pos.X + 5, (int)flower.pos.Y - 10, 4, flower.color);
        }

}

        static void DrawStreetLight(float x, float y)
{
    // pole
    Raylib.DrawRectangle((int)x, (int)y, 6, 40, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // lamp head
    Raylib.DrawRectangle((int)x - 8, (int)y - 8, 22, 10, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    // glow at night
    float night = MathF.Sin(timeOfDay * MathF.PI);
    if (night < 0.4f)
    {
        byte glowAlpha = (byte)(180 * (1f - night / 0.4f));
        Raylib.DrawCircle((int)x + 3, (int)y - 3, 60, new Color((byte)255,(byte)220,(byte)100,(byte)(glowAlpha / 4)));
        Raylib.DrawCircle((int)x + 3, (int)y - 3, 30, new Color((byte)255,(byte)220,(byte)100,(byte)(glowAlpha / 2)));
        Raylib.DrawRectangle((int)x - 8, (int)y - 8, 22, 10, new Color((byte)255,(byte)220,(byte)100,(byte)glowAlpha));
    }
}

        static void DrawStreetLights()
    {
        // main horizontal road - north side
        for (int i = -40000; i < 40000; i += 600)
            DrawStreetLight(i, 520);

        // main horizontal road - south side
        for (int i = -40000; i < 40000; i += 600)
            DrawStreetLight(i, 740);

        // north highway - west side
        for (int i = -40000; i < 550; i += 600)
            DrawStreetLight(180, i);

        // north highway - east side
        for (int i = -40000; i < 550; i += 600)
            DrawStreetLight(326, i);

        // south highway - west side
        for (int i = 730; i < 40000; i += 600)
            DrawStreetLight(180, i);

        // south highway - east side
        for (int i = 730; i < 40000; i += 600)
            DrawStreetLight(326, i);

        // desert side road
        for (int i = 4000; i < 40000; i += 600)
            DrawStreetLight(i, 180);

        // snow side road
        for (int i = -40000; i < -3000; i += 600)
            DrawStreetLight(i, 180);

        // ring road top
        for (int i = -40000; i < 40000; i += 600)
            DrawStreetLight(i, -38020);

        // ring road bottom
        for (int i = -40000; i < 40000; i += 600)
            DrawStreetLight(i, 38020);
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
        player.StrengthLevel.ToString(),
        player.StrengthXP.ToString(),
        player.DrivingLevel.ToString(),
        player.DrivingXP.ToString(),
        player.AthleticsLevel.ToString(),
        player.AthleticsXP.ToString(),
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
    if (lines.Length > 46) player.StrengthLevel = int.Parse(lines[46]);
    if (lines.Length > 47) player.StrengthXP    = int.Parse(lines[47]);
    if (lines.Length > 48) player.DrivingLevel   = int.Parse(lines[48]);
    if (lines.Length > 49) player.DrivingXP      = int.Parse(lines[49]);
    if (lines.Length > 50) player.AthleticsLevel = int.Parse(lines[50]);
    if (lines.Length > 51) player.AthleticsXP    = int.Parse(lines[51]);
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
        else if (!Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 160, ScreenHeight - 390, 140, 335)))
        skillsOpen = false;
    }

    if (skillsOpen)
    {
        Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
        Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);
        Rectangle combatBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 230, 140, 40);
        Rectangle strengthBtn  = new Rectangle(ScreenWidth - 160, ScreenHeight - 380, 140, 40);
        Rectangle athleticsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 330, 140, 40);
        Rectangle drivingBtn   = new Rectangle(ScreenWidth - 160, ScreenHeight - 280, 140, 40);

        hoverStrength  = Raylib.CheckCollisionPointRec(mouse, strengthBtn);
        hoverAthletics = Raylib.CheckCollisionPointRec(mouse, athleticsBtn);
        hoverDriving   = Raylib.CheckCollisionPointRec(mouse, drivingBtn);
        hoverCombat = Raylib.CheckCollisionPointRec(mouse, combatBtn);
        hoverWoodcutting = Raylib.CheckCollisionPointRec(mouse, wcBtn);
        hoverFishing = Raylib.CheckCollisionPointRec(mouse, fishBtn);
    }
    else
    {
        hoverWoodcutting = false;
        hoverFishing = false;
        hoverCombat = false;
        hoverStrength  = false;
        hoverAthletics = false;
        hoverDriving   = false;
            }
}
    static void DrawSkillsUI()
{
    Rectangle skillsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 60, 140, 40);
    Raylib.DrawRectangleRec(skillsBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(skillsBtn, 2, skillsOpen ? Color.Gold : Color.White);
    Raylib.DrawText("SKILLS", ScreenWidth - 130, ScreenHeight - 48, 22, skillsOpen ? Color.Gold : Color.White);

    if (!skillsOpen) return;

    // Woodcutting
    Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
    Color wcColor = hoverWoodcutting ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(wcBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(wcBtn, 2, wcColor);
    Raylib.DrawText($"WC Lv {player.WoodcuttingLevel}", ScreenWidth - 155, ScreenHeight - 118, 20, wcColor);
    if (!hoverWoodcutting)
    {
        int wcRequired = player.WoodcuttingLevel * player.WoodcuttingLevel * 50;
        float wcProgress = (float)player.WoodcuttingXP / wcRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, (int)(140 * wcProgress), 8, Color.Green);
    }

    // Fishing
    Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);
    Color fishColor = hoverFishing ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(fishBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(fishBtn, 2, fishColor);
    Raylib.DrawText($"Fish Lv {player.FishingLevel}", ScreenWidth - 155, ScreenHeight - 168, 20, fishColor);
    if (!hoverFishing)
    {
        int fishRequired = player.FishingLevel * player.FishingLevel * 50;
        float fishProgress = (float)player.FishingXP / fishRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 143, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 143, (int)(140 * fishProgress), 8, new Color((byte)0, (byte)150, (byte)255, (byte)210));
    }

    // Combat
    Rectangle combatBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 230, 140, 40);
    Color combatColor = hoverCombat ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(combatBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(combatBtn, 2, combatColor);
    Raylib.DrawText($"Combat Lv {player.CombatLevel}", ScreenWidth - 155, ScreenHeight - 218, 20, combatColor);
    if (!hoverCombat)
    {
        int combatRequired = player.CombatLevel * player.CombatLevel * 50;
        float combatProgress = (float)player.CombatXP / combatRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, (int)(140 * combatProgress), 8, Color.Red);
    }

   Rectangle drivingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 280, 140, 40);
    Color drivingColor = hoverDriving ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(drivingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(drivingBtn, 2, drivingColor);
    Raylib.DrawText($"Drive Lv {player.DrivingLevel}", ScreenWidth - 155, ScreenHeight - 268, 20, drivingColor);
    if (!hoverDriving)
    {
        int drivingRequired = player.DrivingLevel * player.DrivingLevel * 50;
        float drivingProgress = (float)player.DrivingXP / drivingRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 243, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 243, (int)(140 * drivingProgress), 8, new Color((byte)255,(byte)200,(byte)0,(byte)255));
    }

    Rectangle athleticsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 330, 140, 40);
    Color athleticsColor = hoverAthletics ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(athleticsBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(athleticsBtn, 2, athleticsColor);
    Raylib.DrawText($"Ath Lv {player.AthleticsLevel}", ScreenWidth - 155, ScreenHeight - 318, 20, athleticsColor);
    if (!hoverAthletics)
    {
        int athleticsRequired = player.AthleticsLevel * player.AthleticsLevel * 50;
        float athleticsProgress = (float)player.AthleticsXP / athleticsRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 293, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 293, (int)(140 * athleticsProgress), 8, new Color((byte)0,(byte)200,(byte)255,(byte)255));
    }

    Rectangle strengthBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 380, 140, 40);
    Color strengthColor = hoverStrength ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(strengthBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(strengthBtn, 2, strengthColor);
    Raylib.DrawText($"Str Lv {player.StrengthLevel}", ScreenWidth - 155, ScreenHeight - 368, 20, strengthColor);
    if (!hoverStrength)
    {
        int strengthRequired = player.StrengthLevel * player.StrengthLevel * 50;
        float strengthProgress = (float)player.StrengthXP / strengthRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 343, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 343, (int)(140 * strengthProgress), 8, new Color((byte)255,(byte)80,(byte)80,(byte)255));
    }
    // XP tooltips
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
    if (hoverCombat)
    {
        int required = player.CombatLevel * player.CombatLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 230, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Raylib.DrawText($"XP: {player.CombatXP}/{required}", ScreenWidth - 315, ScreenHeight - 218, 20, Color.LightGray);
    }
    if (hoverDriving)
    {
        int required = player.DrivingLevel * player.DrivingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 280, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Raylib.DrawText($"XP: {player.DrivingXP}/{required}", ScreenWidth - 315, ScreenHeight - 268, 20, Color.LightGray);
    }
    if (hoverAthletics)
    {
        int required = player.AthleticsLevel * player.AthleticsLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 330, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Raylib.DrawText($"XP: {player.AthleticsXP}/{required}", ScreenWidth - 315, ScreenHeight - 318, 20, Color.LightGray);
    }
    if (hoverStrength)
    {
        int required = player.StrengthLevel * player.StrengthLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 380, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Raylib.DrawText($"XP: {player.StrengthXP}/{required}", ScreenWidth - 315, ScreenHeight - 368, 20, Color.LightGray);
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

                    player.Update(dt, buildings, trees, vehicles);

                    foreach (Vehicle vehicle in vehicles)
            {
                vehicle.Update(dt, buildings, trees, vehicles);
                vehicle.OnRoad = IsOnRoad(vehicle.Position);

                if (vehicle.Driving && vehicle.Fuel > 0)
                player.AddDrivingXP(1);

                if (vehicle.Driving)
{
    // sync player to vehicle every frame while driving
    player.Position = vehicle.Position;
    player.Hidden = true;

    if (Raylib.IsKeyPressed(KeyboardKey.F))
    {
        vehicle.Driving = false;
        player.Hidden = false;
        // spawn player beside the vehicle
        player.Position = new Vector2(vehicle.Position.X + 110, vehicle.Position.Y + 10);
    }
}
else
{
    if (Raylib.CheckCollisionRecs(player.Bounds, vehicle.Bounds))
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            if (vehicle.FuelLocked)
            {
                levelUpMessage = "Vehicle is locked. You must pay for fuel first!";
                levelUpTimer = 2.5f;
            }
else
{
    if (player.DrunkLevel >= 3)
    {
        levelUpMessage = "You're too munted to drive bro!";
        levelUpTimer = 2.5f;
        // push player away from vehicle so they don't get stuck
        Vector2 pushDir = Vector2.Normalize(player.Position - vehicle.Position);
        if (pushDir == Vector2.Zero) pushDir = new Vector2(1, 0);
        player.Position += pushDir * 60f;
    }
    else
    {
        vehicle.Driving = true;
        player.Hidden = true;
    }
}
        }
    }
}
            }

                    // gas pump interaction
    pump1Active = false;
    pump2Active = false;

    foreach (Vehicle vehicle in vehicles)
    {
        float distVehicleP1 = Vector2.Distance(vehicle.Position, pump1Pos);
        float distVehicleP2 = Vector2.Distance(vehicle.Position, pump2Pos);
        float distPlayerP1 = Vector2.Distance(player.Position, pump1Pos);
        float distPlayerP2 = Vector2.Distance(player.Position, pump2Pos);

        bool canFuelP1 = distVehicleP1 < 120 && (distPlayerP1 < 150 || vehicle.Driving);
        bool canFuelP2 = distVehicleP2 < 120 && (distPlayerP2 < 150 || vehicle.Driving);

        if (distVehicleP1 < 120) pump1Active = true;
        if (distVehicleP2 < 120) pump2Active = true;

        if (canFuelP1 && Raylib.IsKeyDown(KeyboardKey.R) && vehicle.Fuel < vehicle.MaxFuel)
        {
            vehicle.Refuel(pump1FuelRate * dt);
            vehicle.NeedsPayment = true;
            vehicle.FuelLocked = true;  // lock vehicle
        }

        if (canFuelP2 && Raylib.IsKeyDown(KeyboardKey.R) && vehicle.Fuel < vehicle.MaxFuel)
        {
            vehicle.Refuel(pump2FuelRate * dt);
            vehicle.NeedsPayment = true;
            vehicle.FuelLocked = true;  // lock vehicle
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
        if (Raylib.IsKeyPressed(KeyboardKey.Z))
        {
            player.DrunkLevel = 0;
            player.DrunkTimer = 0f;
            shopMessage = "You slept it off. Feeling fresh!";
            shopMessageTimer = 2f;
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

if (currentBuilding.BuildingName == "GAS STATION")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        Vehicle unpaid = vehicles.FirstOrDefault(v => v.NeedsPayment);
        if (unpaid != null)
        {
            int cost = Math.Max(1, (int)(unpaid.FuelPumped * 0.5f));
            if (player.Money >= cost)
            {
                player.Money -= cost;
                unpaid.NeedsPayment = false;
                unpaid.FuelPumped = 0f;
                unpaid.FuelLocked = false;  // unlock vehicle
                shopMessage = $"Paid ${cost} for fuel. Cheers bro!";
                shopMessageTimer = 2f;
            }
            else
            {
                shopMessage = $"Need ${cost} to pay for fuel!";
                shopMessageTimer = 1.5f;
            }
        }
        else
        {
            shopMessage = "No unpaid fuel. Drive up to a pump first!";
            shopMessageTimer = 1.5f;
        }
    }
}

if (currentBuilding.BuildingName == "GYM")
{
    if (strengthMinigameCooldown > 0) strengthMinigameCooldown -= dt;

    Vector2 dumbbellPos = new Vector2(250, 210);
    Vector2 benchPos    = new Vector2(590, 330);
    bool nearDumbbells  = Vector2.Distance(player.Position, dumbbellPos) < 120;
    bool nearBench      = Vector2.Distance(player.Position, benchPos) < 120;

    // start minigame
    if (!strengthMinigameActive && strengthMinigameCooldown <= 0
        && Raylib.IsKeyPressed(KeyboardKey.F))
    {
        if (nearDumbbells)
        {
            strengthMinigameActive = true;
            strengthMinigameType = "dumbbell";
            dbBarPos = 0f;
            dbBarDir = 1f;
            dbConsecutiveHits = 0;
            // slower cursor and bigger green per 10 strength levels
            dbBarSpeed = Math.Max(0.2f, 0.6f - (player.StrengthLevel / 10) * 0.04f);
        }
        else if (nearBench)
        {
            strengthMinigameActive = true;
            strengthMinigameType = "barbell";
            bbBarPos = 0f;
            bbBarDir = 1f;
            bbConsecutiveHits = 0;
            bbBarSpeed = Math.Max(0.2f, 0.5f - (player.StrengthLevel / 10) * 0.04f);
            // randomise first green zone position (kept away from edges so its fair)
            bbGreenPos = 0.2f + (float)(new Random().NextDouble() * 0.6f);
        }
    }

    if (strengthMinigameActive)
    {
        if (strengthMinigameType == "dumbbell")
        {
            // move cursor
            dbBarPos += dbBarDir * dbBarSpeed * dt;
            if (dbBarPos >= 1f) { dbBarPos = 1f; dbBarDir = -1f; }
            if (dbBarPos <= 0f) { dbBarPos = 0f; dbBarDir = 1f; }

            // green zone size: base 0.15, +0.02 per 10 levels
            float greenSize = 0.15f + (player.StrengthLevel / 10) * 0.02f;
            bool inGreen = dbBarPos <= greenSize || dbBarPos >= (1f - greenSize);

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (inGreen)
                {
                    dbConsecutiveHits++;
                    player.AddStrengthXP(15);
                    shopMessage = $"Rep {dbConsecutiveHits}! +15 Strength XP";
                    shopMessageTimer = 1f;
                    // speed up cursor per consecutive hit
                    dbBarSpeed += 0.24f;
                }
                else
                {
                    shopMessage = $"Failed after {dbConsecutiveHits} reps!";
                    shopMessageTimer = 1.5f;
                    strengthMinigameActive = false;
                    strengthMinigameCooldown = 0.8f;
                    dbConsecutiveHits = 0;
                }
            }
        }
        else if (strengthMinigameType == "barbell")
        {
            // move cursor
            bbBarPos += bbBarDir * bbBarSpeed * dt;
            if (bbBarPos >= 1f) { bbBarPos = 1f; bbBarDir = -1f; }
            if (bbBarPos <= 0f) { bbBarPos = 0f; bbBarDir = 1f; }

            // green zone size: base 0.12, +0.02 per 10 levels
            float greenSize = 0.12f + (player.StrengthLevel / 10) * 0.02f;
            float halfGreen = greenSize / 2f;
            bool inGreen = bbBarPos >= bbGreenPos - halfGreen 
                        && bbBarPos <= bbGreenPos + halfGreen;

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (inGreen)
                {
                    bbConsecutiveHits++;
                    player.AddStrengthXP(30);
                    shopMessage = $"Press {bbConsecutiveHits}! +30 Strength XP";
                    shopMessageTimer = 1f;
                    // speed up and move green zone to new random position
                    bbBarSpeed += 0.20f;
                    var rng = new Random();
                    bbGreenPos = 0.15f + (float)(rng.NextDouble() * 0.7f);
                }
                else
                {
                    shopMessage = $"Failed after {bbConsecutiveHits} presses!";
                    shopMessageTimer = 1.5f;
                    strengthMinigameActive = false;
                    strengthMinigameCooldown = 0.8f;
                    bbConsecutiveHits = 0;
                }
            }
        }
    }
}

        if (currentBuilding.BuildingName == "DBar")
{
if (Raylib.IsKeyPressed(KeyboardKey.E) && !barMenuOpen && Vector2.Distance(player.Position, barCounterPos) < 120)
    barMenuOpen = true;

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && barMenuOpen)
{
    barMenuOpen = false;
    barSelectedDrink = -1;
    return;
}

        if (Vector2.Distance(player.Position, barCounterPos) < 120)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Raylib.DrawText("THE BAR", 20, 630, 30, Color.Gold);
            Raylib.DrawText("E = Order a drink ($5)", 20, 670, 24, Color.White);
        }

    Vector2[] pokiePositions = {
        new Vector2(810, 245),
        new Vector2(810, 365),
        new Vector2(910, 245)
    };

    foreach (Vector2 pokiePos in pokiePositions)
    {
        if (Vector2.Distance(player.Position, pokiePos) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                if (player.Money >= 5)
                {
                    player.Money -= 5;
                    int roll = Raylib.GetRandomValue(1, 10);

                    if (roll <= 2)
                    {
                        int winnings = Raylib.GetRandomValue(10, 50);
                        player.Money += winnings;
                        shopMessage = $"JACKPOT! You won ${winnings}!";
                    }
                    else if (roll <= 5)
                    {
                        int winnings = Raylib.GetRandomValue(1, 9);
                        player.Money += winnings;
                        shopMessage = $"Small win! You got ${winnings} back.";
                    }
                    else
                    {
                        shopMessage = "No luck this time. Lost $5.";
                    }

                    shopMessageTimer = 2f;
                }
                else
                {
                    shopMessage = "Need at least $5 to play!";
                    shopMessageTimer = 1.5f;
                }
            }
        }
        // pokie machine 4 - occupied by NPC
if (Vector2.Distance(player.Position, new Vector2(910, 365)) < 80)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        shopMessage = "Oi back off! That bloke is on a winning streak bro!";
        shopMessageTimer = 2f;
    }
}
    }
    Vector2[] poolTablePositions = {
    new Vector2(240, 330),
    new Vector2(540, 330)
};

foreach (Vector2 tablePos in poolTablePositions)
{
    if (Vector2.Distance(player.Position, tablePos) < 100)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            if (player.Money >= 2)
            {
                player.Money -= 2;
                int roll = Raylib.GetRandomValue(1, 10);

                if (roll <= 3)
                {
                    shopMessage = "Nice shot! You won the round.";
                }
                else if (roll <= 6)
                {
                    shopMessage = "Close game, but you lost.";
                }
                else
                {
                    shopMessage = "Scratched on the 8 ball. Unlucky!";
                }

                shopMessageTimer = 2f;
            }
            else
            {
                shopMessage = "Need $2 to play a round!";
                shopMessageTimer = 1.5f;
            }
        }
    }
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
            player.DrunkLevel = 0;
            player.DrunkTimer = 0f;
            shopMessage = "Full health restored and sobered up for $20!";
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

if (currentBuilding.BuildingName == "GYM")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        int cost = player.CombatLevel * 25;
        if (player.Money >= cost)
        {
            player.Money -= cost;
            player.MaxHealth += 5;
            player.Health = player.MaxHealth;
            shopMessage = $"Trained hard! Max health +5. Now at {player.MaxHealth}HP";
            shopMessageTimer = 2f;
        }
        else
        {
            shopMessage = $"Need ${cost} to train!";
            shopMessageTimer = 1.5f;
        }
    }
}

if (currentBuilding.BuildingName == "MARAE")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        player.Health = player.MaxHealth;
        player.DrunkLevel = 0;
        player.DrunkTimer = 0f;
        shopMessage = "The marae restores your spirit. Fully healed and sobered up!";
        shopMessageTimer = 2f;
    }
}

if (currentBuilding.BuildingName == "POLICE STATION")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        if (player.Money >= 50)
        {
            player.Money -= 50;
            foreach (Enemy e in Program.enemies) e.Dead = true;
            shopMessage = "Police cleared all enemies! Cost $50.";
            shopMessageTimer = 2f;
        }
        else
        {
            shopMessage = "Need $50 to call in a sweep!";
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

   if (Raylib.IsKeyPressed(KeyboardKey.Q) && !wardrobeOpen && !chestOpen && !shopUIOpen && !barMenuOpen)
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
            slotSelected = false;
            playerName = "";
            nameEntered = false;
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
                    player.DrivingLevel = 99;
                    player.AthleticsLevel = 99;
                    player.StrengthLevel = 99;
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
            DrawSafeZoneTexture();
            DrawBiomeTextures();

            // sidewalk - main horizontal road top
            Raylib.DrawRectangle(-40000, 540, 80000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - main horizontal road bottom
            Raylib.DrawRectangle(-40000, 728, 80000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - north highway left
            Raylib.DrawRectangle(188, -40000, 15, 40000, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - north highway right
            Raylib.DrawRectangle(318, -40000, 15, 40000, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - south highway left
            Raylib.DrawRectangle(188, 730, 15, 39270, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - south highway right
            Raylib.DrawRectangle(318, 730, 15, 39270, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - desert side road top
            Raylib.DrawRectangle(4000, 188, 36000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - desert side road bottom
            Raylib.DrawRectangle(4000, 318, 36000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - snow side road top
            Raylib.DrawRectangle(-40000, 188, 37000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

            // sidewalk - snow side road bottom
            Raylib.DrawRectangle(-40000, 318, 37000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

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
            // forecourt road surface - same grey as roads so physics treat it normally
            Raylib.DrawRectangle(300, -1000, 700, 580, Color.DarkGray);

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
                if (Vector2.Distance(player.Position, npc.Position) < 150)
                    DrawSpeechBubble(npc.Position, npc.Dialogue,
                        new Color((byte)80,(byte)120,(byte)80,(byte)255));
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

            DrawStreetLights();

            // lane dividers
            Raylib.DrawRectangle(490, -980, 12, 540, new Color((byte)255,(byte)255,(byte)0,(byte)120));
            Raylib.DrawRectangle(690, -980, 12, 540, new Color((byte)255,(byte)255,(byte)0,(byte)120));

            // entry markings south side
            Raylib.DrawRectangle(380, -440, 80, 20, Color.Yellow);
            Raylib.DrawRectangle(580, -440, 80, 20, Color.Yellow);
            Raylib.DrawRectangle(780, -440, 80, 20, Color.Yellow);

            // canopy over pumps - transparent so vehicles visible underneath
            Raylib.DrawRectangle(340, -780, 620, 160, new Color((byte)80,(byte)80,(byte)80,(byte)60));
            Raylib.DrawRectangle(340, -780, 620, 10, new Color((byte)255,(byte)255,(byte)0,(byte)150));
            Raylib.DrawRectangle(340, -630, 620, 10, new Color((byte)255,(byte)255,(byte)0,(byte)150));

            // canopy support pillars - keep these solid so it looks grounded
            Raylib.DrawRectangle(350, -770, 20, 140, new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle(590, -770, 20, 140, new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle(830, -770, 20, 140, new Color((byte)60,(byte)60,(byte)60,(byte)255));

            // pump 1
            Raylib.DrawRectangle((int)pump1Pos.X - 18, (int)pump1Pos.Y - 35, 36, 60,
                new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle((int)pump1Pos.X - 12, (int)pump1Pos.Y - 28, 24, 36,
                pump1Active ? Color.Green : new Color((byte)200,(byte)50,(byte)50,(byte)255));
            Raylib.DrawText("PUMP 1", (int)pump1Pos.X - 24, (int)pump1Pos.Y + 30, 16, Color.White);
            Raylib.DrawText("R = Fuel", (int)pump1Pos.X - 24, (int)pump1Pos.Y + 48, 14, Color.LightGray);

            // pump 2
            Raylib.DrawRectangle((int)pump2Pos.X - 18, (int)pump2Pos.Y - 35, 36, 60,
                new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle((int)pump2Pos.X - 12, (int)pump2Pos.Y - 28, 24, 36,
                pump2Active ? Color.Green : new Color((byte)200,(byte)50,(byte)50,(byte)255));
            Raylib.DrawText("PUMP 2", (int)pump2Pos.X - 24, (int)pump2Pos.Y + 30, 16, Color.White);
            Raylib.DrawText("R = Fuel", (int)pump2Pos.X - 24, (int)pump2Pos.Y + 48, 14, Color.LightGray);
            

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

    if (currentBuilding.BuildingName == "DBar")
    {
        // bar counter
        Raylib.DrawRectangle(100, 150, 300, 40, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(100, 150, 300, 8, new Color((byte)120, (byte)70, (byte)20, (byte)255));
        Raylib.DrawText("BAR", 220, 162, 20, Color.Gold);

        // pool table 1
        Raylib.DrawRectangle(150, 280, 180, 100, new Color((byte)0, (byte)100, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(150, 280, 180, 100, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 278, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 372, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(148, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(324, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawCircle(240, 330, 6, Color.White);
        Raylib.DrawText("POOL TABLE 1", 162, 336, 14, Color.White);

        // pool table 2
        Raylib.DrawRectangle(450, 280, 180, 100, new Color((byte)0, (byte)100, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(450, 280, 180, 100, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 278, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 372, 184, 8, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(448, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawRectangle(624, 278, 8, 104, new Color((byte)80, (byte)40, (byte)10, (byte)255));
        Raylib.DrawCircle(540, 330, 6, Color.White);
        Raylib.DrawText("POOL TABLE 2", 462, 336, 14, Color.White);

        // pokies area divider
        Raylib.DrawRectangle(750, 150, 8, 400, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawText("POKIES", 820, 158, 22, Color.Gold);

        // pokie machine 1
        Raylib.DrawRectangle(780, 200, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(786, 210, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(796, 220, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(811, 220, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(826, 220, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(800, 272, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Raylib.DrawText("$", 816, 274, 14, Color.Black);

        // pokie machine 2
        Raylib.DrawRectangle(780, 320, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(786, 330, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(796, 340, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(811, 340, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(826, 340, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(800, 392, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Raylib.DrawText("$", 816, 394, 14, Color.Black);

        // pokie machine 3
        Raylib.DrawRectangle(880, 200, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(886, 210, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(896, 220, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(911, 220, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(926, 220, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(900, 272, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Raylib.DrawText("$", 916, 274, 14, Color.Black);

        // pokie machine 4
        Raylib.DrawRectangle(880, 320, 60, 90, new Color((byte)30, (byte)30, (byte)80, (byte)255));
        Raylib.DrawRectangle(886, 330, 48, 50, new Color((byte)0, (byte)0, (byte)40, (byte)255));
        Raylib.DrawRectangle(896, 340, 10, 30, new Color((byte)255, (byte)50, (byte)50, (byte)255));
        Raylib.DrawRectangle(911, 340, 10, 30, new Color((byte)50, (byte)255, (byte)50, (byte)255));
        Raylib.DrawRectangle(926, 340, 10, 30, new Color((byte)255, (byte)200, (byte)0, (byte)255));
        Raylib.DrawRectangle(900, 392, 40, 10, new Color((byte)200, (byte)160, (byte)40, (byte)255));
        Raylib.DrawText("$", 916, 394, 14, Color.Black);

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
            }

    if (currentBuilding.BuildingName != "DBar")
    {
        foreach (Rectangle obj in currentBuilding.InteriorObjects)
        {
            Raylib.DrawRectangleRec(obj, Color.DarkBrown);
        }
    }

    if (currentBuilding.BuildingName == "GYM")
{
    if (gymCounterNPC != null)
    {
        gymCounterNPC.Draw();
        if (Vector2.Distance(player.Position, gymCounterNPC.Position) < 120)
            DrawSpeechBubble(gymCounterNPC.Position, gymCounterNPC.Dialogue,
                new Color((byte)50,(byte)100,(byte)180,(byte)255));
    }
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

    if (currentBuilding.BuildingName == "GYM")
{
    // dumbbell rack - matches collision rect (180,180,140,60)
    Raylib.DrawRectangle(180, 180, 140, 60, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(185, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(215, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(245, 168, 22, 72, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawText("DUMBBELLS", 180, 248, 16, Color.White);

    // bench press - matches collision rect (480,285,220,55)
    Raylib.DrawRectangle(480, 310, 220, 40, new Color((byte)80,(byte)40,(byte)10,(byte)255));  // bench seat
    Raylib.DrawRectangle(540, 270, 100, 20, new Color((byte)60,(byte)60,(byte)60,(byte)255)); // barbell bar
    Raylib.DrawRectangle(528, 258, 24, 38, new Color((byte)40,(byte)40,(byte)40,(byte)255)); // left plate
    Raylib.DrawRectangle(628, 258, 24, 38, new Color((byte)40,(byte)40,(byte)40,(byte)255)); // right plate
    Raylib.DrawRectangle(552, 248, 16, 20, new Color((byte)30,(byte)30,(byte)30,(byte)255)); // left upright
    Raylib.DrawRectangle(612, 248, 16, 20, new Color((byte)30,(byte)30,(byte)30,(byte)255)); // right upright
    Raylib.DrawText("BENCH PRESS", 490, 358, 16, Color.White);

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
    Raylib.DrawText("RUN", tx + 38, ty - 22, 12, Color.White);
    Raylib.DrawText($"TREADMILL {t + 1}", tx + 5, ty + 50, 11, Color.White);
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
    Raylib.DrawText($"BIKE {c + 1}", cx + 18, cy + 52, 11, Color.White);
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
    Raylib.DrawText("YOGA", mx + 18, my + 12, 12, Color.White);
}

// --- COUNTER ---
Raylib.DrawRectangle(700, 150, 200, 40, new Color((byte)60,(byte)60,(byte)70,(byte)255));
Raylib.DrawRectangle(700, 150, 200, 8, new Color((byte)80,(byte)80,(byte)100,(byte)255));
Raylib.DrawRectangle(700, 150, 8, 40, new Color((byte)80,(byte)80,(byte)100,(byte)255));
Raylib.DrawRectangle(892, 150, 8, 40, new Color((byte)80,(byte)80,(byte)100,(byte)255));
// items on counter
Raylib.DrawRectangle(730, 136, 20, 14, new Color((byte)200,(byte)50,(byte)50,(byte)255));  // water bottle
Raylib.DrawRectangle(770, 138, 30, 12, new Color((byte)200,(byte)160,(byte)40,(byte)255)); // protein bar
Raylib.DrawRectangle(820, 136, 24, 14, new Color((byte)50,(byte)150,(byte)200,(byte)255)); // shaker
Raylib.DrawText("COUNTER", 730, 162, 14, Color.White);

// --- TOILETS AND SHOWER AREA ---
// toilet 1
Raylib.DrawRectangle(1310, 208, 60, 75, new Color((byte)220,(byte)220,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 208, 60, 16, new Color((byte)180,(byte)180,(byte)200,(byte)255));
Raylib.DrawEllipse(1340, 260, 22, 16, new Color((byte)200,(byte)200,(byte)215,(byte)255));
Raylib.DrawEllipseLines(1340, 260, 22, 16, new Color((byte)150,(byte)150,(byte)170,(byte)255));
Raylib.DrawText("WC", 1332, 212, 12, Color.DarkGray);

// divider
Raylib.DrawRectangle(1302, 198, 6, 90, new Color((byte)160,(byte)160,(byte)170,(byte)255));

// toilet 2
Raylib.DrawRectangle(1310, 348, 60, 75, new Color((byte)220,(byte)220,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 348, 60, 16, new Color((byte)180,(byte)180,(byte)200,(byte)255));
Raylib.DrawEllipse(1340, 400, 22, 16, new Color((byte)200,(byte)200,(byte)215,(byte)255));
Raylib.DrawEllipseLines(1340, 400, 22, 16, new Color((byte)150,(byte)150,(byte)170,(byte)255));
Raylib.DrawText("WC", 1332, 352, 12, Color.DarkGray);

// shower
Raylib.DrawRectangle(1310, 488, 80, 80, new Color((byte)180,(byte)210,(byte)230,(byte)255));
Raylib.DrawRectangle(1310, 488, 80, 8, new Color((byte)140,(byte)170,(byte)200,(byte)255));
Raylib.DrawRectangle(1310, 488, 8, 80, new Color((byte)140,(byte)170,(byte)200,(byte)255));
Raylib.DrawRectangle(1360, 493, 8, 20, new Color((byte)150,(byte)150,(byte)160,(byte)255));
Raylib.DrawRectangle(1348, 513, 32, 6, new Color((byte)150,(byte)150,(byte)160,(byte)255));
Raylib.DrawCircle(1354, 530, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Raylib.DrawCircle(1364, 534, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Raylib.DrawCircle(1374, 530, 3, new Color((byte)100,(byte)180,(byte)220,(byte)180));
Raylib.DrawText("SHOWER", 1314, 558, 12, Color.DarkGray);

Raylib.DrawText("FACILITIES", 1312, 172, 14, Color.LightGray);
    
}
   

    currentBuilding.InteriorNPC.Draw();

  if (currentBuilding.BuildingName == "DBar")
{
    foreach (NPC npc in dbarTableNPCs)
    {
        npc.Draw();
        if (Vector2.Distance(player.Position, npc.Position) < 120)
            DrawSpeechBubble(npc.Position, npc.Dialogue, 
                new Color((byte)80,(byte)40,(byte)10,(byte)255));
    }

    if (dbarPokieNPC != null)
    {
        dbarPokieNPC.Draw();
        if (Vector2.Distance(player.Position, dbarPokieNPC.Position) < 120)
            DrawSpeechBubble(dbarPokieNPC.Position, dbarPokieNPC.Dialogue,
                new Color((byte)180,(byte)50,(byte)50,(byte)255));
    }
}
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

        if (currentBuilding.BuildingName == "GYM")
{
    Vector2 dumbbellPos = new Vector2(250, 210);
    Vector2 benchPos    = new Vector2(590, 330);
    bool nearDumbbells  = Vector2.Distance(player.Position, dumbbellPos) < 120;
    bool nearBench      = Vector2.Distance(player.Position, benchPos) < 120;

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

            Raylib.DrawText($"DUMBBELLS — Rep {dbConsecutiveHits} | Str Lv {player.StrengthLevel}", 
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

            Raylib.DrawText("SPACE = Hit when cursor is in GREEN | Miss = end set", 
                190, 694, 18, Color.LightGray);
        }
        else if (strengthMinigameActive && strengthMinigameType == "barbell")
        {
            float greenSize = 0.12f + (player.StrengthLevel / 10) * 0.02f;
            int greenW = (int)(barW * greenSize);
            int greenX = barX + (int)((bbGreenPos - greenSize / 2f) * barW);
            greenX = Math.Clamp(greenX, barX, barX + barW - greenW);

            Raylib.DrawText($"BENCH PRESS — Press {bbConsecutiveHits} | Str Lv {player.StrengthLevel}", 
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

            Raylib.DrawText("SPACE = Hit when cursor is in GREEN | Miss = end set", 
                190, 694, 18, Color.LightGray);
        }
        else
        {
            // idle prompt
            if (nearDumbbells)
            {
                Raylib.DrawText("DUMBBELLS", 20, 610, 28, Color.Gold);
                Raylib.DrawText($"F = Start set | Hit greens to keep going, miss to end | Str Lv {player.StrengthLevel}", 
                    20, 650, 22, Color.White);
            }
            else if (nearBench)
            {
                Raylib.DrawText("BENCH PRESS", 20, 610, 28, Color.Gold);
                Raylib.DrawText($"F = Start set | Hit moving green target, miss to end | Str Lv {player.StrengthLevel}", 
                    20, 650, 22, Color.White);
            }
        }
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
        if (Vector2.Distance(player.Position, pokiePos) < 80)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Raylib.DrawText("POKIE MACHINE", 20, 630, 30, Color.Gold);
            Raylib.DrawText($"E = Spin ($5) | Wallet: ${player.Money}", 20, 670, 24, Color.White);
        }
    }

    // occupied pokie machine 4
if (Vector2.Distance(player.Position, new Vector2(910, 365)) < 80)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Raylib.DrawText("POKIE MACHINE", 20, 630, 30, Color.Gold);
    Raylib.DrawText("This machine is taken! That bloke won't budge.", 20, 670, 24, Color.Orange);
}

    foreach (Vector2 tablePos in poolTablePositions)
    {
        if (Vector2.Distance(player.Position, tablePos) < 100)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Raylib.DrawText("POOL TABLE", 20, 630, 30, Color.Gold);
            Raylib.DrawText($"F = Play a round ($2) | Wallet: ${player.Money}", 20, 670, 24, Color.White);
        }
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
        
        if (currentBuilding.BuildingName == "GAS STATION")
        Raylib.DrawText("E = Pay for fuel", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "GYM")
        {
            int cost = player.CombatLevel * 25;
            Raylib.DrawText($"E = Train (+5 Max HP) (${cost})", 20, 600, 22, Color.LightGray);
        }

        if (currentBuilding.BuildingName == "MARAE")
            Raylib.DrawText("E = Rest and Restore Health (Free)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "POLICE STATION")
            Raylib.DrawText("E = Call Sweep - Clear All Enemies ($50)", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "STORE")
              Raylib.DrawText("E = Open Shop", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "BANK")
            Raylib.DrawText("Z = Deposit $10 | X = Withdraw $10", 20, 600, 22, Color.LightGray);

        if (currentBuilding.BuildingName == "MY HOUSE")
        Raylib.DrawText("E = Wardrobe | E near CHEST = Storage | Z = Sleep it off", 20, 600, 22, Color.LightGray);

    }

    if (currentBuilding.BuildingName == "DBar" && barMenuOpen)
{
    string[] drinks = { "Tui", "Waikato", "Speights", "Lion Red", "Woodstock" };

    int panelX = ScreenWidth / 2 - 250;
    int panelY = 100;

    Raylib.DrawRectangle(panelX, panelY, 500, 420, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 500, 420, Color.Gold);
    Raylib.DrawText("DBAR", panelX + 200, panelY + 15, 32, Color.Gold);
    Raylib.DrawText("What are you having bro?", panelX + 80, panelY + 55, 20, Color.LightGray);
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + 170, panelY + 80, 20, Color.Gold);

    if (player.DrunkLevel > 0)
    {
        string drunkText = player.DrunkLevel switch
        {
            1 => "Feeling good...",
            2 => "Getting loose...",
            3 => "Pretty munted...",
            _ => "Absolutely gone bro"
        };
        Raylib.DrawText(drunkText, panelX + 150, panelY + 108, 18, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    }

    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < drinks.Length; i++)
    {
        Rectangle btn = new Rectangle(panelX + 40, panelY + 140 + i * 48, 420, 40);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Raylib.DrawText(drinks[i], panelX + 60, panelY + 152 + i * 48, 22, hover ? Color.Gold : Color.White);
        Raylib.DrawText("$5", panelX + 380, panelY + 152 + i * 48, 22, Color.Gold);

       if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (Vector2.Distance(player.Position, barCounterPos) < 120)
    {
        if (player.Money >= 5)
        {
            player.Money -= 5;
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

    Raylib.DrawText("Q = Close", panelX + 190, panelY + 375, 20, Color.LightGray);
}

    DrawChestUI();
    DrawWardrobe();
    DrawShopUI();
}

        static void DrawHUD()
        {
            DrawSkillsUI();
            DrawQuestsUI();

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.Driving)
                {
                    Color roadColor = vehicle.OnRoad ? Color.Green : Color.Orange;
                    string roadText = vehicle.OnRoad ? "ON ROAD" : "OFF ROAD";
                    Raylib.DrawText(roadText, ScreenWidth / 2 - 40, ScreenHeight - 60, 22, roadColor);

                    // fuel bar
                    int fbWidth = 200;
                    int fbX = ScreenWidth / 2 - fbWidth / 2;
                    int fbY = ScreenHeight - 100;
                    float fuelPercent = vehicle.Fuel / vehicle.MaxFuel;
                    Color fuelColor = fuelPercent > 0.5f ? Color.Green :
                                    fuelPercent > 0.25f ? Color.Orange : Color.Red;

                    Raylib.DrawRectangle(fbX - 60, fbY - 4, 55, 28, new Color((byte)0,(byte)0,(byte)0,(byte)180));
                    Raylib.DrawText("FUEL", fbX - 55, fbY, 20, Color.LightGray);
                    Raylib.DrawRectangle(fbX, fbY, fbWidth, 24, new Color((byte)40,(byte)40,(byte)40,(byte)220));
                    Raylib.DrawRectangle(fbX, fbY, (int)(fbWidth * fuelPercent), 24, fuelColor);
                    Raylib.DrawRectangleLines(fbX, fbY, fbWidth, 24, Color.White);

                    if (vehicle.Fuel <= 0)
                    {
                        Raylib.DrawText("OUT OF FUEL!", ScreenWidth / 2 - 70, fbY - 30, 24, Color.Red);
                    }
                    else if (vehicle.NeedsPayment)
                    {
                        Raylib.DrawText("Go inside to pay for fuel!", ScreenWidth / 2 - 120, fbY - 30, 22, Color.Yellow);
                    }
                }
            }

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
    int textWidth = Raylib.MeasureText(levelUpMessage, 40);
    Raylib.DrawText(levelUpMessage, ScreenWidth / 2 - textWidth / 2, 280, 40, new Color((byte)255, (byte)215, (byte)0, alpha));
}  

if (player.DrunkLevel > 0)
{
    string drunkText = player.DrunkLevel switch
    {
        1 => "Feeling good...",
        2 => "Getting loose...",
        3 => "Pretty munted...",
        _ => "Absolutely gone bro"
    };

    float drunkDayPercent = player.DrunkTimer / 1440f;

    // drunk status text
    int textWidth = Raylib.MeasureText(drunkText, 26);
    Raylib.DrawRectangle(ScreenWidth / 2 - textWidth / 2 - 10, 42, textWidth + 20, 34, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Raylib.DrawText(drunkText, ScreenWidth / 2 - textWidth / 2, 46, 26, new Color((byte)255, (byte)150, (byte)50, (byte)255));

    // drunk timer bar
    int dbWidth = 300;
    int dbX = ScreenWidth / 2 - dbWidth / 2;
    int dbY = 78;
    Raylib.DrawRectangle(dbX, dbY, dbWidth, 12, new Color((byte)40, (byte)40, (byte)40, (byte)220));
    Raylib.DrawRectangle(dbX, dbY, (int)(dbWidth * drunkDayPercent), 12, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    Raylib.DrawRectangleLines(dbX, dbY, dbWidth, 12, Color.White);
    Raylib.DrawText("DRUNK", dbX - 55, dbY - 2, 16, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    Raylib.DrawText("Sober up: Hospital / Marae / Sleep", dbX - 10, dbY + 16, 16, Color.LightGray);
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
            var rng = new Random(12345);

// Forest top - Oak trees
for (int i = -30000; i < 30000; i += 200)
{
    Vector2 pos = new Vector2(i + rng.Next(-80, 80), -300 + rng.Next(-200, 0));
    if (!IsNearRoad(pos)) trees.Add(TreeObject.Oak(pos));
}

// Forest bottom - Oak trees
for (int i = -30000; i < 30000; i += 200)
{
    Vector2 pos = new Vector2(i + rng.Next(-80, 80), 1200 + rng.Next(0, 200));
    if (!IsNearRoad(pos)) trees.Add(TreeObject.Oak(pos));
}

// Safe zone - Normal trees
for (int i = -2800; i < 3800; i += 300)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(-1400, 2400));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(-1400, 2400));
    if (!IsNearRoad(pos1)) trees.Add(TreeObject.Normal(pos1));
    if (!IsNearRoad(pos2)) trees.Add(TreeObject.Normal(pos2));
}

// Grasslands - Birch trees
for (int i = 4200; i < 12000; i += 250)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(350, 750));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(-350, 150));
    Vector2 pos3 = new Vector2(i + rng.Next(-80, 80), rng.Next(800, 950));
    if (!IsNearRoad(pos1)) trees.Add(TreeObject.Birch(pos1));
    if (!IsNearRoad(pos2)) trees.Add(TreeObject.Birch(pos2));
    if (!IsNearRoad(pos3)) trees.Add(TreeObject.Birch(pos3));
}

// Snow zone - Pine and Arctic trees
for (int i = -30000; i < -3100; i += 250)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(400, 530));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(750, 950));
    Vector2 pos3 = new Vector2(i + rng.Next(-80, 80), rng.Next(-350, 150));
    if (!IsNearRoad(pos1)) trees.Add(TreeObject.Pine(pos1));
    if (!IsNearRoad(pos2)) trees.Add(TreeObject.Arctic(pos2));
    if (!IsNearRoad(pos3)) trees.Add(TreeObject.Pine(pos3));
}

// Desert - Dead trees
for (int i = 4200; i < 30000; i += 400)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-100, 100), rng.Next(400, 530));
    Vector2 pos2 = new Vector2(i + rng.Next(-100, 100), rng.Next(750, 950));
    if (!IsNearRoad(pos1)) trees.Add(TreeObject.Dead(pos1));
    if (!IsNearRoad(pos2)) trees.Add(TreeObject.Dead(pos2));
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

var dbar = new Building(
    new Rectangle(1700, 410, 160, 120),
    Color.DarkBlue,
    new Color(50,60,90,255),
    new Vector2(1800,650),
    "DBar",
    new NPC(new Vector2(600,420), "Dbar Owner", "Grab a woodys and relax at Dbar.")
);

dbar.InteriorObjects.Clear();

// bar counter collision
dbar.InteriorObjects.Add(new Rectangle(100, 150, 300, 40));

// pool table 1 collision
dbar.InteriorObjects.Add(new Rectangle(148, 278, 184, 104));

// pool table 2 collision
dbar.InteriorObjects.Add(new Rectangle(448, 278, 184, 104));

// room divider collision
dbar.InteriorObjects.Add(new Rectangle(750, 150, 8, 400));

// pokie machine 1 collision
dbar.InteriorObjects.Add(new Rectangle(780, 200, 60, 90));

// pokie machine 2 collision
dbar.InteriorObjects.Add(new Rectangle(780, 320, 60, 90));

// pokie machine 3 collision
dbar.InteriorObjects.Add(new Rectangle(880, 200, 60, 90));

buildings.Add(dbar);

// table 1 - left side of bar
dbarTableNPCs.Add(new NPC(new Vector2(120, 430), "Patron", "Cheers bro!"));
dbarTableNPCs.Add(new NPC(new Vector2(165, 470), "Patron", "Another round?"));

// table 2 - middle
dbarTableNPCs.Add(new NPC(new Vector2(340, 430), "Patron", "Good times."));
dbarTableNPCs.Add(new NPC(new Vector2(385, 470), "Patron", "Sweet as."));

// table 3 - right side before barrier
dbarTableNPCs.Add(new NPC(new Vector2(560, 430), "Patron", "Nah yeah nah."));
dbarTableNPCs.Add(new NPC(new Vector2(605, 470), "Patron", "Yeah nah yeah."));

dbarPokieNPC = new NPC(new Vector2(882, 340), "Bloke", "Oi back off, I'm on a winning streak bro!");

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
    new Vector2(420,-150),
    "HOSPITAL",
    new NPC(new Vector2(600,420), "Doctor", "Kia ora! I can patch you up for $20.")
));

buildings.Add(new Building(
    new Rectangle(660, 150, 160, 120),
    new Color(80,80,80,255),
    new Color(50,50,60,255),
    new Vector2(740,250),
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

buildings.Add(new Building(
    new Rectangle(450, -1200, 260, 160),
    new Color(220, 220, 180, 255),
    new Color(200, 200, 160, 255),
    new Vector2(580, -1050),
    "GAS STATION",
    new NPC(new Vector2(600, 420), "Attendant", "Pay for your fuel here bro.")
));

var gym = new Building(
    new Rectangle(2700, 410, 160, 120),
    new Color(50, 100, 180, 255),
    new Color(40, 40, 60, 255),
    new Vector2(2800, 650),
    "GYM",
    new NPC(new Vector2(950, 125), "Trainer", "You wanna get big? Train hard every day bro.")
);

gym.InteriorObjects.Clear();

// dumbbell rack collision
gym.InteriorObjects.Add(new Rectangle(180, 180, 140, 60));

// bench press collision
gym.InteriorObjects.Add(new Rectangle(480, 285, 220, 55));

// treadmills
gym.InteriorObjects.Add(new Rectangle(180, 920, 100, 60));
gym.InteriorObjects.Add(new Rectangle(380, 920, 100, 60));
gym.InteriorObjects.Add(new Rectangle(580, 920, 100, 60));

// cycling machines - left wall, spaced 200 apart vertically
gym.InteriorObjects.Add(new Rectangle(10, 200, 80, 60));
gym.InteriorObjects.Add(new Rectangle(10, 400, 80, 60));
gym.InteriorObjects.Add(new Rectangle(10, 600, 80, 60));

// yoga mats
//gym.InteriorObjects.Add(new Rectangle(550, 480, 70, 40));
//gym.InteriorObjects.Add(new Rectangle(550, 530, 70, 40));
//gym.InteriorObjects.Add(new Rectangle(550, 580, 70, 40));

// counter
gym.InteriorObjects.Add(new Rectangle(700, 150, 200, 40));

// toilets and shower - right wall, running vertically
gym.InteriorObjects.Add(new Rectangle(1310, 200, 60, 80));   // toilet 1
gym.InteriorObjects.Add(new Rectangle(1310, 340, 60, 80));   // toilet 2
gym.InteriorObjects.Add(new Rectangle(1310, 480, 80, 80));   // shower

buildings.Add(gym);

gymCounterNPC = new NPC(new Vector2(780, 130), "Staff", "Grab a protein shake bro, $3 each.");

buildings.Add(new Building(
    new Rectangle(-1600, 410, 180, 130),
    new Color(180, 60, 40, 255),    // red/brown marae exterior
    new Color(100, 60, 30, 255),
    new Vector2(-1500, 650),
    "MARAE",
    new NPC(new Vector2(600, 420), "Kaumatua", "Haere mai, haere mai, haere mai. You are welcome here.")
));

buildings.Add(new Building(
    new Rectangle(3200, 410, 160, 120),
    new Color(30, 30, 120, 255),    // dark blue police exterior
    new Color(40, 40, 80, 255),
    new Vector2(3300, 650),
    "POLICE STATION",
    new NPC(new Vector2(600, 420), "Officer", "Keep it legal out there, no funny business.")
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

            GenerateSafeZoneTexture();
            GenerateBiomeTextures();

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
        public enum FacingDirection { Down, Up, Left, Right }
        public FacingDirection Facing = FacingDirection.Down;
        float walkTimer = 0f;
        bool walkFrame = false; // alternates legs
        bool isMoving = false;

        public bool Hidden = false;
        float speed => 300 + (AthleticsLevel * 2);

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
        public int DrivingLevel = 1;
        public int DrivingXP = 0;
        public int AthleticsLevel = 1;
        public int AthleticsXP = 0;
        public int StrengthLevel = 1;
        public int StrengthXP = 0;
        float regenTimer = 0f;
        float damageCooldown = 0f;
        public Color ShirtColor = Color.Blue;
        public Color SkinColor = Color.Beige;
        public Color PantsColor = Color.Black;
        public bool InventoryOpen = false;
        public int DrunkLevel = 0;
        public float DrunkTimer = 0f;
        public float DrunkSpeedMultiplier => DrunkLevel == 0 ? 1f : Math.Max(0.3f, 1f - (DrunkLevel * 0.15f));
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
                if (DrunkTimer > 0)
                {
                    DrunkTimer -= dt;
                    if (DrunkTimer <= 0)
                    {
                        DrunkTimer = 0;
                        DrunkLevel = 0;
                    }
                }
            }
        }

       public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> vehicles)
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
            if (!tree.Chopped && Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            {
                Position = oldPos;
            }
        }

        // player collides with parked or moving vehicles they aren't driving
        foreach (Vehicle vehicle in vehicles)
        {
            if (vehicle.Driving) continue; // skip if already driving it
            if (vehicle.FuelLocked) continue; // allow player to walk around locked vehicle
            
            if (Raylib.CheckCollisionRecs(Bounds, vehicle.Bounds))
            {
                if (!Raylib.IsKeyDown(KeyboardKey.F))
                    Position = oldPos;
            }
        }

    }

        public void UpdateInterior(float dt, List<Rectangle> objects)
{
    Vector2 move = GetInput();
    Vector2 oldPos = Position;
    Position += move * speed * dt;

    // interior boundary walls - matches the DrawRectangle(0,0,1400,1000) room
    int wallThickness = 20;
    Rectangle wallLeft   = new Rectangle(-wallThickness, 0, wallThickness, 1000);
    Rectangle wallRight  = new Rectangle(1400, 0, wallThickness, 1000);
    Rectangle wallTop    = new Rectangle(0, -wallThickness, 1400, wallThickness);
    Rectangle wallBottom = new Rectangle(0, 1000, 1400, wallThickness);

    if (Raylib.CheckCollisionRecs(Bounds, wallLeft)   ||
        Raylib.CheckCollisionRecs(Bounds, wallRight)  ||
        Raylib.CheckCollisionRecs(Bounds, wallTop)    ||
        Raylib.CheckCollisionRecs(Bounds, wallBottom))
    {
        Position = oldPos;
    }

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
    {
        move = Vector2.Normalize(move);
        AddAthleticsXP(1);

        // update facing — horizontal takes priority for side view
        if (MathF.Abs(move.X) >= MathF.Abs(move.Y))
            Facing = move.X < 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            Facing = move.Y < 0 ? FacingDirection.Up : FacingDirection.Down;

        isMoving = true;
    }
    else
    {
        isMoving = false;
    }

    return move * DrunkSpeedMultiplier;
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

        public void AddDrivingXP(int xp)
{
    if (DrivingLevel >= 100) return;

    DrivingXP += xp;

    int requiredXP = DrivingLevel * DrivingLevel * 50;

    if (DrivingXP >= requiredXP)
    {
        DrivingXP = 0;
        DrivingLevel++;
        Program.ShowLevelUp("Driving", DrivingLevel);
    }
}

public void AddAthleticsXP(int xp)
{
    if (AthleticsLevel >= 100) return;

    AthleticsXP += xp;

    int requiredXP = AthleticsLevel * AthleticsLevel * 50;

    if (AthleticsXP >= requiredXP)
    {
        AthleticsXP = 0;
        AthleticsLevel++;
        Program.ShowLevelUp("Athletics", AthleticsLevel);
    }
}

public void AddStrengthXP(int xp)
{
    if (StrengthLevel >= 100) return;
    StrengthXP += xp;
    int requiredXP = StrengthLevel * StrengthLevel * 50;
    if (StrengthXP >= requiredXP)
    {
        StrengthXP = 0;
        StrengthLevel++;
        Program.ShowLevelUp("Strength", StrengthLevel);
    }
}


   public void Draw()
{
    if (Hidden) return;

    // update walk animation timer
    if (isMoving)
    {
        walkTimer += Raylib.GetFrameTime() * 8f;
        if (walkTimer >= 1f)
        {
            walkTimer = 0f;
            walkFrame = !walkFrame;
        }
    }
    else
    {
        walkTimer = 0f;
        walkFrame = false;
    }

    int x = (int)Position.X;
    int y = (int)Position.Y;

    switch (Facing)
    {
        case FacingDirection.Down:
            DrawFacingDown(x, y);
            break;
        case FacingDirection.Up:
            DrawFacingUp(x, y);
            break;
        case FacingDirection.Left:
            DrawFacingLeft(x, y);
            break;
        case FacingDirection.Right:
            DrawFacingRight(x, y);
            break;
    }
}

void DrawFacingDown(int x, int y)
{
    // head
    Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);
    // eyes
    Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
    Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);
    // mouth
    Raylib.DrawRectangle(x + 15, y + 17, 10, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));
    // body/shirt
    Raylib.DrawRectangle(x + 10, y + 24, 20, 30, ShirtColor);
    // arms - swing when walking
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;
    Raylib.DrawRectangle(x + 2,  y + 26 + armSwing, 8, 18, SkinColor);  // left arm
    Raylib.DrawRectangle(x + 30, y + 26 - armSwing, 8, 18, SkinColor);  // right arm
    // legs - alternate
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 10, y + 54,     8, 16, PantsColor); // left forward
            Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 16, PantsColor); // right back
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 16, PantsColor); // left back
            Raylib.DrawRectangle(x + 22, y + 54,     8, 16, PantsColor); // right forward
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 10, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 22, y + 54, 8, 12, PantsColor);
    }
}

void DrawFacingUp(int x, int y)
{
    // head (back of head - slightly darker)
    Raylib.DrawCircle(x + 20, y + 12, 12, 
        new Color((byte)Math.Max(0, SkinColor.R - 30),
                  (byte)Math.Max(0, SkinColor.G - 30),
                  (byte)Math.Max(0, SkinColor.B - 30), (byte)255));
    // hair/top of head detail
    Raylib.DrawRectangle(x + 10, y + 2, 20, 8, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    // body/shirt back
    Raylib.DrawRectangle(x + 10, y + 24, 20, 30, ShirtColor);
    // arms swing
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;
    Raylib.DrawRectangle(x + 2,  y + 26 + armSwing, 8, 18, SkinColor);
    Raylib.DrawRectangle(x + 30, y + 26 - armSwing, 8, 18, SkinColor);
    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 10, y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 10, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 22, y + 54, 8, 12, PantsColor);
    }
}

void DrawFacingLeft(int x, int y)
{
    // head - side profile
    Raylib.DrawCircle(x + 18, y + 12, 12, SkinColor);
    // eye
    Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
    // nose
    Raylib.DrawRectangle(x + 8, y + 14, 4, 3, 
        new Color((byte)Math.Max(0, SkinColor.R - 20),
                  (byte)Math.Max(0, SkinColor.G - 20),
                  (byte)Math.Max(0, SkinColor.B - 20), (byte)255));
    // body - narrower side view
    Raylib.DrawRectangle(x + 12, y + 24, 14, 30, ShirtColor);
    // visible arm (front arm swings)
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;
    Raylib.DrawRectangle(x + 10, y + 26 + armSwing, 8, 16, SkinColor);
    // legs - side walk cycle
    if (isMoving)
    {
        if (walkFrame)
        {
            // left leg forward (toward screen left)
            Raylib.DrawRectangle(x + 8,  y + 54,      8, 16, PantsColor);
            Raylib.DrawRectangle(x + 16, y + 54 - 8,  8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 8,  y + 54 - 8,  8, 16, PantsColor);
            Raylib.DrawRectangle(x + 16, y + 54,       8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 8,  y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 16, y + 54, 8, 12, PantsColor);
    }
}

void DrawFacingRight(int x, int y)
{
    // head - side profile (mirrored)
    Raylib.DrawCircle(x + 22, y + 12, 12, SkinColor);
    // eye
    Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);
    // nose
    Raylib.DrawRectangle(x + 28, y + 14, 4, 3,
        new Color((byte)Math.Max(0, SkinColor.R - 20),
                  (byte)Math.Max(0, SkinColor.G - 20),
                  (byte)Math.Max(0, SkinColor.B - 20), (byte)255));
    // body
    Raylib.DrawRectangle(x + 14, y + 24, 14, 30, ShirtColor);
    // visible arm
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;
    Raylib.DrawRectangle(x + 22, y + 26 + armSwing, 8, 16, SkinColor);
    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 16, y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 24, y + 54 - 8, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 24, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 16, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 24, y + 54, 8, 12, PantsColor);
    }
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
        public bool OnRoad = false;
        public void Refuel() { speed = 1200f; }
        public float Fuel = 100f;
        public float MaxFuel = 100f;
        public bool NeedsPayment = false;
        public float FuelPumped = 0f;
        public bool FuelLocked = false;
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

        public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> allVehicles)
        {
            if (!Driving) return;
            if (FuelLocked)
            {
                velocity = Vector2.Zero;
                return;
            }
            Vector2 move = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.Up)) move.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.Down)) move.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.Left)) move.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

            if (move != Vector2.Zero)
                move = Vector2.Normalize(move);

            if (move != Vector2.Zero && Fuel > 0)
                Fuel = Math.Max(0, Fuel - dt * 2f);

            if (Fuel <= 0) move = Vector2.Zero;

            float speedMultiplier = OnRoad ? 1f + (Program.player.DrivingLevel * 0.01f) : 0.4f;
            Vector2 targetVelocity = move * speed * speedMultiplier;
            velocity = Vector2.Lerp(velocity, targetVelocity, dt * (OnRoad ? 5f : 2f));

            Vector2 oldPos = Position;
            Position += velocity * dt;

            // building collision
            foreach (Building building in buildings)
            {
                Rectangle collisionBox = new Rectangle(
                    building.Bounds.X,
                    building.Bounds.Y,
                    building.Bounds.Width,
                    building.Bounds.Height
                );

                if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
                {
                    Position = oldPos;
                    velocity = Vector2.Zero;
                }
            }

            // tree collision
            foreach (TreeObject tree in trees)
            {
                if (!tree.Chopped && Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
                {
                    Position = oldPos;
                    velocity *= -0.3f; // slight bounce
                }
            }

            // vehicle to vehicle collision
            foreach (Vehicle other in allVehicles)
            {
                if (other == this) continue;

                if (Raylib.CheckCollisionRecs(Bounds, other.Bounds))
                {
                    Position = oldPos;
                    velocity *= -0.5f;
                }
            }
        }
        public void Refuel(float amount)
        {
            float actualAmount = Math.Min(amount, MaxFuel - Fuel);
            FuelPumped += actualAmount;
            Fuel = Math.Min(MaxFuel, Fuel + actualAmount);
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
