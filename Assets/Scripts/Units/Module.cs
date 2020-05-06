using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class Module: MonoBehaviour
    {
        public Unit selfUnit {get; protected set;}

        protected void Awake()
        {
            selfUnit = GetComponent<Unit>();
            selfUnit.RegisterModule(this);

            AwakeAction();
        }

        protected virtual void AwakeAction() { }
    }
}