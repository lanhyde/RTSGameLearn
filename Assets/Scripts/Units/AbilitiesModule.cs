using UnityEngine;
using System.Collections;
using PromiseCode.RTS.Abilities;
using System.Linq;
using System.Collections.Generic;

namespace PromiseCode.RTS.Units
{
    public class AbilitiesModule : Module
    {
        public List<Ability> abilities { get; protected set; }

        void Start()
        {
            abilities = GetComponents<Ability>().ToList();
        }

        public Ability GetAbility(AbilityData abilityData)
        {
            for(int i = 0; i < abilities.Count; ++i)
            {
                if(abilities[i].Data == abilityData)
                {
                    return abilities[i];
                }
            }
            return null;
        }

        public Ability GetAbilityById(int id)
        {
            if(abilities.Count > id)
            {
                return abilities[id];
            }
            return null;
        }
    }

}
