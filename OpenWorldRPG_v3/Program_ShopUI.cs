using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
static bool TrySpend(int amount)
{
    if (player.Money >= amount) { player.Money -= amount; return true; }
    if (bankCardDelivered && bankBalance >= amount
        && cardSpentToday + amount <= cardDailyLimits[bankCardTier])
    {
        bankBalance -= amount; cardSpentToday += amount;
        ShowNotification($"Paid ${amount} by card. (${cardDailyLimits[bankCardTier] - cardSpentToday} left today)");
        return true;
    }
    if (bankCardDelivered && cardSpentToday + amount > cardDailyLimits[bankCardTier])
        ShowNotification("Card daily limit reached!");
    return false;
}   

static float FriendDiscount(string buildingName)
{
    var f = friendNPCs.FirstOrDefault(fr => fr.Shop == buildingName);
    if (f == null) return 0f;
    return f.Friendship >= 90 ? 0.15f : f.Friendship >= 60 ? 0.10f : f.Friendship >= 30 ? 0.05f : 0f;
}

static int DiscountedPrice(int basePrice)
{
    float friendDisc = FriendDiscount(currentBuilding?.BuildingName ?? "");
    float repDisc = GetReputationShopDiscount();
    float totalDisc = MathF.Min(friendDisc + repDisc, 0.35f); // cap at 35%
    return (int)MathF.Ceiling(basePrice * (1f - totalDisc));
}

    public static void DrawTextUI(string text, int x, int y, int fontSize, Color color)
    => Raylib.DrawTextEx(uiFont, text, new Vector2(x, y), fontSize, fontSize / 16f, color);

static int GetGroceryCount(string name)
{
    // check trolley + basket combined
    int count = 0;
    foreach (var s in trolleyInventory) if (s == name) count++;
    foreach (var s in basketInventory)  if (s == name) count++;
    return count;
}

static int GetTotalCartCost()
{
    int total = 0;
    var inv = player.HasTrolley ? trolleyInventory : basketInventory;
    foreach (string s in inv)
    {
        if (s == null) continue;
        var item = groceryItems.FirstOrDefault(g => g.Name == s);
        if (item != null) total += item.Price;
    }
    return total;
}

static void RedeemPrize(string name, int cost)
{
    if (player.Tickets >= cost)
    {
        player.Tickets -= cost;
        player.PlushPrizes++;          // reuse your existing prize counter
        shopMessage = $"Redeemed {name}!";
        shopMessageTimer = 2.5f;
    }
    else
    {
        shopMessage = $"Need {cost} tickets for {name}";
        shopMessageTimer = 2.5f;
    }
}

static void DrawHobbiesStoreUI()
{
    if (!hobbiesShopOpen) return;
    int px = ScreenWidth/2 - 380, py = 40, pw = 760, ph = 560; 
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)20,(byte)15,(byte)10,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, new Color((byte)220,(byte)60,(byte)60,(byte)255));
    Program.DrawTextUI("HOBBIES STORE and TRADING CARDS", px + 100, py + 12, 22, new Color((byte)240,(byte)220,(byte)60,(byte)255));
    Program.DrawTextUI($"${player.Money}", px + pw - 90, py + 16, 20, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    Program.DrawTextUI("Choose a pack:", px + 30, py + 80, 18, Color.LightGray);
    for (int i = 0; i < cardSets.Count; i++)
    {
        Rectangle setBtn = new Rectangle(px + 30 + i * 175, py + 100, 165, 50); 
        bool hovSet = Raylib.CheckCollisionPointRec(mouse, setBtn);
        bool selected = hobbiesShopSetIndex == i;
        Raylib.DrawRectangleRec(setBtn, selected ? cardSets[i].CoverColor : new Color((byte)50,(byte)15,(byte)15,(byte)255));
        Raylib.DrawRectangleLinesEx(setBtn, 2, selected ? Color.Gold : (hovSet ? Color.LightGray : Color.DarkGray));
        Program.DrawTextUI(cardSets[i].CoverTitle, (int)setBtn.X + 12, (int)setBtn.Y + 14, 16, Color.White);
        if (hovSet && Raylib.IsMouseButtonPressed(MouseButton.Left)) hobbiesShopSetIndex = i;
    }

    Rectangle buyBtn = new Rectangle(px + 270, py + 170, 220, 60);
    bool hover = Raylib.CheckCollisionPointRec(mouse, buyBtn);
    Raylib.DrawRectangleRec(buyBtn, hover ? new Color((byte)80,(byte)20,(byte)20,(byte)255) : new Color((byte)50,(byte)15,(byte)15,(byte)255));
    Raylib.DrawRectangleLinesEx(buyBtn, 2, hover ? Color.Gold : Color.DarkGray);
    Program.DrawTextUI($"Buy {cardSets[hobbiesShopSetIndex].CoverTitle} Pack — $5", (int)buyBtn.X + 10, (int)buyBtn.Y + 22, 18, Color.White);

    if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (player.Money >= 5)
        {
            player.Money -= 5;
            var opened = OpenPack(cardSets[hobbiesShopSetIndex]);
            cardSets[hobbiesShopSetIndex].PacksOpened++; 
            foreach (var c in opened) cardSets[hobbiesShopSetIndex].MasterSet.Add(c.Name, c.ReverseHolo);
            lastPackOpened = opened;
            packRevealIndex = 0;
            packRevealTimer = 4f;
            ShowNotification(lastPackWasGod ? "*** GOD PACK!!! ***" : "Cards added to Master Set!");
        }
        else ShowNotification("Need $5 for a pack!");
    }

    if (lastPackOpened != null && packRevealTimer > 0)
    {
        var c = lastPackOpened[packRevealIndex];
        int cardX = px + pw/2 - 90;
        int cardTop = py + 260;
        Color rarityCol = c.Rarity switch
        {
            CardRarity.SecretRare => new Color((byte)255,(byte)80,(byte)220,(byte)255),
            CardRarity.UltraRare => new Color((byte)255,(byte)140,(byte)30,(byte)255),
            CardRarity.Holo => new Color((byte)230,(byte)190,(byte)60,(byte)255),
            CardRarity.Rare => new Color((byte)160,(byte)90,(byte)220,(byte)255),
            CardRarity.Uncommon => new Color((byte)90,(byte)160,(byte)220,(byte)255),
            CardRarity.Trainer => new Color((byte)90,(byte)200,(byte)90,(byte)255),
            CardRarity.Energy => new Color((byte)220,(byte)140,(byte)60,(byte)255),
            _ => Color.LightGray
        };
        Raylib.DrawRectangle(cardX, cardTop, 180, 230, rarityCol);   // height trimmed from 260 to 230
        Raylib.DrawRectangleLines(cardX, cardTop, 180, 230, Color.Black);
        DrawWrappedCardText(c.Name, cardX + 10, cardTop + 165, 160, 16, Color.Black);            // was single line at +200
        if (c.ReverseHolo) Program.DrawTextUI("REV", cardX + 10, cardTop + 210, 14, Color.White);
        if (c.Power > 0) Program.DrawTextUI($"PL {c.Power}", cardX + 10, cardTop + 5, 14, Color.Black);       // top-left
        Program.DrawTextUI($"Card {packRevealIndex + 1} / {lastPackOpened.Count}", cardX + 85, cardTop + 5, 14, Color.Black);

        Rectangle nextBtn = new Rectangle(cardX + 190, cardTop + 95, 60, 40);
        bool hovNext = Raylib.CheckCollisionPointRec(mouse, nextBtn);
        Raylib.DrawRectangleRec(nextBtn, hovNext ? new Color((byte)80,(byte)20,(byte)20,(byte)255) : new Color((byte)50,(byte)15,(byte)15,(byte)255));
        Program.DrawTextUI(">", (int)nextBtn.X + 20, (int)nextBtn.Y + 6, 24, Color.White);

        if (hovNext && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            packRevealIndex++;
            if (packRevealIndex >= lastPackOpened.Count)
            {
                lastPackOpened = null;
                packRevealIndex = 0;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            lastPackOpened = null;
            packRevealIndex = 0;
        }
    }

    Program.DrawTextUI("Q = Close", px + pw/2 - 38, py + ph - 24, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) hobbiesShopOpen = false;
}

static void DrawBillboardUI()   // call after EndMode2D with your other UI
{
    if (!billboardOpen) return;
    int px = 340, py = 140;
    Raylib.DrawRectangle(px, py, 600, 440, new Color((byte)25,(byte)20,(byte)12,(byte)245));
    Raylib.DrawRectangleLines(px, py, 600, 440, new Color((byte)235,(byte)225,(byte)200,(byte)255));
    Program.DrawTextUI("TOWN BILLBOARD — one task at a time (resets daily)", px + 20, py + 14, 19, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < billboardTasks.Count; i++)
    {
        var t = billboardTasks[i];
        Rectangle row = new Rectangle(px + 20, py + 56 + i * 86, 560, 74);
        bool hov = Raylib.CheckCollisionPointRec(mouse, row);
        Raylib.DrawRectangleRec(row, new Color((byte)45,(byte)38,(byte)25,(byte)255));
        Raylib.DrawRectangleLinesEx(row, 2, t.DoneToday ? Color.DarkGray : t.Accepted ? Color.Gold : (hov ? Color.White : Color.Gray));
        Program.DrawTextUI($"{t.Title} — ${t.Pay}", (int)row.X + 12, (int)row.Y + 8, 19, t.DoneToday ? Color.DarkGray : Color.White);
        string line = t.DoneToday ? "DONE TODAY"
            : t.Accepted ? (t.ReadyToDeliver ? t.DeliverLabel + " (H)" : $"In progress: {Math.Clamp(t.Progress() - t.Baseline, 0, t.Target)}/{t.Target}")
            : $"Target: {t.Target}  |  {t.DeliverLabel}  |  CLICK TO ACCEPT";
        Program.DrawTextUI(line, (int)row.X + 12, (int)row.Y + 42, 15, t.ReadyToDeliver ? Color.Green : Color.LightGray);

        if (!t.DoneToday && !t.Accepted && activeSideTask == null && hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            t.Accepted = true;
            t.Baseline = t.Progress();
            activeSideTask = t;
            ShowNotification($"Task accepted: {t.Title}");
        }
    }
    Program.DrawTextUI("Q = Close", px + 260, py + 408, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) billboardOpen = false;
}

static void SpendResource(string res, int n)
{
    switch (res)
    { case "Logs": player.Logs -= n; break; case "Fish": player.Fish -= n; break;
      case "Bones": player.Bones -= n; break; case "Fur": player.Fur -= n; break; }
}

static void DrawLibraryPhotoMenu()
{
    int pw = 460, ph = 340, px = ScreenWidth/2 - pw/2, py = ScreenHeight/2 - ph/2;
    Raylib.DrawRectangle(0,0,ScreenWidth,ScreenHeight,new Color((byte)0,(byte)0,(byte)0,(byte)150));
    Raylib.DrawRectangle(px,py,pw,ph,new Color((byte)24,(byte)24,(byte)34,(byte)245));
    Raylib.DrawRectangleLines(px,py,pw,ph,Color.Gold);
    Program.DrawTextUI("DELIVER ID TO:", px+20, py+18, 24, Color.Gold);
    Program.DrawTextUI("[Q] Cancel", px+pw-110, py+22, 16, Color.LightGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) { libraryPhotoMenuOpen = false; return; }

    Vector2 mouse = Raylib.GetMousePosition();

    int rows = ownedHousePlots.Count + 1;   // CHANGED — always includes Default House
    for (int i = 0; i < rows; i++)
    {
        int ry = py + 70 + i * 54;
        Rectangle r = new Rectangle(px+20, ry, pw-40, 46);
        bool hov = Raylib.CheckCollisionPointRec(mouse, r);
        Raylib.DrawRectangleRec(r, hov ? new Color((byte)60,(byte)60,(byte)80,(byte)255)
                                       : new Color((byte)40,(byte)40,(byte)55,(byte)255));
        Raylib.DrawRectangleLinesEx(r, 2, hov ? Color.Gold : Color.DarkGray);

        string label = i == 0 ? "Default House" : $"House {i}  ({ownedHousePlots[i-1].x}, {ownedHousePlots[i-1].y})";
        Program.DrawTextUI(label, px+34, ry+12, 20, Color.White);

        if (hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            idTargetHouseIndex = i == 0 ? -1 : i - 1;   // CHANGED — -1 = default house, else owned-plot index
            idIssuedDate = $"{GetMonthString()} {dayOfMonth}";
            idPending = true;
            idClaimed = false;
            idDeliveryTimer = 5f * daySpeed;
            libraryPhotoMenuOpen = false;
            ShowNotification("Photo taken! Your ID will arrive by mail in one day.");
        }
    }
}

static void DrawArmorUI()
{
    if (!armorMenuOpen) return;

    int panelW = 700;
    int panelH = 620;
    int panelX = ScreenWidth / 2 - panelW / 2;
    int panelY = ScreenHeight / 2 - panelH / 2;

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)18,(byte)18,(byte)24,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, Color.Gold);
    Program.DrawTextUI("EQUIPMENT", panelX + 270, panelY + 12, 28, Color.Gold);
    Program.DrawTextUI($"Defence: {GetTotalDefense()}", panelX + 20, panelY + 14, 20, Color.LightGray);

    // ── offensive stats from the equipped weapon ──
    string eqWeapon = GetActiveWeapon();
    var style = GetArmorStyleBonus();                         
    int meleeDmg = style.melee, rangedDmg = style.ranged, elementalDmg = style.magic;   
    if (eqWeapon != null)
    {
        if (IsRangedWeapon(eqWeapon))
            rangedDmg += GetWeaponDamage(eqWeapon) + player.RangedLevel / 5;            
        else if (eqWeapon.EndsWith("Staff"))
            elementalDmg += GetWeaponDamage(eqWeapon) + player.ElementalLevel / 3;      
        else
            meleeDmg += 1 + (player.CombatLevel / 10) + GetWeaponDamage(eqWeapon);      
    }

    Program.DrawTextUI($"Melee: {meleeDmg}",      panelX + 20, panelY + 40, 18, new Color((byte)255,(byte)150,(byte)80,(byte)255));
    Program.DrawTextUI($"Ranged: {rangedDmg}",    panelX + 20, panelY + 62, 18, new Color((byte)140,(byte)220,(byte)120,(byte)255));
    Program.DrawTextUI($"Elemental: {elementalDmg}", panelX + 20, panelY + 84, 18, new Color((byte)150,(byte)180,(byte)255,(byte)255));

    string setName = new[] { armorHelmet, armorBody, armorLegs, armorBoots, armorGloves }.All(p => p?.Contains("Mage ") == true) ? "ARCHMAGE SET"
        : new[] { armorHelmet, armorBody, armorLegs, armorBoots, armorGloves }.All(p => p?.Contains("Ranger ") == true) ? "SHARPSHOOTER SET"
        : new[] { armorHelmet, armorBody, armorLegs, armorBoots, armorGloves }.All(p => p != null && !p.Contains("Mage ") && !p.Contains("Ranger ")) ? "JUGGERNAUT SET"
        : null;
    if (setName != null)
        Program.DrawTextUI($"{setName} (+8)", panelX + 20, panelY + 106, 16, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    // ── EQUIPPED SLOTS (unchanged) ──
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
        ("AMMO",    equippedAmmo, panelX + 500, panelY + 270),
    };

    foreach (var (label, value, sx, sy) in slots)
    {
        bool isShieldSlot  = label == "SHIELD";
        bool shieldBlocked = isShieldSlot && IsTwoHandedWeapon(armorWeapon);

        Color borderColor = shieldBlocked ? Color.DarkGray : value != null ? Color.Gold : Color.Gray;

        Raylib.DrawRectangle(sx, sy, 140, 60, new Color((byte)35,(byte)35,(byte)45,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, 140, 60, borderColor);
        Program.DrawTextUI(label, sx + 6, sy + 6, 14, new Color((byte)120,(byte)120,(byte)140,(byte)255));

        if (shieldBlocked)
        {
            Program.DrawTextUI("2H WEAPON", sx + 16, sy + 32, 14, Color.DarkGray);
        }
        else if (value != null)
        {
            DrawArmorIcon(value, sx + 6, sy + 22, 28);
            Program.DrawTextUI(value, sx + 38, sy + 32, 13, Color.Gold);

               if (label == "AMMO")
                {
                     int ammoCount = value == "Arrows" ? player.Arrows
                                  : value == "Bolts" ? player.Bolts
                                  : value == "Arcane Essence" ? player.ArcaneEssence : 0;
                    Program.DrawTextUI($"x{ammoCount}", sx + 38, sy + 16, 14, Color.White);
                }

            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, 140, 60)) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                UnequipArmorSlot(label);
                ShowNotification($"Unequipped {value}");
            }
        }
        else
        {
            Program.DrawTextUI("Empty", sx + 40, sy + 32, 14, Color.DarkGray);
        }
    }

    DrawMannequin(panelX + 280 + 70, panelY + 380);

    // ── TABS: OWNED vs TEST ──
    Rectangle ownedTab = new Rectangle(panelX + 20, panelY + 432, 150, 32);
    Rectangle testTab  = new Rectangle(panelX + 180, panelY + 432, 150, 32);
    bool hoverOwned = Raylib.CheckCollisionPointRec(mouse, ownedTab);
    bool hoverTest  = Raylib.CheckCollisionPointRec(mouse, testTab);

    Raylib.DrawRectangleRec(ownedTab, !gearTestMode ? new Color((byte)70,(byte)55,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(ownedTab, 2, !gearTestMode ? Color.Gold : (hoverOwned ? Color.Gold : Color.White));
    Program.DrawTextUI("MY GEAR", panelX + 50, panelY + 440, 18, !gearTestMode ? Color.Gold : Color.White);

    Raylib.DrawRectangleRec(testTab, gearTestMode ? new Color((byte)70,(byte)55,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(testTab, 2, gearTestMode ? Color.Gold : (hoverTest ? Color.Gold : Color.White));
    Program.DrawTextUI("TEST (ALL)", panelX + 205, panelY + 440, 18, gearTestMode ? Color.Gold : Color.White);

    if (hoverOwned && Raylib.IsMouseButtonPressed(MouseButton.Left)) gearTestMode = false;
    if (hoverTest  && Raylib.IsMouseButtonPressed(MouseButton.Left)) gearTestMode = true;

     if (gearTestMode)
    {
        Rectangle mageBtn = new Rectangle(panelX + 400, panelY + 436, 120, 26);
        Rectangle rangBtn = new Rectangle(panelX + 530, panelY + 436, 130, 26);
        bool hM  = Raylib.CheckCollisionPointRec(mouse, mageBtn);
        bool hRg = Raylib.CheckCollisionPointRec(mouse, rangBtn);

        Raylib.DrawRectangleRec(mageBtn, new Color((byte)45,(byte)30,(byte)70,(byte)255));
        Raylib.DrawRectangleLinesEx(mageBtn, 2, hM ? Color.Gold : new Color((byte)130,(byte)70,(byte)200,(byte)255));
        Program.DrawTextUI("MAGE SET", panelX + 418, panelY + 441, 16, hM ? Color.Gold : Color.White);

        Raylib.DrawRectangleRec(rangBtn, new Color((byte)25,(byte)45,(byte)25,(byte)255));
        Raylib.DrawRectangleLinesEx(rangBtn, 2, hRg ? Color.Gold : new Color((byte)70,(byte)130,(byte)60,(byte)255));
        Program.DrawTextUI("RANGER SET", panelX + 546, panelY + 441, 16, hRg ? Color.Gold : Color.White);

        if (hM && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            string mt = mageTiers[testMageTier];
            armorHelmet = $"{mt} Mage Hat"; armorBody = $"{mt} Mage Top"; armorLegs = $"{mt} Mage Bottoms";
            armorBoots = $"{mt} Mage Boots"; armorGloves = $"{mt} Mage Gloves"; armorCape = $"{mt} Mage Robe Cape";
            if (!IsTwoHandedWeapon(armorWeapon)) armorShield = $"{mt} Mage Book";
            ShowNotification($"Equipped {mt} Mage set");
            testMageTier = (testMageTier + 1) % mageTiers.Length;
        }
        if (hRg && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            string rt = rangerTiers[testRangerTier];
            armorHelmet = $"{rt} Ranger Hat"; armorBody = $"{rt} Ranger Tunic"; armorLegs = $"{rt} Ranger Chaps";
            armorBoots = $"{rt} Ranger Boots"; armorGloves = $"{rt} Ranger Bracers"; armorCape = $"{rt} Ranger Quiver";
            ShowNotification($"Equipped {rt} Ranger set");
            testRangerTier = (testRangerTier + 1) % rangerTiers.Length;
        }
    }

    Raylib.DrawRectangle(panelX + 10, panelY + 470, panelW - 20, 2, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // ── ITEM LIST ──
    string[] testCatalog = {
        "Leather Cap","Iron Helmet","Steel Helmet",
        "Leather Vest","Iron Chestplate","Steel Chestplate",
        "Leather Pants","Iron Leggings","Steel Leggings",
        "Leather Boots","Iron Boots","Steel Boots",
        "Leather Gloves","Iron Gauntlets","Steel Gauntlets",
        "Wool Cape","Magic Cape",
        "Wooden Shield","Iron Shield","Steel Shield",
        "Sword","Great Sword","War Axe","Battle Staff",
        "Bow","Crossbow",
        "Mage Hat","Mage Top","Mage Bottoms","Mage Boots","Mage Gloves","Mage Robe Cape","Mage Book",
        "Ranger Hat","Ranger Tunic","Ranger Chaps","Ranger Boots","Ranger Bracers","Ranger Quiver"
    };

    List<string> shown = gearTestMode
        ? testCatalog.ToList()
        : player.OwnedGear;

if (gearTestMode)
{
    int rowY = panelY + 478;
    for (int s = 0; s < 7; s++)
    {
        int ry = rowY + s * 22;
        string mat = armorMaterials[slotMaterialIndex[s]];
        string piece = slotPieceNames[s];

        // slot label
        Program.DrawTextUI(piece, panelX + 14, ry, 14, Color.LightGray);

        // left arrow
        Rectangle lb = new Rectangle(panelX + 120, ry - 2, 20, 18);
        bool hL = Raylib.CheckCollisionPointRec(mouse, lb);
        Raylib.DrawRectangleRec(lb, new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Program.DrawTextUI("<", panelX + 125, ry, 14, hL ? Color.Gold : Color.White);

        // material name (colored swatch)
        Program.DrawTextUI(mat, panelX + 148, ry, 14, Program.MaterialColor(mat));

        // right arrow
        Rectangle rb = new Rectangle(panelX + 250, ry - 2, 20, 18);
        bool hR = Raylib.CheckCollisionPointRec(mouse, rb);
        Raylib.DrawRectangleRec(rb, new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Program.DrawTextUI(">", panelX + 255, ry, 14, hR ? Color.Gold : Color.White);

        // equip button
        Rectangle eb = new Rectangle(panelX + 285, ry - 2, 70, 18);
        bool hE = Raylib.CheckCollisionPointRec(mouse, eb);
        Raylib.DrawRectangleRec(eb, new Color((byte)60,(byte)50,(byte)20,(byte)255));
        Program.DrawTextUI("Equip", panelX + 292, ry, 14, hE ? Color.Gold : Color.White);

        if (hL && Raylib.IsMouseButtonPressed(MouseButton.Left))
            slotMaterialIndex[s] = (slotMaterialIndex[s] + armorMaterials.Length - 1) % armorMaterials.Length;
        if (hR && Raylib.IsMouseButtonPressed(MouseButton.Left))
            slotMaterialIndex[s] = (slotMaterialIndex[s] + 1) % armorMaterials.Length;
        if (hE && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            string itemName = $"{mat} {piece}";
            switch (s)
            {
                case 0: armorHelmet = itemName; break;
                case 1: armorBody   = itemName; break;
                case 2: armorLegs   = itemName; break;
                case 3: armorBoots  = itemName; break;
                case 4: armorGloves = itemName; break;
                case 5: armorCape   = itemName; break;
                case 6: armorShield = itemName; break;
            }
            ShowNotification($"Equipped {itemName}");
        }
    }
    // ── WEAPON cyclers (right side) ──
    int wRowY = panelY + 478;
    int wx = panelX + 400;   // start to the right of armor
    Program.DrawTextUI("WEAPONS", wx, wRowY - 20, 16, Color.Gold);

    for (int s = 0; s < weaponPieceNames.Length; s++)
    {
        int ry = wRowY + s * 22;
        string mat = armorMaterials[weaponMaterialIndex[s]];
        string piece = weaponPieceNames[s];

        // label
        Program.DrawTextUI(piece, wx, ry, 13, Color.LightGray);

        // left arrow
        Rectangle lb = new Rectangle(wx + 90, ry - 2, 20, 18);
        bool hL = Raylib.CheckCollisionPointRec(mouse, lb);
        Raylib.DrawRectangleRec(lb, new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Program.DrawTextUI("<", wx + 95, ry, 14, hL ? Color.Gold : Color.White);

        // material name
        Program.DrawTextUI(mat, wx + 116, ry, 12, Program.MaterialColor(mat));

        // right arrow
        Rectangle rb = new Rectangle(wx + 200, ry - 2, 20, 18);
        bool hR = Raylib.CheckCollisionPointRec(mouse, rb);
        Raylib.DrawRectangleRec(rb, new Color((byte)40,(byte)40,(byte)50,(byte)255));
        Program.DrawTextUI(">", wx + 205, ry, 14, hR ? Color.Gold : Color.White);

        // equip button
        Rectangle eb = new Rectangle(wx + 230, ry - 2, 60, 18);
        bool hE = Raylib.CheckCollisionPointRec(mouse, eb);
        Raylib.DrawRectangleRec(eb, new Color((byte)60,(byte)50,(byte)20,(byte)255));
        Program.DrawTextUI("Equip", wx + 235, ry, 13, hE ? Color.Gold : Color.White);

        if (hL && Raylib.IsMouseButtonPressed(MouseButton.Left))
            weaponMaterialIndex[s] = (weaponMaterialIndex[s] + armorMaterials.Length - 1) % armorMaterials.Length;
        if (hR && Raylib.IsMouseButtonPressed(MouseButton.Left))
            weaponMaterialIndex[s] = (weaponMaterialIndex[s] + 1) % armorMaterials.Length;
        if (hE && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            string itemName = $"{mat} {piece}";
            // route to 1H or 2H slot
            if (piece == "Sword")
            {
                equipped1H = itemName;
                equipped2H = null;
            }
            else
            {
                equipped2H = itemName;
                equipped1H = null;
                armorShield = null;
            }
            armorWeapon = itemName;
            ShowNotification($"Equipped {itemName}");
        }
    }
}


   else if (!gearTestMode && shown.Count == 0)
    {
        Program.DrawTextUI("No gear yet — craft, loot, or buy some!", panelX + 20, panelY + 485, 18, Color.Gray);
    }
    else
    {
        int itemSlot = 60, itemPad = 8;
        int perRow = 10;
        for (int i = 0; i < shown.Count && i < perRow * 2; i++)   // up to 2 rows of 10
        {
            int col = i % perRow;
            int row = i / perRow;
            int ix = panelX + 14 + col * (itemSlot + itemPad);
            int iy = panelY + 478 + row * (itemSlot + 6);

            string item = shown[i];
            bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ix, iy, itemSlot, itemSlot));
            bool equipped = item == armorHelmet || item == armorBody || item == armorLegs
                         || item == armorBoots || item == armorGloves || item == armorCape
                         || item == armorWeapon || item == armorShield;

            Raylib.DrawRectangle(ix, iy, itemSlot, itemSlot, new Color((byte)40,(byte)40,(byte)50,(byte)255));
            Raylib.DrawRectangleLines(ix, iy, itemSlot, itemSlot,
                equipped ? Color.Green : (hover ? Color.Gold : new Color((byte)80,(byte)80,(byte)100,(byte)255)));

            DrawArmorIcon(item, ix + 4, iy + 6, 24);
            int nameW = Program.MeasureTextUI(item.Length > 8 ? item.Substring(0,8) : item, 11);
            Program.DrawTextUI(item.Length > 8 ? item.Substring(0,8) : item, ix + itemSlot/2 - nameW/2, iy + itemSlot - 16, 11, Color.LightGray);

            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
             // delete   if (gearTestMode)
                    TryEquipItem(item);              // test mode equips anything
            }
        }
    }

    string hint = gearTestMode
        ? "TEST MODE — equip anything  |  Click slot to unequip  |  G = Close"
        : "Click item to equip  |  Click slot to unequip  |  G = Close";
    Program.DrawTextUI(hint, panelX + 30, panelY + panelH - 26, 15, Color.LightGray);
}

static void DrawPlayerMenu()
{
    if (!playerMenuOpen) return;
    Vector2 mouse = Raylib.GetMousePosition();

    // full-screen dark backdrop
     Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)10,(byte)12,(byte)18,(byte)255));

    // ── left tab rail ──
    int railW = 220;
    Raylib.DrawRectangle(0, 0, railW, ScreenHeight, new Color((byte)18,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangle(railW - 2, 0, 2, ScreenHeight, Color.Gold);
    Program.DrawTextUI("PLAYER", 24, 24, 30, Color.Gold);
    Program.DrawTextUI("[J] or ESC to close", 24, 60, 18, Color.Gray);

    for (int i = 0; i < playerMenuTabs.Length; i++)
    {
        var (tab, label) = playerMenuTabs[i];
        int ty = 110 + i * 54;
        Rectangle r = new Rectangle(12, ty, railW - 24, 46);
        bool hov = Raylib.CheckCollisionPointRec(mouse, r);
        bool active = playerMenuTab == tab;
        Raylib.DrawRectangleRec(r, active ? new Color((byte)50,(byte)55,(byte)80,(byte)255)
                                          : hov ? new Color((byte)30,(byte)33,(byte)48,(byte)255)
                                                : new Color((byte)22,(byte)24,(byte)36,(byte)255));
        if (active) Raylib.DrawRectangle(12, ty, 4, 46, Color.Gold);
        Program.DrawTextUI(label, 30, ty + 13, 20, active ? Color.Gold : Color.White);
        if (hov && Raylib.IsMouseButtonPressed(MouseButton.Left)) playerMenuTab = tab;
    }

    // ── content panel ──
    int cx = railW + 30, cy = 30, cw = ScreenWidth - railW - 60;
    switch (playerMenuTab)
    {
        case PlayerMenuTab.Identity:      DrawPMIdentity(cx, cy, cw); break;
        case PlayerMenuTab.Stats:         DrawPMStats(cx, cy, cw); break;
        case PlayerMenuTab.Crafting:      DrawPMCrafting(cx, cy, cw); break;
        case PlayerMenuTab.Achievements:  DrawPMAchievements(cx, cy, cw); break;
        case PlayerMenuTab.Bestiary:      DrawPMBestiary(cx, cy, cw); break;
        case PlayerMenuTab.Unlocks:       DrawPMUnlocks(cx, cy, cw); break;
        case PlayerMenuTab.Relationships: DrawPMRelationships(cx, cy, cw); break;
        case PlayerMenuTab.Collectables:  DrawPMCollectables(cx, cy, cw); break;
        case PlayerMenuTab.Collection:    DrawIdentityCollectionPage(cx, cy, cw); break;
    }   
}

static void DrawHallMannequin(int x, int y, HallItem item, Color skin)
{
    // scale 2.5x — all offsets multiplied from DrawFacingDown base (40x70 sprite)
    // base: head circle at +20,+12 r12, body +10,+24 20x30, legs +10,+54 etc
    int s = 3; // scale factor

    // head
    Raylib.DrawCircle(x + 20*s/2, y + 12*s/2, 12*s/2, skin);
    // eyes
    Raylib.DrawCircle(x + 15*s/2, y + 11*s/2, 2*s/2, Color.Black);
    Raylib.DrawCircle(x + 25*s/2, y + 11*s/2, 2*s/2, Color.Black);
    // mouth
    Raylib.DrawRectangle(x + 15*s/2, y + 17*s/2, 10*s/2, 2*s/2, new Color((byte)150,(byte)80,(byte)80,(byte)255));

    // draw clothing style BODY first, then arms on top
    DrawHallClothingBody(x, y, s, item, skin);

    // arms — always skin coloured, drawn after body clothing
    Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 18*s/2, skin);
    Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 18*s/2, skin);

    // legs
    DrawHallClothingLegs(x, y, s, item, skin);

    // accessories drawn on top of everything
    DrawHallAccessory(x, y, s, item, skin);
}

static void DrawHallClothingBody(int x, int y, int s, HallItem item, Color skin)
{
    Color c = item.Primary;
    Color c2 = item.Secondary;

    switch (item.Style)
    {
        case "tee":
            // full torso rectangle
            Raylib.DrawRectangle(x + 10*s/2, y + 24*s/2, 20*s/2, 30*s/2, c);
            // collar
            Raylib.DrawRectangle(x + 14*s/2, y + 24*s/2, 12*s/2, 4*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            // short sleeves covering top of arms
            Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 8*s/2, c);
            Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 8*s/2, c);
            break;

        case "singlet":
            // narrower body, no sleeves
            Raylib.DrawRectangle(x + 12*s/2, y + 24*s/2, 16*s/2, 30*s/2, c);
            // thin straps
            Raylib.DrawRectangle(x + 12*s/2, y + 22*s/2, 3*s/2, 6*s/2, c);
            Raylib.DrawRectangle(x + 25*s/2, y + 22*s/2, 3*s/2, 6*s/2, c);
            break;

        case "stripe":
            // alternating horizontal stripes
            Raylib.DrawRectangle(x + 10*s/2, y + 24*s/2, 20*s/2, 30*s/2, c);
            Raylib.DrawRectangle(x + 10*s/2, y + 26*s/2, 20*s/2, 4*s/2, c2);
            Raylib.DrawRectangle(x + 10*s/2, y + 34*s/2, 20*s/2, 4*s/2, c2);
            Raylib.DrawRectangle(x + 10*s/2, y + 42*s/2, 20*s/2, 4*s/2, c2);
            Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 8*s/2, c);
            Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 8*s/2, c);
            break;

        case "polo":
            // body
            Raylib.DrawRectangle(x + 10*s/2, y + 24*s/2, 20*s/2, 30*s/2, c);
            // collar (wider, raised)
            Raylib.DrawRectangle(x + 13*s/2, y + 22*s/2, 14*s/2, 6*s/2, c2);
            Raylib.DrawRectangle(x + 16*s/2, y + 22*s/2, 8*s/2, 8*s/2, c); // collar notch
            // short sleeves
            Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 10*s/2, c);
            Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 10*s/2, c);
            // sleeve stripe
            Raylib.DrawRectangle(x + 2*s/2,  y + 34*s/2, 8*s/2, 2*s/2, c2);
            Raylib.DrawRectangle(x + 30*s/2, y + 34*s/2, 8*s/2, 2*s/2, c2);
            break;

        case "hoodie":
            // body — slightly bulkier
            Raylib.DrawRectangle(x + 9*s/2, y + 24*s/2, 22*s/2, 30*s/2, c);
            // hood raised behind head
            Raylib.DrawRectangle(x + 10*s/2, y + 16*s/2, 20*s/2, 10*s/2, new Color((byte)Math.Max(0,c.R-15),(byte)Math.Max(0,c.G-15),(byte)Math.Max(0,c.B-15),(byte)255));
            // front kangaroo pocket
            Raylib.DrawRectangle(x + 13*s/2, y + 38*s/2, 14*s/2, 10*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            // long sleeves to wrist
            Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 18*s/2, c);
            Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 18*s/2, c);
            // cuffs
            Raylib.DrawRectangle(x + 2*s/2,  y + 42*s/2, 8*s/2, 2*s/2, new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
            Raylib.DrawRectangle(x + 30*s/2, y + 42*s/2, 8*s/2, 2*s/2, new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
            break;

        case "jacket":
            // body
            Raylib.DrawRectangle(x + 9*s/2, y + 24*s/2, 22*s/2, 30*s/2, c);
            // lapels
            Raylib.DrawRectangle(x + 14*s/2, y + 24*s/2, 4*s/2, 14*s/2, c2);
            Raylib.DrawRectangle(x + 22*s/2, y + 24*s/2, 4*s/2, 14*s/2, c2);
            // zip line
            Raylib.DrawRectangle(x + 19*s/2, y + 26*s/2, 2*s/2, 26*s/2, c2);
            // long sleeves
            Raylib.DrawRectangle(x + 2*s/2,  y + 26*s/2, 8*s/2, 18*s/2, c);
            Raylib.DrawRectangle(x + 30*s/2, y + 26*s/2, 8*s/2, 18*s/2, c);
            break;

        case "puffer":
            // puffer segments (horizontal quilted lines)
            Raylib.DrawRectangle(x + 9*s/2, y + 24*s/2, 22*s/2, 30*s/2, c);
            for (int seg = 0; seg < 4; seg++)
                Raylib.DrawRectangle(x + 9*s/2, y + (26 + seg*7)*s/2, 22*s/2, 2*s/2,
                    new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
            // puffy sleeves
            Raylib.DrawRectangle(x + 1*s/2,  y + 26*s/2, 10*s/2, 18*s/2, c);
            Raylib.DrawRectangle(x + 29*s/2, y + 26*s/2, 10*s/2, 18*s/2, c);
            for (int seg = 0; seg < 3; seg++)
            {
                Raylib.DrawRectangle(x + 1*s/2, y + (28 + seg*5)*s/2, 10*s/2, 2*s/2, new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
                Raylib.DrawRectangle(x + 29*s/2, y + (28 + seg*5)*s/2, 10*s/2, 2*s/2, new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
            }
            break;

        default:
            // fallback: plain tee
            Raylib.DrawRectangle(x + 10*s/2, y + 24*s/2, 20*s/2, 30*s/2, c);
            break;
    }
}

static void DrawHallClothingLegs(int x, int y, int s, HallItem item, Color skin)
{
    Color c = item.Primary;
    Color c2 = item.Secondary;
    Color defaultPants = player.PantsColor;

    switch (item.Style)
    {
        case "jeans":
            // full length legs
            Raylib.DrawRectangle(x + 10*s/2, y + 54*s/2, 8*s/2, 18*s/2, c);
            Raylib.DrawRectangle(x + 22*s/2, y + 54*s/2, 8*s/2, 18*s/2, c);
            // seam line
            Raylib.DrawRectangle(x + 17*s/2, y + 54*s/2, 2*s/2, 18*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            // waistband
            Raylib.DrawRectangle(x + 10*s/2, y + 52*s/2, 20*s/2, 4*s/2, new Color((byte)Math.Max(0,c.R-25),(byte)Math.Max(0,c.G-25),(byte)Math.Max(0,c.B-25),(byte)255));
            break;

        case "chinos":
            Raylib.DrawRectangle(x + 10*s/2, y + 54*s/2, 8*s/2, 18*s/2, c);
            Raylib.DrawRectangle(x + 22*s/2, y + 54*s/2, 8*s/2, 18*s/2, c);
            // crease line
            Raylib.DrawRectangle(x + 13*s/2, y + 54*s/2, 1*s/2, 18*s/2, new Color((byte)Math.Max(0,c.R+20),(byte)Math.Max(0,c.G+20),(byte)Math.Max(0,c.B+20),(byte)255));
            Raylib.DrawRectangle(x + 25*s/2, y + 54*s/2, 1*s/2, 18*s/2, new Color((byte)Math.Max(0,c.R+20),(byte)Math.Max(0,c.G+20),(byte)Math.Max(0,c.B+20),(byte)255));
            Raylib.DrawRectangle(x + 10*s/2, y + 52*s/2, 20*s/2, 4*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            break;

        case "shorts":
            // shorter — only goes half as far down
            Raylib.DrawRectangle(x + 10*s/2, y + 54*s/2, 8*s/2, 9*s/2, c);
            Raylib.DrawRectangle(x + 22*s/2, y + 54*s/2, 8*s/2, 9*s/2, c);
            Raylib.DrawRectangle(x + 10*s/2, y + 52*s/2, 20*s/2, 4*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            // exposed legs below
            Raylib.DrawRectangle(x + 10*s/2, y + 63*s/2, 8*s/2, 9*s/2, skin);
            Raylib.DrawRectangle(x + 22*s/2, y + 63*s/2, 8*s/2, 9*s/2, skin);
            break;

        case "camo":
            Raylib.DrawRectangle(x + 10*s/2, y + 54*s/2, 8*s/2, 9*s/2, c);
            Raylib.DrawRectangle(x + 22*s/2, y + 54*s/2, 8*s/2, 9*s/2, c);
            // camo patches
            Raylib.DrawRectangle(x + 11*s/2, y + 55*s/2, 3*s/2, 2*s/2, c2);
            Raylib.DrawRectangle(x + 14*s/2, y + 58*s/2, 2*s/2, 2*s/2, c2);
            Raylib.DrawRectangle(x + 23*s/2, y + 56*s/2, 3*s/2, 2*s/2, c2);
            Raylib.DrawRectangle(x + 10*s/2, y + 52*s/2, 20*s/2, 4*s/2, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
            Raylib.DrawRectangle(x + 10*s/2, y + 63*s/2, 8*s/2, 9*s/2, skin);
            Raylib.DrawRectangle(x + 22*s/2, y + 63*s/2, 8*s/2, 9*s/2, skin);
            break;

        default:
            // not a bottoms item — draw player's current pants
            Raylib.DrawRectangle(x + 10*s/2, y + 54*s/2, 8*s/2, 18*s/2, defaultPants);
            Raylib.DrawRectangle(x + 22*s/2, y + 54*s/2, 8*s/2, 18*s/2, defaultPants);
            break;
    }
}

static void DrawHallAccessory(int x, int y, int s, HallItem item, Color skin)
{
    Color c = item.Primary;

    switch (item.Style)
    {
        case "cap":
            // brim
            Raylib.DrawRectangle(x + 7*s/2,  y + 8*s/2, 26*s/2, 4*s/2, c);
            // dome
            Raylib.DrawRectangle(x + 9*s/2,  y + 2*s/2, 22*s/2, 8*s/2, c);
            Raylib.DrawRectangle(x + 20*s/2, y + 8*s/2, 4*s/2, 2*s/2, new Color((byte)Math.Max(0,c.R+40),(byte)Math.Max(0,c.G+40),(byte)Math.Max(0,c.B+40),(byte)255)); // button
            break;

        case "beanie":
            Raylib.DrawRectangle(x + 9*s/2,  y + 2*s/2,  22*s/2, 10*s/2, c);
            // ribbed band
            for (int r = 0; r < 3; r++)
                Raylib.DrawRectangle(x + 9*s/2, y + (8 + r*2)*s/2, 22*s/2, 1*s/2, new Color((byte)Math.Max(0,c.R-30),(byte)Math.Max(0,c.G-30),(byte)Math.Max(0,c.B-30),(byte)255));
            // pom
            Raylib.DrawCircle(x + 20*s/2, y + 1*s/2, 3*s/2, new Color((byte)Math.Min(255,c.R+40),(byte)Math.Min(255,c.G+40),(byte)Math.Min(255,c.B+40),(byte)255));
            break;

        case "sunnies":
            // two lenses + bridge
            Raylib.DrawRectangle(x + 11*s/2, y + 10*s/2, 7*s/2, 5*s/2, c);
            Raylib.DrawRectangle(x + 22*s/2, y + 10*s/2, 7*s/2, 5*s/2, c);
            Raylib.DrawRectangle(x + 18*s/2, y + 11*s/2, 4*s/2, 2*s/2, c); // bridge
            Raylib.DrawRectangle(x + 8*s/2,  y + 10*s/2, 3*s/2, 2*s/2, c); // left arm
            Raylib.DrawRectangle(x + 29*s/2, y + 10*s/2, 3*s/2, 2*s/2, c); // right arm
            break;

        case "chain":
            // gold chain around neck
            Raylib.DrawRectangle(x + 13*s/2, y + 22*s/2, 14*s/2, 2*s/2, c);
            Raylib.DrawRectangle(x + 18*s/2, y + 24*s/2, 4*s/2, 6*s/2, c); // pendant drop
            Raylib.DrawCircle(x + 20*s/2, y + 31*s/2, 2*s/2, c);           // pendant circle
            break;
    }
}

static void DrawBarnShopUI()
{
    if (!barnShopOpen) return;

    int px = ScreenWidth/2 - 340, py = 50, pw = 680, ph = 560;
    Color brown = new Color((byte)170,(byte)110,(byte)50,(byte)255);
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)25,(byte)16,(byte)8,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, brown);
    Program.DrawTextUI("LIVESTOCK & SUPPLIES", px + 190, py + 12, 26, brown);
    Program.DrawTextUI($"${player.Money}", px + pw - 100, py + 16, 20, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();
    int rowH = 50, y = py + 56;

    Program.DrawTextUI("ANIMALS  (placed in a pen behind the barn)", px + 24, y, 18, new Color((byte)200,(byte)170,(byte)110,(byte)255));
    y += 30;
    for (int i = 0; i < barnStock.Length; i++)
    {
        var (animal, price, produce, cycle, feed) = barnStock[i];
        Rectangle row = new Rectangle(px + 20, y + i * rowH, pw - 40, rowH - 8);
        bool hover = Raylib.CheckCollisionPointRec(mouse, row);
        bool canAfford = player.Money >= price;
        Raylib.DrawRectangleRec(row, new Color((byte)38,(byte)26,(byte)14,(byte)255));
        Raylib.DrawRectangleLinesEx(row, hover ? 2 : 1, hover ? brown : new Color((byte)70,(byte)50,(byte)28,(byte)255));
        Program.DrawTextUI($"{animal}  →  {produce}", px + 32, y + i * rowH + 8, 18, Color.White);
        Program.DrawTextUI($"feed: {feed}", px + 320, y + i * rowH + 10, 15, new Color((byte)170,(byte)170,(byte)175,(byte)255));
        Program.DrawTextUI($"${price}", px + pw - 96, y + i * rowH + 10, 16, canAfford ? Color.Gold : Color.Red);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!canAfford) ShowNotification($"Need ${price}!");
            else
            {
                player.Money -= price;
                // place a pen near the farming area origin, offset per existing pen count
                int n = livestockPens.Count;
                Vector2 penPos = new Vector2(-1200 + (n % 5) * 160, -9200 + (n / 5) * 160);
                livestockPens.Add(new LivestockPen {
                    Position = penPos, Animal = animal, Produce = produce,
                    Feed = feed, Cycle = cycle, Timer = 0f
                });
                ShowNotification($"Bought a {animal}! Pen placed on your farm.");
            }
        }
    }

    y += barnStock.Length * rowH + 16;
    Program.DrawTextUI("SUPPLIES", px + 24, y, 18, new Color((byte)200,(byte)170,(byte)110,(byte)255));
    y += 28;
    for (int i = 0; i < barnSupplies.Length; i++)
    {
        var (item, price) = barnSupplies[i];
        Rectangle row = new Rectangle(px + 20, y + i * (rowH - 8), pw - 40, rowH - 14);
        bool hover = Raylib.CheckCollisionPointRec(mouse, row);
        bool canAfford = player.Money >= price;
        Raylib.DrawRectangleRec(row, new Color((byte)38,(byte)26,(byte)14,(byte)255));
        Raylib.DrawRectangleLinesEx(row, hover ? 2 : 1, hover ? brown : new Color((byte)70,(byte)50,(byte)28,(byte)255));
        Program.DrawTextUI(item, px + 32, y + i * (rowH - 8) + 8, 18, Color.White);
        Program.DrawTextUI($"${price}", px + pw - 96, y + i * (rowH - 8) + 8, 16, canAfford ? Color.Gold : Color.Red);
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!canAfford) ShowNotification($"Need ${price}!");
            else if (TryGiveItem(item, 1)) { player.Money -= price; ShowNotification($"Bought {item}!"); }
            else ShowNotification("Inventory full!");
        }
    }

    Program.DrawTextUI("Q = Close", px + pw/2 - 38, py + ph - 26, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) barnShopOpen = false;
}

static void DrawHouseMenuUI()
{
    if (!houseMenuOpen || ActiveHouseData == null) return;

    int pw = 1240; int ph = 680;
    int px = ScreenWidth  / 2 - pw / 2;
    int py = ScreenHeight / 2 - ph / 2;
    Color gold = new Color((byte)220,(byte)180,(byte)40,(byte)255);
    Vector2 mouse = Raylib.GetMousePosition();

    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)8,(byte)8,(byte)18,(byte)250));
    Raylib.DrawRectangleLines(px, py, pw, ph, gold);
    Program.DrawTextUI("MY HOUSE — CUSTOMISE", px + pw/2 - 160, py + 10, 28, gold);
    Program.DrawTextUI($"Wallet: ${player.Money}", px + pw - 200, py + 14, 20, Color.LightGray);

    // ── TABS: Wall / Floor / Furniture ────────────────────────────────────────
    string[] mainTabs = { "Furniture", "Wall Colour", "Floor Colour" };
    for (int i = 0; i < mainTabs.Length; i++)
    {
        Rectangle tab = new Rectangle(px + 20 + i * 200, py + 46, 190, 30);
        bool active = furnitureCategoryIndex == -10 - i  // -10 = Wall, -11 = Floor
            || (i == 0 && furnitureCategoryIndex >= 0);
        // use -10 for wall tab, -11 for floor tab, >=0 for furniture tab
        bool thisTab = (i == 0 && furnitureCategoryIndex >= 0)
                    || (i == 1 && furnitureCategoryIndex == -10)
                    || (i == 2 && furnitureCategoryIndex == -11);
        Raylib.DrawRectangleRec(tab, thisTab
            ? new Color((byte)60,(byte)50,(byte)10,(byte)255)
            : new Color((byte)25,(byte)25,(byte)35,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 2, thisTab ? gold : Color.DarkGray);
        Program.DrawTextUI(mainTabs[i], (int)tab.X + 12, (int)tab.Y + 7, 17,
            thisTab ? gold : Color.LightGray);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
            Raylib.CheckCollisionPointRec(mouse, tab))
        {
            furnitureCategoryIndex = i == 0 ? 0 : (i == 1 ? -10 : -11);
            furnitureShopScroll = 0;
        }
    }

    int contentY = py + 86;

    // ══ WALL COLOUR TAB ═══════════════════════════════════════════════════════
    if (furnitureCategoryIndex == -10)
    {
        Program.DrawTextUI("Choose Wall Colour:", px + 30, contentY + 20, 22, Color.LightGray);
        string[] walls = { "Beige","White","Blue","Green","Red","Yellow","Pink","Dark" };
        Color[] wallPreviews = {
            new Color((byte)210,(byte)190,(byte)150,(byte)255),
            new Color((byte)240,(byte)240,(byte)235,(byte)255),
            new Color((byte)80, (byte)120,(byte)180,(byte)255),
            new Color((byte)80, (byte)150,(byte)80, (byte)255),
            new Color((byte)180,(byte)70, (byte)70, (byte)255),
            new Color((byte)200,(byte)185,(byte)60, (byte)255),
            new Color((byte)200,(byte)120,(byte)160,(byte)255),
            new Color((byte)40, (byte)35, (byte)50, (byte)255),
        };
        for (int i = 0; i < walls.Length; i++)
        {
            Rectangle wb = new Rectangle(px + 30 + i * 148, contentY + 60, 136, 80);
            bool sel = ActiveHouseData.WallColor == walls[i];
            bool hov = Raylib.CheckCollisionPointRec(mouse, wb);
            Raylib.DrawRectangleRec(wb, wallPreviews[i]);
            Raylib.DrawRectangleLinesEx(wb, sel ? 4 : 2, sel ? Color.Gold : (hov ? Color.White : Color.DarkGray));
            Program.DrawTextUI(walls[i], (int)wb.X + 4, (int)wb.Y + 62, 15,
                sel ? Color.Gold : Color.White);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && hov)
            { ActiveHouseData.WallColor = walls[i]; SpawnPlayerHouse(); }
        }
    }

    // ══ FLOOR COLOUR TAB ══════════════════════════════════════════════════════
    else if (furnitureCategoryIndex == -11)
    {
        Program.DrawTextUI("Choose Floor:", px + 30, contentY + 20, 22, Color.LightGray);
        string[] floors = { "Oak","Pine","Stone","Carpet","Tile","Marble","Concrete","Bamboo" };
        Color[] floorPreviews = {
            new Color((byte)160,(byte)110,(byte)60, (byte)255),
            new Color((byte)190,(byte)150,(byte)90, (byte)255),
            new Color((byte)120,(byte)120,(byte)115,(byte)255),
            new Color((byte)100,(byte)80, (byte)140,(byte)255),
            new Color((byte)200,(byte)200,(byte)195,(byte)255),
            new Color((byte)210,(byte)205,(byte)195,(byte)255),
            new Color((byte)90, (byte)90, (byte)88, (byte)255),
            new Color((byte)140,(byte)175,(byte)100,(byte)255),
        };
        for (int i = 0; i < floors.Length; i++)
        {
            Rectangle fb = new Rectangle(px + 30 + i * 148, contentY + 60, 136, 80);
            bool sel = ActiveHouseData.FloorColor == floors[i];
            bool hov = Raylib.CheckCollisionPointRec(mouse, fb);
            // draw floor tile pattern
            for (int tx = 0; tx < 4; tx++)
                for (int ty = 0; ty < 3; ty++)
                    Raylib.DrawRectangle((int)fb.X + tx * 34, (int)fb.Y + ty * 26, 33, 25,
                        new Color((byte)(floorPreviews[i].R + (tx + ty) % 2 * 10),
                                  (byte)(floorPreviews[i].G + (tx + ty) % 2 * 8),
                                  (byte)(floorPreviews[i].B + (tx + ty) % 2 * 6),
                                  (byte)255));
            Raylib.DrawRectangleLinesEx(fb, sel ? 4 : 2, sel ? Color.Gold : (hov ? Color.White : Color.DarkGray));
            Program.DrawTextUI(floors[i], (int)fb.X + 4, (int)fb.Y + 62, 15,
                sel ? Color.Gold : Color.White);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && hov)
                ActiveHouseData.FloorColor = floors[i];
        }
    }

    // ══ FURNITURE TAB ═════════════════════════════════════════════════════════
    else
    {
        // category tabs
        string[] cats = { "All","Seating","Surfaces","Kitchen","Electronics","Plants","Walls","Misc" };
        for (int i = 0; i < cats.Length; i++)
        {
            Rectangle ct = new Rectangle(px + 20 + i * 150, contentY + 4, 142, 26);
            bool active = furnitureCategoryIndex == i;
            Raylib.DrawRectangleRec(ct, active
                ? new Color((byte)40,(byte)80,(byte)40,(byte)255)
                : new Color((byte)20,(byte)20,(byte)30,(byte)255));
            Raylib.DrawRectangleLinesEx(ct, 1, active ? Color.Green : Color.DarkGray);
            Program.DrawTextUI(cats[i], (int)ct.X + 8, (int)ct.Y + 5, 15,
                active ? Color.Green : Color.LightGray);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                Raylib.CheckCollisionPointRec(mouse, ct))
            { furnitureCategoryIndex = i; furnitureShopScroll = 0; }
        }

        // all furniture items with categories
        (string name, int cost, string cat)[] allItems = {
            // Seating
            ("Sofa",        800,  "Seating"),
            ("Chair",       180,  "Seating"),
            ("Bench",       280,  "Seating"),
            ("Armchair",    420,  "Seating"),
            ("BabyChair",   150,  "Seating"),
            // Surfaces
            ("Table",       350,  "Surfaces"),
            ("Desk",        450,  "Surfaces"),
            ("CoffeeTable", 280,  "Surfaces"),
            ("NightStand",  200,  "Surfaces"),
            ("Shelf",       400,  "Surfaces"),
            ("Bench",       280,  "Surfaces"),
            // Kitchen
            ("Stove",       750,  "Kitchen"),
            ("Fridge",      900,  "Kitchen"),
            ("Cupboard",    380,  "Kitchen"),
            ("Sink",        320,  "Kitchen"),
            ("KitchenBench",300,  "Kitchen"),
            ("Dishwasher",  600,  "Kitchen"),
            // Electronics
            ("TV",         1200,  "Electronics"),
            ("Lamp",        200,  "Electronics"),
            ("PCDesk",      900,  "Electronics"),
            ("Speaker",     350,  "Electronics"),
            // Plants
            ("Plant",       150,  "Plants"),
            ("BigPlant",    280,  "Plants"),
            ("Cactus",      120,  "Plants"),
            ("FlowerPot",   90,   "Plants"),
            // Walls
            ("Wall",        500,  "Walls"),
            ("HalfWall",    300,  "Walls"),
            ("WallV",       500,  "Walls"),       
            ("HalfWallV",   300,  "Walls"),
            ("Toilet",      600,  "Walls"),
            ("Bathtub",     800,  "Walls"),
            ("Shower",      700,  "Walls"),
            // Misc
            ("Rug",         300,  "Misc"),
            ("Painting",    250,  "Misc"),
            ("Mirror",      180,  "Misc"),
            ("Bin",          60,  "Misc"),
            ("Fireplace",   950,  "Misc"),
        };

        string activeCat = cats[furnitureCategoryIndex];
        var filtered = activeCat == "All"
            ? allItems
            : allItems.Where(it => it.cat == activeCat).ToArray();

        // scrollable grid — 5 columns, cards 220w x 160h
        int cardW = 220; int cardH = 160; int cardPad = 10;
        int cols = 5;
        int rows = (int)Math.Ceiling(filtered.Length / (float)cols);
        int visRows = 3;
        int maxScroll = Math.Max(0, rows - visRows);
        furnitureShopScroll = Math.Clamp(furnitureShopScroll, 0, maxScroll);

        // scroll with mouse wheel
        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0) furnitureShopScroll = Math.Clamp(furnitureShopScroll - (int)wheel, 0, maxScroll);

        // scroll arrows
        Rectangle upArrow   = new Rectangle(px + pw - 36, contentY + 36,  28, 28);
        Rectangle downArrow = new Rectangle(px + pw - 36, py + ph - 52, 28, 28);
        bool hUp   = furnitureShopScroll > 0        && Raylib.CheckCollisionPointRec(mouse, upArrow);
        bool hDown = furnitureShopScroll < maxScroll && Raylib.CheckCollisionPointRec(mouse, downArrow);
        Raylib.DrawRectangleRec(upArrow,   new Color((byte)30,(byte)30,(byte)40,(byte)255));
        Raylib.DrawRectangleRec(downArrow, new Color((byte)30,(byte)30,(byte)40,(byte)255));
        Program.DrawTextUI("▲", (int)upArrow.X   + 4, (int)upArrow.Y   + 4, 20, hUp   ? gold : Color.DarkGray);
        Program.DrawTextUI("▼", (int)downArrow.X + 4, (int)downArrow.Y + 4, 20, hDown ? gold : Color.DarkGray);
        if (hUp   && Raylib.IsMouseButtonPressed(MouseButton.Left)) furnitureShopScroll--;
        if (hDown && Raylib.IsMouseButtonPressed(MouseButton.Left)) furnitureShopScroll++;

        // scroll bar
        if (maxScroll > 0)
        {
            int sbH  = py + ph - 80 - (contentY + 70);
            int sbY  = contentY + 70 + (int)(sbH * (furnitureShopScroll / (float)maxScroll));
            Raylib.DrawRectangle(px + pw - 30, contentY + 70, 8, sbH,
                new Color((byte)40,(byte)40,(byte)50,(byte)255));
            Raylib.DrawRectangle(px + pw - 30, sbY, 8, Math.Max(20, sbH / Math.Max(1, rows)),
                new Color((byte)120,(byte)100,(byte)40,(byte)255));
        }

        // draw grid with scissor clip
        Raylib.BeginScissorMode(px + 20, contentY + 36, pw - 60, visRows * (cardH + cardPad));

        for (int i = 0; i < filtered.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int visRow = row - furnitureShopScroll;
            if (visRow < 0 || visRow >= visRows) continue;

            int cx = px + 20 + col * (cardW + cardPad);
            int cy = contentY + 36 + visRow * (cardH + cardPad);
            Rectangle card = new Rectangle(cx, cy, cardW, cardH);
            bool canAfford = player.Money >= filtered[i].cost;
            bool hov = Raylib.CheckCollisionPointRec(mouse, card);

            // card background
            Raylib.DrawRectangleRec(card, new Color((byte)18,(byte)18,(byte)30,(byte)255));
            Raylib.DrawRectangleLinesEx(card, 2,
                hov && canAfford ? gold : (canAfford ? Color.DarkGray : new Color((byte)60,(byte)30,(byte)30,(byte)255)));

            // furniture preview centred in top 100px of card
            Raylib.BeginScissorMode(cx + 2, cy + 2, cardW - 4, 100);
            DrawFurniturePiece(filtered[i].name, cx + cardW/2 - 50, cy + 10);
            Raylib.EndScissorMode();

            // name + price
            int nw = Program.MeasureTextUI(filtered[i].name, 16);
            Program.DrawTextUI(filtered[i].name, cx + cardW/2 - nw/2, cy + 108, 16, Color.White);
            Program.DrawTextUI($"${filtered[i].cost}", cx + 10, cy + 132, 15,
                canAfford ? Color.Gold : Color.Red);
            Program.DrawTextUI(filtered[i].cat, cx + cardW - Program.MeasureTextUI(filtered[i].cat, 12) - 8,
                cy + 136, 12, new Color((byte)100,(byte)100,(byte)120,(byte)255));

            if (!canAfford)
                Program.DrawTextUI("Can't afford", cx + cardW/2 - 44, cy + cardH - 20, 13, Color.Red);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && hov && canAfford)
        {
            heldFurnitureType  = filtered[i].name;
            heldFurnitureIndex = -1;
            furniturePlaceMode = true;
            houseMenuOpen      = false;
            // spawn ghost at player's current interior position + small offset forward
            furnitureCursorX   = (int)player.Position.X + 60;
            furnitureCursorY   = (int)player.Position.Y + 80;
            ShowNotification($"Click to place your {filtered[i].name}! ESC to cancel.");
        }
        }

        Raylib.EndScissorMode();

        // page indicator
        Program.DrawTextUI($"Row {furnitureShopScroll+1}/{Math.Max(1,rows)} — Scroll to see more",
            px + 20, py + ph - 36, 15, Color.DarkGray);
    }

    Program.DrawTextUI("ESC = Close", px + pw - 130, py + ph - 28, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        houseMenuOpen      = false;
        furniturePlaceMode = false;
        heldFurnitureType  = "";
        heldFurnitureIndex = -1;
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

 static void DrawShopUI()
{
    if (!shopUIOpen) return;

    int panelX = ScreenWidth / 2 - 420;
    int panelY = 60;
    int slotSize = 70;
    int padding = 8;
    int cols = 5;

    Vector2 mouse = Raylib.GetMousePosition();

    // background
    Raylib.DrawRectangle(panelX, panelY, 840, 560, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 840, 560, Color.Gold);

    // ── MODE TABS ──────────────────────────────────────────────────────
    Rectangle sellTab = new Rectangle(panelX + 20, panelY + 12, 160, 40);
    Rectangle buyTab  = new Rectangle(panelX + 190, panelY + 12, 160, 40);
    bool hoverSellTab = Raylib.CheckCollisionPointRec(mouse, sellTab);
    bool hoverBuyTab  = Raylib.CheckCollisionPointRec(mouse, buyTab);

    Raylib.DrawRectangleRec(sellTab, !shopBuyMode ? new Color((byte)70,(byte)55,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(sellTab, 2, !shopBuyMode ? Color.Gold : (hoverSellTab ? Color.Gold : Color.White));
    Program.DrawTextUI("SELL", panelX + 70, panelY + 22, 22, !shopBuyMode ? Color.Gold : Color.White);

    Raylib.DrawRectangleRec(buyTab, shopBuyMode ? new Color((byte)70,(byte)55,(byte)20,(byte)255) : new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(buyTab, 2, shopBuyMode ? Color.Gold : (hoverBuyTab ? Color.Gold : Color.White));
    Program.DrawTextUI("BUY", panelX + 245, panelY + 22, 22, shopBuyMode ? Color.Gold : Color.White);

    if (hoverSellTab && Raylib.IsMouseButtonPressed(MouseButton.Left)) { shopBuyMode = false; shopSelectedItem = -1; shopSelectedItemName = ""; }
    if (hoverBuyTab  && Raylib.IsMouseButtonPressed(MouseButton.Left)) { shopBuyMode = true;  shopSelectedItem = -1; shopSelectedItemName = ""; }

    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 650, panelY + 22, 22, Color.Gold);

    // ════════════════════════════════════════════════════════════════════
    if (!shopBuyMode)
    {
        // ── SELL MODE — your inventory, click to sell ──
        string[] sellItems = { "Logs", "Birch Logs", "Oak Logs", "Pine Logs", "Arctic Logs", "Dead Wood", "Fish", "Bones", "Fur", "Stingers", "Pelts" };
        int[] sellPrices = { 5, 8, 12, 18, 25, 3, 10, 8, 15, 12, 25 };
        int[] invCounts = {
            player.Logs, player.BirchLogs, player.OakLogs, player.PineLogs,
            player.ArcticLogs, player.DeadWood, player.Fish, player.Bones,
            player.Fur, player.Stingers, player.BearPelts
        };

        Program.DrawTextUI("YOUR ITEMS — click to sell", panelX + 20, panelY + 62, 18, Color.LightGray);

        for (int i = 0; i < sellItems.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int sx = panelX + 20 + col * (slotSize + padding);
            int sy = panelY + 90 + row * (slotSize + padding);

            bool selected = shopSelectedItemName == sellItems[i];
            Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, selected ? Color.Gold : new Color((byte)100,(byte)100,(byte)100,(byte)255));

            if (invCounts[i] > 0)
            {
                DrawInventoryIcon(sellItems[i], sx, sy, slotSize);
                Program.DrawTextUI($"{invCounts[i]}", sx + 6, sy + 6, 16, Color.White);
                Program.DrawTextUI($"${sellPrices[i]}", sx + 6, sy + slotSize - 18, 14, Color.Gold);

                if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
                {
                    Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        shopSelectedItem = i;
                        shopSelectedItemName = sellItems[i];
                    }
                }
            }
        }

        // sell options panel
        if (shopSelectedItem >= 0 && shopSelectedItem < sellItems.Length)
        {
            int sellPanelY = panelY + 430;
            Raylib.DrawRectangle(panelX + 20, sellPanelY, 800, 100, new Color((byte)30,(byte)30,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(panelX + 20, sellPanelY, 800, 100, Color.Gold);

            int price = sellPrices[shopSelectedItem];
            int count = invCounts[shopSelectedItem];
            Program.DrawTextUI($"Selling: {shopSelectedItemName} @ ${price} each", panelX + 30, sellPanelY + 10, 20, Color.White);

            Rectangle sell1Btn = new Rectangle(panelX + 30, sellPanelY + 45, 160, 44);
            Rectangle sell5Btn = new Rectangle(panelX + 210, sellPanelY + 45, 160, 44);
            Rectangle sellAllBtn = new Rectangle(panelX + 390, sellPanelY + 45, 160, 44);
            bool h1 = Raylib.CheckCollisionPointRec(mouse, sell1Btn);
            bool h5 = Raylib.CheckCollisionPointRec(mouse, sell5Btn);
            bool hA = Raylib.CheckCollisionPointRec(mouse, sellAllBtn);

            Raylib.DrawRectangleRec(sell1Btn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(sell1Btn, 2, h1 ? Color.Gold : Color.White);
            Program.DrawTextUI($"Sell 1 (${price})", panelX + 40, sellPanelY + 57, 18, h1 ? Color.Gold : Color.White);

            Raylib.DrawRectangleRec(sell5Btn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(sell5Btn, 2, h5 ? Color.Gold : Color.White);
            Program.DrawTextUI($"Sell 5 (${price * Math.Min(5, count)})", panelX + 220, sellPanelY + 57, 18, h5 ? Color.Gold : Color.White);

            Raylib.DrawRectangleRec(sellAllBtn, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLinesEx(sellAllBtn, 2, hA ? Color.Gold : Color.White);
            Program.DrawTextUI($"Sell All (${price * count})", panelX + 400, sellPanelY + 57, 18, hA ? Color.Gold : Color.White);

            if (h1 && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
            {
                SellItem(shopSelectedItemName, 1, price);
                if (GetItemCount(shopSelectedItemName) <= 0) { shopSelectedItem = -1; shopSelectedItemName = ""; }
            }
            if (h5 && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
            {
                SellItem(shopSelectedItemName, Math.Min(5, count), price);
                if (GetItemCount(shopSelectedItemName) <= 0) { shopSelectedItem = -1; shopSelectedItemName = ""; }
            }
            if (hA && Raylib.IsMouseButtonPressed(MouseButton.Left) && count >= 1)
            {
                SellItem(shopSelectedItemName, count, price);
                shopSelectedItem = -1;
                shopSelectedItemName = "";
            }
        }
    }
    else
    {
        // ── BUY MODE — shop stock, click to buy ──
        string[] buyItems   = { "Arrows", "Bolts", "Apple", "Bread", "Bandage", "Torch" };
        int[]    buyPrices  = { 10,       12,      4,       6,       15,        8 };
        int[]    buyQty     = { 20,       20,      1,       1,       1,         1 };
        string[] buyDesc    = { "x20",    "x20",   "+10 HP","+20 HP","+40 HP",  "Light" };

        Program.DrawTextUI("SHOP STOCK — click to buy", panelX + 20, panelY + 62, 18, Color.LightGray);

        for (int i = 0; i < buyItems.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int sx = panelX + 20 + col * (slotSize + padding);
            int sy = panelY + 90 + row * (slotSize + padding);

            Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

            DrawInventoryIcon(buyItems[i], sx, sy, slotSize);
            Program.DrawTextUI($"${buyPrices[i]}", sx + 6, sy + 6, 16, Color.Gold);
            Program.DrawTextUI(buyDesc[i], sx + 4, sy + slotSize - 18, 12, Color.LightGray);

            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
            {
                Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    BuyShopItem(buyItems[i], buyPrices[i], buyQty[i]);
            }
        }

        Program.DrawTextUI($"Arrows: {player.Arrows}   Bolts: {player.Bolts}", panelX + 20, panelY + 320, 18, Color.LightGray);
    }

    Program.DrawTextUI("Q = Close Shop", panelX + 350, panelY + 528, 20, Color.LightGray);
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
    Program.DrawTextUI("McDONALD'S", panelX + 240, panelY + 10, 32, new Color((byte)255,(byte)220,(byte)0,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);

    if (mcdonaldsOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80,
            new Color((byte)0,(byte)160,(byte)0,(byte)255));
        Program.DrawTextUI($" {mcdonaldsOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
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

        Program.DrawTextUI(items[i], bx + 12, by + 10, 22,
            canAfford ? Color.White : Color.DarkGray);
        Program.DrawTextUI($"${prices[i]}", bx + 260, by + 10, 22,
            canAfford ? Color.Gold : Color.DarkGray);
        Program.DrawTextUI(descriptions[i], bx + 12, by + 40, 15,
            canAfford ? Color.LightGray : Color.DarkGray);

        if (!canAfford)
            Program.DrawTextUI("Not enough $", bx + 12, by + 90, 14, Color.Red);

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

    Program.DrawTextUI("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
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
    Program.DrawTextUI("DOMINO'S", panelX + 260, panelY + 10, 32, new Color((byte)190,(byte)20,(byte)20,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (dominosOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Program.DrawTextUI($"🍕 {dominosOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
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
 
        Program.DrawTextUI(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White    : Color.DarkGray);
        Program.DrawTextUI($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Red     : Color.DarkGray);
        Program.DrawTextUI(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray : Color.DarkGray);
        if (!canAfford) Program.DrawTextUI("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
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
    Program.DrawTextUI("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
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
    Program.DrawTextUI("KFC", panelX + 310, panelY + 10, 32, new Color((byte)240,(byte)225,(byte)195,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (kfcOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Program.DrawTextUI($"🍗 {kfcOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
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
 
        Program.DrawTextUI(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White      : Color.DarkGray);
        Program.DrawTextUI($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Gold      : Color.DarkGray);
        Program.DrawTextUI(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray  : Color.DarkGray);
        if (!canAfford) Program.DrawTextUI("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
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
    Program.DrawTextUI("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
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
    Program.DrawTextUI("BURGER KING", panelX + 210, panelY + 10, 32, new Color((byte)255,(byte)180,(byte)0,(byte)255));
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + 20, panelY + 58, 20, Color.Gold);
 
    if (burgerKingOrderReady)
    {
        Raylib.DrawRectangle(panelX + 100, panelY + 200, 500, 80, new Color((byte)0,(byte)130,(byte)0,(byte)255));
        Program.DrawTextUI($"🍔 {burgerKingOrderName} is ready!", panelX + 120, panelY + 225, 26, Color.White);
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
 
        Program.DrawTextUI(items[i],        bx + 12, by + 10, 22, canAfford ? Color.White     : Color.DarkGray);
        Program.DrawTextUI($"${prices[i]}", bx + 260, by + 10, 22, canAfford ? Color.Gold     : Color.DarkGray);
        Program.DrawTextUI(descriptions[i], bx + 12, by + 40, 15, canAfford ? Color.LightGray : Color.DarkGray);
        if (!canAfford) Program.DrawTextUI("Not enough $", bx + 12, by + 90, 14, Color.Red);
 
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
    Program.DrawTextUI("Q = Close", panelX + 300, panelY + 500, 20, Color.LightGray);
}

static void DrawMasteryShopUI()
{
    if (!masteryShopOpen || currentBuilding?.BuildingName != "CASTLE") return;

    int px = ScreenWidth / 2 - 440;
    int py = 40;
    int pw = 880;
    int ph = 620;
    Color accent = new Color((byte)200,(byte)160,(byte)255,(byte)255);
    Color gold = new Color((byte)220,(byte)180,(byte)40,(byte)255);

    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)12,(byte)8,(byte)20,(byte)248));
    Raylib.DrawRectangleLines(px, py, pw, ph, accent);
    Program.DrawTextUI("HALL OF MASTERY", px + pw / 2 - 100, py + 12, 28, accent);
    Program.DrawTextUI("Reach Lv 100 in any skill to claim its legendary cape & crown — FREE",
        px + 20, py + 46, 15, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // scroll
    int visibleRows = 7;
    int rowH = 74;
    int contentTop = py + 70;
    int maxScroll = Math.Max(0, masteryItems.Length - visibleRows);
    float wheel = Raylib.GetMouseWheelMove();
    if (wheel != 0) masteryShopScroll = Math.Clamp(masteryShopScroll - (int)wheel, 0, maxScroll);

    Raylib.BeginScissorMode(px, contentTop, pw, visibleRows * rowH);

    for (int i = 0; i < masteryItems.Length; i++)
    {
        var m = masteryItems[i];
        int ry = contentTop + (i - masteryShopScroll) * rowH;
        if (ry < contentTop - rowH || ry > contentTop + visibleRows * rowH) continue;

        int lvl = m.GetLevel();
        bool mastered = lvl >= 100;
        bool capeClaimed = player.OwnedGear.Contains(m.Cape);
        bool crownClaimed = player.OwnedGear.Contains(m.Headpiece);

        // row background
        Color rowBg = mastered
            ? new Color((byte)30,(byte)20,(byte)45,(byte)200)
            : new Color((byte)20,(byte)20,(byte)25,(byte)200);
        Raylib.DrawRectangle(px + 10, ry, pw - 20, rowH - 4, rowBg);
        Raylib.DrawRectangleLinesEx(new Rectangle(px + 10, ry, pw - 20, rowH - 4), 1,
            mastered ? accent : new Color((byte)50,(byte)50,(byte)60,(byte)255));

        // colour swatch
        Raylib.DrawRectangle(px + 18, ry + 8, 8, rowH - 20, m.ThemeA);
        Raylib.DrawRectangle(px + 26, ry + 8, 8, rowH - 20, m.ThemeB);

        // mannequin preview
        DrawMasteryPreview(px + 44, ry + 2, m.ThemeA, m.ThemeB, m.Headpiece, mastered);

        // skill name + level — was px + 44, now shifted right for mannequin
        Program.DrawTextUI(m.Skill, px + 100, ry + 6, 18,
            mastered ? Color.White : Color.DarkGray);
        Program.DrawTextUI($"Lv {lvl}/100", px + 100, ry + 28, 14,
            mastered ? gold : new Color((byte)100,(byte)100,(byte)100,(byte)255));

        // progress bar — was px + 44, now shifted
        Raylib.DrawRectangle(px + 100, ry + 48, 120, 8, new Color((byte)30,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangle(px + 100, ry + 48, (int)(120 * Math.Min(1f, lvl / 100f)), 8,
            mastered ? accent : new Color((byte)80,(byte)80,(byte)100,(byte)255));

        // cape
        int capeX = px + 320;
        Program.DrawTextUI(m.Cape, capeX, ry + 6, 15,
            mastered ? m.ThemeA : new Color((byte)60,(byte)60,(byte)60,(byte)255));
        if (mastered && !capeClaimed)
        {
            Rectangle claimCape = new Rectangle(capeX, ry + 28, 100, 26);
            bool hCape = Raylib.CheckCollisionPointRec(mouse, claimCape);
            Raylib.DrawRectangleRec(claimCape, hCape ? gold : new Color((byte)60,(byte)50,(byte)20,(byte)255));
            Raylib.DrawRectangleLinesEx(claimCape, 1, gold);
            Program.DrawTextUI("CLAIM", (int)claimCape.X + 28, (int)claimCape.Y + 5, 16,
                hCape ? Color.Black : gold);
            if (hCape && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                player.OwnedGear.Add(m.Cape);
                ShowNotification($"Claimed {m.Cape}! Equip it from your wardrobe.");
            }
        }
        else if (capeClaimed)
            Program.DrawTextUI("OWNED", capeX, ry + 32, 14, new Color((byte)80,(byte)220,(byte)80,(byte)255));
        else
            Program.DrawTextUI("LOCKED", capeX, ry + 32, 14, new Color((byte)120,(byte)50,(byte)50,(byte)255));

        // headpiece
        int crownX = px + 620;
        Program.DrawTextUI(m.Headpiece, crownX, ry + 6, 15,
            mastered ? m.ThemeB : new Color((byte)60,(byte)60,(byte)60,(byte)255));
        if (mastered && !crownClaimed)
        {
            Rectangle claimCrown = new Rectangle(crownX, ry + 28, 100, 26);
            bool hCrown = Raylib.CheckCollisionPointRec(mouse, claimCrown);
            Raylib.DrawRectangleRec(claimCrown, hCrown ? gold : new Color((byte)60,(byte)50,(byte)20,(byte)255));
            Raylib.DrawRectangleLinesEx(claimCrown, 1, gold);
            Program.DrawTextUI("CLAIM", (int)claimCrown.X + 28, (int)claimCrown.Y + 5, 16,
                hCrown ? Color.Black : gold);
            if (hCrown && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                player.OwnedGear.Add(m.Headpiece);
                ShowNotification($"Claimed {m.Headpiece}! Equip it from your wardrobe.");
            }
        }
        else if (crownClaimed)
            Program.DrawTextUI("OWNED", crownX, ry + 32, 14, new Color((byte)80,(byte)220,(byte)80,(byte)255));
        else
            Program.DrawTextUI("LOCKED", crownX, ry + 32, 14, new Color((byte)120,(byte)50,(byte)50,(byte)255));
    }

    Raylib.EndScissorMode();

    // scroll indicator
    if (maxScroll > 0)
    {
        float barH = Math.Max(30f, (visibleRows * rowH) * (visibleRows / (float)masteryItems.Length));
        float barY = contentTop + ((float)masteryShopScroll / maxScroll) * (visibleRows * rowH - barH);
        Raylib.DrawRectangle(px + pw - 14, contentTop, 8, visibleRows * rowH, new Color((byte)30,(byte)30,(byte)30,(byte)200));
        Raylib.DrawRectangle(px + pw - 14, (int)barY, 8, (int)barH, accent);
    }

    // mastered count
    int masteredCount = masteryItems.Count(m => m.GetLevel() >= 100);
    Program.DrawTextUI($"Skills Mastered: {masteredCount}/{masteryItems.Length}",
        px + 20, py + ph - 24, 16, gold);

    Program.DrawTextUI("Q = Close", px + pw - 90, py + ph - 24, 16, Color.DarkGray);
}

static void DrawMasteryPreview(int x, int y, Color capeCol, Color crownCol, string skill, bool mastered)
{
    // mini mannequin: 40px wide, 70px tall
    byte alpha = mastered ? (byte)255 : (byte)90;
    Color skin = new Color((byte)220,(byte)190,(byte)160, alpha);
    Color body = new Color((byte)60,(byte)60,(byte)70, alpha);
    Color cA = new Color(capeCol.R, capeCol.G, capeCol.B, alpha);
    Color cB = new Color(crownCol.R, crownCol.G, crownCol.B, alpha);

    // cape (behind body)
    Raylib.DrawRectangle(x + 10, y + 26, 20, 38, cA);
    // shimmer stripe on cape
    if (mastered)
        Raylib.DrawRectangle(x + 18, y + 28, 4, 34, new Color(
            (byte)Math.Min(255, cA.R + 50), (byte)Math.Min(255, cA.G + 50),
            (byte)Math.Min(255, cA.B + 50), (byte)140));

    // body
    Raylib.DrawRectangle(x + 12, y + 24, 16, 26, body);
    // arms
    Raylib.DrawRectangle(x + 6, y + 26, 6, 16, skin);
    Raylib.DrawRectangle(x + 28, y + 26, 6, 16, skin);
    // legs
    Raylib.DrawRectangle(x + 13, y + 50, 6, 14, body);
    Raylib.DrawRectangle(x + 21, y + 50, 6, 14, body);
    // head
    Raylib.DrawCircle(x + 20, y + 14, 10, skin);

    // ── HEADPIECE — unique per skill type ──
    if (skill.Contains("Crown") || skill.Contains("Wreath"))
    {
        // crown: base band + 3 points
        Raylib.DrawRectangle(x + 10, y + 5, 20, 6, cB);
        Raylib.DrawTriangle(new Vector2(x + 12, y + 5), new Vector2(x + 16, y + 5),
            new Vector2(x + 14, y - 2), cB);
        Raylib.DrawTriangle(new Vector2(x + 18, y + 5), new Vector2(x + 22, y + 5),
            new Vector2(x + 20, y - 4), cB);
        Raylib.DrawTriangle(new Vector2(x + 24, y + 5), new Vector2(x + 28, y + 5),
            new Vector2(x + 26, y - 2), cB);
        // gem on centre point
        if (mastered)
            Raylib.DrawCircle(x + 20, y - 1, 2, new Color((byte)255,(byte)255,(byte)200,(byte)255));
    }
    else if (skill.Contains("Hat"))
    {
        // top hat / chef hat: tall rectangle + brim
        Raylib.DrawRectangle(x + 12, y - 6, 16, 14, cB);
        Raylib.DrawRectangle(x + 8, y + 5, 24, 4, cB);
        if (mastered)
            Raylib.DrawRectangle(x + 12, y - 4, 16, 3, new Color(
                (byte)Math.Min(255, cB.R + 40), (byte)Math.Min(255, cB.G + 40),
                (byte)Math.Min(255, cB.B + 40), (byte)200));
    }
    else if (skill.Contains("Hood") || skill.Contains("Helm"))
    {
        // hood/helmet: rounded cap covering top of head
        Raylib.DrawCircle(x + 20, y + 8, 12, cB);
        Raylib.DrawRectangle(x + 8, y + 8, 24, 6, cB);
        // visor slit for helmets
        if (skill.Contains("Helm"))
            Raylib.DrawRectangle(x + 12, y + 10, 16, 3, new Color((byte)20,(byte)20,(byte)20,(byte)200));
    }
    else if (skill.Contains("Mortarboard"))
    {
        // academic cap
        Raylib.DrawRectangle(x + 6, y + 2, 28, 4, cB);
        Raylib.DrawRectangle(x + 14, y + 4, 12, 5, cB);
        // tassel
        Raylib.DrawLine(x + 8, y + 2, x + 4, y + 12, new Color((byte)220,(byte)180,(byte)40,(byte)255));
        Raylib.DrawCircle(x + 4, y + 13, 2, new Color((byte)220,(byte)180,(byte)40,(byte)255));
    }
    else
    {
        // default crown
        Raylib.DrawRectangle(x + 10, y + 5, 20, 6, cB);
        Raylib.DrawTriangle(new Vector2(x + 14, y + 5), new Vector2(x + 18, y + 5),
            new Vector2(x + 16, y - 2), cB);
        Raylib.DrawTriangle(new Vector2(x + 22, y + 5), new Vector2(x + 26, y + 5),
            new Vector2(x + 24, y - 2), cB);
    }

    // mastery glow ring
    if (mastered)
        Raylib.DrawCircleLines(x + 20, y + 32, 30, new Color(
            cA.R, cA.G, cA.B, (byte)(80 + (int)(40 * MathF.Sin((float)Raylib.GetTime() * 3f)))));
}

static void DrawFarmingShopUI()
{
    if (!farmingShopOpen) return;

    int px = ScreenWidth/2 - 320, py = 60, pw = 640, ph = 500;
    Color brown = new Color((byte)140,(byte)100,(byte)40,(byte)255);
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)25,(byte)18,(byte)10,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, brown);
    Program.DrawTextUI("FARMING SUPPLIES", px + 200, py + 12, 26, brown);
    Program.DrawTextUI($"${player.Money}", px + pw - 90, py + 16, 20, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    (string name, int price, bool isTool)[] stock =
    {
        ("Spade", 25, true),
        ("Watering Can", 25, true),
        ("Wheat Seeds", 5, false),
        ("Carrot Seeds", 6, false),
        ("Potato Seeds", 6, false),
        ("Tomato Seeds", 7, false),
        ("Apple Tree Seed",  15, false),
        ("Banana Tree Seed",  15, false),
    };

    int rowH = 64, startY = py + 60;
    for (int i = 0; i < stock.Length; i++)
{
    var (name, price, isTool) = stock[i];
    int reqLv = 0;
    bool locked = !isTool && seedUnlockLevel.TryGetValue(name, out reqLv) && player.FarmingLevel < reqLv;
    Rectangle row = new Rectangle(px + 20, startY + i * rowH, pw - 40, rowH - 8);
    bool hover = !locked && Raylib.CheckCollisionPointRec(mouse, row);
    bool alreadyOwned = isTool && ToolInToolbar(name);

    int finalPrice = DiscountedPrice(price);

    bool canAfford = player.Money >= price;

    Raylib.DrawRectangleRec(row, new Color((byte)35,(byte)25,(byte)15,(byte)255));
    Raylib.DrawRectangleLinesEx(row, hover ? 2 : 1, hover ? brown : new Color((byte)70,(byte)55,(byte)30,(byte)255));
    Program.DrawTextUI(name, px + 32, startY + i * rowH + 10, 20, locked ? Color.DarkGray : (alreadyOwned ? Color.Gray : Color.White));
    string rightLabel = locked ? $"Lv {reqLv} req." : alreadyOwned ? "OWNED" : $"${price}";
    Program.DrawTextUI(rightLabel, px + pw - 110, startY + i * rowH + 12, 16, locked ? Color.Red : (alreadyOwned ? Color.Gray : (canAfford ? Color.Gold : Color.Red)));

    // season tag for seeds
    if (!isTool && seedToCrop.TryGetValue(name, out string cropName))
    {
        bool inSeason = IsCropInSeason(cropName);
        Program.DrawTextUI(inSeason ? "IN SEASON" : "OFF SEASON", px + 32, startY + i * rowH + 34, 12,
            inSeason ? new Color((byte)80,(byte)200,(byte)80,(byte)255) : new Color((byte)200,(byte)120,(byte)40,(byte)200));
    }

    if (hover && !alreadyOwned && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (!canAfford)
        {
            ShowNotification($"Need ${finalPrice}!");
        }
        else if (TryGiveItem(name, 1))
        {
            if (name == "Spade") spadePickedUp = true;
            if (name == "Watering Can") wateringCanPickedUp = true;
            player.Money -= price;
            ShowNotification($"Bought {name}!");
        }
        else
        {
            ShowNotification("Inventory full!");
        }
    }
}

    Program.DrawTextUI("Q = Close", px + pw/2 - 38, py + ph - 24, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) farmingShopOpen = false;
}

static void DrawMagicShopUI()
{
    if (!magicShopOpen || currentBuilding?.BuildingName != "MAGIC SHOP") return;

    int panelX = ScreenWidth / 2 - 400;
    int panelY = 50;
    int panelW = 800;
    int panelH = 580;
    Color purple = new Color((byte)120,(byte)40,(byte)200,(byte)255);

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)15,(byte)5,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, purple);
    Program.DrawTextUI("MAGIC SHOP", panelX + 290, panelY + 12, 28, purple);
    Program.DrawTextUI($"Wallet: ${player.Money}   Arcane Essence: {player.ArcaneEssence}",
        panelX + 20, panelY + 46, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // staffs
    (string name, int price, string spell, Color col)[] staffs =
    {
        ("Rock Staff",      80,  "Rock",      new Color((byte)120,(byte)90,(byte)60,(byte)255)),
        ("Wind Staff",      100, "Wind",      new Color((byte)180,(byte)230,(byte)180,(byte)255)),
        ("Water Staff",     130, "Water",     new Color((byte)40,(byte)120,(byte)220,(byte)255)),
        ("Lightning Staff", 170, "Lightning", new Color((byte)240,(byte)220,(byte)60,(byte)255)),
        ("Fire Staff",      220, "Fire",      new Color((byte)240,(byte)80,(byte)20,(byte)255)),
        ("Ice Staff",       280, "Ice",       new Color((byte)160,(byte)220,(byte)255,(byte)255)),
        ("Light Staff",     400, "Light",     new Color((byte)255,(byte)255,(byte)200,(byte)255)),
        ("Dark Staff",      400, "Dark",      new Color((byte)80,(byte)0,(byte)120,(byte)255)),
    };

    Program.DrawTextUI("STAFFS", panelX + 20, panelY + 80, 20, purple);

    for (int i = 0; i < staffs.Length; i++)
    {
        int col = i % 4;
        int row = i / 4;
        int sx = panelX + 20 + col * 190;
        int sy = panelY + 110 + row * 120;

        bool owned  = player.OwnedGear.Contains(staffs[i].name);
        bool hover  = Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, 175, 100));

        Raylib.DrawRectangle(sx, sy, 175, 100, new Color((byte)25,(byte)10,(byte)50,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, 175, 100,
            owned ? staffs[i].col : (hover ? staffs[i].col : Color.DarkGray));

        // ... orb preview unchanged ...
        int finalStaffPrice = DiscountedPrice(staffs[i].price);

        Program.DrawTextUI(owned ? "OWNED" : $"${finalStaffPrice}",
            sx + 10, sy + 62, 14, owned ? Color.Green : Color.White);

        // animated spell orb preview
        float pulse = 1f + MathF.Sin((float)Raylib.GetTime() * 4f + i) * 0.3f;
        Raylib.DrawCircle(sx + 20, sy + 30, (int)(8 * pulse), staffs[i].col);
        Raylib.DrawCircle(sx + 20, sy + 30, 4, Color.White);

        Program.DrawTextUI(staffs[i].name, sx + 36, sy + 22, 14, staffs[i].col);
        Program.DrawTextUI($"Spell: {staffs[i].spell}", sx + 10, sy + 44, 13, Color.LightGray);
    
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (owned)
            {
                shopMessage = $"You already own the {staffs[i].name}. Equip it from your inventory.";
                shopMessageTimer = 2f;
            }
            else if (player.Money >= staffs[i].price)
            {
                player.Money -= staffs[i].price;
                player.OwnedGear.Add(staffs[i].name);   // goes to inventory, NOT equipped
                shopMessage = $"Bought {staffs[i].name}! Equip it from your inventory.";
                shopMessageTimer = 2f;
            }
            else
            {
                shopMessage = $"Need ${finalStaffPrice}!";
                shopMessageTimer = 1.5f;
            }
        }
    }

    // Arcane Essence
    Program.DrawTextUI("ARCANE ESSENCE", panelX + 20, panelY + 360, 20, purple);

    (int qty, int price)[] essencePacks = { (10, 20), (50, 90), (100, 160) };
    for (int i = 0; i < essencePacks.Length; i++)
    {
        int ex = panelX + 20 + i * 200;
        int ey = panelY + 390;
        bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ex, ey, 180, 70));

        int finalEssencePrice = DiscountedPrice(essencePacks[i].price);

        Raylib.DrawRectangle(ex, ey, 180, 70, new Color((byte)25,(byte)10,(byte)50,(byte)255));
        Raylib.DrawRectangleLines(ex, ey, 180, 70, hover ? Color.Gold : purple);

        // pulsing essence orb
        float pulse = 1f + MathF.Sin((float)Raylib.GetTime() * 3f + i) * 0.25f;
        Raylib.DrawCircle(ex + 20, ey + 35, (int)(7 * pulse), new Color((byte)160,(byte)80,(byte)255,(byte)255));
        Raylib.DrawCircle(ex + 20, ey + 35, 3, Color.White);

        Program.DrawTextUI($"x{essencePacks[i].qty} Essence", ex + 36, ey + 12, 16, Color.LightGray);
        Program.DrawTextUI($"${finalEssencePrice}", ex + 36, ey + 38, 18, Color.Gold);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            bool canAfford = player.Money >= essencePacks[i].price;
            if (!canAfford)
            {
                shopMessage = $"Need ${finalEssencePrice}!";
                shopMessageTimer = 1.5f;
            }
            else if (TryGiveItem("Arcane Essence", essencePacks[i].qty))
            {
                player.Money -= essencePacks[i].price;
                shopMessage = $"Bought {essencePacks[i].qty} Arcane Essence!";
                shopMessageTimer = 1.5f;
            }
            else
            {
                shopMessage = "Inventory full!";
                shopMessageTimer = 1.5f;
            }
        }
    }

    Program.DrawTextUI("Q = Close", panelX + 360, panelY + 548, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) magicShopOpen = false;
}

static void DrawRangingShopUI()
{
    if (!rangingShopOpen || currentBuilding?.BuildingName != "RANGING SHOP") return;

    int panelX = ScreenWidth / 2 - 400;
    int panelY = 50;
    int panelW = 800;
    int panelH = 580;
    Color tan = new Color((byte)200,(byte)150,(byte)40,(byte)255);

    Raylib.DrawRectangle(panelX, panelY, panelW, panelH, new Color((byte)20,(byte)12,(byte)4,(byte)245));
    Raylib.DrawRectangleLines(panelX, panelY, panelW, panelH, tan);
    Program.DrawTextUI("FLETCHER'S RANGING SHOP", panelX + 200, panelY + 12, 28, tan);
    Program.DrawTextUI($"Wallet: ${player.Money}   Arrows: {player.Arrows}   Bolts: {player.Bolts}",
        panelX + 20, panelY + 46, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // ── RANGED WEAPONS ──
    Program.DrawTextUI("RANGED WEAPONS", panelX + 20, panelY + 80, 20, tan);

    (string name, int price, string desc, Color col, bool owned)[] weapons =
    {
        ("Bow",            150, "Fast fire rate\n12 dmg/shot",  new Color((byte)160,(byte)110,(byte)40,(byte)255), player.HasBow),
        ("Crossbow",       280, "Slow, heavy hit\n18 dmg/shot", new Color((byte)80,(byte)55,(byte)20,(byte)255),  player.HasCrossbow),
    };

    for (int i = 0; i < weapons.Length; i++)
    {
        int wx = panelX + 20 + i * 220;
        int wy = panelY + 110;
        bool owned = weapons[i].owned;
        bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(wx, wy, 200, 120));
        bool canAfford = player.Money >= weapons[i].price;

        Raylib.DrawRectangle(wx, wy, 200, 120, new Color((byte)30,(byte)18,(byte)6,(byte)255));
        Raylib.DrawRectangleLines(wx, wy, 200, 120, owned ? weapons[i].col : (hover ? tan : Color.DarkGray));

        // bow icon
        Raylib.DrawCircleLines(wx + 30, wy + 60, 22, weapons[i].col);
        Raylib.DrawRectangle(wx + 29, wy + 38, 4, 44, new Color((byte)30,(byte)18,(byte)6,(byte)255));
        Raylib.DrawLine(wx + 30, wy + 38, wx + 30, wy + 82, new Color((byte)200,(byte)185,(byte)140,(byte)255));

        Program.DrawTextUI(weapons[i].name, wx + 60, wy + 10, 18, weapons[i].col);
        Program.DrawTextUI(weapons[i].desc, wx + 60, wy + 34, 14, Color.LightGray);

        if (owned)
        {
            Program.DrawTextUI("OWNED", wx + 60, wy + 90, 16, Color.Green);
        }
        else
        {
            Program.DrawTextUI($"${weapons[i].price}", wx + 60, wy + 90, 18, canAfford ? Color.Gold : Color.Red);
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (!canAfford)
    {
        ShowNotification($"Need ${weapons[i].price}!");
    }
    else
    {
        bool added = weapons[i].name == "Bow" ? BackpackAdd("Bow", 1) : BackpackAdd("Crossbow", 1);
        if (added)
        {
            player.Money -= weapons[i].price;
            if (weapons[i].name == "Bow") player.HasBow = true;
            if (weapons[i].name == "Crossbow") player.HasCrossbow = true;
            ShowNotification($"Bought {weapons[i].name}!");
        }
        else
        {
            ShowNotification("Inventory full!");
        }
    }
}
        }
    }

    // ── AMMO ──
    Program.DrawTextUI("AMMO", panelX + 20, panelY + 250, 20, tan);
    Raylib.DrawRectangle(panelX + 10, panelY + 246, panelW - 20, 1, new Color((byte)80,(byte)55,(byte)20,(byte)255));

    (string name, int price, int qty, Color col)[] ammo =
    {
        ("Arrows", 10,  20, new Color((byte)160,(byte)120,(byte)40,(byte)255)),
        ("Bolts",  14,  20, new Color((byte)80,(byte)120,(byte)180,(byte)255)),
        ("Arrows", 22,  50, new Color((byte)160,(byte)120,(byte)40,(byte)255)),
        ("Bolts",  30,  50, new Color((byte)80,(byte)120,(byte)180,(byte)255)),
        ("Arrows", 80, 200, new Color((byte)160,(byte)120,(byte)40,(byte)255)),
        ("Bolts",  110,200, new Color((byte)80,(byte)120,(byte)180,(byte)255)),
    };

    for (int i = 0; i < ammo.Length; i++)
    {
        int col = i % 3;
        int row = i / 3;
        int ax = panelX + 20 + col * 250;
        int ay = panelY + 278 + row * 100;
        bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ax, ay, 230, 80));
        bool canAfford = player.Money >= ammo[i].price;

        Raylib.DrawRectangle(ax, ay, 230, 80, new Color((byte)28,(byte)16,(byte)5,(byte)255));
        Raylib.DrawRectangleLines(ax, ay, 230, 80, hover ? tan : Color.DarkGray);

        // arrow bundle icon
        for (int ar = 0; ar < 5; ar++)
            Raylib.DrawLine(ax + 14 + ar * 7, ay + 12, ax + 14 + ar * 7, ay + 54, ammo[i].col);
        Raylib.DrawRectangle(ax + 10, ay + 50, 44, 4, new Color((byte)100,(byte)65,(byte)20,(byte)255));

        Program.DrawTextUI($"{ammo[i].name} x{ammo[i].qty}", ax + 64, ay + 10, 16, ammo[i].col);
        Program.DrawTextUI($"${ammo[i].price}", ax + 64, ay + 34, 18, canAfford ? Color.Gold : Color.Red);
        Program.DrawTextUI(canAfford ? "Click to buy" : "Not enough $", ax + 64, ay + 56, 13,
            canAfford ? Color.LightGray : Color.Red);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!canAfford)
            {
                ShowNotification($"Need ${ammo[i].price}!");
            }
            else if (TryGiveItem(ammo[i].name, ammo[i].qty))
            {
                player.Money -= ammo[i].price;
                ShowNotification($"Bought {ammo[i].qty} {ammo[i].name}!");
            }
            else
            {
                ShowNotification("Inventory full!");
            }
        }
    }

    // ── SELL SECTION ──
    Program.DrawTextUI("SELL", panelX + 20, panelY + 490, 20, tan);
    Raylib.DrawRectangle(panelX + 10, panelY + 486, panelW - 20, 1, new Color((byte)80,(byte)55,(byte)20,(byte)255));

    (string name, int sellPer, int count)[] sellable =
    {
        ("Arrows", 4, player.Arrows),
        ("Bolts",  5, player.Bolts),
    };
    for (int i = 0; i < sellable.Length; i++)
    {
        int sx = panelX + 20 + i * 200;
        int sy = panelY + 514;
        bool hover = sellable[i].count > 0 && Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, 180, 48));

        Raylib.DrawRectangle(sx, sy, 180, 48, new Color((byte)28,(byte)16,(byte)5,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, 180, 48, hover ? Color.Gold : Color.DarkGray);
        Program.DrawTextUI($"Sell all {sellable[i].name}", sx + 10, sy + 6, 15, Color.LightGray);
        Program.DrawTextUI($"{sellable[i].count} x ${sellable[i].sellPer} = ${sellable[i].count * sellable[i].sellPer}",
            sx + 10, sy + 26, 14, sellable[i].count > 0 ? Color.Gold : Color.DarkGray);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            int earned = sellable[i].count * sellable[i].sellPer;
            player.Money += earned;
            if (sellable[i].name == "Arrows") player.Arrows = 0;
            else                              player.Bolts  = 0;
            ShowNotification($"Sold all {sellable[i].name} for ${earned}!");
        }
    }

    Program.DrawTextUI("Q = Close", panelX + 360, panelY + 548, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) rangingShopOpen = false;
}

static void DrawLandForSaleUI()
{
    if (!landForSaleUIOpen || selectedPlot < 0) return;
    var plot = landPlots[selectedPlot];

    int pw = 600; int ph = 340;
    int px2 = ScreenWidth / 2 - pw / 2;
    int py2 = ScreenHeight / 2 - ph / 2;
    Color gold = new Color((byte)220,(byte)180,(byte)40,(byte)255);

    Raylib.DrawRectangle(px2, py2, pw, ph, new Color((byte)10,(byte)10,(byte)20,(byte)245));
    Raylib.DrawRectangleLines(px2, py2, pw, ph, gold);
    Program.DrawTextUI("LAND FOR SALE", px2 + 170, py2 + 14, 30, gold);
    Raylib.DrawRectangle(px2 + 20, py2 + 50, pw - 40, 2, new Color((byte)80,(byte)60,(byte)10,(byte)255));

    Program.DrawTextUI(plot.label,    px2 + 30, py2 + 64,  22, Color.White);

    // CHANGED — tagline + perks now vary per house type
    string tagline = plot.houseType switch
    {
        "Farmhouse"  => "Rural build — comes with working stable!",
        "OceanHouse" => "Beachfront build — private dock & dolphin pen!",
        "Mansion"    => "Luxury estate — the biggest build in the game!",
        _            => "240 x 180 plot — build your dream home!"
    };
    Program.DrawTextUI(tagline, px2 + 30, py2 + 94, 18, Color.LightGray);
    Program.DrawTextUI("Includes:", px2 + 30, py2 + 124, 18, Color.LightGray);

    string[] perks = plot.houseType switch
    {
        "Farmhouse" => new[] {
            "  • 1-car garage",
            "  • Stable (horse, donkey or camel)",
            "  • Fully customisable interior",
            "  • Mailbox, chest, bed & save point",
        },
        "OceanHouse" => new[] {
            "  • 2-car garage + bike rack",
            "  • Boat dock storage",
            "  • Aquatic pen for a dolphin",
            "  • Mailbox, chest, bed & save point",
        },
        "Mansion" => new[] {
            "  • 3-car garage",
            "  • Advanced stable (any land animal, 2 slots)",
            "  • Fully customisable interior",
            "  • Mailbox, chest, bed & save point",
        },
        _ => new[] {
            "  • Fully customisable interior",
            "  • Buy furniture from any store",
            "  • Change wall, roof & floor colours",
            "  • Chest, bed & save point included",
        }
    };
    for (int i = 0; i < perks.Length; i++)
        Program.DrawTextUI(perks[i], px2 + 30, py2 + 148 + i * 22, 17, Color.LightGray);

    Program.DrawTextUI($"Price: ${plot.price}", px2 + 30, py2 + 248, 24, gold);
    Program.DrawTextUI($"Your wallet: ${player.Money}", px2 + 30, py2 + 278, 20,
        player.Money >= plot.price ? Color.Green : Color.Red);

    // Buy button
    bool canAfford = player.Money >= plot.price;
    Vector2 mouse = Raylib.GetMousePosition();
    Rectangle buyBtn = new Rectangle(px2 + pw - 200, py2 + ph - 60, 170, 44);
    bool hoverBuy = canAfford && Raylib.CheckCollisionPointRec(mouse, buyBtn);
    Raylib.DrawRectangleRec(buyBtn, canAfford
        ? new Color((byte)20,(byte)80,(byte)20,(byte)255)
        : new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangleLinesEx(buyBtn, 2, hoverBuy ? Color.Green : Color.Gray);
    Program.DrawTextUI(canAfford ? "BUY LAND" : "NOT ENOUGH $",
        (int)buyBtn.X + 16, (int)buyBtn.Y + 12, 20,
        hoverBuy ? Color.Green : Color.Gray);

    if (Raylib.IsMouseButtonPressed(MouseButton.Left) && hoverBuy)
    {
        player.Money -= plot.price;
        ownedHousePlots.Add(((int)plot.x, (int)plot.y));
        houseDataList.Add(new HouseData((int)plot.x, (int)plot.y) { HouseType = plot.houseType });
        activeHousePlotIndex = ownedHousePlots.Count - 1;
        SpawnStructuresForHouse(activeHousePlotIndex, plot.x, plot.y, plot.houseType); 
        landForSaleUIOpen    = false;
        SpawnPlayerHouse(activeHousePlotIndex);
        houseBuildingActive  = true;   // ← trigger animation
        houseBuildingTimer   = 0f;
        houseBuildingFadeIn  = true;
        houseBuildingAlpha   = 0f;
        ShowNotification($"Land purchased! Building your house at {plot.label}...");
    }

    Program.DrawTextUI("ESC = Close", px2 + 30, py2 + ph - 40, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape)) landForSaleUIOpen = false;
}

static void DrawChestUI()
{
    var chest = placedChests.FirstOrDefault(c => c.Id == openChestId);
    if (chest == null) return;

    int panelX = ScreenWidth / 2 - 420, panelY = 60;
    int slotSize = 70, padding = 8, cols = 5;

    Raylib.DrawRectangle(panelX, panelY, 840, 680, new Color((byte)20,(byte)20,(byte)30,(byte)240));
    Raylib.DrawRectangleLines(panelX, panelY, 840, 680, Color.Gold);
    Program.DrawTextUI($"CHEST (Tier {chest.Tier + 1}, {chest.UsedSlots}/{chest.Capacity})", panelX + 20, panelY + 15, 26, Color.Gold);
    Program.DrawTextUI("INVENTORY", panelX + 450, panelY + 15, 28, Color.Gold);
    Raylib.DrawRectangle(panelX + 415, panelY + 10, 4, 660, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    Vector2 mouse = Raylib.GetMousePosition();
    var chestStacks = chest.Contents.Where(kv => kv.Value > 0).ToList();

    Program.DrawTextUI("Click to withdraw", panelX + 20, panelY + 48, 16, Color.LightGray);
    for (int i = 0; i < chest.Capacity; i++)
    {
        int col = i % cols, row = i / cols;
        int sx = panelX + 20 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);
        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (i >= chestStacks.Count) continue;
        var (name, count) = (chestStacks[i].Key, chestStacks[i].Value);
        DrawInventoryIcon(name, sx, sy, slotSize);
        Program.DrawTextUI($"{count}", sx + 6, sy + 6, 16, Color.White);
        Program.DrawTextUI(name, sx + 4, sy + slotSize - 18, 13, Color.LightGray);

        if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
        {
            Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && TryGiveItem(name, 1))
                chest.RemoveOne(name);
        }
    }

    Program.DrawTextUI("Click to deposit", panelX + 450, panelY + 48, 16, Color.LightGray);
    var invStacks = GetAllPlayerItemNames();  // see helper below
    for (int i = 0; i < 20; i++)
    {
        int col = i % cols, row = i / cols;
        int sx = panelX + 435 + col * (slotSize + padding);
        int sy = panelY + 75 + row * (slotSize + padding);
        Raylib.DrawRectangle(sx, sy, slotSize, slotSize, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, new Color((byte)100,(byte)100,(byte)100,(byte)255));

        if (i >= invStacks.Count) continue;
        string name = invStacks[i];
        int count = GetItemCount(name);
        DrawInventoryIcon(name, sx, sy, slotSize);
        Program.DrawTextUI($"{count}", sx + 6, sy + 6, 16, Color.White);
        Program.DrawTextUI(name, sx + 4, sy + slotSize - 18, 13, Color.LightGray);

        if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(sx, sy, slotSize, slotSize)))
        {
            Raylib.DrawRectangleLines(sx, sy, slotSize, slotSize, Color.Gold);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (chest.TryAdd(name, 1)) RemoveOneItem(name);
                else { chestFullMessage = "Chest is full!"; chestFullTimer = 2.5f; }
            }
        }
    }

    if (chestFullTimer > 0)
    {
        chestFullTimer -= Raylib.GetFrameTime();
        int mw = Program.MeasureTextUI(chestFullMessage, 22);
        Raylib.DrawRectangle(panelX + 420 - mw/2 - 10, panelY + 620, mw + 20, 30, new Color((byte)80,(byte)20,(byte)20,(byte)230));
        Program.DrawTextUI(chestFullMessage, panelX + 420 - mw/2, panelY + 625, 22, Color.White);
    }

    Program.DrawTextUI("Q = Close Chest", panelX + 350, panelY + 656, 20, Color.LightGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) openChestId = null;
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
    Program.DrawTextUI(currentDealerBuilding.BuildingName, panelX + 20, panelY + 12, 28, Color.Gold);
    Program.DrawTextUI($"Wallet: ${player.Money}", panelX + panelW - 240, panelY + 18, 20, Color.LightGray);

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
Program.DrawTextUI(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

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
    Program.DrawTextUI(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

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
Program.DrawTextUI(proto.Type.ToString(), bx + 8, by + 8, 14, Color.White);

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
    Program.DrawTextUI("Empty", bx + 8, by + slotH / 2 - 8, 16, Color.DarkGray);
}

// draw price text centered below the preview area
string priceText = $"${price}";
int pw = Program.MeasureTextUI(priceText, 16);
int px = bx + (slotW - pw) / 2;
Program.DrawTextUI(priceText, px, by + slotH - 22, 16, Color.Gold);

    }

    // Scroll buttons placed directly under the option row, non-overlapping
    int scrollBtnSize = 44;
    int scrollY = slotY + slotH + 12;
    Rectangle leftBtn = new Rectangle(leftX, scrollY, scrollBtnSize, scrollBtnSize);
    Rectangle rightBtn = new Rectangle(leftX + leftColW - scrollBtnSize, scrollY, scrollBtnSize, scrollBtnSize);

    Raylib.DrawRectangleRec(leftBtn, new Color(40, 40, 40, 255));
    Raylib.DrawRectangleRec(rightBtn, new Color(40, 40, 40, 255));
    Program.DrawTextUI("<", (int)leftBtn.X + 12, (int)leftBtn.Y + 8, 28, Color.White);
    Program.DrawTextUI(">", (int)rightBtn.X + 12, (int)rightBtn.Y + 8, 28, Color.White);

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
    Program.DrawTextUI(title, rightX + 12, previewY + 12, 20, Color.Gold);
    for (int i = 0; i < stats.Length; i++)
        Program.DrawTextUI(stats[i], rightX + 12, statsY + 12 + i * 22, 16, Color.LightGray);

    // BUY button centered within stats area bottom
    Rectangle buyBtn = new Rectangle(rightX + previewW / 2 - 140, statsY + statsH - 64, 280, 44);
    bool hoverBuy = Raylib.CheckCollisionPointRec(mouse, buyBtn);
    Raylib.DrawRectangleRec(buyBtn, new Color(40, 40, 40, 255));
    Raylib.DrawRectangleLinesEx(buyBtn, 2, hoverBuy ? Color.Gold : Color.White);
    Program.DrawTextUI(hasSelection ? "BUY & SPAWN OUTSIDE" : "NO ITEM SELECTED", (int)buyBtn.X + 18, (int)buyBtn.Y + 10, 18, hasSelection ? (hoverBuy ? Color.Gold : Color.White) : Color.DarkGray);

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
    Program.DrawTextUI("Q = Close", panelX + panelW / 2 - 40, panelY + panelH - 28, 20, Color.LightGray);
}
    }
}
