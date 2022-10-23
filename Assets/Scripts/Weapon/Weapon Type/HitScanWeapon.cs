using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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
            float tracerDuration = 0;//Tracer Duration
            if (hit)
            {
                float distance = hit.distance;
                //Ricochet or Fragmentate or Penetrate Calculate
                if(CheckIsOtherCollision(hit)) 
                    Damage(hit, distance);
                ImpactForce(direction, distance, hit.collider.attachedRigidbody);
                tracerDuration = distance / 1;//Tracer Speed
            }
            if (tracerDuration > TimeTracerDuration)
                Tracer(tracerDuration);
        }
    }
}
