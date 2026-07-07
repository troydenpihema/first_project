using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static float DropzonePlayCost() => dropzoneTierPrices[dropzoneTier];

        static int BackpackCount(string item) => backpack.TryGetValue(item, out int c) ? c : 0;

        static bool BackpackAdd(string item, int count)
        {
            if (count <= 0) return true;
            if (!backpack.ContainsKey(item) && !HasInventoryRoom(item))
                return false;
            backpack[item] = BackpackCount(item) + count;
            return true;
        }

        static bool HasInventoryRoom(string item)
        {
            // already-tracked stackable (stat field or backpack entry) never needs a new slot
            if (GetItemCount(item) > 0) return true;
            if (IsToolItem(item) && (ToolInToolbar(item) || BaseTool(item) == "Axe" && player.HasAxe)) return true;
            return CountUsedInventorySlots() < backpackCapacity;
        }

static int CountUsedInventorySlots()
{
    int n = 0;
    foreach (var s in statItems) if (s.Get() > 0) n++;
    if (player.Money > 0) n++;
    if (player.HasAxe && !ToolInToolbar("Axe")) n++;
    if (pickaxePickedUp && !ToolInToolbar("Pickaxe")) n++;
    if (spadePickedUp && !ToolInToolbar("Spade")) n++;
    if (wateringCanPickedUp && !ToolInToolbar("Watering Can")) n++;
    if (fishingRodPickedUp && !ToolInToolbar("Rod")) n++;
    if (fishingNetPickedUp && !ToolInToolbar("Net")) n++;
    if (torchPickedUp && !ToolInToolbar("Torch")) n++;
    if (stickPickedUp && !ToolInToolbar("Stick") && !WeaponEquipped("Stick")) n++;
    if (swordPickedUp && !ToolInToolbar("Sword") && !WeaponEquipped("Sword")) n++;
    if (player.HasBow && !ToolInToolbar("Bow") && !WeaponEquipped("Bow")) n++;
    if (player.HasCrossbow && !ToolInToolbar("Crossbow") && !WeaponEquipped("Crossbow")) n++;
    if (player.Arrows > 0 && equippedAmmo != "Arrows") n++;
    if (player.Bolts > 0 && equippedAmmo != "Bolts") n++;
    if (player.ArcaneEssence > 0 && equippedAmmo != "Arcane Essence") n++;
    foreach (var g in player.OwnedGear)
        if (g.EndsWith("Staff") && g != equipped1H && g != equipped2H) n++;
    foreach (var kv in backpack)
        if (kv.Value > 0) n++;
    return n;
}

static bool TryGiveItem(string item, int count = 1)
{
    if (count <= 0) return true;

    if (!IsToolItem(item))
        for (int i = 0; i < toolbarSlots.Length; i++)
            if (toolbarSlots[i] == item)
            {
                toolbarCounts[i] += count;
                return true;
            }

    bool alreadyTracked = GetItemCount(item) > 0
        || (IsToolItem(item) && ToolInToolbar(item));

    if (!alreadyTracked && CountUsedInventorySlots() >= backpackCapacity)
    {
        ShowNotification($"Inventory full! Can't pick up {item}.");
        return false;
    }

    switch (item)
    {
        case "Logs": player.Logs += count; break;
        case "Birch Logs": player.BirchLogs += count; break;
        case "Oak Logs": player.OakLogs += count; break;
        case "Pine Logs": player.PineLogs += count; break;
        case "Arctic Logs": player.ArcticLogs += count; break;
        case "Dead Wood": player.DeadWood += count; break;
        case "Stone": player.StoneOre += count; break;
        case "Copper Ore": player.CopperOre += count; break;
        case "Iron Ore": player.IronOre += count; break;
        case "Gold Ore": player.GoldOre += count; break;
        case "Crystal": player.Crystals += count; break;
        case "Fish": player.Fish += count; break;
        case "Bones": player.Bones += count; break;
        case "Fur": player.Fur += count; break;
        case "Stingers": player.Stingers += count; break;
        case "Pelts": player.BearPelts += count; break;
        case "Dog Fangs": player.DogFangs += count; break;
        case "Wolf Claw": player.WolfClaws += count; break;
        case "Venom Sac": player.VenomSacs += count; break;
        case "Crab Claw": player.CrabClaws += count; break;
        case "Bear Claw": player.BearClaws += count; break;
        case "Crab Shell": player.CrabShells += count; break;
        case "Shark Fin": player.SharkFins += count; break;
        case "Shark Tooth": player.SharkTeeth += count; break;
        case "Snake Skin": player.SnakeSkins += count; break;
        case "Snake Fang": player.SnakeFangs += count; break;
        case "Croc Scale": player.CrocScales += count; break;
        case "Croc Tooth": player.CrocTeeth += count; break;
        case "Lizard Scale": player.LizardScales += count; break;
        case "Ember Stone": player.EmberStones += count; break;
        case "Magma Shard": player.MagmaShards += count; break;
        case "Lava Core": player.LavaCores += count; break;
        case "Feather": player.Feathers += count; break;
        case "Eagle Talon": player.EagleTalons += count; break;
        case "Horn": player.Horns += count; break;
        case "Goat Hoof": player.GoatHooves += count; break;
        case "Arrows": player.Arrows += count; break;
        case "Bolts": player.Bolts += count; break;
        case "Arcane Essence": player.ArcaneEssence += count; break;
        case "Money": player.Money += count; break;
        default:
        if (GetItemSlot(item) != null || item.EndsWith("Staff"))
        {
            if (!AcquireGear(item)) return false;
        }
        else if (!BackpackAdd(item, count)) return false;
        break;
        }
        return true;
}

        static void BackpackRemoveOne(string item)
        {
            if (!backpack.ContainsKey(item)) return;
            backpack[item]--;
            if (backpack[item] <= 0) backpack.Remove(item);
        }

        static bool ToolInToolbar(string tool)
        {
            for (int i = 0; i < toolbarSlots.Length; i++)
                if (toolbarSlots[i] == tool) return true;
            return false;
        }

        record StatItemDef(string Name, Func<int> Get, Action<int> Delta);

static StatItemDef FindStatItem(string name) => statItems.FirstOrDefault(s => s.Name == name);

        static bool NeedsDropConfirm(string item) =>
            IsToolItem(item) || GetItemSlot(item) != null || item == "Money";

        static bool IsUsable(string item) => item switch
        {
            "Cooked Fish" or "Apple" or "Bread" or "Bandage" or "Health Potion" or "Cooked Meat"  or "Shopping Bag" or "Roast Potato" or "Cooked Corn" or "Bacon & Eggs" or "Pasta Meal"
            or "Sandwich" or "Fruit Salad" or "Steak & Chips" or "Vegetable Soup"
            or "Pancakes" or "Homemade Pizza" or "Colossus Egg" or "Titan Egg" or "Canteen (Clean)" or "Stamina Potion" => true,
            _ => false
        };

        static bool IsEquippable(string item) =>
            IsToolItem(item) || GetItemSlot(item) != null || item == "Arrows" || item == "Bolts" || item == "Arcane Essence";

static bool IsToolItem(string item) => item switch
{
    "Axe" or "Pickaxe" or "Rod" or "Net" or "Sword" or "Stick" or "Torch" or "Bow" or "Crossbow"
        or "Watering Can" or "Spade" => true,   
     _ => item != null && metalPrefixes.Any(m => item.StartsWith(m + " ")
            && IsToolItem(item.Substring(m.Length + 1)))
};

static void DoInventoryAction(string action, string item)
{
    switch (action)
    {
        case "Use":
            UseItem(item);
            break;
        case "Equip":
            if (item == "Arrows" || item == "Bolts" || item == "Arcane Essence")
            {
                equippedAmmo = item;
                ShowNotification($"{item} equipped as ammo.");
            }
            else if ((item.Contains("Sword") && !item.Contains("Great Sword"))
                     || item == "Stick"
                     || (item.EndsWith("Staff") && !item.Contains("Great")))
            {
                // return the currently-equipped 2H weapon to inventory before clearing
                if (IsRangedWeapon(equipped2H))
                {
                    if (equipped2H.Contains("Crossbow")) player.HasCrossbow = true;
                    else                                 player.HasBow = true;
                }

                equipped1H = item;
                equipped2H = null;
                armorWeapon = item;
                if (item.Contains("Sword")) swordPickedUp = true;
                else if (item == "Stick") stickPickedUp = true;
                if (item.EndsWith("Staff")) player.EquippedStaff = item;
                ShowNotification($"{item} set as 1H weapon. Press T to draw it.");
            }
            else if (IsRangedWeapon(item)
                     || item.Contains("Great Sword")
                     || item.Contains("War Axe")
                     || (item.Contains("Great") && item.EndsWith("Staff")))
            {
                // return the currently-equipped 1H weapon to inventory before clearing
                if (equipped1H != null && equipped1H.Contains("Sword")) swordPickedUp = true;
                else if (equipped1H == "Stick") stickPickedUp = true;

                equipped2H = item;
                equipped1H = null;
                armorShield = null;
                armorWeapon = item;
                if (item.Contains("Crossbow")) player.HasCrossbow = true;
                else if (item.Contains("Bow")) player.HasBow = true;
                if (item.EndsWith("Staff")) player.EquippedStaff = item;
                ShowNotification($"{item} set as 2H weapon. Press T to draw it.");
            }
            else if (IsToolItem(item))
                AddToToolbar(item);
            else
                AcquireGear(item);
            break;
        case "Move to Toolbar":
            if (item.Contains("Sword") || item == "Stick" || IsRangedWeapon(item)
                || item.Contains("War Axe") || item.EndsWith("Staff"))
                ShowNotification($"{item} is a weapon — use Equip instead.");
            else if (item == "Arrows" || item == "Bolts" || item == "Arcane Essence")
                ShowNotification($"{item} is ammo — use Equip to load it.");
            else
                AddToToolbar(item);
            break;
        case "Drop":
            dropQtyItem = item;
            dropQtyAvailable = GetItemCount(item);
            if (NeedsDropConfirm(item))
            {
                dropConfirmOpen = true;
                dropConfirmItem = item;
            }
            else
            {
                dropQtyOpen = true;
            }
            break;
    }
}

static void UseItem(string item)
{
    switch (item)
    {
        case "Apple":         player.Health = Math.Min(player.MaxHealth, player.Health + 10); RemoveOneItem(item); ShowNotification("+10 HP"); break;
        case "Bread":         player.Health = Math.Min(player.MaxHealth, player.Health + 20); RemoveOneItem(item); ShowNotification("+20 HP"); break;
        case "Cooked Fish":   player.Health = Math.Min(player.MaxHealth, player.Health + 15); RemoveOneItem(item); ShowNotification("+15 HP"); break;
        case "Cooked Meat":   player.Health = Math.Min(player.MaxHealth, player.Health + 25); RemoveOneItem(item); ShowNotification("+25 HP"); break;
        case "Bandage":       player.Health = Math.Min(player.MaxHealth, player.Health + 40); RemoveOneItem(item); ShowNotification("+40 HP"); break;
        case "Health Potion": player.Health = player.MaxHealth; RemoveOneItem(item); ShowNotification("Fully healed!"); break;
        case "Roast Potato":      player.Health = Math.Min(player.MaxHealth, player.Health + 15); RemoveOneItem(item); ShowNotification("+15 HP"); break;
        case "Cooked Corn":       player.Health = Math.Min(player.MaxHealth, player.Health + 12); RemoveOneItem(item); ShowNotification("+12 HP"); break;
        case "Bacon & Eggs":      player.Health = Math.Min(player.MaxHealth, player.Health + 40); RemoveOneItem(item); ShowNotification("+40 HP"); break;
        case "Pasta Meal":        player.Health = Math.Min(player.MaxHealth, player.Health + 45); RemoveOneItem(item); ShowNotification("+45 HP"); break;
        case "Sandwich":          player.Health = Math.Min(player.MaxHealth, player.Health + 35); RemoveOneItem(item); ShowNotification("+35 HP"); break;
        case "Fruit Salad":       player.Health = Math.Min(player.MaxHealth, player.Health + 30); RemoveOneItem(item); ShowNotification("+30 HP"); break;
        case "Steak & Chips":     player.Health = Math.Min(player.MaxHealth, player.Health + 60); RemoveOneItem(item); ShowNotification("+60 HP"); break;
        case "Vegetable Soup":    player.Health = Math.Min(player.MaxHealth, player.Health + 50); RemoveOneItem(item); ShowNotification("+50 HP"); break;
        case "Pancakes":          player.Health = Math.Min(player.MaxHealth, player.Health + 35); RemoveOneItem(item); ShowNotification("+35 HP"); break;
        case "Homemade Pizza":    player.Health = Math.Min(player.MaxHealth, player.Health + 70); RemoveOneItem(item); ShowNotification("+70 HP"); break;
        case "Shopping Bag":
        foreach (string groceryItem in groceryBagContents)
            if (!AddOneItemToToolbar(groceryItem))
                ShowNotification($"Toolbar full — {groceryItem} lost!");
        groceryBagContents.Clear();
        RemoveOneItem("Shopping Bag");
        ShowNotification("Unpacked your shopping!");
        return; // early return to avoid "Used Shopping Bag" message
        }
        ShowNotification($"Used {item}");
}

public static string GetActiveItem()
{
    if (currentPhase == HandPhase.Combat)
    {
        if (equipped2H != null) return equipped2H;
        if (equipped1H != null) return equipped1H;
        return null;   // nothing drawn
    }
    else // Tools
    {
        return toolbarSlots[toolbarSelectedSlot];
    }
}

static void EquipToToolbar(string tool)
{
    
    for (int i = 0; i < 8; i++)
        if (toolbarSlots[i] == tool) { ShowNotification($"{tool} already on toolbar"); return; }
    for (int i = 0; i < 8; i++)
    {
        if (toolbarSlots[i] == null || toolbarSlots[i] == "empty")
        {
            toolbarSlots[i] = tool;
            ShowNotification($"Moved {tool} to slot {i + 1}");
            return;
        }
    }
    ShowNotification("Toolbar full!");
}

static void RemoveFromToolbar(int slot)
{
    if (slot < 0 || slot >= toolbarSlots.Length) return;
    string item = toolbarSlots[slot];
    if (item == null) return;

    if (item == "Sword") { swordPickedUp = true; }
    else if (item == "Stick") { stickPickedUp = true; }
    else if (IsToolItem(item))
    {
        // tools keep their pickup flags, so clearing the slot returns them to inventory
    }
    else if (HasOtherHome(item))
    {
        GiveItemBack(item, toolbarCounts[slot]);   // stat-backed stackables go to their counter
    }
    else
    {
        // toolbar-only item (grocery / cooked food / ashes): no other home, don't delete it
        ShowNotification($"{item} can only be kept on the toolbar — use Drop to discard it.");
        return;
    }

    ShowNotification($"Removed {item} from toolbar");
    toolbarSlots[slot] = null;
    toolbarCounts[slot] = 0;
}

static void UseToolbarItem(int slot)
{
    if (slot < 0 || slot >= toolbarSlots.Length) return;
    string item = toolbarSlots[slot];
     if (item == null || !IsUsable(item)) return;

    // ── egg: incubate if near an incubator ──
    if (IsEgg(item))
    {
        bool nearIncubator = incubatorPositions.Any(p => Vector2.Distance(player.Center, p) <= 120f);
        if (!nearIncubator)
        {
            ShowNotification("Stand next to an incubator to use an egg.");
            return;
        }
        StartIncubation(item);
        toolbarCounts[slot]--;
        if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
        return;
    }

    if (item == "Canteen (Clean)")
{
    player.Thirst = Math.Min(100f, player.Thirst + 60f);
    toolbarCounts[slot]--;
    if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
    AddOneItemToToolbar("Canteen");   // empty canteen back
    ShowNotification("Refreshing! +60 Thirst");
    return;
}
if (item == "Stamina Potion")
{
    player.Stamina = 100f;
    toolbarCounts[slot]--;
    if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
    ShowNotification("Stamina fully restored!");
    return;
}

    // figure out the heal amount
    int heal = item switch
    {
        "Apple"        => 10,
        "Bread"        => 20,
        "Cooked Fish"  => 15,
        "Cooked Meat"  => 25,
        "Roast Potato" => 18,
        "Cooked Corn"  => 12,
        "Bandage"      => 40,
        "Health Potion" => player.MaxHealth,   // full heal
        _ => 0
    };

    // recipe meals fall back to their HpRestore value
    if (heal == 0)
    {
        var recipe = cookingRecipes.FirstOrDefault(r => r.Result == item);
        if (recipe != null) heal = recipe.HpRestore;
    }

    // Shopping Bag is "usable" but unpacks rather than heals — let UseItem handle it
    if (item == "Shopping Bag")
    {
        UseItem(item);            // unpacks groceries
        // UseItem doesn't know about the slot, so clear one from the slot here
        toolbarCounts[slot]--;
        if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;
        return;
    }

    if (item == "Health Potion") player.Health = player.MaxHealth;
    else player.Health = Math.Min(player.MaxHealth, player.Health + heal);

    if (heal > 0 && item != "Bandage" && item != "Health Potion")
    player.Food = Math.Min(100f, player.Food + heal * 1.5f);

    // consume one from the slot
    toolbarCounts[slot]--;
    if (toolbarCounts[slot] <= 0) toolbarSlots[slot] = null;

    ShowNotification(item == "Health Potion" ? "Fully healed!" : $"+{heal} HP");
}

static bool HasOtherHome(string item) => item switch
{
    "Logs" or "Birch Logs" or "Oak Logs" or "Pine Logs" or "Arctic Logs"
        or "Dead Wood" or "Fish" or "Bones" or "Fur" or "Stingers" or "Pelts"
        or "Dog Fangs" or "Wolf Claw" or "Venom Sac" or "Crab Claw" or "Bear Claw"
        or "Crab Shell" or "Shark Fin" or "Shark Tooth" or "Snake Skin" or "Snake Fang"
        or "Croc Scale" or "Croc Tooth" or "Lizard Scale" or "Ember Stone" or "Magma Shard"
        or "Lava Core" or "Feather" or "Eagle Talon" or "Horn" or "Goat Hoof"
        or "Stone" or "Copper Ore" or "Iron Ore" or "Gold Ore" or "Crystal"
        or "Arrows" or "Bolts" or "Money" => true,
    _ => true
};

static void RemoveOneItem(string item)
{
    var stat = FindStatItem(item);
    if (stat != null) { stat.Delta(-1); return; }
    switch (item)
    {
        case "Arrows": player.Arrows--; break;
        case "Bolts": player.Bolts--; break;
        case "Axe": player.HasAxe = false; break;
        case "Bow": player.HasBow = false; break;
        case "Crossbow": player.HasCrossbow = false; break;
        case "Sword": swordPickedUp = false; break;
        case "Stick": stickPickedUp = false; break;
        case "Pickaxe": pickaxePickedUp = false; break;
        case "Rod":     fishingRodPickedUp = false; break;
        case "Net":     fishingNetPickedUp = false; break;
        default: BackpackRemoveOne(item); break;
    }
}        

        static bool CanEquipFarmTool(string toolName)
        {
            var tier = spadeTiers.FirstOrDefault(t => t.Name == toolName)
                    ?? wateringCanTiers.FirstOrDefault(t => t.Name == toolName);
            if (tier == null) return true;   // not a tiered farm tool, no restriction
            return player.FarmingLevel >= tier.MinLevel;
        }

        public static Color MaterialColor(string material)
        {
            switch (material)
            {
                case "Leather":  return new Color((byte)120,(byte)80,(byte)30,(byte)255);
                case "Iron":     return new Color((byte)120,(byte)120,(byte)130,(byte)255);
                case "Steel":    return new Color((byte)160,(byte)165,(byte)175,(byte)255);
                case "Gold":     return new Color((byte)220,(byte)180,(byte)40,(byte)255);
                case "Diamond":  return new Color((byte)120,(byte)220,(byte)230,(byte)255);
                case "Ruby":     return new Color((byte)200,(byte)40,(byte)60,(byte)255);
                case "Sapphire": return new Color((byte)50,(byte)90,(byte)220,(byte)255);
                case "Emerald":  return new Color((byte)40,(byte)180,(byte)80,(byte)255);
                case "Mage":   return new Color((byte)130,(byte)70,(byte)200,(byte)255);   
                case "Ranger": return new Color((byte)70,(byte)130,(byte)60,(byte)255); 
                // ── mage tiers ──
                case "Apprentice": return new Color((byte)170,(byte)150,(byte)210,(byte)255);  // pale lavender
                case "Twilight":   return new Color((byte)75,(byte)55,(byte)150,(byte)255);    // deep indigo
                case "Stella":     return new Color((byte)90,(byte)150,(byte)240,(byte)255);   // starlight blue
                case "Phantom":    return new Color((byte)120,(byte)200,(byte)200,(byte)255);  // ghostly teal
                case "Supernova":  // ANIMATED — hot star pulse: orange → magenta → white
                {
                    float t = (float)Raylib.GetTime() * 3f;
                    byte r = (byte)(220 + MathF.Sin(t) * 35);
                    byte g = (byte)(120 + MathF.Sin(t + 2.1f) * 100);
                    byte b = (byte)(100 + MathF.Sin(t + 4.2f) * 120);
                    return new Color(r, g, b, (byte)255);
                }
                // ── ranger tiers ── ("Leather" already exists above)
                case "Scaled":  return new Color((byte)110,(byte)120,(byte)55,(byte)255);      // dull scale green
                case "Sun":     return new Color((byte)240,(byte)190,(byte)60,(byte)255);      // golden
                case "Shadow":  return new Color((byte)45,(byte)45,(byte)58,(byte)255);        // near-black
                case "Falcon":  return new Color((byte)125,(byte)140,(byte)170,(byte)255);     // slate blue-grey
                case "Serpent": // ANIMATED — slow slithering green shimmer
                {
                    float t = (float)Raylib.GetTime() * 2f;
                    byte g = (byte)(150 + MathF.Sin(t) * 70);
                    byte r = (byte)(30 + MathF.Sin(t + 1.5f) * 25);
                    return new Color(r, g, (byte)60, (byte)255);
                } 
                case "Infernal":
                {
                    // dark charcoal base with a faint ember flicker
                    float t = (float)Raylib.GetTime() * 4f;
                    float flicker = (MathF.Sin(t) + 1f) / 2f;
                    byte v = (byte)(20 + flicker * 15);   // 20..35, very dark
                    return new Color(v, (byte)(v - 5), (byte)(v - 5), (byte)255);
                }
                case "Magic":
                {
                    // smoothly cycle through purple → blue → cyan → pink
                    float t = (float)Raylib.GetTime() * 1.5f;
                    byte r = (byte)(140 + MathF.Sin(t) * 100);            // 40..240
                    byte g = (byte)(60 + MathF.Sin(t + 2f) * 60);         // 0..120
                    byte b = (byte)(180 + MathF.Sin(t + 4f) * 70);        // 110..250
                    return new Color(r, g, b, (byte)255);
                }
                case "Mystical":
                {
                    
                    float t = (float)Raylib.GetTime() * 2f;
                    float shimmer = (MathF.Sin(t) + 1f) / 2f;
                    byte v = (byte)(230 + shimmer * 25);   // 230..255, near white
                    return new Color(v, v, (byte)255, (byte)255);
                }
                default:
            return new Color((byte)120,(byte)80,(byte)30,(byte)255);
            }
        }

        static bool TryStowItem(string item)
        {
            for (int i = 0; i < toolbarSlots.Length; i++)
                if (toolbarSlots[i] == item) { toolbarCounts[i]++; return true; }
            for (int i = 0; i < toolbarSlots.Length; i++)
                if (toolbarSlots[i] == null) { toolbarSlots[i] = item; toolbarCounts[i] = 1; return true; }
            return BackpackAdd(item, 1);
        }

        static List<string> GetAllPlayerItemNames()
        {
            var names = new List<string>();
            foreach (var s in statItems) if (s.Get() > 0) names.Add(s.Name);
            void Add(string n) { if (GetItemCount(n) > 0) names.Add(n); }
            Add("Money"); Add("Arrows"); Add("Bolts"); Add("Arcane Essence");
            foreach (var kv in backpack) if (kv.Value > 0) names.Add(kv.Key);
            return names;
        }

        static Vector2 FrontOfPlayer(float dist = 70f) => player.Center + player.Facing switch
        {
            Player.FacingDirection.Up    => new Vector2(0, -dist),
            Player.FacingDirection.Down  => new Vector2(0,  dist),
            Player.FacingDirection.Left  => new Vector2(-dist, 0),
            _                            => new Vector2( dist, 0),
        };

        static void PlaceHeldItem(string item)
        {
            Vector2 pos = FrontOfPlayer();

            // don't stack placeables on top of each other
            bool blocked = placedWorkbenches.Any(w => Vector2.Distance(w, pos) < 60)
                        || placedChests.Any(c => c.BuildingContext == "" && Vector2.Distance(c.Position, pos) < 60)
                        || placedFurnaces.Any(f => Vector2.Distance(f, pos) < 60) 
                        || campfirePositions.Any(c => Vector2.Distance(c, pos) < 60)
                        || stationProps.Values.Any(l => l.Any(p => Vector2.Distance(p, pos) < 60));
            if (blocked) { ShowNotification("Something is already there."); return; }

            if (item == "Chest" || item == "Large Chest")
                placedChests.Add(new PlacedChest {
                    Id = "chest_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Position = pos, BuildingContext = "", Tier = PlaceableChestTier(item) });
            else if (item == "Workbench") placedWorkbenches.Add(pos);
            else if (item == "Furnace")   placedFurnaces.Add(pos);
            else if (item == "Campfire")      campfirePositions.Add(pos);   
            else if (item == "Waypoint Flag") placedFlags.Add(pos);
            else if (stationProps.ContainsKey(item)) stationProps[item].Add(pos); 

            toolbarCounts[toolbarSelectedSlot]--;
            if (toolbarCounts[toolbarSelectedSlot] <= 0) toolbarSlots[toolbarSelectedSlot] = null;
            ShowNotification($"Placed {item}.");
        }

        static void TryPickUpPlaceable()
        {
            // workbench first
            int wb = placedWorkbenches.FindIndex(w => Vector2.Distance(player.Center, w) < 80);
            if (wb >= 0)
            {
                if (!TryStowItem("Workbench")) { ShowNotification("Sorry, not enough space!"); return; }
                placedWorkbenches.RemoveAt(wb);
                ShowNotification("Picked up Workbench.");
                return;
            }

            int fu = placedFurnaces.FindIndex(f => Vector2.Distance(player.Center, f) < 80);
            if (fu >= 0)
            {
                if (!TryStowItem("Furnace")) { ShowNotification("Sorry, not enough space!"); return; }
                placedFurnaces.RemoveAt(fu);
                ShowNotification("Picked up Furnace.");
                return;
            }

            foreach (var kv in stationProps)
            {
                int si = kv.Value.FindIndex(p => Vector2.Distance(player.Center, p) < 80);
                if (si < 0) continue;
                if (!TryStowItem(kv.Key)) { ShowNotification("Sorry, not enough space!"); return; }
                kv.Value.RemoveAt(si);
                ShowNotification($"Picked up {kv.Key}.");
                return;
            }

            int fl = placedFlags.FindIndex(f => Vector2.Distance(player.Center, f) < 80);
            if (fl >= 0)
            {
                if (!TryStowItem("Waypoint Flag")) { ShowNotification("Sorry, not enough space!"); return; }
                placedFlags.RemoveAt(fl);
                ShowNotification("Picked up Waypoint Flag.");
                return;
            }

            // NEW — player-placed campfires only, and only when cold:
            int cf = campfirePositions.FindIndex(c => Vector2.Distance(player.Center, c) < 80);
            if (cf >= builtinCampfireCount)
            {
                if (CampfireIsLit(cf)) { ShowNotification("Let the fire burn out first."); return; }
                if (!TryStowItem("Campfire")) { ShowNotification("Sorry, not enough space!"); return; }
                RemoveCampfireAt(cf);
                ShowNotification("Picked up Campfire.");
                return;
            }

            var chest = placedChests.FirstOrDefault(c =>
                c.BuildingContext == "" && Vector2.Distance(player.Center, c.Position) < 80);
            if (chest != null)
            {
                if (chest.UsedSlots > 0) { ShowNotification("Empty the chest before picking it up."); return; }
                string chestName = chest.Tier >= 1 ? "Large Chest" : "Chest"; 
                if (!TryStowItem("Chest")) { ShowNotification("Sorry, not enough space!"); return; }
                placedChests.Remove(chest);
                ShowNotification($"Picked up {chestName}.");
            }
        }

        record HallItem(string Name, string Category, int Price, Color Primary, Color Secondary, string Style);

        static void BuyDropZoneItem(string name, int cost, int heal)
        {
            if (player.Money >= cost)
            {
                player.Money -= cost;
                player.Health = Math.Min(player.MaxHealth, player.Health + heal);
                shopMessage = $"Bought {name}! +{heal} HP";
                shopMessageTimer = 2f;
            }
            else
            {
                shopMessage = $"Not enough money for {name} (${cost})";
                shopMessageTimer = 2f;
            }
        }

        public static bool TryPayDropzonePlay()
        {
            if (!hasDropzoneCard)
            {
                ShowNotification("You need a DropZone card! Buy one at the counter.");
                return false;
            }
            float cost = DropzonePlayCost();
            if (dropzoneCredit < cost)
            {
                ShowNotification($"Not enough credit (${cost:0.00} needed). Load more at the counter.");
                return false;
            }
            dropzoneCredit -= cost;
            dropzoneLifetimeSpend += cost;
            UpdateDropzoneTier();
            return true;
        }

        static void UpdateDropzoneTier()
        {
            int newTier = 0;
            for (int t = dropzoneTierThresholds.Length - 1; t >= 0; t--)
                if (dropzoneLifetimeSpend >= dropzoneTierThresholds[t]) { newTier = t + 1; break; }  // CHANGED: t → t + 1
            newTier = Math.Min(newTier, dropzoneTierPrices.Length - 1);                              // ADDED: clamp to valid range
            if (newTier > dropzoneTier)
            {
                dropzoneTier = newTier;
                ShowNotification($"DropZone card upgraded to {dropzoneTierNames[dropzoneTier]}! Plays now ${DropzonePlayCost():0.00}");
            }
        }

static int CountInventorySlotsFree()
{
    // counts free toolbar slots as inventory space
    int free = 0;
    foreach (var s in toolbarSlots) if (s == null) free++;
    return free;
}

static Color IconWeaponColor(string item, Color fallback)
{
    if (item == null) return fallback;
    foreach (string mat in armorMaterials)
        if (item.Contains(mat)) return MaterialColor(mat);
    return fallback;
}

static bool AcquireGear(string item, bool announce = true)   // CHANGED return type
{
    if (!player.OwnedGear.Contains(item) && CountUsedInventorySlots() >= backpackCapacity)
    {
        if (announce) ShowNotification("Inventory full!");
        return false;
    }

    player.AddGear(item);

    string slot = GetItemSlot(item);
    if (slot == null) { if (announce) ShowNotification($"Got {item}!"); return true; }

    // figure out if the target slot is empty
    bool slotEmpty = slot switch
    {
        "HELMET" => armorHelmet == null,
        "BODY"   => armorBody   == null,
        "LEGS"   => armorLegs   == null,
        "BOOTS"  => armorBoots  == null,
        "GLOVES" => armorGloves == null,
        "CAPE"   => armorCape   == null,
        "WEAPON" => armorWeapon == null,
        "SHIELD" => armorShield == null,
        _ => false
    };

    if (slotEmpty)
    {
        // respect two-handed rules on auto-equip
        if (slot == "SHIELD" && IsTwoHandedWeapon(armorWeapon))
        {
            if (announce) ShowNotification($"Got {item} (stored — 2H weapon equipped)");
            return true;
        }
        if (slot == "WEAPON" && IsTwoHandedWeapon(item) && armorShield != null)
        {
            if (announce) ShowNotification($"Got {item} (stored — unequip shield to use)");
            return true;
        }
        TryEquipItem(item);
    }
    else if (announce)
    {
        ShowNotification($"Got {item}! (stored in gear)");
    }
    return true;
}

static bool IsGearSellable(string item)
{
    // basic/common gear can be sold; rare or starter gear can't
    return item switch
    {
        "Leather Cap" or "Leather Vest" or "Leather Pants" or "Leather Boots" or "Leather Gloves"
            or "Iron Helmet" or "Iron Chestplate" or "Iron Leggings" or "Iron Boots" or "Iron Gauntlets"
            or "Wooden Shield" or "Iron Shield" or "Wool Cape" or "Sword"
            => true,
        // not sellable: steel tier, magic cape, two-handers, bows/crossbows
        _ => false
    };
}

static int GetGearSellPrice(string item)
{
    return item switch
    {
        "Leather Cap" or "Leather Vest" or "Leather Pants" or "Leather Boots" or "Leather Gloves" => 15,
        "Iron Helmet" or "Iron Chestplate" or "Iron Leggings" or "Iron Boots" or "Iron Gauntlets" => 40,
        "Wooden Shield" => 20,
        "Iron Shield" => 45,
        "Wool Cape" => 25,
        "Sword" => 50,
        _ => 0
    };
}

static List<string> OwnedOneHanded()
{
    var list = new List<string>();

    // owned-gear one-handers
    foreach (var g in player.OwnedGear)
        if (IsOneHandedWeapon(g) && !list.Contains(g)) list.Add(g);

    // flag-based weapons (Sword, Stick)
    if (swordPickedUp && !list.Contains("Sword")) list.Add("Sword");
    if (stickPickedUp && !list.Contains("Stick")) list.Add("Stick");

    // staffs (1H = plain staff, not "Great")
    foreach (var g in player.OwnedGear)
        if (g.EndsWith("Staff") && !g.Contains("Great") && !list.Contains(g)) list.Add(g);

    return list;
}

static List<string> OwnedTwoHanded()
{
    var list = new List<string>();
    foreach (var g in player.OwnedGear)
        if ((IsTwoHandedWeapon(g) || IsRangedWeapon(g)) && !list.Contains(g)) list.Add(g);

    // flag-based ranged weapons (plain Bow / Crossbow)
    if (player.HasBow && !list.Contains("Bow")) list.Add("Bow");
    if (player.HasCrossbow && !list.Contains("Crossbow")) list.Add("Crossbow");

    // great staffs (2H)
    foreach (var g in player.OwnedGear)
        if (g.Contains("Great") && g.EndsWith("Staff") && !list.Contains(g)) list.Add(g);

    return list;
}

static void Cycle1HSlot()
{
    var owned = OwnedOneHanded();
    if (owned.Count == 0)
    {
        if (equipped1H == "Sword") swordPickedUp = true;
        else if (equipped1H == "Stick") stickPickedUp = true;
        equipped1H = null;
        armorWeapon = null;
        player.EquippedStaff = null;
        return;
    }

    int idx = equipped1H == null ? -1 : owned.IndexOf(equipped1H);
    idx++;
    if (idx >= owned.Count)
    {
        if (equipped1H == "Sword") swordPickedUp = true;
        else if (equipped1H == "Stick") stickPickedUp = true;
        equipped1H = null;
        equipped2H = null;
        armorWeapon = null;
        player.EquippedStaff = null;
    }
    else
    {
        equipped1H = owned[idx];
        equipped2H = null;
        armorWeapon = equipped1H;
        player.EquippedStaff = equipped1H.EndsWith("Staff") ? equipped1H : null;
    }
}

static void Cycle2HSlot()
{
    var owned = OwnedTwoHanded();
    if (owned.Count == 0)
    {
        if (equipped2H == "Bow") player.HasBow = true;
        else if (equipped2H == "Crossbow") player.HasCrossbow = true;
        equipped2H = null;
        armorWeapon = null;
        player.EquippedStaff = null;
        return;
    }

    int idx = equipped2H == null ? -1 : owned.IndexOf(equipped2H);
    idx++;
    if (idx >= owned.Count)
    {
        if (equipped2H == "Bow") player.HasBow = true;
        else if (equipped2H == "Crossbow") player.HasCrossbow = true;
        equipped2H = null;
        armorWeapon = null;
        player.EquippedStaff = null;
    }
    else
    {
        equipped2H = owned[idx];
    }

    if (equipped2H != null)
    {
        equipped1H = null;
        armorShield = null;
        armorWeapon = equipped2H;
        player.EquippedStaff = equipped2H.EndsWith("Staff") ? equipped2H : null;
    }
    else if (equipped1H == null)
    {
        armorWeapon = null;
        player.EquippedStaff = null;
    }
}

static void CycleAmmoSlot()
{
    var opts = new List<string>();
    if (player.Arrows > 0) opts.Add("Arrows");
    if (player.Bolts > 0)  opts.Add("Bolts");
    if (player.ArcaneEssence > 0) opts.Add("Arcane Essence");
    if (opts.Count == 0) { equippedAmmo = null; return; }

    int idx = equippedAmmo == null ? -1 : opts.IndexOf(equippedAmmo);
    idx++;
    equippedAmmo = idx >= opts.Count ? null : opts[idx];
}

static int ChestSlotsUsed(int[] chestCounts)
{
    int used = 0;
    foreach (int c in chestCounts)
        if (c > 0) used++;
    return used;
}

static void DrawInventoryUI()
{

    if (player.InventoryOpen)
    {
        int invX = ScreenWidth - 380;
        int invY = 100;
        int slotSize = 80;
        int padding = 10;
        int cols = 4;

        List<(string name, int count)> items = new();
        foreach (var s in statItems)
        if (s.Get() > 0) items.Add((s.Name, s.Get()));
        if (player.Money > 0) items.Add(("Money", player.Money));
        if (player.HasAxe && !ToolInToolbar("Axe")) items.Add(("Axe", 1));
        if (pickaxePickedUp && !ToolInToolbar("Pickaxe")) items.Add(("Pickaxe", 1));
        if (spadePickedUp && !ToolInToolbar("Spade")) items.Add(("Spade", 1));
        if (wateringCanPickedUp && !ToolInToolbar("Watering Can")) items.Add(("Watering Can", 1));
        if (fishingRodPickedUp && !ToolInToolbar("Rod")) items.Add(("Rod", 1));
        if (fishingNetPickedUp && !ToolInToolbar("Net")) items.Add(("Net", 1));
        if (torchPickedUp && !ToolInToolbar("Torch")) items.Add(("Torch", 1));
        if (stickPickedUp && !ToolInToolbar("Stick") && !WeaponEquipped("Stick")) items.Add(("Stick", 1));
        if (swordPickedUp && !ToolInToolbar("Sword") && !WeaponEquipped("Sword")) items.Add(("Sword", 1));
        if (player.HasBow && !ToolInToolbar("Bow") && !WeaponEquipped("Bow")) items.Add(("Bow", 1));
        if (player.HasCrossbow && !ToolInToolbar("Crossbow") && !WeaponEquipped("Crossbow")) items.Add(("Crossbow", 1));
        if (player.Arrows > 0 && equippedAmmo != "Arrows") items.Add(("Arrows", player.Arrows));
        if (player.Bolts > 0 && equippedAmmo != "Bolts") items.Add(("Bolts", player.Bolts));
        if (player.ArcaneEssence > 0 && equippedAmmo != "Arcane Essence") items.Add(("Arcane Essence", player.ArcaneEssence));
        // owned staffs not currently equipped as weapon
        foreach (var g in player.OwnedGear)
            if (g.EndsWith("Staff") && g != equipped1H && g != equipped2H) items.Add((g, 1));
        foreach (var kv in backpack)
            if (kv.Value > 0) items.Add((kv.Key, kv.Value));
      

        Raylib.DrawRectangle(invX - 20, invY - 20, cols * (slotSize + padding) + 30, 5 * (slotSize + padding) + 60, new Color((byte)0,(byte)0,(byte)0,(byte)220));
        Program.DrawTextUI("INVENTORY", invX, invY - 10, 24, Color.Gold);

        Vector2 mouse = Raylib.GetMousePosition();

        for (int i = 0; i < 20; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int x = invX + col * (slotSize + padding);
            int y = invY + 20 + row * (slotSize + padding);

            Raylib.DrawRectangle(x, y, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(x, y, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

            if (i >= items.Count) continue;

            DrawInventoryIcon(items[i].name, x, y, slotSize);
            Program.DrawTextUI($"{items[i].count}", x + 6, y + 6, 16, Color.White);
            Program.DrawTextUI(items[i].name, x + 4, y + slotSize - 20, 13, Color.LightGray);

            // hover + click to select
            Rectangle slotRect = new Rectangle(x, y, slotSize, slotSize);
            if (Raylib.CheckCollisionPointRec(mouse, slotRect))
            {
                Raylib.DrawRectangleLines(x, y, slotSize, slotSize, Color.Gold);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    invSelectedIndex = i;
                    invSelectedName = items[i].name;
                    invSelectedCount = items[i].count;
                }
            }

            // selected highlight
            if (invSelectedIndex == i)
                Raylib.DrawRectangleLinesEx(slotRect, 3, Color.Gold);
        }

        // ── CONTEXT MENU ──
        if (invSelectedIndex >= 0 && invSelectedName != "" && invSelectedIndex < items.Count)
        {
            // keep selected name synced to whatever's in that slot now
            invSelectedName = items[invSelectedIndex].name;

            List<string> actions = new();
            if (IsUsable(invSelectedName))     actions.Add("Use");
            if (IsEquippable(invSelectedName)) actions.Add("Equip");
            actions.Add("Move to Toolbar");
            actions.Add("Drop");

            int menuX = invX - 200;
            int menuY = invY + 40;
            int menuW = 170;
            int rowH = 38;
            int menuH = actions.Count * rowH + 46;

            Raylib.DrawRectangle(menuX, menuY, menuW, menuH, new Color((byte)20,(byte)20,(byte)28,(byte)250));
            Raylib.DrawRectangleLines(menuX, menuY, menuW, menuH, Color.Gold);
            Program.DrawTextUI(invSelectedName.Length > 13 ? invSelectedName.Substring(0,13) : invSelectedName,
                menuX + 10, menuY + 8, 16, Color.Gold);

            for (int a = 0; a < actions.Count; a++)
            {
                int ry = menuY + 36 + a * rowH;
                Rectangle rowRect = new Rectangle(menuX + 8, ry, menuW - 16, rowH - 6);
                bool hover = Raylib.CheckCollisionPointRec(mouse, rowRect);
                Raylib.DrawRectangleRec(rowRect, hover ? new Color((byte)60,(byte)50,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)50,(byte)255));
                Raylib.DrawRectangleLinesEx(rowRect, 1, hover ? Color.Gold : new Color((byte)90,(byte)90,(byte)110,(byte)255));
                Program.DrawTextUI(actions[a], menuX + 16, ry + 8, 16, hover ? Color.Gold : Color.White);

                if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    DoInventoryAction(actions[a], invSelectedName);
                    invSelectedIndex = -1;
                    invSelectedName = "";
                }
            }
        }

        // close menu with right-click anywhere
        if (Raylib.IsMouseButtonPressed(MouseButton.Right))
        {
            invSelectedIndex = -1;
            invSelectedName = "";
        }

        Program.DrawTextUI("Click item to select  |  Right-click to cancel", invX - 10, invY + 5 * (slotSize + padding) + 25, 14, Color.LightGray);
    }
    else
    {
        // clear selection when inventory closes
        invSelectedIndex = -1;
        invSelectedName = "";
    }
}

static bool WeaponEquipped(string weapon)
{
    return equipped1H == weapon || equipped2H == weapon || armorWeapon == weapon;
}

static bool AddToToolbar(string item)
{
    if (!CanEquipFarmTool(item))
{
    var tier = spadeTiers.FirstOrDefault(t => t.Name == item) ?? wateringCanTiers.FirstOrDefault(t => t.Name == item);
    ShowNotification($"Requires Farming level {tier?.MinLevel ?? 0} to equip {item}.");
    return false;
}
    bool isTool = IsToolItem(item);
    int qty = isTool ? 1 : GetItemCount(item);
    if (qty <= 0) { ShowNotification($"No {item} to move"); return false; }

    // already on toolbar? stack onto it
    for (int i = 0; i < toolbarSlots.Length; i++)
    {
        if (toolbarSlots[i] == item)
        {
            if (!isTool)
            {
                toolbarCounts[i] += qty;
                ZeroOutInventory(item);
            }
            ShowNotification($"Moved {item} to toolbar");
            return true;
        }
    }

    // empty slot
    for (int i = 0; i < toolbarSlots.Length; i++)
    {
        if (toolbarSlots[i] == null)
        {
            toolbarSlots[i] = item;
            toolbarCounts[i] = qty;
            if (!isTool) ZeroOutInventory(item);
            ShowNotification($"Moved {qty}x {item} to toolbar");
            return true;
        }
    }
if (BackpackAdd(item, qty))
{
    if (!isTool) ZeroOutInventory(item);
    ShowNotification($"Toolbar full — {item} added to inventory.");
    return true;
}
ShowNotification("Inventory full!");
return false;
}

static bool AddOneItemToToolbar(string item)
{
    for (int i = 0; i < toolbarSlots.Length; i++)
        if (toolbarSlots[i] == item) { toolbarCounts[i]++; return true; }
    for (int i = 0; i < toolbarSlots.Length; i++)
        if (toolbarSlots[i] == null) { toolbarSlots[i] = item; toolbarCounts[i] = 1; return true; }

    if (BackpackAdd(item, 1))
    {
        ShowNotification($"Toolbar full — {item} added to inventory.");
        return true;
    }
    ShowNotification("Inventory full!");
    return false;
}

static bool ToolbarFull() => toolbarSlots.All(s => s != null);

static bool GiveItemToPlayer(string item)
{
    if (AddOneItemToToolbar(item)) return true;
    // fall back to the stat-based inventory if this item is tracked there
    switch (item)
    {
        case "Cooked Fish": case "Fish":      player.Fish++;       return true;
        case "Logs":        player.Logs++;    return true;
        // add any other stat-backed items you want to accept here
        default:
            // not stat-backed and toolbar full → can't store it
            return false;
    }
}

static void ZeroOutInventory(string item)
{
     var stat = FindStatItem(item);
    if (stat != null) { stat.Delta(-stat.Get()); return; }

    switch (item)
    {
        case "Logs": player.Logs = 0; break;
        case "Birch Logs": player.BirchLogs = 0; break;
        case "Oak Logs": player.OakLogs = 0; break;
        case "Pine Logs": player.PineLogs = 0; break;
        case "Arctic Logs": player.ArcticLogs = 0; break;
        case "Dead Wood": player.DeadWood = 0; break;
        case "Fish": player.Fish = 0; break;
        case "Bones": player.Bones = 0; break;
        case "Fur": player.Fur = 0; break;
        case "Stingers": player.Stingers = 0; break;
        case "Pelts": player.BearPelts = 0; break;
        case "Arrows": player.Arrows = 0; break;
        case "Bolts": player.Bolts = 0; break;
        case "Money": player.Money = 0; break;
        case "Dog Fangs": player.DogFangs = 0; break;
        case "Wolf Claw": player.WolfClaws = 0; break;
        case "Venom Sac": player.VenomSacs = 0; break;
        case "Crab Claw": player.CrabClaws = 0; break;
        case "Bear Claw": player.BearClaws = 0; break;
        case "Crab Shell": player.CrabShells = 0; break;
        case "Shark Fin": player.SharkFins = 0; break;
        case "Shark Tooth": player.SharkTeeth = 0; break;
        case "Snake Skin": player.SnakeSkins = 0; break;
        case "Snake Fang": player.SnakeFangs = 0; break;
        case "Croc Scale": player.CrocScales = 0; break;
        case "Croc Tooth": player.CrocTeeth = 0; break;
        case "Lizard Scale": player.LizardScales = 0; break;
        case "Ember Stone": player.EmberStones = 0; break;
        case "Magma Shard": player.MagmaShards = 0; break;
        case "Lava Core": player.LavaCores = 0; break;
        case "Feather": player.Feathers = 0; break;
        case "Eagle Talon": player.EagleTalons = 0; break;
        case "Horn": player.Horns = 0; break;
        case "Goat Hoof": player.GoatHooves = 0; break;
        default: backpack.Remove(item); break;
    }
}

static void DrawToolbar()
{
    

    int slotSize = 72;   
    int padding = 0;
    int totalW = 8 * (slotSize + padding) - padding;
    int startX = 20;
    int startY = ScreenHeight - slotSize - 20;

    Vector2 mouse = Raylib.GetMousePosition();

    byte tbAlpha = currentPhase == HandPhase.Tools ? (byte)210 : (byte)90;

    for (int i = 0; i < 8; i++)
    {
        int sx = startX + i * (slotSize + padding);
        bool selected = i == toolbarSelectedSlot;

        // slot background
        Raylib.DrawRectangle(sx, startY, slotSize, slotSize, new Color((byte)20,(byte)20,(byte)30, tbAlpha));
        Raylib.DrawRectangleLines(sx, startY, slotSize, slotSize,
            selected ? Color.Gold : new Color((byte)80,(byte)80,(byte)100,(byte)255));

        // selected glow
        if (selected)
            Raylib.DrawRectangle(sx, startY, slotSize, 3, Color.Gold);

        // slot number
        Program.DrawTextUI($"{i + 1}", sx + 5, startY + 4, 15,               
            new Color((byte)120,(byte)120,(byte)140,(byte)255));

        // draw item icon if occupied
        if (toolbarSlots[i] != null)
        {
            DrawToolbarIcon(toolbarSlots[i], sx, startY, slotSize);
            if (!IsToolItem(toolbarSlots[i]))
            DrawInventoryIcon(toolbarSlots[i], sx + 12, startY + 10, slotSize - 24);

             if (!IsToolItem(toolbarSlots[i]) && toolbarCounts[i] > 1)
        {
            string cnt = $"{toolbarCounts[i]}";
            int cw = Program.MeasureTextUI(cnt, 16);                          
            Program.DrawTextUI(cnt, sx + slotSize - cw - 5, startY + 4, 16, Color.White);   
        }
            int nameW = Program.MeasureTextUI(toolbarSlots[i], 13);          
            Program.DrawTextUI(toolbarSlots[i],
                sx + slotSize / 2 - nameW / 2,
                startY + slotSize - 17, 13,                                   
                selected ? Color.Gold : Color.LightGray);

                if (toolbarSlots[i] == "Watering Can")
                {
                    int barW = slotSize - 8;
                    int barX = sx + 4;
                    int barY = startY + slotSize + 3;
                    float pct = Math.Clamp(toolbarWaterCharge[i] / WateringCanMaxUses, 0f, 1f);
                    Raylib.DrawRectangle(barX, barY, barW, 4, new Color((byte)40,(byte)40,(byte)40,(byte)255));
                    Raylib.DrawRectangle(barX, barY, (int)(barW * pct), 4, new Color((byte)70,(byte)140,(byte)200,(byte)255));
                    Raylib.DrawRectangleLines(barX, barY, barW, 4, Color.White);
                }
            
            // ── click to remove while inventory is open ──
            if (player.InventoryOpen)
            {
                Rectangle slotRect = new Rectangle(sx, startY, slotSize, slotSize);
                if (Raylib.CheckCollisionPointRec(mouse, slotRect))
                {
                    Raylib.DrawRectangleLines(sx, startY, slotSize, slotSize, Color.Red);
                    Program.DrawTextUI("Remove", sx - 4, startY - 16, 12, Color.Red);
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        ShowNotification($"Removed {toolbarSlots[i]} from toolbar");
                         RemoveFromToolbar(i);
                    }
                }
            }
        }
    }
    // highlight the toolbar when in Tools phase
    if (currentPhase == HandPhase.Tools)
    {
        Raylib.DrawRectangleLinesEx(
            new Rectangle(startX - 4, startY - 4, totalW + 8, slotSize + 8),
            3, Color.Gold);
    }

    // tooltip — show equipped tool name above toolbar
    if (toolbarSlots[toolbarSelectedSlot] != null)
    {
        string equipped = toolbarSlots[toolbarSelectedSlot];
        int tw = Program.MeasureTextUI($"Equipped: {equipped}", 18);
        Raylib.DrawRectangle(startX, startY - 28, tw + 12, 24,
            new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI($"Equipped: {equipped}", startX + 6, startY - 24, 18, Color.Gold);
    }
}

static void DrawToolbarIcon(string tool, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2 - 6;

    Color? metalTint = null;
    foreach (var m in metalPrefixes)
        if (tool != null && tool.StartsWith(m + " ") && IsToolItem(tool))
        {
            metalTint = m switch {
                "Copper"  => new Color((byte)200,(byte)110,(byte)50,(byte)255),
                "Iron"    => new Color((byte)190,(byte)190,(byte)200,(byte)255),
                "Gold"    => new Color((byte)235,(byte)190,(byte)40,(byte)255),
                _         => new Color((byte)140,(byte)230,(byte)255,(byte)255),   // Crystal
            };
            tool = tool.Substring(m.Length + 1);
            break;
        }

    switch (tool)
    {
        case "Axe":
            Raylib.DrawRectangle(cx - 2, cy - 14, 5, 28,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - 15, cy - 12, 16, 10,
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

        case "Spade":
            Raylib.DrawRectangle(cx - 2, cy - 14, 5, 26, new Color((byte)120,(byte)80,(byte)30,(byte)255)); // handle
            Raylib.DrawTriangle(
                new Vector2(cx - 9, cy + 12), new Vector2(cx + 9, cy + 12), new Vector2(cx, cy + 26),
                new Color((byte)165,(byte)165,(byte)175,(byte)255)); // blade
            break;

        case "Watering Can":
            Raylib.DrawRectangle(cx - 11, cy - 4, 22, 15, new Color((byte)70,(byte)140,(byte)200,(byte)255)); // body
            Raylib.DrawRectangle(cx + 9, cy - 1, 13, 5, new Color((byte)70,(byte)140,(byte)200,(byte)255));   // spout
            Raylib.DrawRectangle(cx - 5, cy - 14, 11, 8, new Color((byte)50,(byte)110,(byte)170,(byte)255));  // handle
            break;
    }
    if (metalTint != null)
    {
        Raylib.DrawCircle(x + size - 10, y + 10, 6, metalTint.Value);
        Raylib.DrawCircleLines(x + size - 10, y + 10, 6, Color.Black);
    }
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
    // sync the combat column with the gear-menu weapon slot
if (slot == "WEAPON")
{
    if (IsTwoHandedWeapon(item) || IsRangedWeapon(item))
    {
        equipped2H = item; equipped1H = null;
        if (item.Contains("Crossbow")) player.HasCrossbow = true;
        else if (item.Contains("Bow")) player.HasBow = true;
    }
    else
    {
        equipped1H = item; equipped2H = null;
    }
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
        case "WEAPON":
        if (armorWeapon == "Sword") swordPickedUp = true;
        else if (armorWeapon == "Stick") stickPickedUp = true;
        else if (armorWeapon == "Bow") player.HasBow = true;
        else if (armorWeapon == "Crossbow") player.HasCrossbow = true;
        else if (armorWeapon != null) player.AddGear(armorWeapon);  // other weapons go back to owned gear
        armorWeapon = null;
        equipped1H = null;
        equipped2H = null;
        break;
        case "SHIELD": armorShield = null; break;
        case "AMMO": equippedAmmo = null; break;
    }
}

static string GetItemSlot(string item)
{
    if (item == null) return null;

    // weapons (check specific before general)
    if (item.Contains("Great Sword") || item.Contains("War Axe")
        || item.Contains("Sword") || item.Contains("Stick")
        || item.Contains("Bow") || item.Contains("Crossbow")
        || item.EndsWith("Staff"))  
        return "WEAPON";

    if (item.EndsWith("Hat"))                                                           return "HELMET";  
    if (item.EndsWith("Top") || item.EndsWith("Tunic"))                                 return "BODY";    
    if (item.EndsWith("Bottoms") || item.EndsWith("Chaps"))                             return "LEGS";
    if (item.EndsWith("Bracers"))                                                       return "GLOVES";  
    if (item.EndsWith("Quiver"))                                                        return "CAPE";    
    if (item.EndsWith("Book"))                                                          return "SHIELD";  
    if (item.Contains("Helmet") || item.EndsWith("Cap"))                                return "HELMET";
    if (item.Contains("Chestplate") || item.Contains("Vest") || item.Contains("Body"))  return "BODY";
    if (item.Contains("Leggings") || item.Contains("Pants") || item.Contains("Legs"))   return "LEGS";
    if (item.Contains("Boots"))                                                         return "BOOTS";
    if (item.Contains("Gauntlets") || item.Contains("Gloves"))                          return "GLOVES";
    if (item.Contains("Cape"))                                                          return "CAPE";
    if (item.Contains("Shield"))                                                        return "SHIELD";

    return null;
}

public static string GetEquippedTool() => toolbarSlots[toolbarSelectedSlot];

static void DrawDroppedItems()
{
    foreach (var d in droppedItems)
    {
        bool flashing = d.Lifetime < 5f && (int)(d.Lifetime * 6) % 2 == 0;
        if (!flashing)
        {
            Raylib.DrawCircle((int)d.Position.X, (int)d.Position.Y + 6, 14, new Color((byte)0,(byte)0,(byte)0,(byte)80));
            DrawInventoryIcon(d.Name, (int)d.Position.X - 16, (int)d.Position.Y - 16, 32);
        }

        if (Vector2.Distance(player.Center, d.Position) < 50)
        {
            Program.DrawTextUI($"F = Pick up {d.Name}", (int)d.Position.X - 50, (int)d.Position.Y - 36, 14, Color.White);
            Program.DrawTextUI($"{(int)d.Lifetime}s", (int)d.Position.X - 8, (int)d.Position.Y + 18, 12, Color.Gray);
        }
    }
}

static void DrawDropQuantity()
{
    if (!dropQtyOpen) return;

    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)150));

    int w = 440, h = 200;
    int px = ScreenWidth / 2 - w / 2;
    int py = ScreenHeight / 2 - h / 2;
    Raylib.DrawRectangle(px, py, w, h, new Color((byte)20,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(px, py, w, h, Color.Gold);

    string title = $"Drop how many {dropQtyItem}?";
    int tw = Program.MeasureTextUI(title, 24);
    Program.DrawTextUI(title, ScreenWidth / 2 - tw / 2, py + 20, 24, Color.Gold);
    Program.DrawTextUI($"You have: {dropQtyAvailable}", ScreenWidth / 2 - 60, py + 56, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // three buttons: 1, 5, All
    (string label, int amount)[] opts =
    {
        ("1", 1),
        ("5", Math.Min(5, dropQtyAvailable)),
        ("All", dropQtyAvailable),
    };

    int btnW = 110, btnH = 50, gap = 20;
    int totalW = btnW * 3 + gap * 2;
    int startX = ScreenWidth / 2 - totalW / 2;
    int btnY = py + 100;

    for (int i = 0; i < opts.Length; i++)
    {
        int bx = startX + i * (btnW + gap);
        Rectangle btn = new Rectangle(bx, btnY, btnW, btnH);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        Raylib.DrawRectangleRec(btn, hover ? new Color((byte)60,(byte)50,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : Color.White);
        int lw = Program.MeasureTextUI(opts[i].label, 22);
        Program.DrawTextUI(opts[i].label, bx + btnW / 2 - lw / 2, btnY + 14, 22, hover ? Color.Gold : Color.White);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left) && opts[i].amount > 0)
        {
            DropAmountToGround(dropQtyItem, opts[i].amount);
            dropQtyOpen = false;
            dropQtyItem = "";
            invSelectedIndex = -1;
            invSelectedName = "";
        }
    }

    // cancel
    Rectangle cancelBtn = new Rectangle(ScreenWidth / 2 - 50, py + h - 30, 100, 24);
    bool hCancel = Raylib.CheckCollisionPointRec(mouse, cancelBtn);
    Program.DrawTextUI("Cancel", ScreenWidth / 2 - 28, py + h - 28, 18, hCancel ? Color.Gold : Color.Gray);
    if (hCancel && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        dropQtyOpen = false;
        dropQtyItem = "";
    }
}

static void DropAmountToGround(string item, int amount)
{
    if (amount <= 0) return;

    if (item == "Money")
    {
        amount = Math.Min(amount, player.Money);
        player.Money -= amount;
    }
    else
    {
        amount = Math.Min(amount, GetItemCount(item));
        for (int i = 0; i < amount; i++)
            RemoveOneItem(item);
    }

    droppedItems.Add(new DroppedItem(item, amount, player.Position + new Vector2(0, 20)));
    ShowNotification($"Dropped {amount}x {item}");
}

static void ApplyHallItem(HallItem item)
{
    switch (item.Category)
    {
        case "Tops":
        case "Outerwear":
            player.ShirtColor = item.Primary;
            break;
        case "Bottoms":
            player.PantsColor = item.Primary;
            break;
        // Accessories don't change ShirtColor/PantsColor — 
        // you can wire these to hat/chain fields later
    }
}

static void AddDropZone(float x, float y)
{
    var dropzone = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(40, 20, 70, 255),           // purple exterior
        new Color(25, 20, 45, 255),           // dark purple interior
        new Vector2(x + 100, y + 100),
        "DropZone",
        new NPC(new Vector2(600, 600), "Arcade Attendant", "Welcome to DropZone! Games, prizes and snacks await."),
        entryPos: new Vector2(620, 830)
    );

    dropzone.InteriorObjects.Clear();
    foreach (var kv in dzMachines)
    {
        // skip counters that shouldn't block movement
        if (kv.Key == "prize" || kv.Key == "food") continue;
        Vector2 c = kv.Value.Center;
        dropzone.InteriorObjects.Add(new Rectangle(c.X - 35, c.Y - 55, 70, 110));
    }
    // decorative divider only (not a machine)
    dropzone.InteriorObjects.Add(new Rectangle(360, 150, 8, 400));

    buildings.Add(dropzone);
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
    Program.DrawTextUI(title, px + 12, py + 10, 24, Color.Gold);
    Program.DrawTextUI($"{inventory.Count(s => s != null)}/{capacity}", px + panelW - 60, py + 12, 18, Color.LightGray);

    // aggregate cart contents into stacks (name -> count)
    var stacks = new List<(string name, int count)>();
    foreach (string entry in inventory)
    {
        if (string.IsNullOrEmpty(entry)) continue;
        int idx = stacks.FindIndex(t => t.name == entry);
        if (idx >= 0) stacks[idx] = (entry, stacks[idx].count + 1);
        else stacks.Add((entry, 1));
    }

    for (int i = 0; i < capacity; i++)
    {
        int col = i % cols;
        int row = i / cols;
        int sx = px + 10 + col * (slotSize + pad);
        int sy = py + 44 + row * (slotSize + pad);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (i < stacks.Count)
        {
            DrawInventoryIcon(stacks[i].name, sx + 4, sy + 4, slotSize - 8);
            if (stacks[i].count > 1)
                Program.DrawTextUI($"{stacks[i].count}", sx + 4, sy + 4, 16, Color.White);
            string shortName = stacks[i].name.Length > 7 ? stacks[i].name.Substring(0, 7) : stacks[i].name;
            Program.DrawTextUI(shortName, sx + 2, sy + slotSize - 14, 10, Color.White);
        }
    }

    Program.DrawTextUI("I = Close", px + panelW / 2 - 40, py + panelH - 22, 16, Color.LightGray);
}

static void BuyShopItem(string item, int price, int qty)
{
    if (player.Money < price)
    {
        shopMessage = $"Need ${price} for {item}";
        shopMessageTimer = 1.5f;
        return;
    }

    if (item == "Arrows" || item == "Bolts")
    {
        if (!TryGiveItem(item, qty))
        {
            shopMessage = "Inventory full!";
            shopMessageTimer = 1.5f;
            return;
        }
        player.Money -= price;
        shopMessage = $"Bought {item}!";
        shopMessageTimer = 1.5f;
        return;
    }

    player.Money -= price;
    switch (item)
    {
        case "Apple":   player.Health = Math.Min(player.MaxHealth, player.Health + 10); break;
        case "Bread":   player.Health = Math.Min(player.MaxHealth, player.Health + 20); break;
        case "Bandage": player.Health = Math.Min(player.MaxHealth, player.Health + 40); break;
        case "Torch":   torchPickedUp = true; break;
    }
    shopMessage = $"Bought {item}!";
    shopMessageTimer = 1.5f;
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
        case "Wolf Claw": player.WolfClaws -= amount; break;
        case "Venom Sac": player.VenomSacs -= amount; break;
        case "Crab Claw": player.CrabClaws -= amount; break;
        case "Bear Claw": player.BearClaws -= amount; break;
        case "Crab Shell": player.CrabShells -= amount; break;
        case "Shark Fin": player.SharkFins -= amount; break;
        case "Shark Tooth": player.SharkTeeth -= amount; break;
        case "Snake Skin": player.SnakeSkins -= amount; break;
        case "Snake Fang": player.SnakeFangs -= amount; break;
        case "Croc Scale": player.CrocScales -= amount; break;
        case "Croc Tooth": player.CrocTeeth -= amount; break;
        case "Lizard Scale": player.LizardScales -= amount; break;
        case "Ember Stone": player.EmberStones -= amount; break;
        case "Magma Shard": player.MagmaShards -= amount; break;
        case "Lava Core": player.LavaCores -= amount; break;
        case "Feather": player.Feathers -= amount; break;
        case "Eagle Talon": player.EagleTalons -= amount; break;
        case "Horn": player.Horns -= amount; break;
        case "Goat Hoof": player.GoatHooves -= amount; break;
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
        "Arrows" => player.Arrows,              
        "Bolts" => player.Bolts,               
        "Arcane Essence" => player.ArcaneEssence,
        "Stingers" => player.Stingers,
        "Pelts" => player.BearPelts,
        "Dog Fangs" => player.DogFangs,
        "Wolf Claw" => player.WolfClaws,
        "Venom Sac" => player.VenomSacs,
        "Crab Claw" => player.CrabClaws,
        "Bear Claw" => player.BearClaws,
        "Crab Shell" => player.CrabShells,
        "Shark Fin" => player.SharkFins,
        "Shark Tooth" => player.SharkTeeth,
        "Snake Skin" => player.SnakeSkins,
        "Snake Fang" => player.SnakeFangs,
        "Croc Scale" => player.CrocScales,
        "Croc Tooth" => player.CrocTeeth,
        "Lizard Scale" => player.LizardScales,
        "Ember Stone" => player.EmberStones,
        "Magma Shard" => player.MagmaShards,
        "Lava Core" => player.LavaCores,
        "Feather" => player.Feathers,
        "Eagle Talon" => player.EagleTalons,
        "Horn" => player.Horns,
        "Goat Hoof" => player.GoatHooves,
        "Stone" => player.StoneOre,
        "Copper Ore" => player.CopperOre,
        "Iron Ore" => player.IronOre,
        "Gold Ore" => player.GoldOre,
        "Crystal" => player.Crystals,
        "Money" => player.Money,
        _ when itemName != null && itemName.EndsWith("Staff") => player.OwnedGear.Contains(itemName) ? 1 : 0,
        _ => GetToolbarCount(itemName) + BackpackCount(itemName)
    };
}

static int GetToolbarCount(string item)
{
    for (int i = 0; i < toolbarSlots.Length; i++)
        if (toolbarSlots[i] == item) return toolbarCounts[i];
    return 0;
}

static void DrawInventoryIcon(string item, int x, int y, int size)
{
    if (item == "Colossus Egg" || item == "Titan Egg")
    {
        DrawEggIcon(item, x + size / 2, y + size / 2, size);
        return;
    }

    Color? metalTint = null;
    foreach (var m in metalPrefixes)
        if (item != null && item.StartsWith(m + " ") && IsToolItem(item))
        {
            metalTint = m switch {
                "Copper"  => new Color((byte)200,(byte)110,(byte)50,(byte)255),
                "Iron"    => new Color((byte)190,(byte)190,(byte)200,(byte)255),
                "Gold"    => new Color((byte)235,(byte)190,(byte)40,(byte)255),
                _         => new Color((byte)140,(byte)230,(byte)255,(byte)255),   // Crystal
            };
            item = item.Substring(m.Length + 1);
            break;
        }

    float s = size / 80f;              // scale factor (icons designed for 80px)
    int cx = x + size / 2;
    int cy = y + size / 2 - (int)(10 * s);
    int I(float v) => (int)(v * s);    // helper: scale a value

    string drawName = ResolveCookedIcon(item, out Color cookedTint);
    item = drawName; 
    if (item.EndsWith("Staff"))
    {
        Color orb = GetStaffColor(item);
        bool great = item.Contains("Great");
        int shaftTop = great ? I(18) : I(12);
        int shaftLen = great ? I(40) : I(30);
        int orbR     = great ? I(9)  : I(7);
        int orbY     = great ? I(22) : I(16);

        Raylib.DrawRectangle(cx - I(2), cy - shaftTop, I(great ? 5 : 4), shaftLen, new Color((byte)105,(byte)75,(byte)42,(byte)255));
        Raylib.DrawCircle(cx, cy - orbY, orbR + I(3), new Color(orb.R, orb.G, orb.B, (byte)110));
        Raylib.DrawCircle(cx, cy - orbY, orbR, orb);
        Raylib.DrawCircle(cx - I(2), cy - orbY - I(2), I(3), Color.White);
        if (great) Raylib.DrawCircle(cx, cy + I(16), I(5), orb);
        return;
    }
    if (item == "Arcane Essence")
    {
        Raylib.DrawCircle(cx, cy, I(16), new Color((byte)100,(byte)40,(byte)160,(byte)120));
        Raylib.DrawCircle(cx, cy, I(11), new Color((byte)160,(byte)80,(byte)255,(byte)255));
        Raylib.DrawCircle(cx - I(3), cy - I(3), I(4), Color.White);
        return;
    }

    switch (item)
    {
        case "Logs":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), Color.Brown);
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)100,(byte)60,(byte)20,(byte)255));
            break;

        case "Birch Logs":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), Color.White);
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)200,(byte)200,(byte)200,(byte)255));
            Raylib.DrawRectangle(cx - I(10), cy - I(6), I(4), I(4), Color.DarkGray);
            Raylib.DrawRectangle(cx + I(6), cy - I(2), I(4), I(4), Color.DarkGray);
            break;

        case "Oak Logs":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), new Color((byte)101,(byte)67,(byte)33,(byte)255));
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)80,(byte)50,(byte)20,(byte)255));
            Raylib.DrawCircle(cx - I(18), cy, I(6), new Color((byte)101,(byte)67,(byte)33,(byte)255));
            Raylib.DrawCircle(cx + I(18), cy, I(6), new Color((byte)101,(byte)67,(byte)33,(byte)255));
            break;

        case "Pine Logs":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), new Color((byte)120,(byte)80,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)0,(byte)80,(byte)0,(byte)255));
            break;

        case "Arctic Logs":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), new Color((byte)180,(byte)210,(byte)230,(byte)255));
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)220,(byte)235,(byte)255,(byte)255));
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(5), new Color((byte)220,(byte)235,(byte)255,(byte)180));
            break;

        case "Dead Wood":
            Raylib.DrawRectangle(cx - I(20), cy - I(8), I(40), I(16), Color.DarkGray);
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(6), new Color((byte)80,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangle(cx - I(8), cy - I(6), I(4), I(12), new Color((byte)60,(byte)60,(byte)60,(byte)255));
            break;

        case "Fish":
            Raylib.DrawTriangle(
                new Vector2(cx + I(20), cy),
                new Vector2(cx + I(8), cy - I(8)),
                new Vector2(cx + I(8), cy + I(8)),
                Color.SkyBlue);
            Raylib.DrawEllipse(cx - I(6), cy, I(18), I(10), Color.Blue);
            break;

        case "Bones":
            Raylib.DrawRectangle(cx - I(3), cy - I(18), I(6), I(36), Color.White);
            Raylib.DrawCircle(cx, cy - I(18), I(7), Color.White);
            Raylib.DrawCircle(cx, cy + I(18), I(7), Color.White);
            break;

        case "Fur":
            Raylib.DrawCircle(cx, cy, I(18), new Color((byte)139,(byte)90,(byte)43,(byte)255));
            Raylib.DrawCircle(cx - I(8), cy - I(8), I(8), new Color((byte)160,(byte)110,(byte)60,(byte)255));
            Raylib.DrawCircle(cx + I(8), cy - I(8), I(8), new Color((byte)160,(byte)110,(byte)60,(byte)255));
            break;

        case "Stingers":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(22)),
                new Vector2(cx - I(8), cy + I(18)),
                new Vector2(cx + I(8), cy + I(18)),
                new Color((byte)180,(byte)120,(byte)0,(byte)255));
            break;

        case "Pelts":
            Raylib.DrawRectangle(cx - I(16), cy - I(18), I(32), I(36), new Color((byte)100,(byte)100,(byte)120,(byte)255));
            Raylib.DrawRectangle(cx - I(10), cy - I(10), I(20), I(20), new Color((byte)130,(byte)130,(byte)150,(byte)255));
            break;

        case "Money":
            Raylib.DrawCircle(cx, cy, I(18), Color.Gold);
            Program.DrawTextUI("$", cx - I(6), cy - I(12), I(22), Color.DarkGray);
            break;
        
        case "Wheat Seeds":
            Raylib.DrawRectangle(cx - I(9), cy - I(6), I(18), I(14), new Color((byte)180,(byte)140,(byte)60,(byte)255));
            Raylib.DrawRectangle(cx - I(9), cy - I(10), I(18), I(5), new Color((byte)140,(byte)100,(byte)40,(byte)255));
            break;
        case "Carrot Seeds":
            Raylib.DrawRectangle(cx - I(9), cy - I(6), I(18), I(14), new Color((byte)220,(byte)130,(byte)50,(byte)255));
            Raylib.DrawRectangle(cx - I(9), cy - I(10), I(18), I(5), new Color((byte)170,(byte)95,(byte)35,(byte)255));
            break;
        case "Potato Seeds":
            Raylib.DrawRectangle(cx - I(9), cy - I(6), I(18), I(14), new Color((byte)190,(byte)150,(byte)90,(byte)255));
            Raylib.DrawRectangle(cx - I(9), cy - I(10), I(18), I(5), new Color((byte)140,(byte)105,(byte)60,(byte)255));
            break;
        case "Tomato Seeds":
            Raylib.DrawRectangle(cx - I(9), cy - I(6), I(18), I(14), new Color((byte)210,(byte)80,(byte)60,(byte)255));
            Raylib.DrawRectangle(cx - I(9), cy - I(10), I(18), I(5), new Color((byte)160,(byte)55,(byte)40,(byte)255));
            break;

        case "Axe":
            Raylib.DrawRectangle(cx - I(2), cy - I(14), I(5), I(28), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - I(15), cy - I(12), I(16), I(10), new Color((byte)160,(byte)160,(byte)170,(byte)255));  
            break;

        case "Stick":
            Raylib.DrawRectangle(cx - I(2), cy - I(18), I(5), I(36), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawRectangle(cx - I(1), cy - I(20), I(3), I(6), new Color((byte)100,(byte)60,(byte)20,(byte)255));
            break;

        case "Sword":
            Raylib.DrawRectangle(cx - I(2), cy - I(18), I(5), I(24), new Color((byte)180,(byte)190,(byte)200,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx - I(2), cy - I(18)),
                new Vector2(cx + I(3), cy - I(18)),
                new Vector2(cx, cy - I(26)),
                new Color((byte)200,(byte)210,(byte)220,(byte)255));
            Raylib.DrawRectangle(cx - I(10), cy + I(6), I(21), I(5), new Color((byte)180,(byte)140,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - I(2), cy + I(11), I(5), I(12), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawCircle(cx, cy + I(23), I(4), new Color((byte)180,(byte)140,(byte)40,(byte)255));
            break;
        case "Staff":
            // shorter shaft (1H)
            Raylib.DrawRectangle(cx - I(2), cy - I(12), I(4), I(30), new Color((byte)110,(byte)80,(byte)45,(byte)255));
            // glowing orb at top
            Raylib.DrawCircle(cx, cy - I(16), I(10), new Color((byte)160,(byte)120,(byte)200,(byte)120));
            Raylib.DrawCircle(cx, cy - I(16), I(7), new Color((byte)160,(byte)120,(byte)200,(byte)255));
            Raylib.DrawCircle(cx - I(2), cy - I(18), I(3), Color.White);
            break;

        case "Great Staff":
            // long shaft (2H)
            Raylib.DrawRectangle(cx - I(2), cy - I(18), I(5), I(40), new Color((byte)100,(byte)70,(byte)40,(byte)255));
            // large glowing orb at top
            Raylib.DrawCircle(cx, cy - I(22), I(13), new Color((byte)160,(byte)120,(byte)200,(byte)110));
            Raylib.DrawCircle(cx, cy - I(22), I(9), new Color((byte)160,(byte)120,(byte)200,(byte)255));
            Raylib.DrawCircle(cx - I(3), cy - I(24), I(3), Color.White);
            // base orb to mark the 2H version
            Raylib.DrawCircle(cx, cy + I(16), I(5), new Color((byte)160,(byte)120,(byte)200,(byte)255));
            break;

        case "Arcane Essence":
            // glowing purple orb with sparkle
            Raylib.DrawCircle(cx, cy, I(16), new Color((byte)100,(byte)40,(byte)160,(byte)120));
            Raylib.DrawCircle(cx, cy, I(11), new Color((byte)160,(byte)80,(byte)255,(byte)255));
            Raylib.DrawCircle(cx - I(3), cy - I(3), I(4), Color.White);
            break;

        case "Bow":
            // vertical stave + string (larger)
            Raylib.DrawLineEx(new Vector2(cx, cy - I(20)), new Vector2(cx, cy + I(20)), Math.Max(1, I(4)),
                new Color((byte)140,(byte)90,(byte)40,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx + I(7), cy - I(17)), new Vector2(cx + I(7), cy + I(17)), Math.Max(1, I(1)),
                new Color((byte)220,(byte)220,(byte)220,(byte)255));
            // curved tips
            Raylib.DrawLineEx(new Vector2(cx, cy - I(20)), new Vector2(cx - I(6), cy - I(12)), Math.Max(1, I(3)),
                new Color((byte)110,(byte)70,(byte)30,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy + I(20)), new Vector2(cx - I(6), cy + I(12)), Math.Max(1, I(3)),
                new Color((byte)110,(byte)70,(byte)30,(byte)255));
            break;

        case "Crossbow":
            // horizontal bow arm + vertical stock + bolt tip (larger)
            Raylib.DrawLineEx(new Vector2(cx - I(16), cy - I(6)), new Vector2(cx + I(16), cy - I(6)), Math.Max(1, I(4)),
                new Color((byte)90,(byte)60,(byte)30,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy - I(12)), new Vector2(cx, cy + I(16)), Math.Max(1, I(5)),
                new Color((byte)90,(byte)60,(byte)30,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(18)),
                new Vector2(cx - I(5), cy - I(9)),
                new Vector2(cx + I(5), cy - I(9)),
                Color.Gray);
            break;

        case "Arrows":
            Raylib.DrawLineEx(new Vector2(x + I(16), y + size - I(16)), new Vector2(x + size - I(16), y + I(16)), Math.Max(1, I(2)), new Color((byte)160,(byte)120,(byte)70,(byte)255));
            Raylib.DrawTriangle(new Vector2(x + size - I(16), y + I(16)), new Vector2(x + size - I(24), y + I(18)), new Vector2(x + size - I(18), y + I(24)), Color.Gray);
            break;

        case "Bolts":
            Raylib.DrawLineEx(new Vector2(x + I(16), y + size - I(16)), new Vector2(x + size - I(16), y + I(16)), Math.Max(1, I(3)), new Color((byte)120,(byte)90,(byte)50,(byte)255));
            break;
        case "Pickaxe":
            Raylib.DrawRectangle(cx - I(2), cy - I(12), I(5), I(26), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLine(cx - I(12), cy - I(14), cx + I(12), cy - I(14), new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx - I(12), cy - I(14)),
                new Vector2(cx - I(16), cy - I(6)),
                new Vector2(cx - I(4), cy - I(14)),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx + I(12), cy - I(14)),
                new Vector2(cx + I(16), cy - I(6)),
                new Vector2(cx + I(4), cy - I(14)),
                new Color((byte)140,(byte)140,(byte)150,(byte)255));
            break;

        case "Rod":
            Raylib.DrawRectangle(cx - I(2), cy - I(16), I(4), I(30), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLine(cx + I(2), cy - I(16), cx + I(16), cy, new Color((byte)200,(byte)200,(byte)200,(byte)255));
            Raylib.DrawCircle(cx + I(16), cy, I(3), new Color((byte)100,(byte)180,(byte)220,(byte)255));
            break;

        case "Net":
            for (int n = 0; n < 3; n++)
                Raylib.DrawLine(cx, cy - I(14), cx - I(14) + I(14 * n), cy + I(8), new Color((byte)180,(byte)160,(byte)80,(byte)255));
            for (int n = 0; n < 2; n++)
                Raylib.DrawLine(cx - I(14), cy - I(6) + I(10 * n), cx + I(14), cy - I(6) + I(10 * n), new Color((byte)180,(byte)160,(byte)80,(byte)255));
            break;

        case "Torch":
            Raylib.DrawRectangle(cx - I(3), cy - I(8), I(6), I(22), new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawCircle(cx, cy - I(10), I(5), new Color((byte)255,(byte)180,(byte)0,(byte)255));
            Raylib.DrawCircle(cx, cy - I(10), I(8), new Color((byte)255,(byte)100,(byte)0,(byte)80));
            break;
        case "Dog Fangs":
        case "Snake Fang":
        case "Croc Tooth":
        case "Shark Tooth":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(18)),
                new Vector2(cx - I(8), cy + I(14)),
                new Vector2(cx + I(8), cy + I(14)),
                Color.White);
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(10)),
                new Vector2(cx - I(4), cy + I(8)),
                new Vector2(cx + I(4), cy + I(8)),
                new Color((byte)220,(byte)220,(byte)220,(byte)255));
            break;

        case "Wolf Claw":
        case "Bear Claw":
        case "Crab Claw":
        case "Eagle Talon":
            Raylib.DrawLineEx(new Vector2(cx - I(10), cy + I(14)), new Vector2(cx + I(8), cy - I(16)), Math.Max(1, I(4)),
                new Color((byte)230,(byte)230,(byte)210,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx + I(8), cy - I(16)), new Vector2(cx + I(14), cy - I(10)), Math.Max(1, I(4)),
                new Color((byte)230,(byte)230,(byte)210,(byte)255));
            break;

        case "Venom Sac":
            Raylib.DrawCircle(cx, cy, I(16), new Color((byte)80,(byte)200,(byte)80,(byte)255));
            Raylib.DrawCircle(cx - I(5), cy - I(5), I(5), new Color((byte)140,(byte)240,(byte)140,(byte)255));
            break;

        case "Crab Shell":
        case "Croc Scale":
        case "Lizard Scale":
            Raylib.DrawRectangle(cx - I(16), cy - I(14), I(32), I(28), new Color((byte)120,(byte)160,(byte)90,(byte)255));
            Raylib.DrawRectangle(cx - I(10), cy - I(8), I(20), I(16), new Color((byte)150,(byte)190,(byte)110,(byte)255));
            break;

        case "Shark Fin":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(20)),
                new Vector2(cx - I(16), cy + I(16)),
                new Vector2(cx + I(16), cy + I(16)),
                new Color((byte)150,(byte)150,(byte)170,(byte)255));
            break;

        case "Snake Skin":
            Raylib.DrawRectangle(cx - I(14), cy - I(14), I(28), I(28), new Color((byte)110,(byte)160,(byte)70,(byte)255));
            Raylib.DrawCircle(cx - I(6), cy - I(4), I(3), new Color((byte)70,(byte)110,(byte)40,(byte)255));
            Raylib.DrawCircle(cx + I(6), cy + I(4), I(3), new Color((byte)70,(byte)110,(byte)40,(byte)255));
            break;

        case "Ember Stone":
        case "Lava Core":
            Raylib.DrawCircle(cx, cy, I(16), new Color((byte)255,(byte)100,(byte)0,(byte)255));
            Raylib.DrawCircle(cx, cy, I(8), new Color((byte)255,(byte)200,(byte)40,(byte)255));
            break;

        case "Magma Shard":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(18)),
                new Vector2(cx - I(14), cy + I(14)),
                new Vector2(cx + I(14), cy + I(14)),
                new Color((byte)200,(byte)60,(byte)20,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(8)),
                new Vector2(cx - I(6), cy + I(8)),
                new Vector2(cx + I(6), cy + I(8)),
                new Color((byte)255,(byte)140,(byte)40,(byte)255));
            break;

        case "Feather":
            Raylib.DrawLineEx(new Vector2(cx, cy - I(18)), new Vector2(cx, cy + I(18)), Math.Max(1, I(4)),
                new Color((byte)220,(byte)220,(byte)240,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy - I(14)), new Vector2(cx - I(8), cy - I(6)), Math.Max(1, I(2)),
                new Color((byte)200,(byte)200,(byte)230,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy - I(8)), new Vector2(cx + I(8), cy - I(0)), Math.Max(1, I(2)),
                new Color((byte)200,(byte)200,(byte)230,(byte)255));
            break;

        case "Horn":
            Raylib.DrawTriangle(
                new Vector2(cx - I(4), cy + I(16)),
                new Vector2(cx + I(4), cy + I(16)),
                new Vector2(cx + I(10), cy - I(18)),
                new Color((byte)220,(byte)210,(byte)180,(byte)255));
            break;

        case "Goat Hoof":
            Raylib.DrawRectangle(cx - I(8), cy - I(12), I(16), I(24), new Color((byte)90,(byte)70,(byte)50,(byte)255));
            Raylib.DrawRectangle(cx - I(2), cy - I(4), I(4), I(16), new Color((byte)40,(byte)30,(byte)20,(byte)255));
            break;
        case "Shopping Bag":
            Raylib.DrawRectangle(cx - I(12), cy - I(10), I(24), I(20), new Color((byte)80,(byte)180,(byte)80,(byte)255));
            Raylib.DrawRectangle(cx - I(8),  cy - I(16), I(16), I(8), new Color((byte)60,(byte)140,(byte)60,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx - I(5), cy - I(18)), new Vector2(cx + I(5), cy - I(18)), 2, new Color((byte)80,(byte)100,(byte)80,(byte)255));
            break;
        case "Ashes":
            Raylib.DrawCircle(cx, cy + I(4), I(14), new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawCircle(cx - I(8), cy + I(6), I(8), new Color((byte)80,(byte)80,(byte)80,(byte)255));
            Raylib.DrawCircle(cx + I(8), cy + I(6), I(7), new Color((byte)70,(byte)70,(byte)70,(byte)255));
            break;
        case "Raw Meat":
            // pink steak body
            Raylib.DrawEllipse(cx, cy + I(2), I(18), I(13), new Color((byte)220,(byte)110,(byte)120,(byte)255));
            // darker outer rim
            Raylib.DrawEllipseLines(cx, cy + I(2), I(18), I(13), new Color((byte)150,(byte)60,(byte)70,(byte)255));
            // fat marbling streaks
            Raylib.DrawRectangle(cx - I(8), cy - I(2), I(16), I(2), new Color((byte)240,(byte)200,(byte)200,(byte)200));
            Raylib.DrawRectangle(cx - I(6), cy + I(5), I(12), I(2), new Color((byte)240,(byte)200,(byte)200,(byte)200));
            // white bone nub on the side
            Raylib.DrawCircle(cx - I(18), cy + I(2), I(5), Color.White);
            break;
        case "Apple":
            Raylib.DrawCircle(cx, cy, I(11), new Color((byte)210,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - I(1), cy - I(15), I(2), I(5), new Color((byte)90,(byte)60,(byte)30,(byte)255));
            break;
        case "Banana":
            Raylib.DrawEllipse(cx, cy, I(6), I(16), new Color((byte)235,(byte)210,(byte)60,(byte)255));
            break;
        case "Wheat":
            Raylib.DrawRectangle(cx - I(2), cy - I(14), I(4), I(24), new Color((byte)200,(byte)170,(byte)60,(byte)255));
            Raylib.DrawEllipse(cx, cy - I(16), I(6), I(10), new Color((byte)230,(byte)200,(byte)80,(byte)255));
            break;
        case "Carrot":
            Raylib.DrawTriangle(
                new Vector2(cx, cy - I(12)), new Vector2(cx - I(6), cy + I(10)), new Vector2(cx + I(6), cy + I(10)),
                new Color((byte)230,(byte)120,(byte)30,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy - I(12)), new Vector2(cx - I(4), cy - I(22)), I(2), new Color((byte)70,(byte)160,(byte)60,(byte)255));
            Raylib.DrawLineEx(new Vector2(cx, cy - I(12)), new Vector2(cx + I(4), cy - I(22)), I(2), new Color((byte)70,(byte)160,(byte)60,(byte)255));
            break;
        case "Tomato":
            Raylib.DrawCircle(cx, cy, I(11), new Color((byte)220,(byte)50,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx - I(2), cy - I(15), I(4), I(5), new Color((byte)70,(byte)130,(byte)55,(byte)255));
            break;
        case "Potato":
            // tan oval tuber
            Raylib.DrawEllipse(cx, cy + I(2), I(16), I(12), new Color((byte)190,(byte)150,(byte)90,(byte)255));
            Raylib.DrawEllipseLines(cx, cy + I(2), I(16), I(12), new Color((byte)150,(byte)115,(byte)65,(byte)255));
            // eyes/spots
            Raylib.DrawCircle(cx - I(6), cy - I(2), I(2), new Color((byte)120,(byte)90,(byte)50,(byte)255));
            Raylib.DrawCircle(cx + I(5), cy + I(4), I(2), new Color((byte)120,(byte)90,(byte)50,(byte)255));
            Raylib.DrawCircle(cx + I(2), cy - I(5), I(1), new Color((byte)120,(byte)90,(byte)50,(byte)255));
            break;

        case "Corn":
            // yellow cob
            Raylib.DrawEllipse(cx, cy + I(2), I(9), I(18), new Color((byte)240,(byte)210,(byte)40,(byte)255));
            // kernel rows (vertical + horizontal grid)
            for (int ky = -14; ky <= 14; ky += 5)
                Raylib.DrawRectangle(cx - I(8), cy + I(2 + ky), I(16), I(1), new Color((byte)200,(byte)165,(byte)20,(byte)180));
            for (int kx = -6; kx <= 6; kx += 4)
                Raylib.DrawRectangle(cx + I(kx), cy - I(16), I(1), I(36), new Color((byte)200,(byte)165,(byte)20,(byte)180));
            // green husk leaf at the base
            Raylib.DrawTriangle(
                new Vector2(cx - I(4), cy + I(18)),
                new Vector2(cx + I(4), cy + I(18)),
                new Vector2(cx - I(10), cy + I(26)),
                new Color((byte)60,(byte)160,(byte)60,(byte)255));
            break;
        case "Workbench":
            Raylib.DrawRectangle(cx - I(20), cy - I(6), I(40), I(10), new Color((byte)140,(byte)100,(byte)60,(byte)255));
            Raylib.DrawRectangle(cx - I(16), cy + I(4), I(6), I(12), new Color((byte)100,(byte)70,(byte)40,(byte)255));
            Raylib.DrawRectangle(cx + I(10), cy + I(4), I(6), I(12), new Color((byte)100,(byte)70,(byte)40,(byte)255));
            break;

        case "Chest":
            Raylib.DrawRectangle(cx - I(16), cy - I(10), I(32), I(22), new Color((byte)150,(byte)100,(byte)50,(byte)255));
            Raylib.DrawRectangle(cx - I(16), cy - I(10), I(32), I(7),  new Color((byte)110,(byte)70,(byte)35,(byte)255));
            Raylib.DrawRectangle(cx - I(2),  cy - I(2),  I(4), I(6), Color.Gold);
            break;

        case "Large Chest":
            Raylib.DrawRectangle(cx - I(20), cy - I(11), I(40), I(24), new Color((byte)150,(byte)100,(byte)50,(byte)255));
            Raylib.DrawRectangle(cx - I(20), cy - I(11), I(40), I(8),  new Color((byte)110,(byte)70,(byte)35,(byte)255));
            Raylib.DrawRectangleLines(cx - I(20), cy - I(11), I(40), I(24), Color.Gold);
            Raylib.DrawRectangle(cx - I(2), cy - I(2), I(4), I(7), Color.Gold);
            break;

        case "Furnace":
            Raylib.DrawRectangle(cx - I(15), cy - I(14), I(30), I(28), new Color((byte)90,(byte)90,(byte)95,(byte)255));
            Raylib.DrawRectangle(cx - I(7),  cy - I(1),  I(14), I(11), new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawCircle(cx, cy + I(6), I(4), new Color((byte)255,(byte)140,(byte)0,(byte)255));
            break;

        case "Copper Bar":
        case "Iron Bar":
        case "Gold Bar":
            Color barCol = item == "Copper Bar" ? new Color((byte)200,(byte)110,(byte)50,(byte)255)
                         : item == "Iron Bar"   ? new Color((byte)190,(byte)190,(byte)200,(byte)255)
                                                : new Color((byte)235,(byte)190,(byte)40,(byte)255);
            Raylib.DrawRectangle(cx - I(16), cy - I(4), I(32), I(12), barCol);
            Raylib.DrawRectangle(cx - I(12), cy - I(8), I(24), I(4),
                new Color((byte)(barCol.R * 0.8f), (byte)(barCol.G * 0.8f), (byte)(barCol.B * 0.8f), (byte)255));
            break;
        case "Stone":
            Raylib.DrawCircle(cx - I(6), cy + I(2), I(10), new Color((byte)130,(byte)130,(byte)135,(byte)255));
            Raylib.DrawCircle(cx + I(7), cy + I(5), I(7),  new Color((byte)110,(byte)110,(byte)115,(byte)255));
            Raylib.DrawCircle(cx - I(9), cy - I(1), I(3),  new Color((byte)160,(byte)160,(byte)165,(byte)255));
            break;

        case "Copper Ore":
        case "Iron Ore":
        case "Gold Ore":
            Color veinCol = item == "Copper Ore" ? new Color((byte)200,(byte)110,(byte)50,(byte)255)
                          : item == "Iron Ore"   ? new Color((byte)190,(byte)190,(byte)200,(byte)255)
                                                 : new Color((byte)235,(byte)190,(byte)40,(byte)255);
            Raylib.DrawCircle(cx, cy + I(2), I(12), new Color((byte)115,(byte)115,(byte)120,(byte)255));
            Raylib.DrawCircle(cx - I(4), cy - I(1), I(3), veinCol);
            Raylib.DrawCircle(cx + I(5), cy + I(4), I(3), veinCol);
            Raylib.DrawCircle(cx + I(1), cy + I(8), I(2), veinCol);
            break;

        case "Crystal":
            Raylib.DrawTriangle(new Vector2(cx, cy - I(14)), new Vector2(cx - I(8), cy + I(8)),
                new Vector2(cx + I(8), cy + I(8)), new Color((byte)140,(byte)230,(byte)255,(byte)255));
            Raylib.DrawTriangle(new Vector2(cx, cy - I(10)), new Vector2(cx - I(4), cy + I(6)),
                new Vector2(cx + I(4), cy + I(6)), new Color((byte)200,(byte)250,(byte)255,(byte)255));
            break;
        case "Campfire":
            Raylib.DrawRectangle(cx - I(14), cy + I(4), I(28), I(6), new Color((byte)110,(byte)80,(byte)50,(byte)255));
            Raylib.DrawTriangle(new Vector2(cx, cy - I(14)), new Vector2(cx - I(8), cy + I(4)),
                new Vector2(cx + I(8), cy + I(4)), new Color((byte)255,(byte)140,(byte)0,(byte)255));
            Raylib.DrawTriangle(new Vector2(cx, cy - I(6)), new Vector2(cx - I(4), cy + I(4)),
                new Vector2(cx + I(4), cy + I(4)), new Color((byte)255,(byte)220,(byte)60,(byte)255));
            break;

        case "Waypoint Flag":
            Raylib.DrawRectangle(cx - I(1), cy - I(16), I(3), I(32), new Color((byte)110,(byte)80,(byte)50,(byte)255));
            Raylib.DrawTriangle(new Vector2(cx + I(2), cy - I(16)), new Vector2(cx + I(2), cy - I(4)),
                new Vector2(cx + I(16), cy - I(10)), Color.Red);
            break;
            }
            
    if (cookedTint.A > 0)
        Raylib.DrawRectangle(x, y, size, size, cookedTint);
}

static void DropToGround(string item)
{
    RemoveOneItem(item);
    droppedItems.Add(new DroppedItem(item, 1, player.Center + new Vector2(0, 20)));
    ShowNotification($"Dropped {item}");
}

static void UpdateDroppedItems(float dt)
{
    for (int i = droppedItems.Count - 1; i >= 0; i--)
    {
        var d = droppedItems[i];
        d.Lifetime -= dt;
        if (d.Lifetime <= 0)
        {
            droppedItems.RemoveAt(i);
            continue;
        }

        // press F near it to collect
        if (Vector2.Distance(player.Center, d.Position) < 50 && Raylib.IsKeyPressed(KeyboardKey.F))
        {
            GiveItemBack(d.Name, d.Count);
            ShowNotification($"Picked up {d.Name}");
            droppedItems.RemoveAt(i);
        }
    }
}

static void GiveItemBack(string item, int count)
{
    var stat = FindStatItem(item);
    if (stat != null) { stat.Delta(count); return; }
    switch (item)
    {
        case "Logs": player.Logs += count; break;
        case "Birch Logs": player.BirchLogs += count; break;
        case "Oak Logs": player.OakLogs += count; break;
        case "Pine Logs": player.PineLogs += count; break;
        case "Arctic Logs": player.ArcticLogs += count; break;
        case "Dead Wood": player.DeadWood += count; break;
        case "Fish": player.Fish += count; break;
        case "Bones": player.Bones += count; break;
        case "Fur": player.Fur += count; break;
        case "Stingers": player.Stingers += count; break;
        case "Pelts": player.BearPelts += count; break;
        case "Arrows": player.Arrows += count; break;
        case "Bolts": player.Bolts += count; break;
        case "Money": player.Money += count; break;
        case "Dog Fangs": player.DogFangs += count; break;
        case "Wolf Claw": player.WolfClaws += count; break;
        case "Venom Sac": player.VenomSacs += count; break;
        case "Crab Claw": player.CrabClaws += count; break;
        case "Bear Claw": player.BearClaws += count; break;
        case "Crab Shell": player.CrabShells += count; break;
        case "Shark Fin": player.SharkFins += count; break;
        case "Shark Tooth": player.SharkTeeth += count; break;
        case "Snake Skin": player.SnakeSkins += count; break;
        case "Snake Fang": player.SnakeFangs += count; break;
        case "Croc Scale": player.CrocScales += count; break;
        case "Croc Tooth": player.CrocTeeth += count; break;
        case "Lizard Scale": player.LizardScales += count; break;
        case "Ember Stone": player.EmberStones += count; break;
        case "Magma Shard": player.MagmaShards += count; break;
        case "Lava Core": player.LavaCores += count; break;
        case "Feather": player.Feathers += count; break;
        case "Eagle Talon": player.EagleTalons += count; break;
        case "Horn": player.Horns += count; break;
        case "Goat Hoof": player.GoatHooves += count; break;
        // armor/weapons go back to owned gear; everything else to the backpack
        default:
            if (GetItemSlot(item) != null) AcquireGear(item);
            else BackpackAdd(item, count);
            break;
    }
}

static void DrawDropConfirm()
{
    if (!dropConfirmOpen) return;

    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)150));

    int w = 440, h = 200;
    int px = ScreenWidth / 2 - w / 2;
    int py = ScreenHeight / 2 - h / 2;
    Raylib.DrawRectangle(px, py, w, h, new Color((byte)20,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(px, py, w, h, Color.Red);

    string title = "Drop this item?";
    int tw = Program.MeasureTextUI(title, 28);
    Program.DrawTextUI(title, ScreenWidth / 2 - tw / 2, py + 20, 28, Color.Red);

    string warn = $"{dropConfirmItem} is valuable. Drop it on the ground?";
    int ww = Program.MeasureTextUI(warn, 18);
    Program.DrawTextUI(warn, ScreenWidth / 2 - ww / 2, py + 64, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();
    Rectangle yesBtn = new Rectangle(ScreenWidth / 2 - 160, py + 120, 140, 44);
    Rectangle noBtn  = new Rectangle(ScreenWidth / 2 + 20,  py + 120, 140, 44);
    bool hYes = Raylib.CheckCollisionPointRec(mouse, yesBtn);
    bool hNo  = Raylib.CheckCollisionPointRec(mouse, noBtn);

    Raylib.DrawRectangleRec(yesBtn, hYes ? new Color((byte)160,(byte)30,(byte)30,(byte)255) : new Color((byte)80,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(yesBtn, 2, hYes ? Color.Red : Color.DarkGray);
    Program.DrawTextUI("Yes, Drop", (int)yesBtn.X + 22, (int)yesBtn.Y + 12, 20, Color.White);

    Raylib.DrawRectangleRec(noBtn, hNo ? new Color((byte)30,(byte)100,(byte)30,(byte)255) : new Color((byte)20,(byte)60,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(noBtn, 2, hNo ? Color.Green : Color.DarkGray);
    Program.DrawTextUI("Cancel", (int)noBtn.X + 36, (int)noBtn.Y + 12, 20, Color.White);

    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (hYes)
        {
            dropConfirmOpen = false;
            dropQtyItem = dropConfirmItem;
            dropQtyAvailable = dropConfirmItem == "Money" ? player.Money : GetItemCount(dropConfirmItem);
            dropQtyOpen = true;
            dropConfirmItem = "";
        }
    }
}
    }
}
