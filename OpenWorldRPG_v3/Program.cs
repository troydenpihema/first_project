
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace OpenWorldRPG
{
    enum SceneState
    {
        MainMenu,
        World,
        Building
    }

    class Program
    {
        const int ScreenWidth = 1280;
        const int ScreenHeight = 720;

        static SceneState currentScene = SceneState.MainMenu;

        static Camera2D camera = new Camera2D();

        static Player player = new Player(new Vector2(400, 400));

        static List<TreeObject> trees = new();
        static List<Lake> lakes = new();
        static List<NPC> npcs = new();
        static List<Vehicle> vehicles = new();
        static List<Building> buildings = new();

        static List<FloatingText> floatingTexts = new();

        static Building currentBuilding = null;
        static float shakeDuration = 0f;
        static float shakeMagnitude = 6f;
        static void TriggerShake(float duration) => shakeDuration = duration;

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG V3");
            Raylib.SetTargetFPS(60);

            GenerateWorld();

            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Zoom = 1f;

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                Update(dt);
                Draw();
            }

            Raylib.CloseWindow();
        }

        static void Update(float dt)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Tab))
            {
            player.InventoryOpen = !player.InventoryOpen;
            }
            switch(currentScene)
            {
                case SceneState.MainMenu:

                    if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                    {
                        currentScene = SceneState.World;
                    }

                    break;

                case SceneState.World:

                    if (shakeDuration > 0) shakeDuration -= dt;
                    for (int i = floatingTexts.Count - 1; i >= 0; i--)
                    {
                        var ft = floatingTexts[i];
                        ft.Timer -= dt;
                        ft.Position.Y -= 40f * dt;
                        floatingTexts[i] = ft;
                        if (ft.Timer <= 0) floatingTexts.RemoveAt(i);
}

                    player.Update(dt, buildings, trees);

                    foreach (Vehicle vehicle in vehicles)
                    {
                        vehicle.Update(dt);

                        if (Raylib.CheckCollisionRecs(player.Bounds, vehicle.Bounds))
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.F))
                            {
                                vehicle.Driving = !vehicle.Driving;
                                player.Hidden = vehicle.Driving;
                            }

                            if (vehicle.Driving)
                            {
                                player.Position = vehicle.Position;
                            }
                        }
                    }

                    foreach (TreeObject tree in trees)
                    {
                        tree.Update(dt);

                        if (!tree.Chopped)
                        {
                            if (Vector2.Distance(player.Position, tree.Position) < 80)
                            {
                                if (Raylib.IsKeyPressed(KeyboardKey.Space))
                                {
                                    tree.Health--;

                                    if (tree.Health <= 0)
                                    {
                                        tree.Chopped = true;
                                        player.AddWoodcuttingXP(25);
                                        player.Logs += Raylib.GetRandomValue(1, 3);
                                        TriggerShake(0.15f);
                                        floatingTexts.Add(new FloatingText {   // 👈 add this
                                        Position = player.Position - new Vector2(0, 20),
                                        Text = "+25 WC XP",
                                        Timer = 1.2f,
                                        TextColor = Color.Yellow
                                    });
                                    }
                                }
                            }
                        }
                    }

                    foreach (Lake lake in lakes)
                    {
                        lake.Update(dt);
                        if (Vector2.Distance(player.Position, lake.Position) < 120)
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.R))
                            {
                                player.AddFishingXP(20);
                                player.Fish += 1;
                            }
                        }
                    }

                    foreach (Building building in buildings)
                    {
                        if (Raylib.CheckCollisionRecs(player.Bounds, building.Bounds))
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.E))
                            {
                                currentBuilding = building;
                                currentScene = SceneState.Building;
                                player.Position = new Vector2(300, 500);
                            }
                        }
                    }

                    camera.Target = player.Position;

                    break;

                case SceneState.Building:

                    player.UpdateInterior(dt, currentBuilding.InteriorObjects);

                    if (Raylib.IsKeyPressed(KeyboardKey.Q))
                    {
                        currentScene = SceneState.World;
                        player.Position = currentBuilding.ExitPosition;
                    }

                    camera.Target = player.Position;

                    break;
            }
        }

        static void Draw()
        {
            Raylib.BeginDrawing();

            switch(currentScene)
            {
                case SceneState.MainMenu:
                    DrawMenu();
                    break;

                case SceneState.World:
                    DrawWorld();
                    break;

                case SceneState.Building:
                    DrawInterior();
                    break;
            }

            Raylib.EndDrawing();
        }

        static void DrawMenu()
        {
             Raylib.ClearBackground(new Color(20, 20, 30, 255));

    // Subtle animated background pulse
    float pulse = MathF.Sin((float)Raylib.GetTime() * 1.5f) * 10f;

            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(10, 10, 20, 255));
    Raylib.DrawText("OPEN WORLD RPG", 310, 180, 64, Color.Gold);
    Raylib.DrawText("OPEN WORLD RPG", 312, 182, 64, new Color(255,200,0,80)); // shadow

    // Blinking "press enter"
    if ((int)(Raylib.GetTime() * 2) % 2 == 0)
        Raylib.DrawText("PRESS ENTER TO START", 390, 360, 34, Color.White);
        }

        static void DrawWorld()
        {
            Raylib.ClearBackground(new Color(100,180,100,255));

            if (shakeDuration > 0)
        {
        camera.Offset = new Vector2(
        ScreenWidth / 2 + Raylib.GetRandomValue(-1, 1) * shakeMagnitude,
        ScreenHeight / 2 + Raylib.GetRandomValue(-1, 1) * shakeMagnitude
            );
        }
        else
        {
            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
        }
            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(-5000, -5000, 10000, 10000, new Color(90,170,90,255));

            Raylib.DrawRectangle(-4000, 550, 8000, 180, Color.DarkGray);

            foreach (var ft in floatingTexts)
            {
            byte alpha = (byte)(255 * (ft.Timer / 1.2f));
            Raylib.DrawText(ft.Text, (int)ft.Position.X, (int)ft.Position.Y, 22,
                new Color(ft.TextColor.R, ft.TextColor.G, ft.TextColor.B, alpha));
            }

            foreach (Building building in buildings)
            {
                building.Draw();
            }

            foreach (TreeObject tree in trees)
            {
                tree.Draw();
            }

            foreach (Lake lake in lakes)
            {
                lake.Draw();
            }

            foreach (NPC npc in npcs)
            {
                npc.Draw();
            }

            foreach (Vehicle vehicle in vehicles)
            {
                vehicle.Draw();
            }

            player.Draw();

            Raylib.EndMode2D();

            DrawHUD();
        }

        static void DrawInterior()
        {
            Raylib.ClearBackground(new Color(40,40,40,255));

            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(0,0,1400,1000,currentBuilding.InteriorColor);

            foreach (Rectangle obj in currentBuilding.InteriorObjects)
            {
                Raylib.DrawRectangleRec(obj, Color.DarkBrown);
            }
            currentBuilding.InteriorNPC.Draw();
            player.Draw();

            Raylib.EndMode2D();

            Raylib.DrawText("Q = EXIT BUILDING", 20, 20, 28, Color.White);
            if (Vector2.Distance(
    player.Position,
    currentBuilding.InteriorNPC.Position
) < 120)
{
    Raylib.DrawRectangle(
        0,
        620,
        1280,
        100,
        new Color(0,0,0,180)
    );

    Raylib.DrawText(
        currentBuilding.BuildingName,
        20,
        630,
        30,
        Color.Gold
    );

    Raylib.DrawText(
        currentBuilding.InteriorNPC.Name + ": " +
        currentBuilding.InteriorNPC.Dialogue,
        20,
        670,
        24,
        Color.White
    );
}
        }

        static void DrawHUD()
        {
            Raylib.DrawRectangle(0,0,420,140,new Color(0,0,0,170));

            Raylib.DrawText($"Woodcutting Lv: {player.WoodcuttingLevel}",20,20,28,Color.White);
            Raylib.DrawText($"Fishing Lv: {player.FishingLevel}",20,50,28,Color.White);

            Raylib.DrawText($"Wood XP: {player.WoodcuttingXP}",20,85,22,Color.LightGray);
            Raylib.DrawText($"Fishing XP: {player.FishingXP}",20,110,22,Color.LightGray);
           
           Raylib.DrawText(
            "SPACE = Chop Tree | R = Fish | TAB = Inventory",
                20,
                150,
                22,
                Color.White
                );
           
            if (player.InventoryOpen)
{
    Raylib.DrawRectangle(
        420,
        120,
        440,
        420,
        new Color(0,0,0,220)
    );

    Raylib.DrawText(
        "INVENTORY",
        540,
        150,
        40,
        Color.Gold
    );

    Raylib.DrawText(
        $"Logs: {player.Logs}",
        500,
        240,
        32,
        Color.White
    );

    Raylib.DrawText(
        $"Fish: {player.Fish}",
        500,
        290,
        32,
        Color.White
    );

    Raylib.DrawText(
        $"Money: ${player.Money}",
        500,
        340,
        32,
        Color.White
    );

    Raylib.DrawText(
        "TAB = Close Inventory",
        470,
        470,
        24,
        Color.LightGray
    );
}
        }

        static void GenerateWorld()
        {
            for (int i = -2500; i < 2500; i += 250)
            {
                trees.Add(new TreeObject(new Vector2(i, -300)));
                trees.Add(new TreeObject(new Vector2(i, 1200)));
            }

            lakes.Add(new Lake(new Vector2(700, 1200)));
            lakes.Add(new Lake(new Vector2(-900, -600)));

        buildings.Add(new Building(
            new Rectangle(900, 400, 280, 240),
            new Color(180,120,90,255),
            new Color(90,70,50,255),
            new Vector2(1100,700),
            "BANK",
            new NPC(
            new Vector2(700,450),
            "Bank Manager",
            "Chur maori. Welcome to Waikato Bank."
    )
));

        buildings.Add(new Building(
            new Rectangle(1700, 300, 320, 280),
            Color.DarkBlue,
            new Color(50,60,90,255),
            new Vector2(1800,650),
            "DBar",
            new NPC(
            new Vector2(600,420),
            "Dbar Owner",
            "Grab a woodys and relax at Dbar."
    )
));

           buildings.Add(new Building(
            new Rectangle(-1200, 250, 300, 240),
            Color.DarkGreen,
            new Color(40,90,50,255),
            new Vector2(-1050,600),
            "STORE",
            new NPC(
            new Vector2(500,420),
            "Store Clerk",
            "Need supplies for fishing? Show me the moolack"
    )
));

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

            vehicles.Add(new Vehicle(new Vector2(300,800), Color.Red, 650));
            vehicles.Add(new Vehicle(new Vector2(1200,700), Color.Yellow, 900));
            vehicles.Add(new Vehicle(new Vector2(-400,650), Color.DarkBlue, 500));
        }
    }
    struct FloatingText
    {
    public Vector2 Position;
    public string Text;
    public float Timer;
    public Color TextColor;
    }
    class Player
    {
        public Vector2 Position;

        public bool Hidden = false;

        float speed = 300;

        public int WoodcuttingLevel = 1;
        public int FishingLevel = 1;

        public int WoodcuttingXP = 0;
        public int FishingXP = 0;

        public int Logs = 0;
        public int Fish = 0;
        public int Money = 0;

        public bool InventoryOpen = false;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 position)
        {
            Position = position;
        }

        public void Update(float dt, List<Building> buildings, List<TreeObject> trees)
        {
            Vector2 move = GetInput();

            Vector2 oldPos = Position;

            Position += move * speed * dt;

            foreach (Building building in buildings)
{
    Rectangle collisionBox = new Rectangle(
        building.Bounds.X,
        building.Bounds.Y,
        building.Bounds.Width,
        building.Bounds.Height - 90
    );

    if (Raylib.CheckCollisionRecs(Bounds, collisionBox))
    {
        Position = oldPos;
    }
}

            foreach (TreeObject tree in trees)
            {
                if (!tree.Chopped)
                {
                    if (Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
                    {
                        Position = oldPos;
                    }
                }
            }
        }

        public void UpdateInterior(float dt, List<Rectangle> objects)
        {
            Vector2 move = GetInput();

            Vector2 oldPos = Position;

            Position += move * speed * dt;

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
            Vector2 move = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))
                move.Y -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
                move.Y += 1;

            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
                move.X -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
                move.X += 1;

            if (move != Vector2.Zero)
                move = Vector2.Normalize(move);

            return move;
        }

        public void AddWoodcuttingXP(int xp)
        {
            if (WoodcuttingLevel >= 100) return;

            WoodcuttingXP += xp;

            int requiredXP = WoodcuttingLevel * WoodcuttingLevel * 50;

            if (WoodcuttingXP >= requiredXP)
            {
                WoodcuttingXP = 0;
                WoodcuttingLevel++;
            }
        }

        public void AddFishingXP(int xp)
        {
            if (FishingLevel >= 100) return;

            FishingXP += xp;

            int requiredXP = FishingLevel * FishingLevel * 50;

            if (FishingXP >= requiredXP)
            {
                FishingXP = 0;
                FishingLevel++;
            }
        }

        public void Draw()
        {
            if (Hidden) return;

            Raylib.DrawCircle((int)Position.X + 20,(int)Position.Y + 12,12,Color.Beige);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 24,20,30,Color.Blue);

            Raylib.DrawRectangle((int)Position.X + 10,(int)Position.Y + 54,8,12,Color.Black);
            Raylib.DrawRectangle((int)Position.X + 22,(int)Position.Y + 54,8,12,Color.Black);

            Raylib.DrawRectangle((int)Position.X + 2,(int)Position.Y + 26,8,18,Color.Beige);
            Raylib.DrawRectangle((int)Position.X + 30,(int)Position.Y + 26,8,18,Color.Beige);
        }
    }

    class TreeObject
{
    public Vector2 Position;

    public bool Chopped = false;

    public int Health = 3;

    float respawnTimer = 0f;

    public Rectangle Bounds =>
        new Rectangle(Position.X, Position.Y, 60, 80);

    public TreeObject(Vector2 pos)
    {
        Position = pos;
    }

    public void Update(float dt)
    {
        if (Chopped)
        {
            respawnTimer += dt;

            if (respawnTimer >= 5f)
            {
                Chopped = false;
                Health = 3;
                respawnTimer = 0f;
            }
        }
    }

    public void Draw()
    {
        if (Chopped)
        {
            // stump
            Raylib.DrawRectangle(
                (int)Position.X + 12,
                (int)Position.Y + 55,
                36,
                18,
                Color.Brown
            );

            return;
        }

        // trunk
        Raylib.DrawRectangle(
            (int)Position.X + 20,
            (int)Position.Y + 40,
            20,
            40,
            Color.Brown
        );

        // leaves
        Raylib.DrawCircle(
            (int)Position.X + 30,
            (int)Position.Y + 25,
            35,
            Color.DarkGreen
        );
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

        public void Draw()
        {
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 120, new Color(30, 100, 200, 255));

            float ripple = MathF.Sin(rippleTimer * 2f) * 6f;
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(90 + ripple), Color.SkyBlue);
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, (int)(60 + ripple * 0.5f), Color.SkyBlue);
        }
    }

    class NPC
{
    public Vector2 Position;

    public string Name;
    public string Dialogue;

    public NPC(Vector2 pos, string name, string dialogue)
    {
        Position = pos;
        Name = name;
        Dialogue = dialogue;
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

    class Vehicle
    {
        public Vector2 Position;

        public bool Driving = false;

        float speed;

        Color color;
        Vector2 velocity = Vector2.Zero;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 100, 50);

        public Vehicle(Vector2 pos, Color vehicleColor, float vehicleSpeed)
        {
            Position = pos;
            color = vehicleColor;
            speed = vehicleSpeed;
        }

        public void Update(float dt)
        {
            if (!Driving) return;

            Vector2 move = Vector2.Zero;

            if (Raylib.IsKeyDown(KeyboardKey.Up))
                move.Y -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.Down))
                move.Y += 1;

            if (Raylib.IsKeyDown(KeyboardKey.Left))
                move.X -= 1;

            if (Raylib.IsKeyDown(KeyboardKey.Right))
                move.X += 1;

            if (move != Vector2.Zero)
                move = Vector2.Normalize(move);

            Vector2 targetVelocity = move * speed;
            velocity = Vector2.Lerp(velocity, targetVelocity, dt * 5f);
            Position += velocity * dt;
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, color);

            Raylib.DrawRectangle((int)Position.X + 18,(int)Position.Y + 8,64,20,Color.Black);

            Raylib.DrawCircle((int)Position.X + 20,(int)Position.Y + 50,10,Color.Black);
            Raylib.DrawCircle((int)Position.X + 80,(int)Position.Y + 50,10,Color.Black);
        }
    }

    class Building
    {
        public Rectangle Bounds;

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
            NPC npc
        )
        {
            Bounds = bounds;
            ExteriorColor = exterior;
            InteriorColor = interior;
            ExitPosition = exitPos;
            BuildingName = buildingName;
            InteriorNPC = npc;

            InteriorObjects.Add(new Rectangle(200,200,240,80));
            InteriorObjects.Add(new Rectangle(600,400,200,120));
            InteriorObjects.Add(new Rectangle(900,250,150,200));
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, ExteriorColor);

            Raylib.DrawRectangle(
                (int)Bounds.X + 100,
                (int)Bounds.Y + 160,
                60,
                80,
                Color.Brown
            );

            Raylib.DrawRectangle(
                (int)Bounds.X + 30,
                (int)Bounds.Y + 40,
                60,
                60,
                Color.LightGray
            );

            Raylib.DrawRectangle(
                (int)Bounds.X + 180,
                (int)Bounds.Y + 40,
                60,
                60,
                Color.LightGray
            );
        }
    }
}
