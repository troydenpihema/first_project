
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ─────────────────────────────────────────────────────────────────
        //  1. "WHAT'S NEXT" OBJECTIVE TRACKER
        // ─────────────────────────────────────────────────────────────────
        // Chains naturally: tutorial → tools → crafting → ranger → story
        // Shows a single priority objective at the top-left of the screen

        static string GetWhatsNextObjective()
        {
            // Priority 1: Tutorial still active
            if (tutorialActive && tutorialStep < tutorialTasks.Count)
                return null; // tutorial HUD already visible, don't double up

            // Priority 2: No axe yet — get basic tools
            if (!player.HasAxe)
                return "Find an axe — check near the tutorial area";

            // Priority 3: Low resources — encourage gathering
            if (player.Logs < 20 && player.WoodcuttingLevel < 3)
                return "Chop trees to gather logs (need 20 for a Workbench)";

            // Priority 4: No workbench placed — craft one
            if (placedWorkbenches.Count == 0 && player.Logs >= 20)
                return "Open Crafting (C) and build a Workbench";

            // Priority 5: Haven't visited the Ranger yet
            var wolfQuest = storyQuests.FirstOrDefault(q => q.Title == "The Wolf Menace");
            if (wolfQuest != null && !wolfQuest.Started && !wolfQuest.Completed)
                return "Visit the Ranger north of the farm (gold ! on minimap)";

            // Priority 6: Active story quest — show its current stage
            var activeStory = storyQuests.FirstOrDefault(q => q.Started && !q.Completed && q.Current != null);
            if (activeStory != null)
            {
                var st = activeStory.Current;
                if (st.Progress != null)
                {
                    int prog = Math.Clamp(st.Progress() - st.Baseline, 0, st.Target);
                    return $"[{activeStory.Title}] {st.Description} ({prog}/{st.Target})";
                }
                return $"[{activeStory.Title}] {st.Description}";
            }

            // Priority 7: Incomplete side quests
            var sideQuest = quests.FirstOrDefault(q => !q.Completed);
            if (sideQuest != null)
                return $"Quest: {sideQuest.Description} ({sideQuest.Progress}/{sideQuest.Target})";

            // Priority 8: Active NPC favor
            var favor = friendNPCs.FirstOrDefault(f => f.ActiveFavor != null && !f.ActiveFavor.Completed);
            if (favor != null)
            {
                var fav = favor.ActiveFavor;
                return $"Favor for {favor.Name}: bring {fav.AmountNeeded - fav.AmountDelivered} {fav.ItemNeeded}";
            }

            // Priority 9: Encourage exploration if nothing else
            if (discoveredZones.Count < 5)
                return "Explore the world — discover new zones for XP!";

            // Priority 10: Encourage friendship
            if (friendNPCs.All(f => f.Friendship < 30))
                return "Talk to villagers daily to build friendships";

            return null; // nothing to show — player is self-directed
        }

        static float whatsNextPulse = 0f;

        static void DrawWhatsNextHUD()
        {
            string objective = GetWhatsNextObjective();
            if (objective == null) return;

            whatsNextPulse += Raylib.GetFrameTime();

            int px = 12, py = minimapY + minimapSize + 30;
            int textW = MeasureTextUI(objective, 16);
            int boxW = Math.Min(textW + 40, 560);

            // subtle pulsing border
            float pulse = 0.5f + 0.5f * MathF.Sin(whatsNextPulse * 2f);
            byte borderAlpha = (byte)(120 + (int)(60 * pulse));

            Raylib.DrawRectangle(px, py, boxW, 36, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            Raylib.DrawRectangleLines(px, py, boxW, 36,
                new Color((byte)255, (byte)215, (byte)0, borderAlpha));

            // compass arrow icon
            DrawTextUI("►", px + 6, py + 8, 16,
                new Color((byte)255, (byte)215, (byte)0, (byte)220));

            // objective text — truncate if needed
            string display = objective.Length > 62 ? objective[..59] + "..." : objective;
            DrawTextUI(display, px + 24, py + 9, 15, Color.White);
        }


        // ─────────────────────────────────────────────────────────────────
        //  2. QUEST WAYPOINT MARKERS ON MINIMAP
        // ─────────────────────────────────────────────────────────────────
        // Draws blinking markers for: story quest givers, active story
        // quest return points, and active NPC favor givers.
        // Called from DrawMinimap() before EndScissorMode.

        static void DrawQuestWaypoints(int cx, int cy, float minimapScale)
        {
            bool blinkOn = ((int)(Raylib.GetTime() * 3) % 2) == 0;

            // ── Story quest giver markers (gold "!" for unstarted, blue "?" for return stages) ──
            foreach (var q in storyQuests)
            {
                if (q.Completed) continue;

                Vector2 targetPos = Vector2.Zero;
                Color markerCol = Color.Gold;

                if (!q.Started)
                {
                    // show where to pick up the quest
                    if (q.GiverName == "Ranger")
                        targetPos = rangerNpc.Position;
                    else if (q.TriggerSpot != Vector2.Zero)
                        targetPos = q.TriggerSpot;
                    markerCol = Color.Gold;
                }
                else if (q.Current != null && q.Current.Progress == null)
                {
                    // return stage — show where to go back
                    if (q.GiverName == "Ranger")
                        targetPos = rangerNpc.Position;
                    else if (q.TriggerSpot != Vector2.Zero)
                        targetPos = q.TriggerSpot;
                    markerCol = Color.SkyBlue;
                }

                if (targetPos == Vector2.Zero) continue;

                int mx = cx + (int)((targetPos.X - player.Position.X) * minimapScale);
                int my = cy + (int)((targetPos.Y - player.Position.Y) * minimapScale);
                // clamp to minimap edge
                int cmx = Math.Clamp(mx, minimapX + 4, minimapX + minimapSize - 4);
                int cmy = Math.Clamp(my, minimapY + 4, minimapY + minimapSize - 4);

                if (blinkOn)
                {
                    Raylib.DrawCircle(cmx, cmy, 5, markerCol);
                    Raylib.DrawCircleLines(cmx, cmy, 7, Color.Black);
                }
            }

            // ── NPC favor markers (orange diamonds) ──
            foreach (var fn in friendNPCs)
            {
                if (fn.ActiveFavor == null || fn.ActiveFavor.Completed) continue;

                int fx = cx + (int)((fn.Npc.Position.X - player.Position.X) * minimapScale);
                int fy = cy + (int)((fn.Npc.Position.Y - player.Position.Y) * minimapScale);
                int cfx = Math.Clamp(fx, minimapX + 4, minimapX + minimapSize - 4);
                int cfy = Math.Clamp(fy, minimapY + 4, minimapY + minimapSize - 4);

                if (blinkOn)
                {
                    // small diamond shape
                    Raylib.DrawTriangle(
                        new Vector2(cfx, cfy - 5),
                        new Vector2(cfx - 4, cfy),
                        new Vector2(cfx + 4, cfy),
                        new Color((byte)255, (byte)180, (byte)40, (byte)255));
                    Raylib.DrawTriangle(
                        new Vector2(cfx - 4, cfy),
                        new Vector2(cfx + 4, cfy),
                        new Vector2(cfx, cfy + 5),
                        new Color((byte)255, (byte)180, (byte)40, (byte)255));
                }
            }
        }


        // ─────────────────────────────────────────────────────────────────
        //  3. ZONE DISCOVERY NOTIFICATIONS + XP
        // ─────────────────────────────────────────────────────────────────
        // Tracks which biomes the player has visited for the first time
        // and awards exploration XP + a banner notification.

        static HashSet<string> discoveredZones = new();
        static string zoneDiscoveryMessage = "";
        static float zoneDiscoveryTimer = 0f;

        // XP reward per zone tier
        static int GetZoneDiscoveryXP(string zone) => zone switch
        {
            "SAFE ZONE" => 0,     // starting area, no bonus
            "FARM" => 10,
            "GRASSLANDS" => 15,
            "FOREST" or "DARK FOREST" or "ENCHANTED WOODS" or "MUSHROOM GROVE" => 25,
            "SNOW ZONE" or "TUNDRA" or "FROZEN LAKE" or "ICE CAVES" => 40,
            "DESERT" or "OASIS" or "DUNES" or "BADLANDS" => 40,
            "BEACH" => 20,
            "OCEAN" or "CORAL REEF" or "DEEP OCEAN" or "ISLANDS" => 50,
            "VOLCANO" or "CALDERA" or "ASHEN WASTES" or "LAVA FIELDS" => 60,
            "MOUNTAINS" or "ALPINE MEADOW" or "CLIFFS" or "CRYSTAL CAVES" => 50,
            "SWAMP" or "MANGROVE" or "BOG" or "DEAD MARSH" => 45,
            "HAMILTRON CITY" => 30,
            "ROTOAIRA" => 30,
            "MEADOWLANDS" => 20,
            "JUNGLE" or "RAINFOREST" => 55,
            _ => 20,
        };

        static Color GetZoneDiscoveryColor(string zone) => zone switch
        {
            "SNOW ZONE" or "TUNDRA" or "FROZEN LAKE" or "ICE CAVES"
                => new Color((byte)150, (byte)200, (byte)255, (byte)255),
            "DESERT" or "OASIS" or "DUNES" or "BADLANDS"
                => new Color((byte)210, (byte)180, (byte)80, (byte)255),
            "FOREST" or "DARK FOREST" or "ENCHANTED WOODS" or "MUSHROOM GROVE"
                => new Color((byte)50, (byte)180, (byte)50, (byte)255),
            "VOLCANO" or "CALDERA" or "ASHEN WASTES" or "LAVA FIELDS"
                => new Color((byte)220, (byte)80, (byte)30, (byte)255),
            "OCEAN" or "CORAL REEF" or "DEEP OCEAN" or "ISLANDS"
                => new Color((byte)40, (byte)130, (byte)220, (byte)255),
            "BEACH" => new Color((byte)240, (byte)220, (byte)120, (byte)255),
            "MOUNTAINS" or "ALPINE MEADOW" or "CLIFFS" or "CRYSTAL CAVES"
                => new Color((byte)180, (byte)175, (byte)165, (byte)255),
            "HAMILTRON CITY" or "ROTOAIRA"
                => new Color((byte)200, (byte)200, (byte)220, (byte)255),
            _ => Color.White,
        };

        static void CheckZoneDiscovery()
        {
            if (string.IsNullOrEmpty(currentBiome)) return;
            if (discoveredZones.Contains(currentBiome)) return;

            discoveredZones.Add(currentBiome);
            int xp = GetZoneDiscoveryXP(currentBiome);

            if (xp > 0)
            {
                player.AddCombatXP(xp); // use combat XP as general exploration XP
                zoneDiscoveryMessage = $"NEW ZONE: {currentBiome}  (+{xp} XP)";
            }
            else
            {
                zoneDiscoveryMessage = $"NEW ZONE: {currentBiome}";
            }

            zoneDiscoveryTimer = 3.5f;

            // also feed into achievements visited set
            if (!achievementVisited.Contains(currentBiome))
                achievementVisited.Add(currentBiome);
        }

        static void DrawZoneDiscoveryBanner()
        {
            if (zoneDiscoveryTimer <= 0) return;

            byte alpha = (byte)(255 * Math.Min(1f, zoneDiscoveryTimer));
            Color zoneCol = GetZoneDiscoveryColor(currentBiome);
            zoneCol = new Color(zoneCol.R, zoneCol.G, zoneCol.B, alpha);

            int textW = MeasureTextUI(zoneDiscoveryMessage, 30);
            int bx = ScreenWidth / 2 - textW / 2;

            byte boxAlpha = (byte)(200 * Math.Min(1f, zoneDiscoveryTimer));
            Raylib.DrawRectangle(bx - 24, 220, textW + 48, 48,
                new Color((byte)0, (byte)0, (byte)0, boxAlpha));
            Raylib.DrawRectangleLinesEx(
                new Rectangle(bx - 24, 220, textW + 48, 48), 2, zoneCol);

            // star icon
            DrawTextUI("★", bx - 16, 226, 26, zoneCol);
            DrawTextUI(zoneDiscoveryMessage, bx + 14, 228, 30, zoneCol);

            // discovery count
            string countText = $"Zones discovered: {discoveredZones.Count}";
            int cw = MeasureTextUI(countText, 14);
            DrawTextUI(countText, ScreenWidth / 2 - cw / 2, 254, 14,
                new Color((byte)200, (byte)200, (byte)200, alpha));
        }


        // ─────────────────────────────────────────────────────────────────
        //  4. MILESTONE UNLOCK HINTS
        // ─────────────────────────────────────────────────────────────────
        // When player crosses friendship tiers, reputation tiers, or
        // notable skill levels, show a brief hint about what's unlocked.

        static string milestoneMessage = "";
        static float milestoneTimer = 0f;

        static void CheckMilestoneUnlocks()
        {
            // ── Friendship milestones ──
            foreach (var fn in friendNPCs)
            {
                if (!fn.Milestone30 && fn.Friendship >= 30)
                {
                    fn.Milestone30 = true;
                    string hint = fn.Shop != null
                        ? $"{fn.Name} is now a Friend! Shop discount unlocked."
                        : $"{fn.Name} is now a Friend! New dialogue available.";
                    ShowMilestone(hint);
                }
                if (!fn.Milestone60 && fn.Friendship >= 60)
                {
                    fn.Milestone60 = true;
                    ShowMilestone($"{fn.Name} is now a Close Friend! Gift quality bonus active.");
                }
                if (!fn.Milestone90 && fn.Friendship >= 90)
                {
                    fn.Milestone90 = true;
                    ShowMilestone($"{fn.Name} is now a Best Friend! Special rewards unlocked!");
                }
            }

            // ── Skill level milestones ──
            CheckSkillMilestone("Woodcutting", player.WoodcuttingLevel, 10, "Faster chop speed & oak trees available");
            CheckSkillMilestone("Woodcutting", player.WoodcuttingLevel, 25, "Birch & pine trees available");
            CheckSkillMilestone("Combat", player.CombatLevel, 10, "Increased crit chance");
            CheckSkillMilestone("Combat", player.CombatLevel, 25, "Higher combo damage multiplier");
            CheckSkillMilestone("Mining", player.MiningLevel, 10, "Iron ore nodes available");
            CheckSkillMilestone("Mining", player.MiningLevel, 25, "Gold ore nodes available");
            CheckSkillMilestone("Fishing", player.FishingLevel, 10, "Rare fish unlocked");
            CheckSkillMilestone("Fishing", player.FishingLevel, 25, "Ocean fishing available");
            CheckSkillMilestone("Cooking", player.CookingLevel, 10, "Advanced recipes unlocked");
            CheckSkillMilestone("Farming", player.FarmingLevel, 10, "Fruit trees available");
            CheckSkillMilestone("Crafting", player.CraftingLevel, 10, "Advanced Workstation recipes");
        }

        static HashSet<string> notifiedMilestones = new();

        static void CheckSkillMilestone(string skill, int level, int threshold, string hint)
        {
            string key = $"{skill}_{threshold}";
            if (level >= threshold && !notifiedMilestones.Contains(key))
            {
                notifiedMilestones.Add(key);
                ShowMilestone($"{skill} Lv{threshold}: {hint}");
            }
        }

        static void ShowMilestone(string message)
        {
            milestoneMessage = message;
            milestoneTimer = 4f;
        }

        static void DrawMilestoneHint()
        {
            if (milestoneTimer <= 0) return;

            byte alpha = (byte)(255 * Math.Min(1f, milestoneTimer));
            int textW = MeasureTextUI(milestoneMessage, 20);
            int bx = ScreenWidth / 2 - textW / 2;

            byte boxAlpha = (byte)(210 * Math.Min(1f, milestoneTimer));
            Raylib.DrawRectangle(bx - 20, 170, textW + 40, 40,
                new Color((byte)20, (byte)10, (byte)40, boxAlpha));
            Raylib.DrawRectangleLinesEx(
                new Rectangle(bx - 20, 170, textW + 40, 40), 2,
                new Color((byte)180, (byte)120, (byte)255, alpha));

            DrawTextUI("⬆", bx - 12, 176, 20,
                new Color((byte)180, (byte)120, (byte)255, alpha));
            DrawTextUI(milestoneMessage, bx + 12, 179, 20,
                new Color((byte)220, (byte)200, (byte)255, alpha));
        }


        // ─────────────────────────────────────────────────────────────────
        //  5. DAILY CHALLENGE PULSE
        // ─────────────────────────────────────────────────────────────────
        // A brief "New challenges available!" pulse at the start of each
        // day to draw attention to the daily challenge system.

        static bool dailyChallengePulseShown = false;
        static float dailyChallengePulseTimer = 0f;

        static void TriggerDailyChallengePulse()
        {
            dailyChallengePulseTimer = 3f;
            dailyChallengePulseShown = true;
        }

        static void DrawDailyChallengePulse()
        {
            if (dailyChallengePulseTimer <= 0) return;

            byte alpha = (byte)(255 * Math.Min(1f, dailyChallengePulseTimer));
            string msg = "★ New daily challenges available!";
            int textW = MeasureTextUI(msg, 22);
            int px = ScreenWidth / 2 - textW / 2;

            Raylib.DrawRectangle(px - 16, 130, textW + 32, 34,
                new Color((byte)40, (byte)30, (byte)0, (byte)(180 * Math.Min(1f, dailyChallengePulseTimer))));
            Raylib.DrawRectangleLinesEx(
                new Rectangle(px - 16, 130, textW + 32, 34), 1,
                new Color((byte)255, (byte)200, (byte)40, alpha));
            DrawTextUI(msg, px, 136, 22,
                new Color((byte)255, (byte)220, (byte)60, alpha));
        }


        // ─────────────────────────────────────────────────────────────────
        //  COMBINED UPDATE & DRAW
        // ─────────────────────────────────────────────────────────────────

        static void UpdateDirection(float dt)
        {
            // zone discovery check
            CheckZoneDiscovery();
            if (zoneDiscoveryTimer > 0) zoneDiscoveryTimer -= dt;

            // milestone checks
            CheckMilestoneUnlocks();
            if (milestoneTimer > 0) milestoneTimer -= dt;

            // daily challenge pulse
            if (dailyChallengePulseTimer > 0) dailyChallengePulseTimer -= dt;
        }

        static void DrawDirectionHUD()
        {
            DrawWhatsNextHUD();
            DrawZoneDiscoveryBanner();
            DrawMilestoneHint();
            DrawDailyChallengePulse();
        }
    }
}
