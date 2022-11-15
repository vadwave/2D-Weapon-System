using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    public class MeleeWeapon : Weapon
    {
        public List<Collider2D> Targets => targets;

        [SerializeField] DamageArea damageArea;

        List<Collider2D> targets = new List<Collider2D>();
        
        public override void Initialize(WeaponData data)
        {
            base.Initialize(data);
            damageArea.Init(this);
        }
        public override void Attack()
        {
            Debug.DrawRay(Model.ShootPoint.position, this.transform.right * data.Range, Color.green, 1f);
            base.Attack();
            Damage();
            Model.Animator.Shot(EmptyMagazine);
        }
        void Damage()
        {
            foreach (var hit in targets)
            {               
                Vector2 direction = Model.ShootPoint.position - hit.transform.position;
                float distance = direction.magnitude;
                Vector2 hitPosition = hit.ClosestPoint(this.transform.position);
                if (LayerInMask(collidedMask, hit.gameObject.layer))
                    Damage(hit, distance, hitPosition);
                ImpactForce(direction, distance, hit.attachedRigidbody);
                Impact(hitPosition);
            }
        }
        void AreaState(bool value)
        {
            if (damageArea)
            {
                damageArea.Enabled(value);
            }
        }
    }
}
