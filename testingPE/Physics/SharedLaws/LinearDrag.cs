using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics.SharedLaws
{
    public class LinearDrag : IForceGenerator
    {
        public float Coefficient { get; set; }

        public LinearDrag(float coefficient) => Coefficient = coefficient;

        public void Apply(IRigidBody body, float dt)
        {
            if (body.InverseMass == 0f)
                return;
            float area = body.Width * body.Height;
            float arealCoefficient = -Coefficient * area;
            body.AddForce(arealCoefficient * body.Velocity);
        }
    }
}
