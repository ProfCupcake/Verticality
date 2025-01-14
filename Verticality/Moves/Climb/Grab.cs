using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using static Verticality.Lib.CollisionUtils;

namespace Verticality.Moves.Climb
{
    internal class Grab
    {
        static float minHeight => EntityBehaviorClimb.minHeight;
        static float maxHeight => EntityBehaviorClimb.maxHeight;
        static float grabDistance => EntityBehaviorClimb.grabDistance;

        private EntityPlayer player;

        private BlockSelection grabPos;

        // Checks if given player can grab, then either returns the successful Grab or null if not successful
        public static Grab TryGrab(EntityPlayer entity)
        {
            BlockSelection grabLoc = GetGrabLocationByRaycast(entity);

            if (grabLoc == null) return null;

            double relHeightFeet = grabLoc.FullPosition.Y - entity.Pos.Y;
            double relHeightEyes = relHeightFeet - entity.LocalEyePos.Y;
            
            if (relHeightFeet > minHeight && relHeightEyes < maxHeight)
            {
                return new Grab()
                {
                    player = entity,
                    grabPos = grabLoc
                };
                //debugParticles.Color = ColorUtil.ColorFromRgba(0, 255, 0, 255);
            }/* else
            {
                debugParticles.Color = ColorUtil.ColorFromRgba(0, 0, 255, 255);
            }
            
            debugParticles.MinPos = grabLoc;
            Vec3d towardsPlayer = grabLoc.SubCopy(entity.Pos.XYZ.AddCopy(0, minHeight, 0)).Normalize();
            debugParticles.MinVelocity = towardsPlayer.ToVec3f() * -2;
            debugParticles.AddVelocity = Vec3f.Zero;

            entity.World.SpawnParticles(debugParticles);

            debugParticles.MinVelocity = new Vec3f(-0.1f, -0.1f, -0.1f);
            debugParticles.AddVelocity = new Vec3f(0.2f, 0.2f, 0.2f);

            entity.World.SpawnParticles(debugParticles);
            */
            return null;
        }

        // Checks and returns whether player should still be holding onto this grab point
        public bool CanStillGrab()
        {
            double relHeightFeet = grabPos.FullPosition.Y - player.Pos.Y;
            double relHeightEyes = relHeightFeet - player.LocalEyePos.Y;

            if (relHeightFeet < 0 || relHeightEyes > maxHeight) return false;

            Ray ray = Ray.FromPositions(
                grabPos.FullPosition.AddCopy(0, 1 / 64f, 0),
                grabPos.FullPosition.Clone().SubCopy(0, 1 / 128f, 0)
                );
            AABBIntersectionTest aabb = player.World.InteresectionTester;
            aabb.LoadRayAndPos(ray);
            BlockSelection bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
            if (bs == null) return false;

            ray = Ray.FromPositions(
                grabPos.FullPosition.Clone(),
                grabPos.FullPosition.AddCopy(0, 1 / 64f, 0)
                );
            aabb.LoadRayAndPos(ray);
            bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
            if (bs != null) return false;

            return true;
        }

        public static SimpleParticleProperties debugParticles = new()
        {
            MinSize = 0.2f,
            MaxSize = 0.2f,
            MinVelocity = new Vec3f(-0.1f, -0.1f, -0.1f),
            AddVelocity = new Vec3f(0.2f, 0.2f, 0.2f),
            MinQuantity = 3,
            GravityEffect = 0,
            WithTerrainCollision = false,
            Color = ColorUtil.WhiteArgb,
            AddPos = Vec3d.Zero,
            LifeLength = 0.1f,
            ParticleModel = EnumParticleModel.Cube,
            LightEmission = ColorUtil.WhiteArgb
        };

        // Find closest collision, if any, within a region of blocks in front of player
        // if collision found, crawl up surface until a gap is found or max height reached
        // return location of gap, if any
        public static Vec3d GetGrabLocation(EntityPlayer entity)
        {
            float yaw = entity.BodyYaw - GameMath.PIHALF;

            List<BlockPos> posList = new();

            /*
            debugParticles.Color = ColorUtil.ColorFromRgba(255, 0, 0, 255);
            //*/

            for (int y_offset = 0; y_offset <= 2; y_offset++)
            {
                for (float yaw_offset = -GameMath.PIHALF / 2; yaw_offset <= GameMath.PIHALF / 2; yaw_offset += GameMath.PIHALF / 2)
                {
                    float x_offset = MathF.Cos(yaw + yaw_offset);
                    float z_offset = -MathF.Sin(yaw + yaw_offset);
                    BlockPos newPos = entity.Pos.XYZ.AddCopy(x_offset, minHeight + y_offset, z_offset).AsBlockPos;
                    posList.Add(newPos);

                    /*
                    debugParticles.MinPos = newPos.ToVec3d().AddCopy(0.5,0.5,0.5);
                    entity.World.SpawnParticles(debugParticles);
                    //*/
                }
            }

            IBlockAccessor blockAccessor = entity.World.BlockAccessor;
            Cuboidf[] collBoxes = Array.Empty<Cuboidf>();
            foreach (BlockPos pos in posList)
            {
                Cuboidf[] inCollBoxes = blockAccessor.GetBlock(pos).GetCollisionBoxes(blockAccessor, pos);
                if (inCollBoxes != null && inCollBoxes.Length > 0)
                {
                    Cuboidf[] inCollBoxes_offset = new Cuboidf[inCollBoxes.Length];
                    for (int c = 0; c < inCollBoxes.Length; c++)
                    {
                        inCollBoxes_offset[c] = inCollBoxes[c].OffsetCopy(pos);
                    }
                    collBoxes = CombineBoxArrays(collBoxes, inCollBoxes_offset);
                }
            }

            if (collBoxes.Length == 0) return Vec3d.Zero;

            string arrString = "\n";
            foreach (Cuboidf c in collBoxes)
            {
                arrString += c.ToString() + "\n";
            }


            Vec3d collPos = GetClosestPoint(collBoxes, entity.Pos.XYZ.AddCopy(0, minHeight, 0));

            Vec3d topPos = ToTheTop(collBoxes, collPos);

            /*
            if (collPos.DistanceTo(entity.Pos.XYZ) < 2)
            {
                debugParticles.MinPos = collPos;
                debugParticles.Color = ColorUtil.WhiteArgb;
                entity.World.SpawnParticles(debugParticles);
                
                debugParticles.MinPos = entity.Pos.XYZ.AddCopy(0, min, 0);
                debugParticles.Color = ColorUtil.BlackArgb;
                entity.World.SpawnParticles(debugParticles);

                debugParticles.MinPos = topPos;
                double relHeight = topPos.Y - entity.Pos.Y;
                if (relHeight > min && relHeight < max)
                {
                    debugParticles.Color = ColorUtil.ColorFromRgba(0, 255, 0, 255);
                }
                else
                {
                    debugParticles.Color = ColorUtil.ColorFromRgba(0, 0, 255, 255);
                }
                entity.World.SpawnParticles(debugParticles);
            } //*/

            return topPos;
        }

        public static BlockSelection GetGrabLocationByRaycast(EntityPlayer player)
        {
            float[] offsets = new float[]
            {
                0,
                0.12f, -0.12f,
                0.25f, -0.25f
            };
            foreach (float offset in offsets)
            {
                BlockSelection outPos = DoGrabRaycast(player, offset * GameMath.PIHALF);
                if (outPos != null) return outPos;
            }
            foreach (float offset in offsets)
            {
                BlockSelection outPos = DoGrabRaycast(player, offset * GameMath.PIHALF, (float)player.LocalEyePos.Y);
                if (outPos != null) return outPos;
            }
            return null;
        }

        public static BlockSelection DoGrabRaycast(EntityPlayer player, float yawOffset = 0)
        {
            return DoGrabRaycast(player, yawOffset, minHeight);
        }

        public static BlockSelection DoGrabRaycast(EntityPlayer player, float yawOffset, float heightOffset)
        {
            ICoreClientAPI capi = player.Api as ICoreClientAPI;

            Ray ray = Ray.FromAngles(player.Pos.XYZ.AddCopy(0, heightOffset, 0), 0, (player.Pos.Yaw - GameMath.PI) + yawOffset, grabDistance);
            AABBIntersectionTest aabb = player.World.InteresectionTester;
            aabb.LoadRayAndPos(ray);
            BlockSelection bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
            Vec3d outPos = null;
            if (bs != null)
            {
                bs.HitPosition.Sub(bs.Face.Normald * 1 / 128f);
                Vec3d bottom = bs.FullPosition.Clone();
                ray = Ray.FromPositions(
                    bs.FullPosition.AddCopy(0, maxHeight + player.LocalEyePos.Y, 0),
                    bottom
                    );
                aabb.LoadRayAndPos(ray);
                bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
                while (bs != null && ray.Length > 0)
                {
                    BlockSelection outSel = bs.Clone();
                    outSel.Block ??= aabb.bsTester.blockAccessor.GetBlock(outSel.Position); // for some reason, the BlockSelection Clone() doesn't clone the Block field
                    outPos = bs.FullPosition.Clone();
                    ray = Ray.FromPositions(
                        bs.FullPosition.Clone(),
                        bs.FullPosition.AddCopy(0, 1 / 64f, 0)
                        );
                    aabb.LoadRayAndPos(ray);
                    bs = aabb.GetSelectedBlock((float)ray.Length, null, true);

                    if (bs == null)
                    {
                        return outSel;
                    }

                    ray = Ray.FromPositions(
                        outPos.SubCopy(0, 1/128f, 0),
                        bottom
                        );
                    aabb.LoadRayAndPos(ray);
                    bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
                }
            }

            return null;
        }
    }
}
