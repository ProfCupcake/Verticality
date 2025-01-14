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
            Vec3d walkVectorRelativeToGrabFace = controls.WalkVector.RotatedCopy(grab.grabPos.Face.HorizontalAngleIndex * -GameMath.PIHALF);
            Vec3d posBefore = grab.grabPos.FullPosition;
            /*
            double slide = 0;
            switch(grab.grabPos.Face.HorizontalAngleIndex)
            {
                case 0:
                case 2:
                    slide = (pos.Z - grab.grabPos.FullPosition.Z)/2;
                    break;
                case 1:
                case 3:
                    slide = (pos.X - grab.grabPos.FullPosition.X)/2;
                    break;
            }
            if (!entity.OnGround)
            {
                if (grab.TrySlide(slide))
                {
                    //pos.Add(grab.grabPos.FullPosition.SubCopy(posBefore).ToVec3f());
                }
            }*/

            controls.IsClimbing = true;

            double relHeightEyes = grab.grabPos.FullPosition.Y - (pos.Y + entity.LocalEyePos.Y);
            double diffV = maxHeight - relHeightEyes;

            Vec3d diffVec = grab.grabPos.FullPosition.SubCopy(pos.XYZ);
            diffVec.Y = 0;
            double diffH = diffVec.Length();
            if (diffH > grabDistance || diffV < 0)
            {
                Grab prospectiveNewGrab = Grab.TryGrab((EntityPlayer)entity);
                if (prospectiveNewGrab != null)
                {
                    climbEB.grab = prospectiveNewGrab;
                    grab = prospectiveNewGrab;
                }
                else
                {
                    if (diffH > grabDistance) pos.Motion.Add(diffVec.Normalize().Scale(diffH - grabDistance).ToVec3f());
                    if (diffV < 0) pos.Y -= diffV;
                }
            }
            pos.Motion.Y = -walkVectorRelativeToGrabFace.X * 1.5f;
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
