using System;
using System.Numerics;
using Raylib_cs;

class Program
{
    const int ScreenWidth = 1000;
    const int ScreenHeight = 600;

    static void Main()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "C# Platform Game");
        Raylib.SetTargetFPS(60);

        Player player = new Player(new Vector2(80, 420));

        Rectangle[] platforms =
        {
            new Rectangle(0, 560, 1000, 40),
            new Rectangle(160, 470, 160, 20),
            new Rectangle(390, 400, 170, 20),
            new Rectangle(650, 330, 180, 20),
            new Rectangle(230, 270, 130, 20),
            new Rectangle(520, 210, 160, 20),
            new Rectangle(780, 150, 140, 20)
        };

        Coin[] coins =
        {
            new Coin(new Vector2(210, 430)),
            new Coin(new Vector2(450, 360)),
            new Coin(new Vector2(710, 290)),
            new Coin(new Vector2(270, 230)),
            new Coin(new Vector2(580, 170)),
            new Coin(new Vector2(840, 110))
        };

        Enemy[] enemies =
        {
            new Enemy(new Vector2(400, 535), 320, 600),
            new Enemy(new Vector2(680, 305), 650, 810)
        };

        Rectangle goal = new Rectangle(900, 90, 45, 60);

        int score = 0;
        bool gameWon = false;
        bool gameOver = false;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (!gameWon && !gameOver)
            {
                player.Update(dt, platforms);

                foreach (Enemy enemy in enemies)
                {
                    enemy.Update(dt);

                    if (Raylib.CheckCollisionRecs(player.Bounds, enemy.Bounds))
                    {
                        gameOver = true;
                    }
                }

                foreach (Coin coin in coins)
                {
                    if (!coin.Collected && Raylib.CheckCollisionRecs(player.Bounds, coin.Bounds))
                    {
                        coin.Collected = true;
                        score += 10;
                    }
                }

                if (Raylib.CheckCollisionRecs(player.Bounds, goal))
                {
                    gameWon = true;
                }

                if (player.Position.Y > ScreenHeight + 200)
                {
                    gameOver = true;
                }
            }

            if ((gameWon || gameOver) && Raylib.IsKeyPressed(KeyboardKey.R))
            {
                player = new Player(new Vector2(80, 420));

                for (int i = 0; i < coins.Length; i++)
                {
                    coins[i].Collected = false;
                }

                enemies[0] = new Enemy(new Vector2(400, 535), 320, 600);
                enemies[1] = new Enemy(new Vector2(680, 305), 650, 810);

                score = 0;
                gameWon = false;
                gameOver = false;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(25, 28, 38, 255));

            DrawBackground();

            foreach (Rectangle platform in platforms)
            {
                Raylib.DrawRectangleRec(platform, new Color(95, 70, 45, 255));
                Raylib.DrawRectangle((int)platform.X, (int)platform.Y, (int)platform.Width, 5, new Color(70, 170, 90, 255));
            }

            foreach (Coin coin in coins)
            {
                coin.Draw();
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw();
            }

            Raylib.DrawRectangleRec(goal, new Color(90, 190, 255, 255));
            Raylib.DrawRectangleLinesEx(goal, 3, Color.White);
            Raylib.DrawText("GOAL", (int)goal.X - 5, (int)goal.Y - 25, 18, Color.White);

            player.Draw();

            Raylib.DrawText($"Score: {score}", 20, 20, 28, Color.White);
            Raylib.DrawText("Move: A/D or Arrows | Jump: Space/W/Up", 20, 55, 20, Color.LightGray);

            if (gameWon)
            {
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 160));
                Raylib.DrawText("YOU WIN!", 390, 230, 55, Color.Gold);
                Raylib.DrawText($"Final Score: {score}", 410, 300, 30, Color.White);
                Raylib.DrawText("Press R to restart", 390, 350, 28, Color.White);
            }

            if (gameOver)
            {
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 160));
                Raylib.DrawText("GAME OVER", 350, 230, 55, Color.Red);
                Raylib.DrawText("Press R to restart", 385, 310, 28, Color.White);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void DrawBackground()
    {
        Raylib.DrawCircle(850, 90, 45, new Color(255, 220, 120, 255));

        for (int i = 0; i < 6; i++)
        {
            int x = i * 180 + 40;
            int y = 120 + (i % 2) * 40;

            Raylib.DrawCircle(x, y, 25, new Color(60, 65, 85, 255));
            Raylib.DrawCircle(x + 30, y + 5, 30, new Color(60, 65, 85, 255));
            Raylib.DrawCircle(x + 65, y, 25, new Color(60, 65, 85, 255));
            Raylib.DrawRectangle(x, y, 65, 25, new Color(60, 65, 85, 255));
        }
    }
}

class Player
{
    public Vector2 Position;
    private Vector2 velocity;

    private const float Width = 38;
    private const float Height = 52;
    private const float Speed = 280;
    private const float JumpForce = -620;
    private const float Gravity = 1500;

    private bool onGround = false;
    private bool facingRight = true;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Width, Height);

    public Player(Vector2 startPosition)
    {
        Position = startPosition;
        velocity = Vector2.Zero;
    }

    public void Update(float dt, Rectangle[] platforms)
    {
        float move = 0;

        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
        {
            move = -1;
            facingRight = false;
        }

        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
        {
            move = 1;
            facingRight = true;
        }

        velocity.X = move * Speed;

        if ((Raylib.IsKeyPressed(KeyboardKey.Space) ||
             Raylib.IsKeyPressed(KeyboardKey.W) ||
             Raylib.IsKeyPressed(KeyboardKey.Up)) && onGround)
        {
            velocity.Y = JumpForce;
            onGround = false;
        }

        velocity.Y += Gravity * dt;

        Position.X += velocity.X * dt;
        HandleHorizontalCollisions(platforms);

        Position.Y += velocity.Y * dt;
        HandleVerticalCollisions(platforms);

        if (Position.X < 0)
        {
            Position.X = 0;
        }

        if (Position.X + Width > 1000)
        {
            Position.X = 1000 - Width;
        }
    }

    private void HandleHorizontalCollisions(Rectangle[] platforms)
    {
        foreach (Rectangle platform in platforms)
        {
            if (Raylib.CheckCollisionRecs(Bounds, platform))
            {
                if (velocity.X > 0)
                {
                    Position.X = platform.X - Width;
                }
                else if (velocity.X < 0)
                {
                    Position.X = platform.X + platform.Width;
                }

                velocity.X = 0;
            }
        }
    }

    private void HandleVerticalCollisions(Rectangle[] platforms)
    {
        onGround = false;

        foreach (Rectangle platform in platforms)
        {
            if (Raylib.CheckCollisionRecs(Bounds, platform))
            {
                if (velocity.Y > 0)
                {
                    Position.Y = platform.Y - Height;
                    velocity.Y = 0;
                    onGround = true;
                }
                else if (velocity.Y < 0)
                {
                    Position.Y = platform.Y + platform.Height;
                    velocity.Y = 0;
                }
            }
        }
    }

    public void Draw()
    {
        Color bodyColor = new Color(80, 180, 255, 255);
        Color faceColor = new Color(255, 225, 170, 255);

        Raylib.DrawRectangleRounded(Bounds, 0.25f, 8, bodyColor);

        Rectangle face = new Rectangle(Position.X + 8, Position.Y + 8, 22, 18);
        Raylib.DrawRectangleRounded(face, 0.35f, 8, faceColor);

        if (facingRight)
        {
            Raylib.DrawCircle((int)Position.X + 24, (int)Position.Y + 17, 3, Color.Black);
        }
        else
        {
            Raylib.DrawCircle((int)Position.X + 14, (int)Position.Y + 17, 3, Color.Black);
        }

        if (onGround)
        {
            Raylib.DrawRectangle((int)Position.X + 6, (int)Position.Y + 50, 10, 8, Color.DarkBlue);
            Raylib.DrawRectangle((int)Position.X + 22, (int)Position.Y + 50, 10, 8, Color.DarkBlue);
        }
    }
}

class Coin
{
    public Vector2 Position;
    public bool Collected = false;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 24, 24);

    public Coin(Vector2 position)
    {
        Position = position;
    }

    public void Draw()
    {
        if (Collected)
        {
            return;
        }

        Raylib.DrawCircle((int)Position.X + 12, (int)Position.Y + 12, 12, Color.Gold);
        Raylib.DrawCircleLines((int)Position.X + 12, (int)Position.Y + 12, 12, Color.Orange);
        Raylib.DrawText("$", (int)Position.X + 7, (int)Position.Y + 3, 20, Color.Brown);
    }
}

class Enemy
{
    public Vector2 Position;
    private float speed = 130;
    private float leftLimit;
    private float rightLimit;
    private int direction = 1;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 38, 25);

    public Enemy(Vector2 position, float left, float right)
    {
        Position = position;
        leftLimit = left;
        rightLimit = right;
    }

    public void Update(float dt)
    {
        Position.X += speed * direction * dt;

        if (Position.X < leftLimit)
        {
            Position.X = leftLimit;
            direction = 1;
        }

        if (Position.X > rightLimit)
        {
            Position.X = rightLimit;
            direction = -1;
        }
    }

    public void Draw()
    {
        Raylib.DrawRectangleRounded(Bounds, 0.35f, 8, new Color(220, 70, 70, 255));
        Raylib.DrawCircle((int)Position.X + 10, (int)Position.Y + 9, 3, Color.Black);
        Raylib.DrawCircle((int)Position.X + 28, (int)Position.Y + 9, 3, Color.Black);
        Raylib.DrawRectangle((int)Position.X + 12, (int)Position.Y + 17, 14, 3, Color.Black);
    }
}
