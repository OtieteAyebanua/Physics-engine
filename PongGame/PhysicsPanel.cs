using System.Numerics;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;

namespace PongGame
{
    // A simple avoider game: squares fall from the sky, player avoids them.
    public class GamePanel : Control
    {
        private readonly PhysicsWorld _world;
        private readonly List<(SceneObject obj, Color color)> _objects = new();
        private readonly float _timestep = 1f / 60f;

        private readonly SceneObject _player;
        private readonly int _playerWidth = 80;
        private readonly int _playerHeight = 20;
        private readonly int _worldWidth = 800;
        private readonly int _worldHeight = 600;

        private readonly System.Windows.Forms.Timer _timer;
        private float _spawnAccumulator = 0f;
        private readonly float _spawnInterval = 0.9f; // seconds
        private readonly Random _rnd = new();

        private int _lives = 3;
        private bool _gameOver = false;

        public GamePanel()
        {
            DoubleBuffered = true;
            BackColor = Color.Black;
            ForeColor = Color.White;
            Font = new Font("Arial", 14);

            _world = new PhysicsWorld
            {
                MinBounds = new Vector2(0, 0),
                MaxBounds = new Vector2(_worldWidth, _worldHeight),
            };

            _world.AddGlobalForce(new Gravity(new Vector2(0, 600f)));

            // Create player (static)
            var playerBody = new RigidBody(new Vector2(_worldWidth / 2f, _worldHeight - 40f), mass: float.PositiveInfinity)
            {
                Width = _playerWidth,
                Height = _playerHeight,
                allowCollision = true,
            };
            _player = new SceneObject("Player", playerBody);
            _objects.Add((_player, Color.White));
            _world.AddBody(playerBody);

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            KeyDown += GamePanel_KeyDown;
            KeyUp += GamePanel_KeyUp;
            TabStop = true;
            Focus();
        }

        private readonly HashSet<Keys> _keysDown = new();

        private void GamePanel_KeyDown(object? sender, KeyEventArgs e)
        {
            _keysDown.Add(e.KeyCode);
            if (_gameOver && e.KeyCode == Keys.R)
                Restart();
        }

        private void GamePanel_KeyUp(object? sender, KeyEventArgs e)
        {
            _keysDown.Remove(e.KeyCode);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Fixed-step loop: advance physics once per tick with a fixed dt
            UpdateGame(_timestep);
            _world.Step(_timestep);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw all objects
            foreach (var (obj, color) in _objects)
            {
                var pos = obj.Body.Position;
                var width = (int)obj.Body.Width;
                var height = (int)obj.Body.Height;
                using var brush = new SolidBrush(color);
                e.Graphics.FillRectangle(brush, pos.X - width / 2f, pos.Y - height / 2f, width, height);
            }

            // HUD: Lives
            e.Graphics.DrawString($"Lives: {_lives}", Font, Brushes.White, 10, 10);

            if (_gameOver)
            {
                var msg = "Game Over - Press R to Restart";
                var sz = e.Graphics.MeasureString(msg, Font);
                e.Graphics.DrawString(msg, Font, Brushes.Red, (_worldWidth - sz.Width) / 2f, (_worldHeight - sz.Height) / 2f);
            }
        }

        private void UpdateGame(float dt)
        {
            if (_gameOver)
                return;

            // Player movement
            var playerPos = _player.Body.Position;
            float speed = 400f;
            if (_keysDown.Contains(Keys.Left) || _keysDown.Contains(Keys.A))
            {
                playerPos.X = Math.Max(playerPos.X - speed * dt, _player.Body.Width / 2f);
            }
            if (_keysDown.Contains(Keys.Right) || _keysDown.Contains(Keys.D))
            {
                playerPos.X = Math.Min(playerPos.X + speed * dt, _worldWidth - _player.Body.Width / 2f);
            }
            _player.Body.Position = playerPos;

            // Spawning
            _spawnAccumulator += dt;
            if (_spawnAccumulator >= _spawnInterval)
            {
                _spawnAccumulator = 0f;
                SpawnFallingSquare();
            }
        }
        private void SpawnFallingSquare()
        {
            int size = _rnd.Next(24, 56);
            float x = _rnd.Next(size / 2, _worldWidth - size / 2);
            var body = new RigidBody(new Vector2(x, -size), mass: 1f)
            {
                Width = size,
                Height = size,
                Restitution = 0.1f,
                allowCollision = true,
            };
            var obj = new SceneObject("Falling", body);
            Color color = Color.FromArgb(200, _rnd.Next(40, 256), _rnd.Next(40, 256), _rnd.Next(40, 256));
            _objects.Add((obj, color));
            _world.AddBody(body);
        }

        private void Restart()
        {
            // clear falling objects
            foreach (var (obj, _) in _objects.Where(t => t.obj != _player).ToList())
            {
                _world.Bodies.Remove(obj.Body);
                _objects.Remove((obj, _objects.First(x => x.obj == obj).color));
            }
            _lives = 3;
            _gameOver = false;
        }
    }
}
