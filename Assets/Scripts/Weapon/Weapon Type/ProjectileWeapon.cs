using UnityEngine;

namespace WeaponSystem
{
    public class ProjectileWeapon : DistanceWeapon
    {
        public override void Shot()
        {
            base.Shot();
            Vector3 direction = shotDir;
            Vector3 origin = Model.ShootPoint.position;
            Projectile projectile = SpawnProjectile(origin);
            projectile.Init(this, direction);
        }
        Projectile SpawnProjectile(Vector3 position)
        {
            return Instantiate(data.Ammo.PrefabProjectile, position, transform.rotation);
        }
    }
}
