using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace Verticality
{
    internal class EntityBehaviorClimb : EntityBehavior
    {
        private Grab grab;

        private bool climbKeyDown;

        public EntityBehaviorClimb(Entity entity) : base(entity)
        {
            climbKeyDown = false;
        }

        public override string PropertyName()
        {
            return "climb";
        }

        public void OnKeyClimb()
        {
            
        }
        public override void OnGameTick(float deltaTime)
        {
            base.OnGameTick(deltaTime);

            EntityPlayer player = (EntityPlayer)entity;
            // pseudocode scaffold go
            if (((ICoreClientAPI)entity.Api).Input.IsHotKeyPressed("climb"))
            {
                if (grab == null)
                {
                    grab = Grab.TryGrab(player);
                } else
                {
                    if (grab.CanStillGrab())
                    {
                        player.Properties.CanClimbAnywhere = true;
                        //player.StartAnimation("climbup");
                    } else
                    {
                        player.Properties.CanClimbAnywhere = false;
                        grab = null;
                        //player.StopAnimation("climbup");
                    }
                }
            } else if (grab != null)
            {
                player.Properties.CanClimbAnywhere = false;
                grab = null;
                //player.StopAnimation("climbup");
            }
        }
    }

    internal class Grab
    {
        private EntityPlayer player;

        private BlockPos grabbedBlockPos;

        private Vec3d preciseGrabPos;

        // Checks if given player can grab, then either returns the successful Grab or null if not successful
        public static Grab TryGrab(EntityPlayer entity)
        {
            //Conditions for grab:
            // Block in front of player within grab distance (armLength?)
            // Block is solid/collidable
            // Block is lower than grab max height (eyeheight + armlength?)
            // Block is higher than grab min height (which should be somewhere around waist height - probably just eyeheight/2)
            // Block above block is air

            //When grabbing, we need to log:
            // Block being grabbed
            // Precise grab position (closest point on top edge of face towards player)


            IBlockAccessor blockAccessor = entity.World.BlockAccessor;
            BlockPos footBlock = entity.Pos.Copy().Add(0, 0.2, 0).AheadCopy(1).AsBlockPos; // this should be the block in front of the player's feet/legs
            BlockPos headBlock = footBlock.AddCopy(0, 1, 0); // this should be the block in front of the player's torso/head
            BlockPos aboveHeadBlock = headBlock.AddCopy(0,1,0); // ... and the one above that

            if ((blockAccessor.GetBlock(footBlock).Id != 0) && (blockAccessor.GetBlock(headBlock).Id == 0))
            {
                return new Grab()
                {
                    grabbedBlockPos = footBlock,
                    player = entity
                };
            } else if ((blockAccessor.GetBlock(headBlock).Id != 0) && (blockAccessor.GetBlock(aboveHeadBlock).Id == 0))
            {
                return new Grab()
                {
                    grabbedBlockPos = headBlock,
                    player = entity
                };
            }
            return null;
        }

        // Checks and returns whether player should still be holding onto this grab point
        public bool CanStillGrab()
        {
            return player.Pos.AsBlockPos.DistanceTo(grabbedBlockPos) < 1.5;
        }
    }
}
