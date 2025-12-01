using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine.Physics
{
    public class SceneObject
    {
        public string Name { get; }
        public IRigidBody Body { get; }
        public object? RenderHandle { get; set; }

        public SceneObject(string name, IRigidBody body)
        {
            Name = name;
            Body = body;
        }
    }
}
