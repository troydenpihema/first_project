
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace OpenWorldRPG
{
    enum SceneState
    {
        MainMenu,
        World,
        Building,
        Minigame,
        Dungeon,
        DrivingTest,
        Dive,
        Space,
        Underwater,
        CardGame,
        Sleeping,
        BossArena,
        ClassTest,
    }

              public enum SlotSymbol
{
    Cherry,
    Lemon,
    Bell,
    Star,
    Seven
}

public static class SlotData
{
    // Higher weight = more common. Seven is rare = jackpot symbol.
    public static readonly (SlotSymbol symbol, int weight)[] SymbolWeights =
    {
        (SlotSymbol.Cherry, 40),
        (SlotSymbol.Lemon, 30),
        (SlotSymbol.Bell, 18),
        (SlotSymbol.Star, 9),
        (SlotSymbol.Seven, 3)
    };

    public static readonly Dictionary<SlotSymbol, int> ThreeMatchMultiplier = new()
{
    { SlotSymbol.Cherry, 2  },   // common — get double your bet
    { SlotSymbol.Lemon,  4  },   // slightly better
    { SlotSymbol.Bell,   10 },   // mid-tier
    { SlotSymbol.Star,   20 },   // rare — good payout
    { SlotSymbol.Seven,  50 },   // jackpot
};

public static readonly Dictionary<SlotSymbol, float> TwoMatchMultiplier = new()
{
    { SlotSymbol.Cherry, 0f   },  // two cherries — nothing, too common
    { SlotSymbol.Lemon,  0.5f },  // get half your bet back
    { SlotSymbol.Bell,   1f   },  // break even
    { SlotSymbol.Star,   2f   },  // small win
    { SlotSymbol.Seven,  5f   },  // two sevens — nice payout
};

    public static SlotSymbol GetRandomSymbol()
    {
        int totalWeight = 0;
        foreach (var (_, weight) in SymbolWeights) totalWeight += weight;

        int roll = Raylib.GetRandomValue(1, totalWeight);
        int cumulative = 0;

        foreach (var (symbol, weight) in SymbolWeights)
        {
            cumulative += weight;
            if (roll <= cumulative) return symbol;
        }

        return SlotSymbol.Cherry; // fallback, shouldn't hit
    }

    public static Color GetSymbolColor(SlotSymbol s) => s switch
    {
        SlotSymbol.Cherry => Color.Red,
        SlotSymbol.Lemon => Color.Yellow,
        SlotSymbol.Bell => Color.Gold,
        SlotSymbol.Star => Color.SkyBlue,
        SlotSymbol.Seven => Color.Magenta,
        _ => Color.White
    };

    public static string GetSymbolText(SlotSymbol s) => s switch
    {
        SlotSymbol.Cherry => "CHERRY",
        SlotSymbol.Lemon => "LEMON",
        SlotSymbol.Bell => "BELL",
        SlotSymbol.Star => "STAR",
        SlotSymbol.Seven => "7",
        _ => "?"
    };
}

    partial class Program
    {

        // MAIN INITILIZATION
        const int ScreenWidth = 1280;
        const int ScreenHeight = 720;
        static SceneState currentScene = SceneState.MainMenu;
        static SceneState preTestScene = SceneState.World; // add this alongside your other static vars
        static SceneState lastScene = SceneState.Building;
        enum FadeState { None, FadingOut, FadingIn }
        static FadeState sceneFadeState = FadeState.None;
        static Action pendingSceneSetup = null; 
        static float sceneFadeAlpha = 0f;      
        static SceneState pendingScene;
        static float sceneFadeSpeed = 500f;
        static Camera2D camera = new Camera2D();
        public static Player player = new Player(new Vector2(-1917, -9720));
        public static int dbgRow = 54, dbgFrames = 12;

        // Player
        public static bool useLayeredPlayer = true;

        // Magic
        static Vector2 holyShrinePos = new Vector2(8000, -2000);
        static Vector2 darkShrinePos = new Vector2(-8000, 9000);

        // COMBAT
        enum HandPhase { Tools, Combat }
        static HandPhase currentPhase = HandPhase.Tools;
        public static bool drawShapeArmorOnSprite = true;

        static (int melee, int ranged, int magic) GetArmorStyleBonus()
        {
            int m = 0, r = 0, g = 0, mageN = 0, rangN = 0, meleN = 0;
            foreach (var piece in new[] { armorHelmet, armorBody, armorLegs, armorBoots, armorGloves })
            {
                if (piece == null) continue;
                if (piece.Contains("Mage "))        { g += 3 + ClassTierIndex(piece) * 2; r += 1;         mageN++; }
                else if (piece.Contains("Ranger ")) { r += 3 + ClassTierIndex(piece) * 2; m += 1; g += 1; rangN++; }
                else                                { m += 3; r += 1;         meleN++; }                             
            }
            if (mageN == 5) g += 8;   // set: ARCHMAGE
            if (rangN == 5) r += 8;   // set: SHARPSHOOTER
            if (meleN == 5) m += 8;   // set: JUGGERNAUT
            return (m, r, g);
        }

        // PLayer ID
        static bool  idClaimed = false;        // player has a finished ID
        static bool  idPending = false;        // ID photo taken, awaiting delivery
        static float idDeliveryTimer = 0f;     // counts down one full day cycle
        static string idIssuedDate = "";       // date stamped on the ID
        static int   idTargetHouseIndex = -1;  // which house's mailbox receives it
        static bool  libraryPhotoMenuOpen = false;  // house-picker after clicking Take Photo
        static bool idMailWaiting = false;  
        static bool nearMailbox = false;
        static bool nearMailboxHasMail = false; 
        
        // Dropzone card and credits
        static bool  hasDropzoneCard = false;      // must buy the card first
        static float dropzoneCredit  = 0f;         // loaded balance
        static float dropzoneLifetimeSpend = 0f;   // total ever spent → drives tier
        static int   dropzoneTier = 0;             // 0=Red,1=Blue,2=Gold,3=Platinum,4=Black Diamond
        static readonly string[] dropzoneTierNames = { "Red", "Blue", "Gold", "Platinum", "Black Diamond" };
        static readonly float[]  dropzoneTierThresholds = { 10f, 20f, 30f, 40f, 50f };
        static readonly float[]  dropzoneTierPrices = { 2.00f, 1.80f, 1.50f, 1.00f, 0.50f };
        // Single source of truth: each machine's center point + interaction radius.
        // Draw, collision, and interaction all derive from this.
        
        struct MachineDef { public Vector2 Center; public float Radius; public MachineDef(float x, float y, float r){ Center=new Vector2(x,y); Radius=r; } }
         record MachineInstance(string Type, Vector2 Pos);
        static List<MachineInstance> placedMachines = new()
        {
            new("pinball",  new Vector2(1115, 245)),
            new("pinball",  new Vector2(1115, 700)),   // a SECOND pinball, just add a line
            new("airhock",  new Vector2(1115, 415)),
            // ... add as many as you want, any position ...
        };
        static readonly Dictionary<string, MachineDef> dzMachines = new()
        {
            ["bowl1"]   = new MachineDef(250, 300, 80),
            ["bowl2"]   = new MachineDef(250, 470, 80),
            ["claw1"]   = new MachineDef(600, 250, 70),
            ["claw2"]   = new MachineDef(760, 250, 70),
            ["arcade1"] = new MachineDef(900, 250, 70),
            ["arcade2"] = new MachineDef(1030, 250, 70),
            ["pinball"] = new MachineDef(1115, 245, 75),
            ["airhock"] = new MachineDef(1115, 415, 75),
            ["piano"]   = new MachineDef(1115, 585, 75),
            ["flappy"]  = new MachineDef(935, 615, 75),
            ["pool1"]   = new MachineDef(240, 852, 100),
            ["prize"]   = new MachineDef(785, 615, 80),
            ["food"]    = new MachineDef(550, 615, 80),
        };
        static Vector2 MPos(string k) => dzMachines[k].Center;
        static Vector2 MPosSouth(string k) => dzMachines[k].Center + new Vector2(0, 70);

        // ── COMBO / CRIT ──
        static int comboCount = 0;
        static float comboTimer = 0f;            // counts down; combo resets at 0
        const float comboWindow = 2.5f;          // seconds to land the next hit
        static float critChance = 0.15f;         // 15% base crit chance
        const float critMultiplier = 2f;
        
        // SAVE GAME
        static float autoSaveTimer = 0f;
        static float autoSaveInterval = 300f;
        static int selectedSlot = 0;
        static string[] savePaths = { "savegame1.txt", "savegame2.txt", "savegame3.txt" };
        static string savePath => savePaths[selectedSlot];
        static float totalPlayTime = 0f;
        static int dungeonsCleared = 0;
        static int timesCheated = 0;
        static Dictionary<string,int> sportPlayCounts = new();
        static Dictionary<string,int> minigamePlayCounts = new();

        // NPCS
        static float cRideAnimTimer = 0f;
        static Vector2 cRidePos = new Vector2(565, 250);   
        static bool cRideMessageActive = false;
        static int EnrolledToday()
        {
            int n = 0;
            foreach (var npc in npcs)
                if (npc.IndoorsDuringDay && npc.Hidden) n++;   
            return n;
        }

        // WORLD BOSS
        static WorldBoss worldBoss;
        static WorldBoss superBoss;

        // BOATING SYSTEM
        static bool hasBoatTheory = false;
        static bool isBoatTest = false;
        static readonly int boatStageCount = 3;
        static bool boatMenuOpen = false;
        static Vector2 boatBoardPos = Vector2.Zero;
        static bool[] hasBoatPractical = new bool[4];   // Dinghy, Speedboat, Yacht, Superyacht
        static readonly int[] boatTierLevel = { 1, 15, 30, 50 };
        static readonly string[] boatTierNames = { "Dinghy", "Speedboat", "Yacht", "Superyacht" };
        public static bool IsInWater(Vector2 pos) => Raylib.CheckCollisionPointRec(pos, oceanBounds);
        public static bool IsWaterAt(Vector2 pos) =>
            IsInWater(pos) || lakes.Any(l => Vector2.Distance(pos, l.Position) < 115);
        public static Rectangle oceanBounds = new Rectangle(-79800, 115200, 329600, 134600);
        static List<Vector2> coastguardStations = new();
        const int CoastguardFee = 200;
        static List<Vector2> boatSpawnPoints = new();
        static Vehicle.VehicleType[] boatTierType = { Vehicle.VehicleType.Dinghy, Vehicle.VehicleType.SpeedBoat,
        Vehicle.VehicleType.Yacht,  Vehicle.VehicleType.CruiseShip
        };
        static int BoatTierToTestFor
{
    get
    {
        for (int i = 0; i < hasBoatPractical.Length; i++)
            if (!hasBoatPractical[i]) return i;
        return hasBoatPractical.Length - 1;
    }
}
static bool BoatPracticalAvailable => player.BoatingLevel >= boatTierLevel[BoatTierToTestFor] && hasBoatTheory && !hasBoatPractical[BoatTierToTestFor];
static int boatTheoryQuestion, boatTheoryScore; static int boatTheorySelected = -1;
static bool boatTheoryOpen = false, boatTheoryFinished = false, boatTheoryPassed = false;
static int boatTestingTier = 0;

static (string Question, string[] Options, int Correct)[] boatTheoryTest = {
    ("What side do you pass an oncoming boat on?", new[]{"Port (left)","Starboard (right)","Either side","Stop and wait"}, 1),
    ("What does a red buoy mean when returning to harbour?", new[]{"Keep to your left","Keep to your right","Danger, stop","No meaning"}, 1),
    ("Who has right of way: sail or power boat?", new[]{"Power boat","Sail boat","Whoever is faster","Neither"}, 1),
    ("What's the minimum safety gear required?", new[]{"Life jackets for all aboard","A flag","Nothing required","A horn only"}, 0),
    ("What should you do in fog?", new[]{"Speed up","Sound signals and slow down","Turn off lights","Ignore it"}, 1),
    ("What's a 'wake' ", new[]{"The boat's flag","Trailing waves behind a moving boat","A type of anchor","A distress signal"}, 1),
    ("Distress signal for help?", new[]{"Waving arms up and down","Blowing a horn once","Turning off engine","Speeding away"}, 0),
    ("Before departure you should check?", new[]{"Weather and fuel","Nothing, just go","Only fuel","Only weather"}, 0),
    ("Maximum safe speed near swimmers?", new[]{"Full throttle","No wake speed","Doesn't matter","Reverse only"}, 1),
    ("What do you do if your boat capsizes?", new[]{"Swim to shore immediately","Stay with the boat","Panic","Dive under it"}, 1),
};

        // DRIVER'S LICENCE SYSTEM
        enum LicenceClass { None, D, C, B, A, S }
        static int testStage = 0; // 0=slalom 1=parking 2=traffic 3=lanes 4=course
        static int testStageCount = 5;
        static string testStageName => testStage switch {
            0 => "Stage 1: Slalom",
            1 => "Stage 2: Bay Parking",
            2 => "Stage 3: Intersection",
            3 => "Stage 4: Lane Keeping",
            4 => "Stage 5: Test Course",
            _ => ""
        };

        // stage specific
        static List<Rectangle> parkingCars = new();
        static Rectangle parkingBay;
        static bool parkedSuccessfully = false;
        static float parkingTimer = 0f;

        static List<Vector2> trafficCars = new();
        static List<Vector2> trafficVelocities = new();
        static bool intersectionCleared = false;

        static List<Rectangle> laneBoundaries = new();
        static int laneCheckpointsHit = 0;
        static int laneCheckpointsRequired = 5;
        static Vector2[] laneCheckpoints;

        static float stageTimer = 0f;
        static float stageTimeLimit = 0f;
        static bool stageFailed = false;
        static bool stageComplete = false;

        static bool hasTheoryD = false, hasTheoryC = false, hasTheoryB = false, hasTheoryA = false, hasTheoryS = false;
        static bool hasPracticalD = false, hasPracticalC = false, hasPracticalB = false, hasPracticalA = false, hasPracticalS = false;

        static LicenceClass CurrentLicenceClass => player.DrivingLevel switch {
            >= 1  and <= 19  => LicenceClass.D,
            >= 20 and <= 39  => LicenceClass.C,
            >= 40 and <= 59  => LicenceClass.B,
            >= 60 and <= 79  => LicenceClass.A,
            >= 80 and <= 100 => LicenceClass.S,
            _ => LicenceClass.None
        };

        static bool PreviousLicenceHeld => ClassToTestFor switch {
            LicenceClass.D => true,
            LicenceClass.C => hasPracticalD,
            LicenceClass.B => hasPracticalC,
            LicenceClass.A => hasPracticalB,
            LicenceClass.S => hasPracticalA,
            _ => false
        };

        static bool HasTheoryForCurrentClass => CurrentLicenceClass switch {
            LicenceClass.D => hasTheoryD,
            LicenceClass.C => hasTheoryC,
            LicenceClass.B => hasTheoryB,
            LicenceClass.A => hasTheoryA,
            LicenceClass.S => hasTheoryS,
            _ => false
        };
        static bool HasTheoryForTestClass => ClassToTestFor switch {
            LicenceClass.D => hasTheoryD, LicenceClass.C => hasTheoryC, LicenceClass.B => hasTheoryB,
            LicenceClass.A => hasTheoryA, LicenceClass.S => hasTheoryS, _ => false
        };
        static bool HasPracticalForTestClass => ClassToTestFor switch {
            LicenceClass.D => hasPracticalD, LicenceClass.C => hasPracticalC, LicenceClass.B => hasPracticalB,
            LicenceClass.A => hasPracticalA, LicenceClass.S => hasPracticalS, _ => false
        };

        static bool HasPracticalForCurrentClass => CurrentLicenceClass switch {
            LicenceClass.D => hasPracticalD,
            LicenceClass.C => hasPracticalC,
            LicenceClass.B => hasPracticalB,
            LicenceClass.A => hasPracticalA,
            LicenceClass.S => hasPracticalS,
            _ => false
        };

       static bool PracticalAvailable => ClassToTestFor switch {
            LicenceClass.D => player.DrivingLevel >= 10 && !hasPracticalD,
            LicenceClass.C => player.DrivingLevel >= 30 && !hasPracticalC,
            LicenceClass.B => player.DrivingLevel >= 50 && !hasPracticalB,
            LicenceClass.A => player.DrivingLevel >= 70 && !hasPracticalA,
            LicenceClass.S => player.DrivingLevel >= 90 && !hasPracticalS,
            _ => false
        };
        static LicenceClass ClassToTestFor
        {
            get
            {
                if (!hasPracticalD) return LicenceClass.D;
                if (!hasPracticalC && CurrentLicenceClass >= LicenceClass.C) return LicenceClass.C;
                if (!hasPracticalB && CurrentLicenceClass >= LicenceClass.B) return LicenceClass.B;
                if (!hasPracticalA && CurrentLicenceClass >= LicenceClass.A) return LicenceClass.A;
                if (!hasPracticalS && CurrentLicenceClass >= LicenceClass.S) return LicenceClass.S;
                return CurrentLicenceClass;
            }
        }
    
        static bool[]  licencePending     = new bool[5];   // awaiting delivery, indexed by (int)LicenceClass - 1
        static float[] licenceDeliveryTimer = new float[5];
        static bool[]  licenceMailWaiting = new bool[5];   // arrived, sitting at mailbox
        static bool[] licenceTheoryDelivered    = new bool[5];   
        static bool[] licencePendingIsTheory = new bool[5];
        static bool[] licencePracticalDelivered = new bool[5]; 
        static int[]   licenceTargetHouse = new int[5];
        static bool    licenceMailMenuOpen = false;
        static LicenceClass licencePendingClass = LicenceClass.None;
        static bool   licenceCongratsOpen = false;
        static string licenceCongratsText = "";  

        // AA Building state
        static bool aaMenuOpen       = false;
        static bool aaTheoryOpen     = false;
        static int  aaTheoryQuestion = 0;
        static int  aaTheoryScore    = 0;
        static int  aaTheorySelected = -1;
        static bool aaTheoryFinished = false;
        static bool aaTheoryPassed   = false;

        // Driving test scene
        static Vehicle testVehicle   = null;
        static Vector2[] testCheckpoints;
        static int   testCurrentCheckpoint = 0;
        static float testTimer         = 0f;
        static float testTimeLimit     = 0f;
        static bool  testComplete      = false;
        static bool  testFailed        = false;
        static string testMessage      = "";
        static float testMessageTimer  = 0f;
        static int   testLap           = 0;
        static int   testLapsRequired  = 1;
        static Camera2D testCamera;
        static List<Vector2> testCones = new();
        static List<Rectangle> testBarriers = new();
        static bool testConePenalty = false;
        static float testConePenaltyTimer = 0f;
        static int testConesHit = 0;
        static int maxConesAllowed = 3;
        static LicenceClass testingForClass = LicenceClass.D;
        static Vector2 testReturnPos;
        static Vector2 aaInteriorReturnPos = new Vector2(300, 500);

        static TheoryQ[][] theoryTests = {
            // D Licence (1-19) — basic rules
            new TheoryQ[] {
                new("What does a red traffic light mean?",
                    new[]{"Speed up","Stop","Slow down","Honk"}, 1),
                new("What side of the road do you drive on in New Zealand?",
                    new[]{"Left","Right","Middle","Either"}, 0),
                new("What is the default urban speed limit in NZ?",
                    new[]{"80 km/h","100 km/h","50 km/h","60 km/h"}, 2),
                new("When must you give way?",
                    new[]{"Never","Only at roundabouts","When turning right","When you feel like it"}, 2),
                new("What does a yellow line on the road mean?",
                    new[]{"Parking allowed","No passing","Bus lane","Cycle lane"}, 1),
                new("At a give way sign you must?",
                    new[]{"Stop completely","Give way to traffic on the main road","Flash your lights","Beep your horn"}, 1),
                new("Blood alcohol limit for drivers under 20?",
                    new[]{"80mg/100ml","Zero","50mg/100ml","30mg/100ml"}, 1),
                new("How far before a turn should you signal?",
                    new[]{"3 seconds","At the turn","30 metres","As far in advance as possible"}, 3),
                new("What must you do when an emergency vehicle approaches?",
                    new[]{"Speed up","Pull over and stop","Flash your lights","Ignore it"}, 1),
                new("A solid yellow centre line means?",
                    new[]{"You may overtake","No overtaking","Slow down","Parking here"}, 1),
            },
            // C Licence (20-39) — intermediate
            new TheoryQ[] {
                new("Open road speed limit in NZ?",
                    new[]{"100 km/h","110 km/h","90 km/h","80 km/h"}, 0),
                new("Minimum following distance at 100km/h?",
                    new[]{"1 second","2 seconds","3 seconds","4 seconds"}, 2),
                new("When can you use your horn?",
                    new[]{"To greet friends","When angry","To warn others of danger","Never"}, 2),
                new("Fatigue: what should you do if tired?",
                    new[]{"Open window","Turn up music","Take a break","Speed up to finish faster"}, 2),
                new("What is aquaplaning?",
                    new[]{"Driving on ice","Tyre loss of contact with wet road","Braking too hard","Driving in fog"}, 1),
                new("Tyre tread minimum depth in NZ?",
                    new[]{"1.5mm","2mm","3mm","1mm"}, 0),
                new("When can you overtake on the left?",
                    new[]{"Never","When the vehicle ahead is turning right","Always","On highways only"}, 1),
                new("What does ABS stand for?",
                    new[]{"Auto Brake System","Anti-lock Braking System","Advanced Brake Safety","Automatic Brake Support"}, 1),
                new("How far from a fire hydrant must you not park?",
                    new[]{"1 metre","3 metres","5 metres","6 metres"}, 0),
                new("Dipped headlights required from?",
                    new[]{"Sunrise only","30 min after sunset to 30 min before sunrise","Only in rain","Midnight to 5am"}, 1),
            },
            // B Licence (40-59) — advanced
            new TheoryQ[] {
                new("Maximum towing speed in NZ?",
                    new[]{"80 km/h","90 km/h","100 km/h","70 km/h"}, 0),
                new("Right of way at an uncontrolled intersection?",
                    new[]{"Biggest vehicle","Vehicle on the right","Vehicle on the left","Fastest vehicle"}, 1),
                new("When is it legal to use a mobile phone while driving?",
                    new[]{"At traffic lights","Never unless hands-free","Only for calls","Whenever"}, 1),
                new("Safe gap to merge at 100km/h?",
                    new[]{"2 seconds","4 seconds","6 seconds","1 second"}, 1),
                new("Brake fade is caused by?",
                    new[]{"Cold brakes","Overheated brakes","Wet brakes","New brake pads"}, 1),
                new("What is the purpose of ESC?",
                    new[]{"Improve fuel economy","Prevent skidding","Increase speed","Reduce tyre wear"}, 1),
                new("Double white centre lines mean?",
                    new[]{"You can cross if clear","No crossing at all","Cross only to turn","Cross only going uphill"}, 1),
                new("Heavy vehicle max speed on open road?",
                    new[]{"90 km/h","100 km/h","80 km/h","70 km/h"}, 0),
                new("When must headlights be on?",
                    new[]{"Only at night","Whenever visibility is less than 100m","Only in rain","Never required"}, 1),
                new("Correct hand position on steering wheel?",
                    new[]{"10 and 2","9 and 3","8 and 4","Both hands on top"}, 1),
            },
            // A Licence (60-79) — expert
            new TheoryQ[] {
                new("Understeer is when?",
                    new[]{"Rear slides out","Front wheels lose grip and car goes straight","Car spins","Brakes lock"}, 1),
                new("Oversteer is when?",
                    new[]{"Front pushes wide","Rear breaks away","Car goes straight","Brakes fade"}, 1),
                new("Trail braking is used for?",
                    new[]{"Motorway driving","Rotating the car into a corner","Parking","Emergency stops"}, 1),
                new("What is the racing line?",
                    new[]{"A motorway lane","The fastest path through a corner","The centre of the road","The inside lane"}, 1),
                new("Heel-toe technique involves?",
                    new[]{"Braking and steering","Simultaneous brake, throttle blip and clutch","Gear change only","Handbrake use"}, 1),
                new("Lift-off oversteer occurs when?",
                    new[]{"Accelerating in a corner","Lifting throttle mid-corner","Braking in a straight","Changing gear"}, 1),
                new("Correct apex technique?",
                    new[]{"Turn in early, apex early","Late turn in, late apex","Turn in at the cone","Always hit the inside"}, 1),
                new("Tyre slip angle is?",
                    new[]{"Tyre pressure loss","Difference between wheel direction and travel direction","Tread wear","Puncture angle"}, 1),
                new("Weight transfer under braking moves weight?",
                    new[]{"To the rear","To the front","Equally","To the outside"}, 1),
                new("Launch control is used for?",
                    new[]{"Cruise control","Optimising acceleration from standstill","Parking assist","Motorway merging"}, 1),
            },
            // S Licence (80-100) — professional
            new TheoryQ[] {
                new("Scandinavian flick technique purpose?",
                    new[]{"Emergency braking","Rotate car before a rally corner","Parallel parking","Fuel saving"}, 1),
                new("Threshold braking is?",
                    new[]{"Full ABS engagement","Braking just below lockup point for maximum deceleration","Light braking","Cadence braking"}, 1),
                new("What is power oversteer?",
                    new[]{"Using brakes to rotate","Using excess throttle to break rear traction","Steering with weight","Drifting by braking"}, 1),
                new("Counter-steering means?",
                    new[]{"Steering opposite to corner","Steering into a slide to correct it","Braking while turning","Steering with throttle"}, 1),
                new("G-force in a 2G corner means?",
                    new[]{"Twice earth gravity laterally","Twice speed","Double fuel use","Twice braking distance"}, 0),
                new("What is cadence braking?",
                    new[]{"Full brake hold","Pumping brakes repeatedly to prevent lockup","One-foot braking","ABS simulation"}, 1),
                new("Polar moment of inertia affects?",
                    new[]{"Engine power","How quickly a car rotates","Fuel consumption","Tyre grip"}, 1),
                new("Limited slip differential purpose?",
                    new[]{"Reduce understeer","Allow wheels to spin at different speeds while limiting slip","Save fuel","Improve steering"}, 1),
                new("What is the fastest cornering technique?",
                    new[]{"Early turn in","Late apex with late braking","Always stay wide","Brake in the corner"}, 1),
                new("Yaw moment is?",
                    new[]{"Vertical rotation","Horizontal rotation around the car's vertical axis","Forward tilt","Sideways weight shift"}, 1),
            }
        };
                
        // CALENDAR AND TIME
        static int dayOfWeek = 0; // 0-6, Monday to Sunday
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static float dayCounter = 0f; // tracks full day cycles
        static float timeOfDay = 0f; // 0 to 1, full day cycle
        static float daySpeed = 0.001f; // how fast the day progresses
        // ── PETS ──
        static List<string> ownedEggs = new();        // egg item names the player holds
        static string incubatingEgg = null;           // egg currently in the incubator (null = empty)
        static float incubationProgress = 0f;          // accumulates dt; hatches at one full day
        static float incubationNeeded = 0f;            // set when incubation starts = seconds in a day cycle
        public static Pet activePet = null;                    // the one pet currently following (null = none)
        static int eggDropChance = 10;
        static Pet pendingPet = null;
        const float petCollectRange = 80f;
        static List<string> storedPets = new();    // pet types in storage (boxed)
        const float petTeleportRange = 700f;       // auto-teleport if pet falls this far behind
        static bool petStorageMenuOpen = false;

        // ── INCUBATOR ──
        static List<Vector2> incubatorPositions = new();
        static float incubatorAnimTimer = 0f;
        static bool incubatorMenuOpen = false;
        static bool nearIncubator = false;

        // maps a boss/egg name to its pet color (extend with your real boss names)

        // BIOMES
        static string currentBiome = "SAFE ZONE";
        static string lastBiome = "";
        static float biomeMessageTimer = 0f;

        // Driving

// Garage
static List<Garage> garages = new();
static List<Vehicle> vehiclesToRemove = new();
static List<Rideable> rideablesToRemove = new();
static List<Stable> stables = new();

static float DefaultSpeedFor(Vehicle.VehicleType type) => type switch
{
    Vehicle.VehicleType.Sedan => 650f,
    Vehicle.VehicleType.Truck => 900f,
    Vehicle.VehicleType.SUV => 500f,
    Vehicle.VehicleType.PoliceCar => 500f,
    Vehicle.VehicleType.FireTruck => 500f,
    Vehicle.VehicleType.Ambulance => 500f,
    Vehicle.VehicleType.Ute => 450f,
    Vehicle.VehicleType.MonsterTruck => 380f,
    Vehicle.VehicleType.Convertible => 650f,
    Vehicle.VehicleType.MuscleCar => 700f,
    Vehicle.VehicleType.RacingCar => 750f,
    Vehicle.VehicleType.Jetski => 750f,
    Vehicle.VehicleType.Dinghy => 300f,
    Vehicle.VehicleType.SpeedBoat => 850f,
    Vehicle.VehicleType.Canoe => 250f,
    Vehicle.VehicleType.Yacht => 500f,
    Vehicle.VehicleType.CruiseShip => 320f,
    _ => 500f
};

        // NEW — maps a vehicle's unlock level to the licence class band it belongs to

// Per vehicle drivability check: full licence for that class = no time restriction, theory-only = restricted hours

// Jobs
static List<Job> jobBoard = new()
{
    new Job{ Title="Lumber Order",  Employer="Sawmill",    Resource="Logs",  Target=15, Pay=150 },
    new Job{ Title="Fresh Catch",   Employer="Fishmonger", Resource="Fish",  Target=10, Pay=130 },
    new Job{ Title="Bone Collector",Employer="Alchemist",  Resource="Bones", Target=10, Pay=180 },
    new Job{ Title="Fur Trader",    Employer="Tailor",     Resource="Fur",   Target=6,  Pay=160 },
};
static bool jobBoardOpen = false;
static int lastJobResetDay = -1;

// ── SHOP OPERATING HOURS ──
static readonly Dictionary<string, (float open, float close)> shopHours = new()
{
    { "SUPERMARKET",    (7f,  21f) },
    { "WEAPONS",        (8f,  17f) },
    { "MAGIC SHOP",     (8f,  17f) },
    { "RANGING SHOP",   (8f,  17f) },
    { "FARMING SHOP",   (7f,  18f) },
    { "STORE",          (8f,  17f) },
    { "HOBBIES STORE",  (9f,  17f) },
    { "KiwiCuts",       (9f,  17f) },
    { "HALLENSTEINS",   (9f,  17f) },
    { "HOSPITAL",       (0f,  24f) },  // always open
    { "POLICE STATION", (0f,  24f) },
    { "GAS STATION",    (6f,  22f) },
    { "DBar",           (17f, 2f)  },  // opens evening, closes 2 AM
    { "Casino",         (18f, 4f)  },
    { "BANK",           (9f,  16f) },
    { "LIBRARY",        (8f,  18f) },
    { "GYM",            (6f,  22f) },
};

public static bool IsShopOpen(string buildingName)
{
    if (!shopHours.TryGetValue(buildingName, out var hrs)) return true; // unknown = always open
    float h = GetCurrentHour();
    if (hrs.open < hrs.close)
        return h >= hrs.open && h < hrs.close;
    // wraps midnight (e.g. 17–2)
    return h >= hrs.open || h < hrs.close;
}

static string GetShopHoursLabel(string buildingName)
{
    if (!shopHours.TryGetValue(buildingName, out var hrs)) return "";
    int oh = (int)hrs.open % 12; if (oh == 0) oh = 12;
    int ch = (int)hrs.close % 12; if (ch == 0) ch = 12;
    string op = hrs.open >= 12 ? "PM" : "AM";
    string cp = hrs.close >= 12 ? "PM" : "AM";
    return $"{oh}{op}–{ch}{cp}";
}

// Tasks
static List<SideTask> billboardTasks = new()
{
    new SideTask{ Title="Wolf Cull",    Progress=() => wolvesKilled,   Target=10, Pay=200, DeliverTo="Building:JOB CENTRE" },
    new SideTask{ Title="Home Cooking", Progress=() => mealsCooked,    Target=5,  Pay=150, DeliverTo="NPC:Aroha" },
    new SideTask{ Title="Harvest Help", Progress=() => cropsHarvested, Target=6,  Pay=170, DeliverTo="Building:FARMING SHOP" },
    new SideTask{ Title="Timber Run",   Progress=() => player.Logs,    Target=12, Pay=160, DeliverTo="NPC:Tama" },
};
static SideTask activeSideTask = null;
static bool billboardOpen = false;
static Vector2 billboardPos = new Vector2(350, -600);   // town square — move to suit

// Banking
static bool bankSignedUp = false;
static bool bankCardPending = false, bankCardMailWaiting = false, bankCardDelivered = false;
static int  bankBalance = 0;
static int  bankCardTier = 0;                                   // 0 = starter
static readonly int[] cardDailyLimits = { 100, 500, 2000, 10000 };
static readonly int[] cardUpgradeCosts = { 0, 1000, 10000, 50000 };
static int  cardSpentToday = 0;

// Relationships and friends

static List<FriendNPC> friendNPCs = new()
{
    new FriendNPC {
        Name="JDogg", FavoriteGift="Fish", Shop="SUPERMARKET",
        Npc=new NPC(new Vector2(300, -500), "JDogg", ""),
        Likes=new[]{"Fish","Rod","Water"}, Dislikes=new[]{"Bones","Magic"},
        Fears=new[]{"Sharks"}, Personality="Loyal, competitive, loves the ocean",
        FavoriteFood="Cooked Fish", Opinion="The sea gives you everything you need.",
        Partner="Cride", Children=Array.Empty<string>()
    },
    new FriendNPC {
        Name="Cride", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Cride", ""),
        Likes=new[]{"Fur","Bow","Leather"}, Dislikes=new[]{"Loud","Fire"},
        Fears=new[]{"Blizzards"}, Personality="Quiet, sharp, observant hunter",
        FavoriteFood="Steak & Chips", Opinion="Actions speak louder than words.",
        Partner="JDogg", Children=Array.Empty<string>()
    },
    new FriendNPC {
        Name="Shack", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Shack", ""),
        Likes=new[]{"Bones","Staff","Crystal"}, Dislikes=new[]{"Iron","Sword"},
        Fears=new[]{"Bandits"}, Personality="Fierce, protective, spiritual warrior",
        FavoriteFood="Homemade Pizza", Opinion="Magic is in everything if you look.",
        Partner="Jake", Children=new[]{"Hunter","Ava"}
    },
    new FriendNPC {
        Name="Jake", FavoriteGift="Logs", Shop="FARMING SHOP",
        Npc=new NPC(new Vector2(-400, -750), "Jake", ""),
        Likes=new[]{"Logs","Axe","Wood"}, Dislikes=new[]{"Snake","Venom"},
        Fears=new[]{"Snakes"}, Personality="Joker, lucky, always gambling",
        FavoriteFood="Bacon & Eggs", Opinion="Life's a gamble, might as well enjoy it.",
        Partner="Shack", Children=new[]{"Hunter","Ava"}
    },
    new FriendNPC {
        Name="Traz", FavoriteGift="Fish", Shop="SUPERMARKET",
        Npc=new NPC(new Vector2(360, -520), "Traz", "Kia ora!"),
        Likes=new[]{"Fish","Stone","Iron"}, Dislikes=new[]{"Magic","Staff"},
        Fears=new[]{"Heights"}, Personality="Tough, gruff, heart of gold",
        FavoriteFood="Steak & Chips", Opinion="Hard work beats talent every time.",
        Partner="Nola", Children=new[]{"Eli","Ezra","Eden"}
    },
    new FriendNPC {
        Name="Nola", FavoriteGift="Logs", Shop="FARMING SHOP",
        Npc=new NPC(new Vector2(-400, -750), "Nola", ""),
        Likes=new[]{"Logs","Flower","Seed"}, Dislikes=new[]{"Bones","Violence"},
        Fears=new[]{"Storms"}, Personality="Patient, kind, nurturing gardener",
        FavoriteFood="Vegetable Soup", Opinion="Everything grows if you give it love.",
        Partner="Traz", Children=new[]{"Eli","Ezra","Eden"}
    },
    new FriendNPC {
        Name="Tipene", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Tipene", ""),
        Likes=new[]{"Bones","Crystal","Stone"}, Dislikes=new[]{"Noise","Gambling"},
        Fears=new[]{"Losing loved ones"}, Personality="Calm, spiritual, protective guardian",
        FavoriteFood="Roast Potato", Opinion="Strength comes from knowing who you protect.",
        Partner="Joybells", Children=Array.Empty<string>()
    },
    new FriendNPC {
        Name="Joybells", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Joybells", ""),
        Likes=new[]{"Fur","Music","Flower"}, Dislikes=new[]{"Darkness","Dungeon"},
        Fears=new[]{"The dark"}, Personality="Cheerful, bright, lights up every room",
        FavoriteFood="Fruit Salad", Opinion="There's always a reason to smile!",
        Partner="Tipene", Children=Array.Empty<string>()
    },
    new FriendNPC {
        Name="Rala", FavoriteGift="Fish", Shop="SUPERMARKET",
        Npc=new NPC(new Vector2(300, -500), "Rala", ""),
        Likes=new[]{"Fish","Crystal","Star"}, Dislikes=new[]{"Dirt","Mud"},
        Fears=new[]{"Being alone"}, Personality="Curious, dreamy, mysterious",
        FavoriteFood="Pasta Meal", Opinion="The stars know more than we ever will.",
        Partner="someone", Children=Array.Empty<string>()
    },
    new FriendNPC {
        Name="Leo", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Leo", ""),
        Likes=new[]{"Bones","Gold","Sword"}, Dislikes=new[]{"Cowardice","Running"},
        Fears=new[]{"Failure"}, Personality="Bold, proud, natural leader",
        FavoriteFood="Steak & Chips", Opinion="A true king earns his crown.",
        Partner="Alice", Children=new[]{"Jasper","Whale"}
    },
    new FriendNPC {
        Name="Alice", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Alice", ""),
        Likes=new[]{"Fur","Book","Crystal"}, Dislikes=new[]{"Rude","Fighting"},
        Fears=new[]{"Losing her family"}, Personality="Thoughtful, poised, fiercely loving",
        FavoriteFood="Pancakes", Opinion="Knowledge is the truest kind of power.",
        Partner="Leo", Children=new[]{"Jasper","Whale"}
    },
    new FriendNPC {
        Name="Eden", FavoriteGift="Logs", Shop="FARMING SHOP",
        Npc=new NPC(new Vector2(-400, -750), "Eden", ""),
        Likes=new[]{"Logs","Flower","Seed"}, Dislikes=new[]{"Fire","Smoke"},
        Fears=new[]{"Forest fires"}, Personality="Nature-loving, gentle, adventurous",
        FavoriteFood="Fruit Salad", Opinion="The forest talks if you listen.",
        Partner="", Parents=new[]{"Traz","Nola"}, IsChild=true
    },
    new FriendNPC {
        Name="Eli", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Eli", ""),
        Likes=new[]{"Bones","Iron","Copper"}, Dislikes=new[]{"Mess","Chaos"},
        Fears=new[]{"Thunder"}, Personality="Smart, inventive, always tinkering",
        FavoriteFood="Sandwich", Opinion="If it's broken, I can fix it.",
        Partner="", Parents=new[]{"Traz","Nola"}, IsChild=true
    },
    new FriendNPC {
        Name="Ezra", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Ezra", ""),
        Likes=new[]{"Fur","Speed","Race"}, Dislikes=new[]{"Slow","Waiting"},
        Fears=new[]{"Being stuck"}, Personality="Fast, restless, loves racing",
        FavoriteFood="Bacon & Eggs", Opinion="Life's too short to walk!",
        Partner="", Parents=new[]{"Traz","Nola"}, IsChild=true
    },
    new FriendNPC {
        Name="Hunter", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Hunter", ""),
        Likes=new[]{"Bones","Bow","Stinger"}, Dislikes=new[]{"Sitting","Boredom"},
        Fears=new[]{"Bears"}, Personality="Brave, scrappy, always exploring",
        FavoriteFood="Cooked Fish", Opinion="Adventure is out there!",
        Partner="", Parents=new[]{"Shack","Jake"}, IsChild=true
    },
    new FriendNPC {
        Name="Ava", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Ava", ""),
        Likes=new[]{"Fur","Music","Dance"}, Dislikes=new[]{"Fighting","Ugly"},
        Fears=new[]{"Stage fright"}, Personality="Dramatic, sweet, natural performer",
        FavoriteFood="Pancakes", Opinion="The world is a stage, darling!",
        Partner="", Parents=new[]{"Shack","Jake"}, IsChild=true
    },
    new FriendNPC {
        Name="Whale", FavoriteGift="Bones", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(700, -900), "Whale", ""),
        Likes=new[]{"Bones","Fish","Ocean"}, Dislikes=new[]{"Crowds","Noise"},
        Fears=new[]{"Loneliness"}, Personality="Gentle giant, quiet, deep thinker",
        FavoriteFood="Cooked Fish", Opinion="The deepest waters are the calmest.",
        Partner="", Parents=new[]{"Leo","Alice"}, IsChild=true
    },
    new FriendNPC {
        Name="Jasper", FavoriteGift="Crystal", Shop="MAGIC SHOP",
        Npc=new NPC(new Vector2(500, -800), "Jasper", ""),
        Likes=new[]{"Crystal","Gold","Gem"}, Dislikes=new[]{"Dirt","Mess"},
        Fears=new[]{"The dark"}, Personality="Curious collector, loves shiny things",
        FavoriteFood="Homemade Pizza", Opinion="Every rock has a treasure inside!",
        Partner="", Parents=new[]{"Leo","Alice"}, IsChild=true
    },
    new FriendNPC {
        Name="Jail", FavoriteGift="Fur", Shop="DBar",
        Npc=new NPC(new Vector2(-150, -300), "Jail", ""),
        Likes=new[]{"Fur","Iron","Sword"}, Dislikes=new[]{"Magic","Crystal"},
        Fears=new[]{"Being judged"}, Personality="Tough exterior, loyal once trusted",
        FavoriteFood="Steak & Chips", Opinion="Trust is earned, not given.",
        Partner="", Children=Array.Empty<string>()
    },
};        

        // Grocery / shopping bag
        static int playerGroceryBags = 0;       // Shopping Bag count in inventory
        static List<string> groceryBagContents = new List<string>();  // what's inside the last bag
        static bool groceryShopOpen = false;
        static int groceryAisleFilter = 0;  // 0=all, 1,2,3

        // Fridge storage (MY HOUSE)
        static List<string> fridgeContents = new List<string>(new string[12]);
        static bool fridgeOpen = false;

        // Cupboard storage (MY HOUSE)
        static List<string> cupboardContents = new List<string>(new string[12]);
        static bool cupboardOpen = false;

        // Campfire
        static List<Vector2> campfirePositions = new();
        // per-campfire fuel: logs (0-5) and burn timer for the current log
        static Dictionary<int,int>   campfireLogs  = new();   // index → logs remaining (0-5)
        static Dictionary<int,float> campfireBurn  = new();   // index → seconds left on current log
        const float LogBurnSeconds = 60f;    // one log lasts 1 minute
        const int   MaxLogs = 5;              // stoke up to 5 logs (5 min max)

        static bool CampfireIsLit(int i) =>
            campfireLogs.GetValueOrDefault(i) > 0 && campfireBurn.GetValueOrDefault(i) > 0f;
        static float campfireAnimTimer = 0f;
        static int builtinCampfireCount = 0;

        // SLEEPING
        static float sleepTimer = 0f;
        static float sleepDuration = 3.5f;
        static float sleepFadeAlpha = 0f;  // 0=transparent, 255=black
        static bool sleepFadingIn = true;
        static float zzzTimer = 0f;

        // Cooking state
        static bool cookingMenuOpen = false;
        static string cookingContext = "";   // "campfire" or "kitchen"
        static float cookingTimer = 0f;
        static string cookingItem = "";
        static bool isCooking = false;
        // Quick-cook (raw food from toolbar at a campfire)
        static bool quickCooking = false;        // true = cooking a raw toolbar item, not a recipe
        static float quickCookTimer = 0f;
        static string quickCookRaw = "";         // the raw item being cooked
        static string quickCookResult = "";      // what it becomes if not burnt
        static int quickCookSlot = -1;           // toolbar slot the raw item came from
        const float QuickCookDuration = 2f;

        static CookingRecipe[] cookingRecipes = {
            // campfire + kitchen
            new("Cook Fish",        new[]{"Fish"},                           "Cooked Fish",    10, false, 20),
            new("Cook Meat",        new[]{"Raw Meat"},                       "Cooked Meat",    12, false, 30),
            new("Roast Potato",     new[]{"Potato"},                         "Roast Potato",   8,  false, 15),
            new("Corn on the Cob",  new[]{"Corn"},                           "Cooked Corn",    6,  false, 12),
            // kitchen only
            new("Bacon & Eggs",     new[]{"Bacon","Eggs"},                   "Bacon & Eggs",   20, true,  40),
            new("Pasta",            new[]{"Pasta","Tomato Sauce"},           "Pasta Meal",     25, true,  45),
            new("Sandwich",         new[]{"Bread","Cheese","Ham"},           "Sandwich",       15, true,  35),
            new("Fruit Salad",      new[]{"Apple","Orange","Banana"},        "Fruit Salad",    18, true,  30),
            new("Steak & Chips",    new[]{"Raw Meat","Potato"},              "Steak & Chips",  30, true,  60),
            new("Soup",             new[]{"Carrot","Potato","Onion"},        "Vegetable Soup", 22, true,  50),
            new("Pancakes",         new[]{"Flour","Eggs","Milk"},            "Pancakes",       18, true,  35),
            new("Pizza",            new[]{"Flour","Tomato Sauce","Cheese"},  "Homemade Pizza", 35, true,  70),
        };

        // Supermarket grocery shopping
        static Dictionary<string,int> groceryCart = new Dictionary<string,int>(); // name->qty in trolley/basket
        record GroceryItem(string Name, int Price, int Aisle);

        static GroceryItem[] groceryItems = {
            // Aisle 1 — Produce & Meat
            new("Apple",        2,  1),
            new("Orange",       2,  1),
            new("Banana",       1,  1),
            new("Carrot",       1,  1),
            new("Potato",       1,  1),
            new("Onion",        1,  1),
            new("Corn",         2,  1),
            new("Raw Meat",     5,  1),
            new("Bacon",        4,  1),
            new("Eggs",         3,  1),
            // Aisle 2 — Dairy & Bread
            new("Milk",         3,  2),
            new("Cheese",       4,  2),
            new("Butter",       3,  2),
            new("Bread",        3,  2),
            new("Flour",        2,  2),
            new("Pasta",        2,  2),
            // Aisle 3 — Pantry
            new("Tomato Sauce", 3,  3),
            new("Ham",          4,  3),
            new("Cereal",       4,  3),
            new("Juice",        3,  3),
        };

        //Toolbelt
        static int toolbarSelectedSlot = 0;
        static string[] toolbarSlots = new string[8]; // null = empty
        static int[] toolbarCounts = new int[8];
        static bool torchActive = false;
        static RenderTexture2D nightMask; // holds the punched-out darkness overlay; loaded in Main(), unloaded on close
        const float LightRadius = 350f; 

        // BACKPACK
        // General backpack — holds any named item (cooked food, groceries, ashes, etc.)
        static Dictionary<string,int> backpack = new Dictionary<string,int>();

        static int backpackCapacity = 20;   // was: const int MaxInventorySlots = 20;
        static readonly (int cost, int slots)[] backpackUpgrades = {
            (0,    20),   // starting bag
            (500,  30),
            (1500, 45),
            (4000, 60),
        };
        static int backpackTier = 0;

        // Scroll wheel
        static float pmRelScrollY = 0f;
        static float pmUnlScrollY = 0f;

        // Dungeon
        static Dungeon activeDungeon = new Dungeon();
        static bool dungeonQuitConfirm = false;
        static List<(Vector2 pos, string type, string name)> dungeonEntrances = new();
        static List<NPC> dbarTableNPCs = new();

        // ── DIVE STATE (2D side-scroller) ──
        static Vector2 diveSwimPos = new Vector2(640, 100);
        static float diveSurfaceY = 80f;      // water surface line near top
        static float diveBottomY = 4000f;     // reach this depth → underwater world
        static Camera2D diveCamera = new Camera2D();
        static Vector2 diveWorldReturnPos;    // where to put player back in World on surfacing
        static Player.FacingDirection diveFacing = Player.FacingDirection.Right;

        // ── UNDERWATER WORLD ──
        static Vector2 underwaterPos = new Vector2(10000, 10000);   // player pos in underwater world
        static Camera2D underwaterCamera = new Camera2D();
        static Player.FacingDirection underwaterFacing = Player.FacingDirection.Down;
        const int UnderwaterSize = 20000;
        static Vector2 underwaterWorldReturnPos;   // where to drop player back in World on exit

        // ── SPACE WORLD ──
        static Vector2 spacePos = new Vector2(10000, 10000);
        static Vector2 spaceVel = Vector2.Zero;
        static float spaceAngle = 0f;             // ship heading in radians
        static Camera2D spaceCamera = new Camera2D();
        const int SpaceSize = 20000;
        static Vector2 spaceWorldReturnPos;
        // ── ROCKET (space entry point) ──
        static Vector2 rocketPosition = new Vector2(-14300, 6500);   // pick a spot on your map

        // Player menu
         static bool playerMenuOpen = false;
        enum PlayerMenuTab { Identity, Stats, Crafting, Achievements, Bestiary, Unlocks, Relationships, Collectables, Collection }
        static PlayerMenuTab playerMenuTab = PlayerMenuTab.Identity; 
        static float idColScrollY = 0f; 

        // MINI GAMES
        public static PokieMachine activePokieMachine = new PokieMachine();
        public static DartsGame activeDartsGame = new DartsGame();
        enum MinigameType { None, Pokie, Pool, Darts, Bowling, Claw, Pinball, AirHockey, PianoTiles, Flappy, MiniGolf}
        static MinigameType activeMinigameType = MinigameType.None;
        static PoolGame activePoolGame = new PoolGame();
        static BowlingGame activeBowlingGame = new BowlingGame();
        static ClawMachine activeClawMachine = new ClawMachine();
        static PinballGame activePinballGame = new PinballGame();
        static AirHockeyGame activeAirHockeyGame = new AirHockeyGame();
        static PianoTilesGame activePianoTilesGame = new PianoTilesGame();
        static FlappyBirdGame activeFlappyGame = new FlappyBirdGame();
        static MiniGolfGame activeMiniGolfGame = new MiniGolfGame();
        static NPC dbarPokieNPC = null;

        // Dealer UI state
        enum DealerType { None, Bike, Barn, Car }
        static bool dealerUIOpen = false;
        static DealerType currentDealerType = DealerType.None;
        static Building currentDealerBuilding = null;
        static int dealerSelectedIndex = 0;
        static int dealerScrollOffset = 0; // for paging if >4 options
        public static List<RockObject> rocks = new();
        static List<Vehicle> dealerVehicleOptions = new();
        static List<Rideable> dealerBikeOptions = new();
        static List<Rideable> dealerBarnOptions = new(); // horses as barn animals
        public static FenceManager fenceManager = new FenceManager();

        // INVENTORY
        static int invSelectedIndex = -1;        // which inventory slot is selected
        static string invSelectedName = "";      // the item's name
        static int invSelectedCount = 0;
        static List<DroppedItem> droppedItems = new();
        static bool dropConfirmOpen = false;
        static string dropConfirmItem = "";
        static bool dropQtyOpen = false;
        static string dropQtyItem = "";
        static int dropQtyAvailable = 0;

        // Master list
static List<StatItemDef> statItems = new() {
    new("Logs", () => player.Logs, d => player.Logs += d),
    new("Birch Logs", () => player.BirchLogs, d => player.BirchLogs += d),
    new("Oak Logs", () => player.OakLogs, d => player.OakLogs += d),
    new("Pine Logs", () => player.PineLogs, d => player.PineLogs += d),
    new("Arctic Logs", () => player.ArcticLogs, d => player.ArcticLogs += d),
    new("Dead Wood", () => player.DeadWood, d => player.DeadWood += d),
    new("Fish", () => player.Fish, d => player.Fish += d),
    new("Bones", () => player.Bones, d => player.Bones += d),
    new("Fur", () => player.Fur, d => player.Fur += d),
    new("Stingers", () => player.Stingers, d => player.Stingers += d),
    new("Pelts", () => player.BearPelts, d => player.BearPelts += d),
    new("Dog Fangs", () => player.DogFangs, d => player.DogFangs += d),
    new("Wolf Claw", () => player.WolfClaws, d => player.WolfClaws += d),
    new("Venom Sac", () => player.VenomSacs, d => player.VenomSacs += d),
    new("Crab Claw", () => player.CrabClaws, d => player.CrabClaws += d),
    new("Bear Claw", () => player.BearClaws, d => player.BearClaws += d),
    new("Crab Shell", () => player.CrabShells, d => player.CrabShells += d),
    new("Shark Fin", () => player.SharkFins, d => player.SharkFins += d),
    new("Shark Tooth", () => player.SharkTeeth, d => player.SharkTeeth += d),
    new("Snake Skin", () => player.SnakeSkins, d => player.SnakeSkins += d),
    new("Snake Fang", () => player.SnakeFangs, d => player.SnakeFangs += d),
    new("Croc Scale", () => player.CrocScales, d => player.CrocScales += d),
    new("Croc Tooth", () => player.CrocTeeth, d => player.CrocTeeth += d),
    new("Lizard Scale", () => player.LizardScales, d => player.LizardScales += d),
    new("Ember Stone", () => player.EmberStones, d => player.EmberStones += d),
    new("Magma Shard", () => player.MagmaShards, d => player.MagmaShards += d),
    new("Lava Core", () => player.LavaCores, d => player.LavaCores += d),
    new("Feather", () => player.Feathers, d => player.Feathers += d),
    new("Eagle Talon", () => player.EagleTalons, d => player.EagleTalons += d),
    new("Horn", () => player.Horns, d => player.Horns += d),
    new("Goat Hoof", () => player.GoatHooves, d => player.GoatHooves += d),
    new("Stone", () => player.StoneOre, d => player.StoneOre += d),
    new("Copper Ore", () => player.CopperOre, d => player.CopperOre += d),
    new("Iron Ore", () => player.IronOre, d => player.IronOre += d),
    new("Gold Ore", () => player.GoldOre, d => player.GoldOre += d),
    new("Crystal", () => player.Crystals, d => player.Crystals += d),
};

        // IsToolItem covers Axe/Pickaxe/Rod/Net/Sword/Stick/Bow/Crossbow
        // GetItemSlot != null covers armor & weapons
        // Arrows/Bolts are ammo, equipped into the ammo slot

        // IsToolItem() — CHANGED
static readonly string[] metalPrefixes = { "Copper", "Iron", "Gold", "Crystal" };

// True only for items that have a destination OTHER than the toolbar.

        // Shops
        static bool magicShopOpen = false;
        static bool rangingShopOpen = false;
        static bool farmingShopOpen = false;

        // FARMING
        class LivestockPen
        {
            public Vector2 Position;         // world position of the pen
            public string Animal = "";       // "Chicken", "Cow", ...
            public string Produce = "";      // "Eggs", "Milk", ...
            public string Feed = "";         // required feed item
            public float Cycle = 60f;        // seconds between yields when fed
            public float Timer = 0f;
            public bool Fed = false;         // must be fed to progress
            public bool ReadyToHarvest = false;
        }
        static List<FarmPlot> farmPlots = new();
        // ── LIVESTOCK (barn animals) ──
        static List<LivestockPen> livestockPens = new();
        static bool barnShopOpen = false;
        static bool zooPaid = false;
        // name, price, produce item, seconds to produce, feed item
        static (string animal, int price, string produce, float cycle, string feed)[] barnStock = {
            ("Chicken", 120,  "Eggs",  45f, "Grain Feed"),
            ("Cow",     650,  "Milk",  90f, "Hay Bale"),
            ("Sheep",   400,  "Wool",  120f,"Hay Bale"),
            ("Pig",     500,  "Bacon", 150f,"Grain Feed"),
            ("Goat",    350,  "Goat Milk", 100f, "Hay Bale"),
        };
        static (string item, int price)[] barnSupplies = {
            ("Grain Feed", 8),
            ("Hay Bale", 12),
            ("Animal Trough", 40),
        };
        static Dictionary<string,string> seedToCrop = new() {
        { "Wheat Seeds",   "Wheat" },
        { "Carrot Seeds",  "Carrot" },
        { "Potato Seeds",  "Potato" },
        { "Tomato Seeds",  "Tomato" },
        };
        static Dictionary<string,float> cropGrowDuration = new() {
            { "Wheat", 30f }, { "Carrot", 25f }, { "Potato", 35f }, { "Tomato", 40f },
        };

        // ── SEASONAL CONTENT ──
        // Crops: which seasons they thrive in (southern hemisphere)
        static Dictionary<string, string[]> cropSeasons = new()
        {
            { "Wheat",  new[] { "Spring", "Summer" } },
            { "Carrot", new[] { "Autumn", "Spring" } },
            { "Potato", new[] { "Spring", "Summer", "Autumn" } },
            { "Tomato", new[] { "Summer" } },
        };

        static bool IsCropInSeason(string crop)
        {
            if (!cropSeasons.TryGetValue(crop, out var validSeasons)) return true;
            return validSeasons.Contains(GetSeasonString());
        }

        static float GetSeasonalGrowthMultiplier(string crop)
        {
            if (IsCropInSeason(crop)) return 1.0f;
            return 0.5f;  // out of season = half speed
        }

        // Fish: seasonal weight modifiers (multiplied onto base weight)
        static Dictionary<string, string[]> fishSeasons = new()
        {
            { "Carp",        new[] { "Summer", "Autumn" } },
            { "Perch",       new[] { "Spring", "Summer" } },
            { "Bass",        new[] { "Summer" } },
            { "Catfish",     new[] { "Summer", "Autumn" } },
            { "Golden Carp", new[] { "Autumn" } },
            { "Trout",       new[] { "Autumn", "Winter" } },
            { "Salmon",      new[] { "Autumn" } },
            { "Eel",         new[] { "Winter", "Spring" } },
            { "Crayfish",    new[] { "Spring", "Summer" } },
            { "Sturgeon",    new[] { "Winter" } },
        };

        static bool IsFishInSeason(string fishName)
        {
            if (!fishSeasons.TryGetValue(fishName, out var s)) return true;
            return s.Contains(GetSeasonString());
        }

        // World events: season restrictions
        static Dictionary<WorldEventType, string[]> eventSeasons = new()
        {
            { WorldEventType.Blizzard,          new[] { "Winter" } },
            { WorldEventType.ForestFire,        new[] { "Summer" } },
            { WorldEventType.HarvestFestival,   new[] { "Autumn" } },
            { WorldEventType.FishingTournament, new[] { "Summer", "Autumn" } },
        };
        static List<FruitTree> fruitTrees = new();
        static float[] toolbarWaterCharge = new float[8];   
        const float WateringCanMaxUses = 20f; 
        static float farmToolCooldown = 0f;
        const float FarmToolCooldownDuration = 0.2f;
        record FarmToolTier(string Name, int Range, int MinLevel, int MaxUses);
        static FarmToolTier[] spadeTiers = {
            new("Spade",         1, 1,  0),   // Range = plots tillable per use
            new("Iron Spade",    2, 15, 0),
            new("Steel Spade",   3, 30, 0),
        };
        static FarmToolTier[] wateringCanTiers = {
            new("Watering Can",       1, 1,  5),
            new("Iron Watering Can",  2, 15, 8),
            new("Steel Watering Can", 3, 30, 12),
        };
        static Vector2[] treePlotPositions = {
            new Vector2(-2540, -9870),
            new Vector2(-2540, -9770),
        };
        static bool treeChopConfirmOpen = false;
        static FruitTree treeChopTarget = null;

        // Seeds 
        static Dictionary<string,int> seedUnlockLevel = new() {
        { "Wheat Seeds",  1 }, // Seeds
        { "Carrot Seeds", 1 },
        { "Potato Seeds", 10 },
        { "Tomato Seeds", 20 },
        { "Apple Tree Seed",  5 }, // Fruit tree seeds
        { "Banana Tree Seed", 8 },
                };

        // Eggs
        static bool IsEgg(string item) =>
            item == "Colossus Egg" || item == "Titan Egg";

        static Color EggColor(string egg) => egg switch
        {
            "Colossus Egg" => new Color((byte)90,(byte)40,(byte)120,(byte)255),
            "Titan Egg"    => new Color((byte)140,(byte)20,(byte)20,(byte)255),
            _              => new Color((byte)200,(byte)200,(byte)200,(byte)255)
        };

        // Loot drops
        static Dictionary<string,string> lootDropToItemName = new() {
            { "Bone",        "Bones" },
            { "Fur",         "Fur" },
            { "Stinger",     "Stingers" },
            { "Bear Pelt",   "Pelts" },
            { "Dog Fang",    "Dog Fangs" },
            { "Wolf Claw",   "Wolf Claw" },
            { "Venom Sac",   "Venom Sac" },
            { "Crab Claw",   "Crab Claw" },
            { "Bear Claw",   "Bear Claw" },
            { "Crab Shell",  "Crab Shell" },
            { "Shark Fin",   "Shark Fin" },
            { "Shark Tooth", "Shark Tooth" },
            { "Snake Skin",  "Snake Skin" },
            { "Snake Fang",  "Snake Fang" },
            { "Croc Scale",  "Croc Scale" },
            { "Croc Tooth",  "Croc Tooth" },
            { "Lizard Scale","Lizard Scale" },
            { "Ember Stone", "Ember Stone" },
            { "Magma Shard", "Magma Shard" },
            { "Lava Core",   "Lava Core" },
            { "Feather",     "Feather" },
            { "Eagle Talon", "Eagle Talon" },
            { "Horn",        "Horn" },
            { "Goat Hoof",   "Goat Hoof" },
        };

static readonly Dictionary<string, List<LootEntry>> lootTables = new()
{
    ["emerald_armor"] = new() {
        new LootEntry { Item = "Emerald Helmet",     Min = 1, Max = 1, Weight = 20 },
        new LootEntry { Item = "Emerald Chestplate", Min = 1, Max = 1, Weight = 10 },
        new LootEntry { Item = "Emerald Leggings",   Min = 1, Max = 1, Weight = 15 },
        new LootEntry { Item = "Emerald Boots",      Min = 1, Max = 1, Weight = 25 },
        new LootEntry { Item = "Emerald Gauntlets",  Min = 1, Max = 1, Weight = 25 },
        new LootEntry { Item = "Emerald Shield",     Min = 1, Max = 1, Weight = 5 },
    },
    ["boss_general"] = new() {
        new LootEntry { Item = "Cooked Fish", Min = 10, Max = 10, Weight = 20 },
        new LootEntry { Item = "Raw Fish",    Min = 20, Max = 20, Weight = 20 },
        new LootEntry { Item = "Iron Bar",    Min = 5,  Max = 5,  Weight = 15 },
        new LootEntry { Item = "Steel Bar",   Min = 3,  Max = 3,  Weight = 10 },
        new LootEntry { Item = "Logs",        Min = 30, Max = 50, Weight = 15 },
        new LootEntry { Item = "Crystal",     Min = 2,  Max = 4,  Weight = 10 },
        new LootEntry { Item = "Money",       Min = 200, Max = 500, Weight = 10 },
    },
};

// Roll one entry from a named table → (item, count)
static (string item, int count) RollLootTable(string tableName)
{
    var table = lootTables[tableName];
    int total = table.Sum(e => e.Weight);
    int roll = Raylib.GetRandomValue(1, total);
    foreach (var e in table)
    {
        roll -= e.Weight;
        if (roll <= 0) return (e.Item, Raylib.GetRandomValue(e.Min, e.Max));
    }
    var last = table[^1];
    return (last.Item, last.Min);
}  

static float arenaBossHealth, arenaBossMaxHealth = 3000f;
static Vector2 arenaBossPos;
static bool arenaBossDead = false;
static float orbTimer, minionTimer, spikeTimer, bossContactCd;
static List<ArenaOrb> arenaOrbs = new();
static List<ArenaSpike> arenaSpikes = new();
static List<ArenaMinion> arenaMinions = new();
static Vector2 bossArenaEntrance = new Vector2(5000, -3000);   // world entrance — move to suit
const int BossSize = 160;
static Rectangle ArenaBossBounds => new Rectangle(arenaBossPos.X - BossSize/2, arenaBossPos.Y - BossSize/2, BossSize, BossSize);

static readonly List<Rectangle> arenaObjects = new();

// Call when a player attack lands on the boss (see integration note below)

        //Tools in toolbelt
        static Vector2 axePosition = new Vector2(-2596, -9121);
        static bool axePickedUp = false;
        static Vector2 pickaxePosition = new Vector2(-2440, -8659);
        static bool pickaxePickedUp = false;
        static Vector2 fishingRodPosition = new Vector2(-2832, -8136);  // near a lake
        static bool fishingRodPickedUp = false;
        static Vector2 fishingNetPosition = new Vector2(-2358, -8207);
        static bool fishingNetPickedUp = false;
        static Vector2 torchPosition = new Vector2(-900, 500);
        static bool torchPickedUp = false;
        static Vector2 spadePosition = new Vector2(-1476, -9795);
        static bool spadePickedUp = false;
        static Vector2 wateringCanPosition = new Vector2(-1261, -9795);
        static bool wateringCanPickedUp = false;
        static Vector2 wheatSeedsPosition = new Vector2(-1200, -9620);
        static bool wheatSeedsPickedUp = false;

        // WEAPONS

        static Vector2 stickPosition = new Vector2(-2076, -7449);
        static bool stickPickedUp = false;
        static Vector2 swordPosition = new Vector2(-1170, -7740);
        static bool swordPickedUp = false;
        static bool bowPickedUp = false;
        static bool crossbowPickedUp = false;
        static Vector2 bowSpawnPos = new Vector2(-2559, -6457);
        static Vector2 crossbowSpawnPos = new Vector2(1000, -400);
        static bool staffPickedUp = false;
        static Vector2 staffSpawnPos = new Vector2(-2367, -6458);   // place wherever suits the tutorial
        static string staffType = "Rock Staff";                  // the starter staff given

        // PLAYER HOUSING 
        static List<(int x, int y)> ownedHousePlots = new List<(int x, int y)>();
        static bool housePurchased => ownedHousePlots.Count > 0; // keep for backward compat
        static int activeHousePlotIndex = 0;
        static bool   landForSaleUIOpen   = false;
        static bool   houseMenuOpen       = false;
        static int    houseMenuSelected   = 0;
        static List<HouseData> houseDataList = new List<HouseData>();
        static string houseRoofColor = "Grey";
        static int    furnitureCategoryIndex = 0;
        static int    furnitureShopScroll    = 0;
        static int    heldFurnitureRotation  = 0;
        static float bedMenuInputCooldown = 0f;
        static bool   houseBuildingActive  = false;
        static float  houseBuildingTimer   = 0f;
        static float  houseBuildingDuration = 4f;
        static float  houseBuildingAlpha   = 0f;
        static bool   houseBuildingFadeIn  = true;
        static HouseData ActiveHouseData =>
        activeHousePlotIndex >= 0 && activeHousePlotIndex < houseDataList.Count
        ? houseDataList[activeHousePlotIndex]
        : null;

        // Available land plots (world position, price)
        static (float x, float y, int price, string label, string houseType)[] landPlots = {
            (3000,  1800,  8000,  "Safe Zone Plot A", "Standard"),
            (1900, -1200,  8000,  "Safe Zone Plot B", "Standard"),
            (5000,  2000,  8000,  "Safe Zone Plot C", "Standard"),
            (-1700, 1700,  7500,  "Safe Zone Plot D", "Standard"),
            (-2650,  800,  7500,  "Safe Zone Plot E", "Standard"),
            (3600,  2400, 15000,  "Countryside Farm Plot",  "Farmhouse"),   // ADDED — tweak coords to fit your map
            (4200,  3400, 22000,  "Beachfront Plot",        "OceanHouse"),  // ADDED — place beside your ocean/dock area
            (-3400, -600, 45000,  "Hilltop Estate",         "Mansion"),     // ADDED
        };
        static int selectedPlot = -1;
        static bool   furniturePlaceMode  = false;  // true when holding a piece to place
        static string heldFurnitureType   = "";     // which piece is being held
        static int    heldFurnitureIndex  = -1;     // index in houseFurniture list (-1 = new)
        static int    furnitureCursorX    = 640;    // current ghost position
        static int    furnitureCursorY    = 400;
        static int    furnitureGridSnap   = 40;     // snap to 40px grid

        public static bool  testTransitionActive   = false;
        static float testTransitionTimer    = 0f;
        static float testTransitionDuration = 1.5f;
        static float testTransitionAlpha    = 0f;
        static bool  testTransitionFadeIn   = true;
        static Action testTransitionCallback = null;
        static string testTransitionMessage = "Preparing your test...";

        // ── SKILL MASTERY PERKS ──
record SkillPerk(int Level, string Name, string Description);

static readonly Dictionary<string, SkillPerk[]> skillPerks = new()
{
    ["Woodcutting"] = new SkillPerk[] {
        new(5,  "Quick Chop",       "Trees take 1 fewer hit"),
        new(15, "Hardwood Hands",   "10% chance of bonus log"),
        new(25, "Lumber Efficiency","Double logs from Dead Wood"),
        new(40, "Forest Sense",     "Trees highlight when nearby"),
        new(50, "Master Lumberjack","All trees chop 1 hit faster"),
        new(75, "Splinter Shield",  "+5 max HP permanently"),
        new(100,"Living Legend",    "Triple logs from all trees"),
    },
    ["Fishing"] = new SkillPerk[] {
        new(5,  "Patient Angler",   "Bite window +0.3s longer"),
        new(15, "Keen Eye",         "Green zone 15% wider"),
        new(25, "Double Catch",     "20% chance to catch 2 fish"),
        new(40, "Deep Lure",        "Rare fish chance increased"),
        new(50, "Net Master",       "Nets catch +1 extra fish"),
        new(75, "Ocean Whisper",    "Perfect catch zone doubled"),
        new(100,"Sea King",         "All fish worth 2x value"),
    },
    ["Mining"] = new SkillPerk[] {
        new(5,  "Prospect",         "See ore type before mining"),
        new(15, "Heavy Swing",      "10% chance for bonus ore"),
        new(25, "Vein Finder",      "Rocks drop +1 ore"),
        new(40, "Gem Sense",        "Crystal drop chance +5%"),
        new(50, "Auto-Smelt",       "Copper ore auto-becomes bars"),
        new(75, "Deep Strike",      "+2 ore from every rock"),
        new(100,"Earth Shaper",     "All ore doubled"),
    },
    ["Combat"] = new SkillPerk[] {
        new(5,  "Quick Recovery",   "Damage cooldown -20%"),
        new(15, "Hard Hitter",      "+2 base melee damage"),
        new(25, "Combo Fighter",    "Combo window +0.5s"),
        new(40, "Battle Cry",       "Enemies flee 2s on kill"),
        new(50, "Dodge Roll",       "Shift = dodge (brief invuln)"),
        new(75, "Berserker",        "+25% damage below half HP"),
        new(100,"Immortal Warrior", "Survive lethal hit once/day"),
    },
    ["Cooking"] = new SkillPerk[] {
        new(5,  "Sous Chef",        "Meals heal +5 extra HP"),
        new(15, "Seasoned Cook",    "10% chance to cook 2x"),
        new(25, "Flavour Master",   "All food heals +15 HP"),
        new(50, "Iron Stomach",     "Raw fish heals instead of nothing"),
        new(75, "Feast Maker",      "Cook 3x meals at once"),
        new(100,"Legendary Chef",   "Meals give temp +20 max HP"),
    },
    ["Farming"] = new SkillPerk[] {
        new(5,  "Green Sprout",     "Crops grow 10% faster"),
        new(15, "Fertile Hands",    "Watered crops grow 20% faster"),
        new(25, "Harvest Plus",     "25% chance for double harvest"),
        new(40, "Rain Dancer",      "Crops grow in rain without watering"),
        new(50, "Seed Saver",       "20% chance to keep seeds on plant"),
        new(75, "Season Master",    "Crops grow in any season"),
        new(100,"Golden Harvest",   "All harvests tripled"),
    },
    ["Athletics"] = new SkillPerk[] {
        new(10, "Light Feet",       "+10% move speed"),
        new(25, "Marathon",         "Sprint drains no stamina"),
        new(50, "Parkour",          "Walk speed +20%"),
        new(75, "Wind Runner",      "+30% speed total"),
        new(100,"Blur",             "Fastest in the world"),
    },
    ["Strength"] = new SkillPerk[] {
        new(10, "Pack Mule",        "+5 backpack slots"),
        new(25, "Power Lift",       "+10 backpack slots"),
        new(50, "Iron Grip",        "Swing speed +20%"),
        new(75, "Titan",            "+10 max HP permanently"),
        new(100,"Colossus",         "+25 max HP, +20 slots"),
    },
    ["Ranged"] = new SkillPerk[] {
        new(5,  "Steady Aim",       "+10% arrow accuracy"),
        new(15, "Quick Nock",       "Faster arrow reload"),
        new(25, "Piercing Shot",    "Arrows deal +3 damage"),
        new(50, "Eagle Eye",        "+50% projectile range"),
        new(75, "Multi-Shot",       "10% chance to fire 2 arrows"),
        new(100,"Hawkeye",          "All ranged damage doubled"),
    },
    ["Driving"] = new SkillPerk[] {
        new(10, "Fuel Saver",       "Vehicles use 10% less fuel"),
        new(25, "Speed Demon",      "+15% vehicle speed"),
        new(50, "Road King",        "+25% speed on roads"),
        new(75, "Drift Master",     "Off-road penalty halved"),
        new(100,"Grand Tourer",     "All vehicle speed +40%"),
    },
    ["Riding"] = new SkillPerk[] {
        new(10, "Steady Rider",     "Mount stamina drains slower"),
        new(25, "Bond",             "Mount speed +10%"),
        new(50, "Gallop",           "Mount speed +20%"),
        new(100,"Horse Whisperer",  "Mounts never tire"),
    },
    ["Swimming"] = new SkillPerk[] {
        new(10, "Strong Stroke",    "+15% swim speed"),
        new(25, "Deep Breath",      "Dive time +50%"),
        new(50, "Aquatic",          "Swim speed +30%"),
        new(100,"Poseidon",         "Swim as fast as walking"),
    },
    ["Gambling"] = new SkillPerk[] {
        new(10, "Lucky Streak",     "+5% win chance"),
        new(25, "Card Counter",     "+10% win chance"),
        new(50, "High Roller",      "Max bets doubled"),
        new(100,"House Edge",       "Almost never lose"),
    },
};

// helper: check if player has a specific perk unlocked
static bool HasPerk(string skill, int level)
{
    if (!skillPerks.TryGetValue(skill, out var perks)) return false;
    int playerLevel = skill switch {
        "Woodcutting" => player.WoodcuttingLevel, "Fishing" => player.FishingLevel,
        "Mining" => player.MiningLevel, "Combat" => player.CombatLevel,
        "Cooking" => player.CookingLevel, "Farming" => player.FarmingLevel,
        "Athletics" => player.AthleticsLevel, "Strength" => player.StrengthLevel,
        "Ranged" => player.RangedLevel, "Driving" => player.DrivingLevel,
        "Riding" => player.RidingLevel, "Swimming" => player.SwimmingLevel,
        "Gambling" => player.GamblingLevel, _ => 0
    };
    return playerLevel >= level;
}

        // ── REPUTATION / TOWN STANDING ──
        static readonly (int threshold, string title, string perk)[] reputationTiers =
        {
            (0,    "Newcomer",     ""),
            (100,  "Resident",     "5% shop discount"),
            (250,  "Trusted",      "Job pay +20%"),
            (500,  "Respected",    "10% shop discount"),
            (1000, "Honoured",     "15% shop discount, better NPC gifts"),
            (2500, "Legend",       "20% shop discount, max job pay bonus"),
        };

        static (string title, string perk) GetReputationTier(int rep)
        {
            for (int i = reputationTiers.Length - 1; i >= 0; i--)
                if (rep >= reputationTiers[i].threshold) return (reputationTiers[i].title, reputationTiers[i].perk);
            return (reputationTiers[0].title, reputationTiers[0].perk);
        }

        static (int current, int next) GetReputationProgress(int rep)
        {
            for (int i = reputationTiers.Length - 1; i >= 0; i--)
                if (rep >= reputationTiers[i].threshold)
                {
                    int cur = reputationTiers[i].threshold;
                    int nxt = i < reputationTiers.Length - 1 ? reputationTiers[i + 1].threshold : cur;
                    return (cur, nxt);
                }
            return (0, reputationTiers[1].threshold);
        }

        static float GetReputationShopDiscount()
        {
            int rep = player.Reputation;
            if (rep >= 2500) return 0.20f;
            if (rep >= 1000) return 0.15f;
            if (rep >= 500)  return 0.10f;
            if (rep >= 100)  return 0.05f;
            return 0f;
        }

        static float GetReputationJobBonus()
        {
            int rep = player.Reputation;
            if (rep >= 2500) return 0.40f;
            if (rep >= 250)  return 0.20f;
            return 0f;
        }

        static void AddReputation(int amount, string reason)
        {
            if (amount <= 0) return;
            string oldTitle = GetReputationTier(player.Reputation).title;
            player.Reputation += amount;
            string newTitle = GetReputationTier(player.Reputation).title;
            if (newTitle != oldTitle)
                ShowLevelUp($"Reputation: {newTitle}!", 0);
            else
                ShowNotification($"+{amount} Reputation ({reason})");
        }

        // ── NPC FAVOR SYSTEM ──
        static readonly (string item, int minQty, int maxQty, string skill, int moneyReward, string dialogue)[] favorPool =
        {
            ("Fish",        3, 8,  "Fishing",     80,  "Could you catch me some fish? I'm starving!"),
            ("Logs",        8, 15, "Woodcutting",  90,  "I need some logs for a project. Can you help?"),
            ("Cooked Fish", 2, 5,  "Cooking",     100, "I'd love some cooked fish. I can't cook to save my life!"),
            ("Stone",       5, 12, "Mining",       85,  "I need some stone. Could you mine some for me?"),
            ("Iron Ore",    3, 6,  "Mining",      120, "I've been looking for iron ore. Know where to find some?"),
            ("Copper Ore",  3, 8,  "Mining",      100, "I could really use some copper ore right now."),
            ("Gold Ore",    2, 4,  "Mining",      180, "Gold ore is so hard to find. Any chance you have some?"),
            ("Bones",       4, 8,  "Combat",      100, "I need bones for a ritual. Can you get some from monsters?"),
            ("Fur",         3, 6,  "Combat",      110, "I want to make something warm. Could you bring me some fur?"),
            ("Crystal",     1, 3,  "Mining",      200, "I've always wanted a crystal. Could you find one?"),
            ("Oak Logs",    5, 10, "Woodcutting", 120, "I need strong oak logs. The regular ones won't do!"),
            ("Birch Logs",  5, 10, "Woodcutting", 110, "Birch logs would be perfect for what I'm building."),
            ("Wheat",       4, 8,  "Farming",     100, "My pantry's empty. Could you grow me some wheat?"),
            ("Feather",     2, 5,  "Combat",      100, "I need feathers for fletching. Can you get some?"),
            ("Wolf Claw",   2, 4,  "Combat",      150, "Wolf claws make great tools. I'll pay well for some!"),
            ("Ember Stone",  1, 3, "Combat",      160, "Ember stones are so useful. Could you find some?"),
            ("Cooked Meat", 2, 4,  "Cooking",     130, "I'd love a proper cooked meal. Can you make some?"),
            ("Copper Bar",  2, 4,  "Crafting",    140, "I need copper bars. Can you smelt some for me?"),
            ("Iron Bar",    2, 3,  "Crafting",    170, "Iron bars are hard to come by. I'll make it worth your while!"),
        };

        static Random favorRng = new Random();

        static void RollNpcFavors()
        {
            foreach (var f in friendNPCs)
            {
                if (f.ActiveFavor != null) continue;            // already has one
                if (f.Friendship < 30) continue;                // must be at least Friends
                if (f.IsChild) continue;                        // kids don't ask favors
                if (f.FavorCooldownDays > 0) { f.FavorCooldownDays--; continue; }

                // 40% chance each eligible NPC rolls a favor on a new day
                if (favorRng.Next(100) >= 40) continue;

                // pick a favor — prefer items the NPC likes
                var candidates = new List<int>();
                var fallbacks = new List<int>();
                for (int i = 0; i < favorPool.Length; i++)
                {
                    if (f.Likes.Any(l => favorPool[i].item.Contains(l)))
                        candidates.Add(i);
                    else
                        fallbacks.Add(i);
                }
                if (candidates.Count == 0) candidates = fallbacks;
                if (candidates.Count == 0) continue;

                int pick = candidates[favorRng.Next(candidates.Count)];
                var (item, minQ, maxQ, skill, money, dialogue) = favorPool[pick];
                int qty = favorRng.Next(minQ, maxQ + 1);

                // scale reward by friendship tier
                int friendBonus = f.Friendship >= 90 ? 60 : f.Friendship >= 60 ? 30 : 0;
                int friendshipGain = f.Friendship >= 90 ? 5 : f.Friendship >= 60 ? 8 : 10;

                f.ActiveFavor = new NpcFavor
                {
                    Description = $"Bring {qty} {item}",
                    ItemNeeded = item,
                    AmountNeeded = qty,
                    AmountDelivered = 0,
                    RewardType = "money",
                    RewardAmount = money + friendBonus,
                    FriendshipGain = friendshipGain,
                    Dialogue = dialogue,
                    Completed = false,
                };
            }
        }

        static void TryDeliverFavor(FriendNPC f)
        {
            var fav = f.ActiveFavor;
            if (fav == null || fav.Completed) return;

            int have = GetItemCount(fav.ItemNeeded);
            int remaining = fav.AmountNeeded - fav.AmountDelivered;
            int toDeliver = Math.Min(have, remaining);
            if (toDeliver <= 0)
            {
                ShowNotification($"You don't have any {fav.ItemNeeded} to give {f.Name}.");
                return;
            }

            // consume items
            for (int i = 0; i < toDeliver; i++)
                RemoveOneItem(fav.ItemNeeded);
            fav.AmountDelivered += toDeliver;

            if (fav.AmountDelivered >= fav.AmountNeeded)
            {
                fav.Completed = true;
                player.Money += fav.RewardAmount;
                f.Friendship = Math.Min(100, f.Friendship + fav.FriendshipGain);
                f.FavorsCompleted++;
                AddReputation(20, $"Favor: {f.Name}");
                ShowNotification($"{f.Name}: \"Legend, cheers!\" +${fav.RewardAmount} +{fav.FriendshipGain} friendship");
                f.ActiveFavor = null;
                f.FavorCooldownDays = 2 + favorRng.Next(3); // 2-4 day cooldown
            }
            else
            {
                int left = fav.AmountNeeded - fav.AmountDelivered;
                ShowNotification($"Gave {toDeliver} {fav.ItemNeeded} to {f.Name}. {left} more needed.");
            }
        }

        // ── SKILL SYNERGY SYSTEM ──
        record SynergyDef(string Name, string Description, string Icon,
            (string skill, int level)[] Requirements, Color TierColor);

        static readonly SynergyDef[] synergies =
        {
            new("Gourmet Angler",  "Cooked fish heals +50%",          "fish",
                new[]{ ("Fishing", 25), ("Cooking", 25) },
                new Color((byte)80,(byte)200,(byte)220,(byte)255)),

            new("Master Smith",    "Melee weapon damage +15%",        "anvil",
                new[]{ ("Mining", 25), ("Crafting", 25) },
                new Color((byte)180,(byte)130,(byte)60,(byte)255)),

            new("Nature's Hand",   "Crops & trees grow 20% faster",   "leaf",
                new[]{ ("Farming", 25), ("Woodcutting", 25) },
                new Color((byte)80,(byte)180,(byte)60,(byte)255)),

            new("Sharpshooter",    "Ranged damage +20%",              "bow",
                new[]{ ("Combat", 25), ("Ranged", 25) },
                new Color((byte)220,(byte)160,(byte)40,(byte)255)),

            new("Iron Will",       "All damage taken reduced 10%",    "shield",
                new[]{ ("Combat", 50), ("Defence", 25) },
                new Color((byte)140,(byte)140,(byte)160,(byte)255)),

            new("Sea Lord",        "Swim speed +25%",                 "wave",
                new[]{ ("Fishing", 50), ("Swimming", 25), ("Boating", 10) },
                new Color((byte)40,(byte)140,(byte)220,(byte)255)),

            new("Trailblazer",     "Walk & ride speed +10%",          "boot",
                new[]{ ("Athletics", 30), ("Riding", 20) },
                new Color((byte)200,(byte)100,(byte)40,(byte)255)),

            new("Arcane Warrior",  "Spell damage +25%",               "star",
                new[]{ ("Elemental", 30), ("Combat", 30) },
                new Color((byte)160,(byte)80,(byte)220,(byte)255)),

            new("Jack of All Trades", "+5% XP to all skills",         "gem",
                new[]{ ("Woodcutting", 25), ("Fishing", 25), ("Mining", 25), ("Combat", 25), ("Cooking", 25) },
                new Color((byte)200,(byte)200,(byte)80,(byte)255)),

            new("Renaissance",     "+10% XP to all skills",           "crown",
                new[]{ ("Woodcutting", 25), ("Fishing", 25), ("Mining", 25), ("Combat", 25), ("Cooking", 25),
                        ("Farming", 25), ("Crafting", 25), ("Ranged", 25), ("Athletics", 25), ("Swimming", 25) },
                new Color((byte)255,(byte)215,(byte)0,(byte)255)),
        };

        static int GetSkillLevel(string skill) => skill switch
        {
            "Woodcutting" => player.WoodcuttingLevel, "Fishing" => player.FishingLevel,
            "Mining" => player.MiningLevel, "Combat" => player.CombatLevel,
            "Cooking" => player.CookingLevel, "Farming" => player.FarmingLevel,
            "Athletics" => player.AthleticsLevel, "Strength" => player.StrengthLevel,
            "Ranged" => player.RangedLevel, "Driving" => player.DrivingLevel,
            "Riding" => player.RidingLevel, "Swimming" => player.SwimmingLevel,
            "Gambling" => player.GamblingLevel, "Crafting" => player.CraftingLevel,
            "Elemental" => player.ElementalLevel, "Defence" => player.DefenceLevel,
            "Boating" => player.BoatingLevel, _ => 0
        };

        public static bool HasSynergy(string name)
        {
            var s = Array.Find(synergies, x => x.Name == name);
            if (s == null) return false;
            return s.Requirements.All(r => GetSkillLevel(r.skill) >= r.level);
        }

        public static float SynergyXPMultiplier()
        {
            if (HasSynergy("Renaissance")) return 1.10f;
            if (HasSynergy("Jack of All Trades")) return 1.05f;
            return 1.0f;
        }

        static HashSet<string> notifiedSynergies = new();

        static void CheckSynergyUnlocks()
        {
            foreach (var s in synergies)
            {
                if (notifiedSynergies.Contains(s.Name)) continue;
                if (s.Requirements.All(r => GetSkillLevel(r.skill) >= r.level))
                {
                    notifiedSynergies.Add(s.Name);
                    ShowLevelUp($"Synergy Unlocked: {s.Name}!", 0);
                }
            }
        }

        // ── DAILY CHALLENGE BOARD ──
        static List<DailyChallenge> dailyChallenges = new();
        static bool dailyBonusClaimed = false;
        static bool dailyChallengeHudOpen = false;
        static int dailyChallengesCompletedToday = 0;
        const int DailyChallengeCount = 3;
        const int DailyBonusReward = 300;
        const int DailyBonusRep = 25;

        static readonly (string title, string category, Func<int> progress, int minTarget, int maxTarget, int reward)[] challengePool =
        {
            ("Chop Trees",       "Gathering", () => player.Logs,         5,  15, 60),
            ("Catch Fish",       "Gathering", () => player.Fish,         3,  8,  70),
            ("Mine Rocks",       "Gathering", () => player.StoneOre + player.CopperOre + player.IronOre + player.GoldOre, 5, 12, 65),
            ("Kill Enemies",     "Combat",    () => wolvesKilled,        3,  8,  80),
            ("Cook Meals",       "Cooking",   () => mealsCooked,         2,  5,  75),
            ("Harvest Crops",    "Farming",   () => cropsHarvested,      3,  6,  70),
            ("Talk to Friends",  "Social",    () => friendNPCs.Count(f => f.TalkedToday), 2, 4, 50),
            ("Gift a Friend",    "Social",    () => friendNPCs.Count(f => f.GiftedToday), 1, 2, 60),
            ("Earn Money",       "Economy",   () => player.Money,        200, 500, 50),
            ("Gain Combat XP",   "Combat",    () => player.CombatXP + (player.CombatLevel * player.CombatLevel * 50), 20, 80, 70),
            ("Gain Mining XP",   "Gathering", () => player.MiningXP + (player.MiningLevel * player.MiningLevel * 50), 15, 60, 65),
            ("Gain Fishing XP",  "Gathering", () => player.FishingXP + (player.FishingLevel * player.FishingLevel * 50), 15, 50, 65),
            ("Gain Cooking XP",  "Cooking",   () => player.CookingXP + (player.CookingLevel * player.CookingLevel * 50), 15, 50, 60),
            ("Gain Farming XP",  "Farming",   () => player.FarmingXP + (player.FarmingLevel * player.FarmingLevel * 50), 15, 50, 60),
            ("Collect Bones",    "Combat",    () => player.Bones,        3,  8,  55),
            ("Collect Fur",      "Combat",    () => player.Fur,          2,  5,  60),
            ("Chop Oak Logs",    "Gathering", () => player.OakLogs,      3,  8,  75),
            ("Mine Iron",        "Gathering", () => player.IronOre,      2,  5,  80),
        };

        static Random challengeRng = new Random();

        static void RollDailyChallenges()
        {
            dailyChallenges.Clear();
            dailyBonusClaimed = false;
            dailyChallengesCompletedToday = 0;

            // pick 3 from different categories
            var usedCategories = new HashSet<string>();
            var indices = Enumerable.Range(0, challengePool.Length).OrderBy(_ => challengeRng.Next()).ToList();

            foreach (int i in indices)
            {
                if (dailyChallenges.Count >= DailyChallengeCount) break;
                var (title, category, progress, minT, maxT, reward) = challengePool[i];
                if (usedCategories.Contains(category)) continue;
                usedCategories.Add(category);

                int target = challengeRng.Next(minT, maxT + 1);
                dailyChallenges.Add(new DailyChallenge
                {
                    Title = $"{title} x{target}",
                    Category = category,
                    Progress = progress,
                    Baseline = progress(),
                    Target = target,
                    Reward = reward,
                    Completed = false,
                });
            }

            // fallback: if we couldn't get 3 different categories, fill from any
            while (dailyChallenges.Count < DailyChallengeCount)
            {
                int i = indices[challengeRng.Next(indices.Count)];
                var (title, category, progress, minT, maxT, reward) = challengePool[i];
                if (dailyChallenges.Any(c => c.Title.StartsWith(title))) continue;
                int target = challengeRng.Next(minT, maxT + 1);
                dailyChallenges.Add(new DailyChallenge
                {
                    Title = $"{title} x{target}",
                    Category = category,
                    Progress = progress,
                    Baseline = progress(),
                    Target = target,
                    Reward = reward,
                    Completed = false,
                });
            }
        }

        static void UpdateDailyChallenges()
        {
            if (dailyChallenges.Count == 0) return;

            foreach (var c in dailyChallenges)
            {
                if (c.Completed) continue;
                if (c.Current >= c.Target)
                {
                    c.Completed = true;
                    dailyChallengesCompletedToday++;
                    player.Money += c.Reward;
                    AddReputation(8, c.Title);
                    ShowNotification($"Challenge done: {c.Title} (+${c.Reward})");
                }
            }

            // bonus for all 3
            if (!dailyBonusClaimed && dailyChallenges.All(c => c.Completed))
            {
                dailyBonusClaimed = true;
                player.Money += DailyBonusReward;
                AddReputation(DailyBonusRep, "Daily Challenge Bonus");
                ShowLevelUp($"All Daily Challenges Complete! +${DailyBonusReward}", 0);
            }
        }

        static void DrawDailyChallengeHud()
        {
            int btnX = 170, btnY = 8;
            int allDone = dailyChallenges.Count(c => c.Completed);
            Color btnCol = dailyChallengeHudOpen ? Color.Gold : (allDone == DailyChallengeCount ? Color.Green : Color.White);
            Raylib.DrawRectangle(btnX, btnY, 300, 30, new Color((byte)0,(byte)0,(byte)0,(byte)200));
            Raylib.DrawRectangleLinesEx(new Rectangle(btnX, btnY, 300, 30), 1, btnCol);
            Program.DrawTextUI($"DAILY CHALLENGES  {allDone}/{DailyChallengeCount}", btnX + 8, btnY + 6, 16, btnCol);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left)
                && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(btnX, btnY, 300, 30)))
                dailyChallengeHudOpen = !dailyChallengeHudOpen;

            if (!dailyChallengeHudOpen || dailyChallenges.Count == 0) return;

            // Panel
            int px = 244, py = btnY + 36;
            int pw = 310, ph = 190;
            Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)0,(byte)0,(byte)0,(byte)230));
            Raylib.DrawRectangleLines(px, py, pw, ph, Color.Gold);
            Program.DrawTextUI("TODAY'S CHALLENGES", px + 14, py + 8, 18, Color.Gold);

            for (int i = 0; i < dailyChallenges.Count && i < 3; i++)
            {
                var c = dailyChallenges[i];
                int ry = py + 34 + i * 48;
                Color catCol = c.Category switch
                {
                    "Combat"    => new Color((byte)220,(byte)70,(byte)60,(byte)255),
                    "Gathering" => new Color((byte)80,(byte)180,(byte)80,(byte)255),
                    "Cooking"   => new Color((byte)220,(byte)150,(byte)40,(byte)255),
                    "Farming"   => new Color((byte)120,(byte)200,(byte)60,(byte)255),
                    "Social"    => new Color((byte)140,(byte)120,(byte)220,(byte)255),
                    "Economy"   => new Color((byte)200,(byte)180,(byte)60,(byte)255),
                    _ => Color.White,
                };

                // category dot
                Raylib.DrawCircle(px + 16, ry + 10, 5, c.Completed ? Color.Green : catCol);

                // title
                Program.DrawTextUI(c.Title, px + 28, ry, 15, c.Completed ? Color.Green : Color.White);

                // progress bar
                float prog = Math.Clamp((float)c.Current / c.Target, 0f, 1f);
                Raylib.DrawRectangle(px + 28, ry + 20, 200, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
                Raylib.DrawRectangle(px + 28, ry + 20, (int)(200 * prog), 8, c.Completed ? Color.Green : catCol);

                // count + reward
                Program.DrawTextUI($"{c.Current}/{c.Target}", px + 235, ry + 2, 13, Color.LightGray);
                Program.DrawTextUI($"${c.Reward}", px + 235, ry + 18, 12, c.Completed ? Color.Green : Color.Gold);
            }

            // bonus row
            int bonusY = py + 34 + 3 * 48;
            bool allComplete = dailyChallenges.All(c => c.Completed);
            Program.DrawTextUI($"BONUS: Complete all 3 → +${DailyBonusReward} +{DailyBonusRep} rep",
                px + 14, bonusY, 13, allComplete ? Color.Green : new Color((byte)160,(byte)140,(byte)80,(byte)200));
        }

        // Cheats
        static readonly (string name, Func<int> get, Action<int> set)[] cheatSkills = new (string, Func<int>, Action<int>)[]
        {
            ("Woodcutting", () => player.WoodcuttingLevel, v => player.WoodcuttingLevel = v),
            ("Mining",      () => player.MiningLevel,      v => player.MiningLevel = v),
            ("Fishing",     () => player.FishingLevel,     v => player.FishingLevel = v),
            ("Combat",      () => player.CombatLevel,      v => player.CombatLevel = v),
            ("1H Melee",    () => player.OneHandMeleeLevel, v => player.OneHandMeleeLevel = v),
            ("2H Melee",    () => player.TwoHandMeleeLevel, v => player.TwoHandMeleeLevel = v),
            ("Ranged",      () => player.RangedLevel,      v => player.RangedLevel = v),
            ("Strength",    () => player.StrengthLevel,    v => player.StrengthLevel = v),
            ("Cooking",     () => player.CookingLevel,     v => player.CookingLevel = v),
            ("Athletics",   () => player.AthleticsLevel,   v => player.AthleticsLevel = v),
            ("Driving",     () => player.DrivingLevel,     v => player.DrivingLevel = v),
            ("Cycling",     () => player.CyclingLevel,     v => player.CyclingLevel = v),
            ("Riding",      () => player.RidingLevel,      v => player.RidingLevel = v),
            ("Swimming",    () => player.SwimmingLevel,    v => player.SwimmingLevel = v),
            ("Diving",      () => player.DivingLevel,      v => player.DivingLevel = v),
            ("Sports",      () => player.SportsLevel,      v => player.SportsLevel = v),
            ("Gambling",    () => player.GamblingLevel,    v => player.GamblingLevel = v),
            ("Farming", () => player.FarmingLevel, v => player.FarmingLevel = v),
            ("Education", () => player.EducationLevel, v => player.EducationLevel = v),
            ("Faith",     () => player.FaithLevel,    v => player.FaithLevel = v),
            ("Mystical",  () => player.MysticalLevel, v => player.MysticalLevel = v),
            ("Dark Arts", () => player.DarkArtsLevel, v => player.DarkArtsLevel = v),
            ("Swimming", () => player.SwimmingLevel, v => player.SwimmingLevel = v),

        };

        static bool skillCheatOpen = false;
        public static bool cheatSpeedBoost = false;
        static bool cheatNoclip = false;
        static float cheatScrollY = 0f;

        // Weapons menu
        static string equipped1H = null;     // one-handed weapon
        static string equipped2H = null;     // two-handed weapon
        static string equippedAmmo = null;

        // Armor Menu
        public static bool armorMenuOpen = false;
        static bool gearTestMode = false;   // false = OWNED, true = TEST catalog
        static int testMageTier = 0, testRangerTier = 0;
        public static string[] armorMaterials = { "Leather", "Iron", "Steel", "Gold", "Diamond", "Ruby", "Sapphire", "Emerald", "Infernal", "Magic", "Mystical" };
        public static string[] mageTiers   = { "Apprentice", "Twilight", "Stella", "Phantom", "Supernova" };
        public static string[] rangerTiers = { "Leather", "Scaled", "Sun", "Shadow", "Falcon", "Serpent" };
        public static int ClassTierIndex(string item)   // 0 = weakest tier
        {
            if (item == null) return 0;
            string[] tiers = item.Contains("Mage ") ? mageTiers : item.Contains("Ranger ") ? rangerTiers : null;
            if (tiers == null) return 0;
            for (int i = 0; i < tiers.Length; i++)
                if (item.StartsWith(tiers[i] + " ")) return i;
            return 0;
        }
        static int[] weaponMaterialIndex = new int[5];   // sword, greatsword, axe, bow, crossbow
        static string[] weaponPieceNames = { "Sword", "Great Sword", "War Axe", "Bow", "Crossbow" };

        // returns the color for a material, with infernal animated

        // Armor slots — null = empty
        public static string armorHelmet = null;
        public static string armorBody   = null;
        public static string armorLegs   = null;
        public static string armorBoots  = null;
        public static string armorGloves = null;
        public static string armorCape   = null;
        public static string armorWeapon = null;
        public static string armorShield = null;
        static int[] slotMaterialIndex = new int[7];   // helmet, body, legs, boots, gloves, cape, shield
        static string[] slotPieceNames = { "Helmet", "Chestplate", "Leggings", "Boots", "Gauntlets", "Cape", "Shield" };

        // shared
        static bool strengthMinigameActive = false;
        static string strengthMinigameType = "";
        static float strengthMinigameCooldown = 0f;

        // Dumbbell game
        static float dbBarPos = 0f;
        static float dbBarDir = 1f;
        static float dbBarSpeed = 0.24f;
        static int dbConsecutiveHits = 0;

        // Barbell game
        static float bbBarPos = 0f;
        static float bbBarDir = 1f;
        static float bbBarSpeed = 0.20f;
        static float bbGreenPos = 0f;
        static int bbConsecutiveHits = 0;
        static Vector2 barCounterPos = new Vector2(250, 170);

        // Enemies
        static List<EnemyProjectile> enemyProjectiles = new();

        // RANGED

        static List<Projectile> projectiles = new();
        static float bowCooldown = 0f;

        // ELEMENTAL

        static List<SpellProjectile> spellProjectiles = new();
        static List<RemoteVisualProjectile> remoteVisualProjectiles = new();

        class RemoteVisualProjectile
        {
            public Vector2 Pos;
            public Vector2 Vel;
            public float   Life;
            public string  Kind;      // "Arrows", "Bolts", or a spell type like "Fire","Lightning","Dark"
            public bool    IsSpell;
        }
        static float spellCooldown = 0f;

        // CRAFTING
        static List<PlacedChest> placedChests = new();
        static readonly (string ingredient, int qty)[][] chestTierRecipe = {
            new[] { ("Logs", 15) },
            new[] { ("Logs", 20) },
            new[] { ("Oak Logs", 20), ("Iron Ore", 10) },
            new[] { ("Iron Ore", 20), ("Crystal", 5) },
        };

        class CraftRecipe
        {
            public string Result;
            public (string ingredient, int qty)[] Ingredients = Array.Empty<(string, int)>();
            public string Description;
            public string Station = null;
            public int ResultQty = 1; 
        }

        static readonly List<CraftRecipe> craftRecipes = BuildCraftRecipes();
        static float craftScrollY = 0f;

// optional: tier bonus for chop/mine speed, fishing luck, etc.

        static Dictionary<string, List<Vector2>> stationProps = new()
        {
            ["Anvil"] = new(), ["Advanced Anvil"] = new(),
            ["Advanced Workstation"] = new(), ["Advanced Furnace"] = new(),
            ["Enchanting Table"] = new()
        };

        static List<Vector2> placedWorkbenches = new();
        static List<Vector2> placedFurnaces = new();
        static List<Vector2> placedFlags = new();
        static bool furnaceOpen = false;
        static bool furnaceOpenAdvanced = false;
        static readonly (string input, int qty, string extra, int extraQty, string bar, bool advanced)[] smeltRecipes = {
            ("Copper Ore", 2, null, 0, "Copper Bar", false),
            ("Iron Ore",   2, null, 0, "Iron Bar",   false),
            ("Gold Ore",   2, null, 0, "Gold Bar",   false),
            ("Iron Bar",   2, "Ember Stone", 1, "Steel Bar", true),   
        };

        // Toolbar → backpack, NO notifications (so callers control the message)

        static string openChestId = null;

static Vector2 survLastPos;
static float survivalHpTick = 0f;

// Survival

        // FISHING
        public static bool isFishing = false;
        static int fishingPhase = 0;        // 0 cast/waiting, 1 bite (react!), 2 reeling minigame, 3 result
        static float fishingTimer = 0f;
        static float fishingBiteTime = 0f;  // randomized wait until a bite
        static float fishingReactWindow = 0f; // time left to react to a bite
        static float reelBarPos = 0f;       // moving cursor 0..1
        static bool reelBarUp = true;
        static float reelTargetMin = 0f;    // green zone bounds
        static float reelTargetMax = 0f;
        static string fishingResult = "";
        static float fishingResultTimer = 0f;
        static string fishingWater = "Lake";
        static Lake currentLake = null;
        static bool nearRiver = false;
        
        // SKILLS 
        static string levelUpMessage = "";
        static float levelUpTimer = 0f;
        static bool skillsOpen = false;
        static string skillDetailOpen = "";
        static bool hoverWoodcutting = false;
        static bool hoverCombat = false;
        static bool hoverMining = false;
        static bool hoverGambling = false;
        static bool hoverRiding   = false;
        static bool hoverCycling  = false;
        static bool hoverFishing = false;
        static bool hoverStrength = false;
        static bool hoverAthletics = false;
        static bool hoverDriving = false;
        static bool hoverSwimming = false;
        static bool hoverDiving = false;
        static bool hoverSports = false;
        static bool hoverRanged = false;
        static bool hoverCooking = false;
        static bool hoverStaminaSkill = false;
        static bool hoverFaith = false;
        static bool hoverMystical = false;
        static bool hoverDarkArts = false;

        static bool hoverCraftSkill = false, hoverBlacksmith = false, hoverEnchanting = false;
        static bool hoverElemental = false;
        static bool hoverCards = false;
        static bool hoverOneHand = false;
        static bool hoverTwoHand = false;
        static bool hoverFarming = false;
        static bool hoverEducation = false;

        // TUTORIAL
        public static bool tutorialActive = true;        // on for a new game, off after finishing/loading
        public static int tutorialStep = 0;
        static Vector2 tutorialNpcPos;
        static float tutorialNpcBob = 0f;
        static string tutorialMessage = "";
        static float tutorialMessageTimer = 0f;
        static bool tutorialCompleted = false;
        static bool tutorialChestOpened = false;
        static Vector2 tutorialChestPos = new Vector2(-2500, -6400);

        // each task: instruction text, the gate it unlocks when done
        class TutorialTask
        {
            public string Title;
            public string Instruction;
            public Vector2 GuidePos;      // where the NPC hovers to point you
            public bool Done;
        }

        static List<TutorialTask> tutorialTasks = new()
        {
            new TutorialTask { Title="Woodcutting", Instruction="Equip the Axe and chop a tree! Walk to it and press SPACE.", GuidePos=new Vector2(-2596, -9121) },
            new TutorialTask { Title="Mining",      Instruction="Equip the Pickaxe and mine a rock with SPACE.",            GuidePos=new Vector2(-2440, -8659) },
            new TutorialTask { Title="Fishing",     Instruction="Equip the Rod, stand by the pond and press SPACE to fish.", GuidePos=new Vector2(-2832, -8136) },
            new TutorialTask { Title="Net Fishing", Instruction="Equip the Net, stand by a river and press SPACE to catch 2 river fish.", GuidePos=new Vector2(-2358, -8207) },
            new TutorialTask { Title="Cooking",     Instruction="Light a campfire with a Log (press R), then cook your fish with SPACE.", GuidePos=new Vector2(-2151,  -7873) }, 
            new TutorialTask { Title="Combat",      Instruction="Equip the Sword or Stick and swing with SPACE or Click.",   GuidePos=new Vector2(-2076, -7449) },
            new TutorialTask { Title="Ranged", Instruction="Open the chest for arrows, equip the Bow + Arrows, and shoot with SPACE or Click.", GuidePos=new Vector2(-2559, -6457) },
            new TutorialTask { Title="Magic",  Instruction="Equip the Staff + Arcane Essence, then cast a spell with SPACE or Click.",        GuidePos=new Vector2(-2367, -6458) },
            new TutorialTask { Title="Crafting", Instruction="Stand at the Workbench and craft any item.",              GuidePos=new Vector2(-2200, -6600) },
            new TutorialTask { Title="Farming",  Instruction="Grab the Spade, till a plot and plant Wheat Seeds.",      GuidePos=new Vector2(-2650, -6800) },
            new TutorialTask { Title="Riding",   Instruction="Find the donkey and press E to ride it around!",          GuidePos=new Vector2(-2350, -7150) }, 
            new TutorialTask { Title="Get Your ID", Instruction="Head to the Library and take your ID photo at the booth.", GuidePos=new Vector2(500, -600) },
              
        };
        static bool questsOpen = false;
        static float questScrollY = 0f;
        static List<GasStation> gasStations = new();
        static List<Quest> quests = new();
        static string playerName = "";
        static bool nameEntered = false;
        static string buildingPromptMessage = "";
        static float buildingPromptTimer = 0f;
        static Color buildingPromptColor = Color.White;
        static bool buildingPromptLocked = false;

        // STORE
        static bool showControlsHud = false;   // on by default
        static bool shopOpen = false;
        static bool shopUIOpen = false;
        static bool shopBuyMode = false;   
        static int shopSelectedItem = -1;
        static string shopSelectedItemName = "";
        static string shopMessage = "";
        static float shopMessageTimer = 0f;

        // ── MASTERY SHOP (Castle) ──
static bool masteryShopOpen = false;
static int masteryShopScroll = 0;

record MasteryItem(string Skill, string Cape, string Headpiece, Color ThemeA, Color ThemeB, Func<int> GetLevel);

static readonly MasteryItem[] masteryItems = {
    new("Woodcutting",   "Lumberjack's Cloak",    "Woodsman Crown",     new Color((byte)60,(byte)120,(byte)40,(byte)255),  new Color((byte)100,(byte)70,(byte)30,(byte)255),  () => player.WoodcuttingLevel),
    new("Fishing",       "Sea King's Cape",       "Angler's Crown",     new Color((byte)40,(byte)120,(byte)200,(byte)255), new Color((byte)180,(byte)200,(byte)220,(byte)255), () => player.FishingLevel),
    new("Mining",        "Earthshaper's Mantle",  "Crystal Crown",      new Color((byte)130,(byte)130,(byte)140,(byte)255),new Color((byte)160,(byte)100,(byte)200,(byte)255), () => player.MiningLevel),
    new("Combat",        "Warlord's Cape",        "Battle Crown",       new Color((byte)180,(byte)30,(byte)30,(byte)255),  new Color((byte)40,(byte)40,(byte)40,(byte)255),    () => player.CombatLevel),
    new("Cooking",       "Chef's Royal Cape",     "Golden Chef Hat",    new Color((byte)240,(byte)240,(byte)230,(byte)255),new Color((byte)220,(byte)180,(byte)40,(byte)255),   () => player.CookingLevel),
    new("Farming",       "Harvest Lord's Cloak",  "Wheat Crown",        new Color((byte)200,(byte)180,(byte)60,(byte)255), new Color((byte)80,(byte)140,(byte)50,(byte)255),    () => player.FarmingLevel),
    new("Athletics",     "Windrunner's Cape",     "Laurel Wreath",      new Color((byte)120,(byte)210,(byte)140,(byte)255),new Color((byte)240,(byte)240,(byte)240,(byte)255),   () => player.AthleticsLevel),
    new("Strength",      "Titan's Mantle",        "Iron Crown",         new Color((byte)100,(byte)40,(byte)40,(byte)255),  new Color((byte)80,(byte)80,(byte)90,(byte)255),     () => player.StrengthLevel),
    new("Ranged",        "Hawkeye's Cloak",       "Marksman Hood",      new Color((byte)120,(byte)80,(byte)40,(byte)255),  new Color((byte)220,(byte)150,(byte)50,(byte)255),   () => player.RangedLevel),
    new("Driving",       "Road King's Jacket",    "Racing Helmet",      new Color((byte)180,(byte)180,(byte)190,(byte)255),new Color((byte)200,(byte)50,(byte)50,(byte)255),    () => player.DrivingLevel),
    new("Riding",        "Cavalry Cape",          "Rider's Crown",      new Color((byte)140,(byte)90,(byte)40,(byte)255),  new Color((byte)180,(byte)160,(byte)80,(byte)255),   () => player.RidingLevel),
    new("Swimming",      "Poseidon's Cape",       "Coral Crown",        new Color((byte)40,(byte)180,(byte)200,(byte)255), new Color((byte)220,(byte)130,(byte)140,(byte)255),   () => player.SwimmingLevel),
    new("Gambling",      "High Roller's Cape",    "Golden Top Hat",     new Color((byte)30,(byte)30,(byte)30,(byte)255),   new Color((byte)220,(byte)180,(byte)40,(byte)255),   () => player.GamblingLevel),
    new("Crafting",      "Artisan's Cloak",       "Craftsman Crown",    new Color((byte)160,(byte)110,(byte)60,(byte)255), new Color((byte)200,(byte)160,(byte)80,(byte)255),   () => player.CraftingLevel),
    new("1H Melee",      "Duelist's Cape",        "Fencer's Helm",      new Color((byte)100,(byte)100,(byte)160,(byte)255),new Color((byte)200,(byte)200,(byte)220,(byte)255),   () => player.OneHandMeleeLevel),
    new("2H Melee",      "Berserker's Mantle",    "Warlord Helm",       new Color((byte)150,(byte)50,(byte)30,(byte)255),  new Color((byte)60,(byte)60,(byte)70,(byte)255),     () => player.TwoHandMeleeLevel),
    new("Elemental",     "Archmage's Cloak",      "Elemental Crown",    new Color((byte)100,(byte)60,(byte)180,(byte)255), new Color((byte)180,(byte)120,(byte)255,(byte)255),   () => player.ElementalLevel),
    new("Cycling",       "Velodrome Cape",        "Cyclist's Helm",     new Color((byte)220,(byte)220,(byte)40,(byte)255), new Color((byte)40,(byte)40,(byte)40,(byte)255),     () => player.CyclingLevel),
    new("Diving",        "Abyssal Cloak",         "Deep Sea Crown",     new Color((byte)20,(byte)60,(byte)120,(byte)255),  new Color((byte)80,(byte)200,(byte)180,(byte)255),   () => player.DivingLevel),
    new("Blacksmithing", "Forgemaster's Mantle",  "Anvil Crown",        new Color((byte)80,(byte)80,(byte)90,(byte)255),   new Color((byte)220,(byte)120,(byte)30,(byte)255),   () => player.BlacksmithLevel),
    new("Enchanting",    "Runeweaver's Cape",     "Glyph Crown",        new Color((byte)60,(byte)180,(byte)180,(byte)255), new Color((byte)200,(byte)200,(byte)255,(byte)255),   () => player.EnchantingLevel),
    new("Education",     "Scholar's Robe",        "Mortarboard",        new Color((byte)30,(byte)30,(byte)30,(byte)255),   new Color((byte)220,(byte)180,(byte)40,(byte)255),   () => player.EducationLevel),
    new("Sports",        "Champion's Cape",       "Victory Crown",      new Color((byte)220,(byte)40,(byte)40,(byte)255),  new Color((byte)220,(byte)180,(byte)40,(byte)255),   () => player.SportsLevel),
    new("Cards",         "Cardmaster's Cloak",    "Dealer's Crown",     new Color((byte)20,(byte)80,(byte)40,(byte)255),   new Color((byte)220,(byte)220,(byte)220,(byte)255),  () => player.PlayingCardsLevel),
};

        // DBAR
        static bool barMenuOpen = false;
        static int barSelectedDrink = -1;

        // KIWI CUTS
        static NPC kiwiCutsBarber = new NPC(new Vector2(160, 200), "Barber", "Take a seat mate, won't be long.");
        static List<NPC> kiwiCutsWaitingNPCs = new();
        static bool hairMenuOpen = false;
        static int  barberMenuTab = 0;
        public static string playerHairStyle = "None";  // current hairstyle
        public static Color playerHairColor  = new Color((byte)80,(byte)50,(byte)20,(byte)255);
        public static string playerFacialHair = "None";   // "None","Stubble","Moustache","Goatee","Full Beard"
        public static Color playerFacialHairColor = new Color((byte)80,(byte)50,(byte)20,(byte)255);
        public static bool HasHeadItemEquipped => armorHelmet != null;
        // ... add future head items here (hats, crowns, etc.) with ||

        // HALLENSTEINS
        static bool hallensteinShopOpen = false;
        static int  hallensteinCategory = 0;   // 0=Tops 1=Bottoms 2=Outerwear 3=Accessories
        static int  hallensteinScroll   = 0;
        static HallItem[] hallItems = {
            // Tops
            new("Black Tee",      "Tops",       25, new Color((byte)20,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "tee"),
            new("White Tee",      "Tops",       25, new Color((byte)230,(byte)230,(byte)230,(byte)255), Color.Blank,                                         "tee"),
            new("Navy Tee",       "Tops",       25, new Color((byte)20,(byte)40,(byte)100,(byte)255),   Color.Blank,                                         "tee"),
            new("Red Tee",        "Tops",       25, new Color((byte)180,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "tee"),
            new("Black Singlet",  "Tops",       20, new Color((byte)20,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "singlet"),
            new("White Singlet",  "Tops",       20, new Color((byte)230,(byte)230,(byte)230,(byte)255), Color.Blank,                                         "singlet"),
            new("Stripe Tee",     "Tops",       30, new Color((byte)20,(byte)20,(byte)20,(byte)255),   new Color((byte)230,(byte)230,(byte)230,(byte)255),  "stripe"),
            new("Rugby Polo",     "Tops",       45, new Color((byte)0,(byte)60,(byte)20,(byte)255),    new Color((byte)220,(byte)220,(byte)220,(byte)255),  "polo"),
            // Bottoms
            new("Black Jeans",    "Bottoms",    60, new Color((byte)20,(byte)20,(byte)30,(byte)255),   Color.Blank,                                         "jeans"),
            new("Blue Jeans",     "Bottoms",    60, new Color((byte)40,(byte)60,(byte)140,(byte)255),  Color.Blank,                                         "jeans"),
            new("Khaki Chinos",   "Bottoms",    55, new Color((byte)160,(byte)140,(byte)90,(byte)255), Color.Blank,                                         "chinos"),
            new("Grey Chinos",    "Bottoms",    55, new Color((byte)110,(byte)110,(byte)110,(byte)255), Color.Blank,                                        "chinos"),
            new("Black Shorts",   "Bottoms",    35, new Color((byte)20,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "shorts"),
            new("Navy Shorts",    "Bottoms",    35, new Color((byte)20,(byte)40,(byte)100,(byte)255),  Color.Blank,                                         "shorts"),
            new("Camo Shorts",    "Bottoms",    40, new Color((byte)60,(byte)80,(byte)40,(byte)255),   new Color((byte)40,(byte)55,(byte)25,(byte)255),     "camo"),
            // Outerwear
            new("Black Hoodie",   "Outerwear",  80, new Color((byte)25,(byte)25,(byte)25,(byte)255),   Color.Blank,                                         "hoodie"),
            new("Grey Hoodie",    "Outerwear",  80, new Color((byte)120,(byte)120,(byte)120,(byte)255), Color.Blank,                                        "hoodie"),
            new("Navy Hoodie",    "Outerwear",  80, new Color((byte)20,(byte)30,(byte)90,(byte)255),   Color.Blank,                                         "hoodie"),
            new("Black Jacket",   "Outerwear", 120, new Color((byte)15,(byte)15,(byte)15,(byte)255),   new Color((byte)60,(byte)60,(byte)60,(byte)255),     "jacket"),
            new("Puffer Jacket",  "Outerwear", 110, new Color((byte)20,(byte)40,(byte)100,(byte)255),  new Color((byte)15,(byte)30,(byte)75,(byte)255),     "puffer"),
            // Accessories
            new("Black Cap",      "Accessories",30, new Color((byte)20,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "cap"),
            new("White Cap",      "Accessories",30, new Color((byte)230,(byte)230,(byte)230,(byte)255), Color.Blank,                                        "cap"),
            new("Beanie",         "Accessories",25, new Color((byte)60,(byte)20,(byte)20,(byte)255),   Color.Blank,                                         "beanie"),
            new("Sunglasses",     "Accessories",40, new Color((byte)10,(byte)10,(byte)10,(byte)255),   Color.Blank,                                         "sunnies"),
            new("Gold Chain",     "Accessories",90, new Color((byte)200,(byte)170,(byte)20,(byte)255), Color.Blank,                                         "chain"),
        };

        // PAUSE MENU
        static bool pauseMenuOpen = false;
        static bool mapOpen = false;
        static int overwriteSlot = -1;
        static bool overwriteConfirmOpen = false;
        static bool optionsMenuOpen = false;
        static bool cheatsMenuOpen = false;
        static bool loadMenuOpen = false;
        static int cheatGoldAmount = 10000;

        // WORLD MAP AND MINIMAP
        static int minimapSize = 120;
        static int minimapX = 20;
        static int minimapY = 20;
        static float minimapScale = 0.01f;
        static float worldMapZoom = 1f;
        // FOG OF WAR
        const int FogCellSize = 1000;            
        const int FogOriginX  = -260000;         
        const int FogOriginY  = -260000;         
        const int FogCols     = 520;             
        const int FogRows     = 520;             
        static bool[] fogRevealed = new bool[FogCols * FogRows];
        static int    fogRevealedCount = 0;
        const int     FogRevealRadius = 4; 
        // ── ACHIEVEMENTS ──
        class Achievement
        {
            public string Id;
            public string Title;
            public string Description;
            public string Category;     // Exploration, Combat, Gathering, Crafting, Social, Wealth, Mastery, Misc
            public int    Reward;       // money reward
            public Func<bool> Condition;
            public bool   Unlocked;
            public Color  IconColor;
        }

        static bool achievementsOpen = false;
        static float achievementScrollY = 0f;
        static int   achievementCategory = 0;  // 0=All, then per-category
        static float achievementPopupTimer = 0f;
        static string achievementPopupTitle = "";
        static int   achievementsUnlockedCount = 0;

        static readonly string[] achievementCategories = { "All", "Exploration", "Combat", "Gathering", "Crafting", "Social", "Wealth", "Mastery", "Misc" };

        static List<Achievement> achievements = new();

        static void InitAchievements()
        {
            achievements.Clear();
            achievementsUnlockedCount = 0;

            void A(string id, string title, string desc, string cat, int reward, Func<bool> cond, Color col)
                => achievements.Add(new Achievement { Id=id, Title=title, Description=desc, Category=cat, Reward=reward, Condition=cond, IconColor=col });

            Color expl = new Color((byte)60,(byte)180,(byte)255,(byte)255);
            Color comb = new Color((byte)220,(byte)50,(byte)50,(byte)255);
            Color gath = new Color((byte)80,(byte)200,(byte)80,(byte)255);
            Color craf = new Color((byte)200,(byte)160,(byte)60,(byte)255);
            Color soci = new Color((byte)220,(byte)120,(byte)220,(byte)255);
            Color weal = new Color((byte)255,(byte)215,(byte)0,(byte)255);
            Color mast = new Color((byte)100,(byte)200,(byte)255,(byte)255);
            Color misc = new Color((byte)180,(byte)180,(byte)180,(byte)255);

            // ── Exploration ──
            A("expl_first_steps",   "First Steps",       "Walk 500 units from spawn",          "Exploration", 25,  () => Vector2.Distance(player.Position, new Vector2(-1917,-9720)) > 500, expl);
            A("expl_desert",        "Sandy Boots",       "Visit the Desert biome",             "Exploration", 50,  () => GetCurrentBiome() == "DESERT" || achievementVisited.Contains("DESERT"), expl);
            A("expl_snow",          "Frostbitten",       "Visit the Snow Zone",                "Exploration", 50,  () => GetCurrentBiome() == "SNOW ZONE" || achievementVisited.Contains("SNOW ZONE"), expl);
            A("expl_volcano",       "Playing With Fire", "Visit the Volcano",                  "Exploration", 75,  () => GetCurrentBiome() == "VOLCANO" || achievementVisited.Contains("VOLCANO"), expl);
            A("expl_ocean",         "Deep Blue",         "Visit the Ocean",                    "Exploration", 50,  () => GetCurrentBiome() == "OCEAN" || achievementVisited.Contains("OCEAN"), expl);
            A("expl_mountains",     "Peak Climber",      "Visit the Mountains",                "Exploration", 50,  () => GetCurrentBiome() == "MOUNTAINS" || achievementVisited.Contains("MOUNTAINS"), expl);
            A("expl_beach",         "Beach Bum",         "Visit the Beach",                    "Exploration", 30,  () => GetCurrentBiome() == "BEACH" || achievementVisited.Contains("BEACH"), expl);
            A("expl_hamiltron",     "City Slicker",      "Visit Hamiltron City",               "Exploration", 75,  () => GetCurrentBiome() == "HAMILTRON CITY" || achievementVisited.Contains("HAMILTRON CITY"), expl);
            A("expl_rotoaira",      "Small Town Vibes",  "Visit Rotoaira",                     "Exploration", 75,  () => GetCurrentBiome() == "ROTOAIRA" || achievementVisited.Contains("ROTOAIRA"), expl);
            A("expl_tundra",      "Into the Tundra",    "Visit the Tundra",                    "Exploration", 75,  () => GetCurrentBiome() == "TUNDRA" || achievementVisited.Contains("TUNDRA"), expl);
            A("expl_frozen_lake",  "Frozen Over",        "Visit the Frozen Lake",              "Exploration", 75,  () => GetCurrentBiome() == "FROZEN LAKE" || achievementVisited.Contains("FROZEN LAKE"), expl);
            A("expl_ice_caves",    "Ice Explorer",       "Visit the Ice Caves",                "Exploration", 100, () => GetCurrentBiome() == "ICE CAVES" || achievementVisited.Contains("ICE CAVES"), expl);
            A("expl_crystal",      "Crystal Seeker",     "Visit the Crystal Caves",            "Exploration", 150, () => GetCurrentBiome() == "CRYSTAL CAVES" || achievementVisited.Contains("CRYSTAL CAVES"), expl);
            A("expl_alpine",       "Alpine Wanderer",    "Visit the Alpine Meadow",            "Exploration", 75,  () => GetCurrentBiome() == "ALPINE MEADOW" || achievementVisited.Contains("ALPINE MEADOW"), expl);
            A("expl_cliffs",       "Cliff Hanger",       "Visit the Cliffs",                   "Exploration", 100, () => GetCurrentBiome() == "CLIFFS" || achievementVisited.Contains("CLIFFS"), expl);
            A("expl_caldera",      "Into the Caldera",   "Visit the Caldera",                  "Exploration", 150, () => GetCurrentBiome() == "CALDERA" || achievementVisited.Contains("CALDERA"), expl);
            A("expl_ashen",        "Ash Walker",         "Visit the Ashen Wastes",             "Exploration", 100, () => GetCurrentBiome() == "ASHEN WASTES" || achievementVisited.Contains("ASHEN WASTES"), expl);
            A("expl_lava_fields",  "Floor is Lava",      "Visit the Lava Fields",              "Exploration", 100, () => GetCurrentBiome() == "LAVA FIELDS" || achievementVisited.Contains("LAVA FIELDS"), expl);
            A("expl_mangrove",     "Mangrove Maze",      "Visit the Mangrove",                 "Exploration", 75,  () => GetCurrentBiome() == "MANGROVE" || achievementVisited.Contains("MANGROVE"), expl);
            A("expl_bog",          "Bog Trudger",        "Visit the Bog",                      "Exploration", 75,  () => GetCurrentBiome() == "BOG" || achievementVisited.Contains("BOG"), expl);
            A("expl_dead_marsh",   "Dead Marsh",         "Visit the Dead Marsh",               "Exploration", 100, () => GetCurrentBiome() == "DEAD MARSH" || achievementVisited.Contains("DEAD MARSH"), expl);
            A("expl_oasis",        "Desert Oasis",       "Find the Oasis",                     "Exploration", 100, () => GetCurrentBiome() == "OASIS" || achievementVisited.Contains("OASIS"), expl);
            A("expl_dunes",        "Dune Runner",        "Visit the Dunes",                    "Exploration", 75,  () => GetCurrentBiome() == "DUNES" || achievementVisited.Contains("DUNES"), expl);
            A("expl_badlands",     "Badlands",           "Visit the Badlands",                 "Exploration", 100, () => GetCurrentBiome() == "BADLANDS" || achievementVisited.Contains("BADLANDS"), expl);
            A("expl_dark_forest",  "Into Darkness",      "Visit the Dark Forest",              "Exploration", 100, () => GetCurrentBiome() == "DARK FOREST" || achievementVisited.Contains("DARK FOREST"), expl);
            A("expl_enchanted",    "Enchanted",          "Visit the Enchanted Woods",          "Exploration", 100, () => GetCurrentBiome() == "ENCHANTED WOODS" || achievementVisited.Contains("ENCHANTED WOODS"), expl);
            A("expl_mushroom",     "Shroom Seeker",      "Visit the Mushroom Grove",           "Exploration", 100, () => GetCurrentBiome() == "MUSHROOM GROVE" || achievementVisited.Contains("MUSHROOM GROVE"), expl);
            A("expl_coral",        "Reef Diver",         "Visit the Coral Reef",               "Exploration", 100, () => GetCurrentBiome() == "CORAL REEF" || achievementVisited.Contains("CORAL REEF"), expl);
            A("expl_deep_ocean",   "Abyss",              "Visit the Deep Ocean",               "Exploration", 150, () => GetCurrentBiome() == "DEEP OCEAN" || achievementVisited.Contains("DEEP OCEAN"), expl);
            A("expl_islands",      "Island Hopper",      "Visit the Islands",                  "Exploration", 100, () => GetCurrentBiome() == "ISLANDS" || achievementVisited.Contains("ISLANDS"), expl);
            A("expl_10pct",         "Cartographer",      "Explore 10% of the world map",       "Exploration", 100, () => GetExplorationPercent() >= 10f, expl);
            A("expl_25pct",         "Pathfinder",        "Explore 25% of the world map",       "Exploration", 250, () => GetExplorationPercent() >= 25f, expl);
            A("expl_50pct",         "Trailblazer",       "Explore 50% of the world map",       "Exploration", 500, () => GetExplorationPercent() >= 50f, expl);
            A("expl_100pct",        "World Walker",      "Explore 100% of the world map",      "Exploration", 2000,() => GetExplorationPercent() >= 99.5f, expl);
            A("expl_collectables5", "Treasure Seeker",   "Find 5 hidden collectables",         "Exploration", 150, () => CollectablesFound >= 5, expl);
            A("expl_collectables15","Treasure Master",   "Find 15 hidden collectables",        "Exploration", 500, () => CollectablesFound >= 15, expl);
    

            // ── Combat ──
            A("comb_first_kill",  "First Blood",         "Defeat your first enemy",            "Combat", 25,  () => wolvesKilled > 0 || player.CombatLevel > 1, comb);
            A("comb_wolves10",    "Wolf Culler",          "Slay 10 wolves",                    "Combat", 100, () => wolvesKilled >= 10, comb);
            A("comb_wolves50",    "Alpha Hunter",         "Slay 50 wolves",                    "Combat", 300, () => wolvesKilled >= 50, comb);
            A("comb_dungeon1",    "Dungeon Delver",       "Clear your first dungeon",          "Combat", 150, () => dungeonsCleared >= 1, comb);
            A("comb_dungeon5",    "Dungeon Crawler",      "Clear 5 dungeons",                  "Combat", 400, () => dungeonsCleared >= 5, comb);
            A("comb_dungeon20",   "Dungeon Master",       "Clear 20 dungeons",                 "Combat", 1000,() => dungeonsCleared >= 20, comb);
            A("comb_combat25",    "Brawler",              "Reach Combat level 25",             "Combat", 200, () => player.CombatLevel >= 25, comb);
            A("comb_combat50",    "Warrior",              "Reach Combat level 50",             "Combat", 500, () => player.CombatLevel >= 50, comb);
            A("comb_combat100",   "Legendary Fighter",    "Reach Combat level 100",            "Combat", 2000,() => player.CombatLevel >= 100, comb);
            A("comb_ranged25",    "Sharpshooter",         "Reach Ranged level 25",             "Combat", 200, () => player.RangedLevel >= 25, comb);
            A("comb_magic25",     "Apprentice Mage",      "Reach Elemental level 25",          "Combat", 200, () => player.ElementalLevel >= 25, comb);

            // ── Gathering ──
            A("gath_chop1",       "Lumberjack",           "Chop your first tree",              "Gathering", 15,  () => player.WoodcuttingLevel > 1 || player.Logs > 0, gath);
            A("gath_logs100",     "Timber Baron",         "Accumulate 100 logs",               "Gathering", 150, () => player.Logs + player.BirchLogs + player.OakLogs + player.PineLogs + player.ArcticLogs >= 100, gath);
            A("gath_fish1",       "First Catch",          "Catch your first fish",             "Gathering", 15,  () => player.Fish > 0, gath);
            A("gath_fish50",      "Master Angler",        "Catch 50 fish",                     "Gathering", 200, () => player.Fish >= 50, gath);
            A("gath_mine1",       "Prospector",           "Mine your first rock",              "Gathering", 15,  () => player.MiningLevel > 1 || player.StoneOre > 0 || player.CopperOre > 0, gath);
            A("gath_gold_ore",    "Gold Rush",            "Mine some Gold Ore",                "Gathering", 200, () => player.GoldOre > 0, gath);
            A("gath_crystal",     "Crystal Clear",        "Mine some Crystals",                "Gathering", 300, () => player.Crystals > 0, gath);
            A("gath_wc50",        "Woodcutting 50",       "Reach Woodcutting level 50",        "Gathering", 400, () => player.WoodcuttingLevel >= 50, gath);
            A("gath_fish_lv50",   "Fishing 50",           "Reach Fishing level 50",            "Gathering", 400, () => player.FishingLevel >= 50, gath);
            A("gath_mine50",      "Mining 50",            "Reach Mining level 50",             "Gathering", 400, () => player.MiningLevel >= 50, gath);
            A("gath_harvest10",   "Green Thumb",          "Harvest 10 crops",                  "Gathering", 100, () => cropsHarvested >= 10, gath);
            A("gath_harvest50",   "Farmer Supreme",       "Harvest 50 crops",                  "Gathering", 400, () => cropsHarvested >= 50, gath);

            // ── Crafting ──
            A("craf_first",       "DIY Beginner",         "Reach Crafting level 2",            "Crafting", 25,  () => player.CraftingLevel >= 2, craf);
            A("craf_craft25",     "Artisan",              "Reach Crafting level 25",           "Crafting", 200, () => player.CraftingLevel >= 25, craf);
            A("craf_smith10",     "Apprentice Smith",     "Reach Blacksmith level 10",         "Crafting", 150, () => player.BlacksmithLevel >= 10, craf);
            A("craf_smith50",     "Master Smith",         "Reach Blacksmith level 50",         "Crafting", 500, () => player.BlacksmithLevel >= 50, craf);
            A("craf_enchant10",   "Enchantment Novice",   "Reach Enchanting level 10",         "Crafting", 150, () => player.EnchantingLevel >= 10, craf);
            A("craf_cook15",      "Master Chef",          "Cook 15 meals",                     "Crafting", 150, () => mealsCooked >= 15, craf);
            A("craf_cook50",      "Legendary Chef",       "Cook 50 meals",                     "Crafting", 500, () => mealsCooked >= 50, craf);

            // ── Social ──
            A("soci_friend1",     "Making Friends",       "Reach friendship level 10 with any NPC", "Social", 50,  () => friendNPCs.Any(f => f.Friendship >= 10), soci);
            A("soci_bestfriend",  "Best Friend",          "Max out friendship with any NPC",        "Social", 300, () => friendNPCs.Any(f => f.Friendship >= 100), soci);
            A("soci_popular",     "Popular",              "Have 5 NPCs at friendship 20+",          "Social", 500, () => friendNPCs.Count(f => f.Friendship >= 20) >= 5, soci);
            A("soci_cards_win",   "Card Shark",           "Win 10 card games",                      "Social", 200, () => player.EuchreWins + player.FiveHundredWins + player.SequenceWins >= 10, soci);
            A("soci_cards_win50", "Grand Master",         "Win 50 card games",                      "Social", 1000,() => player.EuchreWins + player.FiveHundredWins + player.SequenceWins >= 50, soci);

            // ── Wealth ──
            A("weal_100",         "Pocket Change",        "Have $100 on hand",                 "Wealth", 0,   () => player.Money >= 100, weal);
            A("weal_1000",        "Comfortable",          "Have $1,000 on hand",               "Wealth", 0,   () => player.Money >= 1000, weal);
            A("weal_10000",       "Wealthy",              "Have $10,000 on hand",              "Wealth", 0,   () => player.Money >= 10000, weal);
            A("weal_100000",      "Tycoon",               "Have $100,000 on hand",             "Wealth", 0,   () => player.Money >= 100000, weal);
            A("weal_bank",        "Banker",               "Open a bank account",               "Wealth", 50,  () => bankSignedUp, weal);
            A("weal_bank1000",    "Saver",                "Bank $1,000",                       "Wealth", 100, () => bankBalance >= 1000, weal);
            A("weal_house1",      "Homeowner",            "Own a land plot",                   "Wealth", 200, () => ownedHousePlots.Count >= 1, weal);
            A("weal_house3",      "Property Mogul",       "Own 3 land plots",                  "Wealth", 500, () => ownedHousePlots.Count >= 3, weal);
            A("weal_vehicle",     "Licensed Driver",      "Own a vehicle",                     "Wealth", 100, () => vehicles.Count >= 1, weal);

            // ── Mastery ──
            A("mast_allskills10", "Jack of All Trades",   "Get 10 skills to level 10+",        "Mastery", 500,  () => CountSkillsAtLevel(10) >= 10, mast);
            A("mast_allskills25", "Skilled",              "Get 10 skills to level 25+",        "Mastery", 1000, () => CountSkillsAtLevel(25) >= 10, mast);
            A("mast_anyskill100", "True Mastery",         "Get any skill to level 100",        "Mastery", 2000, () => cheatSkills.Any(s => s.get() >= 100), mast);
            A("mast_tutorial",    "Graduate",             "Complete the tutorial",              "Mastery", 50,   () => tutorialCompleted, mast);
            A("mast_id",          "Citizen",              "Get your ID card",                  "Mastery", 50,   () => idClaimed, mast);

            // ── Misc ──
            A("misc_sport5",      "Sporty",               "Play 5 sports matches",             "Misc", 100, () => sportPlayCounts.Values.Sum() >= 5, misc);
            A("misc_minigame5",   "Fun & Games",          "Play 5 minigames",                  "Misc", 100, () => minigamePlayCounts.Values.Sum() >= 5, misc);
            A("misc_night",       "Night Owl",            "Stay up past midnight",             "Misc", 25,  () => timeOfDay * 24f >= 0f && timeOfDay * 24f < 4f, misc);
            A("misc_rain",        "Singing in the Rain",  "Experience rain",                   "Misc", 15,  () => isRaining, misc);
            A("misc_dropzone",    "VIP",                  "Get a Dropzone membership card",    "Misc", 50,  () => hasDropzoneCard, misc);
            A("misc_playtime2h",  "Dedicated",            "Play for 2 hours",                  "Misc", 100, () => totalPlayTime >= 7200, misc);
            A("misc_playtime10h", "Addicted",             "Play for 10 hours",                 "Misc", 500, () => totalPlayTime >= 36000, misc);
        }

        static HashSet<string> achievementVisited = new();
        static float pmAchScrollY = 0f;
        static int   pmAchCategory = 0;

        static int CountSkillsAtLevel(int minLevel)
        {
            int count = 0;
            foreach (var s in cheatSkills)
                if (s.get() >= minLevel) count++;
            return count;
        } 

        // ── BESTIARY ──
        class BestiaryEntry
        {
            public string Type;
            public int    Kills;
            public bool   Discovered;
            public string Location;     // biome where found
            public int    HP;
            public int    CombatXP;
            public string[] Drops;      // possible drop names
            public string[] DropRates;  // matching rate strings
            public Color  EnemyColor;
        }

        static Dictionary<string, BestiaryEntry> bestiary = new();
        static float pmBestScrollY = 0f;
        static int   pmBestFilter = 0; // 0=All, 1=Discovered, 2=Undiscovered

        static void InitBestiary()
        {
            bestiary.Clear();

            void B(string type, string loc, int hp, int xp, Color col, string[] drops, string[] rates)
                => bestiary[type] = new BestiaryEntry { Type=type, Location=loc, HP=hp, CombatXP=xp, EnemyColor=col, Drops=drops, DropRates=rates };

            B("Wild Dog",       "Grasslands / Farm",  3,  20,
              new Color((byte)139,(byte)90,(byte)43,(byte)255),
              new[] { "Bone", "Fur", "Dog Fang" },
              new[] { "100%", "20%", "5%" });

            B("Wolf",           "Forest",             5,  35,
              Color.DarkGray,
              new[] { "Fur", "Bone", "Wolf Claw" },
              new[] { "100%", "25%", "8%" });

            B("Scorpion",       "Desert",             4,  30,
              new Color((byte)180,(byte)120,(byte)0,(byte)255),
              new[] { "Stinger", "Stinger (x2)", "Venom Sac" },
              new[] { "100%", "20%", "6%" });

            B("Bear",           "Snow Zone",          8,  50,
              new Color((byte)100,(byte)100,(byte)120,(byte)255),
              new[] { "Bear Pelt", "Fur", "Bear Claw" },
              new[] { "100%", "30%", "7%" });

            B("Crab",           "Beach",              10, 55,
              new Color((byte)210,(byte)80,(byte)30,(byte)255),
              new[] { "Crab Claw", "Crab Claw (x2)", "Crab Shell" },
              new[] { "100%", "25%", "8%" });

            B("Shark",          "Ocean",              13, 70,
              new Color((byte)70,(byte)100,(byte)140,(byte)255),
              new[] { "Shark Fin", "Shark Fin (x2)", "Shark Tooth" },
              new[] { "100%", "20%", "5%" });

            B("Snake",          "Swamp",              10, 55,
              new Color((byte)40,(byte)100,(byte)40,(byte)255),
              new[] { "Snake Skin", "Stinger", "Snake Fang" },
              new[] { "100%", "20%", "6%" });

            B("Crocodile",      "Swamp",              14, 75,
              new Color((byte)50,(byte)80,(byte)30,(byte)255),
              new[] { "Croc Scale", "Croc Scale (x2)", "Croc Tooth" },
              new[] { "100%", "25%", "7%" });

            B("Fire Lizard",    "Volcano",            12, 65,
              new Color((byte)180,(byte)60,(byte)10,(byte)255),
              new[] { "Lizard Scale", "Lizard Scale (x2)", "Ember Stone" },
              new[] { "100%", "20%", "6%" });

            B("Magma Beetle",   "Volcano",            15, 80,
              new Color((byte)120,(byte)30,(byte)0,(byte)255),
              new[] { "Magma Shard", "Magma Shard (x2)", "Lava Core" },
              new[] { "100%", "25%", "5%" });

            B("Eagle",          "Mountains",          11, 60,
              new Color((byte)100,(byte)70,(byte)20,(byte)255),
              new[] { "Feather", "Feather (x2)", "Eagle Talon" },
              new[] { "100%", "30%", "7%" });

            B("Mountain Goat",  "Mountains",          13, 70,
              new Color((byte)200,(byte)195,(byte)185,(byte)255),
              new[] { "Horn", "Fur", "Goat Hoof" },
              new[] { "100%", "25%", "8%" });

            B("Warrior",        "Grasslands",          6, 60,
              new Color((byte)150,(byte)150,(byte)165,(byte)255),
              new[] { "Iron Armour Piece" },
              new[] { "20%" });

            B("Wizard",         "Grasslands",          5, 60,
              new Color((byte)90,(byte)60,(byte)150,(byte)255),
              new[] { "Mage Robe Piece" },
              new[] { "20%" });

            B("Archer",         "Grasslands",          5, 55,
              new Color((byte)120,(byte)85,(byte)45,(byte)255),
              new[] { "Leather Armour Piece" },
              new[] { "20%" });

            B("Goblin",         "Grasslands",          4, 30,
              new Color((byte)90,(byte)150,(byte)70,(byte)255),
              new[] { "Bone" },
              new[] { "Rare" });

            B("Thug",           "City",                7, 40,
              new Color((byte)70,(byte)70,(byte)80,(byte)255),
              new[] { "Bone" },
              new[] { "Rare" });

            B("Robber",         "City",                6, 38,
              new Color((byte)45,(byte)45,(byte)55,(byte)255),
              new[] { "Bone" },
              new[] { "Rare" });

            B("Gangster",       "City",                8, 45,
              new Color((byte)30,(byte)30,(byte)35,(byte)255),
              new[] { "Bone" },
              new[] { "Rare" });

            B("Giant Bug",      "Forest",              9, 50,
              new Color((byte)60,(byte)90,(byte)45,(byte)255),
              new[] { "Stinger", "Venom Sac" },
              new[] { "50%", "10%" });
        }

        static void RecordBestiaryKill(string enemyType, string biome)
        {
            if (!bestiary.TryGetValue(enemyType, out var entry)) return;
            entry.Kills++;
            if (!entry.Discovered)
            {
                entry.Discovered = true;
                entry.Location = biome ?? entry.Location;
                ShowNotification($"Bestiary: {enemyType} discovered!");
            }
        }

        // RAIN
        static bool isRaining = false;
        static float rainTimer = 0f;
        static float rainInterval = 500f;
        static float rainDuration = 30f;

        // ── WORLD EVENTS ──
static WorldEvent activeWorldEvent = null;
static float worldEventCheckTimer = 0f;
static float worldEventCooldown = 0f;        // seconds until next event can roll
static List<Enemy> eventEnemies = new();      // temporary enemies spawned by events
static NPC eventNPC = null;                   // temporary NPC (merchant, lost child, etc.)
static float eventShopDiscount = 0f;          // 0.0–0.5, applied during Harvest Festival
static bool eventDroughtActive = false;
static float eventScreenShake = 0f;           // earthquake shake remaining

enum WorldEventType
{
    TravellingMerchant, TreasureGoblin, GoblinRaid, MeteorCrash,
    BanditAttack, ForestFire, Blizzard, DragonSighting,
    LostChild, FishingTournament, HarvestFestival,
}

class WorldEvent
{
    public WorldEventType Type;
    public Vector2 Position;          // world-space epicentre
    public float Duration;            // total seconds this event lasts
    public float Timer;               // counts up
    public float Radius;              // area of effect
    public bool Completed;            // player resolved it early
    public bool Announced;            // opening notification sent
    public int Data;                  // generic int (kills counted, fish caught, etc.)
    public string BiomeRequirement;   // only spawn in this biome (null = any)
}

        // MUSIC
        static Music musicMainMenu;
        static Music musicForest;
        static Music musicSnow;
        static Music musicBeach;
        static Music musicDesert;
        static Music musicRain;
        static Music musicSafezone;
        static Music musicFarm;
        static Music musicOcean;
        static Music musicVolcano;
        static Music musicCity;
        static Music musicMeadowlands;
        static Music musicTakeaways;
        static Music musicDbar;
        static Music musicHouse;
        static Music currentMusic;
        static Music lastZoneMusic;
        static Music musicBeforeRain = default;

        // SOUND
        static Sound soundTreeChop;
        static Sound soundTreeFall;
        static Sound soundRockHit;
        static Sound soundRockBreak;
        static Sound soundDoorOpen;
        static Sound soundDoorClose;
        static Sound soundVehicleDrive;
        static Sound soundBikeDrive;
        public static Sound soundHorseGallop;
        public static Sound soundCarHorn;
        static Sound soundPlayerWalk;
        static Sound soundPauseOpen;
        static Sound soundPauseClose;
        static Sound soundSwordSwing;
        static Sound soundStickSwing;
        static Sound soundDogHit;
        static Sound soundDogDie;
        static Sound soundWolfHit;
        static Sound soundWolfDie;
        public static Sound soundHorn;

        // MUSIC

        static float musicFadeTimer = 0f;
        static float musicFadeDuration = 2.0f; // seconds to fade out
        static Music pendingMusic = default;
        static bool isFadingOut = false;
        static bool musicLoaded = false;
        static float musicVolume = 0.6f; // 0..1
        public static float soundVolume = 0.7f;
        static bool musicPlaying = false;
        static bool mainMenuChoice = false; // false = showing menu, true = showing name entry
        static bool wardrobeOpen = false;
        static int wardrobeTab = 0; // 0 = shirt, 1 = skin, 2 = pants
        public static bool bedMenuOpen = false;
        static int bedMenuSelected = 0; 

        // CHEST ITEMS
        static bool chestOpen = false;
        static string chestFullMessage = "";
        static float chestFullTimer = 0f;
        static PlacedChest EnsureHouseChest(string houseKey, Vector2 worldPos)
        {
            var existing = placedChests.FirstOrDefault(c => c.BuildingContext == houseKey);
            if (existing != null) return existing;

            var chest = new PlacedChest {
                Id = "chest_" + houseKey,
                Position = worldPos,
                BuildingContext = houseKey,
                Tier = 0,   // starter chest, 20 slots
            };
            placedChests.Add(chest);
            return chest;
        }
        
        // INITIALIZATION
        static bool isLoadingGame = false;
        static bool slotSelected = false;

        // Font
        static Font uiFont; 
        static bool uiFontLoaded = false;
        static void LoadUIFont()
        {
            // ASCII + bullets/dashes + Māori macrons (for "Kaumātua", "Māori" etc.)
            int[] extra = {
                0x2022, 0x2013, 0x2014, 0x2018, 0x2019, 0x201C, 0x201D,   // • – — ' ' " "
                0x0100, 0x0101, 0x0112, 0x0113, 0x012A, 0x012B,           // Ā ā Ē ē Ī ī
                0x014C, 0x014D, 0x016A, 0x016B                            // Ō ō Ū ū
            };
            int[] cps = new int[95 + extra.Length];
            for (int i = 0; i < 95; i++) cps[i] = 32 + i;
            extra.CopyTo(cps, 95);

            if (System.IO.File.Exists("resources/fonts/Inter_18pt-Regular.ttf"))   // CHANGED
            {
                uiFont = Raylib.LoadFontEx("resources/fonts/Inter_18pt-Regular.ttf", 48, cps, cps.Length);
                Raylib.SetTextureFilter(uiFont.Texture, TextureFilter.Bilinear);          
                uiFontLoaded = true;
            }
            else
            {
                uiFont = Raylib.GetFontDefault();   
            }
        }

        public static int MeasureTextUI(string text, int fontSize)
            => (int)Raylib.MeasureTextEx(uiFont, text, fontSize, fontSize / 16f).X;

        // MISC
        static NPC gymCounterNPC = null;
        static bool trolleyPickedUp = false;
        static bool basketPickedUp = false;
        static List<string> trolleyInventory = new List<string>(new string[20]);
        static List<string> basketInventory = new List<string>(new string[10]);
        static int trolleySelectedSlot = -1;
        static int basketSelectedSlot = -1;
        static bool supermarketInventoryOpen = false;
        // calendar
        static int dayOfMonth = 1;                 // 1..14
        static int currentMonth = 0;               // 0..11 -> January..December
        static string[] monthNames = {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };
        static string[] seasons = { "Summer", "Autumn", "Winter", "Spring" };
        // map month -> season index (example southern hemisphere ordering; adjust if you want northern)
        static int[] monthToSeason = {
            0, // Jan -> Summer
            0, // Feb -> Summer
            1, // Mar -> Autumn
            1, // Apr -> Autumn
            1, // May -> Autumn
            2, // Jun -> Winter
            2, // Jul -> Winter
            2, // Aug -> Winter
            3, // Sep -> Spring
            3, // Oct -> Spring
            3, // Nov -> Spring
            0  // Dec -> Summer
        };

        enum ElevationType { StairsUp, StairsDown, Hill }
        static List<ElevationZone> elevationZones = new();
        static float playerElevation = 0f;       // current visual Y offset
        static float playerElevationTarget = 0f;
        static List<(Vector2 pos, float radius, Color color)> desertPatches = new();
        static List<(Vector2 pos, Color color)> desertRocks = new();
        static List<(Vector2 pos, float radius)> snowPatches = new();
        static List<(Vector2 pos, Color color)> snowRocks = new();
        static List<(Vector2 pos, float radius, Color color)> forestPatches = new();
        static List<(Vector2 pos, Color color)> forestMushrooms = new();
        static List<(Vector2 pos, Color color)> grasslandFlowers = new();
        static List<(Vector2 pos, float radius, Color color)> grasslandPatches = new();
        static List<(Vector2 pos, float radius, Color color)> oceanPatches = new();
        static List<(Vector2 pos, Color color)> oceanCoral = new();
        static List<(Vector2 pos, Color color)> oceanShells = new();
        static List<(Vector2 pos, float radius, Color color)> swampPatches = new();
        static List<(Vector2 pos, Color color)> swampReeds = new();
        static List<(Vector2 pos, Color color)> swampLilies = new();
        static List<(Vector2 pos, float radius, Color color)> volcanoPatches = new();
        static List<(Vector2 pos, Color color)> lavaVents = new();
        static List<(Vector2 pos, float radius, Color color)> mountainPatches = new();
        static List<(Vector2 pos, Color color)> mountainTrees = new();
        static List<Vector2> raindrops = new();
        static List<TreeObject> trees = new();
        public static List<Lake> lakes = new();
        static List<River> rivers = new();
        static List<NPC> npcs = new();
        static NPC talkingToNpc = null;
        static float npcActivityBubbleTimer = 0f;
        static List<NPC> FamilyHubNPCs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();
        static List<DecorativeBuilding> decorativeBuildings = new();
        static List<Rideable> rideables = new();
        static List<DecorativeAsset> decorativeAssets = new();

        static List<FloatingText> floatingTexts = new();
        static List<Enemy> enemies = new();
        static List<LootDrop> lootDrops = new();
        static List<(Vector2 pos, string egg, float age)> droppedEggs = new();
        static List<(Vector2 pos, float radius, Color color)> grassPatches = new();
        static List<(Vector2 pos, Color color)> flowers = new();
        static List<Splat> splats = new();
        static List<DeathFx> deathFx = new();

        // MULTIPLAYER
        static MultiplayerManager multiplayer = new MultiplayerManager();
        static List<RemotePlayer> remotePlayers => multiplayer.RemotePlayers;
        static string multiplayerIPInput = "192.168.1.";
        string ipInputString = multiplayerIPInput;
        static bool multiplayerMenuOpen = false;
        static float chatInputCooldown = 0f;

        // CHAT INTERACTION
        static bool chatInputOpen = false;
        static string chatInputText = "";
        static string playerChatMessage = "";
        static float playerChatTimer = 0f;
        static float playerChatDuration = 5f;
        
        static bool worldTextureGenerated = false;
        static Building currentBuilding = null;
        static float shakeDuration = 0f;
        static float shakeMagnitude = 6f;

        public static List<TutorialGate> tutorialGates = new()
        {
            new TutorialGate { Bounds=new Rectangle(-1750, -9700, 120, 20), UnlockedByStep=0, Label="Woodcutting" },
            new TutorialGate { Bounds=new Rectangle(-1650, -9700, 120, 20), UnlockedByStep=1, Label="Mining" },
            new TutorialGate { Bounds=new Rectangle(-1550, -9700, 120, 20), UnlockedByStep=2, Label="Fishing" },
            new TutorialGate { Bounds=new Rectangle(-1850, -9700, 120, 20), UnlockedByStep=3, Label="Exit Farm" },
        };
    
        // ── SWIMMING ──
        static bool swimmingActive = false;
        static string swimmingPoolType = ""; // "lane" or "diving"
        static float swimLapTimer = 0f;
        static float swimLapDuration = 4f;
        static int swimLapsCompleted = 0;
        static bool divingActive = false;
        static float divingBarPos = 0f;
        static float divingBarDir = 1f;
        static float divingBarSpeed = 0.4f;
        static int divingScore = 0;
        static bool divingJumped = false;
        static float divingFallTimer = 0f;
        static string divingResult = "";
        static float divingResultTimer = 0f;
        static float swimSpeed = 0f;          // current momentum
        static bool swimLeftNext = true;      // which stroke key is expected
        static float swimStrokeWindow = 0f;   // timing window feedback
        static int swimPerfectStrokes = 0;

        static int divingStage = 0;           // 0 = power, 1 = rotation timing, 2 = entry, 3 = result
        static float divingPower = 0f;
        static bool divingPowerUp = true;
        static float divingRotation = 0f;
        static float divingRotDir = 1f;
        static float divingEntry = 0f;
        static bool divingEntryUp = true;

        // Cutscenes

        // Mailbox
        static List<(Vector2 pos, int houseIndex)> mailboxes = new();

        static List<(Vector2 pos, int houseIndex)> extraMailboxes = new();

        // Plushies
        public static readonly string[] plushCommon = {
    "Teddy","Bunny","Robot","Star","Frog","Kitty","Panda","Dino","Puppy","Duck",
    "Whale","Octopus","Penguin","Owl","Fox","Hedgehog","Turtle","Snail","Bee","Ladybug",
    "Mouse","Sheep","Cow","Pig","Chick","Seal","Crab","Jellyfish","Mushroom","Cactus",
    "Cloud","Raindrop","Sun","Moon","Kiwi Bird"
};
public static readonly string[] plushRare = {
    "Golden Teddy","Ruby Dragon","Sapphire Unicorn","Emerald Serpent","Shadow Wolf",
    "Crystal Deer","Phoenix Chick","Yeti","Kraken","Royal Corgi"
};
public static readonly string[] plushSuperRare = {
    "Diamond Kiwi","Galaxy Whale","Ancient Golem","Rainbow Phoenix","C RIDE Plush"
};

public static string[] dailyPlushStock = new string[10];
public static bool[] dailyPlushTaken = new bool[10];
public static Dictionary<string, int> plushiesOwned = new();   // name -> count (dupes tracked)
static bool plushLogOpen = false;

public static bool IsRarePlush(string n) => Array.IndexOf(plushRare, n) >= 0;
public static bool IsSuperRarePlush(string n) => Array.IndexOf(plushSuperRare, n) >= 0;
public static Color PlushColor(string n)
{
    int h = n.GetHashCode();
    return new Color((byte)(110 + (h & 0x7F)), (byte)(110 + ((h >> 7) & 0x7F)), (byte)(110 + ((h >> 14) & 0x7F)), (byte)255);
}

static void RollDailyPlushStock()
{
    for (int i = 0; i < 10; i++)
    {
        int roll = Raylib.GetRandomValue(1, 100);   // 5% super rare, 15% rare, 80% common — duplicates allowed
        dailyPlushStock[i] = roll <= 5  ? plushSuperRare[Raylib.GetRandomValue(0, plushSuperRare.Length - 1)]
                           : roll <= 20 ? plushRare[Raylib.GetRandomValue(0, plushRare.Length - 1)]
                           : plushCommon[Raylib.GetRandomValue(0, plushCommon.Length - 1)];
        dailyPlushTaken[i] = false;
    }
}

public static string RegisterPlushWin(int slot, string name)
{
    dailyPlushTaken[slot] = true;
    player.PlushPrizes++;
    bool isNew = !plushiesOwned.ContainsKey(name);
    plushiesOwned[name] = plushiesOwned.GetValueOrDefault(name) + 1;
    string rarity = IsSuperRarePlush(name) ? "SUPER RARE " : IsRarePlush(name) ? "RARE " : "";
    return isNew ? $"NEW {rarity}plushie: {name}!  Set: {plushiesOwned.Count}/50"
                 : $"You won a {rarity}{name} (duplicate).  Set: {plushiesOwned.Count}/50";
}

        // Collectables
        class Collectable
        {
            public string Id;        // unique, used as the save key
            public string Name;
            public string Scene;     // "World", "Underwater", "Space", "Building:LIBRARY", ...
            public Vector2 Position; // in that scene's coordinate space
            public bool Found;
        }

        static List<Collectable> collectables = new()
        {
            // World (tuck these behind trees/rocks — tweak to taste)
            new Collectable{ Id="c_world_pond",   Name="Golden Kiwi Statue",  Scene="World", Position=new Vector2(-2900, -8200) },
            new Collectable{ Id="c_world_jungle", Name="Jade Tiki",           Scene="World", Position=new Vector2(5600, -2150) },
            new Collectable{ Id="c_world_arctic", Name="Frozen Rose",         Scene="World", Position=new Vector2(-24800, 150) },
            // Underwater world (20000x20000 space)
            new Collectable{ Id="c_uw_wreck",     Name="Sunken Doubloon",     Scene="Underwater", Position=new Vector2(3000, 15000) },
            new Collectable{ Id="c_uw_trench",    Name="Black Pearl",         Scene="Underwater", Position=new Vector2(17500, 4200) },
            // Space
            new Collectable{ Id="c_sp_belt",      Name="Meteor Core",         Scene="Space", Position=new Vector2(2500, 3000) },
            new Collectable{ Id="c_sp_deep",      Name="Alien Coin",          Scene="Space", Position=new Vector2(18000, 16000) },
            // Building interiors (screen-space interior coords)
            new Collectable{ Id="c_b_library",    Name="First Edition Book",  Scene="Building:LIBRARY", Position=new Vector2(1160, 620) },
            new Collectable{ Id="c_b_dbar",       Name="Vintage Bottlecap",   Scene="Building:DBar",    Position=new Vector2(150, 850) },
            new Collectable{ Id="c_b_school",     Name="Trophy of '86",       Scene="Building:SCHOOL",  Position=new Vector2(1900, 1900) },
        };
        static int CollectablesFound => collectables.Count(c => c.Found);

        static void UpdateCollectables(string scene, Vector2 playerPos, float pickupRadius = 45f)
        {
            foreach (var c in collectables)
            {
                if (c.Found || c.Scene != scene) continue;
                if (Vector2.Distance(playerPos, c.Position) < pickupRadius)
                {
                    c.Found = true;
                    player.Money += 25;
                    ShowNotification($"Hidden Collectable: {c.Name}! ({CollectablesFound}/{collectables.Count})");
                }
            }
        }

        static void DrawCollectables(string scene)
        {
            float t = (float)Raylib.GetTime();
            foreach (var c in collectables)
            {
                if (c.Found || c.Scene != scene) continue;
                int x = (int)c.Position.X, y = (int)c.Position.Y;
                float pulse = MathF.Sin(t * 4f + x * 0.01f) * 0.5f + 0.5f;   // subtle shimmer so it's findable but hidden
                byte a = (byte)(90 + pulse * 140);
                Raylib.DrawCircle(x, y, 5 + pulse * 3, new Color((byte)255,(byte)215,(byte)0, a));
                Raylib.DrawCircleLines(x, y, (int)(11 + pulse * 4), new Color((byte)255,(byte)240,(byte)140, a));
            }
        }

        // TradingCards
        class TradingCard   // consider renaming to TradingCard later, kept as-is for now to avoid a big rename pass
        {
        public string Name;
        public int Dex;
        public CardRarity Rarity;
        public bool ReverseHolo;
        public CardFranchise Franchise = CardFranchise.Pokemon;   
        public int Power; 
        }

        class CardSet
        {
            public string SetName;
            public string CoverTitle;
            public Color CoverColor;
            public List<TradingCard> Pool = new();
            public List<BinderSlot> Slots = new();
            public CardBinder MasterSet = new();
            public CardBinder Personal = new();
            public int PacksOpened = 0; 
        }
        static List<CardSet> cardSets = new();
        static bool lastPackWasGod = false;
        static int activeCardSetIndex = 0;
        enum CardRarity { Common, Uncommon, Rare, Holo, Trainer, Energy, UltraRare, SecretRare }
        enum CardFranchise { Pokemon, DragonBallZ }
        static bool binderOpen = false;
        static int binderPageIndex = 0;   // 0 = cover-adjacent single page, 1+ = two-page spreads
        class BinderSlot { public TradingCard Card; public bool IsReverse; }
        static List<BinderSlot> binderSlots = new();
        static int cardContextMenuSlot = -1;
        static int heldSingleCardSlot = -1;
        static float pageFlipTimer = 0f;
        static int pageFlipDir = 0;  
        static (int start, int count) GetSinglePageRange() => (0, 9);
        static (int leftStart, int rightStart) GetSpreadRanges(int spread)
        {
            int leftStart = 9 + (spread - 1) * 18;
            int rightStart = leftStart + 9;
            return (leftStart, rightStart);
        }
        static int MaxBinderPage()
        {
            int remaining = cardSets[activeCardSetIndex].Slots.Count - 9;   
            if (remaining <= 0) return 0;
            return (int)Math.Ceiling(remaining / 18f);
        }
        static int TotalBinderSlots() => cardSets[activeCardSetIndex].Slots.Count;
        static string[] pokemonNames = {
            "Bulbasaur","Ivysaur","Venusaur","Charmander","Charmeleon","Charizard","Squirtle","Wartortle","Blastoise",
            "Caterpie","Metapod","Butterfree","Weedle","Kakuna","Beedrill","Pidgey","Pidgeotto","Pidgeot","Rattata",
            "Raticate","Spearow","Fearow","Ekans","Arbok","Pikachu","Raichu","Sandshrew","Sandslash","Nidoran F",
            "Nidorina","Nidoqueen","Nidoran M","Nidorino","Nidoking","Clefairy","Clefable","Vulpix","Ninetales",
            "Jigglypuff","Wigglytuff","Zubat","Golbat","Oddish","Gloom","Vileplume","Paras","Parasect","Venonat",
            "Venomoth","Diglett","Dugtrio","Meowth","Persian","Psyduck","Golduck","Mankey","Primeape","Growlithe",
            "Arcanine","Poliwag","Poliwhirl","Poliwrath","Abra","Kadabra","Alakazam","Machop","Machoke","Machamp",
            "Bellsprout","Weepinbell","Victreebel","Tentacool","Tentacruel","Geodude","Graveler","Golem","Ponyta",
            "Rapidash","Slowpoke","Slowbro","Magnemite","Magneton","Farfetchd","Doduo","Dodrio","Seel","Dewgong",
            "Grimer","Muk","Shellder","Cloyster","Gastly","Haunter","Gengar","Onix","Drowzee","Hypno","Krabby",
            "Kingler","Voltorb","Electrode","Exeggcute","Exeggutor","Cubone","Marowak","Hitmonlee","Hitmonchan",
            "Lickitung","Koffing","Weezing","Rhyhorn","Rhydon","Chansey","Tangela","Kangaskhan","Horsea","Seadra",
            "Goldeen","Seaking","Staryu","Starmie","MrMime","Scyther","Jynx","Electabuzz","Magmar","Pinsir","Tauros",
            "Magikarp","Gyarados","Lapras","Ditto","Eevee","Vaporeon","Jolteon","Flareon","Porygon","Omanyte",
            "Omastar","Kabuto","Kabutops","Aerodactyl","Snorlax","Articuno","Zapdos","Moltres","Dratini","Dragonair",
            "Dragonite","Mewtwo","Mew"
        };
        static (string name, string[] titles)[] crewCharacters = {
            ("JDogg",  new[]{ "the Rookie","the Loyal","the Wanderer","the Quick","the Bold","the Fearless","the Ironfist","the Relentless","the Conqueror","the Unbreakable","the Almighty","God of the Realm" }),
            ("Cride",  new[]{ "the Swift","the Clever","the Scout","the Sharp","the Silent","the Huntress","the Cunning","the Nightblade","the Stormcaller","the Merciless","the Valkyrie","Queen of Shadows" }),
            ("Traz",   new[]{ "the Sturdy","the Brawler","the Gruff","the Steady","the Stubborn","the Bonecrusher","the Juggernaut","the Warhound","the Earthshaker","the Colossus","the Titan","Breaker of Worlds" }),
            ("Nola",   new[]{ "the Gentle","the Healer","the Kind","the Patient","the Wise","the Lightbringer","the Blessed","the Radiant","the Miracle Worker","the Divine","the Archangel","Saint of the Dawn" }),
            ("Jake",   new[]{ "the Joker","the Lucky","the Chancer","the Grinner","the Gambler","the Trickster","the Card Shark","the Escape Artist","the Mastermind","the Untouchable","the Wildcard","King of Fortune" }),
            ("Shack",  new[]{ "the Fierce","the Proud","the Daring","the Loud","the Restless","the Firebrand","the Berserker","the Unstoppable","the Inferno","the Warqueen","the Dragonheart","Empress of Flame" }),
            ("Tipene", new[]{ "the Calm","the Watchful","the Strong","the Humble","the True","the Guardian","the Stonewall","the Protector","the Mountain","the Immovable","the Chosen One","Spirit of the Land" }),
            ("Joy",    new[]{ "the Cheerful","the Bright","the Warm","the Hopeful","the Sunny","the Songbird","the Uplifter","the Sparkling","the Dazzling","the Heartmender","the Everlight","Goddess of Laughter" }),
            ("Rala",   new[]{ "the Curious","the Dreamer","the Stargazer","the Quiet","the Keen","the Mystic","the Seer","the Spellweaver","the Moonblessed","the Arcane","the Oracle","Weaver of Fate" }),
            ("Hunter", new[]{ "the Small","the Speedy","the Sneaky","the Scrappy","the Brave","the Slingshot","the Tracker","the Daredevil","the Prodigy","the Fearless Cub","the Young Legend","Future King" }),
            ("Eli",    new[]{ "the Tinkerer","the Whiz","the Gadget Kid","the Builder","the Smart","the Inventor","the Spark","the Circuit Master","the Genius","the Machinist","the Technomancer","Architect of Tomorrow" }),
            ("Ezra",   new[]{ "the Runner","the Climber","the Jumper","the Racer","the Zoomer","the Lightning Kid","the Freerunner","the Blur","the Sonic","the Untraceable","the Speedforce","Faster than Time" }),
            ("Eden",   new[]{ "the Gardener","the Green Thumb","the Explorer","the Nature Kid","the Mossy","the Beast Friend","the Wildling","the Forest Scout","the Grove Keeper","the Wild Heart","the Beast King","Voice of the Forest" }),
            ("Ava",    new[]{ "the Sweet","the Singer","the Dancer","the Giggler","the Star","the Melody","the Shining","the Show Stopper","the Diva","the Spotlight","the Superstar","Angel of the Stage" }),
            ("Jasper", new[]{ "the Rock Kid","the Collector","the Digger","the Finder","the Miner","the Gem Hunter","the Treasure Seeker","the Prospector","the Crystal Keeper","the Golden Touch","the Diamond Heart","Lord of the Deep Mines" }),
            ("Whale",  new[]{ "the Big","the Mighty","the Deep","the Chill","the Gentle Giant","the Tidal","the Wavecrusher","the Harpoon Dodger","the Deepsea King","the Unsinkable","the Kraken Tamer","God of the Ocean" }),
            ("Alice",  new[]{ "the Inquisitive","the Bookworm","the Thoughtful","the Sharp-Eyed","the Poised","the Riddlemaster","the Looking Glass","the Strategist","the Grandmaster","the All-Knowing","the Wonderland","Queen of Hearts" }),
            ("Leo",    new[]{ "the Lion","the Golden","the Charmer","the Valiant","the Mane Event","the Roaring","the Sunblade","the Kingsguard","the Lionheart","the Crowned","the Sun King","Constellation of Leo" }),
        };
            
        static string[] carNames = {
            "Sedan","Ute","SUV","Truck","Convertible","Ambulance","Fire Truck","Police Car","Monster Truck",
            "Racing Car","Muscle Car","Vintage Coupe","Pickup Truck","Hatchback","Station Wagon","Limousine",
            "Hot Rod","Rally Car","Formula One Car","Tow Truck","Garbage Truck","School Bus","Double Decker Bus",
            "Tractor","Bulldozer","Excavator","Cement Mixer","Tank","Golf Cart","Go-Kart"
        };
        static string[] nzPlaceNames = {
            "Auckland","Wellington","Christchurch","Hamilton","Tauranga","Napier","Dunedin","Palmerston North",
            "Nelson","Rotorua","New Plymouth","Whangarei","Invercargill","Wanaka","Queenstown","Taupo","Gisborne",
            "Timaru","Masterton","Whanganui","Oamaru","Blenheim","Levin","Greymouth","Kaitaia","Motueka",
            "Ashburton","Rangiora","Cambridge","Matamata"
        };
        static List<TradingCard> allCards = new();

        static List<TradingCard> dbzCards = new();
        static CardBinder dbzMasterSetBinder = new();
        static CardBinder dbzPersonalBinder = new();
        static List<BinderSlot> dbzBinderSlots = new();
        static int packRevealIndex = 0;

        // Quests
        static int wolvesKilled = 0;
        static int cropsHarvested = 0;
        static int mealsCooked = 0;

static NPC rangerNpc = new NPC(new Vector2(-1500, -7000), "Ranger", "Those wolves are getting bold...") { SpriteKey = "Ranger" };
static List<StoryQuest> storyQuests = new()
{
    new StoryQuest { Title = "The Wolf Menace", GiverName = "Ranger", Reward = 500,
        Stages = {
            new QuestStage{ Description="Slay 5 wolves",            Progress=() => wolvesKilled, Target=5 },
            new QuestStage{ Description="Collect 3 Fur",            Progress=() => player.Fur,   Target=3 },
            new QuestStage{ Description="Return to the Ranger (E)", Progress=null },
        }},
    new StoryQuest { Title = "Secrets of the Deep", TriggerSpot = new Vector2(2200, 1500), GiverName = "", Reward = 650,   // put this spot near your dive point
        Stages = {
            new QuestStage{ Description="Find 2 underwater collectables", Progress=() => collectables.Count(c => c.Found && c.Scene=="Underwater"), Target=2 },
            new QuestStage{ Description="Catch 5 fish",                   Progress=() => player.Fish, Target=5 },
            new QuestStage{ Description="Return to the glowing marker",   Progress=null },
        }},
};
static void UpdateStoryQuests()
{
    if (testTransitionActive) return;   

    foreach (var q in storyQuests)
    {
        if (q.Completed) continue;

        if (!q.Started)
        {
            bool nearGiver = q.GiverName == "Ranger" && Vector2.Distance(player.Center, rangerNpc.Position) < 70
                             && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen;
            bool onSpot = q.TriggerSpot != Vector2.Zero && Vector2.Distance(player.Center, q.TriggerSpot) < 45;
            if (nearGiver || onSpot)
            {
                var quest = q;
                StartTestTransition(() => {
                    quest.Started = true;
                    if (quest.Current?.Progress != null) quest.Current.Baseline = quest.Current.Progress();
                    ShowNotification($"Quest started: {quest.Title} — {quest.Current.Description}");
                }, $"QUEST: {quest.Title}");
            }
            continue;
        }

        var st = q.Current;
        if (st == null) continue;

        if (st.Progress != null)   // counter stage
        {
            if (st.Progress() - st.Baseline >= st.Target) AdvanceStoryQuest(q);
        }
        else                       // return stage
        {
            bool atGiver = q.GiverName == "Ranger" && Vector2.Distance(player.Center, rangerNpc.Position) < 70
                           && Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen;
            bool atSpot = q.TriggerSpot != Vector2.Zero && Vector2.Distance(player.Center, q.TriggerSpot) < 45;
            if (atGiver || atSpot) AdvanceStoryQuest(q);
        }
    }
}
static void AdvanceStoryQuest(StoryQuest q)
{
    q.Stage++;
    if (q.Stage >= q.Stages.Count)
    {
        q.Completed = true;
        player.Money += q.Reward;
        AddReputation(50, q.Title);
        ShowLevelUp($"Quest Complete: {q.Title}! +${q.Reward}", 0);
    }
    else
    {
        AddReputation(15, $"{q.Title} stage");
        if (q.Current.Progress != null) q.Current.Baseline = q.Current.Progress();
        ShowNotification($"{q.Title} — next: {q.Current.Description}");
    }
}

        // ── TENNIS ──
        // match state
        static bool tennisActive = false;
        static bool tennisDifficultySelect = false;
        static int tennisPlayerScore = 0;
        static int tennisAIScore = 0;
        static int tennisPointsToWin = 7;

        // players (top-down positions on court)
        static Vector2 tennisPlayerPos;
        static Vector2 tennisAIPos;
        static float tennisPlayerSpeed = 240f;
        static float tennisAIMaxSpeed = 240f;
        static float tennisAILead = 0.12f;

        // ball — has a current position and a target it's travelling toward (bounce point)
        static Vector2 tennisBallPos;
        static Vector2 tennisBallStart;
        static Vector2 tennisBallTarget;
        static float tennisBallT = 0f;          // 0..1 progress along the shot
        static float tennisBallDuration = 0f;   // seconds for the shot to land
        static bool tennisBallInFlight = false;
        static bool tennisBallToPlayer = true;  // who the ball is heading toward
        static bool tennisBallBounced = false;  // has it hit the ground yet this shot

        // serve
        static int tennisServePhase = 0;        // 0 none, 1 ready, 2 tossed
        static bool tennisPlayerServing = true;
        static float tennisServeToss = 0f;      // power bar while serving
        static bool tennisServeTossUp = true;
        static float tennisServeTimer = 0f;     // AI serve delay

        // swing
        static float tennisSwingCooldown = 0f;
        static float tennisSwingTimer = 0f;     // active swing window
        static bool tennisSwinging = false;

        static string tennisMessage = "";
        static float tennisMessageTimer = 0f;
        static string tennisLastResult = "";
        // court bounds in interior screen space
        const float CourtLeft = 120f;
        const float CourtRight = 1160f;
        const float CourtTop = 120f;
        const float CourtBottom = 600f;
        const float CourtMidX = (CourtLeft + CourtRight) / 2f;   // the net

        // ── BASKETBALL ──
        static bool basketballActive = false;
        static float bbPower = 0f;
        static float bbPowerDir = 1f;
        static float bbAimX = 0f;
        static float bbAimDir = 1f;
        static bool bbShooting = false;
        static float bbShotTimer = 0f;
        static int bbScore = 0;
        static int bbAttempts = 0;
        static string bbMessage = "";
        static float bbMessageTimer = 0f;
        static bool bbPowerLocked = false;
        // DropZone
        static bool dropZoneFoodMenuOpen = false;
        static bool prizeCounterOpen = false;

        // Returns true if the play is paid for; deducts card credit and tracks lifetime spend.

        // Upgrades the card tier based on lifetime spend, announcing any promotion.

        // ── MCDONALD'S ──
        static bool mcdonaldsMenuOpen = false;
        static int mcdonaldsSelectedItem = -1;
        static string mcdonaldsMessage = "";
        static float mcdonaldsMessageTimer = 0f;
        static float mcdonaldsOrderTimer = 0f;
        static bool mcdonaldsOrderReady = false;
        static string mcdonaldsOrderName = "";

        // DOMINOES
        static bool dominosMenuOpen = false;
        static string dominosOrderName = "";
        static bool dominosOrderReady = false;
        static float dominosOrderTimer = 0f;
        static string dominosMessage = "";
        static float dominosMessageTimer = 0f;
        static int dominosSelectedItem = -1;

        //KFC
        static bool kfcMenuOpen = false;
        static string kfcOrderName = "";
        static bool kfcOrderReady = false;
        static float kfcOrderTimer = 0f;
        static string kfcMessage = "";
        static float kfcMessageTimer = 0f;
        static int kfcSelectedItem = -1;

        //BURGER KING
        static bool burgerKingMenuOpen = false;
        static string burgerKingOrderName = "";
        static bool burgerKingOrderReady = false;
        static float burgerKingOrderTimer = 0f;
        static string burgerKingMessage = "";
        static float burgerKingMessageTimer = 0f;
        static int burgerKingSelectedItem = -1;

        // Airport
        static bool airportMenuOpen = false;
        static string airportDestination = "";
        static float airportFlightTimer = 0f;
        static bool airportFlying = false;

        // Mall
        static string currentMiniShop = "";   // "" = mall concourse, else shop name
        static (string name, Vector2 doorPos, Color col)[] mallShops = {
            ("CLOTHING", new Vector2(360, 640),  new Color(210, 90, 140, 255)),
            ("ELECTRONICS", new Vector2(760, 640),  new Color(70, 130, 210, 255)),
            ("SPORTS",   new Vector2(1160, 640), new Color(80, 180, 90, 255)),
            ("BOOKS",    new Vector2(360, 1360), new Color(200, 150, 60, 255)),
            ("FOOD COURT", new Vector2(760, 1360), new Color(220, 120, 60, 255)),
            ("TOYS",     new Vector2(1160, 1360), new Color(160, 90, 200, 255)),
        };
        static Dictionary<string, List<Rectangle>> miniShopInteriorObjects = new() {
            { "CLOTHING",    new List<Rectangle>{ new(200,200,300,60), new(200,400,300,60), new(900,200,300,400) } },
            { "ELECTRONICS", new List<Rectangle>{ new(200,200,900,60), new(200,600,900,60), new(600,300,200,200) } },
            { "SPORTS",      new List<Rectangle>{ new(200,200,300,400), new(900,200,300,400), new(560,700,280,60) } },
            { "BOOKS",       new List<Rectangle>{ new(150,150,40,500), new(1210,150,40,500), new(300,300,800,60) } },
            { "FOOD COURT",  new List<Rectangle>{ new(200,150,1000,60), new(300,400,150,150), new(700,400,150,150), new(1000,400,150,150) } },
            { "TOYS",        new List<Rectangle>{ new(200,250,300,300), new(900,250,300,300), new(560,650,280,60) } },
        };

        // Class tests
        
        // ── Maths class minigame ──────────────────────────────
        static string classTestSubject = "";
        static int    ctGridSize   = 3;
        static int[]  ctNumbers    = System.Array.Empty<int>();
        static int[]  ctSortedIdx  = System.Array.Empty<int>();
        static int    ctNextClick  = 0;
        static float  ctTimeLeft   = 60f;
        static int    ctGridsDone  = 0;
        static int    ctFlashIdx   = -1;
        static float  ctFlashTimer = 0f;
        static int ctXpGained = 0;
        static readonly System.Random ctRng = new();

        static void StartClassTest(string subject)
        {
            interiorReturnScene     = currentScene;      
            interiorReturnPos       = player.Position;
            interiorReturnBuilding  = currentBuilding;
            interiorReturnClassroom = currentClassroom;

            classTestSubject = subject;
            int lvl = player.EducationLevel;
            ctGridSize  = System.Math.Clamp(3 + lvl / 5, 3, 7);
            ctTimeLeft  = 60f;
            ctGridsDone = 0;
            BuildClassGrid();
            ChangeScene(SceneState.ClassTest);
        }

        static void BuildClassGrid()
        {
            int cells  = ctGridSize * ctGridSize;
            int maxVal = ctGridSize * ctGridSize;
            ctNumbers   = System.Linq.Enumerable.Range(1, maxVal)
                              .OrderBy(_ => ctRng.Next()).Take(cells).ToArray();
            ctSortedIdx = System.Linq.Enumerable.Range(0, cells)
                              .OrderBy(i => ctNumbers[i]).ToArray();
            ctNextClick = 0;
            ctFlashIdx  = -1;
        }

        // Frame-time logic ONLY. Called from the UPDATE switch, never inside BeginDrawing.
        static void UpdateClassTest(float dt)
        {
            ctTimeLeft   -= dt;
            if (ctFlashTimer > 0f) ctFlashTimer -= dt;

            if (ctTimeLeft <= 0f)
            {
                var sTime = schoolSubjects.First(x => x.Name == classTestSubject);
                int reward = ctGridsDone * sTime.XP;
                ShowNotification($"{classTestSubject} class over! {ctGridsDone} grids  +{reward} Education XP");
                ChangeScene(interiorReturnScene, () => {
                    currentBuilding  = interiorReturnBuilding;
                    currentClassroom = interiorReturnClassroom;
                    player.Position  = interiorReturnPos;
                });
                return;
            }

            Vector2 mouse = Raylib.GetMousePosition();
            bool clicked  = Raylib.IsMouseButtonPressed(MouseButton.Left);
            if (Raylib.IsKeyPressed(KeyboardKey.Q))
            {
                var sQ = schoolSubjects.First(x => x.Name == classTestSubject);
                int reward = ctGridsDone * sQ.XP;
                ShowNotification($"Left {classTestSubject} test. +{reward} Education XP");
                ChangeScene(interiorReturnScene, () => {
                    currentBuilding  = interiorReturnBuilding;
                    currentClassroom = interiorReturnClassroom;
                    player.Position  = interiorReturnPos;
                });
                return;
            }

            if (!clicked) return;

            int gap  = 12;
            int oyTop  = 250;                      
            int availH = ScreenHeight - oyTop - 20; 
            int availW = ScreenWidth  - 80;
            int cell = Math.Min(
                (availW - (ctGridSize - 1) * gap) / ctGridSize,
                (availH - (ctGridSize - 1) * gap) / ctGridSize);
            cell = Math.Clamp(cell, 40, 130);
            int totW = ctGridSize * cell + (ctGridSize - 1) * gap;
            int totH = ctGridSize * cell + (ctGridSize - 1) * gap;
            int ox   = ScreenWidth / 2 - totW / 2;
            int oy   = oyTop + (availH - totH) / 2;

            for (int i = 0; i < ctNumbers.Length; i++)
            {
                int gx = i % ctGridSize, gy = i / ctGridSize;
                var r = new Rectangle(ox + gx * (cell + gap), oy + gy * (cell + gap), cell, cell);
                bool solved = System.Array.IndexOf(ctSortedIdx, i) < ctNextClick;
                if (solved || !Raylib.CheckCollisionPointRec(mouse, r)) continue;

                if (i == ctSortedIdx[ctNextClick])
                {
                    ctNextClick++;
                    if (ctNextClick >= ctNumbers.Length)   // grid complete
                    {
                        ctGridsDone++;
                        player.AddEducationXP(50);          
                        player.MathsRating += 8; 
                        ctXpGained += 50;
                        BuildClassGrid();
                    }
                }
                else { ctNextClick = 0; ctFlashIdx = i; ctFlashTimer = 0.3f; }

                break;
            }
        }

        // Drawing ONLY. Called from the DRAW switch, inside BeginDrawing/EndDrawing.
        static void DrawClassTest()
        {
            var s = schoolSubjects.First(x => x.Name == classTestSubject);
            Raylib.ClearBackground(new Color((byte)26,(byte)30,(byte)42,(byte)255));

            Program.DrawTextUI($"{classTestSubject.ToUpper()} TEST", 40, 30, 40, s.Col);
            Program.DrawTextUI($"Click 1..{ctGridSize*ctGridSize} lowest to highest", 40, 80, 22, Color.LightGray);
            Program.DrawTextUI($"Grids: {ctGridsDone}", 40, 112, 22, Color.White);

            float frac = ctTimeLeft / 60f;
            Raylib.DrawRectangle(40, 230, 300, 20, new Color((byte)50,(byte)50,(byte)60,(byte)255));
            Raylib.DrawRectangle(40, 230, (int)(300 * frac), 20,
                frac < 0.25f ? Color.Red : new Color((byte)90,(byte)200,(byte)120,(byte)255));
            Program.DrawTextUI($"{(int)(ctTimeLeft/60)}:{((int)ctTimeLeft%60):00}", 660, 228, 24, Color.White);

            int gap  = 12;
            
            int oyTop  = 250;                     
            int availH = ScreenHeight - oyTop - 20; 
            int availW = ScreenWidth  - 80;
            int cell = Math.Min(
                (availW - (ctGridSize - 1) * gap) / ctGridSize,
                (availH - (ctGridSize - 1) * gap) / ctGridSize);
            cell = Math.Clamp(cell, 40, 130);
            int totW = ctGridSize * cell + (ctGridSize - 1) * gap;
            int totH = ctGridSize * cell + (ctGridSize - 1) * gap;
            int ox   = ScreenWidth / 2 - totW / 2;
            int oy   = oyTop + (availH - totH) / 2; 
            Vector2 mouse = Raylib.GetMousePosition();

            for (int i = 0; i < ctNumbers.Length; i++)
            {
                int gx = i % ctGridSize, gy = i / ctGridSize;
                var r = new Rectangle(ox + gx * (cell + gap), oy + gy * (cell + gap), cell, cell);
                bool solved = System.Array.IndexOf(ctSortedIdx, i) < ctNextClick;
                bool hover  = Raylib.CheckCollisionPointRec(mouse, r);

                Color fill = solved ? new Color((byte)60,(byte)120,(byte)70,(byte)255)
                           : (i == ctFlashIdx && ctFlashTimer > 0f) ? new Color((byte)170,(byte)50,(byte)50,(byte)255)
                           : hover ? new Color((byte)70,(byte)80,(byte)110,(byte)255)
                                   : new Color((byte)55,(byte)62,(byte)85,(byte)255);
                Raylib.DrawRectangleRec(r, fill);
                Raylib.DrawRectangleLinesEx(r, 2, new Color((byte)30,(byte)34,(byte)48,(byte)255));

                string num = ctNumbers[i].ToString();
                int fs = cell / 2;
                int tw = Raylib.MeasureText(num, fs);
                Program.DrawTextUI(num, (int)(r.X + cell/2 - tw/2), (int)(r.Y + cell/2 - fs/2), fs,
                    solved ? Color.LightGray : Color.White);
            }

            Program.DrawTextUI($"Grids: {ctGridsDone}", 40, 112, 22, Color.White);
            // NEW — level + XP gained this session
            Program.DrawTextUI($"Education Lv {player.EducationLevel}", 40, 142, 20, new Color((byte)90,(byte)140,(byte)230,(byte)255));
            Program.DrawTextUI($"+{ctXpGained} XP this class", 260, 142, 20, new Color((byte)90,(byte)200,(byte)120,(byte)255));
             int xpReq = player.EducationLevel * player.EducationLevel * 50;
            float xpFrac = Math.Clamp((float)player.EducationXP / xpReq, 0f, 1f);
            Raylib.DrawRectangle(40, 218, 300, 10, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangle(40, 218, (int)(300 * xpFrac), 10, new Color((byte)90,(byte)200,(byte)120,(byte)255));
            Program.DrawTextUI($"{player.EducationXP}/{xpReq}", 348, 214, 16, Color.LightGray);

            // NEW — Maths rating bar, mirroring the card-game rating style
            int mr = player.MathsRating;
            string mRank = mr >= 1800 ? "Grand Master" : mr >= 1500 ? "Master" :
                           mr >= 1200 ? "Expert" : mr >= 800 ? "Intermediate" : "Beginner";
            Color mCol = mr >= 1800 ? Color.Gold :
                         mr >= 1500 ? new Color((byte)180,(byte)100,(byte)255,(byte)255) :
                         mr >= 1200 ? Color.SkyBlue :
                         mr >= 800  ? Color.LightGray : Color.DarkGray;
            Program.DrawTextUI($"Maths Rating: {mr}  ({mRank})", 40, 176, 18, mCol);
            float mP = Math.Clamp(mr / 2000f, 0f, 1f);
            Raylib.DrawRectangle(40, 200, 300, 10, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            Raylib.DrawRectangle(40, 200, (int)(300 * mP), 10, mCol);

            Program.DrawTextUI("Q = Leave test early", 40, 940, 18, Color.LightGray);
        }
        
        // SCHOOL
        record SchoolSubject(string Name, int Cost, int XP, Color Col);
        static SchoolSubject[] schoolSubjects = {
            new("Science", 10, 15, new Color((byte)80,(byte)200,(byte)140,(byte)255)),
            new("Maths",   10, 15, new Color((byte)90,(byte)140,(byte)230,(byte)255)),
            new("Art",     10, 15, new Color((byte)230,(byte)120,(byte)200,(byte)255)),
            new("Sports",  10, 15, new Color((byte)230,(byte)160,(byte)50,(byte)255)),
            new("English", 10, 15, new Color((byte)220,(byte)200,(byte)80,(byte)255)),
        };
        static Dictionary<string, List<Rectangle>> classroomInteriorObjects = new() {
        { "Science", new List<Rectangle> { new(300, 400, 800, 60), new(300, 530, 800, 60), new(300, 660, 800, 60) } },
        { "Maths",   new List<Rectangle> { new(300, 400, 800, 60), new(300, 530, 800, 60), new(300, 660, 800, 60) } },
        { "Art",     new List<Rectangle> { new(300, 400, 800, 60), new(300, 530, 800, 60), new(300, 660, 800, 60) } },
        { "Sports",  new List<Rectangle> { new(300, 400, 800, 60), new(300, 530, 800, 60), new(300, 660, 800, 60) } },
        { "English", new List<Rectangle> { new(300, 400, 800, 60), new(300, 530, 800, 60), new(300, 660, 800, 60) } },
        };

        static string currentClassroom = ""; 
        static SceneState interiorReturnScene = SceneState.Building;
        static Vector2    interiorReturnPos   = Vector2.Zero;
        static Building   interiorReturnBuilding = null;
        static string     interiorReturnClassroom = "";  
        static (string subject, Vector2 doorPos)[] classroomDoors = {
            ("Maths",   new Vector2(270, 694)),
            ("Art",     new Vector2(620, 694)),
            ("Science", new Vector2(970, 694)),
            ("Sports",  new Vector2(270, 900)),
            ("English", new Vector2(620, 900)),
        };
        Vector2 gymDoorPos = new Vector2(970, 1650);
        static bool enteredCourtFromSchool = false;
        static Building schoolReturnBuilding = null;

        // Prison
        static float playerDebt = 0f;
        static float debtDueTimer = 0f;          // counts down once debt > 0
        const float DebtGracePeriod = 300f;      // seconds before an unpaid debt triggers arrest
        public static bool inPrison = false;
        static float prisonSentenceTimer = 0f;
        const float PrisonSecondsPerDollar = 2f; // sentence length scales with debt owed
        static Vector2 prisonReturnPos;
        public static Vector2 prisonCellCenter = new Vector2(1000, 1200);
        static string currentPrisonRoom = "";   // "" = main block, else "LAUNDRY"/"KITCHEN"/"TOILETS"
        static (string name, Vector2 doorPos, Color col)[] prisonRooms = {
            ("LAUNDRY", new Vector2(360, 1760),  new Color(80, 120, 160, 255)),
            ("KITCHEN", new Vector2(1000, 1760), new Color(170, 110, 50, 255)),
            ("TOILETS", new Vector2(1640, 1760), new Color(90, 150, 150, 255)),
        };
        static Dictionary<string, List<Rectangle>> prisonRoomInteriorObjects = new() {
            { "LAUNDRY", new List<Rectangle>{ new(200,200,300,180), new(600,200,300,180), new(1000,200,300,180), new(300,600,800,80) } },
            { "KITCHEN", new List<Rectangle>{ new(150,150,1100,80), new(200,400,250,200), new(600,400,250,200), new(1000,400,250,200) } },
            { "TOILETS", new List<Rectangle>{ new(150,200,180,220), new(420,200,180,220), new(690,200,180,220), new(960,200,180,220) } },
        };

        static void IssueFine(float amount, string reason)
        {
            playerDebt += amount;
            if (debtDueTimer <= 0f) debtDueTimer = DebtGracePeriod;
            ShowNotification($"Fined ${amount:0} for {reason}. Pay it off before time runs out!");
        }

        // BUS SYSTEM
        record BusStop(string Name, Vector2 WorldPos);
        static BusStop[] busStops = {
            new("Safe Zone",  new Vector2(440,  440)),
            new("Supermarket",new Vector2(3790, 440)),
            new("Desert",     new Vector2(8000, 440)),
            new("Snow Zone",  new Vector2(-9000, 440)),
            new("Hamiltron City", new Vector2(14800, 5660)),
            new("Rotoaira",       new Vector2(-16200, 4540)),
        };
        static int  busCurrentStop  = 0;    
        static int  busNextStop     = 1;
        static float busRouteTimer  = 0f;           // counts up to 4 in-game hours between stops
        static Vector2 busPosition  = new Vector2(300, 580);
        static bool busMoving       = false;
        static float busLerpT       = 0f;
        static bool busOperating    = false;        // true only 6am–12pm
        static bool busMenuOpen = false;

        // CASINO
        static int playerChips = 0;
        static bool casinoChipMenuOpen = false;
        // Blackjack
        public static bool blackjackOpen = false;
        static List<int> bjPlayerHand = new();
        static List<int> bjDealerHand = new();
        static int bjBet = 10;
        static string bjMessage = "";
        static float bjMessageTimer = 0f;
        static bool bjDealerRevealed = false;
        static bool bjRoundOver = false;
        // Roulette
        public static bool rouletteOpen = false;
        static int rouletteBet = 10;
        static string rouletteBetType = "Red";  // "Red","Black","Even","Odd","0-12","13-24","25-36"
        static int rouletteResult = -1;
        static string rouletteMessage = "";
        static float rouletteMessageTimer = 0f;
        static float rouletteSpinTimer = 0f;
        static bool rouletteSpinning = false;
        static int BjCardValue(int card)
        {
            int v = card % 13;
            if (v == 0) return 11; // Ace
            if (v >= 10) return 10; // face cards
            return v + 1;
        }
        static int BjHandValue(List<int> hand)
        {
            int total = 0, aces = 0;
            foreach (int c in hand) { int v = BjCardValue(c); if (v == 11) aces++; total += v; }
            while (total > 21 && aces > 0) { total -= 10; aces--; }
            return total;
        }
        static string BjCardName(int card)
        {
            string[] ranks = { "A","2","3","4","5","6","7","8","9","10","J","Q","K" };
            string[] suits = { "♠","♥","♦","♣" };
            return ranks[card % 13] + suits[card / 13];
        }
        static void BjStartRound()
        {
            if (playerChips < bjBet) { bjMessage = "Not enough chips!"; bjMessageTimer = 2f; return; }
            playerChips -= bjBet;
            bjPlayerHand = new List<int>{ Raylib.GetRandomValue(0,51), Raylib.GetRandomValue(0,51) };
            bjDealerHand = new List<int>{ Raylib.GetRandomValue(0,51), Raylib.GetRandomValue(0,51) };
            bjDealerRevealed = false;
            bjRoundOver = false;
            bjMessage = "";
            // instant blackjack check
            if (BjHandValue(bjPlayerHand) == 21)
            { bjDealerRevealed = true; bjRoundOver = true; playerChips += (int)(bjBet * 2.5f); bjMessage = "BLACKJACK! You win 2.5x!"; bjMessageTimer = 3f; }
        }
        static void BjHit()
        {
            if (bjRoundOver) return;
            bjPlayerHand.Add(Raylib.GetRandomValue(0, 51));
            if (BjHandValue(bjPlayerHand) > 21)
            { bjDealerRevealed = true; bjRoundOver = true; bjMessage = "Bust! Dealer wins."; bjMessageTimer = 2f; }
        }
        static void BjStand()
        {
            if (bjRoundOver) return;
            bjDealerRevealed = true;
            while (BjHandValue(bjDealerHand) < 17)
                bjDealerHand.Add(Raylib.GetRandomValue(0, 51));
            int p = BjHandValue(bjPlayerHand), d = BjHandValue(bjDealerHand);
            bjRoundOver = true;
            if (d > 21 || p > d)       { playerChips += bjBet * 2; bjMessage = $"You win! {p} vs {d}"; }
            else if (p == d)            { playerChips += bjBet;     bjMessage = $"Push! {p} vs {d}"; }
            else                        {                           bjMessage = $"Dealer wins. {p} vs {d}"; }
            bjMessageTimer = 3f;
        }

        //fish species: name, value, rarity weight (higher = more common), water type, tool

        static List<FishSpecies> fishTypes = new()
        {
            // ── LAKE fish ──
            new FishSpecies("Carp",      4,  40, "Lake", "Any"),
            new FishSpecies("Perch",     6,  30, "Lake", "Any"),
            new FishSpecies("Bass",      10, 18, "Lake", "Rod"),    // rod only
            new FishSpecies("Catfish",   15, 8,  "Lake", "Net"),    // net only
            new FishSpecies("Golden Carp",40, 4, "Lake", "Rod"),    // rare rod

            // ── RIVER fish ──
            new FishSpecies("Trout",     8,  35, "River", "Rod"),   // rod only
            new FishSpecies("Salmon",    14, 22, "River", "Rod"),   // rod only
            new FishSpecies("Eel",       12, 20, "River", "Net"),   // net only
            new FishSpecies("Crayfish",  9,  18, "River", "Net"),   // net only
            new FishSpecies("Sturgeon",  50, 3,  "River", "Rod"),   // rare rod
        };

       static (bool exists, string name, string info) GetSlotInfo(int slot)
{
    string path = savePaths[slot];
    if (!System.IO.File.Exists(path)) return (false, "", "");

    var map = new Dictionary<string, string>();
    foreach (var line in System.IO.File.ReadAllLines(path))
    {
        int eq = line.IndexOf('=');
        if (eq > 0) map[line.Substring(0, eq)] = line.Substring(eq + 1);
    }
    if (map.Count == 0) return (false, "", "");

    string GS(string k, string def = "") => map.TryGetValue(k, out var v) ? v : def;
    int GI(string k) => map.TryGetValue(k, out var v) && int.TryParse(v, out var r) ? r : 0;
    float GF(string k) => map.TryGetValue(k, out var v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0f;

    string name = GS("name", "Player");
    int wcLv = GI("woodcutLv");
    int fishLv = GI("fishLv");
    int combatLv = GI("combatLv");
    float playTime = GF("playtime");
    int hours = (int)(playTime / 3600);
    int minutes = (int)((playTime % 3600) / 60);

    return (true, name, $"WC:{wcLv} Fish:{fishLv} Combat:{combatLv} | {hours}h {minutes}m");
}

static Color lastSentSkin, lastSentHair, lastSentFacial, lastSentShirt, lastSentPants;
static string lastSentHelmet="", lastSentBody="", lastSentLegs="", lastSentBoots="",
              lastSentGloves="", lastSentCape="", lastSentShield="", lastSentHeld="";
static string lastSentHairStyle = "";
static string lastSentFacial2 = "None";
static string lastSentWeapon = "";
static bool   lastSentTwoHanded = false;
static int lastKnownRemoteCount = 0;

static void DrawGroceryShopPanel()
{
    if (!groceryShopOpen) return;
    if (currentBuilding?.BuildingName != "SUPERMARKET") return;

    bool hasBag = player.HasTrolley || player.HasBasket;
    if (!hasBag)
    {
        int iw = Program.MeasureTextUI("Grab a trolley or basket first bro!", 22);
        Raylib.DrawRectangle(ScreenWidth/2 - iw/2 - 12, 320, iw + 24, 40, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Program.DrawTextUI("Grab a trolley or basket first bro!", ScreenWidth/2 - iw/2, 328, 22, Color.Orange);
        return;
    }

    int capacity = player.HasTrolley ? 20 : 10;
    var cartInv   = player.HasTrolley ? trolleyInventory : basketInventory;
    int used = cartInv.Count(s => s != null);

    int px = 20, py = 60, pw = 500, ph = 580;
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)10,(byte)25,(byte)10,(byte)245));
    Raylib.DrawRectangleLines(px, py, pw, ph, new Color((byte)60,(byte)130,(byte)60,(byte)255));
    Program.DrawTextUI("GROCERY SHOP", px + 130, py + 8, 24, new Color((byte)80,(byte)200,(byte)80,(byte)255));
    Program.DrawTextUI($"Cart: {used}/{capacity}  |  Total: ${GetTotalCartCost()}", px + 10, py + 40, 16, Color.LightGray);

    // aisle filter tabs
    string[] aisleNames = { "All", "Aisle 1", "Aisle 2", "Aisle 3" };
    for (int t = 0; t < 4; t++)
    {
        Rectangle tab = new Rectangle(px + 8 + t * 122, py + 62, 116, 26);
        bool active = groceryAisleFilter == t;
        Raylib.DrawRectangleRec(tab, active ? new Color((byte)40,(byte)100,(byte)40,(byte)255) : new Color((byte)20,(byte)40,(byte)20,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 1, active ? new Color((byte)80,(byte)200,(byte)80,(byte)255) : Color.DarkGray);
        Program.DrawTextUI(aisleNames[t], (int)tab.X + 8, (int)tab.Y + 6, 14, active ? Color.White : Color.DarkGray);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), tab) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            groceryAisleFilter = t;
    }

    var filtered = groceryAisleFilter == 0
        ? groceryItems
        : groceryItems.Where(g => g.Aisle == groceryAisleFilter).ToArray();

    Vector2 mouse = Raylib.GetMousePosition();
    int rowH = 38, listY = py + 96;

    for (int i = 0; i < filtered.Length && i < 12; i++)
    {
        var g = filtered[i];
        int inCart = cartInv.Count(s => s == g.Name);
        Rectangle row = new Rectangle(px + 8, listY + i * rowH, pw - 16, rowH - 4);
        bool hover = Raylib.CheckCollisionPointRec(mouse, row);
        Raylib.DrawRectangleRec(row, new Color((byte)15,(byte)35,(byte)15,(byte)255));
        Raylib.DrawRectangleLinesEx(row, 1, hover ? new Color((byte)80,(byte)200,(byte)80,(byte)255) : new Color((byte)30,(byte)60,(byte)30,(byte)255));
        Program.DrawTextUI(g.Name, px + 18, listY + i * rowH + 10, 18, Color.White);
        Program.DrawTextUI($"${g.Price}", px + 280, listY + i * rowH + 10, 18, Color.Gold);

        // qty in cart
        if (inCart > 0)
            Program.DrawTextUI($"x{inCart}", px + 340, listY + i * rowH + 10, 16, new Color((byte)80,(byte)220,(byte)80,(byte)255));

        // + / - buttons
        Rectangle addBtn = new Rectangle(px + pw - 80, listY + i * rowH + 4, 32, 28);
        Rectangle remBtn = new Rectangle(px + pw - 40, listY + i * rowH + 4, 32, 28);
        bool hAdd = Raylib.CheckCollisionPointRec(mouse, addBtn);
        bool hRem = Raylib.CheckCollisionPointRec(mouse, remBtn) && inCart > 0;

        Raylib.DrawRectangleRec(addBtn, new Color((byte)30,(byte)80,(byte)30,(byte)255));
        Raylib.DrawRectangleRec(remBtn, new Color((byte)80,(byte)30,(byte)30,(byte)255));
        Program.DrawTextUI("+", px + pw - 72, listY + i * rowH + 6, 20, hAdd ? Color.White : Color.LightGray);
        Program.DrawTextUI("-", px + pw - 32, listY + i * rowH + 6, 20, hRem ? Color.White : Color.DarkGray);

        if (hAdd && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (used < capacity) AddToCart(g.Name);
            else { shopMessage = "Cart is full bro!"; shopMessageTimer = 1.5f; }
        }
        if (hRem && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            int idx = Array.LastIndexOf(cartInv.ToArray(), g.Name);
            // find last matching slot
            for (int s = cartInv.Count - 1; s >= 0; s--)
                if (cartInv[s] == g.Name) { cartInv[s] = null; break; }
        }
    }

    Program.DrawTextUI("Q = Close  |  Go to checkout to pay", px + 20, py + ph - 22, 14, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) groceryShopOpen = false;
}

static float bossSyncTimer = 0f;
static float superBossSyncTimer = 0f;
static float enemySyncTimer = 0f;
static float clockSyncTimer = 0f;

static int tutorialWoodMark, tutorialMineMark, tutorialFishMark, tutorialCombatMark, tutorialNetFishMark, tutorialCookMark, tutorialRangedMark, tutorialMagicMark,
tutorialCraftMark, tutorialFarmMark, tutorialRideMark;

static bool IsTwoHandedMeleeWeapon(string w) =>
    w != null && (w.Contains("Great Sword") || w.Contains("War Axe"));

static bool IsOneHandedWeapon(string w) =>
    w != null && !IsTwoHandedMeleeWeapon(w) && (w.Contains("Sword") || w.Contains("Stick"));

static bool IsRangedWeapon(string w) =>
    w != null && (w.Contains("Bow") || w.Contains("Crossbow"));

// weapons the player owns, split by hand

static void RemoveTreesRocksNearBuildings(float buffer = 150f)
{
    trees.RemoveAll(t => IsNearBuilding(t.Position, buffer));
    rocks.RemoveAll(r => IsNearBuilding(r.Position, buffer));
}

// DrawSchoolHallInterior() — FULL REWRITE of the room layout to match your sketch. Replace the entire method body.
static void DrawSchoolHallInterior()
{
    Color floor1 = new Color((byte)225,(byte)215,(byte)195,(byte)255);
    Color floor2 = new Color((byte)210,(byte)200,(byte)180,(byte)255);
    Color wallCol = new Color((byte)70,(byte)55,(byte)40,(byte)255);
    Color doorGap = new Color((byte)225,(byte)215,(byte)195,(byte)255); // matches floor, "erases" wall for the door

    for (int fx = 0; fx < 2000; fx += 100)
        for (int fy = 0; fy < 2000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);

    // outer boundary
    Raylib.DrawRectangle(0, 0, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 1980, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 2000, wallCol);
    Raylib.DrawRectangle(1980, 0, 20, 2000, wallCol);

    // entrance gap, top wall centre
    Raylib.DrawRectangle(900, 0, 160, 20, doorGap);
    Program.DrawTextUI("ENTRANCE", 900, 30, 16, wallCol);

    // helper: walled room box with a door gap on one side + label
    void Room(int rx, int ry, int rw, int rh, string label, Color fillCol, int doorSide, int doorOffset, int doorW)
    {
        Raylib.DrawRectangle(rx, ry, rw, rh, fillCol);
        Raylib.DrawRectangle(rx, ry, rw, 12, wallCol);              // top
        Raylib.DrawRectangle(rx, ry + rh - 12, rw, 12, wallCol);    // bottom
        Raylib.DrawRectangle(rx, ry, 12, rh, wallCol);              // left
        Raylib.DrawRectangle(rx + rw - 12, ry, 12, rh, wallCol);    // right

        // doorSide: 0=top,1=bottom,2=left,3=right
        if (doorSide == 0) Raylib.DrawRectangle(rx + doorOffset, ry, doorW, 12, doorGap);
        if (doorSide == 1) Raylib.DrawRectangle(rx + doorOffset, ry + rh - 12, doorW, 12, doorGap);
        if (doorSide == 2) Raylib.DrawRectangle(rx, ry + doorOffset, 12, doorW, doorGap);
        if (doorSide == 3) Raylib.DrawRectangle(rx + rw - 12, ry + doorOffset, 12, doorW, doorGap);

        int tw = Program.MeasureTextUI(label, 22);
        Program.DrawTextUI(label, rx + rw/2 - tw/2, ry + rh/2 - 11, 22, wallCol);
    }

// DrawSchoolHallInterior() — CHANGED: downsized rooms with walking corridors, cafeteria/bathrooms flush to boundary walls, adjacent room walls touching
// ── ROW 1: classrooms — Maths, Art, Science ──
Room(150, 300, 350, 400, "MATHS",   new Color((byte)230,(byte)225,(byte)210,(byte)255), 1, 90, 60);
Room(500, 300, 350, 400, "ART",     new Color((byte)230,(byte)225,(byte)210,(byte)255), 1, 90, 60);
Room(850, 300, 350, 400, "SCIENCE", new Color((byte)230,(byte)225,(byte)210,(byte)255), 1, 90, 60);

// ── OFFICE (open desk area, matches the earlier detailed front-desk look) ──
Raylib.DrawRectangle(1500, 60, 400, 180, new Color((byte)200,(byte)190,(byte)170,(byte)255));
Raylib.DrawRectangleLines(1500, 60, 400, 180, new Color((byte)120,(byte)90,(byte)50,(byte)255));
Raylib.DrawRectangle(1520, 100, 300, 60, new Color((byte)120,(byte)90,(byte)50,(byte)255));
Raylib.DrawRectangle(1540, 80, 60, 20, new Color((byte)80,(byte)60,(byte)30,(byte)255));
Raylib.DrawRectangle(1535, 40, 70, 45, new Color((byte)30,(byte)30,(byte)40,(byte)255));
Raylib.DrawRectangle(1540, 45, 60, 35, new Color((byte)90,(byte)160,(byte)220,(byte)255));
Program.DrawTextUI("OFFICE / RECEPTION", 1560, 160, 18, new Color((byte)90,(byte)60,(byte)30,(byte)255));

// ── TROPHIES (open cabinet, no walls) ──
Raylib.DrawRectangle(1800, 400, 170, 300, new Color((byte)180,(byte)210,(byte)230,(byte)150));
Raylib.DrawRectangleLines(1800, 400, 170, 300, new Color((byte)90,(byte)60,(byte)30,(byte)255));
for (int shelf = 0; shelf < 4; shelf++)
{
    Raylib.DrawRectangle(1810, 430 + shelf * 60, 150, 6, new Color((byte)140,(byte)100,(byte)50,(byte)255));
    Raylib.DrawCircle(1850 + shelf * 30, 460 + shelf * 60, 10, new Color((byte)230,(byte)190,(byte)60,(byte)255));
}
Program.DrawTextUI("TROPHIES", 1850, 405, 16, new Color((byte)90,(byte)60,(byte)30,(byte)255));

// ── ROW 2: Sports, English — walls touch Row 1's bottom wall (no gap) ──
Room(150, 900, 350, 400, "SPORTS",  new Color((byte)210,(byte)230,(byte)205,(byte)255), 0, 90, 60);
Room(500, 900, 350, 400, "ENGLISH", new Color((byte)210,(byte)230,(byte)205,(byte)255), 0, 90, 60);

// ── ROW 3: Boys, Girls bathrooms — walls touch Row 2's bottom wall, left wall flush to boundary ──
Room(150, 1650, 350, 350, "BOYS",  new Color((byte)150,(byte)190,(byte)220,(byte)255), 0, 90, 60);
Room(500, 1650, 350, 350, "GIRLS", new Color((byte)230,(byte)170,(byte)200,(byte)255), 0, 90, 60);
Room(850, 1650, 350, 350, "GYM", new Color((byte)210,(byte)230,(byte)205,(byte)255), 0, 90, 60);

Raylib.DrawRectangle(1650, 900, 330, 300, new Color((byte)205,(byte)205,(byte)215,(byte)255));
Raylib.DrawRectangleLines(1650, 900, 330, 300, new Color((byte)120,(byte)120,(byte)130,(byte)255));
Program.DrawTextUI("SERVERY", 1750, 920, 20, new Color((byte)80,(byte)80,(byte)90,(byte)255));
for (int t = 0; t < 3; t++)
{
    Raylib.DrawRectangle(1670 + t * 105, 970, 90, 70, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    Raylib.DrawRectangle(1675 + t * 105, 975, 80, 60, new Color((byte)220,(byte)190,(byte)130,(byte)200));
}

Room(1200, 1200, 800, 800, "CAFETERIA WITH SEATS", new Color((byte)235,(byte)225,(byte)195,(byte)255), 0, 300, 80);

// DrawSchoolHallInterior() — CHANGED: cafeteria table grid repositioned to match new bounds (700-1980, 700-1980)
for (int row = 0; row < 4; row++)
    for (int col = 0; col < 3; col++)
    {
        int tx = 1350 + col * 200;
        int ty = 1320 + row * 200;
        Raylib.DrawRectangle(tx, ty, 100, 50, new Color((byte)175,(byte)135,(byte)85,(byte)255));
        Raylib.DrawRectangleLines(tx, ty, 100, 50, new Color((byte)110,(byte)80,(byte)45,(byte)255));
        Raylib.DrawRectangle(tx + 6, ty - 12, 22, 12, new Color((byte)90,(byte)60,(byte)40,(byte)255));
        Raylib.DrawRectangle(tx + 72, ty - 12, 22, 12, new Color((byte)90,(byte)60,(byte)40,(byte)255));
        Raylib.DrawRectangle(tx + 6, ty + 50, 22, 12, new Color((byte)90,(byte)60,(byte)40,(byte)255));
        Raylib.DrawRectangle(tx + 72, ty + 50, 22, 12, new Color((byte)90,(byte)60,(byte)40,(byte)255));
    }

    // ── LOCKERS along left wall ──
    for (int ly = 60; ly < 1900; ly += 100)
    {
        Raylib.DrawRectangle(30, ly, 90, 80, new Color((byte)150,(byte)160,(byte)175,(byte)255));
        Raylib.DrawRectangleLines(30, ly, 90, 80, new Color((byte)90,(byte)100,(byte)115,(byte)255));
        Raylib.DrawRectangle(110, ly + 10, 5, 60, new Color((byte)60,(byte)70,(byte)85,(byte)255));
    }
    Program.DrawTextUI("LOCKERS", 30, 30, 16, new Color((byte)90,(byte)100,(byte)115,(byte)255));

    (int x, int y)[] plants = { (150,900),(400,900),(150,1200),(400,1200) };
// ... existing code (plant drawing loop, unchanged) ...

Raylib.DrawRectangle(200, 1350, 250, 40, new Color((byte)190,(byte)180,(byte)160,(byte)255));
Raylib.DrawRectangleLines(200, 1350, 250, 40, wallCol);
Raylib.DrawRectangle(500, 1350, 250, 40, new Color((byte)190,(byte)180,(byte)160,(byte)255));
Raylib.DrawRectangleLines(500, 1350, 250, 40, wallCol);

int shownS = 0;
    foreach (var npc in npcs)
    {
        if (!npc.DrawInsideNow || npc.HomeBuilding != "SCHOOL") continue;
        int cx = 760 + (shownS % 4) * 220 + (int)(MathF.Sin((float)Raylib.GetTime() + shownS) * 20);
        int cy = 760 + (shownS / 4) * 180;
        Raylib.DrawCircle(cx + 20, cy + 12, 10, Color.Beige);
        Raylib.DrawRectangle(cx + 12, cy + 22, 16, 22, npc.IsParent ? Color.Maroon : Color.SkyBlue);
        int tw = Program.MeasureTextUI(npc.Name, 12);
        Program.DrawTextUI(npc.Name, cx + 20 - tw/2, cy - 12, 12, wallCol);
        shownS++;
    }

}
static void DrawClassroomInterior(string subject)
{
    var s = schoolSubjects.First(x => x.Name == subject);
    Raylib.ClearBackground(new Color((byte)30,(byte)25,(byte)20,(byte)255));
    Raylib.DrawRectangle(0, 0, 1400, 1000, new Color((byte)228,(byte)218,(byte)198,(byte)255));

    for (int fx = 0; fx < 1400; fx += 100)
        for (int fy = 300; fy < 1000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0)
                ? new Color((byte)238,(byte)228,(byte)208,(byte)255)
                : new Color((byte)218,(byte)208,(byte)188,(byte)255));

    // walls
    Raylib.DrawRectangle(0, 0, 1400, 20, new Color((byte)240,(byte)232,(byte)212,(byte)255));
    Raylib.DrawRectangle(0, 0, 20, 1000, new Color((byte)240,(byte)232,(byte)212,(byte)255));
    Raylib.DrawRectangle(1380, 0, 20, 1000, new Color((byte)240,(byte)232,(byte)212,(byte)255));

    // front board
    Raylib.DrawRectangle(500, 60, 400, 160, new Color((byte)20,(byte)60,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(500, 60, 400, 160, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Program.DrawTextUI(subject.ToUpper(), 620, 120, 30, s.Col);
    Raylib.DrawRectangle(500, 224, 400, 10, new Color((byte)200,(byte)170,(byte)90,(byte)255)); // chalk tray

    // window strip
    for (int wx = 40; wx < 480; wx += 90)
    {
        Raylib.DrawRectangle(wx, 30, 70, 50, new Color((byte)150,(byte)200,(byte)230,(byte)200));
        Raylib.DrawRectangleLines(wx, 30, 70, 50, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangle(wx + 33, 30, 4, 50, new Color((byte)90,(byte)60,(byte)30,(byte)200));
    }

    // bookshelf, right wall
    Raylib.DrawRectangle(1250, 60, 110, 220, new Color((byte)110,(byte)75,(byte)40,(byte)255));
    Raylib.DrawRectangleLines(1250, 60, 110, 220, Color.Black);
    for (int shelf = 0; shelf < 4; shelf++)
    {
        Raylib.DrawRectangle(1255, 75 + shelf * 50, 100, 6, new Color((byte)70,(byte)45,(byte)20,(byte)255));
        for (int b = 0; b < 6; b++)
            Raylib.DrawRectangle(1258 + b * 16, 82 + shelf * 50 - 30, 12, 30,
                new Color((byte)(140 + b*15 % 90), (byte)(60 + b*10 % 60), (byte)(40 + b*8 % 40), (byte)255));
    }

    // subject poster + coat hooks, left wall
    Raylib.DrawRectangle(40, 260, 90, 120, new Color((byte)250,(byte)245,(byte)230,(byte)255));
    Raylib.DrawRectangleLines(40, 260, 90, 120, s.Col);
    Program.DrawTextUI(subject.Substring(0,1), 70, 305, 40, s.Col);
    for (int h = 0; h < 4; h++)
    {
        Raylib.DrawRectangle(40, 420 + h * 40, 60, 6, new Color((byte)90,(byte)60,(byte)30,(byte)255));
        Raylib.DrawCircle(50 + h * 14, 424 + h * 40, 3, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    }

    // teacher's desk near the board
    Raylib.DrawRectangle(640, 250, 130, 60, new Color((byte)130,(byte)95,(byte)55,(byte)255));
    Raylib.DrawRectangleLines(640, 250, 130, 60, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawRectangle(660, 235, 40, 22, new Color((byte)230,(byte)230,(byte)235,(byte)255)); // papers

    // desk rows — 3 rows x 4 columns, filling the room width
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 4; col++)
        {
            int dx = 260 + col * 220;
            int dy = 400 + row * 130;
            Raylib.DrawRectangle(dx, dy, 90, 55, new Color((byte)165,(byte)125,(byte)80,(byte)255));
            Raylib.DrawRectangleLines(dx, dy, 90, 55, new Color((byte)110,(byte)80,(byte)45,(byte)255));
            Raylib.DrawRectangle(dx + 25, dy - 20, 30, 20, new Color((byte)70,(byte)90,(byte)70,(byte)255)); // chair back
        }
    }

    // exit door
    Raylib.DrawRectangle(650, 950, 100, 40, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    Raylib.DrawRectangleLines(650, 950, 100, 40, Color.Black);
    Raylib.DrawCircle(730, 970, 3, new Color((byte)200,(byte)180,(byte)60,(byte)255));
    if (Vector2.Distance(player.Center, new Vector2(700, 950)) < 70)
        Program.DrawTextUI("Q = Leave classroom", 620, 900, 18, Color.LightGray);

    // lesson prompt
    if (Vector2.Distance(player.Center, new Vector2(700, 280)) < 200)
    {
        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI($"{subject} Lesson — ${s.Cost}, +{s.XP} Education XP", 20, 630, 24, s.Col);
        Program.DrawTextUI("SPACE = Attend lesson", 20, 668, 22, Color.White);
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen && player.Money >= s.Cost)
        {
            player.Money -= s.Cost;
            if (subject == "Maths")
                StartClassTest("Maths");
            else
            {
                player.AddEducationXP(s.XP);
                ShowNotification($"Attended {subject} class! +{s.XP} Education XP");
            }
        }
    }
}

static void DrawPrisonInterior()
{
    Color floor1 = new Color((byte)150,(byte)150,(byte)158,(byte)255);
    Color floor2 = new Color((byte)136,(byte)136,(byte)145,(byte)255);
    Color wallCol = new Color((byte)70,(byte)70,(byte)78,(byte)255);
    Color cellCol = new Color((byte)90,(byte)90,(byte)98,(byte)255);
    Color barCol  = new Color((byte)18,(byte)18,(byte)22,(byte)255);
    Color deskCol = new Color((byte)95,(byte)72,(byte)42,(byte)255);

    // concrete floor
    for (int fx = 0; fx < 2000; fx += 100)
        for (int fy = 0; fy < 2000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);

    // outer walls
    Raylib.DrawRectangle(0, 0, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 1980, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 2000, wallCol);
    Raylib.DrawRectangle(1980, 0, 20, 2000, wallCol);

    // guard desk (matches collision 1400,300,1400,80 — kept)
    Raylib.DrawRectangle(1400, 300, 1400, 80, deskCol);
    Raylib.DrawRectangleLines(1400, 300, 1400, 80, Color.Black);
    Raylib.DrawRectangle(1440, 270, 70, 30, new Color((byte)210,(byte)210,(byte)220,(byte)255)); // monitor
    Program.DrawTextUI("GUARD DESK", 1470, 322, 20, Color.White);

    // ── HOLDING CELLS — a row of individual barred cells with bunk + toilet ──
    (int x, int y, int w, int h)[] cells = { (200,1000,400,400), (800,1000,400,400), (1400,1000,400,400) };
    int cellNo = 1;
    foreach (var (cx, cy, cw, ch) in cells)
    {
        Raylib.DrawRectangle(cx, cy, cw, ch, cellCol);
        Raylib.DrawRectangleLines(cx, cy, cw, ch, wallCol);

        // bunk bed (left)
        Raylib.DrawRectangle(cx + 20, cy + 30, 90, 150, new Color((byte)70,(byte)55,(byte)35,(byte)255));
        Raylib.DrawRectangle(cx + 24, cy + 34, 82, 40, new Color((byte)150,(byte)150,(byte)160,(byte)255)); // top mattress
        Raylib.DrawRectangle(cx + 24, cy + 100, 82, 40, new Color((byte)150,(byte)150,(byte)160,(byte)255)); // bottom mattress
        // steel toilet (back-right)
        Raylib.DrawRectangle(cx + cw - 70, cy + 40, 44, 40, new Color((byte)190,(byte)195,(byte)200,(byte)255));
        Raylib.DrawRectangleLines(cx + cw - 70, cy + 40, 44, 40, new Color((byte)120,(byte)120,(byte)130,(byte)255));
        // small barred window
        Raylib.DrawRectangle(cx + cw/2 - 20, cy + 20, 40, 30, new Color((byte)90,(byte)130,(byte)160,(byte)200));
        for (int wbar = cx + cw/2 - 20; wbar <= cx + cw/2 + 20; wbar += 10)
            Raylib.DrawRectangle(wbar, cy + 20, 3, 30, barCol);

        // front bars with a door gap
        for (int b = cx; b <= cx + cw; b += 30)
            if (b < cx + cw/2 - 40 || b > cx + cw/2 + 40)
                Raylib.DrawRectangle(b, cy + ch - 12, 4, 12, barCol);
        Raylib.DrawRectangle(cx, cy + ch - 12, cw, 4, barCol); // sill

        Program.DrawTextUI($"CELL {cellNo}", cx + cw/2 - 34, cy - 26, 20, Color.LightGray);
        cellNo++;
    }

    // back walkway (kept collision 200,1500,1600,60)
    Raylib.DrawRectangle(200, 1500, 1600, 60, new Color((byte)60,(byte)60,(byte)66,(byte)255));

    // ── DOORS to facility sub-rooms along the bottom wall ──
    foreach (var (name, doorPos, col) in prisonRooms)
    {
        int dx = (int)doorPos.X, dy = (int)doorPos.Y;
        Raylib.DrawRectangle(dx - 60, dy - 70, 120, 70, col);
        Raylib.DrawRectangle(dx - 50, dy - 60, 100, 50, new Color((byte)40,(byte)40,(byte)46,(byte)255)); // doorway
        Program.DrawTextUI(name, dx - 52, dy - 96, 18, col);
        if (Vector2.Distance(player.Center, doorPos) < 90)
            Program.DrawTextUI("E = Enter " + name, dx - 60, dy + 8, 18, Color.Gold);
    }
}
static void DrawPrisonRoomInterior(string room)
{
    var meta = prisonRooms.First(r => r.name == room);
    Color col = meta.col;
    Color floor1 = new Color((byte)160,(byte)162,(byte)168,(byte)255);
    Color floor2 = new Color((byte)146,(byte)148,(byte)156,(byte)255);
    Color wallCol = new Color((byte)70,(byte)70,(byte)78,(byte)255);

    Raylib.ClearBackground(new Color((byte)28,(byte)28,(byte)34,(byte)255));
    for (int fx = 0; fx < 1400; fx += 100)
        for (int fy = 0; fy < 1000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);

    Raylib.DrawRectangle(0, 0, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 980, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 1000, wallCol);
    Raylib.DrawRectangle(1380, 0, 20, 1000, wallCol);
    Raylib.DrawRectangle(0, 0, 1400, 64, col);
    Program.DrawTextUI(room, 40, 18, 34, Color.White);

    if (room == "LAUNDRY")
    {
        // washing machines (match collision rows at y=200)
        foreach (int mx in new[] { 200, 600, 1000 })
            for (int m = 0; m < 3; m++)
            {
                int wx = mx + m * 100;
                Raylib.DrawRectangle(wx, 200, 90, 180, new Color((byte)200,(byte)205,(byte)215,(byte)255));
                Raylib.DrawRectangleLines(wx, 200, 90, 180, new Color((byte)120,(byte)125,(byte)135,(byte)255));
                Raylib.DrawCircle(wx + 45, 280, 30, new Color((byte)90,(byte)130,(byte)160,(byte)200)); // door
                Raylib.DrawCircleLines(wx + 45, 280, 30, new Color((byte)70,(byte)75,(byte)85,(byte)255));
            }
        // folding table (match collision 300,600,800,80)
        Raylib.DrawRectangle(300, 600, 800, 80, new Color((byte)120,(byte)90,(byte)55,(byte)255));
        for (int p = 340; p < 1080; p += 90)
            Raylib.DrawRectangle(p, 610, 60, 40, new Color((byte)230,(byte)230,(byte)235,(byte)255)); // folded linens
    }
    else if (room == "KITCHEN")
    {
        // serving counter (match 150,150,1100,80)
        Raylib.DrawRectangle(150, 150, 1100, 80, new Color((byte)180,(byte)185,(byte)195,(byte)255));
        Raylib.DrawRectangleLines(150, 150, 1100, 80, new Color((byte)110,(byte)115,(byte)125,(byte)255));
        Program.DrawTextUI("SERVERY", 620, 168, 22, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        // cooking stations (match 200/600/1000, 400, 250x200)
        foreach (int sx in new[] { 200, 600, 1000 })
        {
            Raylib.DrawRectangle(sx, 400, 250, 200, new Color((byte)150,(byte)150,(byte)160,(byte)255));
            Raylib.DrawRectangleLines(sx, 400, 250, 200, new Color((byte)90,(byte)90,(byte)100,(byte)255));
            // stove burners
            Raylib.DrawCircle(sx + 70, 470, 26, new Color((byte)40,(byte)40,(byte)44,(byte)255));
            Raylib.DrawCircle(sx + 180, 470, 26, new Color((byte)40,(byte)40,(byte)44,(byte)255));
            Raylib.DrawCircle(sx + 70, 470, 14, new Color((byte)200,(byte)80,(byte)40,(byte)220)); // hot ring
            // big pot
            Raylib.DrawRectangle(sx + 100, 520, 60, 50, new Color((byte)70,(byte)70,(byte)78,(byte)255));
        }
        // hanging utensils on right wall
        for (int u = 0; u < 4; u++)
            Raylib.DrawRectangle(1300, 300 + u * 60, 40, 8, new Color((byte)180,(byte)180,(byte)190,(byte)255));
    }
    else // TOILETS
    {
        // stall partitions (match collisions at y=200, widths 180)
        foreach (int sx in new[] { 150, 420, 690, 960 })
        {
            Raylib.DrawRectangle(sx, 200, 180, 220, new Color((byte)205,(byte)210,(byte)218,(byte)255));
            Raylib.DrawRectangleLines(sx, 200, 180, 220, new Color((byte)120,(byte)125,(byte)135,(byte)255));
            // toilet bowl
            Raylib.DrawRectangle(sx + 60, 250, 60, 70, new Color((byte)235,(byte)238,(byte)242,(byte)255));
            Raylib.DrawCircle(sx + 90, 250, 30, new Color((byte)235,(byte)238,(byte)242,(byte)255));
            Raylib.DrawCircleLines(sx + 90, 250, 30, new Color((byte)150,(byte)155,(byte)165,(byte)255));
        }
        // sink row along the bottom
        for (int s = 0; s < 5; s++)
        {
            int sx = 200 + s * 220;
            Raylib.DrawRectangle(sx, 700, 120, 60, new Color((byte)210,(byte)214,(byte)222,(byte)255));
            Raylib.DrawCircle(sx + 60, 730, 14, new Color((byte)150,(byte)155,(byte)165,(byte)255));
        }
        // long mirror above sinks
        Raylib.DrawRectangle(180, 640, 1120, 40, new Color((byte)150,(byte)190,(byte)210,(byte)180));
    }

    // exit door
    Raylib.DrawRectangle(650, 950, 100, 40, new Color((byte)55,(byte)55,(byte)62,(byte)255));
    Raylib.DrawRectangleLines(650, 950, 100, 40, Color.Black);
    if (Vector2.Distance(player.Center, new Vector2(700, 950)) < 70)
        Program.DrawTextUI("Q = Back to cell block", 590, 900, 18, Color.LightGray);
}

static void DrawHobbiesStoreInterior()
{
    // floor
    for (int fx = 0; fx < 1400; fx += 70)
        for (int fy = 0; fy < 1000; fy += 70)
            Raylib.DrawRectangle(fx, fy, 70, 70, ((fx / 70 + fy / 70) % 2 == 0)
                ? new Color((byte)225,(byte)205,(byte)185,(byte)255)
                : new Color((byte)210,(byte)190,(byte)170,(byte)255));

    // counter
    Raylib.DrawRectangle(400, 80, 400, 40, new Color((byte)140,(byte)90,(byte)45,(byte)255));
    Raylib.DrawRectangleLines(400, 80, 400, 40, Color.Black);
    Program.DrawTextUI("COUNTER", 550, 92, 16, Color.White);

    // card racks — spinning display stands
    (int x, int y, string label, Color col)[] racks = {
        (100, 200, "COMMONS", new Color((byte)200,(byte)200,(byte)205,(byte)255)),
        (350, 200, "RARES",   new Color((byte)160,(byte)90,(byte)220,(byte)255)),
        (600, 200, "HOLOS",   new Color((byte)230,(byte)190,(byte)60,(byte)255)),
    };
    foreach (var (rx, ry, label, col) in racks)
    {
        Raylib.DrawRectangle(rx, ry, 180, 100, new Color((byte)60,(byte)45,(byte)30,(byte)255));
        Raylib.DrawRectangleLines(rx, ry, 180, 100, Color.Black);
        for (int c = 0; c < 4; c++)
            Raylib.DrawRectangle(rx + 10 + c * 42, ry + 10, 34, 80, col);
        Program.DrawTextUI(label, rx + 10, ry + 4, 12, Color.White);
    }

    // binder display shelf
    Raylib.DrawRectangle(850, 200, 180, 100, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(850, 200, 180, 100, Color.Black);
    Program.DrawTextUI("BINDERS", 870, 204, 12, Color.White);
    for (int b = 0; b < 3; b++)
    {
        Raylib.DrawRectangle(860 + b * 55, 230, 45, 60, new Color((byte)40,(byte)30,(byte)70,(byte)255));
        Raylib.DrawRectangleLines(860 + b * 55, 230, 45, 60, new Color((byte)80,(byte)65,(byte)140,(byte)255));
    }

    // poster wall art
    Raylib.DrawRectangle(1100, 60, 240, 160, new Color((byte)250,(byte)245,(byte)230,(byte)255));
    Raylib.DrawRectangleLines(1100, 60, 240, 160, new Color((byte)120,(byte)90,(byte)50,(byte)255));
    Raylib.DrawCircle(1220, 140, 60, new Color((byte)235,(byte)215,(byte)55,(byte)255));
    Program.DrawTextUI("GOTTA COLLECT 'EM ALL", 1108, 200, 12, new Color((byte)120,(byte)90,(byte)50,(byte)255));

    // Entrance mat
    Raylib.DrawRectangle(600, 900, 200, 80, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(610, 910, 180, 60, new Color((byte)180,(byte)40,(byte)40,(byte)255));
}

static void CompleteSideTask(string receiver)
{
    int finalPay = activeSideTask.Pay + (int)(activeSideTask.Pay * GetReputationJobBonus());
    player.Money += finalPay;
    activeSideTask.DoneToday = true;
    activeSideTask.Accepted = false;
    activeSideTask.ReadyToDeliver = false;
    AddReputation(15, activeSideTask.Title);
    ShowNotification($"{receiver} thanks you! +${finalPay} ({activeSideTask.Title})");
    activeSideTask = null;
}

static void DrawBillboard()   // call inside BeginMode2D(camera) in DrawWorld
{
    int bx = (int)billboardPos.X, by = (int)billboardPos.Y;
    Raylib.DrawRectangle(bx - 6, by + 10, 12, 30, new Color((byte)90,(byte)60,(byte)35,(byte)255));   // post
    Raylib.DrawRectangle(bx - 40, by - 50, 80, 60, new Color((byte)120,(byte)85,(byte)50,(byte)255)); // frame
    Raylib.DrawRectangle(bx - 34, by - 44, 68, 48, new Color((byte)235,(byte)225,(byte)200,(byte)255));
    Program.DrawTextUI("TASKS", bx - 24, by - 36, 14, Color.DarkGray);
}

static void DrawPlushieLog()
{
    if (!plushLogOpen) return;
    int px = 160, py = 60;
    Raylib.DrawRectangle(px, py, 960, 600, new Color((byte)15,(byte)10,(byte)30,(byte)245));
    Raylib.DrawRectangleLines(px, py, 960, 600, new Color((byte)255,(byte)80,(byte)160,(byte)255));
    Program.DrawTextUI($"PLUSHIE COLLECTION — {plushiesOwned.Count}/50", px + 20, py + 14, 26, Color.Gold);

    int ly = py + 52;
    void Group(string label, string[] names, Color col)
    {
        Program.DrawTextUI($"{label} ({names.Count(n => plushiesOwned.ContainsKey(n))}/{names.Length})", px + 20, ly, 18, col);
        ly += 24;
        int cx = 0;
        foreach (var n in names)
        {
            bool owned = plushiesOwned.ContainsKey(n);
            int dupes = plushiesOwned.GetValueOrDefault(n);
            Program.DrawTextUI(owned ? (dupes > 1 ? $"{n} x{dupes}" : n) : "???",
                px + 20 + cx * 188, ly, 14, owned ? Color.White : Color.DarkGray);
            if (++cx == 5) { cx = 0; ly += 20; }
        }
        if (cx != 0) ly += 20;
        ly += 10;
    }
    Group("COMMON", plushCommon, Color.LightGray);
    Group("RARE", plushRare, Color.SkyBlue);
    Group("SUPER RARE", plushSuperRare, Color.Gold);
    Program.DrawTextUI("Q = Close", px + 440, py + 568, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) plushLogOpen = false;
}

static int GetResourceCount(string res) => res switch
{
    "Logs" => player.Logs, "Fish" => player.Fish, "Bones" => player.Bones, "Fur" => player.Fur, _ => 0
};
static void DrawJobBoard()
{
    if (!jobBoardOpen) return;
    int px = 340, py = 160;
    Raylib.DrawRectangle(px, py, 600, 420, new Color((byte)10,(byte)14,(byte)22,(byte)245));
    Raylib.DrawRectangleLines(px, py, 600, 420, Color.Gold);
    Program.DrawTextUI("JOB BOARD — deliver goods, get paid (resets daily)", px + 20, py + 14, 20, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();
    for (int i = 0; i < jobBoard.Count; i++)
    {
        var j = jobBoard[i];
        Rectangle row = new Rectangle(px + 20, py + 60 + i * 80, 560, 68);
        bool canTurnIn = !j.CompletedToday && GetResourceCount(j.Resource) >= j.Target;
        bool hov = Raylib.CheckCollisionPointRec(mouse, row);
        Raylib.DrawRectangleRec(row, new Color((byte)20,(byte)28,(byte)44,(byte)255));
        Raylib.DrawRectangleLinesEx(row, 2, j.CompletedToday ? Color.DarkGray : (canTurnIn ? Color.Green : Color.White));
        Program.DrawTextUI($"{j.Title}  ({j.Employer})", (int)row.X + 12, (int)row.Y + 8, 20,
            j.CompletedToday ? Color.DarkGray : Color.White);
        string status = j.CompletedToday ? "DONE TODAY"
            : $"{j.Resource}: {GetResourceCount(j.Resource)}/{j.Target}   Pay: ${j.Pay}" + (canTurnIn ? "   CLICK TO TURN IN" : "");
        Program.DrawTextUI(status, (int)row.X + 12, (int)row.Y + 38, 17,
            canTurnIn ? Color.Green : Color.LightGray);

        if (canTurnIn && hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            SpendResource(j.Resource, j.Target);
            int finalPay = j.Pay + (int)(j.Pay * GetReputationJobBonus());
            player.Money += finalPay;
            j.CompletedToday = true;
            AddReputation(10, j.Title);
            ShowNotification($"Job complete: {j.Title} (+${finalPay})");
        }
    }
    Program.DrawTextUI("Q = Close", px + 260, py + 388, 18, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) jobBoardOpen = false;
}

// Each machine draws relative to its center — one anchor moves the whole unit.
    static void DrawPinball(Vector2 c)
    {
        int mx=(int)c.X, my=(int)c.Y;
        Raylib.DrawRectangle(mx-35, my-45, 70, 110, new Color((byte)120,(byte)80,(byte)200,(byte)255));
        Raylib.DrawRectangle(mx-29, my-35, 58, 70, new Color((byte)25,(byte)20,(byte)50,(byte)255));
        Raylib.DrawCircle(mx-15, my-10, 4, Color.Orange);
        Raylib.DrawCircle(mx+5, my+5, 4, Color.Yellow);
        Raylib.DrawCircle(mx-5, my+20, 4, Color.Red);
        Program.DrawTextUI("PINBALL", mx-29, my+69, 11, Color.White);
    }

static void DrawTreeChopConfirm()
{
    if (!treeChopConfirmOpen || treeChopTarget == null) return;

    int w = 420, h = 180;
    int px = ScreenWidth / 2 - w / 2, py = ScreenHeight / 2 - h / 2;
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)150));
    Raylib.DrawRectangle(px, py, w, h, new Color((byte)25,(byte)20,(byte)15,(byte)250));
    Raylib.DrawRectangleLines(px, py, w, h, Color.Gold);

    string msg = $"Chop down this {treeChopTarget.FruitType} tree?";
    int tw = Program.MeasureTextUI(msg, 20);
    Program.DrawTextUI(msg, px + w/2 - tw/2, py + 30, 20, Color.White);
    Program.DrawTextUI("This will let you plant a different seed here.", px + 30, py + 62, 14, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();
    Rectangle yesBtn = new Rectangle(px + 40, py + h - 60, 150, 44);
    Rectangle noBtn  = new Rectangle(px + w - 190, py + h - 60, 150, 44);
    bool hY = Raylib.CheckCollisionPointRec(mouse, yesBtn);
    bool hN = Raylib.CheckCollisionPointRec(mouse, noBtn);

    Raylib.DrawRectangleRec(yesBtn, hY ? new Color((byte)120,(byte)40,(byte)40,(byte)255) : new Color((byte)60,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangleLinesEx(yesBtn, 2, hY ? Color.Red : Color.DarkGray);
    Program.DrawTextUI("Chop it down", (int)yesBtn.X + 14, (int)yesBtn.Y + 12, 16, Color.White);

    Raylib.DrawRectangleRec(noBtn, hN ? new Color((byte)40,(byte)90,(byte)40,(byte)255) : new Color((byte)30,(byte)50,(byte)30,(byte)255));
    Raylib.DrawRectangleLinesEx(noBtn, 2, hN ? Color.Green : Color.DarkGray);
    Program.DrawTextUI("Cancel", (int)noBtn.X + 42, (int)noBtn.Y + 12, 16, Color.White);

    if (hY && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        var tree = treeChopTarget;
        ShowNotification($"Chopped down the {tree.FruitType} tree.");
        tree.Tilled = false; tree.Planted = false; tree.FruitType = "";
        tree.GrowTimer = 0f; tree.ReadyToHarvest = false; tree.RegrowTimer = 0f;
        treeChopConfirmOpen = false; treeChopTarget = null;
    }
    if ((hN && Raylib.IsMouseButtonPressed(MouseButton.Left)) || Raylib.IsKeyPressed(KeyboardKey.Q))
    {
        treeChopConfirmOpen = false; treeChopTarget = null;
    }
}
static void StartTennisPoint(bool playerServes)
{
    tennisPlayerServing = playerServes;
    tennisServePhase = 1;
    tennisServeToss = 0f;
    tennisServeTossUp = true;
    tennisServeTimer = 0f;
    tennisBallInFlight = false;
    tennisBallBounced = false;
    tennisSwinging = false;

    // position players at their baselines
    tennisPlayerPos = new Vector2(CourtLeft + 60, (CourtTop + CourtBottom) / 2f);
    tennisAIPos     = new Vector2(CourtRight - 60, (CourtTop + CourtBottom) / 2f);

    // ball starts at the server's racket
    tennisBallPos = playerServes
        ? new Vector2(tennisPlayerPos.X + 20, tennisPlayerPos.Y)
        : new Vector2(tennisAIPos.X - 20, tennisAIPos.Y);
}

static void HitBall(Vector2 from, Vector2 target, float power, bool towardPlayer)
{
    tennisBallStart = from;
    tennisBallTarget = target;
    tennisBallPos = from;
    tennisBallT = 0f;
    tennisBallDuration = Math.Clamp(Vector2.Distance(from, target) / (power), 0.45f, 1.4f);
    tennisBallInFlight = true;
    tennisBallBounced = false;
    tennisBallToPlayer = towardPlayer;
}

static bool InCourt(Vector2 p, bool playerHalf)
{
    if (p.Y < CourtTop || p.Y > CourtBottom) return false;
    if (playerHalf) return p.X >= CourtLeft && p.X <= CourtMidX;
    return p.X >= CourtMidX && p.X <= CourtRight;
}

static void TennisPoint(bool playerWon)
{
    if (playerWon)
    {
        tennisPlayerScore++;
        player.AddSportsXP(8);
        tennisLastResult = "You win the point!";
    }
    else
    {
        tennisAIScore++;
        tennisLastResult = "AI wins the point.";
    }

    if (tennisPlayerScore >= tennisPointsToWin)
    {
        tennisMessage = "GAME! You win the match! +40 Sports XP";
        tennisMessageTimer = 4f;
        player.AddSportsXP(40);
        tennisActive = false;
        return;
    }
    if (tennisAIScore >= tennisPointsToWin)
    {
        tennisMessage = "Match over — AI wins.";
        tennisMessageTimer = 4f;
        tennisActive = false;
        return;
    }

    tennisMessage = $"{tennisLastResult}  {tennisPlayerScore} - {tennisAIScore}";
    tennisMessageTimer = 2f;
    // loser serves next
    StartTennisPoint(!playerWon ? true : false);
}

static void UpdateTennisPlay(float dt)
{
    const float reach = 70f;   // how close you must be to swing at the ball

    // ---- PLAYER MOVEMENT (free roam own half) ----
    Vector2 mv = Vector2.Zero;
    if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    mv.Y -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  mv.Y += 1;
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  mv.X -= 1;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) mv.X += 1;
    if (mv != Vector2.Zero) mv = Vector2.Normalize(mv);
    tennisPlayerPos += mv * tennisPlayerSpeed * dt;
    tennisPlayerPos.X = Math.Clamp(tennisPlayerPos.X, CourtLeft, CourtMidX - 10);
    tennisPlayerPos.Y = Math.Clamp(tennisPlayerPos.Y, CourtTop, CourtBottom);

    // trigger a swing
    if (tennisServePhase == 0 && tennisBallInFlight && Raylib.IsKeyPressed(KeyboardKey.Space) && tennisSwingCooldown <= 0)
    {
        tennisSwinging = true;
        tennisSwingTimer = 0.25f;     // quarter-second window to connect
        tennisSwingCooldown = 0.35f;
    }

    // swing window
    if (tennisSwinging)
    {
        tennisSwingTimer -= dt;
        if (tennisSwingTimer <= 0) tennisSwinging = false;
    }

    // ---- SERVE ----
    if (tennisServePhase != 0)
    {
        if (tennisPlayerServing)
        {
            // hold SPACE to build toss power, release to serve
            if (Raylib.IsKeyDown(KeyboardKey.Space))
            {
                tennisServePhase = 2;
                tennisServeToss += (tennisServeTossUp ? 1f : -1f) * 1.3f * dt;
                if (tennisServeToss >= 1f) { tennisServeToss = 1f; tennisServeTossUp = false; }
                if (tennisServeToss <= 0f) { tennisServeToss = 0f; tennisServeTossUp = true; }
                tennisBallPos = new Vector2(tennisPlayerPos.X + 20, tennisPlayerPos.Y);
            }
            else if (tennisServePhase == 2)
            {
                // released — serve toward AI service box
                float power = 700f + tennisServeToss * 500f;
                Vector2 target = new Vector2(
                    CourtMidX + Raylib.GetRandomValue(60, (int)(CourtRight - CourtMidX - 40)),
                    Raylib.GetRandomValue((int)CourtTop + 40, (int)CourtBottom - 40));
                HitBall(tennisBallPos, target, power, false);
                tennisServePhase = 0;
            }
        }
        else
        {
            // AI serve after a short delay
            tennisServeTimer += dt;
            tennisBallPos = new Vector2(tennisAIPos.X - 20, tennisAIPos.Y);
            if (tennisServeTimer > 1.0f)
            {
                Vector2 target = new Vector2(
                    Raylib.GetRandomValue((int)CourtLeft + 40, (int)CourtMidX - 40),
                    Raylib.GetRandomValue((int)CourtTop + 40, (int)CourtBottom - 40));
                HitBall(tennisBallPos, target, 850f, true);
                tennisServePhase = 0;
            }
        }
        return;
    }

    // ---- BALL IN FLIGHT ----
    if (tennisBallInFlight)
    {
        tennisBallT += dt / tennisBallDuration;
        tennisBallPos = Vector2.Lerp(tennisBallStart, tennisBallTarget, Math.Min(tennisBallT, 1f));

        // ---- AI MOVEMENT: chase the ball when it's coming to its side ----
        if (!tennisBallToPlayer)
        {
            Vector2 aiTarget = tennisBallTarget;
            Vector2 dir = aiTarget - tennisAIPos;
            if (dir.Length() > 4f)
                tennisAIPos += Vector2.Normalize(dir) * Math.Min(dir.Length(), tennisAIMaxSpeed * dt);
            tennisAIPos.X = Math.Clamp(tennisAIPos.X, CourtMidX + 10, CourtRight);
            tennisAIPos.Y = Math.Clamp(tennisAIPos.Y, CourtTop, CourtBottom);
        }

        // ball lands
        if (tennisBallT >= 1f)
        {
            bool landedInPlayerHalf = tennisBallToPlayer;
            bool landedIn = InCourt(tennisBallTarget, landedInPlayerHalf);

            if (!landedIn)
            {
                // shot went out — point to the receiver of this shot's hitter
                // if ball was heading to player and landed out, the hitter (AI) loses the point
                TennisPoint(tennisBallToPlayer ? true : false);
                return;
            }

            // ball bounced in — now the receiver must return it
            if (tennisBallToPlayer)
            {
                // PLAYER must swing in time and be near the ball
                bool near = Vector2.Distance(tennisPlayerPos, tennisBallPos) < reach;
                if (tennisSwinging && near)
                {
                    // return toward AI side, aim based on player vertical position
                    float power = 750f + Raylib.GetRandomValue(0, 200);
                    Vector2 target = new Vector2(
                        CourtMidX + Raylib.GetRandomValue(40, (int)(CourtRight - CourtMidX)),
                        Math.Clamp(tennisPlayerPos.Y + Raylib.GetRandomValue(-120, 120), CourtTop, CourtBottom));
                    HitBall(tennisBallPos, target, power, false);
                    player.AddSportsXP(3);
                }
                else
                {
                    TennisPoint(false); // missed the return
                }
            }
            else
            {
                // AI returns automatically if it reached the ball
                bool aiNear = Vector2.Distance(tennisAIPos, tennisBallPos) < reach + 10f;
                if (aiNear)
                {
                    float power = 720f;
                    Vector2 target = new Vector2(
                        Raylib.GetRandomValue((int)CourtLeft + 30, (int)CourtMidX - 20),
                        Math.Clamp(tennisAIPos.Y + Raylib.GetRandomValue(-140, 140), CourtTop, CourtBottom));
                    HitBall(tennisBallPos, target, power, true);
                }
                else
                {
                    TennisPoint(true); // AI couldn't reach
                }
            }
        }
    }
}

static int PracticalUnlockLevel() => ClassToTestFor switch {
    LicenceClass.D => 10, LicenceClass.C => 30, LicenceClass.B => 50,
    LicenceClass.A => 70, LicenceClass.S => 90, _ => 999
};

static bool IsNearDeepWater(Vector2 pos)
{
    // ocean zone — adjust to wherever your water actually is
    return pos.X > 28600 && pos.X < 45000;
}

static void DrawSleepScreen()
{
    // black fade overlay
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight,
        new Color((byte)0,(byte)0,(byte)0,(byte)(int)sleepFadeAlpha));

    if (sleepFadeAlpha < 200f) return;

    // zzz cloud bubble
    int cx = ScreenWidth / 2;
    int cy = ScreenHeight / 2;

    // cloud body
    Raylib.DrawEllipse(cx, cy, 90, 50, new Color((byte)40,(byte)40,(byte)60,(byte)220));
    Raylib.DrawEllipse(cx - 50, cy + 10, 50, 36, new Color((byte)40,(byte)40,(byte)60,(byte)220));
    Raylib.DrawEllipse(cx + 50, cy + 10, 50, 36, new Color((byte)40,(byte)40,(byte)60,(byte)220));
    Raylib.DrawEllipse(cx - 20, cy - 30, 44, 30, new Color((byte)40,(byte)40,(byte)60,(byte)220));
    Raylib.DrawEllipse(cx + 28, cy - 28, 44, 30, new Color((byte)40,(byte)40,(byte)60,(byte)220));

    // cloud outline
    Raylib.DrawEllipseLines(cx, cy, 90, 50, new Color((byte)160,(byte)160,(byte)220,(byte)180));

    // bouncing zzz letters — each offset by time so they drift up
    float t = zzzTimer;
    byte[] alphas = {
        (byte)(180 + 75 * MathF.Sin(t * 2f)),
        (byte)(180 + 75 * MathF.Sin(t * 2f - 1f)),
        (byte)(180 + 75 * MathF.Sin(t * 2f - 2f)),
    };
    float[] yOffsets = {
        MathF.Sin(t * 1.8f) * 8f,
        MathF.Sin(t * 1.8f - 0.8f) * 8f,
        MathF.Sin(t * 1.8f - 1.6f) * 8f,
    };

    Program.DrawTextUI("Z", cx - 36, (int)(cy - 12 + yOffsets[0]), 28, new Color((byte)200,(byte)200,(byte)255, alphas[0]));
    Program.DrawTextUI("Z", cx - 8,  (int)(cy - 22 + yOffsets[1]), 24, new Color((byte)200,(byte)200,(byte)255, alphas[1]));
    Program.DrawTextUI("Z", cx + 16, (int)(cy - 30 + yOffsets[2]), 20, new Color((byte)200,(byte)200,(byte)255, alphas[2]));
}
static void UpdateDungeon(float dt)
{
    var d = activeDungeon;
    if (!d.IsOpen) return;
    if (d.MessageTimer > 0) d.MessageTimer -= dt;

    // Pause menu
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        if (pauseMenuOpen)
        {
            pauseMenuOpen = false;
            Raylib.PlaySound(soundPauseClose);
        }
        else
        {
            pauseMenuOpen = true;
            Raylib.PlaySound(soundPauseOpen);
            Raylib.PauseMusicStream(currentMusic);
        }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.T))
    {
        currentPhase = currentPhase == HandPhase.Tools ? HandPhase.Combat : HandPhase.Tools;
    }
    
    if (Raylib.IsKeyPressed(KeyboardKey.Q))
        dungeonQuitConfirm = !dungeonQuitConfirm;

    if (dungeonQuitConfirm)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Y))
        {
            dungeonQuitConfirm = false;
            activeDungeon.Close();
            player.Position = activeDungeon.WorldReturnPos;
            ChangeScene(SceneState.World);
            return;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.N) || Raylib.IsKeyPressed(KeyboardKey.Escape))
            dungeonQuitConfirm = false;
        return;
    }

    // Armor/gear menu
    if (Raylib.IsKeyPressed(KeyboardKey.G))
         if (!pauseMenuOpen && !player.InventoryOpen && !skillsOpen && !questsOpen)
            armorMenuOpen = !armorMenuOpen;

    // Toolbar slot selection
    for (int k = 0; k < 8; k++)
        if (Raylib.IsKeyPressed(KeyboardKey.One + k))
            toolbarSelectedSlot = k;

    if (Raylib.IsKeyPressed(KeyboardKey.Space) && !chatInputOpen)
{
    string held = toolbarSlots[toolbarSelectedSlot];
    if (held != null && IsUsable(held))
        UseToolbarItem(toolbarSelectedSlot);
   
}

    float scroll = Raylib.GetMouseWheelMove();
    if (scroll != 0 && !pauseMenuOpen && !armorMenuOpen)
    {
        toolbarSelectedSlot = (int)Math.Clamp(toolbarSelectedSlot - scroll, 0, 7);
    }

    if (pauseMenuOpen || armorMenuOpen || player.InventoryOpen || dungeonQuitConfirm) return;

    if (d.Complete)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.E) && !chatInputOpen)
        {
            d.Close();
            player.Position = d.WorldReturnPos;
            ChangeScene(SceneState.World);
        }
        return;
    }

    var room = d.Rooms[d.CurrentRoom];

    Vector2 move = Vector2.Zero;
if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    move.Y -= 1;
if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  move.Y += 1;
if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  move.X -= 1;
if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;

if (move != Vector2.Zero)
{
    move = Vector2.Normalize(move);

    if (MathF.Abs(move.X) >= MathF.Abs(move.Y))
        player.Facing = move.X < 0 ? Player.FacingDirection.Left : Player.FacingDirection.Right;
    else
        player.Facing = move.Y < 0 ? Player.FacingDirection.Up : Player.FacingDirection.Down;
}

float dungeonSpeed = player.BaseSpeed * 0.8f;
Vector2 newPos = d.PlayerPos + move * dungeonSpeed * dt;

// clamp to room walls
newPos.X = Math.Clamp(newPos.X, Dungeon.InnerX + 22, Dungeon.InnerX + Dungeon.InnerW - 22);
newPos.Y = Math.Clamp(newPos.Y, Dungeon.InnerY + 22, Dungeon.InnerY + Dungeon.InnerH - 22);

// try full movement — if blocked, try sliding on each axis separately
if (!d.CollidesWithEnemy(newPos, 20f))
{
    d.PlayerPos = newPos;
}
else
{
    // try sliding on X axis only
    Vector2 slideX = new Vector2(newPos.X, d.PlayerPos.Y);
    slideX.X = Math.Clamp(slideX.X, Dungeon.InnerX + 22, Dungeon.InnerX + Dungeon.InnerW - 22);
    if (!d.CollidesWithEnemy(slideX, 20f))
        d.PlayerPos = slideX;
    else
    {
        // try sliding on Y axis only
        Vector2 slideY = new Vector2(d.PlayerPos.X, newPos.Y);
        slideY.Y = Math.Clamp(slideY.Y, Dungeon.InnerY + 22, Dungeon.InnerY + Dungeon.InnerH - 22);
        if (!d.CollidesWithEnemy(slideY, 20f))
            d.PlayerPos = slideY;
        // if both blocked, stay in place
    }
}

player.Position = d.PlayerPos;

    // Update enemies + combat
   foreach (var enemy in room.Enemies)
{
    if (enemy.Dead) continue;
    
    // always update movement regardless of attack state
    enemy.Update(dt, d.PlayerPos);

    // push enemies apart
    foreach (var other in room.Enemies)
    {
        if (other == enemy || other.Dead) continue;
        float sepDist = Vector2.Distance(enemy.Position, other.Position);
        float minDist = 46f;
        if (sepDist < minDist && sepDist > 0.001f)
        {
            Vector2 push = Vector2.Normalize(enemy.Position - other.Position);
            float overlap = (minDist - sepDist) / 2f;
            enemy.Position += push * overlap;
            other.Position -= push * overlap;

            enemy.Position.X = Math.Clamp(enemy.Position.X, Dungeon.InnerX + 24, Dungeon.InnerX + Dungeon.InnerW - 24);
            enemy.Position.Y = Math.Clamp(enemy.Position.Y, Dungeon.InnerY + 24, Dungeon.InnerY + Dungeon.InnerH - 24);
            other.Position.X = Math.Clamp(other.Position.X, Dungeon.InnerX + 24, Dungeon.InnerX + Dungeon.InnerW - 24);
            other.Position.Y = Math.Clamp(other.Position.Y, Dungeon.InnerY + 24, Dungeon.InnerY + Dungeon.InnerH - 24);
        }
    }

    float dist = Vector2.Distance(d.PlayerPos, enemy.Position);

    if (dist < 42f && enemy.AttackCooldown <= 0)
    {
        int def = GetTotalDefense();
        int dmg = Math.Max(1, enemy.Damage - def);
        player.Health -= dmg;
        enemy.AttackCooldown = 1.2f;
        floatingTexts.Add(new FloatingText {
            Position = new Vector2(d.PlayerPos.X, d.PlayerPos.Y - 30),
            Text = $"-{dmg}", Timer = 1f, TextColor = Color.Red
        });
    }

    // player attacks enemy
    if (dist < 65f && (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left)))
{
    string equipped = GetActiveWeapon();
    bool hasWeapon  = equipped == "Stick" || equipped == "Sword" || equipped == "Ruby Sword" || equipped == " Sapphire SwordSword";
    if (!hasWeapon)
    {
        d.Message = "Equip a weapon to fight!";
        d.MessageTimer = 1.5f;
    }
    else
    {
        int weaponBonus = equipped == "Sword" ? 5 : 0;
        int slotBonus   = GetWeaponDamage(armorWeapon);
        int atk         = 1 + (player.CombatLevel / 10) + weaponBonus + slotBonus;
        if (HasPerk("Combat", 15)) atk += 2;
        enemy.Health   -= atk;
        AwardMeleeXP(equipped, Math.Max(1, atk));

        // knock enemy away from player
        if (Vector2.Distance(d.PlayerPos, enemy.Position) > 0.01f)
        {
            Vector2 knockDir = Vector2.Normalize(enemy.Position - d.PlayerPos);
            enemy.Knockback = knockDir * 600f;
        }

        floatingTexts.Add(new FloatingText {
            Position = new Vector2(enemy.Position.X, enemy.Position.Y - 30),
            Text = $"-{atk}", Timer = 1f,
            TextColor = equipped == "Sword" ? Color.Orange : Color.Red
        });

        if (enemy.Health <= 0)
        {
            enemy.Dead = true;
            DropSpiritEssence(enemy.Position, 1);   
            SpawnDeathFx(enemy.Position, enemy.EnemyColor, enemy.Type);
            player.AddCombatXP(enemy.XPReward);
            player.Money += enemy.MoneyDrop;

            room.Loot.Add(new DungeonLoot(
                new Vector2(enemy.Position.X + Raylib.GetRandomValue(-15, 15),
                            enemy.Position.Y + Raylib.GetRandomValue(-15, 15)),
                enemy.LootType));

            d.Message = $"{enemy.Type} defeated!  +{enemy.XPReward} XP  +${enemy.MoneyDrop}";
            d.MessageTimer = 1.8f;
        }
    }
}
}
UpdateSplats(dt);
UpdateDeathFx(dt);

    foreach (var loot in room.Loot)
{
    if (loot.Collected) continue;
    if (Vector2.Distance(d.PlayerPos, loot.Position) < 30f)
    {
        string canonicalName = lootDropToItemName.GetValueOrDefault(loot.ItemType, loot.ItemType);
        if (TryGiveItem(canonicalName, 1))
        {
            loot.Collected = true;
            d.Message = $"+1 {loot.ItemType}";
            d.MessageTimer = 1f;
        }
        else
        {
            d.Message = "Inventory full!";
            d.MessageTimer = 1f;
        }
    }
}

    // Boss chest
    if (room.IsBoss && room.AllEnemiesDead && !room.ChestOpened)
    {
        Vector2 chestPos = new Vector2(Dungeon.InnerX + Dungeon.InnerW - 80, Dungeon.InnerY + Dungeon.InnerH / 2f);
        if (Vector2.Distance(d.PlayerPos, chestPos) < 65f && Raylib.IsKeyPressed(KeyboardKey.E))
        {
            room.ChestOpened = true;
            int reward = 80 + d.CurrentRoom * 25;
            player.Money += reward;
            player.AddCombatXP(250);
            d.Message = $"Dungeon Complete!  +${reward}  +250 Combat XP!";
            d.MessageTimer = 4f;
            d.Complete = true;
            dungeonsCleared++;
        }
    }

    // Advance to next room through right door
    if (room.AllEnemiesDead && !d.IsLastRoom)
    {
        if (d.PlayerPos.X > Dungeon.InnerX + Dungeon.InnerW - 28)
        {
            d.CurrentRoom++;
            d.PlayerPos = new Vector2(Dungeon.InnerX + 70, Dungeon.InnerY + Dungeon.InnerH / 2f);
            player.Position = d.PlayerPos;
            d.Message = d.Rooms[d.CurrentRoom].IsBoss
                ? "BOSS ROOM!  Defeat the boss!"
                : $"Room {d.CurrentRoom + 1} of {d.TotalRooms} — Fight!";
            d.MessageTimer = 2f;
        }
    }

    // Player death — boot out
    if (player.Health <= 0)
    {
        player.Health = Math.Max(1, player.MaxHealth / 3);
        player.Money  = Math.Max(0, player.Money - 30);
        d.Close();
        player.Position = d.WorldReturnPos;
        ChangeScene(SceneState.World);
        ShowNotification("Defeated! Lost $30 and fled the dungeon.");
    }

    // floating texts
    for (int i = floatingTexts.Count - 1; i >= 0; i--)
    {
        var ft = floatingTexts[i];
        ft.Timer -= dt;
        ft.Position.Y -= 40f * dt;
        floatingTexts[i] = ft;
        if (ft.Timer <= 0) floatingTexts.RemoveAt(i);
    }
}
static void DrawDungeon()
{
    var d    = activeDungeon;
    var room = d.Rooms[d.CurrentRoom];

    // Background color per dungeon type
    Color bgColor = d.Type switch {
        "Forest"  => new Color((byte)12,(byte)28,(byte)12,(byte)255),
        "Snow"    => new Color((byte)18,(byte)22,(byte)38,(byte)255),
        "Desert"  => new Color((byte)38,(byte)22,(byte)8, (byte)255),
        "Volcano" => new Color((byte)28,(byte)8, (byte)4, (byte)255),
        _         => new Color((byte)12,(byte)12,(byte)18,(byte)255)
    };
    Color wallColor = d.Type switch {
        "Forest"  => new Color((byte)30,(byte)65,(byte)30,(byte)255),
        "Snow"    => new Color((byte)55,(byte)65,(byte)95,(byte)255),
        "Desert"  => new Color((byte)105,(byte)78,(byte)42,(byte)255),
        "Volcano" => new Color((byte)85,(byte)28,(byte)10,(byte)255),
        _         => new Color((byte)50,(byte)45,(byte)58,(byte)255)
    };
    Color floorA = d.Type switch {
        "Forest"  => new Color((byte)22,(byte)48,(byte)22,(byte)255),
        "Snow"    => new Color((byte)38,(byte)48,(byte)68,(byte)255),
        "Desert"  => new Color((byte)75,(byte)58,(byte)28,(byte)255),
        "Volcano" => new Color((byte)45,(byte)18,(byte)6, (byte)255),
        _         => new Color((byte)32,(byte)28,(byte)38,(byte)255)
    };
    Color floorB = new Color(
        (byte)Math.Min(255, floorA.R + 10),
        (byte)Math.Min(255, floorA.G + 10),
        (byte)Math.Min(255, floorA.B + 10), (byte)255);

    Raylib.ClearBackground(bgColor);

    // ── FLOOR TILES ──────────────────────────────────────────────────────────
    for (int tx = Dungeon.InnerX; tx < Dungeon.InnerX + Dungeon.InnerW; tx += 55)
        for (int ty = Dungeon.InnerY; ty < Dungeon.InnerY + Dungeon.InnerH; ty += 55)
            Raylib.DrawRectangle(tx, ty, 55, 55,
                ((tx / 55 + ty / 55) % 2 == 0) ? floorA : floorB);

    // ── WALLS ────────────────────────────────────────────────────────────────
    Raylib.DrawRectangle(Dungeon.RoomX, Dungeon.RoomY, Dungeon.RoomW, Dungeon.WallThick, wallColor);
    Raylib.DrawRectangle(Dungeon.RoomX, Dungeon.RoomY + Dungeon.RoomH - Dungeon.WallThick, Dungeon.RoomW, Dungeon.WallThick, wallColor);
    Raylib.DrawRectangle(Dungeon.RoomX, Dungeon.RoomY, Dungeon.WallThick, Dungeon.RoomH, wallColor);
    Raylib.DrawRectangle(Dungeon.RoomX + Dungeon.RoomW - Dungeon.WallThick, Dungeon.RoomY, Dungeon.WallThick, Dungeon.RoomH, wallColor);

    // ── WALL TORCHES ─────────────────────────────────────────────────────────
    int[] torchXs = { Dungeon.RoomX + 70, Dungeon.RoomX + Dungeon.RoomW / 2 - 6, Dungeon.RoomX + Dungeon.RoomW - 90 };
    foreach (int tx2 in torchXs)
    {
        Raylib.DrawRectangle(tx2, Dungeon.RoomY + 2, 12, 18, new Color((byte)80,(byte)50,(byte)20,(byte)255));
        Raylib.DrawCircle(tx2 + 6, Dungeon.RoomY + 2, 9,  new Color((byte)255,(byte)180,(byte)0,(byte)255));
        Raylib.DrawCircle(tx2 + 6, Dungeon.RoomY + 2, 14, new Color((byte)255,(byte)100,(byte)0,(byte)50));
    }

    // ── DOORS ────────────────────────────────────────────────────────────────
    bool doorOpen = room.AllEnemiesDead && !d.IsLastRoom;
    int doorY = Dungeon.InnerY + Dungeon.InnerH / 2 - 45;

    // Right door (advance)
    Color doorCol = doorOpen
        ? new Color((byte)0,(byte)180,(byte)60,(byte)255)
        : new Color((byte)140,(byte)30,(byte)30,(byte)255);
    Raylib.DrawRectangle(Dungeon.RoomX + Dungeon.RoomW - Dungeon.WallThick, doorY, Dungeon.WallThick, 90, doorCol);
    Program.DrawTextUI(doorOpen ? ">>" : "X",
        Dungeon.RoomX + Dungeon.RoomW - Dungeon.WallThick + (doorOpen ? 2 : 6),
        doorY + 36, doorOpen ? 14 : 16, Color.White);

    // Left exit door (room 0 only)
    if (d.CurrentRoom == 0)
    {
        Raylib.DrawRectangle(Dungeon.RoomX, doorY, Dungeon.WallThick, 90, new Color((byte)0,(byte)140,(byte)220,(byte)255));
        Program.DrawTextUI("<<", Dungeon.RoomX + 2, doorY + 36, 14, Color.White);
    }

    // ── BOSS CHEST ───────────────────────────────────────────────────────────
    if (room.IsBoss && room.AllEnemiesDead)
    {
        int cX = Dungeon.InnerX + Dungeon.InnerW - 100;
        int cY = Dungeon.InnerY + Dungeon.InnerH / 2 - 30;
        if (!room.ChestOpened)
        {
            Raylib.DrawRectangle(cX, cY, 65, 52, new Color((byte)140,(byte)100,(byte)30,(byte)255));
            Raylib.DrawRectangle(cX, cY, 65, 20, new Color((byte)180,(byte)140,(byte)50,(byte)255));
            Raylib.DrawRectangle(cX + 24, cY + 5, 18, 12, new Color((byte)200,(byte)160,(byte)40,(byte)255));
            if (Vector2.Distance(d.PlayerPos, new Vector2(cX + 32, cY + 26)) < 70f)
            {
                int hw = Program.MeasureTextUI("E = Open Chest", 18);
                Program.DrawTextUI("E = Open Chest", cX + 32 - hw / 2, cY - 30, 18, Color.Gold);
            }
        }
        else
        {
            Raylib.DrawRectangle(cX, cY + 20, 65, 32, new Color((byte)140,(byte)100,(byte)30,(byte)255));
            Raylib.DrawRectangle(cX, cY,      65, 22, new Color((byte)180,(byte)140,(byte)50,(byte)255));
        }
    }

    // ── LOOT DROPS ───────────────────────────────────────────────────────────
    foreach (var loot in room.Loot)
    {
        if (loot.Collected) continue;
        Raylib.DrawCircle((int)loot.Position.X, (int)loot.Position.Y, 10, Color.Gold);
        Raylib.DrawCircleLines((int)loot.Position.X, (int)loot.Position.Y, 10, Color.Yellow);
        int lw = Program.MeasureTextUI(loot.ItemType, 11);
        Program.DrawTextUI(loot.ItemType, (int)loot.Position.X - lw / 2, (int)loot.Position.Y + 14, 11, Color.Gold);
    }

    // ── ENEMIES ──────────────────────────────────────────────────────────────
    foreach (var enemy in room.Enemies)
        enemy.Draw();
    DrawSplats();
    DrawDeathFx();

    // ── FLOATING TEXTS ───────────────────────────────────────────────────────
    foreach (var ft in floatingTexts)
    {
        byte alpha = (byte)(255 * Math.Max(0, ft.Timer / 1.2f));
        Program.DrawTextUI(ft.Text, (int)ft.Position.X, (int)ft.Position.Y, 22,
            new Color(ft.TextColor.R, ft.TextColor.G, ft.TextColor.B, alpha));
    }

    // ── PLAYER — full directional sprite via player.Draw() ───────────────────
    // player.Position is already synced to d.PlayerPos in UpdateDungeon
    // We draw without BeginMode2D so it renders at screen coords directly
    player.Hidden = false;
    player.Draw();

    // ── HUD ──────────────────────────────────────────────────────────────────

    // Dungeon name + room label
    int dnW = Program.MeasureTextUI(d.Name, 30);
    Program.DrawTextUI(d.Name, 640 - dnW / 2, 20, 30, Color.Gold);
    string roomLabel = room.IsBoss ? "★  BOSS ROOM  ★" : $"Room  {d.CurrentRoom + 1}  /  {d.TotalRooms}";
    int rlW = Program.MeasureTextUI(roomLabel, 20);
    Program.DrawTextUI(roomLabel, 640 - rlW / 2, 56, 20, room.IsBoss ? Color.Red : Color.LightGray);

    // Enemies remaining
    int alive = room.Enemies.Count(e => !e.Dead);
    Program.DrawTextUI(alive > 0 ? $"Enemies: {alive}" : "Room cleared!",
        20, 20, 20, alive > 0 ? Color.Red : Color.Green);

    // Wallet
    Program.DrawTextUI($"${player.Money}", ScreenWidth - 20 - Program.MeasureTextUI($"${player.Money}", 20), 20, 20, Color.Gold);

    // Player HP bar
    int hbW = 220, hbX = 640 - hbW / 2;
    Raylib.DrawRectangle(hbX, ScreenHeight - 100, hbW, 20, new Color((byte)40,(byte)40,(byte)40,(byte)220));
    float hpPct = (float)player.Health / player.MaxHealth;
    Color hpCol = hpPct > 0.5f ? Color.Green : hpPct > 0.25f ? Color.Orange : Color.Red;
    Raylib.DrawRectangle(hbX, ScreenHeight - 100, (int)(hbW * hpPct), 20, hpCol);
    Raylib.DrawRectangleLines(hbX, ScreenHeight - 100, hbW, 20, Color.White);
    Program.DrawTextUI($"HP  {player.Health} / {player.MaxHealth}", hbX + hbW / 2 - 50, ScreenHeight - 98, 16, Color.White);

    // Toolbar
    DrawToolbar();
    DrawCombatColumn();

    // Message banner
    if (d.MessageTimer > 0)
    {
        int mw = Program.MeasureTextUI(d.Message, 24);
        Raylib.DrawRectangle(640 - mw / 2 - 12, ScreenHeight - 160, mw + 24, 34,
            new Color((byte)0,(byte)0,(byte)0,(byte)190));
        Program.DrawTextUI(d.Message, 640 - mw / 2, ScreenHeight - 155, 24, Color.Yellow);
    }

    // Door advance hint
    if (room.AllEnemiesDead && !d.IsLastRoom)
    {
        int hw = Program.MeasureTextUI("Move right to advance!", 18);
        Program.DrawTextUI("Move right to advance!",
            Dungeon.RoomX + Dungeon.RoomW - hw - 30,
            Dungeon.RoomY + Dungeon.RoomH + 5, 18, Color.Green);
    }

    // Controls prompt
   if (showControlsHud)
{ 
    string prompt = d.Complete
        ? "E = Exit Dungeon"
        : d.CurrentRoom == 0
            ? "WASD / Arrow Keys = Move  |  SPACE or Click = Attack  |  G = Gear  |  Q = Exit"
            : "WASD / Arrow Keys = Move  |  SPACE or Click = Attack  |  G = Gear  |  ESC = Pause";

    int pw = Program.MeasureTextUI(prompt, 14);
    Program.DrawTextUI(prompt, 640 - pw / 2, ScreenHeight - 28, 14, Color.Gray);
}
    // Quit confirmation popup
if (dungeonQuitConfirm)
{
    // Darken screen
    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)160));

    // Popup box
    int popW = 420, popH = 200;
    int popX = ScreenWidth / 2 - popW / 2;
    int popY = ScreenHeight / 2 - popH / 2;
    Raylib.DrawRectangle(popX, popY, popW, popH, new Color((byte)20,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(popX, popY, popW, popH, Color.Red);

    // Title
    string title = "Quit Dungeon?";
    int tw = Program.MeasureTextUI(title, 28);
    Program.DrawTextUI(title, ScreenWidth / 2 - tw / 2, popY + 20, 28, Color.Red);

    // Warning text
    string warn = "All progress in this run will be lost.";
    int ww = Program.MeasureTextUI(warn, 18);
    Program.DrawTextUI(warn, ScreenWidth / 2 - ww / 2, popY + 62, 18, Color.LightGray);

    // Yes button
    Vector2 mouse = Raylib.GetMousePosition();
    Rectangle yesBtn = new Rectangle(ScreenWidth / 2 - 160, popY + 120, 140, 44);
    Rectangle noBtn  = new Rectangle(ScreenWidth / 2 + 20,  popY + 120, 140, 44);
    bool hYes = Raylib.CheckCollisionPointRec(mouse, yesBtn);
    bool hNo  = Raylib.CheckCollisionPointRec(mouse, noBtn);

    Raylib.DrawRectangleRec(yesBtn, hYes ? new Color((byte)160,(byte)30,(byte)30,(byte)255) : new Color((byte)80,(byte)20,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(yesBtn, 2, hYes ? Color.Red : Color.DarkGray);
    int yw = Program.MeasureTextUI("Yes (Y)", 20);
    Program.DrawTextUI("Yes (Y)", (int)(yesBtn.X + yesBtn.Width / 2 - yw / 2), (int)(yesBtn.Y + 12), 20, Color.White);

    Raylib.DrawRectangleRec(noBtn, hNo ? new Color((byte)30,(byte)100,(byte)30,(byte)255) : new Color((byte)20,(byte)60,(byte)20,(byte)255));
    Raylib.DrawRectangleLinesEx(noBtn, 2, hNo ? Color.Green : Color.DarkGray);
    int nw2 = Program.MeasureTextUI("No (N)", 20);
    Program.DrawTextUI("No (N)", (int)(noBtn.X + noBtn.Width / 2 - nw2 / 2), (int)(noBtn.Y + 12), 20, Color.White);

    // Mouse clicks
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        if (hYes)
        {
            dungeonQuitConfirm = false;
            activeDungeon.Close();
            player.Position = activeDungeon.WorldReturnPos;
            ChangeScene(SceneState.World);
        }
        else if (hNo)
            dungeonQuitConfirm = false;
    }
}

    // Pause and gear menus drawn on top of everything
    DrawArmorUI();
    DrawPauseMenu();
    DrawInventoryUI();
    DrawCalendarHUD();
}
public static bool IsTwoHandedWeapon(string weapon)
{
    if (weapon == null) return false;
    return weapon.Contains("Great Sword") || weapon.Contains("War Axe") || weapon.Contains("Battle Staff");
    // add any future 2H weapons here
}
static int GetTotalDefense()
{
    int def = 0;
    if (armorHelmet != null) def += GetArmorDefense(armorHelmet);
    if (armorBody   != null) def += GetArmorDefense(armorBody);
    if (armorLegs   != null) def += GetArmorDefense(armorLegs);
    if (armorBoots  != null) def += GetArmorDefense(armorBoots);
    if (armorGloves != null) def += GetArmorDefense(armorGloves);
    if (armorCape   != null) def += GetArmorDefense(armorCape);
    if (armorShield != null) def += GetArmorDefense(armorShield);
    return def;
}

static int GetArmorDefense(string item)
{
 if (item != null && (item.Contains("Mage ") || item.Contains("Ranger ")))
    {
        int baseVal = GetItemSlot(item) switch
        {
            "HELMET" => 1, "BODY" => 3, "LEGS" => 2, "BOOTS" => 1,
            "GLOVES" => 1, "CAPE" => 1, "SHIELD" => 2, _ => 0
        };
        if (item.Contains("Ranger ")) baseVal += 1;               // rangers run tougher gear
        return baseVal * (ClassTierIndex(item) + 1);              // Supernova body = 15, Serpent tunic = 24
    }   
    return item switch
    {
        // helmets
        "Leather Helmet"     => 1,
        "Iron Helmet"     => 3,
        "Steel Helmet"    => 5,
        "Gold Helmet"    => 7,
        "Sapphire Helmet"    => 9,
        "Ruby Helmet"    => 11,
        "Emerald Helmet"    => 13,
        "Diamond Helmet"    => 15,
        "Magic Helmet"    => 18,
        "Mystical Helmet"    => 20,
        "Infernal Helmet"    => 20,

        // body
        "Leather Chestplate"    => 2,
        "Iron Chestplate" => 5,
        "Steel Chestplate"=> 8,
        "Gold Chestplate"=> 12,
        "Sapphire Chestplate"=> 16,
        "Ruby Chestplate"=> 20,
        "Emerald Chestplate"=> 25,
        "Diamond Chestplate"=> 30,
        "Magic Chestplate"=> 35,
        "Mystical Chestplate"=> 40,
        "Infernal Chestplate"=> 40,

        // legs
        "Leather Leggings"   => 1,
        "Iron Leggings"   => 3,
        "Steel Leggings"  => 5,
        "Gold Leggings"  => 8,
        "Sapphire Leggings"  => 12,
        "Ruby Leggings"  => 16,
        "Emerald Leggings"  => 20,
        "Diamond Leggings"  => 24,
        "Magic Leggings"  => 28,
        "Mystical Leggings"  => 32,
        "Infernal Leggings"  => 32,

        // boots
        "Leather Boots"   => 1,
        "Iron Boots"      => 2,
        "Steel Boots"     => 3,
        "Gold Boots"   => 5,
        "Sapphire Boots"      => 7,
        "Ruby Boots"     => 9,
        "Emerald Boots"   => 12,
        "Diamond Boots"      => 15,
        "Magic Boots"     => 18,
        "Mystical Boots"      => 20,
        "Infernal Boots"     => 20,

        // gloves
        "Leather Gauntlets"  => 1,
        "Iron Gauntlets"  => 2,
        "Steel Gauntlets" => 3,
        "Gold Gauntlets"  => 5,
        "Sapphire Gauntlets"  => 7,
        "Ruby Gauntlets" => 9,
        "Emerald Gauntlets"  => 12,
        "Diamond Gauntlets"  => 15,
        "Magic Gauntlets" => 18,
        "Mystical Gauntlets"  => 20,
        "Infernal Gauntlets"  => 20,
     
        // cape
        "Leather Cape"       => 1,
        "Iron Cape"      => 3,
        "Steel Cape"       => 5,
        "Gold Cape"      => 8,
        "Sapphire Cape"       => 12,
        "Ruby Cape"      => 15,
        "EmeraldCape"       => 18,
        "Diamond Cape"      => 20,
        "Magic Cape"       => 22,
        "Mystical Cape"      => 25,
        "Infernal Cape"       => 25,

        // shields
        "Leather Shield"   => 4,
        "Iron Shield"     => 8,
        "Steel Shield"    => 12,
        "Gold Shield"   => 16,
        "Sapphire Shield"     => 20,
        "Ruby Shield"    => 24,
        "Emerald Shield"   => 28,
        "Diamond Shield"     => 32,
        "Magic Shield"    => 35,
        "Mystical Shield"   => 40,
        "Infernal Shield"     => 40,

        "Mage Helmet" => 1, "Mage Chestplate" => 3, "Mage Leggings" => 2, "Mage Boots" => 1, "Mage Gauntlets" => 1,
        "Ranger Helmet" => 2, "Ranger Chestplate" => 4, "Ranger Leggings" => 3, "Ranger Boots" => 2, "Ranger Gauntlets" => 2,
        "Mage Hat" => 1, "Mage Top" => 3, "Mage Bottoms" => 2, "Mage Gloves" => 1, "Mage Robe Cape" => 2, "Mage Book" => 3,
        "Ranger Hat" => 2, "Ranger Tunic" => 4, "Ranger Chaps" => 3, "Ranger Bracers" => 2, "Ranger Quiver" => 1,
        
        // weapons (weapon slot armor value = 0, damage handled separately)
        _ => 0
    };
}

static int GetWeaponDamage(string weapon)
{
    if (weapon == null) return 0;

    // material multiplier — applied on top of the base weapon-type damage
    int matBonus =
        weapon.Contains("Leather")  ? 0  :
        weapon.Contains("Iron")     ? 4  :
        weapon.Contains("Steel")    ? 8  :
        weapon.Contains("Gold")     ? 12 :
        weapon.Contains("Sapphire") ? 16 :
        weapon.Contains("Ruby")     ? 20 :
        weapon.Contains("Emerald")  ? 24 :
        weapon.Contains("Diamond")  ? 28 :
        weapon.Contains("Magic")    ? 34 :
        weapon.Contains("Mystical") ? 40 :
        weapon.Contains("Infernal") ? 48 : 0;

    // base damage by weapon type
    int baseDmg =
        weapon.Contains("Great Sword") ? 12 :
        weapon.Contains("Sword")       ? 5  :
        weapon.Contains("War Axe")     ? 10 :
        weapon.Contains("Great Staff") ? 14 :   // 2H staff
        weapon.EndsWith("Staff")       ? 8  :   // 1H staff
        weapon.Contains("Crossbow")    ? 9  :
        weapon.Contains("Bow")         ? 6  :
        weapon == "Stick"              ? 0  :
        0;

    return baseDmg + matBonus;
}
static void DrawCombatColumn()
{
     
    int box = 72;                          
    int gap = 0;                           
    int colX = 20;
    int colBaseY = ScreenHeight - 200;  
    Vector2 mouse = Raylib.GetMousePosition();

    // three slots stacked upward: ammo (top), 2H (mid), 1H (bottom, closest to toolbar)
    (string label, string value, Action onClick)[] cells =
    {
        ("AMMO", equippedAmmo, CycleAmmoSlot),
        ("2H",   equipped2H,   Cycle2HSlot),
        ("1H",   equipped1H,   Cycle1HSlot),
    };

    byte colAlpha = currentPhase == HandPhase.Combat ? (byte)230 : (byte)90;

    for (int i = 0; i < cells.Length; i++)
    {
        int y = colBaseY - i * (box + gap);
        var (label, value, onClick) = cells[i];

        bool hover = Raylib.CheckCollisionPointRec(mouse, new Rectangle(colX, y, box, box));

        Raylib.DrawRectangle(colX, y, box, box, new Color((byte)25,(byte)25,(byte)32, colAlpha));
        Raylib.DrawRectangleLines(colX, y, box, box, value != null ? Color.Gold : (hover ? Color.White : new Color((byte)90,(byte)90,(byte)110,(byte)255)));
        Program.DrawTextUI(label, colX + 3, y + 2, 11, new Color((byte)150,(byte)150,(byte)170,(byte)255));

        if (value != null)
        {
            if (label == "AMMO")
            {
                DrawInventoryIcon(value, colX + 10, y + 16, 48);
                int amt = value == "Arrows" ? player.Arrows
                        : value == "Bolts" ? player.Bolts
                        : value == "Arcane Essence" ? player.ArcaneEssence : 0;
                Raylib.DrawText($"{amt}", colX + box - 22, y + box - 18, 14, Color.White);
            }
            else
            {
                DrawArmorIcon(value, colX + 10, y + 14, 28);
            }
        }
        else
        {
            Program.DrawTextUI("-", colX + box / 2 - 3, y + box / 2 - 6, 16, Color.DarkGray);
        }

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            onClick();
    }
    if (currentPhase == HandPhase.Combat)
{
    int colHeight = cells.Length * (box + gap) - gap;
    int colTop = colBaseY - (cells.Length - 1) * (box + gap);
    Raylib.DrawRectangleLinesEx(
        new Rectangle(colX - 4, colTop - 4, box + 8, colHeight + 8),
        3, Color.Gold);
}
}

// True if every toolbar slot is occupied.

// Routes an item: toolbar first, then player inventory (stat counter), else fail.

static void DrawArmorIcon(string item, int x, int y, int size)
{
    int cx = x + size / 2;
    int cy = y + size / 2;

    // inside DrawArmorIcon, add before or after the WEAPON case
    if (item == "Arrows")
    {
        Raylib.DrawLineEx(new Vector2(cx - 8, cy + 8), new Vector2(cx + 8, cy - 8), 2, new Color((byte)160,(byte)120,(byte)70,(byte)255));
        Raylib.DrawTriangle(new Vector2(cx + 8, cy - 8), new Vector2(cx + 3, cy - 7), new Vector2(cx + 7, cy - 3), Color.Gray);
        return;
    }
    if (item == "Bolts")
    {
        Raylib.DrawLineEx(new Vector2(cx - 8, cy + 8), new Vector2(cx + 8, cy - 8), 3, new Color((byte)120,(byte)90,(byte)50,(byte)255));
        return;
    }
    if (item == "Arcane Essence")
    {
        Raylib.DrawCircle(cx, cy, 12, new Color((byte)100,(byte)40,(byte)160,(byte)120));
        Raylib.DrawCircle(cx, cy, 8, new Color((byte)160,(byte)80,(byte)255,(byte)255));
        Raylib.DrawCircle(cx - 2, cy - 2, 3, Color.White);
        return;
    }

    if (item.EndsWith("Hat"))
    {
        Color hc = Program.MaterialColor(item.Split(' ')[0]);   
        Raylib.DrawRectangle(cx - 10, cy + 4, 20, 3, hc);
        Raylib.DrawTriangle(new Vector2(cx - 6, cy + 4), new Vector2(cx + 6, cy + 4), new Vector2(cx + 3, cy - 9), hc);
        return;
    }

    if (item.EndsWith("Book"))
    {
        Raylib.DrawRectangle(cx - 7, cy - 8, 14, 16, new Color((byte)130,(byte)70,(byte)200,(byte)255));
        Raylib.DrawRectangle(cx - 4, cy - 6, 10, 12, new Color((byte)235,(byte)230,(byte)210,(byte)255));
        return;
    }
    if (item.EndsWith("Quiver"))
    {
        Raylib.DrawRectangle(cx - 4, cy - 4, 8, 14, new Color((byte)110,(byte)75,(byte)40,(byte)255));
        Raylib.DrawLineEx(new Vector2(cx - 1, cy - 4), new Vector2(cx - 3, cy - 11), 2, Color.White);
        Raylib.DrawLineEx(new Vector2(cx + 2, cy - 4), new Vector2(cx + 1, cy - 11), 2, Color.LightGray);
        return;
    }

    switch (GetItemSlot(item))
    {
        case "HELMET":
{
    Color c = IconWeaponColor(item, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    Raylib.DrawRectangle(cx - 8, cy - 2, 16, 10, c);
    Raylib.DrawRectangle(cx - 10, cy - 8, 20, 8, c);
    Raylib.DrawRectangle(cx - 6, cy + 8, 12, 4, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
    break;
}
case "BODY":
{
    Color c = IconWeaponColor(item, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    Raylib.DrawRectangle(cx - 10, cy - 8, 20, 18, c);
    Raylib.DrawRectangle(cx - 14, cy - 6, 6, 12, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
    Raylib.DrawRectangle(cx + 8, cy - 6, 6, 12, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
    break;
}
case "LEGS":
{
    Color c = IconWeaponColor(item, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    Raylib.DrawRectangle(cx - 9, cy - 8, 8, 18, c);
    Raylib.DrawRectangle(cx + 1, cy - 8, 8, 18, c);
    break;
}
case "BOOTS":
{
    Color c = IconWeaponColor(item, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawRectangle(cx - 8, cy - 6, 7, 14, c);
    Raylib.DrawRectangle(cx + 1, cy - 6, 7, 14, c);
    Raylib.DrawRectangle(cx - 10, cy + 6, 9, 4, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
    Raylib.DrawRectangle(cx + 1, cy + 6, 9, 4, new Color((byte)Math.Max(0,c.R-20),(byte)Math.Max(0,c.G-20),(byte)Math.Max(0,c.B-20),(byte)255));
    break;
}
case "GLOVES":
{
    Color c = IconWeaponColor(item, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    Raylib.DrawRectangle(cx - 8, cy - 4, 6, 10, c);
    Raylib.DrawRectangle(cx + 2, cy - 4, 6, 10, c);
    for (int f = 0; f < 4; f++)
        Raylib.DrawRectangle(cx - 8 + f * 4, cy - 8, 3, 6, c);
    break;
}
case "CAPE":
{
    Color c = IconWeaponColor(item, new Color((byte)140,(byte)60,(byte)60,(byte)255));
    Raylib.DrawTriangle(
        new Vector2(cx - 10, cy - 8),
        new Vector2(cx + 10, cy - 8),
        new Vector2(cx, cy + 12),
        c);
    break;
}
case "SHIELD":
{
    Color c = IconWeaponColor(item, new Color((byte)100,(byte)80,(byte)40,(byte)255));
    Raylib.DrawRectangle(cx - 8, cy - 10, 16, 20, c);
    Raylib.DrawTriangle(
        new Vector2(cx - 8, cy + 10),
        new Vector2(cx + 8, cy + 10),
        new Vector2(cx, cy + 18),
        c);
    Raylib.DrawCircle(cx, cy, 4, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    break;
}
        case "WEAPON":
            // WEAPON icons (detect by name, tint by material)
if (item.Contains("Great Sword"))
{
    Color blade = IconWeaponColor(item, new Color((byte)180,(byte)190,(byte)200,(byte)255));
    Raylib.DrawRectangle(cx - 2, cy - 16, 4, 24, blade);                       // long blade
    Raylib.DrawTriangle(new Vector2(cx - 2, cy - 16), new Vector2(cx + 2, cy - 16), new Vector2(cx, cy - 22), blade);
    Raylib.DrawRectangle(cx - 10, cy + 6, 20, 4, new Color((byte)180,(byte)140,(byte)40,(byte)255)); // wide guard
    Raylib.DrawRectangle(cx - 2, cy + 10, 4, 8, new Color((byte)120,(byte)80,(byte)30,(byte)255));   // grip
    return;
}
if (item.Contains("War Axe"))
{
    Color metal = IconWeaponColor(item, new Color((byte)160,(byte)160,(byte)170,(byte)255));
    Raylib.DrawRectangle(cx - 2, cy - 14, 4, 30, new Color((byte)120,(byte)80,(byte)30,(byte)255));   // handle
    Raylib.DrawTriangle(new Vector2(cx, cy - 12), new Vector2(cx, cy + 2), new Vector2(cx + 14, cy - 5), metal); // axe head
    Raylib.DrawTriangle(new Vector2(cx, cy - 9), new Vector2(cx - 8, cy - 5), new Vector2(cx, cy - 1), metal);   // back spike
    return;
}
if (item.Contains("Sword"))
{
    Color blade = IconWeaponColor(item, new Color((byte)180,(byte)190,(byte)200,(byte)255));
    Raylib.DrawRectangle(cx - 2, cy - 12, 4, 18, blade);
    Raylib.DrawTriangle(new Vector2(cx - 2, cy - 12), new Vector2(cx + 2, cy - 12), new Vector2(cx, cy - 18), blade);
    Raylib.DrawRectangle(cx - 8, cy + 4, 16, 4, new Color((byte)180,(byte)140,(byte)40,(byte)255));
    Raylib.DrawRectangle(cx - 2, cy + 8, 4, 8, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    return;
}

if (item.Contains("Great") && item.Contains("Staff"))
{
    Color orb = GetStaffColor(item);
    // long shaft (2H)
    Raylib.DrawRectangle(cx - 2, cy - 16, 4, 32, new Color((byte)100,(byte)70,(byte)40,(byte)255));
    // large glowing orb at top
    Raylib.DrawCircle(cx, cy - 18, 10, new Color(orb.R, orb.G, orb.B, (byte)110));
    Raylib.DrawCircle(cx, cy - 18, 7, orb);
    Raylib.DrawCircle(cx - 2, cy - 20, 2, Color.White);
    // second orb at base to mark it as the greater 2H version
    Raylib.DrawCircle(cx, cy + 12, 4, orb);
    return;
}
if (item.Contains("Staff"))
{
    Color orb = GetStaffColor(item);
    // shorter shaft (1H)
    Raylib.DrawRectangle(cx - 2, cy - 10, 4, 22, new Color((byte)110,(byte)80,(byte)45,(byte)255));
    // glowing orb at top
    Raylib.DrawCircle(cx, cy - 14, 7, new Color(orb.R, orb.G, orb.B, (byte)120));
    Raylib.DrawCircle(cx, cy - 14, 5, orb);
    Raylib.DrawCircle(cx - 1, cy - 15, 2, Color.White);
    return;
}
if (item.Contains("Stick"))
{
    Raylib.DrawRectangle(cx - 2, cy - 14, 4, 28, new Color((byte)120,(byte)80,(byte)30,(byte)255));
    return;
}
if (item.Contains("Bow"))
{
    Color limb = IconWeaponColor(item, new Color((byte)140,(byte)90,(byte)40,(byte)255));
    Raylib.DrawLineEx(new Vector2(cx, cy - 16), new Vector2(cx, cy + 16), 3, limb);
    Raylib.DrawLineEx(new Vector2(cx, cy - 16), new Vector2(cx - 5, cy - 9), 2, limb);
    Raylib.DrawLineEx(new Vector2(cx, cy + 16), new Vector2(cx - 5, cy + 9), 2, limb);
    Raylib.DrawLineEx(new Vector2(cx + 4, cy - 14), new Vector2(cx + 4, cy + 14), 1, new Color((byte)220,(byte)220,(byte)220,(byte)255));
    return;
}
if (item.Contains("Crossbow"))
{
    Color limb = IconWeaponColor(item, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    Raylib.DrawLineEx(new Vector2(cx - 12, cy - 4), new Vector2(cx + 12, cy - 4), 3, limb);
    Raylib.DrawLineEx(new Vector2(cx, cy - 8), new Vector2(cx, cy + 12), 4, limb);
    Raylib.DrawTriangle(new Vector2(cx, cy - 14), new Vector2(cx - 3, cy - 8), new Vector2(cx + 3, cy - 8), Color.Gray);
    return;
}         
                // generic fallback
                Raylib.DrawLineEx(new Vector2(cx - 6, cy + 10), new Vector2(cx + 6, cy - 10), 3,
                    new Color((byte)180,(byte)190,(byte)200,(byte)255));
                Raylib.DrawLineEx(new Vector2(cx - 6, cy), new Vector2(cx + 6, cy), 4,
                    new Color((byte)180,(byte)140,(byte)40,(byte)255));
            break;
    }
}

static void DrawMannequin(int cx, int cy)
{
    // simple mannequin silhouette showing equipped gear
    Color helmetCol  = armorHelmet != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color bodyCol    = armorBody   != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color legsCol    = armorLegs   != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color bootsCol   = armorBoots  != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color glovesCol  = armorGloves != null ? Color.Gold    : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color capeCol    = armorCape   != null ? Color.Purple  : new Color((byte)60,(byte)60,(byte)70,(byte)255);
    Color shieldCol  = armorShield != null ? Color.SkyBlue : new Color((byte)60,(byte)60,(byte)70,(byte)255);

    // cape (behind body)
    Raylib.DrawTriangle(
        new Vector2(cx - 14, cy - 30),
        new Vector2(cx + 14, cy - 30),
        new Vector2(cx,      cy + 40),
        capeCol);

    // helmet
    Raylib.DrawCircle(cx, cy - 42, 14, helmetCol);
    // body
    Raylib.DrawRectangle(cx - 14, cy - 28, 28, 38, bodyCol);
    // legs
    Raylib.DrawRectangle(cx - 12, cy + 10, 10, 26, legsCol);
    Raylib.DrawRectangle(cx + 2,  cy + 10, 10, 26, legsCol);
    // boots
    Raylib.DrawRectangle(cx - 14, cy + 34, 12, 8, bootsCol);
    Raylib.DrawRectangle(cx + 2,  cy + 34, 12, 8, bootsCol);
    // gloves / arms
    Raylib.DrawRectangle(cx - 24, cy - 26, 10, 24, glovesCol);
    Raylib.DrawRectangle(cx + 14, cy - 26, 10, 24, glovesCol);
    // shield (left arm)
    if (armorShield != null && !IsTwoHandedWeapon(armorWeapon))
    {
        Raylib.DrawRectangle(cx - 38, cy - 22, 12, 20, shieldCol);
        Raylib.DrawTriangle(
            new Vector2(cx - 38, cy - 2),
            new Vector2(cx - 26, cy - 2),
            new Vector2(cx - 32, cy + 8),
            shieldCol);
    }
}
static string GetActiveWeapon()
{
    if (equipped2H != null) return equipped2H;
    if (equipped1H != null) return equipped1H;
    return GetEquippedTool();   // fall back to toolbar sword/stick
}
static void DrawSwimmingOverlay()
{
    float ratio = player.SwimDepthRatio;
    if (ratio <= 0.05f) return;

    int x = (int)player.Position.X;
    int y = (int)player.Position.Y;
    int spriteTop = y + 22;      // matches the helmet spike's highest point
    int spriteBottom = y + 70;   // matches the boots
    int spriteHeight = spriteBottom - spriteTop;

    int waterlineY = (int)(spriteBottom - spriteHeight * Math.Min(ratio, 0.9f)) - 8;
    float bob = MathF.Sin(player.SwimStrokeTimer * 2f) * 2f;
    Raylib.DrawEllipse(x + 20, waterlineY + (int)bob, 26, 7, new Color((byte)70,(byte)140,(byte)200,(byte)120));
    Raylib.DrawEllipse(x + 20, waterlineY + (int)bob, 20, 5, new Color((byte)30,(byte)100,(byte)180,(byte)160));

    if (ratio >= 0.85f)
    {
        float strokeOffset = (float)Math.Sin(player.SwimStrokeTimer) * 12f;
        int armY = waterlineY - 4;
        int centerX = x + 20;   // matches the sprite's horizontal center (head at x+20)
        Raylib.DrawEllipse((int)(centerX - 16 + strokeOffset), armY, 9, 5, player.SkinColor);
        Raylib.DrawEllipse((int)(centerX + 16 - strokeOffset), armY, 9, 5, player.SkinColor);
    }
}

static readonly (PlayerMenuTab tab, string label)[] playerMenuTabs =
{
    (PlayerMenuTab.Identity,      "Identity"),
    (PlayerMenuTab.Stats,         "Stats"),
    (PlayerMenuTab.Crafting,      "Crafting"),
    (PlayerMenuTab.Achievements,  "Achievements"),
    (PlayerMenuTab.Bestiary,      "Bestiary"),
    (PlayerMenuTab.Unlocks,       "Unlocks"),
    (PlayerMenuTab.Relationships, "Relationships"),
    (PlayerMenuTab.Collectables,  "Collectables"),
    (PlayerMenuTab.Collection,    "Collection Log"),
};

static void DrawPMPlaceholder(int cx, int cy, string label)
{
    Program.DrawTextUI(label.ToUpper(), cx, cy, 26, Color.Gold);
    Program.DrawTextUI("Coming soon.", cx, cy + 40, 18, Color.LightGray);
}

static void DrawPMAchievements(int x, int y, int w)
{
    Program.DrawTextUI("ACHIEVEMENTS", x, y, 34, Color.Gold);

    // completion summary
    float pct = achievements.Count > 0 ? (float)achievementsUnlockedCount / achievements.Count : 0f;
    Program.DrawTextUI($"{achievementsUnlockedCount}/{achievements.Count} unlocked", x + 260, y + 8, 20, Color.White);
    Raylib.DrawRectangle(x, y + 42, w - 40, 14, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(x, y + 42, (int)((w - 40) * pct), 14, new Color((byte)255,(byte)215,(byte)0,(byte)255));
    Program.DrawTextUI($"{pct*100:F1}%", x + w - 80, y + 40, 16, Color.Gold);

    // category tabs
    Vector2 mouse = Raylib.GetMousePosition();
    int tabY = y + 66;
    int tabX = x;
    for (int i = 0; i < achievementCategories.Length; i++)
    {
        string cat = achievementCategories[i];
        int catW = Program.MeasureTextUI(cat, 15) + 16;
        Rectangle tab = new Rectangle(tabX, tabY, catW, 26);
        bool hover = Raylib.CheckCollisionPointRec(mouse, tab);
        bool active = pmAchCategory == i;
        Raylib.DrawRectangleRec(tab, active ? new Color((byte)60,(byte)50,(byte)20,(byte)255) : new Color((byte)26,(byte)30,(byte)44,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 1, active ? Color.Gold : (hover ? Color.White : new Color((byte)60,(byte)66,(byte)90,(byte)255)));
        Program.DrawTextUI(cat, tabX + 8, tabY + 5, 15, active ? Color.Gold : (hover ? Color.White : Color.Gray));
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left)) { pmAchCategory = i; pmAchScrollY = 0f; }
        tabX += catW + 4;
        if (tabX > x + w - 80) { tabX = x; tabY += 30; }
    }

    // filtered list
    int contentTop = tabY + 34;
    int contentH = 640 - (contentTop - y);  // fill rest of panel
    string filterCat = pmAchCategory == 0 ? null : achievementCategories[pmAchCategory];
    var filtered = filterCat == null ? achievements : achievements.Where(a => a.Category == filterCat).ToList();

    int rowH = 62;
    int totalH = filtered.Count * rowH;
    float maxScroll = Math.Max(0, totalH - contentH);
    if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(x, contentTop, w - 40, contentH)))
        pmAchScrollY = Math.Clamp(pmAchScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
    pmAchScrollY = Math.Clamp(pmAchScrollY, 0f, maxScroll);

    Raylib.BeginScissorMode(x, contentTop, w - 40, contentH);
    for (int i = 0; i < filtered.Count; i++)
    {
        var a = filtered[i];
        int ry = contentTop + i * rowH - (int)pmAchScrollY;
        if (ry + rowH < contentTop || ry > contentTop + contentH) continue;

        int rw = w - 50;
        Color bgCol = a.Unlocked
            ? new Color((byte)30,(byte)42,(byte)24,(byte)220)
            : new Color((byte)22,(byte)24,(byte)32,(byte)220);
        Raylib.DrawRectangle(x, ry, rw, rowH - 4, bgCol);
        Raylib.DrawRectangleLinesEx(new Rectangle(x, ry, rw, rowH - 4), 1,
            a.Unlocked ? new Color((byte)120,(byte)180,(byte)60,(byte)255) : new Color((byte)50,(byte)55,(byte)65,(byte)255));

        // icon circle
        int iconX = x + 26, iconY = ry + rowH / 2 - 2;
        if (a.Unlocked)
        {
            Raylib.DrawCircle(iconX, iconY, 16, a.IconColor);
            Program.DrawTextUI("★", iconX - 8, iconY - 12, 22, new Color((byte)255,(byte)255,(byte)255,(byte)230));
        }
        else
        {
            Raylib.DrawCircle(iconX, iconY, 16, new Color((byte)40,(byte)40,(byte)48,(byte)255));
            Raylib.DrawCircleLines(iconX, iconY, 16, new Color((byte)60,(byte)66,(byte)90,(byte)255));
            Program.DrawTextUI("?", iconX - 5, iconY - 10, 18, Color.DarkGray);
        }

        // title + description
        Color titleCol = a.Unlocked ? Color.White : Color.Gray;
        Color descCol = a.Unlocked ? Color.LightGray : new Color((byte)80,(byte)80,(byte)90,(byte)255);
        Program.DrawTextUI(a.Title, x + 52, ry + 6, 20, titleCol);
        Program.DrawTextUI(a.Description, x + 52, ry + 28, 15, descCol);

        // category badge
        Program.DrawTextUI(a.Category, x + rw - 160, ry + 6, 12, new Color((byte)140,(byte)140,(byte)160,(byte)200));

        // reward + status on the right
        if (a.Reward > 0)
        {
            string rText = a.Unlocked ? $"+${a.Reward}" : $"${a.Reward}";
            Color rCol = a.Unlocked ? new Color((byte)100,(byte)200,(byte)80,(byte)255) : new Color((byte)150,(byte)130,(byte)50,(byte)180);
            Program.DrawTextUI(rText, x + rw - 70, ry + 6, 16, rCol);
        }

        string status = a.Unlocked ? "UNLOCKED" : "LOCKED";
        Color stCol = a.Unlocked ? new Color((byte)100,(byte)220,(byte)60,(byte)255) : new Color((byte)100,(byte)100,(byte)110,(byte)255);
        Program.DrawTextUI(status, x + rw - 90, ry + 30, 13, stCol);
    }
    Raylib.EndScissorMode();

    // scroll hint
    if (maxScroll > 0)
        Program.DrawTextUI("Scroll to see more", x + w / 2 - 70, contentTop + contentH + 4, 14, Color.DarkGray);
}

static void DrawPMBestiary(int x, int y, int w)
{
    int discovered = bestiary.Values.Count(e => e.Discovered);
    int total = bestiary.Count;
    Program.DrawTextUI("BESTIARY", x, y, 34, Color.Gold);
    Program.DrawTextUI($"{discovered}/{total} discovered", x + 220, y + 8, 20, Color.White);

    // progress bar
    float pct = total > 0 ? (float)discovered / total : 0f;
    Raylib.DrawRectangle(x, y + 42, w - 40, 14, new Color((byte)40,(byte)40,(byte)40,(byte)255));
    Raylib.DrawRectangle(x, y + 42, (int)((w - 40) * pct), 14, new Color((byte)220,(byte)50,(byte)50,(byte)255));
    Program.DrawTextUI($"{pct*100:F0}%", x + w - 80, y + 40, 16, Color.Red);

    // filter tabs
    Vector2 mouse = Raylib.GetMousePosition();
    string[] filters = { "All", "Discovered", "Undiscovered" };
    int tabX = x;
    for (int i = 0; i < filters.Length; i++)
    {
        int tw = Program.MeasureTextUI(filters[i], 16) + 16;
        Rectangle tab = new Rectangle(tabX, y + 64, tw, 26);
        bool hover = Raylib.CheckCollisionPointRec(mouse, tab);
        bool active = pmBestFilter == i;
        Raylib.DrawRectangleRec(tab, active ? new Color((byte)60,(byte)30,(byte)20,(byte)255) : new Color((byte)26,(byte)30,(byte)44,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 1, active ? Color.Red : (hover ? Color.White : new Color((byte)60,(byte)66,(byte)90,(byte)255)));
        Program.DrawTextUI(filters[i], tabX + 8, y + 69, 16, active ? Color.Red : (hover ? Color.White : Color.Gray));
        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left)) { pmBestFilter = i; pmBestScrollY = 0f; }
        tabX += tw + 6;
    }

    // total kills stat
    int totalKills = bestiary.Values.Sum(e => e.Kills);
    Program.DrawTextUI($"Total Kills: {totalKills}", x + w - 200, y + 69, 16, Color.LightGray);

    // filtered + sorted list (discovered first, then alphabetical)
    var entries = bestiary.Values.ToList();
    if (pmBestFilter == 1) entries = entries.Where(e => e.Discovered).ToList();
    else if (pmBestFilter == 2) entries = entries.Where(e => !e.Discovered).ToList();
    entries.Sort((a, b) =>
    {
        if (a.Discovered != b.Discovered) return a.Discovered ? -1 : 1;
        return string.Compare(a.Type, b.Type, StringComparison.Ordinal);
    });

    int contentTop = y + 98;
    int contentH = 580;
    int rowH = 90;
    int totalH = entries.Count * rowH;
    float maxScroll = Math.Max(0, totalH - contentH);
    if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(x, contentTop, w - 40, contentH)))
        pmBestScrollY = Math.Clamp(pmBestScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
    pmBestScrollY = Math.Clamp(pmBestScrollY, 0f, maxScroll);

    Raylib.BeginScissorMode(x, contentTop, w - 40, contentH);
    for (int i = 0; i < entries.Count; i++)
    {
        var e = entries[i];
        int ry = contentTop + i * rowH - (int)pmBestScrollY;
        if (ry + rowH < contentTop || ry > contentTop + contentH) continue;

        int rw = w - 50;
        Color bgCol = e.Discovered
            ? new Color((byte)28,(byte)30,(byte)38,(byte)230)
            : new Color((byte)18,(byte)18,(byte)22,(byte)230);
        Raylib.DrawRectangle(x, ry, rw, rowH - 4, bgCol);
        Raylib.DrawRectangleLinesEx(new Rectangle(x, ry, rw, rowH - 4), 1,
            e.Discovered ? new Color((byte)100,(byte)60,(byte)60,(byte)255) : new Color((byte)40,(byte)40,(byte)48,(byte)255));

        // enemy icon (circle silhouette or colored)
        int iconX = x + 30, iconY = ry + rowH / 2 - 2;
        if (e.Discovered)
        {
            Raylib.DrawCircle(iconX, iconY, 20, e.EnemyColor);
            Raylib.DrawCircle(iconX, iconY, 16, new Color(
                (byte)Math.Min(255, e.EnemyColor.R + 30),
                (byte)Math.Min(255, e.EnemyColor.G + 30),
                (byte)Math.Min(255, e.EnemyColor.B + 30), (byte)255));
        }
        else
        {
            Raylib.DrawCircle(iconX, iconY, 20, new Color((byte)30,(byte)30,(byte)35,(byte)255));
            Raylib.DrawCircleLines(iconX, iconY, 20, new Color((byte)60,(byte)60,(byte)70,(byte)255));
            Program.DrawTextUI("?", iconX - 6, iconY - 12, 20, Color.DarkGray);
        }

        if (e.Discovered)
        {
            // name + location
            Program.DrawTextUI(e.Type, x + 60, ry + 4, 22, Color.White);
            Program.DrawTextUI($"Found: {e.Location}", x + 60, ry + 28, 14, new Color((byte)160,(byte)180,(byte)200,(byte)255));

            // stats
            Program.DrawTextUI($"HP: {e.HP}", x + 60, ry + 48, 13, Color.LightGray);
            Program.DrawTextUI($"XP: {e.CombatXP}", x + 130, ry + 48, 13, Color.LightGray);
            Program.DrawTextUI($"Kills: {e.Kills}", x + 210, ry + 48, 13, new Color((byte)255,(byte)100,(byte)100,(byte)255));

            // drops
            string dropStr = "";
            for (int d = 0; d < e.Drops.Length; d++)
            {
                if (d > 0) dropStr += ", ";
                dropStr += $"{e.Drops[d]} ({e.DropRates[d]})";
            }
            // truncate if too long
            if (Program.MeasureTextUI(dropStr, 12) > rw - 280)
                dropStr = dropStr.Substring(0, Math.Min(dropStr.Length, 60)) + "...";
            Program.DrawTextUI($"Drops: {dropStr}", x + 300, ry + 48, 12, new Color((byte)200,(byte)180,(byte)100,(byte)255));

            // kill count badge on right
            Raylib.DrawRectangle(x + rw - 80, ry + 8, 70, 30, new Color((byte)60,(byte)20,(byte)20,(byte)200));
            Raylib.DrawRectangleLinesEx(new Rectangle(x + rw - 80, ry + 8, 70, 30), 1, new Color((byte)200,(byte)60,(byte)60,(byte)255));
            string killText = e.Kills.ToString();
            int kw = Program.MeasureTextUI(killText, 20);
            Program.DrawTextUI(killText, x + rw - 80 + 35 - kw / 2, ry + 13, 20, Color.Red);
        }
        else
        {
            // unknown entry
            Program.DrawTextUI("???", x + 60, ry + 8, 22, Color.DarkGray);
            Program.DrawTextUI("Not yet encountered", x + 60, ry + 34, 15, new Color((byte)80,(byte)80,(byte)90,(byte)255));
            Program.DrawTextUI("Defeat this enemy to reveal its entry", x + 60, ry + 54, 13, new Color((byte)60,(byte)60,(byte)70,(byte)255));
        }
    }
    Raylib.EndScissorMode();

    if (maxScroll > 0)
        Program.DrawTextUI("Scroll to see more", x + w / 2 - 70, contentTop + contentH + 4, 14, Color.DarkGray);
}

static void DrawPMRelationships(int x, int y, int w)
{
    Program.DrawTextUI("RELATIONSHIPS", x, y, 34, Color.Gold);
    if (friendNPCs.Count == 0)
    {
        Program.DrawTextUI("No friends yet — talk to people around town!", x, y + 60, 20, Color.Gray);
        return;
    }

    Vector2 mouse = Raylib.GetMousePosition();
    int contentTop = y + 56;
    int contentH = ScreenHeight - contentTop - 40;
    int rowH = 80;
    int totalH = friendNPCs.Count * rowH;
    float maxScroll = Math.Max(0, totalH - contentH);

    if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(x, contentTop, w - 40, contentH)))
        pmRelScrollY = Math.Clamp(pmRelScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
    pmRelScrollY = Math.Clamp(pmRelScrollY, 0f, maxScroll);

    Program.DrawTextUI($"{friendNPCs.Count} friends", x + 300, y + 10, 18, Color.LightGray);

    Raylib.BeginScissorMode(x, contentTop, w - 40, contentH);
    for (int i = 0; i < friendNPCs.Count; i++)
    {
        var f = friendNPCs[i];
        int ry = contentTop + i * rowH - (int)pmRelScrollY;
        if (ry + rowH < contentTop || ry > contentTop + contentH) continue;

        Raylib.DrawRectangle(x, ry, w - 40, 64, new Color((byte)26,(byte)30,(byte)44,(byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(x, ry, w - 40, 64), 1, new Color((byte)60,(byte)66,(byte)90,(byte)255));
        string label = f.Name;
        if (f.Partner != "" && f.Partner != "someone" && f.Friendship >= 30)
            label += $" & {f.Partner}";
        else if (f.IsChild && f.Parents.Length > 0 && f.Friendship >= 30)
            label += $" (child of {f.Parents[0]} & {f.Parents[1]})";
        Program.DrawTextUI(label, x + 14, ry + 8, 20, Color.White);
        string tierLine = f.Friendship >= 30
            ? $"{f.Tier} — {f.Personality}"
            : f.Tier;
        Program.DrawTextUI(tierLine, x + 14, ry + 34, 14, Color.Gold);
        Raylib.DrawRectangle(x + 260, ry + 26, 240, 10, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(x + 260, ry + 26, (int)(240 * Math.Min(1f, f.Friendship / 100f)), 10, new Color((byte)220,(byte)90,(byte)120,(byte)255));
        Program.DrawTextUI($"{f.Friendship}/100", x + 510, ry + 22, 16, Color.LightGray);
    }
    Raylib.EndScissorMode();
}

static void DrawPMIdentity(int x, int y, int w)
{
    Program.DrawTextUI("IDENTITY", x, y, 34, Color.Gold);

    // ── ID CARD ──
    int cardX = x, cardY = y + 52, cardW = 440, cardH = 170;
    if (!idClaimed)
    {
        // empty placeholder — no ID until claimed from the mailbox
        Raylib.DrawRectangle(cardX, cardY, cardW, cardH, new Color((byte)22,(byte)24,(byte)32,(byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(cardX, cardY, cardW, cardH), 2, Color.DarkGray);
        string tag = "NO ID";
        int tw = Program.MeasureTextUI(tag, 40);
        Program.DrawTextUI(tag, cardX + cardW/2 - tw/2, cardY + cardH/2 - 34, 40, Color.Gray);

        // small status hint under the tag
        string hint = idPending   ? "Photo taken — awaiting delivery"
                    : idMailWaiting ? "Ready for pickup at your mailbox"
                    :                 "Visit the Library to get your ID";
        int hw = Program.MeasureTextUI(hint, 16);
        Program.DrawTextUI(hint, cardX + cardW/2 - hw/2, cardY + cardH/2 + 12, 16, Color.LightGray);
    }
    else
    {

    // card body + border
    Raylib.DrawRectangle(cardX, cardY, cardW, cardH, new Color((byte)28,(byte)34,(byte)52,(byte)255));
    Raylib.DrawRectangleLinesEx(new Rectangle(cardX, cardY, cardW, cardH), 2, Color.Gold);
    // header strip
    Raylib.DrawRectangle(cardX, cardY, cardW, 30, new Color((byte)40,(byte)48,(byte)72,(byte)255));
    Program.DrawTextUI("CITIZEN ID", cardX + 12, cardY + 7, 18, Color.Gold);
    Program.DrawTextUI("MAORI SIDE", cardX + cardW - 150, cardY + 9, 14, Color.LightGray);

    // photo box
    int photoX = cardX + 14, photoY = cardY + 42, photoW = 96, photoH = 96;
    Raylib.DrawRectangle(photoX, photoY, photoW, photoH, new Color((byte)18,(byte)20,(byte)30,(byte)255));
    Raylib.DrawRectangleLinesEx(new Rectangle(photoX, photoY, photoW, photoH), 1, Color.Gray);
    
    Raylib.BeginScissorMode(photoX, photoY, photoW, photoH);
    
    float sc = 2.6f;
    int ax = photoX + photoW/2 - (int)(20 * sc);
    int ay = photoY + (int)(photoH * 0.20f);   // was 0.14f — nudges the head down

    int HX(int off) => ax + (int)(off * sc);
    int HY(int off) => ay + (int)(off * sc);
    int SZ(int v)   => (int)(v * sc);

    // shoulders/shirt first (behind head), only the top sliver visible at box bottom
    Raylib.DrawRectangle(HX(8), HY(20), SZ(24), SZ(20), player.ShirtColor);
    // head
    Raylib.DrawRectangle(HX(9), HY(0), SZ(22), SZ(20), player.SkinColor);
    // eyes
    Raylib.DrawRectangle(HX(13), HY(7), SZ(3), SZ(3), Color.Black);
    Raylib.DrawRectangle(HX(24), HY(7), SZ(3), SZ(3), Color.Black);
    
    // facial hair
    Color fc = playerFacialHairColor;
    switch (playerFacialHair)
    {
        case "Stubble":
            Raylib.DrawRectangle(HX(13), HY(17), SZ(14), SZ(4), new Color(fc.R, fc.G, fc.B, (byte)140));
            break;
        case "Moustache":
            Raylib.DrawRectangle(HX(13), HY(16), SZ(14), SZ(3), fc);
            break;
        case "Goatee":
            Raylib.DrawRectangle(HX(13), HY(16), SZ(14), SZ(3), fc);
            Raylib.DrawRectangle(HX(16), HY(19), SZ(8),  SZ(5), fc);
            break;
        case "Full Beard":
            Raylib.DrawRectangle(HX(11), HY(16), SZ(18), SZ(3), fc);
            Raylib.DrawRectangle(HX(10), HY(19), SZ(20), SZ(6), fc);
            break;
    }
    // mouth
    Raylib.DrawRectangle(HX(15), HY(17), SZ(10), SZ(2), new Color((byte)150,(byte)80,(byte)80,(byte)255));

    // hair
    Color hc = playerHairColor;
    switch (playerHairStyle)
    {
        case "Mini fro":
            Raylib.DrawRectangle(HX(9), HY(-2), SZ(22), SZ(6), hc);
            break;
        case "High top":
            Raylib.DrawRectangle(HX(8), HY(2), SZ(24), SZ(8), hc);
            Raylib.DrawRectangle(HX(6), HY(8), SZ(6), SZ(14), hc);
            Raylib.DrawRectangle(HX(28), HY(8), SZ(6), SZ(14), hc);
            break;
        case "Flat top":
            Raylib.DrawRectangle(HX(8), HY(-2), SZ(24), SZ(7), hc);
            Raylib.DrawRectangle(HX(8), HY(4), SZ(8), SZ(10), hc);
            Raylib.DrawRectangle(HX(24), HY(4), SZ(8), SZ(10), hc);
            break;
        case "Mohawk":
            Raylib.DrawRectangle(HX(17), HY(-8), SZ(6), SZ(14), hc);
            break;
        case "Bald":
            break;
    }
    Raylib.EndScissorMode();

    // card text fields
    int fldX = photoX + photoW + 20, fld = cardY + 44;
    void CardLine(string k, string v)
    {
        Program.DrawTextUI(k, fldX, fld, 14, Color.Gray);
        Program.DrawTextUI(v, fldX + 90, fld, 16, Color.White);
        fld += 24;
    }
    CardLine("NAME", playerName ?? "—");
    CardLine("HEALTH", $"{player.Health}/{player.MaxHealth}");
    CardLine("FUNDS", $"${player.Money}");
    CardLine("ISSUED", idClaimed && idIssuedDate != "" ? idIssuedDate : "—");
    CardLine("STATUS", idClaimed ? "VERIFIED" : idPending ? "PENDING" : idMailWaiting ? "IN MAILBOX" : "UNVERIFIED");

    // ── STATS BELOW THE CARD ──
    int ly = cardY + cardH + 24;
    void Line(string k, string v)
    {
        Program.DrawTextUI(k, x, ly, 20, Color.LightGray);
        Program.DrawTextUI(v, x + 300, ly, 20, Color.White);
        ly += 30;
    }

    int hrs = (int)(totalPlayTime / 3600), mins = (int)((totalPlayTime % 3600) / 60);

    string topName = "—"; int topLv = 0;
    foreach (var s in cheatSkills)
        if (s.get() > topLv) { topLv = s.get(); topName = s.name; }

    string MostPlayed(Dictionary<string,int> d)
    {
        string best = "None yet"; int bestC = 0;
        foreach (var kv in d) if (kv.Value > bestC) { bestC = kv.Value; best = kv.Key; }
        return bestC > 0 ? $"{best} ({bestC})" : best;
    }

    Line("Time Played:", $"{hrs}h {mins}m");
    Line("Date:", $"{dayNames[dayOfWeek]}, {GetMonthString()} {dayOfMonth}");
    Line("Highest Skill:", $"{topName} (Lv {topLv})");
    Line("Most Played Sport:", MostPlayed(sportPlayCounts));
    Line("Most Played Minigame:", MostPlayed(minigamePlayCounts));
    Line("Dropzone Tickets:", player.Tickets.ToString());
    Line("Land Plots Owned:", ownedHousePlots.Count.ToString());
    Line("Dungeons Cleared:", dungeonsCleared.ToString());
    Line("World Explored:", $"{GetExplorationPercent():F1}%");
    Line("Achievements:", $"{achievementsUnlockedCount}/{achievements.Count}");
    Line("Reputation:", $"{player.Reputation} ({GetReputationTier(player.Reputation).title})");
    if (timesCheated > 0)
    Line("Times Cheated:", timesCheated.ToString());

    int colX = x + w - 340;   
    int colY = y + 56;
    Program.DrawTextUI("CARDS", colX, colY, 22, Color.Gold);
    colY += 30;

    // DropZone card — coloured by tier
    if (hasDropzoneCard)
{
    Color dzCol = dropzoneTier switch
    {
        0 => new Color((byte)200,(byte)50,(byte)50,(byte)255),
        1 => new Color((byte)60,(byte)120,(byte)220,(byte)255),
        2 => new Color((byte)220,(byte)180,(byte)60,(byte)255),
        3 => new Color((byte)200,(byte)200,(byte)220,(byte)255),
        _ => new Color((byte)30,(byte)30,(byte)40,(byte)255),
    };
    int dzW = 280, dzH = 130;
    Raylib.DrawRectangle(colX, colY, dzW, dzH, dzCol);
    Raylib.DrawRectangleLinesEx(new Rectangle(colX, colY, dzW, dzH), 2, Color.Gold);
    // header strip
    Raylib.DrawRectangle(colX, colY, dzW, 22, new Color((byte)0,(byte)0,(byte)0,(byte)90));
    Program.DrawTextUI("DROPZONE MEMBERSHIP", colX + 10, colY + 4, 14, Color.White);
    // tier + card number
    Program.DrawTextUI(dropzoneTierNames[dropzoneTier].ToUpper(), colX + 10, colY + 30, 22, Color.White);
    Program.DrawTextUI($"NO. {(1000 + dropzoneTier * 111):D4}", colX + dzW - 90, colY + 34, 12, new Color((byte)255,(byte)255,(byte)255,(byte)160));
    // balance + spend
    Program.DrawTextUI($"Balance: ${dropzoneCredit:0.00}", colX + 10, colY + 58, 14, Color.White);
    Program.DrawTextUI($"Lifetime spend: ${dropzoneLifetimeSpend:0.00}", colX + 10, colY + 76, 12, new Color((byte)255,(byte)255,(byte)255,(byte)200));
    // next-tier progress bar
    if (dropzoneTier < dropzoneTierThresholds.Length)
    {
        float need = dropzoneTierThresholds[dropzoneTier];
        float prog = Math.Clamp(dropzoneLifetimeSpend / need, 0f, 1f);
        Raylib.DrawRectangle(colX + dzW - 90, colY + 58, 80, 6, new Color((byte)0,(byte)0,(byte)0,(byte)120));
        Raylib.DrawRectangle(colX + dzW - 90, colY + 58, (int)(80 * prog), 6, Color.White);
    }
    colY += dzH + 16;
}

 string[] classes = { "D", "C", "B", "A", "S" };
    bool[] restricted = { hasTheoryD, hasTheoryC, hasTheoryB, hasTheoryA, hasTheoryS };
    bool[] full       = { hasPracticalD, hasPracticalC, hasPracticalB, hasPracticalA, hasPracticalS };

    // REPLACED — old stacking loop deleted; show only the top tier held
    int topIdx = -1; bool topIsFull = false;
    for (int i = classes.Length - 1; i >= 0; i--)
        if (full[i] || restricted[i]) { topIdx = i; topIsFull = full[i]; break; }

    if (topIdx >= 0)
    {
        int lcW = 280, lcH = 120;
        bool thisDelivered = topIsFull ? licencePracticalDelivered[topIdx] : licenceTheoryDelivered[topIdx];
        if (!thisDelivered)
        {
            Raylib.DrawRectangle(colX, colY, lcW, lcH, new Color((byte)235,(byte)230,(byte)210,(byte)255));
            Raylib.DrawRectangleLinesEx(new Rectangle(colX, colY, lcW, lcH), 2, new Color((byte)150,(byte)140,(byte)110,(byte)255));
            Program.DrawTextUI($"LICENCE {classes[topIdx]} PENDING", colX + 10, colY + 8, 16, new Color((byte)90,(byte)80,(byte)60,(byte)255));
            string note = licenceMailWaiting[topIdx] ? "Ready for pickup at your mailbox"
                        : licencePending[topIdx]     ? "Awaiting delivery"
                        :                              "Passed — awaiting paperwork";
            Program.DrawTextUI(note, colX + 10, colY + 34, 13, new Color((byte)120,(byte)110,(byte)90,(byte)255));
        }
        else
        {
            Color lc = topIsFull ? new Color((byte)70,(byte)170,(byte)90,(byte)255)
                                 : new Color((byte)220,(byte)200,(byte)60,(byte)255);
            Raylib.DrawRectangle(colX, colY, lcW, lcH, new Color((byte)250,(byte)248,(byte)240,(byte)255));
            Raylib.DrawRectangleLinesEx(new Rectangle(colX, colY, lcW, lcH), 3, lc);
            Raylib.DrawRectangle(colX, colY, lcW, 24, lc);
            Program.DrawTextUI($"CLASS {classes[topIdx]} LICENCE", colX + 10, colY + 4, 16, Color.Black);
            Program.DrawTextUI(topIsFull ? "FULL" : "RESTRICTED", colX + 10, colY + 32, 20, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Program.DrawTextUI($"Holder: {playerName}", colX + 10, colY + 56, 16, new Color((byte)80,(byte)80,(byte)80,(byte)255));
            Program.DrawTextUI($"Class {classes[topIdx]}  •  {(topIsFull ? "Unrestricted hours" : "6am-10pm only")}", colX + 10, colY + 72, 16, new Color((byte)100,(byte)100,(byte)100,(byte)255));
        }
        colY += lcH + 12;
    }

    // ADDED — boat licence card, own slot, highest tier only
    int topBoat = -1;
    for (int b = hasBoatPractical.Length - 1; b >= 0; b--)
        if (hasBoatPractical[b]) { topBoat = b; break; }
    if (topBoat >= 0)
    {
        int lcW = 280, lcH = 120;
        Color bc = new Color((byte)60,(byte)130,(byte)200,(byte)255);
        Raylib.DrawRectangle(colX, colY, lcW, lcH, new Color((byte)240,(byte)246,(byte)252,(byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(colX, colY, lcW, lcH), 3, bc);
        Raylib.DrawRectangle(colX, colY, lcW, 24, bc);
        Program.DrawTextUI($"BOAT LICENCE — {boatTierNames[topBoat].ToUpper()}", colX + 10, colY + 4, 16, Color.White);
        Program.DrawTextUI($"Holder: {playerName}", colX + 10, colY + 44, 16, new Color((byte)80,(byte)80,(byte)80,(byte)255));
        Program.DrawTextUI("Maritime NZ  •  All coastal waters", colX + 10, colY + 68, 14, new Color((byte)100,(byte)100,(byte)100,(byte)255));
        colY += lcH + 12;
    }
}
}

static void DrawPMStats(int x, int y, int w)
{
    Program.DrawTextUI("SKILLS", x, y, 34, Color.Gold);
    int col0 = x, col1 = x + w/2;
    int ry = y + 60, half = (cheatSkills.Length + 1) / 2;
    for (int i = 0; i < cheatSkills.Length; i++)
    {
        var s = cheatSkills[i];
        int colX = i < half ? col0 : col1;
        int rowY = y + 60 + (i % half) * 40;
        int lv = s.get();
        Program.DrawTextUI(s.name, colX, rowY, 20, Color.White);
        Program.DrawTextUI($"Lv {lv}", colX + 200, rowY, 20, Color.Gold);
        // mini level bar
        Raylib.DrawRectangle(colX, rowY + 24, 260, 6, new Color((byte)40,(byte)40,(byte)40,(byte)255));
        Raylib.DrawRectangle(colX, rowY + 24, (int)(260 * Math.Min(1f, lv / 100f)), 6, Color.Gold);
    }
}

static void DrawPMUnlocks(int x, int y, int w)
{
    Program.DrawTextUI("LICENCES & THEORIES", x, y, 34, Color.Gold);

    Vector2 mouse = Raylib.GetMousePosition();
    int contentTop = y + 50;
    int contentH = ScreenHeight - contentTop - 40;

    // calculate total height: 10 licence rows + gap + synergy header + synergy rows
    int licenceRows = 10;
    int synergyHeaderH = 64;
    int synergyRowH = 78;
    int totalH = licenceRows * 30 + 10 + synergyHeaderH + synergies.Length * synergyRowH + 40;
    float maxScroll = Math.Max(0, totalH - contentH);

    if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(x, contentTop, w - 40, contentH)))
        pmUnlScrollY = Math.Clamp(pmUnlScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, maxScroll);
    pmUnlScrollY = Math.Clamp(pmUnlScrollY, 0f, maxScroll);

    Raylib.BeginScissorMode(x, contentTop, w - 40, contentH);

    int ly = contentTop - (int)pmUnlScrollY;
    void L(string label, bool have)
    {
        Program.DrawTextUI(label, x, ly, 20, Color.White);
        Program.DrawTextUI(have ? "OWNED" : "—", x + 300, ly, 20, have ? Color.Green : Color.Gray);
        ly += 30;
    }
    L("Theory D", hasTheoryD); L("Theory C", hasTheoryC); L("Theory B", hasTheoryB);
    L("Theory A", hasTheoryA); L("Theory S", hasTheoryS);
    ly += 10;
    L("Practical D", hasPracticalD); L("Practical C", hasPracticalC); L("Practical B", hasPracticalB);
    L("Practical A", hasPracticalA); L("Practical S", hasPracticalS);

    ly += 20;
    Program.DrawTextUI("SKILL SYNERGIES", x, ly, 34, Color.Gold);
    int unlocked = synergies.Count(s => HasSynergy(s.Name));
    Program.DrawTextUI($"{unlocked}/{synergies.Length} unlocked", x + 300, ly + 8, 18, Color.White);
    ly += 44;

    foreach (var s in synergies)
    {
        bool have = HasSynergy(s.Name);
        Raylib.DrawRectangle(x, ly, w - 40, 70, new Color((byte)20,(byte)22,(byte)32,(byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(x, ly, w - 40, 70), 1,
            have ? s.TierColor : new Color((byte)50,(byte)50,(byte)60,(byte)255));

        Raylib.DrawCircle(x + 18, ly + 22, 8, have ? s.TierColor : new Color((byte)60,(byte)60,(byte)70,(byte)255));

        Program.DrawTextUI(s.Name, x + 36, ly + 6, 20, have ? Color.White : Color.Gray);
        Program.DrawTextUI(s.Description, x + 36, ly + 28, 15,
            have ? new Color((byte)180,(byte)220,(byte)140,(byte)255) : new Color((byte)80,(byte)80,(byte)90,(byte)255));

        string reqStr = string.Join(" + ", s.Requirements.Select(r =>
        {
            int cur = GetSkillLevel(r.skill);
            return $"{r.skill} {cur}/{r.level}";
        }));
        Program.DrawTextUI(reqStr, x + 36, ly + 48, 13,
            have ? new Color((byte)100,(byte)160,(byte)100,(byte)200) : new Color((byte)120,(byte)100,(byte)60,(byte)200));

        string badge = have ? "ACTIVE" : "LOCKED";
        Color badgeCol = have ? new Color((byte)80,(byte)200,(byte)80,(byte)255) : new Color((byte)100,(byte)100,(byte)110,(byte)255);
        Program.DrawTextUI(badge, x + w - 120, ly + 10, 16, badgeCol);

        ly += 78;
    }

    Raylib.EndScissorMode();
}

static void DrawIdentityCollectionPage(int x, int y, int w)
{
    int viewH = 540;                     // visible window height inside the menu
    idColScrollY = Math.Clamp(idColScrollY - Raylib.GetMouseWheelMove() * 40f, 0f, 4000f);
    Raylib.BeginScissorMode(x, y, w, viewH);
    int ly = y - (int)idColScrollY;

    // one slot: silhouette box, revealed when owned
    void Slot(int sx, int sy, int sw, int sh, string label, bool owned, Color revealCol)
    {
        Raylib.DrawRectangle(sx, sy, sw, sh, new Color((byte)18,(byte)20,(byte)28,(byte)255));
        Raylib.DrawRectangleLinesEx(new Rectangle(sx, sy, sw, sh), 1, owned ? Color.Gold : new Color((byte)60,(byte)60,(byte)70,(byte)255));
        // shadowed silhouette blob
        Raylib.DrawRectangle(sx + sw/2 - 14, sy + 10, 28, sh - 38, owned ? revealCol : new Color((byte)0,(byte)0,(byte)0,(byte)170));
        string tag = owned ? label : "???";
        int tw2 = Program.MeasureTextUI(tag, 12);
        Program.DrawTextUI(tag, sx + sw/2 - tw2/2, sy + sh - 18, 12, owned ? Color.White : Color.DarkGray);
    }

    int slotW = 84, slotH = 92, perRow = Math.Max(1, w / (slotW + 10));
    int GridSection(string title, string[] names, Func<string,bool> ownedFn, Color col, int startY)
    {
        Program.DrawTextUI(title, x, startY, 24, Color.Gold);
        int gy = startY + 34;
        for (int i = 0; i < names.Length; i++)
            Slot(x + (i % perRow) * (slotW + 10), gy + (i / perRow) * (slotH + 10),
                 slotW, slotH, names[i], ownedFn(names[i]), col);
        return gy + ((names.Length - 1) / perRow + 1) * (slotH + 10) + 20;
    }

    ly = GridSection($"PLUSHIES ({plushiesOwned.Count}/50)", plushCommon, n => plushiesOwned.ContainsKey(n), new Color((byte)200,(byte)150,(byte)100,(byte)255), ly);
    ly = GridSection("RARE PLUSHIES", plushRare, n => plushiesOwned.ContainsKey(n), new Color((byte)180,(byte)120,(byte)220,(byte)255), ly);
    ly = GridSection("SUPER RARE", plushSuperRare, n => plushiesOwned.ContainsKey(n), Color.Gold, ly);
    ly = GridSection($"HIDDEN COLLECTABLES ({CollectablesFound}/{collectables.Count})",
                     collectables.Select(c => c.Name).ToArray(),
                     n => collectables.First(c => c.Name == n).Found, Color.SkyBlue, ly);

    // licence silhouettes — wider card-shaped slots
    Program.DrawTextUI("LICENCES", x, ly, 24, Color.Gold); ly += 34;
    string[] classes = { "D", "C", "B", "A", "S" };
    bool[] full = { hasPracticalD, hasPracticalC, hasPracticalB, hasPracticalA, hasPracticalS };
    for (int i = 0; i < 5; i++)
        Slot(x + i * 150, ly, 140, 80, $"Class {classes[i]}", full[i], new Color((byte)70,(byte)170,(byte)90,(byte)255));
    ly += 100;
    for (int b = 0; b < boatTierNames.Length; b++)
        Slot(x + b * 150, ly, 140, 80, boatTierNames[b], hasBoatPractical[b], new Color((byte)60,(byte)130,(byte)200,(byte)255));

    Raylib.EndScissorMode();
}

static int collectablesScrollOffset = 0;
static bool viewingMasterSet = false;
static void DrawPMCollectables(int cx, int cy, int cw)
{
    var activeSet = cardSets[activeCardSetIndex];
    var masterSetBinder = activeSet.MasterSet;   
    var personalBinder = activeSet.Personal;     
    var allCards = activeSet.Pool;               
    var binderSlots = activeSet.Slots;           

    Program.DrawTextUI($"COLLECTABLES  {activeSet.CoverTitle}", cx, cy, 26, Color.Gold);   
    Vector2 mouse = Raylib.GetMousePosition();

    for (int i = 0; i < cardSets.Count; i++)
    {
        Rectangle setBtn = new Rectangle(cx + i * 180, cy + 40, 170, 46);   
        bool hovSet = Raylib.CheckCollisionPointRec(mouse, setBtn);
        Raylib.DrawRectangleRec(setBtn, activeCardSetIndex == i ? cardSets[i].CoverColor : new Color((byte)30,(byte)33,(byte)48,(byte)255));
        Raylib.DrawRectangleLinesEx(setBtn, 3, activeCardSetIndex == i ? Color.Gold : Color.Gray);
        Program.DrawTextUI(cardSets[i].CoverTitle, (int)setBtn.X + 12, (int)setBtn.Y + 12, 18, Color.White);   
        if (hovSet && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            activeCardSetIndex = i;
            binderOpen = false;
            binderPageIndex = 0;
        }
    }
    
    Rectangle toggleBtn = new Rectangle(cx + cardSets.Count * 180 + 20, cy + 40, 240, 46);   
    bool hovToggle = Raylib.CheckCollisionPointRec(mouse, toggleBtn);
    Raylib.DrawRectangleRec(toggleBtn, hovToggle ? new Color((byte)50,(byte)55,(byte)80,(byte)255) : new Color((byte)30,(byte)33,(byte)48,(byte)255));
    Raylib.DrawRectangleLinesEx(toggleBtn, 3, hovToggle ? Color.Gold : Color.Gray);
    Program.DrawTextUI("Switch Binder", (int)toggleBtn.X + 36, (int)toggleBtn.Y + 12, 20, Color.White); 
    if (hovToggle && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        viewingMasterSet = !viewingMasterSet;
        binderOpen = false;
        binderPageIndex = 0;
    }
    
    var binder = viewingMasterSet ? masterSetBinder : personalBinder;
    int totalOwned = viewingMasterSet
    ? binder.Cards.Values.Sum() + binder.ReverseHoloCards.Values.Sum()
    : personalBinder.SlotAssignments.Values.Sum(d => d.Count);
    int uniqueOwned = viewingMasterSet
        ? binder.Cards.Keys.Count + binder.ReverseHoloCards.Keys.Count
        : personalBinder.SlotAssignments.Values.Select(d => (d.Name, d.IsReverse)).Distinct().Count();
    Program.DrawTextUI($"Unique: {uniqueOwned} / {TotalBinderSlots()}   Total cards: {totalOwned}", cx, cy + 90, 20, Color.LightGray);
    Program.DrawTextUI($"Packs opened: {activeSet.PacksOpened}", cx + cardSets.Count * 180 + 20, cy + 90, 20, Color.LightGray);

    if (pageFlipTimer > 0f)
    {
        pageFlipTimer -= Raylib.GetFrameTime();
        if (pageFlipTimer <= 0f) pageFlipDir = 0;
    }

    if (!binderOpen)
    {
        // ── CLOSED BINDER COVER ──
        int coverW = 280, coverH = 360;   
        int coverX = cx + cw/2 - coverW/2, coverY = cy + 140;
        Rectangle coverRect = new Rectangle(coverX, coverY, coverW, coverH);
        bool hovCover = Raylib.CheckCollisionPointRec(mouse, coverRect);

        Raylib.DrawRectangle(coverX + 6, coverY + 6, coverW, coverH, new Color((byte)0,(byte)0,(byte)0,(byte)120)); // shadow
        Raylib.DrawRectangle(coverX, coverY, coverW, coverH, new Color((byte)40,(byte)30,(byte)70,(byte)255));
        Raylib.DrawRectangleLinesEx(coverRect, hovCover ? 4 : 3, hovCover ? Color.Gold : new Color((byte)80,(byte)65,(byte)140,(byte)255));
        Raylib.DrawRectangle(coverX + 20, coverY + 20, coverW - 40, coverH - 40, new Color((byte)55,(byte)42,(byte)95,(byte)255));

        string title = "PALMS OFF";
        int tw = Program.MeasureTextUI(title, 30);
        Program.DrawTextUI(title, coverX + coverW/2 - tw/2, coverY + coverH/2 - 30, 30, Color.Gold);
        string sub = viewingMasterSet ? "MASTER SET" : "PERSONAL COLLECTION";
        int sw = Program.MeasureTextUI(sub, 16);
        Program.DrawTextUI(sub, coverX + coverW/2 - sw/2, coverY + coverH/2 + 10, 16, Color.LightGray);

        Program.DrawTextUI("Click to open", coverX + coverW/2 - 80, coverY + coverH + 25, 30, Color.LightGray);

        if (hovCover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            binderOpen = true;
            binderPageIndex = 0;
        }
        return;
    }

    // ── OPEN BINDER ──
    int gridCols = 3, cellW = 120, cellH = 150;   
    int maxPage = MaxBinderPage();

    void DrawCard(int px, int py, BinderSlot slot, int slotIndex)
{
    var card = slot.Card;
    int owned = slot.IsReverse ? binder.ReverseHoloCards.GetValueOrDefault(card.Name) : binder.Cards.GetValueOrDefault(card.Name);
    bool has = owned > 0;

    Rectangle cardRect = new Rectangle(px, py, cellW - 8, cellH - 8);
    bool hover = has && Raylib.CheckCollisionPointRec(mouse, cardRect);
    if (hover) hoveredCardSlot = slotIndex;

    int popOffset = hover ? -6 : 0;
    int popGrow = hover ? 6 : 0;
    Rectangle drawRect = new Rectangle(px - popGrow/2, py + popOffset - popGrow/2, cellW - 8 + popGrow, cellH - 8 + popGrow);

    Color rarityCol = card.Rarity switch
    {
        CardRarity.SecretRare => new Color((byte)255,(byte)80,(byte)220,(byte)255),
        CardRarity.UltraRare => new Color((byte)255,(byte)140,(byte)30,(byte)255),
        CardRarity.Holo => new Color((byte)230,(byte)190,(byte)60,(byte)255),
        CardRarity.Rare => new Color((byte)160,(byte)90,(byte)220,(byte)255),
        CardRarity.Uncommon => new Color((byte)90,(byte)160,(byte)220,(byte)255),
        CardRarity.Trainer => new Color((byte)90,(byte)200,(byte)90,(byte)255),
        CardRarity.Energy => new Color((byte)220,(byte)140,(byte)60,(byte)255),
        _ => Color.LightGray
    };

    if (hover) Raylib.DrawRectangle((int)drawRect.X + 3, (int)drawRect.Y + 5, (int)drawRect.Width, (int)drawRect.Height, new Color((byte)0,(byte)0,(byte)0,(byte)140)); // shadow
    Raylib.DrawRectangleRec(drawRect, has ? rarityCol : new Color((byte)25,(byte)25,(byte)30,(byte)255));
    Raylib.DrawRectangleLinesEx(drawRect, hover ? 3 : 1, hover ? Color.Gold : (has ? Color.Black : new Color((byte)50,(byte)50,(byte)55,(byte)255)));
    string label = has ? (slot.IsReverse ? $"{card.Name} (REV)" : card.Name) : "???";
    DrawWrappedCardText(label, (int)drawRect.X + 6, (int)(drawRect.Y + drawRect.Height) - 72, (int)drawRect.Width - 12, 14, has ? Color.Black : Color.DarkGray);
    if (has && card.Power > 0)
        Program.DrawTextUI($"PL {card.Power}", (int)drawRect.X + 6, (int)drawRect.Y + 8, 14, Color.Black);   // top-left
    Program.DrawTextUI($"x{owned}", (int)(drawRect.X + drawRect.Width) - 44, (int)drawRect.Y + 8, 16, Color.Black); 

    if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        cardPopupOpen = (slotIndex, card.Name, slot.IsReverse);
}

void DrawPersonalSlot(int px, int py, int slotIndex)
{
    bool hasAssignment = personalBinder.SlotAssignments.TryGetValue(slotIndex, out var data);
    Rectangle cardRect = new Rectangle(px, py, cellW - 8, cellH - 8);
    bool hover = Raylib.CheckCollisionPointRec(mouse, cardRect);

    int popOffset = hover ? -6 : 0;
    int popGrow = hover ? 6 : 0;
    Rectangle drawRect = new Rectangle(px - popGrow/2, py + popOffset - popGrow/2, cellW - 8 + popGrow, cellH - 8 + popGrow);

    if (hasAssignment)
    {
        var cardDef = allCards.FirstOrDefault(c => c.Name == data.Name);
        Color rarityCol = cardDef?.Rarity switch
        {
            CardRarity.SecretRare => new Color((byte)255,(byte)80,(byte)220,(byte)255),
            CardRarity.UltraRare => new Color((byte)255,(byte)140,(byte)30,(byte)255),
            CardRarity.Holo => new Color((byte)230,(byte)190,(byte)60,(byte)255),
            CardRarity.Rare => new Color((byte)160,(byte)90,(byte)220,(byte)255),
            CardRarity.Uncommon => new Color((byte)90,(byte)160,(byte)220,(byte)255),
            CardRarity.Trainer => new Color((byte)90,(byte)200,(byte)90,(byte)255),
            CardRarity.Energy => new Color((byte)220,(byte)140,(byte)60,(byte)255),
            _ => Color.LightGray
        };
        bool isHeld = heldSingleCardSlot == slotIndex;
        if (hover) Raylib.DrawRectangle((int)drawRect.X + 3, (int)drawRect.Y + 5, (int)drawRect.Width, (int)drawRect.Height, new Color((byte)0,(byte)0,(byte)0,(byte)140));
        Raylib.DrawRectangleRec(drawRect, rarityCol);
        Raylib.DrawRectangleLinesEx(drawRect, (hover || isHeld) ? 3 : 1, isHeld ? Color.Blue : (hover ? Color.Gold : Color.Black));
        string label = data.IsReverse ? $"{data.Name} (REV)" : data.Name;
        Program.DrawTextUI(label, (int)drawRect.X + 6, (int)(drawRect.Y + drawRect.Height) - 60, 16, Color.Black);
        Program.DrawTextUI($"x{data.Count}", (int)drawRect.X + 6, (int)drawRect.Y + 8, 20, Color.Black);

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (placementModeActive && data.Name == placementCardName && data.IsReverse == placementIsReverse)
    {
        data.Count++;
        placementModeActive = false;
        placementCardName = null;
        ShowNotification($"Added another {data.Name} to that slot!");
    }
    else if (heldSingleCardSlot >= 0 && heldSingleCardSlot != slotIndex)
    {
        // completing a Move onto another occupied slot — only valid if same card, merges 1 card in
        var heldData = personalBinder.SlotAssignments[heldSingleCardSlot];
        if (heldData.Name == data.Name && heldData.IsReverse == data.IsReverse)
        {
            data.Count++;
            heldData.Count--;
            if (heldData.Count <= 0) personalBinder.SlotAssignments.Remove(heldSingleCardSlot);
        }
        heldSingleCardSlot = -1;
    }
    else if (!placementModeActive && heldSingleCardSlot < 0)
    {
        cardContextMenuSlot = slotIndex;   // CHANGED — was: rearrangeHeldSlot = slotIndex;
    }
}
    }
    else
    {
        bool canPlaceHere = placementModeActive || heldSingleCardSlot >= 0;
        Color emptyCol = (canPlaceHere && hover) ? new Color((byte)70,(byte)90,(byte)50,(byte)255) : new Color((byte)25,(byte)25,(byte)30,(byte)255);
        Raylib.DrawRectangleRec(drawRect, emptyCol);
        Raylib.DrawRectangleLinesEx(drawRect, (canPlaceHere && hover) ? 3 : 1,
            (canPlaceHere && hover) ? Color.Green : new Color((byte)50,(byte)50,(byte)55,(byte)255));

        if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (placementModeActive)
            {
                personalBinder.SlotAssignments[slotIndex] = new BinderSlotData { Name = placementCardName, IsReverse = placementIsReverse, Count = 1 };
                placementModeActive = false;
                placementCardName = null;
                ShowNotification("Card placed in your Personal Collection!");
            }
            else if (heldSingleCardSlot >= 0)   // CHANGED — was: rearrangeHeldSlot >= 0
            {
                var heldData = personalBinder.SlotAssignments[heldSingleCardSlot];
                personalBinder.SlotAssignments[slotIndex] = new BinderSlotData { Name = heldData.Name, IsReverse = heldData.IsReverse, Count = 1 };
                heldData.Count--;
                if (heldData.Count <= 0) personalBinder.SlotAssignments.Remove(heldSingleCardSlot);
                heldSingleCardSlot = -1;
                ShowNotification("Card moved!");
            }
        }
    }
}

void DrawPageBlock(int blockX, int blockY, int startIdx)
{
    Raylib.DrawRectangle(blockX - 10, blockY - 10, gridCols * cellW + 20, gridCols * cellH + 20, new Color((byte)45,(byte)35,(byte)75,(byte)255));
    Raylib.DrawRectangleLines(blockX - 10, blockY - 10, gridCols * cellW + 20, gridCols * cellH + 20, new Color((byte)80,(byte)65,(byte)140,(byte)255));
    for (int i = 0; i < 9; i++)
    {
        int idx = startIdx + i;
        int col = i % gridCols, row = i / gridCols;
        if (viewingMasterSet)
        {
            if (idx >= binderSlots.Count) continue;
            DrawCard(blockX + col * cellW, blockY + row * cellH, binderSlots[idx], idx);
        }
        else
        {
            if (idx >= TotalBinderSlots()) continue;
            DrawPersonalSlot(blockX + col * cellW, blockY + row * cellH, idx);
        }
    }
}

    int flipOffset = pageFlipDir != 0 ? (int)(pageFlipDir * pageFlipTimer / 0.25f * 60) : 0;

    if (binderPageIndex == 0)
    {
        var (start, _) = GetSinglePageRange();
        int blockX = cx + cw/2 - (gridCols * cellW)/2 + flipOffset;
        DrawPageBlock(blockX, cy + 140, start);  
    }
    else
    {
        var (leftStart, rightStart) = GetSpreadRanges(binderPageIndex);
        int leftX = cx + cw/2 - gridCols * cellW - 25 + flipOffset;    
        int rightX = cx + cw/2 + 25 + flipOffset;                      
        DrawPageBlock(leftX, cy + 140, leftStart);
        DrawPageBlock(rightX, cy + 140, rightStart);
        Raylib.DrawRectangle(cx + cw/2 - 4, cy + 120, 8, gridCols * cellH + 30, new Color((byte)25,(byte)18,(byte)50,(byte)255));
}

    string pageLabel = binderPageIndex == 0 ? "Page 1" : $"Pages {binderPageIndex * 2}-{binderPageIndex * 2 + 1}";
    int plw = Program.MeasureTextUI(pageLabel, 30);
    Program.DrawTextUI(pageLabel, cx + cw/2 - plw/2, cy + 140 + gridCols * cellH + 40, 30, Color.LightGray);

    // page navigation arrows
    Rectangle leftArrow = new Rectangle(cx + 20, cy + 400, 60, 60);    
    Rectangle rightArrow = new Rectangle(cx + cw - 80, cy + 400, 60, 60);
    bool hovLeft = binderPageIndex > 0 && Raylib.CheckCollisionPointRec(mouse, leftArrow);
    bool hovRight = binderPageIndex < maxPage && Raylib.CheckCollisionPointRec(mouse, rightArrow);

    Raylib.DrawRectangleRec(leftArrow, hovLeft ? new Color((byte)60,(byte)45,(byte)110,(byte)255) : new Color((byte)35,(byte)27,(byte)65,(byte)255));
    Program.DrawTextUI("<", (int)leftArrow.X + 18, (int)leftArrow.Y + 10, 28, binderPageIndex > 0 ? Color.White : Color.DarkGray);
    Raylib.DrawRectangleRec(rightArrow, hovRight ? new Color((byte)60,(byte)45,(byte)110,(byte)255) : new Color((byte)35,(byte)27,(byte)65,(byte)255));
    Program.DrawTextUI(">", (int)rightArrow.X + 18, (int)rightArrow.Y + 10, 28, binderPageIndex < maxPage ? Color.White : Color.DarkGray);

 if (hovLeft && Raylib.IsMouseButtonPressed(MouseButton.Left) && pageFlipDir == 0)
    {
        binderPageIndex--;
        pageFlipDir = -1;
        pageFlipTimer = 0.25f;
    }
    if (hovRight && Raylib.IsMouseButtonPressed(MouseButton.Left) && pageFlipDir == 0)
    {
        binderPageIndex++;
        pageFlipDir = 1;
        pageFlipTimer = 0.25f;
    }
   
if (cardPopupOpen.slot >= 0 && !viewingMasterSet == false)   
{
    int popX = cx + cw/2 - 140, popY = cy + 450;
    Raylib.DrawRectangle(popX, popY, 280, 80, new Color((byte)20,(byte)20,(byte)30,(byte)250));
    Raylib.DrawRectangleLines(popX, popY, 280, 80, Color.Gold);
    Program.DrawTextUI(cardPopupOpen.cardName, popX + 10, popY + 8, 16, Color.White);

    Rectangle addBtn = new Rectangle(popX + 10, popY + 34, 180, 34);
    bool hovAdd = Raylib.CheckCollisionPointRec(mouse, addBtn);
    Raylib.DrawRectangleRec(addBtn, hovAdd ? new Color((byte)50,(byte)55,(byte)80,(byte)255) : new Color((byte)30,(byte)33,(byte)48,(byte)255));
    Program.DrawTextUI("Add to Personal Collection", (int)addBtn.X + 8, (int)addBtn.Y + 8, 13, Color.White);

    if (hovAdd && Raylib.IsMouseButtonPressed(MouseButton.Left))
{
    if (masterSetBinder.RemoveOne(cardPopupOpen.cardName, cardPopupOpen.isReverse))
    {
        placementModeActive = true;
        placementCardName = cardPopupOpen.cardName;
        placementIsReverse = cardPopupOpen.isReverse;
        viewingMasterSet = false;
        binderPageIndex = 0;
        ShowNotification("Pick a slot — click empty for a new stack, or click a matching card to add a duplicate there.");
    }
    cardPopupOpen = (-1, null, false);
}

    Rectangle cancelX = new Rectangle(popX + 240, popY + 4, 30, 24);
    if (Raylib.CheckCollisionPointRec(mouse, cancelX) && Raylib.IsMouseButtonPressed(MouseButton.Left))
        cardPopupOpen = (-1, null, false);
    Program.DrawTextUI("X", (int)cancelX.X + 8, (int)cancelX.Y + 2, 18, Color.Red);
}

// DrawPMCollectables() — ADD, near the master-set transfer popup — the new Move/Delete/Switch context menu
if (cardContextMenuSlot >= 0 && personalBinder.SlotAssignments.TryGetValue(cardContextMenuSlot, out var menuData))
{
    int popX = cx + cw/2 - 110, popY = cy + 450;
    Raylib.DrawRectangle(popX, popY, 220, 130, new Color((byte)20,(byte)20,(byte)30,(byte)250));
    Raylib.DrawRectangleLines(popX, popY, 220, 130, Color.Gold);
    Program.DrawTextUI($"{menuData.Name} (x{menuData.Count})", popX + 10, popY + 6, 14, Color.White);

    Rectangle moveBtn = new Rectangle(popX + 10, popY + 30, 200, 26);
    Rectangle deleteBtn = new Rectangle(popX + 10, popY + 60, 200, 26);
    Rectangle switchBtn = new Rectangle(popX + 10, popY + 90, 200, 26);

    bool hovMove = Raylib.CheckCollisionPointRec(mouse, moveBtn);
    bool hovDelete = Raylib.CheckCollisionPointRec(mouse, deleteBtn);
    bool hovSwitch = Raylib.CheckCollisionPointRec(mouse, switchBtn);

    Raylib.DrawRectangleRec(moveBtn, hovMove ? new Color((byte)50,(byte)55,(byte)80,(byte)255) : new Color((byte)30,(byte)33,(byte)48,(byte)255));
    Program.DrawTextUI("Move (1 card)", (int)moveBtn.X + 8, (int)moveBtn.Y + 5, 14, Color.White);
    Raylib.DrawRectangleRec(deleteBtn, hovDelete ? new Color((byte)80,(byte)40,(byte)40,(byte)255) : new Color((byte)30,(byte)33,(byte)48,(byte)255));
    Program.DrawTextUI("Delete (1 card)", (int)deleteBtn.X + 8, (int)deleteBtn.Y + 5, 14, Color.White);
    Raylib.DrawRectangleRec(switchBtn, hovSwitch ? new Color((byte)40,(byte)70,(byte)80,(byte)255) : new Color((byte)30,(byte)33,(byte)48,(byte)255));
    Program.DrawTextUI("Switch to Master Set", (int)switchBtn.X + 8, (int)switchBtn.Y + 5, 14, Color.White);

    if (hovMove && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        heldSingleCardSlot = cardContextMenuSlot;
        cardContextMenuSlot = -1;
        ShowNotification("Pick a slot to move this card to.");
    }
    if (hovDelete && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        menuData.Count--;
        if (menuData.Count <= 0) personalBinder.SlotAssignments.Remove(cardContextMenuSlot);
        cardContextMenuSlot = -1;
        ShowNotification($"{menuData.Name} deleted.");
    }
    if (hovSwitch && Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        masterSetBinder.Add(menuData.Name, menuData.IsReverse);
        menuData.Count--;
        if (menuData.Count <= 0) personalBinder.SlotAssignments.Remove(cardContextMenuSlot);
        cardContextMenuSlot = -1;
        ShowNotification($"{menuData.Name} sent back to Master Set!");
    }

    Rectangle cancelX = new Rectangle(popX + 190, popY + 4, 24, 20);
    if (Raylib.CheckCollisionPointRec(mouse, cancelX) && Raylib.IsMouseButtonPressed(MouseButton.Left)) cardContextMenuSlot = -1;
    Program.DrawTextUI("X", (int)cancelX.X + 6, (int)cancelX.Y, 16, Color.Red);
}

if ((heldSingleCardSlot >= 0 || cardContextMenuSlot >= 0) && (Raylib.IsMouseButtonPressed(MouseButton.Right) || Raylib.IsKeyPressed(KeyboardKey.Escape)))
{
    heldSingleCardSlot = -1;
    cardContextMenuSlot = -1;
}

if (placementModeActive)
{
    Program.DrawTextUI($"Placing: {placementCardName} click an empty slot", cx, cy + 90, 16, Color.Gold);
}
    Program.DrawTextUI("Q = Close Binder", cx + cardSets.Count * 180 + 20, cy + 2, 20, Color.LightGray); 
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) binderOpen = false;
}

static void DrawRocket(int x, int y)
{
    // launch pad
    Raylib.DrawRectangle(x - 50, y + 80, 100, 16, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(x - 40, y + 96, 12, 30, new Color((byte)50,(byte)50,(byte)60,(byte)255));
    Raylib.DrawRectangle(x + 28, y + 96, 12, 30, new Color((byte)50,(byte)50,(byte)60,(byte)255));

    // body
    Raylib.DrawRectangle(x - 20, y - 40, 40, 120, new Color((byte)220,(byte)220,(byte)230,(byte)255));
    Raylib.DrawRectangle(x - 20, y - 40, 40, 120, new Color((byte)220,(byte)220,(byte)230,(byte)255));
    // body shading stripe
    Raylib.DrawRectangle(x + 8, y - 40, 12, 120, new Color((byte)190,(byte)190,(byte)205,(byte)255));

    // nose cone
    Raylib.DrawTriangle(
        new Vector2(x - 20, y - 40),
        new Vector2(x + 20, y - 40),
        new Vector2(x, y - 80),
        new Color((byte)200,(byte)40,(byte)40,(byte)255));

    // window
    Raylib.DrawCircle(x, y - 10, 10, new Color((byte)100,(byte)180,(byte)220,(byte)255));
    Raylib.DrawCircleLines(x, y - 10, 10, new Color((byte)60,(byte)60,(byte)70,(byte)255));

    // fins
    Raylib.DrawTriangle(
        new Vector2(x - 20, y + 50),
        new Vector2(x - 20, y + 90),
        new Vector2(x - 42, y + 90),
        new Color((byte)200,(byte)40,(byte)40,(byte)255));
    Raylib.DrawTriangle(
        new Vector2(x + 20, y + 90),
        new Vector2(x + 20, y + 50),
        new Vector2(x + 42, y + 90),
        new Color((byte)200,(byte)40,(byte)40,(byte)255));

    // engine nozzle
    Raylib.DrawRectangle(x - 12, y + 80, 24, 12, new Color((byte)80,(byte)80,(byte)90,(byte)255));
}

static void DrawHallensteinShop()
{
    if (!hallensteinShopOpen) return;

    string[] cats = { "Tops", "Bottoms", "Outerwear", "Accessories" };

    int pw = 1180, ph = 600;
    int px = ScreenWidth / 2 - pw / 2;
    int py = 60;
    Color gold = new Color((byte)180,(byte)140,(byte)20,(byte)255);

    // panel background
    Raylib.DrawRectangle(px, py, pw, ph, new Color((byte)10,(byte)10,(byte)10,(byte)250));
    Raylib.DrawRectangleLines(px, py, pw, ph, gold);
    Program.DrawTextUI("HALLENSTEINS", px + pw/2 - Program.MeasureTextUI("HALLENSTEINS",28)/2, py + 10, 28, gold);
    Program.DrawTextUI($"${player.Money}", px + pw - 100, py + 14, 20, Color.Gold);

    // category tabs
    for (int i = 0; i < cats.Length; i++)
    {
        Rectangle tab = new Rectangle(px + 10 + i * 146, py + 46, 140, 30);
        bool active = hallensteinCategory == i;
        Raylib.DrawRectangleRec(tab, active ? gold : new Color((byte)30,(byte)30,(byte)30,(byte)255));
        Raylib.DrawRectangleLinesEx(tab, 1, active ? gold : new Color((byte)60,(byte)60,(byte)60,(byte)255));
        Program.DrawTextUI(cats[i], (int)tab.X + 12, (int)tab.Y + 8, 16, active ? Color.Black : Color.LightGray);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), tab) && Raylib.IsMouseButtonPressed(MouseButton.Left))
        { hallensteinCategory = i; hallensteinScroll = 0; }
    }

    // filter items by category
    string activeCat = cats[hallensteinCategory];
    var catItems = hallItems.Where(h => h.Category == activeCat).ToArray();

    // scroll arrows
    int cardW = 180, cardH = 420, cardPad = 16;
    int visibleCards = 5;
    int maxScroll = Math.Max(0, catItems.Length - visibleCards);
    hallensteinScroll = Math.Clamp(hallensteinScroll, 0, maxScroll);

    Rectangle leftArrow  = new Rectangle(px + 4, py + ph/2 - 24, 28, 48);
    Rectangle rightArrow = new Rectangle(px + pw - 32, py + ph/2 - 24, 28, 48);
    bool hLeft  = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), leftArrow)  && hallensteinScroll > 0;
    bool hRight = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rightArrow) && hallensteinScroll < maxScroll;

    Raylib.DrawRectangleRec(leftArrow,  new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Raylib.DrawRectangleRec(rightArrow, new Color((byte)30,(byte)30,(byte)30,(byte)255));
    Program.DrawTextUI("<", px + 8,      py + ph/2 - 12, 28, hLeft  ? gold : Color.DarkGray);
    Program.DrawTextUI(">", px + pw - 24, py + ph/2 - 12, 28, hRight ? gold : Color.DarkGray);

    if (hLeft  && Raylib.IsMouseButtonPressed(MouseButton.Left)) hallensteinScroll--;
    if (hRight && Raylib.IsMouseButtonPressed(MouseButton.Left)) hallensteinScroll++;

    // draw item cards
    int startX = px + 36;
    int cardY   = py + 84;
    for (int i = 0; i < visibleCards && (i + hallensteinScroll) < catItems.Length; i++)
    {
        var item = catItems[i + hallensteinScroll];
        int cx = startX + i * (cardW + cardPad);

        Rectangle card = new Rectangle(cx, cardY, cardW, cardH);
        bool hover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), card);
        Raylib.DrawRectangleRec(card, new Color((byte)22,(byte)22,(byte)22,(byte)255));
        Raylib.DrawRectangleLinesEx(card, hover ? 2 : 1, hover ? gold : new Color((byte)50,(byte)50,(byte)50,(byte)255));

        // mannequin preview — drawn facing down, 3x scale via offset math
        int mx = cx + cardW/2 - 20;  // mannequin left edge (40px wide at 1x)
        int my = cardY + 20;
        DrawHallMannequin(mx, my, item, player.SkinColor);

        // name + price
        int nameW = Program.MeasureTextUI(item.Name, 16);
        Program.DrawTextUI(item.Name, cx + cardW/2 - nameW/2, cardY + cardH - 70, 16, Color.White);
        string priceStr = $"${item.Price}";
        int priceW = Program.MeasureTextUI(priceStr, 18);
        Program.DrawTextUI(priceStr, cx + cardW/2 - priceW/2, cardY + cardH - 48, 18, Color.Gold);

        // buy button
        Rectangle buyBtn = new Rectangle(cx + 14, cardY + cardH - 26, cardW - 28, 22);
        bool hBuy = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buyBtn);
        Raylib.DrawRectangleRec(buyBtn, hBuy ? gold : new Color((byte)40,(byte)35,(byte)10,(byte)255));
        Raylib.DrawRectangleLinesEx(buyBtn, 1, gold);
        int buyW = Program.MeasureTextUI("BUY", 15);
        Program.DrawTextUI("BUY", (int)buyBtn.X + (int)buyBtn.Width/2 - buyW/2, (int)buyBtn.Y + 4, 15, hBuy ? Color.Black : gold);

        if (hBuy && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (player.Money >= item.Price)
            {
                player.Money -= item.Price;
                ApplyHallItem(item);
                shopMessage = $"Sweet pick bro! {item.Name} is yours.";
                shopMessageTimer = 2f;
            }
            else
            {
                shopMessage = "Not enough cash bro!";
                shopMessageTimer = 1.5f;
            }
        }
    }

    Program.DrawTextUI("Q = Close", px + pw/2 - 40, py + ph - 18, 16, Color.DarkGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Q)) { hallensteinShopOpen = false; }
}

// Adding dcorative assets

// Adding decorative buildings

// Adding interactive buildings

static void DrawLibraryInterior()
{
    Color floor1 = new Color((byte)210,(byte)185,(byte)150,(byte)255);
    Color floor2 = new Color((byte)195,(byte)168,(byte)132,(byte)255);
    Color rug    = new Color((byte)120,(byte)40,(byte)45,(byte)255);
    Color rugHi  = new Color((byte)150,(byte)55,(byte)60,(byte)255);
    Color wood   = new Color((byte)95,(byte)60,(byte)30,(byte)255);
    Color woodHi = new Color((byte)120,(byte)78,(byte)42,(byte)255);
    Color wallCol = new Color((byte)70,(byte)55,(byte)40,(byte)255);

    // parquet floor
    for (int fx = 0; fx < 1400; fx += 80)
        for (int fy = 0; fy < 1000; fy += 80)
            Raylib.DrawRectangle(fx, fy, 80, 80, ((fx / 80 + fy / 80) % 2 == 0) ? floor1 : floor2);

    // walls
    Raylib.DrawRectangle(0, 0, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 984, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 0, 16, 1000, wallCol);
    Raylib.DrawRectangle(1384, 0, 16, 1000, wallCol);

    // central reading rug
    Raylib.DrawRectangle(470, 560, 460, 300, rug);
    Raylib.DrawRectangleLines(470, 560, 460, 300, rugHi);
    Raylib.DrawRectangle(500, 590, 400, 240, new Color((byte)135,(byte)48,(byte)52,(byte)255));

    // bookshelf helper
    void Shelf(int sx, int sy, int sw, int sh, bool horizontal)
    {
        Raylib.DrawRectangle(sx, sy, sw, sh, wood);
        Raylib.DrawRectangleLines(sx, sy, sw, sh, Color.Black);
        if (horizontal)
            for (int shelf = sy + 10; shelf < sy + sh - 6; shelf += 34)
            {
                Raylib.DrawRectangle(sx + 4, shelf, sw - 8, 5, woodHi);
                for (int b = sx + 8; b < sx + sw - 10; b += 14)
                    Raylib.DrawRectangle(b, shelf - 26, 10, 26,
                        new Color((byte)(120 + (b * 7) % 110), (byte)(50 + (b * 5) % 90), (byte)(45 + (b * 3) % 70), (byte)255));
            }
        else
            for (int shelf = sx + 8; shelf < sx + sw - 6; shelf += 34)
            {
                Raylib.DrawRectangle(shelf, sy + 4, 5, sh - 8, woodHi);
                for (int b = sy + 8; b < sy + sh - 12; b += 14)
                    Raylib.DrawRectangle(shelf - 26, b, 26, 10,
                        new Color((byte)(120 + (b * 7) % 110), (byte)(50 + (b * 5) % 90), (byte)(45 + (b * 3) % 70), (byte)255));
            }
    }

    // perimeter shelving (matches InteriorObjects collision rects)
    Shelf(100, 150, 40, 400, false);   // left bookshelf
    Shelf(1140, 150, 40, 400, false);  // right bookshelf
    Shelf(200, 150, 300, 40, true);    // front desk area shelving

    // extra ambience shelves along top wall
    Shelf(560, 60, 300, 40, true);
    Shelf(940, 150, 40, 400, false);

    // reading tables with lamps
    (int x, int y)[] tables = { (540, 620), (760, 620), (540, 740), (760, 740) };
    foreach (var (tx, ty) in tables)
    {
        Raylib.DrawRectangle(tx, ty, 100, 60, woodHi);
        Raylib.DrawRectangleLines(tx, ty, 100, 60, wood);
        Raylib.DrawCircle(tx + 50, ty + 30, 10, new Color((byte)240,(byte)220,(byte)140,(byte)255)); // lamp glow
        Raylib.DrawCircleLines(tx + 50, ty + 30, 10, new Color((byte)180,(byte)150,(byte)70,(byte)255));
    }

    // front desk (librarian counter)
    Raylib.DrawRectangle(200, 150, 300, 40, wood);
    Raylib.DrawRectangle(200, 150, 300, 8, woodHi);
    Program.DrawTextUI("FRONT DESK", 260, 158, 16, new Color((byte)235,(byte)225,(byte)205,(byte)255));

    // photo booth (matches collision + existing interaction rect 560,300,160,200)
    Raylib.DrawRectangle(560, 300, 160, 200, new Color((byte)40,(byte)40,(byte)55,(byte)255));
    Raylib.DrawRectangle(568, 308, 144, 130, new Color((byte)25,(byte)25,(byte)38,(byte)255));
    Raylib.DrawRectangle(576, 316, 128, 90, new Color((byte)60,(byte)150,(byte)190,(byte)200)); // screen
    Raylib.DrawRectangleLines(560, 300, 160, 200, Color.Gold);
    Program.DrawTextUI("PHOTO BOOTH", 575, 270, 20, Color.Gold);

    // entrance mat
    Raylib.DrawRectangle(600, 900, 200, 84, new Color((byte)60,(byte)60,(byte)70,(byte)255));
    Raylib.DrawRectangle(610, 910, 180, 64, new Color((byte)30,(byte)140,(byte)60,(byte)255));
}

static void DrawFarmingShopInterior()
{
    Color floor1 = new Color((byte)185,(byte)150,(byte)100,(byte)255);
    Color floor2 = new Color((byte)170,(byte)135,(byte)88,(byte)255);
    Color wood   = new Color((byte)110,(byte)72,(byte)38,(byte)255);
    Color woodHi = new Color((byte)140,(byte)95,(byte)50,(byte)255);
    Color wallCol = new Color((byte)80,(byte)55,(byte)30,(byte)255);
    Color leaf   = new Color((byte)60,(byte)150,(byte)60,(byte)255);

    // plank floor
    for (int fx = 0; fx < 1400; fx += 70)
        for (int fy = 0; fy < 1000; fy += 70)
            Raylib.DrawRectangle(fx, fy, 70, 70, ((fx / 70 + fy / 70) % 2 == 0) ? floor1 : floor2);
    for (int fy = 0; fy < 1000; fy += 70)
        Raylib.DrawRectangle(0, fy, 1400, 2, new Color((byte)150,(byte)118,(byte)76,(byte)255));

    // walls
    Raylib.DrawRectangle(0, 0, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 984, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 0, 16, 1000, wallCol);
    Raylib.DrawRectangle(1384, 0, 16, 1000, wallCol);

    // counter (matches collision 400,80,400,40)
    Raylib.DrawRectangle(400, 80, 400, 40, wood);
    Raylib.DrawRectangle(400, 80, 400, 8, woodHi);
    Raylib.DrawRectangle(410, 88, 60, 26, new Color((byte)200,(byte)190,(byte)170,(byte)255)); // till
    Program.DrawTextUI("COUNTER", 560, 90, 18, new Color((byte)235,(byte)225,(byte)205,(byte)255));

    // display bin helper (crates of produce/seed)
    void Bin(int dx, int dy, int dw, int dh, Color prod, string label)
    {
        Raylib.DrawRectangle(dx, dy, dw, dh, wood);
        Raylib.DrawRectangleLines(dx, dy, dw, dh, Color.Black);
        Raylib.DrawRectangle(dx + 8, dy + 8, dw - 16, dh - 16, woodHi);
        for (int px = dx + 16; px < dx + dw - 12; px += 22)
            for (int py = dy + 16; py < dy + dh - 12; py += 22)
                Raylib.DrawCircle(px, py, 7, prod);
        Program.DrawTextUI(label, dx + 6, dy - 18, 14, new Color((byte)90,(byte)60,(byte)30,(byte)255));
    }

    // display stands (match collision rects)
    Bin(100, 200, 200, 120, new Color((byte)150,(byte)150,(byte)160,(byte)255), "TOOLS");   // tool display
    Bin(350, 200, 200, 120, new Color((byte)210,(byte)180,(byte)90,(byte)255), "SEEDS");    // seed display 1
    Bin(600, 200, 200, 120, new Color((byte)200,(byte)90,(byte)70,(byte)255), "SEEDS");     // seed display 2

    // hanging tools on right wall
    for (int t = 0; t < 4; t++)
    {
        int tx = 1000 + t * 70;
        Raylib.DrawRectangle(tx, 180, 8, 90, new Color((byte)90,(byte)90,(byte)95,(byte)255));
        Raylib.DrawRectangle(tx - 10, 180, 28, 14, new Color((byte)120,(byte)120,(byte)130,(byte)255));
    }

    // potted plants for ambience
    (int x, int y)[] pots = { (200, 560), (1150, 560), (200, 780), (1150, 780) };
    foreach (var (px, py) in pots)
    {
        Raylib.DrawRectangle(px, py + 26, 40, 26, new Color((byte)150,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircle(px + 20, py + 16, 22, leaf);
        Raylib.DrawCircle(px + 8, py + 22, 12, new Color((byte)75,(byte)165,(byte)75,(byte)255));
    }

    // stacked hay bales bottom-left
    for (int hb = 0; hb < 3; hb++)
        Raylib.DrawRectangle(120 + hb * 6, 800 - hb * 44, 90, 44, new Color((byte)200,(byte)175,(byte)90,(byte)255));

    // entrance mat
    Raylib.DrawRectangle(600, 900, 200, 84, new Color((byte)70,(byte)50,(byte)30,(byte)255));
    Raylib.DrawRectangle(610, 910, 180, 64, new Color((byte)110,(byte)80,(byte)45,(byte)255));
}

static void DrawBarnInterior()
{
    Color floor1 = new Color((byte)175,(byte)140,(byte)90,(byte)255);
    Color floor2 = new Color((byte)160,(byte)126,(byte)80,(byte)255);
    Color wood   = new Color((byte)110,(byte)72,(byte)38,(byte)255);
    Color woodHi = new Color((byte)140,(byte)95,(byte)50,(byte)255);
    Color wallCol = new Color((byte)80,(byte)50,(byte)28,(byte)255);

    for (int fx = 0; fx < 1400; fx += 70)
        for (int fy = 0; fy < 1000; fy += 70)
            Raylib.DrawRectangle(fx, fy, 70, 70, ((fx / 70 + fy / 70) % 2 == 0) ? floor1 : floor2);

    Raylib.DrawRectangle(0, 0, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 984, 1400, 16, wallCol);
    Raylib.DrawRectangle(0, 0, 16, 1000, wallCol);
    Raylib.DrawRectangle(1384, 0, 16, 1000, wallCol);

    // counter
    Raylib.DrawRectangle(350, 80, 700, 60, wood);
    Raylib.DrawRectangle(350, 80, 700, 8, woodHi);
    Program.DrawTextUI("LIVESTOCK COUNTER", 520, 96, 20, new Color((byte)240,(byte)235,(byte)225,(byte)255));

    // stalls with animal silhouettes
    string[] leftAnimals  = { "Chicken", "Cow", "Sheep" };
    string[] rightAnimals = { "Pig", "Goat", "Chicken" };
    void Stall(int sx, int sy, string label)
    {
        Raylib.DrawRectangle(sx, sy, 260, 150, new Color((byte)150,(byte)120,(byte)80,(byte)255));
        for (int b = sx; b <= sx + 260; b += 30)
            Raylib.DrawRectangle(b, sy, 4, 150, wood);
        Raylib.DrawRectangle(sx, sy, 260, 6, woodHi);
        // hay + trough
        Raylib.DrawRectangle(sx + 20, sy + 110, 60, 24, new Color((byte)210,(byte)185,(byte)95,(byte)255));
        Program.DrawTextUI(label, sx + 90, sy + 60, 20, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    }
    for (int i = 0; i < 3; i++) Stall(40, 200 + i * 200, leftAnimals[i]);
    for (int i = 0; i < 3; i++) Stall(1100, 200 + i * 200, rightAnimals[i]);

    // hay bale display
    for (int hb = 0; hb < 3; hb++)
        Raylib.DrawRectangle(560 + hb * 90, 780, 84, 80, new Color((byte)205,(byte)180,(byte)90,(byte)255));

    // browse prompt
    if (Vector2.Distance(player.Center, new Vector2(700, 160)) < 180)
    {
        Raylib.DrawRectangle(0, 620, 1400, 90, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI("RANCHER", 20, 636, 26, new Color((byte)210,(byte)130,(byte)60,(byte)255));
        Program.DrawTextUI("E = Buy animals & supplies", 20, 672, 22, Color.White);
    }
}

static void DrawZooInterior()
{
    Color path1 = new Color((byte)200,(byte)185,(byte)150,(byte)255);
    Color path2 = new Color((byte)188,(byte)172,(byte)138,(byte)255);
    Color grass = new Color((byte)90,(byte)160,(byte)90,(byte)255);
    Color wallCol = new Color((byte)80,(byte)120,(byte)80,(byte)255);

    // grass base
    Raylib.ClearBackground(grass);
    for (int fx = 0; fx < 2000; fx += 100)
        for (int fy = 0; fy < 2000; fy += 100)
            if ((fx/100 + fy/100) % 3 == 0)
                Raylib.DrawRectangle(fx, fy, 100, 100, new Color((byte)80,(byte)150,(byte)82,(byte)255));

    // walking paths (cross layout)
    Raylib.DrawRectangle(600, 0, 200, 2000, path1);
    Raylib.DrawRectangle(0, 900, 2000, 200, path2);

    // outer walls
    Raylib.DrawRectangle(0, 0, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 1980, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 2000, wallCol);
    Raylib.DrawRectangle(1980, 0, 20, 2000, wallCol);

    // enclosure helper
    void Exhibit(int ex, int ey, int ew, int eh, string label, Color habitat, Color animal)
    {
        Raylib.DrawRectangle(ex, ey, ew, eh, habitat);
        // fence
        Raylib.DrawRectangleLinesEx(new Rectangle(ex, ey, ew, eh), 4, new Color((byte)90,(byte)70,(byte)45,(byte)255));
        for (int b = ex; b < ex + ew; b += 26)
            Raylib.DrawRectangle(b, ey, 3, eh, new Color((byte)110,(byte)85,(byte)55,(byte)150));
        // a couple of animal blobs
        Raylib.DrawCircle(ex + ew/2, ey + eh/2, 26, animal);
        Raylib.DrawCircle(ex + ew/2 + 34, ey + eh/2 + 14, 18, animal);
        Raylib.DrawCircle(ex + ew/2 + 18, ey + eh/2 - 6, 8, animal); // head
        // name plaque
        Raylib.DrawRectangle(ex + ew/2 - 60, ey - 26, 120, 22, new Color((byte)60,(byte)45,(byte)30,(byte)255));
        Program.DrawTextUI(label, ex + ew/2 - 52, ey - 24, 16, Color.White);
    }

    Exhibit(150, 200, 420, 320, "LIONS",     new Color((byte)200,(byte)180,(byte)120,(byte)255), new Color((byte)200,(byte)150,(byte)60,(byte)255));
    Exhibit(900, 200, 420, 320, "ELEPHANTS", new Color((byte)170,(byte)175,(byte)165,(byte)255), new Color((byte)130,(byte)130,(byte)138,(byte)255));
    Exhibit(150, 700, 420, 320, "PENGUINS",  new Color((byte)150,(byte)200,(byte)225,(byte)255), new Color((byte)40,(byte)40,(byte)48,(byte)255));
    Exhibit(900, 700, 420, 320, "MONKEYS",   new Color((byte)110,(byte)160,(byte)90,(byte)255),  new Color((byte)120,(byte)80,(byte)45,(byte)255));
    Exhibit(525, 1150, 420, 300, "GIRAFFES", new Color((byte)200,(byte)185,(byte)120,(byte)255), new Color((byte)220,(byte)180,(byte)90,(byte)255));

    // ticket booth
    Raylib.DrawRectangle(880, 60, 240, 60, new Color((byte)210,(byte)120,(byte)40,(byte)255));
    Program.DrawTextUI("TICKET BOOTH", 910, 78, 20, Color.White);

    // exit mat
    Raylib.DrawRectangle(900, 1880, 200, 90, new Color((byte)60,(byte)45,(byte)30,(byte)255));
    Program.DrawTextUI("EXIT", 960, 1905, 20, Color.Gold);
}

static void DrawCastleInterior()
{
    Color floor1 = new Color((byte)90,(byte)85,(byte)95,(byte)255);
    Color floor2 = new Color((byte)78,(byte)74,(byte)84,(byte)255);
    Color wallCol = new Color((byte)55,(byte)52,(byte)62,(byte)255);
    Color stoneHi = new Color((byte)110,(byte)105,(byte)118,(byte)255);
    Color gold    = new Color((byte)210,(byte)175,(byte)70,(byte)255);
    Color carpet  = new Color((byte)130,(byte)30,(byte)40,(byte)255);

    // flagstone floor (2000x2000 like school)
    for (int fx = 0; fx < 2000; fx += 100)
        for (int fy = 0; fy < 2000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);
    for (int g = 0; g < 2000; g += 100)
    {
        Raylib.DrawRectangle(g, 0, 2, 2000, wallCol);
        Raylib.DrawRectangle(0, g, 2000, 2, wallCol);
    }

    // outer walls
    Raylib.DrawRectangle(0, 0, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 1980, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 2000, wallCol);
    Raylib.DrawRectangle(1980, 0, 20, 2000, wallCol);

    // red carpet runner from entrance to throne
    Raylib.DrawRectangle(940, 280, 120, 1600, carpet);
    Raylib.DrawRectangleLines(940, 280, 120, 1600, gold);

    // throne dais (matches collision 850,120,300,160)
    Raylib.DrawRectangle(850, 120, 300, 160, stoneHi);
    Raylib.DrawRectangleLines(850, 120, 300, 160, gold);
    Raylib.DrawRectangle(960, 150, 80, 110, new Color((byte)80,(byte)40,(byte)45,(byte)255)); // throne back
    Raylib.DrawRectangle(950, 220, 100, 50, gold);                                            // throne seat
    Program.DrawTextUI("THRONE", 940, 90, 20, gold);

    // braziers (matches collision)
    foreach (var (bxp, byp) in new[] { (760, 180), (1190, 180) })
    {
        Raylib.DrawRectangle(bxp, byp, 50, 50, new Color((byte)60,(byte)55,(byte)50,(byte)255));
        Raylib.DrawCircle(bxp + 25, byp + 20, 18, new Color((byte)255,(byte)150,(byte)40,(byte)255));
        Raylib.DrawCircle(bxp + 25, byp + 12, 10, new Color((byte)255,(byte)220,(byte)120,(byte)255));
    }

    // banquet tables (matches collision 300,700 / 1540,700)
    foreach (int tx in new[] { 300, 1540 })
    {
        Raylib.DrawRectangle(tx, 700, 160, 500, new Color((byte)110,(byte)72,(byte)38,(byte)255));
        Raylib.DrawRectangleLines(tx, 700, 160, 500, new Color((byte)70,(byte)45,(byte)20,(byte)255));
        for (int p = 720; p < 1180; p += 90)
        {
            Raylib.DrawRectangle(tx + 20, p, 30, 20, gold);            // platters
            Raylib.DrawRectangle(tx + 110, p, 30, 20, new Color((byte)200,(byte)60,(byte)60,(byte)255));
        }
    }

    // pillars (match collision)
    for (int p = 0; p < 4; p++)
        foreach (int px in new[] { 560, 1370 })
        {
            Raylib.DrawRectangle(px, 500 + p * 320, 70, 70, stoneHi);
            Raylib.DrawRectangleLines(px, 500 + p * 320, 70, 70, wallCol);
            Raylib.DrawCircle(px + 35, 500 + p * 320 + 35, 18, new Color((byte)95,(byte)90,(byte)102,(byte)255));
        }

    // wall banners
    for (int b = 0; b < 5; b++)
    {
        int bxp = 200 + b * 380;
        Raylib.DrawRectangle(bxp, 30, 60, 130, (b % 2 == 0) ? carpet : new Color((byte)40,(byte)55,(byte)110,(byte)255));
        Raylib.DrawTriangle(new Vector2(bxp, 160), new Vector2(bxp + 60, 160),
            new Vector2(bxp + 30, 190), (b % 2 == 0) ? carpet : new Color((byte)40,(byte)55,(byte)110,(byte)255));
        Raylib.DrawCircle(bxp + 30, 90, 16, gold);
    }

    // exit mat
    Raylib.DrawRectangle(900, 1880, 200, 90, new Color((byte)60,(byte)45,(byte)30,(byte)255));
    Program.DrawTextUI("EXIT", 960, 1905, 20, gold);

    // ── Prince & Princess near throne ──
    // Prince (left of throne)
    Raylib.DrawCircle(900, 230, 14, new Color((byte)240,(byte)210,(byte)170,(byte)255)); // head
    Raylib.DrawRectangle(888, 244, 24, 32, new Color((byte)60,(byte)60,(byte)150,(byte)255)); // blue tunic
    Raylib.DrawRectangle(893, 218, 14, 8, gold); // crown
    Raylib.DrawRectangle(896, 212, 3, 8, gold);
    Raylib.DrawRectangle(904, 212, 3, 8, gold);
    Program.DrawTextUI("Prince", 876, 195, 14, gold);

    // Princess (right of throne)
    Raylib.DrawCircle(1100, 230, 14, new Color((byte)240,(byte)210,(byte)170,(byte)255));
    Raylib.DrawRectangle(1088, 244, 24, 34, new Color((byte)180,(byte)50,(byte)80,(byte)255)); // dress
    Raylib.DrawTriangle(new Vector2(1082, 278), new Vector2(1118, 278),
        new Vector2(1100, 300), new Color((byte)180,(byte)50,(byte)80,(byte)255)); // skirt
    Raylib.DrawRectangle(1093, 218, 14, 8, gold); // tiara
    Raylib.DrawCircle(1100, 216, 4, new Color((byte)200,(byte)60,(byte)60,(byte)255)); // gem
    Program.DrawTextUI("Princess", 1070, 195, 14, gold);

    // ── Mastery Shop counter ──
    Raylib.DrawRectangle(1600, 300, 300, 60, new Color((byte)80,(byte)50,(byte)30,(byte)255));
    Raylib.DrawRectangleLines(1600, 300, 300, 60, gold);
    Program.DrawTextUI("HALL OF MASTERY", 1620, 280, 18, gold);
    Raylib.DrawRectangle(1610, 310, 280, 40, new Color((byte)50,(byte)30,(byte)18,(byte)255));

    // Grandmaster NPC behind counter
    Raylib.DrawCircle(1750, 280, 16, new Color((byte)240,(byte)210,(byte)170,(byte)255));
    Raylib.DrawRectangle(1736, 296, 28, 36, new Color((byte)140,(byte)100,(byte)200,(byte)255));
    Raylib.DrawRectangle(1740, 260, 20, 10, gold); // crown
    Program.DrawTextUI("Grandmaster", 1710, 244, 14, new Color((byte)200,(byte)160,(byte)255,(byte)255));
}

static void DrawMallConcourse()
{
    Color floor1 = new Color((byte)232,(byte)230,(byte)236,(byte)255);
    Color floor2 = new Color((byte)218,(byte)216,(byte)224,(byte)255);
    Color wallCol = new Color((byte)90,(byte)95,(byte)110,(byte)255);

    // polished tile floor
    for (int fx = 0; fx < 2000; fx += 100)
        for (int fy = 0; fy < 2000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);

    // outer walls
    Raylib.DrawRectangle(0, 0, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 1980, 2000, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 2000, wallCol);
    Raylib.DrawRectangle(1980, 0, 20, 2000, wallCol);

    // entrance gap, bottom-centre
    Program.DrawTextUI("ENTRANCE", 920, 1940, 16, Color.White);

    // storefront helper — matches AddMall ShopFront geometry + door gap
    void Storefront(int rx, int ry, int rw, int rh, string label, Color col, int doorOffset, int doorW)
    {
        Raylib.DrawRectangle(rx, ry, rw, rh, new Color((byte)245,(byte)245,(byte)250,(byte)255)); // shop fill
        Raylib.DrawRectangle(rx, ry, rw, 14, wallCol);
        Raylib.DrawRectangle(rx, ry, 14, rh, wallCol);
        Raylib.DrawRectangle(rx + rw - 14, ry, 14, rh, wallCol);
        Raylib.DrawRectangle(rx, ry + rh - 14, doorOffset, 14, wallCol);
        Raylib.DrawRectangle(rx + doorOffset + doorW, ry + rh - 14, rw - doorOffset - doorW, 14, wallCol);

        // coloured fascia + glowing sign
        Raylib.DrawRectangle(rx, ry + 14, rw, 40, col);
        Program.DrawTextUI(label, rx + 30, ry + 22, 22, Color.White);

        // window display glass either side of the door
        Raylib.DrawRectangle(rx + 20, ry + rh - 90, doorOffset - 30, 76, new Color((byte)150,(byte)205,(byte)230,(byte)200));
        Raylib.DrawRectangle(rx + doorOffset + doorW + 10, ry + rh - 90, rw - doorOffset - doorW - 30, 76, new Color((byte)150,(byte)205,(byte)230,(byte)200));

        // door prompt
        Vector2 dPos = new Vector2(rx + doorOffset + doorW / 2, ry + rh);
        if (Vector2.Distance(player.Center, dPos) < 90)
            Program.DrawTextUI("E = Enter " + label, rx + doorOffset - 40, ry + rh + 20, 18, Color.Gold);
    }

    Storefront(200, 300, 340, 340, "CLOTHING",   mallShops[0].col, 130, 80);
    Storefront(600, 300, 340, 340, "ELECTRONICS", mallShops[1].col, 130, 80);
    Storefront(1000, 300, 340, 340, "SPORTS",     mallShops[2].col, 130, 80);
    Storefront(200, 1020, 340, 340, "BOOKS",      mallShops[3].col, 130, 80);
    Storefront(600, 1020, 340, 340, "FOOD COURT", mallShops[4].col, 130, 80);
    Storefront(1000, 1020, 340, 340, "TOYS",      mallShops[5].col, 130, 80);

    // central fountain (matches collision 920,820,160,160)
    Raylib.DrawCircle(1000, 900, 84, new Color((byte)120,(byte)130,(byte)150,(byte)255));
    Raylib.DrawCircle(1000, 900, 74, new Color((byte)90,(byte)170,(byte)210,(byte)255));
    Raylib.DrawCircle(1000, 900, 30, new Color((byte)140,(byte)205,(byte)235,(byte)255));
    Raylib.DrawCircle(1000, 900, 10, Color.White);

    // benches
    foreach (var (bxp, byp) in new[] { (500, 880), (1380, 880) })
    {
        Raylib.DrawRectangle(bxp, byp, 120, 40, new Color((byte)150,(byte)110,(byte)70,(byte)255));
        Raylib.DrawRectangleLines(bxp, byp, 120, 40, new Color((byte)90,(byte)65,(byte)35,(byte)255));
    }

    // potted planters between shops
    foreach (var (px, py) in new[] { (560, 830), (1420, 830) })
    {
        Raylib.DrawRectangle(px, py, 40, 40, new Color((byte)120,(byte)80,(byte)50,(byte)255));
        Raylib.DrawCircle(px + 20, py, 24, new Color((byte)70,(byte)160,(byte)80,(byte)255));
    }
}

static void DrawMiniShopInterior(string shop)
{
    var meta = mallShops.First(s => s.name == shop);
    Color col = meta.col;
    Color floor1 = new Color((byte)238,(byte)236,(byte)242,(byte)255);
    Color floor2 = new Color((byte)224,(byte)222,(byte)230,(byte)255);
    Color wallCol = new Color((byte)90,(byte)95,(byte)110,(byte)255);

    Raylib.ClearBackground(new Color((byte)30,(byte)30,(byte)38,(byte)255));
    for (int fx = 0; fx < 1400; fx += 100)
        for (int fy = 0; fy < 1000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx / 100 + fy / 100) % 2 == 0) ? floor1 : floor2);

    // walls + coloured header
    Raylib.DrawRectangle(0, 0, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 980, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 1000, wallCol);
    Raylib.DrawRectangle(1380, 0, 20, 1000, wallCol);
    Raylib.DrawRectangle(0, 0, 1400, 70, col);
    Program.DrawTextUI(shop, 40, 22, 34, Color.White);

    // draw the shop's collision furniture as shelves/racks
    foreach (var r in miniShopInteriorObjects.GetValueOrDefault(shop, new List<Rectangle>()))
    {
        Raylib.DrawRectangleRec(r, new Color((byte)110,(byte)78,(byte)42,(byte)255));
        Raylib.DrawRectangleLinesEx(r, 2, new Color((byte)70,(byte)45,(byte)20,(byte)255));
        // product dots
        for (float px = r.X + 20; px < r.X + r.Width - 12; px += 34)
            for (float py = r.Y + 18; py < r.Y + r.Height - 12; py += 34)
                Raylib.DrawCircle((int)px, (int)py, 9,
                    new Color((byte)((int)(px + py) % 200 + 40), (byte)((int)px % 180 + 50), (byte)((int)py % 160 + 60), (byte)255));
    }

    // counter with till
    Raylib.DrawRectangle(560, 760, 280, 60, new Color((byte)120,(byte)85,(byte)45,(byte)255));
    Raylib.DrawRectangle(600, 730, 60, 30, new Color((byte)210,(byte)210,(byte)220,(byte)255));
    Program.DrawTextUI("COUNTER", 620, 775, 18, Color.White);

    // exit door
    Raylib.DrawRectangle(650, 950, 100, 40, new Color((byte)60,(byte)40,(byte)20,(byte)255));
    Raylib.DrawRectangleLines(650, 950, 100, 40, Color.Black);
    if (Vector2.Distance(player.Center, new Vector2(700, 950)) < 70)
        Program.DrawTextUI("Q = Leave shop", 610, 900, 18, Color.LightGray);

    // browse prompt
    if (Vector2.Distance(player.Center, new Vector2(700, 790)) < 180)
    {
        Raylib.DrawRectangle(0, 620, 1400, 90, new Color((byte)0,(byte)0,(byte)0,(byte)180));
        Program.DrawTextUI($"Welcome to {shop}!  SPACE = Browse", 20, 645, 24, col);
    }
}
static void DrawFamilyHubInterior()
{
    Color floor1 = new Color((byte)235,(byte)225,(byte)240,(byte)255);
    Color floor2 = new Color((byte)222,(byte)210,(byte)230,(byte)255);
    Color wallCol = new Color((byte)70,(byte)55,(byte)85,(byte)255);

    for (int fx = 0; fx < 1400; fx += 100)
        for (int fy = 0; fy < 1900; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx/100 + fy/100) % 2 == 0) ? floor1 : floor2);

    // boundary
    Raylib.DrawRectangle(0, 0, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 1880, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 1900, wallCol);
    Raylib.DrawRectangle(1380, 0, 20, 1900, wallCol);

    // entrance gap (bottom centre) — matches entryPos (700,1750)
    Raylib.DrawRectangle(620, 1880, 160, 20, floor1);
    Program.DrawTextUI("EXIT (Q)", 630, 1840, 16, wallCol);

    // reception desk
    Raylib.DrawRectangle(560, 150, 300, 60, new Color((byte)110,(byte)80,(byte)55,(byte)255));
    Program.DrawTextUI("RECEPTION", 610, 168, 20, Color.White);
    Program.DrawTextUI("Press E to adopt", 600, 230, 16, wallCol);

    // beds down each side
    Color bedFrame = new Color((byte)150,(byte)110,(byte)80,(byte)255);
    Color bedSheet = new Color((byte)200,(byte)225,(byte)240,(byte)255);
    Color pillow   = new Color((byte)250,(byte)245,(byte)235,(byte)255);
    for (int row = 0; row < 5; row++)
    {
        int by = 350 + row * 240;
        foreach (int bxp in new[] { 120, 1160 })
        {
            Raylib.DrawRectangle(bxp, by, 120, 70, bedFrame);
            Raylib.DrawRectangle(bxp + 6, by + 6, 108, 58, bedSheet);
            Raylib.DrawRectangle(bxp + 8, by + 8, 40, 28, pillow);
        }
    }
    foreach (var o in FamilyHubNPCs)   
    {
        o.Draw();
        int tw = Program.MeasureTextUI(o.Name, 14);
        Program.DrawTextUI(o.Name, (int)o.Position.X + 20 - tw/2, (int)o.Position.Y - 16, 14, Color.White);
    }
}
static void DrawDaycareInterior()
{
    Color floor1 = new Color((byte)255,(byte)240,(byte)200,(byte)255);
    Color floor2 = new Color((byte)250,(byte)225,(byte)175,(byte)255);
    Color wallCol = new Color((byte)200,(byte)150,(byte)80,(byte)255);

    for (int fx = 0; fx < 1400; fx += 100)
        for (int fy = 0; fy < 1000; fy += 100)
            Raylib.DrawRectangle(fx, fy, 100, 100, ((fx/100 + fy/100) % 2 == 0) ? floor1 : floor2);

    Raylib.DrawRectangle(0, 0, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 980, 1400, 20, wallCol);
    Raylib.DrawRectangle(0, 0, 20, 1000, wallCol);
    Raylib.DrawRectangle(1380, 0, 20, 1000, wallCol);

    Raylib.DrawRectangle(620, 980, 160, 20, floor1);   // entrance gap, matches entryPos (700,900)
    Program.DrawTextUI("EXIT (Q)", 630, 940, 16, wallCol);

    Raylib.DrawRectangle(540, 150, 320, 60, new Color((byte)120,(byte)85,(byte)50,(byte)255));
    Program.DrawTextUI("SIGN IN", 640, 168, 20, Color.White);
    Program.DrawTextUI("Press E to enroll / adopt", 560, 230, 16, wallCol);

    Color mat = new Color((byte)140,(byte)200,(byte)170,(byte)255);
    for (int i = 0; i < 3; i++)
    {
        int mx = 300 + i * 300;
        Raylib.DrawRectangle(mx, 500, 160, 120, mat);
        Raylib.DrawRectangleLines(mx, 500, 160, 120, wallCol);
    }
    int shown = 0;
    foreach (var npc in npcs)
    {
        if (!npc.DrawInsideNow || npc.HomeBuilding != "DAYCARE") continue;
        int cx = 300 + (shown % 3) * 300 + (int)(MathF.Sin((float)Raylib.GetTime() + shown) * 20);
        int cy = 520 + (shown / 3) * 160;
        Raylib.DrawCircle(cx + 20, cy + 12, 10, Color.Beige);                 // head
        Raylib.DrawRectangle(cx + 12, cy + 22, 16, 22, Color.SkyBlue);        // body
        int tw = Program.MeasureTextUI(npc.Name, 12);
        Program.DrawTextUI(npc.Name, cx + 20 - tw/2, cy - 12, 12, wallCol);
        shown++;
    }
}

static void SpawnPlayerHouse(int plotIndex = -1)
{
    if (ownedHousePlots.Count == 0) return;
    if (plotIndex < 0) plotIndex = activeHousePlotIndex;

    int houseWorldX = ownedHousePlots[plotIndex].x;
    int houseWorldY = ownedHousePlots[plotIndex].y;
    string houseName = $"PLAYER HOUSE {plotIndex}";

    var data = houseDataList[plotIndex];

    Color wallCol = data.WallColor switch {
        "Beige"  => new Color((byte)210,(byte)190,(byte)150,(byte)255),
        "White"  => new Color((byte)240,(byte)240,(byte)235,(byte)255),
        "Blue"   => new Color((byte)80,(byte)120,(byte)180,(byte)255),
        "Green"  => new Color((byte)80,(byte)150,(byte)80,(byte)255),
        "Red"    => new Color((byte)180,(byte)70,(byte)70,(byte)255),
        _        => new Color((byte)210,(byte)190,(byte)150,(byte)255)
    };

    var h = new Building(
        new Rectangle(houseWorldX, houseWorldY, 240, 180),
        wallCol,
        new Color((byte)220,(byte)205,(byte)170,(byte)255),
        new Vector2(houseWorldX + 120, houseWorldY + 150),
        houseName,
        new NPC(new Vector2(640, 300), "Home", "Welcome home!"),
        entryPos: new Vector2(640, 880)
    );

    h.InteriorObjects.Clear();
    h.InteriorObjects.Add(new Rectangle(60,   60,  500, 50));
    h.InteriorObjects.Add(new Rectangle(1030, 450, 300, 270));
    h.InteriorObjects.Add(new Rectangle(820,  870, 105, 90));

    foreach (var f in data.Furniture)
    {
        Rectangle baseCol = f.Type switch
        {
            "Sofa"     => new Rectangle(f.RoomX, f.RoomY, 120, 70),
            "Lamp"     => new Rectangle(f.RoomX + 14, f.RoomY + 30, 20, 30),
            "TV"       => new Rectangle(f.RoomX, f.RoomY, 100, 60),
            "Plant"    => new Rectangle(f.RoomX + 10, f.RoomY + 20, 30, 40),
            "Shelf"    => new Rectangle(f.RoomX, f.RoomY, 100, 70),
            "Rug"      => new Rectangle(f.RoomX, f.RoomY, 120, 80),
            "Wall"     => new Rectangle(f.RoomX - 10, f.RoomY, 120, 16),
            "Toilet"   => new Rectangle(f.RoomX + 4, f.RoomY, 36, 68),
            "Table"    => new Rectangle(f.RoomX, f.RoomY, 100, 60),
            "Chair"    => new Rectangle(f.RoomX + 4, f.RoomY, 44, 66),
            "Fridge"   => new Rectangle(f.RoomX, f.RoomY, 60, 90),
            "Desk"     => new Rectangle(f.RoomX, f.RoomY, 100, 55),
            "Stove"    => new Rectangle(f.RoomX, f.RoomY, 80, 70),
            "Cupboard" => new Rectangle(f.RoomX, f.RoomY, 80, 80),
            "Bench"    => new Rectangle(f.RoomX, f.RoomY, 100, 48),
            "Armchair"     => new Rectangle(f.RoomX,      f.RoomY,       80, 64),
            "BabyChair"    => new Rectangle(f.RoomX + 6,  f.RoomY,       42, 54),
            "CoffeeTable"  => new Rectangle(f.RoomX,      f.RoomY + 10,  90, 40),
            "NightStand"   => new Rectangle(f.RoomX,      f.RoomY,       60, 70),
            "Sink"         => new Rectangle(f.RoomX,      f.RoomY,       70, 55),
            "KitchenBench" => new Rectangle(f.RoomX,      f.RoomY,      110, 46),
            "Dishwasher"   => new Rectangle(f.RoomX,      f.RoomY,       65, 80),
            "PCDesk"       => new Rectangle(f.RoomX,      f.RoomY,      110, 70),
            "Speaker"      => new Rectangle(f.RoomX,      f.RoomY,       36, 70),
            "BigPlant"     => new Rectangle(f.RoomX + 10, f.RoomY + 10,  46, 76),
            "Cactus"       => new Rectangle(f.RoomX + 16, f.RoomY + 10,  18, 70),
            "FlowerPot"    => new Rectangle(f.RoomX + 6,  f.RoomY + 6,   28, 54),
            "HalfWall"     => new Rectangle(f.RoomX,      f.RoomY + 28, 120, 22),
            "WallV"        => new Rectangle(f.RoomX - 2, f.RoomY,      16, 90),
            "HalfWallV"    => new Rectangle(f.RoomX - 2, f.RoomY + 20, 16, 50),
            "Bathtub"      => new Rectangle(f.RoomX,      f.RoomY + 10, 110, 70),
            "Shower"       => new Rectangle(f.RoomX,      f.RoomY,       80, 90),
            "Painting"     => new Rectangle(f.RoomX,      f.RoomY,       80,  8),
            "Mirror"       => new Rectangle(f.RoomX,      f.RoomY,       55,  8),
            "Bin"          => new Rectangle(f.RoomX + 8,  f.RoomY + 6,   34, 50),
            "Fireplace"    => new Rectangle(f.RoomX,      f.RoomY,      100, 80),
            _          => new Rectangle(f.RoomX, f.RoomY, 80, 60)
        };

        // swap W/H for 90/270 rotation
        Rectangle col = (f.Rotation == 90 || f.Rotation == 270)
            ? new Rectangle(f.RoomX, f.RoomY, baseCol.Height, baseCol.Width)
            : baseCol;

        h.InteriorObjects.Add(col);
    }

    buildings.RemoveAll(b => b.BuildingName == houseName);
    buildings.Add(h);

    if (currentBuilding?.BuildingName == houseName)
        currentBuilding = h;
}

static void DrawRectRot(int cx, int cy, int w, int h, int rotation, Color col)
{
    float rad = rotation * MathF.PI / 180f;
    float cos = MathF.Cos(rad); float sin = MathF.Sin(rad);
    float hw = w / 2f; float hh = h / 2f;

    Vector2[] corners = {
        new Vector2(-hw, -hh), new Vector2(hw, -hh),
        new Vector2(hw,  hh),  new Vector2(-hw, hh)
    };
    Vector2[] rot = corners.Select(c =>
        new Vector2(cx + c.X * cos - c.Y * sin,
                    cy + c.X * sin + c.Y * cos)).ToArray();

    Raylib.DrawTriangle(rot[0], rot[1], rot[2], col);
    Raylib.DrawTriangle(rot[0], rot[2], rot[3], col);
}

static void DrawLineRot(int cx, int cy, int x1, int y1, int x2, int y2, int rotation, Color col)
{
    float rad = rotation * MathF.PI / 180f;
    float cos = MathF.Cos(rad); float sin = MathF.Sin(rad);
    Vector2 Rot(float x, float y) => new Vector2(
        cx + x * cos - y * sin,
        cy + x * sin + y * cos);
    var a = Rot(x1, y1); var b = Rot(x2, y2);
    Raylib.DrawLine((int)a.X, (int)a.Y, (int)b.X, (int)b.Y, col);
}

static void DrawCircRot(int cx, int cy, int ox, int oy, int radius, int rotation, Color col)
{
    float rad = rotation * MathF.PI / 180f;
    float cos = MathF.Cos(rad); float sin = MathF.Sin(rad);
    int nx = cx + (int)(ox * cos - oy * sin);
    int ny = cy + (int)(ox * sin + oy * cos);
    Raylib.DrawCircle(nx, ny, radius, col);
}

static void DrawFurniturePiece(string type, int x, int y)
{

    switch (type)
    {
        case "Sofa":
            // base
            Raylib.DrawRectangle(x, y + 20, 120, 50, new Color((byte)100,(byte)65,(byte)35,(byte)255));
            // back rest
            Raylib.DrawRectangle(x, y, 120, 24, new Color((byte)80,(byte)48,(byte)22,(byte)255));
            // seat cushions (3)
            Raylib.DrawRectangle(x + 4,  y + 24, 34, 36, new Color((byte)130,(byte)85,(byte)45,(byte)255));
            Raylib.DrawRectangle(x + 43, y + 24, 34, 36, new Color((byte)130,(byte)85,(byte)45,(byte)255));
            Raylib.DrawRectangle(x + 82, y + 24, 34, 36, new Color((byte)130,(byte)85,(byte)45,(byte)255));
            // arm rests
            Raylib.DrawRectangle(x,       y + 20, 8, 50, new Color((byte)70,(byte)42,(byte)18,(byte)255));
            Raylib.DrawRectangle(x + 112, y + 20, 8, 50, new Color((byte)70,(byte)42,(byte)18,(byte)255));
            // legs
            Raylib.DrawRectangle(x + 4,   y + 66, 8, 8, new Color((byte)50,(byte)30,(byte)10,(byte)255));
            Raylib.DrawRectangle(x + 108, y + 66, 8, 8, new Color((byte)50,(byte)30,(byte)10,(byte)255));
            break;

        case "Lamp":
            // base plate
            Raylib.DrawRectangle(x + 14, y + 52, 20, 6, new Color((byte)80,(byte)70,(byte)50,(byte)255));
            // pole
            Raylib.DrawRectangle(x + 21, y + 14, 6, 40, new Color((byte)100,(byte)90,(byte)65,(byte)255));
            // shade
            Raylib.DrawTriangle(
                new Vector2(x + 10, y + 14),
                new Vector2(x + 38, y + 14),
                new Vector2(x + 24, y),
                new Color((byte)220,(byte)200,(byte)120,(byte)255));
            Raylib.DrawRectangle(x + 10, y + 14, 28, 6,
                new Color((byte)200,(byte)180,(byte)100,(byte)255));
            // glow
            Raylib.DrawCircle(x + 24, y + 14, 14,
                new Color((byte)255,(byte)240,(byte)160,(byte)60));
            break;

        case "TV":
            // stand
            Raylib.DrawRectangle(x + 30, y + 52, 40, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangle(x + 44, y + 44, 12, 10, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            // screen bezel
            Raylib.DrawRectangle(x, y, 100, 48, new Color((byte)20,(byte)20,(byte)20,(byte)255));
            // screen
            Raylib.DrawRectangle(x + 4, y + 4, 92, 38, new Color((byte)10,(byte)20,(byte)60,(byte)255));
            // screen content (fake image lines)
            Raylib.DrawRectangle(x + 8,  y + 8,  84, 6,  new Color((byte)40,(byte)80,(byte)160,(byte)200));
            Raylib.DrawRectangle(x + 8,  y + 18, 50, 6,  new Color((byte)60,(byte)100,(byte)180,(byte)160));
            Raylib.DrawRectangle(x + 8,  y + 28, 70, 6,  new Color((byte)40,(byte)80,(byte)160,(byte)140));
            // power light
            Raylib.DrawCircle(x + 92, y + 42, 3,
                new Color((byte)0,(byte)220,(byte)80,(byte)255));
            break;

        case "Plant":
            // pot
            Raylib.DrawRectangle(x + 12, y + 34, 26, 24,
                new Color((byte)140,(byte)90,(byte)40,(byte)255));
            Raylib.DrawRectangle(x + 10, y + 30, 30, 8,
                new Color((byte)160,(byte)110,(byte)50,(byte)255));
            // soil
            Raylib.DrawRectangle(x + 14, y + 30, 22, 6,
                new Color((byte)60,(byte)35,(byte)10,(byte)255));
            // stem
            Raylib.DrawRectangle(x + 23, y + 10, 4, 22,
                new Color((byte)40,(byte)120,(byte)40,(byte)255));
            // leaves
            Raylib.DrawCircle(x + 25, y + 8,  12, new Color((byte)50,(byte)160,(byte)50,(byte)255));
            Raylib.DrawCircle(x + 14, y + 16, 9,  new Color((byte)40,(byte)140,(byte)40,(byte)255));
            Raylib.DrawCircle(x + 36, y + 16, 9,  new Color((byte)60,(byte)150,(byte)60,(byte)255));
            break;

        case "Shelf":
            // back board
            Raylib.DrawRectangle(x, y, 100, 70, new Color((byte)100,(byte)65,(byte)30,(byte)255));
            // shelves (3 planks)
            Raylib.DrawRectangle(x, y,      100, 8, new Color((byte)130,(byte)90,(byte)45,(byte)255));
            Raylib.DrawRectangle(x, y + 28, 100, 8, new Color((byte)130,(byte)90,(byte)45,(byte)255));
            Raylib.DrawRectangle(x, y + 56, 100, 8, new Color((byte)130,(byte)90,(byte)45,(byte)255));
            // side panels
            Raylib.DrawRectangle(x,      y, 6, 70, new Color((byte)80,(byte)50,(byte)20,(byte)255));
            Raylib.DrawRectangle(x + 94, y, 6, 70, new Color((byte)80,(byte)50,(byte)20,(byte)255));
            // decorative items on shelves
            Raylib.DrawRectangle(x + 10, y + 10, 12, 16, new Color((byte)180,(byte)60,(byte)60,(byte)255)); // book
            Raylib.DrawRectangle(x + 26, y + 10, 8,  16, new Color((byte)60,(byte)100,(byte)180,(byte)255)); // book
            Raylib.DrawCircle(x + 80, y + 18, 7, new Color((byte)200,(byte)160,(byte)80,(byte)255)); // ornament
            break;

        case "Rug":
            // rug base
            Raylib.DrawRectangle(x, y, 120, 80, new Color((byte)160,(byte)50,(byte)50,(byte)255));
            // border
            Raylib.DrawRectangleLines(x + 4, y + 4, 112, 72,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangleLines(x + 8, y + 8, 104, 64,
                new Color((byte)120,(byte)30,(byte)30,(byte)255));
            // centre pattern
            Raylib.DrawRectangle(x + 44, y + 28, 32, 24,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            Raylib.DrawCircle(x + 60, y + 40, 10,
                new Color((byte)220,(byte)100,(byte)100,(byte)255));
            // corner diamonds
            Raylib.DrawRectangle(x + 14, y + 14, 10, 10,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 96, y + 14, 10, 10,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 14, y + 56, 10, 10,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 96, y + 56, 10, 10,
                new Color((byte)200,(byte)80,(byte)80,(byte)255));
            break;

        case "Wall":
            // thick wall panel
            Raylib.DrawRectangle(x - 10, y, 120, 16, new Color((byte)150,(byte)150,(byte)145,(byte)255));
            Raylib.DrawRectangle(x - 10, y, 120, 4,  new Color((byte)180,(byte)180,(byte)175,(byte)255));
            Raylib.DrawRectangle(x - 10, y + 12, 120, 4, new Color((byte)110,(byte)110,(byte)105,(byte)255));
            // skirting board
            Raylib.DrawRectangle(x - 10, y + 14, 120, 6, new Color((byte)200,(byte)195,(byte)180,(byte)255));
            break;

        case "Toilet":
            // bowl
            Raylib.DrawEllipse(x + 22, y + 44, 18, 24, new Color((byte)225,(byte)225,(byte)220,(byte)255));
            Raylib.DrawEllipseLines(x + 22, y + 44, 18, 24, new Color((byte)180,(byte)180,(byte)175,(byte)255));
            // seat
            Raylib.DrawEllipse(x + 22, y + 38, 16, 20, new Color((byte)235,(byte)235,(byte)230,(byte)255));
            Raylib.DrawEllipseLines(x + 22, y + 38, 16, 20, new Color((byte)160,(byte)160,(byte)155,(byte)255));
            // cistern
            Raylib.DrawRectangle(x + 4, y, 36, 24, new Color((byte)225,(byte)225,(byte)220,(byte)255));
            Raylib.DrawRectangleLines(x + 4, y, 36, 24, new Color((byte)180,(byte)180,(byte)175,(byte)255));
            // flush button
            Raylib.DrawCircle(x + 22, y + 8, 5, new Color((byte)180,(byte)180,(byte)175,(byte)255));
            break;

        case "Table":
            // table top
            Raylib.DrawRectangle(x, y, 100, 60, new Color((byte)140,(byte)95,(byte)48,(byte)255));
            Raylib.DrawRectangle(x, y, 100, 5,  new Color((byte)170,(byte)120,(byte)65,(byte)255));
            // legs
            Raylib.DrawRectangle(x + 4,  y + 52, 8, 14, new Color((byte)100,(byte)65,(byte)28,(byte)255));
            Raylib.DrawRectangle(x + 88, y + 52, 8, 14, new Color((byte)100,(byte)65,(byte)28,(byte)255));
            // grain lines
            for (int g = 0; g < 4; g++)
                Raylib.DrawLine(x + 12 + g * 24, y + 5, x + 12 + g * 24, y + 52,
                    new Color((byte)120,(byte)80,(byte)38,(byte)80));
            break;

        case "Chair":
            // seat
            Raylib.DrawRectangle(x + 4,  y + 22, 44, 36, new Color((byte)100,(byte)68,(byte)30,(byte)255));
            // back rest
            Raylib.DrawRectangle(x + 4,  y,      44, 26, new Color((byte)80,(byte)52,(byte)20,(byte)255));
            // legs
            Raylib.DrawRectangle(x + 6,  y + 54, 6, 12, new Color((byte)70,(byte)44,(byte)14,(byte)255));
            Raylib.DrawRectangle(x + 40, y + 54, 6, 12, new Color((byte)70,(byte)44,(byte)14,(byte)255));
            // cushion
            Raylib.DrawRectangle(x + 8,  y + 26, 36, 26, new Color((byte)130,(byte)88,(byte)40,(byte)255));
            break;

        case "Fridge":
            // body
            Raylib.DrawRectangle(x, y, 60, 90, new Color((byte)205,(byte)210,(byte)215,(byte)255));
            Raylib.DrawRectangleLines(x, y, 60, 90, new Color((byte)160,(byte)165,(byte)170,(byte)255));
            // door divider
            Raylib.DrawRectangle(x, y + 50, 60, 3, new Color((byte)150,(byte)155,(byte)160,(byte)255));
            // handles
            Raylib.DrawRectangle(x + 50, y + 14, 6, 24, new Color((byte)140,(byte)145,(byte)150,(byte)255));
            Raylib.DrawRectangle(x + 50, y + 60, 6, 18, new Color((byte)140,(byte)145,(byte)150,(byte)255));
            // freezer section highlight
            Raylib.DrawRectangle(x + 3, y + 3,  54, 44, new Color((byte)215,(byte)220,(byte)228,(byte)255));
            Raylib.DrawRectangle(x + 3, y + 53, 54, 34, new Color((byte)215,(byte)218,(byte)220,(byte)255));
            break;

        case "Desk":
            // surface
            Raylib.DrawRectangle(x, y, 100, 55, new Color((byte)115,(byte)80,(byte)35,(byte)255));
            Raylib.DrawRectangle(x, y, 100, 5,  new Color((byte)145,(byte)105,(byte)55,(byte)255));
            // drawer unit
            Raylib.DrawRectangle(x + 70, y + 5, 26, 46, new Color((byte)95,(byte)65,(byte)28,(byte)255));
            // drawers
            Raylib.DrawRectangle(x + 72, y + 8,  22, 12, new Color((byte)110,(byte)76,(byte)34,(byte)255));
            Raylib.DrawRectangle(x + 72, y + 22, 22, 12, new Color((byte)110,(byte)76,(byte)34,(byte)255));
            Raylib.DrawRectangle(x + 72, y + 36, 22, 12, new Color((byte)110,(byte)76,(byte)34,(byte)255));
            // drawer handles
            Raylib.DrawRectangle(x + 80, y + 12, 6, 3, new Color((byte)160,(byte)130,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 80, y + 26, 6, 3, new Color((byte)160,(byte)130,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 80, y + 40, 6, 3, new Color((byte)160,(byte)130,(byte)60,(byte)255));
            // leg
            Raylib.DrawRectangle(x + 4, y + 52, 8, 14, new Color((byte)90,(byte)60,(byte)22,(byte)255));
            break;

        case "Stove":
            // body
            Raylib.DrawRectangle(x, y, 80, 70, new Color((byte)45,(byte)45,(byte)45,(byte)255));
            Raylib.DrawRectangleLines(x, y, 80, 70, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            // cooktop surface
            Raylib.DrawRectangle(x + 4, y + 4, 72, 44, new Color((byte)35,(byte)35,(byte)35,(byte)255));
            // burners (4)
            Raylib.DrawCircle(x + 20, y + 16, 10, new Color((byte)25,(byte)25,(byte)25,(byte)255));
            Raylib.DrawCircleLines(x + 20, y + 16, 10, new Color((byte)70,(byte)70,(byte)70,(byte)255));
            Raylib.DrawCircle(x + 58, y + 16, 10, new Color((byte)25,(byte)25,(byte)25,(byte)255));
            Raylib.DrawCircleLines(x + 58, y + 16, 10, new Color((byte)70,(byte)70,(byte)70,(byte)255));
            Raylib.DrawCircle(x + 20, y + 36, 10, new Color((byte)25,(byte)25,(byte)25,(byte)255));
            Raylib.DrawCircleLines(x + 20, y + 36, 10, new Color((byte)70,(byte)70,(byte)70,(byte)255));
            Raylib.DrawCircle(x + 58, y + 36, 10, new Color((byte)25,(byte)25,(byte)25,(byte)255));
            Raylib.DrawCircleLines(x + 58, y + 36, 10, new Color((byte)70,(byte)70,(byte)70,(byte)255));
            // oven door
            Raylib.DrawRectangle(x + 4, y + 50, 72, 16, new Color((byte)55,(byte)55,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 14, y + 54, 52, 8, new Color((byte)30,(byte)30,(byte)30,(byte)255));
            // knobs
            for (int k = 0; k < 4; k++)
                Raylib.DrawCircle(x + 10 + k * 18, y + 66, 4, new Color((byte)80,(byte)80,(byte)80,(byte)255));
            break;

        case "Cupboard":
            // body
            Raylib.DrawRectangle(x, y, 80, 80, new Color((byte)130,(byte)92,(byte)52,(byte)255));
            Raylib.DrawRectangleLines(x, y, 80, 80, new Color((byte)100,(byte)68,(byte)30,(byte)255));
            // doors (2)
            Raylib.DrawRectangle(x + 3,  y + 3, 35, 74, new Color((byte)145,(byte)105,(byte)58,(byte)255));
            Raylib.DrawRectangle(x + 42, y + 3, 35, 74, new Color((byte)145,(byte)105,(byte)58,(byte)255));
            // door divider
            Raylib.DrawRectangle(x + 38, y, 4, 80, new Color((byte)100,(byte)68,(byte)30,(byte)255));
            // handles
            Raylib.DrawRectangle(x + 32, y + 34, 5, 12, new Color((byte)180,(byte)145,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 43, y + 34, 5, 12, new Color((byte)180,(byte)145,(byte)60,(byte)255));
            break;

        case "Bench":
            // seat plank
            Raylib.DrawRectangle(x, y + 14, 100, 14, new Color((byte)170,(byte)125,(byte)68,(byte)255));
            Raylib.DrawRectangle(x, y + 14, 100, 3,  new Color((byte)195,(byte)150,(byte)85,(byte)255));
            // legs
            Raylib.DrawRectangle(x + 6,  y + 28, 8, 20, new Color((byte)130,(byte)90,(byte)40,(byte)255));
            Raylib.DrawRectangle(x + 86, y + 28, 8, 20, new Color((byte)130,(byte)90,(byte)40,(byte)255));
            // cross support
            Raylib.DrawRectangle(x + 14, y + 36, 72, 5, new Color((byte)130,(byte)90,(byte)40,(byte)255));
            break;
        
        case "Armchair":
            Raylib.DrawRectangle(x,      y + 18, 80, 46, new Color((byte)90,(byte)55,(byte)25,(byte)255));
            Raylib.DrawRectangle(x,      y,      80, 22, new Color((byte)70,(byte)42,(byte)18,(byte)255));
            Raylib.DrawRectangle(x,      y + 18, 10, 46, new Color((byte)60,(byte)35,(byte)12,(byte)255));
            Raylib.DrawRectangle(x + 70, y + 18, 10, 46, new Color((byte)60,(byte)35,(byte)12,(byte)255));
            Raylib.DrawRectangle(x + 10, y + 22, 60, 36, new Color((byte)115,(byte)75,(byte)35,(byte)255));
            break;

        case "BabyChair":
            Raylib.DrawRectangle(x + 10, y + 20, 30, 26, new Color((byte)200,(byte)160,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 10, y,      30, 22, new Color((byte)180,(byte)140,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 44, 6,  10, new Color((byte)160,(byte)120,(byte)40,(byte)255));
            Raylib.DrawRectangle(x + 36, y + 44, 6,  10, new Color((byte)160,(byte)120,(byte)40,(byte)255));
            Raylib.DrawRectangle(x + 4,  y + 24, 42, 6,  new Color((byte)220,(byte)180,(byte)100,(byte)255));
            break;

        case "CoffeeTable":
            Raylib.DrawRectangle(x,      y + 10, 90, 40, new Color((byte)120,(byte)80,(byte)38,(byte)255));
            Raylib.DrawRectangle(x,      y + 10, 90, 4,  new Color((byte)150,(byte)105,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 46, 6,  12, new Color((byte)90,(byte)58,(byte)20,(byte)255));
            Raylib.DrawRectangle(x + 78, y + 46, 6,  12, new Color((byte)90,(byte)58,(byte)20,(byte)255));
            break;

        case "NightStand":
            Raylib.DrawRectangle(x,      y,      60, 70, new Color((byte)115,(byte)78,(byte)36,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     60, 70, new Color((byte)85,(byte)55,(byte)20,(byte)255));
            Raylib.DrawRectangle(x + 3,  y + 4,  54, 28, new Color((byte)130,(byte)90,(byte)44,(byte)255));
            Raylib.DrawRectangle(x + 3,  y + 36, 54, 28, new Color((byte)130,(byte)90,(byte)44,(byte)255));
            Raylib.DrawRectangle(x + 36, y + 14, 8,  6,  new Color((byte)180,(byte)145,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 36, y + 46, 8,  6,  new Color((byte)180,(byte)145,(byte)60,(byte)255));
            break;

        case "Sink":
            Raylib.DrawRectangle(x,      y,      70, 55, new Color((byte)195,(byte)200,(byte)205,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     70, 55, new Color((byte)150,(byte)155,(byte)160,(byte)255));
            Raylib.DrawEllipse(x + 35, y + 30, 22, 16, new Color((byte)170,(byte)180,(byte)190,(byte)255));
            Raylib.DrawEllipseLines(x + 35, y + 30, 22, 16, new Color((byte)120,(byte)130,(byte)140,(byte)255));
            Raylib.DrawCircle(x + 35, y + 12, 4, new Color((byte)160,(byte)165,(byte)170,(byte)255));
            Raylib.DrawRectangle(x + 33, y + 8,  4, 8,  new Color((byte)140,(byte)145,(byte)150,(byte)255));
            break;

        case "KitchenBench":
            Raylib.DrawRectangle(x,      y,      110, 46, new Color((byte)175,(byte)165,(byte)135,(byte)255));
            Raylib.DrawRectangle(x,      y,      110, 5,  new Color((byte)200,(byte)190,(byte)155,(byte)255));
            Raylib.DrawRectangle(x,      y + 42, 110, 8,  new Color((byte)140,(byte)130,(byte)100,(byte)255));
            for (int g = 0; g < 3; g++)
                Raylib.DrawLine(x + 20 + g * 36, y + 5, x + 20 + g * 36, y + 40,
                    new Color((byte)155,(byte)145,(byte)115,(byte)80));
            break;

        case "Dishwasher":
            Raylib.DrawRectangle(x,      y,      65, 80, new Color((byte)185,(byte)190,(byte)195,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     65, 80, new Color((byte)145,(byte)150,(byte)155,(byte)255));
            Raylib.DrawRectangle(x + 5,  y + 10, 55, 55, new Color((byte)170,(byte)175,(byte)180,(byte)255));
            Raylib.DrawRectangleLines(x + 5, y + 10, 55, 55, new Color((byte)130,(byte)135,(byte)140,(byte)255));
            Raylib.DrawCircle(x + 54, y + 6, 4, new Color((byte)80,(byte)200,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 20, y + 70, 26, 6, new Color((byte)155,(byte)160,(byte)165,(byte)255));
            break;

        case "PCDesk":
            // desk surface
            Raylib.DrawRectangle(x,      y + 30, 110, 40, new Color((byte)80,(byte)60,(byte)30,(byte)255));
            Raylib.DrawRectangle(x,      y + 30, 110, 4,  new Color((byte)110,(byte)85,(byte)45,(byte)255));
            // monitor
            Raylib.DrawRectangle(x + 10, y,      80, 32, new Color((byte)20,(byte)20,(byte)20,(byte)255));
            Raylib.DrawRectangle(x + 13, y + 2,  74, 26, new Color((byte)15,(byte)30,(byte)80,(byte)255));
            // stand
            Raylib.DrawRectangle(x + 46, y + 30, 8,  6,  new Color((byte)30,(byte)30,(byte)30,(byte)255));
            // keyboard
            Raylib.DrawRectangle(x + 20, y + 38, 60, 14, new Color((byte)40,(byte)40,(byte)45,(byte)255));
            Raylib.DrawRectangleLines(x + 20, y + 38, 60, 14, new Color((byte)55,(byte)55,(byte)60,(byte)255));
            // mouse
            Raylib.DrawEllipse(x + 92, y + 44, 7, 10, new Color((byte)35,(byte)35,(byte)38,(byte)255));
            break;

        case "Speaker":
            Raylib.DrawRectangle(x,      y,      36, 70, new Color((byte)22,(byte)22,(byte)22,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     36, 70, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawCircle(x + 18, y + 22, 14, new Color((byte)35,(byte)35,(byte)35,(byte)255));
            Raylib.DrawCircle(x + 18, y + 22, 8,  new Color((byte)15,(byte)15,(byte)15,(byte)255));
            Raylib.DrawCircle(x + 18, y + 22, 3,  new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 10, y + 52, 16, 4, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawCircle(x + 18, y + 62, 4, new Color((byte)60,(byte)60,(byte)60,(byte)255));
            break;

        case "BigPlant":
            Raylib.DrawRectangle(x + 18, y + 52, 30, 34, new Color((byte)150,(byte)95,(byte)45,(byte)255));
            Raylib.DrawRectangle(x + 15, y + 48, 36, 8,  new Color((byte)170,(byte)115,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 28, y + 16, 10, 36, new Color((byte)45,(byte)125,(byte)45,(byte)255));
            Raylib.DrawCircle(x + 33, y + 12, 20, new Color((byte)40,(byte)140,(byte)40,(byte)255));
            Raylib.DrawCircle(x + 16, y + 22, 14, new Color((byte)35,(byte)120,(byte)35,(byte)255));
            Raylib.DrawCircle(x + 50, y + 22, 14, new Color((byte)50,(byte)145,(byte)50,(byte)255));
            Raylib.DrawCircle(x + 33, y + 30, 12, new Color((byte)55,(byte)155,(byte)55,(byte)255));
            break;

        case "Cactus":
            Raylib.DrawRectangle(x + 16, y + 52, 18, 28, new Color((byte)145,(byte)95,(byte)45,(byte)255));
            Raylib.DrawRectangle(x + 13, y + 48, 24, 8,  new Color((byte)165,(byte)110,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 18, y + 12, 14, 40, new Color((byte)60,(byte)135,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 22, 12, 20, new Color((byte)60,(byte)130,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 32, y + 26, 12, 16, new Color((byte)60,(byte)130,(byte)55,(byte)255));
            for (int s = 0; s < 4; s++)
                Raylib.DrawRectangle(x + 16 + s * 3, y + 14 + s * 6, 2, 4,
                    new Color((byte)200,(byte)200,(byte)180,(byte)255));
            break;

        case "FlowerPot":
            Raylib.DrawRectangle(x + 8,  y + 36, 24, 24, new Color((byte)180,(byte)75,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 32, 28, 8,  new Color((byte)200,(byte)95,(byte)70,(byte)255));
            Raylib.DrawRectangle(x + 18, y + 18, 4,  18, new Color((byte)45,(byte)120,(byte)45,(byte)255));
            Raylib.DrawCircle(x + 20, y + 14, 10, new Color((byte)220,(byte)80,(byte)100,(byte)255));
            Raylib.DrawCircle(x + 20, y + 14, 5,  new Color((byte)255,(byte)220,(byte)50,(byte)255));
            break;

        case "HalfWall":
            Raylib.DrawRectangle(x,      y + 28, 120, 22, new Color((byte)145,(byte)145,(byte)140,(byte)255));
            Raylib.DrawRectangle(x,      y + 28, 120, 4,  new Color((byte)175,(byte)175,(byte)170,(byte)255));
            Raylib.DrawRectangle(x,      y + 48, 120, 4,  new Color((byte)105,(byte)105,(byte)100,(byte)255));
            for (int b = 0; b < 3; b++)
                Raylib.DrawRectangleLines(x + 2 + b * 40, y + 30, 36, 18,
                    new Color((byte)120,(byte)120,(byte)115,(byte)100));
            break;

        case "WallV":
            // vertical wall — tall and narrow
            Raylib.DrawRectangle(x - 2, y,      16,  90, new Color((byte)150,(byte)150,(byte)145,(byte)255));
            Raylib.DrawRectangle(x - 2, y,       4,  90, new Color((byte)180,(byte)180,(byte)175,(byte)255));
            Raylib.DrawRectangle(x - 2, y,       4,  90, new Color((byte)105,(byte)105,(byte)100,(byte)255));
            Raylib.DrawRectangle(x - 2, y,      16,   4, new Color((byte)200,(byte)195,(byte)180,(byte)255));
            break;

        case "HalfWallV":
            // vertical half wall
            Raylib.DrawRectangle(x + 52, y + 20, 16,  50, new Color((byte)145,(byte)145,(byte)140,(byte)255));
            Raylib.DrawRectangle(x + 52, y + 20,  4,  50, new Color((byte)175,(byte)175,(byte)170,(byte)255));
            Raylib.DrawRectangle(x + 64, y + 20,  4,  50, new Color((byte)105,(byte)105,(byte)100,(byte)255));
            Raylib.DrawRectangle(x + 52, y + 20, 16,   4, new Color((byte)200,(byte)195,(byte)180,(byte)255));
            break;

        case "Bathtub":
            Raylib.DrawRectangle(x,      y + 10, 110, 70, new Color((byte)215,(byte)220,(byte)225,(byte)255));
            Raylib.DrawRectangleLines(x,  y + 10, 110, 70, new Color((byte)170,(byte)175,(byte)180,(byte)255));
            Raylib.DrawRectangle(x + 8,  y + 18, 94, 54,  new Color((byte)185,(byte)210,(byte)225,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 6,  30, 10,  new Color((byte)195,(byte)200,(byte)205,(byte)255));
            Raylib.DrawCircle(x + 18, y + 10, 5, new Color((byte)155,(byte)160,(byte)165,(byte)255));
            Raylib.DrawRectangle(x + 15, y + 6,  6,  8,   new Color((byte)140,(byte)145,(byte)150,(byte)255));
            break;

        case "Shower":
            Raylib.DrawRectangle(x,      y,      80, 90, new Color((byte)195,(byte)205,(byte)210,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     80, 90, new Color((byte)155,(byte)165,(byte)170,(byte)255));
            Raylib.DrawRectangle(x + 4,  y + 4,  72, 82, new Color((byte)180,(byte)200,(byte)210,(byte)200));
            Raylib.DrawCircle(x + 65, y + 20, 8, new Color((byte)155,(byte)160,(byte)165,(byte)255));
            Raylib.DrawRectangle(x + 62, y + 20, 4, 30, new Color((byte)145,(byte)150,(byte)155,(byte)255));
            for (int d = 0; d < 4; d++)
                Raylib.DrawCircle(x + 62 + (d%2)*4, y + 22 + d*5, 2,
                    new Color((byte)150,(byte)200,(byte)220,(byte)180));
            break;

        case "Painting":
            Raylib.DrawRectangle(x,      y,      80, 60, new Color((byte)70,(byte)50,(byte)25,(byte)255));
            Raylib.DrawRectangle(x + 6,  y + 6,  68, 48, new Color((byte)80,(byte)120,(byte)160,(byte)255));
            Raylib.DrawCircle(x + 40, y + 30, 16, new Color((byte)220,(byte)180,(byte)60,(byte)255));
            Raylib.DrawCircle(x + 40, y + 30, 8,  new Color((byte)255,(byte)220,(byte)80,(byte)255));
            Raylib.DrawRectangle(x + 8,  y + 8,  30, 20, new Color((byte)50,(byte)90,(byte)140,(byte)255));
            break;

        case "Mirror":
            Raylib.DrawRectangle(x,      y,      55, 80, new Color((byte)70,(byte)55,(byte)30,(byte)255));
            Raylib.DrawRectangle(x + 5,  y + 5,  45, 70, new Color((byte)180,(byte)205,(byte)215,(byte)255));
            Raylib.DrawRectangle(x + 5,  y + 5,  45, 3,  new Color((byte)220,(byte)235,(byte)240,(byte)180));
            Raylib.DrawRectangle(x + 5,  y + 5,  3,  70, new Color((byte)210,(byte)230,(byte)238,(byte)120));
            break;

        case "Bin":
            Raylib.DrawRectangle(x + 10, y + 14, 30, 42, new Color((byte)55,(byte)55,(byte)55,(byte)255));
            Raylib.DrawRectangle(x + 8,  y + 10, 34, 8,  new Color((byte)70,(byte)70,(byte)70,(byte)255));
            Raylib.DrawRectangle(x + 14, y + 6,  22, 8,  new Color((byte)60,(byte)60,(byte)60,(byte)255));
            Raylib.DrawRectangle(x + 16, y + 14, 18, 4,  new Color((byte)45,(byte)45,(byte)45,(byte)255));
            break;

        case "Fireplace":
            Raylib.DrawRectangle(x,      y,      100, 80, new Color((byte)75,(byte)38,(byte)18,(byte)255));
            Raylib.DrawRectangleLines(x,  y,     100, 80, new Color((byte)55,(byte)28,(byte)10,(byte)255));
            Raylib.DrawRectangle(x + 15, y + 14, 70, 52, new Color((byte)20,(byte)18,(byte)15,(byte)255));
            // flames
            Raylib.DrawTriangle(new Vector2(x+30, y+62), new Vector2(x+50, y+20), new Vector2(x+70, y+62),
                new Color((byte)220,(byte)80,(byte)20,(byte)255));
            Raylib.DrawTriangle(new Vector2(x+38, y+62), new Vector2(x+52, y+30), new Vector2(x+66, y+62),
                new Color((byte)240,(byte)150,(byte)20,(byte)255));
            Raylib.DrawTriangle(new Vector2(x+44, y+62), new Vector2(x+52, y+40), new Vector2(x+60, y+62),
                new Color((byte)255,(byte)220,(byte)60,(byte)255));
            // mantle
            Raylib.DrawRectangle(x - 4, y - 8, 108, 14, new Color((byte)90,(byte)48,(byte)22,(byte)255));
            break;

        default:
            // fallback plain rectangle with name
            Raylib.DrawRectangle(x, y, 80, 60, Color.Gray);
            Program.DrawTextUI(type, x + 4, y + 20, 13, Color.White);
            break;
    }
}

static (int cost, Color col) GetFurnitureTemplate(string type) => type switch
{
    "Sofa"         => (800,  new Color((byte)120,(byte)80, (byte)40, (byte)255)),
    "Chair"        => (180,  new Color((byte)80, (byte)55, (byte)25, (byte)255)),
    "Bench"        => (280,  new Color((byte)160,(byte)120,(byte)70, (byte)255)),
    "Armchair"     => (420,  new Color((byte)100,(byte)60, (byte)30, (byte)255)),
    "BabyChair"    => (150,  new Color((byte)200,(byte)160,(byte)80, (byte)255)),
    "Table"        => (350,  new Color((byte)120,(byte)80, (byte)40, (byte)255)),
    "Desk"         => (450,  new Color((byte)100,(byte)70, (byte)30, (byte)255)),
    "CoffeeTable"  => (280,  new Color((byte)100,(byte)65, (byte)30, (byte)255)),
    "NightStand"   => (200,  new Color((byte)110,(byte)75, (byte)35, (byte)255)),
    "Shelf"        => (400,  new Color((byte)140,(byte)100,(byte)60, (byte)255)),
    "Stove"        => (750,  new Color((byte)50, (byte)50, (byte)50, (byte)255)),
    "Fridge"       => (900,  new Color((byte)210,(byte)215,(byte)220,(byte)255)),
    "Cupboard"     => (380,  new Color((byte)140,(byte)100,(byte)60, (byte)255)),
    "Sink"         => (320,  new Color((byte)200,(byte)205,(byte)210,(byte)255)),
    "KitchenBench" => (300,  new Color((byte)180,(byte)170,(byte)140,(byte)255)),
    "Dishwasher"   => (600,  new Color((byte)190,(byte)195,(byte)200,(byte)255)),
    "TV"           => (1200, new Color((byte)30, (byte)30, (byte)30, (byte)255)),
    "Lamp"         => (200,  new Color((byte)255,(byte)220,(byte)80, (byte)255)),
    "PCDesk"       => (900,  new Color((byte)30, (byte)30, (byte)40, (byte)255)),
    "Speaker"      => (350,  new Color((byte)25, (byte)25, (byte)25, (byte)255)),
    "Plant"        => (150,  new Color((byte)40, (byte)140,(byte)40, (byte)255)),
    "BigPlant"     => (280,  new Color((byte)30, (byte)120,(byte)30, (byte)255)),
    "Cactus"       => (120,  new Color((byte)60, (byte)130,(byte)50, (byte)255)),
    "FlowerPot"    => (90,   new Color((byte)180,(byte)80, (byte)60, (byte)255)),
    "Wall"         => (500,  new Color((byte)160,(byte)160,(byte)155,(byte)255)),
    "HalfWall"     => (300,  new Color((byte)150,(byte)150,(byte)145,(byte)255)),
    "WallV"        => (500,  new Color((byte)150,(byte)150,(byte)145,(byte)255)),
    "HalfWallV"    => (300,  new Color((byte)145,(byte)145,(byte)140,(byte)255)),
    "Toilet"       => (600,  new Color((byte)230,(byte)230,(byte)225,(byte)255)),
    "Bathtub"      => (800,  new Color((byte)220,(byte)225,(byte)230,(byte)255)),
    "Shower"       => (700,  new Color((byte)200,(byte)210,(byte)215,(byte)255)),
    "Rug"          => (300,  new Color((byte)180,(byte)60, (byte)60, (byte)255)),
    "Painting"     => (250,  new Color((byte)80, (byte)60, (byte)40, (byte)255)),
    "Mirror"       => (180,  new Color((byte)180,(byte)200,(byte)210,(byte)255)),
    "Bin"          => (60,   new Color((byte)60, (byte)60, (byte)60, (byte)255)),
    "Fireplace"    => (950,  new Color((byte)80, (byte)40, (byte)20, (byte)255)),
    _              => (0,    Color.Gray)
};

static void DrawEggIcon(string egg, int cx, int cy, int size, bool rareGlow = false, float glowPhase = 0f)
{
    Color shell = EggColor(egg);
    Color light = new Color(
        (byte)Math.Min(255, shell.R + 50),
        (byte)Math.Min(255, shell.G + 50),
        (byte)Math.Min(255, shell.B + 50), (byte)255);

    // pulsing yellow rare-drop glow (drawn behind the egg)
    if (rareGlow)
    {
        float pulse = (MathF.Sin(glowPhase * 4f) + 1f) * 0.5f;   // 0→1
        for (int g = 3; g >= 1; g--)
        {
            byte ga = (byte)((22 + 30 * pulse) * g / 3f);
            Raylib.DrawCircle(cx, cy, size * (0.7f + 0.12f * g),
                new Color((byte)255,(byte)220,(byte)60, ga));
        }
    }

    // egg body (taller than wide)
    Raylib.DrawEllipse(cx, cy, size / 2, (int)(size * 0.62f), shell);
    Raylib.DrawEllipse(cx - size / 8, cy - size / 6, size / 8, size / 5, light); // highlight
    // speckles
    Raylib.DrawCircle(cx + size / 6, cy + size / 8, 2, light);
    Raylib.DrawCircle(cx - size / 5, cy + size / 4, 2, light);

    // bright golden outline when rare
    Color outline = rareGlow
        ? new Color((byte)255,(byte)215,(byte)40,(byte)255)
        : new Color((byte)0,(byte)0,(byte)0,(byte)70);
    Raylib.DrawEllipseLines(cx, cy, size / 2, (int)(size * 0.62f), outline);
}

static void DrawSpeechBubbleScreen(int sx, int sy, string text, float alphaPct = 1f)
{
    int padding  = 8;
    int fontSize = 15;
    int textWidth = Program.MeasureTextUI(text, fontSize);
    int bubbleW  = Math.Min(textWidth + padding * 2, 400);
    int bubbleH  = fontSize + padding * 2;
    int bx = sx - bubbleW / 2;
    int by = sy - bubbleH - 18;
    byte alpha = (byte)(230 * Math.Clamp(alphaPct, 0f, 1f));

    Raylib.DrawRectangle(bx, by, bubbleW, bubbleH,
        new Color((byte)255,(byte)255,(byte)255,(byte)alpha));
    Raylib.DrawRectangleLines(bx, by, bubbleW, bubbleH,
        new Color((byte)80,(byte)80,(byte)80,(byte)alpha));
    // tail
    Raylib.DrawTriangle(
        new Vector2(bx + bubbleW / 2 - 6, by + bubbleH),
        new Vector2(bx + bubbleW / 2 + 6, by + bubbleH),
        new Vector2(bx + bubbleW / 2,     by + bubbleH + 10),
        new Color((byte)255,(byte)255,(byte)255,(byte)alpha));
    // text — truncate if too long
    string display = textWidth > 380 ? text[..Math.Min(text.Length, 28)] + "..." : text;
    Program.DrawTextUI(display, bx + padding, by + padding, fontSize,
        new Color((byte)20,(byte)20,(byte)20,(byte)alpha));
}

        static bool IsNearRoad(Vector2 pos, int buffer = 80)
        {
            // main horizontal road
            if (pos.Y >= 540 - buffer && pos.Y <= 743 + buffer) return true;

            // north/south highway
            if (pos.X >= 188 - buffer && pos.X <= 333 + buffer) return true;

            // desert side road
            if (pos.Y >= 188 - buffer && pos.Y <= 333 + buffer && pos.X >= 4000 - buffer) return true;

            // snow side road
            if (pos.Y >= 188 - buffer && pos.Y <= 333 + buffer && pos.X <= -3000 + buffer) return true;

            // ring road top
            if (pos.Y >= -38020 - buffer && pos.Y <= -37820 + buffer) return true;

            // ring road bottom
            if (pos.Y >= 38000 - buffer && pos.Y <= 38200 + buffer) return true;

            // ring road left
            if (pos.X >= -40000 - buffer && pos.X <= -39820 + buffer) return true;

            // ring road right
            if (pos.X >= 39820 - buffer && pos.X <= 40000 + buffer) return true;

            // snow vertical connectors
            if (pos.X >= -20015 - buffer && pos.X <= -19865 + buffer) return true;
            if (pos.X >= -10015 - buffer && pos.X <= -9865 + buffer) return true;

            // desert vertical connectors
            if (pos.X >= 14985 - buffer && pos.X <= 15135 + buffer) return true;
            if (pos.X >= 24985 - buffer && pos.X <= 25135 + buffer) return true;

            // vertical roads to buildings
            if (pos.X >= 1020 - buffer && pos.X <= 1140 + buffer) return true;
            if (pos.X >= -1260 - buffer && pos.X <= -1140 + buffer) return true;
            if (pos.X >= 200 - buffer && pos.X <= 320 + buffer && pos.Y <= 550 + buffer) return true;
            if (pos.X >= 500 - buffer && pos.X <= 620 + buffer && pos.Y <= 550 + buffer) return true;
            if (CarparkManager.IsNearCarpark(pos, buffer)) return true;
            if (RoadManager.IsNearRoad(pos, buffer)) return true;
            return false;
        }

        static bool IsNearBuilding(Vector2 pos, float buffer = 150f)
        {
            foreach (var b in buildings)
            {
                Rectangle expanded = new Rectangle(b.Bounds.X - buffer, b.Bounds.Y - buffer,
                                                    b.Bounds.Width + buffer * 2, b.Bounds.Height + buffer * 2);
                if (Raylib.CheckCollisionPointRec(pos, expanded)) return true;
            }
            return false;
        }

        public static bool IsOnRoad(Vector2 pos)
{

    // gas station forecourt
    if (pos.X >= 300 && pos.X <= 1000 && pos.Y >= -1000 && pos.Y <= -420) return true;

    // main horizontal road + sidewalks
    if (pos.Y >= 540 && pos.Y <= 743) return true;

    // north/south highway + sidewalks
    if (pos.X >= 188 && pos.X <= 333) return true;

    // desert side road + sidewalks
    if (pos.Y >= 188 && pos.Y <= 333 && pos.X >= 4000) return true;

    // snow side road + sidewalks
    if (pos.Y >= 188 && pos.Y <= 333 && pos.X <= -3000) return true;

    // ring road top
    if (pos.Y >= -38020 && pos.Y <= -37820) return true;

    // ring road bottom
    if (pos.Y >= 38000 && pos.Y <= 38200) return true;

    // ring road left
    if (pos.X >= -40000 && pos.X <= -39800) return true;

    // ring road right
    if (pos.X >= 39800 && pos.X <= 40000) return true;

    // snow vertical connectors
    if (pos.X >= -20015 && pos.X <= -19865) return true;
    if (pos.X >= -10015 && pos.X <= -9865) return true;

    // desert vertical connectors
    if (pos.X >= 14985 && pos.X <= 15135) return true;
    if (pos.X >= 24985 && pos.X <= 25135) return true;

    // ── City of Hamiltron ─────────────────────────────────────────────────────
    if (pos.Y >= 5500 && pos.Y <= 5660 && pos.X >= 11800 && pos.X <= 18200) return true; // boulevard
    if (pos.X >= 14800 && pos.X <= 14960 && pos.Y >= 3000 && pos.Y <= 8200) return true; // north ave
    if (pos.Y >= 3900 && pos.Y <= 4020 && pos.X >= 11800 && pos.X <= 18200) return true; // cross N
    if (pos.Y >= 7200 && pos.Y <= 7320 && pos.X >= 11800 && pos.X <= 18200) return true; // cross S
    if (pos.X >= 12800 && pos.X <= 12920 && pos.Y >= 3000 && pos.Y <= 8200) return true; // cross W
    if (pos.X >= 16600 && pos.X <= 16720 && pos.Y >= 3000 && pos.Y <= 8200) return true; // cross E
    if (pos.Y >= 3000 && pos.Y <= 3120 && pos.X >= 11800 && pos.X <= 18000) return true; // ring N
    if (pos.Y >= 8100 && pos.Y <= 8220 && pos.X >= 11800 && pos.X <= 18000) return true; // ring S
    if (pos.X >= 11800 && pos.X <= 11920 && pos.Y >= 3000 && pos.Y <= 8220) return true; // ring W
    if (pos.X >= 17880 && pos.X <= 18000 && pos.Y >= 3000 && pos.Y <= 8220) return true; // ring E
    if (pos.X >= 14800 && pos.X <= 14960 && pos.Y >= 200  && pos.Y <= 3000) return true; // connector

    // ── Country Town Rotoaira ─────────────────────────────────────────────────
    if (pos.Y >= 4400 && pos.Y <= 4540 && pos.X >= -18000 && pos.X <= -13800) return true; // main st
    if (pos.X >= -17200 && pos.X <= -17080 && pos.Y >= 3200 && pos.Y <= 6200) return true; // side L
    if (pos.X >= -15400 && pos.X <= -15280 && pos.Y >= 3200 && pos.Y <= 6200) return true; // side R
    if (pos.Y >= 3200 && pos.Y <= 3320 && pos.X >= -18000 && pos.X <= -13800) return true; // loop N
    if (pos.Y >= 6100 && pos.Y <= 6220 && pos.X >= -18000 && pos.X <= -13800) return true; // loop S
    if (pos.X >= -18000 && pos.X <= -17880 && pos.Y >= 3200 && pos.Y <= 6220) return true; // loop W
    if (pos.X >= -13920 && pos.X <= -13800 && pos.Y >= 3200 && pos.Y <= 6220) return true; // loop E
    if (pos.X >= -16200 && pos.X <= -16080 && pos.Y >= 260  && pos.Y <= 3320) return true; // connector
    if (CarparkManager.IsOnCarparkSurface(pos)) return true;
    if (RoadManager.IsOnRoadSurface(pos)) return true;
    

    return false;
}

        static void DrawStreetLight(float x, float y)
{
    // pole
    Raylib.DrawRectangle((int)x, (int)y, 6, 40, new Color((byte)80,(byte)80,(byte)80,(byte)255));

    // lamp head
    Raylib.DrawRectangle((int)x - 8, (int)y - 8, 22, 10, new Color((byte)60,(byte)60,(byte)60,(byte)255));

    // glow at night
    float darkness = GetDarkness();
    if (darkness > 0.05f)
    {
        byte glowAlpha = (byte)(180 * darkness);
        Raylib.DrawCircle((int)x + 3, (int)y - 3, 60, new Color((byte)255,(byte)220,(byte)100,(byte)(glowAlpha / 4)));
        Raylib.DrawCircle((int)x + 3, (int)y - 3, 30, new Color((byte)255,(byte)220,(byte)100,(byte)(glowAlpha / 2)));
        Raylib.DrawRectangle((int)x - 8, (int)y - 8, 22, 10, new Color((byte)255,(byte)220,(byte)100,(byte)glowAlpha));
    }
}

        static void DrawStreetLights()
    {
        
    }

        static void DrawWardrobe()
{
    if (!wardrobeOpen) return;

    int wx = ScreenWidth / 2 - 300;
    int wy = 100;

    // background
    Raylib.DrawRectangle(wx, wy, 600, 480, new Color((byte)20, (byte)20, (byte)30, (byte)240));
    Raylib.DrawRectangleLines(wx, wy, 600, 480, Color.Gold);
    Program.DrawTextUI("WARDROBE", wx + 220, wy + 15, 32, Color.Gold);

    // tabs
    string[] tabs = { "SHIRT", "SKIN", "PANTS" };
    for (int i = 0; i < 3; i++)
    {
        Color tabColor = wardrobeTab == i ? Color.Gold : Color.White;
        Raylib.DrawRectangle(wx + 20 + i * 140, wy + 60, 120, 36, new Color((byte)40, (byte)40, (byte)40, (byte)255));
        Raylib.DrawRectangleLines(wx + 20 + i * 140, wy + 60, 120, 36, tabColor);
        Program.DrawTextUI(tabs[i], wx + 40 + i * 140, wy + 70, 20, tabColor);
    }

    // color options
    Color[][] colorOptions = {
        new Color[] { Color.Blue, Color.Red, Color.Green, Color.Black, Color.White, Color.Purple, Color.Orange, Color.Yellow },
        new Color[] { Color.Beige, new Color((byte)210,(byte)160,(byte)110,(byte)255), new Color((byte)150,(byte)100,(byte)60,(byte)255), new Color((byte)80,(byte)50,(byte)30,(byte)255) },
        new Color[] { Color.Black, new Color((byte)80,(byte)50,(byte)20,(byte)255), new Color((byte)30,(byte)50,(byte)100,(byte)255), new Color((byte)80,(byte)80,(byte)80,(byte)255) }
    };

    string[][] colorNames = {
        new string[] { "Blue", "Red", "Green", "Black", "White", "Purple", "Orange", "Yellow" },
        new string[] { "Light", "Medium", "Dark", "Deep" },
        new string[] { "Black", "Brown", "Navy", "Grey" }
    };

    Program.DrawTextUI("SELECT COLOR:", wx + 20, wy + 115, 22, Color.LightGray);

    for (int i = 0; i < colorOptions[wardrobeTab].Length; i++)
    {
        int cx = wx + 20 + (i % 4) * 140;
        int cy = wy + 150 + (i / 4) * 100;

        Raylib.DrawRectangle(cx, cy, 100, 60, colorOptions[wardrobeTab][i]);
        Raylib.DrawRectangleLines(cx, cy, 100, 60, Color.White);
        Program.DrawTextUI(colorNames[wardrobeTab][i], cx + 4, cy + 66, 16, Color.LightGray);

        Vector2 mouse = Raylib.GetMousePosition();
        if (Raylib.CheckCollisionPointRec(mouse, new Rectangle(cx, cy, 100, 60)))
        {
            Raylib.DrawRectangleLines(cx, cy, 100, 60, Color.Gold);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (wardrobeTab == 0) player.ShirtColor = colorOptions[wardrobeTab][i];
                else if (wardrobeTab == 1) player.SkinColor = colorOptions[wardrobeTab][i];
                else if (wardrobeTab == 2) player.PantsColor = colorOptions[wardrobeTab][i];
            }
        }
    }

    // player preview
    Program.DrawTextUI("PREVIEW", wx + 460, wy + 115, 22, Color.LightGray);
    Raylib.DrawCircle(wx + 510, wy + 200, 20, player.SkinColor);
    Raylib.DrawRectangle(wx + 493, wy + 220, 34, 50, player.ShirtColor);
    Raylib.DrawRectangle(wx + 493, wy + 270, 14, 20, player.PantsColor);
    Raylib.DrawRectangle(wx + 510, wy + 270, 14, 20, player.PantsColor);

    // close button
    Raylib.DrawRectangle(wx + 220, wy + 420, 160, 40, new Color((byte)40, (byte)40, (byte)40, (byte)255));
    Raylib.DrawRectangleLines(wx + 220, wy + 420, 160, 40, Color.White);
    Program.DrawTextUI("CLOSE (Q)", wx + 240, wy + 432, 20, Color.White);
}
        static void TriggerShake(float duration) => shakeDuration = duration;
        // bonus damage scales gently with combo (every 5 hits = +1 flat)
        static int ComboDamageBonus() => comboCount / 5;

static void DrawPlacedProps()
{
    foreach (var kv in stationProps)
        foreach (var p in kv.Value)
        {
            int x = (int)p.X, y = (int)p.Y;
            bool adv = kv.Key.StartsWith("Advanced");
            Color accent = adv ? Color.Gold : new Color((byte)70,(byte)70,(byte)78,(byte)255);
            if (kv.Key.Contains("Anvil"))
            {
                Raylib.DrawRectangle(x - 18, y - 8, 36, 10, new Color((byte)55,(byte)55,(byte)62,(byte)255)); // top
                Raylib.DrawTriangle(new Vector2(x - 18, y - 8), new Vector2(x - 30, y - 3), new Vector2(x - 18, y + 2),
                    new Color((byte)55,(byte)55,(byte)62,(byte)255));                                          // horn
                Raylib.DrawRectangle(x - 8, y + 2, 16, 12, new Color((byte)45,(byte)45,(byte)50,(byte)255));   // base
                Raylib.DrawRectangleLines(x - 18, y - 8, 36, 10, accent);
            }
            else if (kv.Key == "Advanced Furnace")
            {
                Raylib.DrawRectangle(x - 24, y - 24, 48, 46, new Color((byte)70,(byte)70,(byte)80,(byte)255));
                Raylib.DrawRectangle(x - 11, y - 2, 22, 18, new Color((byte)25,(byte)25,(byte)25,(byte)255));
                Raylib.DrawCircle(x, y + 8, 7, new Color((byte)120,(byte)200,(byte)255,(byte)255));            // blue-hot
                Raylib.DrawRectangleLines(x - 24, y - 24, 48, 46, accent);
            }
            else if (kv.Key == "Enchanting Table")
            {
                Raylib.DrawRectangle(x - 20, y - 6, 40, 10, new Color((byte)60,(byte)30,(byte)90,(byte)255));   // top
                Raylib.DrawRectangle(x - 12, y + 4, 24, 14, new Color((byte)40,(byte)20,(byte)60,(byte)255));   // base
                Raylib.DrawCircle(x, y - 12, 5, new Color((byte)190,(byte)120,(byte)255,(byte)255));            // floating orb
                Raylib.DrawRectangleLines(x - 20, y - 6, 40, 10, Color.Purple);
            }
            else   // Advanced Workstation
            {
                Raylib.DrawRectangle(x - 34, y - 12, 68, 22, new Color((byte)120,(byte)85,(byte)55,(byte)255));
                Raylib.DrawRectangle(x - 30, y + 10, 8, 16, new Color((byte)90,(byte)60,(byte)38,(byte)255));
                Raylib.DrawRectangle(x + 22, y + 10, 8, 16, new Color((byte)90,(byte)60,(byte)38,(byte)255));
                Raylib.DrawRectangle(x - 26, y - 26, 52, 12, new Color((byte)80,(byte)80,(byte)88,(byte)255)); // tool rack
                Raylib.DrawRectangleLines(x - 34, y - 12, 68, 22, accent);
            }
        }

    foreach (var w in placedWorkbenches)
    {
        int x = (int)w.X, y = (int)w.Y;
        Raylib.DrawRectangle(x - 30, y - 12, 60, 20, new Color((byte)140,(byte)100,(byte)60,(byte)255));
        Raylib.DrawRectangle(x - 26, y + 8,  8, 16, new Color((byte)100,(byte)70,(byte)40,(byte)255));
        Raylib.DrawRectangle(x + 18, y + 8,  8, 16, new Color((byte)100,(byte)70,(byte)40,(byte)255));
        Raylib.DrawRectangleLines(x - 30, y - 12, 60, 20, Color.Black);
    }

    foreach (var f in placedFurnaces)
    {
        int x = (int)f.X, y = (int)f.Y;
        Raylib.DrawRectangle(x - 22, y - 20, 44, 40, new Color((byte)90,(byte)90,(byte)95,(byte)255));   // stone body
        Raylib.DrawRectangle(x - 10, y - 2,  20, 16, new Color((byte)30,(byte)30,(byte)30,(byte)255));   // opening
        Raylib.DrawCircle(x, y + 8, 6, new Color((byte)255,(byte)140,(byte)0,(byte)255));                // fire
        Raylib.DrawRectangle(x - 6, y - 30, 12, 12, new Color((byte)70,(byte)70,(byte)75,(byte)255));    // chimney
        Raylib.DrawRectangleLines(x - 22, y - 20, 44, 40, Color.Black);
    }

    foreach (var f in placedFlags)
    {
        int x = (int)f.X, y = (int)f.Y;
        Raylib.DrawRectangle(x - 2, y - 34, 4, 40, new Color((byte)110,(byte)80,(byte)50,(byte)255)); // pole
        Raylib.DrawTriangle(new Vector2(x + 2, y - 34), new Vector2(x + 2, y - 18),
            new Vector2(x + 22, y - 26), Color.Red);                                                  // flag
    }

    foreach (var c in placedChests.Where(c => c.BuildingContext == ""))
    {
        int x = (int)c.Position.X, y = (int)c.Position.Y;
        int hw = c.Tier >= 1 ? 28 : 22;   // large chest is a bit wider
        Raylib.DrawRectangle(x - hw, y - 14, hw * 2, 28, new Color((byte)150,(byte)100,(byte)50,(byte)255));
        Raylib.DrawRectangle(x - hw, y - 14, hw * 2, 8,  new Color((byte)110,(byte)70,(byte)35,(byte)255));
        Raylib.DrawRectangle(x - 3,  y - 4,  6, 8, Color.Gold);
        Raylib.DrawRectangleLines(x - hw, y - 14, hw * 2, 28, Color.Black);
    }
}

static void DrawPlaceablePrompts()
{
    if (chestOpen || furnaceOpen) return; 
    bool nearChest = placedChests.Any(c => c.BuildingContext == "" && Vector2.Distance(player.Center, c.Position) < 80);
    bool nearBench = NearWorkbench();
    bool nearFlag = placedFlags.Any(f => Vector2.Distance(player.Center, f) < 80);
    bool nearFurnace = placedFurnaces.Any(f => Vector2.Distance(player.Center, f) < 80);
    if (!nearChest && !nearBench && !nearFurnace && !nearFlag) return;

    string txt = nearChest ? "Space = Open Chest  |  E = Pick up"
               : nearFurnace ? "Space = Smelt  |  E = Pick up"
               : nearFlag ? "E = Pick up Flag"                                            
               : "Space = Craft  |  E = Pick up";
    Raylib.DrawRectangle(0, ScreenHeight - 120, 420, 40, new Color((byte)0,(byte)0,(byte)0,(byte)160));
    Program.DrawTextUI(txt, 20, ScreenHeight - 110, 20, Color.LightGray);
}

        static string GetMonthString() => monthNames[currentMonth];

static void SpawnSplat(Vector2 center, Color baseColor, int count = 8)
{
    for (int i = 0; i < count; i++)
    {
        float ang = Raylib.GetRandomValue(0, 360) * (MathF.PI / 180f);
        float spd = Raylib.GetRandomValue(40, 160);
        splats.Add(new Splat {
            Position  = center,
            Velocity  = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd,
            Timer     = 0.45f,
            MaxTimer  = 0.45f,
            Radius    = Raylib.GetRandomValue(2, 5),
            SplatColor = baseColor
        });
    }
}

static void UpdateSplats(float dt)
{
    for (int i = splats.Count - 1; i >= 0; i--)
    {
        var s = splats[i];
        s.Timer -= dt;
        if (s.Timer <= 0f) { splats.RemoveAt(i); continue; }
        s.Position += s.Velocity * dt;
        s.Velocity *= 0.88f;            // drag, so they fan out then settle
        splats[i] = s;
    }
}

static void DrawSplats()
{
    foreach (var s in splats)
    {
        float a = s.Timer / s.MaxTimer;          // 1 → 0
        Raylib.DrawCircleV(s.Position, s.Radius * a,
            new Color(s.SplatColor.R, s.SplatColor.G, s.SplatColor.B, (byte)(220 * a)));
    }
}

                }

}
