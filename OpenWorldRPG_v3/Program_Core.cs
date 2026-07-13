using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        public static float GetCurrentHour() => timeOfDay * 24f;

        static void ChangeScene(SceneState newScene, Action onSwap = null)
{
    if (newScene == currentScene && sceneFadeState == FadeState.None) return;
    pendingScene = newScene;
    pendingSceneSetup = onSwap;
    sceneFadeState = FadeState.FadingOut;
}

static void UpdateSceneFade(float dt)
{
    if (sceneFadeState == FadeState.FadingOut)
    {
        sceneFadeAlpha = Math.Min(255f, sceneFadeAlpha + sceneFadeSpeed * dt);
        if (sceneFadeAlpha >= 255f)
        {
            currentScene = pendingScene;
            pendingSceneSetup?.Invoke();   // safe to move the player now — nothing is on screen
            pendingSceneSetup = null;
            sceneFadeState = FadeState.FadingIn;
        }
    }
    else if (sceneFadeState == FadeState.FadingIn)
    {
                sceneFadeAlpha = Math.Max(0f, sceneFadeAlpha - sceneFadeSpeed * dt);
                if (sceneFadeAlpha <= 0f) sceneFadeState = FadeState.None;
            }
        }

        static void StartTestTransition(Action onComplete, string message = "Preparing your test...")
        {
            testTransitionActive   = true;
            testTransitionTimer    = 0f;
            testTransitionAlpha    = 0f;
            testTransitionFadeIn   = true;
            testTransitionCallback = onComplete;
            testTransitionMessage  = message;   // ADDED
        }

        static void DrawTestTransitionOverlay()
    {
        if (!testTransitionActive) return;
        byte alpha = (byte)(255 * testTransitionAlpha);
        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)alpha));
        if (testTransitionAlpha >= 1f)
        {
            int tw = Program.MeasureTextUI(testTransitionMessage, 28);
            Program.DrawTextUI(testTransitionMessage, ScreenWidth/2 - tw/2, ScreenHeight/2 - 14, 28, Color.Gold);
        }
    }

static void ResetGameState()
{
    // toolbar
    for (int i = 0; i < 8; i++)
        toolbarSlots[i] = null;        // or "empty" if that's your empty marker
    toolbarSelectedSlot = 0;

    // tool pickup flags
    axePickedUp = false;
    pickaxePickedUp = false;
    fishingRodPickedUp = false;
    fishingNetPickedUp = false;
    torchPickedUp = false;
    spadePickedUp = false;
    wateringCanPickedUp = false;
    bowPickedUp = false;
    staffPickedUp = false;
    crossbowPickedUp = false;
    trolleyPickedUp = false;
    basketPickedUp = false;
    tutorialChestOpened = false;

    // equipment slots
    armorHelmet = null;
    armorBody = null;
    armorLegs = null;
    armorBoots = null;
    armorGloves = null;
    armorCape = null;
    armorWeapon = null;
    armorShield = null;

    // combat column
    equipped1H = null;
    equipped2H = null;
    equippedAmmo = null;

    // gear menu state
    gearTestMode = false;

    timeOfDay = 0f; dayOfWeek = 0;
    dayOfMonth = 1;          
    currentMonth = 0;        
    dayCounter = 0f; 
    ResetFogOfWar();
    foreach (var a in achievements) a.Unlocked = false;
    achievementsUnlockedCount = 0;
    achievementVisited.Clear();
    foreach (var e in bestiary.Values) { e.Kills = 0; e.Discovered = false; }
    }
    static void ApplyPlayerSheets()
{
    player.Character.BodyTexture     = AssetManager.Get("maleBody");
    player.Character.HairTexture     = AssetManager.Get("messy");
    player.Character.ClothingTexture = AssetManager.Get("tshirt");
    player.Character.BootsTexture    = AssetManager.Get("shorts");
}

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG V3");

            // Player
            AssetManager.Load("maleBody",  "resources/player/body/maleBody.png");
            AssetManager.Load("messy", "resources/player/hair/messyHairstyle.png");
            AssetManager.Load("tshirt",    "resources/player/torso/tshirt.png");
            AssetManager.Load("shorts",    "resources/player/pants/shorts.png");

            ApplyPlayerSheets(); 

            // Tools and weapons
            AssetManager.Load("held_pickaxe",     "resources/player/tools/pickaxe.png");
            AssetManager.Load("held_axe",         "resources/player/tools/axe.png");
            AssetManager.Load("held_wateringcan", "resources/player/tools/wateringcan.png");
            AssetManager.Load("held_spade",       "resources/player/tools/spade.png");
            AssetManager.Load("held_sword",       "resources/player/weapons/sword.png");
            AssetManager.Load("held_dagger",      "resources/player/weapons/dagger.png");
            AssetManager.Load("held_staff",       "resources/player/weapons/staff.png");
            AssetManager.Load("held_fishingrod", "resources/player/tools/fishingrod.png");   
            AssetManager.Load("held_hammer",     "resources/player/tools/hammer.png");       
            AssetManager.Load("held_scimitar",   "resources/player/weapons/scimitar.png");

            // Friend NPCS
            AssetManager.Load("villager", "resources/npc/villager.png");
            AssetManager.Load("ranger", "resources/npc/Ranger.png"); 
            AssetManager.Load("Cride", "resources/npc/Cride.png");
            AssetManager.Load("JDogg",     "resources/npc/JDogg.png");
            AssetManager.Load("Jake","resources/npc/Jake.png");
            AssetManager.Load("Shack", "resources/npc/Shack.png");
            AssetManager.Load("Traz", "resources/npc/Traz.png");
            AssetManager.Load("Nola",     "resources/npc/Nola.png");
            AssetManager.Load("Tipene","resources/npc/Tipene.png");
            AssetManager.Load("Joybells", "resources/npc/Joybells.png");
            AssetManager.Load("Rala", "resources/npc/Rala.png");
            AssetManager.Load("Hunter",     "resources/npc/Hunter.png");
            AssetManager.Load("Ava","resources/npc/Ava.png");
            AssetManager.Load("Eli", "resources/npc/Eli.png");
            AssetManager.Load("Ezra","resources/npc/Ezra.png");
            AssetManager.Load("Eden", "resources/npc/Eden.png");
            AssetManager.Load("Leo","resources/npc/Leo.png");
            AssetManager.Load("Alice", "resources/npc/Alice.png");
            AssetManager.Load("Whale","resources/npc/Whale.png");
            AssetManager.Load("Jail", "resources/npc/Jail.png");
            AssetManager.Load("player_pickaxe", "resources/player/Player.png");   

            // Shop NPCS
            AssetManager.Load("Wizard", "resources/npc/Wizard.png");  
            AssetManager.Load("Blacksmith","resources/npc/Blacksmith.png");
            AssetManager.Load("nurse", "resources/npc/nurse.png");  
            AssetManager.Load("cop","resources/npc/cop.png");

            // Extra NPCS
            AssetManager.Load("prince", "resources/npc/prince.png");  
            AssetManager.Load("princess","resources/npc/princess.png");
            AssetManager.Load("joey", "resources/npc/joey.png");  
            AssetManager.Load("bella","resources/npc/bella.png");
            AssetManager.Load("Elder", "resources/npc/Elder.png");
            AssetManager.Load("shoolboy", "resources/npc/Schoolboy.png");  
            AssetManager.Load("schoolgirl","resources/npc/Schoolgirl.png");
            AssetManager.Load("goku", "resources/npc/goku.png");  
            AssetManager.Load("martha","resources/npc/martha.png");
            AssetManager.Load("iggy", "resources/npc/Iggy.png");
            AssetManager.Load("angel", "resources/npc/Angel.png");

            // Enemy NPCS
            AssetManager.Load("skeleton", "resources/npc/SkeletonSwordsman.png");




            nightMask = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
            LoadUIFont();
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.Null);

            foreach (var f in friendNPCs)
            f.Npc.SpriteKey = f.Name;
            InitAchievements();
            InitBestiary();

            Raylib.InitAudioDevice();
            
            if (Raylib.IsAudioDeviceReady())
        {
            musicMainMenu = Raylib.LoadMusicStream("resources/music/Meadow Thoughts.ogg");
            musicDbar   = Raylib.LoadMusicStream("resources/music/dbarMusic.mp3");
            musicFarm  = Raylib.LoadMusicStream("resources/music/farmMusic.mp3");
            musicCity  = Raylib.LoadMusicStream("resources/music/Pleasant Creek.mp3");
            musicHouse = Raylib.LoadMusicStream("resources/music/Meadow Thoughts.ogg");
            musicRain = Raylib.LoadMusicStream("resources/music/rainMusic.ogg");
            musicMeadowlands = Raylib.LoadMusicStream("resources/music/meadowlandsMusic.mp3");
            musicForest = Raylib.LoadMusicStream("resources/music/forestMusic.mp3");
            musicBeach = Raylib.LoadMusicStream("resources/music/beachMusic.mp3");
            musicDesert = Raylib.LoadMusicStream("resources/music/desertTownMusic.mp3");
            musicSnow = Raylib.LoadMusicStream("resources/music/snowMusic.mp3");
            musicTakeaways = Raylib.LoadMusicStream("resources/music/takeawayMusic.ogg");
            musicOcean  = Raylib.LoadMusicStream("resources/music/farmMusic.mp3");
            musicVolcano  = Raylib.LoadMusicStream("resources/music/wastelands.ogg");

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
            soundHorn = Raylib.LoadSound("resources/sound/horn.wav");
        

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
            Raylib.SetMusicVolume(musicMeadowlands, musicVolume);
            Raylib.SetMusicVolume(musicCity, musicVolume);
            Raylib.SetMusicVolume(musicOcean, musicVolume);
            Raylib.SetMusicVolume(musicVolcano, musicVolume);
           
           
           
           

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
        
        CarparkManager.Add(new Vector2(3100, 900), new Vector2(3700, 900), depth: 90f, facing: true);
        CarparkManager.Add(new Vector2(3100, 1050), new Vector2(3700, 1050), depth: 90f, facing: true);
        
        RoadManager.Add(new Vector2(2650, -1400), new Vector2(2650, 2000));  
        RoadManager.Add(new Vector2(-2000, 2000), new Vector2(3550, 2000));
        RoadManager.Add(new Vector2(-60000, 500), new Vector2(60000, 500));
        RoadManager.Add(new Vector2(200, -60000), new Vector2(200, 60000));
        RoadManager.Add(new Vector2(1400, 2000), new Vector2(1400, 500));
        RoadManager.Add(new Vector2(200, -200), new Vector2(2650, -200));
        RoadManager.Add(new Vector2(-1700, 1100), new Vector2(200, 1100));
        RoadManager.Add(new Vector2(-1600, 1240), new Vector2(-1600, -850));
        RoadManager.Add(new Vector2(-2500, -850), new Vector2(200, -850)); 
        RoadManager.Add(new Vector2(1800, -850), new Vector2(3500, -850)); 

        
       
        
            GenerateWorld();

            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Zoom = 1f;

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                UpdateSurvival(dt);
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
if (uiFontLoaded) Raylib.UnloadFont(uiFont);
Raylib.UnloadRenderTexture(nightMask); 
            Raylib.CloseWindow();
        }

        static void Update(float dt)  
        {
            UpdateSceneFade(dt);
            if (licenceCongratsOpen || licenceMailMenuOpen)
            {
                return;
            }
            if (treeChopConfirmOpen)
            {
                return;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Tab))
            {
            if (!pauseMenuOpen && !armorMenuOpen && !skillsOpen && !questsOpen)
                    player.InventoryOpen = !player.InventoryOpen;
            }

   if (testTransitionActive)
{
    testTransitionTimer += dt;
    if (testTransitionFadeIn)
    {
        testTransitionAlpha = Math.Min(1f, testTransitionTimer / 0.6f);
        if (testTransitionAlpha >= 1f) testTransitionFadeIn = false;
    }
    else if (testTransitionTimer >= testTransitionDuration)
    {
        testTransitionActive = false;
        testTransitionCallback?.Invoke();
        testTransitionCallback = null;
    }
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

if (Raylib.IsMouseButtonPressed(MouseButton.Left) && !multiplayerMenuOpen)
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

       if (!overwriteConfirmOpen && Raylib.CheckCollisionPointRec(mouse, slotBtn))
{
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (isLoadingGame && exists)
        {
            selectedSlot = i;
            slotSelected = true;
            LoadGame();
            ChangeScene(SceneState.World);
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
                ApplyPlayerSheets();
                ResetGameState();  
                placedChests.Clear();
                timeOfDay = 0f; dayOfWeek = 0;
                quests[0].Progress = 0; quests[0].Completed = false;
                quests[1].Progress = 0; quests[1].Completed = false;
                quests[2].Progress = 0; quests[2].Completed = false;
                totalPlayTime = 0f;
                tutorialActive = true;
                tutorialStep = 0;
                tutorialNpcPos = player.Position + new Vector2(-60, -40);
                foreach (var t in tutorialTasks) t.Done = false;
                SnapshotTutorialMarks();
                ChangeScene(SceneState.World);
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

                UpdateAchievements();
                UpdateFogOfWar();
                CheckZoneMusic();

                        // ── CHAT INPUT ────────────────────────────────────────────────────────────
       if (chatInputCooldown > 0f) chatInputCooldown -= dt;

        // ── CHAT INPUT ────────────────────────────────────────────────────────────
        if (chatInputOpen)
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if (key >= 32 && key <= 125 && chatInputText.Length < 60)
                    chatInputText += (char)key;
                key = Raylib.GetCharPressed();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && chatInputText.Length > 0)
                chatInputText = chatInputText[..^1];

            if (Raylib.IsKeyPressed(KeyboardKey.Enter) && chatInputText.Length > 0)
            {
                playerChatMessage = chatInputText;
                playerChatTimer   = playerChatDuration;
                multiplayer.SendChat(chatInputText);
                multiplayer.SendTyping(false);
                chatInputText = "";
                chatInputOpen = false;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                multiplayer.SendTyping(false);
                chatInputText = "";
                chatInputOpen = false;
            }
        }
        else
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) && chatInputCooldown <= 0f)
            {
                chatInputOpen = true;
                multiplayer.SendTyping(true);
            }
        }

        // age the local speech bubble
        if (playerChatTimer > 0f)
            playerChatTimer -= Raylib.GetFrameTime();
             bool blockShortcutsForChat = chatInputOpen;

                string mpSceneTag = currentScene == SceneState.Building && currentBuilding != null
                ? $"Building:{currentBuilding.BuildingName}"
                : currentScene.ToString();
                multiplayer.Update(player, playerName, mpSceneTag);
                CheckSendAppearanceIfChanged();
                UpdateTutorial(dt);
                UpdateDroppedItems(dt);
                UpdateStoryQuests();

// Toolbar slot selection — number keys 1-8
if (!chatInputOpen)
{
    for (int k = 0; k < 8; k++)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One + k))
            toolbarSelectedSlot = k;
    }
}

if (Raylib.IsKeyPressed(KeyboardKey.P) && !chatInputOpen)     
    useLayeredPlayer = !useLayeredPlayer;

if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen && !playerMenuOpen)
{
    string held = toolbarSlots[toolbarSelectedSlot];
    if (held != null && held.Contains("Pickaxe"))
    player.TriggerMineAnim();
    var nearWorldChest = placedChests.FirstOrDefault(c =>
        c.BuildingContext == "" && Vector2.Distance(player.Center, c.Position) < 80);

    if (chestOpen) { chestOpen = false; openChestId = null; }          // Space closes an open chest
    else if (held != null && IsPlaceable(held)) PlaceHeldItem(held);   // place in front of player
    else if (nearWorldChest != null)                                    // open placed chest
        { chestOpen = true; openChestId = nearWorldChest.Id; }
    else if (NearAnyCraftStation())
        { playerMenuOpen = true; playerMenuTab = PlayerMenuTab.Crafting; }
    else if (stationProps["Advanced Furnace"].Any(f => Vector2.Distance(player.Center, f) < 80))
        { furnaceOpen = true; furnaceOpenAdvanced = true; }
    else if (placedFurnaces.Any(f => Vector2.Distance(player.Center, f) < 80))
        { furnaceOpen = true; furnaceOpenAdvanced = false; }
    else if (held != null && IsUsable(held))
        UseToolbarItem(toolbarSelectedSlot);
}

if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && !chestOpen && !playerMenuOpen)
{
    // NEW — coastguard first, then placeable pickup as before:
    var cg = coastguardStations.FirstOrDefault(s => Vector2.Distance(player.Center, s) < 110);
    if (cg != default) CoastguardRecover(cg);
    else TryPickUpPlaceable();
}

// campfire burn-down
        for (int i = 0; i < campfirePositions.Count; i++)
        {
            if (campfireLogs.GetValueOrDefault(i) > 0 && campfireBurn.GetValueOrDefault(i) > 0f)
            {
                campfireBurn[i] -= dt;
                if (campfireBurn[i] <= 0f)
                {
                    campfireLogs[i]--;                     // current log burned out
                    if (campfireLogs[i] > 0)
                        campfireBurn[i] = LogBurnSeconds;  // next log ignites
                }
            }
        }

foreach (var pen in livestockPens)
{
    if (pen.ReadyToHarvest) continue;
    if (pen.Fed)
    {
        pen.Timer += dt;
        if (pen.Timer >= pen.Cycle) { pen.ReadyToHarvest = true; pen.Timer = 0f; pen.Fed = false; }
    }
}

foreach (var plot in farmPlots)
{
    if (plot.Planted && plot.Watered && !plot.ReadyToHarvest)
    {
        plot.GrowTimer += dt;
        if (plot.GrowTimer >= plot.GrowDuration) 
            plot.ReadyToHarvest = true;
    }
}

foreach (var tree in fruitTrees)
{
    if (!tree.Planted) continue;
    if (!tree.ReadyToHarvest)
    {
        tree.GrowTimer += dt;
        if (tree.GrowTimer >= tree.GrowDuration) tree.ReadyToHarvest = true;
    }
    else if (tree.RegrowTimer > 0f)
    {
        tree.RegrowTimer -= daySpeed * dt;   // CHANGED — was real seconds, now day-cycle units
    }
}

UpdateCollectables("World", player.Center);
UpdateShrines();

// Also mouse scroll wheel
float scroll = Raylib.GetMouseWheelMove();
if (scroll != 0)
    camera.Zoom = Math.Clamp(camera.Zoom + scroll * 0.1f, 0.2f, 4.0f);

                // Axe pickup
                if (!axePickedUp && Vector2.Distance(player.Center, axePosition) < 60)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                    {
                        player.HasAxe = true;
                        axePickedUp = true;
                        AddToToolbar("Axe");
                        ShowNotification("You picked up an Axe! You can now chop trees.");
                    }
                }

                // Pickaxe pickup
if (!pickaxePickedUp && Vector2.Distance(player.Center, pickaxePosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        pickaxePickedUp = true;
        AddToToolbar("Pickaxe");
        ShowNotification("Pickaxe added to toolbar!");
    }
}

// Fishing rod pickup
if (!fishingRodPickedUp && Vector2.Distance(player.Center, fishingRodPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        fishingRodPickedUp = true;
        AddToToolbar("Rod");
        ShowNotification("Fishing Rod added to toolbar! Equip it near lakes.");
    }
}

// Fishing net pickup
if (!fishingNetPickedUp && Vector2.Distance(player.Center, fishingNetPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        fishingNetPickedUp = true;
        AddToToolbar("Net");
        ShowNotification("Fishing Net added to toolbar!");
    }
}

// Torch pickup
if (!torchPickedUp && Vector2.Distance(player.Center, torchPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        torchPickedUp = true;
        AddToToolbar("Torch");
        ShowNotification("Torch added to toolbar!");
    }
}

// pickup interaction — ADD alongside the Axe/Pickaxe/Rod pickup blocks
if (!spadePickedUp && Vector2.Distance(player.Center, spadePosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        spadePickedUp = true;
        AddToToolbar("Spade");
        ShowNotification("You picked up a Spade! You can now till soil.");
    }
}

if (!wateringCanPickedUp && Vector2.Distance(player.Center, wateringCanPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        wateringCanPickedUp = true;
        AddToToolbar("Watering Can");
        ShowNotification("You picked up a Watering Can! Water your tilled soil.");
    }
}

// pickup interaction — ADD alongside the Spade/Watering Can pickups
if (!wheatSeedsPickedUp && Vector2.Distance(player.Center, wheatSeedsPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        wheatSeedsPickedUp = true;
        AddOneItemToToolbar("Wheat Seeds");
        ShowNotification("Picked up Wheat Seeds!");
    }
}

// Weapon pickups and routing to there weapon holders
// Stick pickup
if (!stickPickedUp && Vector2.Distance(player.Center, stickPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        stickPickedUp = true;   // now owned → appears in inventory
        if (equipped1H == null)
        {
            equipped1H  = "Stick";
            equipped2H  = null;
            armorWeapon = "Stick";
            ShowNotification("Stick equipped in 1H weapon slot! Press T to draw it.");
        }
        else
        {
            ShowNotification("Stick added to inventory (1H slot full).");
        }
    }
}

// Sword pickup
if (!swordPickedUp && Vector2.Distance(player.Center, swordPosition) < 60)
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        swordPickedUp = true;   // now owned → appears in inventory
        if (equipped1H == null)
        {
            equipped1H  = "Sword";
            equipped2H  = null;
            armorWeapon = "Sword";
            ShowNotification("Sword equipped in 1H weapon slot! Press T to draw it.");
        }
        else
        {
            ShowNotification("Sword added to inventory (1H slot full).");
        }
    }
}

// Bow pickup
if (!bowPickedUp && Vector2.Distance(player.Center, bowSpawnPos) < 50 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
{
    bowPickedUp = true;
    player.HasBow = true;   // now owned → appears in inventory
    if (equipped2H == null)
    {
        equipped2H  = "Bow";
        equipped1H  = null;
        armorShield = null;
        armorWeapon = "Bow";
        ShowNotification("Bow equipped in 2H weapon slot! Press T to draw it.");
    }
    else
    {
        ShowNotification("Bow added to inventory (2H slot full).");
    }
}

// Crossbow pickup
if (!crossbowPickedUp && Vector2.Distance(player.Center, crossbowSpawnPos) < 50 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
{
    crossbowPickedUp = true;
    player.HasCrossbow = true;   // now owned → appears in inventory
    if (equipped2H == null)
    {
        equipped2H  = "Crossbow";
        equipped1H  = null;
        armorShield = null;
        armorWeapon = "Crossbow";
        ShowNotification("Crossbow equipped in 2H weapon slot! Press T to draw it.");
    }
    else
    {
        ShowNotification("Crossbow added to inventory (2H slot full).");
    }
}

if (!staffPickedUp && Vector2.Distance(player.Center, staffSpawnPos) < 60
    && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
{
    staffPickedUp = true;
    AcquireGear(staffType);   // adds the staff to owned gear/inventory
    ShowNotification($"You picked up a {staffType}! Equip it to cast magic.");
}

if (!tutorialChestOpened && Vector2.Distance(player.Center, tutorialChestPos) < 60
    && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
{
    tutorialChestOpened = true;
    TryGiveItem("Arrows", 20);
    TryGiveItem("Arcane Essence", 15);
    ShowNotification("Chest opened! +20 Arrows, +15 Arcane Essence for training.");
}

if (Raylib.IsKeyPressed(KeyboardKey.T) && !chatInputOpen)
{
    currentPhase = currentPhase == HandPhase.Tools ? HandPhase.Combat : HandPhase.Tools;
    ShowNotification(currentPhase == HandPhase.Tools ? "Tools equipped" : "Weapon drawn");
}

if (Raylib.IsKeyPressed(KeyboardKey.G) && !chatInputOpen)
    if (!pauseMenuOpen && !player.InventoryOpen && !skillsOpen && !questsOpen)
        armorMenuOpen = !armorMenuOpen;

if (Raylib.IsKeyPressed(KeyboardKey.J) && !chatInputOpen)
            playerMenuOpen = !playerMenuOpen;

// Rocket — press E nearby to launch into space
                    if (Vector2.Distance(player.Center, rocketPosition) < 100
                        && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                    {
                        EnterSpace();
                    }

// Dive entry — press J near deep water / ocean
                    if (Raylib.IsKeyPressed(KeyboardKey.Z) && IsNearDeepWater(player.Center) && !chatInputOpen)
                    {
                        StartDive();
                    }

// ── DEV SHORTCUT — remove before release ──────────────────────────────────
if (Raylib.IsKeyPressed(KeyboardKey.F7))
{
    hasTheoryD = true;
    preTestScene = currentScene;
    StartDrivingTest();
}

if (Raylib.IsKeyPressed(KeyboardKey.F9))
{
    hasTheoryD = hasTheoryC = hasTheoryB = hasTheoryA = hasTheoryS = false;
    hasPracticalD = hasPracticalC = hasPracticalB = hasPracticalA = hasPracticalS = false;
    SaveGame();
}

if (Raylib.IsKeyPressed(KeyboardKey.F8))
    skillCheatOpen = !skillCheatOpen;

                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                        {
                            bool wasOpen = pauseMenuOpen;
                            pauseMenuOpen = !pauseMenuOpen;
                            // close all other menus when pause opens
                            if (pauseMenuOpen)
                            {
                                player.InventoryOpen = false;
                                armorMenuOpen = false;
                                skillsOpen = false;
                                questsOpen = false;
                            }
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
                        
                    if (shakeDuration > 0) shakeDuration -= dt;
                    if (levelUpTimer > 0) levelUpTimer -= dt;
                    if (farmToolCooldown > 0f) farmToolCooldown -= dt;
                    if (houseBuildingActive)
                    {
                        houseBuildingTimer += dt;
                        if (houseBuildingFadeIn)
                        {
                            houseBuildingAlpha = Math.Min(1f, houseBuildingTimer / 0.5f);
                            if (houseBuildingAlpha >= 1f) houseBuildingFadeIn = false;
                        }
                        if (houseBuildingTimer >= houseBuildingDuration)
                        {
                            houseBuildingActive = false;
                            houseBuildingTimer  = 0f;
                            houseBuildingAlpha  = 0f;
                            houseBuildingFadeIn = true;
                            if (activeHousePlotIndex >= 0 && activeHousePlotIndex < ownedHousePlots.Count)
                            {
                                var newPlot = ownedHousePlots[activeHousePlotIndex];
                                player.Position = new Vector2(newPlot.x + 120, newPlot.y + 150);
                            }
                        }
                    }                                        

                    if (!multiplayer.Connected || multiplayer.IsHost)   
                    {
                        timeOfDay += daySpeed * dt;
                        if (timeOfDay > 1f) { /* ... existing rollover: dayOfWeek/dayOfMonth/currentMonth ... */ }
                    }

                    UpdateWeather(dt);
                    UpdateQuests();
                    player.UpdateHealth(dt);
                    UpdateQuickCook(dt);
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

                    if (lastJobResetDay != dayOfWeek)
                    {
                        lastJobResetDay = dayOfWeek;
                        foreach (var j in jobBoard) j.CompletedToday = false;
                        foreach (var f in friendNPCs) { f.TalkedToday = false; f.GiftedToday = false; }
                        cardSpentToday = 0;
                        foreach (var t in billboardTasks) { t.DoneToday = false; }
                        RollDailyPlushStock();
                    }


                    for (int i = floatingTexts.Count - 1; i >= 0; i--)
                    {
                        var ft = floatingTexts[i];
                        ft.Timer -= dt;
                        ft.Position.Y -= 40f * dt;
                        floatingTexts[i] = ft;
                        if (ft.Timer <= 0) floatingTexts.RemoveAt(i);
                    }
                    UpdateSplats(dt);
                    UpdateDeathFx(dt);
                    UpdateIncubation(dt);
                    UpdatePendingPet();
                    if (activePet != null) activePet.Update(dt, player.Center);

                     if (comboTimer > 0f)
                    {
                        comboTimer -= dt;
                        if (comboTimer <= 0f) comboCount = 0;
                    }

                    for (int i = lootDrops.Count - 1; i >= 0; i--)
{
    LootDrop drop = lootDrops[i];

    if (!drop.Collected && Raylib.CheckCollisionRecs(player.Bounds, drop.Bounds))
    {
        if (drop.OwnerId >= 0 && drop.OwnerId != multiplayer.MyId) continue; 
        string canonicalName = lootDropToItemName.GetValueOrDefault(drop.ItemType, drop.ItemType);

        if (TryGiveItem(canonicalName, 1))
        {
            drop.Collected = true;
            if (multiplayer.Connected && !multiplayer.IsHost)                    
                multiplayer.SendLootPickup(drop.Position.X, drop.Position.Y, drop.ItemType);
            if (multiplayer.Connected && multiplayer.IsHost)                    // host: despawn for clients
                multiplayer.SendLootDrop(drop.Position.X, drop.Position.Y, "__REMOVE__" + drop.ItemType, -2);
            floatingTexts.Add(new FloatingText {
                Position = player.Position - new Vector2(0, 40),
                Text = $"+1 {drop.ItemType}",
                Timer = 1.5f,
                TextColor = Color.Gold
            });
            lootDrops.RemoveAt(i);
        }
        else
        {
            floatingTexts.Add(new FloatingText {
                Position = player.Position - new Vector2(0, 40),
                Text = "Inventory full!",
                Timer = 1.2f,
                TextColor = Color.Red
            });
            // leave drop.Collected false and skip removal — stays on the ground
        }
    }
}

if (Vector2.Distance(player.Center, billboardPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    billboardOpen = !billboardOpen;

if (activeSideTask != null && !activeSideTask.ReadyToDeliver
    && activeSideTask.Progress() - activeSideTask.Baseline >= activeSideTask.Target)
{
    activeSideTask.ReadyToDeliver = true;
    ShowNotification($"{activeSideTask.Title} done! {activeSideTask.DeliverLabel} (press H)");
}

// NPC delivery — H near the named friend (also boosts friendship)
if (activeSideTask?.ReadyToDeliver == true && activeSideTask.DeliverTo.StartsWith("NPC:"))
{
    var target = friendNPCs.FirstOrDefault(f => f.Name == activeSideTask.DeliverTo[4..]);
    if (target != null && Vector2.Distance(player.Center, target.Npc.Position) < 60
        && Raylib.IsKeyPressed(KeyboardKey.H) && !chatInputOpen)
    {
        target.Friendship = Math.Min(100, target.Friendship + 5);
        CompleteSideTask(target.Name);
    }
}

if (playerDebt > 0f && !inPrison)
{
    debtDueTimer -= dt;
    if (debtDueTimer <= 0f)
    {
        // arrested — sentence length scales with unpaid debt
        inPrison = true;
        prisonSentenceTimer = playerDebt * PrisonSecondsPerDollar;
        prisonReturnPos = player.Position;
        player.Position = prisonCellCenter;
        ChangeScene(SceneState.World);
        ShowNotification($"Arrested for unpaid debt! Sentence: {(int)prisonSentenceTimer}s or pay ${playerDebt:0} bail.");
    }
}

if (inPrison)
{
    prisonSentenceTimer -= dt;
    if (prisonSentenceTimer <= 0f)
    {
        inPrison = false;
        playerDebt = 0f;
        player.Position = prisonReturnPos;
        ShowNotification("Sentence served. You're free to go.");
    }
}

if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
{
    if (!player.isSwinging && !player.InventoryOpen && !chestOpen && !shopUIOpen)
    {
        player.isSwinging = true;
        player.swingTimer = 0f;
        multiplayer.SendSwing();

        string equipped = GetActiveItem();   // respects phase (weapon or tool)
        if (equipped != null)
        {
            if (equipped.Contains("Sword"))        // Sword, Great Sword, Gold Sword, etc.
                Raylib.PlaySound(soundSwordSwing);
            else if (equipped.Contains("Stick"))
                Raylib.PlaySound(soundStickSwing);
            else if (equipped.Contains("Pickaxe"))  // check before Axe (Pickaxe contains "axe")
                Raylib.PlaySound(soundStickSwing);
            else if (equipped.Contains("Axe"))      // Axe, War Axe, Gold Axe, etc.
                Raylib.PlaySound(soundStickSwing);
            else if (equipped.Contains("Rod"))
                Raylib.PlaySound(soundStickSwing);
        }
    }
}

                    foreach (Enemy enemy in enemies)
                    {
                        bool enemyAuthority = !multiplayer.Connected || multiplayer.IsHost;
                        Vector2 enemyOldPos = enemy.Position;
                        if (!enemyAuthority && enemy.FlashTimer > 0f)
                            enemy.FlashTimer -= dt;
                        Vector2 nearest = player.Center;   
                        if (enemyAuthority)
                        {
                            float bestDist = Vector2.DistanceSquared(enemy.Center, player.Center);
                            if (multiplayer.Connected)
                            {
                                lock (multiplayer.RemotePlayers)
                                    foreach (var rp in multiplayer.RemotePlayers)
                                    {
                                        if (!rp.Active || rp.Scene != "World") continue;
                                        Vector2 rpPos = new Vector2(rp.X, rp.Y);
                                        float d = Vector2.DistanceSquared(enemy.Center, rpPos);
                                        if (d < bestDist) { bestDist = d; nearest = rpPos; }
                                    }
                            }
                            enemy.Update(dt, nearest);
                        }

                        // CHANGED: gate firing to the authority, aim at the aggro'd (nearest) player
                        if (enemyAuthority && !enemy.Dead && enemy.Aggro && enemy.AttackCooldown <= 0f)
                        {
                            float dToPlayer = Vector2.Distance(enemy.Center, nearest);
                            Vector2 aim = Vector2.Normalize(nearest - enemy.Center);

                            if (enemy.Type == "Wizard" && dToPlayer <= 420f)
                            {
                                Vector2 vel = aim * 340f;
                                enemyProjectiles.Add(new EnemyProjectile(enemy.Center, vel, "Spell", 3f, 12));
                                if (multiplayer.Connected)
                                    multiplayer.SendEnemyProjectile(enemy.Center.X, enemy.Center.Y, vel.X, vel.Y, 3f, "Spell", 12);
                                enemy.AttackCooldown = 1.6f;
                            }
                            else if (enemy.Type == "Archer" && dToPlayer <= 420f)
                            {
                                Vector2 vel = aim * 520f;
                                enemyProjectiles.Add(new EnemyProjectile(enemy.Center, vel, "Arrow", 3f, 10));
                                if (multiplayer.Connected)
                                    multiplayer.SendEnemyProjectile(enemy.Center.X, enemy.Center.Y, vel.X, vel.Y, 3f, "Arrow", 10);
                                enemy.AttackCooldown = 1.2f;
                            }
                            // (Warrior + other melee handled by the contact-damage block below)
                        }

                        // collide with fences, rocks, trees — revert on overlap
                        bool blocked = false;
                        foreach (Rectangle fenceRect in fenceManager.GetCollisionRects())
                            if (Raylib.CheckCollisionRecs(enemy.Bounds, fenceRect)) { blocked = true; break; }
                        if (!blocked)
                            foreach (RockObject rock in rocks)
                                if (!rock.Broken && Raylib.CheckCollisionRecs(enemy.Bounds, rock.Bounds)) { blocked = true; break; }
                        if (!blocked)
                            foreach (TreeObject tree in trees)
                                if (!tree.Chopped && Raylib.CheckCollisionRecs(enemy.Bounds, tree.Bounds)) { blocked = true; break; }
                        if (blocked)
                            enemy.Position = enemyOldPos;
                        
                        if (!enemy.Dead && enemy.AttackCooldown <= 0f   && Raylib.CheckCollisionRecs(player.Bounds, enemy.Bounds))
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
                else if (enemy.Type == "Warrior")       damage = 16;
                else if (enemy.Type == "Goblin")        damage = 7;
                else if (enemy.Type == "Thug")          damage = 10;
                else if (enemy.Type == "Robber")        damage = 9;
                else if (enemy.Type == "Gangster")      damage = 12;
                else if (enemy.Type == "Giant Bug")     damage = 14;

                int defense = GetTotalDefense();
                int reducedDamage = Math.Max(1, damage - defense);
                player.TakeDamage(reducedDamage);
                enemy.AttackCooldown = 1.0f;   
                TriggerShake(0.2f);

                floatingTexts.Add(new FloatingText {
                    Position = player.Position - new Vector2(0, 20),
                    Text = $"-{damage}",
                    Timer = 1f,
                    TextColor = Color.Red
                });
            }
                        if (!enemy.Dead && Vector2.Distance(player.Center, enemy.Center) < 80)
                {
                    nearEnemy = true;


 if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    string equipped = GetActiveWeapon();
    bool hasWeapon = equipped != null && GetItemSlot(equipped) == "WEAPON";

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
        int weaponBonus = GetWeaponDamage(equipped);
        int slotBonus   = GetWeaponDamage(armorWeapon);
        int attackDamage = 1 + (player.CombatLevel / 10) + weaponBonus + ComboDamageBonus() + GetArmorStyleBonus().melee;

        // crit roll
        bool isCrit = Raylib.GetRandomValue(1, 100) <= (int)(critChance * 100);
        if (isCrit) attackDamage = (int)(attackDamage * critMultiplier);

        if (!multiplayer.Connected || multiplayer.IsHost)
        {
            enemy.LastDamagerId = multiplayer.MyId;
            enemy.Health -= attackDamage;                       // host/singleplayer: apply directly
            if (enemy.Health <= 0) HandleEnemyDeath(enemy);
        }
        else
        {
            multiplayer.SendEnemyHit(enemies.IndexOf(enemy), attackDamage, multiplayer.MyId);
        }

        AwardMeleeXP(equipped, Math.Max(1, Math.Min(attackDamage, enemy.Health + attackDamage)));
        enemy.TriggerFlash();
        SpawnSplat(enemy.Center, new Color((byte)170, (byte)20, (byte)20, (byte)255), isCrit ? 16 : 8);
        RegisterComboHit();

        // dog hit sound
        if (enemy.Type == "Wild Dog")
            Raylib.PlaySound(soundDogHit);

        floatingTexts.Add(new FloatingText {
            Position = enemy.Position - new Vector2(0, 20),
            Text = isCrit ? $"CRIT -{attackDamage}!" : $"-{attackDamage}",
            Timer = isCrit ? 1.3f : 1f,
            TextColor = isCrit ? Color.Gold : (equipped == "Sword" ? Color.Orange : Color.Red)
        });

        TriggerShake(isCrit ? 0.22f : 0.1f);

        if (enemy.Health <= 0)
        {    
            HandleEnemyDeath(enemy);   
        }
        
            }
        }
    }
}

 // Space = fire in facing direction
    if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen
        && currentPhase == HandPhase.Combat
        && IsRangedWeapon(equipped2H))
    {
        Vector2 facingDir = player.Facing switch
        {
            Player.FacingDirection.Up    => new Vector2(0, -1),
            Player.FacingDirection.Down  => new Vector2(0,  1),
            Player.FacingDirection.Left  => new Vector2(-1, 0),
            Player.FacingDirection.Right => new Vector2(1,  0),
            _ => new Vector2(1, 0)
        };
        TryFireProjectile(facingDir);
    }

    // Left click = fire toward mouse world position
    if (Raylib.IsMouseButtonPressed(MouseButton.Left)
        && currentPhase == HandPhase.Combat
        && IsRangedWeapon(equipped2H))
    {
        Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        Vector2 mouseDir = Vector2.Normalize(mouseWorld - player.Center);
        TryFireProjectile(mouseDir);
    }

    UpdateProjectiles(dt);
    UpdateRemoteVisualProjectiles(dt);
    

    // Space = spell in facing direction
     if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen
        && currentPhase == HandPhase.Combat
        && (equipped1H != null && equipped1H.EndsWith("Staff")
            || equipped2H != null && equipped2H.EndsWith("Staff")))
    {
        Vector2 facingDir = player.Facing switch
        {
            Player.FacingDirection.Up    => new Vector2(0, -1),
            Player.FacingDirection.Down  => new Vector2(0,  1),
            Player.FacingDirection.Left  => new Vector2(-1, 0),
            Player.FacingDirection.Right => new Vector2(1,  0),
            _ => new Vector2(1, 0)
        };
        TryFireSpell(facingDir);
    }

    // Left click = spell toward mouse
    if (Raylib.IsMouseButtonPressed(MouseButton.Left)
        && currentPhase == HandPhase.Combat
        && (equipped1H != null && equipped1H.EndsWith("Staff")
            || equipped2H != null && equipped2H.EndsWith("Staff")))
    {
        Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        Vector2 mouseDir = Vector2.Normalize(mouseWorld - player.Center);
        TryFireSpell(mouseDir);
    }

    UpdateSpellProjectiles(dt);
    UpdateEnemyProjectiles(dt);
    SyncEnemiesIfHost(dt);
    SyncWorldClockIfHost(dt); 
    UpdateWorldBoss(worldBoss, dt);    
     UpdateWorldBoss(superBoss, dt);

    player.Update(dt, buildings, trees, vehicles, lakes, decorativeBuildings, decorativeAssets);

     if (buildingPromptTimer > 0) buildingPromptTimer -= dt; 
     if (buildingPromptTimer <= 0)
{
    buildingPromptLocked = false;
}

// elevation interpolation
        playerElevationTarget = 0f;
        foreach (var ez in elevationZones)
            if (Raylib.CheckCollisionPointRec(player.Center, ez.Bounds))
            {
                playerElevationTarget = ez.Kind == ElevationType.StairsDown
                    ? ez.HeightPx : -ez.HeightPx;
                break;
            }
        playerElevation = float.Lerp(playerElevation, playerElevationTarget, dt * 6f);

                    // BUS SYSTEM
{
    float hour = GetCurrentHour();
    busOperating = hour >= 6f && hour < 12f;

    if (busOperating)
    {
        if (!busMoving)
        {
            busRouteTimer += daySpeed * Raylib.GetFrameTime() * 24f; // advance in game hours
            if (busRouteTimer >= 4f)
            {
                busRouteTimer = 0f;
                busMoving = true;
                busLerpT = 0f;
                busNextStop = (busCurrentStop + 1) % busStops.Length;
            }
        }
        else
        {
            busLerpT += dt * 0.4f; // travel speed
            busPosition = Vector2.Lerp(busStops[busCurrentStop].WorldPos,
                                       busStops[busNextStop].WorldPos,
                                       Math.Min(busLerpT, 1f));
            if (busLerpT >= 1f)
            {
                busCurrentStop = busNextStop;
                busPosition = busStops[busCurrentStop].WorldPos;
                busMoving = false;
                busRouteTimer = 0f;
            }
        }
    }
    else
    {
        // parked at stop 0 overnight
        busPosition = busStops[0].WorldPos;
        busCurrentStop = 0;
        busMoving = false;
        busRouteTimer = 0f;
    }

        // board bus
        if (!busMoving && busOperating && Vector2.Distance(player.Center, busPosition) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) busMenuOpen = !busMenuOpen;
        }
        else if (Vector2.Distance(player.Center, busPosition) >= 80)
            busMenuOpen = false;
}

foreach (Vehicle vehicle in vehicles)
{
    Vector2 vehicleOldPos = vehicle.Position;
    vehicle.Update(dt, buildings, trees, vehicles, decorativeBuildings, rocks, decorativeAssets);

    foreach (Rectangle fenceRect in fenceManager.GetCollisionRects())
    {
        if (Raylib.CheckCollisionRecs(vehicle.Bounds, fenceRect))
        {
            vehicle.Position = vehicleOldPos;
            vehicle.velocity = Vector2.Zero;
            break;
        }
    }
    vehicle.OnRoad = IsOnRoad(vehicle.Center);

    if (vehicle.Driving && vehicle.Fuel > 0)
        player.AddDrivingXP(1);

    if (vehicle.Driving)
    {
        player.Position = vehicle.Center - new Vector2(player.Bounds.Width / 2f, player.Bounds.Height / 2f);
        player.Hidden = true;

        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            vehicle.Driving = false;
            player.Hidden = false;
            Rectangle vb = vehicle.Bounds;
            player.Position = vehicle.Facing switch
            {
                Vehicle.FacingDirection.Left  => new Vector2(vb.X + vb.Width + 20, vb.Y + vb.Height / 2),
                Vehicle.FacingDirection.Right => new Vector2(vb.X - 60,            vb.Y + vb.Height / 2),
                Vehicle.FacingDirection.Up    => new Vector2(vb.X + vb.Width / 2,  vb.Y + vb.Height + 20),
                _                             => new Vector2(vb.X + vb.Width / 2,  vb.Y - 80),
            };

           if (vehicle.IsWatercraft && player.SwimmingLevel < 5 && IsInWater(player.Position))
            {
                player.Position = boatBoardPos != Vector2.Zero
                    ? boatBoardPos
                    : new Vector2(oceanBounds.X - 60, player.Position.Y);   // fallback: nearest shore, same latitude
                ShowNotification("You struggle back to shore, exhausted!");
            }
            multiplayer.SendMount("", false);

            var garage = garages.FirstOrDefault(g =>
                Raylib.CheckCollisionRecs(vehicle.Bounds, g.Bounds) &&
                g.IsDock == vehicle.IsWatercraft); 
            if (garage != null)
            {
                if (garage.HasVehicle)
                {
                    ShowNotification("Garage is too full!");
                }
                else
                {
                    garage.Slots.Add((vehicle.Type, vehicle.VehicleColor));
                    vehiclesToRemove.Add(vehicle);
                    ShowNotification($"{vehicle.Type} saved to garage!");
                }
            }
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
                        Vector2 pushDir = Vector2.Normalize(player.Position - vehicle.Position);
                        if (pushDir == Vector2.Zero) pushDir = new Vector2(1, 0);
                        player.Position += pushDir * 60f;
                    }
                    else
                    {
                        if (vehicle.IsWatercraft)
                        {
                            if (!IsInWater(player.Position))
                            {
                                levelUpMessage = "You need to be in the water to board a boat!";
                                levelUpTimer = 2f;
                            }
                            else if (!hasBoatTheory)
                            {
                                levelUpMessage = "You need a Boat Licence! Visit the Boat Licence Office.";
                                levelUpTimer = 2.5f;
                            }
                            else
                            {
                                vehicle.Driving = true;
                                player.Hidden = true;
                                boatBoardPos = player.Position;
                                multiplayer.SendMount(vehicle.Type.ToString(), true);
                            }
                        }
                        else
                        {
                            if (!HasTheoryForClass(VehicleRequiredClass(vehicle.Type)))
                            {
                                levelUpMessage = "You need a licence to drive bro! Visit the AA.";
                                levelUpTimer = 2.5f;
                            }
                            else if (player.DrivingLevel < VehicleUnlockLevel(vehicle.Type))
                            {
                                levelUpMessage = $"Requires Driving level {VehicleUnlockLevel(vehicle.Type)} to drive a {vehicle.Type}!";
                                levelUpTimer = 2.5f;
                            }
                            else if (!CanDriveVehicleNow(vehicle.Type))
                            {
                                levelUpMessage = $"Restricted licence — can only drive 6am-10pm. Current time: {GetTimeString()}";
                                levelUpTimer = 3f;
                            }
                            else
                            {
                                vehicle.Driving = true;
                                player.Hidden = true;
                                multiplayer.SendMount(vehicle.Type.ToString(), true);
                            }
                        }
                        player.Hidden = false;
                    }
                }
            }
        }
    }
}

foreach (var v in vehiclesToRemove) vehicles.Remove(v);
vehiclesToRemove.Clear();

            foreach (Rideable rideable in rideables)
{
    Vector2 rideableOldPos = rideable.Position;
    rideable.Update(dt, buildings, trees, vehicles, rideables, decorativeBuildings, decorativeAssets);
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
            multiplayer.SendMount("", false);
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
                 multiplayer.SendMount(rideable.Type.ToString(), false);
            }
        }
    }

var stable = stables.FirstOrDefault(s => Raylib.CheckCollisionRecs(rideable.Bounds, s.Bounds));
            if (stable != null)
            {
                if (!stable.Accepts(rideable.Type))
                    ShowNotification(stable.Kind == Stable.StableKind.Basic
                        ? "This stable only takes horses, donkeys and camels!"
                        : "This enclosure can't hold that animal!");
                else if (stable.IsFull)
                    ShowNotification("No room in the stable!");
                else
                {
                    stable.Slots.Add((rideable.Type, rideable.RideableColor));
                    rideablesToRemove.Add(rideable);  
                    ShowNotification($"{rideable.Type} stabled! ({stable.Slots.Count}/{stable.Capacity})");
                }
            }

if (rideable.Riding)
{
    if (rideable.Type == Rideable.RideableType.Horse || rideable.Type == Rideable.RideableType.Donkey || rideable.Type == Rideable.RideableType.Camel
    || rideable.Type == Rideable.RideableType.Tiger || rideable.Type == Rideable.RideableType.Elephant || rideable.Type == Rideable.RideableType.Reindeer
    || rideable.Type == Rideable.RideableType.Dolphin )
        player.AddRidingXP(1);
    else
        player.AddCyclingXP(1);

    if (rideable.Type == Rideable.RideableType.MountainBike || rideable.Type == Rideable.RideableType.BMX)
    player.AddStaminaXP(1); 
}

if (rideable.IsAnimal)
{
    string feed = AnimalFeedFor(rideable.Type);
    bool nearAnimal = rideable.Riding || Vector2.Distance(player.Center, rideable.Position) < 90;

    if (feed != null && nearAnimal && rideable.Stamina < rideable.MaxStamina
        && toolbarSlots[toolbarSelectedSlot] == feed
        && Raylib.IsKeyPressed(KeyboardKey.R) && !chatInputOpen)
    {
        toolbarCounts[toolbarSelectedSlot]--;
        if (toolbarCounts[toolbarSelectedSlot] <= 0) toolbarSlots[toolbarSelectedSlot] = null;
        rideable.Stamina = Math.Min(rideable.MaxStamina, rideable.Stamina + 50f);
        ShowNotification($"Fed the {rideable.Type} some {feed}! (+50 stamina)");
    }

    // elephants recharge at any watering hole (lake)
    if (rideable.Type == Rideable.RideableType.Elephant
        && lakes.Any(l => Vector2.Distance(rideable.Position, l.Position) < 220))
        rideable.Stamina = Math.Min(rideable.MaxStamina, rideable.Stamina + 12f * dt);
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
        float distPlayerP1  = Vector2.Distance(player.Center,  station.Pump1Pos);
        float distPlayerP2  = Vector2.Distance(player.Center,  station.Pump2Pos);

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

if (currentBuilding?.BuildingName == "FamilyHub"){
    foreach (var o in FamilyHubNPCs) o.Update(dt);
}

    foreach (var f in friendNPCs)
{
    f.Npc.Update(dt);
    if (Vector2.Distance(player.Center, f.Npc.Position) < 60 && !chatInputOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !f.TalkedToday)
        {
            f.TalkedToday = true;
            f.Friendship = Math.Min(100, f.Friendship + 3);
            ShowNotification($"{f.Name} ({f.Tier}): \"{f.TierDialogue}\" (+3 friendship)");
        }
        if (Raylib.IsKeyPressed(KeyboardKey.G) && !f.GiftedToday && GetResourceCount(f.FavoriteGift) > 0)
        {
            f.GiftedToday = true;
            SpendResource(f.FavoriteGift, 1);
            f.Friendship = Math.Min(100, f.Friendship + 10);
            ShowNotification($"{f.Name} loved the {f.FavoriteGift}! (+10 friendship)");
        }
        if (f.Friendship >= 100 && !f.RewardGiven)
        {
            f.RewardGiven = true;
            player.Money += 500;
            ShowNotification($"{f.Name} is your Best Friend! They gift you $500!");
        }
    }
}

    foreach (RockObject rock in rocks)
{
    rock.Update(dt);

    if (!rock.Broken && !nearEnemy)
    {
        if (Vector2.Distance(player.Center, rock.Position) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen && currentPhase == HandPhase.Tools)
            {
                if (BaseTool(GetEquippedTool()) != "Pickaxe")
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
    int oreGained = Raylib.GetRandomValue(1, 3);

    if (TryGiveItem(rock.OreType, oreGained))
    {
        player.AddMiningXP(rock.XPReward);
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
    else
    {
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 30),
            Text = "Inventory full!",
            Timer = 1.2f,
            TextColor = Color.Red
        });
    }
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
        if (Vector2.Distance(player.Center, tree.Center) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen && currentPhase == HandPhase.Tools)
            {
                if (GetEquippedTool() != "Axe")
                {
                    buildingPromptMessage = player.HasAxe ? "Equip your Axe!" : "Need an axe!";
                    buildingPromptColor = Color.Red;
                    buildingPromptTimer = 1.5f;
                    buildingPromptLocked = true;
                }
                else if (player.WoodcuttingLevel < tree.LevelRequired)
                {
                    buildingPromptMessage = $"Need Woodcutting level {tree.LevelRequired}!";
                    buildingPromptColor = Color.Red;
                    buildingPromptTimer = 1.5f;
                    buildingPromptLocked = true;
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
    int logsGained = Raylib.GetRandomValue(1, 3);

    if (TryGiveItem(tree.LogType, logsGained))
    {
        player.AddWoodcuttingXP(tree.XPReward);
        TriggerShake(0.15f);
        buildingPromptMessage = $"+{tree.XPReward} WC XP  +{logsGained} {tree.LogType}";
        buildingPromptColor = Color.Yellow;
        buildingPromptTimer = 1.5f;
    }
    else
    {
        buildingPromptMessage = "Inventory full! Couldn't collect the wood.";
        buildingPromptColor = Color.Red;
        buildingPromptTimer = 1.5f;
    }
}
                }
            }
        }
    }
}

// ── INTERACTABLE BUILDING ENTRY PROMPTS ───────────────────────────────────
foreach (Building building in buildings)
{
    float distToBuilding = Vector2.Distance(player.Center, new Vector2(
        building.Bounds.X + building.Bounds.Width / 2,
        building.Bounds.Y + building.Bounds.Height));

    if (distToBuilding < 20)
    {
        if (!buildingPromptLocked)
        {
            buildingPromptMessage = $"Press E to enter {building.BuildingName}";
            buildingPromptColor = new Color((byte)255,(byte)215,(byte)0,(byte)255);
            buildingPromptTimer = 0.1f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            // your existing entry logic here
        }
    }
}

// ── DECORATIVE BUILDING PROMPTS ───────────────────────────────────────────
foreach (DecorativeBuilding building in decorativeBuildings)
{
    float distToBuilding = Vector2.Distance(player.Center, new Vector2(
        building.Bounds.X + building.Bounds.Width / 2,
        building.Bounds.Y + building.Bounds.Height));

    if (distToBuilding < 20)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            buildingPromptMessage = "You can't enter this building!";
            buildingPromptColor = Color.Red;
            buildingPromptTimer = 2.0f;
            buildingPromptLocked = true; // lock it so proximity can't overwrite
        }
    }
}

// ── UNLOCK when important message expires ─────────────────────────────────
if (buildingPromptTimer <= 0)
{
    buildingPromptLocked = false;
}

// check mailboxes for collecting a delivered ID
        nearMailbox = false;
        nearMailboxHasMail = false;

        // house-plot mailboxes (your ORIGINAL loop — keep this)
        for (int i = 0; i < ownedHousePlots.Count; i++)
        {
            Vector2 mboxPos = new Vector2(ownedHousePlots[i].x + 53, ownedHousePlots[i].y + 161);
            if (Vector2.Distance(player.Center, mboxPos) < 90)
            {
                bool thisHasMail = idMailWaiting && idTargetHouseIndex == i;
                nearMailbox = true;
                nearMailboxHasMail = thisHasMail;
                if (Raylib.IsKeyPressed(KeyboardKey.E))
                    {
                        if (thisHasMail)
                        {
                            idMailWaiting = false;
                            idClaimed = true;
                            ShowNotification($"ID collected! Issued {idIssuedDate}.");
                        }
                        bool licAtThisBox = false;
                        int licIdxHere = -1;
                        for (int li = 0; li < 5; li++)
                        {
                            if (licenceMailWaiting[li] && licenceTargetHouse[li] == i)
                            { licAtThisBox = true; licIdxHere = li; break; }
                        }
                        if (licAtThisBox) nearMailboxHasMail = true;

                        bool collectedSomething = thisHasMail || licAtThisBox;
                        if (licAtThisBox)
                        {
                            licenceMailWaiting[licIdxHere] = false;
                            if (licencePendingIsTheory[licIdxHere])
                                licenceTheoryDelivered[licIdxHere] = true;
                            else
                                licencePracticalDelivered[licIdxHere] = true;
                            ShowNotification("Licence collected!");
                        }
                        else if (!collectedSomething)
                        {
                            ShowNotification("Mailbox is empty.");
                        }
                    }
            }
        }

        // extra mailboxes interaction (farmhouse etc. — ADDED alongside)
        foreach (var (pos, houseIndex) in extraMailboxes)
        {
            Vector2 mboxCenter = pos + new Vector2(13, 11);
            if (Vector2.Distance(player.Center, mboxCenter) < 90)
            {
                bool thisHasMail = idMailWaiting && idTargetHouseIndex == houseIndex;
                nearMailbox = true;
                nearMailboxHasMail = thisHasMail;
                if (Raylib.IsKeyPressed(KeyboardKey.E))
                {
                    if (thisHasMail)
                    {
                        idMailWaiting = false;
                        idClaimed = true;
                        ShowNotification($"ID collected! Issued {idIssuedDate}.");
                    }
                    bool licAtThisBox = false;
                    int licIdxHere = -1;
                    for (int li = 0; li < 5; li++)
                    {
                        if (licenceMailWaiting[li] && licenceTargetHouse[li] == houseIndex)
                        { licAtThisBox = true; licIdxHere = li; break; }
                    }
                    if (licAtThisBox) nearMailboxHasMail = true;

                    bool collectedSomething = thisHasMail || licAtThisBox;
                    if (licAtThisBox)
                    {
                        licenceMailWaiting[licIdxHere] = false;
                        if (licencePendingIsTheory[licIdxHere])
                            licenceTheoryDelivered[licIdxHere] = true;
                        else
                            licencePracticalDelivered[licIdxHere] = true;
                        ShowNotification("Licence collected!");
                    }
                    else if (!collectedSomething)
                    {
                        ShowNotification("Mailbox is empty.");
                    }
                }
            }
        }
string held = toolbarSlots[toolbarSelectedSlot];
var toolTier = held == "Spade" || held?.Contains("Spade") == true
    ? spadeTiers.FirstOrDefault(t => t.Name == held)
    : wateringCanTiers.FirstOrDefault(t => t.Name == held);
int range = toolTier?.Range ?? 1;

var nearbyPlots = farmPlots
    .Where(p => Vector2.Distance(player.Center, p.Position) < 60)
    .OrderBy(p => Vector2.Distance(player.Center, p.Position))
    .ThenBy(p => farmPlots.IndexOf(p))
    .Take(range)
    .ToList();

foreach (var plot in nearbyPlots)
{
    string held2 = toolbarSlots[toolbarSelectedSlot];

    if (plot.ReadyToHarvest && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        AddOneItemToToolbar(plot.CropType);
        player.AddFarmingXP(20);
        cropsHarvested++;
        ShowNotification($"Harvested {plot.CropType}! +20 Farming XP");
        plot.Tilled = false; plot.Planted = false; plot.Watered = false;
        plot.ReadyToHarvest = false; plot.GrowTimer = 0f; plot.CropType = "";
    }
    else if (!plot.Tilled && held2 == "Spade" && farmToolCooldown <= 0f && Raylib.IsKeyPressed(KeyboardKey.Space))
{
    plot.Tilled = true;
    farmToolCooldown = FarmToolCooldownDuration;
    ShowNotification("Tilled the soil.");
}
    // planting interaction — CHANGED, this is the fix from earlier — confirm this exact version is in place
    else if (plot.Tilled && !plot.Planted && held2 != null && seedToCrop.ContainsKey(held2) && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        plot.Planted = true;
        plot.CropType = seedToCrop[held2];
        plot.GrowDuration = cropGrowDuration.GetValueOrDefault(plot.CropType, 30f);

        int slot = toolbarSelectedSlot;
        toolbarCounts[slot]--;                              // decrements the actual seed stack
        if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;

        ShowNotification($"Planted {plot.CropType}.");
    }
    // farm plot watering interaction — CHANGED to consume/require charge
else if (plot.Planted && !plot.Watered && held2 == "Watering Can" && farmToolCooldown <= 0f && Raylib.IsKeyPressed(KeyboardKey.Space))
{
    int slot = toolbarSelectedSlot;
    if (toolbarWaterCharge[slot] >= 1f)
    {
        toolbarWaterCharge[slot] -= 1f;
        plot.Watered = true;
        farmToolCooldown = FarmToolCooldownDuration;
        ShowNotification("Watered the crop.");
    }
    else
    {
        ShowNotification("Watering can is empty! Refill at water.");
    }
}
}


string heldForTrees = toolbarSlots[toolbarSelectedSlot];
foreach (var tree in fruitTrees)
{
    if (Vector2.Distance(player.Center, tree.Position) > 70) continue;

    if (!tree.Tilled && !tree.Planted && heldForTrees == "Spade" && farmToolCooldown <= 0f && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        tree.Tilled = true;
        farmToolCooldown = FarmToolCooldownDuration;
        ShowNotification("Tilled the tree plot.");
    }
    else if (tree.Tilled && !tree.Planted && heldForTrees == "Apple Tree Seed" && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        tree.Planted = true;
        tree.FruitType = "Apple";
        tree.GrowDuration = 90f;
        int slot = toolbarSelectedSlot;
        toolbarCounts[slot]--; if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
        ShowNotification("Planted an Apple Tree seed.");
    }
    else if (tree.Tilled && !tree.Planted && heldForTrees == "Banana Tree Seed" && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        tree.Planted = true;
        tree.FruitType = "Banana";
        tree.GrowDuration = 100f;
        int slot = toolbarSelectedSlot;
        toolbarCounts[slot]--; if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
        ShowNotification("Planted a Banana Tree seed.");
    }
    else if (tree.Planted && BaseTool(heldForTrees) == "Axe" && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        treeChopConfirmOpen = true;
        treeChopTarget = tree;
    }
    else if (tree.ReadyToHarvest && tree.RegrowTimer <= 0f && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        AddOneItemToToolbar(tree.FruitType);
        player.AddFarmingXP(30);
        ShowNotification($"Picked {tree.FruitType}! +30 Farming XP");
        tree.RegrowTimer = 1f;
    }
}

foreach (var pen in livestockPens.Where(p => Vector2.Distance(player.Center, p.Position) < 70))
{
    if (pen.ReadyToHarvest && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        AddOneItemToToolbar(pen.Produce);
        player.AddFarmingXP(25);
        pen.ReadyToHarvest = false;
        ShowNotification($"Collected {pen.Produce}! +25 Farming XP");
    }
    else if (!pen.Fed && !pen.ReadyToHarvest && Raylib.IsKeyPressed(KeyboardKey.F))
    {
        if (GetItemCount(pen.Feed) > 0 || ToolInToolbar(pen.Feed))
        {
            BackpackRemoveOne(pen.Feed);   // see note below
            pen.Fed = true;
            ShowNotification($"Fed the {pen.Animal}. It'll produce {pen.Produce} soon.");
        }
        else ShowNotification($"Need {pen.Feed} to feed this {pen.Animal}.");
    }
}

foreach (Lake lake in lakes)
{
    lake.Update(dt);

    if (Vector2.Distance(player.Center, lake.Position) < 200 && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen && !isFishing)
    {
        currentLake = lake;
        TryStartFishing("Lake");
    }
}

// refill near open water — ADD alongside your lake/river update loops
foreach (Lake lake in lakes)
{
    if (Vector2.Distance(player.Center, lake.Position) < 200
        && toolbarSlots[toolbarSelectedSlot] == "Watering Can"
        && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
    {
        toolbarWaterCharge[toolbarSelectedSlot] = WateringCanMaxUses;
        ShowNotification("Refilled Watering Can!");
    }
}

UpdateCanteenFill();

foreach (var river in rivers)
{
    Rectangle rb = river.Bounds;
    Rectangle nearZone = new Rectangle(rb.X - 90, rb.Y - 90, rb.Width + 180, rb.Height + 180);
    if (Raylib.CheckCollisionPointRec(player.Center, nearZone)
        && toolbarSlots[toolbarSelectedSlot] == "Watering Can"
        && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
    {
        toolbarWaterCharge[toolbarSelectedSlot] = WateringCanMaxUses;
        ShowNotification("Refilled Watering Can!");
    }
}

// river fishing
        nearRiver = false;
        foreach (var river in rivers)
        {
            // near if within ~90px of the river band
            Rectangle rb = river.Bounds;
            Rectangle nearZone = new Rectangle(rb.X - 90, rb.Y - 90, rb.Width + 180, rb.Height + 180);
            if (Raylib.CheckCollisionPointRec(player.Center, nearZone) && !isFishing)
            {
                nearRiver = true;
                if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
                    TryStartFishing("River");
                break;
            }
        }

// refill near ocean biome — ADD near your world-update block, using existing GetCurrentBiome()
if (GetCurrentBiome() == "OCEAN"
    && toolbarSlots[toolbarSelectedSlot] == "Watering Can"
    && Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
{
    toolbarWaterCharge[toolbarSelectedSlot] = WateringCanMaxUses;
    ShowNotification("Refilled Watering Can!");
}

// ID delivery countdown — one full day cycle
        if (idPending)
        {
            idDeliveryTimer -= dt;
            if (idDeliveryTimer <= 0f)
            {
                idPending = false;
                idMailWaiting = true;   // ID has arrived at the chosen mailbox
                ShowNotification("Your ID has arrived! Check your mailbox.");
            }
        }


for (int li = 0; li < 5; li++)
{
    if (!licencePending[li]) continue;
    licenceDeliveryTimer[li] -= daySpeed * dt;
    if (licenceDeliveryTimer[li] <= 0f)
    {
        licencePending[li] = false;
        licenceMailWaiting[li] = true;
        ShowNotification("A licence has arrived! Check your mailbox.");
    }
}

if (isFishing)
{
    if (fishingResultTimer > 0) fishingResultTimer -= dt;

    // ── PHASE 0: waiting for a bite ──
    if (fishingPhase == 0)
    {
        fishingTimer += dt;
        if (fishingTimer >= fishingBiteTime)
        {
            fishingPhase = 1;
            fishingReactWindow = 0.9f;   // must press Space within this window
            fishingResult = "BITE! Press SPACE!";
            fishingResultTimer = 1f;
        }
    }
    // ── PHASE 1: react to the bite ──
    else if (fishingPhase == 1)
    {
        fishingReactWindow -= dt;
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
        {
            // hooked! start the reeling minigame
            fishingPhase = 2;
            reelBarPos = 0f;
            reelBarUp = true;
            // green zone size: bigger (easier) at low fishing level, set a tidy default
            float zone = 0.22f;
            float centre = Raylib.GetRandomValue(25, 75) / 100f;
            reelTargetMin = Math.Clamp(centre - zone / 2f, 0f, 1f);
            reelTargetMax = Math.Clamp(centre + zone / 2f, 0f, 1f);
        }
        else if (fishingReactWindow <= 0)
        {
            // missed the bite
            fishingResult = "The fish got away...";
            fishingResultTimer = 2f;
            EndFishing();
        }
    }
    // ── PHASE 2: reeling timing bar ──
    else if (fishingPhase == 2)
    {
        // bar bounces up and down
        reelBarPos += (reelBarUp ? 1f : -1f) * 1.4f * dt;
        if (reelBarPos >= 1f) { reelBarPos = 1f; reelBarUp = false; }
        if (reelBarPos <= 0f) { reelBarPos = 0f; reelBarUp = true; }

        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
        {
            bool inZone = reelBarPos >= reelTargetMin && reelBarPos <= reelTargetMax;
            // how centred? closer to middle of the zone = better catch
            float centre = (reelTargetMin + reelTargetMax) / 2f;
            float accuracy = 1f - Math.Abs(reelBarPos - centre) / 0.5f;
            accuracy = Math.Clamp(accuracy, 0f, 1f);

         if (inZone)
{
    string tool = GetEquippedTool();
    FishSpecies caught = RollFish(fishingWater, BaseTool(tool));
    if (caught == null)
    {
        fishingResult = "Nothing biting here with that tool.";
        fishingResultTimer = 2.5f;
        EndFishing();
    }
    else
{
    int fishCount = accuracy > 0.8f ? 2 : 1;
    int xp = 15 + (int)(accuracy * 25f) + caught.Value / 2;

    if (TryGiveItem("Fish", fishCount))
    {
        player.TrackFishSpecies(caught.Name, fishCount);
        player.AddFishingXP(xp);

        string quality = accuracy > 0.8f ? "PERFECT!" : "Caught";
        fishingResult = $"{quality} {fishCount}x {caught.Name} (worth ${caught.Value})  +{xp} XP";
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 20),
            Text = $"+{caught.Name}!",
            Timer = 1.4f,
            TextColor = new Color((byte)0,(byte)206,(byte)209,(byte)255)
        });
    }
    else
    {
        fishingResult = "Inventory full! Couldn't keep the catch.";
    }
}
fishingResultTimer = 3f;
EndFishing();
    }
  }

    // cancel anytime with Q
    if (Raylib.IsKeyPressed(KeyboardKey.Q))
    {
        fishingResult = "Stopped fishing.";
        fishingResultTimer = 1.5f;
        EndFishing();
    }
}
}

foreach (var garage in garages)
{
    Vector2 garageCenter = new Vector2(garage.Bounds.X + garage.Bounds.Width / 2, garage.Bounds.Y + garage.Bounds.Height / 2);
    if (Vector2.Distance(player.Center, garageCenter) > 150) continue;

    if (garage.HasVehicle && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        var slot = garage.Slots[garage.Slots.Count - 1];                   // CHANGED — pop last stored
        var newVehicle = new Vehicle(garageCenter, slot.color, DefaultSpeedFor(slot.type), slot.type);
        vehicles.Add(newVehicle);
        garage.Slots.RemoveAt(garage.Slots.Count - 1); 
        ShowNotification($"{newVehicle.Type} retrieved from garage!");
    }
}

foreach (var stable in stables)
{
    Vector2 sc = new Vector2(stable.Bounds.X + stable.Bounds.Width / 2, stable.Bounds.Y + stable.Bounds.Height / 2);
    if (Vector2.Distance(player.Center, sc) > 150) continue;

    if (stable.HasAnimal && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    {
        var slot = stable.Slots[stable.Slots.Count - 1];
        rideables.Add(new Rideable(sc + new Vector2(0, stable.Bounds.Height / 2 + 20), slot.type, slot.color));
        stable.Slots.RemoveAt(stable.Slots.Count - 1);
        ShowNotification($"{slot.type} taken out!");
    }
}

                    foreach (Building building in buildings)
                    {
                        if (Raylib.CheckCollisionRecs(player.Bounds, building.Bounds))
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                            {
                                currentBuilding = building;
                                ChangeScene(SceneState.Building, () =>
                                {
                                    player.Position = currentBuilding.EntryPosition;
                                });
                                if (building.BuildingName == "MY HOUSE")
                                SwitchMusic(musicHouse);
                                if (building.BuildingName.StartsWith("PLAYER HOUSE"))
                                {
                                    int idx = int.Parse(building.BuildingName.Replace("PLAYER HOUSE ", ""));
                                    activeHousePlotIndex = idx;
                                }   
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


if (Vector2.Distance(player.Center, bossArenaEntrance) < 80 && Raylib.IsKeyPressed(KeyboardKey.E)) EnterBossArena();


                    // Dungeon entrances
foreach (var entrance in dungeonEntrances)
{
    if (Vector2.Distance(player.Center, entrance.pos) < 80)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            activeDungeon.Open(entrance.type, entrance.name, player.Position);
            dungeonQuitConfirm = false;
            player.InventoryOpen = false;
            ChangeScene(SceneState.Dungeon);
        }
    }
}


                    camera.Target = player.Position;
}

                    break;
        case SceneState.Dungeon:
        UpdateDungeon(dt);
        if (!multiplayer.Connected || multiplayer.IsHost){
        timeOfDay += daySpeed * dt;
        if (timeOfDay > 1f) { timeOfDay = 0f; dayOfWeek = (dayOfWeek + 1) % 7; }
        }
        totalPlayTime += dt;
        break;

        case SceneState.DrivingTest:
        UpdateDrivingTest(dt);
        break;

        case SceneState.Dive:
        UpdateDive(dt);
        break;

        case SceneState.Underwater:
        UpdateUnderwater(dt);
        break;

        case SceneState.Space:
        UpdateSpace(dt);
        break;

        case SceneState.BossArena: 
        UpdateBossArena(dt); 
        break;

        case SceneState.ClassTest:
        UpdateClassTest(dt);
        break;

        case SceneState.CardGame:
        string mpCardSceneTag = "CardGame";
        multiplayer.Update(player, playerName, mpCardSceneTag); 
        UpdateCardGame(dt); 
        break;

        case SceneState.Sleeping:
        {
            sleepTimer += dt;
            zzzTimer += dt;
            if (sleepFadingIn)
            {
                sleepFadeAlpha = Math.Min(255f, sleepFadeAlpha + dt * 180f);
                if (sleepFadeAlpha >= 255f) sleepFadingIn = false;
            }
            else if (sleepTimer >= sleepDuration - 1f)
            {
                sleepFadeAlpha = Math.Max(0f, sleepFadeAlpha - dt * 160f);
            }
            if (sleepTimer >= sleepDuration && sleepFadeAlpha <= 0f)
            {
                // apply sleep effects
                player.DrunkLevel = 0;
                player.DrunkTimer = 0f;
                player.Health = player.MaxHealth;
                timeOfDay = (timeOfDay + 0.25f) % 1f;
                idDeliveryTimer -= 0.25f;                          
                for (int li = 0; li < 5; li++)                    
                    if (licencePending[li]) licenceDeliveryTimer[li] -= 0.25f;
                foreach (var tree in fruitTrees)
                    if (tree.Planted && tree.RegrowTimer > 0f) tree.RegrowTimer -= 0.25f;
                dayOfWeek = (dayOfWeek + 1) % 7;
                dayOfMonth++;
                if (dayOfMonth > 14) { dayOfMonth = 1; currentMonth = (currentMonth + 1) % 12; }
                shopMessage = "You slept for 6 hours. Fully rested!";
                shopMessageTimer = 3f;
                currentScene = SceneState.Building;
            }
        }
        break;

         case SceneState.Building:
         timeOfDay += daySpeed * dt;
    if (timeOfDay > 1f) { timeOfDay = 0f; dayOfWeek = (dayOfWeek + 1) % 7; }
    totalPlayTime += dt;
    if (levelUpTimer > 0) levelUpTimer -= dt; 

    UpdateCollectables($"Building:{currentBuilding.BuildingName}", player.Center);

    List<Rectangle> activeInteriorObjects =
        (currentBuilding.BuildingName == "SCHOOL" && currentClassroom != "")
            ? classroomInteriorObjects.GetValueOrDefault(currentClassroom, new List<Rectangle>())
        : (currentBuilding.BuildingName == "MALL" && currentMiniShop != "")
            ? miniShopInteriorObjects.GetValueOrDefault(currentMiniShop, new List<Rectangle>())
        : (currentBuilding.BuildingName == "PRISON" && currentPrisonRoom != "")
            ? prisonRoomInteriorObjects.GetValueOrDefault(currentPrisonRoom, new List<Rectangle>())
        : currentBuilding.InteriorObjects;

    int roomW = (currentBuilding.BuildingName == "SCHOOL" || currentBuilding.BuildingName == "PRISON"
                 || currentBuilding.BuildingName == "CASTLE" || currentBuilding.BuildingName == "MALL"
                 || currentBuilding.BuildingName == "ZOO") ? 2000 : 1400;
    int roomH = (currentBuilding.BuildingName == "SCHOOL" || currentBuilding.BuildingName == "PRISON"
                 || currentBuilding.BuildingName == "CASTLE" || currentBuilding.BuildingName == "MALL"
                 || currentBuilding.BuildingName == "ZOO") ? 2000 
                 : (currentBuilding.BuildingName == "FamilyHub") ? 1900 : 1000;
    player.UpdateInterior(dt, activeInteriorObjects, roomW, roomH);
  
    string mpBuildingSceneTag = currentBuilding != null
        ? $"Building:{currentBuilding.BuildingName}"
        : "Building";
    multiplayer.Update(player, playerName, mpBuildingSceneTag);
    CheckSendAppearanceIfChanged();
    if (shopMessageTimer > 0) shopMessageTimer -= dt;

    if (Raylib.IsKeyPressed(KeyboardKey.J) && !chatInputOpen)
    playerMenuOpen = !playerMenuOpen;

    if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom == "")
{
    foreach (var (subject, doorPos) in classroomDoors)
    {
        if (Vector2.Distance(player.Center, doorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            StartTestTransition(() => {
                currentClassroom = subject;
                player.Position = new Vector2(700, 850);
            }, $"Starting {subject} class");
        }
    }
}

// ── COMPANION: adopt at reception, bond when together ──
        if (currentBuilding.BuildingName == "FamilyHub" || currentBuilding.BuildingName == "BEST START")
        {
            Vector2 deskPos = new Vector2(700, 200);
            if (Vector2.Distance(player.Center, deskPos) < 130
                && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            {
                if (activePet != null)
                    ShowNotification("Store your current pet before adopting.");
                else
                {
                    bool kitten = Raylib.GetRandomValue(0, 1) == 0;
                    activePet = kitten
                        ? Pet.NewBaby(player.Center, "Kitten", "Cat", PetColorFor("Cat Egg"), 120f)
                        : Pet.NewBaby(player.Center, "Puppy",  "Dog", PetColorFor("Dog Egg"), 120f);
                    activePet.Adopted = true;
                    ShowNotification($"You adopted a {activePet.Type}!");
                }
            }
        }

        // bond interactions when your adopted companion is beside you
        if (activePet != null && activePet.Adopted
            && Vector2.Distance(player.Center, activePet.Position) < 90f && !chatInputOpen)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.One))
            {
                activePet.Bond = Math.Min(100, activePet.Bond + 2);
                activePet.SocialXP += 2;
                ShowNotification($"You played together. Bond {activePet.Bond}.");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Two))
            {
                activePet.Bond = Math.Min(100, activePet.Bond + 1);
                activePet.StrengthXP += 2;
                ShowNotification($"Training done. Bond {activePet.Bond}.");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Three))
            {
                activePet.Bond = Math.Min(100, activePet.Bond + 1);
                activePet.MagicXP += 2;
                ShowNotification($"Study session. Bond {activePet.Bond}.");
            }
        }

if (currentBuilding.BuildingName == "MALL" && currentMiniShop == "")
{
    foreach (var (name, doorPos, _) in mallShops)
    {
        if (Vector2.Distance(player.Center, doorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            StartTestTransition(() => {
                currentMiniShop = name;
                player.Position = new Vector2(700, 850);
            }, $"Entering {name}");
        }
    }
}

if (currentBuilding.BuildingName == "PRISON" && currentPrisonRoom == "" && !inPrison)
{
    foreach (var (name, doorPos, _) in prisonRooms)
    {
        if (Vector2.Distance(player.Center, doorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            StartTestTransition(() => {
                currentPrisonRoom = name;
                player.Position = new Vector2(700, 850);
            }, $"Entering {name}");
        }
    }
}

if (activeSideTask?.ReadyToDeliver == true
    && activeSideTask.DeliverTo == "Building:" + currentBuilding.BuildingName
    && Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 160
    && Raylib.IsKeyPressed(KeyboardKey.H) && !chatInputOpen)
    CompleteSideTask(currentBuilding.InteriorNPC.Name);

Vector2 gymDoorPos = new Vector2(970, 1650);
if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom == ""
    && Vector2.Distance(player.Center, gymDoorPos) < 70 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
{
    var court = buildings.FirstOrDefault(b => b.BuildingName == "BASKETBALL COURT");
    if (court != null)
    {
        schoolReturnBuilding = currentBuilding;   
        enteredCourtFromSchool = true;            
        StartTestTransition(() => {
    currentBuilding = court;
    player.Position = court.EntryPosition + new Vector2(0, -150);   // spawn offset from the exit-trigger zone
}, "Entering gym");
    }
}

if (currentBuilding.BuildingName == "BASKETBALL COURT" && enteredCourtFromSchool
    && Raylib.IsKeyPressed(KeyboardKey.Q)
    && Vector2.Distance(player.Center, currentBuilding.EntryPosition) < 120f)
{
    var returnTo = schoolReturnBuilding;
    enteredCourtFromSchool = false;
    schoolReturnBuilding = null;
    StartTestTransition(() => {
        currentBuilding = returnTo;
        player.Position = gymDoorPos + new Vector2(0, 60);
    }, "Exiting gym");
    return;
}

if (currentBuilding.BuildingName == "SCHOOL" && currentClassroom != "")
{
    Vector2 classDoorOut = new Vector2(700, 950);
    if (Vector2.Distance(player.Center, classDoorOut) < 70 && Raylib.IsKeyPressed(KeyboardKey.Q) && !chatInputOpen)
{
    string leavingSubject = currentClassroom;
    StartTestTransition(() => {
        currentClassroom = "";
        var exitDoor = classroomDoors.First(c => c.subject == leavingSubject).doorPos;
        player.Position = new Vector2(exitDoor.X, exitDoor.Y + 150);
    }, $"Exiting {leavingSubject} class");
}
}

if (currentBuilding.BuildingName == "MALL" && currentMiniShop != "")
{
    Vector2 shopDoorOut = new Vector2(700, 950);
    if (Vector2.Distance(player.Center, shopDoorOut) < 70 && Raylib.IsKeyPressed(KeyboardKey.Q) && !chatInputOpen)
    {
        string leaving = currentMiniShop;
        StartTestTransition(() => {
            currentMiniShop = "";
            var door = mallShops.First(s => s.name == leaving).doorPos;
            player.Position = new Vector2(door.X, door.Y + 120);
        }, $"Leaving {leaving}");
    }
}

if (currentBuilding.BuildingName == "PRISON" && currentPrisonRoom != "")
{
    Vector2 roomDoorOut = new Vector2(700, 950);
    if (Vector2.Distance(player.Center, roomDoorOut) < 70 && Raylib.IsKeyPressed(KeyboardKey.Q) && !chatInputOpen)
    {
        string leaving = currentPrisonRoom;
        StartTestTransition(() => {
            currentPrisonRoom = "";
            var door = prisonRooms.First(r => r.name == leaving).doorPos;
            player.Position = new Vector2(door.X, door.Y + 60);
        }, $"Leaving {leaving}");
    }
}

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

    bool nearWardrobe = Vector2.Distance(player.Center, wardrobePos) < 100;
    bool nearChest    = Vector2.Distance(player.Center, chestPos)    < 100;
    bool nearBed      = Vector2.Distance(player.Center, bedPos)      < 200;

    Vector2 seqTablePos = new Vector2(260, 480);
    bool nearSeqTable = Vector2.Distance(player.Center, seqTablePos) < 100;
    if (nearSeqTable && !wardrobeOpen && !chestOpen && !cookingMenuOpen)
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) StartSequenceGame();

    if (!chestOpen && nearWardrobe && !bedMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
            wardrobeOpen = !wardrobeOpen;
    }

    if (!wardrobeOpen && nearChest && !bedMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
        {
            if (chestOpen) { chestOpen = false; openChestId = null; }
            else { var c = EnsureHouseChest("MY HOUSE", chestPos); chestOpen = true; openChestId = c.Id; }
        }
    }

    if (!wardrobeOpen && !chestOpen && nearBed)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
        {
            bedMenuOpen = true;
            bedMenuSelected = 0;
            return; // consume this Space press, don't fall into confirm below
        }

        if (bedMenuOpen)
        {
            int mx = ScreenWidth / 2 - 200;
            int my = ScreenHeight / 2 - 100;
            Rectangle saveRect  = new Rectangle(mx + 20, my + 60,  360, 44);
            Rectangle sleepRect = new Rectangle(mx + 20, my + 116, 360, 44);

            // mouse hover selects
            // keyboard and scroll navigation takes priority
            float bedScroll = Raylib.GetMouseWheelMove();
            if (Raylib.IsKeyPressed(KeyboardKey.Up)   || bedScroll > 0) bedMenuSelected = 0;
            if (Raylib.IsKeyPressed(KeyboardKey.Down) || bedScroll < 0) bedMenuSelected = 1;

            // mouse hover only applies when mouse is actually moving
            if (Raylib.CheckCollisionPointRec(mouse, saveRect)  && !Raylib.CheckCollisionPointRec(mouse, sleepRect)) bedMenuSelected = 0;
            if (Raylib.CheckCollisionPointRec(mouse, sleepRect) && !Raylib.CheckCollisionPointRec(mouse, saveRect))  bedMenuSelected = 1;

            bool confirm = (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
                        || (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                            (Raylib.CheckCollisionPointRec(mouse, saveRect) ||
                             Raylib.CheckCollisionPointRec(mouse, sleepRect)));

            if (confirm)
            {
                if (bedMenuSelected == 0)
                {
                    SaveGame();
                    shopMessage = "Game saved!";
                    shopMessageTimer = 2f;
                    bedMenuOpen = false;
                }
                else
                {
                    bedMenuOpen = false;
                    sleepTimer = 0f;
                    sleepFadeAlpha = 0f;
                    sleepFadingIn = true;
                    zzzTimer = 0f;
                    currentScene = SceneState.Sleeping;
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape)) bedMenuOpen = false;
        }
    }
    if (!nearBed) bedMenuOpen = false;

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && wardrobeOpen)
    {
        wardrobeOpen = false;
        return;
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && chestOpen)
    {
        chestOpen = false;
        openChestId = null;
        return;
    }
    
    // Kitchen stove/bench interaction
Vector2 stovePos = new Vector2(200, 150);
bool nearStove = Vector2.Distance(player.Center, stovePos) < 160;

if (nearStove && !wardrobeOpen && !chestOpen && !fridgeOpen && !cupboardOpen)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("KITCHEN STOVE", 20, 630, 28, new Color((byte)255,(byte)160,(byte)30,(byte)255));
    Program.DrawTextUI("E = Cook (all recipes available)", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen) { cookingContext = "kitchen"; cookingMenuOpen = !cookingMenuOpen; }
}

// Fridge interaction
Vector2 fridgeInteractPos = new Vector2(290, 170);
bool nearFridge = Vector2.Distance(player.Center, fridgeInteractPos) < 120;
if (nearFridge && !wardrobeOpen && !chestOpen && !cookingMenuOpen)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("FRIDGE", 20, 630, 28, new Color((byte)100,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI($"Space = Open  |  Contents: {fridgeContents.Count(s => s != null)}/12", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.Space)) fridgeOpen = !fridgeOpen;
}

// Cupboard interaction — bench top area left of fridge
Vector2 cupboardPos = new Vector2(120, 120);
bool nearCupboard = Vector2.Distance(player.Center, cupboardPos) < 120;
if (nearCupboard && !wardrobeOpen && !chestOpen && !cookingMenuOpen)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("CUPBOARD", 20, 630, 28, new Color((byte)200,(byte)160,(byte)80,(byte)255));
    Program.DrawTextUI($"Space = Open  |  Contents: {cupboardContents.Count(s => s != null)}/12", 20, 668, 22, Color.White);
    if (Raylib.IsKeyPressed(KeyboardKey.Space)) cupboardOpen = !cupboardOpen;
}
}

if (currentBuilding.BuildingName.StartsWith("PLAYER HOUSE"))
{
    Vector2 bedPos   = new Vector2(1120, 580);
    Vector2 chestPos = new Vector2(872, 915);
    if (bedMenuInputCooldown > 0f) bedMenuInputCooldown -= dt;

    bool nearBed2   = Vector2.Distance(player.Center, bedPos)   < 180;
    bool nearChest2 = Vector2.Distance(player.Center, chestPos) < 100;

    if (!wardrobeOpen && nearChest2 && !bedMenuOpen)
    if (Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        if (chestOpen) { chestOpen = false; openChestId = null; }
        else
        {
            string key = "PLAYER HOUSE " + activeHousePlotIndex;
            var c = EnsureHouseChest(key, chestPos);
            chestOpen = true; openChestId = c.Id;
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && chestOpen)
    {
        chestOpen = false;
        openChestId = null;
        return;
    }

    // bed — opens with Space, closes with Escape or menu selection
    if (!wardrobeOpen && !chestOpen && nearBed2)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !bedMenuOpen && bedMenuInputCooldown <= 0f)
        {
            bedMenuOpen = true;
            bedMenuInputCooldown = 0.4f;
        }

        if (bedMenuOpen)
        {
            Vector2 mouse = Raylib.GetMousePosition();
            int mx = ScreenWidth / 2 - 200;
            int my = ScreenHeight / 2 - 100;
            Rectangle saveRect  = new Rectangle(mx + 20, my + 60,  360, 44);
            Rectangle sleepRect = new Rectangle(mx + 20, my + 116, 360, 44);

            float bedScroll = Raylib.GetMouseWheelMove();
            if (Raylib.IsKeyPressed(KeyboardKey.Up)   || bedScroll > 0) bedMenuSelected = 0;
            if (Raylib.IsKeyPressed(KeyboardKey.Down) || bedScroll < 0) bedMenuSelected = 1;
            if (Raylib.CheckCollisionPointRec(mouse, saveRect)  && !Raylib.CheckCollisionPointRec(mouse, sleepRect)) bedMenuSelected = 0;
            if (Raylib.CheckCollisionPointRec(mouse, sleepRect) && !Raylib.CheckCollisionPointRec(mouse, saveRect))  bedMenuSelected = 1;

            bool confirm = bedMenuInputCooldown <= 0f &&
                          (Raylib.IsKeyPressed(KeyboardKey.Space)
                        || (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                            (Raylib.CheckCollisionPointRec(mouse, saveRect) ||
                             Raylib.CheckCollisionPointRec(mouse, sleepRect))));

            if (confirm)
            {
                if (bedMenuSelected == 0) { SaveGame(); shopMessage = "Game saved!"; shopMessageTimer = 2f; bedMenuOpen = false; }
                else { bedMenuOpen = false; sleepTimer = 0f; sleepFadeAlpha = 0f; sleepFadingIn = true; zzzTimer = 0f; currentScene = SceneState.Sleeping; }
                bedMenuInputCooldown = 0.4f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Escape)) bedMenuOpen = false;
        }
    }

    if (!nearBed2) bedMenuOpen = false;

    // ── FURNITURE PLACEMENT ───────────────────────────────────────────────────
    if (!chestOpen && !bedMenuOpen && !houseMenuOpen && ActiveHouseData != null)
    {
        var furniture = ActiveHouseData.Furniture;

        if (furniturePlaceMode)
        {
            Vector2 mouse = Raylib.GetMousePosition();
            Vector2 worldMouse = Raylib.GetScreenToWorld2D(mouse, camera);

            int snapX = (heldFurnitureType == "Wall"  || heldFurnitureType == "HalfWall")  ? 120
                      : (heldFurnitureType == "WallV" || heldFurnitureType == "HalfWallV") ? 16
                      : furnitureGridSnap;
            int snapY = (heldFurnitureType == "Wall"  || heldFurnitureType == "HalfWall")  ? 16
                      : (heldFurnitureType == "WallV" || heldFurnitureType == "HalfWallV") ? 90
                      : furnitureGridSnap;

            furnitureCursorX = ((int)worldMouse.X / snapX) * snapX;
            furnitureCursorY = ((int)worldMouse.Y / snapY) * snapY;
            furnitureCursorX = Math.Clamp(furnitureCursorX, 20,  1180);
            furnitureCursorY = Math.Clamp(furnitureCursorY, 70,  900);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (heldFurnitureIndex >= 0 && heldFurnitureIndex < furniture.Count)
                {
                    furniture[heldFurnitureIndex].RoomX = furnitureCursorX;
                    furniture[heldFurnitureIndex].RoomY = furnitureCursorY;
                    furniturePlaceMode = false;
                    heldFurnitureType  = "";
                    heldFurnitureIndex = -1;
                    SpawnPlayerHouse();
                    ShowNotification("Furniture moved!");
                }
                else
                {
                    var template = GetFurnitureTemplate(heldFurnitureType);
                    if (player.Money >= template.cost)
                    {
                        player.Money -= template.cost;
                        furniture.Add(new HouseFurniture(
                            heldFurnitureType, furnitureCursorX, furnitureCursorY,
                            template.cost, template.col));
                        SpawnPlayerHouse();
                        ShowNotification($"Placed! ${template.cost} charged. Click again to place another or ESC to stop.");
                        furnitureCursorX = (int)player.Position.X + 60;
                        furnitureCursorY = (int)player.Position.Y + 80;
                    }
                    else
                    {
                        ShowNotification("Not enough money!");
                        furniturePlaceMode = false;
                        heldFurnitureType  = "";
                        heldFurnitureIndex = -1;
                    }
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                furniturePlaceMode = false;
                heldFurnitureType  = "";
                heldFurnitureIndex = -1;
            }

            // delete — Delete + right click
            if (Raylib.IsKeyDown(KeyboardKey.Delete) && Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                for (int i = furniture.Count - 1; i >= 0; i--)
                {
                    if (Raylib.CheckCollisionPointRec(worldMouse, new Rectangle(furniture[i].RoomX, furniture[i].RoomY, 80, 60)))
                    {
                        player.Money += furniture[i].Cost / 2;
                        ShowNotification($"Sold {furniture[i].Type} for ${furniture[i].Cost / 2}");
                        furniture.RemoveAt(i);
                        SpawnPlayerHouse();
                        break;
                    }
                }
            }
        }
        else
        {
            // pick up furniture — right click
            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                Vector2 worldMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                for (int i = 0; i < furniture.Count; i++)
                {
                    if (Raylib.CheckCollisionPointRec(worldMouse, new Rectangle(furniture[i].RoomX, furniture[i].RoomY, 80, 60)))
                    {
                        heldFurnitureIndex = i;
                        heldFurnitureType  = furniture[i].Type;
                        furniturePlaceMode = true;
                        furnitureCursorX   = furniture[i].RoomX;
                        furnitureCursorY   = furniture[i].RoomY;
                        ShowNotification($"Moving {furniture[i].Type} — click to place, ESC to cancel");
                        break;
                    }
                }
            }
        }
    }

    if (!chestOpen && !bedMenuOpen)
        if (Raylib.IsKeyPressed(KeyboardKey.H))
        {
            houseMenuOpen = !houseMenuOpen;
            if (!houseMenuOpen)
            {
                furniturePlaceMode = false;
                heldFurnitureType  = "";
                heldFurnitureIndex = -1;
            }
        }
}

if (currentBuilding.BuildingName == "DropZone")
{
    // Bowling lanes
    Vector2[] bowlingLanes = { MPos("bowl1"), MPos("bowl2") };
    for (int i = 0; i < bowlingLanes.Length; i++)
    {
        if (Vector2.Distance(player.Center, bowlingLanes[i]) < 80 && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (TryPayDropzonePlay()) { 
                activeBowlingGame.Open(i);
                activeMinigameType = MinigameType.Bowling;
                ChangeScene(SceneState.Minigame);
            }
            else { shopMessage = "Need $3 to bowl a game!"; shopMessageTimer = 1.5f; }
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.P) && !chatInputOpen) plushLogOpen = !plushLogOpen;

    // ... existing code (inside DropZone update block) ...
    Vector2 counterPos = new Vector2(600, 600);
    if (Vector2.Distance(player.Center, counterPos) < 100)
    {
        if (!hasDropzoneCard && Raylib.IsKeyPressed(KeyboardKey.B) && !chatInputOpen && player.Money >= 5)
        { player.Money -= 5; hasDropzoneCard = true; ShowNotification("DropZone Red card purchased!"); }
        else if (hasDropzoneCard && Raylib.IsKeyPressed(KeyboardKey.L) && !chatInputOpen && player.Money >= 20)
        { player.Money -= 20; dropzoneCredit += 20f; ShowNotification("Loaded $20 onto card."); }
    }

    // Pool tables
Vector2[] poolTables = { MPos("pool1")  };
for (int i = 0; i < poolTables.Length; i++)
{
    if (Vector2.Distance(player.Center, poolTables[i]) < 100 && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        if (TryPayDropzonePlay()) {
            activePoolGame.Open(i);
            activeMinigameType = MinigameType.Pool;
            ChangeScene(SceneState.Minigame);
        }
        else { shopMessage = "Need $2 to play pool!"; shopMessageTimer = 1.5f; }
    }
}

    // Claw machines
    Vector2[] clawMachines = { MPosSouth("claw1"), MPosSouth("claw2") };
    for (int i = 0; i < clawMachines.Length; i++)
    {
        if (Vector2.Distance(player.Center, clawMachines[i]) < 70 && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            activeClawMachine.Open(i);
            activeMinigameType = MinigameType.Claw;
            ChangeScene(SceneState.Minigame);
        }
    }

    // Pinball
 if (Vector2.Distance(player.Center, MPosSouth("pinball")) < 75 && Raylib.IsKeyPressed(KeyboardKey.Space)) 
{
    if (TryPayDropzonePlay()) { activePinballGame.Open(0); activeMinigameType = MinigameType.Pinball; ChangeScene(SceneState.Minigame); }
    else { shopMessage = "Need $2 to play pinball!"; shopMessageTimer = 1.5f; }
}
// Air Hockey
if (Vector2.Distance(player.Center, MPosSouth("airhock")) < 75 && Raylib.IsKeyPressed(KeyboardKey.Space))
{
    if (TryPayDropzonePlay()) { activeAirHockeyGame.Open(0); activeMinigameType = MinigameType.AirHockey; ChangeScene(SceneState.Minigame); }
    else { shopMessage = "Need $3 to play air hockey!"; shopMessageTimer = 1.5f; }
}
// Piano Tiles
 if (Vector2.Distance(player.Center, MPosSouth("piano")) < 75 && Raylib.IsKeyPressed(KeyboardKey.Space))
{
    if (TryPayDropzonePlay()) { activePianoTilesGame.Open(0); activeMinigameType = MinigameType.PianoTiles; ChangeScene(SceneState.Minigame); }
    else { shopMessage = "Need $2 to play!"; shopMessageTimer = 1.5f; }
}

// Flappy Bird (tickets)
 if (Vector2.Distance(player.Center, MPosSouth("flappy")) < 75 && Raylib.IsKeyPressed(KeyboardKey.Space))
{
    if (TryPayDropzonePlay()) { activeFlappyGame.Open(0);  activeMinigameType = MinigameType.Flappy;  ChangeScene(SceneState.Minigame); }
    else { shopMessage = "Need $1 to play!"; shopMessageTimer = 1.5f; }
}

// Prize counter
if (Vector2.Distance(player.Center, MPosSouth("prize")) < 80 && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
    prizeCounterOpen = !prizeCounterOpen;

if (prizeCounterOpen)
{
    if (Raylib.IsKeyPressed(KeyboardKey.One))   RedeemPrize("Small Plush", 10);
    if (Raylib.IsKeyPressed(KeyboardKey.Two))   RedeemPrize("Big Plush", 25);
    if (Raylib.IsKeyPressed(KeyboardKey.Three)) RedeemPrize("Giant Plush", 50);
    if (Raylib.IsKeyPressed(KeyboardKey.Four))  RedeemPrize("Jackpot Trophy", 100);
}

    // Arcade pokie-style cabinets (reuse your pokie machine)
    Vector2[] arcadeCabinets = { MPosSouth("arcade1"), MPosSouth("arcade2") };
    for (int i = 0; i < arcadeCabinets.Length; i++)
    {
        if (Vector2.Distance(player.Center, arcadeCabinets[i]) < 70 && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            activePokieMachine.Open(i);
            activeMinigameType = MinigameType.Pokie;
            ChangeScene(SceneState.Minigame);
        }
    }

    // Food & drink counter
    if (Vector2.Distance(player.Center, MPosSouth("food")) < 80 && Raylib.IsKeyPressed(KeyboardKey.E))
        dropZoneFoodMenuOpen = !dropZoneFoodMenuOpen;

    if (dropZoneFoodMenuOpen)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One))  BuyDropZoneItem("Hot Dog", 6, 18);
        if (Raylib.IsKeyPressed(KeyboardKey.Two))  BuyDropZoneItem("Nachos", 7, 20);
        if (Raylib.IsKeyPressed(KeyboardKey.Three))BuyDropZoneItem("Soda", 4, 12);
        if (Raylib.IsKeyPressed(KeyboardKey.Four)) BuyDropZoneItem("Slushie", 5, 14);
    }

    if (Vector2.Distance(player.Center, cRidePos) < 100)
{
    cRideMessageActive = true;
}
else
{
    cRideMessageActive = false;
}
}
if (currentBuilding.BuildingName == "MiniGolf")
{
    if (Vector2.Distance(player.Center, new Vector2(640, 345)) < 90 && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        if (player.Money >= 5)
        {
            player.Money -= 5;
            activeMiniGolfGame.Open(0);
            activeMinigameType = MinigameType.MiniGolf;
            ChangeScene(SceneState.Minigame);
        }
        else { shopMessage = "Need $5 to play mini golf!"; shopMessageTimer = 1.5f; }
    }
}

              if (currentBuilding.BuildingName == "STORE")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
    bool nearDumbbells  = Vector2.Distance(player.Center, dumbbellPos) < 120;
    bool nearBench      = Vector2.Distance(player.Center, benchPos) < 120;

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
        if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
        if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
        if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
        if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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

// SWIMMING COMPLEX
if (currentBuilding.BuildingName == "SWIMMING COMPLEX")
{
    // ── START INTERACTIONS ──
    if (!swimmingActive)
    {
        // lane pool — stand near it and press Space
        if (player.Position.X < 600 && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            swimmingActive = true;
            swimmingPoolType = "lane";
            swimLapTimer = 0f;
            swimLapsCompleted = 0;
            swimSpeed = 0f;
            swimLeftNext = true;
            swimPerfectStrokes = 0;
            player.Hidden = true;
        }
        // diving pool — stand near it and press E
        else if (player.Position.X >= 600 && (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen))
        {
            swimmingActive = true;
            swimmingPoolType = "diving";
            divingStage = 0;
            divingPower = 0f;
            divingPowerUp = true;
            divingRotation = 0f;
            divingRotDir = 1f;
            divingEntry = 0f;
            divingEntryUp = true;
            divingJumped = false;
            divingFallTimer = 0f;
            divingScore = 0;
            divingResult = "";
            player.Hidden = true;
        }
    }

    // ── LANE SWIMMING — rhythm strokes ──
    if (swimmingActive && swimmingPoolType == "lane")
    {
        // alternate LEFT and RIGHT arrows to stroke; correct alternation builds speed
        bool stroked = false;
        bool correct = false;

        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            stroked = true;
            correct = swimLeftNext;
            swimLeftNext = false;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            stroked = true;
            correct = !swimLeftNext;
            swimLeftNext = true;
        }

        if (stroked)
        {
            if (correct)
            {
                swimSpeed = Math.Min(1f, swimSpeed + 0.12f);
                swimPerfectStrokes++;
                swimStrokeWindow = 0.25f;   // flash green
            }
            else
            {
                swimSpeed = Math.Max(0f, swimSpeed - 0.15f);  // wrong key = lose momentum
                swimStrokeWindow = -0.25f;  // flash red
            }
        }

        // momentum decays if you stop stroking
        swimSpeed = Math.Max(0f, swimSpeed - 0.18f * Raylib.GetFrameTime());
        if (swimStrokeWindow > 0) swimStrokeWindow -= Raylib.GetFrameTime();
        if (swimStrokeWindow < 0) swimStrokeWindow += Raylib.GetFrameTime();

        // progress along the lane scales with current speed
        swimLapTimer += swimSpeed * 2.2f * Raylib.GetFrameTime();

        if (swimLapTimer >= swimLapDuration)
        {
            swimLapTimer = 0f;
            swimLapsCompleted++;
            // XP rewards perfect strokes that lap
            int xp = 25 + swimPerfectStrokes * 2;
            player.AddSwimmingXP(xp);
            player.AddStaminaXP(xp);
            shopMessage = $"Lap {swimLapsCompleted}! +{xp} Swimming XP";
            shopMessageTimer = 1.8f;
            swimPerfectStrokes = 0;

            if (swimLapsCompleted >= 4)
            {
                swimmingActive = false;
                player.Money += 8;
                shopMessage = "Great session! 4 laps done. +$8";
                shopMessageTimer = 2.5f;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
            swimmingActive = false;
    }

    // ── DIVING — multi-stage timing ──
    if (swimmingActive && swimmingPoolType == "diving")
    {
        if (divingStage == 0)
        {
            // STAGE 1: charge jump power with a bouncing bar, lock with SPACE
            divingPower += (divingPowerUp ? 1f : -1f) * 1.3f * Raylib.GetFrameTime();
            if (divingPower >= 1f) { divingPower = 1f; divingPowerUp = false; }
            if (divingPower <= 0f) { divingPower = 0f; divingPowerUp = true; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                divingStage = 1;
        }
        else if (divingStage == 1)
        {
            // STAGE 2: rotation timing — stop the spinning bar in the green zone
            divingRotation += divingRotDir * 1.6f * Raylib.GetFrameTime();
            if (divingRotation >= 1f) { divingRotation = 1f; divingRotDir = -1f; }
            if (divingRotation <= 0f) { divingRotation = 0f; divingRotDir = 1f; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                divingStage = 2;
        }
        else if (divingStage == 2)
        {
            // STAGE 3: entry timing — nail the cursor at centre for a clean entry
            divingEntry += (divingEntryUp ? 1f : -1f) * 1.9f * Raylib.GetFrameTime();
            if (divingEntry >= 1f) { divingEntry = 1f; divingEntryUp = false; }
            if (divingEntry <= 0f) { divingEntry = 0f; divingEntryUp = true; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                divingJumped = true;
                divingStage = 3;
                divingFallTimer = 0f;
                ScoreDive(player);
            }
        }
        else if (divingStage == 3)
        {
            // falling animation, then show result and reset
            divingFallTimer += Raylib.GetFrameTime();
            if (divingFallTimer > 2.2f)
            {
                swimmingActive = false;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && divingStage < 3)
            swimmingActive = false;
    }
}

// restore the real player once any swim/dive activity has ended
if (currentBuilding.BuildingName == "SWIMMING COMPLEX" && !swimmingActive && player.Hidden)
    player.Hidden = false;

// ── TENNIS COURT ─────────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "TENNIS COURT")
{
    Vector2 netPos = new Vector2(680, 300);
    if (!tennisActive && Vector2.Distance(player.Center, netPos) < 150 && Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        tennisActive = true;
        sportPlayCounts["Tennis"] = sportPlayCounts.GetValueOrDefault("Tennis") + 1;
        tennisDifficultySelect = true;
        player.Hidden = true;
        tennisPlayerScore = 0;
        tennisAIScore = 0;
        tennisMessage = "Choose difficulty: 1 Easy  2 Normal  3 Hard";
        tennisMessageTimer = 999f;
    }

    if (tennisActive)
    {
        if (tennisMessageTimer > 0) tennisMessageTimer -= dt;
        if (tennisSwingCooldown > 0) tennisSwingCooldown -= dt;

        if (tennisDifficultySelect)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.One))   { tennisAIMaxSpeed = 190f; tennisAILead = 0.05f; tennisDifficultySelect = false; }
            if (Raylib.IsKeyPressed(KeyboardKey.Two))   { tennisAIMaxSpeed = 260f; tennisAILead = 0.12f; tennisDifficultySelect = false; }
            if (Raylib.IsKeyPressed(KeyboardKey.Three)) { tennisAIMaxSpeed = 340f; tennisAILead = 0.22f; tennisDifficultySelect = false; }
            if (!tennisDifficultySelect)
            {
                StartTennisPoint(true);   // player serves first
                tennisMessage = "Your serve! Hold SPACE to toss, release to serve.";
                tennisMessageTimer = 3f;
            }
        }
        else
        {
            UpdateTennisPlay(dt);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            tennisActive = false;
            shopMessage = $"Tennis ended. {tennisPlayerScore} - {tennisAIScore}";
            shopMessageTimer = 2f;
        }
    }
}

// restore player when match ends
if (currentBuilding.BuildingName == "TENNIS COURT" && !tennisActive && player.Hidden)
    player.Hidden = false;

// ── BASKETBALL COURT ─────────────────────────────────────────────────────
if (currentBuilding.BuildingName == "BASKETBALL COURT")
{
    Vector2 freeThrowLine = new Vector2(400, 500);
    bool nearFreeThrow = Vector2.Distance(player.Center, freeThrowLine) < 100;

    if (bbMessageTimer > 0) bbMessageTimer -= dt;

    if (!basketballActive)
    {
        if (nearFreeThrow && Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            basketballActive = true;
            sportPlayCounts["Basketball"] = sportPlayCounts.GetValueOrDefault("Basketball") + 1;
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
                    player.AddSportsXP(xp);
                    bbMessage = $"SWISH! Score: {bbScore} | +{xp} Sports XP";
                }
                else if (powerDiff < 0.25f && aimDiff < 0.35f)
                {
                    bbMessage = "Off the rim! Close shot.";
                    player.AddSportsXP(3);
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
        Vector2.Distance(player.Center, bp) < 130);

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
    bool nearTrolleyPickup   = Vector2.Distance(player.Center, trolleyPickupPos) < 100;
    bool nearBasketPickup    = Vector2.Distance(player.Center, basketPickupPos)  < 100;

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
    // near grocery shelves — open shop panel
bool nearShelf =
    (player.Center.X > 50 && player.Center.X < 1380 &&
     player.Center.Y > 140 && player.Center.Y < 780 &&
     (player.HasTrolley || player.HasBasket));

if (nearShelf && (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen))
    groceryShopOpen = !groceryShopOpen;

if (!nearShelf && player.Center.Y < 870) groceryShopOpen = false;

if (nearShelf && !groceryShopOpen)
{
    Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI("GROCERY SHELVES", 20, 630, 28, new Color((byte)80,(byte)200,(byte)80,(byte)255));
    Program.DrawTextUI("E = Browse groceries", 20, 668, 22, Color.White);
}

    // open/close inventory — unchanged
    if ((player.HasTrolley || player.HasBasket) && Raylib.IsKeyPressed(KeyboardKey.I))
        supermarketInventoryOpen = !supermarketInventoryOpen;
}

        if (currentBuilding.BuildingName == "DBar")
{
if (Raylib.IsKeyPressed(KeyboardKey.Space) && !barMenuOpen && Vector2.Distance(player.Center, barCounterPos) < 120)
    barMenuOpen = true;

        if (Raylib.IsKeyPressed(KeyboardKey.Q) && barMenuOpen)
{
    barMenuOpen = false;
    barSelectedDrink = -1;
    return;
}

        if (Vector2.Distance(player.Center, barCounterPos) < 120)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Program.DrawTextUI("THE BAR", 20, 630, 30, Color.Gold);
            Program.DrawTextUI("Space = Order a drink ($5)", 20, 670, 24, Color.White);
        }

    Vector2[] pokiePositions = {
        new Vector2(810, 245),
        new Vector2(810, 365),
        new Vector2(910, 245)
    };

    Vector2 dartboardPosition = new Vector2(1250, 80); // matches the dartboard you drew in DBar
    DartsGame activeDartsGame = new DartsGame();

    for (int i = 0; i < pokiePositions.Length; i++)
    {
        if (i == 1) continue; // machine 2 (index 1) is the occupied one — handled below

        if (Vector2.Distance(player.Center, pokiePositions[i]) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                activePokieMachine.Open(i);
                activeMinigameType = MinigameType.Pokie;
                ChangeScene(SceneState.Minigame);
            }
        }
    }

    // occupied pokie machine (index 1)
    if (Vector2.Distance(player.Center, pokiePositions[1]) < 80)
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
    if (Vector2.Distance(player.Center, poolTablePositions[i]) < 100)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (player.Money >= 2)
            {
                player.Money -= 2;
                activePoolGame.Open(i);
                activeMinigameType = MinigameType.Pool;
                ChangeScene(SceneState.Minigame);
            }
            else
            {
                shopMessage = "Need $2 to play a round!";
                shopMessageTimer = 1.5f;
            }
        }
    }
}

if (Vector2.Distance(player.Center, dartboardPosition) < 100)
{
    if (Raylib.IsKeyPressed(KeyboardKey.Space))
    {
        if (player.Money >= 2)
        {
            player.Money -= 2;
            activeDartsGame.Open(false); // false = practice; see note below for Vs AI
            activeMinigameType = MinigameType.Darts;
            ChangeScene(SceneState.Minigame);
        }
        else
        {
            shopMessage = "Need $2 to play a round!";
            shopMessageTimer = 1.5f;
        }
    }
}
}

if (currentBuilding.BuildingName == "KiwiCuts")
{
    Vector2 chair1Pos = new Vector2(130, 245);
    Vector2 chair2Pos = new Vector2(250, 245);

    foreach (Vector2 chairPos in new[] { chair1Pos, chair2Pos })
    {
        if (Vector2.Distance(player.Center, chairPos) < 80)
        {
            Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Program.DrawTextUI("BARBER CHAIR", 20, 630, 30, new Color((byte)40,(byte)220,(byte)180,(byte)255));
            
            string chairPrompt = Program.HasHeadItemEquipped
            ? "Remove your head gear"
            : "Space = Get a haircut | Q = Close";
            Color chairPromptColor = Program.HasHeadItemEquipped ? Color.Orange : Color.White;
            Program.DrawTextUI(chairPrompt, 20, 670, 24, chairPromptColor);

            if (Raylib.IsKeyPressed(KeyboardKey.Space) && !hairMenuOpen)
{
    if (Program.HasHeadItemEquipped)
    {
        shopMessage = "Take off your head gear first bro, can't cut through that!";
        shopMessageTimer = 3f;
    }
    else
        hairMenuOpen = true;
}
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Q) && hairMenuOpen)
        hairMenuOpen = false;
}

    // All other buildings - NPC distance check
    if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
    {
        if (currentBuilding.BuildingName == "HOSPITAL")
{
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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

if (Vector2.Distance(player.Center, currentBuilding.InteriorNPC.Position) < 120)
{
    // existing other cases...
    if (currentBuilding.BuildingName == "BIKE DEALER" ||
        currentBuilding.BuildingName == "CAR DEALER" ||
        currentBuilding.BuildingName == "BARN DEALER")
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
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
    float distToEntrance = Vector2.Distance(player.Center, currentBuilding.EntryPosition);
    bool nearEntrance = distToEntrance < 120f;

    if (nearEntrance)
    {
        if (currentBuilding.BuildingName == "BASKETBALL COURT" && enteredCourtFromSchool)   
        {
            enteredCourtFromSchool = false;
            currentBuilding = schoolReturnBuilding;
            schoolReturnBuilding = null;
            player.Position = gymDoorPos + new Vector2(0, 60);
            return;   // skip the rest of the world-exit logic below
        }

        Building exitingBuilding = currentBuilding;
        ChangeScene(SceneState.World, () =>
        {
            player.Position = exitingBuilding.ExitPosition;
            lastZoneMusic = default; // force zone music to re-evaluate
            CheckZoneMusic();
        });

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

    if (currentBuilding.BuildingName == "SWIMMING COMPLEX" && swimmingActive)
    {
        if (swimmingPoolType == "lane")
        {
            int swimX = (int)(80 + (swimLapTimer / swimLapDuration) * 440);
            camera.Target = new Vector2(swimX, 200);
        }
        else // diving
        {
            camera.Target = divingJumped
                ? new Vector2(800, 180 + Math.Min(divingFallTimer * 200f, 490f))
                : new Vector2(770, 170);
        }
    }
        else if (currentBuilding.BuildingName == "TENNIS COURT" && tennisActive)
{
    camera.Target = new Vector2(640, (CourtTop + CourtBottom) / 2f);
}
    else
    {
        camera.Target = player.Position;
    }


    break;
     case SceneState.Minigame:
                    if (lastScene != SceneState.Minigame)
                        minigamePlayCounts[activeMinigameType.ToString()] =
                            minigamePlayCounts.GetValueOrDefault(activeMinigameType.ToString()) + 1;
                    UpdateMinigameScreen(dt);
                    break;
            }
           lastScene = currentScene;
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

                 case SceneState.Sleeping:
                DrawInterior();
                DrawSleepScreen();
                break;

                case SceneState.Minigame:
                DrawMinigameScreen();
                break;

                case SceneState.Dungeon:
                DrawDungeon();
                break;

                case SceneState.DrivingTest:
                DrawDrivingTest();
                break;

                case SceneState.Dive:
                DrawDive();
                break;

                case SceneState.Underwater:
                DrawUnderwater();
                break;

                case SceneState.Space:
                DrawSpace();
                break;

                case SceneState.BossArena: 
                DrawBossArena(); 
                break;

                case SceneState.ClassTest:
                DrawClassTest();
                break;

                case SceneState.CardGame: 
                DrawCardGame(); 
                break;
            }
            
            DrawCookingMenu();
            DrawTestTransitionOverlay();
            DrawNotificationBanner();
            UpdateSkillsUI();
            DrawQuickCookBar();
            Vector2 mouse = Raylib.GetMousePosition();
            Rectangle questsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 60, 140, 40);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
    if (Raylib.CheckCollisionPointRec(mouse, questsBtn))
        if (!pauseMenuOpen && !player.InventoryOpen && !armorMenuOpen)
        {
            questsOpen = !questsOpen;
            if (questsOpen) skillsOpen = false;
        }
}

if (licenceCongratsOpen)
    DrawLicenceCongrats();
if (licenceMailMenuOpen)
    DrawLicenceMailMenu();

    if (treeChopConfirmOpen)
    DrawTreeChopConfirm();

    if (sceneFadeAlpha > 0f)
        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)sceneFadeAlpha));

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
    Program.DrawTextUI("OVERWRITE SAVE?", px + 120, py + 20, 28, Color.Gold);
    Program.DrawTextUI($"Slot {overwriteSlot + 1}: {name}", px + 30, py + 65, 22, Color.White);
    Program.DrawTextUI(info, px + 30, py + 95, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // confirm button
    Rectangle confirmBtn = new Rectangle(px + 40, py + 140, 180, 44);
    bool hoverConfirm = Raylib.CheckCollisionPointRec(mouse, confirmBtn);
    Raylib.DrawRectangleRec(confirmBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(confirmBtn, 2, hoverConfirm ? Color.Gold : Color.White);
    Program.DrawTextUI("OVERWRITE", px + 65, py + 153, 24, hoverConfirm ? Color.Gold : Color.White);

    // cancel button
    Rectangle cancelBtn = new Rectangle(px + 280, py + 140, 180, 44);
    bool hoverCancel = Raylib.CheckCollisionPointRec(mouse, cancelBtn);
    Raylib.DrawRectangleRec(cancelBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(cancelBtn, 2, hoverCancel ? Color.Red : Color.White);
    Program.DrawTextUI("CANCEL", px + 320, py + 153, 24, hoverCancel ? Color.Red : Color.White);

    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        overwriteConfirmOpen = false;
        overwriteSlot = -1;
    }

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
    Program.DrawTextUI("OPEN WORLD RPG", 312, 152, 64, new Color((byte)255,(byte)200,(byte)0,(byte)80));
    Program.DrawTextUI("OPEN WORLD RPG", 310, 150, 64, Color.Gold);

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

    if (!mainMenuChoice && !multiplayerMenuOpen)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        bool anySaveExists = savePaths.Any(p => System.IO.File.Exists(p));

        Rectangle newGameBtn = new Rectangle(ScreenWidth / 2 - 150, 360, 300, 60);
        bool hoverNew = Raylib.CheckCollisionPointRec(mouse, newGameBtn);
        Raylib.DrawRectangleRec(newGameBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(newGameBtn, 2, hoverNew ? Color.Gold : Color.White);
        Program.DrawTextUI("NEW GAME", ScreenWidth / 2 - 80, 378, 28, hoverNew ? Color.Gold : Color.White);

        Rectangle loadGameBtn = new Rectangle(ScreenWidth / 2 - 150, 440, 300, 60);
        bool hoverLoad = anySaveExists && Raylib.CheckCollisionPointRec(mouse, loadGameBtn);
        Color loadColor = anySaveExists ? (hoverLoad ? Color.Gold : Color.White) : Color.DarkGray;
        Raylib.DrawRectangleRec(loadGameBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(loadGameBtn, 2, loadColor);
        Program.DrawTextUI("LOAD GAME", ScreenWidth / 2 - 85, 458, 28, loadColor);

        Rectangle mpBtn = new Rectangle(ScreenWidth / 2 - 150, 520, 300, 60);
        bool hoverMp = Raylib.CheckCollisionPointRec(mouse, mpBtn);
        Raylib.DrawRectangleRec(mpBtn, new Color((byte)20,(byte)30,(byte)60,(byte)255));
        Raylib.DrawRectangleLinesEx(mpBtn, 2, hoverMp ? Color.SkyBlue : Color.White);
        Program.DrawTextUI("MULTIPLAYER", ScreenWidth / 2 - 100, 538, 28, hoverMp ? Color.SkyBlue : Color.White);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && hoverMp)
            multiplayerMenuOpen = true;

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            mainMenuChoice = false;
            playerName = "";
            nameEntered = false;
        }

        if (!anySaveExists)
            Program.DrawTextUI("No save files found", ScreenWidth / 2 - 100, 596, 20, Color.DarkGray);
    }
    else if (multiplayerMenuOpen)
    {
        Vector2 mouse = Raylib.GetMousePosition();

        Program.DrawTextUI("MULTIPLAYER", ScreenWidth / 2 - 110, 220, 40, Color.SkyBlue);

        // IP input label + box
        Program.DrawTextUI("Host IP to join:", ScreenWidth / 2 - 150, 295, 22, Color.LightGray);
        Raylib.DrawRectangle(ScreenWidth / 2 - 150, 325, 300, 40, new Color((byte)30,(byte)30,(byte)50,(byte)255));
        Raylib.DrawRectangleLines(ScreenWidth / 2 - 150, 325, 300, 40, Color.Gray);
        Program.DrawTextUI(multiplayerIPInput, ScreenWidth / 2 - 140, 337, 20, Color.White);
        if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            Program.DrawTextUI("|", ScreenWidth / 2 - 140 + Program.MeasureTextUI(multiplayerIPInput, 20), 337, 20, Color.White);

        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if ((key >= 48 && key <= 57) || key == 46)
                multiplayerIPInput += (char)key;
            key = Raylib.GetCharPressed();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && multiplayerIPInput.Length > 0)
            multiplayerIPInput = multiplayerIPInput[..^1];

        // HOST button
        Rectangle hostBtn = new Rectangle(ScreenWidth / 2 - 150, 390, 300, 60);
        bool hoverHost = Raylib.CheckCollisionPointRec(mouse, hostBtn);
        Raylib.DrawRectangleRec(hostBtn, new Color((byte)20,(byte)60,(byte)20,(byte)255));
        Raylib.DrawRectangleLinesEx(hostBtn, 2, hoverHost ? Color.Green : Color.Gray);
        Program.DrawTextUI("HOST GAME", ScreenWidth / 2 - 80, 408, 28, hoverHost ? Color.Green : Color.White);

        // JOIN button
        Rectangle joinBtn = new Rectangle(ScreenWidth / 2 - 150, 470, 300, 60);
        bool hoverJoin = Raylib.CheckCollisionPointRec(mouse, joinBtn);
        Raylib.DrawRectangleRec(joinBtn, new Color((byte)20,(byte)20,(byte)60,(byte)255));
        Raylib.DrawRectangleLinesEx(joinBtn, 2, hoverJoin ? Color.SkyBlue : Color.Gray);
        Program.DrawTextUI("JOIN GAME", ScreenWidth / 2 - 75, 488, 28, hoverJoin ? Color.SkyBlue : Color.White);

        // DISCONNECT button
        Rectangle discBtn = new Rectangle(ScreenWidth / 2 - 150, 550, 300, 60);
        bool hoverDisc = Raylib.CheckCollisionPointRec(mouse, discBtn) && multiplayer.Connected;
        Raylib.DrawRectangleRec(discBtn, new Color((byte)50,(byte)15,(byte)15,(byte)255));
        Raylib.DrawRectangleLinesEx(discBtn, 2, hoverDisc ? Color.Red : Color.DarkGray);
        Program.DrawTextUI("DISCONNECT", ScreenWidth / 2 - 85, 568, 28,
            multiplayer.Connected ? (hoverDisc ? Color.Red : Color.LightGray) : Color.DarkGray);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (hoverHost) multiplayer.StartHost();
            if (hoverJoin) multiplayer.StartClient(multiplayerIPInput);
            if (hoverDisc) multiplayer.Stop();
        }

        multiplayer.DrawStatusOverlay();

        Program.DrawTextUI("ESC = Back", ScreenWidth / 2 - 55, 635, 20, Color.LightGray);
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            multiplayerMenuOpen = false;
    }
    else if (!slotSelected)
    {
        // show save slots
        Program.DrawTextUI("SELECT SAVE SLOT", ScreenWidth / 2 - 130, 250, 28, Color.LightGray);
        Vector2 mouse = Raylib.GetMousePosition();

        for (int i = 0; i < 3; i++)
        {
            Rectangle slotBtn = new Rectangle(ScreenWidth / 2 - 250, 300 + i * 100, 500, 80);
            bool hover = Raylib.CheckCollisionPointRec(mouse, slotBtn);
            var (exists, name, info) = GetSlotInfo(i);

            Raylib.DrawRectangleRec(slotBtn, new Color((byte)30,(byte)30,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(slotBtn, 2, hover ? Color.Gold : Color.White);
            Program.DrawTextUI($"SLOT {i + 1}", (int)slotBtn.X + 20, (int)slotBtn.Y + 12, 22, hover ? Color.Gold : Color.White);

            if (exists)
            {
                Program.DrawTextUI(name, (int)slotBtn.X + 120, (int)slotBtn.Y + 12, 22, Color.White);
                Program.DrawTextUI(info, (int)slotBtn.X + 20, (int)slotBtn.Y + 46, 18, Color.LightGray);
            }
            else
            {
                Program.DrawTextUI("Empty Slot", (int)slotBtn.X + 120, (int)slotBtn.Y + 26, 22, Color.DarkGray);
            }
        }

        Program.DrawTextUI("ESC = Back", ScreenWidth / 2 - 50, 620, 20, Color.LightGray);
    }
    else if (slotSelected && !nameEntered)
    {
        Program.DrawTextUI("ENTER YOUR NAME:", 440, 320, 28, Color.LightGray);
        Raylib.DrawRectangle(420, 360, 440, 50, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(420, 360, 440, 50, Color.White);
        Program.DrawTextUI(playerName, 440, 375, 28, Color.White);

        if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            Program.DrawTextUI("|", 440 + Program.MeasureTextUI(playerName, 28), 375, 28, Color.White);

        Program.DrawTextUI("PRESS ENTER TO CONFIRM", 420, 430, 22, Color.LightGray);
    }
    else
    {
        Program.DrawTextUI($"Welcome, {playerName}!", 420, 320, 34, Color.White);

        if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            Program.DrawTextUI("PRESS ENTER TO START", 390, 390, 34, Color.White);
    }
}

// ── ACHIEVEMENTS ─────────────────────────────────────────────
       static void UpdateAchievements()
       {
           // track visited biomes for exploration achievements
           string biome = GetCurrentBiome();
           if (biome != null) achievementVisited.Add(biome);

           foreach (var a in achievements)
           {
               if (a.Unlocked) continue;
               try { if (!a.Condition()) continue; } catch { continue; }
               a.Unlocked = true;
               achievementsUnlockedCount++;
               if (a.Reward > 0) player.Money += a.Reward;
               achievementPopupTitle = a.Title;
               achievementPopupTimer = 4f;
               string rewardText = a.Reward > 0 ? $" +${a.Reward}" : "";
               ShowNotification($"Achievement: {a.Title}!{rewardText}");
           }
       }

// ── FOG OF WAR ──────────────────────────────────────────────
       static void UpdateFogOfWar()
       {
           int cellX = (int)((player.Position.X - FogOriginX) / FogCellSize);
           int cellY = (int)((player.Position.Y - FogOriginY) / FogCellSize);

           for (int dy = -FogRevealRadius; dy <= FogRevealRadius; dy++)
               for (int dx = -FogRevealRadius; dx <= FogRevealRadius; dx++)
               {
                   int gx = cellX + dx, gy = cellY + dy;
                   if (gx < 0 || gx >= FogCols || gy < 0 || gy >= FogRows) continue;
                   int idx = gy * FogCols + gx;
                   if (!fogRevealed[idx]) { fogRevealed[idx] = true; fogRevealedCount++; }
               }
       }

       static float GetExplorationPercent()
           => fogRevealedCount * 100f / (FogCols * FogRows);

       static void ResetFogOfWar()
       {
           Array.Clear(fogRevealed, 0, fogRevealed.Length);
           fogRevealedCount = 0;
       }

       static void DrawWorldMap()
{
    if (!mapOpen) return;

    int mapW = 900, mapH = 600;
    int mapX = ScreenWidth / 2 - mapW / 2;
    int mapY = ScreenHeight / 2 - mapH / 2;
    float scale = 0.003f * worldMapZoom;
    Program.DrawTextUI($"Scroll = Zoom ({worldMapZoom:F1}x)  |  0 = Reset  |  ESC or MAP to close", mapX + 10, mapY + 10, 14, Color.LightGray);

    Raylib.DrawRectangle(mapX, mapY, mapW, mapH, new Color((byte)10,(byte)10,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(mapX, mapY, mapW, mapH, Color.Gold);
    Program.DrawTextUI("WORLD MAP", mapX + mapW / 2 - 70, mapY + 10, 28, Color.Gold);

    int cx = mapX + mapW / 2;
    int cy = mapY + mapH / 2;

    // zoom in/out with scroll wheel or +/- keys
float mapScroll = Raylib.GetMouseWheelMove();
if (mapScroll != 0 && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(mapX, mapY, mapW, mapH)))
    worldMapZoom = Math.Clamp(worldMapZoom + mapScroll * 0.1f, 0.5f, 4f);
if (Raylib.IsKeyPressed(KeyboardKey.Equal)) worldMapZoom = Math.Clamp(worldMapZoom + 0.25f, 0.5f, 4f);
if (Raylib.IsKeyPressed(KeyboardKey.Minus)) worldMapZoom = Math.Clamp(worldMapZoom - 0.25f, 0.5f, 4f);
if (Raylib.IsKeyPressed(KeyboardKey.Zero))  worldMapZoom = 1f; // reset

    // 1. Base grasslands
Raylib.DrawRectangle(mapX, mapY, mapW, mapH, new Color((byte)90,(byte)170,(byte)90,(byte)255));

// 2. Forest 
// 2. Forest top band (X -20000 to 13000, Y < -12000)
int forestL = cx + (int)(-20000 * scale); int forestR = cx + (int)(13000 * scale);
int forestT = cy + (int)(-38000 * scale); int forestB = cy + (int)(-12000 * scale);
Raylib.DrawRectangle(Math.Clamp(forestL, mapX, mapX + mapW), Math.Clamp(forestT, mapY, mapY + mapH),
    Math.Clamp(forestR - forestL, 0, mapW), Math.Clamp(forestB - forestT, 0, mapH),
    new Color((byte)40,(byte)100,(byte)40,(byte)255));

// 3. Forest bottom band (X -20000 to 13000, Y > 12000)
int forestBL = cx + (int)(-20000 * scale); int forestBR = cx + (int)(13000 * scale);
int forestBT = cy + (int)(12000  * scale); int forestBB = cy + (int)(38000 * scale);
Raylib.DrawRectangle(Math.Clamp(forestBL, mapX, mapX + mapW), Math.Clamp(forestBT, mapY, mapY + mapH),
    Math.Clamp(forestBR - forestBL, 0, mapW), Math.Clamp(forestBB - forestBT, 0, mapH),
    new Color((byte)40,(byte)100,(byte)40,(byte)255));


// 4. Swamp (X -55000 to -30000, middle)
int swL = cx + (int)(-55000 * scale); int swR = cx + (int)(-30000 * scale);
int swT = cy + (int)(-12000 * scale); int swB = cy + (int)(12000  * scale);
Raylib.DrawRectangle(Math.Clamp(swL, mapX, mapX + mapW), Math.Clamp(swT, mapY, mapY + mapH),
    Math.Clamp(swR - swL, 0, mapW), Math.Clamp(swB - swT, 0, mapH),
    new Color((byte)55,(byte)75,(byte)35,(byte)255));

// 5. Snow (X -60000 to -30000, middle)
int snL = cx + (int)(-60000 * scale); int snR = cx + (int)(-30000 * scale);
int snT = cy + (int)(-12000 * scale); int snB = cy + (int)(12000 * scale);
Raylib.DrawRectangle(Math.Clamp(snL, mapX, mapX + mapW), Math.Clamp(snT, mapY, mapY + mapH),
    Math.Clamp(snR - snL, 0, mapW), Math.Clamp(snB - snT, 0, mapH),
    new Color((byte)220,(byte)235,(byte)255,(byte)255));

// 6. Desert (X 8000 to 22000, middle)
int deL = cx + (int)(8000  * scale); int deR = cx + (int)(22000 * scale);
int deT = cy + (int)(-12000 * scale); int deB = cy + (int)(12000  * scale);
Raylib.DrawRectangle(Math.Clamp(deL, mapX, mapX + mapW), Math.Clamp(deT, mapY, mapY + mapH),
    Math.Clamp(deR - deL, 0, mapW), Math.Clamp(deB - deT, 0, mapH),
    new Color((byte)210,(byte)180,(byte)100,(byte)255));

// 7. Beach (X 22000 to 28000, full height)
int beL = cx + (int)(22000 * scale); int beR = cx + (int)(28000 * scale);
Raylib.DrawRectangle(Math.Clamp(beL, mapX, mapX + mapW), mapY,
    Math.Clamp(beR - beL, 0, mapW), mapH, new Color((byte)240,(byte)220,(byte)150,(byte)255));

// 8. Ocean (X 28000+, full height)
int ocL = cx + (int)(28000 * scale);
Raylib.DrawRectangle(Math.Clamp(ocL, mapX, mapX + mapW), mapY,
    Math.Clamp(mapX + mapW - ocL, 0, mapW), mapH, new Color((byte)30,(byte)100,(byte)180,(byte)255));

// 9. Volcano (X 22000+, Y above -12000)
int voL = cx + (int)(22000  * scale); int voB = cy + (int)(-12000 * scale);
Raylib.DrawRectangle(Math.Clamp(voL, mapX, mapX + mapW), mapY,
    Math.Clamp(mapX + mapW - voL, 0, mapW), Math.Clamp(voB - mapY, 0, mapH),
    new Color((byte)40,(byte)20,(byte)10,(byte)255));

// 10. Mountains (X below -30000, Y above -12000)
int moL = cx + (int)(-30000 * scale); int moR = cx + (int)(-10000 * scale); 
int moT = cy + (int)(-38000 * scale); int moB = cy + (int)(23000 * scale);
Raylib.DrawRectangle(Math.Clamp(moL, mapX, mapX + mapW),
    Math.Clamp(moT, mapY, mapY + mapH), Math.Clamp(moR - moL, 0, mapW), Math.Clamp(moB - moT, 0, mapH),
    new Color((byte)100,(byte)95,(byte)90,(byte)255));

// 11. Safe zone
int szX  = cx + (int)(-3000 * scale); int szY  = cy + (int)(-1500 * scale);
int szW  = (int)(7000 * scale);       int szH  = (int)(4000 * scale);
Raylib.DrawRectangle(szX, szY, szW, szH, new Color((byte)90,(byte)170,(byte)90,(byte)255));
Raylib.DrawRectangleLines(szX, szY, szW, szH, new Color((byte)120,(byte)200,(byte)120,(byte)255));

// 12. Farm zone
int faX = cx + (int)(-3000  * scale); int faY = cy + (int)(-10000 * scale);
int faW = (int)(3000 * scale);        int faH = (int)(4000  * scale);
Raylib.DrawRectangle(Math.Clamp(faX, mapX, mapX + mapW), Math.Clamp(faY, mapY, mapY + mapH),
    Math.Clamp(faW, 0, mapW), Math.Clamp(faH, 0, mapH),
    new Color((byte)139,(byte)90,(byte)43,(byte)255));

// City of Hamiltron zone
int hmL = cx + (int)(11800 * scale); int hmR = cx + (int)(18200 * scale);
int hmT = cy + (int)(3000  * scale); int hmB = cy + (int)(8200  * scale);
Raylib.DrawRectangle(Math.Clamp(hmL, mapX, mapX+mapW), Math.Clamp(hmT, mapY, mapY+mapH),
    Math.Clamp(hmR-hmL,0,mapW), Math.Clamp(hmB-hmT,0,mapH), new Color((byte)160,(byte)160,(byte)175,(byte)200));
Raylib.DrawRectangleLines(Math.Clamp(hmL,mapX,mapX+mapW), Math.Clamp(hmT,mapY,mapY+mapH),
    Math.Clamp(hmR-hmL,0,mapW), Math.Clamp(hmB-hmT,0,mapH), new Color((byte)220,(byte)180,(byte)20,(byte)255));
Program.DrawTextUI("HAMILTRON", Math.Clamp(cx+(int)(13500*scale),mapX,mapX+mapW), Math.Clamp(cy+(int)(5500*scale),mapY,mapY+mapH), 12, new Color((byte)255,(byte)240,(byte)100,(byte)255));

// Rotoaira zone
int roL = cx + (int)(-18000 * scale); int roR = cx + (int)(-13800 * scale);
int roT = cy + (int)(3200   * scale); int roB = cy + (int)(6200   * scale);
Raylib.DrawRectangle(Math.Clamp(roL, mapX, mapX+mapW), Math.Clamp(roT, mapY, mapY+mapH),
    Math.Clamp(roR-roL,0,mapW), Math.Clamp(roB-roT,0,mapH), new Color((byte)160,(byte)160,(byte)175,(byte)200));
Raylib.DrawRectangleLines(Math.Clamp(roL,mapX,mapX+mapW), Math.Clamp(roT,mapY,mapY+mapH),
    Math.Clamp(roR-roL,0,mapW), Math.Clamp(roB-roT,0,mapH), new Color((byte)180,(byte)220,(byte)120,(byte)255));
Program.DrawTextUI("ROTOAIRA", Math.Clamp(cx+(int)(-16500*scale),mapX,mapX+mapW), Math.Clamp(cy+(int)(4400*scale),mapY,mapY+mapH), 12, new Color((byte)200,(byte)255,(byte)160,(byte)255));

    // Roads
    int roadY    = cy + (int)(550  * scale);
    int highwayX = cx + (int)(200  * scale);
    Raylib.DrawRectangle(mapX, roadY, mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(highwayX, mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    int desertRoadY = cy + (int)(200 * scale);
    Raylib.DrawRectangle(cx + (int)(8000 * scale), desertRoadY, mapX + mapW - (cx + (int)(8000 * scale)), Math.Max(2, (int)(120 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(mapX, desertRoadY, Math.Max(0, cx + (int)(-30000 * scale) - mapX), Math.Max(2, (int)(120 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // Ring roads
    int ringTopY   = cy + (int)(-38000 * scale);
    int ringBotY   = cy + (int)(38000  * scale);
    int ringLeftX  = cx + (int)(-40000 * scale);
    int ringRightX = cx + (int)(39820  * scale);
    Raylib.DrawRectangle(mapX, ringTopY,   mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(mapX, ringBotY,   mapW, Math.Max(2, (int)(180 * scale)), new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(ringLeftX,  mapY, Math.Max(2, (int)(180 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));
    Raylib.DrawRectangle(ringRightX, mapY, Math.Max(2, (int)(180 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    RoadManager.DrawOnWorldMap(cx, cy, scale, mapX, mapY, mapW, mapH);

    // Snow/Desert connectors
    int[] connX = { -20000, -10000, 15000, 25000 };
    foreach (int wx in connX)
        Raylib.DrawRectangle(cx + (int)(wx * scale), mapY, Math.Max(2, (int)(120 * scale)), mapH, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // Lakes
    foreach (Lake lake in lakes)
    {
        int lkX = cx + (int)(lake.Position.X * scale);
        int lkY = cy + (int)(lake.Position.Y * scale);
        Raylib.DrawCircle(lkX, lkY, (int)(120 * scale), new Color((byte)30,(byte)100,(byte)200,(byte)255));
        Program.DrawTextUI("Lake", lkX - 14, lkY - 8, 12, Color.White);
    }

    // Buildings
    foreach (Building building in buildings)
    {
        int bx = cx + (int)(building.Bounds.X * scale);
        int by = cy + (int)(building.Bounds.Y * scale);
        if (bx >= mapX && bx <= mapX + mapW && by >= mapY && by <= mapY + mapH)
        {
            Raylib.DrawRectangle(bx - 5, by - 5, 14, 14, Color.Yellow);
            Raylib.DrawRectangleLines(bx - 5, by - 5, 14, 14, Color.Gold);
        }
    }

    // Flags
    foreach (var f in placedFlags)
    {
        int fx = cx + (int)(f.X * scale);
        int fy = cy + (int)(f.Y * scale);
        if (fx > mapX && fx < mapX + mapW && fy > mapY && fy < mapY + mapH)
        {
            Raylib.DrawCircle(fx, fy, 4, Color.Red);
            Raylib.DrawCircleLines(fx, fy, 4, Color.White);
        }
    }

    // ── FOG OF WAR OVERLAY ──
    Raylib.BeginScissorMode(mapX, mapY, mapW, mapH);
    Color fogColor = new Color((byte)10,(byte)10,(byte)20,(byte)230);
    float cellScreenW = FogCellSize * scale;
    float cellScreenH = FogCellSize * scale;
    int visMinCX = Math.Max(0, (int)((mapX - cx) / scale - FogOriginX) / FogCellSize - 1);
    int visMaxCX = Math.Min(FogCols - 1, (int)((mapX + mapW - cx) / scale - FogOriginX) / FogCellSize + 1);
    int visMinCY = Math.Max(0, (int)((mapY - cy) / scale - FogOriginY) / FogCellSize - 1);
    int visMaxCY = Math.Min(FogRows - 1, (int)((mapY + mapH - cy) / scale - FogOriginY) / FogCellSize + 1);
    for (int fy = visMinCY; fy <= visMaxCY; fy++)
        for (int fx = visMinCX; fx <= visMaxCX; fx++)
        {
            if (fogRevealed[fy * FogCols + fx]) continue;
            int sx = cx + (int)((FogOriginX + fx * FogCellSize) * scale);
            int sy = cy + (int)((FogOriginY + fy * FogCellSize) * scale);
            Raylib.DrawRectangle(sx, sy, (int)cellScreenW + 1, (int)cellScreenH + 1, fogColor);
        }
    Raylib.EndScissorMode();

    string expText = $"Explored: {GetExplorationPercent():F1}%";
    Program.DrawTextUI(expText, mapX + mapW - 180, mapY + mapH - 90, 16, Color.Gold);

    // Player dot
    int px = cx + (int)(player.Position.X * scale);
    int py = cy + (int)(player.Position.Y * scale);
    px = Math.Clamp(px, mapX + 5, mapX + mapW - 5);
    py = Math.Clamp(py, mapY + 5, mapY + mapH - 5);
    Raylib.DrawCircle(px, py, 6, Color.White);
    Raylib.DrawCircleLines(px, py, 6, Color.Gold);
    Program.DrawTextUI(playerName, px + 10, py - 8, 14, Color.White);

    
    // Biome labels
    Program.DrawTextUI("FOREST",    mapX + mapW / 2 - 30, mapY + 8, 16, new Color((byte)150,(byte)255,(byte)150,(byte)255));
    Program.DrawTextUI("DESERT",    Math.Clamp(cx + (int)(12000 * scale), mapX, mapX + mapW), cy, 16, new Color((byte)255,(byte)220,(byte)100,(byte)255));
    Program.DrawTextUI("MOUNTAINS",      Math.Clamp(cx + (int)(-22000 * scale), mapX, mapX + mapW), cy, 16, new Color((byte)200,(byte)220,(byte)255,(byte)255));
    Program.DrawTextUI("SAFE ZONE", szX + 4, szY + 10, 14, new Color((byte)100,(byte)255,(byte)100,(byte)255));
    Program.DrawTextUI("SNOW",     Math.Clamp(cx + (int)(-44000 * scale), mapX, mapX + mapW), cy, 16, new Color((byte)200,(byte)235,(byte)255,(byte)255));
    Program.DrawTextUI("SWAMP",    Math.Clamp(cx + (int)(-44000 * scale), mapX, mapX + mapW), cy + 20, 14, new Color((byte)100,(byte)130,(byte)60,(byte)255));
    Program.DrawTextUI("VOLCANO",   Math.Clamp(cx + (int)(28000 * scale), mapX, mapX + mapW), mapY + 8, 14, new Color((byte)255,(byte)120,(byte)40,(byte)255));
    Program.DrawTextUI("MOUNTAINS", mapX + 4, mapY + 8, 14, new Color((byte)200,(byte)195,(byte)185,(byte)255));
    Program.DrawTextUI("BEACH",     Math.Clamp(cx + (int)(23000 * scale), mapX, mapX + mapW), cy + 20, 12, new Color((byte)240,(byte)220,(byte)150,(byte)255));
    Program.DrawTextUI("OCEAN",     Math.Clamp(cx + (int)(31000 * scale), mapX, mapX + mapW), cy, 12, new Color((byte)100,(byte)180,(byte)255,(byte)255));
    Program.DrawTextUI("FARM",      Math.Clamp(cx + (int)(-2000 * scale), mapX, mapX + mapW), Math.Clamp(cy + (int)(-8000 * scale), mapY, mapY + mapH), 14, new Color((byte)200,(byte)150,(byte)80,(byte)255));

    // Legend
    int lx = mapX + 10;
    int ly = mapY + mapH - 80;
    Raylib.DrawRectangle(lx, ly, mapW - 20, 70, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    (Color col, string label)[] legend = {
        (Color.White,                                        "= You"),
        (Color.Yellow,                                       "= Building"),
        (new Color((byte)30,(byte)100,(byte)200,(byte)255), "= Lake"),
        (new Color((byte)40,(byte)100,(byte)40,(byte)255),  "= Forest"),
        (new Color((byte)210,(byte)180,(byte)100,(byte)255),"= Desert"),
        (new Color((byte)220,(byte)235,(byte)255,(byte)255),"= Snow"),
        (new Color((byte)55,(byte)75,(byte)35,(byte)255),   "= Swamp"),
        (new Color((byte)40,(byte)20,(byte)10,(byte)255),   "= Volcano"),
        (new Color((byte)100,(byte)95,(byte)90,(byte)255),  "= Mountains"),
        (new Color((byte)240,(byte)220,(byte)150,(byte)255),"= Beach"),
        (new Color((byte)30,(byte)100,(byte)180,(byte)255), "= Ocean"),
        (new Color((byte)139,(byte)90,(byte)43,(byte)255),  "= Farm"),
    };
    for (int i = 0; i < legend.Length; i++)
    {
        int col = i % 6;
        int row = i / 6;
        int lbx = lx + 10 + col * 140;
        int lby = ly + 8 + row * 28;
        if (legend[i].label == "= You")
            Raylib.DrawCircle(lbx + 5, lby + 6, 5, legend[i].col);
        else
            Raylib.DrawRectangle(lbx, lby, 12, 12, legend[i].col);
        Program.DrawTextUI(legend[i].label, lbx + 16, lby, 14, Color.LightGray);
    }

    Program.DrawTextUI("ESC or MAP to close", mapX + mapW - 200, mapY + 10, 16, Color.LightGray);
}

        static void DrawPauseMenu()
{
    if (!pauseMenuOpen) return;

    //Raylib.PauseMusicStream(currentMusic);

    // dim background
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)150));

    // panel
    Raylib.DrawRectangle(ScreenWidth / 2 - 200, 20, 400, 680, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 - 200, 20, 400, 680, Color.Gold);
    Program.DrawTextUI("PAUSED", ScreenWidth / 2 - 70, 34, 40, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    string[] buttons = { "RESUME", "LOAD GAME", "OPTIONS", "CHEATS", "MAP", "UNSTUCK", "QUIT TO MENU" };

    for (int i = 0; i < buttons.Length; i++)
    {
        Rectangle btn = new Rectangle(ScreenWidth / 2 - 150, 90 + i * 80, 300, 55);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Program.DrawTextUI(buttons[i], (int)btn.X + 20, (int)btn.Y + 16, 26, hover ? Color.Gold : Color.White);

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
                case "UNSTUCK":
                    // find the nearest open position by trying directions
                    Vector2[] tries = {
                        player.Position + new Vector2(0,    120),
                        player.Position + new Vector2(0,   -120),
                        player.Position + new Vector2(120,  0),
                        player.Position + new Vector2(-120, 0),
                        player.Position + new Vector2(120,  120),
                        player.Position + new Vector2(-120,-120),
                        new Vector2(400, -50),   // safe zone spawn as last resort
                    };
                    foreach (var tryPos in tries)
                    {
                        Rectangle testRect = new Rectangle(tryPos.X - 20, tryPos.Y - 20, 40, 40);
                        bool blocked = false;
                        if (currentScene == SceneState.Building)
                        {
                            foreach (var obj in currentBuilding.InteriorObjects)
                                if (Raylib.CheckCollisionRecs(testRect, obj)) { blocked = true; break; }
                        }
                        if (!blocked)
                        {
                            player.Position = tryPos;
                            pauseMenuOpen   = false;
                            ShowNotification("Player unstuck!");
                            Raylib.PlaySound(soundPauseClose);
                            Raylib.ResumeMusicStream(currentMusic);
                            break;
                        }
                    }
                    break;
                case "QUIT TO MENU":
                    pauseMenuOpen = false;
                    multiplayer.Stop();
                    ChangeScene(SceneState.MainMenu);
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
    Program.DrawTextUI("LOAD GAME", ScreenWidth / 2 + 280, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < 3; i++)
    {
        Rectangle slotBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 190 + i * 85, 360, 70);
        bool hover = Raylib.CheckCollisionPointRec(mouse, slotBtn);
        var (exists, name, info) = GetSlotInfo(i);

        Raylib.DrawRectangleRec(slotBtn, new Color((byte)30, (byte)30, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(slotBtn, 2, hover ? Color.Gold : Color.White);
        Program.DrawTextUI($"SLOT {i + 1}", (int)slotBtn.X + 15, (int)slotBtn.Y + 10, 20, hover ? Color.Gold : Color.White);

        if (exists)
        {
            Program.DrawTextUI(name, (int)slotBtn.X + 110, (int)slotBtn.Y + 10, 20, Color.White);
            Program.DrawTextUI(info, (int)slotBtn.X + 15, (int)slotBtn.Y + 40, 16, Color.LightGray);
        }
        else
        {
            Program.DrawTextUI("Empty Slot", (int)slotBtn.X + 110, (int)slotBtn.Y + 25, 20, Color.DarkGray);
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
    Raylib.DrawRectangle(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 480, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 480, Color.Gold);
    Program.DrawTextUI("OPTIONS", ScreenWidth / 2 + 300, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    // ── DAY SPEED ────────────────────────────────────────────────────────────
    Program.DrawTextUI("Day Speed", ScreenWidth / 2 + 240, ScreenHeight / 2 - 180, 22, Color.White);
    Rectangle sliderBg = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, 300, 16);
    Raylib.DrawRectangleRec(sliderBg, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    float daySpeedNorm = (daySpeed - 0.001f) / (0.1f - 0.01f);
    Raylib.DrawRectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, (int)(300 * daySpeedNorm), 16, Color.Gold);
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 240, ScreenHeight / 2 - 150, 300, 16, Color.White);
    if (Raylib.IsMouseButtonDown(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, sliderBg))
    {
        daySpeedNorm = (mouse.X - (ScreenWidth / 2 + 240)) / 300f;
        daySpeedNorm = Math.Clamp(daySpeedNorm, 0f, 1f);
        daySpeed = 0.001f + daySpeedNorm * (0.1f - 0.001f);
    }
    Program.DrawTextUI($"{daySpeed:F3}", ScreenWidth / 2 + 555, ScreenHeight / 2 - 153, 18, Color.LightGray);

    // ── MUSIC VOLUME ─────────────────────────────────────────────────────────
    Program.DrawTextUI("Music Volume", ScreenWidth / 2 + 240, ScreenHeight / 2 - 110, 22, Color.White);
    Rectangle musicSliderBg = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 80, 260, 16);
    Raylib.DrawRectangleRec(musicSliderBg, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    Raylib.DrawRectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 80, (int)(260 * musicVolume), 16, new Color((byte)80, (byte)160, (byte)255, (byte)255));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 240, ScreenHeight / 2 - 80, 260, 16, Color.White);
    if (Raylib.IsMouseButtonDown(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, musicSliderBg))
    {
        musicVolume = Math.Clamp((mouse.X - (ScreenWidth / 2 + 240)) / 260f, 0f, 1f);
        Raylib.SetMusicVolume(currentMusic, musicVolume);
    }
    // minus button
    Rectangle musicMinus = new Rectangle(ScreenWidth / 2 + 510, ScreenHeight / 2 - 84, 28, 24);
    Rectangle musicPlus  = new Rectangle(ScreenWidth / 2 + 544, ScreenHeight / 2 - 84, 28, 24);
    bool hMusicMinus = Raylib.CheckCollisionPointRec(mouse, musicMinus);
    bool hMusicPlus  = Raylib.CheckCollisionPointRec(mouse, musicPlus);
    Raylib.DrawRectangleRec(musicMinus, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(musicMinus, 2, hMusicMinus ? Color.Gold : Color.White);
    Program.DrawTextUI("-", ScreenWidth / 2 + 519, ScreenHeight / 2 - 81, 20, Color.White);
    Raylib.DrawRectangleRec(musicPlus, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(musicPlus, 2, hMusicPlus ? Color.Gold : Color.White);
    Program.DrawTextUI("+", ScreenWidth / 2 + 552, ScreenHeight / 2 - 81, 20, Color.White);
    if (hMusicMinus && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        musicVolume = Math.Clamp(musicVolume - 0.1f, 0f, 1f);
        Raylib.SetMusicVolume(currentMusic, musicVolume);
    }
    if (hMusicPlus && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        musicVolume = Math.Clamp(musicVolume + 0.1f, 0f, 1f);
        Raylib.SetMusicVolume(currentMusic, musicVolume);
    }
    Program.DrawTextUI($"{(int)(musicVolume * 100)}%", ScreenWidth / 2 + 578, ScreenHeight / 2 - 80, 18, Color.LightGray);

    // ── SOUND EFFECTS VOLUME ─────────────────────────────────────────────────
    Program.DrawTextUI("Sound Effects", ScreenWidth / 2 + 240, ScreenHeight / 2 - 40, 22, Color.White);
    Rectangle soundSliderBg = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 10, 260, 16);
    Raylib.DrawRectangleRec(soundSliderBg, new Color((byte)60, (byte)60, (byte)60, (byte)255));
    Raylib.DrawRectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 10, (int)(260 * soundVolume), 16, new Color((byte)255, (byte)160, (byte)40, (byte)255));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 240, ScreenHeight / 2 - 10, 260, 16, Color.White);
    if (Raylib.IsMouseButtonDown(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, soundSliderBg))
    {
        soundVolume = Math.Clamp((mouse.X - (ScreenWidth / 2 + 240)) / 260f, 0f, 1f);
        Raylib.SetSoundVolume(soundTreeChop,    soundVolume);
        Raylib.SetSoundVolume(soundTreeFall,    soundVolume);
        Raylib.SetSoundVolume(soundRockHit,     soundVolume);
        Raylib.SetSoundVolume(soundRockBreak,   soundVolume);
        Raylib.SetSoundVolume(soundSwordSwing,  soundVolume);
        Raylib.SetSoundVolume(soundStickSwing,  soundVolume);
        Raylib.SetSoundVolume(soundDogHit,      soundVolume);
        Raylib.SetSoundVolume(soundDogDie,      soundVolume);
        Raylib.SetSoundVolume(soundHorseGallop, soundVolume);
        Raylib.SetSoundVolume(soundPauseOpen,   soundVolume);
        Raylib.SetSoundVolume(soundPauseClose,  soundVolume);
    }
    Rectangle soundMinus = new Rectangle(ScreenWidth / 2 + 510, ScreenHeight / 2 - 14, 28, 24);
    Rectangle soundPlus  = new Rectangle(ScreenWidth / 2 + 544, ScreenHeight / 2 - 14, 28, 24);
    bool hSoundMinus = Raylib.CheckCollisionPointRec(mouse, soundMinus);
    bool hSoundPlus  = Raylib.CheckCollisionPointRec(mouse, soundPlus);
    Raylib.DrawRectangleRec(soundMinus, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(soundMinus, 2, hSoundMinus ? Color.Gold : Color.White);
    Program.DrawTextUI("-", ScreenWidth / 2 + 519, ScreenHeight / 2 - 11, 20, Color.White);
    Raylib.DrawRectangleRec(soundPlus, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(soundPlus, 2, hSoundPlus ? Color.Gold : Color.White);
    Program.DrawTextUI("+", ScreenWidth / 2 + 552, ScreenHeight / 2 - 11, 20, Color.White);
    if (hSoundMinus && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        soundVolume = Math.Clamp(soundVolume - 0.1f, 0f, 1f);
        Raylib.SetSoundVolume(soundTreeChop,    soundVolume);
        Raylib.SetSoundVolume(soundTreeFall,    soundVolume);
        Raylib.SetSoundVolume(soundRockHit,     soundVolume);
        Raylib.SetSoundVolume(soundRockBreak,   soundVolume);
        Raylib.SetSoundVolume(soundSwordSwing,  soundVolume);
        Raylib.SetSoundVolume(soundStickSwing,  soundVolume);
        Raylib.SetSoundVolume(soundDogHit,      soundVolume);
        Raylib.SetSoundVolume(soundDogDie,      soundVolume);
        Raylib.SetSoundVolume(soundHorseGallop, soundVolume);
        Raylib.SetSoundVolume(soundPauseOpen,   soundVolume);
        Raylib.SetSoundVolume(soundPauseClose,  soundVolume);
    }
    if (hSoundPlus && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        soundVolume = Math.Clamp(soundVolume + 0.1f, 0f, 1f);
        Raylib.SetSoundVolume(soundTreeChop,    soundVolume);
        Raylib.SetSoundVolume(soundTreeFall,    soundVolume);
        Raylib.SetSoundVolume(soundRockHit,     soundVolume);
        Raylib.SetSoundVolume(soundRockBreak,   soundVolume);
        Raylib.SetSoundVolume(soundSwordSwing,  soundVolume);
        Raylib.SetSoundVolume(soundStickSwing,  soundVolume);
        Raylib.SetSoundVolume(soundDogHit,      soundVolume);
        Raylib.SetSoundVolume(soundDogDie,      soundVolume);
        Raylib.SetSoundVolume(soundHorseGallop, soundVolume);
        Raylib.SetSoundVolume(soundPauseOpen,   soundVolume);
        Raylib.SetSoundVolume(soundPauseClose,  soundVolume);
    }
    Program.DrawTextUI($"{(int)(soundVolume * 100)}%", ScreenWidth / 2 + 578, ScreenHeight / 2 - 10, 18, Color.LightGray);

    // ── MINIMAP SIZE ─────────────────────────────────────────────────────────
    Program.DrawTextUI("Minimap Size", ScreenWidth / 2 + 240, ScreenHeight / 2 + 50, 22, Color.White);
    Rectangle minimapBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 + 80, 160, 40);
    bool hoverMinimap = Raylib.CheckCollisionPointRec(mouse, minimapBtn);
    Raylib.DrawRectangleRec(minimapBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(minimapBtn, 2, hoverMinimap ? Color.Gold : Color.White);
    Program.DrawTextUI(minimapSize == 200 ? "Normal" : "Large", ScreenWidth / 2 + 270, ScreenHeight / 2 + 92, 22, hoverMinimap ? Color.Gold : Color.White);
    if (hoverMinimap && Raylib.IsMouseButtonPressed(MouseButton.Left))
        minimapSize = minimapSize == 200 ? 300 : 200;

    // ── RAIN TOGGLE ──────────────────────────────────────────────────────────
    Program.DrawTextUI("Rain", ScreenWidth / 2 + 240, ScreenHeight / 2 + 140, 22, Color.White);
    Rectangle rainBtn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 + 170, 160, 40);
    bool hoverRain = Raylib.CheckCollisionPointRec(mouse, rainBtn);
    Raylib.DrawRectangleRec(rainBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(rainBtn, 2, hoverRain ? Color.Gold : Color.White);
    Program.DrawTextUI(isRaining ? "ON" : "OFF", ScreenWidth / 2 + 270, ScreenHeight / 2 + 182, 22, isRaining ? Color.SkyBlue : Color.DarkGray);
    if (hoverRain && Raylib.IsMouseButtonPressed(MouseButton.Left))
        isRaining = !isRaining;
    
    // ── CONTROLS HUD TOGGLE ──────────────────────────────────────────────────
    Program.DrawTextUI("Controls Help", ScreenWidth / 2 + 410, ScreenHeight / 2 + 140, 22, Color.White);
    Rectangle controlsBtn = new Rectangle(ScreenWidth / 2 + 410, ScreenHeight / 2 + 170, 160, 40);
    bool hoverControls = Raylib.CheckCollisionPointRec(mouse, controlsBtn);
    Raylib.DrawRectangleRec(controlsBtn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLinesEx(controlsBtn, 2, hoverControls ? Color.Gold : Color.White);
    Program.DrawTextUI(showControlsHud ? "ON" : "OFF", ScreenWidth / 2 + 440, ScreenHeight / 2 + 182, 22, showControlsHud ? Color.Green : Color.DarkGray);
    if (hoverControls && Raylib.IsMouseButtonPressed(MouseButton.Left))
        showControlsHud = !showControlsHud;
}

static void DrawCheatsMenu()
{
    Raylib.DrawRectangle(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 400, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(ScreenWidth / 2 + 220, ScreenHeight / 2 - 250, 400, 400, Color.Gold);
    Program.DrawTextUI("CHEATS", ScreenWidth / 2 + 310, ScreenHeight / 2 - 235, 28, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    string[] cheats = { $"Add ${cheatGoldAmount} Gold", "Max Health", "Fill Inventory", "Max All Skills", "Clear Enemies", "Spawn Horse" };

    for (int i = 0; i < cheats.Length; i++)
    {
        Rectangle btn = new Rectangle(ScreenWidth / 2 + 240, ScreenHeight / 2 - 180 + i * 70, 320, 50);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        Program.DrawTextUI(cheats[i], (int)btn.X + 15, (int)btn.Y + 14, 22, hover ? Color.Gold : Color.White);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            timesCheated++;
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
                    TryGiveItem("Logs", 50); TryGiveItem("Fish", 50); TryGiveItem("Bones", 50);
                    TryGiveItem("Fur", 50); TryGiveItem("Stingers", 50); TryGiveItem("Pelts", 50);
                    TryGiveItem("Dog Fangs", 50);
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
                case 5:
                    rideables.Add(new Rideable(
                        player.Position + new Vector2(80, 0),
                        Rideable.RideableType.Horse,
                        new Color((byte)139,(byte)90,(byte)43,(byte)255)));
                    ShowNotification("Horse spawned next to you!");
                    break;
            }
        }
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

            DrawTutorialWorld();

// =====================
// BASE & BIOMES
// =====================

// Base fallback (shouldn't be visible)
Raylib.DrawRectangle(-55000, -40000, 95000, 80000, new Color((byte)90,(byte)170,(byte)90,(byte)255));

// FOREST — top band (Y -40000 to -12000) full width
Raylib.DrawRectangle(-55000, -40000, 95000, 28000, new Color((byte)40,(byte)100,(byte)40,(byte)255));

// FOREST — bottom band (Y 12000 to 40000) full width
Raylib.DrawRectangle(-55000, 12000, 95000, 28000, new Color((byte)40,(byte)100,(byte)40,(byte)255));

// SWAMP — far left (X -55000 to -30000, middle strip Y -12000 to 12000)
Raylib.DrawRectangle(-55000, -12000, 25000, 24000, new Color((byte)55,(byte)75,(byte)35,(byte)255));

// SNOW — left (X -30000 to -10000, middle strip)
Raylib.DrawRectangle(-60000, -15000, 30000, 30000, new Color((byte)220,(byte)235,(byte)255,(byte)255));

// SAFE ZONE — centre-left (X -10000 to -3000, middle strip + safe zone overlay)
//Raylib.DrawRectangle(-10, -12000, 7000, 24000, new Color((byte)90,(byte)170,(byte)90,(byte)255));

// GRASSLANDS — centre (X -3000 to 8000, middle strip)
Raylib.DrawRectangle(-10000, -12000, 18000, 24000, new Color((byte)140, (byte)195, (byte)80, (byte)255));

// DESERT — centre-right (X 8000 to 22000, middle strip)
Raylib.DrawRectangle(8000, -12000, 14000, 24000, new Color((byte)210,(byte)180,(byte)100,(byte)255));

// BEACH — right (X 22000 to 28000, full height)
Raylib.DrawRectangle(22000, -40000, 6000, 80000, new Color((byte)240,(byte)220,(byte)150,(byte)255));

// OCEAN — far right (X 28000 to 40000, full height)
Raylib.DrawRectangle(28000, -40000, 150, 80000, new Color((byte)230,(byte)210,(byte)160,(byte)255));   // sand/beach strip
Raylib.DrawRectangle(28150, -40000, 11850, 80000, new Color((byte)30,(byte)100,(byte)180,(byte)255));
// NEW — depth gradient: shallows → deep water
Raylib.DrawRectangle(30500, -40000, 3500, 80000, new Color((byte)24,(byte)85,(byte)165,(byte)255));
Raylib.DrawRectangle(34000, -40000, 6000, 80000, new Color((byte)17,(byte)62,(byte)140,(byte)255));

// NEW — animated shoreline foam + drifting wave crests (view-culled)
{
    float t = (float)Raylib.GetTime();
    float ovT = camera.Target.Y - ScreenHeight, ovB = camera.Target.Y + ScreenHeight;
    float ovL = camera.Target.X - ScreenWidth,  ovR = camera.Target.X + ScreenWidth;
    if (ovR > 28000)   // any water on screen at all
    {
        for (int wy = ((int)ovT / 240) * 240; wy < ovB; wy += 240)
        {
            if (wy < -39800 || wy > 39800) continue;
            float ph = MathF.Sin(t * 1.6f + wy * 0.013f);

            // foam lapping the sand strip
            int foamX = 28150 + (int)(ph * 14);
            Raylib.DrawLineEx(new Vector2(foamX, wy), new Vector2(foamX + 10, wy + 60),
                4, new Color((byte)235,(byte)245,(byte)255,(byte)150));

            // two offshore crest lines, out of phase
            int w1 = 28600 + (int)(ph * 50);
            int w2 = 29600 - (int)(ph * 70);
            if (w1 > ovL && w1 < ovR)
                Raylib.DrawLineEx(new Vector2(w1, wy + 40), new Vector2(w1 + 110, wy + 55),
                    3, new Color((byte)205,(byte)230,(byte)250,(byte)110));
            if (w2 > ovL && w2 < ovR)
                Raylib.DrawLineEx(new Vector2(w2, wy + 140), new Vector2(w2 + 90, wy + 150),
                    2, new Color((byte)185,(byte)220,(byte)245,(byte)85));
        }
    }
}

// VOLCANO — top right (X 22000 to 40000, Y -40000 to -12000)
Raylib.DrawRectangle(22000, -40000, 18000, 28000, new Color((byte)40,(byte)20,(byte)10,(byte)255));

// MOUNTAINS — top left (X -30000 to -10000, Y -15000 to 23000)
Raylib.DrawRectangle(-30000, -15000, 20000, 38000, new Color((byte)100,(byte)95,(byte)90,(byte)255));

// SAFE ZONE overlay (exact safe zone bounds, sits on top of grasslands)
Raylib.DrawRectangle(-3000, -1500, 7000, 4000, new Color((byte)90,(byte)170,(byte)90,(byte)255));

// FARM ZONE (north of safe zone)
Raylib.DrawRectangle(-3000, -10000, 3000, 4000, Color.Brown);

DrawSafeZoneTexture();
DrawBiomeTextures();

// forecourt road surface
Raylib.DrawRectangle(300, -1000, 700, 580, Color.DarkGray);

// Roads draw
RoadManager.DrawAll(camera.Target, ScreenWidth, ScreenHeight, camera.Zoom);
CarparkManager.DrawAll(camera.Target, ScreenWidth, ScreenHeight, camera.Zoom);

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

// ── CITY OF HAMILTRON (X 12000-18000, Y 3000-8000) ──────────────────────────

// Main boulevard (horizontal spine)
Raylib.DrawRectangle(11800, 5500, 6400, 160, Color.DarkGray);
// North avenue (vertical spine)
Raylib.DrawRectangle(14800, 3000, 160, 5200, Color.DarkGray);
// East-west cross streets
Raylib.DrawRectangle(11800, 3900, 6400, 120, Color.DarkGray);
Raylib.DrawRectangle(11800, 7200, 6400, 120, Color.DarkGray);
// North-south cross streets
Raylib.DrawRectangle(12800, 3000, 120, 5200, Color.DarkGray);
Raylib.DrawRectangle(16600, 3000, 120, 5200, Color.DarkGray);
Raylib.DrawRectangle(17800, 3000, 120, 5200, Color.DarkGray);
// Connector from main road (Y 200) down to city boulevard
Raylib.DrawRectangle(14800, 200, 160, 3700, Color.DarkGray);
// City ring loop
Raylib.DrawRectangle(11800, 3000, 6200, 120, Color.DarkGray); // north ring
Raylib.DrawRectangle(11800, 8100, 6200, 120, Color.DarkGray); // south ring
Raylib.DrawRectangle(11800, 3000, 120, 5220, Color.DarkGray); // west ring
Raylib.DrawRectangle(17880, 3000, 120, 5220, Color.DarkGray); // east ring

// ── INTERSECTIONS — city (painted boxes + stop lines) ────────────────────────
// Intersection: boulevard x north avenue
Raylib.DrawRectangle(14800, 5500, 160, 160, Color.DarkGray);
Raylib.DrawRectangle(14790, 5490, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255)); // stop N
Raylib.DrawRectangle(14790, 5660, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255)); // stop S
Raylib.DrawRectangle(14790, 5490, 10, 180, new Color((byte)220,(byte)220,(byte)60,(byte)255)); // stop W
Raylib.DrawRectangle(14960, 5490, 10, 180, new Color((byte)220,(byte)220,(byte)60,(byte)255)); // stop E
// Zebra crossing — boulevard x north avenue (east approach)
for (int z = 0; z < 5; z++)
    Raylib.DrawRectangle(14970 + z * 16, 5510, 10, 140, new Color((byte)240,(byte)240,(byte)240,(byte)200));

// Intersection: east cross x north avenue
Raylib.DrawRectangle(14800, 3900, 160, 120, Color.DarkGray);
Raylib.DrawRectangle(14790, 3892, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
Raylib.DrawRectangle(14790, 4012, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));

// Intersection: boulevard x west cross
Raylib.DrawRectangle(12800, 5500, 120, 160, Color.DarkGray);
Raylib.DrawRectangle(12792, 5490, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
Raylib.DrawRectangle(12792, 5660, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
// Zebra W
for (int z = 0; z < 5; z++)
    Raylib.DrawRectangle(12808 + z * 16, 5670, 10, 120, new Color((byte)240,(byte)240,(byte)240,(byte)200));

// Intersection: south cross x north avenue
Raylib.DrawRectangle(14800, 7200, 160, 120, Color.DarkGray);
Raylib.DrawRectangle(14790, 7192, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
Raylib.DrawRectangle(14790, 7312, 180, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));

// ── COUNTRY TOWN OF ROTOAIRA (X -14000 to -18000, Y 3000-6000) ──────────────

// Main street (horizontal)
Raylib.DrawRectangle(-18000, 4400, 4200, 140, Color.DarkGray);
// Two side streets (vertical)
Raylib.DrawRectangle(-17200, 3200, 120, 3000, Color.DarkGray);
Raylib.DrawRectangle(-15400, 3200, 120, 3000, Color.DarkGray);
// Loop road around town
Raylib.DrawRectangle(-18000, 3200, 4200, 120, Color.DarkGray); // north loop
Raylib.DrawRectangle(-18000, 6100, 4200, 120, Color.DarkGray); // south loop
Raylib.DrawRectangle(-18000, 3200, 120, 3020, Color.DarkGray); // west loop
Raylib.DrawRectangle(-13920, 3200, 120, 3020, Color.DarkGray); // east loop
// Connector from snow side road (Y 260) down to town
Raylib.DrawRectangle(-16200, 260, 120, 3060, Color.DarkGray);

// Town intersection: main street x left side street
Raylib.DrawRectangle(-17200, 4400, 120, 140, Color.DarkGray);
Raylib.DrawRectangle(-17210, 4392, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
Raylib.DrawRectangle(-17210, 4534, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
// Zebra
for (int z = 0; z < 4; z++)
    Raylib.DrawRectangle(-17192 + z * 16, 4545, 10, 100, new Color((byte)240,(byte)240,(byte)240,(byte)200));

// Town intersection: main street x right side street
Raylib.DrawRectangle(-15400, 4400, 120, 140, Color.DarkGray);
Raylib.DrawRectangle(-15410, 4392, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));
Raylib.DrawRectangle(-15410, 4534, 140, 10, new Color((byte)220,(byte)220,(byte)60,(byte)255));

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
            
DrawSplats();
DrawDeathFx();

// ── TUTORIAL GUIDE ARROW ──
    if (tutorialActive && tutorialStep < tutorialTasks.Count)
    {
        Vector2 gp = tutorialTasks[tutorialStep].GuidePos;
        // blink on/off ~3 times per second
        bool blinkOn = ((int)(Raylib.GetTime() * 3) % 2) == 0;
        if (blinkOn)
        {
            // bob up and down
            float bob = (float)Math.Sin(Raylib.GetTime() * 4) * 10f;
            float ax = gp.X;
            float ay = gp.Y - 70 + bob;   // hover above the target

            // arrow shaft
            Raylib.DrawRectangle((int)ax - 8, (int)ay - 40, 16, 40, Color.Gold);
            // arrowhead (pointing down at the spot)
            Raylib.DrawTriangle(
                new Vector2(ax,      ay + 20),   // tip (bottom)
                new Vector2(ax - 22, ay),        // left
                new Vector2(ax + 22, ay),        // right
                Color.Gold);
            // outline for contrast
            Raylib.DrawTriangleLines(
                new Vector2(ax,      ay + 20),
                new Vector2(ax - 22, ay),
                new Vector2(ax + 22, ay),
                Color.Black);
        }
    }

            foreach (var ft in floatingTexts)
            {
            byte alpha = (byte)(255 * (ft.Timer / 1.2f));
            Program.DrawTextUI(ft.Text, (int)ft.Position.X, (int)ft.Position.Y, 22,
                new Color(ft.TextColor.R, ft.TextColor.G, ft.TextColor.B, alpha));
            }

            foreach (Building building in buildings)
            {
                building.Draw();
            }

            foreach (var garage in garages)
            {
                Color bodyCol = garage.IsDock
                    ? new Color((byte)150,(byte)110,(byte)60,(byte)255)
                    : new Color((byte)120,(byte)120,(byte)130,(byte)255);
                Raylib.DrawRectangle((int)garage.Bounds.X, (int)garage.Bounds.Y, (int)garage.Bounds.Width, (int)garage.Bounds.Height, bodyCol);
                Raylib.DrawRectangleLinesEx(garage.Bounds, 3, new Color((byte)70,(byte)70,(byte)80,(byte)255));

                if (!garage.IsDock)
                    for (int d = 0; d < garage.Capacity; d++)
                    {
                        int doorW = (int)(garage.Bounds.Width / garage.Capacity) - 12;
                        int doorX = (int)garage.Bounds.X + 6 + d * (doorW + 12);
                        Raylib.DrawRectangle(doorX, (int)garage.Bounds.Y + 24, doorW, (int)garage.Bounds.Height - 30,
                            new Color((byte)90,(byte)90,(byte)100,(byte)255));
                        for (int ln = (int)garage.Bounds.Y + 30; ln < garage.Bounds.Y + garage.Bounds.Height - 10; ln += 10)
                            Raylib.DrawLine(doorX, ln, doorX + doorW, ln, new Color((byte)70,(byte)70,(byte)80,(byte)255)); // door slats
                    }

                Program.DrawTextUI(garage.IsDock ? "BOAT DOCK" : $"GARAGE ({garage.Capacity}-CAR)",
                    (int)garage.Bounds.X + 10, (int)garage.Bounds.Y - 20, 16, Color.White);

                for (int j = 0; j < garage.Slots.Count; j++)
                    Program.DrawTextUI(garage.Slots[j].type.ToString(),
                        (int)garage.Bounds.X + 10, (int)garage.Bounds.Y + 6 + j * 16, 14, Color.LightGray);
            }

            foreach (var st in stables)
{
    int sx = (int)st.Bounds.X, sy = (int)st.Bounds.Y, sw = (int)st.Bounds.Width, sh = (int)st.Bounds.Height;
    switch (st.Kind)
    {
        case Stable.StableKind.Basic:
        case Stable.StableKind.Advanced:
            bool adv = st.Kind == Stable.StableKind.Advanced;
            Raylib.DrawRectangle(sx, sy, sw, sh, new Color((byte)140,(byte)90,(byte)45,(byte)255));           // timber walls
            Raylib.DrawRectangle(sx - 6, sy - 18, sw + 12, 22, new Color((byte)110,(byte)60,(byte)25,(byte)255)); // roof
            Raylib.DrawRectangle(sx + sw/2 - 22, sy + sh - 44, 44, 44, new Color((byte)70,(byte)45,(byte)20,(byte)255)); // door
            Raylib.DrawRectangle(sx + sw/2 - 22, sy + sh - 24, 44, 4, new Color((byte)50,(byte)30,(byte)12,(byte)255));  // half-door rail
            for (int fx = sx - 40; fx < sx; fx += 14)                                                          // paddock fence stubs
                Raylib.DrawRectangle(fx, sy + sh - 10, 4, 14, new Color((byte)120,(byte)80,(byte)40,(byte)255));
            if (adv)
            {
                Raylib.DrawRectangleLines(sx - 4, sy - 4, sw + 8, sh + 8, new Color((byte)90,(byte)90,(byte)100,(byte)255)); // steel reinforcement
                Program.DrawTextUI("ADV. STABLE", sx + 8, sy - 14, 12, Color.Gold);
            }
            else Program.DrawTextUI("STABLE", sx + 8, sy - 14, 12, new Color((byte)255,(byte)230,(byte)180,(byte)255));
            break;

        case Stable.StableKind.BikeRack:
            for (int i = 0; i < 4; i++)                                                                        // hoop rack
            {
                Raylib.DrawCircleLines(sx + 14 + i * 22, sy + sh - 12, 10, new Color((byte)150,(byte)150,(byte)160,(byte)255));
                Raylib.DrawRectangle(sx + 12 + i * 22, sy + sh - 12, 4, 12, new Color((byte)110,(byte)110,(byte)120,(byte)255));
            }
            break;

        case Stable.StableKind.Aquatic:
            Raylib.DrawRectangle(sx, sy, sw, sh, new Color((byte)60,(byte)140,(byte)190,(byte)200));           // water pen
            Raylib.DrawRectangleLinesEx(st.Bounds, 4, new Color((byte)200,(byte)190,(byte)150,(byte)255));     // floating boom
            Raylib.DrawRectangle(sx + sw - 20, sy - 30, 20, sh + 30, new Color((byte)150,(byte)110,(byte)60,(byte)255)); // dock walkway
            for (int py2 = sy - 24; py2 < sy + sh; py2 += 26)
                Raylib.DrawRectangle(sx + sw - 22, py2, 24, 5, new Color((byte)120,(byte)85,(byte)45,(byte)255));       // dock planks
            break;
    }

    // occupancy prompt (mirrors garage prompt)
    Vector2 sc = new Vector2(st.Bounds.X + st.Bounds.Width / 2, st.Bounds.Y + st.Bounds.Height / 2);
    if (st.HasAnimal && Vector2.Distance(player.Center, sc) < 150)
        Program.DrawTextUI($"E = Take out ({st.Slots.Count}/{st.Capacity})", (int)sc.X - 70, (int)sc.Y - 90, 18, Color.White);
}

            foreach (var b in decorativeBuildings)
            b.Draw();

            foreach (var a in decorativeAssets)
            a.Draw();

            DrawShrines();

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
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)200,(byte)160,(byte)0,(byte)220));
    Program.DrawTextUI("McDONALD'S", (int)bx + 22, (int)by - 9, 14, Color.White);
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
    Raylib.DrawRectangle((int)bx + 30, (int)by - 10, 240, 14, new Color((byte)10,(byte)80,(byte)160,(byte)220));
    Program.DrawTextUI("SWIMMING COMPLEX", (int)bx + 32, (int)by - 9, 14, Color.White);
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
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)0,(byte)55,(byte)130,(byte)220));
    Program.DrawTextUI("DOMINO'S", (int)bx + 30, (int)by - 9, 14, Color.White);
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
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)160,(byte)15,(byte)15,(byte)220));
    Program.DrawTextUI("KFC", (int)bx + 72, (int)by - 9, 14, Color.White);
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
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)185,(byte)15,(byte)15,(byte)220));
    Program.DrawTextUI("BURGER KING", (int)bx + 22, (int)by - 9, 14, Color.White);
}

if (building.BuildingName == "TENNIS COURT")
{
    
    Raylib.DrawRectangle((int)bx, (int)by, 200, 120, new Color((byte)40,(byte)140,(byte)40,(byte)255));
    // Court markings
    Raylib.DrawRectangle((int)bx + 5, (int)by + 5, 190, 110, new Color((byte)50,(byte)160,(byte)50,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 10, 180, 100, Color.White);
    Raylib.DrawRectangle((int)bx + 98, (int)by + 10, 4, 100, Color.White); // net
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)30,(byte)100,(byte)30,(byte)220));
    Program.DrawTextUI("TENNIS COURT", (int)bx + 22, (int)by - 9, 14, Color.White);
}

if (building.BuildingName == "BASKETBALL COURT")
{
    
    Raylib.DrawRectangle((int)bx, (int)by, 200, 120, new Color((byte)180,(byte)90,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 5, (int)by + 5, 190, 110, new Color((byte)210,(byte)120,(byte)30,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 10, 180, 100, Color.White);
    Raylib.DrawCircleLines((int)bx + 100, (int)by + 60, 25, Color.White);
    Raylib.DrawRectangle((int)bx + 20, (int)by - 10, 160, 14, new Color((byte)140,(byte)70,(byte)10,(byte)220));
    Program.DrawTextUI("BASKETBALL", (int)bx + 22, (int)by - 9, 14, Color.White);
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
        Program.DrawTextUI("MARAE", (int)bx + 55, (int)by - 20, 22, new Color((byte)220,(byte)140,(byte)60,(byte)255));
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
        Program.DrawTextUI("WAIKATO BANK", (int)bx + 54, (int)by - 13, 14, new Color((byte)240,(byte)210,(byte)80,(byte)255));
        Raylib.DrawRectangle((int)bx + 70, (int)by + 155, 100, 14, new Color((byte)180,(byte)160,(byte)100,(byte)255));
        Raylib.DrawRectangle((int)bx + 80, (int)by + 163, 80, 12, new Color((byte)190,(byte)170,(byte)110,(byte)255));
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
        Program.DrawTextUI("DBar", (int)bx + 42, (int)by - 20, 16, new Color((byte)255,(byte)30,(byte)30,(byte)255));
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

    foreach (var garage in garages)
{
    Vector2 garageCenter = new Vector2(garage.Bounds.X + garage.Bounds.Width / 2, garage.Bounds.Y + garage.Bounds.Height / 2);
    if (garage.HasVehicle && Vector2.Distance(player.Center, garageCenter) < 150)
    {
        Program.DrawTextUI("E = Take vehicle from garage", (int)garageCenter.X - 90, (int)garageCenter.Y - 80, 18, Color.White);
    }
}

    if (building.BuildingName == "PRISON")
    {
        DrawPrisonExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "SCHOOL")
    {
        DrawSchoolExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "FamilyHub")  
    {
        DrawFamilyHubExterior(building.Bounds.X, building.Bounds.Y); 
        continue;
    }
    if (building.BuildingName == "BEST START") 
    {
        DrawDaycareExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    } 
     if (building.BuildingName == "CASTLE")
    {
        DrawCastleExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "MALL")
    {
        DrawMallExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "HOBBIES STORE")
    {
        DrawHobbiesStoreExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "BOAT LICENCE OFFICE")
    {
        DrawBoatLicenceExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "LIBRARY")
    {
        DrawLibraryExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "FARMING SHOP")
    {
        DrawFarmingShopExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "BARN")
    {
        DrawBarnExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }
    if (building.BuildingName == "ZOO")
    {
        DrawZooExterior(building.Bounds.X, building.Bounds.Y);
        continue;
    }


    // ── MAGIC SHOP EXTERIOR ───────────────────────────────────────────────────
    if (building.BuildingName == "MAGIC SHOP")
    {
        // main body — deep indigo
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)35,(byte)10,(byte)70,(byte)255));
        // roof — pointed gothic style, two layers
        Raylib.DrawRectangle((int)bx - 6, (int)by - 14, 172, 16, new Color((byte)20,(byte)5,(byte)45,(byte)255));
        Raylib.DrawRectangle((int)bx - 6, (int)by - 14, 172, 4,  new Color((byte)120,(byte)40,(byte)200,(byte)255));
        // corner spires
        Raylib.DrawTriangle(new Vector2((int)bx - 6, (int)by - 14),
            new Vector2((int)bx + 8,  (int)by - 14),
            new Vector2((int)bx + 1,  (int)by - 30),
            new Color((byte)20,(byte)5,(byte)45,(byte)255));
        Raylib.DrawTriangle(new Vector2((int)bx + 158, (int)by - 14),
            new Vector2((int)bx + 172, (int)by - 14),
            new Vector2((int)bx + 165, (int)by - 30),
            new Color((byte)20,(byte)5,(byte)45,(byte)255));
        // glowing orb window left
        Raylib.DrawCircle((int)bx + 30, (int)by + 36, 18, new Color((byte)20,(byte)5,(byte)45,(byte)255));
        Raylib.DrawCircle((int)bx + 30, (int)by + 36, 16, new Color((byte)80,(byte)20,(byte)140,(byte)180));
        Raylib.DrawCircle((int)bx + 30, (int)by + 36, 9,  new Color((byte)160,(byte)60,(byte)255,(byte)220));
        Raylib.DrawCircle((int)bx + 30, (int)by + 36, 4,  new Color((byte)220,(byte)180,(byte)255,(byte)255));
        Raylib.DrawCircleLines((int)bx + 30, (int)by + 36, 18, new Color((byte)120,(byte)40,(byte)200,(byte)255));
        // glowing orb window right
        Raylib.DrawCircle((int)bx + 130, (int)by + 36, 18, new Color((byte)20,(byte)5,(byte)45,(byte)255));
        Raylib.DrawCircle((int)bx + 130, (int)by + 36, 16, new Color((byte)80,(byte)20,(byte)140,(byte)180));
        Raylib.DrawCircle((int)bx + 130, (int)by + 36, 9,  new Color((byte)160,(byte)60,(byte)255,(byte)220));
        Raylib.DrawCircle((int)bx + 130, (int)by + 36, 4,  new Color((byte)220,(byte)180,(byte)255,(byte)255));
        Raylib.DrawCircleLines((int)bx + 130, (int)by + 36, 18, new Color((byte)120,(byte)40,(byte)200,(byte)255));
        // arcane rune above door (star shape)
        Raylib.DrawCircleLines((int)bx + 80, (int)by + 60, 10, new Color((byte)160,(byte)60,(byte)255,(byte)200));
        Raylib.DrawLine((int)bx + 80, (int)by + 50, (int)bx + 80, (int)by + 70, new Color((byte)120,(byte)40,(byte)200,(byte)180));
        Raylib.DrawLine((int)bx + 70, (int)by + 60, (int)bx + 90, (int)by + 60, new Color((byte)120,(byte)40,(byte)200,(byte)180));
        // door — dark with gold trim
        Raylib.DrawRectangle((int)bx + 60, (int)by + 72, 40, 48, new Color((byte)15,(byte)5,(byte)30,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by + 72, 40, 48, new Color((byte)120,(byte)40,(byte)200,(byte)255));
        Raylib.DrawRectangle((int)bx + 62, (int)by + 74, 16, 22, new Color((byte)25,(byte)8,(byte)50,(byte)255));
        Raylib.DrawRectangle((int)bx + 82, (int)by + 74, 15, 22, new Color((byte)25,(byte)8,(byte)50,(byte)255));
        Raylib.DrawCircle((int)bx + 96, (int)by + 94, 3, new Color((byte)200,(byte)160,(byte)40,(byte)255));
        // hanging neon sign
        Raylib.DrawRectangle((int)bx + 10, (int)by - 28, 140, 16, new Color((byte)10,(byte)3,(byte)22,(byte)240));
        Raylib.DrawRectangleLines((int)bx + 10, (int)by - 28, 140, 16, new Color((byte)160,(byte)60,(byte)255,(byte)255));
        Program.DrawTextUI("MAGIC SHOP", (int)bx + 16, (int)by - 26, 14, new Color((byte)200,(byte)120,(byte)255,(byte)255));
        // sign chains
        Raylib.DrawLine((int)bx + 20,  (int)by - 28, (int)bx + 20,  (int)by - 14, new Color((byte)120,(byte)40,(byte)200,(byte)200));
        Raylib.DrawLine((int)bx + 140, (int)by - 28, (int)bx + 140, (int)by - 14, new Color((byte)120,(byte)40,(byte)200,(byte)200));
        // step
        Raylib.DrawRectangle((int)bx + 50, (int)by + 118, 60, 8, new Color((byte)30,(byte)10,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx + 54, (int)by + 124, 52, 4, new Color((byte)50,(byte)20,(byte)90,(byte)255));
    }

    // ── RANGING SHOP EXTERIOR ─────────────────────────────────────────────────
    if (building.BuildingName == "RANGING SHOP")
    {
        // main body — dark wood brown
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)80,(byte)50,(byte)20,(byte)255));
        // roof band — leather tan
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 14, new Color((byte)55,(byte)30,(byte)8,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 4,  new Color((byte)100,(byte)60,(byte)20,(byte)255));
        // wood grain lines on body
        for (int wg = 0; wg < 5; wg++)
            Raylib.DrawRectangle((int)bx + wg * 32, (int)by, 2, 120, new Color((byte)60,(byte)35,(byte)10,(byte)120));
        // left window — arrow display
        Raylib.DrawRectangle((int)bx + 8,  (int)by + 14, 40, 50, new Color((byte)160,(byte)130,(byte)80,(byte)160));
        Raylib.DrawRectangleLines((int)bx + 8, (int)by + 14, 40, 50, new Color((byte)55,(byte)30,(byte)8,(byte)255));
        // crossed arrows in window
        Raylib.DrawLine((int)bx + 10, (int)by + 16, (int)bx + 46, (int)by + 62, new Color((byte)180,(byte)120,(byte)40,(byte)255));
        Raylib.DrawLine((int)bx + 46, (int)by + 16, (int)bx + 10, (int)by + 62, new Color((byte)180,(byte)120,(byte)40,(byte)255));
        // right window — bow display
        Raylib.DrawRectangle((int)bx + 112, (int)by + 14, 40, 50, new Color((byte)160,(byte)130,(byte)80,(byte)160));
        Raylib.DrawRectangleLines((int)bx + 112, (int)by + 14, 40, 50, new Color((byte)55,(byte)30,(byte)8,(byte)255));
        // bow arc in window
        Raylib.DrawCircleLines((int)bx + 132, (int)by + 39, 16, new Color((byte)180,(byte)120,(byte)40,(byte)255));
        Raylib.DrawRectangle((int)bx + 130, (int)by + 16, 4, 46, new Color((byte)55,(byte)30,(byte)8,(byte)255)); // mask half
        // door — dark wood
        Raylib.DrawRectangle((int)bx + 60, (int)by + 72, 40, 48, new Color((byte)55,(byte)32,(byte)10,(byte)255));
        Raylib.DrawRectangle((int)bx + 63, (int)by + 75, 16, 20, new Color((byte)70,(byte)42,(byte)14,(byte)255));
        Raylib.DrawRectangle((int)bx + 81, (int)by + 75, 15, 20, new Color((byte)70,(byte)42,(byte)14,(byte)255));
        Raylib.DrawCircle((int)bx + 95, (int)by + 95, 3, new Color((byte)180,(byte)140,(byte)60,(byte)255));
        // hanging sign
        Raylib.DrawRectangle((int)bx + 25, (int)by - 28, 110, 18, new Color((byte)55,(byte)32,(byte)10,(byte)240));
        Raylib.DrawRectangleLines((int)bx + 25, (int)by - 28, 110, 18, new Color((byte)160,(byte)110,(byte)40,(byte)255));
        Program.DrawTextUI("FLETCHER", (int)bx + 34, (int)by - 26, 14, new Color((byte)220,(byte)180,(byte)80,(byte)255));
        // sign chains
        Raylib.DrawLine((int)bx + 35,  (int)by - 28, (int)bx + 35,  (int)by - 14, new Color((byte)160,(byte)110,(byte)40,(byte)255));
        Raylib.DrawLine((int)bx + 125, (int)by - 28, (int)bx + 125, (int)by - 14, new Color((byte)160,(byte)110,(byte)40,(byte)255));
        // step
        Raylib.DrawRectangle((int)bx + 50, (int)by + 118, 60, 8, new Color((byte)100,(byte)65,(byte)25,(byte)255));
    }

//KiwiCuts Exterior
    if (building.BuildingName == "KiwiCuts")
{
    // main body - cream/white
    Raylib.DrawRectangle((int)bx, (int)by, 140, 110, new Color((byte)240,(byte)240,(byte)235,(byte)255));
    // roof trim - teal/green NZ vibe
    Raylib.DrawRectangle((int)bx - 4, (int)by - 6, 148, 8, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle((int)bx - 4, (int)by - 6, 148, 3, new Color((byte)40,(byte)200,(byte)170,(byte)255));
    // neon sign
    Raylib.DrawRectangle((int)bx + 10, (int)by - 22, 120, 18, new Color((byte)10,(byte)10,(byte)10,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by - 22, 120, 18, new Color((byte)20,(byte)160,(byte)140,(byte)255));
    Program.DrawTextUI("KiwiCuts", (int)bx + 18, (int)by - 20, 16, new Color((byte)40,(byte)220,(byte)180,(byte)255));
    // barber pole
    Raylib.DrawRectangle((int)bx + 4, (int)by + 20, 8, 60, Color.White);
    Raylib.DrawRectangle((int)bx + 4, (int)by + 20, 8, 10, Color.Red);
    Raylib.DrawRectangle((int)bx + 4, (int)by + 36, 8, 10, Color.Red);
    Raylib.DrawRectangle((int)bx + 4, (int)by + 52, 8, 10, Color.Red);
    Raylib.DrawRectangle((int)bx + 4, (int)by + 68, 8, 10, Color.Red);
    Raylib.DrawRectangle((int)bx + 3, (int)by + 18, 10, 4, new Color((byte)180,(byte)180,(byte)180,(byte)255));
    Raylib.DrawRectangle((int)bx + 3, (int)by + 80, 10, 4, new Color((byte)180,(byte)180,(byte)180,(byte)255));
    // windows
    Raylib.DrawRectangle((int)bx + 20, (int)by + 20, 40, 35, new Color((byte)200,(byte)235,(byte)240,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 20, (int)by + 20, 40, 35, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangle((int)bx + 80, (int)by + 20, 40, 35, new Color((byte)200,(byte)235,(byte)240,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 80, (int)by + 20, 40, 35, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 50, (int)by + 65, 40, 45, new Color((byte)20,(byte)140,(byte)120,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 50, (int)by + 65, 40, 45, new Color((byte)10,(byte)80,(byte)70,(byte)255));
    Raylib.DrawCircle((int)bx + 86, (int)by + 88, 3, new Color((byte)220,(byte)200,(byte)100,(byte)255));
}
// Hallensteins Exterior
if (building.BuildingName == "HALLENSTEINS")
{
    // main body — jet black
    Raylib.DrawRectangle((int)bx, (int)by, 160, 110, new Color((byte)15,(byte)15,(byte)15,(byte)255));
    // gold roof band
    Raylib.DrawRectangle((int)bx - 4, (int)by - 8, 168, 10, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx - 4, (int)by - 8, 168, 3, new Color((byte)220,(byte)180,(byte)40,(byte)255));
    // large display windows
    Raylib.DrawRectangle((int)bx + 8,  (int)by + 14, 50, 50, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 8,  (int)by + 14, 50, 50, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx + 100, (int)by + 14, 50, 50, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 100, (int)by + 14, 50, 50, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    // mannequins in windows
    Raylib.DrawRectangle((int)bx + 26, (int)by + 18, 6, 18, new Color((byte)200,(byte)180,(byte)140,(byte)255)); // body
    Raylib.DrawCircle((int)bx + 29, (int)by + 16, 5, new Color((byte)200,(byte)180,(byte)140,(byte)255));        // head
    Raylib.DrawRectangle((int)bx + 118, (int)by + 18, 6, 18, new Color((byte)200,(byte)180,(byte)140,(byte)255));
    Raylib.DrawCircle((int)bx + 121, (int)by + 16, 5, new Color((byte)200,(byte)180,(byte)140,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 62, (int)by + 66, 36, 44, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 62, (int)by + 66, 36, 44, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Raylib.DrawCircle((int)bx + 93, (int)by + 89, 3, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    // sign
    Raylib.DrawRectangle((int)bx + 10, (int)by - 22, 140, 16, new Color((byte)10,(byte)10,(byte)10,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by - 22, 140, 16, new Color((byte)180,(byte)140,(byte)20,(byte)255));
    Program.DrawTextUI("HALLENSTEINS", (int)bx + 14, (int)by - 20, 14, new Color((byte)180,(byte)140,(byte)20,(byte)255));
}

//AA Exterior
if (building.BuildingName == "AA")
{
    Color aaBlue = new Color((byte)0,(byte)80,(byte)160,(byte)255);
    Raylib.DrawRectangle((int)bx, (int)by, 160, 110, aaBlue);
    Raylib.DrawRectangle((int)bx - 4, (int)by - 10, 168, 12, new Color((byte)0,(byte)60,(byte)130,(byte)255));
    Raylib.DrawRectangle((int)bx - 4, (int)by - 10, 168, 3, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    // large windows
    Raylib.DrawRectangle((int)bx + 8,  (int)by + 14, 55, 45, new Color((byte)160,(byte)200,(byte)240,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 8, (int)by + 14, 55, 45, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    Raylib.DrawRectangle((int)bx + 97, (int)by + 14, 55, 45, new Color((byte)160,(byte)200,(byte)240,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 97, (int)by + 14, 55, 45, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 60, (int)by + 65, 40, 45, new Color((byte)0,(byte)50,(byte)110,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 60, (int)by + 65, 40, 45, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    Raylib.DrawCircle((int)bx + 95, (int)by + 88, 3, new Color((byte)200,(byte)220,(byte)255,(byte)255));
    // AA sign
    Raylib.DrawRectangle((int)bx + 30, (int)by - 26, 100, 18, new Color((byte)0,(byte)40,(byte)90,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 30, (int)by - 26, 100, 18, new Color((byte)0,(byte)120,(byte)220,(byte)255));
    Program.DrawTextUI("AA", (int)bx + 70, (int)by - 24, 14, new Color((byte)200,(byte)220,(byte)255,(byte)255));
    // licence plate decorations
    for (int lp = 0; lp < 5; lp++)
        Raylib.DrawRectangle((int)bx + 8 + lp * 30, (int)by + 68, 20, 10, new Color((lp % 2 == 0) ? (byte)255 : (byte)200, (byte)180, (byte)0, (byte)255));
}
//Airport Exterior
if (building.BuildingName == "Airport")
{
    // main terminal
    Raylib.DrawRectangle((int)bx, (int)by, 400, 200, new Color((byte)220,(byte)220,(byte)225,(byte)255));
    // glass facade
    Raylib.DrawRectangle((int)bx, (int)by, 400, 60, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines((int)bx, (int)by, 400, 200, new Color((byte)140,(byte)150,(byte)160,(byte)255));
    // roof overhang
    Raylib.DrawRectangle((int)bx - 10, (int)by - 14, 420, 16, new Color((byte)80,(byte)90,(byte)100,(byte)255));
    Raylib.DrawRectangle((int)bx - 10, (int)by - 14, 420, 4, new Color((byte)100,(byte)115,(byte)130,(byte)255));
    // control tower
    Raylib.DrawRectangle((int)bx + 340, (int)by - 80, 30, 80, new Color((byte)180,(byte)185,(byte)190,(byte)255));
    Raylib.DrawRectangle((int)bx + 330, (int)by - 90, 50, 14, new Color((byte)100,(byte)180,(byte)220,(byte)255)); // tower cab
    Raylib.DrawRectangleLines((int)bx + 330, (int)by - 90, 50, 14, new Color((byte)60,(byte)120,(byte)160,(byte)255));
    Raylib.DrawCircle((int)bx + 355, (int)by - 96, 4, new Color((byte)220,(byte)40,(byte)40,(byte)255)); // beacon
    // windows row
    for (int w = 0; w < 5; w++)
        Raylib.DrawRectangle((int)bx + 20 + w * 72, (int)by + 10, 50, 40, new Color((byte)120,(byte)180,(byte)210,(byte)180));
    // doors
    Raylib.DrawRectangle((int)bx + 170, (int)by + 150, 60, 50, new Color((byte)100,(byte)160,(byte)190,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 170, (int)by + 150, 60, 50, new Color((byte)60,(byte)100,(byte)140,(byte)255));
    // runway strip hint
    Raylib.DrawRectangle((int)bx - 200, (int)by + 220, 800, 30, new Color((byte)60,(byte)60,(byte)65,(byte)255));
    for (int m = 0; m < 8; m++)
        Raylib.DrawRectangle((int)bx - 180 + m * 100, (int)by + 232, 50, 6, Color.White);
    // sign
    Raylib.DrawRectangle((int)bx + 120, (int)by - 22, 160, 14, new Color((byte)20,(byte)30,(byte)40,(byte)255));
    Program.DrawTextUI("AIRPORT", (int)bx + 134, (int)by - 21, 14, new Color((byte)200,(byte)220,(byte)255,(byte)255));
}

if (building.BuildingName == "Casino")
{
    Raylib.DrawRectangle((int)bx, (int)by, 220, 140, new Color((byte)60,(byte)5,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)bx - 5, (int)by - 10, 230, 12, new Color((byte)120,(byte)10,(byte)120,(byte)255));
    // neon sign
    Raylib.DrawRectangle((int)bx + 20, (int)by - 28, 180, 20, new Color((byte)10,(byte)5,(byte)10,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 20, (int)by - 28, 180, 20, new Color((byte)220,(byte)20,(byte)220,(byte)255));
    Program.DrawTextUI("CASINO", (int)bx + 58, (int)by - 25, 16, new Color((byte)255,(byte)50,(byte)255,(byte)255));
    // windows — purple glow
    Raylib.DrawRectangle((int)bx + 10, (int)by + 14, 60, 50, new Color((byte)100,(byte)10,(byte)100,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 14, 60, 50, new Color((byte)180,(byte)20,(byte)180,(byte)255));
    Raylib.DrawRectangle((int)bx + 150, (int)by + 14, 60, 50, new Color((byte)100,(byte)10,(byte)100,(byte)200));
    Raylib.DrawRectangleLines((int)bx + 150, (int)by + 14, 60, 50, new Color((byte)180,(byte)20,(byte)180,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 90, (int)by + 80, 40, 60, new Color((byte)80,(byte)5,(byte)80,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 90, (int)by + 80, 40, 60, new Color((byte)200,(byte)20,(byte)200,(byte)255));
    // gold trim
    Raylib.DrawRectangle((int)bx, (int)by + 2, 220, 3, new Color((byte)200,(byte)160,(byte)20,(byte)255));
    Raylib.DrawRectangle((int)bx, (int)by + 137, 220, 3, new Color((byte)200,(byte)160,(byte)20,(byte)255));
}

    // Mini golf exterior decoration — find the MiniGolf building and add flourishes
foreach (var b in buildings)
{
    if (b.BuildingName != "MiniGolf") continue;
    int golfX = (int)b.Bounds.X;
    int golfY = (int)b.Bounds.Y;
    int golfW = (int)b.Bounds.Width;
    int golfH = (int)b.Bounds.Height;

    // big sign
    Raylib.DrawRectangle(golfX + golfW / 2 - 70, golfY - 40, 140, 30, new Color((byte)255, (byte)220, (byte)80, (byte)255));
    Raylib.DrawRectangleLines(golfX + golfW / 2 - 70, golfY - 40, 140, 30, new Color((byte)180, (byte)140, (byte)20, (byte)255));
    Program.DrawTextUI("MINI GOLF", golfX + golfW / 2 - 56, golfY - 34, 18, new Color((byte)40, (byte)90, (byte)40, (byte)255));

    // decorative putting green strip out front
    Raylib.DrawRectangle(golfX - 60, golfY + golfH + 10, golfW + 120, 70, new Color((byte)60, (byte)150, (byte)70, (byte)255));
    Raylib.DrawCircle(golfX + 20, golfY + golfH + 45, 7, Color.Black);
    Raylib.DrawLine(golfX + 20, golfY + golfH + 45, golfX + 20, golfY + golfH + 20, Color.White);
    Raylib.DrawTriangle(
        new Vector2(golfX + 20, golfY + golfH + 20),
        new Vector2(golfX + 20, golfY + golfH + 30),
        new Vector2(golfX + 36, golfY + golfH + 25),
        Color.Red);
    Raylib.DrawCircle(golfX + golfW + 40, golfY + golfH + 45, 4, Color.White);

    // windmill-style obstacle decoration
    Raylib.DrawRectangle(golfX + golfW / 2 - 4, golfY + golfH + 25, 8, 40, new Color((byte)120, (byte)75, (byte)35, (byte)255));
    Raylib.DrawCircle(golfX + golfW / 2, golfY + golfH + 25, 4, Color.White);
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
        Raylib.DrawRectangle((int)bx + 40, (int)by - 8, 180, 14, new Color((byte)180,(byte)60,(byte)30,(byte)200));
        Program.DrawTextUI("GAS STATION", (int)bx + 44, (int)by - 7, 14, new Color((byte)255,(byte)240,(byte)180,(byte)255));
        // steps
        Raylib.DrawRectangle((int)bx + 90, (int)by + 155, 80, 10, new Color((byte)200,(byte)195,(byte)160,(byte)255));
    }

    // ── PLAYER HOUSE EXTERIOR ─────────────────────────────────────────────────
    if (building.BuildingName.StartsWith("PLAYER HOUSE"))
    {
        // extract plot index to vary the house style slightly per plot
        int plotIdx = 0;
        if (int.TryParse(building.BuildingName.Replace("PLAYER HOUSE ", ""), out int pi))
            plotIdx = pi;

        // get wall colour from house data
        Color wallCol = plotIdx < houseDataList.Count
            ? building.ExteriorColor
            : new Color((byte)210,(byte)190,(byte)150,(byte)255);

        // ── foundation ──
        Raylib.DrawRectangle((int)bx - 4, (int)by + 172, 248, 12,
            new Color((byte)160,(byte)150,(byte)130,(byte)255));

        // ── main body ──
        Raylib.DrawRectangle((int)bx, (int)by, 240, 178, wallCol);
        // shadow on right and bottom edges
        Raylib.DrawRectangle((int)bx + 236, (int)by + 4,   4, 174,
            new Color((byte)0,(byte)0,(byte)0,(byte)40));
        Raylib.DrawRectangle((int)bx + 4,   (int)by + 174, 236, 4,
            new Color((byte)0,(byte)0,(byte)0,(byte)40));

        // ── roof ──
        // main roof band
        Raylib.DrawRectangle((int)bx - 8,  (int)by - 18, 256, 22,
            new Color((byte)80,(byte)55,(byte)30,(byte)255));
        // roof highlight
        Raylib.DrawRectangle((int)bx - 8,  (int)by - 18, 256, 5,
            new Color((byte)110,(byte)80,(byte)45,(byte)255));
        // roof shadow line
        Raylib.DrawRectangle((int)bx - 8,  (int)by + 2,  256, 3,
            new Color((byte)50,(byte)32,(byte)14,(byte)255));
        // roof tiles (horizontal lines)
        for (int t = 0; t < 3; t++)
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14 + t * 5, 256, 2,
                new Color((byte)60,(byte)40,(byte)18,(byte)120));
        // chimney
        Raylib.DrawRectangle((int)bx + 180, (int)by - 42, 22, 28,
            new Color((byte)140,(byte)100,(byte)70,(byte)255));
        Raylib.DrawRectangle((int)bx + 177, (int)by - 44, 28, 6,
            new Color((byte)120,(byte)85,(byte)55,(byte)255));
        // chimney smoke puffs
        float smokeT = (float)Raylib.GetTime();
        Raylib.DrawCircle((int)bx + 191, (int)by - 50 + (int)(MathF.Sin(smokeT * 1.2f) * 3f), 5,
            new Color((byte)180,(byte)180,(byte)180,(byte)120));
        Raylib.DrawCircle((int)bx + 188, (int)by - 60 + (int)(MathF.Sin(smokeT * 0.9f) * 4f), 7,
            new Color((byte)160,(byte)160,(byte)160,(byte)80));
        Raylib.DrawCircle((int)bx + 193, (int)by - 70 + (int)(MathF.Sin(smokeT * 1.5f) * 3f), 4,
            new Color((byte)140,(byte)140,(byte)140,(byte)50));

        // ── left window ──
        Raylib.DrawRectangle((int)bx + 14, (int)by + 18, 60, 55,
            new Color((byte)160,(byte)200,(byte)220,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 14, (int)by + 18, 60, 55,
            new Color((byte)80,(byte)55,(byte)30,(byte)255));
        // window cross
        Raylib.DrawRectangle((int)bx + 43, (int)by + 18, 3, 55,
            new Color((byte)80,(byte)55,(byte)30,(byte)200));
        Raylib.DrawRectangle((int)bx + 14, (int)by + 44, 60, 3,
            new Color((byte)80,(byte)55,(byte)30,(byte)200));
        // window sill
        Raylib.DrawRectangle((int)bx + 10, (int)by + 71, 68, 5,
            new Color((byte)200,(byte)185,(byte)155,(byte)255));
        // curtains hint
        Raylib.DrawRectangle((int)bx + 15, (int)by + 19, 10, 53,
            new Color((byte)220,(byte)180,(byte)140,(byte)60));
        Raylib.DrawRectangle((int)bx + 63, (int)by + 19, 10, 53,
            new Color((byte)220,(byte)180,(byte)140,(byte)60));

        // ── right window ──
        Raylib.DrawRectangle((int)bx + 166, (int)by + 18, 60, 55,
            new Color((byte)160,(byte)200,(byte)220,(byte)180));
        Raylib.DrawRectangleLines((int)bx + 166, (int)by + 18, 60, 55,
            new Color((byte)80,(byte)55,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx + 195, (int)by + 18, 3, 55,
            new Color((byte)80,(byte)55,(byte)30,(byte)200));
        Raylib.DrawRectangle((int)bx + 166, (int)by + 44, 60, 3,
            new Color((byte)80,(byte)55,(byte)30,(byte)200));
        Raylib.DrawRectangle((int)bx + 162, (int)by + 71, 68, 5,
            new Color((byte)200,(byte)185,(byte)155,(byte)255));
        Raylib.DrawRectangle((int)bx + 167, (int)by + 19, 10, 53,
            new Color((byte)220,(byte)180,(byte)140,(byte)60));
        Raylib.DrawRectangle((int)bx + 215, (int)by + 19, 10, 53,
            new Color((byte)220,(byte)180,(byte)140,(byte)60));

        // ── door ──
        Raylib.DrawRectangle((int)bx + 96,  (int)by + 98, 48, 80,
            new Color((byte)90,(byte)55,(byte)22,(byte)255));
        Raylib.DrawRectangleLines((int)bx + 96, (int)by + 98, 48, 80,
            new Color((byte)60,(byte)36,(byte)12,(byte)255));
        // door panels
        Raylib.DrawRectangle((int)bx + 100, (int)by + 102, 18, 28,
            new Color((byte)106,(byte)66,(byte)28,(byte)255));
        Raylib.DrawRectangle((int)bx + 122, (int)by + 102, 18, 28,
            new Color((byte)106,(byte)66,(byte)28,(byte)255));
        Raylib.DrawRectangle((int)bx + 100, (int)by + 136, 40, 36,
            new Color((byte)106,(byte)66,(byte)28,(byte)255));
        // door handle
        Raylib.DrawCircle((int)bx + 136, (int)by + 138, 4,
            new Color((byte)200,(byte)165,(byte)60,(byte)255));
        // door knocker
        Raylib.DrawCircle((int)bx + 120, (int)by + 112, 3,
            new Color((byte)190,(byte)155,(byte)50,(byte)255));
        // door step
        Raylib.DrawRectangle((int)bx + 86,  (int)by + 174, 68, 8,
            new Color((byte)180,(byte)165,(byte)135,(byte)255));
        // door arch
        Raylib.DrawRectangle((int)bx + 93,  (int)by + 90,  54, 10,
            new Color((byte)80,(byte)55,(byte)30,(byte)255));

        // ── wall details ──
        // brick texture lines (horizontal)
        for (int br = 0; br < 6; br++)
            Raylib.DrawRectangle((int)bx, (int)by + 24 + br * 26, 240, 1,
                new Color((byte)0,(byte)0,(byte)0,(byte)18));
        // vertical brick offsets alternating
        for (int br = 0; br < 6; br++)
        {
            int offset = br % 2 == 0 ? 40 : 20;
            for (int bc = 0; bc < 5; bc++)
                Raylib.DrawRectangle((int)bx + offset + bc * 48, (int)by + 24 + br * 26, 1, 26,
                    new Color((byte)0,(byte)0,(byte)0,(byte)14));
        }

        // ── hanging sign ──
        Raylib.DrawRectangle((int)bx + 60,  (int)by - 36, 120, 20,
            new Color((byte)60,(byte)36,(byte)12,(byte)240));
        Raylib.DrawRectangleLines((int)bx + 60, (int)by - 36, 120, 20,
            new Color((byte)160,(byte)120,(byte)50,(byte)255));
        // sign chains
        Raylib.DrawLine((int)bx + 70,  (int)by - 36, (int)bx + 70,  (int)by - 18,
            new Color((byte)140,(byte)105,(byte)40,(byte)200));
        Raylib.DrawLine((int)bx + 170, (int)by - 36, (int)bx + 170, (int)by - 18,
            new Color((byte)140,(byte)105,(byte)40,(byte)200));
        string houseLabel = plotIdx == 0 ? "MY HOUSE" : $"HOUSE {plotIdx + 1}";
        int labelW = Program.MeasureTextUI(houseLabel, 13);
        Program.DrawTextUI(houseLabel, (int)bx + 120 - labelW / 2, (int)by - 33, 13,
            new Color((byte)220,(byte)185,(byte)100,(byte)255));

        // ── garden — small flower dots either side of door ──
        for (int fl = 0; fl < 3; fl++)
        {
            Raylib.DrawCircle((int)bx + 20 + fl * 14, (int)by + 170, 4,
                new Color((byte)60,(byte)140,(byte)60,(byte)255));
            Raylib.DrawCircle((int)bx + 20 + fl * 14, (int)by + 168, 3,
                new Color((byte)220,(byte)80,(byte)80,(byte)255));
            Raylib.DrawCircle((int)bx + 196 + fl * 14, (int)by + 170, 4,
                new Color((byte)60,(byte)140,(byte)60,(byte)255));
            Raylib.DrawCircle((int)bx + 196 + fl * 14, (int)by + 168, 3,
                new Color((byte)255,(byte)180,(byte)60,(byte)255));
        }

        // ── mailbox ──
        Raylib.DrawRectangle((int)bx + 4,  (int)by + 140, 6,  32,
            new Color((byte)80,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangle((int)bx,      (int)by + 130, 14, 14,
            new Color((byte)60,(byte)90,(byte)150,(byte)255));
        Raylib.DrawRectangleLines((int)bx, (int)by + 130, 14, 14,
            new Color((byte)40,(byte)65,(byte)115,(byte)255));
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
    Program.DrawTextUI("BIKE DEALER", (int)bx + 62, (int)by - 9, 12, new Color((byte)160, (byte)210, (byte)255, (byte)255));
 
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
    Raylib.DrawRectangle((int)bx + 100, (int)by - 14, 220, 14, new Color((byte)10, (byte)10, (byte)14, (byte)230));
    Program.DrawTextUI("CAR DEALER", (int)bx + 126, (int)by - 13, 14, new Color((byte)100, (byte)170, (byte)255, (byte)255));
 
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
    Program.DrawTextUI("BARN DEALER", (int)bx + 88, (int)by + 120, 14, new Color((byte)220, (byte)185, (byte)120, (byte)255));
 
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
        Raylib.DrawRectangle((int)bx + 55, (int)by - 8, 50, 14, new Color((byte)80,(byte)30,(byte)30,(byte)255));
        Program.DrawTextUI("WEAPONS", (int)bx + 18, (int)by - 7, 14, new Color((byte)180,(byte)50,(byte)50,(byte)255));
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
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 14, new Color((byte)200,(byte)30,(byte)30,(byte)200));
        Program.DrawTextUI("HOSPITAL", (int)bx + 26, (int)by - 7, 14, Color.White);
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
        Raylib.DrawRectangle((int)bx + 10, (int)by - 8, 140, 14, new Color((byte)50,(byte)120,(byte)50,(byte)220));
        Program.DrawTextUI("SUPERMARKET", (int)bx + 12, (int)by - 7, 14, Color.White);
        // trolley bay outside
        Raylib.DrawRectangle((int)bx - 50, (int)by + 50, 45, 70, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawRectangleLines((int)bx - 50, (int)by + 50, 45, 70, new Color((byte)120,(byte)120,(byte)130,(byte)255));
        Program.DrawTextUI("Bay", (int)bx - 44, (int)by + 80, 13, Color.DarkGray);
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
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 14, new Color((byte)20,(byte)40,(byte)100,(byte)220));
        Program.DrawTextUI("GYM", (int)bx + 52, (int)by - 7, 14, new Color((byte)180,(byte)210,(byte)255,(byte)255));
    }
 
    // ── POLICE STATION EXTERIOR ───────────────────────────────────────────────
    if (building.BuildingName == "POLICE STATION")
    {
        Raylib.DrawRectangle((int)bx, (int)by, 160, 120, new Color((byte)30,(byte)30,(byte)100,(byte)255));
        // dark blue band
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 14, new Color((byte)15,(byte)15,(byte)60,(byte)255));
        Raylib.DrawRectangle((int)bx - 5, (int)by - 12, 170, 4, new Color((byte)20,(byte)20,(byte)80,(byte)255));
        // badge/star sign
        Raylib.DrawRectangle((int)bx + 55, (int)by - 8, 50, 14, new Color((byte)15,(byte)15,(byte)60,(byte)255));
        Program.DrawTextUI("POLICE", (int)bx + 22, (int)by - 7, 14, new Color((byte)200,(byte)200,(byte)255,(byte)255));
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
        Raylib.DrawRectangle((int)bx + 20, (int)by - 8, 120, 14, new Color((byte)25,(byte)60,(byte)30,(byte)220));
        Program.DrawTextUI("STORE", (int)bx + 46, (int)by - 7, 14, new Color((byte)180,(byte)220,(byte)180,(byte)255));
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

    // ── MAILBOXES ──

    // house-plot mailboxes (original)
    for (int i = 0; i < ownedHousePlots.Count; i++)
    {
        int mx = ownedHousePlots[i].x + 40;
        int my = ownedHousePlots[i].y + 150;
        // post
        Raylib.DrawRectangle(mx + 8, my + 20, 6, 30, new Color((byte)80,(byte)60,(byte)40,(byte)255));
        // box
        Raylib.DrawRectangle(mx, my, 26, 22, new Color((byte)120,(byte)120,(byte)140,(byte)255));
        Raylib.DrawRectangleLines(mx, my, 26, 22, Color.Black);
        // red flag up if THIS mailbox holds the waiting ID
        if (idMailWaiting && idTargetHouseIndex == i)
            Raylib.DrawRectangle(mx + 24, my - 6, 4, 14, Color.Red);

        for (int li = 0; li < 5; li++)
        if (licenceMailWaiting[li] && licenceTargetHouse[li] == i)
        Raylib.DrawRectangle(mx + 24, my - 6, 4, 14, Color.Red);
    }

    // extra mailboxes (farmhouse etc.)
    foreach (var (pos, houseIndex) in extraMailboxes)
    {
        int mx = (int)pos.X;
        int my = (int)pos.Y;
        // post
        Raylib.DrawRectangle(mx + 8, my + 20, 6, 30, new Color((byte)80,(byte)60,(byte)40,(byte)255));
        // box
        Raylib.DrawRectangle(mx, my, 26, 22, new Color((byte)120,(byte)120,(byte)140,(byte)255));
        Raylib.DrawRectangleLines(mx, my, 26, 22, Color.Black);
        // red flag up if THIS mailbox holds the waiting ID
        if (idMailWaiting && idTargetHouseIndex == houseIndex)
            Raylib.DrawRectangle(mx + 24, my - 6, 4, 14, Color.Red);

        for (int li = 0; li < 5; li++)
        if (licenceMailWaiting[li] && licenceTargetHouse[li] == houseIndex)
        Raylib.DrawRectangle(mx + 24, my - 6, 4, 14, Color.Red);
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
    Program.DrawTextUI("PUMP 1", (int)p1.X - 24, (int)p1.Y + 30, 16, Color.White);
    Program.DrawTextUI("R = Fuel", (int)p1.X - 24, (int)p1.Y + 48, 14, Color.LightGray);

    // pump 2
    Vector2 p2 = station.Pump2Pos;
    Raylib.DrawRectangle((int)p2.X - 18, (int)p2.Y - 35, 36, 60,
        new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle((int)p2.X - 12, (int)p2.Y - 28, 24, 36,
        station.Pump2Active ? Color.Green : new Color((byte)200,(byte)50,(byte)50,(byte)255));
    Program.DrawTextUI("PUMP 2", (int)p2.X - 24, (int)p2.Y + 30, 16, Color.White);
    Program.DrawTextUI("R = Fuel", (int)p2.X - 24, (int)p2.Y + 48, 14, Color.LightGray);
}
foreach (GasStation gs in gasStations)
    DrawGasStation(gs, gs.OriginX, gs.OriginY);

// ── LAND FOR SALE SIGNS ───────────────────────────────────────────────────
    foreach (var plot in landPlots)
    {
        bool alreadyOwned = ownedHousePlots.Any(p => p.x == (int)plot.x && p.y == (int)plot.y);
        if (alreadyOwned) continue;

        int sx = (int)plot.x; int sy = (int)plot.y;
        // plot boundary
        Raylib.DrawRectangleLines(sx, sy, 240, 180, new Color((byte)255,(byte)200,(byte)0,(byte)200));
        Raylib.DrawRectangle(sx, sy, 240, 180, new Color((byte)255,(byte)220,(byte)80,(byte)30));
        // sign post
        Raylib.DrawRectangle(sx + 110, sy - 60, 6, 60, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        // sign board
        bool premium = plot.houseType != "Standard";
        int boardH = premium ? 58 : 44;
        Raylib.DrawRectangle(sx + 60, sy - 100, 120, boardH, new Color((byte)255,(byte)220,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx + 60, sy - 100, 120, boardH, new Color((byte)180,(byte)140,(byte)0,(byte)255));
        Program.DrawTextUI("FOR SALE", sx + 68, sy - 96, 16, new Color((byte)20,(byte)20,(byte)20,(byte)255));
        Program.DrawTextUI($"${plot.price}", sx + 76, sy - 78, 14, new Color((byte)20,(byte)20,(byte)20,(byte)255));

        if (premium)
        {
            string typeLabel = plot.houseType switch
            {
                "OceanHouse" => "OCEAN HOUSE",
                _            => plot.houseType.ToUpper()   // FARMHOUSE / MANSION
            };
            Program.DrawTextUI(typeLabel, sx + 66, sy - 60, 14, new Color((byte)140,(byte)40,(byte)20,(byte)255));
        }


        // interaction prompt when nearby
        float dist = Vector2.Distance(player.Position, new Vector2(plot.x + 120, plot.y + 90));
        if (dist < 200)
        {
            Program.DrawTextUI($"E = View {plot.label}", sx - 20, sy - 120, 22, Color.Yellow);
            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen && !landForSaleUIOpen)
            {
                selectedPlot = Array.IndexOf(landPlots, plot);
                landForSaleUIOpen = true;
            }
        }
    }



// Dungeon cave entrances
foreach (var entrance in dungeonEntrances)
{
    int ex = (int)entrance.pos.X;
    int ey = (int)entrance.pos.Y;

    // Rocky surround
    Raylib.DrawCircle(ex, ey + 8, 38, new Color((byte)70,(byte)65,(byte)55,(byte)255));
    Raylib.DrawCircle(ex - 14, ey,     20, new Color((byte)80,(byte)72,(byte)60,(byte)255));
    Raylib.DrawCircle(ex + 16, ey + 2, 18, new Color((byte)75,(byte)68,(byte)57,(byte)255));

    // Cave mouth
    Raylib.DrawEllipse(ex, ey + 6, 28, 20, new Color((byte)15,(byte)10,(byte)8,(byte)255));

    // Stalactites
    Raylib.DrawTriangle(new Vector2(ex - 14, ey - 12), new Vector2(ex - 8, ey - 12), new Vector2(ex - 11, ey - 2),  new Color((byte)60,(byte)54,(byte)44,(byte)255));
    Raylib.DrawTriangle(new Vector2(ex - 2,  ey - 14), new Vector2(ex + 4, ey - 14), new Vector2(ex + 1,  ey - 3),  new Color((byte)60,(byte)54,(byte)44,(byte)255));
    Raylib.DrawTriangle(new Vector2(ex + 9,  ey - 12), new Vector2(ex + 15,ey - 12), new Vector2(ex + 12, ey - 2),  new Color((byte)60,(byte)54,(byte)44,(byte)255));

    // Glow from inside
    Raylib.DrawEllipse(ex, ey + 8, 16, 11, new Color((byte)255,(byte)140,(byte)0,(byte)40));

    // Sign above entrance
    Raylib.DrawRectangle(ex - 36, ey - 48, 72, 20, new Color((byte)70,(byte)45,(byte)18,(byte)255));
    Raylib.DrawRectangleLines(ex - 36, ey - 48, 72, 20, new Color((byte)100,(byte)70,(byte)30,(byte)255));
    int nw = Program.MeasureTextUI(entrance.name, 11);
    Program.DrawTextUI(entrance.name, ex - nw / 2, ey - 46, 11, Color.Gold);

    // Prompt when nearby
    if (Vector2.Distance(player.Center, entrance.pos) < 80)
    {
        int pw2 = Program.MeasureTextUI("E = Enter Dungeon", 20);
        Program.DrawTextUI("E = Enter Dungeon", ex - pw2 / 2, ey - 75, 20, Color.Gold);
    }
}
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

     // Rivers
            foreach (var river in rivers)
            {
                Rectangle rb = river.Bounds;
                Raylib.DrawRectangleRec(rb, new Color((byte)40,(byte)110,(byte)180,(byte)255));

                if (river.Vertical)
                {
                    // shimmer runs down the river
                    for (int s = 0; s < rb.Height; s += 60)
                    {
                        float wobble = MathF.Sin((float)Raylib.GetTime() * 2f + s * 0.05f) * 6f;
                        Raylib.DrawRectangle((int)(rb.X + 20 + wobble), (int)(rb.Y + s), 6, 30,
                            new Color((byte)120,(byte)190,(byte)230,(byte)160));
                        Raylib.DrawRectangle((int)(rb.X + 70 - wobble), (int)(rb.Y + s + 20), 6, 30,
                            new Color((byte)120,(byte)190,(byte)230,(byte)160));
                    }
                    // banks (left/right edges)
                    Raylib.DrawRectangle((int)rb.X - 4, (int)rb.Y, 4, (int)rb.Height, new Color((byte)90,(byte)70,(byte)40,(byte)255));
                    Raylib.DrawRectangle((int)(rb.X + rb.Width), (int)rb.Y, 4, (int)rb.Height, new Color((byte)90,(byte)70,(byte)40,(byte)255));
                }
                else
                {
                    // shimmer runs across the river
                    for (int s = 0; s < rb.Width; s += 60)
                    {
                        float wobble = MathF.Sin((float)Raylib.GetTime() * 2f + s * 0.05f) * 6f;
                        Raylib.DrawRectangle((int)(rb.X + s), (int)(rb.Y + 20 + wobble), 30, 6,
                            new Color((byte)120,(byte)190,(byte)230,(byte)160));
                        Raylib.DrawRectangle((int)(rb.X + s + 20), (int)(rb.Y + 70 - wobble), 30, 6,
                            new Color((byte)120,(byte)190,(byte)230,(byte)160));
                    }
                    // banks (top/bottom edges)
                    Raylib.DrawRectangle((int)rb.X, (int)rb.Y - 4, (int)rb.Width, 4, new Color((byte)90,(byte)70,(byte)40,(byte)255));
                    Raylib.DrawRectangle((int)rb.X, (int)(rb.Y + rb.Height), (int)rb.Width, 4, new Color((byte)90,(byte)70,(byte)40,(byte)255));
                }
            }

// Livestock
foreach (var pen in livestockPens)
{
    int px = (int)pen.Position.X, py = (int)pen.Position.Y;
    // fenced pen
    Raylib.DrawRectangle(px - 60, py - 50, 120, 100, new Color((byte)180,(byte)160,(byte)110,(byte)255));
    Raylib.DrawRectangleLines(px - 60, py - 50, 120, 100, new Color((byte)110,(byte)80,(byte)45,(byte)255));
    // animal blob (colour per type)
    Color ac = pen.Animal switch {
        "Chicken" => new Color((byte)240,(byte)230,(byte)210,(byte)255),
        "Cow"     => new Color((byte)60,(byte)50,(byte)45,(byte)255),
        "Sheep"   => new Color((byte)235,(byte)235,(byte)235,(byte)255),
        "Pig"     => new Color((byte)230,(byte)150,(byte)160,(byte)255),
        _          => new Color((byte)200,(byte)190,(byte)170,(byte)255),
    };
    Raylib.DrawCircle(px, py, 20, ac);
    Raylib.DrawCircle(px + 14, py - 8, 8, ac);
    Program.DrawTextUI(pen.Animal, px - 28, py - 74, 16, new Color((byte)80,(byte)55,(byte)30,(byte)255));

    if (Vector2.Distance(player.Center, pen.Position) < 70)
    {
        if (pen.ReadyToHarvest) Program.DrawTextUI($"SPACE = Collect {pen.Produce}", px - 70, py + 56, 16, Color.Gold);
        else if (pen.Fed)       Program.DrawTextUI($"Producing... {(int)(pen.Cycle - pen.Timer)}s", px - 60, py + 56, 16, Color.White);
        else                    Program.DrawTextUI($"F = Feed ({pen.Feed})", px - 60, py + 56, 16, Color.LightGray);
    }
}

// Farm plots           
foreach (var plot in farmPlots)
{
    int px = (int)plot.Position.X, py = (int)plot.Position.Y;

    if (!plot.Tilled)
    {
        // untouched grass patch
        Raylib.DrawRectangle(px - 24, py - 16, 48, 32, new Color((byte)70,(byte)140,(byte)60,(byte)255));
        continue;
    }

    // tilled soil — furrow rows
    Raylib.DrawRectangle(px - 24, py - 16, 48, 32, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    for (int r = 0; r < 3; r++)
        Raylib.DrawRectangle(px - 22, py - 12 + r * 10, 44, 4, new Color((byte)70,(byte)45,(byte)20,(byte)255));

    if (plot.Watered && !plot.ReadyToHarvest)
        Raylib.DrawRectangle(px - 24, py - 16, 48, 32, new Color((byte)40,(byte)60,(byte)120,(byte)70)); // damp overlay

    if (plot.Planted && !plot.ReadyToHarvest)
    {
        // young sprout — height grows with progress
        float growPct = Math.Clamp(plot.GrowTimer / plot.GrowDuration, 0f, 1f);
        int stalkH = (int)(6 + growPct * 18);
        Raylib.DrawRectangle(px - 2, py - stalkH, 4, stalkH, new Color((byte)90,(byte)160,(byte)50,(byte)255));
        if (growPct > 0.4f)
        {
            Raylib.DrawTriangle(new Vector2(px, py - stalkH), new Vector2(px - 6, py - stalkH + 8), new Vector2(px, py - stalkH + 4), new Color((byte)110,(byte)180,(byte)60,(byte)255));
            Raylib.DrawTriangle(new Vector2(px, py - stalkH), new Vector2(px + 6, py - stalkH + 8), new Vector2(px, py - stalkH + 4), new Color((byte)110,(byte)180,(byte)60,(byte)255));
        }
    }

    if (plot.ReadyToHarvest)
{
    switch (plot.CropType)
    {
        case "Wheat":
            for (int s = -1; s <= 1; s++)
            {
                int sx = px + s * 8;
                Raylib.DrawRectangle(sx - 2, py - 24, 4, 24, new Color((byte)200,(byte)170,(byte)60,(byte)255));
                Raylib.DrawEllipse(sx, py - 26, 6, 10, new Color((byte)230,(byte)200,(byte)80,(byte)255));
            }
            break;

        case "Carrot":
            // orange tops poking through soil, leafy green tufts
            for (int s = -1; s <= 1; s++)
            {
                int sx = px + s * 8;
                Raylib.DrawTriangle(
                    new Vector2(sx, py - 6), new Vector2(sx - 5, py + 6), new Vector2(sx + 5, py + 6),
                    new Color((byte)230,(byte)120,(byte)30,(byte)255)); // visible root top
                Raylib.DrawLineEx(new Vector2(sx, py - 6), new Vector2(sx - 4, py - 20), 2, new Color((byte)70,(byte)160,(byte)60,(byte)255));
                Raylib.DrawLineEx(new Vector2(sx, py - 6), new Vector2(sx, py - 24), 2, new Color((byte)80,(byte)175,(byte)65,(byte)255));
                Raylib.DrawLineEx(new Vector2(sx, py - 6), new Vector2(sx + 4, py - 20), 2, new Color((byte)70,(byte)160,(byte)60,(byte)255));
            }
            break;

        case "Potato":
            // low leafy bush with small flowers, tubers stay underground
            Raylib.DrawEllipse(px, py - 10, 20, 12, new Color((byte)60,(byte)140,(byte)55,(byte)255));
            Raylib.DrawEllipse(px - 10, py - 6, 12, 8, new Color((byte)70,(byte)150,(byte)60,(byte)255));
            Raylib.DrawEllipse(px + 10, py - 6, 12, 8, new Color((byte)70,(byte)150,(byte)60,(byte)255));
            Raylib.DrawCircle(px - 6, py - 14, 3, new Color((byte)230,(byte)230,(byte)250,(byte)255)); // flower
            Raylib.DrawCircle(px + 7, py - 12, 3, new Color((byte)230,(byte)230,(byte)250,(byte)255));
            break;

        case "Tomato":
            // taller stem with hanging red fruit clusters
            Raylib.DrawRectangle(px - 2, py - 30, 4, 30, new Color((byte)70,(byte)130,(byte)55,(byte)255));
            Raylib.DrawEllipse(px - 8, py - 22, 8, 6, new Color((byte)80,(byte)150,(byte)60,(byte)255));
            Raylib.DrawEllipse(px + 8, py - 14, 8, 6, new Color((byte)80,(byte)150,(byte)60,(byte)255));
            Raylib.DrawCircle(px - 7, py - 18, 5, new Color((byte)220,(byte)50,(byte)40,(byte)255));
            Raylib.DrawCircle(px + 6, py - 10, 5, new Color((byte)220,(byte)50,(byte)40,(byte)255));
            Raylib.DrawCircle(px, py - 6, 4, new Color((byte)220,(byte)50,(byte)40,(byte)255));
            break;
    }

}
}

// fruit tree world draw — CHANGED: more detailed plot states (bare, tilled, growing, mature)
foreach (var tree in fruitTrees)
{
    int tx = (int)tree.Position.X, ty = (int)tree.Position.Y;

    if (!tree.Tilled)
    {
        Raylib.DrawEllipse(tx, ty + 14, 26, 12, new Color((byte)70,(byte)140,(byte)60,(byte)255)); // grass patch
        Program.DrawTextUI("Till with Spade", tx - 44, ty + 30, 11, Color.LightGray);
        continue;
    }

    // tilled mound — raised soil ring, distinct from flat farm plots
    Raylib.DrawEllipse(tx, ty + 16, 28, 13, new Color((byte)95,(byte)62,(byte)32,(byte)255));
    Raylib.DrawEllipse(tx, ty + 14, 24, 10, new Color((byte)115,(byte)78,(byte)42,(byte)255));
    for (int r = -18; r <= 18; r += 9)
        Raylib.DrawLine(tx + r, ty + 9, tx + r, ty + 19, new Color((byte)80,(byte)52,(byte)26,(byte)255));

    if (!tree.Planted)
    {
        Program.DrawTextUI("Plant a tree seed", tx - 50, ty + 32, 11, Color.LightGray);
        continue;
    }

    // trunk
    Raylib.DrawRectangle(tx - 5, ty - 10, 10, 26, new Color((byte)110,(byte)75,(byte)40,(byte)255));
    Raylib.DrawRectangle(tx - 5, ty - 10, 3, 26, new Color((byte)90,(byte)58,(byte)28,(byte)255)); // shading

    float growPct = Math.Clamp(tree.GrowTimer / tree.GrowDuration, 0f, 1f);
    int canopyR = (int)(12 + growPct * 24);
    Raylib.DrawCircle(tx, ty - 16 - canopyR / 2, canopyR, new Color((byte)55,(byte)135,(byte)50,(byte)255));
    Raylib.DrawCircle(tx - canopyR/2, ty - 8 - canopyR/2, canopyR - 6, new Color((byte)65,(byte)150,(byte)55,(byte)255));
    Raylib.DrawCircle(tx + canopyR/2, ty - 8 - canopyR/2, canopyR - 6, new Color((byte)65,(byte)150,(byte)55,(byte)255));
    Raylib.DrawCircleLines(tx, ty - 16 - canopyR / 2, canopyR, new Color((byte)40,(byte)100,(byte)38,(byte)255)); // outline for depth

    if (tree.ReadyToHarvest && tree.RegrowTimer <= 0f)
    {
        Color fruitCol = tree.FruitType == "Apple"
            ? new Color((byte)210,(byte)40,(byte)40,(byte)255)
            : new Color((byte)235,(byte)210,(byte)60,(byte)255);

        if (tree.FruitType == "Apple")
        {
            Raylib.DrawCircle(tx - canopyR/2, ty - 16 - canopyR/2, 5, fruitCol);
            Raylib.DrawCircle(tx + canopyR/3, ty - 4 - canopyR/2, 5, fruitCol);
            Raylib.DrawCircle(tx, ty - 22 - canopyR/2, 5, fruitCol);
        }
        else
        {
            Raylib.DrawEllipse(tx - canopyR/3, ty - 2 - canopyR/2, 4, 9, fruitCol);
            Raylib.DrawEllipse(tx, ty + 2 - canopyR/2, 4, 9, fruitCol);
            Raylib.DrawEllipse(tx + canopyR/3, ty - 2 - canopyR/2, 4, 9, fruitCol);
        }
        Program.DrawTextUI("SPACE = Pick  |  Axe = Chop down", tx - 78, ty + 32, 11, Color.Gold);
    }
    else if (!tree.ReadyToHarvest)
    {
        Program.DrawTextUI($"{tree.FruitType} tree growing...", tx - 55, ty + 32, 11, Color.LightGray);
    }
}


            foreach (NPC npc in npcs)
            {
                if (npc.Hidden) continue;
                npc.DrawSprite(AssetManager.Get(npc.SpriteKey));   
                if (Vector2.Distance(player.Center, npc.Position) < 150)
                    DrawSpeechBubble(npc.Position, npc.Dialogue,
                        new Color((byte)80,(byte)120,(byte)80,(byte)255));
            }

            foreach (var f in friendNPCs)
            {
                f.Npc.DrawSprite(AssetManager.Get(f.Npc.SpriteKey));
                // friendship hearts above head
                int hearts = f.Friendship / 20;   // 0–5
                for (int h = 0; h < 5; h++)
                {
                    Color hc = h < hearts ? new Color((byte)230,(byte)60,(byte)90,(byte)255)
                                        : new Color((byte)80,(byte)80,(byte)90,(byte)180);
                    Raylib.DrawCircle((int)f.Npc.Position.X - 16 + h * 12, (int)f.Npc.Position.Y - 34, 4, hc);
                }
                if (Vector2.Distance(player.Center, f.Npc.Position) < 60)
                    DrawSpeechBubble(f.Npc.Position, $"E = Talk  |  G = Gift ({f.FavoriteGift})", Color.Gold);
            }

rangerNpc.DrawSprite(AssetManager.Get(rangerNpc.SpriteKey));
foreach (var q in storyQuests)
{
    if (q.Completed) continue;
    if (!q.Started && q.GiverName == "Ranger")
        Program.DrawTextUI("!", (int)rangerNpc.Position.X + 14, (int)rangerNpc.Position.Y - 46, 30, Color.Gold);
    if (q.Started && q.Current?.Progress == null && q.GiverName == "Ranger")
        Program.DrawTextUI("?", (int)rangerNpc.Position.X + 14, (int)rangerNpc.Position.Y - 46, 30, Color.SkyBlue);
    if (!q.Started && q.TriggerSpot != Vector2.Zero)
        Raylib.DrawCircleLines((int)q.TriggerSpot.X, (int)q.TriggerSpot.Y,
            30 + MathF.Sin((float)Raylib.GetTime() * 3f) * 6f, Color.Gold);
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

            foreach (var w in placedWorkbenches)
{
    int x = (int)w.X, y = (int)w.Y;
    Raylib.DrawRectangle(x - 30, y - 12, 60, 20, new Color((byte)140,(byte)100,(byte)60,(byte)255)); // top
    Raylib.DrawRectangle(x - 26, y + 8,  8, 16, new Color((byte)100,(byte)70,(byte)40,(byte)255));   // legs
    Raylib.DrawRectangle(x + 18, y + 8,  8, 16, new Color((byte)100,(byte)70,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(x - 30, y - 12, 60, 20, Color.Black);
}
foreach (var c in placedChests.Where(c => c.BuildingContext == ""))
{
    int x = (int)c.Position.X, y = (int)c.Position.Y;
    Raylib.DrawRectangle(x - 22, y - 14, 44, 28, new Color((byte)150,(byte)100,(byte)50,(byte)255));
    Raylib.DrawRectangle(x - 22, y - 14, 44, 8,  new Color((byte)110,(byte)70,(byte)35,(byte)255));  // lid
    Raylib.DrawRectangle(x - 3,  y - 4,  6, 8,   Color.Gold);                                        // latch
    Raylib.DrawRectangleLines(x - 22, y - 14, 44, 28, Color.Black);
}

// proximity prompt (draw in screen space, after EndMode2D)
bool nearPlaceable = placedWorkbenches.Any(w => Vector2.Distance(player.Center, w) < 80)
    || placedChests.Any(c => c.BuildingContext == "" && Vector2.Distance(player.Center, c.Position) < 80);
if (nearPlaceable && !chestOpen)
    Program.DrawTextUI("Space = Open/Use  |  E = Pick up", 20, ScreenHeight - 110, 20, Color.LightGray);

            foreach (LootDrop drop in lootDrops)
            {
                drop.Draw();
            }

            // ── dropped boss eggs ──
            for (int i = droppedEggs.Count - 1; i >= 0; i--)
            {
                var (epos, egg, age) = droppedEggs[i];
                age += Raylib.GetFrameTime();
                droppedEggs[i] = (epos, egg, age);

                // spawn animation: pop up then settle with a damped bounce
                float yOff = 0f, scale = 1f;
                if (age < 0.6f)
                {
                    float t = age / 0.6f;                          // 0→1
                    yOff  = -MathF.Abs(MathF.Sin(t * MathF.PI * 2f)) * 22f * (1f - t); // bounce
                    scale = 0.4f + 0.6f * Math.Min(1f, t * 2f);    // grow in
                }
                int drawY = (int)(epos.Y + yOff);
                int sz = (int)(28 * scale);

                DrawEggIcon(egg, (int)epos.X, drawY, sz, true, age);

                if (Vector2.Distance(player.Center, epos) < 60)
                {
                    Program.DrawTextUI($"[E] Pick up {egg}", (int)epos.X - 70, (int)epos.Y - 34, 14, Color.White);
                    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                    {
                        if (AddOneItemToToolbar(egg))
                        {
                            ShowNotification($"Picked up {egg}!");
                            droppedEggs.RemoveAt(i);
                        }
                        else
                        {
                            ShowNotification("Toolbar full — make room for the egg.");
                        }
                    }
                }
            }

            foreach (var p in projectiles)
            {
                Color c = p.AmmoType == "Bolts"
                    ? new Color((byte)180,(byte)160,(byte)80,(byte)255)
                    : new Color((byte)160,(byte)120,(byte)70,(byte)255);
                Vector2 tail = p.Pos - Vector2.Normalize(p.Vel) * 14f;
                Raylib.DrawLineEx(tail, p.Pos, 3f, c);
                Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 3, c);
            }

            foreach (var p in spellProjectiles)
{
    Color col = GetSpellColor(p.SpellType);
    float lifeRatio = p.Life / p.MaxLife;

    // animated pulsing ball
    float pulse = 1f + MathF.Sin(p.Life * 20f) * 0.3f;
    int radius = (int)(7 * pulse);

    // outer glow
    byte glowAlpha = (byte)(120 * lifeRatio);
    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius + 4,
        new Color(col.R, col.G, col.B, glowAlpha));
    // core
    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius, col);
    // bright centre
    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius / 2,
        new Color((byte)255,(byte)255,(byte)255,(byte)180));

    // spell-specific trail effects
    if (p.SpellType == "Fire")
        Raylib.DrawCircle((int)p.Pos.X - (int)(p.Vel.X * 0.02f),
            (int)p.Pos.Y - (int)(p.Vel.Y * 0.02f), radius - 2,
            new Color((byte)255,(byte)140,(byte)0,(byte)80));
    else if (p.SpellType == "Lightning")
    {
        Raylib.DrawLineEx(p.Pos, p.Pos - Vector2.Normalize(p.Vel) * 12f, 2f,
            new Color((byte)255,(byte)255,(byte)100,(byte)150));
    }
    else if (p.SpellType == "Dark")
    {
        Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius + 8,
            new Color((byte)40,(byte)0,(byte)60,(byte)60));
    }
}

foreach (var p in enemyProjectiles)
            {
                if (p.Kind == "Arrow")
                {
                    Color c = new Color((byte)190,(byte)120,(byte)60,(byte)255);
                    Vector2 tail = p.Pos - Vector2.Normalize(p.Vel) * 14f;
                    Raylib.DrawLineEx(tail, p.Pos, 3f, c);
                    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 3, c);
                }
                else // Spell
                {
                    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 9,
                        new Color((byte)140,(byte)60,(byte)200,(byte)120));
                    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 5,
                        new Color((byte)180,(byte)90,(byte)230,(byte)255));
                    Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 2, Color.White);
                }
            }


// ── REMOTE PLAYER ARROWS & SPELLS (visual only, no damage) ────────────────
foreach (var p in remoteVisualProjectiles)
{
    if (p.IsSpell)
    {
        Color col = GetSpellColor(p.Kind);
        float lifeRatio = Math.Clamp(p.Life / 1.5f, 0f, 1f);
        float pulse = 1f + MathF.Sin(p.Life * 20f) * 0.3f;
        int radius = (int)(7 * pulse);

        byte glowAlpha = (byte)(120 * lifeRatio);
        Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius + 4,
            new Color(col.R, col.G, col.B, glowAlpha));
        Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius, col);
        Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius / 2,
            new Color((byte)255,(byte)255,(byte)255,(byte)180));

        if (p.Kind == "Fire")
            Raylib.DrawCircle((int)p.Pos.X - (int)(p.Vel.X * 0.02f),
                (int)p.Pos.Y - (int)(p.Vel.Y * 0.02f), radius - 2,
                new Color((byte)255,(byte)140,(byte)0,(byte)80));
        else if (p.Kind == "Lightning")
            Raylib.DrawLineEx(p.Pos, p.Pos - Vector2.Normalize(p.Vel) * 12f, 2f,
                new Color((byte)255,(byte)255,(byte)100,(byte)150));
        else if (p.Kind == "Dark")
            Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, radius + 8,
                new Color((byte)40,(byte)0,(byte)60,(byte)60));
    }
    else
    {
        Color c = p.Kind == "Bolts"
            ? new Color((byte)180,(byte)160,(byte)80,(byte)255)
            : new Color((byte)160,(byte)120,(byte)70,(byte)255);
        Vector2 tail = p.Pos - Vector2.Normalize(p.Vel) * 14f;
        Raylib.DrawLineEx(tail, p.Pos, 3f, c);
        Raylib.DrawCircle((int)p.Pos.X, (int)p.Pos.Y, 3, c);
    }
}

DrawRocket((int)rocketPosition.X, (int)rocketPosition.Y);
    // "press E" prompt when near
    if (Vector2.Distance(player.Center, rocketPosition) < 100)
        Program.DrawTextUI("E = Launch into space", (int)rocketPosition.X - 40, (int)rocketPosition.Y - 120, 18, Color.White);

// Campfires
campfireAnimTimer += Raylib.GetFrameTime();
for (int i = 0; i < campfirePositions.Count; i++)
{
    Vector2 cf = campfirePositions[i];
    int cx = (int)cf.X, cy = (int)cf.Y;
    bool lit = CampfireIsLit(i);

    // log ring (always visible)
    Raylib.DrawRectangle(cx - 14, cy + 4,  10, 6, new Color((byte)100,(byte)60,(byte)20,(byte)255));
    Raylib.DrawRectangle(cx + 4,  cy + 4,  10, 6, new Color((byte)100,(byte)60,(byte)20,(byte)255));
    Raylib.DrawRectangle(cx - 6,  cy + 8,  12, 5, new Color((byte)80,(byte)45,(byte)15,(byte)255));

    if (lit)
    {
        // ember base
        Raylib.DrawCircle(cx, cy + 6, 8, new Color((byte)200,(byte)80,(byte)10,(byte)255));
        // animated flames
        float flicker = MathF.Sin(campfireAnimTimer * 8f + cf.X * 0.01f) * 4f;
        Raylib.DrawTriangle(
            new Vector2(cx - 8, cy + 6),
            new Vector2(cx + 8, cy + 6),
            new Vector2(cx + (int)(flicker * 0.5f), cy - 18 + (int)flicker),
            new Color((byte)220,(byte)100,(byte)10,(byte)220));
        Raylib.DrawTriangle(
            new Vector2(cx - 5, cy + 4),
            new Vector2(cx + 5, cy + 4),
            new Vector2(cx + (int)(flicker), cy - 10 + (int)(flicker * 0.5f)),
            new Color((byte)255,(byte)200,(byte)20,(byte)180));
        Raylib.DrawCircle(cx, cy - 4, 3, new Color((byte)255,(byte)240,(byte)80,(byte)120));
        Raylib.DrawCircle(cx, cy, 24, new Color((byte)255,(byte)120,(byte)10,(byte)30));
    }

    // interaction prompt
    if (Vector2.Distance(player.Center, cf) < 80)
    {
        string heldItem = toolbarSlots[toolbarSelectedSlot];
        bool holdingRaw = heldItem == "Fish" || heldItem == "Raw Meat"
                       || heldItem == "Potato" || heldItem == "Corn"
                       || heldItem == "Canteen (Dirty)";

        if (lit)
        {
            // show fuel status
            Program.DrawTextUI($"{campfireLogs.GetValueOrDefault(i)}/{MaxLogs} logs  {(int)campfireBurn.GetValueOrDefault(i)}s",
                cx - 40, cy - 58, 14, Color.Gold);

            if (holdingRaw)
                Program.DrawTextUI($"Space = Cook {heldItem}   |   E = Cooking menu", cx - 90, cy - 40, 20, Color.White);
            else
                Program.DrawTextUI("E = Cooking menu", cx - 55, cy - 40, 20, Color.White);

            if (campfireLogs.GetValueOrDefault(i) < MaxLogs)
                Program.DrawTextUI("R = Add log (stoke)", cx - 60, cy - 22, 20, Color.LightGray);

            if (Raylib.IsKeyPressed(KeyboardKey.Space) && holdingRaw && !isCooking)
                CookRawItemFromToolbar(heldItem);

            if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
            {
                cookingContext = "campfire";
                cookingMenuOpen = true;
            }
        }
        else
        {
            Program.DrawTextUI(player.Logs > 0 ? "R = Add log to light fire" : "Need Logs to light the fire",
                cx - 70, cy - 40, 20, Color.LightGray);
        }

        // add/stoke a log
        if (Raylib.IsKeyPressed(KeyboardKey.R)
            && campfireLogs.GetValueOrDefault(i) < MaxLogs && player.Logs > 0)
        {
            player.Logs--;
            campfireLogs[i] = campfireLogs.GetValueOrDefault(i) + 1;
            if (campfireBurn.GetValueOrDefault(i) <= 0f)
                campfireBurn[i] = LogBurnSeconds;
            ShowNotification(lit ? $"Stoked the fire ({campfireLogs[i]}/{MaxLogs})" : "Fire lit!");
        }
    }
}

// Incubators
            incubatorAnimTimer += Raylib.GetFrameTime();
            nearIncubator = incubatorPositions.Any(p => Vector2.Distance(player.Center, p) < 120);

            foreach (var inc in incubatorPositions)
            {
                int ix = (int)inc.X, iy = (int)inc.Y;
                // base + glass cavity
                Raylib.DrawRectangle(ix - 30, iy - 10, 60, 50, new Color((byte)70,(byte)70,(byte)90,(byte)255));
                Raylib.DrawRectangle(ix - 26, iy - 6, 52, 30, new Color((byte)40,(byte)40,(byte)55,(byte)255));
                Raylib.DrawRectangleLines(ix - 30, iy - 10, 60, 50, new Color((byte)20,(byte)20,(byte)30,(byte)255));

                // egg + warmth glow when active
                if (incubatingEgg != null)
                {
                    DrawEggIcon(incubatingEgg, ix, iy + 6, 30);
                    float pulse = (MathF.Sin(incubatorAnimTimer * 4f) + 1f) * 0.5f;   // 0→1
                    Raylib.DrawCircle(ix, iy + 6, 22,
                        new Color((byte)255,(byte)160,(byte)40,(byte)(30 + (byte)(40 * pulse))));
                }

                // interaction prompt
                // key handling when in range
                if (Vector2.Distance(player.Center, inc) < 120)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
                        incubatorMenuOpen = !incubatorMenuOpen;

                    if (Raylib.IsKeyPressed(KeyboardKey.P) && !chatInputOpen)
                        petStorageMenuOpen = !petStorageMenuOpen;
                }
            }

            // Draw bus
if (busOperating)
{
    int bx = (int)busPosition.X;
    int by = (int)busPosition.Y - 30;
    Color busCol = new Color((byte)20,(byte)60,(byte)180,(byte)255);
    // body
    Raylib.DrawRectangle(bx, by, 120, 40, busCol);
    Raylib.DrawRectangle(bx, by, 120, 6, new Color((byte)40,(byte)90,(byte)220,(byte)255));
    // windows row
    for (int w = 0; w < 4; w++)
        Raylib.DrawRectangle(bx + 8 + w * 28, by + 6, 22, 16, new Color((byte)160,(byte)210,(byte)240,(byte)200));
    // front windscreen
    Raylib.DrawRectangle(bx + 2, by + 6, 18, 16, new Color((byte)120,(byte)180,(byte)220,(byte)180));
    // wheels
    Raylib.DrawCircle(bx + 18, by + 42, 10, Color.Black);
    Raylib.DrawCircle(bx + 100, by + 42, 10, Color.Black);
    Raylib.DrawCircle(bx + 18, by + 42, 4, Color.DarkGray);
    Raylib.DrawCircle(bx + 100, by + 42, 4, Color.DarkGray);
    // destination sign
    Raylib.DrawRectangle(bx + 4, by - 14, 112, 14, new Color((byte)10,(byte)10,(byte)20,(byte)255));
    string destText = busMoving
        ? busStops[busNextStop].Name
        : busStops[busCurrentStop].Name;
    Program.DrawTextUI(destText, bx + 8, by - 13, 10, new Color((byte)255,(byte)220,(byte)50,(byte)255));

    // nearby prompt
    if (Vector2.Distance(player.Center, busPosition) < 80 && !busMoving)
    {
        Program.DrawTextUI("E = Bus Menu", bx + 10, by - 30, 16, Color.White);
    }
}

// Draw bus stop signs at each stop
foreach (var stop in busStops)
{
    int bx = (int)stop.WorldPos.X - 10;
    int by = (int)stop.WorldPos.Y - 240;
    Raylib.DrawRectangle((int)bx + 28, (int)by + 28, 6, 110, new Color((byte)80,(byte)88,(byte)96,(byte)255)); // pole
            Raylib.DrawRectangle((int)bx + 8, (int)by, 50, 30, new Color((byte)232,(byte)176,(byte)32,(byte)255));     // sign panel
            Raylib.DrawRectangle((int)bx + 12, (int)by + 4, 42, 22, new Color((byte)208,(byte)152,(byte)16,(byte)255)); // sign inner
            Program.DrawTextUI("BUS", (int)bx + 18, (int)by + 7, 10, Color.White);
            Program.DrawTextUI("STOP", (int)bx + 15, (int)by + 18, 9, Color.White);
            Raylib.DrawRectangle((int)bx, (int)by + 46, 76, 8, new Color((byte)74,(byte)80,(byte)90,(byte)255));       // shelter roof
            Raylib.DrawRectangle((int)bx, (int)by + 54, 8, 50, new Color((byte)128,(byte)144,(byte)160,(byte)128));    // shelter back
            Raylib.DrawRectangle((int)bx + 4, (int)by + 78, 64, 6, new Color((byte)106,(byte)120,(byte)136,(byte)255)); // bench
            Raylib.DrawRectangle((int)bx + 8, (int)by + 84, 5, 12, new Color((byte)80,(byte)88,(byte)96,(byte)255));   // bench leg L
            Raylib.DrawRectangle((int)bx + 59, (int)by + 84, 5, 12, new Color((byte)80,(byte)88,(byte)96,(byte)255));  // bench leg R
            Raylib.DrawRectangle((int)bx, (int)by + 96, 76, 4, new Color((byte)80,(byte)88,(byte)96,(byte)255));       // base
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
    
    if (Vector2.Distance(player.Center, axePosition) < 60)
        Program.DrawTextUI("E = Pick up Axe", ax - 50, ay - 50, 18, Color.Gold);
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
    if (Vector2.Distance(player.Center, pickaxePosition) < 60)
        Program.DrawTextUI("E = Pick up Pickaxe", px - 60, py - 50, 18, Color.Gold);
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
    if (Vector2.Distance(player.Center, fishingRodPosition) < 60)
        Program.DrawTextUI("E = Pick up Fishing Rod", rx - 70, ry - 60, 18, Color.Gold);
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
    if (Vector2.Distance(player.Center, fishingNetPosition) < 60)
        Program.DrawTextUI("E = Pick up Fishing Net", nx - 70, ny - 50, 18, Color.Gold);
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
    if (Vector2.Distance(player.Center, torchPosition) < 60)
        Program.DrawTextUI("E = Pick up Torch", tx - 50, ty - 55, 18, Color.Gold);
}

// world draw pass — ADD alongside your other pickup-item draw calls (Axe/Pickaxe sprites)
if (!spadePickedUp)
{
    Raylib.DrawRectangle((int)spadePosition.X - 3, (int)spadePosition.Y - 20, 6, 24, new Color((byte)120,(byte)80,(byte)40,(byte)255)); // handle
    Raylib.DrawTriangle(
        new Vector2(spadePosition.X - 10, spadePosition.Y),
        new Vector2(spadePosition.X + 10, spadePosition.Y),
        new Vector2(spadePosition.X, spadePosition.Y + 16),
        new Color((byte)160,(byte)160,(byte)170,(byte)255)); // blade
    Program.DrawTextUI("Spade", (int)spadePosition.X - 20, (int)spadePosition.Y + 20, 12, Color.White);
}

if (!wateringCanPickedUp)
{
    Raylib.DrawRectangle((int)wateringCanPosition.X - 12, (int)wateringCanPosition.Y - 10, 24, 16, new Color((byte)70,(byte)140,(byte)200,(byte)255)); // body
    Raylib.DrawRectangle((int)wateringCanPosition.X + 10, (int)wateringCanPosition.Y - 6, 14, 5, new Color((byte)70,(byte)140,(byte)200,(byte)255)); // spout
    Raylib.DrawRectangle((int)wateringCanPosition.X - 6, (int)wateringCanPosition.Y - 18, 12, 8, new Color((byte)50,(byte)110,(byte)170,(byte)255)); // handle
    Program.DrawTextUI("Watering Can", (int)wateringCanPosition.X - 34, (int)wateringCanPosition.Y + 10, 12, Color.White);
}

if (!wheatSeedsPickedUp)
{
    Raylib.DrawRectangle((int)wheatSeedsPosition.X - 10, (int)wheatSeedsPosition.Y - 8, 20, 16, new Color((byte)180,(byte)140,(byte)60,(byte)255)); // sack
    Raylib.DrawRectangle((int)wheatSeedsPosition.X - 10, (int)wheatSeedsPosition.Y - 12, 20, 5, new Color((byte)140,(byte)100,(byte)40,(byte)255)); // tied top
    Program.DrawTextUI("Wheat Seeds", (int)wheatSeedsPosition.X - 32, (int)wheatSeedsPosition.Y + 12, 12, Color.White);
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
    if (Vector2.Distance(player.Center, stickPosition) < 60)
        Program.DrawTextUI("E = Pick up Stick", sx - 50, sy - 50, 18, Color.Gold);
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
    if (Vector2.Distance(player.Center, swordPosition) < 60)
        Program.DrawTextUI("E = Pick up Sword", sx - 50, sy - 60, 18, Color.Gold);
}
// Bow pickup
if (!bowPickedUp)
{
    int bx = (int)bowSpawnPos.X;
    int by = (int)bowSpawnPos.Y;
    // glow behind
    Raylib.DrawCircle(bx, by - 10, 22, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    // bow limb (curved arc via two triangles)
    Raylib.DrawRectangle(bx - 10, by - 24, 5, 44, new Color((byte)150,(byte)90,(byte)40,(byte)255)); // wooden limb
    Raylib.DrawTriangle(
        new Vector2(bx - 10, by - 24),
        new Vector2(bx - 10, by + 20),
        new Vector2(bx + 2,  by - 2),
        new Color((byte)170,(byte)110,(byte)50,(byte)255));
    // bowstring
    Raylib.DrawLine(bx - 8, by - 22, bx - 8, by + 18, new Color((byte)230,(byte)230,(byte)220,(byte)255));
    if (Vector2.Distance(player.Center, bowSpawnPos) < 60)
        Program.DrawTextUI("E = Pick up Bow", bx - 50, by - 50, 18, Color.Gold);
}

// Crossbow pickup
if (!crossbowPickedUp)
{
    int cx2 = (int)crossbowSpawnPos.X;
    int cy2 = (int)crossbowSpawnPos.Y;
    // glow behind
    Raylib.DrawCircle(cx2, cy2 - 10, 22, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    // stock (vertical body)
    Raylib.DrawRectangle(cx2 - 2, cy2 - 20, 5, 34, new Color((byte)110,(byte)70,(byte)30,(byte)255));
    // cross limb (horizontal bow)
    Raylib.DrawRectangle(cx2 - 16, cy2 - 18, 33, 5, new Color((byte)150,(byte)90,(byte)40,(byte)255));
    // string
    Raylib.DrawLine(cx2 - 16, cy2 - 16, cx2 + 16, cy2 - 16, new Color((byte)230,(byte)230,(byte)220,(byte)255));
    // metal tip
    Raylib.DrawRectangle(cx2 - 1, cy2 - 26, 3, 8, new Color((byte)180,(byte)190,(byte)200,(byte)255));
    if (Vector2.Distance(player.Center, crossbowSpawnPos) < 60)
        Program.DrawTextUI("E = Pick up Crossbow", cx2 - 60, cy2 - 50, 18, Color.Gold);
}

// Staff pickup
if (!staffPickedUp)
{
    int sx = (int)staffSpawnPos.X;
    int sy = (int)staffSpawnPos.Y;
    Color staffCol = GetStaffColor(staffType);
    // glow
    Raylib.DrawCircle(sx, sy - 10, 22, new Color((byte)255,(byte)220,(byte)0,(byte)40));
    // shaft
    Raylib.DrawRectangle(sx - 2, sy - 26, 5, 44, new Color((byte)120,(byte)80,(byte)40,(byte)255));
    // orb at the top (staff element color)
    Raylib.DrawCircle(sx, sy - 28, 7, staffCol);
    Raylib.DrawCircleLines(sx, sy - 28, 7, Color.White);
    if (Vector2.Distance(player.Center, staffSpawnPos) < 60)
        Program.DrawTextUI("E = Pick up Staff", sx - 50, sy - 50, 18, Color.Gold);
}

// Tutorial chest (world draw)
if (!tutorialChestOpened)
{
    int chx = (int)tutorialChestPos.X;
    int chy = (int)tutorialChestPos.Y;
    // glow
    Raylib.DrawCircle(chx, chy - 4, 26, new Color((byte)255,(byte)220,(byte)0,(byte)50));
    // chest base
    Raylib.DrawRectangle(chx - 20, chy - 6, 40, 24, new Color((byte)120,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(chx - 20, chy - 6, 40, 24, new Color((byte)70,(byte)45,(byte)20,(byte)255));
    // lid
    Raylib.DrawRectangle(chx - 20, chy - 16, 40, 12, new Color((byte)150,(byte)100,(byte)50,(byte)255));
    Raylib.DrawRectangleLines(chx - 20, chy - 16, 40, 12, new Color((byte)70,(byte)45,(byte)20,(byte)255));
    // gold lock
    Raylib.DrawRectangle(chx - 4, chy - 8, 8, 8, new Color((byte)230,(byte)190,(byte)60,(byte)255));
    // metal bands
    Raylib.DrawRectangle(chx - 14, chy - 16, 3, 34, new Color((byte)90,(byte)90,(byte)100,(byte)255));
    Raylib.DrawRectangle(chx + 11, chy - 16, 3, 34, new Color((byte)90,(byte)90,(byte)100,(byte)255));

    if (Vector2.Distance(player.Center, tutorialChestPos) < 60)
        Program.DrawTextUI("E = Open chest", chx - 45, chy - 46, 18, Color.Gold);
}


            DrawStreetLights();
            fenceManager.Draw();
            DrawDroppedItems();
            DrawPlacedProps();
            DrawCoastguardStations();
            DrawBillboard();
            DrawCollectables("World");
            worldBoss?.Draw();
            superBoss?.Draw();
            if (activePet != null) activePet.Draw();
            if (pendingPet != null)
            {
                pendingPet.Draw();
                if (Vector2.Distance(player.Center, pendingPet.Position) < 150f)
                {
                    string msg = activePet != null
                        ? "Store or release your current pet to claim this one!"
                        : "Walk closer to collect your pet!";
                    Program.DrawTextUI(msg,
                        (int)pendingPet.Position.X - 110, (int)pendingPet.Position.Y - 30, 14, Color.White);
                }
            }

            if (MathF.Abs(playerElevation) > 1f)
                Raylib.DrawEllipse(
                    (int)player.Center.X,
                    (int)(player.Center.Y + 28 - playerElevation * 0.3f),
                    22, 7, new Color((byte)0,(byte)0,(byte)0,(byte)60));
            var savedPosW = player.Position;
            player.Position = new Vector2(player.Position.X, player.Position.Y + playerElevation);
            if (player.IsSwimming && player.SwimDepthRatio > 0.05f)
            {
                // sprite spans y+22 (head top) to y+70 (boots) = 48 tall; cap at 0.8 so the head stays dry
                float cutWorldY = (player.Position.Y + 70) - 48f * Math.Min(player.SwimDepthRatio, 0.9f) - 8f;
                Vector2 cutScreen = Raylib.GetWorldToScreen2D(new Vector2(0, cutWorldY), camera);
                Raylib.BeginScissorMode(0, 0, ScreenWidth, (int)cutScreen.Y);
                player.Draw();
                Raylib.EndScissorMode();
                DrawSwimmingOverlay();
            }
            else
            {
                player.Draw();
            }
            player.Position = savedPosW;

            foreach (var rp in remotePlayers) rp.Draw();

            Raylib.EndMode2D();

            Color overlay = GetNightOverlay();
            bool hasLight = (GetEquippedTool() == "Torch" || vehicles.Any(v => v.Driving && v.HeadlightsOn)) && overlay.A > 80;
            torchActive = hasLight;
            if (hasLight)
            {
                var lightCenters = new List<Vector2>();
                if (GetEquippedTool() == "Torch")
                    lightCenters.Add(Raylib.GetWorldToScreen2D(player.Center, camera));
                foreach (var v in vehicles)
                    if (v.Driving && v.HeadlightsOn)
                    {
                        Rectangle vb = v.Bounds;
                        Vector2 vc = new Vector2(vb.X + vb.Width / 2, vb.Y + vb.Height / 2);
                        lightCenters.Add(Raylib.GetWorldToScreen2D(vc, camera));
                    }

                Raylib.BeginTextureMode(nightMask);
                Raylib.ClearBackground(Color.Blank);
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, overlay);
                Raylib.BeginBlendMode(BlendMode.SubtractColors);
                foreach (var lc in lightCenters)
                    Raylib.DrawCircleGradient((int)lc.X, (int)lc.Y, LightRadius,
                        new Color((byte)0, (byte)0, (byte)0, (byte)255),
                        new Color((byte)0, (byte)0, (byte)0, (byte)0));
                Raylib.EndBlendMode();
                Raylib.EndTextureMode();

                Raylib.DrawTexturePro(nightMask.Texture,
                    new Rectangle(0, 0, nightMask.Texture.Width, -nightMask.Texture.Height),
                    new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                    Vector2.Zero, 0f, Color.White);
            }
            else
            {
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, overlay);
            }
            DrawNightSky();

            if (buildingPromptTimer > 0)
            {
                byte alpha = (byte)(255 * Math.Min(1f, buildingPromptTimer));
                Program.DrawTextUI(buildingPromptMessage, 480, 560, 30, Color.Yellow);
            }

            // ── incubator hint banner ──
    if (nearIncubator)
    {
        string hint = "Press E to incubate  |  Press P to store pet";
        int fs = 20;
        int tw = Program.MeasureTextUI(hint, fs);
        int bx = ScreenWidth / 2 - tw / 2 - 16;
        int by = ScreenHeight - 90;
        Raylib.DrawRectangle(bx, by, tw + 32, 40, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Raylib.DrawRectangleLines(bx, by, tw + 32, 40, Color.Gold);
        Program.DrawTextUI(hint, bx + 16, by + 10, fs, Color.White);
    }

    if (nearRiver && !isFishing)
    {
        string tool = GetEquippedTool();
        string msg = (BaseTool(tool) == "Net" || BaseTool(tool) == "Rod")
            ? "SPACE = Fish the river"
            : "Equip a Net or Rod to fish here";
        Program.DrawTextUI(msg, ScreenWidth/2 - 120, ScreenHeight - 150, 20, Color.White);
    }

    if (nearMailbox)
    {      
    string msg = nearMailboxHasMail ? "[E] Collect mail" : "[E] Check mailbox (empty)";
        Program.DrawTextUI(msg, ScreenWidth/2 - 110, ScreenHeight - 150, 20,
            nearMailboxHasMail ? Color.Gold : Color.LightGray);
    }
            
            DrawWeather();
            DrawHUD();
            DrawChestUI();
            DrawFurnaceUI();          
            DrawPlaceablePrompts();
            DrawArmorUI();
            DrawLandForSaleUI();
            DrawBillboardUI();

            // ── HOUSE BUILDING ANIMATION ──────────────────────────────────────────────
    if (houseBuildingActive)
    {
        byte alpha = (byte)(255 * houseBuildingAlpha);
        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight,
            new Color((byte)0,(byte)0,(byte)0,(byte)alpha));

        if (houseBuildingAlpha >= 1f)
        {
            // animated hammer — bobs up and down
            float bob    = MathF.Sin(houseBuildingTimer * 6f) * 18f;
            float angle  = MathF.Sin(houseBuildingTimer * 5f) * 30f; // swing angle degrees
            int   hx     = ScreenWidth  / 2;
            int   hy     = ScreenHeight / 2 - 40 + (int)bob;

            // hammer handle
            Raylib.DrawRectangle(hx - 4,  hy,       8,  70,
                new Color((byte)160,(byte)110,(byte)50,(byte)255));
            // hammer head
            Raylib.DrawRectangle(hx - 28, hy - 22,  56, 24,
                new Color((byte)100,(byte)100,(byte)105,(byte)255));
            Raylib.DrawRectangle(hx - 28, hy - 22,  56,  6,
                new Color((byte)140,(byte)140,(byte)145,(byte)255));
            // impact sparks when hammer is at bottom of swing
            if (bob > 12f)
            {
                Raylib.DrawCircle(hx - 20, hy + 70, 4,
                    new Color((byte)255,(byte)200,(byte)50,(byte)200));
                Raylib.DrawCircle(hx + 20, hy + 70, 3,
                    new Color((byte)255,(byte)160,(byte)30,(byte)180));
                Raylib.DrawCircle(hx,      hy + 74, 5,
                    new Color((byte)255,(byte)220,(byte)80,(byte)220));
            }

            // dust particles
            for (int d = 0; d < 5; d++)
            {
                float dustX = hx - 60 + d * 28 + MathF.Sin(houseBuildingTimer * 3f + d) * 8f;
                float dustY = hy + 80  + MathF.Cos(houseBuildingTimer * 4f + d) * 6f;
                byte  dustA = (byte)(120 + (int)(MathF.Sin(houseBuildingTimer * 5f + d) * 60f));
                Raylib.DrawCircle((int)dustX, (int)dustY, 5,
                    new Color((byte)180,(byte)160,(byte)120,(byte)dustA));
            }

            // progress bar
            float progress = houseBuildingTimer / houseBuildingDuration;
            int   barW     = 300;
            int   barX     = ScreenWidth / 2 - barW / 2;
            int   barY     = ScreenHeight / 2 + 100;
            Raylib.DrawRectangle(barX,     barY, barW,     16,
                new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangle(barX,     barY, (int)(barW * progress), 16,
                new Color((byte)220,(byte)160,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(barX, barY, barW,    16, Color.Gold);

            // text
            string msg = houseBuildingTimer < 1.5f ? "Breaking ground..."
                       : houseBuildingTimer < 2.8f ? "Building your house..."
                       : "Almost done!";
            int tw = Program.MeasureTextUI(msg, 24);
            Program.DrawTextUI(msg, ScreenWidth / 2 - tw / 2, hy + 120, 24, Color.Gold);

            int tw2 = Program.MeasureTextUI("YOUR NEW HOME", 36);
            Program.DrawTextUI("YOUR NEW HOME",
                ScreenWidth / 2 - tw2 / 2, hy - 100, 36,
                new Color((byte)220,(byte)180,(byte)40,(byte)255));
        }
    }          

            // ── LOCAL SPEECH BUBBLE ───────────────────────────────────────────────────
if (playerChatTimer > 0f && playerChatMessage.Length > 0)
{
    Vector2 screenPos = Raylib.GetWorldToScreen2D(
        new Vector2(player.Position.X + 20, player.Position.Y), camera);
    DrawSpeechBubbleScreen((int)screenPos.X, (int)screenPos.Y - 20,
        playerChatMessage, playerChatTimer / playerChatDuration);
}

// ── CHAT INPUT BOX ────────────────────────────────────────────────────────
if (chatInputOpen)
{
    int boxW = 500; int boxH = 40;
    int boxX = ScreenWidth / 2 - boxW / 2;
    int boxY = ScreenHeight - 80;
    Raylib.DrawRectangle(boxX, boxY, boxW, boxH, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLines(boxX, boxY, boxW, boxH, Color.White);
    Program.DrawTextUI("Say:", boxX - 44, boxY + 10, 20, Color.LightGray);
    Program.DrawTextUI(chatInputText, boxX + 8, boxY + 10, 20, Color.White);
    if ((int)(Raylib.GetTime() * 2) % 2 == 0)
        Program.DrawTextUI("|", boxX + 8 + Program.MeasureTextUI(chatInputText, 20), boxY + 10, 20, Color.White);
    Program.DrawTextUI("ENTER = Send   ESC = Cancel", boxX, boxY + 46, 14, Color.DarkGray);
}
            
            
        }
    }
}
