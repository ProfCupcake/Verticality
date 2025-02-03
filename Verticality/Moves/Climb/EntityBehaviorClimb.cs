using System;
using Verticality.Lib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Verticality.Moves.Climb
{
    internal class EntityBehaviorClimb : EntityBehavior
    {
        public static float minHeight
        {
            get
            {
                return VerticalityModSystem.Config.modConfig.climbMinHeight;
            }
        }

        public static float maxHeight
        {
            get
            {
                return VerticalityModSystem.Config.modConfig.climbMaxHeight;
            }
        }

        public static float grabDistance
        {
            get
            {
                return VerticalityModSystem.Config.modConfig.climbGrabDistance;
            }
        }

        public static float climbJumpHForce
        {
            get
            {
                return VerticalityModSystem.Config.modConfig.climbJumpHForce;
            }
        }
        public static float climbJumpVForce
        {
            get
            {
                return VerticalityModSystem.Config.modConfig.climbJumpVForce;
            }
        }
        public int climbJumpCooldown = 1000;

        public long climbJumpTime;

        public bool canClimbJump = true;

        public Grab grab;

        public bool ClimbKeyDown
        {
            get
            {
                return ((ICoreClientAPI)entity.Api).Input.IsHotKeyPressed("climb");
            }
        }

        public EntityBehaviorClimb(Entity entity) : base(entity) { }

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
                    if (((ICoreClientAPI)entity.Api).ElapsedMilliseconds > climbJumpTime)
                    {
                        grab = Grab.TryGrab(player, null, null, (float?)(grabDistance * 1.5));
                        if (((ICoreClientAPI)entity.Api).Input.IsHotKeyPressed("jump"))
                        {
                            canClimbJump = false;
                        }
                    }
                }
                else
                {
                    if (grab.CanStillGrab())
                    {
                        //player.Properties.CanClimbAnywhere = true;
                        if (VerticalityModSystem.ClientConfig.showDebugParticles)
                        {
                            Grab.debugParticles.MinPos = grab.grabPos.FullPosition;
                            Grab.debugParticles.Color = ColorUtil.WhiteArgb;
                            player.World.SpawnParticles(Grab.debugParticles);
                        }

                        if (((ICoreClientAPI)entity.Api).Input.IsHotKeyPressed("jump"))
                        {
                            if (canClimbJump)
                            {
                                entity.Pos.Motion
                                    .Add(grab.grabPos.Face.Normald * climbJumpHForce / 60f)
                                    .Add(0, climbJumpVForce / 60f, 0);

                                grab = null;
                                climbJumpTime = ((ICoreClientAPI)entity.Api).ElapsedMilliseconds + climbJumpCooldown;
                            }
                        } else
                        {
                            canClimbJump = true;
                        }
                    }
                    else
                    {
                        grab = Grab.TryGrab(player);
                        //if (grab == null) player.Properties.CanClimbAnywhere = false;
                    }
                }
            }
            else
            {
                climbJumpTime = 0;
                if (grab != null)
                {
                    //player.Properties.CanClimbAnywhere = false;
                    grab = null;
                }
            }
        }
    }
}
