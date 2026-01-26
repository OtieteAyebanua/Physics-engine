using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;
using System.Numerics;

public enum ShapeType
{
    Circle,
    Rectangle
}

public class SceneObject
{
    public IRigidBody Body { get; set; }
    public ShapeType Shape { get; set; } = ShapeType.Circle;
    public Color Color { get; set; } = Color.White;

    public SceneObject() { }

    public SceneObject(string name, IRigidBody body)
    {
        Body = body;
    }
}

public class PhysicsComponent : DrawableGameComponent
{
    private PhysicsWorld _world;
    private List<SceneObject> _objects = new();

    private Texture2D _whitePixel;
    private Texture2D _circleTexture;

    private const float TIMESTEP = 1f / 60f;

    public PhysicsComponent(Game game) : base(game)
    {
        InitializePhysics();
    }

    private void InitializePhysics()
    {
        _world = new PhysicsWorld
        {
            MinBounds = new System.Numerics.Vector2(0, 0),
            MaxBounds = new System.Numerics.Vector2(800, 600)
        };

        _world.AddGlobalForce(new Gravity(9.8f));
        _world.AddGlobalForce(new LinearDrag(1f));
    }

    public void AddObject(SceneObject obj)
    {
        _objects.Add(obj);
        _world.AddBody(obj.Body);
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        _circleTexture = CreateCircleTexture(GraphicsDevice, 50);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _world.Step(TIMESTEP);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var obj in _objects)
        {
            if (obj.Shape == ShapeType.Circle)
                DrawCircle(spriteBatch, obj);
            else
                DrawRectangle(spriteBatch, obj);
        }
    }

    private void DrawCircle(SpriteBatch spriteBatch, SceneObject obj)
    {
        System.Numerics.Vector2 position = new System.Numerics.Vector2(obj.Body.Position.X, obj.Body.Position.Y);
        float diameter = obj.Body.Width;
        float scale = diameter / _circleTexture.Width;

        spriteBatch.Draw(
            _circleTexture,
            position,
            null,
            obj.Color,
            0f,
            new System.Numerics.Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f),
            scale,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawRectangle(SpriteBatch spriteBatch, SceneObject obj)
    {
        System.Numerics.Vector2 position = new System.Numerics.Vector2(obj.Body.Position.X, obj.Body.Position.Y);

        Rectangle rect = new Rectangle(
            (int)(position.X - obj.Body.Width / 2f),
            (int)(position.Y - obj.Body.Height / 2f),
            (int)obj.Body.Width,
            (int)obj.Body.Height
        );

        spriteBatch.Draw(_whitePixel, rect, obj.Color);
    }

    private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius)
    {
        int diameter = radius * 2;
        Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);

        Color[] data = new Color[diameter * diameter];
        System.Numerics.Vector2 center = new System.Numerics.Vector2(radius);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                int index = x + y * diameter;
                System.Numerics.Vector2 pos = new System.Numerics.Vector2(x, y);

                data[index] =
                    System.Numerics.Vector2.Distance(pos, center) <= radius
                        ? Color.White
                        : Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
    }
}
