using UnityEngine;

namespace WeaponSystem
{
    public class HitScanWeapon : DistanceWeapon
    {
        const float TimeTracerDuration = 0.05f;
        public override void Shot()
        {
            base.Shot();
            Vector3 direction = shotDir;
            Vector3 origin = Model.ShootPoint.position;
            Ray2D ray = new Ray2D(origin, direction);
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, data.Range, collidedMask);
            Debug.DrawRay(origin, direction * data.Range, Color.yellow, 1f);
            float tracerDuration = data.Ammo.TracerDuration;
            if (hit)
            {
                float distance = hit.distance;
                //Ricochet or Fragmentate or Penetrate Calculate
                if(CheckIsOtherCollision(hit)) 
                    Damage(hit, distance);
                ImpactForce(direction, distance, hit.collider.attachedRigidbody);
                Impact(hit.point);
                tracerDuration = distance / data.Ammo.TracerSpeed;
            }
            if (tracerDuration > TimeTracerDuration)
                Tracer(direction, tracerDuration);
        }
    }
}
