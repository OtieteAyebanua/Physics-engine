using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics
{
    public class RigidBody : IRigidBody
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Mass { get; set; }
        public required float Width { get; set; }
        public required float Height { get; set; }
        public float Restitution { get; set; } = 1f;
        public float LinearDamping { get; set; } = 0.0f;
        public bool allowCollision { get; set; } = true;

        private Vector2 _accumulateedForces;

        public float InverseMass
        {
            get => (Mass <= 0f || float.IsInfinity(Mass)) ? 0f : 1f / Mass;
            set
            {
                Mass =
                    (value <= 0f || float.IsInfinity(value)) ? float.PositiveInfinity : 1f / value;
            }
        }

        public RigidBody(Vector2 position, float mass)
        {
            Position = position;
            Mass = mass;
        }

        public void AddForce(Vector2 f) => _accumulateedForces += f;

        public void ClearForces() => _accumulateedForces = Vector2.Zero;

        public void Integrate(float dt)
        {
            if (InverseMass <= 0f)
            {
                ClearForces();
                return;
            }

            Vector2 acceleration = _accumulateedForces * InverseMass;
            Velocity += acceleration * dt;
            Velocity *= MathF.Max(0f, 1f - LinearDamping * dt);

            // Integrate position using the updated velocity (semi-implicit Euler)
            Position += Velocity * dt;

            ClearForces();
        }
    }
}
