using System;
using UnityEngine;

namespace Rover656.Survivors.Common.Utility {
    // Developed and adapted with assistance from generative AI.
    public static class CollisionExtensions {
        public static bool Intersects(this Rect rect1, Rect rect2, out Vector2 penetrationVector) {
            if (rect1.Overlaps(rect2)) {
                penetrationVector = GetPenetrationVector(rect1, rect2);
                return true;
            }

            penetrationVector = Vector2.zero;
            return false;
        }

        public static bool Intersects(this Vector2 pos, Vector2 size, Rect otherRect, out Vector2 penetrationVector) {
            return Intersects(new Rect(pos, size), otherRect, out penetrationVector);
        }
        
        private static Vector2 GetPenetrationVector(Rect rectA, Rect rectB) {
            var overlapX = Math.Min(rectA.xMax - rectB.xMin, rectB.xMax - rectA.xMin);
            var overlapY = Math.Min(rectA.yMax - rectB.yMin, rectB.yMax - rectA.yMin);

            if (overlapX < overlapY) {
                return new Vector2(overlapX * (rectA.x < rectB.x ? -1 : 1), 0);
            } else {
                return new Vector2(0, overlapY * (rectA.y < rectB.y ? -1 : 1));
            }
        }
    }
}