using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics.SharedLaws
{
    public class Gravity : IForceGenerator
    {
        public Vector2 G { get; set; }

        public Gravity(Vector2 g) => G = g;

        public void Apply(IRigidBody body, float dt)
        {
            if (body.InverseMass == 0f)
                return;
            body.AddForce(G * body.Mass);
        }
    }
}
