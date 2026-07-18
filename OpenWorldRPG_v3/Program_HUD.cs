using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void DrawSkillCheatPanel()
        {
            if (!skillCheatOpen) return;

            int pw = 340;
            int px = ScreenWidth - pw - 20;
            int py = 60;
            int visibleH = ScreenHeight - py - 20; // fill screen height

            Vector2 mouse = Raylib.GetMousePosition();

            // calculate total content height
            int rowH = 30;
            int skillsH = cheatSkills.Length * rowH;
            int togglesH = 26 + 34 * 3 + 6; // header + 3 buttons
            int teleportHeaderH = 26;
            int teleportCount = 19;
            int teleportH = teleportCount * 26;
            int totalH = 40 + skillsH + 16 + togglesH + 40 + teleportHeaderH + teleportH + 20;

            float maxScroll = Math.Max(0, totalH - visibleH);

            // scroll with mouse wheel when hovering panel
            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(px, py, pw, visibleH)))
                cheatScrollY = Math.Clamp(cheatScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
            cheatScrollY = Math.Clamp(cheatScrollY, 0f, maxScroll);

            // panel background
            Raylib.DrawRectangle(px, py, pw, visibleH, new Color((byte)10, (byte)10, (byte)20, (byte)240));
            Raylib.DrawRectangleLines(px, py, pw, visibleH, Color.Lime);

            // scrollbar track
            if (maxScroll > 0)
            {
                int trackX = px + pw - 10;
                int trackH = visibleH - 4;
                Raylib.DrawRectangle(trackX, py + 2, 8, trackH, new Color((byte)30, (byte)30, (byte)40, (byte)255));
                float thumbRatio = (float)visibleH / totalH;
                int thumbH = Math.Max(20, (int)(trackH * thumbRatio));
                int thumbY = py + 2 + (int)((trackH - thumbH) * (cheatScrollY / maxScroll));
                Raylib.DrawRectangle(trackX, thumbY, 8, thumbH, Color.Lime);
            }

            // clip content
            Raylib.BeginScissorMode(px, py, pw, visibleH);

            int cy = py + 10 - (int)cheatScrollY;

            Program.DrawTextUI("DEBUG CHEATS (F8)", px + 10, cy, 18, Color.Lime);
            cy += 30;

            // ── SKILL CHEATS ──
            for (int i = 0; i < cheatSkills.Length; i++)
            {
                var s = cheatSkills[i];
                int ry = cy + i * rowH;

                Program.DrawTextUI($"{s.name}", px + 12, ry + 4, 16, Color.White);
                Program.DrawTextUI($"{s.get()}", px + 150, ry + 4, 16, Color.Gold);

                Rectangle minus = new Rectangle(px + 200, ry, 26, 24);
                bool hMinus = Raylib.CheckCollisionPointRec(mouse, minus);
                Raylib.DrawRectangleRec(minus, hMinus ? new Color((byte)120, (byte)40, (byte)40, (byte)255) : new Color((byte)60, (byte)30, (byte)30, (byte)255));
                Program.DrawTextUI("-", (int)minus.X + 9, (int)minus.Y + 3, 18, Color.White);

                Rectangle plus = new Rectangle(px + 234, ry, 26, 24);
                bool hPlus = Raylib.CheckCollisionPointRec(mouse, plus);
                Raylib.DrawRectangleRec(plus, hPlus ? new Color((byte)40, (byte)120, (byte)40, (byte)255) : new Color((byte)30, (byte)60, (byte)30, (byte)255));
                Program.DrawTextUI("+", (int)plus.X + 7, (int)plus.Y + 3, 18, Color.White);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (hMinus && s.get() > 1) s.set(s.get() - 1);
                    if (hPlus) s.set(s.get() + 1);
                    if (hMinus || hPlus) timesCheated++;
                }
            }
            cy += cheatSkills.Length * rowH + 16;

            // ── TOGGLES ──
            Program.DrawTextUI("─── TOGGLES ───", px + 10, cy, 16, Color.Lime);
            cy += 26;

            // Speed boost
            Rectangle speedBtn = new Rectangle(px + 10, cy, pw - 30, 28);
            bool hSpeed = Raylib.CheckCollisionPointRec(mouse, speedBtn);
            Raylib.DrawRectangleRec(speedBtn, hSpeed ? new Color((byte)40, (byte)60, (byte)40, (byte)255) : new Color((byte)30, (byte)30, (byte)40, (byte)255));
            Raylib.DrawRectangleLinesEx(speedBtn, 1, cheatSpeedBoost ? Color.Lime : Color.Gray);
            Program.DrawTextUI($"Speed Boost (2000): {(cheatSpeedBoost ? "ON" : "OFF")}", px + 18, cy + 5, 16,
                cheatSpeedBoost ? Color.Lime : Color.White);
            if (hSpeed && Raylib.IsMouseButtonPressed(MouseButton.Left))
                cheatSpeedBoost = !cheatSpeedBoost;
            cy += 34;

            // Noclip
            Rectangle noclipBtn = new Rectangle(px + 10, cy, pw - 30, 28);
            bool hNoclip = Raylib.CheckCollisionPointRec(mouse, noclipBtn);
            Raylib.DrawRectangleRec(noclipBtn, hNoclip ? new Color((byte)40, (byte)60, (byte)40, (byte)255) : new Color((byte)30, (byte)30, (byte)40, (byte)255));
            Raylib.DrawRectangleLinesEx(noclipBtn, 1, cheatNoclip ? Color.Lime : Color.Gray);
            Program.DrawTextUI($"Noclip (No Collision): {(cheatNoclip ? "ON" : "OFF")}", px + 18, cy + 5, 16,
                cheatNoclip ? Color.Lime : Color.White);
            if (hNoclip && Raylib.IsMouseButtonPressed(MouseButton.Left))
                cheatNoclip = !cheatNoclip;
            cy += 34;

            // Full heal
            Rectangle healBtn = new Rectangle(px + 10, cy, pw - 30, 28);
            bool hHeal = Raylib.CheckCollisionPointRec(mouse, healBtn);
            Raylib.DrawRectangleRec(healBtn, hHeal ? new Color((byte)40, (byte)80, (byte)40, (byte)255) : new Color((byte)30, (byte)30, (byte)40, (byte)255));
            Raylib.DrawRectangleLinesEx(healBtn, 1, Color.Green);
            Program.DrawTextUI("Full Heal + Restore", px + 18, cy + 5, 16, hHeal ? Color.Lime : Color.White);
            if (hHeal && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                player.Health = player.MaxHealth;
                player.Food = 100; player.Thirst = 100; player.Stamina = 100;
                ShowNotification("Fully healed!");
            }
            cy += 40;

            // ── TELEPORT ──
            Program.DrawTextUI("─── TELEPORT ───", px + 10, cy, 16, Color.Lime);
            cy += 26;

            (string label, float tx, float ty)[] teleports =
            {
                ("Spawn / Safe Zone",     500,      0),
                ("Farm Zone",            -1500,  -8000),
                ("Hamiltron City",       14000,   5600),
                ("Rotoaira Town",       -15800,   4700),
                ("Frosthold (Snow)",    -140000, -140000),
                ("Ironpeak (Mountain)", -10000,  -160000),
                ("Cinderfall (Volcano)", 140000, -150000),
                ("Murkwater (Swamp)",   -140000,  10000),
                ("Sunhaven (Desert)",    140000,  10000),
                ("Eldergrove (Forest)", -140000,  140000),
                ("Tidecrest (Beach)",    20000,   95000),
                ("Frost Wyrm Boss",     -160000, -160000),
                ("Stone Guardian Boss",  -20000, -180000),
                ("Infernal Golem Boss",  160000, -180000),
                ("Sand King Boss",       160000,  20000),
                ("Ancient Treant Boss", -160000,  160000),
                ("Reef Leviathan Boss",  40000,   160000),
                ("Colossus Boss",        5000,   -2000),
                ("Titan Boss",          -8000,    4000),
            };

            for (int i = 0; i < teleports.Length; i++)
            {
                var tp = teleports[i];
                Rectangle btn = new Rectangle(px + 10, cy, pw - 30, 24);
                bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
                Raylib.DrawRectangleRec(btn, hover ? new Color((byte)50, (byte)40, (byte)20, (byte)255) : new Color((byte)25, (byte)25, (byte)35, (byte)255));
                Program.DrawTextUI(tp.label, px + 18, cy + 4, 14, hover ? Color.Gold : Color.LightGray);

                if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    player.Position = new Vector2(tp.tx, tp.ty);
                    ShowNotification($"Teleported to {tp.label}");
                    timesCheated++;
                }
                cy += 26;
            }

            Raylib.EndScissorMode();
        }

        static void DrawNotificationBanner()
    {
        if (levelUpTimer <= 0) return;
        byte alpha = (byte)(255 * Math.Min(1f, levelUpTimer));
        int textWidth = Program.MeasureTextUI(levelUpMessage, 40);
        int bx = ScreenWidth / 2 - textWidth / 2;
        // NEW — black backing box for readability (alpha fades with the text)
        byte boxA = (byte)(200 * Math.Min(1f, levelUpTimer));
        Raylib.DrawRectangle(bx - 20, 272, textWidth + 40, 56, new Color((byte)0,(byte)0,(byte)0,boxA));
        Raylib.DrawRectangleLines(bx - 20, 272, textWidth + 40, 56, new Color((byte)255,(byte)215,(byte)0,alpha));
        Program.DrawTextUI(levelUpMessage, bx, 280, 40, new Color((byte)255,(byte)215,(byte)0,alpha));
    }

static void DrawCashHUD()
{
    int boxW = 150, boxH = 40;
    int boxX = ScreenWidth - boxW - 12, boxY = 8 + 54 + 20;   // 6px gap below the calendar box
    Raylib.DrawRectangle(boxX, boxY, boxW, boxH, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Raylib.DrawRectangleLines(boxX, boxY, boxW, boxH, Color.Gold);
    Program.DrawTextUI($"${player.Money}", boxX + 8, boxY + 8, 22, Color.Gold);
}

        static void DrawSpeechBubble(Vector2 npcPos, string text, Color bubbleColor)
{
    int padding = 8;
    int fontSize = 14;
    int textWidth = Program.MeasureTextUI(text, fontSize);
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
    Program.DrawTextUI(text, bx + padding, by + padding, fontSize,
        new Color((byte)20,(byte)20,(byte)20,(byte)255));
}

        public static void ShowNotification(string message)
        {
            QueueNotification(message);
        }

        static void RegisterComboHit()
        {
            comboCount++;
            comboTimer = comboWindow;
        }

                public static void ShowLevelUp(string skill, int level)
                    {
                    levelUpMessage = $"{skill} LEVEL UP! {level}";
                    levelUpTimer = 2.5f;
                    TriggerScreenFlash(Color.Gold, 0.15f);
                    }

        static void UpdateQuests()
{
    foreach (Quest quest in quests)
    {
        if (quest.Completed) continue;
        if (quest.Title == "Lumberjack")     quest.Progress = player.Logs;
        if (quest.Title == "Fisher")         quest.Progress = player.Fish;
        if (quest.Title == "Big Money")      quest.Progress = player.Money;       
        if (quest.Title == "Treasure Hunter") quest.Progress = CollectablesFound;
        if (quest.Title == "Wolf Culler")     quest.Progress = wolvesKilled;
        if (quest.Title == "Green Thumb")     quest.Progress = cropsHarvested;
        if (quest.Title == "Master Chef")     quest.Progress = mealsCooked;
        if (quest.Title == "Deep Pockets")    quest.Progress = player.BankBalance;
        if (quest.Progress >= quest.Target)
        {
            quest.Completed = true;
            quest.Progress = quest.Target;
            player.Money += quest.Reward;
            AddReputation(25, quest.Title);
            ShowLevelUp($"Quest Complete: {quest.Title}! +${quest.Reward}", 0);
        }
    }
}

            static void DrawQuestsUI()
{
    // 0. REPUTATION badge (above quest/skill buttons, left-aligned)
    {
        var (repTitle, _) = GetReputationTier(player.Reputation);
        var (curThresh, nxtThresh) = GetReputationProgress(player.Reputation);
        float repProg = nxtThresh > curThresh ? (float)(player.Reputation - curThresh) / (nxtThresh - curThresh) : 1f;
        int repX = 170, repY = 38;
        Raylib.DrawRectangle(repX, repY, 300, 32, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Raylib.DrawRectangleLinesEx(new Rectangle(repX, repY, 300, 32), 1, new Color((byte)180,(byte)140,(byte)40,(byte)255));
        Program.DrawTextUI($"{repTitle}", repX + 6, repY + 3, 14, new Color((byte)255,(byte)215,(byte)0,(byte)255));
        Program.DrawTextUI($"{player.Reputation} rep", repX + 110, repY + 3, 14, Color.LightGray);
        Raylib.DrawRectangle(repX + 6, repY + 22, 288, 6, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(repX + 6, repY + 22, (int)(288 * repProg), 6, new Color((byte)255,(byte)215,(byte)0,(byte)255));
    }

    // 1. QUESTS Toggle Button (Bottom Right)
    Rectangle questsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 60, 140, 40);
    Raylib.DrawRectangleRec(questsBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(questsBtn, 2, questsOpen ? Color.Gold : Color.White);
    Program.DrawTextUI("QUESTS", ScreenWidth - 300, ScreenHeight - 48, 22, questsOpen ? Color.Gold : Color.White);

    if (!questsOpen) return;

    // 2. Dynamic Panel Positioning Math
    // We base everything off px (X) and py (Y) so it forms a clean matching container box
    int px = ScreenWidth - 330;
    int py = ScreenHeight - 480; // The box top edge sits at Y=240
    int pw = 310;
    int ph = 410; // Expanded to 410 tall so both standard and story quests easily fit!

    // Draw the main panel background frame
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)0, (byte)0, (byte)0, (byte)220));
    Raylib.DrawRectangleLines(px, py, pw, ph, Color.White);
    Program.DrawTextUI("ACTIVE QUESTS", px + 20, py + 12, 22, Color.Gold);

    int contentTop = py + 44;
    int contentH = ph - 54;
    int activeFavors = friendNPCs.Count(fn => fn.ActiveFavor != null && !fn.ActiveFavor.Completed);
    int totalItems = quests.Count + storyQuests.Count(sq => sq != null && sq.Started && !sq.Completed && sq.Current != null) + activeFavors;
    int contentTotal = totalItems * 55;
    float maxScroll = Math.Max(0, contentTotal - contentH);
    if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(px, py, pw, ph)))
        questScrollY = Math.Clamp(questScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
    questScrollY = Math.Clamp(questScrollY, 0f, maxScroll);   // re-clamp when quests complete
    Raylib.BeginScissorMode(px, contentTop, pw, contentH);

    int yOffset = 50 - (int)questScrollY; 

    // --- LOOP A: Standard Side-Quests ---
    foreach (Quest quest in quests)
    {
        Color questColor = quest.Completed ? Color.Green : Color.White;
        string tick = quest.Completed ? "[DONE]" : $"{quest.Progress}/{quest.Target}";
        
        Program.DrawTextUI(quest.Description, px + 15, py + yOffset, 18, questColor);
        Program.DrawTextUI(tick, px + 15, py + yOffset + 22, 16, questColor);
        Program.DrawTextUI($"Reward: ${quest.Reward}", px + 160, py + yOffset + 22, 16, Color.Gold);
        
        yOffset += 55; // Steps down so the next quest draws cleanly beneath
    }

    // --- LOOP B: Main Story Quests (Appends beneath standard quests automatically!) ---
    foreach (var sq in storyQuests)
    {
        if (sq == null || !sq.Started || sq.Completed) continue;
        
        var st = sq.Current;
        if (st == null) continue;

        int prog = st.Progress != null ? Math.Clamp(st.Progress() - st.Baseline, 0, st.Target) : 0;
        
        Program.DrawTextUI($"[STORY] {sq.Title}", px + 15, py + yOffset, 18, Color.SkyBlue);
        
        string progressText = st.Progress != null ? $"{st.Description} ({prog}/{st.Target})" : st.Description;
        Program.DrawTextUI(progressText, px + 15, py + yOffset + 22, 16, Color.LightGray);
        
        yOffset += 55; // Pushes down context steps for any additional listings
    }

    // --- LOOP C: NPC Favors ---
    foreach (var fn in friendNPCs)
    {
        if (fn.ActiveFavor == null || fn.ActiveFavor.Completed) continue;
        var fav = fn.ActiveFavor;
        int delivered = fav.AmountDelivered;
        int needed = fav.AmountNeeded;

        Program.DrawTextUI($"[FAVOR] {fn.Name}", px + 15, py + yOffset, 18, new Color((byte)255,(byte)200,(byte)40,(byte)255));
        Program.DrawTextUI($"{fav.ItemNeeded}: {delivered}/{needed}  |  +${fav.RewardAmount}", px + 15, py + yOffset + 22, 16, Color.LightGray);
        yOffset += 55;
    }

    Raylib.EndScissorMode();
    if (maxScroll > 0)   
    {
        float barH = Math.Max(30f, contentH * (contentH / (float)contentTotal));
        float barY = contentTop + (questScrollY / maxScroll) * (contentH - barH);
        Raylib.DrawRectangle(px + pw - 8, contentTop, 6, contentH, new Color((byte)40,(byte)40,(byte)40,(byte)200));
        Raylib.DrawRectangle(px + pw - 8, (int)barY, 6, (int)barH, Color.Gold);
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
            if (!pauseMenuOpen && !player.InventoryOpen && !armorMenuOpen)
            {
                skillsOpen = !skillsOpen;
                if (skillsOpen) questsOpen = false;
            }

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
        Rectangle gamblingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 480, 140, 40);
        Rectangle ridingBtn  = new Rectangle(ScreenWidth - 160, ScreenHeight - 530, 140, 40);
        Rectangle cyclingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 580, 140, 40);
        Rectangle swimmingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 630, 140, 40);
        Rectangle divingBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 180, 140, 40);
        Rectangle sportsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 130, 140, 40);
        Rectangle rangedBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 230, 140, 40);
        Rectangle cookingBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 280, 140, 40);
        Rectangle elementalBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 330, 140, 40);


        hoverStrength  = Raylib.CheckCollisionPointRec(mouse, strengthBtn);
        hoverAthletics = Raylib.CheckCollisionPointRec(mouse, athleticsBtn);
        hoverDriving   = Raylib.CheckCollisionPointRec(mouse, drivingBtn);
        hoverCombat = Raylib.CheckCollisionPointRec(mouse, combatBtn);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(mouse, wcBtn))
                skillDetailOpen = skillDetailOpen == "wc" ? "" : "wc";
            else if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 380, 140, 40)))
                skillDetailOpen = skillDetailOpen == "cards" ? "" : "cards";
            else if (Raylib.CheckCollisionPointRec(mouse, fishBtn))
                skillDetailOpen = skillDetailOpen == "fishing" ? "" : "fishing";
            else if (Raylib.CheckCollisionPointRec(mouse, combatBtn))
                skillDetailOpen = skillDetailOpen == "combat" ? "" : "combat";
            else if (Raylib.CheckCollisionPointRec(mouse, miningBtn))
                skillDetailOpen = skillDetailOpen == "mining" ? "" : "mining";
            else if (Raylib.CheckCollisionPointRec(mouse, cookingBtn))
                skillDetailOpen = skillDetailOpen == "cooking" ? "" : "cooking";
            else if (Raylib.CheckCollisionPointRec(mouse, strengthBtn))
                skillDetailOpen = skillDetailOpen == "strength" ? "" : "strength";
            else if (Raylib.CheckCollisionPointRec(mouse, athleticsBtn))
                skillDetailOpen = skillDetailOpen == "athletics" ? "" : "athletics";
            else if (Raylib.CheckCollisionPointRec(mouse, drivingBtn))
                skillDetailOpen = skillDetailOpen == "driving" ? "" : "driving";
            else if (Raylib.CheckCollisionPointRec(mouse, rangedBtn))
                skillDetailOpen = skillDetailOpen == "ranged" ? "" : "ranged";
            else if (Raylib.CheckCollisionPointRec(mouse, ridingBtn))
                skillDetailOpen = skillDetailOpen == "riding" ? "" : "riding";
            else if (Raylib.CheckCollisionPointRec(mouse, swimmingBtn))
                skillDetailOpen = skillDetailOpen == "swimming" ? "" : "swimming";
            else if (Raylib.CheckCollisionPointRec(mouse, gamblingBtn))
                skillDetailOpen = skillDetailOpen == "gambling" ? "" : "gambling";
            else if (skillDetailOpen != "" &&
                     !Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth/2 - 300, ScreenHeight/2 - 260, 600, 520)))
                skillDetailOpen = "";
            else if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 530, 140, 40)))
                skillDetailOpen = skillDetailOpen == "farming" ? "" : "farming";
            else if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 580, 140, 40)))
                skillDetailOpen = skillDetailOpen == "education" ? "" : "education";
                    }
        hoverWoodcutting = Raylib.CheckCollisionPointRec(mouse, wcBtn);
        hoverWoodcutting = Raylib.CheckCollisionPointRec(mouse, wcBtn);
        hoverFishing = Raylib.CheckCollisionPointRec(mouse, fishBtn);
        hoverMining = Raylib.CheckCollisionPointRec(mouse, miningBtn);
        hoverGambling = Raylib.CheckCollisionPointRec(mouse, gamblingBtn);
        hoverRiding  = Raylib.CheckCollisionPointRec(mouse, ridingBtn);
        hoverCycling = Raylib.CheckCollisionPointRec(mouse, cyclingBtn);
        hoverSwimming = Raylib.CheckCollisionPointRec(mouse, swimmingBtn);
        hoverDiving   = Raylib.CheckCollisionPointRec(mouse, divingBtn);
        hoverSports = Raylib.CheckCollisionPointRec(mouse, sportsBtn);
        hoverRanged = Raylib.CheckCollisionPointRec(mouse, rangedBtn);
        hoverCooking = Raylib.CheckCollisionPointRec(mouse, cookingBtn);
        hoverElemental = Raylib.CheckCollisionPointRec(mouse, elementalBtn);
        hoverCards = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 380, 140, 40));
        hoverOneHand = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 430, 140, 40));
        hoverTwoHand = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 480, 140, 40));
        hoverFarming = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 530, 140, 40));
        hoverEducation = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 320, ScreenHeight - 580, 140, 40));
        hoverCraftSkill = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 130, 140, 40));
        hoverBlacksmith = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 180, 140, 40));
        hoverEnchanting = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 230, 140, 40));
        hoverStaminaSkill = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 280, 140, 40));
        hoverFaith = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 330, 140, 40));
        hoverMystical = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 380, 140, 40));
        hoverDarkArts = Raylib.CheckCollisionPointRec(mouse, new Rectangle(ScreenWidth - 480, ScreenHeight - 430, 140, 40));

    }
    else
    {
        skillDetailOpen = "";
        hoverWoodcutting = false;
        hoverFishing = false;
        hoverCombat = false;
        hoverStrength  = false;
        hoverAthletics = false;
        hoverDriving   = false;
        hoverMining   = false;
        hoverGambling = false;
        hoverRiding  = false;
        hoverCycling = false;
        hoverSwimming = false;
        hoverDiving = false;
        hoverSports = false;
        hoverRanged = false;
        hoverCooking = false;
        hoverElemental = false;
        hoverCards = false;
        hoverOneHand = false;
        hoverTwoHand = false;
        hoverFarming = false;
        hoverEducation = false;
        hoverCraftSkill = false; 
        hoverBlacksmith = false;
        hoverEnchanting = false;
        hoverStaminaSkill = false;
        hoverFaith = false;
        hoverMystical = false;
        hoverDarkArts = false;

            }
}

// ── ACHIEVEMENTS UI ──────────────────────────────────────────
    static void DrawAchievementsUI()
    {
        // achievement popup banner (gold bar at top)
        if (achievementPopupTimer > 0)
        {
            achievementPopupTimer -= Raylib.GetFrameTime();
            byte alpha = (byte)(255 * Math.Min(1f, achievementPopupTimer));
            string popText = $"ACHIEVEMENT UNLOCKED: {achievementPopupTitle}";
            int tw = Program.MeasureTextUI(popText, 24);
            int bx = ScreenWidth / 2 - tw / 2 - 20;
            Raylib.DrawRectangle(bx, 30, tw + 40, 50, new Color((byte)20,(byte)15,(byte)5,alpha));
            Raylib.DrawRectangleLinesEx(new Rectangle(bx, 30, tw + 40, 50), 2, new Color((byte)255,(byte)215,(byte)0,alpha));
            // trophy icon
            Raylib.DrawCircle(bx + 20, 55, 10, new Color((byte)255,(byte)215,(byte)0,alpha));
            Program.DrawTextUI("★", bx + 13, 43, 18, new Color((byte)40,(byte)20,(byte)0,alpha));
            Program.DrawTextUI(popText, bx + 38, 42, 24, new Color((byte)255,(byte)230,(byte)100,alpha));
        }

        // toggle button — positioned left of QUESTS button
        Rectangle achBtn = new Rectangle(ScreenWidth - 490, ScreenHeight - 60, 150, 40);
        Raylib.DrawRectangleRec(achBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Raylib.DrawRectangleLinesEx(achBtn, 2, achievementsOpen ? Color.Gold : Color.White);
        string achLabel = $"ACHIEVE {achievementsUnlockedCount}/{achievements.Count}";
        Program.DrawTextUI(achLabel, (int)achBtn.X + 8, ScreenHeight - 48, 18, achievementsOpen ? Color.Gold : Color.White);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left)
            && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), achBtn))
            achievementsOpen = !achievementsOpen;

        if (!achievementsOpen) return;

        // panel
        int pw = 420, ph = 460;
        int px = ScreenWidth - 500;
        int py = ScreenHeight - ph - 70;
        Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)0,(byte)0,(byte)0,(byte)230));
        Raylib.DrawRectangleLines(px, py, pw, ph, Color.Gold);

        // title + completion bar
        Program.DrawTextUI("ACHIEVEMENTS", px + 12, py + 8, 24, Color.Gold);
        float pct = achievements.Count > 0 ? (float)achievementsUnlockedCount / achievements.Count : 0f;
        Raylib.DrawRectangle(px + 180, py + 14, 220, 14, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(px + 180, py + 14, (int)(220 * pct), 14, new Color((byte)255,(byte)215,(byte)0,(byte)255));
        Program.DrawTextUI($"{pct*100:F0}%", px + 405, py + 10, 14, Color.White);

        // category tabs
        Vector2 mouse = Raylib.GetMousePosition();
        int tabY = py + 36;
        int tabX = px + 6;
        for (int i = 0; i < achievementCategories.Length; i++)
        {
            string cat = achievementCategories[i];
            int catW = Program.MeasureTextUI(cat, 13) + 12;
            Rectangle tab = new Rectangle(tabX, tabY, catW, 22);
            bool hover = Raylib.CheckCollisionPointRec(mouse, tab);
            bool active = achievementCategory == i;
            Raylib.DrawRectangleRec(tab, active ? new Color((byte)60,(byte)50,(byte)20,(byte)255) : new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangleLinesEx(tab, 1, active ? Color.Gold : (hover ? Color.White : new Color((byte)60,(byte)60,(byte)60,(byte)255)));
            Program.DrawTextUI(cat, tabX + 6, tabY + 4, 13, active ? Color.Gold : (hover ? Color.White : Color.Gray));
            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left)) { achievementCategory = i; achievementScrollY = 0f; }
            tabX += catW + 4;
            if (tabX > px + pw - 40) { tabX = px + 6; tabY += 26; }
        }

        // filtered list
        int contentTop = tabY + 30;
        int contentH = py + ph - contentTop - 8;
        string filterCat = achievementCategory == 0 ? null : achievementCategories[achievementCategory];
        var filtered = filterCat == null ? achievements : achievements.Where(a => a.Category == filterCat).ToList();

        int rowH = 56;
        int totalH = filtered.Count * rowH;
        float maxScroll = Math.Max(0, totalH - contentH);
        if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(px, contentTop, pw, contentH)))
            achievementScrollY = Math.Clamp(achievementScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
        achievementScrollY = Math.Clamp(achievementScrollY, 0f, maxScroll);

        Raylib.BeginScissorMode(px, contentTop, pw, contentH);
        for (int i = 0; i < filtered.Count; i++)
        {
            var a = filtered[i];
            int ry = contentTop + i * rowH - (int)achievementScrollY;
            if (ry + rowH < contentTop || ry > contentTop + contentH) continue;

            Color bgCol = a.Unlocked
                ? new Color((byte)30,(byte)40,(byte)20,(byte)200)
                : new Color((byte)20,(byte)20,(byte)25,(byte)200);
            Raylib.DrawRectangle(px + 4, ry, pw - 8, rowH - 4, bgCol);
            Raylib.DrawRectangleLinesEx(new Rectangle(px + 4, ry, pw - 8, rowH - 4), 1,
                a.Unlocked ? new Color((byte)120,(byte)180,(byte)60,(byte)255) : new Color((byte)50,(byte)50,(byte)50,(byte)255));

            // icon
            if (a.Unlocked)
            {
                Raylib.DrawCircle(px + 24, ry + rowH / 2 - 2, 12, a.IconColor);
                Program.DrawTextUI("★", px + 17, ry + rowH / 2 - 12, 18, new Color((byte)255,(byte)255,(byte)255,(byte)230));
            }
            else
            {
                Raylib.DrawCircle(px + 24, ry + rowH / 2 - 2, 12, new Color((byte)40,(byte)40,(byte)40,(byte)255));
                Program.DrawTextUI("?", px + 19, ry + rowH / 2 - 10, 16, Color.DarkGray);
            }

            // title + desc
            Color titleCol = a.Unlocked ? Color.White : Color.Gray;
            Color descCol = a.Unlocked ? Color.LightGray : new Color((byte)80,(byte)80,(byte)80,(byte)255);
            Program.DrawTextUI(a.Title, px + 44, ry + 4, 18, titleCol);
            Program.DrawTextUI(a.Description, px + 44, ry + 24, 14, descCol);

            // reward
            if (a.Reward > 0)
            {
                string rText = a.Unlocked ? $"+${a.Reward}" : $"${a.Reward}";
                Color rCol = a.Unlocked ? new Color((byte)100,(byte)200,(byte)80,(byte)255) : new Color((byte)150,(byte)130,(byte)50,(byte)255);
                Program.DrawTextUI(rText, px + pw - 70, ry + 6, 14, rCol);
            }

            // status
            string status = a.Unlocked ? "DONE" : "LOCKED";
            Color stCol = a.Unlocked ? new Color((byte)100,(byte)220,(byte)60,(byte)255) : new Color((byte)100,(byte)100,(byte)100,(byte)255);
            Program.DrawTextUI(status, px + pw - 70, ry + 28, 12, stCol);
        }
        Raylib.EndScissorMode();
    }

    static void DrawSkillsUI()
{
    Rectangle skillsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 60, 140, 40);
    Raylib.DrawRectangleRec(skillsBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(skillsBtn, 2, skillsOpen ? Color.Gold : Color.White);
    Program.DrawTextUI("SKILLS", ScreenWidth - 130, ScreenHeight - 48, 22, skillsOpen ? Color.Gold : Color.White);

    if (!skillsOpen) return;

    // Woodcutting
    Rectangle wcBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 130, 140, 40);
    Color wcColor = hoverWoodcutting ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(wcBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(wcBtn, 2, wcColor);
    Program.DrawTextUI($"WC Lv {player.WoodcuttingLevel}", ScreenWidth - 155, ScreenHeight - 118, 20, wcColor);
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
    Program.DrawTextUI($"Mining Lv {player.MiningLevel}", ScreenWidth - 155, ScreenHeight - 418, 20, miningColor);
    if (!hoverMining)
    {
        int miningRequired = player.MiningLevel * player.MiningLevel * 50;
        float miningProgress = (float)player.MiningXP / miningRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 393, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 393, (int)(140 * miningProgress), 8, Color.Green);
    }

    // Fishing
    Rectangle fishBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 180, 140, 40);
    Color fishColor = hoverFishing ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(fishBtn, new Color((byte)0, (byte)0, (byte)0, (byte)200));
    Raylib.DrawRectangleLinesEx(fishBtn, 2, fishColor);
    Program.DrawTextUI($"Fish Lv {player.FishingLevel}", ScreenWidth - 155, ScreenHeight - 168, 20, fishColor);
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
    Program.DrawTextUI($"Combat Lv {player.CombatLevel}", ScreenWidth - 155, ScreenHeight - 218, 20, combatColor);
    if (!hoverCombat)
    {
        int combatRequired = player.CombatLevel * player.CombatLevel * 50;
        float combatProgress = (float)player.CombatXP / combatRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, 140, 8, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 193, (int)(140 * combatProgress), 8, Color.Red);
    }
    // Driving
   Rectangle drivingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 280, 140, 40);
    Color drivingColor = hoverDriving ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(drivingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(drivingBtn, 2, drivingColor);
    Program.DrawTextUI($"Drive Lv {player.DrivingLevel}", ScreenWidth - 155, ScreenHeight - 268, 20, drivingColor);
    if (!hoverDriving)
    {
        int drivingRequired = player.DrivingLevel * player.DrivingLevel * 50;
        float drivingProgress = (float)player.DrivingXP / drivingRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 243, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 243, (int)(140 * drivingProgress), 8, new Color((byte)255,(byte)200,(byte)0,(byte)255));
    }
    // Athletics
    Rectangle athleticsBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 330, 140, 40);
    Color athleticsColor = hoverAthletics ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(athleticsBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(athleticsBtn, 2, athleticsColor);
    Program.DrawTextUI($"Ath Lv {player.AthleticsLevel}", ScreenWidth - 155, ScreenHeight - 318, 20, athleticsColor);
    if (!hoverAthletics)
    {
        int athleticsRequired = player.AthleticsLevel * player.AthleticsLevel * 50;
        float athleticsProgress = (float)player.AthleticsXP / athleticsRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 293, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 293, (int)(140 * athleticsProgress), 8, new Color((byte)0,(byte)200,(byte)255,(byte)255));
    }
    // Strength
    Rectangle strengthBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 380, 140, 40);
    Color strengthColor = hoverStrength ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(strengthBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(strengthBtn, 2, strengthColor);
    Program.DrawTextUI($"Str Lv {player.StrengthLevel}", ScreenWidth - 155, ScreenHeight - 368, 20, strengthColor);
    if (!hoverStrength)
    {
        int strengthRequired = player.StrengthLevel * player.StrengthLevel * 50;
        float strengthProgress = (float)player.StrengthXP / strengthRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 343, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 343, (int)(140 * strengthProgress), 8, new Color((byte)255,(byte)80,(byte)80,(byte)255));
    }
    // Gambling
    Rectangle gamblingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 480, 140, 40);
    Color gamblingColor = hoverGambling ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(gamblingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(gamblingBtn, 2, gamblingColor);
    Program.DrawTextUI($"Gamble Lv {player.GamblingLevel}", ScreenWidth - 155, ScreenHeight - 468, 20, gamblingColor);
    if (!hoverGambling)
    {
        int gamblingRequired = player.GamblingLevel * player.GamblingLevel * 50;
        float gamblingProgress = (float)player.GamblingXP / gamblingRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 443, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 443, (int)(140 * gamblingProgress), 8, new Color((byte)220,(byte)160,(byte)20,(byte)255));
    }
        // Riding
    Rectangle ridingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 530, 140, 40);
    Color ridingColor = hoverRiding ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(ridingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(ridingBtn, 2, ridingColor);
    Program.DrawTextUI($"Riding Lv {player.RidingLevel}", ScreenWidth - 155, ScreenHeight - 518, 20, ridingColor);
    if (!hoverRiding)
    {
        int ridingRequired = player.RidingLevel * player.RidingLevel * 50;
        float ridingProgress = (float)player.RidingXP / ridingRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 493, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 493, (int)(140 * ridingProgress), 8, new Color((byte)160,(byte)100,(byte)40,(byte)255));
    }

    // Cycling
    Rectangle cyclingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 580, 140, 40);
    Color cyclingColor = hoverCycling ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(cyclingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(cyclingBtn, 2, cyclingColor);
    Program.DrawTextUI($"Cycling Lv {player.CyclingLevel}", ScreenWidth - 155, ScreenHeight - 568, 20, cyclingColor);
    if (!hoverCycling)
    {
        int cyclingRequired = player.CyclingLevel * player.CyclingLevel * 50;
        float cyclingProgress = (float)player.CyclingXP / cyclingRequired;
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 543, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 543, (int)(140 * cyclingProgress), 8, new Color((byte)100,(byte)200,(byte)80,(byte)255));
    }
    // Swimming
Rectangle swimmingBtn = new Rectangle(ScreenWidth - 160, ScreenHeight - 630, 140, 40);
Color swimmingColor = hoverSwimming ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(swimmingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(swimmingBtn, 2, swimmingColor);
Program.DrawTextUI($"Swimming Lv {player.SwimmingLevel}", ScreenWidth - 155, ScreenHeight - 618, 18, swimmingColor);
if (!hoverSwimming)
{
    int req = player.SwimmingLevel * player.SwimmingLevel * 50;
    float prog = (float)player.SwimmingXP / req;
    Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 593, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 160, ScreenHeight - 593, (int)(140 * prog), 8, new Color((byte)40,(byte)140,(byte)220,(byte)255));
}

// Diving
Rectangle divingBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 180, 140, 40);
Color divingColor = hoverDiving ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(divingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(divingBtn, 2, divingColor);
Program.DrawTextUI($"Diving Lv {player.DivingLevel}", ScreenWidth - 315, ScreenHeight - 168, 18, divingColor);
if (!hoverDiving)
{
    int req = player.DivingLevel * player.DivingLevel * 50;
    float prog = (float)player.DivingXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 143, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 143, (int)(140 * prog), 8, new Color((byte)80,(byte)180,(byte)255,(byte)255));
}

// Sports
Rectangle sportsBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 130, 140, 40);
Color sportsColor = hoverSports ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(sportsBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(sportsBtn, 2, sportsColor);
Program.DrawTextUI($"Sports Lv {player.SportsLevel}", ScreenWidth - 315, ScreenHeight - 118, 18, sportsColor);
if (!hoverSports)
{
    int req = player.SportsLevel * player.SportsLevel * 50;
    float prog = (float)player.SportsXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 93, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 93, (int)(140 * prog), 8, new Color((byte)230,(byte)120,(byte)40,(byte)255));
}

// Ranged
Rectangle rangedBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 230, 140, 40);
Color rangedColor = hoverRanged ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(rangedBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(rangedBtn, 2, rangedColor);
Program.DrawTextUI($"Ranged Lv {player.RangedLevel}", ScreenWidth - 315, ScreenHeight - 218, 18, rangedColor);
if (!hoverRanged)
{
    int req = player.RangedLevel * player.RangedLevel * 50;
    float prog = (float)player.RangedXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 193, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 193, (int)(140 * prog), 8, new Color((byte)150,(byte)100,(byte)200,(byte)255));
}

// Cooking
Rectangle cookingBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 280, 140, 40);
Color cookingColor = hoverCooking ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(cookingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(cookingBtn, 2, cookingColor);
Program.DrawTextUI($"Cooking Lv {player.CookingLevel}", ScreenWidth - 315, ScreenHeight - 268, 18, cookingColor);
if (!hoverCooking)
{
    int req = player.CookingLevel * player.CookingLevel * 50;
    float prog = (float)player.CookingXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 243, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 243, (int)(140 * prog), 8, new Color((byte)150,(byte)100,(byte)200,(byte)255));
}

// Elemental
Rectangle elementalBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 330, 140, 40);
Color elementalColor = hoverElemental ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(elementalBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(elementalBtn, 2, elementalColor);
Program.DrawTextUI($"Elemental: {player.ElementalLevel}", ScreenWidth - 315, ScreenHeight - 318, 18, elementalColor);

if (!hoverElemental)
{
    int req = player.ElementalLevel * player.ElementalLevel * 50;
    float prog = (float)player.ElementalXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 293, 140, 8, new Color((byte)0,(byte)0,(byte)0,(byte)210));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 293, (int)(140 * prog), 8, new Color((byte)150,(byte)100,(byte)200,(byte)255));
}

// Cards
Rectangle cardsSkillBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 380, 140, 40);
Color cardsColor = hoverCards ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(cardsSkillBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(cardsSkillBtn, 2, cardsColor);
Program.DrawTextUI($"Cards Lv {player.PlayingCardsLevel}", ScreenWidth - 315, ScreenHeight - 368, 20, cardsColor);
int cardsReq = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
float cardsProg = (float)player.PlayingCardsXP / cardsReq;
Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 343, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 343, (int)(140 * cardsProg), 8, Color.Gold);

// 1H Melee
Rectangle oneHandBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 430, 140, 40);
Color oneHandColor = hoverOneHand ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(oneHandBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(oneHandBtn, 2, oneHandColor);
Program.DrawTextUI($"1H Melee Lv {player.OneHandMeleeLevel}", ScreenWidth - 315, ScreenHeight - 418, 18, oneHandColor);
if (!hoverOneHand)
{
    int req = player.OneHandMeleeLevel * player.OneHandMeleeLevel * 50;
    float prog = (float)player.OneHandMeleeXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 393, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 393, (int)(140 * prog), 8, new Color((byte)230,(byte)160,(byte)60,(byte)255));
}

// 2H Melee
Rectangle twoHandBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 480, 140, 40);
Color twoHandColor = hoverTwoHand ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(twoHandBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(twoHandBtn, 2, twoHandColor);
Program.DrawTextUI($"2H Melee Lv {player.TwoHandMeleeLevel}", ScreenWidth - 315, ScreenHeight - 468, 18, twoHandColor);
if (!hoverTwoHand)
{
    int req = player.TwoHandMeleeLevel * player.TwoHandMeleeLevel * 50;
    float prog = (float)player.TwoHandMeleeXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 443, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 443, (int)(140 * prog), 8, new Color((byte)200,(byte)80,(byte)40,(byte)255));
}

// Farming
Rectangle farmingBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 530, 140, 40);
Color farmingColor = hoverFarming ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(farmingBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(farmingBtn, 2, farmingColor);
Program.DrawTextUI($"Farming Lv {player.FarmingLevel}", ScreenWidth - 315, ScreenHeight - 518, 18, farmingColor);
if (!hoverFarming)
{
    int req = player.FarmingLevel * player.FarmingLevel * 50;
    float prog = (float)player.FarmingXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 493, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 493, (int)(140 * prog), 8, new Color((byte)90,(byte)170,(byte)60,(byte)255));
}

// Education
Rectangle educationBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 580, 140, 40);
Color educationColor = hoverEducation ? Color.Gold : Color.White;
Raylib.DrawRectangleRec(educationBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
Raylib.DrawRectangleLinesEx(educationBtn, 2, educationColor);
Program.DrawTextUI($"Education Lv {player.EducationLevel}", ScreenWidth - 315, ScreenHeight - 568, 18, educationColor);
if (!hoverEducation)
{
    int req = player.EducationLevel * player.EducationLevel * 50;
    float prog = (float)player.EducationXP / req;
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 543, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 543, (int)(140 * prog), 8, new Color((byte)220,(byte)80,(byte)80,(byte)255));
}
(string name, int lv, int xp, bool hov, int slotY)[] newSkills = {
    ("Crafting",   player.CraftingLevel,   player.CraftingXP,   hoverCraftSkill, 130),
    ("Blacksmith", player.BlacksmithLevel, player.BlacksmithXP, hoverBlacksmith, 180),
    ("Enchanting", player.EnchantingLevel, player.EnchantingXP, hoverEnchanting, 230),
    ("Stamina", player.StaminaLevel, player.StaminaXP, hoverStaminaSkill, 280),
    ("Faith", player.FaithLevel, player.FaithXP, hoverFaith, 330),
    ("Mystical", player.MysticalLevel, player.MysticalXP, hoverMystical, 380),
    ("DarkArts", player.DarkArtsLevel, player.DarkArtsXP, hoverDarkArts, 430),
};
foreach (var s in newSkills)
{
    Rectangle b = new Rectangle(ScreenWidth - 480, ScreenHeight - s.slotY, 140, 40);
    Color c = s.hov ? Color.Gold : Color.White;
    Raylib.DrawRectangleRec(b, new Color((byte)0,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangleLinesEx(b, 2, c);
    Program.DrawTextUI($"{s.name} Lv {s.lv}", ScreenWidth - 475, ScreenHeight - s.slotY + 12, 17, c);
    if (!s.hov)
    {
        int req = s.name == "Stamina" ? s.lv * s.lv * 300 : s.lv * s.lv * 50; 
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - s.slotY + 37, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - s.slotY + 37, (int)(140 * (float)s.xp / req), 8, new Color((byte)200,(byte)120,(byte)255,(byte)255));
    }
    else
    {
        int req = s.name == "Stamina" ? s.lv * s.lv * 300 : s.lv * s.lv * 50; 
        Raylib.DrawRectangle(ScreenWidth - 640, ScreenHeight - s.slotY, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {s.xp}/{req}", ScreenWidth - 635, ScreenHeight - s.slotY + 12, 18, Color.LightGray);
    }
}

    // XP tooltips
    if (hoverWoodcutting)
    {
        int required = player.WoodcuttingLevel * player.WoodcuttingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 130, 200, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.WoodcuttingXP}/{required}  [click]", ScreenWidth - 315, ScreenHeight - 118, 18, Color.LightGray);
    }
    if (hoverFishing)
    {
        int required = player.FishingLevel * player.FishingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 180, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Program.DrawTextUI($"XP: {player.FishingXP}/{required}", ScreenWidth - 315, ScreenHeight - 168, 20, Color.LightGray);
    }
    if (hoverCombat)
    {
        int required = player.CombatLevel * player.CombatLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 230, 150, 40, new Color((byte)0, (byte)0, (byte)0, (byte)210));
        Program.DrawTextUI($"XP: {player.CombatXP}/{required}", ScreenWidth - 315, ScreenHeight - 218, 20, Color.LightGray);
    }
    if (hoverDriving)
    {
        int required = player.DrivingLevel * player.DrivingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 280, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.DrivingXP}/{required}", ScreenWidth - 315, ScreenHeight - 268, 20, Color.LightGray);
    }
    if (hoverAthletics)
    {
        int required = player.AthleticsLevel * player.AthleticsLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 330, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.AthleticsXP}/{required}", ScreenWidth - 315, ScreenHeight - 318, 20, Color.LightGray);
    }
    if (hoverStrength)
    {
        int required = player.StrengthLevel * player.StrengthLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 380, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.StrengthXP}/{required}", ScreenWidth - 315, ScreenHeight - 368, 20, Color.LightGray);
    }
    if (hoverMining)
    {
        int required = player.MiningLevel * player.MiningLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 430, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.MiningXP}/{required}", ScreenWidth - 315, ScreenHeight - 418, 20, Color.LightGray);
    }
    if (hoverGambling)
    {
        int required = player.GamblingLevel * player.GamblingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 480, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.GamblingXP}/{required}", ScreenWidth - 315, ScreenHeight - 468, 20, Color.LightGray);
    }
     if (hoverRiding)
    {
        int required = player.RidingLevel * player.RidingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 530, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.RidingXP}/{required}", ScreenWidth - 315, ScreenHeight - 518, 20, Color.LightGray);
    }
     if (hoverCycling)
    {
        int required = player.CyclingLevel * player.CyclingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 580, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.CyclingXP}/{required}", ScreenWidth - 315, ScreenHeight - 568, 20, Color.LightGray);
    }
    if (hoverSwimming)
    {
        int req = player.SwimmingLevel * player.SwimmingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 630, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.SwimmingXP}/{req}", ScreenWidth - 315, ScreenHeight - 618, 18, Color.LightGray);
    }
   if (hoverDiving)
    {
        int req = player.DivingLevel * player.DivingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 180, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.DivingXP}/{req}", ScreenWidth - 475, ScreenHeight - 168, 18, Color.LightGray);
    }
   if (hoverSports)
    {
        int req = player.SportsLevel * player.SportsLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 130, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.SportsXP}/{req}", ScreenWidth - 475, ScreenHeight - 118, 18, Color.LightGray);
    }
    if (hoverRanged)
    {
        int req = player.RangedLevel * player.RangedLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 230, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.RangedXP}/{req}", ScreenWidth - 475, ScreenHeight - 218, 18, Color.LightGray);
    }
     if (hoverCooking)
    {
        int req = player.CookingLevel * player.CookingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 280, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.CookingXP}/{req}", ScreenWidth - 475, ScreenHeight - 268, 18, Color.LightGray);
    }
    if (hoverElemental)
        {
        int req = player.ElementalLevel * player.ElementalLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 330, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.ElementalXP}/{req}", ScreenWidth - 475, ScreenHeight - 318, 18, Color.LightGray);
        }
        if (hoverCards)
    {
    int req = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
    Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 380, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
    Program.DrawTextUI($"XP: {player.PlayingCardsXP}/{req}", ScreenWidth - 475, ScreenHeight - 368, 18, Color.LightGray);
    }
    if (hoverOneHand)
    {
        int req = player.OneHandMeleeLevel * player.OneHandMeleeLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 430, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.OneHandMeleeXP}/{req}", ScreenWidth - 475, ScreenHeight - 418, 18, Color.LightGray);
    }
    if (hoverTwoHand)
    {
        int req = player.TwoHandMeleeLevel * player.TwoHandMeleeLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 480, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.TwoHandMeleeXP}/{req}", ScreenWidth - 475, ScreenHeight - 468, 18, Color.LightGray);
    }
    if (hoverFarming)
    {
        int req = player.FarmingLevel * player.FarmingLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 530, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.FarmingXP}/{req}", ScreenWidth - 475, ScreenHeight - 518, 18, Color.LightGray);
    }
    if (hoverEducation)
    {
        int req = player.EducationLevel * player.EducationLevel * 50;
        Raylib.DrawRectangle(ScreenWidth - 480, ScreenHeight - 580, 150, 40, new Color((byte)0,(byte)0,(byte)0,(byte)210));
        Program.DrawTextUI($"XP: {player.EducationXP}/{req}", ScreenWidth - 475, ScreenHeight - 568, 18, Color.LightGray);
    }
    DrawSkillDetailMenu();
    }

    static void DrawSkillDetailMenu()
    {
        if (skillDetailOpen == "") return;

        int mx = ScreenWidth / 2 - 300;
        int my = ScreenHeight / 2 - 260;
        int mw = 600;
        int mh = 520;

        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)110));
        Raylib.DrawRectangle(mx, my, mw, mh, new Color((byte)10,(byte)14,(byte)22,(byte)248));
        Raylib.DrawRectangleLines(mx, my, mw, mh, Color.Gold);
        Program.DrawTextUI("Click outside to close", mx + mw - 172, my + 8, 13, Color.DarkGray);

        if (skillDetailOpen == "wc")
        {
            Color green = new Color((byte)100,(byte)210,(byte)80,(byte)255);
            Program.DrawTextUI("WOODCUTTING", mx + 20, my + 14, 26, green);

            int wcReq = player.WoodcuttingLevel * player.WoodcuttingLevel * 50;
            Program.DrawTextUI($"Level {player.WoodcuttingLevel}   XP: {player.WoodcuttingXP} / {wcReq}",
                mx + 20, my + 48, 17, Color.LightGray);

            int pbW = mw - 40;
            float wcP = Math.Clamp((float)player.WoodcuttingXP / wcReq, 0f, 1f);
            Raylib.DrawRectangle(mx + 20, my + 72, pbW, 10, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangle(mx + 20, my + 72, (int)(pbW * wcP), 10, green);
            Raylib.DrawRectangleLines(mx + 20, my + 72, pbW, 10, Color.DarkGray);

            Raylib.DrawLine(mx + 10, my + 94, mx + mw - 10, my + 94, Color.DarkGray);

            Program.DrawTextUI("Tree",       mx + 20,  my + 102, 15, Color.Gray);
            Program.DrawTextUI("Lv Req",     mx + 230, my + 102, 15, Color.Gray);
            Program.DrawTextUI("Log Type",   mx + 320, my + 102, 15, Color.Gray);
            Program.DrawTextUI("Status",     mx + 470, my + 102, 15, Color.Gray);
            Raylib.DrawLine(mx + 10, my + 120, mx + mw - 10, my + 120, new Color((byte)50,(byte)50,(byte)50,(byte)255));

            (string tree, int lvl, string log, Color col)[] trees =
            {
                ("Dead Wood",   1,  "Dead Wood",   new Color((byte)120,(byte)90,(byte)60,(byte)255)),
                ("Normal Tree", 1,  "Logs",        new Color((byte)160,(byte)130,(byte)80,(byte)255)),
                ("Birch Tree",  3,  "Birch Logs",  new Color((byte)200,(byte)200,(byte)170,(byte)255)),
                ("Oak Tree",    5,  "Oak Logs",    new Color((byte)160,(byte)110,(byte)50,(byte)255)),
                ("Pine Tree",   10, "Pine Logs",   new Color((byte)80,(byte)150,(byte)80,(byte)255)),
                ("Arctic Tree", 15, "Arctic Logs", new Color((byte)180,(byte)220,(byte)240,(byte)255)),
            };

            for (int i = 0; i < trees.Length; i++)
            {
                var (tName, tLvl, tLog, tCol) = trees[i];
                int ry = my + 128 + i * 38;
                bool unlocked = player.WoodcuttingLevel >= tLvl;

                if (unlocked)
                    Raylib.DrawRectangle(mx + 10, ry - 2, mw - 20, 34, new Color((byte)20,(byte)40,(byte)20,(byte)140));

                Color nameCol = unlocked ? tCol : new Color((byte)70,(byte)70,(byte)70,(byte)255);
                Program.DrawTextUI(tName, mx + 20, ry + 6, 17, nameCol);
                Program.DrawTextUI($"Lv {tLvl}", mx + 230, ry + 6, 17, unlocked ? Color.White : Color.DarkGray);
                Program.DrawTextUI(tLog, mx + 320, ry + 6, 15, unlocked ? Color.LightGray : Color.DarkGray);

                if (unlocked)
                    Program.DrawTextUI("UNLOCKED", mx + 470, ry + 6, 15, new Color((byte)80,(byte)220,(byte)80,(byte)255));
                else
                    Program.DrawTextUI($"Need Lv {tLvl}", mx + 470, ry + 6, 14, new Color((byte)160,(byte)60,(byte)60,(byte)255));
            }
        }

        if (skillDetailOpen == "farming")
        {
            Color green = new Color((byte)90,(byte)170,(byte)60,(byte)255);
            Program.DrawTextUI("FARMING", mx + 20, my + 14, 26, green);
            int req = player.FarmingLevel * player.FarmingLevel * 50;
            Program.DrawTextUI($"Level {player.FarmingLevel}   XP: {player.FarmingXP} / {req}", mx + 20, my + 50, 18, Color.LightGray);
            Program.DrawTextUI("SEEDS UNLOCKED", mx + 20, my + 90, 20, Color.Gold);

            int ly = my + 124;
            foreach (var kv in seedUnlockLevel)
            {
                bool unlocked = player.FarmingLevel >= kv.Value;
                Program.DrawTextUI(kv.Key, mx + 30, ly, 18, unlocked ? Color.White : Color.DarkGray);
                Program.DrawTextUI(unlocked ? "UNLOCKED" : $"Requires Lv {kv.Value}", mx + 260, ly, 16, unlocked ? Color.Green : Color.Red);
                ly += 30;
            }
        }
        else if (skillDetailOpen == "cards")
        {
            Program.DrawTextUI("PLAYING CARDS", mx + 20, my + 14, 26, Color.Gold);

            int cReq = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
            Program.DrawTextUI($"Level {player.PlayingCardsLevel}   XP: {player.PlayingCardsXP} / {cReq}",
                mx + 20, my + 48, 17, Color.LightGray);

            int pbW = mw - 40;
            float cP = Math.Clamp((float)player.PlayingCardsXP / cReq, 0f, 1f);
            Raylib.DrawRectangle(mx + 20, my + 72, pbW, 10, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangle(mx + 20, my + 72, (int)(pbW * cP), 10, Color.Gold);
            Raylib.DrawRectangleLines(mx + 20, my + 72, pbW, 10, Color.DarkGray);

            Raylib.DrawLine(mx + 10, my + 94, mx + mw - 10, my + 94, Color.DarkGray);

            // Ratings
            Program.DrawTextUI("YOUR RATINGS", mx + 20, my + 102, 18, Color.White);

            (string game, int rating)[] games =
            {
                ("Euchre",   player.EuchreRating),
                ("500",      player.FiveHundredRating),
                ("Sequence", player.SequenceRating),
            };

            for (int i = 0; i < games.Length; i++)
            {
                var (gName, gRating) = games[i];
                int ry = my + 130 + i * 62;

                Raylib.DrawRectangle(mx + 10, ry, mw - 20, 54, new Color((byte)15,(byte)15,(byte)30,(byte)200));
                Raylib.DrawRectangleLines(mx + 10, ry, mw - 20, 54, new Color((byte)55,(byte)55,(byte)55,(byte)255));

                string rank = gRating >= 1800 ? "Grand Master" :
                              gRating >= 1500 ? "Master" :
                              gRating >= 1200 ? "Expert" :
                              gRating >= 800  ? "Intermediate" : "Beginner";

                Color rankCol = gRating >= 1800 ? Color.Gold :
                                gRating >= 1500 ? new Color((byte)180,(byte)100,(byte)255,(byte)255) :
                                gRating >= 1200 ? Color.SkyBlue :
                                gRating >= 800  ? Color.LightGray : Color.DarkGray;

                Program.DrawTextUI(gName, mx + 20, ry + 6, 18, Color.White);
                Program.DrawTextUI($"Rating: {gRating}", mx + 20, ry + 30, 14, Color.LightGray);
                Program.DrawTextUI(rank, mx + 290, ry + 16, 20, rankCol);

                float rP = Math.Clamp(gRating / 2000f, 0f, 1f);
                Raylib.DrawRectangle(mx + 430, ry + 20, 140, 8, new Color((byte)30,(byte)30,(byte)30,(byte)255));
                Raylib.DrawRectangle(mx + 430, ry + 20, (int)(140 * rP), 8, rankCol);
            }

            Raylib.DrawLine(mx + 10, my + 322, mx + mw - 10, my + 322, Color.DarkGray);
            Program.DrawTextUI("LEVEL PERKS", mx + 20, my + 330, 18, Color.White);

            (int lvl, string perk)[] perks =
            {
                (1,  "Access to Euchre"),
                (10, "Access to 500"),
                (20, "Access to Sequence"),
                (30, "Improved AI tells in bidding"),
                (50, "Bonus XP on wins"),
                (75, "Legendary Card Player title"),
            };

            for (int i = 0; i < perks.Length; i++)
            {
                var (pLvl, pText) = perks[i];
                bool has = player.PlayingCardsLevel >= pLvl;
                Color pc = has ? new Color((byte)80,(byte)220,(byte)80,(byte)255) : new Color((byte)90,(byte)90,(byte)90,(byte)255);
                string prefix = has ? "✓" : $"Lv {pLvl}";
                Program.DrawTextUI($"{prefix}  {pText}", mx + 20, my + 356 + i * 22, 15, pc);
            }
        }
        else if (skillDetailOpen == "education")
        {
            Program.DrawTextUI("EDUCATION", mx + 20, my + 20, 28, new Color((byte)90,(byte)140,(byte)230,(byte)255));
            Program.DrawTextUI($"Level {player.EducationLevel}", mx + 20, my + 60, 22, Color.White);
            int req = player.EducationLevel * player.EducationLevel * 50;
            Program.DrawTextUI($"XP: {player.EducationXP}/{req}", mx + 20, my + 90, 18, Color.LightGray);

            Program.DrawTextUI("SUBJECT RATINGS", mx + 20, my + 130, 18, Color.White);
            int mr = player.MathsRating;
            string mRank = mr >= 1800 ? "Grand Master" : mr >= 1500 ? "Master" :
                           mr >= 1200 ? "Expert" : mr >= 800 ? "Intermediate" : "Beginner";
            Color mCol = mr >= 1800 ? Color.Gold :
                         mr >= 1500 ? new Color((byte)180,(byte)100,(byte)255,(byte)255) :
                         mr >= 1200 ? Color.SkyBlue :
                         mr >= 800  ? Color.LightGray : Color.DarkGray;
            int ry = my + 160;
            Raylib.DrawRectangle(mx + 10, ry, mw - 20, 54, new Color((byte)15,(byte)15,(byte)30,(byte)200));
            Raylib.DrawRectangleLines(mx + 10, ry, mw - 20, 54, new Color((byte)55,(byte)55,(byte)55,(byte)255));
            Program.DrawTextUI("Maths", mx + 20, ry + 6, 18, Color.White);
            Program.DrawTextUI($"Rating: {mr}", mx + 20, ry + 30, 14, Color.LightGray);
            Program.DrawTextUI(mRank, mx + 290, ry + 16, 20, mCol);
            float mP = Math.Clamp(mr / 2000f, 0f, 1f);
            Raylib.DrawRectangle(mx + 430, ry + 20, 140, 8, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangle(mx + 430, ry + 20, (int)(140 * mP), 8, mCol);
        }
        
        if (skillDetailOpen == "fishing")
            DrawSkillPerksPanel("Fishing", "FISHING", player.FishingLevel, player.FishingXP,
                new Color((byte)0,(byte)150,(byte)255,(byte)255));
        else if (skillDetailOpen == "combat")
            DrawSkillPerksPanel("Combat", "COMBAT", player.CombatLevel, player.CombatXP,
                new Color((byte)220,(byte)60,(byte)60,(byte)255));
        else if (skillDetailOpen == "mining")
            DrawSkillPerksPanel("Mining", "MINING", player.MiningLevel, player.MiningXP,
                new Color((byte)180,(byte)160,(byte)120,(byte)255));
        else if (skillDetailOpen == "cooking")
            DrawSkillPerksPanel("Cooking", "COOKING", player.CookingLevel, player.CookingXP,
                new Color((byte)240,(byte)160,(byte)40,(byte)255));
        else if (skillDetailOpen == "strength")
            DrawSkillPerksPanel("Strength", "STRENGTH", player.StrengthLevel, player.StrengthXP,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
        else if (skillDetailOpen == "athletics")
            DrawSkillPerksPanel("Athletics", "ATHLETICS", player.AthleticsLevel, player.AthleticsXP,
                new Color((byte)80,(byte)200,(byte)120,(byte)255));
        else if (skillDetailOpen == "driving")
            DrawSkillPerksPanel("Driving", "DRIVING", player.DrivingLevel, player.DrivingXP,
                new Color((byte)100,(byte)160,(byte)220,(byte)255));
        else if (skillDetailOpen == "ranged")
            DrawSkillPerksPanel("Ranged", "RANGED", player.RangedLevel, player.RangedXP,
                new Color((byte)200,(byte)150,(byte)50,(byte)255));
        else if (skillDetailOpen == "riding")
            DrawSkillPerksPanel("Riding", "RIDING", player.RidingLevel, player.RidingXP,
                new Color((byte)180,(byte)120,(byte)60,(byte)255));
        else if (skillDetailOpen == "swimming")
            DrawSkillPerksPanel("Swimming", "SWIMMING", player.SwimmingLevel, player.SwimmingXP,
                new Color((byte)60,(byte)180,(byte)220,(byte)255));
        else if (skillDetailOpen == "gambling")
            DrawSkillPerksPanel("Gambling", "GAMBLING", player.GamblingLevel, player.GamblingXP,
                new Color((byte)220,(byte)180,(byte)40,(byte)255));
    }

    static void DrawSkillPerksPanel(string skillKey, string title, int playerLevel, int playerXP, Color accent)
{
    int mx = ScreenWidth / 2 - 300;
    int my = ScreenHeight / 2 - 260;
    int mw = 600;
    int mh = 520;

    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)110));
    Raylib.DrawRectangle(mx, my, mw, mh, new Color((byte)10,(byte)14,(byte)22,(byte)248));
    Raylib.DrawRectangleLines(mx, my, mw, mh, Color.Gold);
    Program.DrawTextUI("Click outside to close", mx + mw - 172, my + 8, 13, Color.DarkGray);

    Program.DrawTextUI(title, mx + 20, my + 14, 26, accent);

    int req = playerLevel * playerLevel * 50;
    Program.DrawTextUI($"Level {playerLevel}   XP: {playerXP} / {req}", mx + 20, my + 48, 17, Color.LightGray);

    int pbW = mw - 40;
    float pct = Math.Clamp((float)playerXP / Math.Max(1, req), 0f, 1f);
    Raylib.DrawRectangle(mx + 20, my + 72, pbW, 10, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(mx + 20, my + 72, (int)(pbW * pct), 10, accent);
    Raylib.DrawRectangleLines(mx + 20, my + 72, pbW, 10, Color.DarkGray);

    Raylib.DrawLine(mx + 10, my + 94, mx + mw - 10, my + 94, Color.DarkGray);

    Program.DrawTextUI("Lv",   mx + 20,  my + 102, 15, Color.Gray);
    Program.DrawTextUI("Perk", mx + 70,  my + 102, 15, Color.Gray);
    Program.DrawTextUI("Effect",mx + 260, my + 102, 15, Color.Gray);
    Program.DrawTextUI("Status",mx + 490, my + 102, 15, Color.Gray);
    Raylib.DrawLine(mx + 10, my + 120, mx + mw - 10, my + 120, new Color((byte)50,(byte)50,(byte)50,(byte)255));

    if (!skillPerks.TryGetValue(skillKey, out var perks)) return;

    for (int i = 0; i < perks.Length; i++)
    {
        var p = perks[i];
        int ry = my + 128 + i * 50;
        bool unlocked = playerLevel >= p.Level;

        if (unlocked)
            Raylib.DrawRectangle(mx + 10, ry - 2, mw - 20, 46, new Color((byte)20,(byte)40,(byte)20,(byte)140));

        Color nameCol = unlocked ? accent : new Color((byte)70,(byte)70,(byte)70,(byte)255);
        Program.DrawTextUI($"{p.Level}", mx + 20, ry + 6, 17, unlocked ? Color.White : Color.DarkGray);
        Program.DrawTextUI(p.Name, mx + 70, ry + 4, 17, nameCol);
        Program.DrawTextUI(p.Description, mx + 70, ry + 24, 13, unlocked ? Color.LightGray : new Color((byte)60,(byte)60,(byte)60,(byte)255));

        if (unlocked)
            Program.DrawTextUI("UNLOCKED", mx + 490, ry + 8, 14, new Color((byte)80,(byte)220,(byte)80,(byte)255));
        else
            Program.DrawTextUI($"Need Lv {p.Level}", mx + 490, ry + 8, 13, new Color((byte)160,(byte)60,(byte)60,(byte)255));
    }
}

    static void DrawMinimap()
{
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, minimapSize, new Color((byte)0,(byte)0,(byte)0,(byte)180));
    Raylib.BeginScissorMode(minimapX, minimapY, minimapSize, minimapSize);

    int cx = minimapX + minimapSize / 2;
    int cy = minimapY + minimapSize / 2;

    // 1. Base grasslands
    Raylib.DrawRectangle(minimapX, minimapY, minimapSize, minimapSize, new Color((byte)140, (byte)195, (byte)80, (byte)255));;

    // 2. Forest 
    int forL = cx + (int)((-250000 - player.Position.X) * minimapScale);
    int forR = cx + (int)((-80000 - player.Position.X) * minimapScale);
    int forT = cy + (int)((80000 - player.Position.Y) * minimapScale);
    int forB = cy + (int)((250000  - player.Position.Y) * minimapScale);
    int frX  = Math.Clamp(forL, minimapX, minimapX + minimapSize);
    int frX2 = Math.Clamp(forR, minimapX, minimapX + minimapSize);
    int frY  = Math.Clamp(forT, minimapY, minimapY + minimapSize);
    int frY2 = Math.Clamp(forB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(frX, frY, frX2 - frX, frY2 - frY,
    new Color((byte)40,(byte)100,(byte)40,(byte)220));

    // 4. Swamp (X -55000 to -30000, middle strip)
    int swampL = cx + (int)((-250000 - player.Position.X) * minimapScale);
    int swampR = cx + (int)((-80000 - player.Position.X) * minimapScale);
    int swampT = cy + (int)((-80000 - player.Position.Y) * minimapScale);
    int swampB = cy + (int)((80000  - player.Position.Y) * minimapScale);
    int swX  = Math.Clamp(swampL, minimapX, minimapX + minimapSize);
    int swX2 = Math.Clamp(swampR, minimapX, minimapX + minimapSize);
    int swY  = Math.Clamp(swampT, minimapY, minimapY + minimapSize);
    int swY2 = Math.Clamp(swampB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(swX, swY, swX2 - swX, swY2 - swY,
        new Color((byte)55,(byte)75,(byte)35,(byte)220));

    // 5. Snow (X -30000 to -10000, middle strip)
    int snowL = cx + (int)((-250000 - player.Position.X) * minimapScale);
    int snowR = cx + (int)((-80000- player.Position.X) * minimapScale);
    int snowT = cy + (int)((-250000 - player.Position.Y) * minimapScale);
    int snowB = cy + (int)((-80000  - player.Position.Y) * minimapScale);
    int snX  = Math.Clamp(snowL, minimapX, minimapX + minimapSize);
    int snX2 = Math.Clamp(snowR, minimapX, minimapX + minimapSize);
    int snY  = Math.Clamp(snowT, minimapY, minimapY + minimapSize);
    int snY2 = Math.Clamp(snowB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(snX, snY, snX2 - snX, snY2 - snY,
        new Color((byte)220,(byte)235,(byte)255,(byte)220));

    // 6. Desert (X 8000 to 22000, middle strip)
    int desL = cx + (int)((80000  - player.Position.X) * minimapScale);
    int desR = cx + (int)((250000 - player.Position.X) * minimapScale);
    int desT = cy + (int)((-80000 - player.Position.Y) * minimapScale);
    int desB = cy + (int)((80000  - player.Position.Y) * minimapScale);
    int dX  = Math.Clamp(desL, minimapX, minimapX + minimapSize);
    int dX2 = Math.Clamp(desR, minimapX, minimapX + minimapSize);
    int dY  = Math.Clamp(desT, minimapY, minimapY + minimapSize);
    int dY2 = Math.Clamp(desB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(dX, dY, dX2 - dX, dY2 - dY,
        new Color((byte)210,(byte)180,(byte)100,(byte)220));

    // 7. Beach (X 22000 to 28000, full height)
    int bchL = cx + (int)((-80000  - player.Position.X) * minimapScale);
    int bchR = cx + (int)((250000 - player.Position.X) * minimapScale);
    int bchT = cy + (int)((80000 - player.Position.Y) * minimapScale);
    int bchB = cy + (int)((115000  - player.Position.Y) * minimapScale);
    int bX  = Math.Clamp(bchL, minimapX, minimapX + minimapSize);
    int bX2 = Math.Clamp(bchR, minimapX, minimapX + minimapSize);
    int bY  = Math.Clamp(bchT, minimapY, minimapY + minimapSize);
    int bY2 = Math.Clamp(bchB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(bX, bY, bX2 - bX, bY2 - bY,
        new Color((byte)240,(byte)220,(byte)150,(byte)220));

    // 8. Ocean (X 28000+, full height)
    int ocnL = cx + (int)((-80000  - player.Position.X) * minimapScale);
    int ocnR = cx + (int)((250000 - player.Position.X) * minimapScale);
    int ocnT = cy + (int)((115000 - player.Position.Y) * minimapScale);
    int ocnB = cy + (int)((250000  - player.Position.Y) * minimapScale);
    int oX  = Math.Clamp(ocnL, minimapX, minimapX + minimapSize);
    int oX2 = Math.Clamp(ocnR, minimapX, minimapX + minimapSize);
    int oY  = Math.Clamp(ocnT, minimapY, minimapY + minimapSize);
    int oY2 = Math.Clamp(ocnB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(oX, oY, oX2 - oX, oY2 - oY,
        new Color((byte)30,(byte)100,(byte)180,(byte)220));

    // 9. Volcano (X 22000+, Y above -12000)
    int volL = cx + (int)((-80000 - player.Position.X) * minimapScale);
    int volR = cx + (int)((80000 - player.Position.X) * minimapScale);
    int volT = cy + (int)((-250000 - player.Position.Y) * minimapScale);
    int volB = cy + (int)((-80000 - player.Position.Y) * minimapScale);
    int vX = Math.Clamp(volL, minimapX, minimapX + minimapSize);
    int vX2 = Math.Clamp(volR, minimapX, minimapX + minimapSize);
    int vY  = Math.Clamp(volT, minimapY, minimapY + minimapSize);
    int vY2 = Math.Clamp(volB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(vX, vY, vX2 - vX, vY2 - vY,
        new Color((byte)40,(byte)20,(byte)10,(byte)220));

    // 10. Mountains (X -30000 to -10000, Y -15000 to 23000)
    int mtL = cx + (int)((-80000 - player.Position.X) * minimapScale);
    int mtR = cx + (int)((80000 - player.Position.X) * minimapScale);
    int mtT = cy + (int)((-250000 - player.Position.Y) * minimapScale);
    int mtB = cy + (int)((-80000  - player.Position.Y) * minimapScale);
    int mX  = Math.Clamp(mtL, minimapX, minimapX + minimapSize);
    int mX2 = Math.Clamp(mtR, minimapX, minimapX + minimapSize);
    int mY  = Math.Clamp(mtT, minimapY, minimapY + minimapSize);
    int mY2 = Math.Clamp(mtB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(mX, mY, mX2 - mX, mY2 - mY,
        new Color((byte)100,(byte)95,(byte)90,(byte)220));

    // 11. Safe zone
    int szX  = cx + (int)((-3000 - player.Position.X) * minimapScale);
    int szY  = cy + (int)((-1500 - player.Position.Y) * minimapScale);
    int szX2 = cx + (int)((4000  - player.Position.X) * minimapScale);
    int szY2 = cy + (int)((2500  - player.Position.Y) * minimapScale);
    int csX  = Math.Clamp(szX,  minimapX, minimapX + minimapSize);
    int csY  = Math.Clamp(szY,  minimapY, minimapY + minimapSize);
    int csX2 = Math.Clamp(szX2, minimapX, minimapX + minimapSize);
    int csY2 = Math.Clamp(szY2, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(csX, csY, csX2 - csX, csY2 - csY, new Color((byte)90,(byte)170,(byte)90,(byte)180));

    // 12. Farm zone
    int farmX1 = cx + (int)((-3000  - player.Position.X) * minimapScale);
    int farmX2 = cx + (int)((0      - player.Position.X) * minimapScale);
    int farmY1 = cy + (int)((-10000 - player.Position.Y) * minimapScale);
    int farmY2 = cy + (int)((-6000  - player.Position.Y) * minimapScale);
    int fX  = Math.Clamp(farmX1, minimapX, minimapX + minimapSize);
    int fY  = Math.Clamp(farmY1, minimapY, minimapY + minimapSize);
    int fX2 = Math.Clamp(farmX2, minimapX, minimapX + minimapSize);
    int fY2 = Math.Clamp(farmY2, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(fX, fY, fX2 - fX, fY2 - fY,
        new Color((byte)139,(byte)90,(byte)43,(byte)220));

    // 13. Hamiltron City (X 11800–18200, Y 3000–8200)
    int htL = cx + (int)((11800 - player.Position.X) * minimapScale);
    int htR = cx + (int)((18200 - player.Position.X) * minimapScale);
    int htT = cy + (int)((3000  - player.Position.Y) * minimapScale);
    int htB = cy + (int)((8200  - player.Position.Y) * minimapScale);
    int hcX  = Math.Clamp(htL, minimapX, minimapX + minimapSize);
    int hcY  = Math.Clamp(htT, minimapY, minimapY + minimapSize);
    int hcX2 = Math.Clamp(htR, minimapX, minimapX + minimapSize);
    int hcY2 = Math.Clamp(htB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(hcX, hcY, hcX2 - hcX, hcY2 - hcY, new Color((byte)160,(byte)160,(byte)175,(byte)220));

    // 14. Rotoaira (X -18000 to -13800, Y 3200–6200)
    int rtL = cx + (int)((-18000 - player.Position.X) * minimapScale);
    int rtR = cx + (int)((-13800 - player.Position.X) * minimapScale);
    int rtT = cy + (int)((3200   - player.Position.Y) * minimapScale);
    int rtB = cy + (int)((6200   - player.Position.Y) * minimapScale);
    int rcX  = Math.Clamp(rtL, minimapX, minimapX + minimapSize);
    int rcY  = Math.Clamp(rtT, minimapY, minimapY + minimapSize);
    int rcX2 = Math.Clamp(rtR, minimapX, minimapX + minimapSize);
    int rcY2 = Math.Clamp(rtB, minimapY, minimapY + minimapSize);
    Raylib.DrawRectangle(rcX, rcY, rcX2 - rcX, rcY2 - rcY, new Color((byte)160,(byte)160,(byte)175,(byte)220));

    // Buildings — dot only, no label
    foreach (Building building in buildings)
    {
        int bx = cx + (int)MathF.Round((building.Bounds.X - player.Position.X) * minimapScale);
        int by = cy + (int)MathF.Round((building.Bounds.Y - player.Position.Y) * minimapScale);
        if (bx >= minimapX && bx <= minimapX + minimapSize && by >= minimapY && by <= minimapY + minimapSize)
            Raylib.DrawRectangle(bx - 1, by - 1, 3, 3, Color.Yellow);
    }

    RoadManager.DrawOnMinimap(cx, cy, minimapScale, player.Position);

    // Roads drawn on top of buildings
    Color roadCol = new Color((byte)75,(byte)75,(byte)75,(byte)255);
    int mmRoadT  = cy + (int)((550 - player.Position.Y) * minimapScale);
    int mmHwyX   = cx + (int)((200 - player.Position.X) * minimapScale);
    int mmSideY  = cy + (int)((200 - player.Position.Y) * minimapScale);

    // ── MINIMAP FOG OF WAR ──
    {
        Color mmFog = new Color((byte)0,(byte)0,(byte)0,(byte)180);
        float mmCellW = FogCellSize * minimapScale;
        if (mmCellW >= 0.5f)
        {
            int pcx = (int)((player.Position.X - FogOriginX) / FogCellSize);
            int pcy = (int)((player.Position.Y - FogOriginY) / FogCellSize);
            int halfView = (int)(minimapSize / 2f / mmCellW) + 2;
            int fxMin = Math.Max(0, pcx - halfView);
            int fxMax = Math.Min(FogCols - 1, pcx + halfView);
            int fyMin = Math.Max(0, pcy - halfView);
            int fyMax = Math.Min(FogRows - 1, pcy + halfView);
            for (int fy = fyMin; fy <= fyMax; fy++)
                for (int fx = fxMin; fx <= fxMax; fx++)
                {
                    if (fogRevealed[fy * FogCols + fx]) continue;
                    float wx = FogOriginX + fx * FogCellSize;
                    float wy = FogOriginY + fy * FogCellSize;
                    int sx = cx + (int)((wx - player.Position.X) * minimapScale);
                    int sy = cy + (int)((wy - player.Position.Y) * minimapScale);
                    Raylib.DrawRectangle(sx, sy, Math.Max(1, (int)mmCellW + 1), Math.Max(1, (int)mmCellW + 1), mmFog);
                }
        }
    }

    Raylib.DrawRectangleLines(minimapX, minimapY, minimapSize, minimapSize, Color.White);
    Raylib.DrawCircle(cx, cy, 4, Color.White);
    // ── TUTORIAL GUIDE MARKER ──
    if (tutorialActive && tutorialStep < tutorialTasks.Count)
    {
        Vector2 gp = tutorialTasks[tutorialStep].GuidePos;
        int gx = cx + (int)((gp.X - player.Position.X) * minimapScale);
        int gy = cy + (int)((gp.Y - player.Position.Y) * minimapScale);

        bool blinkOn = ((int)(Raylib.GetTime() * 3) % 2) == 0;
        if (blinkOn)
        {
            // clamp to the minimap edge so it's visible even when off-map (edge indicator)
            int mgx = Math.Clamp(gx, minimapX + 4, minimapX + minimapSize - 4);
            int mgy = Math.Clamp(gy, minimapY + 4, minimapY + minimapSize - 4);
            Raylib.DrawCircle(mgx, mgy, 5, Color.Gold);
            Raylib.DrawCircleLines(mgx, mgy, 5, Color.Black);
        }
    }

    foreach (var f in placedFlags)
    {
        int x = (int)f.X, y = (int)f.Y;
        Raylib.DrawRectangle(x - 2, y - 34, 4, 40, new Color((byte)110,(byte)80,(byte)50,(byte)255)); // pole
        Raylib.DrawTriangle(new Vector2(x + 2, y - 34), new Vector2(x + 2, y - 18),
            new Vector2(x + 22, y - 26), Color.Red);                                                  // flag
    }

    // ── QUEST WAYPOINT MARKERS ──
    DrawQuestWaypoints(cx, cy, minimapScale);

    Raylib.EndScissorMode();

    Raylib.DrawRectangleLines(minimapX, minimapY, minimapSize, minimapSize, Color.White);

    // biome label below minimap
    Color biomeCol = currentBiome switch
    {
        "SNOW ZONE"  => new Color((byte)150, (byte)200, (byte)255, (byte)200),
        "DESERT"     => new Color((byte)210, (byte)170, (byte)60,  (byte)200),
        "FOREST"     => new Color((byte)80,  (byte)180, (byte)80,  (byte)200),
        "SAFE ZONE"  => new Color((byte)100, (byte)200, (byte)100, (byte)200),
        "BEACH"      => new Color((byte)220, (byte)200, (byte)120, (byte)200),
        "VOLCANO"    => new Color((byte)220, (byte)100, (byte)40,  (byte)200),
        _            => new Color((byte)200, (byte)200, (byte)200, (byte)200),
    };
    string biomeLabel = currentBiome ?? "UNKNOWN";
    int labelW = Program.MeasureTextUI(biomeLabel, 14);
    int labelX = minimapX + minimapSize / 2 - labelW / 2;
    int labelY = minimapY + minimapSize + 6;
    Raylib.DrawRectangle(labelX - 6, labelY - 2, labelW + 12, 20,
        new Color((byte)0, (byte)0, (byte)0, (byte)160));
    Program.DrawTextUI(biomeLabel, labelX, labelY, 14, biomeCol);
}


        static void DrawHUD()
        {
            DrawSkillsUI();
            DrawQuestsUI();
            DrawDailyChallengeHud();
            DrawAchievementsUI();
            DrawFishingUI();
            DrawTutorialHUD();
            DrawDirectionHUD();
            DrawDirection2HUD();
            DrawToolbarTooltip();
            DrawPerkFlash();
            DrawDropConfirm();
            DrawDropQuantity();
            DrawSkillCheatPanel();
            DrawIncubatorMenu();
            DrawPetStorageMenu();
            multiplayer.DrawStatusOverlay();
            multiplayer.DrawChat();

            string coordText = $"X: {(int)player.Position.X}  Y: {(int)player.Position.Y}";
            int coordWidth = Program.MeasureTextUI(coordText, 14);
            int coordX = ScreenWidth - coordWidth - 20;
            int coordY = 8 + 54 + 6 + 56; 
            Raylib.DrawRectangle(coordX, coordY, coordWidth + 12, 22, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            Program.DrawTextUI(coordText, coordX + 6, coordY + 3, 14, Color.LightGray);

            // ── combo counter ──
            if (comboCount > 1)
            {
                float t = comboTimer / comboWindow;                 // 1 → 0
                int fontSize = 40 + Math.Min(comboCount, 20);       // grows with combo, capped
                string comboStr = $"{comboCount}x COMBO";
                int cw = Program.MeasureTextUI(comboStr, fontSize);
                int cx = ScreenWidth - cw - 40;
                int cy = 90;
                // color shifts white → orange → red as combo climbs
                Color comboCol = comboCount >= 15 ? Color.Red
                               : comboCount >= 8  ? Color.Orange
                               : Color.White;
                byte alpha = (byte)(180 + 75 * t);
                Program.DrawTextUI(comboStr, cx + 2, cy + 2, fontSize, new Color((byte)0,(byte)0,(byte)0,(byte)alpha)); // shadow
                Program.DrawTextUI(comboStr, cx, cy, fontSize,
                    new Color(comboCol.R, comboCol.G, comboCol.B, alpha));
                // draining timer bar under it
                Raylib.DrawRectangle(cx, cy + fontSize + 4, (int)(cw * t), 5, comboCol);
            }

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.Driving)
                {
                    Color roadColor = vehicle.OnRoad ? Color.Green : Color.Orange;
                    string roadText = vehicle.OnRoad ? "ON ROAD" : "OFF ROAD";
                    Program.DrawTextUI(roadText, ScreenWidth / 2 - 40, ScreenHeight - 60, 22, roadColor);

                    // fuel bar
                    int fbWidth = 200;
                    int fbX = ScreenWidth / 2 - fbWidth / 2;
                    int fbY = ScreenHeight - 100;
                    float fuelPercent = vehicle.Fuel / vehicle.MaxFuel;
                    Color fuelColor = fuelPercent > 0.5f ? Color.Green :
                                    fuelPercent > 0.25f ? Color.Orange : Color.Red;

                    Raylib.DrawRectangle(fbX - 60, fbY - 4, 55, 28, new Color((byte)0,(byte)0,(byte)0,(byte)180));
                    Program.DrawTextUI("FUEL", fbX - 55, fbY, 20, Color.LightGray);
                    Raylib.DrawRectangle(fbX, fbY, fbWidth, 24, new Color((byte)40,(byte)40,(byte)40,(byte)220));
                    Raylib.DrawRectangle(fbX, fbY, (int)(fbWidth * fuelPercent), 24, fuelColor);
                    Raylib.DrawRectangleLines(fbX, fbY, fbWidth, 24, Color.White);

                    if (vehicle.Fuel <= 0)
                    {
                        Program.DrawTextUI("OUT OF FUEL!", ScreenWidth / 2 - 70, fbY - 30, 24, Color.Red);
                    }
                    else if (vehicle.NeedsPayment)
                    {
                        Program.DrawTextUI("Go inside to pay for fuel!", ScreenWidth / 2 - 120, fbY - 30, 22, Color.Yellow);
                    }
                }
            }
if (busMenuOpen && busOperating)
{
    Color busBlue = new Color((byte)20,(byte)60,(byte)160,(byte)255);
    int px = ScreenWidth/2 - 220, py = 80;
    Raylib.DrawRectangle(px, py, 440, 80 + busStops.Length * 52, new Color((byte)10,(byte)15,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 440, 80 + busStops.Length * 52, busBlue);
    Program.DrawTextUI("BUS ROUTES — $2 each", px + 60, py + 12, 22, Color.White);
    Program.DrawTextUI($"Currently at: {busStops[busCurrentStop].Name}", px + 40, py + 40, 16, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < busStops.Length; i++)
    {
        if (i == busCurrentStop) continue; // can't travel to current stop
        Rectangle btn = new Rectangle(px + 20, py + 64 + i * 52, 400, 44);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);
        Raylib.DrawRectangleRec(btn, new Color((byte)20,(byte)30,(byte)60,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : new Color((byte)40,(byte)60,(byte)100,(byte)255));
        Program.DrawTextUI(busStops[i].Name, px + 40, py + 78 + i * 52, 20, hover ? Color.Gold : Color.White);
        Program.DrawTextUI("$2", px + 380, py + 78 + i * 52, 20, Color.Gold);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (player.Money >= 2)
            {
                player.Money -= 2;
                player.Position = busStops[i].WorldPos + new Vector2(60, 0);
                busMenuOpen = false;
                ShowNotification($"Hopped off at {busStops[i].Name} bro!");
                lastZoneMusic = default;
                CheckZoneMusic();
            }
            else { ShowNotification("Not enough cash for the bus bro!"); busMenuOpen = false; }
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
            Rideable.RideableType.Donkey       => "DONKEY",
            _                                  => "RIDEABLE"
        };
        break;
    }
}

DrawSmoothedHealthBar();

int hbWidth = 300;
int hbX = ScreenWidth / 2 - hbWidth / 2;

var mount = rideables.FirstOrDefault(r => r.Riding);
if (mount != null && mount.Type != Rideable.RideableType.MountainBike && mount.Type != Rideable.RideableType.BMX)
{
    Raylib.DrawRectangle(hbX, 68, hbWidth, 12, new Color((byte)40,(byte)40,(byte)40,(byte)220));
    Raylib.DrawRectangle(hbX, 68, (int)(hbWidth * mount.Stamina / mount.MaxStamina), 12, Color.Yellow);
    Raylib.DrawRectangleLines(hbX, 68, hbWidth, 12, Color.White);
    Program.DrawTextUI($"{mount.Type} STAMINA — feed with R", hbX + 4, 82, 18, Color.LightGray);
}

            if (currentScene == SceneState.World)
    DrawToolbar();
    DrawCombatColumn();

    if (camera.Zoom != 1f)
{
    Program.DrawTextUI($"Zoom: {camera.Zoom:F1}x  (Alt+Scroll to adjust)", 20, ScreenHeight - 55, 18, Color.Gold);
}
if (showControlsHud)
{
           Raylib.DrawRectangle(0, ScreenHeight - 34, ScreenWidth, 34, new Color((byte)0, (byte)0, (byte)0, (byte)170));
            Program.DrawTextUI("SPACE = Action | TAB = Inventory | E = Enter Building | F = Drive Vehicle | G = Equipment | Esc = Options | F5 = Save", 20, ScreenHeight - 28, 20, Color.White);
}
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

    string biomeMsg = $"ENTERING {currentBiome}";
    int textWidth = Program.MeasureTextUI(biomeMsg, 36);
    int bx = ScreenWidth / 2 - textWidth / 2;
    
    byte boxA = (byte)(200 * Math.Min(1f, biomeMessageTimer));
    Raylib.DrawRectangle(bx - 20, 274, textWidth + 40, 52, new Color((byte)0,(byte)0,(byte)0,boxA));
    Raylib.DrawRectangleLines(bx - 20, 274, textWidth + 40, 52, biomeColor);
    Program.DrawTextUI(biomeMsg, bx, 280, 36, biomeColor);

}
           
 DrawInventoryUI();  

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
    int textWidth = Program.MeasureTextUI(drunkText, 26);
    Raylib.DrawRectangle(ScreenWidth / 2 - textWidth / 2 - 10, 42, textWidth + 20, 34, new Color((byte)0, (byte)0, (byte)0, (byte)180));
    Program.DrawTextUI(drunkText, ScreenWidth / 2 - textWidth / 2, 46, 26, new Color((byte)255, (byte)150, (byte)50, (byte)255));

    // drunk timer bar
    int dbWidth = 300;
    int dbX = ScreenWidth / 2 - dbWidth / 2;
    int dbY = 78;
    Raylib.DrawRectangle(dbX, dbY, dbWidth, 12, new Color((byte)40, (byte)40, (byte)40, (byte)220));
    Raylib.DrawRectangle(dbX, dbY, (int)(dbWidth * drunkDayPercent), 12, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    Raylib.DrawRectangleLines(dbX, dbY, dbWidth, 12, Color.White);
    Program.DrawTextUI("DRUNK", dbX - 55, dbY - 2, 16, new Color((byte)255, (byte)150, (byte)50, (byte)255));
    Program.DrawTextUI("Sober up: Hospital / Marae / Sleep", dbX - 10, dbY + 16, 16, Color.LightGray);
}

            DrawCalendarHUD();
            DrawCashHUD();
            DrawMinimap();
            DrawPlayerMenu();

        }

        static void DrawCalendarHUD()
        {
            string weekday = dayNames[dayOfWeek];
            int weekNum = GetWeekOfMonth();
            string month = GetMonthString();
            string season = GetSeasonString();
            string timeStr = GetTimeString();
            string line1 = $"{weekday} week {weekNum}   {timeStr}";

            Color seasonCol = season switch
            {
                "Summer" => new Color((byte)255,(byte)200,(byte)40,(byte)255),
                "Autumn" => new Color((byte)220,(byte)140,(byte)40,(byte)255),
                "Winter" => new Color((byte)140,(byte)180,(byte)220,(byte)255),
                "Spring" => new Color((byte)100,(byte)220,(byte)80,(byte)255),
                _ => Color.LightGray,
            };

            // gather in-season crops for the hint line
            var inSeason = cropSeasons
                .Where(kv => kv.Value.Contains(season))
                .Select(kv => kv.Key);
            string cropHint = string.Join(", ", inSeason);

            int boxW = 320, boxH = 70;
            int boxX = ScreenWidth - boxW - 12, boxY = 8;
            Raylib.DrawRectangle(boxX, boxY, boxW, boxH, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Raylib.DrawRectangleLines(boxX, boxY, boxW, boxH, seasonCol);
            Program.DrawTextUI(line1, boxX + 8, boxY + 6,  20, Color.Gold);
            Program.DrawTextUI($"{month}, {season}", boxX + 8, boxY + 28, 18, seasonCol);
            if (cropHint.Length > 0)
                Program.DrawTextUI($"In season: {cropHint}", boxX + 8, boxY + 50, 12, new Color((byte)160,(byte)200,(byte)140,(byte)200));
            else
                Program.DrawTextUI("No crops in season", boxX + 8, boxY + 50, 12, new Color((byte)160,(byte)120,(byte)100,(byte)200));
        }
    }
}
