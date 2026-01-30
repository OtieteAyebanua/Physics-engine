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
        public float G { get; set; }

        public Gravity(float g) => G = g;

        public void Apply(IRigidBody body)
        {
            var x = body.Velocity;
            body.AddForce(new Vector2(0f, G * body.Mass));
        }
    }
}
