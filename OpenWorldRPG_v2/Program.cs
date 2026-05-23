
using Raylib_cs;
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

        static Vehicle car = new Vehicle(new Vector2(900, 700));

        static List<Rectangle> worldBuildings = new();
        static List<Rectangle> worldTrees = new();
        static List<Rectangle> interiorObjects = new();

        static Rectangle buildingEntrance = new Rectangle(1600, 800, 220, 220);

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG");
            Raylib.SetTargetFPS(60);

            GenerateWorld();
            GenerateInterior();

            camera.Target = player.Position;
            camera.Offset = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Rotation = 0;
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
            switch(currentScene)
            {
                case SceneState.MainMenu:
                    if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                    {
                        currentScene = SceneState.World;
                    }
                    break;

                case SceneState.World:

                    if (!car.Driving)
                    {
                        player.Update(dt, worldBuildings, worldTrees);
                    }

                    if (Raylib.CheckCollisionRecs(player.Bounds, car.Bounds))
                    {
                        if (Raylib.IsKeyPressed(KeyboardKey.F))
                        {
                            car.Driving = !car.Driving;
                        }
                    }

                    if (car.Driving)
                    {
                        car.Update(dt);

                        player.Position = car.Position;
                    }

                    if (Raylib.CheckCollisionRecs(player.Bounds, buildingEntrance))
                    {
                        if (Raylib.IsKeyPressed(KeyboardKey.E))
                        {
                            currentScene = SceneState.Building;

                            player.Position = new Vector2(300, 500);
                        }
                    }

                    camera.Target = player.Position;

                    break;

                case SceneState.Building:

                    player.Update(dt, interiorObjects, new List<Rectangle>());

                    if (Raylib.IsKeyPressed(KeyboardKey.Q))
                    {
                        currentScene = SceneState.World;

                        player.Position = new Vector2(1500, 1100);
                    }

                    camera.Target = player.Position;

                    break;
            }
        }

        static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

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
            Raylib.ClearBackground(new Color(20,20,30,255));

            Raylib.DrawText("OPEN WORLD RPG", 360, 180, 60, Color.White);

            Raylib.DrawRectangle(450, 320, 350, 80, Color.DarkBlue);
            Raylib.DrawText("PRESS ENTER TO START", 485, 348, 28, Color.White);

            Raylib.DrawRectangle(450, 440, 350, 80, Color.DarkGray);
            Raylib.DrawText("ESC TO QUIT", 540, 468, 28, Color.White);
        }

        static void DrawWorld()
        {
            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(-5000, -5000, 10000, 10000, new Color(70,170,70,255));

            Raylib.DrawRectangle(-4000, 600, 8000, 160, Color.DarkGray);

            for (int i = -4000; i < 4000; i += 120)
            {
                Raylib.DrawRectangle(i, 670, 60, 10, Color.Yellow);
            }

            foreach (Rectangle tree in worldTrees)
            {
                Raylib.DrawRectangleRec(tree, Color.DarkGreen);
            }

            foreach (Rectangle building in worldBuildings)
            {
                Raylib.DrawRectangleRec(building, new Color(120,90,70,255));
            }

            Raylib.DrawRectangleRec(buildingEntrance, Color.Brown);

            Raylib.DrawText(
                "PRESS E TO ENTER",
                (int)buildingEntrance.X,
                (int)buildingEntrance.Y - 30,
                24,
                Color.White
            );

            car.Draw();

            if (!car.Driving)
            {
                player.Draw();
            }

            Raylib.EndMode2D();

            Raylib.DrawText("WASD / Arrows = Move", 20, 20, 24, Color.White);
            Raylib.DrawText("F = Enter Vehicle", 20, 50, 24, Color.White);
            Raylib.DrawText("E = Enter Building", 20, 80, 24, Color.White);
        }

        static void DrawInterior()
        {
            Raylib.BeginMode2D(camera);

            Raylib.DrawRectangle(-2000, -2000, 4000, 4000, new Color(45,45,45,255));

            Raylib.DrawRectangle(0, 0, 1200, 900, new Color(90,70,50,255));

            foreach (Rectangle obj in interiorObjects)
            {
                Raylib.DrawRectangleRec(obj, Color.DarkBlue);
            }

            player.Draw();

            Raylib.EndMode2D();

            Raylib.DrawText("BUILDING INTERIOR", 20, 20, 30, Color.White);
            Raylib.DrawText("Q = EXIT BUILDING", 20, 60, 24, Color.White);
        }

        static void GenerateWorld()
        {
            worldBuildings.Add(new Rectangle(1000, 400, 300, 300));
            worldBuildings.Add(new Rectangle(2200, 300, 350, 350));
            worldBuildings.Add(new Rectangle(-1200, 200, 280, 280));

            for (int i = -3000; i < 3000; i += 300)
            {
                worldTrees.Add(new Rectangle(i, 1000, 70, 70));
                worldTrees.Add(new Rectangle(i, -400, 70, 70));
            }
        }

        static void GenerateInterior()
        {
            interiorObjects.Add(new Rectangle(200, 200, 240, 80));
            interiorObjects.Add(new Rectangle(600, 180, 120, 220));
            interiorObjects.Add(new Rectangle(850, 500, 180, 120));
            interiorObjects.Add(new Rectangle(300, 650, 260, 100));
        }
    }

    class Player
    {
        public Vector2 Position;

        float speed = 300f;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 pos)
        {
            Position = pos;
        }

        public void Update(float dt, List<Rectangle> collisions, List<Rectangle> trees)
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

            Vector2 oldPos = Position;

            Position += move * speed * dt;

            foreach (Rectangle rect in collisions)
            {
                if (Raylib.CheckCollisionRecs(Bounds, rect))
                {
                    Position = oldPos;
                }
            }

            foreach (Rectangle rect in trees)
            {
                if (Raylib.CheckCollisionRecs(Bounds, rect))
                {
                    Position = oldPos;
                }
            }
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, Color.Blue);
            Raylib.DrawCircle((int)Position.X + 20, (int)Position.Y + 15, 12, Color.Beige);
        }
    }

    class Vehicle
    {
        public Vector2 Position;

        public bool Driving = false;

        float speed = 650f;

        public Rectangle Bounds =>
            new Rectangle(Position.X, Position.Y, 100, 50);

        public Vehicle(Vector2 pos)
        {
            Position = pos;
        }

        public void Update(float dt)
        {
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

            Position += move * speed * dt;
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, Color.Red);

            Raylib.DrawCircle((int)Position.X + 20, (int)Position.Y + 50, 10, Color.Black);
            Raylib.DrawCircle((int)Position.X + 80, (int)Position.Y + 50, 10, Color.Black);
        }
    }
}
