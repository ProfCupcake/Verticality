using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Verticality.Moves.Crawl
{
    public class EntityBehaviorCrawl : EntityBehavior
    {
        ICoreClientAPI capi;
        ICoreServerAPI sapi;

        EntityProperties baseProperties;

        bool DidKeyPress;
        private bool sneakPressed;

        public bool IsCrawling
        {
            get
            {
                return entity.WatchedAttributes.GetBool("Verticality:IsCrawling");
            }
            set
            {
                if (entity.Api.Side == EnumAppSide.Server)
                {
                    entity.WatchedAttributes.SetBool("Verticality:IsCrawling", value);
                }
                else if (entity.Api.Side == EnumAppSide.Client)
                {
                    capi.Network.GetChannel(VerticalityModSystem.crawlNetChannel)
                        .SendPacket<IsCrawlingPacket>(new() { isCrawling = value });
                }
            }
        }

        private bool IsClientCrawling;
        private long doubleTapTime;

        public EntityBehaviorCrawl(Entity entity) : base(entity) { }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            baseProperties = entity.World.GetEntityType(GlobalConstants.EntityPlayerTypeCode);

            if (entity.Api.Side == EnumAppSide.Client)
            {
                capi = entity.Api as ICoreClientAPI;
            }
            else if (entity.Api.Side == EnumAppSide.Server)
            {
                sapi = entity.Api as ICoreServerAPI;
            }
        }

        public override void OnGameTick(float dt)
        {

            if (IsLocalPlayer())
            {
                if (VerticalityModSystem.ClientConfig.holdCrawl)
                {
                    if (CrawlInputPressed())
                    {
                        if (!IsCrawling)
                        {
                            TryCrawl();
                        };
                    }
                    else
                    {
                        if (IsCrawling)
                        {
                            TryStand();
                        }
                    }
                }
                else
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
                }
            }


            //if (!IsCrawling)
            //  if (StandCollisionCheck())
            //    TryCrawl();

            if (IsCrawling != IsClientCrawling)
            {
                if (IsCrawling)
                {
                    entity.Properties.EyeHeight = baseProperties.EyeHeight - 1;
                    entity.Properties.CollisionBoxSize.Y = baseProperties.CollisionBoxSize.Y - 1;

                    if (IsLocalPlayer()) GlobalConstants.BaseJumpForce = 0f; // this shouldn't be necessary, but the stat change doesn't fukken work for some reason >:[

                    IsClientCrawling = true;
                }
                else
                {
                    entity.Properties.EyeHeight = baseProperties.EyeHeight;
                    entity.Properties.CollisionBoxSize.Y = baseProperties.CollisionBoxSize.Y;

                    if (IsLocalPlayer()) GlobalConstants.BaseJumpForce = 8.2f;

                    entity.Stats.Remove("walkspeed", "crawlSpeed");
                    entity.Stats.Remove("jumpHeightMul", "crawlJump");

                    entity.StopAnimation("crawl-idle");
                    entity.StopAnimation("crawl");

                    IsClientCrawling = false;
                }
            }


            if (IsCrawling)
            {
                entity.Stats.Set("walkspeed", "crawlSpeed", VerticalityModSystem.Config.modConfig.crawlSpeedReduction, true);
                entity.Stats.Set("jumpHeightMul", "crawlJump", 0f, true);

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


        private bool IsLocalPlayer()
        {
            if (entity.Api.Side == EnumAppSide.Client)
                if (entity == capi.World.Player.Entity)
                    return true;
            return false;
        }

        private bool CrawlInputPressed()
        {
            if (VerticalityModSystem.ClientConfig.combinationCrawlKeys)
                if (capi.Input.IsHotKeyPressed("climb") && capi.Input.IsHotKeyPressed("sneak"))
                    return true;

            if (VerticalityModSystem.ClientConfig.dedicatedCrawlKey)
                if (capi.Input.IsHotKeyPressed("crawl"))
                    return true;

            if (VerticalityModSystem.ClientConfig.standOnJump)
                if (IsCrawling)
                    if (capi.Input.IsHotKeyPressed("jump") && !VerticalityModSystem.ClientConfig.holdCrawl)
                        return true;

            if (VerticalityModSystem.ClientConfig.doubleTapSneakToCrawl)
                if (!IsCrawling)
                {
                    if (capi.Input.IsHotKeyPressed("sneak"))
                    {
                        if (!sneakPressed)
                        {
                            sneakPressed = true;

                            if (capi.ElapsedMilliseconds < doubleTapTime)
                            {
                                return true;
                            }
                            else
                            {
                                doubleTapTime = capi.ElapsedMilliseconds + VerticalityModSystem.ClientConfig.doubleTapSpeed;
                            }
                        }
                    }
                    else
                    {
                        sneakPressed = false;
                    }
                }
                else if (VerticalityModSystem.ClientConfig.holdCrawl)
                {
                    if (capi.Input.IsHotKeyPressed("sneak"))
                        return true;
                }

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
                IsCrawling = true;

                return true;
            }

            return false;
        }

        public bool TryStand()
        {
            if (IsCrawling)
            {
                if (WouldCollideIfStanding()) return false;

                IsCrawling = false;

                return true;
            }
            return false;
        }

        public bool WouldCollideIfStanding()
        {
            Cuboidf collBox = baseProperties.SpawnCollisionBox.Clone();
            collBox.Y2 -= 0.4f; // adjust to sneak height

            return entity.World.CollisionTester.IsColliding(entity.World.BlockAccessor, collBox, entity.Pos.XYZ, false);
        }
    }
}
