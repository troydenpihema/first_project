
using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ═══════════════════════════════════════════════════════════════
        //  BIOME BOSSES — one unique boss per biome
        // ═══════════════════════════════════════════════════════════════
        static List<WorldBoss> biomeBosses = new();

        static void SpawnBiomeBosses()
        {
            // SNOW — Frost Wyrm: massive, slow, high HP
            var frostWyrm = new WorldBoss(
                new Vector2(-160000, -160000), "Frost Wyrm", 4000f, 2000f,
                new Color((byte)140, (byte)200, (byte)240, (byte)255))
            { Size = 300, ContactDamage = 45, ChaseSpeed = 100f, WanderSpeed = 40f,
              AggroRange = 500f, DeAggroRange = 1500f, ShakesWhenNear = true, ProximityShakeRange = 800f };
            biomeBosses.Add(frostWyrm);

            // MOUNTAINS — Stone Guardian: tanky, moderate speed
            var stoneGuardian = new WorldBoss(
                new Vector2(-20000, -180000), "Stone Guardian", 5000f, 1800f,
                new Color((byte)120, (byte)110, (byte)100, (byte)255))
            { Size = 350, ContactDamage = 50, ChaseSpeed = 90f, WanderSpeed = 35f,
              AggroRange = 450f, DeAggroRange = 1400f, ShakesWhenNear = true, ProximityShakeRange = 900f };
            biomeBosses.Add(stoneGuardian);

            // VOLCANO — Infernal Golem: very high damage, glowing
            var infernalGolem = new WorldBoss(
                new Vector2(160000, -180000), "Infernal Golem", 4500f, 1600f,
                new Color((byte)200, (byte)60, (byte)10, (byte)255))
            { Size = 320, ContactDamage = 60, ChaseSpeed = 110f, WanderSpeed = 45f,
              AggroRange = 550f, DeAggroRange = 1600f, ShakesWhenNear = true, ProximityShakeRange = 1000f };
            biomeBosses.Add(infernalGolem);

            // SWAMP — Swamp Hydra: multi-hit, poison themed
            var swampHydra = new WorldBoss(
                new Vector2(-160000, -20000), "Swamp Hydra", 3500f, 2200f,
                new Color((byte)50, (byte)100, (byte)40, (byte)255))
            { Size = 280, ContactDamage = 35, ChaseSpeed = 120f, WanderSpeed = 50f,
              AggroRange = 500f, DeAggroRange = 1400f };
            biomeBosses.Add(swampHydra);

            // DESERT — Sand King: fast, hit-and-run
            var sandKing = new WorldBoss(
                new Vector2(160000, 20000), "Sand King", 3000f, 2500f,
                new Color((byte)210, (byte)170, (byte)60, (byte)255))
            { Size = 260, ContactDamage = 40, ChaseSpeed = 160f, WanderSpeed = 70f,
              AggroRange = 600f, DeAggroRange = 1800f };
            biomeBosses.Add(sandKing);

            // FOREST — Ancient Treant: massive HP, slow but devastating
            var ancientTreant = new WorldBoss(
                new Vector2(-160000, 160000), "Ancient Treant", 6000f, 1500f,
                new Color((byte)60, (byte)100, (byte)30, (byte)255))
            { Size = 400, ContactDamage = 55, ChaseSpeed = 75f, WanderSpeed = 30f,
              AggroRange = 400f, DeAggroRange = 1200f, ShakesWhenNear = true, ProximityShakeRange = 1100f };
            biomeBosses.Add(ancientTreant);

            // OCEAN — Reef Leviathan: deep ocean boss
            var reefLeviathan = new WorldBoss(
                new Vector2(40000, 160000), "Reef Leviathan", 5500f, 3000f,
                new Color((byte)30, (byte)80, (byte)160, (byte)255))
            { Size = 380, ContactDamage = 50, ChaseSpeed = 130f, WanderSpeed = 55f,
              AggroRange = 550f, DeAggroRange = 1600f, ShakesWhenNear = true, ProximityShakeRange = 900f };
            biomeBosses.Add(reefLeviathan);
        }

        static void UpdateBiomeBosses(float dt)
        {
            foreach (var boss in biomeBosses)
                UpdateWorldBoss(boss, dt);
        }

        static void DrawBiomeBosses()
        {
            foreach (var boss in biomeBosses)
                boss?.Draw();
        }


        // ═══════════════════════════════════════════════════════════════
        //  BIOME SETTLEMENTS — themed towns in each biome
        // ═══════════════════════════════════════════════════════════════

        static void SpawnBiomeSettlements()
        {
            // ── SNOW: Frosthold ──
            SpawnSettlement("Frosthold", -140000, -140000,
                new Color((byte)180, (byte)200, (byte)220, (byte)255),
                new Color((byte)160, (byte)185, (byte)210, (byte)255),
                new[] {
                    ("GENERAL STORE", "Yuki", "Welcome to Frosthold! Stock up before the storm."),
                    ("INN", "Kahu", "Rest here, the blizzard will pass."),
                    ("BLACKSMITH", "Hone", "Ice-forged steel cuts deepest."),
                },
                new[] { "Aroha", "Nikau", "Tane", "Mere" },
                "Patrolling", "Chopping wood");

            // ── MOUNTAINS: Ironpeak ──
            SpawnSettlement("Ironpeak", -10000, -160000,
                new Color((byte)140, (byte)130, (byte)125, (byte)255),
                new Color((byte)120, (byte)115, (byte)110, (byte)255),
                new[] {
                    ("GENERAL STORE", "Petra", "Ironpeak's got the best ore this side of the range."),
                    ("INN", "Cliff", "Take a load off, mountaineer."),
                    ("MINING GUILD", "Boulder", "Join the guild and mine the deep veins."),
                },
                new[] { "Flint", "Ridge", "Slate", "Pebble" },
                "Patrolling", "Chopping wood");

            // ── VOLCANO: Cinderfall ──
            SpawnSettlement("Cinderfall", 140000, -150000,
                new Color((byte)80, (byte)40, (byte)20, (byte)255),
                new Color((byte)60, (byte)30, (byte)15, (byte)255),
                new[] {
                    ("GENERAL STORE", "Ember", "Fire resistance potions, half price today."),
                    ("INN", "Ash", "Coolest spot in the caldera, literally."),
                    ("FORGE", "Magmus", "Lava-tempered blades, only here."),
                },
                new[] { "Scoria", "Pumice", "Char", "Spark" },
                "Patrolling", "Eating lunch");

            // ── SWAMP: Murkwater ──
            SpawnSettlement("Murkwater", -140000, 10000,
                new Color((byte)55, (byte)70, (byte)40, (byte)255),
                new Color((byte)45, (byte)60, (byte)35, (byte)255),
                new[] {
                    ("GENERAL STORE", "Mire", "Antidotes and bog boots, you'll need both."),
                    ("INN", "Moss", "It ain't pretty, but it's dry."),
                    ("HERBALIST", "Fern", "The swamp provides the best medicine."),
                },
                new[] { "Reed", "Bog", "Lily", "Toad" },
                "Fishing", "Farming");

            // ── DESERT: Sunhaven ──
            SpawnSettlement("Sunhaven", 140000, 10000,
                new Color((byte)210, (byte)185, (byte)130, (byte)255),
                new Color((byte)190, (byte)165, (byte)110, (byte)255),
                new[] {
                    ("GENERAL STORE", "Dune", "Water skins and sun cloaks, essential gear."),
                    ("INN", "Oasis", "Rest in the shade, friend."),
                    ("BAZAAR", "Sahir", "Exotic goods from across the sands."),
                },
                new[] { "Arid", "Mesa", "Sandy", "Mirage" },
                "Patrolling", "Eating lunch");

            // ── FOREST: Eldergrove ──
            SpawnSettlement("Eldergrove", -140000, 140000,
                new Color((byte)50, (byte)90, (byte)40, (byte)255),
                new Color((byte)40, (byte)75, (byte)30, (byte)255),
                new[] {
                    ("GENERAL STORE", "Willow", "Forest herbs and rare wood, all natural."),
                    ("INN", "Oak", "The oldest inn in the land, they say."),
                    ("RANGER LODGE", "Hazel", "Track beasts, protect the grove."),
                },
                new[] { "Birch", "Rowan", "Ivy", "Elm" },
                "Chopping wood", "Farming");

            // ── OCEAN/BEACH: Tidecrest ──
            SpawnSettlement("Tidecrest", 20000, 95000,
                new Color((byte)180, (byte)200, (byte)210, (byte)255),
                new Color((byte)160, (byte)185, (byte)200, (byte)255),
                new[] {
                    ("GENERAL STORE", "Coral", "Fishing gear and diving supplies!"),
                    ("INN", "Marina", "Best seafood on the coast."),
                    ("DOCK MASTER", "Skipper", "Need a boat? Talk to me."),
                },
                new[] { "Kai", "Wave", "Shelly", "Anchor" },
                "Fishing", "Mending nets");
        }

        /// Helper to spawn a settlement: ground pad, buildings, and NPCs
        static void SpawnSettlement(string name, float cx, float cy,
            Color buildingColor, Color interiorColor,
            (string type, string npcName, string dialogue)[] shopDefs,
            string[] citizenNames,
            string activity1, string activity2)
        {
            // ground pad — flattened area for the settlement
            // (drawn in DrawSettlementGrounds)

            float spacing = 280f;
            Vector2 center = new Vector2(cx, cy);

            // signpost text stored for drawing
            settlementMarkers.Add((name, center));

            // spawn buildings in a row
            for (int i = 0; i < shopDefs.Length; i++)
            {
                float bx = cx - spacing + i * spacing;
                float by = cy - 60;

                var b = new Building(
                    new Rectangle(bx, by, 180, 120),
                    buildingColor, interiorColor,
                    new Vector2(bx + 90, by + 90),
                    $"{name} {shopDefs[i].type}",
                    new NPC(new Vector2(700, 160), shopDefs[i].npcName, shopDefs[i].dialogue),
                    entryPos: new Vector2(700, 900));

                b.InteriorObjects.Clear();
                b.InteriorObjects.Add(new Rectangle(400, 100, 300, 40));  // counter
                b.InteriorObjects.Add(new Rectangle(100, 300, 150, 80)); // shelf 1
                b.InteriorObjects.Add(new Rectangle(700, 300, 150, 80)); // shelf 2

                buildings.Add(b);
            }

            // spawn citizen NPCs with schedules
            Vector2 tavernPos = new Vector2(cx, cy + 200);
            for (int i = 0; i < citizenNames.Length; i++)
            {
                Vector2 home = center + new Vector2(-300 + i * 160, -250);
                Vector2 work = center + new Vector2(-100 + i * 70, 100);

                var npc = new NPC(home, citizenNames[i], $"Welcome to {name}!")
                {
                    SpriteKey = "villager", Role = NPC.NpcRole.Citizen,
                    HomeAnchor = home, ScheduleSpeed = 70f,
                    DailySchedule = new List<NPC.ScheduleSlot>
                    {
                        new() { StartHour = 7f,  EndHour = 12f, Destination = work,
                                Activity = activity1, WanderRadius = 60f },
                        new() { StartHour = 12f, EndHour = 13f, Destination = home,
                                Activity = "Eating lunch", WanderRadius = 15f },
                        new() { StartHour = 13f, EndHour = 18f, Destination = work,
                                Activity = activity2, WanderRadius = 60f },
                        new() { StartHour = 19f, EndHour = 22f, Destination = tavernPos,
                                Activity = "At the tavern", WanderRadius = 30f },
                    }
                };
                npcs.Add(npc);
            }

            // spawn a guard NPC
            var guard = new NPC(center + new Vector2(0, -200), $"{name} Guard", "Stay safe out there.")
            {
                SpriteKey = "villager", Role = NPC.NpcRole.Guard,
                HomeAnchor = center + new Vector2(0, -200), ScheduleSpeed = 60f,
                DailySchedule = new List<NPC.ScheduleSlot>
                {
                    new() { StartHour = 6f, EndHour = 22f, Destination = center,
                            Activity = "Patrolling", WanderRadius = 200f },
                }
            };
            npcs.Add(guard);
        }

        // settlement marker data for signposts and ground pads
        static List<(string name, Vector2 center)> settlementMarkers = new();

        /// Draw settlement ground pads and signposts — call in the world draw section
        static void DrawSettlementGrounds()
        {
            float cullLeft   = camera.Target.X - ScreenWidth / 2f / camera.Zoom - 500;
            float cullRight  = camera.Target.X + ScreenWidth / 2f / camera.Zoom + 500;
            float cullTop    = camera.Target.Y - ScreenHeight / 2f / camera.Zoom - 400;
            float cullBottom = camera.Target.Y + ScreenHeight / 2f / camera.Zoom + 400;

            foreach (var (name, center) in settlementMarkers)
            {
                float sx = center.X, sy = center.Y;

                // skip if off-screen (large margin for settlement size)
                if (sx + 500 < cullLeft || sx - 500 > cullRight ||
                    sy + 400 < cullTop || sy - 400 > cullBottom) continue;

                // ground pad — cleared area
                Raylib.DrawRectangle((int)(sx - 450), (int)(sy - 300), 900, 600,
                    new Color((byte)130, (byte)115, (byte)80, (byte)255));

                // inner area — lighter packed earth
                Raylib.DrawRectangle((int)(sx - 420), (int)(sy - 270), 840, 540,
                    new Color((byte)150, (byte)135, (byte)95, (byte)255));

                // cobblestone texture — subtle grid
                for (int gx = -420; gx < 420; gx += 40)
                    for (int gy = -270; gy < 270; gy += 40)
                    {
                        int hash = ((gx * 7 + gy * 13) & 0xFF);
                        byte shade = (byte)(140 + hash % 25);
                        Raylib.DrawRectangle((int)(sx + gx), (int)(sy + gy), 38, 38,
                            new Color(shade, (byte)(shade - 15), (byte)(shade - 40), (byte)80));
                    }

                // path leading south from settlement
                Raylib.DrawRectangle((int)(sx - 30), (int)(sy + 300), 60, 200,
                    new Color((byte)140, (byte)120, (byte)75, (byte)200));

                // signpost
                int signX = (int)(sx - 40), signY = (int)(sy - 290);
                // post
                Raylib.DrawRectangle(signX + 16, signY, 8, 50,
                    new Color((byte)100, (byte)70, (byte)35, (byte)255));
                // board
                Raylib.DrawRectangle(signX - 10, signY - 10, 60, 24,
                    new Color((byte)140, (byte)100, (byte)50, (byte)255));
                Raylib.DrawRectangleLines(signX - 10, signY - 10, 60, 24,
                    new Color((byte)90, (byte)60, (byte)25, (byte)255));

                // settlement name
                int nameW = MeasureTextUI(name, 18);
                DrawTextUI(name, (int)(sx - nameW / 2), signY - 30, 18, Color.Gold);

                // torches at settlement entrance
                Raylib.DrawRectangle((int)(sx - 55), (int)(sy + 290), 6, 20,
                    new Color((byte)100, (byte)70, (byte)35, (byte)255));
                Raylib.DrawCircle((int)(sx - 52), (int)(sy + 288), 6,
                    new Color((byte)255, (byte)180, (byte)40, (byte)180));
                Raylib.DrawRectangle((int)(sx + 49), (int)(sy + 290), 6, 20,
                    new Color((byte)100, (byte)70, (byte)35, (byte)255));
                Raylib.DrawCircle((int)(sx + 52), (int)(sy + 288), 6,
                    new Color((byte)255, (byte)180, (byte)40, (byte)180));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  BIOME ENEMY CLUSTERS — spread enemies across each biome
        // ═══════════════════════════════════════════════════════════════

        static void SpawnBiomeEnemyClusters()
        {
            var rand = new Random(42); // deterministic seed

            // helper to spawn a cluster of enemies around a position
            void Cluster(float cx, float cy, string type, int hp, Color color, int count, float spread)
            {
                for (int i = 0; i < count; i++)
                {
                    float ox = (float)(rand.NextDouble() * spread * 2 - spread);
                    float oy = (float)(rand.NextDouble() * spread * 2 - spread);
                    enemies.Add(new Enemy(new Vector2(cx + ox, cy + oy), type, hp, color));
                }
            }

            Color scorpionCol = new Color((byte)180, (byte)120, (byte)0, (byte)255);
            Color bearCol     = new Color((byte)100, (byte)100, (byte)120, (byte)255);
            Color snakeCol    = new Color((byte)40, (byte)100, (byte)40, (byte)255);
            Color crocCol     = new Color((byte)50, (byte)80, (byte)30, (byte)255);
            Color lizardCol   = new Color((byte)180, (byte)60, (byte)10, (byte)255);
            Color beetleCol   = new Color((byte)120, (byte)30, (byte)0, (byte)255);
            Color eagleCol    = new Color((byte)100, (byte)70, (byte)20, (byte)255);
            Color goatCol     = new Color((byte)200, (byte)195, (byte)185, (byte)255);
            Color wolfCol     = Color.DarkGray;
            Color crabCol     = new Color((byte)210, (byte)80, (byte)30, (byte)255);

            // ── SNOW — Ice Wolves, Frost Bears, Yetis ──
            Color iceWolfCol = new Color((byte)180, (byte)200, (byte)220, (byte)255);
            Color yetiCol    = new Color((byte)200, (byte)215, (byte)230, (byte)255);

            Cluster(-120000, -120000, "Ice Wolf", 8, iceWolfCol, 6, 1500);
            Cluster(-180000, -100000, "Ice Wolf", 8, iceWolfCol, 5, 1200);
            Cluster(-200000, -200000, "Frost Bear", 12, bearCol, 4, 2000);
            Cluster(-140000, -200000, "Frost Bear", 12, bearCol, 4, 1800);
            Cluster(-100000, -150000, "Ice Wolf", 8, iceWolfCol, 5, 1400);
            Cluster(-160000, -180000, "Yeti", 18, yetiCol, 3, 2500);
            Cluster(-220000, -140000, "Yeti", 18, yetiCol, 2, 2000);

            // ── MOUNTAINS — Eagles, Mountain Goats, Rock Trolls ──
            Color trollCol = new Color((byte)100, (byte)90, (byte)80, (byte)255);

            Cluster(-40000, -120000, "Eagle", 11, eagleCol, 5, 1500);
            Cluster(20000, -150000, "Eagle", 11, eagleCol, 5, 1500);
            Cluster(-20000, -200000, "Mountain Goat", 13, goatCol, 4, 1800);
            Cluster(40000, -180000, "Mountain Goat", 13, goatCol, 4, 1600);
            Cluster(0, -160000, "Rock Troll", 20, trollCol, 3, 2500);
            Cluster(-60000, -130000, "Rock Troll", 20, trollCol, 2, 2000);

            // ── VOLCANO — Fire Lizards, Magma Beetles, Flame Wraiths ──
            Color wraithCol = new Color((byte)255, (byte)100, (byte)20, (byte)255);

            Cluster(120000, -120000, "Fire Lizard", 12, lizardCol, 6, 1500);
            Cluster(180000, -150000, "Fire Lizard", 12, lizardCol, 5, 1200);
            Cluster(140000, -200000, "Magma Beetle", 15, beetleCol, 5, 1800);
            Cluster(200000, -180000, "Magma Beetle", 15, beetleCol, 4, 1600);
            Cluster(160000, -160000, "Flame Wraith", 22, wraithCol, 3, 2500);
            Cluster(100000, -140000, "Flame Wraith", 22, wraithCol, 2, 2000);

            // ── SWAMP — Snakes, Crocodiles, Bog Lurkers ──
            Color lurkerCol = new Color((byte)40, (byte)60, (byte)30, (byte)255);

            Cluster(-120000, -40000, "Snake", 10, snakeCol, 6, 1500);
            Cluster(-180000, 20000, "Snake", 10, snakeCol, 5, 1200);
            Cluster(-140000, 40000, "Crocodile", 14, crocCol, 4, 2000);
            Cluster(-200000, -20000, "Crocodile", 14, crocCol, 4, 1800);
            Cluster(-160000, 0, "Bog Lurker", 18, lurkerCol, 3, 2500);
            Cluster(-220000, 40000, "Bog Lurker", 18, lurkerCol, 2, 2000);

            // ── DESERT — Scorpions, Sand Vipers, Dust Devils ──
            Color viperCol = new Color((byte)190, (byte)160, (byte)80, (byte)255);
            Color devilCol = new Color((byte)180, (byte)150, (byte)90, (byte)255);

            Cluster(120000, -20000, "Scorpion", 8, scorpionCol, 6, 1500);
            Cluster(180000, 30000, "Scorpion", 8, scorpionCol, 5, 1200);
            Cluster(140000, 50000, "Sand Viper", 12, viperCol, 5, 1800);
            Cluster(200000, -40000, "Sand Viper", 12, viperCol, 4, 1600);
            Cluster(160000, 0, "Dust Devil", 16, devilCol, 3, 2500);
            Cluster(220000, 40000, "Dust Devil", 16, devilCol, 2, 2000);

            // ── FOREST — Wolves, Forest Spiders, Ents ──
            Color spiderCol = new Color((byte)50, (byte)45, (byte)40, (byte)255);
            Color entCol    = new Color((byte)70, (byte)110, (byte)40, (byte)255);

            Cluster(-120000, 120000, "Wolf", 7, wolfCol, 6, 1500);
            Cluster(-180000, 160000, "Wolf", 7, wolfCol, 5, 1200);
            Cluster(-140000, 200000, "Forest Spider", 10, spiderCol, 5, 1800);
            Cluster(-200000, 140000, "Forest Spider", 10, spiderCol, 4, 1600);
            Cluster(-160000, 180000, "Ent", 20, entCol, 3, 2500);
            Cluster(-100000, 220000, "Ent", 20, entCol, 2, 2000);

            // ── OCEAN/BEACH — Crabs, Sharks, Sea Serpents ──
            Color serpentCol = new Color((byte)40, (byte)120, (byte)160, (byte)255);
            Color sharkCol   = new Color((byte)70, (byte)100, (byte)140, (byte)255);

            Cluster(-20000, 100000, "Crab", 10, crabCol, 6, 1500);
            Cluster(60000, 95000, "Crab", 10, crabCol, 5, 1200);
            Cluster(0, 140000, "Shark", 13, sharkCol, 4, 2000);
            Cluster(80000, 150000, "Shark", 13, sharkCol, 4, 1800);
            Cluster(40000, 180000, "Sea Serpent", 20, serpentCol, 3, 3000);
            Cluster(120000, 170000, "Sea Serpent", 20, serpentCol, 2, 2500);
        }


        // ═══════════════════════════════════════════════════════════════
        //  BIOME RESOURCE NODES — unique trees/rocks per biome
        // ═══════════════════════════════════════════════════════════════

        static void SpawnBiomeResources()
        {
            var rand = new Random(99);

            void TreeCluster(float cx, float cy, string type, int count, float spread)
            {
                for (int i = 0; i < count; i++)
                {
                    float ox = (float)(rand.NextDouble() * spread * 2 - spread);
                    float oy = (float)(rand.NextDouble() * spread * 2 - spread);
                    var tree = type switch
                    {
                        "Birch"  => TreeObject.Birch(new Vector2(cx + ox, cy + oy)),
                        "Oak"    => TreeObject.Oak(new Vector2(cx + ox, cy + oy)),
                        "Pine"   => TreeObject.Pine(new Vector2(cx + ox, cy + oy)),
                        "Arctic" => TreeObject.Arctic(new Vector2(cx + ox, cy + oy)),
                        "Dead"   => TreeObject.Dead(new Vector2(cx + ox, cy + oy)),
                        _        => TreeObject.Normal(new Vector2(cx + ox, cy + oy)),
                    };
                    trees.Add(tree);
                }
            }

            void RockCluster(float cx, float cy, string oreType, int count, float spread)
            {
                for (int i = 0; i < count; i++)
                {
                    float ox = (float)(rand.NextDouble() * spread * 2 - spread);
                    float oy = (float)(rand.NextDouble() * spread * 2 - spread);
                    Vector2 pos = new Vector2(cx + ox, cy + oy);
                    var rock = oreType switch
                    {
                        "Copper"  => RockObject.Copper(pos),
                        "Iron"    => RockObject.Iron(pos),
                        "Gold"    => RockObject.Gold(pos),
                        "Crystal" => RockObject.Crystal(pos),
                        _         => RockObject.Stone(pos),
                    };
                    rocks.Add(rock);
                }
            }

            // SNOW — Arctic trees, Crystal ore
            TreeCluster(-130000, -130000, "Arctic", 12, 3000);
            TreeCluster(-200000, -180000, "Arctic", 10, 2500);
            TreeCluster(-170000, -110000, "Arctic", 8, 2000);
            RockCluster(-150000, -150000, "Crystal", 6, 2500);
            RockCluster(-190000, -190000, "Crystal", 5, 2000);

            // MOUNTAINS — Pine trees, Iron/Gold ore
            TreeCluster(-30000, -130000, "Pine", 10, 2500);
            TreeCluster(30000, -170000, "Pine", 8, 2000);
            RockCluster(-10000, -150000, "Iron", 8, 3000);
            RockCluster(20000, -190000, "Gold", 5, 2500);
            RockCluster(-40000, -200000, "Iron", 6, 2000);

            // VOLCANO — Dead trees, Gold ore
            TreeCluster(130000, -130000, "Dead", 8, 2500);
            TreeCluster(190000, -170000, "Dead", 6, 2000);
            RockCluster(150000, -160000, "Gold", 6, 3000);
            RockCluster(180000, -200000, "Gold", 5, 2500);

            // SWAMP — Normal/Dead trees, Copper ore
            TreeCluster(-130000, -10000, "Normal", 10, 2500);
            TreeCluster(-180000, 30000, "Dead", 8, 2000);
            RockCluster(-150000, 20000, "Copper", 8, 3000);

            // DESERT — Dead trees (sparse), Copper ore
            TreeCluster(130000, 30000, "Dead", 5, 4000);
            RockCluster(150000, -10000, "Copper", 6, 3000);
            RockCluster(200000, 30000, "Iron", 5, 2500);

            // FOREST — Oak/Birch/Pine mix, all ore types
            TreeCluster(-130000, 130000, "Oak", 12, 3000);
            TreeCluster(-180000, 170000, "Birch", 10, 2500);
            TreeCluster(-200000, 200000, "Pine", 10, 2500);
            TreeCluster(-120000, 200000, "Normal", 8, 2000);
            RockCluster(-140000, 150000, "Iron", 6, 2500);
            RockCluster(-190000, 190000, "Copper", 5, 2000);
            RockCluster(-160000, 220000, "Gold", 4, 2000);

            // BEACH/OCEAN — sparse Normal trees near shore
            TreeCluster(0, 90000, "Normal", 6, 3000);
            TreeCluster(60000, 92000, "Normal", 5, 2500);
        }


        // ═══════════════════════════════════════════════════════════════
        //  MASTER INIT — call from your world generation
        // ═══════════════════════════════════════════════════════════════

        static void InitBiomeContent()
        {
            SpawnBiomeBosses();
            SpawnBiomeSettlements();
            SpawnBiomeEnemyClusters();
            SpawnBiomeResources();
        }
    }
}
