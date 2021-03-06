﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code
{
    public class Trait_Aware : Trait
    {
        public Trait_Aware()
        {
            name = "Aware";

            desc = "This character is aware of the darkness, and will seek to find evidence of it in others. This character gains suspicion faster, and gain awareness (if enabled) at a higher rate.";
            groupCode = Trait.CODE_SUSPICION;
        }

        public override double suspicionMult()
        {
            return World.staticMap.param.trait_aware;
        }

        public override double getAwarenessMult()
        {
            return World.staticMap.param.trait_aware_awarenessMult;
        }
    }
}
