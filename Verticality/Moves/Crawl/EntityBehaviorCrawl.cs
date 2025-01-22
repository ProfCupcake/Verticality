using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Verticality.Moves.Crawl
{
    public class EntityBehaviorCrawl : EntityBehavior
    {
        ICoreClientAPI capi;

        bool DidKeyPress;
        bool IsCrawling;

        public EntityBehaviorCrawl(Entity entity) : base(entity) { }

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
                        //capi.ShowChatMessage("trying to stand");
                        TryStand();
                    }
                    else
                    {
                        //capi.ShowChatMessage("trying to crawl");
                        TryCrawl();
                    }
                    DidKeyPress = true;
                }
            }
            else
            {
                DidKeyPress = false;
            }

            if (IsCrawling)
            {
                if (((EntityPlayer)entity).Controls.TriesToMove)
                {
                    if (entity.AnimManager.IsAnimationActive("crawl-idle")) entity.StopAnimation("crawl-idle");
                    if (!entity.AnimManager.IsAnimationActive("crawl")) entity.StartAnimation("crawl");
                }
                else
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

                entity.Stats.Set("walkspeed", "crawlSpeed", -0.66f, true);

                IsCrawling = true;

                return true;
            }

            return false;
        }

        public bool TryStand()
        {
            if (IsCrawling)
            {
                Cuboidf collBox = entity.World.GetEntityType("game:player").SpawnCollisionBox.Clone();
                collBox.Y2 -= 0.4f; // adjust to sneak height

                if (entity.World.CollisionTester.IsColliding(entity.World.BlockAccessor, collBox, entity.Pos.XYZ, false))
                {
                    return false;
                }

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
