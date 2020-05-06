using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class Wheels : Module
    {
        public enum Axis { X, Y, Z }

        public enum WheelsType
        {
            ForwardAndSide,
            OnlyForward
        }

        [SerializeField] Transform[] wheels;
        [SerializeField] float rotationSpeed = 0.5f;
        [SerializeField] Axis rotateAroundAxis = Axis.X;
        [SerializeField] bool rotateAlways;

        void Update()
        {
            if(rotateAlways)
            {
                RotateWheelsForward(false, true);
            }
        }

        public void RotateWheelsForward(bool inverse = false, bool fromSelfComponent = false)
        {
            // if rotateAlways is true, wheels will be rotated only when called from this component's Update method
            if(rotateAlways && !fromSelfComponent)
            {
                return;
            }

            float rotationSpeedResult = rotationSpeed * Time.deltaTime * (inverse ? -1 : 1);

            for(int i = 0; i < wheels.Length; ++i)
            {
                wheels[i].Rotate(rotateAroundAxis == Axis.X ? rotationSpeedResult : 0f, rotateAroundAxis == Axis.Y ? rotationSpeedResult : 0f, rotateAroundAxis == Axis.Z ? rotationSpeedResult : 0);
            }
        }

        public void SetupWheels(List<Transform> wheelBones) => wheels = wheelBones.ToArray();
    }
}