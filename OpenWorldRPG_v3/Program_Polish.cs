
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
        //  1. SMOOTH CAMERA + ENHANCED SHAKE
        // ═════════════════════════════════════════════════════════════════

        static Vector2 smoothCameraPos;
        static bool smoothCameraInitialized = false;

        static Vector2 GetSmoothedCameraTarget(float dt)
        {
            Vector2 target = player.Position;

            if (!smoothCameraInitialized)
            {
                smoothCameraPos = target;
                smoothCameraInitialized = true;
            }

            // lerp speed — faster when far away, slower when close
            float dist = Vector2.Distance(smoothCameraPos, target);
            float speed = dist > 400 ? 20f : dist > 100 ? 10f : 6f;

            // in vehicles, tighten the follow so it doesn't lag behind
            bool driving = vehicles.Any(v => v.Driving);
            bool riding = rideables.Any(r => r.Riding);
            if (driving) speed = 14f;
            if (riding) speed = 10f;

            smoothCameraPos = Vector2.Lerp(smoothCameraPos, target, speed * dt);
            return smoothCameraPos;
        }

        // Enhanced shake: variable magnitude based on duration
        static void TriggerShakeEnhanced(float duration, float magnitude = 6f)
        {
            shakeDuration = Math.Max(shakeDuration, duration);
            shakeMagnitude = magnitude;
        }


        // ═════════════════════════════════════════════════════════════════
        //  2. LOADING SCREEN TIPS
        // ═════════════════════════════════════════════════════════════════

        static readonly string[] loadingTips =
        {
            "Rain auto-waters your crops — save your watering can charges!",
            "Talk to villagers daily to build friendship and unlock gifts.",
            "Craft a Waypoint Flag to mark important locations on your map.",
            "Press J to open your Journal — track quests, zones, and lore.",
            "Higher fishing levels unlock rare fish in deeper waters.",
            "The Ranger north of the farm has story quests for you.",
            "Combo hits deal bonus damage — keep attacking without pausing!",
            "Explore new zones to earn discovery XP and fill your journal.",
            "Reputation unlocks shop discounts and better job pay.",
            "Visit the hospital or marae to sober up after drinking.",
            "Craft a Campfire to cook food anywhere in the wild.",
            "Press T to switch between Tools and Combat modes.",
            "NPC favors reward money and bonus friendship — check daily!",
            "Daily challenges reset each day — complete all 3 for a bonus.",
            "Dungeons get harder with each room — bring good gear!",
            "In-season crops grow faster — check the calendar for seasons.",
            "Mining level 10 unlocks iron ore, level 25 unlocks gold.",
            "Your workbench opens advanced crafting recipes.",
            "Bus routes run from 6 AM to noon — $2 per trip.",
            "Press M for the world map, G for equipment, TAB for inventory.",
        };

        static int currentTipIndex = -1;

        static void DrawLoadingTip()
        {
            if (sceneFadeAlpha < 200f) return; // only show when mostly faded

            // pick a tip once per transition
            if (currentTipIndex < 0)
                currentTipIndex = Raylib.GetRandomValue(0, loadingTips.Length - 1);

            string tip = loadingTips[currentTipIndex];
            int tw = MeasureTextUI(tip, 18);
            int tx = ScreenWidth / 2 - tw / 2;

            DrawTextUI("TIP", ScreenWidth / 2 - 15, ScreenHeight / 2 + 20, 16, Color.Gold);
            DrawTextUI(tip, tx, ScreenHeight / 2 + 42, 18, Color.White);
        }

        // Reset tip when transition ends
        static void ResetLoadingTip()
        {
            currentTipIndex = -1;
        }


        // ═════════════════════════════════════════════════════════════════
        //  3. PERK ACTIVATION FEEDBACK
        // ═════════════════════════════════════════════════════════════════

        static string perkFlashMessage = "";
        static float perkFlashTimer = 0f;
        static Color perkFlashColor = Color.Gold;

        static void TriggerPerkFlash(string perkName, string description, Color color)
        {
            perkFlashMessage = $"PERK: {perkName} — {description}";
            perkFlashTimer = 2f;
            perkFlashColor = color;
        }

        static void UpdatePerkFlash(float dt)
        {
            if (perkFlashTimer > 0) perkFlashTimer -= dt;
        }

        static void DrawPerkFlash()
        {
            if (perkFlashTimer <= 0) return;

            byte alpha = (byte)(255 * Math.Min(1f, perkFlashTimer));
            int tw = MeasureTextUI(perkFlashMessage, 18);
            int px = ScreenWidth / 2 - tw / 2;
            int py = 100;

            Raylib.DrawRectangle(px - 16, py, tw + 32, 30,
                new Color((byte)20, (byte)10, (byte)30, (byte)(180 * Math.Min(1f, perkFlashTimer))));
            Raylib.DrawRectangleLinesEx(new Rectangle(px - 16, py, tw + 32, 30), 1,
                new Color(perkFlashColor.R, perkFlashColor.G, perkFlashColor.B, alpha));
            DrawTextUI(perkFlashMessage, px, py + 6, 18,
                new Color(perkFlashColor.R, perkFlashColor.G, perkFlashColor.B, alpha));
        }

        // Track which perks have been announced so we only flash once
        static HashSet<string> announcedPerks = new();

        static void CheckPerkActivations()
        {
            void Check(string skill, int level, string perkName, string desc, Color col)
            {
                string key = $"{skill}_{perkName}";
                if (level >= GetPerkLevel(skill, perkName) && !announcedPerks.Contains(key))
                {
                    announcedPerks.Add(key);
                    TriggerPerkFlash(perkName, desc, col);
                }
            }

            int GetPerkLevel(string skill, string name)
            {
                if (!skillPerks.TryGetValue(skill, out var perks)) return 999;
                foreach (var p in perks) if (p.Name == name) return p.Level;
                return 999;
            }

            // Check key perks across skills
            Check("Woodcutting", player.WoodcuttingLevel, "Timber!",         "Trees drop +1 log",        new Color((byte)139,(byte)90,(byte)43,(byte)255));
            Check("Woodcutting", player.WoodcuttingLevel, "Deep Roots",      "Rare wood chance +10%",     new Color((byte)139,(byte)90,(byte)43,(byte)255));
            Check("Combat",     player.CombatLevel,       "Critical Eye",    "Crit chance increased",     Color.Red);
            Check("Combat",     player.CombatLevel,       "Berserker",       "+25% dmg below half HP",    Color.Red);
            Check("Fishing",    player.FishingLevel,       "Steady Hand",    "Reel zone wider",           new Color((byte)40,(byte)150,(byte)220,(byte)255));
            Check("Mining",     player.MiningLevel,        "Prospector",     "Iron ore available",        Color.Gray);
            Check("Farming",    player.FarmingLevel,       "Green Sprout",   "Crops grow 10% faster",     Color.Green);
            Check("Farming",    player.FarmingLevel,       "Rain Dancer",    "Crops grow in rain w/o water", Color.Green);
            Check("Cooking",    player.CookingLevel,       "Sous Chef",      "Meals heal +5 extra HP",    new Color((byte)220,(byte)150,(byte)40,(byte)255));
            Check("Athletics",  player.AthleticsLevel,     "Light Feet",     "+10% move speed",           new Color((byte)200,(byte)200,(byte)80,(byte)255));
            Check("Athletics",  player.AthleticsLevel,     "Marathon",       "Sprint drains no stamina",  new Color((byte)200,(byte)200,(byte)80,(byte)255));
            Check("Driving",    player.DrivingLevel,       "Fuel Saver",     "Vehicles use 10% less fuel",new Color((byte)100,(byte)160,(byte)220,(byte)255));
            Check("Swimming",   player.SwimmingLevel,      "Strong Stroke",  "+15% swim speed",           new Color((byte)40,(byte)180,(byte)220,(byte)255));
            Check("Strength",   player.StrengthLevel,      "Pack Mule",      "+5 backpack slots",         new Color((byte)200,(byte)140,(byte)60,(byte)255));
        }


        // ═════════════════════════════════════════════════════════════════
        //  4. ENHANCED STATS SUMMARY PAGE
        // ═════════════════════════════════════════════════════════════════

        static void DrawEnhancedStats(int x, int y, int w)
        {
            DrawTextUI("PLAYER STATISTICS", x, y, 34, Color.Gold);

            int ly = y + 50;

            // ── Playtime ──
            int hours = (int)(totalPlayTime / 3600);
            int mins = (int)((totalPlayTime % 3600) / 60);
            DrawStatRow(x, ly, w, "Total Playtime", $"{hours}h {mins}m", Color.White); ly += 28;

            // ── Exploration ──
            DrawTextUI("EXPLORATION", x, ly, 22, new Color((byte)100,(byte)200,(byte)100,(byte)255)); ly += 28;
            DrawStatRow(x, ly, w, "Zones Discovered", $"{discoveredZones.Count}", Color.Green); ly += 24;
            DrawStatRow(x, ly, w, "Map Explored", $"{GetExplorationPercent():F1}%", Color.Green); ly += 24;
            DrawStatRow(x, ly, w, "Collectables Found", $"{CollectablesFound}", Color.Green); ly += 30;

            // ── Combat ──
            DrawTextUI("COMBAT", x, ly, 22, new Color((byte)220,(byte)80,(byte)60,(byte)255)); ly += 28;
            int totalKills = bestiary.Values.Sum(e => e.Kills);
            int speciesFound = bestiary.Values.Count(e => e.Discovered);
            DrawStatRow(x, ly, w, "Enemies Defeated", $"{totalKills}", Color.Red); ly += 24;
            DrawStatRow(x, ly, w, "Bestiary Entries", $"{speciesFound}/{bestiary.Count}", Color.Red); ly += 24;
            DrawStatRow(x, ly, w, "Dungeons Cleared", $"{dungeonsCleared}", Color.Red); ly += 30;

            // ── Economy ──
            DrawTextUI("ECONOMY", x, ly, 22, Color.Gold); ly += 28;
            DrawStatRow(x, ly, w, "Current Cash", $"${player.Money}", Color.Gold); ly += 24;
            DrawStatRow(x, ly, w, "Bank Balance", $"${player.BankBalance}", Color.Gold); ly += 24;
            var (repTitle, _) = GetReputationTier(player.Reputation);
            DrawStatRow(x, ly, w, "Reputation", $"{player.Reputation} ({repTitle})", Color.Gold); ly += 30;

            // ── Social ──
            DrawTextUI("SOCIAL", x, ly, 22, new Color((byte)220,(byte)120,(byte)180,(byte)255)); ly += 28;
            int friends = friendNPCs.Count(f => f.Friendship >= 30);
            int closeFriends = friendNPCs.Count(f => f.Friendship >= 60);
            int bestFriends = friendNPCs.Count(f => f.Friendship >= 90);
            DrawStatRow(x, ly, w, "Friends", $"{friends}", new Color((byte)220,(byte)120,(byte)180,(byte)255)); ly += 24;
            DrawStatRow(x, ly, w, "Close Friends", $"{closeFriends}", new Color((byte)220,(byte)120,(byte)180,(byte)255)); ly += 24;
            DrawStatRow(x, ly, w, "Best Friends", $"{bestFriends}", new Color((byte)220,(byte)120,(byte)180,(byte)255)); ly += 30;

            // ── Gathering ──
            DrawTextUI("GATHERING", x, ly, 22, new Color((byte)140,(byte)200,(byte)80,(byte)255)); ly += 28;
            DrawStatRow(x, ly, w, "Crops Harvested", $"{cropsHarvested}", new Color((byte)140,(byte)200,(byte)80,(byte)255)); ly += 24;
            DrawStatRow(x, ly, w, "Meals Cooked", $"{mealsCooked}", new Color((byte)140,(byte)200,(byte)80,(byte)255)); ly += 24;
            DrawStatRow(x, ly, w, "Fish Caught", $"{player.Fish}+", new Color((byte)140,(byte)200,(byte)80,(byte)255)); ly += 30;

            // ── Activities ──
            DrawTextUI("ACTIVITIES", x, ly, 22, new Color((byte)180,(byte)160,(byte)220,(byte)255)); ly += 28;
            int sportsPlayed = sportPlayCounts.Values.Sum();
            int gamesPlayed = minigamePlayCounts.Values.Sum();
            DrawStatRow(x, ly, w, "Sports Matches", $"{sportsPlayed}", new Color((byte)180,(byte)160,(byte)220,(byte)255)); ly += 24;
            DrawStatRow(x, ly, w, "Minigames Played", $"{gamesPlayed}", new Color((byte)180,(byte)160,(byte)220,(byte)255)); ly += 24;
            int achievementsPct = achievements.Count > 0 ? (int)(100f * achievementsUnlockedCount / achievements.Count) : 0;
            DrawStatRow(x, ly, w, "Achievements", $"{achievementsUnlockedCount}/{achievements.Count} ({achievementsPct}%)",
                new Color((byte)180,(byte)160,(byte)220,(byte)255)); ly += 30;

            // ── Skills summary (compact, two columns) ──
            DrawTextUI("SKILLS", x, ly, 22, Color.Gold); ly += 28;
            int col0 = x, col1 = x + w / 2;
            int half = (cheatSkills.Length + 1) / 2;
            for (int i = 0; i < cheatSkills.Length; i++)
            {
                var s = cheatSkills[i];
                int colX = i < half ? col0 : col1;
                int rowY = ly + (i % half) * 24;
                int lv = s.get();
                DrawTextUI($"{s.name}: Lv {lv}", colX, rowY, 15, lv >= 50 ? Color.Gold : Color.White);
                Raylib.DrawRectangle(colX + 200, rowY + 4, 80, 5, new Color((byte)40, (byte)40, (byte)40, (byte)255));
                Raylib.DrawRectangle(colX + 200, rowY + 4, (int)(80 * Math.Min(1f, lv / 100f)), 5, Color.Gold);
            }
        }

        static void DrawStatRow(int x, int y, int w, string label, string value, Color col)
        {
            DrawTextUI(label, x + 16, y, 16, Color.LightGray);
            int vw = MeasureTextUI(value, 16);
            DrawTextUI(value, x + w / 2 - vw - 20, y, 16, col);
        }


        // ═════════════════════════════════════════════════════════════════
        //  5. TOOLBAR TOOLTIP SYSTEM
        // ═════════════════════════════════════════════════════════════════

        static void DrawToolbarTooltip()
        {
            if (pauseMenuOpen || playerMenuOpen || journalOpen) return;

            Vector2 mouse = Raylib.GetMousePosition();

            int slotSize = 72;
            int startX = 20;
            int startY = ScreenHeight - slotSize - 20;

            for (int i = 0; i < 8; i++)
            {
                Rectangle slot = new Rectangle(startX + i * slotSize, startY, slotSize, slotSize);
                if (!Raylib.CheckCollisionPointRec(mouse, slot)) continue;

                string item = toolbarSlots[i];
                if (item == null) continue;

                // build tooltip lines
                string line1 = item;
                string line2 = GetItemTooltipInfo(item);
                int count = toolbarCounts[i];
                string line3 = count > 1 ? $"Qty: {count}" : "";

                int maxW = Math.Max(MeasureTextUI(line1, 16),
                    Math.Max(MeasureTextUI(line2, 13), MeasureTextUI(line3, 13)));
                int tipW = maxW + 20;
                int tipH = 52 + (line3.Length > 0 ? 16 : 0);
                int tipX = (int)slot.X;
                int tipY = startY - tipH - 8;

                // clamp to screen
                if (tipX + tipW > ScreenWidth) tipX = ScreenWidth - tipW - 4;

                Raylib.DrawRectangle(tipX, tipY, tipW, tipH,
                    new Color((byte)10, (byte)10, (byte)20, (byte)230));
                Raylib.DrawRectangleLinesEx(new Rectangle(tipX, tipY, tipW, tipH), 1, Color.Gold);

                DrawTextUI(line1, tipX + 8, tipY + 6, 16, Color.White);
                DrawTextUI(line2, tipX + 8, tipY + 26, 13, Color.LightGray);
                if (line3.Length > 0)
                    DrawTextUI(line3, tipX + 8, tipY + 42, 13, Color.Gold);

                break; // only one tooltip at a time
            }
        }

        static string GetItemTooltipInfo(string item)
        {
            if (item == null) return "";

            // weapons
            if (item.Contains("Sword"))   return "Melee weapon — SPACE to swing";
            if (item.Contains("War Axe")) return "2H melee weapon — heavy damage";
            if (item.Contains("Great Sword")) return "2H melee — wide sweeping attacks";
            if (item.Contains("Bow"))     return "Ranged weapon — SPACE to fire arrows";
            if (item.Contains("Crossbow"))return "Ranged weapon — fires bolts";
            if (item.Contains("Staff"))   return "Magic weapon — casts spells";
            if (item.Contains("Stick"))   return "Basic melee weapon";

            // tools
            if (item.Contains("Axe"))     return "Chop trees for logs — SPACE near tree";
            if (item.Contains("Pickaxe")) return "Mine rocks for ore — SPACE near rock";
            if (item.Contains("Rod"))     return "Fish at water — SPACE near lake/ocean";
            if (item.Contains("Net"))     return "Net fish at water — SPACE near lake";
            if (item.Contains("Spade"))   return "Till soil on farm plots — SPACE";
            if (item.Contains("Watering"))return "Water crops — SPACE on tilled plot";
            if (item.Contains("Torch"))   return "Lights your way at night";

            // food & cooking
            if (item.Contains("Cooked"))  return "Eat for HP — SPACE to consume";
            if (item == "Raw Fish")       return "Cook at a fire first!";

            // crafting materials
            if (item.Contains("Logs"))    return "Building material — craft at workbench";
            if (item.Contains("Ore"))     return "Smelt in furnace for bars";
            if (item.Contains("Bar"))     return "Crafting material — used at anvil";
            if (item.Contains("Seeds"))   return "Plant on tilled farm plot";

            // ammo
            if (item == "Arrows")         return "Ammo for bows";
            if (item == "Bolts")          return "Ammo for crossbows";

            // placeable
            if (item == "Workbench")      return "Place in world — SPACE to set down";
            if (item == "Chest")          return "Placeable storage — 20 slots";
            if (item == "Campfire")       return "Cook anywhere — place then use";
            if (item == "Waypoint Flag")  return "Marks your map and minimap";
            if (item == "Furnace")        return "Smelts ore into bars";
            if (item == "Anvil")          return "Forge metal armor";

            if (item == "Hay")            return "Feed for donkeys, camels, reindeer";
            if (item == "Canteen")        return "Fill at water, boil at fire, then drink";

            return "Equip or use with SPACE";
        }


        // ═════════════════════════════════════════════════════════════════
        //  6. MORE STORY QUESTS
        // ═════════════════════════════════════════════════════════════════

        static void AddNewStoryQuests()
        {
            // only add if not already present (idempotent)
            if (storyQuests.Any(q => q.Title == "Desert Expedition")) return;

            storyQuests.Add(new StoryQuest
            {
                Title = "Desert Expedition",
                GiverName = "Ranger",
                Reward = 800,
                Stages = {
                    new QuestStage { Description = "Travel to the Desert zone",
                        Progress = () => discoveredZones.Contains("DESERT") ? 1 : 0, Target = 1 },
                    new QuestStage { Description = "Defeat 8 desert enemies",
                        Progress = () => bestiary.Values.Where(e =>
                            e.Location == "DESERT" || e.Location == "OASIS").Sum(e => e.Kills),
                        Target = 8 },
                    new QuestStage { Description = "Mine 5 Gold Ore",
                        Progress = () => player.GoldOre, Target = 5 },
                    new QuestStage { Description = "Return to the Ranger",
                        Progress = null },
                }
            });

            storyQuests.Add(new StoryQuest
            {
                Title = "Frozen Frontier",
                GiverName = "Ranger",
                Reward = 1000,
                Stages = {
                    new QuestStage { Description = "Discover the Snow Zone",
                        Progress = () => discoveredZones.Contains("SNOW ZONE") ? 1 : 0, Target = 1 },
                    new QuestStage { Description = "Chop 15 Arctic Logs",
                        Progress = () => player.ArcticLogs, Target = 15 },
                    new QuestStage { Description = "Survive a Blizzard event",
                        Progress = () => (activeWorldEvent?.Type == WorldEventType.Blizzard
                            && activeWorldEvent.Timer > 10f) ? 1 : 0,
                        Target = 1 },
                    new QuestStage { Description = "Return to the Ranger",
                        Progress = null },
                }
            });

            storyQuests.Add(new StoryQuest
            {
                Title = "Volcanic Heart",
                TriggerSpot = new Vector2(-50000, -150000),
                GiverName = "",
                Reward = 1500,
                Stages = {
                    new QuestStage { Description = "Discover the Volcano zone",
                        Progress = () => discoveredZones.Contains("VOLCANO") ? 1 : 0, Target = 1 },
                    new QuestStage { Description = "Collect 3 Lava Cores",
                        Progress = () => player.LavaCores, Target = 3 },
                    new QuestStage { Description = "Collect 5 Ember Stones",
                        Progress = () => player.EmberStones, Target = 5 },
                    new QuestStage { Description = "Clear a dungeon",
                        Progress = () => dungeonsCleared, Target = 1 },
                    new QuestStage { Description = "Return to the glowing marker",
                        Progress = null },
                }
            });
        }


        // ═════════════════════════════════════════════════════════════════
        //  COMBINED UPDATE (call from World update loop)
        // ═════════════════════════════════════════════════════════════════

        static void UpdatePolish(float dt)
        {
            UpdatePerkFlash(dt);
            CheckPerkActivations();

            // reset loading tip when fade ends
            if (sceneFadeAlpha <= 0f && currentTipIndex >= 0)
                ResetLoadingTip();
        }
    }
}
