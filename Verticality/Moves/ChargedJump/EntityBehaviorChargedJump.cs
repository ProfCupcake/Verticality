using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Verticality.Moves.ChargedJump
{
    internal class EntityBehaviorChargedJump : EntityBehavior
    {
        private static float jumpForceAdd = 1.7f;
        private const float jumpChargeTime = 1.5f;

        private float jumpForce
        {
            get
            {
                // Vintagestory.API.Common.Entities.PModuleOnGround
                return (GlobalConstants.BaseJumpForce + jumpForceAdd) * MathF.Sqrt(MathF.Max(1f, entity.Stats.GetBlended("jumpHeightMul"))) / 60f;
            }
        }
        float t;
        public EntityBehaviorChargedJump(Entity entity) : base(entity) { }

        public override string PropertyName()
        {
            return "chargedjump";
        }

        private TextCommandResult SetJumpForce(TextCommandCallingArgs args)
        {
            if ((float)args[0] < 0f)
            {
                ((IClientPlayer)args.Caller.Player).ShowChatNotification("Current jumpForceAdd: " + jumpForceAdd);
            }
            else
            {
                jumpForceAdd = (float)args[0];
            }

            return TextCommandResult.Success();
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if (entity.Api.Side != EnumAppSide.Client) return;

            EntityPlayer player = (EntityPlayer)entity;

            ICoreClientAPI capi = player.Api as ICoreClientAPI;

            if (player.Controls.Sneak && player.OnGround)
            {
                if (capi.Input.IsHotKeyPressed("jump"))
                {
                    t += dt;
                }
                else
                {
                    if (t > 0.1f)
                    {
                        player.Pos.Motion.Y += GameMath.Clamp(GameMath.Lerp(0, jumpForce, t / jumpChargeTime), 0, jumpForce);
                    }
                    t = 0;
                }
            }
            else t = 0;
        }
    }
}
