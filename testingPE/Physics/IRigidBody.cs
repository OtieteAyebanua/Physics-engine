using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics
{
    public interface IRigidBody
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        float Mass { get; }
        float Width { get; set; }
        float Height { get; set; }
        float InverseMass { get; }
        float Restitution { get; set; }
        float LinearDamping { get; set; }
        void AddForce(Vector2 f);
        void ClearForces();
        void Integrate(float dt);
    }
}
