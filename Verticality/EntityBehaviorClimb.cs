using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Common;
using Vintagestory.Server;
using static Verticality.CollisionUtils;

namespace Verticality
{
    internal class EntityBehaviorClimb : EntityBehavior
    {
        public const float minHeight = 0.5f; // minimum height from player's feet
        public const float maxHeight = 0.7f; // maximum height from player's LocalEyePos
        public const float grabDistance = 0.5f; // maximum distance to grab point

        private Grab grab;

        public bool ClimbKeyDown
        {
            get
            {
                return ((ICoreClientAPI)entity.Api).Input.IsHotKeyPressed("climb");
            }
        }

        public EntityBehaviorClimb(Entity entity) : base(entity) {}

        public override string PropertyName()
        {
            return "climb";
        }

        public override void OnGameTick(float deltaTime)
        {
            base.OnGameTick(deltaTime);

            if (entity.World.Side != EnumAppSide.Client) return;

            EntityPlayer player = (EntityPlayer)entity;

            if (ClimbKeyDown)
            {
                if (grab == null)
                {
                    grab = Grab.TryGrab(player);
                } else
                {
                    if (grab.CanStillGrab())
                    {
                        player.Properties.CanClimbAnywhere = true;
                    } else
                    {
                        grab = Grab.TryGrab(player);
                        if (grab == null) player.Properties.CanClimbAnywhere = false;
                    }
                }
            } else if (grab != null)
            {
                player.Properties.CanClimbAnywhere = false;
                grab = null;
            }
        }
    }

    internal class Grab
    {
        static float minHeight => EntityBehaviorClimb.minHeight;
        static float maxHeight => EntityBehaviorClimb.maxHeight;
        static float grabDistance => EntityBehaviorClimb.grabDistance;

        private EntityPlayer player;

        private BlockPos grabbedBlockPos;

        private Vec3d preciseGrabPos;

        // Checks if given player can grab, then either returns the successful Grab or null if not successful
        public static Grab TryGrab(EntityPlayer entity)
        {
            Vec3d grabLoc = GetGrabLocation(entity);

            double relHeightFeet = grabLoc.Y - entity.Pos.Y;
            double relHeightEyes = relHeightFeet - entity.LocalEyePos.Y;

            if (relHeightFeet > minHeight && relHeightEyes < maxHeight)
            {
                return new Grab()
                {
                    player = entity,
                    preciseGrabPos = grabLoc
                };
            }

            return null;
        }

        // Checks and returns whether player should still be holding onto this grab point
        public bool CanStillGrab()
        {
            double relHeightFeet = preciseGrabPos.Y - player.Pos.Y;
            double relHeightEyes = relHeightFeet - player.LocalEyePos.Y;

            if (relHeightFeet >= 0 && relHeightEyes < maxHeight)
                return player.Pos.HorDistanceTo(preciseGrabPos) <= grabDistance;

            return false;
        }

        public static SimpleParticleProperties debugParticles = new SimpleParticleProperties()
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
            ParticleModel = EnumParticleModel.Cube
        };

        // Find closest collision, if any, within a region of blocks in front of player
        // if collision found, crawl up surface until a gap is found or max height reached
        // return location of gap, if any
        public static Vec3d GetGrabLocation(EntityPlayer entity)
        {
            float yaw = entity.BodyYaw - GameMath.PIHALF;

            List<BlockPos> posList = new List<BlockPos>();
            
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


            Vec3d collPos = GetClosestPoint(collBoxes, entity.Pos.XYZ.AddCopy(0,minHeight,0));

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
    }
}
