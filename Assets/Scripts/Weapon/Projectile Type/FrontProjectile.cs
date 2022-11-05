using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    public class FrontProjectile : Projectile
    {
        protected override Vector3 HitPoint => model.transform.position;

        protected override void FixedUpdate()
        {
            Fall(Time.fixedDeltaTime);
            //base.FixedUpdate();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            //base.OnTriggerEnter2D(collision);
        }
        protected override void Movement(float delta)
        {
            //base.Movement(delta);
        }
        protected virtual void Fall(float delta)
        {
            Vector3 finishScale = new Vector3(0, 0, model.transform.localScale.z);
            float speedFall = 3f;
            float maxDistanceDelta = delta * speedFall;
            model.transform.localScale = Vector3.MoveTowards(model.transform.localScale, finishScale, maxDistanceDelta);
        }
        internal override void Init(DistanceWeapon weapon, Vector3 direction)
        {
            base.Init(weapon, direction);
        }
        protected override void Activate()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            StartCoroutine(IE_WaitActivate());
        }
        IEnumerator IE_WaitActivate()
        {
            yield return new WaitForSeconds(0.1f);
            Explode();
        }
    }
}
