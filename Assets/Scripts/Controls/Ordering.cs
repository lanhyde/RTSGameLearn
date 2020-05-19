using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PromiseCode.RTS.Controls
{
    public static class Ordering
    {
        public static void GiveOrder(Vector2 screenPosition, bool isAdditive)
        {
            if(EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if(Selection.selectedUnits.Count == 0 || Selection.selectedUnits[0].data.isBuilding)
            {
                return;
            }
            var ray = GameController.cachedMainCamera.ScreenPointToRay(screenPosition);
            
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1000))
            {
                var unit = hit.collider.GetComponent<Unit>();
                Order order = null;

                if(unit)
                {
                    if(!GameController.instance.playersController.IsPlayersInOneTeam(unit.OwnerPlayerId, Player.localPlayerId))
                    {
                        order = new AttackOrder();
                        (order as AttackOrder).attackTarget = unit;
                        SpawnEffect(hit.point, GameController.instance.MainStorage.attackOrderEffect);
                    }
                    else
                    {
                        order = new FollowOrder();
                        (order as FollowOrder).followTarget = unit.transform;
                        SpawnEffect(hit.point, GameController.instance.MainStorage.moveOrderEffect);

                        var carryModule = unit.GetModule<CarryModule>();
                        if(unit.data.canCarryUnitsCount > 0 && carryModule && carryModule.CanCarryOneMoreUnit())
                        {
                            carryModule.PrepareToCarryUnits(Selection.selectedUnits);
                        }
                    }
                }
                else
                {
                    order = new MovePositionOrder();
                    (order as MovePositionOrder).movePosition = hit.point;
                    SpawnEffect(hit.point, GameController.instance.MainStorage.moveOrderEffect);
                }

                SendOrderToSelection(order, isAdditive);
            }
        }

        public static void GiveMapOrder(Vector2 mapPoint)
        {
            if(Selection.selectedUnits.Count == 0 || Selection.selectedUnits[0].data.isBuilding)
            {
                return;
            }

        }
    }
}
