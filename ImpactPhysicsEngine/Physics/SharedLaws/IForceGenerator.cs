using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics.SharedLaws
{
    public interface IForceGenerator
    {
        void Apply(IRigidBody body, float dt);
    }
}
