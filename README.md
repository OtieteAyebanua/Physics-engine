# ImpactPhysicsEngine

A lightweight, .NET 8.0 physics engine library for 2D rigid body dynamics, collision detection, and force simulation. Designed for easy integration with rendering frameworks like WinForms and MonoGame.

## Features

- **Rigid Body Dynamics**: Full 2D rigid body simulation with mass, velocity, and acceleration
- **Collision Detection**: AABB (Axis-Aligned Bounding Box) collision detection and response
- **Force System**: Pluggable force generators including gravity, drag, and custom forces
- **Boundary Constraints**: Optional world boundaries with automatic bounce-back
- **Impulse-Based Collision**: Physics-accurate collision response with restitution

## Getting Started

### Installation

1. Add the `ImpactPhysicsEngine` project as a reference to your application
2. Import the namespace:
```csharp
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;
```

## Core Concepts

### PhysicsWorld
The main simulation engine that orchestrates all physics calculations.

```csharp
// Create a physics world
var world = new PhysicsWorld
{
    MinBounds = new Vector2(0, 0),      // Bottom-left boundary
    MaxBounds = new Vector2(800, 600)   // Top-right boundary
};

// Add global forces (affects all bodies)
world.AddGlobalForce(new Gravity(new Vector2(0, -9.81f)));
world.AddGlobalForce(new LinearDrag(0.1f));

// Step the simulation (call once per frame)
world.Step(deltaTime);
```

### RigidBody
Represents a physical object with mass, position, and velocity.

```csharp
// Create a rigid body
var body = new RigidBody(
    position: new Vector2(100, 100),
    mass: 5f
)
{
    Width = 32,
    Height = 32,
    Restitution = 0.8f,           // Bounciness (0-1)
    LinearDamping = 0.1f,         // Air resistance
    allowCollision = true
};

// Add to world
world.AddBody(body);

// Apply forces each frame
body.AddForce(new Vector2(100, 0)); // 100N to the right
```

### SceneObject
Wraps a physics body with scene metadata and rendering information.

```csharp
var sceneObject = new SceneObject(
    name: "Player",
    body: body
);

// Store rendering handle (texture, sprite, etc.)
sceneObject.RenderHandle = myTexture;
```

## Integration Guides

### WinForms Integration

#### Setup

1. Create a custom `Control` for rendering:

```csharp
using System.Numerics;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;

public class PhysicsPanel : Control
{
    private PhysicsWorld _world;
    private List<SceneObject> _objects = new();
    private float _accumulator = 0;
    private const float TIMESTEP = 1f / 60f; // 60 FPS

    public PhysicsPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        InitializePhysics();
    }

    private void InitializePhysics()
    {
        _world = new PhysicsWorld
        {
            MinBounds = new Vector2(0, 0),
            MaxBounds = new Vector2(Width, Height)
        };

        _world.AddGlobalForce(new Gravity(new Vector2(0, 500))); // Downward gravity
        _world.AddGlobalForce(new LinearDrag(0.05f));
    }

    public void AddObject(SceneObject obj)
    {
        _objects.Add(obj);
        _world.AddBody(obj.Body);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Render all objects
        foreach (var obj in _objects)
        {
            var pos = obj.Body.Position;
            var width = obj.Body.Width;
            var height = obj.Body.Height;

            // Draw as rectangle (you can customize this)
            e.Graphics.FillRectangle(
                Brushes.Blue,
                pos.X - width / 2,
                pos.Y - height / 2,
                width,
                height
            );

            e.Graphics.DrawRectangle(
                Pens.White,
                pos.X - width / 2,
                pos.Y - height / 2,
                width,
                height
            );
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Start game loop timer
        var timer = new Timer { Interval = 16 }; // ~60 FPS
        timer.Tick += (s, args) =>
        {
            _accumulator += 0.016f;
            while (_accumulator >= TIMESTEP)
            {
                _world.Step(TIMESTEP);
                _accumulator -= TIMESTEP;
            }
            Invalidate();
        };
        timer.Start();
    }
}
```

#### Usage in a Form

```csharp
public class MainForm : Form
{
    private PhysicsPanel _physicsPanel;

    public MainForm()
    {
        Text = "Physics Engine Demo";
        Size = new Size(800, 600);

        _physicsPanel = new PhysicsPanel
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_physicsPanel);

        // Create some test objects
        for (int i = 0; i < 5; i++)
        {
            var body = new RigidBody(
                new Vector2(100 + i * 60, 50),
                mass: 2f
            )
            {
                Width = 30,
                Height = 30,
                Restitution = 0.7f
            };

            var obj = new SceneObject($"Box_{i}", body);
            _physicsPanel.AddObject(obj);
        }
    }
}
```

---

### MonoGame Integration

#### Setup

1. Create a custom `GameComponent`:

```csharp
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;

public class PhysicsGameComponent : GameComponent
{
    private PhysicsWorld _world;
    private List<SceneObject> _objects = new();
    private Texture2D _whitePixel;
    private const float TIMESTEP = 1f / 60f;

    public PhysicsGameComponent(Game game) : base(game)
    {
        InitializePhysics();
    }

    private void InitializePhysics()
    {
        _world = new PhysicsWorld
        {
            MinBounds = new Vector2(0, 0),
            MaxBounds = new Vector2(800, 600)
        };

        _world.AddGlobalForce(new Gravity(new Vector2(0, 500)));
        _world.AddGlobalForce(new LinearDrag(0.05f));
    }

    public void AddObject(SceneObject obj)
    {
        _objects.Add(obj);
        _world.AddBody(obj.Body);
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        // Create a 1x1 white pixel for rendering
        _whitePixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var obj in _objects)
        {
            var pos = obj.Body.Position;
            var width = obj.Body.Width;
            var height = obj.Body.Height;

            // Draw rectangle using scaled pixel texture
            spriteBatch.Draw(
                _whitePixel,
                new Rectangle(
                    (int)(pos.X - width / 2),
                    (int)(pos.Y - height / 2),
                    (int)width,
                    (int)height
                ),
                Color.CornflowerBlue
            );
        }
    }
}
```

#### Usage in a MonoGame Game

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using PhysicsEngine.Physics;

public class PhysicsGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private PhysicsGameComponent _physicsComponent;

    public PhysicsGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();

        _physicsComponent = new PhysicsGameComponent(this);
        Components.Add(_physicsComponent);

        // Create test objects
        for (int i = 0; i < 5; i++)
        {
            var body = new RigidBody(
                new Vector2(100 + i * 80, 100),
                mass: 2f
            )
            {
                Width = 32,
                Height = 32,
                Restitution = 0.75f
            };

            var obj = new SceneObject($"Box_{i}", body);
            _physicsComponent.AddObject(obj);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _physicsComponent.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
```

---

## API Reference

### PhysicsWorld

| Method | Description |
|--------|-------------|
| `AddBody(IRigidBody body)` | Add a rigid body to the simulation |
| `AddGlobalForce(IForceGenerator force)` | Add a global force affecting all bodies |
| `Step(float dt)` | Advance simulation by `dt` seconds |

| Property | Description |
|----------|-------------|
| `Bodies` | List of all bodies in the world |
| `GlobalForces` | List of global force generators |
| `MinBounds` | Lower boundary (nullable) |
| `MaxBounds` | Upper boundary (nullable) |

### RigidBody

| Property | Description |
|----------|-------------|
| `Position` | World position (Vector2) |
| `Velocity` | Linear velocity (Vector2) |
| `Mass` | Body mass in kg |
| `Width` | Body width for collision |
| `Height` | Body height for collision |
| `InverseMass` | 1/Mass (read-only for static bodies) |
| `Restitution` | Bounciness (0-1) |
| `LinearDamping` | Air resistance (0-1) |
| `allowCollision` | Enable/disable collision |

| Method | Description |
|--------|-------------|
| `AddForce(Vector2 f)` | Apply a force in Newtons |
| `ClearForces()` | Reset accumulated forces |
| `Integrate(float dt)` | Update velocity and position |

### Force Generators

#### Gravity
```csharp
var gravity = new Gravity(new Vector2(0, -9.81f)); // or (0, 500) for pixels
world.AddGlobalForce(gravity);
```

#### LinearDrag
```csharp
var drag = new LinearDrag(0.1f); // coefficient
world.AddGlobalForce(drag);
```

#### Custom Force Generator
```csharp
public class CustomForce : IForceGenerator
{
    public void Apply(IRigidBody body, float dt)
    {
        // Your force logic here
        body.AddForce(new Vector2(force, 0));
    }
}

world.AddGlobalForce(new CustomForce());
```

---

## Best Practices

1. **Fixed Timestep**: Always use fixed timesteps (e.g., 1/60s) for consistent physics
2. **Mass Validation**: Use positive masses; 0 or infinity creates static bodies
3. **Restitution**: Values > 1 add energy (unstable); 0 is inelastic
4. **Damping**: Small values (0.01-0.2) prevent infinite oscillation
5. **Boundary Checking**: Set MinBounds/MaxBounds to prevent objects escaping
6. **Collision Flags**: Disable collision on non-physical objects to improve performance

## Performance Tips

- Disable collision on static scenery: `body.allowCollision = false`
- Reduce collision checks by grouping objects spatially
- Use appropriate damping to prevent instability
- Limit the number of simultaneous collisions

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Objects fall through floor | Increase collision resolution (adjust `slop` in Collision.cs) |
| Jittering physics | Use consistent timestep, reduce damping |
| Objects escape bounds | Check MinBounds/MaxBounds are set correctly |
| No collision | Ensure `allowCollision = true` and bodies overlap |

## License

MIT License - See LICENSE file for details

## Contributing

Feel free to submit issues and pull requests!
