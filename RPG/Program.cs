using System;
using System.Numerics;
using Raylib_cs;

const int ScreenWidth = 1280;
const int ScreenHeight = 720;

Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG");
Raylib.SetTargetFPS(60);

Camera2D camera = new Camera2D();
camera.Zoom = 1.0f;

Player player = new Player();

List<TreeObject> trees = new();
List<LakeObject> lakes = new();
List<NPC> npcs = new();
List<Building> buildings = new();
List<Vehicle> vehicles = new();

GenerateWorld();

Vehicle? currentVehicle = null;

bool insideBuilding = false;
string currentDialogue = "";

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();

    if (currentVehicle == null)
    {
        player.Update(dt, trees, buildings);
    }
    else
    {
        currentVehicle.Update(dt, trees, buildings);
        player.Position = currentVehicle.Position;

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            currentVehicle.DriverInside = false;
            currentVehicle = null;
        }
    }

    camera.Target = player.Position;
    camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);

    currentDialogue = "";

    foreach (NPC npc in npcs)
    {
        if (Vector2.Distance(player.Position, npc.Position) < 80)
        {
            currentDialogue = npc.Dialogue;

            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                npc.Talking = !npc.Talking;
            }
        }
    }

    foreach (TreeObject tree in trees)
    {
        if (!tree.Chopped &&
            Vector2.Distance(player.Position, tree.Position) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F))
            {
                tree.Health--;

                if (tree.Health <= 0)
                {
                    tree.Chopped = true;

                    player.LumberjackXP += 25;
                    player.CheckLevelUp();
                }
            }
        }
    }

    foreach (LakeObject lake in lakes)
    {
        if (Vector2.Distance(player.Position, lake.Position) < 140)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                player.FishingXP += 30;
                player.CheckLevelUp();
            }
        }
    }

    foreach (Vehicle vehicle in vehicles)
    {
        if (Vector2.Distance(player.Position, vehicle.Position) < 80)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                currentVehicle = vehicle;
                vehicle.DriverInside = true;
            }
        }
    }

    foreach (Building building in buildings)
    {
        if (Raylib.CheckCollisionRecs(player.Bounds, building.Bounds))
        {
            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                insideBuilding = !insideBuilding;
            }
        }
    }

    Raylib.BeginDrawing();

    Raylib.ClearBackground(new Color(120, 200, 255, 255));

    Raylib.BeginMode2D(camera);

    DrawWorld();

    foreach (TreeObject tree in trees)
    {
        tree.Draw();
    }

    foreach (LakeObject lake in lakes)
    {
        lake.Draw();
    }

    foreach (NPC npc in npcs)
    {
        npc.Draw();
    }

    foreach (Building building in buildings)
    {
        building.Draw();
    }

    foreach (Vehicle vehicle in vehicles)
    {
        vehicle.Draw();
    }

    player.Draw();

    Raylib.EndMode2D();

    DrawUI();

    if (insideBuilding)
    {
        Raylib.DrawRectangle(250, 120, 780, 480, new Color(30, 30, 30, 240));

        Raylib.DrawText("Inside Building", 470, 180, 40, Color.White);
        Raylib.DrawText("Press E to leave", 480, 260, 28, Color.LightGray);
    }

    if (currentDialogue != "")
    {
        Raylib.DrawRectangle(100, 560, 1080, 120, new Color(0, 0, 0, 220));
        Raylib.DrawText(currentDialogue, 130, 610, 28, Color.White);
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

void DrawUI()
{
    Raylib.DrawRectangle(10, 10, 340, 140, new Color(0, 0, 0, 180));

    Raylib.DrawText($"Lumberjack Lv: {player.LumberjackLevel}", 25, 25, 24, Color.White);
    Raylib.DrawText($"Lumberjack XP: {player.LumberjackXP}", 25, 55, 20, Color.LightGray);

    Raylib.DrawText($"Fishing Lv: {player.FishingLevel}", 25, 90, 24, Color.White);
    Raylib.DrawText($"Fishing XP: {player.FishingXP}", 25, 120, 20, Color.LightGray);
}

void DrawWorld()
{
    Raylib.DrawRectangle(-5000, -5000, 10000, 10000, new Color(90, 180, 90, 255));
}

void GenerateWorld()
{
    Random random = new Random();

    for (int i = 0; i < 80; i++)
    {
        trees.Add(new TreeObject(
            new Vector2(
                random.Next(-4000, 4000),
                random.Next(-3000, 3000)
            )
        ));
    }

    for (int i = 0; i < 15; i++)
    {
        lakes.Add(new LakeObject(
            new Vector2(
                random.Next(-3500, 3500),
                random.Next(-2500, 2500)
            )
        ));
    }

    npcs.Add(new NPC(new Vector2(500, 440), "Welcome to the village traveler."));
    npcs.Add(new NPC(new Vector2(1700, 440), "Fishing helps you earn money later."));

    buildings.Add(new Building(new Vector2(700, 320)));
    buildings.Add(new Building(new Vector2(1900, 320)));
    buildings.Add(new Building(new Vector2(-1200, -300)));
    buildings.Add(new Building(new Vector2(2800, 900)));

    vehicles.Add(new Vehicle(new Vector2(1000, 450)));
    vehicles.Add(new Vehicle(new Vector2(-800, -200)));
    vehicles.Add(new Vehicle(new Vector2(2200, 700)));
}

class Player
{
    public Vector2 Position = new Vector2(100, 400);

    public int LumberjackLevel = 1;
    public int FishingLevel = 1;

    public int LumberjackXP = 0;
    public int FishingXP = 0;

    float speed = 250;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 40, 70);

    public void Update(float dt, List<TreeObject> trees, List<Building> buildings)
    {
        Vector2 movement = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.A) ||
            Raylib.IsKeyDown(KeyboardKey.Left))
        {
            movement.X -= 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.D) ||
            Raylib.IsKeyDown(KeyboardKey.Right))
        {
            movement.X += 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.W) ||
            Raylib.IsKeyDown(KeyboardKey.Up))
        {
            movement.Y -= 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.S) ||
            Raylib.IsKeyDown(KeyboardKey.Down))
        {
            movement.Y += 1;
        }

        if (movement != Vector2.Zero)
        {
            movement = Vector2.Normalize(movement);
        }

        Vector2 oldPosition = Position;

        Position += movement * speed * dt;

        foreach (TreeObject tree in trees)
        {
            if (!tree.Chopped &&
                Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            {
                Position = oldPosition;
            }
        }

        foreach (Building building in buildings)
        {
            if (Raylib.CheckCollisionRecs(Bounds, building.Bounds))
            {
                Position = oldPosition;
            }
        }
    }

    public void CheckLevelUp()
    {
        while (LumberjackLevel < 100 &&
               LumberjackXP >= XPRequired(LumberjackLevel))
        {
            LumberjackXP -= XPRequired(LumberjackLevel);
            LumberjackLevel++;
        }

        while (FishingLevel < 100 &&
               FishingXP >= XPRequired(FishingLevel))
        {
            FishingXP -= XPRequired(FishingLevel);
            FishingLevel++;
        }
    }

    int XPRequired(int level)
    {
        return 100 + (level * level * 25);
    }

    public void Draw()
    {
        Raylib.DrawRectangleRec(Bounds, Color.Blue);

        Raylib.DrawCircle((int)Position.X + 20, (int)Position.Y + 15, 10, Color.Beige);

        Raylib.DrawRectangle((int)Position.X + 42, (int)Position.Y + 10, 25, 5, Color.Brown);

        Raylib.DrawRectangle((int)Position.X + 42, (int)Position.Y + 25, 25, 3, Color.DarkBlue);
    }
}

class TreeObject
{
    public Vector2 Position;
    public bool Chopped = false;

    public Rectangle Bounds => new Rectangle(Position.X - 20, Position.Y - 20, 40, 80);
    public int Health = 3;

    public TreeObject(Vector2 position)
    {
        Position = position;
    }

    public void Draw()
    {
        if (Chopped)
        {
            Raylib.DrawRectangle((int)Position.X, (int)Position.Y + 50, 40, 20, Color.Brown);
            return;
        }

        Raylib.DrawRectangle((int)Position.X + 15, (int)Position.Y + 50, 10, 40, Color.Brown);
        Raylib.DrawCircle((int)Position.X + 20, (int)Position.Y + 30, 35, Color.Green);
    }
}

class LakeObject
{
    public Vector2 Position;

    public LakeObject(Vector2 position)
    {
        Position = position;
    }

    public void Draw()
    {
        Raylib.DrawEllipse((int)Position.X, (int)Position.Y, 120, 50, Color.Blue);
    }
}

class NPC
{
    public Vector2 Position;
    public string Dialogue;
    public bool Talking = false;

    public NPC(Vector2 position, string dialogue)
    {
        Position = position;
        Dialogue = dialogue;
    }

    public void Draw()
    {
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, 40, 70, Color.Purple);
    }
}

class Vehicle
{
    public Vector2 Position;

    public bool DriverInside = false;

    float speed = 420;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 90, 50);

    public Vehicle(Vector2 position)
    {
        Position = position;
    }

    public void Update(
        float dt,
        List<TreeObject> trees,
        List<Building> buildings
    )
    {
        Vector2 movement = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.Left))
        {
            movement.X -= 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.Right))
        {
            movement.X += 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.Up))
        {
            movement.Y -= 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.Down))
        {
            movement.Y += 1;
        }

        if (movement != Vector2.Zero)
        {
            movement = Vector2.Normalize(movement);
        }

        Vector2 oldPosition = Position;

        Position += movement * speed * dt;

        foreach (TreeObject tree in trees)
        {
            if (!tree.Chopped &&
                Raylib.CheckCollisionRecs(Bounds, tree.Bounds))
            {
                Position = oldPosition;
            }
        }

        foreach (Building building in buildings)
        {
            if (Raylib.CheckCollisionRecs(Bounds, building.Bounds))
            {
                Position = oldPosition;
            }
        }
    }

    public void Draw()
    {
        Raylib.DrawRectangleRec(Bounds, Color.Red);

        Raylib.DrawRectangle(
            (int)Position.X + 10,
            (int)Position.Y + 10,
            20,
            12,
            Color.Black
        );

        Raylib.DrawRectangle(
            (int)Position.X + 60,
            (int)Position.Y + 10,
            20,
            12,
            Color.Black
        );

        Raylib.DrawCircle(
            (int)Position.X + 15,
            (int)Position.Y + 50,
            10,
            Color.Black
        );

        Raylib.DrawCircle(
            (int)Position.X + 75,
            (int)Position.Y + 50,
            10,
            Color.Black
        );
    }
}

class Building
{
    public Vector2 Position;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 180, 180);

    public Building(Vector2 position)
    {
        Position = position;
    }

    public void Draw()
    {
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, 180, 180, Color.Brown);
        Raylib.DrawTriangle(
            new Vector2(Position.X - 20, Position.Y),
            new Vector2(Position.X + 90, Position.Y - 80),
            new Vector2(Position.X + 200, Position.Y),
            Color.DarkBrown
        );

        Raylib.DrawRectangle((int)Position.X + 70, (int)Position.Y + 110, 40, 70, Color.DarkBrown);
    }
}

