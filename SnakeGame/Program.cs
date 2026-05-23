using System;
using System.Numerics;
using Raylib_cs;

const int screenWidth = 800;
const int screenHeight = 600;
const int cellSize = 20;
const int gridWidth = screenWidth / cellSize;
const int gridHeight = screenHeight / cellSize;

Random random = new Random();

GameState gameState = GameState.Menu;

List<SnakeSegment> snake = new List<SnakeSegment>();
snake.Add(new SnakeSegment(10, 10));

Vector2 food = new Vector2(random.Next(gridWidth), random.Next(gridHeight));

int directionX = 1;
int directionY = 0;

int score = 0;

bool darkMode = true;

Rectangle startButton = new Rectangle(300, 200, 200, 50);
Rectangle optionsButton = new Rectangle(300, 280, 200, 50);
Rectangle exitButton = new Rectangle(300, 360, 200, 50);
Rectangle backButton = new Rectangle(300, 450, 200, 50);

Raylib.InitWindow(screenWidth, screenHeight, "Snake Game - C# Raylib");
Raylib.SetTargetFPS(10);

while (!Raylib.WindowShouldClose())
{
    Color backgroundColor = darkMode ? Color.Black : Color.RayWhite;
    Color textColor = darkMode ? Color.RayWhite : Color.Black;

    Raylib.BeginDrawing();
    Raylib.ClearBackground(backgroundColor);

    switch (gameState)
    {
        case GameState.Menu:
        {
            Raylib.DrawText("SNAKE GAME", 240, 100, 50, Color.Green);

            Raylib.DrawRectangleRec(startButton, Color.DarkGreen);
            Raylib.DrawText("START GAME", 325, 215, 20, Color.White);

            Raylib.DrawRectangleRec(optionsButton, Color.DarkBlue);
            Raylib.DrawText("OPTIONS", 350, 295, 20, Color.White);

            Raylib.DrawRectangleRec(exitButton, Color.Maroon);
            Raylib.DrawText("EXIT", 375, 375, 20, Color.White);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mouse = Raylib.GetMousePosition();

                if (Raylib.CheckCollisionPointRec(mouse, startButton))
                {
                    snake.Clear();
                    snake.Add(new SnakeSegment(10, 10));

                    directionX = 1;
                    directionY = 0;

                    food = new Vector2(random.Next(gridWidth), random.Next(gridHeight));
                    score = 0;

                    gameState = GameState.Playing;
                }

                if (Raylib.CheckCollisionPointRec(mouse, optionsButton))
                {
                    gameState = GameState.Options;
                }

                if (Raylib.CheckCollisionPointRec(mouse, exitButton))
                {
                    Raylib.CloseWindow();
                    return;
                }
            }
            break;
        }

        case GameState.Options:
        {
            Raylib.DrawText("OPTIONS", 300, 100, 50, textColor);
            Raylib.DrawText("Press D to Toggle Dark Mode", 180, 250, 25, textColor);

            if (Raylib.IsKeyPressed(KeyboardKey.D))
            {
                darkMode = !darkMode;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("BACK", 370, 465, 20, Color.White);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mouse = Raylib.GetMousePosition();

                if (Raylib.CheckCollisionPointRec(mouse, backButton))
                {
                    gameState = GameState.Menu;
                }
            }
            break;
        }

        case GameState.Playing:
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Up) && directionY == 0)
            {
                directionX = 0;
                directionY = -1;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Down) && directionY == 0)
            {
                directionX = 0;
                directionY = 1;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Left) && directionX == 0)
            {
                directionX = -1;
                directionY = 0;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Right) && directionX == 0)
            {
                directionX = 1;
                directionY = 0;
            }

            SnakeSegment newHead = new SnakeSegment(
                snake[0].X + directionX,
                snake[0].Y + directionY
            );

            snake.Insert(0, newHead);

            if ((int)food.X == newHead.X && (int)food.Y == newHead.Y)
            {
                score++;
                food = new Vector2(random.Next(gridWidth), random.Next(gridHeight));
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            if (newHead.X < 0 || newHead.X >= gridWidth ||
                newHead.Y < 0 || newHead.Y >= gridHeight)
            {
                gameState = GameState.GameOver;
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (snake[i].X == newHead.X && snake[i].Y == newHead.Y)
                {
                    gameState = GameState.GameOver;
                }
            }

            foreach (var segment in snake)
            {
                Raylib.DrawRectangle(
                    segment.X * cellSize,
                    segment.Y * cellSize,
                    cellSize,
                    cellSize,
                    Color.Green
                );
            }

            Raylib.DrawRectangle(
                (int)food.X * cellSize,
                (int)food.Y * cellSize,
                cellSize,
                cellSize,
                Color.Red
            );

            Raylib.DrawText($"Score: {score}", 10, 10, 25, textColor);

            break;
        }

        case GameState.GameOver:
        {
            Raylib.DrawText("GAME OVER", 240, 200, 50, Color.Red);
            Raylib.DrawText($"Final Score: {score}", 290, 280, 30, textColor);
            Raylib.DrawText("Press ENTER To Return To Menu", 170, 380, 25, textColor);

            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                gameState = GameState.Menu;
            }

            break;
        }
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

public enum GameState
{
    Menu,
    Options,
    Playing,
    GameOver
}

public class SnakeSegment
{
    public int X;
    public int Y;

    public SnakeSegment(int x, int y)
    {
        X = x;
        Y = y;
    }
}

