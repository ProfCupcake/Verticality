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
using Vintagestory.API.Util;
using Vintagestory.Common;
using Vintagestory.Server;

namespace Verticality.Moves.Climb
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
