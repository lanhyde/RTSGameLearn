using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class CarryModule : Module
    {
        public delegate void OnCarryStateChanged(bool isCarried);
        public event OnCarryStateChanged onCarryStateCHanged;
        public readonly List<Unit> carryingUnits = new List<Unit>();
        readonly List<Unit> unitsToTake = new List<Unit>();
        readonly List<Vector3> randomedOffsets = new List<Vector3>();

        protected override void AwakeAction()
        {
            if(!GetComponent<Abilities.CarryOut>())
            {
                gameObject.AddComponent<Abilities.CarryOut>();
            }
        }

        void Start()
        {
            for(var i = 0; i < selfUnit.data.canCarryUnitsCount; ++i)
            {
                var randomedX = Mathf.Sin(Random.Range(-1f, 1f) * Mathf.PI);
                var randomedZ = Mathf.Cos(Random.Range(-1f, 1f) * Mathf.PI);

                randomedOffsets.Add(new Vector3(randomedX, 0, randomedZ));
            }
        }

        void Update()
        {
            for(var i = 0; i < carryingUnits.Count; ++i)
            {
                carryingUnits[i].transform.position = transform.position + randomedOffsets[i];
            }
            for(var i = unitsToTake.Count - 1; i >= 0; --i)
            {
                if(!CanCarryOneMoreUnit())
                {
                    unitsToTake[i].EndCurrentOrder();
                    unitsToTake.RemoveAt(i);
                    continue;
                }

                if(Vector3.Distance(unitsToTake[i].transform.position, transform.position) < 1.75f)
                {
                    CarryUnit(unitsToTake[i]);
                    unitsToTake.RemoveAt(i);
                }
            }
        }

        public bool CanCarryOneMoreUnit()
        {
            return selfUnit.data.canCarryUnitsCount > carryingUnits.Count;
        }

        public int CanCarryCount() => Mathf.Clamp(selfUnit.data.canCarryUnitsCount - carryingUnits.Count, 0, selfUnit.data.canCarryUnitsCount);
        public void PrepareToCarryUnits(List<Unit> units) => units.ForEach(unit => PrepareToCarryUnit(unit));
        public void PrepareToCarryUnit(Unit unit)
        {
            if(!unit || !unit.IsInMyTeam(selfUnit) || !unit.data.canBeCarried)
            {
                return;
            }

            var order = new FollowOrder
            {
                followTarget = selfUnit.transform
            };

            unit.AddOrder(order, false);

            if(!unitsToTake.Contains(unit))
            {
                unitsToTake.Add(unit);
            }
        }

        public void CarryUnit(Unit unit)
        {
            if(!CanCarryOneMoreUnit() || unit.isBeingCarried)
            {
                return;
            }
            SetUnitCarryState(unit, true);
            carryingUnits.Add(unit);

            UI.UIController.instance.carryingUnitList.Redraw();
        }

        public void ExitUnit(Unit unit)
        {
            SetUnitCarryState(unit, false);
            var randomedX = Mathf.Sin(Random.Range(-1, 1) * Mathf.PI);
            var randomedZ = Mathf.Cos(Random.Range(-1, 1) * Mathf.PI);

            unit.transform.position = transform.position + new Vector3(randomedX, 0, randomedZ);

            var order = new MovePositionOrder();
            order.movePosition = unit.transform.position + new Vector3(randomedX, 0, randomedZ) * 2f;
            unit.AddOrder(order, false, false);

            carryingUnits.Remove(unit);
        }

        void SetUnitCarryState(Unit unit, bool isCarried)
        {
            if(unit.data.hasMoveModule && isCarried)
            {
                unit.GetModule<Movable>().Stop();
            }
            unit.SetCarryState(isCarried);
            onCarryStateCHanged?.Invoke(isCarried);
        }

        public void ExitAllUnits(bool dieExit = false)
        {
            for(var i = carryingUnits.Count - 1; i >= 0; --i)
            {
                if(dieExit && carryingUnits[i].GetModule<Damageable>())
                {
                    carryingUnits[i].GetModule<Damageable>().TakeDamage(carryingUnits[i].data.maxHealth / 2);
                }
                ExitUnit(carryingUnits[i]);
            }
            UI.UIController.instance.carryingUnitList.Redraw();
        }
    }

}
