using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void GenerateWorld()
        {
            var rng = new Random(12345);

// Forest top - Oak trees
for (int i = -30000; i < 30000; i += 200)
{
    Vector2 pos = new Vector2(i + rng.Next(-80, 80), -300 + rng.Next(-200, 0));
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) trees.Add(TreeObject.Oak(pos));
}

// Forest bottom - Oak trees
for (int i = -30000; i < 30000; i += 200)
{
    Vector2 pos = new Vector2(i + rng.Next(-80, 80), 1200 + rng.Next(0, 200));
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) trees.Add(TreeObject.Oak(pos));
}

// Safe zone - Normal trees
for (int i = -2800; i < 3800; i += 300)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(-1400, 2400));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(-1400, 2400));
    if (!IsNearRoad(pos1) && !IsNearBuilding(pos1)) trees.Add(TreeObject.Normal(pos1));
    if (!IsNearRoad(pos2) && !IsNearBuilding(pos2)) trees.Add(TreeObject.Normal(pos2));
}

// Grasslands - Birch trees
for (int i = 4200; i < 12000; i += 250)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(350, 750));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(-350, 150));
    Vector2 pos3 = new Vector2(i + rng.Next(-80, 80), rng.Next(800, 950));
    if (!IsNearRoad(pos1) && !IsNearBuilding(pos1)) trees.Add(TreeObject.Birch(pos1));
    if (!IsNearRoad(pos2) && !IsNearBuilding(pos2)) trees.Add(TreeObject.Birch(pos2));
    if (!IsNearRoad(pos3) && !IsNearBuilding(pos3)) trees.Add(TreeObject.Birch(pos3));
}

// Snow zone - Pine and Arctic trees
for (int i = -30000; i < -3100; i += 250)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-80, 80), rng.Next(400, 530));
    Vector2 pos2 = new Vector2(i + rng.Next(-80, 80), rng.Next(750, 950));
    Vector2 pos3 = new Vector2(i + rng.Next(-80, 80), rng.Next(-350, 150));
    if (!IsNearRoad(pos1) && !IsNearBuilding(pos1)) trees.Add(TreeObject.Pine(pos1));
    if (!IsNearRoad(pos2) && !IsNearBuilding(pos2)) trees.Add(TreeObject.Arctic(pos2));
    if (!IsNearRoad(pos3) && !IsNearBuilding(pos3)) trees.Add(TreeObject.Pine(pos3));
}

// Desert - Dead trees
for (int i = 4200; i < 30000; i += 400)
{
    Vector2 pos1 = new Vector2(i + rng.Next(-100, 100), rng.Next(400, 530));
    Vector2 pos2 = new Vector2(i + rng.Next(-100, 100), rng.Next(750, 950));
    if (!IsNearRoad(pos1) && !IsNearBuilding(pos1)) trees.Add(TreeObject.Dead(pos1));
    if (!IsNearRoad(pos2) && !IsNearBuilding(pos2)) trees.Add(TreeObject.Dead(pos2));
}

// Stone rocks - safe zone outskirts
for (int i = -2800; i < 3800; i += 400)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(-1400, 2400));
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) rocks.Add(RockObject.Stone(pos));
}

// Copper - grasslands
for (int i = 4200; i < 12000; i += 500)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(-300, 950));
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) rocks.Add(RockObject.Copper(pos));
}

// Iron - snow and desert
for (int i = -30000; i < -3100; i += 600)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), rng.Next(400, 900));
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) rocks.Add(RockObject.Iron(pos));
}

// Gold - deep forest
for (int i = -30000; i < 30000; i += 800)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), -14000 + rng.Next(-1500, 0));;
    if (!IsNearRoad(pos) && !IsNearBuilding(pos)) rocks.Add(RockObject.Gold(pos));
}

// Crystals - mountains
for (int i = -29000; i < -11000; i += 700)
{
    Vector2 pos = new Vector2(i + rng.Next(-100, 100), -5000 + rng.Next(-1000, 0));
    rocks.Add(RockObject.Crystal(pos));
}

lakes.Add(new Lake(new Vector2(700, 1200)));
lakes.Add(new Lake(new Vector2(-900, -600)));

rivers.Add(new River(new Vector2(-5000, 2000), 400));
rivers.Add(new River(new Vector2(-1950, -8221), 300, true));
rivers.Add(new River(new Vector2(-2250, -8221), 300));        

dungeonEntrances.Add((new Vector2(-2750, -900),  "Crypt",   "The Dark Crypt"));
dungeonEntrances.Add((new Vector2(-600,  -3200),  "Forest",  "Forest Tomb"));
dungeonEntrances.Add((new Vector2(-18000, 300),   "Snow",    "Ice Cavern"));
dungeonEntrances.Add((new Vector2(14000,  400),   "Desert",  "Desert Ruins"));
dungeonEntrances.Add((new Vector2(32000, -4000),  "Volcano", "Lava Cavern"));

// BUILDING INTERIOR AND NPCS

//-- Dbar --
// table 1 - left side of bar
dbarTableNPCs.Add(new NPC(new Vector2(120, 430), "Patron", "Cheers bro!"));
dbarTableNPCs.Add(new NPC(new Vector2(165, 470), "Patron", "Another round?"));

// table 2 - middle
dbarTableNPCs.Add(new NPC(new Vector2(340, 430), "Patron", "Good times."));
dbarTableNPCs.Add(new NPC(new Vector2(385, 470), "Patron", "Sweet as."));

// table 3 - right side before barrier
dbarTableNPCs.Add(new NPC(new Vector2(560, 430), "Patron", "Nah yeah nah."));
dbarTableNPCs.Add(new NPC(new Vector2(605, 470), "Patron", "Yeah nah yeah."));

dbarPokieNPC = new NPC(new Vector2(882, 340), "Bloke", "Oi back off, I'm on a winning streak bro!");

//-- Kiwi Cuts --
kiwiCutsWaitingNPCs.Add(new NPC(new Vector2(557, 215), "Patron", "Just a trim thanks."));
kiwiCutsWaitingNPCs.Add(new NPC(new Vector2(602, 215), "Patron", "Short back and sides bro."));

// DECORATIVE ASSETS
// park area
AddFountain(2280, -580);
AddBench(-2150, 220);
AddBench(-2000, 220);
AddPicnicTable(-1300, 820);
AddSwingSet(-460, 1130);
AddSwingSet(3035, -1170);
AddSandbox(-370, -50);
AddLamppost(990, -1100);
AddLamppost(350, -1100);
AddPostbox(-1100, 430);

// farm area
AddBarnHouse(-1075, -9700);
AddShed(-2800, -9250);
AddWindmill(-1520, -9200);
AddTractor(-550, -9800);
AddWaterTrough(2450, -9141);
AddHayBale(-2220, -9350);
AddHayBale(-1045, -9200);
AddHayBale(-850, -9200);
AddWheelbarrow(2180, 1020);


// residential street
AddCottagePeaked(-70, -100);
AddCottagePeaked(-70, 190);
AddTerracedHouse(1550, 800);
AddTerracedHouse(1100, 1100);

// DECORATIVE BUILDINGS
// Residential neighbourhood
AddCottage(1100, 1700);
AddCottage(900, 1700);
AddCottage(-800, 250);
AddCottage(-600, 250);
AddTownhouse(1530, 170);
AddTownhouse(1530, 1400);
AddApartmentBlock(740, 650);
AddApartmentBlock(960, 650);

// INTERACTIVE BUILDINGS

// Coastguards
coastguardStations.Add(new Vector2(27850, -12000));
coastguardStations.Add(new Vector2(27850,  -2000));
coastguardStations.Add(new Vector2(27850,   8000));

//Library
AddLibrary(-1310, -1150);

//Farming shop
AddFarmingShop(420, -11200);

//Dbars
AddDBar(1540, 650);
AddDBar(-4000, 410);

//AA buildings
AddAA(-1430, 100);       // safe zone
AddAA(14000, 5600);     // city of Hamiltron
AddAA(-15800, 4700);    // Rotoaira town

//Boatschool
AddBoatLicenceOffice(27700, 6000);

//Airports
AddAirport(5000, -500);

//Barn 
AddBarn(450, -10450);

//Zoo
AddZoo(-2420, -1270);

//Schools
AddSchool(2400, -1400);

// Pet adoption
AddFamilyHub(-2374, 1745); 

// Daycare
AddDaycare(3000, -300);
AddDaycareFamilies(new Vector2(3230, 40), new Vector2(2700, 400));
AddSchoolFamilies(new Vector2(2650, -950), new Vector2(2700, 700));

//Castle
AddCastle(3200, -1400);

//Mall
AddMall(4000, -900);

//Prison
AddPrison(6500, 2000);

//Casinos
AddCasino(2300, 1650);

//KiwiCuts
AddKiwiCuts(1910, 220);

//Hallensteins
AddHallensteins(2110, 220);

//DropZone
AddDropZone(-950, 2200);

//Mini golf
AddMiniGolf(420, 2200);

//Banks
AddBank(720, 185);
AddBank(-9850, -700); 

// Add dealers (exterior + interior)
AddDealerBuilding(340, 800, "BIKE DEALER", DealerType.Bike);
AddDealerBuilding(3200, 655, "CAR DEALER", DealerType.Car);
AddDealerBuilding(-1380, -445, "BARN DEALER", DealerType.Barn);

AddStore(-1000, 220);

AddHospital(340, -30);

AddMagicShop(-500, -200); 

AddRangingShop(1850, 650);

AddHobbiesStore(2050, 650);
InitAllCardSets();

AddWeapons(340, 220);

AddMyHouse(-400, 200);
AddMyHouse(-2090, -9855);
AddMailbox(-2050, -9705, -1);

AddGasStation(300, -420);
AddGasStation(7000, 540);
AddGasStation(-9000, 540);

AddMarae(-6900, -230);
AddMarae(-2050, -300);

AddGym(2340, 650);

AddSupermarket(3600, 200);

AddSwimmingComplex(1480, -1000);

AddTennisCourt(1975, 1588);

AddBasketballCourt(3680, -920);

AddMcDonalds(2310, 200);

AddKFC(1145, 200);
AddBurgerKing(-225, -1416);
AddDominos(-2572, 200);

gymCounterNPC = new NPC(new Vector2(780, 130), "Staff", "Grab a protein shake bro, $3 each.");

AddPoliceStation(3200, 200);

// ── CITY OF HAMILTRON ────────────────────────────────────────────────────────
// NW block
AddBank(12000, 4100);
AddHospital(12400, 4100);
AddPoliceStation(12000, 5800);
AddGym(12400, 5800);
AddSupermarket(13000, 4100);
AddKFC(13000, 5800);
AddMcDonalds(13600, 4100);
AddCasino(13600, 5800);

// Central (north avenue corridor)
AddAirport(15100, 3200);          // large footprint north of boulevard
AddGasStation(15100, 5800);
AddHallensteins(15500, 4100);
AddKiwiCuts(15900, 4100);
AddDropZone(16000, 5800);
AddBank(16700, 4100);

// East block
AddStore(17000, 4100);
AddDBar(17000, 5800);
AddWeapons(17400, 4100);
AddBurgerKing(17400, 5800);

// Extra houses — residential block south
for (int hx = 12100; hx < 17800; hx += 300)
    AddMyHouse(hx, 7400);

// ── COUNTRY TOWN OF ROTOAIRA ─────────────────────────────────────────────────
AddStore(-17600, 3400);
AddMyHouse(-17200, 3400);
AddMyHouse(-16800, 3400);
AddMyHouse(-16400, 3400);
AddMyHouse(-16000, 3400);
AddMyHouse(-15600, 3400);
AddGasStation(-17600, 4600);
AddBank(-17000, 4600);
AddHospital(-16400, 4600);
AddKiwiCuts(-15800, 4600);
AddPoliceStation(-15200, 4600);
AddMyHouse(-17600, 5500);
AddMyHouse(-17200, 5500);
AddMyHouse(-16800, 5500);
AddMyHouse(-16400, 5500);
AddMyHouse(-16000, 5500);
AddDBar(-15400, 5500);
AddMarae(-14600, 4400);

RemoveTreesRocksNearBuildings();

// examples — tune positions to match your actual building steps/hills
        elevationZones.Add(new ElevationZone(new Rectangle(400, -200, 80, 40),  ElevationType.StairsUp,   16));
        elevationZones.Add(new ElevationZone(new Rectangle(400, -160, 80, 40),  ElevationType.StairsDown, 16));
        elevationZones.Add(new ElevationZone(new Rectangle(-500, 300, 300, 200), ElevationType.Hill,       10));

// Town NPCs
npcs.Add(new NPC(new Vector2(-3000, 1500), "Mara", "Mind the wolves.") { SpriteKey = "elder" });
npcs.Add(new NPC(new Vector2(-2800, 1500), "Doric", "Steel's ready.")  { SpriteKey = "blacksmith" });

            npcs.Add(new NPC(
                new Vector2(500,500),
                    "Local Resident",
                    "Nice weather today."
            ));

            npcs.Add(new NPC(
                new Vector2(1400,900),
                    "Fisherman",
                    "The lakes nearby have good fishing."
            ));

            //Fence safezone
            fenceManager.SpawnAt(new Vector2(-3000, -1500), 25, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(345, -1500), 30, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(3980, -1500), 14, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(3980, 630), 14, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 104);
            fenceManager.SpawnAt(new Vector2(-3000, -1500), 14, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(-3000, 630), 14, horizontal: false, segmentWidth: 12, segmentHeight: 30, spacing: 104);
            fenceManager.SpawnAt(new Vector2(-3000, 2500), 25, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);
            fenceManager.SpawnAt(new Vector2(345, 2500), 30, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 110);

            //Fence farmzone
            //Boundary
            fenceManager.SpawnAt(new Vector2(-3000, -10000), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(-20, -10000), 31, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 110);
            fenceManager.SpawnAt(new Vector2(-3000, -10000), 2, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 108);
            fenceManager.SpawnAt(new Vector2(-3000, -9610), 28, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 110);
            fenceManager.SpawnAt(new Vector2(-3000, -6000), 26, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);

            //Left fences
            fenceManager.SpawnAt(new Vector2(-3000, -9000), 10, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 103);
            fenceManager.SpawnAt(new Vector2(-2000, -8883), 2, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-3000, -7500), 5, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-3000, -7320), 9, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-3000, -6800), 9, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2850, -8300), 9, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2325, -7500), 5, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 101);
            fenceManager.SpawnAt(new Vector2(-2865, -6209), 8, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 101);
            fenceManager.SpawnAt(new Vector2(-2865, -6640), 7, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 101);

            //Right fences
            fenceManager.SpawnAt(new Vector2(-1780, -9000), 12, horizontal: true, segmentWidth: 12, segmentHeight: 30, spacing: 105);

            //Middle fences
            fenceManager.SpawnAt(new Vector2(-1780, -10000), 4, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-1780, -9400), 25, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-1780, -6112), 1, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-1957, -7320), 3, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-1957, -6820), 5, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2865, -6640), 4, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2607, -8300), 5, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);
            fenceManager.SpawnAt(new Vector2(-2441, -8130), 5, horizontal: false, segmentWidth: 12, segmentHeight: 20, spacing: 105);

            // Farm plots           
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2200, -9900) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2260, -9900) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2320, -9900) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2380, -9900) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2440, -9900) });

            farmPlots.Add(new FarmPlot { Position = new Vector2(-2380, -9850) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2320, -9850) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2260, -9850) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2200, -9850) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2440, -9850) });

            farmPlots.Add(new FarmPlot { Position = new Vector2(-2380, -9800) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2320, -9800) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2260, -9800) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2200, -9800) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2440, -9800) });

            farmPlots.Add(new FarmPlot { Position = new Vector2(-2380, -9750) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2320, -9750) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2260, -9750) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2200, -9750) });
            farmPlots.Add(new FarmPlot { Position = new Vector2(-2440, -9750) });

            // Fruit trees
            foreach (var pos in treePlotPositions)
                fruitTrees.Add(new FruitTree { Position = pos });

            // Trees
            trees.Add(TreeObject.Normal(new Vector2(-2370, -9176)));
            trees.Add(TreeObject.Normal(new Vector2(-2370, -9347)));
            trees.Add(TreeObject.Normal(new Vector2(-2891, -9408)));
            trees.Add(TreeObject.Normal(new Vector2(-2700, -9400)));
            trees.Add(TreeObject.Oak(new Vector2(-1267, -9183)));
            trees.Add(TreeObject.Normal(new Vector2(-2040, -8980)));
            trees.Add(TreeObject.Birch(new Vector2(-2700, -7000)));
            trees.Add(TreeObject.Normal(new Vector2(-2296, -8111)));
            trees.Add(TreeObject.Normal(new Vector2(-2202, -7676)));
            trees.Add(TreeObject.Normal(new Vector2(-2392, -7501)));
            trees.Add(TreeObject.Birch(new Vector2(-1996, -7881)));

            //Lakes
            lakes.Add(new Lake(new Vector2(-2730, -7844)));

            //Rocks
            rocks.Add(RockObject.Stone(new Vector2(-1797, -6221)));
            rocks.Add(RockObject.Stone(new Vector2(-1761, -6164)));
            rocks.Add(RockObject.Stone(new Vector2(-1761, -6212)));
            rocks.Add(RockObject.Stone(new Vector2(-2950, -8290)));
            rocks.Add(RockObject.Stone(new Vector2(-2900, -8290)));
            rocks.Add(RockObject.Stone(new Vector2(-2670, -8580)));
            rocks.Add(RockObject.Stone(new Vector2(-2805, -8790)));

            // Campfires
            campfirePositions.Add(new Vector2(-2151,  -7873)); // Tutorial
            campfirePositions.Add(new Vector2(-800,  800));
            campfirePositions.Add(new Vector2(2000, 1800));
            campfirePositions.Add(new Vector2(-5000, 600));
            campfirePositions.Add(new Vector2(6000,  800));
            campfirePositions.Add(new Vector2(-14000,5000));  // near country town
            campfirePositions.Add(new Vector2(15000, 6000));
            builtinCampfireCount = campfirePositions.Count;  // near city

            // Incubators
            incubatorPositions.Add(new Vector2(-1850, -9800));   // near spawn

            //Boats
            float coastX = oceanBounds.X + 140;

            GenerateBoatSpawns();
            foreach (var pos in boatSpawnPoints)
                vehicles.Add(new Vehicle(pos, new Color((byte)60,(byte)140,(byte)200,(byte)255), 420f, Vehicle.VehicleType.Boat));
            
            vehicles.Add(new Vehicle(new Vector2(coastX, -6000), new Color((byte)240,(byte)200,(byte)40,(byte)255),  750f, Vehicle.VehicleType.Jetski));
            vehicles.Add(new Vehicle(new Vector2(coastX, -3000), new Color((byte)120,(byte)120,(byte)130,(byte)255), 300f, Vehicle.VehicleType.Dinghy));
            vehicles.Add(new Vehicle(new Vector2(coastX,     0), new Color((byte)210,(byte)30,(byte)30,(byte)255),   850f, Vehicle.VehicleType.SpeedBoat));
            vehicles.Add(new Vehicle(new Vector2(coastX,  3000), new Color((byte)140,(byte)90,(byte)45,(byte)255),   250f, Vehicle.VehicleType.Canoe));
            vehicles.Add(new Vehicle(new Vector2(coastX + 120,  6000), new Color((byte)250,(byte)250,(byte)255,(byte)255), 500f, Vehicle.VehicleType.Yacht));
            vehicles.Add(new Vehicle(new Vector2(coastX + 260, 10000), new Color((byte)200,(byte)160,(byte)60,(byte)255),  320f, Vehicle.VehicleType.CruiseShip));
            
            //vehicles
            vehicles.Add(new Vehicle(new Vector2(50,  800), Color.Red,      650, Vehicle.VehicleType.Sedan));
            vehicles.Add(new Vehicle(new Vector2(-670, 650), Color.Yellow,   900, Vehicle.VehicleType.Truck));
            vehicles.Add(new Vehicle(new Vector2(-400, 650), Color.DarkBlue, 500, Vehicle.VehicleType.SUV));
            vehicles.Add(new Vehicle(new Vector2(3082, 248), Color.Black, 500, Vehicle.VehicleType.PoliceCar));
            vehicles.Add(new Vehicle(new Vector2(3082, 758), Color.Red, 500, Vehicle.VehicleType.FireTruck));
            vehicles.Add(new Vehicle(new Vector2(550, -30), Color.White, 500, Vehicle.VehicleType.Ambulance));

            //Updated vehciles
            vehicles.Add(new Vehicle(new Vector2(-100, 800),  new Color((byte)180,(byte)140,(byte)60,(byte)255),  450f, Vehicle.VehicleType.Ute));
            vehicles.Add(new Vehicle(new Vector2(-250, 800), new Color((byte)60,(byte)120,(byte)60,(byte)255),   380f, Vehicle.VehicleType.MonsterTruck));
            vehicles.Add(new Vehicle(new Vector2(-400, 800), new Color((byte)200,(byte)40,(byte)40,(byte)255),   650f, Vehicle.VehicleType.Convertible));
            vehicles.Add(new Vehicle(new Vector2(-550, 800), new Color((byte)30,(byte)30,(byte)40,(byte)255),    700f, Vehicle.VehicleType.MuscleCar));
            vehicles.Add(new Vehicle(new Vector2(-700, 800), new Color((byte)220,(byte)20,(byte)20,(byte)255),   900f, Vehicle.VehicleType.RacingCar));

            // City traffic
            vehicles.Add(new Vehicle(new Vector2(12400, 5560), new Color((byte)40,(byte)80,(byte)160,(byte)255), 280f, Vehicle.VehicleType.Sedan));
            vehicles.Add(new Vehicle(new Vector2(15200, 3960), new Color((byte)160,(byte)40,(byte)40,(byte)255), 260f, Vehicle.VehicleType.Sedan));
            vehicles.Add(new Vehicle(new Vector2(16800, 5560), new Color((byte)40,(byte)140,(byte)40,(byte)255), 300f, Vehicle.VehicleType.SUV));
            vehicles.Add(new Vehicle(new Vector2(14900, 7260), new Color((byte)120,(byte)120,(byte)120,(byte)255), 240f, Vehicle.VehicleType.Sedan));
            // Police cruiser patrolling city
            vehicles.Add(new Vehicle(new Vector2(13200, 5560), new Color((byte)20,(byte)20,(byte)180,(byte)255), 320f, Vehicle.VehicleType.PoliceCar));
            // Town traffic
            vehicles.Add(new Vehicle(new Vector2(-17000, 4460), new Color((byte)140,(byte)100,(byte)40,(byte)255), 200f, Vehicle.VehicleType.Sedan));
            vehicles.Add(new Vehicle(new Vector2(-15800, 4460), new Color((byte)80,(byte)60,(byte)30,(byte)255),  180f, Vehicle.VehicleType.Truck));


            // Mountain bikes - safe zone and grasslands
            rideables.Add(new Rideable(new Vector2(-210, 650),  Rideable.RideableType.MountainBike, new Color((byte)180,(byte)80,(byte)20,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-500, 700), Rideable.RideableType.MountainBike, new Color((byte)20,(byte)100,(byte)180,(byte)255)));
            rideables.Add(new Rideable(new Vector2(5000, 600), Rideable.RideableType.MountainBike, new Color((byte)20,(byte)150,(byte)50,(byte)255)));

            // BMX bikes - safe zone
            rideables.Add(new Rideable(new Vector2(-80, 650),  Rideable.RideableType.BMX, new Color((byte)220,(byte)50,(byte)50,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-300, 700), Rideable.RideableType.BMX, new Color((byte)200,(byte)150,(byte)0,(byte)255)));
            rideables.Add(new Rideable(new Vector2(900, 1000), Rideable.RideableType.BMX, new Color((byte)150,(byte)0,(byte)200,(byte)255)));

            // Donkeys
            rideables.Add(new Rideable(new Vector2(-2807, -10100),  Rideable.RideableType.Donkey, new Color((byte)130,(byte)130,(byte)140,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-4000, -8500),  Rideable.RideableType.Donkey, new Color((byte)130,(byte)130,(byte)140,(byte)255)));

            // Horses - grasslands and safe zone outskirts
           // rideables.Add(new Rideable(new Vector2(4500, 400),  Rideable.RideableType.Horse, new Color((byte)139,(byte)90,(byte)43,(byte)255)));
           // rideables.Add(new Rideable(new Vector2(7000, 700),  Rideable.RideableType.Horse, new Color((byte)80,(byte)50,(byte)20,(byte)255)));
           // rideables.Add(new Rideable(new Vector2(-2000, 800), Rideable.RideableType.Horse, new Color((byte)200,(byte)180,(byte)160,(byte)255)));
           // rideables.Add(new Rideable(new Vector2(3000, 500),  Rideable.RideableType.Horse, Color.White));
            rideables.Add(new Rideable(new Vector2(-450, -6400),  Rideable.RideableType.Horse, Color.White));
            rideables.Add(new Rideable(new Vector2(-2807, -9564),  Rideable.RideableType.Horse, Color.White));

            // Camels - desert/sandy areas
            rideables.Add(new Rideable(new Vector2(9000, 1200), Rideable.RideableType.Camel, new Color((byte)210,(byte)140,(byte)50,(byte)255)));
            rideables.Add(new Rideable(new Vector2(10750, 400), Rideable.RideableType.Camel, new Color((byte)225,(byte)150,(byte)55,(byte)255)));

            // Elephants - savanna/grassland
            rideables.Add(new Rideable(new Vector2(6000, 1500), Rideable.RideableType.Elephant, new Color((byte)140,(byte)140,(byte)150,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-3200, 1000), Rideable.RideableType.Elephant, new Color((byte)130,(byte)130,(byte)140,(byte)255)));

            // Dolphins - near the ocean/beach
            rideables.Add(new Rideable(new Vector2(28000, 300), Rideable.RideableType.Dolphin, new Color((byte)90,(byte)140,(byte)200,(byte)255)));
            rideables.Add(new Rideable(new Vector2(30000, -200), Rideable.RideableType.Dolphin, new Color((byte)80,(byte)130,(byte)190,(byte)255)));

            // Reindeer - arctic/snow areas
            rideables.Add(new Rideable(new Vector2(-25000, 200), Rideable.RideableType.Reindeer, new Color((byte)140,(byte)100,(byte)70,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-23000, -400), Rideable.RideableType.Reindeer, new Color((byte)120,(byte)85,(byte)55,(byte)255)));

            // Tigers - jungle/forest
            rideables.Add(new Rideable(new Vector2(5500, -2000), Rideable.RideableType.Tiger, new Color((byte)230,(byte)140,(byte)40,(byte)255)));
            rideables.Add(new Rideable(new Vector2(-4000, -1500), Rideable.RideableType.Tiger, new Color((byte)235,(byte)145,(byte)45,(byte)255)));

            quests.Add(new Quest("Lumberjack", "Chop 10 trees", 10, 50));
            quests.Add(new Quest("Fisher", "Catch 10 fish", 10, 75));
            quests.Add(new Quest("Big Money", "Earn $100", 100, 200));
            quests.Add(new Quest("Treasure Hunter", "Find 5 hidden collectables", 5, 300));
            quests.Add(new Quest("Wolf Culler",     "Slay 8 wolves",              8, 250));
            quests.Add(new Quest("Green Thumb",     "Harvest 10 crops",          10, 200));
            quests.Add(new Quest("Master Chef",     "Cook 15 meals",             15, 220));
            quests.Add(new Quest("Deep Pockets",    "Bank $1000",              1000, 400));

            // Grasslands - Wild Dogs
            enemies.Add(new Enemy(new Vector2(-500, 3000), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-450, 3600), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-360, 4000), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(1100, 4050), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(1000, 4400), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(670, 4150), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-2546, -7100), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-2854, -7000), "Wild Dog", 3, Color.Brown));
            enemies.Add(new Enemy(new Vector2(-2450, -6935), "Wild Dog", 3, Color.Brown));

            // Forest - Wolves
            enemies.Add(new Enemy(new Vector2(-300, -2500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(400, -3000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-200, 4000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(500, 3000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(5000, -2200), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-5000, 3500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(843, 5000), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(10000, -500), "Wolf", 5, Color.DarkGray));
            enemies.Add(new Enemy(new Vector2(-10000, 1200), "Wolf", 5, Color.DarkGray));

            // Desert - Scorpions
            enemies.Add(new Enemy(new Vector2(6000, 300), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(10000, 700), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(15000, 200), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(20000, 600), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));
            enemies.Add(new Enemy(new Vector2(25000, 400), "Scorpion", 4, new Color((byte)180, (byte)120, (byte)0, (byte)255)));

            // Snow - Bears
            enemies.Add(new Enemy(new Vector2(-6000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-10000, 600), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-15000, 400), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-20000, 700), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-25000, 300), "Bear", 8, new Color((byte)100, (byte)100, (byte)120, (byte)255)));

            // Ocean/Beach (X 26000–45000)
            enemies.Add(new Enemy(new Vector2(27000,  400), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(28500,  200), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(29500, -300), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(27800,  600), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(29000, -100), "Crab", 10, new Color((byte)210, (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(32000,  300), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));
            enemies.Add(new Enemy(new Vector2(36000, -200), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));
            enemies.Add(new Enemy(new Vector2(40000,  500), "Shark", 13, new Color((byte)70,  (byte)100, (byte)140, (byte)255)));

            // Swamp (X -30000 to -55000)
            enemies.Add(new Enemy(new Vector2(-31000,  300), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-34000, -200), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-37000,  500), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-40000,  100), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-43000, -400), "Snake", 10, new Color((byte)40,  (byte)100, (byte)40,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-33000,  600), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-39000, -300), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-46000,  400), "Crocodile", 14, new Color((byte)50,  (byte)80,  (byte)30,  (byte)255)));

            // Volcano (X 26000–55000, Y -2000 to -20000)
            enemies.Add(new Enemy(new Vector2(28000, -3000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(32000, -5000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(37000, -4000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(42000, -6000), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(48000, -3500), "Fire Lizard", 12, new Color((byte)180, (byte)60,  (byte)10,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(30000, -7000), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));
            enemies.Add(new Enemy(new Vector2(38000, -5500), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));
            enemies.Add(new Enemy(new Vector2(45000, -8000), "Magma Beetle", 15, new Color((byte)120, (byte)30,  (byte)0,   (byte)255)));

            // Mountains (X -26000 to -55000, Y -2000 to -20000)
            enemies.Add(new Enemy(new Vector2(-28000, -3000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-32000, -5000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-37000, -4000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-43000, -6000), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-49000, -3500), "Eagle", 11, new Color((byte)100, (byte)70,  (byte)20,  (byte)255)));
            enemies.Add(new Enemy(new Vector2(-30000, -7000), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-38000, -5500), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));
            enemies.Add(new Enemy(new Vector2(-46000, -8000), "Mountain Goat", 13, new Color((byte)200, (byte)195, (byte)185, (byte)255)));

            // Humanoids
            enemies.Add(new Enemy(new Vector2(2200, 4200),  "Warrior",   6, new Color((byte)150,(byte)150,(byte)165,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2320, 4200),  "Wizard",    5, new Color((byte)90,(byte)60,(byte)150,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2440, 4200),  "Archer",    5, new Color((byte)120,(byte)85,(byte)45,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2200, 4340),  "Goblin",    4, new Color((byte)90,(byte)150,(byte)70,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2320, 4340),  "Thug",      7, new Color((byte)70,(byte)70,(byte)80,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2440, 4340),  "Robber",    6, new Color((byte)45,(byte)45,(byte)55,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2560, 4340),  "Gangster",  8, new Color((byte)30,(byte)30,(byte)35,(byte)255)));
            enemies.Add(new Enemy(new Vector2(2680, 4340),  "Giant Bug", 9, new Color((byte)60,(byte)90,(byte)45,(byte)255)));

            // WORLD BOSS
            worldBoss = new WorldBoss(
            new Vector2(5000, -2000),   // spawn location — pick an open area
            "Colossus",
            2000f,                       // health
            1500f,                       // roam radius
            new Color((byte)90,(byte)40,(byte)120,(byte)255));

            superBoss = new WorldBoss(
            new Vector2(-8000, 4000),    // different spawn area from Colossus
            "Titan",                      // name
            6000f,                        // triple Colossus's health
            2500f,                        // bigger roam radius
            new Color((byte)140,(byte)20,(byte)20,(byte)255));   // dark red
            superBoss.Size = 480;             // over double Colossus's 220
            superBoss.ContactDamage = 60;     // hits harder
            superBoss.ChaseSpeed = 120f;      // slightly slower (it's massive)
            superBoss.ShakesWhenNear = true;  // proximity rumble

// ── MULTIPLAYER: wire up host-authoritative boss HP sync ──────────────
            multiplayer.BossHitReceived = null;   // clear any previous subscription first
            multiplayer.BossStateReceived = null;

            multiplayer.BossHitReceived += (isSuper, dmg, killerId) =>
            {
                var boss = isSuper ? superBoss : worldBoss;
                if (boss == null || boss.Dead) return;

                boss.LastDamagerId = killerId;
                boss.Health -= dmg;
                if (boss.Health <= 0)
                {
                    boss.Health = 0;
                    boss.Dead = true;
                    AwardBossKill(boss, isSuper);
                    DropSpiritEssence(worldBoss.Position, 20);
                }

                multiplayer.BroadcastBossState(isSuper, boss.Health, boss.MaxHealth, boss.Dead,
                    boss.Position.X, boss.Position.Y);
            };

            multiplayer.BossStateReceived += (isSuper, health, maxHealth, dead, posX, posY) =>
            {
                var boss = isSuper ? superBoss : worldBoss;
                if (boss == null) return;
                boss.Health = health;
                boss.MaxHealth = maxHealth;
                boss.Dead = dead;
                boss.Position = new Vector2(posX, posY); 
            };

            // ── MULTIPLAYER: host-authoritative enemy sync ──
            multiplayer.EnemyStateReceived = null;
            multiplayer.EnemyHitReceived   = null;

            // CLIENT: overwrite local enemy with host truth
            multiplayer.EnemyStateReceived += (id, posX, posY, health, dead, aggro) =>
            {
                if (id < 0 || id >= enemies.Count) return;
                var e = enemies[id];
                bool wasAlive = !e.Dead;
                e.Position = new Vector2(posX, posY);
                e.Health   = health;
                e.Aggro    = aggro;
                e.Dead     = dead;
                if (wasAlive && dead)                       // host says it just died → local death FX only
                    SpawnDeathFx(e.Center, e.EnemyColor, e.Type);
            };

            // HOST: a client reported a hit → apply damage + resolve kill authoritatively
            multiplayer.EnemyHitReceived += (id, dmg, killerId) =>
            {
                if (id < 0 || id >= enemies.Count) return;
                var e = enemies[id];
                if (e.Dead) return;
                e.LastDamagerId = killerId;
                e.Health -= dmg;
                if (e.Health <= 0)
                    HandleEnemyDeath(e);                    // drops loot into shared world (Option A)
            };

multiplayer.EnemyKillReceived = null;
            multiplayer.EnemyKillReceived += (enemyType) =>
            {
                player.AddCombatXP(CombatXpFor(enemyType));
            };

multiplayer.BossKillRewardReceived = null;
            multiplayer.BossKillRewardReceived += (isSuper) =>
            {
                player.AddCombatXP(isSuper ? 1500 : 500);
                player.Money += isSuper ? 3000 : 1000;
                ShowLevelUp("Boss defeated!", 0);
            };

multiplayer.LootDropReceived   = null;
            multiplayer.LootPickupReceived = null;

            multiplayer.LootDropReceived += (x, y, itemType, ownerId) =>
            {
                if (ownerId == -2 && itemType.StartsWith("__REMOVE__"))   // NEW: host despawn signal
                {
                    string real = itemType.Substring("__REMOVE__".Length);
                    var pos = new Vector2(x, y);
                    lootDrops.RemoveAll(d => d.ItemType == real
                                           && Vector2.DistanceSquared(d.Position, pos) < 4f);
                    return;
                }
                lootDrops.Add(new LootDrop(new Vector2(x, y), itemType, ownerId));
            };

            // HOST: a client collected a drop → remove the matching one for everyone
            multiplayer.LootPickupReceived += (x, y, itemType) =>
            {
                var pos = new Vector2(x, y);
                var match = lootDrops.Find(d => !d.Collected && d.ItemType == itemType
                                              && Vector2.DistanceSquared(d.Position, pos) < 4f);
                if (match != null)
                {
                    match.Collected = true;
                    lootDrops.Remove(match);
                    multiplayer.SendLootDrop(x, y, "__REMOVE__" + itemType, -2);   // tell other clients to despawn
                }
            };

multiplayer.WorldClockReceived = null;
            multiplayer.WorldClockReceived += (tod, dow, dom, mon, raining) =>
            {
                timeOfDay = tod; dayOfWeek = dow; dayOfMonth = dom; currentMonth = mon;
                if (raining != isRaining)   // flip weather to match host; let UpdateWeather handle particles
                {
                    isRaining = raining;
                    rainTimer = 0f;
                    if (isRaining) { raindrops.Clear(); musicBeforeRain = currentMusic; SwitchMusic(musicRain); }
                    else { lastZoneMusic = default; CheckZoneMusic(); }
                }
            };

multiplayer.CardStateReceived = null;
multiplayer.CardActionReceived = null;
multiplayer.CardAwardReceived = null;

multiplayer.CardAwardReceived = (xp, won, gameType) =>
{
    AddPlayingCardsXP(xp);
    RecordGameResult((CardGameType)gameType, won);
};

multiplayer.CardStateReceived += (state) =>
{
    if (state.StartsWith("HUBSTART|"))
    {
        string[] hs = state.Split('|');
        hubGame = (CardGameType)int.Parse(hs[1]);
        cardSeatOwner[0] = int.Parse(hs[2]);
        cardSeatOwner[1] = int.Parse(hs[3]);
        cardSeatOwner[2] = int.Parse(hs[4]);
        cardSeatOwner[3] = int.Parse(hs[5]);
        hubActive = true;
        hubScreen = HubScreen.Playing;
        LaunchGame(hubGame);      // client launches the SAME game the host chose
        return;
    }
    if (state.StartsWith("SEQ|")) { ApplySeqState(state); return; }

    string[] parts = state.Split('|');
    if (parts.Length < 27) return;

    cardGameType   = (CardGameType)int.Parse(parts[0]);
    cardPhase      = (CardPhase)int.Parse(parts[1]);
    currentPlayer  = int.Parse(parts[2]);
    dealer         = int.Parse(parts[3]);
    teamScore[0]   = int.Parse(parts[4]);
    teamScore[1]   = int.Parse(parts[5]);
    tricksWon[0]   = int.Parse(parts[6]);
    tricksWon[1]   = int.Parse(parts[7]);
    trumpSuit      = int.Parse(parts[8]);
    cardSeatOwner[0] = int.Parse(parts[9]);
    cardSeatOwner[1] = int.Parse(parts[10]);
    cardSeatOwner[2] = int.Parse(parts[11]);
    cardSeatOwner[3] = int.Parse(parts[12]);

    string[] counts = parts[13].Split(',');
    for (int seat = 0; seat < 4; seat++)
    {
        int count = int.Parse(counts[seat]);
        if (!IsMyCardSeat(seat))
        {
            hands[seat] = new List<Card>();
            for (int i = 0; i < count; i++) hands[seat].Add(new Card(-2, -2));
        }
    }

    currentTrick.Clear();
    if (!string.IsNullOrEmpty(parts[14]))
    {
        foreach (var entry in parts[14].Split(';'))
        {
            var bits = entry.Split(':');
            Card c = new Card(int.Parse(bits[0]), int.Parse(bits[1]));
            int who = int.Parse(bits[2]);
            currentTrick.Add((c, who));
        }
    }

    maker       = int.Parse(parts[15]);
    makerTeam   = int.Parse(parts[16]);
    goingAlone  = parts[17] == "1";
    upCard      = DeserializeCard(parts[18]);
    euchreBidRound = int.Parse(parts[19]);
    fiveHundredBid = int.Parse(parts[20]);
    fiveHundredBidSuit = int.Parse(parts[21]);
    fiveHundredHighBidder = int.Parse(parts[22]);
    fiveHundredBidValue = int.Parse(parts[23]);
    if (parts.Length > 24) cardMessage = parts[24].Replace('/', '|');
    if (parts.Length > 25) float.TryParse(parts[25], System.Globalization.CultureInfo.InvariantCulture, out cardMessageTimer);

    bool gameStarted = parts.Length > 26 && parts[26] == "1";
    cardGameStarted = gameStarted;

    if (hubActive && hubScreen == HubScreen.SeatSelect && gameStarted)
    {
        hubScreen = HubScreen.Playing;
    }
};

multiplayer.OwnHandReceived += (seat, serializedHand) =>
{
    if (seqActive) seqHands[seat] = DeserializeCardList(serializedHand);
    else           hands[seat]    = DeserializeCardList(serializedHand);
};

multiplayer.CardActionReceived += (fromId, action) =>
{
    string[] parts = action.Split('|');
    if (parts.Length < 1) return;

    if (parts[0] == "SEAT" && parts.Length >= 2 && int.TryParse(parts[1], out int seat) && seat >= 0 && seat < 4)
    {
        if (cardSeatOwner[seat] <= 0)
        {
            for (int s = 0; s < 4; s++)
                if (cardSeatOwner[s] == fromId) cardSeatOwner[s] = -1;

            cardSeatOwner[seat] = fromId;
            BroadcastCardTableState();
        }
    }
    else if (parts[0] == "PLAY" && parts.Length >= 2)
    {
        int playerSeat = -1;
        for (int s = 0; s < 4; s++)
            if (cardSeatOwner[s] == fromId) { playerSeat = s; break; }

        if (playerSeat >= 0 && currentPlayer == playerSeat && cardPhase == CardPhase.Playing)
        {
            Card requestedCard = DeserializeCard(parts[1]);
            var legal = LegalMoves(playerSeat, trumpSuit);
            bool isLegal = legal.Any(c => c.Suit == requestedCard.Suit && c.Rank == requestedCard.Rank && c.IsJoker == requestedCard.IsJoker);

            if (isLegal)
            {
                PlayCard(playerSeat, requestedCard);
                BroadcastCardTableState();
            }
        }
    }
    else if (parts[0] == "ORDERUP" && parts.Length >= 3)
    {
        int s = SeatOfSender(fromId);
        if (s == currentPlayer && cardPhase == CardPhase.Bidding && euchreBidRound == 1)
        {
            EuchreOrderUp(s, parts[2] == "1");
            BroadcastCardTableState();
        }
    }
    else if (parts[0] == "NAMESUIT" && parts.Length >= 4)
    {
        int s = SeatOfSender(fromId);
        if (s == currentPlayer && cardPhase == CardPhase.Bidding && euchreBidRound == 2)
        {
            EuchreNameSuit(s, int.Parse(parts[2]), parts[3] == "1");
            BroadcastCardTableState();
        }
    }
    else if (parts[0] == "PASS" && parts.Length >= 1)
    {
        int s = SeatOfSender(fromId);
        if (s == currentPlayer && cardPhase == CardPhase.Bidding)
        {
            if (cardGameType == CardGameType.Euchre) EuchrePass(s);
            else FiveHundredPass(s);
            BroadcastCardTableState();
        }
    }
    else if (parts[0] == "BID500" && parts.Length >= 4)
    {
        int s = SeatOfSender(fromId);
        if (s == currentPlayer && cardPhase == CardPhase.Bidding)
        {
            FiveHundredMakeBid(s, int.Parse(parts[2]), int.Parse(parts[3]));
            BroadcastCardTableState();
        }
    }
    else if (parts[0] == "SEQPLAY" && parts.Length >= 4)
    {
        int cardIdx = int.Parse(parts[1]);
        int row = int.Parse(parts[2]);
        int col = int.Parse(parts[3]);

        int playerSeat = -1;
        for (int s = 0; s < 4; s++)
            if (cardSeatOwner[s] == fromId) { playerSeat = s; break; }

        if (playerSeat >= 0 && seqCurrentPlayer == playerSeat && !seqGameOver
            && cardIdx >= 0 && cardIdx < seqHands[playerSeat].Count)
        {
            SeqApplyMove(playerSeat, cardIdx, row, col);
            SeqBroadcastState();
        }
    }
    else if (parts[0] == "SEQDISCARD" && parts.Length >= 2)
    {
        int cardIdx = int.Parse(parts[1]);
        int playerSeat = -1;
        for (int s = 0; s < 4; s++)
            if (cardSeatOwner[s] == fromId) { playerSeat = s; break; }

        if (playerSeat >= 0 && seqCurrentPlayer == playerSeat && !seqGameOver
            && cardIdx >= 0 && cardIdx < seqHands[playerSeat].Count)
        {
            seqHands[playerSeat].RemoveAt(cardIdx);
            if (seqDeckIdx < seqDeck.Count) seqHands[playerSeat].Add(SeqDrawCard());
            seqCurrentPlayer = (playerSeat + 1) % 4;
            seqAiTimer = 0.9f;
            SeqBroadcastState();
        }
    }
};

// Dealer options: specific mapping (horses -> barn, BMX/Mountain -> bike, Sedan/Truck/SUV -> car)
dealerBikeOptions.Clear();
dealerBarnOptions.Clear();
dealerVehicleOptions.Clear();


// Bikes: take rideables that are BMX or MountainBike
var bikeProtos = rideables
    .Where(r => r.Type == Rideable.RideableType.MountainBike || r.Type == Rideable.RideableType.BMX)
    .Take(8)
    .ToList();
foreach (var r in bikeProtos)
    dealerBikeOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));

// Barn: only horses
var horseProtos = rideables
    .Where(r => r.Type == Rideable.RideableType.Horse)
    .Take(8)
    .ToList();
foreach (var h in horseProtos)
    dealerBarnOptions.Add(new Rideable(h.SpawnPosition, h.Type, h.RideableColor));

// Cars: only Sedan, Truck, SUV
var carProtos = vehicles
    .Where(v => v.Type == Vehicle.VehicleType.Sedan
             || v.Type == Vehicle.VehicleType.Truck
             || v.Type == Vehicle.VehicleType.SUV)
    .Take(8)
    .ToList();
foreach (var v in carProtos)
    dealerVehicleOptions.Add(new Vehicle(v.Position, v.VehicleColor, v.TopSpeed, v.Type));


// bikes (mountain + bmx from rideables or create sample rideables)
foreach (var r in rideables.Take(4))
    dealerBikeOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));

// barns (horses)
foreach (var r in rideables.Where(r => r.Type == Rideable.RideableType.Horse).Take(4))
    dealerBarnOptions.Add(new Rideable(r.SpawnPosition, r.Type, r.RideableColor));
    dealerBarnOptions.Add(new Rideable(Vector2.Zero, Rideable.RideableType.Donkey, new Color((byte)150,(byte)150,(byte)160,(byte)255)));

// cars (vehicles list)
foreach (var v in vehicles.Take(4))
    dealerVehicleOptions.Add(new Vehicle(v.Position, v.VehicleColor, v.TopSpeed, v.Type) { MaxFuel = v.MaxFuel });

            GenerateSafeZoneTexture();
            GenerateBiomeTextures();

                    }
    }
}
