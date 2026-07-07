using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    struct FloatingText
    {
    public Vector2 Position;
    public string Text;
    public float Timer;
    public Color TextColor;
    }

    struct Splat
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Timer;
        public float MaxTimer;
        public float Radius;
        public Color SplatColor;
    }
    struct DeathFx
    {
        public Vector2 Position;
        public string Type;        // enemy type, for the silhouette
        public float Timer;
        public float MaxTimer;
        public Color TintColor;
    }

    class LootDrop
{
    public Vector2 Position;
    public string ItemType;
    public bool Collected = false;
    public int OwnerId = -1;
    public LootDrop(Vector2 pos, string itemType, int ownerId = -1)
    {
        Position = pos;
        ItemType = itemType;
        OwnerId = ownerId;
    }

    public Rectangle Bounds =>
        new Rectangle(Position.X - 10, Position.Y - 10, 20, 20);

    public void Draw()
    {
        if (Collected) return;

        Raylib.DrawCircle((int)Position.X, (int)Position.Y, 10, Color.Gold);

        switch (ItemType)
        {
            case "Bone":
                Raylib.DrawRectangle((int)Position.X - 3, (int)Position.Y - 8, 6, 16, Color.White);
                break;
            case "Fur":
                Raylib.DrawCircle((int)Position.X, (int)Position.Y, 7, new Color((byte)139, (byte)90, (byte)43, (byte)255));
                break;
            case "Stinger":
                Raylib.DrawTriangle(
                    new Vector2(Position.X, Position.Y - 10),
                    new Vector2(Position.X - 6, Position.Y + 6),
                    new Vector2(Position.X + 6, Position.Y + 6),
                    new Color((byte)180, (byte)120, (byte)0, (byte)255)
                );
                break;
            case "Bear Pelt":
                Raylib.DrawRectangle((int)Position.X - 8, (int)Position.Y - 8, 16, 16, new Color((byte)100, (byte)100, (byte)120, (byte)255));
                break;
            case "Dog Fang":
            case "Snake Fang":
            case "Croc Tooth":
            case "Shark Tooth":
                Raylib.DrawTriangle(
                    new Vector2(Position.X, Position.Y - 9),
                    new Vector2(Position.X - 4, Position.Y + 6),
                    new Vector2(Position.X + 4, Position.Y + 6),
                    Color.White);
                break;
            case "Wolf Claw":
            case "Bear Claw":
            case "Crab Claw":
            case "Eagle Talon":
                Raylib.DrawLineEx(new Vector2(Position.X - 5, Position.Y + 6), new Vector2(Position.X + 5, Position.Y - 6), 3, new Color((byte)230,(byte)230,(byte)210,(byte)255));
                break;
            case "Venom Sac":
                Raylib.DrawCircle((int)Position.X, (int)Position.Y, 7, new Color((byte)80,(byte)200,(byte)80,(byte)255));
                break;
            case "Crab Shell":
            case "Croc Scale":
            case "Lizard Scale":
                Raylib.DrawRectangle((int)Position.X - 7, (int)Position.Y - 6, 14, 12, new Color((byte)120,(byte)160,(byte)90,(byte)255));
                break;
            case "Shark Fin":
                Raylib.DrawTriangle(
                    new Vector2(Position.X, Position.Y - 9),
                    new Vector2(Position.X - 7, Position.Y + 7),
                    new Vector2(Position.X + 7, Position.Y + 7),
                    new Color((byte)150,(byte)150,(byte)170,(byte)255));
                break;
            case "Snake Skin":
                Raylib.DrawRectangle((int)Position.X - 6, (int)Position.Y - 6, 12, 12, new Color((byte)110,(byte)160,(byte)70,(byte)255));
                break;
            case "Ember Stone":
            case "Lava Core":
                Raylib.DrawCircle((int)Position.X, (int)Position.Y, 7, new Color((byte)255,(byte)100,(byte)0,(byte)255));
                break;
            case "Magma Shard":
                Raylib.DrawTriangle(
                    new Vector2(Position.X, Position.Y - 8),
                    new Vector2(Position.X - 6, Position.Y + 6),
                    new Vector2(Position.X + 6, Position.Y + 6),
                    new Color((byte)200,(byte)60,(byte)20,(byte)255));
                break;
            case "Feather":
                Raylib.DrawLineEx(new Vector2(Position.X, Position.Y - 8), new Vector2(Position.X, Position.Y + 8), 3, new Color((byte)220,(byte)220,(byte)240,(byte)255));
                break;
            case "Horn":
            case "Goat Hoof":
                Raylib.DrawRectangle((int)Position.X - 4, (int)Position.Y - 8, 8, 16, new Color((byte)200,(byte)190,(byte)160,(byte)255));
                break;
        }
    }
}
    class Enemy
{
    public Vector2 Position;
    public int Health;
    public int MaxHealth;
    public string Type;
    public bool Aggro = false;
    float aggroRange = 220f;     // distance at which it notices the player
    float deAggroRange = 600f;   // distance at which it gives up
    float chaseSpeed = 90f;
    public Color EnemyColor;
    public bool Dead = false;
    public int LastDamagerId = -1;
    float respawnTimer = 0f;
    Vector2 wanderTarget;
    float wanderTimer = 0f;
    float speed = 40f;

    // ── COMBAT (humanoid enemies) ──
    public float AttackCooldown = 0f;      // counts down between attacks
    public bool IsRanged => Type is "Wizard" or "Archer";
    float attackRange => IsRanged ? 420f : 46f;   // ranged fire distance vs melee reach
    float attackInterval => Type switch
    {
        "Wizard"  => 1.6f,
        "Archer"  => 1.2f,
        "Warrior" => 1.0f,
        _         => 1.1f
    };
   
    public Vector2 SpawnPosition;

    // ── HIT FLASH ──
    public float FlashTimer = 0f;        // counts down while flashing white
    const float FlashDuration = 0.12f;
    public bool IsFlashing => FlashTimer > 0f;
    public void TriggerFlash() => FlashTimer = FlashDuration;
    // ... existing code ...

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 40, 40);

    public Vector2 Center => new Vector2(Position.X + 20, Position.Y + 20);

    public Enemy(Vector2 pos, string type, int health, Color color)
    {
        Position = pos;
        SpawnPosition = pos;
        Type = type;
        Health = health;
        MaxHealth = health;
        EnemyColor = color;
        wanderTarget = pos;
    }

     public void Update(float dt, Vector2 playerPos)
    {
        if (FlashTimer > 0f) FlashTimer -= dt;
        if (Dead)
        {
            respawnTimer += dt;
            if (respawnTimer >= 10f)
            {
                Dead = false;
                Health = MaxHealth;
                Position = SpawnPosition;
                respawnTimer = 0f;
                Aggro = false;
            }
            return;
        }

        float distToPlayer = Vector2.Distance(Center, playerPos);

        // aggro logic
        if (!Aggro && distToPlayer < aggroRange) Aggro = true;
        if (Aggro && distToPlayer > deAggroRange) Aggro = false;

        if (AttackCooldown > 0f) AttackCooldown -= dt;   // NEW: tick attack timer

        if (Aggro)
        {
            Vector2 dir = playerPos - Center;
            float dist = dir.Length();

            // Ranged enemies stop at range and hold position to fire; melee close in.
            bool inFiringRange = IsRanged && dist <= attackRange;
            if (dir.Length() > 5f && !inFiringRange)
                Position += Vector2.Normalize(dir) * chaseSpeed * dt;
        }

        else
        {
            // normal wander
            wanderTimer -= dt;
            if (wanderTimer <= 0)
            {
                wanderTarget = SpawnPosition + new Vector2(
                    Raylib.GetRandomValue(-100, 100),
                    Raylib.GetRandomValue(-100, 100)
                );
                wanderTimer = Raylib.GetRandomValue(2, 5);
            }
            Position = Vector2.Lerp(Position, wanderTarget, dt * 1.2f);
        }
    }
    public void Draw()
{
    if (Dead) return;

    int x = (int)Position.X;
    int y = (int)Position.Y;

    switch (Type)
    {
        case "Wild Dog":
            DrawWildDog(x, y);
            break;
        case "Wolf":
            DrawWolf(x, y);
            break;
        case "Scorpion":
            DrawScorpion(x, y);
            break;
        case "Bear":
            DrawBear(x, y);
            break;
        case "Crab":          DrawCrab(x, y);         break;
        case "Shark":         DrawShark(x, y);        break;
        case "Snake":         DrawSnake(x, y);        break;
        case "Crocodile":     DrawCrocodile(x, y);    break;
        case "Fire Lizard":   DrawFireLizard(x, y);   break;
        case "Magma Beetle":  DrawMagmaBeetle(x, y);  break;
        case "Eagle":         DrawEagle(x, y);        break;
        case "Mountain Goat": DrawMountainGoat(x, y); break;
        case "Warrior":       DrawWarrior(x, y);      break;
        case "Wizard":        DrawWizard(x, y);       break;
        case "Archer":        DrawArcher(x, y);       break;
        case "Goblin":        DrawGoblin(x, y);       break;
        case "Thug":          DrawThug(x, y);         break;
        case "Robber":        DrawRobber(x, y);       break;
        case "Gangster":      DrawGangster(x, y);     break;
        case "Giant Bug":     DrawGiantBug(x, y);     break;
            }
     if (IsFlashing)
    {
        float a = FlashTimer / FlashDuration;          // 1 → 0
        Raylib.DrawRectangle(x, y, 40, 40,
            new Color((byte)255, (byte)255, (byte)255, (byte)(170 * a)));
    }

    // Health bar background
    Raylib.DrawRectangle(x, y - 10, 40, 6, Color.DarkGray);

    // Health bar fill
    float healthPercent = (float)Health / MaxHealth;
    Raylib.DrawRectangle(x, y - 10, (int)(40 * healthPercent), 6, Color.Red);
}

private void DrawWildDog(int x, int y)
{
    Color brown = Color.Brown;
    Color dark = new Color((byte)80, (byte)40, (byte)10, (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 16, 10, brown);

    // Head
    Raylib.DrawCircle(x + 32, y + 18, 9, brown);

    // Snout
    Raylib.DrawEllipse(x + 39, y + 20, 5, 4, dark);

    // Nose
    Raylib.DrawCircle(x + 42, y + 19, 2, Color.Black);

    // Eye
    Raylib.DrawCircle(x + 35, y + 16, 2, Color.Black);
    Raylib.DrawCircle(x + 36, y + 15, 1, Color.White); // shine

    // Ears
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 10),
        new Vector2(x + 24, y + 18),
        new Vector2(x + 32, y + 18),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 35, y + 9),
        new Vector2(x + 31, y + 17),
        new Vector2(x + 38, y + 16),
        dark
    );

    // Legs (4 short rectangles)
    Raylib.DrawRectangle(x + 8,  y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 15, y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 24, y + 33, 5, 8, dark);
    Raylib.DrawRectangle(x + 31, y + 33, 5, 8, dark);

    // Tail (curved via small circles)
    Raylib.DrawCircle(x + 5,  y + 24, 3, brown);
    Raylib.DrawCircle(x + 2,  y + 20, 3, brown);
    Raylib.DrawCircle(x + 1,  y + 16, 2, brown);
}

private void DrawWolf(int x, int y)
{
    Color gray = Color.DarkGray;
    Color light = new Color((byte)180, (byte)180, (byte)190, (byte)255);
    Color dark = new Color((byte)40, (byte)40, (byte)50, (byte)255);

    // Body — bigger and bulkier than dog
    Raylib.DrawEllipse(x + 20, y + 27, 18, 12, gray);

    // Chest lighter patch
    Raylib.DrawEllipse(x + 28, y + 28, 7, 8, light);

    // Head
    Raylib.DrawCircle(x + 33, y + 17, 11, gray);

    // Snout — longer and more pointed
    Raylib.DrawTriangle(
        new Vector2(x + 33, y + 18),
        new Vector2(x + 33, y + 23),
        new Vector2(x + 46, y + 21),
        dark
    );

    // Nose
    Raylib.DrawCircle(x + 44, y + 20, 2, Color.Black);

    // Eyes — more menacing
    Raylib.DrawCircle(x + 36, y + 14, 2, new Color((byte)200, (byte)200, (byte)0, (byte)255)); // yellow
    Raylib.DrawCircle(x + 36, y + 14, 1, Color.Black);

    // Ears — tall and pointed
    Raylib.DrawTriangle(
        new Vector2(x + 27, y + 7),
        new Vector2(x + 23, y + 18),
        new Vector2(x + 32, y + 17),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 6),
        new Vector2(x + 33, y + 16),
        new Vector2(x + 41, y + 15),
        dark
    );

    // Legs — longer than dog
    Raylib.DrawRectangle(x + 6,  y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 14, y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 24, y + 35, 5, 10, dark);
    Raylib.DrawRectangle(x + 32, y + 35, 5, 10, dark);

    // Tail
    Raylib.DrawCircle(x + 4,  y + 26, 4, gray);
    Raylib.DrawCircle(x + 1,  y + 21, 3, gray);
    Raylib.DrawCircle(x + 0,  y + 16, 3, light); // white tail tip
}

private void DrawScorpion(int x, int y)
{
    Color shell = new Color((byte)180, (byte)120, (byte)0, (byte)255);
    Color dark  = new Color((byte)100, (byte)60,  (byte)0, (byte)255);
    Color sting = new Color((byte)220, (byte)60,  (byte)0, (byte)255);

    // Body segments
    Raylib.DrawEllipse(x + 20, y + 26, 10, 7, shell);  // abdomen
    Raylib.DrawEllipse(x + 28, y + 24, 7,  6, shell);  // mid
    Raylib.DrawEllipse(x + 34, y + 22, 6,  5, shell);  // thorax

    // Head
    Raylib.DrawCircle(x + 38, y + 20, 6, dark);

    // Eyes (two small dots)
    Raylib.DrawCircle(x + 36, y + 18, 1, Color.Black);
    Raylib.DrawCircle(x + 40, y + 18, 1, Color.Black);

    // Claws (left)
    Raylib.DrawLineEx(new Vector2(x + 14, y + 20), new Vector2(x + 7,  y + 15), 3, dark);
    Raylib.DrawCircle(x + 5,  y + 13, 5, shell);
    Raylib.DrawCircle(x + 9,  y + 10, 3, shell);

    // Claws (right)
    Raylib.DrawLineEx(new Vector2(x + 14, y + 22), new Vector2(x + 6, y + 28), 3, dark);
    Raylib.DrawCircle(x + 4,  y + 30, 5, shell);
    Raylib.DrawCircle(x + 8,  y + 34, 3, shell);

    // Legs (3 per side)
    Raylib.DrawLineEx(new Vector2(x + 18, y + 28), new Vector2(x + 12, y + 22), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 30), new Vector2(x + 13, y + 36), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 23, y + 30), new Vector2(x + 18, y + 38), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 28), new Vector2(x + 32, y + 22), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 24, y + 30), new Vector2(x + 31, y + 36), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 22, y + 30), new Vector2(x + 27, y + 38), 2, dark);

    // Tail segments curving up and over
    Raylib.DrawCircle(x + 13, y + 20, 4, shell);
    Raylib.DrawCircle(x + 9,  y + 15, 4, shell);
    Raylib.DrawCircle(x + 7,  y + 9,  4, shell);
    Raylib.DrawCircle(x + 10, y + 4,  3, shell);

    // Stinger tip
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 1),
        new Vector2(x + 7,  y + 4),
        new Vector2(x + 14, y + 4),
        sting
    );
}

private void DrawBear(int x, int y)
{
    Color fur  = new Color((byte)100, (byte)100, (byte)120, (byte)255); // blue-gray for snow bear
    Color dark = new Color((byte)50,  (byte)50,  (byte)65,  (byte)255);
    Color snout = new Color((byte)140, (byte)130, (byte)140, (byte)255);

    // Large body
    Raylib.DrawEllipse(x + 20, y + 28, 20, 15, fur);

    // Head — large and round
    Raylib.DrawCircle(x + 20, y + 14, 14, fur);

    // Snout
    Raylib.DrawEllipse(x + 20, y + 18, 7, 5, snout);

    // Nose
    Raylib.DrawEllipse(x + 20, y + 14, 4, 3, Color.Black);

    // Eyes
    Raylib.DrawCircle(x + 14, y + 11, 3, Color.Black);
    Raylib.DrawCircle(x + 26, y + 11, 3, Color.Black);
    Raylib.DrawCircle(x + 15, y + 10, 1, Color.White); // shine left
    Raylib.DrawCircle(x + 27, y + 10, 1, Color.White); // shine right

    // Round ears
    Raylib.DrawCircle(x + 10, y + 3, 5, fur);
    Raylib.DrawCircle(x + 30, y + 3, 5, fur);
    Raylib.DrawCircle(x + 10, y + 3, 3, dark); // inner ear
    Raylib.DrawCircle(x + 30, y + 3, 3, dark);

    // Thick legs
    Raylib.DrawRectangleRounded(new Rectangle(x + 4,  y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 14, y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 24, y + 36, 9, 12), 0.4f, 4, dark);
    Raylib.DrawRectangleRounded(new Rectangle(x + 34, y + 36, 9, 12), 0.4f, 4, dark);

    // Claws
    for (int i = 0; i < 3; i++)
    {
        Raylib.DrawLineEx(
            new Vector2(x + 5  + i * 3, y + 48),
            new Vector2(x + 4  + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 15 + i * 3, y + 48),
            new Vector2(x + 14 + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 25 + i * 3, y + 48),
            new Vector2(x + 24 + i * 3, y + 52),
            2, Color.White
        );
        Raylib.DrawLineEx(
            new Vector2(x + 35 + i * 3, y + 48),
            new Vector2(x + 34 + i * 3, y + 52),
            2, Color.White
        );
    }
}
private void DrawCrab(int x, int y)
{
    Color shell = new Color((byte)210, (byte)80,  (byte)30,  (byte)255);
    Color dark  = new Color((byte)140, (byte)40,  (byte)10,  (byte)255);
    Color white = Color.White;

    // Body — wide oval
    Raylib.DrawEllipse(x + 20, y + 24, 16, 10, shell);

    // Eyes on stalks
    Raylib.DrawLineEx(new Vector2(x + 14, y + 16), new Vector2(x + 12, y + 12), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 16), new Vector2(x + 28, y + 12), 2, dark);
    Raylib.DrawCircle(x + 12, y + 11, 3, dark);
    Raylib.DrawCircle(x + 12, y + 11, 1, white);
    Raylib.DrawCircle(x + 28, y + 11, 3, dark);
    Raylib.DrawCircle(x + 28, y + 11, 1, white);

    // Big claws
    Raylib.DrawLineEx(new Vector2(x + 6,  y + 20), new Vector2(x - 4, y + 14), 3, dark);
    Raylib.DrawEllipse(x - 6, y + 12, 6, 4, shell);
    Raylib.DrawEllipse(x - 4, y + 16, 4, 3, shell);

    Raylib.DrawLineEx(new Vector2(x + 34, y + 20), new Vector2(x + 44, y + 14), 3, dark);
    Raylib.DrawEllipse(x + 46, y + 12, 6, 4, shell);
    Raylib.DrawEllipse(x + 44, y + 16, 4, 3, shell);

    // Walking legs (3 per side)
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 26), new Vector2(x + 2,  y + 20), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 28), new Vector2(x + 1,  y + 34), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 10, y + 30), new Vector2(x + 5,  y + 38), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 32, y + 26), new Vector2(x + 38, y + 20), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 32, y + 28), new Vector2(x + 39, y + 34), 2, dark);
    Raylib.DrawLineEx(new Vector2(x + 30, y + 30), new Vector2(x + 35, y + 38), 2, dark);
}

private void DrawShark(int x, int y)
{
    Color blue  = new Color((byte)70,  (byte)100, (byte)140, (byte)255);
    Color light = new Color((byte)200, (byte)220, (byte)230, (byte)255);
    Color dark  = new Color((byte)30,  (byte)50,  (byte)80,  (byte)255);

    // Body — long torpedo shape
    Raylib.DrawEllipse(x + 20, y + 22, 20, 10, blue);

    // Belly (lighter underside)
    Raylib.DrawEllipse(x + 20, y + 26, 14, 6, light);

    // Dorsal fin on top
    Raylib.DrawTriangle(
        new Vector2(x + 18, y + 12),
        new Vector2(x + 14, y + 22),
        new Vector2(x + 26, y + 22),
        dark
    );

    // Tail fin
    Raylib.DrawTriangle(
        new Vector2(x + 0,  y + 16),
        new Vector2(x + 8,  y + 22),
        new Vector2(x + 2,  y + 30),
        dark
    );

    // Pectoral fins
    Raylib.DrawTriangle(
        new Vector2(x + 14, y + 24),
        new Vector2(x + 6,  y + 30),
        new Vector2(x + 18, y + 30),
        blue
    );
    Raylib.DrawTriangle(
        new Vector2(x + 26, y + 24),
        new Vector2(x + 34, y + 30),
        new Vector2(x + 22, y + 30),
        blue
    );

    // Eye
    Raylib.DrawCircle(x + 32, y + 20, 3, Color.Black);
    Raylib.DrawCircle(x + 33, y + 19, 1, Color.White);

    // Mouth with teeth
    Raylib.DrawLineEx(new Vector2(x + 36, y + 24), new Vector2(x + 42, y + 22), 2, dark);
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 24),
        new Vector2(x + 39, y + 24),
        new Vector2(x + 38, y + 27),
        Color.White
    );
    Raylib.DrawTriangle(
        new Vector2(x + 40, y + 23),
        new Vector2(x + 42, y + 23),
        new Vector2(x + 41, y + 26),
        Color.White
    );
}

private void DrawSnake(int x, int y)
{
    Color green  = new Color((byte)40,  (byte)100, (byte)40,  (byte)255);
    Color dark   = new Color((byte)20,  (byte)60,  (byte)20,  (byte)255);
    Color yellow = new Color((byte)200, (byte)200, (byte)50,  (byte)255);

    // Body — S-curve using circles
    Raylib.DrawCircle(x + 30, y + 30, 6, green);
    Raylib.DrawCircle(x + 24, y + 26, 6, green);
    Raylib.DrawCircle(x + 18, y + 24, 6, green);
    Raylib.DrawCircle(x + 13, y + 26, 5, green);
    Raylib.DrawCircle(x + 9,  y + 30, 5, green);
    Raylib.DrawCircle(x + 7,  y + 34, 4, green);

    // Scale pattern stripes
    Raylib.DrawCircle(x + 24, y + 26, 3, dark);
    Raylib.DrawCircle(x + 13, y + 26, 3, dark);

    // Tail tip
    Raylib.DrawCircle(x + 5,  y + 37, 2, dark);

    // Head
    Raylib.DrawEllipse(x + 35, y + 28, 7, 5, green);

    // Eyes
    Raylib.DrawCircle(x + 37, y + 26, 2, yellow);
    Raylib.DrawCircle(x + 37, y + 26, 1, Color.Black);

    // Forked tongue
    Raylib.DrawLineEx(new Vector2(x + 41, y + 28), new Vector2(x + 45, y + 26), 1, Color.Red);
    Raylib.DrawLineEx(new Vector2(x + 41, y + 28), new Vector2(x + 45, y + 30), 1, Color.Red);
}

private void DrawCrocodile(int x, int y)
{
    Color green = new Color((byte)50,  (byte)80,  (byte)30,  (byte)255);
    Color dark  = new Color((byte)25,  (byte)45,  (byte)15,  (byte)255);
    Color belly = new Color((byte)120, (byte)140, (byte)80,  (byte)255);

    // Body — long and low
    Raylib.DrawEllipse(x + 20, y + 28, 20, 9, green);

    // Belly
    Raylib.DrawEllipse(x + 20, y + 30, 14, 5, belly);

    // Armored back bumps
    Raylib.DrawCircle(x + 10, y + 22, 4, dark);
    Raylib.DrawCircle(x + 17, y + 21, 4, dark);
    Raylib.DrawCircle(x + 24, y + 21, 4, dark);
    Raylib.DrawCircle(x + 31, y + 22, 3, dark);

    // Head — wide flat snout
    Raylib.DrawEllipse(x + 36, y + 27, 10, 6, green);

    // Teeth
    for (int i = 0; i < 3; i++)
    {
        Raylib.DrawTriangle(
            new Vector2(x + 38 + i * 4, y + 22),
            new Vector2(x + 36 + i * 4, y + 26),
            new Vector2(x + 40 + i * 4, y + 26),
            Color.White
        );
    }

    // Eyes on top of head
    Raylib.DrawCircle(x + 33, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 3, new Color((byte)200, (byte)200, (byte)50, (byte)255));
    Raylib.DrawCircle(x + 39, y + 23, 1, Color.Black);

    // Legs
    Raylib.DrawRectangle(x + 6,  y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 14, y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 22, y + 34, 6, 6, dark);
    Raylib.DrawRectangle(x + 30, y + 34, 6, 6, dark);

    // Tail
    Raylib.DrawCircle(x + 3,  y + 28, 5, green);
    Raylib.DrawCircle(x - 2, y + 27, 3, green);
}

private void DrawFireLizard(int x, int y)
{
    Color orange = new Color((byte)180, (byte)60,  (byte)10,  (byte)255);
    Color red    = new Color((byte)220, (byte)30,  (byte)0,   (byte)255);
    Color yellow = new Color((byte)255, (byte)200, (byte)0,   (byte)255);
    Color dark   = new Color((byte)80,  (byte)20,  (byte)0,   (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 14, 9, orange);

    // Fire pattern on back
    Raylib.DrawTriangle(
        new Vector2(x + 12, y + 18),
        new Vector2(x + 9,  y + 26),
        new Vector2(x + 15, y + 26),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 20, y + 16),
        new Vector2(x + 17, y + 26),
        new Vector2(x + 23, y + 26),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 18),
        new Vector2(x + 25, y + 26),
        new Vector2(x + 31, y + 26),
        red
    );

    // Head
    Raylib.DrawEllipse(x + 33, y + 22, 8, 6, orange);

    // Spiky crest on head
    Raylib.DrawTriangle(
        new Vector2(x + 30, y + 16),
        new Vector2(x + 28, y + 22),
        new Vector2(x + 33, y + 22),
        red
    );
    Raylib.DrawTriangle(
        new Vector2(x + 35, y + 14),
        new Vector2(x + 32, y + 21),
        new Vector2(x + 38, y + 21),
        yellow
    );

    // Eye
    Raylib.DrawCircle(x + 36, y + 21, 2, yellow);
    Raylib.DrawCircle(x + 36, y + 21, 1, Color.Black);

    // Legs
    Raylib.DrawRectangle(x + 8,  y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 16, y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 24, y + 32, 5, 8, dark);
    Raylib.DrawRectangle(x + 30, y + 32, 5, 8, dark);

    // Tail with flame tip
    Raylib.DrawCircle(x + 6,  y + 26, 4, orange);
    Raylib.DrawCircle(x + 2,  y + 23, 3, red);
    Raylib.DrawCircle(x + 0,  y + 19, 2, yellow);
}

private void DrawMagmaBeetle(int x, int y)
{
    Color black  = new Color((byte)30,  (byte)15,  (byte)5,   (byte)255);
    Color lava   = new Color((byte)220, (byte)80,  (byte)0,   (byte)255);
    Color glow   = new Color((byte)255, (byte)180, (byte)0,   (byte)255);

    // Shell — large domed oval
    Raylib.DrawEllipse(x + 20, y + 24, 17, 13, black);

    // Lava cracks on shell
    Raylib.DrawLineEx(new Vector2(x + 12, y + 18), new Vector2(x + 20, y + 28), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 18), new Vector2(x + 26, y + 30), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 18), new Vector2(x + 20, y + 28), 2, lava);
    Raylib.DrawLineEx(new Vector2(x + 16, y + 28), new Vector2(x + 12, y + 34), 2, lava);

    // Glowing lava spots
    Raylib.DrawCircle(x + 16, y + 22, 3, glow);
    Raylib.DrawCircle(x + 24, y + 20, 2, glow);
    Raylib.DrawCircle(x + 22, y + 30, 3, glow);

    // Head
    Raylib.DrawCircle(x + 34, y + 22, 6, black);

    // Glowing eyes
    Raylib.DrawCircle(x + 32, y + 20, 2, glow);
    Raylib.DrawCircle(x + 37, y + 20, 2, glow);

    // Horn
    Raylib.DrawTriangle(
        new Vector2(x + 38, y + 16),
        new Vector2(x + 35, y + 22),
        new Vector2(x + 41, y + 22),
        lava
    );

    // Legs
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 28), new Vector2(x + 2,  y + 22), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 8,  y + 30), new Vector2(x + 1,  y + 36), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 10, y + 32), new Vector2(x + 4,  y + 38), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 28), new Vector2(x + 34, y + 22), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 28, y + 30), new Vector2(x + 35, y + 36), 3, black);
    Raylib.DrawLineEx(new Vector2(x + 26, y + 32), new Vector2(x + 32, y + 38), 3, black);
}

private void DrawEagle(int x, int y)
{
    Color brown  = new Color((byte)100, (byte)70,  (byte)20,  (byte)255);
    Color white  = Color.White;
    Color yellow = new Color((byte)240, (byte)180, (byte)0,   (byte)255);
    Color dark   = new Color((byte)40,  (byte)25,  (byte)5,   (byte)255);

    // Body
    Raylib.DrawEllipse(x + 20, y + 26, 12, 9, brown);

    // White head
    Raylib.DrawCircle(x + 30, y + 18, 9, white);

    // Beak
    Raylib.DrawTriangle(
        new Vector2(x + 37, y + 18),
        new Vector2(x + 44, y + 20),
        new Vector2(x + 37, y + 22),
        yellow
    );

    // Eye
    Raylib.DrawCircle(x + 33, y + 16, 2, Color.Black);
    Raylib.DrawCircle(x + 34, y + 15, 1, white);

    // Wings spread wide
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 22),
        new Vector2(x - 8,  y + 16),
        new Vector2(x - 4,  y + 30),
        brown
    );
    Raylib.DrawTriangle(
        new Vector2(x + 10, y + 22),
        new Vector2(x - 4,  y + 30),
        new Vector2(x + 14, y + 30),
        dark
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 22),
        new Vector2(x + 48, y + 16),
        new Vector2(x + 44, y + 30),
        brown
    );
    Raylib.DrawTriangle(
        new Vector2(x + 28, y + 22),
        new Vector2(x + 44, y + 30),
        new Vector2(x + 24, y + 30),
        dark
    );

    // Talons
    Raylib.DrawLineEx(new Vector2(x + 16, y + 34), new Vector2(x + 12, y + 40), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 18, y + 34), new Vector2(x + 16, y + 41), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 20, y + 34), new Vector2(x + 20, y + 41), 2, yellow);
    Raylib.DrawLineEx(new Vector2(x + 22, y + 34), new Vector2(x + 24, y + 41), 2, yellow);
}

private void DrawMountainGoat(int x, int y)
{
    Color white = new Color((byte)200, (byte)195, (byte)185, (byte)255);
    Color dark  = new Color((byte)80,  (byte)75,  (byte)65,  (byte)255);
    Color horn  = new Color((byte)60,  (byte)45,  (byte)20,  (byte)255);

    // Body — stocky
    Raylib.DrawEllipse(x + 20, y + 26, 16, 11, white);

    // Shaggy chest tuft
    Raylib.DrawEllipse(x + 30, y + 30, 7, 9, white);

    // Head
    Raylib.DrawCircle(x + 33, y + 17, 9, white);

    // Snout
    Raylib.DrawEllipse(x + 39, y + 19, 5, 4, dark);
    Raylib.DrawCircle(x + 42, y + 18, 2, Color.Black);

    // Eye
    Raylib.DrawCircle(x + 35, y + 14, 2, Color.Black);
    Raylib.DrawCircle(x + 36, y + 13, 1, Color.White);

    // Curved horns
    Raylib.DrawLineEx(new Vector2(x + 30, y + 9),  new Vector2(x + 24, y + 4),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 24, y + 4),  new Vector2(x + 20, y + 8),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 36, y + 8),  new Vector2(x + 42, y + 3),  3, horn);
    Raylib.DrawLineEx(new Vector2(x + 42, y + 3),  new Vector2(x + 46, y + 8),  3, horn);

    // Beard
    Raylib.DrawEllipse(x + 37, y + 25, 3, 5, dark);

    // Sturdy legs
    Raylib.DrawRectangle(x + 7,  y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 15, y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 23, y + 34, 6, 10, dark);
    Raylib.DrawRectangle(x + 31, y + 34, 6, 10, dark);

    // Hooves
    Raylib.DrawRectangle(x + 7,  y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 15, y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 23, y + 43, 6, 3, Color.Black);
    Raylib.DrawRectangle(x + 31, y + 43, 6, 3, Color.Black);
}
private void DrawHumanoidBase(int x, int y, Color body, Color skin)
{
    Raylib.DrawRectangle(x + 12, y + 18, 16, 18, body);   // torso
    Raylib.DrawCircle(x + 20, y + 12, 7, skin);           // head
    Raylib.DrawRectangle(x + 12, y + 36, 6, 8, body);     // left leg
    Raylib.DrawRectangle(x + 22, y + 36, 6, 8, body);     // right leg
}
private void DrawWarrior(int x, int y)
{
    Color armor = new Color((byte)150,(byte)150,(byte)165,(byte)255);
    DrawHumanoidBase(x, y, armor, new Color((byte)225,(byte)185,(byte)150,(byte)255));
    // sword
    Raylib.DrawLineEx(new Vector2(x + 30, y + 30), new Vector2(x + 40, y + 8), 3, Color.LightGray);
    Raylib.DrawLineEx(new Vector2(x + 27, y + 24), new Vector2(x + 33, y + 24), 3, new Color((byte)90,(byte)70,(byte)40,(byte)255));
    // shield
    Raylib.DrawCircle(x + 9, y + 26, 6, new Color((byte)120,(byte)90,(byte)50,(byte)255));
}

private void DrawWizard(int x, int y)
{
    Color robe = new Color((byte)90,(byte)60,(byte)150,(byte)255);
    DrawHumanoidBase(x, y, robe, new Color((byte)225,(byte)185,(byte)150,(byte)255));
    // pointed hat
    Raylib.DrawTriangle(new Vector2(x + 12, y + 8), new Vector2(x + 28, y + 8), new Vector2(x + 20, y - 8), robe);
    // staff + glowing orb
    Raylib.DrawLineEx(new Vector2(x + 32, y + 34), new Vector2(x + 34, y + 6), 3, new Color((byte)110,(byte)80,(byte)45,(byte)255));
    Raylib.DrawCircle(x + 34, y + 5, 4, new Color((byte)180,(byte)90,(byte)230,(byte)255));
}

private void DrawArcher(int x, int y)
{
    Color leather = new Color((byte)120,(byte)85,(byte)45,(byte)255);
    DrawHumanoidBase(x, y, leather, new Color((byte)225,(byte)185,(byte)150,(byte)255));
    // bow arc
    Raylib.DrawLineEx(new Vector2(x + 32, y + 8), new Vector2(x + 32, y + 30), 2, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawLineEx(new Vector2(x + 32, y + 8), new Vector2(x + 30, y + 19), 1, Color.White);
    Raylib.DrawLineEx(new Vector2(x + 32, y + 30), new Vector2(x + 30, y + 19), 1, Color.White);
}

private void DrawGoblin(int x, int y)
{
    Color green = new Color((byte)90,(byte)150,(byte)70,(byte)255);
    DrawHumanoidBase(x, y, new Color((byte)80,(byte)70,(byte)55,(byte)255), green);
    // pointy ears
    Raylib.DrawTriangle(new Vector2(x + 13, y + 12), new Vector2(x + 16, y + 8), new Vector2(x + 16, y + 14), green);
    Raylib.DrawTriangle(new Vector2(x + 27, y + 12), new Vector2(x + 24, y + 8), new Vector2(x + 24, y + 14), green);
    Raylib.DrawCircle(x + 17, y + 12, 1, Color.Red);
    Raylib.DrawCircle(x + 23, y + 12, 1, Color.Red);
}

private void DrawThug(int x, int y)
{
    DrawHumanoidBase(x, y, new Color((byte)70,(byte)70,(byte)80,(byte)255), new Color((byte)210,(byte)170,(byte)140,(byte)255));
    // club
    Raylib.DrawLineEx(new Vector2(x + 30, y + 32), new Vector2(x + 36, y + 14), 4, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawCircle(x + 36, y + 13, 4, new Color((byte)70,(byte)45,(byte)20,(byte)255));
}

private void DrawRobber(int x, int y)
{
    DrawHumanoidBase(x, y, new Color((byte)45,(byte)45,(byte)55,(byte)255), new Color((byte)210,(byte)170,(byte)140,(byte)255));
    // eye mask
    Raylib.DrawRectangle(x + 13, y + 10, 14, 3, Color.Black);
}

private void DrawGangster(int x, int y)
{
    DrawHumanoidBase(x, y, new Color((byte)30,(byte)30,(byte)35,(byte)255), new Color((byte)210,(byte)170,(byte)140,(byte)255));
    // fedora
    Raylib.DrawRectangle(x + 11, y + 7, 18, 3, Color.Black);
    Raylib.DrawRectangle(x + 15, y + 2, 10, 5, Color.Black);
    // red tie
    Raylib.DrawTriangle(new Vector2(x + 20, y + 18), new Vector2(x + 18, y + 24), new Vector2(x + 22, y + 24), Color.Red);
}

private void DrawGiantBug(int x, int y)
{
    Color chitin = new Color((byte)60,(byte)90,(byte)45,(byte)255);
    Raylib.DrawEllipse(x + 20, y + 26, 15, 11, chitin);          // abdomen
    Raylib.DrawCircle(x + 20, y + 14, 8, chitin);                // head
    Raylib.DrawCircle(x + 16, y + 12, 2, Color.Red);             // eyes
    Raylib.DrawCircle(x + 24, y + 12, 2, Color.Red);
    // legs
    for (int i = -1; i <= 1; i++)
    {
        Raylib.DrawLineEx(new Vector2(x + 8, y + 22 + i * 5), new Vector2(x + 2, y + 18 + i * 6), 2, chitin);
        Raylib.DrawLineEx(new Vector2(x + 32, y + 22 + i * 5), new Vector2(x + 38, y + 18 + i * 6), 2, chitin);
    }
    // antennae
    Raylib.DrawLineEx(new Vector2(x + 16, y + 8), new Vector2(x + 12, y + 2), 1, chitin);
    Raylib.DrawLineEx(new Vector2(x + 24, y + 8), new Vector2(x + 28, y + 2), 1, chitin);
}

}
    class Quest
{
    public string Title;
    public string Description;
    public int Target;
    public int Progress;
    public bool Completed;
    public int Reward;

    public Quest(string title, string description, int target, int reward)
    {
        Title = title;
        Description = description;
        Target = target;
        Reward = reward;
        Progress = 0;
        Completed = false;
    }
}
}
