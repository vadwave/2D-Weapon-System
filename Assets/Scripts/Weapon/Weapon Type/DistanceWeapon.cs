using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;
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
                shotDir = CalculateBulletSpread();
                base.Attack();

                for (int i = 0; i < data.BulletsPerShoot; i++)
                {
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
            for(int x = 0; x < data.BulletsPerBurst; x++)
            {
                if (EmptyMagazine) break;
                shotDir = CalculateBulletSpread();
                CurrentRounds -= data.Consume;

                for (int i = 0; i < data.BulletsPerShoot; i++)
                {
                    Shot();
                }
                SetAttackTime();
                Model.Animator.Shot(EmptyMagazine);
                Muzzle();
                DropShell();
                Recoil();

                yield return new WaitForSeconds(data.FireRate);
            }

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
        public float CalculateDistanceDamage(float distance)
        {
            return data.CalculateDistanceDamage(distance);
        }
        public void Damage(RaycastHit2D hit, float distance)
        {
            Vector2 hitPosition = hit.point;
            float damage = CalculateDistanceDamage(distance);
            IDamageable damageableTarget = hit.collider.GetComponent<IDamageable>();
            damageableTarget?.Damage(damage, transform.root.position, hitPosition);
        }
        public void Damage(Collider2D collision, float distance, Vector2 hitPosition)
        {
            float damage = CalculateDistanceDamage(distance);
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();
            damageableTarget?.Damage(damage, transform.root.position, hitPosition);
        }
        public bool CheckIsOtherCollision(RaycastHit2D hit)
        {
            //if (hit.transform.root == transform.root) return false;
            if (!LayerInMask(collidedMask, hit.collider.gameObject.layer)) return false;
            return true;
        }
        public void ImpactForce(Vector2 direction, float distance, Rigidbody2D rigidBody)
        {
            if (rigidBody)
            {
                float impactForce = data.CalculateImpactForce(distance);
                Vector2 force = direction * impactForce;
                rigidBody.AddForce(force, ForceMode2D.Impulse);
            }
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
                return this.transform.right;
            }
            else
            {
                Vector2 randomPointInScreen = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * ((1 - currentAccuracy) * (data.Spread / 10));
                return this.transform.right + new Vector3(0, randomPointInScreen.y, 0);
            }
        }
        protected void Tracer(float duration)
        {

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
