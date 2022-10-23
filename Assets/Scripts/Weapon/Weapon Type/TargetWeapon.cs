using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    public class TargetWeapon : DistanceWeapon
    {
        public override void Shot()
        {
            base.Shot();
            Vector3 direction = shotDir;
            Vector3 origin = Model.ShootPoint.position;
            Vector3 position = origin + (shotDir * Data.Range);
            Projectile projectile = SpawnProjectile(position);
            projectile.Init(this, direction);
        }
        Projectile SpawnProjectile(Vector3 position)
        {
            return Instantiate(data.Ammo.PrefabProjectile, position, Quaternion.identity);
        }
    }
}
