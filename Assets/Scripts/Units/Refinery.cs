using UnityEngine;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;

namespace PromiseCode.RTS.Units
{
    public class Refinery : Module
    {
        public static List<Refinery> allRefineries { get; private set;}
        [SerializeField] Transform carryOutResourcesPoint;
        [Tooltip("Harvester unit data which will be spawned on this refinery at start")]
        [SerializeField] UnitData harvesterUnitData;
        object mutexLock = new object();
        public Transform CarryoutResourcesPoint => carryOutResourcesPoint;
        protected override void AwakeAction()
        {
            if(allRefineries == null)
            {
                lock(mutexLock)
                {
                    if(allRefineries == null)
                    {
                        allRefineries = new List<Refinery>();
                    }
                }
            }
            allRefineries.Add(this);
        }

        void Start()
        {
            SpawnHarvester();
        }

        public void AddResources(int amount)
        {
            GameController.instance.playersController.playersInGame[selfUnit.OwnerPlayerId].AddMoney(amount);
        }

        void SpawnHarvester()
        {
            var spawnedHarvester = SpawnController.SpawnUnit(harvesterUnitData, selfUnit.OwnerPlayerId, carryOutResourcesPoint);
            spawnedHarvester.GetComponent<Harvester>().SetRefinery(this);
        }

        void OnDestroy()
        {
            allRefineries.Remove(this);
        }
    }
}