using System;
using System.Collections.Generic;
using System.Numerics;
using PhysicsEngine.Physics;

namespace PhysicsEngine
{
    public class Collision
    {
        public void CollisionHandler(List<IRigidBody> bodies)
        {
            for (int i = 0; i < bodies.Count - 1; i++)
            {
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    if (DetectCollision(bodies[i], bodies[j]))
                    {
                        if (i == j)
                            continue;
                        Console.WriteLine("Collision detected");
                        ResolveCollision(bodies[i], bodies[j]);
                    }
                }
            }
        }

        public bool DetectCollision(IRigidBody object1, IRigidBody object2)
        {
            float dx = object2.Position.X - object1.Position.X;
            float dy = object2.Position.Y - object1.Position.Y;

            float distanceSq = dx * dx + dy * dy;
            float minDistance = object1.RenderRadius + object2.RenderRadius;
            float minDistanceSq = minDistance * minDistance;

            if (distanceSq > minDistanceSq)
                return false;
            if (distanceSq == 0f)
            {
                dx = 1f;
                dy = 0f;
                distanceSq = 1f;
            }

            float distance = MathF.Sqrt(distanceSq);
            float penetration = minDistance - distance;

            if (penetration > 0f)
            {
                Vector2 normal = new Vector2(dx / distance, dy / distance);
                float invMass1 = object1.Mass > 0f ? 1f / object1.Mass : 0f;
                float invMass2 = object2.Mass > 0f ? 1f / object2.Mass : 0f;
                float invMassSum = invMass1 + invMass2;

                if (invMassSum > 0f)
                {
                    const float percent = 0.8f; // 80% of the penetration
                    const float slop = 0.01f; // small overlap allowed

                    float corrMag = MathF.Max(penetration - slop, 0f) / invMassSum * percent;
                    Vector2 correction = normal * corrMag;

                    object1.Position -= correction * invMass1;
                    object2.Position += correction * invMass2;
                }
            }
            return true;
        }

        public void ResolveCollision(IRigidBody object1, IRigidBody object2)
        {
            Vector2 delta = object2.Position - object1.Position;
            float distSq = delta.LengthSquared();

            if (distSq < 1e-8f)
            {
                delta = new Vector2(1f, 0f);
                distSq = 1f;
            }

            float dist = MathF.Sqrt(distSq);
            Vector2 normal = delta / dist;

            float radiusSum = object1.RenderRadius + object2.RenderRadius;
            float penetration = radiusSum - dist;
            if (penetration <= 0f)
                return;

            // Use InverseMass consistently with your world
            float invMass1 = object1.InverseMass;
            float invMass2 = object2.InverseMass;
            float invMassSum = invMass1 + invMass2;
            if (invMassSum <= 0f)
                return;
        }
    }
}
