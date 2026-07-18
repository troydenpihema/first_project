
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ═══════════════════════════════════════════════════════════════
        //  1. HIT-STOP (FREEZE FRAME) SYSTEM
        // ═══════════════════════════════════════════════════════════════
        // Call TriggerHitStop() when a melee hit lands. The game loop
        // skips dt for a few frames, giving attacks more weight.

        static float hitStopTimer = 0f;
        static bool IsHitStopped => hitStopTimer > 0f;

        static void TriggerHitStop(float duration = 0.045f)
        {
            hitStopTimer = Math.Max(hitStopTimer, duration);
        }

        static void UpdateHitStop(float dt)
        {
            if (hitStopTimer > 0f) hitStopTimer -= dt;
        }


        // ═══════════════════════════════════════════════════════════════
        //  2. DODGE ROLL
        // ═══════════════════════════════════════════════════════════════
        // Shift = short invincibility dash. Costs stamina.
        // Unlocked at Combat level 1 (basic) — perk at 50 adds i-frames.

        static bool   isDodging = false;
        static float  dodgeTimer = 0f;
        const  float  dodgeDuration = 0.25f;
        const  float  dodgeSpeed = 600f;
        const  float  dodgeCooldownMax = 0.6f;
        static float  dodgeCooldown = 0f;
        static Vector2 dodgeDirection = Vector2.Zero;
        static float  dodgeStaminaCost = 15f;

        static bool PlayerInvincible => isDodging && HasPerk("Combat", 50);

        static void TryDodgeRoll()
        {
            if (isDodging || dodgeCooldown > 0f) return;
            if (player.Stamina < dodgeStaminaCost) return;
            if (player.InventoryOpen || chestOpen || shopUIOpen || chatInputOpen) return;

            // direction from movement keys, fall back to facing
            Vector2 dir = Vector2.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    dir.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  dir.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  dir.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) dir.X += 1;

            if (dir == Vector2.Zero)
            {
                dir = player.Facing switch
                {
                    Player.FacingDirection.Up    => new Vector2(0, -1),
                    Player.FacingDirection.Down  => new Vector2(0,  1),
                    Player.FacingDirection.Left  => new Vector2(-1, 0),
                    Player.FacingDirection.Right => new Vector2(1,  0),
                    _ => new Vector2(0, 1)
                };
            }

            dodgeDirection = Vector2.Normalize(dir);
            isDodging = true;
            dodgeTimer = 0f;
            dodgeCooldown = dodgeCooldownMax;
            player.Stamina = Math.Max(0, player.Stamina - dodgeStaminaCost);
        }

        static void UpdateDodgeRoll(float dt)
        {
            if (dodgeCooldown > 0f) dodgeCooldown -= dt;

            if (!isDodging) return;

            dodgeTimer += dt;
            if (dodgeTimer >= dodgeDuration)
            {
                isDodging = false;
                return;
            }

            // move player at dodge speed
            float ease = 1f - (dodgeTimer / dodgeDuration); // fast start, slow end
            player.Position += dodgeDirection * dodgeSpeed * ease * dt;
        }

        static void DrawDodgeAfterimage()
        {
            if (!isDodging) return;

            float alpha = 1f - (dodgeTimer / dodgeDuration);
            byte a = (byte)(80 * alpha);
            // simple trail: draw 2 faded circles at offset positions behind the roll
            for (int i = 1; i <= 2; i++)
            {
                Vector2 trail = player.Position - dodgeDirection * (i * 20f);
                Raylib.DrawCircleV(trail + new Vector2(20, 20), 16,
                    new Color(player.ShirtColor.R, player.ShirtColor.G, player.ShirtColor.B, (byte)(a / i)));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  3. ENEMY TELEGRAPH / WIND-UP SYSTEM
        // ═══════════════════════════════════════════════════════════════
        // Enemies flash a warning color and pause briefly before attacking.
        // This gives the player a reaction window to dodge.

        static void DrawEnemyTelegraph(Enemy enemy)
        {
            if (enemy.Dead || enemy.AttackCooldown > 0.3f || enemy.AttackCooldown <= 0f) return;

            // when cooldown is between 0 and 0.3s, the enemy is "winding up"
            float t = 1f - (enemy.AttackCooldown / 0.3f); // 0→1 as attack approaches
            byte alpha = (byte)(120 + (int)(135 * t));

            // pulsing red circle = "about to attack"
            float radius = 24f + t * 12f;
            Raylib.DrawCircleLines((int)enemy.Center.X, (int)enemy.Center.Y, radius,
                new Color((byte)255, (byte)40, (byte)40, alpha));

            // exclamation mark
            if (t > 0.5f)
            {
                Raylib.DrawText("!", (int)enemy.Center.X - 4, (int)enemy.Center.Y - 36, 20,
                    new Color((byte)255, (byte)60, (byte)60, alpha));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  4. STATUS EFFECTS
        // ═══════════════════════════════════════════════════════════════
        // Enemies can inflict poison, burn, slow on contact.
        // Effects tick every second and have a duration.

        enum StatusType { Poison, Burn, Slow }

        struct ActiveStatus
        {
            public StatusType Type;
            public float Duration;    // remaining seconds
            public float TickTimer;   // counts to 1s then applies
        }

        static List<ActiveStatus> playerStatuses = new();

        static void InflictStatus(StatusType type, float duration)
        {
            // refresh if already active, don't stack
            for (int i = 0; i < playerStatuses.Count; i++)
            {
                if (playerStatuses[i].Type == type)
                {
                    var s = playerStatuses[i];
                    s.Duration = Math.Max(s.Duration, duration);
                    playerStatuses[i] = s;
                    return;
                }
            }
            playerStatuses.Add(new ActiveStatus { Type = type, Duration = duration, TickTimer = 0f });
        }

        static bool HasStatus(StatusType type) =>
            playerStatuses.Any(s => s.Type == type);

        static float GetSlowMultiplier() =>
            HasStatus(StatusType.Slow) ? 0.5f : 1f;

        static void UpdateStatusEffects(float dt)
        {
            for (int i = playerStatuses.Count - 1; i >= 0; i--)
            {
                var s = playerStatuses[i];
                s.Duration -= dt;
                s.TickTimer += dt;

                if (s.Duration <= 0f) { playerStatuses.RemoveAt(i); continue; }

                if (s.TickTimer >= 1f)
                {
                    s.TickTimer = 0f;
                    switch (s.Type)
                    {
                        case StatusType.Poison:
                            player.Health = Math.Max(1, player.Health - 3); // never kills
                            floatingTexts.Add(new FloatingText {
                                Position = player.Position - new Vector2(0, 30),
                                Text = "-3 poison", Timer = 0.8f,
                                TextColor = new Color((byte)80, (byte)200, (byte)40, (byte)255)
                            });
                            break;
                        case StatusType.Burn:
                            player.Health = Math.Max(1, player.Health - 5);
                            floatingTexts.Add(new FloatingText {
                                Position = player.Position - new Vector2(0, 30),
                                Text = "-5 burn", Timer = 0.8f,
                                TextColor = new Color((byte)255, (byte)120, (byte)20, (byte)255)
                            });
                            break;
                        // Slow doesn't deal damage — it's handled via GetSlowMultiplier()
                    }
                }

                playerStatuses[i] = s;
            }
        }

        static void DrawStatusEffectIcons()
        {
            int ix = 10;
            int iy = ScreenHeight - 160;
            foreach (var s in playerStatuses)
            {
                Color col = s.Type switch
                {
                    StatusType.Poison => new Color((byte)80, (byte)200, (byte)40, (byte)255),
                    StatusType.Burn   => new Color((byte)255, (byte)120, (byte)20, (byte)255),
                    StatusType.Slow   => new Color((byte)100, (byte)160, (byte)255, (byte)255),
                    _ => Color.White
                };
                string label = s.Type switch
                {
                    StatusType.Poison => "PSN",
                    StatusType.Burn   => "BRN",
                    StatusType.Slow   => "SLW",
                    _ => "?"
                };

                Raylib.DrawRectangle(ix, iy, 36, 36, new Color((byte)0, (byte)0, (byte)0, (byte)160));
                Raylib.DrawRectangleLines(ix, iy, 36, 36, col);
                DrawTextUI(label, ix + 4, iy + 4, 12, col);
                DrawTextUI($"{(int)s.Duration}s", ix + 4, iy + 20, 10, Color.White);
                ix += 42;
            }
        }

        /// Returns the status a given enemy type inflicts on contact, or null.
        static (StatusType type, float duration)? GetEnemyStatusInflict(string enemyType) => enemyType switch
        {
            "Snake"       => (StatusType.Poison, 6f),
            "Scorpion"    => (StatusType.Poison, 5f),
            "Fire Lizard" => (StatusType.Burn,   4f),
            "Magma Beetle"=> (StatusType.Burn,   5f),
            "Crab"        => (StatusType.Slow,   3f),
            "Crocodile"   => (StatusType.Slow,   4f),
            _ => null
        };


        // ═══════════════════════════════════════════════════════════════
        //  5. LOOT MAGNET — items lerp toward player
        // ═══════════════════════════════════════════════════════════════

        const float lootMagnetRange = 120f;
        const float lootMagnetSpeed = 280f;

        static void UpdateLootMagnet(float dt)
        {
            foreach (var drop in lootDrops)
            {
                if (drop.Collected) continue;
                float dist = Vector2.Distance(player.Center, drop.Position);
                if (dist < lootMagnetRange && dist > 5f)
                {
                    Vector2 dir = Vector2.Normalize(player.Center - drop.Position);
                    // accelerate as it gets closer
                    float speed = lootMagnetSpeed * (1f - dist / lootMagnetRange);
                    drop.Position += dir * speed * dt;
                }
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  6. ENVIRONMENTAL PARTICLES
        // ═══════════════════════════════════════════════════════════════
        // Reuses your existing Splat struct for lightweight particles.

        struct EnvParticle
        {
            public Vector2 Pos;
            public Vector2 Vel;
            public float Life, MaxLife;
            public float Size;
            public Color Col;
        }

        static List<EnvParticle> envParticles = new();
        static float envParticleTimer = 0f;

        static void UpdateEnvParticles(float dt)
        {
            envParticleTimer += dt;

            // spawn particles based on player activity & biome
            if (envParticleTimer >= 0.15f)
            {
                envParticleTimer = 0f;
                string biome = currentBiome ?? "";

                // dust when running on sand/desert
                bool moving = player.isSwinging == false &&
                    (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.S) ||
                     Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.D));

                if (moving && (biome == "DESERT" || biome == "BEACH"))
                {
                    SpawnEnvParticle(
                        player.Position + new Vector2(20 + Raylib.GetRandomValue(-8, 8), 40),
                        new Vector2(Raylib.GetRandomValue(-20, 20), Raylib.GetRandomValue(-30, -10)),
                        new Color((byte)200, (byte)180, (byte)130, (byte)140),
                        0.5f, Raylib.GetRandomValue(2, 4));
                }

                // snow flakes in snow biome
                if (biome == "SNOW ZONE")
                {
                    Vector2 snowPos = player.Position + new Vector2(
                        Raylib.GetRandomValue(-300, 300), Raylib.GetRandomValue(-200, 200));
                    SpawnEnvParticle(snowPos,
                        new Vector2(Raylib.GetRandomValue(-15, 15), Raylib.GetRandomValue(20, 50)),
                        new Color((byte)230, (byte)240, (byte)255, (byte)180),
                        2f, Raylib.GetRandomValue(2, 4));
                }

                // leaves in forest
                if (biome == "FOREST" && Raylib.GetRandomValue(1, 3) == 1)
                {
                    Vector2 leafPos = player.Position + new Vector2(
                        Raylib.GetRandomValue(-250, 250), Raylib.GetRandomValue(-200, -50));
                    SpawnEnvParticle(leafPos,
                        new Vector2(Raylib.GetRandomValue(-30, 30), Raylib.GetRandomValue(15, 40)),
                        new Color((byte)80, (byte)160, (byte)50, (byte)160),
                        2.5f, Raylib.GetRandomValue(2, 4));
                }

                // embers in volcano
                if (biome == "VOLCANO" && Raylib.GetRandomValue(1, 2) == 1)
                {
                    Vector2 emberPos = player.Position + new Vector2(
                        Raylib.GetRandomValue(-200, 200), Raylib.GetRandomValue(-100, 100));
                    SpawnEnvParticle(emberPos,
                        new Vector2(Raylib.GetRandomValue(-10, 10), Raylib.GetRandomValue(-60, -20)),
                        new Color((byte)255, (byte)140, (byte)30, (byte)200),
                        1.5f, Raylib.GetRandomValue(2, 3));
                }

                // bubbles when swimming
                if (player.IsSwimming && Raylib.GetRandomValue(1, 3) == 1)
                {
                    SpawnEnvParticle(
                        player.Position + new Vector2(20 + Raylib.GetRandomValue(-10, 10), 10),
                        new Vector2(Raylib.GetRandomValue(-5, 5), Raylib.GetRandomValue(-40, -20)),
                        new Color((byte)180, (byte)220, (byte)255, (byte)140),
                        1f, Raylib.GetRandomValue(2, 4));
                }
            }

            // tick particles
            for (int i = envParticles.Count - 1; i >= 0; i--)
            {
                var p = envParticles[i];
                p.Life -= dt;
                if (p.Life <= 0f) { envParticles.RemoveAt(i); continue; }
                p.Pos += p.Vel * dt;
                envParticles[i] = p;
            }
        }

        static void SpawnEnvParticle(Vector2 pos, Vector2 vel, Color col, float life, int size)
        {
            if (envParticles.Count > 120) return; // cap
            envParticles.Add(new EnvParticle {
                Pos = pos, Vel = vel, Col = col,
                Life = life, MaxLife = life, Size = size
            });
        }

        static void DrawEnvParticles()
        {
            foreach (var p in envParticles)
            {
                float alpha = p.Life / p.MaxLife;
                Raylib.DrawCircleV(p.Pos, p.Size,
                    new Color(p.Col.R, p.Col.G, p.Col.B, (byte)(p.Col.A * alpha)));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  7. DEATH SCREEN OVERLAY
        // ═══════════════════════════════════════════════════════════════
        // Instead of instant teleport on death, show a brief "YOU DIED"
        // screen with fade, then respawn. 

        static bool   deathScreenActive = false;
        static float  deathScreenTimer  = 0f;
        const  float  deathScreenDuration = 2.5f;
        static int    deathPenaltyMoney = 50;
        static string deathMessage = "YOU DIED";

        static void TriggerDeathScreen()
        {
            deathScreenActive = true;
            deathScreenTimer = 0f;

            // immortal warrior perk: survive lethal once per day
            if (HasPerk("Combat", 100) && !usedImmortalToday)
            {
                usedImmortalToday = true;
                player.Health = 1;
                deathScreenActive = false;
                ShowNotification("Immortal Warrior saved you from death!");
                TriggerPerkFlash("Immortal Warrior", "Survived a lethal blow!", Color.Gold);
                return;
            }

            // berserker flavor message
            deathMessage = player.CombatLevel >= 50
                ? "FALLEN IN BATTLE"
                : "YOU DIED";
        }

        static bool usedImmortalToday = false;

        static void UpdateDeathScreen(float dt)
        {
            if (!deathScreenActive) return;

            deathScreenTimer += dt;
            if (deathScreenTimer >= deathScreenDuration)
            {
                deathScreenActive = false;

                // respawn logic (replaces the instant teleport)
                player.Health = player.MaxHealth;
                playerStatuses.Clear(); // clear all status effects on death

                var hospital = buildings.FirstOrDefault(b => b.BuildingName == "HOSPITAL");
                if (hospital != null)
                {
                    currentBuilding = hospital;
                    player.Position = new Vector2(100, 750);
                    ChangeScene(SceneState.Building);
                }
                else
                {
                    player.Position = new Vector2(550, -50);
                }

                deathPenaltyMoney = Math.Min(50 + player.CombatLevel * 2, player.Money);
                player.Money = Math.Max(0, player.Money - deathPenaltyMoney);
                ShowLevelUp($"Lost ${deathPenaltyMoney} on death", 0);
            }
        }

        static void DrawDeathScreen()
        {
            if (!deathScreenActive) return;

            float t = deathScreenTimer / deathScreenDuration; // 0→1
            // fade in red overlay
            byte alpha = (byte)(Math.Min(1f, t * 3f) * 200);
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight,
                new Color((byte)40, (byte)0, (byte)0, alpha));

            if (t > 0.3f)
            {
                // "YOU DIED" text slides down and fades in
                float textAlpha = Math.Min(1f, (t - 0.3f) * 3f);
                byte ta = (byte)(255 * textAlpha);
                int tw = MeasureTextUI(deathMessage, 60);
                int ty = (int)(ScreenHeight / 2 - 60 + (1f - textAlpha) * 40); // slide down

                DrawTextUI(deathMessage, ScreenWidth / 2 - tw / 2, ty, 60,
                    new Color((byte)200, (byte)30, (byte)30, ta));

                // subtitle
                if (t > 0.6f)
                {
                    string sub = $"Respawning... (-${deathPenaltyMoney})";
                    int sw = MeasureTextUI(sub, 20);
                    byte sa = (byte)(255 * Math.Min(1f, (t - 0.6f) * 4f));
                    DrawTextUI(sub, ScreenWidth / 2 - sw / 2, ty + 70, 20,
                        new Color((byte)200, (byte)200, (byte)200, sa));
                }
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  8. AMBIENT NPC SPEECH BUBBLES
        // ═══════════════════════════════════════════════════════════════

        static float ambientBubbleTimer = 0f;

        static readonly Dictionary<string, string[]> activityQuips = new()
        {
            ["Farming"]      = new[] { "Beautiful day for planting!", "These crops are coming along nicely.", "Need more rain...", "Time to check the irrigation." },
            ["Fishing"]      = new[] { "The sea provides, if you're patient.", "Big one's out there today!", "Should've brought more bait.", "Kia ora, the fish are biting!" },
            ["Mending nets"]  = new[] { "These nets won't fix themselves.", "Almost done here...", "Good nets, good catch." },
            ["At the tavern"] = new[] { "Cheers!", "Long day, cold drink.", "Another round!", "What a night!" },
            ["Patrolling"]   = new[] { "All clear so far.", "Stay safe, traveller.", "Keeping watch.", "Nothing gets past me." },
            ["Eating lunch"]  = new[] { "Mmm, good kai!", "Taking a break.", "Need the energy.", "Best part of the day." },
            ["Chopping wood"] = new[] { "Timber!", "Strong wood today.", "Chop chop!", "Almost got enough." },
            ["Sleeping"]     = new string[] { },  // no quips while sleeping
        };

        static void UpdateAmbientBubbles(float dt)
        {
            ambientBubbleTimer += dt;
            if (ambientBubbleTimer < 8f) return; // every 8 seconds
            ambientBubbleTimer = 0f;

            foreach (var npc in npcs)
            {
                if (npc.Hidden || npc.DrawInsideNow) continue;
                float dist = Vector2.Distance(player.Center, npc.Position);
                if (dist > 300f || dist < 30f) continue;

                // 20% chance per nearby NPC per cycle
                if (Raylib.GetRandomValue(1, 100) > 20) continue;

                string activity = npc.CurrentActivity ?? "";
                if (activityQuips.TryGetValue(activity, out var quips) && quips.Length > 0)
                {
                    npc.Dialogue = quips[Raylib.GetRandomValue(0, quips.Length - 1)];
                }
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  9. COOKING BUFF SYSTEM
        // ═══════════════════════════════════════════════════════════════
        // Eating cooked meals grants a temporary buff lasting a few minutes.

        struct CookingBuff
        {
            public string Name;
            public float Duration;
            public int BonusDamage;
            public int BonusDefense;
            public float SpeedBoost; // multiplier, e.g. 1.1 = +10%
            public float XPBoost;    // multiplier, e.g. 1.15 = +15%
        }

        static CookingBuff? activeCookingBuff = null;

        static void ApplyCookingBuff(string mealName)
        {
            var buff = mealName switch
            {
                "Cooked Fish" => new CookingBuff {
                    Name = "Well Fed", Duration = 180f,
                    BonusDamage = 0, BonusDefense = 1, SpeedBoost = 1.0f, XPBoost = 1.1f },
                "Cooked Meat" => new CookingBuff {
                    Name = "Hearty Meal", Duration = 240f,
                    BonusDamage = 2, BonusDefense = 2, SpeedBoost = 1.0f, XPBoost = 1.0f },
                "Fish Stew" => new CookingBuff {
                    Name = "Stew Strength", Duration = 300f,
                    BonusDamage = 3, BonusDefense = 1, SpeedBoost = 1.05f, XPBoost = 1.15f },
                "Veggie Wrap" => new CookingBuff {
                    Name = "Green Energy", Duration = 240f,
                    BonusDamage = 0, BonusDefense = 0, SpeedBoost = 1.1f, XPBoost = 1.2f },
                "Berry Pie" => new CookingBuff {
                    Name = "Sugar Rush", Duration = 120f,
                    BonusDamage = 0, BonusDefense = 0, SpeedBoost = 1.15f, XPBoost = 1.0f },
                "Meat Pie" => new CookingBuff {
                    Name = "Warrior's Feast", Duration = 300f,
                    BonusDamage = 4, BonusDefense = 3, SpeedBoost = 1.0f, XPBoost = 1.0f },
                _ => new CookingBuff {
                    Name = "Nourished", Duration = 120f,
                    BonusDamage = 1, BonusDefense = 0, SpeedBoost = 1.0f, XPBoost = 1.05f },
            };

            // cooking level bonus: higher level = longer buffs
            buff.Duration *= 1f + player.CookingLevel * 0.005f;
            activeCookingBuff = buff;

            ShowNotification($"Buff: {buff.Name} ({(int)buff.Duration}s)");
        }

        static void UpdateCookingBuff(float dt)
        {
            if (activeCookingBuff == null) return;
            var b = activeCookingBuff.Value;
            b.Duration -= dt;
            if (b.Duration <= 0f)
            {
                activeCookingBuff = null;
                ShowNotification("Food buff expired.");
                return;
            }
            activeCookingBuff = b;
        }

        static int  GetBuffBonusDamage()  => activeCookingBuff?.BonusDamage ?? 0;
        static int  GetBuffBonusDefense() => activeCookingBuff?.BonusDefense ?? 0;
        static float GetBuffSpeedMult()   => activeCookingBuff?.SpeedBoost ?? 1f;
        static float GetBuffXPMult()      => activeCookingBuff?.XPBoost ?? 1f;

        static void DrawCookingBuffHUD()
        {
            if (activeCookingBuff == null) return;
            var b = activeCookingBuff.Value;

            int bx = ScreenWidth - 170, by = 130;
            Raylib.DrawRectangle(bx, by, 158, 34,
                new Color((byte)0, (byte)0, (byte)0, (byte)160));
            Raylib.DrawRectangleLines(bx, by, 158, 34,
                new Color((byte)220, (byte)180, (byte)60, (byte)200));

            int mins = (int)(b.Duration / 60);
            int secs = (int)(b.Duration % 60);
            DrawTextUI($"{b.Name} {mins}:{secs:D2}", bx + 6, by + 8, 14,
                new Color((byte)255, (byte)220, (byte)80, (byte)255));
        }


        // ═══════════════════════════════════════════════════════════════
        //  10. KEYBIND REFERENCE PANEL (F1)
        // ═══════════════════════════════════════════════════════════════

        static bool keybindPanelOpen = false;

        static readonly (string key, string action)[] keybindList =
        {
            ("WASD / Arrows", "Move"),
            ("Space / LMB",   "Attack / Use tool"),
            ("Shift",         "Dodge roll"),
            ("TAB",           "Inventory"),
            ("M",             "World map"),
            ("G",             "Equipment"),
            ("C",             "Crafting menu"),
            ("J",             "Journal"),
            ("T",             "Toggle Tools / Combat"),
            ("E",             "Interact / Pick up"),
            ("F",             "Collect dropped items"),
            ("R",             "Refuel / Reload"),
            ("H",             "Deliver quest item"),
            ("Q",             "Close / Exit building"),
            ("1-8",           "Toolbar slots"),
            ("F1",            "This keybind panel"),
            ("F5",            "Quick save"),
            ("F8",            "Skill cheats (debug)"),
            ("ESC",           "Pause menu"),
            ("Enter",         "Open chat"),
        };

        static void DrawKeybindPanel()
        {
            if (!keybindPanelOpen) return;

            int pw = 420, ph = keybindList.Length * 24 + 60;
            int px = ScreenWidth / 2 - pw / 2;
            int py = ScreenHeight / 2 - ph / 2;

            Raylib.DrawRectangle(px, py, pw, ph,
                new Color((byte)10, (byte)10, (byte)20, (byte)240));
            Raylib.DrawRectangleLines(px, py, pw, ph, Color.Gold);
            DrawTextUI("CONTROLS  (F1 to close)", px + 10, py + 10, 22, Color.Gold);

            for (int i = 0; i < keybindList.Length; i++)
            {
                int ry = py + 44 + i * 24;
                DrawTextUI(keybindList[i].key, px + 16, ry, 16, Color.White);
                DrawTextUI(keybindList[i].action, px + 200, ry, 16, Color.LightGray);
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  COMBINED UPDATE & DRAW — call from your game loops
        // ═══════════════════════════════════════════════════════════════

        static void UpdatePolish2(float dt)
        {
            UpdateHitStop(dt);
            UpdateDodgeRoll(dt);
            UpdateStatusEffects(dt);
            UpdateLootMagnet(dt);
            UpdateEnvParticles(dt);
            UpdateDeathScreen(dt);
            UpdateAmbientBubbles(dt);
            UpdateCookingBuff(dt);
            UpdateFishingIdleRipples(dt);

            // F1 toggle
            if (Raylib.IsKeyPressed(KeyboardKey.F1))
                keybindPanelOpen = !keybindPanelOpen;

            // Shift = dodge roll
            if (Raylib.IsKeyPressed(KeyboardKey.LeftShift) || Raylib.IsKeyPressed(KeyboardKey.RightShift))
                TryDodgeRoll();

            // reset immortal perk at day change (approximate — resets when hour crosses 6AM)
            if (GetCurrentHour() >= 6f && GetCurrentHour() < 6.1f)
                usedImmortalToday = false;
        }

        // ═══════════════════════════════════════════════════════════════
        //  11. MINING PARTICLES
        // ═══════════════════════════════════════════════════════════════
        // Sparks + rock chips fly out on each pickaxe hit.
        // Ore-colored sparkle burst on rock break.

        static Color GetOreSparkColor(string rockType) => rockType switch
        {
            "Copper"  => new Color((byte)200, (byte)120, (byte)50,  (byte)255),
            "Iron"    => new Color((byte)160, (byte)150, (byte)165, (byte)255),
            "Gold"    => new Color((byte)230, (byte)190, (byte)20,  (byte)255),
            "Crystal" => new Color((byte)180, (byte)220, (byte)240, (byte)255),
            _         => new Color((byte)150, (byte)150, (byte)155, (byte)255),  // Stone
        };

        /// Call when the pickaxe connects (every hit, not just break)
        static void SpawnMiningHitParticles(Vector2 rockPos, string rockType)
        {
            Color spark = GetOreSparkColor(rockType);
            Color chip  = new Color((byte)120, (byte)120, (byte)125, (byte)220);

            // 4–6 stone chips flying outward
            for (int i = 0; i < Raylib.GetRandomValue(4, 6); i++)
            {
                float ang = Raylib.GetRandomValue(180, 360) * (MathF.PI / 180f); // upward arc
                float spd = Raylib.GetRandomValue(60, 160);
                SpawnEnvParticle(rockPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    chip, 0.4f, Raylib.GetRandomValue(2, 4));
            }

            // 2–3 bright sparks (ore-colored)
            for (int i = 0; i < Raylib.GetRandomValue(2, 3); i++)
            {
                float ang = Raylib.GetRandomValue(200, 340) * (MathF.PI / 180f);
                float spd = Raylib.GetRandomValue(100, 200);
                SpawnEnvParticle(rockPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    spark, 0.3f, Raylib.GetRandomValue(1, 3));
            }
        }

        /// Call when the rock breaks — bigger burst of ore-colored sparkles
        static void SpawnMiningBreakParticles(Vector2 rockPos, string rockType)
        {
            Color spark = GetOreSparkColor(rockType);

            for (int i = 0; i < 12; i++)
            {
                float ang = Raylib.GetRandomValue(0, 360) * (MathF.PI / 180f);
                float spd = Raylib.GetRandomValue(80, 220);
                SpawnEnvParticle(rockPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    spark, 0.6f, Raylib.GetRandomValue(2, 5));
            }

            // dust cloud at the base
            for (int i = 0; i < 5; i++)
            {
                SpawnEnvParticle(
                    rockPos + new Vector2(Raylib.GetRandomValue(-12, 12), Raylib.GetRandomValue(0, 10)),
                    new Vector2(Raylib.GetRandomValue(-30, 30), Raylib.GetRandomValue(-20, -5)),
                    new Color((byte)160, (byte)150, (byte)130, (byte)120),
                    0.7f, Raylib.GetRandomValue(3, 6));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  12. WOODCUTTING PARTICLES
        // ═══════════════════════════════════════════════════════════════
        // Wood chips on each chop. Leaf shower + trunk dust on fell.

        static Color GetLeafColor(string treeType) => treeType switch
        {
            "Birch"  => new Color((byte)144, (byte)238, (byte)144, (byte)200),
            "Oak"    => new Color((byte)0,   (byte)100, (byte)0,   (byte)200),
            "Pine"   => new Color((byte)0,   (byte)80,  (byte)0,   (byte)200),
            "Arctic" => new Color((byte)200, (byte)230, (byte)255, (byte)200), // snow flakes
            "Dead"   => new Color((byte)140, (byte)120, (byte)90,  (byte)180),
            _        => new Color((byte)34,  (byte)139, (byte)34,  (byte)200), // Normal = forest green
        };

        /// Call on every axe hit
        static void SpawnChopHitParticles(Vector2 treeCenter, string treeType)
        {
            Color wood = new Color((byte)160, (byte)110, (byte)60, (byte)230);

            // 3–5 wood chips spraying from impact
            for (int i = 0; i < Raylib.GetRandomValue(3, 5); i++)
            {
                float ang = Raylib.GetRandomValue(150, 390) * (MathF.PI / 180f);
                float spd = Raylib.GetRandomValue(50, 140);
                SpawnEnvParticle(
                    treeCenter + new Vector2(0, 20), // trunk area
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    wood, 0.35f, Raylib.GetRandomValue(2, 4));
            }

            // 1–2 leaves shake loose from the canopy
            Color leaf = GetLeafColor(treeType);
            for (int i = 0; i < Raylib.GetRandomValue(1, 2); i++)
            {
                SpawnEnvParticle(
                    treeCenter + new Vector2(Raylib.GetRandomValue(-20, 20), Raylib.GetRandomValue(-30, -10)),
                    new Vector2(Raylib.GetRandomValue(-25, 25), Raylib.GetRandomValue(15, 40)),
                    leaf, 1.2f, Raylib.GetRandomValue(2, 4));
            }
        }

        /// Call when tree falls — big leaf shower + sawdust cloud
        static void SpawnTreeFellParticles(Vector2 treeCenter, string treeType)
        {
            Color leaf = GetLeafColor(treeType);

            // 15–20 leaves cascading down
            for (int i = 0; i < Raylib.GetRandomValue(15, 20); i++)
            {
                SpawnEnvParticle(
                    treeCenter + new Vector2(Raylib.GetRandomValue(-35, 35), Raylib.GetRandomValue(-40, -5)),
                    new Vector2(Raylib.GetRandomValue(-40, 40), Raylib.GetRandomValue(20, 60)),
                    leaf, Raylib.GetRandomValue(10, 20) / 10f, // 1.0–2.0s lifetime
                    Raylib.GetRandomValue(2, 5));
            }

            // sawdust cloud at trunk base
            for (int i = 0; i < 6; i++)
            {
                SpawnEnvParticle(
                    treeCenter + new Vector2(Raylib.GetRandomValue(-10, 10), 30),
                    new Vector2(Raylib.GetRandomValue(-35, 35), Raylib.GetRandomValue(-15, -5)),
                    new Color((byte)180, (byte)150, (byte)100, (byte)140),
                    0.8f, Raylib.GetRandomValue(3, 6));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  13. FISHING PARTICLES
        // ═══════════════════════════════════════════════════════════════
        // Ripples on cast, splash on bite, big splash on catch.

        /// Call when casting the line (phase 0 starts)
        static void SpawnFishingCastParticles(Vector2 waterPos)
        {
            // small splash ring
            for (int i = 0; i < 6; i++)
            {
                float ang = Raylib.GetRandomValue(0, 360) * (MathF.PI / 180f);
                float spd = Raylib.GetRandomValue(20, 60);
                SpawnEnvParticle(waterPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    new Color((byte)140, (byte)200, (byte)255, (byte)160),
                    0.6f, Raylib.GetRandomValue(2, 3));
            }
        }

        /// Call when a fish bites (phase 0→1 transition)
        static void SpawnFishingBiteParticles(Vector2 waterPos)
        {
            // sharp upward splash droplets
            for (int i = 0; i < 8; i++)
            {
                float ang = Raylib.GetRandomValue(200, 340) * (MathF.PI / 180f); // mostly upward
                float spd = Raylib.GetRandomValue(60, 140);
                SpawnEnvParticle(waterPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    new Color((byte)180, (byte)220, (byte)255, (byte)200),
                    0.5f, Raylib.GetRandomValue(2, 4));
            }
        }

        /// Call on successful catch — celebratory splash
        static void SpawnFishingCatchParticles(Vector2 playerPos)
        {
            // big water burst upward from the player
            for (int i = 0; i < 14; i++)
            {
                float ang = Raylib.GetRandomValue(180, 360) * (MathF.PI / 180f);
                float spd = Raylib.GetRandomValue(80, 200);
                SpawnEnvParticle(
                    playerPos + new Vector2(20, 0),
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
                    new Color((byte)100, (byte)200, (byte)255, (byte)180),
                    0.7f, Raylib.GetRandomValue(2, 5));
            }

            // a few shiny "fish scale" sparkles
            for (int i = 0; i < 4; i++)
            {
                SpawnEnvParticle(
                    playerPos + new Vector2(20 + Raylib.GetRandomValue(-15, 15), Raylib.GetRandomValue(-30, -10)),
                    new Vector2(Raylib.GetRandomValue(-20, 20), Raylib.GetRandomValue(-50, -20)),
                    new Color((byte)255, (byte)255, (byte)200, (byte)220),
                    0.5f, Raylib.GetRandomValue(1, 3));
            }
        }

        // Ongoing bobber ripples while waiting (phase 0) — call from UpdatePolish2
        static float bobberRippleTimer = 0f;

        static void UpdateFishingIdleRipples(float dt)
        {
            if (!isFishing || fishingPhase != 0) return;

            bobberRippleTimer += dt;
            if (bobberRippleTimer < 1.2f) return; // gentle ripple every 1.2s
            bobberRippleTimer = 0f;

            // approximate bobber position: ahead of the player in facing direction
            Vector2 bobberPos = player.Position + new Vector2(20, 0) + player.Facing switch
            {
                Player.FacingDirection.Up    => new Vector2(0, -50),
                Player.FacingDirection.Down  => new Vector2(0,  50),
                Player.FacingDirection.Left  => new Vector2(-50, 0),
                Player.FacingDirection.Right => new Vector2(50,  0),
                _ => new Vector2(0, 50)
            };

            // 2–3 tiny expanding ripple particles
            for (int i = 0; i < Raylib.GetRandomValue(2, 3); i++)
            {
                float ang = Raylib.GetRandomValue(0, 360) * (MathF.PI / 180f);
                SpawnEnvParticle(bobberPos,
                    new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * Raylib.GetRandomValue(8, 20),
                    new Color((byte)160, (byte)210, (byte)255, (byte)100),
                    0.8f, Raylib.GetRandomValue(1, 2));
            }
        }


        /// Call inside BeginMode2D (world-space particles)
        static void DrawPolish2World()
        {
            DrawDodgeAfterimage();
            DrawEnvParticles();

            // draw telegraph rings for nearby enemies
            foreach (var enemy in enemies)
            {
                if (!enemy.Dead && enemy.Aggro)
                    DrawEnemyTelegraph(enemy);
            }
        }

        /// Call after EndMode2D (screen-space HUD elements)
        static void DrawPolish2HUD()
        {
            DrawStatusEffectIcons();
            DrawCookingBuffHUD();
            DrawDeathScreen();
            DrawKeybindPanel();
        }
    }
}
