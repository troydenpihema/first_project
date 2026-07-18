using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void UpdateShrines()
        {
            void Shrine(Vector2 pos, bool holy)
            {
                if (Vector2.Distance(player.Center, pos) > 90) return;
                if (Raylib.IsKeyPressed(KeyboardKey.E) && GetItemCount("Spirit Essence") > 0)
                {
                    RemoveOneItem("Spirit Essence");
                    if (holy) player.AddMysticalXP(10); else player.AddDarkArtsXP(10);
                }
            }
            Shrine(holyShrinePos, true);
            Shrine(darkShrinePos, false);
        }

        static void DrawShrines()
        {
            void ShrineVis(Vector2 pos, bool holy)
            {
                Vector2 sp = Raylib.GetWorldToScreen2D(pos, camera); 
                Raylib.DrawRectangle((int)sp.X - 20, (int)sp.Y - 40, 40, 60, holy ? new Color((byte)220,(byte)210,(byte)170,(byte)255) : new Color((byte)40,(byte)30,(byte)50,(byte)255));
                if (Vector2.Distance(player.Center, pos) <= 90)
                    Program.DrawTextUI($"E = Sacrifice Spirit Essence ({(holy ? "Mystical" : "Dark Arts")} +10xp)", (int)sp.X - 140, (int)sp.Y - 70, 16, Color.White);
            }
            ShrineVis(holyShrinePos, true);
            ShrineVis(darkShrinePos, false);
        }

        static Color PetColorFor(string bossOrEggName)
        {
            if (bossOrEggName.Contains("Colossus")) return new Color((byte)90,(byte)40,(byte)120,(byte)255);
            if (bossOrEggName.Contains("Titan"))    return new Color((byte)140,(byte)20,(byte)20,(byte)255);
            return new Color((byte)80,(byte)180,(byte)200,(byte)255);
        }

static void UpdateSurvival(float dt)
{
    bool exhausted = player.Stamina <= 0f;
    float drainMult = exhausted ? 2f : 1f;                 // running on empty burns you out faster
    player.Food   = Math.Max(0f, player.Food   - dt * (100f / 600f) * drainMult);  // ~10 min
    player.Thirst = Math.Max(0f, player.Thirst - dt * (100f / 420f) * drainMult);  // ~7 min

    bool moving = Vector2.Distance(player.Position, survLastPos) > 1f;
    survLastPos = player.Position;
    bool mounted = rideables.Any(r => r.Riding);

    if (moving && !mounted)
        player.Stamina = Math.Max(0f, player.Stamina - dt * 2.5f);
    else  // higher Stamina level = faster recovery
        player.Stamina = Math.Min(100f, player.Stamina + dt * (2f + player.StaminaLevel * 0.05f));

    // starving or parched — slow HP drain (never kills outright)
    if (player.Food <= 0f || player.Thirst <= 0f)
    {
        survivalHpTick += dt;
        if (survivalHpTick >= 3f)
        {
            survivalHpTick = 0f;
            if (player.Health > 1) player.Health = Math.Max(1, player.Health - 2);
        }
    }
    else survivalHpTick = 0f;
}

static string AnimalFeedFor(Rideable.RideableType t) => t switch
{
    Rideable.RideableType.Donkey   => "Hay",
    Rideable.RideableType.Tiger    => "Raw Meat",
    Rideable.RideableType.Horse    => "Carrot",
    Rideable.RideableType.Dolphin  => "Fish",
    Rideable.RideableType.Camel    => "Hay",
    Rideable.RideableType.Reindeer => "Hay",
    Rideable.RideableType.Elephant => null,   // slow boys only need a watering hole
    _ => null
};

        record ElevationZone(Rectangle Bounds, ElevationType Kind, int HeightPx);

        record FishSpecies(string Name, int Value, int Weight, string Water, string Tool);

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

    // Exact zones (unchanged positions)
    if (player.Position.X > -3000 && player.Position.X < 0 &&
        player.Position.Y > -10000 && player.Position.Y < -6000)
        zoneMusic = musicFarm;
    else if (player.Position.X > -3000 && player.Position.X < 4000 &&
             player.Position.Y > -1500 && player.Position.Y < 2500)
        zoneMusic = musicCity;
    // Grid sectors
    else if (player.Position.X > -80000 && player.Position.X < 80000 &&
             player.Position.Y > -80000 && player.Position.Y < 80000)
        zoneMusic = musicMeadowlands;
    else if (player.Position.X >= 80000 &&
             player.Position.Y > -80000 && player.Position.Y < 80000)
        zoneMusic = musicDesert;
    else if (player.Position.X < -80000 && player.Position.Y < -80000)
        zoneMusic = musicSnow;      // add musicSnow or reuse existing
    else if (player.Position.X >= -80000 && player.Position.X < 80000 &&
             player.Position.Y < -80000)
        zoneMusic = musicSnow;   // add musicMountain or reuse existing
    else if (player.Position.X >= 80000 && player.Position.Y < -80000)
        zoneMusic = musicVolcano;
    else if (player.Position.X < -80000 &&
             player.Position.Y >= -80000 && player.Position.Y < 80000)
        zoneMusic = musicSnow;      // add musicSwamp or reuse existing
    else if (player.Position.X < -80000 && player.Position.Y >= 80000)
        zoneMusic = musicForest;
    else if (player.Position.Y >= 80000 && player.Position.Y < 115000)
        zoneMusic = musicBeach;
    else if (player.Position.Y >= 115000)
        zoneMusic = musicOcean;
    else
        zoneMusic = musicMainMenu;

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

static void StartIncubation(string egg)
{
    if (incubatingEgg != null) { ShowNotification("The incubator is already in use."); return; }
    if (pendingPet != null)    { ShowNotification("Claim the hatched pet before incubating another egg."); return; }
    incubatingEgg = egg;
    incubationProgress = 0f;
    incubationNeeded = 1f / daySpeed;   // one full day cycle
    ShowNotification($"Incubating {egg}. It will hatch in one full day.");
}

static void UpdateIncubation(float dt)
{
    if (incubatingEgg == null) return;
    incubationProgress += dt;
    if (incubationProgress >= incubationNeeded)
    {
        string type = incubatingEgg.Replace(" Egg", "");
        // spawn at the incubator that's currently running it (nearest one to player as fallback)
        Vector2 spawnAt = incubatorPositions.Count > 0
            ? incubatorPositions.OrderBy(p => Vector2.Distance(player.Center, p)).First()
            : player.Center;
        pendingPet = new Pet(spawnAt, type, PetColorFor(incubatingEgg));
        ShowNotification($"Your {type} egg hatched! Walk over to collect it.");
        incubatingEgg = null;
        incubationProgress = 0f;
    }
}

static void UpdatePendingPet()
{
    if (pendingPet == null) return;
    if (Vector2.Distance(player.Center, pendingPet.Position) > petCollectRange) return;

    if (activePet != null)
    {
        // can't claim while a pet is already following — prompt is shown in the draw loop
        return;
    }

    activePet = pendingPet;
    pendingPet = null;
    ShowNotification($"{activePet.Type} joined you!");
}

static void StoreActivePet()
{
    if (activePet == null) { ShowNotification("No pet is following you."); return; }
    if (activePet.Adopted)                                   
    {
        activePet.AtSanctuary = true;                        
        ShowNotification($"{activePet.Type} is safe at the sanctuary.");
        return;
    }
    storedPets.Add(activePet.Type);
    ShowNotification($"{activePet.Type} was sent to storage.");
    activePet = null;
}

static void WithdrawPet(int index)
{
    if (index < 0 || index >= storedPets.Count) return;
    if (activePet != null) { ShowNotification("Store your current pet first."); return; }
    string type = storedPets[index];
    storedPets.RemoveAt(index);
    activePet = new Pet(player.Center, type, PetColorFor(type + " Egg"));
    activePet = Pet.NewBaby(player.Center, "Kitten", "Cat", PetColorFor("Cat Egg"), 120f);
    ShowNotification($"{type} is now following you!");
}

static void DrawPetStorageMenu()
{
    if (!petStorageMenuOpen) return;

    int pw = 480, ph = 360;
    int px = ScreenWidth / 2 - pw / 2, py = ScreenHeight / 2 - ph / 2;
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)120));
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)24,(byte)24,(byte)34,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, Color.Gold);
    Program.DrawTextUI("PET STORAGE", px + 20, py + 18, 26, Color.Gold);
    Program.DrawTextUI("[Q] Close", px + pw - 110, py + 22, 16, Color.LightGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) { petStorageMenuOpen = false; return; }

    // currently following pet + store button
    Program.DrawTextUI("Following:", px + 20, py + 60, 18, Color.White);
    if (activePet != null)
    {
        Raylib.DrawCircle(px + 130, py + 70, 12, activePet.BodyColor);
        Program.DrawTextUI(activePet.Type, px + 150, py + 60, 18, Color.White);
        Rectangle storeBtn = new Rectangle(px + pw - 150, py + 54, 120, 30);
        bool hs = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), storeBtn);
        Raylib.DrawRectangleRec(storeBtn, hs ? Color.Gold : new Color((byte)50,(byte)50,(byte)65,(byte)255));
        Program.DrawTextUI("Store", px + pw - 120, py + 60, 18, hs ? Color.Black : Color.White);
        if (hs && Raylib.IsMouseButtonPressed(MouseButton.Left)) StoreActivePet();
    }
    else
    {
        Program.DrawTextUI("(none)", px + 130, py + 60, 18, Color.Gray);
    }

    // stored list
    Program.DrawTextUI("Stored pets:", px + 20, py + 110, 18, Color.LightGray);
    if (storedPets.Count == 0)
        Program.DrawTextUI("Storage is empty.", px + 20, py + 140, 16, Color.Gray);

    for (int i = 0; i < storedPets.Count; i++)
    {
        int ry = py + 140 + i * 44;
        Rectangle r = new Rectangle(px + 20, ry, pw - 40, 38);
        bool hov = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r);
        Raylib.DrawRectangleRec(r, hov ? new Color((byte)60,(byte)60,(byte)80,(byte)255)
                                       : new Color((byte)40,(byte)40,(byte)55,(byte)255));
        Raylib.DrawRectangleLinesEx(r, 2, hov ? Color.Gold : Color.DarkGray);
        Raylib.DrawCircle(px + 44, ry + 19, 10, PetColorFor(storedPets[i] + " Egg"));
        Program.DrawTextUI(storedPets[i], px + 64, ry + 9, 18, Color.White);
        Program.DrawTextUI(activePet == null ? "[Withdraw]" : "[Store current first]",
            px + pw - 190, ry + 10, 16, activePet == null ? Color.Gold : Color.Gray);
        if (hov && activePet == null && Raylib.IsMouseButtonPressed(MouseButton.Left))
            WithdrawPet(i);
    }
}

static void CheckSendAppearanceIfChanged()
{
    string heldNow = GetActiveItem() ?? "";
    string weaponNow = armorWeapon ?? "";
    bool twoHandedNow = weaponNow.Length > 0 && IsTwoHandedWeapon(weaponNow);
    string hairStyleNow = playerHairStyle ?? "None";
    string facialHairNow = playerFacialHair ?? "None";
    int remoteNow = multiplayer.RemotePlayers.Count;
    if (remoteNow > lastKnownRemoteCount)
        lastSentSkin = default;          
    lastKnownRemoteCount = remoteNow;

    if (player.SkinColor.R != lastSentSkin.R || player.SkinColor.G != lastSentSkin.G || player.SkinColor.B != lastSentSkin.B ||
        playerHairColor.R != lastSentHair.R || playerHairColor.G != lastSentHair.G || playerHairColor.B != lastSentHair.B ||
        playerFacialHairColor.R != lastSentFacial.R ||
        player.ShirtColor.R != lastSentShirt.R || player.ShirtColor.G != lastSentShirt.G || player.ShirtColor.B != lastSentShirt.B ||
        player.PantsColor.R != lastSentPants.R || player.PantsColor.G != lastSentPants.G || player.PantsColor.B != lastSentPants.B ||
        armorHelmet != lastSentHelmet || armorBody != lastSentBody || armorLegs != lastSentLegs ||
        armorBoots != lastSentBoots || armorGloves != lastSentGloves || armorCape != lastSentCape ||
        armorShield != lastSentShield || heldNow != lastSentHeld ||
        hairStyleNow != lastSentHairStyle || facialHairNow != lastSentFacial2 ||
        weaponNow != lastSentWeapon || twoHandedNow != lastSentTwoHanded)
    {
        multiplayer.SendAppearance(player.SkinColor, playerHairColor, playerFacialHairColor,
            player.ShirtColor, player.PantsColor,
            armorHelmet ?? "", armorBody ?? "", armorLegs ?? "", armorBoots ?? "",
            armorGloves ?? "", armorCape ?? "", armorShield ?? "", heldNow,
            hairStyleNow, facialHairNow, weaponNow, twoHandedNow);

        lastSentSkin = player.SkinColor;
        lastSentHair = playerHairColor;
        lastSentFacial = playerFacialHairColor;
        lastSentShirt = player.ShirtColor;
        lastSentPants = player.PantsColor;
        lastSentHelmet = armorHelmet; lastSentBody = armorBody; lastSentLegs = armorLegs;
        lastSentBoots = armorBoots; lastSentGloves = armorGloves; lastSentCape = armorCape;
        lastSentShield = armorShield; lastSentHeld = heldNow;
        lastSentHairStyle = hairStyleNow; lastSentFacial2 = facialHairNow;
        lastSentWeapon = weaponNow; lastSentTwoHanded = twoHandedNow;
    }
}

static FishSpecies RollFish(string water, string tool)
{
    // valid species for this water + tool (Any tool matches both)
    var pool = fishTypes.Where(f => f.Water == water && (f.Tool == "Any" || f.Tool == tool)).ToList();
    if (pool.Count == 0) return null;

    // seasonal weighting: in-season fish get full weight, out-of-season get halved
    int totalWeight = pool.Sum(f => IsFishInSeason(f.Name) ? f.Weight : Math.Max(1, f.Weight / 2));
    int roll = Raylib.GetRandomValue(1, totalWeight);
    int acc = 0;
    foreach (var f in pool)
    {
        acc += IsFishInSeason(f.Name) ? f.Weight : Math.Max(1, f.Weight / 2);
        if (roll <= acc) return f;
    }
    return pool[0];
}

static void UpdateTutorial(float dt)
{
    if (tutorialMessageTimer > 0) tutorialMessageTimer -= dt;
    if (tutorialCompleted) { tutorialActive = false; return; }
    if (!tutorialActive) return;

    
    if (tutorialStep >= tutorialTasks.Count)
    {
        tutorialActive = false;
        tutorialCompleted = true;
        tutorialMessage = "Training complete! The farm gates are open. Good luck!";
        tutorialMessageTimer = 4f;
        return;
    }

    var task = tutorialTasks[tutorialStep];

    // NPC smoothly follows behind the player, drifting toward the current guide spot
    tutorialNpcBob += dt;
    Vector2 followTarget = Vector2.Lerp(player.Position + new Vector2(-60, -40), task.GuidePos, 0.35f);
    tutorialNpcPos = Vector2.Lerp(tutorialNpcPos, followTarget, 2.5f * dt);

    // ---- AUTO-DETECT task completion ----
    bool justCompleted = false;
    switch (tutorialStep)
    {
        case 0: // Woodcutting — detect a tree chop via XP gain
            if (player.WoodcuttingXP > tutorialWoodMark || player.WoodcuttingLevel > 1) justCompleted = true;
            break;
        case 1: // Mining
            if (player.MiningXP > tutorialMineMark || player.MiningLevel > 1) justCompleted = true;
            break;
        case 2: // Fishing
            if (player.FishingXP > tutorialFishMark || player.FishingLevel > 1) justCompleted = true;
            break;
        case 3: // Net Fishing
            if (player.FishingXP > tutorialNetFishMark) justCompleted = true;
            break;
        case 4: // Cooking
            if (player.CookingXP > tutorialCookMark) justCompleted = true;
            break;
        case 5: // Combat
            if (player.CombatXP > tutorialCombatMark || player.CombatLevel > 1) justCompleted = true;
            break;
        case 6:   // Ranged — completes when a shot is fired (ranged XP rises)
            if (player.RangedXP > tutorialRangedMark) justCompleted = true;
            break;
        case 7: // Magic — completes when a spell is cast (elemental XP rises)
            if (player.ElementalXP > tutorialMagicMark) justCompleted = true;
            break;
        case 8:  // Crafting
            if (player.CraftingXP > tutorialCraftMark || player.CraftingLevel > 1) justCompleted = true;
            break;
        case 9:  // Farming
            if (player.FarmingXP > tutorialFarmMark || player.FarmingLevel > 1) justCompleted = true;
            break;
        case 10: // Riding
            if (player.RidingXP > tutorialRideMark || player.RidingLevel > 1) justCompleted = true;
            break;
        case 11: // Get Your ID — completes when the photo is taken (ID becomes pending)
            if (idPending || idMailWaiting || idClaimed) justCompleted = true;
            break;   
        
    }

    if (justCompleted)
    {
        task.Done = true;
        tutorialStep++;
        tutorialMessage = $"Great! {task.Title} learned. Gate opened!";
        tutorialMessageTimer = 4f;

        // snapshot marks for the next task so we detect fresh XP gains
        SnapshotTutorialMarks();
    }
}

static void SyncWorldClockIfHost(float dt)
{
    if (!multiplayer.Connected || !multiplayer.IsHost) return;
    clockSyncTimer -= dt;
    if (clockSyncTimer > 0f) return;
    clockSyncTimer = 1.0f;   // once a second is plenty for a slow clock
    multiplayer.BroadcastWorldClock(timeOfDay, dayOfWeek, dayOfMonth, currentMonth, isRaining);
}

static void SyncEnemiesIfHost(float dt)
{
    if (!multiplayer.Connected || !multiplayer.IsHost) return;
    enemySyncTimer -= dt;
    if (enemySyncTimer > 0f) return;
    enemySyncTimer = 0.1f;   // 10 Hz, same cadence as boss

    for (int i = 0; i < enemies.Count; i++)
    {
        var e = enemies[i];
        multiplayer.BroadcastEnemyState(i, e.Position.X, e.Position.Y, e.Health, e.Dead, e.Aggro);
    }
}

static void SnapshotTutorialMarks()
{
    tutorialWoodMark   = player.WoodcuttingXP;
    tutorialMineMark   = player.MiningXP;
    tutorialFishMark   = player.FishingXP;
    tutorialCombatMark = player.CombatXP;
    tutorialNetFishMark = player.FishingXP;
    tutorialCookMark    = player.CookingXP;
    tutorialRangedMark = player.RangedXP;
    tutorialMagicMark  = player.ElementalXP;
    tutorialCraftMark = player.CraftingXP;
    tutorialFarmMark  = player.FarmingXP;
    tutorialRideMark  = player.RidingXP;
}

static void TryStartFishing(string water)
{
    string tool = GetEquippedTool();
    if (BaseTool(tool) != "Rod" && BaseTool(tool) != "Net")
    {
        ShowNotification("Equip a Fishing Rod or Net first!");
        return;
    }
    isFishing = true;
    fishingPhase = 0;
    fishingTimer = 0f;
    fishingBiteTime = Raylib.GetRandomValue(15, 40) / 10f;
    fishingWater = water;
    fishingResult = $"Casting in the {water.ToLower()}... wait for a bite!";
    fishingResultTimer = 2f;
}

static void DrawNightSky()
{
    float d = GetDarkness();
    if (d < 0.55f) return;
    byte fade = (byte)(255 * ((d - 0.55f) / 0.45f));   // stars fade in as full dark approaches
    float t = (float)Raylib.GetTime();

    // stars — deterministic positions from index, gentle twinkle, upper 55% of screen
    for (int i = 0; i < 46; i++)
    {
        int sx = (i * 137 + 61)  % ScreenWidth;
        int sy = (i * 89  + 23)  % (int)(ScreenHeight * 0.55f);
        float tw = 0.55f + 0.45f * MathF.Sin(t * 1.7f + i * 2.3f);
        byte a = (byte)(fade * tw);
        Raylib.DrawPixel(sx, sy, new Color((byte)235,(byte)240,(byte)255, a));
        if (i % 7 == 0)   // a few brighter ones
            Raylib.DrawCircle(sx, sy, 1.4f, new Color((byte)255,(byte)255,(byte)255, (byte)(a / 2)));
    }

    // moon — arcs across the sky over the night (8pm → 5am)
    float hour = timeOfDay * 24f;
    float h = hour < 7f ? hour + 24f : hour;
    float mt = Math.Clamp((h - 20f) / 9f, 0f, 1f);     // 0 at 8pm, 1 at 5am
    int mx = (int)(ScreenWidth * 0.08f + (ScreenWidth * 0.84f) * mt);
    int my = (int)(ScreenHeight * 0.22f - MathF.Sin(mt * MathF.PI) * ScreenHeight * 0.1f);
    Raylib.DrawCircle(mx, my, 26, new Color((byte)225,(byte)225,(byte)210, fade));
    Raylib.DrawCircle(mx + 9, my - 6, 22, new Color((byte)0,(byte)0,(byte)30, fade));   // crescent bite, matches night tint
    Raylib.DrawCircle(mx, my, 34, new Color((byte)225,(byte)225,(byte)200, (byte)(fade / 6)));   // halo
}

static void EndFishing()
{
    isFishing = false;
    fishingPhase = 0;
    fishingTimer = 0f;
    currentLake = null;
}

static void DrawFishingUI()
{
    if (!isFishing) return;

    int barW = 500;
    int barX = ScreenWidth / 2 - barW / 2;
    int panelY = ScreenHeight - 150;

    Raylib.DrawRectangle(0, panelY - 10, ScreenWidth, 110, new Color((byte)0,(byte)0,(byte)0,(byte)170));

    // status text
    if (fishingResultTimer > 0 || fishingPhase < 2)
    {
        int tw = Program.MeasureTextUI(fishingResult, 26);
        Color c = fishingResult.Contains("BITE") ? Color.Yellow
                : fishingResult.Contains("PERFECT") ? Color.Gold
                : fishingResult.Contains("away") || fishingResult.Contains("Missed") || fishingResult.Contains("snapped") ? Color.Red
                : Color.SkyBlue;
        Program.DrawTextUI(fishingResult, ScreenWidth / 2 - tw / 2, panelY, 26, c);
    }

    // phase 0 — waiting indicator (a little bobbing dot)
    if (fishingPhase == 0)
    {
        float bob = MathF.Sin((float)Raylib.GetTime() * 4f) * 4f;
        Raylib.DrawCircle(ScreenWidth / 2, panelY + 55 + (int)bob, 8, Color.White);
        Program.DrawTextUI("Waiting for a bite...", ScreenWidth / 2 - 90, panelY + 75, 16, Color.LightGray);
    }

    // phase 1 — react prompt flashing
    if (fishingPhase == 1)
    {
        if ((int)(Raylib.GetTime() * 6) % 2 == 0)
            Program.DrawTextUI("! SPACE !", ScreenWidth / 2 - 60, panelY + 45, 34, Color.Yellow);
    }

    // phase 2 — reeling bar
    if (fishingPhase == 2)
    {
        int by = panelY + 50;
        Raylib.DrawRectangle(barX, by, barW, 28, new Color((byte)30,(byte)40,(byte)60,(byte)255));
        // green target zone
        int zoneX = barX + (int)(reelTargetMin * barW);
        int zoneW = (int)((reelTargetMax - reelTargetMin) * barW);
        Raylib.DrawRectangle(zoneX, by, zoneW, 28, new Color((byte)0,(byte)200,(byte)0,(byte)200));
        // perfect centre line
        int cx = barX + (int)((reelTargetMin + reelTargetMax) / 2f * barW);
        Raylib.DrawRectangle(cx - 1, by, 2, 28, new Color((byte)255,(byte)255,(byte)150,(byte)255));
        // moving cursor
        int curX = barX + (int)(reelBarPos * barW);
        Raylib.DrawRectangle(curX - 4, by - 6, 8, 40, Color.White);
        Raylib.DrawRectangleLines(barX, by, barW, 28, Color.White);
        Program.DrawTextUI("SPACE in the green!  Q = Stop", ScreenWidth / 2 - 130, by + 36, 16, Color.LightGray);
    }
}

static void DrawTutorialWorld()
{
    if (!tutorialActive) return;

    // gates
    foreach (var gate in tutorialGates)
    {
        bool locked = tutorialStep <= gate.UnlockedByStep;
        Color gc = locked ? new Color((byte)150,(byte)40,(byte)40,(byte)255) : new Color((byte)40,(byte)150,(byte)40,(byte)180);
        Raylib.DrawRectangleRec(gate.Bounds, gc);
        if (locked)
            Program.DrawTextUI("LOCKED", (int)gate.Bounds.X + 10, (int)gate.Bounds.Y - 18, 14, Color.White);
    }

    // companion NPC — simple figure with a bobbing motion
    float bob = MathF.Sin(tutorialNpcBob * 3f) * 4f;
    int nx = (int)tutorialNpcPos.X;
    int ny = (int)(tutorialNpcPos.Y + bob);
    Raylib.DrawCircle(nx, ny - 18, 12, new Color((byte)255,(byte)220,(byte)170,(byte)255)); // head
    Raylib.DrawRectangle(nx - 12, ny - 6, 24, 28, new Color((byte)80,(byte)140,(byte)220,(byte)255)); // body
    Program.DrawTextUI("GUIDE", nx - 20, ny - 40, 14, Color.Yellow);

    // a little "!" above the NPC pointing to the current task
    Program.DrawTextUI("!", nx + 14, ny - 36, 22, Color.Gold);
}

static void DrawTutorialHUD()
{
    if (!tutorialActive && tutorialMessageTimer <= 0) return;

    // current instruction
    if (tutorialActive && tutorialStep < tutorialTasks.Count)
    {
        var task = tutorialTasks[tutorialStep];
        Raylib.DrawRectangle(420, 90, 570, 80, new Color((byte)0,(byte)0,(byte)0,(byte)190));
        Raylib.DrawRectangleLines(420, 90, 570, 80, Color.Gold);
        Program.DrawTextUI($"TUTORIAL ({tutorialStep + 1}/{tutorialTasks.Count}): {task.Title}", 432, 98, 20, Color.Gold);
        Program.DrawTextUI(task.Instruction, 432, 128, 18, Color.White);
    }

    // transient completion message
    if (tutorialMessageTimer > 0)
    {
        int tw = Program.MeasureTextUI(tutorialMessage, 24);
        Raylib.DrawRectangle(ScreenWidth / 2 - tw / 2 - 14, 200, tw + 28, 40, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Program.DrawTextUI(tutorialMessage, ScreenWidth / 2 - tw / 2, 208, 24, Color.Yellow);
    }
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

            if (pos.Y >= -245020 && pos.Y <= -244820) return true; // ring road top
            if (pos.Y >= 245000 && pos.Y <= 245200) return true;   // ring road bottom
            if (pos.X >= -245000 && pos.X <= -244820) return true;  // ring road left
            if (pos.X >= 244820 && pos.X <= 245000) return true;    // ring road right

            // snow vertical connectors - scoped to Y range between side road and ring road
            if (pos.X >= -20015 && pos.X <= -19865 && (pos.Y <= 188 || pos.Y >= 333)) return true;
            if (pos.X >= -10015 && pos.X <= -9865 && (pos.Y <= 188 || pos.Y >= 333)) return true;

            // desert vertical connectors - scoped to Y range
            if (pos.X >= 14985 && pos.X <= 15135 && (pos.Y <= 188 || pos.Y >= 333)) return true;
            if (pos.X >= 24985 && pos.X <= 25135 && (pos.Y <= 188 || pos.Y >= 333)) return true;
            if (RoadManager.IsOnFootprint(pos)) return true;

            return false;
        }

static void DrawBiomeGroundDetail()
{
    float vL = camera.Target.X - ScreenWidth;
    float vR = camera.Target.X + ScreenWidth;
    float vT = camera.Target.Y - ScreenHeight;
    float vB = camera.Target.Y + ScreenHeight;

    // tile size — each tile gets a few detail elements
    const int tile = 80;

    int startX = ((int)vL / tile) * tile;
    int startY = ((int)vT / tile) * tile;

    for (int tx = startX; tx < vR; tx += tile)
    {
        for (int ty = startY; ty < vB; ty += tile)
        {
            // deterministic hash from tile position (stable as camera moves)
            int h = HashTile(tx, ty);
            int h2 = HashTile(tx + 7, ty + 13);
            int h3 = HashTile(tx + 31, ty + 3);

            string biome = GetBiomeAt(tx, ty);

            switch (biome)
            {
                case "GRASSLANDS":
                case "SAFE ZONE":
                    DrawGrassDetail(tx, ty, h, h2, h3);
                    break;
                case "DESERT":
                case "OASIS":
                case "DUNES":
                case "BADLANDS":
                    DrawSandDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "BEACH":
                    DrawBeachDetail(tx, ty, h, h2, h3);
                    break;
                case "OCEAN":
                case "CORAL REEF":
                case "DEEP OCEAN":
                case "ISLANDS":
                    DrawOceanDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "SNOW ZONE":
                case "TUNDRA":
                case "FROZEN LAKE":
                case "ICE CAVES":
                    DrawSnowDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "FOREST":
                case "DARK FOREST":
                case "ENCHANTED WOODS":
                case "MUSHROOM GROVE":
                    DrawForestFloorDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "SWAMP":
                case "MANGROVE":
                case "BOG":
                case "DEAD MARSH":
                    DrawSwampDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "VOLCANO":
                case "CALDERA":
                case "ASHEN WASTES":
                case "LAVA FIELDS":
                    DrawVolcanoDetail(tx, ty, h, h2, h3, biome);
                    break;
                case "MOUNTAINS":
                case "ALPINE MEADOW":
                case "CLIFFS":
                case "CRYSTAL CAVES":
                    DrawMountainDetail(tx, ty, h, h2, h3, biome);
                    break;
            }
        }
    }
}

// ── Deterministic hash for stable patterns ──
static int HashTile(int x, int y)
{
    int n = x * 374761393 + y * 668265263;
    n = (n ^ (n >> 13)) * 1274126177;
    return n ^ (n >> 16);
}

// ── Lightweight biome lookup by coordinate (no player dependency) ──
static string GetBiomeAt(float x, float y)
{
    // exact zones
    if (x >= -3000 && x <= 4000 && y >= -1500 && y <= 2500) return "SAFE ZONE";
    if (x >= -3000 && x <= 0 && y >= -10000 && y <= -6000) return "FARM";

    // 3×3 grid
    if (x < -80000 && y < -80000)
    {
        if (x < -180000) return "TUNDRA";
        if (x >= -180000 && x < -140000 && y >= -180000 && y < -140000) return "FROZEN LAKE";
        if (x >= -200000 && x < -140000 && y >= -120000) return "ICE CAVES";
        return "SNOW ZONE";
    }
    if (x >= -80000 && x < 80000 && y < -80000)
    {
        if (x >= -80000 && x < -20000 && y >= -170000 && y < -120000) return "ALPINE MEADOW";
        if (x >= 20000 && y >= -200000 && y < -140000) return "CLIFFS";
        if (x >= -60000 && x < 20000 && y < -200000) return "CRYSTAL CAVES";
        return "MOUNTAINS";
    }
    if (x >= 80000 && y < -80000)
    {
        if (x >= 140000 && x < 200000 && y >= -200000 && y < -140000) return "CALDERA";
        if (x < 140000 && y >= -170000 && y < -110000) return "ASHEN WASTES";
        if (y >= -110000) return "LAVA FIELDS";
        return "VOLCANO";
    }
    if (x < -80000 && y >= -80000 && y < 80000)
    {
        if (x < -180000 && y >= -30000 && y < 30000) return "MANGROVE";
        if (x >= -180000 && x < -130000 && y >= -30000 && y < 30000) return "BOG";
        if (x < -170000 && y >= 30000) return "DEAD MARSH";
        return "SWAMP";
    }
    if (x >= 80000 && y >= -80000 && y < 80000)
    {
        if (x >= 140000 && x < 190000 && y >= -20000 && y < 20000) return "OASIS";
        if (x >= 200000 && y >= -50000 && y < 50000) return "DUNES";
        if (x < 160000 && y >= 30000) return "BADLANDS";
        return "DESERT";
    }
    if (x < -80000 && y >= 80000)
    {
        if (x < -170000 && y < 170000) return "DARK FOREST";
        if (x >= -170000 && y < 160000) return "ENCHANTED WOODS";
        if (x < -160000 && y >= 170000) return "MUSHROOM GROVE";
        return "FOREST";
    }
    if (y >= 80000)
    {
        if (y < 115000) return "BEACH";
        if (x >= -80000 && x < 40000 && y < 180000) return "CORAL REEF";
        if (x >= -80000 && x < 40000 && y >= 180000) return "DEEP OCEAN";
        if (x >= 80000 && x < 200000 && y >= 150000 && y < 220000) return "ISLANDS";
        return "OCEAN";
    }
    return "GRASSLANDS";
}

// ══════════════════════════════════════════════════════════════
//  BIOME-SPECIFIC GROUND DETAIL DRAWERS
// ══════════════════════════════════════════════════════════════

// ── GRASSLANDS: individual grass blades + clover tufts ──
static void DrawGrassDetail(int tx, int ty, int h, int h2, int h3)
{
    // 2-3 grass blade tufts per tile
    int bladeCount = 2 + (Math.Abs(h) % 2);
    for (int i = 0; i < bladeCount; i++)
    {
        int bx = tx + ((h + i * 17) & 0x7F) % 70 + 5;
        int by = ty + ((h2 + i * 23) & 0x7F) % 70 + 5;
        int height = 8 + (Math.Abs(h + i) % 8);
        int lean = ((h3 + i) % 7) - 3;
        byte green = (byte)(100 + Math.Abs(h + i * 31) % 80);
        Color blade = new Color((byte)30, green, (byte)20, (byte)160);

        // blade = thin line from base upward with slight lean
        Raylib.DrawLineEx(
            new Vector2(bx, by),
            new Vector2(bx + lean, by - height),
            1.5f, blade);
        // second blade slightly offset
        Raylib.DrawLineEx(
            new Vector2(bx + 3, by),
            new Vector2(bx + 3 - lean, by - height + 2),
            1.5f, blade);
    }

    // occasional clover patch (1 in 5 tiles)
    if ((h & 0xF) < 3)
    {
        int cx = tx + (Math.Abs(h2) % 50) + 15;
        int cy = ty + (Math.Abs(h3) % 50) + 15;
        Color clover = new Color((byte)50, (byte)140, (byte)40, (byte)100);
        Raylib.DrawCircle(cx, cy, 3, clover);
        Raylib.DrawCircle(cx + 4, cy - 2, 3, clover);
        Raylib.DrawCircle(cx + 2, cy + 3, 3, clover);
    }
}

// ── DESERT / DUNES / BADLANDS: sand grain + wind ripples ──
static void DrawSandDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // sand grain dots
    int grainCount = 3 + (Math.Abs(h) % 3);
    for (int i = 0; i < grainCount; i++)
    {
        int gx = tx + ((h + i * 19) & 0x7F) % 74 + 3;
        int gy = ty + ((h2 + i * 29) & 0x7F) % 74 + 3;
        byte shade = (byte)(180 + Math.Abs(h + i * 7) % 50);
        Raylib.DrawCircle(gx, gy, 1, new Color(shade, (byte)(shade - 30), (byte)(shade - 70), (byte)120));
    }

    // wind ripple lines (every ~3 tiles)
    if ((h & 0x7) < 3)
    {
        int ry = ty + (Math.Abs(h2) % 60) + 10;
        int rx = tx + 5;
        byte alpha = (byte)(60 + Math.Abs(h) % 40);
        Color ripple = sub == "DUNES"
            ? new Color((byte)170,(byte)145,(byte)60,(byte)alpha)
            : new Color((byte)190,(byte)165,(byte)90,(byte)alpha);
        // gentle wavy line
        Raylib.DrawLineEx(new Vector2(rx, ry), new Vector2(rx + 30, ry + 2), 1.5f, ripple);
        Raylib.DrawLineEx(new Vector2(rx + 30, ry + 2), new Vector2(rx + 60, ry - 1), 1.5f, ripple);
    }

    // Badlands: occasional cracked earth
    if (sub == "BADLANDS" && (h & 0xF) < 4)
    {
        int cx = tx + (Math.Abs(h3) % 40) + 20;
        int cy = ty + (Math.Abs(h2) % 40) + 20;
        Color crack = new Color((byte)100,(byte)65,(byte)30,(byte)90);
        Raylib.DrawLineEx(new Vector2(cx, cy), new Vector2(cx + 12, cy + 8), 1, crack);
        Raylib.DrawLineEx(new Vector2(cx + 12, cy + 8), new Vector2(cx + 8, cy + 20), 1, crack);
        Raylib.DrawLineEx(new Vector2(cx + 12, cy + 8), new Vector2(cx + 24, cy + 5), 1, crack);
    }

    // Oasis: tiny green tufts
    if (sub == "OASIS" && (h & 0x7) < 2)
    {
        int gx = tx + (Math.Abs(h) % 60) + 10;
        int gy = ty + (Math.Abs(h2) % 60) + 10;
        Raylib.DrawCircle(gx, gy, 3, new Color((byte)60,(byte)130,(byte)50,(byte)130));
        Raylib.DrawCircle(gx + 5, gy - 3, 2, new Color((byte)50,(byte)120,(byte)40,(byte)110));
    }
}

// ── BEACH: sand texture + footprint impressions + pebbles ──
static void DrawBeachDetail(int tx, int ty, int h, int h2, int h3)
{
    // sand grain
    int grains = 3 + (Math.Abs(h) % 3);
    for (int i = 0; i < grains; i++)
    {
        int gx = tx + ((h + i * 13) & 0x7F) % 74 + 3;
        int gy = ty + ((h2 + i * 37) & 0x7F) % 74 + 3;
        byte s = (byte)(210 + Math.Abs(h + i) % 35);
        Raylib.DrawCircle(gx, gy, 1, new Color(s, (byte)(s - 20), (byte)(s - 60), (byte)100));
    }

    // pebbles (1 in 4 tiles)
    if ((h & 0x7) < 2)
    {
        int px = tx + (Math.Abs(h3) % 50) + 15;
        int py = ty + (Math.Abs(h2) % 50) + 15;
        byte ps = (byte)(140 + Math.Abs(h) % 60);
        Raylib.DrawEllipse(px, py, 3, 2, new Color(ps, ps, (byte)(ps - 10), (byte)180));
    }

    // wet sand patches near water edge (darker)
    if ((h & 0xF) < 3)
    {
        int wx = tx + (Math.Abs(h) % 40) + 20;
        int wy = ty + (Math.Abs(h2) % 40) + 20;
        Raylib.DrawCircle(wx, wy, 6, new Color((byte)190,(byte)170,(byte)110,(byte)50));
    }
}

// ── OCEAN: gentle wave shimmer + bubbles ──
static void DrawOceanDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // wave shimmer highlights
    if ((h & 0x7) < 3)
    {
        int wx = tx + (Math.Abs(h) % 50) + 15;
        int wy = ty + (Math.Abs(h2) % 50) + 15;
        float t = (float)Raylib.GetTime();
        float shimmer = MathF.Sin(t * 2f + wx * 0.01f + wy * 0.008f);
        byte alpha = (byte)(30 + (int)(shimmer * 20));
        Raylib.DrawLineEx(
            new Vector2(wx, wy),
            new Vector2(wx + 20 + shimmer * 5, wy + 3),
            1.5f, new Color((byte)180,(byte)220,(byte)255, alpha));
    }

    // coral reef: colourful dots
    if (sub == "CORAL REEF" && (h & 0x7) < 4)
    {
        int cx = tx + (Math.Abs(h3) % 60) + 10;
        int cy = ty + (Math.Abs(h) % 60) + 10;
        byte r = (byte)(150 + Math.Abs(h2) % 100);
        byte g = (byte)(80 + Math.Abs(h) % 80);
        byte b = (byte)(100 + Math.Abs(h3) % 100);
        Raylib.DrawCircle(cx, cy, 2 + Math.Abs(h) % 3, new Color(r, g, b, (byte)130));
    }

    // deep ocean: darker shimmer
    if (sub == "DEEP OCEAN" && (h & 0xF) < 3)
    {
        int bx = tx + (Math.Abs(h2) % 60) + 10;
        int by = ty + (Math.Abs(h3) % 60) + 10;
        Raylib.DrawCircle(bx, by, 2, new Color((byte)100,(byte)160,(byte)220,(byte)50));
    }
}

// ── SNOW: snowdrift mounds + sparkle dots + frost lines ──
static void DrawSnowDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // snowdrift mounds (soft ellipses)
    if ((h & 0x7) < 3)
    {
        int dx = tx + (Math.Abs(h) % 50) + 15;
        int dy = ty + (Math.Abs(h2) % 50) + 15;
        byte white = (byte)(230 + Math.Abs(h) % 25);
        Raylib.DrawEllipse(dx, dy, 10 + Math.Abs(h3) % 8, 4 + Math.Abs(h) % 4,
            new Color(white, white, (byte)Math.Min(255, white + 5), (byte)60));
    }

    // sparkle dots (glinting ice crystals)
    if ((h2 & 0xF) < 5)
    {
        int sx = tx + (Math.Abs(h3) % 70) + 5;
        int sy = ty + (Math.Abs(h) % 70) + 5;
        float t = (float)Raylib.GetTime();
        float sparkle = MathF.Sin(t * 4f + sx * 0.1f) * 0.5f + 0.5f;
        byte sa = (byte)(80 * sparkle);
        Raylib.DrawCircle(sx, sy, 1, new Color((byte)255,(byte)255,(byte)255, sa));
    }

    // frost lines (Tundra gets more)
    if (sub == "TUNDRA" && (h & 0x3) == 0)
    {
        int fx = tx + (Math.Abs(h) % 30) + 10;
        int fy = ty + (Math.Abs(h2) % 50) + 15;
        Color frost = new Color((byte)200,(byte)220,(byte)245,(byte)50);
        Raylib.DrawLineEx(new Vector2(fx, fy), new Vector2(fx + 18, fy + 4), 1, frost);
        Raylib.DrawLineEx(new Vector2(fx + 8, fy + 2), new Vector2(fx + 14, fy - 6), 1, frost);
    }

    // Frozen Lake: ice crack lines
    if (sub == "FROZEN LAKE" && (h & 0x7) < 3)
    {
        int cx = tx + (Math.Abs(h) % 40) + 20;
        int cy = ty + (Math.Abs(h2) % 40) + 20;
        Color ice = new Color((byte)160,(byte)200,(byte)240,(byte)70);
        Raylib.DrawLineEx(new Vector2(cx, cy), new Vector2(cx + 15, cy + 10), 1, ice);
        Raylib.DrawLineEx(new Vector2(cx + 15, cy + 10), new Vector2(cx + 10, cy + 22), 1, ice);
    }
}

// ── FOREST: leaf litter + roots + undergrowth ──
static void DrawForestFloorDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // leaf litter — small coloured dots
    int leafCount = 2 + (Math.Abs(h) % 3);
    for (int i = 0; i < leafCount; i++)
    {
        int lx = tx + ((h + i * 11) & 0x7F) % 74 + 3;
        int ly = ty + ((h2 + i * 27) & 0x7F) % 74 + 3;
        bool autumn = (Math.Abs(h + i) % 4 == 0);
        Color leaf = autumn
            ? new Color((byte)(140 + Math.Abs(h) % 60),(byte)(80 + Math.Abs(h2) % 40),(byte)20,(byte)120)
            : new Color((byte)25,(byte)(60 + Math.Abs(h) % 40),(byte)18,(byte)100);
        Raylib.DrawCircle(lx, ly, 2, leaf);
    }

    // root lines across forest floor
    if ((h & 0x7) < 2)
    {
        int rx = tx + (Math.Abs(h3) % 30) + 10;
        int ry = ty + (Math.Abs(h) % 50) + 15;
        Color root = new Color((byte)55,(byte)35,(byte)15,(byte)80);
        Raylib.DrawLineEx(new Vector2(rx, ry), new Vector2(rx + 25, ry + 6), 2, root);
    }

    // Dark Forest: extra dense undergrowth
    if (sub == "DARK FOREST" && (h & 0x3) < 2)
    {
        int ux = tx + (Math.Abs(h2) % 50) + 15;
        int uy = ty + (Math.Abs(h3) % 50) + 15;
        Color dark = new Color((byte)12,(byte)40,(byte)12,(byte)100);
        Raylib.DrawCircle(ux, uy, 5, dark);
        Raylib.DrawCircle(ux + 8, uy - 3, 4, dark);
    }

    // Enchanted Woods: faint glow spots
    if (sub == "ENCHANTED WOODS" && (h2 & 0xF) < 3)
    {
        int gx = tx + (Math.Abs(h) % 60) + 10;
        int gy = ty + (Math.Abs(h2) % 60) + 10;
        float t = (float)Raylib.GetTime();
        float pulse = MathF.Sin(t * 1.5f + gx * 0.05f) * 0.5f + 0.5f;
        byte ga = (byte)(40 * pulse);
        Raylib.DrawCircle(gx, gy, 4, new Color((byte)120,(byte)220,(byte)140, ga));
    }

    // Mushroom Grove: tiny mushroom caps
    if (sub == "MUSHROOM GROVE" && (h & 0x7) < 3)
    {
        int mx = tx + (Math.Abs(h3) % 50) + 15;
        int my = ty + (Math.Abs(h) % 50) + 15;
        byte mr = (byte)(150 + Math.Abs(h2) % 100);
        byte mg = (byte)(40 + Math.Abs(h) % 60);
        // stem
        Raylib.DrawRectangle(mx, my, 2, 5, new Color((byte)200,(byte)190,(byte)170,(byte)150));
        // cap
        Raylib.DrawCircle(mx + 1, my - 1, 4, new Color(mr, mg, (byte)40, (byte)160));
    }
}

// ── SWAMP: bubbles + mud splatters + scum film ──
static void DrawSwampDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // murky water film
    if ((h & 0x3) < 2)
    {
        int wx = tx + (Math.Abs(h) % 40) + 20;
        int wy = ty + (Math.Abs(h2) % 40) + 20;
        Raylib.DrawEllipse(wx, wy, 8, 5, new Color((byte)35,(byte)55,(byte)25,(byte)60));
    }

    // mud splatters
    int mudCount = 1 + (Math.Abs(h) % 2);
    for (int i = 0; i < mudCount; i++)
    {
        int mx = tx + ((h + i * 23) & 0x7F) % 70 + 5;
        int my = ty + ((h2 + i * 17) & 0x7F) % 70 + 5;
        Raylib.DrawCircle(mx, my, 2 + Math.Abs(h3 + i) % 3,
            new Color((byte)50,(byte)40,(byte)20,(byte)80));
    }

    // bubbles (animated)
    if ((h2 & 0xF) < 3)
    {
        float t = (float)Raylib.GetTime();
        int bx = tx + (Math.Abs(h3) % 60) + 10;
        int by = ty + (Math.Abs(h) % 60) + 10;
        float bob = MathF.Sin(t * 3f + bx * 0.1f) * 2f;
        Raylib.DrawCircleLines(bx, (int)(by + bob), 2,
            new Color((byte)80,(byte)100,(byte)60,(byte)100));
    }

    // Dead Marsh: bone-white sticks
    if (sub == "DEAD MARSH" && (h & 0x7) < 2)
    {
        int sx = tx + (Math.Abs(h) % 40) + 20;
        int sy = ty + (Math.Abs(h2) % 40) + 20;
        Color bone = new Color((byte)180,(byte)175,(byte)160,(byte)90);
        Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + 10, sy - 8), 1.5f, bone);
        Raylib.DrawLineEx(new Vector2(sx + 4, sy), new Vector2(sx + 14, sy + 6), 1.5f, bone);
    }
}

// ── VOLCANO: cracked earth + embers + ash particles ──
static void DrawVolcanoDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // cracked earth lines
    if ((h & 0x3) < 2)
    {
        int cx = tx + (Math.Abs(h) % 40) + 20;
        int cy = ty + (Math.Abs(h2) % 40) + 20;
        Color crack = new Color((byte)70,(byte)35,(byte)10,(byte)120);
        Raylib.DrawLineEx(new Vector2(cx, cy), new Vector2(cx + 14, cy + 9), 1, crack);
        Raylib.DrawLineEx(new Vector2(cx + 14, cy + 9), new Vector2(cx + 22, cy + 4), 1, crack);
        Raylib.DrawLineEx(new Vector2(cx + 14, cy + 9), new Vector2(cx + 11, cy + 22), 1, crack);
    }

    // glowing embers
    if ((h2 & 0x7) < 3)
    {
        int ex = tx + (Math.Abs(h3) % 60) + 10;
        int ey = ty + (Math.Abs(h) % 60) + 10;
        float t = (float)Raylib.GetTime();
        float glow = MathF.Sin(t * 2.5f + ex * 0.05f) * 0.5f + 0.5f;
        byte er = (byte)(200 + (int)(55 * glow));
        byte eg = (byte)(60 + (int)(80 * glow));
        Raylib.DrawCircle(ex, ey, 2, new Color(er, eg, (byte)0, (byte)(100 + (int)(60 * glow))));
    }

    // ash particles (fine grey dots)
    int ashCount = 2 + (Math.Abs(h) % 2);
    for (int i = 0; i < ashCount; i++)
    {
        int ax = tx + ((h + i * 31) & 0x7F) % 74 + 3;
        int ay = ty + ((h2 + i * 19) & 0x7F) % 74 + 3;
        byte ag = (byte)(50 + Math.Abs(h + i) % 30);
        Raylib.DrawCircle(ax, ay, 1, new Color(ag, ag, ag, (byte)70));
    }

    // Caldera: intense red cracks
    if (sub == "CALDERA" && (h & 0x3) == 0)
    {
        int lx = tx + (Math.Abs(h) % 30) + 20;
        int ly = ty + (Math.Abs(h2) % 30) + 20;
        Color lava = new Color((byte)255,(byte)80,(byte)0,(byte)90);
        Raylib.DrawLineEx(new Vector2(lx, ly), new Vector2(lx + 16, ly + 10), 2, lava);
        Raylib.DrawLineEx(new Vector2(lx + 16, ly + 10), new Vector2(lx + 12, ly + 24), 2, lava);
    }

    // Lava Fields: molten pools
    if (sub == "LAVA FIELDS" && (h & 0x7) < 2)
    {
        int px = tx + (Math.Abs(h3) % 40) + 20;
        int py = ty + (Math.Abs(h) % 40) + 20;
        float t = (float)Raylib.GetTime();
        float pulse = MathF.Sin(t * 1.8f + px * 0.03f) * 0.3f + 0.7f;
        byte pa = (byte)(80 * pulse);
        Raylib.DrawEllipse(px, py, 7, 4, new Color((byte)255,(byte)120,(byte)0, pa));
    }
}

// ── MOUNTAINS: gravel dots + crag lines + lichen patches ──
static void DrawMountainDetail(int tx, int ty, int h, int h2, int h3, string sub)
{
    // gravel dots
    int gravelCount = 3 + (Math.Abs(h) % 3);
    for (int i = 0; i < gravelCount; i++)
    {
        int gx = tx + ((h + i * 17) & 0x7F) % 74 + 3;
        int gy = ty + ((h2 + i * 23) & 0x7F) % 74 + 3;
        byte gs = (byte)(70 + Math.Abs(h + i) % 40);
        Raylib.DrawCircle(gx, gy, 1 + Math.Abs(h3 + i) % 2,
            new Color(gs, (byte)(gs - 5), (byte)(gs - 10), (byte)120));
    }

    // crag/rock edge lines
    if ((h & 0x7) < 3)
    {
        int rx = tx + (Math.Abs(h) % 30) + 10;
        int ry = ty + (Math.Abs(h2) % 50) + 15;
        byte rs = (byte)(60 + Math.Abs(h3) % 30);
        Color rock = new Color(rs, (byte)(rs - 5), (byte)(rs - 8), (byte)100);
        Raylib.DrawLineEx(new Vector2(rx, ry), new Vector2(rx + 20, ry + 5), 1.5f, rock);
    }

    // Alpine Meadow: tiny wildflowers
    if (sub == "ALPINE MEADOW" && (h & 0x7) < 4)
    {
        int fx = tx + (Math.Abs(h3) % 60) + 10;
        int fy = ty + (Math.Abs(h) % 60) + 10;
        Color[] alpine = {
            new Color((byte)220,(byte)180,(byte)60,(byte)140),
            new Color((byte)180,(byte)80,(byte)180,(byte)140),
            new Color((byte)255,(byte)255,(byte)255,(byte)140)
        };
        Raylib.DrawCircle(fx, fy, 2, alpine[Math.Abs(h2) % alpine.Length]);
        // grass blade next to flower
        Raylib.DrawLineEx(new Vector2(fx + 5, fy + 2), new Vector2(fx + 7, fy - 6), 1, 
            new Color((byte)60,(byte)140,(byte)50,(byte)130));
    }

    // Crystal Caves: glowing crystal dots
    if (sub == "CRYSTAL CAVES" && (h2 & 0x7) < 4)
    {
        int cx = tx + (Math.Abs(h) % 60) + 10;
        int cy = ty + (Math.Abs(h2) % 60) + 10;
        float t = (float)Raylib.GetTime();
        float pulse = MathF.Sin(t * 2f + cx * 0.08f) * 0.5f + 0.5f;
        byte ca = (byte)(60 + (int)(80 * pulse));
        byte cr = (byte)(120 + Math.Abs(h3) % 80);
        byte cb = (byte)(180 + Math.Abs(h) % 75);
        Raylib.DrawCircle(cx, cy, 2, new Color(cr, (byte)60, cb, ca));
        // tiny crystal shard line
        Raylib.DrawLineEx(new Vector2(cx, cy), new Vector2(cx + 3, cy - 6), 1,
            new Color(cr, (byte)100, cb, (byte)(ca - 20)));
    }

    // Cliffs: strong horizontal strata lines
    if (sub == "CLIFFS" && (h & 0x3) < 2)
    {
        int sx = tx + 5;
        int sy = ty + (Math.Abs(h) % 50) + 15;
        byte ss = (byte)(55 + Math.Abs(h2) % 25);
        Raylib.DrawLineEx(new Vector2(sx, sy), new Vector2(sx + 70, sy + 2), 2,
            new Color(ss, (byte)(ss - 3), (byte)(ss - 8), (byte)80));
    }
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
    // ── DESERT patches (X 80k–250k, Y -80k–80k) ──
    for (int x = 80000; x < 249000; x += 2000)
    {
        for (int y = -79000; y < 79000; y += 2000)
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
    for (int x = 80000; x < 249000; x += 3600)
    {
        for (int y = -79000; y < 79000; y += 3600)
        {
            Vector2 pos = new Vector2(x + 150, y + 150);
            if (IsNearRoad(pos)) continue;
            byte shade = (byte)(155 + Math.Abs(x + y) % 35);
            desertRocks.Add((pos, new Color(shade, shade, (byte)Math.Max(0, shade - 25), (byte)255)));
        }
    }
 
    // ── SNOW patches (X -250k–-80k, Y -250k–-80k) ──
    for (int x = -249000; x < -81000; x += 2000)
    {
        for (int y = -249000; y < -81000; y += 2000)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            if (IsNearRoad(pos)) continue;
            float radius = 18 + (Math.Abs(x * y) % 38);
            snowPatches.Add((pos, radius));
        }
    }
 
    // SNOW rocks
    for (int x = -249000; x < -81000; x += 4000)
    {
        for (int y = -249000; y < -81000; y += 4000)
        {
            Vector2 pos = new Vector2(x + 200, y + 200);
            if (IsNearRoad(pos)) continue;
            byte shade = (byte)(135 + Math.Abs(x + y) % 35);
            snowRocks.Add((pos, new Color(shade, shade, (byte)Math.Min(255, shade + 12), (byte)255)));
        }
    }
 
    // ── FOREST patches (X -250k–-80k, Y 80k–250k) ──
    for (int x = -249000; x < -81000; x += 2200)
    {
        for (int y = 81000; y < 249000; y += 2200)
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
    for (int x = -249000; x < -81000; x += 4800)
    {
        for (int y = 81000; y < 249000; y += 4800)
        {
            Vector2 pos = new Vector2(x + 300, y + 300);
            if (IsNearRoad(pos)) continue;
            forestMushrooms.Add((pos, mushroomColors[Math.Abs(x + y) % mushroomColors.Length]));
        }
    }
 
    // ── GRASSLANDS patches (X -80k–80k, Y -80k–80k, excluding safe zone core) ──
    for (int x = -78000; x < 78000; x += 1600)
    {
        for (int y = -78000; y < 78000; y += 1600)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            if (IsNearRoad(pos)) continue;
            // skip the safe-zone / farm core area — those have their own textures
            if (pos.X >= -3500 && pos.X <= 4500 && pos.Y >= -10500 && pos.Y <= 3000) continue;
            float radius = 12 + (Math.Abs(x * y) % 24);
            byte green = (byte)(125 + Math.Abs(x + y) % 40);
            grasslandPatches.Add((pos, radius, new Color((byte)75, green, (byte)55, (byte)95)));
        }
    }
 
    // GRASSLANDS flowers
    Color[] flowerColors = { Color.Red, Color.Yellow, Color.White, Color.Pink, Color.Orange, Color.Purple };
    for (int x = -78000; x < 78000; x += 1400)
    {
        for (int y = -78000; y < 78000; y += 1400)
        {
            Vector2 pos = new Vector2(x + 50, y + 50);
            if (IsNearRoad(pos)) continue;
            if (pos.X >= -3500 && pos.X <= 4500 && pos.Y >= -10500 && pos.Y <= 3000) continue;
            grasslandFlowers.Add((pos, flowerColors[Math.Abs(x + y) % flowerColors.Length]));
        }
    }
 
    // ── BEACH sand ripples (X -80k–250k, Y 80k–115k) ──
    for (int x = -79000; x < 249000; x += 1600)
    {
        for (int y = 81000; y < 114000; y += 1600)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 20 + (Math.Abs(x * y) % 30);
            byte r = (byte)(200 + Math.Abs(x + y) % 40);
            byte g = (byte)(180 + Math.Abs(x - y) % 30);
            oceanPatches.Add((pos, radius, new Color(r, g, (byte)110, (byte)100)));
        }
    }
 
    // BEACH shells
    Color[] shellColors = {
        new Color((byte)240,(byte)220,(byte)200,(byte)255),
        new Color((byte)220,(byte)180,(byte)160,(byte)255),
        new Color((byte)200,(byte)150,(byte)130,(byte)255)
    };
    for (int x = -79000; x < 249000; x += 1400)
    {
        for (int y = 81000; y < 114000; y += 1200)
        {
            Vector2 pos = new Vector2(x + 60, y + 60);
            oceanShells.Add((pos, shellColors[Math.Abs(x + y) % shellColors.Length]));
        }
    }
 
    // OCEAN coral (Coral Reef sub-biome: X -80k–40k, Y 115k–180k)
    Color[] coralColors = {
        new Color((byte)220,(byte)90,(byte)90,(byte)200),
        new Color((byte)255,(byte)160,(byte)60,(byte)200),
        new Color((byte)180,(byte)80,(byte)180,(byte)200)
    };
    for (int x = -79000; x < 39000; x += 2400)
    {
        for (int y = 116000; y < 179000; y += 2200)
        {
            Vector2 pos = new Vector2(x + 100, y + 100);
            oceanCoral.Add((pos, coralColors[Math.Abs(x + y) % coralColors.Length]));
        }
    }
 
    // ── SWAMP murky patches (X -250k–-80k, Y -80k–80k) ──
    for (int x = -249000; x < -81000; x += 1800)
    {
        for (int y = -79000; y < 79000; y += 1800)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 25 + (Math.Abs(x * y) % 35);
            byte g = (byte)(55 + Math.Abs(x + y) % 35);
            swampPatches.Add((pos, radius, new Color((byte)30, g, (byte)25, (byte)160)));
        }
    }
 
    // SWAMP reeds
    for (int x = -249000; x < -81000; x += 1120)
    {
        for (int y = -79000; y < 79000; y += 1120)
        {
            Vector2 pos = new Vector2(x + 50, y + 50);
            Color reedColor = new Color((byte)50, (byte)90, (byte)30, (byte)220);
            swampReeds.Add((pos, reedColor));
        }
    }
 
    // SWAMP lily pads
    for (int x = -248000; x < -81000; x += 2400)
    {
        for (int y = -79000; y < 79000; y += 2400)
        {
            Vector2 pos = new Vector2(x + 120, y + 120);
            bool hasFlower = (Math.Abs(x + y) % 3 == 0);
            Color lilyColor = hasFlower
                ? new Color((byte)240,(byte)220,(byte)80,(byte)220)
                : new Color((byte)40,(byte)110,(byte)40,(byte)200);
            swampLilies.Add((pos, lilyColor));
        }
    }
 
    // ── VOLCANO lava patches (X 80k–250k, Y -250k–-80k) ──
    for (int x = 81000; x < 249000; x += 2000)
    {
        for (int y = -249000; y < -81000; y += 2000)
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
    for (int x = 81000; x < 249000; x += 4000)
    {
        for (int y = -249000; y < -81000; y += 3600)
        {
            Vector2 pos = new Vector2(x + 200, y + 200);
            lavaVents.Add((pos, new Color((byte)80,(byte)70,(byte)65,(byte)180)));
        }
    }
 
    // ── MOUNTAIN rocky patches (X -80k–80k, Y -250k–-80k) ──
    for (int x = -79000; x < 79000; x += 1680)
    {
        for (int y = -249000; y < -81000; y += 1600)
        {
            Vector2 pos = new Vector2(x + 80, y + 80);
            float radius = 10 + (Math.Abs(x * y) % 20);
            byte shade = (byte)(75 + Math.Abs(x + y) % 30);
            mountainPatches.Add((pos, radius, new Color(shade, (byte)(shade - 5), (byte)(shade - 10), (byte)255)));
        }
    }
 
    // MOUNTAIN pine trees
    for (int x = -79000; x < 79000; x += 1680)
    {
        for (int y = -249000; y < -81000; y += 1600)
        {
            if (Math.Abs(x + y) % 3 != 0) continue;
            Vector2 pos = new Vector2(x + 100, y + 100);
            mountainTrees.Add((pos, new Color((byte)25,(byte)55,(byte)30,(byte)255)));
        }
    }
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

static void DrawIncubatorMenu()
{
    if (!incubatorMenuOpen) return;

    int pw = 460, ph = 320;
    int px = ScreenWidth / 2 - pw / 2, py = ScreenHeight / 2 - ph / 2;
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)120));
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)24,(byte)24,(byte)34,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, Color.Gold);
    Program.DrawTextUI("INCUBATOR", px + 20, py + 18, 26, Color.Gold);

    Program.DrawTextUI("[Q] Close", px + pw - 110, py + 22, 16, Color.LightGray);

    if (Raylib.IsKeyPressed(KeyboardKey.Q))
    {
        incubatorMenuOpen = false;
        return;
    }

    if (incubatingEgg != null)
    {
        // currently incubating: show egg + progress
        DrawEggIcon(incubatingEgg, px + 90, py + 150, 70);
        Program.DrawTextUI(incubatingEgg, px + 150, py + 90, 22, Color.White);

        float prog = incubationNeeded > 0 ? incubationProgress / incubationNeeded : 0f;
        prog = Math.Clamp(prog, 0f, 1f);
        int barX = px + 150, barY = py + 140, barW = 260;
        Raylib.DrawRectangle(barX, barY, barW, 22, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(barX, barY, (int)(barW * prog), 22, new Color((byte)255,(byte)160,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(barX, barY, barW, 22, Color.White);
        Program.DrawTextUI($"{(int)(prog * 100)}%  —  hatches after one full day",
            barX, barY + 30, 16, Color.LightGray);
    }
    else
    {
        Program.DrawTextUI("Select an egg to incubate:", px + 20, py + 60, 18, Color.LightGray);

        // list eggs found in the toolbar
        int row = 0;
        for (int s = 0; s < toolbarSlots.Length; s++)
        {
            if (toolbarSlots[s] == null || !IsEgg(toolbarSlots[s])) continue;
            string egg = toolbarSlots[s];
            int ry = py + 95 + row * 56;
            Rectangle r = new Rectangle(px + 20, ry, pw - 40, 48);
            bool hov = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r);
            Raylib.DrawRectangleRec(r, hov ? new Color((byte)60,(byte)60,(byte)80,(byte)255)
                                           : new Color((byte)40,(byte)40,(byte)55,(byte)255));
            Raylib.DrawRectangleLinesEx(r, 2, hov ? Color.Gold : Color.DarkGray);
            DrawEggIcon(egg, px + 48, ry + 24, 34);
            Program.DrawTextUI($"{egg}  x{toolbarCounts[s]}", px + 78, ry + 14, 20, Color.White);
            Program.DrawTextUI("[Incubate]", px + pw - 130, ry + 16, 18, Color.Gold);

            if (hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                StartIncubation(egg);
                toolbarCounts[s]--;
                if (toolbarCounts[s] <= 0) toolbarSlots[s] = null;
            }
            row++;
        }
        if (row == 0)
            Program.DrawTextUI("No eggs. Defeat a world boss for a chance at one.",
                px + 20, py + 110, 18, Color.Gray);
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

        static string GetSeasonString()
        {
            int s = monthToSeason[currentMonth];
            return seasons[Math.Clamp(s, 0, seasons.Length - 1)];
        }

        static string GetCurrentBiome()
{
    float x = player.Position.X;
    float y = player.Position.Y;

    // ── Exact zones inside grasslands (checked first) ──
    if (x >= -3000 && x <= 4000 && y >= -1500 && y <= 2500)
        return "SAFE ZONE";
    if (x >= -3000 && x <= 0 && y >= -10000 && y <= -6000)
        return "FARM";
    if (x >= 11800 && x <= 18200 && y >= 3000 && y <= 8200)
        return "HAMILTRON CITY";
    if (x >= -18000 && x <= -13800 && y >= 3200 && y <= 6200)
        return "ROTOAIRA";

    // ── 3×3 grid sectors ──
    // NW: Snow (X -250k to -80k, Y -250k to -80k)
    if (x < -80000 && y < -80000)
    {
        if (x < -180000) return "TUNDRA";
        if (x >= -180000 && x < -140000 && y >= -180000 && y < -140000) return "FROZEN LAKE";
        if (x >= -200000 && x < -140000 && y >= -120000) return "ICE CAVES";
        return "SNOW ZONE";
    }
    // N: Mountains (X -80k to 80k, Y -250k to -80k)
    if (x >= -80000 && x < 80000 && y < -80000)
    {
        if (x >= -80000 && x < -20000 && y >= -170000 && y < -120000) return "ALPINE MEADOW";
        if (x >= 20000 && y >= -200000 && y < -140000) return "CLIFFS";
        if (x >= -60000 && x < 20000 && y < -200000) return "CRYSTAL CAVES";
        return "MOUNTAINS";
    }
    // NE: Volcano (X 80k to 250k, Y -250k to -80k)
    if (x >= 80000 && y < -80000)
    {
        if (x >= 140000 && x < 200000 && y >= -200000 && y < -140000) return "CALDERA";
        if (x < 140000 && y >= -170000 && y < -110000) return "ASHEN WASTES";
        if (y >= -110000) return "LAVA FIELDS";
        return "VOLCANO";
    }
    // W: Swamp (X -250k to -80k, Y -80k to 80k)
    if (x < -80000 && y >= -80000 && y < 80000)
    {
        if (x < -180000 && y >= -30000 && y < 30000) return "MANGROVE";
        if (x >= -180000 && x < -130000 && y >= -30000 && y < 30000) return "BOG";
        if (x < -170000 && y >= 30000) return "DEAD MARSH";
        return "SWAMP";
    }
    // E: Desert (X 80k to 250k, Y -80k to 80k)
    if (x >= 80000 && y >= -80000 && y < 80000)
    {
        if (x >= 140000 && x < 190000 && y >= -20000 && y < 20000) return "OASIS";
        if (x >= 200000 && y >= -50000 && y < 50000) return "DUNES";
        if (x < 160000 && y >= 30000) return "BADLANDS";
        return "DESERT";
    }
    // SW: Forest (X -250k to -80k, Y 80k to 250k)
    if (x < -80000 && y >= 80000)
    {
        if (x < -170000 && y < 170000) return "DARK FOREST";
        if (x >= -170000 && y < 160000) return "ENCHANTED WOODS";
        if (x < -160000 && y >= 170000) return "MUSHROOM GROVE";
        return "FOREST";
    }
    // S+SE: Beach / Ocean (X -80k to 250k, Y 80k to 250k)
    if (y >= 80000)
    {
        if (y < 115000) return "BEACH";
        if (x >= -80000 && x < 40000 && y < 180000) return "CORAL REEF";
        if (x >= -80000 && x < 40000 && y >= 180000) return "DEEP OCEAN";
        if (x >= 80000 && x < 200000 && y >= 150000 && y < 220000) return "ISLANDS";
        return "OCEAN";
    }
    // C: Grasslands (fallback for X -80k to 80k, Y -80k to 80k)
    return "GRASSLANDS";
}

        static Color GetNightOverlay()
            {
                float d = GetDarkness();
                byte alpha = (byte)(185 * d);
                float hour = timeOfDay * 24f;
                // warm orange sunset tint through the first half of dusk, cooling to deep night blue
                if (hour >= 20f && hour < 22.5f)
                {
                    float t = (hour - 20f) / 2.5f;   // 0 = sunset start, 1 = fully cooled
                    return new Color((byte)(120 * (1f - t)), (byte)(45 * (1f - t)), (byte)(30 + 20 * t), alpha);
                }
                if (hour >= 4.5f && hour < 7f)       // pre-dawn indigo → pale morning
                    return new Color((byte)20, (byte)15, (byte)45, alpha);
                return new Color((byte)0, (byte)0, (byte)30, alpha);
            }

            public static float GetDarkness()  
{
    float hour = timeOfDay * 24f;
    float h = hour < 5f ? hour + 24f : hour;   
    if (h >= 20f && h < 24.5f) { float t = (h - 20f) / 4.5f; return t * t * (3f - 2f * t); }
    if (h >= 24.5f && h < 29f) return 1f;
    if (hour >= 5f && hour < 7f) { float t = 1f - (hour - 5f) / 2f; return t * t * (3f - 2f * t); }
    return 0f;  
}

 static void UpdateWeather(float dt)
{
    rainTimer += dt;

    float interval = isRaining ? rainDuration : rainInterval;

    if (rainTimer >= interval)
    {
        rainTimer = 0f;
        isRaining = !isRaining;

        if (multiplayer.Connected && !multiplayer.IsHost)
        {
            if (isRaining)
                for (int i = 0; i < raindrops.Count; i++) { /* ... existing drop-move code ... */ }
            return;
        }
    
        if (isRaining)
        {
            // just started raining — pick how long this rain lasts
            rainDuration = Raylib.GetRandomValue(15, 45);   // seconds of rain
            raindrops.Clear();
            for (int i = 0; i < 200; i++)
            {
                raindrops.Add(new Vector2(
                    Raylib.GetRandomValue(0, ScreenWidth),
                    Raylib.GetRandomValue(0, ScreenHeight)
                ));
            }
            musicBeforeRain = currentMusic;
            SwitchMusic(musicRain);
        }
        else
        {
            // just stopped raining — pick how long until the next rain
            rainInterval = Raylib.GetRandomValue(150, 1000);  // seconds of dry weather
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
    }
}
