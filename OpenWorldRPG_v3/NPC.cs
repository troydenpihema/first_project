using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    class NPC
{
    public Vector2 Position;

    public string Name;
    public string Dialogue;
    Vector2 wanderTarget;
    float wanderTimer = 0f;
    float speed = 60f;
    public bool HasSchedule = false;
    public Vector2 HomeAnchor;        // where they are at night
    public Vector2 DayAnchor;         // where they go during the day (e.g. daycare)
    public float DayStartHour = 8f;   // travels to DayAnchor after this
    public float DayEndHour = 17f;    // returns to HomeAnchor after this
    public float ScheduleSpeed = 90f;
    public bool AtDayAnchor = false;
    public bool Hidden = false;
    public bool IndoorsDuringDay = false;
    public bool IsParent = false;          // parent: drops off, leaves, returns to collect
    public float DropoffHour     = 8f;      // 8:00  everyone outside at the building
    public float ParentLeaveHour = 8.5f;    // 8:30  parents home; kids go indoors
    public float PickupHour      = 16.5f;   // 4:30  parents arrive & enter building
    public float ExitHour        = 16.833f; // 4:50  parents + kids exit, all outside
    public float GoHomeHour      = 17f;     // 5:00  everyone hidden (heading home)
    public bool DrawInsideNow = false;
    public string HomeBuilding = "DAYCARE";
    bool wasHidden = false;

    public NPC(Vector2 pos, string name, string dialogue)
    {
        Position = pos;
        Name = name;
        Dialogue = dialogue;
    }
public void Update(float dt)
{
    if (HasSchedule)
    {
        float hour = Program.GetCurrentHour();
        DrawInsideNow = false;

        bool atBuilding = hour >= DropoffHour && hour < GoHomeHour;
        if (!atBuilding)
        {
            if (!wasHidden) Position = HomeAnchor;   // snap home when the day ends
            Hidden = true; AtDayAnchor = false; wasHidden = true;
            return;
        }

        if (wasHidden) Position = HomeAnchor;        // snap home before the morning walk-in
        wasHidden = false;

        bool dropoff  = hour < ParentLeaveHour;                       // 8:00–8:30  all outside
        bool midday   = hour >= ParentLeaveHour && hour < PickupHour; // 8:30–4:30  kids indoors
        bool collect  = hour >= PickupHour && hour < ExitHour;        // 4:30–4:50  parents inside
        bool exiting  = hour >= ExitHour;                             // 4:50–5:00  all outside

        if (IsParent)
        {
            if (midday) { Hidden = true; AtDayAnchor = false; return; } // home 8:30–4:30
            if (collect)                                                // inside collecting
            {
                Hidden = true; AtDayAnchor = true; DrawInsideNow = true; return;
            }
            Hidden = false;                                             // dropoff + exiting: outside
        }
        else // kids / babies
        {
            if ((midday || collect) && IndoorsDuringDay)                // indoors 8:30–4:50
            {
                Hidden = true; AtDayAnchor = true; DrawInsideNow = true; return;
            }
            Hidden = false;                                             // dropoff + exiting: outside
        }

        Vector2 goal = DayAnchor;

        wanderTimer -= dt;
        if (wanderTimer <= 0)
        {
            wanderTarget = goal + new Vector2(
                Raylib.GetRandomValue(-30, 30),
                Raylib.GetRandomValue(-30, 30));
            wanderTimer = Raylib.GetRandomValue(2, 4);
        }

        float distToGoal = Vector2.Distance(Position, goal);
        if (distToGoal > 60f)
            Position += Vector2.Normalize(goal - Position) * ScheduleSpeed * dt;
        else
            Position = Vector2.Lerp(Position, wanderTarget, dt * 1.5f);

        AtDayAnchor = distToGoal <= 60f;
        return;
    }

    wanderTimer -= dt;

    if (wanderTimer <= 0)
    {
        wanderTarget = Position + new Vector2(
            Raylib.GetRandomValue(-80, 80),
            Raylib.GetRandomValue(-80, 80)
        );
        wanderTimer = Raylib.GetRandomValue(2, 5);
    }

    Position = Vector2.Lerp(Position, wanderTarget, dt * 1.5f);
}
public NPC(Vector2 pos)
{
    Position = pos;
    Name = "Citizen";
    Dialogue = "Hello there.";
}
    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 40, 60);

    public void Draw()
    {
        // head
        Raylib.DrawCircle(
            (int)Position.X + 20,
            (int)Position.Y + 12,
            12,
            Color.Beige
        );

        // shirt
        Raylib.DrawRectangle(
            (int)Position.X + 10,
            (int)Position.Y + 24,
            20,
            30,
            Color.Red
        );

        // legs
        Raylib.DrawRectangle(
            (int)Position.X + 10,
            (int)Position.Y + 54,
            8,
            12,
            Color.Black
        );

        Raylib.DrawRectangle(
            (int)Position.X + 22,
            (int)Position.Y + 54,
            8,
            12,
            Color.Black
        );
    }
}

}
