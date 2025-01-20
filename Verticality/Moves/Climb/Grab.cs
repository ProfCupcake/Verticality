﻿using System;
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
        private EntityPlayer player;

        public BlockSelection grabPos;

        public double lastY;

        // Checks if given player can grab, then either returns the successful Grab or null if not successful
        public static Grab TryGrab(EntityPlayer entity, float? minHeight, float? maxHeight, float? grabDistance)
        {
            minHeight ??= EntityBehaviorClimb.minHeight;
            maxHeight ??= (float?)(EntityBehaviorClimb.maxHeight + entity.Pos.Y + entity.LocalEyePos.Y); // TODO: fix how alarmingly inconsistent this is
            grabDistance ??= EntityBehaviorClimb.grabDistance;

            BlockSelection grabLoc = GetGrabLocationByRaycast(entity, (float)minHeight, (float)maxHeight, (float)grabDistance);

            if (grabLoc == null) return null;

            double relHeightFeet = grabLoc.FullPosition.Y - entity.Pos.Y;
            double relHeightEyes = relHeightFeet - entity.LocalEyePos.Y;
            
            if (relHeightFeet > minHeight && relHeightEyes < maxHeight)
            {
                return new Grab()
                {
                    player = entity,
                    grabPos = grabLoc,
                    lastY = entity.Pos.Y
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

        public static Grab TryGrab(EntityPlayer entity)
        {
            return TryGrab(entity, null, null, null);
        }

        // Checks and returns whether player should still be holding onto this grab point
        public bool CanStillGrab()
        {
            double relHeightFeet = grabPos.FullPosition.Y - player.Pos.Y;
            double relHeightEyes = relHeightFeet - player.LocalEyePos.Y;

            if (relHeightFeet < 0/* || relHeightEyes > maxHeight*/) return false;

            //if (player.Pos.HorDistanceTo(grabPos.FullPosition) > grabDistance) return false;

            if (!GapCheck(grabPos.FullPosition, player.World.InteresectionTester)) return false;

            return true;
        }

        public static bool GapCheck(Vec3d pos, AABBIntersectionTest aabb)
        {
            BlockSelection _ = null;
            return GapCheck(pos, aabb, ref _);
        }

        public static bool GapCheck(Vec3d pos, AABBIntersectionTest aabb, ref BlockSelection outBS)
        {
            Ray ray = Ray.FromPositions(
                pos.AddCopy(0, 1 / 64f, 0),
                pos.SubCopy(0, 1 / 128f, 0)
                );
            aabb.LoadRayAndPos(ray);
            BlockSelection bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
            if (bs == null) return false;

            outBS = bs.Clone();
            outBS.Block ??= bs.Block;

            ray = Ray.FromPositions(
                pos.Clone(),
                pos.AddCopy(0, 1 / 64f, 0)
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

        public static BlockSelection GetGrabLocationByRaycast(EntityPlayer player, float minHeight, float maxHeight, float grabDistance)
        {
            float[] heightOffsets = new float[] { 
                minHeight, 
                (float)player.LocalEyePos.Y * 0.8f
            };
            float[] yawOffsets = new float[]
            {
                0,
                0.12f, -0.12f,
                0.25f, -0.25f
            };

            foreach (float heightOffset in heightOffsets)
            {
                foreach (float yawOffset in yawOffsets)
                {
                    BlockSelection outPos = DoGrabRaycast(player, yawOffset * GameMath.PIHALF, heightOffset, maxHeight, grabDistance);
                    if (outPos != null) return outPos;
                }
            }
            return null;
        }

        public static BlockSelection DoGrabRaycast(EntityPlayer player, float yawOffset, float heightOffset, float maxHeight, float grabDistance)
        {
            ICoreClientAPI capi = player.Api as ICoreClientAPI;

            Ray ray = Ray.FromAngles(player.Pos.XYZ.AddCopy(0, heightOffset, 0), 0, (player.Pos.Yaw - GameMath.PI) + yawOffset, grabDistance);
            AABBIntersectionTest aabb = player.World.InteresectionTester;
            aabb.LoadRayAndPos(ray);
            BlockSelection bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
            if (bs != null)
            {
                BlockFacing face = bs.Face;
                bs.HitPosition.Sub(bs.Face.Normald * 1 / 128f);
                Vec3d bottom = bs.FullPosition.Clone();
                ray = Ray.FromPositions(
                    new(bs.FullPosition.X, maxHeight, bs.FullPosition.Z),
                    bottom
                    );
                aabb.LoadRayAndPos(ray);
                bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
                while (bs != null && ray.Length > 1/128f)
                {
                    if (GapCheck(bs.FullPosition, aabb))
                    {
                        bs.Face = face;
                        //((ICoreClientAPI)player.Api).ShowChatMessage("Grab! Face: " + face.ToString());
                        return bs;
                    }

                    ray = Ray.FromPositions(
                        bs.FullPosition.SubCopy(0, 1/128f, 0),
                        bottom
                        );
                    aabb.LoadRayAndPos(ray);
                    bs = aabb.GetSelectedBlock((float)ray.Length, null, true);
                }
            }

            return null;
        }

        public bool TrySlide(double slide)
        {
            Vec3d newPos = grabPos.FullPosition.AddCopy(grabPos.Face.GetHorizontalRotated(90).Normald.Clone().Scale(slide));
            BlockSelection bs = null;
            if (GapCheck(newPos, player.World.InteresectionTester, ref bs))
            {
                bs.Face = grabPos.Face;
                grabPos = bs;
                return true;
            }
            return false;
        }
    }
}
