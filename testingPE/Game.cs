using System;
using System.Drawing;
using System.Numerics;
using System.Reflection.Emit;
using System.Windows.Forms;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;

namespace PhysicsEngine
{
    public partial class Game : Form
    {
        private readonly PhysicsWorld _world = new PhysicsWorld();
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer
        {
            Interval = 16,
        };
        private const float Dt = 1f / 60f;

        private SceneObject _ballA,
            _ballB,
            _ballC,
            _floor;

        public Game()
        {
            Text = "Physics Engine";
            DoubleBuffered = true;
            ClientSize = new Size(900, 800);

            _world.AddGlobalForce(new Gravity(new Vector2(0f, 0f)));
            _world.AddGlobalForce(new LinearDrag(2.0f));

            _ballA = new SceneObject(
                "Ball A",
                new RigidBody(new Vector2(400, 50), mass: 0.1f) { Width = 0.5f, Height = 0.5f }
            );
            _ballB = new SceneObject(
                "Ball B",
                new RigidBody(new Vector2(400, 100), mass: 0.1f) { Width = 0.5f, Height = 0.5f }
            );
            _ballC = new SceneObject(
                "Ball B",
                new RigidBody(new Vector2(400, 150), mass: 0.1f) { Width = 0.5f, Height = 0.5f }
            );

            _floor = new SceneObject(
                "Ground",
                new RigidBody(new Vector2(200, 200), mass: 0.1f) { Width = 0.5f, Height = 0.5f }
            );

            _world.AddBody(_ballA.Body);
            _world.AddBody(_ballB.Body);
            _world.AddBody(_floor.Body);
            _world.AddBody(_ballC.Body);

            UpdateWorldBounds();
            Resize += (_, __) => UpdateWorldBounds();

            _timer.Tick += (_, __) =>
            {
                _world.Step(Dt);
                Invalidate();
            };
            _timer.Start();
        }

        private void UpdateWorldBounds()
        {
            _world.MinBounds = new Vector2(0, 0);
            _world.MaxBounds = new Vector2(ClientSize.Width, ClientSize.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Color.Black);
            DrawBody(g, _ballA.Body, Brushes.DeepSkyBlue);
            DrawBody(g, _ballB.Body, Brushes.Gold);
            DrawBody(g, _ballC.Body, Brushes.Red);
            // DrawRectBody(g, _floor.Body, Brushes.Green);
            using var f = new Font("Consolas", 10f);
            g.DrawString("Esc to quit", f, Brushes.White, 10, 10);
        }

        private void DrawBody(Graphics g, IRigidBody body, Brush brush)
        {
            if (body is RigidBody rb)
            {
                float r = rb.RenderRadius;
                float x = rb.Position.X - r;
                float y = rb.Position.Y - r;
                g.FillEllipse(brush, x, y, r * 2, r * 2);
            }
        }

        private void DrawRectBody(Graphics g, IRigidBody body, Brush brush)
        {
            if (body is RigidBody rb)
            {
                float width = 30f;
                float height = 30f;
                float x = rb.Position.X - width;
                float y = rb.Position.Y - height;
                g.FillRectangle(brush, x, y, width, height);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            if (_ballA?.Body is IRigidBody body)
            {
                const float moveSpeed = 55f;

                switch (keyData)
                {
                    case Keys.Left:
                        body.Velocity = new Vector2(moveSpeed, body.Velocity.Y);
                        return true;

                    case Keys.Right:
                        body.Velocity = new Vector2(-moveSpeed, body.Velocity.Y);
                        return true;

                    case Keys.Up:
                        body.Velocity = new Vector2(body.Velocity.X, +moveSpeed);
                        return true;

                    case Keys.Down:
                        body.Velocity = new Vector2(body.Velocity.X, -moveSpeed);
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Game_Load(object? sender, EventArgs e) { }
    }
}
