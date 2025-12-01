using System;
using System.Drawing;
using System.Windows.Forms;

namespace PhysicsEngine
{
    public partial class Game : Form
    {
        // explicitly use WinForms timer
        private readonly System.Windows.Forms.Timer _timer;

        // World dimensions (in pixels)
        private const int PaddleWidth = 16;
        private const int PaddleHeight = 100;
        private const int BallSize = 16;

        // Game objects (AABB style)
        private RectangleF _leftPaddle;
        private RectangleF _rightPaddle;
        private RectangleF _ball;

        // Ball velocity
        private float _ballVelX;
        private float _ballVelY;

        // Paddle movement
        private bool _leftUp;
        private bool _leftDown;
        private bool _rightUp;
        private bool _rightDown;
        private const float PaddleSpeed = 7f;

        // Scores
        private int _leftScore;
        private int _rightScore;

        public Game()
        {
            InitializeComponent(); // from Designer

            // Make sure these are set (also in designer, but safe to re-set)
            Text = "Pong – WinForms AABB";
            DoubleBuffered = true;
            BackColor = Color.Black;
            KeyPreview = true;

            // Setup timer (60 FPS-ish)
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 16; // ~60FPS
            _timer.Tick += (_, __) => GameLoop();
            _timer.Start();

            // Initialize objects
            ResetGameObjects();
        }

        private void ResetGameObjects()
        {
            // Left paddle
            _leftPaddle = new RectangleF(
                x: 40,
                y: (ClientSize.Height - PaddleHeight) / 2f,
                width: PaddleWidth,
                height: PaddleHeight
            );

            // Right paddle
            _rightPaddle = new RectangleF(
                x: ClientSize.Width - 40 - PaddleWidth,
                y: (ClientSize.Height - PaddleHeight) / 2f,
                width: PaddleWidth,
                height: PaddleHeight
            );

            // Ball in the center
            _ball = new RectangleF(
                x: (ClientSize.Width - BallSize) / 2f,
                y: (ClientSize.Height - BallSize) / 2f,
                width: BallSize,
                height: BallSize
            );

            // Random-ish initial direction
            var rand = new Random();
            _ballVelX = rand.Next(0, 2) == 0 ? 6f : -6f;
            _ballVelY = (float)(rand.NextDouble() * 4f - 2f); // -2..2
        }

        private void GameLoop()
        {
            UpdatePaddles();
            UpdateBall();
            Invalidate(); // triggers OnPaint
        }

        private void UpdatePaddles()
        {
            // Left paddle movement
            if (_leftUp)
                _leftPaddle.Y -= PaddleSpeed;
            if (_leftDown)
                _leftPaddle.Y += PaddleSpeed;

            // Right paddle movement
            if (_rightUp)
                _rightPaddle.Y -= PaddleSpeed;
            if (_rightDown)
                _rightPaddle.Y += PaddleSpeed;

            // Clamp paddles inside window
            _leftPaddle.Y = Math.Max(
                0,
                Math.Min(ClientSize.Height - _leftPaddle.Height, _leftPaddle.Y)
            );
            _rightPaddle.Y = Math.Max(
                0,
                Math.Min(ClientSize.Height - _rightPaddle.Height, _rightPaddle.Y)
            );
        }

        private void UpdateBall()
        {
            // Move ball
            _ball.X += _ballVelX;
            _ball.Y += _ballVelY;

            // Top / bottom collision
            if (_ball.Top <= 0)
            {
                _ball.Y = 0;
                _ballVelY = Math.Abs(_ballVelY); // go down
            }
            else if (_ball.Bottom >= ClientSize.Height)
            {
                _ball.Y = ClientSize.Height - _ball.Height;
                _ballVelY = -Math.Abs(_ballVelY); // go up
            }

            // Paddle collisions (AABB)
            if (Intersects(_ball, _leftPaddle) && _ballVelX < 0)
            {
                // Place ball just outside paddle
                _ball.X = _leftPaddle.Right;
                _ballVelX = Math.Abs(_ballVelX); // bounce right
                AddPaddleSpin(_leftPaddle);
            }
            else if (Intersects(_ball, _rightPaddle) && _ballVelX > 0)
            {
                _ball.X = _rightPaddle.Left - _ball.Width;
                _ballVelX = -Math.Abs(_ballVelX); // bounce left
                AddPaddleSpin(_rightPaddle);
            }

            // Left / right out-of-bounds (score)
            if (_ball.Right < 0)
            {
                _rightScore++;
                ResetGameObjects();
            }
            else if (_ball.Left > ClientSize.Width)
            {
                _leftScore++;
                ResetGameObjects();
            }
        }

        /// <summary>
        /// Simple AABB intersection for RectangleF
        /// </summary>
        private static bool Intersects(RectangleF a, RectangleF b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }

        /// <summary>
        /// Add some vertical "spin" based on where the ball hits the paddle.
        /// </summary>
        private void AddPaddleSpin(RectangleF paddle)
        {
            float paddleCenter = paddle.Top + paddle.Height / 2f;
            float ballCenter = _ball.Top + _ball.Height / 2f;
            float offset = ballCenter - paddleCenter; // negative = top, positive = bottom

            // Normalize offset to [-1, 1] and scale
            float normalized = offset / (paddle.Height / 2f);
            _ballVelY = normalized * 6f; // tweak as you like
        }

        // ===== Input handling =====

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Escape to quit
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Left paddle: W/S
            if (e.KeyCode == Keys.W)
                _leftUp = true;
            if (e.KeyCode == Keys.S)
                _leftDown = true;

            // Right paddle: Up/Down
            if (e.KeyCode == Keys.Up)
                _rightUp = true;
            if (e.KeyCode == Keys.Down)
                _rightDown = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.W)
                _leftUp = false;
            if (e.KeyCode == Keys.S)
                _leftDown = false;

            if (e.KeyCode == Keys.Up)
                _rightUp = false;
            if (e.KeyCode == Keys.Down)
                _rightDown = false;
        }

        // ===== Drawing =====

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            g.Clear(Color.Black);

            using var paddleBrush = new SolidBrush(Color.White);
            using var ballBrush = new SolidBrush(Color.White);
            using var textBrush = new SolidBrush(Color.White);
            using var midPen = new Pen(Color.FromArgb(60, 60, 60), 2f);

            // Center line
            for (int y = 0; y < ClientSize.Height; y += 20)
            {
                g.DrawLine(midPen, ClientSize.Width / 2, y, ClientSize.Width / 2, y + 10);
            }

            // Draw paddles
            g.FillRectangle(paddleBrush, _leftPaddle);
            g.FillRectangle(paddleBrush, _rightPaddle);

            // Draw ball
            g.FillRectangle(ballBrush, _ball);
        }
    }
}
