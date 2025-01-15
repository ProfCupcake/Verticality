using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verticality.Lib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Verticality.Moves.Climb
{
    public class PModuleGrab : PModule
    {
        float maxHeight => VerticalityModSystem.Config.modConfig.climbMaxHeight;
        float minHeight => VerticalityModSystem.Config.modConfig.climbMinHeight;
        float grabDistance => VerticalityModSystem.Config.modConfig.climbGrabDistance;
        float climbSpeed => VerticalityModSystem.Config.modConfig.climbSpeed;
        EntityBehaviorClimb climbEB;
        Grab grab;

        public override bool Applicable(Entity entity, EntityPos pos, EntityControls controls)
        {
            if (climbEB != null)
            {
                grab = climbEB.grab;
                return (grab != null) && grab.CanStillGrab();
            }
            return false;
        }

        public override void DoApply(float dt, Entity entity, EntityPos pos, EntityControls controls)
        {
            bool grabYLowered = false;
            Vec3d walkVectorRelativeToGrabFace = controls.WalkVector.RotatedCopy(grab.grabPos.Face.HorizontalAngleIndex * -GameMath.PIHALF);
            
            controls.IsClimbing = true;

            double relHeightEyes = grab.grabPos.FullPosition.Y - (pos.Y + entity.LocalEyePos.Y);
            double diffV = maxHeight - relHeightEyes;

            Vec3d diffVec = grab.grabPos.FullPosition.SubCopy(pos.XYZ);
            diffVec.Y = 0;
            double diffH = diffVec.Length();
            if (diffV < 0)
            {
                Grab prospectiveNewGrab = Grab.TryGrab((EntityPlayer)entity, null, (float?)(grab.grabPos.FullPosition.Y - 1 / 64f), grabDistance + 0.2f);
                if (prospectiveNewGrab != null)
                {
                    climbEB.grab = prospectiveNewGrab;
                    grab = prospectiveNewGrab;
                    grabYLowered = true;
                } else
                {
                    pos.Y -= diffV;
                }
            }
            if (diffH > grabDistance)
            {
                Grab prospectiveNewGrab = Grab.TryGrab((EntityPlayer)entity, null, grabYLowered ? (float?)(grab.grabPos.FullPosition.Y) : null, null);
                if (prospectiveNewGrab != null)
                {
                    climbEB.grab = prospectiveNewGrab;
                    grab = prospectiveNewGrab;
                }
                else
                {
                    if (diffH > grabDistance) pos.Motion.Set(diffVec.Normalize().Scale(diffH - grabDistance).ToVec3f());
                }
            }
            pos.Motion.Y = -walkVectorRelativeToGrabFace.X * climbSpeed;
        }

        public override void Initialize(JsonObject config, Entity entity)
        {
            climbEB = entity.GetBehavior<EntityBehaviorClimb>();
            if (climbEB == null)
            {
                // throw exception?
            }
        }
    }
}
