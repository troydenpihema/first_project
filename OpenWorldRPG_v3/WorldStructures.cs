using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
class BinderSlotData { public string Name; public bool IsReverse; public int Count; }
class CardBinder
{
    public Dictionary<string,int> Cards = new();                      // Master Set only — fixed layout, global counts
    public Dictionary<string,int> ReverseHoloCards = new();
    public Dictionary<int,BinderSlotData> SlotAssignments = new();     // Personal Collection only — per-slot independent stacks
    const int MaxPerCard = 100;

    public void Add(string name, bool reverseHolo)
    {
        var dict = reverseHolo ? ReverseHoloCards : Cards;
        int current = dict.GetValueOrDefault(name);
        if (current < MaxPerCard) dict[name] = current + 1;
    }

    public bool RemoveOne(string name, bool reverseHolo)
    {
        var dict = reverseHolo ? ReverseHoloCards : Cards;
        if (dict.GetValueOrDefault(name) <= 0) return false;
        dict[name]--;
        if (dict[name] <= 0) dict.Remove(name);
        return true;
    }
}
class DroppedItem
{
    public string Name;
    public int Count;
    public Vector2 Position;
    public float Lifetime = 30f;
    public DroppedItem(string name, int count, Vector2 pos)
    {
        Name = name; Count = count; Position = pos;
    }
}
class HouseFurniture
{
    public string Type;      // "Bed", "Sofa", "Table", "Lamp", "Tv", "Plant", "Rug", "Shelf"
    public int    RoomX;     // interior position
    public int    RoomY;
    public int    Cost;
    public Color  Col;
    public int    Rotation = 0;

    public HouseFurniture(string type, int x, int y, int cost, Color col)
    {
        Type = type; RoomX = x; RoomY = y; Cost = cost; Col = col;
    }
}

class HouseData
{
    public int PlotX;
    public int PlotY;
    public string WallColor  = "Beige";
    public string FloorColor = "Oak";
    public List<HouseFurniture> Furniture = new List<HouseFurniture>();
    public string HouseType = "Standard";

    public HouseData(int x, int y) { PlotX = x; PlotY = y; }
}

class DungeonEnemy
{
    public Vector2 Position;
    public float Health, MaxHealth;
    public Vector2 Knockback = Vector2.Zero;
    public string Type;
    public Color EnemyColor;
    public bool Dead = false;
    public float AttackCooldown = 0f;
    public int Damage;
    public float Speed;
    public string LootType;
    public int XPReward;
    public int MoneyDrop;

    public Rectangle Bounds => new Rectangle(Position.X - 20, Position.Y - 20, 40, 40);

    public DungeonEnemy(Vector2 pos, string type, float hp, int dmg, float spd, Color col, string loot, int xp, int money = 0)
    {
        Position = pos; Type = type; Health = MaxHealth = hp;
        Damage = dmg; Speed = spd; EnemyColor = col;
        LootType = loot; XPReward = xp; MoneyDrop = money;
    }

    public void Update(float dt, Vector2 playerPos)
{
    if (Dead) return;
    if (AttackCooldown > 0) AttackCooldown -= dt;

    // apply knockback and decay it
    if (Knockback.Length() > 0.1f)
    {
        Position += Knockback * dt;
        Knockback *= 0.85f; // friction
    }
    else
    {
        Knockback = Vector2.Zero;
        Vector2 dir = playerPos - Position;
        float dist = dir.Length();
        if (dist > 5f)
            Position += Vector2.Normalize(dir) * Speed * dt;
    }
}

    public void Draw()
    {
        if (Dead) return;
        int x = (int)Position.X, y = (int)Position.Y;
        Raylib.DrawCircle(x, y, 22, EnemyColor);
        Raylib.DrawCircle(x, y, 18, new Color(
            (byte)Math.Min(255, EnemyColor.R + 30),
            (byte)Math.Min(255, EnemyColor.G + 30),
            (byte)Math.Min(255, EnemyColor.B + 30), (byte)255));
        // health bar
        Raylib.DrawRectangle(x - 22, y - 34, 44, 6, Color.DarkGray);
        Raylib.DrawRectangle(x - 22, y - 34, (int)(44 * (Health / MaxHealth)), 6, Color.Red);
        // name
        int tw = Program.MeasureTextUI(Type, 12);
        Program.DrawTextUI(Type, x - tw / 2, y - 48, 12, Color.White);
    }
}
class TutorialGate
{
    public Rectangle Bounds;
    public int UnlockedByStep;   // becomes passable when tutorialStep > this
    public string Label;
}
class PlacedChest
{
    public string Id;                  // unique, e.g. "chest_" + Guid
    public Vector2 Position;
    public string BuildingContext;     // BuildingName it belongs to, "" if world-placed
    public int Tier = 0;               // 0=30, 1=40, 2=50
    public Dictionary<string,int> Contents = new();

    public int Capacity => tierCapacities[Tier];
    static readonly int[] tierCapacities = { 20, 30, 40, 50 };

    public int UsedSlots => Contents.Count(kv => kv.Value > 0);

    public bool TryAdd(string item, int count)
    {
        if (count <= 0) return true;
        if (!Contents.ContainsKey(item) && UsedSlots >= Capacity) return false;
        Contents[item] = Contents.GetValueOrDefault(item) + count;
        return true;
    }

    public void RemoveOne(string item)
    {
        if (!Contents.ContainsKey(item)) return;
        Contents[item]--;
        if (Contents[item] <= 0) Contents.Remove(item);
    }
}

class DungeonLoot
{
    public Vector2 Position;
    public string ItemType;
    public bool Collected = false;

    public DungeonLoot(Vector2 pos, string item) { Position = pos; ItemType = item; }
}

class DungeonRoom
{
    public List<DungeonEnemy> Enemies = new();
    public List<DungeonLoot> Loot = new();
    public bool IsBoss = false;
    public bool ChestOpened = false;
    public bool AllEnemiesDead => Enemies.All(e => e.Dead);
}

class Dungeon
{
    public bool IsOpen = false;
    public string Name = "";
    public string Type = "";
    public int CurrentRoom = 0;
    public int TotalRooms = 5;
    public DungeonRoom[] Rooms;
    public bool Complete = false;
    public string Message = "";
    public float MessageTimer = 0f;
    public Vector2 PlayerPos;
    public Vector2 WorldReturnPos;

    public const int WallThick = 22;
    public const int RoomX = 60;
    public const int RoomY = 80;
    public const int RoomW = 1160;
    public const int RoomH = 520;
    public const int InnerX = RoomX + WallThick;
    public const int InnerY = RoomY + WallThick;
    public const int InnerW = RoomW - WallThick * 2;
    public const int InnerH = RoomH - WallThick * 2;

    public bool IsLastRoom => CurrentRoom == TotalRooms - 1;
    public bool CollidesWithEnemy(Vector2 pos, float radius, DungeonEnemy ignore = null)
{
    var room = Rooms[CurrentRoom];
    foreach (var enemy in room.Enemies)
    {
        if (enemy.Dead) continue;
        if (enemy == ignore) continue;
        if (Vector2.Distance(pos, enemy.Position) < radius + 22f)
            return true;
    }
    return false;
}

    public void Open(string type, string name, Vector2 worldPos)
    {
        IsOpen = true; Type = type; Name = name;
        CurrentRoom = 0; Complete = false;
        WorldReturnPos = worldPos;
        PlayerPos = new Vector2(InnerX + 80, InnerY + InnerH / 2f);
        Message = "Clear the room to advance!";
        MessageTimer = 2.5f;

        Rooms = new DungeonRoom[TotalRooms];
        for (int i = 0; i < TotalRooms; i++)
        {
            Rooms[i] = new DungeonRoom { IsBoss = (i == TotalRooms - 1) };
            GenerateRoom(i);
        }
    }

    public void Close() { IsOpen = false; }

    void GenerateRoom(int idx)
    {
        var room = Rooms[idx];
        int count = room.IsBoss ? 1 : Math.Min(2 + idx, 6);
        for (int i = 0; i < count; i++)
        {
            float ex = InnerX + 300 + Raylib.GetRandomValue(0, InnerW - 420);
            float ey = InnerY + 50  + Raylib.GetRandomValue(0, InnerH - 100);
            room.Enemies.Add(SpawnEnemy(new Vector2(ex, ey), room.IsBoss));
        }
    }

    DungeonEnemy SpawnEnemy(Vector2 pos, bool boss)
    {
        float hm = boss ? 4f : 1f;
        float sm = boss ? 0.65f : 1f;
        int roll = Raylib.GetRandomValue(0, 1);

        return Type switch
        {
            "Forest" => boss
                ? new DungeonEnemy(pos, "Ancient Bear",   150*hm, 20, 65*sm, new Color((byte)80,(byte)40,(byte)100,(byte)255), "Bear Pelt", 120, 40)
                : roll == 0
                    ? new DungeonEnemy(pos, "Wolf",       35, 10, 90,  Color.DarkGray, "Fur",  35, 6)
                    : new DungeonEnemy(pos, "Wild Dog",   22,  6, 100, Color.Brown,    "Bone", 20, 4),

            "Snow" => boss
                ? new DungeonEnemy(pos, "Frost Titan",   180*hm, 22, 55*sm, new Color((byte)160,(byte)200,(byte)255,(byte)255), "Bear Pelt", 140, 50)
                : roll == 0
                    ? new DungeonEnemy(pos, "Ice Wolf",  40, 12, 85, new Color((byte)180,(byte)210,(byte)240,(byte)255), "Fur",       40, 8)
                    : new DungeonEnemy(pos, "Snow Bear",  55, 15, 65, new Color((byte)100,(byte)100,(byte)130,(byte)255), "Bear Pelt", 55, 10),

            "Desert" => boss
                ? new DungeonEnemy(pos, "Dune Warlord",  160*hm, 20, 60*sm, new Color((byte)220,(byte)150,(byte)0,(byte)255),  "Stinger", 130, 45)
                : roll == 0
                    ? new DungeonEnemy(pos, "Scorpion",  28,  8, 80, new Color((byte)180,(byte)120,(byte)0,(byte)255),  "Stinger", 30, 5)
                    : new DungeonEnemy(pos, "Sand Snake", 22, 10, 95, new Color((byte)200,(byte)160,(byte)40,(byte)255), "Stinger", 25, 4),

            "Volcano" => boss
                ? new DungeonEnemy(pos, "Magma Colossus", 200*hm, 25, 50*sm, new Color((byte)255,(byte)60,(byte)0,(byte)255),  "Bear Pelt", 150, 60)
                : roll == 0
                    ? new DungeonEnemy(pos, "Fire Lizard",  32, 12, 85, new Color((byte)180,(byte)60,(byte)10,(byte)255),  "Stinger", 35, 7)
                    : new DungeonEnemy(pos, "Magma Beetle", 42, 15, 70, new Color((byte)120,(byte)30,(byte)0,(byte)255),   "Stinger", 40, 8),

            _ => boss  // Crypt
                ? new DungeonEnemy(pos, "Skeleton King",  120*hm, 16, 55*sm, new Color((byte)220,(byte)210,(byte)180,(byte)255), "Bone", 100, 30)
                : roll == 0
                    ? new DungeonEnemy(pos, "Skeleton", 22,  7, 80, new Color((byte)220,(byte)215,(byte)195,(byte)255), "Bone", 20, 3)
                    : new DungeonEnemy(pos, "Ghoul",    18,  9, 90, new Color((byte)80,(byte)120,(byte)80,(byte)255),   "Bone", 18, 2)
        };
    }
}

class Pet
{
    public Vector2 Position;
    public string Type;          
    public Color BodyColor;
    public bool Adopted = false;          
    public int Bond = 0;                   
    public int StrengthXP = 0;             
    public int MagicXP = 0;
    public int SocialXP = 0;
    public bool AtSanctuary = false;       
    public string AdultRole = ""; 
    public bool IsBaby = false;          
    public string AdultType = "";        // what this evolves into ("Cat"/"Dog")
    public float AgeTimer = 0f;          // accumulates real dt while following
    public float MatureAfter = 120f;     // seconds of game time to grow up
    public bool Matured = false;
    float bobTimer = 0f;
    Vector2 velocity = Vector2.Zero;
    public static Pet NewBaby(Vector2 pos, string babyType, string adultType, Color color, float matureAfter = 120f)
    {
        var p = new Pet(pos, babyType, color);
        p.IsBaby = true;
        p.AdultType = adultType;
        p.MatureAfter = matureAfter;
        return p;
    }

    public Pet(Vector2 pos, string type, Color color)
    {
        Position = pos; Type = type; BodyColor = color;
    }

    // follows the player, keeping a small trailing distance
    public void Update(float dt, Vector2 playerPos)
    {
        if (Vector2.Distance(Position, playerPos) > 700f)
        {
            // drop in just behind the player rather than exactly on them
            Position = playerPos - new Vector2(40f, 0f);
            return;
        }
        if (IsBaby && !Matured)
        {
            AgeTimer += dt;
            if (AgeTimer >= MatureAfter)
            {
                Matured = true;
                IsBaby = false;
                Type = AdultType; 
                if (StrengthXP >= MagicXP && StrengthXP >= SocialXP) AdultRole = "Guard";
                else if (MagicXP >= SocialXP)                        AdultRole = "Mage";
                else                                                 AdultRole = "Friend";
                Program.ShowNotification($"Your {AdultType} grew up as a {AdultRole}! (Bond {Bond})");                // "Kitten" -> "Cat", "Puppy" -> "Dog"
                Program.ShowNotification($"Your pet grew into a {AdultType}!");
            }
        }
        bobTimer += dt * 4f;
        Vector2 toPlayer = playerPos - Position;
        float dist = toPlayer.Length();
        if (dist > 70f)
            Position += Vector2.Normalize(toPlayer) * 160f * dt;   // catch up
        else if (dist > 50f)
            Position += Vector2.Normalize(toPlayer) * 60f * dt;    // amble
    }

    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y + (int)(MathF.Sin(bobTimer) * 3f);   // gentle hover bob
        // body
        float sc = IsBaby ? 0.65f : 1f;   
        Raylib.DrawCircle(x, y, 12, BodyColor);
        Raylib.DrawCircle(x, y, 12, new Color((byte)0,(byte)0,(byte)0,(byte)60)); // soft outline
        Raylib.DrawCircle(x, y, 9, new Color(
            (byte)Math.Min(255, BodyColor.R + 40),
            (byte)Math.Min(255, BodyColor.G + 40),
            (byte)Math.Min(255, BodyColor.B + 40), (byte)255));
        // eyes
        Raylib.DrawCircle(x - 4, y - 3, 2, Color.White);
        Raylib.DrawCircle(x + 4, y - 3, 2, Color.White);
        Raylib.DrawCircle(x - 4, y - 3, 1, Color.Black);
        Raylib.DrawCircle(x + 4, y - 3, 1, Color.Black);
    }
}

class WorldBoss
{
    public Vector2 Position;
    public float Health, MaxHealth;
    public bool Dead = false;
    public bool Aggro = false;
    public string Name;
    public Color BodyColor;
    public int LastDamagerId = -1;

    public Vector2 SpawnPosition;
    public float RoamRadius;       // how far it wanders from spawn
    public float AggroRange = 400f;
    public float DeAggroRange = 1200f;
    public float WanderSpeed = 60f;
    public float ChaseSpeed = 140f;
    public int ContactDamage = 35;
    public float AttackCooldown = 0f;
    public bool ShakesWhenNear = false;   // super-boss proximity rumble
    public float ProximityShakeRange = 1000f;

    Vector2 wanderTarget;
    float wanderTimer = 0f;
    float animTimer = 0f;
    float respawnTimer = 0f;

    public int Size = 220;   // much bigger than buildings

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Size, Size);
    public Vector2 Center => new Vector2(Position.X + Size / 2f, Position.Y + Size / 2f);

    public WorldBoss(Vector2 pos, string name, float hp, float roamRadius, Color color)
    {
        Position = pos;
        SpawnPosition = pos;
        Name = name;
        Health = MaxHealth = hp;
        RoamRadius = roamRadius;
        BodyColor = color;
        wanderTarget = pos;
    }

    public void Update(float dt, Vector2 playerPos)
    {
        animTimer += dt;
        if (AttackCooldown > 0) AttackCooldown -= dt;

        if (Dead)
        {
            respawnTimer += dt;
            if (respawnTimer >= 10f)   // boss respawns after 1 minute
            {
                Dead = false;
                Health = MaxHealth;
                Position = SpawnPosition;
                Aggro = false;
                respawnTimer = 0f;
            }
            return;
        }

        float distToPlayer = Vector2.Distance(Center, playerPos);

        if (!Aggro && distToPlayer < AggroRange) Aggro = true;
        if (Aggro && distToPlayer > DeAggroRange) Aggro = false;

        if (Aggro)
        {
            Vector2 dir = playerPos - Center;
            if (dir.Length() > 5f)
                Position += Vector2.Normalize(dir) * ChaseSpeed * dt;
        }
        else
        {
            wanderTimer -= dt;
            if (wanderTimer <= 0)
            {
                // pick a random point within RoamRadius of spawn
                float ang = Raylib.GetRandomValue(0, 360) * MathF.PI / 180f;
                float r = Raylib.GetRandomValue(0, (int)RoamRadius);
                wanderTarget = SpawnPosition + new Vector2(MathF.Cos(ang) * r, MathF.Sin(ang) * r);
                wanderTimer = Raylib.GetRandomValue(3, 7);
            }
            Vector2 toTarget = wanderTarget - Position;
            if (toTarget.Length() > 5f)
                Position += Vector2.Normalize(toTarget) * WanderSpeed * dt;
        }

        // clamp so it never strays too far from spawn even while chasing
        Vector2 fromSpawn = Position - SpawnPosition;
        float maxStray = RoamRadius + 400f;
        if (fromSpawn.Length() > maxStray)
            Position = SpawnPosition + Vector2.Normalize(fromSpawn) * maxStray;
    }

    public void Draw()
    {
        if (Dead) return;
        int x = (int)Position.X, y = (int)Position.Y;
        int s = Size;
        float bob = MathF.Sin(animTimer * 2f) * 8f;

        // huge body
        Raylib.DrawRectangle(x, y + (int)bob, s, s, BodyColor);
        // darker core
        Raylib.DrawRectangle(x + s/6, y + s/6 + (int)bob, s*2/3, s*2/3,
            new Color((byte)Math.Max(0, BodyColor.R-40), (byte)Math.Max(0, BodyColor.G-40), (byte)Math.Max(0, BodyColor.B-40), (byte)255));
        // glowing eyes
        Raylib.DrawCircle(x + s/3, y + s/3 + (int)bob, 18, Color.Red);
        Raylib.DrawCircle(x + 2*s/3, y + s/3 + (int)bob, 18, Color.Red);

        // aggro indicator
        if (Aggro)
            Program.DrawTextUI("!", x + s/2 - 8, y - 40 + (int)bob, 48, Color.Red);

        // big health bar above
        int barW = s;
        Raylib.DrawRectangle(x, y - 30 + (int)bob, barW, 16, Color.DarkGray);
        Raylib.DrawRectangle(x, y - 30 + (int)bob, (int)(barW * (Health / MaxHealth)), 16, Color.Red);
        int tw = Program.MeasureTextUI(Name, 24);
        Program.DrawTextUI(Name, x + s/2 - tw/2, y - 60 + (int)bob, 24, Color.White);
    }
}
class FruitTree
{
    public Vector2 Position;
    public string FruitType;
    public bool Tilled = false;  
    public bool Planted = false;      
    public float GrowTimer = 0f;
    public float GrowDuration;    
    public bool ReadyToHarvest = false;
    public float RegrowTimer = 0f;
    public const float RegrowDuration = 60f;   
}

class FarmPlot
{
    public Vector2 Position;
    public bool Tilled = false;
    public bool Planted = false;
    public bool Watered = false;
    public string CropType = "";       // "Wheat", "Carrot", "Potato", "Tomato"
    public float GrowTimer = 0f;
    public float GrowDuration = 30f;   // set from cropGrowDuration when planted
    public bool ReadyToHarvest = false;
}
class Garage
{
    public Rectangle Bounds;
    public int OwnerHouseIndex;
    public int Capacity = 1;                                              
    public bool IsDock = false;                                           
    public List<(Vehicle.VehicleType type, Color color)> Slots = new();   
    public bool HasVehicle => Slots.Count > 0;
    public bool IsFull => Slots.Count >= Capacity;
}
class Stable
{
    public enum StableKind { Basic, Advanced, BikeRack, Aquatic }
    public StableKind Kind;
    public Rectangle Bounds;
    public int OwnerHouseIndex;
    public int Capacity = 1;
    public List<(Rideable.RideableType type, Color color)> Slots = new();
    public bool HasAnimal => Slots.Count > 0;
    public bool IsFull => Slots.Count >= Capacity;

    public bool Accepts(Rideable.RideableType t) => Kind switch
    {
        StableKind.Basic    => t is Rideable.RideableType.Horse or Rideable.RideableType.Donkey or Rideable.RideableType.Camel,
        StableKind.Advanced => t != Rideable.RideableType.MountainBike && t != Rideable.RideableType.BMX && t != Rideable.RideableType.Dolphin,
        StableKind.BikeRack => t is Rideable.RideableType.MountainBike or Rideable.RideableType.BMX,
        StableKind.Aquatic  => t == Rideable.RideableType.Dolphin,
        _ => false
    };
}

    class Building
    {
        public Rectangle Bounds;
        public Vector2 EntryPosition;

        public Color ExteriorColor;
        public Color InteriorColor;

        public Vector2 ExitPosition;
        public string BuildingName;

        public NPC InteriorNPC;

        public List<Rectangle> InteriorObjects = new();

        public Building(
            Rectangle bounds,
            Color exterior,
            Color interior,
            Vector2 exitPos,
            string buildingName,
            NPC npc,
            Vector2 entryPos = default
        )
        {
            Bounds = bounds;
            ExteriorColor = exterior;
            InteriorColor = interior;
            ExitPosition = exitPos;
            BuildingName = buildingName;
            InteriorNPC = npc;
            EntryPosition = entryPos == default ? new Vector2(300, 500) : entryPos;

        }

        public void Draw()
{
    Raylib.DrawRectangleRec(Bounds, ExteriorColor);

    // door
    Raylib.DrawRectangle(
        (int)Bounds.X + 50,
        (int)Bounds.Y + 70,
        30,
        50,
        Color.Brown
    );

    // window left
    Raylib.DrawRectangle(
        (int)Bounds.X + 10,
        (int)Bounds.Y + 20,
        30,
        30,
        Color.LightGray
    );

    // window right
    Raylib.DrawRectangle(
        (int)Bounds.X + 100,
        (int)Bounds.Y + 20,
        30,
        30,
        Color.LightGray
    );

    float glow = Program.GetDarkness();
    if (glow > 0.4f)
    {
        byte ga = (byte)(200 * Math.Min(1f, (glow - 0.4f) / 0.4f));
        Color lit  = new Color((byte)255,(byte)210,(byte)110, ga);
        Color halo = new Color((byte)255,(byte)200,(byte)90, (byte)(ga / 5));
        Raylib.DrawRectangle((int)Bounds.X + 10,  (int)Bounds.Y + 20, 30, 30, lit);
        Raylib.DrawRectangle((int)Bounds.X + 100, (int)Bounds.Y + 20, 30, 30, lit);
        Raylib.DrawCircle((int)Bounds.X + 25,  (int)Bounds.Y + 35, 34, halo);
        Raylib.DrawCircle((int)Bounds.X + 115, (int)Bounds.Y + 35, 34, halo);
    }
}
    }
    class DecorativeBuilding
{
    public Rectangle Bounds;
    public string BuildingName;

    public DecorativeBuilding(Rectangle bounds, string buildingName)
    {
        Bounds = bounds;
        BuildingName = buildingName;
    }

    public void Draw()
    {
        float bx = Bounds.X;
        float by = Bounds.Y;

        // ── COTTAGE ───────────────────────────────────────────────────────
        if (BuildingName == "Cottage")
        {
            Raylib.DrawRectangle((int)bx, (int)by, 120, 90, new Color((byte)242,(byte)232,(byte)200,(byte)255));
            Raylib.DrawRectangle((int)bx + 118, (int)by + 4, 6, 86, new Color((byte)200,(byte)184,(byte)122,(byte)128));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 136, 16, new Color((byte)122,(byte)140,(byte)154,(byte)255));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 136, 5, new Color((byte)154,(byte)174,(byte)187,(byte)255));
            Raylib.DrawRectangle((int)bx + 88, (int)by - 28, 14, 20, new Color((byte)176,(byte)112,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 86, (int)by - 30, 18, 5, new Color((byte)152,(byte)90,(byte)56,(byte)255));
            Raylib.DrawRectangle((int)bx + 10, (int)by + 15, 30, 28, new Color((byte)168,(byte)212,(byte)224,(byte)192));
            Raylib.DrawRectangleLines((int)bx + 10, (int)by + 15, 30, 28, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawLine((int)bx + 25, (int)by + 15, (int)bx + 25, (int)by + 43, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawLine((int)bx + 10, (int)by + 29, (int)bx + 40, (int)by + 29, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 10, (int)by + 43, 30, 7, new Color((byte)107,(byte)58,(byte)31,(byte)255));
            Raylib.DrawCircle((int)bx + 17, (int)by + 45, 3, new Color((byte)232,(byte)64,(byte)64,(byte)255));
            Raylib.DrawCircle((int)bx + 25, (int)by + 44, 3, new Color((byte)245,(byte)200,(byte)66,(byte)255));
            Raylib.DrawCircle((int)bx + 33, (int)by + 45, 3, new Color((byte)232,(byte)64,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 80, (int)by + 15, 30, 28, new Color((byte)168,(byte)212,(byte)224,(byte)192));
            Raylib.DrawRectangleLines((int)bx + 80, (int)by + 15, 30, 28, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawLine((int)bx + 95, (int)by + 15, (int)bx + 95, (int)by + 43, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawLine((int)bx + 80, (int)by + 29, (int)bx + 110, (int)by + 29, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 80, (int)by + 43, 30, 7, new Color((byte)107,(byte)58,(byte)31,(byte)255));
            Raylib.DrawCircle((int)bx + 87, (int)by + 45, 3, new Color((byte)245,(byte)200,(byte)66,(byte)255));
            Raylib.DrawCircle((int)bx + 95, (int)by + 44, 3, new Color((byte)232,(byte)64,(byte)64,(byte)255));
            Raylib.DrawCircle((int)bx + 103, (int)by + 45, 3, new Color((byte)245,(byte)200,(byte)66,(byte)255));
            Raylib.DrawRectangle((int)bx + 42, (int)by + 54, 36, 36, new Color((byte)139,(byte)82,(byte)48,(byte)255));
            Raylib.DrawRectangle((int)bx + 46, (int)by + 58, 28, 14, new Color((byte)160,(byte)100,(byte)53,(byte)255));
            Raylib.DrawRectangle((int)bx + 46, (int)by + 76, 28, 11, new Color((byte)160,(byte)100,(byte)53,(byte)255));
            Raylib.DrawCircle((int)bx + 71, (int)by + 75, 3, new Color((byte)200,(byte)160,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 52, (int)by + 90, 10, 8, new Color((byte)204,(byte)191,(byte)154,(byte)255));
            Raylib.DrawRectangle((int)bx + 65, (int)by + 90, 10, 8, new Color((byte)204,(byte)191,(byte)154,(byte)255));
            Raylib.DrawRectangle((int)bx + 15, (int)by - 8, 90, 8, new Color((byte)139,(byte)82,(byte)48,(byte)220));
        }

        // ── TOWNHOUSE ─────────────────────────────────────────────────────
        if (BuildingName == "Townhouse")
        {
            Raylib.DrawRectangle((int)bx, (int)by, 140, 190, new Color((byte)212,(byte)144,(byte)106,(byte)255));
            Raylib.DrawRectangle((int)bx, (int)by + 90, 140, 4, new Color((byte)184,(byte)112,(byte)80,(byte)160));
            Raylib.DrawRectangle((int)bx + 136, (int)by + 4, 6, 186, new Color((byte)154,(byte)90,(byte)48,(byte)100));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 156, 16, new Color((byte)74,(byte)56,(byte)40,(byte)255));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 156, 5, new Color((byte)106,(byte)80,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 110, (int)by - 30, 16, 22, new Color((byte)176,(byte)112,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 108, (int)by - 32, 20, 5, new Color((byte)152,(byte)88,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 4, (int)by + 10, 6, 30, new Color((byte)90,(byte)58,(byte)32,(byte)128));
            Raylib.DrawRectangle((int)bx + 10, (int)by + 10, 35, 30, new Color((byte)184,(byte)220,(byte)232,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 10, (int)by + 10, 35, 30, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 27, (int)by + 10, (int)bx + 27, (int)by + 40, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 10, (int)by + 25, (int)bx + 45, (int)by + 25, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 45, (int)by + 10, 6, 30, new Color((byte)90,(byte)58,(byte)32,(byte)128));
            Raylib.DrawRectangle((int)bx + 75, (int)by + 10, 6, 30, new Color((byte)90,(byte)58,(byte)32,(byte)128));
            Raylib.DrawRectangle((int)bx + 81, (int)by + 10, 35, 30, new Color((byte)184,(byte)220,(byte)232,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 81, (int)by + 10, 35, 30, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 98, (int)by + 10, (int)bx + 98, (int)by + 40, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 81, (int)by + 25, (int)bx + 116, (int)by + 25, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 116, (int)by + 10, 6, 30, new Color((byte)90,(byte)58,(byte)32,(byte)128));
            Raylib.DrawRectangle((int)bx + 10, (int)by + 102, 35, 30, new Color((byte)184,(byte)220,(byte)232,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 10, (int)by + 102, 35, 30, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 27, (int)by + 102, (int)bx + 27, (int)by + 132, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 10, (int)by + 117, (int)bx + 45, (int)by + 117, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 81, (int)by + 102, 35, 30, new Color((byte)184,(byte)220,(byte)232,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 81, (int)by + 102, 35, 30, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 98, (int)by + 102, (int)bx + 98, (int)by + 132, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawLine((int)bx + 81, (int)by + 117, (int)bx + 116, (int)by + 117, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 42, (int)by + 152, 36, 38, new Color((byte)90,(byte)48,(byte)16,(byte)255));
            Raylib.DrawRectangle((int)bx + 46, (int)by + 156, 28, 16, new Color((byte)110,(byte)64,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 46, (int)by + 176, 28, 12, new Color((byte)110,(byte)64,(byte)32,(byte)255));
            Raylib.DrawCircle((int)bx + 72, (int)by + 175, 3, new Color((byte)200,(byte)160,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 38, (int)by + 188, 44, 5, new Color((byte)176,(byte)144,(byte)112,(byte)255));
            Raylib.DrawRectangle((int)bx + 8, (int)by + 168, 20, 10, new Color((byte)138,(byte)112,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 10, (int)by + 173, 16, 3, new Color((byte)90,(byte)58,(byte)32,(byte)255));
            Raylib.DrawRectangle((int)bx + 52, (int)by + 193, 8, 7, new Color((byte)200,(byte)184,(byte)144,(byte)255));
            Raylib.DrawRectangle((int)bx + 63, (int)by + 193, 8, 7, new Color((byte)200,(byte)184,(byte)144,(byte)255));
            Raylib.DrawRectangle((int)bx + 15, (int)by - 8, 110, 8, new Color((byte)74,(byte)56,(byte)40,(byte)220));
        }

        // ── APARTMENT BLOCK ───────────────────────────────────────────────
        if (BuildingName == "Apartment Block")
        {
            Raylib.DrawRectangle((int)bx, (int)by, 200, 210, new Color((byte)216,(byte)212,(byte)204,(byte)255));
            Raylib.DrawRectangle((int)bx, (int)by + 70, 200, 3, new Color((byte)184,(byte)180,(byte)170,(byte)178));
            Raylib.DrawRectangle((int)bx, (int)by + 135, 200, 3, new Color((byte)184,(byte)180,(byte)170,(byte)178));
            Raylib.DrawRectangle((int)bx + 196, (int)by + 4, 6, 206, new Color((byte)168,(byte)164,(byte)152,(byte)128));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 216, 16, new Color((byte)74,(byte)74,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx - 8, (int)by - 14, 216, 5, new Color((byte)106,(byte)106,(byte)114,(byte)255));
            for (int w = 0; w < 3; w++)
            {
                Raylib.DrawRectangle((int)bx + 12 + w * 64, (int)by + 8, 34, 28, new Color((byte)160,(byte)200,(byte)216,(byte)204));
                Raylib.DrawRectangleLines((int)bx + 12 + w * 64, (int)by + 8, 34, 28, new Color((byte)120,(byte)120,(byte)120,(byte)255));
                Raylib.DrawLine((int)bx + 29 + w * 64, (int)by + 8, (int)bx + 29 + w * 64, (int)by + 36, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            }
            for (int w = 0; w < 3; w++)
            {
                Raylib.DrawRectangle((int)bx + 12 + w * 64, (int)by + 78, 34, 28, new Color((byte)160,(byte)200,(byte)216,(byte)204));
                Raylib.DrawRectangleLines((int)bx + 12 + w * 64, (int)by + 78, 34, 28, new Color((byte)120,(byte)120,(byte)120,(byte)255));
                Raylib.DrawLine((int)bx + 29 + w * 64, (int)by + 78, (int)bx + 29 + w * 64, (int)by + 106, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            }
            Raylib.DrawRectangle((int)bx + 12, (int)by + 143, 34, 28, new Color((byte)160,(byte)200,(byte)216,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 12, (int)by + 143, 34, 28, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            Raylib.DrawLine((int)bx + 29, (int)by + 143, (int)bx + 29, (int)by + 171, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            Raylib.DrawRectangle((int)bx + 154, (int)by + 143, 34, 28, new Color((byte)160,(byte)200,(byte)216,(byte)204));
            Raylib.DrawRectangleLines((int)bx + 154, (int)by + 143, 34, 28, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            Raylib.DrawLine((int)bx + 171, (int)by + 143, (int)bx + 171, (int)by + 171, new Color((byte)120,(byte)120,(byte)120,(byte)255));
            Raylib.DrawRectangle((int)bx + 62, (int)by + 132, 76, 8, new Color((byte)74,(byte)74,(byte)80,(byte)255));
            Raylib.DrawRectangle((int)bx + 70, (int)by + 140, 60, 70, new Color((byte)136,(byte)184,(byte)204,(byte)178));
            Raylib.DrawRectangleLines((int)bx + 70, (int)by + 140, 30, 70, new Color((byte)80,(byte)88,(byte)96,(byte)255));
            Raylib.DrawRectangleLines((int)bx + 100, (int)by + 140, 30, 70, new Color((byte)80,(byte)88,(byte)96,(byte)255));
            Raylib.DrawCircle((int)bx + 99, (int)by + 177, 2, new Color((byte)160,(byte)144,(byte)96,(byte)255));
            Raylib.DrawCircle((int)bx + 102, (int)by + 177, 2, new Color((byte)160,(byte)144,(byte)96,(byte)255));
            Raylib.DrawRectangle((int)bx + 70, (int)by + 140, 5, 70, new Color((byte)192,(byte)188,(byte)184,(byte)255));
            Raylib.DrawRectangle((int)bx + 125, (int)by + 140, 5, 70, new Color((byte)192,(byte)188,(byte)184,(byte)255));
            Raylib.DrawRectangle((int)bx + 64, (int)by + 208, 72, 4, new Color((byte)184,(byte)176,(byte)160,(byte)255));
            Raylib.DrawRectangle((int)bx + 68, (int)by + 212, 64, 4, new Color((byte)172,(byte)164,(byte)150,(byte)255));
            Raylib.DrawRectangle((int)bx + 8, (int)by + 160, 44, 24, new Color((byte)144,(byte)144,(byte)144,(byte)255));
            Raylib.DrawRectangle((int)bx + 11, (int)by + 163, 18, 9, new Color((byte)112,(byte)112,(byte)112,(byte)255));
            Raylib.DrawRectangle((int)bx + 31, (int)by + 163, 18, 9, new Color((byte)112,(byte)112,(byte)112,(byte)255));
            Raylib.DrawRectangle((int)bx + 11, (int)by + 174, 18, 7, new Color((byte)112,(byte)112,(byte)112,(byte)255));
            Raylib.DrawRectangle((int)bx + 31, (int)by + 174, 18, 7, new Color((byte)112,(byte)112,(byte)112,(byte)255));
            Raylib.DrawRectangle((int)bx + 15, (int)by - 8, 170, 8, new Color((byte)58,(byte)58,(byte)66,(byte)230));
        }
        // ── COTTAGE WITH TRIANGULAR ROOF ──────────────────────────────────────────
if (BuildingName == "Cottage Peaked")
{
    // walls
    Raylib.DrawRectangle((int)bx, (int)by + 58, 120, 85, new Color((byte)200,(byte)212,(byte)224,(byte)255));
    // side shadow
    Raylib.DrawRectangle((int)bx + 116, (int)by + 62, 5, 81, new Color((byte)136,(byte)152,(byte)168,(byte)100));
    // triangular roof
    int[] rx = { (int)bx - 8, (int)bx + 60, (int)bx + 128 };
    int[] ry = { (int)by + 58, (int)by, (int)by + 58 };
    Raylib.DrawTriangle(
    new Vector2((int)bx + 128, (int)by + 58),  // bottom right
    new Vector2((int)bx + 60, (int)by),          // top centre
    new Vector2((int)bx - 8, (int)by + 58),      // bottom left
    new Color((byte)74,(byte)90,(byte)106,(byte)255));
    Raylib.DrawTriangleLines(
    new Vector2((int)bx + 128, (int)by + 58),
    new Vector2((int)bx + 60, (int)by),
    new Vector2((int)bx - 8, (int)by + 58),
    new Color((byte)90,(byte)110,(byte)128,(byte)255));
    // chimney (draw after roof so it overlaps correctly)
    Raylib.DrawRectangle((int)bx + 82, (int)by + 14, 14, 26, new Color((byte)138,(byte)112,(byte)96,(byte)255));
    Raylib.DrawRectangle((int)bx + 80, (int)by + 12, 18, 5, new Color((byte)122,(byte)94,(byte)74,(byte)255));
    // left window
    Raylib.DrawRectangle((int)bx + 10, (int)by + 70, 32, 28, new Color((byte)168,(byte)208,(byte)224,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 70, 32, 28, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    Raylib.DrawLine((int)bx + 26, (int)by + 70, (int)bx + 26, (int)by + 98, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    Raylib.DrawLine((int)bx + 10, (int)by + 84, (int)bx + 42, (int)by + 84, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    // right window
    Raylib.DrawRectangle((int)bx + 78, (int)by + 70, 32, 28, new Color((byte)168,(byte)208,(byte)224,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 78, (int)by + 70, 32, 28, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    Raylib.DrawLine((int)bx + 94, (int)by + 70, (int)bx + 94, (int)by + 98, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    Raylib.DrawLine((int)bx + 78, (int)by + 84, (int)bx + 110, (int)by + 84, new Color((byte)106,(byte)120,(byte)136,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 44, (int)by + 106, 32, 37, new Color((byte)90,(byte)64,(byte)48,(byte)255));
    Raylib.DrawRectangle((int)bx + 48, (int)by + 110, 24, 14, new Color((byte)108,(byte)80,(byte)56,(byte)255));
    Raylib.DrawRectangle((int)bx + 48, (int)by + 128, 24, 12, new Color((byte)108,(byte)80,(byte)56,(byte)255));
    Raylib.DrawCircle((int)bx + 69, (int)by + 127, 3, new Color((byte)176,(byte)144,(byte)64,(byte)255));
    // path
    Raylib.DrawRectangle((int)bx + 52, (int)by + 143, 9, 7, new Color((byte)184,(byte)174,(byte)144,(byte)255));
    Raylib.DrawRectangle((int)bx + 64, (int)by + 143, 9, 7, new Color((byte)184,(byte)174,(byte)144,(byte)255));
    // sign strip
    Raylib.DrawRectangle((int)bx + 10, (int)by + 54, 100, 7, new Color((byte)74,(byte)90,(byte)106,(byte)220));
}

// ── TERRACED HOUSE (BRICK, TRIANGULAR ROOF) ───────────────────────────────
if (BuildingName == "Terraced House")
{
    // walls - red brick
    Raylib.DrawRectangle((int)bx, (int)by + 58, 130, 155, new Color((byte)192,(byte)120,(byte)88,(byte)255));
    // brick row lines
    for (int row = 0; row < 8; row++)
        Raylib.DrawRectangle((int)bx, (int)by + 76 + row * 20, 130, 2, new Color((byte)160,(byte)96,(byte)64,(byte)128));
    // side shadow
    Raylib.DrawRectangle((int)bx + 126, (int)by + 62, 5, 151, new Color((byte)128,(byte)64,(byte)40,(byte)80));
    // triangular roof
    Raylib.DrawTriangle(
    new Vector2((int)bx + 128, (int)by + 58),  // bottom right
    new Vector2((int)bx + 60, (int)by),          // top centre
    new Vector2((int)bx - 8, (int)by + 58),      // bottom left
    new Color((byte)74,(byte)90,(byte)106,(byte)255));
    Raylib.DrawTriangleLines(
    new Vector2((int)bx + 128, (int)by + 58),
    new Vector2((int)bx + 60, (int)by),
    new Vector2((int)bx - 8, (int)by + 58),
    new Color((byte)90,(byte)110,(byte)128,(byte)255));
    // chimney
    Raylib.DrawRectangle((int)bx + 96, (int)by + 14, 14, 28, new Color((byte)144,(byte)96,(byte)80,(byte)255));
    Raylib.DrawRectangle((int)bx + 94, (int)by + 12, 18, 5, new Color((byte)122,(byte)78,(byte)58,(byte)255));
    // 2nd floor windows
    Raylib.DrawRectangle((int)bx + 10, (int)by + 70, 34, 30, new Color((byte)176,(byte)208,(byte)220,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 70, 34, 30, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 27, (int)by + 70, (int)bx + 27, (int)by + 100, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 10, (int)by + 85, (int)bx + 44, (int)by + 85, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawRectangle((int)bx + 86, (int)by + 70, 34, 30, new Color((byte)176,(byte)208,(byte)220,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 86, (int)by + 70, 34, 30, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 103, (int)by + 70, (int)bx + 103, (int)by + 100, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 86, (int)by + 85, (int)bx + 120, (int)by + 85, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    // 1st floor windows
    Raylib.DrawRectangle((int)bx + 10, (int)by + 143, 34, 30, new Color((byte)176,(byte)208,(byte)220,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 143, 34, 30, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 27, (int)by + 143, (int)bx + 27, (int)by + 173, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 10, (int)by + 158, (int)bx + 44, (int)by + 158, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawRectangle((int)bx + 86, (int)by + 143, 34, 30, new Color((byte)176,(byte)208,(byte)220,(byte)204));
    Raylib.DrawRectangleLines((int)bx + 86, (int)by + 143, 34, 30, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 103, (int)by + 143, (int)bx + 103, (int)by + 173, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 86, (int)by + 158, (int)bx + 120, (int)by + 158, new Color((byte)128,(byte)80,(byte)64,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 48, (int)by + 176, 34, 37, new Color((byte)42,(byte)48,(byte)64,(byte)255));
    Raylib.DrawRectangle((int)bx + 52, (int)by + 180, 26, 14, new Color((byte)56,(byte)64,(byte)78,(byte)255));
    Raylib.DrawRectangle((int)bx + 52, (int)by + 198, 26, 12, new Color((byte)56,(byte)64,(byte)78,(byte)255));
    Raylib.DrawCircle((int)bx + 74, (int)by + 197, 3, new Color((byte)176,(byte)144,(byte)64,(byte)255));
    // step
    Raylib.DrawRectangle((int)bx + 44, (int)by + 211, 42, 5, new Color((byte)160,(byte)144,(byte)112,(byte)255));
    // path
    Raylib.DrawRectangle((int)bx + 56, (int)by + 216, 9, 7, new Color((byte)184,(byte)174,(byte)144,(byte)255));
    Raylib.DrawRectangle((int)bx + 68, (int)by + 216, 9, 7, new Color((byte)184,(byte)174,(byte)144,(byte)255));
    // sign
    Raylib.DrawRectangle((int)bx + 10, (int)by + 54, 110, 7, new Color((byte)58,(byte)40,(byte)24,(byte)220));
}
    }
}
class DecorativeAsset
{
    public Rectangle Bounds;
    public string AssetName;

    public DecorativeAsset(Rectangle bounds, string assetName)
    {
        Bounds = bounds;
        AssetName = assetName;
    }

    public void Draw()
    {
        float bx = Bounds.X;
        float by = Bounds.Y;

        // ── PARK BENCH ────────────────────────────────────────────────────
        if (AssetName == "Bench")
        {
            Raylib.DrawRectangle((int)bx, (int)by + 12, 70, 8, new Color((byte)139,(byte)94,(byte)48,(byte)255));   // seat
            Raylib.DrawRectangle((int)bx, (int)by, 70, 7, new Color((byte)139,(byte)94,(byte)48,(byte)255));         // backrest
            Raylib.DrawRectangle((int)bx, (int)by + 2, 5, 18, new Color((byte)106,(byte)68,(byte)32,(byte)255));     // left armrest
            Raylib.DrawRectangle((int)bx + 65, (int)by + 2, 5, 18, new Color((byte)106,(byte)68,(byte)32,(byte)255)); // right armrest
            Raylib.DrawRectangle((int)bx + 4, (int)by + 20, 6, 14, new Color((byte)106,(byte)68,(byte)32,(byte)255)); // left leg
            Raylib.DrawRectangle((int)bx + 60, (int)by + 20, 6, 14, new Color((byte)106,(byte)68,(byte)32,(byte)255)); // right leg
        }

        // ── LAMPPOST ──────────────────────────────────────────────────────
        if (AssetName == "Lamppost")
        {
            Raylib.DrawRectangle((int)bx + 7, (int)by + 18, 6, 90, new Color((byte)96,(byte)104,(byte)112,(byte)255)); // pole
            Raylib.DrawRectangle((int)bx, (int)by + 18, 22, 5, new Color((byte)96,(byte)104,(byte)112,(byte)255));     // arm
            Raylib.DrawRectangle((int)bx, (int)by, 20, 20, new Color((byte)232,(byte)216,(byte)112,(byte)255));        // lamp body
            Raylib.DrawRectangle((int)bx + 2, (int)by + 2, 16, 16, new Color((byte)255,(byte)248,(byte)176,(byte)230)); // lamp glow
            Raylib.DrawRectangle((int)bx + 3, (int)by + 105, 14, 6, new Color((byte)80,(byte)88,(byte)96,(byte)255)); // base
        }

        // ── SWING SET ─────────────────────────────────────────────────────
        if (AssetName == "Swing Set")
        {
            // frame
            Raylib.DrawLine((int)bx, (int)by + 80, (int)bx + 20, (int)by + 20, new Color((byte)138,(byte)104,(byte)64,(byte)255));
            Raylib.DrawLine((int)bx + 80, (int)by + 80, (int)bx + 60, (int)by + 20, new Color((byte)138,(byte)104,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 18, (int)by + 14, 44, 8, new Color((byte)160,(byte)120,(byte)64,(byte)255)); // top bar
            // ground anchors
            Raylib.DrawRectangle((int)bx - 5, (int)by + 78, 14, 4, new Color((byte)138,(byte)104,(byte)64,(byte)255));
            Raylib.DrawRectangle((int)bx + 71, (int)by + 78, 14, 4, new Color((byte)138,(byte)104,(byte)64,(byte)255));
            // left swing
            Raylib.DrawLine((int)bx + 27, (int)by + 22, (int)bx + 23, (int)by + 58, new Color((byte)96,(byte)96,(byte)96,(byte)255));
            Raylib.DrawLine((int)bx + 37, (int)by + 22, (int)bx + 33, (int)by + 58, new Color((byte)96,(byte)96,(byte)96,(byte)255));
            Raylib.DrawRectangle((int)bx + 20, (int)by + 58, 16, 5, new Color((byte)192,(byte)64,(byte)64,(byte)255));
            // right swing
            Raylib.DrawLine((int)bx + 43, (int)by + 22, (int)bx + 47, (int)by + 52, new Color((byte)96,(byte)96,(byte)96,(byte)255));
            Raylib.DrawLine((int)bx + 53, (int)by + 22, (int)bx + 57, (int)by + 52, new Color((byte)96,(byte)96,(byte)96,(byte)255));
            Raylib.DrawRectangle((int)bx + 44, (int)by + 52, 16, 5, new Color((byte)64,(byte)96,(byte)192,(byte)255));
        }

        // ── SANDBOX ───────────────────────────────────────────────────────
        if (AssetName == "Sandbox")
        {
            Raylib.DrawRectangle((int)bx, (int)by, 80, 60, new Color((byte)200,(byte)160,(byte)80,(byte)255));      // border
            Raylib.DrawRectangle((int)bx + 4, (int)by + 4, 72, 52, new Color((byte)232,(byte)200,(byte)112,(byte)255)); // sand
            Raylib.DrawRectangle((int)bx + 18, (int)by + 16, 14, 12, new Color((byte)224,(byte)64,(byte)64,(byte)255)); // bucket body
            Raylib.DrawRectangle((int)bx + 16, (int)by + 14, 18, 4, new Color((byte)192,(byte)48,(byte)48,(byte)255)); // bucket rim
            Raylib.DrawRectangle((int)bx + 44, (int)by + 12, 4, 18, new Color((byte)139,(byte)94,(byte)48,(byte)255)); // spade handle
            Raylib.DrawRectangle((int)bx + 41, (int)by + 26, 10, 8, new Color((byte)192,(byte)160,(byte)64,(byte)255)); // spade head
        }

        // ── PICNIC TABLE ──────────────────────────────────────────────────
        if (AssetName == "Picnic Table")
        {
            Raylib.DrawRectangle((int)bx, (int)by + 10, 90, 10, new Color((byte)160,(byte)112,(byte)64,(byte)255));  // tabletop
            Raylib.DrawRectangle((int)bx + 10, (int)by + 20, 5, 16, new Color((byte)122,(byte)80,(byte)48,(byte)255)); // left leg
            Raylib.DrawRectangle((int)bx + 75, (int)by + 20, 5, 16, new Color((byte)122,(byte)80,(byte)48,(byte)255)); // right leg
            Raylib.DrawRectangle((int)bx - 5, (int)by + 30, 36, 7, new Color((byte)139,(byte)96,(byte)48,(byte)255));  // left bench
            Raylib.DrawRectangle((int)bx + 59, (int)by + 30, 36, 7, new Color((byte)139,(byte)96,(byte)48,(byte)255)); // right bench
            Raylib.DrawRectangle((int)bx - 2, (int)by + 37, 4, 10, new Color((byte)106,(byte)72,(byte)32,(byte)255)); // bench leg L1
            Raylib.DrawRectangle((int)bx + 28, (int)by + 37, 4, 10, new Color((byte)106,(byte)72,(byte)32,(byte)255)); // bench leg L2
            Raylib.DrawRectangle((int)bx + 58, (int)by + 37, 4, 10, new Color((byte)106,(byte)72,(byte)32,(byte)255)); // bench leg R1
            Raylib.DrawRectangle((int)bx + 88, (int)by + 37, 4, 10, new Color((byte)106,(byte)72,(byte)32,(byte)255)); // bench leg R2
        }

        // ── POSTBOX ───────────────────────────────────────────────────────
        if (AssetName == "Postbox")
        {
            Raylib.DrawRectangle((int)bx + 9, (int)by + 38, 6, 30, new Color((byte)96,(byte)96,(byte)96,(byte)255));  // post
            Raylib.DrawRectangle((int)bx, (int)by, 24, 30, new Color((byte)208,(byte)48,(byte)48,(byte)255));          // body
            Raylib.DrawCircle((int)bx + 12, (int)by, 12, new Color((byte)176,(byte)32,(byte)32,(byte)255));            // dome top
            Raylib.DrawRectangle((int)bx + 4, (int)by + 16, 16, 4, new Color((byte)144,(byte)24,(byte)24,(byte)255)); // slot
            Raylib.DrawRectangle((int)bx + 3, (int)by + 66, 18, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));  // base plate
        }

        // ── BUS STOP ──────────────────────────────────────────────────────
        if (AssetName == "Bus Stop")
        {
            Raylib.DrawRectangle((int)bx + 28, (int)by + 28, 6, 110, new Color((byte)80,(byte)88,(byte)96,(byte)255)); // pole
            Raylib.DrawRectangle((int)bx + 8, (int)by, 50, 30, new Color((byte)232,(byte)176,(byte)32,(byte)255));     // sign panel
            Raylib.DrawRectangle((int)bx + 12, (int)by + 4, 42, 22, new Color((byte)208,(byte)152,(byte)16,(byte)255)); // sign inner
            Program.DrawTextUI("BUS", (int)bx + 18, (int)by + 7, 10, Color.White);
            Program.DrawTextUI("STOP", (int)bx + 15, (int)by + 18, 9, Color.White);
            Raylib.DrawRectangle((int)bx, (int)by + 46, 76, 8, new Color((byte)74,(byte)80,(byte)90,(byte)255));       // shelter roof
            Raylib.DrawRectangle((int)bx, (int)by + 54, 8, 50, new Color((byte)128,(byte)144,(byte)160,(byte)128));    // shelter back
            Raylib.DrawRectangle((int)bx + 4, (int)by + 78, 64, 6, new Color((byte)106,(byte)120,(byte)136,(byte)255)); // bench
            Raylib.DrawRectangle((int)bx + 8, (int)by + 84, 5, 12, new Color((byte)80,(byte)88,(byte)96,(byte)255));   // bench leg L
            Raylib.DrawRectangle((int)bx + 59, (int)by + 84, 5, 12, new Color((byte)80,(byte)88,(byte)96,(byte)255));  // bench leg R
            Raylib.DrawRectangle((int)bx, (int)by + 96, 76, 4, new Color((byte)80,(byte)88,(byte)96,(byte)255));       // base
        }
        // ── BARN ──────────────────────────────────────────────────────────────────
if (AssetName == "Barn")
{
    // main walls
    Raylib.DrawRectangle((int)bx, (int)by + 100, 200, 130, new Color((byte)192,(byte)48,(byte)32,(byte)255));
    // board lines
    for (int row = 0; row < 6; row++)
        Raylib.DrawRectangle((int)bx, (int)by + 116 + row * 20, 200, 2, new Color((byte)160,(byte)32,(byte)16,(byte)128));
    // side shadow
    Raylib.DrawRectangle((int)bx + 194, (int)by + 104, 6, 126, new Color((byte)128,(byte)16,(byte)8,(byte)100));
    // triangular roof - counter-clockwise winding
    Raylib.DrawTriangle(
        new Vector2((int)bx + 208, (int)by + 100),
        new Vector2((int)bx + 100, (int)by),
        new Vector2((int)bx - 8, (int)by + 100),
        new Color((byte)74,(byte)32,(byte)16,(byte)255));
    Raylib.DrawTriangleLines(
        new Vector2((int)bx + 208, (int)by + 100),
        new Vector2((int)bx + 100, (int)by),
        new Vector2((int)bx - 8, (int)by + 100),
        new Color((byte)112,(byte)46,(byte)26,(byte)255));
    // roof ridge cap
    Raylib.DrawRectangle((int)bx + 96, (int)by, 8, 22, new Color((byte)58,(byte)24,(byte)8,(byte)255));
    // loft window
    Raylib.DrawRectangle((int)bx + 84, (int)by + 38, 32, 28, new Color((byte)26,(byte)16,(byte)8,(byte)178));
    Raylib.DrawLine((int)bx + 100, (int)by + 38, (int)bx + 100, (int)by + 66, new Color((byte)42,(byte)24,(byte)16,(byte)255));
    // left double door with X brace
    Raylib.DrawRectangle((int)bx + 10, (int)by + 160, 50, 70, new Color((byte)106,(byte)48,(byte)16,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 160, 24, 70, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 36, (int)by + 160, 24, 70, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawLine((int)bx + 10, (int)by + 160, (int)bx + 60, (int)by + 230, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawLine((int)bx + 60, (int)by + 160, (int)bx + 10, (int)by + 230, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    // right double door with X brace
    Raylib.DrawRectangle((int)bx + 140, (int)by + 160, 50, 70, new Color((byte)106,(byte)48,(byte)16,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 140, (int)by + 160, 24, 70, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 164, (int)by + 160, 26, 70, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawLine((int)bx + 140, (int)by + 160, (int)bx + 190, (int)by + 230, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawLine((int)bx + 190, (int)by + 160, (int)bx + 140, (int)by + 230, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    // side window
    Raylib.DrawRectangle((int)bx + 94, (int)by + 118, 32, 28, new Color((byte)200,(byte)216,(byte)160,(byte)154));
    Raylib.DrawRectangleLines((int)bx + 94, (int)by + 118, 32, 28, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    Raylib.DrawLine((int)bx + 110, (int)by + 118, (int)bx + 110, (int)by + 146, new Color((byte)74,(byte)32,(byte)8,(byte)255));
    // sign strip
    Raylib.DrawRectangle((int)bx + 40, (int)by + 106, 120, 12, new Color((byte)58,(byte)24,(byte)8,(byte)230));
}

// ── SHED ──────────────────────────────────────────────────────────────────
if (AssetName == "Shed")
{
    // walls - weathered grey wood
    Raylib.DrawRectangle((int)bx, (int)by + 52, 140, 80, new Color((byte)154,(byte)144,(byte)128,(byte)255));
    // board lines
    for (int row = 0; row < 4; row++)
        Raylib.DrawRectangle((int)bx, (int)by + 66 + row * 16, 140, 2, new Color((byte)128,(byte)112,(byte)96,(byte)154));
    // side shadow
    Raylib.DrawRectangle((int)bx + 135, (int)by + 56, 5, 76, new Color((byte)96,(byte)80,(byte)64,(byte)100));
    // triangular roof - counter-clockwise
    Raylib.DrawTriangle(
        new Vector2((int)bx + 148, (int)by + 52),
        new Vector2((int)bx + 70, (int)by),
        new Vector2((int)bx - 8, (int)by + 52),
        new Color((byte)112,(byte)96,(byte)80,(byte)255));
    Raylib.DrawTriangleLines(
        new Vector2((int)bx + 148, (int)by + 52),
        new Vector2((int)bx + 70, (int)by),
        new Vector2((int)bx - 8, (int)by + 52),
        new Color((byte)144,(byte)120,(byte)96,(byte)255));
    // door
    Raylib.DrawRectangle((int)bx + 70, (int)by + 80, 40, 52, new Color((byte)90,(byte)72,(byte)48,(byte)255));
    Raylib.DrawRectangle((int)bx + 74, (int)by + 84, 32, 22, new Color((byte)106,(byte)88,(byte)56,(byte)255));
    Raylib.DrawRectangle((int)bx + 74, (int)by + 110, 32, 18, new Color((byte)106,(byte)88,(byte)56,(byte)255));
    Raylib.DrawCircle((int)bx + 100, (int)by + 109, 3, new Color((byte)160,(byte)136,(byte)64,(byte)255));
    // small window
    Raylib.DrawRectangle((int)bx + 10, (int)by + 62, 28, 22, new Color((byte)168,(byte)200,(byte)144,(byte)154));
    Raylib.DrawRectangleLines((int)bx + 10, (int)by + 62, 28, 22, new Color((byte)96,(byte)80,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 24, (int)by + 62, (int)bx + 24, (int)by + 84, new Color((byte)96,(byte)80,(byte)64,(byte)255));
    // sign
    Raylib.DrawRectangle((int)bx + 20, (int)by + 48, 100, 7, new Color((byte)112,(byte)96,(byte)80,(byte)220));
}

// ── HAY BALE ──────────────────────────────────────────────────────────────
if (AssetName == "Hay Bale")
{
    // main bale body
    Raylib.DrawEllipse((int)bx + 35, (int)by + 25, 35, 25, new Color((byte)212,(byte)168,(byte)64,(byte)255));
    Raylib.DrawEllipseLines((int)bx + 35, (int)by + 25, 35, 25, new Color((byte)176,(byte)136,(byte)32,(byte)255));
    // wrap bands
    Raylib.DrawEllipse((int)bx + 35, (int)by + 25, 10, 25, new Color((byte)212,(byte)168,(byte)64,(byte)255));
    Raylib.DrawEllipseLines((int)bx + 35, (int)by + 25, 10, 25, new Color((byte)176,(byte)136,(byte)32,(byte)255));
    Raylib.DrawEllipseLines((int)bx + 35, (int)by + 25, 22, 25, new Color((byte)176,(byte)136,(byte)32,(byte)200));
    // straw texture
    Raylib.DrawLine((int)bx + 2, (int)by + 19, (int)bx + 68, (int)by + 19, new Color((byte)192,(byte)152,(byte)48,(byte)154));
    Raylib.DrawLine((int)bx + 1, (int)by + 25, (int)bx + 69, (int)by + 25, new Color((byte)192,(byte)152,(byte)48,(byte)154));
    Raylib.DrawLine((int)bx + 2, (int)by + 31, (int)bx + 68, (int)by + 31, new Color((byte)192,(byte)152,(byte)48,(byte)154));
}

// ── TRACTOR ───────────────────────────────────────────────────────────────
if (AssetName == "Tractor")
{
    // body
    Raylib.DrawRectangle((int)bx + 20, (int)by + 30, 100, 50, new Color((byte)42,(byte)96,(byte)32,(byte)255));
    // cab roof
    Raylib.DrawRectangle((int)bx + 60, (int)by + 14, 54, 20, new Color((byte)30,(byte)72,(byte)24,(byte)255));
    // cab windows
    Raylib.DrawRectangle((int)bx + 64, (int)by + 18, 22, 12, new Color((byte)168,(byte)208,(byte)232,(byte)204));
    Raylib.DrawRectangle((int)bx + 88, (int)by + 18, 22, 12, new Color((byte)168,(byte)208,(byte)232,(byte)204));
    // exhaust pipe
    Raylib.DrawRectangle((int)bx + 57, (int)by + 4, 6, 16, new Color((byte)26,(byte)48,(byte)16,(byte)255));
    Raylib.DrawCircle((int)bx + 60, (int)by + 3, 5, new Color((byte)16,(byte)32,(byte)8,(byte)255));
    // front grille
    Raylib.DrawRectangle((int)bx + 20, (int)by + 36, 18, 32, new Color((byte)26,(byte)64,(byte)16,(byte)255));
    for (int g = 0; g < 3; g++)
        Raylib.DrawRectangle((int)bx + 23, (int)by + 40 + g * 8, 12, 4, new Color((byte)14,(byte)40,(byte)8,(byte)204));
    // headlight
    Raylib.DrawCircle((int)bx + 22, (int)by + 34, 4, new Color((byte)240,(byte)224,(byte)96,(byte)255));
    // big rear wheel
    Raylib.DrawCircle((int)bx + 105, (int)by + 82, 26, new Color((byte)26,(byte)26,(byte)26,(byte)255));
    Raylib.DrawCircle((int)bx + 105, (int)by + 82, 20, new Color((byte)42,(byte)42,(byte)42,(byte)255));
    Raylib.DrawCircle((int)bx + 105, (int)by + 82, 8, new Color((byte)58,(byte)58,(byte)58,(byte)255));
    // rear tread marks
    Raylib.DrawLine((int)bx + 105, (int)by + 56, (int)bx + 105, (int)by + 62, new Color((byte)64,(byte)64,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 105, (int)by + 102, (int)bx + 105, (int)by + 108, new Color((byte)64,(byte)64,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 79, (int)by + 82, (int)bx + 85, (int)by + 82, new Color((byte)64,(byte)64,(byte)64,(byte)255));
    Raylib.DrawLine((int)bx + 125, (int)by + 82, (int)bx + 131, (int)by + 82, new Color((byte)64,(byte)64,(byte)64,(byte)255));
    // small front wheel
    Raylib.DrawCircle((int)bx + 33, (int)by + 82, 16, new Color((byte)26,(byte)26,(byte)26,(byte)255));
    Raylib.DrawCircle((int)bx + 33, (int)by + 82, 11, new Color((byte)42,(byte)42,(byte)42,(byte)255));
    Raylib.DrawCircle((int)bx + 33, (int)by + 82, 4, new Color((byte)58,(byte)58,(byte)58,(byte)255));
}

// ── WATER TROUGH ──────────────────────────────────────────────────────────
if (AssetName == "Water Trough")
{
    // trough body
    Raylib.DrawRectangle((int)bx, (int)by, 100, 40, new Color((byte)122,(byte)104,(byte)72,(byte)255));
    // water inside
    Raylib.DrawRectangle((int)bx + 4, (int)by + 4, 92, 32, new Color((byte)72,(byte)136,(byte)184,(byte)204));
    // water shimmer lines
    Raylib.DrawLine((int)bx + 10, (int)by + 12, (int)bx + 40, (int)by + 12, new Color((byte)128,(byte)184,(byte)224,(byte)178));
    Raylib.DrawLine((int)bx + 56, (int)by + 18, (int)bx + 86, (int)by + 18, new Color((byte)128,(byte)184,(byte)224,(byte)178));
    // rim highlight
    Raylib.DrawRectangle((int)bx, (int)by, 100, 5, new Color((byte)154,(byte)136,(byte)96,(byte)255));
    // legs
    Raylib.DrawRectangle((int)bx + 8, (int)by + 38, 8, 14, new Color((byte)90,(byte)72,(byte)48,(byte)255));
    Raylib.DrawRectangle((int)bx + 84, (int)by + 38, 8, 14, new Color((byte)90,(byte)72,(byte)48,(byte)255));
}

// ── WINDMILL ──────────────────────────────────────────────────────────────
if (AssetName == "Windmill")
{
    // tower body (tapered via trapezoid)
    Raylib.DrawRectangle((int)bx + 18, (int)by + 120, 44, 160, new Color((byte)212,(byte)200,(byte)160,(byte)255));
    Raylib.DrawRectangle((int)bx + 22, (int)by + 124, 36, 4, new Color((byte)176,(byte)164,(byte)128,(byte)255)); // taper hint top
    // tower boards
    for (int row = 0; row < 6; row++)
        Raylib.DrawRectangle((int)bx + 18, (int)by + 136 + row * 24, 44, 2, new Color((byte)176,(byte)164,(byte)128,(byte)128));
    // tower door
    Raylib.DrawRectangle((int)bx + 28, (int)by + 244, 24, 36, new Color((byte)138,(byte)120,(byte)80,(byte)255));
    Raylib.DrawCircle((int)bx + 46, (int)by + 263, 3, new Color((byte)160,(byte)136,(byte)64,(byte)255));
    // tower window
    Raylib.DrawRectangle((int)bx + 27, (int)by + 186, 26, 22, new Color((byte)168,(byte)200,(byte)216,(byte)178));
    Raylib.DrawRectangleLines((int)bx + 27, (int)by + 186, 26, 22, new Color((byte)138,(byte)120,(byte)80,(byte)255));
    // cap (triangle roof on tower)
    Raylib.DrawTriangle(
        new Vector2((int)bx + 68, (int)by + 122),
        new Vector2((int)bx + 40, (int)by + 100),
        new Vector2((int)bx + 12, (int)by + 122),
        new Color((byte)138,(byte)104,(byte)64,(byte)255));
    // hub centre
    Raylib.DrawCircle((int)bx + 40, (int)by + 128, 7, new Color((byte)96,(byte)96,(byte)96,(byte)255));
    Raylib.DrawCircle((int)bx + 40, (int)by + 128, 4, new Color((byte)128,(byte)128,(byte)128,(byte)255));
    // blade top
    Raylib.DrawRectangle((int)bx + 37, (int)by + 48, 6, 80, new Color((byte)232,(byte)224,(byte)192,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 37, (int)by + 48, 6, 80, new Color((byte)192,(byte)184,(byte)144,(byte)255));
    // blade right
    Raylib.DrawRectangle((int)bx + 40, (int)by + 125, 80, 6, new Color((byte)232,(byte)224,(byte)192,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 40, (int)by + 125, 80, 6, new Color((byte)192,(byte)184,(byte)144,(byte)255));
    // blade bottom
    Raylib.DrawRectangle((int)bx + 37, (int)by + 128, 6, 80, new Color((byte)232,(byte)224,(byte)192,(byte)255));
    Raylib.DrawRectangleLines((int)bx + 37, (int)by + 128, 6, 80, new Color((byte)192,(byte)184,(byte)144,(byte)255));
    // blade left
    Raylib.DrawRectangle((int)bx - 40, (int)by + 125, 80, 6, new Color((byte)232,(byte)224,(byte)192,(byte)255));
    Raylib.DrawRectangleLines((int)bx - 40, (int)by + 125, 80, 6, new Color((byte)192,(byte)184,(byte)144,(byte)255));
}

// ── WHEELBARROW ───────────────────────────────────────────────────────────
if (AssetName == "Wheelbarrow")
{
    // handles
    Raylib.DrawLine((int)bx + 10, (int)by + 40, (int)bx - 8, (int)by + 68, new Color((byte)106,(byte)72,(byte)32,(byte)255));
    Raylib.DrawLine((int)bx + 60, (int)by + 40, (int)bx + 78, (int)by + 68, new Color((byte)106,(byte)72,(byte)32,(byte)255));
    // handle grips
    Raylib.DrawRectangle((int)bx - 12, (int)by + 66, 10, 5, new Color((byte)74,(byte)48,(byte)16,(byte)255));
    Raylib.DrawRectangle((int)bx + 72, (int)by + 66, 10, 5, new Color((byte)74,(byte)48,(byte)16,(byte)255));
    // tray body
    Raylib.DrawRectangle((int)bx + 4, (int)by, 62, 42, new Color((byte)138,(byte)104,(byte)48,(byte)255));
    // soil/dirt inside
    Raylib.DrawRectangle((int)bx + 8, (int)by + 4, 54, 34, new Color((byte)122,(byte)88,(byte)40,(byte)204));
    // tray front face
    Raylib.DrawRectangle((int)bx + 4, (int)by + 40, 62, 6, new Color((byte)106,(byte)80,(byte)36,(byte)255));
    // wheel support leg
    Raylib.DrawLine((int)bx + 35, (int)by + 46, (int)bx + 35, (int)by + 58, new Color((byte)106,(byte)72,(byte)32,(byte)255));
    // wheel
    Raylib.DrawCircle((int)bx + 35, (int)by + 66, 12, new Color((byte)42,(byte)42,(byte)42,(byte)255));
    Raylib.DrawCircle((int)bx + 35, (int)by + 66, 8, new Color((byte)58,(byte)58,(byte)58,(byte)255));
    Raylib.DrawCircle((int)bx + 35, (int)by + 66, 3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
}

        // ── FOUNTAIN ──────────────────────────────────────────────────────
        if (AssetName == "Fountain")
        {
            Raylib.DrawEllipse((int)bx + 40, (int)by + 56, 40, 16, new Color((byte)120,(byte)152,(byte)184,(byte)178)); // outer basin
            Raylib.DrawEllipseLines((int)bx + 40, (int)by + 56, 40, 16, new Color((byte)88,(byte)120,(byte)160,(byte)255));
            Raylib.DrawEllipse((int)bx + 40, (int)by + 56, 33, 12, new Color((byte)144,(byte)192,(byte)224,(byte)154)); // water
            Raylib.DrawRectangle((int)bx + 36, (int)by + 24, 8, 34, new Color((byte)152,(byte)152,(byte)168,(byte)255)); // centre pillar
            Raylib.DrawEllipse((int)bx + 40, (int)by + 24, 16, 6, new Color((byte)136,(byte)152,(byte)184,(byte)255));  // top bowl
            Raylib.DrawEllipseLines((int)bx + 40, (int)by + 24, 16, 6, new Color((byte)104,(byte)120,(byte)160,(byte)255));
            Raylib.DrawLine((int)bx + 40, (int)by + 18, (int)bx + 40, (int)by + 6, new Color((byte)144,(byte)200,(byte)240,(byte)255)); // spout
            Raylib.DrawCircle((int)bx + 40, (int)by + 5, 5, new Color((byte)176,(byte)224,(byte)248,(byte)204));        // water drop
        }
    }
}
}
