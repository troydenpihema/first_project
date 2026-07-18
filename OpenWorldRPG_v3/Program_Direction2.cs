
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ═════════════════════════════════════════════════════════════════
        //  1. ENHANCED WORLD MAP — quest markers & discovered zone overlay
        // ═════════════════════════════════════════════════════════════════

        static void DrawWorldMapQuestMarkers(int cx, int cy, float scale, int mapX, int mapY, int mapW, int mapH)
        {
            bool blink = ((int)(Raylib.GetTime() * 3) % 2) == 0;

            // ── Story quest markers ──
            foreach (var q in storyQuests)
            {
                if (q.Completed) continue;
                Vector2 pos = Vector2.Zero;
                Color col = Color.Gold;
                string label = "";

                if (!q.Started)
                {
                    if (q.GiverName == "Ranger") pos = rangerNpc.Position;
                    else if (q.TriggerSpot != Vector2.Zero) pos = q.TriggerSpot;
                    col = Color.Gold; label = "!";
                }
                else if (q.Current != null && q.Current.Progress == null)
                {
                    if (q.GiverName == "Ranger") pos = rangerNpc.Position;
                    else if (q.TriggerSpot != Vector2.Zero) pos = q.TriggerSpot;
                    col = Color.SkyBlue; label = "?";
                }

                if (pos == Vector2.Zero) continue;
                int mx = cx + (int)(pos.X * scale);
                int my = cy + (int)(pos.Y * scale);
                if (mx < mapX || mx > mapX + mapW || my < mapY || my > mapY + mapH) continue;

                if (blink)
                {
                    Raylib.DrawCircle(mx, my, 8, col);
                    DrawTextUI(label, mx - 4, my - 10, 16, Color.Black);
                }
            }

            // ── NPC favor markers ──
            foreach (var fn in friendNPCs)
            {
                if (fn.ActiveFavor == null || fn.ActiveFavor.Completed) continue;
                int fx = cx + (int)(fn.Npc.Position.X * scale);
                int fy = cy + (int)(fn.Npc.Position.Y * scale);
                if (fx < mapX || fx > mapX + mapW || fy < mapY || fy > mapY + mapH) continue;
                if (blink)
                    Raylib.DrawCircle(fx, fy, 6, new Color((byte)255, (byte)180, (byte)40, (byte)255));
            }

            // ── Bus stops (fast travel points) ──
            foreach (var stop in busStops)
            {
                int sx = cx + (int)(stop.WorldPos.X * scale);
                int sy = cx + (int)(stop.WorldPos.Y * scale);
                // recalc sy properly
                sy = cy + (int)(stop.WorldPos.Y * scale);
                if (sx < mapX || sx > mapX + mapW || sy < mapY || sy > mapY + mapH) continue;
                Raylib.DrawRectangle(sx - 3, sy - 3, 6, 6, new Color((byte)40, (byte)120, (byte)220, (byte)255));
            }

            // ── Placed waypoint flags ──
            foreach (var f in placedFlags)
            {
                int fx = cx + (int)(f.X * scale);
                int fy = cy + (int)(f.Y * scale);
                if (fx < mapX || fx > mapX + mapW || fy < mapY || fy > mapY + mapH) continue;
                Raylib.DrawTriangle(
                    new Vector2(fx, fy - 8), new Vector2(fx, fy),
                    new Vector2(fx + 8, fy - 4), Color.Red);
            }

            // ── Discovered zones count ──
            DrawTextUI($"Zones discovered: {discoveredZones.Count}  |  Explored: {GetExplorationPercent():F1}%",
                mapX + 10, mapY + mapH - 22, 14, Color.Gold);
        }


        // ═════════════════════════════════════════════════════════════════
        //  2. NPC CONTEXTUAL DIALOGUE
        // ═════════════════════════════════════════════════════════════════
        // Replaces static tier dialogue with lines that react to:
        // - time of day, weather, recent world events, quest state,
        //   friendship level, player reputation, and biome

        static string GetContextualDialogue(FriendNPC f)
        {
            float hour = GetCurrentHour();
            var (repTitle, _) = GetReputationTier(player.Reputation);

            // ── World event reactions (highest priority) ──
            if (activeWorldEvent != null)
            {
                string reaction = activeWorldEvent.Type switch
                {
                    WorldEventType.GoblinRaid => $"Goblins?! Stay safe, {playerName}!",
                    WorldEventType.BanditAttack => "Bandits nearby... watch your back!",
                    WorldEventType.Blizzard => "This blizzard is brutal! Get inside!",
                    WorldEventType.ForestFire => "I can smell smoke... is the forest burning?",
                    WorldEventType.HarvestFestival => "Happy Harvest Festival! Everything's on sale!",
                    WorldEventType.FishingTournament => "The fishing tournament is on! Go catch some fish!",
                    WorldEventType.TravellingMerchant => "A travelling merchant showed up! Check their wares.",
                    WorldEventType.DragonSighting => "Did you see that dragon?! Incredible!",
                    WorldEventType.MeteorCrash => "Something fell from the sky! Go investigate!",
                    _ => null,
                };
                if (reaction != null) return reaction;
            }

            // ── Quest-aware lines ──
            var wolfQuest = storyQuests.FirstOrDefault(q => q.Title == "The Wolf Menace");
            if (wolfQuest != null && wolfQuest.Started && !wolfQuest.Completed && f.Friendship >= 30)
                return "The Ranger mentioned wolves... be careful out there.";

            if (wolfQuest != null && wolfQuest.Completed && f.Friendship >= 30)
                return "You dealt with those wolves? Legend!";

            // ── Weather reactions ──
            if (isRaining)
            {
                string[] rainLines = {
                    "Ugh, this rain! At least the crops love it.",
                    "Nice day for ducks, eh?",
                    "Hope you brought a raincoat!",
                    "The rain makes everything smell so fresh.",
                };
                return rainLines[Math.Abs(f.Name.GetHashCode()) % rainLines.Length];
            }

            // ── Time-of-day lines ──
            if (hour < 6f)
                return f.Friendship >= 60
                    ? $"You're up early! Can't sleep either, {playerName}?"
                    : "What are you doing up at this hour?";

            if (hour >= 20f)
                return f.Friendship >= 60
                    ? "Getting late... don't stay out too long!"
                    : "It's getting dark. I should head home.";

            // ── Reputation-aware ──
            if (player.Reputation >= 500 && f.Friendship < 30)
                return $"Wait... you're {playerName}?! Everyone talks about you!";

            if (player.Reputation >= 200 && f.Friendship < 30)
                return "I've heard good things about you around town.";

            // ── Biome-aware (if NPC is in a specific area) ──
            string npcBiome = GetBiomeAt(f.Npc.Position.X, f.Npc.Position.Y);
            if (npcBiome == "FARM" && f.Friendship >= 30)
                return "Nothing like fresh air on the farm!";

            // ── Personality-driven fallback ──
            if (f.Friendship >= 60 && f.Opinion != "")
                return f.Opinion;

            // ── Standard tier dialogue ──
            return f.TierDialogue;
        }


        // ═════════════════════════════════════════════════════════════════
        //  3. PLAYER JOURNAL / CODEX (J key)
        // ═════════════════════════════════════════════════════════════════

        static bool journalOpen = false;
        static int journalTab = 0;  // 0=Zones, 1=Quests, 2=NPCs, 3=Lore
        static float journalScrollY = 0f;

        static readonly string[] journalTabNames = { "ZONES", "QUESTS", "PEOPLE", "LORE" };

        static void UpdateJournal()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.J) && !chatInputOpen
                && currentScene == SceneState.World && !pauseMenuOpen)
            {
                journalOpen = !journalOpen;
                journalScrollY = 0f;
            }
            if (journalOpen && Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                journalOpen = false;
            }
        }

        static void DrawJournalUI()
        {
            if (!journalOpen) return;

            int pw = 700, ph = 520;
            int px = ScreenWidth / 2 - pw / 2;
            int py = ScreenHeight / 2 - ph / 2;

            // backdrop
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0, (byte)0, (byte)0, (byte)150));
            Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)15, (byte)12, (byte)25, (byte)245));
            Raylib.DrawRectangleLinesEx(new Rectangle(px, py, pw, ph), 2, Color.Gold);

            DrawTextUI("JOURNAL", px + pw / 2 - 55, py + 10, 32, Color.Gold);
            DrawTextUI("J or ESC to close", px + pw - 150, py + 16, 13, Color.LightGray);

            // tabs
            Vector2 mouse = Raylib.GetMousePosition();
            for (int t = 0; t < journalTabNames.Length; t++)
            {
                Rectangle tabBtn = new Rectangle(px + 10 + t * 170, py + 50, 160, 32);
                bool hover = Raylib.CheckCollisionPointRec(mouse, tabBtn);
                bool active = journalTab == t;
                Raylib.DrawRectangleRec(tabBtn, active
                    ? new Color((byte)60, (byte)50, (byte)20, (byte)255)
                    : new Color((byte)30, (byte)25, (byte)40, (byte)255));
                Raylib.DrawRectangleLinesEx(tabBtn, 1, active ? Color.Gold : (hover ? Color.Gold : Color.DarkGray));
                DrawTextUI(journalTabNames[t], (int)tabBtn.X + 20, (int)tabBtn.Y + 6, 18,
                    active ? Color.Gold : (hover ? Color.White : Color.Gray));
                if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    journalTab = t;
                    journalScrollY = 0f;
                }
            }

            // content area
            int cx = px + 14, cy = py + 92;
            int cw = pw - 28, ch = ph - 102;
            Raylib.BeginScissorMode(cx, cy, cw, ch);

            int yOff = cy - (int)journalScrollY;

            switch (journalTab)
            {
                case 0: yOff = DrawJournalZones(cx, yOff, cw); break;
                case 1: yOff = DrawJournalQuests(cx, yOff, cw); break;
                case 2: yOff = DrawJournalPeople(cx, yOff, cw); break;
                case 3: yOff = DrawJournalLore(cx, yOff, cw); break;
            }

            Raylib.EndScissorMode();

            // scroll
            float contentH = (yOff + journalScrollY) - cy;
            float maxScroll = Math.Max(0, contentH - ch);
            if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(px, py, pw, ph)))
                journalScrollY = Math.Clamp(journalScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
        }

        static int DrawJournalZones(int x, int y, int w)
        {
            DrawTextUI("DISCOVERED ZONES", x, y, 22, Color.Gold);
            y += 32;

            if (discoveredZones.Count == 0)
            {
                DrawTextUI("No zones discovered yet — explore the world!", x, y, 16, Color.Gray);
                return y + 30;
            }

            foreach (string zone in discoveredZones.OrderBy(z => z))
            {
                Color zCol = GetZoneDiscoveryColor(zone);
                Raylib.DrawRectangle(x, y, 12, 12, zCol);
                DrawTextUI(zone, x + 20, y - 1, 16, Color.White);

                // XP that was awarded
                int xp = GetZoneDiscoveryXP(zone);
                if (xp > 0)
                    DrawTextUI($"+{xp} XP", x + w - 80, y - 1, 14, Color.Gold);

                y += 24;
            }

            y += 10;
            int total = 30; // approximate total zone types
            DrawTextUI($"Progress: {discoveredZones.Count}/{total} zones", x, y, 16, Color.LightGray);
            return y + 30;
        }

        static int DrawJournalQuests(int x, int y, int w)
        {
            // Story quests
            DrawTextUI("STORY QUESTS", x, y, 22, Color.SkyBlue);
            y += 30;
            foreach (var sq in storyQuests)
            {
                Color c = sq.Completed ? Color.Green : sq.Started ? Color.White : Color.DarkGray;
                string status = sq.Completed ? "[COMPLETE]" : sq.Started ? "[IN PROGRESS]" : "[NOT STARTED]";
                DrawTextUI($"{sq.Title} — {status}", x + 8, y, 16, c);
                y += 20;
                if (sq.Started && !sq.Completed && sq.Current != null)
                {
                    DrawTextUI($"  → {sq.Current.Description}", x + 20, y, 14, Color.LightGray);
                    y += 18;
                }
            }

            y += 16;
            DrawTextUI("SIDE QUESTS", x, y, 22, Color.Gold);
            y += 30;
            foreach (var q in quests)
            {
                Color c = q.Completed ? Color.Green : Color.White;
                DrawTextUI($"{q.Description} — {q.Progress}/{q.Target} {(q.Completed ? "[DONE]" : "")}",
                    x + 8, y, 16, c);
                y += 22;
            }

            // Active favors
            var activeFavors = friendNPCs.Where(f => f.ActiveFavor != null && !f.ActiveFavor.Completed).ToList();
            if (activeFavors.Count > 0)
            {
                y += 16;
                DrawTextUI("NPC FAVORS", x, y, 22, new Color((byte)255, (byte)200, (byte)40, (byte)255));
                y += 30;
                foreach (var fn in activeFavors)
                {
                    var fav = fn.ActiveFavor;
                    DrawTextUI($"{fn.Name}: bring {fav.AmountNeeded - fav.AmountDelivered} {fav.ItemNeeded}  (+${fav.RewardAmount})",
                        x + 8, y, 16, Color.White);
                    y += 22;
                }
            }

            return y + 10;
        }

        static int DrawJournalPeople(int x, int y, int w)
        {
            DrawTextUI("PEOPLE YOU'VE MET", x, y, 22, Color.Gold);
            y += 32;

            foreach (var f in friendNPCs.OrderByDescending(f => f.Friendship))
            {
                Color tierCol = f.Friendship >= 90 ? new Color((byte)255, (byte)100, (byte)150, (byte)255)
                    : f.Friendship >= 60 ? new Color((byte)220, (byte)140, (byte)60, (byte)255)
                    : f.Friendship >= 30 ? Color.Gold : Color.Gray;

                DrawTextUI(f.Name, x + 8, y, 18, Color.White);
                DrawTextUI($"[{f.Tier}]", x + 140, y + 2, 14, tierCol);

                // friendship bar
                Raylib.DrawRectangle(x + 280, y + 4, 120, 10, new Color((byte)40, (byte)40, (byte)40, (byte)255));
                Raylib.DrawRectangle(x + 280, y + 4, (int)(120 * f.Friendship / 100f), 10, tierCol);

                // personality & details (only if friend+)
                if (f.Friendship >= 30)
                {
                    string details = f.Personality;
                    if (f.Partner != "" && f.Partner != "someone")
                        details += $" — with {f.Partner}";
                    DrawTextUI(details, x + 420, y + 2, 13, Color.LightGray);
                }

                y += 28;

                // show likes at close friend
                if (f.Friendship >= 60 && f.Likes.Length > 0)
                {
                    DrawTextUI($"  Likes: {string.Join(", ", f.Likes.Take(3))}  |  Fav gift: {f.FavoriteGift}",
                        x + 20, y, 13, new Color((byte)140, (byte)180, (byte)140, (byte)255));
                    y += 18;
                }

                // show dislikes at best friend
                if (f.Friendship >= 90 && f.Dislikes.Length > 0)
                {
                    DrawTextUI($"  Dislikes: {string.Join(", ", f.Dislikes.Take(3))}",
                        x + 20, y, 13, new Color((byte)200, (byte)130, (byte)130, (byte)255));
                    y += 18;
                }
            }

            return y + 10;
        }

        static int DrawJournalLore(int x, int y, int w)
        {
            DrawTextUI("WORLD LORE", x, y, 22, Color.Gold);
            y += 32;

            // biome lore entries — unlocked by discovery
            var loreEntries = new (string zone, string lore)[]
            {
                ("SAFE ZONE", "The starting village, sheltered from the wild. A safe haven for newcomers."),
                ("FARM", "Rich soil stretches north of town. Farmers grow wheat, corn, and potatoes in the seasonal cycle."),
                ("FOREST", "Dense woodland full of timber, wild dogs, and wolves. The Ranger keeps watch from his cabin."),
                ("SNOW ZONE", "A frozen expanse where the air bites. Arctic logs grow here, and blizzards strike without warning."),
                ("DESERT", "Scorching sands with hidden oases. Scorpions and snakes lurk beneath the dunes."),
                ("BEACH", "Golden sand meets the ocean. A peaceful zone where fishermen cast their lines."),
                ("OCEAN", "Deep waters teeming with sea life. Sharks patrol the deeper regions."),
                ("VOLCANO", "Molten rock and ember stones. The most dangerous zone, home to magma shards and lava cores."),
                ("MOUNTAINS", "Steep cliffs and alpine meadows. Crystal caves hide deep within the rock."),
                ("SWAMP", "Murky waters and twisted trees. Crocodiles and snakes make this a treacherous place."),
                ("HAMILTRON CITY", "The biggest settlement in the region. Shops, takeaways, and entertainment abound."),
                ("ROTOAIRA", "A quiet lakeside town with a rich cultural heritage."),
                ("MEADOWLANDS", "Rolling green hills stretching to the horizon. A peaceful pastoral landscape."),
            };

            foreach (var (zone, lore) in loreEntries)
            {
                if (!discoveredZones.Contains(zone)) continue;

                Color zCol = GetZoneDiscoveryColor(zone);
                Raylib.DrawRectangle(x, y, w - 20, 2, new Color((byte)40, (byte)40, (byte)50, (byte)255));
                y += 6;
                DrawTextUI(zone, x + 4, y, 18, zCol);
                y += 24;

                // word-wrap lore text
                string[] words = lore.Split(' ');
                string line = "";
                foreach (string word in words)
                {
                    string test = line.Length > 0 ? line + " " + word : word;
                    if (MeasureTextUI(test, 14) > w - 40)
                    {
                        DrawTextUI(line, x + 12, y, 14, Color.LightGray);
                        y += 18;
                        line = word;
                    }
                    else line = test;
                }
                if (line.Length > 0)
                {
                    DrawTextUI(line, x + 12, y, 14, Color.LightGray);
                    y += 18;
                }
                y += 10;
            }

            // undiscovered tease
            int undiscovered = loreEntries.Count(e => !discoveredZones.Contains(e.zone));
            if (undiscovered > 0)
            {
                DrawTextUI($"{undiscovered} more lore entries to discover...", x + 4, y, 14, Color.DarkGray);
                y += 24;
            }

            return y + 10;
        }


        // ═════════════════════════════════════════════════════════════════
        //  4. WEATHER GAMEPLAY EFFECTS
        // ═════════════════════════════════════════════════════════════════
        // Rain:  auto-waters all farm plots, +25% crop growth, fish bite faster
        // Snow biome in rain: slower stamina regen, slight HP drain (handled in existing blizzard)
        // Drought (world event): crops grow 50% slower, stamina drains faster
        // Clear weather: normal rates

        static float weatherGrowthMult = 1f;
        static float weatherStaminaMult = 1f;
        static float weatherFishMult = 1f;
        static string weatherStatusText = "";

        static float GetWeatherGrowthMult() => weatherGrowthMult;
        static float GetWeatherFishMult() => weatherFishMult;
        static float GetWeatherStaminaMult() => weatherStaminaMult;

        static void UpdateWeatherEffects(float dt)
        {
            // reset each frame
            weatherGrowthMult = 1f;
            weatherStaminaMult = 1f;
            weatherFishMult = 1f;
            weatherStatusText = "";

            if (isRaining)
            {
                // auto-water all planted crops (not just spring)
                foreach (var plot in farmPlots)
                {
                    if (plot.Planted && !plot.Watered && !plot.ReadyToHarvest)
                        plot.Watered = true;
                }

                weatherGrowthMult = 1.25f;  // rain boosts growth 25%
                weatherFishMult = 1.3f;     // fish bite 30% faster in rain
                weatherStatusText = "Rain: crops auto-watered, +25% growth, fish bite faster";

                // snow zone during rain = extra cold
                if (currentBiome == "SNOW ZONE" || currentBiome == "TUNDRA"
                    || currentBiome == "FROZEN LAKE" || currentBiome == "ICE CAVES")
                {
                    weatherStaminaMult = 0.7f;  // stamina regens slower
                    weatherStatusText = "Snowstorm: stamina regen reduced, crops boosted";
                }
            }

            // drought from world event
            if (eventDroughtActive)
            {
                weatherGrowthMult = 0.5f;
                weatherStaminaMult = 0.8f;
                weatherStatusText = "Drought: crop growth halved, stamina drains faster";
            }
        }

        static void DrawWeatherStatus()
        {
            if (string.IsNullOrEmpty(weatherStatusText)) return;

            // small status bar at bottom-center
            int tw = MeasureTextUI(weatherStatusText, 13);
            int bx = ScreenWidth / 2 - tw / 2 - 10;
            int by = ScreenHeight - 22;

            Color barCol = isRaining
                ? new Color((byte)40, (byte)80, (byte)140, (byte)160)
                : new Color((byte)140, (byte)80, (byte)20, (byte)160);

            Raylib.DrawRectangle(bx, by, tw + 20, 20, barCol);
            DrawTextUI(weatherStatusText, bx + 10, by + 3, 13, Color.White);
        }


        // ═════════════════════════════════════════════════════════════════
        //  5. NOTIFICATION QUEUE
        // ═════════════════════════════════════════════════════════════════
        // Replaces the single ShowNotification that gets overwritten.
        // Queues up to 5 messages, shows them one at a time with fade.

        struct QueuedNotification
        {
            public string Message;
            public float Timer;
            public float Duration;
        }

        static Queue<QueuedNotification> notificationQueue = new();
        static QueuedNotification? activeNotification = null;
        const int MaxQueuedNotifications = 8;
        const float DefaultNotificationDuration = 2.5f;

        // Call this instead of the old ShowNotification
        static void QueueNotification(string message, float duration = DefaultNotificationDuration)
        {
            if (notificationQueue.Count >= MaxQueuedNotifications)
                notificationQueue.Dequeue(); // drop oldest if full

            notificationQueue.Enqueue(new QueuedNotification
            {
                Message = message,
                Timer = duration,
                Duration = duration,
            });
        }

        static void UpdateNotificationQueue(float dt)
        {
            // tick active notification
            if (activeNotification.HasValue)
            {
                var n = activeNotification.Value;
                n.Timer -= dt;
                if (n.Timer <= 0)
                    activeNotification = null;
                else
                    activeNotification = n;
            }

            // pull next from queue if no active
            if (!activeNotification.HasValue && notificationQueue.Count > 0)
                activeNotification = notificationQueue.Dequeue();

            // also update the legacy levelUpTimer for backward compat
            if (levelUpTimer > 0) levelUpTimer -= dt;
        }

        static void DrawNotificationQueue()
        {
            // draw the stacked notification (top-center, below the biome banner area)
            if (!activeNotification.HasValue) return;

            var n = activeNotification.Value;
            float fadeIn = Math.Min(1f, (n.Duration - n.Timer + 0.01f) * 4f); // fast fade in
            float fadeOut = Math.Min(1f, n.Timer * 3f);                        // slower fade out
            float alpha = Math.Min(fadeIn, fadeOut);

            byte a = (byte)(255 * alpha);
            byte boxA = (byte)(210 * alpha);

            int textW = MeasureTextUI(n.Message, 22);
            int bx = ScreenWidth / 2 - textW / 2;

            Raylib.DrawRectangle(bx - 18, 274, textW + 36, 44,
                new Color((byte)0, (byte)0, (byte)0, boxA));
            Raylib.DrawRectangleLinesEx(
                new Rectangle(bx - 18, 274, textW + 36, 44), 1,
                new Color((byte)255, (byte)215, (byte)0, a));
            DrawTextUI(n.Message, bx, 282, 22,
                new Color((byte)255, (byte)215, (byte)0, a));

            // queue count indicator (if more waiting)
            if (notificationQueue.Count > 0)
            {
                DrawTextUI($"+{notificationQueue.Count} more", bx + textW + 24, 288, 12,
                    new Color((byte)180, (byte)180, (byte)180, a));
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  COMBINED DRAW (called from DrawHUD)
        // ═════════════════════════════════════════════════════════════════

        static void DrawDirection2HUD()
        {
            DrawNotificationQueue();
            DrawWeatherStatus();
            DrawJournalUI();
        }
    }
}
