﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code
{
    public class Overmind
    {
        public double power;
        public bool hasTakenAction;

        public List<Ability> abilities = new List<Ability>();
        public List<Ability> powers = new List<Ability>();
        public List<God> namesChosen = new List<God>();
        public Map map;
        public Person enthralled;
        public bool victoryAchieved = false;
        public bool hasEnthrallAbilities = false;
        public double panicFromPowerUse;
        public int nStartingHumanSettlements;

        public Overmind(Map map)
        {
            this.map = map;

            hasEnthrallAbilities = true;
            powers.Add(new Ab_Enth_Enthrall());
            powers.Add(new Ab_Over_EnthrallAgent());
            powers.Add(new Ab_Enth_DarkEmpire());
            powers.Add(new Ab_Over_HateTheLight());
            //abilities.Add(new Ab_TestAddShadow());
        }

        public void addDefaultAbilities()
        {
            powers.Add(new Ab_Enth_MiliaryAid());
            powers.Add(new Ab_Enth_TrustingFool());
            powers.Add(new Ab_Enth_Enshadow());
            powers.Add(new Ab_Enth_Apoptosis());
            powers.Add(new Ab_Over_DelayVote());
            powers.Add(new Ab_Over_CancelVote());
            //powers.Add(new Ab_Over_InformationBlackout());
            powers.Add(new Ab_Over_SowDissent());
            powers.Add(new Ab_Over_UncannyGlamour());
            powers.Add(new Ab_Enth_AuraOfLunacy());

            abilities.Add(new Ab_Soc_Vote());
            abilities.Add(new Ab_Soc_ProposeVote());
            abilities.Add(new Ab_Soc_SharedGlory());
            abilities.Add(new Ab_Soc_JoinRebels());
            abilities.Add(new Ab_Soc_JoinLoyalists());
            abilities.Add(new Ab_Soc_ShareEvidence());
            abilities.Add(new Ab_Soc_BoycottVote());
            abilities.Add(new Ab_Soc_Fearmonger());
            abilities.Add(new Ab_Soc_DenounceOther());
            abilities.Add(new Ab_Soc_ProvincialSentiments());
            abilities.Add(new Ab_Soc_SwitchVote());
            abilities.Add(new Ab_Soc_ShareTruth());
            abilities.Add(new Ab_Soc_BoostMilitarism());

            if (map.param.useAwareness == 1)
            {
                powers.Add(new Ab_Over_DisruptAction());
            }
        }

        public double computeWorldPanic(List<ReasonMsg> reasons)
        {
            double panic = 0;
            panic += panicFromPowerUse;
            reasons.Add(new ReasonMsg("Power use", panic*100));

            double shadow = map.data_avrgEnshadowment*map.param.panic_panicAtFullShadow;
            panic += shadow;
            reasons.Add(new ReasonMsg("World Shadow", shadow*100));

            double nHumans = map.data_nSocietyLocations;
            double extinction = (nStartingHumanSettlements - nHumans)/nStartingHumanSettlements;
            extinction *= map.param.panic_panicAtFullExtinction;
            if (extinction < 0) { extinction = 0; }//In the off chance they reclaim something
            panic += extinction;
            reasons.Add(new ReasonMsg("Lost Settlements", extinction*100));

            if (panic > 1) { panic = 1; }
            return panic;
        }
        public void increasePanicFromPower(int cost, Ability ability)
        {
            if (cost == 0) { return; }

            panicFromPowerUse += cost * map.param.panic_panicPerPower;
            if (panicFromPowerUse > 1) { panicFromPowerUse = 1; }

            List<Person> allPeople = new List<Person>();
            foreach (SocialGroup sg in map.socialGroups)
            {
                if (sg is Society)
                {
                    allPeople.AddRange(((Society)sg).people);
                }
            }
            double sumWeighting = 0;
            foreach (Person p in allPeople)
            {
                double pv = p.getAwarenessMult();
                if (p.title_land == null) { continue; }
                if (p.awareness >= 1) { pv = 0; }
                if (p.awareness > 0) { pv *= map.param.awarenessInvestigationDetectMult; }
                pv *= pv;
                sumWeighting += pv;
            }
            Person detector = null;
            double roll = Eleven.random.NextDouble() * sumWeighting;
            foreach (Person p in allPeople)
            {
                double pv = p.getAwarenessMult();
                if (p.title_land == null) { continue; }
                if (p.awareness >= 1) { pv = 0; }
                if (p.awareness > 0) { pv *= map.param.awarenessInvestigationDetectMult; }
                pv *= pv;
                roll -= pv;
                if (roll <= 0)
                {
                    detector = p;
                    break;
                }
            }

            if (detector != null) {
                double gain = cost * map.param.awareness_increasePerCost * map.param.awareness_master_speed;
                gain *= detector.getAwarenessMult();
                detector.awareness += gain;
                if (detector.awareness > 1) { detector.awareness = 1; }
                map.turnMessages.Add(new MsgEvent(detector.getFullName() + " has noticed a sign of dark power. Gains " + (int)(100 * gain) + " awareness", MsgEvent.LEVEL_RED, false));
             }

            map.worldPanic = this.computeWorldPanic(new List<ReasonMsg>());
        }

        public void turnTick()
        {
            hasTakenAction = false;
            power += map.param.overmind_powerRegen;
            power = Math.Min(power, map.param.overmind_maxPower);


            panicFromPowerUse -= map.param.panic_dropPerTurn;
            if (panicFromPowerUse < 0) { panicFromPowerUse = 0; }

            processEnthralled();
            int count = 0;
            double sum = 0;
            int nHumanSettlements = 0;
            foreach (Location loc in map.locations)
            {
                if (loc.person() != null) { sum += loc.person().shadow;count += 1; }
                if (loc.soc != null && loc.settlement != null && (loc.settlement is Set_Ruins == false) && (loc.settlement is Set_CityRuins == false) && loc.soc is Society)
                {
                    nHumanSettlements += 1;
                }
            }
            if (count == 0) { map.data_avrgEnshadowment = 0; }
            else { map.data_avrgEnshadowment = sum / count; }
            if ((!victoryAchieved) && map.data_avrgEnshadowment > map.param.victory_targetEnshadowmentAvrg)
            {
                victory();
            }
            if (nHumanSettlements == 0)
            {
                victory();
            }
            map.data_nSocietyLocations = nHumanSettlements;
        }

        public void startedComplete()
        {
            foreach (Location loc in map.locations)
            {
                if (loc.soc is Society && loc.settlement != null)
                {
                    nStartingHumanSettlements += 1;
                }
            }
        }

        public void victory()
        {
            victoryAchieved = true;
            World.log("VICTORY DETECTED");
            map.world.prefabStore.popVictoryBox();
        }

        public void processEnthralled()
        {
            if (enthralled == null) { return; }

            if (enthralled.isDead) { enthralled = null; }
        }
        public int countAvailableAbilities(Hex hex)
        {
            if (hex == null) { return 0; }
            if (hex.location == null) { return 0; }
            int n = 0;
            foreach (Ability a in abilities)
            {
                if (a.castable(map, hex))
                {
                    n += 1;
                }
            }
            return n;
        }
        public int countAvailablePowers(Hex hex)
        {
            if (hex == null) { return 0; }
            if (hex.location == null) { return 0; }
            int n = 0;
            foreach (Ability a in powers)
            {
                if (a.castable(map, hex))
                {
                    n += 1;
                }
            }
            return n;
        }
        public List<Ability> getAvailableAbilities(Hex hex)
        {
            if (hex == null) { return new List<Ability>(); }
            if (hex.location == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            foreach (Ability a in abilities)
            {
                if (a.castable(map, hex))
                {
                    reply.Add(a);
                }
            }
            return reply;
        }
        public List<Ability> getAvailablePowers(Hex hex)
        {
            if (hex == null) { return new List<Ability>(); }
            if (hex.location == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            foreach (Ability a in powers)
            {
                if (a.castable(map, hex))
                {
                    reply.Add(a);
                }
            }
            return reply;
        }
        public int countAvailableAbilities(Person p)
        {
            if (p == null) { return 0; }
            int n = 0;
            foreach (Ability a in abilities)
            {
                if (a.castable(map, p))
                {
                    n += 1;
                }
            }
            return n;
        }
        public int countAvailablePowers(Person p)
        {
            if (p == null) { return 0; }
            int n = 0;
            foreach (Ability a in powers)
            {
                if (a.castable(map, p))
                {
                    n += 1;
                }
            }
            return n;
        }
        public List<Ability> getAvailableAbilities(Person p)
        {
            if (p == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            foreach (Ability a in abilities)
            {
                if (a.castable(map, p))
                {
                    reply.Add(a);
                }
            }
            return reply;
        }
        public List<Ability> getAvailablePowers(Person p)
        {
            if (p == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            foreach (Ability a in powers)
            {
                if (a.castable(map, p))
                {
                    reply.Add(a);
                }
            }
            return reply;
        }
        public int countAvailableAbilities(Unit p)
        {
            if (p == null) { return 0; }
            int n = 0;

            if (p.isEnthralled())
            {
                foreach (Ability a in p.abilities)
                {
                    if (a.castable(map, p))
                    {
                        n += 1;
                    }
                }
                return n;
            }

            foreach (Ability a in abilities)
            {
                if (a.castable(map, p))
                {
                    n += 1;
                }
            }
            return n;
        }
        public int countAvailablePowers(Unit p)
        {
            if (p == null) { return 0; }
            int n = 0;

            if (p.isEnthralled())
            {
                foreach (Ability a in p.powers)
                {
                    if (a.castable(map, p))
                    {
                        n += 1;
                    }
                }
                return n;
            }

            foreach (Ability a in powers)
            {
                if (a.castable(map, p))
                {
                    n += 1;
                }
            }
            return n;
        }
        public List<Ability> getAvailableAbilities(Unit p)
        {
            if (p == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            if (p.isEnthralled())
            {
                foreach (Ability a in p.abilities)
                {
                    if (a.castable(map, p))
                    {
                        reply.Add(a);
                    }
                }
            }
            else
            {
                foreach (Ability a in abilities)
                {
                    if (a.castable(map, p))
                    {
                        reply.Add(a);
                    }
                }
            }
            return reply;
        }
        public List<Ability> getAvailablePowers(Unit p)
        {
            if (p == null) { return new List<Ability>(); }
            List<Ability> reply = new List<Ability>();
            if (p.isEnthralled())
            {
                foreach (Ability a in p.powers)
                {
                    if (a.castable(map, p))
                    {
                        reply.Add(a);
                    }
                }
            }
            else
            {
                foreach (Ability a in powers)
                {
                    if (a.castable(map, p))
                    {
                        reply.Add(a);
                    }
                }
            }
            return reply;
        }
    }
}
