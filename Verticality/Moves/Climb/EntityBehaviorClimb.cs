using Verticality.Lib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

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

        private Grab grab;

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
                    grab = Grab.TryGrab(player);
                }
                else
                {
                    if (grab.CanStillGrab())
                    {
                        player.Properties.CanClimbAnywhere = true;
                    }
                    else
                    {
                        grab = Grab.TryGrab(player);
                        if (grab == null) player.Properties.CanClimbAnywhere = false;
                    }
                }
            }
            else if (grab != null)
            {
                player.Properties.CanClimbAnywhere = false;
                grab = null;
            }
        }
    }
}
