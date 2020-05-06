using UnityEngine;

namespace PromiseCode.RTS.Units
{
    public class Tower : Module
    {
        [SerializeField] Transform turretTransform;
        [SerializeField] Transform secondAxisGun;

        float timerToNextRandom;
        float randomRotationTime;
        int randomRotateDirection;

        Quaternion secondAxisGunDefaultLocalRotation;

        void Start()
        {
            if(!selfUnit.data.hasTurret)
            {
                Debug.LogWarning("[Tower module] Unit " + name + " has disabled Has Turret toggle, but Tower module still added to prefab. Fix this.");
            }
            if(secondAxisGun)
            {
                secondAxisGunDefaultLocalRotation = secondAxisGun.localRotation;
            }
        }

        void Update()
        {
            RotateTower();
        }

        public bool IsTurretAimedToTarget(Collider target)
        {
            Vector3 otherSameToTowerYPosition = target.transform.position;
            otherSameToTowerYPosition.y = turretTransform.position.y;
            var turretForwardNoY = turretTransform.forward;
            turretForwardNoY.y = 0;

            Vector3 toOther = (otherSameToTowerYPosition - turretTransform.position).normalized;
            return Vector3.Angle(turretForwardNoY, toOther) < 3f;
        }

        void RotateTower()
        {

        }
    }
}