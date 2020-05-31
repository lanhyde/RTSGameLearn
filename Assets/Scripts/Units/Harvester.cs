using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class Harvester : Module
    {
        const float randomFieldDistance = 2f;
        const float sqrRandomFieldDistance = 24f;

        public enum HarvestState
        {
            MoveToField,
            Harvest,
            MoveToRefinery,
            CarryOutResources,
            Idle,
        }

        public event HarvesterResourcesChanged harvesterResourcesChangedEvent;

        public int MaxResources => selfUnit.data.harvestMaxResources;
        public int harvestedResources {get; protected set;}

        HarvestState harvestState;
        Refinery nearestRefinery;
        ResourcesField resourcesField;
        float recheckTimer = 1f;
        float harvestTimeLeft;
        float carryoutTimeLeft;
        int addedToRefinaryResources;
        /// <summary>
        /// Field for temporary colliders store for some physical radius check of harvester.
        /// </summary>
        readonly Collider[] nearestColliders = new Collider[15];
        public delegate void HarvesterResourcesChanged(float newValue, float maxValue);

        void Start()
        {
            selfUnit.unitReceivedOrderEvent += OnUnitReceivedOrder;
            harvesterResourcesChangedEvent?.Invoke(0, MaxResources);
        }

        void Update()
        {
            if(!nearestRefinary)
            {
                SearchNearestRefinery();
                return;
            }
            if(!resourcesField)
            {
                SearchNearestResourcesField();
                return;
            }

            switch(harvestState)
            {
                case HarvestState.MoveToField:
                    CheckMoveToFieldState();
                    break;
                case HarvestState.Harvest:
                    CheckHarvestState();
                    break;
                case HarvestState.MoveToRefinery:
                    CheckMoveToRefinaryState();
                    break;
                case HarvestState.CarryOutResources:
                    CheckCarryOutResources();
                    break;
            }
        }

        void CheckMoveToFieldState()
        {
            if((transform.position - resourcesField.transform.position).sqrMagnitude < sqrRandomFieldDistance)
            {
                SetHarvestState(HarvestState.Harvest);
            }
        }

        void CheckHarvestState()
        {
            harvestTimeLeft -= Time.deltaTime;
            harvestedResources = (int)Mathf.Lerp(0, MaxResources, 1 - harvestTimeLeft / selfUnit.data.harvestTime);
            harvesterResourcesChangedEvent?.Invoke(harvestedResources, MaxResources);
            if(harvestTimeLeft <= 0)
            {
                harvestTimeLeft = 0;
                harvestedResources = MaxResources;
                SetHarvestState(HarvestState.MoveToRefinery);
            }
        }
        void CheckMoveToRefinaryState()
        {
            if((transform.position - nearestRefinary.CarryOutResourcePoint.position).sqrMagnitude < 8)
            {
                // TODO: align to refinery
                SetHarvestState(HarvestState.CarryOutResources);
            }
        }
        void CheckCarryOutResources()
        {
            carryoutTimeLeft -= Time.deltaTime;

            // TODO: using lerp
            if(carryoutTimeLeft <= 0)
            {
                carryoutTimeLeft = 0;
                nearestRefinery.AddResources(harvestedResources);
                harvestedResources = 0;

                harvesterResourcesChangedEvent?.Invoke(harvestedResources, MaxResources);
                SetHarvestState(HarvestState.MoveToField);
            }
        }

        void SearchNearestRefinery()
        {
            if(recheckTimer > 0)
            {
                recheckTimer -= Time.deltaTime;
                return;
            }

            var allRefineries = Refinery.allRefineries;
            allRefineries = allRefineries.FindAll(refinery => refinery.selfUnit.OwnerPlayerId == selfUnit.OwnerPlayerId);

            float distance = float.MaxValue - 1f;
            for(int i = 0; i < allRefineries.Count; ++i)
            {
                float curDistance = (transform.position - allRefineries[i].transform.position).sqrMagnitude;

                if(curDistance < distance)
                {
                    nearestRefinery = allRefineries[i];
                    distance = curDistance;
                }
            }
            recheckTimer = 1f;
        }

        void SearchNearestResourcesField()
        {
            if(recheckTimer > 0)
            {
                recheckTimer -= Time.deltaTime;
                return;
            }

            var allFields = ResourcesField.sceneResourceFields;
            float distance = float.MaxValue - 1f;

            for(int i = 0; i < allFields.Count; ++i)
            {
                float curDistance = (transform.position - allFields[i].transform.position).sqrMagnitude;

                if(curDistance < distance)
                {
                    resourcesField = allFields[i];
                    distance = curDistance;
                }
            }

            if(resourcesField)
            {
                SetHarvestState(HarvestState.MoveToField);
            }
            recheckTimer = 1f;
        }

        public void SetHarvestState(HarvestState newState)
        {
            harvestState = newState;

            switch(harvestState)
            {
                case HarvestState.MoveToField:
                    var order = new MovePositionOrder();
                    order.executor = selfUnit;
                    // TODO: change to proportion of resource field size
                    order.movePosition = resourcesField.transform.position + new Vector3(Random.Range(-randomFieldDistance, randomFieldDistance), 0, Random.Range(-randomFieldDistance, randomFieldDistance));
                    selfUnit.AddOrder(order, false, isReceivedEventNeeded: false);
                    break;
                case HarvestState.Harvest:
                    harvestTimeLeft = selfUnit.data.harvestTime;
                    break;
                case HarvestState.MoveToRefinery:
                    var orderBack = new MovePositionOrder();
                    orderBack.movePosition = nearestRefinery.CarryoutResourcesPoint.position;
                    selfUnit.AddOrder(orderBack, false, isReceivedEventNeeded: false);
                    break;
                case HarvestState.CarryOutResource:
                    carryOutTimeLeft = selfUnit.data.harvestCarryOutTime;
                    addedToRefineryResources = 0;
                    break;
            }
        }

        public void OnUnitReceivedOrder(Unit unit, Order order)
        {
            if(order is MovePositionOrder)
            {
                var position = (order as MovePositionOrder).movePosition;
                var size = Physics.OverlapSphereNonAlloc(position, 7f, nearestColliders);

                for(int i = 0; i < size; ++i)
                {
                    var field = nearestColliders[i].GetComponent<ResourcesField>();

                    if(field)
                    {
                        resourcesField = field;
                        SetHarvestState(HarvestState.MoveToField);
                        return;
                    }
                }
            }
            else if(order is FollowOrder)
            {
                var target = (order as FollowOrder).followTarget;
                var refinery = target.GetComponent<Refinery>();

                if(refinery)
                {
                    SetRefinery(refinery);

                    if(harvestedResources > 0)
                    {
                        SetHarvestState(HarvestState.MoveToRefinery);
                    }
                    return;
                }
            }
            SetHarvestState(HarvestState.Idle);
        }

        public void SetRefinery(Refinery refinery) => nearestRefinery = refinery;
    }

}
