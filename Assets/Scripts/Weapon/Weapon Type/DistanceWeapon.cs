using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaponSystem
{
    public class DistanceWeapon : Weapon
    {
        protected float currentAccuracy;
        protected Vector3 shotDir;
        public bool IsShoot => shootingTime > 0;
        protected float shootingTime;

        private void Update()
        {
            if (IsActive)
            {
                UpdateAttackTime(Time.deltaTime);
                //StabiliseWeaponRecoil();
                //Trigger();
            }
            CalculateCurrentAccuracy(Time.deltaTime);
        }
        public bool LayerInMask(int layer)
        {
            return LayerInMask(collidedMask, layer);
        }
        public override void Attack()
        {
            if (IsCanReloadMagazine)
            {
                EmptyCapacity();
            }
            else
            {
                Debug.DrawRay(Model.ShootPoint.position, this.transform.right * data.Range, Color.green, 1f);
                base.Attack();
                for (int i = 0; i < data.BulletsPerShoot; i++)
                {
                    shotDir = CalculateBulletSpread();
                    Shot();
                }
                SetAttackTime();
                Model.Animator.Shot(EmptyMagazine);
                Muzzle();
                DropShell();
                Recoil();
            }
        }


        public override void Burst()
        {
            base.Burst();
            StartCoroutine(LaunchBurst());
        }
        IEnumerator LaunchBurst()
        {
            int bullets = BulletsPerBurst;
            for (int x = 0; x < bullets; x++)
            {
                if (EmptyMagazine || isOverheating) break;
                CurrentRounds -= data.Consume;
                CurrentHeatLevel += data.Consume;
                if(data.Origin.ChargeType == ChargeType.Capacity) CurrentChargeLevel -= data.Consume;
                for (int i = 0; i < data.BulletsPerShoot; i++)
                {
                    shotDir = CalculateBulletSpread();
                    Shot();
                }
                SetAttackTime();
                Model.Animator.Shot(EmptyMagazine);
                Muzzle();
                DropShell();
                Recoil();

                yield return new WaitForSeconds(data.FireRate);
            }
            CurrentChargeLevel = 0;
        }
        protected void UpdateAttackTime(float delta)
        {
            shootingTime = Mathf.MoveTowards(shootingTime, 0, delta);
        }
        protected void SetAttackTime(float time = 0.1f)
        {
            shootingTime = time;
        }
        protected void ResetAttackTime()
        {
            shootingTime = 0.0f;
        }

        public virtual void Shot()
        {

        }
        private void CalculateCurrentAccuracy(float delta)
        {
            float stateAccuracy = data.GetStateAccuracy(IsShoot, IsAiming, IsIdling);
            float decrease = data.GetDecreaseRate(IsShoot);
            float value = Mathf.MoveTowards(currentAccuracy, stateAccuracy, delta * decrease);
            currentAccuracy = Mathf.Clamp(value, data.Accuracy, data.AIMAccuracy);
        }
        private Vector3 CalculateBulletSpread()
        {
            if (Mathf.Abs(currentAccuracy - 1) < Mathf.Epsilon)
            {
                return Model.ShootPoint.right;
            }
            else
            {
                float valueAcurrace = (1 - currentAccuracy) * (data.Spread / 10);
                Vector2 randomPointInScreen = new Vector2(Random.Range(-1.0f, 1.0f) * valueAcurrace, Random.Range(-1.0f, 1.0f) * valueAcurrace);
                return Model.ShootPoint.right + new Vector3(randomPointInScreen.x, randomPointInScreen.y, 0);
            }
        }
        protected void Tracer(Vector2 direction, float duration)
        {
            Model.CreateTracer(direction, duration);
        }
        protected void Muzzle()
        {
            Model.CreateMuzzle();
        }
        protected void DropShell()
        {

        }
        private void Recoil()
        {

        }

    }
}
