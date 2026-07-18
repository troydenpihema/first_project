using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    class Vehicle
{
    public enum VehicleType { Sedan, Truck, SUV, PoliceCar, Ambulance, FireTruck, Ute, MonsterTruck,
    Convertible, MuscleCar, RacingCar, Boat, Jetski, Dinghy, SpeedBoat, Canoe, Yacht, CruiseShip }
    public bool IsWatercraft => Type is VehicleType.Boat or VehicleType.Jetski or VehicleType.Dinghy
    or VehicleType.SpeedBoat or VehicleType.Canoe or VehicleType.Yacht or VehicleType.CruiseShip;
    public VehicleType Type;
    public Vector2 Position;
    public bool HeadlightsOn = false;
    public bool Driving = false;
    public bool OnRoad = false;
    public float Fuel = 100f;
    public float MaxFuel = 100f;
    public bool NeedsPayment = false;
    public float FuelPumped = 0f;
    public bool FuelLocked = false;
    public float TopSpeed => speed;
    float speed;
    public Color VehicleColor;
    public Vector2 velocity = Vector2.Zero;

    public enum FacingDirection { Down, Up, Left, Right }
    public FacingDirection Facing = FacingDirection.Right;
    public Vector2 Center
    {
        get { Rectangle b = Bounds; return new Vector2(b.X + b.Width / 2f, b.Y + b.Height / 2f); }
    }

    float animTimer = 0f;
    bool animFrame = false;
    bool isMoving = false;
    float wheelAngle = 0f;
    float exhaustTimer = 0f;
    List<(Vector2 pos, float life, float maxLife)> exhaustParticles = new();
    float wakeTimer = 0f;
    List<(Vector2 pos, Vector2 vel, float life, float maxLife)> wakeParticles = new();
    public Rectangle Bounds => Type switch
{
    VehicleType.Jetski or VehicleType.Canoe => new Rectangle(Position.X, Position.Y, 70, 40),
    VehicleType.Yacht      => new Rectangle(Position.X, Position.Y, 160, 70),
    VehicleType.CruiseShip => new Rectangle(Position.X, Position.Y, 280, 110),
    _ => new Rectangle(Position.X, Position.Y, 100, 50)
};

    public Vehicle(Vector2 pos, Color vehicleColor, float vehicleSpeed, VehicleType type = VehicleType.Sedan)
    {
        Position = pos;
        VehicleColor = vehicleColor;
        speed = vehicleSpeed;
        Type = type;
        Facing = FacingDirection.Right;
    }

    public void Update(float dt, List<Building> buildings, List<TreeObject> trees, List<Vehicle> allVehicles, List<DecorativeBuilding> decorativeBuildings, List<RockObject> rocks, List<DecorativeAsset> decorativeAssets)
    {
        for (int i = wakeParticles.Count - 1; i >= 0; i--)
        {
            var p = wakeParticles[i];
            p.life -= dt;
            p.pos += p.vel * dt;
            if (p.life <= 0) wakeParticles.RemoveAt(i);
            else wakeParticles[i] = p;
        }

        for (int i = exhaustParticles.Count - 1; i >= 0; i--)
        {
            var ep = exhaustParticles[i];
            ep.life -= dt;
            exhaustParticles[i] = ep;
            if (ep.life <= 0) exhaustParticles.RemoveAt(i);
        }

        if (!Driving) return;
        if (Raylib.IsKeyPressed(KeyboardKey.L)) HeadlightsOn = !HeadlightsOn;
        if (Raylib.IsKeyPressed(KeyboardKey.H)) Program.PlayHorn(Type);
        if (FuelLocked)
        {
            velocity = Vector2.Zero;
            return;
        }

        Vector2 move = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

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

        if (move != Vector2.Zero && Fuel > 0)
            Fuel = Math.Max(0, Fuel - dt * 2f);

        if (Fuel <= 0) move = Vector2.Zero;

        float speedMultiplier = OnRoad ? 1f + (Program.player.DrivingLevel * 0.01f) : 0.2f;
        Vector2 targetVelocity = move * speed * speedMultiplier;
        velocity = Vector2.Lerp(velocity, targetVelocity, dt * (OnRoad ? 5f : 1.2f));

        if (isMoving)
        {
            animTimer += dt * 8f;
            if (animTimer >= 1f) { animTimer = 0f; animFrame = !animFrame; }
            float spd = velocity.Length();
            wheelAngle += spd * dt * 0.06f; // rotate proportional to speed
            exhaustTimer -= dt;
            if (exhaustTimer <= 0f && !IsWatercraft)
            {
                exhaustTimer = 0.08f;
                Vector2 exhaustPos = Facing == FacingDirection.Right
                    ? new Vector2(Position.X - 4, Position.Y + 30)
                    : new Vector2(Position.X + 104, Position.Y + 30);
                exhaustParticles.Add((exhaustPos, 0.5f, 0.5f));
            }
        }

        Vector2 oldPos = Position;
        Position += velocity * dt;

        Vector2 hullCenter = new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
        if (IsWatercraft)
        {
            if (!Program.IsInWater(hullCenter)) { Position = oldPos; velocity = Vector2.Zero; }
        }
        else if (Program.IsWaterAt(hullCenter))
        {
            Position = oldPos; velocity = Vector2.Zero;
        }

        bool skidding = !IsWatercraft && !OnRoad && isMoving && velocity.Length() > 60f;
        if ((IsWatercraft && isMoving && velocity.Length() > 40f) || skidding)
        {
            wakeTimer += dt;
            if (wakeTimer >= 0.07f)
            {
                wakeTimer = 0f;
                Rectangle wb = Bounds;
                Vector2 stern = Facing switch
                {
                    FacingDirection.Right => new Vector2(wb.X,                 wb.Y + wb.Height / 2),
                    FacingDirection.Left  => new Vector2(wb.X + wb.Width,     wb.Y + wb.Height / 2),
                    FacingDirection.Up    => new Vector2(wb.X + wb.Width / 2, wb.Y + wb.Height),
                    _                     => new Vector2(wb.X + wb.Width / 2, wb.Y),
                };
                // slight sideways jitter so the trail isn't a razor line
                Vector2 side = Facing is FacingDirection.Left or FacingDirection.Right
                    ? new Vector2(0, Raylib.GetRandomValue(-8, 8))
                    : new Vector2(Raylib.GetRandomValue(-8, 8), 0);
                wakeParticles.Add((stern + side, -Vector2.Normalize(velocity) * 25f, 0.9f, 0.9f));
                if (wakeParticles.Count > 40) wakeParticles.RemoveAt(0);   // cruise ship spam guard
            }
        }

        foreach (Building building in buildings)
        {
            Rectangle collisionBox = new Rectangle(
                building.Bounds.X, building.Bounds.Y,
                building.Bounds.Width, building.Bounds.Height);
            if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
            { Position = oldPos; velocity = Vector2.Zero; }
        }

         foreach (DecorativeBuilding decorativeBuilding in decorativeBuildings)
        {
            Rectangle collisionBox = new Rectangle(
                decorativeBuilding.Bounds.X, decorativeBuilding.Bounds.Y,
                decorativeBuilding.Bounds.Width, decorativeBuilding.Bounds.Height);
            if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
            { Position = oldPos; velocity = Vector2.Zero; }
        }

        foreach (DecorativeAsset asset in decorativeAssets)
        {
            if (Raylib.CheckCollisionRecs(Bounds, asset.Bounds))
            {
                Position = oldPos; velocity = Vector2.Zero;
            }
        }


        foreach (TreeObject tree in trees)
            if (!tree.Chopped && Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            { Position = oldPos; velocity *= -0.3f; }

        foreach (RockObject rock in rocks)
            if (!rock.Broken && Raylib.CheckCollisionRecs(Bounds, rock.Bounds))
            { Position = oldPos; velocity *= -0.3f; }

        foreach (Vehicle other in allVehicles)
        {
            if (other == this) continue;
            if (Raylib.CheckCollisionRecs(Bounds, other.Bounds))
            { Position = oldPos; velocity *= -0.5f; }
        }
    }

    public void Refuel(float amount)
    {
        float actualAmount = Math.Min(amount, MaxFuel - Fuel);
        FuelPumped += actualAmount;
        Fuel = Math.Min(MaxFuel, Fuel + actualAmount);
    }

    public void Draw()
    {
        switch (Type)
        {
            case VehicleType.Sedan: DrawSedan(); break;
            case VehicleType.Truck: DrawTruck(); break;
            case VehicleType.SUV:   DrawSUV();   break;
            case VehicleType.PoliceCar:   DrawPoliceCar();   break;
            case VehicleType.Ambulance:   DrawAmbulance();   break;
            case VehicleType.FireTruck:   DrawFireTruck();   break;
            case VehicleType.Ute:          DrawUte();          break;
            case VehicleType.MonsterTruck: DrawMonsterTruck(); break;
            case VehicleType.Convertible:  DrawConvertible();  break;
            case VehicleType.MuscleCar:    DrawMuscleCar();    break;
            case VehicleType.RacingCar:    DrawRacingCar();    break;
            case VehicleType.Boat: DrawBoat(); break;
            case VehicleType.Jetski:     DrawJetski();     break;
            case VehicleType.Dinghy:     DrawDinghy();     break;
            case VehicleType.SpeedBoat:  DrawSpeedBoat();  break;
            case VehicleType.Canoe:      DrawCanoe();      break;
            case VehicleType.Yacht:      DrawYacht();      break;
            case VehicleType.CruiseShip: DrawCruiseShip(); break;
        }

        foreach (var wp in wakeParticles)
        {
            float t = wp.life / wp.maxLife;                       // 1 → 0
            byte a = (byte)(t * 150);
            float len = 10f + (1f - t) * 26f;                     // streaks stretch as they fade
            Vector2 tail = wp.pos + (wp.vel == Vector2.Zero ? Vector2.UnitX : Vector2.Normalize(wp.vel)) * len;
            Raylib.DrawLineEx(wp.pos, tail, 1f + t * 2f,
                IsWatercraft ? new Color((byte)235,(byte)245,(byte)255, a)
                             : new Color((byte)150,(byte)125,(byte)90,  a)); 
        }
        // exhaust smoke puffs
        foreach (var ep in exhaustParticles)
        {
            float t = ep.life / ep.maxLife;
            byte alpha = (byte)(t * 120);
            int r = (int)(6 + (1f - t) * 10);
            Raylib.DrawCircle((int)ep.pos.X, (int)ep.pos.Y - (int)((1f - t) * 14), r,
                new Color((byte)160, (byte)160, (byte)160, alpha));
        }
        // speed lines when fast
        if (isMoving && velocity.Length() > 200f)
        {
            int lineCount = 4;
            for (int i = 0; i < lineCount; i++)
            {
                float ox = (Facing == FacingDirection.Right ? 1 : -1) * (20 + i * 14);
                float oy = 10 + i * 8;
                byte a = (byte)(80 - i * 15);
                Raylib.DrawLine(
                    (int)(Position.X + 50 - ox), (int)(Position.Y + (int)oy),
                    (int)(Position.X + 50 - ox - (Facing == FacingDirection.Right ? 30 : -30)),
                    (int)(Position.Y + (int)oy),
                    new Color((byte)220, (byte)220, (byte)255, a));
            }
        }

         if (HeadlightsOn)
        {
            Rectangle hb = Bounds;
            Vector2 c = new Vector2(hb.X + hb.Width / 2, hb.Y + hb.Height / 2);
            bool horiz = Facing is FacingDirection.Left or FacingDirection.Right;
            Vector2 dir = Facing switch
            {
                FacingDirection.Right => new Vector2(1, 0),
                FacingDirection.Left  => new Vector2(-1, 0),
                FacingDirection.Up    => new Vector2(0, -1),
                _                     => new Vector2(0, 1),
            };
            float dirAngle = Facing switch
            {
                FacingDirection.Right => 0f, FacingDirection.Down => 90f,
                FacingDirection.Left => 180f, _ => 270f
            };
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            Vector2 front = c + dir * (horiz ? hb.Width : hb.Height) * 0.5f;
            float spread = (horiz ? hb.Height : hb.Width) * 0.28f;
            float reach = 120f + hb.Width * 0.3f;   // bigger vehicles throw further

            foreach (float s in new[] { -1f, 1f })
            {
                Vector2 bulb = front + perp * s * spread;
                Raylib.DrawCircleSector(bulb, reach, dirAngle - 14, dirAngle + 14, 12,
                    new Color((byte)255,(byte)240,(byte)160,(byte)55));
                Raylib.DrawCircleV(bulb, 3f, new Color((byte)255,(byte)250,(byte)210,(byte)230));
            }
        }
    }

    // ── SEDAN ────────────────────────────────────────────────────────────────
    void DrawSedan()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        switch (Facing)
        {
            case FacingDirection.Right:
                // body
                Raylib.DrawRectangle(x, y + 18, 100, 28, VehicleColor);
                Raylib.DrawRectangle(x, y + 18, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // cabin
                Raylib.DrawRectangle(x + 22, y + 4, 52, 16, VehicleColor);
                Raylib.DrawRectangle(x + 24, y + 6, 48, 12,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                // windscreen lines
                Raylib.DrawLine(x + 24, y + 6,  x + 48, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawLine(x + 72, y + 6,  x + 48, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                // wheels
                Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // headlights
                Raylib.DrawRectangle(x + 92, y + 22, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 92, y + 22, 8, 3,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                // tail lights
                Raylib.DrawRectangle(x, y + 22, 6, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // door line
                Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                // door handle
                Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x, y + 18, 100, 28, VehicleColor);
                Raylib.DrawRectangle(x, y + 18, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 26, y + 4, 52, 16, VehicleColor);
                Raylib.DrawRectangle(x + 28, y + 6, 48, 12,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 28, y + 6,  x + 52, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawLine(x + 76, y + 6,  x + 52, y + 18, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // headlights left side
                Raylib.DrawRectangle(x, y + 22, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x, y + 22, 8, 3,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                Raylib.DrawRectangle(x + 94, y + 22, 6, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

   case FacingDirection.Down:
    // front-on view

    // Wheels
    Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
    Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);

    // Car Body
    Raylib.DrawRectangle(x + 7, y + 30, 76, 27, VehicleColor);
    Raylib.DrawRectangle(x + 16, y + 9, 59, 22,
        new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

    // windscreen
    Raylib.DrawRectangle(x + 20, y + 13, 52, 14,
        new Color((byte)100,(byte)180,(byte)220,(byte)200));

    // headlights
    Raylib.DrawRectangle(x + 11, y + 32, 16, 9,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));
    Raylib.DrawRectangle(x + 63, y + 32, 16, 9,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));

    // grille
    Raylib.DrawRectangle(x + 31, y + 34, 29, 7, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 32; i < x + 58; i += 5)
        Raylib.DrawRectangle(i, y + 34, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);
    break;

           case FacingDirection.Up:
    // rear view - draw wheels FIRST so body overlaps them
    Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
    Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);

    // Car Body
    Raylib.DrawRectangle(x + 7, y + 30, 76, 27, VehicleColor);
    Raylib.DrawRectangle(x + 16, y + 9, 59, 22,
        new Color((byte)Math.Max(0,VehicleColor.R-20),(byte)Math.Max(0,VehicleColor.G-20),(byte)Math.Max(0,VehicleColor.B-20),(byte)255));

    // exhaust pipe
    Raylib.DrawCircle(x + 11, y + 50, 3, Color.Black);
    Raylib.DrawCircleLines(x + 11, y + 50, 3, Color.Gray);

    // Number plate
    Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);

    // rear window
    Raylib.DrawRectangle(x + 20, y + 13, 52, 14,
        new Color((byte)80,(byte)160,(byte)200,(byte)160));

    // tail lights
    Raylib.DrawRectangle(x + 11, y + 32, 16, 9,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(x + 63, y + 32, 16, 9,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));

    // boot/trunk line
    Raylib.DrawRectangle(x + 29, y + 34, 32, 2, new Color((byte)0,(byte)0,(byte)0,(byte)80));
    break;
        }

        // driver visible through window when riding
        //if (Driving)
        {
            //Raylib.DrawCircle((int)Position.X + 50, (int)Position.Y + 14, 7,
                //Program.player.SkinColor);
        }
    }

    // ── UTE (pickup, tray at back) ──
    void DrawUte()
    {
        int x = (int)Position.X, y = (int)Position.Y;
        Color tray = new Color((byte)Math.Max(0,VehicleColor.R-25),(byte)Math.Max(0,VehicleColor.G-25),(byte)Math.Max(0,VehicleColor.B-25),(byte)255);
        switch (Facing)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
            {
                bool left = Facing == FacingDirection.Right;
                int cabX = left ? x + 58 : x;      // cab front
                int trayX = left ? x : x + 42;     // open tray
                Raylib.DrawRectangle(trayX, y + 22, 58, 24, tray);          // tray bed
                Raylib.DrawRectangle(trayX, y + 18, 58, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(cabX, y + 12, 42, 34, VehicleColor);   // cab
                Raylib.DrawRectangle(cabX + 6, y + 16, 30, 14, new Color((byte)100,(byte)180,(byte)220,(byte)200)); // window
                Raylib.DrawCircle(x + 22, y + 46, 12, Color.Black);
                Raylib.DrawCircle(x + 78, y + 46, 12, Color.Black);
                Raylib.DrawRectangle(left ? x : x + 92, y + 26, 8, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255)); // headlight
                break;
            }
            case FacingDirection.Down:
            case FacingDirection.Up:
            {
                Raylib.DrawRectangle(x + 15, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 62, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 7, y + 8, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 16, y + 12, 59, 16, new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawRectangle(x + 27, y + 50, 36, 6, Color.White);
                if (Facing == FacingDirection.Down)
                {
                    Raylib.DrawRectangle(x + 11, y + 40, 14, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                    Raylib.DrawRectangle(x + 65, y + 40, 14, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                }
                break;
            }
        }
    }

    // ── MONSTER TRUCK (huge wheels) ──
    void DrawMonsterTruck()
    {
        int x = (int)Position.X, y = (int)Position.Y;
        switch (Facing)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
            {
                // giant wheels
                Raylib.DrawCircle(x + 22, y + 44, 20, Color.Black);
                Raylib.DrawCircle(x + 22, y + 44, 9, new Color((byte)90,(byte)90,(byte)90,(byte)255));
                Raylib.DrawCircle(x + 78, y + 44, 20, Color.Black);
                Raylib.DrawCircle(x + 78, y + 44, 9, new Color((byte)90,(byte)90,(byte)90,(byte)255));
                // raised body
                Raylib.DrawRectangle(x + 10, y + 6, 80, 24, VehicleColor);
                Raylib.DrawRectangle(x + 28, y - 4, 44, 14, VehicleColor);   // cabin
                Raylib.DrawRectangle(x + 32, y - 1, 36, 9, new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawRectangle(Facing == FacingDirection.Left ? x + 10 : x + 84, y + 12, 6, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                break;
            }
            case FacingDirection.Down:
            case FacingDirection.Up:
            {
                Raylib.DrawCircle(x + 16, y + 44, 18, Color.Black);
                Raylib.DrawCircle(x + 74, y + 44, 18, Color.Black);
                Raylib.DrawRectangle(x + 10, y + 2, 70, 44, VehicleColor);
                Raylib.DrawRectangle(x + 18, y + 8, 54, 16, new Color((byte)100,(byte)180,(byte)220,(byte)200));
                if (Facing == FacingDirection.Down)
                {
                    Raylib.DrawRectangle(x + 14, y + 30, 14, 9, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                    Raylib.DrawRectangle(x + 62, y + 30, 14, 9, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                }
                break;
            }
        }
    }

    // ── CONVERTIBLE (low, open top) ──
    void DrawConvertible()
    {
        int x = (int)Position.X, y = (int)Position.Y;
        switch (Facing)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
            {
                Raylib.DrawRectangle(x, y + 24, 100, 22, VehicleColor);     // low body
                Raylib.DrawRectangle(x + 28, y + 18, 44, 8, new Color((byte)40,(byte)40,(byte)50,(byte)255)); // open cockpit rim
                Raylib.DrawRectangle(x + 30, y + 14, 16, 8, new Color((byte)120,(byte)190,(byte)220,(byte)160)); // windshield
                if (Driving) Raylib.DrawCircle(x + 50, y + 18, 6, Program.player.SkinColor); // driver head visible (open top)
                Raylib.DrawCircle(x + 20, y + 46, 11, Color.Black);
                Raylib.DrawCircle(x + 80, y + 46, 11, Color.Black);
                Raylib.DrawRectangle(Facing == FacingDirection.Left ? x : x + 94, y + 28, 6, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                break;
            }
            case FacingDirection.Down:
            case FacingDirection.Up:
            {
                Raylib.DrawRectangle(x + 15, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 62, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 9, y + 14, 72, 38, VehicleColor);
                Raylib.DrawRectangle(x + 18, y + 18, 54, 12, new Color((byte)40,(byte)40,(byte)50,(byte)255)); // open seats
                if (Driving && Facing == FacingDirection.Down) Raylib.DrawCircle(x + 45, y + 24, 6, Program.player.SkinColor);
                break;
            }
        }
    }
    // ── BOAT ─────────────────────────────────────────────────────────────────
void DrawBoat()
{
    int x = (int)Position.X;
    int y = (int)Position.Y;
    Color hull = VehicleColor;
    Color deck = new Color((byte)Math.Min(255,hull.R+50),(byte)Math.Min(255,hull.G+50),(byte)Math.Min(255,hull.B+50),(byte)255);
    Color wake = new Color((byte)200,(byte)220,(byte)240,(byte)140);

    switch (Facing)
    {
        case FacingDirection.Right:
            
            // hull — tapered bow on the right
            Raylib.DrawRectangle(x + 10, y + 20, 80, 24, hull);
            Raylib.DrawTriangle(new Vector2(x + 90, y + 20), new Vector2(x + 100, y + 32), new Vector2(x + 90, y + 44), hull);
            Raylib.DrawRectangle(x + 10, y + 20, 80, 4, deck);
            // cabin
            Raylib.DrawRectangle(x + 28, y + 6, 36, 16, deck);
            Raylib.DrawRectangle(x + 32, y + 8, 28, 10, new Color((byte)100,(byte)180,(byte)220,(byte)200));
            // waterline
            Raylib.DrawRectangle(x + 6, y + 42, 90, 4, new Color((byte)30,(byte)90,(byte)140,(byte)200));
            break;

        case FacingDirection.Left:
           
            Raylib.DrawRectangle(x + 10, y + 20, 80, 24, hull);
            Raylib.DrawTriangle(new Vector2(x + 10, y + 20), new Vector2(x, y + 32), new Vector2(x + 10, y + 44), hull);
            Raylib.DrawRectangle(x + 10, y + 20, 80, 4, deck);
            Raylib.DrawRectangle(x + 36, y + 6, 36, 16, deck);
            Raylib.DrawRectangle(x + 40, y + 8, 28, 10, new Color((byte)100,(byte)180,(byte)220,(byte)200));
            Raylib.DrawRectangle(x + 6, y + 42, 90, 4, new Color((byte)30,(byte)90,(byte)140,(byte)200));
            break;

        case FacingDirection.Up:
                
            Raylib.DrawRectangle(x + 22, y + 10, 56, 34, hull);
            Raylib.DrawTriangle(new Vector2(x + 22, y + 10), new Vector2(x + 50, y), new Vector2(x + 78, y + 10), hull);
            Raylib.DrawRectangle(x + 30, y + 20, 40, 14, deck);
            Raylib.DrawRectangle(x + 34, y + 22, 32, 8, new Color((byte)100,(byte)180,(byte)220,(byte)200));
            break;

        case FacingDirection.Down:
                
            Raylib.DrawRectangle(x + 22, y + 6, 56, 34, hull);
            Raylib.DrawTriangle(new Vector2(x + 22, y + 40), new Vector2(x + 50, y + 50), new Vector2(x + 78, y + 40), hull);
            Raylib.DrawRectangle(x + 30, y + 12, 40, 14, deck);
            Raylib.DrawRectangle(x + 34, y + 14, 32, 8, new Color((byte)100,(byte)180,(byte)220,(byte)200));
            break;
    }
}

void DrawJetski()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = VehicleColor;
    Color seat = new Color((byte)40,(byte)40,(byte)45,(byte)255);
    Color wake = new Color((byte)200,(byte)220,(byte)240,(byte)140);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        bool r = Facing == FacingDirection.Right;
        
        Raylib.DrawRectangle(x + 8, y + 16, 54, 16, hull);                                  // body
        Raylib.DrawTriangle(                                                                 // nose
            new Vector2(r ? x + 62 : x + 8, y + 16),
            new Vector2(r ? x + 70 : x,     y + 24),
            new Vector2(r ? x + 62 : x + 8, y + 32), hull);
        Raylib.DrawRectangle(x + (r ? 16 : 30), y + 8, 24, 10, seat);                        // seat
        Raylib.DrawRectangle(x + (r ? 44 : 20), y + 4, 6, 12, seat);                         // handlebar
        Raylib.DrawRectangle(x + 6, y + 30, 60, 3, new Color((byte)30,(byte)90,(byte)140,(byte)200));
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        
        Raylib.DrawRectangle(x + 24, y + 6, 22, 30, hull);
        Raylib.DrawTriangle(                                                                 // nose
            new Vector2(x + 24, up ? y + 6 : y + 36),
            new Vector2(x + 35, up ? y - 2 : y + 44),
            new Vector2(x + 46, up ? y + 6 : y + 36), hull);
        Raylib.DrawRectangle(x + 28, y + (up ? 18 : 10), 14, 14, seat);
        Raylib.DrawRectangle(x + 30, y + (up ? 12 : 26), 10, 4, seat);
    }
}

void DrawDinghy()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = VehicleColor;
    Color rim  = new Color((byte)Math.Min(255,hull.R+60),(byte)Math.Min(255,hull.G+60),(byte)Math.Min(255,hull.B+60),(byte)255);
    Color wake = new Color((byte)200,(byte)220,(byte)240,(byte)140);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        bool r = Facing == FacingDirection.Right;
        
        Raylib.DrawRectangle(x + 12, y + 18, 72, 24, hull);
        Raylib.DrawTriangle(
            new Vector2(r ? x + 84 : x + 12, y + 18),
            new Vector2(r ? x + 96 : x,      y + 30),
            new Vector2(r ? x + 84 : x + 12, y + 42), hull);
        Raylib.DrawRectangle(x + 12, y + 18, 72, 5, rim);                                    // inflatable rim
        Raylib.DrawRectangle(x + (r ? 14 : 74), y + 12, 8, 12, new Color((byte)60,(byte)60,(byte)65,(byte)255)); // outboard
        Raylib.DrawRectangle(x + 30, y + 26, 10, 8, rim);                                    // bench
        Raylib.DrawRectangle(x + 54, y + 26, 10, 8, rim);
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        
        Raylib.DrawRectangle(x + 20, y + 8, 30, 32, hull);
        Raylib.DrawTriangle(
            new Vector2(x + 20, up ? y + 8 : y + 40),
            new Vector2(x + 35, up ? y - 2 : y + 50),
            new Vector2(x + 50, up ? y + 8 : y + 40), hull);
        Raylib.DrawRectangleLines(x + 20, y + 8, 30, 32, rim);
        Raylib.DrawRectangle(x + 24, y + 20, 22, 6, rim);
        Raylib.DrawRectangle(x + 31, up ? y + 36 : y + 4, 8, 8, new Color((byte)60,(byte)60,(byte)65,(byte)255));
    }
}

void DrawSpeedBoat()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = VehicleColor;
    Color deck = new Color((byte)Math.Min(255,hull.R+50),(byte)Math.Min(255,hull.G+50),(byte)Math.Min(255,hull.B+50),(byte)255);
    Color glass = new Color((byte)120,(byte)200,(byte)240,(byte)200);
    Color wake = new Color((byte)210,(byte)230,(byte)250,(byte)160);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        bool r = Facing == FacingDirection.Right;
        Raylib.DrawRectangle(x + 8, y + 22, 76, 20, hull);
        Raylib.DrawTriangle(                                                                 // long racing bow
            new Vector2(r ? x + 84 : x + 8, y + 22),
            new Vector2(r ? x + 100 : x - 8, y + 32),
            new Vector2(r ? x + 84 : x + 8, y + 42), hull);
        Raylib.DrawRectangle(x + 8, y + 22, 76, 4, deck);
        Raylib.DrawTriangle(                                                                 // raked windshield
            new Vector2(x + (r ? 34 : 58), y + 22),
            new Vector2(x + (r ? 34 : 58), y + 8),
            new Vector2(x + (r ? 52 : 40), y + 22), glass);
        Raylib.DrawRectangle(x + 4, y + 40, 92, 4, new Color((byte)30,(byte)90,(byte)140,(byte)200));
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        Raylib.DrawRectangle(x + 26, y + 8, 48, 32, hull);
        Raylib.DrawTriangle(
            new Vector2(x + 26, up ? y + 8 : y + 40),
            new Vector2(x + 50, up ? y - 8 : y + 56),
            new Vector2(x + 74, up ? y + 8 : y + 40), hull);
        Raylib.DrawRectangle(x + 32, y + (up ? 20 : 12), 36, 10, glass);
    }
}

void DrawCanoe()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = VehicleColor;
    Color inner = new Color((byte)Math.Max(0,hull.R-40),(byte)Math.Max(0,hull.G-40),(byte)Math.Max(0,hull.B-40),(byte)255);
    Color wake = new Color((byte)200,(byte)220,(byte)240,(byte)120);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        Raylib.DrawRectangle(x + 10, y + 18, 50, 14, hull);                                  // slim hull
        Raylib.DrawTriangle(new Vector2(x + 60, y + 18), new Vector2(x + 70, y + 25), new Vector2(x + 60, y + 32), hull);
        Raylib.DrawTriangle(new Vector2(x + 10, y + 18), new Vector2(x, y + 25), new Vector2(x + 10, y + 32), hull);
        Raylib.DrawRectangle(x + 16, y + 21, 38, 8, inner);                                  // open top
        Raylib.DrawRectangle(x + 30, y + 21, 4, 8, hull);                                    // thwart
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        Raylib.DrawRectangle(x + 27, y + 8, 16, 26, hull);
        Raylib.DrawTriangle(new Vector2(x + 27, y + 8),  new Vector2(x + 35, y - 2),  new Vector2(x + 43, y + 8),  hull);
        Raylib.DrawTriangle(new Vector2(x + 27, y + 34), new Vector2(x + 35, y + 44), new Vector2(x + 43, y + 34), hull);
        Raylib.DrawRectangle(x + 30, y + 12, 10, 18, inner);
    }
}

void DrawYacht()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = VehicleColor;
    Color upper = new Color((byte)240,(byte)240,(byte)245,(byte)255);
    Color glass = new Color((byte)100,(byte)180,(byte)220,(byte)200);
    Color wake = new Color((byte)210,(byte)230,(byte)250,(byte)150);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        bool r = Facing == FacingDirection.Right;
        Raylib.DrawRectangle(x + 12, y + 34, 128, 28, hull);                                 // hull
        Raylib.DrawTriangle(
            new Vector2(r ? x + 140 : x + 12, y + 34),
            new Vector2(r ? x + 160 : x - 8,  y + 48),
            new Vector2(r ? x + 140 : x + 12, y + 62), hull);
        Raylib.DrawRectangle(x + 24, y + 18, 90, 16, upper);                                 // main deck
        Raylib.DrawRectangle(x + (r ? 36 : 46), y + 6, 60, 12, upper);                       // upper deck
        for (int i = 0; i < 4; i++)
            Raylib.DrawRectangle(x + 32 + i * 20, y + 22, 12, 8, glass);                     // windows
        Raylib.DrawRectangle(x + (r ? 90 : 40), y, 4, 8, new Color((byte)180,(byte)180,(byte)185,(byte)255)); // mast
        Raylib.DrawRectangle(x + 8, y + 58, 148, 5, new Color((byte)30,(byte)90,(byte)140,(byte)200));
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        Raylib.DrawRectangle(x + 54, y + 12, 52, 46, hull);
        Raylib.DrawTriangle(
            new Vector2(x + 54, up ? y + 12 : y + 58),
            new Vector2(x + 80, up ? y - 8 : y + 78),
            new Vector2(x + 106, up ? y + 12 : y + 58), hull);
        Raylib.DrawRectangle(x + 60, y + (up ? 26 : 16), 40, 28, upper);
        Raylib.DrawRectangle(x + 68, y + (up ? 32 : 22), 24, 16, glass);
    }
}

void DrawCruiseShip()
{
    int x = (int)Position.X, y = (int)Position.Y;
    Color hull = new Color((byte)30,(byte)40,(byte)60,(byte)255);      // dark hull regardless of paint
    Color upper = new Color((byte)245,(byte)245,(byte)250,(byte)255);
    Color stripe = VehicleColor;                                        // paint colour = hull stripe
    Color glass = new Color((byte)110,(byte)190,(byte)230,(byte)220);
    Color wake = new Color((byte)215,(byte)235,(byte)255,(byte)150);

    if (Facing == FacingDirection.Left || Facing == FacingDirection.Right)
    {
        bool r = Facing == FacingDirection.Right;
        Raylib.DrawRectangle(x + 16, y + 56, 232, 44, hull);                                 // hull
        Raylib.DrawTriangle(
            new Vector2(r ? x + 248 : x + 16, y + 56),
            new Vector2(r ? x + 278 : x - 14, y + 78),
            new Vector2(r ? x + 248 : x + 16, y + 100), hull);
        Raylib.DrawRectangle(x + 16, y + 66, 232, 6, stripe);                                // stripe
        Raylib.DrawRectangle(x + 30, y + 34, 200, 22, upper);                                // deck 1
        Raylib.DrawRectangle(x + 46, y + 16, 168, 18, upper);                                // deck 2
        for (int i = 0; i < 9; i++) Raylib.DrawRectangle(x + 40 + i * 22, y + 40, 12, 8, glass);
        for (int i = 0; i < 7; i++) Raylib.DrawRectangle(x + 56 + i * 22, y + 21, 12, 8, glass);
        Raylib.DrawRectangle(x + (r ? 170 : 70), y + 2, 20, 14, stripe);                     // funnel
        Raylib.DrawRectangle(x + (r ? 174 : 74), y - 4, 12, 6, new Color((byte)200,(byte)200,(byte)200,(byte)160)); // smoke cap
        Raylib.DrawRectangle(x + 10, y + 96, 260, 6, new Color((byte)25,(byte)80,(byte)130,(byte)220));
    }
    else
    {
        bool up = Facing == FacingDirection.Up;
        Raylib.DrawRectangle(x + 108, y + 18, 64, 76, hull);
        Raylib.DrawTriangle(
            new Vector2(x + 108, up ? y + 18 : y + 94),
            new Vector2(x + 140, up ? y - 10 : y + 122),
            new Vector2(x + 172, up ? y + 18 : y + 94), hull);
        Raylib.DrawRectangle(x + 116, y + (up ? 34 : 24), 48, 54, upper);
        for (int i = 0; i < 4; i++)
            Raylib.DrawRectangle(x + 124, y + (up ? 40 : 30) + i * 12, 32, 7, glass);
        Raylib.DrawRectangle(x + 130, y + (up ? 84 : 20), 20, 10, stripe);                   // funnel
    }
}

    // ── MUSCLE CAR (long bonnet, racing stripes) ──
    void DrawMuscleCar()
    {
        int x = (int)Position.X, y = (int)Position.Y;
        Color stripe = new Color((byte)240,(byte)240,(byte)240,(byte)255);
        switch (Facing)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
            {
                Raylib.DrawRectangle(x, y + 18, 100, 28, VehicleColor);
                Raylib.DrawRectangle(x + 24, y + 6, 46, 14, VehicleColor);  // low cabin
                Raylib.DrawRectangle(x + 27, y + 8, 40, 10, new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawRectangle(x, y + 28, 100, 4, stripe);            // side stripe
                Raylib.DrawCircle(x + 20, y + 46, 12, Color.Black);
                Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
                Raylib.DrawRectangle(Facing == FacingDirection.Left ? x : x + 92, y + 22, 8, 10, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                break;
            }
            case FacingDirection.Down:
            case FacingDirection.Up:
            {
                Raylib.DrawRectangle(x + 15, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 62, y + 32, 13, 30, Color.Black);
                Raylib.DrawRectangle(x + 7, y + 8, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 38, y + 8, 14, 50, stripe);        // twin stripe
                Raylib.DrawRectangle(x + 16, y + 14, 59, 16, new Color((byte)100,(byte)180,(byte)220,(byte)200));
                if (Facing == FacingDirection.Down)
                {
                    Raylib.DrawRectangle(x + 11, y + 40, 14, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                    Raylib.DrawRectangle(x + 65, y + 40, 14, 8, new Color((byte)255,(byte)240,(byte)150,(byte)255));
                }
                break;
            }
        }
    }

    // ── RACING CAR (F1-style, rear wing) ──
    void DrawRacingCar()
    {
        int x = (int)Position.X, y = (int)Position.Y;
        switch (Facing)
        {
            case FacingDirection.Right:
            case FacingDirection.Left:
            {
                bool left = Facing == FacingDirection.Left;
                Raylib.DrawRectangle(x + 10, y + 28, 80, 14, VehicleColor); // slim body
                Raylib.DrawRectangle(x + 40, y + 20, 20, 10, VehicleColor); // cockpit hump
                Raylib.DrawRectangle(x + 44, y + 22, 12, 6, new Color((byte)40,(byte)40,(byte)50,(byte)255));
                // rear wing
                int wingX = left ? x + 86 : x + 6;
                Raylib.DrawRectangle(wingX, y + 18, 8, 22, new Color((byte)30,(byte)30,(byte)30,(byte)255));
                // front nose cone
                int noseX = left ? x : x + 86;
                Raylib.DrawTriangle(
                    new Vector2(noseX + (left ? 0 : 14), y + 34),
                    new Vector2(noseX + (left ? 14 : 0), y + 28),
                    new Vector2(noseX + (left ? 14 : 0), y + 40), VehicleColor);
                Raylib.DrawCircle(x + 22, y + 44, 10, Color.Black);
                Raylib.DrawCircle(x + 78, y + 44, 10, Color.Black);
                break;
            }
            case FacingDirection.Down:
            case FacingDirection.Up:
            {
                Raylib.DrawRectangle(x + 12, y + 30, 10, 28, Color.Black);
                Raylib.DrawRectangle(x + 68, y + 30, 10, 28, Color.Black);
                Raylib.DrawRectangle(x + 30, y + 6, 30, 52, VehicleColor);  // narrow body
                Raylib.DrawRectangle(x + 36, y + 20, 18, 12, new Color((byte)40,(byte)40,(byte)50,(byte)255)); // cockpit
                Raylib.DrawRectangle(x + 14, y + 10, 62, 6, new Color((byte)30,(byte)30,(byte)30,(byte)255));  // front wing
                break;
            }
        }
    }

    // ── Police Car ────────────────────────────────────────────────────────────────
    void DrawPoliceCar()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;

        // helper colors
Color bodyColor = new Color((byte)20, (byte)20, (byte)30, (byte)255); // dark police body
Color stripeColor = new Color((byte)240, (byte)240, (byte)240, (byte)255); // white stripe/door
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)200);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // body
        Raylib.DrawRectangle(x, y + 18, 100, 28, bodyColor);
        Raylib.DrawRectangle(x, y + 18, 100, 4,
            new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        // white door stripe
        Raylib.DrawRectangle(x + 28, y + 22, 44, 12, stripeColor);
        // POLICE text on side
        Program.DrawTextUI("POLICE", x + 36, y + 24, 10, Color.Black);
        // cabin
        Raylib.DrawRectangle(x + 22, y + 4, 52, 16, bodyColor);
        Raylib.DrawRectangle(x + 24, y + 6, 48, 12, windowTint);
        // windscreen lines
        Raylib.DrawLine(x + 24, y + 6, x + 48, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawLine(x + 72, y + 6, x + 48, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        // wheels
        Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // headlights / tail
        Raylib.DrawRectangle(x + 92, y + 22, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x, y + 22, 6, 10, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        // door line & handles
        Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0, (byte)0, (byte)0, (byte)80));
        Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));
        Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren (roof) — centered above cabin for right-facing
        Raylib.DrawRectangle(x + 40, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 42, y - 0, 8, 6, sirenRed);
        Raylib.DrawRectangle(x + 50, y - 0, 8, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        Raylib.DrawRectangle(x, y + 18, 100, 28, bodyColor);
        Raylib.DrawRectangle(x, y + 18, 100, 4,
            new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 28, y + 22, 44, 12, stripeColor);
        Program.DrawTextUI("POLICE", x + 36, y + 24, 10, Color.Black);
        Raylib.DrawRectangle(x + 26, y + 4, 52, 16, bodyColor);
        Raylib.DrawRectangle(x + 28, y + 6, 48, 12, windowTint);
        Raylib.DrawLine(x + 28, y + 6, x + 52, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawLine(x + 76, y + 6, x + 52, y + 18, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawCircle(x + 18, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 80, y + 46, 12, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 46, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 46, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // headlights left side / tail
        Raylib.DrawRectangle(x, y + 22, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 94, y + 22, 6, 10, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawLine(x + 50, y + 18, x + 50, y + 46, new Color((byte)0, (byte)0, (byte)0, (byte)80));
        Raylib.DrawRectangle(x + 38, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));
        Raylib.DrawRectangle(x + 62, y + 30, 10, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren centered for left-facing
        Raylib.DrawRectangle(x + 40, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 42, y - 0, 8, 6, sirenBlue);
        Raylib.DrawRectangle(x + 50, y - 0, 8, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // front-on view (same dimensions)
        Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 7, y + 30, 76, 27, bodyColor);
        Raylib.DrawRectangle(x + 16, y + 9, 59, 22, new Color((byte)Math.Min(255, bodyColor.R + 40), (byte)Math.Min(255, bodyColor.G + 40), (byte)Math.Min(255, bodyColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 20, y + 13, 52, 14, windowTint);
        Raylib.DrawRectangle(x + 11, y + 32, 16, 9, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 63, y + 32, 16, 9, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 31, y + 34, 29, 7, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 32; i < x + 58; i += 5)
            Raylib.DrawRectangle(i, y + 34, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);

        // POLICE on front bumper
        Program.DrawTextUI("POLICE", x + 30, y + 38, 8, Color.Black);

        // siren — between the windscreen and front bonnet
        Raylib.DrawRectangle(x + 34, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 36, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 52, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // rear view
        Raylib.DrawRectangle(x + 15, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 62, y + 32, 13, 32, Color.Black);
        Raylib.DrawRectangle(x + 7, y + 30, 76, 27, bodyColor);
        Raylib.DrawRectangle(x + 16, y + 9, 59, 22, new Color((byte)Math.Max(0, bodyColor.R - 20), (byte)Math.Max(0, bodyColor.G - 20), (byte)Math.Max(0, bodyColor.B - 20), (byte)255));
        Raylib.DrawCircle(x + 11, y + 50, 3, Color.Black);
        Raylib.DrawCircleLines(x + 11, y + 50, 3, Color.Gray);
        Raylib.DrawRectangle(x + 27, y + 45, 36, 6, Color.White);
        Raylib.DrawRectangle(x + 20, y + 13, 52, 14, new Color((byte)80, (byte)160, (byte)200, (byte)160));
        Raylib.DrawRectangle(x + 11, y + 32, 16, 9, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 63, y + 32, 16, 9, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 29, y + 34, 32, 2, new Color((byte)0, (byte)0, (byte)0, (byte)80));

        // POLICE on trunk
        Program.DrawTextUI("POLICE", x + 30, y + 36, 8, Color.Black);

        // siren — mounted on roof near rear window in rear view
        Raylib.DrawRectangle(x + 34, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 36, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 52, y + 6, 12, 6, sirenRed);
        break;
}
        }

    

    // ── TRUCK ────────────────────────────────────────────────────────────────
    void DrawTruck()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        Color panelColor = new Color(
            (byte)Math.Max(0, VehicleColor.R - 20),
            (byte)Math.Max(0, VehicleColor.G - 20),
            (byte)Math.Max(0, VehicleColor.B - 20), (byte)255);

        switch (Facing)
        {
            case FacingDirection.Right:
                // tray / bed
                Raylib.DrawRectangle(x, y + 14, 60, 34, panelColor);
                Raylib.DrawRectangle(x, y + 14, 60, 4,
                    new Color((byte)Math.Min(255,panelColor.R+30),(byte)Math.Min(255,panelColor.G+30),(byte)Math.Min(255,panelColor.B+30),(byte)255));
                // tray sides
                Raylib.DrawRectangle(x, y + 14, 4, 34, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 44, 60, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // cab
                Raylib.DrawRectangle(x + 58, y + 8, 42, 40, VehicleColor);
                Raylib.DrawRectangle(x + 58, y + 8, 42, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // cab window
                Raylib.DrawRectangle(x + 64, y + 12, 28, 18,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 64, y + 12, x + 78, y + 30, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                // headlight
                Raylib.DrawRectangle(x + 92, y + 20, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                // tail light on tray
                Raylib.DrawRectangle(x, y + 20, 4, 8,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // wheels — dual rear
                Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 38, y + 48, 11, Color.Black);
                Raylib.DrawCircleLines(x + 38, y + 48, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 38, y + 48, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // front wheel
                Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // exhaust pipe
                Raylib.DrawRectangle(x + 96, y + 10, 4, 16, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x + 40, y + 14, 60, 34, panelColor);
                Raylib.DrawRectangle(x + 40, y + 14, 60, 4,
                    new Color((byte)Math.Min(255,panelColor.R+30),(byte)Math.Min(255,panelColor.G+30),(byte)Math.Min(255,panelColor.B+30),(byte)255));
                Raylib.DrawRectangle(x + 96, y + 14, 4, 34, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 40, y + 44, 60, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 8, 42, 40, VehicleColor);
                Raylib.DrawRectangle(x, y + 8, 42, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 8, y + 12, 28, 18,
                    new Color((byte)100,(byte)180,(byte)220,(byte)200));
                Raylib.DrawLine(x + 36, y + 12, x + 22, y + 30, new Color((byte)60,(byte)140,(byte)180,(byte)180));
                Raylib.DrawRectangle(x, y + 20, 8, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 96, y + 20, 4, 8,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 62, y + 48, 11, Color.Black);
                Raylib.DrawCircleLines(x + 62, y + 48, 11, Color.DarkGray);
                Raylib.DrawCircle(x + 62, y + 48, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
                Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
                Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x, y + 10, 4, 16, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                break;

            case FacingDirection.Down:

    // wheels
    Raylib.DrawRectangle(x + 7, y + 47, 12, 25, Color.Black);
    Raylib.DrawRectangle(x + 59, y + 47, 12, 25, Color.Black);

    // wide front grille
    Raylib.DrawRectangle(x + 5, y + 7, 68, 54, VehicleColor);
    Raylib.DrawRectangle(x + 5, y + 7, 68, 4,
        new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

    // windscreen
    Raylib.DrawRectangle(x + 13, y + 13, 54, 14,
        new Color((byte)100,(byte)180,(byte)220,(byte)200));

    // wing mirrors
    Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
    Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);

    // big grille
    Raylib.DrawRectangle(x + 13, y + 34, 54, 9, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 18; i < x + 52; i += 5)
        Raylib.DrawRectangle(i, y + 35, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // smaller grille
    Raylib.DrawRectangle(x + 18, y + 45, 43, 9, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    for (int i = x + 20; i < x + 41; i += 5)
        Raylib.DrawRectangle(i, y + 45, 2, 7, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // bull bar
    Raylib.DrawRectangle(x + 5, y + 50, 68, 14, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangleLines(x + 5, y + 50, 68, 14, Color.Black);

    // large headlights
    Raylib.DrawRectangle(x + 11, y + 52, 7, 7,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));
    Raylib.DrawRectangle(x + 59, y + 52, 7, 7,
        new Color((byte)255,(byte)240,(byte)150,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);

    break;

case FacingDirection.Up:

    // tray rear view
    Raylib.DrawRectangle(x + 5, y + 7, 68, 50, panelColor);
    Raylib.DrawRectangle(x + 5, y + 7, 68, 18,
        new Color((byte)Math.Max(0,panelColor.R-30),(byte)Math.Max(0,panelColor.G-30),(byte)Math.Max(0,panelColor.B-30),(byte)255));

    // wheels
    Raylib.DrawRectangle(x + 7, y + 47, 12, 20, Color.Black);
    Raylib.DrawRectangle(x + 59, y + 47, 12, 20, Color.Black);

    // tray walls
    Raylib.DrawRectangle(x + 5,  y + 18, 4, 38, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(x + 68, y + 18, 4, 38, new Color((byte)60,(byte)60,(byte)60,(byte)255));
    Raylib.DrawRectangle(x + 5,  y + 18, 68, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    // wing mirrors
    Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
    Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);

    // tail lights
    Raylib.DrawRectangle(x + 7,  y + 50, 9, 5,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangle(x + 59, y + 50, 9, 5,
        new Color((byte)220,(byte)30,(byte)30,(byte)255));

    // Number plate
    Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);

    // tow bar
    Raylib.DrawRectangle(x + 32, y + 54, 11, 5, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    break;
        }

    }

    // ── Fire Truck ────────────────────────────────────────────────────────────────
    void DrawFireTruck()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
    // firetruck-specific colors
Color truckRed = new Color((byte)200, (byte)30, (byte)30, (byte)255);
Color trimWhite = new Color((byte)240, (byte)240, (byte)240, (byte)255);
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)200);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);
Color hoseDark = new Color((byte)40, (byte)40, (byte)40, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // tray / body (red with white trim)
        Raylib.DrawRectangle(x, y + 8, 94, 40, Color.White);
        Raylib.DrawRectangle(x, y + 14, 60, 34, truckRed);
        Raylib.DrawRectangle(x, y + 14, 60, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        Raylib.DrawRectangle(x + 6, y + 22, 48, 10, trimWhite); // white stripe
        
        // ladder on side (stacked rungs)
        Raylib.DrawRectangle(x + 8, y + 18, 4, 28, new Color((byte)150, (byte)150, (byte)150, (byte)255));
        for (int r = 0; r < 5; r++)
            Raylib.DrawRectangle(x + 12, y + 20 + r * 6, 28, 3, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        // hose reel at rear
        Raylib.DrawCircle(x + 12, y + 32, 8, hoseDark);
        Raylib.DrawCircleLines(x + 12, y + 32, 8, Color.DarkGray);
        // tray sides
        Raylib.DrawRectangle(x, y + 14, 4, 34, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 44, 60, 4, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // cab
        Raylib.DrawRectangle(x + 58, y + 8, 42, 40, VehicleColor);
        Raylib.DrawRectangle(x + 58, y + 8, 42, 4, new Color((byte)Math.Min(255, VehicleColor.R + 40), (byte)Math.Min(255, VehicleColor.G + 40), (byte)Math.Min(255, VehicleColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 64, y + 12, 28, 18, windowTint);
        Raylib.DrawLine(x + 64, y + 12, x + 78, y + 30, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        // headlight and tail light
        Raylib.DrawRectangle(x + 92, y + 20, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x, y + 20, 4, 8, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        // wheels — 
        Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        // exhaust
        Raylib.DrawRectangle(x + 96, y + 10, 4, 16, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        // "FIRE" text on stripe
        Program.DrawTextUI("FIRE", x + 20, y + 22, 10, Color.Red);

        // siren (roof) — centered above cabin
        Raylib.DrawRectangle(x + 60, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 62, y - 0, 8, 6, sirenRed);
        Raylib.DrawRectangle(x + 70, y - 0, 8, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        Raylib.DrawRectangle(x, y + 8, 96, 40, Color.White);
        Raylib.DrawRectangle(x + 40, y + 14, 60, 34, truckRed);
        Raylib.DrawRectangle(x + 40, y + 14, 60, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        Raylib.DrawRectangle(x + 46, y + 22, 48, 10, trimWhite); // stripe
        // ladder on side (mirrored)
        Raylib.DrawRectangle(x + 96, y + 18, 4, 28, new Color((byte)150, (byte)150, (byte)150, (byte)255));
        for (int r = 0; r < 5; r++)
            Raylib.DrawRectangle(x + 56, y + 20 + r * 6, 28, 3, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        // hose reel at rear (mirrored)
        Raylib.DrawCircle(x + 88, y + 32, 8, hoseDark);
        Raylib.DrawCircleLines(x + 88, y + 32, 8, Color.DarkGray);
        Raylib.DrawRectangle(x + 96, y + 14, 4, 34, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 40, y + 44, 60, 4, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 8, 42, 40, VehicleColor);
        Raylib.DrawRectangle(x, y + 8, 42, 4, new Color((byte)Math.Min(255, VehicleColor.R + 40), (byte)Math.Min(255, VehicleColor.G + 40), (byte)Math.Min(255, VehicleColor.B + 40), (byte)255));
        Raylib.DrawRectangle(x + 8, y + 12, 28, 18, windowTint);
        Raylib.DrawLine(x + 36, y + 12, x + 22, y + 30, new Color((byte)60, (byte)140, (byte)180, (byte)180));
        Raylib.DrawRectangle(x, y + 20, 8, 10, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 96, y + 20, 4, 8, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawCircle(x + 18, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 18, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 18, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
       
        Raylib.DrawCircle(x + 82, y + 48, 12, Color.Black);
        Raylib.DrawCircleLines(x + 82, y + 48, 12, Color.DarkGray);
        Raylib.DrawCircle(x + 82, y + 48, 5, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x, y + 10, 4, 16, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Program.DrawTextUI("FIRE", x + 52, y + 22, 10, Color.Red);

        // siren centered for left-facing (swap halves visually)
        Raylib.DrawRectangle(x + 20, y - 2, 20, 8, sirenBase);
        Raylib.DrawRectangle(x + 22, y - 0, 8, 6, sirenBlue);
        Raylib.DrawRectangle(x + 30, y - 0, 8, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // wheels
        Raylib.DrawRectangle(x + 7, y + 47, 12, 25, Color.Black);
        Raylib.DrawRectangle(x + 59, y + 47, 12, 25, Color.Black);
        // wide front grille (red with white trim)
        Raylib.DrawRectangle(x + 5, y + 7, 68, 54, truckRed);
        Raylib.DrawRectangle(x + 5, y + 7, 68, 4, new Color((byte)Math.Min(255, truckRed.R + 10), (byte)Math.Min(255, truckRed.G + 10), (byte)Math.Min(255, truckRed.B + 10), (byte)255));
        // white stripe across front
        Raylib.DrawRectangle(x + 12, y + 34, 44, 8, trimWhite);
        // windscreen
        Raylib.DrawRectangle(x + 13, y + 13, 54, 14, windowTint);
        Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
        Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);
        // grilles / bull bar
        Raylib.DrawRectangle(x + 13, y + 34, 54, 9, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 18; i < x + 52; i += 5)
            Raylib.DrawRectangle(i, y + 35, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 18, y + 45, 43, 9, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        for (int i = x + 20; i < x + 41; i += 5)
            Raylib.DrawRectangle(i, y + 45, 2, 7, new Color((byte)80, (byte)80, (byte)80, (byte)255));
        Raylib.DrawRectangle(x + 5, y + 50, 68, 14, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangleLines(x + 5, y + 50, 68, 14, Color.Black);
        Raylib.DrawRectangle(x + 11, y + 52, 7, 7, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 59, y + 52, 7, 7, new Color((byte)255, (byte)240, (byte)150, (byte)255));
        Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);
        Program.DrawTextUI("FIRE", x + 26, y + 38, 10, Color.White);

        // front hose coupling detail
        Raylib.DrawRectangle(x + 30, y + 42, 8, 6, hoseDark);
        // siren — between windscreen and bonnet
        Raylib.DrawRectangle(x + 28, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 30, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 46, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // tray rear view (red)
        Raylib.DrawRectangle(x + 5, y + 7, 68, 50, truckRed);
        Raylib.DrawRectangle(x + 5, y + 7, 68, 18, new Color((byte)Math.Max(0, truckRed.R - 30), (byte)Math.Max(0, truckRed.G - 30), (byte)Math.Max(0, truckRed.B - 30), (byte)255));
        // wheels
        Raylib.DrawRectangle(x + 7, y + 47, 12, 20, Color.Black);
        Raylib.DrawRectangle(x + 59, y + 47, 12, 20, Color.Black);
        // tray walls
        Raylib.DrawRectangle(x + 5, y + 18, 4, 38, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangle(x + 68, y + 18, 4, 38, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Raylib.DrawRectangle(x + 5, y + 18, 68, 4, Color.White);
        Raylib.DrawRectangle(x - 2, y + 14, 4, 8, Color.Black);
        Raylib.DrawRectangle(x + 74, y + 14, 4, 8, Color.Black);
        // tail lights and tow bar
        Raylib.DrawRectangle(x + 7, y + 50, 9, 5, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 59, y + 50, 9, 5, new Color((byte)220, (byte)30, (byte)30, (byte)255));
        Raylib.DrawRectangle(x + 23, y + 59, 32, 5, Color.White);
        Raylib.DrawRectangle(x + 32, y + 54, 11, 5, new Color((byte)60, (byte)60, (byte)60, (byte)255));
        Program.DrawTextUI("FIRE", x + 26, y + 36, 10, Color.White);

        // ladder roof detail (rear view)
        Raylib.DrawRectangle(x + 12, y + 8, 44, 4, new Color((byte)180, (byte)180, (byte)180, (byte)255));
        for (int r = 0; r < 4; r++)
            Raylib.DrawRectangle(x + 14 + r * 10, y + 12, 6, 3, new Color((byte)160, (byte)160, (byte)160, (byte)255));

        // siren — mounted on roof near rear window (rear view)
        Raylib.DrawRectangle(x + 28, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 30, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 46, y + 6, 12, 6, sirenRed);
        break;
}
    }


    // ── SUV ──────────────────────────────────────────────────────────────────
    void DrawSUV()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        Color darkColor = new Color(
            (byte)Math.Max(0, VehicleColor.R - 30),
            (byte)Math.Max(0, VehicleColor.G - 30),
            (byte)Math.Max(0, VehicleColor.B - 30), (byte)255);

        switch (Facing)
        {
            case FacingDirection.Right:
                // body — taller and boxier than sedan
                Raylib.DrawRectangle(x, y + 12, 100, 36, VehicleColor);
                Raylib.DrawRectangle(x, y + 12, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // full length roof
                Raylib.DrawRectangle(x + 10, y + 2, 80, 12, VehicleColor);
                Raylib.DrawRectangle(x + 10, y + 2, 80, 3,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                // roof rack
                Raylib.DrawRectangle(x + 14, y,     72, 3, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawRectangle(x + 14, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 82, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 48, y,     4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // windows — two separate panes
                Raylib.DrawRectangle(x + 14, y + 5, 36, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 54, y + 5, 32, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                // window divider (B pillar)
                Raylib.DrawRectangle(x + 50, y + 4, 4, 12, VehicleColor);
                // headlights
                Raylib.DrawRectangle(x + 92, y + 16, 8, 12,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 92, y + 16, 8, 4,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                // tail lights
                Raylib.DrawRectangle(x, y + 16, 6, 12,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));

                // spare tyre on back
                Raylib.DrawRectangle(x - 6, y + 10, 8, 20,
                    Color.Black);

                // skid plate / running boards
                Raylib.DrawRectangle(x + 10, y + 46, 80, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                // wheels — larger than sedan
                Raylib.DrawCircle(x + 20, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 20, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                // door lines
                Raylib.DrawLine(x + 50, y + 12, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                // door handles
                Raylib.DrawRectangle(x + 30, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 60, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Left:
                Raylib.DrawRectangle(x, y + 12, 100, 36, VehicleColor);
                Raylib.DrawRectangle(x, y + 12, 100, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 10, y + 2, 80, 12, VehicleColor);
                Raylib.DrawRectangle(x + 10, y + 2, 80, 3,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));
                Raylib.DrawRectangle(x + 14, y, 72, 3, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawRectangle(x + 14, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 82, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 48, y, 4,  3, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawRectangle(x + 14, y + 5, 32, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 50, y + 5, 36, 10,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 46, y + 4, 4, 12, VehicleColor);
                // headlights on left side
                Raylib.DrawRectangle(x, y + 16, 8, 12,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x, y + 16, 8, 4,
                    new Color((byte)255,(byte)255,(byte)200,(byte)255));
                Raylib.DrawRectangle(x + 94, y + 16, 6, 12,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));

                // Spare wheel on back
                Raylib.DrawRectangle(x + 98, y + 10, 8, 20,
                    Color.Black);

                Raylib.DrawRectangle(x + 10, y + 46, 80, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));
                Raylib.DrawCircle(x + 20, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 20, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawCircle(x + 80, y + 48, 14, Color.Black);
                Raylib.DrawCircleLines(x + 80, y + 48, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
                Raylib.DrawLine(x + 50, y + 12, x + 50, y + 46, new Color((byte)0,(byte)0,(byte)0,(byte)80));
                Raylib.DrawRectangle(x + 28, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                Raylib.DrawRectangle(x + 58, y + 28, 12, 3, new Color((byte)180,(byte)180,(byte)180,(byte)255));
                break;

            case FacingDirection.Down:

                // wheels
                Raylib.DrawRectangle(x + 5, y + 32, 13, 32, Color.Black);
                Raylib.DrawRectangle(x + 68, y + 32, 13, 32, Color.Black);

                // boxy front
                Raylib.DrawRectangle(x + 5, y + 6, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 5, y + 6, 76, 4,
                    new Color((byte)Math.Min(255,VehicleColor.R+40),(byte)Math.Min(255,VehicleColor.G+40),(byte)Math.Min(255,VehicleColor.B+40),(byte)255));

                // windscreen
                Raylib.DrawRectangle(x + 9, y + 10, 68, 22,
                    new Color((byte)100,(byte)180,(byte)220,(byte)180));
                Raylib.DrawRectangle(x + 40, y + 10, 4, 22, VehicleColor);
                // headlights — square and wide
                Raylib.DrawRectangle(x + 9,  y + 34, 16, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                Raylib.DrawRectangle(x + 64, y + 34, 16, 10,
                    new Color((byte)255,(byte)240,(byte)150,(byte)255));
                // grille
                Raylib.DrawRectangle(x + 28, y + 36, 36, 20, new Color((byte)40,(byte)40,(byte)40,(byte)255));
                for (int i = x + 34; i < x + 66; i += 6)
                    Raylib.DrawRectangle(i, y + 37, 2, 10, new Color((byte)80,(byte)80,(byte)80,(byte)255));

                // bull bar
                Raylib.DrawRectangle(x + 5, y + 48, 76, 12, new Color((byte)60,(byte)60,(byte)60,(byte)255));

                // Number plate
                Raylib.DrawRectangle(x + 26, y + 55, 32, 5, Color.White);
                
                break;

            case FacingDirection.Up:

                // wheels
                Raylib.DrawRectangle(x + 5, y + 32, 13, 32, Color.Black);
                Raylib.DrawRectangle(x + 68, y + 32, 13, 32, Color.Black);

                // Back view of SUV body
                Raylib.DrawRectangle(x + 5, y + 6, 76, 50, VehicleColor);
                Raylib.DrawRectangle(x + 5, y + 6, 76, 4,
                    new Color((byte)Math.Max(0,VehicleColor.R-40),(byte)Math.Max(0,VehicleColor.G-40),(byte)Math.Max(0,VehicleColor.B-40),(byte)255));

                // rear window
                Raylib.DrawRectangle(x + 9, y + 10, 68, 22,
                    new Color((byte)80,(byte)160,(byte)200,(byte)140));
                //Raylib.DrawRectangle(x + 48, y + 10, 4, 22, VehicleColor);

                // tail lights — wide
                Raylib.DrawRectangle(x + 7,  y + 34, 14, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                Raylib.DrawRectangle(x + 62, y + 34, 14, 10,
                    new Color((byte)220,(byte)30,(byte)30,(byte)255));
                // spare tyre
                Raylib.DrawCircle(x + 38, y + 30, 14, Color.Black);
                Raylib.DrawCircleLines(x + 38, y + 30, 14, Color.DarkGray);
                Raylib.DrawCircle(x + 38, y + 30, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));

                // tow hitch
                Raylib.DrawRectangle(x + 44, y + 48, 12, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));

                // exhaust pipe
                Raylib.DrawCircle(x + 11, y + 47, 3, Color.Black);
                Raylib.DrawCircleLines(x + 11, y + 47, 3, Color.Gray);

                // Number plate
                Raylib.DrawRectangle(x + 26, y + 55, 32, 5, Color.White);
                break;
        }
    }

    // ── Ambulance ──────────────────────────────────────────────────────────────────
    void DrawAmbulance()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
 // preserved wheel sizes/positions (same as SUV)
// colors
Color bodyWhite = new Color((byte)245, (byte)245, (byte)245, (byte)255);
Color redStripe = new Color((byte)200, (byte)20, (byte)20, (byte)255);
Color windowTint = new Color((byte)100, (byte)180, (byte)220, (byte)180);
Color sirenRed = new Color((byte)220, (byte)40, (byte)40, (byte)255);
Color sirenBlue = new Color((byte)40, (byte)100, (byte)220, (byte)255);
Color sirenBase = new Color((byte)200, (byte)200, (byte)200, (byte)255);

switch (Facing)
{
    case FacingDirection.Right:
        // boxy cargo area (keeps overall width/height same)
        Raylib.DrawRectangle(x, y + 6, 72, 44, bodyWhite);              // main box
        Raylib.DrawRectangle(x + 72, y + 12, 8, 38, VehicleColor);     // cab
        Raylib.DrawRectangle(x + 80, y + 30, 16, 20, VehicleColor);     // cab
        
        Raylib.DrawRectangle(x, y + 6, 72, 4, new Color((byte)230, (byte)230, (byte)230, (byte)255));
        // red stripe mid-body
        Raylib.DrawRectangle(x + 6, y + 22, 60, 8, redStripe);
        // large rear roll-up door lines to suggest box truck
        Raylib.DrawRectangleLines(x + 6, y + 10, 60, 36, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        for (int i = 14; i < 46; i += 8)
            Raylib.DrawRectangle(x + 8, y + i, 56, 3, new Color((byte)230,(byte)230,(byte)230,(byte)255));
        // small side window in cab
        Raylib.DrawRectangle(x + 68, y + 13, 9, 18, windowTint);

        // headlights/tail
        Raylib.DrawRectangle(x + 90, y + 30, 5, 5, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        Raylib.DrawRectangle(x, y + 28, 6, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        // preserved wheels (positions unchanged)
        Raylib.DrawCircle(x + 20, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 20, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle(x + 80, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        // Medical cross
        Raylib.DrawRectangle(x + 10, y + 18, 10, 4, Color.Red);
        Raylib.DrawRectangle(x + 13, y + 16, 4, 10, Color.Red);
        // siren on roof (centered)
        Raylib.DrawRectangle(x + 36, y - 2, 28, 8, sirenBase);
        Raylib.DrawRectangle(x + 38, y, 10, 6, sirenRed);
        Raylib.DrawRectangle(x + 50, y, 10, 6, sirenBlue);
        break;

    case FacingDirection.Left:
        // mirror layout: cab on left, box on right

        Raylib.DrawRectangle(x + 24, y + 6, 72, 44, bodyWhite);   // main box
        Raylib.DrawRectangle(x + 16, y + 12, 8, 38, VehicleColor);     // cab
        Raylib.DrawRectangle(x, y + 30, 16, 20, VehicleColor);     // cab

        Raylib.DrawRectangle(x + 24, y + 6, 72, 4, new Color((byte)230, (byte)230, (byte)230, (byte)255));
        Raylib.DrawRectangle(x + 30, y + 22, 60, 8, redStripe);
        // Roll up doors
        Raylib.DrawRectangleLines(x + 34, y + 10, 60, 36, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        for (int i = 14; i < 46; i += 8)
            Raylib.DrawRectangle(x + 38, y + i, 56, 3, new Color((byte)230,(byte)230,(byte)230,(byte)255));
        // Cab window
        Raylib.DrawRectangle(x + 17, y + 13, 9, 18, windowTint);
        // Head and tail lights
        Raylib.DrawRectangle(x + 90, y + 30, 6, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255)); 
        Raylib.DrawRectangle(x, y + 30, 5, 5, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        // wheels (same)
        Raylib.DrawCircle(x + 20, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 20, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 20, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        Raylib.DrawCircle(x + 80, y + 48, 10, Color.Black);
        Raylib.DrawCircleLines(x + 80, y + 48, 10, Color.DarkGray);
        Raylib.DrawCircle(x + 80, y + 48, 6, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        
        // mirrored medical cross
        Raylib.DrawRectangle(x + 82, y + 18, 10, 4, Color.Red);
        Raylib.DrawRectangle(x + 85, y + 16, 4, 10, Color.Red);
        // siren
        Raylib.DrawRectangle(x + 36, y - 2, 28, 8, sirenBase);
        Raylib.DrawRectangle(x + 38, y, 10, 6, sirenBlue);
        Raylib.DrawRectangle(x + 50, y, 10, 6, sirenRed);
        break;

    case FacingDirection.Down:
        // wheels (same)
        Raylib.DrawRectangle(x + 7, y + 32, 12, 28, Color.Black);
        Raylib.DrawRectangle(x + 60, y + 32, 12, 28, Color.Black);
        // front box face
        Raylib.DrawRectangle(x + 6, y + 6, 68, 48, bodyWhite);
        // cab windshield integrated at top center
        Raylib.DrawRectangle(x + 16, y + 16, 44, 12, windowTint);
        // red stripe across front
        Raylib.DrawRectangle(x + 16, y + 30, 44, 8, redStripe);
        // grille / bumper
        Raylib.DrawRectangle(x + 6, y + 46, 68, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));
        Raylib.DrawRectangle(x + 22, y + 36, 32, 6, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        // headlights
        Raylib.DrawRectangle(x + 9, y + 40, 8, 6, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        Raylib.DrawRectangle(x + 59, y + 40, 8, 6, new Color((byte)255,(byte)240,(byte)150,(byte)255));
        // siren centered on roof/front
        Raylib.DrawRectangle(x + 24, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 20, y + 6, 12, 6, sirenRed);
        Raylib.DrawRectangle(x + 42, y + 6, 12, 6, sirenBlue);
        break;

    case FacingDirection.Up:
        // wheels (same)
        Raylib.DrawRectangle(x + 7, y + 32, 12, 28, Color.Black);
        Raylib.DrawRectangle(x + 60, y + 32, 12, 28, Color.Black);
        // rear box face
        Raylib.DrawRectangle(x + 6, y + 6, 68, 48, bodyWhite);
        // rear window small
        Raylib.DrawRectangle(x + 22, y + 16, 16, 28, windowTint);
        Raylib.DrawRectangle(x + 42, y + 16, 16, 28, windowTint);
        // red stripe near middle
        Raylib.DrawRectangle(x + 16, y + 50, 44, 4, redStripe);
        
        // tail lights and bumper
        Raylib.DrawRectangle(x + 8, y + 38, 10, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangle(x + 62, y + 38, 10, 10, new Color((byte)220,(byte)30,(byte)30,(byte)255));
        //Raylib.DrawRectangle(x + 6, y + 44, 68, 6, new Color((byte)60,(byte)60,(byte)60,(byte)255));
        // rear door seams
        Raylib.DrawRectangleLines(x + 10, y + 12, 56, 32, new Color((byte)210,(byte)210,(byte)210,(byte)255));
        // roof ladder detail
        Raylib.DrawRectangle(x + 20, y + 8, 44, 4, new Color((byte)180,(byte)180,(byte)180,(byte)255));
        for (int r = 0; r < 4; r++)
            Raylib.DrawRectangle(x + 14 + r * 10, y + 12, 6, 3, new Color((byte)160,(byte)160,(byte)160,(byte)255));
        // siren rear-mounted orientation
        Raylib.DrawRectangle(x + 24, y + 4, 32, 8, sirenBase);
        Raylib.DrawRectangle(x + 26, y + 6, 12, 6, sirenBlue);
        Raylib.DrawRectangle(x + 42, y + 6, 12, 6, sirenRed);
        break;
}


    }
}
}
