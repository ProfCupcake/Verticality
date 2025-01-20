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

                entity.StartAnimation("crawl");

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

                entity.StopAnimation("crawl");

                IsCrawling = false;

                return true;
            }
            return false;
        }
    }
}
