using System;
using Verticality.Lib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Verticality.Moves.Crawl
{
    public class EntityBehaviorCrawl : EntityBehavior
    {
        ICoreClientAPI capi;

        float baseJumpForce; 

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
            if (CrawlInputPressed())
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

            //if (!IsCrawling)
              //  if (StandCollisionCheck())
                //    TryCrawl();

            if (IsCrawling)
            {
                entity.Stats.Set("walkspeed", "crawlSpeed", VerticalityModSystem.Config.modConfig.crawlSpeedReduction, true);
                entity.Stats.Set("jumpHeightMul", "crawlJump", 0f, true);

                GlobalConstants.BaseJumpForce = 0f; // this shouldn't be necessary, but the above doesn't fukken work for some reason >:[

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

        private bool CrawlInputPressed()
        {
            if (VerticalityModSystem.ClientConfig.combinationCrawlKeys)
                if (capi.Input.IsHotKeyPressed("climb") && capi.Input.IsHotKeyPressed("sneak"))
                    return true;
            if (VerticalityModSystem.ClientConfig.dedicatedCrawlKey)
                if (capi.Input.IsHotKeyPressed("crawl"))
                    return true;
            return false;
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

                baseJumpForce = GlobalConstants.BaseJumpForce;

                IsCrawling = true;

                return true;
            }

            return false;
        }

        public bool TryStand()
        {
            if (IsCrawling)
            {
                if (StandCollisionCheck()) return false;

                entity.Properties.EyeHeight += 1;
                entity.Properties.CollisionBoxSize.Y += 1f;

                GlobalConstants.BaseJumpForce = baseJumpForce;

                entity.Stats.Remove("walkspeed", "crawlSpeed");
                entity.Stats.Remove("jumpHeightMul", "crawlJump");

                entity.StopAnimation("crawl-idle");
                entity.StopAnimation("crawl");

                IsCrawling = false;

                return true;
            }
            return false;
        }

        public bool StandCollisionCheck()
        {
            Cuboidf collBox = entity.World.GetEntityType("game:player").SpawnCollisionBox.Clone();
            collBox.Y2 -= 0.4f; // adjust to sneak height

            return entity.World.CollisionTester.IsColliding(entity.World.BlockAccessor, collBox, entity.Pos.XYZ, false);
        }
    }
}
