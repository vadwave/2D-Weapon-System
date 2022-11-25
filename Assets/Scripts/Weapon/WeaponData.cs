using System;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public enum ShotType
    {
        None,
        Projectile,
        HitScan,
        Melee,
        Target,
    }
    [Serializable]
    public enum CapacityType
    {
        Numeric,
        Percent,
    }
    [Serializable]
    public enum ChargeType
    {
        None,
        Standart,
        Full,
        Capacity
    }
    [Serializable]
    public enum FireMode
    {
        None,
        SemiAuto,
        FullAuto,
        Burst,
        Charge,
    }
    [Serializable]
    public enum ReloadMode
    {
        None,
        ChangeMagazine,
        BulletByBullet,
    }
    [Serializable]
    public enum AutoReload
    {
        None,
        Trigger,
        AfterShot
    }
    [Serializable]
    public enum WeaponState
    {
        Idle,
        Reload,
        Attack,
        Overheat,
        Charge,
        Modify, // Modify Weapon in Game
        Switch, // Switch Fire Mode or Weapon Mode
    }
    public interface IDamageable
    {
        bool IsAlive
        {
            get;
        }

        void Damage(float damage);

        void Damage(float damage, Vector3 targetPosition, Vector3 hitPosition);
    }
    public class WeaponRealData
    {
        public WeaponData Origin => origin;
        public AmmoData Ammo => ammo;

        public float Damage => ammo.Damage;
        public int BulletsPerShoot => origin.BulletsPerShoot;
        public int BulletsPerBurst => origin.BulletsPerBurst;
        public float FireRate => oneMinute / origin.FireRate;
        public float Range => origin.Range;

        public float Spread => origin.Spread;
        public float Accuracy => origin.Accuracy;
        public float AIMAccuracy => origin.AimAccuracy;
        public float HIPAccuracy => origin.HipAccuracy;
        public float DecreaseRateByShooting => origin.InaccuracyInShooting;
        public float DecreaseRateByWalking => origin.InaccuracyInMoving;

        public float ReloadTime => origin.ReloadTime;
        public float Capacity => origin.Capacity;
        public float Consume => origin.Consume;

        public float CapacityHeat => origin.CapacityHeat;
        public float OverheatTime => origin.OverheatTime;

        public float ChargeCapacity => origin.ChargeCapacity;
        public float ChargeTime => origin.ChargeTime;


        const float oneMinute = 60.0f;

        WeaponData origin;
        AmmoData ammo;
        public WeaponRealData(WeaponData data)
        {
            origin = data;
            ammo = data.Ammo;
        }

        public float GetStateAccuracy(bool isShooting, bool isAiming, bool isIdling)
        {
            if (isAiming)
            {
                if (isShooting) return HIPAccuracy;
                else return isIdling ? AIMAccuracy : HIPAccuracy;
            }
            else if (isShooting) return Accuracy;
            else return isIdling ? HIPAccuracy : Accuracy;
        }

        internal float GetDecreaseRate(bool isShooting)
        {
            return isShooting ? DecreaseRateByShooting : DecreaseRateByWalking;
        }

        internal float GetNextAttack(FireMode fireMode)
        {
            switch (fireMode)
            {
                case FireMode.None: return Time.time + 0.25f;
                case FireMode.Burst: return Time.time + FireRate * (BulletsPerBurst + 1);
                case FireMode.Charge: return Time.time + FireRate * (BulletsPerBurst + 1);
                default: return Time.time + FireRate;
            }
        }
        public float CalculateImpactForce(float distance)
        {
            float minForce = Ammo.MinDistanceForce(distance); 
            return Mathf.Min(minForce, Ammo.ImpactForce);
        }
        public float CalculateDistanceDamage(float distance)
        {
            switch (Ammo.DamageMode)
            {
                case DamageMode.None:
                    return 0;
                case DamageMode.Constant:
                    return Damage;
                case DamageMode.DecreaseByDistance:
                    return Damage * Ammo.DamageFalloffCurve.Evaluate(distance / Range);
                default:
                    return Damage;
            }
        }
    }

    [CreateAssetMenu(menuName = "Weapon System/Weapon", fileName = "Weapon Data", order = 201)]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private new string name;
        [SerializeField] private string desc;
        [SerializeField] private Sprite icon;

        [Header("Shoot")]
        [SerializeField] private ShotType shotType;
        [SerializeField] private FireMode fireMode;
        [SerializeField, Range(1, 100)] private int bulletsPerShoot = 1;
        [SerializeField, Range(1, 100)] private int bulletsPerBurst = 1;
        [SerializeField, Range(1, 1500)] private int fireRate;

        [Header("Accuracy")]
        [SerializeField] private float range;
        [SerializeField] private float spread;
        [SerializeField] private float recoilForce;
        [SerializeField, Range(0.01f, 1f)] private float accuracy;
        [SerializeField, Range(0.00f, 3f)] private float inaccuracyInShooting;
        [SerializeField, Range(0.00f, 3f)] private float inaccuracyInMoving;
        [Header("Hip")]
        [SerializeField, Range(0.01f, 1f)] private float hipAccuracy;
        [Header("Aim")]
        [SerializeField, Range(0.01f, 1f)] private float aimAccuracy;

        [Header("Capacity")]
        [SerializeField] private AmmoData ammo;
        [SerializeField] private CapacityType capacityType;
        [SerializeField] private float capacity;
        [SerializeField] private float consume;
        [Header("Reload")]
        [SerializeField] private ReloadMode reloadMode;
        [SerializeField] private float reloadTime;
        [SerializeField] private AutoReload autoReload;
        [Header("Overheating")]
        [SerializeField] private float capacityHeat;
        [SerializeField] private float overheatTime;
        [Header("Charging")]
        [SerializeField] private ChargeType chargeType;
        [SerializeField] private float chargeCapacity;
        [SerializeField] private float chargeTime;
        [SerializeField] private bool shotAfterFullCharge;
        [Header("Visual")]
        [SerializeField] private WeaponModel model;

        public string Name { get => name; set => name = value; }
        public string Desc { get => desc; set => desc = value; }
        public Sprite Icon { get => icon; set => icon = value; }
        public ShotType ShotType { get => shotType; set => shotType = value; }
        public FireMode FireMode { get => fireMode; set => fireMode = value; }
        public int BulletsPerShoot { get => bulletsPerShoot; set => bulletsPerShoot = value; }
        public int BulletsPerBurst { get => bulletsPerBurst; set => bulletsPerBurst = value; }
        public int FireRate { get => fireRate; set => fireRate = value; }
        public float Range { get => range; set => range = value; }
        public float Spread { get => spread; set => spread = value; }
        public float RecoilForce { get => recoilForce; set => recoilForce = value; }
        public float Accuracy { get => accuracy; set => accuracy = value; }
        public float InaccuracyInShooting { get => inaccuracyInShooting; set => inaccuracyInShooting = value; }
        public float InaccuracyInMoving { get => inaccuracyInMoving; set => inaccuracyInMoving = value; }
        public float HipAccuracy { get => hipAccuracy; set => hipAccuracy = value; }
        public float AimAccuracy { get => aimAccuracy; set => aimAccuracy = value; }
        public AmmoData Ammo { get => ammo; set => ammo = value; }
        public CapacityType CapacityType { get => capacityType; set => capacityType = value; }
        public float Capacity { get => capacity; set => capacity = value; }
        public float Consume { get => consume; set => consume = value; }
        public ReloadMode ReloadMode { get => reloadMode; set => reloadMode = value; }
        public float ReloadTime { get => reloadTime; set => reloadTime = value; }
        public AutoReload AutoReload { get => autoReload; set => autoReload = value; }
        public float OverheatTime { get => overheatTime; set => overheatTime = value; }
        public float CapacityHeat { get => capacityHeat; set => capacityHeat = value; }
        public ChargeType ChargeType { get => chargeType; set => chargeType = value; }
        public float ChargeTime { get => chargeTime; set => chargeTime = value; }
        public bool ShotAfterFullCharge { get => shotAfterFullCharge; set => shotAfterFullCharge = value; }
        public float ChargeCapacity { get => chargeCapacity; set => chargeCapacity = value; }
        public WeaponModel Model { get => model; set => model = value; }
    }
}