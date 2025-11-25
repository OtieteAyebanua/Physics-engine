using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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
        private readonly List<SceneObject> _balls = new List<SceneObject>();
        private SceneObject _floor;
        private SceneObject _selectedBall;
        private readonly Gravity _gravityForce;
        private readonly LinearDrag _dragForce;
        private Vector2 _gravityValue;
        private float _dragValue;
        private bool _isDragging;
        private SceneObject _draggedBall;
        private Vector2 _dragOffset;
        private readonly Panel _sidebar;
        private readonly Panel _scrollPanel;
        private readonly Label _lblTitle;
        private readonly GroupBox _sceneGroup;
        private readonly Button _btnAddBall;
        private readonly Button _btnPause;
        private readonly Label _lblBallCount;
        private readonly Label _lblSelectedBall;
        private readonly ListBox _listBalls;
        private readonly GroupBox _controlsGroup;
        private readonly Button _btnNudgeLeft;
        private readonly Button _btnNudgeRight;
        private readonly Button _btnNudgeUp;
        private readonly Button _btnNudgeDown;
        private readonly Button _btnStop;
        private readonly GroupBox _worldGroup;
        private readonly CheckBox _chkGravityEnabled;
        private readonly NumericUpDown _numGravityX;
        private readonly NumericUpDown _numGravityY;
        private readonly CheckBox _chkDragEnabled;
        private readonly NumericUpDown _numDragCoeff;
        private readonly GroupBox _propsGroup;
        private readonly NumericUpDown _numPosX;
        private readonly NumericUpDown _numPosY;
        private readonly NumericUpDown _numVelX;
        private readonly NumericUpDown _numVelY;
        private readonly NumericUpDown _numRadius;
        private readonly NumericUpDown _numWidth;
        private readonly NumericUpDown _numHeight;
        private readonly NumericUpDown _numRestitution;
        private readonly NumericUpDown _numLinearDamping;
        private readonly NumericUpDown _numMass;
        private readonly NumericUpDown _numInvMass;
        private readonly CheckBox _chkAllowCollision;
        private readonly Random _rng = new Random();
        private bool _updatingUI;
        private bool _isPaused;
        public Game()
        {
            Text = "Physics Engine";
            DoubleBuffered = true;
            ClientSize = new Size(1200, 800);
            BackColor = Color.Black;

            // ===== SIDEBAR ROOT =====
            _sidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 340,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(0),
            };
            Controls.Add(_sidebar);

            // ===== SCROLLABLE CONTENT PANEL =====
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(8),
            };
            _sidebar.Controls.Add(_scrollPanel);
            _lblTitle = new Label
            {
                Text = "Physics Editor",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _scrollPanel.Controls.Add(_lblTitle);
            _sceneGroup = new GroupBox
            {
                Text = "Scene",
                Dock = DockStyle.Top,
                Height = 220,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Padding = new Padding(8, 24, 8, 8),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            _scrollPanel.Controls.Add(_sceneGroup);

            _listBalls = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
            };
            _listBalls.SelectedIndexChanged += OnBallListSelectionChanged;
            _sceneGroup.Controls.Add(_listBalls);

            var sceneInfoPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            _sceneGroup.Controls.Add(sceneInfoPanel);

            _lblBallCount = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            sceneInfoPanel.Controls.Add(_lblBallCount);

            _lblSelectedBall = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            sceneInfoPanel.Controls.Add(_lblSelectedBall);

            var sceneButtonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(0, 6, 0, 0),
            };
            _sceneGroup.Controls.Add(sceneButtonPanel);

            _btnAddBall = new Button
            {
                Text = "Add Ball",
                Dock = DockStyle.Left,
                Width = 140,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnAddBall.FlatAppearance.BorderSize = 0;
            _btnAddBall.Click += (_, __) => AddBall();
            sceneButtonPanel.Controls.Add(_btnAddBall);

            _btnPause = new Button
            {
                Text = "Pause",
                Dock = DockStyle.Right,
                Width = 140,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnPause.FlatAppearance.BorderSize = 0;
            _btnPause.Click += (_, __) => TogglePause();
            sceneButtonPanel.Controls.Add(_btnPause);

            // ===== BALL CONTROLS GROUP =====
            _controlsGroup = new GroupBox
            {
                Text = "Ball Controls",
                Dock = DockStyle.Top,
                Height = 90,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Padding = new Padding(8, 24, 8, 8),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            _scrollPanel.Controls.Add(_controlsGroup);

            // simple D-pad layout
            _btnNudgeUp = new Button
            {
                Text = "↑",
                Width = 40,
                Height = 28,
                Left = 70,
                Top = 26,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnNudgeUp.Click += (_, __) => NudgeSelectedBall(0, -150f);
            _controlsGroup.Controls.Add(_btnNudgeUp);

            _btnNudgeLeft = new Button
            {
                Text = "←",
                Width = 40,
                Height = 28,
                Left = 30,
                Top = 54,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnNudgeLeft.Click += (_, __) => NudgeSelectedBall(-150f, 0);
            _controlsGroup.Controls.Add(_btnNudgeLeft);

            _btnNudgeRight = new Button
            {
                Text = "→",
                Width = 40,
                Height = 28,
                Left = 110,
                Top = 54,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnNudgeRight.Click += (_, __) => NudgeSelectedBall(150f, 0);
            _controlsGroup.Controls.Add(_btnNudgeRight);

            _btnNudgeDown = new Button
            {
                Text = "↓",
                Width = 40,
                Height = 28,
                Left = 70,
                Top = 54,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnNudgeDown.Click += (_, __) => NudgeSelectedBall(0, 150f);
            _controlsGroup.Controls.Add(_btnNudgeDown);

            _btnStop = new Button
            {
                Text = "Stop",
                Width = 60,
                Height = 28,
                Left = 180,
                Top = 40,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnStop.Click += (_, __) => NudgeSelectedBallStop();
            _controlsGroup.Controls.Add(_btnStop);

            // ===== WORLD GROUP (Gravity / Drag) =====
            _worldGroup = new GroupBox
            {
                Text = "World",
                Dock = DockStyle.Top,
                Height = 160,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Padding = new Padding(8, 24, 8, 8),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            _scrollPanel.Controls.Add(_worldGroup);

            int worldTop = 24;
            int worldStep = 26;

            // Gravity header
            var lblWorldGravity = new Label
            {
                Text = "Gravity",
                Left = 6,
                Top = worldTop,
                Width = _worldGroup.Width - 12,
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            };
            _worldGroup.Controls.Add(lblWorldGravity);
            worldTop += 18;

            _chkGravityEnabled = new CheckBox
            {
                Text = "Enabled",
                Left = 10,
                Top = worldTop,
                Width = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Font = new Font("Segoe UI", 8.5f),
            };
            _worldGroup.Controls.Add(_chkGravityEnabled);

            _numGravityX = new NumericUpDown
            {
                Left = 95,
                Top = worldTop - 2,
                Width = 80,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f),
            };
            _worldGroup.Controls.Add(_numGravityX);

            _numGravityY = new NumericUpDown
            {
                Left = 185,
                Top = worldTop - 2,
                Width = 80,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f),
            };
            _worldGroup.Controls.Add(_numGravityY);

            worldTop += worldStep + 4;

            // Drag header
            var lblWorldDrag = new Label
            {
                Text = "Linear Drag",
                Left = 6,
                Top = worldTop,
                Width = _worldGroup.Width - 12,
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            };
            _worldGroup.Controls.Add(lblWorldDrag);
            worldTop += 18;

            _chkDragEnabled = new CheckBox
            {
                Text = "Enabled",
                Left = 10,
                Top = worldTop,
                Width = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Font = new Font("Segoe UI", 8.5f),
            };
            _worldGroup.Controls.Add(_chkDragEnabled);

            _numDragCoeff = new NumericUpDown
            {
                Left = 95,
                Top = worldTop - 2,
                Width = 80,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f),
            };
            _worldGroup.Controls.Add(_numDragCoeff);

            // ranges for world controls
            SetupNumericGravity(_numGravityX);
            SetupNumericGravity(_numGravityY);
            _numDragCoeff.Minimum = 0m;
            _numDragCoeff.Maximum = 100m;
            _numDragCoeff.DecimalPlaces = 2;
            _numDragCoeff.Increment = 0.1m;

            _chkGravityEnabled.CheckedChanged += OnWorldPropertyChanged;
            _chkDragEnabled.CheckedChanged += OnWorldPropertyChanged;
            _numGravityX.ValueChanged += OnWorldPropertyChanged;
            _numGravityY.ValueChanged += OnWorldPropertyChanged;
            _numDragCoeff.ValueChanged += OnWorldPropertyChanged;

            // ===== INSPECTOR GROUP =====
            _propsGroup = new GroupBox
            {
                Text = "Inspector",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Padding = new Padding(8, 24, 8, 8),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Height = 430,
            };
            _scrollPanel.Controls.Add(_propsGroup);

            int top = 24;
            int step = 26;

            CreateSectionHeader(_propsGroup, "Transform", ref top);
            _numPosX = CreateLabeledNumeric(_propsGroup, "Pos X:", ref top, step);
            _numPosY = CreateLabeledNumeric(_propsGroup, "Pos Y:", ref top, step);
            _numRadius = CreateLabeledNumeric(_propsGroup, "Radius:", ref top, step);
            _numWidth = CreateLabeledNumeric(_propsGroup, "Width:", ref top, step);
            _numHeight = CreateLabeledNumeric(_propsGroup, "Height:", ref top, step);

            CreateSectionHeader(_propsGroup, "Motion", ref top);
            _numVelX = CreateLabeledNumeric(_propsGroup, "Vel X:", ref top, step);
            _numVelY = CreateLabeledNumeric(_propsGroup, "Vel Y:", ref top, step);
            _numLinearDamping = CreateLabeledNumeric(_propsGroup, "Lin Damp:", ref top, step);

            CreateSectionHeader(_propsGroup, "Physical", ref top);
            _numRestitution = CreateLabeledNumeric(_propsGroup, "Restitution:", ref top, step);
            _numMass = CreateLabeledNumeric(_propsGroup, "Mass:", ref top, step);
            _numInvMass = CreateLabeledNumeric(_propsGroup, "Inv Mass:", ref top, step);

            _chkAllowCollision = new CheckBox
            {
                Text = "Allow Collision",
                Left = 10,
                Top = top + 4,
                Width = 150,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Font = new Font("Segoe UI", 8.5f),
            };
            _propsGroup.Controls.Add(_chkAllowCollision);

            // numeric ranges for inspector
            SetupNumericPosVel(_numPosX);
            SetupNumericPosVel(_numPosY);
            SetupNumericPosVel(_numVelX);
            SetupNumericPosVel(_numVelY);

            SetupNumericPositive(_numRadius, 2m);
            SetupNumericPositive(_numWidth, 0.1m);
            SetupNumericPositive(_numHeight, 0.1m);

            _numRestitution.Minimum = 0m;
            _numRestitution.Maximum = 1m;
            _numRestitution.DecimalPlaces = 2;
            _numRestitution.Increment = 0.05m;

            _numLinearDamping.Minimum = 0m;
            _numLinearDamping.Maximum = 100m;
            _numLinearDamping.DecimalPlaces = 2;
            _numLinearDamping.Increment = 0.1m;

            SetupNumericPositive(_numMass, 0.01m);
            SetupNumericPositive(_numInvMass, 0m);

            // inspector events
            _numPosX.ValueChanged += OnPropertyChanged;
            _numPosY.ValueChanged += OnPropertyChanged;
            _numVelX.ValueChanged += OnPropertyChanged;
            _numVelY.ValueChanged += OnPropertyChanged;
            _numRadius.ValueChanged += OnPropertyChanged;
            _numWidth.ValueChanged += OnPropertyChanged;
            _numHeight.ValueChanged += OnPropertyChanged;
            _numRestitution.ValueChanged += OnPropertyChanged;
            _numLinearDamping.ValueChanged += OnPropertyChanged;
            _numMass.ValueChanged += OnPropertyChanged;
            _numInvMass.ValueChanged += OnPropertyChanged;
            _chkAllowCollision.CheckedChanged += OnCollisionToggled;

            SetPropertyControlsEnabled(false);

            // ===== PHYSICS WORLD (forces) =====
            _gravityForce = new Gravity(new Vector2(0f, 0f));
            _dragForce = new LinearDrag(2.0f);

            _world.AddGlobalForce(_gravityForce);
            _world.AddGlobalForce(_dragForce);

            // cache and sync UI for world forces
            _gravityValue = _gravityForce.G;
            _dragValue = _dragForce.Coefficient;

            _updatingUI = true;
            _chkGravityEnabled.Checked = true;
            _numGravityX.Value = (decimal)_gravityValue.X;
            _numGravityY.Value = (decimal)_gravityValue.Y;

            _chkDragEnabled.Checked = true;
            _numDragCoeff.Value = (decimal)_dragValue;
            _updatingUI = false;

            // ===== BODIES =====
            AddBallAt(new Vector2(300, 150));
            AddBallAt(new Vector2(450, 200));
            AddBallAt(new Vector2(600, 250));

            _floor = new SceneObject(
                "Ground",
                new RigidBody(new Vector2(500, 740), mass: 0.1f) { Width = 20f, Height = 1f }
            );
            _world.AddBody(_floor.Body);

            UpdateWorldBounds();
            Resize += (_, __) => UpdateWorldBounds();

            _timer.Tick += (_, __) =>
            {
                if (!_isPaused)
                    _world.Step(Dt);
                Invalidate();
            };
            _timer.Start();
        }

        // ===== UI helpers =====
        private void CreateSectionHeader(Control parent, string text, ref int top)
        {
            var lbl = new Label
            {
                Text = text,
                Left = 6,
                Top = top,
                Width = parent.Width - 12,
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            };
            parent.Controls.Add(lbl);
            top += 18;
        }

        private NumericUpDown CreateLabeledNumeric(
            Control parent,
            string labelText,
            ref int top,
            int step
        )
        {
            var label = new Label
            {
                Text = labelText,
                Left = 10,
                Top = top + 4,
                Width = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Font = new Font("Segoe UI", 8.5f),
            };
            parent.Controls.Add(label);

            var num = new NumericUpDown
            {
                Left = 95,
                Top = top,
                Width = 150,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f),
            };
            parent.Controls.Add(num);

            top += step;
            return num;
        }

        private void SetupNumericPosVel(NumericUpDown num)
        {
            num.Minimum = -5000m;
            num.Maximum = 5000m;
            num.DecimalPlaces = 1;
            num.Increment = 5m;
        }

        private void SetupNumericGravity(NumericUpDown num)
        {
            num.Minimum = -2000m;
            num.Maximum = 2000m;
            num.DecimalPlaces = 1;
            num.Increment = 10m;
        }

        private void SetupNumericPositive(NumericUpDown num, decimal min)
        {
            num.Minimum = min;
            num.Maximum = 5000m;
            num.DecimalPlaces = 2;
            num.Increment = 0.1m;
        }

        private void SetPropertyControlsEnabled(bool enabled)
        {
            _numPosX.Enabled = enabled;
            _numPosY.Enabled = enabled;
            _numVelX.Enabled = enabled;
            _numVelY.Enabled = enabled;
            _numRadius.Enabled = enabled;
            _numWidth.Enabled = enabled;
            _numHeight.Enabled = enabled;
            _numRestitution.Enabled = enabled;
            _numLinearDamping.Enabled = enabled;
            _numMass.Enabled = enabled;
            _numInvMass.Enabled = enabled;
            _chkAllowCollision.Enabled = enabled;
        }

        private decimal ClampToRange(NumericUpDown num, decimal value)
        {
            if (value < num.Minimum)
                return num.Minimum;
            if (value > num.Maximum)
                return num.Maximum;
            return value;
        }

        // ===== world / bodies =====
        private void UpdateWorldBounds()
        {
            int sidebarWidth = _sidebar?.Width ?? 0;
            _world.MinBounds = new Vector2(0, 0);
            _world.MaxBounds = new Vector2(ClientSize.Width - sidebarWidth, ClientSize.Height);
        }

        private void AddBallAt(Vector2 position)
        {
            var body = new RigidBody(position, mass: 1.0f)
            {
                Width = 1.0f,
                Height = 1.0f,
                RenderRadius = 15f,
            };

            var ball = new SceneObject($"Ball {_balls.Count + 1}", body);
            _balls.Add(ball);
            _world.AddBody(body);

            _listBalls.Items.Add(ball);

            if (_selectedBall == null)
                _selectedBall = ball;

            UpdateBallCountLabel();
            UpdateSelectedBallLabel();
            LoadSelectedBallIntoUI();
            UpdateBallListSelection();
        }

        private void AddBall()
        {
            float margin = 60f;
            int sidebarWidth = _sidebar.Width;
            float maxX = ClientSize.Width - sidebarWidth - margin;
            float maxY = ClientSize.Height - margin;

            if (maxX <= margin)
                maxX = margin + 1;
            if (maxY <= margin)
                maxY = margin + 1;

            float x = (float)(_rng.NextDouble() * (maxX - margin) + margin);
            float y = (float)(_rng.NextDouble() * (maxY - margin) + margin);

            AddBallAt(new Vector2(x, y));
        }

        private void UpdateBallCountLabel() => _lblBallCount.Text = $"Balls: {_balls.Count}";

        private void UpdateSelectedBallLabel()
        {
            string status = _isPaused ? "Paused" : "Running";
            _lblSelectedBall.Text =
                _selectedBall != null
                    ? $"Selected: {_selectedBall.Name}   |   {status}"
                    : $"Selected: none   |   {status}";
        }

        private void UpdateBallListSelection()
        {
            if (_selectedBall == null)
            {
                _listBalls.ClearSelected();
                return;
            }

            int index = _balls.IndexOf(_selectedBall);
            if (index >= 0 && index < _listBalls.Items.Count)
                _listBalls.SelectedIndex = index;
        }

        private void LoadSelectedBallIntoUI()
        {
            _updatingUI = true;

            if (_selectedBall != null && _selectedBall.Body is IRigidBody rb)
            {
                SetPropertyControlsEnabled(true);

                _numPosX.Value = ClampToRange(_numPosX, (decimal)rb.Position.X);
                _numPosY.Value = ClampToRange(_numPosY, (decimal)rb.Position.Y);
                _numVelX.Value = ClampToRange(_numVelX, (decimal)rb.Velocity.X);
                _numVelY.Value = ClampToRange(_numVelY, (decimal)rb.Velocity.Y);
                _numRadius.Value = ClampToRange(_numRadius, (decimal)rb.RenderRadius);
                _numWidth.Value = ClampToRange(_numWidth, (decimal)rb.Width);
                _numHeight.Value = ClampToRange(_numHeight, (decimal)rb.Height);
                _numRestitution.Value = ClampToRange(_numRestitution, (decimal)rb.Restitution);
                _numLinearDamping.Value = ClampToRange(
                    _numLinearDamping,
                    (decimal)rb.LinearDamping
                );
                _numMass.Value = ClampToRange(_numMass, (decimal)rb.Mass);
                _numInvMass.Value = ClampToRange(_numInvMass, (decimal)rb.InverseMass);
                _chkAllowCollision.Checked = rb.allowCollision;
            }
            else
            {
                SetPropertyControlsEnabled(false);
            }

            _updatingUI = false;
        }

        // ===== world settings apply =====
        private void ApplyWorldSettings()
        {
            _gravityValue = new Vector2((float)_numGravityX.Value, (float)_numGravityY.Value);
            _dragValue = (float)_numDragCoeff.Value;

            if (_chkGravityEnabled.Checked)
                _gravityForce.G = _gravityValue;
            else
                _gravityForce.G = Vector2.Zero;

            if (_chkDragEnabled.Checked)
                _dragForce.Coefficient = _dragValue;
            else
                _dragForce.Coefficient = 0f;
        }

        private void OnWorldPropertyChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;

            ApplyWorldSettings();
        }

        // ===== ball controls helpers =====
        private void NudgeSelectedBall(float vx, float vy)
        {
            if (_selectedBall?.Body is IRigidBody body)
            {
                body.Velocity += new Vector2(vx, vy);
                _updatingUI = true;
                _numVelX.Value = ClampToRange(_numVelX, (decimal)body.Velocity.X);
                _numVelY.Value = ClampToRange(_numVelY, (decimal)body.Velocity.Y);
                _updatingUI = false;
            }
        }

        private void NudgeSelectedBallStop()
        {
            if (_selectedBall?.Body is IRigidBody body)
            {
                body.Velocity = Vector2.Zero;
                _updatingUI = true;
                _numVelX.Value = 0;
                _numVelY.Value = 0;
                _updatingUI = false;
            }
        }

        // ===== drawing / input =====
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            g.Clear(Color.Black);

            foreach (var ball in _balls)
            {
                if (ball == _selectedBall)
                    DrawBody(g, ball.Body, Brushes.OrangeRed);
                else
                    DrawBody(g, ball.Body, Brushes.DeepSkyBlue);
            }

            using var f = new Font("Consolas", 10f);
            g.DrawString("Esc to quit", f, Brushes.White, 10, 10);
            g.DrawString(
                "Click & drag a ball, scroll to resize. Arrow keys or controls move it.",
                f,
                Brushes.Gray,
                10,
                28
            );
        }

        private void DrawBody(Graphics g, IRigidBody body, Brush brush)
        {
            float r = body.RenderRadius;
            float x = body.Position.X - r;
            float y = body.Position.Y - r;
            g.FillEllipse(brush, x, y, r * 2, r * 2);
        }

        // --- mouse: select + drag ---
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.X >= ClientSize.Width - _sidebar.Width)
                return;

            Vector2 click = new Vector2(e.X, e.Y);
            SceneObject? clickedBall = null;

            for (int i = _balls.Count - 1; i >= 0; i--)
            {
                var ball = _balls[i];
                if (ball.Body is IRigidBody rb)
                {
                    float r = rb.RenderRadius;
                    Vector2 center = rb.Position;
                    float dx = click.X - center.X;
                    float dy = click.Y - center.Y;
                    if (dx * dx + dy * dy <= r * r)
                    {
                        clickedBall = ball;
                        break;
                    }
                }
            }

            _selectedBall = clickedBall;
            UpdateSelectedBallLabel();
            LoadSelectedBallIntoUI();
            UpdateBallListSelection();
            Invalidate();

            if (
                clickedBall != null
                && clickedBall.Body is IRigidBody dragRb
                && e.Button == MouseButtons.Left
            )
            {
                _isDragging = true;
                _draggedBall = clickedBall;
                _dragOffset = dragRb.Position - click;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging && _draggedBall?.Body is IRigidBody rb)
            {
                var newPos = new Vector2(e.X, e.Y) + _dragOffset;
                rb.Position = newPos;
                rb.Velocity = Vector2.Zero; // don't launch after drag

                if (_draggedBall == _selectedBall)
                {
                    _updatingUI = true;
                    _numPosX.Value = ClampToRange(_numPosX, (decimal)newPos.X);
                    _numPosY.Value = ClampToRange(_numPosY, (decimal)newPos.Y);
                    _updatingUI = false;
                }

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            _draggedBall = null;
        }

        // mouse wheel = resize selected ball (desktop-friendly "pinch")
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_selectedBall?.Body is IRigidBody rb)
            {
                float delta = e.Delta > 0 ? 1.5f : -1.5f;
                float newRadius = Math.Max(2f, rb.RenderRadius + delta);
                rb.RenderRadius = newRadius;

                _updatingUI = true;
                _numRadius.Value = ClampToRange(_numRadius, (decimal)newRadius);
                _updatingUI = false;

                Invalidate();
            }
        }

        private void OnBallListSelectionChanged(object? sender, EventArgs e)
        {
            int index = _listBalls.SelectedIndex;
            if (index < 0 || index >= _balls.Count)
                return;

            _selectedBall = _balls[index];
            UpdateSelectedBallLabel();
            LoadSelectedBallIntoUI();
            Invalidate();
        }

        private void OnPropertyChanged(object? sender, EventArgs e)
        {
            if (_updatingUI || _selectedBall?.Body is not IRigidBody rb)
                return;

            rb.Position = new Vector2((float)_numPosX.Value, (float)_numPosY.Value);
            rb.Velocity = new Vector2((float)_numVelX.Value, (float)_numVelY.Value);
            rb.RenderRadius = (float)_numRadius.Value;
            rb.Width = (float)_numWidth.Value;
            rb.Height = (float)_numHeight.Value;
            rb.Restitution = (float)_numRestitution.Value;
            rb.LinearDamping = (float)_numLinearDamping.Value;

            if (sender == _numMass)
            {
                float m = (float)_numMass.Value;
                rb.Mass = m;
                rb.InverseMass = (m <= 0f || float.IsInfinity(m)) ? 0f : 1f / m;

                _updatingUI = true;
                _numInvMass.Value = ClampToRange(_numInvMass, (decimal)rb.InverseMass);
                _updatingUI = false;
            }
            else if (sender == _numInvMass)
            {
                float im = (float)_numInvMass.Value;
                rb.InverseMass = im;
                rb.Mass = (im <= 0f || float.IsInfinity(im)) ? float.PositiveInfinity : 1f / im;

                _updatingUI = true;
                _numMass.Value = ClampToRange(_numMass, (decimal)rb.Mass);
                _updatingUI = false;
            }
        }

        private void OnCollisionToggled(object? sender, EventArgs e)
        {
            if (_updatingUI || _selectedBall?.Body is not IRigidBody rb)
                return;

            rb.allowCollision = _chkAllowCollision.Checked;
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _btnPause.Text = _isPaused ? "Resume" : "Pause";
            UpdateSelectedBallLabel();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            // keyboard ball controls (same as before)
            if (_selectedBall != null && _selectedBall.Body is IRigidBody body)
            {
                const float moveSpeed = 50f;

                switch (keyData)
                {
                    case Keys.Left:
                        body.Velocity = new Vector2(moveSpeed, body.Velocity.Y);
                        return true;
                    case Keys.Right:
                        body.Velocity = new Vector2(-moveSpeed, body.Velocity.Y);
                        return true;
                    case Keys.Up:
                        body.Velocity = new Vector2(body.Velocity.X, moveSpeed);
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
