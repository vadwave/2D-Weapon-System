using System.Collections;
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

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distancePos = Vector2.Distance(origin, mousePos);

            float distance = (distancePos >= Data.Range) ? Data.Range : distancePos;
            float waitTime = distance / data.Ammo.Speed;

            Vector3 position = origin + (shotDir.normalized * distance);
            StartCoroutine(LaunchProjectile(direction, position, waitTime));
        }
        IEnumerator LaunchProjectile(Vector3 direction, Vector3 position,float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            FrontProjectile projectile = (FrontProjectile)SpawnProjectile(position);
            projectile.Init(this, direction);
        }
        Projectile SpawnProjectile(Vector3 position)
        {
            ShotDebug(position);
            return Instantiate(data.Ammo.PrefabProjectile, position, Quaternion.identity);
        }
        void ShotDebug(Vector3 position)
        {
            float size = data.Ammo.Radius;
            float duration = 1f;
            Debug.DrawLine(position, new Vector3(position.x + size, position.y, position.z), Color.yellow, duration);
            Debug.DrawLine(position, new Vector3(position.x - size, position.y, position.z), Color.yellow, duration);
            Debug.DrawLine(position, new Vector3(position.x, position.y + size, position.z), Color.yellow, duration);
            Debug.DrawLine(position, new Vector3(position.x, position.y - size, position.z), Color.yellow, duration);
        }
    }
}
