using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void AddDaycareFamilies(Vector2 bestStartAnchor, Vector2 homeAnchor)
        {
            string[] parentNames = { "Mara", "Tomas", "Ien", "Sela", "Rui" };
            string[] kidNames    = { "Pip", "Bo", "Juni", "Eden", "Ada" };

            for (int i = 0; i < parentNames.Length; i++)
            {
                Vector2 home = homeAnchor + new Vector2(i * 90, (i % 2) * 70);

                var parent = new NPC(home, parentNames[i], "Just dropping the little one at Best Start.");
                parent.HasSchedule = true;
                parent.HomeBuilding = "DAYCARE";
                parent.IsParent = true;
                parent.HomeAnchor = home;
                parent.DayAnchor  = bestStartAnchor + new Vector2(i * 24, 0);
                parent.DropoffHour = 8f; parent.ParentLeaveHour = 8.5f;
                parent.PickupHour = 16.5f; parent.ExitHour = 16.833f; parent.GoHomeHour = 17f;
                npcs.Add(parent);

                var kid = new NPC(home + new Vector2(20, 20), kidNames[i], "We play all day at daycare!");
                kid.HasSchedule = true;
                kid.HomeBuilding = "DAYCARE";
                kid.HomeAnchor = home + new Vector2(20, 20);
                kid.DayAnchor  = bestStartAnchor + new Vector2(i * 24, 40);
                kid.DropoffHour = 8f; kid.ParentLeaveHour = 8.5f;
                kid.PickupHour = 16.5f; kid.ExitHour = 16.833f; kid.GoHomeHour = 17f;
                kid.IndoorsDuringDay = true;
                npcs.Add(kid);

                var baby = new NPC(home + new Vector2(-16, 20), kidNames[i] + " Jr", "Goo goo!");
                baby.HasSchedule = true;
                baby.HomeBuilding = "DAYCARE";
                baby.HomeAnchor = home + new Vector2(-16, 20);
                baby.DayAnchor  = bestStartAnchor + new Vector2(i * 24, 64);
                baby.DropoffHour = 8f; baby.ParentLeaveHour = 8.5f;
                baby.PickupHour = 16.5f; baby.ExitHour = 16.833f; baby.GoHomeHour = 17f;
                baby.IndoorsDuringDay = true;
                npcs.Add(baby);
            }
        }

        static void AddSchoolFamilies(Vector2 schoolAnchor, Vector2 homeAnchor)
        {
            string[] parentNames = { "Ova", "Dax", "Lena", "Kip", "Nara" };
            string[] kidNames    = { "Theo", "Wren", "Cato", "Isa", "Rye" };

            for (int i = 0; i < parentNames.Length; i++)
            {
                Vector2 home = homeAnchor + new Vector2(i * 90, (i % 2) * 70);

                var parent = new NPC(home, parentNames[i], "Off to drop the kids at school.");
                parent.HasSchedule = true;
                parent.HomeBuilding = "SCHOOL";
                parent.IsParent = true;
                parent.HomeAnchor = home;
                parent.DayAnchor  = schoolAnchor + new Vector2(i * 24, 0);
                parent.DropoffHour = 8f; parent.ParentLeaveHour = 8.5f;
                parent.PickupHour = 16.5f; parent.ExitHour = 16.833f; parent.GoHomeHour = 17f;
                npcs.Add(parent);

                var kid = new NPC(home + new Vector2(20, 20), kidNames[i], "School's the best!");
                kid.HasSchedule = true;
                kid.HomeBuilding = "SCHOOL";
                kid.HomeAnchor = home + new Vector2(20, 20);
                kid.DayAnchor  = schoolAnchor + new Vector2(i * 24, 40);
                kid.DropoffHour = 8f; kid.ParentLeaveHour = 8.5f;
                kid.PickupHour = 16.5f; kid.ExitHour = 16.833f; kid.GoHomeHour = 17f;
                kid.IndoorsDuringDay = true;
                npcs.Add(kid);

                var baby = new NPC(home + new Vector2(-16, 20), kidNames[i] + " Jr", "Goo goo!");
                baby.HasSchedule = true;
                baby.HomeBuilding = "SCHOOL";
                baby.HomeAnchor = home + new Vector2(-16, 20);
                baby.DayAnchor  = schoolAnchor + new Vector2(i * 24, 64);
                baby.DropoffHour = 8f; baby.ParentLeaveHour = 8.5f;
                baby.PickupHour = 16.5f; baby.ExitHour = 16.833f; baby.GoHomeHour = 17f;
                baby.IndoorsDuringDay = true;
                npcs.Add(baby);
            }
        }

static void AddGarageForHouse(int houseIndex, float x, float y, int capacity = 1, bool isDock = false)
{
    garages.Add(new Garage {
        Bounds = new Rectangle(x, y, 100 + capacity * 60, 100),   // wider per car tier
        OwnerHouseIndex = houseIndex,
        Capacity = capacity,
        IsDock = isDock
    });
}

static void SpawnStructuresForHouse(int houseIndex, float x, float y, string houseType)
{
    switch (houseType)
    {
        case "Farmhouse":   // 1-car garage + basic stable (horse/donkey/camel)
            AddGarageForHouse(houseIndex, x + 300, y);
            AddStableForHouse(houseIndex, x + 300, y + 120, Stable.StableKind.Basic);
            break;
        case "OceanHouse":  // 2-car garage + bike rack + aquatic pen with boat dock
            AddGarageForHouse(houseIndex, x + 300, y, capacity: 2);
            AddStableForHouse(houseIndex, x + 300, y + 120, Stable.StableKind.BikeRack);
            AddStableForHouse(houseIndex, x - 220, y + 40, Stable.StableKind.Aquatic);      // dolphin pen
            AddGarageForHouse(houseIndex, x - 220, y + 200, capacity: 1, isDock: true);     // boat storage
            break;
        case "Mansion":     // 3-car garage + advanced stable (any land animal, incl. elephant/tiger/reindeer)
            AddGarageForHouse(houseIndex, x + 300, y, capacity: 3);
            AddStableForHouse(houseIndex, x + 300, y + 120, Stable.StableKind.Advanced, capacity: 2);
            break;
        default:
            AddGarageForHouse(houseIndex, x + 300, y);
            break;
    }
}

static void AddStableForHouse(int houseIndex, float x, float y, Stable.StableKind kind, int capacity = 1)
{
    (int w, int h) = kind switch
    {
        Stable.StableKind.Advanced => (190, 130),
        Stable.StableKind.BikeRack => (90, 50),
        Stable.StableKind.Aquatic  => (180, 140),
        _                          => (150, 110)
    };
    stables.Add(new Stable { Kind = kind, Bounds = new Rectangle(x, y, w, h), OwnerHouseIndex = houseIndex, Capacity = capacity });
}

        static void RemoveCampfireAt(int idx)
        {
            campfirePositions.RemoveAt(idx);
            var newLogs = new Dictionary<int, int>();
            var newBurn = new Dictionary<int, float>();
            foreach (var kv in campfireLogs) if (kv.Key != idx) newLogs[kv.Key > idx ? kv.Key - 1 : kv.Key] = kv.Value;
            foreach (var kv in campfireBurn) if (kv.Key != idx) newBurn[kv.Key > idx ? kv.Key - 1 : kv.Key] = kv.Value;
            campfireLogs = newLogs;
            campfireBurn = newBurn;
        }

        static void AddMailbox(float x, float y, int houseIndex)
        {
            extraMailboxes.Add((new Vector2(x, y), houseIndex));
        }

static bool AddToCart(string name)
{
    var inv = player.HasTrolley ? trolleyInventory : basketInventory;
    int cap  = player.HasTrolley ? 20 : 10;
    int used = inv.Count(s => s != null);
    if (used >= cap) return false;
    for (int i = 0; i < inv.Count; i++)
        if (inv[i] == null) { inv[i] = name; return true; }
    return false;
}

static void AddCasino(float x, float y)
{
    var c = new Building(
        new Rectangle(x, y, 220, 140),
        new Color(80, 10, 80, 255),
        new Color(50, 5, 50, 255),
        new Vector2(x + 110, y + 100),
        "Casino",
        new NPC(new Vector2(700, 140), "Casino Host", "Welcome bro, buy some chips and try your luck!"),
        entryPos: new Vector2(700, 900)
    );
    c.InteriorObjects.Clear();
    c.InteriorObjects.Add(new Rectangle(500, 80, 400, 40));   // chip desk
    c.InteriorObjects.Add(new Rectangle(100, 300, 200, 120)); // blackjack table 1
    c.InteriorObjects.Add(new Rectangle(400, 300, 200, 120)); // blackjack table 2
    c.InteriorObjects.Add(new Rectangle(800, 280, 200, 140)); // roulette table
    c.InteriorObjects.Add(new Rectangle(100, 600, 80, 80));   // pokie 1
    c.InteriorObjects.Add(new Rectangle(220, 600, 80, 80));   // pokie 2
    c.InteriorObjects.Add(new Rectangle(340, 600, 80, 80));   // pokie 3
    buildings.Add(c);
}

static void AddAirport(float x, float y)
{
    var a = new Building(
        new Rectangle(x, y, 400, 200),
        new Color(220, 220, 225, 255),
        new Color(200, 210, 215, 255),
        new Vector2(x + 200, y + 160),
        "Airport",
        new NPC(new Vector2(700, 160), "Check-in Staff", "Welcome to the airport bro, where ya headed?"),
        entryPos: new Vector2(700, 900)
    );
    a.InteriorObjects.Clear();
    a.InteriorObjects.Add(new Rectangle(400, 100, 400, 40));   // check-in counter
    a.InteriorObjects.Add(new Rectangle(100, 200, 200, 40));   // security desk
    a.InteriorObjects.Add(new Rectangle(800, 200, 200, 40));   // departure gate A
    a.InteriorObjects.Add(new Rectangle(1000, 200, 200, 40));  // departure gate B
    a.InteriorObjects.Add(new Rectangle(100, 500, 120, 80));   // seating block 1
    a.InteriorObjects.Add(new Rectangle(300, 500, 120, 80));   // seating block 2
    a.InteriorObjects.Add(new Rectangle(700, 500, 120, 80));   // seating block 3
    a.InteriorObjects.Add(new Rectangle(900, 500, 120, 80));   // seating block 4
    buildings.Add(a);
}

static void AddSchool(float x, float y)
{
    var school = new Building(
        new Rectangle(x, y, 500, 380),
        new Color(160, 30, 30, 255),
        new Color(90, 20, 20, 255),
        new Vector2(x + 250, y + 340),
        "SCHOOL",
        new NPC(new Vector2(1665, 100), "Teacher", "Take a seat — class is about to start."),
        entryPos: new Vector2(920, 100)
    );
    school.InteriorObjects.Clear();

    // local helper — mirrors DrawSchoolHallInterior's Room() wall geometry, split around the door gap
    void RoomWalls(int rx, int ry, int rw, int rh, int doorSide, int doorOffset, int doorW)
    {
        if (doorSide != 0) school.InteriorObjects.Add(new Rectangle(rx, ry, rw, 12));                     // top
        if (doorSide != 1) school.InteriorObjects.Add(new Rectangle(rx, ry + rh - 12, rw, 12));           // bottom
        if (doorSide != 2) school.InteriorObjects.Add(new Rectangle(rx, ry, 12, rh));                     // left
        if (doorSide != 3) school.InteriorObjects.Add(new Rectangle(rx + rw - 12, ry, 12, rh));           // right

        if (doorSide == 0) { school.InteriorObjects.Add(new Rectangle(rx, ry, doorOffset, 12)); school.InteriorObjects.Add(new Rectangle(rx + doorOffset + doorW, ry, rw - doorOffset - doorW, 12)); }
        if (doorSide == 1) { school.InteriorObjects.Add(new Rectangle(rx, ry + rh - 12, doorOffset, 12)); school.InteriorObjects.Add(new Rectangle(rx + doorOffset + doorW, ry + rh - 12, rw - doorOffset - doorW, 12)); }
        if (doorSide == 2) { school.InteriorObjects.Add(new Rectangle(rx, ry, 12, doorOffset)); school.InteriorObjects.Add(new Rectangle(rx, ry + doorOffset + doorW, 12, rh - doorOffset - doorW)); }
        if (doorSide == 3) { school.InteriorObjects.Add(new Rectangle(rx + rw - 12, ry, 12, doorOffset)); school.InteriorObjects.Add(new Rectangle(rx + rw - 12, ry + doorOffset + doorW, 12, rh - doorOffset - doorW)); }
    }

    RoomWalls(150, 300, 350, 400, 1, 90, 60);    // Maths
    RoomWalls(500, 300, 350, 400, 1, 90, 60);    // Art
    RoomWalls(850, 300, 350, 400, 1, 90, 60);    // Science
    RoomWalls(150, 900, 350, 400, 0, 90, 60);    // Sports
    RoomWalls(500, 900, 350, 400, 0, 90, 60);    // English
    RoomWalls(150, 1650, 350, 350, 0, 90, 60);   // Boys
    RoomWalls(500, 1650, 350, 350, 0, 90, 60);   // Girls
    RoomWalls(850, 1650, 350, 350, 0, 90, 60);   // Gym
    RoomWalls(1200, 1200, 800, 800, 0, 300, 80); // Cafeteria seating

    // furniture / open-zone collision — matches DrawSchoolHallInterior's drawn pieces
    school.InteriorObjects.Add(new Rectangle(1520, 100, 300, 60));   // office desk
    school.InteriorObjects.Add(new Rectangle(1800, 400, 200, 300));  // trophy cabinet
    for (int t = 0; t < 3; t++)
        school.InteriorObjects.Add(new Rectangle(1670 + t * 105, 970, 90, 70)); // servery counters

    for (int row = 0; row < 4; row++)
        for (int col = 0; col < 3; col++)
            school.InteriorObjects.Add(new Rectangle(1350 + col * 200, 1320 + row * 200, 100, 50)); // cafeteria tables

    for (int ly = 60; ly < 1900; ly += 100)
        school.InteriorObjects.Add(new Rectangle(30, ly, 90, 80)); // lockers

    school.InteriorObjects.Add(new Rectangle(200, 1350, 250, 40)); // bench 1
    school.InteriorObjects.Add(new Rectangle(500, 1350, 250, 40)); // bench 2

    // gym door to basketball court (matches gymDoorPos interaction, small non-blocking marker not needed here)

    buildings.Add(school);
}

static void DrawSchoolExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color brick = new Color((byte)150,(byte)45,(byte)45,(byte)255);
    Color mortar = new Color((byte)110,(byte)30,(byte)30,(byte)255);
    Color roofCol = new Color((byte)70,(byte)55,(byte)45,(byte)255);
    Color trim = new Color((byte)230,(byte)220,(byte)200,(byte)255);

    int outerW = 500, outerH = 380;

    // solid building block (was a hollow brick ring around a field — now a real facade)
    Raylib.DrawRectangle(bx, by, outerW, outerH, brick);
    for (int row = 0; row < outerH; row += 12)
        Raylib.DrawRectangle(bx, by + row, outerW, 2, mortar);

    // roof band along the top
    Raylib.DrawRectangle(bx - 8, by - 16, outerW + 16, 18, roofCol);
    Raylib.DrawRectangle(bx - 8, by - 16, outerW + 16, 4, new Color((byte)90,(byte)72,(byte)58,(byte)255));

    // trim band under the roof
    Raylib.DrawRectangle(bx, by, outerW, 10, trim);

    // rows of classroom windows across the facade
    for (int wx = bx + 20; wx < bx + outerW - 20; wx += 55)
    {
        Raylib.DrawRectangle(wx, by + 40, 34, 40, new Color((byte)160,(byte)200,(byte)220,(byte)210));
        Raylib.DrawRectangleLines(wx, by + 40, 34, 40, trim);
        Raylib.DrawRectangle(wx + 15, by + 40, 4, 40, trim); // window divider

        Raylib.DrawRectangle(wx, by + 220, 34, 40, new Color((byte)160,(byte)200,(byte)220,(byte)210));
        Raylib.DrawRectangleLines(wx, by + 220, 34, 40, trim);
        Raylib.DrawRectangle(wx + 15, by + 220, 4, 40, trim);
    }

    // main entrance
    int doorW = 90, doorX = bx + outerW/2 - doorW/2;
    Raylib.DrawRectangle(doorX, by + outerH - 60, doorW, 60, new Color((byte)80,(byte)55,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(doorX, by + outerH - 60, doorW, 60, trim);
    Raylib.DrawRectangle(doorX + doorW/2 - 2, by + outerH - 60, 4, 60, trim); // double-door split
    Raylib.DrawRectangle(doorX - 10, by + outerH - 68, doorW + 20, 10, trim); // entrance awning

    // steps leading up to the door
    Raylib.DrawRectangle(doorX - 14, by + outerH, doorW + 28, 8, new Color((byte)200,(byte)195,(byte)185,(byte)255));
    Raylib.DrawRectangle(doorX - 20, by + outerH + 8, doorW + 40, 8, new Color((byte)190,(byte)185,(byte)175,(byte)255));

    // flagpole
    Raylib.DrawRectangle(bx + 30, by + outerH - 90, 4, 90, new Color((byte)150,(byte)150,(byte)150,(byte)255));
    Raylib.DrawRectangle(bx + 34, by + outerH - 88, 26, 16, new Color((byte)60,(byte)90,(byte)170,(byte)255));

    // sign above entrance
    Raylib.DrawRectangle(bx + outerW/2 - 90, by - 40, 180, 20, new Color((byte)70,(byte)55,(byte)45,(byte)255));
    Program.DrawTextUI("SCHOOL", bx + outerW/2 - 40, by - 37, 20, trim);
}

static void DrawPrisonExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color concrete = new Color((byte)120,(byte)120,(byte)128,(byte)255);
    Color seam     = new Color((byte)90,(byte)90,(byte)98,(byte)255);
    Color roofCol  = new Color((byte)55,(byte)55,(byte)62,(byte)255);
    Color trim     = new Color((byte)200,(byte)200,(byte)205,(byte)255);
    Color barCol   = new Color((byte)30,(byte)30,(byte)35,(byte)255);
    Color yardGrey = new Color((byte)150,(byte)150,(byte)155,(byte)255);
    Color fence    = new Color((byte)70,(byte)70,(byte)78,(byte)255);
    Color wire     = new Color((byte)205,(byte)205,(byte)210,(byte)255);

    int outerW = 500, outerH = 380;

    // ── ADDITIVE YARDS (drawn first, around the footprint — MARAE/airport style) ──
    // left exercise yard
    void Yard(int yx, int yy, int yw, int yh, string label)
    {
        Raylib.DrawRectangle(yx, yy, yw, yh, yardGrey);
        Raylib.DrawRectangleLinesEx(new Rectangle(yx, yy, yw, yh), 3, fence);
        // chain-link cross-hatch
        for (int fx = yx; fx < yx + yw; fx += 20)
            Raylib.DrawLine(fx, yy, fx, yy + yh, new Color((byte)110,(byte)110,(byte)115,(byte)110));
        for (int fy = yy; fy < yy + yh; fy += 20)
            Raylib.DrawLine(yx, fy, yx + yw, fy, new Color((byte)110,(byte)110,(byte)115,(byte)90));
        // razor-wire coil along the top rail
        for (int wx = yx + 8; wx < yx + yw - 8; wx += 24)
            Raylib.DrawCircleLines(wx, yy, 7, wire);
        Program.DrawTextUI(label, yx + 8, yy + 6, 14, new Color((byte)80,(byte)80,(byte)88,(byte)255));
    }
    Yard(bx - 210, by + 30, 200, 320, "YARD");                       // left exercise yard
    Yard(bx + outerW + 10, by + 30, 200, 320, "YARD");               // right exercise yard

    // basketball hoop + half-court markings in the left yard (ambience, like airport runway hints)
    Raylib.DrawCircleLines(bx - 110, by + 190, 60, new Color((byte)200,(byte)90,(byte)60,(byte)150));
    Raylib.DrawRectangle(bx - 113, by + 44, 6, 34, roofCol);
    Raylib.DrawRectangleLines(bx - 124, by + 78, 28, 16, new Color((byte)220,(byte)140,(byte)40,(byte)255));
    // picnic-style benches in the right yard
    Raylib.DrawRectangle(bx + outerW + 40, by + 120, 90, 20, new Color((byte)110,(byte)110,(byte)118,(byte)255));
    Raylib.DrawRectangle(bx + outerW + 40, by + 220, 90, 20, new Color((byte)110,(byte)110,(byte)118,(byte)255));

    // watchtowers at the outer yard corners
    foreach (var (cx, cy) in new[] { (bx - 216, by + 24), (bx + outerW + 204, by + 24),
                                     (bx - 216, by + 344), (bx + outerW + 204, by + 344) })
    {
        Raylib.DrawRectangle(cx, cy, 26, 26, roofCol);
        Raylib.DrawRectangle(cx + 3, cy + 3, 20, 12, new Color((byte)90,(byte)130,(byte)160,(byte)200)); // cab glass
        Raylib.DrawRectangleLines(cx, cy, 26, 26, Color.Black);
    }

    // ── MAIN CELLBLOCK FACADE (school-style solid block) ──
    Raylib.DrawRectangle(bx, by, outerW, outerH, concrete);
    for (int row = 0; row < outerH; row += 22)
        Raylib.DrawRectangle(bx, by + row, outerW, 2, seam);
    for (int row = 0; row < outerH; row += 22)
        for (int cx = ((row / 22) % 2 == 0 ? 0 : 34); cx < outerW; cx += 68)
            Raylib.DrawRectangle(bx + cx, by + row, 2, 22, seam);

    // flat roof band along the top
    Raylib.DrawRectangle(bx - 8, by - 16, outerW + 16, 18, roofCol);
    Raylib.DrawRectangle(bx - 8, by - 16, outerW + 16, 4, new Color((byte)75,(byte)75,(byte)82,(byte)255));
    // razor wire along the roofline
    for (int wx = bx + 6; wx < bx + outerW - 6; wx += 26)
        Raylib.DrawCircleLines(wx, by - 20, 8, wire);

    // trim band under the roof
    Raylib.DrawRectangle(bx, by, outerW, 10, trim);

    // rows of BARRED windows across the facade (school window loop, with bars)
    for (int wx = bx + 20; wx < bx + outerW - 20; wx += 55)
    {
        foreach (int wy in new[] { by + 40, by + 220 })
        {
            Raylib.DrawRectangle(wx, wy, 34, 40, new Color((byte)90,(byte)120,(byte)140,(byte)210));
            Raylib.DrawRectangleLines(wx, wy, 34, 40, trim);
            for (int b = wx + 6; b < wx + 34; b += 8)
                Raylib.DrawRectangle(b, wy, 3, 40, barCol); // vertical bars
        }
    }

    // main entrance — reinforced sally-port door
    int doorW = 90, doorX = bx + outerW/2 - doorW/2;
    Raylib.DrawRectangle(doorX, by + outerH - 60, doorW, 60, new Color((byte)45,(byte)45,(byte)50,(byte)255));
    Raylib.DrawRectangleLines(doorX, by + outerH - 60, doorW, 60, trim);
    Raylib.DrawRectangle(doorX + doorW/2 - 2, by + outerH - 60, 4, 60, trim); // door split
    for (int b = doorX + 10; b < doorX + doorW; b += 14)
        Raylib.DrawRectangle(b, by + outerH - 56, 3, 52, new Color((byte)90,(byte)90,(byte)100,(byte)255)); // gate bars
    Raylib.DrawRectangle(doorX - 10, by + outerH - 68, doorW + 20, 10, roofCol); // awning

    // steps leading up to the door
    Raylib.DrawRectangle(doorX - 14, by + outerH, doorW + 28, 8, new Color((byte)175,(byte)175,(byte)180,(byte)255));
    Raylib.DrawRectangle(doorX - 20, by + outerH + 8, doorW + 40, 8, new Color((byte)160,(byte)160,(byte)166,(byte)255));

    // guard tower rising from the roof (like the airport control tower)
    Raylib.DrawRectangle(bx + 24, by - 76, 30, 62, new Color((byte)100,(byte)100,(byte)108,(byte)255));
    Raylib.DrawRectangle(bx + 14, by - 88, 50, 16, new Color((byte)90,(byte)130,(byte)160,(byte)255)); // cab
    Raylib.DrawRectangleLines(bx + 14, by - 88, 50, 16, roofCol);
    Raylib.DrawCircle(bx + 39, by - 94, 4, new Color((byte)220,(byte)40,(byte)40,(byte)255)); // searchlight beacon

    // sign above entrance
    Raylib.DrawRectangle(bx + outerW/2 - 90, by - 40, 180, 20, new Color((byte)40,(byte)40,(byte)45,(byte)255));
    Program.DrawTextUI("PRISON", bx + outerW/2 - 40, by - 37, 20, trim);
}

static void AddPrison(float x, float y)
{
    var prison = new Building(
        new Rectangle(x, y, 220, 160),
        new Color(70, 70, 75, 255),
        new Color(35, 35, 40, 255),
        new Vector2(x + 110, y + 120),
        "PRISON",
        new NPC(new Vector2(600, 120), "Guard", "Do your time or pay it off."),
        entryPos: new Vector2(640, 880)
    );
    prison.InteriorObjects.Clear();
    prison.InteriorObjects.Add(new Rectangle(1400, 300, 1400, 80));    // guard desk
    prison.InteriorObjects.Add(new Rectangle(200, 1000, 400, 400));    // cell block 1
    prison.InteriorObjects.Add(new Rectangle(800, 1000, 400, 400));    // cell block 2
    prison.InteriorObjects.Add(new Rectangle(1400, 1000, 400, 400));   // cell block 3
    prison.InteriorObjects.Add(new Rectangle(200, 1500, 1600, 60));    // back wall walkway
    // facility-door surrounds along bottom wall (gaps left at door centres)
    prison.InteriorObjects.Add(new Rectangle(0, 1690, 300, 40));       // wall before LAUNDRY
    prison.InteriorObjects.Add(new Rectangle(420, 1690, 520, 40));     // between LAUNDRY & KITCHEN
    prison.InteriorObjects.Add(new Rectangle(1060, 1690, 520, 40));    // between KITCHEN & TOILETS
    prison.InteriorObjects.Add(new Rectangle(1700, 1690, 300, 40));    // wall after TOILETS
    prison.InteriorObjects.Add(new Rectangle(1400, 300, 1400, 80));    // guard desk
    prison.InteriorObjects.Add(new Rectangle(200, 1000, 400, 400));    // cell block 1
    prison.InteriorObjects.Add(new Rectangle(800, 1000, 400, 400));    // cell block 2
    prison.InteriorObjects.Add(new Rectangle(1400, 1000, 400, 400));   // cell block 3
    prison.InteriorObjects.Add(new Rectangle(200, 1500, 1600, 60));    // back wall walkway
    buildings.Add(prison);
}

static void AddHobbiesStore(float x, float y)
{
    var shack = new NPC(new Vector2(530, 20), "Shack", "Goodluck on the rips!, my luck has has been terrible");
    shack.SpriteKey = "Shack";        
    shack.Facing = NPC.Dir.South; 

    var shop = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(220, 60, 60, 255),
        new Color(240, 220, 60, 255),
        new Vector2(x + 80, y + 100),
        "HOBBIES STORE",
        shack,
        entryPos: new Vector2(640, 880)
    );
    shop.InteriorObjects.Clear();
shop.InteriorObjects.Add(new Rectangle(400, 80, 400, 40));    // counter
shop.InteriorObjects.Add(new Rectangle(100, 200, 180, 100));  // card rack left
shop.InteriorObjects.Add(new Rectangle(350, 200, 180, 100));  // card rack mid
shop.InteriorObjects.Add(new Rectangle(600, 200, 180, 100));  // card rack right
shop.InteriorObjects.Add(new Rectangle(850, 200, 180, 100));
    buildings.Add(shop);
}

static void DrawHobbiesStoreExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color red = new Color((byte)210,(byte)55,(byte)55,(byte)255);
    Color yellow = new Color((byte)235,(byte)215,(byte)55,(byte)255);

    Raylib.DrawRectangle(bx, by, 160, 120, red);
    Raylib.DrawRectangle(bx - 5, by - 14, 170, 16, yellow);
    Raylib.DrawRectangle(bx - 5, by - 14, 170, 4, new Color((byte)250,(byte)235,(byte)90,(byte)255));

    // pack-of-cards logo — three overlapping tilted rectangles
    Raylib.DrawRectanglePro(new Rectangle(bx + 65, by + 15, 22, 30), new Vector2(11, 15), -12f, new Color((byte)40,(byte)90,(byte)200,(byte)255));
    Raylib.DrawRectanglePro(new Rectangle(bx + 78, by + 18, 22, 30), new Vector2(11, 15), 0f, new Color((byte)230,(byte)230,(byte)230,(byte)255));
    Raylib.DrawRectanglePro(new Rectangle(bx + 91, by + 15, 22, 30), new Vector2(11, 15), 12f, new Color((byte)200,(byte)40,(byte)60,(byte)255));

    // windows with card displays
    Raylib.DrawRectangle(bx + 10, by + 55, 45, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines(bx + 10, by + 55, 45, 45, yellow);
    Raylib.DrawRectangle(bx + 20, by + 65, 10, 14, new Color((byte)230,(byte)190,(byte)60,(byte)255));
    Raylib.DrawRectangle(bx + 33, by + 65, 10, 14, new Color((byte)160,(byte)90,(byte)220,(byte)255));

    Raylib.DrawRectangle(bx + 105, by + 55, 45, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines(bx + 105, by + 55, 45, 45, yellow);
    Raylib.DrawRectangle(bx + 115, by + 65, 10, 14, new Color((byte)90,(byte)200,(byte)90,(byte)255));
    Raylib.DrawRectangle(bx + 128, by + 65, 10, 14, new Color((byte)220,(byte)140,(byte)60,(byte)255));

    // door
    Raylib.DrawRectangle(bx + 62, by + 78, 36, 42, new Color((byte)140,(byte)90,(byte)45,(byte)255));
    Raylib.DrawRectangleLines(bx + 62, by + 78, 36, 42, yellow);
    Raylib.DrawCircle(bx + 92, by + 100, 2, new Color((byte)230,(byte)200,(byte)90,(byte)255));

    // sign
    Raylib.DrawRectangle(bx + 15, by - 34, 130, 18, new Color((byte)30,(byte)20,(byte)10,(byte)220));
    Program.DrawTextUI("HOBBIES STORE", bx + 20, by - 31, 12, yellow);

    // sandwich board on the footpath with card art
    Raylib.DrawTriangle(new Vector2(bx - 30, by + 130), new Vector2(bx - 10, by + 100), new Vector2(bx + 10, by + 130), new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawRectangle(bx - 24, by + 105, 28, 22, new Color((byte)230,(byte)230,(byte)230,(byte)255));
    Program.DrawTextUI("PACKS $5", bx - 22, by + 110, 8, Color.Black);
}

static void AddJobCentre(float x, float y)
{
    var jc = new Building(
        new Rectangle(x, y, 180, 130),
        new Color(70, 90, 140, 255), new Color(35, 45, 70, 255),
        new Vector2(x + 90, y + 110), "JOB CENTRE",
        new NPC(new Vector2(640, 250), "Job Broker", "Looking for work? Bring me the goods, I'll pay cash."),
        entryPos: new Vector2(640, 850));
    jc.InteriorObjects.Clear();
    jc.InteriorObjects.Add(new Rectangle(440, 180, 400, 50));   // broker counter
    jc.InteriorObjects.Add(new Rectangle(100, 400, 60, 300));   // notice board wall
    buildings.Add(jc);
}

static void AddMiniGolf(float x, float y)
{
    var golf = new Building(
        new Rectangle(x, y, 180, 140),
        new Color(40, 110, 50, 255),          // green exterior
        new Color(30, 70, 40, 255),            // interior bg
        new Vector2(x + 90, y + 100),
        "MiniGolf",
        new NPC(new Vector2(640, 640), "Golf Pro", "9 holes of putt-putt fun! Beat par for a cash prize."),
        entryPos: new Vector2(640, 640)
    );

    golf.InteriorObjects.Clear();
    golf.InteriorObjects.Add(new Rectangle(540, 300, 200, 90));   // the course kiosk / start tee

    buildings.Add(golf);
}

        static void AddDealerBuilding(float x, float y, string name, DealerType type)
{
    var b = new Building(
        new Rectangle(x, y, 260, 160),
        new Color(180, 180, 200, 255),
        new Color(210, 210, 220, 255),
        new Vector2(x + 120, y + 150),
        name,
        new NPC(new Vector2(700, 120), "Dealer", $"Welcome to the {name}! E to browse."),
        entryPos: new Vector2(700, 900)
    );

    b.InteriorObjects.Clear();

    // simple counter and display area
    b.InteriorObjects.Add(new Rectangle(80, 140, 620, 60));  // counter
    b.InteriorObjects.Add(new Rectangle(80, 220, 600, 300)); // showroom floor (interactive)
    buildings.Add(b);

    // register a special NPC for this building so we can open dealer UI (store the building ref)
    if (type == DealerType.Bike) dealerVehicleOptions.Clear(); // repurpose lists later
    // We'll identify this building by its name when inside to show dealer UI
}

static void AddDominos(float x, float y)
{
    var dom = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(0, 75, 155, 255),
        new Color(20, 20, 20, 255),
        new Vector2(x + 100, y + 100),
        "DOMINO'S",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to Domino's! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    dom.InteriorObjects.Clear();
 
    // Counter
    dom.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    dom.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    dom.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    dom.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        dom.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        dom.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Pizza oven area wall
    dom.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(dom);
}

static void AddKFC(float x, float y)
{
    var kfc = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(180, 20, 20, 255),
        new Color(30, 15, 10, 255),
        new Vector2(x + 100, y + 100),
        "KFC",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to KFC! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    kfc.InteriorObjects.Clear();
 
    // Counter
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    kfc.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    kfc.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        kfc.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        kfc.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Kitchen wall
    kfc.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(kfc);
}

static void AddBurgerKing(float x, float y)
{
    var bk = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(210, 80, 0, 255),
        new Color(25, 15, 5, 255),
        new Vector2(x + 100, y + 100),
        "BURGER KING",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to Burger King! Press E to order."),
        entryPos: new Vector2(700, 900)
    );
 
    bk.InteriorObjects.Clear();
 
    // Counter
    bk.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    bk.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    bk.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    bk.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));
 
    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        bk.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        bk.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }
 
    // Flame grill area wall
    bk.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));
 
    buildings.Add(bk);
}

      static void AddGasStation(float x, float y)
{
    var gasBuilding = new Building(
        new Rectangle(x + 300, y - 780, 260, 160),
        new Color(220, 220, 180, 255),
        new Color(200, 195, 160, 255),
        new Vector2(x + 280, y - 600),
        "GAS STATION",
        new NPC(new Vector2(1000, 150), "Attendant", "Pay for your fuel here bro."),
        entryPos: new Vector2(580, 870)
    );
 
    gasBuilding.InteriorObjects.Clear();
 
    // --- service counter (right side) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 380, 50));   // main counter
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 380, 8));    // counter top edge
    gasBuilding.InteriorObjects.Add(new Rectangle(800, 80, 8, 50));     // left side
    gasBuilding.InteriorObjects.Add(new Rectangle(1172, 80, 8, 50));    // right side
 
    // --- shelving aisles (snack/drink shop) ---
    // aisle 1
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  200, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  260, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  320, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  380, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  440, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(80,  500, 180, 30));
 
    // aisle 2
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 200, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 260, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 320, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 380, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 440, 180, 30));
    gasBuilding.InteriorObjects.Add(new Rectangle(340, 500, 180, 30));
 
    // --- drink fridges (back wall) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(600,  80, 60, 120));
    gasBuilding.InteriorObjects.Add(new Rectangle(680,  80, 60, 120));
    gasBuilding.InteriorObjects.Add(new Rectangle(760,  80, 60, 120));
 
    // --- oil/auto products wall (left wall) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(20, 200, 40, 400));
 
    // --- toilets (back corner) ---
    gasBuilding.InteriorObjects.Add(new Rectangle(1080, 200, 120, 120)); // toilet room
 
    // --- waiting bench ---
    gasBuilding.InteriorObjects.Add(new Rectangle(80, 620, 240, 30));
 
    // entrance mat
    //gasBuilding.InteriorObjects.Add(new Rectangle(480, 870, 200, 30));
 
    buildings.Add(gasBuilding);
 
    // add to gas stations list for pump logic
    gasStations.Add(new GasStation(x, y));
}

static void AddSwimmingComplex(float x, float y)
{
    var pool = new Building(
        new Rectangle(x, y, 300, 200),
        new Color(30, 120, 200, 255),
        new Color(20, 160, 220, 255),
        new Vector2(x + 150, y + 160),
        "SWIMMING COMPLEX",
        new NPC(new Vector2(700, 100), "Lifeguard", "Lane pool: swim laps. Diving pool: time your jump!"),
        entryPos: new Vector2(700, 900)
    );

    pool.InteriorObjects.Clear();

    // Lane pool walls
    pool.InteriorObjects.Add(new Rectangle(50, 100, 500, 20));   // top wall
    pool.InteriorObjects.Add(new Rectangle(50, 420, 500, 20));   // bottom wall
    pool.InteriorObjects.Add(new Rectangle(50, 100, 20, 340));   // left wall
    pool.InteriorObjects.Add(new Rectangle(530, 100, 20, 340));  // right wall

    // Lane dividers (can't walk through)
  //  for (int lane = 0; lane < 4; lane++)
   //     pool.InteriorObjects.Add(new Rectangle(52, 152 + lane * 60, 496, 8));

    // Diving pool walls
    pool.InteriorObjects.Add(new Rectangle(700, 100, 450, 20));  // top
    pool.InteriorObjects.Add(new Rectangle(700, 500, 450, 20));  // bottom
    pool.InteriorObjects.Add(new Rectangle(700, 100, 20, 420));  // left
    pool.InteriorObjects.Add(new Rectangle(1130, 100, 20, 420)); // right

    // Diving board platform (collision - player stands on edge)
    pool.InteriorObjects.Add(new Rectangle(700, 180, 80, 20));   // board platform base
    pool.InteriorObjects.Add(new Rectangle(700, 160, 20, 360));  // left wall of dive area

    buildings.Add(pool);
}

static void AddTennisCourt(float x, float y)
{
    var court = new Building(
        new Rectangle(x, y, 200, 120),
        new Color(180, 120, 40, 255),
        new Color(60, 160, 60, 255),
        new Vector2(x + 100, y + 100),
        "TENNIS COURT",
        new NPC(new Vector2(700, 100), "Tennis Coach", "Press E near the net to start a game! WASD to move your paddle."),
        entryPos: new Vector2(700, 920)
    );

    court.InteriorObjects.Clear();
    // Net post collision
    court.InteriorObjects.Add(new Rectangle(670, 80, 20, 560));

    buildings.Add(court);
}

static void AddBasketballCourt(float x, float y)
{
    var court = new Building(
        new Rectangle(x, y, 200, 120),
        new Color(200, 100, 20, 255),
        new Color(200, 140, 60, 255),
        new Vector2(x + 100, y + 100),
        "BASKETBALL COURT",
        new NPC(new Vector2(700, 100), "Coach", "Shoot hoops! Stand at the free throw line and press F. Lock power, then lock aim!"),
        entryPos: new Vector2(700, 920)
    );

    court.InteriorObjects.Clear();
    // Backboard collision
    court.InteriorObjects.Add(new Rectangle(640, 80, 80, 20));

    buildings.Add(court);
}

static void AddMcDonalds(float x, float y)
{
    var mcd = new Building(
        new Rectangle(x, y, 200, 140),
        new Color(220, 30, 30, 255),
        new Color(255, 200, 0, 255),
        new Vector2(x + 100, y + 100),
        "McDONALD'S",
        new NPC(new Vector2(700, 120), "Cashier", "Welcome to McDonald's! Press E to order."),
        entryPos: new Vector2(700, 900)
    );

    mcd.InteriorObjects.Clear();

    // Counter
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 900, 50));
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 900, 8));
    mcd.InteriorObjects.Add(new Rectangle(200, 80, 8, 50));
    mcd.InteriorObjects.Add(new Rectangle(1092, 80, 8, 50));

    // Tables
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        mcd.InteriorObjects.Add(new Rectangle(tx, 400, 120, 70));
    }
    for (int t = 0; t < 3; t++)
    {
        int tx = 150 + t * 300;
        mcd.InteriorObjects.Add(new Rectangle(tx, 600, 120, 70));
    }

    // Play area wall
    mcd.InteriorObjects.Add(new Rectangle(1100, 300, 20, 600));

    buildings.Add(mcd);
}

static void AddBank(float x, float y)
{
    var bank = new Building(
        new Rectangle(x, y, 240, 160),
        new Color(180, 150, 80, 255),
        new Color(220, 210, 180, 255),
        new Vector2(x + 120, y + 140),
        "BANK",
        new NPC(new Vector2(700, 450), "Bank Manager", "Kia ora! Welcome to Waikato Bank."),
        entryPos: new Vector2(700, 900)
    );

    bank.InteriorObjects.Clear();

    // outer walls
    bank.InteriorObjects.Add(new Rectangle(0,    0,   20, 1000)); // left wall
    bank.InteriorObjects.Add(new Rectangle(1380, 0,   20, 1000)); // right wall
    bank.InteriorObjects.Add(new Rectangle(0,    0, 1400,   20)); // top wall

    // teller counter wall (with gap at 1100-1200 for staff)
    bank.InteriorObjects.Add(new Rectangle(0,   350, 1100,  20));
    bank.InteriorObjects.Add(new Rectangle(1200,350,  200,  20));

    // 3 teller booths
    bank.InteriorObjects.Add(new Rectangle(150, 260, 200, 90));
    bank.InteriorObjects.Add(new Rectangle(500, 260, 200, 90));
    bank.InteriorObjects.Add(new Rectangle(850, 260, 200, 90));


    // --- office (top left) ---
    bank.InteriorObjects.Add(new Rectangle(0,    20,  20, 330)); // already covered by left wall
    bank.InteriorObjects.Add(new Rectangle(20,   20, 330,  20)); // top wall
    bank.InteriorObjects.Add(new Rectangle(330,  20,  20, 330)); // right wall
    bank.InteriorObjects.Add(new Rectangle(20,  330, 130,  20)); // bottom left
    bank.InteriorObjects.Add(new Rectangle(250, 330,  80,  20)); // bottom right (gap at 150-250 for door)

    // office desk
    bank.InteriorObjects.Add(new Rectangle(50,  80, 240,  60));  // manager desk

    // --- vault door (back wall centre) ---
    bank.InteriorObjects.Add(new Rectangle(580, 20, 240, 40));   // vault door

    buildings.Add(bank);
}

static void AddBikeDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 320, 180),
        new Color(40,  80,  140, 255),
        new Color(30,  40,  60,  255),
        new Vector2(x + 160, y + 100),
        "BIKE DEALER",
        new NPC(new Vector2(700, 120), "Bike Dealer", "Want a two-wheeler? E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // front counter
    b.InteriorObjects.Add(new Rectangle(400,  80,   600,  50));
 
    // display island collisions (3 islands)
    int[] islandX = { 100, 520, 940 };
    foreach (int ix in islandX)
    {
        b.InteriorObjects.Add(new Rectangle(ix, 240, 260, 120)); // island platform
        b.InteriorObjects.Add(new Rectangle(ix, 400, 260, 100)); // second row island
    }
 
    // tool/parts rack along left wall
    b.InteriorObjects.Add(new Rectangle(20,  120, 40,  680));
 
    // parts counter along right wall
    b.InteriorObjects.Add(new Rectangle(1340, 120, 40, 680));
 
    b.EntryPosition = new Vector2(x + 160, y + 220);
    buildings.Add(b);
}

static void AddCarDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 420, 240),
        new Color(20,  20,  20,  255),
        new Color(18,  18,  22,  255),
        new Vector2(x + 210, y + 100),
        "CAR DEALER",
        new NPC(new Vector2(700, 120), "Car Dealer", "Find your next ride here. E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // service counter (right side, top)
    b.InteriorObjects.Add(new Rectangle(800,  50,   560,  70));
 
    // car display bays — 2 rows of 3 bays
    int[] bayX = { 60, 480, 900 };
    int[] bayY = { 130, 380 };
    foreach (int byx in bayX)
        foreach (int byy in bayY)
            b.InteriorObjects.Add(new Rectangle(byx, byy, 340, 180)); // car bay
 
    // divider pillars between bays
    int[] pillarX = { 440, 860 };
    foreach (int px in pillarX)
    {
        b.InteriorObjects.Add(new Rectangle(px, 40,  20, 780)); // full-height pillar
    }
 
    b.EntryPosition = new Vector2(x + 210, y + 280);
    buildings.Add(b);
}

static void AddBarnDealer(float x, float y)
{
    var b = new Building(
        new Rectangle(x, y, 360, 220),
        new Color(120, 60,  30,  255),
        new Color(100, 70,  45,  255),
        new Vector2(x + 180, y + 320),
        "BARN DEALER",
        new NPC(new Vector2(700, 120), "Barn Dealer", "We have the finest mounts. E to browse."),
        entryPos: new Vector2(700, 900)
    );
 
    b.InteriorObjects.Clear();
 
    // top wall
    b.InteriorObjects.Add(new Rectangle(0,    0,    1400, 40));
    // bottom wall
    b.InteriorObjects.Add(new Rectangle(0,    860,  1400, 140));
 
    // front reception / hay counter
    b.InteriorObjects.Add(new Rectangle(350,  60,   700,  55));
 
    // stalls — left side (3 stalls)
    int[] leftStallY = { 180, 370, 560 };
    foreach (int sy in leftStallY)
        b.InteriorObjects.Add(new Rectangle(20, sy, 280, 150));
 
    // stalls — right side (3 stalls)
    int[] rightStallY = { 180, 370, 560 };
    foreach (int sy in rightStallY)
        b.InteriorObjects.Add(new Rectangle(1100, sy, 280, 150));
 
    // central hay bale display row
    b.InteriorObjects.Add(new Rectangle(420,  300, 560,  80));
 
    b.EntryPosition = new Vector2(x + 180, y + 260);
    buildings.Add(b);
}

static void AddHallensteins(float x, float y)
{
    var h = new Building(
        new Rectangle(x, y, 160, 110),
        new Color(20, 20, 20, 255),          // near-black exterior
        new Color(15, 15, 15, 255),          // very dark interior
        new Vector2(x + 80, y + 100),
        "HALLENSTEINS",
        new NPC(new Vector2(400, 200), "Sales Assistant", "Kia ora bro, check out our latest fits."),
        entryPos: new Vector2(318, 885)
    );

    h.InteriorObjects.Clear();
    h.InteriorObjects.Add(new Rectangle(60,  120, 500, 30));   // front counter
    h.InteriorObjects.Add(new Rectangle(60,  200, 120, 200));  // rack left
    h.InteriorObjects.Add(new Rectangle(260, 200, 120, 200));  // rack middle
    h.InteriorObjects.Add(new Rectangle(460, 200, 120, 200));  // rack right
    h.InteriorObjects.Add(new Rectangle(700, 150, 8,  400));   // divider
    h.InteriorObjects.Add(new Rectangle(720, 180, 80, 100));   // fitting room 1
    h.InteriorObjects.Add(new Rectangle(820, 180, 80, 100));   // fitting room 2

    buildings.Add(h);
}

static void AddBench(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 70, 34), "Bench"));

static void AddLamppost(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 20, 111), "Lamppost"));

static void AddSwingSet(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 80, 84), "Swing Set"));

static void AddSandbox(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 80, 60), "Sandbox"));

static void AddPicnicTable(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 90, 47), "Picnic Table"));

static void AddPostbox(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 24, 71), "Postbox"));

static void AddBusStop(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 76, 100), "Bus Stop"));

static void AddFountain(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 80, 72), "Fountain"));

static void AddHayBale(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 70, 50), "Hay Bale"));

static void AddTractor(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 140, 108), "Tractor"));

static void AddWaterTrough(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 100, 52), "Water Trough"));

static void AddWindmill(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 120, 280), "Windmill"));

static void AddWheelbarrow(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 90, 78), "Wheelbarrow"));

static void AddCottagePeaked(float x, float y) =>
    decorativeBuildings.Add(new DecorativeBuilding(new Rectangle(x, y, 120, 143), "Cottage Peaked"));

static void AddTerracedHouse(float x, float y) =>
    decorativeBuildings.Add(new DecorativeBuilding(new Rectangle(x, y, 130, 213), "Terraced House"));

static void AddCottage(float x, float y)
{
    decorativeBuildings.Add(new DecorativeBuilding(
        new Rectangle(x, y, 120, 90), "Cottage"));
}

static void AddBarnHouse(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 200, 230), "Barn"));

static void AddShed(float x, float y) =>
    decorativeAssets.Add(new DecorativeAsset(new Rectangle(x, y, 140, 132), "Shed"));

static void AddTownhouse(float x, float y)
{
    decorativeBuildings.Add(new DecorativeBuilding(
        new Rectangle(x, y, 140, 190), "Townhouse"));
}

static void AddApartmentBlock(float x, float y)
{
    decorativeBuildings.Add(new DecorativeBuilding(
        new Rectangle(x, y, 200, 210), "Apartment Block"));
}

static void AddAA(float x, float y)
{
    var aa = new Building(
        new Rectangle(x, y, 160, 110),
        new Color(0, 80, 160, 255),
        new Color(10, 50, 100, 255),
        new Vector2(x + 80, y + 100),
        "AA",
        new NPC(new Vector2(700, 160), "AA Officer", "Kia ora! Theory tests, licence checks and practical bookings here bro."),
        entryPos: new Vector2(700, 900)
    );
    aa.InteriorObjects.Clear();
    aa.InteriorObjects.Add(new Rectangle(400, 100, 400, 40));   // front counter
    aa.InteriorObjects.Add(new Rectangle(100, 250, 200, 120));  // waiting seats
    aa.InteriorObjects.Add(new Rectangle(700, 250, 200, 120));  // theory computer desks
    aa.InteriorObjects.Add(new Rectangle(900, 250, 200, 120));
    buildings.Add(aa);
}

static void AddLibrary(float x, float y)
{
    var library = new Building(
        new Rectangle(x, y, 200, 150),
        new Color(150, 120, 80, 255),          // warm stone exterior
        new Color(40, 35, 55, 255),            // dim interior
        new Vector2(x + 60, y + 130),         // exit position (world coords)
        "LIBRARY",
        new NPC(new Vector2(600, 300), "Librarian", "Here to claim your ID? Step up to the photo booth."),
        entryPos: new Vector2(640, 850)
    );

    library.InteriorObjects.Clear();
    library.InteriorObjects.Add(new Rectangle(560, 300, 160, 200));  // photo booth
    library.InteriorObjects.Add(new Rectangle(100, 150, 40, 400));   // left bookshelf
    library.InteriorObjects.Add(new Rectangle(1140, 150, 40, 400));  // right bookshelf
    library.InteriorObjects.Add(new Rectangle(200, 150, 300, 40));   // front desk

    buildings.Add(library);
}

static void DrawLibraryExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color stone   = new Color((byte)170,(byte)140,(byte)95,(byte)255);
    Color stoneHi = new Color((byte)195,(byte)165,(byte)120,(byte)255);
    Color mortar  = new Color((byte)140,(byte)115,(byte)78,(byte)255);
    Color roofCol = new Color((byte)70,(byte)50,(byte)35,(byte)255);
    Color trim    = new Color((byte)235,(byte)225,(byte)205,(byte)255);
    Color glass    = new Color((byte)150,(byte)195,(byte)220,(byte)220);
    int w = 200, h = 150;

    // body + ashlar stone courses
    Raylib.DrawRectangle(bx, by, w, h, stone);
    for (int row = 8; row < h; row += 18)
        Raylib.DrawRectangle(bx, by + row, w, 2, mortar);
    for (int row = 0; row < h; row += 18)
        for (int cx = ((row / 18) % 2 == 0 ? 0 : 30); cx < w; cx += 60)
            Raylib.DrawRectangle(bx + cx, by + row, 2, 18, mortar);

    // pediment roof
    Raylib.DrawTriangle(new Vector2(bx - 10, by + 4), new Vector2(bx + w + 10, by + 4),
        new Vector2(bx + w / 2, by - 34), roofCol);
    Raylib.DrawTriangleLines(new Vector2(bx - 10, by + 4), new Vector2(bx + w + 10, by + 4),
        new Vector2(bx + w / 2, by - 34), trim);
    Raylib.DrawRectangle(bx - 10, by, w + 20, 8, trim);

    // portico columns
    for (int c = 0; c < 4; c++)
    {
        int cxp = bx + 22 + c * 52;
        Raylib.DrawRectangle(cxp, by + 40, 14, h - 40, stoneHi);
        Raylib.DrawRectangle(cxp - 2, by + 36, 18, 6, trim);       // capital
        Raylib.DrawRectangle(cxp - 3, by + h - 8, 20, 8, trim);    // base
    }

    // tall arched windows between columns
    for (int c = 0; c < 3; c++)
    {
        int wx = bx + 44 + c * 52;
        Raylib.DrawRectangle(wx, by + 56, 24, 60, glass);
        Raylib.DrawCircle(wx + 12, by + 56, 12, glass);
        Raylib.DrawRectangleLines(wx, by + 56, 24, 60, trim);
    }

    // door
    int dW = 40, dX = bx + w / 2 - dW / 2;
    Raylib.DrawRectangle(dX, by + h - 52, dW, 52, new Color((byte)80,(byte)55,(byte)30,(byte)255));
    Raylib.DrawRectangle(dX + dW / 2 - 1, by + h - 52, 2, 52, trim);

    // steps
    Raylib.DrawRectangle(dX - 12, by + h, dW + 24, 7, stoneHi);
    Raylib.DrawRectangle(dX - 20, by + h + 7, dW + 40, 7, stone);

    // sign
    Raylib.DrawRectangle(bx + w / 2 - 70, by - 30, 140, 18, roofCol);
    Program.DrawTextUI("LIBRARY", bx + w / 2 - 42, by - 28, 16, trim);
}

static void AddDBar(float x, float y)
{
    var dbar = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(15, 15, 15, 255),          // black exterior
        new Color(20, 20, 25, 255),           // very dark interior
        new Vector2(x + 100, y + 80),
        "DBar",
        new NPC(new Vector2(600, 420), "Dbar Owner", "Grab a woodys and relax at Dbar."),
        entryPos: new Vector2(318, 885)
    );
 
    dbar.InteriorObjects.Clear();
    dbar.InteriorObjects.Add(new Rectangle(100, 150, 300, 40));  // bar counter
    dbar.InteriorObjects.Add(new Rectangle(148, 278, 184, 104)); // pool table 1
    dbar.InteriorObjects.Add(new Rectangle(448, 278, 184, 104)); // pool table 2
    dbar.InteriorObjects.Add(new Rectangle(750, 150, 8, 400));   // divider
    dbar.InteriorObjects.Add(new Rectangle(780, 200, 60, 90));   // pokie 1
    dbar.InteriorObjects.Add(new Rectangle(780, 320, 60, 90));   // pokie 2
    dbar.InteriorObjects.Add(new Rectangle(880, 200, 60, 90));   // pokie 3
 
    buildings.Add(dbar);
}

static void AddFarmingShop(float x, float y)
{
    var farmShop = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(90, 60, 20, 255),        // brown exterior
        new Color(50, 35, 15, 255),        // dark interior
        new Vector2(x + 80, y + 100),
        "FARMING SHOP",
        new NPC(new Vector2(600, 120), "Farmhand", "G'day! Tools and seeds for the farm, right here."),
        entryPos: new Vector2(640, 880)
    );

    farmShop.InteriorObjects.Clear();
    farmShop.InteriorObjects.Add(new Rectangle(400, 80, 400, 40));   // counter
    farmShop.InteriorObjects.Add(new Rectangle(100, 200, 200, 120)); // tool display
    farmShop.InteriorObjects.Add(new Rectangle(350, 200, 200, 120)); // seed display 1
    farmShop.InteriorObjects.Add(new Rectangle(600, 200, 200, 120)); // seed display 2

    buildings.Add(farmShop);
}

static void DrawFarmingShopExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color plank   = new Color((byte)140,(byte)90,(byte)45,(byte)255);
    Color plankHi = new Color((byte)165,(byte)110,(byte)58,(byte)255);
    Color roofCol = new Color((byte)120,(byte)40,(byte)35,(byte)255);
    Color roofHi  = new Color((byte)150,(byte)55,(byte)48,(byte)255);
    Color trim    = new Color((byte)235,(byte)225,(byte)205,(byte)255);
    int w = 160, h = 120;

    // barn body with vertical planks
    Raylib.DrawRectangle(bx, by, w, h, plank);
    for (int px = 0; px < w; px += 16)
        Raylib.DrawRectangle(bx + px, by, 2, h, plankHi);

    // gambrel barn roof (two-slope)
    Raylib.DrawTriangle(new Vector2(bx - 8, by + 2), new Vector2(bx + w + 8, by + 2),
        new Vector2(bx + w / 2, by - 40), roofCol);
    Raylib.DrawTriangleLines(new Vector2(bx - 8, by + 2), new Vector2(bx + w + 8, by + 2),
        new Vector2(bx + w / 2, by - 40), roofHi);
    Raylib.DrawRectangle(bx - 8, by, w + 16, 6, roofHi);

    // hayloft window (diamond)
    Raylib.DrawCircle(bx + w / 2, by - 16, 8, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    Raylib.DrawCircle(bx + w / 2, by - 16, 5, new Color((byte)230,(byte)200,(byte)120,(byte)255));

    // big white cross-braced barn doors
    int dW = 60, dX = bx + w / 2 - dW / 2, dY = by + h - 70;
    Raylib.DrawRectangle(dX, dY, dW, 70, new Color((byte)120,(byte)70,(byte)35,(byte)255));
    Raylib.DrawRectangleLines(dX, dY, dW, 70, trim);
    Raylib.DrawRectangle(dX + dW / 2 - 1, dY, 2, 70, trim);
    Raylib.DrawLineEx(new Vector2(dX, dY), new Vector2(dX + dW / 2, dY + 70), 3, trim);
    Raylib.DrawLineEx(new Vector2(dX + dW, dY), new Vector2(dX + dW / 2, dY + 70), 3, trim);

    // side windows
    Raylib.DrawRectangle(bx + 14, by + 36, 28, 28, new Color((byte)150,(byte)195,(byte)220,(byte)220));
    Raylib.DrawRectangleLines(bx + 14, by + 36, 28, 28, trim);
    Raylib.DrawRectangle(bx + w - 42, by + 36, 28, 28, new Color((byte)150,(byte)195,(byte)220,(byte)220));
    Raylib.DrawRectangleLines(bx + w - 42, by + 36, 28, 28, trim);

    // little produce crates by the door
    Raylib.DrawRectangle(dX - 26, by + h - 22, 20, 20, plankHi);
    Raylib.DrawRectangle(dX + dW + 6, by + h - 22, 20, 20, plankHi);

    // sign
    Raylib.DrawRectangle(bx + w / 2 - 66, by - 34, 132, 18, new Color((byte)70,(byte)50,(byte)25,(byte)255));
    Program.DrawTextUI("FARMING SHOP", bx + w / 2 - 58, by - 32, 13, trim);
}

static void AddBarn(float x, float y)
{
    var barn = new Building(
        new Rectangle(x, y, 220, 180),
        new Color(150, 60, 45, 255),
        new Color(70, 45, 25, 255),
        new Vector2(x + 110, y + 160),
        "BARN",
        new NPC(new Vector2(700, 160), "Rancher", "After livestock? Step up and I'll sort you out."),
        entryPos: new Vector2(700, 900)
    );
    barn.InteriorObjects.Clear();
    barn.InteriorObjects.Add(new Rectangle(350, 80, 700, 60));    // counter
    // stalls left
    foreach (int sy in new[] { 200, 400, 600 })
        barn.InteriorObjects.Add(new Rectangle(40, sy, 260, 150));
    // stalls right
    foreach (int sy in new[] { 200, 400, 600 })
        barn.InteriorObjects.Add(new Rectangle(1100, sy, 260, 150));
    barn.InteriorObjects.Add(new Rectangle(560, 780, 280, 80));   // hay bale display
    buildings.Add(barn);
}

static void DrawBarnExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color plank   = new Color((byte)150,(byte)60,(byte)45,(byte)255);
    Color plankHi = new Color((byte)175,(byte)75,(byte)55,(byte)255);
    Color roofCol = new Color((byte)90,(byte)45,(byte)35,(byte)255);
    Color trim    = new Color((byte)240,(byte)235,(byte)225,(byte)255);
    int w = 220, h = 180;

    // barn body, vertical planks
    Raylib.DrawRectangle(bx, by, w, h, plank);
    for (int px = 0; px < w; px += 18)
        Raylib.DrawRectangle(bx + px, by, 2, h, plankHi);

    // gambrel roof (two slopes)
    Raylib.DrawTriangle(new Vector2(bx - 10, by + 40), new Vector2(bx + w/2, by - 44), new Vector2(bx + w + 10, by + 40), roofCol);
    Raylib.DrawRectangle(bx - 10, by + 36, w + 20, 8, new Color((byte)70,(byte)35,(byte)28,(byte)255));

    // hayloft door + pulley
    Raylib.DrawRectangle(bx + w/2 - 18, by - 12, 36, 34, new Color((byte)70,(byte)40,(byte)22,(byte)255));
    Raylib.DrawRectangle(bx + w/2 - 18, by - 12, 36, 4, trim);
    Raylib.DrawCircle(bx + w/2, by - 20, 4, new Color((byte)40,(byte)40,(byte)44,(byte)255));

    // big white cross-braced doors
    int dW = 80, dX = bx + w/2 - dW/2, dY = by + h - 90;
    Raylib.DrawRectangle(dX, dY, dW, 90, new Color((byte)120,(byte)55,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(dX, dY, dW, 90, trim);
    Raylib.DrawRectangle(dX + dW/2 - 1, dY, 2, 90, trim);
    Raylib.DrawLineEx(new Vector2(dX, dY), new Vector2(dX + dW/2, dY + 90), 3, trim);
    Raylib.DrawLineEx(new Vector2(dX + dW, dY), new Vector2(dX + dW/2, dY + 90), 3, trim);
    Raylib.DrawLineEx(new Vector2(dX, dY + 90), new Vector2(dX + dW/2, dY), 3, trim);
    Raylib.DrawLineEx(new Vector2(dX + dW, dY + 90), new Vector2(dX + dW/2, dY), 3, trim);

    // side windows
    Raylib.DrawRectangle(bx + 20, by + 60, 30, 30, new Color((byte)150,(byte)195,(byte)220,(byte)220));
    Raylib.DrawRectangleLines(bx + 20, by + 60, 30, 30, trim);
    Raylib.DrawRectangle(bx + w - 50, by + 60, 30, 30, new Color((byte)150,(byte)195,(byte)220,(byte)220));
    Raylib.DrawRectangleLines(bx + w - 50, by + 60, 30, 30, trim);

    // little fenced paddock beside the barn
    for (int fxp = bx + w + 6; fxp < bx + w + 90; fxp += 20)
        Raylib.DrawRectangle(fxp, by + h - 60, 3, 60, new Color((byte)120,(byte)90,(byte)55,(byte)255));
    Raylib.DrawRectangle(bx + w + 6, by + h - 60, 84, 3, new Color((byte)120,(byte)90,(byte)55,(byte)255));

    // sign
    Raylib.DrawRectangle(bx + w/2 - 50, by - 40, 100, 20, roofCol);
    Program.DrawTextUI("BARN", bx + w/2 - 26, by - 38, 16, trim);
}

static void AddZoo(float x, float y)
{
    var zoo = new Building(
        new Rectangle(x, y, 360, 260),
        new Color(70, 140, 80, 255),
        new Color(40, 80, 50, 255),
        new Vector2(x + 180, y + 240),
        "ZOO",
        new NPC(new Vector2(700, 160), "Zookeeper", "Welcome to the zoo! $20 entry — mind the enclosures."),
        entryPos: new Vector2(1000, 1850)
    );
    zoo.InteriorObjects.Clear();
    // enclosure walls (rects the player can't walk through) — a ring of exhibits
    (int x, int y, int w, int h)[] pens = {
        (150,200,420,320), (900,200,420,320), (150,700,420,320),
        (900,700,420,320), (525,1150,420,300),
    };
    foreach (var (ex, ey, ew, eh) in pens)
    {
        zoo.InteriorObjects.Add(new Rectangle(ex, ey, ew, 20));
        zoo.InteriorObjects.Add(new Rectangle(ex, ey + eh - 20, ew, 20));
        zoo.InteriorObjects.Add(new Rectangle(ex, ey, 20, eh));
        zoo.InteriorObjects.Add(new Rectangle(ex + ew - 20, ey, 20, eh));
    }
    zoo.InteriorObjects.Add(new Rectangle(880, 60, 240, 60)); // ticket booth
    buildings.Add(zoo);
}

static void DrawZooExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color wall   = new Color((byte)90,(byte)150,(byte)95,(byte)255);
    Color wallHi = new Color((byte)115,(byte)175,(byte)118,(byte)255);
    Color stone  = new Color((byte)150,(byte)150,(byte)140,(byte)255);
    Color trim   = new Color((byte)240,(byte)240,(byte)225,(byte)255);
    int w = 360, h = 260;

    // safari-green perimeter wall
    Raylib.DrawRectangle(bx, by, w, h, wall);
    Raylib.DrawRectangle(bx, by, w, 40, wallHi);
    for (int px = 0; px < w; px += 40)
        Raylib.DrawRectangle(bx + px, by, 3, h, new Color((byte)70,(byte)120,(byte)78,(byte)255));

    // grand stone archway entrance
    int aW = 120, aX = bx + w/2 - aW/2;
    Raylib.DrawRectangle(aX, by + h - 90, aW, 90, stone);
    Raylib.DrawCircle(bx + w/2, by + h - 90, aW/2, stone);
    Raylib.DrawRectangle(aX + 16, by + h - 74, aW - 32, 74, new Color((byte)50,(byte)40,(byte)30,(byte)255)); // gate opening
    Raylib.DrawCircle(bx + w/2, by + h - 90, aW/2 - 16, new Color((byte)50,(byte)40,(byte)30,(byte)255));

    // "ZOO" banner across the arch
    Raylib.DrawRectangle(aX - 6, by + h - 120, aW + 12, 26, new Color((byte)210,(byte)120,(byte)40,(byte)255));
    Program.DrawTextUI("ZOO", bx + w/2 - 24, by + h - 118, 22, trim);

    // palm-tree silhouettes on the wall corners
    foreach (int txp in new[] { bx + 30, bx + w - 40 })
    {
        Raylib.DrawRectangle(txp, by + 60, 8, 80, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(txp + 4, by + 56, 22, new Color((byte)60,(byte)150,(byte)70,(byte)255));
    }

    // animal-print signage flags along the top
    for (int fxp = bx + 20; fxp < bx + w - 20; fxp += 60)
        Raylib.DrawTriangle(new Vector2(fxp, by - 12), new Vector2(fxp + 24, by - 12), new Vector2(fxp + 12, by + 6),
            (fxp / 60) % 2 == 0 ? new Color((byte)230,(byte)180,(byte)60,(byte)255) : new Color((byte)220,(byte)110,(byte)50,(byte)255));
}

static void AddCastle(float x, float y)
{
    var castle = new Building(
        new Rectangle(x, y, 420, 320),
        new Color(120, 120, 130, 255),
        new Color(45, 42, 55, 255),
        new Vector2(x + 210, y + 300),
        "CASTLE",
        new NPC(new Vector2(700, 260), "Steward", "Welcome to the keep, traveller."),
        entryPos: new Vector2(986, 1866)
    );
    castle.InteriorObjects.Clear();

    // throne dais (top-centre)
    castle.InteriorObjects.Add(new Rectangle(850, 120, 300, 160));
    // long banquet tables
    castle.InteriorObjects.Add(new Rectangle(300, 700, 160, 500));
    castle.InteriorObjects.Add(new Rectangle(1540, 700, 160, 500));
    // central pillars
    for (int p = 0; p < 4; p++)
    {
        castle.InteriorObjects.Add(new Rectangle(560, 500 + p * 320, 70, 70));
        castle.InteriorObjects.Add(new Rectangle(1370, 500 + p * 320, 70, 70));
    }
    // braziers flanking throne
    castle.InteriorObjects.Add(new Rectangle(760, 180, 50, 50));
    castle.InteriorObjects.Add(new Rectangle(1190, 180, 50, 50));

    buildings.Add(castle);
}

static void DrawCastleExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color stone   = new Color((byte)140,(byte)140,(byte)150,(byte)255);
    Color stoneHi = new Color((byte)165,(byte)165,(byte)175,(byte)255);
    Color mortar  = new Color((byte)110,(byte)110,(byte)120,(byte)255);
    Color roofCol = new Color((byte)90,(byte)45,(byte)55,(byte)255);
    Color flag    = new Color((byte)180,(byte)40,(byte)45,(byte)255);
    int w = 420, h = 320;

    // curtain wall
    Raylib.DrawRectangle(bx, by + 40, w, h - 40, stone);
    for (int row = 40; row < h; row += 22)
        Raylib.DrawRectangle(bx, by + row, w, 2, mortar);
    for (int row = 40; row < h; row += 22)
        for (int cx = ((row / 22) % 2 == 0 ? 0 : 34); cx < w; cx += 68)
            Raylib.DrawRectangle(bx + cx, by + row, 2, 22, mortar);

    // corner + centre towers
    int[] towerX = { bx - 10, bx + w / 2 - 35, bx + w - 60 };
    foreach (int tx in towerX)
    {
        Raylib.DrawRectangle(tx, by, 70, h, stoneHi);
        for (int row = 0; row < h; row += 22)
            Raylib.DrawRectangle(tx, by + row, 70, 2, mortar);
        // crenellations
        for (int cxp = tx; cxp < tx + 70; cxp += 24)
            Raylib.DrawRectangle(cxp, by - 14, 14, 16, stone);
        // conical roof on centre tower
        if (tx == bx + w / 2 - 35)
        {
            Raylib.DrawTriangle(new Vector2(tx - 4, by - 12), new Vector2(tx + 74, by - 12),
                new Vector2(tx + 35, by - 70), roofCol);
            // flag
            Raylib.DrawRectangle(tx + 34, by - 100, 3, 34, new Color((byte)60,(byte)50,(byte)40,(byte)255));
            Raylib.DrawTriangle(new Vector2(tx + 37, by - 100), new Vector2(tx + 37, by - 84),
                new Vector2(tx + 62, by - 92), flag);
        }
        // arrow-slit windows
        Raylib.DrawRectangle(tx + 30, by + 60, 8, 30, new Color((byte)30,(byte)30,(byte)40,(byte)255));
        Raylib.DrawRectangle(tx + 30, by + 140, 8, 30, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    }

    // curtain-wall crenellations
    for (int cxp = bx + 60; cxp < bx + w - 60; cxp += 30)
        Raylib.DrawRectangle(cxp, by + 26, 18, 16, stone);

    // portcullis gate
    int gW = 90, gX = bx + w / 2 - gW / 2, gY = by + h - 110;
    Raylib.DrawRectangle(gX, gY, gW, 110, new Color((byte)55,(byte)40,(byte)25,(byte)255));
    Raylib.DrawCircle(gX + gW / 2, gY, gW / 2, new Color((byte)55,(byte)40,(byte)25,(byte)255)); // arch
    for (int g = gX + 10; g < gX + gW; g += 16)
        Raylib.DrawRectangle(g, gY, 3, 108, new Color((byte)90,(byte)90,(byte)100,(byte)255));
    for (int g = gY + 12; g < gY + 108; g += 18)
        Raylib.DrawRectangle(gX, g, gW, 3, new Color((byte)90,(byte)90,(byte)100,(byte)255));

    // sign
    Raylib.DrawRectangle(bx + w / 2 - 60, by + h + 4, 120, 18, roofCol);
    Program.DrawTextUI("CASTLE", bx + w / 2 - 36, by + h + 6, 16, new Color((byte)235,(byte)225,(byte)210,(byte)255));
}

static void AddMall(float x, float y)
{
    var mall = new Building(
        new Rectangle(x, y, 480, 340),
        new Color(180, 185, 195, 255),
        new Color(235, 235, 240, 255),
        new Vector2(x + 240, y + 320),
        "MALL",
        new NPC(new Vector2(1000, 200), "Concierge", "Welcome! Six shops to browse — take your pick."),
        entryPos: new Vector2(1000, 1850)
    );
    mall.InteriorObjects.Clear();

    // storefront walls with door gaps (mirrors mallShops door positions)
    void ShopFront(int rx, int ry, int rw, int rh, int doorOffset, int doorW)
    {
        mall.InteriorObjects.Add(new Rectangle(rx, ry, rw, 14));                          // top
        mall.InteriorObjects.Add(new Rectangle(rx, ry, 14, rh));                          // left
        mall.InteriorObjects.Add(new Rectangle(rx + rw - 14, ry, 14, rh));               // right
        // bottom split around the door
        mall.InteriorObjects.Add(new Rectangle(rx, ry + rh - 14, doorOffset, 14));
        mall.InteriorObjects.Add(new Rectangle(rx + doorOffset + doorW, ry + rh - 14, rw - doorOffset - doorW, 14));
    }

    // top row of shops
    ShopFront(200, 300, 340, 340, 130, 80);   // CLOTHING
    ShopFront(600, 300, 340, 340, 130, 80);   // ELECTRONICS
    ShopFront(1000, 300, 340, 340, 130, 80);  // SPORTS
    // bottom row of shops
    ShopFront(200, 1020, 340, 340, 130, 80);  // BOOKS
    ShopFront(600, 1020, 340, 340, 130, 80);  // FOOD COURT
    ShopFront(1000, 1020, 340, 340, 130, 80); // TOYS

    // central fountain + benches (concourse furniture)
    mall.InteriorObjects.Add(new Rectangle(920, 820, 160, 160)); // fountain
    mall.InteriorObjects.Add(new Rectangle(500, 880, 120, 40));  // bench
    mall.InteriorObjects.Add(new Rectangle(1380, 880, 120, 40)); // bench

    buildings.Add(mall);
}

static void DrawMallExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color body   = new Color((byte)190,(byte)195,(byte)205,(byte)255);
    Color bodyHi = new Color((byte)215,(byte)220,(byte)230,(byte)255);
    Color glass  = new Color((byte)150,(byte)205,(byte)230,(byte)220);
    Color trim   = new Color((byte)90,(byte)95,(byte)110,(byte)255);
    Color accent = new Color((byte)210,(byte)90,(byte)140,(byte)255);
    int w = 480, h = 340;

    // main block
    Raylib.DrawRectangle(bx, by, w, h, body);
    Raylib.DrawRectangle(bx, by, w, 40, bodyHi);

    // flat roof parapet + accent stripe
    Raylib.DrawRectangle(bx - 8, by - 14, w + 16, 16, trim);
    Raylib.DrawRectangle(bx - 8, by - 4, w + 16, 4, accent);

    // curved glass entrance atrium
    Raylib.DrawRectangle(bx + w / 2 - 90, by + 40, 180, h - 40, glass);
    Raylib.DrawRectangleLines(bx + w / 2 - 90, by + 40, 180, h - 40, trim);
    for (int g = bx + w / 2 - 90; g <= bx + w / 2 + 90; g += 30)
        Raylib.DrawRectangle(g, by + 40, 2, h - 40, trim);
    Raylib.DrawRectangle(bx + w / 2 - 40, by + h - 60, 80, 60, new Color((byte)70,(byte)80,(byte)95,(byte)255)); // doors
    Raylib.DrawRectangle(bx + w / 2 - 1, by + h - 60, 2, 60, trim);

    // window bands each side
    for (int side = 0; side < 2; side++)
    {
        int sx = side == 0 ? bx + 20 : bx + w / 2 + 110;
        for (int wx = sx; wx < sx + 130; wx += 44)
        {
            Raylib.DrawRectangle(wx, by + 70, 34, 60, glass);
            Raylib.DrawRectangleLines(wx, by + 70, 34, 60, trim);
            Raylib.DrawRectangle(wx, by + 170, 34, 60, glass);
            Raylib.DrawRectangleLines(wx, by + 170, 34, 60, trim);
        }
    }

    // pylon sign
    Raylib.DrawRectangle(bx + 20, by - 70, 8, 60, trim);
    Raylib.DrawRectangle(bx + 2, by - 96, 90, 30, accent);
    Program.DrawTextUI("MALL", bx + 18, by - 90, 18, Color.White);

    // main fascia sign
    Raylib.DrawRectangle(bx + w / 2 - 70, by - 40, 140, 22, trim);
    Program.DrawTextUI("MALL", bx + w / 2 - 26, by - 37, 18, Color.White);
}

static void AddFamilyHub(float x, float y)
{
    var FamilyHub = new Building(
        new Rectangle(x, y, 500, 380),
        new Color(120, 90, 150, 255),
        new Color(60, 45, 80, 255),
        new Vector2(x + 250, y + 340),
        "FamilyHub",
        new NPC(new Vector2(700, 200), "Matron", "Welcome. You can adopt a little one at reception."),
        entryPos: new Vector2(700, 1750)
    );
    FamilyHub.InteriorObjects.Clear();

    // reception desk (top, near Matron)
    FamilyHub.InteriorObjects.Add(new Rectangle(560, 150, 300, 60));

    // bedroom rows: beds down each side of a central 1400-wide room, walking aisle in the middle
    // left column of beds (x=120) and right column (x=1180), 5 rows
    for (int row = 0; row < 5; row++)
    {
        int by = 350 + row * 240;
        FamilyHub.InteriorObjects.Add(new Rectangle(120, by, 120, 70));   // left bed
        FamilyHub.InteriorObjects.Add(new Rectangle(1160, by, 120, 70));  // right bed
    }
    FamilyHubNPCs.Clear();
    FamilyHubNPCs.Add(new NPC(new Vector2(700, 300), "Matron", "Welcome to the sanctuary."));
    FamilyHubNPCs.Add(new NPC(new Vector2(300, 600), "Nurse Ivy", "The little ones are thriving."));
    for (int k = 0; k < 6; k++)
        FamilyHubNPCs.Add(new NPC(new Vector2(250 + (k % 3) * 450, 700 + (k / 3) * 500),
                                  "Fairy" + (k + 1), "Wanna play?"));
    buildings.Add(FamilyHub);
}

static void DrawFamilyHubExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color wall  = new Color((byte)140,(byte)110,(byte)165,(byte)255);
    Color trim  = new Color((byte)235,(byte)225,(byte)240,(byte)255);
    Color roof  = new Color((byte)80,(byte)60,(byte)95,(byte)255);
    int w = 500, h = 380;

    Raylib.DrawRectangle(bx, by, w, h, wall);
    Raylib.DrawRectangle(bx - 8, by - 18, w + 16, 20, roof);
    Raylib.DrawRectangle(bx, by, w, 10, trim);

    for (int wx = bx + 24; wx < bx + w - 24; wx += 58)
    {
        Raylib.DrawRectangle(wx, by + 46, 34, 40, new Color((byte)200,(byte)220,(byte)235,(byte)210));
        Raylib.DrawRectangleLines(wx, by + 46, 34, 40, trim);
    }

    int doorW = 90, doorX = bx + w/2 - doorW/2;
    Raylib.DrawRectangle(doorX, by + h - 60, doorW, 60, new Color((byte)90,(byte)60,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(doorX, by + h - 60, doorW, 60, trim);

    Raylib.DrawRectangle(bx + w/2 - 95, by - 42, 190, 20, roof);
    Program.DrawTextUI("FamilyHub", bx + w/2 - 58, by - 39, 18, trim);
}

static void AddDaycare(float x, float y)
{
    var Cride = new NPC(new Vector2(700, 200), "Cride", "Welcome to best start!");
    Cride.SpriteKey = "Cride";        
    Cride.Facing = NPC.Dir.South;

    var daycare = new Building(
        new Rectangle(x, y, 460, 340),
        new Color(255, 200, 90, 255),
        new Color(255, 235, 190, 255),
        new Vector2(x + 230, y + 300),
        "BEST START",
        Cride,
        entryPos: new Vector2(700, 900)
    );
    daycare.InteriorObjects.Clear();

    daycare.InteriorObjects.Add(new Rectangle(540, 150, 320, 60));   // sign-in desk
    // play mats / low tables (non-blocking feel kept small)
    for (int i = 0; i < 3; i++)
        daycare.InteriorObjects.Add(new Rectangle(300 + i * 300, 500, 160, 120));

    buildings.Add(daycare);
}

static void DrawDaycareExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    float hr = Program.GetCurrentHour();
    bool open = hr >= 8f && hr < 17f;
    Color wall = new Color((byte)255,(byte)205,(byte)100,(byte)255);
    Color trim = new Color((byte)255,(byte)250,(byte)235,(byte)255);
    Color roof = new Color((byte)230,(byte)140,(byte)70,(byte)255);
    int w = 460, h = 340;

    Raylib.DrawRectangle(bx, by, w, h, wall);
    // cheerful scalloped roof
    Raylib.DrawRectangle(bx - 8, by - 16, w + 16, 18, roof);
    for (int sx = bx; sx < bx + w; sx += 40)
        Raylib.DrawCircle(sx + 20, by - 16, 12, roof);

    for (int wx = bx + 24; wx < bx + w - 24; wx += 60)
    {
        Raylib.DrawRectangle(wx, by + 60, 36, 40, new Color((byte)200,(byte)230,(byte)245,(byte)220));
        Raylib.DrawRectangleLines(wx, by + 60, 36, 40, trim);
    }

    int doorW = 84, doorX = bx + w/2 - doorW/2;
    Color doorCol = open ? new Color((byte)200,(byte)120,(byte)60,(byte)255)   // ── CHANGED ──
                         : new Color((byte)90,(byte)55,(byte)30,(byte)255);    // shut = darker
    Raylib.DrawRectangle(doorX, by + h - 56, doorW, 56, doorCol);
    Raylib.DrawRectangleLines(doorX, by + h - 56, doorW, 56, trim);

    Raylib.DrawRectangle(bx + w/2 - 100, by - 40, 200, 22, roof);
    Program.DrawTextUI("BEST START", bx + w/2 - 62, by - 37, 18, trim);

    string status = open ? $"{EnrolledToday()} enrolled today" : "Closed - open 8am";
    Color statusCol = open ? trim : new Color((byte)255,(byte)180,(byte)180,(byte)255);
    int sw = Program.MeasureTextUI(status, 14);
    Program.DrawTextUI(status, bx + w/2 - sw/2, by - 16, 14, statusCol);
}

static void AddKiwiCuts(float x, float y)
{
    var barber = new NPC(new Vector2(400, 300), "Barber", "Sweet as, take a seat bro.");
    barber.SpriteKey = "Barber";       
    barber.Facing = NPC.Dir.South;   

    var kiwicuts = new Building(
        new Rectangle(x, y, 140, 110),
        new Color(240, 240, 235, 255),
        new Color(245, 240, 230, 255),
        new Vector2(x + 80, y + 80),          // ExitPosition (world coords, where player lands outside)
        "KiwiCuts",
        new NPC(new Vector2(400, 300), "Barber", "Sweet as, take a seat bro."),
        entryPos: new Vector2(640, 640)        // EntryPosition — bottom-centre, clear floor near exit door
    );

    kiwicuts.InteriorObjects.Clear();

    // Reception counter (top-left) — matches screen rect (60,60,280,40)
    kiwicuts.InteriorObjects.Add(new Rectangle(60, 60, 280, 40));

    // Barber chair 1 (seat + armrests) — matches screen draw (70,160,80,65)
    kiwicuts.InteriorObjects.Add(new Rectangle(64, 160, 88, 65));

    // Barber chair 2 (seat + armrests) — matches screen draw (210,160,80,65)
    kiwicuts.InteriorObjects.Add(new Rectangle(204, 160, 88, 65));

    // Waiting bench (right side) — matches screen rect (700,120,200,50)
    kiwicuts.InteriorObjects.Add(new Rectangle(700, 120, 200, 50));

    // Magazine table — matches screen rect (700,200,200,10)
    kiwicuts.InteriorObjects.Add(new Rectangle(700, 200, 200, 14));

    buildings.Add(kiwicuts);
}

static void AddSupermarket(float x, float y)
{
    var supermarket = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(200, 220, 200, 255),
        new Color(210, 225, 210, 255),
        new Vector2(x + 100, y + 100),
        "SUPERMARKET",
        new NPC(new Vector2(1280, 60), "Cashier", "Welcome! Grab a trolley or basket at the entrance. Use spacebar to pickup and return"),
        entryPos: new Vector2(500, 920)
    );

    supermarket.InteriorObjects.Clear();

    // --- entrance barrier ---
    supermarket.InteriorObjects.Add(new Rectangle(0,   870, 380, 20));
    supermarket.InteriorObjects.Add(new Rectangle(650, 870, 750, 20));   // right barrier

    // --- checkout counters ---
    supermarket.InteriorObjects.Add(new Rectangle(50,  30, 140, 45));    // checkout 1
    supermarket.InteriorObjects.Add(new Rectangle(250, 30, 140, 45));    // checkout 2
    supermarket.InteriorObjects.Add(new Rectangle(450, 30, 140, 45));    // checkout 3
    supermarket.InteriorObjects.Add(new Rectangle(650, 30, 140, 45));    // checkout 4
    supermarket.InteriorObjects.Add(new Rectangle(850, 30, 140, 45));    // checkout 5
    supermarket.InteriorObjects.Add(new Rectangle(1050,30, 140, 45));    // checkout 6

    // --- aisle shelves (pairs of shelves with walking gap) ---
    // aisle column 1
    supermarket.InteriorObjects.Add(new Rectangle(50,  150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(50,  625, 220, 35));

    // aisle column 2
    supermarket.InteriorObjects.Add(new Rectangle(340, 150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(340, 625, 220, 35));

    // aisle column 3
    supermarket.InteriorObjects.Add(new Rectangle(630, 150, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 205, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 290, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 345, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 430, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 485, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 570, 220, 35));
    supermarket.InteriorObjects.Add(new Rectangle(630, 625, 220, 35));

    // --- meat fridges (right wall) ---
    supermarket.InteriorObjects.Add(new Rectangle(1310, 150, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 280, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 410, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 540, 55, 110));
    supermarket.InteriorObjects.Add(new Rectangle(1310, 670, 55, 110));

    // --- fruit & veg bins (back wall) ---
    supermarket.InteriorObjects.Add(new Rectangle(50,  750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(160, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(270, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(380, 750, 90, 90));
    supermarket.InteriorObjects.Add(new Rectangle(490, 750, 90, 90));

    // --- deli station (back right) ---
    supermarket.InteriorObjects.Add(new Rectangle(650, 750, 300, 45));   // deli counter
    supermarket.InteriorObjects.Add(new Rectangle(650, 808, 120, 70));   // display case
    supermarket.InteriorObjects.Add(new Rectangle(790, 808, 70, 70));    // slicer/equipment
    supermarket.InteriorObjects.Add(new Rectangle(878, 808, 70, 70));    // warmer

    buildings.Add(supermarket);
}

static void AddHospital(float x, float y)
{
    var nurse = new NPC(new Vector2(1235, 80), "Nurse", "How may I help today?");
    nurse.SpriteKey = "nurse";        
    nurse.Facing = NPC.Dir.South; 

    var hospital = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(220, 50, 50, 255),
        new Color(200, 220, 220, 255),
        new Vector2(x + 80, y + 100),
        "HOSPITAL",
        nurse,
        entryPos: new Vector2(1225, 897)
    );

    hospital.InteriorObjects.Clear();

    // front counter (single collision box)
    hospital.InteriorObjects.Add(new Rectangle(1095, 80, 280, 50));

    // computer on counter (single collision box)
    hospital.InteriorObjects.Add(new Rectangle(1215, 60, 40, 28));

    // medical cross sign (two bars)
    hospital.InteriorObjects.Add(new Rectangle(1228, 20, 14, 42));
    hospital.InteriorObjects.Add(new Rectangle(1214, 34, 42, 14));

    // hallway divider wall - moved down to give more room

    hospital.InteriorObjects.Add(new Rectangle(120, 400, 370, 20));
    hospital.InteriorObjects.Add(new Rectangle(120, 600, 370, 20));
    hospital.InteriorObjects.Add(new Rectangle(610, 400, 200, 20));
    hospital.InteriorObjects.Add(new Rectangle(610, 600, 200, 20));
    hospital.InteriorObjects.Add(new Rectangle(950, 400, 120, 20));
    hospital.InteriorObjects.Add(new Rectangle(950, 600, 100, 20));

    // room dividers top row
    hospital.InteriorObjects.Add(new Rectangle(350, 0, 20, 400));
    hospital.InteriorObjects.Add(new Rectangle(700, 0, 20, 400));
    hospital.InteriorObjects.Add(new Rectangle(1050, 0, 20, 400));

    // room dividers bottom row  
    hospital.InteriorObjects.Add(new Rectangle(350, 600, 20, 420));
    hospital.InteriorObjects.Add(new Rectangle(700, 600, 20, 420));
    hospital.InteriorObjects.Add(new Rectangle(1050, 600, 20, 420));

    // beds top row
    hospital.InteriorObjects.Add(new Rectangle(30,  30, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(380, 30, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(730, 30, 300, 80));

    // beds bottom row - moved up from wall
    hospital.InteriorObjects.Add(new Rectangle(30,  820, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(380, 820, 300, 80));
    hospital.InteriorObjects.Add(new Rectangle(730, 820, 300, 80));

    // side tables top row
    hospital.InteriorObjects.Add(new Rectangle(335, 30, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(685, 30, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(1035, 30, 15, 30));

    // side tables bottom row
    hospital.InteriorObjects.Add(new Rectangle(335, 820, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(685, 820, 15, 30));
    hospital.InteriorObjects.Add(new Rectangle(1035, 820, 15, 30));

    // chairs top row
    hospital.InteriorObjects.Add(new Rectangle(30,  165, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(380, 165, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(730, 165, 35, 35));

    // chairs bottom row
    hospital.InteriorObjects.Add(new Rectangle(30,  760, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(380, 760, 35, 35));
    hospital.InteriorObjects.Add(new Rectangle(730, 760, 35, 35));

    buildings.Add(hospital);
}

static void AddStore(float x, float y)
{
    var store = new Building(
        new Rectangle(x, y, 160, 120),
        Color.DarkGreen,
        new Color(40, 90, 50, 255),
        new Vector2(x + 60, y + 100),
        "STORE",
        new NPC(new Vector2(700, 120), "Store Clerk", "Need supplies? Show me the moolack."),
        entryPos: new Vector2(700, 850)
    );

    store.InteriorObjects.Clear();

    // --- front counter ---
    store.InteriorObjects.Add(new Rectangle(400, 80, 500, 50));
    store.InteriorObjects.Add(new Rectangle(400, 80, 500, 10));

    // --- aisle shelves (3 aisles, 8 shelf rows each, evenly spaced) ---
    int[] aisleStartX     = { 150, 550, 950 };
    int[] shelfYPositions = { 230, 290, 380, 440, 530, 590, 670, 730 };

    foreach (int ax in aisleStartX)
    {
        foreach (int sy in shelfYPositions)
            store.InteriorObjects.Add(new Rectangle(ax, sy, 280, 35));

        // end cap
        store.InteriorObjects.Add(new Rectangle(ax + 280, 220, 20, 510));
    }

    // --- back wall shelves (two banks of 3, split around entrance mat) ---
    int[] backShelfY = { 860, 907, 954 };

    foreach (int bsy in backShelfY)
    {
        store.InteriorObjects.Add(new Rectangle(50,  bsy, 500, 30));  // left bank
        store.InteriorObjects.Add(new Rectangle(850, bsy, 500, 30));  // right bank
    }

    buildings.Add(store);
}

static void AddMagicShop(float x, float y)
{
    var Wizard = new NPC(new Vector2(1000, 500), "Wizard", "Welcome traveller. Arcane power awaits those who seek it.");
    Wizard.SpriteKey = "Wizard";        
    Wizard.Facing = NPC.Dir.South;      

    var magic = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(60, 20, 100, 255),
        new Color(30, 10, 60, 255),
        new Vector2(x + 80, y + 100),
        "MAGIC SHOP",
        Wizard,                          
        entryPos: new Vector2(640, 880)
    );

    magic.InteriorObjects.Clear();
    magic.InteriorObjects.Add(new Rectangle(400, 80, 400, 40));   // counter
    magic.InteriorObjects.Add(new Rectangle(100, 200, 200, 120)); // staff display left
    magic.InteriorObjects.Add(new Rectangle(350, 200, 200, 120)); // staff display mid
    magic.InteriorObjects.Add(new Rectangle(600, 200, 200, 120)); // staff display right
    magic.InteriorObjects.Add(new Rectangle(850, 200, 200, 120)); // staff display far right
    magic.InteriorObjects.Add(new Rectangle(100, 500, 160, 80));  // essence shelf 1
    magic.InteriorObjects.Add(new Rectangle(280, 500, 160, 80));  // essence shelf 2

    buildings.Add(magic);
}

static void AddRangingShop(float x, float y)
{
    var ranger = new NPC(new Vector2(400, 350), "Ranger", "Welcome traveller. Arcane power awaits those who seek it.");
        ranger.SpriteKey = "ranger";        
        ranger.Facing = NPC.Dir.South; 

    var ranging = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(60, 35, 10, 255),
        new Color(35, 20, 8, 255),
        new Vector2(x + 80, y + 100),
        "RANGING SHOP",
        ranger,
        entryPos: new Vector2(640, 880)
    );

    ranging.InteriorObjects.Clear();
    ranging.InteriorObjects.Add(new Rectangle(400, 80, 400, 40));    // counter
    ranging.InteriorObjects.Add(new Rectangle(80,  200, 180, 120));  // bow rack left
    ranging.InteriorObjects.Add(new Rectangle(300, 200, 180, 120));  // bow rack mid
    ranging.InteriorObjects.Add(new Rectangle(520, 200, 180, 120));  // bow rack right
    ranging.InteriorObjects.Add(new Rectangle(80,  480, 160, 60));   // arrow shelf 1
    ranging.InteriorObjects.Add(new Rectangle(260, 480, 160, 60));   // arrow shelf 2
    ranging.InteriorObjects.Add(new Rectangle(440, 480, 160, 60));   // bolt shelf

    buildings.Add(ranging);
}

static void AddWeapons(float x, float y)
{
    var Blacksmith = new NPC(new Vector2(585, 130), "Blacksmith", "Welcome traveller. Looking to purchase a weapon?.");
    Blacksmith.SpriteKey = "Blacksmith";        
    Blacksmith.Facing = NPC.Dir.South; 

    var weapons = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(80, 80, 80, 255),
        new Color(30, 30, 35, 255),
        new Vector2(x + 80, y + 100),
        "WEAPONS",
        Blacksmith,
        entryPos: new Vector2(600, 880)
    );
 
    weapons.InteriorObjects.Clear();
 
    // front service counter
    weapons.InteriorObjects.Add(new Rectangle(400, 80, 500, 50));
    weapons.InteriorObjects.Add(new Rectangle(400, 80, 500, 8));
 
    // --- wall-mounted weapon racks (back wall) ---
    // rack 1 - swords/blades
    weapons.InteriorObjects.Add(new Rectangle(50,  80, 260, 20));
    // rack 2 - bows/ranged
    weapons.InteriorObjects.Add(new Rectangle(50, 180, 260, 20));
    // rack 3 - shields
    weapons.InteriorObjects.Add(new Rectangle(50, 280, 260, 20));
 
    // --- display cases (centre floor) ---
    weapons.InteriorObjects.Add(new Rectangle(420, 280, 180, 60));  // display case 1
    weapons.InteriorObjects.Add(new Rectangle(650, 280, 180, 60));  // display case 2
    weapons.InteriorObjects.Add(new Rectangle(420, 400, 180, 60));  // display case 3
    weapons.InteriorObjects.Add(new Rectangle(650, 400, 180, 60));  // display case 4
 
    // --- armour stand area (right wall) ---
    weapons.InteriorObjects.Add(new Rectangle(1100, 80, 70, 140));  // armour stand 1
    weapons.InteriorObjects.Add(new Rectangle(1230, 80, 70, 140));  // armour stand 2
 
    // --- ammo/supplies shelves (right wall lower) ---
    weapons.InteriorObjects.Add(new Rectangle(1050, 280, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 350, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 420, 320, 30));
    weapons.InteriorObjects.Add(new Rectangle(1050, 490, 320, 30));
 
    // entrance mat
    //weapons.InteriorObjects.Add(new Rectangle(500, 870, 200, 30));
 
    buildings.Add(weapons);
}

static void AddMyHouse(float x, float y)
{
    var house = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(200, 160, 100, 255),
        new Color(180, 140, 100, 255),
        new Vector2(x + 80, y + 100),
        "MY HOUSE",
        new NPC(new Vector2(450, 180), "Mum", "Press space to interact with your chest and wardrobe and to sleep in your bed"),
        entryPos: new Vector2(700, 880)  // centre of entry mat
    );

    house.InteriorObjects.Clear();

    // ── Kitchen ──
    house.InteriorObjects.Add(new Rectangle(60,  55,  530, 55));   // bench top
    house.InteriorObjects.Add(new Rectangle(60,  110, 55,  200));  // bench left
    house.InteriorObjects.Add(new Rectangle(125, 120, 100, 100));  // stove
    house.InteriorObjects.Add(new Rectangle(240, 120, 90,  120));  // fridge

    // ── Dining ──
    house.InteriorObjects.Add(new Rectangle(130, 415, 260, 130));  // table

    // ── Lounge ──
    house.InteriorObjects.Add(new Rectangle(572, 670, 28,  160));  // TV
    house.InteriorObjects.Add(new Rectangle(60,  880, 340, 75));   // couch bottom
    house.InteriorObjects.Add(new Rectangle(60,  660, 75,  216));  // couch left

    // Hallway right wall split into 3 segments with 150px door gaps
    house.InteriorObjects.Add(new Rectangle(800, 0,   12, 150));  // above bathroom door
    house.InteriorObjects.Add(new Rectangle(800, 300, 12, 150));  // between bathroom and bedroom doors
    house.InteriorObjects.Add(new Rectangle(800, 600, 12, 400));  // below bedroom door

    house.InteriorObjects.Add(new Rectangle(800, 400, 540, 12));  // bathroom/bedroom divider

    // ── Bathroom ──
    house.InteriorObjects.Add(new Rectangle(1050, 55,  290, 170)); // shower
    house.InteriorObjects.Add(new Rectangle(820,  55,  80,  105)); // toilet
    house.InteriorObjects.Add(new Rectangle(1260, 320, 80,  75));  // sink

    // ── Bedroom ──
    house.InteriorObjects.Add(new Rectangle(1030, 450, 300, 270)); // bed
    house.InteriorObjects.Add(new Rectangle(950,  462, 72,  72));  // side table
    house.InteriorObjects.Add(new Rectangle(1030, 760, 300, 190)); // wardrobe
    house.InteriorObjects.Add(new Rectangle(820,  870, 105, 90));  // chest

    buildings.Add(house);
}

static void AddGym(float x, float y)
{
    var gym = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(50, 100, 180, 255),
        new Color(40, 40, 60, 255),
        new Vector2(x + 100, y + 100),
        "GYM",
        new NPC(new Vector2(950, 125), "Trainer", "You wanna get big? Train hard every day bro."),
        entryPos: new Vector2(1036, 902)
    );

    gym.InteriorObjects.Clear();
    gym.InteriorObjects.Add(new Rectangle(180, 180, 140, 60));   // dumbbell rack
    gym.InteriorObjects.Add(new Rectangle(480, 285, 220, 55));   // bench press
    gym.InteriorObjects.Add(new Rectangle(180, 920, 100, 60));   // treadmill 1
    gym.InteriorObjects.Add(new Rectangle(380, 920, 100, 60));   // treadmill 2
    gym.InteriorObjects.Add(new Rectangle(580, 920, 100, 60));   // treadmill 3
    gym.InteriorObjects.Add(new Rectangle(10, 200, 80, 60));     // bike 1
    gym.InteriorObjects.Add(new Rectangle(10, 400, 80, 60));     // bike 2
    gym.InteriorObjects.Add(new Rectangle(10, 600, 80, 60));     // bike 3
    gym.InteriorObjects.Add(new Rectangle(700, 150, 200, 40));   // counter
    gym.InteriorObjects.Add(new Rectangle(1310, 200, 60, 80));   // toilet 1
    gym.InteriorObjects.Add(new Rectangle(1310, 340, 60, 80));   // toilet 2
    gym.InteriorObjects.Add(new Rectangle(1310, 480, 80, 80));   // shower

    buildings.Add(gym);
}

static void AddMarae(float x, float y)
{
    var marae = new Building(
        new Rectangle(x, y, 220, 160),
        new Color(180, 60, 40, 255),
        new Color(140, 80, 40, 255),
        new Vector2(x + 110, y + 150),
        "MARAE",
        new NPC(new Vector2(700, 200), "Kaumatua", "Haere mai, haere mai, haere mai."),
        entryPos: new Vector2(700, 880)
    );

    marae.InteriorObjects.Clear();

    // --- outer walls (leave gap at bottom for entry) ---
    marae.InteriorObjects.Add(new Rectangle(0,   0,   20, 1000)); // left wall
    marae.InteriorObjects.Add(new Rectangle(1380, 0,  20, 1000)); // right wall
    marae.InteriorObjects.Add(new Rectangle(0,   0,  1400, 20)); // top wall

    // --- wharenui (meeting house) back wall ---
    marae.InteriorObjects.Add(new Rectangle(200, 80, 1000, 20));  // front wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(200, 80, 20, 400));   // left wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(1180, 80, 20, 400));  // right wall of wharenui
    marae.InteriorObjects.Add(new Rectangle(200, 460, 20, 20));   // bottom left corner
    marae.InteriorObjects.Add(new Rectangle(1180, 460, 20, 20));  // bottom right corner

    // door gaps in wharenui front wall (two doors, centred)
    // left door gap: x=530-640, right door gap: x=760-870
    marae.InteriorObjects.Add(new Rectangle(220, 80, 310, 20));   // left of left door
    marae.InteriorObjects.Add(new Rectangle(640, 80, 120, 20));   // between doors
    marae.InteriorObjects.Add(new Rectangle(870, 80, 310, 20));   // right of right door

    // --- wharenui interior furniture ---
    // carvings on back wall
    marae.InteriorObjects.Add(new Rectangle(220, 90, 30, 120));   // left carving
    marae.InteriorObjects.Add(new Rectangle(1150, 90, 30, 120));  // right carving
    marae.InteriorObjects.Add(new Rectangle(650, 90, 100, 80));   // centre carving

    // benches along walls inside wharenui
    marae.InteriorObjects.Add(new Rectangle(220, 100, 20, 340));  // left bench
    marae.InteriorObjects.Add(new Rectangle(1160, 100, 20, 340)); // right bench

    // --- atea (courtyard) features ---
    // pou (posts) lining the path
    marae.InteriorObjects.Add(new Rectangle(280, 530, 20, 20));   // left pou 1
    marae.InteriorObjects.Add(new Rectangle(280, 640, 20, 20));   // left pou 2
    marae.InteriorObjects.Add(new Rectangle(280, 750, 20, 20));   // left pou 3
    marae.InteriorObjects.Add(new Rectangle(1100, 530, 20, 20));  // right pou 1
    marae.InteriorObjects.Add(new Rectangle(1100, 640, 20, 20));  // right pou 2
    marae.InteriorObjects.Add(new Rectangle(1100, 750, 20, 20));  // right pou 3

    // waharoa (gateway) posts at entrance
    marae.InteriorObjects.Add(new Rectangle(500, 880, 20, 120));  // left gate post
    marae.InteriorObjects.Add(new Rectangle(880, 880, 20, 120));  // right gate post

    buildings.Add(marae);
}

static void AddPoliceStation(float x, float y)
{
    var cop = new NPC(new Vector2(1235, 80), "Police officer", "What can I do for you today?");
    cop.SpriteKey = "cop";        
    cop.Facing = NPC.Dir.South;

    var station = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(30, 30, 120, 255),
        new Color(40, 40, 80, 255),
        new Vector2(x + 80, y + 100),
        "POLICE STATION",
        cop,
        entryPos: new Vector2(1225, 897)
    );

    station.InteriorObjects.Clear();

    // --- front desk (right side reception area) ---
    station.InteriorObjects.Add(new Rectangle(1095, 80, 280, 50));   // desk
    station.InteriorObjects.Add(new Rectangle(1215, 60, 40, 28));    // computer

    // --- police badge / shield sign ---
    station.InteriorObjects.Add(new Rectangle(1224, 15, 22, 30));    // badge body

    // --- vertical wall dividers (left zone | office zone | reception) ---
    station.InteriorObjects.Add(new Rectangle(350, 0, 20, 420));
    station.InteriorObjects.Add(new Rectangle(700, 0, 20, 420));
    station.InteriorObjects.Add(new Rectangle(1050, 0, 20, 400));
    station.InteriorObjects.Add(new Rectangle(1050, 600, 20, 420));

    // --- horizontal hallway walls ---
    station.InteriorObjects.Add(new Rectangle(0,   420, 810, 20));   // top hallway wall
    station.InteriorObjects.Add(new Rectangle(0,   600, 810, 20));   // bottom hallway wall

    // --- vertical dividers bottom zone ---
    station.InteriorObjects.Add(new Rectangle(350, 600, 20, 420));
    station.InteriorObjects.Add(new Rectangle(700, 600, 20, 420));

    // --- desks top row (offices) ---
    station.InteriorObjects.Add(new Rectangle(40,  40, 260, 60));
    station.InteriorObjects.Add(new Rectangle(400, 40, 260, 60));
    station.InteriorObjects.Add(new Rectangle(750, 40, 260, 60));

    // --- desks bottom row (offices) ---
    station.InteriorObjects.Add(new Rectangle(40,  830, 260, 60));
    station.InteriorObjects.Add(new Rectangle(400, 830, 260, 60));
    station.InteriorObjects.Add(new Rectangle(750, 830, 260, 60));

    // --- chairs top ---
    station.InteriorObjects.Add(new Rectangle(40,  150, 35, 35));
    station.InteriorObjects.Add(new Rectangle(400, 150, 35, 35));
    station.InteriorObjects.Add(new Rectangle(750, 150, 35, 35));

    // --- chairs bottom ---
    station.InteriorObjects.Add(new Rectangle(40,  760, 35, 35));
    station.InteriorObjects.Add(new Rectangle(400, 760, 35, 35));
    station.InteriorObjects.Add(new Rectangle(750, 760, 35, 35));

    // --- holding cells (far left, full height) ---
    station.InteriorObjects.Add(new Rectangle(0, 0, 20, 1000));      // left outer wall
    station.InteriorObjects.Add(new Rectangle(0, 490, 120, 20));     // cell divider

    buildings.Add(station);
}
    }
}
