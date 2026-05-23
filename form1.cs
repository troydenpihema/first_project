using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace PlatformerGame
{
    public partial class Form1 : Form
    {
        // PLAYERS
        Rectangle player1 = new Rectangle(100, 300, 50, 60);
        Rectangle player2 = new Rectangle(300, 300, 50, 60);

        // ATTACK HITBOXES
        Rectangle p1AttackBox;
        Rectangle p2AttackBox;

        // HEALTH
        int p1Health = 100;
        int p2Health = 100;

        // MOVEMENT
        int p1SpeedX = 0;
        int p2SpeedX = 0;

        int p1SpeedY = 0;
        int p2SpeedY = 0;

        bool p1Jumping = false;
        bool p2Jumping = false;

        bool p1Attack = false;
        bool p2Attack = false;

        bool facingRight1 = true;
        bool facingRight2 = false;

        // CAMERA
        int cameraX = 0;

        // PLATFORM LIST
        List<Rectangle> platforms = new List<Rectangle>();

        // ENEMIES
        List<Rectangle> enemies = new List<Rectangle>();

        // TIMER
        Timer gameTimer = new Timer();

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.Width = 1200;
            this.Height = 700;
            this.BackColor = Color.Black;

            // PLATFORMS
            platforms.Add(new Rectangle(0, 500, 2000, 50));
            platforms.Add(new Rectangle(300, 400, 200, 20));
            platforms.Add(new Rectangle(650, 300, 200, 20));
            platforms.Add(new Rectangle(1000, 420, 200, 20));

            // ENEMIES
            enemies.Add(new Rectangle(700, 250, 40, 50));
            enemies.Add(new Rectangle(1100, 370, 40, 50));

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += KeyIsDown;
            this.KeyUp += KeyIsUp;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // GRAVITY
            p1SpeedY += 1;
            p2SpeedY += 1;

            // MOVE PLAYERS
            player1.X += p1SpeedX;
            player2.X += p2SpeedX;

            player1.Y += p1SpeedY;
            player2.Y += p2SpeedY;

            // PLATFORM COLLISION
            HandlePlatformCollision(ref player1, ref p1SpeedY, ref p1Jumping);
            HandlePlatformCollision(ref player2, ref p2SpeedY, ref p2Jumping);

            // ATTACK BOXES
            if (facingRight1)
                p1AttackBox = new Rectangle(player1.Right, player1.Y + 10, 40, 20);
            else
                p1AttackBox = new Rectangle(player1.Left - 40, player1.Y + 10, 40, 20);

            if (facingRight2)
                p2AttackBox = new Rectangle(player2.Right, player2.Y + 10, 40, 20);
            else
                p2AttackBox = new Rectangle(player2.Left - 40, player2.Y + 10, 40, 20);

            // PLAYER DAMAGE
            if (p1Attack && p1AttackBox.IntersectsWith(player2))
            {
                p2Health -= 1;
            }

            if (p2Attack && p2AttackBox.IntersectsWith(player1))
            {
                p1Health -= 1;
            }

            // CAMERA FOLLOW
            cameraX = player1.X - 300;

            // GAME OVER
            if (p1Health <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("PLAYER 2 WINS!");
            }

            if (p2Health <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("PLAYER 1 WINS!");
            }

            Invalidate();
        }

        private void HandlePlatformCollision(ref Rectangle player, ref int speedY, ref bool jumping)
        {
            foreach (Rectangle platform in platforms)
            {
                if (player.IntersectsWith(platform) && speedY >= 0)
                {
                    player.Y = platform.Top - player.Height;
                    speedY = 0;
                    jumping = false;
                }
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            // PLAYER 1
            if (e.KeyCode == Keys.A)
            {
                p1SpeedX = -6;
                facingRight1 = false;
            }

            if (e.KeyCode == Keys.D)
            {
                p1SpeedX = 6;
                facingRight1 = true;
            }

            if (e.KeyCode == Keys.Space && !p1Jumping)
            {
                p1SpeedY = -18;
                p1Jumping = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                p1Attack = true;
            }

            // PLAYER 2
            if (e.KeyCode == Keys.Left)
            {
                p2SpeedX = -6;
                facingRight2 = false;
            }

            if (e.KeyCode == Keys.Right)
            {
                p2SpeedX = 6;
                facingRight2 = true;
            }

            if (e.KeyCode == Keys.Up && !p2Jumping)
            {
                p2SpeedY = -18;
                p2Jumping = true;
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                p2Attack = true;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            // PLAYER 1
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.D)
            {
                p1SpeedX = 0;
            }

            if (e.KeyCode == Keys.Enter)
            {
                p1Attack = false;
            }

            // PLAYER 2
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                p2SpeedX = 0;
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                p2Attack = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // CAMERA
            g.TranslateTransform(-cameraX, 0);

            // BACKGROUND
            g.Clear(Color.SkyBlue);

            // PLATFORMS
            foreach (Rectangle platform in platforms)
            {
                g.FillRectangle(Brushes.ForestGreen, platform);
            }

            // ENEMIES
            foreach (Rectangle enemy in enemies)
            {
                g.FillRectangle(Brushes.DarkRed, enemy);
            }

            // PLAYERS
            g.FillRectangle(Brushes.Blue, player1);
            g.FillRectangle(Brushes.Red, player2);

            // ATTACKS
            if (p1Attack)
            {
                g.FillRectangle(Brushes.Yellow, p1AttackBox);
            }

            if (p2Attack)
            {
                g.FillRectangle(Brushes.Orange, p2AttackBox);
            }

            // HEALTH BARS
            g.ResetTransform();

            g.FillRectangle(Brushes.Red, 20, 20, p1Health * 2, 20);
            g.DrawString("P1", Font, Brushes.White, 20, 45);

            g.FillRectangle(Brushes.Blue, 900, 20, p2Health * 2, 20);
            g.DrawString("P2", Font, Brushes.White, 900, 45);
        }
    }
}