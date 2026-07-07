using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
      static void SaveGame()
{
    var d = new List<string>();
    void S(string k, object v) => d.Add(k + "=" + v.ToString());
    void SF(string k, float v) => d.Add(k + "=" + v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    void SB(string k, bool v) => d.Add(k + "=" + (v ? "1" : "0"));
    string NN(string s) => s ?? "empty";

    S("version", 2);

    // ── Position & core ──
    SF("pos.x", player.Position.X); SF("pos.y", player.Position.Y);
    S("money", player.Money); S("bank", player.BankBalance);

    // ── Resources ──
    S("logs", player.Logs); S("birchLogs", player.BirchLogs); S("oakLogs", player.OakLogs);
    S("pineLogs", player.PineLogs); S("arcticLogs", player.ArcticLogs); S("deadWood", player.DeadWood);
    S("fish", player.Fish); S("bones", player.Bones); S("fur", player.Fur);
    S("stingers", player.Stingers); S("bearPelts", player.BearPelts);
    S("stoneOre", player.StoneOre); S("copperOre", player.CopperOre); S("ironOre", player.IronOre);
    S("goldOre", player.GoldOre); S("crystals", player.Crystals);

    // ── Skills ──
    S("woodcutLv", player.WoodcuttingLevel); S("woodcutXp", player.WoodcuttingXP);
    S("fishLv", player.FishingLevel); S("fishXp", player.FishingXP);
    S("combatLv", player.CombatLevel); S("combatXp", player.CombatXP);
    S("oneHandLv", player.OneHandMeleeLevel); S("oneHandXp", player.OneHandMeleeXP);
    S("twoHandLv", player.TwoHandMeleeLevel); S("twoHandXp", player.TwoHandMeleeXP);
    S("strengthLv", player.StrengthLevel); S("strengthXp", player.StrengthXP);
    S("drivingLv", player.DrivingLevel); S("drivingXp", player.DrivingXP);
    S("athleticsLv", player.AthleticsLevel); S("athleticsXp", player.AthleticsXP);
    S("gamblingLv", player.GamblingLevel); S("gamblingXp", player.GamblingXP);
    S("ridingLv", player.RidingLevel); S("ridingXp", player.RidingXP);
    S("cyclingLv", player.CyclingLevel); S("cyclingXp", player.CyclingXP);
    S("miningLv", player.MiningLevel); S("miningXp", player.MiningXP);
    S("swimmingLv", player.SwimmingLevel); S("swimmingXp", player.SwimmingXP);
    S("divingLv", player.DivingLevel); S("divingXp", player.DivingXP);
    S("sportsLv", player.SportsLevel); S("sportsXp", player.SportsXP);
    S("rangedLv", player.RangedLevel); S("rangedXp", player.RangedXP);
    S("elementalLv", player.ElementalLevel); S("elementalXp", player.ElementalXP);
    S("cookingLv", player.CookingLevel); S("cookingXp", player.CookingXP);
    S("farmingLv", player.FarmingLevel); S("farmingXp", player.FarmingXP);
    S("craftingLv", player.CraftingLevel); S("craftingXp", player.CraftingXP);
    S("blacksmithLv", player.BlacksmithLevel); S("blacksmithXp", player.BlacksmithXP);
    S("enchantingLv", player.EnchantingLevel); S("enchantingXp", player.EnchantingXP);
    SF("food", player.Food); SF("thirst", player.Thirst); SF("stamina", player.Stamina);
    S("educationLv", player.EducationLevel); S("educationXp", player.EducationXP);
    S("staminaLv", player.StaminaLevel); S("staminaXp", player.StaminaXP);
    S("faithLv", player.FaithLevel); S("faithXp", player.FaithXP);
    S("mysticalLv", player.MysticalLevel); S("mysticalXp", player.MysticalXP); 
    S("darkArtsLv", player.DarkArtsLevel); S("darkArtsXp", player.DarkArtsXP);

    // Ratings
    S("mathsRating", player.MathsRating); 

    // ── Health & identity ──
    S("health", player.Health); S("maxHealth", player.MaxHealth); S("name", playerName);

    // ── Appearance ──
    S("shirtR", player.ShirtColor.R); S("shirtG", player.ShirtColor.G); S("shirtB", player.ShirtColor.B);
    S("skinR", player.SkinColor.R); S("skinG", player.SkinColor.G); S("skinB", player.SkinColor.B);
    S("pantsR", player.PantsColor.R); S("pantsG", player.PantsColor.G); S("pantsB", player.PantsColor.B);
    S("hairStyle", playerHairStyle ?? "None");
    S("hairR", playerHairColor.R); S("hairG", playerHairColor.G); S("hairB", playerHairColor.B);
    S("facialHair", playerFacialHair ?? "None");
    S("facialR", playerFacialHairColor.R); S("facialG", playerFacialHairColor.G); S("facialB", playerFacialHairColor.B);

    // ── Crafting save ──
     S("placedChests.count", placedChests.Count);

    S("workbench.count", placedWorkbenches.Count);
    for (int i = 0; i < placedWorkbenches.Count; i++)
    { SF("workbench." + i + ".x", placedWorkbenches[i].X); SF("workbench." + i + ".y", placedWorkbenches[i].Y); }

    foreach (var kv in stationProps)
    {
        string key = kv.Key.Replace(" ", "");
        S(key + ".count", kv.Value.Count);
        for (int i = 0; i < kv.Value.Count; i++)
        { SF(key + "." + i + ".x", kv.Value[i].X); SF(key + "." + i + ".y", kv.Value[i].Y); }
    }

    S("furnace.count", placedFurnaces.Count);
    for (int i = 0; i < placedFurnaces.Count; i++)
    { SF("furnace." + i + ".x", placedFurnaces[i].X); SF("furnace." + i + ".y", placedFurnaces[i].Y); }

    S("flag.count", placedFlags.Count);
    for (int i = 0; i < placedFlags.Count; i++)
    { SF("flag." + i + ".x", placedFlags[i].X); SF("flag." + i + ".y", placedFlags[i].Y); }

    S("placedCampfire.count", Math.Max(0, campfirePositions.Count - builtinCampfireCount));
    for (int i = builtinCampfireCount; i < campfirePositions.Count; i++)
    { SF("placedCampfire." + (i - builtinCampfireCount) + ".x", campfirePositions[i].X);
    SF("placedCampfire." + (i - builtinCampfireCount) + ".y", campfirePositions[i].Y); }

    for (int i = 0; i < placedChests.Count; i++)
    {
        var c = placedChests[i];
        S("chest." + i + ".id", c.Id);
        SF("chest." + i + ".x", c.Position.X); SF("chest." + i + ".y", c.Position.Y);
        S("chest." + i + ".building", c.BuildingContext ?? "");
        S("chest." + i + ".tier", c.Tier);
        S("chest." + i + ".itemCount", c.Contents.Count);
        int ci = 0;
        foreach (var kv in c.Contents) { S("chest." + i + ".item." + ci + ".k", kv.Key); S("chest." + i + ".item." + ci + ".v", kv.Value); ci++; }
    }

    // Collectables
    foreach (var c in collectables) SB("collect." + c.Id, c.Found);

    // Plushies
    for (int i = 0; i < 10; i++) { S("plushStock." + i, dailyPlushStock[i] ?? ""); SB("plushTaken." + i, dailyPlushTaken[i]); }
    S("plushOwned.count", plushiesOwned.Count);
    int pli = 0;
    foreach (var kv in plushiesOwned) { S("plushOwned." + pli + ".k", kv.Key); S("plushOwned." + pli + ".v", kv.Value); pli++; }

    // Jobs
    for (int i = 0; i < jobBoard.Count; i++) SB("job." + i + ".done", jobBoard[i].CompletedToday);
    S("lastJobResetDay", lastJobResetDay);

    // Tasks
    S("sideTask.active", activeSideTask != null ? billboardTasks.IndexOf(activeSideTask) : -1);
    for (int i = 0; i < billboardTasks.Count; i++)
    { SB("sideTask." + i + ".done", billboardTasks[i].DoneToday); SB("sideTask." + i + ".ready", billboardTasks[i].ReadyToDeliver);
    S("sideTask." + i + ".base", billboardTasks[i].Baseline); }

    // Relationships
    foreach (var f in friendNPCs)
    { S("friend." + f.Name, f.Friendship); SB("friend." + f.Name + ".talked", f.TalkedToday);
    SB("friend." + f.Name + ".gifted", f.GiftedToday); SB("friend." + f.Name + ".reward", f.RewardGiven); }

    // Banking
    S("bankSigned", bankSignedUp ? 1 : 0); S("bankBal", bankBalance);
    S("bankTier", bankCardTier); S("bankSpent", cardSpentToday);
    S("bankCardState", bankCardDelivered ? 3 : bankCardMailWaiting ? 2 : bankCardPending ? 1 : 0);
    
    // ── Player loot materials ──
    S("dogFangs", player.DogFangs); S("wolfClaws", player.WolfClaws); S("venomSacs", player.VenomSacs);
    S("crabClaws", player.CrabClaws); S("bearClaws", player.BearClaws); S("crabShells", player.CrabShells);
    S("sharkFins", player.SharkFins); S("sharkTeeth", player.SharkTeeth); S("snakeSkins", player.SnakeSkins);
    S("snakeFangs", player.SnakeFangs); S("crocScales", player.CrocScales); S("crocTeeth", player.CrocTeeth);
    S("lizardScales", player.LizardScales); S("emberStones", player.EmberStones); S("magmaShards", player.MagmaShards);
    S("lavaCores", player.LavaCores); S("feathers", player.Feathers); S("eagleTalons", player.EagleTalons);
    S("horns", player.Horns); S("goatHooves", player.GoatHooves);

    // ── Quests ──
    S("quests.count", quests.Count);
    for (int i = 0; i < quests.Count; i++)
    { S("quest." + i + ".prog", quests[i].Progress); SB("quest." + i + ".done", quests[i].Completed); }
    S("wolvesKilled", wolvesKilled); S("cropsHarvested", cropsHarvested); S("mealsCooked", mealsCooked);
    for (int i = 0; i < storyQuests.Count; i++)

    { SB("story." + i + ".started", storyQuests[i].Started); SB("story." + i + ".done", storyQuests[i].Completed);
    S("story." + i + ".stage", storyQuests[i].Stage); S("story." + i + ".base", storyQuests[i].Current?.Baseline ?? 0); }

    // ── World / time ──
    SF("playtime", totalPlayTime);
    S("dayOfMonth", dayOfMonth); S("currentMonth", currentMonth);
    SF("timeOfDay", timeOfDay); S("dayOfWeek", dayOfWeek); S("tickets", player.Tickets);

    // ── Tool pickup flags ──
    SB("axePicked", axePickedUp); SB("pickaxePicked", pickaxePickedUp);
    SB("rodPicked", fishingRodPickedUp); SB("netPicked", fishingNetPickedUp);
    SB("torchPicked", torchPickedUp); SB("hasAxe", player.HasAxe);

    // ── Weapons / ammo ──
    S("arrows", player.Arrows); S("bolts", player.Bolts);
    SB("hasBow", player.HasBow); SB("hasCrossbow", player.HasCrossbow);
    S("equipped1H", NN(equipped1H)); S("equipped2H", NN(equipped2H)); S("equippedAmmo", NN(equippedAmmo));

    // ── Elemental / staff ──
    S("arcaneEssence", player.ArcaneEssence); S("equippedStaff", NN(player.EquippedStaff));

    // ── Armor ──
    S("armorHelmet", NN(armorHelmet)); S("armorBody", NN(armorBody)); S("armorLegs", NN(armorLegs));
    S("armorBoots", NN(armorBoots)); S("armorGloves", NN(armorGloves)); S("armorCape", NN(armorCape));
    S("armorShield", NN(armorShield));

    // ── Licences ──
    SB("theoryD", hasTheoryD); SB("theoryC", hasTheoryC); SB("theoryB", hasTheoryB);
    SB("theoryA", hasTheoryA); SB("theoryS", hasTheoryS);
    SB("pracD", hasPracticalD); SB("pracC", hasPracticalC); SB("pracB", hasPracticalB);
    SB("pracA", hasPracticalA); SB("pracS", hasPracticalS);

    // Boating
    S("boatingLv", player.BoatingLevel); S("boatingXp", player.BoatingXP);
    SB("hasBoatTheory", hasBoatTheory);
    for (int i = 0; i < hasBoatPractical.Length; i++)
        SB("boatPractical." + i, hasBoatPractical[i]);

    // Mailboxes
    for (int i = 0; i < 5; i++)
{
    SB("licPending." + i, licencePending[i]);
    SF("licTimer." + i, licenceDeliveryTimer[i]);
    SB("licWaiting." + i, licenceMailWaiting[i]);
    SB("licTheoryDelivered." + i, licenceTheoryDelivered[i]);       // was: licDelivered
    SB("licPracticalDelivered." + i, licencePracticalDelivered[i]); // ADDED
    SB("licPendingIsTheory." + i, licencePendingIsTheory[i]);       // ADDED
    S("licHouse." + i, licenceTargetHouse[i]);
    }

    // TradingCards
for (int s = 0; s < cardSets.Count; s++)
{
    var set = cardSets[s];
    S("set." + s + ".mb.count", set.MasterSet.Cards.Count);
    int mbi = 0;
    foreach (var kv in set.MasterSet.Cards) { S("set." + s + ".mb." + mbi + ".k", kv.Key); S("set." + s + ".mb." + mbi + ".v", kv.Value); mbi++; }
    S("set." + s + ".mbr.count", set.MasterSet.ReverseHoloCards.Count);
    S("set." + s + ".packs", set.PacksOpened);

    int mbri = 0;
    foreach (var kv in set.MasterSet.ReverseHoloCards) { S("set." + s + ".mbr." + mbri + ".k", kv.Key); S("set." + s + ".mbr." + mbri + ".v", kv.Value); mbri++; }

    S("set." + s + ".ps.count", set.Personal.SlotAssignments.Count);
    int psi = 0;
    foreach (var kv in set.Personal.SlotAssignments)
    {
        S("set." + s + ".ps." + psi + ".slot", kv.Key);
        S("set." + s + ".ps." + psi + ".name", kv.Value.Name);
        SB("set." + s + ".ps." + psi + ".rev", kv.Value.IsReverse);
        S("set." + s + ".ps." + psi + ".count", kv.Value.Count);
        psi++;
    }
}


    // Prison
    SF("playerDebt", playerDebt);
    SF("debtDueTimer", debtDueTimer);
    SB("inPrison", inPrison);
    SF("prisonSentenceTimer", prisonSentenceTimer);
    SF("prisonReturnPos.x", prisonReturnPos.X); SF("prisonReturnPos.y", prisonReturnPos.Y);
    S("currentPrisonRoom", currentPrisonRoom ?? "");

    // School
    S("currentClassroom", currentClassroom ?? "");

    // Mall
    S("currentMiniShop", currentMiniShop ?? "");

    // Garages
    S("garages.count", garages.Count);
    for (int i = 0; i < garages.Count; i++)
    {
        SF("garage." + i + ".x", garages[i].Bounds.X); SF("garage." + i + ".y", garages[i].Bounds.Y);
        S("garage." + i + ".owner", garages[i].OwnerHouseIndex);
        S("garage." + i + ".cap", garages[i].Capacity);
        SB("garage." + i + ".dock", garages[i].IsDock);
        S("garage." + i + ".slots", garages[i].Slots.Count);
        for (int j = 0; j < garages[i].Slots.Count; j++)
        {
            var (t, c) = garages[i].Slots[j];
            S("garage." + i + ".slot." + j, $"{t}|{c.R}|{c.G}|{c.B}");
        }
    }

    // Stables
    S("stables.count", stables.Count);
    for (int i = 0; i < stables.Count; i++)
    {
        SF("stable." + i + ".x", stables[i].Bounds.X); SF("stable." + i + ".y", stables[i].Bounds.Y);
        SF("stable." + i + ".w", stables[i].Bounds.Width); SF("stable." + i + ".h", stables[i].Bounds.Height);
        S("stable." + i + ".owner", stables[i].OwnerHouseIndex);
        S("stable." + i + ".kind", stables[i].Kind.ToString());
        S("stable." + i + ".cap", stables[i].Capacity);
        S("stable." + i + ".slots", stables[i].Slots.Count);
        for (int j = 0; j < stables[i].Slots.Count; j++)
        {
            var (t, c) = stables[i].Slots[j];
            S("stable." + i + ".slot." + j, $"{t}|{c.R}|{c.G}|{c.B}");
        }
    }

    // Stats
    S("dungeonsCleared", dungeonsCleared);
    S("timesCheated", timesCheated);
    S("sportCounts.count", sportPlayCounts.Count);
    int spi = 0;
    foreach (var kv in sportPlayCounts) { S("sportCount." + spi + ".k", kv.Key); S("sportCount." + spi + ".v", kv.Value); spi++; }
    S("miniCounts.count", minigamePlayCounts.Count);
    int mpi = 0;
    foreach (var kv in minigamePlayCounts) { S("miniCount." + mpi + ".k", kv.Key); S("miniCount." + mpi + ".v", kv.Value); mpi++; }

    // PLayer id cards
    S("idClaimed", idClaimed ? 1 : 0);
    S("idPending", idPending ? 1 : 0);
    S("idMailWaiting", idMailWaiting ? 1 : 0);
    SF("idDeliveryTimer", idDeliveryTimer);
    S("idIssuedDate", idIssuedDate);
    S("idTargetHouseIndex", idTargetHouseIndex);
    S("hasDropzoneCard", hasDropzoneCard ? "1" : "0");
    SF("dropzoneCredit", dropzoneCredit);
    SF("dropzoneLifetimeSpend", dropzoneLifetimeSpend);
    S("dropzoneTier", dropzoneTier);

    // ── Tutorial state ──
    SB("tutDone", tutorialCompleted); SB("tutActive", tutorialActive); S("tutStep", tutorialStep);
    S("tutTaskCount", tutorialTasks.Count);
    for (int i = 0; i < tutorialTasks.Count; i++) SB("tutTask." + i, tutorialTasks[i].Done);

    // ── Cards ──
    S("cardsLv", player.PlayingCardsLevel); S("cardsXp", player.PlayingCardsXP);
    S("euchreRating", player.EuchreRating); S("fiveHundredRating", player.FiveHundredRating);
    S("sequenceRating", player.SequenceRating); S("euchreWins", player.EuchreWins);
    S("fiveHundredWins", player.FiveHundredWins); S("sequenceWins", player.SequenceWins);

    // Farming
    S("farmPlots.count", farmPlots.Count);
    for (int i = 0; i < farmPlots.Count; i++)
    {
    var p = farmPlots[i];
    SF("farmPlot." + i + ".x", p.Position.X); SF("farmPlot." + i + ".y", p.Position.Y);
    SB("farmPlot." + i + ".tilled", p.Tilled);
    SB("farmPlot." + i + ".planted", p.Planted);
    SB("farmPlot." + i + ".watered", p.Watered);
    S("farmPlot." + i + ".crop", p.CropType ?? "");
    SF("farmPlot." + i + ".growTimer", p.GrowTimer);
    SF("farmPlot." + i + ".growDuration", p.GrowDuration);
    SB("farmPlot." + i + ".ready", p.ReadyToHarvest);
    }

    S("fruitTrees.count", fruitTrees.Count);
    for (int i = 0; i < fruitTrees.Count; i++)
    {
    var t = fruitTrees[i];
    SF("fruitTree." + i + ".x", t.Position.X); SF("fruitTree." + i + ".y", t.Position.Y);
    SB("fruitTree." + i + ".tilled", t.Tilled);
    SB("fruitTree." + i + ".planted", t.Planted);
    S("fruitTree." + i + ".type", t.FruitType ?? "");
    SF("fruitTree." + i + ".growTimer", t.GrowTimer);
    SF("fruitTree." + i + ".growDuration", t.GrowDuration);
    SB("fruitTree." + i + ".ready", t.ReadyToHarvest);
    SF("fruitTree." + i + ".regrowTimer", t.RegrowTimer);
}

    SB("spadePickedUp", spadePickedUp);
    SB("wateringCanPickedUp", wateringCanPickedUp);
    SB("wheatSeedsPickedUp", wheatSeedsPickedUp);
    for (int i = 0; i < 8; i++) SF("toolbarWaterCharge." + i, toolbarWaterCharge[i]);

    // Livestock save
    S("livestockPens.count", livestockPens.Count);
for (int i = 0; i < livestockPens.Count; i++)
{
    var pen = livestockPens[i];
    SF("pen." + i + ".x", pen.Position.X); SF("pen." + i + ".y", pen.Position.Y);
    S("pen." + i + ".animal", pen.Animal); S("pen." + i + ".produce", pen.Produce);
    S("pen." + i + ".feed", pen.Feed); SF("pen." + i + ".cycle", pen.Cycle);
    SF("pen." + i + ".timer", pen.Timer); SB("pen." + i + ".fed", pen.Fed);
    SB("pen." + i + ".ready", pen.ReadyToHarvest);
}

    // ── Gear ──
    S("gear.count", player.OwnedGear.Count);
    for (int i = 0; i < player.OwnedGear.Count; i++) S("gear." + i, player.OwnedGear[i]);

    // ── Toolbar ──
    for (int i = 0; i < 8; i++) { S("toolbar." + i + ".item", NN(toolbarSlots[i])); S("toolbar." + i + ".count", toolbarCounts[i]); }

    // ── Backpack ──
    S("backpack.count", backpack.Count);
    int bi = 0;
    foreach (var kv in backpack) { S("backpack." + bi + ".key", kv.Key); S("backpack." + bi + ".val", kv.Value); bi++; }

    // ── Houses ──
    S("houses.count", ownedHousePlots.Count);
    for (int i = 0; i < ownedHousePlots.Count; i++)
    {
        S("house." + i + ".x", ownedHousePlots[i].x);
        S("house." + i + ".y", ownedHousePlots[i].y);
        var hd = i < houseDataList.Count ? houseDataList[i] : new HouseData(0, 0);
        S("house." + i + ".wall", hd.WallColor);
        S("house." + i + ".floor", hd.FloorColor);
        S("house." + i + ".type", hd.HouseType);
        S("house." + i + ".furnCount", hd.Furniture.Count);
        for (int j = 0; j < hd.Furniture.Count; j++)
        {
            var f = hd.Furniture[j];
            S("house." + i + ".furn." + j, $"{f.Type}|{f.RoomX}|{f.RoomY}|{f.Cost}");
        }
    }

    // ── Pets ──
    S("activePet.adultType", activePet != null ? activePet.AdultType : "");
    SF("activePet.age", activePet != null ? activePet.AgeTimer : 0f);
    S("eggs.count", ownedEggs.Count);
    for (int i = 0; i < ownedEggs.Count; i++) S("egg." + i, ownedEggs[i]);
    S("incubatingEgg", NN(incubatingEgg));
    SF("incubationProgress", incubationProgress);
    S("activePet.type", activePet != null ? activePet.Type : "empty");
    SF("activePet.x", activePet != null ? activePet.Position.X : 0f);
    SF("activePet.y", activePet != null ? activePet.Position.Y : 0f);
    S("pendingPet.type", pendingPet != null ? pendingPet.Type : "empty");
    SF("pendingPet.x", pendingPet != null ? pendingPet.Position.X : 0f);
    SF("pendingPet.y", pendingPet != null ? pendingPet.Position.Y : 0f);
    S("activePet.adultRole", activePet != null ? activePet.AdultRole : "");
    SF("activePet.bond",     activePet != null ? activePet.Bond : 0);
    SF("activePet.str",      activePet != null ? activePet.StrengthXP : 0);
    SF("activePet.mag",      activePet != null ? activePet.MagicXP : 0);
    SF("activePet.soc",      activePet != null ? activePet.SocialXP : 0);
    S("activePet.adopted",   activePet != null && activePet.Adopted ? "1" : "0");
    S("storedPets.count", storedPets.Count);
    for (int i = 0; i < storedPets.Count; i++) S("storedPet." + i, storedPets[i]);

    System.IO.File.WriteAllLines(savePath, d);
    ShowNotification("Game Saved!");
}

static void LoadGame()
{
    if (!System.IO.File.Exists(savePath)) return;
    string[] lines = System.IO.File.ReadAllLines(savePath);

    var map = new Dictionary<string, string>();
    foreach (var line in lines)
    {
        int eq = line.IndexOf('=');
        if (eq <= 0) continue;
        map[line.Substring(0, eq)] = line.Substring(eq + 1);
    }
    if (map.Count == 0) return;

    string GS(string k, string def = "") => map.TryGetValue(k, out var v) ? v : def;
    int GI(string k, int def = 0) => map.TryGetValue(k, out var v) && int.TryParse(v, out var r) ? r : def;
    float GF(string k, float def = 0f) => map.TryGetValue(k, out var v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : def;
    bool GB(string k, bool def = false) => map.TryGetValue(k, out var v) ? v == "1" : def;
    string GN(string k) { var v = GS(k, "empty"); return v == "empty" ? null : v; }

    // ── Position & core ──
    player.Position = new Vector2(GF("pos.x"), GF("pos.y"));
    player.Money = GI("money"); player.BankBalance = GI("bank");

    // ── Resources ──
    player.Logs = GI("logs"); player.BirchLogs = GI("birchLogs"); player.OakLogs = GI("oakLogs");
    player.PineLogs = GI("pineLogs"); player.ArcticLogs = GI("arcticLogs"); player.DeadWood = GI("deadWood");
    player.Fish = GI("fish"); player.Bones = GI("bones"); player.Fur = GI("fur");
    player.Stingers = GI("stingers"); player.BearPelts = GI("bearPelts");

    // ── Skills ──
    player.WoodcuttingLevel = GI("woodcutLv", 1); player.WoodcuttingXP = GI("woodcutXp");
    player.FishingLevel = GI("fishLv", 1); player.FishingXP = GI("fishXp");
    player.CombatLevel = GI("combatLv", 1); player.CombatXP = GI("combatXp");
    player.OneHandMeleeLevel = GI("oneHandLv", 1); player.OneHandMeleeXP = GI("oneHandXp");
    player.TwoHandMeleeLevel = GI("twoHandLv", 1); player.TwoHandMeleeXP = GI("twoHandXp");
    player.StrengthLevel = GI("strengthLv", 1); player.StrengthXP = GI("strengthXp");
    player.DrivingLevel = GI("drivingLv", 1); player.DrivingXP = GI("drivingXp");
    player.AthleticsLevel = GI("athleticsLv", 1); player.AthleticsXP = GI("athleticsXp");
    player.GamblingLevel = GI("gamblingLv", 1); player.GamblingXP = GI("gamblingXp");
    player.RidingLevel = GI("ridingLv", 1); player.RidingXP = GI("ridingXp");
    player.CyclingLevel = GI("cyclingLv", 1); player.CyclingXP = GI("cyclingXp");
    player.MiningLevel = GI("miningLv", 1); player.MiningXP = GI("miningXp");
    player.SwimmingLevel = GI("swimmingLv", 1); player.SwimmingXP = GI("swimmingXp");
    player.DivingLevel = GI("divingLv", 1); player.DivingXP = GI("divingXp");
    player.SportsLevel = GI("sportsLv", 1); player.SportsXP = GI("sportsXp");
    player.RangedLevel = GI("rangedLv", 1); player.RangedXP = GI("rangedXp");
    player.ElementalLevel = GI("elementalLv", 1); player.ElementalXP = GI("elementalXp");
    player.CookingLevel = GI("cookingLv", 1); player.CookingXP = GI("cookingXp");
    player.FarmingLevel = GI("farmingLv", 1); player.FarmingXP = GI("farmingXp");
    player.CraftingLevel = GI("craftingLv", 1); player.CraftingXP = GI("craftingXp");
    player.BlacksmithLevel = GI("blacksmithLv", 1); player.BlacksmithXP = GI("blacksmithXp");
    player.EnchantingLevel = GI("enchantingLv", 1); player.EnchantingXP = GI("enchantingXp");
    player.Food = GF("food", 100f); player.Thirst = GF("thirst", 100f); player.Stamina = GF("stamina", 100f);
    player.EducationLevel = GI("educationLv", 1); player.EducationXP = GI("educationXp");
    player.StaminaLevel = GI("staminaLv", 1); player.StaminaXP = GI("staminaXp");
    player.FaithLevel = GI("faithLv", 1);    player.FaithXP = GI("faithXp");
    player.MysticalLevel = GI("mysticalLv", 1); player.MysticalXP = GI("mysticalXp");
    player.DarkArtsLevel = GI("darkArtsLv", 1); player.DarkArtsXP = GI("darkArtsXp");

    // Ratings
    player.MathsRating = GI("mathsRating", 200);

    // ── Health & identity ──
    player.Health = GI("health", 100); player.MaxHealth = GI("maxHealth", 100);
    playerName = GS("name"); nameEntered = true;

    // ── Appearance ──
    player.ShirtColor = new Color((byte)GI("shirtR"), (byte)GI("shirtG"), (byte)GI("shirtB"), (byte)255);
    player.SkinColor  = new Color((byte)GI("skinR"), (byte)GI("skinG"), (byte)GI("skinB"), (byte)255);
    player.PantsColor = new Color((byte)GI("pantsR"), (byte)GI("pantsG"), (byte)GI("pantsB"), (byte)255);
    playerHairStyle = GS("hairStyle", "None");
    playerHairColor = new Color((byte)GI("hairR"), (byte)GI("hairG"), (byte)GI("hairB"), (byte)255);
    playerFacialHair = GS("facialHair", "None");
    playerFacialHairColor = new Color((byte)GI("facialR"), (byte)GI("facialG"), (byte)GI("facialB"), (byte)255);

    // Crafting load
    placedChests.Clear();
    placedWorkbenches.Clear();
    placedFurnaces.Clear();
    placedFlags.Clear();

    foreach (var kv in stationProps)
    {
        string key = kv.Key.Replace(" ", "");
        kv.Value.Clear();
        int n = GI(key + ".count");
        for (int i = 0; i < n; i++)
            kv.Value.Add(new Vector2(GF(key + "." + i + ".x"), GF(key + "." + i + ".y")));
    }

    int fuCount = GI("furnace.count");
    for (int i = 0; i < fuCount; i++)
        placedFurnaces.Add(new Vector2(GF("furnace." + i + ".x"), GF("furnace." + i + ".y")));

    int wbCount = GI("workbench.count");
    for (int i = 0; i < wbCount; i++)
        placedWorkbenches.Add(new Vector2(GF("workbench." + i + ".x"), GF("workbench." + i + ".y")));

    int flCount = GI("flag.count");
    for (int i = 0; i < flCount; i++)
        placedFlags.Add(new Vector2(GF("flag." + i + ".x"), GF("flag." + i + ".y")));

    if (builtinCampfireCount > 0 && campfirePositions.Count > builtinCampfireCount)
        campfirePositions.RemoveRange(builtinCampfireCount, campfirePositions.Count - builtinCampfireCount);
    int pcfCount = GI("placedCampfire.count");
    for (int i = 0; i < pcfCount; i++)
        campfirePositions.Add(new Vector2(GF("placedCampfire." + i + ".x"), GF("placedCampfire." + i + ".y")));

    int chestCount = GI("placedChests.count");
    for (int i = 0; i < chestCount; i++)
    {
        var c = new PlacedChest {
            Id = GS("chest." + i + ".id", "chest_" + i),
            Position = new Vector2(GF("chest." + i + ".x"), GF("chest." + i + ".y")),
            BuildingContext = GS("chest." + i + ".building", ""),
            Tier = GI("chest." + i + ".tier"),
        };
        int itemCount = GI("chest." + i + ".itemCount");
        for (int j = 0; j < itemCount; j++)
        {
            string k = GS("chest." + i + ".item." + j + ".k", "");
            if (k != "") c.Contents[k] = GI("chest." + i + ".item." + j + ".v");
        }
        placedChests.Add(c);
    }

    // Collectables
    foreach (var c in collectables) c.Found = GB("collect." + c.Id);

    // Plushies
    for (int i = 0; i < 10; i++) { dailyPlushStock[i] = GS("plushStock." + i, ""); dailyPlushTaken[i] = GB("plushTaken." + i); }
    if (string.IsNullOrEmpty(dailyPlushStock[0])) RollDailyPlushStock();
    plushiesOwned.Clear();
    int plc = GI("plushOwned.count");
    for (int i = 0; i < plc; i++) { string k = GS("plushOwned." + i + ".k", ""); if (k != "") plushiesOwned[k] = GI("plushOwned." + i + ".v"); }

    // Jobs
    for (int i = 0; i < jobBoard.Count; i++) jobBoard[i].CompletedToday = GB("job." + i + ".done");
    lastJobResetDay = GI("lastJobResetDay", -1);

    // Tasks
    for (int i = 0; i < billboardTasks.Count; i++)
    { billboardTasks[i].DoneToday = GB("sideTask." + i + ".done"); billboardTasks[i].ReadyToDeliver = GB("sideTask." + i + ".ready");
    billboardTasks[i].Baseline = GI("sideTask." + i + ".base"); }
    int atIdx = GI("sideTask.active", -1);
    if (atIdx >= 0) { activeSideTask = billboardTasks[atIdx]; activeSideTask.Accepted = true; }

    // Relationships
    foreach (var f in friendNPCs)
    { f.Friendship = GI("friend." + f.Name); f.TalkedToday = GB("friend." + f.Name + ".talked");
    f.GiftedToday = GB("friend." + f.Name + ".gifted"); f.RewardGiven = GB("friend." + f.Name + ".reward"); }

    // Banking load
    bankSignedUp = GI("bankSigned") == 1; bankBalance = GI("bankBal");
    bankCardTier = GI("bankTier"); cardSpentToday = GI("bankSpent");
    int bcs = GI("bankCardState");
    bankCardPending = bcs == 1; bankCardMailWaiting = bcs == 2; bankCardDelivered = bcs == 3;

    // Stray stat-items
     foreach (var stat in statItems)
        while (BackpackCount(stat.Name) > 0)
        { BackpackRemoveOne(stat.Name); stat.Delta(1); }

    // ── Player loot materials ──
    player.DogFangs = GI("dogFangs"); player.WolfClaws = GI("wolfClaws"); player.VenomSacs = GI("venomSacs");
    player.CrabClaws = GI("crabClaws"); player.BearClaws = GI("bearClaws"); player.CrabShells = GI("crabShells");
    player.SharkFins = GI("sharkFins"); player.SharkTeeth = GI("sharkTeeth"); player.SnakeSkins = GI("snakeSkins");
    player.SnakeFangs = GI("snakeFangs"); player.CrocScales = GI("crocScales"); player.CrocTeeth = GI("crocTeeth");
    player.LizardScales = GI("lizardScales"); player.EmberStones = GI("emberStones"); player.MagmaShards = GI("magmaShards");
    player.LavaCores = GI("lavaCores"); player.Feathers = GI("feathers"); player.EagleTalons = GI("eagleTalons");
    player.Horns = GI("horns"); player.GoatHooves = GI("goatHooves");
    player.StoneOre = GI("stoneOre"); player.CopperOre = GI("copperOre"); player.IronOre = GI("ironOre");
    player.GoldOre = GI("goldOre"); player.Crystals = GI("crystals");

    // ── Quests ──
    for (int i = 0; i < quests.Count; i++)
    {
        quests[i].Progress  = GI("quest." + i + ".prog", GI("quest" + i + "prog"));
        quests[i].Completed = GB("quest." + i + ".done", GB("quest" + i + "done"));
    }
    wolvesKilled = GI("wolvesKilled"); cropsHarvested = GI("cropsHarvested"); mealsCooked = GI("mealsCooked");

    for (int i = 0; i < storyQuests.Count; i++)
    { var q = storyQuests[i];
    q.Started = GB("story." + i + ".started"); q.Completed = GB("story." + i + ".done");
    q.Stage = GI("story." + i + ".stage");
    if (q.Current != null) q.Current.Baseline = GI("story." + i + ".base"); }

    // ── World / time ──
    totalPlayTime = GF("playtime");
    dayOfMonth = GI("dayOfMonth"); currentMonth = GI("currentMonth");
    timeOfDay = GF("timeOfDay"); dayOfWeek = GI("dayOfWeek"); player.Tickets = GI("tickets");

    // ── Tool pickup flags ──
    axePickedUp = GB("axePicked"); pickaxePickedUp = GB("pickaxePicked");
    fishingRodPickedUp = GB("rodPicked"); fishingNetPickedUp = GB("netPicked");
    torchPickedUp = GB("torchPicked"); player.HasAxe = GB("hasAxe");

    // ── Weapons / ammo ──
    player.Arrows = GI("arrows"); player.Bolts = GI("bolts");
    player.HasBow = GB("hasBow"); player.HasCrossbow = GB("hasCrossbow");
    equipped1H = GN("equipped1H"); equipped2H = GN("equipped2H"); equippedAmmo = GN("equippedAmmo");
    if (equipped2H != null) armorWeapon = equipped2H;
    else if (equipped1H != null) armorWeapon = equipped1H;

    // ── Elemental / staff ──
    player.ArcaneEssence = GI("arcaneEssence"); player.EquippedStaff = GN("equippedStaff");

    // ── Armor ──
    armorHelmet = GN("armorHelmet"); armorBody = GN("armorBody"); armorLegs = GN("armorLegs");
    armorBoots = GN("armorBoots"); armorGloves = GN("armorGloves"); armorCape = GN("armorCape");
    armorShield = GN("armorShield");

    // TradingCards
    for (int s = 0; s < cardSets.Count; s++)
{
    var set = cardSets[s];
    set.MasterSet.Cards.Clear(); set.MasterSet.ReverseHoloCards.Clear();
    set.PacksOpened = GI("set." + s + ".packs");
    int mbc = GI("set." + s + ".mb.count");
    for (int i = 0; i < mbc; i++) { var k = GS("set." + s + ".mb." + i + ".k", ""); if (k != "") set.MasterSet.Cards[k] = GI("set." + s + ".mb." + i + ".v"); }
    int mbrc = GI("set." + s + ".mbr.count");
    for (int i = 0; i < mbrc; i++) { var k = GS("set." + s + ".mbr." + i + ".k", ""); if (k != "") set.MasterSet.ReverseHoloCards[k] = GI("set." + s + ".mbr." + i + ".v"); }

    set.Personal.SlotAssignments.Clear();
    int psc = GI("set." + s + ".ps.count");
    for (int i = 0; i < psc; i++)
    {
        string name = GS("set." + s + ".ps." + i + ".name", "");
        if (name == "") continue;
        int slotIdx = GI("set." + s + ".ps." + i + ".slot");
        set.Personal.SlotAssignments[slotIdx] = new BinderSlotData
        {
            Name = name,
            IsReverse = GB("set." + s + ".ps." + i + ".rev"),
            Count = GI("set." + s + ".ps." + i + ".count")
        };
    }
}

    // ── Licences ──
    hasTheoryD = GB("theoryD"); hasTheoryC = GB("theoryC"); hasTheoryB = GB("theoryB");
    hasTheoryA = GB("theoryA"); hasTheoryS = GB("theoryS");
    hasPracticalD = GB("pracD"); hasPracticalC = GB("pracC"); hasPracticalB = GB("pracB");
    hasPracticalA = GB("pracA"); hasPracticalS = GB("pracS");

    //Boating
    player.BoatingLevel = GI("boatingLv", 1); player.BoatingXP = GI("boatingXp");
    hasBoatTheory = GB("hasBoatTheory");
    for (int i = 0; i < hasBoatPractical.Length; i++)
        hasBoatPractical[i] = GB("boatPractical." + i);

    // Mailboxes 
    for (int i = 0; i < 5; i++)
{
    licencePending[i] = GB("licPending." + i);
    licenceDeliveryTimer[i] = GF("licTimer." + i);
    licenceMailWaiting[i] = GB("licWaiting." + i);
    licenceTheoryDelivered[i] = GB("licTheoryDelivered." + i);
    licencePracticalDelivered[i] = GB("licPracticalDelivered." + i);
    licencePendingIsTheory[i] = GB("licPendingIsTheory." + i);
    licenceTargetHouse[i] = GI("licHouse." + i);
}

    // School
    currentClassroom = GS("currentClassroom", "");

    // Mall
    currentMiniShop = GS("currentMiniShop", ""); 

    // Prison
    playerDebt = GF("playerDebt");
    debtDueTimer = GF("debtDueTimer");
    inPrison = GB("inPrison");
    prisonSentenceTimer = GF("prisonSentenceTimer");
    prisonReturnPos = new Vector2(GF("prisonReturnPos.x"), GF("prisonReturnPos.y"));
    currentPrisonRoom = GS("currentPrisonRoom", "");
    if (inPrison)
    {
        player.Position = prisonCellCenter;
        ChangeScene(SceneState.World);
    }

    // Garages
    garages.Clear();
    int garageCount = GI("garages.count");
    for (int i = 0; i < garageCount; i++)
    {
        int cap = Math.Max(1, GI("garage." + i + ".cap"));
        var g = new Garage {
            Bounds = new Rectangle(GF("garage." + i + ".x"), GF("garage." + i + ".y"), 100 + cap * 60, 100),
            OwnerHouseIndex = GI("garage." + i + ".owner"),
            Capacity = cap,
            IsDock = GB("garage." + i + ".dock"),
        };
        int slotCount = GI("garage." + i + ".slots");
        for (int j = 0; j < slotCount; j++)
        {
            string[] sp = GS("garage." + i + ".slot." + j, "").Split('|');
            if (sp.Length >= 4)
                g.Slots.Add((Enum.Parse<Vehicle.VehicleType>(sp[0]),
                    new Color((byte)int.Parse(sp[1]), (byte)int.Parse(sp[2]), (byte)int.Parse(sp[3]), (byte)255)));
        }
        garages.Add(g);
    }
    
    // Stables
    stables.Clear();
    int stableCount = GI("stables.count");
    for (int i = 0; i < stableCount; i++)
    {
        var st = new Stable {
            Bounds = new Rectangle(GF("stable." + i + ".x"), GF("stable." + i + ".y"), GF("stable." + i + ".w"), GF("stable." + i + ".h")),
            OwnerHouseIndex = GI("stable." + i + ".owner"),
            Kind = Enum.Parse<Stable.StableKind>(GS("stable." + i + ".kind", "Basic")),
            Capacity = Math.Max(1, GI("stable." + i + ".cap")),
        };
        int slotCount = GI("stable." + i + ".slots");
        for (int j = 0; j < slotCount; j++)
        {
            string[] sp = GS("stable." + i + ".slot." + j, "").Split('|');
            if (sp.Length >= 4)
                st.Slots.Add((Enum.Parse<Rideable.RideableType>(sp[0]),
                    new Color((byte)int.Parse(sp[1]), (byte)int.Parse(sp[2]), (byte)int.Parse(sp[3]), (byte)255)));
        }
        stables.Add(st);
    }

    // ── Tutorial state ──
    tutorialCompleted = GB("tutDone"); tutorialActive = GB("tutActive"); tutorialStep = GI("tutStep");
    for (int i = 0; i < tutorialTasks.Count; i++) tutorialTasks[i].Done = GB("tutTask." + i);
    tutorialMessage = ""; tutorialMessageTimer = 0f;
    SnapshotTutorialMarks();

    // Stats
    dungeonsCleared = GI("dungeonsCleared");
    timesCheated = GI("timesCheated");
    sportPlayCounts.Clear();
    int spc = GI("sportCounts.count");
    for (int i = 0; i < spc; i++) { var k = GS("sportCount." + i + ".k", ""); if (k != "") sportPlayCounts[k] = GI("sportCount." + i + ".v"); }
    minigamePlayCounts.Clear();
    int mpc = GI("miniCounts.count");
    for (int i = 0; i < mpc; i++) { var k = GS("miniCount." + i + ".k", ""); if (k != "") minigamePlayCounts[k] = GI("miniCount." + i + ".v"); }

    // Players IDs
    idClaimed = GI("idClaimed") == 1;
    idPending = GI("idPending") == 1;
    idMailWaiting = GI("idMailWaiting") == 1;
    idDeliveryTimer = GF("idDeliveryTimer");
    idIssuedDate = GS("idIssuedDate", "");
    idTargetHouseIndex = GI("idTargetHouseIndex");
    hasDropzoneCard = GB("hasDropzoneCard");
    dropzoneCredit = GF("dropzoneCredit");
    dropzoneLifetimeSpend = GF("dropzoneLifetimeSpend");
    dropzoneTier = GI("dropzoneTier");

    // ── Cards ──
    player.PlayingCardsLevel = GI("cardsLv", 1); player.PlayingCardsXP = GI("cardsXp");
    player.EuchreRating = GI("euchreRating"); player.FiveHundredRating = GI("fiveHundredRating");
    player.SequenceRating = GI("sequenceRating"); player.EuchreWins = GI("euchreWins");
    player.FiveHundredWins = GI("fiveHundredWins"); player.SequenceWins = GI("sequenceWins");

    // Farming
    farmPlots.Clear();
int fpCount = GI("farmPlots.count");
for (int i = 0; i < fpCount; i++)
{
    var p = new FarmPlot
    {
        Position = new Vector2(GF("farmPlot." + i + ".x"), GF("farmPlot." + i + ".y")),
        Tilled = GB("farmPlot." + i + ".tilled"),
        Planted = GB("farmPlot." + i + ".planted"),
        Watered = GB("farmPlot." + i + ".watered"),
        CropType = GS("farmPlot." + i + ".crop", ""),
        GrowTimer = GF("farmPlot." + i + ".growTimer"),
        GrowDuration = GF("farmPlot." + i + ".growDuration", 30f),
        ReadyToHarvest = GB("farmPlot." + i + ".ready"),
    };
    farmPlots.Add(p);
}

fruitTrees.Clear();
int ftCount = GI("fruitTrees.count");
for (int i = 0; i < ftCount; i++)
{
    var t = new FruitTree
    {
        Position = new Vector2(GF("fruitTree." + i + ".x"), GF("fruitTree." + i + ".y")),
        Tilled = GB("fruitTree." + i + ".tilled"),
        Planted = GB("fruitTree." + i + ".planted"),
        FruitType = GS("fruitTree." + i + ".type", ""),
        GrowTimer = GF("fruitTree." + i + ".growTimer"),
        GrowDuration = GF("fruitTree." + i + ".growDuration", 90f),
        ReadyToHarvest = GB("fruitTree." + i + ".ready"),
        RegrowTimer = GF("fruitTree." + i + ".regrowTimer"),
    };
    fruitTrees.Add(t);
}

spadePickedUp = GB("spadePickedUp");
wateringCanPickedUp = GB("wateringCanPickedUp");
wheatSeedsPickedUp = GB("wheatSeedsPickedUp");
for (int i = 0; i < 8; i++) toolbarWaterCharge[i] = GF("toolbarWaterCharge." + i);

    // Livestock load
    livestockPens.Clear();
int penCount = GI("livestockPens.count");
for (int i = 0; i < penCount; i++)
    livestockPens.Add(new LivestockPen {
        Position = new Vector2(GF("pen." + i + ".x"), GF("pen." + i + ".y")),
        Animal = GS("pen." + i + ".animal"), Produce = GS("pen." + i + ".produce"),
        Feed = GS("pen." + i + ".feed"), Cycle = GF("pen." + i + ".cycle"),
        Timer = GF("pen." + i + ".timer"), Fed = GB("pen." + i + ".fed"),
        ReadyToHarvest = GB("pen." + i + ".ready"),
    });

    // ── Gear ──
    player.OwnedGear.Clear();
    int gearCount = GI("gear.count");
    for (int i = 0; i < gearCount; i++) { var g = GS("gear." + i, "empty"); if (g != "empty") player.OwnedGear.Add(g); }

    // ── Toolbar ──
    for (int i = 0; i < 8; i++)
    {
        toolbarSlots[i] = GN("toolbar." + i + ".item");
        toolbarCounts[i] = GI("toolbar." + i + ".count");
    }

    // ── Backpack ──
    backpack.Clear();
    int bpCount = GI("backpack.count");
    for (int i = 0; i < bpCount; i++)
    {
        string k = GS("backpack." + i + ".key", "");
        if (k != "") backpack[k] = GI("backpack." + i + ".val");
    }

    // ── Houses ──
    ownedHousePlots.Clear();
    houseDataList.Clear();
    int houseCount = GI("houses.count");
    for (int i = 0; i < houseCount; i++)
    {
        int hx = GI("house." + i + ".x");
        int hy = GI("house." + i + ".y");
        var hd = new HouseData(hx, hy)
        {
            WallColor = GS("house." + i + ".wall", "Beige"),
            FloorColor = GS("house." + i + ".floor", "Oak"),
            HouseType = GS("house." + i + ".type", "Standard")
        };
        int furnCount = GI("house." + i + ".furnCount");
        for (int j = 0; j < furnCount; j++)
        {
            string[] fp = GS("house." + i + ".furn." + j, "").Split('|');
            if (fp.Length >= 4 &&
                int.TryParse(fp[1], out int rx) &&
                int.TryParse(fp[2], out int ry) &&
                int.TryParse(fp[3], out int cost))
            {
                hd.Furniture.Add(new HouseFurniture(fp[0], rx, ry, cost, Color.Gray));
            }
        }
        ownedHousePlots.Add((hx, hy));
        houseDataList.Add(hd);
    }
    for (int i = 0; i < ownedHousePlots.Count; i++) { activeHousePlotIndex = i; SpawnPlayerHouse(i); }
    activeHousePlotIndex = Math.Max(0, ownedHousePlots.Count - 1);

    // ── Pets ──
    ownedEggs.Clear();
    int eggCount = GI("eggs.count");
    for (int i = 0; i < eggCount; i++) { var e = GS("egg." + i, "empty"); if (e != "empty") ownedEggs.Add(e); }
    incubatingEgg = GN("incubatingEgg");
    incubationProgress = GF("incubationProgress");
    incubationNeeded = 1f / daySpeed;
    storedPets.Clear();
    int spCount = GI("storedPets.count");
    for (int i = 0; i < spCount; i++) { var sp = GS("storedPet." + i, "empty"); if (sp != "empty") storedPets.Add(sp); }

    string apType = GS("activePet.type", "empty");
    activePet = apType == "empty" ? null
        : new Pet(new Vector2(GF("activePet.x"), GF("activePet.y")), apType, PetColorFor(apType + " Egg"));
    if (activePet != null && (apType == "Kitten" || apType == "Puppy"))
    {
        activePet.IsBaby = true;
        activePet.AdultType = GS("activePet.adultType", apType == "Kitten" ? "Cat" : "Dog");
        activePet.AgeTimer = GF("activePet.age");
    }
    if (activePet != null)
    {
        activePet.Adopted   = GS("activePet.adopted", "0") == "1";
        activePet.Bond      = (int)GF("activePet.bond");
        activePet.StrengthXP= (int)GF("activePet.str");
        activePet.MagicXP   = (int)GF("activePet.mag");
        activePet.SocialXP  = (int)GF("activePet.soc");
        activePet.AdultRole = GS("activePet.adultRole", "");
    }
    string ppType = GS("pendingPet.type", "empty");
    pendingPet = ppType == "empty" ? null
        : new Pet(new Vector2(GF("pendingPet.x"), GF("pendingPet.y")), ppType, PetColorFor(ppType + " Egg"));
}  
    }
}
