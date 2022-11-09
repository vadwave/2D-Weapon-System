using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    public class Weapon : MonoBehaviour
    {
        public event Action<float, float, CapacityType> OnChangedCapacity;
        public event Action OnStartReloaded;
        public event Action OnStopReloaded;
        public WeaponModel Model => model;
        public AmmoData Ammo => Origin.Ammo;
        public WeaponData Origin => data.Origin;
        public WeaponRealData Data => data;
        public LayerMask CollidedMask => collidedMask;
        public bool Attacking => nextAttackTime >= Time.time;
        public bool IsIdling { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsActive { get; private set; }
        public bool EmptyMagazine => CurrentRounds == 0;
        public bool FullMagazine => CurrentRounds == data.Capacity;
        public bool IsCanReloadMagazine => EmptyMagazine && !InventoryEmpty;
        public bool InventoryEmpty => InventoryIsEmpty(data.Ammo);
        public bool IsCanAttack => !IsReloading && !Attacking && CurrentRounds >= 0;
        public bool IsCanReload => !IsReloading && nextReloadTime < Time.time;
        public float CurrentRounds { get => currentRounds; 
            protected set 
            { 
                currentRounds = value; 
                OnChangedCapacity?.Invoke(currentRounds, data.Capacity, data.Origin.CapacityType); 
            } 
        }
        public float CurrentHeatLevel { get => currentHeatLevel;
            protected set
            { 
                currentHeatLevel = Mathf.Clamp(value, 0, data.CapacityHeat); 
            } 
        }
        public bool IsReloading { get => isReloading;
            protected set
            {
                if (isReloading != value)
                {
                    isReloading = value;
                    if (isReloading)
                        OnStartReloaded?.Invoke();
                    else
                        OnStopReloaded?.Invoke();
                }
            }
        }

        [SerializeField] protected WeaponData weaponData;

        protected LayerMask collidedMask;
        protected WeaponRealData data;
        protected WeaponModel model;
        protected WeaponState state;

        protected FireMode currentFireMode;
        protected float nextAttackTime;
        protected float nextReloadTime;
        protected float currentRounds;
        private float currentHeatLevel;

        private bool isReloading = false;
        private bool isOverheating = false;

        protected void Awake()
        {
            Initialize(weaponData);
        }
        protected void FixedUpdate()
        {
            Heating(Time.fixedDeltaTime);
        }
        protected bool LayerInMask(LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) != 0);
        }
        public void Initialize(WeaponData data)
        {
            this.data = new WeaponRealData(data);
            this.state = WeaponState.Idle;
            this.model = this.GetComponentInChildren<WeaponModel>();
            if(model == null) CreateModel();
            this.model.Init(this);
            this.currentFireMode = this.data.Origin.FireMode;
            this.CurrentRounds = this.data.Capacity;
        }
        public void Initialize(LayerMask mask)
        {
            collidedMask = mask;
            IsActive = true;
        }
        public void CreateModel()
        {
            this.model = Instantiate(this.data.Origin.Model, this.transform);
        }

        public void Trigger(bool isTriggered = false, bool isHelded = false)
        {
            bool canAttack = IsCanAttack;
            if (canAttack)
            {
                switch (Origin.FireMode)
                {
                    case FireMode.None:
                        break;
                    case FireMode.SemiAuto:
                        {
                            TriggerAttack(isTriggered);
                        }
                        break;
                    case FireMode.FullAuto:
                        {
                            TriggerAttack(isHelded);
                        }
                        break;
                    case FireMode.Burst:
                        {
                            if (isTriggered)
                            {
                                if (IsCanReloadMagazine)
                                {
                                    Reload();
                                }
                                else
                                {
                                    Burst();
                                }
                            }
                        }
                        break;
                    case FireMode.Charge:
                        {
                            TriggerAttack(isHelded);
                        }
                        break;
                }
            }
            bool canReloadBulletByBullet = nextReloadTime < Time.time && IsReloading && data.Origin.ReloadMode == ReloadMode.BulletByBullet && !EmptyMagazine;
            if (canReloadBulletByBullet)
            {
                if (isTriggered)
                {
                    StopReload();
                }
            }
            bool canReload = IsCanReload && !Attacking && IsCanReloadMagazine;
            if (canReload)
            {
                if (isTriggered)
                {
                    Reload();
                }
            }
        }
        public void TriggerReload()
        {
            bool canReload = IsCanReload && !Attacking && !FullMagazine && !InventoryEmpty;
            if (canReload)
            {
                Reload();
            }
        }
        private void TriggerAttack(bool isActivated)
        {
            if (isActivated)
            {
                if (IsCanReloadMagazine)
                {
                    Reload();
                }
                else
                {
                    Attack();
                }
            }
        }

        public virtual void Attack()
        {
            //GetDirection
            nextAttackTime = data.GetNextAttack(currentFireMode);
            CurrentRounds -= data.Consume;
            CurrentHeatLevel += data.Consume;
        }
        public virtual void Burst()
        {
            nextAttackTime = data.GetNextAttack(currentFireMode);
        }

        public virtual void Reload()
        {
            switch (Origin.ReloadMode)
            {
                case ReloadMode.None:
                    break;
                case ReloadMode.ChangeMagazine:
                    ReloadMagazine();
                    break;
                case ReloadMode.BulletByBullet:
                    ReloadBulletByBullet();
                    break;
                default:
                    ReloadMagazine();
                    break;
            }
        }
        protected void ReloadMagazine()
        {
            StartCoroutine(IE_ReloadMagazines());
            StartCoroutine(IE_DropMagazine());
        }
        private IEnumerator IE_ReloadMagazines()
        {
            IsReloading = true;

            Model.Animator.Reload(!EmptyMagazine);

            yield return EmptyMagazine ? Model.CompleteReloadDuration : Model.ReloadDuration;

            if (IsActive && IsReloading)
            {
                if (!EmptyMagazine)
                {
                    float amount = data.Capacity - CurrentRounds;
                    CurrentRounds += RequestAmmo(data.Ammo, amount);
                }
                else
                {
                    CurrentRounds += RequestAmmo(data.Ammo, data.Capacity);
                }
            }

            IsReloading = false;
        }
        protected void ReloadBulletByBullet()
        {
            StartCoroutine(IE_ReloadBulletByBullet());
        }
        private IEnumerator IE_ReloadBulletByBullet()
        {
            IsReloading = true;

            Model.Animator.StartReload(!EmptyMagazine);

            if (EmptyMagazine)
            {
                yield return Model.InsertInChamberDuration;
                CurrentRounds += RequestAmmo(data.Ammo, data.Consume);
                yield return Model.InsertInChamberDuration;
            }
            else
            {
                yield return Model.StartReloadDuration;
            }

            while (IsActive && IsReloading && !FullMagazine && !InventoryEmpty)
            {
                Model.Animator.InsertRound();
                yield return Model.InsertDuration;

                if (IsActive && IsReloading)
                {
                    CurrentRounds += RequestAmmo(data.Ammo, data.Consume);
                }
                yield return Model.InsertDuration;
            }

            if (IsActive && IsReloading)
            {
                StopReload();
            }
        }
        protected void StopReload()
        {
            StartCoroutine(IE_StopReload());
        }
        private IEnumerator IE_StopReload()
        {
            Model.Animator.StopReload();
            IsReloading = false;
            nextReloadTime = Model.StopReloadTime + Time.time;
            yield return Model.StopReloadDuration;
        }
        private IEnumerator IE_DropMagazine()
        {
            yield return EmptyMagazine ? Model.CompleteReloadDropDuration : Model.ReloadDropDuration;
            DropMagazine();
        }
        public void DropMagazine()
        {

        }

        protected void EmptyCapacity()
        {
            nextAttackTime = data.GetNextAttack(FireMode.None);
            Model.Animator.OutOfAmmo();
        }
        private bool InventoryIsEmpty(AmmoData ammo)
        {
            return false;
        }
        private float RequestAmmo(AmmoData ammo, float value)
        {
            return value;
        }

        void Heating(float delta)
        {
            if (data.CapacityHeat == 0) return;
            float coolingDelay = 1f;
            float percentHeat = (CurrentHeatLevel / data.CapacityHeat);
            bool isCanOverheat = percentHeat == 1;
            if (isCanOverheat)
            {
                Overheat();
            }
            else if (CurrentHeatLevel > 0 && nextAttackTime + coolingDelay < Time.time && !isCanOverheat)
            {
                Cooling(delta);
                Model.Cooling(percentHeat, delta);
            }
            else
            {
                Model.Heating(percentHeat, delta);
            }
        }
        void Cooling(float delta)
        {
            float speedCooling = data.CapacityHeat / data.OverheatTime;
            float heatLevel = speedCooling * delta;
            CurrentHeatLevel -= heatLevel;
        }
        void Overheat() 
        {
            //Start Overheating
            if (!isOverheating)
            {
                float overheatDelay = Time.time + data.OverheatTime;
                nextAttackTime = overheatDelay;
                nextReloadTime = overheatDelay;
                isOverheating = true;
            }
            //Stop Overheating
            else if (nextAttackTime <= Time.time)
            {
                CurrentHeatLevel = 0;
                isOverheating = false;
            }
        }

        public void Charge() { }

        public void Modify() { }
        public void Switch() { }


        public void Hide() { }
        public void Draw() { }
    }
}
