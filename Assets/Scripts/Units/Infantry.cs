using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class Infantry : Module
    {
        [SerializeField] Animator animator;
        static readonly int attackId = Animator.StringToHash("Attack");
        static readonly int MoveId = Animator.StringToHash("Move");
        static readonly int dieId = Animator.StringToHash("Die");

        void Start()
        {
            if(!animator)
            {
                animator = GetComponent<Animator>();
            }
            if(!animator)
            {
                Debug.LogWarning($"Infantry soldier {name} does not have Animator component! It will have to animations, if you don't attach it.");
                return;
            }
            if(!animator.runtimeAnimatorController && selfUnit.data.animatorController)
            {
                animator.runtimeAnimatorController = selfUnit.data.animatorController;
            }

            selfUnit.GetModule<Attackable>().startAttackEvent += OnStartAttack;
            selfUnit.GetModule<Attackable>().stopAttackEvent += OnStopAttack;
            selfUnit.GetModule<Movable>().startMoveEvent += OnStartMove;
            selfUnit.GetModule<Movable>().stopMoveEvent += OnStopMove;
            selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
        }

        void OnStartAttack()
        {
            if(animator.isActiveAndEnabled)
            {
                animator.SetBool(attackId, true);
            }
        }

        void OnStartMove()
        {
            if(animator.isActiveAndEnabled)
            {
                animator.SetBool(MoveId, true);
            }
        }

        void OnStopMove()
        {
            if(animator.isActiveAndEnabled)
            {
                animator.SetBool(MoveId, false);
            }
        }

        void OnStopAttack()
        {
            if(animator.isActiveAndEnabled)
            {
                animator.SetBool(attackId, false);
            }
        }

        void OnDie(Unit unit)
        {
            animator.transform.SetParent(null);
            var timedRemover = animator.transform.gameObject.AddComponent<TimedObjectDestructor>();
            // remove corpse after 5 seconds
            timedRemover.SetCustomTime(5f);

            if(animator.isActiveAndEnabled)
            {
                animator.SetBool(dieId, true);
            }
        }
    }
}
