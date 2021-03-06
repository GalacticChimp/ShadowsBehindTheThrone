﻿using System.Collections.Generic;
using UnityEngine;


namespace Assets.Code
{
    public abstract class Ability
    {
        public abstract string getName();
        public abstract string getDesc();
        public abstract int getCost();
        public abstract bool castable(Map map, Hex hex);
        public abstract Sprite getSprite(Map map);
        public virtual string specialCost() { return null; }
        public virtual int getCooldown() { return 0; }
        public int turnLastCast;
        public virtual bool castable(Map map,Person person) { return false; }
        public virtual bool castable(Map map,Unit unit) { return false; }

        public virtual void cast(Map map, Hex hex)
        {
            map.overmind.power -= getCost();
            if (map.param.overmind_singleAbilityPerTurn) { map.overmind.hasTakenAction = true; }
            World.log("Cast " + this.ToString() + " " + this.getName());
            turnLastCast = map.turn;

            if (map.param.useAwareness == 1)
            {
                map.overmind.increasePanicFromPower(getCost(), this);
            }
        }

        public void cast(Map map,Person person)
        {
            map.overmind.power -= getCost();
            if (map.param.overmind_singleAbilityPerTurn) { map.overmind.hasTakenAction = true; }
            World.log("Cast " + this.ToString() + " " + this.getName());
            turnLastCast = map.turn;
            castInner(map, person);

            if (map.param.useAwareness == 1)
            {
                map.overmind.increasePanicFromPower(getCost(), this);
            }
        }
        public virtual void castInner(Map map, Person person)
        {

        }
        public void cast(Map map, Unit unit)
        {
            map.overmind.power -= getCost();
            if (map.param.overmind_singleAbilityPerTurn) { map.overmind.hasTakenAction = true; }
            World.log("Cast " + this.ToString() + " " + this.getName());
            turnLastCast = map.turn;
            castInner(map, unit);

            if (map.param.useAwareness == 1)
            {
                map.overmind.increasePanicFromPower(getCost(), this);
            }
        }
        public virtual void castInner(Map map, Unit u)
        {

        }


        public virtual void playSound(AudioStore audioStore) {
            audioStore.playActivate();
        }
    }
}