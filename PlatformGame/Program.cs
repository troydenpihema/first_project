using System;
using System.Numerics;
using Raylib_cs;

class Program
{
    const int ScreenWidth = 1000;
    const int ScreenHeight = 600;

    static int currentLevel = 1;
    static int score = 0;

    static void Main()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "C# Platform Game - 3 Levels");
        Raylib.SetTargetFPS(60);

        LevelData level = LoadLevel(currentLevel);
        Player player = new Player(level.PlayerStart);

        bool gameWon = false;
        bool gameOver = false;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (!gameWon && !gameOver)
            {
                player.Update(dt, level.Platforms);

                foreach (Enemy enemy in level.Enemies)
                {
                    enemy.Update(dt);

                    if (Raylib.CheckCollisionRecs(player.Bounds, enemy.Bounds))
                    {
                        gameOver = true;
                    }
                }

                foreach (Coin coin in level.Coins)
                {
                    if (!coin.Collected && Raylib.CheckCollisionRecs(player.Bounds, coin.Bounds))
                    {
                        coin.Collected = true;
                        score += 10;
                    }
                }

                if (Raylib.CheckCollisionRecs(player.Bounds, level.Goal))
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
                level = LoadLevel(currentLevel);
                player = new Player(level.PlayerStart);
                gameWon = false;
                gameOver = false;
            }

            if (gameWon && Raylib.IsKeyPressed(KeyboardKey.N))
            {
                if (currentLevel < 3)
                {
                    currentLevel++;
                    level = LoadLevel(currentLevel);
                    player = new Player(level.PlayerStart);
                    gameWon = false;
                    gameOver = false;
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(25, 28, 38, 255));

            DrawBackground();

            foreach (Rectangle platform in level.Platforms)
            {
                Raylib.DrawRectangleRec(platform, new Color(95, 70, 45, 255));
                Raylib.DrawRectangle((int)platform.X, (int)platform.Y, (int)platform.Width, 5, new Color(70, 170, 90, 255));
            }

            foreach (Coin coin in level.Coins)
            {
                coin.Draw();
            }

            foreach (Enemy enemy in level.Enemies)
            {
                enemy.Draw();
            }

            Raylib.DrawRectangleRec(level.Goal, new Color(90, 190, 255, 255));
            Raylib.DrawRectangleLinesEx(level.Goal, 3, Color.White);
            Raylib.DrawText("GOAL", (int)level.Goal.X - 5, (int)level.Goal.Y - 25, 18, Color.White);

            player.Draw();

            Raylib.DrawText($"Level: {currentLevel}", 20, 20, 28, Color.White);
            Raylib.DrawText($"Score: {score}", 20, 55, 28, Color.White);
            Raylib.DrawText("Move: A/D or Arrows | Jump: Space/W/Up", 20, 90, 20, Color.LightGray);

            if (gameWon)
            {
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 170));

                if (currentLevel < 3)
                {
                    Raylib.DrawText($"LEVEL {currentLevel} COMPLETE!", 270, 220, 45, Color.Gold);
                    Raylib.DrawText("Press N for next level", 350, 300, 28, Color.White);
                    Raylib.DrawText("Press R to replay level", 350, 340, 24, Color.LightGray);
                }
                else
                {
                    Raylib.DrawText("YOU BEAT THE GAME!", 250, 220, 48, Color.Gold);
                    Raylib.DrawText($"Final Score: {score}", 390, 295, 30, Color.White);
                    Raylib.DrawText("Press R to replay Level 3", 345, 345, 24, Color.LightGray);
                }
            }

            if (gameOver)
            {
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 170));
                Raylib.DrawText("GAME OVER", 350, 230, 55, Color.Red);
                Raylib.DrawText("Press R to restart level", 350, 310, 28, Color.White);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static LevelData LoadLevel(int levelNumber)
    {
        if (levelNumber == 1)
        {
            return new LevelData
            {
                PlayerStart = new Vector2(80, 420),

                Platforms = new Rectangle[]
                {
                    new Rectangle(0, 560, 1000, 40),
                    new Rectangle(160, 470, 160, 20),
                    new Rectangle(390, 400, 170, 20),
                    new Rectangle(650, 330, 180, 20),
                    new Rectangle(230, 270, 130, 20),
                    new Rectangle(520, 210, 160, 20),
                    new Rectangle(780, 150, 140, 20)
                },

                Coins = new Coin[]
                {
                    new Coin(new Vector2(210, 430)),
                    new Coin(new Vector2(450, 360)),
                    new Coin(new Vector2(710, 290)),
                    new Coin(new Vector2(270, 230)),
                    new Coin(new Vector2(580, 170)),
                    new Coin(new Vector2(840, 110))
                },

                Enemies = new Enemy[]
                {
                    new Enemy(new Vector2(400, 535), 320, 600, 130),
                    new Enemy(new Vector2(680, 305), 650, 810, 130)
                },

                Goal = new Rectangle(900, 90, 45, 60)
            };
        }

        if (levelNumber == 2)
        {
            return new LevelData
            {
                PlayerStart = new Vector2(50, 500),

                Platforms = new Rectangle[]
                {
                    new Rectangle(0, 560, 1000, 40),
                    new Rectangle(130, 485, 120, 20),
                    new Rectangle(320, 425, 110, 20),
                    new Rectangle(520, 360, 100, 20),
                    new Rectangle(720, 300, 100, 20),
                    new Rectangle(560, 240, 90, 20),
                    new Rectangle(350, 190, 90, 20),
                    new Rectangle(160, 140, 90, 20),
                    new Rectangle(790, 130, 130, 20)
                },

                Coins = new Coin[]
                {
                    new Coin(new Vector2(170, 445)),
                    new Coin(new Vector2(350, 385)),
                    new Coin(new Vector2(550, 320)),
                    new Coin(new Vector2(750, 260)),
                    new Coin(new Vector2(590, 200)),
                    new Coin(new Vector2(380, 150)),
                    new Coin(new Vector2(190, 100)),
                    new Coin(new Vector2(840, 70))
                },

                Enemies = new Enemy[]
                {
                    new Enemy(new Vector2(300, 535), 260, 480, 160),
                    new Enemy(new Vector2(600, 535), 560, 850, 170),
                    new Enemy(new Vector2(730, 275), 720, 810, 140)
                },

                Goal = new Rectangle(885, 50, 45, 60)
            };
        }

        return new LevelData
        {
            PlayerStart = new Vector2(40, 500),

            Platforms = new Rectangle[]
            {
                new Rectangle(0, 560, 1000, 40),
                new Rectangle(120, 500, 85, 18),
                new Rectangle(260, 445, 80, 18),
                new Rectangle(410, 390, 75, 18),
                new Rectangle(570, 335, 70, 18),
                new Rectangle(760, 285, 70, 18),
                new Rectangle(610, 225, 65, 18),
                new Rectangle(440, 170, 65, 18),
                new Rectangle(250, 115, 65, 18),
                new Rectangle(780, 120, 130, 18)
            },

            Coins = new Coin[]
            {
                new Coin(new Vector2(145, 460)),
                new Coin(new Vector2(285, 405)),
                new Coin(new Vector2(430, 350)),
                new Coin(new Vector2(590, 295)),
                new Coin(new Vector2(780, 245)),
                new Coin(new Vector2(630, 185)),
                new Coin(new Vector2(460, 130)),
                new Coin(new Vector2(270, 75)),
                new Coin(new Vector2(835, 55))
            },

            Enemies = new Enemy[]
            {
                new Enemy(new Vector2(220, 535), 200, 390, 190),
                new Enemy(new Vector2(500, 535), 480, 690, 210),
                new Enemy(new Vector2(760, 535), 730, 930, 220),
                new Enemy(new Vector2(570, 310), 570, 640, 160),
                new Enemy(new Vector2(760, 260), 760, 830, 170)
            },

            Goal = new Rectangle(875, 35, 45, 60)
        };
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

class LevelData
{
    public Vector2 PlayerStart;
    public Rectangle[] Platforms = Array.Empty<Rectangle>();
    public Coin[] Coins = Array.Empty<Coin>();
    public Enemy[] Enemies = Array.Empty<Enemy>();
    public Rectangle Goal;
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
    private float speed;
    private float leftLimit;
    private float rightLimit;
    private int direction = 1;

    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 38, 25);

    public Enemy(Vector2 position, float left, float right, float enemySpeed)
    {
        Position = position;
        leftLimit = left;
        rightLimit = right;
        speed = enemySpeed;
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