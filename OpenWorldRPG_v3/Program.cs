
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace OpenWorldRPG
{
    enum SceneState
    {
        MainMenu,
        World,
        Building,
        Minigame
    }

              public enum SlotSymbol
{
    Cherry,
    Lemon,
    Bell,
    Star,
    Seven
}

public static class SlotData
{
    // Higher weight = more common. Seven is rare = jackpot symbol.
    public static readonly (SlotSymbol symbol, int weight)[] SymbolWeights =
    {
        (SlotSymbol.Cherry, 40),
        (SlotSymbol.Lemon, 30),
        (SlotSymbol.Bell, 18),
        (SlotSymbol.Star, 9),
        (SlotSymbol.Seven, 3)
    };

    // Multiplier applied to the bet when you land 3-in-a-row of a symbol
    public static readonly Dictionary<SlotSymbol, int> ThreeMatchMultiplier = new()
    {
        { SlotSymbol.Cherry, 2 },
        { SlotSymbol.Lemon, 3 },
        { SlotSymbol.Bell, 5 },
        { SlotSymbol.Star, 10 },
        { SlotSymbol.Seven, 25 } // jackpot
    };

    // Multiplier for 2-in-a-row (any symbol, smaller payout)
    public const float TwoMatchMultiplier = 1.0f;

    public static SlotSymbol GetRandomSymbol()
    {
        int totalWeight = 0;
        foreach (var (_, weight) in SymbolWeights) totalWeight += weight;

        int roll = Raylib.GetRandomValue(1, totalWeight);
        int cumulative = 0;

        foreach (var (symbol, weight) in SymbolWeights)
        {
            cumulative += weight;
            if (roll <= cumulative) return symbol;
        }

        return SlotSymbol.Cherry; // fallback, shouldn't hit
    }

    public static Color GetSymbolColor(SlotSymbol s) => s switch
    {
        SlotSymbol.Cherry => Color.Red,
        SlotSymbol.Lemon => Color.Yellow,
        SlotSymbol.Bell => Color.Gold,
        SlotSymbol.Star => Color.SkyBlue,
        SlotSymbol.Seven => Color.Magenta,
        _ => Color.White
    };

    public static string GetSymbolText(SlotSymbol s) => s switch
    {
        SlotSymbol.Cherry => "CHERRY",
        SlotSymbol.Lemon => "LEMON",
        SlotSymbol.Bell => "BELL",
        SlotSymbol.Star => "STAR",
        SlotSymbol.Seven => "7",
        _ => "?"
    };
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
        public static Player player = new Player(new Vector2(-1917, -9720));
        static int dayOfWeek = 0; // 0-6, Monday to Sunday
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static float dayCounter = 0f; // tracks full day cycles
        static string currentBiome = "SAFE ZONE";
        static string lastBiome = "";
        static float biomeMessageTimer = 0f;

        //Toolbelt
        static int toolbarSelectedSlot = 0;
        static string[] toolbarSlots = new string[8]; // null = empty
        static bool torchActive = false;
        static List<NPC> dbarTableNPCs = new();
        public static PokieMachine activePokieMachine = new PokieMachine();
        enum MinigameType { None, Pokie, Pool }
        static MinigameType activeMinigameType = MinigameType.None;
        static PoolGame activePoolGame = new PoolGame();
        static NPC dbarPokieNPC = null;
        // Dealer UI state
        enum DealerType { None, Bike, Barn, Car }
        static bool dealerUIOpen = false;
        static DealerType currentDealerType = DealerType.None;
        static Building currentDealerBuilding = null;
        static int dealerSelectedIndex = 0;
        static int dealerScrollOffset = 0; // for paging if >4 options
        public static List<RockObject> rocks = new();
        static List<Vehicle> dealerVehicleOptions = new();
        static List<Rideable> dealerBikeOptions = new();
        static List<Rideable> dealerBarnOptions = new(); // horses as barn animals
        public static FenceManager fenceManager = new FenceManager();

        //Tools in toolbelt
        static Vector2 axePosition = new Vector2(-2954, -9938);
        static bool axePickedUp = false;
        static Vector2 pickaxePosition = new Vector2(-2898, -6228);
        static bool pickaxePickedUp = false;
        static Vector2 fishingRodPosition = new Vector2(-2623, -8396);  // near a lake
        static bool fishingRodPickedUp = false;
        static Vector2 fishingNetPosition = new Vector2(750, 1300);
        static bool fishingNetPickedUp = false;
        static Vector2 torchPosition = new Vector2(-900, 500);
        static bool torchPickedUp = false;
        static Vector2 stickPosition = new Vector2(-284, -6179);
        static bool stickPickedUp = false;
        static Vector2 swordPosition = new Vector2(-1170, -7740);
        static bool swordPickedUp = false;

        // Armor Menu
        static bool armorMenuOpen = false;

        // Armor slots — null = empty
        public static string armorHelmet = null;
        public static string armorBody   = null;
        public static string armorLegs   = null;
        public static string armorBoots  = null;
        public static string armorGloves = null;
        public static string armorCape   = null;
        public static string armorWeapon = null;
        public static string armorShield = null;

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
        static bool isFishing = false;
        static float fishingTimer = 0f;
        static float fishingDuration = 3f;
        static bool skillsOpen = false;
        static bool hoverWoodcutting = false;
        static bool hoverMining = false;
        static bool hoverFishing = false;
        static bool hoverStrength = false;
        static bool hoverAthletics = false;
        static bool hoverDriving = false;
        static bool questsOpen = false;
        static List<GasStation> gasStations = new();
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
        //Sound
        static Music musicMainMenu;
        static Music musicForest;
        static Music musicSnow;
        static Music musicBeach;
        static Music musicDesert;
        static Music musicRain;
        static Music musicSafezone;
        static Music musicFarm;
        static Music musicTakeaways;
        static Music musicDbar;
        static Music musicHouse;
        static Music currentMusic;
        static Music lastZoneMusic;
        static Music musicBeforeRain = default;
        static Sound soundTreeChop;
        static Sound soundTreeFall;
        static Sound soundRockHit;
        static Sound soundRockBreak;
        static Sound soundDoorOpen;
        static Sound soundDoorClose;
        static Sound soundVehicleDrive;
        static Sound soundBikeDrive;
        public static Sound soundHorseGallop;
        static Sound soundPlayerWalk;
        static Sound soundPauseOpen;
        static Sound soundPauseClose;
        static Sound soundSwordSwing;
        static Sound soundStickSwing;
        static Sound soundDogHit;
        static Sound soundDogDie;

        static float musicFadeTimer = 0f;
        static float musicFadeDuration = 2.0f; // seconds to fade out
        static Music pendingMusic = default;
        static bool isFadingOut = false;
        static bool musicLoaded = false;
        static float musicVolume = 0.6f; // 0..1
        public static float soundVolume = 0.7f;
        static bool musicPlaying = false;
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
        static int chestDogFangs = 0;

        static int chestSelectedSlot = -1;
        static bool isLoadingGame = false;
        static bool slotSelected = false;
        static NPC gymCounterNPC = null;
        static bool trolleyPickedUp = false;
        static bool basketPickedUp = false;
        static List<string> trolleyInventory = new List<string>(new string[20]);
        static List<string> basketInventory = new List<string>(new string[10]);
        static int trolleySelectedSlot = -1;
        static int basketSelectedSlot = -1;
        static bool supermarketInventoryOpen = false;
        // calendar
        static int dayOfMonth = 1;                 // 1..14
        static int currentMonth = 0;               // 0..11 -> January..December
        static string[] monthNames = {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };
        static string[] seasons = { "Summer", "Autumn", "Winter", "Spring" };
        // map month -> season index (example southern hemisphere ordering; adjust if you want northern)
        static int[] monthToSeason = {
            0, // Jan -> Summer
            0, // Feb -> Summer
            1, // Mar -> Autumn
            1, // Apr -> Autumn
            1, // May -> Autumn
            2, // Jun -> Winter
            2, // Jul -> Winter
            2, // Aug -> Winter
            3, // Sep -> Spring
            3, // Oct -> Spring
            3, // Nov -> Spring
            0  // Dec -> Summer
        };

        static List<(Vector2 pos, float radius, Color color)> desertPatches = new();
        static List<(Vector2 pos, Color color)> desertRocks = new();
        static List<(Vector2 pos, float radius)> snowPatches = new();
        static List<(Vector2 pos, Color color)> snowRocks = new();
        static List<(Vector2 pos, float radius, Color color)> forestPatches = new();
        static List<(Vector2 pos, Color color)> forestMushrooms = new();
        static List<(Vector2 pos, Color color)> grasslandFlowers = new();
        static List<(Vector2 pos, float radius, Color color)> grasslandPatches = new();
        static List<(Vector2 pos, float radius, Color color)> oceanPatches = new();
        static List<(Vector2 pos, Color color)> oceanCoral = new();
        static List<(Vector2 pos, Color color)> oceanShells = new();
        static List<(Vector2 pos, float radius, Color color)> swampPatches = new();
        static List<(Vector2 pos, Color color)> swampReeds = new();
        static List<(Vector2 pos, Color color)> swampLilies = new();
        static List<(Vector2 pos, float radius, Color color)> volcanoPatches = new();
        static List<(Vector2 pos, Color color)> lavaVents = new();
        static List<(Vector2 pos, float radius, Color color)> mountainPatches = new();
        static List<(Vector2 pos, Color color)> mountainTrees = new();
        static List<Vector2> raindrops = new();
        static List<TreeObject> trees = new();
        static List<Lake> lakes = new();
        static List<NPC> npcs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();
        static List<Rideable> rideables = new();

        static List<FloatingText> floatingTexts = new();
        static List<Enemy> enemies = new();
        static List<LootDrop> lootDrops = new();
        static List<(Vector2 pos, float radius, Color color)> grassPatches = new();
        static List<(Vector2 pos, Color color)> flowers = new();
        
        static bool worldTextureGenerated = false;
        static Building currentBuilding = null;
        static float shakeDuration = 0f;
        static float shakeMagnitude = 6f;
        // ── SWIMMING ──
        static bool swimmingActive = false;
        static string swimmingPoolType = ""; // "lane" or "diving"
        static float swimLapTimer = 0f;
        static float swimLapDuration = 4f;
        static int swimLapsCompleted = 0;
        static bool divingActive = false;
        static float divingBarPos = 0f;
        static float divingBarDir = 1f;
        static float divingBarSpeed = 0.4f;
        static int divingScore = 0;
        static bool divingJumped = false;
        static float divingFallTimer = 0f;
        static string divingResult = "";
        static float divingResultTimer = 0f;

        // ── TENNIS ──
        static bool tennisActive = false;
        static Vector2 tennisBallPos = new Vector2(400, 300);
        static Vector2 tennisBallVel = new Vector2(200, 150);
        static float tennisPlayerPaddleY = 270f;
        static float tennisAIPaddleY = 270f;
        static int tennisPlayerScore = 0;
        static int tennisAIScore = 0;
        static float tennisSwingCooldown = 0f;
        static bool tennisServing = true;
        static float tennisMessageTimer = 0f;
        static string tennisMessage = "";

        // ── BASKETBALL ──
        static bool basketballActive = false;
        static float bbPower = 0f;
        static float bbPowerDir = 1f;
        static float bbAimX = 0f;
        static float bbAimDir = 1f;
        static bool bbShooting = false;
        static float bbShotTimer = 0f;
        static int bbScore = 0;
        static int bbAttempts = 0;
        static string bbMessage = "";
        static float bbMessageTimer = 0f;
        static bool bbPowerLocked = false;

        // ── MCDONALD'S ──
        static bool mcdonaldsMenuOpen = false;
        static int mcdonaldsSelectedItem = -1;
        static string mcdonaldsMessage = "";
        static float mcdonaldsMessageTimer = 0f;
        static float mcdonaldsOrderTimer = 0f;
        static bool mcdonaldsOrderReady = false;
        static string mcdonaldsOrderName = "";

        // DOMINOES
          static bool dominosMenuOpen = false;
          static string dominosOrderName = "";
          static bool dominosOrderReady = false;
          static float dominosOrderTimer = 0f;
          static string dominosMessage = "";
          static float dominosMessageTimer = 0f;
          static int dominosSelectedItem = -1;

          //KFC
          static bool kfcMenuOpen = false;
          static string kfcOrderName = "";
          static bool kfcOrderReady = false;
          static float kfcOrderTimer = 0f;
          static string kfcMessage = "";
          static float kfcMessageTimer = 0f;
          static int kfcSelectedItem = -1;

          //BURGER KING
          static bool burgerKingMenuOpen = false;
          static string burgerKingOrderName = "";
          static bool burgerKingOrderReady = false;
          static float burgerKingOrderTimer = 0f;
          static string burgerKingMessage = "";
          static float burgerKingMessageTimer = 0f;
          static int burgerKingSelectedItem = -1;

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

 static void SwitchMusic(Music newTrack)
{
    if (!musicLoaded) return;
    if (newTrack.Stream.Buffer == currentMusic.Stream.Buffer) return; // already playing

    pendingMusic = newTrack;
    isFadingOut = true;
    musicFadeTimer = musicFadeDuration;
}

static void CheckZoneMusic()
{
    Music zoneMusic;

    // Define your zones by coordinate ranges
    if (player.Position.X > -3000 && player.Position.X < 0 &&
        player.Position.Y > -10000 && player.Position.Y < -6000)
    {
        zoneMusic = musicFarm;
    }
    else if (player.Position.X > -6100 && player.Position.X < 4000 &&
             player.Position.Y > -12000 && player.Position.Y < -5000)
    {
        zoneMusic = musicForest;
    }
    else
    {
        zoneMusic = musicMainMenu; // default overworld track
    }

    // Only switch if the zone has changed
    if (zoneMusic.Stream.Buffer != lastZoneMusic.Stream.Buffer)
    {
        SwitchMusic(zoneMusic);
        lastZoneMusic = zoneMusic;
    }
}

static void UpdateMusicFade(float dt)
{
    if (!isFadingOut) return;

    musicFadeTimer -= dt;
    float volume = musicVolume * (musicFadeTimer / musicFadeDuration); // ramp down
    Raylib.SetMusicVolume(currentMusic, Math.Max(0, volume));

    if (musicFadeTimer <= 0)
    {
        // Fade complete, switch track
        Raylib.StopMusicStream(currentMusic);
        currentMusic = pendingMusic;
        Raylib.SetMusicVolume(currentMusic, musicVolume); // restore full volume
        Raylib.PlayMusicStream(currentMusic);
        isFadingOut = false;
    }
}
static bool IsTwoHandedWeapon(string weapon)
{
    if (weapon == null) return false;
    return weapon == "Great Sword" || weapon == "War Axe" || weapon == "Battle Staff";
    // add any future 2H weapons here
}
static int GetTotalDefense()
{
    int def = 0;
    if (armorHelmet != null) def += GetArmorDefense(armorHelmet);
    if (armorBody   != null) def += GetArmorDefense(armorBody);
    if (armorLegs   != null) def += GetArmorDefense(armorLegs);
    if (armorBoots  != null) def += GetArmorDefense(armorBoots);
    if (armorGloves != null) def += GetArmorDefense(armorGloves);
    if (armorCape   != null) def += GetArmorDefense(armorCape);
    if (armorShield != null) def += GetArmorDefense(armorShield);
    return def;
}

static int GetArmorDefense(string item)
{
    return item switch
    {
        // helmets
        "Leather Cap"     => 1,
        "Iron Helmet"     => 3,
        "Steel Helmet"    => 5,
        // body
        "Leather Vest"    => 2,
        "Iron Chestplate" => 5,
        "Steel Chestplate"=> 8,
        // legs
        "Leather Pants"   => 1,
        "Iron Leggings"   => 3,
        "Steel Leggings"  => 5,
        // boots
        "Leather Boots"   => 1,
        "Iron Boots"      => 2,
        "Steel Boots"     => 3,
        // gloves
        "Leather Gloves"  => 1,
        "Iron Gauntlets"  => 2,
        "Steel Gauntlets" => 3,
        // cape
        "Wool Cape"       => 1,
        "Magic Cape"      => 3,
        // shields
        "Wooden Shield"   => 2,
        "Iron Shield"     => 4,
        "Steel Shield"    => 6,
        // weapons (weapon slot armor value = 0, damage handled separately)
        _ => 0
    };
}

static int GetWeaponDamage(string weapon)
{
    return weapon switch
    {
        "Stick"       => 0,
        "Sword"       => 5,
        "Great Sword" => 12,
        "War Axe"     => 10,
        "Battle Staff" => 8,
        _             => 0
    };
}
static bool AddToToolbar(string item)
{
    for (int i = 0; i < toolbarSlots.Length; i++)
    {
        if (toolbarSlots[i] == null)
        {
            toolbarSlots[i] = item;
            return true;
        }
    }
    ShowNotification("Toolbar is full!");
    return false;
}

static void DrawToolbar()
{
    int slotSize = 56;
    int padding = 6;
    int totalW = 8 * (slotSize + padding) - padding;
    int startX = 20;
    int startY = ScreenHeight - slotSize - 20;

    for (int i = 0; i < 8; i++)
    {
        int sx = startX + i * (slotSize + padding);
        bool selected = i == toolbarSelectedSlot;

        // slot background
        Raylib.DrawRectangle(sx, startY, slotSize, slotSize,
            new Color((byte)20,(byte)20,(byte)30,(byte)210));
        Raylib.DrawRectangleLines(sx, startY, slotSize, slotSize,
            selected ? Color.Gold : new Color((byte)80,(byte)80,(byte)100,(byte)255));

        // selected glow
        if (selected)
            Raylib.DrawRectangle(sx, startY, slotSize, 3, Color.Gold);

        // slot number
        Raylib.DrawText($"{i + 1}", sx + 4, startY + 3, 13,
            new Color((byte)120,(byte)120,(byte)140,(byte)255));

        // draw item icon if occupied
        if (toolbarSlots[i] != null)
        {
            DrawToolbarIcon(toolbarSlots[i], sx, startY, slotSize);
            // item name below icon
            int nameW = Raylib.MeasureText(toolbarSlots[i], 11);
            Raylib.DrawText(toolbarSlots[i],
                sx + slotSize / 2 - nameW / 2,
                startY + slotSize - 14, 11,
                selected ? Color.Gold : Color.LightGray);
        }
    }

    // tooltip — show equipped tool name above toolbar
    if (toolbarSlots[toolbarSelectedSlot] != null)
    {
        string equipped = toolbarSlots[toolbarSelectedSlot];
        int tw = Raylib.MeasureText($"Equipped: {equipped}", 18);
        Raylib.DrawRectangle(startX, startY - 28, tw + 12, 24,
            new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Raylib.DrawText($"Equipped: {equipped}", startX + 6, startY - 24, 18, Color.Gold);
    }
}

static void DrawToolbarIcon(string tool, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2 - 6;

    switch (tool)
    {
        case "Axe":
            Raylib.DrawRectangle(cx - 2, cy - 14, 5, 28,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx + 3, cy - 14),
                new Vector2(cx + 16, cy - 20),
                new Vector2(cx + 14, cy - 2),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            break;

        case "Pickaxe":
            Raylib.DrawRectangle(cx - 2, cy - 12, 5, 26,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLine(cx - 12, cy - 14, cx + 12, cy - 14,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx - 12, cy - 14),
                new Vector2(cx - 16, cy - 6),
                new Vector2(cx - 4,  cy - 14),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx + 12, cy - 14),
                new Vector2(cx + 16, cy - 6),
                new Vector2(cx + 4,  cy - 14),
                new Color((byte)140,(byte)140,(byte)150,(byte)255));
            break;

        case "Rod":
            Raylib.DrawRectangle(cx - 2, cy - 16, 4, 30,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLine(cx + 2, cy - 16, cx + 16, cy,
                new Color((byte)200,(byte)200,(byte)200,(byte)255));
            Raylib.DrawCircle(cx + 16, cy, 3,
                new Color((byte)100,(byte)180,(byte)220,(byte)255));
            break;

        case "Net":
            for (int i = 0; i < 3; i++)
                Raylib.DrawLine(cx, cy - 14, cx - 14 + i * 14, cy + 8,
                    new Color((byte)180,(byte)160,(byte)80,(byte)255));
            for (int i = 0; i < 2; i++)
                Raylib.DrawLine(cx - 14, cy - 6 + i * 10, cx + 14, cy - 6 + i * 10,
                    new Color((byte)180,(byte)160,(byte)80,(byte)255));
            break;

        case "Torch":
            Raylib.DrawRectangle(cx - 3, cy - 8, 6, 22,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawCircle(cx, cy - 10, 5,
                new Color((byte)255,(byte)180,(byte)0,(byte)255));
            Raylib.DrawCircle(cx, cy - 10, 8,
                new Color((byte)255,(byte)100,(byte)0,(byte)80));
            break;

        case "Stick":
            Raylib.DrawRectangle(cx - 2, cy - 16, 5, 32,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - 1, cy - 18, 3, 5,
                new Color((byte)100,(byte)60,(byte)20,(byte)255));
            break;

        case "Sword":
            // blade
            Raylib.DrawRectangle(cx - 2, cy - 18, 5, 26,
                new Color((byte)180,(byte)190,(byte)200,(byte)255));
            // tip
            Raylib.DrawTriangle(
                new Vector2(cx - 2, cy - 18),
                new Vector2(cx + 3, cy - 18),
                new Vector2(cx,     cy - 26),
                new Color((byte)200,(byte)210,(byte)220,(byte)255));
            // crossguard
            Raylib.DrawRectangle(cx - 10, cy + 7, 21, 5,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
            // handle
            Raylib.DrawRectangle(cx - 2, cy + 12, 5, 14,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            // pommel
            Raylib.DrawCircle(cx, cy + 26, 4,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
            break;
    }
}

static void DrawArmorUI()
{
    if (!armorMenuOpen) return;

    int panelW = 700;
    int panelH = 620;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = ScreenHeight / 2 - panelH / 2;

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH,
        new Color((byte)18,(byte)18,(byte)24,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, Color.Gold);
    Raylib.DrawText("EQUIPMENT", panelX + 270, panelY + 12, 28, Color.Gold);
    Raylib.DrawText($"Defence: {GetTotalDefense()}", panelX + 20, panelY + 14, 20, Color.LightGray);

    // slot layout: label, ref getter/setter, x, y
    (string label, string value, int sx, int sy)[] slots =
    {
        ("HELMET",  armorHelmet, panelX + 280, panelY + 60),
        ("BODY",    armorBody,   panelX + 280, panelY + 130),
        ("LEGS",    armorLegs,   panelX + 280, panelY + 200),
        ("BOOTS",   armorBoots,  panelX + 280, panelY + 270),
        ("GLOVES",  armorGloves, panelX + 60,  panelY + 200),
        ("CAPE",    armorCape,   panelX + 60,  panelY + 130),
        ("WEAPON",  armorWeapon, panelX + 500, panelY + 130),
        ("SHIELD",  armorShield, panelX + 500, panelY + 200),
    };

    Vector2 mouse = Raylib.GetMousePosition();

    foreach (var (label, value, sx, sy) in slots)
    {
        bool isShieldSlot  = label == "SHIELD";
        bool shieldBlocked = isShieldSlot && IsTwoHandedWeapon(armorWeapon);

        Color borderColor = shieldBlocked ? Color.DarkGray
                          : value != null  ? Color.Gold
                          : Color.Gray;
        Color textColor   = shieldBlocked ? Color.DarkGray : Color.White;

        Raylib.DrawRectangle(sx, sy, 140, 60,
            new Color((byte)35,(byte)35,(byte)45,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, 140, 60, borderColor);

        Raylib.DrawText(label, sx + 6, sy + 6, 14,
            new Color((byte)120,(byte)120,(byte)140,(byte)255));

        if (shieldBlocked)
        {
            Raylib.DrawText("2H WEAPON", sx + 16, sy + 32, 14, Color.DarkGray);
        }
        else if (value != null)
        {
            // draw icon
            DrawArmorIcon(value, sx + 6, sy + 22, 28);
            Raylib.DrawText(value, sx + 38, sy + 32, 13, Color.Gold);

            // unequip on click
            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, 140, 60))
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                UnequipArmorSlot(label);
                ShowNotification($"Unequipped {value}");
            }
        }
        else
        {
            Raylib.DrawText("Empty", sx + 40, sy + 32, 14, Color.DarkGray);
        }
    }

    // mannequin silhouette in centre
    int mx = panelX + 280 + 70; // centre of helmet slot
    DrawMannequin(mx, panelY + 380);

    // item list to equip from (items the player owns)
    Raylib.DrawText("AVAILABLE ITEMS", panelX + 20, panelY + 440, 20, Color.Gold);
    Raylib.DrawRectangle(panelX + 10, panelY + 465, panelW - 20, 2,
        new Color((byte)80,(byte)80,(byte)80,(byte)255));

    string[] allItems = {
        "Leather Cap","Iron Helmet","Steel Helmet",
        "Leather Vest","Iron Chestplate","Steel Chestplate",
        "Leather Pants","Iron Leggings","Steel Leggings",
        "Leather Boots","Iron Boots","Steel Boots",
        "Leather Gloves","Iron Gauntlets","Steel Gauntlets",
        "Wool Cape","Magic Cape",
        "Wooden Shield","Iron Shield","Steel Shield",
        "Sword","Great Sword","War Axe","Battle Staff"
    };

    // for demo, show first 6 in a row — in a full game tie to player inventory
    int itemX = panelX + 14;
    int itemY = panelY + 475;
    int itemSlot = 60;
    int itemPad  = 8;
    int col = 0;

    foreach (string item in allItems)
    {
        if (col >= 10) break; // one row of 10 for now
        int ix = itemX + col * (itemSlot + itemPad);

        bool hover = Raylib.CheckCollisionPointRec(mouse,
            new Rectangle(ix, itemY, itemSlot, itemSlot));

        Raylib.DrawRectangle(ix, itemY, itemSlot, itemSlot,
            new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Raylib.DrawRectangleLines(ix, itemY, itemSlot, itemSlot,
            hover ? Color.Gold : new Color((byte)80,(byte)80,(byte)100,(byte)255));

        DrawArmorIcon(item, ix + 4, itemY + 6, 24);

        int nameW = Raylib.MeasureText(item.Length > 8 ? item.Substring(0,8) : item, 11);
        Raylib.DrawText(item.Length > 8 ? item.Substring(0,8) : item,
            ix + itemSlot/2 - nameW/2, itemY + itemSlot - 16, 11, Color.LightGray);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            TryEquipItem(item);

        col++;
    }

    Raylib.DrawText("Click item to equip  |  Click slot to unequip  |  G = Close",
        panelX + 60, panelY + panelH - 28, 16, Color.LightGray);
}

static void TryEquipItem(string item)
{
    // figure out which slot this item belongs to
    string slot = GetItemSlot(item);
    if (slot == null) return;

    // block shield if 2H weapon equipped
    if (slot == "SHIELD" && IsTwoHandedWeapon(armorWeapon))
    {
        ShowNotification("Can't use a shield with a two-handed weapon!");
        return;
    }

    // block 2H weapon if shield equipped
    if (slot == "WEAPON" && IsTwoHandedWeapon(item) && armorShield != null)
    {
        ShowNotification("Unequip your shield first for a two-handed weapon!");
        return;
    }

    switch (slot)
    {
        case "HELMET": armorHelmet = item; break;
        case "BODY":   armorBody   = item; break;
        case "LEGS":   armorLegs   = item; break;
        case "BOOTS":  armorBoots  = item; break;
        case "GLOVES": armorGloves = item; break;
        case "CAPE":   armorCape   = item; break;
        case "WEAPON": armorWeapon = item; break;
        case "SHIELD": armorShield = item; break;
    }
    ShowNotification($"Equipped {item}!");
}

static void UnequipArmorSlot(string slot)
{
    switch (slot)
    {
        case "HELMET": armorHelmet = null; break;
        case "BODY":   armorBody   = null; break;
        case "LEGS":   armorLegs   = null; break;
        case "BOOTS":  armorBoots  = null; break;
        case "GLOVES": armorGloves = null; break;
        case "CAPE":   armorCape   = null; break;
        case "WEAPON": armorWeapon = null; break;
        case "SHIELD": armorShield = null; break;
    }
}

static string GetItemSlot(string item)
{
    return item switch
    {
        "Leather Cap" or "Iron Helmet" or "Steel Helmet"
            => "HELMET",
        "Leather Vest" or "Iron Chestplate" or "Steel Chestplate"
            => "BODY",
        "Leather Pants" or "Iron Leggings" or "Steel Leggings"
            => "LEGS",
        "Leather Boots" or "Iron Boots" or "Steel Boots"
            => "BOOTS",
        "Leather Gloves" or "Iron Gauntlets" or "Steel Gauntlets"
            => "GLOVES",
        "Wool Cape" or "Magic Cape"
            => "CAPE",
        "Sword" or "Stick" or "Great Sword" or "War Axe" or "Battle Staff"
            => "WEAPON",
        "Wooden Shield" or "Iron Shield" or "Steel Shield"
            => "SHIELD",
        _ => null
    };
}

static void DrawArmorIcon(string item, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2;

    switch (GetItemSlot(item))
    {
        case "HELMET":
            Raylib.DrawRectangle(cx - 8, cy - 2, 16, 10,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawRectangle(cx - 10, cy - 8, 20, 8,
                new Color((byte)140,(byte)140,(byte)150,(byte)255));
            Raylib.DrawRectangle(cx - 6, cy + 8, 12, 4,
                new Color((byte)120,(byte)120,(byte)130,(byte)255));
            break;
        case "BODY":
            Raylib.DrawRectangle(cx - 10, cy - 8, 20, 18,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawRectangle(cx - 14, cy - 6, 6, 12,
                new Color((byte)140,(byte)140,(byte)150,(byte)255));
            Raylib.DrawRectangle(cx + 8, cy - 6, 6, 12,
                new Color((byte)140,(byte)140,(byte)150,(byte)255));
            break;
        case "LEGS":
            Raylib.DrawRectangle(cx - 9, cy - 8, 8, 18,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawRectangle(cx + 1, cy - 8, 8, 18,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            break;
        case "BOOTS":
            Raylib.DrawRectangle(cx - 8, cy - 6, 7, 14,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx + 1, cy - 6, 7, 14,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - 10, cy + 6, 9, 4,
                new Color((byte)100,(byte)60,(byte)20,(byte)255));
            Raylib.DrawRectangle(cx + 1, cy + 6, 9, 4,
                new Color((byte)100,(byte)60,(byte)20,(byte)255));
            break;
        case "GLOVES":
            Raylib.DrawRectangle(cx - 8, cy - 4, 6, 10,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx + 2, cy - 4, 6, 10,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            for (int f = 0; f < 4; f++)
                Raylib.DrawRectangle(cx - 8 + f * 4, cy - 8, 3, 6,
                    new Color((byte)120,(byte)80,(byte)30,(byte)255));
            break;
        case "CAPE":
            Raylib.DrawTriangle(
                new Vector2(cx - 10, cy - 8),
                new Vector2(cx + 10, cy - 8),
                new Vector2(cx,      cy + 12),
                new Color((byte)120,(byte)40,(byte)160,(byte)255));
            break;
        case "WEAPON":
            // sword/axe icon
            Raylib.DrawLineEx(new Vector2(cx - 6, cy + 10),
                              new Vector2(cx + 6, cy - 10), 3,
                new Color((byte)180,(byte)190,(byte)200,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx - 6, cy),
                              new Vector2(cx + 6, cy), 4,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
            break;
        case "SHIELD":
            Raylib.DrawRectangle(cx - 8, cy - 10, 16, 20,
                new Color((byte)100,(byte)80,(byte)40,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx - 8, cy + 10),
                new Vector2(cx + 8, cy + 10),
                new Vector2(cx,     cy + 18),
                new Color((byte)100,(byte)80,(byte)40,(byte)255));
            Raylib.DrawCircle(cx, cy, 4,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
            break;
    }
}

static void DrawMannequin(int cx, int cy)
{
    // simple mannequin silhouette showing equipped gear
    Color helmetCol  = armorHelmet != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color bodyCol    = armorBody   != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color legsCol    = armorLegs   != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color bootsCol   = armorBoots  != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color glovesCol  = armorGloves != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color capeCol    = armorCape   != null ? Color.Purple  : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color shieldCol  = armorShield != null ? Color.SkyBlue : new Color((byte)60,(byte)60,(byte)70,(byte)255);

    // cape (behind body)
    Raylib.DrawTriangle(
        new Vector2(cx - 14, cy - 30),
        new Vector2(cx + 14, cy - 30),
        new Vector2(cx,      cy + 40),
        capeCol);

    // helmet
    Raylib.DrawCircle(cx, cy - 42, 14, helmetCol);
    // body
    Raylib.DrawRectangle(cx - 14, cy - 28, 28, 38, bodyCol);
    // legs
    Raylib.DrawRectangle(cx - 12, cy + 10, 10, 26, legsCol);
    Raylib.DrawRectangle(cx + 2,  cy + 10, 10, 26, legsCol);
    // boots
    Raylib.DrawRectangle(cx - 14, cy + 34, 12, 8, bootsCol);
    Raylib.DrawRectangle(cx + 2,  cy + 34, 12, 8, bootsCol);
    // gloves / arms
    Raylib.DrawRectangle(cx - 24, cy - 26, 10, 24, glovesCol);
    Raylib.DrawRectangle(cx + 14, cy - 26, 10, 24, glovesCol);
    // shield (left arm)
    if (armorShield != null && !IsTwoHandedWeapon(armorWeapon))
    {
        Raylib.DrawRectangle(cx - 38, cy - 22, 12, 20, shieldCol);
        Raylib.DrawTriangle(
            new Vector2(cx - 38, cy - 2),
            new Vector2(cx - 26, cy - 2),
            new Vector2(cx - 32, cy + 8),
            shieldCol);
    }
}

public static string GetEquippedTool() => toolbarSlots[toolbarSelectedSlot];

        static void AddDealerBuilding(float x, float y, string name, DealerType type)
{
    var b = new Building(
        new Rectangle(x, y, 260, 160),
        new Color(180, 180, 200, 255),
        new Color(210, 210, 220, 255),
        new Vector2(x + 120, y + 220),
        name,
        new NPC(new Vector2(700, 120), "Dealer", $"Welcome to the {name}! E to browse."),
        entryPos: new Vector2(700, 900)
    );

    b.InteriorObjects.Clear();

    // simple counter and display area
    b.InteriorObjects.Add(new Rectangle(80, 140, 620, 60));  // counter
    b.InteriorObjects.Add(new Rectangle(80, 220, 600, 300)); // showroom floor (interactive)
    buildings.Add(b);

    // register a special NPC for this building so we can open dealer UI (store the building ref)
    if (type == DealerType.Bike) dealerVehicleOptions.Clear(); // repurpose lists later
    // We'll identify this building by its name when inside to show dealer UI
}

static void AddDominos(float x, float y)
{
    var dom = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(0, 75, 155, 255),
        new Color(20, 20, 20, 255),
        new Vector2(x + 100, y + 240),
        "DOMINO'S",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to Domino's! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    dom.InteriorObjects.Clear();
 
    // Counter
    dom.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    dom.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    dom.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    dom.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        dom.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        dom.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Pizza oven area wall
    dom.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(dom);
}
 
static void AddKFC(float x, float y)
{
    var kfc = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(180, 20, 20, 255),
        new Color(30, 15, 10, 255),
        new Vector2(x + 100, y + 240),
        "KFC",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to KFC! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    kfc.InteriorObjects.Clear();
 
    // Counter
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    kfc.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        kfc.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        kfc.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Kitchen wall
    kfc.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(kfc);
}
 
static void AddBurgerKing(float x, float y)
{
    var bk = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(210, 80, 0, 255),
        new Color(25, 15, 5, 255),
        new Vector2(x + 100, y + 240),
        "BURGER KING",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to Burger King! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    bk.InteriorObjects.Clear();
 
    // Counter
    bk.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    bk.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    bk.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    bk.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        bk.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        bk.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Flame grill area wall
    bk.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(bk);
}

      static void AddGasStation(float x, float y)
{
    var gasBuilding = new Building(
        new Rectangle(x + 300, y - 780, 260, 160),
        new Color(220, 220, 180, 255),
        new Color(200, 195, 160, 255),
        new Vector2(x + 280, y - 700),
        "GAS STATION",
        new NPC(new Vector2(1000, 150), "Attendant", "Pay for your fuel here bro."),
        entryPos: new Vector2(580, 870)
    );
 
    gasBuilding.InteriorObjects.Clear();
 
    // --- service counter (right side) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 380, 50));   // main counter
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 380, 8));    // counter top edge
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 8, 50));     // left side
    gasBuilding.InteriorObjects.Add(new Rectangle(1172, 80, 8, 50));    // right side
 
    // --- shelving aisles (snack/drink shop) ---
    // aisle 1
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  200, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  260, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  320, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  380, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  440, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  500, 180, 30));
 
    // aisle 2
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 200, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 260, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 320, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 380, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 440, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 500, 180, 30));
 
    // --- drink fridges (back wall) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(600,  80, 60, 120));
    gasBuilding.InteriorObjects.Add(new Rectangle(680,  80, 60, 120));
    gasBuilding.InteriorObjects.Add(new Rectangle(760,  80, 60, 120));
 
    // --- oil/auto products wall (left wall) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(20, 200, 40, 400));
 
    // --- toilets (back corner) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(1080, 200, 120, 120)); // toilet room
 
    // --- waiting bench ---
    gasBuilding.InteriorObjects.Add(new Rectangle(80, 620, 240, 30));
 
    // entrance mat
    //gasBuilding.InteriorObjects.Add(new Rectangle(480, 870, 200, 30));
 
    buildings.Add(gasBuilding);
 
    // add to gas stations list for pump logic
    gasStations.Add(new GasStation(x, y));
}

static void AddSwimmingComplex(float x, float y)
{
    var pool = new Building(
        new Rectangle(x, y, 300, 200),
        new Color(30, 120, 200, 255),
        new Color(20, 160, 220, 255),
        new Vector2(x + 150, y + 300),
        "SWIMMING COMPLEX",
        new NPC(new Vector2(700, 100), "Lifeguard", "Lane pool: swim laps. Diving pool: time your jump!"),
        entryPos: new Vector2(700, 900)
    );

    pool.InteriorObjects.Clear();

    // Lane pool walls
    pool.InteriorObjects.Add(new Rectangle(50, 100, 500, 20));   // top wall
    pool.InteriorObjects.Add(new Rectangle(50, 420, 500, 20));   // bottom wall
    pool.InteriorObjects.Add(new Rectangle(50, 100, 20, 340));   // left wall
    pool.InteriorObjects.Add(new Rectangle(530, 100, 20, 340));  // right wall

    // Lane dividers (can't walk through)
  //  for (int lane = 0; lane < 4; lane++)
   //     pool.InteriorObjects.Add(new Rectangle(52, 152 + lane * 60, 496, 8));

    // Diving pool walls
    pool.InteriorObjects.Add(new Rectangle(700, 100, 450, 20));  // top
    pool.InteriorObjects.Add(new Rectangle(700, 500, 450, 20));  // bottom
    pool.InteriorObjects.Add(new Rectangle(700, 100, 20, 420));  // left
    pool.InteriorObjects.Add(new Rectangle(1130, 100, 20, 420)); // right

    // Diving board platform (collision - player stands on edge)
    pool.InteriorObjects.Add(new Rectangle(700, 180, 80, 20));   // board platform base
    pool.InteriorObjects.Add(new Rectangle(700, 160, 20, 360));  // left wall of dive area

    buildings.Add(pool);
}

static void AddTennisCourt(float x, float y)
{
    var court = new Building(
        new Rectangle(x, y, 200, 120),
        new Color(180, 120, 40, 255),
        new Color(60, 160, 60, 255),
        new Vector2(x + 100, y + 200),
        "TENNIS COURT",
        new NPC(new Vector2(700, 100), "Tennis Coach", "Press E near the net to start a game! WASD to move your paddle."),
        entryPos: new Vector2(700, 920)
    );

    court.InteriorObjects.Clear();
    // Net post collision
    court.InteriorObjects.Add(new Rectangle(670, 80, 20, 560));

    buildings.Add(court);
}

static void AddBasketballCourt(float x, float y)
{
    var court = new Building(
        new Rectangle(x, y, 200, 120),
        new Color(200, 100, 20, 255),
        new Color(200, 140, 60, 255),
        new Vector2(x + 100, y + 200),
        "BASKETBALL COURT",
        new NPC(new Vector2(700, 100), "Coach", "Shoot hoops! Stand at the free throw line and press F. Lock power, then lock aim!"),
        entryPos: new Vector2(700, 920)
    );

    court.InteriorObjects.Clear();
    // Backboard collision
    court.InteriorObjects.Add(new Rectangle(640, 80, 80, 20));

    buildings.Add(court);
}

static void AddMcDonalds(float x, float y)
{
    var mcd = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(220, 30, 30, 255),
        new Color(255, 200, 0, 255),
        new Vector2(x + 100, y + 240),
        "McDONALD'S",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to McDonald's! Press E to order."),
        entryPos: new Vector2(700, 900)
    );

    mcd.InteriorObjects.Clear();

    // Counter
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    mcd.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));

    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        mcd.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        mcd.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }

    // Play area wall
    mcd.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));

    buildings.Add(mcd);
}
static void AddBank(float x, float y)
{
    var bank = new Building(
        new Rectangle(x, y, 240, 160),
        new Color(180, 150, 80, 255),
        new Color(220, 210, 180, 255),
        new Vector2(x + 120, y + 240),
        "BANK",
        new NPC(new Vector2(700, 450), "Bank Manager", "Kia ora! Welcome to Waikato Bank."),
        entryPos: new Vector2(700, 900)
    );

    bank.InteriorObjects.Clear();

    // outer walls
    bank.InteriorObjects.Add(new Rectangle(0,    0,   20, 1000)); // left wall
    bank.InteriorObjects.Add(new Rectangle(1380, 0,   20, 1000)); // right wall
    bank.InteriorObjects.Add(new Rectangle(0,    0, 1400,   20)); // top wall

    // teller counter wall (with gap at 1100-1200 for staff)
    bank.InteriorObjects.Add(new Rectangle(0,   350, 1100,  20));
    bank.InteriorObjects.Add(new Rectangle(1200,350,  200,  20));

    // 3 teller booths
    bank.InteriorObjects.Add(new Rectangle(150, 260, 200, 90));
    bank.InteriorObjects.Add(new Rectangle(500, 260, 200, 90));
    bank.InteriorObjects.Add(new Rectangle(850, 260, 200, 90));


    // --- office (top left) ---
    bank.InteriorObjects.Add(new Rectangle(0,    20,  20, 330)); // already covered by left wall
    bank.InteriorObjects.Add(new Rectangle(20,   20, 330,  20)); // top wall
    bank.InteriorObjects.Add(new Rectangle(330,  20,  20, 330)); // right wall
    bank.InteriorObjects.Add(new Rectangle(20,  330, 130,  20)); // bottom left
    bank.InteriorObjects.Add(new Rectangle(250, 330,  80,  20)); // bottom right (gap at 150-250 for door)

    // office desk
    bank.InteriorObjects.Add(new Rectangle(50,  80, 240,  60));  // manager desk

    // --- vault door (back wall centre) ---
    bank.InteriorObjects.Add(new Rectangle(580, 20, 240, 40));   // vault door

    buildings.Add(bank);
}

static void AddBikeDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 320, 180),
        new Color(40,  80,  140, 255),
        new Color(30,  40,  60,  255),
        new Vector2(x + 160, y + 260),
        "BIKE DEALER",
        new NPC(new Vector2(700, 120), "Bike Dealer", "Want a two-wheeler? E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // front counter
    b.InteriorObjects.Add(new Rectangle(400,  80,   600,  50));
 
    // display island collisions (3 islands)
    int[] islandX = { 100, 520, 940 };
    foreach (int ix in islandX)
    {
        b.InteriorObjects.Add(new Rectangle(ix, 240, 260, 120)); // island platform
        b.InteriorObjects.Add(new Rectangle(ix, 400, 260, 100)); // second row island
    }
 
    // tool/parts rack along left wall
    b.InteriorObjects.Add(new Rectangle(20,  120, 40,  680));
 
    // parts counter along right wall
    b.InteriorObjects.Add(new Rectangle(1340, 120, 40, 680));
 
    b.EntryPosition = new Vector2(x + 160, y + 220);
    buildings.Add(b);
}
 
static void AddCarDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 420, 240),
        new Color(20,  20,  20,  255),
        new Color(18,  18,  22,  255),
        new Vector2(x + 210, y + 320),
        "CAR DEALER",
        new NPC(new Vector2(700, 120), "Car Dealer", "Find your next ride here. E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // service counter (right side, top)
    b.InteriorObjects.Add(new Rectangle(800,  50,   560,  70));
 
    // car display bays — 2 rows of 3 bays
    int[] bayX = { 60, 480, 900 };
    int[] bayY = { 130, 380 };
    foreach (int byx in bayX)
        foreach (int byy in bayY)
            b.InteriorObjects.Add(new Rectangle(byx, byy, 340, 180)); // car bay
 
    // divider pillars between bays
    int[] pillarX = { 440, 860 };
    foreach (int px in pillarX)
    {
        b.InteriorObjects.Add(new Rectangle(px, 40,  20, 780)); // full-height pillar
    }
 
    b.EntryPosition = new Vector2(x + 210, y + 280);
    buildings.Add(b);
}
 
static void AddBarnDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 360, 220),
        new Color(120, 60,  30,  255),
        new Color(100, 70,  45,  255),
        new Vector2(x + 180, y + 320),
        "BARN DEALER",
        new NPC(new Vector2(700, 120), "Barn Dealer", "We have the finest mounts. E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // front reception / hay counter
    b.InteriorObjects.Add(new Rectangle(350,  60,   700,  55));
 
    // stalls — left side (3 stalls)
    int[] leftStallY = { 180, 370, 560 };
    foreach (int sy in leftStallY)
        b.InteriorObjects.Add(new Rectangle(20, sy, 280, 150));
 
    // stalls — right side (3 stalls)
    int[] rightStallY = { 180, 370, 560 };
    foreach (int sy in rightStallY)
        b.InteriorObjects.Add(new Rectangle(1100, sy, 280, 150));
 
    // central hay bale display row
    b.InteriorObjects.Add(new Rectangle(420,  300, 560,  80));
 
    b.EntryPosition = new Vector2(x + 180, y + 260);
    buildings.Add(b);
}


static void AddDBar(float x, float y)
{
    var dbar = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(15, 15, 15, 255),          // black exterior
        new Color(20, 20, 25, 255),           // very dark interior
        new Vector2(x + 100, y + 240),
        "DBar",
        new NPC(new Vector2(600, 420), "Dbar Owner", "Grab a woodys and relax at Dbar."),
        entryPos: new Vector2(318, 885)
    );
 
    dbar.InteriorObjects.Clear();
    dbar.InteriorObjects.Add(new Rectangle(100, 150, 300, 40));  // bar counter
    dbar.InteriorObjects.Add(new Rectangle(148, 278, 184, 104)); // pool table 1
    dbar.InteriorObjects.Add(new Rectangle(448, 278, 184, 104)); // pool table 2
    dbar.InteriorObjects.Add(new Rectangle(750, 150, 8, 400));   // divider
    dbar.InteriorObjects.Add(new Rectangle(780, 200, 60, 90));   // pokie 1
    dbar.InteriorObjects.Add(new Rectangle(780, 320, 60, 90));   // pokie 2
    dbar.InteriorObjects.Add(new Rectangle(880, 200, 60, 90));   // pokie 3
 
    buildings.Add(dbar);
}

static void AddSupermarket(float x, float y)
{
    var supermarket = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(200, 220, 200, 255),
        new Color(210, 225, 210, 255),
        new Vector2(x + 100, y + 240),
        "SUPERMARKET",
        new NPC(new Vector2(1280, 60), "Cashier", "Welcome! Grab a trolley or basket at the entrance. Use spacebar to pickup and return"),
        entryPos: new Vector2(500, 920)
    );

    supermarket.InteriorObjects.Clear();

    // --- entrance barrier ---
    supermarket.InteriorObjects.Add(new Rectangle(0,   870, 380, 20));
    supermarket.InteriorObjects.Add(new Rectangle(650, 870, 750, 20));   // right barrier

    // --- checkout counters ---
    supermarket.InteriorObjects.Add(new Rectangle(50,  30, 140, 45));    // checkout 1
    supermarket.InteriorObjects.Add(new Rectangle(250, 30, 140, 45));    // checkout 2
    supermarket.InteriorObjects.Add(new Rectangle(450, 30, 140, 45));    // checkout 3
    supermarket.InteriorObjects.Add(new Rectangle(650, 30, 140, 45));    // checkout 4
    supermarket.InteriorObjects.Add(new Rectangle(850, 30, 140, 45));    // checkout 5
    supermarket.InteriorObjects.Add(new Rectangle(1050,30, 140, 45));    // checkout 6

    // --- aisle shelves (pairs of shelves with walking gap) ---
    // aisle column 1
    supermarket.InteriorObjects.Add(new Rectangle(50,  150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  625, 220, 35));

    // aisle column 2
    supermarket.InteriorObjects.Add(new Rectangle(340, 150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 625, 220, 35));

    // aisle column 3
    supermarket.InteriorObjects.Add(new Rectangle(630, 150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 625, 220, 35));

    // --- meat fridges (right wall) ---
    supermarket.InteriorObjects.Add(new Rectangle(1310, 150, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 280, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 410, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 540, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 670, 55, 110));

    // --- fruit & veg bins (back wall) ---
    supermarket.InteriorObjects.Add(new Rectangle(50,  750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(160, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(270, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(380, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(490, 750, 90, 90));

    // --- deli station (back right) ---
    supermarket.InteriorObjects.Add(new Rectangle(650, 750, 300, 45));   // deli counter
    supermarket.InteriorObjects.Add(new Rectangle(650, 808, 120, 70));   // display case
    supermarket.InteriorObjects.Add(new Rectangle(790, 808, 70, 70));    // slicer/equipment
    supermarket.InteriorObjects.Add(new Rectangle(878, 808, 70, 70));    // warmer

    buildings.Add(supermarket);
}

static void AddHospital(float x, float y)
{
    var hospital = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(220, 50, 50, 255),
        new Color(200, 220, 220, 255),
        new Vector2(x + 80, y + 150),
        "HOSPITAL",
        new NPC(new Vector2(1235, 80), "Doctor", "Kia ora! I can patch you up for $20."),
        entryPos: new Vector2(1225, 897)
    );

    hospital.InteriorObjects.Clear();

    // front counter (single collision box)
    hospital.InteriorObjects.Add(new Rectangle(1095, 80, 280, 50));

    // computer on counter (single collision box)
    hospital.InteriorObjects.Add(new Rectangle(1215, 60, 40, 28));

    // medical cross sign (two bars)
    hospital.InteriorObjects.Add(new Rectangle(1228, 20, 14, 42));
    hospital.InteriorObjects.Add(new Rectangle(1214, 34, 42, 14));

    // hallway divider wall - moved down to give more room

    hospital.InteriorObjects.Add(new Rectangle(120, 400, 370, 20));
    hospital.InteriorObjects.Add(new Rectangle(120, 600, 370, 20));
    hospital.InteriorObjects.Add(new Rectangle(610, 400, 200, 20));
    hospital.InteriorObjects.Add(new Rectangle(610, 600, 200, 20));
    hospital.InteriorObjects.Add(new Rectangle(950, 400, 120, 20));
    hospital.InteriorObjects.Add(new Rectangle(950, 600, 100, 20));

    // room dividers top row
    hospital.InteriorObjects.Add(new Rectangle(350, 0, 20, 400));
    hospital.InteriorObjects.Add(new Rectangle(700, 0, 20, 400));
    hospital.InteriorObjects.Add(new Rectangle(1050, 0, 20, 400));

    // room dividers bottom row  
    hospital.InteriorObjects.Add(new Rectangle(350, 600, 20, 420));
    hospital.InteriorObjects.Add(new Rectangle(700, 600, 20, 420));
    hospital.InteriorObjects.Add(new Rectangle(1050, 600, 20, 420));

    // beds top row
    hospital.InteriorObjects.Add(new Rectangle(30,  30, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(380, 30, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(730, 30, 300, 80));

    // beds bottom row - moved up from wall
    hospital.InteriorObjects.Add(new Rectangle(30,  820, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(380, 820, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(730, 820, 300, 80));

    // side tables top row
    hospital.InteriorObjects.Add(new Rectangle(335, 30, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(685, 30, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(1035, 30, 15, 30));

    // side tables bottom row
    hospital.InteriorObjects.Add(new Rectangle(335, 820, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(685, 820, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(1035, 820, 15, 30));

    // chairs top row
    hospital.InteriorObjects.Add(new Rectangle(30,  165, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(380, 165, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(730, 165, 35, 35));

    // chairs bottom row
    hospital.InteriorObjects.Add(new Rectangle(30,  760, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(380, 760, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(730, 760, 35, 35));

    buildings.Add(hospital);
}

static void AddStore(float x, float y)
{
    var store = new Building(
        new Rectangle(x, y, 160, 120),
        Color.DarkGreen,
        new Color(40, 90, 50, 255),
        new Vector2(x + 60, y + 190),
        "STORE",
        new NPC(new Vector2(700, 120), "Store Clerk", "Need supplies? Show me the moolack."),
        entryPos: new Vector2(700, 850)
    );

    store.InteriorObjects.Clear();

    // --- front counter ---
    store.InteriorObjects.Add(new Rectangle(400, 80, 500, 50));
    store.InteriorObjects.Add(new Rectangle(400, 80, 500, 10));

    // --- aisle shelves (3 aisles, 8 shelf rows each, evenly spaced) ---
    int[] aisleStartX     = { 150, 550, 950 };
    int[] shelfYPositions = { 230, 290, 380, 440, 530, 590, 670, 730 };

    foreach (int ax in aisleStartX)
    {
        foreach (int sy in shelfYPositions)
            store.InteriorObjects.Add(new Rectangle(ax, sy, 280, 35));

        // end cap
        store.InteriorObjects.Add(new Rectangle(ax + 280, 220, 20, 510));
    }

    // --- back wall shelves (two banks of 3, split around entrance mat) ---
    int[] backShelfY = { 860, 907, 954 };

    foreach (int bsy in backShelfY)
    {
        store.InteriorObjects.Add(new Rectangle(50,  bsy, 500, 30));  // left bank
        store.InteriorObjects.Add(new Rectangle(850, bsy, 500, 30));  // right bank
    }

    buildings.Add(store);
}

static void AddWeapons(float x, float y)
{
    var weapons = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(80, 80, 80, 255),
        new Color(30, 30, 35, 255),
        new Vector2(x + 80, y + 100),
        "WEAPONS",
        new NPC(new Vector2(600, 420), "Weapons Dealer", "Need a sharper blade bro? I got you."),
        entryPos: new Vector2(600, 880)
    );
 
    weapons.InteriorObjects.Clear();
 
    // front service counter
    weapons.InteriorObjects.Add(new Rectangle(400, 80, 500, 50));
    weapons.InteriorObjects.Add(new Rectangle(400, 80, 500, 8));
 
    // --- wall-mounted weapon racks (back wall) ---
    // rack 1 - swords/blades
    weapons.InteriorObjects.Add(new Rectangle(50,  80, 260, 20));
    // rack 2 - bows/ranged
    weapons.InteriorObjects.Add(new Rectangle(50, 180, 260, 20));
    // rack 3 - shields
    weapons.InteriorObjects.Add(new Rectangle(50, 280, 260, 20));
 
    // --- display cases (centre floor) ---
    weapons.InteriorObjects.Add(new Rectangle(420, 280, 180, 60));  // display case 1
    weapons.InteriorObjects.Add(new Rectangle(650, 280, 180, 60));  // display case 2
    weapons.InteriorObjects.Add(new Rectangle(420, 400, 180, 60));  // display case 3
    weapons.InteriorObjects.Add(new Rectangle(650, 400, 180, 60));  // display case 4
 
    // --- armour stand area (right wall) ---
    weapons.InteriorObjects.Add(new Rectangle(1100, 80, 70, 140));  // armour stand 1
    weapons.InteriorObjects.Add(new Rectangle(1230, 80, 70, 140));  // armour stand 2
 
    // --- ammo/supplies shelves (right wall lower) ---
    weapons.InteriorObjects.Add(new Rectangle(1050, 280, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 350, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 420, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 490, 320, 30));
 
    // entrance mat
    //weapons.InteriorObjects.Add(new Rectangle(500, 870, 200, 30));
 
    buildings.Add(weapons);
}

static void AddMyHouse(float x, float y)
{
    var house = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(200, 160, 100, 255),
        new Color(180, 140, 100, 255),
        new Vector2(x + 80, y + 240),
        "MY HOUSE",
        new NPC(new Vector2(450, 180), "Mum", "Press E to interact with your chest and wardrobe | Press Z to sleep in your bed"),
        entryPos: new Vector2(700, 880)  // centre of entry mat
    );

    house.InteriorObjects.Clear();

    // ── Kitchen ──
    house.InteriorObjects.Add(new Rectangle(60,  55,  530, 55));   // bench top
    house.InteriorObjects.Add(new Rectangle(60,  110, 55,  200));  // bench left
    house.InteriorObjects.Add(new Rectangle(125, 120, 100, 100));  // stove
    house.InteriorObjects.Add(new Rectangle(240, 120, 90,  120));  // fridge

    // ── Dining ──
    house.InteriorObjects.Add(new Rectangle(130, 415, 260, 130));  // table

    // ── Lounge ──
    house.InteriorObjects.Add(new Rectangle(572, 670, 28,  160));  // TV
    house.InteriorObjects.Add(new Rectangle(60,  880, 340, 75));   // couch bottom
    house.InteriorObjects.Add(new Rectangle(60,  660, 75,  216));  // couch left

    // Hallway right wall split into 3 segments with 150px door gaps
    house.InteriorObjects.Add(new Rectangle(800, 0,   12, 150));  // above bathroom door
    house.InteriorObjects.Add(new Rectangle(800, 300, 12, 150));  // between bathroom and bedroom doors
    house.InteriorObjects.Add(new Rectangle(800, 600, 12, 400));  // below bedroom door

    house.InteriorObjects.Add(new Rectangle(800, 400, 540, 12));  // bathroom/bedroom divider

    // ── Bathroom ──
    house.InteriorObjects.Add(new Rectangle(1050, 55,  290, 170)); // shower
    house.InteriorObjects.Add(new Rectangle(820,  55,  80,  105)); // toilet
    house.InteriorObjects.Add(new Rectangle(1260, 320, 80,  75));  // sink

    // ── Bedroom ──
    house.InteriorObjects.Add(new Rectangle(1030, 450, 300, 270)); // bed
    house.InteriorObjects.Add(new Rectangle(950,  462, 72,  72));  // side table
    house.InteriorObjects.Add(new Rectangle(1030, 760, 300, 190)); // wardrobe
    house.InteriorObjects.Add(new Rectangle(820,  870, 105, 90));  // chest

    buildings.Add(house);
}

static void AddGym(float x, float y)
{
    var gym = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(50, 100, 180, 255),
        new Color(40, 40, 60, 255),
        new Vector2(x + 100, y + 240),
        "GYM",
        new NPC(new Vector2(950, 125), "Trainer", "You wanna get big? Train hard every day bro."),
        entryPos: new Vector2(1036, 902)
    );

    gym.InteriorObjects.Clear();
    gym.InteriorObjects.Add(new Rectangle(180, 180, 140, 60));   // dumbbell rack
    gym.InteriorObjects.Add(new Rectangle(480, 285, 220, 55));   // bench press
    gym.InteriorObjects.Add(new Rectangle(180, 920, 100, 60));   // treadmill 1
    gym.InteriorObjects.Add(new Rectangle(380, 920, 100, 60));   // treadmill 2
    gym.InteriorObjects.Add(new Rectangle(580, 920, 100, 60));   // treadmill 3
    gym.InteriorObjects.Add(new Rectangle(10, 200, 80, 60));     // bike 1
    gym.InteriorObjects.Add(new Rectangle(10, 400, 80, 60));     // bike 2
    gym.InteriorObjects.Add(new Rectangle(10, 600, 80, 60));     // bike 3
    gym.InteriorObjects.Add(new Rectangle(700, 150, 200, 40));   // counter
    gym.InteriorObjects.Add(new Rectangle(1310, 200, 60, 80));   // toilet 1
    gym.InteriorObjects.Add(new Rectangle(1310, 340, 60, 80));   // toilet 2
    gym.InteriorObjects.Add(new Rectangle(1310, 480, 80, 80));   // shower

    buildings.Add(gym);
}

static void AddMarae(float x, float y)
{
    var marae = new Building(
        new Rectangle(x, y, 220, 160),
        new Color(180, 60, 40, 255),
        new Color(140, 80, 40, 255),
        new Vector2(x + 110, y + 240),
        "MARAE",
        new NPC(new Vector2(700, 200), "Kaumatua", "Haere mai, haere mai, haere mai."),
        entryPos: new Vector2(700, 880)
    );

    marae.InteriorObjects.Clear();

    // --- outer walls (leave gap at bottom for entry) ---
    marae.InteriorObjects.Add(new Rectangle(0,   0,   20, 1000)); // left wall
    marae.InteriorObjects.Add(new Rectangle(1380, 0,  20, 1000)); // right wall
    marae.InteriorObjects.Add(new Rectangle(0,   0,  1400, 20)); // top wall

    // --- wharenui (meeting house) back wall ---
    marae.InteriorObjects.Add(new Rectangle(200, 80, 1000, 20));  // front wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(200, 80, 20, 400));   // left wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(1180, 80, 20, 400));  // right wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(200, 460, 20, 20));   // bottom left corner
    marae.InteriorObjects.Add(new Rectangle(1180, 460, 20, 20));  // bottom right corner

    // door gaps in wharenui front wall (two doors, centred)
    // left door gap: x=530-640, right door gap: x=760-870
    marae.InteriorObjects.Add(new Rectangle(220, 80, 310, 20));   // left of left door
    marae.InteriorObjects.Add(new Rectangle(640, 80, 120, 20));   // between doors
    marae.InteriorObjects.Add(new Rectangle(870, 80, 310, 20));   // right of right door

    // --- wharenui interior furniture ---
    // carvings on back wall
    marae.InteriorObjects.Add(new Rectangle(220, 90, 30, 120));   // left carving
    marae.InteriorObjects.Add(new Rectangle(1150, 90, 30, 120));  // right carving
    marae.InteriorObjects.Add(new Rectangle(650, 90, 100, 80));   // centre carving

    // benches along walls inside wharenui
    marae.InteriorObjects.Add(new Rectangle(220, 100, 20, 340));  // left bench
    marae.InteriorObjects.Add(new Rectangle(1160, 100, 20, 340)); // right bench

    // --- atea (courtyard) features ---
    // pou (posts) lining the path
    marae.InteriorObjects.Add(new Rectangle(280, 530, 20, 20));   // left pou 1
    marae.InteriorObjects.Add(new Rectangle(280, 640, 20, 20));   // left pou 2
    marae.InteriorObjects.Add(new Rectangle(280, 750, 20, 20));   // left pou 3
    marae.InteriorObjects.Add(new Rectangle(1100, 530, 20, 20));  // right pou 1
    marae.InteriorObjects.Add(new Rectangle(1100, 640, 20, 20));  // right pou 2
    marae.InteriorObjects.Add(new Rectangle(1100, 750, 20, 20));  // right pou 3

    // waharoa (gateway) posts at entrance
    marae.InteriorObjects.Add(new Rectangle(500, 880, 20, 120));  // left gate post
    marae.InteriorObjects.Add(new Rectangle(880, 880, 20, 120));  // right gate post

    buildings.Add(marae);
}

static void AddPoliceStation(float x, float y)
{
    var station = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(30, 30, 120, 255),
        new Color(40, 40, 80, 255),
        new Vector2(x + 80, y + 240),
        "POLICE STATION",
        new NPC(new Vector2(1235, 80), "Officer", "Keep it legal out there, no funny business."),
        entryPos: new Vector2(1225, 897)
    );

    station.InteriorObjects.Clear();

    // --- front desk (right side reception area) ---
    station.InteriorObjects.Add(new Rectangle(1095, 80, 280, 50));   // desk
    station.InteriorObjects.Add(new Rectangle(1215, 60, 40, 28));    // computer

    // --- police badge / shield sign ---
    station.InteriorObjects.Add(new Rectangle(1224, 15, 22, 30));    // badge body

    // --- vertical wall dividers (left zone | office zone | reception) ---
    station.InteriorObjects.Add(new Rectangle(350, 0, 20, 420));
    station.InteriorObjects.Add(new Rectangle(700, 0, 20, 420));
    station.InteriorObjects.Add(new Rectangle(1050, 0, 20, 400));
    station.InteriorObjects.Add(new Rectangle(1050, 600, 20, 420));

    // --- horizontal hallway walls ---
    station.InteriorObjects.Add(new Rectangle(0,   420, 810, 20));   // top hallway wall
    station.InteriorObjects.Add(new Rectangle(0,   600, 810, 20));   // bottom hallway wall

    // --- vertical dividers bottom zone ---
    station.InteriorObjects.Add(new Rectangle(350, 600, 20, 420));
    station.InteriorObjects.Add(new Rectangle(700, 600, 20, 420));

    // --- desks top row (offices) ---
    station.InteriorObjects.Add(new Rectangle(40,  40, 260, 60));
    station.InteriorObjects.Add(new Rectangle(400, 40, 260, 60));
    station.InteriorObjects.Add(new Rectangle(750, 40, 260, 60));

    // --- desks bottom row (offices) ---
    station.InteriorObjects.Add(new Rectangle(40,  830, 260, 60));
    station.InteriorObjects.Add(new Rectangle(400, 830, 260, 60));
    station.InteriorObjects.Add(new Rectangle(750, 830, 260, 60));

    // --- chairs top ---
    station.InteriorObjects.Add(new Rectangle(40,  150, 35, 35));
    station.InteriorObjects.Add(new Rectangle(400, 150, 35, 35));
    station.InteriorObjects.Add(new Rectangle(750, 150, 35, 35));

    // --- chairs bottom ---
    station.InteriorObjects.Add(new Rectangle(40,  760, 35, 35));
    station.InteriorObjects.Add(new Rectangle(400, 760, 35, 35));
    station.InteriorObjects.Add(new Rectangle(750, 760, 35, 35));

    // --- holding cells (far left, full height) ---
    station.InteriorObjects.Add(new Rectangle(0, 0, 20, 1000));      // left outer wall
    station.InteriorObjects.Add(new Rectangle(0, 490, 120, 20));     // cell divider

    buildings.Add(station);
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

    // OCEAN/BEACH sand ripples
    for (int x = 26000; x < 30000; x += 400)
    {
        for (int y = -39000; y < 39000; y += 400)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 20 + (Math.Abs(x * y) % 30);
            byte r = (byte)(200 + Math.Abs(x + y) % 40);
            byte g = (byte)(180 + Math.Abs(x - y) % 30);
            oceanPatches.Add((pos, radius, new Color(r, g, (byte)110, (byte)100)));
        }
    }

    // OCEAN shells
    Color[] shellColors = {
        new Color((byte)240,(byte)220,(byte)200,(byte)255),
        new Color((byte)220,(byte)180,(byte)160,(byte)255),
        new Color((byte)200,(byte)150,(byte)130,(byte)255)
    };
    for (int x = 26200; x < 30000; x += 350)
    {
        for (int y = -39000; y < 39000; y += 300)
        {
            Vector2 pos = new Vector2(x + 60, y + 60);
            oceanShells.Add((pos, shellColors[Math.Abs(x + y) % shellColors.Length]));
        }
    }

    // OCEAN coral
    Color[] coralColors = {
        new Color((byte)220,(byte)90,(byte)90,(byte)200),
        new Color((byte)255,(byte)160,(byte)60,(byte)200),
        new Color((byte)180,(byte)80,(byte)180,(byte)200)
    };
    for (int x = 30000; x < 60000; x += 600)
    {
        for (int y = -39000; y < 39000; y += 550)
        {
            Vector2 pos = new Vector2(x + 100, y + 100);
            oceanCoral.Add((pos, coralColors[Math.Abs(x + y) % coralColors.Length]));
        }
    }

    // SWAMP murky patches
    for (int x = -55000; x < -30000; x += 450)
    {
        for (int y = -39000; y < 39000; y += 450)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 25 + (Math.Abs(x * y) % 35);
            byte g = (byte)(55 + Math.Abs(x + y) % 35);
            swampPatches.Add((pos, radius, new Color((byte)30, g, (byte)25, (byte)160)));
        }
    }

    // SWAMP reeds
    for (int x = -55000; x < -30000; x += 280)
    {
        for (int y = -39000; y < 39000; y += 260)
        {
            Vector2 pos = new Vector2(x + 50, y + 50);
            Color reedColor = new Color((byte)50, (byte)90, (byte)30, (byte)220);
            swampReeds.Add((pos, reedColor));
        }
    }

    // SWAMP lily pads
    for (int x = -54000; x < -30000; x += 600)
    {
        for (int y = -39000; y < 39000; y += 550)
        {
            Vector2 pos = new Vector2(x + 120, y + 120);
            bool hasFlower = (Math.Abs(x + y) % 3 == 0);
            Color lilyColor = hasFlower
                ? new Color((byte)240,(byte)220,(byte)80,(byte)220)
                : new Color((byte)40,(byte)110,(byte)40,(byte)200);
            swampLilies.Add((pos, lilyColor));
        }
    }

    // VOLCANO lava patches
    for (int x = 26000; x < 60000; x += 500)
    {
        for (int y = -20000; y < -2000; y += 500)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 15 + (Math.Abs(x * y) % 25);
            bool bright = (Math.Abs(x + y) % 3 == 0);
            Color lavaColor = bright
                ? new Color((byte)255,(byte)180,(byte)0,(byte)200)
                : new Color((byte)200,(byte)60,(byte)0,(byte)200);
            volcanoPatches.Add((pos, radius, lavaColor));
        }
    }

    // VOLCANO steam vents
    for (int x = 27000; x < 60000; x += 1000)
    {
        for (int y = -20000; y < -2000; y += 900)
        {
            Vector2 pos = new Vector2(x + 200, y + 200);
            lavaVents.Add((pos, new Color((byte)80,(byte)70,(byte)65,(byte)180)));
        }
    }

    // MOUNTAIN rocky patches
    for (int x = -60000; x < -26000; x += 420)
    {
        for (int y = -20000; y < -2000; y += 400)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 10 + (Math.Abs(x * y) % 20);
            byte shade = (byte)(75 + Math.Abs(x + y) % 30);
            mountainPatches.Add((pos, radius, new Color(shade, (byte)(shade - 5), (byte)(shade - 10), (byte)255)));
        }
    }

    // MOUNTAIN pine trees
    for (int x = -59500; x < -26000; x += 550)
    {
        for (int y = -20000; y < -2000; y += 500)
        {
            if (Math.Abs(x + y) % 3 != 0) continue; // keep sparse
            Vector2 pos = new Vector2(x + 100, y + 100);
            mountainTrees.Add((pos, new Color((byte)25,(byte)55,(byte)30,(byte)255)));
        }
    }
}
        static void ShowNotification(string message)
        {
            levelUpMessage = message;
            levelUpTimer = 2.5f;
        }

        public static bool IsOnRoad(Vector2 pos)
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

// OCEAN sand patches
    foreach (var patch in oceanPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    // OCEAN shells
    foreach (var shell in oceanShells)
        if (shell.pos.X > viewLeft && shell.pos.X < viewRight &&
            shell.pos.Y > viewTop && shell.pos.Y < viewBottom)
        {
            int sx = (int)shell.pos.X;
            int sy = (int)shell.pos.Y;
            Raylib.DrawCircle(sx,     sy,     6, shell.color);
            Raylib.DrawCircle(sx + 3, sy - 2, 3, shell.color);
            Raylib.DrawCircle(sx + 5, sy - 4, 2, shell.color);
        }

    // OCEAN coral
    foreach (var coral in oceanCoral)
        if (coral.pos.X > viewLeft && coral.pos.X < viewRight &&
            coral.pos.Y > viewTop && coral.pos.Y < viewBottom)
        {
            int cx2 = (int)coral.pos.X;
            int cy2 = (int)coral.pos.Y;
            Raylib.DrawLineEx(new Vector2(cx2, cy2),      new Vector2(cx2,      cy2 - 18), 4, coral.color);
            Raylib.DrawLineEx(new Vector2(cx2, cy2 - 10), new Vector2(cx2 - 10, cy2 - 20), 3, coral.color);
            Raylib.DrawLineEx(new Vector2(cx2, cy2 - 10), new Vector2(cx2 + 10, cy2 - 20), 3, coral.color);
            Raylib.DrawCircle(cx2,      cy2 - 18, 4, coral.color);
            Raylib.DrawCircle(cx2 - 10, cy2 - 20, 3, coral.color);
            Raylib.DrawCircle(cx2 + 10, cy2 - 20, 3, coral.color);
        }

    // SWAMP murky patches
    foreach (var patch in swampPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    // SWAMP reeds
    foreach (var reed in swampReeds)
        if (reed.pos.X > viewLeft && reed.pos.X < viewRight &&
            reed.pos.Y > viewTop && reed.pos.Y < viewBottom)
        {
            int gx = (int)reed.pos.X;
            int gy = (int)reed.pos.Y;
            Raylib.DrawLineEx(new Vector2(gx,     gy), new Vector2(gx - 3, gy - 22), 2, reed.color);
            Raylib.DrawLineEx(new Vector2(gx + 6, gy), new Vector2(gx + 4, gy - 18), 2, reed.color);
            Raylib.DrawLineEx(new Vector2(gx - 5, gy), new Vector2(gx - 8, gy - 20), 2, reed.color);
            Raylib.DrawEllipse(gx - 3, gy - 22, 3, 6, new Color((byte)80, (byte)50, (byte)20, (byte)255));
            Raylib.DrawEllipse(gx + 4, gy - 18, 3, 6, new Color((byte)80, (byte)50, (byte)20, (byte)255));
            Raylib.DrawEllipse(gx - 8, gy - 20, 3, 6, new Color((byte)80, (byte)50, (byte)20, (byte)255));
        }

    // SWAMP lily pads
    foreach (var lily in swampLilies)
        if (lily.pos.X > viewLeft && lily.pos.X < viewRight &&
            lily.pos.Y > viewTop && lily.pos.Y < viewBottom)
        {
            int lx = (int)lily.pos.X;
            int ly = (int)lily.pos.Y;
            Raylib.DrawCircle(lx, ly, 12, new Color((byte)40, (byte)110, (byte)40, (byte)200));
            Raylib.DrawTriangle(
                new Vector2(lx,     ly),
                new Vector2(lx + 6, ly - 12),
                new Vector2(lx - 6, ly - 12),
                new Color((byte)30, (byte)55, (byte)25, (byte)180));
            if (lily.color.R > 200)
                Raylib.DrawCircle(lx, ly - 4, 4, lily.color);
        }

    // VOLCANO lava patches
    foreach (var patch in volcanoPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            Raylib.DrawCircle((int)patch.pos.X, (int)patch.pos.Y, patch.radius, patch.color);

    // VOLCANO rocks (sparse, offset from patch positions)
    foreach (var patch in volcanoPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
            if ((int)(patch.pos.X + patch.pos.Y) % 3 == 0)
            {
                int rx = (int)patch.pos.X + 20;
                int ry = (int)patch.pos.Y + 15;
                Color rockCol = new Color((byte)35, (byte)18, (byte)8, (byte)255);
                Raylib.DrawCircle(rx,      ry,     10, rockCol);
                Raylib.DrawCircle(rx + 10, ry + 5,  7, rockCol);
                Raylib.DrawCircle(rx - 6,  ry + 7,  6, rockCol);
            }

    // VOLCANO steam vents
    foreach (var vent in lavaVents)
        if (vent.pos.X > viewLeft && vent.pos.X < viewRight &&
            vent.pos.Y > viewTop && vent.pos.Y < viewBottom)
        {
            int vx = (int)vent.pos.X;
            int vy = (int)vent.pos.Y;
            for (int s = 0; s < 4; s++)
                Raylib.DrawEllipse(vx, vy - s * 10, 8 - s, 5 - s,
                    new Color((byte)200, (byte)200, (byte)200, (byte)(80 - s * 18)));
        }

    // MOUNTAIN rocky patches with snow dusting
    foreach (var patch in mountainPatches)
        if (patch.pos.X > viewLeft && patch.pos.X < viewRight &&
            patch.pos.Y > viewTop && patch.pos.Y < viewBottom)
        {
            int rx = (int)patch.pos.X;
            int ry = (int)patch.pos.Y;
            Raylib.DrawCircle(rx,      ry,     patch.radius, patch.color);
            Raylib.DrawCircle(rx + 12, ry + 6, patch.radius * 0.7f, patch.color);
            Raylib.DrawEllipse(rx, ry - 4, (int)(patch.radius * 0.6f), 3,
                new Color((byte)230, (byte)235, (byte)245, (byte)180));
        }

    // MOUNTAIN pine trees
    foreach (var tree in mountainTrees)
        if (tree.pos.X > viewLeft && tree.pos.X < viewRight &&
            tree.pos.Y > viewTop && tree.pos.Y < viewBottom)
        {
            int tx = (int)tree.pos.X;
            int ty = (int)tree.pos.Y;
            Raylib.DrawRectangle(tx - 3, ty, 6, 16, new Color((byte)70, (byte)50, (byte)30, (byte)255));
            Raylib.DrawTriangle(
                new Vector2(tx,      ty - 28), new Vector2(tx - 18, ty),      new Vector2(tx + 18, ty),
                tree.color);
            Raylib.DrawTriangle(
                new Vector2(tx,      ty - 38), new Vector2(tx - 13, ty - 14), new Vector2(tx + 13, ty - 14),
                new Color((byte)30, (byte)65, (byte)35, (byte)255));
            Raylib.DrawTriangle(
                new Vector2(tx,     ty - 46), new Vector2(tx - 8, ty - 28), new Vector2(tx + 8, ty - 28),
                new Color((byte)35, (byte)75, (byte)40, (byte)255));
            Raylib.DrawCircle(tx, ty - 46, 3, new Color((byte)230, (byte)235, (byte)245, (byte)200));
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
        dayOfMonth.ToString(),
        currentMonth.ToString(),
        timeOfDay.ToString(),
        dayOfWeek.ToString(),
    };
        lines.Add(axePickedUp ? "1" : "0");
        lines.Add(pickaxePickedUp ? "1" : "0");
        lines.Add(fishingRodPickedUp ? "1" : "0");
        lines.Add(fishingNetPickedUp ? "1" : "0");
        lines.Add(torchPickedUp ? "1" : "0");
        lines.Add(player.HasAxe ? "1" : "0");

        for (int i = 0; i < 8; i++)
    lines.Add(toolbarSlots[i] ?? "empty");

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
    if (lines.Length > 52) dayOfMonth = int.Parse(lines[52]);
    if (lines.Length > 53) currentMonth = int.Parse(lines[53]);
    if (lines.Length > 54) timeOfDay = float.Parse(lines[54]);
    if (lines.Length > 55) dayOfWeek = int.Parse(lines[55]);
    if (lines.Length > 56) axePickedUp        = lines[56] == "1";
if (lines.Length > 57) pickaxePickedUp    = lines[57] == "1";
if (lines.Length > 58) fishingRodPickedUp = lines[58] == "1";
if (lines.Length > 59) fishingNetPickedUp = lines[59] == "1";
if (lines.Length > 60) torchPickedUp      = lines[60] == "1";
if (lines.Length > 61) player.HasAxe      = lines[61] == "1";

// load toolbar slots
for (int i = 0; i < 8; i++)
{
    int lineIndex = 62 + i;
    if (lines.Length > lineIndex)
        toolbarSlots[i] = lines[lineIndex] == "empty" ? null : lines[lineIndex];
}
    }

static void DrawTrolley(int x, int y, bool takenAway)
{
    if (takenAway) return;
    Raylib.DrawRectangle(x, y, 60, 35, new Color((byte)150,(byte)150,(byte)160,(byte)255));
    Raylib.DrawRectangleLines(x, y, 60, 35, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    for (int i = x + 10; i < x + 60; i += 10)
        Raylib.DrawRectangle(i, y, 2, 35, new Color((byte)120,(byte)120,(byte)130,(byte)255));
    for (int j = y + 8; j < y + 35; j += 8)
        Raylib.DrawRectangle(x, j, 60, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
    Raylib.DrawRectangle(x, y - 12, 60, 8, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    Raylib.DrawCircle(x + 12, y + 42, 6, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawCircle(x + 48, y + 42, 6, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawCircle(x + 12, y + 42, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    Raylib.DrawCircle(x + 48, y + 42, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
}

static void DrawBasket(int x, int y, bool takenAway)
{
    if (takenAway) return;
    Raylib.DrawRectangle(x, y + 6, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
    for (int i = x + 4; i < x + 22; i += 5)
        Raylib.DrawRectangle(i, y + 6, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
    Raylib.DrawRectangle(x + 4, y, 14, 8, new Color((byte)140,(byte)70,(byte)15,(byte)255));
    Raylib.DrawRectangle(x + 4, y, 4, 8, new Color((byte)160,(byte)90,(byte)25,(byte)255));
    Raylib.DrawRectangle(x + 14, y, 4, 8, new Color((byte)160,(byte)90,(byte)25,(byte)255));
}

static void DrawSupermarketInventoryUI()
{
    if (!supermarketInventoryOpen) return;
    if (!player.HasTrolley && !player.HasBasket) return;

    bool isTrolley = player.HasTrolley;
    int capacity = isTrolley ? 20 : 10;
    var inventory = isTrolley ? trolleyInventory : basketInventory;
    string title = isTrolley ? "TROLLEY" : "BASKET";

    int cols = 5;
    int rows = isTrolley ? 4 : 2;
    int slotSize = 60;
    int pad = 8;
    int panelW = cols * (slotSize + pad) + 20;
    int panelH = rows * (slotSize + pad) + 60;
    int px = ScreenWidth / 2 - panelW / 2;
    int py = 200;

    Raylib.DrawRectangle(px, py, panelW, panelH, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(px, py, panelW, panelH, Color.Gold);
    Raylib.DrawText(title, px + 12, py + 10, 24, Color.Gold);
    Raylib.DrawText($"{inventory.Count(s => s != null)}/{capacity}", px + panelW - 60, py + 12, 18, Color.LightGray);

    for (int i = 0; i < capacity; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = px + 10 + col * (slotSize + pad);
        int sy = py + 44 + row * (slotSize + pad);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (!string.IsNullOrEmpty(inventory[i]))
            Raylib.DrawText(inventory[i].Substring(0, Math.Min(6, inventory[i].Length)),
                sx + 4, sy + slotSize / 2 - 8, 13, Color.White);
    }

    Raylib.DrawText("I = Close", px + panelW / 2 - 40, py + panelH - 22, 16, Color.LightGray);
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
        player.Fur, player.Stingers, player.BearPelts, player.DogFangs
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
        case "Dog Fangs": player.DogFangs -= amount; break;
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
        "Dog Fangs" => player.DogFangs,
        _ => 0
    };
}

static void DrawMcDonaldsMenu()
{
    if (!mcdonaldsMenuOpen) return;
    if (currentBuilding == null || currentBuilding.BuildingName != "McDONALD'S") return;

    string[] items = { "Big Mac", "McChicken", "Large Fries", "McFlurry", "Happy Meal", "10pc Nuggets" };
    int[]    prices = { 8, 7, 4, 5, 9, 6 };
    string[] descriptions = {
        "Two beef patties, special sauce. +25 HP",
        "Crispy chicken fillet. +20 HP",
        "Golden salty fries. +10 HP",
        "Creamy ice cream swirl. +15 HP",
        "Kids meal + toy! +20 HP",
        "Ten crispy nuggets. +22 HP"
    };
    int[] hpGain = { 25, 20, 10, 15, 20, 22 };

    int panelW = 700;
    int panelH = 540;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = 80;

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)20,(byte)20,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color((byte)255,(byte)200,(byte)0,(byte)255));
    // Header
    Raylib.DrawRectangle(panelX, panelY, panelW, 50, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Raylib.DrawText("McDONALD'S", panelX + 240, panelY + 10, 32, new Color((byte)255,(byte)220,(byte)0,(byte)255));
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);

    if (mcdonaldsOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80,
            new Color((byte)0,(byte)160,(byte)0,(byte)255));
        Raylib.DrawText($"🍟 {mcdonaldsOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
        return;
    }

    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < items.Length; i++)
    {
        int row = i / 2;
        int col = i % 2;
        int bx = panelX + 20 + col * 340;
        int by = panelY + 90 + row * 140;

        bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(bx, by, 320, 120));
        bool canAfford = player.Money >= prices[i];

        Raylib.DrawRectangle(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)50,(byte)30,(byte)0,(byte)255)
                               : new Color((byte)30,(byte)20,(byte)0,(byte)255));
        Raylib.DrawRectangleLines(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)255,(byte)200,(byte)0,(byte)255)
                               : new Color((byte)150,(byte)100,(byte)0,(byte)255));

        Raylib.DrawText(items[i], bx + 12, by + 10, 22,
            canAfford ? Color.White : Color.DarkGray);
        Raylib.DrawText($"${prices[i]}", bx + 260, by + 10, 22,
            canAfford ? Color.Gold : Color.DarkGray);
        Raylib.DrawText(descriptions[i], bx + 12, by + 40, 15,
            canAfford ? Color.LightGray : Color.DarkGray);

        if (!canAfford)
            Raylib.DrawText("Not enough $", bx + 12, by + 90, 14, Color.Red);

        if (hover && canAfford && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            player.Money -= prices[i];
            mcdonaldsOrderName = items[i];
            mcdonaldsOrderReady = true;
            mcdonaldsOrderTimer = 3f;  // 3 second wait
            player.Health = Math.Min(player.MaxHealth, player.Health + hpGain[i]);
            mcdonaldsMessage = $"Ordered {items[i]}! Ready in a moment...";
            mcdonaldsMessageTimer = 4f;
        }
    }

    Raylib.DrawText("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
}

static void DrawDominosMenu()
{
    if (!dominosMenuOpen) return;
    if (currentBuilding == null || currentBuilding.BuildingName != "DOMINO'S") return;
 
    string[] items        = { "Pepperoni",    "BBQ Chicken",   "Veggie Supreme", "Garlic Bread", "Cheesy Bread", "Cola" };
    int[]    prices       = { 12, 13, 11, 5, 6, 3 };
    string[] descriptions = {
        "Classic pepperoni pizza. +25 HP",
        "Smoky BBQ chicken pizza. +22 HP",
        "Fresh garden veggie pizza. +20 HP",
        "Toasted garlic bread. +10 HP",
        "Melted cheese bread. +12 HP",
        "Ice cold cola. +5 HP"
    };
    int[] hpGain = { 25, 22, 20, 10, 12, 5 };
 
    int panelW = 700, panelH = 540;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = 80;
 
    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)10,(byte)10,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    // Header
    Raylib.DrawRectangle(panelX, panelY, panelW, 50, new Color((byte)0,(byte)60,(byte)140,(byte)255));
    Raylib.DrawText("DOMINO'S", panelX + 260, panelY + 10, 32, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (dominosOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Raylib.DrawText($"🍕 {dominosOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
        return;
    }
 
    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < items.Length; i++)
    {
        int row = i / 2, col = i % 2;
        int bx = panelX + 20 + col * 340;
        int by = panelY + 90 + row * 140;
 
        bool hover     = Raylib.CheckCollisionPointRec(mouse, new Rectangle(bx, by, 320, 120));
        bool canAfford = player.Money >= prices[i];
 
        Raylib.DrawRectangle(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)0,(byte)40,(byte)90,(byte)255)
                               : new Color((byte)0,(byte)25,(byte)60,(byte)255));
        Raylib.DrawRectangleLines(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)190,(byte)20,(byte)20,(byte)255)
                               : new Color((byte)0,(byte)50,(byte)110,(byte)255));
 
        Raylib.DrawText(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White    : Color.DarkGray);
        Raylib.DrawText($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Red     : Color.DarkGray);
        Raylib.DrawText(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray : Color.DarkGray);
        if (!canAfford) Raylib.DrawText("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
        if (hover && canAfford && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            player.Money -= prices[i];
            dominosOrderName  = items[i];
            dominosOrderReady = true;
            dominosOrderTimer = 3f;
            player.Health = Math.Min(player.MaxHealth, player.Health + hpGain[i]);
            dominosMessage      = $"Ordered {items[i]}! Ready in a moment...";
            dominosMessageTimer = 4f;
        }
    }
    Raylib.DrawText("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
}
 
static void DrawKFCMenu()
{
    if (!kfcMenuOpen) return;
    if (currentBuilding == null || currentBuilding.BuildingName != "KFC") return;
 
    string[] items        = { "Original Chicken", "Zinger Burger",   "3pc Meal",       "Coleslaw",     "Popcorn Chicken", "Pepsi" };
    int[]    prices       = { 10, 9, 14, 4, 7, 3 };
    string[] descriptions = {
        "11 herbs & spices chicken. +28 HP",
        "Spicy crispy chicken burger. +24 HP",
        "Three pieces + sides. +35 HP",
        "Creamy classic coleslaw. +8 HP",
        "Bite-size crispy chicken. +18 HP",
        "Ice cold Pepsi. +5 HP"
    };
    int[] hpGain = { 28, 24, 35, 8, 18, 5 };
 
    int panelW = 700, panelH = 540;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = 80;
 
    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)20,(byte)10,(byte)5,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    // Header
    Raylib.DrawRectangle(panelX, panelY, panelW, 50, new Color((byte)160,(byte)15,(byte)15,(byte)255));
    Raylib.DrawText("KFC", panelX + 310, panelY + 10, 32, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (kfcOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Raylib.DrawText($"🍗 {kfcOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
        return;
    }
 
    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < items.Length; i++)
    {
        int row = i / 2, col = i % 2;
        int bx = panelX + 20 + col * 340;
        int by = panelY + 90 + row * 140;
 
        bool hover     = Raylib.CheckCollisionPointRec(mouse, new Rectangle(bx, by, 320, 120));
        bool canAfford = player.Money >= prices[i];
 
        Raylib.DrawRectangle(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)60,(byte)20,(byte)10,(byte)255)
                               : new Color((byte)35,(byte)12,(byte)5,(byte)255));
        Raylib.DrawRectangleLines(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)240,(byte)225,(byte)195,(byte)255)
                               : new Color((byte)130,(byte)10,(byte)10,(byte)255));
 
        Raylib.DrawText(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White      : Color.DarkGray);
        Raylib.DrawText($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Gold      : Color.DarkGray);
        Raylib.DrawText(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray  : Color.DarkGray);
        if (!canAfford) Raylib.DrawText("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
        if (hover && canAfford && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            player.Money -= prices[i];
            kfcOrderName  = items[i];
            kfcOrderReady = true;
            kfcOrderTimer = 3f;
            player.Health = Math.Min(player.MaxHealth, player.Health + hpGain[i]);
            kfcMessage      = $"Ordered {items[i]}! Ready in a moment...";
            kfcMessageTimer = 4f;
        }
    }
    Raylib.DrawText("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
}
 
static void DrawBurgerKingMenu()
{
    if (!burgerKingMenuOpen) return;
    if (currentBuilding == null || currentBuilding.BuildingName != "BURGER KING") return;
 
    string[] items        = { "Whopper",        "Chicken Royale",  "Onion Rings",  "Cheese Sticks", "BK Meal",       "Milkshake" };
    int[]    prices       = { 11, 10, 5, 6, 15, 6 };
    string[] descriptions = {
        "Flame-grilled beef burger. +28 HP",
        "Crispy chicken fillet burger. +24 HP",
        "Golden crispy onion rings. +10 HP",
        "Melted mozzarella sticks. +12 HP",
        "Whopper + rings + drink. +38 HP",
        "Thick creamy milkshake. +15 HP"
    };
    int[] hpGain = { 28, 24, 10, 12, 38, 15 };
 
    int panelW = 700, panelH = 540;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = 80;
 
    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)18,(byte)10,(byte)0,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    // Header
    Raylib.DrawRectangle(panelX, panelY, panelW, 50, new Color((byte)185,(byte)20,(byte)20,(byte)255));
    Raylib.DrawText("BURGER KING", panelX + 210, panelY + 10, 32, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (burgerKingOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Raylib.DrawText($"🍔 {burgerKingOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
        return;
    }
 
    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < items.Length; i++)
    {
        int row = i / 2, col = i % 2;
        int bx = panelX + 20 + col * 340;
        int by = panelY + 90 + row * 140;
 
        bool hover     = Raylib.CheckCollisionPointRec(mouse, new Rectangle(bx, by, 320, 120));
        bool canAfford = player.Money >= prices[i];
 
        Raylib.DrawRectangle(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)60,(byte)25,(byte)0,(byte)255)
                               : new Color((byte)35,(byte)15,(byte)0,(byte)255));
        Raylib.DrawRectangleLines(bx, by, 320, 120,
            hover && canAfford ? new Color((byte)255,(byte)180,(byte)0,(byte)255)
                               : new Color((byte)140,(byte)60,(byte)0,(byte)255));
 
        Raylib.DrawText(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White     : Color.DarkGray);
        Raylib.DrawText($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Gold     : Color.DarkGray);
        Raylib.DrawText(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray : Color.DarkGray);
        if (!canAfford) Raylib.DrawText("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
        if (hover && canAfford && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            player.Money -= prices[i];
            burgerKingOrderName  = items[i];
            burgerKingOrderReady = true;
            burgerKingOrderTimer = 3f;
            player.Health = Math.Min(player.MaxHealth, player.Health + hpGain[i]);
            burgerKingMessage      = $"Ordered {items[i]}! Ready in a moment...";
            burgerKingMessageTimer = 4f;
        }
    }
    Raylib.DrawText("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
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

    string[] itemNames = { "Logs", "Fish", "Bones", "Fur", "Stingers", "Pelts",};
    int[] chestCounts = { chestLogs, chestFish, chestBones, chestFur, chestStingers, chestBearPelts, chestDogFangs, };
    int[] invCounts = { player.Logs, player.Fish, player.Bones, player.Fur, player.Stingers, player.BearPelts, player.DogFangs, };

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
                        case 6: player.DogFangs++; chestDogFangs--; break;
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
                        case 6: chestDogFangs++; player.DogFangs--; break;
                    }
                }
            }
        }
    }

    Raylib.DrawText("Q = Close Chest", panelX + 350, panelY + 520, 20, Color.LightGray);
}
static void DrawDealerUI()
{
    if (!dealerUIOpen || currentDealerBuilding == null) return;

    // Panel
    int panelW = 980;
    int panelH = 480;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = 56;

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color(18, 18, 22, 240));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, Color.Gold);
    Raylib.DrawText(currentDealerBuilding.BuildingName, panelX + 20, panelY + 12, 28, Color.Gold);
    Raylib.DrawText($"Wallet: ${player.Money}", panelX + panelW - 240, panelY + 18, 20, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // Layout: left column = options + scroll buttons, right column = preview+stats
    int leftColW = 560;                         // holds 4 slots in a row
    int rightColW = panelW - leftColW - 40;     // preview + stats area
    int leftX = panelX + 20;
    int rightX = panelX + 20 + leftColW + 20;
    int topY = panelY + 60;

    // OPTION SLOTS (4 across, single row)
    int colsVisible = 4;
    int slotGap = 12;
    int slotW = (leftColW - (colsVisible - 1) * slotGap) / colsVisible; // fits evenly
    int slotH = 160;
    int slotY = topY;

    bool isBike = currentDealerType == DealerType.Bike;
    bool isCar  = currentDealerType == DealerType.Car;
    bool isBarn = currentDealerType == DealerType.Barn;

    for (int i = 0; i < colsVisible; i++)
    {
        int idx = dealerScrollOffset + i;
        int bx = leftX + i * (slotW + slotGap);
        int by = slotY;
        Rectangle slotRect = new Rectangle(bx, by, slotW, slotH);

        Raylib.DrawRectangleRec(slotRect, new Color(40, 40, 40, 255));
        Raylib.DrawRectangleLines((int)slotRect.X, (int)slotRect.Y, (int)slotRect.Width, (int)slotRect.Height,
            dealerSelectedIndex == idx ? Color.Gold : Color.White);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
            Raylib.CheckCollisionPointRec(mouse, slotRect))
        {
            dealerSelectedIndex = idx;
        }

        // inside the for loop that draws each slot (replace the per-proto drawing blocks)
int price = 0;

if (isBike && idx < dealerBikeOptions.Count)
{
    var proto = dealerBikeOptions[idx];
Raylib.DrawText(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

// draw live preview Rideable facing RIGHT inside slot
float slotCenterX = bx + slotW / 2f;
float slotCenterY = by + slotH / 2f - 6f;
var slotPreview = new Rideable(new Vector2(slotCenterX - 30f, slotCenterY - 20f), proto.Type, proto.RideableColor);
slotPreview.Facing = Rideable.FacingDirection.Right;
slotPreview.velocity = Vector2.Zero;
slotPreview.Draw();

price = proto.Type == Rideable.RideableType.BMX ? 200 : 300;

}
else if (isCar && idx < dealerVehicleOptions.Count)
{
    var proto = dealerVehicleOptions[idx];
    Raylib.DrawText(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

    // compute center of slot for drawing vehicle
    float drawX = bx + slotW / 2f;
    float drawY = by + slotH / 2f - 6f; // small vertical tweak to align visually

    // create a temporary preview Vehicle positioned to the slot area
    var previewVehicle = new Vehicle(new Vector2(drawX - 50f, drawY - 25f),
                                     proto.VehicleColor,
                                     proto.TopSpeed,
                                     proto.Type)
                                     {MaxFuel = proto.MaxFuel};
                                     

    // force facing left so preview matches the requested orientation
    previewVehicle.Facing = Vehicle.FacingDirection.Left;

    // make sure it's stationary and doesn't use gameplay movement
    previewVehicle.velocity = Vector2.Zero;

    // Draw the vehicle directly (we are inside BeginMode2D with camera so this will line up)
    previewVehicle.Draw();

    // price mapping
    switch (proto.Type)
    {
        case Vehicle.VehicleType.Sedan: price = 1200; break;
        case Vehicle.VehicleType.Truck: price = 1800; break;
        case Vehicle.VehicleType.SUV:   price = 1600; break;
        default: price = 1000; break;
    }
}

else if (isBarn && idx < dealerBarnOptions.Count)
{
    var proto = dealerBarnOptions[idx];
Raylib.DrawText(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

// draw live preview Horse facing RIGHT inside slot
float slotCenterX = bx + slotW / 2f;
float slotCenterY = by + slotH / 2f - 6f;
var slotPreview = new Rideable(new Vector2(slotCenterX - 40f, slotCenterY - 20f), proto.Type, proto.RideableColor);
slotPreview.Facing = Rideable.FacingDirection.Right;
slotPreview.velocity = Vector2.Zero;
slotPreview.Draw();

price = 900;

}
else
{
    Raylib.DrawText("Empty", bx + 8, by + slotH / 2 - 8, 16, Color.DarkGray);
}

// draw price text centered below the preview area
string priceText = $"${price}";
int pw = Raylib.MeasureText(priceText, 16);
int px = bx + (slotW - pw) / 2;
Raylib.DrawText(priceText, px, by + slotH - 22, 16, Color.Gold);

    }

    // Scroll buttons placed directly under the option row, non-overlapping
    int scrollBtnSize = 44;
    int scrollY = slotY + slotH + 12;
    Rectangle leftBtn = new Rectangle(leftX, scrollY, scrollBtnSize, scrollBtnSize);
    Rectangle rightBtn = new Rectangle(leftX + leftColW - scrollBtnSize, scrollY, scrollBtnSize, scrollBtnSize);

    Raylib.DrawRectangleRec(leftBtn, new Color(40, 40, 40, 255));
    Raylib.DrawRectangleRec(rightBtn, new Color(40, 40, 40, 255));
    Raylib.DrawText("<", (int)leftBtn.X + 12, (int)leftBtn.Y + 8, 28, Color.White);
    Raylib.DrawText(">", (int)rightBtn.X + 12, (int)rightBtn.Y + 8, 28, Color.White);

    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (Raylib.CheckCollisionPointRec(mouse, leftBtn) && dealerScrollOffset > 0) dealerScrollOffset--;
        if (Raylib.CheckCollisionPointRec(mouse, rightBtn))
        {
            int maxOffset = 0;
            if (isBike) maxOffset = Math.Max(0, dealerBikeOptions.Count - colsVisible);
            if (isCar)  maxOffset = Math.Max(0, dealerVehicleOptions.Count - colsVisible);
            if (isBarn) maxOffset = Math.Max(0, dealerBarnOptions.Count - colsVisible);
            dealerScrollOffset = Math.Min(maxOffset, dealerScrollOffset + 1);
        }
    }

    // RIGHT COLUMN: Preview pane (top) + Stats pane (bottom)
    int previewW = rightColW;
    int previewH = 260;
    int previewY = topY;
    Raylib.DrawRectangle(rightX, previewY, previewW, previewH, new Color(24, 24, 28, 240));
    Raylib.DrawRectangleLines(rightX, previewY, previewW, previewH, Color.White);

    // Stats area (below preview)
    int statsH = 160;
    int statsY = previewY + previewH + 12;
    Raylib.DrawRectangle(rightX, statsY, previewW, statsH, new Color(20, 20, 24, 240));
    Raylib.DrawRectangleLines(rightX, statsY, previewW, statsH, Color.White);

    // Selected prototype details: compute only if in range
    bool hasSelection = false;
    string title = "No Selection";
    string[] stats = new string[] { "Speed: -", "Handling: -", "Fuel: -", "Durability: -" };
    Color previewColor = new Color(120, 120, 120, 255);

    if (isBike && dealerSelectedIndex < dealerBikeOptions.Count)
    {
         var proto = dealerBikeOptions[dealerSelectedIndex];
 hasSelection = true;
 title = proto.Type.ToString();
 previewColor = proto.RideableColor;
 stats[0] = $"Speed: {(proto.Type == Rideable.RideableType.BMX ? 120 : 100)} km/h";
 stats[1] = $"Handling: {(proto.Type == Rideable.RideableType.BMX ? 85 : 75)}%";
 stats[2] = $"Fuel: N/A";
 stats[3] = $"Durability: 70%";

 // draw accurate rideable preview facing RIGHT
 int cx = rightX + previewW / 2;
 int cy = previewY + previewH / 2 - 8;
 var previewRide = new Rideable(new Vector2(cx - 40, cy - 20), proto.Type, proto.RideableColor);
 previewRide.Facing = Rideable.FacingDirection.Right;
 previewRide.velocity = Vector2.Zero;
 previewRide.Draw();
    }
    else if (isCar && dealerSelectedIndex < dealerVehicleOptions.Count)
    {
        var proto = dealerVehicleOptions[dealerSelectedIndex];
        hasSelection = true;
        title = proto.Type.ToString();
        previewColor = proto.VehicleColor;
        stats[0] = $"Speed: {(int)Math.Clamp(proto.TopSpeed, 0f, 1000f)} km/h";
        stats[1] = $"Handling: {(int)Math.Clamp(50f + proto.MaxFuel * 0.05f, 40f, 95f)}%";
        stats[2] = $"Fuel: {(int)proto.MaxFuel}";
        stats[3] = $"Durability: 75%";

        // accurate vehicle preview facing LEFT
        int cx = rightX + previewW / 2;
        int cy = previewY + previewH / 2 - 8;
        var previewVehicle = new Vehicle(new Vector2(cx - 50, cy - 25), proto.VehicleColor, proto.TopSpeed, proto.Type);
        previewVehicle.Facing = Vehicle.FacingDirection.Left;
        previewVehicle.velocity = Vector2.Zero;
        previewVehicle.Draw();
    }
    else if (isBarn && dealerSelectedIndex < dealerBarnOptions.Count)
    {
        var proto = dealerBarnOptions[dealerSelectedIndex];
 hasSelection = true;
 title = proto.Type.ToString();
 previewColor = proto.RideableColor;
 stats[0] = $"Speed: 60 km/h";
 stats[1] = $"Handling: 70%";
 stats[2] = $"Fuel: N/A";
 stats[3] = $"Durability: 80%";

 int cx = rightX + previewW / 2;
 int cy = previewY + previewH / 2 - 8;
 var previewHorse = new Rideable(new Vector2(cx - 50, cy - 22), proto.Type, proto.RideableColor);
 previewHorse.Facing = Rideable.FacingDirection.Right;
 previewHorse.velocity = Vector2.Zero;
 previewHorse.Draw();
    }

    // Draw title and stats
    Raylib.DrawText(title, rightX + 12, previewY + 12, 20, Color.Gold);
    for (int i = 0; i < stats.Length; i++)
        Raylib.DrawText(stats[i], rightX + 12, statsY + 12 + i * 22, 16, Color.LightGray);

    // BUY button centered within stats area bottom
    Rectangle buyBtn = new Rectangle(rightX + previewW / 2 - 140, statsY + statsH - 64, 280, 44);
    bool hoverBuy = Raylib.CheckCollisionPointRec(mouse, buyBtn);
    Raylib.DrawRectangleRec(buyBtn, new Color(40, 40, 40, 255));
    Raylib.DrawRectangleLinesEx(buyBtn, 2, hoverBuy ? Color.Gold : Color.White);
    Raylib.DrawText(hasSelection ? "BUY & SPAWN OUTSIDE" : "NO ITEM SELECTED", (int)buyBtn.X + 18, (int)buyBtn.Y + 10, 18, hasSelection ? (hoverBuy ? Color.Gold : Color.White) : Color.DarkGray);

    if (hasSelection && hoverBuy && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        int cost = isCar ? 1000 : isBike ? 250 : 800;
        if (player.Money >= cost)
        {
            player.Money -= cost;
            shopMessage = $"Purchased {title}. Spawning outside...";
            shopMessageTimer = 2f;
            Vector2 spawnPos = currentDealerBuilding.ExitPosition + new Vector2(140, 20);

            if (isCar)
            {
                var proto = dealerVehicleOptions[dealerSelectedIndex];
                var spawned = new Vehicle(spawnPos, proto.VehicleColor, proto.TopSpeed, proto.Type);
                vehicles.Add(spawned);
            }
            else
            {
                var proto = (isBike ? dealerBikeOptions[dealerSelectedIndex] : dealerBarnOptions[dealerSelectedIndex]);
                var spawned = new Rideable(spawnPos, proto.Type, proto.RideableColor);
                rideables.Add(spawned);
            }
        }
        else
        {
            shopMessage = "Not enough money!";
            shopMessageTimer = 1.5f;
        }
    }

    // Close hint
    Raylib.DrawText("Q = Close", panelX + panelW / 2 - 40, panelY + panelH - 28, 20, Color.LightGray);
}

static void UpdateMinigameScreen(float dt)
{
    if (activeMinigameType == MinigameType.Pokie)
    {
        activePokieMachine.Update(dt, player);

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
            currentScene = SceneState.Building;
        }
    }
    else if (activeMinigameType == MinigameType.Pool)
    {
        activePoolGame.Update(dt, player);

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            activePoolGame.Close();
            activeMinigameType = MinigameType.None;
            currentScene = SceneState.Building;
        }
    }
}

static void DrawMinigameScreen()
{
    if (activeMinigameType == MinigameType.Pokie)
    {
        Raylib.ClearBackground(Color.White);

        Raylib.DrawRectangle(0, 0, ScreenWidth, 110, new Color((byte)230, (byte)40, (byte)40, (byte)255));
        int titleWidth = Raylib.MeasureText("POKIE MACHINE", 50);
        Raylib.DrawText("POKIE MACHINE", ScreenWidth / 2 - titleWidth / 2, 30, 50, Color.White);

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

        Raylib.DrawText($"Bet: ${activePokieMachine.BetAmount}  (Up/Down to change)", startX, 420, 24,
            new Color((byte)30, (byte)30, (byte)35, (byte)255));
        Raylib.DrawText($"Wallet: ${player.Money}", startX, 460, 24,
            new Color((byte)30, (byte)30, (byte)35, (byte)255));

        if (activePokieMachine.ResultTimer > 0f)
        {
            int msgWidth = Raylib.MeasureText(activePokieMachine.ResultMessage, 26);
            Raylib.DrawText(activePokieMachine.ResultMessage, ScreenWidth / 2 - msgWidth / 2, 500, 26,
                new Color((byte)0, (byte)140, (byte)0, (byte)255));
        }

        string prompt = activePokieMachine.IsSpinning ? "Spinning..."
            : activePokieMachine.HasSpunOnce ? "SPACE = Spin Again | ESC = Leave"
            : "SPACE = Spin | ESC = Leave";
        int promptWidth = Raylib.MeasureText(prompt, 22);
        Raylib.DrawText(prompt, ScreenWidth / 2 - promptWidth / 2, 560, 22,
            new Color((byte)90, (byte)90, (byte)95, (byte)255));
    }
    else if (activeMinigameType == MinigameType.Pool)
    {
        activePoolGame.Draw(player);
    }
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
            int tw = Raylib.MeasureText(text, fontSize);
            Raylib.DrawText(text, cx - tw / 2 + 2, cy - fontSize / 2 + 2, fontSize, new Color((byte)120,(byte)0,(byte)90,(byte)120));
            Raylib.DrawText(text, cx - tw / 2, cy - fontSize / 2, fontSize, new Color((byte)220,(byte)20,(byte)160,(byte)255));
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

        static int GetWeekOfMonth()
        {
            // dayOfMonth ranges 1..14 -> two weeks: days 1-7 = week1, 8-14 = week2
            return (dayOfMonth - 1) / 7 + 1;
        }

        static string GetMonthString() => monthNames[currentMonth];

        static string GetSeasonString()
        {
            int s = monthToSeason[currentMonth];
            return seasons[Math.Clamp(s, 0, seasons.Length - 1)];
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

        case "Axe":
            // Handle
            Raylib.DrawRectangle(cx - 2, cy - 14, 5, 28, new Color((byte)120,(byte)80,(byte)30,(byte)255));
            // Head
            Raylib.DrawTriangle(
                new Vector2(cx + 3, cy - 14),
                new Vector2(cx + 18, cy - 20),
                new Vector2(cx + 14, cy - 2),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            break;

        case "Stick":
            Raylib.DrawRectangle(cx - 2, cy - 18, 5, 36,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - 1, cy - 20, 3, 6,
                new Color((byte)100,(byte)60,(byte)20,(byte)255));
            break;

        case "Sword":
            Raylib.DrawRectangle(cx - 2, cy - 18, 5, 24,
                new Color((byte)180,(byte)190,(byte)200,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx - 2, cy - 18),
                new Vector2(cx + 3, cy - 18),
                new Vector2(cx,     cy - 26),
                new Color((byte)200,(byte)210,(byte)220,(byte)255));
            Raylib.DrawRectangle(cx - 10, cy + 6, 21, 5,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - 2, cy + 11, 5, 12,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawCircle(cx, cy + 23, 4,
                new Color((byte)180,(byte)140,(byte)40,(byte)255));
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
            musicBeforeRain = currentMusic;
            SwitchMusic(musicRain); // ← starts rain track
        }
        else
        {
             lastZoneMusic = default;
            CheckZoneMusic();
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
        Rectangle miningBtn   = new Rectangle(ScreenWidth - 160, ScreenHeight - 430, 140, 40);

        hoverStrength  = Raylib.CheckCollisionPointRec(mouse, strengthBtn);
        hoverAthletics = Raylib.CheckCollisionPointRec(mouse, athleticsBtn);
        hoverDriving   = Raylib.CheckCollisionPointRec(mouse, drivingBtn);
        hoverCombat = Raylib.CheckCollisionPointRec(mouse, combatBtn);
        hoverWoodcutting = Raylib.CheckCollisionPointRec(mouse, wcBtn);
        hoverFishing = Raylib.CheckCollisionPointRec(mouse, fishBtn);
        hoverMining = Raylib.CheckCollisionPointRec(mouse, miningBtn);
    }
    else
    {
        hoverWoodcutting = false;
        hoverFishing = false;
        hoverCombat = false;
        hoverStrength  = false;
        hoverAthletics = false;
        hoverDriving   = false;
        hoverMining   = false;
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

    // Mining
    Rectangle miningBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 430, 140, 40);
    Color miningColor = hoverMining ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(miningBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(miningBtn, 2, miningColor);
    Raylib.DrawText($"Mining Lv {player.MiningLevel}", ScreenWidth - 155, ScreenHeight - 418, 20, miningColor);
    if (!hoverMining)
    {
        int miningRequired = player.MiningLevel * player.MiningLevel * 50;
        float miningProgress = (float)player.MiningXP / miningRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 93, (int)(140 * miningProgress), 8, Color.Green);
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
    if (hoverMining)
    {
        int required = player.MiningLevel * player.MiningLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 430, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Raylib.DrawText($"XP: {player.MiningXP}/{required}", ScreenWidth - 315, ScreenHeight - 418, 20, Color.LightGray);
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

    // Ocean/Beach right (X > 26000)
    int beachX = cx + (int)((26000 - player.Position.X) * minimapScale);
    int oceanX = cx + (int)((30000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(beachX, minimapY, Math.Clamp(oceanX - beachX, 0, minimapSize), minimapSize, new Color((byte)240,(byte)220,(byte)150,(byte)220)); // sand
    Raylib.DrawRectangle(oceanX, minimapY, Math.Clamp(minimapX + minimapSize - oceanX, 0, minimapSize), minimapSize, new Color((byte)30,(byte)100,(byte)180,(byte)220)); // deep water

    // Swamp far left (X < -30000)
    int swampRightX = cx + (int)((-30000 - player.Position.X) * minimapScale);
    Raylib.DrawRectangle(minimapX, minimapY, Math.Clamp(swampRightX - minimapX, 0, minimapSize), minimapSize, new Color((byte)55,(byte)75,(byte)35,(byte)220));

    // Volcano — far right, top (X > 26000, Y < -2000)
    int volcanoLeftX  = cx + (int)((26000 - player.Position.X) * minimapScale);
    int volcanoBottomY = cy + (int)((-2000 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(
        Math.Clamp(volcanoLeftX, minimapX, minimapX + minimapSize),
        minimapY,
        Math.Clamp(minimapX + minimapSize - volcanoLeftX, 0, minimapSize),
        Math.Clamp(volcanoBottomY - minimapY, 0, minimapSize),
        new Color((byte)40,(byte)20,(byte)10,(byte)220));

    // Mountains — far left, top (X < -26000, Y < -2000)
    int mountainRightX  = cx + (int)((-26000 - player.Position.X) * minimapScale);
    int mountainBottomY = cy + (int)((-2000 - player.Position.Y) * minimapScale);
    Raylib.DrawRectangle(
        minimapX,
        minimapY,
        Math.Clamp(mountainRightX - minimapX, 0, minimapSize),
        Math.Clamp(mountainBottomY - minimapY, 0, minimapSize),
        new Color((byte)100,(byte)95,(byte)90,(byte)220));

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
            Raylib.InitAudioDevice();
            
            if (Raylib.IsAudioDeviceReady())
        {
            musicMainMenu = Raylib.LoadMusicStream("resources/music/Meadow Thoughts.ogg");
            musicDbar   = Raylib.LoadMusicStream("resources/music/dbarMusic.mp3");
            musicFarm  = Raylib.LoadMusicStream("resources/music/Salt Marsh Birds.mp3");
            musicHouse = Raylib.LoadMusicStream("resources/music/Meadow Thoughts.ogg");
            musicRain = Raylib.LoadMusicStream("resources/music/rainMusic.ogg");
            musicForest = Raylib.LoadMusicStream("resources/music/forestMusic.mp3");
            musicBeach = Raylib.LoadMusicStream("resources/music/forestMusic.mp3");
            musicDesert = Raylib.LoadMusicStream("resources/music/forestMusic.mp3");
            musicSnow = Raylib.LoadMusicStream("resources/music/forestMusic.mp3");
            musicTakeaways = Raylib.LoadMusicStream("resources/music/takeawayMusic.ogg");

            // Load these once at startup/asset loading section
            soundPauseOpen  = Raylib.LoadSound("resources/sound/pauseOpen.wav");
            soundPauseClose = Raylib.LoadSound("resources/sound/pauseClose.wav");
            soundTreeChop  = Raylib.LoadSound("resources/sound/treeChop.wav");
            soundTreeFall  = Raylib.LoadSound("resources/sound/treeFall.wav");
            soundRockHit   = Raylib.LoadSound("resources/sound/rockHit.wav");
            soundRockBreak = Raylib.LoadSound("resources/sound/rockBreak.wav");
            soundHorseGallop = Raylib.LoadSound("resources/sound/horseGallop.ogg");
            soundSwordSwing = Raylib.LoadSound("resources/sound/swordSwing.wav");
            soundStickSwing = Raylib.LoadSound("resources/sound/stickSwing.wav");
            soundDogHit     = Raylib.LoadSound("resources/sound/dogHit.wav");
            soundDogDie     = Raylib.LoadSound("resources/sound/dogDie.ogg");
        

            Raylib.SetMusicVolume(musicMainMenu, musicVolume);
            Raylib.SetMusicVolume(musicDbar,   musicVolume);
            Raylib.SetMusicVolume(musicFarm,   musicVolume);
            Raylib.SetMusicVolume(musicHouse, musicVolume);
            Raylib.SetMusicVolume(musicRain, musicVolume);
            Raylib.SetMusicVolume(musicForest, musicVolume);
            Raylib.SetMusicVolume(musicBeach, musicVolume);
            Raylib.SetMusicVolume(musicDesert, musicVolume);
            Raylib.SetMusicVolume(musicSnow, musicVolume);
            Raylib.SetMusicVolume(musicTakeaways, musicVolume);

            Raylib.SetSoundVolume(soundPauseOpen, soundVolume);
            Raylib.SetSoundVolume(soundPauseClose, soundVolume);
            Raylib.SetSoundVolume(soundSwordSwing, soundVolume);
            Raylib.SetSoundVolume(soundStickSwing, soundVolume);
            Raylib.SetSoundVolume(soundDogHit,     soundVolume);
            Raylib.SetSoundVolume(soundDogDie,     soundVolume);

            // Start on main menu track
            currentMusic = musicMainMenu;
            Raylib.PlayMusicStream(currentMusic);
            musicLoaded = true;
            musicPlaying = true;
        }

            GenerateWorld();

            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Zoom = 1f;

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                if (musicLoaded)
                {
                Raylib.UpdateMusicStream(currentMusic);
                UpdateMusicFade(dt);
                }
                Update(dt);
                Draw();
            }
            if (musicLoaded)
{
    Raylib.StopMusicStream(currentMusic);
    Raylib.UnloadMusicStream(musicMainMenu);
    Raylib.UnloadMusicStream(musicDbar);
    Raylib.UnloadMusicStream(musicTakeaways);
    Raylib.UnloadMusicStream(musicBeach);
    Raylib.UnloadMusicStream(musicDesert);
    Raylib.UnloadMusicStream(musicSnow);

    Raylib.UnloadSound(soundPauseOpen);   
    Raylib.UnloadSound(soundPauseClose);
    Raylib.UnloadSound(soundRockHit);
    Raylib.UnloadSound(soundRockBreak);
    Raylib.UnloadSound(soundTreeChop);
    Raylib.UnloadSound(soundTreeFall);
    Raylib.UnloadSound(soundHorseGallop);
    Raylib.UnloadSound(soundSwordSwing);
    Raylib.UnloadSound(soundStickSwing);
    Raylib.UnloadSound(soundDogHit);
    Raylib.UnloadSound(soundDogDie);
    Raylib.CloseAudioDevice();
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
                    player = new Player(new Vector2(-1917, -9720));
                    chestLogs = 0; chestFish = 0; chestBones = 0;
                    chestFur = 0; chestStingers = 0; chestBearPelts = 0;
                    chestDogFangs = 0;
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


                CheckZoneMusic();

                // Toolbar slot selection — number keys 1-8
for (int k = 0; k < 8; k++)
{
    if (Raylib.IsKeyPressed(KeyboardKey.One   + k))
        toolbarSelectedSlot = k;
}
// Also mouse scroll wheel
float scroll = Raylib.GetMouseWheelMove();
if (scroll != 0)
    camera.Zoom = Math.Clamp(camera.Zoom + scroll * 0.1f, 0.2f, 4.0f);

                // Axe pickup
                if (!axePickedUp && Vector2.Distance(player.Position, axePosition) < 60)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.E))
                    {
                        player.HasAxe = true;
                        axePickedUp = true;
                        AddToToolbar("Axe");
                        ShowNotification("You picked up an Axe! You can now chop trees.");
                    }
                }

                // Pickaxe pickup
if (!pickaxePickedUp && Vector2.Distance(player.Position, pickaxePosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        pickaxePickedUp = true;
        AddToToolbar("Pickaxe");
        ShowNotification("Pickaxe added to toolbar!");
    }
}

// Fishing rod pickup
if (!fishingRodPickedUp && Vector2.Distance(player.Position, fishingRodPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        fishingRodPickedUp = true;
        AddToToolbar("Rod");
        ShowNotification("Fishing Rod added to toolbar! Equip it near lakes.");
    }
}

// Fishing net pickup
if (!fishingNetPickedUp && Vector2.Distance(player.Position, fishingNetPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        fishingNetPickedUp = true;
        AddToToolbar("Net");
        ShowNotification("Fishing Net added to toolbar!");
    }
}

// Torch pickup
if (!torchPickedUp && Vector2.Distance(player.Position, torchPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        torchPickedUp = true;
        AddToToolbar("Torch");
        ShowNotification("Torch added to toolbar!");
    }
}

// Stick pickup
if (!stickPickedUp && Vector2.Distance(player.Position, stickPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        stickPickedUp = true;
        AddToToolbar("Stick");
        ShowNotification("Stick added to toolbar! Equip it to fight.");
    }
}

// Sword pickup
if (!swordPickedUp && Vector2.Distance(player.Position, swordPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E))
    {
        swordPickedUp = true;
        AddToToolbar("Sword");
        ShowNotification("Sword added to toolbar! Much better than a stick.");
    }
}

if (Raylib.IsKeyPressed(KeyboardKey.G))
    armorMenuOpen = !armorMenuOpen;

                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                        {
                            bool wasOpen = pauseMenuOpen;
                            pauseMenuOpen = !pauseMenuOpen;
                            optionsMenuOpen = false;
                            cheatsMenuOpen = false;
                            loadMenuOpen = false;
                            mapOpen = false;

                            if (!wasOpen)
                            { // was closed, now open
                            Raylib.PlaySound(soundPauseOpen);
                            Raylib.PauseMusicStream(currentMusic);
                            }
                        else
                        {
                           Raylib.ResumeMusicStream(currentMusic);
                            Raylib.PlaySound(soundPauseClose); 
                        }
                     
                        }

                        if (!pauseMenuOpen)
                        {
                        
                    //Raylib.ResumeMusicStream(currentMusic);
                    //Raylib.PlaySound(soundPauseClose);
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
                    player.Position = new Vector2(400, -50);
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

                        // advance weekday
                        dayOfWeek = (dayOfWeek + 1) % 7;

                        // advance calendar day (14-day months)
                        dayOfMonth++;
                        dayCounter += 1f; // optional tracker if you use elsewhere

                        if (dayOfMonth > 14)
                        {
                            dayOfMonth = 1;
                            currentMonth = (currentMonth + 1) % 12;
                        }
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
                                case "Dog Fang":
                                    player.DogFangs++;
                                    break;
                                case "Wolf Claw":
                                    player.BearPelts++;
                                    break;
                                case "Venom sac":
                                    player.BearPelts++;
                                    break;
                                case "Crab Claw":
                                    player.BearPelts++;
                                    break;
                                case "Bear Claw":
                                    player.BearPelts++;
                                    break;
                                case "Crab Shell":
                                    player.BearPelts++;
                                    break;
                                case "Shark Fin":
                                    player.BearPelts++;
                                    break;
                                case "Shark Tooth":
                                    player.BearPelts++;
                                    break;
                                case "Snake Fang":
                                    player.BearPelts++;
                                    break;
                                case "Croc Scale":
                                    player.BearPelts++;
                                    break;
                                case "Croc Tooth":
                                    player.BearPelts++;
                                    break;
                                case "Lizard Scale":
                                    player.BearPelts++;
                                    break;
                                case "Ember Stone":
                                    player.BearPelts++;
                                    break;
                                case "Magma Shard":
                                    player.BearPelts++;
                                    break;
                                case "Lava Core":
                                    player.BearPelts++;
                                    break;
                                case "Feather":
                                    player.BearPelts++;
                                    break;
                                case "Eagle Talon":
                                    player.BearPelts++;
                                    break;
                                case "Goat Hoof":
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

if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    string equipped = GetEquippedTool();
    if (equipped == "Sword")
        Raylib.PlaySound(soundSwordSwing);
    else if (equipped == "Stick")
        Raylib.PlaySound(soundStickSwing);
    else if (equipped == "Axe")
        Raylib.PlaySound(soundStickSwing);
    else if (equipped == "Pickaxe")
        Raylib.PlaySound(soundStickSwing);
    else if (equipped == "Fishing rod")
        Raylib.PlaySound(soundStickSwing);
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
                else if (enemy.Type == "Crab")          damage = 12;
                else if (enemy.Type == "Shark")         damage = 18;
                else if (enemy.Type == "Snake")         damage = 14;
                else if (enemy.Type == "Crocodile")     damage = 20;
                else if (enemy.Type == "Fire Lizard")   damage = 16;
                else if (enemy.Type == "Magma Beetle")  damage = 22;
                else if (enemy.Type == "Eagle")         damage = 13;
                else if (enemy.Type == "Mountain Goat") damage = 17;

                int defense = GetTotalDefense();
                int reducedDamage = Math.Max(1, damage - defense);
                player.TakeDamage(reducedDamage);
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
    string equipped = GetEquippedTool();
    bool hasWeapon = equipped == "Stick" || equipped == "Sword";

    if (!hasWeapon)
    {
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 40),
            Text = "Equip a weapon first!",
            Timer = 1.5f,
            TextColor = Color.Red
        });
    }
    else
    {
        int weaponBonus = equipped == "Sword" ? 5 : 0;
        int slotBonus   = GetWeaponDamage(armorWeapon); // bonus from equipped armor slot weapon
        int attackDamage = 1 + (player.CombatLevel / 10) + weaponBonus + slotBonus;
        enemy.Health -= attackDamage;

        // dog hit sound
        if (enemy.Type == "Wild Dog")
            Raylib.PlaySound(soundDogHit);

        floatingTexts.Add(new FloatingText {
            Position = enemy.Position - new Vector2(0, 20),
            Text = $"-{attackDamage}",
            Timer = 1f,
            TextColor = equipped == "Sword" ? Color.Orange : Color.Red
        });

        TriggerShake(0.1f);

        if (enemy.Health <= 0)
        {
            enemy.Dead = true;

                if (enemy.Type == "Wild Dog") Raylib.PlaySound(soundDogDie);

                if (enemy.Type == "Wild Dog") player.AddCombatXP(20);
                else if (enemy.Type == "Wolf") player.AddCombatXP(35);
                else if (enemy.Type == "Scorpion") player.AddCombatXP(30);
                else if (enemy.Type == "Bear") player.AddCombatXP(50);
                else if (enemy.Type == "Crab")          player.AddCombatXP(55);
                else if (enemy.Type == "Shark")         player.AddCombatXP(70);
                else if (enemy.Type == "Snake")         player.AddCombatXP(55);
                else if (enemy.Type == "Crocodile")     player.AddCombatXP(75);
                else if (enemy.Type == "Fire Lizard")   player.AddCombatXP(65);
                else if (enemy.Type == "Magma Beetle")  player.AddCombatXP(80);
                else if (enemy.Type == "Eagle")         player.AddCombatXP(60);
                else if (enemy.Type == "Mountain Goat") player.AddCombatXP(70);

        // Loot drops
        int rareRoll = Raylib.GetRandomValue(1, 100);

if (enemy.Type == "Wild Dog") {
    lootDrops.Add(new LootDrop(enemy.Position, "Bone"));
    if (rareRoll <= 20) lootDrops.Add(new LootDrop(enemy.Position, "Fur"));          // 20% extra fur
    if (rareRoll <= 5)  lootDrops.Add(new LootDrop(enemy.Position, "Dog Fang"));     // 5% rare fang
}
else if (enemy.Type == "Wolf") {
    lootDrops.Add(new LootDrop(enemy.Position, "Fur"));
    if (rareRoll <= 25) lootDrops.Add(new LootDrop(enemy.Position, "Bone"));         // 25% bone
    if (rareRoll <= 8)  lootDrops.Add(new LootDrop(enemy.Position, "Wolf Claw"));    // 8% rare claw
}
else if (enemy.Type == "Scorpion") {
    lootDrops.Add(new LootDrop(enemy.Position, "Stinger"));
    if (rareRoll <= 20) lootDrops.Add(new LootDrop(enemy.Position, "Stinger"));      // 20% extra stinger
    if (rareRoll <= 6)  lootDrops.Add(new LootDrop(enemy.Position, "Venom Sac"));    // 6% rare venom
}
else if (enemy.Type == "Bear") {
    lootDrops.Add(new LootDrop(enemy.Position, "Bear Pelt"));
    if (rareRoll <= 30) lootDrops.Add(new LootDrop(enemy.Position, "Fur"));          // 30% fur
    if (rareRoll <= 7)  lootDrops.Add(new LootDrop(enemy.Position, "Bear Claw"));    // 7% rare claw
}
else if (enemy.Type == "Crab") {
    lootDrops.Add(new LootDrop(enemy.Position, "Crab Claw"));
    if (rareRoll <= 25) lootDrops.Add(new LootDrop(enemy.Position, "Crab Claw"));    // 25% extra claw
    if (rareRoll <= 8)  lootDrops.Add(new LootDrop(enemy.Position, "Crab Shell"));   // 8% rare shell
}
else if (enemy.Type == "Shark") {
    lootDrops.Add(new LootDrop(enemy.Position, "Shark Fin"));
    if (rareRoll <= 20) lootDrops.Add(new LootDrop(enemy.Position, "Shark Fin"));    // 20% extra fin
    if (rareRoll <= 5)  lootDrops.Add(new LootDrop(enemy.Position, "Shark Tooth"));  // 5% rare tooth
}
else if (enemy.Type == "Snake") {
    lootDrops.Add(new LootDrop(enemy.Position, "Snake Skin"));
    if (rareRoll <= 20) lootDrops.Add(new LootDrop(enemy.Position, "Stinger"));      // 20% venom fang (reuse stinger)
    if (rareRoll <= 6)  lootDrops.Add(new LootDrop(enemy.Position, "Snake Fang"));   // 6% rare fang
}
else if (enemy.Type == "Crocodile") {
    lootDrops.Add(new LootDrop(enemy.Position, "Croc Scale"));
    if (rareRoll <= 25) lootDrops.Add(new LootDrop(enemy.Position, "Croc Scale"));   // 25% extra scale
    if (rareRoll <= 7)  lootDrops.Add(new LootDrop(enemy.Position, "Croc Tooth"));   // 7% rare tooth
}
else if (enemy.Type == "Fire Lizard") {
    lootDrops.Add(new LootDrop(enemy.Position, "Lizard Scale"));
    if (rareRoll <= 20) lootDrops.Add(new LootDrop(enemy.Position, "Lizard Scale")); // 20% extra scale
    if (rareRoll <= 6)  lootDrops.Add(new LootDrop(enemy.Position, "Ember Stone"));  // 6% rare ember
}
else if (enemy.Type == "Magma Beetle") {
    lootDrops.Add(new LootDrop(enemy.Position, "Magma Shard"));
    if (rareRoll <= 25) lootDrops.Add(new LootDrop(enemy.Position, "Magma Shard"));  // 25% extra shard
    if (rareRoll <= 5)  lootDrops.Add(new LootDrop(enemy.Position, "Lava Core"));    // 5% rare core
}
else if (enemy.Type == "Eagle") {
    lootDrops.Add(new LootDrop(enemy.Position, "Feather"));
    if (rareRoll <= 30) lootDrops.Add(new LootDrop(enemy.Position, "Feather"));      // 30% extra feather
    if (rareRoll <= 7)  lootDrops.Add(new LootDrop(enemy.Position, "Eagle Talon"));  // 7% rare talon
}
else if (enemy.Type == "Mountain Goat") {
    lootDrops.Add(new LootDrop(enemy.Position, "Horn"));
    if (rareRoll <= 25) lootDrops.Add(new LootDrop(enemy.Position, "Fur"));          // 25% fur
    if (rareRoll <= 8)  lootDrops.Add(new LootDrop(enemy.Position, "Goat Hoof"));    // 8% rare hoof
}
                        }
    }
                    }
                }
            }
                    

                    player.Update(dt, buildings, trees, vehicles);

                    foreach (Vehicle vehicle in vehicles)
            {
                Vector2 vehicleOldPos = vehicle.Position;
                vehicle.Update(dt, buildings, trees, vehicles);

                foreach (Rectangle fenceRect in fenceManager.GetCollisionRects())
    {
        if (Raylib.CheckCollisionRecs(vehicle.Bounds, fenceRect))
        {
            vehicle.Position = vehicleOldPos;
            vehicle.velocity = Vector2.Zero;
            break;
        }
    }
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

            foreach (Rideable rideable in rideables)
{
    Vector2 rideableOldPos = rideable.Position;
    rideable.Update(dt, buildings, trees, vehicles, rideables);
              foreach (Rectangle fenceRect in fenceManager.GetCollisionRects())
    {
        if (Raylib.CheckCollisionRecs(rideable.Bounds, fenceRect))
        {
            rideable.Position = rideableOldPos;
            rideable.velocity = Vector2.Zero;
            break;
        }
    }

    if (rideable.Riding)
    {
        player.Position = rideable.Position;
        player.Hidden = true;

        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            rideable.Riding = false;
            player.Hidden = false;
            player.Position = new Vector2(rideable.Position.X + 70, rideable.Position.Y);
        }
    }
    else
    {
        if (Raylib.CheckCollisionRecs(player.Bounds, rideable.Bounds))
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F))
            {
                // dismount any vehicle first
                foreach (Vehicle v in vehicles)
                    if (v.Driving) { v.Driving = false; player.Hidden = false; }

                rideable.Riding = true;
                player.Hidden = true;
            }
        }
    }

    // give athletics XP while riding bikes, and athletics+strength for horse
    if (rideable.Riding)
    {
        if (rideable.Type != Rideable.RideableType.Horse)
            player.AddAthleticsXP(1);
        else
        {
            player.AddAthleticsXP(1);
            player.AddStrengthXP(1);
        }
    }
}

 // gas pump interaction
foreach (GasStation station in gasStations)
{
    station.Pump1Active = false;
    station.Pump2Active = false;

    foreach (Vehicle vehicle in vehicles)
    {
        float distVehicleP1 = Vector2.Distance(vehicle.Position, station.Pump1Pos);
        float distVehicleP2 = Vector2.Distance(vehicle.Position, station.Pump2Pos);
        float distPlayerP1  = Vector2.Distance(player.Position,  station.Pump1Pos);
        float distPlayerP2  = Vector2.Distance(player.Position,  station.Pump2Pos);

        bool canFuelP1 = distVehicleP1 < 120 && (distPlayerP1 < 150 || vehicle.Driving);
        bool canFuelP2 = distVehicleP2 < 120 && (distPlayerP2 < 150 || vehicle.Driving);

        if (distVehicleP1 < 120) station.Pump1Active = true;
        if (distVehicleP2 < 120) station.Pump2Active = true;

        if (canFuelP1 && Raylib.IsKeyDown(KeyboardKey.R) && vehicle.Fuel < vehicle.MaxFuel)
        {
            vehicle.Refuel(station.PumpFuelRate * dt);
            vehicle.NeedsPayment = true;
            vehicle.FuelLocked = true;
        }

        if (canFuelP2 && Raylib.IsKeyDown(KeyboardKey.R) && vehicle.Fuel < vehicle.MaxFuel)
        {
            vehicle.Refuel(station.PumpFuelRate * dt);
            vehicle.NeedsPayment = true;
            vehicle.FuelLocked = true;
        }
    }
}   

                    foreach (NPC npc in npcs)
                    {
                        npc.Update(dt);
                    }
    foreach (RockObject rock in rocks)
{
    rock.Update(dt);

    if (!rock.Broken && !nearEnemy)
    {
        if (Vector2.Distance(player.Position, rock.Position) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (GetEquippedTool() != "Pickaxe")
                {
                    floatingTexts.Add(new FloatingText {
                        Position = player.Position - new Vector2(0, 40),
                        Text = "Equip your Pickaxe!",
                        Timer = 1.5f,
                        TextColor = Color.Red
                    });
                }
                else if (player.MiningLevel < rock.LevelRequired)
                {
                    floatingTexts.Add(new FloatingText {
                        Position = player.Position - new Vector2(0, 40),
                        Text = $"Need Mining level {rock.LevelRequired}!",
                        Timer = 1.5f,
                        TextColor = Color.Red
                    });
                }
                else
                {
                    rock.Health--;
                    rock.IsBeingMined = true;  
                    rock.MineAnimTimer = 0f;
                    player.TriggerMineAnim(); 
                    Raylib.PlaySound(soundRockHit);
                    TriggerShake(0.1f);

                    if (rock.Health <= 0)
                    {
                        rock.Broken = true;
                        Raylib.PlaySound(soundRockBreak);
                        player.AddMiningXP(rock.XPReward);
                        int oreGained = Raylib.GetRandomValue(1, 3);

                        switch (rock.OreType)
                        {
                            case "Stone":     player.StoneOre  += oreGained; break;
                            case "Copper Ore":player.CopperOre += oreGained; break;
                            case "Iron Ore":  player.IronOre   += oreGained; break;
                            case "Gold Ore":  player.GoldOre   += oreGained; break;
                            case "Crystal":   player.Crystals  += oreGained; break;
                        }

                        floatingTexts.Add(new FloatingText {
                            Position = player.Position - new Vector2(0, 20),
                            Text = $"+{rock.XPReward} Mining XP",
                            Timer = 1.2f,
                            TextColor = Color.Orange
                        });
                        floatingTexts.Add(new FloatingText {
                            Position = player.Position - new Vector2(0, 44),
                            Text = $"+{oreGained} {rock.OreType}",
                            Timer = 1.2f,
                            TextColor = Color.Yellow
                        });
                    }
                }
            }
        }
    }
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
    if (GetEquippedTool() != "Axe")
{
    floatingTexts.Add(new FloatingText {
        Position = player.Position - new Vector2(0, 40),
        Text = player.HasAxe ? "Equip your Axe!" : "Need an axe!",
        Timer = 1.5f,
        TextColor = Color.Red
    });
}
    else if (player.WoodcuttingLevel < tree.LevelRequired)
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
                    tree.IsBeingChopped = true;  
                    tree.ChopAnimTimer = 0f;
                    player.TriggerChopAnim();
                    Raylib.PlaySound(soundTreeChop);

                    if (tree.Health <= 0)
                    {
                        tree.Chopped = true;
                        Raylib.PlaySound(soundTreeFall);
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
                       if (Raylib.IsKeyPressed(KeyboardKey.Space) && !isFishing)
                        {
                            string tool = GetEquippedTool();
                            if (tool == "Rod" || tool == "Net")
                            {
                                isFishing = true;
                                fishingTimer = 0f;
                            }
                            else
                            {
                                ShowNotification("Equip a Fishing Rod or Net first!");
                            }
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
                                player.Position = currentBuilding.EntryPosition;
                                if (building.BuildingName == "MY HOUSE")
                                SwitchMusic(musicHouse);
                               else if (building.BuildingName == "McDONALD'S")
                                SwitchMusic(musicTakeaways);
                               else if (building.BuildingName == "KFC")
                                SwitchMusic(musicTakeaways);
                              else if (building.BuildingName == "BURGER KING")
                                SwitchMusic(musicTakeaways);
                              else if (building.BuildingName == "DBar")
                                SwitchMusic(musicDbar);
                                else
                                SwitchMusic(currentMusic);
                                break;
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
    Vector2 wardrobePos = new Vector2(1080, 810);  // centre of wardrobe (1030,760,300,190)
    Vector2 chestPos    = new Vector2(872, 915);   // centre of chest (820,870,105,90)
    Vector2 bedPos      = new Vector2(1180, 585);  // centre of bed (1030,450,300,270)

    for (int i = 0; i < 3; i++)
    {
        Rectangle tabBtn = new Rectangle(ScreenWidth / 2 - 280 + i * 140, 160, 120, 36);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
            Raylib.CheckCollisionPointRec(mouse, tabBtn))
            wardrobeTab = i;
    }

    bool nearWardrobe = Vector2.Distance(player.Position, wardrobePos) < 200;
    bool nearChest    = Vector2.Distance(player.Position, chestPos)    < 200;
    bool nearBed      = Vector2.Distance(player.Position, bedPos)      < 200;

    if (!chestOpen && nearWardrobe)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E))
            wardrobeOpen = !wardrobeOpen;
    }

    if (!wardrobeOpen && nearChest)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E))
            chestOpen = !chestOpen;
    }

    if (!wardrobeOpen && !chestOpen && nearBed)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Z))
        {
            player.DrunkLevel = 0;
            player.DrunkTimer = 0f;
            player.Health = player.MaxHealth;
            shopMessage = "You slept it off. Fully rested and healed!";
            shopMessageTimer = 2f;
        }
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
if (dealerUIOpen && Raylib.IsKeyPressed(KeyboardKey.Q))
{
    dealerUIOpen = false;
    currentDealerType = DealerType.None;
    dealerSelectedIndex = 0;
    dealerScrollOffset = 0;
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
        && Raylib.IsKeyPressed(KeyboardKey.Space))
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

// ── McDONALD'S ──────────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "McDONALD'S")
{
    if (mcdonaldsMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            mcdonaldsMenuOpen = false;
            mcdonaldsSelectedItem = -1;
        }
    }
    else
    {
        if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                mcdonaldsMenuOpen = true;
        }
    }

    if (mcdonaldsOrderReady)
    {
        mcdonaldsOrderTimer -= dt;
        if (mcdonaldsOrderTimer <= 0)
        {
            mcdonaldsOrderReady = false;
            mcdonaldsMessage = $"Order ready! Enjoy your {mcdonaldsOrderName}. +20 HP";
            mcdonaldsMessageTimer = 3f;
            player.Health = Math.Min(player.MaxHealth, player.Health + 20);
        }
    }
    if (mcdonaldsMessageTimer > 0) mcdonaldsMessageTimer -= dt;
}
if (currentBuilding.BuildingName == "DOMINO'S")
{
    if (dominosMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            dominosMenuOpen = false;
            dominosSelectedItem = -1;
        }
    }
    else
    {
        if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                dominosMenuOpen = true;
        }
    }
 
    if (dominosOrderReady)
    {
        dominosOrderTimer -= dt;
        if (dominosOrderTimer <= 0)
        {
            dominosOrderReady = false;
            dominosMessage = $"Order ready! Enjoy your {dominosOrderName}.";
            dominosMessageTimer = 3f;
        }
    }
    if (dominosMessageTimer > 0) dominosMessageTimer -= dt;
}
 
// ── KFC BUILDING LOGIC ────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "KFC")
{
    if (kfcMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            kfcMenuOpen = false;
            kfcSelectedItem = -1;
        }
    }
    else
    {
        if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                kfcMenuOpen = true;
        }
    }
 
    if (kfcOrderReady)
    {
        kfcOrderTimer -= dt;
        if (kfcOrderTimer <= 0)
        {
            kfcOrderReady = false;
            kfcMessage = $"Order ready! Enjoy your {kfcOrderName}.";
            kfcMessageTimer = 3f;
        }
    }
    if (kfcMessageTimer > 0) kfcMessageTimer -= dt;
}
 
// ── BURGER KING BUILDING LOGIC ────────────────────────────────────────────
if (currentBuilding.BuildingName == "BURGER KING")
{
    if (burgerKingMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            burgerKingMenuOpen = false;
            burgerKingSelectedItem = -1;
        }
    }
    else
    {
        if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
                burgerKingMenuOpen = true;
        }
    }
 
    if (burgerKingOrderReady)
    {
        burgerKingOrderTimer -= dt;
        if (burgerKingOrderTimer <= 0)
        {
            burgerKingOrderReady = false;
            burgerKingMessage = $"Order ready! Enjoy your {burgerKingOrderName}.";
            burgerKingMessageTimer = 3f;
        }
    }
    if (burgerKingMessageTimer > 0) burgerKingMessageTimer -= dt;
}

// ── SWIMMING COMPLEX ─────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "SWIMMING COMPLEX")
{
    // Lane pool zone: x 50-550, y 100-440
    Vector2 lanePoolCenter = new Vector2(300, 270);
    Vector2 divingBoardPos = new Vector2(780, 200);

    bool nearLanePool  = player.Position.X > 40  && player.Position.X < 560
                      && player.Position.Y > 90 && player.Position.Y < 700;
    bool nearDivingBoard = player.Position.X > 690 && player.Position.X < 870
                    && player.Position.Y > 90  && player.Position.Y < 700;

    if (!swimmingActive && !divingActive)
    {
        if (nearLanePool && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            swimmingActive = true;
            swimmingPoolType = "lane";
            swimLapTimer = 0f;
            swimLapsCompleted = 0;
        }
        if (nearDivingBoard && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            swimmingActive = true;
            swimmingPoolType = "diving";
            divingActive = false;
            divingBarPos = 0f;
            divingBarDir = 1f;
            divingJumped = false;
            divingScore = 0;
            divingResult = "";
        }
    }

    if (swimmingActive && swimmingPoolType == "lane")
    {
        swimLapTimer += dt;
        if (swimLapTimer >= swimLapDuration)
        {
            swimLapTimer = 0f;
            swimLapsCompleted++;
            player.AddAthleticsXP(30);
            shopMessage = $"Lap {swimLapsCompleted} complete! +30 Athletics XP";
            shopMessageTimer = 1.2f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            swimmingActive = false;
            shopMessage = $"Finished swimming! {swimLapsCompleted} laps completed.";
            shopMessageTimer = 2f;
        }
    }

    if (swimmingActive && swimmingPoolType == "diving")
    {
        if (!divingJumped)
        {
            // Move power bar
            divingBarPos += divingBarDir * divingBarSpeed * dt;
            if (divingBarPos >= 1f) { divingBarPos = 1f; divingBarDir = -1f; }
            if (divingBarPos <= 0f) { divingBarPos = 0f; divingBarDir = 1f; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                divingJumped = true;
                divingFallTimer = 0f;
                // Score based on how close to centre (0.5)
                float dist = MathF.Abs(divingBarPos - 0.5f);
                if (dist < 0.05f)      { divingScore = 10; divingResult = "PERFECT DIVE! 10/10"; }
                else if (dist < 0.12f) { divingScore = 8;  divingResult = "Great dive! 8/10"; }
                else if (dist < 0.22f) { divingScore = 6;  divingResult = "Good dive! 6/10"; }
                else if (dist < 0.35f) { divingScore = 4;  divingResult = "Average dive. 4/10"; }
                else                   { divingScore = 1;  divingResult = "Belly flop! 1/10"; }

                int xp = divingScore * 8;
                player.AddAthleticsXP(xp);
                shopMessage = $"{divingResult} +{xp} Athletics XP";
                shopMessageTimer = 3f;
            }
        }
        else
        {
            divingFallTimer += dt;
            if (divingFallTimer > 2f)
            {
                divingJumped = false;
                divingBarPos = 0f;
                divingBarDir = 1f;
                // allow another dive or exit
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && !divingJumped)
        {
            swimmingActive = false;
            swimmingPoolType = "";
        }
    }
}

// ── TENNIS COURT ─────────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "TENNIS COURT")
{
    Vector2 netPos = new Vector2(680, 300);
    if (!tennisActive && Vector2.Distance(player.Position, netPos) < 150)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            tennisActive = true;
            tennisBallPos = new Vector2(300, 300);
            tennisBallVel = new Vector2(180, 100);
            tennisPlayerScore = 0;
            tennisAIScore = 0;
            tennisPlayerPaddleY = 270f;
            tennisAIPaddleY = 270f;
            tennisServing = true;
            tennisMessage = "Tennis started! W/S to move your paddle. Space to serve.";
            tennisMessageTimer = 3f;
        }
    }

    if (tennisActive)
    {
        if (tennisSwingCooldown > 0) tennisSwingCooldown -= dt;
        if (tennisMessageTimer > 0) tennisMessageTimer -= dt;

        // Player paddle movement (left side, x~100)
        if (Raylib.IsKeyDown(KeyboardKey.W)) tennisPlayerPaddleY -= 300f * dt;
        if (Raylib.IsKeyDown(KeyboardKey.S)) tennisPlayerPaddleY += 300f * dt;
        tennisPlayerPaddleY = Math.Clamp(tennisPlayerPaddleY, 90f, 500f);

        // AI tracks ball
        float aiTarget = tennisBallPos.Y - 30f;
        tennisAIPaddleY += Math.Sign(aiTarget - tennisAIPaddleY) * 200f * dt;
        tennisAIPaddleY = Math.Clamp(tennisAIPaddleY, 90f, 500f);

        if (!tennisServing)
        {
            // Move ball
            tennisBallPos += tennisBallVel * dt;

            // Top/bottom bounce
            if (tennisBallPos.Y < 90f)  { tennisBallPos.Y = 90f;  tennisBallVel.Y = MathF.Abs(tennisBallVel.Y); }
            if (tennisBallPos.Y > 540f) { tennisBallPos.Y = 540f; tennisBallVel.Y = -MathF.Abs(tennisBallVel.Y); }

            // Player paddle (x 80-110, y playerPaddleY to +60)
            Rectangle playerPaddle = new Rectangle(80, tennisPlayerPaddleY, 22, 60);
            Rectangle ballRect = new Rectangle(tennisBallPos.X - 8, tennisBallPos.Y - 8, 16, 16);
            if (Raylib.CheckCollisionRecs(ballRect, playerPaddle) && tennisBallVel.X < 0)
            {
                tennisBallVel.X = MathF.Abs(tennisBallVel.X) * 1.05f;
                tennisBallVel.Y += Raylib.GetRandomValue(-40, 40);
                player.AddAthleticsXP(2);
            }

            // AI paddle (x 1270-1292)
            Rectangle aiPaddle = new Rectangle(1270, tennisAIPaddleY, 22, 60);
            if (Raylib.CheckCollisionRecs(ballRect, aiPaddle) && tennisBallVel.X > 0)
            {
                tennisBallVel.X = -MathF.Abs(tennisBallVel.X) * 1.02f;
                tennisBallVel.Y += Raylib.GetRandomValue(-30, 30);
            }

            // Score: ball goes off left = AI scores, off right = player scores
            if (tennisBallPos.X < 60f)
            {
                tennisAIScore++;
                tennisMessage = tennisAIScore >= 5 ? "AI wins the set! Good game." : $"AI scores! {tennisPlayerScore} - {tennisAIScore}";
                tennisMessageTimer = 2f;
                if (tennisAIScore >= 5)
                {
                    tennisActive = false;
                    player.AddAthleticsXP(20);
                }
                else { tennisServing = true; tennisBallPos = new Vector2(300, 300); }
            }
            if (tennisBallPos.X > 1320f)
            {
                tennisPlayerScore++;
                player.AddAthleticsXP(10);
                tennisMessage = tennisPlayerScore >= 5 ? "You win the set! +50 Athletics XP" : $"You score! {tennisPlayerScore} - {tennisAIScore}";
                tennisMessageTimer = 2f;
                if (tennisPlayerScore >= 5)
                {
                    tennisActive = false;
                    player.AddAthleticsXP(50);
                }
                else { tennisServing = true; tennisBallPos = new Vector2(1000, 300); tennisBallVel.X = MathF.Abs(tennisBallVel.X); }
            }
        }
        else
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                tennisServing = false;
                tennisBallVel = new Vector2(
                    tennisBallPos.X < 680 ? 220 : -220,
                    Raylib.GetRandomValue(-120, 120)
                );
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && !tennisActive == false)
        {
            tennisActive = false;
            shopMessage = $"Tennis ended. Score: {tennisPlayerScore} - {tennisAIScore}";
            shopMessageTimer = 2f;
        }
    }
}

// ── BASKETBALL COURT ─────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "BASKETBALL COURT")
{
    Vector2 freeThrowLine = new Vector2(400, 500);
    bool nearFreeThrow = Vector2.Distance(player.Position, freeThrowLine) < 100;

    if (bbMessageTimer > 0) bbMessageTimer -= dt;

    if (!basketballActive)
    {
        if (nearFreeThrow && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            basketballActive = true;
            bbPower = 0f;
            bbPowerDir = 1f;
            bbAimX = 0f;
            bbAimDir = 1f;
            bbPowerLocked = false;
            bbShooting = false;
            bbShotTimer = 0f;
            bbAttempts++;
        }
    }
    else
    {
        if (!bbPowerLocked)
        {
            // Phase 1: lock power
            bbPower += bbPowerDir * 0.8f * dt;
            if (bbPower >= 1f) { bbPower = 1f; bbPowerDir = -1f; }
            if (bbPower <= 0f) { bbPower = 0f; bbPowerDir = 1f; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                bbPowerLocked = true;
        }
        else if (!bbShooting)
        {
            // Phase 2: lock aim
            bbAimX += bbAimDir * 1.2f * dt;
            if (bbAimX >= 1f) { bbAimX = 1f; bbAimDir = -1f; }
            if (bbAimX <= 0f) { bbAimX = 0f; bbAimDir = 1f; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                bbShooting = true;
                bbShotTimer = 0f;

                // Score: power near 0.65 = good arc, aim near 0.5 = centred
                float powerDiff = MathF.Abs(bbPower - 0.65f);
                float aimDiff   = MathF.Abs(bbAimX - 0.5f);
                bool scored = powerDiff < 0.15f && aimDiff < 0.2f;

                if (scored)
                {
                    bbScore++;
                    int xp = 15 + (int)(10 * (1f - powerDiff / 0.15f));
                    player.AddAthleticsXP(xp);
                    bbMessage = $"SWISH! Score: {bbScore} | +{xp} Athletics XP";
                }
                else if (powerDiff < 0.25f && aimDiff < 0.35f)
                {
                    bbMessage = "Off the rim! Close shot.";
                    player.AddAthleticsXP(3);
                }
                else
                {
                    bbMessage = "Missed! Try adjusting power and aim.";
                }
                bbMessageTimer = 2.5f;
            }
        }
        else
        {
            bbShotTimer += dt;
            if (bbShotTimer > 1.5f)
            {
                basketballActive = false;
                bbShooting = false;
                bbPowerLocked = false;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            basketballActive = false;
            bbMessage = $"Shooting session ended. {bbScore}/{bbAttempts} shots.";
            bbMessageTimer = 2.5f;
            bbAttempts = 0; bbScore = 0;
        }
    }
}

      if (currentBuilding.BuildingName == "BANK")
{
    // any of the 3 teller booths
    Vector2[] boothPositions = {
        new Vector2(250, 370),
        new Vector2(600, 370),
        new Vector2(950, 370)
    };

    bool nearAnyBooth = boothPositions.Any(bp =>
        Vector2.Distance(player.Position, bp) < 130);

    if (nearAnyBooth)
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

 if (currentBuilding.BuildingName == "SUPERMARKET")
{
    Vector2 trolleyPickupPos = new Vector2(80, 945);
    Vector2 basketPickupPos  = new Vector2(1000, 945);
    bool nearTrolleyPickup   = Vector2.Distance(player.Position, trolleyPickupPos) < 100;
    bool nearBasketPickup    = Vector2.Distance(player.Position, basketPickupPos)  < 100;

    if (Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        // --- PICKUP (only if not already holding something) ---
        if (!player.HasTrolley && !player.HasBasket)
        {
            if (nearTrolleyPickup)
            {
                player.HasTrolley = true;
                trolleyPickedUp = true;
                shopMessage = "Trolley grabbed! Speed reduced. Holds 20 items.";
                shopMessageTimer = 2f;
            }
            else if (nearBasketPickup)
            {
                player.HasBasket = true;
                basketPickedUp = true;
                shopMessage = "Basket grabbed! Holds 10 items.";
                shopMessageTimer = 2f;
            }
        }
        // --- RETURN (only if holding something AND near the return spots) ---
        else if (player.HasTrolley || player.HasBasket)
        {
            if (nearTrolleyPickup || nearBasketPickup)
            {
                if (player.HasTrolley)
                {
                    player.HasTrolley = false;
                    trolleyPickedUp = false;
                    shopMessage = "Trolley returned.";
                }
                else
                {
                    player.HasBasket = false;
                    basketPickedUp = false;
                    shopMessage = "Basket returned.";
                }
                shopMessageTimer = 1.5f;
                supermarketInventoryOpen = false;
            }
        }
    }

    // open/close inventory — unchanged
    if ((player.HasTrolley || player.HasBasket) && Raylib.IsKeyPressed(KeyboardKey.I))
        supermarketInventoryOpen = !supermarketInventoryOpen;
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

    for (int i = 0; i < pokiePositions.Length; i++)
    {
        if (i == 1) continue; // machine 2 (index 1) is the occupied one — handled below

        if (Vector2.Distance(player.Position, pokiePositions[i]) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                activePokieMachine.Open(i);
                activeMinigameType = MinigameType.Pokie;
                currentScene = SceneState.Minigame;
            }
        }
    }

    // occupied pokie machine (index 1)
    if (Vector2.Distance(player.Position, pokiePositions[1]) < 80)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            shopMessage = "Oi back off! That bloke is on a winning streak bro!";
            shopMessageTimer = 2f;
        }
    }
    
    Vector2[] poolTablePositions = {
    new Vector2(240, 330),
    new Vector2(540, 330)
};

for (int i = 0; i < poolTablePositions.Length; i++)
{
    if (Vector2.Distance(player.Position, poolTablePositions[i]) < 100)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (player.Money >= 2)
            {
                player.Money -= 2;
                activePoolGame.Open(i);
                activeMinigameType = MinigameType.Pool;
                currentScene = SceneState.Minigame;
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

// existing: if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120) { ... }
if (Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
{
    // existing other cases...
    if (currentBuilding.BuildingName == "BIKE DEALER" ||
        currentBuilding.BuildingName == "CAR DEALER" ||
        currentBuilding.BuildingName == "BARN DEALER")
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            dealerUIOpen = !dealerUIOpen;
            currentDealerBuilding = currentBuilding;
            dealerSelectedIndex = 0;
            dealerScrollOffset = 0;
            currentDealerType = currentBuilding.BuildingName.Contains("BIKE") ? DealerType.Bike
                              : currentBuilding.BuildingName.Contains("CAR")  ? DealerType.Car
                              : DealerType.Barn;
        }
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

 
    }

 if (Raylib.IsKeyPressed(KeyboardKey.Q) && !wardrobeOpen && !chestOpen && !shopUIOpen && !barMenuOpen)
{
    // Only allow exit when near the entry position (the entrance/door area)
    float distToEntrance = Vector2.Distance(player.Position, currentBuilding.EntryPosition);
    bool nearEntrance = distToEntrance < 120f;

    if (nearEntrance)
    {
        currentScene = SceneState.World;
        player.Position = currentBuilding.ExitPosition;
         lastZoneMusic = default; // force zone music to re-evaluate
        CheckZoneMusic();

        if (currentBuilding.BuildingName == "MY HOUSE")
        
        shopUIOpen = false;
        shopSelectedItem = -1;
        shopSelectedItemName = "";

        if (currentBuilding.BuildingName == "SUPERMARKET")
        {
            player.HasTrolley = false;
            player.HasBasket = false;
            trolleyPickedUp = false;
            basketPickedUp = false;
            supermarketInventoryOpen = false;
        }
    }
    else
    {
        // Show a hint so the player isn't confused
        shopMessage = "Return to the entrance to exit.";
        shopMessageTimer = 1.5f;
    }
}

    camera.Target = player.Position;

    break;
    case SceneState.Minigame:
                    UpdateMinigameScreen(dt);
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
                case SceneState.Minigame:
                DrawMinigameScreen();
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
    Raylib.ClearBackground(new Color(0, 191, 255, 255));
    Raylib.DrawText("OPEN WORLD RPG", 312, 152, 64, new Color((byte)255,(byte)200,(byte)0,(byte)80));
    Raylib.DrawText("OPEN WORLD RPG", 310, 150, 64, Color.Gold);

    // Ground / grass
Raylib.DrawRectangle(0, 400, 1600, 800, new Color(88, 214, 141, 255)); // full green strip

// Simple tree (trunk + leaves)
Raylib.DrawRectangle(130, 300, 30, 100, new Color(101, 67, 33, 255)); // trunk
Raylib.DrawCircle(145, 270, 48, new Color(34, 139, 34, 255)); // foliage
Raylib.DrawCircle(115, 300, 28, new Color(50, 205, 50, 255)); // extra leaves
Raylib.DrawCircle(175, 300, 28, new Color(50, 205, 50, 255));

// Simple house
Raylib.DrawRectangle(300, 300, 180, 120, new Color(210, 180, 140, 255)); // walls
Raylib.DrawTriangle(new Vector2(290, 300), new Vector2(490, 300), new Vector2(390, 240), new Color(165, 42, 42, 255)); // roof
Raylib.DrawRectangle(360, 350, 40, 70, new Color(80, 40, 20, 255)); // door
Raylib.DrawRectangle(330, 330, 30, 30, new Color(173, 216, 230, 255)); // window
Raylib.DrawRectangle(420, 330, 30, 30, new Color(173, 216, 230, 255)); // window

// Simple horse (blocky silhouette)
int hx = 620, hy = 360;
Raylib.DrawRectangle(hx, hy - 20, 60, 20, new Color(139, 69, 19, 255)); // body
Raylib.DrawRectangle(hx + 45, hy - 40, 10, 30, new Color(139, 69, 19, 255)); // neck
Raylib.DrawCircle(hx + 55, hy - 45, 10, new Color(139, 69, 19, 255)); // head
// legs
Raylib.DrawRectangle(hx + 5, hy, 8, 20, Color.DarkBrown);
Raylib.DrawRectangle(hx + 25, hy, 8, 20, Color.DarkBrown);
Raylib.DrawRectangle(hx + 40, hy, 8, 20, Color.DarkBrown);
Raylib.DrawRectangle(hx + 55, hy, 8, 20, Color.DarkBrown);

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
    float scale = 0.003f;

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

    //Raylib.PauseMusicStream(currentMusic);

    // dim background
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)150));

    // panel
    Raylib.DrawRectangle(ScreenWidth / 2 - 200, ScreenHeight / 2 - 250, 400, 600, new Color((byte)20, (byte)20, (byte)30, (byte)240));
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
                    Raylib.PlaySound(soundPauseClose); 
                    Raylib.ResumeMusicStream(currentMusic);
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
                    Raylib.ResumeMusicStream(currentMusic);
                    SwitchMusic(musicMainMenu);
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
                    player.DogFangs += 50;
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
Raylib.DrawRectangle(-40000, -40000, 80000, 39700, new Color(40, 100, 40, 255));

// forest bottom
Raylib.DrawRectangle(-40000, 1000, 80000, 39000, new Color(40, 100, 40, 255));

// desert (right, X 4000 to 26000, middle strip only)
Raylib.DrawRectangle(4000, -300, 22000, 1300, new Color(210, 180, 100, 255));

// snow (left, X -30000 to -3000, middle strip only)
Raylib.DrawRectangle(-30000, -300, 27000, 1300, new Color(220, 235, 255, 255));

// ocean beach sand (X 26000 to 30000)
Raylib.DrawRectangle(26000, -40000, 4000, 80000, new Color(240, 220, 150, 255));

// ocean water (X 30000+)
Raylib.DrawRectangle(30000, -40000, 10000, 80000, new Color(30, 100, 180, 255));

// swamp (far left, X -55000 to -30000)
Raylib.DrawRectangle(-55000, -40000, 25000, 80000, new Color(55, 75, 35, 255));

// volcano (far right top, X 26000+, Y -40000 to -2000)
Raylib.DrawRectangle(26000, -40000, 14000, 38000, new Color(40, 20, 10, 255));

// mountains (far left top, X -55000 to -26000, Y -40000 to -2000)
Raylib.DrawRectangle(-55000, -40000, 29000, 38000, new Color(100, 95, 90, 255));

// safe zone
Raylib.DrawRectangle(-3000, -1500, 7000, 4000, new Color(90, 170, 90, 255));

// Farm zone
Raylib.DrawRectangle(-3000, -10000, 3000, 4000, Color.Brown);

DrawSafeZoneTexture();
DrawBiomeTextures();

// forecourt road surface
Raylib.DrawRectangle(300, -1000, 700, 580, Color.DarkGray);

// =====================
// SIDEWALKS
// =====================

// main horizontal road
Raylib.DrawRectangle(-55000, 540, 95000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(-55000, 728, 95000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// north highway
Raylib.DrawRectangle(188, -55000, 15, 55000, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(318, -55000, 15, 55000, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// south highway
Raylib.DrawRectangle(188, 730, 15, 54270, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(318, 730, 15, 54270, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// desert side road
Raylib.DrawRectangle(4000, 188, 51000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(4000, 318, 51000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// snow side road
Raylib.DrawRectangle(-55000, 188, 52000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(-55000, 318, 52000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// volcano access road (horizontal, top right)
Raylib.DrawRectangle(26000, -10188, 14000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(26000, -10318, 14000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// mountain access road (horizontal, top left)
Raylib.DrawRectangle(-55000, -10188, 29000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));
Raylib.DrawRectangle(-55000, -10318, 29000, 15, new Color((byte)180,(byte)180,(byte)180,(byte)255));

// =====================
// ROADS
// =====================

// main horizontal road (extended to cover all biomes)
Raylib.DrawRectangle(-55000, 550, 95000, 180, Color.DarkGray);

// north highway (extended to mountains/volcano top)
Raylib.DrawRectangle(200, -55000, 120, 55000, Color.DarkGray);

// south highway (extended to map bottom)
Raylib.DrawRectangle(200, 730, 120, 54270, Color.DarkGray);

// desert/ocean side road (extended to ocean edge)
Raylib.DrawRectangle(4000, 200, 51000, 120, Color.DarkGray);

// snow/swamp side road (extended to swamp edge)
Raylib.DrawRectangle(-55000, 200, 52000, 120, Color.DarkGray);

// volcano access road (horizontal connector at Y -10200)
Raylib.DrawRectangle(26000, -10200, 14000, 120, Color.DarkGray);

// mountain access road (horizontal connector at Y -10200)
Raylib.DrawRectangle(-55000, -10200, 29000, 120, Color.DarkGray);

// north highway extension to volcano/mountain road (vertical connector)
Raylib.DrawRectangle(200, -55000, 120, 44800, Color.DarkGray);

// swamp vertical connector (links swamp side road to ring road)
Raylib.DrawRectangle(-40000, -10200, 120, 10920, Color.DarkGray);
Raylib.DrawRectangle(-50000, -10200, 120, 10920, Color.DarkGray);

// ocean/beach vertical connector
Raylib.DrawRectangle(28000, -10200, 120, 10920, Color.DarkGray);
Raylib.DrawRectangle(35000, -10200, 120, 10920, Color.DarkGray);

// vertical road to bank
Raylib.DrawRectangle(1020, -200, 120, 760, Color.DarkGray);

// vertical road to store
Raylib.DrawRectangle(-1260, -200, 120, 760, Color.DarkGray);

// vertical road to hospital
Raylib.DrawRectangle(200, -200, 120, 760, Color.DarkGray);

// vertical road to weapons shop
Raylib.DrawRectangle(500, -200, 120, 760, Color.DarkGray);

// =====================
// ROAD MARKINGS
// =====================

// main horizontal road markings (extended)
for (int i = -55000; i < 40000; i += 200)
    Raylib.DrawRectangle(i, 630, 100, 12, Color.Yellow);

// north highway markings
for (int i = -55000; i < 730; i += 200)
    Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);

// south highway markings
for (int i = 730; i < 55000; i += 200)
    Raylib.DrawRectangle(248, i, 12, 100, Color.Yellow);

// desert/ocean side road markings
for (int i = 4000; i < 55000; i += 200)
    Raylib.DrawRectangle(i, 248, 12, 100, Color.Yellow);

// snow/swamp side road markings
for (int i = -55000; i < -3000; i += 200)
    Raylib.DrawRectangle(i, 248, 12, 100, Color.Yellow);

// volcano access road markings
for (int i = 26000; i < 40000; i += 200)
    Raylib.DrawRectangle(i, -10140, 100, 12, Color.Yellow);

// mountain access road markings
for (int i = -55000; i < -26000; i += 200)
    Raylib.DrawRectangle(i, -10140, 100, 12, Color.Yellow);

// swamp vertical connector markings
for (int i = -10200; i < 730; i += 200)
{
    Raylib.DrawRectangle(-39920, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(-49920, i, 12, 100, Color.Yellow);
}

// ocean vertical connector markings
for (int i = -10200; i < 730; i += 200)
{
    Raylib.DrawRectangle(28080, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(35080, i, 12, 100, Color.Yellow);
}

// =====================
// OUTER RING ROAD
// =====================

// outer ring road - top
Raylib.DrawRectangle(-55000, -53000, 95000, 180, Color.DarkGray);

// outer ring road - bottom
Raylib.DrawRectangle(-55000, 53000, 95000, 180, Color.DarkGray);

// outer ring road - left
Raylib.DrawRectangle(-55000, -53000, 180, 106180, Color.DarkGray);

// outer ring road - right
Raylib.DrawRectangle(39820, -53000, 180, 106180, Color.DarkGray);

// snow/swamp vertical connectors
Raylib.DrawRectangle(-20000, -53000, 120, 106180, Color.DarkGray);
Raylib.DrawRectangle(-10000, -53000, 120, 106180, Color.DarkGray);
Raylib.DrawRectangle(-40000, -53000, 120, 106180, Color.DarkGray);
Raylib.DrawRectangle(-50000, -53000, 120, 106180, Color.DarkGray);

// desert/ocean vertical connectors
Raylib.DrawRectangle(15000, -53000, 120, 106180, Color.DarkGray);
Raylib.DrawRectangle(25000, -53000, 120, 106180, Color.DarkGray);
Raylib.DrawRectangle(35000, -53000, 120, 106180, Color.DarkGray);

// ring road markings top
for (int i = -55000; i < 40000; i += 200)
    Raylib.DrawRectangle(i, -52920, 100, 12, Color.Yellow);

// ring road markings bottom
for (int i = -55000; i < 40000; i += 200)
    Raylib.DrawRectangle(i, 53080, 100, 12, Color.Yellow);

// ring road markings left
for (int i = -53000; i < 53000; i += 200)
    Raylib.DrawRectangle(-54920, i, 12, 100, Color.Yellow);

// ring road markings right
for (int i = -53000; i < 53000; i += 200)
    Raylib.DrawRectangle(39900, i, 12, 100, Color.Yellow);

// snow/swamp connector markings
for (int i = -53000; i < 53000; i += 200)
{
    Raylib.DrawRectangle(-19920, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(-9920,  i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(-39920, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(-49920, i, 12, 100, Color.Yellow);
}

// desert/ocean connector markings
for (int i = -53000; i < 53000; i += 200)
{
    Raylib.DrawRectangle(15080, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(25080, i, 12, 100, Color.Yellow);
    Raylib.DrawRectangle(35080, i, 12, 100, Color.Yellow);
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

// ─── DRAW ALL BUILDING EXTERIORS ────────────────────────────────────────────
foreach (Building building in buildings)
{
    float bx = building.Bounds.X;
    float by = building.Bounds.Y;
    if (building.BuildingName == "McDONALD'S")
{

    // Main body
    Raylib.DrawRectangle((int)bx, (int)by, 200, 140, new Color((byte)210,(byte)30,(byte)30,(byte)255));
    // Roof band
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 14, new Color((byte)220,(byte)180,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 4, new Color((byte)255,(byte)210,(byte)0,(byte)255));
    // Golden arches
    Raylib.DrawCircleLines((int)bx + 65, (int)by + 20, 28, new Color((byte)255,(byte)210,(byte)0,(byte)255));
    Raylib.DrawCircleLines((int)bx + 65, (int)by + 20, 26, new Color((byte)255,(byte)210,(byte)0,(byte)255));
    Raylib.DrawCircleLines((int)bx + 105, (int)by + 20, 28, new Color((byte)255,(byte)210,(byte)0,(byte)255));
    Raylib.DrawCircleLines((int)bx + 105, (int)by + 20, 26, new Color((byte)255,(byte)210,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx + 40, (int)by + 20, 100, 14, new Color((byte)210,(byte)30,(byte)30,(byte)255)); // mask bottom of arches
    // Windows
    Raylib.DrawRectangle((int)bx + 10, (int)by + 20, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 20, 55, 45, new Color((byte)180,(byte)150,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx + 135, (int)by + 20, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 135, (int)by + 20, 55, 45, new Color((byte)180,(byte)150,(byte)0,(byte)255));
    // Door
    Raylib.DrawRectangle((int)bx + 80, (int)by + 80, 40, 60, new Color((byte)160,(byte)200,(byte)215,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 80, (int)by + 80, 40, 60, new Color((byte)180,(byte)150,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx + 99, (int)by + 80, 2, 60, new Color((byte)160,(byte)150,(byte)0,(byte)180));
    // Sign
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)200,(byte)160,(byte)0,(byte)220));
    Raylib.DrawText("McDONALD'S", (int)bx + 22, (int)by - 9, 10, Color.White);
}

if (building.BuildingName == "SWIMMING COMPLEX")
{
    
    Raylib.DrawRectangle((int)bx, (int)by, 300, 200, new Color((byte)20,(byte)100,(byte)180,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 310, 14, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 310, 4, new Color((byte)30,(byte)120,(byte)210,(byte)255));
    // Pool visible through "window"
    Raylib.DrawRectangle((int)bx + 10, (int)by + 15, 130, 130, new Color((byte)30,(byte)140,(byte)210,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 15, 130, 130, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Raylib.DrawRectangle((int)bx + 155, (int)by + 15, 130, 130, new Color((byte)30,(byte)140,(byte)210,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 155, (int)by + 15, 130, 130, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    // Lane lines
    for (int ln = 0; ln < 4; ln++)
        Raylib.DrawRectangle((int)bx + 10, (int)by + 15 + ln * 32, 130, 3, new Color((byte)255,(byte)200,(byte)0,(byte)180));
    // Diving board hint
    Raylib.DrawRectangle((int)bx + 160, (int)by + 20, 40, 6, new Color((byte)200,(byte)200,(byte)200,(byte)255));
    // Door
    Raylib.DrawRectangle((int)bx + 130, (int)by + 155, 40, 45, new Color((byte)160,(byte)200,(byte)215,(byte)200));
    // Sign
    Raylib.DrawRectangle((int)bx + 30, (int)by - 10, 240, 10, new Color((byte)10,(byte)80,(byte)160,(byte)220));
    Raylib.DrawText("SWIMMING COMPLEX", (int)bx + 32, (int)by - 9, 10, Color.White);
}

if (building.BuildingName == "DOMINO'S")
{
    // Main body — dark navy blue
    Raylib.DrawRectangle((int)bx, (int)by, 200, 140, new Color((byte)0,(byte)75,(byte)155,(byte)255));
    // Roof band — red
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 14, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 4,  new Color((byte)220,(byte)40,(byte)40,(byte)255));
    // Domino tile logo (two squares side by side with dots)
    Raylib.DrawRectangle((int)bx + 55, (int)by + 8,  38, 20, new Color((byte)20,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 95, (int)by + 8,  38, 20, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 92, (int)by + 8,  4,  20, new Color((byte)255,(byte)255,(byte)255,(byte)255)); // divider
    // dots on left tile
    Raylib.DrawCircle((int)bx + 65,  (int)by + 14, 2, Color.White);
    Raylib.DrawCircle((int)bx + 75,  (int)by + 14, 2, Color.White);
    Raylib.DrawCircle((int)bx + 65,  (int)by + 21, 2, Color.White);
    // dots on right tile
    Raylib.DrawCircle((int)bx + 106, (int)by + 12, 2, Color.White);
    Raylib.DrawCircle((int)bx + 120, (int)by + 18, 2, Color.White);
    Raylib.DrawCircle((int)bx + 128, (int)by + 12, 2, Color.White);
    // Windows
    Raylib.DrawRectangle((int)bx + 10,  (int)by + 36, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10,  (int)by + 36, 55, 45, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 135, (int)by + 36, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 135, (int)by + 36, 55, 45, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    // Door
    Raylib.DrawRectangle((int)bx + 80,  (int)by + 80, 40, 60, new Color((byte)160,(byte)200,(byte)215,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 80, (int)by + 80, 40, 60, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 99,  (int)by + 80, 2,  60, new Color((byte)140,(byte)160,(byte)180,(byte)180));
    // Sign
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)0,(byte)55,(byte)130,(byte)220));
    Raylib.DrawText("DOMINO'S", (int)bx + 30, (int)by - 9, 10, Color.White);
}
 
// ── KFC EXTERIOR ──────────────────────────────────────────────────────────
else if (building.BuildingName == "KFC")
{
    // Main body — red
    Raylib.DrawRectangle((int)bx, (int)by, 200, 140, new Color((byte)180,(byte)20,(byte)20,(byte)255));
    // Roof band — cream/white
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 14, new Color((byte)240,(byte)230,(byte)200,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 4,  new Color((byte)255,(byte)245,(byte)215,(byte)255));
    // Colonel silhouette (simple: oval head + bow tie)
    Raylib.DrawCircle((int)bx + 100, (int)by + 22, 14, new Color((byte)240,(byte)225,(byte)195,(byte)255)); // head
    Raylib.DrawRectangle((int)bx + 93, (int)by + 32, 14, 10, new Color((byte)255,(byte)255,(byte)255,(byte)255)); // collar
    // bow tie
    Raylib.DrawRectangle((int)bx + 93, (int)by + 34, 6,  6,  new Color((byte)180,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 101,(int)by + 34, 6,  6,  new Color((byte)180,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 98, (int)by + 36, 4,  3,  new Color((byte)140,(byte)10,(byte)10,(byte)255));
    // Windows
    Raylib.DrawRectangle((int)bx + 10,  (int)by + 20, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10,  (int)by + 20, 55, 45, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Raylib.DrawRectangle((int)bx + 135, (int)by + 20, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 135, (int)by + 20, 55, 45, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    // Door
    Raylib.DrawRectangle((int)bx + 80,  (int)by + 80, 40, 60, new Color((byte)160,(byte)200,(byte)215,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 80, (int)by + 80, 40, 60, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Raylib.DrawRectangle((int)bx + 99,  (int)by + 80, 2,  60, new Color((byte)180,(byte)170,(byte)150,(byte)180));
    // Sign
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)160,(byte)15,(byte)15,(byte)220));
    Raylib.DrawText("KFC", (int)bx + 72, (int)by - 9, 10, Color.White);
}
 
// ── BURGER KING EXTERIOR ──────────────────────────────────────────────────
else if (building.BuildingName == "BURGER KING")
{
    // Main body — orange/red
    Raylib.DrawRectangle((int)bx, (int)by, 200, 140, new Color((byte)210,(byte)80,(byte)0,(byte)255));
    // Roof band — red
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 14, new Color((byte)185,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 210, 4,  new Color((byte)210,(byte)40,(byte)40,(byte)255));
    // BK crown logo (simplified)
    int crownX = (int)bx + 72;
    int crownY = (int)by + 8;
    Raylib.DrawRectangle(crownX,      crownY + 8,  56, 14, new Color((byte)255,(byte)180,(byte)0,(byte)255)); // crown base
    Raylib.DrawRectangle(crownX,      crownY,       8, 10, new Color((byte)255,(byte)180,(byte)0,(byte)255)); // left point
    Raylib.DrawRectangle(crownX + 24, crownY - 4,   8, 14, new Color((byte)255,(byte)180,(byte)0,(byte)255)); // centre point
    Raylib.DrawRectangle(crownX + 48, crownY,       8, 10, new Color((byte)255,(byte)180,(byte)0,(byte)255)); // right point
    // Windows
    Raylib.DrawRectangle((int)bx + 10,  (int)by + 30, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10,  (int)by + 30, 55, 45, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx + 135, (int)by + 30, 55, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 135, (int)by + 30, 55, 45, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    // Door
    Raylib.DrawRectangle((int)bx + 80,  (int)by + 80, 40, 60, new Color((byte)160,(byte)200,(byte)215,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 80, (int)by + 80, 40, 60, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawRectangle((int)bx + 99,  (int)by + 80, 2,  60, new Color((byte)180,(byte)150,(byte)100,(byte)180));
    // Sign
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)185,(byte)15,(byte)15,(byte)220));
    Raylib.DrawText("BURGER KING", (int)bx + 22, (int)by - 9, 10, Color.White);
}

if (building.BuildingName == "TENNIS COURT")
{
    
    Raylib.DrawRectangle((int)bx, (int)by, 200, 120, new Color((byte)40,(byte)140,(byte)40,(byte)255));
    // Court markings
    Raylib.DrawRectangle((int)bx + 5, (int)by + 5, 190, 110, new Color((byte)50,(byte)160,(byte)50,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 10, 180, 100, Color.White);
    Raylib.DrawRectangle((int)bx + 98, (int)by + 10, 4, 100, Color.White); // net
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)30,(byte)100,(byte)30,(byte)220));
    Raylib.DrawText("TENNIS COURT", (int)bx + 22, (int)by - 9, 10, Color.White);
}

if (building.BuildingName == "BASKETBALL COURT")
{
    
    Raylib.DrawRectangle((int)bx, (int)by, 200, 120, new Color((byte)180,(byte)90,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 5, (int)by + 5, 190, 110, new Color((byte)210,(byte)120,(byte)30,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 10, 180, 100, Color.White);
    Raylib.DrawCircleLines((int)bx + 100, (int)by + 60, 25, Color.White);
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 10, new Color((byte)140,(byte)70,(byte)10,(byte)220));
    Raylib.DrawText("BASKETBALL", (int)bx + 22, (int)by - 9, 10, Color.White);
}
 
    // ── MARAE (unchanged) ────────────────────────────────────────────────────
    if (building.BuildingName == "MARAE")
    {
        // atea (forecourt) grass
        Raylib.DrawRectangle((int)bx - 60, (int)by + 130, 340, 280,
            new Color((byte)60, (byte)130, (byte)50, (byte)255));
        Raylib.DrawRectangleLines((int)bx - 60, (int)by + 130, 340, 280,
            new Color((byte)40, (byte)100, (byte)30, (byte)255));
 
        // pou lining atea
        int[] pouX = { (int)bx - 50, (int)bx + 260 };
        int[] pouY = { (int)by + 140, (int)by + 230, (int)by + 320 };
        foreach (int px in pouX)
            foreach (int py in pouY)
            {
                Raylib.DrawRectangle(px, py, 10, 60, new Color((byte)100,(byte)55,(byte)20,(byte)255));
                Raylib.DrawRectangle(px + 1, py + 5, 8, 6, new Color((byte)140,(byte)80,(byte)30,(byte)255));
                Raylib.DrawRectangle(px + 2, py + 14, 6, 4, new Color((byte)80,(byte)40,(byte)10,(byte)255));
            }
 
        // waharoa
        Raylib.DrawRectangle((int)bx + 50, (int)by + 390, 12, 50, new Color((byte)120,(byte)60,(byte)20,(byte)255));
        Raylib.DrawRectangle((int)bx + 148, (int)by + 390, 12, 50, new Color((byte)120,(byte)60,(byte)20,(byte)255));
        Raylib.DrawRectangle((int)bx + 50, (int)by + 390, 110, 12, new Color((byte)160,(byte)70,(byte)30,(byte)255));
        for (int i = (int)bx + 56; i < (int)bx + 160; i += 14)
            Raylib.DrawRectangle(i, (int)by + 393, 8, 6, new Color((byte)200,(byte)100,(byte)40,(byte)255));
 
        // wharenui body
        Raylib.DrawRectangle((int)bx, (int)by, 220, 130, new Color((byte)160,(byte)55,(byte)30,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(bx + 110, by - 60),
            new Vector2(bx - 10,  by + 10),
            new Vector2(bx + 230, by + 10),
            new Color((byte)100,(byte)40,(byte)20,(byte)255));
        Raylib.DrawRectangle((int)bx + 105, (int)by - 60, 10, 70, new Color((byte)80,(byte)30,(byte)10,(byte)255));
        Raylib.DrawLine((int)bx + 110, (int)by - 60, (int)bx - 10,  (int)by + 10, new Color((byte)200,(byte)100,(byte)40,(byte)255));
        Raylib.DrawLine((int)bx + 110, (int)by - 60, (int)bx + 230, (int)by + 10, new Color((byte)200,(byte)100,(byte)40,(byte)255));
        for (int i = (int)bx + 10; i < (int)bx + 210; i += 30)
        {
            Raylib.DrawCircle(i + 8, (int)by + 40, 7, new Color((byte)220,(byte)80,(byte)30,(byte)255));
            Raylib.DrawCircle(i + 8, (int)by + 40, 4, new Color((byte)240,(byte)160,(byte)60,(byte)255));
        }
        Raylib.DrawRectangle((int)bx + 85, (int)by + 65, 50, 65, new Color((byte)80,(byte)40,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx + 87, (int)by + 67, 46, 61, new Color((byte)100,(byte)55,(byte)20,(byte)255));
        Raylib.DrawRectangle((int)bx + 105, (int)by + 75, 10, 45, new Color((byte)140,(byte)75,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 98,  (int)by + 90, 24, 10, new Color((byte)140,(byte)75,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 20, (int)by + 30, 40, 30, new Color((byte)60,(byte)35,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx + 22, (int)by + 32, 36, 26, new Color((byte)80,(byte)50,(byte)20,(byte)180));
        Raylib.DrawRectangle((int)bx + 160, (int)by + 30, 40, 30, new Color((byte)60,(byte)35,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx + 162, (int)by + 32, 36, 26, new Color((byte)80,(byte)50,(byte)20,(byte)180));
        Raylib.DrawText("MARAE", (int)bx + 55, (int)by - 20, 22, new Color((byte)220,(byte)140,(byte)60,(byte)255));
    }
 
    // ── BANK (unchanged) ────────────────────────────────────────────────────
    if (building.BuildingName == "BANK")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 240, 160, new Color((byte)200,(byte)175,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx - 10, (int)by - 18, 260, 22, new Color((byte)160,(byte)130,(byte)60,(byte)255));
        Raylib.DrawRectangleLines((int)bx - 10, (int)by - 18, 260, 22, new Color((byte)120,(byte)95,(byte)40,(byte)255));
        Raylib.DrawRectangle((int)bx + 20, (int)by, 18, 160, new Color((byte)220,(byte)200,(byte)130,(byte)255));
        Raylib.DrawRectangle((int)bx + 202, (int)by, 18, 160, new Color((byte)220,(byte)200,(byte)130,(byte)255));
        Raylib.DrawRectangle((int)bx + 16,  (int)by - 8, 26, 10, new Color((byte)180,(byte)155,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 198, (int)by - 8, 26, 10, new Color((byte)180,(byte)155,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 45, (int)by + 20, 38, 50, new Color((byte)150,(byte)190,(byte)210,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 45, (int)by + 20, 38, 50, new Color((byte)120,(byte)100,(byte)50,(byte)255));
        Raylib.DrawRectangle((int)bx + 64, (int)by + 20, 2, 50, new Color((byte)120,(byte)100,(byte)50,(byte)180));
        Raylib.DrawRectangle((int)bx + 157, (int)by + 20, 38, 50, new Color((byte)150,(byte)190,(byte)210,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 157, (int)by + 20, 38, 50, new Color((byte)120,(byte)100,(byte)50,(byte)255));
        Raylib.DrawRectangle((int)bx + 176, (int)by + 20, 2, 50, new Color((byte)120,(byte)100,(byte)50,(byte)180));
        Raylib.DrawRectangle((int)bx + 90, (int)by + 80, 60, 80, new Color((byte)100,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 90, (int)by + 80, 28, 80, new Color((byte)120,(byte)100,(byte)40,(byte)255));
        Raylib.DrawRectangle((int)bx + 122,(int)by + 80, 28, 80, new Color((byte)120,(byte)100,(byte)40,(byte)255));
        Raylib.DrawCircle((int)bx + 116, (int)by + 120, 4, new Color((byte)200,(byte)160,(byte)40,(byte)255));
        Raylib.DrawCircle((int)bx + 124, (int)by + 120, 4, new Color((byte)200,(byte)160,(byte)40,(byte)255));
        Raylib.DrawRectangle((int)bx + 50, (int)by - 14, 140, 14, new Color((byte)140,(byte)110,(byte)50,(byte)255));
        Raylib.DrawText("WAIKATO BANK", (int)bx + 54, (int)by - 13, 12, new Color((byte)240,(byte)210,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 70, (int)by + 155, 100, 12, new Color((byte)180,(byte)160,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx + 80, (int)by + 163, 80, 10, new Color((byte)190,(byte)170,(byte)110,(byte)255));
    }
 
    // ── DBAR — BLACK BUILDING + SMOKING AREA ────────────────────────────────
    if (building.BuildingName == "DBar")
    {
        // main building body - black
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)15,(byte)15,(byte)15,(byte)255));
        // roof trim - dark red neon glow effect
        Raylib.DrawRectangle((int)bx - 4, (int)by - 6, 168, 8, new Color((byte)140,(byte)10,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx - 4, (int)by - 6, 168, 3, new Color((byte)220,(byte)20,(byte)20,(byte)255));
        // neon sign
        Raylib.DrawRectangle((int)bx + 20, (int)by - 22, 120, 18, new Color((byte)10,(byte)10,(byte)10,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 20, (int)by - 22, 120, 18, new Color((byte)180,(byte)10,(byte)10,(byte)255));
        Raylib.DrawText("DBar", (int)bx + 42, (int)by - 20, 16, new Color((byte)255,(byte)30,(byte)30,(byte)255));
        // windows — dark tinted
        Raylib.DrawRectangle((int)bx + 10, (int)by + 20, 40, 30, new Color((byte)20,(byte)20,(byte)30,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 10, (int)by + 20, 40, 30, new Color((byte)80,(byte)10,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx + 110, (int)by + 20, 40, 30, new Color((byte)20,(byte)20,(byte)30,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 110, (int)by + 20, 40, 30, new Color((byte)80,(byte)10,(byte)10,(byte)255));
        // door
        Raylib.DrawRectangle((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)30,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)100,(byte)10,(byte)10,(byte)255));
        // door handle
        Raylib.DrawCircle((int)bx + 96, (int)by + 96, 3, new Color((byte)160,(byte)160,(byte)160,(byte)255));
 
        
    }
 
    // ── GAS STATION BUILDING EXTERIOR ───────────────────────────────────────
    if (building.BuildingName == "GAS STATION")
    {
        // main shop building
        Raylib.DrawRectangle((int)bx, (int)by, 260, 160, new Color((byte)230,(byte)225,(byte)190,(byte)255));
        // roof band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 270, 14, new Color((byte)180,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 270, 4, new Color((byte)220,(byte)80,(byte)40,(byte)255));
        // large shop windows
        Raylib.DrawRectangle((int)bx + 15, (int)by + 15, 100, 80, new Color((byte)160,(byte)200,(byte)220,(byte)220));
        Raylib.DrawRectangleLines((int)bx + 15, (int)by + 15, 100, 80, new Color((byte)160,(byte)140,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx + 65, (int)by + 15, 2, 80, new Color((byte)140,(byte)120,(byte)80,(byte)180)); // window divide
        Raylib.DrawRectangle((int)bx + 145, (int)by + 15, 80, 80, new Color((byte)160,(byte)200,(byte)220,(byte)220));
        Raylib.DrawRectangleLines((int)bx + 145, (int)by + 15, 80, 80, new Color((byte)160,(byte)140,(byte)100,(byte)255));
        // door
        Raylib.DrawRectangle((int)bx + 110, (int)by + 90, 40, 70, new Color((byte)180,(byte)200,(byte)215,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 110, (int)by + 90, 40, 70, new Color((byte)160,(byte)140,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx + 128, (int)by + 115, 6, 18, new Color((byte)160,(byte)160,(byte)160,(byte)255)); // handle
        // sign
        Raylib.DrawRectangle((int)bx + 40, (int)by - 8, 180, 10, new Color((byte)180,(byte)60,(byte)30,(byte)200));
        Raylib.DrawText("GAS STATION", (int)bx + 44, (int)by - 7, 10, new Color((byte)255,(byte)240,(byte)180,(byte)255));
        // steps
        Raylib.DrawRectangle((int)bx + 90, (int)by + 155, 80, 10, new Color((byte)200,(byte)195,(byte)160,(byte)255));
    }

    // ── BIKE DEALER EXTERIOR ─────────────────────────────────────────────────
if (building.BuildingName == "BIKE DEALER")
{
    // main facade — steel blue
    Raylib.DrawRectangle((int)bx, (int)by, 320, 180, new Color((byte)40, (byte)80, (byte)140, (byte)255));
 
    // roof overhang with riveted strip
    Raylib.DrawRectangle((int)bx - 6,  (int)by - 14, 332, 16, new Color((byte)25, (byte)50, (byte)100, (byte)255));
    Raylib.DrawRectangle((int)bx - 6,  (int)by - 14, 332, 4,  new Color((byte)60, (byte)110, (byte)180, (byte)255));
    // rivet dots along roof band
    for (int rv = 0; rv < 10; rv++)
        Raylib.DrawCircle((int)bx + 16 + rv * 30, (int)by - 8, 3, new Color((byte)180, (byte)200, (byte)230, (byte)200));
 
    // large showroom window (left)
    Raylib.DrawRectangle((int)bx + 10,  (int)by + 20, 120, 90, new Color((byte)160, (byte)200, (byte)230, (byte)160));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 20, 120, 90, new Color((byte)20, (byte)50, (byte)100, (byte)255));
    // window cross frame
    Raylib.DrawRectangle((int)bx + 68,  (int)by + 20, 4,   90, new Color((byte)20, (byte)50, (byte)100, (byte)200));
    Raylib.DrawRectangle((int)bx + 10,  (int)by + 64, 120, 4,  new Color((byte)20, (byte)50, (byte)100, (byte)200));
 
    // large showroom window (right)
    Raylib.DrawRectangle((int)bx + 190, (int)by + 20, 120, 90, new Color((byte)160, (byte)200, (byte)230, (byte)160));
    Raylib.DrawRectangleLines((int)bx + 190, (int)by + 20, 120, 90, new Color((byte)20, (byte)50, (byte)100, (byte)255));
    Raylib.DrawRectangle((int)bx + 248, (int)by + 20, 4,   90, new Color((byte)20, (byte)50, (byte)100, (byte)200));
    Raylib.DrawRectangle((int)bx + 190, (int)by + 64, 120, 4,  new Color((byte)20, (byte)50, (byte)100, (byte)200));
 
    // metal door (centre)
    Raylib.DrawRectangle((int)bx + 138, (int)by + 110, 44, 70, new Color((byte)50, (byte)70, (byte)110, (byte)255));
    Raylib.DrawRectangle((int)bx + 140, (int)by + 112, 40, 30, new Color((byte)70, (byte)100, (byte)150, (byte)255)); // upper panel
    Raylib.DrawRectangle((int)bx + 140, (int)by + 146, 40, 30, new Color((byte)70, (byte)100, (byte)150, (byte)255)); // lower panel
    Raylib.DrawCircle((int)bx + 176,    (int)by + 158, 3,  new Color((byte)210, (byte)210, (byte)210, (byte)255));    // handle
 
    // sign
    Raylib.DrawRectangle((int)bx + 50,  (int)by - 10, 220, 12, new Color((byte)20, (byte)45, (byte)90, (byte)230));
    Raylib.DrawText("BIKE DEALER", (int)bx + 62, (int)by - 9, 10, new Color((byte)160, (byte)210, (byte)255, (byte)255));
 
    // concrete step
    Raylib.DrawRectangle((int)bx + 120, (int)by + 178, 80, 8, new Color((byte)120, (byte)130, (byte)145, (byte)255));
    Raylib.DrawRectangle((int)bx + 128, (int)by + 186, 64, 5, new Color((byte)140, (byte)150, (byte)165, (byte)255));
}
 
// ── CAR DEALER EXTERIOR ───────────────────────────────────────────────────
else if (building.BuildingName == "CAR DEALER")
{
    // main facade — near-black with slight blue tint
    Raylib.DrawRectangle((int)bx, (int)by, 420, 240, new Color((byte)22, (byte)22, (byte)28, (byte)255));
 
    // glass curtain wall (full width, top two-thirds)
    Raylib.DrawRectangle((int)bx + 4,   (int)by + 4,   412, 155, new Color((byte)140, (byte)170, (byte)200, (byte)100));
    // vertical mullions
    for (int ml = 0; ml < 5; ml++)
        Raylib.DrawRectangle((int)bx + 4 + ml * 82, (int)by + 4, 4, 155, new Color((byte)20, (byte)20, (byte)25, (byte)255));
    // horizontal transom
    Raylib.DrawRectangle((int)bx + 4,   (int)by + 82,  412, 4,  new Color((byte)20, (byte)20, (byte)25, (byte)255));
 
    // roof cap / fascia
    Raylib.DrawRectangle((int)bx - 8,   (int)by - 16,  436, 20, new Color((byte)15, (byte)15, (byte)18, (byte)255));
    Raylib.DrawRectangle((int)bx - 8,   (int)by - 16,  436, 5,  new Color((byte)80, (byte)130, (byte)200, (byte)255)); // blue accent line
 
    // lower solid panel
    Raylib.DrawRectangle((int)bx,       (int)by + 163, 420, 77, new Color((byte)18, (byte)18, (byte)22, (byte)255));
 
    // double glass entrance doors
    Raylib.DrawRectangle((int)bx + 168, (int)by + 163, 84,  77, new Color((byte)120, (byte)160, (byte)200, (byte)130));
    Raylib.DrawRectangle((int)bx + 168, (int)by + 163, 42,  77, new Color((byte)100, (byte)140, (byte)185, (byte)120)); // left door
    Raylib.DrawRectangle((int)bx + 210, (int)by + 163, 42,  77, new Color((byte)100, (byte)140, (byte)185, (byte)120)); // right door
    Raylib.DrawRectangle((int)bx + 208, (int)by + 163, 4,   77, new Color((byte)15, (byte)15, (byte)18,  (byte)255));  // door gap
    Raylib.DrawRectangleLines((int)bx + 168, (int)by + 163, 84, 77, new Color((byte)60, (byte)100, (byte)160, (byte)255));
    // door handles
    Raylib.DrawRectangle((int)bx + 200, (int)by + 198, 8,   3,  new Color((byte)200, (byte)200, (byte)210, (byte)255));
    Raylib.DrawRectangle((int)bx + 212, (int)by + 198, 8,   3,  new Color((byte)200, (byte)200, (byte)210, (byte)255));
 
    // logo / sign on fascia
    Raylib.DrawRectangle((int)bx + 100, (int)by - 14, 220, 12, new Color((byte)10, (byte)10, (byte)14, (byte)230));
    Raylib.DrawText("CAR DEALER", (int)bx + 126, (int)by - 13, 10, new Color((byte)100, (byte)170, (byte)255, (byte)255));
 
    // side pillar accents
    Raylib.DrawRectangle((int)bx,       (int)by,        8,  240, new Color((byte)15, (byte)15, (byte)18, (byte)255));
    Raylib.DrawRectangle((int)bx + 412, (int)by,        8,  240, new Color((byte)15, (byte)15, (byte)18, (byte)255));
 
    // concrete apron / step
    Raylib.DrawRectangle((int)bx + 148, (int)by + 238, 124, 10, new Color((byte)80, (byte)85, (byte)95, (byte)255));
    Raylib.DrawRectangle((int)bx + 156, (int)by + 248, 108, 7,  new Color((byte)95, (byte)100, (byte)110, (byte)255));
}
 
// ── BARN DEALER EXTERIOR ──────────────────────────────────────────────────
else if (building.BuildingName == "BARN DEALER")
{
    // main barn body — weathered red-brown
    Raylib.DrawRectangle((int)bx, (int)by, 360, 220, new Color((byte)140, (byte)55, (byte)28, (byte)255));
 
    // vertical wood plank lines
    for (int pl = 0; pl < 360; pl += 30)
        Raylib.DrawRectangle((int)bx + pl, (int)by, 3, 220, new Color((byte)110, (byte)42, (byte)18, (byte)180));
 
    // barn roof (pitched triangle top)
    for (int ri = 0; ri < 30; ri++)
    {
        int rw = 370 - ri * 12;
        int rx = (int)bx - 5 + ri * 6;
        Raylib.DrawRectangle(rx, (int)by - 12 - ri * 4, rw, 5, new Color((byte)80, (byte)35, (byte)15, (byte)255));
    }
    // roof ridge cap
    Raylib.DrawRectangle((int)bx + 155, (int)by - 130, 50, 130, new Color((byte)65, (byte)28, (byte)10, (byte)255));
    Raylib.DrawRectangle((int)bx + 150, (int)by - 132, 60, 8,   new Color((byte)50, (byte)20, (byte)8,  (byte)255));
 
    // loft hatch (top centre)
    Raylib.DrawRectangle((int)bx + 148, (int)by + 10, 64, 48, new Color((byte)55, (byte)30, (byte)12, (byte)255));
    Raylib.DrawRectangleLines((int)bx + 148, (int)by + 10, 64, 48, new Color((byte)90, (byte)50, (byte)22, (byte)255));
    Raylib.DrawRectangle((int)bx + 178, (int)by + 10, 4,  48, new Color((byte)90, (byte)50, (byte)22, (byte)200)); // hatch split
 
    // left window with X brace
    Raylib.DrawRectangle((int)bx + 18, (int)by + 30, 50, 45, new Color((byte)190, (byte)210, (byte)170, (byte)150));
    Raylib.DrawRectangleLines((int)bx + 18, (int)by + 30, 50, 45, new Color((byte)80, (byte)38, (byte)15, (byte)255));
    Raylib.DrawLine((int)bx + 18, (int)by + 30, (int)bx + 68, (int)by + 75, new Color((byte)80, (byte)38, (byte)15, (byte)200));
    Raylib.DrawLine((int)bx + 68, (int)by + 30, (int)bx + 18, (int)by + 75, new Color((byte)80, (byte)38, (byte)15, (byte)200));
 
    // right window with X brace
    Raylib.DrawRectangle((int)bx + 292, (int)by + 30, 50, 45, new Color((byte)190, (byte)210, (byte)170, (byte)150));
    Raylib.DrawRectangleLines((int)bx + 292, (int)by + 30, 50, 45, new Color((byte)80, (byte)38, (byte)15, (byte)255));
    Raylib.DrawLine((int)bx + 292, (int)by + 30, (int)bx + 342, (int)by + 75, new Color((byte)80, (byte)38, (byte)15, (byte)200));
    Raylib.DrawLine((int)bx + 342, (int)by + 30, (int)bx + 292, (int)by + 75, new Color((byte)80, (byte)38, (byte)15, (byte)200));
 
    // large barn doors (double, lower centre)
    Raylib.DrawRectangle((int)bx + 118, (int)by + 130, 60,  90, new Color((byte)90,  (byte)48, (byte)20, (byte)255)); // left door
    Raylib.DrawRectangle((int)bx + 182, (int)by + 130, 60,  90, new Color((byte)100, (byte)55, (byte)22, (byte)255)); // right door
    Raylib.DrawRectangle((int)bx + 176, (int)by + 130, 8,   90, new Color((byte)70,  (byte)35, (byte)12, (byte)255)); // gap
    Raylib.DrawRectangleLines((int)bx + 118, (int)by + 130, 124, 90, new Color((byte)70, (byte)35, (byte)12, (byte)255));
    // door cross braces
    Raylib.DrawLine((int)bx + 118, (int)by + 130, (int)bx + 176, (int)by + 220, new Color((byte)70, (byte)35, (byte)12, (byte)200));
    Raylib.DrawLine((int)bx + 176, (int)by + 130, (int)bx + 118, (int)by + 220, new Color((byte)70, (byte)35, (byte)12, (byte)200));
    Raylib.DrawLine((int)bx + 184, (int)by + 130, (int)bx + 242, (int)by + 220, new Color((byte)70, (byte)35, (byte)12, (byte)200));
    Raylib.DrawLine((int)bx + 242, (int)by + 130, (int)bx + 184, (int)by + 220, new Color((byte)70, (byte)35, (byte)12, (byte)200));
    // door handles
    Raylib.DrawCircle((int)bx + 170, (int)by + 175, 4, new Color((byte)180, (byte)140, (byte)60, (byte)255));
    Raylib.DrawCircle((int)bx + 190, (int)by + 175, 4, new Color((byte)180, (byte)140, (byte)60, (byte)255));
 
    // sign on plank above doors
    Raylib.DrawRectangle((int)bx + 80, (int)by + 118, 200, 14, new Color((byte)65, (byte)30, (byte)10, (byte)230));
    Raylib.DrawText("BARN DEALER", (int)bx + 88, (int)by + 120, 10, new Color((byte)220, (byte)185, (byte)120, (byte)255));
 
    // dirt/hay ground step
    Raylib.DrawRectangle((int)bx + 100, (int)by + 218, 160, 10, new Color((byte)110, (byte)85, (byte)45, (byte)255));
    Raylib.DrawRectangle((int)bx + 110, (int)by + 228, 140, 7,  new Color((byte)130, (byte)100, (byte)55, (byte)255));
}
 
    // ── WEAPONS STORE EXTERIOR ───────────────────────────────────────────────
    if (building.BuildingName == "WEAPONS")
    {
        // base body
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)60,(byte)60,(byte)65,(byte)255));
        // stone-like wall texture lines
        for (int wx = (int)bx; wx < (int)bx + 160; wx += 30)
            Raylib.DrawRectangle(wx, (int)by, 2, 120, new Color((byte)40,(byte)40,(byte)45,(byte)150));
        for (int wy = (int)by; wy < (int)by + 120; wy += 20)
            Raylib.DrawRectangle((int)bx, wy, 160, 1, new Color((byte)40,(byte)40,(byte)45,(byte)150));
        // roof battlements / parapet
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 14, new Color((byte)50,(byte)50,(byte)55,(byte)255));
        for (int wx = (int)bx; wx < (int)bx + 165; wx += 22)
            Raylib.DrawRectangle(wx, (int)by - 12, 12, 14, new Color((byte)70,(byte)70,(byte)75,(byte)255));
        // shield/skull emblem above door
        Raylib.DrawRectangle((int)bx + 55, (int)by - 8, 50, 10, new Color((byte)80,(byte)30,(byte)30,(byte)255));
        Raylib.DrawText("WEAPONS", (int)bx + 18, (int)by - 7, 11, new Color((byte)180,(byte)50,(byte)50,(byte)255));
        // barred windows
        Raylib.DrawRectangle((int)bx + 10, (int)by + 20, 35, 30, new Color((byte)20,(byte)20,(byte)25,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 10, (int)by + 20, 35, 30, new Color((byte)80,(byte)80,(byte)85,(byte)255));
        for (int wx = (int)bx + 18; wx < (int)bx + 45; wx += 8)
            Raylib.DrawRectangle(wx, (int)by + 20, 2, 30, new Color((byte)60,(byte)60,(byte)65,(byte)255)); // bars
        Raylib.DrawRectangle((int)bx + 115, (int)by + 20, 35, 30, new Color((byte)20,(byte)20,(byte)25,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 115, (int)by + 20, 35, 30, new Color((byte)80,(byte)80,(byte)85,(byte)255));
        for (int wx = (int)bx + 123; wx < (int)bx + 150; wx += 8)
            Raylib.DrawRectangle(wx, (int)by + 20, 2, 30, new Color((byte)60,(byte)60,(byte)65,(byte)255));
        // heavy door
        Raylib.DrawRectangle((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)40,(byte)40,(byte)45,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)80,(byte)80,(byte)85,(byte)255));
        // door rivets
        Raylib.DrawCircle((int)bx + 65, (int)by + 76, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        Raylib.DrawCircle((int)bx + 95, (int)by + 76, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        Raylib.DrawCircle((int)bx + 65, (int)by + 108, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        Raylib.DrawCircle((int)bx + 95, (int)by + 108, 3, new Color((byte)100,(byte)100,(byte)110,(byte)255));
        // door handle
        Raylib.DrawRectangle((int)bx + 92, (int)by + 88, 4, 14, new Color((byte)140,(byte)140,(byte)150,(byte)255));
        // crossed swords emblem on door
        Raylib.DrawLine((int)bx + 68, (int)by + 82, (int)bx + 90, (int)by + 105, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawLine((int)bx + 90, (int)by + 82, (int)bx + 68, (int)by + 105, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
 
    // ── HOSPITAL EXTERIOR ────────────────────────────────────────────────────
    if (building.BuildingName == "HOSPITAL")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)240,(byte)240,(byte)248,(byte)255));
        // red band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 12, new Color((byte)200,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 4, new Color((byte)230,(byte)50,(byte)50,(byte)255));
        // large windows each side
        Raylib.DrawRectangle((int)bx + 8, (int)by + 15, 40, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 15, 40, 45, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawRectangle((int)bx + 112, (int)by + 15, 40, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 112, (int)by + 15, 40, 45, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        // red cross on wall
        Raylib.DrawRectangle((int)bx + 66, (int)by + 20, 12, 35, new Color((byte)200,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 57, (int)by + 29, 30, 12, new Color((byte)200,(byte)30,(byte)30,(byte)255));
        // door
        Raylib.DrawRectangle((int)bx + 55, (int)by + 75, 50, 45, new Color((byte)180,(byte)200,(byte)210,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 55, (int)by + 75, 50, 45, new Color((byte)160,(byte)170,(byte)180,(byte)255));
        // sign
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 10, new Color((byte)200,(byte)30,(byte)30,(byte)200));
        Raylib.DrawText("HOSPITAL", (int)bx + 26, (int)by - 7, 10, Color.White);
        // ambulance bay lines on forecourt
        Raylib.DrawRectangle((int)bx - 5, (int)by + 118, 170, 4, new Color((byte)220,(byte)50,(byte)50,(byte)255));
    }
 
    // ── SUPERMARKET EXTERIOR ─────────────────────────────────────────────────
    if (building.BuildingName == "SUPERMARKET")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)200,(byte)220,(byte)200,(byte)255));
        // roof band - green
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 12, new Color((byte)60,(byte)140,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 4, new Color((byte)80,(byte)170,(byte)80,(byte)255));
        // wide glass front
        Raylib.DrawRectangle((int)bx + 5, (int)by + 10, 150, 70, new Color((byte)140,(byte)190,(byte)160,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 5, (int)by + 10, 150, 70, new Color((byte)80,(byte)140,(byte)80,(byte)255));
        // window frame dividers
        Raylib.DrawRectangle((int)bx + 55, (int)by + 10, 4, 70, new Color((byte)80,(byte)140,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 105, (int)by + 10, 4, 70, new Color((byte)80,(byte)140,(byte)80,(byte)255));
        // automatic door indicators
        Raylib.DrawRectangle((int)bx + 57, (int)by + 82, 46, 38, new Color((byte)160,(byte)200,(byte)175,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 57, (int)by + 82, 46, 38, new Color((byte)60,(byte)120,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx + 79, (int)by + 82, 2, 38, new Color((byte)60,(byte)120,(byte)60,(byte)180)); // door gap
        // sign
        Raylib.DrawRectangle((int)bx + 10, (int)by - 8, 140, 10, new Color((byte)50,(byte)120,(byte)50,(byte)220));
        Raylib.DrawText("SUPERMARKET", (int)bx + 12, (int)by - 7, 10, Color.White);
        // trolley bay outside
        Raylib.DrawRectangle((int)bx - 50, (int)by + 50, 45, 70, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawRectangleLines((int)bx - 50, (int)by + 50, 45, 70, new Color((byte)120,(byte)120,(byte)130,(byte)255));
        Raylib.DrawText("Bay", (int)bx - 44, (int)by + 80, 11, Color.DarkGray);
    }
 
    // ── GYM EXTERIOR ─────────────────────────────────────────────────────────
    if (building.BuildingName == "GYM")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)40,(byte)80,(byte)160,(byte)255));
        // bold blue roof band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 12, new Color((byte)20,(byte)40,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 4, new Color((byte)30,(byte)60,(byte)140,(byte)255));
        // large tinted windows
        Raylib.DrawRectangle((int)bx + 8, (int)by + 10, 55, 55, new Color((byte)60,(byte)100,(byte)180,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 10, 55, 55, new Color((byte)20,(byte)40,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx + 98, (int)by + 10, 55, 55, new Color((byte)60,(byte)100,(byte)180,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 98, (int)by + 10, 55, 55, new Color((byte)20,(byte)40,(byte)100,(byte)255));
        // dumbbell icon on wall
        Raylib.DrawRectangle((int)bx + 60, (int)by + 22, 40, 8, new Color((byte)60,(byte)100,(byte)180,(byte)255)); // bar
        Raylib.DrawRectangle((int)bx + 56, (int)by + 18, 8, 16, new Color((byte)60,(byte)100,(byte)180,(byte)255)); // left plate
        Raylib.DrawRectangle((int)bx + 96, (int)by + 18, 8, 16, new Color((byte)60,(byte)100,(byte)180,(byte)255)); // right plate
        // door
        Raylib.DrawRectangle((int)bx + 60, (int)by + 75, 40, 45, new Color((byte)20,(byte)40,(byte)100,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by + 75, 40, 45, new Color((byte)40,(byte)70,(byte)140,(byte)255));
        // sign
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 10, new Color((byte)20,(byte)40,(byte)100,(byte)220));
        Raylib.DrawText("GYM", (int)bx + 52, (int)by - 7, 10, new Color((byte)180,(byte)210,(byte)255,(byte)255));
    }
 
    // ── POLICE STATION EXTERIOR ───────────────────────────────────────────────
    if (building.BuildingName == "POLICE STATION")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)30,(byte)30,(byte)100,(byte)255));
        // dark blue band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 14, new Color((byte)15,(byte)15,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 4, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        // badge/star sign
        Raylib.DrawRectangle((int)bx + 55, (int)by - 8, 50, 10, new Color((byte)15,(byte)15,(byte)60,(byte)255));
        Raylib.DrawText("POLICE", (int)bx + 22, (int)by - 7, 11, new Color((byte)200,(byte)200,(byte)255,(byte)255));
        // windows
        Raylib.DrawRectangle((int)bx + 8, (int)by + 15, 40, 35, new Color((byte)60,(byte)80,(byte)160,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 15, 40, 35, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 112, (int)by + 15, 40, 35, new Color((byte)60,(byte)80,(byte)160,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 112, (int)by + 15, 40, 35, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        // blue-and-white checkered strip (police pattern)
        for (int cx = (int)bx; cx < (int)bx + 160; cx += 10)
            Raylib.DrawRectangle(cx, (int)by + 52,
                10, 10,
                (cx / 10 % 2 == 0)
                    ? new Color((byte)20,(byte)20,(byte)80,(byte)255)
                    : new Color((byte)240,(byte)240,(byte)255,(byte)255));
        // door
        Raylib.DrawRectangle((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by + 70, 40, 50, new Color((byte)40,(byte)40,(byte)120,(byte)255));
        // gold handle
        Raylib.DrawRectangle((int)bx + 92, (int)by + 88, 4, 14, new Color((byte)200,(byte)170,(byte)40,(byte)255));
    }
 
    // ── STORE EXTERIOR ───────────────────────────────────────────────────────
    if (building.BuildingName == "STORE")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)45,(byte)100,(byte)55,(byte)255));
        // wooden-style dark green roof band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 12, new Color((byte)30,(byte)65,(byte)35,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 170, 4, new Color((byte)40,(byte)80,(byte)45,(byte)255));
        // rustic windows with shutters
        Raylib.DrawRectangle((int)bx + 8, (int)by + 15, 38, 40, new Color((byte)160,(byte)195,(byte)175,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 15, 38, 40, new Color((byte)25,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 3, (int)by + 15, 6, 40, new Color((byte)60,(byte)110,(byte)70,(byte)255)); // left shutter
        Raylib.DrawRectangle((int)bx + 45, (int)by + 15, 6, 40, new Color((byte)60,(byte)110,(byte)70,(byte)255)); // right shutter
        Raylib.DrawRectangle((int)bx + 114, (int)by + 15, 38, 40, new Color((byte)160,(byte)195,(byte)175,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 114, (int)by + 15, 38, 40, new Color((byte)25,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 109, (int)by + 15, 6, 40, new Color((byte)60,(byte)110,(byte)70,(byte)255));
        Raylib.DrawRectangle((int)bx + 151, (int)by + 15, 6, 40, new Color((byte)60,(byte)110,(byte)70,(byte)255));
        // wooden door with panel detail
        Raylib.DrawRectangle((int)bx + 58, (int)by + 72, 44, 48, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        Raylib.DrawRectangle((int)bx + 62, (int)by + 76, 18, 20, new Color((byte)120,(byte)80,(byte)35,(byte)255)); // upper panel
        Raylib.DrawRectangle((int)bx + 82, (int)by + 76, 16, 20, new Color((byte)120,(byte)80,(byte)35,(byte)255));
        Raylib.DrawRectangle((int)bx + 62, (int)by + 100, 36, 16, new Color((byte)120,(byte)80,(byte)35,(byte)255)); // lower panel
        Raylib.DrawCircle((int)bx + 97, (int)by + 97, 3, new Color((byte)200,(byte)160,(byte)60,(byte)255)); // handle
        // sign with barrel icon
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 10, new Color((byte)25,(byte)60,(byte)30,(byte)220));
        Raylib.DrawText("STORE", (int)bx + 46, (int)by - 7, 10, new Color((byte)180,(byte)220,(byte)180,(byte)255));
        // wood plank steps
        Raylib.DrawRectangle((int)bx + 48, (int)by + 118, 64, 8, new Color((byte)120,(byte)80,(byte)40,(byte)255));
        Raylib.DrawRectangle((int)bx + 54, (int)by + 126, 52, 6, new Color((byte)140,(byte)95,(byte)50,(byte)255));
    }
 
    // ── MY HOUSE EXTERIOR ─────────────────────────────────────────────────────
    if (building.BuildingName == "MY HOUSE")
    {
        // base walls - warm sandstone
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)210,(byte)175,(byte)120,(byte)255));
        // roof - warm terracotta
        Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 176, 16, new Color((byte)180,(byte)90,(byte)50,(byte)255));
        Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 176, 5, new Color((byte)200,(byte)110,(byte)60,(byte)255));
        // left window with flower box
        Raylib.DrawRectangle((int)bx + 8, (int)by + 15, 38, 35, new Color((byte)160,(byte)200,(byte)210,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 15, 38, 35, new Color((byte)140,(byte)100,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx + 8, (int)by + 48, 38, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255)); // flower box
        Raylib.DrawCircle((int)bx + 16, (int)by + 50, 4, new Color((byte)220,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle((int)bx + 27, (int)by + 51, 4, new Color((byte)255,(byte)200,(byte)50,(byte)255));
        Raylib.DrawCircle((int)bx + 38, (int)by + 50, 4, new Color((byte)220,(byte)80,(byte)80,(byte)255));
        // right window with flower box
        Raylib.DrawRectangle((int)bx + 114, (int)by + 15, 38, 35, new Color((byte)160,(byte)200,(byte)210,(byte)200));
        Raylib.DrawRectangleLines((int)bx + 114, (int)by + 15, 38, 35, new Color((byte)140,(byte)100,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx + 114, (int)by + 48, 38, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
        Raylib.DrawCircle((int)bx + 122, (int)by + 50, 4, new Color((byte)255,(byte)200,(byte)50,(byte)255));
        Raylib.DrawCircle((int)bx + 133, (int)by + 51, 4, new Color((byte)220,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle((int)bx + 144, (int)by + 50, 4, new Color((byte)255,(byte)200,(byte)50,(byte)255));
        // front door - warm wood with welcome mat detail
        Raylib.DrawRectangle((int)bx + 58, (int)by + 72, 44, 48, new Color((byte)140,(byte)85,(byte)35,(byte)255));
        Raylib.DrawRectangle((int)bx + 62, (int)by + 76, 36, 20, new Color((byte)160,(byte)100,(byte)45,(byte)255)); // upper panel
        Raylib.DrawRectangle((int)bx + 62, (int)by + 100, 36, 16, new Color((byte)160,(byte)100,(byte)45,(byte)255)); // lower panel
        Raylib.DrawCircle((int)bx + 97, (int)by + 95, 4, new Color((byte)200,(byte)160,(byte)60,(byte)255)); // brass handle
        // letterbox / house number
        Raylib.DrawRectangle((int)bx + 10, (int)by + 90, 30, 14, new Color((byte)140,(byte)100,(byte)50,(byte)255));
        Raylib.DrawRectangle((int)bx + 12, (int)by + 97, 26, 4, new Color((byte)80,(byte)50,(byte)20,(byte)255)); // slot
        // sign
        Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 120, 10, new Color((byte)160,(byte)90,(byte)40,(byte)220));
        
        // garden path
        for (int px = (int)bx + 68; px < (int)bx + 93; px += 8)
            Raylib.DrawRectangle(px, (int)by + 118, 6, 20, new Color((byte)200,(byte)185,(byte)150,(byte)255));
    }
}
// ─── END DRAW ALL BUILDING EXTERIORS ────────────────────────────────────────

    static void DrawGasStation(GasStation station, float x, float y)
{
    // forecourt
    Raylib.DrawRectangle((int)x + 30, (int)y - 580, 700, 580, Color.DarkGray);

    // canopy
    Raylib.DrawRectangle((int)x + 40, (int)y - 360, 620, 160,
        new Color((byte)80,(byte)80,(byte)80,(byte)60));
    Raylib.DrawRectangle((int)x + 40, (int)y - 360, 620, 10,
        new Color((byte)255,(byte)255,(byte)0,(byte)150));
    Raylib.DrawRectangle((int)x + 40, (int)y - 210, 620, 10,
        new Color((byte)255,(byte)255,(byte)0,(byte)150));

    // canopy pillars
    Raylib.DrawRectangle((int)x + 50,  (int)y - 350, 20, 140,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)x + 290, (int)y - 350, 20, 140,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)x + 530, (int)y - 350, 20, 140,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));

    // lane dividers
    Raylib.DrawRectangle((int)x + 190, (int)y - 560, 12, 540,
        new Color((byte)255,(byte)255,(byte)0,(byte)120));
    Raylib.DrawRectangle((int)x + 390, (int)y - 560, 12, 540,
        new Color((byte)255,(byte)255,(byte)0,(byte)120));

    // entry markings
    Raylib.DrawRectangle((int)x + 80,  (int)y - 20, 80, 20, Color.Yellow);
    Raylib.DrawRectangle((int)x + 280, (int)y - 20, 80, 20, Color.Yellow);
    Raylib.DrawRectangle((int)x + 480, (int)y - 20, 80, 20, Color.Yellow);

    // pump 1
    Vector2 p1 = station.Pump1Pos;
    Raylib.DrawRectangle((int)p1.X - 18, (int)p1.Y - 35, 36, 60,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)p1.X - 12, (int)p1.Y - 28, 24, 36,
        station.Pump1Active ? Color.Green : new Color((byte)200,(byte)50,(byte)50,(byte)255));
    Raylib.DrawText("PUMP 1", (int)p1.X - 24, (int)p1.Y + 30, 16, Color.White);
    Raylib.DrawText("R = Fuel", (int)p1.X - 24, (int)p1.Y + 48, 14, Color.LightGray);

    // pump 2
    Vector2 p2 = station.Pump2Pos;
    Raylib.DrawRectangle((int)p2.X - 18, (int)p2.Y - 35, 36, 60,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)p2.X - 12, (int)p2.Y - 28, 24, 36,
        station.Pump2Active ? Color.Green : new Color((byte)200,(byte)50,(byte)50,(byte)255));
    Raylib.DrawText("PUMP 2", (int)p2.X - 24, (int)p2.Y + 30, 16, Color.White);
    Raylib.DrawText("R = Fuel", (int)p2.X - 24, (int)p2.Y + 48, 14, Color.LightGray);
}
foreach (GasStation gs in gasStations)
    DrawGasStation(gs, gs.OriginX, gs.OriginY);


            foreach (TreeObject tree in trees)
            {
                tree.Draw();
            }
            foreach (RockObject rock in rocks)
            rock.Draw();

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
            foreach (Rideable rideable in rideables)
                rideable.Draw();

             foreach (Enemy enemy in enemies)
            {
                enemy.Draw();
            }

            foreach (LootDrop drop in lootDrops)
            {
                drop.Draw();
            }

            // Draw axe pickup
if (!axePickedUp)
{
    int ax = (int)axePosition.X;
    int ay = (int)axePosition.Y;
    // Handle
    Raylib.DrawRectangle(ax - 2, ay - 20, 5, 28, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    // Axe head
    Raylib.DrawTriangle(
        new Vector2(ax + 3, ay - 20),
        new Vector2(ax + 18, ay - 26),
        new Vector2(ax + 14, ay - 8),
        new Color((byte)160,(byte)160,(byte)170,(byte)255)
    );
    // Glow/pickup hint
    Raylib.DrawCircle(ax, ay - 10, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    
    if (Vector2.Distance(player.Position, axePosition) < 60)
        Raylib.DrawText("E = Pick up Axe", ax - 50, ay - 50, 18, Color.Gold);
}

// Pickaxe
if (!pickaxePickedUp)
{
    int px = (int)pickaxePosition.X;
    int py = (int)pickaxePosition.Y;
    Raylib.DrawRectangle(px - 2, py - 20, 5, 28, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawTriangle(
        new Vector2(px + 3, py - 20),
        new Vector2(px + 16, py - 28),
        new Vector2(px + 16, py - 10),
        new Color((byte)160,(byte)160,(byte)170,(byte)255)
    );
    Raylib.DrawCircle(px, py - 10, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, pickaxePosition) < 60)
        Raylib.DrawText("E = Pick up Pickaxe", px - 60, py - 50, 18, Color.Gold);
}

// Fishing rod
if (!fishingRodPickedUp)
{
    int rx = (int)fishingRodPosition.X;
    int ry = (int)fishingRodPosition.Y;
    Raylib.DrawRectangle(rx - 2, ry - 30, 4, 32, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawLine(rx + 2, ry - 30, rx + 20, ry - 10, new Color((byte)200,(byte)200,(byte)200,(byte)255));
    Raylib.DrawCircle(rx + 20, ry - 10, 3, new Color((byte)100,(byte)180,(byte)220,(byte)255));
    Raylib.DrawCircle(rx, ry - 15, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, fishingRodPosition) < 60)
        Raylib.DrawText("E = Pick up Fishing Rod", rx - 70, ry - 60, 18, Color.Gold);
}

// Fishing net
if (!fishingNetPickedUp)
{
    int nx = (int)fishingNetPosition.X;
    int ny = (int)fishingNetPosition.Y;
    for (int i = 0; i < 3; i++)
        Raylib.DrawLine(nx, ny - 20, nx - 15 + i * 15, ny + 5, new Color((byte)180,(byte)160,(byte)80,(byte)255));
    for (int i = 0; i < 2; i++)
        Raylib.DrawLine(nx - 15, ny - 10 + i * 10, nx + 15, ny - 10 + i * 10, new Color((byte)180,(byte)160,(byte)80,(byte)255));
    Raylib.DrawCircle(nx, ny - 8, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, fishingNetPosition) < 60)
        Raylib.DrawText("E = Pick up Fishing Net", nx - 70, ny - 50, 18, Color.Gold);
}

// Torch
if (!torchPickedUp)
{
    int tx = (int)torchPosition.X;
    int ty = (int)torchPosition.Y;
    Raylib.DrawRectangle(tx - 3, ty - 24, 6, 24, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawCircle(tx, ty - 26, 5, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawCircle(tx, ty - 26, 8, new Color((byte)255,(byte)100,(byte)0,(byte)80));
    Raylib.DrawCircle(tx, ty - 8, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, torchPosition) < 60)
        Raylib.DrawText("E = Pick up Torch", tx - 50, ty - 55, 18, Color.Gold);
}

// Stick pickup
if (!stickPickedUp)
{
    int sx = (int)stickPosition.X;
    int sy = (int)stickPosition.Y;
    Raylib.DrawRectangle(sx - 2, sy - 20, 5, 32,
        new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawRectangle(sx - 1, sy - 22, 3, 5,
        new Color((byte)100,(byte)60,(byte)20,(byte)255));
    Raylib.DrawCircle(sx, sy - 10, 20, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, stickPosition) < 60)
        Raylib.DrawText("E = Pick up Stick", sx - 50, sy - 50, 18, Color.Gold);
}

// Sword pickup
if (!swordPickedUp)
{
    int sx = (int)swordPosition.X;
    int sy = (int)swordPosition.Y;
    // blade
    Raylib.DrawRectangle(sx - 2, sy - 24, 5, 26,
        new Color((byte)180,(byte)190,(byte)200,(byte)255));
    Raylib.DrawTriangle(
        new Vector2(sx - 2, sy - 24),
        new Vector2(sx + 3, sy - 24),
        new Vector2(sx,     sy - 32),
        new Color((byte)200,(byte)210,(byte)220,(byte)255));
    Raylib.DrawRectangle(sx - 10, sy + 2, 21, 5,
        new Color((byte)180,(byte)140,(byte)40,(byte)255));
    Raylib.DrawRectangle(sx - 2, sy + 7, 5, 14,
        new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawCircle(sx, sy + 21, 4,
        new Color((byte)180,(byte)140,(byte)40,(byte)255));
    Raylib.DrawCircle(sx, sy - 10, 22, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    if (Vector2.Distance(player.Position, swordPosition) < 60)
        Raylib.DrawText("E = Pick up Sword", sx - 50, sy - 60, 18, Color.Gold);
}

            DrawStreetLights();
            fenceManager.Draw();

            player.Draw();

            Raylib.EndMode2D();

            Color overlay = GetNightOverlay();
            // if torch equipped and it's dark, reduce overlay
            if (GetEquippedTool() == "Torch" && overlay.A > 80)
            {
                torchActive = true;
                overlay = new Color(overlay.R, overlay.G, overlay.B, (byte)(overlay.A / 2));
            }
            else { torchActive = false; }
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, overlay);
            DrawWeather();

            DrawHUD();
            DrawArmorUI();
        }

    static void DrawInterior()
{
    Raylib.ClearBackground(new Color(40,40,40,255));
    

    Raylib.BeginMode2D(camera);

    Raylib.DrawRectangle(0,0,1400,1000,currentBuilding.InteriorColor);
    
// draw player position as a dot so we can see it
    Raylib.DrawCircle((int)player.Position.X, (int)player.Position.Y, 8, Color.Yellow);

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
        Raylib.DrawText($"#{i+1}", cx + 56, 50, 14, Color.White);
    }
    Raylib.DrawText("CHECKOUTS", 540, 10, 22, Color.DarkGreen);

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
        Raylib.DrawText($"AISLE {col+1}", ax + 74, 122, 16, Color.White);
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
        Raylib.DrawText(meatTypes[i], 1310, fy + 100, 11, Color.DarkBlue);
    }
    Raylib.DrawText("MEAT & SEAFOOD", 1295, 120, 14, Color.DarkBlue);

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
        Raylib.DrawText(binLabels[i], bx + 2, 844, 12, Color.DarkGreen);
    }
    Raylib.DrawText("FRUIT & VEG", 160, 726, 20, Color.DarkGreen);

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
    Raylib.DrawText("DELI", 762, 726, 20, Color.DarkGray);
    Raylib.DrawText("E = Order", 688, 758, 14, Color.DarkGray);

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
    Raylib.DrawText("TROLLEYS", 150, 900, 14, Color.DarkGray);
    Raylib.DrawText("BASKETS", 970, 900, 14, Color.DarkGray);

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
    Raylib.DrawText("WAIKATO BANK", 598, 886, 16,
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
    Raylib.DrawText("WAITING", 55, 756, 16,
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
    Raylib.DrawText("ATM", 1326, 628, 18, Color.White);
    Raylib.DrawText("24/7", 1326, 648, 14, Color.LightGray);

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
        Raylib.DrawText(tellerNames[i], bx + 54, 374, 14,
            new Color((byte)220, (byte)185, (byte)70, (byte)255));
        // approach prompt label
        Raylib.DrawText("Z=Deposit X=Withdraw", bx + 10, 400, 12,
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
    Raylib.DrawText("VAULT", 672, 60, 12,
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
    Raylib.DrawText("MANAGER", 100, 27, 16,
        new Color((byte)220, (byte)185, (byte)70, (byte)255));



    // =============================================
    // LABELS
    // =============================================
    Raylib.DrawText("WAIKATO BANK", 550, 430, 20,
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
    Raylib.DrawText("*", 693, 668, 18,
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
    Raylib.DrawText("WHARENUI", 580, 20, 22,
        new Color((byte)220, (byte)160, (byte)60, (byte)255));
    Raylib.DrawText("ATEA", 660, 600,  20,
        new Color((byte)180, (byte)140, (byte)60, (byte)200));
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
    currentBuilding.BuildingName != "STORE")
{
    foreach (Rectangle obj in currentBuilding.InteriorObjects)
        Raylib.DrawRectangleRec(obj, Color.DarkBrown);
}

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
    Raylib.DrawText("★", 1226, 18, 20, new Color((byte)220, (byte)180, (byte)0, (byte)255));

    Raylib.DrawText("RECEPTION", 1130, 92, 20, new Color((byte)160, (byte)180, (byte)220, (byte)255));
    Raylib.DrawText("POLICE STATION", 1080, 500, 18, new Color((byte)100, (byte)120, (byte)180, (byte)255));

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
        Raylib.DrawText(offLabels[r], rx + 88, 8, 14, new Color((byte)220, (byte)220, (byte)255, (byte)255));
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
        Raylib.DrawText(botLabels[r], rx + 88, 976, 14, new Color((byte)220, (byte)220, (byte)255, (byte)255));
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
    Raylib.DrawText("NOTICES", 448, 465, 12, new Color((byte)80, (byte)50, (byte)10, (byte)255));
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

    Raylib.DrawText("RECEPTION", 1178, 92, 22, new Color((byte)60, (byte)80, (byte)80, (byte)255));

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
        Raylib.DrawText($"R{roomNums[r]}", rx + 100, 7, 16, Color.White);

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
        Raylib.DrawText($"R{roomNums[r + 3]}", rx + 100, 977, 16, Color.White);

        // IV stand
        Raylib.DrawRectangle(rx + 200, 840, 4, 80, new Color((byte)160, (byte)160, (byte)170, (byte)255));
        Raylib.DrawCircle(rx + 202, 840, 8, new Color((byte)200, (byte)220, (byte)240, (byte)220));
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
    Raylib.DrawText("$", 1076, 60, 20, Color.White);
    // lottery scratch card display
    Raylib.DrawRectangle(820, 60, 70, 22, new Color((byte)220,(byte)180,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(820, 60, 70, 22, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Raylib.DrawText("LOTTO", 828, 64, 12, new Color((byte)100,(byte)60,(byte)0,(byte)255));
    // card reader terminal
    Raylib.DrawRectangle(900, 58, 30, 26, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(902, 60, 26, 16, new Color((byte)0,(byte)80,(byte)180,(byte)255));
    Raylib.DrawText("COUNTER", 940, 92, 20, new Color((byte)200,(byte)195,(byte)160,(byte)255));
    
 
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
        Raylib.DrawText(aisleSignNames[col], ax + 36, 172, 16, Color.Gold);
 
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
        Raylib.DrawText(fridgeDrinks[f], fx + 6, 200, 11, Color.DarkBlue);
    }
    Raylib.DrawText("COOL DRINKS", 600, 64, 14, Color.DarkGray);
 
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
        Raylib.DrawText(oilLabels[o], 18, oy + 72, 10, Color.DarkGray);
    }
    Raylib.DrawText("AUTO", 20, 164, 12, Color.DarkGray);
 
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
    Raylib.DrawText("WC", 1120, 204, 14, Color.DarkGray);
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
    Raylib.DrawText("WELCOME", 518, 878, 16, new Color((byte)200,(byte)195,(byte)165,(byte)255));
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
    Raylib.DrawText("E = Upgrade Weapon", 480, 92, 16, new Color((byte)200,(byte)200,(byte)100,(byte)255));
 
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
        Raylib.DrawText(swordLabels[i], rx - 10, 92, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
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
        Raylib.DrawText(bowLabels[i], rx - 12, 180, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
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
        Raylib.DrawText(shieldLabels[i], rx - 5, 284, 10, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    Raylib.DrawText("BLADES",  50,  22, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    Raylib.DrawText("RANGED",  50, 122, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    Raylib.DrawText("SHIELDS", 50, 222, 14, new Color((byte)180,(byte)180,(byte)190,(byte)255));
 
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
        Raylib.DrawText(caseTitles[c], cx + 30, cy + 64, 14, caseColors[c]);
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
        Raylib.DrawText(armourNames[a], ax + 6, 228, 14, new Color((byte)160,(byte)160,(byte)170,(byte)255));
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
        Raylib.DrawText(suppLabels[s], 1052, sy + 32, 12, new Color((byte)140,(byte)140,(byte)150,(byte)255));
    }
 
    // --- ENTRANCE MAT ---
    Raylib.DrawRectangle(500, 870, 200, 30, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(500, 870, 200, 30, new Color((byte)80,(byte)80,(byte)90,(byte)255));
    Raylib.DrawText("WELCOME", 550, 878, 16, new Color((byte)120,(byte)120,(byte)140,(byte)255));
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
    Raylib.DrawText("$", 896, 59, 20, Color.White);
    Raylib.DrawText("SERVICE COUNTER", 480, 95, 20, new Color((byte)100, (byte)170, (byte)255, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 190, 1400, 28, new Color((byte)20, (byte)28, (byte)50, (byte)230));
    string[] bikeZoneNames = { "MOUNTAIN BIKES", "ROAD BIKES", "ACCESSORIES" };
    int[]    bikeZoneX     = { 60, 430, 820 };
    for (int a = 0; a < 3; a++)
        Raylib.DrawText(bikeZoneNames[a], bikeZoneX[a], 196, 16, new Color((byte)100, (byte)180, (byte)255, (byte)255));
 
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
            Raylib.DrawText(row == 0 ? "$1,299" : "$899", ix + 184, iy + 12, 14, Color.Gold);
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
    Raylib.DrawText("WELCOME", 612, 940, 22, new Color((byte)100, (byte)170, (byte)255, (byte)255));
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
    Raylib.DrawText("SALES", 1266, 35, 14, Color.White);
    Raylib.DrawText("FINANCE & SALES COUNTER", 820, 68, 18, new Color((byte)80, (byte)150, (byte)255, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 190, 800, 28, new Color((byte)15, (byte)15, (byte)18, (byte)230));
    Raylib.DrawText("SHOWROOM FLOOR", 80, 196, 18, new Color((byte)80, (byte)150, (byte)255, (byte)255));
 
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
            Raylib.DrawText(carLabels[row, col], bx2 + 225, by2 + 12, 14, Color.Gold);
        }
    }
 
    // --- entrance mat ---
    Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)18, (byte)18, (byte)22, (byte)255));
    Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)120, (byte)220, (byte)255));
    Raylib.DrawText("WELCOME", 610, 940, 22, new Color((byte)80, (byte)150, (byte)255, (byte)255));
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
    Raylib.DrawText("RECEPTION", 620, 78, 22, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
    // --- overhead sign bar ---
    Raylib.DrawRectangle(0, 165, 1400, 28, new Color((byte)80, (byte)45, (byte)18, (byte)220));
    string[] barnZones = { "HORSES", "MOUNTS", "FEED & TACK" };
    int[]    barnZoneX = { 140, 570, 1000 };
    for (int a = 0; a < 3; a++)
        Raylib.DrawText(barnZones[a], barnZoneX[a], 172, 16, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
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
        Raylib.DrawText(horseNames[s], 24, sy2 + 4, 12, Color.Gold);
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
        Raylib.DrawText(horseNames2[s], 1104, sy2 + 4, 12, Color.Gold);
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
    Raylib.DrawText("PREMIUM FEED & HAY", 468, 264, 18, new Color((byte)220, (byte)185, (byte)100, (byte)255));
 
    // --- entrance mat ---
    Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)80, (byte)55, (byte)25, (byte)255));
    Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)140, (byte)95, (byte)40, (byte)255));
    Raylib.DrawText("WELCOME", 610, 940, 22, new Color((byte)220, (byte)185, (byte)100, (byte)255));
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
    Raylib.DrawText("$", 834, 62, 20, Color.White);
    Raylib.DrawText("COUNTER", 540, 92, 22, new Color((byte)200, (byte)160, (byte)80, (byte)255));

    // --- aisle sign bar ---
    Raylib.DrawRectangle(0, 190, 1400, 25, new Color((byte)60, (byte)40, (byte)20, (byte)220));
    string[] aisleNames = { "FOOD & DRINK", "TOOLS & GEAR", "SUPPLIES" };
    int[] aisleSignX = { 80, 420, 760 };
    for (int a = 0; a < 3; a++)
        Raylib.DrawText(aisleNames[a], aisleSignX[a], 195, 16, Color.Gold);

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
    Raylib.DrawText($"AISLE {col + 1}", ax + 104, 169, 15, Color.Gold);
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
Raylib.DrawText("WELCOME", 610, 940, 22, new Color((byte)180, (byte)220, (byte)180, (byte)255));

// --- entrance mat ---
Raylib.DrawRectangle(550, 920, 300, 60, new Color((byte)40, (byte)80, (byte)40, (byte)255));
Raylib.DrawRectangleLines(550, 920, 300, 60, new Color((byte)60, (byte)100, (byte)60, (byte)255));
Raylib.DrawText("WELCOME", 610, 940, 22, new Color((byte)180, (byte)220, (byte)180, (byte)255));
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
    Raylib.DrawText("ENTRY", 648, 938, 16, Color.White);

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
        Raylib.DrawText(domMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)190,(byte)20,(byte)20,(byte)255));
 
    // Pizza oven (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)100,(byte)30,(byte)10,(byte)255));
    Raylib.DrawText("OVEN", 1128, 52, 12, Color.Orange);
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
    Raylib.DrawText("DELIVERY AREA", 1132, 320, 18, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    // delivery bags
    for (int db = 0; db < 3; db++)
    {
        Raylib.DrawRectangle(1140, 360 + db * 100, 60, 70, new Color((byte)0,(byte)60,(byte)140,(byte)255));
        Raylib.DrawRectangleLines(1140, 360 + db * 100, 60, 70, new Color((byte)190,(byte)20,(byte)20,(byte)255));
        Raylib.DrawText("BAG", 1152, 385 + db * 100, 14, Color.White);
    }
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)0,(byte)60,(byte)130,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Raylib.DrawText("DOMINO'S", 618, 882, 18, Color.White);
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
        Raylib.DrawText(kfcMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)240,(byte)225,(byte)195,(byte)255));
 
    // Fry station (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)40,(byte)25,(byte)10,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)160,(byte)100,(byte)20,(byte)255));
    Raylib.DrawText("FRYER", 1124, 52, 12, Color.Gold);
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
    Raylib.DrawText("KITCHEN", 1158, 320, 20, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    // warming lamps
    Raylib.DrawRectangle(1140, 360, 200, 10, new Color((byte)60,(byte)30,(byte)10,(byte)255));
    for (int wl = 0; wl < 4; wl++)
        Raylib.DrawCircle(1155 + wl * 48, 365, 8, new Color((byte)255,(byte)150,(byte)30,(byte)200));
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)160,(byte)15,(byte)15,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Raylib.DrawText("KFC", 672, 882, 18, new Color((byte)240,(byte)225,(byte)195,(byte)255));
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
        Raylib.DrawText(bkMenuItems[m], 165 + m * 155, 22, 16, new Color((byte)255,(byte)180,(byte)0,(byte)255));
 
    // Flame grill (back right)
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)35,(byte)20,(byte)5,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)180,(byte)60,(byte)0,(byte)255));
    Raylib.DrawText("GRILL", 1126, 52, 12, Color.Orange);
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
    Raylib.DrawText("CROWN LOUNGE", 1128, 320, 18, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    // lounge seats
    for (int ls = 0; ls < 2; ls++)
    {
        Raylib.DrawRectangle(1140, 380 + ls * 160, 200, 80, new Color((byte)160,(byte)15,(byte)15,(byte)255));
        Raylib.DrawRectangleLines(1140, 380 + ls * 160, 200, 80, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    }
 
    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)185,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Raylib.DrawText("BURGER KING", 590, 882, 18, new Color((byte)255,(byte)180,(byte)0,(byte)255));
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
        Raylib.DrawText(menuItems[m], 165 + m * 155, 22, 16, new Color((byte)255,(byte)220,(byte)0,(byte)255));

    // Drink machine
    Raylib.DrawRectangle(1110, 30, 80, 80, new Color((byte)50,(byte)50,(byte)50,(byte)255));
    Raylib.DrawRectangle(1115, 35, 70, 50, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Raylib.DrawText("DRINKS", 1118, 50, 12, Color.Gold);
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
    Raylib.DrawText("PLAY AREA", 1140, 320, 20, new Color((byte)220,(byte)30,(byte)30,(byte)255));
    // slide
    Raylib.DrawRectangle(1150, 360, 30, 120, new Color((byte)255,(byte)100,(byte)0,(byte)255));
    Raylib.DrawRectangle(1200, 400, 120, 20, new Color((byte)0,(byte)180,(byte)0,(byte)255)); // platform

    // Entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(560, 870, 280, 40, new Color((byte)255,(byte)200,(byte)0,(byte)255));
    Raylib.DrawText("McDONALD'S", 602, 882, 18, new Color((byte)255,(byte)220,(byte)0,(byte)255));
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
        Raylib.DrawText($"{ln}", 24, 118 + (ln - 1) * 60, 18, Color.DarkGray);
    // start blocks
    for (int ln = 0; ln < 5; ln++)
    {
        Raylib.DrawRectangle(50, 118 + ln * 60, 28, 26, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        Raylib.DrawRectangle(53, 121 + ln * 60, 22, 8, new Color((byte)100,(byte)100,(byte)120,(byte)255));
    }
    // water shimmer
    for (int wx = 80; wx < 540; wx += 60)
        Raylib.DrawRectangle(wx, 108, 40, 4, new Color((byte)100,(byte)180,(byte)220,(byte)80));
    Raylib.DrawText("LANE POOL", 220, 70, 24, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Raylib.DrawText("Space = Start Swimming", 170, 450, 18, Color.DarkGray);

    // ── DIVING POOL ──
    Raylib.DrawRectangle(700, 100, 450, 420, new Color((byte)10,(byte)100,(byte)190,(byte)255));
    Raylib.DrawRectangleLines(700, 100, 450, 420, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    // depth markings
    Raylib.DrawText("5m", 1110, 200, 16, new Color((byte)180,(byte)230,(byte)255,(byte)200));
    Raylib.DrawText("10m", 1108, 400, 16, new Color((byte)180,(byte)230,(byte)255,(byte)200));
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
    Raylib.DrawText("5m", 760, 155, 14, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    // 10m board
    Raylib.DrawRectangle(716, 280, 110, 12, new Color((byte)200,(byte)205,(byte)210,(byte)255));
    Raylib.DrawRectangle(716, 280, 110, 4, new Color((byte)230,(byte)235,(byte)240,(byte)255));
    Raylib.DrawText("10m", 760, 265, 14, new Color((byte)100,(byte)100,(byte)110,(byte)255));
    Raylib.DrawText("DIVING POOL", 850, 70, 24, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Raylib.DrawText("E = Dive (time your jump!)", 760, 540, 18, Color.DarkGray);

    // lifeguard chairs
    Raylib.DrawRectangle(580, 130, 30, 50, new Color((byte)220,(byte)50,(byte)50,(byte)255));
    Raylib.DrawRectangle(575, 110, 40, 22, new Color((byte)200,(byte)30,(byte)30,(byte)255));
    Raylib.DrawText("LIFEGUARD", 555, 90, 12, Color.Red);

    // spectator bench
    Raylib.DrawRectangle(100, 700, 1000, 30, new Color((byte)180,(byte)150,(byte)100,(byte)255));
    Raylib.DrawRectangle(100, 700, 1000, 5, new Color((byte)210,(byte)180,(byte)130,(byte)255));
    Raylib.DrawText("SPECTATOR AREA", 480, 740, 18, Color.DarkGray);

    // towel hooks
    for (int h = 0; h < 6; h++)
    {
        Raylib.DrawRectangle(80 + h * 180, 800, 12, 30, new Color((byte)80,(byte)80,(byte)90,(byte)255));
        Raylib.DrawRectangle(78 + h * 180, 826, 16, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    }

    // entrance mat
    Raylib.DrawRectangle(560, 870, 280, 40, new Color((byte)10,(byte)80,(byte)160,(byte)255));
    Raylib.DrawText("SWIMMING COMPLEX", 575, 882, 16, Color.White);

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
    // Court surface
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)50,(byte)160,(byte)50,(byte)255));

    // Court lines
    Raylib.DrawRectangleLines(60, 80, 1280, 560, Color.White);
    // Service boxes
    Raylib.DrawLine(700, 80, 700, 640, Color.White);   // centre line
    Raylib.DrawLine(60, 360, 1340, 360, Color.White);  // service line
    Raylib.DrawLine(60, 200, 700, 200, Color.White);   // left service boxes
    Raylib.DrawLine(700, 200, 1340, 200, Color.White);
    // Net
    Raylib.DrawRectangle(670, 80, 10, 560, new Color((byte)200,(byte)200,(byte)200,(byte)255));
    Raylib.DrawRectangle(665, 78, 20, 10, new Color((byte)160,(byte)160,(byte)160,(byte)255));
    Raylib.DrawRectangle(665, 628, 20, 10, new Color((byte)160,(byte)160,(byte)160,(byte)255));
    // Net mesh lines
    for (int ny = 88; ny < 638; ny += 20)
        Raylib.DrawLine(670, ny, 680, ny, new Color((byte)160,(byte)160,(byte)160,(byte)120));

    // Baselines ticks
    for (int bx2 = 60; bx2 <= 1340; bx2 += 64)
        Raylib.DrawRectangle(bx2, 636, 4, 10, Color.White);

    // Score display
    Raylib.DrawRectangle(540, 0, 320, 70, new Color((byte)20,(byte)20,(byte)20,(byte)200));
    Raylib.DrawText($"YOU: {tennisPlayerScore}  AI: {tennisAIScore}", 560, 15, 28, Color.Gold);

    if (tennisActive)
    {
        // Player paddle (left)
        Raylib.DrawRectangle(80, (int)tennisPlayerPaddleY, 22, 60, new Color((byte)255,(byte)200,(byte)0,(byte)255));
        Raylib.DrawRectangleLines(80, (int)tennisPlayerPaddleY, 22, 60, Color.White);
        // AI paddle (right)
        Raylib.DrawRectangle(1270, (int)tennisAIPaddleY, 22, 60, new Color((byte)220,(byte)50,(byte)50,(byte)255));
        Raylib.DrawRectangleLines(1270, (int)tennisAIPaddleY, 22, 60, Color.White);
        // Ball
        Raylib.DrawCircle((int)tennisBallPos.X, (int)tennisBallPos.Y, 10, new Color((byte)200,(byte)220,(byte)50,(byte)255));
        Raylib.DrawCircleLines((int)tennisBallPos.X, (int)tennisBallPos.Y, 10, Color.DarkGreen);
    }

    Raylib.DrawText("YOU", 68, 72, 16, Color.Gold);
    Raylib.DrawText("AI", 1272, 72, 16, new Color((byte)220,(byte)50,(byte)50,(byte)255));

    if (tennisMessageTimer > 0)
        Raylib.DrawText(tennisMessage, 200, 700, 22, Color.White);

    if (!tennisActive)
        Raylib.DrawText("Space = Start Game (stand near net)", 400, 700, 22, Color.LightGray);
    else if (tennisServing)
        Raylib.DrawText("SPACE = Serve!", 560, 680, 24, Color.Gold);
    else
        Raylib.DrawText("W/S = Move  |  Q = Quit", 480, 680, 22, Color.LightGray);

    // entrance mat
    Raylib.DrawRectangle(560, 900, 280, 40, new Color((byte)30,(byte)100,(byte)30,(byte)255));
    Raylib.DrawText("TENNIS COURT", 592, 912, 18, Color.White);
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
    Raylib.DrawText("FREE THROW LINE", 280, 490, 16, new Color((byte)255,(byte)255,(byte)255,(byte)180));
    Raylib.DrawLine(270, 460, 510, 460, new Color((byte)255,(byte)200,(byte)0,(byte)200));

    // Score display
    Raylib.DrawRectangle(540, 0, 320, 70, new Color((byte)20,(byte)20,(byte)20,(byte)200));
    Raylib.DrawText($"SCORE: {bbScore}", 600, 18, 30, Color.Gold);

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
        Raylib.DrawText(bbMessage, 200, 700, 24, Color.Gold);

    Raylib.DrawText(!basketballActive ? "Space = Shoot (near free throw)" : "SPACE = Lock Power, then SPACE = Lock Aim | Q = Quit",
        130, 870, 20, Color.LightGray);

    // entrance mat
    Raylib.DrawRectangle(560, 920, 280, 40, new Color((byte)160,(byte)80,(byte)10,(byte)255));
    Raylib.DrawText("BASKETBALL COURT", 578, 932, 16, Color.White);
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
     // HUD text outside BeginMode2D
    Raylib.DrawText($"Player: {(int)player.Position.X}, {(int)player.Position.Y}", 20, 50, 24, Color.White);

    if (shopMessageTimer > 0)
    {
        byte alpha = (byte)(255 * Math.Min(1f, shopMessageTimer));
        Raylib.DrawText(shopMessage, 480, 560, 30, new Color((byte)255, (byte)215, (byte)0, alpha));
    }

    // Swimming minigame HUD
if (currentBuilding.BuildingName == "SWIMMING COMPLEX" && swimmingActive)
{
    if (swimmingPoolType == "lane")
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Raylib.DrawText($"SWIMMING LAPS — Lap {swimLapsCompleted + 1}", 20, 630, 28, Color.SkyBlue);
        int barW = 900;
        float prog = swimLapTimer / swimLapDuration;
        Raylib.DrawRectangle(190, 658, barW, 32, new Color((byte)20,(byte)60,(byte)120,(byte)255));
        Raylib.DrawRectangle(190, 658, (int)(barW * prog), 32, new Color((byte)30,(byte)160,(byte)220,(byte)255));
        Raylib.DrawRectangleLines(190, 658, barW, 32, Color.White);
        Raylib.DrawText($"+30 Athletics XP per lap | Q = Stop", 190, 700, 18, Color.LightGray);
    }
    else if (swimmingPoolType == "diving")
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));

        if (!divingJumped)
        {
            Raylib.DrawText("DIVING — Time your jump! Land near CENTRE for high score.", 20, 628, 22, Color.SkyBlue);
            int barW = 900;
            int barX = 190;
            int barY = 658;
            int barH = 32;
            // background
            Raylib.DrawRectangle(barX, barY, barW, barH, new Color((byte)20,(byte)40,(byte)80,(byte)255));
            // sweet spot centre
            int centreW = (int)(barW * 0.1f);
            Raylib.DrawRectangle(barX + barW / 2 - centreW / 2, barY, centreW, barH,
                new Color((byte)0,(byte)220,(byte)0,(byte)255));
            // good zone
            int goodW = (int)(barW * 0.24f);
            Raylib.DrawRectangle(barX + barW / 2 - goodW / 2, barY, goodW, barH,
                new Color((byte)100,(byte)220,(byte)50,(byte)180));
            // cursor
            int cursorX = barX + (int)(divingBarPos * barW) - 5;
            Raylib.DrawRectangle(cursorX, barY - 6, 10, barH + 12, Color.White);
            Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.White);
            Raylib.DrawText("SPACE = Jump!", 190, 698, 18, Color.LightGray);
        }
        else
        {
            Raylib.DrawText(divingScore >= 8 ? $"🤸 {divingResult}" : divingResult,
                20, 648, 28, divingScore >= 8 ? Color.Gold : Color.White);
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
        Raylib.DrawText("SHOT POWER — Hit SPACE when bar is in the sweet spot!", 20, 628, 22, Color.Gold);
        Raylib.DrawRectangle(barX2, 658, barW, 32, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // sweet spot (0.5–0.8)
        int sweetX = barX2 + (int)(0.5f * barW);
        int sweetW = (int)(0.3f * barW);
        Raylib.DrawRectangle(sweetX, 658, sweetW, 32, new Color((byte)0,(byte)200,(byte)0,(byte)255));
        // cursor
        int cursorX2 = barX2 + (int)(bbPower * barW) - 6;
        Raylib.DrawRectangle(cursorX2, 652, 12, 44, Color.White);
        Raylib.DrawRectangleLines(barX2, 658, barW, 32, Color.White);
        Raylib.DrawText("SPACE = Lock Power", barX2, 698, 18, Color.LightGray);
    }
    else
    {
        Raylib.DrawText("AIM — Hit SPACE when cursor is in the centre!", 20, 628, 22, Color.Gold);
        Raylib.DrawRectangle(barX2, 658, barW, 32, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // aim sweet spot (0.35–0.65)
        int aimSweetX = barX2 + (int)(0.35f * barW);
        int aimSweetW = (int)(0.3f * barW);
        Raylib.DrawRectangle(aimSweetX, 658, aimSweetW, 32, new Color((byte)0,(byte)200,(byte)0,(byte)255));
        int aimCursorX = barX2 + (int)(bbAimX * barW) - 6;
        Raylib.DrawRectangle(aimCursorX, 652, 12, 44, Color.White);
        Raylib.DrawRectangleLines(barX2, 658, barW, 32, Color.White);
        Raylib.DrawText("SPACE = Shoot!", barX2, 698, 18, Color.LightGray);
    }
}

// McDonald's message
if (currentBuilding.BuildingName == "McDONALD'S" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Raylib.DrawText(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "Dominos" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Raylib.DrawText(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "KFC" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Raylib.DrawText(mcdonaldsMessage, 300, 560, 28,
        new Color((byte)255,(byte)220,(byte)0, alpha));
}
if (currentBuilding.BuildingName == "BurgerKing" && mcdonaldsMessageTimer > 0)
{
    byte alpha = (byte)(255 * Math.Min(1f, mcdonaldsMessageTimer));
    Raylib.DrawText(mcdonaldsMessage, 300, 560, 28,
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
        if (Vector2.Distance(player.Position, boothPositions[i]) < 130)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Raylib.DrawText(tellerNames[i], 20, 630, 30, Color.Gold);
            Raylib.DrawText("Z = Deposit $10 | X = Withdraw $10", 20, 670, 24, Color.White);
            break; // only show one at a time if somehow near two
        }
    }
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
                Raylib.DrawText($"Space = Start set | Hit greens to keep going, miss to end | Str Lv {player.StrengthLevel}", 
                    20, 650, 22, Color.White);
            }
            else if (nearBench)
            {
                Raylib.DrawText("BENCH PRESS", 20, 610, 28, Color.Gold);
                Raylib.DrawText($"Space = Start set | Hit moving green target, miss to end | Str Lv {player.StrengthLevel}", 
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
            Raylib.DrawText($"Space = Play a round ($2) | Wallet: ${player.Money}", 20, 670, 24, Color.White);
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
        {
            Vector2[] boothPositions = {
                new Vector2(250, 370),
                new Vector2(600, 370),
                new Vector2(950, 370)
            };
            bool nearBooth = boothPositions.Any(bp =>
                Vector2.Distance(player.Position, bp) < 130);

            if (nearBooth)
                Raylib.DrawText($"Z = Deposit $10 | X = Withdraw $10 | Balance: ${player.BankBalance}", 20, 600, 22, Color.LightGray);
        }

        if (currentBuilding.BuildingName == "MY HOUSE")
        {
            Vector2 wardrobePos = new Vector2(1080, 810);
            Vector2 chestPos    = new Vector2(872, 915);
            Vector2 bedPos      = new Vector2(1180, 585);

            if (Vector2.Distance(player.Position, wardrobePos) < 200)
                Raylib.DrawText("E = Open Wardrobe", 20, 600, 22, Color.LightGray);
            else if (Vector2.Distance(player.Position, chestPos) < 200)
                Raylib.DrawText("E = Open Chest", 20, 600, 22, Color.LightGray);
            else if (Vector2.Distance(player.Position, bedPos) < 200)
                Raylib.DrawText("Z = Sleep (restore health + sober up)", 20, 600, 22, Color.LightGray);
    
        }

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


  if (currentBuilding.BuildingName == "SUPERMARKET")
{
    bool nearCashier = Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120;

    if (!nearCashier && (player.HasTrolley || player.HasBasket))
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        if (player.HasTrolley)
        {
            Raylib.DrawText("TROLLEY", 20, 630, 30, Color.Gold);
            Raylib.DrawText("I = Open Inventory | Space = Put Down near entrance", 20, 670, 24, Color.White);
        }
        else
        {
            Raylib.DrawText("BASKET", 20, 630, 30, Color.Gold);
            Raylib.DrawText("I = Open Inventory | Space = Put Down near entrance", 20, 670, 24, Color.White);
        }
    }
}

if (currentBuilding.BuildingName == "McDONALD'S" &&
    Vector2.Distance(player.Position, currentBuilding.InteriorNPC.Position) < 120)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Raylib.DrawText("McDONALD'S", 20, 630, 30, new Color((byte)255,(byte)220,(byte)0,(byte)255));
    Raylib.DrawText("E = Open Menu | Food restores HP", 20, 670, 24, Color.White);
}

    DrawChestUI();
    DrawWardrobe();
    DrawShopUI();
    DrawSupermarketInventoryUI();
    DrawMcDonaldsMenu();
    DrawKFCMenu();
    DrawDominosMenu();
    DrawBurgerKingMenu();
    DrawDealerUI();
}

        static void DrawHUD()
        {
            DrawSkillsUI();
            DrawQuestsUI();

            // Coordinates display with background
            string coordText = $"X: {(int)player.Position.X}  Y: {(int)player.Position.Y}";
            int coordWidth = Raylib.MeasureText(coordText, 18);
            Raylib.DrawRectangle(244, 6, coordWidth + 8, 26, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            Raylib.DrawText(coordText, 250, 10, 18, Color.White);

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

            foreach (Rideable rideable in rideables)
{
    if (rideable.Riding)
    {
        string rideableName = rideable.Type switch
        {
            Rideable.RideableType.MountainBike => "MOUNTAIN BIKE",
            Rideable.RideableType.BMX          => "BMX",
            Rideable.RideableType.Horse        => "HORSE",
            _                                  => "RIDEABLE"
        };
        break;
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

            if (currentScene == SceneState.World)
    DrawToolbar();

    if (camera.Zoom != 1f)
{
    Raylib.DrawText($"Zoom: {camera.Zoom:F1}x  (Alt+Scroll to adjust)", 20, ScreenHeight - 55, 18, Color.Gold);
}

           Raylib.DrawRectangle(0, ScreenHeight - 34, ScreenWidth, 34, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Raylib.DrawText("SPACE = Action | TAB = Inventory | E = Enter Building | F = Drive Vehicle | G = Equipment | Esc = Options", 20, ScreenHeight - 28, 20, Color.White);

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
    if (player.DogFangs > 0) items.Add(("Bones", player.DogFangs));
    if (player.Money > 0) items.Add(("Money", player.Money));
    if (player.HasAxe) items.Add(("Axe", 1));

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

    if (player.HasTrolley || player.HasBasket)
    Raylib.DrawText("SPACE = Return (anywhere near entrance)", 400, 890, 14, Color.DarkGray);
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
            // Calendar two-line box (weekday/week + time) and (month, season)
{
    string weekday = dayNames[dayOfWeek];
    int weekNum = GetWeekOfMonth();
    string month = GetMonthString();
    string season = GetSeasonString();
    string timeStr = GetTimeString();

    // Lines to show:
    // Line1: "Friday week 2   10:00 AM"
    // Line2: "January, Summer"
    string line1 = $"{weekday} week {weekNum}   {timeStr}";
    string line2 = $"{month}, {season}";

    // Box dimensions & position (top-right)
    int boxW = 320;
    int boxH = 54;            // two lines
    int boxX = ScreenWidth - boxW - 12;
    int boxY = 8;

    // Background and border
    Raylib.DrawRectangle(boxX, boxY, boxW, boxH, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Raylib.DrawRectangleLines(boxX, boxY, boxW, boxH, Color.White);

    // Text layout
    int padding = 8;
    int line1Y = boxY + 8;
    int line2Y = boxY + 28;

    // Line 1: weekday/week + time (slightly larger)
    Raylib.DrawText(line1, boxX + padding, line1Y, 20, Color.Gold);

    // Line 2: month and season (smaller)
    Raylib.DrawText(line2, boxX + padding, line2Y, 18, Color.LightGray);
}


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

// Stone rocks - safe zone outskirts
for (int i = -2800; i < 3800; i += 400)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(-1400, 2400));
    if (!IsNearRoad(pos)) rocks.Add(RockObject.Stone(pos));
}

// Copper - grasslands
for (int i = 4200; i < 12000; i += 500)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(-300, 950));
    if (!IsNearRoad(pos)) rocks.Add(RockObject.Copper(pos));
}

// Iron - snow and desert
for (int i = -30000; i < -3100; i += 600)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(400, 900));
    if (!IsNearRoad(pos)) rocks.Add(RockObject.Iron(pos));
}

// Gold - deep forest
for (int i = -30000; i < 30000; i += 800)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), -2000 + rng.Next(-500, 0));
    if (!IsNearRoad(pos)) rocks.Add(RockObject.Gold(pos));
}

// Crystals - mountains
for (int i = -55000; i < -26000; i += 700)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), -5000 + rng.Next(-1000, 0));
    rocks.Add(RockObject.Crystal(pos));
}
            

            lakes.Add(new Lake(new Vector2(700, 1200)));
            lakes.Add(new Lake(new Vector2(-900, -600)));


//Dbars
AddDBar(1700, 410);
AddDBar(-4000, 410);

//Banks
AddBank(1200, 380);
AddBank(-9850, -700); 

// Add dealers (exterior + interior)
AddDealerBuilding(340, 800, "BIKE DEALER", DealerType.Bike);
AddDealerBuilding(-258, -848, "CAR DEALER", DealerType.Car);
AddDealerBuilding(-1380, -445, "BARN DEALER", DealerType.Barn);

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

AddStore(-1000, 410);

AddHospital(340, -200);

AddWeapons(640, 150);

AddMyHouse(-400, 410);
AddMyHouse(-2090, -9855);


AddGasStation(300, -420);
AddGasStation(7000, 540);
AddGasStation(-9000, 540);

AddMarae(-6900, -230);
AddMarae(-2050, -300);

AddGym(2700, 410);

AddSupermarket(3600, 410);

AddSwimmingComplex(700, -274);

AddTennisCourt(-250, 820);

AddBasketballCourt(-550, 820);

AddMcDonalds(2250, 400);

AddKFC(1145, -192);
AddBurgerKing(-25, -1416);
AddDominos(-2572, 386);

gymCounterNPC = new NPC(new Vector2(780, 130), "Staff", "Grab a protein shake bro, $3 each.");

AddPoliceStation(3200, 410);

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

            //Fence safezone
            fenceManager.SpawnAt(new Vector2(-3000, -1500), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(345, -1500), 30, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(3980, -1500), 15, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(3980, 760), 13, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 104);
            fenceManager.SpawnAt(new Vector2(-3000, -1500), 15, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-3000, 760), 13, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 104);
            fenceManager.SpawnAt(new Vector2(-3000, 2500), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(345, 2500), 30, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);

            //Fence farmzone
            //Boundary
            fenceManager.SpawnAt(new Vector2(-3000, -10000), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(-20, -10000), 31, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 110);
            fenceManager.SpawnAt(new Vector2(-3000, -10000), 2, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 108);
            fenceManager.SpawnAt(new Vector2(-3000, -9610), 28, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 110);
            fenceManager.SpawnAt(new Vector2(-3000, -6000), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);

            //Left fences
            fenceManager.SpawnAt(new Vector2(-3000, -9000), 10, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(-2000, -8883), 2, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-3000, -7500), 5, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2325, -7500), 5, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 101);

            //Right fences
            fenceManager.SpawnAt(new Vector2(-1780, -9000), 15, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);

            //Middle fences
            fenceManager.SpawnAt(new Vector2(-1780, -10000), 30, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-1780, -6112), 1, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            

            //Trees
            trees.Add(TreeObject.Normal(new Vector2(-2650, -9750)));
            trees.Add(TreeObject.Normal(new Vector2(-2400, -9750)));
            trees.Add(TreeObject.Oak(new Vector2(-2700, -9400)));
         //   trees.Add(TreeObject.Oak(new Vector2(-2040, -8980)));
            trees.Add(TreeObject.Normal(new Vector2(-2040, -8980)));
            trees.Add(TreeObject.Birch(new Vector2(-2700, -7000)));

            //Lakes
            lakes.Add(new Lake(new Vector2(-2708, -8244)));

            //Rocks
            rocks.Add(RockObject.Stone(new Vector2(-1797, -6221)));
            rocks.Add(RockObject.Stone(new Vector2(-1761, -6164)));
            rocks.Add(RockObject.Stone(new Vector2(-1761, -6212)));
            
            //vehicles
            vehicles.Add(new Vehicle(new Vector2(100,  800), Color.Red,      650, Vehicle.VehicleType.Sedan));
            vehicles.Add(new Vehicle(new Vector2(1200, 700), Color.Yellow,   900, Vehicle.VehicleType.Truck));
            vehicles.Add(new Vehicle(new Vector2(-400, 650), Color.DarkBlue, 500, Vehicle.VehicleType.SUV));
            vehicles.Add(new Vehicle(new Vector2(3082, 458), Color.Black, 500, Vehicle.VehicleType.PoliceCar));
            vehicles.Add(new Vehicle(new Vector2(3082, 758), Color.Red, 500, Vehicle.VehicleType.FireTruck));
            vehicles.Add(new Vehicle(new Vector2(510, -189), Color.White, 500, Vehicle.VehicleType.Ambulance));


            // Mountain bikes - safe zone and grasslands
            rideables.Add(new Rideable(new Vector2(600, 650),  Rideable.RideableType.MountainBike, new Color((byte)180,(byte)80,(byte)20,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-500, 700), Rideable.RideableType.MountainBike, new Color((byte)20,(byte)100,(byte)180,(byte)255)));
            rideables.Add(new Rideable(new Vector2(5000, 600), Rideable.RideableType.MountainBike, new Color((byte)20,(byte)150,(byte)50,(byte)255)));

            // BMX bikes - safe zone
            rideables.Add(new Rideable(new Vector2(800, 650),  Rideable.RideableType.BMX, new Color((byte)220,(byte)50,(byte)50,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-300, 700), Rideable.RideableType.BMX, new Color((byte)200,(byte)150,(byte)0,(byte)255)));
            rideables.Add(new Rideable(new Vector2(1500, 800), Rideable.RideableType.BMX, new Color((byte)150,(byte)0,(byte)200,(byte)255)));

            // Horses - grasslands and safe zone outskirts
            rideables.Add(new Rideable(new Vector2(4500, 400),  Rideable.RideableType.Horse, new Color((byte)139,(byte)90,(byte)43,(byte)255)));
            rideables.Add(new Rideable(new Vector2(7000, 700),  Rideable.RideableType.Horse, new Color((byte)80,(byte)50,(byte)20,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-2000, 800), Rideable.RideableType.Horse, new Color((byte)200,(byte)180,(byte)160,(byte)255)));
            rideables.Add(new Rideable(new Vector2(3000, 500),  Rideable.RideableType.Horse, Color.White));
            rideables.Add(new Rideable(new Vector2(-450, -6400),  Rideable.RideableType.Horse, Color.White));
            rideables.Add(new Rideable(new Vector2(-2807, -9564),  Rideable.RideableType.Horse, Color.White));

            quests.Add(new Quest("Lumberjack", "Chop 10 trees", 10, 50));
            quests.Add(new Quest("Fisher", "Catch 10 fish", 10, 75));
            quests.Add(new Quest("Big Money", "Earn $100", 100, 200));

            // Grasslands - Wild Dogs
            enemies.Add(new Enemy(new Vector2(-500, 3000), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-450, 3600), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-360, 4000), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(1100, 4050), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(1000, 4400), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(670, 4150), "Wild Dog", 3, Color.Brown));

            // Forest - Wolves
            enemies.Add(new Enemy(new Vector2(-300, -2500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(400, -3000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-200, 4000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(500, 3000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(5000, -2200), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-5000, 3500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(843, 5000), "Wolf", 5, Color.DarkGray));
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

            // Ocean/Beach (X 26000–45000)
            enemies.Add(new Enemy(new Vector2(27000,  400), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(28500,  200), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(29500, -300), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(27800,  600), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(29000, -100), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(32000,  300), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));
            enemies.Add(new Enemy(new Vector2(36000, -200), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));
            enemies.Add(new Enemy(new Vector2(40000,  500), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));

            // Swamp (X -30000 to -55000)
            enemies.Add(new Enemy(new Vector2(-31000,  300), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-34000, -200), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-37000,  500), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-40000,  100), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-43000, -400), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-33000,  600), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-39000, -300), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-46000,  400), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));

            // Volcano (X 26000–55000, Y -2000 to -20000)
            enemies.Add(new Enemy(new Vector2(28000, -3000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(32000, -5000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(37000, -4000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(42000, -6000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(48000, -3500), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(30000, -7000), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));
            enemies.Add(new Enemy(new Vector2(38000, -5500), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));
            enemies.Add(new Enemy(new Vector2(45000, -8000), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));

            // Mountains (X -26000 to -55000, Y -2000 to -20000)
            enemies.Add(new Enemy(new Vector2(-28000, -3000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-32000, -5000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-37000, -4000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-43000, -6000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-49000, -3500), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-30000, -7000), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-38000, -5500), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-46000, -8000), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));

            // Dealer options: specific mapping (horses -> barn, BMX/Mountain -> bike, Sedan/Truck/SUV -> car)
dealerBikeOptions.Clear();
dealerBarnOptions.Clear();
dealerVehicleOptions.Clear();

// Bikes: take rideables that are BMX or MountainBike
var bikeProtos = rideables
    .Where(r => r.Type == Rideable.RideableType.MountainBike || r.Type == Rideable.RideableType.BMX)
    .Take(8)
    .ToList();
foreach (var r in bikeProtos)
    dealerBikeOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));

// Barn: only horses
var horseProtos = rideables
    .Where(r => r.Type == Rideable.RideableType.Horse)
    .Take(8)
    .ToList();
foreach (var h in horseProtos)
    dealerBarnOptions.Add(new Rideable(h.SpawnPosition, h.Type, h.RideableColor));

// Cars: only Sedan, Truck, SUV
var carProtos = vehicles
    .Where(v => v.Type == Vehicle.VehicleType.Sedan
             || v.Type == Vehicle.VehicleType.Truck
             || v.Type == Vehicle.VehicleType.SUV)
    .Take(8)
    .ToList();
foreach (var v in carProtos)
    dealerVehicleOptions.Add(new Vehicle(v.Position, v.VehicleColor, v.TopSpeed, v.Type));


// bikes (mountain + bmx from rideables or create sample rideables)
foreach (var r in rideables.Take(4))
    dealerBikeOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));

// barns (horses)
foreach (var r in rideables.Where(r => r.Type == Rideable.RideableType.Horse).Take(4))
    dealerBarnOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));

// cars (vehicles list)
foreach (var v in vehicles.Take(4))
    dealerVehicleOptions.Add(new Vehicle(v.Position, v.VehicleColor, v.TopSpeed, v.Type) { MaxFuel = v.MaxFuel });

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

    int x = (int)Position.X;
    int y = (int)Position.Y;

    switch (Type)
    {
        case "Wild Dog":
            DrawWildDog(x, y);
            break;
        case "Wolf":
            DrawWolf(x, y);
            break;
        case "Scorpion":
            DrawScorpion(x, y);
            break;
        case "Bear":
            DrawBear(x, y);
            break;
        case "Crab":          DrawCrab(x, y);         break;
        case "Shark":         DrawShark(x, y);        break;
        case "Snake":         DrawSnake(x, y);        break;
        case "Crocodile":     DrawCrocodile(x, y);    break;
        case "Fire Lizard":   DrawFireLizard(x, y);   break;
        case "Magma Beetle":  DrawMagmaBeetle(x, y);  break;
        case "Eagle":         DrawEagle(x, y);        break;
        case "Mountain Goat": DrawMountainGoat(x, y); break;
            }

    // Health bar background
    Raylib.DrawRectangle(x, y - 10, 40, 6, Color.DarkGray);

    // Health bar fill
    float healthPercent = (float)Health / MaxHealth;
    Raylib.DrawRectangle(x, y - 10, (int)(40 * healthPercent), 6, Color.Red);
}

private void DrawWildDog(int x, int y)
{
    Color brown = Color.Brown;
    Color dark = new Color((byte)80, (byte)40, (byte)10, (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 16, 10, brown);

    // Head
    Raylib.DrawCircle(x + 32, y + 18, 9, brown);

    // Snout
    Raylib.DrawEllipse(x + 39, y + 20, 5, 4, dark);

    // Nose
    Raylib.DrawCircle(x + 42, y + 19, 2, Color.Black);

    // Eye
    Raylib.DrawCircle(x + 35, y + 16, 2, Color.Black);
    Raylib.DrawCircle(x + 36, y + 15, 1, Color.White); // shine

    // Ears
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 10),
        new Vector2(x + 24, y + 18),
        new Vector2(x + 32, y + 18),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 35, y + 9),
        new Vector2(x + 31, y + 17),
        new Vector2(x + 38, y + 16),
        dark
    );

    // Legs (4 short rectangles)
    Raylib.DrawRectangle(x + 8,  y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 15, y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 24, y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 31, y + 33, 5, 8, dark);

    // Tail (curved via small circles)
    Raylib.DrawCircle(x + 5,  y + 24, 3, brown);
    Raylib.DrawCircle(x + 2,  y + 20, 3, brown);
    Raylib.DrawCircle(x + 1,  y + 16, 2, brown);
}

private void DrawWolf(int x, int y)
{
    Color gray = Color.DarkGray;
    Color light = new Color((byte)180, (byte)180, (byte)190, (byte)255);
    Color dark = new Color((byte)40, (byte)40, (byte)50, (byte)255);

    // Body — bigger and bulkier than dog
    Raylib.DrawEllipse(x + 20, y + 27, 18, 12, gray);

    // Chest lighter patch
    Raylib.DrawEllipse(x + 28, y + 28, 7, 8, light);

    // Head
    Raylib.DrawCircle(x + 33, y + 17, 11, gray);

    // Snout — longer and more pointed
    Raylib.DrawTriangle(
        new Vector2(x + 33, y + 18),
        new Vector2(x + 33, y + 23),
        new Vector2(x + 46, y + 21),
        dark
    );

    // Nose
    Raylib.DrawCircle(x + 44, y + 20, 2, Color.Black);

    // Eyes — more menacing
    Raylib.DrawCircle(x + 36, y + 14, 2, new Color((byte)200, (byte)200, (byte)0, (byte)255)); // yellow
    Raylib.DrawCircle(x + 36, y + 14, 1, Color.Black);

    // Ears — tall and pointed
    Raylib.DrawTriangle(
        new Vector2(x + 27, y + 7),
        new Vector2(x + 23, y + 18),
        new Vector2(x + 32, y + 17),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 6),
        new Vector2(x + 33, y + 16),
        new Vector2(x + 41, y + 15),
        dark
    );

    // Legs — longer than dog
    Raylib.DrawRectangle(x + 6,  y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 14, y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 24, y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 32, y + 35, 5, 10, dark);

    // Tail
    Raylib.DrawCircle(x + 4,  y + 26, 4, gray);
    Raylib.DrawCircle(x + 1,  y + 21, 3, gray);
    Raylib.DrawCircle(x + 0,  y + 16, 3, light); // white tail tip
}

private void DrawScorpion(int x, int y)
{
    Color shell = new Color((byte)180, (byte)120, (byte)0, (byte)255);
    Color dark  = new Color((byte)100, (byte)60,  (byte)0, (byte)255);
    Color sting = new Color((byte)220, (byte)60,  (byte)0, (byte)255);

    // Body segments
    Raylib.DrawEllipse(x + 20, y + 26, 10, 7, shell);  // abdomen
    Raylib.DrawEllipse(x + 28, y + 24, 7,  6, shell);  // mid
    Raylib.DrawEllipse(x + 34, y + 22, 6,  5, shell);  // thorax

    // Head
    Raylib.DrawCircle(x + 38, y + 20, 6, dark);

    // Eyes (two small dots)
    Raylib.DrawCircle(x + 36, y + 18, 1, Color.Black);
    Raylib.DrawCircle(x + 40, y + 18, 1, Color.Black);

    // Claws (left)
    Raylib.DrawLineEx(new Vector2(x + 14, y + 20), new Vector2(x + 7,  y + 15), 3, dark);
    Raylib.DrawCircle(x + 5,  y + 13, 5, shell);
    Raylib.DrawCircle(x + 9,  y + 10, 3, shell);

    // Claws (right)
    Raylib.DrawLineEx(new Vector2(x + 14, y + 22), new Vector2(x + 6, y + 28), 3, dark);
    Raylib.DrawCircle(x + 4,  y + 30, 5, shell);
    Raylib.DrawCircle(x + 8,  y + 34, 3, shell);

    // Legs (3 per side)
    Raylib.DrawLineEx(new Vector2(x + 18, y + 28), new Vector2(x + 12, y + 22), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 30), new Vector2(x + 13, y + 36), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 23, y + 30), new Vector2(x + 18, y + 38), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 28), new Vector2(x + 32, y + 22), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 24, y + 30), new Vector2(x + 31, y + 36), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 22, y + 30), new Vector2(x + 27, y + 38), 2, dark);

    // Tail segments curving up and over
    Raylib.DrawCircle(x + 13, y + 20, 4, shell);
    Raylib.DrawCircle(x + 9,  y + 15, 4, shell);
    Raylib.DrawCircle(x + 7,  y + 9,  4, shell);
    Raylib.DrawCircle(x + 10, y + 4,  3, shell);

    // Stinger tip
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 1),
        new Vector2(x + 7,  y + 4),
        new Vector2(x + 14, y + 4),
        sting
    );
}

private void DrawBear(int x, int y)
{
    Color fur  = new Color((byte)100, (byte)100, (byte)120, (byte)255); // blue-gray for snow bear
    Color dark = new Color((byte)50,  (byte)50,  (byte)65,  (byte)255);
    Color snout = new Color((byte)140, (byte)130, (byte)140, (byte)255);

    // Large body
    Raylib.DrawEllipse(x + 20, y + 28, 20, 15, fur);

    // Head — large and round
    Raylib.DrawCircle(x + 20, y + 14, 14, fur);

    // Snout
    Raylib.DrawEllipse(x + 20, y + 18, 7, 5, snout);

    // Nose
    Raylib.DrawEllipse(x + 20, y + 14, 4, 3, Color.Black);

    // Eyes
    Raylib.DrawCircle(x + 14, y + 11, 3, Color.Black);
    Raylib.DrawCircle(x + 26, y + 11, 3, Color.Black);
    Raylib.DrawCircle(x + 15, y + 10, 1, Color.White); // shine left
    Raylib.DrawCircle(x + 27, y + 10, 1, Color.White); // shine right

    // Round ears
    Raylib.DrawCircle(x + 10, y + 3, 5, fur);
    Raylib.DrawCircle(x + 30, y + 3, 5, fur);
    Raylib.DrawCircle(x + 10, y + 3, 3, dark); // inner ear
    Raylib.DrawCircle(x + 30, y + 3, 3, dark);

    // Thick legs
    Raylib.DrawRectangleRounded(new Rectangle(x + 4,  y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 14, y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 24, y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 34, y + 36, 9, 12), 0.4f, 4, dark);

    // Claws
    for (int i = 0; i < 3; i++)
    {
        Raylib.DrawLineEx(
            new Vector2(x + 5  + i * 3, y + 48),
            new Vector2(x + 4  + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 15 + i * 3, y + 48),
            new Vector2(x + 14 + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 25 + i * 3, y + 48),
            new Vector2(x + 24 + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 35 + i * 3, y + 48),
            new Vector2(x + 34 + i * 3, y + 52),
            2, Color.White
        );
    }
}
private void DrawCrab(int x, int y)
{
    Color shell = new Color((byte)210, (byte)80,  (byte)30,  (byte)255);
    Color dark  = new Color((byte)140, (byte)40,  (byte)10,  (byte)255);
    Color white = Color.White;

    // Body — wide oval
    Raylib.DrawEllipse(x + 20, y + 24, 16, 10, shell);

    // Eyes on stalks
    Raylib.DrawLineEx(new Vector2(x + 14, y + 16), new Vector2(x + 12, y + 12), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 16), new Vector2(x + 28, y + 12), 2, dark);
    Raylib.DrawCircle(x + 12, y + 11, 3, dark);
    Raylib.DrawCircle(x + 12, y + 11, 1, white);
    Raylib.DrawCircle(x + 28, y + 11, 3, dark);
    Raylib.DrawCircle(x + 28, y + 11, 1, white);

    // Big claws
    Raylib.DrawLineEx(new Vector2(x + 6,  y + 20), new Vector2(x - 4, y + 14), 3, dark);
    Raylib.DrawEllipse(x - 6, y + 12, 6, 4, shell);
    Raylib.DrawEllipse(x - 4, y + 16, 4, 3, shell);

    Raylib.DrawLineEx(new Vector2(x + 34, y + 20), new Vector2(x + 44, y + 14), 3, dark);
    Raylib.DrawEllipse(x + 46, y + 12, 6, 4, shell);
    Raylib.DrawEllipse(x + 44, y + 16, 4, 3, shell);

    // Walking legs (3 per side)
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 26), new Vector2(x + 2,  y + 20), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 28), new Vector2(x + 1,  y + 34), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 10, y + 30), new Vector2(x + 5,  y + 38), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 32, y + 26), new Vector2(x + 38, y + 20), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 32, y + 28), new Vector2(x + 39, y + 34), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 30, y + 30), new Vector2(x + 35, y + 38), 2, dark);
}

private void DrawShark(int x, int y)
{
    Color blue  = new Color((byte)70,  (byte)100, (byte)140, (byte)255);
    Color light = new Color((byte)200, (byte)220, (byte)230, (byte)255);
    Color dark  = new Color((byte)30,  (byte)50,  (byte)80,  (byte)255);

    // Body — long torpedo shape
    Raylib.DrawEllipse(x + 20, y + 22, 20, 10, blue);

    // Belly (lighter underside)
    Raylib.DrawEllipse(x + 20, y + 26, 14, 6, light);

    // Dorsal fin on top
    Raylib.DrawTriangle(
        new Vector2(x + 18, y + 12),
        new Vector2(x + 14, y + 22),
        new Vector2(x + 26, y + 22),
        dark
    );

    // Tail fin
    Raylib.DrawTriangle(
        new Vector2(x + 0,  y + 16),
        new Vector2(x + 8,  y + 22),
        new Vector2(x + 2,  y + 30),
        dark
    );

    // Pectoral fins
    Raylib.DrawTriangle(
        new Vector2(x + 14, y + 24),
        new Vector2(x + 6,  y + 30),
        new Vector2(x + 18, y + 30),
        blue
    );
    Raylib.DrawTriangle(
        new Vector2(x + 26, y + 24),
        new Vector2(x + 34, y + 30),
        new Vector2(x + 22, y + 30),
        blue
    );

    // Eye
    Raylib.DrawCircle(x + 32, y + 20, 3, Color.Black);
    Raylib.DrawCircle(x + 33, y + 19, 1, Color.White);

    // Mouth with teeth
    Raylib.DrawLineEx(new Vector2(x + 36, y + 24), new Vector2(x + 42, y + 22), 2, dark);
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 24),
        new Vector2(x + 39, y + 24),
        new Vector2(x + 38, y + 27),
        Color.White
    );
    Raylib.DrawTriangle(
        new Vector2(x + 40, y + 23),
        new Vector2(x + 42, y + 23),
        new Vector2(x + 41, y + 26),
        Color.White
    );
}

private void DrawSnake(int x, int y)
{
    Color green  = new Color((byte)40,  (byte)100, (byte)40,  (byte)255);
    Color dark   = new Color((byte)20,  (byte)60,  (byte)20,  (byte)255);
    Color yellow = new Color((byte)200, (byte)200, (byte)50,  (byte)255);

    // Body — S-curve using circles
    Raylib.DrawCircle(x + 30, y + 30, 6, green);
    Raylib.DrawCircle(x + 24, y + 26, 6, green);
    Raylib.DrawCircle(x + 18, y + 24, 6, green);
    Raylib.DrawCircle(x + 13, y + 26, 5, green);
    Raylib.DrawCircle(x + 9,  y + 30, 5, green);
    Raylib.DrawCircle(x + 7,  y + 34, 4, green);

    // Scale pattern stripes
    Raylib.DrawCircle(x + 24, y + 26, 3, dark);
    Raylib.DrawCircle(x + 13, y + 26, 3, dark);

    // Tail tip
    Raylib.DrawCircle(x + 5,  y + 37, 2, dark);

    // Head
    Raylib.DrawEllipse(x + 35, y + 28, 7, 5, green);

    // Eyes
    Raylib.DrawCircle(x + 37, y + 26, 2, yellow);
    Raylib.DrawCircle(x + 37, y + 26, 1, Color.Black);

    // Forked tongue
    Raylib.DrawLineEx(new Vector2(x + 41, y + 28), new Vector2(x + 45, y + 26), 1, Color.Red);
    Raylib.DrawLineEx(new Vector2(x + 41, y + 28), new Vector2(x + 45, y + 30), 1, Color.Red);
}

private void DrawCrocodile(int x, int y)
{
    Color green = new Color((byte)50,  (byte)80,  (byte)30,  (byte)255);
    Color dark  = new Color((byte)25,  (byte)45,  (byte)15,  (byte)255);
    Color belly = new Color((byte)120, (byte)140, (byte)80,  (byte)255);

    // Body — long and low
    Raylib.DrawEllipse(x + 20, y + 28, 20, 9, green);

    // Belly
    Raylib.DrawEllipse(x + 20, y + 30, 14, 5, belly);

    // Armored back bumps
    Raylib.DrawCircle(x + 10, y + 22, 4, dark);
    Raylib.DrawCircle(x + 17, y + 21, 4, dark);
    Raylib.DrawCircle(x + 24, y + 21, 4, dark);
    Raylib.DrawCircle(x + 31, y + 22, 3, dark);

    // Head — wide flat snout
    Raylib.DrawEllipse(x + 36, y + 27, 10, 6, green);

    // Teeth
    for (int i = 0; i < 3; i++)
    {
        Raylib.DrawTriangle(
            new Vector2(x + 38 + i * 4, y + 22),
            new Vector2(x + 36 + i * 4, y + 26),
            new Vector2(x + 40 + i * 4, y + 26),
            Color.White
        );
    }

    // Eyes on top of head
    Raylib.DrawCircle(x + 33, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 1, Color.Black);

    // Legs
    Raylib.DrawRectangle(x + 6,  y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 14, y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 22, y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 30, y + 34, 6, 6, dark);

    // Tail
    Raylib.DrawCircle(x + 3,  y + 28, 5, green);
    Raylib.DrawCircle(x - 2, y + 27, 3, green);
}

private void DrawFireLizard(int x, int y)
{
    Color orange = new Color((byte)180, (byte)60,  (byte)10,  (byte)255);
    Color red    = new Color((byte)220, (byte)30,  (byte)0,   (byte)255);
    Color yellow = new Color((byte)255, (byte)200, (byte)0,   (byte)255);
    Color dark   = new Color((byte)80,  (byte)20,  (byte)0,   (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 14, 9, orange);

    // Fire pattern on back
    Raylib.DrawTriangle(
        new Vector2(x + 12, y + 18),
        new Vector2(x + 9,  y + 26),
        new Vector2(x + 15, y + 26),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 20, y + 16),
        new Vector2(x + 17, y + 26),
        new Vector2(x + 23, y + 26),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 18),
        new Vector2(x + 25, y + 26),
        new Vector2(x + 31, y + 26),
        red
    );

    // Head
    Raylib.DrawEllipse(x + 33, y + 22, 8, 6, orange);

    // Spiky crest on head
    Raylib.DrawTriangle(
        new Vector2(x + 30, y + 16),
        new Vector2(x + 28, y + 22),
        new Vector2(x + 33, y + 22),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 35, y + 14),
        new Vector2(x + 32, y + 21),
        new Vector2(x + 38, y + 21),
        yellow
    );

    // Eye
    Raylib.DrawCircle(x + 36, y + 21, 2, yellow);
    Raylib.DrawCircle(x + 36, y + 21, 1, Color.Black);

    // Legs
    Raylib.DrawRectangle(x + 8,  y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 16, y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 24, y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 30, y + 32, 5, 8, dark);

    // Tail with flame tip
    Raylib.DrawCircle(x + 6,  y + 26, 4, orange);
    Raylib.DrawCircle(x + 2,  y + 23, 3, red);
    Raylib.DrawCircle(x + 0,  y + 19, 2, yellow);
}

private void DrawMagmaBeetle(int x, int y)
{
    Color black  = new Color((byte)30,  (byte)15,  (byte)5,   (byte)255);
    Color lava   = new Color((byte)220, (byte)80,  (byte)0,   (byte)255);
    Color glow   = new Color((byte)255, (byte)180, (byte)0,   (byte)255);

    // Shell — large domed oval
    Raylib.DrawEllipse(x + 20, y + 24, 17, 13, black);

    // Lava cracks on shell
    Raylib.DrawLineEx(new Vector2(x + 12, y + 18), new Vector2(x + 20, y + 28), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 18), new Vector2(x + 26, y + 30), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 18), new Vector2(x + 20, y + 28), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 16, y + 28), new Vector2(x + 12, y + 34), 2, lava);

    // Glowing lava spots
    Raylib.DrawCircle(x + 16, y + 22, 3, glow);
    Raylib.DrawCircle(x + 24, y + 20, 2, glow);
    Raylib.DrawCircle(x + 22, y + 30, 3, glow);

    // Head
    Raylib.DrawCircle(x + 34, y + 22, 6, black);

    // Glowing eyes
    Raylib.DrawCircle(x + 32, y + 20, 2, glow);
    Raylib.DrawCircle(x + 37, y + 20, 2, glow);

    // Horn
    Raylib.DrawTriangle(
        new Vector2(x + 38, y + 16),
        new Vector2(x + 35, y + 22),
        new Vector2(x + 41, y + 22),
        lava
    );

    // Legs
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 28), new Vector2(x + 2,  y + 22), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 30), new Vector2(x + 1,  y + 36), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 10, y + 32), new Vector2(x + 4,  y + 38), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 28), new Vector2(x + 34, y + 22), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 30), new Vector2(x + 35, y + 36), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 32), new Vector2(x + 32, y + 38), 3, black);
}

private void DrawEagle(int x, int y)
{
    Color brown  = new Color((byte)100, (byte)70,  (byte)20,  (byte)255);
    Color white  = Color.White;
    Color yellow = new Color((byte)240, (byte)180, (byte)0,   (byte)255);
    Color dark   = new Color((byte)40,  (byte)25,  (byte)5,   (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 12, 9, brown);

    // White head
    Raylib.DrawCircle(x + 30, y + 18, 9, white);

    // Beak
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 18),
        new Vector2(x + 44, y + 20),
        new Vector2(x + 37, y + 22),
        yellow
    );

    // Eye
    Raylib.DrawCircle(x + 33, y + 16, 2, Color.Black);
    Raylib.DrawCircle(x + 34, y + 15, 1, white);

    // Wings spread wide
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 22),
        new Vector2(x - 8,  y + 16),
        new Vector2(x - 4,  y + 30),
        brown
    );
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 22),
        new Vector2(x - 4,  y + 30),
        new Vector2(x + 14, y + 30),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 22),
        new Vector2(x + 48, y + 16),
        new Vector2(x + 44, y + 30),
        brown
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 22),
        new Vector2(x + 44, y + 30),
        new Vector2(x + 24, y + 30),
        dark
    );

    // Talons
    Raylib.DrawLineEx(new Vector2(x + 16, y + 34), new Vector2(x + 12, y + 40), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 18, y + 34), new Vector2(x + 16, y + 41), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 34), new Vector2(x + 20, y + 41), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 22, y + 34), new Vector2(x + 24, y + 41), 2, yellow);
}

private void DrawMountainGoat(int x, int y)
{
    Color white = new Color((byte)200, (byte)195, (byte)185, (byte)255);
    Color dark  = new Color((byte)80,  (byte)75,  (byte)65,  (byte)255);
    Color horn  = new Color((byte)60,  (byte)45,  (byte)20,  (byte)255);

    // Body — stocky
    Raylib.DrawEllipse(x + 20, y + 26, 16, 11, white);

    // Shaggy chest tuft
    Raylib.DrawEllipse(x + 30, y + 30, 7, 9, white);

    // Head
    Raylib.DrawCircle(x + 33, y + 17, 9, white);

    // Snout
    Raylib.DrawEllipse(x + 39, y + 19, 5, 4, dark);
    Raylib.DrawCircle(x + 42, y + 18, 2, Color.Black);

    // Eye
    Raylib.DrawCircle(x + 35, y + 14, 2, Color.Black);
    Raylib.DrawCircle(x + 36, y + 13, 1, Color.White);

    // Curved horns
    Raylib.DrawLineEx(new Vector2(x + 30, y + 9),  new Vector2(x + 24, y + 4),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 24, y + 4),  new Vector2(x + 20, y + 8),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 36, y + 8),  new Vector2(x + 42, y + 3),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 42, y + 3),  new Vector2(x + 46, y + 8),  3, horn);

    // Beard
    Raylib.DrawEllipse(x + 37, y + 25, 3, 5, dark);

    // Sturdy legs
    Raylib.DrawRectangle(x + 7,  y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 15, y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 23, y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 31, y + 34, 6, 10, dark);

    // Hooves
    Raylib.DrawRectangle(x + 7,  y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 15, y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 23, y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 31, y + 43, 6, 3, Color.Black);
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
        float bobOffset = 0f;
        bool walkFrame = false; // alternates legs
        bool isMoving = false;
        float chopAnimAngle = 0f;
        bool isChopping = false;
        bool isMining = false;

        public bool Hidden = false;
        public bool HasAxe = false;

        public int WoodcuttingLevel = 1;
        public int FishingLevel = 1;

        public int WoodcuttingXP = 0;
        public int FishingXP = 0;
        public int MiningLevel = 1;
        public int MiningXP = 0;
        public int StoneOre = 0;
        public int CopperOre = 0;
        public int IronOre = 0;
        public int GoldOre = 0;
        public int Crystals = 0;

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
        public int DogFangs = 0;
        public int Health = 100;
        public int MaxHealth = 100;
        public int CombatLevel = 1;
        public int CombatXP = 0;
        public int DrivingLevel = 1;
        public int DrivingXP = 0;
        public int AthleticsLevel = 1;
        public int AthleticsXP = 0;
        public int CyclingLevel = 1;
        public int CyclingXP = 0;
        public int RidingLevel = 1;
        public int RidingXP = 0;
        public int DefenceLevel = 1;
        public int DefenceXP = 0;
        public int HitpointsLevel = 1;
        public int HitpointsXP = 0;
        public int StaminaLevel = 1;
        public int StaminaXP = 0;
        public int SwimmingLevel = 1;
        public int SwimmingXP = 0;
        public int RangeLevel = 1;
        public int RangeXP = 0;
        public int FarmingLevel = 1;
        public int FarmingXP = 0;
        public int SportsLevel = 1;
        public int SportsXP = 0;
        public int SpiritualLevel = 1;
        public int SpiritualXP = 0;
        public int CookingLevel = 1;
        public int CookingXP = 0;
        public int DivingLevel = 1;
        public int DivingXP = 0;
        public int ElementalLevel = 1;
        public int ElementalXP = 0;
        public int AlchemistLevel = 1;
        public int AlchemistXP = 0;
        public int DarkArtsLevel = 1;
        public int DarkArtsXP = 0;
        public int EnchantmentLevel = 1;
        public int EnchanmenttXP = 0;
        public int MysticalLevel = 1;
        public int MysticalXP = 0;   
        public int StrengthLevel = 1;
        public int StrengthXP = 0;
        public bool HasTrolley = false;
        public bool HasBasket = false;
        public float BaseSpeed => 300 + (AthleticsLevel * 2);
        float speed => HasTrolley ? BaseSpeed * 0.65f : BaseSpeed;
        float regenTimer = 0f;
        float damageCooldown = 0f;
        public Color ShirtColor = Color.Blue;
        public Color SkinColor = Color.Beige;
        public Color PantsColor = Color.Black;
        public bool InventoryOpen = false;
        public int DrunkLevel = 0;
        public float DrunkTimer = 0f;
        public float DrunkSpeedMultiplier => DrunkLevel == 0 ? 1f : Math.Max(0.3f, 1f - (DrunkLevel * 0.15f));
        List<(Vector2 pos, float timer, Color color)> dustParticles = new();
        public Color BootsColor => Program.armorBoots != null
        ? new Color((byte)120, (byte)80, (byte)30, (byte)255)  // leather brown default
        : new Color((byte)0, (byte)0, (byte)0, (byte)0);       // transparent = no boots
        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 position)
        {
            Position = position;
        }
        
     void DrawPickaxeSwingLeft(int x, int y)
{
    float swingAngle = chopAnimAngle * MathF.PI / 180f;

    // pivot at hand — shifted further out from body
    Vector2 pivot = new Vector2(x + 8, y + 36);

    float handleLength = 26f;

    Vector2 handleEnd = new Vector2(
        pivot.X - MathF.Sin(swingAngle) * handleLength,
        pivot.Y - MathF.Cos(swingAngle) * handleLength
    );

    // draw handle
    Raylib.DrawLineEx(pivot, handleEnd, 5,
        new Color((byte)120, (byte)80, (byte)30, (byte)255));

    // head bar — perpendicular to handle, spread 12px each side
    float perpAngle = swingAngle + MathF.PI / 2f;
    Vector2 headTop = new Vector2(
        handleEnd.X + MathF.Cos(perpAngle) * 12f,
        handleEnd.Y + MathF.Sin(perpAngle) * 12f
    );
    Vector2 headBot = new Vector2(
        handleEnd.X - MathF.Cos(perpAngle) * 12f,
        handleEnd.Y - MathF.Sin(perpAngle) * 12f
    );

    Raylib.DrawLineEx(headTop, headBot, 4,
        new Color((byte)160, (byte)160, (byte)170, (byte)255));

    // spike direction — points away from player (same dir as handle)
    Vector2 spikeDir = new Vector2(
        -(handleEnd.X - pivot.X),
        -(handleEnd.Y - pivot.Y)
    );
    // normalize
    float len = MathF.Sqrt(spikeDir.X * spikeDir.X + spikeDir.Y * spikeDir.Y);
    spikeDir = new Vector2(spikeDir.X / len, spikeDir.Y / len);

    // bottom spike tip
    Vector2 botSpikeTip = new Vector2(
        headBot.X + spikeDir.X * 10f,
        headBot.Y + spikeDir.Y * 10f
    );

    // top spike tip (shorter, points back)
    Vector2 topSpikeTip = new Vector2(
        headTop.X - spikeDir.X * 8f,
        headTop.Y - spikeDir.Y * 8f
    );

    // bottom spike triangle — wide enough to be visible (4px base)
    Raylib.DrawTriangle(
        new Vector2(headBot.X + MathF.Cos(perpAngle) * 3f, headBot.Y + MathF.Sin(perpAngle) * 3f),
        new Vector2(headBot.X - MathF.Cos(perpAngle) * 3f, headBot.Y - MathF.Sin(perpAngle) * 3f),
        botSpikeTip,
        new Color((byte)160, (byte)160, (byte)170, (byte)255));

    // top spike triangle
    Raylib.DrawTriangle(
        new Vector2(headTop.X + MathF.Cos(perpAngle) * 3f, headTop.Y + MathF.Sin(perpAngle) * 3f),
        new Vector2(headTop.X - MathF.Cos(perpAngle) * 3f, headTop.Y - MathF.Sin(perpAngle) * 3f),
        topSpikeTip,
        new Color((byte)160, (byte)160, (byte)170, (byte)255));
}

    void DrawPickaxeSwingRight(int x, int y)
{
    // chopAnimAngle goes 0 -> 360, we want pickaxe to start pointing up
    // then swing down through 180 degrees
    float swingAngle = chopAnimAngle * MathF.PI / 300f;

    // pivot point where hand holds the pickaxe
    Vector2 pivot = new Vector2(x + 25, y + 40);

    // handle direction — starts pointing up (angle 0 = up)
    float handleLength = 22f;
    float headOffset   = 8f;

    // calculate handle end point from pivot using angle
    Vector2 handleEnd = new Vector2(
        pivot.X + MathF.Sin(swingAngle) * handleLength,
        pivot.Y - MathF.Cos(swingAngle) * handleLength
    );

    // draw handle
    Raylib.DrawLineEx(pivot, handleEnd, 5,
        new Color((byte)120, (byte)80, (byte)30, (byte)255));

    // pickaxe head perpendicular to handle at the end
    float perpAngle = swingAngle + MathF.PI / 10f;
    Vector2 headLeft = new Vector2(
        handleEnd.X - MathF.Cos(perpAngle) * headOffset,
        handleEnd.Y - MathF.Sin(perpAngle) * headOffset
    );
    Vector2 headRight = new Vector2(
        handleEnd.X + MathF.Cos(perpAngle) * headOffset,
        handleEnd.Y + MathF.Sin(perpAngle) * headOffset
    );

    // draw pickaxe head bar
    Raylib.DrawLineEx(headLeft, headRight, 3,
        new Color((byte)160, (byte)160, (byte)170, (byte)255));

    // left spike (curves down)
    Vector2 leftSpike = new Vector2(
        headLeft.X - MathF.Sin(swingAngle) * 10f,
        headLeft.Y + MathF.Cos(swingAngle) * 10f
    );
    Raylib.DrawLineEx(headLeft, leftSpike, 2,
        new Color((byte)160, (byte)160, (byte)170, (byte)255));

    // right spike
    Vector2 rightSpike = new Vector2(
        headRight.X - MathF.Sin(swingAngle) * 10f,
        headRight.Y + MathF.Cos(swingAngle) * 10f
    );
    Raylib.DrawLineEx(headRight, rightSpike, 2,
        new Color((byte)140, (byte)140, (byte)150, (byte)255));
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

        // in Player.Update(), after the buildings loop:
foreach (Rectangle fenceRect in Program.fenceManager.GetCollisionRects())
{
    if (Raylib.CheckCollisionRecs(Bounds, fenceRect))
    {
        Position = oldPos;
        break;
    }
}
foreach (RockObject rock in Program.rocks)
{
    if (!rock.Broken && Raylib.CheckCollisionRecs(Bounds, rock.Bounds))
        Position = oldPos;
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


        dustParticles.Add((
            new Vector2(Position.X + 20, Position.Y + 66),
            0.3f,
            new Color((byte)Raylib.GetRandomValue(100, 150),
                      (byte)Raylib.GetRandomValue(100, 150),
                      (byte)Raylib.GetRandomValue(50, 80),
                      (byte)180)
        ));

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
float dt = Raylib.GetFrameTime();
    // update dust particles
for (int i = dustParticles.Count - 1; i >= 0; i--)
{
    var p = dustParticles[i];
    p.timer -= dt;
    dustParticles[i] = p;
    if (p.timer <= 0) dustParticles.RemoveAt(i);
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
        public void AddMiningXP(int xp)
        {
            if (MiningLevel >= 100) return;
            MiningXP += xp;
            int requiredXP = MiningLevel * MiningLevel * 50;
            if (MiningXP >= requiredXP)
            {
                MiningXP = 0;
                MiningLevel++;
                Program.ShowLevelUp("Mining", MiningLevel);
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
public void TriggerChopAnim()  { isChopping = true; chopAnimAngle = 0f; }
public void TriggerMineAnim()  { isMining  = true; chopAnimAngle = 0f; }


   public void Draw()
{
    if (Hidden) return;

    // update walk animation timer
    if (isMoving)
    {
        // dust particles
    foreach (var p in dustParticles)
    {
        byte alpha = (byte)(255 * (p.timer / 0.3f));
        float size = 4f * (p.timer / 0.3f);
        Raylib.DrawCircle((int)p.pos.X + Raylib.GetRandomValue(-4, 4),
                          (int)p.pos.Y,
                          size,
                          new Color(p.color.R, p.color.G, p.color.B, alpha));
    }
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

    // drive chop/mine swing animation
if (isChopping || isMining)
{
    chopAnimAngle += Raylib.GetFrameTime() * 800f;
    if (chopAnimAngle > 360f) { chopAnimAngle = 0f; isChopping = false; isMining = false; }
}

// bob the player position during swing
float chopBob = (isChopping || isMining)
    ? MathF.Sin(chopAnimAngle * MathF.PI / 180f) * 4f
    : 0f;

int x = (int)Position.X;
int y = (int)(Position.Y + chopBob);

    switch (Facing)
{
    case FacingDirection.Down:
        DrawFacingDown(x, y);
        DrawHeldItem(x, y, Program.GetEquippedTool());
        break;
    case FacingDirection.Up:
        DrawFacingUp(x, y);
        DrawHeldItem(x, y, Program.GetEquippedTool());
        break;
    case FacingDirection.Left:
        DrawFacingLeft(x, y);
        DrawHeldItem(x, y, Program.GetEquippedTool());
        break;
    case FacingDirection.Right:
        DrawFacingRight(x, y);
        DrawHeldItem(x, y, Program.GetEquippedTool());
        break;
    }
    // draw held item on top of player
    if (HasBasket)
        DrawHeldBasket(x, y);
    else if (HasTrolley)
        DrawPushedTrolley(x, y);
}

void DrawFacingDown(int x, int y)
{
    // head
    Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);

    // eyes
    Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
    Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);

        if (Program.armorHelmet != null)
{
    Color hc = new Color((byte)120, (byte)80, (byte)30, (byte)255);
    if (Program.armorHelmet.Contains("Iron"))  hc = new Color((byte)120, (byte)120, (byte)130, (byte)255);
    if (Program.armorHelmet.Contains("Steel")) hc = new Color((byte)160, (byte)165, (byte)175, (byte)255);

    // dome over top of head
    Raylib.DrawRectangle(x + 10, y + 2, 20, 10, hc);
    // visor brim
    Raylib.DrawRectangle(x + 8, y + 11, 24, 4, hc);
    // cheek guards
    Raylib.DrawRectangle(x + 8,  y + 10, 5, 8, hc);
    Raylib.DrawRectangle(x + 27, y + 10, 5, 8, hc);
}

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
    if (Program.armorBoots != null)
{
    Color bc = new Color((byte)100, (byte)65, (byte)25, (byte)255);
    if (Program.armorBoots.Contains("Iron")) bc = new Color((byte)120, (byte)120, (byte)130, (byte)255);
    if (Program.armorBoots.Contains("Steel")) bc = new Color((byte)160, (byte)165, (byte)175, (byte)255);

    // left boot
    Raylib.DrawRectangle(x + 9, y + 63, 10, 7, bc);
    // right boot
    Raylib.DrawRectangle(x + 21, y + 63, 10, 7, bc);
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
    if (Program.armorBoots != null)
{
    Color bc = new Color((byte)100, (byte)65, (byte)25, (byte)255);
    if (Program.armorBoots.Contains("Iron"))  bc = new Color((byte)120, (byte)120, (byte)130, (byte)255);
    if (Program.armorBoots.Contains("Steel")) bc = new Color((byte)160, (byte)165, (byte)175, (byte)255);

    if (isMoving)
    {
        if (walkFrame)
        {
            // left leg forward — boot at bottom of forward leg
            Raylib.DrawRectangle(x + 6,  y + 56 + 8,  10, 6, bc); // forward foot
            Raylib.DrawRectangle(x + 14, y + 56 - 8 + 8, 10, 6, bc); // back foot (raised)
        }
        else
        {
            // legs swapped
            Raylib.DrawRectangle(x + 6,  y + 56 - 8 + 8, 10, 6, bc); // back foot (raised)
            Raylib.DrawRectangle(x + 14, y + 56 + 8,     10, 6, bc); // forward foot
        }
    }
    else
    {
        // standing still — boots sit flat at bottom of each leg
        Raylib.DrawRectangle(x + 6,  y + 62, 10, 6, bc);
        Raylib.DrawRectangle(x + 14, y + 62, 10, 6, bc);
    }
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

void DrawHeldItem(int x, int y, string tool)
{
    if (tool == null) return;

    switch (Facing)
    {
        case FacingDirection.Right:
        int armSwingRight = isMoving ? (walkFrame ? 3 : -3) : 0; 
            switch (tool)
            {
                case "Axe":
                    Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 41 , y + 40 + armSwingRight), new Vector2(x + 41, y + 48 + armSwingRight), 6,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    break;
                case "Pickaxe":
                if (isMining)
                    DrawPickaxeSwingRight(x, y);
                    else
                    {
                    Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 45, y + 34 + armSwingRight), new Vector2(x + 45, y + 44 + armSwingRight), 3,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 46, y + 44 + armSwingRight),
                        new Vector2(x + 43, y + 44 + armSwingRight),
                        new Vector2(x + 37, y + 52 + armSwingRight),
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 43, y + 34 + armSwingRight),
                        new Vector2(x + 46, y + 34 + armSwingRight),
                        new Vector2(x + 40, y + 30 + armSwingRight),
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    }
                    break;
                case "Stick":
                    Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    break;
                case "Sword":
                    Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 35, y + 40 + armSwingRight), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 40 + armSwingRight), new Vector2(x + 48, y + 40 + armSwingRight), 4,
                        new Color((byte)180,(byte)190,(byte)200,(byte)255));
                    Raylib.DrawTriangle(
                    new Vector2(x + 48, y + 42 + armSwingRight),  
                    new Vector2(x + 53, y + 40 + armSwingRight),  
                    new Vector2(x + 48, y + 38 + armSwingRight),  
                    new Color((byte)200,(byte)210,(byte)220,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 34, y + 36 + armSwingRight), new Vector2(x + 34, y + 44 + armSwingRight), 5,
                        new Color((byte)180,(byte)140,(byte)40,(byte)255));
                    break;
                case "Rod":
                    Raylib.DrawLineEx(new Vector2(x + 26, y + 40 + armSwingRight), new Vector2(x + 46, y + 26 + armSwingRight), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 46, y + 26 + armSwingRight), new Vector2(x + 62, y + 40 + armSwingRight), 2,
                        new Color((byte)200,(byte)200,(byte)200,(byte)255));
                    Raylib.DrawCircle(x + 62, y + 40 + armSwingRight, 3,
                        new Color((byte)100,(byte)180,(byte)220,(byte)255));
                    break;
                case "Torch":
                    Raylib.DrawLineEx(new Vector2(x + 36, y + 30 + armSwingRight), new Vector2(x + 50, y + 18 + armSwingRight), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawCircle(x + 50, y + 16 + armSwingRight, 5,
                        new Color((byte)255,(byte)180,(byte)0,(byte)255));
                    Raylib.DrawCircle(x + 50, y + 16 + armSwingRight, 9,
                        new Color((byte)255,(byte)100,(byte)0,(byte)60));
                    break;
            }
            break;

        case FacingDirection.Left:
            int armSwingLeft = isMoving ? (walkFrame ? 3 : -3) : 0;
            switch (tool)
            {
                case "Axe":
                    Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x - 1 , y + 40 + armSwingLeft), new Vector2(x - 1, y + 48 + armSwingLeft), 6,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    break;
                case "Pickaxe":
                if (isMining)
                    DrawPickaxeSwingLeft(x, y);
                    else
                    {
                    Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x - 5, y + 34 + armSwingLeft), new Vector2(x - 5, y + 44 + armSwingLeft), 3,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x - 3, y + 44 + armSwingLeft),
                        new Vector2(x - 6, y + 44 + armSwingLeft),
                        new Vector2(x + 3, y + 52 + armSwingLeft),
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x - 3, y + 34 + armSwingLeft),
                        new Vector2(x, y + 30 + armSwingLeft),
                        new Vector2(x - 6, y + 34 + armSwingLeft),  
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    }
                    break;
                case "Stick":
                    Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    break;
                case "Sword":
                    Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x + 5, y + 40 + armSwingLeft), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 40 + armSwingLeft), new Vector2(x - 8, y + 40 + armSwingLeft), 4,
                        new Color((byte)180,(byte)190,(byte)200,(byte)255));
                    Raylib.DrawTriangle(
                    new Vector2(x - 12, y + 40 + armSwingLeft),  
                    new Vector2(x - 8, y + 42 + armSwingLeft),  
                    new Vector2(x - 8, y + 38 + armSwingLeft),  
                    new Color((byte)200,(byte)210,(byte)220,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 6, y + 36 + armSwingLeft ), new Vector2(x + 6, y + 44 + armSwingLeft), 5,
                        new Color((byte)180,(byte)140,(byte)40,(byte)255));
                    break;
                case "Rod":
                    Raylib.DrawLineEx(new Vector2(x + 14, y + 40 + armSwingLeft), new Vector2(x - 6, y + 26 + armSwingLeft), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x - 6, y + 26 + armSwingLeft), new Vector2(x - 22, y + 40 + armSwingLeft), 2,
                        new Color((byte)200,(byte)200,(byte)200,(byte)255));
                    Raylib.DrawCircle(x - 22, y + 40 + armSwingLeft, 3,
                        new Color((byte)100,(byte)180,(byte)220,(byte)255));
                    break;
                case "Torch":
                    Raylib.DrawLineEx(new Vector2(x + 4, y + 30 + armSwingLeft), new Vector2(x - 10, y + 18 + armSwingLeft), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawCircle(x - 10, y + 16 + armSwingLeft, 5,
                        new Color((byte)255,(byte)180,(byte)0,(byte)255));
                    Raylib.DrawCircle(x - 10, y + 16 + armSwingLeft, 9,
                        new Color((byte)255,(byte)100,(byte)0,(byte)60));
                    break;
            }
            break;

        case FacingDirection.Down:
        int armSwingDown = isMoving ? (walkFrame ? -3 : 3) : 0; 
            switch (tool)
            {
                case "Axe":
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 35 , y + 15 + armSwingDown), new Vector2(x + 35, y + 21 + armSwingDown), 3,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    break;
                case "Pickaxe":
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 14 + armSwingDown), new Vector2(x + 35, y + 18 + armSwingDown), 5,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 37, y + 18 + armSwingDown),
                        new Vector2(x + 33, y + 18 + armSwingDown),
                        new Vector2(x + 35, y + 28 + armSwingDown),
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    
                    break;
                case "Stick":
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    break;
                case "Sword":
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 33 + armSwingDown), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 33 + armSwingDown), new Vector2(x + 35, y + 18 + armSwingDown), 3,
                        new Color((byte)180,(byte)190,(byte)200,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 34, y + 18 + armSwingDown),
                        new Vector2(x + 36, y + 18 + armSwingDown),
                        new Vector2(x + 35, y + 10 + armSwingDown),
                        new Color((byte)200,(byte)210,(byte)220,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 32, y + 33 + armSwingDown), new Vector2(x + 38, y + 33 + armSwingDown), 4,
                        new Color((byte)180,(byte)140,(byte)40,(byte)255));
                    break;
                case "Rod":
                    Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 35 , y + 15 + armSwingDown), new Vector2(x + 35, y + 40 + armSwingDown), 2,
                        new Color((byte)200,(byte)200,(byte)200,(byte)255));
                    Raylib.DrawCircle(x + 35, y + 40 + armSwingDown, 3,
                        new Color((byte)100,(byte)180,(byte)220,(byte)255));
                    break;
                case "Torch":
                    Raylib.DrawLineEx(new Vector2(x + 30, y + 28), new Vector2(x + 44, y + 16), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawCircle(x + 44, y + 14, 5,
                        new Color((byte)255,(byte)180,(byte)0,(byte)255));
                    Raylib.DrawCircle(x + 44, y + 14, 9,
                        new Color((byte)255,(byte)100,(byte)0,(byte)60));
                    break;
            }
            break;

        case FacingDirection.Up:
        int armSwingUp = isMoving ? (walkFrame ? 3 : -3) : 0; // + armSwingUp
            switch (tool)
            {
                case "Axe":
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 15 + armSwingUp), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 15 + armSwingUp), new Vector2(x + 5, y + 18 + armSwingUp), 4,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    break;
                case "Pickaxe":
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 15 + armSwingUp), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 15 + armSwingUp), new Vector2(x + 5, y + 12 + armSwingUp), 4,
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 7, y + 15 + armSwingUp),
                        new Vector2(x + 3, y + 15 + armSwingUp),
                        new Vector2(x + 5, y + 22 + armSwingUp),
                        new Color((byte)160,(byte)160,(byte)170,(byte)255));
                    break;
                case "Stick":
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    break;
                case "Sword":
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4,
                        new Color((byte)180,(byte)190,(byte)200,(byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(x + 3, y + 14 + armSwingUp),
                        new Vector2(x + 7, y + 14 + armSwingUp),
                        new Vector2(x + 5, y + 10 + armSwingUp),
                        new Color((byte)200,(byte)210,(byte)220,(byte)255));
                    break;
                case "Rod":
                    Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawLineEx(new Vector2(x + 7, y + 14 + armSwingUp), new Vector2(x + 7, y + 26 + armSwingUp), 1,
                        new Color((byte)200,(byte)200,(byte)200,(byte)255));
                    Raylib.DrawCircle(x + 5, y + 48 + armSwingUp, 3,
                        new Color((byte)100,(byte)180,(byte)220,(byte)255));
                    break;
                case "Torch":
                    Raylib.DrawLineEx(new Vector2(x + 10, y + 30), new Vector2(x - 4, y + 18), 5,
                        new Color((byte)120,(byte)80,(byte)30,(byte)255));
                    Raylib.DrawCircle(x - 4, y + 16, 5,
                        new Color((byte)255,(byte)180,(byte)0,(byte)255));
                    Raylib.DrawCircle(x - 4, y + 16, 9,
                        new Color((byte)255,(byte)100,(byte)0,(byte)60));
                    break;
            }
            break;
    }
}
void DrawHeldBasket(int x, int y)
{
    // basket appears on the player's side depending on facing
    int bx, by;
    foreach (var p in dustParticles)
    {
        byte alpha = (byte)(255 * (p.timer / 0.3f));
        float size = 4f * (p.timer / 0.3f);
        Raylib.DrawCircle((int)p.pos.X + Raylib.GetRandomValue(-4, 4),
                          (int)p.pos.Y,
                          size,
                          new Color(p.color.R, p.color.G, p.color.B, alpha));
    }
    switch (Facing)
    {
        case FacingDirection.Down:
            bx = x + 28; by = y + 28;
            // basket body
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Up:
            bx = x + 28; by = y + 28;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Left:
            bx = x - 20; by = y + 30;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Right:
            bx = x + 38; by = y + 30;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
    }
}

void DrawPushedTrolley(int x, int y)
{
    int tx, ty;
    switch (Facing)
    {
        case FacingDirection.Down:
            tx = x - 10; ty = y + 65;
            // trolley body
            Raylib.DrawRectangle(tx, ty, 60, 32, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 60, 32, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            // mesh
            for (int i = tx + 10; i < tx + 60; i += 10)
                Raylib.DrawRectangle(i, ty, 2, 32, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 8; j < ty + 32; j += 8)
                Raylib.DrawRectangle(tx, j, 60, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            // handle connecting to player
            Raylib.DrawRectangle(tx + 10, ty - 10, 40, 8, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            // wheels
            Raylib.DrawCircle(tx + 12, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 48, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Up:
            tx = x - 10; ty = y - 50;
            Raylib.DrawRectangle(tx, ty, 60, 32, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 60, 32, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 10; i < tx + 60; i += 10)
                Raylib.DrawRectangle(i, ty, 2, 32, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 8; j < ty + 32; j += 8)
                Raylib.DrawRectangle(tx, j, 60, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx + 10, ty + 32, 40, 8, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx + 12, ty - 6, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 48, ty - 6, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Left:
            tx = x - 65; ty = y + 10;
            Raylib.DrawRectangle(tx, ty, 32, 45, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 32, 45, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 8; i < tx + 32; i += 8)
                Raylib.DrawRectangle(i, ty, 2, 45, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 10; j < ty + 45; j += 10)
                Raylib.DrawRectangle(tx, j, 32, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx + 32, ty + 12, 8, 22, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx - 6, ty + 10, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx - 6, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Right:
            tx = x + 40; ty = y + 10;
            Raylib.DrawRectangle(tx, ty, 32, 45, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 32, 45, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 8; i < tx + 32; i += 8)
                Raylib.DrawRectangle(i, ty, 2, 45, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 10; j < ty + 45; j += 10)
                Raylib.DrawRectangle(tx, j, 32, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx - 8, ty + 12, 8, 22, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx + 38, ty + 10, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 38, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;
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
    public float ChopAnimTimer = 0f;
    public bool IsBeingChopped = false;

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
        if (IsBeingChopped)
{
    ChopAnimTimer += dt * 10f;
    if (ChopAnimTimer >= 1f) { ChopAnimTimer = 0f; IsBeingChopped = false; }
}
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
                Raylib.DrawRectangle((int)Position.X + 20, (int)Position.Y + 40, 20, 40, Color.DarkBrown);
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

        // crack overlay based on damage taken
if (Health < MaxHealth)
{
    float damagePercent = 1f - ((float)Health / MaxHealth);

    int tx = (int)Position.X + 30; // centre of tree trunk
    int ty = (int)Position.Y + 50;

    // get equipped tool to determine chop mark style
    string equippedTool = Program.GetEquippedTool();
    Color chopColor = new Color((byte)60, (byte)30, (byte)10, (byte)220);  // default wood colour
    float chopWidth = 2f;

    if (equippedTool != null && equippedTool.Contains("Iron"))
    {
        chopColor = new Color((byte)40, (byte)20, (byte)5, (byte)240);
        chopWidth = 3f;  // deeper cuts
    }
    else if (equippedTool != null && equippedTool.Contains("Steel"))
    {
        chopColor = new Color((byte)25, (byte)12, (byte)3, (byte)255);
        chopWidth = 4f;  // even deeper
    }
    else if (equippedTool != null && equippedTool.Contains("Gold"))
    {
        chopColor = new Color((byte)20, (byte)10, (byte)2, (byte)255);
        chopWidth = 5f;  // deepest cuts
    }

    // first chop mark — any damage
    if (damagePercent > 0f)
    {
        // V shaped chop mark on trunk
        Raylib.DrawLineEx(
            new Vector2(tx - 8, ty - 4),
            new Vector2(tx,     ty + 4),
            chopWidth, chopColor);
        Raylib.DrawLineEx(
            new Vector2(tx,     ty + 4),
            new Vector2(tx + 8, ty - 4),
            chopWidth, chopColor);
        // wood chip flying off
        Raylib.DrawCircle(tx - 12, ty - 8, 3,
            new Color((byte)120, (byte)80, (byte)30, (byte)200));
        Raylib.DrawCircle(tx - 14, ty - 12, 2,
            new Color((byte)100, (byte)60, (byte)20, (byte)180));
    }

    // second chop mark at 40% damage — deeper cut
    if (damagePercent > 0.4f)
    {
        // second V cut slightly above first
        Raylib.DrawLineEx(
            new Vector2(tx - 7, ty - 14),
            new Vector2(tx,     ty - 6),
            chopWidth, chopColor);
        Raylib.DrawLineEx(
            new Vector2(tx,     ty - 6),
            new Vector2(tx + 7, ty - 14),
            chopWidth, chopColor);
        // exposed wood grain inside cut
        Raylib.DrawRectangle(tx - 4, ty - 10, 8, 6,
            new Color((byte)180, (byte)130, (byte)70, (byte)180));
        // more chips
        Raylib.DrawCircle(tx + 14, ty - 10, 3,
            new Color((byte)120, (byte)80, (byte)30, (byte)200));
        Raylib.DrawCircle(tx + 16, ty - 16, 2,
            new Color((byte)100, (byte)60, (byte)20, (byte)180));

        // bark cracking up the trunk
        Raylib.DrawLineEx(
            new Vector2(tx - 2, ty - 20),
            new Vector2(tx + 4, ty - 10),
            2f, new Color((byte)40, (byte)20, (byte)5, (byte)200));
    }

    // near breaking at 66% damage — trunk splitting
    if (damagePercent > 0.66f)
    {
        // deep split running up through trunk
        Raylib.DrawLineEx(
            new Vector2(tx,     ty - 30),
            new Vector2(tx - 4, ty),
            chopWidth + 1f, new Color((byte)15, (byte)8, (byte)2, (byte)255));

        // large exposed wood patch
        Raylib.DrawRectangle(tx - 6, ty - 18, 12, 14,
            new Color((byte)200, (byte)150, (byte)80, (byte)200));
        Raylib.DrawRectangle(tx - 4, ty - 16, 8, 10,
            new Color((byte)220, (byte)170, (byte)90, (byte)180));

        // bark chunks falling
        Raylib.DrawRectangle(tx - 18, ty - 20, 6, 8,
            new Color((byte)100, (byte)60, (byte)20, (byte)220));
        Raylib.DrawRectangle(tx + 12, ty - 14, 5, 7,
            new Color((byte)100, (byte)60, (byte)20, (byte)220));
        Raylib.DrawCircle(tx - 20, ty - 28, 4,
            new Color((byte)80, (byte)50, (byte)15, (byte)200));
        Raylib.DrawCircle(tx + 18, ty - 22, 3,
            new Color((byte)80, (byte)50, (byte)15, (byte)200));

        // canopy shaking — draw some loose leaves falling
        Raylib.DrawCircle(tx - 24, ty - 40, 5,
            new Color((byte)0, (byte)100, (byte)0, (byte)160));
        Raylib.DrawCircle(tx + 20, ty - 38, 4,
            new Color((byte)0, (byte)120, (byte)0, (byte)140));
        Raylib.DrawCircle(tx - 10, ty - 50, 4,
            new Color((byte)0, (byte)110, (byte)0, (byte)150));
    }
}

        // level requirement label
       // if (LevelRequired > 1)
       // {
      //      Raylib.DrawText($"WC {LevelRequired}", (int)Position.X + 5, (int)Position.Y - 18, 16, Color.Yellow);
      // }
    }
}
class RockObject
{
    public Vector2 Position;
    public bool Broken = false;
    public int Health;
    public int MaxHealth;
    public string RockType;
    public int LevelRequired;
    public int XPReward;
    public string OreType;
    public float MineAnimTimer = 0f;
    public bool IsBeingMined = false;
    float respawnTimer = 0f;

    public Rectangle Bounds =>
        new Rectangle(Position.X - 20, Position.Y - 20, 40, 40);

    public static RockObject Stone(Vector2 pos)  => new RockObject(pos, "Stone",  3, 1,  20, "Stone");
    public static RockObject Copper(Vector2 pos) => new RockObject(pos, "Copper", 4, 3,  40, "Copper Ore");
    public static RockObject Iron(Vector2 pos)   => new RockObject(pos, "Iron",   5, 5,  60, "Iron Ore");
    public static RockObject Gold(Vector2 pos)   => new RockObject(pos, "Gold",   6, 10, 100, "Gold Ore");
    public static RockObject Crystal(Vector2 pos)=> new RockObject(pos, "Crystal",7, 15, 140, "Crystal");

    public RockObject(Vector2 pos, string rockType, int health, int levelRequired, int xpReward, string oreType)
    {
        Position = pos;
        RockType = rockType;
        Health = health;
        MaxHealth = health;
        LevelRequired = levelRequired;
        XPReward = xpReward;
        OreType = oreType;
    }

    public void Update(float dt)
    {
        if (IsBeingMined)
{
    MineAnimTimer += dt * 10f;
    if (MineAnimTimer >= 1f) { MineAnimTimer = 0f; IsBeingMined = false; }
}
        if (Broken)
        {
            respawnTimer += dt;
            if (respawnTimer >= 8f)
            {
                Broken = false;
                Health = MaxHealth;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        if (Broken)
        {
            // rubble
            Raylib.DrawCircle(x - 8, y + 6,  5, Color.DarkGray);
            Raylib.DrawCircle(x + 6, y + 8,  4, Color.DarkGray);
            Raylib.DrawCircle(x,     y + 10, 3, Color.Gray);
            return;
        }

        switch (RockType)
        {
            case "Stone":
                Raylib.DrawCircle(x, y, 18, new Color((byte)120,(byte)120,(byte)125,(byte)255));
                Raylib.DrawCircle(x - 6, y - 4, 10, new Color((byte)140,(byte)140,(byte)145,(byte)255));
                Raylib.DrawCircle(x + 8, y - 2,  8, new Color((byte)130,(byte)130,(byte)135,(byte)255));
                Raylib.DrawCircle(x - 4, y - 8,  5, new Color((byte)160,(byte)160,(byte)165,(byte)255));
                break;

            case "Copper":
                Raylib.DrawCircle(x, y, 18, new Color((byte)120,(byte)120,(byte)125,(byte)255));
                Raylib.DrawCircle(x - 6, y - 4, 10, new Color((byte)140,(byte)120,(byte)125,(byte)255));
                // copper ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)180,(byte)100,(byte)40,(byte)255));
                Raylib.DrawCircle(x - 4,  y + 4, 4, new Color((byte)200,(byte)120,(byte)50,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 6, 3, new Color((byte)180,(byte)100,(byte)40,(byte)255));
                break;

            case "Iron":
                Raylib.DrawCircle(x, y, 20, new Color((byte)110,(byte)110,(byte)115,(byte)255));
                Raylib.DrawCircle(x - 8, y - 4, 12, new Color((byte)130,(byte)125,(byte)130,(byte)255));
                // iron ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)140,(byte)130,(byte)140,(byte)255));
                Raylib.DrawCircle(x - 2,  y + 6, 5, new Color((byte)100,(byte)90,(byte)100,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 4, 4, new Color((byte)120,(byte)110,(byte)120,(byte)255));
                break;

            case "Gold":
                Raylib.DrawCircle(x, y, 18, new Color((byte)115,(byte)110,(byte)100,(byte)255));
                Raylib.DrawCircle(x - 5, y - 4, 10, new Color((byte)130,(byte)125,(byte)115,(byte)255));
                // gold ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)220,(byte)180,(byte)0,(byte)255));
                Raylib.DrawCircle(x - 3,  y + 5, 5, new Color((byte)200,(byte)160,(byte)0,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 3, 3, new Color((byte)230,(byte)190,(byte)20,(byte)255));
                break;

            case "Crystal":
                Raylib.DrawCircle(x, y, 16, new Color((byte)160,(byte)200,(byte)220,(byte)255));
                // crystal spikes
                Raylib.DrawTriangle(
                    new Vector2(x, y - 22),
                    new Vector2(x - 8, y),
                    new Vector2(x + 8, y),
                    new Color((byte)180,(byte)220,(byte)240,(byte)255));
                Raylib.DrawTriangle(
                    new Vector2(x + 14, y - 16),
                    new Vector2(x + 6,  y),
                    new Vector2(x + 18, y),
                    new Color((byte)160,(byte)200,(byte)225,(byte)220));
                Raylib.DrawTriangle(
                    new Vector2(x - 14, y - 16),
                    new Vector2(x - 18, y),
                    new Vector2(x - 6,  y),
                    new Color((byte)160,(byte)200,(byte)225,(byte)220));
                break;
        }

        // crack overlay based on damage taken
if (Health < MaxHealth)
{
    float damagePercent = 1f - ((float)Health / MaxHealth);
    
    int rx = (int)Position.X;
    int ry = (int)Position.Y;

    // first crack appears after any damage
    if (damagePercent > 0f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx - 2, ry - 12),
            new Vector2(rx + 6, ry - 2),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        Raylib.DrawLineEx(
            new Vector2(rx + 6, ry - 2),
            new Vector2(rx + 2, ry + 8),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
    }

    // second crack at 40% damage
    if (damagePercent > 0.4f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx - 8, ry),
            new Vector2(rx,     ry + 6),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        Raylib.DrawLineEx(
            new Vector2(rx,     ry + 6),
            new Vector2(rx - 4, ry + 14),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        // small chip flaking off
        Raylib.DrawCircle(rx - 10, ry - 6, 3,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
    }

    // third crack at 66% damage — rock is nearly broken
    if (damagePercent > 0.66f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx + 4,  ry - 8),
            new Vector2(rx + 14, ry + 4),
            2f, new Color((byte)20, (byte)20, (byte)20, (byte)220));
        Raylib.DrawLineEx(
            new Vector2(rx + 14, ry + 4),
            new Vector2(rx + 8,  ry + 14),
            2f, new Color((byte)20, (byte)20, (byte)20, (byte)220));
        // more chips
        Raylib.DrawCircle(rx + 16, ry - 4, 3,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
        Raylib.DrawCircle(rx + 10, ry + 16, 2,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
        // dark split running through centre
        Raylib.DrawLineEx(
            new Vector2(rx - 6, ry - 6),
            new Vector2(rx + 8, ry + 10),
            3f, new Color((byte)15, (byte)15, (byte)15, (byte)240));
    }
}

        // health bar
        if (Health < MaxHealth)
        {
            Raylib.DrawRectangle(x - 20, y - 30, 40, 5, Color.DarkGray);
            Raylib.DrawRectangle(x - 20, y - 30, (int)(40 * ((float)Health / MaxHealth)), 5, Color.Orange);
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

    class GasStation
{
    public Vector2 Pump1Pos;
    public Vector2 Pump2Pos;
    public bool Pump1Active = false;
    public bool Pump2Active = false;
    public float PumpFuelRate = 20f;
    public Rectangle BuildingBounds;
    public float OriginX;   // add these
    public float OriginY;

    public GasStation(float x, float y)
    {
        // positions relative to the station's x/y origin
        OriginX = x;
        OriginY = y;
        Pump1Pos = new Vector2(x + 180,  y - 280);
        Pump2Pos = new Vector2(x + 420, y - 280);
        BuildingBounds = new Rectangle(x + 150, y - 200, 260, 160);
    }
}
class Rideable
{
    public enum RideableType { MountainBike, BMX, Horse }
    public RideableType Type;
    public Vector2 Position;
    public Vector2 SpawnPosition;
    public bool Riding = false;
    public Color RideableColor;
    float speed;
    public Vector2 velocity = Vector2.Zero;
    public enum FacingDirection { Down, Up, Left, Right }
    public FacingDirection Facing = FacingDirection.Down;
    float gallopTimer = 0f;
    float gallopInterval = 0.3f;

    float animTimer = 0f;
    bool animFrame = false;
    bool isMoving = false;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 50);

    public Rideable(Vector2 pos, RideableType type, Color color)
    {
        Position = pos;
        SpawnPosition = pos;
        Type = type;
        RideableColor = color;
        Facing = FacingDirection.Right;

        speed = type switch
        {
            RideableType.MountainBike => 400f,
            RideableType.BMX          => 500f,
            RideableType.Horse        => 1000f,
            _                         => 400f
        };
    }

    public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> vehicles, List<Rideable> allRideables)
    {
        if (!Riding) return;

        Vector2 move = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

        if (move != Vector2.Zero)
        {
            move = Vector2.Normalize(move);

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

        if (move != Vector2.Zero && Type == RideableType.Horse)
{
    gallopTimer -= dt;
    if (gallopTimer <= 0f)
    {
        Raylib.SetSoundVolume(Program.soundHorseGallop, Program.soundVolume * 0.3f);
        Raylib.PlaySound(Program.soundHorseGallop);
        gallopTimer = gallopInterval;
    }
}
else
{
    gallopTimer = 0f;
}

        bool onRoad = Program.IsOnRoad(Position);
        float speedMult = Type == RideableType.Horse
            ? (onRoad ? 1.1f : 0.85f)
            : (onRoad ? 1f   : 0.5f);

        Vector2 target = move * speed * speedMult;
        velocity = Vector2.Lerp(velocity, target, dt * 6f);

        if (isMoving)
        {
            animTimer += dt * (Type == RideableType.Horse ? 10f : 7f);
            if (animTimer >= 1f) { animTimer = 0f; animFrame = !animFrame; }
        }

        Vector2 oldPos = Position;
        Position += velocity * dt;

        foreach (Building b in buildings)
            if (Raylib.CheckCollisionRecs(Bounds, new Rectangle(b.Bounds.X, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height - 90)))
            { Position = oldPos; velocity = Vector2.Zero; }

        foreach (TreeObject t in trees)
            if (!t.Chopped && Raylib.CheckCollisionRecs(Bounds, t.Bounds))
            { Position = oldPos; velocity *= -0.2f; }

        foreach (Vehicle v in vehicles)
            if (Raylib.CheckCollisionRecs(Bounds, v.Bounds))
            { Position = oldPos; velocity *= -0.3f; }

        foreach (Rideable r in allRideables)
        {
            if (r == this) continue;
            if (Raylib.CheckCollisionRecs(Bounds, r.Bounds))
            { Position = oldPos; velocity = Vector2.Zero; }
        }
    }

    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        switch (Type)
        {
            case RideableType.MountainBike: DrawMountainBike(x, y); break;
            case RideableType.BMX:          DrawBMX(x, y);          break;
            case RideableType.Horse:        DrawHorse(x, y);        break;
        }
    }

    // ─── SHARED RIDER DRAWING ───────────────────────────────────────────────
    void DrawRider(int rx, int ry, Color skinColor, Color shirtColor, Color pantsColor)
    {
        switch (Facing)
        {
            case FacingDirection.Down:
                // head
                Raylib.DrawCircle(rx + 20, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 15, ry + 8, 2, Color.Black);
                Raylib.DrawCircle(rx + 25, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 14, ry + 14, 12, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));
                // body
                Raylib.DrawRectangle(rx + 10, ry + 20, 20, 20, shirtColor);
                // arms out to sides gripping bars
                Raylib.DrawRectangle(rx,      ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 30, ry + 22, 10, 8, skinColor);
                // legs bent down on pedals/stirrups
                Raylib.DrawRectangle(rx + 10, ry + 40, 8, 12, pantsColor);
                Raylib.DrawRectangle(rx + 22, ry + 40, 8, 12, pantsColor);
                break;

            case FacingDirection.Up:
                // back of head
                Raylib.DrawCircle(rx + 20, ry + 10, 11,
                    new Color((byte)Math.Max(0, skinColor.R - 30),
                              (byte)Math.Max(0, skinColor.G - 30),
                              (byte)Math.Max(0, skinColor.B - 30), (byte)255));
                Raylib.DrawRectangle(rx + 10, ry + 2, 20, 8, new Color((byte)60,(byte)40,(byte)20,(byte)255));
                // body back
                Raylib.DrawRectangle(rx + 10, ry + 20, 20, 20, shirtColor);
                Raylib.DrawRectangle(rx,      ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 30, ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 10, ry + 40, 8, 12, pantsColor);
                Raylib.DrawRectangle(rx + 22, ry + 40, 8, 12, pantsColor);
                break;

            case FacingDirection.Left:
                // side profile head
                Raylib.DrawCircle(rx + 16, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 10, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 6, ry + 13, 4, 3,
                    new Color((byte)Math.Max(0, skinColor.R - 20),
                              (byte)Math.Max(0, skinColor.G - 20),
                              (byte)Math.Max(0, skinColor.B - 20), (byte)255));
                // body leaning forward
                Raylib.DrawRectangle(rx + 8, ry + 20, 14, 20, shirtColor);
                // front arm reaching handlebar
                int armSwingL = isMoving ? (animFrame ? 3 : -3) : 0;
                Raylib.DrawRectangle(rx + 18, ry + 22 + armSwingL, 10, 7, skinColor);
                // legs alternating on pedals
                if (isMoving)
                {
                    Raylib.DrawRectangle(rx + 6,  ry + 40,     8, animFrame ? 14 : 10, pantsColor);
                    Raylib.DrawRectangle(rx + 16, ry + 40, 8, animFrame ? 10 : 14, pantsColor);
                }
                else
                {
                    Raylib.DrawRectangle(rx + 6,  ry + 40, 8, 12, pantsColor);
                    Raylib.DrawRectangle(rx + 16, ry + 40, 8, 12, pantsColor);
                }
                break;

            case FacingDirection.Right:
                Raylib.DrawCircle(rx + 24, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 30, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 30, ry + 13, 4, 3,
                    new Color((byte)Math.Max(0, skinColor.R - 20),
                              (byte)Math.Max(0, skinColor.G - 20),
                              (byte)Math.Max(0, skinColor.B - 20), (byte)255));
                Raylib.DrawRectangle(rx + 18, ry + 20, 14, 20, shirtColor);
                int armSwingR = isMoving ? (animFrame ? 3 : -3) : 0;
                Raylib.DrawRectangle(rx + 12, ry + 22 + armSwingR, 10, 7, skinColor);
                if (isMoving)
                {
                    Raylib.DrawRectangle(rx + 18, ry + 40,     8, animFrame ? 14 : 10, pantsColor);
                    Raylib.DrawRectangle(rx + 28, ry + 40, 8, animFrame ? 10 : 14, pantsColor);
                }
                else
                {
                    Raylib.DrawRectangle(rx + 18, ry + 40, 8, 12, pantsColor);
                    Raylib.DrawRectangle(rx + 28, ry + 40, 8, 12, pantsColor);
                }
                break;
        }
    }

    // ─── MOUNTAIN BIKE ──────────────────────────────────────────────────────
    void DrawMountainBike(int x, int y)
    {
        switch (Facing)
        {
            case FacingDirection.Left:
            case FacingDirection.Right:
                bool flipped = Facing == FacingDirection.Left;
                int fx = flipped ? x + 60 : x;
                int fd = flipped ? -1 : 1;

                // wheels
                Raylib.DrawCircle(x + 12, y + 42, 14, Color.DarkGray);
                Raylib.DrawCircleLines(x + 12, y + 42, 14, Color.Gray);
                Raylib.DrawCircle(x + 12, y + 42, 4, Color.Gray);
                Raylib.DrawCircle(x + 50, y + 42, 14, Color.DarkGray);
                Raylib.DrawCircleLines(x + 50, y + 42, 14, Color.Gray);
                Raylib.DrawCircle(x + 50, y + 42, 4, Color.Gray);

                // frame
                Raylib.DrawLine(x + 12, y + 42, x + 28, y + 22, RideableColor);
                Raylib.DrawLine(x + 28, y + 22, x + 50, y + 42, RideableColor);
                Raylib.DrawLine(x + 12, y + 42, x + 36, y + 22, RideableColor);
                Raylib.DrawLine(x + 28, y + 22, x + 36, y + 22, RideableColor);

                // seat and bars on correct side
                if (!flipped)
                {
                    Raylib.DrawRectangle(x + 22, y + 17, 18, 5, Color.Black);
                    Raylib.DrawRectangle(x + 44, y + 14, 12, 5, Color.DarkGray);
                    Raylib.DrawRectangle(x + 52, y + 10, 4, 14, Color.DarkGray);
                }
                else
                {
                    Raylib.DrawRectangle(x + 20, y + 17, 18, 5, Color.Black);
                    Raylib.DrawRectangle(x + 4,  y + 14, 12, 5, Color.DarkGray);
                    Raylib.DrawRectangle(x + 4,  y + 10, 4, 14, Color.DarkGray);
                }

                // suspension fork
                Raylib.DrawLine(x + 46, y + 20, x + 50, y + 42, Color.LightGray);

                if (Riding) DrawRider(x + 8, y - 16, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;

            case FacingDirection.Down:
            case FacingDirection.Up:
                // single centre wheel
                Raylib.DrawRectangle(x + 27, y + 42, 6, 24, Color.LightGray);
                // frame coming down to wheel
                Raylib.DrawLine(x + 30, y + 20, x + 30, y + 30, RideableColor);
                // handlebars
                Raylib.DrawRectangle(x + 10, y + 18, 40, 5, Color.DarkGray);
                // seat
                Raylib.DrawRectangle(x + 22, y + 30, 15, 10, Color.Black);

                if (Riding) DrawRider(x + 8, y - 18, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;
                    }
                }

    // ─── BMX ────────────────────────────────────────────────────────────────
    void DrawBMX(int x, int y)
    {
        switch (Facing)
        {
            case FacingDirection.Left:
            case FacingDirection.Right:
                bool flipped = Facing == FacingDirection.Left;

                Raylib.DrawCircle(x + 10, y + 42, 11, Color.Black);
                Raylib.DrawCircleLines(x + 10, y + 42, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 10, y + 42, 3, Color.DarkGray);
                Raylib.DrawCircle(x + 46, y + 42, 11, Color.Black);
                Raylib.DrawCircleLines(x + 46, y + 42, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 46, y + 42, 3, Color.DarkGray);

                Raylib.DrawLine(x + 10, y + 42, x + 26, y + 24, RideableColor);
                Raylib.DrawLine(x + 26, y + 24, x + 46, y + 42, RideableColor);
                Raylib.DrawLine(x + 10, y + 42, x + 32, y + 24, RideableColor);
                Raylib.DrawLine(x + 26, y + 24, x + 32, y + 24, RideableColor);

                // pegs
                Raylib.DrawRectangle(x + 2,  y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 18, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 38, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 54, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));

                Raylib.DrawRectangle(x + 20, y + 19, 14, 5, Color.Black);

                if (!flipped)
                {
                    Raylib.DrawRectangle(x + 36, y + 12, 18, 4, Color.DarkGray);
                    Raylib.DrawRectangle(x + 42, y + 12, 4, 12, Color.DarkGray);
                }
                else
                {
                    Raylib.DrawRectangle(x + 6,  y + 12, 18, 4, Color.DarkGray);
                    Raylib.DrawRectangle(x + 6,  y + 12, 4, 12, Color.DarkGray);
                }

                if (Riding) DrawRider(x + 8, y - 16, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;

           case FacingDirection.Down:
            case FacingDirection.Up:
                // single centre wheel
                Raylib.DrawRectangle(x + 27, y + 42, 6, 24, Color.Black);
                
                // frame
                Raylib.DrawLine(x + 30, y + 22, x + 30, y + 32, RideableColor);
                // flat BMX bars
                Raylib.DrawRectangle(x + 8, y + 16, 44, 4, Color.DarkGray);
                // seat
                Raylib.DrawRectangle(x + 22, y + 30, 15, 10, Color.Brown);
                // pegs front-on — just left and right of the single wheel
                Raylib.DrawRectangle(x + 16, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 38, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));

                if (Riding) DrawRider(x + 8, y - 16, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;
        }
    }

    // ─── HORSE ──────────────────────────────────────────────────────────────
    void DrawHorse(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 6 : -6) : 0;

        switch (Facing)
        {
            case FacingDirection.Right:
                // body
                Raylib.DrawRectangle(x + 8,  y + 20, 44, 22, RideableColor);
                // neck + head
                Raylib.DrawRectangle(x + 42, y + 10, 12, 20, RideableColor);
                Raylib.DrawRectangle(x + 46, y + 2,  16, 14, RideableColor);
                Raylib.DrawTriangle(new Vector2(x+52,y+2),new Vector2(x+56,y-6),new Vector2(x+60,y+2), RideableColor);
                Raylib.DrawCircle(x + 58, y + 6, 2, Color.Black);
                Raylib.DrawCircle(x + 61, y + 11, 2, new Color((byte)Math.Max(0,RideableColor.R-40),(byte)Math.Max(0,RideableColor.G-40),(byte)Math.Max(0,RideableColor.B-40),(byte)255));
                // mane + tail
                Raylib.DrawRectangle(x + 44, y + 4, 6, 18, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 8, y + 24, x - 4, y + 40, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 8, y + 24, x - 2, y + 44, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                // legs
                Raylib.DrawRectangle(x + 12, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 24, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 44, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 11, y + 56 + leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 23, y + 56 - leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 + leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 43, y + 56 - leg,  10, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 20, y + 17, 22, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                Raylib.DrawRectangle(x + 22, y + 15, 18, 4, new Color((byte)130,(byte)80,(byte)30,(byte)255));
                if (Riding) DrawRider(x + 12, y - 20, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;

            case FacingDirection.Left:
                // mirrored body
                Raylib.DrawRectangle(x + 8,  y + 20, 44, 22, RideableColor);
                Raylib.DrawRectangle(x + 6,  y + 10, 12, 20, RideableColor);
                Raylib.DrawRectangle(x - 2,  y + 2,  16, 14, RideableColor);
                Raylib.DrawTriangle(new Vector2(x+8,y+2),new Vector2(x+4,y-6),new Vector2(x,y+2), RideableColor);
                Raylib.DrawCircle(x + 2,  y + 6,  2, Color.Black);
                Raylib.DrawCircle(x - 1,  y + 11, 2, new Color((byte)Math.Max(0,RideableColor.R-40),(byte)Math.Max(0,RideableColor.G-40),(byte)Math.Max(0,RideableColor.B-40),(byte)255));
                Raylib.DrawRectangle(x + 10, y + 4, 6, 18, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 52, y + 24, x + 64, y + 40, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 52, y + 24, x + 62, y + 44, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 12, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 24, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 44, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 11, y + 56 + leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 23, y + 56 - leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 + leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 43, y + 56 - leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 18, y + 17, 22, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                Raylib.DrawRectangle(x + 20, y + 15, 18, 4, new Color((byte)130,(byte)80,(byte)30,(byte)255));
                if (Riding) DrawRider(x + 12, y - 20, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;

            case FacingDirection.Down:
                // front-on horse face
                Raylib.DrawRectangle(x + 10, y + 18, 40, 24, RideableColor);
                // head front-on
                Raylib.DrawRectangle(x + 18, y + 2,  24, 20, RideableColor);
                Raylib.DrawCircle(x + 20, y + 10, 3, Color.Black); // left eye
                Raylib.DrawCircle(x + 40, y + 10, 3, Color.Black); // right eye
                Raylib.DrawRectangle(x + 26, y + 16, 8, 6,
                    new Color((byte)Math.Max(0,RideableColor.R-30),(byte)Math.Max(0,RideableColor.G-30),(byte)Math.Max(0,RideableColor.B-30),(byte)255)); // muzzle
                // ears
                Raylib.DrawTriangle(new Vector2(x+18,y+2),new Vector2(x+14,y-6),new Vector2(x+22,y+2), RideableColor);
                Raylib.DrawTriangle(new Vector2(x+42,y+2),new Vector2(x+42,y+2),new Vector2(x+46,y-6), RideableColor);
                // front legs
                Raylib.DrawRectangle(x + 14, y + 40, 10, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 10, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 13, y + 56 + leg, 12, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 - leg, 12, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 14, y + 15, 32, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                if (Riding) DrawRider(x + 8, y - 20, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;

            case FacingDirection.Up:
                // rear-on horse
                Raylib.DrawRectangle(x + 10, y + 18, 40, 24, RideableColor);
                // rump
                Raylib.DrawRectangle(x + 14, y + 10, 32, 12, RideableColor);
                // tail centre
                Raylib.DrawRectangle(x + 26, y + 2, 8, 20, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 20, y + 14, 6, 12, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 34, y + 14, 6, 12, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                // rear legs
                Raylib.DrawRectangle(x + 14, y + 40, 10, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 10, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 13, y + 56 + leg, 12, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 - leg, 12, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 14, y + 15, 32, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                if (Riding) DrawRider(x + 8, y - 20, Program.player.SkinColor, Program.player.ShirtColor, Program.player.PantsColor);
                break;
        }
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
    public enum VehicleType { Sedan, Truck, SUV, PoliceCar, Ambulance, FireTruck }
    public VehicleType Type;
    public Vector2 Position;
    public bool Driving = false;
    public bool OnRoad = false;
    public float Fuel = 100f;
    public float MaxFuel = 100f;
    public bool NeedsPayment = false;
    public float FuelPumped = 0f;
    public bool FuelLocked = false;
    public float TopSpeed => speed;
    float speed;
    public Color VehicleColor;
    public Vector2 velocity = Vector2.Zero;

    public enum FacingDirection { Down, Up, Left, Right }
    public FacingDirection Facing = FacingDirection.Right;

    float animTimer = 0f;
    bool animFrame = false;
    bool isMoving = false;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 100, 50);

    public Vehicle(Vector2 pos, Color vehicleColor, float vehicleSpeed, VehicleType type = VehicleType.Sedan)
    {
        Position = pos;
        VehicleColor = vehicleColor;
        speed = vehicleSpeed;
        Type = type;
        Facing = FacingDirection.Right;
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
        if (Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

        if (move != Vector2.Zero)
        {
            move = Vector2.Normalize(move);

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

        if (move != Vector2.Zero && Fuel > 0)
            Fuel = Math.Max(0, Fuel - dt * 2f);

        if (Fuel <= 0) move = Vector2.Zero;

        float speedMultiplier = OnRoad ? 1f + (Program.player.DrivingLevel * 0.01f) : 0.4f;
        Vector2 targetVelocity = move * speed * speedMultiplier;
        velocity = Vector2.Lerp(velocity, targetVelocity, dt * (OnRoad ? 5f : 2f));

        if (isMoving)
        {
            animTimer += dt * 8f;
            if (animTimer >= 1f) { animTimer = 0f; animFrame = !animFrame; }
        }

        Vector2 oldPos = Position;
        Position += velocity * dt;

        foreach (Building building in buildings)
        {
            Rectangle collisionBox = new Rectangle(
                building.Bounds.X, building.Bounds.Y,
                building.Bounds.Width, building.Bounds.Height);
            if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
            { Position = oldPos; velocity = Vector2.Zero; }
        }


        foreach (TreeObject tree in trees)
            if (!tree.Chopped && Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            { Position = oldPos; velocity *= -0.3f; }

        foreach (Vehicle other in allVehicles)
        {
            if (other == this) continue;
            if (Raylib.CheckCollisionRecs(Bounds, other.Bounds))
            { Position = oldPos; velocity *= -0.5f; }
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
        switch (Type)
        {
            case VehicleType.Sedan: DrawSedan(); break;
            case VehicleType.Truck: DrawTruck(); break;
            case VehicleType.SUV:   DrawSUV();   break;
            case VehicleType.PoliceCar:   DrawPoliceCar();   break;
            case VehicleType.Ambulance:   DrawAmbulance();   break;
            case VehicleType.FireTruck:   DrawFireTruck();   break;
        }
    }

    // ── SEDAN ────────────────────────────────────────────────────────────────
    void DrawSedan()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        switch (Facing)
        {
            case FacingDirection.Right:
                // body
                Raylib.DrawRectangle(x, y + 18, 100, 28, VehicleColor);
                Raylib.DrawRectangle(x, y + 18, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // cabin
                Raylib.DrawRectangle(x + 22, y + 4, 52, 16, VehicleColor);
                Raylib.DrawRectangle(x + 24, y + 6, 48, 12,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                // windscreen lines
                Raylib.DrawLine(x + 24, y + 6,  x + 48, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawLine(x + 72, y + 6,  x + 48, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                // wheels
                Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // headlights
                Raylib.DrawRectangle(x + 92, y + 22, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 92, y + 22, 8, 3,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                // tail lights
                Raylib.DrawRectangle(x, y + 22, 6, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // door line
                Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                // door handle
                Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x, y + 18, 100, 28, VehicleColor);
                Raylib.DrawRectangle(x, y + 18, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 26, y + 4, 52, 16, VehicleColor);
                Raylib.DrawRectangle(x + 28, y + 6, 48, 12,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 28, y + 6,  x + 52, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawLine(x + 76, y + 6,  x + 52, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // headlights left side
                Raylib.DrawRectangle(x, y + 22, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x, y + 22, 8, 3,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                Raylib.DrawRectangle(x + 94, y + 22, 6, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

   case FacingDirection.Down:
    // front-on view

    // Wheels
    Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
    Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);

    // Car Body
    Raylib.DrawRectangle(x + 7, y + 30, 76, 27, VehicleColor);
    Raylib.DrawRectangle(x + 16, y + 9, 59, 22,
        new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

    // windscreen
    Raylib.DrawRectangle(x + 20, y + 13, 52, 14,
        new Color((byte)100,(byte)180,(byte)220,(byte)200));

    // headlights
    Raylib.DrawRectangle(x + 11, y + 32, 16, 9,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));
    Raylib.DrawRectangle(x + 63, y + 32, 16, 9,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));

    // grille
    Raylib.DrawRectangle(x + 31, y + 34, 29, 7, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 32; i < x + 58; i += 5)
        Raylib.DrawRectangle(i, y + 34, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);
    break;

           case FacingDirection.Up:
    // rear view - draw wheels FIRST so body overlaps them
    Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
    Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);

    // Car Body
    Raylib.DrawRectangle(x + 7, y + 30, 76, 27, VehicleColor);
    Raylib.DrawRectangle(x + 16, y + 9, 59, 22,
        new Color((byte)Math.Max(0,VehicleColor.R-20),(byte)Math.Max(0,VehicleColor.G-20),(byte)Math.Max(0,VehicleColor.B-20),(byte)255));

    // exhaust pipe
    Raylib.DrawCircle(x + 11, y + 50, 3, Color.Black);
    Raylib.DrawCircleLines(x + 11, y + 50, 3, Color.Gray);

    // Number plate
    Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);

    // rear window
    Raylib.DrawRectangle(x + 20, y + 13, 52, 14,
        new Color((byte)80,(byte)160,(byte)200,(byte)160));

    // tail lights
    Raylib.DrawRectangle(x + 11, y + 32, 16, 9,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(x + 63, y + 32, 16, 9,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));

    // boot/trunk line
    Raylib.DrawRectangle(x + 29, y + 34, 32, 2, new Color((byte)0,(byte)0,(byte)0,(byte)80));
    break;
        }

        // driver visible through window when riding
        //if (Driving)
        {
            //Raylib.DrawCircle((int)Position.X + 50, (int)Position.Y + 14, 7,
                //Program.player.SkinColor);
        }
    }

    // ── Police Car ────────────────────────────────────────────────────────────────
    void DrawPoliceCar()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        // helper colors
Color bodyColor = new Color((byte)20, (byte)20, (byte)30, (byte)255); // dark police body
Color stripeColor = new Color((byte)240, (byte)240, (byte)240, (byte)255); // white stripe/door
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)200);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // body
        Raylib.DrawRectangle(x, y + 18, 100, 28, bodyColor);
        Raylib.DrawRectangle(x, y + 18, 100, 4,
            new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        // white door stripe
        Raylib.DrawRectangle(x + 28, y + 22, 44, 12, stripeColor);
        // POLICE text on side
        Raylib.DrawText("POLICE", x + 36, y + 24, 10, Color.Black);
        // cabin
        Raylib.DrawRectangle(x + 22, y + 4, 52, 16, bodyColor);
        Raylib.DrawRectangle(x + 24, y + 6, 48, 12, windowTint);
        // windscreen lines
        Raylib.DrawLine(x + 24, y + 6, x + 48, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawLine(x + 72, y + 6, x + 48, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        // wheels
        Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // headlights / tail
        Raylib.DrawRectangle(x + 92, y + 22, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x, y + 22, 6, 10, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        // door line & handles
        Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0, (byte)0, (byte)0, (byte)80));
        Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));
        Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren (roof) — centered above cabin for right-facing
        Raylib.DrawRectangle(x + 40, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 42, y - 0, 8, 6, sirenRed);
        Raylib.DrawRectangle(x + 50, y - 0, 8, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        Raylib.DrawRectangle(x, y + 18, 100, 28, bodyColor);
        Raylib.DrawRectangle(x, y + 18, 100, 4,
            new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 28, y + 22, 44, 12, stripeColor);
        Raylib.DrawText("POLICE", x + 36, y + 24, 10, Color.Black);
        Raylib.DrawRectangle(x + 26, y + 4, 52, 16, bodyColor);
        Raylib.DrawRectangle(x + 28, y + 6, 48, 12, windowTint);
        Raylib.DrawLine(x + 28, y + 6, x + 52, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawLine(x + 76, y + 6, x + 52, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // headlights left side / tail
        Raylib.DrawRectangle(x, y + 22, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 94, y + 22, 6, 10, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0, (byte)0, (byte)0, (byte)80));
        Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));
        Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren centered for left-facing
        Raylib.DrawRectangle(x + 40, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 42, y - 0, 8, 6, sirenBlue);
        Raylib.DrawRectangle(x + 50, y - 0, 8, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // front-on view (same dimensions)
        Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 7, y + 30, 76, 27, bodyColor);
        Raylib.DrawRectangle(x + 16, y + 9, 59, 22, new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 20, y + 13, 52, 14, windowTint);
        Raylib.DrawRectangle(x + 11, y + 32, 16, 9, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 63, y + 32, 16, 9, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 31, y + 34, 29, 7, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 32; i < x + 58; i += 5)
            Raylib.DrawRectangle(i, y + 34, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);

        // POLICE on front bumper
        Raylib.DrawText("POLICE", x + 30, y + 38, 8, Color.Black);

        // siren — between the windscreen and front bonnet
        Raylib.DrawRectangle(x + 34, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 36, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 52, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // rear view
        Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 7, y + 30, 76, 27, bodyColor);
        Raylib.DrawRectangle(x + 16, y + 9, 59, 22, new Color((byte)Math.Max(0, bodyColor.R - 20), (byte)Math.Max(0, bodyColor.G - 20), (byte)Math.Max(0, bodyColor.B - 20), (byte)255));
        Raylib.DrawCircle(x + 11, y + 50, 3, Color.Black);
        Raylib.DrawCircleLines(x + 11, y + 50, 3, Color.Gray);
        Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);
        Raylib.DrawRectangle(x + 20, y + 13, 52, 14, new Color((byte)80, (byte)160, (byte)200, (byte)160));
        Raylib.DrawRectangle(x + 11, y + 32, 16, 9, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 63, y + 32, 16, 9, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 29, y + 34, 32, 2, new Color((byte)0, (byte)0, (byte)0, (byte)80));

        // POLICE on trunk
        Raylib.DrawText("POLICE", x + 30, y + 36, 8, Color.Black);

        // siren — mounted on roof near rear window in rear view
        Raylib.DrawRectangle(x + 34, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 36, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 52, y + 6, 12, 6, sirenRed);
        break;
}
        }

    

    // ── TRUCK ────────────────────────────────────────────────────────────────
    void DrawTruck()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        Color panelColor = new Color(
            (byte)Math.Max(0, VehicleColor.R - 20),
            (byte)Math.Max(0, VehicleColor.G - 20),
            (byte)Math.Max(0, VehicleColor.B - 20), (byte)255);

        switch (Facing)
        {
            case FacingDirection.Right:
                // tray / bed
                Raylib.DrawRectangle(x, y + 14, 60, 34, panelColor);
                Raylib.DrawRectangle(x, y + 14, 60, 4,
                    new Color((byte)Math.Min(255,panelColor.R+30),(byte)Math.Min(255,panelColor.G+30),(byte)Math.Min(255,panelColor.B+30),(byte)255));
                // tray sides
                Raylib.DrawRectangle(x, y + 14, 4, 34, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 44, 60, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // cab
                Raylib.DrawRectangle(x + 58, y + 8, 42, 40, VehicleColor);
                Raylib.DrawRectangle(x + 58, y + 8, 42, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // cab window
                Raylib.DrawRectangle(x + 64, y + 12, 28, 18,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 64, y + 12, x + 78, y + 30, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                // headlight
                Raylib.DrawRectangle(x + 92, y + 20, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                // tail light on tray
                Raylib.DrawRectangle(x, y + 20, 4, 8,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // wheels — dual rear
                Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 38, y + 48, 11, Color.Black);
                Raylib.DrawCircleLines(x + 38, y + 48, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 38, y + 48, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // front wheel
                Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // exhaust pipe
                Raylib.DrawRectangle(x + 96, y + 10, 4, 16, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x + 40, y + 14, 60, 34, panelColor);
                Raylib.DrawRectangle(x + 40, y + 14, 60, 4,
                    new Color((byte)Math.Min(255,panelColor.R+30),(byte)Math.Min(255,panelColor.G+30),(byte)Math.Min(255,panelColor.B+30),(byte)255));
                Raylib.DrawRectangle(x + 96, y + 14, 4, 34, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 40, y + 44, 60, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 8, 42, 40, VehicleColor);
                Raylib.DrawRectangle(x, y + 8, 42, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 8, y + 12, 28, 18,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 36, y + 12, x + 22, y + 30, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawRectangle(x, y + 20, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 96, y + 20, 4, 8,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 62, y + 48, 11, Color.Black);
                Raylib.DrawCircleLines(x + 62, y + 48, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 62, y + 48, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 10, 4, 16, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                break;

            case FacingDirection.Down:

    // wheels
    Raylib.DrawRectangle(x + 7, y + 47, 12, 25, Color.Black);
    Raylib.DrawRectangle(x + 59, y + 47, 12, 25, Color.Black);

    // wide front grille
    Raylib.DrawRectangle(x + 5, y + 7, 68, 54, VehicleColor);
    Raylib.DrawRectangle(x + 5, y + 7, 68, 4,
        new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

    // windscreen
    Raylib.DrawRectangle(x + 13, y + 13, 54, 14,
        new Color((byte)100,(byte)180,(byte)220,(byte)200));

    // wing mirrors
    Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
    Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);

    // big grille
    Raylib.DrawRectangle(x + 13, y + 34, 54, 9, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 18; i < x + 52; i += 5)
        Raylib.DrawRectangle(i, y + 35, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // smaller grille
    Raylib.DrawRectangle(x + 18, y + 45, 43, 9, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 20; i < x + 41; i += 5)
        Raylib.DrawRectangle(i, y + 45, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // bull bar
    Raylib.DrawRectangle(x + 5, y + 50, 68, 14, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangleLines(x + 5, y + 50, 68, 14, Color.Black);

    // large headlights
    Raylib.DrawRectangle(x + 11, y + 52, 7, 7,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));
    Raylib.DrawRectangle(x + 59, y + 52, 7, 7,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);

    break;

case FacingDirection.Up:

    // tray rear view
    Raylib.DrawRectangle(x + 5, y + 7, 68, 50, panelColor);
    Raylib.DrawRectangle(x + 5, y + 7, 68, 18,
        new Color((byte)Math.Max(0,panelColor.R-30),(byte)Math.Max(0,panelColor.G-30),(byte)Math.Max(0,panelColor.B-30),(byte)255));

    // wheels
    Raylib.DrawRectangle(x + 7, y + 47, 12, 20, Color.Black);
    Raylib.DrawRectangle(x + 59, y + 47, 12, 20, Color.Black);

    // tray walls
    Raylib.DrawRectangle(x + 5,  y + 18, 4, 38, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(x + 68, y + 18, 4, 38, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(x + 5,  y + 18, 68, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    // wing mirrors
    Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
    Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);

    // tail lights
    Raylib.DrawRectangle(x + 7,  y + 50, 9, 5,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(x + 59, y + 50, 9, 5,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);

    // tow bar
    Raylib.DrawRectangle(x + 32, y + 54, 11, 5, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    break;
        }

    }

    // ── Fire Truck ────────────────────────────────────────────────────────────────
    void DrawFireTruck()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
    // firetruck-specific colors
Color truckRed = new Color((byte)200, (byte)30, (byte)30, (byte)255);
Color trimWhite = new Color((byte)240, (byte)240, (byte)240, (byte)255);
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)200);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);
Color hoseDark = new Color((byte)40, (byte)40, (byte)40, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // tray / body (red with white trim)
        Raylib.DrawRectangle(x, y + 8, 94, 40, Color.White);
        Raylib.DrawRectangle(x, y + 14, 60, 34, truckRed);
        Raylib.DrawRectangle(x, y + 14, 60, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        Raylib.DrawRectangle(x + 6, y + 22, 48, 10, trimWhite); // white stripe
        
        // ladder on side (stacked rungs)
        Raylib.DrawRectangle(x + 8, y + 18, 4, 28, new Color((byte)150, (byte)150, (byte)150, (byte)255));
        for (int r = 0; r < 5; r++)
            Raylib.DrawRectangle(x + 12, y + 20 + r * 6, 28, 3, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        // hose reel at rear
        Raylib.DrawCircle(x + 12, y + 32, 8, hoseDark);
        Raylib.DrawCircleLines(x + 12, y + 32, 8, Color.DarkGray);
        // tray sides
        Raylib.DrawRectangle(x, y + 14, 4, 34, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 44, 60, 4, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // cab
        Raylib.DrawRectangle(x + 58, y + 8, 42, 40, VehicleColor);
        Raylib.DrawRectangle(x + 58, y + 8, 42, 4, new Color((byte)Math.Min(255, VehicleColor.R + 40), (byte)Math.Min(255, VehicleColor.G + 40), (byte)Math.Min(255, VehicleColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 64, y + 12, 28, 18, windowTint);
        Raylib.DrawLine(x + 64, y + 12, x + 78, y + 30, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        // headlight and tail light
        Raylib.DrawRectangle(x + 92, y + 20, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x, y + 20, 4, 8, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        // wheels — 
        Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // exhaust
        Raylib.DrawRectangle(x + 96, y + 10, 4, 16, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        // "FIRE" text on stripe
        Raylib.DrawText("FIRE", x + 20, y + 22, 10, Color.Red);

        // siren (roof) — centered above cabin
        Raylib.DrawRectangle(x + 60, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 62, y - 0, 8, 6, sirenRed);
        Raylib.DrawRectangle(x + 70, y - 0, 8, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        Raylib.DrawRectangle(x, y + 8, 96, 40, Color.White);
        Raylib.DrawRectangle(x + 40, y + 14, 60, 34, truckRed);
        Raylib.DrawRectangle(x + 40, y + 14, 60, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        Raylib.DrawRectangle(x + 46, y + 22, 48, 10, trimWhite); // stripe
        // ladder on side (mirrored)
        Raylib.DrawRectangle(x + 96, y + 18, 4, 28, new Color((byte)150, (byte)150, (byte)150, (byte)255));
        for (int r = 0; r < 5; r++)
            Raylib.DrawRectangle(x + 56, y + 20 + r * 6, 28, 3, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        // hose reel at rear (mirrored)
        Raylib.DrawCircle(x + 88, y + 32, 8, hoseDark);
        Raylib.DrawCircleLines(x + 88, y + 32, 8, Color.DarkGray);
        Raylib.DrawRectangle(x + 96, y + 14, 4, 34, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 40, y + 44, 60, 4, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 8, 42, 40, VehicleColor);
        Raylib.DrawRectangle(x, y + 8, 42, 4, new Color((byte)Math.Min(255, VehicleColor.R + 40), (byte)Math.Min(255, VehicleColor.G + 40), (byte)Math.Min(255, VehicleColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 8, y + 12, 28, 18, windowTint);
        Raylib.DrawLine(x + 36, y + 12, x + 22, y + 30, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawRectangle(x, y + 20, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 96, y + 20, 4, 8, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
       
        Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 10, 4, 16, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawText("FIRE", x + 52, y + 22, 10, Color.Red);

        // siren centered for left-facing (swap halves visually)
        Raylib.DrawRectangle(x + 20, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 22, y - 0, 8, 6, sirenBlue);
        Raylib.DrawRectangle(x + 30, y - 0, 8, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // wheels
        Raylib.DrawRectangle(x + 7, y + 47, 12, 25, Color.Black);
        Raylib.DrawRectangle(x + 59, y + 47, 12, 25, Color.Black);
        // wide front grille (red with white trim)
        Raylib.DrawRectangle(x + 5, y + 7, 68, 54, truckRed);
        Raylib.DrawRectangle(x + 5, y + 7, 68, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        // white stripe across front
        Raylib.DrawRectangle(x + 12, y + 34, 44, 8, trimWhite);
        // windscreen
        Raylib.DrawRectangle(x + 13, y + 13, 54, 14, windowTint);
        Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
        Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);
        // grilles / bull bar
        Raylib.DrawRectangle(x + 13, y + 34, 54, 9, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 18; i < x + 52; i += 5)
            Raylib.DrawRectangle(i, y + 35, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 18, y + 45, 43, 9, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 20; i < x + 41; i += 5)
            Raylib.DrawRectangle(i, y + 45, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 5, y + 50, 68, 14, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangleLines(x + 5, y + 50, 68, 14, Color.Black);
        Raylib.DrawRectangle(x + 11, y + 52, 7, 7, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 59, y + 52, 7, 7, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);
        Raylib.DrawText("FIRE", x + 26, y + 38, 10, Color.White);

        // front hose coupling detail
        Raylib.DrawRectangle(x + 30, y + 42, 8, 6, hoseDark);
        // siren — between windscreen and bonnet
        Raylib.DrawRectangle(x + 28, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 30, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 46, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // tray rear view (red)
        Raylib.DrawRectangle(x + 5, y + 7, 68, 50, truckRed);
        Raylib.DrawRectangle(x + 5, y + 7, 68, 18, new Color((byte)Math.Max(0, truckRed.R - 30), (byte)Math.Max(0, truckRed.G - 30), (byte)Math.Max(0, truckRed.B - 30), (byte)255));
        // wheels
        Raylib.DrawRectangle(x + 7, y + 47, 12, 20, Color.Black);
        Raylib.DrawRectangle(x + 59, y + 47, 12, 20, Color.Black);
        // tray walls
        Raylib.DrawRectangle(x + 5, y + 18, 4, 38, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangle(x + 68, y + 18, 4, 38, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangle(x + 5, y + 18, 68, 4, Color.White);
        Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
        Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);
        // tail lights and tow bar
        Raylib.DrawRectangle(x + 7, y + 50, 9, 5, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 59, y + 50, 9, 5, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);
        Raylib.DrawRectangle(x + 32, y + 54, 11, 5, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawText("FIRE", x + 26, y + 36, 10, Color.White);

        // ladder roof detail (rear view)
        Raylib.DrawRectangle(x + 12, y + 8, 44, 4, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        for (int r = 0; r < 4; r++)
            Raylib.DrawRectangle(x + 14 + r * 10, y + 12, 6, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren — mounted on roof near rear window (rear view)
        Raylib.DrawRectangle(x + 28, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 30, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 46, y + 6, 12, 6, sirenRed);
        break;
}
    }


    // ── SUV ──────────────────────────────────────────────────────────────────
    void DrawSUV()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        Color darkColor = new Color(
            (byte)Math.Max(0, VehicleColor.R - 30),
            (byte)Math.Max(0, VehicleColor.G - 30),
            (byte)Math.Max(0, VehicleColor.B - 30), (byte)255);

        switch (Facing)
        {
            case FacingDirection.Right:
                // body — taller and boxier than sedan
                Raylib.DrawRectangle(x, y + 12, 100, 36, VehicleColor);
                Raylib.DrawRectangle(x, y + 12, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // full length roof
                Raylib.DrawRectangle(x + 10, y + 2, 80, 12, VehicleColor);
                Raylib.DrawRectangle(x + 10, y + 2, 80, 3,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // roof rack
                Raylib.DrawRectangle(x + 14, y,     72, 3, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawRectangle(x + 14, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 82, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 48, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // windows — two separate panes
                Raylib.DrawRectangle(x + 14, y + 5, 36, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 54, y + 5, 32, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                // window divider (B pillar)
                Raylib.DrawRectangle(x + 50, y + 4, 4, 12, VehicleColor);
                // headlights
                Raylib.DrawRectangle(x + 92, y + 16, 8, 12,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 92, y + 16, 8, 4,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                // tail lights
                Raylib.DrawRectangle(x, y + 16, 6, 12,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));

                // spare tyre on back
                Raylib.DrawRectangle(x - 6, y + 10, 8, 20,
                    Color.Black);

                // skid plate / running boards
                Raylib.DrawRectangle(x + 10, y + 46, 80, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                // wheels — larger than sedan
                Raylib.DrawCircle(x + 20, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 20, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // door lines
                Raylib.DrawLine(x + 50, y + 12, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                // door handles
                Raylib.DrawRectangle(x + 30, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 60, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x, y + 12, 100, 36, VehicleColor);
                Raylib.DrawRectangle(x, y + 12, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 10, y + 2, 80, 12, VehicleColor);
                Raylib.DrawRectangle(x + 10, y + 2, 80, 3,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 14, y, 72, 3, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawRectangle(x + 14, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 82, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 48, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 14, y + 5, 32, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 50, y + 5, 36, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 46, y + 4, 4, 12, VehicleColor);
                // headlights on left side
                Raylib.DrawRectangle(x, y + 16, 8, 12,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x, y + 16, 8, 4,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                Raylib.DrawRectangle(x + 94, y + 16, 6, 12,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));

                // Spare wheel on back
                Raylib.DrawRectangle(x + 98, y + 10, 8, 20,
                    Color.Black);

                Raylib.DrawRectangle(x + 10, y + 46, 80, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawCircle(x + 20, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 20, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawLine(x + 50, y + 12, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                Raylib.DrawRectangle(x + 28, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 58, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Down:

                // wheels
                Raylib.DrawRectangle(x + 5, y + 32, 13, 32, Color.Black);
                Raylib.DrawRectangle(x + 68, y + 32, 13, 32, Color.Black);

                // boxy front
                Raylib.DrawRectangle(x + 5, y + 6, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 5, y + 6, 76, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

                // windscreen
                Raylib.DrawRectangle(x + 9, y + 10, 68, 22,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 40, y + 10, 4, 22, VehicleColor);
                // headlights — square and wide
                Raylib.DrawRectangle(x + 9,  y + 34, 16, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 64, y + 34, 16, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                // grille
                Raylib.DrawRectangle(x + 28, y + 36, 36, 20, new Color((byte)40,(byte)40,(byte)40,(byte)255));
                for (int i = x + 34; i < x + 66; i += 6)
                    Raylib.DrawRectangle(i, y + 37, 2, 10, new Color((byte)80,(byte)80,(byte)80,(byte)255));

                // bull bar
                Raylib.DrawRectangle(x + 5, y + 48, 76, 12, new Color((byte)60,(byte)60,(byte)60,(byte)255));

                // Number plate
                Raylib.DrawRectangle(x + 26, y + 55, 32, 5, Color.White);
                
                break;

            case FacingDirection.Up:

                // wheels
                Raylib.DrawRectangle(x + 5, y + 32, 13, 32, Color.Black);
                Raylib.DrawRectangle(x + 68, y + 32, 13, 32, Color.Black);

                // Back view of SUV body
                Raylib.DrawRectangle(x + 5, y + 6, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 5, y + 6, 76, 4,
                    new Color((byte)Math.Max(0,VehicleColor.R-40),(byte)Math.Max(0,VehicleColor.G-40),(byte)Math.Max(0,VehicleColor.B-40),(byte)255));

                // rear window
                Raylib.DrawRectangle(x + 9, y + 10, 68, 22,
                    new Color((byte)80,(byte)160,(byte)200,(byte)140));
                //Raylib.DrawRectangle(x + 48, y + 10, 4, 22, VehicleColor);

                // tail lights — wide
                Raylib.DrawRectangle(x + 7,  y + 34, 14, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 34, 14, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // spare tyre
                Raylib.DrawCircle(x + 38, y + 30, 14, Color.Black);
                Raylib.DrawCircleLines(x + 38, y + 30, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 38, y + 30, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));

                // tow hitch
                Raylib.DrawRectangle(x + 44, y + 48, 12, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));

                // exhaust pipe
                Raylib.DrawCircle(x + 11, y + 47, 3, Color.Black);
                Raylib.DrawCircleLines(x + 11, y + 47, 3, Color.Gray);

                // Number plate
                Raylib.DrawRectangle(x + 26, y + 55, 32, 5, Color.White);
                break;
        }
    }

    // ── Ambulance ──────────────────────────────────────────────────────────────────
    void DrawAmbulance()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
 // preserved wheel sizes/positions (same as SUV)
// colors
Color bodyWhite = new Color((byte)245, (byte)245, (byte)245, (byte)255);
Color redStripe = new Color((byte)200, (byte)20, (byte)20, (byte)255);
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)180);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // boxy cargo area (keeps overall width/height same)
        Raylib.DrawRectangle(x, y + 6, 72, 44, bodyWhite);              // main box
        Raylib.DrawRectangle(x + 72, y + 12, 8, 38, VehicleColor);     // cab
        Raylib.DrawRectangle(x + 80, y + 30, 16, 20, VehicleColor);     // cab
        
        Raylib.DrawRectangle(x, y + 6, 72, 4, new Color((byte)230, (byte)230, (byte)230, (byte)255));
        // red stripe mid-body
        Raylib.DrawRectangle(x + 6, y + 22, 60, 8, redStripe);
        // large rear roll-up door lines to suggest box truck
        Raylib.DrawRectangleLines(x + 6, y + 10, 60, 36, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        for (int i = 14; i < 46; i += 8)
            Raylib.DrawRectangle(x + 8, y + i, 56, 3, new Color((byte)230,(byte)230,(byte)230,(byte)255));
        // small side window in cab
        Raylib.DrawRectangle(x + 68, y + 13, 9, 18, windowTint);

        // headlights/tail
        Raylib.DrawRectangle(x + 90, y + 30, 5, 5, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        Raylib.DrawRectangle(x, y + 28, 6, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        // preserved wheels (positions unchanged)
        Raylib.DrawCircle(x + 20, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 20, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle(x + 80, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        // Medical cross
        Raylib.DrawRectangle(x + 10, y + 18, 10, 4, Color.Red);
        Raylib.DrawRectangle(x + 13, y + 16, 4, 10, Color.Red);
        // siren on roof (centered)
        Raylib.DrawRectangle(x + 36, y - 2, 28, 8, sirenBase);
        Raylib.DrawRectangle(x + 38, y, 10, 6, sirenRed);
        Raylib.DrawRectangle(x + 50, y, 10, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        // mirror layout: cab on left, box on right

        Raylib.DrawRectangle(x + 24, y + 6, 72, 44, bodyWhite);   // main box
        Raylib.DrawRectangle(x + 16, y + 12, 8, 38, VehicleColor);     // cab
        Raylib.DrawRectangle(x, y + 30, 16, 20, VehicleColor);     // cab

        Raylib.DrawRectangle(x + 24, y + 6, 72, 4, new Color((byte)230, (byte)230, (byte)230, (byte)255));
        Raylib.DrawRectangle(x + 30, y + 22, 60, 8, redStripe);
        // Roll up doors
        Raylib.DrawRectangleLines(x + 34, y + 10, 60, 36, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        for (int i = 14; i < 46; i += 8)
            Raylib.DrawRectangle(x + 38, y + i, 56, 3, new Color((byte)230,(byte)230,(byte)230,(byte)255));
        // Cab window
        Raylib.DrawRectangle(x + 17, y + 13, 9, 18, windowTint);
        // Head and tail lights
        Raylib.DrawRectangle(x + 90, y + 30, 6, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255)); 
        Raylib.DrawRectangle(x, y + 30, 5, 5, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        // wheels (same)
        Raylib.DrawCircle(x + 20, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 20, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle(x + 80, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        
        // mirrored medical cross
        Raylib.DrawRectangle(x + 82, y + 18, 10, 4, Color.Red);
        Raylib.DrawRectangle(x + 85, y + 16, 4, 10, Color.Red);
        // siren
        Raylib.DrawRectangle(x + 36, y - 2, 28, 8, sirenBase);
        Raylib.DrawRectangle(x + 38, y, 10, 6, sirenBlue);
        Raylib.DrawRectangle(x + 50, y, 10, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // wheels (same)
        Raylib.DrawRectangle(x + 7, y + 32, 12, 28, Color.Black);
        Raylib.DrawRectangle(x + 60, y + 32, 12, 28, Color.Black);
        // front box face
        Raylib.DrawRectangle(x + 6, y + 6, 68, 48, bodyWhite);
        // cab windshield integrated at top center
        Raylib.DrawRectangle(x + 16, y + 16, 44, 12, windowTint);
        // red stripe across front
        Raylib.DrawRectangle(x + 16, y + 30, 44, 8, redStripe);
        // grille / bumper
        Raylib.DrawRectangle(x + 6, y + 46, 68, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));
        Raylib.DrawRectangle(x + 22, y + 36, 32, 6, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // headlights
        Raylib.DrawRectangle(x + 9, y + 40, 8, 6, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        Raylib.DrawRectangle(x + 59, y + 40, 8, 6, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        // siren centered on roof/front
        Raylib.DrawRectangle(x + 24, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 20, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 42, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // wheels (same)
        Raylib.DrawRectangle(x + 7, y + 32, 12, 28, Color.Black);
        Raylib.DrawRectangle(x + 60, y + 32, 12, 28, Color.Black);
        // rear box face
        Raylib.DrawRectangle(x + 6, y + 6, 68, 48, bodyWhite);
        // rear window small
        Raylib.DrawRectangle(x + 22, y + 16, 16, 28, windowTint);
        Raylib.DrawRectangle(x + 42, y + 16, 16, 28, windowTint);
        // red stripe near middle
        Raylib.DrawRectangle(x + 16, y + 50, 44, 4, redStripe);
        
        // tail lights and bumper
        Raylib.DrawRectangle(x + 8, y + 38, 10, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 62, y + 38, 10, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        //Raylib.DrawRectangle(x + 6, y + 44, 68, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));
        // rear door seams
        Raylib.DrawRectangleLines(x + 10, y + 12, 56, 32, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        // roof ladder detail
        Raylib.DrawRectangle(x + 20, y + 8, 44, 4, new Color((byte)180,(byte)180,(byte)180,(byte)255));
        for (int r = 0; r < 4; r++)
            Raylib.DrawRectangle(x + 14 + r * 10, y + 12, 6, 3, new Color((byte)160,(byte)160,(byte)160,(byte)255));
        // siren rear-mounted orientation
        Raylib.DrawRectangle(x + 24, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 26, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 42, y + 6, 12, 6, sirenRed);
        break;
}


    }
}
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

    public void Open(int tableIndex)
    {
        IsOpen = true;
        TableIndex = tableIndex;
        GameOver = false;
        BallsPotted = 0;
        Message = "";
        MessageTimer = 0;
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

    public void Update(float dt, Player player)
    {
        if (MessageTimer > 0) MessageTimer -= dt;

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

    void Shoot()
    {
        float speed = 200f + Power * 1400f;
        BallVel[0] = new Vector2(MathF.Cos(AimAngle), MathF.Sin(AimAngle)) * speed;
        Power = 0f;
        BallsMoving = true;
        Aiming = false;
    }

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
                        Message = "Scratch! Cue ball potted.";
                        MessageTimer = 2f;
                    }
                    else
                    {
                        BallsPotted++;
                        player.Money += 3;
                        Message = i == 8 ? "8-Ball potted! You win!" : "Ball potted! +$3";
                        MessageTimer = 1.5f;
                        if (i == 8) GameOver = true;
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

            if (Scratched)
            {
                BallPos[0] = new Vector2(TableLeft + TableWidth * 0.25f, TableTop + TableHeight / 2f);
                BallVel[0] = Vector2.Zero;
                Potted[0] = false;
                Scratched = false;
            }
        }
    }

    public void Draw(Player player)
    {
        Raylib.DrawRectangle(0, 0, 1280, 720, new Color((byte)20, (byte)60, (byte)20, (byte)255));

        Raylib.DrawRectangle(TableLeft, TableTop, TableWidth, TableHeight, new Color((byte)10, (byte)110, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(TableLeft - 12, TableTop - 12, TableWidth + 24, TableHeight + 24), 12,
            new Color((byte)90, (byte)55, (byte)25, (byte)255));

        foreach (var p in Pockets)
            Raylib.DrawCircle((int)p.X, (int)p.Y, 18, Color.Black);

        for (int i = 0; i < 16; i++)
            if (!Potted[i]) DrawBall(i);

        if (Aiming && !BallsMoving && !Potted[0])
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
            Raylib.DrawText("POWER", TableLeft, TableTop + TableHeight + 54, 16, Color.White);
        }

        Raylib.DrawText("8-BALL POOL", 1280 / 2 - 110, 30, 40, Color.Gold);
        Raylib.DrawText($"Balls Potted: {BallsPotted}", TableLeft, TableTop - 40, 22, Color.White);
        Raylib.DrawText($"Wallet: ${player.Money}", TableLeft + TableWidth - 200, TableTop - 40, 22, Color.White);

        if (MessageTimer > 0)
        {
            int mw = Raylib.MeasureText(Message, 26);
            Raylib.DrawText(Message, 1280 / 2 - mw / 2, TableTop + TableHeight + 80, 26, Color.Yellow);
        }

        if (GameOver)
        {
            string winText = "YOU WIN! 8-Ball potted!";
            int ww = Raylib.MeasureText(winText, 34);
            Raylib.DrawText(winText, 1280 / 2 - ww / 2, TableTop + TableHeight / 2 - 20, 34, Color.Gold);
            Raylib.DrawText("ESC = Leave Table", 1280 / 2 - 90, TableTop + TableHeight / 2 + 30, 20, Color.LightGray);
        }
        else
        {
            string prompt = BallsMoving ? "Balls rolling..." : "LEFT/RIGHT = Aim | Hold SPACE = Power | Release = Shoot | ESC = Leave";
            int pw = Raylib.MeasureText(prompt, 18);
            Raylib.DrawText(prompt, 1280 / 2 - pw / 2, 720 - 40, 18, Color.LightGray);
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
        int tw = Raylib.MeasureText(numText, 12);
        Raylib.DrawText(numText, (int)pos.X - tw / 2, (int)pos.Y - 6, 12, Color.Black);
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
        bool twoMatch = !allMatch && (Reels[0] == Reels[1] || Reels[1] == Reels[2] || Reels[0] == Reels[2]);

        if (allMatch)
        {
            int multiplier = SlotData.ThreeMatchMultiplier[Reels[0]];
            int winnings = BetAmount * multiplier;
            player.Money += winnings;
            ResultMessage = Reels[0] == SlotSymbol.Seven
                ? $"JACKPOT!!! Triple 7s — you won ${winnings}!"
                : $"Triple match! You won ${winnings}!";
        }
        else if (twoMatch)
        {
            int winnings = (int)(BetAmount * SlotData.TwoMatchMultiplier);
            player.Money += winnings;
            ResultMessage = $"Two in a row! You got ${winnings} back.";
        }
        else
        {
            ResultMessage = $"No match. Lost ${BetAmount}.";
        }

        ResultTimer = 2.5f;
    }
}



    class Building
    {
        public Rectangle Bounds;
        public Vector2 EntryPosition;

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
            NPC npc,
            Vector2 entryPos = default
        )
        {
            Bounds = bounds;
            ExteriorColor = exterior;
            InteriorColor = interior;
            ExitPosition = exitPos;
            BuildingName = buildingName;
            InteriorNPC = npc;
            EntryPosition = entryPos == default ? new Vector2(300, 500) : entryPos;

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
