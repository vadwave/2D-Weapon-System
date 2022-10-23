using System;
using UnityEngine;

namespace WeaponSystem
{
    public enum DamageMode
    {
        None,
        Constant,
        DecreaseByDistance,
    }

    [CreateAssetMenu(menuName = "Weapon System/Ammo", fileName = "Ammo Data", order = 202)]
    public class AmmoData : ScriptableObject
    {
        public string Name { get => name; private set => name = value; }
        public string Description { get => desc; private set => desc = value; }
        public Sprite Icon { get => icon; private set => icon = value; }
        public float Speed { get => speed; private set => speed = value; }
        public float Mass { get => mass; private set => mass = value; }
        public int Damage { get => damage; private set => damage = value; }
        public DamageMode DamageMode { get => damageMode; private set => damageMode = value; }
        public AnimationCurve DamageFalloffCurve { get => damageFalloffCurve; private set => damageFalloffCurve = value; }
        public float ImpactForce { get => impactForce; private set => impactForce = value; }
        public Projectile PrefabProjectile => projectile;

        [SerializeField] private new string name;
        [SerializeField] private string desc;
        [SerializeField] private Sprite icon;

        [SerializeField] private float speed;
        [SerializeField] private AnimationCurve speedFalloffCurve;

        [SerializeField] private float mass;

        [SerializeField] private float radius;
        [SerializeField] private AnimationCurve radiusFalloffCurve;

        [SerializeField] private int damage;
        [SerializeField] private DamageMode damageMode;
        [SerializeField] private AnimationCurve damageFalloffCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.4f, 1), new Keyframe(0.6f, 0.5f), new Keyframe(1, 0.5f));
        
        [SerializeField] private float impactForce;

        [Header("Visual")]
        [SerializeField] private Projectile projectile;

        internal float MinDistanceForce(float distance)
        {
            return 0.5f * mass * speed * speed / distance;
        }

        //Surface Option
        //Ability Option


    }
}
