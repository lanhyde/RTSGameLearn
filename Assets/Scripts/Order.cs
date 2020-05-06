using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    [System.Serializable]
    public abstract class Order
    {
        public Unit executor;
        public virtual void Execute() { End(); }
        public virtual void End() { executor.EndCurrentOrder(); }
        public abstract Order Clone();
        protected virtual Vector3 GetActualMovePosition() { return Vector3.zero; }
    }

    [System.Serializable]
    public class AttackOrder: Order
    {
        public Unit attackTarget;
        public override void Execute()
        {
            if(!executor.attackable || !attackTarget)
            {
                End();
                return;
            }
            if(executor.movable)
            {
                if(!executor.attackable.IsFireLineFree(attackTarget) || !executor.attackable.IsTargetInAttackRange(attackTarget))
                {
                    executor.movable.MoveToPosition(attackTarget.transform.position);
                }
                else
                {
                    executor.movable.Stop();
                }
            }
            // TODO: extend attackTarget
        }

        public override Order Clone()
        {
            AttackOrder order = new AttackOrder
            {
                attackTarget = attackTarget
            };
            return order;
        }

        protected override Vector3 GetActualMovePosition() => attackTarget.transform.position;
    }

    [System.Serializable]
    public class MovePositionOrder: Order
    {
        public Vector3 movePosition;
        public override void Execute()
        {
            if(!executor || !executor.movable)
            {
                if(!executor.movable)
                {
                    Debug.LogWarning("Movable order given to wrong unit.");
                }
                End();
                return;
            }
            executor.movable.MoveToPosition(movePosition);
            var movePosWithSameY = movePosition;
            movePosWithSameY.y = executor.transform.position.y;

            if((executor.transform.position - movePosWithSameY).sqrMagnitude <= executor.movable.sqrDistanceFineToStop)
            {
                End();
            }
        }

        public override Order Clone()
        {
            MovePositionOrder order = new MovePositionOrder
            {
                movePosition = movePosition
            };
            return order;
        }
        protected override Vector3 GetActualMovePosition() => movePosition;
    }

    [System.Serializable]
    public class FollowOrder: Order
    {
        public Transform followTarget;

        public override void Execute()
        {
            if(!followTarget)
            {
                End();
                return;
            }
            executor.movable.MoveToPosition(followTarget.position);
            // TODO: change it to make set movable target to follow target
        }

        public override Order Clone()
        {
            FollowOrder order = new FollowOrder
            {
                followTarget = followTarget
            };
            return order;
        }

        protected override Vector3 GetActualMovePosition() => followTarget.transform.position;
    }
}