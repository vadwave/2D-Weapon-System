using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    public class Weapon : MonoBehaviour
    {
        public event Action<float, float, CapacityType> OnChangedCapacity;
        public event Action<float, float> OnChangedHeat;
        public event Action<float, float> OnChangedCharge;
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
        public bool EmptyCapacity => CurrentRounds == 0;
        public bool FullCapacity => CurrentRounds == data.Capacity;
        public bool EmptyMagazine => CurrentRounds <= data.Origin.ChamberCapacity;
        public bool IsCanReloadMagazine => EmptyMagazine && !InventoryEmpty;
        public bool InventoryEmpty => InventoryIsEmpty(data.Ammo);
        public bool IsCanAttack => !IsReloading && !Attacking && CurrentRounds >= 0;
        public bool IsCanReload => !IsReloading && nextReloadTime < Time.time;
        public bool IsCanAutoReloadAfterShot => data.Origin.AutoReload == AutoReload.AfterShot && IsCanReloadMagazine;
        public bool IsCanAutoReloadTrigger => data.Origin.AutoReload == AutoReload.Trigger && IsCanReloadMagazine;
        public bool IsReloading
        {
            get => isReloading;
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
        public bool IsCharged => data.ChargeTime > 0 && data.Origin.ChargeType != ChargeType.None;
        public bool IsHeated => data.CapacityHeat > 0;
        public bool IsChargingBurst => data.BulletsPerBurst > 1;
        public float CurrentRounds { get => currentRounds;
            protected set
            {
                if (currentRounds != value)
                {
                    currentRounds = value;
                    OnChangedCapacity?.Invoke(currentRounds, data.MagazineCapacity, data.Origin.CapacityType);
                }
            }
        }
        public float CurrentHeatLevel { get => currentHeatLevel;
            protected set
            {
                if (IsHeated)
                {
                    currentHeatLevel = Mathf.Clamp(value, 0, data.CapacityHeat);
                    OnChangedHeat?.Invoke(currentHeatLevel, data.CapacityHeat);
                }
            }
        }
        public float CurrentChargeLevel
        {
            get => currentChargeLevel;
            protected set
            {
                currentChargeLevel = Mathf.Clamp(value, 0, data.ChargeCapacity);
                OnChangedCharge?.Invoke(currentChargeLevel, data.ChargeCapacity);
            }
        }
        public int BulletsPerBurst
        {
            get
            {
                switch (data.Origin.ChargeType)
                {
                    case ChargeType.None: return data.BulletsPerBurst;
                    case ChargeType.Standart: return data.BulletsPerBurst;
                    case ChargeType.Full: return (CurrentChargeLevel == data.ChargeCapacity) ? data.BulletsPerBurst : 0;
                    case ChargeType.Capacity: return (Mathf.RoundToInt(CurrentChargeLevel / data.Consume));
                    default: return data.BulletsPerBurst;
                }
            }
        }

        [SerializeField] protected WeaponData weaponData;

        protected LayerMask collidedMask;
        protected WeaponRealData data;
        [SerializeField] protected WeaponModel model;
        protected WeaponState state;

        protected FireMode currentFireMode;

        protected float nextAttackTime;
        protected float nextChargeTime;
        protected float nextReloadTime;

        protected float currentRounds;
        protected float currentHeatLevel;
        protected float currentChargeLevel;

        protected bool isReloading = false;
        protected bool isOverheating = false;
        protected bool isCharging = false;

        protected void FixedUpdate()
        {
            Heating(Time.fixedDeltaTime);
            Charging(Time.fixedDeltaTime);
        }
        protected bool LayerInMask(LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) != 0);
        }
        public virtual void Initialize(WeaponData data)
        {
            this.data = new WeaponRealData(data);
            this.state = WeaponState.Idle;
            this.model = this.GetComponentInChildren<WeaponModel>();
            if(model == null) CreateModel();
            this.model.Init(this);
            this.currentFireMode = this.data.Origin.FireMode;
            this.CurrentRounds = this.data.MagazineCapacity;
        }
        public void Initialize(LayerMask mask)
        {
            collidedMask = mask;
            Initialize(weaponData);
        }
        public void CreateModel()
        {
            this.model = Instantiate(this.data.Origin.Model, this.transform);
        }

        public void Trigger(InputType input)
        {
            if (!IsActive) return;
            bool canAttack = IsCanAttack;
            if (canAttack)
            {
                if (IsCanAutoReloadTrigger)
                {
                    if ((input == InputType.Down && (Origin.FireMode == FireMode.Single || Origin.FireMode == FireMode.SemiAuto || Origin.FireMode == FireMode.Burst)) ||
                        (input == InputType.Hold && Origin.FireMode == FireMode.FullAuto) ||
                        (input == InputType.Up && Origin.ChargeType != ChargeType.None))
                        Reload();
                }
                else if (EmptyCapacity)
                {
                    if ((input == InputType.Down && (Origin.FireMode == FireMode.Single || Origin.FireMode == FireMode.SemiAuto || Origin.FireMode == FireMode.Burst || Origin.FireMode == FireMode.FullAuto)) ||
                        (input == InputType.Up && Origin.ChargeType != ChargeType.None))
                        EmptyAttack();
                }
                else if(Origin.ChargeType == ChargeType.None)
                {
                    switch (Origin.FireMode)
                    {
                        case FireMode.None: break;
                        case FireMode.Single:
                            {
                                if (input == InputType.Down)
                                {
                                    Attack();
                                }
                            }
                            break;
                        case FireMode.SemiAuto:
                            {
                                if (input == InputType.Down)
                                {
                                    Attack();
                                }
                            }
                            break;
                        case FireMode.FullAuto:
                            {
                                if (input == InputType.Hold)
                                {
                                    Attack();
                                }
                            }
                            break;
                        case FireMode.Burst:
                            {
                                if (input == InputType.Down)
                                {
                                    Burst();
                                }
                            }
                            break;
                    }
                }
                else
                {
                    if (input == InputType.Hold)
                    {
                        Charge();
                    }
                    else if (input == InputType.Up)
                    {
                        if (!isCharging) return;
                        Burst();
                    }
                }
            }
            bool canReloadBulletByBullet = nextReloadTime < Time.time && IsReloading && data.Origin.ReloadMode == ReloadMode.BulletByBullet && !EmptyMagazine;
            if (canReloadBulletByBullet)
            {
                if (input == InputType.Down)
                {
                    StopReload();
                }
            }
            /*
            bool canReload = IsCanReload && !Attacking && IsCanReloadMagazine;
            if (canReload)
            {
                if (input == InputType.Down)
                {
                    Reload();
                }
            }
            */
        }
        public virtual void Charge()
        {
            if (isCharging) return;
            isCharging = true;
        }
        public virtual void ReleaseCharge()
        {
            if (!isCharging) return;
            isCharging = false;
        }
        public void TriggerReload()
        {
            bool canReload = IsCanReload && !Attacking && !FullCapacity && !InventoryEmpty && IsActive;
            if (canReload)
            {
                Reload();
            }
        }
        public virtual void Attack()
        {
            //GetDirection
            nextAttackTime = data.GetNextAttack(currentFireMode, IsCharged, BulletsPerBurst);
            CurrentRounds -= data.Consume;
            CurrentHeatLevel += data.Consume;
        }
        public virtual void Burst()
        {
            nextAttackTime = data.GetNextAttack(currentFireMode, IsCharged, BulletsPerBurst);
            if (Origin.ChargeType != ChargeType.None) ReleaseCharge();
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
            if(EmptyMagazine) StartCoroutine(IE_DropMagazine());
        }
        private IEnumerator IE_ReloadMagazines()
        {
            IsReloading = true;

            Model.Animator.Reload(!EmptyMagazine);

            yield return EmptyMagazine ? Model.CompleteReloadDuration : Model.ReloadDuration;

            if (IsActive && IsReloading)
            {
                if (EmptyMagazine)
                {
                    CurrentRounds += RequestAmmo(data.Ammo, data.MagazineCapacity);
                }
                else
                {
                    float amount = data.Capacity - CurrentRounds;
                    CurrentRounds += RequestAmmo(data.Ammo, amount);
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

            Model.Animator.StartReload(!EmptyCapacity);

            if (EmptyCapacity)
            {
                yield return Model.InsertInChamberDuration;
                CurrentRounds += RequestAmmo(data.Ammo, data.Consume);
                yield return Model.InsertInChamberDuration;
            }
            else
            {
                yield return Model.StartReloadDuration;
            }

            while (IsActive && IsReloading && !FullCapacity && !InventoryEmpty)
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
            Model.DropMagazine();
        }

        protected void EmptyAttack()
        {
            nextAttackTime = data.GetNextAttack(FireMode.None, IsCharged, BulletsPerBurst);
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
            if (!IsHeated) return;
            float coolingDelay = 1f;
            float percentHeat = (CurrentHeatLevel / data.CapacityHeat);
            bool isCanOverheat = percentHeat == 1;
            if (isCanOverheat)
            {
                Model.Heating(percentHeat, delta);
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
                Model.Animator.Overheat();
            }
            //Stop Overheating
            else if (nextAttackTime <= Time.time)
            {
                CurrentHeatLevel = 0;
                isOverheating = false;
            }
        }
        void Charging(float delta)
        {
            if (!IsCharged) return;
            if (isCharging && nextAttackTime < Time.time)
            {
                bool isCanRequered = data.BulletsPerBurst > 1 ? CurrentChargeLevel < CurrentRounds : CurrentChargeLevel < data.ChargeCapacity;
                if (nextChargeTime < Time.time && CurrentChargeLevel < data.ChargeCapacity && isCanRequered)
                {
                    float speedCharging = data.ChargeCapacity / data.ChargeTime;
                    float timeChargeOne = data.Consume / speedCharging;
                    CurrentChargeLevel += data.Consume;
                    nextChargeTime = Time.time + timeChargeOne;
                }
            }
        }

        public void Modify() { }
        public void Switch() { }


        public void Hide() 
        {
            IsActive = false;
            Model.Animator.Hide();
            ReleaseCharge();
        }
        public void Draw() 
        {
            Model.Animator.Initialize(data.Origin.FireMode);
            StartCoroutine(IE_DrawWait());
        }
        IEnumerator IE_DrawWait()
        {
            Model.Animator.Draw();
            yield return new WaitForSeconds(Model.Animator.DrawAnimationLength);
            IsActive = true;
        }

        public float CalculateDistanceDamage(float distance)
        {
            return data.CalculateDistanceDamage(distance);
        }
        public void Damage(RaycastHit2D hit, float distance)
        {
            Vector2 hitPosition = hit.point;
            float damage = CalculateDistanceDamage(distance);
            IDamageable damageableTarget = hit.collider.GetComponent<IDamageable>();
            damageableTarget?.Damage(damage, transform.root.position, hitPosition);
        }
        public void Damage(Collider2D collision, float distance, Vector2 hitPosition)
        {
            float damage = CalculateDistanceDamage(distance);
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();
            damageableTarget?.Damage(damage, transform.root.position, hitPosition);
        }
        public bool CheckIsOtherCollision(RaycastHit2D hit)
        {
            //if (hit.transform.root == transform.root) return false;
            if (!LayerInMask(collidedMask, hit.collider.gameObject.layer)) return false;
            return true;
        }
        public void ImpactForce(Vector2 direction, float distance, Rigidbody2D rigidBody)
        {
            if (rigidBody)
            {
                float impactForce = data.CalculateImpactForce(distance);
                Vector2 force = direction * impactForce;
                rigidBody.AddForce(force, ForceMode2D.Impulse);
            }
        }
        internal void Impact(Vector2 position)
        {
            Model.CreateImpact(position);
            Decal(position, 0.3f);
        }
        internal void Decal(Vector2 position)
        {
            Model.CreateDecal(position);
        }
        internal void Decal(Vector2 position, float waitTime)
        {
            if(this.gameObject.activeInHierarchy)
                StartCoroutine(IE_PaintDecal(position, waitTime));
        }
        IEnumerator IE_PaintDecal(Vector2 position, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            Decal(position);
        }
    }
}
