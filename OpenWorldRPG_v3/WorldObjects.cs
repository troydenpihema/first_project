using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    class TreeObject
{
    public Vector2 Position;
    public bool Chopped = false;
    public int Health;
    public int MaxHealth;
    public string TreeType;
    public int LevelRequired;
    public int XPReward;
    public string LogType;
    float respawnTimer = 0f;
    public float ChopAnimTimer = 0f;
    public bool IsBeingChopped = false;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 80);
    public Vector2 Center => new Vector2(Position.X + 30, Position.Y + 40);

    public static TreeObject Normal(Vector2 pos) => new TreeObject(pos, "Normal", 3, 1, 25, "Logs");
    public static TreeObject Birch(Vector2 pos) => new TreeObject(pos, "Birch", 4, 3, 40, "Birch Logs");
    public static TreeObject Oak(Vector2 pos) => new TreeObject(pos, "Oak", 5, 5, 60, "Oak Logs");
    public static TreeObject Pine(Vector2 pos) => new TreeObject(pos, "Pine", 6, 10, 80, "Pine Logs");
    public static TreeObject Arctic(Vector2 pos) => new TreeObject(pos, "Arctic", 7, 15, 100, "Arctic Logs");
    public static TreeObject Dead(Vector2 pos) => new TreeObject(pos, "Dead", 2, 1, 15, "Dead Wood");

    public TreeObject(Vector2 pos, string treeType, int health, int levelRequired, int xpReward, string logType)
    {
        Position = pos;
        TreeType = treeType;
        Health = health;
        MaxHealth = health;
        LevelRequired = levelRequired;
        XPReward = xpReward;
        LogType = logType;
    }

    public void Update(float dt)
    {
        if (IsBeingChopped)
{
    ChopAnimTimer += dt * 10f;
    if (ChopAnimTimer >= 1f) { ChopAnimTimer = 0f; IsBeingChopped = false; }
}
        if (Chopped)
        {
            respawnTimer += dt;
            if (respawnTimer >= 5f)
            {
                Chopped = false;
                Health = MaxHealth;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        if (Chopped)
        {
            Raylib.DrawRectangle((int)Position.X + 12, (int)Position.Y + 55, 36, 18, Color.Brown);
            return;
        }

        switch (TreeType)
        {
            case "Normal":
                // brown trunk, dark green leaves
                Raylib.DrawRectangle((int)Position.X + 20, (int)Position.Y + 40, 20, 40, Color.DarkBrown);
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 25, 35, Color.DarkGreen);
                break;

            case "Birch":
                // white trunk, light green leaves
                Raylib.DrawRectangle((int)Position.X + 20, (int)Position.Y + 40, 20, 40, Color.White);
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 45, 4, 6, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 58, 4, 6, Color.DarkGray);
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 25, 35, new Color((byte)144,(byte)238,(byte)144,(byte)255));
                break;

            case "Oak":
                // wide dark trunk, big deep green canopy
                Raylib.DrawRectangle((int)Position.X + 18, (int)Position.Y + 40, 24, 40, new Color((byte)101,(byte)67,(byte)33,(byte)255));
                Raylib.DrawCircle((int)Position.X + 30, (int)Position.Y + 20, 42, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                Raylib.DrawCircle((int)Position.X + 10, (int)Position.Y + 30, 28, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                Raylib.DrawCircle((int)Position.X + 50, (int)Position.Y + 30, 28, new Color((byte)0,(byte)100,(byte)0,(byte)255));
                break;

            case "Pine":
                // thin trunk, triangular layers
                Raylib.DrawRectangle((int)Position.X + 24, (int)Position.Y + 55, 12, 25, Color.Brown);
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X, Position.Y + 45),
                    new Vector2(Position.X + 60, Position.Y + 45),
                    new Color((byte)0,(byte)80,(byte)0,(byte)255)
                );
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y + 15),
                    new Vector2(Position.X + 5, Position.Y + 55),
                    new Vector2(Position.X + 55, Position.Y + 55),
                    new Color((byte)0,(byte)100,(byte)0,(byte)255)
                );
                break;

            case "Arctic":
                // snow covered pine
                Raylib.DrawRectangle((int)Position.X + 24, (int)Position.Y + 55, 12, 25, new Color((byte)100,(byte)70,(byte)40,(byte)255));
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X, Position.Y + 45),
                    new Vector2(Position.X + 60, Position.Y + 45),
                    new Color((byte)0,(byte)60,(byte)0,(byte)255)
                );
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y + 15),
                    new Vector2(Position.X + 5, Position.Y + 55),
                    new Vector2(Position.X + 55, Position.Y + 55),
                    new Color((byte)20,(byte)80,(byte)20,(byte)255)
                );
                // snow on top
                Raylib.DrawTriangle(
                    new Vector2(Position.X + 30, Position.Y),
                    new Vector2(Position.X + 8, Position.Y + 28),
                    new Vector2(Position.X + 52, Position.Y + 28),
                    new Color((byte)220,(byte)235,(byte)255,(byte)200)
                );
                break;

            case "Dead":
                // bare grey trunk with branches
                Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 20, 16, 60, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 10, (int)Position.Y + 28, 22, 6, Color.DarkGray);
                Raylib.DrawRectangle((int)Position.X + 28, (int)Position.Y + 38, 20, 5, Color.DarkGray);
                break;
        }

        // crack overlay based on damage taken
if (Health < MaxHealth)
{
    float damagePercent = 1f - ((float)Health / MaxHealth);

    int tx = (int)Position.X + 30; // centre of tree trunk
    int ty = (int)Position.Y + 50;

    // get equipped tool to determine chop mark style
    string equippedTool = Program.GetEquippedTool();
    Color chopColor = new Color((byte)60, (byte)30, (byte)10, (byte)220);  // default wood colour
    float chopWidth = 2f;

    if (equippedTool != null && equippedTool.Contains("Iron"))
    {
        chopColor = new Color((byte)40, (byte)20, (byte)5, (byte)240);
        chopWidth = 3f;  // deeper cuts
    }
    else if (equippedTool != null && equippedTool.Contains("Steel"))
    {
        chopColor = new Color((byte)25, (byte)12, (byte)3, (byte)255);
        chopWidth = 4f;  // even deeper
    }
    else if (equippedTool != null && equippedTool.Contains("Gold"))
    {
        chopColor = new Color((byte)20, (byte)10, (byte)2, (byte)255);
        chopWidth = 5f;  // deepest cuts
    }

    // first chop mark — any damage
    if (damagePercent > 0f)
    {
        // V shaped chop mark on trunk
        Raylib.DrawLineEx(
            new Vector2(tx - 8, ty - 4),
            new Vector2(tx,     ty + 4),
            chopWidth, chopColor);
        Raylib.DrawLineEx(
            new Vector2(tx,     ty + 4),
            new Vector2(tx + 8, ty - 4),
            chopWidth, chopColor);
        // wood chip flying off
        Raylib.DrawCircle(tx - 12, ty - 8, 3,
            new Color((byte)120, (byte)80, (byte)30, (byte)200));
        Raylib.DrawCircle(tx - 14, ty - 12, 2,
            new Color((byte)100, (byte)60, (byte)20, (byte)180));
    }

    // second chop mark at 40% damage — deeper cut
    if (damagePercent > 0.4f)
    {
        // second V cut slightly above first
        Raylib.DrawLineEx(
            new Vector2(tx - 7, ty - 14),
            new Vector2(tx,     ty - 6),
            chopWidth, chopColor);
        Raylib.DrawLineEx(
            new Vector2(tx,     ty - 6),
            new Vector2(tx + 7, ty - 14),
            chopWidth, chopColor);
        // exposed wood grain inside cut
        Raylib.DrawRectangle(tx - 4, ty - 10, 8, 6,
            new Color((byte)180, (byte)130, (byte)70, (byte)180));
        // more chips
        Raylib.DrawCircle(tx + 14, ty - 10, 3,
            new Color((byte)120, (byte)80, (byte)30, (byte)200));
        Raylib.DrawCircle(tx + 16, ty - 16, 2,
            new Color((byte)100, (byte)60, (byte)20, (byte)180));

        // bark cracking up the trunk
        Raylib.DrawLineEx(
            new Vector2(tx - 2, ty - 20),
            new Vector2(tx + 4, ty - 10),
            2f, new Color((byte)40, (byte)20, (byte)5, (byte)200));
    }

    // near breaking at 66% damage — trunk splitting
    if (damagePercent > 0.66f)
    {
        // deep split running up through trunk
        Raylib.DrawLineEx(
            new Vector2(tx,     ty - 30),
            new Vector2(tx - 4, ty),
            chopWidth + 1f, new Color((byte)15, (byte)8, (byte)2, (byte)255));

        // large exposed wood patch
        Raylib.DrawRectangle(tx - 6, ty - 18, 12, 14,
            new Color((byte)200, (byte)150, (byte)80, (byte)200));
        Raylib.DrawRectangle(tx - 4, ty - 16, 8, 10,
            new Color((byte)220, (byte)170, (byte)90, (byte)180));

        // bark chunks falling
        Raylib.DrawRectangle(tx - 18, ty - 20, 6, 8,
            new Color((byte)100, (byte)60, (byte)20, (byte)220));
        Raylib.DrawRectangle(tx + 12, ty - 14, 5, 7,
            new Color((byte)100, (byte)60, (byte)20, (byte)220));
        Raylib.DrawCircle(tx - 20, ty - 28, 4,
            new Color((byte)80, (byte)50, (byte)15, (byte)200));
        Raylib.DrawCircle(tx + 18, ty - 22, 3,
            new Color((byte)80, (byte)50, (byte)15, (byte)200));

        // canopy shaking — draw some loose leaves falling
        Raylib.DrawCircle(tx - 24, ty - 40, 5,
            new Color((byte)0, (byte)100, (byte)0, (byte)160));
        Raylib.DrawCircle(tx + 20, ty - 38, 4,
            new Color((byte)0, (byte)120, (byte)0, (byte)140));
        Raylib.DrawCircle(tx - 10, ty - 50, 4,
            new Color((byte)0, (byte)110, (byte)0, (byte)150));
    }
}

        // level requirement label
       // if (LevelRequired > 1)
       // {
      //      Program.DrawTextUI($"WC {LevelRequired}", (int)Position.X + 5, (int)Position.Y - 18, 16, Color.Yellow);
      // }
    }
}
class RockObject
{
    public Vector2 Position;
    public bool Broken = false;
    public int Health;
    public int MaxHealth;
    public string RockType;
    public int LevelRequired;
    public int XPReward;
    public string OreType;
    public float MineAnimTimer = 0f;
    public bool IsBeingMined = false;
    float respawnTimer = 0f;

    public Rectangle Bounds =>
        new Rectangle(Position.X - 20, Position.Y - 20, 40, 40);

    public static RockObject Stone(Vector2 pos)  => new RockObject(pos, "Stone",  3, 1,  20, "Stone");
    public static RockObject Copper(Vector2 pos) => new RockObject(pos, "Copper", 4, 3,  40, "Copper Ore");
    public static RockObject Iron(Vector2 pos)   => new RockObject(pos, "Iron",   5, 5,  60, "Iron Ore");
    public static RockObject Gold(Vector2 pos)   => new RockObject(pos, "Gold",   6, 10, 100, "Gold Ore");
    public static RockObject Crystal(Vector2 pos)=> new RockObject(pos, "Crystal",7, 15, 140, "Crystal");

    public RockObject(Vector2 pos, string rockType, int health, int levelRequired, int xpReward, string oreType)
    {
        Position = pos;
        RockType = rockType;
        Health = health;
        MaxHealth = health;
        LevelRequired = levelRequired;
        XPReward = xpReward;
        OreType = oreType;
    }

    public void Update(float dt)
    {
        if (IsBeingMined)
{
    MineAnimTimer += dt * 10f;
    if (MineAnimTimer >= 1f) { MineAnimTimer = 0f; IsBeingMined = false; }
}
        if (Broken)
        {
            respawnTimer += dt;
            if (respawnTimer >= 8f)
            {
                Broken = false;
                Health = MaxHealth;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        if (Broken)
        {
            // rubble
            Raylib.DrawCircle(x - 8, y + 6,  5, Color.DarkGray);
            Raylib.DrawCircle(x + 6, y + 8,  4, Color.DarkGray);
            Raylib.DrawCircle(x,     y + 10, 3, Color.Gray);
            return;
        }

        switch (RockType)
        {
            case "Stone":
                Raylib.DrawCircle(x, y, 18, new Color((byte)120,(byte)120,(byte)125,(byte)255));
                Raylib.DrawCircle(x - 6, y - 4, 10, new Color((byte)140,(byte)140,(byte)145,(byte)255));
                Raylib.DrawCircle(x + 8, y - 2,  8, new Color((byte)130,(byte)130,(byte)135,(byte)255));
                Raylib.DrawCircle(x - 4, y - 8,  5, new Color((byte)160,(byte)160,(byte)165,(byte)255));
                break;

            case "Copper":
                Raylib.DrawCircle(x, y, 18, new Color((byte)120,(byte)120,(byte)125,(byte)255));
                Raylib.DrawCircle(x - 6, y - 4, 10, new Color((byte)140,(byte)120,(byte)125,(byte)255));
                // copper ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)180,(byte)100,(byte)40,(byte)255));
                Raylib.DrawCircle(x - 4,  y + 4, 4, new Color((byte)200,(byte)120,(byte)50,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 6, 3, new Color((byte)180,(byte)100,(byte)40,(byte)255));
                break;

            case "Iron":
                Raylib.DrawCircle(x, y, 20, new Color((byte)110,(byte)110,(byte)115,(byte)255));
                Raylib.DrawCircle(x - 8, y - 4, 12, new Color((byte)130,(byte)125,(byte)130,(byte)255));
                // iron ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)140,(byte)130,(byte)140,(byte)255));
                Raylib.DrawCircle(x - 2,  y + 6, 5, new Color((byte)100,(byte)90,(byte)100,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 4, 4, new Color((byte)120,(byte)110,(byte)120,(byte)255));
                break;

            case "Gold":
                Raylib.DrawCircle(x, y, 18, new Color((byte)115,(byte)110,(byte)100,(byte)255));
                Raylib.DrawCircle(x - 5, y - 4, 10, new Color((byte)130,(byte)125,(byte)115,(byte)255));
                // gold ore veins
                Raylib.DrawCircle(x + 4,  y - 2, 6, new Color((byte)220,(byte)180,(byte)0,(byte)255));
                Raylib.DrawCircle(x - 3,  y + 5, 5, new Color((byte)200,(byte)160,(byte)0,(byte)255));
                Raylib.DrawCircle(x + 8,  y + 3, 3, new Color((byte)230,(byte)190,(byte)20,(byte)255));
                break;

            case "Crystal":
                Raylib.DrawCircle(x, y, 16, new Color((byte)160,(byte)200,(byte)220,(byte)255));
                // crystal spikes
                Raylib.DrawTriangle(
                    new Vector2(x, y - 22),
                    new Vector2(x - 8, y),
                    new Vector2(x + 8, y),
                    new Color((byte)180,(byte)220,(byte)240,(byte)255));
                Raylib.DrawTriangle(
                    new Vector2(x + 14, y - 16),
                    new Vector2(x + 6,  y),
                    new Vector2(x + 18, y),
                    new Color((byte)160,(byte)200,(byte)225,(byte)220));
                Raylib.DrawTriangle(
                    new Vector2(x - 14, y - 16),
                    new Vector2(x - 18, y),
                    new Vector2(x - 6,  y),
                    new Color((byte)160,(byte)200,(byte)225,(byte)220));
                break;
        }

        // crack overlay based on damage taken
if (Health < MaxHealth)
{
    float damagePercent = 1f - ((float)Health / MaxHealth);
    
    int rx = (int)Position.X;
    int ry = (int)Position.Y;

    // first crack appears after any damage
    if (damagePercent > 0f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx - 2, ry - 12),
            new Vector2(rx + 6, ry - 2),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        Raylib.DrawLineEx(
            new Vector2(rx + 6, ry - 2),
            new Vector2(rx + 2, ry + 8),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
    }

    // second crack at 40% damage
    if (damagePercent > 0.4f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx - 8, ry),
            new Vector2(rx,     ry + 6),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        Raylib.DrawLineEx(
            new Vector2(rx,     ry + 6),
            new Vector2(rx - 4, ry + 14),
            2f, new Color((byte)30, (byte)30, (byte)30, (byte)200));
        // small chip flaking off
        Raylib.DrawCircle(rx - 10, ry - 6, 3,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
    }

    // third crack at 66% damage — rock is nearly broken
    if (damagePercent > 0.66f)
    {
        Raylib.DrawLineEx(
            new Vector2(rx + 4,  ry - 8),
            new Vector2(rx + 14, ry + 4),
            2f, new Color((byte)20, (byte)20, (byte)20, (byte)220));
        Raylib.DrawLineEx(
            new Vector2(rx + 14, ry + 4),
            new Vector2(rx + 8,  ry + 14),
            2f, new Color((byte)20, (byte)20, (byte)20, (byte)220));
        // more chips
        Raylib.DrawCircle(rx + 16, ry - 4, 3,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
        Raylib.DrawCircle(rx + 10, ry + 16, 2,
            new Color((byte)80, (byte)80, (byte)85, (byte)255));
        // dark split running through centre
        Raylib.DrawLineEx(
            new Vector2(rx - 6, ry - 6),
            new Vector2(rx + 8, ry + 10),
            3f, new Color((byte)15, (byte)15, (byte)15, (byte)240));
    }
}

        // health bar
        if (Health < MaxHealth)
        {
            Raylib.DrawRectangle(x - 20, y - 30, 40, 5, Color.DarkGray);
            Raylib.DrawRectangle(x - 20, y - 30, (int)(40 * ((float)Health / MaxHealth)), 5, Color.Orange);
        }
    }
}

    class Lake
    {
        public Vector2 Position;
        float rippleTimer = 0f;
        public void Update(float dt) => rippleTimer += dt;

        public Lake(Vector2 pos)
        {
            Position = pos;
        }
        public Rectangle Bounds => new Rectangle(Position.X - 100, Position.Y - 100, 200, 200);
        public void Draw()
        {
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 120, new Color(30, 100, 200, 255));

            float ripple = MathF.Sin(rippleTimer * 2f) * 6f;
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(90 + ripple), Color.SkyBlue);
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(60 + ripple * 0.5f), Color.SkyBlue);
        }
    }

class River
{
    public Vector2 Position;
    public float Length;
    public bool Vertical;
    public River(Vector2 pos, float length, bool vertical = false)
    {
        Position = pos;
        Length = length;
        Vertical = vertical;
    }
    public void Update(float dt) { }

    // horizontal: Length wide, 120 tall.  vertical: 120 wide, Length tall.
    public Rectangle Bounds => Vertical
        ? new Rectangle(Position.X, Position.Y, 120, Length)
        : new Rectangle(Position.X, Position.Y, Length, 120);
}

    class GasStation
{
    public Vector2 Pump1Pos;
    public Vector2 Pump2Pos;
    public bool Pump1Active = false;
    public bool Pump2Active = false;
    public float PumpFuelRate = 20f;
    public Rectangle BuildingBounds;
    public float OriginX;   // add these
    public float OriginY;

    public GasStation(float x, float y)
    {
        // positions relative to the station's x/y origin
        OriginX = x;
        OriginY = y;
        Pump1Pos = new Vector2(x + 180,  y - 280);
        Pump2Pos = new Vector2(x + 420, y - 280);
        BuildingBounds = new Rectangle(x + 150, y - 200, 260, 160);
    }
}
class Rideable
{
    public enum RideableType { MountainBike, BMX, Horse, Camel, Elephant, Dolphin, Reindeer, Tiger, Donkey }
    public RideableType Type;
    public Vector2 Position;
    public Vector2 SpawnPosition;
    public bool Riding = false;
    public Color RideableColor;
    public Color? RiderSkinOverride  = null;
    public Color? RiderShirtOverride = null;
    public Color? RiderPantsOverride = null;
    public float Stamina = 100f;
    public float MaxStamina = 100f;
    public bool IsAnimal => Type != RideableType.MountainBike && Type != RideableType.BMX;

    Color RiderSkin  => RiderSkinOverride  ?? Program.player.SkinColor;
    Color RiderShirt => RiderShirtOverride ?? Program.player.ShirtColor;
    Color RiderPants => RiderPantsOverride ?? Program.player.PantsColor;
    float speed;
    public Vector2 velocity = Vector2.Zero;
    public enum FacingDirection { Down, Up, Left, Right }
    public FacingDirection Facing = FacingDirection.Down;
    float gallopTimer = 0f;
    float gallopInterval = 0.3f;

    float animTimer = 0f;
    bool animFrame = false;
    bool isMoving = false;
    float dustTimer = 0f;
    List<(Vector2 pos, float life, float maxLife)> dustTrail = new();

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 50);

    public Rideable(Vector2 pos, RideableType type, Color color)
    {
        Position = pos;
        SpawnPosition = pos;
        Type = type;
        RideableColor = color;
        Facing = FacingDirection.Right;

        speed = type switch
        {
            RideableType.MountainBike => 400f,
            RideableType.BMX          => 500f,
            RideableType.Horse        => 1000f,
            RideableType.Camel        => 700f,
            RideableType.Elephant     => 450f,
            RideableType.Dolphin      => 900f,
            RideableType.Reindeer     => 850f,
            RideableType.Tiger        => 1100f,
            RideableType.Donkey       => 600f,
            _                         => 400f
        };
    }

    public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> vehicles, List<Rideable> allRideables, List<DecorativeBuilding> decorativeBuildings, List<DecorativeAsset> decorativeAssets)
    {
        if (!Riding) return;

        Vector2 move = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

        if (move != Vector2.Zero)
        {
            move = Vector2.Normalize(move);

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

        if (move != Vector2.Zero && Type == RideableType.Horse)
{
    gallopTimer -= dt;
    if (gallopTimer <= 0f)
    {
        Raylib.SetSoundVolume(Program.soundHorseGallop, Program.soundVolume * 0.3f);
        Raylib.PlaySound(Program.soundHorseGallop);
        gallopTimer = gallopInterval;
    }
}
else
{
    gallopTimer = 0f;
}

        bool onRoad = Program.IsOnRoad(Position);
        float speedMult = Type == RideableType.Horse
            ? (onRoad ? 1.1f : 0.85f)
            : (onRoad ? 1f   : 0.5f);
        
        if (IsAnimal)
        {
            if (isMoving) Stamina = MathF.Max(0f, Stamina - dt * 1.6f);
            else          Stamina = MathF.Min(MaxStamina, Stamina + dt * 0.8f);
            if (Stamina <= 0f) speedMult *= 0.35f;
        }

        Vector2 target = move * speed * speedMult;
        velocity = Vector2.Lerp(velocity, target, dt * 6f);

        if (isMoving)
        {
            animTimer += dt * (Type == RideableType.Horse ? 10f : 7f);
            if (animTimer >= 1f) { animTimer = 0f; animFrame = !animFrame; }
        }

        dustTimer -= dt;
            if (dustTimer <= 0f && isMoving)
            {
                dustTimer = 0.07f;
                dustTrail.Add((new Vector2(Position.X + 30, Position.Y + 48), 0.4f, 0.4f));
            }
        for (int i = dustTrail.Count - 1; i >= 0; i--)
        {
            var d = dustTrail[i]; d.life -= dt; dustTrail[i] = d;
            if (d.life <= 0) dustTrail.RemoveAt(i);
        }

        Vector2 oldPos = Position;
        Position += velocity * dt;

        if (Type == RideableType.Dolphin)
        {
            if (!Program.IsInWater(Position)) { Position = oldPos; velocity = Vector2.Zero; }
        }
        else if (Program.IsWaterAt(Position))
        {
            Position = oldPos; velocity = Vector2.Zero;
        }

        foreach (Building b in buildings)
            if (Raylib.CheckCollisionRecs(Bounds, new Rectangle(b.Bounds.X, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height -40)))
            { Position = oldPos; velocity = Vector2.Zero; }

        foreach (DecorativeBuilding b in decorativeBuildings)
            if (Raylib.CheckCollisionRecs(Bounds, new Rectangle(b.Bounds.X, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height -40)))
            { Position = oldPos; velocity = Vector2.Zero; }

        foreach (DecorativeAsset asset in decorativeAssets)
            {
                if (Raylib.CheckCollisionRecs(Bounds, asset.Bounds))
                {
                    Position = oldPos; velocity = Vector2.Zero;
                }
            }

        foreach (TreeObject t in trees)
            if (!t.Chopped && Raylib.CheckCollisionRecs(Bounds, t.Bounds))
            { Position = oldPos; velocity *= -0.2f; }

        foreach (Vehicle v in vehicles)
            if (Raylib.CheckCollisionRecs(Bounds, v.Bounds))
            { Position = oldPos; velocity *= -0.3f; }

        foreach (Rideable r in allRideables)
        {
            if (r == this) continue;
            if (Raylib.CheckCollisionRecs(Bounds, r.Bounds))
            { Position = oldPos; velocity = Vector2.Zero; }
        }
    }

    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        if (IsAnimal && (Riding || Stamina < MaxStamina))
        {
            int bx = (int)Position.X + 5, by = (int)Position.Y - 14;
            Raylib.DrawRectangle(bx, by, 50, 6, new Color((byte)30,(byte)30,(byte)30,(byte)200));
            Raylib.DrawRectangle(bx, by, (int)(50 * Stamina / MaxStamina), 6, Stamina > 30f ? Color.Yellow : Color.Red);
            Raylib.DrawRectangleLines(bx, by, 50, 6, Color.Black);
        }

        switch (Type)
        {
            case RideableType.MountainBike: DrawMountainBike(x, y); break;
            case RideableType.BMX:          DrawBMX(x, y);          break;
            case RideableType.Horse:        DrawHorse(x, y);        break;
            case RideableType.Camel:        DrawCamel(x, y);        break;
            case RideableType.Elephant:     DrawElephant(x, y);     break;
            case RideableType.Dolphin:      DrawDolphin(x, y);      break;
            case RideableType.Reindeer:     DrawReindeer(x, y);     break;
            case RideableType.Tiger:        DrawTiger(x, y);        break;
            case RideableType.Donkey:       DrawDonkey(x, y);       break;
        }
        foreach (var d in dustTrail)
        {
            float t = d.life / d.maxLife;
            byte alpha = (byte)(t * 100);
            int r = (int)(3 + (1f - t) * 8);
            Raylib.DrawCircle((int)d.pos.X, (int)d.pos.Y, r,
                new Color((byte)180, (byte)160, (byte)120, alpha));
        }
        // gallop speed lines for fast animals
        bool isFastAnimal = Type == RideableType.Horse || Type == RideableType.Tiger || Type == RideableType.Reindeer;
        if (isMoving && isFastAnimal)
        {
            for (int i = 0; i < 3; i++)
            {
                float ox = (Facing == FacingDirection.Right ? -1 : 1) * (12 + i * 10);
                Raylib.DrawLine(
                    (int)(Position.X + 30 + ox), (int)(Position.Y + 20 + i * 7),
                    (int)(Position.X + 30 + ox + (Facing == FacingDirection.Right ? -20 : 20)),
                    (int)(Position.Y + 20 + i * 7),
                    new Color((byte)255, (byte)255, (byte)200, (byte)(60 - i * 15)));
            }
        }
    }

 static bool RiderArmorColor(string piece, out Color col)
    {
        col = Color.White;
        if (string.IsNullOrEmpty(piece)) return false;
        foreach (var mat in Program.armorMaterials)
            if (piece.StartsWith(mat)) { col = Program.MaterialColor(mat); return true; }
        return false;
    }

    // ─── SHARED RIDER DRAWING ───────────────────────────────────────────────
    void DrawRider(int rx, int ry, Color skinColor, Color shirtColor, Color pantsColor)
    {
        switch (Facing)
        {
            case FacingDirection.Down:
                // head
                Raylib.DrawCircle(rx + 20, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 15, ry + 8, 2, Color.Black);
                Raylib.DrawCircle(rx + 25, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 14, ry + 14, 12, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));
                // body
                Raylib.DrawRectangle(rx + 10, ry + 20, 20, 20, shirtColor);
                // arms out to sides gripping bars
                Raylib.DrawRectangle(rx,      ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 30, ry + 22, 10, 8, skinColor);
                // legs bent down on pedals/stirrups
                Raylib.DrawRectangle(rx + 10, ry + 40, 8, 12, pantsColor);
                Raylib.DrawRectangle(rx + 22, ry + 40, 8, 12, pantsColor);
                break;

            case FacingDirection.Up:
                // back of head
                Raylib.DrawCircle(rx + 20, ry + 10, 11,
                    new Color((byte)Math.Max(0, skinColor.R - 30),
                              (byte)Math.Max(0, skinColor.G - 30),
                              (byte)Math.Max(0, skinColor.B - 30), (byte)255));
                Raylib.DrawRectangle(rx + 10, ry + 2, 20, 8, new Color((byte)60,(byte)40,(byte)20,(byte)255));
                // body back
                Raylib.DrawRectangle(rx + 10, ry + 20, 20, 20, shirtColor);
                Raylib.DrawRectangle(rx,      ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 30, ry + 22, 10, 8, skinColor);
                Raylib.DrawRectangle(rx + 10, ry + 40, 8, 12, pantsColor);
                Raylib.DrawRectangle(rx + 22, ry + 40, 8, 12, pantsColor);
                break;

            case FacingDirection.Left:
                // side profile head
                Raylib.DrawCircle(rx + 16, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 10, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 6, ry + 13, 4, 3,
                    new Color((byte)Math.Max(0, skinColor.R - 20),
                              (byte)Math.Max(0, skinColor.G - 20),
                              (byte)Math.Max(0, skinColor.B - 20), (byte)255));
                // body leaning forward
                Raylib.DrawRectangle(rx + 8, ry + 20, 14, 20, shirtColor);
                // front arm reaching handlebar
                int armSwingL = isMoving ? (animFrame ? 3 : -3) : 0;
                Raylib.DrawRectangle(rx + 18, ry + 22 + armSwingL, 10, 7, skinColor);
                // legs alternating on pedals
                if (isMoving)
                {
                    Raylib.DrawRectangle(rx + 6,  ry + 40,     8, animFrame ? 14 : 10, pantsColor);
                    Raylib.DrawRectangle(rx + 16, ry + 40, 8, animFrame ? 10 : 14, pantsColor);
                }
                else
                {
                    Raylib.DrawRectangle(rx + 6,  ry + 40, 8, 12, pantsColor);
                    Raylib.DrawRectangle(rx + 16, ry + 40, 8, 12, pantsColor);
                }
                break;

            case FacingDirection.Right:
                Raylib.DrawCircle(rx + 24, ry + 10, 11, skinColor);
                Raylib.DrawCircle(rx + 30, ry + 8, 2, Color.Black);
                Raylib.DrawRectangle(rx + 30, ry + 13, 4, 3,
                    new Color((byte)Math.Max(0, skinColor.R - 20),
                              (byte)Math.Max(0, skinColor.G - 20),
                              (byte)Math.Max(0, skinColor.B - 20), (byte)255));
                Raylib.DrawRectangle(rx + 18, ry + 20, 14, 20, shirtColor);
                int armSwingR = isMoving ? (animFrame ? 3 : -3) : 0;
                Raylib.DrawRectangle(rx + 12, ry + 22 + armSwingR, 10, 7, skinColor);
                if (isMoving)
                {
                    Raylib.DrawRectangle(rx + 18, ry + 40,     8, animFrame ? 14 : 10, pantsColor);
                    Raylib.DrawRectangle(rx + 28, ry + 40, 8, animFrame ? 10 : 14, pantsColor);
                }
                else
                {
                    Raylib.DrawRectangle(rx + 18, ry + 40, 8, 12, pantsColor);
                    Raylib.DrawRectangle(rx + 28, ry + 40, 8, 12, pantsColor);
                }
                break;
        }
        // ── equipped armor overlay (drawn over the rider body) ──
        bool hasBody = RiderArmorColor(Program.armorBody,   out Color bodyCol);
        bool hasLegs = RiderArmorColor(Program.armorLegs,   out Color legCol);
        bool hasHelm = RiderArmorColor(Program.armorHelmet, out Color helmCol);

        switch (Facing)
        {
            case FacingDirection.Down:
            case FacingDirection.Up:
                if (hasBody) Raylib.DrawRectangle(rx + 10, ry + 20, 20, 20, bodyCol);
                if (hasLegs) {
                    Raylib.DrawRectangle(rx + 10, ry + 40, 8, 12, legCol);
                    Raylib.DrawRectangle(rx + 22, ry + 40, 8, 12, legCol);
                }
                if (hasHelm) {
                    Raylib.DrawCircle(rx + 20, ry + 10, 11, helmCol);
                    Raylib.DrawRectangle(rx + 9, ry + 9, 22, 4, helmCol); // brow band
                }
                break;

            case FacingDirection.Left:
                if (hasBody) Raylib.DrawRectangle(rx + 8, ry + 20, 14, 20, bodyCol);
                if (hasLegs) {
                    Raylib.DrawRectangle(rx + 6,  ry + 40, 8, 12, legCol);
                    Raylib.DrawRectangle(rx + 16, ry + 40, 8, 12, legCol);
                }
                if (hasHelm) Raylib.DrawCircle(rx + 16, ry + 10, 11, helmCol);
                break;

            case FacingDirection.Right:
                if (hasBody) Raylib.DrawRectangle(rx + 18, ry + 20, 14, 20, bodyCol);
                if (hasLegs) {
                    Raylib.DrawRectangle(rx + 18, ry + 40, 8, 12, legCol);
                    Raylib.DrawRectangle(rx + 28, ry + 40, 8, 12, legCol);
                }
                if (hasHelm) Raylib.DrawCircle(rx + 24, ry + 10, 11, helmCol);
                break;
        }
    }

    // ─── MOUNTAIN BIKE ──────────────────────────────────────────────────────
    void DrawMountainBike(int x, int y)
    {
        switch (Facing)
        {
            case FacingDirection.Left:
            case FacingDirection.Right:
                bool flipped = Facing == FacingDirection.Left;
                int fx = flipped ? x + 60 : x;
                int fd = flipped ? -1 : 1;

                // wheels
                Raylib.DrawCircle(x + 12, y + 42, 14, Color.DarkGray);
                Raylib.DrawCircleLines(x + 12, y + 42, 14, Color.Gray);
                Raylib.DrawCircle(x + 12, y + 42, 4, Color.Gray);
                Raylib.DrawCircle(x + 50, y + 42, 14, Color.DarkGray);
                Raylib.DrawCircleLines(x + 50, y + 42, 14, Color.Gray);
                Raylib.DrawCircle(x + 50, y + 42, 4, Color.Gray);

                // frame
                Raylib.DrawLine(x + 12, y + 42, x + 28, y + 22, RideableColor);
                Raylib.DrawLine(x + 28, y + 22, x + 50, y + 42, RideableColor);
                Raylib.DrawLine(x + 12, y + 42, x + 36, y + 22, RideableColor);
                Raylib.DrawLine(x + 28, y + 22, x + 36, y + 22, RideableColor);

                // seat and bars on correct side
                if (!flipped)
                {
                    Raylib.DrawRectangle(x + 22, y + 17, 18, 5, Color.Black);
                    Raylib.DrawRectangle(x + 44, y + 14, 12, 5, Color.DarkGray);
                    Raylib.DrawRectangle(x + 52, y + 10, 4, 14, Color.DarkGray);
                }
                else
                {
                    Raylib.DrawRectangle(x + 20, y + 17, 18, 5, Color.Black);
                    Raylib.DrawRectangle(x + 4,  y + 14, 12, 5, Color.DarkGray);
                    Raylib.DrawRectangle(x + 4,  y + 10, 4, 14, Color.DarkGray);
                }

                // suspension fork
                Raylib.DrawLine(x + 46, y + 20, x + 50, y + 42, Color.LightGray);

                if (Riding) DrawRider(x + 8, y - 16, RiderSkin, RiderShirt, RiderPants);
                break;

            case FacingDirection.Down:
            case FacingDirection.Up:
                // single centre wheel
                Raylib.DrawRectangle(x + 27, y + 42, 6, 24, Color.LightGray);
                // frame coming down to wheel
                Raylib.DrawLine(x + 30, y + 20, x + 30, y + 30, RideableColor);
                // handlebars
                Raylib.DrawRectangle(x + 10, y + 18, 40, 5, Color.DarkGray);
                // seat
                Raylib.DrawRectangle(x + 22, y + 30, 15, 10, Color.Black);

                if (Riding) DrawRider(x + 8, y - 18, RiderSkin, RiderShirt, RiderPants);
                break;
                    }
                }

    // ─── BMX ────────────────────────────────────────────────────────────────
    void DrawBMX(int x, int y)
    {
        switch (Facing)
        {
            case FacingDirection.Left:
            case FacingDirection.Right:
                bool flipped = Facing == FacingDirection.Left;

                Raylib.DrawCircle(x + 10, y + 42, 11, Color.Black);
                Raylib.DrawCircleLines(x + 10, y + 42, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 10, y + 42, 3, Color.DarkGray);
                Raylib.DrawCircle(x + 46, y + 42, 11, Color.Black);
                Raylib.DrawCircleLines(x + 46, y + 42, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 46, y + 42, 3, Color.DarkGray);

                Raylib.DrawLine(x + 10, y + 42, x + 26, y + 24, RideableColor);
                Raylib.DrawLine(x + 26, y + 24, x + 46, y + 42, RideableColor);
                Raylib.DrawLine(x + 10, y + 42, x + 32, y + 24, RideableColor);
                Raylib.DrawLine(x + 26, y + 24, x + 32, y + 24, RideableColor);

                // pegs
                Raylib.DrawRectangle(x + 2,  y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 18, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 38, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 54, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));

                Raylib.DrawRectangle(x + 20, y + 19, 14, 5, Color.Black);

                if (!flipped)
                {
                    Raylib.DrawRectangle(x + 36, y + 12, 18, 4, Color.DarkGray);
                    Raylib.DrawRectangle(x + 42, y + 12, 4, 12, Color.DarkGray);
                }
                else
                {
                    Raylib.DrawRectangle(x + 6,  y + 12, 18, 4, Color.DarkGray);
                    Raylib.DrawRectangle(x + 6,  y + 12, 4, 12, Color.DarkGray);
                }

                if (Riding) DrawRider(x + 8, y - 16, RiderSkin, RiderShirt, RiderPants);
                break;

           case FacingDirection.Down:
            case FacingDirection.Up:
                // single centre wheel
                Raylib.DrawRectangle(x + 27, y + 42, 6, 24, Color.Black);
                
                // frame
                Raylib.DrawLine(x + 30, y + 22, x + 30, y + 32, RideableColor);
                // flat BMX bars
                Raylib.DrawRectangle(x + 8, y + 16, 44, 4, Color.DarkGray);
                // seat
                Raylib.DrawRectangle(x + 22, y + 30, 15, 10, Color.Brown);
                // pegs front-on — just left and right of the single wheel
                Raylib.DrawRectangle(x + 16, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));
                Raylib.DrawRectangle(x + 38, y + 40, 6, 4, new Color((byte)192,(byte)192,(byte)192,(byte)255));

                if (Riding) DrawRider(x + 8, y - 16, RiderSkin, RiderShirt, RiderPants);
                break;
        }
    }

    // ─── HORSE ──────────────────────────────────────────────────────────────
    void DrawHorse(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 6 : -6) : 0;

        switch (Facing)
        {
            case FacingDirection.Right:
                // body
                Raylib.DrawRectangle(x + 8,  y + 20, 44, 22, RideableColor);
                // neck + head
                Raylib.DrawRectangle(x + 42, y + 10, 12, 20, RideableColor);
                Raylib.DrawRectangle(x + 46, y + 2,  16, 14, RideableColor);
                Raylib.DrawTriangle(new Vector2(x+52,y+2),new Vector2(x+56,y-6),new Vector2(x+60,y+2), RideableColor);
                Raylib.DrawCircle(x + 58, y + 6, 2, Color.Black);
                Raylib.DrawCircle(x + 61, y + 11, 2, new Color((byte)Math.Max(0,RideableColor.R-40),(byte)Math.Max(0,RideableColor.G-40),(byte)Math.Max(0,RideableColor.B-40),(byte)255));
                // mane + tail
                Raylib.DrawRectangle(x + 44, y + 4, 6, 18, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 8, y + 24, x - 4, y + 40, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 8, y + 24, x - 2, y + 44, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                // legs
                Raylib.DrawRectangle(x + 12, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 24, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 44, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 11, y + 56 + leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 23, y + 56 - leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 + leg,  10, 5, Color.Black);
                Raylib.DrawRectangle(x + 43, y + 56 - leg,  10, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 20, y + 17, 22, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                Raylib.DrawRectangle(x + 22, y + 15, 18, 4, new Color((byte)130,(byte)80,(byte)30,(byte)255));
                if (Riding) DrawRider(x + 12, y - 20, RiderSkin, RiderShirt, RiderPants);
                break;

            case FacingDirection.Left:
                // mirrored body
                Raylib.DrawRectangle(x + 8,  y + 20, 44, 22, RideableColor);
                Raylib.DrawRectangle(x + 6,  y + 10, 12, 20, RideableColor);
                Raylib.DrawRectangle(x - 2,  y + 2,  16, 14, RideableColor);
                Raylib.DrawTriangle(new Vector2(x+8,y+2),new Vector2(x+4,y-6),new Vector2(x,y+2), RideableColor);
                Raylib.DrawCircle(x + 2,  y + 6,  2, Color.Black);
                Raylib.DrawCircle(x - 1,  y + 11, 2, new Color((byte)Math.Max(0,RideableColor.R-40),(byte)Math.Max(0,RideableColor.G-40),(byte)Math.Max(0,RideableColor.B-40),(byte)255));
                Raylib.DrawRectangle(x + 10, y + 4, 6, 18, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 52, y + 24, x + 64, y + 40, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawLine(x + 52, y + 24, x + 62, y + 44, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 12, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 24, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 8, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 44, y + 40, 8, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 11, y + 56 + leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 23, y + 56 - leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 + leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 43, y + 56 - leg, 10, 5, Color.Black);
                Raylib.DrawRectangle(x + 18, y + 17, 22, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                Raylib.DrawRectangle(x + 20, y + 15, 18, 4, new Color((byte)130,(byte)80,(byte)30,(byte)255));
                if (Riding) DrawRider(x + 12, y - 20, RiderSkin, RiderShirt, RiderPants);
                break;

            case FacingDirection.Down:
                // front-on horse face
                Raylib.DrawRectangle(x + 10, y + 18, 40, 24, RideableColor);
                // head front-on
                Raylib.DrawRectangle(x + 18, y + 2,  24, 20, RideableColor);
                Raylib.DrawCircle(x + 20, y + 10, 3, Color.Black); // left eye
                Raylib.DrawCircle(x + 40, y + 10, 3, Color.Black); // right eye
                Raylib.DrawRectangle(x + 26, y + 16, 8, 6,
                    new Color((byte)Math.Max(0,RideableColor.R-30),(byte)Math.Max(0,RideableColor.G-30),(byte)Math.Max(0,RideableColor.B-30),(byte)255)); // muzzle
                // ears
                Raylib.DrawTriangle(new Vector2(x+18,y+2),new Vector2(x+14,y-6),new Vector2(x+22,y+2), RideableColor);
                Raylib.DrawTriangle(new Vector2(x+42,y+2),new Vector2(x+42,y+2),new Vector2(x+46,y-6), RideableColor);
                // front legs
                Raylib.DrawRectangle(x + 14, y + 40, 10, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 10, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 13, y + 56 + leg, 12, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 - leg, 12, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 14, y + 15, 32, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                if (Riding) DrawRider(x + 8, y - 20, RiderSkin, RiderShirt, RiderPants);
                break;

            case FacingDirection.Up:
                // rear-on horse
                Raylib.DrawRectangle(x + 10, y + 18, 40, 24, RideableColor);
                // rump
                Raylib.DrawRectangle(x + 14, y + 10, 32, 12, RideableColor);
                // tail centre
                Raylib.DrawRectangle(x + 26, y + 2, 8, 20, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 20, y + 14, 6, 12, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                Raylib.DrawRectangle(x + 34, y + 14, 6, 12, new Color((byte)60,(byte)40,(byte)10,(byte)255));
                // rear legs
                Raylib.DrawRectangle(x + 14, y + 40, 10, 18 + leg, Color.Brown);
                Raylib.DrawRectangle(x + 36, y + 40, 10, 18 - leg, Color.Brown);
                Raylib.DrawRectangle(x + 13, y + 56 + leg, 12, 5, Color.Black);
                Raylib.DrawRectangle(x + 35, y + 56 - leg, 12, 5, Color.Black);
                // saddle
                Raylib.DrawRectangle(x + 14, y + 15, 32, 8, new Color((byte)100,(byte)60,(byte)20,(byte)255));
                if (Riding) DrawRider(x + 8, y - 20, RiderSkin, RiderShirt, RiderPants);
                break;
        }
    }
    // ─── CAMEL ──────────────────────────────────────────────────────────────
    void DrawCamel(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 5 : -5) : 0;
        Color sand = RideableColor;
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;
        int dir = Facing == FacingDirection.Left ? -1 : 1;

        if (side)
        {
            int hx = Facing == FacingDirection.Left ? x + 6 : x + 46;
            // body
            Raylib.DrawRectangle(x + 8, y + 22, 44, 20, sand);
            // two humps
            Raylib.DrawCircle(x + 24, y + 20, 9, sand);
            Raylib.DrawCircle(x + 38, y + 20, 9, sand);
            // neck + head
            Raylib.DrawRectangle(hx, y + 4, 8, 22, sand);
            Raylib.DrawRectangle(hx - 4 * dir, y + 2, 12, 10, sand);
            Raylib.DrawCircle(hx + (Facing == FacingDirection.Left ? -2 : 6), y + 6, 2, Color.Black);
            // legs
            Raylib.DrawRectangle(x + 12, y + 40, 7, 20 + leg, sand);
            Raylib.DrawRectangle(x + 24, y + 40, 7, 20 - leg, sand);
            Raylib.DrawRectangle(x + 36, y + 40, 7, 20 + leg, sand);
            Raylib.DrawRectangle(x + 44, y + 40, 7, 20 - leg, sand);
            if (Riding) DrawRider(x + 12, y - 22, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            // front/rear: body block + single hump + two legs
            Raylib.DrawRectangle(x + 12, y + 20, 36, 22, sand);
            Raylib.DrawCircle(x + 30, y + 18, 10, sand);
            if (Facing == FacingDirection.Down)
            {
                Raylib.DrawRectangle(x + 20, y + 4, 20, 18, sand); // head
                Raylib.DrawCircle(x + 24, y + 12, 2, Color.Black);
                Raylib.DrawCircle(x + 36, y + 12, 2, Color.Black);
            }
            Raylib.DrawRectangle(x + 16, y + 40, 9, 20 + leg, sand);
            Raylib.DrawRectangle(x + 36, y + 40, 9, 20 - leg, sand);
            if (Riding) DrawRider(x + 10, y - 22, RiderSkin, RiderShirt, RiderPants);
        }
    }

    // ─── ELEPHANT ───────────────────────────────────────────────────────────
    void DrawElephant(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 4 : -4) : 0;
        Color grey = new Color((byte)140,(byte)140,(byte)150,(byte)255);
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;

        if (side)
        {
            bool left = Facing == FacingDirection.Left;
            // big body
            Raylib.DrawRectangle(x + 4, y + 16, 56, 30, grey);
            Raylib.DrawCircle(left ? x + 8 : x + 56, y + 24, 16, grey); // head end
            // ear
            Raylib.DrawCircle(left ? x + 12 : x + 52, y + 22, 8, new Color((byte)120,(byte)120,(byte)130,(byte)255));
            // trunk
            int tx = left ? x + 2 : x + 60;
            Raylib.DrawRectangle(tx - (left ? 4 : 0), y + 26, 6, 22, grey);
            // tusk
            Raylib.DrawRectangle(left ? x : x + 60, y + 34, 6, 3, new Color((byte)240,(byte)235,(byte)220,(byte)255));
            // eye
            Raylib.DrawCircle(left ? x + 10 : x + 54, y + 20, 2, Color.Black);
            // legs (thick)
            Raylib.DrawRectangle(x + 10, y + 44, 11, 18 + leg, grey);
            Raylib.DrawRectangle(x + 24, y + 44, 11, 18 - leg, grey);
            Raylib.DrawRectangle(x + 38, y + 44, 11, 18 + leg, grey);
            Raylib.DrawRectangle(x + 48, y + 44, 11, 18 - leg, grey);
            if (Riding) DrawRider(x + 16, y - 26, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            Raylib.DrawRectangle(x + 6, y + 16, 50, 30, grey);
            if (Facing == FacingDirection.Down)
            {
                Raylib.DrawCircle(x + 18, y + 22, 10, new Color((byte)120,(byte)120,(byte)130,(byte)255)); // ear
                Raylib.DrawCircle(x + 44, y + 22, 10, new Color((byte)120,(byte)120,(byte)130,(byte)255));
                Raylib.DrawRectangle(x + 27, y + 24, 8, 24, grey); // trunk
                Raylib.DrawCircle(x + 22, y + 18, 2, Color.Black);
                Raylib.DrawCircle(x + 40, y + 18, 2, Color.Black);
            }
            Raylib.DrawRectangle(x + 12, y + 44, 12, 18 + leg, grey);
            Raylib.DrawRectangle(x + 38, y + 44, 12, 18 - leg, grey);
            if (Riding) DrawRider(x + 12, y - 26, RiderSkin, RiderShirt, RiderPants);
        }
    }

    // ─── DOLPHIN ────────────────────────────────────────────────────────────
    void DrawDolphin(int x, int y)
    {
        float bob = MathF.Sin(animTimer * 6f) * 3f;
        Color blue = new Color((byte)90,(byte)140,(byte)200,(byte)255);
        Color belly = new Color((byte)220,(byte)230,(byte)240,(byte)255);
        bool left = Facing == FacingDirection.Left;
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;

        if (side)
        {
            int cx = x + 30, cy = y + 30 + (int)bob;
            // body ellipse
            Raylib.DrawEllipse(cx, cy, 26, 12, blue);
            Raylib.DrawEllipse(cx, cy + 4, 20, 6, belly);
            // snout
            int sx = left ? cx - 26 : cx + 26;
            Raylib.DrawTriangle(
                new Vector2(sx, cy),
                new Vector2(sx + (left ? 10 : -10), cy - 4),
                new Vector2(sx + (left ? 10 : -10), cy + 4), blue);
            // dorsal fin
            Raylib.DrawTriangle(
                new Vector2(cx, cy - 12),
                new Vector2(cx - 6, cy - 22),
                new Vector2(cx + 6, cy - 12), blue);
            // tail
            int tx = left ? cx + 26 : cx - 26;
            Raylib.DrawTriangle(
                new Vector2(tx, cy),
                new Vector2(tx + (left ? 10 : -10), cy - 8),
                new Vector2(tx + (left ? 10 : -10), cy + 8), blue);
            Raylib.DrawCircle(sx + (left ? 6 : -6), cy - 2, 2, Color.Black);
            if (Riding) DrawRider(x + 12, y - 14 + (int)bob, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            int cx = x + 30, cy = y + 30 + (int)bob;
            Raylib.DrawEllipse(cx, cy, 12, 22, blue);
            Raylib.DrawTriangle(
                new Vector2(cx, cy - 22),
                new Vector2(cx - 5, cy - 14),
                new Vector2(cx + 5, cy - 14), blue);
            Raylib.DrawCircle(cx - 4, cy - 16, 2, Color.Black);
            Raylib.DrawCircle(cx + 4, cy - 16, 2, Color.Black);
            if (Riding) DrawRider(x + 12, y - 14 + (int)bob, RiderSkin, RiderShirt, RiderPants);
        }
    }
    // ─── REINDEER ───────────────────────────────────────────────────────────
    void DrawReindeer(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 6 : -6) : 0;
        Color fur = RideableColor;
        Color antler = new Color((byte)150,(byte)110,(byte)60,(byte)255);
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;

        if (side)
        {
            bool left = Facing == FacingDirection.Left;
            int hx = left ? x + 6 : x + 46;
            // body
            Raylib.DrawRectangle(x + 8, y + 22, 44, 20, fur);
            // neck + head
            Raylib.DrawRectangle(hx, y + 8, 8, 18, fur);
            Raylib.DrawRectangle(hx - 4 * (left ? 1 : -1), y + 4, 12, 10, fur);
            Raylib.DrawCircle(hx + (left ? -2 : 6), y + 8, 2, Color.Black);
            // antlers
            int ax = left ? hx - 2 : hx + 6;
            Raylib.DrawLine(ax, y + 4, ax - 6, y - 8, antler);
            Raylib.DrawLine(ax - 6, y - 8, ax - 12, y - 6, antler);
            Raylib.DrawLine(ax - 6, y - 8, ax - 2, y - 14, antler);
            Raylib.DrawLine(ax, y + 4, ax + 6, y - 8, antler);
            Raylib.DrawLine(ax + 6, y - 8, ax + 12, y - 6, antler);
            Raylib.DrawLine(ax + 6, y - 8, ax + 2, y - 14, antler);
            // legs
            Raylib.DrawRectangle(x + 12, y + 40, 7, 20 + leg, fur);
            Raylib.DrawRectangle(x + 24, y + 40, 7, 20 - leg, fur);
            Raylib.DrawRectangle(x + 36, y + 40, 7, 20 + leg, fur);
            Raylib.DrawRectangle(x + 44, y + 40, 7, 20 - leg, fur);
            if (Riding) DrawRider(x + 12, y - 22, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 20, 40, 22, fur);
            if (Facing == FacingDirection.Down)
            {
                Raylib.DrawRectangle(x + 20, y + 4, 20, 18, fur); // head
                Raylib.DrawCircle(x + 24, y + 12, 2, Color.Black);
                Raylib.DrawCircle(x + 36, y + 12, 2, Color.Black);
                // antlers spread out front-on
                Raylib.DrawLine(x + 22, y + 4, x + 12, y - 8, antler);
                Raylib.DrawLine(x + 12, y - 8, x + 6, y - 4, antler);
                Raylib.DrawLine(x + 38, y + 4, x + 48, y - 8, antler);
                Raylib.DrawLine(x + 48, y - 8, x + 54, y - 4, antler);
            }
            Raylib.DrawRectangle(x + 14, y + 40, 9, 20 + leg, fur);
            Raylib.DrawRectangle(x + 36, y + 40, 9, 20 - leg, fur);
            if (Riding) DrawRider(x + 10, y - 22, RiderSkin, RiderShirt, RiderPants);
        }
    }

    // ─── TIGER ──────────────────────────────────────────────────────────────
    void DrawTiger(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 5 : -5) : 0;
        Color body = RideableColor;                                  // orange
        Color stripe = new Color((byte)30,(byte)20,(byte)10,(byte)255);
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;

        if (side)
        {
            bool left = Facing == FacingDirection.Left;
            int hx = left ? x + 4 : x + 44;
            // body
            Raylib.DrawRectangle(x + 6, y + 24, 48, 18, body);
            // stripes
            Raylib.DrawRectangle(x + 16, y + 24, 3, 18, stripe);
            Raylib.DrawRectangle(x + 26, y + 24, 3, 18, stripe);
            Raylib.DrawRectangle(x + 36, y + 24, 3, 18, stripe);
            // head
            Raylib.DrawRectangle(hx, y + 18, 16, 16, body);
            Raylib.DrawCircle(hx + (left ? 2 : 14), y + 22, 2, Color.Black);
            Raylib.DrawTriangle( // ear
                new Vector2(hx + 2, y + 18), new Vector2(hx + 4, y + 12), new Vector2(hx + 8, y + 18), body);
            // tail
            int tx = left ? x + 54 : x + 6;
            Raylib.DrawLine(tx, y + 28, tx + (left ? 12 : -12), y + 18, body);
            // legs
            Raylib.DrawRectangle(x + 12, y + 40, 7, 18 + leg, body);
            Raylib.DrawRectangle(x + 22, y + 40, 7, 18 - leg, body);
            Raylib.DrawRectangle(x + 34, y + 40, 7, 18 + leg, body);
            Raylib.DrawRectangle(x + 44, y + 40, 7, 18 - leg, body);
            if (Riding) DrawRider(x + 12, y - 20, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            Raylib.DrawRectangle(x + 10, y + 22, 40, 20, body);
            // stripes
            Raylib.DrawRectangle(x + 18, y + 22, 3, 20, stripe);
            Raylib.DrawRectangle(x + 28, y + 22, 3, 20, stripe);
            Raylib.DrawRectangle(x + 38, y + 22, 3, 20, stripe);
            if (Facing == FacingDirection.Down)
            {
                Raylib.DrawRectangle(x + 20, y + 6, 20, 18, body); // head
                Raylib.DrawCircle(x + 25, y + 14, 2, Color.Black);
                Raylib.DrawCircle(x + 35, y + 14, 2, Color.Black);
                Raylib.DrawTriangle(new Vector2(x+20,y+6), new Vector2(x+18,y),  new Vector2(x+26,y+6), body); // ears
                Raylib.DrawTriangle(new Vector2(x+34,y+6), new Vector2(x+42,y),  new Vector2(x+40,y+6), body);
                Raylib.DrawRectangle(x + 27, y + 18, 6, 4, new Color((byte)240,(byte)230,(byte)220,(byte)255)); // muzzle
            }
            Raylib.DrawRectangle(x + 14, y + 40, 9, 18 + leg, body);
            Raylib.DrawRectangle(x + 36, y + 40, 9, 18 - leg, body);
            if (Riding) DrawRider(x + 10, y - 20, RiderSkin, RiderShirt, RiderPants);
        }
    }
    // DONKEY
    void DrawDonkey(int x, int y)
    {
        int leg = isMoving ? (animFrame ? 5 : -5) : 0;
        Color grey  = RideableColor;
        Color belly = new Color((byte)Math.Min(255,RideableColor.R+40),(byte)Math.Min(255,RideableColor.G+40),(byte)Math.Min(255,RideableColor.B+40),(byte)255);
        Color dark  = new Color((byte)Math.Max(0,RideableColor.R-50),(byte)Math.Max(0,RideableColor.G-50),(byte)Math.Max(0,RideableColor.B-50),(byte)255);
        bool side = Facing == FacingDirection.Left || Facing == FacingDirection.Right;
        int dir = Facing == FacingDirection.Left ? -1 : 1;

        if (side)
        {
            int hx = dir == -1 ? x + 4 : x + 44;
            // compact body, lower than horse
            Raylib.DrawRectangle(x + 10, y + 26, 40, 18, grey);
            Raylib.DrawRectangle(x + 14, y + 38, 32, 6, belly);            // light belly
            // short neck + head
            Raylib.DrawRectangle(hx, y + 16, 10, 16, grey);
            Raylib.DrawRectangle(hx - 4 * dir, y + 10, 14, 12, grey);
            Raylib.DrawRectangle(hx + (dir == -1 ? -8 : 12), y + 15, 6, 6, belly); // pale muzzle
            Raylib.DrawCircle(hx + (dir == -1 ? 2 : 8), y + 14, 2, Color.Black);
            // BIG donkey ears (two, angled back)
            Raylib.DrawRectangle(hx + 2, y - 2, 4, 14, grey);
            Raylib.DrawRectangle(hx + 8, y - 4, 4, 16, grey);
            // dorsal stripe + tail with tuft
            Raylib.DrawRectangle(x + 12, y + 26, 36, 3, dark);
            int tx = dir == -1 ? x + 50 : x + 10;
            Raylib.DrawLine(tx, y + 30, tx - 8 * dir, y + 44, dark);
            Raylib.DrawCircle(tx - 8 * dir, y + 45, 3, dark);              // tail tuft
            // short stubby legs
            Raylib.DrawRectangle(x + 14, y + 42, 7, 14 + leg, dark);
            Raylib.DrawRectangle(x + 24, y + 42, 7, 14 - leg, dark);
            Raylib.DrawRectangle(x + 34, y + 42, 7, 14 + leg, dark);
            Raylib.DrawRectangle(x + 42, y + 42, 7, 14 - leg, dark);
            // saddle blanket
            Raylib.DrawRectangle(x + 20, y + 22, 22, 7, new Color((byte)160,(byte)40,(byte)40,(byte)255));
            if (Riding) DrawRider(x + 14, y - 14, RiderSkin, RiderShirt, RiderPants);
        }
        else
        {
            Raylib.DrawRectangle(x + 12, y + 22, 36, 22, grey);
            if (Facing == FacingDirection.Down)
            {
                Raylib.DrawRectangle(x + 19, y + 6, 22, 18, grey);         // head front-on
                Raylib.DrawCircle(x + 24, y + 13, 2, Color.Black);
                Raylib.DrawCircle(x + 36, y + 13, 2, Color.Black);
                Raylib.DrawRectangle(x + 26, y + 18, 8, 5, belly);         // muzzle
                Raylib.DrawRectangle(x + 16, y - 8, 5, 16, grey);          // tall ears
                Raylib.DrawRectangle(x + 39, y - 8, 5, 16, grey);
            }
            else
            {
                Raylib.DrawRectangle(x + 27, y + 8, 6, 16, dark);          // tail from behind
                Raylib.DrawRectangle(x + 16, y - 4, 5, 12, grey);
                Raylib.DrawRectangle(x + 39, y - 4, 5, 12, grey);
            }
            Raylib.DrawRectangle(x + 16, y + 42, 8, 14 + leg, dark);
            Raylib.DrawRectangle(x + 36, y + 42, 8, 14 - leg, dark);
            Raylib.DrawRectangle(x + 20, y + 18, 20, 7, new Color((byte)160,(byte)40,(byte)40,(byte)255));
            if (Riding) DrawRider(x + 10, y - 16, RiderSkin, RiderShirt, RiderPants);
        }
    }
}

}
