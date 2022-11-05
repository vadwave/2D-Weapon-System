using UnityEngine;

namespace WeaponSystem
{
    public class Projectile : MonoBehaviour
    {
        public AmmoModel Model => model;
        protected virtual Vector3 HitPoint => model.HitPoint.position;

        [SerializeField] protected AmmoModel model;
        [SerializeField] protected Rigidbody2D rigBody2D;

        protected LayerMask collidedMask;
        protected DistanceWeapon weapon;
        protected Vector3 moveDir;
        protected float speed;
        protected Vector2 direction;
        private float acceleration = 1;
        private Vector3 movement;
        private GameObject tracer;

        protected virtual void FixedUpdate()
        {
            Movement(Time.fixedDeltaTime);
        }

        protected virtual void Movement(float delta)
        {
            movement = direction * (speed / 10.0f) * delta;
            if (rigBody2D != null)
            {
                rigBody2D.MovePosition(this.transform.position + movement);
            }
            // We apply the acceleration to increase the speed
            speed += acceleration * delta;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            Collision(collision);
        }
        protected virtual void OnTriggerStay2D(Collider2D collision)
        {

        }
        protected virtual void OnTriggerExit2D(Collider2D collision)
        {

        }
        protected virtual void Collision(Collider2D collision)
        {
            if (weapon.LayerInMask(collision.gameObject.layer))
            {
                float distance = Vector2.Distance(weapon.Model.ShootPoint.position, HitPoint);
                weapon.Damage(collision, distance, HitPoint);
                weapon.Impact(HitPoint);
                SelfDamage();
            }
        }
        private void SelfDamage()
        {
            Destroy(this.gameObject);
        }
        protected virtual void Explode()
        {
            weapon.Impact(HitPoint);
            var colliders = Physics2D.OverlapCircleAll(transform.position, weapon.Data.Ammo.Radius, collidedMask);
            float distance = Vector2.Distance(weapon.Model.ShootPoint.position, HitPoint);
            foreach (var collider in colliders)
            {
                weapon.Damage(collider, distance, HitPoint);
            }
            SelfDamage();
        }
        internal virtual void Init(DistanceWeapon weapon, Vector3 direction)
        {
            this.weapon = weapon;
            this.collidedMask = weapon.CollidedMask;
            moveDir = direction;
            speed = weapon.Data.Ammo.Speed;
            this.direction = direction;
            Activate();
        }
        protected virtual void Activate()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            tracer = Instantiate(this.weapon.Data.Ammo.PrefabTracer, model.TracerPoint);
        }
    }
}