using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponSystem;

namespace WeaponSystem
{
    public class Projectile : MonoBehaviour
    {
        public AmmoModel Model => model;

        [SerializeField] private AmmoModel model;

        private DistanceWeapon weapon;
        private Vector3 moveDir;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Attack(collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {

        }
        private void OnTriggerExit2D(Collider2D collision)
        {

        }
        void Attack(Collider2D collision)
        {
            if (!weapon.LayerInMask(collision.gameObject.layer))
            {
                float distance = 4f;
                weapon.Damage(collision, distance, model.HitPoint.position);
                SelfDamage();
            }
        }
        private void SelfDamage()
        {
            Destroy(this.gameObject);
        }
        internal void Init(DistanceWeapon projectileWeapon, Vector3 direction)
        {
            weapon = projectileWeapon;
            moveDir = direction;
        }
    }
}