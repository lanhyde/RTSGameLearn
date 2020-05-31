using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class ElectricityModule : Module
    {
        int  addsElectricity, neededElectricity;

        protected override void AwakeAction()
        {
            addsElectricity = selfUnit.data.addsElectricity;
            neededElectricity = selfUnit.data.usesElectricity;

            Unit.unitSpawnedEvent += OnBuildingComplete;
        }

        void Start()
        {
            selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
        }

        void OnBuildingComplete(Unit unit)
        {
            if(unit != selfUnit)
            {
                return;
            }
            var player = Player.GetPlayerById(selfUnit.OwnerPlayerId);
            player.AddElectricity(addsElectricity);
            player.AddUsedElectricity(neededElectricity);
        }

        void OnDie(Unit unit)
        {
            if(unit != selfUnit)
            {
                return;
            }

            var player = Player.GetPlayerById(selfUnit.OwnerPlayerId);
            player.RemoveElectricity(addsElectricity);
            player.RemoveUsedElectricity(neededElectricity);
        }

        public void IncreaseAddingElectricity(int addToAdding)
        {
            addsElectricity += addToAdding;
            Player.GetPlayerById(selfUnit.OwnerPlayerId).AddUsedElectricity(addToAdding);
        }

        void OnDestroy()
        {
            Unit.unitSpawnedEvent -= OnBuildingComplete;
        }
    }

}
