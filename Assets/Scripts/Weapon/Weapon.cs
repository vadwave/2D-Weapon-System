using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace WeaponSystem
{
    public class Weapon : MonoBehaviour
    {
        public event Action<float, float> OnChangedCapacity;
        public event Action OnStartReloaded;
        public WeaponModel Model => model;
        public AmmoData Ammo => Origin.Ammo;
        public WeaponData Origin => data.Origin;
        public WeaponRealData Data => data;
        public bool Attacking => nextAttackTime >= Time.time;
        public bool IsIdling { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsActive { get; private set; }
        public bool EmptyMagazine => CurrentRounds == 0;
        public bool FullMagazine => CurrentRounds == data.Capacity;
        public bool IsCanReloadMagazine => EmptyMagazine && !InventoryEmpty;
        public bool InventoryEmpty => InventoryIsEmpty(data.Ammo);
        public bool IsCanAttack => !EmptyMagazine && !Attacking;
        public bool IsCanReload => !isReloading && nextReloadTime < Time.time;
        public float CurrentRounds { get => currentRounds; protected set { currentRounds = value; OnChangedCapacity?.Invoke(currentRounds, data.Capacity); } }

        [SerializeField] protected WeaponData weaponData;

        protected LayerMask collidedMask;
        protected WeaponRealData data;
        protected WeaponModel model;
        protected WeaponState state;

        protected FireMode currentFireMode;
        protected float nextAttackTime;
        protected float nextReloadTime;
        protected float currentRounds;

        protected bool isReloading = false;


        protected void Awake()
        {
            Initialize(weaponData);
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
            bool canAttack = IsCanReload && !Attacking && CurrentRounds >= 0;
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
            bool canReloadBulletByBullet = nextReloadTime < Time.time && isReloading && data.Origin.ReloadMode == ReloadMode.BulletByBullet && !EmptyMagazine;
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
        public virtual void Attack()
        {
            //GetDirection
            nextAttackTime = data.GetNextAttack(currentFireMode);
            CurrentRounds -= data.Consume;
        }
        

        public virtual void Reload()
        {
            OnStartReloaded?.Invoke();
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

        protected void ReloadBulletByBullet()
        {
            StartCoroutine(IE_ReloadBulletByBullet());
        }
        protected void ReloadMagazine()
        {
            StartCoroutine(IE_ReloadMagazines());
            StartCoroutine(IE_DropMagazine());
        }
        private IEnumerator IE_ReloadBulletByBullet()
        {
            isReloading = true;

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

            while (IsActive && isReloading && !FullMagazine && !InventoryEmpty)
            {
                Model.Animator.InsertRound();
                yield return Model.InsertDuration;

                if (IsActive && isReloading)
                {
                    CurrentRounds += RequestAmmo(data.Ammo, data.Consume);
                }
                yield return Model.InsertDuration;
            }

            if (IsActive && isReloading)
            {
                StopReload();
            }
        }
        protected void StopReload()
        {
            StartCoroutine(IE_StopReload());
        }
        private IEnumerator IE_ReloadMagazines()
        {
            isReloading = true;

            Model.Animator.Reload(!EmptyMagazine);

            yield return EmptyMagazine ? Model.CompleteReloadDuration : Model.ReloadDuration;

            if (IsActive && isReloading)
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

            isReloading = false;
        }


        private IEnumerator IE_StopReload()
        {
            Model.Animator.StopReload();
            isReloading = false;
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


        public void Overheat() { }
        public void Charge() { }
        public virtual void Burst() 
        {
            nextAttackTime = data.GetNextAttack(currentFireMode);
        }
        public void Modify() { }
        public void Switch() { }


        public void Hide() { }
        public void Draw() { }
    }
}
