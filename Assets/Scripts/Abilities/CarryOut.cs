using PromiseCode.RTS.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Abilities
{
    public class CarryOut : Ability
    {
        public override void CustomAction()
        {
            if(Selection.selectedUnits.Count == 0)
            {
                return;
            }    
            for(int i = 0; i < Selection.selectedUnits.Count; ++i)
            {
                var carryModule = Selection.selectedUnits[i].GetModule<CarryModule>();
                carryModule?.ExitAllUnits();
            }
        }
    }

}