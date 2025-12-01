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
                if (!bodies[i].allowCollision)
                    continue;
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    if (!bodies[j].allowCollision)
                        continue;
                    if (DetectCollision(bodies[i], bodies[j]))
                    {
                        if (i == j)
                            continue;
                        ResolveCollision(bodies[i], bodies[j]);
                    }
                }
            }
        }

        public bool DetectCollision(IRigidBody object1, IRigidBody object2)
        {
            float halfWidth1 = object1.Width * 0.5f;
            float halfHeight1 = object1.Height * 0.5f;
            float halfWidth2 = object2.Width * 0.5f;
            float halfHeight2 = object2.Height * 0.5f;
            float dx = object1.Position.X - object2.Position.X;
            float dy = object1.Position.Y - object2.Position.Y;
            bool overlapX = Math.Abs(dx) <= (halfWidth1 + halfWidth2);
            bool overlapY = Math.Abs(dy) <= (halfHeight1 + halfHeight2);
            return overlapX && overlapY;
        }

        public void ResolveCollision(IRigidBody object1, IRigidBody object2)
        {
            if (!object1.allowCollision || !object2.allowCollision)
                return;

            float halfW1 = object1.Width * 0.5f;
            float halfH1 = object1.Height * 0.5f;
            float halfW2 = object2.Width * 0.5f;
            float halfH2 = object2.Height * 0.5f;

            Vector2 delta = object2.Position - object1.Position;

            float overlapX = (halfW1 + halfW2) - MathF.Abs(delta.X);
            if (overlapX <= 0f)
                return;

            float overlapY = (halfH1 + halfH2) - MathF.Abs(delta.Y);
            if (overlapY <= 0f)
                return;

            Vector2 normal;
            float penetration;

            if (overlapX < overlapY)
            {
                penetration = overlapX;
                normal = delta.X > 0f ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);
            }
            else
            {
                penetration = overlapY;
                normal = delta.Y > 0f ? new Vector2(0f, 1f) : new Vector2(0f, -1f);
            }

            float invMass1 = object1.InverseMass;
            float invMass2 = object2.InverseMass;
            float invMassSum = invMass1 + invMass2;
            if (invMassSum <= 0f)
                return;

            Vector2 relativeVel = object2.Velocity - object1.Velocity;
            float velAlongNormal = Vector2.Dot(relativeVel, normal);
            if (velAlongNormal < 0f)
            {
                float restitution = MathF.Min(object1.Restitution, object2.Restitution);

                float j = -(1f + restitution) * velAlongNormal;
                j /= invMassSum;

                Vector2 impulse = j * normal;

                object1.Velocity -= impulse * invMass1;
                object2.Velocity += impulse * invMass2;
            }

            const float percent = 0.8f;
            const float slop = 0.01f;

            float correctionMag = MathF.Max(penetration - slop, 0f) / invMassSum * percent;
            Vector2 correction = correctionMag * normal;

            object1.Position -= correction * invMass1;
            object2.Position += correction * invMass2;
        }
    }
}
