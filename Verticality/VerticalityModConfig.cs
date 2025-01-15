﻿using ProtoBuf;

namespace Verticality
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class VerticalityModConfig
    {
        public float climbMaxHeight = 0.7f;
        public float climbMinHeight = 0.5f;
        public float climbGrabDistance = 0.5f;
        public float climbSpeed = 1.5f;
        public float chargedJumpChargeTime = 0.5f;
        public float chargedJumpAddForce = 1.9f;
    }

    public class VerticalityClientModConfig
    {
        public bool showDebugParticles = false;
    }
}
