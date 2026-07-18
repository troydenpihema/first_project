using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ── ROLL FOR NEW EVENT ──────────────────────────────────────────────
        static void UpdateWorldEvents(float dt)
        {
            // tick cooldown
            if (worldEventCooldown > 0f) { worldEventCooldown -= dt; return; }

            // don't roll while an event is active
            if (activeWorldEvent != null)
            {
                UpdateActiveEvent(dt);
                return;
            }

            worldEventCheckTimer += dt;
            if (worldEventCheckTimer < 30f) return; // check every 30 real seconds
            worldEventCheckTimer = 0f;

            // 15% chance each check
            if (Raylib.GetRandomValue(1, 100) > 15) return;

            // pick a random event type
            var types = Enum.GetValues<WorldEventType>();
            var picked = types[Raylib.GetRandomValue(0, types.Length - 1)];

            TryStartWorldEvent(picked);
        }

        static void TryStartWorldEvent(WorldEventType type)
        {
            // Season restriction — some events only fire in certain seasons
            if (eventSeasons.TryGetValue(type, out var allowedSeasons)
                && !allowedSeasons.Contains(GetSeasonString()))
                return;

            string biome = GetCurrentBiome();
            var ev = new WorldEvent { Type = type, Timer = 0f, Completed = false, Announced = false };

            switch (type)
            {
                case WorldEventType.TravellingMerchant:
                    ev.Position = player.Position + RandomOffset(400, 800);
                    ev.Duration = 180f; // 3 real minutes
                    ev.Radius = 100f;
                    SpawnMerchantNPC(ev.Position);
                    break;

                case WorldEventType.TreasureGoblin:
                    ev.Position = player.Position + RandomOffset(300, 600);
                    ev.Duration = 60f;
                    ev.Radius = 80f;
                    SpawnTreasureGoblin(ev.Position);
                    break;

                case WorldEventType.GoblinRaid:
                    ev.Position = new Vector2(500, 0); // town centre
                    ev.Duration = 120f;
                    ev.Radius = 600f;
                    SpawnGoblinRaid(ev.Position, 6);
                    break;

                case WorldEventType.MeteorCrash:
                    // only in mountains or volcano
                    if (biome != "MOUNTAINS" && biome != "VOLCANO")
                        ev.Position = new Vector2(-28000 + Raylib.GetRandomValue(0, 8000),
                                                  -5000 + Raylib.GetRandomValue(0, 3000));
                    else
                        ev.Position = player.Position + RandomOffset(500, 1200);
                    ev.Duration = 300f;
                    ev.Radius = 200f;
                    break;

                case WorldEventType.BanditAttack:
                    ev.Position = player.Position + RandomOffset(200, 400);
                    ev.Duration = 90f;
                    ev.Radius = 400f;
                    SpawnBandits(ev.Position, 4);
                    break;

                case WorldEventType.ForestFire:
                    if (biome != "FOREST")
                        ev.Position = new Vector2(-5000 + Raylib.GetRandomValue(0, 4000), -14000);
                    else
                        ev.Position = player.Position + RandomOffset(300, 800);
                    ev.Duration = 150f;
                    ev.Radius = 500f;
                    break;

                case WorldEventType.Blizzard:
                    if (biome != "SNOW ZONE")
                        ev.Position = new Vector2(-35000, Raylib.GetRandomValue(-5000, 5000));
                    else
                        ev.Position = player.Position;
                    ev.Duration = 120f;
                    ev.Radius = 3000f;
                    break;

                case WorldEventType.DragonSighting:
                    ev.Position = player.Position + new Vector2(Raylib.GetRandomValue(-2000, 2000), -1500);
                    ev.Duration = 30f;
                    ev.Radius = 2000f;
                    break;

                case WorldEventType.LostChild:
                    ev.Position = player.Position + RandomOffset(400, 900);
                    ev.Duration = 180f;
                    ev.Radius = 80f;
                    SpawnLostChildNPC(ev.Position);
                    break;

                case WorldEventType.FishingTournament:
                    ev.Position = player.Position;
                    ev.Duration = 120f;
                    ev.Data = 0; // fish caught during event
                    break;

                case WorldEventType.HarvestFestival:
                    ev.Position = new Vector2(500, 0);
                    ev.Duration = 300f;
                    eventShopDiscount = 0.3f; // 30% off
                    break;

            }

            activeWorldEvent = ev;
            worldEventCooldown = 120f; // 2 min minimum between events
        }

        // ── UPDATE ACTIVE EVENT ─────────────────────────────────────────────
        static void UpdateActiveEvent(float dt)
        {
            var ev = activeWorldEvent;
            ev.Timer += dt;

            if (!ev.Announced)
            {
                ev.Announced = true;
                ShowNotification(GetEventAnnouncement(ev.Type));
            }

            switch (ev.Type)
            {
                case WorldEventType.TreasureGoblin:
                    // check if goblin is dead
                    if (eventEnemies.Count > 0 && eventEnemies[0].Dead)
                    {
                        if (!ev.Completed)
                        {
                            ev.Completed = true;
                            // bonus loot shower
                            for (int i = 0; i < 5; i++)
                                AddLootDrop(ev.Position + RandomOffset(10, 40), 
                                    i % 2 == 0 ? "Crystal" : "Gold Ore");
                            player.Money += 200;
                            AddReputation(30, "Treasure Goblin");
                            ShowNotification("Treasure Goblin defeated! Loot everywhere!");
                        }
                    }
                    else if (eventEnemies.Count > 0 && !eventEnemies[0].Dead)
                    {
                        // goblin runs away from player
                        var gob = eventEnemies[0];
                        float dist = Vector2.Distance(gob.Position, player.Center);
                        if (dist < 400f && dist > 5f)
                        {
                            Vector2 flee = Vector2.Normalize(gob.Position - player.Center);
                            gob.Position += flee * 130f * dt;
                        }
                    }
                    break;

                case WorldEventType.ForestFire:
                    // damage player if inside radius
                    if (Vector2.Distance(player.Center, ev.Position) < ev.Radius)
                    {
                        player.Health -= (int)(15 * dt);
                        if ((int)(ev.Timer * 4) % 2 == 0)
                            ShowNotification("You're in a forest fire! Move away!");
                    }
                    break;

                case WorldEventType.Blizzard:
                    if (Vector2.Distance(player.Center, ev.Position) < ev.Radius)
                    {
                        player.Health -= (int)(5 * dt);
                    }
                    break;


                case WorldEventType.DragonSighting:
                    // dragon flies across — handled in draw
                    break;

                case WorldEventType.LostChild:
                    if (eventNPC != null && Vector2.Distance(player.Center, eventNPC.Position) < 60f)
                    {
                        if (!ev.Completed)
                        {
                            ev.Completed = true;
                            player.Money += 150;
                            player.AddCombatXP(50);
                            AddReputation(40, "Lost Child");
                            ShowNotification("You found the lost child! Reward: $150");
                            eventNPC.Hidden = true;
                        }
                    }
                    break;

                case WorldEventType.GoblinRaid:
                case WorldEventType.BanditAttack:
                    // check if all event enemies are dead
                    if (eventEnemies.Count > 0 && eventEnemies.All(e => e.Dead) && !ev.Completed)
                    {
                        ev.Completed = true;
                        string label = ev.Type == WorldEventType.GoblinRaid ? "Goblin Raid" : "Bandit Attack";
                        int reward = ev.Type == WorldEventType.GoblinRaid ? 300 : 200;
                        player.Money += reward;
                        AddReputation(35, label);
                        ShowNotification($"{label} repelled! Reward: ${reward}");
                    }
                    break;

                case WorldEventType.HarvestFestival:
                    // discount applied via eventShopDiscount, nothing to tick
                    break;

                case WorldEventType.MeteorCrash:
                    // deposit rare ore near the crash site when player arrives
                    if (!ev.Completed && Vector2.Distance(player.Center, ev.Position) < ev.Radius)
                    {
                        ev.Completed = true;
                        AddLootDrop(ev.Position + new Vector2(20, 0), "Crystal");
                        AddLootDrop(ev.Position + new Vector2(-20, 10), "Gold Ore");
                        AddLootDrop(ev.Position + new Vector2(0, -20), "Iron Ore");
                        AddLootDrop(ev.Position + new Vector2(30, 30), "Crystal");
                        AddReputation(20, "Meteor Crash");
                        ShowNotification("You found the meteor crash site! Rare ores scattered around.");
                    }
                    break;
            }

            // event expired
            if (ev.Timer >= ev.Duration)
                EndWorldEvent();
        }

        static void EndWorldEvent()
        {
            if (activeWorldEvent == null) return;

            var type = activeWorldEvent.Type;

            // cleanup event enemies from the main list
            foreach (var e in eventEnemies)
                enemies.Remove(e);
            eventEnemies.Clear();

            // cleanup event NPC
            if (eventNPC != null) { npcs.Remove(eventNPC); eventNPC = null; }

            // reset effects
            eventShopDiscount = 0f;
            eventDroughtActive = false;
            eventScreenShake = 0f;

            if (!activeWorldEvent.Completed)
                ShowNotification(GetEventEndMessage(type));

            activeWorldEvent = null;
        }

        // ── DRAW WORLD EVENT EFFECTS ────────────────────────────────────────
        // Call inside BeginMode2D, after enemies but before HUD
        static void DrawWorldEventEffects()
        {
            if (activeWorldEvent == null) return;
            var ev = activeWorldEvent;

            switch (ev.Type)
            {
                case WorldEventType.ForestFire:
                    // flickering orange-red circle
                    float flicker = 0.7f + 0.3f * MathF.Sin(ev.Timer * 5f);
                    Raylib.DrawCircle((int)ev.Position.X, (int)ev.Position.Y, ev.Radius * flicker,
                        new Color((byte)255, (byte)80, (byte)0, (byte)40));
                    Raylib.DrawCircle((int)ev.Position.X, (int)ev.Position.Y, ev.Radius * 0.5f * flicker,
                        new Color((byte)255, (byte)40, (byte)0, (byte)60));
                    break;

                case WorldEventType.MeteorCrash:
                    // glowing crater
                    Raylib.DrawCircle((int)ev.Position.X, (int)ev.Position.Y, 60,
                        new Color((byte)80, (byte)40, (byte)10, (byte)200));
                    Raylib.DrawCircle((int)ev.Position.X, (int)ev.Position.Y, 30,
                        new Color((byte)255, (byte)120, (byte)0, (byte)(int)(100 + 50 * MathF.Sin(ev.Timer * 3f))));
                    break;

                case WorldEventType.DragonSighting:
                    // dragon flies left-to-right across the sky area
                    float progress = ev.Timer / ev.Duration;
                    float dx = ev.Position.X - 3000 + progress * 6000;
                    float dy = ev.Position.Y + MathF.Sin(progress * 8f) * 200f;
                    // body
                    Raylib.DrawEllipse((int)dx, (int)dy, 80, 30, new Color((byte)120, (byte)20, (byte)20, (byte)230));
                    // wings
                    float wingFlap = MathF.Sin(ev.Timer * 6f) * 25f;
                    Raylib.DrawTriangle(
                        new Vector2(dx - 40, dy), new Vector2(dx, dy - 60 - wingFlap),
                        new Vector2(dx + 20, dy), new Color((byte)160, (byte)40, (byte)40, (byte)200));
                    Raylib.DrawTriangle(
                        new Vector2(dx - 20, dy), new Vector2(dx, dy + 60 + wingFlap),
                        new Vector2(dx + 40, dy), new Color((byte)160, (byte)40, (byte)40, (byte)200));
                    // head
                    Raylib.DrawCircle((int)dx + 70, (int)dy - 5, 18,
                        new Color((byte)140, (byte)25, (byte)25, (byte)230));
                    // eye
                    Raylib.DrawCircle((int)dx + 78, (int)dy - 10, 4, Color.Yellow);
                    break;

                case WorldEventType.TravellingMerchant:
                    // draw a little cart/tent marker
                    Raylib.DrawRectangle((int)ev.Position.X - 30, (int)ev.Position.Y - 20, 60, 40,
                        new Color((byte)160, (byte)100, (byte)40, (byte)230));
                    Raylib.DrawTriangle(
                        new Vector2(ev.Position.X - 35, ev.Position.Y - 20),
                        new Vector2(ev.Position.X + 35, ev.Position.Y - 20),
                        new Vector2(ev.Position.X, ev.Position.Y - 50),
                        new Color((byte)200, (byte)50, (byte)50, (byte)230));
                    DrawTextUI("MERCHANT", (int)ev.Position.X - 30, (int)ev.Position.Y - 65, 14, Color.Gold);
                    break;

                case WorldEventType.LostChild:
                    if (eventNPC != null && !eventNPC.Hidden)
                    {
                        DrawTextUI("HELP!", (int)eventNPC.Position.X, (int)eventNPC.Position.Y - 30,
                            16, new Color((byte)255, (byte)80, (byte)80, (byte)(int)(180 + 75 * MathF.Sin(ev.Timer * 4f))));
                    }
                    break;
            }
        }

        // Call AFTER EndMode2D for screen-space overlays
        static void DrawWorldEventOverlay()
        {
            if (activeWorldEvent == null) return;
            var ev = activeWorldEvent;

            switch (ev.Type)
            {
                case WorldEventType.Blizzard:
                    if (Vector2.Distance(player.Center, ev.Position) < ev.Radius)
                    {
                        // white-out overlay
                        byte a = (byte)(120 + 40 * MathF.Sin(ev.Timer * 2f));
                        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight,
                            new Color((byte)220, (byte)225, (byte)240, a));
                        // swirling snow particles
                        for (int i = 0; i < 60; i++)
                        {
                            int sx = (int)((i * 137 + ev.Timer * 200 + i * 43) % ScreenWidth);
                            int sy = (int)((i * 89 + ev.Timer * 350 + i * 67) % ScreenHeight);
                            Raylib.DrawCircle(sx, sy, 2, new Color((byte)255, (byte)255, (byte)255, (byte)200));
                        }
                    }
                    break;

                case WorldEventType.FishingTournament:
                    int remaining = (int)(ev.Duration - ev.Timer);
                    DrawTextUI($"FISHING TOURNAMENT  |  {remaining}s left  |  Fish caught: {ev.Data}",
                        ScreenWidth / 2 - 200, 60, 20, Color.Gold);
                    break;

                case WorldEventType.HarvestFestival:
                    DrawTextUI("HARVEST FESTIVAL — 30% OFF ALL SHOPS!",
                        ScreenWidth / 2 - 180, 60, 20, new Color((byte)255, (byte)200, (byte)50, (byte)255));
                    break;
            }

            // event timer bar (all events)
            float pct = 1f - ev.Timer / ev.Duration;
            int barW = 200;
            Raylib.DrawRectangle(ScreenWidth - barW - 20, 100, barW, 12, new Color((byte)30, (byte)30, (byte)30, (byte)180));
            Raylib.DrawRectangle(ScreenWidth - barW - 20, 100, (int)(barW * pct), 12,
                new Color((byte)255, (byte)180, (byte)40, (byte)220));
            DrawTextUI(GetEventLabel(ev.Type), ScreenWidth - barW - 20, 82, 14, Color.White);
        }

        // ── SPAWNERS ────────────────────────────────────────────────────────
        static void SpawnMerchantNPC(Vector2 pos)
        {
            eventNPC = new NPC(pos, "Travelling Merchant",
                "E = Browse rare wares! I won't be here long.")
            { SpriteKey = "villager" };
            npcs.Add(eventNPC);
        }

        static void SpawnLostChildNPC(Vector2 pos)
        {
            eventNPC = new NPC(pos, "Lost Child", "Please help me find my way home!")
            { SpriteKey = "villager" };
            npcs.Add(eventNPC);
        }

        static void SpawnTreasureGoblin(Vector2 pos)
        {
            var gob = new Enemy(pos, "Goblin", 25, new Color((byte)200, (byte)180, (byte)0, (byte)255));
            enemies.Add(gob);
            eventEnemies.Add(gob);
        }

        static void SpawnGoblinRaid(Vector2 centre, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 sp = centre + RandomOffset(100, 400);
                var gob = new Enemy(sp, "Goblin", 4, new Color((byte)90, (byte)150, (byte)70, (byte)255));
                enemies.Add(gob);
                eventEnemies.Add(gob);
            }
        }

        static void SpawnBandits(Vector2 centre, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 sp = centre + RandomOffset(80, 300);
                string type = i % 2 == 0 ? "Thug" : "Robber";
                int hp = type == "Thug" ? 7 : 6;
                var bandit = new Enemy(sp, type, hp,
                    new Color((byte)70, (byte)70, (byte)80, (byte)255));
                enemies.Add(bandit);
                eventEnemies.Add(bandit);
            }
        }

        static Vector2 RandomOffset(int minDist, int maxDist)
        {
            float angle = Raylib.GetRandomValue(0, 360) * MathF.PI / 180f;
            float dist = Raylib.GetRandomValue(minDist, maxDist);
            return new Vector2(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist);
        }

        // ── TEXT HELPERS ────────────────────────────────────────────────────
        static string GetEventAnnouncement(WorldEventType t) => t switch
        {
            WorldEventType.TravellingMerchant => "A Travelling Merchant has appeared nearby!",
            WorldEventType.TreasureGoblin     => "A Treasure Goblin has been spotted! Catch it!",
            WorldEventType.GoblinRaid         => "GOBLIN RAID! The town is under attack!",
            WorldEventType.MeteorCrash        => "A meteor has crashed in the mountains! Investigate!",
            WorldEventType.BanditAttack       => "Bandits are attacking! Defend yourself!",
            WorldEventType.ForestFire         => "A forest fire has broken out! Stay clear!",
            WorldEventType.Blizzard           => "A blizzard is sweeping the snow zone!",
            WorldEventType.DragonSighting     => "A DRAGON has been sighted overhead!",
            WorldEventType.LostChild          => "A child has gone missing nearby. Can you find them?",
            WorldEventType.FishingTournament  => "FISHING TOURNAMENT! Catch as many fish as you can!",
            WorldEventType.HarvestFestival    => "Harvest Festival! All shops 30% off!",
            _ => "Something strange is happening..."
        };

        static string GetEventEndMessage(WorldEventType t) => t switch
        {
            WorldEventType.TravellingMerchant => "The Travelling Merchant has moved on.",
            WorldEventType.TreasureGoblin     => "The Treasure Goblin escaped!",
            WorldEventType.GoblinRaid         => "The goblin raid has ended.",
            WorldEventType.ForestFire         => "The forest fire has burned out.",
            WorldEventType.Blizzard           => "The blizzard has passed.",
            WorldEventType.DragonSighting     => "The dragon has flown away.",
            WorldEventType.LostChild          => "The lost child wandered home on their own.",
            WorldEventType.FishingTournament  => "The fishing tournament is over!",
            WorldEventType.HarvestFestival    => "The Harvest Festival has ended.",
            _ => "The event has ended."
        };

        static string GetEventLabel(WorldEventType t) => t switch
        {
            WorldEventType.TravellingMerchant => "Merchant",
            WorldEventType.TreasureGoblin     => "Treasure Goblin",
            WorldEventType.GoblinRaid         => "Goblin Raid",
            WorldEventType.MeteorCrash        => "Meteor Crash",
            WorldEventType.BanditAttack       => "Bandit Attack",
            WorldEventType.ForestFire         => "Forest Fire",
            WorldEventType.Blizzard           => "Blizzard",
            WorldEventType.DragonSighting     => "Dragon!",
            WorldEventType.LostChild          => "Lost Child",
            WorldEventType.FishingTournament  => "Fishing Tourney",
            WorldEventType.HarvestFestival    => "Festival",
            _ => "Event"
        };
    }
}