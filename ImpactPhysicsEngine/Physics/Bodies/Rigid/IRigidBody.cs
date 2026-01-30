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
        float Mass { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        float InverseMass { get; set; }
        float Restitution { get; set; }
        float LinearDamping { get; set; }
        bool allowCollision { get; set; }
        void AddForce(Vector2 f);
        void ClearForces();
        void Integrate(float dt);
    }
}
