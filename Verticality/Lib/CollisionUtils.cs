using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Verticality.Lib
{
    internal class CollisionUtils
    {
        public static readonly double EPSILON = 1e-6f;

        // Returns closest point within box to given position
        public static Vec3d GetClosestPoint(Cuboidf coll, Vec3d pos)
        {
            return new Vec3d(
                Math.Clamp(pos.X, coll.X1, coll.X2),
                Math.Clamp(pos.Y, coll.Y1, coll.Y2),
                Math.Clamp(pos.Z, coll.Z1, coll.Z2)
                );
        }
        // Returns closest point within array of boxes to given position
        // out index returns index of collider in array
        public static Vec3d GetClosestPoint(Cuboidf[] colls, Vec3d pos, out int index)
        {
            Vec3d outPos = new();
            float distance = float.MaxValue;
            index = -1;
            for (int i = 0; i < colls.Length; i++)
            {
                Vec3d testPos = GetClosestPoint(colls[i], pos);
                if (testPos.SquareDistanceTo(pos) < distance)
                {
                    index = i;
                    distance = testPos.SquareDistanceTo(pos);
                    outPos = testPos;
                }
            }
            return outPos;
        }
        public static Vec3d GetClosestPoint(Cuboidf[] colls, Vec3d pos)
        {
            return GetClosestPoint(colls, pos, out int index);
        }

        // Check if given point collides with any box in given array
        // Returns index of first collision found, or -1 if no collision found
        public static int CollisionArrayContains(Cuboidf[] colls, Vec3d pos)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i].ContainsOrTouches(pos)) return i;
            }
            return -1;
        }

        // Moves pos upwards (positive Y) until it is no longer colliding with anything in the collider array
        // Returns resulting position
        public static Vec3d ToTheTop(Cuboidf[] colls, Vec3d pos)
        {
            if (colls.Length == 0) return pos;
            int curColl = CollisionArrayContains(colls, pos);
            if (curColl == -1) return pos;

            Vec3d outPos = new Vec3d(pos.X, pos.Y, pos.Z);
            while (curColl != -1)
            {
                outPos.Set(outPos.X, colls[curColl].MaxY, outPos.Z);
                do
                {
                    outPos.Add(0, EPSILON, 0);
                } while (colls[curColl].ContainsOrTouches(outPos));
                curColl = CollisionArrayContains(colls, outPos);
            }
            return outPos;
        }

        public static Cuboidf[] CombineBoxArrays(Cuboidf[] arr1, Cuboidf[] arr2)
        {
            Cuboidf[] outArr = new Cuboidf[arr1.Length + arr2.Length];
            Array.ConstrainedCopy(arr1, 0, outArr, 0, arr1.Length);
            Array.ConstrainedCopy(arr2, 0, outArr, arr1.Length, arr2.Length);
            return outArr;
        }
    }
}
