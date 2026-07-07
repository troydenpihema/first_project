using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void DropSpiritEssence(Vector2 pos, int amount)
        {
            droppedItems.Add(new DroppedItem("Spirit Essence", amount, pos));
            if (multiplayer.Connected && multiplayer.IsHost)                     // NEW
                multiplayer.SendLootDrop(pos.X, pos.Y, "Spirit Essence", -1);    // -1 = public
        }

        static void AddLootDrop(Vector2 pos, string item, int ownerId = -1)
        {
            lootDrops.Add(new LootDrop(pos, item, ownerId));
            if (multiplayer.Connected && multiplayer.IsHost)
                multiplayer.SendLootDrop(pos.X, pos.Y, item, ownerId);
        }

        static Vector2 NearestPlayerTo(Vector2 from)
        {
            Vector2 best = player.Center;
            float bestD = Vector2.DistanceSquared(from, player.Center);
            if (multiplayer.Connected)
                lock (multiplayer.RemotePlayers)
                    foreach (var rp in multiplayer.RemotePlayers)
                    {
                        if (!rp.Active || rp.Scene != "World") continue;
                        var pos = new Vector2(rp.X, rp.Y);
                        float d = Vector2.DistanceSquared(from, pos);
                        if (d < bestD) { bestD = d; best = pos; }
                    }
            return best;
        }

        static int CombatXpFor(string type) => type switch
        {
            "Wild Dog" => 20, "Wolf" => 35, "Scorpion" => 30, "Bear" => 50,
            "Crab" => 55, "Shark" => 70, "Snake" => 55, "Crocodile" => 75,
            "Fire Lizard" => 65, "Magma Beetle" => 80, "Eagle" => 60, "Mountain Goat" => 70,
            "Warrior" => 60, "Wizard" => 60, "Archer" => 55, "Goblin" => 30,
            "Thug" => 40, "Robber" => 38, "Gangster" => 45, "Giant Bug" => 50,
            _ => 20
        };

         static void HandleEnemyDeath(Enemy enemy)
            {
            enemy.Dead = true;
            DropSpiritEssence(enemy.Position, 1);
            SpawnDeathFx(enemy.Center, enemy.EnemyColor, enemy.Type);

                if (enemy.Type == "Wild Dog") Raylib.PlaySound(soundDogDie);

                int killerId = enemy.LastDamagerId >= 0 ? enemy.LastDamagerId : multiplayer.MyId;
                if (killerId == multiplayer.MyId)
                player.AddCombatXP(CombatXpFor(enemy.Type));           
                else
                multiplayer.SendEnemyKill(killerId, enemy.Type);

        // Loot drops
        int rareRoll = Raylib.GetRandomValue(1, 100);

if (enemy.Type == "Wild Dog") {
    AddLootDrop(enemy.Position, "Bone", killerId);
    if (rareRoll <= 20) AddLootDrop(enemy.Position, "Fur", killerId);
    if (rareRoll <= 5)  AddLootDrop(enemy.Position, "Dog Fang", killerId);
}
else if (enemy.Type == "Wolf")  {
    wolvesKilled++;
    AddLootDrop(enemy.Position, "Fur", killerId);
    if (rareRoll <= 25) AddLootDrop(enemy.Position, "Bone", killerId);
    if (rareRoll <= 8)  AddLootDrop(enemy.Position, "Wolf Claw", killerId);
}
else if (enemy.Type == "Scorpion") {
    AddLootDrop(enemy.Position, "Stinger", killerId);
    if (rareRoll <= 20) AddLootDrop(enemy.Position, "Stinger", killerId);
    if (rareRoll <= 6)  AddLootDrop(enemy.Position, "Venom Sac", killerId);
}
else if (enemy.Type == "Bear") {
    AddLootDrop(enemy.Position, "Bear Pelt", killerId);
    if (rareRoll <= 30) AddLootDrop(enemy.Position, "Fur", killerId);          // 30% fur
    if (rareRoll <= 7)  AddLootDrop(enemy.Position, "Bear Claw", killerId);    // 7% rare claw
}
else if (enemy.Type == "Crab") {
    AddLootDrop(enemy.Position, "Crab Claw", killerId);
    if (rareRoll <= 25) AddLootDrop(enemy.Position, "Crab Claw", killerId);    // 25% extra claw
    if (rareRoll <= 8)  AddLootDrop(enemy.Position, "Crab Shell", killerId);   // 8% rare shell
}
else if (enemy.Type == "Shark") {
    AddLootDrop(enemy.Position, "Shark Fin", killerId);
    if (rareRoll <= 20) AddLootDrop(enemy.Position, "Shark Fin", killerId);    // 20% extra fin
    if (rareRoll <= 5)  AddLootDrop(enemy.Position, "Shark Tooth", killerId);  // 5% rare tooth
}
else if (enemy.Type == "Snake") {
    AddLootDrop(enemy.Position, "Snake Skin", killerId);
    if (rareRoll <= 20) AddLootDrop(enemy.Position, "Stinger", killerId);      // 20% venom fang (reuse stinger)
    if (rareRoll <= 6)  AddLootDrop(enemy.Position, "Snake Fang", killerId);   // 6% rare fang
}
else if (enemy.Type == "Crocodile") {
    AddLootDrop(enemy.Position, "Croc Scale", killerId);
    if (rareRoll <= 25) AddLootDrop(enemy.Position, "Croc Scale", killerId);   // 25% extra scale
    if (rareRoll <= 7)  AddLootDrop(enemy.Position, "Croc Tooth", killerId);   // 7% rare tooth
}
else if (enemy.Type == "Fire Lizard") {
    AddLootDrop(enemy.Position, "Lizard Scale", killerId);
    if (rareRoll <= 20) AddLootDrop(enemy.Position, "Lizard Scale", killerId); // 20% extra scale
    if (rareRoll <= 6)  AddLootDrop(enemy.Position, "Ember Stone", killerId);  // 6% rare ember
}
else if (enemy.Type == "Magma Beetle") {
    AddLootDrop(enemy.Position, "Magma Shard", killerId);
    if (rareRoll <= 25) AddLootDrop(enemy.Position, "Magma Shard", killerId);  // 25% extra shard
    if (rareRoll <= 5)  AddLootDrop(enemy.Position, "Lava Core", killerId);    // 5% rare core
}
else if (enemy.Type == "Eagle") {
    AddLootDrop(enemy.Position, "Feather", killerId);
    if (rareRoll <= 30) AddLootDrop(enemy.Position, "Feather", killerId);      // 30% extra feather
    if (rareRoll <= 7)  AddLootDrop(enemy.Position, "Eagle Talon", killerId);  // 7% rare talon
}
else if (enemy.Type == "Mountain Goat") {
    AddLootDrop(enemy.Position, "Horn", killerId);
    if (rareRoll <= 25) AddLootDrop(enemy.Position, "Fur", killerId);          // 25% fur
    if (rareRoll <= 8)  AddLootDrop(enemy.Position, "Goat Hoof", killerId);    // 8% rare hoof
}
else if (enemy.Type == "Warrior") {
    // Iron armour set — 1-in-5 chance per kill for a random piece
    if (Raylib.GetRandomValue(1, 5) == 1)
    {
        string[] ironSet = { "Iron Helmet", "Iron Chestplate", "Iron Leggings", "Iron Boots", "Iron Gauntlets" };
        string piece = ironSet[Raylib.GetRandomValue(0, ironSet.Length - 1)];
        AddLootDrop(enemy.Position, piece, killerId);
    }
}
else if (enemy.Type == "Wizard") {
    // Apprentice robe (Apprentice Mage) set — 1-in-5 chance per kill
    if (Raylib.GetRandomValue(1, 5) == 1)
    {
        string[] robeSet = { "Apprentice Mage Hat", "Apprentice Mage Top", "Apprentice Mage Bottoms", "Apprentice Mage Boots", "Apprentice Mage Gloves" };
        string piece = robeSet[Raylib.GetRandomValue(0, robeSet.Length - 1)];
        AddLootDrop(enemy.Position, piece, killerId);
    }
}
else if (enemy.Type == "Archer") {
    // Leather (Ranger) set — 1-in-5 chance per kill
    if (Raylib.GetRandomValue(1, 5) == 1)
    {
        string[] leatherSet = { "Leather Cap", "Leather Vest", "Leather Pants", "Leather Boots", "Leather Gloves" };
        string piece = leatherSet[Raylib.GetRandomValue(0, leatherSet.Length - 1)];
        AddLootDrop(enemy.Position, piece, killerId);
    }
}
}

static void EnterBossArena()
{
    ChangeScene(SceneState.BossArena, () =>
    {
        player.Position = new Vector2(ScreenWidth / 2f, ScreenHeight - 120);
        arenaBossPos = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);   // centred in the room
        arenaBossHealth = arenaBossMaxHealth;
        arenaBossDead = false;
        arenaOrbs.Clear(); arenaSpikes.Clear(); arenaMinions.Clear();
        orbTimer = 3f; minionTimer = 8f; spikeTimer = 5f;
    });
}

static void UpdateBossArena(float dt)
{
    // ESC / exit at the door
    if (Raylib.IsKeyPressed(KeyboardKey.Q) && !chestOpen)
        { ChangeScene(SceneState.World, () => player.Position = bossArenaEntrance + new Vector2(0, 80)); return; }

    player.UpdateInterior(dt, arenaObjects, ScreenWidth, ScreenHeight);

    if (Raylib.IsKeyPressed(KeyboardKey.Space))   
    {
        if (chestOpen) { chestOpen = false; openChestId = null; }
        else
        {
            var near = placedChests.FirstOrDefault(c => c.BuildingContext == "BOSS ARENA"
                        && Vector2.Distance(player.Center, c.Position) < 80);
            if (near != null) { chestOpen = true; openChestId = near.Id; }
        }
    }

    if (!arenaBossDead)
    {
        // slow chase
        Vector2 toPlayer = player.Center - arenaBossPos;
        if (toPlayer.Length() > 5) arenaBossPos += Vector2.Normalize(toPlayer) * 45f * dt;

        // contact damage
        bossContactCd -= dt;
        if (bossContactCd <= 0 && Raylib.CheckCollisionRecs(player.Bounds, ArenaBossBounds))
            { player.Health -= 25; bossContactCd = 1f; }

        // MECHANIC 1 — orb ring every 4s, 12 orbs radially
        orbTimer -= dt;
        if (orbTimer <= 0)
        {
            orbTimer = 4f;
            for (int i = 0; i < 12; i++)
            {
                float ang = MathF.Tau * i / 12f;
                arenaOrbs.Add(new ArenaOrb { Pos = arenaBossPos,
                    Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 220f });
            }
        }

        // MECHANIC 2 — spawn 3 minions every 12s (cap 6)
        minionTimer -= dt;
        if (minionTimer <= 0 && arenaMinions.Count < 6)
        {
            minionTimer = 12f;
            for (int i = 0; i < 3; i++)
                arenaMinions.Add(new ArenaMinion { Pos = arenaBossPos + new Vector2(Raylib.GetRandomValue(-200, 200), Raylib.GetRandomValue(-200, 200)) });
        }

        // MECHANIC 3 — spikes telegraph under the player every 6s
        spikeTimer -= dt;
        if (spikeTimer <= 0)
        {
            spikeTimer = 6f;
            for (int i = 0; i < 3; i++)
                arenaSpikes.Add(new ArenaSpike { Pos = player.Center + new Vector2(Raylib.GetRandomValue(-80, 80), Raylib.GetRandomValue(-80, 80)) });
        }
    }

    // orbs
    for (int i = arenaOrbs.Count - 1; i >= 0; i--)
    {
        var o = arenaOrbs[i]; o.Pos += o.Vel * dt; o.Life -= dt;
        if (Raylib.CheckCollisionCircleRec(o.Pos, 10, player.Bounds)) { player.Health -= 12; o.Life = 0; }
        if (o.Life <= 0 || o.Pos.X < 0 || o.Pos.X > ScreenWidth || o.Pos.Y < 0 || o.Pos.Y > ScreenHeight) arenaOrbs.RemoveAt(i);
    }
    // spikes: telegraph → fire once
    for (int i = arenaSpikes.Count - 1; i >= 0; i--)
    {
        var s = arenaSpikes[i]; s.Telegraph -= dt;
        if (s.Telegraph <= 0 && !s.Fired)
            { s.Fired = true; if (Vector2.Distance(player.Center, s.Pos) < 45) player.Health -= 30; }
        if (s.Fired) { s.ActiveTime -= dt; if (s.ActiveTime <= 0) arenaSpikes.RemoveAt(i); }
    }
    // minions chase + contact
    for (int i = arenaMinions.Count - 1; i >= 0; i--)
    {
        var m = arenaMinions[i];
        m.Pos += Vector2.Normalize(player.Center - m.Pos) * m.Speed * dt;
        if (Vector2.Distance(m.Pos, player.Center) < 30 && bossContactCd <= 0)
            { player.Health -= 8; bossContactCd = 0.8f; }
        if (m.Health <= 0) { DropSpiritEssence(m.Pos, 1); arenaMinions.RemoveAt(i); }
    }
}

static void DamageArenaBoss(float dmg)
{
    if (arenaBossDead) return;
    arenaBossHealth -= dmg;
    if (arenaBossHealth <= 0) { arenaBossDead = true; SpawnBossLootChest(); }
}

static void SpawnBossLootChest()
{
    var chest = new PlacedChest {
        Id = "chest_" + Guid.NewGuid().ToString("N").Substring(0, 8),
        Position = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
        BuildingContext = "BOSS ARENA", Tier = 3,
    };
    chest.TryAdd("Spirit Essence", 500);                       // guaranteed
    if (Raylib.GetRandomValue(1, 5) == 1)                      // 1/5 emerald armor table
        { var (item, n) = RollLootTable("emerald_armor"); chest.TryAdd(item, n); }
    var (gItem, gN) = RollLootTable("boss_general");           // guaranteed general roll
    chest.TryAdd(gItem, gN);
    placedChests.Add(chest);
    ShowNotification("The boss dropped a chest!");
} 

static void DrawBossArena()
{
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)24,(byte)18,(byte)28,(byte)255));
    Raylib.DrawRectangleLinesEx(new Rectangle(30, 50, ScreenWidth - 60, ScreenHeight - 90), 6, new Color((byte)70,(byte)50,(byte)90,(byte)255));

    foreach (var s in arenaSpikes)   // telegraph circle, then spike
        if (!s.Fired) Raylib.DrawCircleLines((int)s.Pos.X, (int)s.Pos.Y, 45, new Color((byte)255,(byte)80,(byte)80,(byte)200));
        else Raylib.DrawTriangle(new Vector2(s.Pos.X - 20, s.Pos.Y + 20), new Vector2(s.Pos.X + 20, s.Pos.Y + 20), new Vector2(s.Pos.X, s.Pos.Y - 35), Color.LightGray);

    foreach (var m in arenaMinions)
        Raylib.DrawRectangle((int)m.Pos.X - 14, (int)m.Pos.Y - 14, 28, 28, new Color((byte)120,(byte)40,(byte)140,(byte)255));

    if (!arenaBossDead)
    {
        Raylib.DrawRectangleRec(ArenaBossBounds, new Color((byte)90,(byte)20,(byte)110,(byte)255));
        // boss HP bar
        Raylib.DrawRectangle(ScreenWidth/2 - 300, 14, 600, 22, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(ScreenWidth/2 - 300, 14, (int)(600 * (arenaBossHealth / arenaBossMaxHealth)), 22, new Color((byte)200,(byte)40,(byte)60,(byte)255));
        Raylib.DrawRectangleLines(ScreenWidth/2 - 300, 14, 600, 22, Color.White);
    }

    foreach (var o in arenaOrbs)
        Raylib.DrawCircle((int)o.Pos.X, (int)o.Pos.Y, 10, new Color((byte)255,(byte)120,(byte)200,(byte)255));

    foreach (var c in placedChests.Where(c => c.BuildingContext == "BOSS ARENA"))
    {
        int x = (int)c.Position.X, y = (int)c.Position.Y;
        Raylib.DrawRectangle(x - 22, y - 14, 44, 28, new Color((byte)150,(byte)100,(byte)50,(byte)255));
        Raylib.DrawRectangle(x - 22, y - 14, 44, 8,  new Color((byte)110,(byte)70,(byte)35,(byte)255));  // lid
        Raylib.DrawRectangle(x - 3,  y - 4,  6, 8,   Color.Gold);                                        // latch
        Raylib.DrawRectangleLines(x - 22, y - 14, 44, 28, Color.Black);
        if (Vector2.Distance(player.Center, c.Position) < 80 && !chestOpen)
            Program.DrawTextUI("Space = Open chest", x - 60, y - 40, 16, Color.LightGray);
    }
    player.Draw();                 // your existing player draw call
    Program.DrawTextUI("Q = Leave arena", 40, ScreenHeight - 32, 16, Color.Gray);
}

        record struct EnemyProjectile(Vector2 Pos, Vector2 Vel, string Kind, float Life, int Damage);

        record struct Projectile(Vector2 Pos, Vector2 Vel, string AmmoType, float Life);

        record struct SpellProjectile(Vector2 Pos, Vector2 Vel, string SpellType, float Life, float MaxLife);

        static Color GetSpellColor(string spellType) => spellType switch
        {
            "Rock"      => new Color((byte)120,(byte)90,(byte)60,(byte)255),
            "Wind"      => new Color((byte)180,(byte)230,(byte)180,(byte)255),
            "Water"     => new Color((byte)40,(byte)120,(byte)220,(byte)255),
            "Lightning" => new Color((byte)240,(byte)220,(byte)60,(byte)255),
            "Fire"      => new Color((byte)240,(byte)80,(byte)20,(byte)255),
            "Ice"       => new Color((byte)160,(byte)220,(byte)255,(byte)255),
            "Light"     => new Color((byte)255,(byte)255,(byte)200,(byte)255),
            "Dark"      => new Color((byte)80,(byte)0,(byte)120,(byte)255),
            _ => Color.White
        };

        public static Color GetStaffColor(string staff) => staff switch
        {
            "Rock Staff"      or "Great Rock Staff"      => new Color((byte)120,(byte)90,(byte)60,(byte)255),
            "Wind Staff"      or "Great Wind Staff"      => new Color((byte)180,(byte)230,(byte)180,(byte)255),
            "Water Staff"     or "Great Water Staff"     => new Color((byte)40,(byte)120,(byte)220,(byte)255),
            "Lightning Staff" or "Great Lightning Staff" => new Color((byte)240,(byte)220,(byte)60,(byte)255),
            "Fire Staff"      or "Great Fire Staff"      => new Color((byte)240,(byte)80,(byte)20,(byte)255),
            "Ice Staff"       or "Great Ice Staff"       => new Color((byte)160,(byte)220,(byte)255,(byte)255),
            "Light Staff"     or "Great Light Staff"     => new Color((byte)255,(byte)255,(byte)200,(byte)255),
            "Dark Staff"      or "Great Dark Staff"      => new Color((byte)80,(byte)0,(byte)120,(byte)255),
            _ => new Color((byte)160,(byte)120,(byte)200,(byte)255)
        };

        static bool IsStaff(string item) => item != null && item.EndsWith("Staff");

        static string GetStaffSpell(string staff) => staff switch
        {
            "Rock Staff"      => "Rock",
            "Wind Staff"      => "Wind",
            "Water Staff"     => "Water",
            "Lightning Staff" => "Lightning",
            "Fire Staff"      => "Fire",
            "Ice Staff"       => "Ice",
            "Light Staff"     => "Light",
            "Dark Staff"      => "Dark",
            _ => null
        };

        static int GetSpellDamage(string spellType) => spellType switch
        {
            "Rock"      => 8,
            "Wind"      => 10,
            "Water"     => 12,
            "Lightning" => 15,
            "Fire"      => 18,
            "Ice"       => 20,
            "Light"     => 25,
            "Dark"      => 25,
            _ => 5
        };

static void AwardBossKill(WorldBoss boss, bool isSuper)
{
    int killerId = boss.LastDamagerId >= 0 ? boss.LastDamagerId : multiplayer.MyId;
    DropSpiritEssence(boss.Position, 20);                       // shared-world essence

    if (killerId == multiplayer.MyId)                           // reward is ours
    {
        player.AddCombatXP(isSuper ? 1500 : 500);
        player.Money += isSuper ? 3000 : 1000;
        ShowLevelUp($"{boss.Name} defeated!", 0);
    }
    else
        multiplayer.SendBossKillReward(killerId, isSuper);      // NEW: tell the client (see note)

    if (Raylib.GetRandomValue(1, 100) <= eggDropChance)
    {
        string egg = $"{boss.Name} Egg";
        droppedEggs.Add((boss.Center, egg, 0f));               
        if (multiplayer.Connected && multiplayer.IsHost)
            multiplayer.SendLootDrop(boss.Center.X, boss.Center.Y, egg, killerId);
            if (killerId == multiplayer.MyId)                      
        {
            ShowNotification($" RARE DROP   {egg}!");
            TriggerShake(0.25f);
        }
    }
}

public static void SpawnRemoteVisualProjectile(float x, float y, float vx, float vy, float life, string kind, bool isSpell)
{
    remoteVisualProjectiles.Add(new RemoteVisualProjectile
    {
        Pos = new Vector2(x, y),
        Vel = new Vector2(vx, vy),
        Life = life,
        Kind = kind,
        IsSpell = isSpell
    });
}

public static void SpawnNetworkEnemyProjectile(float x, float y, float vx, float vy, float life, string kind, int damage)
{
    enemyProjectiles.Add(new EnemyProjectile(new Vector2(x, y), new Vector2(vx, vy), kind, life, damage));
}

static void TryFireSpell(Vector2 dir)
{
    // staff must come from the actually-equipped weapon
    string staff = (equipped2H != null && equipped2H.EndsWith("Staff")) ? equipped2H
                 : (equipped1H != null && equipped1H.EndsWith("Staff")) ? equipped1H
                 : null;
    string spell = GetStaffSpell(staff);

    if (spell == null)
    {
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 40),
            Text = "Equip a staff first!",
            Timer = 1.5f, TextColor = Color.Purple
        });
        return;
    }

    // essence must be loaded in the ammo slot
    if (equippedAmmo != "Arcane Essence")
    {
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 40),
            Text = "Load Arcane Essence in your ammo slot!",
            Timer = 1.5f, TextColor = Color.Purple
        });
        return;
    }

    if (player.ArcaneEssence <= 0)
    {
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 40),
            Text = "Not enough Arcane Essence!",
            Timer = 1.5f, TextColor = Color.Purple
        });
        return;
    }

    if (spellCooldown > 0) return;

    float maxLife = 1.2f + (player.ElementalLevel * 0.015f);
    float speed   = 380f;
    spellCooldown = 0.5f;

    player.ArcaneEssence--;
    spellProjectiles.Add(new SpellProjectile(player.Center, dir * speed, spell, maxLife, maxLife));
    multiplayer.SendProjectile(player.Center.X, player.Center.Y, (dir * speed).X, (dir * speed).Y,
    maxLife, spell, true);
    player.AddElementalXP(1);

    // auto-clear ammo slot when essence runs out
    if (player.ArcaneEssence <= 0) equippedAmmo = null;
}

static void UpdateEnemyProjectiles(float dt)
{
    for (int i = enemyProjectiles.Count - 1; i >= 0; i--)
    {
        var p = enemyProjectiles[i];
        p = p with { Pos = p.Pos + p.Vel * dt, Life = p.Life - dt };

        if (p.Life <= 0) { enemyProjectiles.RemoveAt(i); continue; }

        // hit the player?
        Rectangle box = new Rectangle(p.Pos.X - 6, p.Pos.Y - 6, 12, 12);
        if (Raylib.CheckCollisionRecs(box, player.Bounds))
        {
            int reduced = Math.Max(1, p.Damage - GetTotalDefense());
            player.TakeDamage(reduced);
            TriggerShake(0.18f);
            floatingTexts.Add(new FloatingText {
                Position = player.Position - new Vector2(0, 20),
                Text = $"-{reduced}",
                Timer = 1f,
                TextColor = p.Kind == "Arrow" ? Color.Orange : Color.Violet
            });
            enemyProjectiles.RemoveAt(i);
            continue;
        }

        enemyProjectiles[i] = p;
    }
}

static void UpdateSpellProjectiles(float dt)
{
    if (spellCooldown > 0) spellCooldown -= dt;

    for (int i = spellProjectiles.Count - 1; i >= 0; i--)
    {
        var p = spellProjectiles[i];
        p = p with { Pos = p.Pos + p.Vel * dt, Life = p.Life - dt };

        if (p.Life <= 0) { spellProjectiles.RemoveAt(i); continue; }

        bool hit = false;
        foreach (var enemy in enemies)
        {
            if (enemy.Dead) continue;
            Rectangle bounds = new Rectangle(p.Pos.X - 6, p.Pos.Y - 6, 12, 12);
            if (!Raylib.CheckCollisionRecs(bounds, enemy.Bounds)) continue;

            int baseDmg  = GetSpellDamage(p.SpellType);
            int lvlBonus = player.ElementalLevel / 5;
            int dmg      = baseDmg + lvlBonus;

            int actualDmg = Math.Min(dmg, enemy.Health);

            if (!multiplayer.Connected || multiplayer.IsHost)
            {
                enemy.LastDamagerId = multiplayer.MyId;
                enemy.Health -= dmg;
                if (enemy.Health <= 0) HandleEnemyDeath(enemy);
            }
            else
            {
                multiplayer.SendEnemyHit(enemies.IndexOf(enemy), dmg, multiplayer.MyId);
            }

            enemy.TriggerFlash();
            SpawnSplat(enemy.Center, GetSpellColor(p.SpellType));
            if (enemy.Health <= 0)
            {
                enemy.Dead = true;
                DropSpiritEssence(enemy.Position, 1);
                SpawnDeathFx(enemy.Center, enemy.EnemyColor, enemy.Type);
            }

            floatingTexts.Add(new FloatingText {
                Position = enemy.Position - new Vector2(0, 20),
                Text = $"-{dmg}",
                Timer = 1f,
                TextColor = GetSpellColor(p.SpellType)
            });

            player.AddElementalXP(Math.Max(1, actualDmg));
            hit = true;
            break;
        }

        // ── world boss hit ──
        if (!hit && worldBoss != null && !worldBoss.Dead
    && Raylib.CheckCollisionPointRec(p.Pos, worldBoss.Bounds))
{
    int dmg = GetSpellDamage(p.SpellType) + player.ElementalLevel / 3 + GetArmorStyleBonus().magic;

    if (multiplayer.IsHost || !multiplayer.Connected)
    {
        // host (or single-player) applies damage directly
        worldBoss.Health -= dmg;
        if (worldBoss.Health <= 0)
        {
            worldBoss.Dead = true;
            DropSpiritEssence(worldBoss.Position, 20);
            player.AddCombatXP(500);
            player.Money += 1000;
            ShowLevelUp($"{worldBoss.Name} defeated!", 0);
        }
        if (multiplayer.Connected)
            multiplayer.BroadcastBossState(false, worldBoss.Health, worldBoss.MaxHealth, worldBoss.Dead,
                worldBoss.Position.X, worldBoss.Position.Y);
    }
    else
    {
        // client: report the hit, let the host apply real damage and broadcast back
        multiplayer.SendBossHit(false, dmg, multiplayer.MyId);
    }

    floatingTexts.Add(new FloatingText {
        Position = worldBoss.Center - new Vector2(0, worldBoss.Size / 2f),
        Text = $"-{dmg}", Timer = 1f, TextColor = GetSpellColor(p.SpellType)
    });
    player.AddElementalXP(dmg);
    hit = true;
}

        if (hit) spellProjectiles.RemoveAt(i);
        else     spellProjectiles[i] = p;
    }
}

static void TryFireProjectile(Vector2 dir)
{
    
    bool isBow      = equipped2H != null && equipped2H.Contains("Bow") && !equipped2H.Contains("Crossbow");
    bool isCrossbow = equipped2H != null && equipped2H.Contains("Crossbow");

    bool hasBow      = isBow      && equippedAmmo == "Arrows" && player.Arrows > 0;
    bool hasCrossbow = isCrossbow && equippedAmmo == "Bolts"  && player.Bolts  > 0;

    if (!hasBow && !hasCrossbow)
    {
        if (isBow || isCrossbow)
        {
            string requiredAmmo = isCrossbow ? "Bolts" : "Arrows";
            int    have         = isCrossbow ? player.Bolts : player.Arrows;
            string msg = have <= 0
                ? $"Out of {requiredAmmo}! This {equipped2H} needs {requiredAmmo}."
                : $"Equip {requiredAmmo} as ammo to fire the {equipped2H}!";

            ShowNotification(msg);
        }
        return;
    }

    float cooldown = hasCrossbow ? 1.2f : 0.6f;
    if (bowCooldown > 0) return;
    bowCooldown = cooldown;

    float speed = hasCrossbow ? 700f : 500f;

    projectiles.Add(new Projectile(player.Center, dir * speed,
        hasCrossbow ? "Bolts" : "Arrows", 3f));

    multiplayer.SendProjectile(player.Center.X, player.Center.Y, (dir * speed).X, (dir * speed).Y,
    3f, hasCrossbow ? "Bolts" : "Arrows", false);

    if (hasCrossbow) player.Bolts--;
    else             player.Arrows--;
    player.AddRangedXP(1);
}

static void UpdateProjectiles(float dt)
{
    
    if (bowCooldown > 0) bowCooldown -= dt;

    for (int i = projectiles.Count - 1; i >= 0; i--)
    {
        var p = projectiles[i];
        p = p with { Pos = p.Pos + p.Vel * dt, Life = p.Life - dt };

        if (p.Life <= 0) { projectiles.RemoveAt(i); continue; }

        bool hit = false;
        foreach (var enemy in enemies)
        {
            if (enemy.Dead) continue;
            if (!Raylib.CheckCollisionPointRec(p.Pos, enemy.Bounds)) continue;

            int baseDmg  = p.AmmoType == "Bolts" ? 18 : 12;
            int dmg = baseDmg + (player.RangedLevel / 5) + GetArmorStyleBonus().ranged;
            if (!multiplayer.Connected || multiplayer.IsHost)
            {
                enemy.LastDamagerId = multiplayer.MyId;
                enemy.Health -= dmg;
                if (enemy.Health <= 0) HandleEnemyDeath(enemy);
            }
            else
            {
                multiplayer.SendEnemyHit(enemies.IndexOf(enemy), dmg, multiplayer.MyId);
            }
            enemy.TriggerFlash();
            SpawnSplat(enemy.Center, new Color((byte)170, (byte)20, (byte)20, (byte)255));
            if (enemy.Health <= 0)
            {
                HandleEnemyDeath(enemy);
            }


            floatingTexts.Add(new FloatingText {
                Position = enemy.Position - new Vector2(0, 20),
                Text = $"-{dmg}",
                Timer = 1f,
                TextColor = Color.Orange
            });

            int actualDmg = Math.Min(dmg, Math.Max(0, enemy.Health + dmg)); // HP dealt
            player.AddRangedXP(Math.Max(1, actualDmg));
            hit = true;  
            break; 
        }

        // also check the world boss
        if (!hit && worldBoss != null && !worldBoss.Dead
    && Raylib.CheckCollisionPointRec(p.Pos, worldBoss.Bounds))
{
    int dmg = p.AmmoType == "Bolts" ? 18 : 12;

    if (multiplayer.IsHost || !multiplayer.Connected)
    {
        worldBoss.Health -= dmg;
        if (worldBoss.Health <= 0) worldBoss.Dead = true;
        if (multiplayer.Connected)
            multiplayer.BroadcastBossState(false, worldBoss.Health, worldBoss.MaxHealth, worldBoss.Dead,
                worldBoss.Position.X, worldBoss.Position.Y);
    }
    else
    {
        multiplayer.SendBossHit(false, dmg, multiplayer.MyId);
    }

    player.AddRangedXP(p.AmmoType == "Bolts" ? 6 : 4);
    hit = true;
}

        if (hit) projectiles.RemoveAt(i);
        else     projectiles[i] = p;
    }
}

static void UpdateRemoteVisualProjectiles(float dt)
{
    for (int i = remoteVisualProjectiles.Count - 1; i >= 0; i--)
    {
        var p = remoteVisualProjectiles[i];
        p.Pos += p.Vel * dt;
        p.Life -= dt;
        if (p.Life <= 0) remoteVisualProjectiles.RemoveAt(i);
    }
}

static void UpdateWorldBoss(WorldBoss boss, float dt)
{
    if (boss == null) return;
    bool isSuper = boss.ShakesWhenNear;
    bool amHost = multiplayer.IsHost && multiplayer.Connected;

    if (amHost)
    {
        boss.Update(dt, NearestPlayerTo(boss.Center));

        if (isSuper)
        {
            superBossSyncTimer -= dt;
            if (superBossSyncTimer <= 0f)
            {
                superBossSyncTimer = 0.1f;
                multiplayer.BroadcastBossState(true, boss.Health, boss.MaxHealth, boss.Dead,
                    boss.Position.X, boss.Position.Y);
            }
        }
        else
        {
            bossSyncTimer -= dt;
            if (bossSyncTimer <= 0f)
            {
                bossSyncTimer = 0.1f;
                multiplayer.BroadcastBossState(false, boss.Health, boss.MaxHealth, boss.Dead,
                    boss.Position.X, boss.Position.Y);
            }
        }
    }
    else if (!multiplayer.Connected)
    {
        boss.Update(dt, NearestPlayerTo(boss.Center));
    }
    // else: client — boss state comes entirely from BossStateReceived; no local Update() at all.

    if (boss.Dead) return;

    // proximity rumble for super bosses
    if (boss.ShakesWhenNear)
    {
        float dist = Vector2.Distance(player.Center, boss.Center);
        if (dist < boss.ProximityShakeRange)
        {
            float intensity = 0.15f * (1f - dist / boss.ProximityShakeRange);
            TriggerShake(intensity);
        }
    }

    // contact damage
    if (Raylib.CheckCollisionRecs(player.Bounds, boss.Bounds) && boss.AttackCooldown <= 0)
    {
        int def = GetTotalDefense();
        int dmg = Math.Max(1, boss.ContactDamage - def);
        player.TakeDamage(dmg);
        boss.AttackCooldown = 1.0f;
        TriggerShake(0.3f);
        floatingTexts.Add(new FloatingText {
            Position = player.Position - new Vector2(0, 20),
            Text = $"-{dmg}", Timer = 1f, TextColor = Color.Red
        });
    }

    // player melee
    float reach = 70f;
    Rectangle hitBox = new Rectangle(
        boss.Position.X - reach, boss.Position.Y - reach,
        boss.Size + reach * 2, boss.Size + reach * 2);

    if (Raylib.CheckCollisionRecs(player.Bounds, hitBox)
        && (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left))
        && currentPhase == HandPhase.Combat)
    {
        string equipped = GetActiveWeapon();
        if (equipped != null && GetItemSlot(equipped) == "WEAPON")
        {
            int dmg = 1 + (player.CombatLevel / 10) + GetWeaponDamage(equipped) + GetArmorStyleBonus().melee;

            if (amHost || !multiplayer.Connected)
            {
                boss.LastDamagerId = multiplayer.MyId;
                boss.Health -= dmg;
                if (boss.Health <= 0)
                {
                    boss.Dead = true;
                    AwardBossKill(boss, isSuper);
                }
                if (multiplayer.Connected)
                    multiplayer.BroadcastBossState(isSuper, boss.Health, boss.MaxHealth, boss.Dead,
                        boss.Position.X, boss.Position.Y);
            }
            else
            {
                multiplayer.SendBossHit(isSuper, dmg, multiplayer.MyId);
            }

            floatingTexts.Add(new FloatingText {
                Position = boss.Center - new Vector2(0, boss.Size / 2f),
                Text = $"-{dmg}", Timer = 1f, TextColor = Color.Orange
            });
        }
    }
}

static void AwardMeleeXP(string weapon, int amount)
{
    if (IsTwoHandedMeleeWeapon(weapon)) player.AddTwoHandMeleeXP(amount);
    else                                player.AddOneHandMeleeXP(amount);
}

static void SpawnDeathFx(Vector2 center, Color tint, string type)
{
    deathFx.Add(new DeathFx {
        Position = center, Type = type,
        Timer = 0.6f, MaxTimer = 0.6f, TintColor = tint
    });
    // burst of corpse particles, reusing the splat system
    SpawnSplat(center, tint, 18);

    // fire/magma types throw extra orange embers
    if (type == "Fire Lizard" || type == "Magma Beetle")
        SpawnSplat(center, new Color((byte)240,(byte)120,(byte)20,(byte)255), 12);

    
}

static void UpdateDeathFx(float dt)
{
    for (int i = deathFx.Count - 1; i >= 0; i--)
    {
        var d = deathFx[i];
        d.Timer -= dt;
        if (d.Timer <= 0f) { deathFx.RemoveAt(i); continue; }
        deathFx[i] = d;
    }
}

static void DrawDeathFx()
{
    foreach (var d in deathFx)
    {
        float p = 1f - (d.Timer / d.MaxTimer);   // 0 → 1 progress
        float a = d.Timer / d.MaxTimer;           // 1 → 0 fade

        // expanding shockwave ring
        float ringR = 6f + p * 38f;
        Raylib.DrawCircleLines((int)d.Position.X, (int)d.Position.Y, ringR,
            new Color(d.TintColor.R, d.TintColor.G, d.TintColor.B, (byte)(200 * a)));

        // fading, rising silhouette
        int sy = (int)(d.Position.Y - p * 18f);
        Raylib.DrawCircle((int)d.Position.X, sy, (int)(14 * a),
            new Color(d.TintColor.R, d.TintColor.G, d.TintColor.B, (byte)(140 * a)));
    }
}
    }
}
