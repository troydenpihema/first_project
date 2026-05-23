using Raylib_cs;
using System.Numerics;

namespace OpenWorldRPG
{
    enum GameScene
    {
        MainMenu,
        World,
        BuildingInterior
    }

    class Program
    {
        const int ScreenWidth = 1280;
        const int ScreenHeight = 720;

        static GameScene currentScene = GameScene.MainMenu;

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Open World RPG");
            Raylib.SetTargetFPS(60);

            Player player = new Player(new Vector2(400, 300));
            Vehicle car = new Vehicle(new Vector2(700, 500));

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                switch (currentScene)
                {
                    case GameScene.MainMenu:
                        UpdateMainMenu();
                        break;

                    case GameScene.World:
                        player.Update(dt);

                        if (Raylib.IsKeyPressed(KeyboardKey.E))
                        {
                            currentScene = GameScene.BuildingInterior;
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

                        break;

                    case GameScene.BuildingInterior:
                        if (Raylib.IsKeyPressed(KeyboardKey.Q))
                        {
                            currentScene = GameScene.World;
                        }
                        break;
                }

                DrawGame(player, car);
            }

            Raylib.CloseWindow();
        }

        static void UpdateMainMenu()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                currentScene = GameScene.World;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                Raylib.CloseWindow();
            }
        }

        static void DrawGame(Player player, Vehicle car)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);

            switch (currentScene)
            {
                case GameScene.MainMenu:
                    DrawMainMenu();
                    break;

                case GameScene.World:
                    DrawWorld(player, car);
                    break;

                case GameScene.BuildingInterior:
                    DrawInterior();
                    break;
            }

            Raylib.EndDrawing();
        }

        static void DrawMainMenu()
        {
            Raylib.DrawText("OPEN WORLD RPG", 400, 180, 50, Color.White);

            Raylib.DrawRectangle(450, 300, 350, 70, Color.DarkBlue);
            Raylib.DrawText("PRESS ENTER TO START", 485, 322, 28, Color.White);

            Raylib.DrawRectangle(450, 400, 350, 70, Color.DarkGray);
            Raylib.DrawText("ESC TO QUIT", 545, 422, 28, Color.White);
        }

        static void DrawWorld(Player player, Vehicle car)
        {
            Raylib.DrawRectangle(-5000, -5000, 10000, 10000, new Color(80, 170, 80, 255));

            Raylib.DrawRectangle(0, 520, 2000, 120, Color.DarkGray);

            for (int i = 0; i < 2000; i += 80)
            {
                Raylib.DrawRectangle(i, 575, 40, 10, Color.Yellow);
            }

            Raylib.DrawRectangle(850, 260, 220, 220, Color.Brown);
            Raylib.DrawText("PRESS E TO ENTER", 855, 230, 24, Color.White);

            Raylib.DrawCircle(250, 250, 70, Color.DarkGreen);
            Raylib.DrawCircle(500, 350, 80, Color.Blue);

            car.Draw();

            if (!car.Driving)
            {
                player.Draw();
            }

            Raylib.DrawText("Move: WASD / Arrow Keys", 20, 20, 24, Color.White);
            Raylib.DrawText("F = Enter/Exit Car", 20, 50, 24, Color.White);
            Raylib.DrawText("E = Enter Building", 20, 80, 24, Color.White);
        }

        static void DrawInterior()
        {
            Raylib.ClearBackground(new Color(45, 45, 45, 255));

            Raylib.DrawRectangle(140, 90, 1000, 540, new Color(110, 90, 70, 255));

            Raylib.DrawRectangle(220, 140, 180, 90, Color.DarkBrown);
            Raylib.DrawRectangle(520, 240, 220, 100, Color.DarkBlue);
            Raylib.DrawRectangle(850, 180, 150, 150, Color.DarkGreen);

            Raylib.DrawText("BUILDING INTERIOR", 420, 50, 40, Color.White);
            Raylib.DrawText("PRESS Q TO EXIT", 460, 650, 28, Color.LightGray);
        }
    }

    class Player
    {
        public Vector2 Position;

        float speed = 260;

        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 40, 60);

        public Player(Vector2 position)
        {
            Position = position;
        }

        public void Update(float dt)
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
            {
                move = Vector2.Normalize(move);
            }

            Position += move * speed * dt;
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Bounds, Color.Blue);
            Raylib.DrawCircle((int)Position.X + 20, (int)Position.Y + 12, 10, Color.Beige);
        }
    }

    class Vehicle
    {
        public Vector2 Position;
        public bool Driving = false;

        float speed = 500;

        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 100, 50);

        public Vehicle(Vector2 position)
        {
            Position = position;
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
            {
                move = Vector2.Normalize(move);
            }

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
