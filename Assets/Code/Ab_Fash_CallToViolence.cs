﻿using UnityEngine;

using System;

namespace Assets.Code
{
    public class Ab_Fash_CallToViolence: Ability
    {
        public override void cast(Map map, Hex hex)
        {
            base.cast(map, hex);
            castInner(map, hex.location.person());

        }
        public override void castInner(Map map, Person person)
        {
            person.politics_militarism = 1;

            string msgs = "You exploit the terror of " + person.getFullName() + ", causing them to become violent and cruel, as an attempt to defend themselves.";
            map.world.prefabStore.popImgMsg(
                msgs,
                map.world.wordStore.lookup("ABILITY_CALL_TO_VIOLENCE"));
        }

        public override bool castable(Map map, Person person)
        {
            return person.getGreatestThreat() != null && person.getGreatestThreat().threat >= person.map.param.person_fearLevel_terrified;
        }

        public override bool castable(Map map, Hex hex)
        {
            if (hex.location.person() == null) { return false; }
            return castable(map, hex.location.person());
        }

        public override int getCooldown()
        {
            return World.staticMap.param.ability_callToViolenceCooldown;
        }
        public override int getCost()
        {
            return 0;
        }

        public override string getDesc()
        {
            return "Causes a person to become 100% militaristic if their greatest threat estimation is at the 'terrified' level."
                + "\n[Requires a noble with threat level of 'terrified']";
        }

        public override string getName()
        {
            return "Call to Violence";
        }

        public override Sprite getSprite(Map map)
        {
            return map.world.textureStore.icon_axes;
        }
    }
}