using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Verticality.Moves.Crawl
{
    public class EntityBehaviorCrawl : EntityBehavior
    {
        ICoreClientAPI capi;

        bool DidKeyPress;
        bool IsCrawling;

        public EntityBehaviorCrawl(Entity entity) : base(entity) {}

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            if (entity.Api.Side == EnumAppSide.Client)
            {
                capi = entity.Api as ICoreClientAPI;
            }
        }

        public override void OnGameTick(float dt)
        {
            if (capi.Input.IsHotKeyPressed("climb") && capi.Input.IsHotKeyPressed("sneak"))
            {
                if (!DidKeyPress)
                {
                    if (IsCrawling)
                    {
                        capi.ShowChatMessage("trying to stand");
                        TryStand();
                    } else
                    {
                        capi.ShowChatMessage("trying to crawl");
                        TryCrawl();
                    }
                    DidKeyPress = true;
                }
            } else
            {
                DidKeyPress = false;
            }

            if (IsCrawling)
            {
                if (((EntityPlayer)entity).Controls.TriesToMove)
                {
                    if (entity.AnimManager.IsAnimationActive("crawl-idle")) entity.StopAnimation("crawl-idle");
                    if (!entity.AnimManager.IsAnimationActive("crawl")) entity.StartAnimation("crawl");
                } else
                {
                    if (entity.AnimManager.IsAnimationActive("crawl")) entity.StopAnimation("crawl");
                    if (!entity.AnimManager.IsAnimationActive("crawl-idle")) entity.StartAnimation("crawl-idle");
                }
            }
        }

        public override string PropertyName()
        {
            return "crawl";
        }

        public bool TryCrawl()
        {
            if (!IsCrawling)
            {
                entity.Properties.EyeHeight -= 1;
                entity.Properties.CollisionBoxSize.Y -= 1f;

                entity.Stats.Set("walkspeed", "crawlSpeed", -0.5f, true);

                IsCrawling = true;

                return true;
            }

            return false;
        }

        public bool TryStand()
        {
            if (IsCrawling)
            {
                entity.Properties.EyeHeight += 1;
                entity.Properties.CollisionBoxSize.Y += 1f;

                entity.Stats.Remove("walkspeed", "crawlSpeed");

                entity.StopAnimation("crawl-idle");
                entity.StopAnimation("crawl");

                IsCrawling = false;

                return true;
            }
            return false;
        }
    }
}
