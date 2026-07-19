using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
     class Player
    {
        public Vector2 Position;
        public PlayerCharacter Character = new PlayerCharacter();
        public enum FacingDirection { Down, Up, Left, Right }
        public FacingDirection Facing = FacingDirection.Down;
        float walkTimer = 0f;
        float bobOffset = 0f;
        public string HeldItemOverride = null;
        public bool   UseHeldItemOverride = false;
        public bool IsSwimming = false;
        public float SwimDepthRatio = 0f;
        public float SwimStrokeTimer = 0f;

        string GetDisplayedItem()
        {
            if (UseHeldItemOverride)
                return string.IsNullOrEmpty(HeldItemOverride) ? null : HeldItemOverride;
            return Program.GetActiveItem();
        }
        public bool isSwinging = false;
        public float swingTimer = 0f;
        public float swingDuration = 0.55f;   // how long one swing takes
        bool walkFrame = false; // alternates legs
        bool isMoving = false;
        float chopAnimAngle = 0f;
        bool isChopping = false;
        bool isMining = false;
        public static bool isFishing = false;
        public int ArcaneEssence = 0;
        public string EquippedStaff = null;

        //CARDS
        public int PlayingCardsXP;

        public int PlayingCardsLevel = 1;
        public int EuchreRating = 200;
        public int FiveHundredRating = 200;
        public int SequenceRating = 200;
        public int EuchreWins = 0;
        public int FiveHundredWins = 0;
        public int SequenceWins = 0;
        static string EuchreRank(int r) =>
            r >= 1800 ? "Grand Master" : r >= 1500 ? "Master" : r >= 1200 ? "Expert" :
            r >= 1000 ? "Intermediate" : "Beginner";

        static string CardsRank(int r) =>
            r >= 1800 ? "Grand Master" : r >= 1500 ? "Master" : r >= 1200 ? "Expert" :
            r >= 1000 ? "Intermediate" : "Beginner";
        public Dictionary<string, int> FishCaught = new Dictionary<string, int>();
        public void TriggerSwing()
        {
            isSwinging = true;
            swingTimer = 0f;
        }
        public void TickSwing(float dt)
        {
            if (isSwinging)
            {
                swingTimer += dt;
                if (swingTimer >= swingDuration)
                {
                    isSwinging = false;
                    swingTimer = 0f;
                }
            }
        }
        float GetSwingAngle()
        {
            if (!isSwinging) return 0f;
            float t = swingTimer / swingDuration;        // 0..1
            // ease out and back: sin curve peaks at the middle
            float arc = MathF.Sin(t * MathF.PI);          // 0 → 1 → 0
            return arc * 90f;                             // up to 90 degrees
        }
        
        public void TrackFishSpecies(string species, int count = 1)
        {
            if (!FishCaught.ContainsKey(species)) FishCaught[species] = 0;
            FishCaught[species] += count;
        }

        public bool Hidden = false;
        public bool HasAxe = false;
        public int Arrows = 0;
        public int Bolts = 0;
        public bool HasBow = false;
        public bool HasCrossbow = false;
        public int MathsRating = 200;
        public int WoodcuttingLevel = 1;
        public int FishingLevel = 1;

        public int WoodcuttingXP = 0;
        public int FishingXP = 0;
        public int MiningLevel = 1;
        public int MiningXP = 0;
        public int CraftingLevel = 1;
        public int CraftingXP = 0;
        public int BlacksmithLevel = 1;
        public int BlacksmithXP = 0;
        public int EnchantingLevel = 1;
        public int EnchantingXP = 0;
        public float Food = 400f;
        public float Thirst = 400f;
        public float Stamina = 100f;
        public int StoneOre = 0;
        public int CopperOre = 0;
        public int IronOre = 0;
        public int GoldOre = 0;
        public int Crystals = 0;

        public int Logs = 0;
        public int BirchLogs = 0;
        public int OakLogs = 0;
        public int PineLogs = 0;
        public int ArcticLogs = 0;
        public int DeadWood = 0;
        public int Fish = 0;
        public int Money = 0;
        public int BankBalance = 0;
        public int Bones = 0;
        public int Fur = 0;
        public int Stingers = 0;
        public int BearPelts = 0;
        public int DogFangs = 0;
        public int WolfClaws = 0;
        public int VenomSacs = 0;
        public int CrabClaws = 0;
        public int BearClaws = 0;
        public int CrabShells = 0;
        public int SharkFins = 0;
        public int SharkTeeth = 0;
        public int SnakeSkins = 0;
        public int SnakeFangs = 0;
        public int CrocScales = 0;
        public int CrocTeeth = 0;
        public int LizardScales = 0;
        public int EmberStones = 0;
        public int MagmaShards = 0;
        public int LavaCores = 0;
        public int Feathers = 0;
        public int EagleTalons = 0;
        public int Horns = 0;
        public int GoatHooves = 0;
        public int Health = 100;
        public int MaxHealth = 100;
        public int CombatLevel = 1;
        public int CombatXP = 0;
        public int OneHandMeleeLevel = 1;
        public int OneHandMeleeXP = 0;
        public int TwoHandMeleeLevel = 1;
        public int TwoHandMeleeXP = 0;
        public int DrivingLevel = 1;
        public int DrivingXP = 0;
        public int AthleticsLevel = 1;
        public int AthleticsXP = 0;
        public int CyclingLevel = 1;
        public int CyclingXP = 0;
        public int RidingLevel = 1;
        public int RidingXP = 0;
        public int DefenceLevel = 1;
        public int DefenceXP = 0;
        public int HitpointsLevel = 1;
        public int HitpointsXP = 0;
        public int StaminaLevel = 1;
        public int StaminaXP = 0;
        public int SwimmingLevel = 1;
        public int SwimmingXP = 0;
        public int DivingLevel = 1;
        public int DivingXP = 0;
        public int RangedLevel = 1;
        public int RangedXP = 0;
        public int FarmingLevel = 1;
        public int FarmingXP = 0;
        public int FaithLevel = 1;    public int FaithXP = 0;
        public int MysticalLevel = 1; public int MysticalXP = 0;
        public int DarkArtsLevel = 1; public int DarkArtsXP = 0;
        public void AddFaithXP(int xp)    => AddNewSkillXP(ref FaithXP, ref FaithLevel, xp, "Faith");
        public void AddMysticalXP(int xp) => AddNewSkillXP(ref MysticalXP, ref MysticalLevel, xp, "Mystical");
        public void AddDarkArtsXP(int xp) => AddNewSkillXP(ref DarkArtsXP, ref DarkArtsLevel, xp, "Dark Arts");
        void AddNewSkillXP(ref int xpField, ref int lvField, int xp, string name)
        {
            if (lvField >= 100) return;
            xpField += (int)(xp * Program.SynergyXPMultiplier());
            int required = lvField * lvField * 50;
            if (xpField >= required) { xpField = 0; lvField++; Program.ShowLevelUp(name, lvField); }
        }
        public int SportsLevel = 1;
        public int SportsXP = 0;
        public int SpiritualLevel = 1;
        public int SpiritualXP = 0;
        public int CookingLevel = 1;
        public int CookingXP = 0;
        public int ElementalLevel = 1;
        public int ElementalXP = 0;
        public int AlchemistLevel = 1;
        public int AlchemistXP = 0;
        public int EnchantmentLevel = 1;
        public int EnchanmenttXP = 0; 
        public int StrengthLevel = 1;
        public int StrengthXP = 0;
        public int GamblingLevel = 1;
        public int GamblingXP = 0;
        public int EducationLevel = 1;
        public int EducationXP = 0;
        public int BoatingLevel = 1;
        public int BoatingXP = 0;

        // ── REPUTATION / TOWN STANDING ──
        public int Reputation = 0;
        public int PlushPrizes = 0;
        public int Tickets = 0;
        public bool HasTrolley = false;
        public bool HasBasket = false;
        public float BaseSpeed => (170 + (AthleticsLevel * 2)) * (Program.HasSynergy("Trailblazer") ? 1.1f : 1f);
        float speed => Program.cheatSpeedBoost ? 2000f : (HasTrolley ? BaseSpeed * 0.65f : BaseSpeed);
        float regenTimer = 0f;
        float damageCooldown = 0f;
        public Color ShirtColor = Color.Blue;
        public Color SkinColor = Color.Beige;
        public Color PantsColor = Color.Black;
        public bool InventoryOpen = false;
        public int DrunkLevel = 0;
        public float DrunkTimer = 0f;
        public float DrunkSpeedMultiplier => DrunkLevel == 0 ? 1f : Math.Max(0.3f, 1f - (DrunkLevel * 0.15f));
        public List<string> OwnedGear = new List<string>();
        Color ArmorColor(string item, Color fallback)
        {
            if (item == null) return fallback;
            
            if (item == "Hawkeye's Cloak" || item == "Marksman Hood")
            return new Color((byte)120,(byte)80,(byte)40,(byte)255);

            if (item.Contains("Mage ") || item.Contains("Ranger "))
            {
                string[] tiers = item.Contains("Mage ") ? Program.mageTiers : Program.rangerTiers;
                foreach (string t in tiers)
                    if (item.StartsWith(t + " ")) return Program.MaterialColor(t);
                return Program.MaterialColor(item.Contains("Mage ") ? "Mage" : "Ranger");  
            }
            foreach (string mat in Program.armorMaterials)
                if (item.Contains(mat)) return Program.MaterialColor(mat);
            return fallback;
        }
        Color WeaponColor(string item, Color fallback)
        {
            if (item == null) return fallback;
            if (item.Contains("Mage ") || item.Contains("Ranger "))
            {
                string[] tiers = item.Contains("Mage ") ? Program.mageTiers : Program.rangerTiers;
                foreach (string t in tiers)
                    if (item.StartsWith(t + " ")) return Program.MaterialColor(t);
                return Program.MaterialColor(item.Contains("Mage ") ? "Mage" : "Ranger"); 
            }
            foreach (string mat in Program.armorMaterials)
                if (item.Contains(mat)) return Program.MaterialColor(mat);
            return fallback;
        }
        public bool OwnsGear(string item) => OwnedGear.Contains(item);
        public void AddGear(string item)
        {
            if (!OwnedGear.Contains(item))   // own one copy is enough to equip freely
                OwnedGear.Add(item);
        }
        List<(Vector2 pos, float timer, Color color)> dustParticles = new();
        public Color BootsColor => Program.armorBoots != null
        ? new Color((byte)120, (byte)80, (byte)30, (byte)255)  // leather brown default
        : new Color((byte)0, (byte)0, (byte)0, (byte)0);       // transparent = no boots
        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Vector2 Center => new Vector2(Position.X + 20, Position.Y + 30);

        public Player(Vector2 position)
        {
            Position = position;
        }
    
    void DrawHorn(float baseX, float baseY, int dir, Color col)
{
    // dir = -1 for left horn, +1 for right horn
    // build a curve of points: out, up, then curl inward
    int segments = 6;
    Vector2[] pts = new Vector2[segments + 1];
    for (int i = 0; i <= segments; i++)
    {
        float t = i / (float)segments;          // 0..1 along the horn
        // x: sweep out then curl back in (parabola)
        float outward = MathF.Sin(t * MathF.PI) * 10f;     // peaks mid-horn
        float curlIn  = t * t * 6f;                         // pulls back in near tip
        float px = baseX + dir * (outward - curlIn);
        // y: rise upward, faster near the tip
        float py = baseY - t * 22f;
        pts[i] = new Vector2(px, py);
    }

    // draw the horn as quads between consecutive points, tapering width
    for (int i = 0; i < segments; i++)
    {
        float w1 = 5f * (1f - i / (float)segments) + 1f;     // width tapers toward tip
        float w2 = 5f * (1f - (i + 1) / (float)segments) + 1f;

        Vector2 a = pts[i];
        Vector2 b = pts[i + 1];

        // perpendicular offset for width
        Vector2 dirVec = Vector2.Normalize(b - a);
        Vector2 perp = new Vector2(-dirVec.Y, dirVec.X);

        Vector2 a1 = a + perp * w1;
        Vector2 a2 = a - perp * w1;
        Vector2 b1 = b + perp * w2;
        Vector2 b2 = b - perp * w2;

        // two triangles per quad (wound CCW)
        Raylib.DrawTriangle(a2, a1, b1, col);
        Raylib.DrawTriangle(a2, b1, b2, col);
    }
}

bool WearingClassHat => Program.armorHelmet != null && Program.armorHelmet.EndsWith("Hat");

// lean: 0 = front/back, -1 = facing left, +1 = facing right
void DrawClassHat(int cx, int topY, int lean)
{
    Color hc   = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Color dark = new Color((byte)Math.Max(0,hc.R-35),(byte)Math.Max(0,hc.G-35),(byte)Math.Max(0,hc.B-35),(byte)255);
    Color gold = new Color((byte)235,(byte)200,(byte)60,(byte)255);

    if (Program.armorHelmet.Contains("Mage "))
    {
        Raylib.DrawRectangle(cx - 16, topY + 2, 32, 5, dark);                       // wide brim
        int tipX = cx + lean * 8;                                                    // floppy tip follows facing
        Raylib.DrawTriangle(new Vector2(cx - 10, topY + 3), new Vector2(cx + 10, topY + 3),
                            new Vector2(tipX, topY - 16), hc);                       // cone
        Raylib.DrawRectangle(cx - 9, topY - 1, 18, 3, gold);                         // hat band
        Raylib.DrawCircle(cx + lean * 3, topY - 8, 2, gold);                         // star stud
        if (Program.armorHelmet.StartsWith("Supernova"))
        {
            float tw = (MathF.Sin((float)Raylib.GetTime() * 6f) + 1f) / 2f;      // twinkle
            Raylib.DrawCircle(cx + lean * 3, topY - 8, 2 + tw * 2, new Color((byte)255,(byte)255,(byte)220,(byte)200));
        }
    }
    else // Ranger — low hood with a feather
    {
        Raylib.DrawCircle(cx, topY + 6, 13, hc);
        Raylib.DrawRectangle(cx - 13, topY, 26, 7, hc);
        Raylib.DrawRectangle(cx - 13, topY + 6, 26, 3, dark);                        // rim
        int fx = cx - (lean == 0 ? 10 : lean * -4);
        Raylib.DrawTriangle(new Vector2(fx, topY + 2), new Vector2(fx + 4, topY + 2),
                            new Vector2(fx - 4 - lean * 4, topY - 10),
                            new Color((byte)200,(byte)60,(byte)60,(byte)255));       // feather

        if (Program.armorHelmet.StartsWith("Serpent"))
        {
            float g = (MathF.Sin((float)Raylib.GetTime() * 5f) + 1f) / 2f;       // glowing snake eyes
            Color eye = new Color((byte)(150 + g * 105), (byte)230, (byte)60, (byte)255);
            Raylib.DrawCircle(cx - 4, topY + 4, 2, eye);
            Raylib.DrawCircle(cx + 4, topY + 4, 2, eye);
        }
    }
}

void DrawQuiver(int rx, int ry, int rw)   // rendered where the cape cloth would sit
{
    Color leather = new Color((byte)110,(byte)75,(byte)40,(byte)255);
    int qx = rx + rw / 2 - 4;
    Raylib.DrawRectangle(qx, ry + 2, 8, 22, leather);                                // tube
    Raylib.DrawRectangle(qx, ry + 2, 8, 3, new Color((byte)80,(byte)55,(byte)30,(byte)255));
    Raylib.DrawTriangle(new Vector2(qx + 1, ry + 2), new Vector2(qx + 4, ry + 2), new Vector2(qx + 2, ry - 5), Color.White);
    Raylib.DrawTriangle(new Vector2(qx + 5, ry + 2), new Vector2(qx + 8, ry + 2), new Vector2(qx + 7, ry - 5), Color.LightGray);
}

void DrawBookOffhand(int bx, int by)
{
    Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangle(bx, by, 10, 13, col);                                       // cover
    Raylib.DrawRectangle(bx + 2, by + 2, 7, 9, new Color((byte)235,(byte)230,(byte)210,(byte)255)); // pages
    Raylib.DrawRectangle(bx, by, 2, 13, new Color((byte)Math.Max(0,col.R-40),(byte)Math.Max(0,col.G-40),(byte)Math.Max(0,col.B-40),(byte)255)); // spine
}
        public void TakeDamage(int damage)
{
    if (damageCooldown > 0) return;
    Health -= damage;
    damageCooldown = 1f;
    if (Health < 0) Health = 0;
}
void DrawStar(int cx, int cy, float size, Color col)
{
    // 4-point star: two crossed triangles / diamonds
    Raylib.DrawTriangle(
        new Vector2(cx, cy - size), new Vector2(cx - size*0.4f, cy), new Vector2(cx + size*0.4f, cy), col);
    Raylib.DrawTriangle(
        new Vector2(cx, cy + size), new Vector2(cx + size*0.4f, cy), new Vector2(cx - size*0.4f, cy), col);
    Raylib.DrawTriangle(
        new Vector2(cx - size, cy), new Vector2(cx, cy - size*0.4f), new Vector2(cx, cy + size*0.4f), col);
    Raylib.DrawTriangle(
        new Vector2(cx + size, cy), new Vector2(cx, cy + size*0.4f), new Vector2(cx, cy - size*0.4f), col);
}

        public void UpdateHealth(float dt)
        {
            if (damageCooldown > 0) damageCooldown -= dt;

            if (damageCooldown <= 0 && Health < MaxHealth)
            {
                regenTimer += dt;
                if (regenTimer >= 2f)
                {
                    Health++;
                    regenTimer = 0f;
                }
                if (DrunkTimer > 0)
                {
                    DrunkTimer -= dt;
                    if (DrunkTimer <= 0)
                    {
                        DrunkTimer = 0;
                        DrunkLevel = 0;
                    }
                }
            }
        }

       public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> vehicles,  List<Lake> lakes, 
       List<DecorativeBuilding> decorativeBuildings, List<DecorativeAsset> decorativeAssets)
    {
        Vector2 move = GetInput();
        Vector2 oldPos = Position;
        float walkMult = FootpathManager.IsOnSurface(Position) ? 1.12f : 1f;
        Position += move * speed * walkMult * dt;

        if (Program.inPrison)
        {
            float lockRadius = 300f;
            if (Vector2.Distance(Position, Program.prisonCellCenter) > lockRadius)
            {
                Vector2 dir = Vector2.Normalize(Program.prisonCellCenter - Position);
                Position = Program.prisonCellCenter - dir * lockRadius;
            }
        }

        if (Program.IsInWater(Position) && SwimmingLevel >= 5 && !Hidden)
{
            IsSwimming = true;
            float maxSwimDepth = 100f + SwimmingLevel * 25f;
            float coastX = Program.oceanBounds.X;

            SwimDepthRatio = Math.Clamp((Position.X - coastX) / maxSwimDepth, 0f, 1f);   // ADDED
            SwimStrokeTimer += dt * (2f + SwimDepthRatio * 2f);                          // ADDED — strokes faster the deeper you are

            if (Position.X > coastX + maxSwimDepth)
                Position.X = coastX + maxSwimDepth;

            float swimSpeedMultiplier = 0.5f + Math.Min(SwimmingLevel, 50) * 0.01f;
            if (Program.HasSynergy("Sea Lord")) swimSpeedMultiplier *= 1.25f;
            Position = oldPos + (Position - oldPos) * swimSpeedMultiplier;
            AddSwimmingXP(1);
            AddStaminaXP(1); 
        }
        else
        {
            IsSwimming = false;
            SwimDepthRatio = 0f;   // ADDED
            if (Program.IsInWater(Position) && SwimmingLevel < 5 && !Hidden)
            {
                bool xWasSafe = !Program.IsInWater(new Vector2(oldPos.X, Position.Y));
                bool yWasSafe = !Program.IsInWater(new Vector2(Position.X, oldPos.Y));

                if (xWasSafe)
                    Position = new Vector2(oldPos.X, Position.Y);
                else if (yWasSafe)
                    Position = new Vector2(Position.X, oldPos.Y);
                else
                    Position = oldPos;

                 if ((Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up)
                || Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)
                || Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down)
                || Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)))
                Program.ShowNotification("Requires Swimming level 5 to enter the ocean!");
                    }
        }

        if (isSwinging)
        {
            swingTimer += dt;
            if (swingTimer >= swingDuration)
            {
                isSwinging = false;
                swingTimer = 0f;
            }
        }

        foreach (Building building in buildings)
        {
            Rectangle collisionBox = new Rectangle(
                building.Bounds.X,
                building.Bounds.Y,
                building.Bounds.Width,
                building.Bounds.Height - 40
            );

            if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
            {
                Position = oldPos;
            }
        }

      foreach (DecorativeBuilding building in decorativeBuildings)
{
    Rectangle collisionBox = new Rectangle(
        building.Bounds.X,
        building.Bounds.Y,
        building.Bounds.Width,
        Math.Max(building.Bounds.Height -40, 30)  // never shrink below 30px
    );

    if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
    {
        Position = oldPos;
    }
}

foreach (DecorativeAsset asset in decorativeAssets)
{
    if (Raylib.CheckCollisionRecs(Bounds, asset.Bounds))
    {
        Position = oldPos;
    }
}

        
foreach (Rectangle fenceRect in Program.fenceManager.GetCollisionRects())
{
    if (Raylib.CheckCollisionRecs(Bounds, fenceRect))
    {
        Position = oldPos;
        break;
    }
}
foreach (RockObject rock in Program.rocks)
{
    if (!rock.Broken && Raylib.CheckCollisionRecs(Bounds, rock.Bounds))
        Position = oldPos;
}
        
        
        foreach (TreeObject tree in trees)
        {
            if (!tree.Chopped && Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            {
                Position = oldPos;
            }
        }
foreach (Lake lake in Program.lakes)
{
    if (Raylib.CheckCollisionRecs(Bounds, lake.Bounds))
    {
        Position = oldPos;
    }
}

if (Program.tutorialActive)
{
    foreach (var gate in Program.tutorialGates)
    {
        if (Program.tutorialStep <= gate.UnlockedByStep &&
            Raylib.CheckCollisionRecs(Bounds, gate.Bounds))
        {
            Position = oldPos;
        }
    }
}

        // player collides with parked or moving vehicles they aren't driving
        foreach (Vehicle vehicle in vehicles)
        {
            if (vehicle.Driving) continue; // skip if already driving it
            if (vehicle.FuelLocked) continue; // allow player to walk around locked vehicle
            
            if (Raylib.CheckCollisionRecs(Bounds, vehicle.Bounds))
            {
                if (!Raylib.IsKeyDown(KeyboardKey.F))
                    Position = oldPos;
            }
        }

    }

    public void UpdateInterior(float dt, List<Rectangle> objects, int roomW = 1400, int roomH = 1000)
{
    Vector2 move = GetInput();
    Vector2 oldPos = Position;
    Position += move * speed * dt;

    int wallThickness = 20;
    Rectangle wallLeft   = new Rectangle(-wallThickness, 0, wallThickness, roomH);
    Rectangle wallRight  = new Rectangle(roomW, 0, wallThickness, roomH);
    Rectangle wallTop    = new Rectangle(0, -wallThickness, roomW, wallThickness);
    Rectangle wallBottom = new Rectangle(0, roomH, roomW, wallThickness);

    if (Raylib.CheckCollisionRecs(Bounds, wallLeft)   ||
        Raylib.CheckCollisionRecs(Bounds, wallRight)  ||
        Raylib.CheckCollisionRecs(Bounds, wallTop)    ||
        Raylib.CheckCollisionRecs(Bounds, wallBottom))
    {
        Position = oldPos;
    }

    foreach (Rectangle rect in objects)
    {
        if (Raylib.CheckCollisionRecs(Bounds, rect))
        {
            Position = oldPos;
        }
    }
}

        Vector2 GetInput()
{
    if (Program.isFishing)
        return Vector2.Zero;

    if (InventoryOpen || Program.armorMenuOpen || Program.bedMenuOpen || Program.rouletteOpen || Program.blackjackOpen || Program.testTransitionActive)
        return Vector2.Zero;

    Vector2 move = Vector2.Zero;

    if (Raylib.IsKeyDown(KeyboardKey.W)  || Raylib.IsKeyDown(KeyboardKey.Up))
        move.Y -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
        move.Y += 1;
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
        move.X -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
        move.X += 1;

    if (move != Vector2.Zero)
    {
        move = Vector2.Normalize(move);
        AddAthleticsXP(1);
        AddStaminaXP(1);

        dustParticles.Add((
            new Vector2(Position.X + 20, Position.Y + 66),
            0.3f,
            new Color((byte)Raylib.GetRandomValue(100, 150),
                      (byte)Raylib.GetRandomValue(100, 150),
                      (byte)Raylib.GetRandomValue(50, 80),
                      (byte)180)
        ));

        // update facing — horizontal takes priority for side view
        if (MathF.Abs(move.X) >= MathF.Abs(move.Y))
            Facing = move.X < 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            Facing = move.Y < 0 ? FacingDirection.Up : FacingDirection.Down;

        isMoving = true;
    }
    else
    {
        isMoving = false;
    }
float dt = Raylib.GetFrameTime();
    // update dust particles
for (int i = dustParticles.Count - 1; i >= 0; i--)
{
    var p = dustParticles[i];
    p.timer -= dt;
    dustParticles[i] = p;
    if (p.timer <= 0) dustParticles.RemoveAt(i);
}


    return move * DrunkSpeedMultiplier;
}

public void DriveAnimation(bool moving, float dt)
{
    isMoving = moving;
    if (isMoving)
    {
        walkTimer += dt * 8f;
        if (walkTimer >= 1f)
        {
            walkTimer = 0f;
            walkFrame = !walkFrame;
        }
    }
    else
    {
        walkTimer = 0f;
        walkFrame = false;
    }
}

        public void AddWoodcuttingXP(int xp)
        {
            if (WoodcuttingLevel >= 100) return;

            WoodcuttingXP += (int)(xp * Program.SynergyXPMultiplier());

            int requiredXP = WoodcuttingLevel * WoodcuttingLevel * 50;

            if (WoodcuttingXP >= requiredXP)
            {
                WoodcuttingXP = 0;
                WoodcuttingLevel++;
                Program.ShowLevelUp("Woodcutting", WoodcuttingLevel);
            }
        }
        public void AddMiningXP(int xp)
        {
            if (MiningLevel >= 100) return;
            MiningXP += (int)(xp * Program.SynergyXPMultiplier());
            int requiredXP = MiningLevel * MiningLevel * 50;
            if (MiningXP >= requiredXP)
            {
                MiningXP = 0;
                MiningLevel++;
                Program.ShowLevelUp("Mining", MiningLevel);
            }
        }

        public void AddFishingXP(int xp)
        {
            if (FishingLevel >= 100) return;

            FishingXP += (int)(xp * Program.SynergyXPMultiplier());

            int requiredXP = FishingLevel * FishingLevel * 50;

            if (FishingXP >= requiredXP)
            {
                FishingXP = 0;
                FishingLevel++;
                Program.ShowLevelUp("Fishing", FishingLevel);
            }
        }

        public void AddCombatXP(int xp)
        {
            if (CombatLevel >= 100) return;

            CombatXP += (int)(xp * Program.SynergyXPMultiplier());

            int requiredXP = CombatLevel * CombatLevel * 50;

            if (CombatXP >= requiredXP)
            {
                CombatXP = 0;
                CombatLevel++;
                Program.ShowLevelUp("Combat", CombatLevel);
            }
        }

        public void AddOneHandMeleeXP(int xp)
        {
            if (OneHandMeleeLevel >= 100) return;
            OneHandMeleeXP += (int)(xp * Program.SynergyXPMultiplier());
            int requiredXP = OneHandMeleeLevel * OneHandMeleeLevel * 50;
            if (OneHandMeleeXP >= requiredXP)
            {
                OneHandMeleeXP = 0;
                OneHandMeleeLevel++;
                Program.ShowLevelUp("1H Melee", OneHandMeleeLevel);
            }
        }

        public void AddTwoHandMeleeXP(int xp)
        {
            if (TwoHandMeleeLevel >= 100) return;
            TwoHandMeleeXP += (int)(xp * Program.SynergyXPMultiplier());
            int requiredXP = TwoHandMeleeLevel * TwoHandMeleeLevel * 50;
            if (TwoHandMeleeXP >= requiredXP)
            {
                TwoHandMeleeXP = 0;
                TwoHandMeleeLevel++;
                Program.ShowLevelUp("2H Melee", TwoHandMeleeLevel);
            }
        }

        public void AddDrivingXP(int xp)
{
    if (DrivingLevel >= 100) return;

    DrivingXP += (int)(xp * Program.SynergyXPMultiplier());

    int requiredXP = DrivingLevel * DrivingLevel * 150;

    if (DrivingXP >= requiredXP)
    {
        DrivingXP = 0;
        DrivingLevel++;
        Program.ShowLevelUp("Driving", DrivingLevel);
    }
}

public void AddAthleticsXP(int xp)
{
    if (AthleticsLevel >= 100) return;

    AthleticsXP += (int)(xp * Program.SynergyXPMultiplier());

    int requiredXP = AthleticsLevel * AthleticsLevel * 150;

    if (AthleticsXP >= requiredXP)
    {
        AthleticsXP = 0;
        AthleticsLevel++;
        Program.ShowLevelUp("Athletics", AthleticsLevel);
    }
}

public void AddStrengthXP(int xp)
{
    if (StrengthLevel >= 100) return;
    StrengthXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = StrengthLevel * StrengthLevel * 50;
    if (StrengthXP >= requiredXP)
    {
        StrengthXP = 0;
        StrengthLevel++;
        Program.ShowLevelUp("Strength", StrengthLevel);
    }
}

public void AddGamblingXP(int xp)
{
    if (GamblingLevel >= 100) return;
    GamblingXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = GamblingLevel * GamblingLevel * 50;
    if (GamblingXP >= requiredXP)
    {
        GamblingXP = 0;
        GamblingLevel++;
        Program.ShowLevelUp("Gambling", GamblingLevel);
    }
}
public void AddFarmingXP(int xp)
{
    if (FarmingLevel >= 100) return;
    FarmingXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = FarmingLevel * FarmingLevel * 50;
    if (FarmingXP >= required)
    {
        FarmingXP = 0;
        FarmingLevel++;
        Program.ShowLevelUp("Farming", FarmingLevel);
    }
}
public void AddRidingXP(int xp)
{
    if (RidingLevel >= 100) return;
    RidingXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = RidingLevel * RidingLevel * 150;
    if (RidingXP >= requiredXP)
    {
        RidingXP = 0;
        RidingLevel++;
        Program.ShowLevelUp("Riding", RidingLevel);
    }
}

public void AddCyclingXP(int xp)
{
    if (CyclingLevel >= 100) return;
    CyclingXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = CyclingLevel * CyclingLevel * 150;
    if (CyclingXP >= requiredXP)
    {
        CyclingXP = 0;
        CyclingLevel++;
        Program.ShowLevelUp("Cycling", CyclingLevel);
    }
}
public void AddSwimmingXP(int xp)
{
    if (SwimmingLevel >= 100) return;
    SwimmingXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = SwimmingLevel * SwimmingLevel * 50;
    if (SwimmingXP >= requiredXP)
    {
        SwimmingXP = 0;
        SwimmingLevel++;
        Program.ShowLevelUp("Swimming", SwimmingLevel);
    }
}
public void AddStaminaXP(int xp)
{
    if (StaminaLevel >= 100) return;
    StaminaXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = StaminaLevel * StaminaLevel * 300;   // 2x slower than the movement skills — it trains from all of them
    if (StaminaXP >= requiredXP)
    {
        StaminaXP = 0;
        StaminaLevel++;
        Program.ShowLevelUp("Stamina", StaminaLevel);
    }
}

public void AddDivingXP(int xp)
{
    if (DivingLevel >= 100) return;
    DivingXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = DivingLevel * DivingLevel * 50;
    if (DivingXP >= requiredXP)
    {
        DivingXP = 0;
        DivingLevel++;
        Program.ShowLevelUp("Diving", DivingLevel);
    }
}
public void AddSportsXP(int xp)
{
    if (SportsLevel >= 100) return;
    SportsXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = SportsLevel * SportsLevel * 50;
    if (SportsXP >= requiredXP)
    {
        SportsXP = 0;
        SportsLevel++;
        Program.ShowLevelUp("Sports", SportsLevel);
    }
}
public void AddRangedXP(int xp)
{
    if (RangedLevel >= 100) return;
    RangedXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = RangedLevel * RangedLevel * 50;
    if (RangedXP >= requiredXP)
    {
        RangedXP = 0;
        RangedLevel++;
        Program.ShowLevelUp("Ranged", RangedLevel);
    }
}
public void AddCookingXP(int xp)
{
    if (CookingLevel >= 100) return;
    CookingXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = CookingLevel * CookingLevel * 40;
    if (CookingXP >= required)
    {
        CookingXP = 0;
        CookingLevel++;
        Program.ShowLevelUp("Cooking", CookingLevel);
    }
}

public void AddElementalXP(int xp)
{
    if (ElementalLevel >= 100) return;
    ElementalXP += (int)(xp * Program.SynergyXPMultiplier());
    int requiredXP = ElementalLevel * ElementalLevel * 50;
    if (ElementalXP >= requiredXP)
    {
        ElementalXP = 0;
        ElementalLevel++;
        Program.ShowLevelUp("Elemental", ElementalLevel);
    }
}
public void AddCraftingXP(int xp)
{
    if (CraftingLevel >= 100) return;
    CraftingXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = CraftingLevel * CraftingLevel * 50;
    if (CraftingXP >= required)
    {
        CraftingXP = 0;
        CraftingLevel++;
        Program.ShowLevelUp("Crafting", CraftingLevel);
    }
}
public void AddBlacksmithXP(int xp)
{
    if (BlacksmithLevel >= 100) return;
    BlacksmithXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = BlacksmithLevel * BlacksmithLevel * 50;
    if (BlacksmithXP >= required)
    {
        BlacksmithXP = 0;
        BlacksmithLevel++;
        Program.ShowLevelUp("Blacksmith", BlacksmithLevel);
    }
}
public void AddEnchantingXP(int xp)
{
    if (EnchantingLevel >= 100) return;
    EnchantingXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = EnchantingLevel * EnchantingLevel * 50;
    if (EnchantingXP >= required)
    {
        EnchantingXP = 0;
        EnchantingLevel++;
        Program.ShowLevelUp("Enchanting", EnchantingLevel);
    }
}
public void AddEducationXP(int xp)
{
    if (EducationLevel >= 100) return;
    EducationXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = EducationLevel * EducationLevel * 50;
    if (EducationXP >= required)
    {
        EducationXP = 0;
        EducationLevel++;
        Program.ShowLevelUp("Education", EducationLevel);
    }
}
public void AddBoatingXP(int xp)
{
    if (BoatingLevel >= 100) return;
    BoatingXP += (int)(xp * Program.SynergyXPMultiplier());
    int required = BoatingLevel * BoatingLevel * 50;
    if (BoatingXP >= required)
    {
        BoatingXP = 0;
        BoatingLevel++;
        Program.ShowLevelUp("Boating", BoatingLevel);
    }
}
public void TriggerChopAnim()  { isChopping = true; chopAnimAngle = 0f; }
public void TriggerMineAnim()  { isMining  = true; chopAnimAngle = 0f; }


   public void Draw()
{
    if (Hidden) return;

    // update walk animation timer
    if (isMoving)
    {
        // dust particles
    foreach (var p in dustParticles)
    {
        byte alpha = (byte)(255 * (p.timer / 0.3f));
        float size = 4f * (p.timer / 0.3f);
        Raylib.DrawCircle((int)p.pos.X + Raylib.GetRandomValue(-4, 4),
                          (int)p.pos.Y,
                          size,
                          new Color(p.color.R, p.color.G, p.color.B, alpha));
    }
        walkTimer += Raylib.GetFrameTime() * 8f;
        if (walkTimer >= 1f)
        {
            walkTimer = 0f;
            walkFrame = !walkFrame;
        }
    }
    else
    {
        walkTimer = 0f;
        walkFrame = false;
    }

    // drive chop/mine swing animation
if (isChopping || isMining)
{
    chopAnimAngle += Raylib.GetFrameTime() * 800f;
    if (chopAnimAngle > 360f) { chopAnimAngle = 0f; isChopping = false; isMining = false; }
}

// bob the player position during swing
float chopBob = (isChopping || isMining)
    ? MathF.Sin(chopAnimAngle * MathF.PI / 180f) * 4f
    : 0f;

int x = (int)Position.X;
int y = (int)(Position.Y + chopBob);

DrawArmorAura(x, y);

if (Program.useLayeredPlayer)
{ 
    DrawCharacter(x, y);
}
else 
{
    switch (Facing)
    {
        case FacingDirection.Down:
            DrawFacingDown(x, y);
            DrawHeldItem(x, y, GetDisplayedItem());
            break;
        case FacingDirection.Up:
            DrawFacingUp(x, y);
            DrawHeldItem(x, y, GetDisplayedItem());
            break;
        case FacingDirection.Left:
            DrawFacingLeft(x, y);
            DrawHeldItem(x, y, GetDisplayedItem());
            break;
        case FacingDirection.Right:
            DrawFacingRight(x, y);
            DrawHeldItem(x, y, GetDisplayedItem());
            DrawShieldRight(x, y);
            break;
    }
}
    // draw held item on top of player
    if (HasBasket)
        DrawHeldBasket(x, y);
    else if (HasTrolley)
        DrawPushedTrolley(x, y);

    // center indicaor for the player
    //Raylib.DrawCircle((int)Center.X, (int)Center.Y, 4, Color.Red);
}

// ── LPC sprite draw (incremental migration) ──────────────
bool TryDrawSprite(int x, int y)
{
    string item = GetDisplayedItem();
    if (item == null) return false;

    // Which sprite sheet for the active tool? null = not migrated yet → shape draw.
    string sheet = null;
    if (item.Contains("Pickaxe"))                               sheet = "player_pickaxe";
    else if (item.Contains("Axe") && !item.Contains("War Axe")) sheet = "player_axe";
    else if (item.Contains("Rod"))                              sheet = "player_fishingrod";
    if (sheet == null) return false;                            // other tools/weapons still shape-drawn

    int dir = Facing switch
    {
        FacingDirection.Up    => 0,
        FacingDirection.Left  => 1,
        FacingDirection.Down  => 2,
        FacingDirection.Right => 3,
        _ => 2
    };

    const int Cell = 64;
    int row, frame;

    bool fishing = Program.isFishing && sheet == "player_fishingrod";
    bool chopping = isSwinging && sheet != "player_fishingrod";

    if (fishing)                    // rod cast → shoot rows (bow-style, reads as casting)
    {
        row = 16 + dir;             // shoot rows 16–19 on the classic block
        frame = (int)(chopAnimAngle / 360f * 8) % 8;   // reuse timer if you drive it; else stays frame 0
    }
    else if (chopping)              // pickaxe/axe swing → mining rows, fires on input anywhere
    {
        row = 55 + dir;                                   
        float t = swingTimer / swingDuration;             
        frame = Math.Clamp((int)(t * 6), 0, 5);           
    }
    else if (isMoving)              // walking with tool
    {
        row = 8 + dir;
        frame = walkFrame ? 4 : 0;
    }
    else                            // idle with tool
    {
        row = 8 + dir;
        frame = 0;
    }

    Rectangle src = new(frame * Cell, row * Cell, Cell, Cell);
    float scale = 1.5f;
    Rectangle dst = new(x - (Cell*scale - Cell)/2f, y - (Cell*scale - Cell)/2f, Cell*scale, Cell*scale);
    Raylib.DrawTexturePro(AssetManager.Get(sheet), src, dst, Vector2.Zero, 0f, Color.White);
    return true;
}

// Feed current state into the layered renderer, then draw it.
bool _diagPrinted;   
void DrawCharacter(int x, int y)
{
    Character.Dir = Facing switch
    {
        FacingDirection.Up    => 0,
        FacingDirection.Left  => 1,
        FacingDirection.Down  => 2,
        FacingDirection.Right => 3,
        _ => 2
    };

    string held = GetDisplayedItem();
    Character.HeldTexture = GetHeldSheet(held);

    // TEMP DIAG: prints once per swing — delete when bindings are confirmed
    if (isSwinging && !_diagPrinted)
    {
        _diagPrinted = true;
        Console.WriteLine($"[LPC DIAG] held='{held}' " +
            $"body={Character.BodyTexture.Id}/{Character.BodyTexture.Height}px " +
            $"cloth={Character.ClothingTexture.Id}/{Character.ClothingTexture.Height}px " +
            $"hair={Character.HairTexture.Id}/{Character.HairTexture.Height}px " +
            $"boots={Character.BootsTexture.Id}/{Character.BootsTexture.Height}px " +
            $"heldTex={Character.HeldTexture.Id}/{Character.HeldTexture.Height}px");
    }
    if (!isSwinging) _diagPrinted = false;

    var (actRow, actFrames) = GetActionAnim(held);
    bool smashTool = held != null &&
        (held.Contains("Pickaxe") || held.Contains("Hammer") ||          // CHANGED: hammer uses 128px smash block
        (held.Contains("Axe") && !held.Contains("War Axe")));

    bool fishing = Program.isFishing && held != null && held.Contains("Rod");   // NEW

    if (fishing)                                                      // CHANGED: rod art lives in its 128px block, not the 64px rows
    {
        float castT = isSwinging ? swingTimer / swingDuration : 1f;
        Character.FallbackRow    = PlayerCharacter.RowShoot;          // NEW: body layers cast via 64px shoot rows
        Character.FallbackFrames = 13;                                // NEW: shoot rows are 13 columns
        Character.OversizeBlock  = 0;                                 // NEW: first (only) 128px block on the rod sheet
        Character.SetAction(PlayerCharacter.RowShoot, castT, 13, oversize: true);   // CHANGED: oversize on
    }
    else if (isSwinging && actRow >= 0)
    {
        float t = swingTimer / swingDuration;
        Character.FallbackRow    = PlayerCharacter.RowSlash;          // NEW: restore smash fallback after fishing
        Character.FallbackFrames = 6;                                 
        Character.OversizeBlock = 0;                                   // smash = first (only) 128px block
        Character.SetAction(actRow, t, smashTool ? 6 : actFrames, oversize: smashTool);   // CHANGED: no targetRow
    }
    else if (isMoving)
    {
        Character.BaseRow = PlayerCharacter.RowWalk;
        float walkFps = Math.Clamp(speed / 28f, 5f, 14f);          // CHANGED: anim rate follows move speed
        Character.TickWalk(Raylib.GetFrameTime(), 9, walkFps);
    }
    else
    {
        Character.SetIdle();
    }

    Character.Draw(new Vector2(x, y));
    if (Program.drawShapeArmorOnSprite)               // NEW: test overlay of drawn armor
    {
        switch (Facing)
        {
            case FacingDirection.Down:  DrawArmorOverlayDown(x, y);  break;
            case FacingDirection.Up:    DrawArmorOverlayUp(x, y);    break;
            case FacingDirection.Left:  DrawArmorOverlayLeft(x, y);  break;
            case FacingDirection.Right: DrawArmorOverlayRight(x, y); break;
        }
    }
}

void DrawArmorOverlayDown(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;

      // ── Mastery: Hawkeye's Cloak (Ranged Lv 100) ──
    if (Program.armorCape != null && Program.armorCape == "Hawkeye's Cloak")
    {
        Color cloak  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color cloakH = new Color((byte)150,(byte)100,(byte)55,(byte)255);
        // draping cloak behind shoulders, visible at sides
        Raylib.DrawRectangle(x + 2, y + 22, 8, 30, cloak);   // left drape
        Raylib.DrawRectangle(x + 30, y + 22, 8, 30, cloak);  // right drape
        // tattered bottom edge
        Raylib.DrawTriangle(new Vector2(x + 2, y + 52), new Vector2(x + 10, y + 52),
            new Vector2(x + 6, y + 58), cloak);
        Raylib.DrawTriangle(new Vector2(x + 30, y + 52), new Vector2(x + 38, y + 52),
            new Vector2(x + 34, y + 58), cloak);
        // clasp at collar
        Raylib.DrawCircle(x + 20, y + 24, 3, cloakH);
    }
    else if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 11, y + 24, 18);
        else Raylib.DrawRectangle(x + 11, y + 24, 18, 30, cc);
    }

    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 24, 20, 30, col);
        Raylib.DrawRectangle(x + 13, y + 28, 14, 10, new Color((byte)Math.Max(0,col.R-25),(byte)Math.Max(0,col.G-25),(byte)Math.Max(0,col.B-25),(byte)255));
        if (Program.armorBody.EndsWith("Top"))
            Raylib.DrawRectangle(x + 18, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))
            Raylib.DrawRectangle(x + 10, y + 44, 20, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 2,  y + 38 + armSwing, 8, 8, col);
        Raylib.DrawRectangle(x + 30, y + 38 - armSwing, 8, 8, col);
    }

    if (Program.armorLegs != null)
    {
        Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 10, y + 54,     8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 10, col);
            }
            else
            {
                Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54,     8, 10, col);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54, 8, 10, col);
            Raylib.DrawRectangle(x + 22, y + 54, 8, 10, col);
        }
    }

    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 9,  y + 63,     10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63 - 6, 10, 7, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 9,  y + 63 - 6, 10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63,     10, 7, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 9,  y + 63, 10, 7, bc);
            Raylib.DrawRectangle(x + 21, y + 63, 10, 7, bc);
        }
    }

    if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }

    if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
    {
        Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 20, y + 8, 13, hc);
        Raylib.DrawRectangle(x + 7, y + 2, 26, 8, hc);
        Raylib.DrawRectangle(x + 7,  y + 8, 6, 10, hc);
        Raylib.DrawRectangle(x + 27, y + 8, 6, 10, hc);
        Raylib.DrawRectangle(x + 7, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

        if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
        {
            Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
            Raylib.DrawTriangle(
                new Vector2(x + 17, y + 1),
                new Vector2(x + 23, y + 1),
                new Vector2(x + 20, y - 7),
                spikeCol);
        }
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
    {
        Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
        Raylib.DrawRectangle(x + 4, y + 2, 6, 5, horn);
        Raylib.DrawRectangle(x + 1, y - 2, 6, 5, horn);
        Raylib.DrawTriangle(new Vector2(x + 1, y - 2), new Vector2(x + 6, y - 2), new Vector2(x + 2, y - 11), horn);
        Raylib.DrawRectangle(x + 30, y + 2, 6, 5, horn);
        Raylib.DrawRectangle(x + 33, y - 2, 6, 5, horn);
        Raylib.DrawTriangle(new Vector2(x + 33, y - 2), new Vector2(x + 38, y - 2), new Vector2(x + 37, y - 11), horn);
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
    {
        Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
        DrawHorn(x + 12, y + 6, -1, horn);
        DrawHorn(x + 28, y + 6, +1, horn);
    }

    // ── Mastery: Marksman Hood (Ranged Lv 100) ──
    if (Program.armorHelmet != null && Program.armorHelmet == "Marksman Hood")
    {
        Color hood  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color hoodD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // hood wraps around head
        Raylib.DrawCircle(x + 20, y + 12, 14, hood);
        // face opening — cut-out so skin shows through
        Raylib.DrawCircle(x + 20, y + 14, 10, SkinColor);
        // brow ridge
        Raylib.DrawRectangle(x + 8, y + 5, 24, 4, hoodD);
        // feather on right side
        Color feather = new Color((byte)220,(byte)150,(byte)50,(byte)255);
        Raylib.DrawLineEx(new Vector2(x + 32, y + 6), new Vector2(x + 38, y - 8), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 32, y + 6), new Vector2(x + 40, y - 4), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 32, y + 6), new Vector2(x + 36, y - 10), 2, feather);
    }
    
    if (WearingClassHat) DrawClassHat(x + 20, y, 0);

    if (Program.armorShield != null && !Program.IsTwoHandedWeapon(Program.armorWeapon))
    {
        Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
        if (Program.armorShield.EndsWith("Book")) DrawBookOffhand(x + 4, y + 31 + armSwing);
        else Raylib.DrawRectangle(x + 4, y + 30 + armSwing, 8, 16, col);
    }
}

void DrawArmorOverlayUp(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;

    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 24, 20, 30, col);
        if (Program.armorBody.EndsWith("Top"))
            Raylib.DrawRectangle(x + 18, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))
            Raylib.DrawRectangle(x + 10, y + 44, 20, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 2,  y + 38 + armSwing, 8, 8, col);
        Raylib.DrawRectangle(x + 30, y + 38 - armSwing, 8, 8, col);
    }

    if (Program.armorLegs != null)
    {
        Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 10, y + 54,     8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 10, col);
            }
            else
            {
                Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54,     8, 10, col);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54, 8, 10, col);
            Raylib.DrawRectangle(x + 22, y + 54, 8, 10, col);
        }
    }

    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 9,  y + 63,     10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63 - 6, 10, 7, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 9,  y + 63 - 6, 10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63,     10, 7, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 9,  y + 63, 10, 7, bc);
            Raylib.DrawRectangle(x + 21, y + 63, 10, 7, bc);
        }
    }

    if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }

    if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
    {
        Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 20, y + 10, 13, hc);
        Raylib.DrawRectangle(x + 7, y + 2, 26, 9, hc);
        Raylib.DrawRectangle(x + 7,  y + 10, 6, 9, hc);
        Raylib.DrawRectangle(x + 27, y + 10, 6, 9, hc);
        Raylib.DrawRectangle(x + 11, y + 16, 18, 5, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

        if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
        {
            Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
            Raylib.DrawTriangle(
                new Vector2(x + 17, y + 1),
                new Vector2(x + 23, y + 1),
                new Vector2(x + 20, y - 7),
                spikeCol);
        }
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
    {
        Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
        Raylib.DrawRectangle(x + 4, y + 2, 6, 5, horn);
        Raylib.DrawRectangle(x + 1, y - 2, 6, 5, horn);
        Raylib.DrawTriangle(new Vector2(x + 1, y - 2), new Vector2(x + 6, y - 2), new Vector2(x + 2, y - 11), horn);
        Raylib.DrawRectangle(x + 30, y + 2, 6, 5, horn);
        Raylib.DrawRectangle(x + 33, y - 2, 6, 5, horn);
        Raylib.DrawTriangle(new Vector2(x + 33, y - 2), new Vector2(x + 38, y - 2), new Vector2(x + 37, y - 11), horn);
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
    {
        Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
        DrawHorn(x + 12, y + 6, -1, horn);
        DrawHorn(x + 28, y + 6, +1, horn);
    }

    // ── Mastery: Marksman Hood (facing up) ──
    if (Program.armorHelmet != null && Program.armorHelmet == "Marksman Hood")
    {
        Color hood  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color hoodD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // hood covers back of head
        Raylib.DrawCircle(x + 20, y + 12, 14, hood);
        // pointed tip at back
        Raylib.DrawTriangle(new Vector2(x + 14, y + 2), new Vector2(x + 26, y + 2),
            new Vector2(x + 20, y - 6), hoodD);
        // feather tucked in right side
        Color feather = new Color((byte)220,(byte)150,(byte)50,(byte)255);
        Raylib.DrawLineEx(new Vector2(x + 30, y + 6), new Vector2(x + 36, y - 6), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 30, y + 6), new Vector2(x + 38, y - 2), 2, feather);
    }

    if (WearingClassHat) DrawClassHat(x + 20, y - 1, 0);

    // ── Mastery: Hawkeye's Cloak (facing up — full cloak visible from behind) ──
    if (Program.armorCape != null && Program.armorCape == "Hawkeye's Cloak")
    {
        Color cloak  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color cloakH = new Color((byte)150,(byte)100,(byte)55,(byte)255);
        Color cloakD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // full back cloak
        Raylib.DrawRectangle(x + 6, y + 20, 28, 38, cloak);
        // centre seam
        Raylib.DrawRectangle(x + 19, y + 22, 2, 34, cloakD);
        // tattered bottom edge
        for (int t = 0; t < 4; t++)
            Raylib.DrawTriangle(
                new Vector2(x + 6 + t * 7, y + 58), new Vector2(x + 13 + t * 7, y + 58),
                new Vector2(x + 9 + t * 7, y + 64), cloak);
        // shoulder highlight
        Raylib.DrawRectangle(x + 6, y + 20, 28, 3, cloakH);
    }
    else if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 10, y + 22, 20);
        else Raylib.DrawRectangle(x + 10, y + 22, 20, 34, cc);
    }
}

void DrawArmorOverlayLeft(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;

    if (Program.armorShield != null && !Program.IsTwoHandedWeapon(Program.armorWeapon))
    {
        Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
        Raylib.DrawRectangle(x - 2, y + 28 + armSwing, 10, 18, col);
        Raylib.DrawCircle(x + 3, y + 37 + armSwing, 3, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }

    // ── Mastery: Hawkeye's Cloak (facing left — drapes behind) ──
    if (Program.armorCape != null && Program.armorCape == "Hawkeye's Cloak")
    {
        Color cloak  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color cloakD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // cloak trails behind (right side)
        Raylib.DrawRectangle(x + 26, y + 22, 12, 34, cloak);
        Raylib.DrawRectangle(x + 26, y + 22, 12, 3, new Color((byte)150,(byte)100,(byte)55,(byte)255));
        // tattered edge
        Raylib.DrawTriangle(new Vector2(x + 26, y + 56), new Vector2(x + 38, y + 56),
            new Vector2(x + 32, y + 62), cloak);
        Raylib.DrawTriangle(new Vector2(x + 32, y + 56), new Vector2(x + 40, y + 56),
            new Vector2(x + 36, y + 64), cloakD);
    }
    else if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 18, y + 24, 12);
        else Raylib.DrawRectangle(x + 18, y + 24, 12, 28, cc);
    }

    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 12, y + 24, 14, 30, col);
        if (Program.armorBody.EndsWith("Top"))
            Raylib.DrawRectangle(x + 17, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))
            Raylib.DrawRectangle(x + 12, y + 44, 14, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 38 + armSwing, 8, 7, col);
    }

    if (Program.armorLegs != null)
    {
        Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 8,  y + 54,     8, 14, col);
                Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 14, col);
            }
            else
            {
                Raylib.DrawRectangle(x + 8,  y + 54 - 8, 8, 14, col);
                Raylib.DrawRectangle(x + 16, y + 54,     8, 14, col);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 8,  y + 54, 8, 12, col);
            Raylib.DrawRectangle(x + 16, y + 54, 8, 12, col);
        }
    }

    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 6,  y + 64, 10, 6, bc);
                Raylib.DrawRectangle(x + 14, y + 56, 10, 6, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 6,  y + 56, 10, 6, bc);
                Raylib.DrawRectangle(x + 14, y + 64, 10, 6, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 6,  y + 62, 10, 6, bc);
            Raylib.DrawRectangle(x + 14, y + 62, 10, 6, bc);
        }
    }

    if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 11, y + 50, 16, 14, rc);      // narrowed for side view (per your inline note)
        Raylib.DrawRectangle(x + 11, y + 62, 16, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }

    if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
    {
        Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 18, y + 8, 13, hc);
        Raylib.DrawRectangle(x + 6, y + 2, 26, 8, hc);
        Raylib.DrawRectangle(x + 24, y + 8, 6, 11, hc);
        Raylib.DrawRectangle(x + 6, y + 8, 5, 7, hc);
        Raylib.DrawRectangle(x + 6, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

        if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
        {
            Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
            Raylib.DrawTriangle(
                new Vector2(x + 15, y + 1),
                new Vector2(x + 21, y + 1),
                new Vector2(x + 18, y - 7),
                spikeCol);
        }
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
    {
        Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
        Color hornFar = new Color((byte)255, (byte)225, (byte)0, (byte)255);
        Raylib.DrawTriangle(
            new Vector2(x + 24, y - 2),
            new Vector2(x + 28, y - 2),
            new Vector2(x + 38, y - 15),
            hornFar);
        Raylib.DrawRectangle(x + 18, y, 6, 5, horn);
        Raylib.DrawRectangle(x + 22, y - 4, 6, 5, horn);
        Raylib.DrawTriangle(
            new Vector2(x + 22, y - 4),
            new Vector2(x + 29, y - 4),
            new Vector2(x + 33, y - 14),
            horn);
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
    {
        Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
        Color hornFar = new Color((byte)22,(byte)18,(byte)18,(byte)255);
        DrawHorn(x + 22, y + 5, +1, hornFar);
        DrawHorn(x + 18, y + 6, +1, horn);
    }

    // ── Mastery: Marksman Hood (facing left) ──
    if (Program.armorHelmet != null && Program.armorHelmet == "Marksman Hood")
    {
        Color hood  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color hoodD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // hood wraps head, profile view
        Raylib.DrawCircle(x + 16, y + 12, 13, hood);
        // face opening
        Raylib.DrawCircle(x + 14, y + 14, 9, SkinColor);
        // brow ridge
        Raylib.DrawRectangle(x + 4, y + 6, 16, 4, hoodD);
        // feather sweeping back
        Color feather = new Color((byte)220,(byte)150,(byte)50,(byte)255);
        Raylib.DrawLineEx(new Vector2(x + 26, y + 4), new Vector2(x + 36, y - 6), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 26, y + 4), new Vector2(x + 34, y - 10), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 26, y + 4), new Vector2(x + 38, y - 3), 2, feather);
    }

    if (WearingClassHat) DrawClassHat(x + 18, y - 2, -1);
}

void DrawArmorOverlayRight(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;

    // ── Mastery: Hawkeye's Cloak (facing right — drapes behind left) ──
    if (Program.armorCape != null && Program.armorCape == "Hawkeye's Cloak")
    {
        Color cloak  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color cloakD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // cloak trails behind (left side)
        Raylib.DrawRectangle(x + 2, y + 22, 12, 34, cloak);
        Raylib.DrawRectangle(x + 2, y + 22, 12, 3, new Color((byte)150,(byte)100,(byte)55,(byte)255));
        // tattered edge
        Raylib.DrawTriangle(new Vector2(x + 2, y + 56), new Vector2(x + 14, y + 56),
            new Vector2(x + 8, y + 62), cloak);
        Raylib.DrawTriangle(new Vector2(x, y + 56), new Vector2(x + 8, y + 56),
            new Vector2(x + 4, y + 64), cloakD);
    }
    else if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 10, y + 24, 12);
        else Raylib.DrawRectangle(x + 10, y + 24, 12, 28, cc);
    }

    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 14, y + 24, 14, 30, col);
        if (Program.armorBody.EndsWith("Top"))
            Raylib.DrawRectangle(x + 19, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))
            Raylib.DrawRectangle(x + 14, y + 44, 14, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 22, y + 38 + armSwing, 8, 7, col);
    }

    if (Program.armorLegs != null)
    {
        Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 16, y + 54,     8, 14, col);
                Raylib.DrawRectangle(x + 24, y + 54 - 8, 8, 14, col);
            }
            else
            {
                Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 14, col);
                Raylib.DrawRectangle(x + 24, y + 54,     8, 14, col);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 16, y + 54, 8, 12, col);
            Raylib.DrawRectangle(x + 24, y + 54, 8, 12, col);
        }
    }

    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 14, y + 64, 10, 6, bc);
                Raylib.DrawRectangle(x + 22, y + 56, 10, 6, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 14, y + 56, 10, 6, bc);
                Raylib.DrawRectangle(x + 22, y + 64, 10, 6, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 14, y + 62, 10, 6, bc);
            Raylib.DrawRectangle(x + 22, y + 62, 10, 6, bc);
        }
    }

    if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 13, y + 50, 16, 14, rc);      // narrowed for side view (per your inline note)
        Raylib.DrawRectangle(x + 13, y + 62, 16, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }

    if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
    {
        Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 22, y + 8, 13, hc);
        Raylib.DrawRectangle(x + 9, y + 2, 26, 8, hc);
        Raylib.DrawRectangle(x + 10, y + 8, 6, 11, hc);
        Raylib.DrawRectangle(x + 29, y + 8, 5, 7, hc);
        Raylib.DrawRectangle(x + 9, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

        if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
        {
            Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
            Raylib.DrawTriangle(
                new Vector2(x + 19, y + 1),
                new Vector2(x + 25, y + 1),
                new Vector2(x + 22, y - 7),
                spikeCol);
        }
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
    {
        Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
        Color hornFar = new Color((byte)255, (byte)225, (byte)0, (byte)255);
        Raylib.DrawTriangle(
            new Vector2(x + 12, y - 2),
            new Vector2(x + 17, y - 2),
            new Vector2(x + 5, y - 15),
            hornFar);
        Raylib.DrawRectangle(x + 16, y, 6, 5, horn);
        Raylib.DrawRectangle(x + 12, y - 4, 6, 5, horn);
        Raylib.DrawTriangle(
            new Vector2(x + 12, y - 4),
            new Vector2(x + 18, y - 4),
            new Vector2(x + 9, y - 14),
            horn);
    }

    if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
    {
        Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
        Color hornFar = new Color((byte)22,(byte)18,(byte)18,(byte)255);
        DrawHorn(x + 18, y + 5, -1, hornFar);
        DrawHorn(x + 22, y + 6, -1, horn);
    }

    // ── Mastery: Marksman Hood (facing right) ──
    if (Program.armorHelmet != null && Program.armorHelmet == "Marksman Hood")
    {
        Color hood  = new Color((byte)120,(byte)80,(byte)40,(byte)255);
        Color hoodD = new Color((byte)90,(byte)60,(byte)30,(byte)255);
        // hood wraps head, mirrored profile
        Raylib.DrawCircle(x + 22, y + 12, 13, hood);
        // face opening
        Raylib.DrawCircle(x + 24, y + 14, 9, SkinColor);
        // brow ridge
        Raylib.DrawRectangle(x + 18, y + 6, 16, 4, hoodD);
        // feather sweeping back (left side)
        Color feather = new Color((byte)220,(byte)150,(byte)50,(byte)255);
        Raylib.DrawLineEx(new Vector2(x + 12, y + 4), new Vector2(x + 2, y - 6), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 12, y + 4), new Vector2(x + 4, y - 10), 2, feather);
        Raylib.DrawLineEx(new Vector2(x + 12, y + 4), new Vector2(x, y - 3), 2, feather);
    }

    if (WearingClassHat) DrawClassHat(x + 22, y - 2, +1);

    DrawShieldRight(x, y);   // already a standalone method — reused as-is
}

Texture2D GetHeldSheet(string item)
{
    if (item == null) return default;
    string key =
        item.Contains("Pickaxe")                          ? "held_pickaxe"
      : item.Contains("Axe") && !item.Contains("War Axe") ? "held_axe"
      : item.Contains("Watering")                         ? "held_wateringcan"
      : item.Contains("Spade")                            ? "held_spade"
      : item.Contains("Sword")                            ? "held_sword"
      : item.Contains("Dagger")                           ? "held_dagger"
      : item.Contains("Rod")                              ? "held_fishingrod"   
      : item.Contains("Hammer")                           ? "held_hammer"       
      : item.Contains("Scimitar")                         ? "held_scimitar" 
      : item.Contains("Staff")                            ? "held_staff"
      : null;
    return (key != null && AssetManager.Has(key)) ? AssetManager.Get(key) : default;
}

// row = -1 means "no action anim for this item"
(int row, int frames) GetActionAnim(string item)
{
    if (item == null) return (-1, 0);
    if (item.Contains("Pickaxe") ||
       (item.Contains("Axe") && !item.Contains("War Axe"))) return (PlayerCharacter.RowMining, 12);
    if (item.Contains("Sword") || item.Contains("Dagger")
     || item.Contains("Scimitar"))                          return (PlayerCharacter.RowSlash, 6);   
    if (item.Contains("Rod"))                               return (PlayerCharacter.RowShoot, 13);   
    if (item.Contains("Staff"))                             return (PlayerCharacter.RowSpellcast, 7);
    if (item.Contains("Spade") || item.Contains("Watering"))return (PlayerCharacter.RowThrust, 8);
    return (-1, 0);
}
void DrawFacingDown(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;

    // cape (behind everything)
    if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 11, y + 24, 18);
        else Raylib.DrawRectangle(x + 11, y + 24, 18, 30, cc);
    }

    // head
    Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);
    Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
    Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);
    Raylib.DrawRectangle(x + 15, y + 17, 10, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));

    // Hair — drawn after head, before helmet
if (Program.armorHelmet == null)
{
    switch (Program.playerHairStyle)
    {
        case "Mini fro":
            Raylib.DrawCircle(x + 20, y + 8, 13, Program.playerHairColor);
            Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);
            Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
            Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);
            Raylib.DrawRectangle(x + 15, y + 17, 10, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));
            break;
        case "High top":
            Raylib.DrawRectangle(x + 8, y - 9, 24, 19, Program.playerHairColor);
            Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);  
            Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
            Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);
            break;
        case "Flat top":
            Raylib.DrawRectangle(x + 8, y - 2, 24, 12, Program.playerHairColor);
            Raylib.DrawCircle(x + 20, y + 12, 12, SkinColor);  
            Raylib.DrawCircle(x + 15, y + 11, 2, Color.Black);
            Raylib.DrawCircle(x + 25, y + 11, 2, Color.Black);
            break;
        case "Mohawk":
            Raylib.DrawRectangle(x + 17, y - 8, 6, 14, Program.playerHairColor);
            break;
        case "Bald":
            // no hair drawn
            break;
    }
}

    // Facial hair
if (Program.armorHelmet == null)
{
Color fc = Program.playerFacialHairColor;
switch (Program.playerFacialHair)
{
    case "Stubble":
        Raylib.DrawRectangle(x + 13, y + 17, 14, 4, new Color(fc.R, fc.G, fc.B, (byte)140)); // semi-transparent stubble
        break;
    case "Moustache":
        Raylib.DrawRectangle(x + 13, y + 16, 14, 3, fc);
        break;
    case "Goatee":
        Raylib.DrawRectangle(x + 13, y + 16, 14, 3, fc);  // moustache
        Raylib.DrawRectangle(x + 16, y + 19, 8,  5, fc);  // chin strip
        break;
    case "Full Beard":
        Raylib.DrawRectangle(x + 11, y + 16, 18, 3, fc);  // moustache
        Raylib.DrawRectangle(x + 10, y + 19, 20, 6, fc);  // full jaw
        break;
}
}

    //Mouth
    Raylib.DrawRectangle(x + 15, y + 17, 10, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));



    // body/shirt
    Raylib.DrawRectangle(x + 10, y + 24, 20, 30, ShirtColor);

    // body armor over shirt
    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 24, 20, 30, col);
        Raylib.DrawRectangle(x + 13, y + 28, 14, 10, new Color((byte)Math.Max(0,col.R-25),(byte)Math.Max(0,col.G-25),(byte)Math.Max(0,col.B-25),(byte)255));
        if (Program.armorBody.EndsWith("Top"))          // mage: gold center trim
            Raylib.DrawRectangle(x + 18, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))   // ranger: leather belt
            Raylib.DrawRectangle(x + 10, y + 44, 20, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    // arms
    Raylib.DrawRectangle(x + 2,  y + 26 + armSwing, 8, 18, SkinColor);
    Raylib.DrawRectangle(x + 30, y + 26 - armSwing, 8, 18, SkinColor);

    // gloves
    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 2,  y + 38 + armSwing, 8, 8, col);
        Raylib.DrawRectangle(x + 30, y + 38 - armSwing, 8, 8, col);
    }

    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 10, y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 10, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 22, y + 54, 8, 12, PantsColor);
    }

    // leg armor
        if (Program.armorLegs != null)
        {
            Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
            if (isMoving)
            {
                if (walkFrame)
                {
                    Raylib.DrawRectangle(x + 10, y + 54,     8, 10, col);
                    Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 10, col);
                }
                else
                {
                    Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 10, col);
                    Raylib.DrawRectangle(x + 22, y + 54,     8, 10, col);
                }
            }
            else
            {
                Raylib.DrawRectangle(x + 10, y + 54, 8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54, 8, 10, col);
            }
        }

    // boots — follow the walk cycle
    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 9,  y + 63,     10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63 - 6, 10, 7, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 9,  y + 63 - 6, 10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63,     10, 7, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 9,  y + 63, 10, 7, bc);
            Raylib.DrawRectangle(x + 21, y + 63, 10, 7, bc);
        }
    }

 if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);      // Down/Up; Left: x+11 w16, Right: x+13 w16
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }


if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
{
    Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));

    // dome over the top of the head
    Raylib.DrawCircle(x + 20, y + 8, 13, hc);
    // square off the top so it sits like a helm
    Raylib.DrawRectangle(x + 7, y + 2, 26, 8, hc);

    // side cheek guards down the sides of the face
    Raylib.DrawRectangle(x + 7,  y + 8, 6, 10, hc);   // left guard
    Raylib.DrawRectangle(x + 27, y + 8, 6, 10, hc);   // right guard

    // brow brim across the forehead (just above the eyes)
    Raylib.DrawRectangle(x + 7, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

    // spike on top — skip for Mystical and Magic
    if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
    {
        Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
        Raylib.DrawTriangle(
            new Vector2(x + 17, y + 1),
            new Vector2(x + 23, y + 1),
            new Vector2(x + 20, y - 7),
            spikeCol);
    }
}

// Mystical helmet 
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
{
    Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);

    // LEFT horn — segments overlap and step out/up
    Raylib.DrawRectangle(x + 4, y + 2, 6, 5, horn);        // base
    Raylib.DrawRectangle(x + 1, y - 2, 6, 5, horn);        // mid (overlaps base)
    Raylib.DrawTriangle(                                     // tip
        new Vector2(x + 1, y - 2),
        new Vector2(x + 6, y - 2),
        new Vector2(x + 2, y - 11),
        horn);

    // RIGHT horn
    Raylib.DrawRectangle(x + 30, y + 2, 6, 5, horn);
    Raylib.DrawRectangle(x + 33, y - 2, 6, 5, horn);
    Raylib.DrawTriangle(
        new Vector2(x + 33, y - 2),
        new Vector2(x + 38, y - 2),
        new Vector2(x + 37, y - 11),
        horn);
}

// infernal helmet horns — smooth curved horns from angled quads (FacingDown test)
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
{
    Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);

    DrawHorn(x + 12, y + 6, -1, horn);   // left horn (sweeps left, curls in)
    DrawHorn(x + 28, y + 6, +1, horn);   // right horn (mirrored)
}
if (WearingClassHat) DrawClassHat(x + 20, y, 0);


    // Shield
    if (Program.armorShield != null && !Program.IsTwoHandedWeapon(Program.armorWeapon))
    {
        Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
        if (Program.armorShield.EndsWith("Book")) DrawBookOffhand(x + 4, y + 31 + armSwing);
        else Raylib.DrawRectangle(x + 4, y + 30 + armSwing, 8, 16, col);
    }
}

void DrawFacingUp(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 4 : -4) : 0;

    // head (back, darker)
    Raylib.DrawCircle(x + 20, y + 12, 12,
        new Color((byte)Math.Max(0,SkinColor.R-10),(byte)Math.Max(0,SkinColor.G-10),(byte)Math.Max(0,SkinColor.B-10),(byte)255));
    
    if (Program.armorHelmet == null)
{
    switch (Program.playerHairStyle)
    {
        case "Mini fro":
            Raylib.DrawCircle(x + 20, y + 8, 13, Program.playerHairColor);
       //     Raylib.DrawCircle(x + 20, y + 12, 12,
       // new Color((byte)Math.Max(0,SkinColor.R-10),(byte)Math.Max(0,SkinColor.G-10),(byte)Math.Max(0,SkinColor.B-10),(byte)255));
            break;
        case "High top":
            Raylib.DrawRectangle(x + 8, y - 9, 24, 23, Program.playerHairColor);
            break;
        case "Flat top":
            Raylib.DrawRectangle(x + 8, y - 2, 24, 16, Program.playerHairColor);
            break;
        case "Mohawk":
            Raylib.DrawRectangle(x + 17, y - 10, 6, 16, Program.playerHairColor);
            break;
        case "Bald":
            // no hair drawn
            break;
        default:
            // no hair
            break;
    }
}
    

    // shirt
    Raylib.DrawRectangle(x + 10, y + 24, 20, 30, ShirtColor);

    // body armor
    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 24, 20, 30, col);
        if (Program.armorBody.EndsWith("Top"))          // mage: gold center trim
            Raylib.DrawRectangle(x + 18, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))   // ranger: leather belt
            Raylib.DrawRectangle(x + 10, y + 44, 20, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    // arms
    Raylib.DrawRectangle(x + 2,  y + 26 + armSwing, 8, 18, SkinColor);
    Raylib.DrawRectangle(x + 30, y + 26 - armSwing, 8, 18, SkinColor);

    // gloves
    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 2,  y + 38 + armSwing, 8, 8, col);
        Raylib.DrawRectangle(x + 30, y + 38 - armSwing, 8, 8, col);
    }

    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 10, y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 22, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 10, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 22, y + 54, 8, 12, PantsColor);
    }

    // leg armor — follows the walk cycle
    if (Program.armorLegs != null)
    {
        Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 10, y + 54,     8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54 - 6, 8, 10, col);
            }
            else
            {
                Raylib.DrawRectangle(x + 10, y + 54 - 6, 8, 10, col);
                Raylib.DrawRectangle(x + 22, y + 54,     8, 10, col);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 54, 8, 10, col);
            Raylib.DrawRectangle(x + 22, y + 54, 8, 10, col);
        }
    }

    // boots — follow the walk cycle
        if (Program.armorBoots != null)
        {
            Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
            if (isMoving)
            {
                if (walkFrame)
                {
                    Raylib.DrawRectangle(x + 9,  y + 63,     10, 7, bc);
                    Raylib.DrawRectangle(x + 21, y + 63 - 6, 10, 7, bc);
                }
                else
                {
                    Raylib.DrawRectangle(x + 9,  y + 63 - 6, 10, 7, bc);
                    Raylib.DrawRectangle(x + 21, y + 63,     10, 7, bc);
                }
            }
            else
            {
                Raylib.DrawRectangle(x + 9,  y + 63, 10, 7, bc);
                Raylib.DrawRectangle(x + 21, y + 63, 10, 7, bc);
            }
        }

if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);      // Down/Up; Left: x+11 w16, Right: x+13 w16
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }
      
if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat")) 
{
    Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));

    // dome covering the whole back of the head
    Raylib.DrawCircle(x + 20, y + 10, 13, hc);
    // square off the top
    Raylib.DrawRectangle(x + 7, y + 2, 26, 9, hc);

    // side guards wrapping down both sides
    Raylib.DrawRectangle(x + 7,  y + 10, 6, 9, hc);   // left
    Raylib.DrawRectangle(x + 27, y + 10, 6, 9, hc);   // right

    // neck guard at the bottom (back of neck)
    Raylib.DrawRectangle(x + 11, y + 16, 18, 5, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

   // spike on top
   if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
    {
    Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
    Raylib.DrawTriangle(
        new Vector2(x + 17, y + 1),
        new Vector2(x + 23, y + 1),
        new Vector2(x + 20, y - 7),
        spikeCol);
    }
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
{
    Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
    // left
    Raylib.DrawRectangle(x + 4, y + 2, 6, 5, horn);
    Raylib.DrawRectangle(x + 1, y - 2, 6, 5, horn);
    Raylib.DrawTriangle(new Vector2(x + 1, y - 2), new Vector2(x + 6, y - 2), new Vector2(x + 2, y - 11), horn);
    // right
    Raylib.DrawRectangle(x + 30, y + 2, 6, 5, horn);
    Raylib.DrawRectangle(x + 33, y - 2, 6, 5, horn);
    Raylib.DrawTriangle(new Vector2(x + 33, y - 2), new Vector2(x + 38, y - 2), new Vector2(x + 37, y - 11), horn);
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
{
    Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
    DrawHorn(x + 12, y + 6, -1, horn);   // left
    DrawHorn(x + 28, y + 6, +1, horn);   // right
}
if (WearingClassHat) DrawClassHat(x + 20, y - 1, 0);

     // cape (very visible from behind)
    if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 10, y + 22, 20);
        else Raylib.DrawRectangle(x + 10, y + 22, 20, 34, cc);
    }
}

void DrawFacingLeft(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;

        // shield (left side, moves with left arm)
    if (Program.armorShield != null && !Program.IsTwoHandedWeapon(Program.armorWeapon))
    {
        Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
        Raylib.DrawRectangle(x - 2, y + 28 + armSwing, 10, 18, col);
        Raylib.DrawCircle(x + 3, y + 37 + armSwing, 3, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }

    // cape (trails behind, to the right when facing left)
    if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 18, y + 24, 12);
        else Raylib.DrawRectangle(x + 18, y + 24, 12, 28, cc);
    }

    // head
    Raylib.DrawCircle(x + 18, y + 12, 12, SkinColor);
    Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
    Raylib.DrawRectangle(x + 8, y + 14, 4, 3,
        new Color((byte)Math.Max(0,SkinColor.R-20),(byte)Math.Max(0,SkinColor.G-20),(byte)Math.Max(0,SkinColor.B-20),(byte)255));

    // Hair — drawn after head, before helmet
if (Program.armorHelmet == null)
{
    switch (Program.playerHairStyle)
    {
        case "Mini fro":
            Raylib.DrawCircle(x + 21, y + 8, 13, Program.playerHairColor);
            Raylib.DrawCircle(x + 18, y + 12, 12, SkinColor);
            Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
            Raylib.DrawRectangle(x + 28, y - 2, 2, 20, Program.playerHairColor);
            Raylib.DrawRectangle(x + 10, y + 0 , 18, 3, Program.playerHairColor);
            break;
        case "High top":
            Raylib.DrawRectangle(x + 8, y - 9, 22, 14, Program.playerHairColor);
            Raylib.DrawRectangle(x + 20, y - 2, 10, 12, Program.playerHairColor);
            Raylib.DrawCircle(x + 18, y + 12, 11, SkinColor);
            Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
            break;
        case "Flat top":
            Raylib.DrawRectangle(x + 8, y - 2, 22, 7, Program.playerHairColor);
            Raylib.DrawRectangle(x + 20, y - 2, 10, 12, Program.playerHairColor);
            Raylib.DrawCircle(x + 18, y + 12, 11, SkinColor);
            Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
            break;
        case "Mohawk":
            Raylib.DrawRectangle(x + 23, y + 4, 7, 6, Program.playerHairColor);
            Raylib.DrawTriangle(
            new Vector2(x + 10, y + 4),
            new Vector2(x + 30, y + 4),
            new Vector2(x + 10, y - 10),
            Program.playerHairColor);
            Raylib.DrawCircle(x + 18, y + 12, 12, SkinColor);
            Raylib.DrawCircle(x + 12, y + 10, 2, Color.Black);
            Raylib.DrawRectangle(x + 8, y + 14, 4, 3,
                new Color((byte)Math.Max(0,SkinColor.R-20),(byte)Math.Max(0,SkinColor.G-20),(byte)Math.Max(0,SkinColor.B-20),(byte)255));
            break;
        case "Bald":
            // no hair drawn
            break;
    }
}



if (Program.armorHelmet == null)
{
    Color fc = Program.playerFacialHairColor;
    switch (Program.playerFacialHair)
{
    case "Stubble":
        Raylib.DrawRectangle(x + 7, y + 15, 7, 8, new Color(fc.R, fc.G, fc.B, (byte)140));
        break;
    case "Moustache":
        Raylib.DrawRectangle(x + 6, y + 12, 7, 1, fc);
        Raylib.DrawRectangle(x + 12, y + 12, 1, 4, fc);
        break;
    case "Goatee":
        Raylib.DrawRectangle(x + 6, y + 15, 9, 3, fc);
        Raylib.DrawRectangle(x + 7, y + 18, 7, 6, fc);
        break;
    case "Full Beard":
        Raylib.DrawRectangle(x + 6, y + 15, 18, 3, fc);
        Raylib.DrawRectangle(x + 7, y + 18, 12, 6, fc);
        Raylib.DrawRectangle(x + 19, y + 12, 5, 10, fc);
        break;
}
} 

//Mouth
Raylib.DrawRectangle(x + 8, y + 14, 4, 3,
        new Color((byte)Math.Max(0,SkinColor.R-20),(byte)Math.Max(0,SkinColor.G-20),(byte)Math.Max(0,SkinColor.B-20),(byte)255));

    // body
    Raylib.DrawRectangle(x + 12, y + 24, 14, 30, ShirtColor);
    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 12, y + 24, 14, 30, col);
        if (Program.armorBody.EndsWith("Top"))          // mage: gold center trim
            Raylib.DrawRectangle(x + 17, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))   // ranger: leather belt
            Raylib.DrawRectangle(x + 12, y + 44, 14, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    // arm
    Raylib.DrawRectangle(x + 10, y + 26 + armSwing, 8, 16, SkinColor);
    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 10, y + 38 + armSwing, 8, 7, col);
    }

    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 8,  y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 8,  y + 54 - 8, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 16, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 8,  y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 16, y + 54, 8, 12, PantsColor);
    }

    // leg armor
    if (Program.armorLegs != null)
{
    Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
    // match the leg positions including the lift, and make taller to cover tops
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 8,  y + 54,     8, 14, col);
            Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 14, col);
        }
        else
        {
            Raylib.DrawRectangle(x + 8,  y + 54 - 8, 8, 14, col);
            Raylib.DrawRectangle(x + 16, y + 54,     8, 14, col);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 8,  y + 54, 8, 12, col);
        Raylib.DrawRectangle(x + 16, y + 54, 8, 12, col);
    }
}

    // boots
    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 6,  y + 64, 10, 6, bc);
                Raylib.DrawRectangle(x + 14, y + 56, 10, 6, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 6,  y + 56, 10, 6, bc);
                Raylib.DrawRectangle(x + 14, y + 64, 10, 6, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 6,  y + 62, 10, 6, bc);
            Raylib.DrawRectangle(x + 14, y + 62, 10, 6, bc);
        }
    }

if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);      // Down/Up; Left: x+11 w16, Right: x+13 w16
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }


    // helmet — side profile, covers top/back, exposes lower front face
if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
{
    Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));

    // dome over the head
    Raylib.DrawCircle(x + 18, y + 8, 13, hc);
    Raylib.DrawRectangle(x + 6, y + 2, 26, 8, hc);

    // back of head guard (right side when facing left)
    Raylib.DrawRectangle(x + 24, y + 8, 6, 11, hc);

    // front cheek guard (partial, down the face — leaves mouth/chin open)
    Raylib.DrawRectangle(x + 6, y + 8, 5, 7, hc);

    // brow brim
    Raylib.DrawRectangle(x + 6, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

    // spike on top
    if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
    {
    Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
    Raylib.DrawTriangle(
        new Vector2(x + 15, y + 1),
        new Vector2(x + 21, y + 1),
        new Vector2(x + 18, y - 7),
        spikeCol);
    }
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
{
   Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
Color hornFar = new Color((byte)255, (byte)225, (byte)0, (byte)255);

// FAR horn (other side) — tip peeking, behind
Raylib.DrawTriangle(
    new Vector2(x + 24, y - 2),
    new Vector2(x + 28, y - 2),
    new Vector2(x + 38, y - 15),
    hornFar);

// NEAR horn — full horn curving back-right
Raylib.DrawRectangle(x + 18, y, 6, 5, horn);
Raylib.DrawRectangle(x + 22, y - 4, 6, 5, horn);
Raylib.DrawTriangle(
    new Vector2(x + 22, y - 4),
    new Vector2(x + 29, y - 4),
    new Vector2(x + 33, y - 14),
    horn);
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
{
    Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
    Color hornFar = new Color((byte)22,(byte)18,(byte)18,(byte)255);

    // far horn (other side of head) — both sweep back-right
    DrawHorn(x + 22, y + 5, +1, hornFar);
    // near horn
    DrawHorn(x + 18, y + 6, +1, horn);
}
if (WearingClassHat) DrawClassHat(x + 18, y - 2, -1); 
}

void DrawFacingRight(int x, int y)
{
    int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;

    // cape (trails behind, to the left when facing right)
    if (Program.armorCape != null)
    {
        Color cc = ArmorColor(Program.armorCape, new Color((byte)140,(byte)60,(byte)60,(byte)255));
        if (Program.armorCape.EndsWith("Quiver")) DrawQuiver(x + 10, y + 24, 12);
        else Raylib.DrawRectangle(x + 10, y + 24, 12, 28, cc);
    }

    // head
    Raylib.DrawCircle(x + 22, y + 12, 12, SkinColor);
    Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);

    // Hair — drawn after head, before helmet
if (Program.armorHelmet == null)
{
    switch (Program.playerHairStyle)
    {
        case "Mini fro":
            Raylib.DrawCircle(x + 19, y + 8, 13, Program.playerHairColor);
            Raylib.DrawCircle(x + 22, y + 12, 12, SkinColor);
            Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);
            Raylib.DrawRectangle(x + 10, y - 2, 2, 20, Program.playerHairColor);
            Raylib.DrawRectangle(x + 11, y + 0 , 18, 3, Program.playerHairColor);
            break;
        case "High top":
            Raylib.DrawRectangle(x + 9, y - 9, 22, 14, Program.playerHairColor);
            Raylib.DrawRectangle(x + 9, y - 2, 10, 12, Program.playerHairColor);
            Raylib.DrawCircle(x + 22, y + 12, 11, SkinColor);
            Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);
            break;
        case "Flat top":
            Raylib.DrawRectangle(x + 10, y - 2, 22, 7, Program.playerHairColor);
            Raylib.DrawRectangle(x + 10, y - 2, 10, 12, Program.playerHairColor);
            Raylib.DrawCircle(x + 22, y + 12, 11, SkinColor);
            Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);
            break;
        case "Mohawk":
            Raylib.DrawRectangle(x + 10, y + 4, 7, 6, Program.playerHairColor);
            Raylib.DrawTriangle(
            new Vector2(x + 10, y + 4),
            new Vector2(x + 30, y + 4),
            new Vector2(x + 30, y - 10),
            Program.playerHairColor);
            Raylib.DrawCircle(x + 22, y + 12, 12, SkinColor);
            Raylib.DrawCircle(x + 28, y + 10, 2, Color.Black);   
            break;
        case "Bald":
            // no hair drawn
            break;
    }
}



if (Program.armorHelmet == null)
{
    Color fc = Program.playerFacialHairColor;
    switch (Program.playerFacialHair)
{
    case "Stubble":
        Raylib.DrawRectangle(x + 26, y + 15, 7, 8, new Color(fc.R, fc.G, fc.B, (byte)140));
        break;
    case "Moustache":
        Raylib.DrawRectangle(x + 26, y + 12, 7, 1, fc);
        Raylib.DrawRectangle(x + 26, y + 12, 1, 4, fc);
        break;
    case "Goatee":
        Raylib.DrawRectangle(x + 25, y + 15, 9, 3, fc);
        Raylib.DrawRectangle(x + 26, y + 18, 7, 6, fc);
        break;
    case "Full Beard":
        Raylib.DrawRectangle(x + 16, y + 15, 18, 3, fc);
        Raylib.DrawRectangle(x + 21, y + 18, 12, 6, fc);
        Raylib.DrawRectangle(x + 16, y + 12, 5, 10, fc);
        break;
}
} 

//Mouth
Raylib.DrawRectangle(x + 28, y + 14, 4, 3,
                new Color((byte)Math.Max(0,SkinColor.R-20),(byte)Math.Max(0,SkinColor.G-20),(byte)Math.Max(0,SkinColor.B-20),(byte)255));

    // body
    Raylib.DrawRectangle(x + 14, y + 24, 14, 30, ShirtColor);
    if (Program.armorBody != null)
    {
        Color col = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 14, y + 24, 14, 30, col);
        if (Program.armorBody.EndsWith("Top"))          // mage: gold center trim
            Raylib.DrawRectangle(x + 19, y + 26, 3, 26, new Color((byte)235,(byte)200,(byte)60,(byte)255));
        else if (Program.armorBody.EndsWith("Tunic"))   // ranger: leather belt
            Raylib.DrawRectangle(x + 14, y + 44, 14, 5, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    // arm
    Raylib.DrawRectangle(x + 22, y + 26 + armSwing, 8, 16, SkinColor);
    if (Program.armorGloves != null)
    {
        Color col = ArmorColor(Program.armorGloves, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 22, y + 38 + armSwing, 8, 7, col);
    }

    // legs
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 16, y + 54,     8, 16, PantsColor);
            Raylib.DrawRectangle(x + 24, y + 54 - 8, 8, 16, PantsColor);
        }
        else
        {
            Raylib.DrawRectangle(x + 16, y + 54 - 8, 8, 16, PantsColor);
            Raylib.DrawRectangle(x + 24, y + 54,     8, 16, PantsColor);
        }
    }
    else
    {
        Raylib.DrawRectangle(x + 16, y + 54, 8, 12, PantsColor);
        Raylib.DrawRectangle(x + 24, y + 54, 8, 12, PantsColor);
    }

    if (Program.armorLegs != null)
{
    Color col = ArmorColor(Program.armorLegs, new Color((byte)90,(byte)90,(byte)100,(byte)255));
    // match the leg positions including the lift, and make taller to cover tops
    if (isMoving)
    {
        if (walkFrame)
        {
            Raylib.DrawRectangle(x + 16,  y + 54,     8, 14, col);
            Raylib.DrawRectangle(x + 24, y + 54 - 8, 8, 14, col);
        }
        else
        {
            Raylib.DrawRectangle(x + 16,  y + 54 - 8, 8, 14, col);
            Raylib.DrawRectangle(x + 24, y + 54,     8, 14, col);
        }
    }
    else
    {
        Raylib.DrawRectangle(x +16,  y + 54, 8, 12, col);
        Raylib.DrawRectangle(x + 24, y + 54, 8, 12, col);
    }
}

    // boots
    if (Program.armorBoots != null)
    {
        Color bc = ArmorColor(Program.armorBoots, new Color((byte)100,(byte)65,(byte)25,(byte)255));
        if (isMoving)
        {
            if (walkFrame)
            {
                Raylib.DrawRectangle(x + 14, y + 64, 10, 6, bc);
                Raylib.DrawRectangle(x + 22, y + 56, 10, 6, bc);
            }
            else
            {
                Raylib.DrawRectangle(x + 14, y + 56, 10, 6, bc);
                Raylib.DrawRectangle(x + 22, y + 64, 10, 6, bc);
            }
        }
        else
        {
            Raylib.DrawRectangle(x + 14, y + 62, 10, 6, bc);
            Raylib.DrawRectangle(x + 22, y + 62, 10, 6, bc);
        }
    }

if (Program.armorBody != null && Program.armorBody.EndsWith("Top"))
    {
        Color rc = ArmorColor(Program.armorBody, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 9, y + 50, 22, 14, rc);      // Down/Up; Left: x+11 w16, Right: x+13 w16
        Raylib.DrawRectangle(x + 9, y + 62, 22, 3, new Color((byte)Math.Max(0,rc.R-35),(byte)Math.Max(0,rc.G-35),(byte)Math.Max(0,rc.B-35),(byte)255));
    }


if (Program.armorHelmet != null && !Program.armorHelmet.EndsWith("Hat"))
{
    Color hc = ArmorColor(Program.armorHelmet, new Color((byte)120,(byte)80,(byte)30,(byte)255));

    // dome over the head
    Raylib.DrawCircle(x + 22, y + 8, 13, hc);
    Raylib.DrawRectangle(x + 9, y + 2, 26, 8, hc);

    // back of head guard (left side when facing right)
    Raylib.DrawRectangle(x + 10, y + 8, 6, 11, hc);

    // front cheek guard (partial, leaves mouth/chin open)
    Raylib.DrawRectangle(x + 29, y + 8, 5, 7, hc);

    // brow brim
    Raylib.DrawRectangle(x + 9, y + 7, 26, 4, new Color((byte)Math.Max(0,hc.R-25),(byte)Math.Max(0,hc.G-25),(byte)Math.Max(0,hc.B-25),(byte)255));

    // spike on top
    if (!Program.armorHelmet.Contains("Mystical") && !Program.armorHelmet.Contains("Magic") && !Program.armorHelmet.Contains("Infernal"))
    {
    Color spikeCol = new Color((byte)Math.Min(255,hc.R+30),(byte)Math.Min(255,hc.G+30),(byte)Math.Min(255,hc.B+30),(byte)255);
    Raylib.DrawTriangle(
        new Vector2(x + 19, y + 1),
        new Vector2(x + 25, y + 1),
        new Vector2(x + 22, y - 7),
        spikeCol);
    }
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Mystical"))
{
    Color horn = new Color((byte)255, (byte)215, (byte)0, (byte)255);
    Color hornFar = new Color((byte)255, (byte)225, (byte)0, (byte)255);

    // FAR horn (other side) — tip peeking, behind
    Raylib.DrawTriangle(
        new Vector2(x + 12, y - 2),
        new Vector2(x + 17, y - 2),
        new Vector2(x + 5, y - 15),
        hornFar);

    // NEAR horn — full horn curving back-left
    Raylib.DrawRectangle(x + 16, y, 6, 5, horn);
    Raylib.DrawRectangle(x + 12, y - 4, 6, 5, horn);
    Raylib.DrawTriangle(
        new Vector2(x + 12, y - 4),
        new Vector2(x + 18, y - 4),
        new Vector2(x + 9, y - 14),
        horn);
}
if (Program.armorHelmet != null && Program.armorHelmet.Contains("Infernal"))
{
    Color horn = new Color((byte)30,(byte)25,(byte)25,(byte)255);
    Color hornFar = new Color((byte)22,(byte)18,(byte)18,(byte)255);

    // far horn (other side) — both sweep back-left
    DrawHorn(x + 18, y + 5, -1, hornFar);
    // near horn
    DrawHorn(x + 22, y + 6, -1, horn);
}
if (WearingClassHat) DrawClassHat(x + 22, y - 2, +1); 
}
void DrawShieldRight(int x, int y)
{
    if (Program.armorShield != null && !Program.IsTwoHandedWeapon(Program.armorWeapon))
    {
        int armSwing = isMoving ? (walkFrame ? 6 : -2) : 0;
        Color col = ArmorColor(Program.armorShield, new Color((byte)100,(byte)80,(byte)40,(byte)255));
        if (Program.armorShield.EndsWith("Book")) { DrawBookOffhand(x + 28, y + 31 + armSwing); return; }
        Raylib.DrawRectangle(x + 28, y + 30 + armSwing, 10, 18, col);
        Raylib.DrawCircle(x + 33, y + 39 + armSwing, 3, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
}
void DrawArmorAura(int x, int y)
{
    bool hasInfernal = HasMaterial("Infernal");
    bool hasMagic = HasMaterial("Magic");

    int cx = x + 20;   // player center
    int cy = y + 35;

    if (hasInfernal)
{
    float t = (float)Raylib.GetTime();

    // flames licking up from the body
    for (int i = 0; i < 10; i++)
    {
        float phase = t * 3f + i * 0.9f;
        float baseX = cx - 18 + (i * 4);                  // spread across the body
        float flameH = 12 + MathF.Sin(phase * 2f) * 8f;   // flame height varies
        float sway = MathF.Sin(phase + i) * 3f;
        float fx = baseX + sway;
        float fy = cy + 18 - ((phase * 14f) % 40f);        // rise and loop

        float life = (MathF.Sin(phase) + 1f) / 2f;
        // flame color: bright yellow core fading to red tips
        byte r = 255;
        byte g = (byte)(120 + life * 130);
        byte b = (byte)(life * 30);
        byte a = (byte)(180 * life);
        Raylib.DrawCircle((int)fx, (int)fy, 2.5f * life + 1f, new Color(r, g, b, a));
    }

    // occasional rising embers
    for (int i = 0; i < 4; i++)
    {
        float phase = t * 2f + i * 1.7f;
        float ex = cx + MathF.Sin(phase * 1.5f + i) * 20f;
        float ey = cy + 15 - ((phase * 22f) % 55f);
        float fade = 1f - ((phase * 22f) % 55f) / 55f;
        Raylib.DrawCircle((int)ex, (int)ey, 1.5f, new Color((byte)255,(byte)160,(byte)40,(byte)(fade * 200)));
    }
}

    if (hasMagic)
    {
        float t = (float)Raylib.GetTime();
        // orbiting sparkles
        for (int i = 0; i < 5; i++)
        {
            float ang = t * 2f + i * (MathF.PI * 2f / 5f);
            float ex = cx + MathF.Cos(ang) * 26f;
            float ey = cy + MathF.Sin(ang) * 20f;
            float twinkle = (MathF.Sin(t * 5f + i) + 1f) / 2f;
            byte a = (byte)(100 + twinkle * 130);
            Raylib.DrawCircle((int)ex, (int)ey, 1.5f + twinkle * 2f,
                new Color((byte)(150 + (byte)(twinkle*100)), (byte)80, (byte)255, a));
        }
        // pulsing aura ring
        float pulse = (MathF.Sin(t * 2.5f) + 1f) / 2f;
        Raylib.DrawCircleLines(cx, cy, 28 + pulse * 5f, new Color((byte)150,(byte)80,(byte)255,(byte)70));
    }
    if (HasMaterial("Mystical"))
{
    float t = (float)Raylib.GetTime();

    // orbiting stars
    for (int i = 0; i < 6; i++)
    {
        float ang = t * 1.5f + i * (MathF.PI * 2f / 6f);
        float ex = cx + MathF.Cos(ang) * 28f;
        float ey = cy + MathF.Sin(ang) * 22f;
        float twinkle = (MathF.Sin(t * 4f + i) + 1f) / 2f;
        byte a = (byte)(120 + twinkle * 135);
        DrawStar((int)ex, (int)ey, 4f + twinkle * 2f, new Color((byte)255,(byte)255,(byte)240, a));
    }

    // soft white halo
    float pulse = (MathF.Sin(t * 2f) + 1f) / 2f;
    Raylib.DrawCircleLines(cx, cy, 30 + pulse * 4f, new Color((byte)255,(byte)255,(byte)255,(byte)70));
}
}

bool HasMaterial(string mat)
{
    return (Program.armorHelmet != null && Program.armorHelmet.Contains(mat))
        || (Program.armorBody   != null && Program.armorBody.Contains(mat))
        || (Program.armorLegs   != null && Program.armorLegs.Contains(mat))
        || (Program.armorBoots  != null && Program.armorBoots.Contains(mat))
        || (Program.armorGloves != null && Program.armorGloves.Contains(mat))
        || (Program.armorCape   != null && Program.armorCape.Contains(mat))
        || (Program.armorShield != null && Program.armorShield.Contains(mat));
}

void DrawHeldItem(int x, int y, string tool)
{
    if (tool == null) return;

    bool isWeapon = tool.Contains("Sword") || tool.Contains("Stick")
                 || tool.Contains("Bow") || tool.Contains("Crossbow")
                 || tool.Contains("War Axe") || tool.Contains("Great Sword")
                 || tool.Contains("Staff");
    bool isChopTool = (tool.Contains("Axe") && !tool.Contains("War Axe"))
                   || tool.Contains("Pickaxe");

    float swing = GetSwingAngle();

    // pivot point = the player's hand
    float pivotX = x + 35;
    float pivotY = y + 40;

    if (Facing == FacingDirection.Left)  pivotX = x + 15;
    if (Facing == FacingDirection.Right) pivotX = x + 25;
    if (Facing == FacingDirection.Up)    pivotY = y + 26;
    if (Facing == FacingDirection.Down)  pivotY = y + 38;

    bool doRotate = isSwinging && (isWeapon || isChopTool);
    bool sideways = (Facing == FacingDirection.Left || Facing == FacingDirection.Right);

    if (doRotate && sideways)
    {
        // rotation swing for left/right (sideways arc)
        float angle = swing;
        if (isChopTool) angle = swing * 0.5f;
        if (Facing == FacingDirection.Left) angle = -angle;

        Rlgl.PushMatrix();
        Rlgl.Translatef(pivotX, pivotY, 0);
        Rlgl.Rotatef(angle, 0, 0, 1);
        Rlgl.Translatef(-pivotX, -pivotY, 0);
        DrawToolShapes(x, y, tool);
        Rlgl.PopMatrix();
    }
    else if (doRotate && !sideways)
{
    // straight thrust toward/away from camera — no rotation
    float t = swing / 90f;                          // 0..1..0
    float thrust = MathF.Sin(t * MathF.PI) * 24f;  // how far it stabs
    int dir = (Facing == FacingDirection.Down) ? 1 : -1;   // down = toward camera

    // optional: grow slightly when thrusting toward camera (facing down)
    float scale = (Facing == FacingDirection.Down)
    ? 1f + MathF.Sin(t * MathF.PI) * 0.35f          // grows more dramatically
    : 1f - MathF.Sin(t * MathF.PI) * 0.2f;    // shrink slightly going away (up)

    Rlgl.PushMatrix();
    Rlgl.Translatef(pivotX, pivotY, 0);
    Rlgl.Scalef(scale, scale, 1);
    Rlgl.Translatef(-pivotX, -pivotY, 0);
    Rlgl.Translatef(0, thrust * dir, 0);            // straight push along Y
    DrawToolShapes(x, y, tool);
    Rlgl.PopMatrix();
}
    else
    {
        // not swinging — draw normally
        DrawToolShapes(x, y, tool);
    }
}
void DrawStaffShape(int x, int y, string tool, int armSwing)
{
    Color orb = Program.GetStaffColor(tool);
    bool great = tool.Contains("Great");
    int shaftCol = great ? 100 : 110;

    // anchor points per facing
    Vector2 baseP, topP, orbP;
    switch (Facing)
    {
        case FacingDirection.Right:
            baseP = new Vector2(x + 25, y + 44 + armSwing);
            topP  = new Vector2(x + 45, y + 18 + armSwing);
            break;
        case FacingDirection.Left:
            baseP = new Vector2(x + 15, y + 44 + armSwing);
            topP  = new Vector2(x - 5,  y + 18 + armSwing);
            break;
        case FacingDirection.Down:
            baseP = new Vector2(x + 35, y + 40 + armSwing);
            topP  = new Vector2(x + 35, y + 12 + armSwing);
            break;
        default: // Up
            baseP = new Vector2(x + 5,  y + 28 + armSwing);
            topP  = new Vector2(x + 5,  y + 2  + armSwing);
            break;
    }
    orbP = topP;

    Raylib.DrawLineEx(baseP, topP, great ? 5 : 4, new Color((byte)shaftCol,(byte)80,(byte)45,(byte)255));
    int r = great ? 8 : 6;
    Raylib.DrawCircle((int)orbP.X, (int)orbP.Y, r + 3, new Color(orb.R, orb.G, orb.B, (byte)110));
    Raylib.DrawCircle((int)orbP.X, (int)orbP.Y, r, orb);
    Raylib.DrawCircle((int)orbP.X - 2, (int)orbP.Y - 2, 2, Color.White);
}
void DrawToolShapes(int x, int y, string tool)
{
    switch (Facing)
    {
         case FacingDirection.Right:
    int armSwingRight = isMoving ? (walkFrame ? 3 : -3) : 0;

    if (tool.Contains("Great Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 38, y + 40 + armSwingRight), 6, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 38, y + 40 + armSwingRight), new Vector2(x + 60, y + 40 + armSwingRight), 6, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 60, y + 43 + armSwingRight),
            new Vector2(x + 66, y + 40 + armSwingRight),
            new Vector2(x + 60, y + 37 + armSwingRight),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 37, y + 34 + armSwingRight), new Vector2(x + 37, y + 46 + armSwingRight), 6, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("War Axe"))
    {
        Color metal = WeaponColor(tool, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 50, y + 40 + armSwingRight), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x + 46, y + 30 + armSwingRight),
            new Vector2(x + 46, y + 50 + armSwingRight),
            new Vector2(x + 60, y + 40 + armSwingRight),
            metal);
        Raylib.DrawTriangle(
            new Vector2(x + 46, y + 36 + armSwingRight),
            new Vector2(x + 38, y + 40 + armSwingRight),
            new Vector2(x + 46, y + 44 + armSwingRight),
            metal);
    }

    else if (tool.Contains("Staff"))
    {
        Color orb = Program.GetStaffColor(tool);
        bool great = tool.Contains("Great");
        int len = great ? 62 : 55;
        int r = great ? 8 : 5;
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + len, y + 40 + armSwingRight), great ? 6 : 5, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(x + len + 3, y + 40 + armSwingRight, r + 2, new Color(orb.R, orb.G, orb.B, (byte)110));
        Raylib.DrawCircle(x + len + 3, y + 40 + armSwingRight, r, orb);
    }

    else if (tool.Contains("Sword"))   // plain sword (Great Sword already handled above)
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 35, y + 40 + armSwingRight), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 40 + armSwingRight), new Vector2(x + 48, y + 40 + armSwingRight), 4, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 48, y + 42 + armSwingRight),
            new Vector2(x + 53, y + 40 + armSwingRight),
            new Vector2(x + 48, y + 38 + armSwingRight),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 34, y + 36 + armSwingRight), new Vector2(x + 34, y + 44 + armSwingRight), 5, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("Stick"))
    {
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    }
    else if (tool.Contains("Bow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)140,(byte)90,(byte)40,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 40, y + 30 + armSwingRight), new Vector2(x + 40, y + 50 + armSwingRight), 3, limb);
        Raylib.DrawLineEx(new Vector2(x + 40, y + 30 + armSwingRight), new Vector2(x + 40, y + 50 + armSwingRight), 1, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    }
    else if (tool.Contains("Crossbow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 28, y + 40 + armSwingRight), new Vector2(x + 48, y + 40 + armSwingRight), 4, limb);
        Raylib.DrawLineEx(new Vector2(x + 40, y + 32 + armSwingRight), new Vector2(x + 40, y + 48 + armSwingRight), 4, limb);
        Raylib.DrawTriangle(
            new Vector2(x + 52, y + 40 + armSwingRight),
            new Vector2(x + 48, y + 38 + armSwingRight),
            new Vector2(x + 48, y + 42 + armSwingRight),
            Color.Gray);
    }
    else if (tool.Contains("Pickaxe"))
    {
            Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5,
                new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLineEx(new Vector2(x + 45, y + 34 + armSwingRight), new Vector2(x + 45, y + 44 + armSwingRight), 3,
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(x + 46, y + 44 + armSwingRight),
                new Vector2(x + 43, y + 44 + armSwingRight),
                new Vector2(x + 37, y + 52 + armSwingRight),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(x + 43, y + 34 + armSwingRight),
                new Vector2(x + 46, y + 34 + armSwingRight),
                new Vector2(x + 40, y + 30 + armSwingRight),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }

    else if (tool.Contains("Axe"))   // plain axe (War Axe already handled above)
    {
        Raylib.DrawLineEx(new Vector2(x + 25, y + 40 + armSwingRight), new Vector2(x + 45, y + 40 + armSwingRight), 5,
            new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 41, y + 40 + armSwingRight), new Vector2(x + 41, y + 48 + armSwingRight), 6,
            new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Rod"))
    {
        Raylib.DrawLineEx(new Vector2(x + 26, y + 40 + armSwingRight), new Vector2(x + 46, y + 26 + armSwingRight), 4,
            new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 46, y + 26 + armSwingRight), new Vector2(x + 62, y + 40 + armSwingRight), 2,
            new Color((byte)200,(byte)200,(byte)200,(byte)255));
        Raylib.DrawCircle(x + 62, y + 40 + armSwingRight, 3,
            new Color((byte)100,(byte)180,(byte)220,(byte)255));
    }
    else if (tool.Contains("Torch"))
    {
        Raylib.DrawLineEx(new Vector2(x + 36, y + 30 + armSwingRight), new Vector2(x + 50, y + 18 + armSwingRight), 5,
            new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 50, y + 16 + armSwingRight, 5,
            new Color((byte)255,(byte)180,(byte)0,(byte)255));
        Raylib.DrawCircle(x + 50, y + 16 + armSwingRight, 9,
            new Color((byte)255,(byte)100,(byte)0,(byte)60));
    }
    break;

        case FacingDirection.Left:
    int armSwingLeft = isMoving ? (walkFrame ? 3 : -3) : 0;

    if (tool.Contains("Great Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x + 2, y + 40 + armSwingLeft), 6, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 2, y + 40 + armSwingLeft), new Vector2(x - 20, y + 40 + armSwingLeft), 6, blade);
        Raylib.DrawTriangle(
            new Vector2(x - 20, y + 37 + armSwingLeft),
            new Vector2(x - 26, y + 40 + armSwingLeft),
            new Vector2(x - 20, y + 43 + armSwingLeft),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 3, y + 34 + armSwingLeft), new Vector2(x + 3, y + 46 + armSwingLeft), 6, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("War Axe"))
    {
        Color metal = WeaponColor(tool, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 10, y + 40 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x - 6, y + 50 + armSwingLeft),
            new Vector2(x - 6, y + 30 + armSwingLeft),
            new Vector2(x - 20, y + 40 + armSwingLeft),
            metal);
        Raylib.DrawTriangle(
            new Vector2(x - 6, y + 44 + armSwingLeft),
            new Vector2(x + 2, y + 40 + armSwingLeft),
            new Vector2(x - 6, y + 36 + armSwingLeft),
            metal);
    }

    else if (tool.Contains("Staff"))
    {
        Color orb = Program.GetStaffColor(tool);
        bool great = tool.Contains("Great");
        int len = great ? 22 : 15;
        int r = great ? 8 : 5;
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - len, y + 40 + armSwingLeft), great ? 6 : 5, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(x - len - 3, y + 40 + armSwingLeft, r + 2, new Color(orb.R, orb.G, orb.B, (byte)110));
        Raylib.DrawCircle(x - len - 3, y + 40 + armSwingLeft, r, orb);
    }

    else if (tool.Contains("Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x + 5, y + 40 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 40 + armSwingLeft), new Vector2(x - 8, y + 40 + armSwingLeft), 4, blade);
        Raylib.DrawTriangle(
            new Vector2(x - 8, y + 38 + armSwingLeft),
            new Vector2(x - 13, y + 40 + armSwingLeft),
            new Vector2(x - 8, y + 42 + armSwingLeft),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 6, y + 36 + armSwingLeft), new Vector2(x + 6, y + 44 + armSwingLeft), 5, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("Stick"))
    {
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    }
    else if (tool.Contains("Bow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)140,(byte)90,(byte)40,(byte)255));
        Raylib.DrawLineEx(new Vector2(x, y + 30 + armSwingLeft), new Vector2(x, y + 50 + armSwingLeft), 3, limb);
        Raylib.DrawLineEx(new Vector2(x, y + 30 + armSwingLeft), new Vector2(x, y + 50 + armSwingLeft), 1, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    }
    else if (tool.Contains("Crossbow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 12, y + 40 + armSwingLeft), new Vector2(x - 8, y + 40 + armSwingLeft), 4, limb);
        Raylib.DrawLineEx(new Vector2(x, y + 32 + armSwingLeft), new Vector2(x, y + 48 + armSwingLeft), 4, limb);
        Raylib.DrawTriangle(
            new Vector2(x - 12, y + 40 + armSwingLeft),
            new Vector2(x - 8, y + 42 + armSwingLeft),
            new Vector2(x - 8, y + 38 + armSwingLeft),
            Color.Gray);
    }
    else if (tool.Contains("Pickaxe"))
    {
        
            Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
            Raylib.DrawLineEx(new Vector2(x - 5, y + 34 + armSwingLeft), new Vector2(x - 5, y + 44 + armSwingLeft), 3, new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(x - 3, y + 44 + armSwingLeft),
                new Vector2(x - 6, y + 44 + armSwingLeft),
                new Vector2(x + 3, y + 52 + armSwingLeft),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
            Raylib.DrawTriangle(
                new Vector2(x - 3, y + 34 + armSwingLeft),
                new Vector2(x, y + 30 + armSwingLeft),
                new Vector2(x - 6, y + 34 + armSwingLeft),
                new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }

    else if (tool.Contains("Axe"))
    {
        Raylib.DrawLineEx(new Vector2(x + 15, y + 40 + armSwingLeft), new Vector2(x - 5, y + 40 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x - 1, y + 40 + armSwingLeft), new Vector2(x - 1, y + 48 + armSwingLeft), 6, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Rod"))
    {
        Raylib.DrawLineEx(new Vector2(x + 14, y + 40 + armSwingLeft), new Vector2(x - 6, y + 26 + armSwingLeft), 4, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x - 6, y + 26 + armSwingLeft), new Vector2(x - 22, y + 40 + armSwingLeft), 2, new Color((byte)200,(byte)200,(byte)200,(byte)255));
        Raylib.DrawCircle(x - 22, y + 40 + armSwingLeft, 3, new Color((byte)100,(byte)180,(byte)220,(byte)255));
    }
    else if (tool.Contains("Torch"))
    {
        Raylib.DrawLineEx(new Vector2(x + 4, y + 30 + armSwingLeft), new Vector2(x - 10, y + 18 + armSwingLeft), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x - 10, y + 16 + armSwingLeft, 5, new Color((byte)255,(byte)180,(byte)0,(byte)255));
        Raylib.DrawCircle(x - 10, y + 16 + armSwingLeft, 9, new Color((byte)255,(byte)100,(byte)0,(byte)60));
    }
    break;

        case FacingDirection.Down:
    int armSwingDown = isMoving ? (walkFrame ? -3 : 3) : 0;

    if (tool.Contains("Great Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 25 + armSwingDown), 6, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 25 + armSwingDown), new Vector2(x + 35, y + 2 + armSwingDown), 6, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 32, y + 2 + armSwingDown),
            new Vector2(x + 35, y - 5 + armSwingDown),
            new Vector2(x + 38, y + 2 + armSwingDown),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 29, y + 25 + armSwingDown), new Vector2(x + 41, y + 25 + armSwingDown), 6, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("War Axe"))
    {
        Color metal = WeaponColor(tool, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x + 25, y + 18 + armSwingDown),
            new Vector2(x + 45, y + 18 + armSwingDown),
            new Vector2(x + 35, y + 30 + armSwingDown),
            metal);
        Raylib.DrawTriangle(
            new Vector2(x + 31, y + 18 + armSwingDown),
            new Vector2(x + 39, y + 18 + armSwingDown),
            new Vector2(x + 35, y + 10 + armSwingDown),
            metal);
    }

    else if (tool.Contains("Staff"))
    {
        Color orb = Program.GetStaffColor(tool);
        bool great = tool.Contains("Great");
        int top = great ? -6 : 2;
        int r = great ? 8 : 5;
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + top + armSwingDown), great ? 6 : 5, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(x + 35, y + top - 4 + armSwingDown, r + 2, new Color(orb.R, orb.G, orb.B, (byte)110));
        Raylib.DrawCircle(x + 35, y + top - 4 + armSwingDown, r, orb);
    }

    else if (tool.Contains("Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 33 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 33 + armSwingDown), new Vector2(x + 35, y + 18 + armSwingDown), 3, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 34, y + 18 + armSwingDown),
            new Vector2(x + 36, y + 18 + armSwingDown),
            new Vector2(x + 35, y + 10 + armSwingDown),
            blade);
        Raylib.DrawLineEx(new Vector2(x + 32, y + 33 + armSwingDown), new Vector2(x + 38, y + 33 + armSwingDown), 4, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("Stick"))
    {
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    }
    else if (tool.Contains("Bow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)140,(byte)90,(byte)40,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 14 + armSwingDown), new Vector2(x + 35, y + 38 + armSwingDown), 3, limb);
        Raylib.DrawLineEx(new Vector2(x + 35, y + 14 + armSwingDown), new Vector2(x + 35, y + 38 + armSwingDown), 1, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    }
    else if (tool.Contains("Crossbow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 27, y + 26 + armSwingDown), new Vector2(x + 43, y + 26 + armSwingDown), 4, limb);
        Raylib.DrawLineEx(new Vector2(x + 35, y + 18 + armSwingDown), new Vector2(x + 35, y + 34 + armSwingDown), 4, limb);
        Raylib.DrawTriangle(
            new Vector2(x + 35, y + 14 + armSwingDown),
            new Vector2(x + 33, y + 18 + armSwingDown),
            new Vector2(x + 37, y + 18 + armSwingDown),
            Color.Gray);
    }
    else if (tool.Contains("Pickaxe"))
    {
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 14 + armSwingDown), new Vector2(x + 35, y + 18 + armSwingDown), 5, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x + 37, y + 18 + armSwingDown),
            new Vector2(x + 33, y + 18 + armSwingDown),
            new Vector2(x + 35, y + 28 + armSwingDown),
            new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Axe"))
    {
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 15 + armSwingDown), new Vector2(x + 35, y + 21 + armSwingDown), 3, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Rod"))
    {
        Raylib.DrawLineEx(new Vector2(x + 35, y + 38 + armSwingDown), new Vector2(x + 35, y + 14 + armSwingDown), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 35, y + 15 + armSwingDown), new Vector2(x + 35, y + 40 + armSwingDown), 2, new Color((byte)200,(byte)200,(byte)200,(byte)255));
        Raylib.DrawCircle(x + 35, y + 40 + armSwingDown, 3, new Color((byte)100,(byte)180,(byte)220,(byte)255));
    }
    else if (tool.Contains("Torch"))
    {
        Raylib.DrawLineEx(new Vector2(x + 30, y + 28), new Vector2(x + 44, y + 16), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x + 44, y + 14, 5, new Color((byte)255,(byte)180,(byte)0,(byte)255));
        Raylib.DrawCircle(x + 44, y + 14, 9, new Color((byte)255,(byte)100,(byte)0,(byte)60));
    }
    break;

        case FacingDirection.Up:
    int armSwingUp = isMoving ? (walkFrame ? 3 : -3) : 0;

    if (tool.Contains("Great Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 12 + armSwingUp), 6, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 12 + armSwingUp), new Vector2(x + 5, y - 8 + armSwingUp), 6, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 2, y - 8 + armSwingUp),
            new Vector2(x + 5, y - 14 + armSwingUp),
            new Vector2(x + 8, y - 8 + armSwingUp),
            blade);
        Raylib.DrawLineEx(new Vector2(x - 1, y + 12 + armSwingUp), new Vector2(x + 11, y + 12 + armSwingUp), 5, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    }
    else if (tool.Contains("War Axe"))
    {
        Color metal = WeaponColor(tool, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 4 + armSwingUp), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x - 4, y + 8 + armSwingUp),
            new Vector2(x + 14, y + 8 + armSwingUp),
            new Vector2(x + 5, y + 18 + armSwingUp),
            metal);
    }

    else if (tool.Contains("Staff"))
    {
        Color orb = Program.GetStaffColor(tool);
        bool great = tool.Contains("Great");
        int top = great ? -8 : -2;
        int r = great ? 8 : 5;
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + top + armSwingUp), great ? 6 : 5, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(x + 5, y + top - 4 + armSwingUp, r + 2, new Color(orb.R, orb.G, orb.B, (byte)110));
        Raylib.DrawCircle(x + 5, y + top - 4 + armSwingUp, r, orb);
    }

    else if (tool.Contains("Sword"))
    {
        Color blade = WeaponColor(tool, new Color((byte)180,(byte)190,(byte)200,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4, blade);
        Raylib.DrawTriangle(
            new Vector2(x + 3, y + 14 + armSwingUp),
            new Vector2(x + 7, y + 14 + armSwingUp),
            new Vector2(x + 5, y + 10 + armSwingUp),
            blade);
    }
    else if (tool.Contains("Stick"))
    {
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    }
    else if (tool.Contains("Bow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)140,(byte)90,(byte)40,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 14 + armSwingUp), new Vector2(x + 5, y + 28 + armSwingUp), 3, limb);
        Raylib.DrawLineEx(new Vector2(x + 5, y + 14 + armSwingUp), new Vector2(x + 5, y + 28 + armSwingUp), 1, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    }
    else if (tool.Contains("Crossbow"))
    {
        Color limb = WeaponColor(tool, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x - 3, y + 20 + armSwingUp), new Vector2(x + 13, y + 20 + armSwingUp), 4, limb);
        Raylib.DrawLineEx(new Vector2(x + 5, y + 14 + armSwingUp), new Vector2(x + 5, y + 28 + armSwingUp), 4, limb);
        Raylib.DrawTriangle(
            new Vector2(x + 5, y + 10 + armSwingUp),
            new Vector2(x + 3, y + 14 + armSwingUp),
            new Vector2(x + 7, y + 14 + armSwingUp),
            Color.Gray);
    }
    else if (tool.Contains("Pickaxe"))
    {
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 15 + armSwingUp), 4, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 15 + armSwingUp), new Vector2(x + 5, y + 12 + armSwingUp), 4, new Color((byte)160,(byte)160,(byte)170,(byte)255));
        Raylib.DrawTriangle(
            new Vector2(x + 7, y + 15 + armSwingUp),
            new Vector2(x + 3, y + 15 + armSwingUp),
            new Vector2(x + 5, y + 22 + armSwingUp),
            new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Axe"))
    {
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 15 + armSwingUp), 4, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 5, y + 15 + armSwingUp), new Vector2(x + 5, y + 18 + armSwingUp), 4, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    }
    else if (tool.Contains("Rod"))
    {
        Raylib.DrawLineEx(new Vector2(x + 5, y + 26 + armSwingUp), new Vector2(x + 5, y + 14 + armSwingUp), 4, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawLineEx(new Vector2(x + 7, y + 14 + armSwingUp), new Vector2(x + 7, y + 26 + armSwingUp), 1, new Color((byte)200,(byte)200,(byte)200,(byte)255));
        Raylib.DrawCircle(x + 5, y + 48 + armSwingUp, 3, new Color((byte)100,(byte)180,(byte)220,(byte)255));
    }
    else if (tool.Contains("Torch"))
    {
        Raylib.DrawLineEx(new Vector2(x + 10, y + 30), new Vector2(x - 4, y + 18), 5, new Color((byte)120,(byte)80,(byte)30,(byte)255));
        Raylib.DrawCircle(x - 4, y + 16, 5, new Color((byte)255,(byte)180,(byte)0,(byte)255));
        Raylib.DrawCircle(x - 4, y + 16, 9, new Color((byte)255,(byte)100,(byte)0,(byte)60));
    }
    break;
    }
}
void DrawHeldBasket(int x, int y)
{
    // basket appears on the player's side depending on facing
    int bx, by;
    foreach (var p in dustParticles)
    {
        byte alpha = (byte)(255 * (p.timer / 0.3f));
        float size = 4f * (p.timer / 0.3f);
        Raylib.DrawCircle((int)p.pos.X + Raylib.GetRandomValue(-4, 4),
                          (int)p.pos.Y,
                          size,
                          new Color(p.color.R, p.color.G, p.color.B, alpha));
    }
    switch (Facing)
    {
        case FacingDirection.Down:
            bx = x + 28; by = y + 28;
            // basket body
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Up:
            bx = x + 28; by = y + 28;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Left:
            bx = x - 20; by = y + 30;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
        case FacingDirection.Right:
            bx = x + 38; by = y + 30;
            Raylib.DrawRectangle(bx, by, 22, 18, new Color((byte)180,(byte)100,(byte)30,(byte)255));
            for (int i = bx + 4; i < bx + 22; i += 5)
                Raylib.DrawRectangle(i, by, 2, 18, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            Raylib.DrawRectangle(bx + 4, by - 8, 14, 10, new Color((byte)140,(byte)70,(byte)15,(byte)255));
            break;
    }
}

void DrawPushedTrolley(int x, int y)
{
    int tx, ty;
    switch (Facing)
    {
        case FacingDirection.Down:
            tx = x - 10; ty = y + 65;
            // trolley body
            Raylib.DrawRectangle(tx, ty, 60, 32, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 60, 32, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            // mesh
            for (int i = tx + 10; i < tx + 60; i += 10)
                Raylib.DrawRectangle(i, ty, 2, 32, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 8; j < ty + 32; j += 8)
                Raylib.DrawRectangle(tx, j, 60, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            // handle connecting to player
            Raylib.DrawRectangle(tx + 10, ty - 10, 40, 8, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            // wheels
            Raylib.DrawCircle(tx + 12, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 48, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Up:
            tx = x - 10; ty = y - 50;
            Raylib.DrawRectangle(tx, ty, 60, 32, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 60, 32, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 10; i < tx + 60; i += 10)
                Raylib.DrawRectangle(i, ty, 2, 32, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 8; j < ty + 32; j += 8)
                Raylib.DrawRectangle(tx, j, 60, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx + 10, ty + 32, 40, 8, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx + 12, ty - 6, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 48, ty - 6, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Left:
            tx = x - 65; ty = y + 10;
            Raylib.DrawRectangle(tx, ty, 32, 45, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 32, 45, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 8; i < tx + 32; i += 8)
                Raylib.DrawRectangle(i, ty, 2, 45, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 10; j < ty + 45; j += 10)
                Raylib.DrawRectangle(tx, j, 32, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx + 32, ty + 12, 8, 22, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx - 6, ty + 10, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx - 6, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;

        case FacingDirection.Right:
            tx = x + 40; ty = y + 10;
            Raylib.DrawRectangle(tx, ty, 32, 45, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(tx, ty, 32, 45, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            for (int i = tx + 8; i < tx + 32; i += 8)
                Raylib.DrawRectangle(i, ty, 2, 45, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            for (int j = ty + 10; j < ty + 45; j += 10)
                Raylib.DrawRectangle(tx, j, 32, 2, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            Raylib.DrawRectangle(tx - 8, ty + 12, 8, 22, new Color((byte)100,(byte)100,(byte)110,(byte)255));
            Raylib.DrawCircle(tx + 38, ty + 10, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Raylib.DrawCircle(tx + 38, ty + 38, 5, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            break;
    }
}
    }
}
