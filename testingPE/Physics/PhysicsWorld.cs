using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PhysicsEngine.Physics.SharedLaws;

namespace PhysicsEngine.Physics
{
    public class PhysicsWorld
    {
        public List<IRigidBody> Bodies = new();
        public List<IForceGenerator> GlobalForces = new();

        public Vector2? MinBounds;
        public Vector2? MaxBounds;

        public void AddBody(IRigidBody body) => Bodies.Add(body);

        public void AddGlobalForce(IForceGenerator force) => GlobalForces.Add(force);

        public void Step(float dt)
        {
            List<IRigidBody> bodiesAllowingCollision = new List<IRigidBody>();
            foreach (var body in Bodies)
            {
                foreach (var force in GlobalForces)
                {
                    force.Apply(body, dt);
                }
            }

            foreach (var body in Bodies)
            {
                body.Integrate(dt);
            }

            for (int i = 0; i < Bodies.Count; i++)
            {
                if (Bodies[i].allowCollision)
                {
                    bodiesAllowingCollision.Add(Bodies[i]);
                }
            }
            var collision = new Collision();
            collision.CollisionHandler(bodiesAllowingCollision);

            foreach (var body in Bodies)
            {
                if (body.InverseMass == 0f)
                    continue;

                var pos = body.Position;
                var vel = body.Velocity;

                if (MinBounds.HasValue)
                {
                    var min = MinBounds.Value;
                    if (pos.X < min.X)
                    {
                        pos.X = min.X;
                        vel.X = -vel.X * body.Restitution;
                    }
                    if (pos.Y < min.Y)
                    {
                        pos.Y = min.Y;
                        vel.Y = -vel.Y * body.Restitution;
                    }
                }

                if (MaxBounds.HasValue)
                {
                    var max = MaxBounds.Value;
                    if (pos.X > max.X)
                    {
                        pos.X = max.X;
                        vel.X = -vel.X * body.Restitution;
                    }
                    if (pos.Y > max.Y)
                    {
                        pos.Y = max.Y;
                        vel.Y = -vel.Y * body.Restitution;
                    }
                }

                body.Position = pos;
                body.Velocity = vel;
            }
        }
    }
}
