using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PhysicsEngine.Physics.SharedLaws
{
    public class LinearDrag : IForceGenerator
    {
        public float Coefficient { get; set; }

        public LinearDrag(float coefficient) => Coefficient = coefficient;

        public void Apply(IRigidBody body)
        {
            var x = body.Velocity;
            body.AddForce(-Coefficient * body.Velocity);
        }

    }
}
