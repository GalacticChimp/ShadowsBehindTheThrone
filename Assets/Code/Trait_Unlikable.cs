﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code
{
    public class Trait_Unlikable : Trait
    {
        public Trait_Unlikable()
        {
            name = "Unlikable";
            receivedLikingDelta = World.staticMap.param.trait_unlikable;
            desc = "This character is unpleasant to talk to, unpleasant to associate with, and suffers relationship penalties with other characters.";
            groupCode = Trait.CODE_LIKABILITY;
        }
        
    }
}
