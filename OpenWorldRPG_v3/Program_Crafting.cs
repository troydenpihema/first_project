using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        record CookingRecipe(string Name, string[] Ingredients, string Result, int XP, bool KitchenOnly, int HpRestore);

        static List<CraftRecipe> BuildCraftRecipes()
        {
            var list = new List<CraftRecipe>
            {
                new CraftRecipe { Result = "Workbench", Ingredients = new[] { ("Logs", 20) },
                    Description = "A sturdy bench for future crafting." },
                new CraftRecipe { Result = "Chest", Ingredients = new[] { ("Logs", 15) },
                    Description = "Placeable storage (20 slots)." },
                new CraftRecipe { Result = "Large Chest", Ingredients = new[] { ("Logs", 20), ("Oak Logs", 10) },
                    Description = "Placeable storage (30 slots).", Station = "Workbench"},
                new CraftRecipe { Result = "Furnace", Ingredients = new[] { ("Stone", 20), ("Logs", 10) },
                    Description = "Smelts ore into bars.", Station = "Workbench"},
                new CraftRecipe { Result = "Campfire", Ingredients = new[] { ("Stone", 10), ("Logs", 5) },
                    Description = "Cook anywhere. Fuel with logs (R).", Station = "Workbench" },
                new CraftRecipe { Result = "Waypoint Flag", Ingredients = new[] { ("Logs", 5), ("Copper Bar", 1) },
                    Description = "Marks your map and minimap." },
                new CraftRecipe { Result = "Arrows", ResultQty = 5, Ingredients = new[] { ("Logs", 1), ("Feather", 1) },
                    Description = "Makes 5 arrows." },
                new CraftRecipe { Result = "Bolts", ResultQty = 4, Ingredients = new[] { ("Iron Bar", 1) },
                    Description = "Makes 4 bolts." },
                new CraftRecipe { Result = "Torch", Ingredients = new[] { ("Logs", 2), ("Ember Stone", 1) },
                    Description = "Lights your way at night." },
                new CraftRecipe { Result = "Canteen", Ingredients = new[] { ("Iron Bar", 2) },
                    Description = "Fill at water, boil at a fire, then drink.", Station = "Workbench" },
                new CraftRecipe { Result = "Hay", ResultQty = 2, Ingredients = new[] { ("Wheat", 3) },
                    Description = "Feed for donkeys, camels & reindeer." },
            };

            // stations
            list.Add(new CraftRecipe { Result = "Anvil", Ingredients = new[] { ("Iron Bar", 5), ("Stone", 10) },
                Description = "Forge metal armor here.", Station = "Workbench" });
            list.Add(new CraftRecipe { Result = "Advanced Workstation", Ingredients = new[] { ("Iron Bar", 10), ("Logs", 20) },
                Description = "Unlocks advanced station crafting.", Station = "Workbench" });
            list.Add(new CraftRecipe { Result = "Advanced Furnace", Ingredients = new[] { ("Stone", 30), ("Iron Bar", 5) },
                Description = "Smelts steel.", Station = "Advanced Workstation" });
            list.Add(new CraftRecipe { Result = "Advanced Anvil", Ingredients = new[] { ("Steel Bar", 5), ("Stone", 10) },
                Description = "Forge steel armor here.", Station = "Advanced Workstation" });

            list.Add(new CraftRecipe { Result = "Enchanting Table", Ingredients = new[] { ("Logs", 10), ("Crystal", 5), ("Arcane Essence", 3) },
                Description = "Brew potions & enchant robes.", Station = "Advanced Workstation" });
            list.Add(new CraftRecipe { Result = "Health Potion", Ingredients = new[] { ("Crystal", 1), ("Apple", 2) },
                Description = "Full heal.", Station = "Enchanting Table" });
            list.Add(new CraftRecipe { Result = "Stamina Potion", Ingredients = new[] { ("Crystal", 1), ("Carrot", 2) },
                Description = "Fully restores your stamina.", Station = "Enchanting Table" });

            // leather set — workbench (note the Cap/Vest/Pants naming your armor system uses)
            (string piece, (string, int)[] mats)[] leather = {
                ("Leather Cap",    new[] { ("Fur", 3) }),
                ("Leather Vest",   new[] { ("Fur", 4), ("Pelts", 2) }),
                ("Leather Pants",  new[] { ("Fur", 3), ("Pelts", 1) }),
                ("Leather Boots",  new[] { ("Fur", 2) }),
                ("Leather Gloves", new[] { ("Fur", 2) }),
            };

            foreach (var (piece, mats) in leather)
                list.Add(new CraftRecipe { Result = piece, Ingredients = mats,
                    Description = "Light leather armor.", Station = "Workbench" });

            (string piece, int a, int b)[] magePieces = {
                ("Hat", 2, 1), ("Top", 4, 2), ("Bottoms", 3, 2), ("Boots", 2, 1), ("Gloves", 2, 1),
                ("Robe Cape", 3, 1), ("Book", 2, 3) };
            for (int t = 0; t < mageTiers.Length; t++)
                foreach (var (piece, a, b) in magePieces)
                    list.Add(new CraftRecipe { Result = $"{mageTiers[t]} Mage {piece}",
                        Ingredients = new[] { ("Arcane Essence", a * (t + 1)), ("Crystal", b * (t + 1)) },
                        Description = $"Tier {t + 1} wizard vestments.", Station = "Enchanting table" });

            (string piece, int a, int b)[] rangerPieces = {
                ("Hat", 2, 1), ("Tunic", 4, 2), ("Chaps", 3, 2), ("Boots", 2, 1), ("Bracers", 2, 1),
                ("Quiver", 3, 2) };
            for (int t = 0; t < rangerTiers.Length; t++)
                foreach (var (piece, a, b) in rangerPieces)
                    list.Add(new CraftRecipe { Result = $"{rangerTiers[t]} Ranger {piece}",
                        Ingredients = new[] { ("Fur", a * (t + 1)), ("Feather", b * (t + 1)), ("Snake Skin", t + 1) },
                        Description = $"Tier {t + 1} hunter's gear.", Station = "Workbench" });

            // metal sets — material requirements only, no upgrade chain
            (string mat, string ingot, string station)[] armorTiers = {
                ("Iron",  "Iron Bar",  "Anvil"),
                ("Gold",  "Gold Bar",  "Anvil"),
                ("Steel", "Steel Bar", "Advanced Anvil"),
            };
            (string piece, int cost)[] pieces = {
                ("Helmet", 3), ("Chestplate", 5), ("Leggings", 4), ("Boots", 2), ("Gauntlets", 2) };
            foreach (var (mat, ingot, station) in armorTiers)
                foreach (var (piece, cost) in pieces)
                    list.Add(new CraftRecipe { Result = $"{mat} {piece}", Ingredients = new[] { (ingot, cost) },
                        Description = $"{mat.ToLower()} armor.", Station = station });

            // metal tool chain: base → Copper → Iron → Gold → Crystal
            (string metal, string mat, int qty)[] tiers = {
                ("Copper",  "Copper Bar", 5),
                ("Iron",    "Iron Bar",   5),
                ("Gold",    "Gold Bar",   5),
                ("Crystal", "Crystal",    8),   // crystal is used raw, not smelted
            };
            string[] tools = { "Axe", "Pickaxe", "Rod", "Net" };

            foreach (string tool in tools)
            {
                string prev = tool;
                foreach (var (metal, mat, qty) in tiers)
                {
                    list.Add(new CraftRecipe {
                        Result = $"{metal} {tool}",
                        Ingredients = new[] { (mat, qty), (prev, 1) },   // consumes the previous tier
                        Description = $"Upgrade your {prev}.",
                        Station = "Workbench",
                    });
                    prev = $"{metal} {tool}";
                }
            }
            return list;
        }

        static int CraftCount(string item) => item switch
    {
        "Axe"     => player.HasAxe ? 1 : 0,
        "Pickaxe" => pickaxePickedUp ? 1 : 0,
        "Rod"     => fishingRodPickedUp ? 1 : 0,
        "Net"     => fishingNetPickedUp ? 1 : 0,
        _ => GetItemCount(item) + (FindStatItem(item) != null ? GetToolbarCount(item) : 0)
    };

    static string BaseTool(string item)
{
    if (item == null) return null;
    foreach (var m in metalPrefixes)
        if (item.StartsWith(m + " ") && IsToolItem(item)) return item.Substring(m.Length + 1);
    return item;
}

static int ToolTier(string item) => item == null ? 0 : Array.FindIndex(metalPrefixes, m => item.StartsWith(m + " ")) + 1;

    static void ConsumeCraftIngredient(string item)
    {
        if (IsToolItem(item))   {
            for (int i = 0; i < toolbarSlots.Length; i++)
                if (toolbarSlots[i] == item) { toolbarSlots[i] = null; toolbarCounts[i] = 0; }
        RemoveOneItem(item);
        return;
    }
    var stat = FindStatItem(item);
    if (stat != null && stat.Get() <= 0)
    {
        for (int i = 0; i < toolbarSlots.Length; i++)
            if (toolbarSlots[i] == item && toolbarCounts[i] > 0)
            {
                toolbarCounts[i]--;
                if (toolbarCounts[i] <= 0) toolbarSlots[i] = null;
                return;
            }
        return;
    }
        RemoveOneItem(item);  
}

        static bool IsPlaceable(string item) => item == "Workbench" || item == "Chest" || item == "Large Chest" 
        || item == "Furnace" || item == "Campfire" || item == "Waypoint Flag" || item == "Anvil" || item == "Advanced Anvil"
        || item == "Advanced Workstation" || item == "Advanced Furnace" || item == "Enchanting Table";

        static bool NearWorkbench() =>
            placedWorkbenches.Any(w => Vector2.Distance(player.Center, w) < 100);

        static bool NearStation(string s) => s switch
        {
            null => true,
            "Workbench" => NearWorkbench(),
            "Furnace" => placedFurnaces.Any(f => Vector2.Distance(player.Center, f) < 100),
            _ => stationProps.TryGetValue(s, out var list) && list.Any(p => Vector2.Distance(player.Center, p) < 100)
        };

        static bool NearAnyCraftStation() => NearWorkbench()
            || stationProps.Any(kv => kv.Key != "Advanced Furnace"
                && kv.Value.Any(p => Vector2.Distance(player.Center, p) < 100));

        static int PlaceableChestTier(string item) => item == "Large Chest" ? 1 : 0;

        static bool GiveCraftResult(string item, int qty)
        {
            if (item == "Torch")
            {
                if (torchPickedUp || ToolInToolbar("Torch"))
                    { ShowNotification("You already have a Torch."); return false; }
                torchPickedUp = true;
                return true;
            }
            if (item == "Canteen" || item == "Hay") return TryStowItem(item);
            if (IsPlaceable(item) || IsToolItem(item)) return TryStowItem(item);
            return TryGiveItem(item, qty);   // Arrows/Bolts hit their stat counters
        }

        static bool TryCraftItem(CraftRecipe r)
        {
            foreach (var (ing, qty) in r.Ingredients)
                if (CraftCount(ing) < qty) { ShowNotification($"Need {qty}x {ing}."); return false; }

            foreach (var (ing, qty) in r.Ingredients)
                for (int i = 0; i < qty; i++) ConsumeCraftIngredient(ing);

            if (!GiveCraftResult(r.Result, r.ResultQty))
            {
                // refund — don't eat materials if nothing can hold the result
                foreach (var (ing, qty) in r.Ingredients) TryGiveItem(ing, qty);
                ShowNotification("Sorry, not enough space!");
                return false;
            }
            ShowNotification(r.ResultQty > 1 ? $"Crafted {r.ResultQty}x {r.Result}!" : $"Crafted a {r.Result}!");
            int craftXp = 5 + r.Ingredients.Sum(ing => ing.qty) * 3;
            if (r.Station != null && r.Station.Contains("Anvil")) player.AddBlacksmithXP(craftXp);
            else if (r.Station == "Enchanting Table")             player.AddEnchantingXP(craftXp);
            else                                                  player.AddCraftingXP(craftXp);
            return true;
        }

        static bool TryCraftChest(int tier, Vector2 placeAt, string buildingContext)
        {
            foreach (var (ingredient, qty) in chestTierRecipe[tier])
                if (GetItemCount(ingredient) < qty) { ShowNotification($"Need {qty}x {ingredient}."); return false; }

            foreach (var (ingredient, qty) in chestTierRecipe[tier])
                for (int i = 0; i < qty; i++) RemoveOneItem(ingredient);

            var chest = new PlacedChest {
                Id = "chest_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Position = placeAt,
                BuildingContext = buildingContext,
                Tier = tier,
            };
            placedChests.Add(chest);
            ShowNotification($"Crafted a Tier {tier + 1} chest! ({chest.Capacity} slots)");
            return true;
        }

        static void UpdateCanteenFill()
{
    if (toolbarSlots[toolbarSelectedSlot] != "Canteen"
        || !Raylib.IsKeyPressed(KeyboardKey.Space) || chatInputOpen) return;

    bool nearWater =
        lakes.Any(l => Vector2.Distance(player.Center, l.Position) < 200)
        || rivers.Any(r => Raylib.CheckCollisionPointRec(player.Center,
             new Rectangle(r.Bounds.X - 90, r.Bounds.Y - 90, r.Bounds.Width + 180, r.Bounds.Height + 180)))
        || GetCurrentBiome() == "OCEAN";
    if (!nearWater) return;

    toolbarSlots[toolbarSelectedSlot] = "Canteen (Dirty)";
    ShowNotification("Filled Canteen — boil it on a campfire or stove before drinking!");
}

static int GetCookingIngredientCount(string ingredient)
{
    // check toolbar counts + fridge + cupboard
    for (int i = 0; i < toolbarSlots.Length; i++)
        if (toolbarSlots[i] == ingredient) return toolbarCounts[i];
    if (fridgeContents.Contains(ingredient))   return fridgeContents.Count(s => s == ingredient);
    if (cupboardContents.Contains(ingredient)) return cupboardContents.Count(s => s == ingredient);
    return 0;
}

static bool CanCook(CookingRecipe recipe)
{
    foreach (string ing in recipe.Ingredients)
        if (GetCookingIngredientCount(ing) < 1) return false;
    return true;
}

static void ConsumeIngredients(CookingRecipe recipe)
{
    foreach (string ing in recipe.Ingredients)
    {
        // try toolbar first
        for (int i = 0; i < toolbarSlots.Length; i++)
        {
            if (toolbarSlots[i] == ing && toolbarCounts[i] > 0)
            { toolbarCounts[i]--; if (toolbarCounts[i] <= 0) toolbarSlots[i] = null; goto next; }
        }
        // try fridge
        int fi = fridgeContents.IndexOf(ing);
        if (fi >= 0) { fridgeContents[fi] = null; goto next; }
        // try cupboard
        int ci = cupboardContents.IndexOf(ing);
        if (ci >= 0) { cupboardContents[ci] = null; }
        next:;
    }
}

static void DrawFridgeUI()
{
    if (!fridgeOpen) return;

    int px = ScreenWidth/2 - 260, py = 80;
    Color iceBlue = new Color((byte)100,(byte)200,(byte)255,(byte)255);
    Raylib.DrawRectangle(px, py, 520, 440, new Color((byte)5,(byte)20,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 520, 440, iceBlue);
    Program.DrawTextUI("FRIDGE", px + 190, py + 10, 28, iceBlue);

    int slotSize = 70, pad = 8, cols = 4;
    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < 12; i++)
    {
        int col = i % cols, row = i / cols;
        int sx = px + 20 + col * (slotSize + pad);
        int sy = py + 50 + row * (slotSize + pad);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)15,(byte)40,(byte)55,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)100,(byte)140,(byte)255));

        if (fridgeContents[i] != null)
        {
            DrawInventoryIcon(fridgeContents[i], sx + 8, sy + 8, slotSize - 16);
            int nw = Program.MeasureTextUI(fridgeContents[i], 11);
            Program.DrawTextUI(fridgeContents[i], sx + slotSize/2 - nw/2, sy + slotSize - 16, 11, Color.White);

            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize));
            if (hover) Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                // take item out to toolbar
                if (AddToToolbar(fridgeContents[i]))
                { toolbarCounts[Array.IndexOf(toolbarSlots, fridgeContents[i])] = 1; fridgeContents[i] = null; }
                else ShowNotification("Toolbar full!");
            }
        }
        else
        {
            // can put item in — check if holding something
            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize));
            if (hover && invSelectedName != "" && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                fridgeContents[i] = invSelectedName;
                RemoveOneItem(invSelectedName);
                invSelectedName = "";
            }
        }
    }

    Program.DrawTextUI("Click item to take out  |  Q = Close", px + 60, py + 408, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) fridgeOpen = false;
}

static void DrawCupboardUI()
{
    if (!cupboardOpen) return;

    int px = ScreenWidth/2 - 260, py = 80;
    Color woodCol = new Color((byte)200,(byte)160,(byte)80,(byte)255);
    Raylib.DrawRectangle(px, py, 520, 440, new Color((byte)30,(byte)20,(byte)10,(byte)245));
    Raylib.DrawRectangleLines(px, py, 520, 440, woodCol);
    Program.DrawTextUI("CUPBOARD", px + 170, py + 10, 28, woodCol);

    int slotSize = 70, pad = 8, cols = 4;
    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < 12; i++)
    {
        int col = i % cols, row = i / cols;
        int sx = px + 20 + col * (slotSize + pad);
        int sy = py + 50 + row * (slotSize + pad);

        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)50,(byte)35,(byte)15,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)120,(byte)80,(byte)30,(byte)255));

        if (cupboardContents[i] != null)
        {
            DrawInventoryIcon(cupboardContents[i], sx + 8, sy + 8, slotSize - 16);
            int nw = Program.MeasureTextUI(cupboardContents[i], 11);
            Program.DrawTextUI(cupboardContents[i], sx + slotSize/2 - nw/2, sy + slotSize - 16, 11, Color.White);

            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize));
            if (hover) Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (AddToToolbar(cupboardContents[i]))
                { toolbarCounts[Array.IndexOf(toolbarSlots, cupboardContents[i])] = 1; cupboardContents[i] = null; }
                else ShowNotification("Toolbar full!");
            }
        }
    }

    Program.DrawTextUI("Click item to take out  |  Q = Close", px + 60, py + 408, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) cupboardOpen = false;
}

static void CookRawItemFromToolbar(string rawItem)
{
    if (isCooking || quickCooking) return;   // already cooking — no spam

    string result = rawItem switch {
        "Fish"     => "Cooked Fish",
        "Raw Meat" => "Cooked Meat",
        "Potato"   => "Roast Potato",
        "Corn"     => "Cooked Corn",
        "Canteen (Dirty)" => "Canteen (Clean)",
        _ => null
    };
    if (result == null) return;

    int slot = toolbarSelectedSlot;
    if (toolbarSlots[slot] != rawItem || toolbarCounts[slot] <= 0) return;

    // need somewhere for the output; the slot frees if this is the last raw one
    bool slotWillFree = toolbarCounts[slot] == 1;
    bool hasRoom = slotWillFree
                || toolbarSlots.Any(s => s == result || s == "Ashes")
                || !ToolbarFull();
    if (!hasRoom)
    {
        ShowNotification("Your inventory is too full to cook!");
        return;
    }

    // start the timed cook
    quickCooking   = true;
    quickCookTimer = QuickCookDuration;
    quickCookRaw   = rawItem;
    quickCookResult = result;
    quickCookSlot  = slot;
}

static void UpdateQuickCook(float dt)
{
    if (!quickCooking) return;

    quickCookTimer -= dt;
    if (quickCookTimer > 0f) return;

    quickCooking = false;

    // validate the slot still holds the raw item (player may have swapped)
    if (quickCookSlot < 0 || quickCookSlot >= toolbarSlots.Length
        || toolbarSlots[quickCookSlot] != quickCookRaw
        || toolbarCounts[quickCookSlot] <= 0)
    {
        ShowNotification("Cooking cancelled.");
        return;
    }

    // consume one raw item
    toolbarCounts[quickCookSlot]--;
    if (toolbarCounts[quickCookSlot] <= 0) toolbarSlots[quickCookSlot] = null;

    // burn chance: 30% base, -2% per cooking level, floor 5%
    float burnChance = Math.Max(0.05f, 0.30f - (player.CookingLevel * 0.02f));
    bool burned = Raylib.GetRandomValue(0, 100) < (int)(burnChance * 100);
    if (quickCookResult == "Canteen (Clean)") burned = false;

    string output = burned ? "Ashes" : quickCookResult;
    if (!AddOneItemToToolbar(output))
    {
        ShowNotification("Your inventory is too full to cook!");
        return;
    }

    if (burned)
    {
        ShowNotification($"Burnt the {quickCookRaw}! Got Ashes.");
        player.AddCookingXP(2);
    }
    else
    {
        int xp = quickCookResult == "Cooked Meat" ? 12 : 10;
        player.AddCookingXP(xp);
        ShowNotification($"Cooked {quickCookResult}! +{xp} Cooking XP");
    }
}

static void DrawQuickCookBar()
{
    if (!quickCooking) return;

    int bw = 360, bh = 22;
    int bx = ScreenWidth / 2 - bw / 2;
    int by = ScreenHeight - 160;

    float prog = Math.Clamp(1f - quickCookTimer / QuickCookDuration, 0f, 1f);
    Color orange = new Color((byte)220,(byte)120,(byte)20,(byte)255);

    Raylib.DrawRectangle(bx - 4, by - 22, bw + 8, bh + 30, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Program.DrawTextUI($"Cooking {quickCookRaw}...", bx, by - 20, 16, Color.LightGray);
    Raylib.DrawRectangle(bx, by, bw, bh, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(bx, by, (int)(bw * prog), bh, orange);
    Raylib.DrawRectangleLines(bx, by, bw, bh, Color.White);
}

static void DrawCookingMenu()
{
    if (!cookingMenuOpen) return;

    bool isKitchen = cookingContext == "kitchen";
    var available = cookingRecipes.Where(r => !r.KitchenOnly || isKitchen).ToArray();

    int px = ScreenWidth/2 - 360, py = 50;
    int pw = 720, ph = 560;
    Color orange = new Color((byte)220,(byte)120,(byte)20,(byte)255);

    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)15,(byte)10,(byte)5,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, orange);
    string title = isKitchen ? "KITCHEN — COOK" : "CAMPFIRE — COOK";
    Program.DrawTextUI(title, px + pw/2 - Program.MeasureTextUI(title,26)/2, py + 10, 26, orange);
    Program.DrawTextUI($"Cooking Lv {player.CookingLevel}", px + 10, py + 10, 16, Color.LightGray);

    // cooking progress bar
    if (isCooking)
    {
        cookingTimer -= Raylib.GetFrameTime();
        int bw = pw - 40;
        float prog = Math.Max(0, 1f - cookingTimer / 2f);
        Raylib.DrawRectangle(px + 20, py + 46, bw, 14, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(px + 20, py + 46, (int)(bw * prog), 14, orange);
        Raylib.DrawRectangleLines(px + 20, py + 46, bw, 14, Color.White);
        Program.DrawTextUI($"Cooking {cookingItem}...", px + 20, py + 64, 16, Color.LightGray);
        if (cookingTimer <= 0f)
        {
            isCooking = false;
            AddOneItemToToolbar(cookingItem);
            player.AddCookingXP(cookingRecipes.FirstOrDefault(r => r.Result == cookingItem)?.XP ?? 10);
            mealsCooked++;
            ShowNotification($"Cooked {cookingItem}!");
        }
        Program.DrawTextUI("Q = Cancel", px + pw/2 - 40, py + ph - 20, 16, Color.DarkGray);
        if (Raylib.IsKeyPressed(KeyboardKey.Q)) { isCooking = false; cookingMenuOpen = false; }
        return;
    }

    Vector2 mouse = Raylib.GetMousePosition();
    int rowH = 68, startY = py + 86;

    for (int i = 0; i < available.Length; i++)
    {
        var r = available[i];
        bool canCook = CanCook(r);
        Rectangle row = new Rectangle(px + 12, startY + i * rowH, pw - 24, rowH - 6);
        bool hover = canCook && Raylib.CheckCollisionPointRec(mouse, row);

        Raylib.DrawRectangleRec(row, new Color((byte)25,(byte)15,(byte)5,(byte)255));
        Raylib.DrawRectangleLinesEx(row, hover ? 2 : 1, hover ? orange : (canCook ? new Color((byte)80,(byte)50,(byte)10,(byte)255) : new Color((byte)40,(byte)40,(byte)40,(byte)255)));

        // result name
        Color nameCol = canCook ? Color.White : Color.DarkGray;
        Program.DrawTextUI(r.Result, px + 22, startY + i * rowH + 8, 20, nameCol);
        // HP restore
        Program.DrawTextUI($"+{r.HpRestore} HP", px + 22, startY + i * rowH + 32, 14, canCook ? new Color((byte)80,(byte)200,(byte)80,(byte)255) : Color.DarkGray);
        // XP
        Program.DrawTextUI($"+{r.XP} XP", px + 110, startY + i * rowH + 32, 14, canCook ? new Color((byte)200,(byte)160,(byte)20,(byte)255) : Color.DarkGray);
        // ingredients
        string ingList = string.Join(" + ", r.Ingredients);
        Program.DrawTextUI(ingList, px + 220, startY + i * rowH + 22, 14, canCook ? Color.LightGray : Color.DarkGray);
        // kitchen only tag
        if (r.KitchenOnly)
            Program.DrawTextUI("[Kitchen]", px + pw - 110, startY + i * rowH + 8, 13, new Color((byte)100,(byte)160,(byte)220,(byte)255));

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            ConsumeIngredients(r);
            cookingItem = r.Result;
            isCooking = true;
            cookingTimer = 2f;
        }
    }

    Program.DrawTextUI("Q = Close", px + pw/2 - 38, py + ph - 20, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) cookingMenuOpen = false;
}

static void DrawPMCrafting(int cx, int cy, int cw)
{
    Program.DrawTextUI("CRAFTING", cx, cy, 26, Color.Gold);
    Program.DrawTextUI("Gather materials, craft, then press Space in the world to place.", cx, cy + 34, 18, Color.Gray);

    bool nearBench = NearWorkbench();
    if (nearBench)
        Program.DrawTextUI("WORKBENCH IN RANGE", cx + cw - 260, cy + 4, 18, Color.Green);

    Vector2 mouse = Raylib.GetMousePosition();
    int rowH = 84, startY = cy + 70;

    int listTop = startY, listH = ScreenHeight - listTop - 60;
    int maxScroll = Math.Max(0, craftRecipes.Count * rowH - listH);
    craftScrollY = Math.Clamp(craftScrollY - Raylib.GetMouseWheelMove() * 60f, 0f, maxScroll);
    startY -= (int)craftScrollY;
    bool mouseInList = Raylib.CheckCollisionPointRec(mouse, new Rectangle(cx, listTop, cw, listH));
    Raylib.BeginScissorMode(cx - 4, listTop, cw + 8, listH);

    for (int i = 0; i < craftRecipes.Count; i++)
    {
        int rowY = startY + i * rowH;
        if (rowY + rowH < listTop || rowY > listTop + listH) continue;

        var r = craftRecipes[i];
        bool unlocked = NearStation(r.Station);
        bool canCraft = unlocked && r.Ingredients.All(ing => CraftCount(ing.ingredient) >= ing.qty);

        Rectangle row = new Rectangle(cx, startY + i * rowH, cw - 20, rowH - 10);
        bool hover = mouseInList && Raylib.CheckCollisionPointRec(mouse, row);
        Raylib.DrawRectangleRec(row, hover ? new Color((byte)30,(byte)33,(byte)48,(byte)255)
                                           : new Color((byte)22,(byte)24,(byte)36,(byte)255));
        Raylib.DrawRectangleLinesEx(row, 2, canCraft ? Color.Gold : new Color((byte)70,(byte)70,(byte)80,(byte)255));

        Program.DrawTextUI(r.Result, cx + 14, startY + i * rowH + 8, 22, canCraft ? Color.White : Color.Gray);
        Program.DrawTextUI(r.Description, cx + 14, startY + i * rowH + 36, 16, Color.Gray);

        if (r.Station != null && !unlocked)
        Program.DrawTextUI($"Requires {r.Station} nearby", cx + 14, rowY + 54, 15, Color.Orange);

        // ingredient list with have/need colouring
        int ix = cx + 320;
        foreach (var (ing, qty) in r.Ingredients)
        {
            int have = CraftCount(ing); 
            bool ok = have >= qty;
            string txt = $"{ing} {Math.Min(have, qty)}/{qty}";
            Program.DrawTextUI(txt, ix, startY + i * rowH + 14, 20, ok ? Color.Green : Color.Red);
            ix += Program.MeasureTextUI(txt, 16) + 24;
        }

        // craft button
        Rectangle btn = new Rectangle(cx + cw - 150, startY + i * rowH + 18, 110, 38);
        bool bHov = mouseInList && Raylib.CheckCollisionPointRec(mouse, btn);
        Raylib.DrawRectangleRec(btn, canCraft ? (bHov ? new Color((byte)60,(byte)50,(byte)20,(byte)255)
                                                      : new Color((byte)40,(byte)40,(byte)50,(byte)255))
                                              : new Color((byte)25,(byte)25,(byte)30,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, canCraft ? Color.Gold : Color.DarkGray);
        Program.DrawTextUI("CRAFT", (int)btn.X + 26, (int)btn.Y + 10, 18, canCraft ? Color.Gold : Color.DarkGray);

        if (canCraft && bHov && Raylib.IsMouseButtonPressed(MouseButton.Left))
            TryCraftItem(r);
    }
    Raylib.EndScissorMode();
    if (maxScroll > 0)
    {
        int barH = Math.Max(30, listH * listH / (craftRecipes.Count * rowH));
        int barY = listTop + (int)((listH - barH) * (craftScrollY / maxScroll));
        Raylib.DrawRectangle(cx + cw - 10, barY, 6, barH, new Color((byte)120,(byte)120,(byte)140,(byte)255));
    }
}

static void DrawFurnaceUI()
{
    if (!furnaceOpen) return;

    int px = ScreenWidth / 2 - 260, py = 140, pw = 520, ph = 350;   // taller: 4 rows now
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(px, py, pw, ph, Color.Orange);
    Program.DrawTextUI(furnaceOpenAdvanced ? "ADVANCED FURNACE" : "FURNACE",
        px + (furnaceOpenAdvanced ? 130 : 190), py + 14, 28, Color.Orange);
    Program.DrawTextUI("Each smelt: 1x Logs (fuel) + inputs  |  Q = Close", px + 20, py + 50, 15, Color.Gray);

    bool advFurnace = furnaceOpenAdvanced;

    Vector2 mouse = Raylib.GetMousePosition();
    int rowH = 62, startY = py + 80;

    for (int i = 0; i < smeltRecipes.Length; i++)
    {
        var (input, qty, extra, extraQty, bar, advanced) = smeltRecipes[i];
        bool can = (!advanced || advFurnace)
            && CraftCount(input) >= qty
            && CraftCount("Logs") >= 1
            && (extra == null || CraftCount(extra) >= extraQty);

        Rectangle row = new Rectangle(px + 16, startY + i * rowH, pw - 32, rowH - 8);
        bool hover = Raylib.CheckCollisionPointRec(mouse, row);
        Raylib.DrawRectangleRec(row, hover ? new Color((byte)45,(byte)35,(byte)25,(byte)255)
                                           : new Color((byte)30,(byte)30,(byte)40,(byte)255));
        Raylib.DrawRectangleLinesEx(row, 2, can ? Color.Orange : Color.DarkGray);

        string inputTxt = extra == null
            ? $"{qty}x {input} + 1x Logs"
            : $"{qty}x {input} + {extraQty}x {extra} + 1x Logs";
        Program.DrawTextUI(inputTxt, px + 30, startY + i * rowH + 8, 18, can ? Color.White : Color.Gray);

        if (advanced && !advFurnace)
            Program.DrawTextUI("Requires Advanced Furnace", px + 30, startY + i * rowH + 32, 15, Color.Orange);
        else
            Program.DrawTextUI($"-> 1x {bar}   (have {CraftCount(input)})", px + 30, startY + i * rowH + 32, 15,
                can ? Color.Green : Color.Red);

        if (can && hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            for (int k = 0; k < qty; k++) ConsumeCraftIngredient(input);
            if (extra != null)
                for (int k = 0; k < extraQty; k++) ConsumeCraftIngredient(extra);
            ConsumeCraftIngredient("Logs");
            if (TryGiveItem(bar, 1))
                {
                    ShowNotification($"Smelted 1x {bar}!");
                    player.AddBlacksmithXP(bar == "Steel Bar" ? 25 : 12);   // NEW
                }
            if (TryGiveItem(bar, 1)) ShowNotification($"Smelted 1x {bar}!");
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Q) || Raylib.IsKeyPressed(KeyboardKey.Escape))
        furnaceOpen = false;
}

static string ResolveCookedIcon(string item, out Color tint)
{
    tint = Color.Blank;
    switch (item)
    {
        case "Cooked Fish":  tint = new Color((byte)180,(byte)110,(byte)50,(byte)90);  return "Fish";
        case "Cooked Meat":  tint = new Color((byte)150,(byte)80,(byte)40,(byte)90);   return "Raw Meat";
        case "Roast Potato": tint = new Color((byte)160,(byte)110,(byte)50,(byte)90);  return "Potato";
        case "Cooked Corn":  tint = new Color((byte)200,(byte)150,(byte)20,(byte)80);  return "Corn";
        // burnt — heavy dark tint over whatever it was
        case "Ashes":        tint = new Color((byte)20,(byte)20,(byte)20,(byte)170);   return "Ashes";
        default: return item;
    }
}
    }
}
