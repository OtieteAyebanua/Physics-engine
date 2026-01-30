using System.Numerics;

namespace PhysicsEngine.Physics
{
    public class TinyRigidBody
    {
        public Vector2 LocalPosition;
        public IRigidBody Body;
    }
    public class CompoundBody
    {
        private List<TinyRigidBody> parts = new List<TinyRigidBody>();

        public void AddPart(IRigidBody body, Vector2 localPosition)
        {
            parts.Add(new TinyRigidBody { Body = body, LocalPosition = localPosition });
            Console.WriteLine(body.Position);
        }

        public void SetPosition(Vector2 worldPos)
        {
            foreach (var part in parts)
            {
                part.Body.Position = worldPos + part.LocalPosition;
            }
        }
    }
}