using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicsEngine.Physics;
using System.Numerics;
using System.Diagnostics;

namespace sample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PhysicsComponent _physicsComponent;

        private SceneObject _playerObject;
        private SceneObject _floorObject;

        private const float MOVE_SPEED = 200f;
        private const float JUMP_SPEED = -200f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            _physicsComponent = new PhysicsComponent(this);
            Components.Add(_physicsComponent);

            // Floor
            var floorBody = new RigidBody(
                position: new System.Numerics.Vector2(400, 580), // bottom of screen
                mass: 1000f
            )
            {
                Width = 800,
                Height = 40,
                InverseMass = 0 // immovable
            };

            _floorObject = new SceneObject
            {
                Body = floorBody,
                Shape = ShapeType.Rectangle,
                Color = Color.DarkGray
            };
            _physicsComponent.AddObject(_floorObject);

            // Player
            var playerBody = new RigidBody(
                position: new System.Numerics.Vector2(400, 500),
                mass: 10f
            )
            {
                Width = 40,
                Height = 40,
                Restitution = 0.75f
            };

            _playerObject = new SceneObject
            {
                Body = playerBody,
                Shape = ShapeType.Circle,
                Color = Color.Red
            };
            _physicsComponent.AddObject(_playerObject);

            // Tiny blocks
            int numBlocks = 500;
            float blockWidth = 1f;
            float blockHeight = 1f;
            float spacing = 5f;
            float startX = 50f;
            float floorTopY = floorBody.Position.Y - floorBody.Height / 2f;

            for (int i = 0; i < numBlocks; i++)
            {
                int blocksPerRow = 10;
                int row = i / blocksPerRow;
                int col = i % blocksPerRow;

                float x = startX + col * (blockWidth + spacing) + blockWidth / 2f;
                float y = floorTopY - (row * (blockHeight + spacing)) - blockHeight / 2f;

                var blockBody = new RigidBody(
                    position: new System.Numerics.Vector2(x, y),
                    mass: 1f
                )
                {
                    Width = blockWidth,
                    Height = blockHeight
                };

                var blockObject = new SceneObject
                {
                    Body = blockBody,
                    Shape = ShapeType.Rectangle,
                    Color = Color.LightBlue
                };

                _physicsComponent.AddObject(blockObject);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            var velocity = _playerObject.Body.Velocity;

            if (keyboard.IsKeyDown(Keys.Left))
                velocity.X = -MOVE_SPEED;
            else if (keyboard.IsKeyDown(Keys.Right))
                velocity.X = MOVE_SPEED;
            else
                velocity.X = 0f;

            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Up))
                velocity.Y = JUMP_SPEED;
            else if (keyboard.IsKeyDown(Keys.Down))
                velocity.Y = MOVE_SPEED;

            _playerObject.Body.Velocity = velocity;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _physicsComponent.Draw(_spriteBatch); // Draw all rectangles/circles

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
