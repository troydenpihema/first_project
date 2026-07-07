using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void CoastguardRecover(Vector2 station)
        {
            // stranded = watercraft in the ocean, unmanned, and either out of fuel or far offshore
            var stranded = vehicles
                .Where(v => v.IsWatercraft && !v.Driving
                    && IsInWater(new Vector2(v.Bounds.X + v.Bounds.Width / 2, v.Bounds.Y + v.Bounds.Height / 2))
                    && (v.Fuel < 10f || v.Position.X > oceanBounds.X + 600))
                .OrderByDescending(v => v.Position.X)   // deepest first
                .FirstOrDefault();

            if (stranded == null) { ShowNotification("Coastguard: no stranded vessels out there!"); return; }
            if (player.Money < CoastguardFee) { ShowNotification($"Coastguard tow costs ${CoastguardFee}."); return; }

            player.Money -= CoastguardFee;
            Vector2 dock = boatSpawnPoints.OrderBy(p => Vector2.Distance(p, station)).First();
            stranded.Position = dock;
            stranded.velocity = Vector2.Zero;
            stranded.Fuel = Math.Max(stranded.Fuel, stranded.MaxFuel * 0.25f);   // emergency jerry can
            ShowNotification($"Coastguard towed your {stranded.Type} to the dock! (-${CoastguardFee})");
        }

        static void GenerateBoatSpawns()
        {
            boatSpawnPoints.Clear();
            float spawnX = oceanBounds.X + 60;   // just inside water, clear of the beach edge
            for (float y = oceanBounds.Y + 200; y < oceanBounds.Y + oceanBounds.Height - 200; y += 3000)
                boatSpawnPoints.Add(new Vector2(spawnX, y));
        }

static void DrawBoatTheoryTest()
{
    if (!boatTheoryOpen || currentBuilding?.BuildingName != "BOAT LICENCE OFFICE") return;

    var questions = boatTheoryTest;
    Color navy = new Color((byte)10,(byte)50,(byte)90,(byte)255);
    int px = ScreenWidth/2 - 380, py = 40;
    Raylib.DrawRectangle(px, py, 760, 620, new Color((byte)5,(byte)15,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 760, 620, navy);
    Program.DrawTextUI("BOAT THEORY TEST", px + 260, py + 12, 26, new Color((byte)100,(byte)200,(byte)255,(byte)255));

    if (boatTheoryFinished)
    {
        Color resCol = boatTheoryPassed ? Color.Green : Color.Red;
        string resText = boatTheoryPassed ? "PASSED! ✓" : "FAILED ✗";
        int rw = Program.MeasureTextUI(resText, 40);
        Program.DrawTextUI(resText, px + 380 - rw/2, py + 160, 40, resCol);
        Program.DrawTextUI($"Score: {boatTheoryScore} / {questions.Length}", px + 300, py + 220, 26, Color.White);
        Program.DrawTextUI(boatTheoryPassed ? "Boat theory passed — practicals unlocked!" : "Need 8/10 to pass. Try again ($50).", px + 60, py + 270, 20, Color.LightGray);

        Rectangle closeBtn = new Rectangle(px + 280, py + 340, 200, 50);
        bool hC = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeBtn);
        Raylib.DrawRectangleRec(closeBtn, new Color((byte)0,(byte)40,(byte)80,(byte)255));
        Raylib.DrawRectangleLinesEx(closeBtn, 2, hC ? Color.Gold : navy);
        Program.DrawTextUI("Continue", px + 320, py + 357, 22, hC ? Color.Gold : Color.White);
        if ((hC && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            boatTheoryOpen = false;
            boatMenuOpen = true;
        }
        return;
    }

    var q = questions[boatTheoryQuestion];
    Program.DrawTextUI($"Question {boatTheoryQuestion + 1} of {questions.Length}   Score: {boatTheoryScore}", px + 20, py + 50, 16, Color.LightGray);

    float prog = (float)boatTheoryQuestion / questions.Length;
    Raylib.DrawRectangle(px + 20, py + 72, 720, 10, new Color((byte)20,(byte)20,(byte)40,(byte)255));
    Raylib.DrawRectangle(px + 20, py + 72, (int)(720 * prog), 10, navy);

    Program.DrawTextUI(q.Question, px + 20, py + 100, 22, Color.White);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < q.Options.Length; i++)
    {
        Rectangle opt = new Rectangle(px + 20, py + 160 + i * 80, 720, 68);
        bool hover = Raylib.CheckCollisionPointRec(mouse, opt) && boatTheorySelected < 0;
        Color bg = boatTheorySelected >= 0
            ? (i == q.Correct ? new Color((byte)0,(byte)80,(byte)20,(byte)255)
               : i == boatTheorySelected ? new Color((byte)80,(byte)10,(byte)10,(byte)255)
               : new Color((byte)20,(byte)20,(byte)30,(byte)255))
            : hover ? new Color((byte)0,(byte)50,(byte)100,(byte)255) : new Color((byte)15,(byte)25,(byte)45,(byte)255);
        Color border = boatTheorySelected >= 0
            ? (i == q.Correct ? Color.Green : i == boatTheorySelected ? Color.Red : new Color((byte)40,(byte)40,(byte)60,(byte)255))
            : hover ? Color.Gold : navy;

        Raylib.DrawRectangleRec(opt, bg);
        Raylib.DrawRectangleLinesEx(opt, 2, border);
        Program.DrawTextUI($"{(char)('A' + i)}. {q.Options[i]}", px + 34, py + 175 + i * 80, 20, Color.White);

        if (hover && boatTheorySelected < 0 && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            boatTheorySelected = i;
            if (i == q.Correct) boatTheoryScore++;
        }
    }

    if (boatTheorySelected >= 0)
    {
        Rectangle nextBtn = new Rectangle(px + 560, py + 560, 180, 44);
        bool hN = Raylib.CheckCollisionPointRec(mouse, nextBtn);
        Raylib.DrawRectangleRec(nextBtn, new Color((byte)0,(byte)60,(byte)130,(byte)255));
        Raylib.DrawRectangleLinesEx(nextBtn, 2, hN ? Color.Gold : navy);
        Program.DrawTextUI(boatTheoryQuestion < questions.Length - 1 ? "Next >" : "Finish", px + 590, py + 573, 20, hN ? Color.Gold : Color.White);

        if ((hN && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            boatTheoryQuestion++;
            boatTheorySelected = -1;
            if (boatTheoryQuestion >= questions.Length)
            {
                boatTheoryFinished = true;
                boatTheoryPassed = boatTheoryScore >= 8;
                if (boatTheoryPassed) hasBoatTheory = true;
            }
        }
    }
}

static float VehicleSpeedFor(Vehicle.VehicleType type) => type == Vehicle.VehicleType.Boat ? 420f : DefaultSpeedFor(type);

static void StartBoatTest()
{
    testReturnPos = player.Position;
    testVehicle = new Vehicle(new Vector2(200, 500), new Color((byte)60,(byte)140,(byte)200,(byte)255), 420f, Vehicle.VehicleType.Boat);
    testVehicle.Driving = true;
    testVehicle.Fuel = 100f;
    player.Hidden = true;
    testCamera = new Camera2D { Offset = new Vector2(ScreenWidth/2, ScreenHeight/2), Zoom = 1f, Target = testVehicle.Position };
    testStage = 0; testComplete = false; testFailed = false; testConesHit = 0;
    maxConesAllowed = 2 - boatTestingTier / 2;   // higher tiers = stricter (Superyacht allows fewer buoy hits)
    LoadBoatStage(0);
    isBoatTest = true;   // ADDED — see note below
    ChangeScene(SceneState.DrivingTest);
}

static void LoadBoatStage(int stage)
{
    testStage = stage;
    stageFailed = false;
    stageComplete = false;
    testCones.Clear();
    parkingCars.Clear();
    trafficCars.Clear();
    trafficVelocities.Clear();
    laneBoundaries.Clear();

    switch (stage)
    {
        case 0:
            testVehicle.Position = new Vector2(100, 500);
            stageTimeLimit = 35f;
            stageTimer = stageTimeLimit;
            for (int i = 0; i < 6; i++)
            {
                float x = 200 + i * 180f;
                float y = i % 2 == 0 ? 460f : 540f;
                testCones.Add(new Vector2(x, y));
            }
            testCheckpoints = new Vector2[] { new Vector2(1300, 500) };
            testCurrentCheckpoint = 0;
            testMessage = "Boat Test: Weave through the buoys!";
            testMessageTimer = 3f;
            break;

        case 1:
            testVehicle.Position = new Vector2(300, 300);
            stageTimeLimit = 30f;
            stageTimer = stageTimeLimit;
            parkedSuccessfully = false;
            parkingTimer = 0f;
            parkingCars.Add(new Rectangle(580, 500, 20, 80));
            parkingCars.Add(new Rectangle(780, 500, 20, 80));
            parkingBay = new Rectangle(600, 500, 180, 80);
            testMessage = "Boat Test: Dock cleanly between the markers!";
            testMessageTimer = 3f;
            break;

        case 2:
            testVehicle.Position = new Vector2(100, 500);
            stageTimeLimit = 40f;
            stageTimer = stageTimeLimit;
            testCheckpoints = new Vector2[] {
                new Vector2(600, 200), new Vector2(1200, 600), new Vector2(700, 900)
            };
            testCurrentCheckpoint = 0;
            testMessage = "Boat Test: Navigate the checkpoints!";
            testMessageTimer = 3f;
            break;
    }
}

        static bool CanDriveNow()
        {
            if (!HasTheoryForCurrentClass) return false;
            if (HasPracticalForCurrentClass) return true;
            float hour = GetCurrentHour();
            return hour >= 6f && hour < 22f;
        }

        static void DrawLicenceCongrats()
        {
            Raylib.DrawRectangle(0,0,ScreenWidth,ScreenHeight,Color.Black);
            int tw = Program.MeasureTextUI(licenceCongratsText, 40);
            Program.DrawTextUI(licenceCongratsText, ScreenWidth/2 - tw/2, ScreenHeight/2 - 60, 40, Color.Gold);
            string sub = "Press SPACE to continue";
            int sw = Program.MeasureTextUI(sub, 20);
            Program.DrawTextUI(sub, ScreenWidth/2 - sw/2, ScreenHeight/2 + 10, 20, Color.LightGray);
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                licenceCongratsOpen = false;
                licenceMailMenuOpen = true;   // now show the delivery picker
            }
        }   

        record TheoryQ(string Question, string[] Options, int Correct);

        static int VehicleUnlockLevel(Vehicle.VehicleType type) => type switch {
        Vehicle.VehicleType.Sedan       => 1,
        Vehicle.VehicleType.Ute         => 10,
        Vehicle.VehicleType.SUV         => 20,
        Vehicle.VehicleType.Truck       => 30,
        Vehicle.VehicleType.Convertible => 30,
        Vehicle.VehicleType.Ambulance   => 40,
        Vehicle.VehicleType.FireTruck   => 40,
        Vehicle.VehicleType.PoliceCar   => 60,
        Vehicle.VehicleType.MonsterTruck => 70,
        Vehicle.VehicleType.RacingCar   => 70,
        Vehicle.VehicleType.MuscleCar   => 70,
        _ => 1
    };

static LicenceClass VehicleRequiredClass(Vehicle.VehicleType type) => VehicleUnlockLevel(type) switch {
    >= 1  and <= 19 => LicenceClass.D,
    >= 20 and <= 39 => LicenceClass.C,
    >= 40 and <= 59 => LicenceClass.B,
    >= 60 and <= 79 => LicenceClass.A,
    >= 80           => LicenceClass.S,
    _ => LicenceClass.D
};

static bool HasTheoryForClass(LicenceClass c) => c switch {
    LicenceClass.D => hasTheoryD, LicenceClass.C => hasTheoryC, LicenceClass.B => hasTheoryB,
    LicenceClass.A => hasTheoryA, LicenceClass.S => hasTheoryS, _ => false
};

static bool HasFullLicenceForClass(LicenceClass c) => c switch {
    LicenceClass.D => hasPracticalD, LicenceClass.C => hasPracticalC, LicenceClass.B => hasPracticalB,
    LicenceClass.A => hasPracticalA, LicenceClass.S => hasPracticalS, _ => false
};

static bool CanDriveVehicleNow(Vehicle.VehicleType type)
{
    var reqClass = VehicleRequiredClass(type);
    if (!HasTheoryForClass(reqClass)) return false;
    if (HasFullLicenceForClass(reqClass)) return true;
    for (int c = (int)reqClass + 1; c <= (int)LicenceClass.S; c++)
        if (HasTheoryForClass((LicenceClass)c) || HasFullLicenceForClass((LicenceClass)c)) return true;
    float hour = GetCurrentHour();
    return hour >= 6f && hour < 22f;
}

        public static void PlayHorn(Vehicle.VehicleType t)
        {
            float pitch = t switch
            {
                Vehicle.VehicleType.CruiseShip => 0.4f,   // deep ship blast
                Vehicle.VehicleType.Yacht      => 0.6f,
                Vehicle.VehicleType.Truck or Vehicle.VehicleType.FireTruck or Vehicle.VehicleType.MonsterTruck => 0.7f,
                Vehicle.VehicleType.Jetski or Vehicle.VehicleType.Dinghy => 1.3f,   // squeaky
                _ => 1f
            };
            Raylib.SetSoundPitch(soundHorn, pitch);
            Raylib.PlaySound(soundHorn);
        }

static void DrawCoastguardStations()
{
    foreach (var s in coastguardStations)
    {
        int x = (int)s.X, y = (int)s.Y;
        // bench
        Raylib.DrawRectangle(x - 30, y, 60, 10, new Color((byte)160,(byte)115,(byte)70,(byte)255));
        Raylib.DrawRectangle(x - 26, y + 10, 8, 14, new Color((byte)120,(byte)85,(byte)50,(byte)255));
        Raylib.DrawRectangle(x + 18, y + 10, 8, 14, new Color((byte)120,(byte)85,(byte)50,(byte)255));
        Raylib.DrawRectangle(x - 30, y - 14, 60, 6, new Color((byte)160,(byte)115,(byte)70,(byte)255)); // backrest
        // umbrella
        Raylib.DrawRectangle(x - 2, y - 60, 4, 60, new Color((byte)200,(byte)200,(byte)205,(byte)255));
        for (int i = 0; i < 4; i++)
            Raylib.DrawCircleSector(new Vector2(x, y - 58), 42, 180 + i * 45, 225 + i * 45, 8,
                i % 2 == 0 ? Color.Red : Color.White);
        // sign
        Raylib.DrawRectangle(x - 44, y - 40, 10, 54, new Color((byte)230,(byte)230,(byte)235,(byte)255));
        Raylib.DrawRectangle(x - 46, y - 46, 14, 10, Color.Red);

        if (Vector2.Distance(player.Center, s) < 110)
            Program.DrawTextUI($"E = Coastguard tow (${CoastguardFee})", x - 80, y - 90, 16, Color.White);
    }
}

static void DrawAAMenu()
{
    if (!aaMenuOpen || currentBuilding?.BuildingName != "AA") return;

    Color aaBlue = new Color((byte)0,(byte)80,(byte)160,(byte)255);
    int px = ScreenWidth/2 - 300, py = 60;
    Raylib.DrawRectangle(px, py, 600, 520, new Color((byte)5,(byte)20,(byte)40,(byte)245));
    Raylib.DrawRectangleLines(px, py, 600, 520, aaBlue);
    Program.DrawTextUI("AA DRIVING SERVICES", px + 130, py + 12, 26, new Color((byte)0,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI($"Driving Lv {player.DrivingLevel}  |  Class: {ClassToTestFor}  |  ${player.Money}", px + 60, py + 46, 16, Color.LightGray);

    // current licence status
    Program.DrawTextUI($"Theory: {(HasTheoryForTestClass ? "PASSED " : "NOT PASSED")}",   px + 20, py + 80, 18, HasTheoryForTestClass ? Color.Green : Color.Red);
    Program.DrawTextUI($"Practical: {(HasPracticalForTestClass ? "PASSED " : "NOT PASSED")}", px + 20, py + 104, 18, HasPracticalForTestClass ? Color.Green : Color.Red);
    Program.DrawTextUI(HasPracticalForTestClass ? "Full licence  drive anytime!" : HasTheoryForTestClass ? "Restricted  6am to 10pm only" : "No licence  cannot drive!", px + 20, py + 128, 16, HasPracticalForTestClass ? Color.Green : HasTheoryForTestClass ? Color.Orange : Color.Red);

    Vector2 mouse = Raylib.GetMousePosition();

    // Theory test button
    bool theoryAvail = !HasTheoryForTestClass && PreviousLicenceHeld;
    Rectangle theoryBtn = new Rectangle(px + 20, py + 170, 560, 60);
    bool hT = Raylib.CheckCollisionPointRec(mouse, theoryBtn);
    Raylib.DrawRectangleRec(theoryBtn, theoryAvail ? new Color((byte)0,(byte)40,(byte)80,(byte)255) : new Color((byte)20,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(theoryBtn, 2, theoryAvail && hT ? Color.Gold : aaBlue);
    Program.DrawTextUI($"THEORY TEST  Class {ClassToTestFor}  ($50)", px + 30, py + 182, 20, theoryAvail ? Color.White : Color.DarkGray);
    Program.DrawTextUI(!PreviousLicenceHeld ? $"Need Class {ClassToTestFor - 1} licence first"
              : theoryAvail ? "10 questions, need 8/10 to pass" : "Already passed ",
              px + 30, py + 204, 14, theoryAvail ? Color.LightGray : Color.Green);
    if (theoryAvail && hT && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (player.Money >= 50)
        {
            player.Money -= 50;
            StartTestTransition(() => {
                aaTheoryQuestion = 0;
                aaTheoryScore = 0;
                aaTheorySelected = -1;
                aaTheoryFinished = false;
                aaTheoryPassed = false;
                aaTheoryOpen = true;
            });
            aaMenuOpen = false;
        }
        else { shopMessage = "Need $50 for the theory test bro!"; shopMessageTimer = 2f; }
    }

    // Practical test button
    bool practAvail = HasTheoryForTestClass && PracticalAvailable && PreviousLicenceHeld;
    Rectangle practBtn = new Rectangle(px + 20, py + 250, 560, 60);
    bool hP = Raylib.CheckCollisionPointRec(mouse, practBtn);
    Raylib.DrawRectangleRec(practBtn, practAvail ? new Color((byte)0,(byte)60,(byte)20,(byte)255) : new Color((byte)20,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(practBtn, 2, practAvail && hP ? Color.Gold : new Color((byte)0,(byte)100,(byte)40,(byte)255));
    string practLabel = !PreviousLicenceHeld     ? $"Need Class {ClassToTestFor - 1} licence first" :
                        !HasTheoryForTestClass   ? "Pass theory first!" :
                        !PracticalAvailable      ? $"Reach Lv {PracticalUnlockLevel()} first" :
                                                    $"PRACTICAL TEST — Class {ClassToTestFor}  ($100)";
    Program.DrawTextUI(practLabel, px + 30, py + 262, 20, practAvail ? Color.White : Color.DarkGray);
    Program.DrawTextUI(practAvail ? "Drive a test course within time limit" : "", px + 30, py + 284, 14, Color.LightGray);

    if (practAvail && hP && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (player.Money >= 100)
        {
            player.Money -= 100;
            preTestScene = currentScene;
            StartTestTransition(() => StartDrivingTest());
            aaMenuOpen = false;
        }
        else { shopMessage = "Need $100 for the practical test bro!"; shopMessageTimer = 2f; }
    }

    // Licence info panel
    Raylib.DrawRectangle(px + 20, py + 330, 560, 160, new Color((byte)5,(byte)15,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(px + 20, py + 330, 560, 160, new Color((byte)0,(byte)60,(byte)100,(byte)255));
    Program.DrawTextUI("LICENCE CLASSES", px + 190, py + 340, 18, new Color((byte)0,(byte)200,(byte)255,(byte)255));
    string[] info = {
        "D (Lv 1-19)  — Basic vehicles, 6am-10pm restricted",
        "C (Lv 20-39) — All standard vehicles",
        "B (Lv 40-59) — Trucks & large vehicles",
        "A (Lv 60-79) — High performance vehicles",
        "S (Lv 80+)   — Unrestricted, all vehicles anytime"
    };
    for (int i = 0; i < 5; i++)
        Program.DrawTextUI(info[i], px + 30, py + 362 + i * 22, 13, i == (int)ClassToTestFor - 1 ? Color.Gold : Color.LightGray);

    Program.DrawTextUI("Q = Close", px + 260, py + 498, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) aaMenuOpen = false;
}

static void DrawAATheoryTest()
{
    if (!aaTheoryOpen || currentBuilding?.BuildingName != "AA") return;

    int testIdx = (int)ClassToTestFor - 1;
    var questions = theoryTests[Math.Clamp(testIdx, 0, 4)];

    Color aaBlue = new Color((byte)0,(byte)80,(byte)160,(byte)255);
    int px = ScreenWidth/2 - 380, py = 40;
    Raylib.DrawRectangle(px, py, 760, 620, new Color((byte)5,(byte)15,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 760, 620, aaBlue);
    Program.DrawTextUI($"THEORY TEST — Class {ClassToTestFor}", px + 210, py + 12, 26, new Color((byte)0,(byte)200,(byte)255,(byte)255));

    if (aaTheoryFinished)
    {
        Color resCol = aaTheoryPassed ? Color.Green : Color.Red;
        string resText = aaTheoryPassed ? "PASSED! ✓" : "FAILED ✗";
        int rw = Program.MeasureTextUI(resText, 40);
        Program.DrawTextUI(resText, px + 380 - rw/2, py + 160, 40, resCol);
        Program.DrawTextUI($"Score: {aaTheoryScore} / {questions.Length}", px + 300, py + 220, 26, Color.White);
        Program.DrawTextUI(aaTheoryPassed ? "Your theory licence is now active!" : "Need 8/10 to pass. Try again ($50).", px + 80, py + 270, 20, Color.LightGray);

        Rectangle closeBtn = new Rectangle(px + 280, py + 340, 200, 50);
        bool hC = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeBtn);
        Raylib.DrawRectangleRec(closeBtn, new Color((byte)0,(byte)40,(byte)80,(byte)255));
        Raylib.DrawRectangleLinesEx(closeBtn, 2, hC ? Color.Gold : aaBlue);
        Program.DrawTextUI("Continue", px + 320, py + 357, 22, hC ? Color.Gold : Color.White);
        if ((hC && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            aaTheoryOpen = false;
            aaMenuOpen = true;
        }
        return;
    }

    var q = questions[aaTheoryQuestion];
    Program.DrawTextUI($"Question {aaTheoryQuestion + 1} of {questions.Length}   Score: {aaTheoryScore}", px + 20, py + 50, 16, Color.LightGray);

    float prog = (float)aaTheoryQuestion / questions.Length;
    Raylib.DrawRectangle(px + 20, py + 72, 720, 10, new Color((byte)20,(byte)20,(byte)40,(byte)255));
    Raylib.DrawRectangle(px + 20, py + 72, (int)(720 * prog), 10, aaBlue);

    Program.DrawTextUI(q.Question, px + 20, py + 100, 22, Color.White);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < q.Options.Length; i++)
    {
        Rectangle opt = new Rectangle(px + 20, py + 160 + i * 80, 720, 68);
        bool hover = Raylib.CheckCollisionPointRec(mouse, opt) && aaTheorySelected < 0;
        Color bg = aaTheorySelected >= 0
            ? (i == q.Correct ? new Color((byte)0,(byte)80,(byte)20,(byte)255)
               : i == aaTheorySelected ? new Color((byte)80,(byte)10,(byte)10,(byte)255)
               : new Color((byte)20,(byte)20,(byte)30,(byte)255))
            : hover ? new Color((byte)0,(byte)50,(byte)100,(byte)255) : new Color((byte)15,(byte)25,(byte)45,(byte)255);
        Color border = aaTheorySelected >= 0
            ? (i == q.Correct ? Color.Green : i == aaTheorySelected ? Color.Red : new Color((byte)40,(byte)40,(byte)60,(byte)255))
            : hover ? Color.Gold : aaBlue;

        Raylib.DrawRectangleRec(opt, bg);
        Raylib.DrawRectangleLinesEx(opt, 2, border);
        Program.DrawTextUI($"{(char)('A' + i)}. {q.Options[i]}", px + 34, py + 175 + i * 80, 20, Color.White);

        if (hover && aaTheorySelected < 0 && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            aaTheorySelected = i;
            if (i == q.Correct) aaTheoryScore++;
        }
    }

    if (aaTheorySelected >= 0)
    {
        Rectangle nextBtn = new Rectangle(px + 560, py + 560, 180, 44);
        bool hN = Raylib.CheckCollisionPointRec(mouse, nextBtn);
        Raylib.DrawRectangleRec(nextBtn, new Color((byte)0,(byte)60,(byte)130,(byte)255));
        Raylib.DrawRectangleLinesEx(nextBtn, 2, hN ? Color.Gold : aaBlue);
        Program.DrawTextUI(aaTheoryQuestion < questions.Length - 1 ? "Next >" : "Finish", px + 590, py + 573, 20, hN ? Color.Gold : Color.White);

        if ((hN && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            aaTheoryQuestion++;
            aaTheorySelected = -1;
            if (aaTheoryQuestion >= questions.Length)
            {
                aaTheoryFinished = true;
                aaTheoryPassed = aaTheoryScore >= 8;
                if (aaTheoryPassed)
                {
                    switch (ClassToTestFor) {
                        case LicenceClass.D: hasTheoryD = true; break;
                        case LicenceClass.C: hasTheoryC = true; break;
                        case LicenceClass.B: hasTheoryB = true; break;
                        case LicenceClass.A: hasTheoryA = true; break;
                        case LicenceClass.S: hasTheoryS = true; break;
                    }
                    licencePendingClass = ClassToTestFor;
                    licencePendingIsTheory[(int)ClassToTestFor - 1] = true;
                    licenceCongratsText = $"CONGRATULATIONS! You passed your Class {ClassToTestFor} THEORY test!";
                    licenceCongratsOpen = true;
                }
            }
        }
    }
}

static void StartDrivingTest()
{
    testingForClass = ClassToTestFor;
    testReturnPos = player.Position;

    Vehicle.VehicleType vtype = testingForClass switch {
        LicenceClass.D => Vehicle.VehicleType.Sedan,
        LicenceClass.C => Vehicle.VehicleType.SUV,
        LicenceClass.B => Vehicle.VehicleType.Truck,
        LicenceClass.A => Vehicle.VehicleType.Sedan,
        LicenceClass.S => Vehicle.VehicleType.PoliceCar,
        _ => Vehicle.VehicleType.Sedan
    };
    Color vCol = testingForClass switch {
        LicenceClass.D => new Color((byte)60,(byte)120,(byte)200,(byte)255),
        LicenceClass.C => new Color((byte)60,(byte)160,(byte)60,(byte)255),
        LicenceClass.B => new Color((byte)160,(byte)100,(byte)40,(byte)255),
        LicenceClass.A => new Color((byte)180,(byte)20,(byte)20,(byte)255),
        LicenceClass.S => new Color((byte)20,(byte)20,(byte)20,(byte)255),
        _ => Color.Gray
    };
    float spd = testingForClass switch {
        LicenceClass.D => 280f, LicenceClass.C => 340f, LicenceClass.B => 260f,
        LicenceClass.A => 500f, LicenceClass.S => 600f, _ => 300f
    };

    testVehicle = new Vehicle(new Vector2(200, 500), vCol, spd, vtype);
    testVehicle.Driving = true;
    testVehicle.Fuel = 100f;
    player.Hidden = true;

    testCamera = new Camera2D();
    testCamera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
    testCamera.Zoom = 1f;
    testCamera.Target = testVehicle.Position;

    testStage = 0;
    testComplete = false;
    testFailed = false;
    testConesHit = 0;
    maxConesAllowed = testingForClass switch {
        LicenceClass.D => 3, LicenceClass.C => 3, LicenceClass.B => 2,
        LicenceClass.A => 1, LicenceClass.S => 0, _ => 3
    };

    LoadStage(0);

    ChangeScene(SceneState.DrivingTest);
}

static void LoadStage(int stage)
{
    testStage = stage;
    stageFailed = false;
    stageComplete = false;
    testCones.Clear();
    parkingCars.Clear();
    trafficCars.Clear();
    trafficVelocities.Clear();
    laneBoundaries.Clear();

    switch (stage)
    {
        case 0: // ── SLALOM ────────────────────────────────────────────────
            testVehicle.Position = new Vector2(100, 500);
            stageTimeLimit = 30f;
            stageTimer = stageTimeLimit;
            // staggered cones in two rows
            for (int i = 0; i < 8; i++)
            {
                float x = 200 + i * 150f;
                float y = i % 2 == 0 ? 460f : 540f;
                testCones.Add(new Vector2(x, y));
            }
            // end gate cones
            testCones.Add(new Vector2(1450, 460));
            testCones.Add(new Vector2(1450, 540));
            testCheckpoints = new Vector2[] { new Vector2(1500, 500) };
            testCurrentCheckpoint = 0;
            testMessage = "Stage 1: Slalom! Weave through the cones!";
            testMessageTimer = 3f;
            break;

        case 1: // ── BAY PARKING ───────────────────────────────────────────
            testVehicle.Position = new Vector2(400, 300);
            stageTimeLimit = 30f;
            stageTimer = stageTimeLimit;
            parkedSuccessfully = false;
            parkingTimer = 0f;
            // two parked cars either side of the bay
            parkingCars.Add(new Rectangle(580, 500, 80, 40));  // left car
            parkingCars.Add(new Rectangle(780, 500, 80, 40));  // right car
            // bay is the gap between them
            parkingBay = new Rectangle(662, 500, 116, 40);
            testMessage = "Stage 2: Bay Parking! Drive into the marked bay!";
            testMessageTimer = 3f;
            break;

        case 2: // ── INTERSECTION ──────────────────────────────────────────
            testVehicle.Position = new Vector2(200, 500);
            stageTimeLimit = 45f;
            stageTimer = stageTimeLimit;
            intersectionCleared = false;
            // traffic cars and their velocities
            trafficCars.Add(new Vector2(1200, 500)); trafficVelocities.Add(new Vector2(-180f, 0));
            trafficCars.Add(new Vector2(100, 480));  trafficVelocities.Add(new Vector2(160f, 0));
            trafficCars.Add(new Vector2(700, 100));  trafficVelocities.Add(new Vector2(0, 150f));
            trafficCars.Add(new Vector2(750, 900));  trafficVelocities.Add(new Vector2(0, -170f));
            // goal checkpoint on far side
            testCheckpoints = new Vector2[] { new Vector2(1200, 500) };
            testCurrentCheckpoint = 0;
            testMessage = "Stage 3: Intersection! Cross safely — avoid all traffic!";
            testMessageTimer = 3f;
            break;

        case 3: // ── LANE KEEPING ──────────────────────────────────────────
            testVehicle.Position = new Vector2(100, 480);
            stageTimeLimit = 45f;
            stageTimer = stageTimeLimit;
            laneCheckpointsHit = 0;
            laneCheckpointsRequired = 5;
            // road boundaries — top and bottom of lane
            laneBoundaries.Add(new Rectangle(0, 420, 2000, 20));    // top wall
            laneBoundaries.Add(new Rectangle(0, 560, 2000, 20));    // bottom wall
            // checkpoints spaced along the lane
            laneCheckpoints = new Vector2[] {
                new Vector2(300,  500),
                new Vector2(600,  480),
                new Vector2(900,  520),
                new Vector2(1200, 490),
                new Vector2(1500, 500)
            };
            testCheckpoints = laneCheckpoints;
            testCurrentCheckpoint = 0;
            testMessage = "Stage 4: Lane Keeping! Stay inside the lane markings!";
            testMessageTimer = 3f;
            break;

        case 4: // ── TEST COURSE ───────────────────────────────────────────
            testVehicle.Position = new Vector2(600, 800);
            stageTimeLimit = testingForClass switch {
                LicenceClass.D => 90f, LicenceClass.C => 80f, LicenceClass.B => 100f,
                LicenceClass.A => 75f, LicenceClass.S => 70f, _ => 90f
            };
            stageTimer = stageTimeLimit;
            testLap = 0;
            testLapsRequired = testingForClass switch {
                LicenceClass.D => 1, LicenceClass.C => 1, LicenceClass.B => 2,
                LicenceClass.A => 2, LicenceClass.S => 3, _ => 1
            };
            testCheckpoints = new Vector2[] {
                new Vector2(600,  500),
                new Vector2(1000, 300),
                new Vector2(1400, 500),
                new Vector2(1000, 900)
            };
            testCurrentCheckpoint = 0;
            testMessage = $"Stage 5: Test Course! Complete {testLapsRequired} lap(s)!";
            testMessageTimer = 3f;
            break;
    }
}

static List<Vector2> GenerateCones(LicenceClass cls)
{
    var cones = new List<Vector2>();

    // slalom cones - two staggered rows leading to the track
    float spacing = cls switch {
        LicenceClass.D => 120f, LicenceClass.C => 100f, LicenceClass.B => 90f,
        LicenceClass.A => 80f, LicenceClass.S => 70f, _ => 120f
    };
    for (int i = 0; i < 8; i++)
    {
        float x = 300 + i * spacing;
        float yOffset = (i % 2 == 0) ? -60f : 60f;
        cones.Add(new Vector2(x, 1800 + yOffset));
    }

    // cones marking track entry
    cones.Add(new Vector2(1340, 1720));
    cones.Add(new Vector2(1340, 1880));

    return cones;
}

static List<Rectangle> GenerateBarriers()
{
    var barriers = new List<Rectangle>();

    // barriers defining the track edges (outer and inner wall segments)
    // outer track boundary
    barriers.Add(new Rectangle(1400, 800, 20, 1200));   // right wall
    barriers.Add(new Rectangle(600, 800, 20, 20));       // top left corner
    barriers.Add(new Rectangle(600, 800, 1820, 20));     // top wall
    barriers.Add(new Rectangle(600, 1980, 820, 20));     // bottom wall

    // inner island (creates the loop)
    barriers.Add(new Rectangle(900, 1000, 700, 800));    // inner block

    return barriers;
}

static Vector2[] GenerateTestCourse(int count, LicenceClass cls)
{
    // cone slalom checkpoints (first half)
    int slalomCps = count / 2;
    int trackCps = count - slalomCps;
    var pts = new Vector2[count];

    // slalom section checkpoints down the cone lane
    float spacing = cls switch {
        LicenceClass.D => 120f, LicenceClass.C => 100f, LicenceClass.B => 90f,
        LicenceClass.A => 80f, LicenceClass.S => 70f, _ => 120f
    };
    for (int i = 0; i < slalomCps; i++)
        pts[i] = new Vector2(360 + i * spacing * 1.5f, 1800f);

    // timed track lap checkpoints (loop around the track)
    float[] trackAngles = { 0f, MathF.PI * 0.5f, MathF.PI, MathF.PI * 1.5f };
    for (int i = 0; i < trackCps; i++)
    {
        float angle = i * (MathF.PI * 2f / trackCps);
        pts[slalomCps + i] = new Vector2(
            1150 + MathF.Cos(angle) * 340f,
            1390 + MathF.Sin(angle) * 340f);
    }
    return pts;
}

static void ExitDrivingTest()
{
    testVehicle = null;
    player.Hidden = false;

    if (preTestScene == SceneState.Building)
    {
        if (isBoatTest) boatMenuOpen = true;   // ADDED
        else aaMenuOpen = true;
    }
    else
    {
        player.Position = testReturnPos;
    }

    testCones.Clear();
    testBarriers.Clear();
    testConesHit = 0;
    testConePenaltyTimer = 0f;
    testComplete = false;
    testFailed = false;
    isBoatTest = false;   // ADDED — reset for next test
    currentScene = preTestScene;
}

static void UpdateDrivingTest(float dt)
{
    if (testComplete || testFailed)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            ExitDrivingTest();
        return;
    }

    if (testMessageTimer > 0) testMessageTimer -= dt;
    if (testConePenaltyTimer > 0) testConePenaltyTimer -= dt;

    stageTimer -= dt;
    if (stageTimer <= 0f)
    {
        testFailed = true;
        testMessage = $"{testStageName} — Time's up! Test failed!";
        testMessageTimer = 4f;
        return;
    }

    // ── VEHICLE MOVEMENT ──────────────────────────────────────────────────
    Vector2 move = Vector2.Zero;
    if (Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
    if (Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;
    if (move != Vector2.Zero) move = Vector2.Normalize(move);
    testVehicle.velocity = Vector2.Lerp(testVehicle.velocity, move * testVehicle.TopSpeed, dt * 5f);

    Vector2 oldPos = testVehicle.Position;
    testVehicle.Position += testVehicle.velocity * dt;

    Rectangle vehicleRect = new Rectangle(
        testVehicle.Position.X, testVehicle.Position.Y, 50, 30);

    // ── FACING ────────────────────────────────────────────────────────────
    if (testVehicle.velocity.Length() > 20f)
    {
        if (MathF.Abs(testVehicle.velocity.X) >= MathF.Abs(testVehicle.velocity.Y))
            testVehicle.Facing = testVehicle.velocity.X < 0 ? Vehicle.FacingDirection.Left : Vehicle.FacingDirection.Right;
        else
            testVehicle.Facing = testVehicle.velocity.Y < 0 ? Vehicle.FacingDirection.Up : Vehicle.FacingDirection.Down;
    }

    // ── STAGE SPECIFIC LOGIC ──────────────────────────────────────────────
    if (isBoatTest)
    {
        switch (testStage)
        {
            case 0: // BUOY WEAVE
                if (testConePenaltyTimer <= 0)
                {
                    for (int i = testCones.Count - 1; i >= 0; i--)
                    {
                        if (Vector2.Distance(testVehicle.Position + new Vector2(25, 15), testCones[i]) < 24f)
                        {
                            testCones.RemoveAt(i);
                            testConesHit++;
                            testConePenaltyTimer = 1f;
                            if (testConesHit > maxConesAllowed)
                            {
                                testFailed = true;
                                testMessage = "Too many buoys hit! Test failed!";
                                testMessageTimer = 4f;
                                return;
                            }
                            stageTimer -= 10f;
                            testMessage = $"Buoy hit! -10s ({testConesHit}/{maxConesAllowed} allowed)";
                            testMessageTimer = 1.5f;
                            break;
                        }
                    }
                }
                if (testCurrentCheckpoint < testCheckpoints.Length &&
                    Vector2.Distance(testVehicle.Position + new Vector2(25, 15), testCheckpoints[testCurrentCheckpoint]) < 60f)
                {
                    stageComplete = true;
                    testMessage = "Buoy weave complete!";
                    testMessageTimer = 2f;
                    AdvanceStage();
                }
                break;

            case 1: // DOCKING
                foreach (Rectangle marker in parkingCars)
                {
                    if (Raylib.CheckCollisionRecs(vehicleRect, marker))
                    {
                        testFailed = true;
                        testMessage = "You hit a dock marker! Test failed!";
                        testMessageTimer = 4f;
                        return;
                    }
                }
                if (Raylib.CheckCollisionRecs(vehicleRect, parkingBay) && testVehicle.velocity.Length() < 10f)
                {
                    parkingTimer += dt;
                    if (parkingTimer >= 2f)
                    {
                        stageComplete = true;
                        testMessage = "Docked! Great job!";
                        testMessageTimer = 2f;
                        AdvanceStage();
                    }
                }
                else parkingTimer = 0f;
                break;

            case 2: // OPEN WATER CHECKPOINTS
                if (testCurrentCheckpoint < testCheckpoints.Length &&
                    Vector2.Distance(testVehicle.Position + new Vector2(25, 15), testCheckpoints[testCurrentCheckpoint]) < 60f)
                {
                    testCurrentCheckpoint++;
                    if (testCurrentCheckpoint >= testCheckpoints.Length)
                    {
                        stageComplete = true;
                        testMessage = "All checkpoints reached!";
                        testMessageTimer = 2f;
                        AdvanceStage();
                    }
                    else
                    {
                        testMessage = $"CP {testCurrentCheckpoint}/{testCheckpoints.Length}";
                        testMessageTimer = 1f;
                    }
                }
                break;
        }
    }
    else
    {
        switch (testStage)
        {
            case 0: // SLALOM
                if (testConePenaltyTimer <= 0)
                {
                    for (int i = testCones.Count - 1; i >= 0; i--)
                    {
                        if (Vector2.Distance(testVehicle.Position + new Vector2(25, 15), testCones[i]) < 24f)
                        {
                            testCones.RemoveAt(i);
                            testConesHit++;
                            testConePenaltyTimer = 1f;
                            if (testConesHit > maxConesAllowed)
                            {
                                testFailed = true;
                                testMessage = "Too many cones hit! Test failed!";
                                testMessageTimer = 4f;
                                return;
                            }
                            stageTimer -= 10f;
                            testMessage = $"Cone hit! -10s ({testConesHit}/{maxConesAllowed} allowed)";
                            testMessageTimer = 1.5f;
                            break;
                        }
                    }
                }
                if (testCurrentCheckpoint < testCheckpoints.Length)
                {
                    if (Vector2.Distance(testVehicle.Position + new Vector2(25, 15),
                        testCheckpoints[testCurrentCheckpoint]) < 60f)
                    {
                        stageComplete = true;
                        testMessage = "Slalom complete! Well done!";
                        testMessageTimer = 2f;
                        AdvanceStage();
                    }
                }
                break;

            case 1: // BAY PARKING
                foreach (Rectangle car in parkingCars)
                {
                    if (Raylib.CheckCollisionRecs(vehicleRect, car))
                    {
                        testFailed = true;
                        testMessage = "You hit a parked car! Test failed!";
                        testMessageTimer = 4f;
                        return;
                    }
                }
                if (Raylib.CheckCollisionRecs(vehicleRect, parkingBay) &&
                    testVehicle.velocity.Length() < 10f)
                {
                    parkingTimer += dt;
                    if (parkingTimer >= 2f)
                    {
                        stageComplete = true;
                        testMessage = "Parked! Great job!";
                        testMessageTimer = 2f;
                        AdvanceStage();
                    }
                }
                else
                {
                    parkingTimer = 0f;
                }
                break;

            case 2: // INTERSECTION
                for (int i = 0; i < trafficCars.Count; i++)
                {
                    trafficCars[i] = trafficCars[i] + trafficVelocities[i] * dt;
                    if (trafficCars[i].X > 1400) trafficCars[i] = new Vector2(-100, trafficCars[i].Y);
                    if (trafficCars[i].X < -100) trafficCars[i] = new Vector2(1400, trafficCars[i].Y);
                    if (trafficCars[i].Y > 1000) trafficCars[i] = new Vector2(trafficCars[i].X, -50);
                    if (trafficCars[i].Y < -50)  trafficCars[i] = new Vector2(trafficCars[i].X, 1000);

                    Rectangle trafficRect = new Rectangle(
                        trafficCars[i].X, trafficCars[i].Y, 50, 30);
                    if (Raylib.CheckCollisionRecs(vehicleRect, trafficRect))
                    {
                        testFailed = true;
                        testMessage = "You hit a traffic car! Test failed!";
                        testMessageTimer = 4f;
                        return;
                    }
                }
                if (Vector2.Distance(testVehicle.Position + new Vector2(25,15),
                    testCheckpoints[0]) < 80f)
                {
                    stageComplete = true;
                    testMessage = "Intersection cleared!";
                    testMessageTimer = 2f;
                    AdvanceStage();
                }
                break;

            case 3: // LANE KEEPING
                foreach (Rectangle boundary in laneBoundaries)
                {
                    if (Raylib.CheckCollisionRecs(vehicleRect, boundary))
                    {
                        testFailed = true;
                        testMessage = "Left the lane! Test failed!";
                        testMessageTimer = 4f;
                        return;
                    }
                }
                if (testCurrentCheckpoint < testCheckpoints.Length)
                {
                    if (Vector2.Distance(testVehicle.Position + new Vector2(25, 15),
                        testCheckpoints[testCurrentCheckpoint]) < 50f)
                    {
                        testCurrentCheckpoint++;
                        laneCheckpointsHit++;
                        testMessage = $"Checkpoint {laneCheckpointsHit}/{laneCheckpointsRequired}!";
                        testMessageTimer = 1f;
                        if (laneCheckpointsHit >= laneCheckpointsRequired)
                        {
                            stageComplete = true;
                            testMessage = "Lane keeping complete!";
                            testMessageTimer = 2f;
                            AdvanceStage();
                        }
                    }
                }
                break;

            case 4: // TEST COURSE
                if (testCurrentCheckpoint < testCheckpoints.Length)
                {
                    if (Vector2.Distance(testVehicle.Position + new Vector2(25, 15),
                        testCheckpoints[testCurrentCheckpoint]) < 60f)
                    {
                        testCurrentCheckpoint++;
                        if (testCurrentCheckpoint >= testCheckpoints.Length)
                        {
                            testLap++;
                            if (testLap >= testLapsRequired)
                            {
                                testComplete = true;
                                testMessage = $"Test Passed! Class {testingForClass} licence earned!";
                                testMessageTimer = 4f;
                                GrantLicence();
                            }
                            else
                            {
                                testCurrentCheckpoint = 0;
                                testMessage = $"Lap {testLap}/{testLapsRequired} done! Keep going!";
                                testMessageTimer = 2f;
                            }
                        }
                        else
                        {
                            testMessage = $"CP {testCurrentCheckpoint}/{testCheckpoints.Length}";
                            testMessageTimer = 1f;
                        }
                    }
                }
                break;
        }
    }
    testCamera.Target = testVehicle.Position;
}

static void AdvanceStage()
{
    int next = testStage + 1;
    int stageCount = isBoatTest ? boatStageCount : testStageCount;

    if (next >= stageCount)
    {
        testComplete = true;
        if (isBoatTest)
        {
            testMessage = $"All stages complete! {boatTierNames[boatTestingTier]} licence earned!";
            testMessageTimer = 4f;
            GrantBoatLicence();
        }
        else
        {
            testMessage = $"All stages complete! Class {testingForClass} licence earned!";
            testMessageTimer = 4f;
            GrantLicence();
        }
    }
    else
    {
        testMessage = $"Stage {testStage + 1} complete! Loading next stage...";
        testMessageTimer = 2f;
        if (isBoatTest) LoadBoatStage(next);
        else LoadStage(next);
    }
}

static void GrantLicence()
{
    switch (testingForClass) {
        case LicenceClass.D: hasPracticalD = true; break;
        case LicenceClass.C: hasPracticalC = true; break;
        case LicenceClass.B: hasPracticalB = true; break;
        case LicenceClass.A: hasPracticalA = true; break;
        case LicenceClass.S: hasPracticalS = true; break;
    }
    player.AddDrivingXP(200);

    licencePendingClass = testingForClass;
    licencePendingIsTheory[(int)testingForClass - 1] = false;   // ADDED — this was missing
    licenceCongratsText = $"CONGRATULATIONS! You passed your Class {testingForClass} PRACTICAL test!";
    licenceCongratsOpen = true;
}

static void GrantBoatLicence()
{
    hasBoatPractical[boatTestingTier] = true;
    player.AddBoatingXP(200);

    licenceCongratsText = $"CONGRATULATIONS! You earned your {boatTierNames[boatTestingTier]} licence!";
    licenceCongratsOpen = true;
}

static void UpdateUnderwater(float dt)
{
    float swimSpeed = 320f;
    Vector2 move = Vector2.Zero;
    if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

    if (move != Vector2.Zero)
    {
        move = Vector2.Normalize(move);
        if (MathF.Abs(move.X) >= MathF.Abs(move.Y))
            underwaterFacing = move.X < 0 ? Player.FacingDirection.Left : Player.FacingDirection.Right;
        else
            underwaterFacing = move.Y < 0 ? Player.FacingDirection.Up : Player.FacingDirection.Down;
    }

    underwaterPos += move * swimSpeed * dt;

    // clamp to world bounds
    underwaterPos.X = Math.Clamp(underwaterPos.X, 40f, UnderwaterSize - 40f);
    underwaterPos.Y = Math.Clamp(underwaterPos.Y, 40f, UnderwaterSize - 40f);

    // exit back to World with J
    if (Raylib.IsKeyPressed(KeyboardKey.J))
    {
        player.Position = underwaterWorldReturnPos;
        ChangeScene(SceneState.World);
        lastZoneMusic = default;
        CheckZoneMusic();
        return;
    }

    UpdateCollectables("Underwater", underwaterPos);

    // camera follows player
    underwaterCamera.Target = underwaterPos;
    underwaterCamera.Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
    underwaterCamera.Zoom = 1f;
}

static void EnterSpace()
{
    spaceWorldReturnPos = player.Position;
    spacePos = new Vector2(SpaceSize / 2f, SpaceSize / 2f);
    spaceVel = Vector2.Zero;
    spaceAngle = 0f;
    ChangeScene(SceneState.Space);
}

static void UpdateSpace(float dt)
{
    float turnSpeed = 3.0f;      // radians per second
    float thrustPower = 400f;    // acceleration
    float drag = 0.4f;           // very light drag so you keep drifting
    float maxSpeed = 900f;

    // rotate ship (A/D or Left/Right)
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  spaceAngle -= turnSpeed * dt;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) spaceAngle += turnSpeed * dt;

    // thrust forward (W/Up) in the direction the ship faces
    bool thrusting = Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up);
    if (thrusting)
    {
        Vector2 forward = new Vector2(MathF.Cos(spaceAngle), MathF.Sin(spaceAngle));
        spaceVel += forward * thrustPower * dt;
    }

    // optional reverse/brake (S/Down)
    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
        spaceVel -= spaceVel * 1.5f * dt;

    // light drag so it slowly settles but mostly drifts
    spaceVel -= spaceVel * drag * dt;

    // clamp top speed
    if (spaceVel.Length() > maxSpeed)
        spaceVel = Vector2.Normalize(spaceVel) * maxSpeed;

    spacePos += spaceVel * dt;

    // clamp to bounds (bounce gently off edges)
    if (spacePos.X < 40f)            { spacePos.X = 40f;            spaceVel.X *= -0.4f; }
    if (spacePos.X > SpaceSize - 40f){ spacePos.X = SpaceSize - 40f;spaceVel.X *= -0.4f; }
    if (spacePos.Y < 40f)            { spacePos.Y = 40f;            spaceVel.Y *= -0.4f; }
    if (spacePos.Y > SpaceSize - 40f){ spacePos.Y = SpaceSize - 40f;spaceVel.Y *= -0.4f; }

    // exit back to World
    if (Raylib.IsKeyPressed(KeyboardKey.J))
    {
        player.Position = spaceWorldReturnPos;
        ChangeScene(SceneState.World);
        lastZoneMusic = default;
        CheckZoneMusic();
        return;
    }

    UpdateCollectables("Space", spacePos, 90f);

    spaceCamera.Target = spacePos;
    spaceCamera.Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
    spaceCamera.Zoom = 1f;
}

static void StartDive()
{
    diveWorldReturnPos = player.Position;
    diveSwimPos = new Vector2(640, 120);   // start just under the surface
    ChangeScene(SceneState.Dive);
}

static void UpdateDive(float dt)
{
    float swimSpeed = 280f;
    Vector2 move = Vector2.Zero;
    if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

    // face left or right only (side-on swimmer)
    if (move.X < 0) diveFacing = Player.FacingDirection.Left;
    else if (move.X > 0) diveFacing = Player.FacingDirection.Right;

    if (move != Vector2.Zero) move = Vector2.Normalize(move);
    diveSwimPos += move * swimSpeed * dt;

    // keep within horizontal bounds
    diveSwimPos.X = Math.Clamp(diveSwimPos.X, 40f, 1240f);

    // touch the surface → exit back to World
    if (diveSwimPos.Y <= diveSurfaceY)
    {
        player.Position = diveWorldReturnPos;
        ChangeScene(SceneState.World);
        lastZoneMusic = default;
        CheckZoneMusic();
        return;
    }

    // reach the bottom → enter the underwater world
    if (diveSwimPos.Y >= diveBottomY)
    {
        EnterUnderwater();
        return;
    }

    // camera follows vertically, locked horizontally to screen centre
    diveCamera.Target = new Vector2(640, diveSwimPos.Y);
    diveCamera.Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
    diveCamera.Zoom = 1f;
}

static void EnterUnderwater()
{
    underwaterWorldReturnPos = diveWorldReturnPos;   // return to where the dive started
    underwaterPos = new Vector2(UnderwaterSize / 2f, UnderwaterSize / 2f);
    ChangeScene(SceneState.Underwater);
}

static void DrawDive()
{
    Raylib.BeginMode2D(diveCamera);

    // water column — gets darker with depth
    for (int band = 0; band < 40; band++)
    {
        int by = (int)(diveSurfaceY + band * 100);
        byte shade = (byte)Math.Max(20, 130 - band * 3);
        Raylib.DrawRectangle(-200, by, 1700, 100, new Color((byte)10,(byte)(shade/2),(byte)shade,(byte)255));
    }

    // surface line
    Raylib.DrawRectangle(-200, (int)diveSurfaceY - 6, 1700, 6, new Color((byte)180,(byte)230,(byte)255,(byte)200));
    Program.DrawTextUI("SURFACE — swim up to exit", 420, (int)diveSurfaceY - 30, 20, Color.White);

    // sea bed marker near the bottom
    Raylib.DrawRectangle(-200, (int)diveBottomY, 1700, 200, new Color((byte)40,(byte)35,(byte)20,(byte)255));
    Program.DrawTextUI("Swim down to the depths...", 420, (int)diveBottomY - 30, 20, Color.White);

    // bubbles for ambience
    float t = (float)Raylib.GetTime();
    for (int i = 0; i < 12; i++)
    {
        float bx = 200 + i * 90;
        float by = diveBottomY - ((t * 60 + i * 200) % diveBottomY);
        Raylib.DrawCircle((int)bx, (int)by, 3, new Color((byte)200,(byte)230,(byte)255,(byte)90));
    }

    // the swimmer — simple side-on figure facing left/right
    int px = (int)diveSwimPos.X, py = (int)diveSwimPos.Y;
    bool faceLeft = diveFacing == Player.FacingDirection.Left;
    Raylib.DrawCircle(px + (faceLeft ? -8 : 8), py, 9, player.SkinColor);    // head
    Raylib.DrawRectangle(px - 6, py - 4, 12, 20, player.ShirtColor);          // body
    Raylib.DrawRectangle(px - 4, py + 16, 12, 8, player.PantsColor);          // legs/fins

    Raylib.EndMode2D();

    // HUD
    Program.DrawTextUI("DIVE — W/S/A/D to swim", 10, 10, 20, Color.White);
    float depthPct = (diveSwimPos.Y - diveSurfaceY) / (diveBottomY - diveSurfaceY);
    Raylib.DrawRectangle(10, 40, 200, 14, new Color((byte)20,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangle(10, 40, (int)(200 * Math.Clamp(depthPct, 0f, 1f)), 14, new Color((byte)60,(byte)160,(byte)220,(byte)255));
    Program.DrawTextUI("Depth", 220, 38, 16, Color.LightGray);
}

static void DrawUnderwater()
{
    Raylib.ClearBackground(new Color((byte)10,(byte)40,(byte)80,(byte)255));

    Raylib.BeginMode2D(underwaterCamera);

    

    // faint grid so movement is visible in the bare world
    int grid = 1000;
    Color gridCol = new Color((byte)20,(byte)60,(byte)110,(byte)255);
    for (int gx = 0; gx <= UnderwaterSize; gx += grid)
        Raylib.DrawLine(gx, 0, gx, UnderwaterSize, gridCol);
    for (int gy = 0; gy <= UnderwaterSize; gy += grid)
        Raylib.DrawLine(0, gy, UnderwaterSize, gy, gridCol);

    // boundary walls
    int t = 40;
    Color wall = new Color((byte)30,(byte)25,(byte)15,(byte)255);
    Raylib.DrawRectangle(0, 0, UnderwaterSize, t, wall);                       // top
    Raylib.DrawRectangle(0, UnderwaterSize - t, UnderwaterSize, t, wall);      // bottom
    Raylib.DrawRectangle(0, 0, t, UnderwaterSize, wall);                       // left
    Raylib.DrawRectangle(UnderwaterSize - t, 0, t, UnderwaterSize, wall);      // right

    // drifting bubbles for ambience
    float time = (float)Raylib.GetTime();
    for (int i = 0; i < 30; i++)
    {
        float bx = (i * 670) % UnderwaterSize;
        float by = UnderwaterSize - ((time * 50 + i * 400) % UnderwaterSize);
        Raylib.DrawCircle((int)bx, (int)by, 4, new Color((byte)120,(byte)180,(byte)220,(byte)60));
    }

    DrawCollectables("Underwater");

    // the player — simple swimmer facing 4 ways
    int px = (int)underwaterPos.X, py = (int)underwaterPos.Y;
    Raylib.DrawCircle(px, py - 6, 10, player.SkinColor);              // head
    Raylib.DrawRectangle(px - 8, py + 2, 16, 22, player.ShirtColor);  // body
    Raylib.DrawRectangle(px - 6, py + 24, 12, 8, player.PantsColor);  // legs
    // facing indicator
    Vector2 look = underwaterFacing switch
    {
        Player.FacingDirection.Up    => new Vector2(0, -14),
        Player.FacingDirection.Down  => new Vector2(0, 14),
        Player.FacingDirection.Left  => new Vector2(-14, 0),
        _                            => new Vector2(14, 0)
    };
    Raylib.DrawCircle(px + (int)look.X, py + (int)look.Y, 3, Color.White);

    Raylib.EndMode2D();

    // HUD
    Program.DrawTextUI("UNDERWATER — WASD to swim  |  J = surface to world", 10, 10, 20, Color.White);
    Program.DrawTextUI($"Pos: {(int)underwaterPos.X}, {(int)underwaterPos.Y}", 10, 36, 16, Color.LightGray);
}

static void DrawSpace()
{
    Raylib.ClearBackground(new Color((byte)5,(byte)5,(byte)20,(byte)255));

    Raylib.BeginMode2D(spaceCamera);

    // starfield — deterministic scatter so stars stay put
    var rng = new Random(1234);
    for (int i = 0; i < 600; i++)
    {
        int sx = rng.Next(0, SpaceSize);
        int sy = rng.Next(0, SpaceSize);
        int size = rng.Next(1, 3);
        byte b = (byte)rng.Next(120, 255);
        Raylib.DrawRectangle(sx, sy, size, size, new Color(b, b, (byte)255, (byte)255));
    }

    // boundary walls (nebula edge)
    int t = 40;
    Color edge = new Color((byte)60,(byte)20,(byte)80,(byte)255);
    Raylib.DrawRectangle(0, 0, SpaceSize, t, edge);
    Raylib.DrawRectangle(0, SpaceSize - t, SpaceSize, t, edge);
    Raylib.DrawRectangle(0, 0, t, SpaceSize, edge);
    Raylib.DrawRectangle(SpaceSize - t, 0, t, SpaceSize, edge);

    DrawCollectables("Space");

    // the ship — triangle pointing along spaceAngle
    float px = spacePos.X, py = spacePos.Y;
    Vector2 forward = new Vector2(MathF.Cos(spaceAngle), MathF.Sin(spaceAngle));
    Vector2 right   = new Vector2(-forward.Y, forward.X);
    Vector2 nose  = new Vector2(px, py) + forward * 18f;
    Vector2 tailL = new Vector2(px, py) - forward * 12f + right * 10f;
    Vector2 tailR = new Vector2(px, py) - forward * 12f - right * 10f;

    // thrust flame when accelerating
    if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))
    {
        Vector2 flame = new Vector2(px, py) - forward * (20f + (float)(Raylib.GetTime() * 60 % 12));
        Raylib.DrawTriangle(tailR, tailL, flame, new Color((byte)255,(byte)160,(byte)40,(byte)255));
    }

    Raylib.DrawTriangle(nose, tailL, tailR, new Color((byte)200,(byte)210,(byte)230,(byte)255));
    Raylib.DrawTriangleLines(nose, tailL, tailR, Color.White);

    Raylib.EndMode2D();

    // HUD
    Program.DrawTextUI("SPACE — W thrust, A/D rotate, S brake  |  J = return", 10, 10, 20, Color.White);
    Program.DrawTextUI($"Speed: {(int)spaceVel.Length()}", 10, 36, 16, Color.LightGray);
    Program.DrawTextUI($"Pos: {(int)spacePos.X}, {(int)spacePos.Y}", 10, 56, 16, Color.LightGray);
}

static void AssignLicenceMailbox(int houseIndex)
{
    int idx = (int)licencePendingClass - 1;
    if (idx < 0) return;
    licenceTargetHouse[idx] = ownedHousePlots.Count > 0 ? houseIndex : -1;
    licencePending[idx] = true;
     licenceDeliveryTimer[idx] = 5f * daySpeed;//licenceDeliveryTimer[idx] = 1f;  
    licenceMailMenuOpen = false;
    licencePendingClass = LicenceClass.None;
    ShowNotification("Licence paperwork lodged! Arrives by mail in 24 hours.");
}

static void DrawDrivingTest()
{
    Raylib.BeginMode2D(testCamera);

    if (isBoatTest)
    {
        switch (testStage)
        {
            case 0: // BUOY WEAVE — open water
                Raylib.ClearBackground(new Color((byte)30,(byte)100,(byte)180,(byte)255));
                for (int i = 0; i < 20; i++)
                    Raylib.DrawCircleLines(-100 + i * 100, 500 + (i % 3) * 20, 30, new Color((byte)255,(byte)255,(byte)255,(byte)30));
                break;

            case 1: // DOCKING — water with a wooden pier
                Raylib.ClearBackground(new Color((byte)30,(byte)100,(byte)180,(byte)255));
                Raylib.DrawRectangle(500, 460, 400, 160, new Color((byte)140,(byte)100,(byte)60,(byte)255));
                for (int p = 0; p < 6; p++)
                    Raylib.DrawRectangle(520 + p * 65, 460, 8, 160, new Color((byte)100,(byte)70,(byte)40,(byte)255));
                Raylib.DrawRectangle((int)parkingBay.X, (int)parkingBay.Y, (int)parkingBay.Width, (int)parkingBay.Height, new Color((byte)255,(byte)215,(byte)0,(byte)40));
                Raylib.DrawRectangleLines((int)parkingBay.X, (int)parkingBay.Y, (int)parkingBay.Width, (int)parkingBay.Height, new Color((byte)255,(byte)215,(byte)0,(byte)200));
                Program.DrawTextUI("DOCK HERE", (int)parkingBay.X + 8, (int)parkingBay.Y + 12, 14, Color.Gold);
                foreach (Rectangle marker in parkingCars)
                {
                    Raylib.DrawRectangle((int)marker.X, (int)marker.Y, (int)marker.Width, (int)marker.Height, new Color((byte)90,(byte)60,(byte)30,(byte)255));
                    Raylib.DrawRectangleLines((int)marker.X, (int)marker.Y, (int)marker.Width, (int)marker.Height, new Color((byte)120,(byte)90,(byte)50,(byte)255));
                }
                break;

            case 2: // OPEN WATER CHECKPOINTS
                Raylib.ClearBackground(new Color((byte)30,(byte)100,(byte)180,(byte)255));
                for (int i = 0; i < testCheckpoints.Length; i++)
                {
                    bool done = i < testCurrentCheckpoint;
                    bool next = i == testCurrentCheckpoint;
                    Color cpCol = done ? Color.Green : next ? Color.Gold : new Color((byte)60,(byte)80,(byte)160,(byte)255);
                    Raylib.DrawCircle((int)testCheckpoints[i].X, (int)testCheckpoints[i].Y, next ? 24 : 18, cpCol);
                    Raylib.DrawCircleLines((int)testCheckpoints[i].X, (int)testCheckpoints[i].Y, next ? 24 : 18, Color.White);
                    Program.DrawTextUI($"{i + 1}", (int)testCheckpoints[i].X - 5, (int)testCheckpoints[i].Y - 9, 18, Color.White);
                    if (next && i + 1 < testCheckpoints.Length)
                        Raylib.DrawLineEx(testCheckpoints[i], testCheckpoints[i + 1], 2f, new Color((byte)255,(byte)200,(byte)0,(byte)80));
                }
                break;
        }
    }
    else
    {
        switch (testStage)
        {
            case 0: // SLALOM — straight road
                Raylib.ClearBackground(new Color((byte)80,(byte)120,(byte)50,(byte)255));
                Raylib.DrawRectangle(-200, 430, 2000, 140, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                for (int i = 0; i < 16; i++)
                    Raylib.DrawRectangle(-100 + i * 120, 495, 60, 8, new Color((byte)255,(byte)255,(byte)255,(byte)140));
                Raylib.DrawRectangle(-200, 428, 2000, 4, Color.White);
                Raylib.DrawRectangle(-200, 568, 2000, 4, Color.White);
                Raylib.DrawRectangle(1440, 420, 20, 40, new Color((byte)220,(byte)50,(byte)50,(byte)255));
                Raylib.DrawRectangle(1440, 540, 20, 40, new Color((byte)220,(byte)50,(byte)50,(byte)255));
                Program.DrawTextUI("FINISH", 1420, 400, 18, Color.White);
                break;

            case 1: // PARKING — car park
                Raylib.ClearBackground(new Color((byte)70,(byte)70,(byte)75,(byte)255));
                Raylib.DrawRectangle(200, 200, 900, 400, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                for (int i = 0; i < 6; i++)
                    Raylib.DrawRectangle(560 + i * 80, 490, 3, 100, Color.White);
                Raylib.DrawRectangle(560, 588, 400, 3, Color.White);
                Raylib.DrawRectangle((int)parkingBay.X, (int)parkingBay.Y, (int)parkingBay.Width, (int)parkingBay.Height, new Color((byte)255,(byte)215,(byte)0,(byte)40));
                Raylib.DrawRectangleLines((int)parkingBay.X, (int)parkingBay.Y, (int)parkingBay.Width, (int)parkingBay.Height, new Color((byte)255,(byte)215,(byte)0,(byte)200));
                Program.DrawTextUI("PARK HERE", (int)parkingBay.X + 8, (int)parkingBay.Y + 12, 14, Color.Gold);
                foreach (Rectangle car in parkingCars)
                {
                    Raylib.DrawRectangle((int)car.X, (int)car.Y, (int)car.Width, (int)car.Height, new Color((byte)80,(byte)100,(byte)160,(byte)255));
                    Raylib.DrawRectangleLines((int)car.X, (int)car.Y, (int)car.Width, (int)car.Height, new Color((byte)120,(byte)140,(byte)200,(byte)255));
                }
                break;

            case 2: // INTERSECTION
                Raylib.ClearBackground(new Color((byte)80,(byte)120,(byte)50,(byte)255));
                Raylib.DrawRectangle(-200, 460, 1800, 100, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                Raylib.DrawRectangle(620, -200, 100, 1400, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                for (int i = 0; i < 10; i++)
                    Raylib.DrawRectangle(-100 + i * 120, 507, 60, 6, new Color((byte)255,(byte)255,(byte)0,(byte)140));
                for (int i = 0; i < 10; i++)
                    Raylib.DrawRectangle(667, -100 + i * 120, 6, 60, new Color((byte)255,(byte)255,(byte)0,(byte)140));
                Raylib.DrawRectangle(-200, 458, 1800, 4, Color.White);
                Raylib.DrawRectangle(-200, 558, 1800, 4, Color.White);
                Raylib.DrawRectangle(618, -200, 4, 1400, Color.White);
                Raylib.DrawRectangle(718, -200, 4, 1400, Color.White);
                Raylib.DrawCircle(1200, 500, 30, new Color((byte)0,(byte)200,(byte)100,(byte)100));
                Raylib.DrawCircleLines(1200, 500, 30, Color.Green);
                Program.DrawTextUI("GOAL", 1175, 490, 16, Color.Green);
                for (int i = 0; i < trafficCars.Count; i++)
                {
                    Color tc = i % 2 == 0 ? new Color((byte)200,(byte)50,(byte)50,(byte)255) : new Color((byte)50,(byte)160,(byte)80,(byte)255);
                    Raylib.DrawRectangle((int)trafficCars[i].X, (int)trafficCars[i].Y, 50, 30, tc);
                    Raylib.DrawRectangleLines((int)trafficCars[i].X, (int)trafficCars[i].Y, 50, 30, Color.White);
                }
                break;

            case 3: // LANE KEEPING
                Raylib.ClearBackground(new Color((byte)80,(byte)120,(byte)50,(byte)255));
                Raylib.DrawRectangle(-200, 420, 2000, 160, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                Raylib.DrawRectangle(-200, 420, 2000, 20, new Color((byte)220,(byte)50,(byte)50,(byte)255));
                Raylib.DrawRectangle(-200, 560, 2000, 20, new Color((byte)220,(byte)50,(byte)50,(byte)255));
                for (int i = 0; i < 16; i++)
                    Raylib.DrawRectangle(-100 + i * 120, 495, 60, 8, new Color((byte)255,(byte)255,(byte)0,(byte)140));
                for (int i = 0; i < laneCheckpoints.Length; i++)
                {
                    bool done = i < testCurrentCheckpoint;
                    bool next = i == testCurrentCheckpoint;
                    Color cpCol = done ? Color.Green : next ? Color.Gold : new Color((byte)60,(byte)80,(byte)160,(byte)255);
                    Raylib.DrawCircle((int)laneCheckpoints[i].X, (int)laneCheckpoints[i].Y, next ? 22 : 16, cpCol);
                    Raylib.DrawCircleLines((int)laneCheckpoints[i].X, (int)laneCheckpoints[i].Y, next ? 22 : 16, Color.White);
                    Program.DrawTextUI($"{i + 1}", (int)laneCheckpoints[i].X - 5, (int)laneCheckpoints[i].Y - 9, 18, Color.White);
                }
                break;

            case 4: // TEST COURSE
                Raylib.ClearBackground(new Color((byte)80,(byte)120,(byte)50,(byte)255));
                Raylib.DrawRectangle(300, 200, 1000, 800, new Color((byte)55,(byte)55,(byte)60,(byte)255));
                Raylib.DrawRectangle(500, 380, 600, 440, new Color((byte)60,(byte)100,(byte)40,(byte)255));
                Raylib.DrawRectangleLines(500, 380, 600, 440, new Color((byte)200,(byte)200,(byte)60,(byte)255));
                for (int i = 0; i < 16; i++)
                {
                    float a = i * (MathF.PI * 2f / 16f);
                    int mx = (int)(800 + MathF.Cos(a) * 380f);
                    int my = (int)(600 + MathF.Sin(a) * 300f);
                    Raylib.DrawRectangle(mx - 4, my - 10, 8, 20, new Color((byte)255,(byte)255,(byte)255,(byte)140));
                }
                for (int i = 0; i < 6; i++)
                {
                    Color chk = i % 2 == 0 ? Color.White : Color.Black;
                    Raylib.DrawRectangle(300 + i * 14, 740, 14, 80, chk);
                }
                Program.DrawTextUI("START/FINISH", 290, 720, 16, Color.White);
                for (int i = 0; i < testCheckpoints.Length; i++)
                {
                    bool done = i < testCurrentCheckpoint;
                    bool next = i == testCurrentCheckpoint;
                    Color cpCol = done ? Color.Green : next ? Color.Gold : new Color((byte)60,(byte)80,(byte)160,(byte)255);
                    Raylib.DrawCircle((int)testCheckpoints[i].X, (int)testCheckpoints[i].Y, next ? 24 : 18, cpCol);
                    Raylib.DrawCircleLines((int)testCheckpoints[i].X, (int)testCheckpoints[i].Y, next ? 24 : 18, Color.White);
                    Program.DrawTextUI($"{i + 1}", (int)testCheckpoints[i].X - 5, (int)testCheckpoints[i].Y - 9, 18, Color.White);
                    if (next && i + 1 < testCheckpoints.Length)
                        Raylib.DrawLineEx(testCheckpoints[i], testCheckpoints[i + 1], 2f, new Color((byte)255,(byte)200,(byte)0,(byte)80));
                }
                break;
        }
    }

    foreach (Vector2 marker in testCones)
    {
        if (isBoatTest)
        {
            Raylib.DrawCircle((int)marker.X, (int)marker.Y, 10, new Color((byte)230,(byte)60,(byte)60,(byte)255));
            Raylib.DrawCircleLines((int)marker.X, (int)marker.Y, 10, Color.White);
            Raylib.DrawRectangle((int)marker.X - 2, (int)marker.Y + 8, 4, 14, new Color((byte)180,(byte)180,(byte)190,(byte)150));
        }
        else
        {
            Raylib.DrawRectangle((int)marker.X - 8, (int)marker.Y - 18, 16, 20, new Color((byte)240,(byte)100,(byte)20,(byte)255));
            Raylib.DrawRectangle((int)marker.X - 8, (int)marker.Y - 8, 16, 5, new Color((byte)255,(byte)255,(byte)255,(byte)220));
            Raylib.DrawRectangle((int)marker.X - 12, (int)marker.Y + 2, 24, 6, new Color((byte)200,(byte)80,(byte)10,(byte)255));
        }
    }

    if (testVehicle != null) testVehicle.Draw();

    Raylib.EndMode2D();

    Raylib.DrawRectangle(0, 0, ScreenWidth, 54, new Color((byte)5,(byte)15,(byte)30,(byte)220));

    int stageCount = isBoatTest ? boatStageCount : testStageCount;
    for (int i = 0; i < stageCount; i++)
    {
        Color dot = i < testStage ? Color.Green : i == testStage ? Color.Gold : Color.DarkGray;
        Raylib.DrawCircle(ScreenWidth / 2 - 80 + i * 40, 42, 8, dot);
        Raylib.DrawCircleLines(ScreenWidth / 2 - 80 + i * 40, 42, 8, Color.White);
        Program.DrawTextUI($"{i + 1}", ScreenWidth / 2 - 83 + i * 40, 36, 12, i == testStage ? Color.Black : Color.White);
    }

    float timerPct = stageTimer / stageTimeLimit;
    Color timerCol = timerPct > 0.5f ? Color.Green : timerPct > 0.25f ? Color.Orange : Color.Red;
    Raylib.DrawRectangle(200, 8, 700, 18, new Color((byte)30,(byte)30,(byte)40,(byte)255));
    Raylib.DrawRectangle(200, 8, (int)(700 * timerPct), 18, timerCol);
    Raylib.DrawRectangleLines(200, 8, 700, 18, Color.White);
    Program.DrawTextUI($"{(int)stageTimer}s", 910, 6, 20, timerCol);

    Program.DrawTextUI(testStageName, 10, 8, 20, new Color((byte)0,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI("Arrow keys = Drive", 10, 32, 14, Color.DarkGray);

    if (testConePenaltyTimer > 0)
        Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)255,(byte)80,(byte)0,(byte)40));

    if (testMessageTimer > 0)
    {
        byte alpha = (byte)(255 * Math.Min(1f, testMessageTimer));
        int mw = Program.MeasureTextUI(testMessage, 28);
        Raylib.DrawRectangle(ScreenWidth / 2 - mw / 2 - 16, 280, mw + 32, 48, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI(testMessage, ScreenWidth / 2 - mw / 2, 288, 28, new Color((byte)255,(byte)215,(byte)0, alpha));
    }

    if (testComplete || testFailed)
    {
        Raylib.DrawRectangle(ScreenWidth / 2 - 280, 380, 560, 90, new Color((byte)0,(byte)0,(byte)0,(byte)220));
        Raylib.DrawRectangleLines(ScreenWidth / 2 - 280, 380, 560, 90, testComplete ? Color.Green : Color.Red);
        Program.DrawTextUI(
            testComplete ? "ALL STAGES PASSED!  SPACE = Continue" : "FAILED.  SPACE = Try Again",
            ScreenWidth / 2 - 260, 410, 22,
            testComplete ? Color.Green : Color.Red);
    }
}

static void DrawLicenceMailMenu()
{
if (licencePendingClass == LicenceClass.None) { licenceMailMenuOpen = false; return; }
    int pw = 460, ph = 340, px = ScreenWidth/2 - pw/2, py = ScreenHeight/2 - ph/2;
    Raylib.DrawRectangle(0,0,ScreenWidth,ScreenHeight,Color.Black);
    Raylib.DrawRectangle(px,py,pw,ph,new Color((byte)24,(byte)24,(byte)34,(byte)245));
    Raylib.DrawRectangleLines(px,py,pw,ph,Color.Gold);
    Program.DrawTextUI("DELIVER LICENCE TO:", px+20, py+18, 24, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();

    int rows = ownedHousePlots.Count + 1;   // was: Math.Max(1, ownedHousePlots.Count)
    for (int i = 0; i < rows; i++)
    {
        int ry = py + 70 + i * 54;
        Rectangle r = new Rectangle(px+20, ry, pw-40, 46);
        bool hov = Raylib.CheckCollisionPointRec(mouse, r);
        Raylib.DrawRectangleRec(r, hov ? new Color((byte)60,(byte)60,(byte)80,(byte)255)
                                    : new Color((byte)40,(byte)40,(byte)55,(byte)255));
        Raylib.DrawRectangleLinesEx(r, 2, hov ? Color.Gold : Color.DarkGray);

        string label = i == 0 ? "Default House" : $"House {i}  ({ownedHousePlots[i-1].x}, {ownedHousePlots[i-1].y})";
        Program.DrawTextUI(label, px+34, ry+12, 20, Color.White);

        if (hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            int idx = (int)licencePendingClass - 1;
            licenceTargetHouse[idx] = i == 0 ? -1 : i - 1;   // -1 = default house, else owned-plot index
            licencePending[idx] = true;
            licenceDeliveryTimer[idx] = 5f * daySpeed;
            licenceMailMenuOpen = false;
            licencePendingClass = LicenceClass.None;
            aaMenuOpen = true;
            ShowNotification("Licence paperwork lodged! Arrives by mail in 24 hours.");
        }
    }
}

static void AddBoatLicenceOffice(float x, float y)
{
    var office = new Building(
        new Rectangle(x, y, 160, 120),
        new Color(20, 90, 140, 255),
        new Color(200, 220, 230, 255),
        new Vector2(x + 80, y + 100),
        "BOAT LICENCE OFFICE",
        new NPC(new Vector2(600, 120), "Harbourmaster", "Pass your boat theory once, then work up through the practicals."),
        entryPos: new Vector2(640, 880)
    );
    office.InteriorObjects.Clear();
    office.InteriorObjects.Add(new Rectangle(400, 80, 400, 40));    // counter
    office.InteriorObjects.Add(new Rectangle(100, 200, 200, 100));  // model boat display
    office.InteriorObjects.Add(new Rectangle(850, 200, 200, 100));
    buildings.Add(office);
}

static void DrawBoatLicenceExterior(float x, float y)
{
    int bx = (int)x, by = (int)y;
    Color hullBlue = new Color((byte)20,(byte)90,(byte)140,(byte)255);
    Color trim = new Color((byte)230,(byte)220,(byte)200,(byte)255);

    Raylib.DrawRectangle(bx, by, 160, 120, hullBlue);
    Raylib.DrawRectangle(bx - 5, by - 14, 170, 16, trim);
    Raylib.DrawRectangle(bx - 5, by - 14, 170, 4, new Color((byte)250,(byte)245,(byte)235,(byte)255));

    // anchor emblem
    Raylib.DrawCircleLines(bx + 80, by + 25, 12, trim);
    Raylib.DrawRectangle(bx + 78, by + 12, 4, 26, trim);
    Raylib.DrawLine(bx + 65, by + 20, bx + 95, by + 20, trim);

    // windows
    Raylib.DrawRectangle(bx + 10, by + 55, 45, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines(bx + 10, by + 55, 45, 45, trim);
    Raylib.DrawRectangle(bx + 105, by + 55, 45, 45, new Color((byte)160,(byte)200,(byte)220,(byte)200));
    Raylib.DrawRectangleLines(bx + 105, by + 55, 45, 45, trim);

    // door
    Raylib.DrawRectangle(bx + 62, by + 78, 36, 42, new Color((byte)90,(byte)65,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(bx + 62, by + 78, 36, 42, trim);

    Raylib.DrawRectangle(bx + 15, by - 34, 130, 18, new Color((byte)10,(byte)40,(byte)65,(byte)220));
    Program.DrawTextUI("BOAT LICENCE OFFICE", bx + 20, by - 31, 11, trim);

    // dock posts leading up to the entrance
    for (int p = -2; p <= 2; p++)
        Raylib.DrawRectangle(bx + 80 + p * 30, by + 130, 6, 20, new Color((byte)110,(byte)80,(byte)50,(byte)255));
}

static void DrawBoatLicenceInterior()
{
    for (int fx = 0; fx < 1400; fx += 70)
        for (int fy = 0; fy < 1000; fy += 70)
            Raylib.DrawRectangle(fx, fy, 70, 70, ((fx / 70 + fy / 70) % 2 == 0)
                ? new Color((byte)210,(byte)225,(byte)230,(byte)255)
                : new Color((byte)195,(byte)210,(byte)218,(byte)255));

    Raylib.DrawRectangle(400, 80, 400, 40, new Color((byte)90,(byte)65,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(400, 80, 400, 40, Color.Black);
    Program.DrawTextUI("HARBOURMASTER", 540, 92, 14, Color.White);

    // model boat display
    Raylib.DrawRectangle(100, 200, 200, 100, new Color((byte)140,(byte)110,(byte)70,(byte)255));
    Raylib.DrawRectangleLines(100, 200, 200, 100, Color.Black);
    Raylib.DrawTriangle(new Vector2(150, 260), new Vector2(180, 210), new Vector2(210, 260), new Color((byte)230,(byte)230,(byte)230,(byte)255));
    Raylib.DrawRectangle(140, 260, 100, 15, new Color((byte)60,(byte)40,(byte)25,(byte)255));
    Program.DrawTextUI("MODEL BOAT", 130, 210, 12, Color.White);

    // navigation chart table
    Raylib.DrawRectangle(850, 200, 200, 100, new Color((byte)70,(byte)100,(byte)140,(byte)255));
    Raylib.DrawRectangleLines(850, 200, 200, 100, Color.Black);
    for (int l = 0; l < 3; l++)
        Raylib.DrawLine(870, 220 + l * 25, 1030, 220 + l * 25, new Color((byte)200,(byte)220,(byte)230,(byte)180));
    Program.DrawTextUI("NAV CHARTS", 890, 205, 12, Color.White);

    // life ring wall decor
    Raylib.DrawCircleLines(1200, 400, 40, new Color((byte)220,(byte)60,(byte)60,(byte)255));
    Raylib.DrawCircleLines(1200, 400, 30, new Color((byte)220,(byte)60,(byte)60,(byte)255));
    for (int a = 0; a < 4; a++)
    {
        float rad = a * MathF.PI / 2f;
        Raylib.DrawLine(
            (int)(1200 + MathF.Cos(rad) * 30), (int)(400 + MathF.Sin(rad) * 30),
            (int)(1200 + MathF.Cos(rad) * 40), (int)(400 + MathF.Sin(rad) * 40),
            new Color((byte)220,(byte)60,(byte)60,(byte)255));
    }

    // entrance mat
    Raylib.DrawRectangle(600, 900, 200, 80, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(610, 910, 180, 60, new Color((byte)20,(byte)90,(byte)140,(byte)255));
}

static void DrawBoatMenu()
{
    if (!boatMenuOpen || currentBuilding?.BuildingName != "BOAT LICENCE OFFICE") return;

    Color navy = new Color((byte)10,(byte)50,(byte)90,(byte)255);
    int px = ScreenWidth/2 - 300, py = 60;
    Raylib.DrawRectangle(px, py, 600, 480, new Color((byte)5,(byte)20,(byte)35,(byte)245));
    Raylib.DrawRectangleLines(px, py, 600, 480, navy);
    Program.DrawTextUI("BOAT LICENCE OFFICE", px + 150, py + 12, 26, new Color((byte)100,(byte)200,(byte)255,(byte)255));
    Program.DrawTextUI($"Boating Lv {player.BoatingLevel}  |  ${player.Money}", px + 60, py + 46, 16, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    // theory — only ever needs passing once
    bool theoryAvail = !hasBoatTheory;
    Rectangle theoryBtn = new Rectangle(px + 20, py + 90, 560, 60);
    bool hT = Raylib.CheckCollisionPointRec(mouse, theoryBtn);
    Raylib.DrawRectangleRec(theoryBtn, theoryAvail ? new Color((byte)0,(byte)40,(byte)80,(byte)255) : new Color((byte)20,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(theoryBtn, 2, theoryAvail && hT ? Color.Gold : navy);
    Program.DrawTextUI(theoryAvail ? "BOAT THEORY TEST ($50)" : "Theory PASSED", px + 30, py + 104, 20, theoryAvail ? Color.White : Color.Green);
    if (theoryAvail && hT && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (player.Money >= 50)
        {
            player.Money -= 50;
            StartTestTransition(() => {
                boatTheoryQuestion = 0; boatTheoryScore = 0; boatTheorySelected = -1;
                boatTheoryFinished = false; boatTheoryPassed = false; boatTheoryOpen = true;
            }, "Starting Boat Theory Test");
            boatMenuOpen = false;
        }
        else { shopMessage = "Need $50 for the boat theory test!"; shopMessageTimer = 2f; }
    }

    // practical tiers — all listed, gated by theory + level
    for (int i = 0; i < boatTierLevel.Length; i++)
    {
        int ty = py + 170 + i * 70;
        bool avail = hasBoatTheory && !hasBoatPractical[i] && player.BoatingLevel >= boatTierLevel[i];
        Rectangle btn = new Rectangle(px + 20, ty, 560, 60);
        bool hov = Raylib.CheckCollisionPointRec(mouse, btn);
        Raylib.DrawRectangleRec(btn, avail ? new Color((byte)0,(byte)60,(byte)20,(byte)255) : new Color((byte)20,(byte)20,(byte)20,(byte)255));
        Raylib.DrawRectangleLinesEx(btn, 2, avail && hov ? Color.Gold : new Color((byte)0,(byte)100,(byte)40,(byte)255));
        string label = hasBoatPractical[i] ? $"{boatTierNames[i]} — LICENSED"
                     : !hasBoatTheory ? "Pass theory first!"
                     : player.BoatingLevel < boatTierLevel[i] ? $"{boatTierNames[i]} — Reach Lv {boatTierLevel[i]}"
                     : $"{boatTierNames[i]} PRACTICAL — Lv {boatTierLevel[i]}+  ($100)";
        Program.DrawTextUI(label, px + 30, ty + 18, 18, hasBoatPractical[i] ? Color.Green : (avail ? Color.White : Color.DarkGray));

        if (avail && hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (player.Money >= 100)
            {
                player.Money -= 100;
                boatTestingTier = i;
                preTestScene = currentScene;
                StartTestTransition(() => StartBoatTest(), $"Starting {boatTierNames[i]} Practical Test");
                boatMenuOpen = false;
            }
            else { shopMessage = "Need $100 for the practical test!"; shopMessageTimer = 2f; }
        }
    }

    Program.DrawTextUI("Q = Close", px + 260, py + 440, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) boatMenuOpen = false;
}

static void ScoreDive(Player player)
{
    // power: higher is better. rotation & entry: closer to centre (0.5) is better
    float powerScore = divingPower;                          // 0..1
    float rotScore = 1f - Math.Abs(divingRotation - 0.5f) * 2f;  // 1 at centre
    float entryScore = 1f - Math.Abs(divingEntry - 0.5f) * 2f;   // 1 at centre

    float combined = (powerScore * 0.3f + rotScore * 0.35f + entryScore * 0.35f);
    divingScore = (int)Math.Round(combined * 10f);   // 0..10

    string grade =
        divingScore >= 9 ? "PERFECT DIVE!" :
        divingScore >= 7 ? "Great dive!" :
        divingScore >= 4 ? "Decent dive." :
        divingScore >= 1 ? "Bit of a splash..." :
                           "Belly flop!";

    int xp = 8 + divingScore * 4;
    player.AddDivingXP(xp);

    int cash = divingScore >= 7 ? divingScore : 0;
    if (cash > 0) player.Money += cash;

    divingResult = cash > 0
        ? $"{grade}  Score {divingScore}/10  +{xp} Diving XP  +${cash}"
        : $"{grade}  Score {divingScore}/10  +{xp} Diving XP";
    divingResultTimer = 3f;
}
    }
}
